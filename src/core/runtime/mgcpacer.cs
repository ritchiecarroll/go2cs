// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cpu = @internal.cpu_package;
using goexperiment = @internal.goexperiment_package;
using atomic = @internal.runtime.atomic_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using @internal;
using @internal.runtime;
using @unsafe = unsafe_package;

partial class runtime_package {

internal static UntypedFloat gcGoalUtilization => /* gcBackgroundUtilization */ 0.25;
internal static UntypedFloat gcBackgroundUtilization => 0.25;
internal static UntypedInt gcCreditSlack => 2000;
internal static UntypedInt gcAssistTimeSlack => 5000;
internal static UntypedInt gcOverAssistWork => /* 64 << 10 */ 65536;
internal static UntypedInt defaultHeapMinimum => /* (goexperiment.HeapMinimum512KiBInt)*(512<<10) +
	(1-goexperiment.HeapMinimum512KiBInt)*(4<<20) */ 4194304;
internal static UntypedInt maxStackScanSlack => /* 8 << 10 */ 8192;
internal static UntypedInt memoryLimitMinHeapGoalHeadroom => /* 1 << 20 */ 1048576;
internal static UntypedInt memoryLimitHeapGoalHeadroomPercent => 3;

// gcController implements the GC pacing controller that determines
// when to trigger concurrent garbage collection and how much marking
// work to do in mutator assists and background marking.
//
// It calculates the ratio between the allocation rate (in terms of CPU
// time) and the GC scan throughput to determine the heap size at which to
// trigger a GC cycle such that no GC assists are required to finish on time.
// This algorithm thus optimizes GC CPU utilization to the dedicated background
// mark utilization of 25% of GOMAXPROCS by minimizing GC assists.
// GOMAXPROCS. The high-level design of this algorithm is documented
// at https://github.com/golang/proposal/blob/master/design/44167-gc-pacer-redesign.md.
// See https://golang.org/s/go15gcpacing for additional historical context.
internal static ж<gcControllerState> ᏑgcController = new StandardBox<gcControllerState>(new gcControllerState());
internal static ref gcControllerState gcController => ref ᏑgcController.Value;

[GoType] partial struct gcControllerState {
    // Initialized from GOGC. GOGC=off means no GC.
    internal atomic.Int32 gcPercent;
    // memoryLimit is the soft memory limit in bytes.
    //
    // Initialized from GOMEMLIMIT. GOMEMLIMIT=off is equivalent to MaxInt64
    // which means no soft memory limit in practice.
    //
    // This is an int64 instead of a uint64 to more easily maintain parity with
    // the SetMemoryLimit API, which sets a maximum at MaxInt64. This value
    // should never be negative.
    internal atomic.Int64 memoryLimit;
    // heapMinimum is the minimum heap size at which to trigger GC.
    // For small heaps, this overrides the usual GOGC*live set rule.
    //
    // When there is a very small live set but a lot of allocation, simply
    // collecting when the heap reaches GOGC*live results in many GC
    // cycles and high total per-GC overhead. This minimum amortizes this
    // per-GC overhead while keeping the heap reasonably small.
    //
    // During initialization this is set to 4MB*GOGC/100. In the case of
    // GOGC==0, this will set heapMinimum to 0, resulting in constant
    // collection even when the heap size is small, which is useful for
    // debugging.
    internal uint64 heapMinimum;
    // runway is the amount of runway in heap bytes allocated by the
    // application that we want to give the GC once it starts.
    //
    // This is computed from consMark during mark termination.
    internal atomic.Uint64 runway;
    // consMark is the estimated per-CPU consMark ratio for the application.
    //
    // It represents the ratio between the application's allocation
    // rate, as bytes allocated per CPU-time, and the GC's scan rate,
    // as bytes scanned per CPU-time.
    // The units of this ratio are (B / cpu-ns) / (B / cpu-ns).
    //
    // At a high level, this value is computed as the bytes of memory
    // allocated (cons) per unit of scan work completed (mark) in a GC
    // cycle, divided by the CPU time spent on each activity.
    //
    // Updated at the end of each GC cycle, in endCycle.
    internal float64 consMark;
    // lastConsMark is the computed cons/mark value for the previous 4 GC
    // cycles. Note that this is *not* the last value of consMark, but the
    // measured cons/mark value in endCycle.
    internal array<float64> lastConsMark = new(4);
    // gcPercentHeapGoal is the goal heapLive for when next GC ends derived
    // from gcPercent.
    //
    // Set to ^uint64(0) if gcPercent is disabled.
    internal atomic.Uint64 gcPercentHeapGoal;
    // sweepDistMinTrigger is the minimum trigger to ensure a minimum
    // sweep distance.
    //
    // This bound is also special because it applies to both the trigger
    // *and* the goal (all other trigger bounds must be based *on* the goal).
    //
    // It is computed ahead of time, at commit time. The theory is that,
    // absent a sudden change to a parameter like gcPercent, the trigger
    // will be chosen to always give the sweeper enough headroom. However,
    // such a change might dramatically and suddenly move up the trigger,
    // in which case we need to ensure the sweeper still has enough headroom.
    internal atomic.Uint64 sweepDistMinTrigger;
    // triggered is the point at which the current GC cycle actually triggered.
    // Only valid during the mark phase of a GC cycle, otherwise set to ^uint64(0).
    //
    // Updated while the world is stopped.
    internal uint64 triggered;
    // lastHeapGoal is the value of heapGoal at the moment the last GC
    // ended. Note that this is distinct from the last value heapGoal had,
    // because it could change if e.g. gcPercent changes.
    //
    // Read and written with the world stopped or with mheap_.lock held.
    internal uint64 lastHeapGoal;
    // heapLive is the number of bytes considered live by the GC.
    // That is: retained by the most recent GC plus allocated
    // since then. heapLive ≤ memstats.totalAlloc-memstats.totalFree, since
    // heapAlloc includes unmarked objects that have not yet been swept (and
    // hence goes up as we allocate and down as we sweep) while heapLive
    // excludes these objects (and hence only goes up between GCs).
    //
    // To reduce contention, this is updated only when obtaining a span
    // from an mcentral and at this point it counts all of the unallocated
    // slots in that span (which will be allocated before that mcache
    // obtains another span from that mcentral). Hence, it slightly
    // overestimates the "true" live heap size. It's better to overestimate
    // than to underestimate because 1) this triggers the GC earlier than
    // necessary rather than potentially too late and 2) this leads to a
    // conservative GC rate rather than a GC rate that is potentially too
    // low.
    //
    // Whenever this is updated, call traceHeapAlloc() and
    // this gcControllerState's revise() method.
    internal atomic.Uint64 heapLive;
    // heapScan is the number of bytes of "scannable" heap. This is the
    // live heap (as counted by heapLive), but omitting no-scan objects and
    // no-scan tails of objects.
    //
    // This value is fixed at the start of a GC cycle. It represents the
    // maximum scannable heap.
    internal atomic.Uint64 heapScan;
    // lastHeapScan is the number of bytes of heap that were scanned
    // last GC cycle. It is the same as heapMarked, but only
    // includes the "scannable" parts of objects.
    //
    // Updated when the world is stopped.
    internal uint64 lastHeapScan;
    // lastStackScan is the number of bytes of stack that were scanned
    // last GC cycle.
    internal atomic.Uint64 lastStackScan;
    // maxStackScan is the amount of allocated goroutine stack space in
    // use by goroutines.
    //
    // This number tracks allocated goroutine stack space rather than used
    // goroutine stack space (i.e. what is actually scanned) because used
    // goroutine stack space is much harder to measure cheaply. By using
    // allocated space, we make an overestimate; this is OK, it's better
    // to conservatively overcount than undercount.
    internal atomic.Uint64 maxStackScan;
    // globalsScan is the total amount of global variable space
    // that is scannable.
    internal atomic.Uint64 globalsScan;
    // heapMarked is the number of bytes marked by the previous
    // GC. After mark termination, heapLive == heapMarked, but
    // unlike heapLive, heapMarked does not change until the
    // next mark termination.
    internal uint64 heapMarked;
    // heapScanWork is the total heap scan work performed this cycle.
    // stackScanWork is the total stack scan work performed this cycle.
    // globalsScanWork is the total globals scan work performed this cycle.
    //
    // These are updated atomically during the cycle. Updates occur in
    // bounded batches, since they are both written and read
    // throughout the cycle. At the end of the cycle, heapScanWork is how
    // much of the retained heap is scannable.
    //
    // Currently these are measured in bytes. For most uses, this is an
    // opaque unit of work, but for estimation the definition is important.
    //
    // Note that stackScanWork includes only stack space scanned, not all
    // of the allocated stack.
    internal atomic.Int64 heapScanWork;
    internal atomic.Int64 stackScanWork;
    internal atomic.Int64 globalsScanWork;
    // bgScanCredit is the scan work credit accumulated by the concurrent
    // background scan. This credit is accumulated by the background scan
    // and stolen by mutator assists.  Updates occur in bounded batches,
    // since it is both written and read throughout the cycle.
    internal atomic.Int64 bgScanCredit;
    // assistTime is the nanoseconds spent in mutator assists
    // during this cycle. This is updated atomically, and must also
    // be updated atomically even during a STW, because it is read
    // by sysmon. Updates occur in bounded batches, since it is both
    // written and read throughout the cycle.
    internal atomic.Int64 assistTime;
    // dedicatedMarkTime is the nanoseconds spent in dedicated mark workers
    // during this cycle. This is updated at the end of the concurrent mark
    // phase.
    internal atomic.Int64 dedicatedMarkTime;
    // fractionalMarkTime is the nanoseconds spent in the fractional mark
    // worker during this cycle. This is updated throughout the cycle and
    // will be up-to-date if the fractional mark worker is not currently
    // running.
    internal atomic.Int64 fractionalMarkTime;
    // idleMarkTime is the nanoseconds spent in idle marking during this
    // cycle. This is updated throughout the cycle.
    internal atomic.Int64 idleMarkTime;
    // markStartTime is the absolute start time in nanoseconds
    // that assists and background mark workers started.
    internal int64 markStartTime;
    // dedicatedMarkWorkersNeeded is the number of dedicated mark workers
    // that need to be started. This is computed at the beginning of each
    // cycle and decremented as dedicated mark workers get started.
    internal atomic.Int64 dedicatedMarkWorkersNeeded;
    // idleMarkWorkers is two packed int32 values in a single uint64.
    // These two values are always updated simultaneously.
    //
    // The bottom int32 is the current number of idle mark workers executing.
    //
    // The top int32 is the maximum number of idle mark workers allowed to
    // execute concurrently. Normally, this number is just gomaxprocs. However,
    // during periodic GC cycles it is set to 0 because the system is idle
    // anyway; there's no need to go full blast on all of GOMAXPROCS.
    //
    // The maximum number of idle mark workers is used to prevent new workers
    // from starting, but it is not a hard maximum. It is possible (but
    // exceedingly rare) for the current number of idle mark workers to
    // transiently exceed the maximum. This could happen if the maximum changes
    // just after a GC ends, and an M with no P.
    //
    // Note that if we have no dedicated mark workers, we set this value to
    // 1 in this case we only have fractional GC workers which aren't scheduled
    // strictly enough to ensure GC progress. As a result, idle-priority mark
    // workers are vital to GC progress in these situations.
    //
    // For example, consider a situation in which goroutines block on the GC
    // (such as via runtime.GOMAXPROCS) and only fractional mark workers are
    // scheduled (e.g. GOMAXPROCS=1). Without idle-priority mark workers, the
    // last running M might skip scheduling a fractional mark worker if its
    // utilization goal is met, such that once it goes to sleep (because there's
    // nothing to do), there will be nothing else to spin up a new M for the
    // fractional worker in the future, stalling GC progress and causing a
    // deadlock. However, idle-priority workers will *always* run when there is
    // nothing left to do, ensuring the GC makes progress.
    //
    // See github.com/golang/go/issues/44163 for more details.
    internal atomic.Uint64 idleMarkWorkers;
    // assistWorkPerByte is the ratio of scan work to allocated
    // bytes that should be performed by mutator assists. This is
    // computed at the beginning of each cycle and updated every
    // time heapScan is updated.
    internal atomic.Float64 assistWorkPerByte;
    // assistBytesPerWork is 1/assistWorkPerByte.
    //
    // Note that because this is read and written independently
    // from assistWorkPerByte users may notice a skew between
    // the two values, and such a state should be safe.
    internal atomic.Float64 assistBytesPerWork;
    // fractionalUtilizationGoal is the fraction of wall clock
    // time that should be spent in the fractional mark worker on
    // each P that isn't running a dedicated worker.
    //
    // For example, if the utilization goal is 25% and there are
    // no dedicated workers, this will be 0.25. If the goal is
    // 25%, there is one dedicated worker, and GOMAXPROCS is 5,
    // this will be 0.05 to make up the missing 5%.
    //
    // If this is zero, no fractional workers are needed.
    internal float64 fractionalUtilizationGoal;
    // These memory stats are effectively duplicates of fields from
    // memstats.heapStats but are updated atomically or with the world
    // stopped and don't provide the same consistency guarantees.
    //
    // Because the runtime is responsible for managing a memory limit, it's
    // useful to couple these stats more tightly to the gcController, which
    // is intimately connected to how that memory limit is maintained.
    internal sysMemStat heapInUse;    // bytes in mSpanInUse spans
    internal sysMemStat heapReleased;    // bytes released to the OS
    internal sysMemStat heapFree;    // bytes not in any span, but not released to the OS
    internal atomic.Uint64 totalAlloc; // total bytes allocated
    internal atomic.Uint64 totalFree; // total bytes freed
    internal atomic.Uint64 mappedReady; // total virtual memory in the Ready state (see mem.go).
    // test indicates that this is a test-only copy of gcControllerState.
    internal bool test;
    internal cpu.CacheLinePad _;
}

internal static void init(this ж<gcControllerState> Ꮡc, int32 gcPercent, int64 memoryLimit) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.heapMinimum = defaultHeapMinimum;
    c.triggered = ~(uint64)0;
    Ꮡc.setGCPercent(gcPercent);
    Ꮡc.setMemoryLimit(memoryLimit);
    Ꮡc.commit(true); // No sweep phase in the first GC cycle.
}

