// MainExit.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace go.golib;

/// <summary>
/// Go's <c>runtime.main</c> tail: when <c>main</c> returns while another goroutine is panicking, the
/// process does not exit 0 before that goroutine's panic report.
/// </summary>
/// <remarks>
/// <para>
/// Go (runtime/proc.go, "Make racy client program work", issues 3934 and 20018) counts the goroutines
/// running a panic's deferred calls (<c>runningPanicDefers</c>: gopanic adds one, a recovery or the fatal
/// report takes it away) and marks the process <c>panicking</c> once a report begins. After
/// <c>main.main</c> returns, <c>runtime.main</c> yields while the count is non-zero, then parks for good
/// if a report is running, so the dying goroutine prints and exits 2. The case that needs it is ordinary:
/// <c>defer wg.Done(); panic(v)</c> wakes <c>wg.Wait()</c> from INSIDE the panicking goroutine's deferred
/// calls, and <c>main</c> returns at once.
/// </para>
/// <para>
/// A converted <c>main</c> is the CLR entry point and a goroutine is a background thread, so when
/// <c>Main</c> returned the runtime shut down and the panicking goroutine never reached golib's
/// unhandled-exception backstop: exit 0 and an empty stderr (P1, 5 of 5 runs). The tail runs here, from
/// <c>AppDomain.ProcessExit</c>, which the CLR raises after <c>Main</c> returns, on JIT and Native AOT
/// alike.
/// </para>
/// <para>
/// Departures from Go's shape, forced by .NET's shutdown and by goroutines being OS threads:
/// <list type="bullet">
/// <item>The handler cannot park forever as Go's main does. Shutdown is serialized, so the backstop's own
/// <c>Environment.Exit(2)</c> would wait behind it (a deadlock, measured on net10.0). It waits for the
/// report instead, sets <see cref="Environment.ExitCode"/>, and returns. Setting it there decides the
/// status for a <c>void</c> or <c>int</c> <c>Main</c>, on JIT and Native AOT (measured on net10.0).</item>
/// <item>The yield loop cannot hand the panicking goroutine the CPU, as Go's <c>Gosched</c> does under one P: a
/// goroutine is a thread the OS schedules beside <c>main</c>'s. So it asks Go's question instead of taking Go's
/// turns: CAN the goroutine run? A goroutine parked and not readied, or inside a system call, cannot
/// (<see cref="Goroutine.MainReturnSample"/>), and once the same block has lasted <see cref="BlockedConfirm"/>
/// it releases <c>main</c>, as Go's loop runs out at once for a goroutine with nothing to run. A goroutine that
/// can run holds <c>main</c> until it has used <see cref="PanicDefersCpuBudget"/> of CPU since the wait began
/// (<see cref="ThreadCpuTime"/>): the role of Go's 1000 turns, which bounds one running user code in a deferred
/// call. Its rethrow and the CLR's first pass to its root take 1.2 to 1.8 ms from <c>GoFrame.Capture</c>
/// (Release, 30 runs on G-LAPTOP, 2026-10-24). A goroutine waiting for a CPU behind other threads uses none, so
/// it is waited for: the case a wall-clock bound lost (the i9, 2026-10-31: 1 to 2 runs in 20 exited 0 under
/// pinned load, with an empty stderr).</item>
/// <item>One visible divergence, in a pathological case only: a goroutine that can run but is given NO CPU for
/// <see cref="HangGuard"/> (10 s) stops being waited for. Go's single P cannot starve it while <c>main</c> runs,
/// so Go would wait on. Reaching it takes threads at a higher priority pinned over every core for longer than the
/// OS's own starvation boost (about 4 s on Windows), or a goroutine blocked somewhere golib does not mark (a C#
/// wait of the program's own, outside the converted population; docs/phase4/data/main-return-wait). Where the
/// CPU cannot be read (no seam on this OS, or the thread exited), the bound is the wall-clock
/// <see cref="PanicDefersWait"/> it was before.</item>
/// </list>
/// </para>
/// <para>
/// Go's <c>paniclk</c> decides which report a crash prints: the first <c>startpanic_m</c> owns it, and any other
/// thread's fatal path waits behind it. Here the first <see cref="BeginFatal"/> claims the report, and the backstop
/// installed by <see cref="InstallBackstop"/> makes a later thread wait for the owner's report, whose exit then ends
/// the process. Without it, <c>main</c>'s own panic racing a goroutine's took the backstop's lock first and printed
/// <c>panic: m</c> every time, where Go prints the goroutine's (C1's review of this seat, 3 of 3 runs each way).
/// </para>
/// <para>
/// ProcessExit is ALSO raised by every <c>Environment.Exit</c>. An exit golib takes on purpose (os.Exit,
/// a fatal error, the panic backstop) owns the status and goes through <see cref="Exit"/>, which the
/// handler leaves alone, as Go's <c>exit</c> never consults the count.
/// </para>
/// <para>
/// The count is kept per GOROUTINE (<see cref="GoThreadState.RunningPanicDefers"/>), not per panic: one
/// panic is adopted by every deferring frame it crosses, and a hand-owned C# <c>catch</c> can stop a
/// panic without a recover(). The goroutine's end clears it, so nothing is counted for longer than the
/// goroutine that set it lives. The main goroutine is never counted: its own panic cannot race its own
/// return.
/// </para>
/// </remarks>
public static class MainExit
{
    /// <summary>
    /// The longest <c>main</c>'s return waits, by the wall clock, for a goroutine running a panic's deferred calls
    /// whose CPU time cannot be read (the fallback; see the remarks).
    /// </summary>
    internal static readonly TimeSpan PanicDefersWait = TimeSpan.FromMilliseconds(20);

