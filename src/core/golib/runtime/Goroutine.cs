// Goroutine.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;

namespace go.golib;

/// <summary>
/// A goroutine's IDENTITY, and the ROOT where the body of every Go <c>go</c> statement begins and
/// ends.
/// </summary>
/// <remarks>
/// <para>
/// Every <c>builtin.goǃ</c> overload dispatches through <see cref="Start"/>, so the root's policy is
/// stated exactly once instead of being duplicated across the arity overloads. That funnel is also
/// what makes the EXECUTOR a single method body: <see cref="Start"/> gives each goroutine its own
/// dedicated thread, so a parked goroutine can never delay a not-yet-started one.
/// </para>
/// <para>
/// <b>Why not the ThreadPool.</b> Goroutines used to be <c>ThreadPool.QueueUserWorkItem</c> work
/// items. A goroutine that parks — an unbuffered rendezvous, a full buffer, a blocked select, a
/// <c>WaitGroup</c>, a sleep — blocks its thread for as long as it is parked, so on a SHARED pool
/// every parked goroutine subtracted one unit of capacity from the goroutines that had not started
/// yet. The pool answers a capacity shortfall with heuristics (hill-climbing injection at roughly
/// one thread per second, gated on ~1s of continuous starvation, fighting a ~20s idle-retirement
/// timeout), and those heuristics — not parking latency — were the whole cost: a 1000-goroutine
/// rendezvous storm drained in pool-sized waves, and <c>internal/singleflight</c>'s
/// <c>TestDoAndForgetUnsharedRace</c> spent 28.7 MINUTES climbing that ladder for work Go finishes
/// in 0.04s. A thread per goroutine makes capacity equal demand by construction: there is no queue,
/// no floor and no inference to get wrong. See <c>docs/phase4/DESIGN-cooperative-scheduler.md</c>
/// §1 for the measured ladder and §4 for the mechanisms priced against each other.
/// </para>
/// <para>
/// <b>What this is not.</b> Not an M:N scheduler. The CLR offers no stack switching, so a go2cs
/// goroutine is, and remains, a thread for its whole life — every thread-affinity invariant golib
/// relies on (the Goexit gate, <c>recover()</c>'s slots, <c>GoFrame</c>'s <c>ref struct</c> defer
/// frames, <c>procPin</c>'s shard id, the high-resolution sleep timer) holds by construction
/// because the body still runs start-to-finish on one thread. The cost is that OS threads bound
/// this model at roughly 10⁴ concurrent goroutines rather than Go's 10⁶ — a documented divergence,
/// smaller than the one it replaces.
/// </para>
/// </remarks>
public sealed class Goroutine
{
    /// <summary>
    /// Environment variable overriding <see cref="StackReserve"/>.
    /// </summary>
    public const string StackReserveVariable = "GO2CS_GOROUTINE_STACK";

    // Go goroutine stacks GROW (to ~1GB on 64-bit), so deeply recursive code is legal Go, while a
    // .NET thread's stack is a FIXED reservation whose overflow is uncatchable and kills the whole
    // process. A reservation is ADDRESS SPACE, not memory — pages commit on demand — so 256MB buys
    // Go-scale headroom at no real cost: 10⁴ goroutines reserve ~2.5TB of a 64-bit process's 128TB,
    // and it is the live THREAD count, never the reserve, that bounds this model.
    //
    // Deliberately SEPARATE from the converted-test host's per-test thread reserve
    // (testing/TestExecution.cs). That one sizes ONE thread per test and answers "how deep does a
    // TEST BODY recurse"; this one is multiplied by the goroutine count and answers "how deep does a
    // GOROUTINE body recurse". The two are not required to agree and nothing should unify them — a
    // host that needs deeper test stacks must not drag every goroutine's reservation up with it, and
    // the multipliers differ by orders of magnitude. GO2CS_GOROUTINE_STACK overrides this one alone.
    private const int DefaultStackReserve = 256 * 1024 * 1024;

    private static readonly int s_stackReserve = ResolveStackReserve();

    // The live-goroutine REGISTRY. The set answers "which goroutines exist" for the consumers this
    // unblocks (runtime.NumGoroutine, the deadlock detector, Goexit option A — none of which land
    // here); the count is carried separately because ConcurrentDictionary.Count takes every lock and
    // is O(buckets), which is the wrong shape for something a program may poll.
    private static readonly ConcurrentDictionary<long, Goroutine> s_live = new();
    private static long s_nextId;
    private static int s_count;