// N.B. Don't bother calling traceHeapGoal. Tracing is never enabled at
// initialization time.
// N.B. No need to call revise; there's no GC enabled during
// initialization.

// startCycle resets the GC controller's state and computes estimates
// for a new GC cycle. The caller must hold worldsema and the world
// must be stopped.
internal static void startCycle(this ж<gcControllerState> Ꮡc, int64 markStartTime, nint procs, gcTrigger trigger) {
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.of(gcControllerState.ᏑheapScanWork).Store(0);
    Ꮡc.of(gcControllerState.ᏑstackScanWork).Store(0);
    Ꮡc.of(gcControllerState.ᏑglobalsScanWork).Store(0);
    Ꮡc.of(gcControllerState.ᏑbgScanCredit).Store(0);
    Ꮡc.of(gcControllerState.ᏑassistTime).Store(0);
    Ꮡc.of(gcControllerState.ᏑdedicatedMarkTime).Store(0);
    Ꮡc.of(gcControllerState.ᏑfractionalMarkTime).Store(0);
    Ꮡc.of(gcControllerState.ᏑidleMarkTime).Store(0);
    c.markStartTime = markStartTime;
    c.triggered = Ꮡc.of(gcControllerState.ᏑheapLive).Load();
    // Compute the background mark utilization goal. In general,
    // this may not come out exactly. We round the number of
    // dedicated workers so that the utilization is closest to
    // 25%. For small GOMAXPROCS, this would introduce too much
    // error, so we add fractional workers in that case.
    var totalUtilizationGoal = (float64)procs * (float64)gcBackgroundUtilization;
    var dedicatedMarkWorkersNeeded = (int64)(totalUtilizationGoal + 0.5D);
    var utilError = (float64)dedicatedMarkWorkersNeeded / totalUtilizationGoal - 1D;
    UntypedFloat maxUtilError = 0.3;
    if (utilError < /* -maxUtilError */ -0.3D || utilError > maxUtilError){
        // Rounding put us more than 30% off our goal. With
        // gcBackgroundUtilization of 25%, this happens for
        // GOMAXPROCS<=3 or GOMAXPROCS=6. Enable fractional
        // workers to compensate.
        if ((float64)dedicatedMarkWorkersNeeded > totalUtilizationGoal) {
            // Too many dedicated workers.
            dedicatedMarkWorkersNeeded--;
        }
        c.fractionalUtilizationGoal = (totalUtilizationGoal - (float64)dedicatedMarkWorkersNeeded) / (float64)procs;
    } else {
        c.fractionalUtilizationGoal = 0D;
    }
    // In STW mode, we just want dedicated workers.
    if (debug.gcstoptheworld > 0) {
        dedicatedMarkWorkersNeeded = (int64)procs;
        c.fractionalUtilizationGoal = 0D;
    }
    // Clear per-P state
    foreach (var (_, Δp) in allp) {
        Δp.Value.gcAssistTime = 0;
        Δp.Value.gcFractionalMarkTime = 0;
    }
    if (trigger.kind == gcTriggerTime){
        // During a periodic GC cycle, reduce the number of idle mark workers
        // required. However, we need at least one dedicated mark worker or
        // idle GC worker to ensure GC progress in some scenarios (see comment
        // on maxIdleMarkWorkers).
        if (dedicatedMarkWorkersNeeded > 0){
            Ꮡc.setMaxIdleMarkWorkers(0);
        } else {
            // TODO(mknyszek): The fundamental reason why we need this is because
            // we can't count on the fractional mark worker to get scheduled.
            // Fix that by ensuring it gets scheduled according to its quota even
            // if the rest of the application is idle.
            Ꮡc.setMaxIdleMarkWorkers(1);
        }
    } else {
        // N.B. gomaxprocs and dedicatedMarkWorkersNeeded are guaranteed not to
        // change during a GC cycle.
        Ꮡc.setMaxIdleMarkWorkers((int32)procs - (int32)dedicatedMarkWorkersNeeded);
    }
    // Compute initial values for controls that are updated
    // throughout the cycle.
    Ꮡc.of(gcControllerState.ᏑdedicatedMarkWorkersNeeded).Store(dedicatedMarkWorkersNeeded);
    Ꮡc.revise();
    if (debug.gcpacertrace > 0) {
        var heapGoal = Ꮡc.heapGoal();
        var assistRatio = Ꮡc.of(gcControllerState.ᏑassistWorkPerByte).Load();
        print((@string)"pacer: assist ratio="u8, assistRatio,
            (@string)" (scan "u8, (ᏑgcController.of(gcControllerState.ᏑheapScan).Load() >> (int)(20)), (@string)" MB in "u8,
            (work.initialHeapLive >> (int)(20)), (@string)"->"u8,
            (heapGoal >> (int)(20)), (@string)" MB)"u8,
            (@string)" workers="u8, dedicatedMarkWorkersNeeded,
            (@string)"+"u8, c.fractionalUtilizationGoal, (@string)"\n"u8);
    }
}

