// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Scavenging free pages.
//
// This file implements scavenging (the release of physical pages backing mapped
// memory) of free and unused pages in the heap as a way to deal with page-level
// fragmentation and reduce the RSS of Go applications.
//
// Scavenging in Go happens on two fronts: there's the background
// (asynchronous) scavenger and the allocation-time (synchronous) scavenger.
//
// The former happens on a goroutine much like the background sweeper which is
// soft-capped at using scavengePercent of the mutator's time, based on
// order-of-magnitude estimates of the costs of scavenging. The latter happens
// when allocating pages from the heap.
//
// The scavenger's primary goal is to bring the estimated heap RSS of the
// application down to a goal.
//
// Before we consider what this looks like, we need to split the world into two
// halves. One in which a memory limit is not set, and one in which it is.
//
// For the former, the goal is defined as:
//   (retainExtraPercent+100) / 100 * (heapGoal / lastHeapGoal) * lastHeapInUse
//
// Essentially, we wish to have the application's RSS track the heap goal, but
// the heap goal is defined in terms of bytes of objects, rather than pages like
// RSS. As a result, we need to take into account for fragmentation internal to
// spans. heapGoal / lastHeapGoal defines the ratio between the current heap goal
// and the last heap goal, which tells us by how much the heap is growing and
// shrinking. We estimate what the heap will grow to in terms of pages by taking
// this ratio and multiplying it by heapInUse at the end of the last GC, which
// allows us to account for this additional fragmentation. Note that this
// procedure makes the assumption that the degree of fragmentation won't change
// dramatically over the next GC cycle. Overestimating the amount of
// fragmentation simply results in higher memory use, which will be accounted
// for by the next pacing up date. Underestimating the fragmentation however
// could lead to performance degradation. Handling this case is not within the
// scope of the scavenger. Situations where the amount of fragmentation balloons
// over the course of a single GC cycle should be considered pathologies,
// flagged as bugs, and fixed appropriately.
//
// An additional factor of retainExtraPercent is added as a buffer to help ensure
// that there's more unscavenged memory to allocate out of, since each allocation
// out of scavenged memory incurs a potentially expensive page fault.
//
// If a memory limit is set, then we wish to pick a scavenge goal that maintains
// that memory limit. For that, we look at total memory that has been committed
// (memstats.mappedReady) and try to bring that down below the limit. In this case,
// we want to give buffer space in the *opposite* direction. When the application
// is close to the limit, we want to make sure we push harder to keep it under, so
// if we target below the memory limit, we ensure that the background scavenger is
// giving the situation the urgency it deserves.
//
// In this case, the goal is defined as:
//    (100-reduceExtraPercent) / 100 * memoryLimit
//
// We compute both of these goals, and check whether either of them have been met.
// The background scavenger continues operating as long as either one of the goals
// has not been met.
//
// The goals are updated after each GC.
//
// Synchronous scavenging happens for one of two reasons: if an allocation would
// exceed the memory limit or whenever the heap grows in size, for some
// definition of heap-growth. The intuition behind this second reason is that the
// application had to grow the heap because existing fragments were not sufficiently
// large to satisfy a page-level memory allocation, so we scavenge those fragments
// eagerly to offset the growth in RSS that results.
//
// Lastly, not all pages are available for scavenging at all times and in all cases.
// The background scavenger and heap-growth scavenger only release memory in chunks
// that have not been densely-allocated for at least 1 full GC cycle. The reason
// behind this is likelihood of reuse: the Go heap is allocated in a first-fit order
// and by the end of the GC mark phase, the heap tends to be densely packed. Releasing
// memory in these densely packed chunks while they're being packed is counter-productive,
// and worse, it breaks up huge pages on systems that support them. The scavenger (invoked
// during memory allocation) further ensures that chunks it identifies as "dense" are
// immediately eligible for being backed by huge pages. Note that for the most part these
// density heuristics are best-effort heuristics. It's totally possible (but unlikely)
// that a chunk that just became dense is scavenged in the case of a race between memory
// allocation and scavenging.
//
// When synchronously scavenging for the memory limit or for debug.FreeOSMemory, these
// "dense" packing heuristics are ignored (in other words, scavenging is "forced") because
// in these scenarios returning memory to the OS is more important than keeping CPU
// overheads low.
namespace go;

using goos = @internal.goos_package;
using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

internal static UntypedInt scavengePercent => 1; // 1%
internal static UntypedInt retainExtraPercent => 10;
internal static UntypedInt reduceExtraPercent => 5;
internal static UntypedInt maxPagesPerPhysPage => /* maxPhysPageSize / pageSize */ 64;
internal static UntypedFloat scavengeCostRatio => /* 0.7 * (goos.IsDarwin + goos.IsIos) */ 0.7;
internal static UntypedFloat scavChunkHiOccFrac => 0.96875;
internal const uint16 scavChunkHiOccPages = /* uint16(scavChunkHiOccFrac * pallocChunkPages) */ 496;

// heapRetained returns an estimate of the current heap RSS.
internal static uint64 heapRetained() {
    return ᏑgcController.of(gcControllerState.ᏑheapInUse).load() + ᏑgcController.of(gcControllerState.ᏑheapFree).load();
}

// gcPaceScavenger updates the scavenger's pacing, particularly
// its rate and RSS goal. For this, it requires the current heapGoal,
// and the heapGoal for the previous GC cycle.
//
// The RSS goal is based on the current heap goal with a small overhead
// to accommodate non-determinism in the allocator.
//
// The pacing is based on scavengePageRate, which applies to both regular and
// huge pages. See that constant for more information.
//
// Must be called whenever GC pacing is updated.
//
// mheap_.lock must be held or the world must be stopped.
internal static void gcPaceScavenger(int64 memoryLimit, uint64 heapGoal, uint64 lastHeapGoal) {
    assertWorldStoppedOrLockHeld(ref mheap_.@lock);
    // As described at the top of this file, there are two scavenge goals here: one
    // for gcPercent and one for memoryLimit. Let's handle the latter first because
    // it's simpler.
    // We want to target retaining (100-reduceExtraPercent)% of the heap.
    var memoryLimitGoal = (uint64)((float64)memoryLimit * /* (1 - reduceExtraPercent/100.0) */ 0.95D);
    // mappedReady is comparable to memoryLimit, and represents how much total memory
    // the Go runtime has committed now (estimated).
    var mappedReady = ᏑgcController.of(gcControllerState.ᏑmappedReady).Load();
    // If we're below the goal already indicate that we don't need the background
    // scavenger for the memory limit. This may seems worrisome at first, but note
    // that the allocator will assist the background scavenger in the face of a memory
    // limit, so we'll be safe even if we stop the scavenger when we shouldn't have.
    if (mappedReady <= memoryLimitGoal){
        ᏑΔscavenge.of(Δscavengeᴛ1.ᏑmemoryLimitGoal).Store(~(uint64)0);
    } else {
        ᏑΔscavenge.of(Δscavengeᴛ1.ᏑmemoryLimitGoal).Store(memoryLimitGoal);
    }
    // Now handle the gcPercent goal.
    // If we're called before the first GC completed, disable scavenging.
    // We never scavenge before the 2nd GC cycle anyway (we don't have enough
    // information about the heap yet) so this is fine, and avoids a fault
    // or garbage data later.
    if (lastHeapGoal == 0) {
        ᏑΔscavenge.of(Δscavengeᴛ1.ᏑgcPercentGoal).Store(~(uint64)0);
        return;
    }
    // Compute our scavenging goal.
    var goalRatio = (float64)heapGoal / (float64)lastHeapGoal;
    var gcPercentGoal = (uint64)((float64)memstats.lastHeapInUse * goalRatio);
    // Add retainExtraPercent overhead to retainedGoal. This calculation
    // looks strange but the purpose is to arrive at an integer division
    // (e.g. if retainExtraPercent = 12.5, then we get a divisor of 8)
    // that also avoids the overflow from a multiplication.
    gcPercentGoal += gcPercentGoal / (uint64)(/* 1.0 / (retainExtraPercent / 100.0) */ 10UL);
    // Align it to a physical page boundary to make the following calculations
    // a bit more exact.
    gcPercentGoal = (uint64)((gcPercentGoal + (uint64)physPageSize - 1) & ~((uint64)physPageSize - 1));
    // Represents where we are now in the heap's contribution to RSS in bytes.
    //
    // Guaranteed to always be a multiple of physPageSize on systems where
    // physPageSize <= pageSize since we map new heap memory at a size larger than
    // any physPageSize and released memory in multiples of the physPageSize.
    //
    // However, certain functions recategorize heap memory as other stats (e.g.
    // stacks) and this happens in multiples of pageSize, so on systems
    // where physPageSize > pageSize the calculations below will not be exact.
    // Generally this is OK since we'll be off by at most one regular
    // physical page.
    var heapRetainedNow = heapRetained();
    // If we're already below our goal, or within one page of our goal, then indicate
    // that we don't need the background scavenger for maintaining a memory overhead
    // proportional to the heap goal.
    if (heapRetainedNow <= gcPercentGoal || heapRetainedNow - gcPercentGoal < (uint64)physPageSize){
        ᏑΔscavenge.of(Δscavengeᴛ1.ᏑgcPercentGoal).Store(~(uint64)0);
    } else {
        ᏑΔscavenge.of(Δscavengeᴛ1.ᏑgcPercentGoal).Store(gcPercentGoal);
    }
}


