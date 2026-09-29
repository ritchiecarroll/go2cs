// managed_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The runtime package's PROCESS-CONTROL surface, reimplemented on managed primitives.
//
// Everything here is a public runtime API whose Go body drives machinery that does not exist
// under the CLR — stopTheWorld/startTheWorld, gcStart and the mark/sweep engine, mcall(gosched_m),
// and the g/m/p stack walk. Converted faithfully they compile, then die on the first getg() or
// mcall() assembly stub: sync's TestPool died in debug.SetGCPercent, TestOnceXGC in runtime.GC →
// gcStart → acquirem → getg, TestParallelReaders in GOMAXPROCS → stopTheWorldGC → semacquire →
// getg, and runtime.Gosched → mcall took the whole test host down mid-run.
//
// The fork this takes is the one sync's Mutex/notifyList established (docs/Baseline-vs-
// FullConversion.md, "Hand-owning a package to make it OPERATIONAL"): where a Go mechanism has no
// managed counterpart but its PUBLIC CONTRACT does, reimplement the CONTRACT at the API boundary
// and never emulate the mechanism. The alternative — synthesizing a fake g/m so the converted
// scheduler can walk it — buys nothing: the code underneath would still need a real run queue, a
// real heap, and real stacks. Everything below these entry points stays auto-converted and simply
// becomes unreachable.
//
// The converter drops the auto forms of exactly these declarations (manualConversionFuncs
// ["runtime"] in go2cs/manualTypeOperations.go), leaving a placeholder comment at each site.
//
// Honest divergences, stated once:
//   - GOMAXPROCS is a real GET/SET of a remembered value but does NOT cap parallelism: a goroutine
//     is a managed thread and the CLR schedules it. The universal test idiom
//     `defer runtime.GOMAXPROCS(runtime.GOMAXPROCS(n))` is exactly right; a program that measures
//     actual parallelism against it is capability-divergent.
//   - Stack() walks the MANAGED stack and renders it in GO'S SHAPE (`<pkg>.<Func>()` + a
//     tab-indented `<file>:<line>`), because a traceback is observable output that Go programs and
//     Go's own tests grep by package-qualified function name. Frames that already unwound are
//     recovered for the case that matters: while a panic is in flight, golib's snapshot of the
//     panic's origin is appended below the live frames — where Go's traceback also shows them,
//     since a deferred call runs on top of gopanic. Frames that are not converted Go code (golib,
//     the BCL, the test host) keep their .NET names rather than being given invented Go ones, and
//     Go's `+0x<offset>` PC deltas are omitted.
//   - Stack(all=true) enumerates EVERY live goroutine — golib's registry, in goid order, as Go
//     dumps them — with a truthful header per goroutine: the id the registry minted, and the wait
//     reason the park accounting recorded (Go's own waitReasonStrings; see golib's WaitReason).
//     The CALLING goroutine's block carries real frames; every other goroutine's carries ONE line
//     saying its stack is unavailable, because the CLR has no supported cross-thread stack walk and
//     inventing frames for an unwalked stack is the one thing a traceback must not do. Capturing a
//     goroutine's own stack AT PARK TIME is Stage B of docs/phase4/DESIGN-cooperative-scheduler.md,
//     held behind the synthetic-PC registry that would symbolize it. Two further honest limits:
//     `running` covers Go's _Grunnable as well as _Grunning (no P, no run queue, nothing to ask),
//     and Go's ` (scan)` / `, N minutes` / `, locked to thread` header decorations are omitted
//     rather than invented.
//   - ReadMemStats fills the fields the CLR genuinely measures and leaves the allocator-internal
//     ones (Mallocs/Frees/HeapObjects/BySize) zero rather than inventing numbers. The per-GC pause
//     history, LastGC, PauseTotalNs and NumGC come from golib's GcPauseRecorder (one gen2 recorder,
//     one ring, one snapshot shared with runtime/debug.readGCStats); HeapReleased is
//     max(0, committedHighWater - currentCommitted). Both are docs/phase4/DESIGN-readmemstats-
//     surface.md, ratified 2026-08-21 — read the recorder's own header for the mechanism, the
//     measured boundaries and the GO2CS_GC_PAUSE_HISTORY=0 escape hatch. Two MemStats invariants
//     stated here rather than repaired, because repairing either would mean inventing or clamping a
//     measured number (§4.4): `Sys == StackSys + MSpanSys + ... + OtherSys` is FALSE (Sys is
//     committed bytes while every breakdown term is an allocator arena the CLR does not partition),
//     and `HeapIdle >= HeapReleased` can be false after a large release (HeapIdle is instantaneous,
//     HeapReleased is a difference against a historical high-water mark). GCCPUFraction stays ZERO
//     for the same rule: the adjacent CLR quantity, PauseTimePercentage, is pause time as a share of
//     wall time since the last GC, where Go's field is GC's share of the program's available CPU
//     since it started — a number in the right range and of the wrong kind.
//   - Goexit is exact for the GOROUTINE case (defers run, recover() sees nil, no other goroutine is
//     affected) and GATED for the main goroutine, whose "main ends but the program keeps running"
//     shape has no managed counterpart yet — docs/phase4/DESIGN-goexit.md option C.
//   - gcount — and therefore NumGoroutine, /sched/goroutines, and the goroutine profile's size and
//     count — reports golib's live goroutine registry. It COUNTS UP EARLY-BY-ONE AND DECAYS LATE.
//     Measured against Go on the same program: with eight goroutines blocked on a channel it reads 8
//     where Go reads 9, and immediately after the WaitGroup releases them it reads 9 where Go reads
//     1. Go's own caveat — "all these variables can be changed concurrently, so the result can be
//     inconsistent" — covers the climb; the DECAY LAG is ours, because a goroutine's registry slot is
//     retired after its body returns rather than at the instant it does. Stated in those terms rather
//     than as "approximate" because the DIRECTION is what matters to consumers: a leak check sampling
//     during teardown reads a stale HIGH count, which reads as a leak rather than as a miscount.
//     Not repaired: no consumer's guard needs prompt decay today (net/http/httputil's leak check
//     passes against these values at its `<= 4` threshold), so a timing fix would be speculative
//     machinery. It is a board item whose trigger is the first flaky leak check, or the first
//     consumer that needs prompt decay.
//   - LockOSThread/UnlockOSThread carry GO'S WHOLE BODY, and the split is worth stating exactly.
//     The BINDING they exist to provide — "this goroutine will not be migrated to another OS
//     thread" — holds BY CONSTRUCTION here, since go2cs runs each goroutine on its own managed
//     thread; that half is a no-op and always was. The ACCOUNTING is NOT: `m.lockedExt`,
//     `m.lockedInt` and the `m.lockedg`/`g.lockedm` back-links are state Go's own suite reads back
//     through `runtime.LockOSCounts`, and until 2026-09-13 these four bodies were empty, so the
//     counters read 0,0 where Go reads 1,0. ⚠ This bullet claimed the WHOLE pair was a no-op by
//     construction, which conflated the two halves and is why the gap survived: the binding was
//     the reason given, and the accounting was never separately checked.
//   - Callers()/callers()/Frames.Next() walk the MANAGED stack projected to GO-LOGICAL frames:
//     only converted Go declarations and function literals count — adapter shells (IGoAdapter) and
//     go2cs-gen forwarders are dispatch plumbing Go has no frame for, and golib/the BCL/the test
//     host are not Go code. RELATIVE depths between two Callers calls on one goroutine therefore
//     match Go's logical model (io's multiReader flatten tests assert exactly this). A host method
//     marked GoStackRoot reports the Go frame it stands in for (the test host's testing.tRunner),
//     and a goroutine's walk ends at runtime.goexit as Go's does; the main goroutine's runtime.main
//     root is not modeled. PC values are opaque
//     process-lifetime tokens, never addresses; Frame.Function is the Go spelling (goFrameName);
//     Frame.File/Line name the GO position the conversion recorded for that frame, and the
//     converted `.cs` position where it recorded none (goFramePosition). FuncForPC
//     and Frame.Func stayed unimplemented/nil while a *Func had no managed referent; that
//     premise EXPIRED when ManagedPointerTokens landed, and FuncForPC/Func.Name are managed
//     below as of 2026-08-29, joined by Func.Entry/Func.FileLine as of 2026-09-02. Frame.Func stayed
//     nil until 2026-09-28 (census A2, D3): Frames.Next now sets it for every Go frame, interned per
//     function, with Frame.startLine beside it (see Frames.Next). getcallersp
//     itself remains an honest stub: a caller's stack pointer has no managed answer, so the
//     chain is severed HERE, at the API boundary that does (the methodName precedent).
//     runtime.Caller stays AUTO-converted and works through the same walk, because the funnel it
//     calls — the lower-case `callers` — is hand-owned here too, so it reads the same positions a
//     traceback does. The POSITION MAP is what supplies them: one `[assembly: GoPositionMap]`
//     record per converted file, emitted into that file, carrying the Go file's identity AND its
//     C#-line → Go-line table together. The pair is INDIVISIBLE by construction (coordinator
//     ruling, 2026-08-21) — a Go file paired with a C# line is a position in NEITHER tree — so a
//     frame either has a record and reports a Go position that exists, or has none and reports the
//     converted `.cs` position, which is what golib, the BCL, the hand-owned test host and every
//     whole-file hand-own do. Nothing composes one half from the other. `log`'s TestAll pins
//     `(63|65)` in log_test.go and now reads them, because the conversion recorded them.
//
// Hand-owned: there is no managed_impl.go, so a -stdlib reconvert never regenerates this file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using go.golib;
// The plain namespace using is what brings internal/runtime/atomic's [GoRecv] extension methods
// (Int64.Load and friends) into scope — an alias alone does not participate in extension lookup.
using go.@internal.runtime;
using abi = go.@internal.abi_package;
using profilerecord = go.@internal.profilerecord_package;
using @unsafe = go.unsafe_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // ⟨OQ-1⟩ as ratified: the GC pause recorder is ALWAYS ON, armed from this package's initializer.
    // The two alternatives were rejected on correctness rather than cost — arming on the first
    // ReadMemStats would make NumGC LESS true than it is today (either reporting 0 when the CLR has
    // really collected N times, or claiming a ring it cannot fill), and arming only under the test
    // host would make a measurement surface answer differently under test than in production, which
    // is the one shape a measurement surface must never have. Its measured price is one finalizer
    // run and one GetGCMemoryInfo call per gen2 collection, which is below the noise floor of a
    // 1.25-1.64 ms collection (docs/phase4/DESIGN-readmemstats-surface.md §7.1.3).
    //
    // runtime/debug arms it from its own module initializer too: either assembly can be the first a
    // program touches, and GcPauseRecorder.Arm is idempotent.
    [ModuleInitializer]
    internal static void ᴛArmGcPauseRecorder()
    {
        GcPauseRecorder.Arm();
    }

    // The traceback half of Go's crash report for a panic nobody recovered. golib composes the
    // report (go.golib.CrashReport) because it is the only assembly both the Phase-4 test host and
    // every converted program share, but it cannot spell a Go frame name or map a converted .cs
    // line back to its Go position — that is this file's machinery. So the dependency inverts
    // exactly as the divide-by-zero panic VALUE does in panicvalues_impl.cs: golib declares the
    // hook, the runtime package fills it here. See docs/phase4/DESIGN-crash-report.md.
    [ModuleInitializer]
    internal static void ᴛRegisterCrashTraceback()
    {
        CrashReport.TracebackRenderer = crashTraceback;
    }

    // Exactly the block debug.Stack() produces, from the same appendGoFrames: the header Go writes
    // above a traceback, then one `<pkg>.<Func>()` line per frame with its tab-indented Go position
    // beneath. A crash renders nothing new — runtime/debug's TestStack already compares these very
    // frames against Go's own expectations, frame for frame.
    private static string crashTraceback(PanicException panic, Exception thrown)
    {
        // PanicTrace is the ORIGIN, snapshotted at the first catch, and is the right answer
        // whenever the panic passed through a deferred sequence: re-raising a stored instance
        // resets Exception.StackTrace to the re-raise point, and a synthesized runtime-error panic
        // was never thrown at all. A panic no frame ever caught — a panic() in a function with no
        // defer, which is what runtime/debug_test.TestMain does — has no snapshot, and there the
        // exception that actually travelled still carries the throw site.
        StackTrace stack = panic.PanicTrace ?? new StackTrace(thrown, fNeedFileInfo: true);
        StringBuilder trace = new();

        trace.Append("goroutine 1 [running]:\n");
        appendGoFrames(trace, stack);

        return trace.ToString();
    }

    // The traceback half of Go's report for a FATAL error — runtime.throw and runtime.fatal, the
    // path no recover() can see. Registered into golib for the same inverted-dependency reason the
    // crash traceback is, and beside it deliberately: golib composes the report and owns the
    // `fatal error:` line, this file owns the goroutine blocks, and neither can move the other's.
    // docs/phase4/DESIGN-fatal-path.md.
    [ModuleInitializer]
    internal static void ᴛRegisterFatalTraceback()
    {
        FatalReport.TracebackRenderer = fatalTraceback;
    }

    private static readonly RuntimeMethodHandle s_fatalTracebackMethodHandle =
        typeof(runtime_package).GetMethod(
            nameof(fatalTraceback),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            binder: null,
            [typeof(bool)],
            modifiers: null)!.MethodHandle;

    // Go's fatal dump: the failing goroutine's real frames, then EVERY other goroutine — five
    // headers on the probe's own row at the corpus pin, not one (DESIGN-fatal-path.md §1). It is
    // the same renderStack the panic side and runtime.Stack use; what this method owns is which
    // frames it starts from and which goroutines it is allowed to show.
    //
    // THE BOUNDARY. callerFrames anchors on this method's own frame, so the frames above it are
    // golib's fatal plumbing (FatalReport.Format, FatalReport.Fatal, and whatever delegate stub the
    // runtime put between them) and then the converted function that raised the fatal. Everything
    // up to and including the LAST FatalReport frame is dropped, by TYPE IDENTITY rather than by a
    // name or a count: the first frame rendered is therefore runtime.throw / runtime.fatal itself,
    // which is exactly the frame Go's own traceback begins at, because Go starts its walk from
    // fatalthrow's getcallerpc() — the return address inside throw. Dropping to the LAST such frame
    // rather than skipping while-they-match is what makes a delegate-invoke stub between them
    // harmless.
    //
    // THE SYSTEM-GOROUTINE AXIS is Go's throwType, read here because GOTRACEBACK is read here.
    // Go's gotraceback raises the level for a throwTypeRuntime crash — "the runtime is crashing due
    // to a runtime error, so print system goroutines and runtime frames" — while a throwTypeUser
    // fatal leaves it alone. So a throw shows them unconditionally and a fatal shows them only
    // under GOTRACEBACK=system or crash.
    //
    // The runtime-FRAME half of that same Go sentence is NOT implemented: this renderer has no
    // frame filter, so a `fatal` prints the runtime frames a `throw` does. Stated rather than
    // silently approximated — it costs a user-fault dump some noise and can never hide a frame.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string fatalTraceback(bool userFault)
    {
        List<StackFrame> above = new(callerFrames(s_fatalTracebackMethodHandle));
        int start = 0;

        for (int i = 0; i < above.Count; i++)
        {
            if (above[i].GetMethod()?.DeclaringType == typeof(FatalReport))
                start = i + 1;
        }

        List<StackFrame> frames = above.GetRange(start, above.Count - start);

        return renderStack(frames, all: true, showSystem: !userFault || s_tracebackShowsSystem);
    }

    // GOMAXPROCS' remembered setting. Go's starts at NumCPU.
    private static nint s_gomaxprocs = Environment.ProcessorCount;

    // GOMAXPROCS sets the maximum number of CPUs that can be executing simultaneously and returns
    // the previous setting. If n < 1, it does not change the current setting.
    public static nint GOMAXPROCS(nint n)
    {
        nint previous = Volatile.Read(ref s_gomaxprocs);

        if (n < 1 || n == previous)
            return previous;

        // Go changes the P count inside stopTheWorldGC(stwGOMAXPROCS); the pause is recorded the
        // same way (the stop-the-world contract, stopTheWorld below).
        worldStop stw = stopTheWorldGC(stwGOMAXPROCS);

        lock (s_cpuStatsLock)
        {
            Volatile.Write(ref s_gomaxprocs, n);

            // procresize's time bookkeeping, verbatim (proc.go): sched.totaltime integrates
            // gomaxprocs over time, and cpuStats.accumulate reads it and the gomaxprocs global to
            // compute /cpu/classes/total. There are no Ps to resize, so this is all of procresize
            // the managed host has.
            int64 now = nanotime();
            if (sched.procresizetime != 0)
                sched.totaltime += (int64)gomaxprocs * (now - sched.procresizetime);
            sched.procresizetime = now;
            gomaxprocs = (int32)n;
        }

        startTheWorldGC(stw);

        return previous;
    }

    // THE /cpu/classes ACCOUNTING. Go writes work.cpuStats only at gcMarkTermination
    // (accumulateGCPauseTime, then accumulate, then sched.idleTime reset), which the managed host
    // never runs, so every /cpu/classes metric and ReadCPUStats read 0. This feeds Go's own state
    // from the CLR and leaves accumulate, accumulateGCPauseTime and cpuStatsAggregate.compute
    // auto-converted:
    //   - total: sched.totaltime/procresizetime, kept by GOMAXPROCS above and started below.
    //   - GC pause: the interval's GC.GetTotalPauseDuration delta, times gomaxprocs, through
    //     accumulateGCPauseTime (Go multiplies each pause by maxprocs the same way).
    //   - idle: the interval's total, less the pause CPU, less the process CPU spent outside the
    //     pauses, floored at 0. Go counts Ps sitting in _Pidle; with no Ps, the CPU the process did
    //     not use is that time. User is accumulate's remainder, so every identity holds.
    // The mark classes stay 0: runtime.GC() runs blocking CLR collections, which mark inside their
    // pause, and a background CLR gen2's mark time is not separately measurable. The scavenge
    // classes stay 0: there is no scavenger.
    //
    // WHEN (COORD ruling, ledger 2026-09-27 00:35): a LAZY CATCH-UP at the read point rather than a
    // callback per collection. Go's values change only when a GC cycle completes; a Go cycle is a
    // CLR gen2 (GcPauseRecorder's definition), so a reader accounts the interval since the last
    // accounting only when the gen2 count has advanced. readMetricsManaged runs it before the
    // metrics are computed, and runtime.GC() ends with it. No completed cycle, no change: the cost
    // on that path is one CollectionCount read.
    private static readonly object s_cpuStatsLock = new();
    private static int s_cpuStatsGen2;
    private static long s_cpuStatsPauseNs;
    private static long s_cpuStatsProcessNs;
    private static int64 s_cpuStatsTotal;

    [ModuleInitializer]
    internal static void ᴛStartCPUStats()
    {
        lock (s_cpuStatsLock)
        {
            // Go's schedinit calls procresize, which is where procresizetime first gets a value.
            sched.procresizetime = nanotime();
            s_cpuStatsGen2 = System.GC.CollectionCount(System.GC.MaxGeneration);
            s_cpuStatsPauseNs = System.GC.GetTotalPauseDuration().Ticks * 100;
            s_cpuStatsProcessNs = Environment.CpuUsage.TotalTime.Ticks * 100;
        }
    }

    internal static void catchUpCPUStats()
    {
        if (System.GC.CollectionCount(System.GC.MaxGeneration) == Volatile.Read(ref s_cpuStatsGen2))
            return;

        lock (s_cpuStatsLock)
        {
            int gen2 = System.GC.CollectionCount(System.GC.MaxGeneration);

            if (gen2 == s_cpuStatsGen2)
                return;

            int64 now = nanotime();
            long pauseNs = System.GC.GetTotalPauseDuration().Ticks * 100;
            long processNs = Environment.CpuUsage.TotalTime.Ticks * 100;
            int64 total = sched.totaltime + (now - sched.procresizetime) * (int64)gomaxprocs;

            int64 pause = pauseNs - s_cpuStatsPauseNs;
            int64 pauseCPU = pause * (int64)gomaxprocs;
            int64 busy = Math.Max(0L, processNs - s_cpuStatsProcessNs - pause);
            int64 idle = Math.Max(0L, total - s_cpuStatsTotal - pauseCPU - busy);

            ref cpuStats stats = ref work.cpuStats;
            stats.accumulateGCPauseTime(pause, gomaxprocs);
            Ꮡsched.of(schedt.ᏑidleTime).Store(idle);
            stats.accumulate(now, false);
            Ꮡsched.of(schedt.ᏑidleTime).Store(0);

            s_cpuStatsTotal = total;
            s_cpuStatsPauseNs = pauseNs;
            s_cpuStatsProcessNs = processNs;
            Volatile.Write(ref s_cpuStatsGen2, gen2);
        }
    }

    /// <summary>
    /// GolibTests' probe for the /cpu/classes accounting (RuntimeCPUStatsTests): reads work.cpuStats,
    /// the snapshot runtime/metrics and the ReadCPUStats export report, plus the Go global gomaxprocs.
    /// Order: GCAssist, GCDedicated, GCIdle, GCPause, GCTotal, ScavengeAssist, ScavengeBg,
    /// ScavengeTotal, Idle, User, Total (all cpu-ns), then gomaxprocs.
    /// </summary>
    public static long[] GoCPUStatsProbe()
    {
        cpuStats s = work.cpuStats;

        return [s.GCAssistTime, s.GCDedicatedTime, s.GCIdleTime, s.GCPauseTime, s.GCTotalTime,
            s.ScavengeAssistTime, s.ScavengeBgTime, s.ScavengeTotalTime, s.IdleTime, s.UserTime,
            s.TotalTime, gomaxprocs];
    }

    // Gosched yields the processor, allowing other goroutines to run. It does not suspend the
    // current goroutine, so execution resumes automatically.
    public static void Gosched()
    {
        // A bare Thread.Yield honored the "give someone else a turn, then carry on" contract on
        // Windows but not on Linux, where it lowers to sched_yield(2) and CFS leaves CPU-bound
        // yielders effectively in place — a strict handoff ring (sync/atomic's CAS-concurrent
        // test) starved for 45+ minutes there against 183 s on the same hardware under Windows.
        // GoschedBackoff keeps the contract by measuring each yield and escalating consecutive
        // provably-inert ones to a 1 ms sleep, which leaves the run queue so a starved goroutine's
        // thread can actually run (board finding 2026-08-21, ratified).
        golib.GoschedBackoff.Yield();
    }

    // registerPoolCleanup is where sync's //go:linkname runtime_registerPoolCleanup crosses into this
    // assembly. The symbol that linkname names, sync_runtime_registerPoolCleanup (mgc.cs), is
    // `internal` under the exported-ness rule, and a cross-assembly forwarder cannot reach an internal
    // target — the same constraint blockUntilEmptyFinalizerQueue documents in mfinal.cs. So sync calls
    // this shim, which hands the cleanup to the converted registration unchanged.
    public static void registerPoolCleanup(Action cleanup) => sync_runtime_registerPoolCleanup(cleanup);

    // godebugRegisterMetric is where internal/godebug's //go:linkname registerMetric crosses into
    // this assembly (the registerPoolCleanup pattern above). The symbol that linkname names,
    // godebug_registerMetric (metrics.cs), is `internal` under the exported-ness rule, so the
    // hand-owned godebug calls this shim, which hands the registration to the converted
    // implementation unchanged — it swaps the metric's compute0 placeholder for the real counter
    // read, and runtime/metrics.Read reports it from then on.
    public static void godebugRegisterMetric(@string name, Func<uint64> read) => godebug_registerMetric(name, read);

    // godebugSetNewIncNonDefault is internal/godebug's //go:linkname setNewIncNonDefault, the same
    // crossing: Go's godebug registers its counter factory from init, and the runtime's own GODEBUG
    // settings (panicnil, asynctimerchan, ...) count their non-default events through it
    // (godebugInc.IncNonDefault). Without the registration every such increment was dropped on the
    // floor — Go's documented behavior only for "calls before internal/godebug registers itself".
    public static void godebugSetNewIncNonDefault(Func<@string, Action> newIncNonDefault) => godebug_setNewIncNonDefault(newIncNonDefault);

    // syscallRuntimeSetenv / syscallRuntimeUnsetenv are syscall's runtimeSetenv/runtimeUnsetenv, the
    // same crossing. The environment itself needs no mirroring (syscall reads the live process
    // environment), but the runtime's HALF of the call does: when the key is GODEBUG it reparses the
    // runtime's atomic debug settings, which is how `t.Setenv("GODEBUG", "panicnil=1")` reaches
    // debug.panicnil. A no-op in syscall left every such setting at its startup value.
    public static void syscallRuntimeSetenv(@string key, @string value) => syscall_runtimeSetenv(key, value);

    public static void syscallRuntimeUnsetenv(@string key) => syscall_runtimeUnsetenv(key);

    // GC runs a garbage collection and blocks the caller until the garbage collection is complete.
    public static void GC()
    {
        // Go's gcStart runs clearpools() at the START of every cycle, and that is what ages
        // sync.Pool's victim cache — without it a Pool never releases what it cached. All three of
        // clearpools' arms are wired here.
        if (poolcleanup != default!)
            poolcleanup();

        // The boringcrypto caches, the third arm — and the one that used to be missing, because
        // Go clears it with `atomicstorep(p, nil)` stores into registered ADDRESSES and the
        // registered word (an atomic.Pointer[cacheTable[K,V]], whose managed slot holds a
        // reference) is not pinnable, so its address recovers nothing. golib.BoringCaches carries
        // the reasoning in full; the short of it is that a registration is a clear DELEGATE here —
        // the same currency the two arms above already use — so this runs the very Clear that Go's
        // own comment says the runtime performs at each collection.
        //
        // Called DIRECTLY rather than left to the registry's per-collection sentinel: Go's GC() is
        // documented to complete a full cycle, and bcache's suite reads the registered cache on the
        // statement after runtime.GC() returns. The converted clearpools() in mgc.cs keeps its
        // faithful boringCaches walk and is simply inert — nothing registers a raw pointer into it
        // any more, which is the point rather than a defect.
        golib.BoringCaches.ClearAll();

        // unique's map cleanup, the second arm — verbatim clearpools(): a NON-BLOCKING send that
        // wakes the goroutine unique_runtime_registerUniqueMapCleanup parked on this channel, which
        // evicts every intern-map entry whose weak pointer has gone nil. Inert until unique.Make has
        // run (the channel is nil before registration), so nothing else pays for it.
        //
        // Wiring it is not cosmetic: `unique`'s own suite calls drainMaps() — arm a one-shot
        // notification, runtime.GC(), then BLOCK on `<-wait` until the cleanup runs — so with this
        // arm missing the cleanup could never run and every TestHandle subtest deadlocked, taking
        // the whole test host to its package timeout and erasing the verdicts of the rows that had
        // nothing to do with it. That deadlock only became REACHABLE once internal/weak stopped
        // panicking (its hand-own, same arc); before that the subtests died one frame earlier.
        if (uniqueMapCleanup != default!)
            uniqueMapCleanup.TrySend(new EmptyStruct());

        // Go's GC() is documented to complete a full cycle, and callers (sync's pool/oncefunc
        // tests among them) rely on finalizers having RUN by the time it returns. The second
        // collect reclaims what the finalizers released, matching the state a completed Go cycle
        // leaves behind.
        //
        // The blocking collection is the cycle's stopped-world phase, so it takes the pair Go's
        // mark termination takes (stwGCMarkTerm, a GC pause in /sched/pauses).
        worldStop stw = stopTheWorldGC(stwGCMarkTerm);
        System.GC.Collect(System.GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        startTheWorldGC(stw);
        System.GC.WaitForPendingFinalizers();

        // The CLR wait above only drains the CLR's OWN finalizer queue, and a GoFinalizerSentinel's
        // `~` does nothing there but HAND the Go finalizer to the runner thread (mfinal.cs's
        // GoFinalizerQueue — the `fing` analogue; its header says why the body must not run inline).
        // So this second wait is what actually makes the sentence above true.
        //
        // It is BOUNDED, and the bound is the point rather than a hedge: Go's runtime.GC() waits for
        // no finalizer body at all, so a finalizer that blocks until its CALLER does something is
        // legal Go — runtime/pprof's TestGoroutineCounts registers exactly that (`close(fingReady)`
        // then `<-c`, with `c` closed only at the end of the test) and used to deadlock here. The
        // budget is a safety net sized for the slowest legitimate host, never a performance
        // assumption: a well-behaved finalizer completes in microseconds, and a PARKED one is
        // recognized so later collections do not re-pay the budget (GoFinalizerQueue.WaitForIdle).
        GoFinalizerQueue.WaitForIdle(GoFinalizerQueue.DrainBudgetMs);

        System.GC.Collect(System.GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

        // The heap profile's cycle for this collection, in Go's order: mark termination's
        // mProf_NextCycle and mProf_Flush, the sweep's frees, then the mProf_PostSweep that Go's GC()
        // calls once sweeping is done (mprof_impl.cs, class M piece M2).
        memProfileCycle(requested: true);

        // §3.4's mitigation. The pause recorder's sentinel is woken by the FINALIZER thread, so a
        // ReadMemStats landing in the gap between a collection completing and its finalizer running
        // would see NumGC one short. Go's GC() is documented to complete a full cycle and its tests
        // rely on the world being quiet when it returns, so this is exactly the boundary where the
        // lag must be closed: wait the finalizer out and observe directly. Observe() is idempotent
        // per collection, so the direct call cannot double-record what the finalizer already took,
        // and vice versa. The counter is Go's NumForcedGC — "GC cycles that were forced by the
        // application calling the GC function" — which is a fact about the PROGRAM, so it is counted
        // whether or not the recorder is armed.
        GcPauseRecorder.Drain();
        GcPauseRecorder.NoteForcedGC();

        // Go's cycle ends in gcMarkTermination, which is where work.cpuStats is written.
        catchUpCPUStats();
    }

    // metricsLock/metricsUnlock protect the runtime metrics table (initMetrics' map and the agg
    // scratch state) for readMetrics (behind runtime/metrics.Read) and readMetricNames (behind
    // the runtime/metrics_test push). Go's bodies acquire metricsSema — a runtime sleeping
    // semaphore whose acquire path is getg() → sudog → gopark, none of which exists under the
    // CLR — with handoff enabled because metrics operations are long. The contract is mutual
    // exclusion with waiter handoff, and SemaphoreSlim is the CLR's spelling of exactly that.
    // Everything the lock protects stays auto-converted.
    //
    // The HOLDER is recorded, as worldsema's is below, so a panic inside the region releases the
    // lock (releaseStoppedWorldOnPanic): the 2026-09-26 park came from this lock, when
    // readMetricsLocked threw while holding it and the next reader waited for ever. Lock and unlock
    // therefore run on one goroutine, which every Go caller does (each is a single function body).
    private static readonly SemaphoreSlim s_metricsSema = new(1, 1);
    private static int s_metricsSemaHolder;

    internal static void metricsLock()
    {
        s_metricsSema.Wait();
        Volatile.Write(ref s_metricsSemaHolder, Environment.CurrentManagedThreadId);
    }

    // Releases only what the calling goroutine still holds: after a panic released it, a later
    // unlock on the unwinding path is a no-op rather than a second release.
    internal static void metricsUnlock()
    {
        if (Interlocked.CompareExchange(ref s_metricsSemaHolder, 0, Environment.CurrentManagedThreadId) == Environment.CurrentManagedThreadId)
            s_metricsSema.Release();
    }

    // ---- STOP THE WORLD: the contract model (owner ruling, ledger 2026-09-28 00:40) --------------
    //
    // stopTheWorld (proc.go) takes worldsema and records a /sched/pauses stopping sample;
    // startTheWorld records the total sample and releases worldsema with handoff. That is Go's
    // contract minus the stop itself: other goroutines are NOT suspended, since the CLR cannot do it
    // safely (a real goroutine barrier was ruled too invasive). A caller that reads state it expects
    // the world to hold still reads it live, and each region the runtime reaches is managed:
    // flushallmcaches is a no-op without Ps, the debug log is managed memory (debuglog_impl.cs),
    // readMetricsLocked crosses through runtime/metrics (below), StartTrace starts golib's managed
    // tracer inside the pair (<goos>/trace_impl.cs), and the regions with no managed form refuse
    // BEFORE the world is stopped (goroutineProfileWithLabels' fill path, doAllThreadsSyscall on
    // linux).
    //
    // Go's stopTheWorldWithSema stops every P and its startTheWorldWithSema restarts them; both
    // record the pause into sched's four timeHistograms. There are no Ps here (m.p is nil by
    // construction), so the stop is the acquisition itself: startedStopping and finishedStopping
    // bracket it, stoppingCPUTime is 0. m.preemptoff and the STW trace events are not kept.
    //
    // THE LEAK. A region that throws while holding worldsema left it held for every later caller
    // (2026-09-26: the runtime row's host parked in TestDebugLogInterleaving). Go fatals there
    // ("panic during preemptoff"). The managed host's safety net is releaseStoppedWorldOnPanic,
    // registered as golib's RuntimeErrorPanic.PanicObserved: when a Go frame's panic filter sees an
    // exception on the goroutine that holds worldsema (or metricsSema), the permit is released.
    // startTheWorld releases only by CAS on the recorded holder, so there is never a double release.
    // stopTheWorldGC/startTheWorldGC stay converted: gcsema around this pair.
    private static int s_worldsemaHolder;

    [ModuleInitializer]
    internal static void ᴛRegisterStoppedWorldRelease()
    {
        RuntimeErrorPanic.PanicObserved = releaseStoppedWorldOnPanic;
    }

    private static void releaseStoppedWorldOnPanic()
    {
        int me = Environment.CurrentManagedThreadId;

        // Reverse acquisition order: a region that holds both took worldsema first (ReadMetricsSlow).
        if (Volatile.Read(ref s_metricsSemaHolder) == me && Interlocked.CompareExchange(ref s_metricsSemaHolder, 0, me) == me)
            s_metricsSema.Release();

        if (Volatile.Read(ref s_worldsemaHolder) == me && Interlocked.CompareExchange(ref s_worldsemaHolder, 0, me) == me)
            semrelease1(Ꮡworldsema, true, 0);
    }

    internal static worldStop stopTheWorld(stwReason reason)
    {
        semacquire(Ꮡworldsema);
        Volatile.Write(ref s_worldsemaHolder, Environment.CurrentManagedThreadId);

        int64 start = nanotime();
        int64 finished = nanotime();

        if (reason.isGC())
            StwPauses.StoppingGC.record(finished - start);
        else
            StwPauses.StoppingOther.record(finished - start);

        return new worldStop(reason: reason, startedStopping: start, finishedStopping: finished, stoppingCPUTime: 0);
    }

    internal static void startTheWorld(worldStop w)
    {
        int64 total = nanotime() - w.startedStopping;

        if (w.reason.isGC())
            StwPauses.TotalGC.record(total);
        else
            StwPauses.TotalOther.record(total);

        int me = Environment.CurrentManagedThreadId;

        if (Interlocked.CompareExchange(ref s_worldsemaHolder, 0, me) == me)
            semrelease1(Ꮡworldsema, true, 0);
    }

    // sched's four stop-the-world timeHistograms, each with its cells' words boxed ONCE. The
    // converted timeHistogram.record (histogram.cs) takes a fresh element box per sample
    // (`Ꮡ(h.counts, i).Add(1)`), and atomic's Uint64.Add a fresh box of the word under it, while
    // ReadMemStats, which now stops the world, is held to zero allocations per call (GolibTests'
    // ReadMemStatsPerCallAllocation). So each cell's word address is taken once (WordAddress) and
    // the sample added through Xadd64. The bucket arithmetic is record's, verbatim. The class
    // initializer runs once, on the first stop.
    private static class StwPauses
    {
        internal static readonly PauseHistogram StoppingGC = new(Ꮡsched.of(schedt.ᏑstwStoppingTimeGC));
        internal static readonly PauseHistogram StoppingOther = new(Ꮡsched.of(schedt.ᏑstwStoppingTimeOther));
        internal static readonly PauseHistogram TotalGC = new(Ꮡsched.of(schedt.ᏑstwTotalTimeGC));
        internal static readonly PauseHistogram TotalOther = new(Ꮡsched.of(schedt.ᏑstwTotalTimeOther));
    }

    private sealed class PauseHistogram
    {
        private readonly ж<uint64> m_underflow;
        private readonly ж<uint64> m_overflow;
        private readonly ж<uint64>[] m_counts;

        internal PauseHistogram(ж<timeHistogram> h)
        {
            m_underflow = h.of(timeHistogram.Ꮡunderflow).WordAddress();
            m_overflow = h.of(timeHistogram.Ꮡoverflow).WordAddress();
            m_counts = new ж<uint64>[(int)len(h.Value.counts)];

            for (int i = 0; i < m_counts.Length; i++)
                m_counts[i] = Ꮡ(h.Value.counts, i).WordAddress();
        }

        internal void record(int64 duration)
        {
            if (duration < 0)
            {
                atomic_package.Xadd64(m_underflow, 1);
                return;
            }

            nuint bucketBit;
            nuint bucket;
            nint l = sys_package.Len64((uint64)duration);

            if (l < timeHistMinBucketBits)
            {
                bucketBit = timeHistMinBucketBits;
                bucket = 0;
            }
            else
            {
                bucketBit = (nuint)l;
                bucket = bucketBit - (nuint)timeHistMinBucketBits + 1;
            }

            if (bucket >= timeHistNumBuckets)
            {
                atomic_package.Xadd64(m_overflow, 1);
                return;
            }

            nuint subBucket = (nuint)(duration >> (int)(bucketBit - 1 - (nuint)timeHistSubBucketBits)) % (nuint)timeHistNumSubBuckets;
            atomic_package.Xadd64(m_counts[(int)(bucket * (nuint)timeHistNumSubBuckets + subBucket)], 1);
        }
    }

    // flushallmcaches (mstats.go) flushes every P's mcache. There are no Ps, so there is nothing
    // to flush: ReadMemStatsSlow, ReadMetricsSlow and readmemstats_m reach it inside their stopped
    // world, and the converted body indexed allp out of range there while holding worldsema.
    internal static void flushallmcaches()
    {
    }

    // goroutineProfileWithLabels (mprof.go), behind runtime.GoroutineProfile (ruling Q3). Go's
    // concurrent collector takes goroutineProfile.sema and the world, counts, and returns (n, false)
    // at once when the caller's slice is too short; otherwise it records every goroutine's stack
    // through sys.GetCallerSP/GetCallerPC, intrinsics that stay throwing by ruling. So the COUNT
    // path is kept, pair and all, over golib's goroutine registry (gcount), and the FILL path
    // refuses by name BEFORE any semaphore, a stated limit: runtime/pprof's goroutine profile does
    // not come through here, it has its own managed body (runtime/pprof/pprof_impl.cs).
    internal static (nint n, bool ok) goroutineProfileWithLabels(slice<profilerecord.StackRecord> Δp, slice<@unsafe.Pointer> labels)
    {
        if (gcount() <= len(Δp))
            throw refuseGoroutineProfileFill();

        semacquire(ᏑgoroutineProfile.of(goroutineProfileᴛ1.Ꮡsema));
        worldStop stw = stopTheWorld(stwGoroutineProfile);
        nint n = gcount();
        startTheWorld(stw);
        semrelease(ᏑgoroutineProfile.of(goroutineProfileᴛ1.Ꮡsema));

        // A goroutine exited between the unlocked count and this one: the slice now fits, and the
        // fill path is still the one this host cannot take.
        if (n <= len(Δp))
            throw refuseGoroutineProfileFill();

        return (n, false);
    }

    private static PanicException refuseGoroutineProfileFill() =>
        new("runtime: goroutineProfileWithLabels: filling the profile records each goroutine's stack through sys.GetCallerSP/GetCallerPC, which do not exist in the managed model; only the count is kept (runtime/pprof's goroutine profile has its own managed body)");

    // runtime/debug.WriteHeapDump (heapdump.go), ruling Q4 (a): the pair, and a well-formed MINIMAL
    // dump: the header, the params record and the EOF tag, no objects. Go's dump walks its own heap
    // arenas, spans and goroutine stacks; the managed model has none of them (the CLR owns the
    // heap), so the dump is truthful and empty rather than invented. The params record says what
    // this host can say: pointer byte order and size, no Go arena (0, 0), GOARCH, the Go release the
    // corpus was converted from, ncpu. TestSchedPauseMetrics asserts the pause accounting, not the
    // contents. runtime/debug's own WriteHeapDump forwards here (WriteHeapDumpManaged).
    internal static void runtime_debug_WriteHeapDump(uintptr fd)
    {
        worldStop stw = stopTheWorld(stwWriteHeapDump);
        writeMinimalHeapDump(fd);
        startTheWorld(stw);
    }

    // The public crossing runtime/debug's hand-owned WriteHeapDump calls (the readMetricsManaged
    // pattern: the //go:linkname push has no cross-assembly form).
    public static void WriteHeapDumpManaged(uintptr fd) => runtime_debug_WriteHeapDump(fd);

    private static void writeMinimalHeapDump(uintptr fd)
    {
        List<byte> dump = new(64);

        dump.AddRange("go1.7 heap dump\n"u8);
        uvarint((uint64)tagParams);
        uvarint(BitConverter.IsLittleEndian ? 0UL : 1UL);
        uvarint((uint64)global::go.@internal.goarch_package.PtrSize);
        uvarint(0);
        uvarint(0);
        str(global::go.@internal.goarch_package.GOARCH);
        str(buildVersion);
        uvarint((uint64)ncpu);
        uvarint((uint64)tagEOF);

        // THE WRITE IS MANAGED, on every target. It used to go through runtime.write, which is a
        // working syscall on linux and darwin but, on the windows flavor, write1 -> stdcall ->
        // asmcgocall: an assembly door with no body, so every windows WriteHeapDump threw
        // NotImplementedException (GolibTests' WriteHeapDump arm, TRAIN I battery). A non-owning
        // SafeFileHandle over the caller's fd -- a Windows HANDLE there, a descriptor elsewhere --
        // written through an unbuffered FileStream serves a file and a pipe alike, and closes
        // nothing: the fd stays the caller's.
        //
        // ONE STATED DIVERGENCE, unix only: for a SEEKABLE descriptor .NET writes positionally
        // (pwrite) from the descriptor's current offset and does not move that offset, where Go's
        // write(2) advances it. The dump lands where Go's would; only a later write through the same
        // descriptor would start at the old offset. Go's own callers stat or close the file after the
        // dump and never write again (runtime/debug's heapdump tests, TestSchedPauseMetrics).
        //
        // dwrite's contract: a write error is not reported, so a failed write ends the dump silently.
        try
        {
            using global::Microsoft.Win32.SafeHandles.SafeFileHandle handle = new((nint)fd, ownsHandle: false);
            using global::System.IO.FileStream stream = new(handle, global::System.IO.FileAccess.Write, bufferSize: 0);

            stream.Write(global::System.Runtime.InteropServices.CollectionsMarshal.AsSpan(dump));
        }
        catch (Exception ex) when (ex is global::System.IO.IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or ObjectDisposedException)
        {
        }

        // dumpint: Go's uvarint encoding.
        void uvarint(uint64 v)
        {
            for (; v >= 0x80; v >>= 7)
                dump.Add((byte)(v | 0x80));

            dump.Add((byte)v);
        }

        // dumpstr: the length, then the bytes.
        void str(@string s)
        {
            uvarint((uint64)len(s));

            for (nint i = 0; i < len(s); i++)
                dump.Add(s[i]);
        }
    }

    // ---- THE GC PACER'S KNOBS: GOGC and GOMEMLIMIT -------------------------------------------
    //
    // gcinit's pacer half (mgc.go): Go initializes gcController from the environment in schedinit,
    // after goenvs, and every reader of GOGC and GOMEMLIMIT -- runtime/debug's SetGCPercent and
    // SetMemoryLimit, the /gc/gogc:percent and /gc/gomemlimit:bytes metrics, the pacer's heap goal
    // -- reads gcController.gcPercent and memoryLimit from then on. The managed host never runs
    // schedinit, so both stayed 0.
    //
    // THE ORDER. goenvs_impl.cs's module initializer is schedinit's slot here, and it calls
    // gcinitController after it fills envs and runs parsedebugvars (schedinit's goenvs, parsedebugvars,
    // gcinit): Go's own order, since readGOGC and readGOMEMLIMIT read through gogetenv, which throws
    // "getenv before env init" on a nil environment. It sits outside parsedebugvars' try/catch because
    // its only failure is a Go fatal (the malformed-GOMEMLIMIT throw below). It is ONE
    // initializer, not a second one, because C# leaves the order between two module initializers
    // unspecified. It is eager, not lazy on first use, because Go's values exist before any Go code
    // runs: a reader that never touches a knob (a metrics exporter) still reads GOGC. A malformed
    // GOMEMLIMIT throws there, as Go's gcinit does at startup.
    internal static void gcinitController() => ᏑgcController.init(readGOGC(), readGOMEMLIMIT());

    // runtime/debug's setGCPercent and setMemoryLimit ARE runtime's (mgcpacer.go pushes them with
    // //go:linkname): the heap lock, the controller's setter, gcControllerCommit, and for a negative
    // percent gcWaitOnMark, which returns at once with no mark phase running. The push has no
    // cross-assembly form, so runtime/debug's hand-owned stubs call these public crossings (the
    // WriteHeapDumpManaged pattern) and runtime keeps the one value.
    public static int32 SetGCPercentManaged(int32 @in) => setGCPercent(@in);

    public static int64 SetMemoryLimitManaged(int64 @in) => setMemoryLimit(@in);

    // shrinkstack (stack.go) REFUSES BY NAME. Go's body copies a goroutine's stack into a smaller
    // one. A goroutine here is a CLR thread with no Go stack (g.stack.lo is 0), so the converted
    // body's first check threw "missing stack in shrinkstack", and a throw exits the process: the
    // runtime row lost every test after TestSystemstackFramePointerAdjust. Its other callers
    // (newstack, scanstack) are Go-scheduler and GC-mark paths the managed host does not run, and
    // would have thrown at the same check.
    internal static void shrinkstack(ж<g> Ꮡgp) =>
        throw new PanicException("runtime: shrinkstack: goroutines are CLR threads with no Go stack to shrink");

    // NumCgoCall returns the number of cgo calls made by the current process. Go's body walks the
    // scheduler's `allm` thread list summing per-m counters — a list the managed model never
    // populates (the walk nil-derefs where Go always has at least m0). The managed model makes no
    // cgo calls at all, so zero is the true count, not an approximation. Reached by the
    // /cgo/go-to-c-calls:calls metric's compute closure on every metrics.Read.
    public static int64 NumCgoCall()
    {
        return 0;
    }

    // NumGoroutine returns the number of goroutines that currently exist. Go's body (gcount) derives
    // that by subtraction over scheduler state — allglen, less sched.gFree.n, less sched.ngsys, less
    // each P's gFree.n — none of which the managed model populates, so every term was zero and
    // gcount's own `if n < 1 { n = 1 }` floor reported a constant 1 for every program ever run. The
    // managed model has the real count: golib's Goroutine registry maintains it as goroutines are
    // created and retired, including the main goroutine, which is what Go counts too.
    //
    // Go's staleness caveat carries over unchanged and for the same reason — "all these variables
    // can be changed concurrently, so the result can be inconsistent" — since a goroutine may be
    // created or may exit between the read and the caller's use of it. What does NOT carry over is
    // gcount's floor: the registry cannot report less than the caller's own goroutine, so there is
    // no nonsense to clamp.
    //
    // USER goroutines only, as Go's gcount subtracts sched.ngsys: the runtime's own goroutines (the
    // unique map-cleanup goroutine is the measured one) are registered like any other but are not
    // what a program means by "goroutines that currently exist". Go's floor returns with the
    // subtraction -- two reads that can be transiently inconsistent, exactly Go's caveat.
    public static nint NumGoroutine()
    {
        nint n = Goroutine.UserCount;

        return n < 1 ? 1 : n;
    }

    // gcount is the body NumGoroutine above used to call, and the one THREE other consumers still
    // reach directly: metrics.cs's /sched/goroutines compute closure, and mprof.cs's goroutine-profile
    // size and count. Hand-owning NumGoroutine alone left all three on the auto body's clamped
    // constant 1, so a program could report a true count through the public API and a fabricated one
    // through its own metrics and profiles in the same breath.
    //
    // Same registry, same answer, no second source of truth — which is why this is done here rather
    // than repeated at each call site.
    //
    // What does NOT carry over is the auto body's `if n < 1 { n = 1 }` floor. Go needs it because its
    // subtraction over concurrently-changing scheduler state can transiently go negative; the registry
    // cannot report fewer goroutines than the caller's own, so there is no nonsense to clamp and a
    // floor could only hide a real zero if one ever arose. The count's measured divergence — early by
    // one climbing, late to decay — is in this file's Honest-divergences ledger.
    internal static int32 gcount()
    {
        int n = Goroutine.UserCount;

        return n < 1 ? 1 : n;
    }

    // totalMutexWaitTimeNanos sums the mutex wait time observed by the runtime. Go's body loads
    // two global counters and then walks the same `allm` list as NumCgoCall for per-m
    // lock-profile wait times — the walk nil-derefs here, and the per-m profiles it would sum
    // never exist. The managed body keeps the two REAL counter loads and drops only the walk.
    // Reached by the /sync/mutex/wait/total:seconds metric's compute closure.
    internal static int64 totalMutexWaitTimeNanos()
    {
        var total = Ꮡsched.of(schedt.ᏑtotalMutexWaitTime).Load();

        total += Ꮡsched.of(schedt.ᏑtotalRuntimeLockWaitTime).Load();

        return total;
    }

    // consistentHeapStats.read takes a globally consistent snapshot of the heap-stats deltas. Go's
    // body disables preemption (acquirem → getg) to hold `allp` stable, then merges every P's
    // delta buffer under a generation rotation. The managed model has no Ps and nothing ever
    // writes a heapStatsDelta — the CLR allocator does not populate Go's allocator bookkeeping —
    // so the faithful snapshot is the ZERO delta: the same honest zero ReadMemStats reports for
    // the identical Mallocs/Frees/HeapObjects fields (see its comment above), never an invented
    // number. Reached from heapStatsAggregate.compute for every heap-dependent metric.
    internal static void read(this ж<consistentHeapStats> Ꮡm, ж<heapStatsDelta> Ꮡout)
    {
        Ꮡout.Value = new heapStatsDelta(nil);
    }

    // readMetricsManaged is the managed crossing for runtime/metrics.Read — the shim
    // runtime/metrics/sample.cs's hand-owned Read calls instead of the linkname-pushed
    // readMetrics (the registerPoolCleanup pattern above: a public shim where a cross-assembly
    // crossing cannot take its Go form). Go's crossing hands this package the RAW ADDRESS of the
    // caller's []Sample backing store and readMetricsLocked reconstructs a []metricSample over it
    // — an address-reinterpret no managed pointer can alias, so the reconstructed slice read
    // garbage. This shim carries the same data as plain managed values instead: names in;
    // computed (kind, scalar, pointer) out, index-aligned. The BATCH semantics of
    // readMetricsLocked are preserved exactly — one metricsLock hold, one defensive agg clear,
    // then per-sample ensure+compute in order — and everything of substance (initMetrics' table,
    // the compute closures, the stat aggregates) stays auto-converted.
    //
    // A HISTOGRAM crosses as its two slices, not as its address. Go's metricValue.pointer holds the
    // address of the runtime's own metricFloat64Histogram, and runtime/metrics' Value.Float64Histogram
    // casts it to *Float64Histogram: a different type with the same layout. The managed pointer model
    // refuses that reinterpret (arm 2a), and runtime cannot name runtime/metrics' type (metrics
    // references runtime, not the reverse), so the runtime hands over counts and buckets and
    // runtime/metrics/sample.cs builds its own Float64Histogram over the same two slices. Histograms
    // are the only kind that sets metricValue.pointer (float64HistOrInit is its one writer).
    //
    // ⚠ A STATED DIVERGENCE, deliberately not repaired here (⟨OQ-4⟩ of DESIGN-readmemstats-surface.md,
    // ratified 2026-08-21): runtime/metrics does NOT read the ReadMemStats surface. Its compute
    // closures stay auto-converted over Go's own memstats/gcController/consistentHeapStats — and
    // consistentHeapStats.read is hand-owned above to return the ZERO delta, because nothing ever
    // writes a heapStatsDelta. So after the pause recorder landed, go2cs answers "how many bytes did
    // this process return to the OS" with a real number on MemStats.HeapReleased and with 0 on
    // /memory/classes/heap/released:bytes, where Go documents the two as the same quantity;
    // /gc/pauses:seconds and /gc/cycles/total:gc-cycles are likewise unwired. Rewiring them converts
    // auto-converted closures into hand-owns on a banked package for no consuming test — the banked
    // runtime/metrics row is TestNames + TestDocs, which asserts no VALUE — so the divergence is
    // recorded rather than papered over, and the wiring waits for a consumer that demands it.
    public static void readMetricsManaged(slice<@string> names, slice<nint> kinds, slice<uint64> scalars, slice<slice<uint64>> histCounts, slice<slice<float64>> histBuckets)
    {
        metricsLock();

        // Ensure the map is initialized.
        initMetrics();

        // Account any GC cycle completed since the last reading (catchUpCPUStats above).
        catchUpCPUStats();

        readMetricsBatchLocked(names, kinds, scalars, histCounts, histBuckets);

        metricsUnlock();
    }

    // readMetricsLocked's batch, over plain managed values: one defensive agg clear, then
    // per-sample ensure+compute in order. metricsLock must be held and initMetrics called.
    private static void readMetricsBatchLocked(slice<@string> names, slice<nint> kinds, slice<uint64> scalars, slice<slice<uint64>> histCounts, slice<slice<float64>> histBuckets)
    {
        // Clear agg defensively.
        agg = new statAggregate(nil);

        for (nint i = 0; i < len(names); i++)
        {
            ref var data = ref heap<metricData>(out var Ꮡdata);
            (data, var ok) = metrics[names[i], ꟷ];

            if (!ok)
            {
                kinds[i] = (nint)metricKindBad;
                continue;
            }

            // Ensure we have all the stats we need. agg is populated lazily.
            Ꮡagg.ensure(Ꮡdata.of(metricData.Ꮡdeps));

            // Compute the value based on the stats we have.
            ref var value = ref heap<metricValue>(out var Ꮡvalue);
            value = new metricValue(nil);
            data.compute(Ꮡagg, Ꮡvalue);

            kinds[i] = (nint)value.kind;
            scalars[i] = value.scalar;

            if (value.kind == metricKindFloat64Histogram)
            {
                ж<metricFloat64Histogram> hist = (ж<metricFloat64Histogram>)(uintptr)value.pointer;
                histCounts[i] = hist.Value.counts;
                histBuckets[i] = hist.Value.buckets;
            }
        }
    }

    // readMetricsLocked (metrics.go), the REVERSED crossing (ruling 2026-09-28 02:10, Q2). Its one
    // caller here is ReadMetricsSlow (export_test.go, TestReadMetrics), which hands it the RAW
    // ADDRESS of the caller's []runtime/metrics.Sample backing store, a type this package cannot
    // name (runtime/metrics references runtime, not the reverse); Go reinterprets that address as a
    // []metricSample, which the managed pointer model refuses (arm 2a), and that refusal threw while
    // metricsSema was held. runtime/metrics registers MetricSamplesCrossing at its module
    // initialization: given the address and the length, it reads the samples' names, runs this
    // package's batch, and writes the values back into the caller's samples, exactly as its Read
    // does over readMetricsManaged. A caller that holds such an address has loaded runtime/metrics.
    public delegate void ReadMetricsBatch(slice<@string> names, slice<nint> kinds, slice<uint64> scalars, slice<slice<uint64>> histCounts, slice<slice<float64>> histBuckets);

    public static Action<@unsafe.Pointer, nint, ReadMetricsBatch>? MetricSamplesCrossing { get; set; }

    internal static void readMetricsLocked(@unsafe.Pointer samplesp, nint n, nint c)
    {
        Action<@unsafe.Pointer, nint, ReadMetricsBatch> crossing = MetricSamplesCrossing ??
            throw new PanicException("runtime: readMetricsLocked: the samples' address names runtime/metrics.Sample values, and runtime/metrics has not registered its crossing");

        crossing(samplesp, n, readMetricsBatchLocked);
    }

    // Goexit terminates the goroutine that calls it. No other goroutine is affected. Goexit runs
    // all deferred calls before terminating the goroutine. Because Goexit is not a panic, any
    // recover calls in those deferred functions will return nil.
    public static void Goexit()
    {
        // Calling Goexit from the MAIN goroutine terminates main without returning while the
        // program keeps running its other goroutines — and crashes with "no goroutines" once they
        // all exit. That needs a live-goroutine registry and a main-thread parking protocol the
        // managed model does not have yet (DESIGN-goexit.md option A), so the main-goroutine case
        // stays GATED rather than silently doing something else: ending the process here would
        // kill goroutines Go would keep running. The gate is honest and loud, never a no-op.
        if (!Goroutine.OnGoroutine)
        {
            throw new NotSupportedException(
                "runtime.Goexit from the main goroutine is not supported: main-goroutine Goexit " +
                "must leave the other goroutines running (see docs/phase4/DESIGN-goexit.md). " +
                "Goexit from a goroutine is fully supported.");
        }

        // The goroutine case: unwind. GoFunc's finally-based defer machinery runs this goroutine's
        // deferred calls on the way out, recover() cannot observe the unwind (GoexitException is
        // deliberately not a PanicException), and the goroutine root swallows it — Go's three
        // documented Goexit properties, each falling out of machinery that already existed.
        throw new GoexitException();
    }

    // GOTRACEBACK's system/crash levels, read once as Go's setTraceback reads the variable at
    // startup: at level 2 and above the runtime's own goroutines join the traceback.
    private static readonly bool s_tracebackShowsSystem =
        Environment.GetEnvironmentVariable("GOTRACEBACK") is "system" or "crash";

    // The one line go2cs prints where Go prints another goroutine's frames.
    //
    // Deliberately NOT a frame: no tab-indented position line beneath it, nothing that could be
    // mistaken for `<pkg>.<Func>()` by a program grepping a traceback, and no package-qualified name
    // anywhere in it. Fabricating frames for a goroutine whose stack was never walked would be the
    // one thing a traceback must never do — the whole surface exists to be read literally.
    //
    // Stage B of docs/phase4/DESIGN-cooperative-scheduler.md replaces it with the parking
    // goroutine's OWN stack, captured at park time and symbolized through the synthetic-PC registry;
    // until that registry exists, the honest answer is this sentence.
    private const string ForeignStackPlaceholder =
        "[stack unavailable: go2cs does not capture another goroutine's frames]\n";

    // Stack formats a stack trace of the calling goroutine into buf and returns the number of
    // bytes written to buf.
    //
    // NoInlining, for the reason the Callers family three hundred lines below already states: the
    // CLR's StackTrace does not report inlined frames. Until 2026-09-04 this method was the ONE
    // traceback entry point without the attribute, and it walked with `skipFrames: 1` — a COUNT that
    // assumes this method owns frame 0. When the JIT inlined it into its caller, frame 0 was the
    // caller and the skip removed THAT, so the rendered block began one frame deep. Measured on the
    // banked net/http row at its configuration of record (Release, tiered on; two preserved records,
    // trains 20 and 22): the main goroutine's block began at `net/http_test.goroutineLeaked()`, the
    // `interestingGoroutines()` frame above it missing — which is the one frame Go's own
    // goroutine-leak filter (main_test.go's interestingGoroutines: keep-unless-contains over the
    // whole block) reads to drop the main goroutine, so the host counted itself as a leaked
    // goroutine and TestMain exited 1 while the comparison record read 1,345/1,345 both sides.
    //
    // Two halves, because the attribute alone would leave the count: the attribute guarantees this
    // method HAS a frame, and callerFrames locates that frame by IDENTITY (the frame whose method is
    // this one) and starts the walk one above it — so the boundary is where this method's frame is,
    // never where a count assumed it to be.
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static nint Stack(slice<byte> buf, bool all)
    {
        // GOTRACEBACK decides the system-goroutine axis here; the FATAL path decides it from Go's
        // throwType as well (fatalTraceback), which is why renderStack takes the answer rather than
        // reading the variable itself.
        // Go stops the world to walk every goroutine (stwAllGoroutinesStack), and only then.
        worldStop stw = all ? stopTheWorld(stwAllGoroutinesStack) : default;
        string rendered = renderStack(callerFrames(s_stackMethodHandle), all, s_tracebackShowsSystem);

        if (all)
            startTheWorld(stw);
        byte[] encoded = Encoding.UTF8.GetBytes(rendered);
        nint count = Math.Min((nint)encoded.Length, len(buf));

        for (nint i = 0; i < count; i++)
            buf[i] = encoded[i];

        return count;
    }

    // The goroutine BLOCKS of a traceback, shared by runtime.Stack and by the fatal report: the
    // calling goroutine's header and frames, the in-flight panic's snapshot beneath them, and under
    // `all` one block per other live goroutine. Extracted rather than duplicated for the reason the
    // semaphore hoist records one package over — two branches of one rule drift apart, and a
    // traceback that renders one way for a panic and another way for a fatal is a divergence nobody
    // would find until an operator read both.
    private static string renderStack(IEnumerable<StackFrame> frames, bool all, bool showSystem)
    {
        StringBuilder trace = new();

        // The CALLING goroutine, always first and always with real frames — Go dumps the current
        // goroutine ahead of the rest, and this is the one stack the CLR can walk.
        Goroutine? current = Goroutine.Current;

        appendGoroutineHeader(trace, current);
        appendGoFrames(trace, frames);

        // Go keeps a panicking goroutine's frames on the stack until the panic completes, so a
        // debug.Stack() taken inside a deferred function shows the PANIC SITE. A CLR exception has
        // already unwound those frames before the finally-based defer runs, so the frames Go would
        // still be showing are appended from the in-flight panic's snapshot — below the live frames,
        // which is where Go's traceback puts them too (the deferred call runs on top of gopanic).
        PanicException? inFlight = GoFuncRoot.InFlightPanic;

        if (inFlight?.PanicTrace is StackTrace panicSite && panicSite.FrameCount > 0)
        {
            // `Message`, not `State.ToString()`: a panic value renders ONE way in this runtime, and
            // that way is Go's preprintpanics rule (an error prints its Error(), a Stringer its
            // String()). Reading the state directly here printed an address for every error panic.
            trace.Append("panic: ").Append(inFlight.Message).Append('\n');
            appendGoFrames(trace, panicSite);
        }

        if (all)
        {
            // Every OTHER live goroutine, in goid order, as Go dumps them: one blank-line-separated
            // block each, carrying a real header — the id the registry minted and the wait reason
            // the park accounting recorded — and the honest placeholder in place of frames.
            //
            // The HEADER is the half this runtime can now answer truthfully, and it is the half Go's
            // own consumers read: `runtime/pprof`'s awaitBlockedGoroutine matches on the bracketed
            // word, and `runtime`'s TestNumGoroutine counts "goroutine " occurrences against
            // NumGoroutine(). Both were unanswerable while this printed one literal header.
            foreach (Goroutine goroutine in Goroutine.Snapshot())
            {
                if (ReferenceEquals(goroutine, current))
                    continue;

                // Go's tracebackothers skips isSystemGoroutine(gp, false) unless the traceback level
                // is at least 2 (GOTRACEBACK=system or crash): the runtime's own goroutines are not
                // part of a program's traceback, which is why no leak filter over runtime.Stack ever
                // has to name them -- net/http's counted the unique map-cleanup goroutine as a leak
                // for as long as this runtime rendered it (2026-09-04).
                if (goroutine.IsSystem && !showSystem)
                    continue;

                trace.Append('\n');
                appendGoroutineHeader(trace, goroutine);
                trace.Append(ForeignStackPlaceholder);
                appendCreatedBy(trace, goroutine);
            }
        }

        return trace.ToString();
    }

    // Go's printcreatedby1 (runtime/traceback.go): `created by <func> in goroutine <parentGoid>` --
    // the one line of a foreign goroutine's block this runtime CAN state truthfully, because the
    // registry records the creator at launch (Goroutine.Creator). It is what a leak filter reads:
    // net/http's interestingGoroutines drops blocks by `created by testing.RunTests`, `created by
    // runtime.gc`, and an operator reading a dump learns which `go` statement left the goroutine
    // behind. Go's position line beneath it is deliberately omitted: it would cost a file-info
    // capture on every `go` statement, and nothing matches on it. Absent for the main goroutine
    // and for a host-entered thread, exactly as Go prints none for goroutine 1.
    private static void appendCreatedBy(StringBuilder trace, Goroutine goroutine)
    {
        if (goroutine.Creator is not System.Reflection.MethodBase creator)
            return;

        trace.Append("created by ").Append(goFrameName(creator, null));

        if (goroutine.ParentId != 0)
            trace.Append(" in goroutine ").Append(goroutine.ParentId);

        trace.Append('\n');
    }

    // Go's goroutineheader (runtime/traceback.go): `goroutine <goid> [<status>]:`, where the status
    // word is the g's status EXCEPT while it is _Gwaiting, when Go substitutes gp.waitreason.String().
    // That substitution is the whole shape here — a parked goroutine prints its reason, everything
    // else prints `running`.
    //
    // Three of Go's header decorations are omitted rather than invented: ` (scan)` (no GC of ours to
    // be scanned by), `, N minutes` (nothing records when a park began), and `, locked to thread`
    // (LockOSThread is a no-op here because every goroutine already owns its thread, so the note
    // would be true of every goroutine and therefore say nothing).
    //
    // A goroutine with no identity — Stack called on a host thread that never ran Go code — has no
    // goid to print, and Go has no such state at all. It reports id 0, which is not a goid Go's
    // monotonic allocator ever mints, rather than borrowing another goroutine's number.
    private static void appendGoroutineHeader(StringBuilder trace, Goroutine? goroutine)
    {
        long id = goroutine is null ? 0 : goroutine.Id;

        // "running" covers Go's _Grunnable too, and cannot be split from it: Go distinguishes a
        // goroutine QUEUED on a P from one executing, and under the CLR a thread waiting for a core
        // is indistinguishable from one running on it. Stated rather than approximated.
        string status = goroutine is { State: GoroutineState.Parked }
            ? WaitReasons.Text(goroutine.Reason)
            : "running";

        trace.Append("goroutine ").Append(id).Append(" [").Append(status).Append("]:\n");
    }

    // Renders frames the way Go's traceback does — `<pkg>.<Func>()` on one line, a tab-indented
    // `<file>:<line>` beneath it — rather than the CLR's `at <Namespace>.<Type>.<Method>(...) in
    // <file>:line <n>`. This is observable output: Go programs (and Go's own tests) grep a traceback
    // for `<pkg>.<Func>`, which the CLR form never contains because a converted package's frames
    // live on a `<pkg>_package` class inside namespace `go`.
    // The calling goroutine's frames ABOVE the anchor method itself, located by identity. Every
    // anchor is NoInlining, so its frame is always present; if the search ever fails anyway, the
    // fallback keeps the OLD count-based boundary (skip frame 0) rather than rendering the anchor's
    // own frame — a frame too many is the shape Go's readers tolerate, a frame too few is the shape
    // that broke net/http.
    //
    // The anchor is a PARAMETER rather than this file's one Stack handle because there are now two
    // entry points into the walk — Stack, and the fatal path's renderer — and each owns a different
    // boundary. Hard-coding Stack's handle here would silently give the second entry point the
    // fallback branch, i.e. a count, which is exactly the failure the identity boundary replaced.
    private static readonly RuntimeMethodHandle s_stackMethodHandle =
        typeof(runtime_package).GetMethod(nameof(Stack), [typeof(slice<byte>), typeof(bool)])!.MethodHandle;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IEnumerable<StackFrame> callerFrames(RuntimeMethodHandle anchor)
    {
        StackFrame[] frames = new StackTrace(skipFrames: 0, fNeedFileInfo: true).GetFrames();
        int boundary = -1;

        for (int i = 0; i < frames.Length; i++)
        {
            System.Reflection.MethodBase? method = frames[i].GetMethod();

            if (method is not null && method.MethodHandle == anchor)
            {
                boundary = i;
                break;
            }
        }

        // boundary == -1 cannot happen while the anchor is NoInlining (its frame is on this stack,
        // one above callerFrames' own); the fallback skips exactly the frames a count of 1 used to.
        int first = boundary >= 0 ? boundary + 1 : 1;

        for (int i = first; i < frames.Length; i++)
            yield return frames[i];
    }

    private static void appendGoFrames(StringBuilder trace, StackTrace stack) =>
        appendGoFrames(trace, stack.GetFrames());

    // The frames printed are the frames runtime.Callers counts (captureCallers' rule): Go frames, and
    // a GoStackRoot host method as the Go frame it stands in for. Everything else on the CLR stack —
    // golib, the BCL, the test host's own frames — has no Go frame, and printing it broke Go's own
    // traceback readers: runtime's parseTraceback requires a tab-indented source line under every
    // function line, and `System.Threading.ExecutionContext.RunInternal()` has none. runtime.goexit
    // is not printed: Go's showframe hides runtime frames, and the walk's goexit tail is Callers-only.
    private static void appendGoFrames(StringBuilder trace, IEnumerable<StackFrame> frames)
    {
        List<(string name, string file, int line)> printed = [];
        bool calledGo = false;

        foreach (StackFrame frame in frames)
        {
            System.Reflection.MethodBase? method = frame.GetMethod();

            // A [StackTraceHidden] linkname forwarder has no Go frame (isGoSourceFrame skips it for
            // Callers by the same predicate), and StackTrace.GetFrames() still returns it.
            if (method is null || method.IsDefined(typeof(System.Diagnostics.StackTraceHiddenAttribute), inherit: false))
                continue;

            // A method-expression WRAPPER (GoWrapperAttribute) prints only where Go's traceback keeps
            // one: when it is the first Go frame of the block, i.e. the wrapper itself faulted (a nil
            // receiver: Go's sigpanic or panicwrap). A wrapper that called a Go function is elided, as
            // Go's elideWrapperCalling rules and as the Callers walk does.
            if (method.IsDefined(typeof(GoWrapperAttribute), inherit: false))
            {
                if (calledGo)
                    continue;
            }
            else if (isGoSourceFrame(method))
            {
                calledGo = true;
            }
            else
            {
                if (stackRootOf(method) is GoStackRootAttribute root)
                    printed.Add((root.Function, root.File, root.Line));

                continue;
            }

            (string file, int line) = goFramePosition(method, frame);
            printed.Add((goFrameName(method, frame), file, line));
        }

        // Go's elision (traceback1): a stack deeper than tracebackInnerFrames + tracebackOuterFrames
        // prints its innermost and outermost frames and one line counting the ones between.
        int inner = (int)tracebackInnerFrames, outer = (int)tracebackOuterFrames;
        int elided = printed.Count - inner - outer;

        for (int i = 0; i < printed.Count; i++)
        {
            if (elided > 0 && i == inner)
            {
                trace.Append("...").Append(elided).Append(" frames elided...\n");
                i += elided - 1;
                continue;
            }

            (string name, string file, int line) = printed[i];
            trace.Append(name).Append("()\n");

            if (!string.IsNullOrEmpty(file))
                trace.Append('\t').Append(file).Append(':').Append(line).Append('\n');
        }
    }

    // `go.sync_test_package.onceFuncPanic` -> `sync_test.onceFuncPanic`;
    // `go.runtime.debug_package.Stack`     -> `runtime/debug.Stack` (Go names a package by its
    //                                         import path, and the namespace mirrors it);
    // `go.log.slog_internal_test_package.TestCallDepth` -> `log/slog.TestCallDepth` (an internal
    //                                         test file is compiled INTO the package under test —
    //                                         see the suffix rule below);
    // a closure's compiler-generated `<Outer>b__N` on a nested display class -> `<pkg>.Outer.funcN`,
    // Go's own spelling for a function literal — the counter suffix (`1`, `2.1`) read from the
    // file's recorded GoPositionMap funcLits map when the conversion recorded one (see
    // goFuncLiteralSuffix), and derived from the compiler-generated name only as the fallback.
    // A frame that is not converted Go code (golib, the
    // BCL, the test host) keeps its .NET name — inventing a Go name for it would be a lie.
    private static string goFrameName(System.Reflection.MethodBase method, StackFrame? frame)
    {
        Type? declaring = method.DeclaringType;

        if (declaring is null)
            return method.Name;

        string typeName = declaring.FullName ?? declaring.Name;

        // LastIndexOf, not IndexOf: a Go package whose own name ends in `_package` would produce
        // `go.x_package_package`, and the FIRST match would truncate the import path.
        int packageSuffix = typeName.LastIndexOf("_package", StringComparison.Ordinal);

        if (!typeName.StartsWith("go.", StringComparison.Ordinal) || packageSuffix < 0)
            return $"{typeName}.{method.Name}";

        // "go.runtime.debug_package" -> "runtime/debug"
        string importPath = typeName[3..packageSuffix].Replace('.', '/');

        // ...unless the package class carries its VERBATIM import path (GoPackageAttribute.ImportPath),
        // which the converter stamps exactly where the line above cannot reproduce it: a '.' inside a
        // segment (`example.com/x`), a major-version directory, a name that differs from its directory.
        // The stamp is final, test variants included (each is stamped with the path Go gives it), so
        // the internal-test suffix rule below does not apply to it. The frame may be a closure class
        // nested in the package class, so the class is found by walking out to the `_package` level.
        Type? packageClass = declaring;

        while (packageClass is not null && !packageClass.Name.EndsWith("_package", StringComparison.Ordinal))
            packageClass = packageClass.DeclaringType;

        bool stamped = go.GoReflect.GoPackageImportPathOf(packageClass) is { } verbatim && (importPath = verbatim) is not null;

        // An INTERNAL test file (`package slog`, in logger_test.go) is compiled INTO the package
        // under test, so Go names its frames with that package's own import path and NO suffix at
        // all. The `-tests` pipeline cannot compile it into the production class — it emits a
        // separate `<pkg>_internal_test_package` (testConversion.go's `production.Name +
        // "_internal_test" + PackageSuffix`) — and that token is a go2cs emission detail rather
        // than anything Go ever spells, so it is stripped back off here.
        //
        // An EXTERNAL test file (`package slog_test`) is a genuinely separate Go package and Go
        // KEEPS its `_test`. The two suffixes are therefore deliberately NOT symmetric, and the
        // tempting generalization — strip any trailing `_test` — is wrong in a way a banked row
        // already measures: runtime/debug's own TestStack greps a rendered traceback for
        // `runtime/debug_test.(*T).ptrmethod`.
        //
        // Measured against the go1.23.12 toolchain rather than reasoned about:
        //     internal (`package callerprobe`)      -> callerprobe.TestInternalCallerName
        //     external (`package callerprobe_test`) -> callerprobe_test.TestExternalCallerName
        //
        // DESIGN-position-map.md §8 records that the two suffix rules retire from the FILE half
        // (which is RECORDED, so there is nothing to derive) but "remain necessary and unchanged
        // for the FUNCTION half" — this is that rule, which the file-half arc never actually
        // landed here. Without it log/slog's logger_test.go reads
        // `log/slog_internal_test.TestCallDepth` where it asserts `log/slog.TestCallDepth`, and the
        // leak is systemic rather than slog-specific: every package whose suite inspects caller
        // info shows it. Guarded by GolibTests/CallerFrameTestVariantNamingTests, which pins all
        // three shapes so neither rule can be "fixed" into the other.
        //
        // Known edge, stated rather than guarded: a Go package genuinely NAMED `<x>_internal_test`
        // would emit the same class name and be stripped wrongly. It is not resolvable at this
        // layer — the emission spells both cases identically — and the durable fix would be for the
        // converter to RECORD the package identity the way the file half is recorded (§11.1). No
        // such package exists in GOROOT, and the shape is pathological in user code, so the
        // derivation stands as the design ruled it (§8: "goImportPath stays").
        const string internalTestSuffix = "_internal_test";

        if (!stamped &&
            importPath.Length > internalTestSuffix.Length &&
            importPath.EndsWith(internalTestSuffix, StringComparison.Ordinal))
        {
            importPath = importPath[..^internalTestSuffix.Length];
        }

        // A method-expression WRAPPER (GoWrapperAttribute) is named as Go names its autogenerated
        // wrapper: the expression, in the package that declares the receiver type.
        if (method.GetCustomAttributes(typeof(GoWrapperAttribute), inherit: false) is [GoWrapperAttribute wrapper])
            return $"{wrapper.PackagePath ?? importPath}.{wrapper.GoName}";

        string name = method.Name;

        // An sstring twin's canonical value delegate is a lambda carrying the Go name of the function
        // it stands for (GoTwinForwarderAttribute), so a func VALUE's *Func names that function
        // rather than a `.cctor` literal. Its declaring type is the closure class nested in the
        // package class, which the `_package` search above already reads through.
        if (method.GetCustomAttributes(typeof(GoTwinForwarderAttribute), inherit: false) is [GoTwinForwarderAttribute { GoName: { } goName }])
            name = goName;

        // A function literal is emitted as a compiler-generated method — `<Outer>b__X_Y` for a
        // lambda on a `<>c__DisplayClassX_Y` nested in the package class, `<Outer>g__name|X_Y`
        // for a local function; Go renders the same literal as `Outer.funcN` (a nested one as
        // `Outer.funcN.M`). The counter is Go's per-enclosing-function, source-order counter
        // starting at 1, which the conversion RECORDS in the file's GoPositionMap funcLits map:
        // the frame's C# line maps through the record's line table to its Go line, and the
        // innermost recorded literal span containing that line names the frame. Roslyn's own
        // X_Y numbering is a closure-GROUP index plus a per-group index that matches Go's
        // counter only by coincidence (measured: two sibling literals answered func0/func1 for
        // Go's func1/func2, and a nested literal cannot be represented at all), so the derived
        // ordinal below is kept ONLY as the fallback for frames no conversion recorded — an
        // older artifact, a hand-written lambda, a frame with no PDB, or a literal outside a
        // function declaration (package-level initializers keep Go's package-global `glob..`
        // counter, which is a compile-schedule fact no per-file record can carry).
        if (name.Length > 0 && name[0] == '<')
        {
            int close = name.IndexOf('>');

            if (close > 1)
            {
                string outer = name[1..close];
                string? recorded = frame is null ? goFuncLiteralSuffix(method) : goFuncLiteralSuffix(method, frame);

                if (recorded is not null)
                {
                    name = $"{outer}.func{recorded}";
                }
                else
                {
                    int lastUnderscore = name.LastIndexOf('_');
                    string ordinal = lastUnderscore >= 0 && lastUnderscore + 1 < name.Length ? name[(lastUnderscore + 1)..] : "1";
                    name = $"{outer}.func{ordinal}";
                }
            }
        }

        // Go's traceback names a METHOD frame with its receiver TYPE between the package and the
        // method, which the flat `<pkg>.<name>` form drops.
        if (goReceiverName(method) is string receiver)
            name = $"{receiver}.{name}";

        return $"{importPath}.{name}";
    }

    // Spells a function-literal frame's recorded Go counter suffix (`1`, `2.1`), or null when the
    // conversion recorded none — no PDB path, no record for the file, no Go line for the frame's
    // C# line, or no recorded literal span containing that Go line. The caller falls back to the
    // Roslyn-derived ordinal in every null case, so an unrecorded frame answers exactly what it
    // always did. Both facts consumed here — the Go line and the literal spans — come from the ONE
    // GoPositionMap record, the same indivisibility the position half rests on.
    //
    // Known edge, stated rather than guarded: line granularity cannot split two literals sharing
    // one Go line, nor a frame in an OUTER literal sitting on the very line a nested literal
    // starts — the innermost span wins the tie, which favors the far more common frame (a body
    // line of the nested literal) over the rarer one (the outer literal mid-call on that line).
    private static string? goFuncLiteralSuffix(System.Reflection.MethodBase method, StackFrame frame)
    {
        string csPath = goSourcePath(frame.GetFileName());

        if (csPath.Length == 0)
            return null;

        GoPositionMapRecord? record = goPositionMapRecord(method, csPath);

        if (record is null)
            return null;

        int goLine = record.GoLineFor(frame.GetFileLineNumber());

        if (goLine <= 0)
            return null;

        return record.FuncLiteralFor(goLine);
    }

    // The same suffix for a method with NO live frame: a synthetic PC (a CPU profile's sampled method,
    // a goroutine's start PC) names a function, never an instruction, so the C# line the record is
    // read against is the method's FIRST sequence point in its portable PDB, which lies inside the
    // literal's own span exactly as any frame in its body does. The PDB is the source a StackFrame
    // reads too; where none is found (no PDB beside the assembly and none embedded) the caller keeps
    // the derived fallback, the same answer a frame with no file information gets.
    private static string? goFuncLiteralSuffix(System.Reflection.MethodBase method)
    {
        (string? csFile, int csLine) = methodSourcePosition(method);

        if (csFile is null || csLine <= 0)
            return null;

        GoPositionMapRecord? record = goPositionMapRecord(method, goSourcePath(csFile));

        if (record is null)
            return null;

        int goLine = record.GoLineFor(csLine);

        return goLine <= 0 ? null : record.FuncLiteralFor(goLine);
    }

    private static readonly object s_pdbLock = new();
    private static readonly Dictionary<System.Reflection.Assembly, System.Reflection.Metadata.MetadataReaderProvider?> s_pdbs = new();

    // A method's first non-hidden sequence point (document name, line), or (null, 0).
    private static (string? file, int line) methodSourcePosition(System.Reflection.MethodBase method)
    {
        System.Reflection.Assembly assembly = method.Module.Assembly;
        System.Reflection.Metadata.MetadataReaderProvider? provider;

        lock (s_pdbLock)
        {
            if (!s_pdbs.TryGetValue(assembly, out provider))
            {
                provider = openPortablePdb(assembly);
                s_pdbs[assembly] = provider;
            }
        }

        if (provider is null)
            return (null, 0);

        try
        {
            lock (s_pdbLock)
            {
                System.Reflection.Metadata.MetadataReader pdb = provider.GetMetadataReader();
                var definition = System.Reflection.Metadata.Ecma335.MetadataTokens.MethodDefinitionHandle(method.MetadataToken);
                System.Reflection.Metadata.MethodDebugInformation information = pdb.GetMethodDebugInformation(definition.ToDebugInformationHandle());

                foreach (System.Reflection.Metadata.SequencePoint point in information.GetSequencePoints())
                {
                    if (point.IsHidden)
                        continue;

                    return (pdb.GetString(pdb.GetDocument(point.Document).Name), point.StartLine);
                }
            }
        }
        catch (Exception)
        {
            // An unreadable PDB names no position.
        }

        return (null, 0);
    }

    // The assembly's portable PDB: embedded in the image, or `<name>.pdb` beside the assembly, or
    // beside the application for a single-file host, whose bundled assemblies have no location.
    private static System.Reflection.Metadata.MetadataReaderProvider? openPortablePdb(System.Reflection.Assembly assembly)
    {
        try
        {
            #pragma warning disable IL3000 // Location is empty for a bundled assembly, which the fallback below covers.
            string location = assembly.Location;
            #pragma warning restore IL3000

            if (location.Length > 0 && System.IO.File.Exists(location))
            {
                using var pe = new System.Reflection.PortableExecutable.PEReader(System.IO.File.OpenRead(location));

                foreach (System.Reflection.PortableExecutable.DebugDirectoryEntry entry in pe.ReadDebugDirectory())
                {
                    if (entry.Type == System.Reflection.PortableExecutable.DebugDirectoryEntryType.EmbeddedPortablePdb)
                        return pe.ReadEmbeddedPortablePdbDebugDirectoryData(entry);
                }

                if (pe.TryOpenAssociatedPortablePdb(location, path => System.IO.File.Exists(path) ? System.IO.File.OpenRead(path) : null, out System.Reflection.Metadata.MetadataReaderProvider? associated, out _))
                    return associated;
            }

            string? name = assembly.GetName().Name;

            if (name is not null)
            {
                string beside = System.IO.Path.Combine(AppContext.BaseDirectory, name + ".pdb");

                if (System.IO.File.Exists(beside))
                    return System.Reflection.Metadata.MetadataReaderProvider.FromPortablePdbStream(System.IO.File.OpenRead(beside));
            }
        }
        catch (Exception)
        {
            // No readable PDB: the derived fallback names the literal.
        }

        return null;
    }

    // Spells the receiver qualifier of a converted Go method frame, or null when the frame is not a
    // method. Measured against a Go control on this box: a pointer receiver renders
    // `main.(*T).ptrmethod`, a value receiver `main.T.method`, and a generic receiver
    // `main.G[...].gmethod` — Go prints the LITERAL `[...]`, never the instantiated argument.
    //
    // Observable, not cosmetic, for the same reason the path separator is: runtime/debug's own
    // TestStack greps a rendered traceback for `runtime/debug_test.(*T).ptrmethod`, and the flat
    // form answers `runtime/debug_test.ptrmethod`.
    //
    // A converted Go method is a C# EXTENSION method on the package class whose FIRST parameter is
    // the receiver — `this ref T` for a pointer receiver (the [GoRecv] form), `this T` for a value
    // one, and RecvGenerator's boxed `this ж<T>` overload for the pointer form reached through a
    // pointer value. That `this` is the whole discriminator: a package-level Go func is a plain
    // static method and keeps its bare `<pkg>.<name>`, exactly as Go renders one.
    private static string? goReceiverName(System.Reflection.MethodBase method)
    {
        if (!method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), inherit: false))
            return null;

        System.Reflection.ParameterInfo[] parameters = method.GetParameters();

        if (parameters.Length == 0)
            return null;

        Type receiver = parameters[0].ParameterType;
        bool pointer = false;

        if (receiver.IsByRef)
        {
            // `this ref T` — Go's pointer receiver, lowered to a by-reference parameter.
            pointer = true;
            receiver = receiver.GetElementType() ?? receiver;
        }
        else if (pointerReferent(receiver) is Type referent)
        {
            // `this ж<T>` — the same Go pointer receiver reached through a heap box. IPointer<T>
            // rather than ж<T> itself so a generated named-pointer wrapper answers the same way.
            pointer = true;
            receiver = referent;
        }

        string name = receiver.Name;

        // A generic type's CLR name carries its arity after a backtick (G`1); Go writes G[...].
        int arity = name.IndexOf('`');

        if (arity >= 0)
            name = string.Concat(name.AsSpan(0, arity), "[...]");

        return pointer ? $"(*{name})" : name;
    }

    // The T of an IPointer<T>, or null when the type is not a pointer box.
    private static Type? pointerReferent(Type type)
    {
        if (!type.IsGenericType)
            return null;

        foreach (Type contract in type.GetInterfaces())
        {
            if (contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IPointer<>))
                return contract.GetGenericArguments()[0];
        }

        return null;
    }

    // Spells a frame's source path the way Go spells one. Go records source paths with FORWARD
    // slashes on every platform — on Windows `runtime.Caller` answers
    // `C:/Program Files/Go/src/runtime/proc.go`, never the host's native separator — while the
    // CLR hands back whatever the PDB holds, which on Windows is backslash-separated. The
    // difference is observable, not cosmetic: a Go program can read the string, and Go's own
    // suites match patterns against it (`flag`'s TestDefineAfterSet asserts
    // `.*/flag_test.go:.*`), so reporting the host separator diverges from Go on a value the
    // program under test inspects. Converted `path/filepath` accepts either separator on
    // Windows, exactly as Go's does, so normalizing costs no consumer anything.
    private static string goSourcePath(string? file)
    {
        return string.IsNullOrEmpty(file) ? string.Empty : file.Replace('\\', '/');
    }

    // ---------------------------------------------------------------------------------------------
    // The POSITION MAP: a frame's GO position, when the conversion recorded one.
    //
    // The converter emits one [assembly: GoPositionMap] record per converted file, carrying that
    // file's Go identity AND its C#-line to Go-line table. Both halves come from the one record,
    // which is what makes the pair INDIVISIBLE (coordinator ruling, 2026-08-21): a Go file paired
    // with a C# line is a position in NEITHER tree, so a frame either has a record and reports a Go
    // position that exists, or has none and reports the honest converted .cs position it always did.
    // Nothing here composes a file from one source and a line from another.
    //
    // A frame with no record is not a failure and not a gap to be filled in: golib, the BCL and the
    // hand-owned test host are not converted Go code, and a whole-file hand-own is C# that was
    // WRITTEN rather than converted, so no line of it corresponds to a line of Go. Each keeps its
    // .cs position for exactly the reason goFrameName keeps its .NET name for them.
    // ---------------------------------------------------------------------------------------------

    private static readonly object s_positionMapLock = new();
    private static readonly Dictionary<System.Reflection.Assembly, Dictionary<string, GoPositionMapRecord>> s_positionMaps = new();

    // goFramePosition spells one frame's source position: the Go one the conversion recorded, or the
    // converted C# one when it recorded none. The single funnel both consumers read, so a traceback
    // and a runtime.Caller on the same frame can never disagree about where it is.
    private static (string file, int line) goFramePosition(System.Reflection.MethodBase method, StackFrame frame)
    {
        string csPath = goSourcePath(frame.GetFileName());
        int csLine = frame.GetFileLineNumber();

        if (csPath.Length == 0)
            return (csPath, csLine);

        GoPositionMapRecord? record = goPositionMapRecord(method, csPath);

        if (record is null)
            return (csPath, csLine);

        int goLine = record.GoLineFor(csLine);

        // Below the file's first mapped construct there is no Go line to name, and half a position
        // is the one answer this design does not give.
        if (goLine <= 0)
            return (csPath, csLine);

        return (record.ResolveGoFile(csPath), goLine);
    }

    // goPositionMapRecord finds the record describing the file a frame's PDB names, reading the
    // frame's own assembly once and caching the result. Only a program that actually inspects frames
    // ever pays for this, and it pays once per assembly.
    private static GoPositionMapRecord? goPositionMapRecord(System.Reflection.MethodBase method, string csPath)
    {
        System.Reflection.Assembly? assembly = method.DeclaringType?.Assembly ?? method.Module.Assembly;

        if (assembly is null)
            return null;

        Dictionary<string, GoPositionMapRecord>? records;

        lock (s_positionMapLock)
        {
            if (!s_positionMaps.TryGetValue(assembly, out records))
            {
                records = readGoPositionMaps(assembly);
                s_positionMaps[assembly] = records;
            }
        }

        int separator = csPath.LastIndexOf('/');
        string csFile = separator < 0 ? csPath : csPath[(separator + 1)..];

        return records.TryGetValue(csFile, out GoPositionMapRecord? record) ? record : null;
    }

    // readGoPositionMaps materializes one assembly's records, keyed by the emitted file name the PDB
    // will report. Reflection failure is answered with an empty map rather than an exception: a
    // traceback is diagnostic output, and it must not be the thing that takes a program down.
    private static Dictionary<string, GoPositionMapRecord> readGoPositionMaps(System.Reflection.Assembly assembly)
    {
        Dictionary<string, GoPositionMapRecord> records = new(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (object attribute in assembly.GetCustomAttributes(typeof(GoPositionMapAttribute), false))
            {
                if (attribute is GoPositionMapAttribute map && map.CsFile.Length > 0)
                    records[map.CsFile] = new GoPositionMapRecord(map.GoFile, map.Table, map.FuncLits);
            }
        }
        catch (Exception)
        {
            // An assembly whose attributes cannot be read simply has no Go positions.
        }

        return records;
    }

    // resolveRecordedGoFile roots a recorded Go source identity (see GoPositionMapRecord.ResolveGoFile):
    // a bare file name against the C# file's directory, a GOROOT-relative form against linkRoot's src
    // directory when there is a link-time root, an absolute path verbatim.
    private static string resolveRecordedGoFile(string goFile, string csPath, string linkRoot)
    {
        if (goFile.Length == 0 || GoPositionMapRecord.isRootedGoPath(goFile))
            return goFile;

        if (goFile.IndexOf('/') < 0)
        {
            int separator = csPath.LastIndexOf('/');
            return separator > 0 ? string.Concat(csPath.AsSpan(0, separator + 1), goFile) : goFile;
        }

        string root = linkRoot.Replace('\\', '/').TrimEnd('/');
        return root.Length > 0 ? string.Concat(root, "/src/", goFile) : goFile;
    }

    /// <summary>
    /// GolibTests' probe (CallerFrameTestVariantNamingTests): how a recorded Go source identity is
    /// rooted for a frame of <paramref name="csPath"/> under the link-time root <paramref name="linkRoot"/>.
    /// </summary>
    public static string GoResolveRecordedFileProbe(string goFile, string csPath, string linkRoot) =>
        resolveRecordedGoFile(goFile, csPath, linkRoot);

    // One converted file's recorded position map.
    private sealed class GoPositionMapRecord(string goFile, string table, string funcLits = "")
    {
        private int[]? m_csLines;
        private int[]? m_goLines;
        private int[]? m_litStarts;
        private int[]? m_litEnds;
        private string[]? m_litSuffixes;
        private string? m_resolvedGoFile;

        // ResolveGoFile spells the recorded identity as the absolute path Go answers, without a
        // machine-specific path having been baked into a committed artifact. Each recorded form is
        // rooted at run time:
        //   - a bare file name, which the converter writes when the Go source sits BESIDE the C# it
        //     emitted, against the C# file's own compile-time directory;
        //   - the GOROOT-relative form of a standard-library source (`runtime/extern.go`, always
        //     carrying a separator), against the LINK-TIME root's src directory, defaultGOROOT, which
        //     the -tests pipeline hands the host (goenvs_impl.cs): default `go test` and `go build`
        //     bake that absolute path, so runtime.Caller, Frame.File and the traceback answer it too,
        //     and a program that opens its own source through them (the re-exec child of
        //     TestTracebackSystem) finds the file. Never the ambient GOROOT: Go's frames do not move
        //     with it (runtime/debug's TestStack runs its child with `GOROOT=` and expects the same
        //     frames), and it can name another Go install whose files do not match the recorded lines.
        //     With no link-time root, the recorded form is answered as recorded, which is Go's
        //     -trimpath form;
        //   - an already-absolute path, verbatim.
        public string ResolveGoFile(string csPath)
        {
            return m_resolvedGoFile ??= resolveRecordedGoFile(goFile, csPath, defaultGOROOT.ToString());
        }

        // GoLineFor answers the Go line the given emitted C# line was converted for — a PREDECESSOR
        // search, so a line inside a multi-line emission answers the Go statement it was emitted for,
        // which is the same model Go's own pclntab uses. A line above the file's first mapped
        // construct has no Go line and answers 0.
        public int GoLineFor(int csLine)
        {
            decode();

            int[] csLines = m_csLines!;

            if (csLines.Length == 0 || csLine < csLines[0])
                return 0;

            int low = 0;
            int high = csLines.Length - 1;

            while (low < high)
            {
                int middle = (low + high + 1) / 2;

                if (csLines[middle] <= csLine)
                    low = middle;
                else
                    high = middle - 1;
            }

            return m_goLines![low];
        }

        // decode reads the delta stream described by GoPositionMapAttribute. A byte with its high bit
        // set packs one record; a 0x00 byte introduces the varint form; no other value below 0x80 is
        // ever emitted, so anything else means the stream is corrupt and the rest of it is dropped
        // rather than mis-read into plausible line numbers.
        private void decode()
        {
            if (m_csLines is not null)
                return;

            List<int> csLines = new();
            List<int> goLines = new();

            try
            {
                byte[] buffer = Convert.FromBase64String(table);
                int index = 0;
                int csLine = 0;
                int goLine = 0;

                while (index < buffer.Length)
                {
                    byte marker = buffer[index++];
                    ulong advance;
                    ulong zigzag;

                    if ((marker & 0x80) != 0)
                    {
                        advance = (ulong)((marker >> 4) & 0x07);
                        zigzag = (ulong)(marker & 0x0F);
                    }
                    else if (marker == 0x00)
                    {
                        advance = readVarint(buffer, ref index);
                        zigzag = readVarint(buffer, ref index);
                    }
                    else
                    {
                        break;
                    }

                    csLine += (int)advance + 1;
                    goLine += (int)((long)(zigzag >> 1) ^ -(long)(zigzag & 1));

                    csLines.Add(csLine);
                    goLines.Add(goLine);
                }
            }
            catch (Exception)
            {
                // A table that will not decode leaves the file unmapped — its frames report the .cs
                // position, exactly as an unrecorded file does.
                csLines.Clear();
                goLines.Clear();
            }

            m_goLines = goLines.ToArray();
            m_csLines = csLines.ToArray();
        }

        private static ulong readVarint(byte[] buffer, ref int index)
        {
            ulong value = 0;
            int shift = 0;

            while (index < buffer.Length)
            {
                byte current = buffer[index++];
                value |= (ulong)(current & 0x7F) << shift;

                if ((current & 0x80) == 0)
                    return value;

                shift += 7;
            }

            return value;
        }

        // FuncLiteralFor answers the recorded counter suffix (`1`, `2.1`) of the function literal
        // whose Go source span contains goLine — the INNERMOST such span, so a frame in a nested
        // literal answers the nested literal's name — or null when no recorded span contains it.
        // Innermost is the largest start line among the containing spans (spans nest lexically),
        // with the smaller end winning a shared start; a full tie keeps the first recorded entry.
        public string? FuncLiteralFor(int goLine)
        {
            int best = innermostFuncLiteral(goLine);

            return best < 0 ? null : m_litSuffixes![best];
        }

        // FuncLiteralStartFor answers the Go line of the `func` keyword of the innermost recorded
        // function literal containing goLine (a literal's span starts at lit.Pos()), or 0.
        public int FuncLiteralStartFor(int goLine)
        {
            int best = innermostFuncLiteral(goLine);

            return best < 0 ? 0 : m_litStarts![best];
        }

        private int innermostFuncLiteral(int goLine)
        {
            decodeFuncLits();

            int[] starts = m_litStarts!;
            int[] ends = m_litEnds!;
            int best = -1;

            for (int i = 0; i < starts.Length; i++)
            {
                if (goLine < starts[i] || goLine > ends[i])
                    continue;

                if (best < 0 || starts[i] > starts[best] || (starts[i] == starts[best] && ends[i] < ends[best]))
                    best = i;
            }

            return best;
        }

        // FunctionStartFor answers the Go line of the `func` keyword of the named function whose FIRST
        // sequence point is at csLine: the converter marks every function declaration's signature line
        // with the declaration's position (visitFuncDecl's sentinel at funcDecl.Pos(), which is the
        // `func` keyword, so a multi-line parameter list still answers the keyword's line), and nothing
        // it emits between the signature and the first statement carries a marker. So the entry at or
        // before csLine is that declaration's, unless csLine is a statement that carries its OWN entry,
        // in which case the declaration's is the one before it. 0 when the record has no such entry.
        public int FunctionStartFor(int csLine)
        {
            decode();

            int[] csLines = m_csLines!;

            if (csLines.Length == 0 || csLine < csLines[0])
                return 0;

            int low = 0;
            int high = csLines.Length - 1;

            while (low < high)
            {
                int middle = (low + high + 1) / 2;

                if (csLines[middle] <= csLine)
                    low = middle;
                else
                    high = middle - 1;
            }

            if (csLines[low] == csLine)
                return low > 0 ? m_goLines![low - 1] : 0;

            return m_goLines![low];
        }

        // decodeFuncLits parses the funcLits map — `<startLine>-<endLine>:<suffix>` entries,
        // semicolon-joined, suffix a dotted counter (`1`, `1.2`). Anything malformed drops the
        // WHOLE map rather than part of it, exactly as an undecodable line table does: those
        // frames then answer the derived fallback, never a plausible-but-wrong recorded name.
        private void decodeFuncLits()
        {
            if (m_litStarts is not null)
                return;

            List<int> starts = new();
            List<int> ends = new();
            List<string> suffixes = new();

            if (funcLits.Length > 0)
            {
                foreach (string entry in funcLits.Split(';'))
                {
                    int dash = entry.IndexOf('-');
                    int colon = entry.IndexOf(':');

                    if (dash <= 0 || colon <= dash + 1 || colon == entry.Length - 1 ||
                        !int.TryParse(entry.AsSpan(0, dash), out int start) ||
                        !int.TryParse(entry.AsSpan(dash + 1, colon - dash - 1), out int end) ||
                        start <= 0 || end < start || !validFuncLitSuffix(entry.AsSpan(colon + 1)))
                    {
                        starts.Clear();
                        ends.Clear();
                        suffixes.Clear();
                        break;
                    }

                    starts.Add(start);
                    ends.Add(end);
                    suffixes.Add(entry[(colon + 1)..]);
                }
            }

            m_litSuffixes = suffixes.ToArray();
            m_litEnds = ends.ToArray();
            m_litStarts = starts.ToArray();
        }

        // A recorded suffix is dot-separated counter segments, each a bare positive decimal.
        private static bool validFuncLitSuffix(ReadOnlySpan<char> suffix)
        {
            int digits = 0;

            foreach (char c in suffix)
            {
                if (c >= '0' && c <= '9')
                {
                    digits++;
                }
                else if (c == '.' && digits > 0)
                {
                    digits = 0;
                }
                else
                {
                    return false;
                }
            }

            return digits > 0;
        }

        // isRootedGoPath recognizes the absolute recorded form on either platform shape: a leading
        // slash, or a Windows drive letter. Go spells every recorded path with forward slashes, so
        // there is only ever one separator to consider.
        internal static bool isRootedGoPath(string path)
        {
            if (path.Length > 0 && path[0] == '/')
                return true;

            return path.Length > 1 && path[1] == ':';
        }
    }

    // ReadMemStats populates m with memory allocator statistics.
    public static void ReadMemStats(ж<MemStats> Ꮡm)
    {
        // Go reads the stats inside stopTheWorld(stwReadMemStats). The pair records its pause
        // without allocating (StwPauses), so this read path stays allocation-free.
        worldStop stw = stopTheWorld(stwReadMemStats);
        ref var m = ref Ꮡm.Value;

        // ⚠ THIS READ PATH MUST NOT ALLOCATE, and that is a landing precondition rather than a
        // nicety (DESIGN-readmemstats-surface.md §8.2): net/textproto's banked
        // TestReadMIMEHeaderAllocations brackets each header read between two ReadMemStats calls and
        // asserts under 32,768 B per iteration, so anything allocated after the first call captures
        // TotalAlloc — or before the second one does — lands INSIDE the measured window and is
        // charged to ReadMIMEHeader. GolibTests.GcMeasurementSurfaceProbes.ReadMemStatsPerCallAllocation
        // is the guard, and it is pinned at ZERO.
        //
        // The one allocation this body used to make was invisible: GCMemoryInfo is a struct, but
        // GC.GetGCMemoryInfo() allocates a fresh GCMemoryInfoData CLASS behind it on EVERY call —
        // 288 B on net9.0/9.0.19 x64, measured, which is 25 % of net/textproto's per-iteration budget
        // in the worst bracketed window (§7.1.4). So the committed/heap-size figures now come from
        // the recorder, which already samples them once per gen2 collection; the direct read below
        // runs only when there is no recorder sample to reuse — before the first observed collection,
        // or with GO2CS_GC_PAUSE_HISTORY=0, where it restores the pre-recorder behavior exactly.
        //
        // The cost of reusing the recorder's sample is freshness: TotalCommittedBytes is a snapshot
        // as of the last GC in EITHER case, and is now as of the last observed GEN2 collection. It is
        // therefore fresh at every point a Go test reads it — runtime.GC() and debug.FreeOSMemory()
        // both drain the recorder before returning — and as stale as the last full cycle elsewhere.
        if (!GcPauseRecorder.HasCommittedSample)
        {
            GCMemoryInfo info = System.GC.GetGCMemoryInfo();
            GcPauseRecorder.SampleCommitted(info.TotalCommittedBytes, info.HeapSizeBytes);
        }

        // One snapshot under one lock, filling the caller's own PauseNs/PauseEnd backing storage
        // in place. This is the SAME snapshot runtime/debug.readGCStats reads, which is what makes
        // TestReadGCStats' nine cross-surface assertions hold by construction: there is no second
        // source for them to disagree with.
        GcPauseSnapshot gc = GcPauseRecorder.ReadInto(m.PauseNs, m.PauseEnd);

        uint64 live = (uint64)System.GC.GetTotalMemory(forceFullCollection: false);
        uint64 committed = gc.CommittedBytes;

        m.Alloc = live;
        m.HeapAlloc = live;
        m.TotalAlloc = (uint64)System.GC.GetTotalAllocatedBytes(precise: false);
        m.Sys = committed;
        m.HeapSys = committed;
        m.HeapInuse = live;
        m.HeapIdle = committed > live ? committed - live : 0;
        m.HeapReleased = gc.HeapReleased;
        m.NextGC = gc.HeapSizeBytes;
        m.LastGC = gc.LastGcEndUnixNs;
        m.PauseTotalNs = gc.PauseTotalNs;
        m.NumGC = (uint32)gc.NumGC;
        m.NumForcedGC = gc.NumForcedGC;
        m.EnableGC = true;

        // Deliberately left zero: Mallocs/Frees/Lookups/HeapObjects/BySize, the Stack/MSpan/MCache/
        // BuckHash/GC/OtherSys breakdown, GCCPUFraction and DebugGC. A field is answered only when a
        // managed measurement means the SAME THING the Go field means; where the CLR measures
        // something adjacent-but-different, the field stays zero and the header names the adjacent
        // quantity and why it was refused (§4.3).
        startTheWorld(stw);
    }

    // LockOSThread wires the calling goroutine to its current operating system thread.
    //
    // THE BINDING HALF IS STILL TRUE BY CONSTRUCTION — a goroutine IS a managed thread here, so
    // "this goroutine will not be migrated" holds whether or not these bodies run. What the four
    // empty bodies got WRONG is the ACCOUNTING half: Go's `m.lockedExt`/`m.lockedInt` counters and
    // the `m.lockedg`/`g.lockedm` back-links are STATE Go's own suite reads back, through
    // `runtime.LockOSCounts` (export_test.go), and a no-op answers 0,0 where Go answers 1,0.
    // Measured 2026-09-08 at 44f858717 and reproduced at this tree: `lockedExt` had ZERO
    // increment/decrement sites corpus-wide and `lockedInt` exactly 3, all of them `oneNewExtraM`'s
    // cgo extra-M path — because the hand-own displaced the very code that maintains them
    // (positive control: `locks++` reads 24 in this same folder). So these carry Go's whole body now.
    //
    // DELIBERATE DIVERGENCE, ONE, NAMED: Go's LockOSThread starts the template thread first
    // (proc.go, the `newmHandoff.haveTemplateThread` guard) so that a LOCKED thread can hand thread
    // CREATION to a known-good one. `startTemplateThread` exists in the converted corpus
    // (<goos>/proc.cs) but the managed host never creates threads through `newm`, so calling it
    // would start a thread nothing hands work to — speculative machinery, not fidelity. Omitted,
    // and this comment is the omission's record.
    //
    // `dolockOSThread`/`dounlockOSThread` are NOT hand-owned: they are the auto-converted bodies in
    // <goos>/proc.cs, already faithful (they set and clear the same two back-links, behind Go's own
    // `GOARCH == "wasm"` guard), in this same partial class. These four call them exactly as Go does.
    public static void LockOSThread()
    {
        var gp = getg();
        gp.Value.m.Value.lockedExt++;
        if (gp.Value.m.Value.lockedExt == 0)
        {
            gp.Value.m.Value.lockedExt--;
            throw panic("LockOSThread nesting overflow");
        }
        dolockOSThread();
    }

    // UnlockOSThread undoes an earlier call to LockOSThread.
    public static void UnlockOSThread()
    {
        var gp = getg();
        if (gp.Value.m.Value.lockedExt == 0)
            return;
        gp.Value.m.Value.lockedExt--;
        dounlockOSThread();
    }

    // The runtime-internal variants, reached through syscall and startTemplateThread.
    internal static void lockOSThread()
    {
        getg().Value.m.Value.lockedInt++;
        dolockOSThread();
    }

    internal static void unlockOSThread()
    {
        var gp = getg();
        if (gp.Value.m.Value.lockedInt == 0)
            systemstack(badunlockosthread);
        gp.Value.m.Value.lockedInt--;
        dounlockOSThread();
    }

    // Pinner.Pin / Pinner.Unpin live in pinner_impl.cs (Q45). The "address is stable" half of
    // their contract IS a no-op here, exactly as the LockOSThread class above — but the pin BIT
    // and the lifetime HOLD Go's suite measures are real state, and that file owns the whole seam.

    // ------- The traceback surface: Callers / callers (Caller's funnel) / Frames.Next -------

    // One converted-Go call site observed on the managed stack, resolved at intern time so a
    // later Frames walk needs no live StackFrame. Tokens are process-lifetime, like Go's program
    // counters, so a pc slice recorded by Callers stays resolvable by any later CallersFrames.
    private sealed class CallerFrameRecord
    {
        public string Function = string.Empty;
        public string File = string.Empty;
        public nint Line;

        // The Go line of the function's `func` keyword (Go's _func.startLine / Frame.startLine), 0 where
        // no source position is known. See goFunctionStartLine.
        public nint StartLine;
    }

    private static readonly object s_callerTableLock = new();
    private static readonly Dictionary<string, nuint> s_callerTokens = new();
    private static readonly List<CallerFrameRecord> s_callerRecords = new();

    // EVERY CALL SITE OWNS A SPAN, AND ITS TOKEN SITS IN THE MIDDLE. Go's consumers do arithmetic on a
    // return PC: runtime.expandFrames and runtime/pprof's expandInlinedFrames write `f.PC + 1`, and Go's
    // Frames.Next takes `pc--` on a return PC, so the pair round-trips inside one function. Tokens once
    // were dense (1, 2, 3, ...) and resolved by exact match, so that `+1` named the NEXT minted call
    // site: every block-profile stack symbolized as the wrong function (TestBlockProfileBias, measured
    // at the I2 store, 2026-09-26). The span is GoSyntheticPC's stride, for the same reason it has one.
    //
    // THE BAND IS NON-CANONICAL AND UNTAGGED. It starts at 0x8000_8000_0000_0000: bits 63 and 47 set,
    // bits 62..48 clear. That keeps it disjoint by construction from every other space a uintptr can
    // hold in this corpus: managed-pointer hashes (below 2^32); tagged pointer tokens (bit 63 set, bit
    // 47 CLEAR; ManagedPointerTokens.IsTaggedToken); synthetic PCs (from 0xFFFF_8000_0000_0000,
    // GoSyntheticPC); and every x64 user-mode or kernel address, which has bits 63..47 all equal, so
    // a pinned data pointer or a marshal buffer can never name a call site. The band holds 2^35 spans
    // before a carry into bit 48 would clear bit 47 and make a tagged token; the record list is an
    // int-indexed List, so it can never reach that. (It first started at 2^32, a valid user-mode
    // address: the i9's hardening note on 96ce90f997. The i9's 0x8000_0000_0000_0000 is tagged.)
    // 64-bit only, as the corpus is (the synthetic registry refuses 32-bit).
    private const int CallerSpanShift = 12;
    private static readonly nuint s_callerSpanBase = unchecked((nuint)0x8000_8000_0000_0000UL);

    private static nuint callerSpanStart(int index) => s_callerSpanBase + ((nuint)index << CallerSpanShift);

    // Callers fills pc with the return PCs of function invocations on the calling goroutine's
    // stack, skipping `skip` frames (0 identifies the frame for Callers itself, 1 its caller).
    // The auto body enters the raw-metal unwinder on its first step (callers → getcallersp, an
    // assembly stub); the CLR answers the API CONTRACT natively — walk the managed stack and
    // project it to GO-LOGICAL frames (captureCallers below).
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static nint Callers(nint skip, slice<uintptr> pc)
    {
        // Go picks off a 0-length pc before touching the walker (a nil pc array is the
        // print-a-traceback signal there); preserve the observable short-circuit.
        if (len(pc) == 0)
            return 0;

        // +1 drops captureCallers' own frame, so Go's skip==0 lands on THIS frame — which is
        // exactly what "0 identifies the frame for Callers itself" means.
        return captureCallers(skip + 1, pc);
    }

    // callers is the runtime-internal funnel every other traceback entry point goes through
    // (Caller, mprof's profile recorders, proc's createstack, tracestack) and the only one that
    // actually reaches getcallersp — Go's own body opens with `sp := getcallersp()`. It is
    // "almost identical to Callers" (Go's comment on the declaration) with one difference that
    // matters here: it starts from its CALLER's pc/sp, so skip==0 identifies the frame that
    // called it, not its own. Severing the chain at this boundary rather than at each public
    // entry point is what leaves runtime.Caller auto-converted and Go-shaped while still working.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static nint callers(nint skip, slice<uintptr> pcbuf)
    {
        // +2 drops captureCallers' frame and this one, so Go's skip==0 lands on the caller.
        return captureCallers(skip + 2, pcbuf);
    }

    // The walk itself. What counts as a frame is what Go's unwinder reports: functions the GO
    // SOURCE declares. Converted declarations count — free functions and [GoRecv] receivers on
    // the package class, methods on the structs nested in it, and function literals (compiler
    // display classes nested in the same scope). go2cs dispatch machinery does not: an interface
    // adapter shell or a generated forwarder has no Go frame, exactly as Go's interface dispatch
    // adds none. Depth DELTAS between two Callers calls on one goroutine therefore match Go's
    // logical model — the property io's flatten tests assert (readDepth == myDepth+2). The BOTTOM
    // is Go's too where the host can name it: a host method marked GoStackRoot reports the Go
    // frame it stands in for (the test host's testing.tRunner), and a goroutine ends at
    // runtime.goexit (see the walk's tail).
    //
    // ⚠ This method IS itself a Go-source frame by that test (it is declared on runtime_package),
    // as is every entry point above it, so each caller adds its own frame to `skip`. Keep them
    // and this one NoInlining: the CLR's StackTrace does not report inlined frames, and Go's
    // unwinder does (through the compiler's inline trees), so an inlined hop would silently
    // shift every answer by one.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nint captureCallers(nint skip, slice<uintptr> pc)
    {
        StackTrace stack = new(skipFrames: 0, fNeedFileInfo: true);
        StackFrame[] frames = stack.GetFrames();
        nint remainingSkip = skip;
        nint count = 0;

        bool sawRoot = false;
        int sequencesMet = 0;

        for (int i = 0; i < frames.Length; i++)
        {
            StackFrame frame = frames[i];
            System.Reflection.MethodBase? method = frame.GetMethod();

            if (method is null)
                continue;

            // A deferring GoFrame.Run: if its sequence is running a panic, Go's unwinder would find that
            // panic's frames here, beneath the deferred call (splicePanic). Spliced frames take the
            // live frames' own skip and capacity path, since Go's skip counts gopanic ([P2-3]).
            if (method == GoFrame.RunMethod)
            {
                // Pairing advances for every Run frame; a thread that refuses splices (a stopped
                // range-over-func seq's coro) answers its live stack only.
                if (GoFrame.SequenceFromTop(sequencesMet++) is ({ } panic, long activation) && !GoFrame.SplicesRefused)
                {
                    foreach (uintptr spliced in splicePanic(panic, activation, frames, i))
                    {
                        if (remainingSkip > 0)
                        {
                            remainingSkip--;
                            continue;
                        }

                        if (count >= len(pc))
                            return count;

                        pc[count++] = spliced;
                    }
                }

                continue;
            }

            GoStackRootAttribute? root = null;

            if (!isGoSourceFrame(method))
            {
                root = stackRootOf(method);

                if (root is null)
                    continue;

                sawRoot = true;
            }

            if (remainingSkip > 0)
            {
                remainingSkip--;
                continue;
            }

            // A full buffer ends the walk from the top, as Go's does: no root is forced in at the
            // cost of a real frame.
            if (count >= len(pc))
                return count;

            pc[count++] = root is null ? internCallerFrame(method, frame) : internRootFrame(root.Function, root.File, root.Line);
        }

        // GO'S BOTTOM FRAME. Go's unwinder ends every goroutine at runtime.goexit, the return address
        // newproc plants below the start function, and Go's own tests read that bottom (runtime's
        // testCallersEqual drops the last frame it is given). A goroutine a `go` statement started
        // carries a creator; a host method standing in for Go's own root (the test host's tRunner)
        // carries GoStackRoot. The main goroutine's runtime.main root is not modeled, and a thread a
        // host entered without a root gets none: nothing on it stands where a Go frame is.
        if ((sawRoot || Goroutine.Current?.Creator is not null) && remainingSkip == 0 && count < len(pc))
            pc[count++] = internRootFrame("runtime.goexit", "runtime/asm_amd64.s", 1700);

        return count;
    }

    // The GoStackRoot a host method carries, read once per method: a walk passes the same few host
    // frames on every call.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<System.Reflection.MethodBase, GoStackRootAttribute?> s_stackRoots = new();

    private static GoStackRootAttribute? stackRootOf(System.Reflection.MethodBase method) =>
        s_stackRoots.GetOrAdd(method, static m => (GoStackRootAttribute?)Attribute.GetCustomAttribute(m, typeof(GoStackRootAttribute), inherit: false));

    // Interns a root frame no live call site backs: the record is the Go frame itself, keyed by its
    // Go function so every walk that reaches the root answers the same PC, as Go's does.
    private static uintptr internRootFrame(string function, string file, int line)
    {
        string key = $"root:{function}";

        lock (s_callerTableLock)
        {
            if (s_callerTokens.TryGetValue(key, out nuint token))
                return token;

            s_callerRecords.Add(new CallerFrameRecord { Function = function, File = file, Line = line });
            token = callerSpanStart(s_callerRecords.Count - 1) + ((nuint)1 << (CallerSpanShift - 1));
            s_callerTokens[key] = token;
            return token;
        }
    }

    // GO'S PANICKING FRAMES (docs/phase4/DESIGN-panic-stack-frames.md §3.B, option B). Go runs a
    // panic's deferred calls ON the panicking stack, so its unwinder, walking down from a deferred
    // call, finds runtime.gopanic, the fault frames (panicmem and sigpanic for a hardware fault,
    // panicdivide for an integer divide), then the frames the panic was raised in, then the deferring
    // function. The CLR has unwound those frames before the emitted finally runs the deferred call.
    // They survive as the panic's SiteTrace, the raised exception's trace from its throw site to its
    // first catching frame, and this splices them back. Go's lines are go1.24.13's: gopanic calls the
    // deferred func at panic.go:792.
    //
    // OWNERSHIP IS CHECKED BY ACTIVATION, NOT BY METHOD ([P2-2], amended by COORD's review of the cut).
    // Every link of a splice must be a panic whose SiteOwner is the ACTIVATION of the Run being walked:
    // golib stamps it only where the site provably ends (GoFrame.Run). A link fails the whole splice
    // otherwise, so a panic re-raised past its first catcher, a recursive activation, or a site golib
    // could not place splices NOTHING: a missing splice is a known divergence, and a wrong one is a
    // silent lie. The owner is the activation AND its thread (SiteOwnerThread): activation numbers are
    // per thread, and range-over-func carries a panic from a coro's thread to the ranging goroutine.
    // Two kinds of site pass:
    //   - one that ends at the DEFERRING FUNCTION (its own catch caught the panic first): the site's
    //     last Go frame is that function, checked again against the next live Go frame by method, and
    //     dropped, since the live walk reports it;
    //   - one that ends at the RUN, in exactly two cases, and the chain then continues with the panic
    //     Beneath, whose gopanic called the deferred call: the zero-argument nil-deferred-func thunk
    //     (no Go frame of its own; its fault frames alone), or the deferred delegate that is itself the
    //     panic's first catcher (SiteIsTheDeferredCall; all its Go frames stay). Every OTHER panic a
    //     deferred call raises is refused: the converter's defer wrappers (`() => c()` for a named func
    //     type, `defer panic(v)`'s thunk, the lambdas for a call whose results are dropped) are
    //     indistinguishable at run time from Go's closures, and Go ELIDES its deferwrap except over the
    //     panic machinery, so any of them would splice a frame Go does not show, or misname one Go does
    //     (COORD's verification of the re-cut, B2 and C3). A stated missing splice until the converter
    //     marks its defer wrappers.
    // Only the fault kinds golib tags where the panic is RAISED are spliced: an explicit panic(v), a nil
    // dereference and an integer divide. Every other runtime error (goPanicIndex, the goPanicSlice family,
    // panicdottypeE/I, mapassign, closechan, ...: a runtime frame that is not modelled) is refused, and an
    // explicit panic thrown outside Go code (a golib helper) is refused too, as a second guard.
    //
    // The chain is walked iteratively with a visited set, and every frame goes out through the caller's
    // skip and capacity path, so a long chain truncates from the top as Go's does ([P2-3]).
    //
    // A WRAPPER THAT IS THE PANIC SITE IS KEPT (COORD 93c2bdd50b): Go's elideWrapperCalling keeps a
    // wrapper whose callee is gopanic, sigpanic or panicwrap. So a GoWrapper frame that is the site's
    // FIRST Go frame is emitted; every other wrapper keeps the live walk's elision.
    //
    // Named, not folded ([P2-4]): runtime.gopanic is also a real converted method, and a live walk
    // through it (Go code calling it directly) interns a second record for the function.
    private static List<uintptr> splicePanic(PanicException panic, long activation, StackFrame[] live, int runIndex)
    {
        System.Reflection.MethodBase? owner = null;

        for (int j = runIndex + 1; j < live.Length && owner is null; j++)
        {
            if (live[j].GetMethod() is { } liveMethod && isGoSourceFrame(liveMethod))
                owner = liveMethod;
        }

        List<uintptr> spliced = [];
        HashSet<PanicException> visited = new(ReferenceEqualityComparer.Instance);

        for (PanicException? link = panic; link is not null; link = link.Beneath)
        {
            if (!visited.Add(link) || link.SiteOwner != activation || !ReferenceEquals(link.SiteOwnerThread, GoFrame.ThreadToken))
                return [];

            if (link.FaultKind == PanicFaultKind.Unmodelled || link.CrossedRangeFunc)
                return [];

            StackFrame[] raw = link.SiteTrace?.GetFrames() ?? [];
            List<StackFrame> site = [];

            foreach (StackFrame siteFrame in raw)
            {
                System.Reflection.MethodBase? siteMethod = siteFrame.GetMethod();

                if (siteMethod is not null && (isGoSourceFrame(siteMethod) || site.Count == 0 && isGoSourceFrame(siteMethod, keepWrapper: true)))
                    site.Add(siteFrame);
            }

            // The second guard: an explicit panic that golib code, not Go source, threw.
            if (link.FaultKind == PanicFaultKind.Explicit && firstMethodOf(raw) is { } thrower && !isGoSourceFrame(thrower, keepWrapper: true))
                return [];

            // THE WRAPPER FRAME, keyed on the EMITTER'S marker, never on names or frame shapes (COORD's check of
            // round 3). A method-expression wrapper as the site's first Go frame is kept only when the wrapper
            // raised the panic itself (RaisedByWrapper: builtin.wrapperRecv/panicwrapRecv on a nil receiver),
            // because Go keeps a wrapper only when its callee is the panic machinery. Any other panic with a
            // wrapper there (a callee the JIT inlined into it, a promoted method's embedded deref) is refused.
            if (site.Count > 0 && isWrapperFrame(site[0].GetMethod()) != link.RaisedByWrapper)
                return [];

            // An INTRINSIFIED atomic on a nil address: Go's intrinsic faults with no frame of its own, while a
            // call through a func value keeps the frame, and a run cannot tell the two apart (COORD's check of
            // round 3), so a fault whose first site frame is one of them is refused.
            if (link.FaultKind == PanicFaultKind.Memory && site.Count > 0 && isIntrinsifiedAtomic(site[0].GetMethod()))
                return [];

            // An explicit panic from a function Go replaces with an INTRINSIC on amd64: Go's frames are the
            // intrinsic's runtime frames (panicdivide, panicoverflow), not the Go source's, so it is refused.
            if (link.FaultKind == PanicFaultKind.Explicit && site.Count > 0 && isPanickingIntrinsic(site[0].GetMethod()))
                return [];

            // A nil receiver box dereferenced inside a go2cs-gen interface ADAPTER: Go answers panicwrap when the
            // call is dispatched and panicmem/sigpanic when its compiler devirtualizes it, which a run cannot
            // know, so it is refused.
            if (link.FaultKind == PanicFaultKind.Memory && adapterBeforeFirstGoFrame(raw))
                return [];

            // An ends-at-Run site: only the nil-func thunk, or the deferred delegate that caught first.
            if (link.SiteEndsAtRun && !link.SiteIsTheDeferredCall &&
                !(site.Count == 0 && link.FaultKind == PanicFaultKind.Memory && firstMethodOf(raw) == GoFrame.NilDeferredCallMethod))
                return [];

            spliced.Add(internRootFrame("runtime.gopanic", "runtime/panic.go", 792));

            switch (link.FaultKind)
            {
                case PanicFaultKind.Panicwrap:
                    // Go's `(*T).M` wrapper called with a nil *T (builtin.panicwrapRecv raised it, the
                    // emitter's marker): gopanic | runtime.panicwrap | (*T).M | owner.
                    spliced.Add(internRootFrame("runtime.panicwrap", "runtime/error.go", 356));
                    break;
                case PanicFaultKind.Memory:
                    spliced.Add(internRootFrame("runtime.panicmem", "runtime/panic.go", 262));
                    spliced.Add(GOOS == "windows"u8
                        ? internRootFrame("runtime.sigpanic", "runtime/signal_windows.go", 401)
                        : internRootFrame("runtime.sigpanic", "runtime/signal_unix.go", 925));
                    break;
                case PanicFaultKind.Divide:
                    spliced.Add(internRootFrame("runtime.panicdivide", "runtime/panic.go", 241));
                    break;
            }

            if (!link.SiteEndsAtRun)
            {
                // The site ends at the deferring function, and the chain ends with it.
                if (site.Count == 0 || site[^1].GetMethod() != owner)
                    return [];

                site.RemoveAt(site.Count - 1);

                foreach (StackFrame siteFrame in site)
                    spliced.Add(internCallerFrame(siteFrame.GetMethod()!, siteFrame));

                return spliced;
            }

            foreach (StackFrame siteFrame in site)
                spliced.Add(internCallerFrame(siteFrame.GetMethod()!, siteFrame));
        }

        return spliced;
    }

    // Go's amd64 intrinsics whose Go source panics (cmd/compile/internal/ssagen/intrinsics.go, go1.24.13):
    // math/bits.Div64, and Div, its alias on amd64. Their panic(divideError)/panic(overflowError) never runs;
    // the intrinsic raises through runtime.panicdivide/panicoverflow. Div32 is NOT intrinsified, so its
    // Go-source panic keeps its frame (gopanic | math/bits.Div32), as Go's does. Matched by declaring type
    // and name because runtime cannot reference math/bits.
    private static bool isPanickingIntrinsic(System.Reflection.MethodBase? method) =>
        method is { Name: "Div64" or "Div", DeclaringType.FullName: "go.math.bits_package" };

    private static bool isWrapperFrame(System.Reflection.MethodBase? method) =>
        method is not null && method.IsDefined(typeof(GoWrapperAttribute), inherit: false);

    // Go's intrinsified atomics (cmd/compile's intrinsics.go): internal/runtime/atomic wholesale, and
    // sync/atomic's Load/Store/Swap/CompareAndSwap/Add/And/Or functions except the *Pointer forms, which Go
    // does not intrinsify (their frame stays, as it does here). Keyed by declaring type and name because
    // runtime cannot reference sync/atomic; a refusal, never a model.
    private static bool isIntrinsifiedAtomic(System.Reflection.MethodBase? method)
    {
        if (method?.DeclaringType?.FullName is not { } type)
            return false;

        if (type == "go.@internal.runtime.atomic_package" || type == "go.internal.runtime.atomic_package")
            return true;

        if (type != "go.sync.atomic_package" || method.Name.EndsWith("Pointer", StringComparison.Ordinal))
            return false;

        return method.Name.StartsWith("Load", StringComparison.Ordinal) || method.Name.StartsWith("Store", StringComparison.Ordinal) ||
               method.Name.StartsWith("Swap", StringComparison.Ordinal) || method.Name.StartsWith("CompareAndSwap", StringComparison.Ordinal) ||
               method.Name.StartsWith("Add", StringComparison.Ordinal) || method.Name.StartsWith("And", StringComparison.Ordinal) ||
               method.Name.StartsWith("Or", StringComparison.Ordinal);
    }

    // Whether a go2cs-gen interface adapter frame lies between the throw and the site's first Go frame.
    private static bool adapterBeforeFirstGoFrame(StackFrame[] frames)
    {
        foreach (StackFrame frame in frames)
        {
            if (frame.GetMethod() is not { } method)
                continue;

            if (method.DeclaringType is { } declaring && typeof(IGoAdapter).IsAssignableFrom(declaring))
                return true;

            if (isGoSourceFrame(method, keepWrapper: true))
                return false;
        }

        return false;
    }

    private static System.Reflection.MethodBase? firstMethodOf(StackFrame[] frames)
    {
        foreach (StackFrame frame in frames)
        {
            if (frame.GetMethod() is { } method)
                return method;
        }

        return null;
    }

    // Frames.Next expands the next recorded PC into a Frame. The auto body resolves PCs through
    // findfunc's linker-built funcInfo tables, which have no managed form; the records minted by
    // Callers carry the same answers (Function in Go's spelling, File, Line). A PC this runtime
    // never minted resolves like Go's !funcInfo.valid() — skipped, not fatal. Entry is the start of
    // the call site's span: entry points are not distinct from call SITES in the token model, so two
    // sites in one function report two entries where Go reports one (named, not modeled).
    //
    // Frame.Func and Frame.startLine (census A2, D3; COORD 2026-09-28, reopening the semantic bill's
    // "Frame.Func unchanged"): Go leaves Func nil ONLY for an inlined frame, and go2cs keeps Go's function
    // boundaries one-for-one, so no frame is inlined and every Go frame carries its function's interned
    // *Func (frameFunc). startLine is the function's `func` keyword line (goFunctionStartLine), or a root
    // frame's go1.24.13 declaration line (rootFunctionStartLine).
    [GoRecv] public static (Frame frame, bool more) Next(this ref Frames ci)
    {
        while (len(ci.callers) > 0)
        {
            uintptr pcToken = ci.callers[0];
            ci.callers = ci.callers[1..];

            CallerFrameRecord? record = callerFrameRecord(pcToken);

            if (record is null)
                continue;

            // Go's own step: a recorded PC is a RETURN pc, "the start of the instruction following
            // the call", so Frames.Next reports the call pc one before it. Consumers add the 1 back
            // (expandFrames, pprof's expandInlinedFrames), and the span keeps both inside the site.
            uintptr entry = frameEntry(pcToken);
            uintptr callPC = pcToken > entry ? pcToken - 1 : pcToken;

            Frame frame = new()
            {
                PC = callPC,
                Func = frameFunc(record.Function, entry),
                Function = record.Function,
                File = record.File,
                Line = record.Line,
                startLine = record.StartLine != 0 ? record.StartLine : rootFunctionStartLine(record.Function),
                Entry = entry
            };

            return (frame, moreCallerFrames(ci.callers));
        }

        return (default!, false);
    }

    // True when another Callers-minted PC remains — the precise "more" Go's two-frame prefetch
    // computes (a trailing foreign PC does not promise a Frame that will never come).
    private static bool moreCallerFrames(slice<uintptr> callers)
    {
        for (nint i = 0; i < len(callers); i++)
        {
            if (callerFrameRecord(callers[i]) is not null)
                return true;
        }

        return false;
    }

    // The Go-frame test (see Callers). The frame's TOP-LEVEL declaring scope must be a
    // `<pkg>_package` class in namespace `go` — covering the package class itself, the struct
    // types nested in it, and a function literal's display class — and the method must not be
    // go2cs machinery: a generated adapter (IGoAdapter, dispatch plumbing), a go2cs-gen
    // synthesized member (RecvGenerator's ж-forwarders carry [GeneratedCode("go2cs-gen", …)]), or a
    // [StackTraceHidden] linkname forwarder.
    // Everything outside a package class — golib, the BCL, the test-host runtime — is not Go
    // code and never counts. keepWrapper admits a method-expression wrapper, which only splicePanic
    // asks for: the one place Go keeps a wrapper frame is directly beneath the panic machinery.
    private static bool isGoSourceFrame(System.Reflection.MethodBase method, bool keepWrapper = false)
    {
        Type? declaring = method.DeclaringType;

        if (declaring is null)
            return false;

        if (typeof(IGoAdapter).IsAssignableFrom(declaring))
            return false;

        // A converted //go:linkname or assembly-trampoline forwarder (the converter marks it): Go binds
        // the pull to the target's symbol, so no frame of the puller exists.
        if (method.IsDefined(typeof(System.Diagnostics.StackTraceHiddenAttribute), inherit: false))
            return false;

        foreach (object attribute in method.GetCustomAttributes(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute), inherit: false))
        {
            if (attribute is System.CodeDom.Compiler.GeneratedCodeAttribute generated && generated.Tool == "go2cs-gen")
                return false;
        }

        // An sstring twin's canonical value delegate (its lambda carries GoTwinForwarderAttribute) is
        // the same kind of machinery: a call through it shows the frames a direct call shows. The
        // twin's @string forwarder is go2cs-gen output, skipped by the check above
        // (docs/phase4/DESIGN-sstring-twin-pilot.md §3.4).
        if (method.IsDefined(typeof(GoTwinForwarderAttribute), inherit: false))
            return false;

        // A method-expression WRAPPER is elided, as Go's tracebackPCs elides a wrapper whose callee
        // is an ordinary function (elideWrapperCalling). A live walk only ever sees that case: Go
        // keeps the wrapper only when its callee is the panic machinery, which no live frame is
        // (splicePanic passes keepWrapper for the spliced site's first frame).
        if (!keepWrapper && method.IsDefined(typeof(GoWrapperAttribute), inherit: false))
            return false;

        Type topLevel = declaring;

        while (topLevel.DeclaringType is not null)
            topLevel = topLevel.DeclaringType;

        string? ns = topLevel.Namespace;

        if (ns is null || (ns != "go" && !ns.StartsWith("go.", StringComparison.Ordinal)))
            return false;

        return topLevel.Name.EndsWith("_package", StringComparison.Ordinal);
    }

    // Interns one observed call site to its process-lifetime token. Keyed by (module version id,
    // method metadata token, IL offset) — the managed spelling of "a PC": stable for the process
    // lifetime, distinct per call site, equal on every recurrence, so pc-equality comparisons
    // behave as they do in Go. Token 0 stays invalid, matching Go's zero-pc sentinel.
    private static uintptr internCallerFrame(System.Reflection.MethodBase method, StackFrame frame)
    {
        string key = $"{method.Module.ModuleVersionId}:{method.MetadataToken}:{frame.GetILOffset()}";

        lock (s_callerTableLock)
        {
            if (s_callerTokens.TryGetValue(key, out nuint token))
                return token;

            // A method-expression wrapper is Go's AUTOGENERATED function, positioned as Go positions it
            // (the same rule the FuncForPC record applies).
            (string file, int line) = method.IsDefined(typeof(GoWrapperAttribute), inherit: false) ? ("<autogenerated>", 1) : goFramePosition(method, frame);

            CallerFrameRecord record = new()
            {
                Function = goFrameName(method, frame),
                File = file,
                Line = line,
                StartLine = goFunctionStartLine(method)
            };

            s_callerRecords.Add(record);
            // The middle of the new site's span; never 0, so Go's zero-pc sentinel stays invalid.
            token = callerSpanStart(s_callerRecords.Count - 1) + ((nuint)1 << (CallerSpanShift - 1));
            s_callerTokens[key] = token;
            return token;
        }
    }

    // The call site whose span holds pc, or null. Callers hold s_callerTableLock.
    private static int? callerSpanIndex(nuint pc)
    {
        if (pc < s_callerSpanBase)
            return null;

        nuint index = (pc - s_callerSpanBase) >> CallerSpanShift;
        return index < (nuint)s_callerRecords.Count ? (int)index : null;
    }

    // The ENTRY of the span holding pc: a caller span's start, or a synthetic PC's function span
    // start. Any other pc is its own entry, as a function value's token is (FuncForPC(fn.Pointer())).
    private static uintptr frameEntry(uintptr pc)
    {
        lock (s_callerTableLock)
        {
            if (callerSpanIndex(pc) is int index)
                return callerSpanStart(index);
        }

        if (GoSyntheticPC.Resolve(pc) is { } method)
            return GoSyntheticPC.Of(method);

        return pc;
    }

    private static CallerFrameRecord? callerFrameRecord(uintptr token)
    {
        nuint value = token;

        lock (s_callerTableLock)
        {
            if (callerSpanIndex(value) is int index)
                return s_callerRecords[index];
        }

        // SECOND SOURCE, ONE RENDERER. A pc outside the caller table is not necessarily foreign: it
        // may be a SYNTHETIC PC minted by internal/abi's FuncPCABI0/FuncPCABIInternal for a function
        // whose address Go takes without calling it (runtime/pprof's lostProfileEvent is the first
        // consumer — its frame printed as `0x0` until this arm existed, because Frames.Next skips a
        // pc this returns null for). The two spaces are disjoint BY CONSTRUCTION and it is asserted
        // rather than assumed: caller tokens sit in spans from 0x8000_8000_0000_0000 (callerSpanStart,
        // GolibTests.RuntimeCallerPCSpanTests); synthetic PCs sit in the canonical high half
        // (GolibTests.SyntheticPCRegistryTests). So the caller
        // table always answers first and this arm can never shadow it.
        if (syntheticFrameRecord(value) is CallerFrameRecord synthetic)
            return synthetic;

        // THIRD SOURCE: a reflect func token. reflect's Value.Pointer() answers a delegate-method
        // token (value_impl.cs, delegateMethodToken) that FuncForPC already names (managedFuncName);
        // without this arm Frames.Next skipped it as foreign, so CallersFrames and FuncForPC
        // disagreed about one pc (runtime's TestCallersFromWrapper). Only a token registered for a
        // delegate answers, so a pointer token never resolves here.
        return ManagedPointerTokens.ResolveDelegateMethod(value) is System.Reflection.MethodBase method
            ? methodFrameRecord(method)
            : null;
    }

    private static readonly Dictionary<System.Reflection.MethodBase, CallerFrameRecord> s_methodFrames = new();

    // The record for a function reached through a reflect func token, cached per method. Keyed by
    // the MethodBase OBJECT: reflect's Method(i) builds its func values as DynamicMethods, whose
    // MethodHandle throws (the reason delegateMethodToken keys the same way).
    private static CallerFrameRecord methodFrameRecord(System.Reflection.MethodBase method)
    {
        lock (s_syntheticFrameLock)
        {
            if (s_methodFrames.TryGetValue(method, out CallerFrameRecord? cached))
                return cached;
        }

        CallerFrameRecord record = newMethodFrameRecord(method);

        lock (s_syntheticFrameLock)
        {
            s_methodFrames.TryAdd(method, record);
            return s_methodFrames[method];
        }
    }

    // One function's record with no live frame, named by the rule Callers applies to a Go frame
    // (goFrameName: receivers, test variants, function literals, wrappers) and by the registry's own
    // spelling otherwise. A method-expression wrapper reports Go's position for one:
    // <autogenerated>:1.
    private static CallerFrameRecord newMethodFrameRecord(System.Reflection.MethodBase method)
    {
        if (method.IsDefined(typeof(GoWrapperAttribute), inherit: false))
            return new CallerFrameRecord { Function = goFrameName(method, null), File = "<autogenerated>", Line = 1, StartLine = 1 };

        (string file, int line) = method is System.Reflection.Emit.DynamicMethod ? (string.Empty, 0) : syntheticFramePosition(method);

        return new CallerFrameRecord
        {
            Function = isGoSourceFrame(method) ? goFrameName(method, null) : GoSyntheticPC.GoNameOf(method),
            File = file,
            Line = line,
            StartLine = method is System.Reflection.Emit.DynamicMethod ? 0 : goFunctionStartLine(method)
        };
    }

    private static readonly object s_syntheticFrameLock = new();
    private static readonly Dictionary<RuntimeMethodHandle, CallerFrameRecord> s_syntheticFrames = new();

    // A synthetic PC resolved to the frame a traceback can print.
    //
    // A token knows WHICH FUNCTION, never which instruction, so the position is the FUNCTION's: the
    // method's first sequence point in its portable PDB (methodSourcePosition, the source the
    // closure-name rule already reads), mapped through the file's [GoPositionMap] record exactly as
    // goFramePosition maps a live frame's. That is a function's own declaration-site position, which
    // is what Go's frame for a function-entry PC reports too. Go's runtime/pprof requires it: a
    // location is `lookupFailed` when any frame has an empty Function, an empty File or Line 0
    // (proto.go), and then its mapping's HasFunctions is false -- windows' TestConvertCPUProfile and
    // TestConvertMemProfile read exactly that for FuncPCABIInternal(f1)/(f2), both synthetic here.
    //
    // WHERE NO PDB IS FOUND (none embedded, none beside the assembly) file and line stay empty and
    // line 0, the answer this record gave before a PDB was read. That was once the reason not to read
    // one at all: a published single-file host without a PDB would leave frames unnamed. The
    // closure-name seat measured that the -tests host finds its PDBs, so the fallback now costs only
    // the host that ships none, and it costs that host exactly what every host paid before.
    //
    // Keyed on the method rather than the pc because callers do arithmetic on a PC — runtime writes
    // `FuncPCABI0(goexit) + sys.PCQuantum`, pprof writes `+ 1` — so an unbounded set of pcs resolves
    // to one bounded set of functions, and moreCallerFrames re-resolves every remaining pc per frame.
    private static CallerFrameRecord? syntheticFrameRecord(nuint pc)
    {
        if (GoSyntheticPC.Resolve(pc) is not System.Reflection.MethodBase method)
            return null;

        RuntimeMethodHandle handle = method.MethodHandle;

        lock (s_syntheticFrameLock)
        {
            if (s_syntheticFrames.TryGetValue(handle, out CallerFrameRecord? cached))
                return cached;

            // A Go frame is named by the rule Callers applies (goFrameName: receivers, test variants,
            // function literals, wrappers); anything else keeps the registry's own spelling.
            CallerFrameRecord record = newMethodFrameRecord(method);

            s_syntheticFrames[handle] = record;
            return record;
        }
    }

    private static readonly Dictionary<System.Reflection.MethodBase, int> s_functionStartLines = new();

    // The Go line of a function's `func` keyword, Go's _func.startLine: what runtime.FrameStartLine and
    // runtime/pprof's Function.start_line report (census A2, D3). It is read from the SAME record every
    // frame position comes from, at the method's first sequence point:
    //   - a function LITERAL (a compiler-generated lambda or local function, `<Outer>b__…`) answers the
    //     start of the innermost recorded funcLits span containing that point, which the converter records
    //     from lit.Pos(), the literal's `func` keyword;
    //   - a named function answers its declaration's entry (GoPositionMapRecord.FunctionStartFor).
    // A method-expression wrapper is Go's autogenerated function and answers 1 (<autogenerated>:1). 0 where
    // no PDB or no record names a position, the answer every frame gave before. Cached per method: every
    // call site of one function reads the same line.
    private static int goFunctionStartLine(System.Reflection.MethodBase method)
    {
        lock (s_functionStartLines)
        {
            if (s_functionStartLines.TryGetValue(method, out int cached))
                return cached;
        }

        int startLine = 0;

        if (method.IsDefined(typeof(GoWrapperAttribute), inherit: false))
        {
            startLine = 1;
        }
        else
        {
            (string? csFile, int csLine) = methodSourcePosition(method);

            if (csFile is not null && csLine > 0 && goPositionMapRecord(method, goSourcePath(csFile)) is { } record)
            {
                if (method.Name.Length > 0 && method.Name[0] == '<')
                {
                    int goLine = record.GoLineFor(csLine);
                    startLine = goLine <= 0 ? 0 : record.FuncLiteralStartFor(goLine);
                }
                else
                {
                    startLine = record.FunctionStartFor(csLine);
                }
            }
        }

        lock (s_functionStartLines)
        {
            s_functionStartLines[method] = startLine;
        }

        return startLine;
    }

    // The start lines of the ROOT frames no live method backs (internRootFrame's records: the goroutine
    // root, the test-host root, and the panic machinery splicePanic splices), as go1.24.13 declares them,
    // the same release the frames' own lines are pinned to. Keyed by the Go function name the root is
    // interned under; sigpanic is declared per OS, as splicePanic names its file.
    private static nint rootFunctionStartLine(string function) => function switch
    {
        "runtime.goexit" => 1699,       // runtime/asm_amd64.s: TEXT runtime·goexit
        "runtime.gopanic" => 742,       // runtime/panic.go
        "runtime.panicmem" => 260,      // runtime/panic.go
        "runtime.panicdivide" => 239,   // runtime/panic.go
        "runtime.panicwrap" => 332,     // runtime/error.go
        "runtime.sigpanic" => GOOS == "windows"u8 ? 392 : 906, // runtime/signal_windows.go : signal_unix.go
        "testing.tRunner" => 1642,      // testing/testing.go
        _ => 0
    };

    private static readonly Dictionary<string, ж<Func>> s_frameFuncs = new(StringComparer.Ordinal);

    // The *Func a Go frame carries (Frame.Func), INTERNED per Go function name: Go's Func is a pointer
    // into the one pclntab entry of its function, so every frame of one function carries the same *Func.
    // Minted like FuncForPC's (a FuncRecord in s_funcRecords, so Name/Entry/FileLine answer the same way),
    // with the entry of the first frame that reached it. Every frame here is a Go frame (captureCallers
    // keeps no other), and nothing is ever inlined, so every frame with a name carries one; an unnamed
    // record keeps Go's nil. The box is a HandleBox for FuncForPC's reason below: Func is zero-size, and
    // a plain box of it would be the zerobase, making every frame's *Func equal to every other.
    private static ж<Func> frameFunc(string function, uintptr entry)
    {
        if (function.Length == 0)
            return default!;

        lock (s_frameFuncs)
        {
            if (s_frameFuncs.TryGetValue(function, out ж<Func>? box))
                return box;

            box = new HandleBox<Func>(new Func());
            s_funcRecords.Add(box, new FuncRecord { Name = function, Pc = entry });
            s_frameFuncs[function] = box;
            return box;
        }
    }

    // A method's own position with no live frame: its first sequence point, reported the way
    // goFramePosition reports a frame -- the Go position its file's record maps it to, or the converted
    // C# position where the record has none -- and ("", 0) where no PDB names one.
    private static (string file, int line) syntheticFramePosition(System.Reflection.MethodBase method)
    {
        (string? csFile, int csLine) = methodSourcePosition(method);

        if (csFile is null || csLine <= 0)
            return (string.Empty, 0);

        string csPath = goSourcePath(csFile);
        GoPositionMapRecord? record = goPositionMapRecord(method, csPath);

        if (record is null)
            return (csPath, csLine);

        int goLine = record.GoLineFor(csLine);

        return goLine <= 0 ? (csPath, csLine) : (record.ResolveGoFile(csPath), goLine);
    }

    // ------- FuncForPC / Func.Name / Func.Entry / Func.FileLine: a *Func recovered from a token -------
    //
    // The header above used to say a *Func has no managed referent. That was true when it was
    // written and stopped being true when ManagedPointerTokens landed (2026-08-29): reflect's
    // Value.Pointer() mints an identity token AND registers the object behind it, so a function
    // VALUE's token resolves back to its delegate, and a Callers() PC token already resolves to a
    // CallerFrameRecord carrying the Go-spelled name. Both are recoverable; nothing about PCs
    // being opaque tokens rather than addresses changed.
    //
    // What this does NOT restore is Go's *Func as a window onto pclntab — there is still no
    // symbol table and no inline tree. It answers the questions callers actually ask of a *Func
    // recovered from a function value or a traceback frame: its name (reflect's own abi_test.go
    // names every subtest `t.Run(runtime.FuncForPC(fn.Pointer()).Name(), ...)`, and answering ""
    // there made Go's testing package renumber the subtests #00, #01, ... turning one naming gap
    // into 83 orphaned comparison rows that read as 83 defects), and — since 2026-09-02 — its
    // Entry() and FileLine(pc). Both fell through to the auto-converted funcInfo()/firstmoduledata
    // walk until now, which is a permanent empty stub (symtab.cs's Ꮡfirstmoduledata, assigned
    // exactly once, to a moduledata whose pclntable is always empty) and could never resolve —
    // structurally, not intermittently, which is why TestCaller (runtime_test, symtab_test.go)
    // crashed the whole host on any goroutine that happened to reach Entry(). The record below
    // widens to carry the PC beside the name rather than adding a second table, so FuncForPC mints
    // both in the one mint site. Entry() returns the start of the span holding that PC (2026-09-26,
    // caller spans; the PC itself before) — this host's documented answer
    // to "what identifies this function" (PC values are opaque process-lifetime tokens, never
    // addresses; see the file header) — and FileLine(pc) resolves the SAME Go-position data
    // Callers()/Frames.Next() already serve, through callerFrameRecord. firstmoduledata and
    // Frame.Func are deliberately UNCHANGED by this arc: see docs/phase4/CENSUS-runtime-semantic-bill.md.
    // AMENDED 2026-09-28 (census A2, D3; COORD reopened the Frame.Func half): Frames.Next now sets
    // Frame.Func, one *Func interned per Go function (frameFunc, a record in this same table), because
    // Go leaves Func nil only for an inlined frame and no frame here is inlined. FuncForPC interns one
    // box per call-site ENTRY (s_funcHandles below) and Frame.Func one per Go function NAME (frameFunc),
    // so FuncForPC(pc) and a frame's Func are NOT the same pointer, where Go's are, and two call sites of
    // one function give two FuncForPC boxes but one Frame.Func; stated, not modeled (the one reader,
    // TestFunctionAlignmentTraceback, is disclosed on its code-byte read before it gets there).
    // firstmoduledata stays unchanged.
    private sealed class FuncRecord
    {
        public string Name = string.Empty;
        public uintptr Pc;
    }

    private static readonly ConditionalWeakTable<object, FuncRecord> s_funcRecords = new();

    // One *Func per ENTRY. Go's *Func points into the pclntab at its function, so FuncForPC answers
    // the same pointer for every pc in it, and symtab_test.go walks `for FuncForPC(pc) == f { pc++ }`
    // to the function's end; a fresh box per call made that loop exit at once. This host's entry is a
    // call site's span (frameEntry), the unit Entry() already reports, so a *Func is equal to another
    // exactly when their Entry() is. Strong: the entries are the finite set of call sites and function
    // tokens this process has named.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<uintptr, ж<Func>> s_funcHandles = new();

    // FuncForPC returns a *Func describing the function the token names, or nil when the token
    // names nothing this host can resolve — which is Go's own answer for a pc in no function.
    //
    // The box is a HandleBox: Func is zero-size, and a plain box of it would be the zerobase, which
    // every zero-size allocation shares (golib's GoZeroBase), making every *Func equal to every other.
    public static ж<Func> FuncForPC(uintptr pc)
    {
        string? name = managedFuncName(pc);

        if (string.IsNullOrEmpty(name))
            return default!;

        return s_funcHandles.GetOrAdd(frameEntry(pc), static (entry, funcName) =>
        {
            ж<Func> box = new HandleBox<Func>(new Func());
            s_funcRecords.Add(box, new FuncRecord { Name = funcName, Pc = entry });
            return box;
        }, name!);
    }

    // Name returns the Go spelling recorded when the *Func was minted. A Func this host did not
    // mint carries no record and answers "", exactly as Go's Name() does for a nil *Func.
    public static @string Name(this ж<Func> Ꮡf)
    {
        if (Ꮡf == nil)
            return ""u8;

        return s_funcRecords.TryGetValue(Ꮡf, out FuncRecord? record) ? (@string)record.Name : ""u8;
    }

    // Entry returns the start of the span holding the PC this *Func was minted from (the PC itself
    // for a token outside every span, such as a function value's). Go's Entry() names "the entry
    // address of the function"; this host has no addresses, only opaque per-call-site tokens
    // (the file header's standing doctrine), and a token already IS this host's answer to which
    // function a *Func names — the same identity Name() reads out of the same record. A Func
    // this host did not mint (Ꮡf == nil, or a box with no record — there should be none minted
    // any other way) answers 0, matching Go's zero-entry case for an unresolved Func.
    public static uintptr Entry(this ж<Func> Ꮡf)
    {
        if (Ꮡf == nil)
            return 0;

        return s_funcRecords.TryGetValue(Ꮡf, out FuncRecord? record) ? record.Pc : 0;
    }

    // FileLine returns the Go position recorded for pc, exactly as Callers()/Frames.Next() already
    // resolve it — a CallerFrameRecord keyed directly by the token, with no per-function line
    // table to walk (each call site is its own token here, unlike Go's linker-built pclntab where
    // one function's *Func spans many pcs). Go's own doc is explicit that pc need not belong to f
    // ("anyone can call this function, and they might just be wrong about targetpc belonging to
    // f"), so this reads pc alone; the common case is a caller passing Ꮡf.Entry() straight back in,
    // which resolves because Entry() returns the start of the span FuncForPC minted Ꮡf from. No record
    // for pc answers Go's own no-position case: ("", 0).
    public static (@string @file, nint line) FileLine(this ж<Func> Ꮡf, uintptr pc)
    {
        @string @file = default!;

        if (Ꮡf == nil)
            return (@file, 0);

        CallerFrameRecord? record = callerFrameRecord(pc);

        if (record is null)
            return (@file, 0);

        @file = record.File;
        return (@file, record.Line);
    }

    // The two token kinds a pc can be in this host, tried in the order that costs least.
    private static string? managedFuncName(uintptr pc)
    {
        // A Callers()/callers() PC: the record already holds the Go spelling goFrameName produced.
        if (callerFrameRecord(pc) is CallerFrameRecord record && record.Function.Length > 0)
            return record.Function;

        // A reflect Value.Pointer() token: resolves to the object the token named. For a function
        // value that is the delegate, whose MethodInfo is what goFrameName spells.
        object? referent = ManagedPointerTokens.Resolve((nuint)pc);

        while (referent is IInterfaceAdapter { Value: not null } adapter)
            referent = adapter.Value;

        if (referent is Delegate d)
            return goFrameName(d.Method, null);

        // THE FALLBACK, and it is why this increment is two changes rather than one. A delegate's
        // token is now derived from its TARGET METHOD, so two method values of one method share a
        // token — and the box table above holds WEAK references, so the delegate that happened to
        // register last can be collected while another sharing the token is still live. Resolve then
        // answers nothing, and answering "" here is the exact failure that once made Go's testing
        // package renumber its subtests: one naming gap became 83 orphaned comparison rows that read
        // as 83 defects.
        //
        // The method is remembered strongly beside the weak box precisely so this question does not
        // depend on any delegate surviving. That makes the seam better than it was before the
        // collapse rather than merely as good: today a collected function value is unrecoverable, and
        // after this it is not.
        if (ManagedPointerTokens.ResolveDelegateMethod((nuint)pc) is {} method)
            return goFrameName(method, null);

        return null;
    }

    // ---- the guard's view (RuntimeCallerPCSpanTests): GolibTests is outside runtime's
    //      InternalsVisibleTo grant, and its own methods are not Go frames, so this Go-prefixed
    //      public helper owns the two call sites the guard needs ----

    /// <summary>Returns the PCs <c>Callers(1, ...)</c> records at two DISTINCT call sites in this
    /// one function: the frame each names is this probe, at a different site.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static (uintptr first, uintptr second) GoCallerSitesProbe()
    {
        slice<uintptr> a = new slice<uintptr>(1);
        slice<uintptr> b = new slice<uintptr>(1);
        Callers(1, a);
        Callers(1, b);
        return (a[0], b[0]);
    }

    /// <summary>Returns the first and last value the caller-span band can ever hold: the start of span 0
    /// and the end of span <c>int.MaxValue</c>, the largest index the record list can reach.</summary>
    public static (uintptr first, uintptr last) GoCallerSpanBand()
    {
        return (callerSpanStart(0), callerSpanStart(int.MaxValue) + (((nuint)1 << CallerSpanShift) - 1));
    }

    /// <summary>Records <c>Callers(1, ...)</c> two frames deep beneath a converted linkname forwarder's
    /// shape: this probe calls a [StackTraceHidden] forwarder, which calls the recording function. Go
    /// has no frame for a forwarder, so the second frame must name this probe.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static slice<uintptr> GoForwardedCallersProbe()
    {
        return goHiddenForwarderProbe();
    }

    [System.Diagnostics.StackTraceHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static slice<uintptr> goHiddenForwarderProbe()
    {
        return goCallersRecordingProbe();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static slice<uintptr> goCallersRecordingProbe()
    {
        slice<uintptr> pc = new slice<uintptr>(2);
        Callers(1, pc);
        return pc;
    }

    /// <summary>Renders the crash traceback of a panic raised beneath a [StackTraceHidden] forwarder
    /// probe, as an unrecovered panic's report prints it. Go has no frame for a forwarder, so the
    /// traceback must name the raising function and this probe, and never the forwarder.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GoForwardedPanicTraceProbe()
    {
        try
        {
            goHiddenPanicForwarderProbe();
        }
        catch (PanicException panic)
        {
            return crashTraceback(panic, panic);
        }

        return string.Empty;
    }

    [System.Diagnostics.StackTraceHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void goHiddenPanicForwarderProbe()
    {
        goPanicRaisingProbe();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void goPanicRaisingProbe()
    {
        throw panic((@string)"probe");
    }

    // ---- the guard's view (GolibTests RuntimeStopTheWorldContractTests) ----

    /// <summary>
    /// Two stop/start-the-world pairs in sequence, each on its own goroutine, the shape of
    /// TestDebugLog followed by TestDebugLogInterleaving. Returns the first pair's failure by name,
    /// if it had one, and whether the second pair got through worldsema within the timeout: a
    /// first pair that dies while holding worldsema leaves the second parked for ever.
    /// </summary>
    public static (string? firstFailure, bool secondCompleted) GoStopTheWorldTwiceProbe(int timeoutMs)
    {
        string? firstFailure = null;

        using (ManualResetEventSlim first = new(false))
        {
            Goroutine.Start(() =>
            {
                try
                {
                    worldStop stw = stopTheWorld(stwUnknown);
                    startTheWorld(stw);
                }
                catch (Exception ex)
                {
                    firstFailure = $"{ex.GetType().Name}: {ex.Message}";
                }

                first.Set();
            });

            if (!first.Wait(timeoutMs))
                return ("the first stop-the-world never returned", false);
        }

        ManualResetEventSlim second = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                worldStop stw = stopTheWorld(stwUnknown);
                startTheWorld(stw);
            }
            catch (Exception)
            {
                // A failure of the second pair still returned, which is not what this watches.
            }

            second.Set();
        });

        // The event is not disposed: on a leak the parked goroutine still holds it.
        return (firstFailure, second.Wait(timeoutMs));
    }

    /// <summary>
    /// One stop/start-the-world pair on the calling thread, with a GC reason (stwGCMarkTerm) or an
    /// other one (stwUnknown). What it raises propagates.
    /// </summary>
    public static void GoStopTheWorldPair(bool gcReason)
    {
        worldStop stw = stopTheWorld(gcReason ? stwGCMarkTerm : stwUnknown);
        startTheWorld(stw);
    }

    /// <summary>
    /// The four /sched/pauses sample counts, read the way runtime/metrics reads them: each
    /// histogram written into a metric value, every bucket summed (underflow and overflow included),
    /// which is exactly what TestSchedPauseMetrics' sampleCount adds up.
    /// </summary>
    public static (uint64 stoppingGC, uint64 stoppingOther, uint64 totalGC, uint64 totalOther) GoStwPauseSampleCounts()
    {
        // metrics.Read's own order: the metrics lock, then initMetrics (which sets timeHistBuckets,
        // the bucket boundaries every time histogram writes against), then the reads.
        metricsLock();

        try
        {
            initMetrics();

            return (count(Ꮡsched.of(schedt.ᏑstwStoppingTimeGC)), count(Ꮡsched.of(schedt.ᏑstwStoppingTimeOther)),
                    count(Ꮡsched.of(schedt.ᏑstwTotalTimeGC)), count(Ꮡsched.of(schedt.ᏑstwTotalTimeOther)));
        }
        finally
        {
            metricsUnlock();
        }

        static uint64 count(ж<timeHistogram> h)
        {
            ж<metricValue> value = Ꮡ(new metricValue(nil));
            h.write(value);
            ж<metricFloat64Histogram> hist = (ж<metricFloat64Histogram>)(uintptr)value.Value.pointer;
            uint64 n = 0;

            slice<uint64> counts = hist.Value.counts;

            for (nint i = 0; i < len(counts); i++)
                n += counts[i];

            return n;
        }
    }

    /// <summary>
    /// flushallmcaches, as ReadMemStatsSlow, ReadMetricsSlow and readmemstats_m reach it inside their
    /// stopped world. Returns what it raised by type and message, or null if it returned.
    /// </summary>
    public static string? GoFlushAllMcachesProbe()
    {
        try
        {
            flushallmcaches();
            return null;
        }
        catch (Exception ex)
        {
            return $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    /// <summary>
    /// THE LEAK ARM. A goroutine stops the world and then panics INSIDE the stopped-world region,
    /// under a frame that recovers, the way a test body recovers what its callee raised. Then a
    /// second goroutine stops and starts the world. Returns whether the region was entered, what the
    /// first goroutine recovered, and whether the second pair got through worldsema within the
    /// timeout. Go would throw "panic during preemptoff" here; the managed contract releases
    /// worldsema as the panic leaves the region, so the next stop does not park on it.
    /// </summary>
    public static (bool regionEntered, string? recovered, bool secondCompleted) GoStopTheWorldRegionPanicProbe(int timeoutMs)
    {
        bool regionEntered = false;
        string? recovered = null;

        using (ManualResetEventSlim first = new(false))
        {
            Goroutine.Start(() =>
            {
                try
                {
                    GoFrame frame = default;

                    try
                    {
                        frame.Push(() => recovered = recover()?.ToString());
                        stopTheWorld(stwUnknown);
                        regionEntered = true;
                        throw panic((@string)"a panic inside a stop-the-world region");
                    }
                    catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
                    {
                        GoFrame.Capture(p);
                    }
                    finally
                    {
                        frame.Run();
                    }
                }
                catch (Exception ex)
                {
                    recovered ??= $"escaped: {ex.GetType().Name}: {ex.Message}";
                }

                first.Set();
            });

            if (!first.Wait(timeoutMs))
                return (regionEntered, "the first goroutine never finished", false);
        }

        ManualResetEventSlim second = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                worldStop stw = stopTheWorld(stwUnknown);
                startTheWorld(stw);
            }
            catch (Exception)
            {
                // A failure of the second pair still returned, which is not what this watches.
            }

            second.Set();
        });

        bool secondCompleted = second.Wait(timeoutMs);

        // On a leak, release the permit the panic left held so the parked goroutine finishes and a
        // red arm does not park every later stop in the test host. The reading is taken first.
        if (!secondCompleted)
            semrelease(Ꮡworldsema);

        return (regionEntered, recovered, secondCompleted);
    }

    /// <summary>
    /// THE metricsSema LEAK ARM, the same shape: a goroutine takes metricsLock and then panics
    /// INSIDE the region under a recovering frame; a second goroutine then takes and releases
    /// metricsLock. The 2026-09-26 park came from this lock (readMetricsLocked threw while holding
    /// it). Returns whether the region was entered, what was recovered, and whether the second
    /// goroutine got the lock within the timeout.
    /// </summary>
    public static (bool regionEntered, string? recovered, bool secondCompleted) GoMetricsRegionPanicProbe(int timeoutMs)
    {
        bool regionEntered = false;
        string? recovered = null;

        using (ManualResetEventSlim first = new(false))
        {
            Goroutine.Start(() =>
            {
                try
                {
                    GoFrame frame = default;

                    try
                    {
                        frame.Push(() => recovered = recover()?.ToString());
                        metricsLock();
                        regionEntered = true;
                        throw panic((@string)"a panic inside a metricsSema region");
                    }
                    catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
                    {
                        GoFrame.Capture(p);
                    }
                    finally
                    {
                        frame.Run();
                    }
                }
                catch (Exception ex)
                {
                    recovered ??= $"escaped: {ex.GetType().Name}: {ex.Message}";
                }

                first.Set();
            });

            if (!first.Wait(timeoutMs))
                return (regionEntered, "the first goroutine never finished", false);
        }

        ManualResetEventSlim second = new(false);

        Goroutine.Start(() =>
        {
            metricsLock();
            metricsUnlock();
            second.Set();
        });

        bool secondCompleted = second.Wait(timeoutMs);

        // On a leak, release the permit the panic left held (see the worldsema arm above).
        if (!secondCompleted)
            metricsUnlock();

        return (regionEntered, recovered, secondCompleted);
    }

    /// <summary>
    /// readMetricsLocked as ReadMetricsSlow (export_test) reaches it: the metrics lock and
    /// initMetrics, then the RAW ADDRESS of the caller's []runtime/metrics.Sample backing store.
    /// What it raises propagates; the lock is released either way, so a red arm leaks nothing.
    /// </summary>
    public static void GoReadMetricsLockedProbe(@unsafe.Pointer samplesp, nint len, nint cap)
    {
        metricsLock();

        try
        {
            initMetrics();
            readMetricsLocked(samplesp, len, cap);
        }
        finally
        {
            metricsUnlock();
        }
    }

    /// <summary>
    /// The shape of runtime's TestDebugLog without its ResetDebugLog: one record written through a
    /// debug logger, then the dump DumpDebugLog takes (printDebugLogImpl into the goroutine's
    /// writebuf). Returns the dump. What it raises propagates.
    /// </summary>
    public static string GoDebugLogRoundTripProbe(string text)
    {
        dlogImpl().s(text).end();

        ж<g> gp = getg();
        gp.Value.writebuf = new slice<byte>(0, 1 << 20);

        try
        {
            printDebugLogImpl();
            return ((@string)gp.Value.writebuf).ToString();
        }
        finally
        {
            gp.Value.writebuf = default!;
        }
    }

    // ---- the guard's view (GolibTests RuntimeGCPacerKnobTests) ----

    /// <summary>
    /// The GC pacer's two knobs as the runtime holds them (gcController.gcPercent and memoryLimit, what
    /// /gc/gogc:percent and /gc/gomemlimit:bytes read), beside what readGOGC and readGOMEMLIMIT compute
    /// from the runtime's environment snapshot now.
    /// </summary>
    public static (int32 gcPercent, int64 memoryLimit, int32 fromGOGC, int64 fromGOMEMLIMIT) GoGCPacerKnobsProbe() =>
        (ᏑgcController.of(gcControllerState.ᏑgcPercent).Load(), ᏑgcController.of(gcControllerState.ᏑmemoryLimit).Load(),
         readGOGC(), readGOMEMLIMIT());

    /// <summary>
    /// Runs the pacer's startup step (gcinitController) over a SUBSTITUTED environment snapshot, then
    /// <paramref name="body"/>. The runtime's environment snapshot and the whole gcController are
    /// restored afterwards, whatever the body does.
    /// </summary>
    public static void GoGCPacerFromEnvironmentProbe(string[] environment, Action body)
    {
        slice<@string> savedEnvs = envs;
        gcControllerState savedController = gcController;

        try
        {
            slice<@string> snapshot = new slice<@string>(environment.Length);

            for (int i = 0; i < environment.Length; i++)
                snapshot[i] = environment[i];

            envs = snapshot;
            gcinitController();
            body();
        }
        finally
        {
            envs = savedEnvs;
            gcController = savedController;
        }
    }

    // ---- the guard's view (GolibTests RuntimeSchedZeroValueTests) ----

    /// <summary>
    /// The lengths of the count arrays of the five time histograms Go's schedt embeds by value
    /// (timeToRun and the four stop-the-world ones). Go's zero value holds each as a zeroed
    /// [timeHistNumBuckets*timeHistNumSubBuckets]atomic.Uint64; a zero length here means sched was
    /// built as default(schedt), which skips the field initializers that allocate those arrays, and a
    /// record into any of them indexes out of range.
    /// </summary>
    public static (nint timeToRun, nint stwStoppingGC, nint stwStoppingOther, nint stwTotalGC, nint stwTotalOther) GoSchedHistogramLengths() =>
        (len(sched.timeToRun.counts), len(sched.stwStoppingTimeGC.counts), len(sched.stwStoppingTimeOther.counts),
         len(sched.stwTotalTimeGC.counts), len(sched.stwTotalTimeOther.counts));

    /// <summary>
    /// Go's timeHistNumBuckets * timeHistNumSubBuckets: the length every one of those arrays has in Go.
    /// </summary>
    public static nint GoTimeHistogramLength => timeHistNumBuckets * timeHistNumSubBuckets;

    /// <summary>
    /// GolibTests' probe for shrinkstack's refusal (RuntimeHostFatalRefusalTests): shrinks the
    /// calling goroutine's stack, as runtime's ShrinkStackAndVerifyFramePointers export does, and
    /// returns what it raised, or null if it returned.
    /// </summary>
    public static string? GoShrinkstackRefusalProbe(int timeoutMs) =>
        RunRefusalProbe(timeoutMs, "shrinkstack", () => shrinkstack(getg()));

    private static string? RunRefusalProbe(int timeoutMs, string name, Action call)
    {
        string? failure = $"{name} never returned";

        using ManualResetEventSlim returned = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                call();
                failure = null;
            }
            catch (Exception ex)
            {
                failure = $"{ex.GetType().Name}: {ex.Message}";
            }

            returned.Set();
        });

        returned.Wait(timeoutMs);
        return failure;
    }

    /// <summary>The address range of this runtime's Go TEXT: every program counter it hands to Go code
    /// lies inside it -- caller tokens from the caller-span band's base, function PCs in GoSyntheticPC's
    /// band above it. runtime/pprof reports it as the executable's text mapping (proto_*_impl.cs), the
    /// role Go's own text segment plays, so a profile location built from one of these PCs has a
    /// mapping. <c>end</c> is exclusive, so the single address <c>ulong.MaxValue</c> is outside.</summary>
    public static (ulong start, ulong end) GoSyntheticTextRange()
    {
        return ((ulong)s_callerSpanBase, ulong.MaxValue);
    }

    /// <summary>
    /// GolibTests' probe for traceMap (RuntimeTraceMapTests): puts each string twice into a fresh
    /// traceMap, as runtime's TraceMap export does, resets it, and repeats. Returns one line per put
    /// ("s=id,inserted"), or what the puts raised.
    /// </summary>
    public static string GoTraceMapProbe(string[] values, int rounds)
    {
        StringBuilder reading = new();

        try
        {
            ж<traceMap> tab = @new<traceMap>();

            for (int round = 0; round < rounds; round++)
            {
                for (int pass = 0; pass < 2; pass++)
                {
                    foreach (string value in values)
                    {
                        @string s = value;
                        (uint64 id, bool inserted) = tab.put(@unsafe.Pointer.FromPinnedBox(@unsafe.StringData(s)), (uintptr)len(s));
                        reading.Append($"{value}={id},{inserted};");
                    }
                }

                tab.reset();
            }
        }
        catch (Exception ex)
        {
            reading.Append($"{ex.GetType().Name}: {ex.Message}");
        }

        return reading.ToString();
    }

    // rawstring (string.go) allocates storage for a new string; the returned string and byte slice both
    // refer to the same storage, which the caller fills through the slice and then drops. Go takes it
    // from mallocgc, which this host does not run (there is no Go heap), so the converted body died in
    // mallocgcTiny on an anonymous nil dereference. One managed byte[] backs both views: @string(byte[])
    // wraps its argument without copying, and no @string hands out a writable view of its bytes, so
    // the slice is the only writer. The storage is zeroed, which Go permits ("not zeroed" is what
    // callers may not rely on).
    internal static (@string s, slice<byte> b) rawstring(nint size)
    {
        byte[] backing = AllocationCounter.NewArray<byte>(size);
        return (new @string(backing), new slice<byte>(backing));
    }

    // GoRawstringProbe is the GolibTests seam for rawstring (GolibTests is outside the
    // InternalsVisibleTo grant). It fills the returned byte slice and reads the string back, which is
    // rawstring's contract: the string and the slice refer to the same storage.
    public static @string GoRawstringProbe(nint size, byte fill)
    {
        var (s, b) = rawstring(size);

        for (nint i = 0; i < size; i++)
            b[i] = fill;

        return s;
    }

    // efaceHash (alg.go) is the hash Go gives an interface value. Go's body hands nilinterhash a pointer
    // to the interface variable, which reads the eface and hashes its data word by the dynamic type.
    // Here the variable is a managed reference with no address, so the converted body died in the
    // arm-2a refusal ("*eface over 0x...") and took runtime's TestSmhasherAvalanche with it. This
    // hashes the dynamic VALUE instead, on Go's own rules for the part that is expressible: a nil
    // interface hashes to its seed, an unhashable dynamic type panics naming it (the topmost type, as
    // Go's comment asks), and the value is hashed as c1 * typehash(t, value, seed ^ c0).
    //
    // typehash over a managed value covers the two shapes whose memory is the value itself: a
    // regular-memory kind (bool, the integers, uintptr) hashed over its own bytes at Go's size (the
    // memhash32/memhash64/memhash split typehash applies), and a string hashed over its content
    // (strhash is memhash over the bytes). Every other kind (floats, complex, arrays, structs,
    // nested interfaces, pointers) needs a walk over fields or an address this host does not have,
    // and is refused by name below instead of hashed to a number that would look real.
    //
    // ifaceHash stays converted: its signature lifts a dynamic interface (interface{ F() }) whose
    // GoDynamicTypeLift record the converter publishes only from a converted declaration, and the
    // -tests conversion of export_test.go resolves IfaceHash's type through that record.
    internal static uintptr efaceHash(any i, uintptr seed) => interfaceValueHash(i, seed);

    private sealed class RawBoxData
    {
        public byte Data;
    }

    private static uintptr interfaceValueHash(object? v, uintptr h)
    {
        if (v is null)
            return h;

        ж<_type> Ꮡt = abi.TypeOf(v);

        if ((~Ꮡt).Equal == default!)
            throw panic(((errorString)("hash of unhashable type "u8 + toRType(Ꮡt).@string())));

        return c1 * dynamicValueHash(Ꮡt, v, (uintptr)(h ^ c0));
    }

    private static uintptr dynamicValueHash(ж<_type> Ꮡt, object v, uintptr h)
    {
        // The value the descriptor describes: an adapter carrier unwraps to the Go value it stands for,
        // as abi.TypeOf's own classification did.
        while (v is IInterfaceAdapter { Value: not null } carrier)
            v = carrier.Value;

        if (v is IValueAdapter { Value: not null } valueAdapter)
            v = valueAdapter.Value;

        abi.ΔKind kind = (abi.ΔKind)(Ꮡt.Value.Kind_ & abi.KindMask);

        if (kind >= abi.Bool && kind <= abi.Uintptr)
        {
            // Regular memory: the payload of the boxed value type IS the value, at its Go size.
            int size = (int)Ꮡt.Value.Size_;

            if (!v.GetType().IsValueType || size == 0)
                throw panic("runtime: efaceHash: a " + kind + " value of dynamic type " + v.GetType().FullName + " is not a boxed value the managed host can hash");

            byte[] bytes = new byte[size];
            MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<RawBoxData>(v).Data, size).CopyTo(bytes);
            return hashManagedBytes(bytes, h);
        }

        if (kind == abi.ΔString && v is @string s)
        {
            byte[] bytes = new byte[len(s)];
            s.ToSpan().CopyTo(bytes);
            return hashManagedBytes(bytes, h);
        }

        throw panic("runtime: efaceHash: hashing a " + kind + " value is not implemented by the managed host");
    }

    // hashManagedBytes hashes a byte[] through the retained-box route memhash accepts (hash_impl.cs, rule 2:
    // a pointer minted from an element of a managed byte[]), with the memhash32/memhash64 split typehash
    // applies to a 4- or 8-byte regular-memory type. An empty array has no element to point at, so a
    // one-byte stand-in carries the pointer for the zero-length hash.
    private static uintptr hashManagedBytes(byte[] bytes, uintptr h)
    {
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(@unsafe.StringData(new @string(bytes.Length == 0 ? new byte[1] : bytes)));

        return bytes.Length switch
        {
            4 => memhash32(p, h),
            8 => memhash64(p, h),
            _ => memhash(p, h, (uintptr)(nuint)bytes.Length)
        };
    }

    // GoEfaceHashProbe is the GolibTests seam for efaceHash (GolibTests is outside the
    // InternalsVisibleTo grant): the hash Go gives an interface value, as export_test.go's EfaceHash does.
    public static uintptr GoEfaceHashProbe(any i, uintptr seed) => efaceHash(i, seed);
}