// revise updates the assist ratio during the GC cycle to account for
// improved estimates. This should be called whenever gcController.heapScan,
// gcController.heapLive, or if any inputs to gcController.heapGoal are
// updated. It is safe to call concurrently, but it may race with other
// calls to revise.
//
// The result of this race is that the two assist ratio values may not line
// up or may be stale. In practice this is OK because the assist ratio
// moves slowly throughout a GC cycle, and the assist ratio is a best-effort
// heuristic anyway. Furthermore, no part of the heuristic depends on
// the two assist ratio values being exact reciprocals of one another, since
// the two values are used to convert values from different sources.
//
// The worst case result of this raciness is that we may miss a larger shift
// in the ratio (say, if we decide to pace more aggressively against the
// hard heap goal) but even this "hard goal" is best-effort (see #40460).
// The dedicated GC should ensure we don't exceed the hard goal by too much
// in the rare case we do exceed it.
//
// It should only be called when gcBlackenEnabled != 0 (because this
// is when assists are enabled and the necessary statistics are
// available).
internal static void revise(this ж<gcControllerState> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var gcPercent = Ꮡc.of(gcControllerState.ᏑgcPercent).Load();
    if (gcPercent < 0) {
        // If GC is disabled but we're running a forced GC,
        // act like GOGC is huge for the below calculations.
        gcPercent = 100000;
    }
    var live = Ꮡc.of(gcControllerState.ᏑheapLive).Load();
    var scan = Ꮡc.of(gcControllerState.ᏑheapScan).Load();
    var work = Ꮡc.of(gcControllerState.ᏑheapScanWork).Load() + Ꮡc.of(gcControllerState.ᏑstackScanWork).Load() + Ꮡc.of(gcControllerState.ᏑglobalsScanWork).Load();
    // Assume we're under the soft goal. Pace GC to complete at
    // heapGoal assuming the heap is in steady-state.
    var heapGoal = (int64)Ꮡc.heapGoal();
    // The expected scan work is computed as the amount of bytes scanned last
    // GC cycle (both heap and stack), plus our estimate of globals work for this cycle.
    var scanWorkExpected = (int64)(c.lastHeapScan + Ꮡc.of(gcControllerState.ᏑlastStackScan).Load() + Ꮡc.of(gcControllerState.ᏑglobalsScan).Load());
    // maxScanWork is a worst-case estimate of the amount of scan work that
    // needs to be performed in this GC cycle. Specifically, it represents
    // the case where *all* scannable memory turns out to be live, and
    // *all* allocated stack space is scannable.
    var maxStackScan = Ꮡc.of(gcControllerState.ᏑmaxStackScan).Load();
    var maxScanWork = (int64)(scan + maxStackScan + Ꮡc.of(gcControllerState.ᏑglobalsScan).Load());
    if (work > scanWorkExpected) {
        // We've already done more scan work than expected. Because our expectation
        // is based on a steady-state scannable heap size, we assume this means our
        // heap is growing. Compute a new heap goal that takes our existing runway
        // computed for scanWorkExpected and extrapolates it to maxScanWork, the worst-case
        // scan work. This keeps our assist ratio stable if the heap continues to grow.
        //
        // The effect of this mechanism is that assists stay flat in the face of heap
        // growths. It's OK to use more memory this cycle to scan all the live heap,
        // because the next GC cycle is inevitably going to use *at least* that much
        // memory anyway.
        var extHeapGoal = (int64)((float64)(heapGoal - (int64)c.triggered) / (float64)scanWorkExpected * (float64)maxScanWork) + (int64)c.triggered;
        scanWorkExpected = maxScanWork;
        // hardGoal is a hard limit on the amount that we're willing to push back the
        // heap goal, and that's twice the heap goal (i.e. if GOGC=100 and the heap and/or
        // stacks and/or globals grow to twice their size, this limits the current GC cycle's
        // growth to 4x the original live heap's size).
        //
        // This maintains the invariant that we use no more memory than the next GC cycle
        // will anyway.
        var hardGoal = (int64)((1.0D + (float64)gcPercent / 100.0D) * (float64)heapGoal);
        if (extHeapGoal > hardGoal) {
            extHeapGoal = hardGoal;
        }
        heapGoal = extHeapGoal;
    }
    if ((int64)live > heapGoal) {
        // We're already past our heap goal, even the extrapolated one.
        // Leave ourselves some extra runway, so in the worst case we
        // finish by that point.
        const float64 maxOvershoot = 1.1;
        heapGoal = (int64)((float64)heapGoal * maxOvershoot);
        // Compute the upper bound on the scan work remaining.
        scanWorkExpected = maxScanWork;
    }
    // Compute the remaining scan work estimate.
    //
    // Note that we currently count allocations during GC as both
    // scannable heap (heapScan) and scan work completed
    // (scanWork), so allocation will change this difference
    // slowly in the soft regime and not at all in the hard
    // regime.
    var scanWorkRemaining = scanWorkExpected - work;
    if (scanWorkRemaining < 1000) {
        // We set a somewhat arbitrary lower bound on
        // remaining scan work since if we aim a little high,
        // we can miss by a little.
        //
        // We *do* need to enforce that this is at least 1,
        // since marking is racy and double-scanning objects
        // may legitimately make the remaining scan work
        // negative, even in the hard goal regime.
        scanWorkRemaining = 1000;
    }
    // Compute the heap distance remaining.
    var heapRemaining = heapGoal - (int64)live;
    if (heapRemaining <= 0) {
        // This shouldn't happen, but if it does, avoid
        // dividing by zero or setting the assist negative.
        heapRemaining = 1;
    }
    // Compute the mutator assist ratio so by the time the mutator
    // allocates the remaining heap bytes up to heapGoal, it will
    // have done (or stolen) the remaining amount of scan work.
    // Note that the assist ratio values are updated atomically
    // but not together. This means there may be some degree of
    // skew between the two values. This is generally OK as the
    // values shift relatively slowly over the course of a GC
    // cycle.
    var assistWorkPerByte = (float64)scanWorkRemaining / (float64)heapRemaining;
    var assistBytesPerWork = (float64)heapRemaining / (float64)scanWorkRemaining;
    Ꮡc.of(gcControllerState.ᏑassistWorkPerByte).Store(assistWorkPerByte);
    Ꮡc.of(gcControllerState.ᏑassistBytesPerWork).Store(assistBytesPerWork);
}

