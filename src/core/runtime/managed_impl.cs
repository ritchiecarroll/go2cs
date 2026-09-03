// managed_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
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
//   - LockOSThread/UnlockOSThread are no-ops BY CONSTRUCTION, not by omission: go2cs runs each
//     goroutine on its own managed thread, so the guarantee they exist to provide — "this
//     goroutine will not be migrated to another OS thread" — already holds unconditionally.
//   - Callers()/callers()/Frames.Next() walk the MANAGED stack projected to GO-LOGICAL frames:
//     only converted Go declarations and function literals count — adapter shells (IGoAdapter) and
//     go2cs-gen forwarders are dispatch plumbing Go has no frame for, and golib/the BCL/the test
//     host are not Go code. RELATIVE depths between two Callers calls on one goroutine therefore
//     match Go's logical model (io's multiReader flatten tests assert exactly this); ABSOLUTE
//     depth reflects the managed host's own frames below main. PC values are opaque
//     process-lifetime tokens, never addresses; Frame.Function is the Go spelling (goFrameName);
//     Frame.File/Line name the GO position the conversion recorded for that frame, and the
//     converted `.cs` position where it recorded none (goFramePosition). FuncForPC
//     and Frame.Func stayed unimplemented/nil while a *Func had no managed referent; that
//     premise EXPIRED when ManagedPointerTokens landed, and FuncForPC/Func.Name are managed
//     below as of 2026-08-29, joined by Func.Entry/Func.FileLine as of 2026-09-02 (Frame.Func is
//     still nil -- not a token this host mints). getcallersp
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
using System.Text;
using System.Threading;
using go.golib;
// The plain namespace using is what brings internal/runtime/atomic's [GoRecv] extension methods
// (Int64.Load and friends) into scope — an alias alone does not participate in extension lookup.
using go.@internal.runtime;

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

    // GOMAXPROCS' remembered setting. Go's starts at NumCPU.
    private static nint s_gomaxprocs = Environment.ProcessorCount;

    // GOMAXPROCS sets the maximum number of CPUs that can be executing simultaneously and returns
    // the previous setting. If n < 1, it does not change the current setting.
    public static nint GOMAXPROCS(nint n)
    {
        nint previous = Volatile.Read(ref s_gomaxprocs);

        if (n >= 1)
            Volatile.Write(ref s_gomaxprocs, n);

        return previous;
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
        System.GC.Collect(System.GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect(System.GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

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
    }

    // metricsLock/metricsUnlock protect the runtime metrics table (initMetrics' map and the agg
    // scratch state) for readMetrics (behind runtime/metrics.Read) and readMetricNames (behind
    // the runtime/metrics_test push). Go's bodies acquire metricsSema — a runtime sleeping
    // semaphore whose acquire path is getg() → sudog → gopark, none of which exists under the
    // CLR — with handoff enabled because metrics operations are long. The contract is mutual
    // exclusion with waiter handoff, and SemaphoreSlim is the CLR's spelling of exactly that:
    // FIFO-ish waiter wakeup, no thread affinity (a goroutine IS a managed thread here, but the
    // lock/unlock pair need not run on one thread for the semaphore to be correct, matching Go).
    // Everything the lock protects stays auto-converted.
    private static readonly SemaphoreSlim s_metricsSema = new(1, 1);

    internal static void metricsLock() => s_metricsSema.Wait();

    internal static void metricsUnlock() => s_metricsSema.Release();

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
    public static nint NumGoroutine()
    {
        return Goroutine.Count;
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
        return (int32)Goroutine.Count;
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
    // The pointer column is the metricValue.pointer word as-is (histogram kinds put a runtime
    // histogram's address there). It crosses as the same opaque address the Go form would carry
    // and is exactly as (un)readable on the other side — Value.Float64Histogram()'s reinterpret
    // is a pre-existing limitation of the address model, not something this shim changes.
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
    public static void readMetricsManaged(slice<@string> names, slice<nint> kinds, slice<uint64> scalars, slice<unsafe_package.Pointer> pointers)
    {
        metricsLock();

        // Ensure the map is initialized.
        initMetrics();

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
            pointers[i] = value.pointer;
        }

        metricsUnlock();
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
    public static nint Stack(slice<byte> buf, bool all)
    {
        StringBuilder trace = new();

        // The CALLING goroutine, always first and always with real frames — Go dumps the current
        // goroutine ahead of the rest, and this is the one stack the CLR can walk.
        Goroutine? current = Goroutine.Current;

        appendGoroutineHeader(trace, current);
        appendGoFrames(trace, new StackTrace(skipFrames: 1, fNeedFileInfo: true));

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

                trace.Append('\n');
                appendGoroutineHeader(trace, goroutine);
                trace.Append(ForeignStackPlaceholder);
            }
        }

        byte[] encoded = Encoding.UTF8.GetBytes(trace.ToString());
        nint count = Math.Min((nint)encoded.Length, len(buf));

        for (nint i = 0; i < count; i++)
            buf[i] = encoded[i];

        return count;
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
    private static void appendGoFrames(StringBuilder trace, StackTrace stack)
    {
        foreach (StackFrame frame in stack.GetFrames())
        {
            System.Reflection.MethodBase? method = frame.GetMethod();

            if (method is null)
                continue;

            trace.Append(goFrameName(method, frame)).Append("()\n");

            (string file, int line) = goFramePosition(method, frame);

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

        if (importPath.Length > internalTestSuffix.Length &&
            importPath.EndsWith(internalTestSuffix, StringComparison.Ordinal))
        {
            importPath = importPath[..^internalTestSuffix.Length];
        }

        string name = method.Name;

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
                string? recorded = frame is null ? null : goFuncLiteralSuffix(method, frame);

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

    // One converted file's recorded position map.
    private sealed class GoPositionMapRecord(string goFile, string table, string funcLits = "")
    {
        private int[]? m_csLines;
        private int[]? m_goLines;
        private int[]? m_litStarts;
        private int[]? m_litEnds;
        private string[]? m_litSuffixes;
        private string? m_resolvedGoFile;

        // ResolveGoFile spells the recorded identity as an absolute path where the record is a bare
        // file name, which the converter writes when the Go source sits BESIDE the C# it emitted.
        // Rooting it against the C# file's own compile-time directory is what lets a converted user
        // program answer the rooted path Go answers, without a machine-specific path having been
        // baked into a committed artifact. The two other recorded forms — the GOROOT-relative form,
        // which always carries a separator, and an already-absolute path — are reported verbatim.
        public string ResolveGoFile(string csPath)
        {
            if (m_resolvedGoFile is not null)
                return m_resolvedGoFile;

            string resolved = goFile;

            if (goFile.Length > 0 && goFile.IndexOf('/') < 0 && !isRootedGoPath(goFile))
            {
                int separator = csPath.LastIndexOf('/');

                if (separator > 0)
                    resolved = string.Concat(csPath.AsSpan(0, separator + 1), goFile);
            }

            m_resolvedGoFile = resolved;
            return resolved;
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

            return best < 0 ? null : m_litSuffixes![best];
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
        private static bool isRootedGoPath(string path)
        {
            if (path.Length > 0 && path[0] == '/')
                return true;

            return path.Length > 1 && path[1] == ':';
        }
    }

    // ReadMemStats populates m with memory allocator statistics.
    public static void ReadMemStats(ж<MemStats> Ꮡm)
    {
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
    }

    // LockOSThread wires the calling goroutine to its current operating system thread.
    public static void LockOSThread()
    {
        // Already true by construction — a goroutine IS a managed thread here (see the header).
    }

    // UnlockOSThread undoes an earlier call to LockOSThread.
    public static void UnlockOSThread()
    {
    }

    // The runtime-internal variants, reached through syscall and startTemplateThread.
    internal static void lockOSThread()
    {
    }

    internal static void unlockOSThread()
    {
    }

    // Pinner.Pin pins a Go object so its address is stable for the duration of the pin. The
    // guarantee it exists to provide — "the object will not be moved or freed while pinned" —
    // is about handing addresses to non-GC-aware code; a golib pointer is a managed ж<T> box
    // the GC tracks through every move, so the contract already holds unconditionally and the
    // pin set is a no-op BY CONSTRUCTION (the same class as LockOSThread above). The auto body
    // walks the scheduler (acquirem → getg) and the span table (setPinned → findObject) —
    // machinery that does not exist here. internal/fmtsort's test init (runtime.Pinner over
    // channel addresses, issue #49431's address-ordering guard) is the demonstrated consumer.
    [GoRecv] public static void Pin(this ref Pinner Δp, any pointer)
    {
    }

    // Pinner.Unpin unpins all pinned objects of the Pinner (no-op: nothing was pinned).
    [GoRecv] public static void Unpin(this ref Pinner Δp)
    {
    }

    // ------- The traceback surface: Callers / callers (Caller's funnel) / Frames.Next -------

    // One converted-Go call site observed on the managed stack, resolved at intern time so a
    // later Frames walk needs no live StackFrame. Tokens are process-lifetime, like Go's program
    // counters, so a pc slice recorded by Callers stays resolvable by any later CallersFrames.
    private sealed class CallerFrameRecord
    {
        public string Function = string.Empty;
        public string File = string.Empty;
        public nint Line;
    }

    private static readonly object s_callerTableLock = new();
    private static readonly Dictionary<string, nuint> s_callerTokens = new();
    private static readonly List<CallerFrameRecord> s_callerRecords = new();

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
    // logical model — the property io's flatten tests assert (readDepth == myDepth+2) — while
    // ABSOLUTE depth reflects the managed host (see the header).
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
        nint remainingSkip = skip;
        nint count = 0;

        foreach (StackFrame frame in stack.GetFrames())
        {
            System.Reflection.MethodBase? method = frame.GetMethod();

            if (method is null || !isGoSourceFrame(method))
                continue;

            if (remainingSkip > 0)
            {
                remainingSkip--;
                continue;
            }

            if (count >= len(pc))
                break;

            pc[count] = internCallerFrame(method, frame);
            count++;
        }

        return count;
    }

    // Frames.Next expands the next recorded PC into a Frame. The auto body resolves PCs through
    // findfunc's linker-built funcInfo tables, which have no managed form; the records minted by
    // Callers carry the same answers (Function in Go's spelling, File, Line). A PC this runtime
    // never minted resolves like Go's !funcInfo.valid() — skipped, not fatal. Frame.Func stays
    // nil (allowed by contract: "may be nil for non-Go code"), and Entry mirrors PC — entry
    // points are not distinct from call sites in the token model.
    [GoRecv] public static (Frame frame, bool more) Next(this ref Frames ci)
    {
        while (len(ci.callers) > 0)
        {
            uintptr pcToken = ci.callers[0];
            ci.callers = ci.callers[1..];

            CallerFrameRecord? record = callerFrameRecord(pcToken);

            if (record is null)
                continue;

            Frame frame = new()
            {
                PC = pcToken,
                Function = record.Function,
                File = record.File,
                Line = record.Line,
                Entry = pcToken
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
    // go2cs machinery: a generated adapter (IGoAdapter, dispatch plumbing) or a go2cs-gen
    // synthesized member (RecvGenerator's ж-forwarders carry [GeneratedCode("go2cs-gen", …)]).
    // Everything outside a package class — golib, the BCL, the test-host runtime — is not Go
    // code and never counts.
    private static bool isGoSourceFrame(System.Reflection.MethodBase method)
    {
        Type? declaring = method.DeclaringType;

        if (declaring is null)
            return false;

        if (typeof(IGoAdapter).IsAssignableFrom(declaring))
            return false;

        foreach (object attribute in method.GetCustomAttributes(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute), inherit: false))
        {
            if (attribute is System.CodeDom.Compiler.GeneratedCodeAttribute generated && generated.Tool == "go2cs-gen")
                return false;
        }

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

            (string file, int line) = goFramePosition(method, frame);

            CallerFrameRecord record = new()
            {
                Function = goFrameName(method, frame),
                File = file,
                Line = line
            };

            s_callerRecords.Add(record);
            token = (nuint)s_callerRecords.Count; // index + 1 — 0 stays the invalid sentinel
            s_callerTokens[key] = token;
            return token;
        }
    }

    private static CallerFrameRecord? callerFrameRecord(uintptr token)
    {
        nuint value = token;

        lock (s_callerTableLock)
        {
            if (value == 0 || value > (nuint)s_callerRecords.Count)
                return null;

            return s_callerRecords[(int)(value - 1)];
        }
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
    // both in the one mint site. Entry() returns that PC directly — this host's documented answer
    // to "what identifies this function" (PC values are opaque process-lifetime tokens, never
    // addresses; see the file header) — and FileLine(pc) resolves the SAME Go-position data
    // Callers()/Frames.Next() already serve, through callerFrameRecord. firstmoduledata and
    // Frame.Func are deliberately UNCHANGED by this arc: see docs/phase4/CENSUS-runtime-semantic-bill.md.
    private sealed class FuncRecord
    {
        public string Name = string.Empty;
        public uintptr Pc;
    }

    private static readonly ConditionalWeakTable<object, FuncRecord> s_funcRecords = new();

    // FuncForPC returns a *Func describing the function the token names, or nil when the token
    // names nothing this host can resolve — which is Go's own answer for a pc in no function.
    public static ж<Func> FuncForPC(uintptr pc)
    {
        string? name = managedFuncName(pc);

        if (string.IsNullOrEmpty(name))
            return default!;

        ж<Func> box = Ꮡ(new Func());
        s_funcRecords.Add(box, new FuncRecord { Name = name!, Pc = pc });
        return box;
    }

    // Name returns the Go spelling recorded when the *Func was minted. A Func this host did not
    // mint carries no record and answers "", exactly as Go's Name() does for a nil *Func.
    public static @string Name(this ж<Func> Ꮡf)
    {
        if (Ꮡf == nil)
            return ""u8;

        return s_funcRecords.TryGetValue(Ꮡf, out FuncRecord? record) ? (@string)record.Name : ""u8;
    }

    // Entry returns the PC token this *Func was minted from. Go's Entry() names "the entry
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
    // which resolves because Entry() returns exactly the token FuncForPC minted Ꮡf from. No record
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

        return referent is Delegate d ? goFrameName(d.Method, null) : null;
    }
}