    // The identity of the goroutine running on this thread — getg(), in Go's terms — or null on a
    // thread that is not running Go code at all (BCL callbacks, the timer service thread, the
    // finalizer). Goroutine state is thread-affine everywhere in golib, so this is the natural root.
    [ThreadStatic]
    private static Goroutine? t_current;

    // The containment policy, when a HOST installed one. Only ever assigned non-null, so a root
    // whose filter observed a policy can never find it gone by the time the handler runs.
    private static Action<Exception>? s_containment;

    // The panic observer, when a HOST installed one. Read once per escaping panic and never cleared.
    private static Action<PanicException>? s_panicObserver;

    private Goroutine(long id, bool isMain)
    {
        Id = id;
        IsMain = isMain;
    }

    // Why this goroutine is parked, or WaitReason.Zero when it is not — Go's gp.waitreason, and the
    // WHOLE of this goroutine's park accounting. Held as an int because Volatile has no enum
    // overload; written only by this goroutine's own thread (inside Park/ParkScope.Dispose) and read
    // by any thread rendering a traceback, which is why both sides go through Volatile: a stale
    // reason would read as a goroutine parked for something it has already finished waiting on.
    //
    // ONE field, not two. The obvious shape — a State enum plus a reason beside it — can represent
    // "parked, no reason" and "running, reason chan receive", neither of which is a thing, and it
    // doubles the writes on a path that is taken every time any Go program blocks. Encoding the
    // state IN the reason (Zero means running) makes the two facts one fact, costs exactly one store
    // to park and one to unpark, and is what Go's own traceback does at the printing site:
    // goroutineheader prints the status word and OVERRIDES it with gp.waitreason.String() whenever
    // the status is _Gwaiting (runtime/traceback.go).
    private int m_waitReason;

    // Monotonic, and deliberately INTERNAL: Go hides goroutine ids from programs precisely so that
    // nothing builds goroutine-local storage on them, and a converted program must inherit that
    // constraint. It exists for the thread name and the debugger.
    internal long Id { get; }

    // The main goroutine is registered like any other — it is what runtime.NumGoroutine counts and
    // what a deadlock detector must see — but it is NOT "a goroutine" for the one contract that
    // distinguishes them: runtime.Goexit means something different there (see OnGoroutine).
    internal bool IsMain { get; }

    // Running until a park-accounting scope says otherwise — DERIVED, never stored, so it cannot
    // disagree with the reason (see m_waitReason). This is the gopark/goready accounting contract of
    // DESIGN-cooperative-scheduler.md §5.3, which wraps the existing wait primitives rather than
    // relocating any of them.
    //
    // There is no _Grunnable here and there cannot be: Go distinguishes a goroutine QUEUED on a P
    // from one executing, and under the CLR a goroutine is a thread the OS schedules — a thread
    // waiting for a core is indistinguishable from one running on it, with no P, no run queue and
    // nothing to ask. Every not-parked goroutine therefore reads Running, which is what a traceback
    // prints as [running].
    internal GoroutineState State =>
        Volatile.Read(ref m_waitReason) == (int)WaitReason.Zero ? GoroutineState.Running : GoroutineState.Parked;

    // Go's gp.waitreason. WaitReason.Zero on a running goroutine, exactly as in Go.
    internal WaitReason Reason => (WaitReason)Volatile.Read(ref m_waitReason);

    /// <summary>
    /// The stack size, in bytes, reserved for each goroutine's thread.
    /// </summary>
    public static int StackReserve => s_stackReserve;

    /// <summary>
    /// The number of goroutines that currently exist, including the main goroutine.
    /// </summary>
    public static int Count => Volatile.Read(ref s_count);