    /// <summary>
    /// The CPU a goroutine that can run may use, since the wait began, before <c>main</c> stops waiting for it.
    /// </summary>
    internal static readonly TimeSpan PanicDefersCpuBudget = TimeSpan.FromMilliseconds(20);

    /// <summary>How long one block (a park, a system call) must last before it releases <c>main</c>.</summary>
    internal static readonly TimeSpan BlockedConfirm = TimeSpan.FromMilliseconds(1);

    /// <summary>
    /// The longest <c>main</c>'s return waits for a goroutine that can run but uses no CPU; see the remarks. Settable
    /// for GolibTests only, which scale it down to show what reaches it.
    /// </summary>
    internal static TimeSpan HangGuard { get; set; } = TimeSpan.FromSeconds(10);

    // The goroutines counted in runningPanicDefers, with the token of the thread each runs on (ThreadCpuTime), so a
    // returning main can ask each whether it can run. Keyed by its thread state: one entry per counted goroutine.
    private static readonly ConcurrentDictionary<GoThreadState, (Goroutine Goroutine, long Token)> s_counted = new();

    // How long the return waits, once a report has begun, for it to be written. The report always ends in an exit,
    // so this bounds only a report that itself hangs.
    private static readonly TimeSpan ReportWait = TimeSpan.FromSeconds(60);

    // Go's runningPanicDefers: goroutines (never main) running an unrecovered panic's deferred calls.
    private static int s_runningPanicDefers;

    // Go's panicking: a fatal report has begun. Its status, and the signal that it has been written.
    private static int s_panicking;
    private static int s_fatalCode;
    private static readonly ManualResetEventSlim s_reported = new(false);

    // Go's paniclk: the managed thread id of the first fatal start, which owns the report (0: none yet).
    private static int s_reportOwner;

    // An exit golib took on purpose: it owns the status, and the main-return tail does not run.
    private static int s_exitRequested;

    /// <summary>The number of goroutines counted as running a panic's deferred calls (GolibTests reads it).</summary>
    internal static int RunningPanicDefers => Volatile.Read(ref s_runningPanicDefers);

    /// <summary>Go's <c>panicking</c>: a fatal report has begun somewhere, and its owner will end the process.</summary>
    internal static bool Panicking => Volatile.Read(ref s_panicking) != 0;

    private static int s_installed;

    /// <summary>Installs the main-return tail, once: golib's module initializer registers the main goroutine.</summary>
    internal static void Install()
    {
        if (Interlocked.Exchange(ref s_installed, 1) == 0)
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    /// <summary>
    /// Subscribes golib's panic <paramref name="backstop"/> behind Go's <c>paniclk</c>: a thread whose crash did not
    /// claim the report first waits for the owner's report, OUTSIDE the backstop's own lock, so the owner can take
    /// that lock, print, and exit.
    /// </summary>
    internal static void InstallBackstop(UnhandledExceptionEventHandler backstop) =>
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            // Main's runtime.Goexit, past its outermost deferring frame: not a crash, so it claims no report. Claiming
            // one here would make a goroutine that panics later wait behind a report main never writes.
            if (MainGoexit.Is(e.ExceptionObject as Exception))
                MainGoexit.Park();

            AwaitReportOwner();
            backstop(sender, e);
        };