partial struct Δscavengeᴛ1 /*dyn*/ {
    // gcPercentGoal is the amount of retained heap memory (measured by
    // heapRetained) that the runtime will try to maintain by returning
    // memory to the OS. This goal is derived from gcController.gcPercent
    // by choosing to retain enough memory to allocate heap memory up to
    // the heap goal.
    internal atomic.Uint64 gcPercentGoal;
    // memoryLimitGoal is the amount of memory retained by the runtime (
    // measured by gcController.mappedReady) that the runtime will try to
    // maintain by returning memory to the OS. This goal is derived from
    // gcController.memoryLimit by choosing to target the memory limit or
    // some lower target to keep the scavenger working.
    internal atomic.Uint64 memoryLimitGoal;
    // assistTime is the time spent by the allocator scavenging in the last GC cycle.
    //
    // This is reset once a GC cycle ends.
    internal atomic.Int64 assistTime;
    // backgroundTime is the time spent by the background scavenger in the last GC cycle.
    //
    // This is reset once a GC cycle ends.
    internal atomic.Int64 backgroundTime;
}
internal static ж<Δscavengeᴛ1> ᏑΔscavenge = new StandardBox<Δscavengeᴛ1>(default(Δscavengeᴛ1));
internal static ref Δscavengeᴛ1 Δscavenge => ref ᏑΔscavenge.Value;

internal static UntypedFloat startingScavSleepRatio => 0.001;
internal static UntypedFloat minScavWorkTime => 1e6;

// Sleep/wait state of the background scavenger.
internal static ж<scavengerState> Ꮡscavenger = new StandardBox<scavengerState>(new scavengerState());
internal static ref scavengerState scavenger => ref Ꮡscavenger.Value;

