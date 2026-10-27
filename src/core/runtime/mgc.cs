// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Garbage collector (GC).
//
// The GC runs concurrently with mutator threads, is type accurate (aka precise), allows multiple
// GC threads to run in parallel. It is a concurrent mark and sweep that uses a write barrier. It is
// non-generational and non-compacting. Allocation is done using size segregated per P allocation
// areas to minimize fragmentation while eliminating locks in the common case.
//
// The algorithm decomposes into several steps.
// This is a high level description of the algorithm being used. For an overview of GC a good
// place to start is Richard Jones' gchandbook.org.
//
// The algorithm's intellectual heritage includes Dijkstra's on-the-fly algorithm, see
// Edsger W. Dijkstra, Leslie Lamport, A. J. Martin, C. S. Scholten, and E. F. M. Steffens. 1978.
// On-the-fly garbage collection: an exercise in cooperation. Commun. ACM 21, 11 (November 1978),
// 966-975.
// For journal quality proofs that these steps are complete, correct, and terminate see
// Hudson, R., and Moss, J.E.B. Copying Garbage Collection without stopping the world.
// Concurrency and Computation: Practice and Experience 15(3-5), 2003.
//
// 1. GC performs sweep termination.
//
//    a. Stop the world. This causes all Ps to reach a GC safe-point.
//
//    b. Sweep any unswept spans. There will only be unswept spans if
//    this GC cycle was forced before the expected time.
//
// 2. GC performs the mark phase.
//
//    a. Prepare for the mark phase by setting gcphase to _GCmark
//    (from _GCoff), enabling the write barrier, enabling mutator
//    assists, and enqueueing root mark jobs. No objects may be
//    scanned until all Ps have enabled the write barrier, which is
//    accomplished using STW.
//
//    b. Start the world. From this point, GC work is done by mark
//    workers started by the scheduler and by assists performed as
//    part of allocation. The write barrier shades both the
//    overwritten pointer and the new pointer value for any pointer
//    writes (see mbarrier.go for details). Newly allocated objects
//    are immediately marked black.
//
//    c. GC performs root marking jobs. This includes scanning all
//    stacks, shading all globals, and shading any heap pointers in
//    off-heap runtime data structures. Scanning a stack stops a
//    goroutine, shades any pointers found on its stack, and then
//    resumes the goroutine.
//
//    d. GC drains the work queue of grey objects, scanning each grey
//    object to black and shading all pointers found in the object
//    (which in turn may add those pointers to the work queue).
//
//    e. Because GC work is spread across local caches, GC uses a
//    distributed termination algorithm to detect when there are no
//    more root marking jobs or grey objects (see gcMarkDone). At this
//    point, GC transitions to mark termination.
//
// 3. GC performs mark termination.
//
//    a. Stop the world.
//
//    b. Set gcphase to _GCmarktermination, and disable workers and
//    assists.
//
//    c. Perform housekeeping like flushing mcaches.
//
// 4. GC performs the sweep phase.
//
//    a. Prepare for the sweep phase by setting gcphase to _GCoff,
//    setting up sweep state and disabling the write barrier.
//
//    b. Start the world. From this point on, newly allocated objects
//    are white, and allocating sweeps spans before use if necessary.
//
//    c. GC does concurrent sweeping in the background and in response
//    to allocation. See description below.
//
// 5. When sufficient allocation has taken place, replay the sequence
// starting with 1 above. See discussion of GC rate below.
// Concurrent sweep.
//
// The sweep phase proceeds concurrently with normal program execution.
// The heap is swept span-by-span both lazily (when a goroutine needs another span)
// and concurrently in a background goroutine (this helps programs that are not CPU bound).
// At the end of STW mark termination all spans are marked as "needs sweeping".
//
// The background sweeper goroutine simply sweeps spans one-by-one.
//
// To avoid requesting more OS memory while there are unswept spans, when a
// goroutine needs another span, it first attempts to reclaim that much memory
// by sweeping. When a goroutine needs to allocate a new small-object span, it
// sweeps small-object spans for the same object size until it frees at least
// one object. When a goroutine needs to allocate large-object span from heap,
// it sweeps spans until it frees at least that many pages into heap. There is
// one case where this may not suffice: if a goroutine sweeps and frees two
// nonadjacent one-page spans to the heap, it will allocate a new two-page
// span, but there can still be other one-page unswept spans which could be
// combined into a two-page span.
//
// It's critical to ensure that no operations proceed on unswept spans (that would corrupt
// mark bits in GC bitmap). During GC all mcaches are flushed into the central cache,
// so they are empty. When a goroutine grabs a new span into mcache, it sweeps it.
// When a goroutine explicitly frees an object or sets a finalizer, it ensures that
// the span is swept (either by sweeping it, or by waiting for the concurrent sweep to finish).
// The finalizer goroutine is kicked off only when all spans are swept.
// When the next GC starts, it sweeps all not-yet-swept spans (if any).
// GC rate.
// Next GC is after we've allocated an extra amount of memory proportional to
// the amount already in use. The proportion is controlled by GOGC environment variable
// (100 by default). If GOGC=100 and we're using 4M, we'll GC again when we get to 8M
// (this mark is computed by the gcController.heapGoal method). This keeps the GC cost in
// linear proportion to the allocation cost. Adjusting GOGC just changes the linear constant
// (and also the amount of extra memory used).
// Oblets
//
// In order to prevent long pauses while scanning large objects and to
// improve parallelism, the garbage collector breaks up scan jobs for
// objects larger than maxObletBytes into "oblets" of at most
// maxObletBytes. When scanning encounters the beginning of a large
// object, it scans only the first oblet and enqueues the remaining
// oblets as new scan jobs.
namespace go;

using cpu = @internal.cpu_package;
using goarch = @internal.goarch_package;
using goexperiment = @internal.goexperiment_package;
using atomic = @internal.runtime.atomic_package;
using gc = @internal.runtime.gc_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using System.Runtime.CompilerServices;

partial class runtime_package {

internal static UntypedInt _DebugGC => 0;

// concurrentSweep is a debug flag. Disabling this flag
// ensures all spans are swept while the world is stopped.
internal const bool concurrentSweep = true;

// debugScanConservative enables debug logging for stack
// frames that are scanned conservatively.
internal const bool debugScanConservative = false;

// sweepMinHeapDistance is a lower bound on the heap distance
// (in bytes) reserved for concurrent sweeping between GC
// cycles.
internal static UntypedInt sweepMinHeapDistance => /* 1024 * 1024 */ 1048576;

// heapObjectsCanMove always returns false in the current garbage collector.
// It exists for go4.org/unsafe/assume-no-moving-gc, which is an
// unfortunate idea that had an even more unfortunate implementation.
// Every time a new Go release happened, the package stopped building,
// and the authors had to add a new file with a new //go:build line, and
// then the entire ecosystem of packages with that as a dependency had to
// explicitly update to the new version. Many packages depend on
// assume-no-moving-gc transitively, through paths like
// inet.af/netaddr -> go4.org/intern -> assume-no-moving-gc.
// This was causing a significant amount of friction around each new
// release, so we added this bool for the package to //go:linkname
// instead. The bool is still unfortunate, but it's not as bad as
// breaking the ecosystem on every new release.
//
// If the Go garbage collector ever does move heap objects, we can set
// this to true to break all the programs using assume-no-moving-gc.
//
//go:linkname heapObjectsCanMove
public static bool heapObjectsCanMove() {
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sizeOfWorkbufIsˢ = "size of Workbuf is suboptimal"u8;

internal static void gcinit() {
    if (/* unsafe.Sizeof(workbuf{}) */ (uintptr)2048 != _WorkbufSize) {
        @throw(sizeOfWorkbufIsˢ);
    }
    // No sweep on the first cycle.
    ᏑΔsweep.of(sweepdata.Ꮡactive).of(activeSweep.Ꮡstate).Store(sweepDrainedMask);

    // Initialize GC pacer state.
    // Use the environment variable GOGC for the initial gcPercent value.
    // Use the environment variable GOMEMLIMIT for the initial memoryLimit value.
    ᏑgcController.init(readGOGC(), readGOMEMLIMIT());

    // Set up the cleanup block ptr mask.
    foreach (var i in range(cleanupBlockPtrMask)) {
        cleanupBlockPtrMask[i] = 0xff;
    }

    work.startSema = 1;
    work.markDoneSema = 1;
    Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlist).init(/* unsafe.Offsetof(spanSPMC{}.allnode) */ (uintptr)0);
    lockInit(ref work.sweepWaiters.@lock, lockRankSweepWaiters);
    lockInit(ref work.assistQueue.@lock, lockRankAssistQueue);
    lockInit(ref work.strongFromWeak.@lock, lockRankStrongFromWeakQueue);
    lockInit(ref work.wbufSpans.@lock, lockRankWbufSpans);
    lockInit(ref work.spanSPMCs.@lock, lockRankSpanSPMCs);
    lockInit(ref gcCleanups.@lock, lockRankCleanupQueue);
}

// gcenable is called after the bulk of the runtime initialization,
// just before we're about to start letting user code run.
// It kicks off the background sweeper goroutine, the background
// scavenger goroutine, and enables GC.
internal static partial void gcenable() {
    // Kick off sweeping and scavenging.
    var c = new channel<nint>(2);
    goǃ(bgsweep, c);
    goǃ(bgscavenge, c);
    ᐸꟷ(c);
    ᐸꟷ(c);
    memstats.enablegc = true; // now that runtime is initialized, GC is okay
}

// Garbage collector phase.
// Indicates to write barrier and synchronization task to perform.
internal static readonly ж<uint32> Ꮡgcphase = new StaticBox<uint32>(default(uint32));
internal static ref uint32 gcphase => ref ((StaticBox<uint32>)Ꮡgcphase).ValueSlot;

// The compiler knows about this variable.
// If you change it, you must change builtin/runtime.go, too.
// If you change the first four bytes, you must also change the write
// barrier insertion code.
//
// writeBarrier should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname writeBarrier
partial struct writeBarrierᴛ1 /*dyn*/ {
    internal bool enabled;    // compiler emits a check of this before calling write barrier
    internal array<byte> pad = new(3); // compiler uses 32-bit load for "enabled" field
    internal uint64 alignme;  // guarantee alignment so that compiler can use a 32 or 64-bit load
}
internal static writeBarrierᴛ1 writeBarrier = new();

// gcBlackenEnabled is 1 if mutator assists and background mark
// workers are allowed to blacken objects. This must only be set when
// gcphase == _GCmark.
internal static readonly ж<uint32> ᏑgcBlackenEnabled = new StaticBox<uint32>(default(uint32));
internal static ref uint32 gcBlackenEnabled => ref ((StaticBox<uint32>)ᏑgcBlackenEnabled).ValueSlot;

internal static UntypedInt _GCoff => iota; // GC not running; sweeping in background, write barrier disabled
internal static UntypedInt _GCmark => 1; // GC marking roots and workbufs: allocate black, write barrier ENABLED
internal static UntypedInt _GCmarktermination => 2; // GC mark termination: allocate black, P's help GC, write barrier ENABLED

//go:nosplit
internal static void setGCPhase(uint32 x) {
    atomic.Store(Ꮡgcphase, x);
    writeBarrier.enabled = gcphase == _GCmark || gcphase == _GCmarktermination;
}

// gcMarkWorkerMode represents the mode that a concurrent mark worker
// should operate in.
//
// Concurrent marking happens through four different mechanisms. One
// is mutator assists, which happen in response to allocations and are
// not scheduled. The other three are variations in the per-P mark
// workers and are distinguished by gcMarkWorkerMode.
partial struct gcMarkWorkerMode /*num:nint*/;

// gcMarkWorkerNotWorker indicates that the next scheduled G is not
// starting work and the mode should be ignored.
internal static gcMarkWorkerMode gcMarkWorkerNotWorker => /* iota */ 0;

// gcMarkWorkerDedicatedMode indicates that the P of a mark
// worker is dedicated to running that mark worker. The mark
// worker should run without preemption.
internal static gcMarkWorkerMode gcMarkWorkerDedicatedMode => 1;

// gcMarkWorkerFractionalMode indicates that a P is currently
// running the "fractional" mark worker. The fractional worker
// is necessary when GOMAXPROCS*gcBackgroundUtilization is not
// an integer and using only dedicated workers would result in
// utilization too far from the target of gcBackgroundUtilization.
// The fractional worker should run until it is preempted and
// will be scheduled to pick up the fractional part of
// GOMAXPROCS*gcBackgroundUtilization.
internal static gcMarkWorkerMode gcMarkWorkerFractionalMode => 2;

// gcMarkWorkerIdleMode indicates that a P is running the mark
// worker because it has nothing else to do. The idle worker
// should run until it is preempted and account its time
// against gcController.idleMarkTime.
internal static gcMarkWorkerMode gcMarkWorkerIdleMode => 3;

// gcMarkWorkerModeStrings are the strings labels of gcMarkWorkerModes
// to use in execution traces.
internal static array<@string> gcMarkWorkerModeStrings = new @string[]{
    "Not worker"u8,
    "GC (dedicated)"u8,
    "GC (fractional)"u8,
    "GC (idle)"u8
}.array();

// pollFractionalWorkerExit reports whether a fractional mark worker
// should self-preempt. It assumes it is called from the fractional
// worker.
internal static bool pollFractionalWorkerExit() {
    // This should be kept in sync with the fractional worker
    // scheduler logic in findRunnableGCWorker.
    var now = nanotime();
    var delta = now - gcController.markStartTime;
    if (delta <= 0) {
        return true;
    }
    var Δp = (~(~getg()).m).p.ptr();
    var selfTime = Δp.of(runtime_package.Δp.ᏑgcFractionalMarkTime).Load() + (now - (~Δp).gcMarkWorkerStartTime);
    // Add some slack to the utilization goal so that the
    // fractional worker isn't behind again the instant it exits.
    return (float64)selfTime / (float64)delta > 1.2D * gcController.fractionalUtilizationGoal;
}

internal static readonly ж<workType> Ꮡwork = new StaticBox<workType>(new workType(nil));
internal static ref workType work => ref ((StaticBox<workType>)Ꮡwork).ValueSlot;

partial struct workType_wbufSpans /*dyn*/ {
    internal mutex @lock;
    // free is a list of spans dedicated to workbufs, but
    // that don't currently contain any workbufs.
    internal mSpanList free;
    // busy is a list of all spans containing workbufs on
    // one of the workbuf lists.
    internal mSpanList busy;
}

partial struct workType_spanSPMCs /*dyn*/ {
    internal mutex @lock;
    internal listHeadManual list; // *spanSPMC
}

partial struct workType_goroutineLeak /*dyn*/ {
    // Once set, it indicates that the GC will perform goroutine leak detection during
    // the next GC cycle; it is set by goroutineLeakGC and unset during gcStart.
    internal atomic.Bool pending;
    // Once set, it indicates that the GC has started a goroutine leak detection run;
    // it is set during gcStart and unset during gcMarkTermination;
    //
    // Protected by STW.
    internal bool enabled;
    // Once set, it indicates that the GC has performed goroutine leak detection during
    // the current GC cycle; it is set during gcMarkDone, right after goroutine leak detection,
    // and unset during gcMarkTermination;
    //
    // Protected by STW.
    internal bool done;
    // The number of leaked goroutines during the last leak detection GC cycle.
    //
    // Write-protected by STW in findGoroutineLeaks.
    internal nint count;
}

partial struct workType_assistQueue /*dyn*/ {
    internal mutex @lock;
    internal gQueue q;
}

partial struct workType_sweepWaiters /*dyn*/ {
    internal mutex @lock;
    internal gList list;
}

partial struct workType_strongFromWeak /*dyn*/ {
    // block is a flag set during mark termination that prevents
    // new weak->strong conversions from executing by blocking the
    // goroutine and enqueuing it onto q.
    //
    // Mutated only by one goroutine at a time in gcMarkDone,
    // with globally-synchronizing events like forEachP and
    // stopTheWorld.
    internal bool block;
    // q is a queue of goroutines that attempted to perform a
    // weak->strong conversion during mark termination.
    //
    // Protected by lock.
    internal mutex @lock;
    internal gQueue q;
}

partial struct workType {
    internal lfstack full;          // lock-free list of full blocks workbuf
    internal cpu.CacheLinePad _; // prevents false-sharing between full and empty
    internal lfstack empty;          // lock-free list of empty blocks workbuf
    internal cpu.CacheLinePad __; // prevents false-sharing between empty and wbufSpans
    internal workType_wbufSpans wbufSpans;
    internal cpu.CacheLinePad ___; // prevents false-sharing between wbufSpans and spanWorkMask
    // spanqMask is a bitmap indicating which Ps have local work worth stealing.
    // Set or cleared by the owning P, cleared by stealing Ps.
    //
    // spanqMask is like a proxy for a global queue. An important invariant is that
    // forced flushing like gcw.dispose must set this bit on any P that has local
    // span work.
    internal pMask spanqMask;
    internal cpu.CacheLinePad ____; // prevents false-sharing between spanqMask and everything else
    // List of all spanSPMCs.
    //
    // Only used if goexperiment.GreenTeaGC.
    internal workType_spanSPMCs spanSPMCs;
// Restore 64-bit alignment on 32-bit.
// _ uint32