    // The first fatal start on another thread owns the report: wait for it, and never print a second one. paniclk is
    // never released, so once the owner HAS reported, its exit is already ending the process and this thread parks
    // for good, as Go's fatal paths park on the lock. Waking to print would put a second report beside a fatal
    // error's (FatalReport prints without the backstop's lock), or beside one written before this thread arrived
    // (C1's F1). Only an owner that never reported (ReportWait expired) lets this thread print its own report.
    // This thread claims the report when nothing has, and goes on to print when it owns it.
    internal static void AwaitReportOwner()
    {
        int self = Environment.CurrentManagedThreadId;
        int owner = Interlocked.CompareExchange(ref s_reportOwner, self, 0);

        if (owner != 0 && owner != self && s_reported.Wait(ReportWait))
            Thread.Sleep(Timeout.Infinite);
    }

    /// <summary>
    /// gopanic's <c>runningPanicDefers.Add(1)</c>: a panic starts running this goroutine's deferred calls.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void PanicDefersStarted(GoThreadState state)
    {
        if (!state.RunningPanicDefers)
            Count(state);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Count(GoThreadState state)
    {
        if (Goroutine.Current is not { IsMain: false } goroutine)
            return;

        if (!state.CpuTimeTokenTaken)
        {
            state.CpuTimeToken = ThreadCpuTime.CurrentThreadToken();
            state.CpuTimeTokenTaken = true;
        }

        state.RunningPanicDefers = true;
        goroutine.SetPanicDefersCounted(true);

        // Registered before the count moves, so a main that reads the count finds the goroutine.
        s_counted[state] = (goroutine, state.CpuTimeToken);
        Interlocked.Increment(ref s_runningPanicDefers);
    }

    /// <summary>
    /// The recovery's <c>runningPanicDefers.Add(-1)</c>: no panic is in flight on this goroutine any more,
    /// because one was recovered, the goroutine ended, or its fatal report began.
    /// </summary>
    internal static void PanicDefersEnded(GoThreadState state)
    {
        if (!state.RunningPanicDefers)
            return;

        state.RunningPanicDefers = false;

        if (s_counted.TryRemove(state, out (Goroutine Goroutine, long Token) counted))
            counted.Goroutine.SetPanicDefersCounted(false);

        Interlocked.Decrement(ref s_runningPanicDefers);
    }

    /// <summary>
    /// The guard seam: forgets a fatal start that did not end the process. In production every fatal start ends it,
    /// because the goroutine root runs at its thread's base (the pool, a coroutine). A test that drives the root on its
    /// own thread catches the escape, and must not leave <c>panicking</c> set for the rest of the run, or for the test
    /// host's own exit, which would then wait for a report that never comes.
    /// </summary>
    internal static void ResetFatalStartForTest()
    {
        Volatile.Write(ref s_reportOwner, 0);
        Volatile.Write(ref s_fatalCode, 0);
        Volatile.Write(ref s_panicking, 0);
        s_reported.Reset();
    }

    /// <summary>
    /// <c>startpanic_m</c>: a fatal report begins on this thread and the process will exit with
    /// <paramref name="code"/>. <c>panicking</c> is set BEFORE this goroutine leaves the count, so a returning
    /// <c>main</c> never sees neither. A goroutine's escaping panic marks it in the goroutine root's first-pass
    /// filter; a fatal error at <see cref="FatalReport"/>'s start.
    /// </summary>
    internal static void BeginFatal(int code)
    {
        // paniclk: the first fatal start owns the report.
        Interlocked.CompareExchange(ref s_reportOwner, Environment.CurrentManagedThreadId, 0);
        Volatile.Write(ref s_fatalCode, code);
        Interlocked.Exchange(ref s_panicking, 1);
        PanicDefersEnded(GoThreadState.Current);
    }

    /// <summary>The fatal report has been written: a <c>main</c> that returned meanwhile may now exit with its status.</summary>
    internal static void FatalReported() => s_reported.Set();

    /// <summary>
    /// The panic backstop's exit: the report is written, so mark it so (and mark the fatal start, for a report that
    /// did not begin on a goroutine root: main's own panic, or one escaping a thread golib did not start), then exit.
    /// </summary>
    [DoesNotReturn]
    internal static void FatalExit(int code)
    {
        BeginFatal(code);
        FatalReported();
        Exit(code);
    }

    /// <summary>
    /// Exits the process with <paramref name="code"/> on purpose: os.Exit, a fatal error, the panic backstop.
    /// The exit owns the status, so the main-return tail does not wait on any goroutine.
    /// </summary>
    [DoesNotReturn]
    public static void Exit(int code)
    {
        Volatile.Write(ref s_exitRequested, 1);
        Environment.Exit(code);

        // Environment.Exit does not return, but the compiler does not know that.
        throw new InvalidOperationException("unreachable: Environment.Exit returned");
    }

    /// <summary>
    /// runtime.main's yield loop: waits while a goroutine runs a panic's deferred calls and no report has begun,
    /// stopping the moment either changes, or once no counted goroutine can still run (<see cref="Holds"/>).
    /// </summary>
    /// <remarks>
    /// Spins and yields rather than sleeping: <c>Thread.Sleep(1)</c> lasts a whole timer tick (15.6 ms) on Windows,
    /// which is far longer than a block's confirmation.
    /// </remarks>
    internal static void AwaitPanickingGoroutines()
    {
        if (Volatile.Read(ref s_runningPanicDefers) == 0)
            return;

        long start = System.Diagnostics.Stopwatch.GetTimestamp();
        Dictionary<GoThreadState, (ThreadCpuReader? Reader, PanickerWatch Watch)> watches = new();
        SpinWait spin = default;

        try
        {
            while (Volatile.Read(ref s_runningPanicDefers) != 0 && Volatile.Read(ref s_panicking) == 0)
            {
                TimeSpan elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(start);
                bool holds = false;

                foreach ((GoThreadState state, (Goroutine goroutine, long token)) in s_counted)
                {
                    // A goroutine first seen now: its reader opens once and lives for the wait (one handle on windows).
                    if (!watches.TryGetValue(state, out (ThreadCpuReader? Reader, PanickerWatch Watch) entry))
                        entry = (ThreadCpuTime.Open(token), default);

                    (bool blocked, int sequence) = goroutine.MainReturnSample();
                    TimeSpan? cpu = entry.Reader is { } reader && reader.TryRead(out TimeSpan read) ? read : null;

                    holds |= Holds(ref entry.Watch, new PanickerSample(blocked, sequence, cpu), elapsed);
                    watches[state] = entry;
                }

                if (!holds || elapsed >= HangGuard)
                    return;

                spin.SpinOnce(sleep1Threshold: -1);
            }
        }
        finally
        {
            foreach ((ThreadCpuReader? reader, _) in watches.Values)
                reader?.Dispose();
        }
    }

    /// <summary>
    /// One look at a counted goroutine: blocked (parked and not readied, or blocked outside a park), which block,
    /// and its CPU time.
    /// </summary>
    internal readonly record struct PanickerSample(bool Blocked, int BlockSequence, TimeSpan? Cpu);

    /// <summary>What a wait remembers of one counted goroutine between its samples.</summary>
    internal struct PanickerWatch
    {
        internal TimeSpan? CpuStart;
        internal bool Blocked;
        internal int BlockSequence;
        internal TimeSpan BlockedSince;
    }

    /// <summary>
    /// Whether one counted goroutine still holds a returning <c>main</c>, <paramref name="elapsed"/> into the wait: Go's
    /// "can it run?" (see the remarks). A block that lasts <see cref="BlockedConfirm"/> as the same block releases it.
    /// Otherwise it holds while its CPU since the wait began is under <see cref="PanicDefersCpuBudget"/>, or, when the
    /// CPU cannot be read, while the wait is under <see cref="PanicDefersWait"/>.
    /// </summary>
    internal static bool Holds(ref PanickerWatch watch, PanickerSample sample, TimeSpan elapsed)
    {
        if (!sample.Blocked)
        {
            watch.Blocked = false;
        }
        else if (!watch.Blocked || watch.BlockSequence != sample.BlockSequence)
        {
            watch.Blocked = true;
            watch.BlockSequence = sample.BlockSequence;
            watch.BlockedSince = elapsed;
        }
        else if (elapsed - watch.BlockedSince >= BlockedConfirm)
        {
            return false;
        }

        if (sample.Cpu is not { } cpu)
            return elapsed < PanicDefersWait;

        watch.CpuStart ??= cpu;

        return cpu - watch.CpuStart.Value < PanicDefersCpuBudget;
    }

    // runtime.main after main.main returns. ProcessExit runs here for main's return and for every exit; only the
    // return waits.
    private static void OnProcessExit(object? sender, EventArgs e)
    {
        if (Volatile.Read(ref s_exitRequested) != 0)
            return;

        AwaitPanickingGoroutines();

        if (Volatile.Read(ref s_panicking) == 0)
            return;

        s_reported.Wait(ReportWait);
        Environment.ExitCode = Volatile.Read(ref s_fatalCode);
    }
}