// endCycle computes the consMark estimate for the next cycle.
// userForced indicates whether the current GC cycle was forced
// by the application.
internal static void endCycle(this ж<gcControllerState> Ꮡc, int64 now, nint procs, bool userForced) {
    ref var c = ref Ꮡc.DerefOrNull();

    // Record last heap goal for the scavenger.
    // We'll be updating the heap goal soon.
    gcController.lastHeapGoal = Ꮡc.heapGoal();
    // Compute the duration of time for which assists were turned on.
    var assistDuration = now - c.markStartTime;
    // Assume background mark hit its utilization goal.
    float64 utilization = gcBackgroundUtilization;
    // Add assist utilization; avoid divide by zero.
    if (assistDuration > 0) {
        utilization += (float64)Ꮡc.of(gcControllerState.ᏑassistTime).Load() / (float64)(assistDuration * (int64)procs);
    }
    if (Ꮡc.of(gcControllerState.ᏑheapLive).Load() <= c.triggered) {
        // Shouldn't happen, but let's be very safe about this in case the
        // GC is somehow extremely short.
        //
        // In this case though, the only reasonable value for c.heapLive-c.triggered
        // would be 0, which isn't really all that useful, i.e. the GC was so short
        // that it didn't matter.
        //
        // Ignore this case and don't update anything.
        return;
    }
    var idleUtilization = 0.0D;
    if (assistDuration > 0) {
        idleUtilization = (float64)Ꮡc.of(gcControllerState.ᏑidleMarkTime).Load() / (float64)(assistDuration * (int64)procs);
    }
    // Determine the cons/mark ratio.
    //
    // The units we want for the numerator and denominator are both B / cpu-ns.
    // We get this by taking the bytes allocated or scanned, and divide by the amount of
    // CPU time it took for those operations. For allocations, that CPU time is
    //
    //    assistDuration * procs * (1 - utilization)
    //
    // Where utilization includes just background GC workers and assists. It does *not*
    // include idle GC work time, because in theory the mutator is free to take that at
    // any point.
    //
    // For scanning, that CPU time is
    //
    //    assistDuration * procs * (utilization + idleUtilization)
    //
    // In this case, we *include* idle utilization, because that is additional CPU time that
    // the GC had available to it.
    //
    // In effect, idle GC time is sort of double-counted here, but it's very weird compared
    // to other kinds of GC work, because of how fluid it is. Namely, because the mutator is
    // *always* free to take it.
    //
    // So this calculation is really:
    //     (heapLive-trigger) / (assistDuration * procs * (1-utilization)) /
    //         (scanWork) / (assistDuration * procs * (utilization+idleUtilization))
    //
    // Note that because we only care about the ratio, assistDuration and procs cancel out.
    var scanWork = Ꮡc.of(gcControllerState.ᏑheapScanWork).Load() + Ꮡc.of(gcControllerState.ᏑstackScanWork).Load() + Ꮡc.of(gcControllerState.ᏑglobalsScanWork).Load();
    var currentConsMark = ((float64)(Ꮡc.of(gcControllerState.ᏑheapLive).Load() - c.triggered) * (utilization + idleUtilization)) / ((float64)scanWork * (1D - utilization));
    // Update our cons/mark estimate. This is the maximum of the value we just computed and the last
    // 4 cons/mark values we measured. The reason we take the maximum here is to bias a noisy
    // cons/mark measurement toward fewer assists at the expense of additional GC cycles (starting
    // earlier).
    var oldConsMark = c.consMark;
    c.consMark = currentConsMark;
    foreach (var (i, _) in c.lastConsMark) {
        if (c.lastConsMark[i] > c.consMark) {
            c.consMark = c.lastConsMark[i];
        }
    }
    copy(c.lastConsMark[..], c.lastConsMark[1..]);
    c.lastConsMark[len(c.lastConsMark) - 1] = currentConsMark;
    if (debug.gcpacertrace > 0) {
        printlock();
        var goal = gcGoalUtilization * 100D;
        print((@string)"pacer: "u8, (nint)(utilization * 100D), (@string)"% CPU ("u8, (nint)goal, (@string)" exp.) for "u8);
        print(Ꮡc.of(gcControllerState.ᏑheapScanWork).Load(), (@string)"+"u8, Ꮡc.of(gcControllerState.ᏑstackScanWork).Load(), (@string)"+"u8, Ꮡc.of(gcControllerState.ᏑglobalsScanWork).Load(), (@string)" B work ("u8, c.lastHeapScan + Ꮡc.of(gcControllerState.ᏑlastStackScan).Load() + Ꮡc.of(gcControllerState.ᏑglobalsScan).Load(), (@string)" B exp.) "u8);
        var live = Ꮡc.of(gcControllerState.ᏑheapLive).Load();
        print((@string)"in "u8, c.triggered, (@string)" B -> "u8, live, (@string)" B (∆goal "u8, (int64)live - (int64)c.lastHeapGoal, (@string)", cons/mark "u8, oldConsMark, (@string)")"u8);
        println();
        printunlock();
    }
}