    /// <summary>
    /// Indicates whether the calling thread is running a goroutine body rather than the main
    /// goroutine.
    /// </summary>
    /// <remarks>
    /// <c>runtime.Goexit</c> is the one Go primitive whose contract differs between the two: from a
    /// goroutine it ends that goroutine, while from the MAIN goroutine it ends <c>main</c> without
    /// returning and leaves the program running its other goroutines — a shape with no managed
    /// counterpart today, so the main-goroutine case stays gated (see the runtime package's
    /// <c>managed_impl.cs</c>). A thread with no identity at all is not a goroutine either, so the
    /// gate reads false there too — exactly as it did when this was a bare <c>[ThreadStatic]</c>
    /// flag.
    /// </remarks>
    public static bool OnGoroutine => t_current is { IsMain: false };

    /// <summary>
    /// The goroutine running on the calling thread — <c>getg()</c>, in Go's terms — or <c>null</c>
    /// on a thread that is not running Go code (a BCL callback, an IO completion, the finalizer).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Exposed as an opaque IDENTITY, never as a number. <see cref="Id"/> stays internal on purpose:
    /// Go hides goroutine ids from programs precisely so nothing builds goroutine-local storage on
    /// them, and a converted program inherits that constraint. A reference is a different thing — it
    /// cannot be parsed, forged or persisted past the goroutine's life, and it is what a hand-owned
    /// implementation needs when a pair of OS calls Go keeps independent must be correlated across
    /// one goroutine's own call sequence.
    /// </para>
    /// <para>
    /// The corpus's first consumer is the Windows accept path, where <c>AcceptEx</c> stages a native
    /// buffer that <c>GetAcceptExSockaddrs</c> — a routine carrying neither handle nor overlapped —
    /// then parses (<c>syscall/windows/zsyscall_windows_wsa_impl.cs</c>;
    /// <c>docs/phase4/DESIGN-netpoll-managed-poller.md</c> §4.5). Under the dedicated-thread executor
    /// a goroutine is one thread for life, so a <c>[ThreadStatic]</c> would work today; keying on the
    /// goroutine instead makes that handoff's PREMISE explicit and survives an executor that stops
    /// making it true.
    /// </para>
    /// </remarks>
    public static Goroutine? Current => t_current;

    /// <summary>
    /// Marks the calling thread as running a goroutine body for the lifetime of the returned scope.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="Start"/> itself, and by hosts that run Go code on a thread they created
    /// themselves rather than through it — the converted-test host runs each test on a dedicated
    /// thread, and that thread IS the test's goroutine in Go terms (Go's <c>tRunner</c> runs every
    /// test in one). Entering a thread that already has an identity keeps that identity and returns
    /// an inert scope, so nesting can neither mint a second goroutine for one thread nor retire the
    /// outer one early.
    /// </remarks>
    public static Scope Enter()
    {
        if (t_current is not null)
            return default;

        Goroutine goroutine = Register(isMain: false);
        t_current = goroutine;

        // Named for the debugger and for a thread dump, which is where a leaked or wedged goroutine
        // is diagnosed. A host that named the thread itself keeps its own name.
        Thread.CurrentThread.Name ??= $"goroutine-{goroutine.Id.ToString(CultureInfo.InvariantCulture)}";

        return new Scope(goroutine);
    }

    /// <summary>
    /// Accounts for the calling goroutine being parked on <paramref name="reason"/> until the
    /// returned scope is disposed — Go's <c>gopark(reason)</c>, minus the parts that do not exist
    /// here.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Go's <c>gopark</c> does three jobs: publish "parked, for this reason", switch the g off its M,
    /// and run a commit callback under the publication protocol. Under a thread-affine executor the
    /// middle job does not exist, and the third stays inside each primitive's own — already
    /// adversarially hardened — park/claim discipline. What remains is the first, and it is the fact
    /// every consumer of this contract needs: how many goroutines are parked, and why. So this is
    /// deliberately ACCOUNTING ONLY. It relocates no wait, re-opens no protocol, and owes no
    /// lost-wakeup re-verification; the caller still blocks on exactly the primitive it blocked on
    /// before:
    /// </para>
    /// <code>
    /// using (Goroutine.Park(WaitReason.ChanReceive))
    ///     parked.Park.Wait();
    /// </code>
    /// <para>
    /// There is deliberately no <c>goready</c> side. The waker already signals the primitive, and the
    /// woken thread un-marks ITSELF when its scope disposes — which is also why the write is only ever
    /// made by the goroutine's own thread.
    /// </para>
    /// <para>
    /// <b>Cost:</b> one volatile store to park, one to unpark, and no allocation — the scope is a
    /// <c>readonly struct</c> the <c>using</c> disposes without boxing. Both stores land on a path
    /// that has already decided to block on a kernel wait primitive, so they sit far beneath that
    /// path's own noise floor. Nothing per-<c>ж</c> and nothing per-object changes, so there is no
    /// corpus-wide byte cost.
    /// </para>
    /// <para>
    /// A thread with no goroutine identity — the timer service thread, a BCL callback, the finalizer —
    /// gets an inert scope and writes nothing. Nesting restores the ENCLOSING reason rather than
    /// clearing to <see cref="WaitReason.Zero"/>, so an inner park cannot report an outer one as
    /// having woken.
    /// </para>
    /// </remarks>
    public static ParkScope Park(WaitReason reason)
    {
        if (t_current is not { } goroutine)
            return default;

        int previous = Volatile.Read(ref goroutine.m_waitReason);
        Volatile.Write(ref goroutine.m_waitReason, (int)reason);

        return new ParkScope(goroutine, previous);
    }