partial struct scavengerState {
    // lock protects all fields below.
    internal mutex @lock;
    // g is the goroutine the scavenger is bound to.
    internal ж<g> g;
    // timer is the timer used for the scavenger to sleep.
    internal ж<timer> timer;
    // sysmonWake signals to sysmon that it should wake the scavenger.
    internal atomic.Uint32 sysmonWake;
    // parked is whether or not the scavenger is parked.
    internal bool parked;
    // printControllerReset instructs printScavTrace to signal that
    // the controller was reset.
    internal bool printControllerReset;
    // targetCPUFraction is the target CPU overhead for the scavenger.
    internal float64 targetCPUFraction;
    // sleepRatio is the ratio of time spent doing scavenging work to
    // time spent sleeping. This is used to decide how long the scavenger
    // should sleep for in between batches of work. It is set by
    // critSleepController in order to maintain a CPU overhead of
    // targetCPUFraction.
    //
    // Lower means more sleep, higher means more aggressive scavenging.
    internal float64 sleepRatio;
    // sleepController controls sleepRatio.
    //
    // See sleepRatio for more details.
    internal piController sleepController;
    // controllerCooldown is the time left in nanoseconds during which we avoid
    // using the controller and we hold sleepRatio at a conservative
    // value. Used if the controller's assumptions fail to hold.
    internal int64 controllerCooldown;
    // sleepStub is a stub used for testing to avoid actually having
    // the scavenger sleep.
    //
    // Unlike the other stubs, this is not populated if left nil
    // Instead, it is called when non-nil because any valid implementation
    // of this function basically requires closing over this scavenger
    // state, and allocating a closure is not allowed in the runtime as
    // a matter of policy.
    internal Func<int64, int64> sleepStub;
    // scavenge is a function that scavenges n bytes of memory.
    // Returns how many bytes of memory it actually scavenged, as
    // well as the time it took in nanoseconds. Usually mheap.pages.scavenge
    // with nanotime called around it, but stubbed out for testing.
    // Like mheap.pages.scavenge, if it scavenges less than n bytes of
    // memory, the caller may assume the heap is exhausted of scavengable
    // memory for now.
    //
    // If this is nil, it is populated with the real thing in init.
    internal Func<uintptr, (uintptr, int64)> scavenge;
    // shouldStop is a callback called in the work loop and provides a
    // point that can force the scavenger to stop early, for example because
    // the scavenge policy dictates too much has been scavenged already.
    //
    // If this is nil, it is populated with the real thing in init.
    internal Func<bool> shouldStop;
    // gomaxprocs returns the current value of gomaxprocs. Stub for testing.
    //
    // If this is nil, it is populated with the real thing in init.
    internal Func<int32> gomaxprocs;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string scavengerStateIsAlreadyˢ = "scavenger state is already wired"u8;

// init initializes a scavenger state and wires to the current G.
//
// Must be called from a regular goroutine that can allocate.
internal static void init(this ж<scavengerState> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (s.g != nil) {
        @throw(scavengerStateIsAlreadyˢ);
    }
    lockInit(Ꮡs.of(scavengerState.Ꮡlock), lockRankScavenge);
    s.g = getg();
    s.timer = @new<timer>();
    var f = (any sΔ1, uintptr _Δp1, int64 _Δp2) => {
        sΔ1._<ж<scavengerState>>().wake();
    };
    s.timer.init(f, Ꮡs.OrTypedNil());
    // input: fraction of CPU time actually used.
    // setpoint: ideal CPU fraction.
    // output: ratio of time worked to time slept (determines sleep time).
    //
    // The output of this controller is somewhat indirect to what we actually
    // want to achieve: how much time to sleep for. The reason for this definition
    // is to ensure that the controller's outputs have a direct relationship with
    // its inputs (as opposed to an inverse relationship), making it somewhat
    // easier to reason about for tuning purposes.
    s.sleepController = new piController( // Tuned loosely via Ziegler-Nichols process.

        kp: 0.3375D,
        ti: 3.2e6D,
        tt: 1e9D, // 1 second reset time.
 // These ranges seem wide, but we want to give the controller plenty of
 // room to hunt for the optimal value.

        min: 0.001D, // 1:1000

        max: 1000.0D
    );
    // 1000:1
    s.sleepRatio = startingScavSleepRatio;
    // Install real functions if stubs aren't present.
    if (s.scavenge == default!) {
        s.scavenge = (uintptr n) => {
            var start = nanotime();
            var r = Ꮡmheap_.of(mheap.Ꮡpages).scavenge(n, default!, false);
            var end = nanotime();
            if (start >= end) {
                return (r, 0);
            }
            ᏑΔscavenge.of(runtime_package.Δscavengeᴛ1.ᏑbackgroundTime).Add(end - start);
            return (r, end - start);
        };
    }
    if (s.shouldStop == default!) {
        s.shouldStop = () => {
            // If background scavenging is disabled or if there's no work to do just stop.
            return heapRetained() <= ᏑΔscavenge.of(runtime_package.Δscavengeᴛ1.ᏑgcPercentGoal).Load() && ᏑgcController.of(gcControllerState.ᏑmappedReady).Load() <= ᏑΔscavenge.of(runtime_package.Δscavengeᴛ1.ᏑmemoryLimitGoal).Load();
        };
    }
    if (s.gomaxprocs == default!) {
        s.gomaxprocs = () => gomaxprocs;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string triedToParkScavengerFromˢ = "tried to park scavenger from another goroutine"u8;

// park parks the scavenger goroutine.
internal static void park(this ж<scavengerState> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    @lock(Ꮡs.of(scavengerState.Ꮡlock));
    if (getg() != s.g) {
        @throw(triedToParkScavengerFromˢ);
    }
    s.parked = true;
    goparkunlock(Ꮡs.of(scavengerState.Ꮡlock), waitReasonGCScavengeWait, traceBlockSystemGoroutine, 2);
}

// ready signals to sysmon that the scavenger should be awoken.
internal static void ready(this ж<scavengerState> Ꮡs) {
    Ꮡs.of(scavengerState.ᏑsysmonWake).Store(1);
}

// wake immediately unparks the scavenger if necessary.
//
// Safe to run without a P.
internal static void wake(this ж<scavengerState> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    @lock(Ꮡs.of(scavengerState.Ꮡlock));
    if (s.parked) {
        // Unset sysmonWake, since the scavenger is now being awoken.
        Ꮡs.of(scavengerState.ᏑsysmonWake).Store(0);
        // s.parked is unset to prevent a double wake-up.
        s.parked = false;
        // Ready the goroutine by injecting it. We use injectglist instead
        // of ready or goready in order to allow us to run this function
        // without a P. injectglist also avoids placing the goroutine in
        // the current P's runnext slot, which is desirable to prevent
        // the scavenger from interfering with user goroutine scheduling
        // too much.
        ref var list = ref heap(new gList(), out var Ꮡlist);
        list.push(s.g);
        injectglist(Ꮡlist);
    }
    unlock(Ꮡs.of(scavengerState.Ꮡlock));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string triedToSleepScavengerˢ = "tried to sleep scavenger from another goroutine"u8;

// sleep puts the scavenger to sleep based on the amount of time that it worked
// in nanoseconds.
//
// Note that this function should only be called by the scavenger.
//
// The scavenger may be woken up earlier by a pacing change, and it may not go
// to sleep at all if there's a pending pacing change.
internal static void sleep(this ж<scavengerState> Ꮡs, float64 worked) {
    ref var s = ref Ꮡs.DerefOrNull();

    @lock(Ꮡs.of(scavengerState.Ꮡlock));
    if (getg() != s.g) {
        @throw(triedToSleepScavengerˢ);
    }
    if (worked < minScavWorkTime) {
        // This means there wasn't enough work to actually fill up minScavWorkTime.
        // That's fine; we shouldn't try to do anything with this information
        // because it's going result in a short enough sleep request that things
        // will get messy. Just assume we did at least this much work.
        // All this means is that we'll sleep longer than we otherwise would have.
        worked = minScavWorkTime;
    }
    // Multiply the critical time by 1 + the ratio of the costs of using
    // scavenged memory vs. scavenging memory. This forces us to pay down
    // the cost of reusing this memory eagerly by sleeping for a longer period
    // of time and scavenging less frequently. More concretely, we avoid situations
    // where we end up scavenging so often that we hurt allocation performance
    // because of the additional overheads of using scavenged memory.
    worked *= 1D + scavengeCostRatio;
    // sleepTime is the amount of time we're going to sleep, based on the amount
    // of time we worked, and the sleepRatio.
    var sleepTime = (int64)(worked / s.sleepRatio);
    int64 slept = default!;
    if (s.sleepStub == default!){
        // Set the timer.
        //
        // This must happen here instead of inside gopark
        // because we can't close over any variables without
        // failing escape analysis.
        var start = nanotime();
        s.timer.reset(start + sleepTime, 0);
        // Mark ourselves as asleep and go to sleep.
        s.parked = true;
        goparkunlock(Ꮡs.of(scavengerState.Ꮡlock), waitReasonSleep, traceBlockSleep, 2);
        // How long we actually slept for.
        slept = nanotime() - start;
        @lock(Ꮡs.of(scavengerState.Ꮡlock));
        // Stop the timer here because s.wake is unable to do it for us.
        // We don't really care if we succeed in stopping the timer. One
        // reason we might fail is that we've already woken up, but the timer
        // might be in the process of firing on some other P; essentially we're
        // racing with it. That's totally OK. Double wake-ups are perfectly safe.
        s.timer.stop();
        unlock(Ꮡs.of(scavengerState.Ꮡlock));
    } else {
        unlock(Ꮡs.of(scavengerState.Ꮡlock));
        slept = s.sleepStub(sleepTime);
    }
    // Stop here if we're cooling down from the controller.
    if (s.controllerCooldown > 0) {
        // worked and slept aren't exact measures of time, but it's OK to be a bit
        // sloppy here. We're just hoping we're avoiding some transient bad behavior.
        var t = slept + (int64)worked;
        if (t > s.controllerCooldown){
            s.controllerCooldown = 0;
        } else {
            s.controllerCooldown -= t;
        }
        return;
    }
    // idealFraction is the ideal % of overall application CPU time that we
    // spend scavenging.
    var idealFraction = (float64)scavengePercent / 100.0D;
    // Calculate the CPU time spent.
    //
    // This may be slightly inaccurate with respect to GOMAXPROCS, but we're
    // recomputing this often enough relative to GOMAXPROCS changes in general
    // (it only changes when the world is stopped, and not during a GC) that
    // that small inaccuracy is in the noise.
    var cpuFraction = worked / (((float64)slept + worked) * (float64)s.gomaxprocs());
    // Update the critSleepRatio, adjusting until we reach our ideal fraction.
    bool ok = default!;
    (s.sleepRatio, ok) = s.sleepController.next(cpuFraction, idealFraction, (float64)slept + worked);
    if (!ok) {
        // The core assumption of the controller, that we can get a proportional
        // response, broke down. This may be transient, so temporarily switch to
        // sleeping a fixed, conservative amount.
        s.sleepRatio = startingScavSleepRatio;
        s.controllerCooldown = 5000000000; // 5 seconds.
        // Signal the scav trace printer to output this.
        Ꮡs.controllerFailed();
    }
}

// controllerFailed indicates that the scavenger's scheduling
// controller failed.
internal static void controllerFailed(this ж<scavengerState> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    @lock(Ꮡs.of(scavengerState.Ꮡlock));
    s.printControllerReset = true;
    unlock(Ꮡs.of(scavengerState.Ꮡlock));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string triedToRunScavengerFromˢ = "tried to run scavenger from another goroutine"u8;
internal static readonly @string releasedLessThanOneˢ = "released less than one physical page of memory"u8;

// run is the body of the main scavenging loop.
//
// Returns the number of bytes released and the estimated time spent
// releasing those bytes.
//
// Must be run on the scavenger goroutine.
internal static (uintptr released, float64 worked) run(this ж<scavengerState> Ꮡs) {
    uintptr released = default!;
    float64 worked = default!;

    ref var s = ref Ꮡs.DerefOrNull();
    @lock(Ꮡs.of(scavengerState.Ꮡlock));
    if (getg() != s.g) {
        @throw(triedToRunScavengerFromˢ);
    }
    unlock(Ꮡs.of(scavengerState.Ꮡlock));
    while (worked < minScavWorkTime) {
        // If something from outside tells us to stop early, stop.
        if (s.shouldStop()) {
            break;
        }
        // scavengeQuantum is the amount of memory we try to scavenge
        // in one go. A smaller value means the scavenger is more responsive
        // to the scheduler in case of e.g. preemption. A larger value means
        // that the overheads of scavenging are better amortized, so better
        // scavenging throughput.
        //
        // The current value is chosen assuming a cost of ~10µs/physical page
        // (this is somewhat pessimistic), which implies a worst-case latency of
        // about 160µs for 4 KiB physical pages. The current value is biased
        // toward latency over throughput.
        uintptr scavengeQuantum = /* 64 << 10 */ 65536;
        // Accumulate the amount of time spent scavenging.
        var (r, duration) = s.scavenge(scavengeQuantum);
        // On some platforms we may see end >= start if the time it takes to scavenge
        // memory is less than the minimum granularity of its clock (e.g. Windows) or
        // due to clock bugs.
        //
        // In this case, just assume scavenging takes 10 µs per regular physical page
        // (determined empirically), and conservatively ignore the impact of huge pages
        // on timing.
        const float64 approxWorkedNSPerPhysicalPage = 10e3;
        if (duration == 0){
            worked += approxWorkedNSPerPhysicalPage * (float64)(r / physPageSize);
        } else {
            // TODO(mknyszek): If duration is small compared to worked, it could be
            // rounded down to zero. Probably not a problem in practice because the
            // values are all within a few orders of magnitude of each other but maybe
            // worth worrying about.
            worked += (float64)duration;
        }
        released += r;
        // scavenge does not return until it either finds the requisite amount of
        // memory to scavenge, or exhausts the heap. If we haven't found enough
        // to scavenge, then the heap must be exhausted.
        if (r < scavengeQuantum) {
            break;
        }
        // When using fake time just do one loop.
        if (faketime != 0) {
            break;
        }
    }
    if (released > 0 && released < physPageSize) {
        // If this happens, it means that we may have attempted to release part
        // of a physical page, but the likely effect of that is that it released
        // the whole physical page, some of which may have still been in-use.
        // This could lead to memory corruption. Throw.
        @throw(releasedLessThanOneˢ);
    }
    return (released, worked);
}

// Background scavenger.
//
// The background scavenger maintains the RSS of the application below
// the line described by the proportional scavenging statistics in
// the mheap struct.
internal static void bgscavenge(channel<nint> c) {
    Ꮡscavenger.init();
    c.ᐸꟷ(1);
    Ꮡscavenger.park();
    while (ᐧ) {
        var (released, workTime) = Ꮡscavenger.run();
        if (released == 0) {
            Ꮡscavenger.park();
            continue;
        }
        Ꮡmheap_.of(mheap.Ꮡpages).of(pageAlloc.Ꮡscav).of(pageAlloc_scav.ᏑreleasedBg).Add(released);
        Ꮡscavenger.sleep(workTime);
    }
}

// scavenge scavenges nbytes worth of free pages, starting with the
// highest address first. Successive calls continue from where it left
// off until the heap is exhausted. force makes all memory available to
// scavenge, ignoring huge page heuristics.
//
// Returns the amount of memory scavenged in bytes.
//
// scavenge always tries to scavenge nbytes worth of memory, and will
// only fail to do so if the heap is exhausted for now.
internal static uintptr scavenge(this ж<pageAlloc> Ꮡp, uintptr nbytes, Func<bool> shouldStop, bool force) {
    var released = (uintptr)0;
    while (released < nbytes) {
        var (ci, pageIdx) = Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).find(force);
        if (ci == 0) {
            break;
        }
        systemstack(() => {
            released += Ꮡp.Value.scavengeOne(ci, pageIdx, nbytes - released);
        });
        if (shouldStop != default! && shouldStop()) {
            break;
        }
    }
    return released;
}

// printScavTrace prints a scavenge trace line to standard error.
//
// released should be the amount of memory released since the last time this
// was called, and forced indicates whether the scavenge was forced by the
// application.
//
// scavenger.lock must be held.
internal static void printScavTrace(uintptr releasedBg, uintptr releasedEager, bool forced) {
    assertLockHeld(Ꮡscavenger.of(scavengerState.Ꮡlock));
    printlock();
    print((@string)"scav "u8,
        (releasedBg >> (int)(10)), (@string)" KiB work (bg), "u8,
        (releasedEager >> (int)(10)), (@string)" KiB work (eager), "u8,
        (ᏑgcController.of(gcControllerState.ᏑheapReleased).load() >> (int)(10)), (@string)" KiB now, "u8,
        (ᏑgcController.of(gcControllerState.ᏑheapInUse).load() * 100) / heapRetained(), (@string)"% util"u8);
    if (forced){
        print((@string)" (forced)"u8);
    } else 
    if (scavenger.printControllerReset) {
        print((@string)" [controller reset]"u8);
        scavenger.printControllerReset = false;
    }
    println();
    printunlock();
}

// scavengeOne walks over the chunk at chunk index ci and searches for
// a contiguous run of pages to scavenge. It will try to scavenge
// at most max bytes at once, but may scavenge more to avoid
// breaking huge pages. Once it scavenges some memory it returns
// how much it scavenged in bytes.
//
// searchIdx is the page index to start searching from in ci.
//
// Returns the number of bytes scavenged.
//
// Must run on the systemstack because it acquires p.mheapLock.
//
//go:systemstack
internal static uintptr scavengeOne(this ref pageAlloc Δp, chunkIdx ci, nuint searchIdx, uintptr max) {
    // Calculate the maximum number of pages to scavenge.
    //
    // This should be alignUp(max, pageSize) / pageSize but max can and will
    // be ^uintptr(0), so we need to be very careful not to overflow here.
    // Rather than use alignUp, calculate the number of pages rounded down
    // first, then add back one if necessary.
    var maxPages = max / (uintptr)pageSize;
    if (max % (uintptr)pageSize != 0) {
        maxPages++;
    }
    // Calculate the minimum number of pages we can scavenge.
    //
    // Because we can only scavenge whole physical pages, we must
    // ensure that we scavenge at least minPages each time, aligned
    // to minPages*pageSize.
    var minPages = physPageSize / (uintptr)pageSize;
    if (minPages < 1) {
        minPages = 1;
    }
    @lock(Δp.mheapLock);
    if (Δp.summary[len(Δp.summary) - 1][ci].max() >= (nuint)minPages) {
        // We only bother looking for a candidate if there at least
        // minPages free pages at all.
        var (@base, npages) = Δp.chunkOf(ci).findScavengeCandidate(searchIdx, minPages, maxPages);
        // If we found something, scavenge it and return!
        if (npages != 0) {
            // Compute the full address for the start of the range.
            var addr = chunkBase(ci) + (uintptr)@base * (uintptr)pageSize;
            // Mark the range we're about to scavenge as allocated, because
            // we don't want any allocating goroutines to grab it while
            // the scavenging is in progress. Be careful here -- just do the
            // bare minimum to avoid stepping on our own scavenging stats.
            Δp.chunkOf(ci).allocRange(@base, npages);
            Δp.update(addr, (uintptr)npages, true, true);
            // With that done, it's safe to unlock.
            unlock(Δp.mheapLock);
            if (!Δp.test) {
                // Only perform sys* operations if we're not in a test.
                // It's dangerous to do so otherwise.
                sysUnused((@unsafe.Pointer)addr, (uintptr)npages * (uintptr)pageSize);
                // Update global accounting only when not in test, otherwise
                // the runtime's accounting will be wrong.
                var nbytes = (int64)(npages * (nuint)pageSize);
                ᏑgcController.of(gcControllerState.ᏑheapReleased).add(nbytes);
                ᏑgcController.of(gcControllerState.ᏑheapFree).add(-nbytes);
                var stats = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
                atomic.Xaddint64(stats.of(heapStatsDelta.Ꮡcommitted), -nbytes);
                atomic.Xaddint64(stats.of(heapStatsDelta.Ꮡreleased), nbytes);
                Ꮡmemstats.of(mstats.ᏑheapStats).release();
            }
            // Relock the heap, because now we need to make these pages
            // available allocation. Free them back to the page allocator.
            @lock(Δp.mheapLock);
            {
                var b = (new offAddr(addr)); if (b.lessThan(Δp.searchAddr)) {
                    Δp.searchAddr = b;
                }
            }
            Δp.chunkOf(ci).of(pallocData.ᏑpallocBits).free(@base, npages);
            Δp.update(addr, (uintptr)npages, true, false);
            // Mark the range as scavenged.
            Δp.chunkOf(ci).of(pallocData.Ꮡscavenged).setRange(@base, npages);
            unlock(Δp.mheapLock);
            return (uintptr)npages * (uintptr)pageSize;
        }
    }
    // Mark this chunk as having no free pages.
    Δp.scav.index.setEmpty(ci);
    unlock(Δp.mheapLock);
    return 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badMValueˢ = "bad m value"u8;

// fillAligned returns x but with all zeroes in m-aligned
// groups of m bits set to 1 if any bit in the group is non-zero.
//
// For example, fillAligned(0x0100a3, 8) == 0xff00ff.
//
// Note that if m == 1, this is a no-op.
//
// m must be a power of 2 <= maxPagesPerPhysPage.
internal static uint64 fillAligned(uint64 x, nuint m) {
    uint64 apply(uint64 xΔ1, uint64 c) {
        // The technique used it here is derived from
        // https://graphics.stanford.edu/~seander/bithacks.html#ZeroInWord
        // and extended for more than just bytes (like nibbles
        // and uint16s) by using an appropriate constant.
        //
        // To summarize the technique, quoting from that page:
        // "[It] works by first zeroing the high bits of the [8]
        // bytes in the word. Subsequently, it adds a number that
        // will result in an overflow to the high bit of a byte if
        // any of the low bits were initially set. Next the high
        // bits of the original word are ORed with these values;
        // thus, the high bit of a byte is set iff any bit in the
        // byte was set. Finally, we determine if any of these high
        // bits are zero by ORing with ones everywhere except the
        // high bits and inverting the result."
        return ~((uint64)(((uint64)((((uint64)(xΔ1 & c)) + c) | xΔ1)) | c));
    }
    // Transform x to contain a 1 bit at the top of each m-aligned
    // group of m zero bits.
    switch (m) {
    case 1: {
        return x;
    }
    case 2: {
        x = apply(x, 0x5555555555555555UL);
        break;
    }
    case 4: {
        x = apply(x, 0x7777777777777777UL);
        break;
    }
    case 8: {
        x = apply(x, 0x7f7f7f7f7f7f7f7fUL);
        break;
    }
    case 16: {
        x = apply(x, 0x7fff7fff7fff7fffUL);
        break;
    }
    case 32: {
        x = apply(x, 0x7fffffff7fffffffUL);
        break;
    }
    case 64: {
        x = apply(x, // == maxPagesPerPhysPage
 0x7fffffffffffffffUL);
        break;
    }
    default: {
        @throw(badMValueˢ);
        break;
    }}

    // Now, the top bit of each m-aligned group in x is set
    // that group was all zero in the original x.
    // From each group of m bits subtract 1.
    // Because we know only the top bits of each
    // m-aligned group are set, we know this will
    // set each group to have all the bits set except
    // the top bit, so just OR with the original
    // result to set all the bits.
    return ~((uint64)((x - (x.Rsh((m - 1)))) | x));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string minMustBeANonZeroPowerOfˢ = "min must be a non-zero power of 2"u8;
internal static readonly @string minTooLargeˢ = "min too large"u8;

// findScavengeCandidate returns a start index and a size for this pallocData
// segment which represents a contiguous region of free and unscavenged memory.
//
// searchIdx indicates the page index within this chunk to start the search, but
// note that findScavengeCandidate searches backwards through the pallocData. As
// a result, it will return the highest scavenge candidate in address order.
//
// min indicates a hard minimum size and alignment for runs of pages. That is,
// findScavengeCandidate will not return a region smaller than min pages in size,
// or that is min pages or greater in size but not aligned to min. min must be
// a non-zero power of 2 <= maxPagesPerPhysPage.
//
// max is a hint for how big of a region is desired. If max >= pallocChunkPages, then
// findScavengeCandidate effectively returns entire free and unscavenged regions.
// If max < pallocChunkPages, it may truncate the returned region such that size is
// max. However, findScavengeCandidate may still return a larger region if, for
// example, it chooses to preserve huge pages, or if max is not aligned to min (it
// will round up). That is, even if max is small, the returned size is not guaranteed
// to be equal to max. max is allowed to be less than min, in which case it is as if
// max == min.
internal static (nuint, nuint) findScavengeCandidate(this ref pallocData m, nuint searchIdx, uintptr minimum, uintptr max) {
    if ((uintptr)(minimum & (minimum - 1)) != 0 || minimum == 0){
        print((@string)"runtime: min = "u8, minimum, (@string)"\n"u8);
        @throw(minMustBeANonZeroPowerOfˢ);
    } else 
    if (minimum > maxPagesPerPhysPage) {
        print((@string)"runtime: min = "u8, minimum, (@string)"\n"u8);
        @throw(minTooLargeˢ);
    }
    // max may not be min-aligned, so we might accidentally truncate to
    // a max value which causes us to return a non-min-aligned value.
    // To prevent this, align max up to a multiple of min (which is always
    // a power of 2). This also prevents max from ever being less than
    // min, unless it's zero, so handle that explicitly.
    if (max == 0){
        max = minimum;
    } else {
        max = alignUp(max, minimum);
    }
    nint i = (nint)(searchIdx / 64);
    // Start by quickly skipping over blocks of non-free or scavenged pages.
    for (; i >= 0; i--) {
        // 1s are scavenged OR non-free => 0s are unscavenged AND free
        var xΔ1 = fillAligned((uint64)(m.scavenged[i] | m.pallocBits[i]), (nuint)minimum);
        if (xΔ1 != ~(uint64)0) {
            break;
        }
    }
    if (i < 0) {
        // Failed to find any free/unscavenged pages.
        return (0, 0);
    }
    // We have something in the 64-bit chunk at i, but it could
    // extend further. Loop until we find the extent of it.
    // 1s are scavenged OR non-free => 0s are unscavenged AND free
    var x = fillAligned((uint64)(m.scavenged[i] | m.pallocBits[i]), (nuint)minimum);
    nuint z1 = (nuint)sys.LeadingZeros64(~x);
    nuint run = (nuint)0;
    nuint end = (nuint)i * 64 + (64 - z1);
    if (x.Lsh(z1) != 0){
        // After shifting out z1 bits, we still have 1s,
        // so the run ends inside this word.
        run = (nuint)sys.LeadingZeros64(x.Lsh(z1));
    } else {
        // After shifting out z1 bits, we have no more 1s.
        // This means the run extends to the bottom of the
        // word so it may extend into further words.
        run = 64 - z1;
        for (nint j = i - 1; j >= 0; j--) {
            var xΔ2 = fillAligned((uint64)(m.scavenged[j] | m.pallocBits[j]), (nuint)minimum);
            run += (nuint)sys.LeadingZeros64(xΔ2);
            if (xΔ2 != 0) {
                // The run stopped in this word.
                break;
            }
        }
    }
    // Split the run we found if it's larger than max but hold on to
    // our original length, since we may need it later.
    nuint size = min(run, (nuint)max);
    nuint start = end - size;
    // Each huge page is guaranteed to fit in a single palloc chunk.
    //
    // TODO(mknyszek): Support larger huge page sizes.
    // TODO(mknyszek): Consider taking pages-per-huge-page as a parameter
    // so we can write tests for this.
    if (physHugePageSize > pageSize && physHugePageSize > physPageSize) {
        // We have huge pages, so let's ensure we don't break one by scavenging
        // over a huge page boundary. If the range [start, start+size) overlaps with
        // a free-and-unscavenged huge page, we want to grow the region we scavenge
        // to include that huge page.
        // Compute the huge page boundary above our candidate.
        var pagesPerHugePage = physHugePageSize / (uintptr)pageSize;
        nuint hugePageAbove = (nuint)alignUp((uintptr)start, pagesPerHugePage);
        // If that boundary is within our current candidate, then we may be breaking
        // a huge page.
        if (hugePageAbove <= end) {
            // Compute the huge page boundary below our candidate.
            nuint hugePageBelow = (nuint)alignDown((uintptr)start, pagesPerHugePage);
            if (hugePageBelow >= end - run) {
                // We're in danger of breaking apart a huge page since start+size crosses
                // a huge page boundary and rounding down start to the nearest huge
                // page boundary is included in the full run we found. Include the entire
                // huge page in the bound by rounding down to the huge page size.
                size = size + (start - hugePageBelow);
                start = hugePageBelow;
            }
        }
    }
    return (start, size);
}

// scavengeIndex is a structure for efficiently managing which pageAlloc chunks have
// memory available to scavenge.
partial struct scavengeIndex {
    // chunks is a scavChunkData-per-chunk structure that indicates the presence of pages
    // available for scavenging. Updates to the index are serialized by the pageAlloc lock.
    //
    // It tracks chunk occupancy and a generation counter per chunk. If a chunk's occupancy
    // never exceeds pallocChunkDensePages over the course of a single GC cycle, the chunk
    // becomes eligible for scavenging on the next cycle. If a chunk ever hits this density
    // threshold it immediately becomes unavailable for scavenging in the current cycle as
    // well as the next.
    //
    // [min, max) represents the range of chunks that is safe to access (i.e. will not cause
    // a fault). As an optimization minHeapIdx represents the true minimum chunk that has been
    // mapped, since min is likely rounded down to include the system page containing minHeapIdx.
    //
    // For a chunk size of 4 MiB this structure will only use 2 MiB for a 1 TiB contiguous heap.
    internal slice<atomicScavChunkData> chunks;
    internal atomic.Uintptr min, max;
    internal atomic.Uintptr minHeapIdx;
    // searchAddr* is the maximum address (in the offset address space, so we have a linear
    // view of the address space; see mranges.go:offAddr) containing memory available to
    // scavenge. It is a hint to the find operation to avoid O(n^2) behavior in repeated lookups.
    //
    // searchAddr* is always inclusive and should be the base address of the highest runtime
    // page available for scavenging.
    //
    // searchAddrForce is managed by find and free.
    // searchAddrBg is managed by find and nextGen.
    //
    // Normally, find monotonically decreases searchAddr* as it finds no more free pages to
    // scavenge. However, mark, when marking a new chunk at an index greater than the current
    // searchAddr, sets searchAddr to the *negative* index into chunks of that page. The trick here
    // is that concurrent calls to find will fail to monotonically decrease searchAddr*, and so they
    // won't barge over new memory becoming available to scavenge. Furthermore, this ensures
    // that some future caller of find *must* observe the new high index. That caller
    // (or any other racing with it), then makes searchAddr positive before continuing, bringing
    // us back to our monotonically decreasing steady-state.
    //
    // A pageAlloc lock serializes updates between min, max, and searchAddr, so abs(searchAddr)
    // is always guaranteed to be >= min and < max (converted to heap addresses).
    //
    // searchAddrBg is increased only on each new generation and is mainly used by the
    // background scavenger and heap-growth scavenging. searchAddrForce is increased continuously
    // as memory gets freed and is mainly used by eager memory reclaim such as debug.FreeOSMemory
    // and scavenging to maintain the memory limit.
    internal atomicOffAddr searchAddrBg;
    internal atomicOffAddr searchAddrForce;
    // freeHWM is the highest address (in offset address space) that was freed
    // this generation.
    internal offAddr freeHWM;
    // Generation counter. Updated by nextGen at the end of each mark phase.
    internal uint32 gen;
    // test indicates whether or not we're in a test.
    internal bool test;
}

// init initializes the scavengeIndex.
//
// Returns the amount added to sysStat.
internal static uintptr init(this ж<scavengeIndex> Ꮡs, bool test, ж<sysMemStat> ᏑsysStat) {
    ref var s = ref Ꮡs.DerefOrNull();

    Ꮡs.of(scavengeIndex.ᏑsearchAddrBg).Clear();
    Ꮡs.of(scavengeIndex.ᏑsearchAddrForce).Clear();
    s.freeHWM = minOffAddr;
    s.test = test;
    return s.sysInit(test, ᏑsysStat);
}

// sysGrow updates the index's backing store in response to a heap growth.
//
// Returns the amount of memory added to sysStat.
internal static uintptr grow(this ж<scavengeIndex> Ꮡs, uintptr @base, uintptr limit, ж<sysMemStat> ᏑsysStat) {
    ref var s = ref Ꮡs.DerefOrNull();

    // Update minHeapIdx. Note that even if there's no mapping work to do,
    // we may still have a new, lower minimum heap address.
    var minHeapIdx = Ꮡs.of(scavengeIndex.ᏑminHeapIdx).Load();
    {
        var baseIdx = (uintptr)(nuint)chunkIndex(@base); if (minHeapIdx == 0 || baseIdx < minHeapIdx) {
            Ꮡs.of(scavengeIndex.ᏑminHeapIdx).Store(baseIdx);
        }
    }
    return Ꮡs.sysGrow(@base, limit, ᏑsysStat);
}

// find returns the highest chunk index that may contain pages available to scavenge.
// It also returns an offset to start searching in the highest chunk.
internal static (chunkIdx, nuint) find(this ж<scavengeIndex> Ꮡs, bool force) {
    ref var s = ref Ꮡs.DerefOrNull();

    var cursor = Ꮡs.of(scavengeIndex.ᏑsearchAddrBg);
    if (force) {
        cursor = Ꮡs.of(scavengeIndex.ᏑsearchAddrForce);
    }
    var (searchAddr, marked) = cursor.Load();
    if (searchAddr == minOffAddr.addr()) {
        // We got a cleared search addr.
        return (0, 0);
    }
    // Starting from searchAddr's chunk, iterate until we find a chunk with pages to scavenge.
    var gen = s.gen;
    chunkIdx min = ((chunkIdx)(nuint)Ꮡs.of(scavengeIndex.ᏑminHeapIdx).Load());
    chunkIdx start = chunkIndex(searchAddr);
    // N.B. We'll never map the 0'th chunk, so minHeapIdx ensures this loop overflow.
    for (chunkIdx i = start; i >= min; i--) {
        // Skip over chunks.
        if (!Ꮡ(s.chunks, i).load().shouldScavenge(gen, force)) {
            continue;
        }
        // We're still scavenging this chunk.
        if (i == start) {
            return (i, chunkPageIndex(searchAddr));
        }
        // Try to reduce searchAddr to newSearchAddr.
        var newSearchAddr = chunkBase(i) + (uintptr)pallocChunkBytes - (uintptr)pageSize;
        if (marked){
            // Attempt to be the first one to decrease the searchAddr
            // after an increase. If we fail, that means there was another
            // increase, or somebody else got to it before us. Either way,
            // it doesn't matter. We may lose some performance having an
            // incorrect search address, but it's far more important that
            // we don't miss updates.
            cursor.StoreUnmark(searchAddr, newSearchAddr);
        } else {
            // Decrease searchAddr.
            cursor.StoreMin(newSearchAddr);
        }
        return (i, pallocChunkPages - 1);
    }
    // Clear searchAddr, because we've exhausted the heap.
    cursor.Clear();
    return (0, 0);
}

// alloc updates metadata for chunk at index ci with the fact that
// an allocation of npages occurred. It also eagerly attempts to collapse
// the chunk's memory into hugepage if the chunk has become sufficiently
// dense and we're not allocating the whole chunk at once (which suggests
// the allocation is part of a bigger one and it's probably not worth
// eagerly collapsing).
//
// alloc may only run concurrently with find.
internal static void alloc(this ref scavengeIndex s, chunkIdx ci, nuint npages) {
    var sc = Ꮡ(s.chunks, ci).load();
    sc.alloc(npages, s.gen);
    // TODO(mknyszek): Consider eagerly backing memory with huge pages
    // here and track whether we believe this chunk is backed by huge pages.
    // In the past we've attempted to use sysHugePageCollapse (which uses
    // MADV_COLLAPSE on Linux, and is unsupported elswhere) for this purpose,
    // but that caused performance issues in production environments.
    Ꮡ(s.chunks, ci).store(sc);
}

// free updates metadata for chunk at index ci with the fact that
// a free of npages occurred.
//
// free may only run concurrently with find.
internal static void free(this ж<scavengeIndex> Ꮡs, chunkIdx ci, nuint page, nuint npages) {
    ref var s = ref Ꮡs.DerefOrNull();

    var sc = Ꮡ(s.chunks, ci).load();
    sc.free(npages, s.gen);
    Ꮡ(s.chunks, ci).store(sc);
    // Update scavenge search addresses.
    var addr = chunkBase(ci) + (uintptr)(page + npages - 1) * (uintptr)pageSize;
    if (s.freeHWM.lessThan(new offAddr(addr))) {
        s.freeHWM = new offAddr(addr);
    }
    // N.B. Because free is serialized, it's not necessary to do a
    // full CAS here. free only ever increases searchAddr, while
    // find only ever decreases it. Since we only ever race with
    // decreases, even if the value we loaded is stale, the actual
    // value will never be larger.
    var (searchAddr, _) = Ꮡs.of(scavengeIndex.ᏑsearchAddrForce).Load();
    if ((new offAddr(searchAddr)).lessThan(new offAddr(addr))) {
        Ꮡs.of(scavengeIndex.ᏑsearchAddrForce).StoreMarked(addr);
    }
}

// nextGen moves the scavenger forward one generation. Must be called
// once per GC cycle, but may be called more often to force more memory
// to be released.
//
// nextGen may only run concurrently with find.
internal static void nextGen(this ж<scavengeIndex> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    s.gen++;
    var (searchAddr, _) = Ꮡs.of(scavengeIndex.ᏑsearchAddrBg).Load();
    if ((new offAddr(searchAddr)).lessThan(s.freeHWM)) {
        Ꮡs.of(scavengeIndex.ᏑsearchAddrBg).StoreMarked(s.freeHWM.addr());
    }
    s.freeHWM = minOffAddr;
}

// setEmpty marks that the scavenger has finished looking at ci
// for now to prevent the scavenger from getting stuck looking
// at the same chunk.
//
// setEmpty may only run concurrently with find.
internal static void setEmpty(this ref scavengeIndex s, chunkIdx ci) {
    var val = Ꮡ(s.chunks, ci).load();
    val.scavChunkFlags.setEmpty();
    Ꮡ(s.chunks, ci).store(val);
}

// atomicScavChunkData is an atomic wrapper around a scavChunkData
// that stores it in its packed form.
partial struct atomicScavChunkData {
    internal atomic.Uint64 value;
}

// load loads and unpacks a scavChunkData.
internal static scavChunkData load(this ж<atomicScavChunkData> Ꮡsc) {
    return unpackScavChunkData(Ꮡsc.of(atomicScavChunkData.Ꮡvalue).Load());
}

// store packs and writes a new scavChunkData. store must be serialized
// with other calls to store.
internal static void store(this ж<atomicScavChunkData> Ꮡsc, scavChunkData ssc) {
    Ꮡsc.of(atomicScavChunkData.Ꮡvalue).Store(ssc.pack());
}

// scavChunkData tracks information about a palloc chunk for
// scavenging. It packs well into 64 bits.
//
// The zero value always represents a valid newly-grown chunk.
partial struct scavChunkData {
    // inUse indicates how many pages in this chunk are currently
    // allocated.
    //
    // Only the first 10 bits are used.
    internal uint16 inUse;
    // lastInUse indicates how many pages in this chunk were allocated
    // when we transitioned from gen-1 to gen.
    //
    // Only the first 10 bits are used.
    internal uint16 lastInUse;
    // gen is the generation counter from a scavengeIndex from the
    // last time this scavChunkData was updated.
    internal uint32 gen;
    // scavChunkFlags represents additional flags
    //
    // Note: only 6 bits are available.
    internal partial ref scavChunkFlags scavChunkFlags { get; }
}

// unpackScavChunkData unpacks a scavChunkData from a uint64.
internal static scavChunkData unpackScavChunkData(uint64 sc) {
    return new scavChunkData(
        inUse: (uint16)sc,
        lastInUse: (uint16)((uint16)((sc >> (int)(16))) & (uint16)scavChunkInUseMask),
        gen: (uint32)((sc >> (int)(32))),
        scavChunkFlags: ((scavChunkFlags)((uint8)((uint8)((sc >> (int)((16 + logScavChunkInUseMax)))) & (uint8)scavChunkFlagsMask)))
    );
}

// pack returns sc packed into a uint64.
internal static uint64 pack(this scavChunkData sc) {
    return (uint64)((uint64)((uint64)((uint64)sc.inUse | (((uint64)sc.lastInUse << (int)(16)))) | (((uint64)(uint8)sc.scavChunkFlags << (int)((16 + logScavChunkInUseMax))))) | (((uint64)sc.gen << (int)(32))));
}

internal static scavChunkFlags scavChunkHasFree => /* 1 << iota */ 1;
internal static UntypedInt scavChunkMaxFlags => 6;
internal static UntypedInt scavChunkFlagsMask => /* (1 << scavChunkMaxFlags) - 1 */ 63;
internal static UntypedInt logScavChunkInUseMax => /* logPallocChunkPages + 1 */ 10;
internal static UntypedInt scavChunkInUseMask => /* (1 << logScavChunkInUseMax) - 1 */ 1023;

partial struct scavChunkFlags /*num:uint8*/;

// isEmpty returns true if the hasFree flag is unset.
internal static bool isEmpty(this ref scavChunkFlags sc) {
    return (scavChunkFlags)((sc) & scavChunkHasFree) == 0;
}

// setEmpty clears the hasFree flag.
internal static void setEmpty(this ref scavChunkFlags sc) {
    sc &= unchecked((scavChunkFlags)~(scavChunkFlags)(scavChunkHasFree));
}

// setNonEmpty sets the hasFree flag.
internal static void setNonEmpty(this ref scavChunkFlags sc) {
    sc |= (scavChunkFlags)(scavChunkHasFree);
}

// shouldScavenge returns true if the corresponding chunk should be interrogated
// by the scavenger.
internal static bool shouldScavenge(this scavChunkData sc, uint32 currGen, bool force) {
    if (sc.scavChunkFlags.isEmpty()) {
        // Nothing to scavenge.
        return false;
    }
    if (force) {
        // We're forcing the memory to be scavenged.
        return true;
    }
    if (sc.gen == currGen) {
        // In the current generation, if either the current or last generation
        // is dense, then skip scavenging. Inverting that, we should scavenge
        // if both the current and last generation were not dense.
        return sc.inUse < scavChunkHiOccPages && sc.lastInUse < scavChunkHiOccPages;
    }
    // If we're one or more generations ahead, we know inUse represents the current
    // state of the chunk, since otherwise it would've been updated already.
    return sc.inUse < scavChunkHiOccPages;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tooManyPagesAllocatedInˢ = "too many pages allocated in chunk?"u8;

// alloc updates sc given that npages were allocated in the corresponding chunk.
internal static void alloc(this ref scavChunkData sc, nuint npages, uint32 newGen) {
    if ((nuint)sc.inUse + npages > pallocChunkPages) {
        print((@string)"runtime: inUse="u8, sc.inUse, (@string)" npages="u8, npages, (@string)"\n"u8);
        @throw(tooManyPagesAllocatedInˢ);
    }
    if (sc.gen != newGen) {
        sc.lastInUse = sc.inUse;
        sc.gen = newGen;
    }
    sc.inUse += (uint16)npages;
    if (sc.inUse == pallocChunkPages) {
        // There's nothing for the scavenger to take from here.
        sc.scavChunkFlags.setEmpty();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string allocatedPagesBelowZeroˢ = "allocated pages below zero?"u8;

// free updates sc given that npages was freed in the corresponding chunk.
internal static void free(this ref scavChunkData sc, nuint npages, uint32 newGen) {
    if ((nuint)sc.inUse < npages) {
        print((@string)"runtime: inUse="u8, sc.inUse, (@string)" npages="u8, npages, (@string)"\n"u8);
        @throw(allocatedPagesBelowZeroˢ);
    }
    if (sc.gen != newGen) {
        sc.lastInUse = sc.inUse;
        sc.gen = newGen;
    }
    sc.inUse -= (uint16)npages;
    // The scavenger can no longer be done with this chunk now that
    // new memory has been freed into it.
    sc.scavChunkFlags.setNonEmpty();
}

partial struct piController {
    internal float64 kp; // Proportional constant.
    internal float64 ti; // Integral time constant.
    internal float64 tt; // Reset time.
    internal float64 min, max; // Output boundaries.
// PI controller state.
    internal float64 errIntegral; // Integral of the error from t=0 to now.
    // Error flags.
    internal bool errOverflow; // Set if errIntegral ever overflowed.
    internal bool inputOverflow; // Set if an operation with the input overflowed.
}

// next provides a new sample to the controller.
//
// input is the sample, setpoint is the desired point, and period is how much
// time (in whatever unit makes the most sense) has passed since the last sample.
//
// Returns a new value for the variable it's controlling, and whether the operation
// completed successfully. One reason this might fail is if error has been growing
// in an unbounded manner, to the point of overflow.
//
// In the specific case of an error overflow occurs, the errOverflow field will be
// set and the rest of the controller's internal state will be fully reset.
internal static (float64, bool) next(this ref piController c, float64 input, float64 setpoint, float64 period) {
    // Compute the raw output value.
    var prop = c.kp * (setpoint - input);
    var rawOutput = prop + c.errIntegral;
    // Clamp rawOutput into output.
    var output = rawOutput;
    if (isInf(output) || isNaN(output)) {
        // The input had a large enough magnitude that either it was already
        // overflowed, or some operation with it overflowed.
        // Set a flag and reset. That's the safest thing to do.
        c.reset();
        c.inputOverflow = true;
        return (c.min, false);
    }
    if (output < c.min){
        output = c.min;
    } else 
    if (output > c.max) {
        output = c.max;
    }
    // Update the controller's state.
    if (c.ti != 0D && c.tt != 0D) {
        c.errIntegral += (c.kp * period / c.ti) * (setpoint - input) + (period / c.tt) * (output - rawOutput);
        if (isInf(c.errIntegral) || isNaN(c.errIntegral)) {
            // So much error has accumulated that we managed to overflow.
            // The assumptions around the controller have likely broken down.
            // Set a flag and reset. That's the safest thing to do.
            c.reset();
            c.errOverflow = true;
            return (c.min, false);
        }
    }
    return (output, true);
}

// reset resets the controller state, except for controller error flags.
internal static void reset(this ref piController c) {
    c.errIntegral = 0D;
}

} // end runtime_package