// enlistWorker encourages another dedicated mark worker to start on
// another P if there are spare worker slots. It is used by putfull
// when more work is made available.
//
//go:nowritebarrier
internal static void enlistWorker(this ж<gcControllerState> Ꮡc) {
    // If there are idle Ps, wake one so it will run an idle worker.
    // NOTE: This is suspected of causing deadlocks. See golang.org/issue/19112.
    //
    //	if sched.npidle.Load() != 0 && sched.nmspinning.Load() == 0 {
    //		wakep()
    //		return
    //	}
    // There are no idle Ps. If we need more dedicated workers,
    // try to preempt a running P so it will switch to a worker.
    if (Ꮡc.of(gcControllerState.ᏑdedicatedMarkWorkersNeeded).Load() <= 0) {
        return;
    }
    // Pick a random other P to preempt.
    if (gomaxprocs <= 1) {
        return;
    }
    var gp = getg();
    if (gp == nil || (~gp).m == nil || (~(~gp).m).p == 0) {
        return;
    }
    var myID = (~(~gp).m).p.ptr().Value.id;
    for (nint tries = 0; tries < 5; tries++) {
        var id = (int32)cheaprandn((uint32)(gomaxprocs - 1));
        if (id >= myID) {
            id++;
        }
        var Δp = allp[id];
        if ((~Δp).status != _Prunning) {
            continue;
        }
        if (preemptone(ref (Δp).DerefOrNull())) {
            return;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcControllerStateˢ = "gcControllerState.findRunnable: blackening not enabled"u8;

// findRunnableGCWorker returns a background mark worker for pp if it
// should be run. This must only be called when gcBlackenEnabled != 0.
internal static (ж<g>, int64) findRunnableGCWorker(this ж<gcControllerState> Ꮡc, ж<Δp> Ꮡpp, int64 now) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var pp = ref Ꮡpp.DerefOrNull();

    if (gcBlackenEnabled == 0) {
        @throw(gcControllerStateˢ);
    }
    // Since we have the current time, check if the GC CPU limiter
    // hasn't had an update in a while. This check is necessary in
    // case the limiter is on but hasn't been checked in a while and
    // so may have left sufficient headroom to turn off again.
    if (now == 0) {
        now = nanotime();
    }
    if (ᏑgcCPULimiter.needUpdate(now)) {
        ᏑgcCPULimiter.update(now);
    }
    if (!gcMarkWorkAvailable(Ꮡpp)) {
        // No work to be done right now. This can happen at
        // the end of the mark phase when there are still
        // assists tapering off. Don't bother running a worker
        // now because it'll just return immediately.
        return (default!, now);
    }
    // Grab a worker before we commit to running below.
    var node = (ж<gcBgMarkWorkerNode>)(uintptr)(ᏑgcBgMarkWorkerPool.pop());
    if (node == nil) {
        // There is at least one worker per P, so normally there are
        // enough workers to run on all Ps, if necessary. However, once
        // a worker enters gcMarkDone it may park without rejoining the
        // pool, thus freeing a P with no corresponding worker.
        // gcMarkDone never depends on another worker doing work, so it
        // is safe to simply do nothing here.
        //
        // If gcMarkDone bails out without completing the mark phase,
        // it will always do so with queued global work. Thus, that P
        // will be immediately eligible to re-run the worker G it was
        // just using, ensuring work can complete.
        return (default!, now);
    }
    bool decIfPositive(ж<atomic.Int64> val) {
        while (ᐧ) {
            var v = val.Load();
            if (v <= 0) {
                return false;
            }
            if (val.CompareAndSwap(v, v - 1)) {
                return true;
            }
        }
    }
    if (decIfPositive(Ꮡc.of(gcControllerState.ᏑdedicatedMarkWorkersNeeded))){
        // This P is now dedicated to marking until the end of
        // the concurrent mark phase.
        pp.gcMarkWorkerMode = gcMarkWorkerDedicatedMode;
    } else 
    if (c.fractionalUtilizationGoal == 0D){
        // No need for fractional workers.
        ᏑgcBgMarkWorkerPool.push(node.of(gcBgMarkWorkerNode.Ꮡnode));
        return (default!, now);
    } else {
        // Is this P behind on the fractional utilization
        // goal?
        //
        // This should be kept in sync with pollFractionalWorkerExit.
        var delta = now - c.markStartTime;
        if (delta > 0 && (float64)pp.gcFractionalMarkTime / (float64)delta > c.fractionalUtilizationGoal) {
            // Nope. No need to run a fractional worker.
            ᏑgcBgMarkWorkerPool.push(node.of(gcBgMarkWorkerNode.Ꮡnode));
            return (default!, now);
        }
        // Run a fractional worker.
        pp.gcMarkWorkerMode = gcMarkWorkerFractionalMode;
    }
    // Run the background mark worker.
    var gp = (~node).gp.ptr();
    var Δtrace = traceAcquire();
    casgstatus(gp, _Gwaiting, _Grunnable);
    if (Δtrace.ok()) {
        Δtrace.GoUnpark(gp, 0);
        traceRelease(Δtrace);
    }
    return (gp, now);
}

// resetLive sets up the controller state for the next mark phase after the end
// of the previous one. Must be called after endCycle and before commit, before
// the world is started.
//
// The world must be stopped.
internal static void resetLive(this ж<gcControllerState> Ꮡc, uint64 bytesMarked) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.heapMarked = bytesMarked;
    Ꮡc.of(gcControllerState.ᏑheapLive).Store(bytesMarked);
    Ꮡc.of(gcControllerState.ᏑheapScan).Store((uint64)Ꮡc.of(gcControllerState.ᏑheapScanWork).Load());
    c.lastHeapScan = (uint64)Ꮡc.of(gcControllerState.ᏑheapScanWork).Load();
    Ꮡc.of(gcControllerState.ᏑlastStackScan).Store((uint64)Ꮡc.of(gcControllerState.ᏑstackScanWork).Load());
    c.triggered = ~(uint64)0; // Reset triggered.
    // heapLive was updated, so emit a trace event.
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.HeapAlloc(bytesMarked);
        traceRelease(Δtrace);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string markWorkerStopUnknownˢ = "markWorkerStop: unknown mark worker mode"u8;

// markWorkerStop must be called whenever a mark worker stops executing.
//
// It updates mark work accounting in the controller by a duration of
// work in nanoseconds and other bookkeeping.
//
// Safe to execute at any time.
internal static void markWorkerStop(this ж<gcControllerState> Ꮡc, gcMarkWorkerMode mode, int64 duration) {
    var exprᴛ1 = mode;
    if (exprᴛ1 == gcMarkWorkerDedicatedMode) {
        Ꮡc.of(gcControllerState.ᏑdedicatedMarkTime).Add(duration);
        Ꮡc.of(gcControllerState.ᏑdedicatedMarkWorkersNeeded).Add(1);
    }
    else if (exprᴛ1 == gcMarkWorkerFractionalMode) {
        Ꮡc.of(gcControllerState.ᏑfractionalMarkTime).Add(duration);
    }
    else if (exprᴛ1 == gcMarkWorkerIdleMode) {
        Ꮡc.of(gcControllerState.ᏑidleMarkTime).Add(duration);
        Ꮡc.removeIdleMarkWorker();
    }
    else { /* default: */
        @throw(markWorkerStopUnknownˢ);
    }

}

internal static void update(this ж<gcControllerState> Ꮡc, int64 dHeapLive, int64 dHeapScan) {
    if (dHeapLive != 0) {
        var Δtrace = traceAcquire();
        var live = ᏑgcController.of(gcControllerState.ᏑheapLive).Add(dHeapLive);
        if (Δtrace.ok()) {
            // gcController.heapLive changed.
            Δtrace.HeapAlloc(live);
            traceRelease(Δtrace);
        }
    }
    if (gcBlackenEnabled == 0){
        // Update heapScan when we're not in a current GC. It is fixed
        // at the beginning of a cycle.
        if (dHeapScan != 0) {
            ᏑgcController.of(gcControllerState.ᏑheapScan).Add(dHeapScan);
        }
    } else {
        // gcController.heapLive changed.
        Ꮡc.revise();
    }
}

internal static void addScannableStack(this ж<gcControllerState> Ꮡc, ж<Δp> Ꮡpp, int64 amount) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    if (Ꮡpp == nil) {
        Ꮡc.of(gcControllerState.ᏑmaxStackScan).Add(amount);
        return;
    }
    pp.maxStackScanDelta += amount;
    if (pp.maxStackScanDelta >= maxStackScanSlack || pp.maxStackScanDelta <= (int64)(-maxStackScanSlack)) {
        Ꮡc.of(gcControllerState.ᏑmaxStackScan).Add(pp.maxStackScanDelta);
        pp.maxStackScanDelta = 0;
    }
}

internal static void addGlobals(this ж<gcControllerState> Ꮡc, int64 amount) {
    Ꮡc.of(gcControllerState.ᏑglobalsScan).Add(amount);
}

// heapGoal returns the current heap goal.
internal static uint64 heapGoal(this ж<gcControllerState> Ꮡc) {
    var (goal, _) = Ꮡc.heapGoalInternal();
    return goal;
}

// heapGoalInternal is the implementation of heapGoal which returns additional
// information that is necessary for computing the trigger.
//
// The returned minTrigger is always <= goal.
internal static (uint64 goal, uint64 minTrigger) heapGoalInternal(this ж<gcControllerState> Ꮡc) {
    uint64 goal = default!;
    uint64 minTrigger = default!;

    ref var c = ref Ꮡc.DerefOrNull();
    // Start with the goal calculated for gcPercent.
    goal = Ꮡc.of(gcControllerState.ᏑgcPercentHeapGoal).Load();
    // Check if the memory-limit-based goal is smaller, and if so, pick that.
    {
        var newGoal = Ꮡc.memoryLimitHeapGoal(); if (newGoal < goal){
            goal = newGoal;
        } else {
            // We're not limited by the memory limit goal, so perform a series of
            // adjustments that might move the goal forward in a variety of circumstances.
            var sweepDistTrigger = Ꮡc.of(gcControllerState.ᏑsweepDistMinTrigger).Load();
            if (sweepDistTrigger > goal) {
                // Set the goal to maintain a minimum sweep distance since
                // the last call to commit. Note that we never want to do this
                // if we're in the memory limit regime, because it could push
                // the goal up.
                goal = sweepDistTrigger;
            }
            // Since we ignore the sweep distance trigger in the memory
            // limit regime, we need to ensure we don't propagate it to
            // the trigger, because it could cause a violation of the
            // invariant that the trigger < goal.
            minTrigger = sweepDistTrigger;
            // Ensure that the heap goal is at least a little larger than
            // the point at which we triggered. This may not be the case if GC
            // start is delayed or if the allocation that pushed gcController.heapLive
            // over trigger is large or if the trigger is really close to
            // GOGC. Assist is proportional to this distance, so enforce a
            // minimum distance, even if it means going over the GOGC goal
            // by a tiny bit.
            //
            // Ignore this if we're in the memory limit regime: we'd prefer to
            // have the GC respond hard about how close we are to the goal than to
            // push the goal back in such a manner that it could cause us to exceed
            // the memory limit.
            const uint64 minRunway = /* 64 << 10 */ 65536;
            if (c.triggered != ~(uint64)0 && goal < c.triggered + minRunway) {
                goal = c.triggered + minRunway;
            }
        }
    }
    return (goal, minTrigger);
}

// memoryLimitHeapGoal returns a heap goal derived from memoryLimit.
internal static uint64 memoryLimitHeapGoal(this ж<gcControllerState> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    // Start by pulling out some values we'll need. Be careful about overflow.
    uint64 heapFree = default!;
    uint64 heapAlloc = default!;
    uint64 mappedReady = default!;
    while (ᐧ) {
        heapFree = Ꮡc.of(gcControllerState.ᏑheapFree).load(); // Free and unscavenged memory.
        heapAlloc = Ꮡc.of(gcControllerState.ᏑtotalAlloc).Load() - Ꮡc.of(gcControllerState.ᏑtotalFree).Load(); // Heap object bytes in use.
        mappedReady = Ꮡc.of(gcControllerState.ᏑmappedReady).Load(); // Total unreleased mapped memory.
        if (heapFree + heapAlloc <= mappedReady) {
            break;
        }
    }
    // It is impossible for total unreleased mapped memory to exceed heap memory, but
    // because these stats are updated independently, we may observe a partial update
    // including only some values. Thus, we appear to break the invariant. However,
    // this condition is necessarily transient, so just try again. In the case of a
    // persistent accounting error, we'll deadlock here.
    // Below we compute a goal from memoryLimit. There are a few things to be aware of.
    // Firstly, the memoryLimit does not easily compare to the heap goal: the former
    // is total mapped memory by the runtime that hasn't been released, while the latter is
    // only heap object memory. Intuitively, the way we convert from one to the other is to
    // subtract everything from memoryLimit that both contributes to the memory limit (so,
    // ignore scavenged memory) and doesn't contain heap objects. This isn't quite what
    // lines up with reality, but it's a good starting point.
    //
    // In practice this computation looks like the following:
    //
    //    goal := memoryLimit - ((mappedReady - heapFree - heapAlloc) + max(mappedReady - memoryLimit, 0))
    //                    ^1                                    ^2
    //    goal -= goal / 100 * memoryLimitHeapGoalHeadroomPercent
    //    ^3
    //
    // Let's break this down.
    //
    // The first term (marker 1) is everything that contributes to the memory limit and isn't
    // or couldn't become heap objects. It represents, broadly speaking, non-heap overheads.
    // One oddity you may have noticed is that we also subtract out heapFree, i.e. unscavenged
    // memory that may contain heap objects in the future.
    //
    // Let's take a step back. In an ideal world, this term would look something like just
    // the heap goal. That is, we "reserve" enough space for the heap to grow to the heap
    // goal, and subtract out everything else. This is of course impossible; the definition
    // is circular! However, this impossible definition contains a key insight: the amount
    // we're *going* to use matters just as much as whatever we're currently using.
    //
    // Consider if the heap shrinks to 1/10th its size, leaving behind lots of free and
    // unscavenged memory. mappedReady - heapAlloc will be quite large, because of that free
    // and unscavenged memory, pushing the goal down significantly.
    //
    // heapFree is also safe to exclude from the memory limit because in the steady-state, it's
    // just a pool of memory for future heap allocations, and making new allocations from heapFree
    // memory doesn't increase overall memory use. In transient states, the scavenger and the
    // allocator actively manage the pool of heapFree memory to maintain the memory limit.
    //
    // The second term (marker 2) is the amount of memory we've exceeded the limit by, and is
    // intended to help recover from such a situation. By pushing the heap goal down, we also
    // push the trigger down, triggering and finishing a GC sooner in order to make room for
    // other memory sources. Note that since we're effectively reducing the heap goal by X bytes,
    // we're actually giving more than X bytes of headroom back, because the heap goal is in
    // terms of heap objects, but it takes more than X bytes (e.g. due to fragmentation) to store
    // X bytes worth of objects.
    //
    // The final adjustment (marker 3) reduces the maximum possible memory limit heap goal by
    // memoryLimitHeapGoalPercent. As the name implies, this is to provide additional headroom in
    // the face of pacing inaccuracies, and also to leave a buffer of unscavenged memory so the
    // allocator isn't constantly scavenging. The reduction amount also has a fixed minimum
    // (memoryLimitMinHeapGoalHeadroom, not pictured) because the aforementioned pacing inaccuracies
    // disproportionately affect small heaps: as heaps get smaller, the pacer's inputs get fuzzier.
    // Shorter GC cycles and less GC work means noisy external factors like the OS scheduler have a
    // greater impact.
    var memoryLimit = (uint64)Ꮡc.of(gcControllerState.ᏑmemoryLimit).Load();
    // Compute term 1.
    var nonHeapMemory = mappedReady - heapFree - heapAlloc;
    // Compute term 2.
    uint64 overage = default!;
    if (mappedReady > memoryLimit) {
        overage = mappedReady - memoryLimit;
    }
    if (nonHeapMemory + overage >= memoryLimit) {
        // We're at a point where non-heap memory exceeds the memory limit on its own.
        // There's honestly not much we can do here but just trigger GCs continuously
        // and let the CPU limiter reign that in. Something has to give at this point.
        // Set it to heapMarked, the lowest possible goal.
        return c.heapMarked;
    }
    // Compute the goal.
    var goal = memoryLimit - (nonHeapMemory + overage);
    // Apply some headroom to the goal to account for pacing inaccuracies and to reduce
    // the impact of scavenging at allocation time in response to a high allocation rate
    // when GOGC=off. See issue #57069. Also, be careful about small limits.
    var headroom = goal / 100 * (uint64)memoryLimitHeapGoalHeadroomPercent;
    if (headroom < memoryLimitMinHeapGoalHeadroom) {
        // Set a fixed minimum to deal with the particularly large effect pacing inaccuracies
        // have for smaller heaps.
        headroom = memoryLimitMinHeapGoalHeadroom;
    }
    if (goal < headroom || goal - headroom < headroom){
        goal = headroom;
    } else {
        goal = goal - headroom;
    }
    // Don't let us go below the live heap. A heap goal below the live heap doesn't make sense.
    if (goal < c.heapMarked) {
        goal = c.heapMarked;
    }
    return goal;
}

internal static UntypedInt triggerRatioDen => 64;
internal static UntypedInt minTriggerRatioNum => 45; // ~0.7
internal static UntypedInt maxTriggerRatioNum => 61; // ~0.95

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string producedATriggerGreaterˢ = "produced a trigger greater than the heap goal"u8;

// trigger returns the current point at which a GC should trigger along with
// the heap goal.
//
// The returned value may be compared against heapLive to determine whether
// the GC should trigger. Thus, the GC trigger condition should be (but may
// not be, in the case of small movements for efficiency) checked whenever
// the heap goal may change.
internal static (uint64, uint64) trigger(this ж<gcControllerState> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (goal, minTrigger) = Ꮡc.heapGoalInternal();
    // Invariant: the trigger must always be less than the heap goal.
    //
    // Note that the memory limit sets a hard maximum on our heap goal,
    // but the live heap may grow beyond it.
    if (c.heapMarked >= goal) {
        // The goal should never be smaller than heapMarked, but let's be
        // defensive about it. The only reasonable trigger here is one that
        // causes a continuous GC cycle at heapMarked, but respect the goal
        // if it came out as smaller than that.
        return (goal, goal);
    }
    // Below this point, c.heapMarked < goal.
    // heapMarked is our absolute minimum, and it's possible the trigger
    // bound we get from heapGoalinternal is less than that.
    if (minTrigger < c.heapMarked) {
        minTrigger = c.heapMarked;
    }
    // If we let the trigger go too low, then if the application
    // is allocating very rapidly we might end up in a situation
    // where we're allocating black during a nearly always-on GC.
    // The result of this is a growing heap and ultimately an
    // increase in RSS. By capping us at a point >0, we're essentially
    // saying that we're OK using more CPU during the GC to prevent
    // this growth in RSS.
    var triggerLowerBound = ((goal - c.heapMarked) / (uint64)triggerRatioDen) * (uint64)minTriggerRatioNum + c.heapMarked;
    if (minTrigger < triggerLowerBound) {
        minTrigger = triggerLowerBound;
    }
    // For small heaps, set the max trigger point at maxTriggerRatio of the way
    // from the live heap to the heap goal. This ensures we always have *some*
    // headroom when the GC actually starts. For larger heaps, set the max trigger
    // point at the goal, minus the minimum heap size.
    //
    // This choice follows from the fact that the minimum heap size is chosen
    // to reflect the costs of a GC with no work to do. With a large heap but
    // very little scan work to perform, this gives us exactly as much runway
    // as we would need, in the worst case.
    var maxTrigger = ((goal - c.heapMarked) / (uint64)triggerRatioDen) * (uint64)maxTriggerRatioNum + c.heapMarked;
    if (goal > defaultHeapMinimum && goal - (uint64)defaultHeapMinimum > maxTrigger) {
        maxTrigger = goal - (uint64)defaultHeapMinimum;
    }
    maxTrigger = builtin.max(maxTrigger, minTrigger);
    // Compute the trigger from our bounds and the runway stored by commit.
    uint64 trigger = default!;
    var runway = Ꮡc.of(gcControllerState.Ꮡrunway).Load();
    if (runway > goal){
        trigger = minTrigger;
    } else {
        trigger = goal - runway;
    }
    trigger = builtin.max(trigger, minTrigger);
    trigger = min(trigger, maxTrigger);
    if (trigger > goal) {
        print((@string)"trigger="u8, trigger, (@string)" heapGoal="u8, goal, (@string)"\n"u8);
        print((@string)"minTrigger="u8, minTrigger, (@string)" maxTrigger="u8, maxTrigger, (@string)"\n"u8);
        @throw(producedATriggerGreaterˢ);
    }
    return (trigger, goal);
}

// commit recomputes all pacing parameters needed to derive the
// trigger and the heap goal. Namely, the gcPercent-based heap goal,
// and the amount of runway we want to give the GC this cycle.
//
// This can be called any time. If GC is the in the middle of a
// concurrent phase, it will adjust the pacing of that phase.
//
// isSweepDone should be the result of calling isSweepDone(),
// unless we're testing or we know we're executing during a GC cycle.
//
// This depends on gcPercent, gcController.heapMarked, and
// gcController.heapLive. These must be up to date.
//
// Callers must call gcControllerState.revise after calling this
// function if the GC is enabled.
//
// mheap_.lock must be held or the world must be stopped.
internal static void commit(this ж<gcControllerState> Ꮡc, bool isSweepDone) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!c.test) {
        assertWorldStoppedOrLockHeld(ref mheap_.@lock);
    }
    if (isSweepDone){
        // The sweep is done, so there aren't any restrictions on the trigger
        // we need to think about.
        Ꮡc.of(gcControllerState.ᏑsweepDistMinTrigger).Store(0);
    } else {
        // Concurrent sweep happens in the heap growth
        // from gcController.heapLive to trigger. Make sure we
        // give the sweeper some runway if it doesn't have enough.
        Ꮡc.of(gcControllerState.ᏑsweepDistMinTrigger).Store(Ꮡc.of(gcControllerState.ᏑheapLive).Load() + (uint64)sweepMinHeapDistance);
    }
    // Compute the next GC goal, which is when the allocated heap
    // has grown by GOGC/100 over where it started the last cycle,
    // plus additional runway for non-heap sources of GC work.
    var gcPercentHeapGoal = ~(uint64)0;
    {
        var gcPercent = Ꮡc.of(gcControllerState.ᏑgcPercent).Load(); if (gcPercent >= 0) {
            gcPercentHeapGoal = c.heapMarked + (c.heapMarked + Ꮡc.of(gcControllerState.ᏑlastStackScan).Load() + Ꮡc.of(gcControllerState.ᏑglobalsScan).Load()) * (uint64)gcPercent / 100;
        }
    }
    // Apply the minimum heap size here. It's defined in terms of gcPercent
    // and is only updated by functions that call commit.
    if (gcPercentHeapGoal < c.heapMinimum) {
        gcPercentHeapGoal = c.heapMinimum;
    }
    Ꮡc.of(gcControllerState.ᏑgcPercentHeapGoal).Store(gcPercentHeapGoal);
    // Compute the amount of runway we want the GC to have by using our
    // estimate of the cons/mark ratio.
    //
    // The idea is to take our expected scan work, and multiply it by
    // the cons/mark ratio to determine how long it'll take to complete
    // that scan work in terms of bytes allocated. This gives us our GC's
    // runway.
    //
    // However, the cons/mark ratio is a ratio of rates per CPU-second, but
    // here we care about the relative rates for some division of CPU
    // resources among the mutator and the GC.
    //
    // To summarize, we have B / cpu-ns, and we want B / ns. We get that
    // by multiplying by our desired division of CPU resources. We choose
    // to express CPU resources as GOMAPROCS*fraction. Note that because
    // we're working with a ratio here, we can omit the number of CPU cores,
    // because they'll appear in the numerator and denominator and cancel out.
    // As a result, this is basically just "weighing" the cons/mark ratio by
    // our desired division of resources.
    //
    // Furthermore, by setting the runway so that CPU resources are divided
    // this way, assuming that the cons/mark ratio is correct, we make that
    // division a reality.
    Ꮡc.of(gcControllerState.Ꮡrunway).Store((uint64)((c.consMark * /* (1 - gcGoalUtilization) */ 0.75D / /* (gcGoalUtilization) */ 0.25D) * (float64)(c.lastHeapScan + Ꮡc.of(gcControllerState.ᏑlastStackScan).Load() + Ꮡc.of(gcControllerState.ᏑglobalsScan).Load())));
}