    /// <summary>
    /// Every live goroutine, ordered by the sequence in which they were created.
    /// </summary>
    /// <remarks>
    /// The order is Go's: a traceback dumps goroutines by <c>goid</c>, and the registry's ids are
    /// minted monotonically, so sorting by id reproduces creation order. Internal for the same reason
    /// <see cref="Id"/> is — Go hides goroutine identity from programs — and reachable from the
    /// hand-owned <c>runtime</c> package through golib's <c>InternalsVisibleTo</c>.
    /// </remarks>
    internal static Goroutine[] Snapshot()
    {
        Goroutine[] live = [.. s_live.Values];

        Array.Sort(live, static (left, right) => left.Id.CompareTo(right.Id));

        return live;
    }

    /// <summary>
    /// Starts <paramref name="body"/> as a goroutine.
    /// </summary>
    /// <remarks>
    /// One dedicated background thread per goroutine — see the type remarks for why this is not a
    /// pool work item. <c>IsBackground</c> is Go's exit semantics: when <c>main</c> returns the
    /// process exits regardless of how many goroutines are still live or parked, which a foreground
    /// thread would prevent. The <see cref="ExecutionContext"/> flows into the new thread exactly as
    /// it did through <c>QueueUserWorkItem</c> — <c>Thread.Start</c> captures it the same way — which
    /// is what keeps a failure inside a goroutine attributable to the test that spawned it.
    /// </remarks>
    public static void Start(Action body)
    {
        Thread thread = new(() => Run(body), s_stackReserve)
        {
            IsBackground = true
        };

        thread.Start();
    }

    /// <summary>
    /// Installs a HOST policy that contains a non-panic exception escaping a goroutine instead of
    /// letting it terminate the process.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A converted program must NOT call this: Go's behavior for an unhandled failure in a goroutine
    /// IS process death, and that fidelity is the default here (the exception reaches golib's
    /// AppDomain backstop, which reports it and exits 2). The policy exists for a host that runs many
    /// independent Go programs in one process — the converted-test host — where killing the process
    /// discards every result it had not yet written and blanks the tail of the package's run. There,
    /// one goroutine's infrastructure failure belongs to ONE test.
    /// </para>
    /// <para>
    /// A <c>PanicException</c> (or any exception that maps to a Go runtime panic) is NEVER offered to
    /// the policy: a panic crossing a goroutine root keeps its Go-faithful fatal path even under a
    /// host, because that is what Go does and what the differential oracle must observe.
    /// </para>
    /// </remarks>
    public static void ContainUnhandledExceptions(Action<Exception> policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        Volatile.Write(ref s_containment, policy);
    }

