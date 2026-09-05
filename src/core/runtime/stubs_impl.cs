// stubs_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Bodies for the runtime's assembly primitives that DO have an exact managed form.
//
// runtime/stubs.go declares a large family of functions implemented in `.s`; go2cs emits each as a
// bodyless `partial` and the PartialStubGenerator fills it with a throwing stub. That default is
// right for genuine raw metal (duffcopy, the retpoline thunks, the write barriers) — but a few of
// them are not raw metal at all once the g/m/p model is gone, and stubbing THOSE turns working
// code into a crash. Only those few are implemented here; every other assembly stub deliberately
// keeps throwing, so an unported path fails loudly instead of silently doing nothing.
//
// Hand-owned: there is no stubs_impl.go, so a -stdlib reconvert never regenerates this file.
//
// getg() — the current goroutine's `g` AND the `m` that is its thread (Q40's design, Q47's cut;
// docs/phase4/DESIGN-managed-getg.md §6–§8, 2026-09-04/05). Until Q47 this file recorded the
// decision to leave getg throwing "while no reachable path needs it". The Linux runtime row's bill
// (2026-09-04) measured that decision: 47 of the 378 rows behind position 57 die on this one stub —
// the allocator suite through mheap locking, GCTest, GoroutineProfile, LFStack, ReadMemStats and
// ReadMetrics, UserArena, SignalM, StringW, TraceMap, the traceback pair, the DebugCall family, the
// semaphore and RWMutex rows — and the design's reader census showed why a `g` ALONE would have made
// it worse, not better: 202 of the 280 production readers read `gp.m` FIRST, so a `g` with a nil `m`
// throws an anonymous NullReferenceException one frame later, the same loud death minus its name.
// The honest shape is the pair, because golib's executor gives every goroutine its own dedicated
// thread for its whole life: a thread-static IS goroutine identity here (golib's own `t_current`
// rests on the same fact), and an `m` that names that thread with `curg` the goroutine it runs is a
// TRUE statement about the managed scheduler — one M per goroutine, no Ps — not a modelling choice.
//
// What is populated, and from where (nothing else):
//   g.goid, g.parentGoid      the goroutine registry (Goroutine.Current.Id / .ParentId), at mint;
//                             a thread with no goroutine (a host thread that never ran Go code)
//                             mints goid 0 — the id runtime.Stack already prints for such a thread.
//   g.gopc, g.startpc         GoSyntheticPC.Of(Creator) / .Of(Entry) — Q27's synthetic PC space.
//   g.atomicstatus            _Grunning — true of the caller by construction.
//   g.waitreason              waitReasonZero.
//   g.labels                  golib's profile-label mirror (Goroutine.GetProfileLabels), refreshed on
//                             EVERY call — it is the one H field programs mutate; the mirror stays the
//                             source of truth (runtime/pprof/proflabel_impl.cs), the g reads from it.
//   g.m / m.curg              each other, at mint.
//   everything else           its zero value, in three classes the design names: stack bounds and
//                             scheduling context (`stack`, `stackguard*`, `sched`, `syscall*`,
//                             `stktopsp` — the REPLACED representation, honestly absent); P, g0 and
//                             gsignal linkage (`m.p`, `m.g0`, `m.gsignal` — absent by construction:
//                             there are no Ps and no system stack); counters and bookkeeping
//                             (`locks`, `printlock`, `mallocing`, `throwing`, `dying`, `preemptoff`,
//                             `lockedExt/Int`, `libcall*`, `profilehz` — honest by persistence: the
//                             converted code that increments them is the code that reads them).
//
// What this buys and what it does not, as measured (the increment's own row re-read is the record):
//   - every reader that needed only the pair proceeds — acquirem/releasem, the m.locks and
//     mallocing bookkeeping, semacquire's `gp == gp.m.curg` assertion, LockOSCounts;
//   - a reader that dereferences the P (`gp.m.p.ptr().…`, 37 sites on the reachable set) dies one
//     frame later on the nil linkage — the design's stated falsifier class, the scheduler's replaced
//     representation beginning at the P, not at the g;
//   - the g0 assertions fire as Go's OWN throws: inside systemstack(fn) the managed fn runs on the
//     caller's goroutine, so `gp == gp.m.g0` is false and the reader `throw`s its own message.
//   No field is fabricated to make a row pass; a row that passes on this floor is a finding about
//   the floor, stated as such.
//
// Cost: one g box and one m box per goroutine that ever calls getg (lazily; sizes measured by the
// guard, RuntimeGetgTests), one thread-static read plus one AsyncLocal read per call. Zero for the
// banked roster by construction: a reached getg was a foreign exception no recover() adopts, so a
// banked row that reached it would have been red, and the roster is green.
//
// NOT here: any other goroutine's g (allgs, forEachG, sudog.g, m.curg for a goroutine that is not
// the caller stay unpopulated — tracebackothers and the profile's per-goroutine walk remain the
// registry's); no P, g0 or gsignal; no stack bounds; no scheduler state. See the design's §11.