    // bytesMarked is the number of bytes marked this cycle. This
    // includes bytes blackened in scanned objects, noscan objects
    // that go straight to black, objects allocated as black during
    // the cycle, and permagrey objects scanned by markroot during
    // the concurrent scan phase.
    //
    // This is updated atomically during the cycle. Updates may be batched
    // arbitrarily, since the value is only read at the end of the cycle.
    //
    // Because of benign races during marking, this number may not
    // be the exact number of marked bytes, but it should be very
    // close.
    //
    // Put this field here because it needs 64-bit atomic access
    // (and thus 8-byte alignment even on 32-bit architectures).
    internal uint64 bytesMarked;
    internal atomic.Uint32 markrootNext; // next markroot job
    internal atomic.Uint32 markrootJobs; // number of markroot jobs
    internal uint32 nproc;
    internal int64 tstart;
    internal uint32 nwait;
    // Number of roots of various root types. Set by gcPrepareMarkRoots.
    //
    // During normal GC cycle, nStackRoots == nMaybeRunnableStackRoots == len(stackRoots);
    // during goroutine leak detection, nMaybeRunnableStackRoots is the number of stackRoots
    // scheduled for marking.
    // In both variants, nStackRoots == len(stackRoots).
    internal nint nDataRoots, nBSSRoots, nSpanRoots, nStackRoots, nMaybeRunnableStackRoots;
    // The following fields monitor the GC phase of the current cycle during
    // goroutine leak detection.
    internal workType_goroutineLeak goroutineLeak;
    // Base indexes of each root type. Set by gcPrepareMarkRoots.
    internal uint32 baseData, baseBSS, baseSpans, baseStacks, baseEnd;
    // stackRoots is a snapshot of all of the Gs that existed before the
    // beginning of concurrent marking.  During goroutine leak detection, stackRoots
    // is partitioned into two sets; to the left of nMaybeRunnableStackRoots are stackRoots
    // of running / runnable goroutines and to the right of nMaybeRunnableStackRoots are
    // stackRoots of unmarked / not runnable goroutines
    // The stackRoots array is re-partitioned after each marking phase iteration.
    internal slice<ж<g>> stackRoots;
    // Each type of GC state transition is protected by a lock.
    // Since multiple threads can simultaneously detect the state
    // transition condition, any thread that detects a transition
    // condition must acquire the appropriate transition lock,
    // re-check the transition condition and return if it no
    // longer holds or perform the transition if it does.
    // Likewise, any transition must invalidate the transition
    // condition before releasing the lock. This ensures that each
    // transition is performed by exactly one thread and threads
    // that need the transition to happen block until it has
    // happened.
    //
    // startSema protects the transition from "off" to mark or
    // mark termination.
    internal uint32 startSema;
    // markDoneSema protects transitions from mark to mark termination.
    internal uint32 markDoneSema;
    internal uint32 bgMarkDone; // cas to 1 when at a background mark completion point
// Background mark completion signaling