    /// <summary>
    /// Installs a HOST observer that is handed a PANIC escaping a goroutine root, immediately before
    /// the fatal path it is entitled to takes the process down.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This contains nothing and cannot: an unrecovered panic in a goroutine kills a Go program, so
    /// it kills a converted one (see <see cref="ContainUnhandledExceptions"/> for why a host may not
    /// opt out of that). The observer runs from an exception FILTER — first pass, before any unwind —
    /// and the filter always declines, so the panic still reaches golib's AppDomain backstop with its
    /// stack intact and the intervening finally blocks unrun, byte-identical to an unobserved run.
    /// </para>
    /// <para>
    /// It exists because that fatal path is FRAMELESS by design: the backstop prints the panic VALUE
    /// and exits 2, which is Go's own report and exactly what a converted program should print. A
    /// host running many independent Go programs in one process needs more than the value — WHICH
    /// program died, WHERE it faulted, and a last chance to flush the results it has already gathered
    /// but not yet written. Without this, a goroutine panic anywhere in a converted package's test
    /// suite erased every verdict in the run and reported one line with no frame in their place.
    /// </para>
    /// </remarks>
    public static void ObserveUnhandledPanic(Action<PanicException> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        Volatile.Write(ref s_panicObserver, observer);
    }

    /// <summary>
    /// Registers the calling thread as the MAIN goroutine.
    /// </summary>
    /// <remarks>
    /// Called once from golib's module initializer, which runs on the thread that first touches
    /// golib — <c>main</c>, in a converted program. The main goroutine is registered so that the
    /// live count includes it, as Go's does; it is not marked as "on a goroutine", so the
    /// <c>runtime.Goexit</c> gate keeps reading false there.
    /// </remarks>
    internal static void RegisterMainGoroutine()
    {
        if (t_current is not null)
            return;

        t_current = Register(isMain: true);
    }

    private static Goroutine Register(bool isMain)
    {
        Goroutine goroutine = new(Interlocked.Increment(ref s_nextId), isMain);

        s_live[goroutine.Id] = goroutine;
        Interlocked.Increment(ref s_count);

        return goroutine;
    }

    private static void Unregister(Goroutine goroutine)
    {
        if (s_live.TryRemove(goroutine.Id, out _))
            Interlocked.Decrement(ref s_count);
    }

    private static int ResolveStackReserve()
    {
        string? setting = Environment.GetEnvironmentVariable(StackReserveVariable);

        if (string.IsNullOrWhiteSpace(setting))
            return DefaultStackReserve;

        // 0 is meaningful and deliberately allowed: it hands Thread the framework default (~1MB),
        // which is the escape hatch for a host where reserved address space is genuinely scarce.
        if (TryParseByteSize(setting, out long bytes) && bytes >= 0 && bytes <= int.MaxValue)
            return (int)bytes;

        // Loud once, never fatal, and on stderr so it cannot contaminate a converted program's
        // compared stdout: a misspelled override that silently did nothing would be indistinguishable
        // from one that worked.
        Console.Error.WriteLine(
            $"go2cs: ignoring {StackReserveVariable}=\"{setting}\" — expected a byte count with an " +
            $"optional B, KiB, MiB or GiB suffix; using the default {DefaultStackReserve / (1024 * 1024)}MiB reservation.");

        return DefaultStackReserve;
    }