// setGCPercent updates gcPercent. commit must be called after.
// Returns the old value of gcPercent.
//
// The world must be stopped, or mheap_.lock must be held.
internal static int32 setGCPercent(this ж<gcControllerState> Ꮡc, int32 @in) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!c.test) {
        assertWorldStoppedOrLockHeld(ref mheap_.@lock);
    }
    var @out = Ꮡc.of(gcControllerState.ᏑgcPercent).Load();
    if (@in < 0) {
        @in = -1;
    }
    c.heapMinimum = (uint64)defaultHeapMinimum * (uint64)@in / 100;
    Ꮡc.of(gcControllerState.ᏑgcPercent).Store(@in);
    return @out;
}

//go:linkname setGCPercent runtime/debug.setGCPercent
internal static int32 /*out*/ setGCPercent(int32 @in) {
    int32 @out = default!;

    // Run on the system stack since we grab the heap lock.
    systemstack(() => {
        @lock(Ꮡmheap_.of(mheap.Ꮡlock));
        @out = ᏑgcController.setGCPercent(@in);
        gcControllerCommit();
        unlock(Ꮡmheap_.of(mheap.Ꮡlock));
    });
    // If we just disabled GC, wait for any concurrent GC mark to
    // finish so we always return with no GC running.
    if (@in < 0) {
        gcWaitOnMark(Ꮡwork.of(workType.Ꮡcycles).Load());
    }
    return @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gogcˢ = "GOGC"u8;

internal static int32 readGOGC() {
    @string Δp = gogetenv(gogcˢ);
    if (Δp == "off"u8) {
        return -1;
    }
    {
        var (n, ok) = atoi32(Δp); if (ok) {
            return n;
        }
    }
    return 100;
}

// setMemoryLimit updates memoryLimit. commit must be called after
// Returns the old value of memoryLimit.
//
// The world must be stopped, or mheap_.lock must be held.
internal static int64 setMemoryLimit(this ж<gcControllerState> Ꮡc, int64 @in) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!c.test) {
        assertWorldStoppedOrLockHeld(ref mheap_.@lock);
    }
    var @out = Ꮡc.of(gcControllerState.ᏑmemoryLimit).Load();
    if (@in >= 0) {
        Ꮡc.of(gcControllerState.ᏑmemoryLimit).Store(@in);
    }
    return @out;
}