    // mode is the concurrency mode of the current GC cycle.
    internal gcMode mode;
    // userForced indicates the current GC cycle was forced by an
    // explicit user call.
    internal bool userForced;
    // initialHeapLive is the value of gcController.heapLive at the
    // beginning of this GC cycle.
    internal uint64 initialHeapLive;
    // assistQueue is a queue of assists that are blocked because
    // there was neither enough credit to steal or enough work to
    // do.
    internal workType_assistQueue assistQueue;
    // sweepWaiters is a list of blocked goroutines to wake when
    // we transition from mark termination to sweep.
    internal workType_sweepWaiters sweepWaiters;
    // strongFromWeak controls how the GC interacts with weak->strong
    // pointer conversions.
    internal workType_strongFromWeak strongFromWeak;
    // cycles is the number of completed GC cycles, where a GC
    // cycle is sweep termination, mark, mark termination, and
    // sweep. This differs from memstats.numgc, which is
    // incremented at mark termination.
    internal atomic.Uint32 cycles;
    // Timing/utilization stats for this cycle.
    internal int32 stwprocs, maxprocs;
    internal int64 tSweepTerm, tMark, tMarkTerm, tEnd; // nanotime() of phase start
    // pauseNS is the total STW time this cycle, measured as the time between
    // when stopping began (just before trying to stop Ps) and just after the
    // world started again.
    internal int64 pauseNS;
    // debug.gctrace heap sizes for this cycle.
    internal uint64 heap0, heap1, heap2;
    // Cumulative estimated CPU usage.
    /*embed*/ internal cpuStats cpuStats;
}

// go2cs generated this placeholder — func GC is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// goroutineLeakGC runs a GC cycle that performs goroutine leak detection.
//
//go:linkname goroutineLeakGC runtime/pprof.runtime_goroutineLeakGC
internal static void goroutineLeakGC() {
    // Set the pending flag to true, instructing the next GC cycle to
    // perform goroutine leak detection.
    Ꮡwork.of(workType.ᏑgoroutineLeak).of(workType_goroutineLeak.Ꮡpending).Store(true);

    // Spin GC cycles until the pending flag is unset.
    // This ensures that goroutineLeakGC waits for a GC cycle that
    // actually performs goroutine leak detection.
    //
    // This is needed in case multiple concurrent calls to GC
    // are simultaneously fired by the system, wherein some
    // of them are dropped.
    //
    // In the vast majority of cases, only one loop iteration is needed;
    // however, multiple concurrent calls to goroutineLeakGC could lead to
    // the execution of additional GC cycles.
    //
    // Examples:
    //
    // pending? |   G1                    | G2
    // ---------|-------------------------|-----------------------
    //     -    | goroutineLeakGC()       | goroutineLeakGC()
    //     -    | pending.Store(true)     | .
    //     X    | for pending.Load()      | .
    //     X    | GC()                    | .
    //     X    | > gcStart()             | .
    //     X    |   pending.Store(false)  | .
    // ...
    //     -    | > gcMarkDone()          | .
    //     -    |   .                     | pending.Store(true)
    // ...
    //     X    | > gcMarkTermination()   | .
    //     X    |   ...
    //     X    | < GC returns            | .
    //     X    | for pending.Load        | .
    //     X    | GC()                    | .
    //     X    | .                       | for pending.Load()
    //     X    | .                       | GC()
    // ...
    // The first to pick up the pending flag will start a
    // leak detection cycle.
    while (Ꮡwork.of(workType.ᏑgoroutineLeak).of(workType_goroutineLeak.Ꮡpending).Load()) {
        GC();
    }
}

// gcWaitOnMark blocks until GC finishes the Nth mark phase. If GC has
// already completed this mark phase, it returns immediately.
internal static void gcWaitOnMark(uint32 n) {
    while (ᐧ) {
        // Disable phase transitions.
        @lock(Ꮡwork.of(workType.ᏑsweepWaiters).of(workType_sweepWaiters.Ꮡlock));
        var nMarks = Ꮡwork.of(workType.Ꮡcycles).Load();
        if (gcphase != _GCmark) {
            // We've already completed this cycle's mark.
            nMarks++;
        }
        if (nMarks > n) {
            // We're done.
            unlock(Ꮡwork.of(workType.ᏑsweepWaiters).of(workType_sweepWaiters.Ꮡlock));
            return;
        }

        // Wait until sweep termination, mark, and mark
        // termination of cycle N complete.
        Ꮡwork.of(workType.ᏑsweepWaiters).of(workType_sweepWaiters.Ꮡlist).push(getg());
        goparkunlock(Ꮡwork.of(workType.ᏑsweepWaiters).of(workType_sweepWaiters.Ꮡlock), waitReasonWaitForGCCycle, traceBlockUntilGCEnds, 1);
    }
}

// gcMode indicates how concurrent a GC cycle should be.
partial struct gcMode /*num:nint*/;

internal static gcMode gcBackgroundMode => /* iota */ 0; // concurrent GC and sweep
internal static gcMode gcForceMode => 1;     // stop-the-world GC now, concurrent sweep
internal static gcMode gcForceBlockMode => 2; // stop-the-world GC now and STW sweep (forced by user)

// A gcTrigger is a predicate for starting a GC cycle. Specifically,
// it is an exit condition for the _GCoff phase.
partial struct gcTrigger {
    internal gcTriggerKind kind;
    internal int64 now;  // gcTriggerTime: current time
    internal uint32 n; // gcTriggerCycle: cycle number to start
}

partial struct gcTriggerKind /*num:nint*/;

// gcTriggerHeap indicates that a cycle should be started when
// the heap size reaches the trigger heap size computed by the
// controller.
internal static gcTriggerKind gcTriggerHeap => /* iota */ 0;

// gcTriggerTime indicates that a cycle should be started when
// it's been more than forcegcperiod nanoseconds since the
// previous GC cycle.
internal static gcTriggerKind gcTriggerTime => 1;

// gcTriggerCycle indicates that a cycle should be started if
// we have not yet started cycle number gcTrigger.n (relative
// to work.cycles).
internal static gcTriggerKind gcTriggerCycle => 2;

// test reports whether the trigger condition is satisfied, meaning
// that the exit condition for the _GCoff phase has been met. The exit
// condition should be tested when allocating.
internal static bool test(this gcTrigger t) {
    if (!memstats.enablegc || Ꮡpanicking.Load() != 0 || gcphase != _GCoff) {
        return false;
    }
    var exprᴛ1 = t.kind;
    if (exprᴛ1 == gcTriggerHeap) {
        var (trigger, _) = ᏑgcController.trigger();
        return ᏑgcController.of(gcControllerState.ᏑheapLive).Load() >= trigger;
    }
    if (exprᴛ1 == gcTriggerTime) {
        if (ᏑgcController.of(gcControllerState.ᏑgcPercent).Load() < 0) {
            return false;
        }
        var lastgc = (int64)atomic.Load64(Ꮡmemstats.of(mstats.Ꮡlast_gc_nanotime));
        return lastgc != 0 && t.now - lastgc > forcegcperiod;
    }
    if (exprᴛ1 == gcTriggerCycle) {
        // t.n > work.cycles, but accounting for wraparound.
        return (int32)(t.n - Ꮡwork.of(workType.Ꮡcycles).Load()) > 0;
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pMcacheNotFlushedˢ = "p mcache not flushed"u8;

// gcStart starts the GC. It transitions from _GCoff to _GCmark (if
// debug.gcstoptheworld == 0) or performs all of GC (if
// debug.gcstoptheworld != 0).
//
// This may return without performing this transition in some cases,
// such as when called on a system stack or with locks held.
internal static void gcStart(gcTrigger trigger) {
    GoFrame ᒐ = default;
    try {
        // Since this is called from malloc and malloc is called in
        // the guts of a number of libraries that might be holding
        // locks, don't attempt to start GC in non-preemptible or
        // potentially unstable situations.
        var mp = acquirem();
        {
            var gp = getg(); if (gp == (~mp).g0 || (~mp).locks > 1 || (~mp).preemptoff != ""u8) {
                releasem(ref (mp).DerefOrNull());
                return;
            }
        }
        releasem(ref (mp).DerefOrNull());
        mp = default!;

        {
            var gp = getg(); if ((~gp).bubble != nil) {
                // Disassociate the G from its synctest bubble while allocating.
                // This is less elegant than incrementing the group's active count,
                // but avoids any contamination between GC and synctest.
                var bubble = gp.Value.bubble;
                gp.Value.bubble = default!;
                var bubbleʗ1 = bubble;
                var gpʗ1 = gp;
                defer(() => {
                    gpʗ1.Value.bubble = bubbleʗ1;
                }, ref ᒐ);
            }
        }

        // Pick up the remaining unswept/not being swept spans concurrently
        //
        // This shouldn't happen if we're being invoked in background
        // mode since proportional sweep should have just finished
        // sweeping everything, but rounding errors, etc, may leave a
        // few spans unswept. In forced mode, this is necessary since
        // GC can be forced at any point in the sweeping cycle.
        //
        // We check the transition condition continuously here in case
        // this G gets delayed in to the next GC cycle.
        while (trigger.test() && sweepone() != ~(uintptr)0) {
        }

        // Perform GC initialization and the sweep termination
        // transition.
        semacquire(Ꮡwork.of(workType.ᏑstartSema));
        // Re-check transition condition under transition lock.
        if (!trigger.test()) {
            semrelease(Ꮡwork.of(workType.ᏑstartSema));
            return;
        }

        // In gcstoptheworld debug mode, upgrade the mode accordingly.
        // We do this after re-checking the transition condition so
        // that multiple goroutines that detect the heap trigger don't
        // start multiple STW GCs.
        gcMode mode = gcBackgroundMode;
        if (debug.gcstoptheworld == 1){
            mode = gcForceMode;
        } else 
        if (debug.gcstoptheworld == 2) {
            mode = gcForceBlockMode;
        }

        // Ok, we're doing it! Stop everybody else
        semacquire(Ꮡgcsema);
        semacquire(Ꮡworldsema);

        // For stats, check if this GC was forced by the user.
        // Update it under gcsema to avoid gctrace getting wrong values.
        work.userForced = trigger.kind == gcTriggerCycle;

        var Δtrace = traceAcquire();
        if (Δtrace.ok()) {
            Δtrace.GCStart();
            traceRelease(Δtrace);
        }

        // Check and setup per-P state.
        foreach (var (_, Δp) in allp) {
            // Check that all Ps have finished deferred mcache flushes.
            {
                var fg = (~Δp).mcache.of(mcache.ᏑflushGen).Load(); if (fg != mheap_.sweepgen) {
                    println((@string)"runtime: p"u8, (~Δp).id, (@string)"flushGen"u8, fg, (@string)"!= sweepgen"u8, mheap_.sweepgen);
                    @throw(pMcacheNotFlushedˢ);
                }
            }
            // Initialize ptrBuf if necessary.
            if (goexperiment.GreenTeaGC && (~Δp).gcw.ptrBuf == nil) {
                Δp.Value.gcw.ptrBuf = (ж<array<uintptr>>)(uintptr)(persistentalloc(gc.PageSize, goarch.PtrSize, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
            }
        }

        gcBgMarkStartWorkers();

        systemstack(gcResetMarkState);

        (work.stwprocs, work.maxprocs) = (gomaxprocs, gomaxprocs);
        if (work.stwprocs > numCPUStartup) {
            // This is used to compute CPU time of the STW phases, so it
            // can't be more than the CPU count, even if GOMAXPROCS is.
            work.stwprocs = numCPUStartup;
        }
        work.heap0 = ᏑgcController.of(gcControllerState.ᏑheapLive).Load();
        work.pauseNS = 0;
        work.mode = mode;

        var now = nanotime();
        work.tSweepTerm = now;
        ref var stw = ref heap(new worldStop(), out var Ꮡstw);
        systemstack(() => {
            Ꮡstw.Value = stopTheWorldWithSema(stwGCSweepTerm);
        });

        // Accumulate fine-grained stopping time.
        Ꮡwork.of(workType.ᏑcpuStats).accumulateGCPauseTime(stw.stoppingCPUTime, 1);

        if (goexperiment.RuntimeSecret) {
            // The world is stopped. Every M is either parked
            // or in a syscall, or running some non-go code which can't run in secret mode.
            // To get to a parked or a syscall state
            // they have to transition through a point where we erase any
            // confidential information in the registers. Making them
            // handle a signal now would clobber the signal stack
            // with non-confidential information.
            //
            // TODO(dmo): this is linear with respect to the number of Ms.
            // Investigate just how long this takes and whether we can somehow
            // loop over just the Ms that have secret info on their signal stack,
            // or cooperatively have the Ms send signals to themselves just
            // after they erase their registers, but before they enter a syscall
            for (var mpΔ1 = allm; mpΔ1 != nil; mpΔ1 = mpΔ1.Value.alllink) {
                // even through the world is stopped, the kernel can still
                // invoke our signal handlers. No confidential information can be spilled
                // (because it's been erased by this time), but we can avoid
                // sending additional signals by atomically inspecting this variable
                if (atomic.Xchg(mpΔ1.of(m.ᏑsignalSecret), 0) != 0) {
                    noopSignal(ref (mpΔ1).DerefOrNull());
                }
                // TODO: syncronize with the signal handler to ensure that the signal
                // was actually delivered.
            }
        }

        // Finish sweep before we start concurrent scan.
        systemstack(() => {
            finishsweep_m();
        });

        // clearpools before we start the GC. If we wait the memory will not be
        // reclaimed until the next GC cycle.
        clearpools();

        Ꮡwork.of(workType.Ꮡcycles).Add(1);

        // Assists and workers can start the moment we start
        // the world.
        ᏑgcController.startCycle(now, (nint)gomaxprocs, trigger);

        // Notify the CPU limiter that assists may begin.
        ᏑgcCPULimiter.startGCTransition(true, now);

        // In STW mode, disable scheduling of user Gs. This may also
        // disable scheduling of this goroutine, so it may block as
        // soon as we start the world again.
        if (mode != gcBackgroundMode) {
            schedEnableUser(false);
        }

        // If goroutine leak detection is pending, enable it for this GC cycle.
        if (Ꮡwork.of(workType.ᏑgoroutineLeak).of(workType_goroutineLeak.Ꮡpending).Load()) {
            work.goroutineLeak.enabled = true;
            Ꮡwork.of(workType.ᏑgoroutineLeak).of(workType_goroutineLeak.Ꮡpending).Store(false);
            // Set all sync objects of blocked goroutines as untraceable
            // by the GC. Only set as traceable at the end of the GC cycle.
            setSyncObjectsUntraceable();
        }

        // Enter concurrent mark phase and enable
        // write barriers.
        //
        // Because the world is stopped, all Ps will
        // observe that write barriers are enabled by
        // the time we start the world and begin
        // scanning.
        //
        // Write barriers must be enabled before assists are
        // enabled because they must be enabled before
        // any non-leaf heap objects are marked. Since
        // allocations are blocked until assists can
        // happen, we want to enable assists as early as
        // possible.
        setGCPhase(_GCmark);

        gcBgMarkPrepare(); // Must happen before assists are enabled.
        gcPrepareMarkRoots();

        // Mark all active tinyalloc blocks. Since we're
        // allocating from these, they need to be black like
        // other allocations. The alternative is to blacken
        // the tiny block on every allocation from it, which
        // would slow down the tiny allocator.
        gcMarkTinyAllocs();

        // At this point all Ps have enabled the write
        // barrier, thus maintaining the no white to
        // black invariant. Enable mutator assists to
        // put back-pressure on fast allocating
        // mutators.
        atomic.Store(ᏑgcBlackenEnabled, 1);

        // In STW mode, we could block the instant systemstack
        // returns, so make sure we're not preemptible.
        mp = acquirem();

        // Update the CPU stats pause time.
        //
        // Use maxprocs instead of stwprocs here because the total time
        // computed in the CPU stats is based on maxprocs, and we want them
        // to be comparable.
        Ꮡwork.of(workType.ᏑcpuStats).accumulateGCPauseTime(nanotime() - stw.finishedStopping, work.maxprocs);

        // Concurrent mark.
        systemstack(() => {
            now = startTheWorldWithSema(0, Ꮡstw.Value);
            work.pauseNS += now - Ꮡstw.Value.startedStopping;
            work.tMark = now;

            // Release the CPU limiter.
            ᏑgcCPULimiter.finishGCTransition(now);
        });

        // Release the world sema before Gosched() in STW mode
        // because we will need to reacquire it later but before
        // this goroutine becomes runnable again, and we could
        // self-deadlock otherwise.
        semrelease(Ꮡworldsema);
        releasem(ref (mp).DerefOrNull());

        // Make sure we block instead of returning to user code
        // in STW mode.
        if (mode != gcBackgroundMode) {
            Gosched();
        }

        semrelease(Ꮡwork.of(workType.ᏑstartSema));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// gcMarkDoneFlushed counts the number of P's with flushed work.
//
// Ideally this would be a captured local in gcMarkDone, but forEachP
// escapes its callback closure, so it can't capture anything.
//
// This is protected by markDoneSema.
internal static readonly ж<uint32> ᏑgcMarkDoneFlushed = new StaticBox<uint32>(default(uint32));
internal static ref uint32 gcMarkDoneFlushed => ref ((StaticBox<uint32>)ᏑgcMarkDoneFlushed).ValueSlot;

// gcDebugMarkDone contains fields used to debug/test mark termination.
partial struct gcDebugMarkDoneᴛ1 /*dyn*/ {
    // spinAfterRaggedBarrier forces gcMarkDone to spin after it executes
    // the ragged barrier.
    internal atomic.Bool spinAfterRaggedBarrier;
    // restartedDueTo27993 indicates that we restarted mark termination
    // due to the bug described in issue #27993.
    //
    // Protected by worldsema.
    internal bool restartedDueTo27993;
}
internal static readonly ж<gcDebugMarkDoneᴛ1> ᏑgcDebugMarkDone = new StaticBox<gcDebugMarkDoneᴛ1>(default(gcDebugMarkDoneᴛ1));
internal static ref gcDebugMarkDoneᴛ1 gcDebugMarkDone => ref ((StaticBox<gcDebugMarkDoneᴛ1>)ᏑgcDebugMarkDone).ValueSlot;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcingˢ = "gcing"u8;

// gcMarkDone transitions the GC from mark to mark termination if all
// reachable objects have been marked (that is, there are no grey
// objects and can be no more in the future). Otherwise, it flushes
// all local work to the global queues where it can be discovered by
// other workers.
//
// All goroutines performing GC work must call gcBeginWork to signal
// that they're executing GC work. They must call gcEndWork when done.
// This should be called when all local mark work has been drained and
// there are no remaining workers. Specifically, when gcEndWork returns
// true.
//
// The calling context must be preemptible.
//
// Flushing local work is important because idle Ps may have local
// work queued. This is the only way to make that work visible and
// drive GC to completion.
//
// It is explicitly okay to have write barriers in this function. If
// it does transition to mark termination, then all reachable objects
// have been marked, so the write barrier cannot shade any more
// objects.
internal static void gcMarkDone() {
    // Ensure only one thread is running the ragged barrier at a
    // time.
    semacquire(Ꮡwork.of(workType.ᏑmarkDoneSema));

top:
    if (!(gcphase == _GCmark && gcIsMarkDone())) {
        // Re-check transition condition under transition lock.
        //
        // It's critical that this checks the global work queues are
        // empty before performing the ragged barrier. Otherwise,
        // there could be global work that a P could take after the P
        // has passed the ragged barrier.
        semrelease(Ꮡwork.of(workType.ᏑmarkDoneSema));
        return;
    }

    // forEachP needs worldsema to execute, and we'll need it to
    // stop the world later, so acquire worldsema now.
    semacquire(Ꮡworldsema);

    // Prevent weak->strong conversions from generating additional
    // GC work. forEachP will guarantee that it is observed globally.
    work.strongFromWeak.block = true;

    // Flush all local buffers and collect flushedWork flags.
    gcMarkDoneFlushed = 0;
    forEachP(waitReasonGCMarkTermination, (ж<Δp> pp) => {
        // Flush the write barrier buffer, since this may add
        // work to the gcWork.
        wbBufFlush1(pp);

        // Flush the gcWork, since this may create global work
        // and set the flushedWork flag.
        //
        // TODO(austin): Break up these workbufs to
        // better distribute work.
        pp.of(runtime_package.Δp.Ꮡgcw).dispose();

        // Collect the flushedWork flag.
        if ((~pp).gcw.flushedWork) {
            atomic.Xadd(ᏑgcMarkDoneFlushed, 1);
            pp.Value.gcw.flushedWork = false;
        }
    });

    if (gcMarkDoneFlushed != 0) {
        // More grey objects were discovered since the
        // previous termination check, so there may be more
        // work to do. Keep going. It's possible the
        // transition condition became true again during the
        // ragged barrier, so re-check it.
        semrelease(Ꮡworldsema);
        goto top;
    }

    // For debugging/testing.
    while (ᏑgcDebugMarkDone.of(gcDebugMarkDoneᴛ1.ᏑspinAfterRaggedBarrier).Load()) {
    }

    // There was no global work, no local work, and no Ps
    // communicated work since we took markDoneSema. Therefore
    // there are no grey objects and no more objects can be
    // shaded. Transition to mark termination.
    var now = nanotime();
    work.tMarkTerm = now;
    getg().Value.m.Value.preemptoff = gcingˢ;
    ref var stw = ref heap(new worldStop(), out var Ꮡstw);
    systemstack(() => {
        Ꮡstw.Value = stopTheWorldWithSema(stwGCMarkTerm);
    });
    // The gcphase is _GCmark, it will transition to _GCmarktermination
    // below. The important thing is that the wb remains active until
    // all marking is complete. This includes writes made by the GC.

    // Accumulate fine-grained stopping time.
    Ꮡwork.of(workType.ᏑcpuStats).accumulateGCPauseTime(stw.stoppingCPUTime, 1);

    // There is sometimes work left over when we enter mark termination due
    // to write barriers performed after the completion barrier above.
    // Detect this and resume concurrent mark. This is obviously
    // unfortunate.
    //
    // See issue #27993 for details.
    //
    // Switch to the system stack to call wbBufFlush1, though in this case
    // it doesn't matter because we're non-preemptible anyway.
    var restart = false;
    systemstack(() => {
        foreach (var (_, Δp) in allp) {
            wbBufFlush1(Δp);
            if (!Δp.of(runtime_package.Δp.Ꮡgcw).empty()) {
                restart = true;
                break;
            }
        }
    });

    // Check whether we need to resume the marking phase because of issue #27993
    // or because of goroutine leak detection.
    if (restart || (work.goroutineLeak.enabled && !work.goroutineLeak.done)) {
        if (restart){
            // Restart because of issue #27993.
            gcDebugMarkDone.restartedDueTo27993 = true;
        } else {
            // Marking has reached a fixed-point. Attempt to detect goroutine leaks.
            //
            // If the returned value is true, then detection already concluded for this cycle.
            // Otherwise, more runnable goroutines were discovered, requiring additional mark work.
            work.goroutineLeak.done = findGoroutineLeaks();
        }

        getg().Value.m.Value.preemptoff = ""u8;
        systemstack(() => {
            // Accumulate the time we were stopped before we had to start again.
            Ꮡwork.of(workType.ᏑcpuStats).accumulateGCPauseTime(nanotime() - Ꮡstw.Value.finishedStopping, work.maxprocs);

            // Start the world again.
            var nowΔ1 = startTheWorldWithSema(0, Ꮡstw.Value);
            work.pauseNS += nowΔ1 - Ꮡstw.Value.startedStopping;
        });
        semrelease(Ꮡworldsema);
        goto top;
    }

    gcComputeStartingStackSize();

    // Disable assists and background workers. We must do
    // this before waking blocked assists.
    atomic.Store(ᏑgcBlackenEnabled, 0);

    // Notify the CPU limiter that GC assists will now cease.
    ᏑgcCPULimiter.startGCTransition(false, now);

    // Wake all blocked assists. These will run when we
    // start the world again.
    gcWakeAllAssists();

    // Wake all blocked weak->strong conversions. These will run
    // when we start the world again.
    work.strongFromWeak.block = false;
    gcWakeAllStrongFromWeak();

    // Likewise, release the transition lock. Blocked
    // workers and assists will run when we start the
    // world again.
    semrelease(Ꮡwork.of(workType.ᏑmarkDoneSema));

    // In STW mode, re-enable user goroutines. These will be
    // queued to run after we start the world.
    schedEnableUser(true);

    // endCycle depends on all gcWork cache stats being flushed.
    // The termination algorithm above ensured that up to
    // allocations since the ragged barrier.
    ᏑgcController.endCycle(now, (nint)gomaxprocs, work.userForced);

    // Perform mark termination. This will restart the world.
    gcMarkTermination(stw);
}

// isMaybeRunnable checks whether a goroutine may still be semantically runnable.
// For goroutines which are semantically runnable, this will eventually return true
// as the GC marking phase progresses. It returns false for leaked goroutines, or for
// goroutines which are not yet computed as possibly runnable by the GC.
internal static bool isMaybeRunnable(this ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Check whether the goroutine is actually in a waiting state first.
    if (readgstatus(Ꮡgp) != _Gwaiting) {
        // If the goroutine is not waiting, then clearly it is maybe runnable.
        return true;
    }

    var exprᴛ1 = gp.waitreason;
    if (exprᴛ1 == waitReasonSelectNoCases || exprᴛ1 == waitReasonChanSendNilChan || exprᴛ1 == waitReasonChanReceiveNilChan) {
        // Select with no cases or communicating on nil channels
        // make goroutines unrunnable by definition.
        return false;
    }
    if (exprᴛ1 == waitReasonChanReceive || exprᴛ1 == waitReasonSelect || exprᴛ1 == waitReasonChanSend) {
        // Cycle all through all *sudog to check whether
        // the goroutine is waiting on a marked channel.
        for (var sg = gp.waiting; sg != nil; sg = sg.Value.waitlink) {
            if (isMarkedOrNotInHeap(@unsafe.Pointer.FromPinnedBox(sg.of(sudog.Ꮡc).get()))) {
                return true;
            }
        }
        return false;
    }
    if (exprᴛ1 == waitReasonSyncCondWait || exprᴛ1 == waitReasonSyncWaitGroupWait || exprᴛ1 == waitReasonSyncMutexLock || exprᴛ1 == waitReasonSyncRWMutexLock || exprᴛ1 == waitReasonSyncRWMutexRLock) {
        // If waiting on mutexes, wait groups, or condition variables,
        // check if the synchronization primitive attached to the sudog is marked.
        if (gp.waiting != nil) {
            return isMarkedOrNotInHeap((uintptr)gp.waiting.of(sudog.Ꮡelem).get());
        }
    }
    return true;
}

// findMaybeRunnableGoroutines checks to see if more blocked but maybe-runnable goroutines exist.
// If so, it adds them into root set and increments work.markrootJobs accordingly.
// Returns true if we need to run another phase of markroots; returns false otherwise.
internal static bool /*moreWork*/ findMaybeRunnableGoroutines() {
    var oldRootJobs = Ꮡwork.of(workType.ᏑmarkrootJobs).Load();

    // To begin with we have a set of unchecked stackRoots between
    // vIndex and ivIndex. During the loop, anything < vIndex should be
    // valid stackRoots and anything >= ivIndex should be invalid stackRoots.
    // The loop terminates when the two indices meet.
    nint vIndex = work.nMaybeRunnableStackRoots;
    nint ivIndex = work.nStackRoots;
    // Reorder goroutine list
    while (vIndex < ivIndex) {
        if (work.stackRoots[vIndex].isMaybeRunnable()) {
            vIndex = vIndex + 1;
            continue;
        }
        for (ivIndex = ivIndex - 1; ivIndex != vIndex; ivIndex = ivIndex - 1) {
            {
                var gp = work.stackRoots[ivIndex]; if (gp.isMaybeRunnable()) {
                    work.stackRoots[ivIndex] = work.stackRoots[vIndex];
                    work.stackRoots[vIndex] = gp;
                    vIndex = vIndex + 1;
                    break;
                }
            }
        }
    }

    var newRootJobs = work.baseStacks + (uint32)vIndex;
    if (newRootJobs > oldRootJobs) {
        work.nMaybeRunnableStackRoots = vIndex;
        Ꮡwork.of(workType.ᏑmarkrootJobs).Store(newRootJobs);
    }
    return newRootJobs > oldRootJobs;
}

// setSyncObjectsUntraceable scans allgs and sets the elem and c fields of all sudogs to
// an untrackable pointer. This prevents the GC from marking these objects as live in memory
// by following these pointers when runnning deadlock detection.
internal static void setSyncObjectsUntraceable() {
    assertWorldStopped();

    forEachGRace((ж<g> gp) => {
        // Set as untraceable all synchronization objects of goroutines
        // blocked at concurrency operations that could leak.
        switch (ᐧ) {
        case {} when (~gp).waitreason.isSyncWait(): {
            // Synchronization primitives are reachable from the *sudog via
            // via the elem field.
            for (var sg = gp.Value.waiting; sg != nil; sg = sg.Value.waitlink) {
                sg.of(sudog.Ꮡelem).setUntraceable();
            }
            break;
        }
        case {} when (~gp).waitreason.isChanWait(): {
            // Channels and select statements are reachable from the *sudog via the c field.
            for (var sg = gp.Value.waiting; sg != nil; sg = sg.Value.waitlink) {
                sg.of(sudog.Ꮡc).of(maybeTraceableChan.ᏑmaybeTraceablePtr).setUntraceable();
            }
            break;
        }}
    });
}

// gcRestoreSyncObjects restores the elem and c fields of all sudogs to their original values.
// Should be invoked after the goroutine leak detection phase.
internal static void gcRestoreSyncObjects() {
    assertWorldStopped();

    forEachGRace((ж<g> gp) => {
        for (var sg = gp.Value.waiting; sg != nil; sg = sg.Value.waitlink) {
            sg.of(sudog.Ꮡelem).setTraceable();
            sg.of(sudog.Ꮡc).of(maybeTraceableChan.ᏑmaybeTraceablePtr).setTraceable();
        }
    });
}

// findGoroutineLeaks scans the remaining stackRoots and marks any which are
// blocked over exclusively unreachable concurrency primitives as leaked (deadlocked).
// Returns true if the goroutine leak check was performed (or unnecessary).
// Returns false if the GC cycle has not yet computed all maybe-runnable goroutines.
internal static bool findGoroutineLeaks() {
    assertWorldStopped();

    // Report goroutine leaks and mark them unreachable, and resume marking
    // we still need to mark these unreachable *g structs as they
    // get reused, but their stack won't get scanned
    if (work.nMaybeRunnableStackRoots == work.nStackRoots) {
        // nMaybeRunnableStackRoots == nStackRoots means that all goroutines are marked.
        return true;
    }

    // Check whether any more maybe-runnable goroutines can be found by the GC.
    if (findMaybeRunnableGoroutines()) {
        // We found more work, so we need to resume the marking phase.
        return false;
    }

    // For the remaining goroutines, mark them as unreachable and leaked.
    work.goroutineLeak.count = work.nStackRoots - work.nMaybeRunnableStackRoots;

    for (nint i = work.nMaybeRunnableStackRoots; i < work.nStackRoots; i++) {
        var gp = work.stackRoots[i];
        casgstatus(gp, _Gwaiting, _Gleaked);

        // Add the primitives causing the goroutine leaks
        // to the GC work queue, to ensure they are marked.
        //
        // NOTE(vsaioc): these primitives should also be reachable
        // from the goroutine's stack, but let's play it safe.
        switch (ᐧ) {
        case {} when (~gp).waitreason.isChanWait(): {
            for (var sg = gp.Value.waiting; sg != nil; sg = sg.Value.waitlink) {
                shade(sg.of(sudog.Ꮡc).of(maybeTraceableChan.ᏑmaybeTraceablePtr).uintptr());
            }
            break;
        }
        case {} when (~gp).waitreason.isSyncWait(): {
            for (var sg = gp.Value.waiting; sg != nil; sg = sg.Value.waitlink) {
                shade(sg.of(sudog.Ꮡelem).uintptr());
            }
            break;
        }}
    }
    // Put the remaining roots as ready for marking and drain them.
    Ꮡwork.of(workType.ᏑmarkrootJobs).Add((int32)(work.nStackRoots - work.nMaybeRunnableStackRoots));
    work.nMaybeRunnableStackRoots = work.nStackRoots;
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcDoneButGcphaseGCoffˢ = "gc done but gcphase != _GCoff"u8;
internal static readonly @string failedToSetSweepBarrierˢ = "failed to set sweep barrier"u8;
internal static readonly @string nonConcurrentSweepFailedˢ = "non-concurrent sweep failed to drain all sweep queues"u8;

// World must be stopped and mark assists and background workers must be
// disabled.
internal static void gcMarkTermination(worldStop stw) {
    // Start marktermination (write barrier remains enabled for now).
    setGCPhase(_GCmarktermination);

    work.heap1 = ᏑgcController.of(gcControllerState.ᏑheapLive).Load();
    var startTime = nanotime();

    var mp = acquirem();
    mp.Value.preemptoff = gcingˢ;
    mp.Value.traceback = 2;
    var curgp = mp.Value.curg;
    // N.B. The execution tracer is not aware of this status
    // transition and handles it specially based on the
    // wait reason.
    casGToWaitingForSuspendG(curgp, _Grunning, waitReasonGarbageCollection);

    // Run gc on the g0 stack. We do this so that the g stack
    // we're currently running on will no longer change. Cuts
    // the root set down a bit (g0 stacks are not scanned, and
    // we don't need to scan gc's internal state).  We also
    // need to switch to g0 so we can shrink the stack.
    systemstack(() => {
        gcMark(startTime);
        // Must return immediately.
        // The outer function's stack may have moved
        // during gcMark (it shrinks stacks, including the
        // outer function's stack), so we must not refer
        // to any of its variables. Return back to the
        // non-system stack to pick up the new addresses
        // before continuing.
    });

    bool stwSwept = default!;
    systemstack(() => {
        work.heap2 = work.bytesMarked;
        if (debug.gccheckmark > 0) {
            runCheckmark((ж<gcWork> _Δp0) => {
                gcPrepareMarkRoots();
            });
        }
        if (debug.checkfinalizers > 0) {
            checkFinalizersAndCleanups();
        }

        // marking is complete so we can turn the write barrier off
        setGCPhase(_GCoff);
        stwSwept = gcSweep(work.mode);
    });

    mp.Value.traceback = 0;
    casgstatus(curgp, _Gwaiting, _Grunning);

    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCDone();
        traceRelease(Δtrace);
    }

    // all done
    mp.Value.preemptoff = ""u8;

    if (gcphase != _GCoff) {
        @throw(gcDoneButGcphaseGCoffˢ);
    }

    // Record heapInUse for scavenger.
    memstats.lastHeapInUse = ᏑgcController.of(gcControllerState.ᏑheapInUse).load();

    // Update GC trigger and pacing, as well as downstream consumers
    // of this pacing information, for the next cycle.
    systemstack(gcControllerCommit);

    // Update timing memstats
    var now = nanotime();
    var (sec, nsec, _) = time_now();
    var unixNow = sec * 1000000000 + (int64)nsec;
    work.pauseNS += now - stw.startedStopping;
    work.tEnd = now;
    atomic.Store64(Ꮡmemstats.of(mstats.Ꮡlast_gc_unix), (uint64)unixNow); // must be Unix time to make sense to user
    atomic.Store64(Ꮡmemstats.of(mstats.Ꮡlast_gc_nanotime), (uint64)now); // monotonic time for us
    memstats.pause_ns[memstats.numgc % (uint32)len(memstats.pause_ns)] = (uint64)work.pauseNS;
    memstats.pause_end[memstats.numgc % (uint32)len(memstats.pause_end)] = (uint64)unixNow;
    memstats.pause_total_ns += (uint64)work.pauseNS;

    // Accumulate CPU stats.
    //
    // Use maxprocs instead of stwprocs for GC pause time because the total time
    // computed in the CPU stats is based on maxprocs, and we want them to be
    // comparable.
    //
    // Pass gcMarkPhase=true to accumulate so we can get all the latest GC CPU stats
    // in there too.
    Ꮡwork.of(workType.ᏑcpuStats).accumulateGCPauseTime(now - stw.finishedStopping, work.maxprocs);
    Ꮡwork.of(workType.ᏑcpuStats).accumulate(now, true);

    // Compute overall GC CPU utilization.
    // Omit idle marking time from the overall utilization here since it's "free".
    memstats.gc_cpu_fraction = (float64)(work.cpuStats.GCTotalTime - work.cpuStats.GCIdleTime) / (float64)work.cpuStats.TotalTime;

    // Reset assist time and background time stats.
    //
    // Do this now, instead of at the start of the next GC cycle, because
    // these two may keep accumulating even if the GC is not active.
    ᏑΔscavenge.of(Δscavengeᴛ1.ᏑassistTime).Store(0);
    ᏑΔscavenge.of(Δscavengeᴛ1.ᏑbackgroundTime).Store(0);

    // Reset idle time stat.
    Ꮡsched.of(schedt.ᏑidleTime).Store(0);

    if (work.userForced) {
        memstats.numforcedgc++;
    }

    // Bump GC cycle count and wake goroutines waiting on sweep.
    @lock(Ꮡwork.of(workType.ᏑsweepWaiters).of(workType_sweepWaiters.Ꮡlock));
    memstats.numgc++;
    injectglist(Ꮡwork.of(workType.ᏑsweepWaiters).of(workType_sweepWaiters.Ꮡlist));
    unlock(Ꮡwork.of(workType.ᏑsweepWaiters).of(workType_sweepWaiters.Ꮡlock));

    // Increment the scavenge generation now.
    //
    // This moment represents peak heap in use because we're
    // about to start sweeping.
    Ꮡmheap_.of(mheap.Ꮡpages).of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).nextGen();

    // Release the CPU limiter.
    ᏑgcCPULimiter.finishGCTransition(now);

    // Finish the current heap profiling cycle and start a new
    // heap profiling cycle. We do this before starting the world
    // so events don't leak into the wrong cycle.
    mProf_NextCycle();

    // There may be stale spans in mcaches that need to be swept.
    // Those aren't tracked in any sweep lists, so we need to
    // count them against sweep completion until we ensure all
    // those spans have been forced out.
    //
    // If gcSweep fully swept the heap (for example if the sweep
    // is not concurrent due to a GODEBUG setting), then we expect
    // the sweepLocker to be invalid, since sweeping is done.
    //
    // N.B. Below we might duplicate some work from gcSweep; this is
    // fine as all that work is idempotent within a GC cycle, and
    // we're still holding worldsema so a new cycle can't start.
    var sl = ᏑΔsweep.of(sweepdata.Ꮡactive).begin();
    if (!stwSwept && !sl.valid){
        @throw(failedToSetSweepBarrierˢ);
    } else 
    if (stwSwept && sl.valid) {
        @throw(nonConcurrentSweepFailedˢ);
    }

    if (work.goroutineLeak.enabled) {
        // Restore the elem and c fields of all sudogs to their original values.
        gcRestoreSyncObjects();
    }

    bool goroutineLeakDone = default!;
    var stwʗ1 = stw;
    systemstack(() => {
        // Pull the GC out of goroutine leak detection mode.
        work.goroutineLeak.enabled = false;
        goroutineLeakDone = work.goroutineLeak.done;
        work.goroutineLeak.done = false;

        // The memstats updated above must be updated with the world
        // stopped to ensure consistency of some values, such as
        // sched.idleTime and sched.totaltime. memstats also include
        // the pause time (work,pauseNS), forcing computation of the
        // total pause time before the pause actually ends.
        //
        // Here we reuse the same now for start the world so that the
        // time added to /sched/pauses/total/gc:seconds will be
        // consistent with the value in memstats.
        startTheWorldWithSema(now, stwʗ1);
    });

    // Flush the heap profile so we can start a new cycle next GC.
    // This is relatively expensive, so we don't do it with the
    // world stopped.
    mProf_Flush();

    // Prepare workbufs for freeing by the sweeper. We do this
    // asynchronously because it can take non-trivial time.
    prepareFreeWorkbufs();

    // Free stack spans. This must be done between GC cycles.
    systemstack(freeStackSpans);

    // Ensure all mcaches are flushed. Each P will flush its own
    // mcache before allocating, but idle Ps may not. Since this
    // is necessary to sweep all spans, we need to ensure all
    // mcaches are flushed before we start the next GC cycle.
    //
    // While we're here, flush the page cache for idle Ps to avoid
    // having pages get stuck on them. These pages are hidden from
    // the scavenger, so in small idle heaps a significant amount
    // of additional memory might be held onto.
    //
    // Also, flush the pinner cache, to avoid leaking that memory
    // indefinitely.
    if (debug.gctrace > 1) {
        builtin.clear(memstats.lastScanStats[..]);
    }
    forEachP(waitReasonFlushProcCaches, (ж<Δp> pp) => {
        (~pp).mcache.prepareForSweep();
        if ((~pp).status == _Pidle) {
            systemstack(() => {
                @lock(Ꮡmheap_.of(mheap.Ꮡlock));
                pp.of(runtime_package.Δp.Ꮡpcache).flush(Ꮡmheap_.of(mheap.Ꮡpages));
                unlock(Ꮡmheap_.of(mheap.Ꮡlock));
            });
        }
        if (debug.gctrace > 1) {
            pp.of(runtime_package.Δp.Ꮡgcw).flushScanStats(Ꮡmemstats.of(mstats.ᏑlastScanStats));
        }
        pp.Value.pinnerCache = default!;
    });
    if (sl.valid) {
        // Now that we've swept stale spans in mcaches, they don't
        // count against unswept spans.
        //
        // Note: this sweepLocker may not be valid if sweeping had
        // already completed during the STW. See the corresponding
        // begin() call that produced sl.
        ᏑΔsweep.of(sweepdata.Ꮡactive).end(sl);
    }

    // Print gctrace before dropping worldsema. As soon as we drop
    // worldsema another cycle could start and smash the stats
    // we're trying to print.
    if (debug.gctrace > 0) {
        nint util = (nint)(memstats.gc_cpu_fraction * 100D);

        array<byte> sbuf = new(24);
        printlock();
        print((@string)"gc "u8, memstats.numgc,
            (@string)" @"u8, (@string)itoaDiv(sbuf[..], (uint64)(work.tSweepTerm - runtimeInitTime) / 1000000, 3), (@string)"s "u8,
            util, (@string)"%"u8);
        if (goroutineLeakDone) {
            print((@string)" (checking for goroutine leaks)"u8);
        }
        print((@string)": "u8);
        var prev = work.tSweepTerm;
        foreach (var (i, ns) in new int64[]{work.tMark, work.tMarkTerm, work.tEnd}.slice()) {
            if (i != 0) {
                print((@string)"+"u8);
            }
            print((@string)fmtNSAsMS(sbuf[..], (uint64)(ns - prev)));
            prev = ns;
        }
        print((@string)" ms clock, "u8);
        foreach (var (i, ns) in new int64[]{
            (int64)work.stwprocs * (work.tMark - work.tSweepTerm),
            ᏑgcController.of(gcControllerState.ᏑassistTime).Load(),
            ᏑgcController.of(gcControllerState.ᏑdedicatedMarkTime).Load() + ᏑgcController.of(gcControllerState.ᏑfractionalMarkTime).Load(),
            ᏑgcController.of(gcControllerState.ᏑidleMarkTime).Load(),
            (int64)work.stwprocs * (work.tEnd - work.tMarkTerm)
        }.slice()) {
            if (i == 2 || i == 3){
                // Separate mark time components with /.
                print((@string)"/"u8);
            } else 
            if (i != 0) {
                print((@string)"+"u8);
            }
            print((@string)fmtNSAsMS(sbuf[..], (uint64)ns));
        }
        print((@string)" ms cpu, "u8,
            work.heap0 >> 20, (@string)"->"u8, work.heap1 >> 20, (@string)"->"u8, work.heap2 >> 20, (@string)" MB, "u8,
            gcController.lastHeapGoal >> 20, (@string)" MB goal, "u8,
            ᏑgcController.of(gcControllerState.ᏑlastStackScan).Load() >> 20, (@string)" MB stacks, "u8,
            ᏑgcController.of(gcControllerState.ᏑglobalsScan).Load() >> 20, (@string)" MB globals, "u8,
            work.maxprocs, (@string)" P"u8);
        if (work.userForced) {
            print((@string)" (forced)"u8);
        }
        print((@string)"\n"u8);

        if (debug.gctrace > 1) {
            dumpScanStats();
        }
        printunlock();
    }

    // Print finalizer/cleanup queue length. Like gctrace, do this before the next GC starts.
    // The fact that the next GC might start is not that problematic here, but acts as a convenient
    // lock on printing this information (so it cannot overlap with itself from the next GC cycle).
    if (debug.checkfinalizers > 0) {
        var (fq, fe) = finReadQueueStats();
        var fn = builtin.max((int64)fq - (int64)fe, 0);

        var (cq, ce) = ᏑgcCleanups.readQueueStats();
        var cn = builtin.max((int64)cq - (int64)ce, 0);

        println((@string)"checkfinalizers: queue:"u8, fn, (@string)"finalizers +"u8, cn, (@string)"cleanups"u8);
    }

    // Set any arena chunks that were deferred to fault.
    @lock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
    var faultList = userArenaState.fault;
    userArenaState.fault = default!;
    unlock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
    foreach (var (_, lc) in faultList) {
        lc.mspan.setUserArenaChunkToFault();
    }

    // Enable huge pages on some metadata if we cross a heap threshold.
    if (ᏑgcController.heapGoal() > minHeapForMetadataHugePages) {
        systemstack(() => {
            Ꮡmheap_.enableMetadataHugePages();
        });
    }

    semrelease(Ꮡworldsema);
    semrelease(Ꮡgcsema);
    // Careful: another GC cycle may start now.

    releasem(ref (mp).DerefOrNull());
    mp = default!;

    // now that gc is done, kick off finalizer thread if needed
    if (!concurrentSweep) {
        // give the queued finalizers, if any, a chance to run
        Gosched();
    }
}

// gcBgMarkStartWorkers prepares background mark worker goroutines. These
// goroutines will not run until the mark phase, but they must be started while
// the work is not stopped and from a regular G stack. The caller must hold
// worldsema.
internal static partial void gcBgMarkStartWorkers() {
    // Background marking is performed by per-P G's. Ensure that each P has
    // a background GC G.
    //
    // Worker Gs don't exit if gomaxprocs is reduced. If it is raised
    // again, we can reuse the old workers; no need to create new workers.
    if (gcBgMarkWorkerCount >= gomaxprocs) {
        return;
    }

    // Increment mp.locks when allocating. We are called within gcStart,
    // and thus must not trigger another gcStart via an allocation. gcStart
    // bails when allocating with locks held, so simulate that for these
    // allocations.
    //
    // TODO(prattmic): cleanup gcStart to use a more explicit "in gcStart"
    // check for bailing.
    var mp = acquirem();
    var ready = new channel<EmptyStruct>(1);
    releasem(ref (mp).DerefOrNull());

    while (gcBgMarkWorkerCount < gomaxprocs) {
        var mpΔ1 = acquirem(); // See above, we allocate a closure here.
        goǃ(gcBgMarkWorker, ready);
        releasem(ref (mpΔ1).DerefOrNull());

        // N.B. we intentionally wait on each goroutine individually
        // rather than starting all in a batch and then waiting once
        // afterwards. By running one goroutine at a time, we can take
        // advantage of runnext to bounce back and forth between
        // workers and this goroutine. In an overloaded application,
        // this can reduce GC start latency by prioritizing these
        // goroutines rather than waiting on the end of the run queue.
        ᐸꟷ(ready);
        // The worker is now guaranteed to be added to the pool before
        // its P's next findRunnableGCWorker.

        gcBgMarkWorkerCount++;
    }
}

// gcBgMarkPrepare sets up state for background marking.
// Mutator assists must not yet be enabled.
internal static void gcBgMarkPrepare() {
    // Background marking will stop when the work queues are empty
    // and there are no more workers (note that, since this is
    // concurrent, this may be a transient state, but mark
    // termination will clean it up). Between background workers
    // and assists, we don't really know how many workers there
    // will be, so we pretend to have an arbitrarily large number
    // of workers, almost all of which are "waiting". While a
    // worker is working it decrements nwait. If nproc == nwait,
    // there are no workers.
    work.nproc = ~(uint32)0;
    work.nwait = ~(uint32)0;
}

// gcBgMarkWorkerNode is an entry in the gcBgMarkWorkerPool. It points to a single
// gcBgMarkWorker goroutine.
partial struct gcBgMarkWorkerNode {
    // Unused workers are managed in a lock-free stack. This field must be first.
    internal lfnode node;
    // The g of this worker.
    internal Δguintptr gp;
    // Release this m on park. This is used to communicate with the unlock
    // function, which cannot access the G's stack. It is unused outside of
    // gcBgMarkWorker().
    internal muintptr m;
}

partial struct gcBgMarkWorkerNodePadded {
    /*embed*/ internal gcBgMarkWorkerNode gcBgMarkWorkerNode;
    internal array<byte> pad = new((uintptr)((uintptr)tagAlign - /* unsafe.Sizeof(gcBgMarkWorkerNode{}) */ (uintptr)32) - gcBgMarkWorkerNodeRedZoneSize);
}

internal static UntypedInt gcBgMarkWorkerNodeRedZoneSize => /* (16 << 2) * asanenabledBit */ 0; // redZoneSize(512)

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcWorkerInitˢ = "GC worker init"u8;
internal static readonly @string gcBgMarkWorkerBlackeningˢ = "gcBgMarkWorker: blackening not enabled"u8;
internal static readonly @string gcBgMarkWorkerModeNotSetˢ = "gcBgMarkWorker: mode not set"u8;
internal static readonly @string gcBgMarkWorkerUnexpectedˢ = "gcBgMarkWorker: unexpected gcMarkWorkerMode"u8;

internal static void gcBgMarkWorker(channel<EmptyStruct> ready) {
    var gp = getg();

    // We pass node to a gopark unlock function, so it can't be on
    // the stack (see gopark). Prevent deadlock from recursively
    // starting GC by disabling preemption.
    gp.Value.m.Value.preemptoff = gcWorkerInitˢ;
    // TODO: This is technically not allowed in the heap. See comment in tagptr.go.
    //
    // It is kept alive simply by virtue of being used in the infinite loop
    // below. gcBgMarkWorkerPool keeps pointers to nodes that are not
    // GC-visible, so this must be kept alive indefinitely (even if
    // GOMAXPROCS decreases).
    var node = @new<gcBgMarkWorkerNodePadded>().of(gcBgMarkWorkerNodePadded.ᏑgcBgMarkWorkerNode);
    gp.Value.m.Value.preemptoff = ""u8;

    node.of(gcBgMarkWorkerNode.Ꮡgp).set(gp);

    node.of(gcBgMarkWorkerNode.Ꮡm).set(acquirem());

    ready.ᐸꟷ(new EmptyStruct());
    // After this point, the background mark worker is generally scheduled
    // cooperatively by gcController.findRunnableGCWorker. While performing
    // work on the P, preemption is disabled because we are working on
    // P-local work buffers. When the preempt flag is set, this puts itself
    // into _Gwaiting to be woken up by gcController.findRunnableGCWorker
    // at the appropriate time.
    //
    // When preemption is enabled (e.g., while in gcMarkDone), this worker
    // may be preempted and schedule as a _Grunnable G from a runq. That is
    // fine; it will eventually gopark again for further scheduling via
    // findRunnableGCWorker.
    //
    // Since we disable preemption before notifying ready, we guarantee that
    // this G will be in the worker pool for the next findRunnableGCWorker.
    // This isn't strictly necessary, but it reduces latency between
    // _GCmark starting and the workers starting.

    while (ᐧ) {
        // Go to sleep until woken by
        // gcController.findRunnableGCWorker.
        gopark((ж<g> g, @unsafe.Pointer nodep) => {
            var nodeΔ1 = (ж<gcBgMarkWorkerNode>)(uintptr)(nodep);

            {
                var mp = (~nodeΔ1).m.ptr(); if (mp != nil) {
                    // The worker G is no longer running; release
                    // the M.
                    //
                    // N.B. it is _safe_ to release the M as soon
                    // as we are no longer performing P-local mark
                    // work.
                    //
                    // However, since we cooperatively stop work
                    // when gp.preempt is set, if we releasem in
                    // the loop then the following call to gopark
                    // would immediately preempt the G. This is
                    // also safe, but inefficient: the G must
                    // schedule again only to enter gopark and park
                    // again. Thus, we defer the release until
                    // after parking the G.
                    releasem(ref (mp).DerefOrNull());
                }
            }

            // Release this G to the pool.
            ᏑgcBgMarkWorkerPool.push(nodeΔ1.of(gcBgMarkWorkerNode.Ꮡnode));
            // Note that at this point, the G may immediately be
            // rescheduled and may be running.
            return true;
        }, @unsafe.Pointer.FromPinnedBox(node), waitReasonGCWorkerIdle, traceBlockSystemGoroutine, 0);

        // Preemption must not occur here, or another G might see
        // p.gcMarkWorkerMode.

        // Disable preemption so we can use the gcw. If the
        // scheduler wants to preempt us, we'll stop draining,
        // dispose the gcw, and then preempt.
        node.of(gcBgMarkWorkerNode.Ꮡm).set(acquirem());
        var pp = (~(~gp).m).p.ptr(); // P can't change with preemption disabled.

        if (gcBlackenEnabled == 0) {
            println((@string)"worker mode"u8, (~pp).gcMarkWorkerMode);
            @throw(gcBgMarkWorkerBlackeningˢ);
        }

        if ((~pp).gcMarkWorkerMode == gcMarkWorkerNotWorker) {
            @throw(gcBgMarkWorkerModeNotSetˢ);
        }

        var startTime = nanotime();
        pp.Value.gcMarkWorkerStartTime = startTime;
        bool trackLimiterEvent = default!;
        if ((~pp).gcMarkWorkerMode == gcMarkWorkerIdleMode) {
            trackLimiterEvent = pp.of(runtime_package.Δp.ᏑlimiterEvent).start(limiterEventIdleMarkWork, startTime);
        }

        gcBeginWork();

        var gpʗ1 = gp;
        var ppʗ1 = pp;
        systemstack(() => {
            // Mark our goroutine preemptible so its stack can be scanned or observed
            // by the execution tracer. This, for example, lets two mark workers scan
            // each other (otherwise, they would deadlock).
            //
            // casGToWaitingForSuspendG marks the goroutine as ineligible for a
            // stack shrink, effectively pinning the stack in memory for the duration.
            //
            // N.B. The execution tracer is not aware of this status transition and
            // handles it specially based on the wait reason.
            casGToWaitingForSuspendG(gpʗ1, _Grunning, waitReasonGCWorkerActive);
            var exprᴛ1 = (~ppʗ1).gcMarkWorkerMode;
            if (exprᴛ1 == gcMarkWorkerDedicatedMode) {
                gcDrainMarkWorkerDedicated(ppʗ1.of(runtime_package.Δp.Ꮡgcw), true);
                if ((~gpʗ1).preempt) {
                    // We were preempted. This is
                    // a useful signal to kick
                    // everything out of the run
                    // queue so it can run
                    // somewhere else.
                    {
                        ref var drainQ = ref heap<gQueue>(out var ᏑdrainQ);
                        drainQ = runqdrain(ppʗ1); if (!drainQ.empty()) {
                            @lock(Ꮡsched.of(schedt.Ꮡlock));
                            globrunqputbatch(ref (ᏑdrainQ).DerefOrNull());
                            unlock(Ꮡsched.of(schedt.Ꮡlock));
                        }
                    }
                }
                // Go back to draining, this time
                // without preemption.
                gcDrainMarkWorkerDedicated(ppʗ1.of(runtime_package.Δp.Ꮡgcw), false);
            }
            else if (exprᴛ1 == gcMarkWorkerFractionalMode) {
                gcDrainMarkWorkerFractional(ppʗ1.of(runtime_package.Δp.Ꮡgcw));
            }
            else if (exprᴛ1 == gcMarkWorkerIdleMode) {
                gcDrainMarkWorkerIdle(ppʗ1.of(runtime_package.Δp.Ꮡgcw));
            }
            else { /* default: */
                @throw(gcBgMarkWorkerUnexpectedˢ);
            }
            casgstatus(gpʗ1, _Gwaiting, _Grunning);
        });

        // Account for time and mark us as stopped.
        var now = nanotime();
        var duration = now - startTime;
        ᏑgcController.markWorkerStop((~pp).gcMarkWorkerMode, duration);
        if (trackLimiterEvent) {
            pp.of(runtime_package.Δp.ᏑlimiterEvent).stop(limiterEventIdleMarkWork, now);
        }
        if ((~pp).gcMarkWorkerMode == gcMarkWorkerFractionalMode) {
            pp.of(runtime_package.Δp.ᏑgcFractionalMarkTime).Add(duration);
        }

        // We'll releasem after this point and thus this P may run
        // something else. We must clear the worker mode to avoid
        // attributing the mode to a different (non-worker) G in
        // tracev2.GoStart.
        pp.Value.gcMarkWorkerMode = gcMarkWorkerNotWorker;

        // If this worker reached a background mark completion
        // point, signal the main GC goroutine.
        if (gcEndWork()) {
            // We don't need the P-local buffers here, allow
            // preemption because we may schedule like a regular
            // goroutine in gcMarkDone (block on locks, etc).
            releasem(ref ((~node).m.ptr()).DerefOrNull());
            node.of(gcBgMarkWorkerNode.Ꮡm).set(nil);

            gcMarkDone();
        }
    }
}

// gcShouldScheduleWorker reports whether executing a mark worker
// on p is potentially useful. p may be nil.
internal static bool gcShouldScheduleWorker(ж<Δp> Ꮡp) {
    ref var Δp = ref Ꮡp.DerefOrNull();

    if (Ꮡp != nil && !Ꮡp.of(runtime_package.Δp.Ꮡgcw).empty()) {
        return true;
    }
    return gcMarkWorkAvailable();
}

// gcIsMarkDone reports whether the mark phase is (probably) done.
internal static bool gcIsMarkDone() {
    return work.nwait == work.nproc && !gcMarkWorkAvailable();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string workNwaitWasWorkNprocˢ = "work.nwait was > work.nproc"u8;

// gcBeginWork signals to the garbage collector that a new worker is
// about to process GC work.
internal static void gcBeginWork() {
    var decnwait = atomic.Xadd(Ꮡwork.of(workType.Ꮡnwait), -1);
    if (decnwait == work.nproc) {
        println((@string)"runtime: work.nwait="u8, decnwait, (@string)"work.nproc="u8, work.nproc);
        @throw(workNwaitWasWorkNprocˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string workNwaitWorkNprocˢ = "work.nwait > work.nproc"u8;

// gcEndWork signals to the garbage collector that a new worker has just finished
// its work. It reports whether it was the last worker and there's no more work
// to do. If it returns true, the caller must call gcMarkDone.
internal static bool /*last*/ gcEndWork() {
    var incnwait = atomic.Xadd(Ꮡwork.of(workType.Ꮡnwait), +1);
    if (incnwait > work.nproc) {
        println((@string)"runtime: work.nwait="u8, incnwait, (@string)"work.nproc="u8, work.nproc);
        @throw(workNwaitWorkNprocˢ);
    }
    return incnwait == work.nproc && !gcMarkWorkAvailable();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string inGcMarkExpectingToSeeˢ = "in gcMark expecting to see gcphase as _GCmarktermination"u8;
internal static readonly @string pHasCachedGcWorkAtEndOfˢ = "P has cached GC work at end of mark termination"u8;

// gcMark runs the mark (or, for concurrent GC, mark termination)
// All gcWork caches must be empty.
// STW is in effect at this point.
internal static void gcMark(int64 startTime) {
    if (gcphase != _GCmarktermination) {
        @throw(inGcMarkExpectingToSeeˢ);
    }
    work.tstart = startTime;

    // Check that there's no marking work remaining.
    {
        var (next, jobs) = (Ꮡwork.of(workType.ᏑmarkrootNext).Load(), Ꮡwork.of(workType.ᏑmarkrootJobs).Load()); if (work.full != 0 || next < jobs) {
            print((@string)"runtime: full="u8, (Δhex)(uint64)work.full, (@string)" next="u8, next, (@string)" jobs="u8, jobs, (@string)" nDataRoots="u8, work.nDataRoots, (@string)" nBSSRoots="u8, work.nBSSRoots, (@string)" nSpanRoots="u8, work.nSpanRoots, (@string)" nStackRoots="u8, work.nStackRoots, (@string)"\n"u8);
            throw panic("non-empty mark queue after concurrent mark");
        }
    }

    if (debug.gccheckmark > 0) {
        // This is expensive when there's a large number of
        // Gs, so only do it if checkmark is also enabled.
        gcMarkRootCheck();
    }

    // Drop allg snapshot. allgs may have grown, in which case
    // this is the only reference to the old backing store and
    // there's no need to keep it around.
    work.stackRoots = default!;

    // Clear out buffers and double-check that all gcWork caches
    // are empty. This should be ensured by gcMarkDone before we
    // enter mark termination.
    //
    // TODO: We could clear out buffers just before mark if this
    // has a non-negligible impact on STW time.
    foreach (var (_, Δp) in allp) {
        // The write barrier may have buffered pointers since
        // the gcMarkDone barrier. However, since the barrier
        // ensured all reachable objects were marked, all of
        // these must be pointers to black objects. Hence we
        // can just discard the write barrier buffer.
        if (debug.gccheckmark > 0){
            // For debugging, flush the buffer and make
            // sure it really was all marked.
            wbBufFlush1(Δp);
        } else {
            Δp.of(runtime_package.Δp.ᏑwbBuf).reset();
        }

        var gcw = Δp.of(runtime_package.Δp.Ꮡgcw);
        if (!gcw.empty()) {
            printlock();
            print((@string)"runtime: P "u8, (~Δp).id, (@string)" flushedWork "u8, (~gcw).flushedWork);
            if ((~gcw).wbuf1 == nil){
                print((@string)" wbuf1=<nil>"u8);
            } else {
                print((@string)" wbuf1.n="u8, (~(~gcw).wbuf1).nobj);
            }
            if ((~gcw).wbuf2 == nil){
                print((@string)" wbuf2=<nil>"u8);
            } else {
                print((@string)" wbuf2.n="u8, (~(~gcw).wbuf2).nobj);
            }
            print((@string)"\n"u8);
            @throw(pHasCachedGcWorkAtEndOfˢ);
        }
        // There may still be cached empty buffers, which we
        // need to flush since we're going to free them. Also,
        // there may be non-zero stats because we allocated
        // black after the gcMarkDone barrier.
        gcw.dispose();
    }

    // Flush scanAlloc from each mcache since we're about to modify
    // heapScan directly. If we were to flush this later, then scanAlloc
    // might have incorrect information.
    //
    // Note that it's not important to retain this information; we know
    // exactly what heapScan is at this point via scanWork.
    foreach (var (_, Δp) in allp) {
        var c = Δp.Value.mcache;
        if (c == nil) {
            continue;
        }
        c.Value.scanAlloc = 0;
    }

    // Reset controller state.
    ᏑgcController.resetLive(work.bytesMarked);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcSweepBeingDoneButPhaseˢ = "gcSweep being done but phase is not GCoff"u8;

// gcSweep must be called on the system stack because it acquires the heap
// lock. See mheap for details.
//
// Returns true if the heap was fully swept by this function.
//
// The world must be stopped.
//
//go:systemstack
internal static bool gcSweep(gcMode mode) {
    assertWorldStopped();

    if (gcphase != _GCoff) {
        @throw(gcSweepBeingDoneButPhaseˢ);
    }

    @lock(Ꮡmheap_.of(mheap.Ꮡlock));
    mheap_.sweepgen += 2;
    ᏑΔsweep.of(sweepdata.Ꮡactive).reset();
    Ꮡmheap_.of(mheap.ᏑpagesSwept).Store(0);
    mheap_.sweepArenas = mheap_.heapArenas;
    Ꮡmheap_.of(mheap.ᏑreclaimIndex).Store(0);
    Ꮡmheap_.of(mheap.ᏑreclaimCredit).Store(0);
    unlock(Ꮡmheap_.of(mheap.Ꮡlock));

    ᏑΔsweep.of(sweepdata.ᏑcentralIndex).clear();

    if (!concurrentSweep || mode == gcForceBlockMode) {
        // Special case synchronous sweep.
        // Record that no proportional sweeping has to happen.
        @lock(Ꮡmheap_.of(mheap.Ꮡlock));
        mheap_.sweepPagesPerByte = 0D;
        unlock(Ꮡmheap_.of(mheap.Ꮡlock));
        // Flush all mcaches.
        foreach (var (_, pp) in allp) {
            (~pp).mcache.prepareForSweep();
        }
        // Sweep all spans eagerly.
        while (sweepone() != ~(uintptr)0) {
        }
        // Free workbufs and span rings eagerly.
        prepareFreeWorkbufs();
        while (freeSomeWbufs(false)) {
        }
        freeDeadSpanSPMCs();
        // All "free" events for this mark/sweep cycle have
        // now happened, so we can make this profile cycle
        // available immediately.
        mProf_NextCycle();
        mProf_Flush();
        return true;
    }

    // Background sweep.
    @lock(ᏑΔsweep.of(sweepdata.Ꮡlock));
    if (Δsweep.parked) {
        Δsweep.parked = false;
        ready(Δsweep.g, 0, true);
    }
    unlock(ᏑΔsweep.of(sweepdata.Ꮡlock));
    return false;
}

// gcResetMarkState resets global state prior to marking (concurrent
// or STW) and resets the stack scan state of all Gs.
//
// This is safe to do without the world stopped because any Gs created
// during or after this will start out in the reset state.
//
// gcResetMarkState must be called on the system stack because it acquires
// the heap lock. See mheap for details.
//
//go:systemstack
internal static void gcResetMarkState() {
    // This may be called during a concurrent phase, so lock to make sure
    // allgs doesn't change.
    forEachG((ж<g> gp) => {
        gp.Value.gcscandone = false; // set to true in gcphasework
        gp.Value.gcAssistBytes = 0;
    });

    // Clear page marks. This is just 1MB per 64GB of heap, so the
    // time here is pretty trivial.
    @lock(Ꮡmheap_.of(mheap.Ꮡlock));
    var arenas = mheap_.heapArenas;
    unlock(Ꮡmheap_.of(mheap.Ꮡlock));
    foreach (var (_, ai) in arenas) {
        var ha = mheap_.arenas[ai.l1()].Value[ai.l2()];
        builtin.clear((~ha).pageMarks[..]);
    }

    work.bytesMarked = 0;
    work.initialHeapLive = ᏑgcController.of(gcControllerState.ᏑheapLive).Load();
}

// Hooks for other packages
internal static Action poolcleanup;

internal static slice<@unsafe.Pointer> boringCaches;        // for crypto/internal/boring

// sync_runtime_registerPoolCleanup should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/gopkg
//   - github.com/songzhibin97/gkit
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname sync_runtime_registerPoolCleanup sync.runtime_registerPoolCleanup
internal static void sync_runtime_registerPoolCleanup(Action f) {
    poolcleanup = f;
}

//go:linkname boring_registerCache crypto/internal/boring/bcache.registerCache
internal static void boring_registerCache(@unsafe.Pointer Δp) {
    boringCaches = append(boringCaches, Δp);
}

internal static void clearpools() {
    // clear sync.Pools
    if (poolcleanup != default!) {
        poolcleanup();
    }

    // clear boringcrypto caches
    foreach (var (_, Δp) in boringCaches) {
        atomicstorep(Δp, nil);
    }

    // Clear central sudog cache.
    // Leave per-P caches alone, they have strictly bounded size.
    // Disconnect cached list before dropping it on the floor,
    // so that a dangling ref to one entry does not pin all of them.
    @lock(Ꮡsched.of(schedt.Ꮡsudoglock));
    ж<sudog> sg = default!;
    ж<sudog> sgnext = default!;
    for (sg = sched.sudogcache; sg != nil; sg = sgnext) {
        sgnext = sg.Value.next;
        sg.Value.next = default!;
    }
    sched.sudogcache = default!;
    unlock(Ꮡsched.of(schedt.Ꮡsudoglock));

    // Clear central defer pool.
    // Leave per-P pools alone, they have strictly bounded size.
    @lock(Ꮡsched.of(schedt.Ꮡdeferlock));
    // disconnect cached list before dropping it on the floor,
    // so that a dangling ref to one entry does not pin all of them.
    ж<_defer> d = default!;
    ж<_defer> dlink = default!;
    for (d = sched.deferpool; d != nil; d = dlink) {
        dlink = d.Value.link;
        d.Value.link = default!;
    }
    sched.deferpool = default!;
    unlock(Ꮡsched.of(schedt.Ꮡdeferlock));
}

// Timing

// itoaDiv formats val/(10**dec) into buf.
internal static slice<byte> itoaDiv(slice<byte> buf, uint64 val, nint dec) {
    nint i = len(buf) - 1;
    nint idec = i - dec;
    while (val >= 10 || i >= idec) {
        buf[i] = (byte)(val % 10 + (rune)'0');
        i--;
        if (i == idec) {
            buf[i] = (rune)'.';
            i--;
        }
        val /= 10;
    }
    buf[i] = (byte)(val + (rune)'0');
    return buf.slice(i);
}

// fmtNSAsMS nicely formats ns nanoseconds as milliseconds.
internal static slice<byte> fmtNSAsMS(slice<byte> buf, uint64 ns) {
    if (ns >= 10000000) {
        // Format as whole milliseconds.
        return itoaDiv(buf, ns / 1000000, 0);
    }
    // Format two digits of precision, with at most three decimal places.
    var x = ns / 1000;
    if (x == 0) {
        buf[0] = (rune)'0';
        return buf[..1];
    }
    nint dec = 3;
    while (x >= 100) {
        x /= 10;
        dec--;
    }
    return itoaDiv(buf, x, dec);
}

// Helpers for testing GC.

// gcTestMoveStackOnNextCall causes the stack to be moved on a call
// immediately following the call to this. It may not work correctly
// if any other work appears after this call (such as returning).
// Typically the following call should be marked go:noinline so it
// performs a stack check.
//
// In rare cases this may not cause the stack to move, specifically if
// there's a preemption between this call and the next.
internal static void gcTestMoveStackOnNextCall() {
    var gp = getg();
    gp.Value.stackguard0 = stackForceMove;
}

// go2cs generated this placeholder — func gcTestIsReachable is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stackˢ = "stack"u8;
internal static readonly @string dataˢ = "data"u8;
internal static readonly @string bssˢ = "bss"u8;
internal static readonly @string otherˢ = "other"u8;

// gcTestPointerClass returns the category of what p points to, one of:
// "heap", "stack", "data", "bss", "other". This is useful for checking
// that a test is doing what it's intended to do.
//
// This is nosplit simply to avoid extra pointer shuffling that may
// complicate a test.
//
//go:nosplit
internal static @string gcTestPointerClass(@unsafe.Pointer Δp) {
    var p2 = (uintptr)(uintptr)noescape(Δp);
    var gp = getg();
    if ((~gp).stack.lo <= p2 && p2 < (~gp).stack.hi) {
        return stackˢ;
    }
    {
        var (@base, _, _) = findObject(p2, 0, 0); if (@base != 0) {
            return heapˢ;
        }
    }
    foreach (var (_, datap) in activeModules()) {
        if ((~datap).data <= p2 && p2 < (~datap).edata || (~datap).noptrdata <= p2 && p2 < (~datap).enoptrdata) {
            return dataˢ;
        }
        if ((~datap).bss <= p2 && p2 < (~datap).ebss || (~datap).noptrbss <= p2 && p2 <= (~datap).enoptrbss) {
            return bssˢ;
        }
    }
    KeepAlive(@unsafe.Pointer.OrTypedNil(Δp));
    return otherˢ;
}

} // end runtime_package