    // Go's own convention for a memory-sized environment variable (GOMEMLIMIT): a decimal byte count
    // with an optional power-of-two unit suffix.
    internal static bool TryParseByteSize(string setting, out long bytes)
    {
        // Deliberately a LOCAL, not a static field: s_stackReserve's initializer calls this, and
        // static field initializers run in TEXTUAL order — a static table declared below it would
        // still be null here, and only for the one input (an override that is actually set) that
        // reaches this far.
        // Longest match first: "KiB" must be tested before "B".
        ReadOnlySpan<(string Suffix, long Factor)> suffixes =
        [
            ("KiB", 1024L),
            ("MiB", 1024L * 1024L),
            ("GiB", 1024L * 1024L * 1024L),
            ("B", 1L)
        ];

        bytes = 0;

        ReadOnlySpan<char> value = setting.AsSpan().Trim();
        long scale = 1;

        foreach ((string suffix, long factor) in suffixes)
        {
            if (!value.EndsWith(suffix, StringComparison.Ordinal))
                continue;

            value = value[..^suffix.Length].TrimEnd();
            scale = factor;
            break;
        }

        if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out long count))
            return false;

        try
        {
            bytes = checked(count * scale);
        }
        catch (OverflowException)
        {
            return false;
        }

        return true;
    }

    // The goroutine ROOT itself, on the CALLING thread. Start queues it; GolibTests calls it directly,
    // because the root's policy can only be observed from a thread whose exception the caller can
    // still catch — the real path ends in an unhandled exception and a dead process by design.
    internal static void Run(Action body)
    {
        using Scope scope = Enter();

        try
        {
            body();
        }
        // Clause order is load-bearing: a GoexitException is not a panic either, so it would satisfy
        // the containment filter below and be reported as a host failure. It is handled here first.
        catch (GoexitException)
        {
            // runtime.Goexit: THIS goroutine ends here. Its deferred calls have already run on the
            // way out (GoFunc.HandleFinally), recover() never saw the unwind (GoexitException is not
            // a PanicException), and no other goroutine is affected — Go's three Goexit properties,
            // all of them falling out of machinery that already existed.
        }
        catch (Exception ex) when (CanContain(ex))
        {
            // A host installed a containment policy and this is not a panic — hand it over instead
            // of letting it reach the process-killing backstop. The filter (rather than a catch and
            // rethrow) is deliberate: when nothing contains the exception, NO handler matches and the
            // runtime's unhandled path is reached with the stack intact and the intervening finally
            // blocks unrun — byte-identical to the behavior before any of this existed. A
            // PanicException never reaches a policy at all: it stays unhandled and hits golib's
            // AppDomain backstop, which reports it Go-style and exits 2, exactly as Go crashes on an
            // unrecovered panic in any goroutine.
            Volatile.Read(ref s_containment)!(ex);
        }
        catch (Exception ex) when (Observed(ex))
        {
            // Unreachable by construction: Observed always returns false, so this root catches a
            // panic no more than it did before an observer could exist. The clause is here only to
            // give the filter — which runs while the panicking stack is still standing — a place to
            // be attached.
        }
    }

    private static bool CanContain(Exception ex) =>
        Volatile.Read(ref s_containment) is not null && !RuntimeErrorPanic.TryAsPanic(ex, out _);

    // Hands an escaping panic to the host observer and ALWAYS declines it. Reached only after
    // CanContain has said no, which for a panic is unconditional.
    private static bool Observed(Exception ex)
    {
        if (Volatile.Read(ref s_panicObserver) is not { } observer)
            return false;

        // Adoption also snapshots the panic's ORIGIN (RuntimeErrorPanic's documented side effect at
        // every adoption point), so a panic that passed through no defer frame at all — nothing to
        // catch it between the fault and here — still reports where it faulted rather than naming
        // this root.
        if (!RuntimeErrorPanic.TryAsPanic(ex, out PanicException? panic))
            return false;

        try
        {
            observer(panic);
        }
        catch
        {
            // An observer is a diagnostic, never a participant: whatever it does, or fails to do,
            // the panic reaches the fatal path unchanged.
        }

        return false;
    }

    /// <summary>
    /// Restores the wait reason a matching <see cref="Park"/> replaced.
    /// </summary>
    public readonly struct ParkScope : IDisposable
    {
        private readonly Goroutine? m_goroutine;
        private readonly int m_previous;

        internal ParkScope(Goroutine goroutine, int previous)
        {
            m_goroutine = goroutine;
            m_previous = previous;
        }

        public void Dispose()
        {
            // Inert when Park found no identity: a thread that is not a goroutine parks nothing.
            if (m_goroutine is null)
                return;

            Volatile.Write(ref m_goroutine.m_waitReason, m_previous);
        }
    }

    /// <summary>
    /// Retires the goroutine identity a matching <see cref="Enter"/> minted.
    /// </summary>
    public readonly struct Scope : IDisposable
    {
        private readonly Goroutine? m_goroutine;

        internal Scope(Goroutine goroutine)
        {
            m_goroutine = goroutine;
        }

        public void Dispose()
        {
            // Inert when Enter() found an identity already in place: that scope minted nothing, so
            // it retires nothing. A thread that finished its goroutine must stop looking like one.
            if (m_goroutine is null)
                return;

            t_current = null;
            Unregister(m_goroutine);
        }
    }
}

/// <summary>
/// What a goroutine is doing, as the runtime accounts for it.
/// </summary>
internal enum GoroutineState
{
    /// <summary>
    /// Runnable or running — the CLR schedules the goroutine's thread.
    /// </summary>
    Running,

    /// <summary>
    /// Blocked on a Go-semantic wait (a channel operation, a semaphore, a sleep).
    /// </summary>
    Parked
}