using System.Runtime.CompilerServices;
using System.Threading;
using go.golib;
using go.@internal.runtime;   // atomic.Uint32's Store is an extension method of atomic_package over ж<Uint32>
using @unsafe = go.unsafe_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // systemstack runs fn on the system stack. Go's own contract already says that when the caller
    // is ALREADY on a system stack (g0 or gsignal), systemstack "calls fn directly and returns" —
    // and in the managed model there is exactly one stack per goroutine and no g0 to switch to, so
    // that branch is the only branch. This is a faithful implementation, not an approximation.
    internal static partial void systemstack(Action fn) => fn();

    // procyield spins for the given number of iterations, emitting the architecture's pause hint.
    // Thread.SpinWait is the CLR's spelling of exactly that.
    internal static partial void procyield(uint32 cycles) => Thread.SpinWait((int)cycles);

    // ---- getg: the calling goroutine's g and its m, minted once per thread ----

    // A goroutine is a dedicated thread for its whole life (golib's executor), so the thread IS the
    // goroutine and a thread-static is the exact cache: the same fact golib's own Goroutine.Current
    // (`t_current`) rests on.
    [ThreadStatic]
    private static ж<g>? t_getg;

    internal static partial ж<g> getg()
    {
        ж<g>? gp = t_getg;

        if (gp is null)
        {
            gp = mintGoroutineDescriptor();
            t_getg = gp;
        }

        // The one H field programs mutate: refreshed from the mirror on every call, never cached,
        // so a runtime_setProfLabel between two reads is visible to the second (Go: `getg().labels`).
        gp.Value.labels = Goroutine.GetProfileLabels() as @unsafe.Pointer;

        return gp;
    }

    private static ж<g> mintGoroutineDescriptor()
    {
        Goroutine? current = Goroutine.Current;

        ж<g> gp = Ꮡ(new g());
        ж<m> mp = Ꮡ(new m());

        ref g gv = ref gp.Value;
        ref m mv = ref mp.Value;

        gv.goid = current is null ? 0UL : unchecked((ulong)current.Id);
        gv.parentGoid = current is null ? 0UL : unchecked((ulong)current.ParentId);
        gv.gopc = current?.Creator is { } creator ? GoSyntheticPC.Of(creator) : (nuint)0;
        gv.startpc = current?.Entry is { } entry ? GoSyntheticPC.Of(entry) : (nuint)0;
        gp.of(g.Ꮡatomicstatus).Store((uint32)_Grunning);   // the corpus's own spelling: atomic.Uint32's methods take the field-ref box
        gv.waitreason = waitReasonZero;

        gv.m = mp;
        mv.curg = gp;

        return gp;
    }

    // ---- the guard's view (RuntimeGetgTests): runtime keeps its internals, so the arms read through
    //      Go-prefixed public helpers, never a g they cannot name ----

    /// <summary>What <c>getg()</c> answers on the calling thread, read twice so the cache is observable.</summary>
    public readonly record struct GoGetgView(ulong Goid, ulong ParentGoid, nuint Gopc, nuint Startpc, uint Status, bool HasM, bool MCurgIsSelf, object? Labels, bool SecondCallIsSameG);

    public static GoGetgView GoGetgSnapshot()
    {
        ж<g> first = getg();
        ж<g> second = getg();
        ref g gv = ref first.Value;
        bool hasM = gv.m is not null && !gv.m.IsNilPointer;

        return new GoGetgView(
            gv.goid,
            gv.parentGoid,
            gv.gopc.Value,
            gv.startpc.Value,
            readgstatus(first),
            hasM,
            hasM && ReferenceEquals(gv.m.Value.curg, first),
            gv.labels,
            ReferenceEquals(first, second));
    }

    /// <summary>The unmanaged sizes of the two descriptors this thread's <c>getg()</c> mints (the design's provisional figures were 0.5 KB and 1.5–2.5 KB).</summary>
    public static (int GBytes, int MBytes) GoGetgDescriptorSizes()
    {
        return (Unsafe.SizeOf<g>(), Unsafe.SizeOf<m>());
    }

    /// <summary>Whether this thread has minted its descriptor yet — lets a guard measure the FIRST call's allocation on a fresh goroutine.</summary>
    public static bool GoGetgIsMinted()
    {
        return t_getg is not null;
    }

    // NOT implemented here, on purpose:
    //   mcall(fn)     — parks the current goroutine and runs fn on the system stack, never
    //                   returning to the caller. Its only callers are the scheduler's own
    //                   continuations (gosched_m, park_m, goexit0, exitsyscall0), and there is no
    //                   managed answer at THIS layer: the managed runtime has no g to park and no
    //                   run queue to hand it to. The public entry points that reach it are
    //                   reimplemented one level up instead (managed_impl.cs).
    //   getcallerpc / getcallersp / getclosureptr / getfp — read the caller's machine registers;
    //                   the managed equivalent (a StackTrace walk) answers a different question
    //                   and would make Go's PC arithmetic silently wrong.

    // ---- Q61: the park hook — gopark's accounting half over golib's Park scope (2026-09-05) ----
    //
    // WHY. Every converted wait (sync's semaphore seam, the hand-owned Mutex/RWMutex/WaitGroup, chan
    // send/receive/select, time.Sleep) passes through golib's Goroutine.Park(reason), which published
    // the reason for traceback rendering and touched nothing else — so the managed g increment 4 mints
    // stayed _Grunning for its whole life, and TestMutexWaitTimeMetric spun forever in Go's own
    //   for { if runtime.GIsWaitingOnMutex(gp) { break }; runtime.Gosched() }
    // (readgstatus(gp) == _Gwaiting && gp.waitreason.isMutexWait(), export_test.go), while the metric
    // it then asserts, sched.totalMutexWaitTime, is accumulated by casgstatus and read 0. Go's sync
    // reaches semacquire1 -> gopark(..., waitReasonSyncMutexLock) -> casgstatus(gp, _Grunning, _Gwaiting);
    // this hook is that transition and its inverse, installed into golib's slot at module init.
    //
    // WHAT MOVES. Entering: gp.waitreason = the mapped reason; casgstatus(gp, _Grunning, _Gwaiting) —
    // whose tracking arm stamps trackingStamp when casgstatusAlwaysTrack (or every gTrackingPeriod-th
    // transition) and the reason is a mutex wait. Leaving: casgstatus(gp, _Gwaiting, _Grunning) — Go's
    // ready (_Gwaiting -> _Grunnable, where the mutex wait time is added) and execute (_Grunnable ->
    // _Grunning) collapsed into one step, since the managed model has no runnable interval; the
    // accounting arm keys on oldval == _Gwaiting, so the sum is the same. Then waitreason = zero.
    // The g is minted on demand (getg) — mint-before-park is the order the CAS needs, since a fresh g
    // is _Grunning and casgstatus spins until it reads its oldval. That spin is the one hazard: a g
    // whose status is not what the boundary expects would spin forever, so the hook READS the status
    // first and PANICS by name on a mismatch (a bookkeeping bug is loud, never a hang). Only the
    // outermost golib scope reaches here (nesting is filtered in golib), and only goroutine threads
    // (a thread with no golib identity gets an inert scope, as before; getg still answers for it).
    //
    // WHAT DOES NOT. No scheduler, no M/P hand-off, no _Grunnable, no sudog, no semaphore table — the
    // box's status word and its reason are the whole model. GC-side readers of _Gwaiting (the
    // managed runtime has no GC of its own) see nothing new.

    [ModuleInitializer]
    internal static void ᴛInstallParkTransition()
    {
        Goroutine.ParkTransition = parkTransition;
    }

    private static void parkTransition(WaitReason reason, bool entering)
    {
        ж<g> gp = getg();
        uint32 status = readgstatus(gp);

        if (entering)
        {
            if (status != (uint32)_Grunning)
                throw new PanicException($"runtime: park transition entering {WaitReasons.Text(reason)} on goroutine {gp.Value.goid} whose status is {status}, not _Grunning ({(uint32)_Grunning})");

            gp.Value.waitreason = mapWaitReason(reason);
            casgstatus(gp, (uint32)_Grunning, (uint32)_Gwaiting);
        }
        else
        {
            if (status != (uint32)_Gwaiting)
                throw new PanicException($"runtime: park transition leaving {WaitReasons.Text(reason)} on goroutine {gp.Value.goid} whose status is {status}, not _Gwaiting ({(uint32)_Gwaiting})");

            // Go's ready: _Gwaiting -> _Grunnable, where casgstatus adds the mutex wait time
            // ((now - trackingStamp) * gTrackingPeriod) to sched.totalMutexWaitTime and stamps the
            // runnable interval.
            casgstatus(gp, (uint32)_Gwaiting, (uint32)_Grunnable);

            // Go's execute: _Grunnable -> _Grunning, done HERE rather than through casgstatus, because
            // that arm also records sched.timeToRun (the /sched/latencies histogram), whose inline
            // counts array is UNALLOCATED in the managed zero-value `sched` (a package-level struct
            // var's [N]T field the converter never constructs -- index out of range [0] with length
            // 0, measured 2026-09-05). The histogram was unreached before this hook and stays zero;
            // the two fields execute clears are cleared the same way.
            if (!gp.of(g.Ꮡatomicstatus).CompareAndSwap((uint32)_Grunnable, (uint32)_Grunning))
                throw new PanicException($"runtime: park transition leaving {WaitReasons.Text(reason)} on goroutine {gp.Value.goid}: the g moved off _Grunnable under the hook");

            gp.Value.tracking = false;
            gp.Value.runnableTime = 0;
            gp.Value.waitreason = waitReasonZero;
        }
    }

    // golib's WaitReason is NOT numbered as Go's waitReason (golib carries only the reasons its model
    // parks with; Go's table has the GC and trace reasons between them), so the map is by NAME, and the
    // guard derives its correctness from the two STRING tables agreeing (WaitReasons.Text against
    // waitReasonStrings), never from these numbers.
    private static waitReason mapWaitReason(WaitReason reason) => reason switch
    {
        WaitReason.IOWait => waitReasonIOWait,
        WaitReason.ChanReceiveNilChan => waitReasonChanReceiveNilChan,
        WaitReason.ChanSendNilChan => waitReasonChanSendNilChan,
        WaitReason.Select => waitReasonSelect,
        WaitReason.SelectNoCases => waitReasonSelectNoCases,
        WaitReason.ChanReceive => waitReasonChanReceive,
        WaitReason.ChanSend => waitReasonChanSend,
        WaitReason.Semacquire => waitReasonSemacquire,
        WaitReason.Sleep => waitReasonSleep,
        WaitReason.SyncCondWait => waitReasonSyncCondWait,
        WaitReason.SyncMutexLock => waitReasonSyncMutexLock,
        WaitReason.SyncRWMutexRLock => waitReasonSyncRWMutexRLock,
        WaitReason.SyncRWMutexLock => waitReasonSyncRWMutexLock,
        _ => waitReasonZero,
    };

    // ---- the guard's view (RuntimeParkTransitionTests) ----

    /// <summary>Every golib park reason beside the Go string the runtime's own table gives the mapped value.</summary>
    public static (string golibText, string runtimeText, bool isMutexWait)[] GoWaitReasonMapProbe()
    {
        WaitReason[] reasons = WaitReasons.Parked();
        var rows = new (string, string, bool)[reasons.Length];

        for (int i = 0; i < reasons.Length; i++)
        {
            waitReason mapped = mapWaitReason(reasons[i]);
            rows[i] = (WaitReasons.Text(reasons[i]), mapped.String().ToString(), mapped.isMutexWait());
        }

        return rows;
    }

    /// <summary>
    /// A goroutine publishes its own g, parks under <paramref name="reason"/> on a gate this thread
    /// holds, and the caller reads that g's status and reason WHILE it is parked (Go's
    /// GIsWaitingOnMutex shape), then releases it and reads them again; with
    /// casgstatusAlwaysTrack set for the window, the mutex-wait metric's growth is returned too.
    /// </summary>
    public static (uint statusParked, string reasonParked, bool isMutexWaitParked, uint statusAfter, string reasonAfter, long mutexWaitDeltaNs) GoParkTransitionProbe(WaitReason reason, int blockMs)
    {
        using ManualResetEventSlim published = new(false);
        using ManualResetEventSlim gate = new(false);
        using ManualResetEventSlim released = new(false);
        ж<g>? parkedG = null;
        bool previousTrack = casgstatusAlwaysTrack;
        casgstatusAlwaysTrack = true;

        try
        {
            long before = Ꮡsched.of(schedt.ᏑtotalMutexWaitTime).Load();

            Goroutine.Start(() =>
            {
                parkedG = getg();
                published.Set();

                using (Goroutine.Park(reason))
                    gate.Wait();

                released.Set();
            });

            awaitProbeEvent(published, "published");
            ж<g> gp = parkedG!;

            // Wait for the goroutine to actually park (the status is written by its own thread after Set).
            awaitProbeStatus(gp, (uint32)_Gwaiting, "park");

            uint statusParked = readgstatus(gp);
            string reasonParked = gp.Value.waitreason.String().ToString();
            bool isMutexWaitParked = gp.Value.waitreason.isMutexWait();

            Thread.Sleep(blockMs);
            gate.Set();
            awaitProbeEvent(released, "released");

            awaitProbeStatus(gp, (uint32)_Grunning, "release");

            long after = Ꮡsched.of(schedt.ᏑtotalMutexWaitTime).Load();

            return (statusParked, reasonParked, isMutexWaitParked, readgstatus(gp), gp.Value.waitreason.String().ToString(), after - before);
        }
        finally
        {
            casgstatusAlwaysTrack = previousTrack;
        }
    }

    // The probe's waits are BOUNDED, and each names what did not happen. Without the module
    // initializer's installer a parked g's status never moves, so an UNBOUNDED spin here would burn
    // the whole test deadline and report as a timeout -- a hang reads as a mass-empty and says
    // nothing about WHICH transition failed, which is exactly what the negative control needs it to
    // say. Measured before this bound existed: the neutered arm exited 124, killed at 300 s.
    private const int ProbeWaitMs = 10_000;

    private static void awaitProbeStatus(ж<g> gp, uint32 want, string what)
    {
        SpinWait spinner = default;
        long deadline = System.Environment.TickCount64 + ProbeWaitMs;

        while (readgstatus(gp) != want)
        {
            if (System.Environment.TickCount64 > deadline)
            {
                throw new PanicException($"runtime: the probe's goroutine did not reach status {want} on {what} within {ProbeWaitMs} ms (it is {readgstatus(gp)}) -- the park transition is not installed");
            }

            spinner.SpinOnce();
        }
    }

    private static void awaitProbeEvent(ManualResetEventSlim signal, string what)
    {
        if (!signal.Wait(ProbeWaitMs))
        {
            throw new PanicException($"runtime: the probe's {what} did not signal within {ProbeWaitMs} ms");
        }
    }

    /// <summary>
    /// A goroutine nests two park scopes and reads its own status at each boundary: outer enter,
    /// inner enter, inner leave, outer leave — the inner pair must move nothing (Go's parked
    /// goroutine cannot park again) and the outer pair must move _Grunning -> _Gwaiting -> _Grunning.
    /// </summary>
    public static uint[] GoNestedParkProbe()
    {
        uint[] statuses = new uint[5];
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            ж<g> gp = getg();
            statuses[0] = readgstatus(gp);

            using (Goroutine.Park(WaitReason.Select))
            {
                statuses[1] = readgstatus(gp);

                using (Goroutine.Park(WaitReason.ChanReceive))
                    statuses[2] = readgstatus(gp);

                statuses[3] = readgstatus(gp);
            }

            statuses[4] = readgstatus(gp);
            done.Set();
        });

        done.Wait();
        return statuses;
    }

    /// <summary>The status word constants the probes compare against, so the arms need no runtime internals.</summary>
    public static (uint running, uint waiting) GoGStatusWords() => ((uint)_Grunning, (uint)_Gwaiting);
}