//go:linkname setMemoryLimit runtime/debug.setMemoryLimit
internal static int64 /*out*/ setMemoryLimit(int64 @in) {
    int64 @out = default!;

    // Run on the system stack since we grab the heap lock.
    systemstack(() => {
        @lock(Ꮡmheap_.of(mheap.Ꮡlock));
        @out = ᏑgcController.setMemoryLimit(@in);
        if (@in < 0 || @out == @in) {
            // If we're just checking the value or not changing
            // it, there's no point in doing the rest.
            unlock(Ꮡmheap_.of(mheap.Ꮡlock));
            return;
        }
        gcControllerCommit();
        unlock(Ꮡmheap_.of(mheap.Ꮡlock));
    });
    return @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gomemlimitˢ = "GOMEMLIMIT"u8;
internal static readonly @string malformedGomemlimitSeeGoˢ = "malformed GOMEMLIMIT; see `go doc runtime/debug.SetMemoryLimit`"u8;

internal static int64 readGOMEMLIMIT() {
    @string Δp = gogetenv(gomemlimitˢ);
    if (Δp == ""u8 || Δp == "off"u8) {
        return maxInt64;
    }
    var (n, ok) = parseByteCount(Δp);
    if (!ok) {
        print((@string)"GOMEMLIMIT="u8, Δp, (@string)"\n"u8);
        @throw(malformedGomemlimitSeeGoˢ);
    }
    return n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string negativeIdleMarkWorkersˢ = "negative idle mark workers"u8;

// addIdleMarkWorker attempts to add a new idle mark worker.
//
// If this returns true, the caller must become an idle mark worker unless
// there's no background mark worker goroutines in the pool. This case is
// harmless because there are already background mark workers running.
// If this returns false, the caller must NOT become an idle mark worker.
//
// nosplit because it may be called without a P.
//
//go:nosplit
internal static bool addIdleMarkWorker(this ж<gcControllerState> Ꮡc) {
    while (ᐧ) {
        var old = Ꮡc.of(gcControllerState.ᏑidleMarkWorkers).Load();
        var (n, max) = ((int32)((uint64)(old & (uint64)(~(uint32)0))), (int32)((old >> (int)(32))));
        if (n >= max) {
            // See the comment on idleMarkWorkers for why
            // n > max is tolerated.
            return false;
        }
        if (n < 0) {
            print((@string)"n="u8, n, (@string)" max="u8, max, (@string)"\n"u8);
            @throw(negativeIdleMarkWorkersˢ);
        }
        var @new = (uint64)((uint64)(uint32)(n + 1) | (((uint64)max << (int)(32))));
        if (Ꮡc.of(gcControllerState.ᏑidleMarkWorkers).CompareAndSwap(old, @new)) {
            return true;
        }
    }
}

// needIdleMarkWorker is a hint as to whether another idle mark worker is needed.
//
// The caller must still call addIdleMarkWorker to become one. This is mainly
// useful for a quick check before an expensive operation.
//
// nosplit because it may be called without a P.
//
//go:nosplit
internal static bool needIdleMarkWorker(this ж<gcControllerState> Ꮡc) {
    var Δp = Ꮡc.of(gcControllerState.ᏑidleMarkWorkers).Load();
    var (n, max) = ((int32)((uint64)(Δp & (uint64)(~(uint32)0))), (int32)((Δp >> (int)(32))));
    return n < max;
}

// removeIdleMarkWorker must be called when a new idle mark worker stops executing.
internal static void removeIdleMarkWorker(this ж<gcControllerState> Ꮡc) {
    while (ᐧ) {
        var old = Ꮡc.of(gcControllerState.ᏑidleMarkWorkers).Load();
        var (n, max) = ((int32)((uint64)(old & (uint64)(~(uint32)0))), (int32)((old >> (int)(32))));
        if (n - 1 < 0) {
            print((@string)"n="u8, n, (@string)" max="u8, max, (@string)"\n"u8);
            @throw(negativeIdleMarkWorkersˢ);
        }
        var @new = (uint64)((uint64)(uint32)(n - 1) | (((uint64)max << (int)(32))));
        if (Ꮡc.of(gcControllerState.ᏑidleMarkWorkers).CompareAndSwap(old, @new)) {
            return;
        }
    }
}

// setMaxIdleMarkWorkers sets the maximum number of idle mark workers allowed.
//
// This method is optimistic in that it does not wait for the number of
// idle mark workers to reduce to max before returning; it assumes the workers
// will deschedule themselves.
internal static void setMaxIdleMarkWorkers(this ж<gcControllerState> Ꮡc, int32 max) {
    while (ᐧ) {
        var old = Ꮡc.of(gcControllerState.ᏑidleMarkWorkers).Load();
        var n = (int32)((uint64)(old & (uint64)(~(uint32)0)));
        if (n < 0) {
            print((@string)"n="u8, n, (@string)" max="u8, max, (@string)"\n"u8);
            @throw(negativeIdleMarkWorkersˢ);
        }
        var @new = (uint64)((uint64)(uint32)n | (((uint64)max << (int)(32))));
        if (Ꮡc.of(gcControllerState.ᏑidleMarkWorkers).CompareAndSwap(old, @new)) {
            return;
        }
    }
}

// gcControllerCommit is gcController.commit, but passes arguments from live
// (non-test) data. It also updates any consumers of the GC pacing, such as
// sweep pacing and the background scavenger.
//
// Calls gcController.commit.
//
// The heap lock must be held, so this must be executed on the system stack.
//
//go:systemstack
internal static void gcControllerCommit() {
    assertWorldStoppedOrLockHeld(ref mheap_.@lock);
    ᏑgcController.commit(isSweepDone());
    // Update mark pacing.
    if (gcphase != _GCoff) {
        ᏑgcController.revise();
    }
    // TODO(mknyszek): This isn't really accurate any longer because the heap
    // goal is computed dynamically. Still useful to snapshot, but not as useful.
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.HeapGoal();
        traceRelease(Δtrace);
    }
    var (trigger, heapGoal) = ᏑgcController.trigger();
    gcPaceSweeper(trigger);
    gcPaceScavenger(ᏑgcController.of(gcControllerState.ᏑmemoryLimit).Load(), heapGoal, gcController.lastHeapGoal);
}

} // end runtime_package
