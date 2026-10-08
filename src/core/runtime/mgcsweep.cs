// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Garbage collector: sweeping
// The sweeper consists of two different algorithms:
//
// * The object reclaimer finds and frees unmarked slots in spans. It
//   can free a whole span if none of the objects are marked, but that
//   isn't its goal. This can be driven either synchronously by
//   mcentral.cacheSpan for mcentral spans, or asynchronously by
//   sweepone, which looks at all the mcentral lists.
//
// * The span reclaimer looks for spans that contain no marked objects
//   and frees whole spans. This is a separate algorithm because
//   freeing whole spans is the hardest task for the object reclaimer,
//   but is critical when allocating new spans. The entry point for
//   this is mheap_.reclaim and it's driven by a sequential scan of
//   the page marks bitmap in the heap arenas.
//
// Both algorithms ultimately call mspan.sweep, which sweeps a single
// heap span.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

internal static ж<sweepdata> ᏑΔsweep = new StandardBox<sweepdata>(new sweepdata());
internal static ref sweepdata Δsweep => ref ᏑΔsweep.Value;

// State of background sweep.
partial struct sweepdata {
    internal mutex @lock;
    internal ж<g> g;
    internal bool parked;
    // active tracks outstanding sweepers and the sweep
    // termination condition.
    internal activeSweep active;
    // centralIndex is the current unswept span class.
    // It represents an index into the mcentral span
    // sets. Accessed and updated via its load and
    // update methods. Not protected by a lock.
    //
    // Reset at mark termination.
    // Used by mheap.nextSpanForSweep.
    internal sweepClass centralIndex;
}

partial struct sweepClass /*num:uint32*/;

internal static UntypedInt numSweepClasses => /* numSpanClasses * 2 */ 272;
internal static sweepClass sweepClassDone => /* sweepClass(^uint32(0)) */ 4294967295;

internal static sweepClass load(this ж<sweepClass> Ꮡs) {
    return ((sweepClass)atomic.Load(Ꮡs.Reinterpret<sweepClass, uint32>()));
}

internal static void update(this ж<sweepClass> Ꮡs, sweepClass sNew) {
    ref var s = ref Ꮡs.DerefOrNull();

    // Only update *s if its current value is less than sNew,
    // since *s increases monotonically.
    var sOld = Ꮡs.load();
    while (sOld < sNew && !atomic.Cas(Ꮡs.Reinterpret<sweepClass, uint32>(), (uint32)sOld, (uint32)sNew)) {
        sOld = Ꮡs.load();
    }
}

// TODO(mknyszek): This isn't the only place we have
// an atomic monotonically increasing counter. It would
// be nice to have an "atomic max" which is just implemented
// as the above on most architectures. Some architectures
// like RISC-V however have native support for an atomic max.
internal static void clear(this ж<sweepClass> Ꮡs) {
    atomic.Store(Ꮡs.Reinterpret<sweepClass, uint32>(), 0);
}

// split returns the underlying span class as well as
// whether we're interested in the full or partial
// unswept lists for that class, indicated as a boolean
// (true means "full").
internal static (spanClass spc, bool full) split(this sweepClass s) {
    return (((spanClass)(uint8)((uint32)((s >> (int)(1))))), (sweepClass)(s & 1) == 0);
}

// nextSpanForSweep finds and pops the next span for sweeping from the
// central sweep buffers. It returns ownership of the span to the caller.
// Returns nil if no such span exists.
internal static ж<mspan> nextSpanForSweep(this ref mheap h) {
    var sg = h.sweepgen;
    for (var sc = ᏑΔsweep.of(sweepdata.ᏑcentralIndex).load(); sc < numSweepClasses; sc++) {
        var (spc, full) = sc.split();
        var c = Ꮡ(h.central, spc).of(mheap_central.Ꮡmcentral);
        ж<mspan> s = default!;
        if (full){
            s = c.fullUnswept(sg).pop();
        } else {
            s = c.partialUnswept(sg).pop();
        }
        if (s != nil) {
            // Write down that we found something so future sweepers
            // can start from here.
            ᏑΔsweep.of(sweepdata.ᏑcentralIndex).update(sc);
            return s;
        }
    }
    // Write down that we found nothing.
    ᏑΔsweep.of(sweepdata.ᏑcentralIndex).update(sweepClassDone);
    return default!;
}

internal static UntypedInt sweepDrainedMask => /* 1 << 31 */ 2147483648;

// activeSweep is a type that captures whether sweeping
// is done, and whether there are any outstanding sweepers.
//
// Every potential sweeper must call begin() before they look
// for work, and end() after they've finished sweeping.
partial struct activeSweep {
    // state is divided into two parts.
    //
    // The top bit (masked by sweepDrainedMask) is a boolean
    // value indicating whether all the sweep work has been
    // drained from the queue.
    //
    // The rest of the bits are a counter, indicating the
    // number of outstanding concurrent sweepers.
    internal atomic.Uint32 state;
}

// begin registers a new sweeper. Returns a sweepLocker
// for acquiring spans for sweeping. Any outstanding sweeper blocks
// sweep termination.
//
// If the sweepLocker is invalid, the caller can be sure that all
// outstanding sweep work has been drained, so there is nothing left
// to sweep. Note that there may be sweepers currently running, so
// this does not indicate that all sweeping has completed.
//
// Even if the sweepLocker is invalid, its sweepGen is always valid.
internal static sweepLocker begin(this ж<activeSweep> Ꮡa) {
    while (ᐧ) {
        var state = Ꮡa.of(activeSweep.Ꮡstate).Load();
        if ((uint32)(state & (uint32)sweepDrainedMask) != 0) {
            return new sweepLocker(mheap_.sweepgen, false);
        }
        if (Ꮡa.of(activeSweep.Ꮡstate).CompareAndSwap(state, state + 1)) {
            return new sweepLocker(mheap_.sweepgen, true);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sweeperLeftOutstandingˢ = "sweeper left outstanding across sweep generations"u8;
internal static readonly @string mismatchedBeginEndOfˢ = "mismatched begin/end of activeSweep"u8;

// end deregisters a sweeper. Must be called once for each time
// begin is called if the sweepLocker is valid.
internal static void end(this ж<activeSweep> Ꮡa, sweepLocker sl) {
    if (sl.sweepGen != mheap_.sweepgen) {
        @throw(sweeperLeftOutstandingˢ);
    }
    while (ᐧ) {
        var state = Ꮡa.of(activeSweep.Ꮡstate).Load();
        if (((uint32)(state & ~(uint32)sweepDrainedMask)) - 1 >= sweepDrainedMask) {
            @throw(mismatchedBeginEndOfˢ);
        }
        if (Ꮡa.of(activeSweep.Ꮡstate).CompareAndSwap(state, state - 1)) {
            if (state != sweepDrainedMask) {
                return;
            }
            if (debug.gcpacertrace > 0) {
                var live = ᏑgcController.of(gcControllerState.ᏑheapLive).Load();
                print((@string)"pacer: sweep done at heap size "u8, (live >> (int)(20)), (@string)"MB; allocated "u8, ((live - mheap_.sweepHeapLiveBasis) >> (int)(20)), (@string)"MB during sweep; swept "u8, Ꮡmheap_.of(mheap.ᏑpagesSwept).Load(), (@string)" pages at "u8, mheap_.sweepPagesPerByte, (@string)" pages/byte\n"u8);
            }
            return;
        }
    }
}

// markDrained marks the active sweep cycle as having drained
// all remaining work. This is safe to be called concurrently
// with all other methods of activeSweep, though may race.
//
// Returns true if this call was the one that actually performed
// the mark.
internal static bool markDrained(this ж<activeSweep> Ꮡa) {
    while (ᐧ) {
        var state = Ꮡa.of(activeSweep.Ꮡstate).Load();
        if ((uint32)(state & (uint32)sweepDrainedMask) != 0) {
            return false;
        }
        if (Ꮡa.of(activeSweep.Ꮡstate).CompareAndSwap(state, (uint32)(state | (uint32)sweepDrainedMask))) {
            return true;
        }
    }
}

// sweepers returns the current number of active sweepers.
internal static uint32 sweepers(this ж<activeSweep> Ꮡa) {
    return (uint32)(Ꮡa.of(activeSweep.Ꮡstate).Load() & ~(uint32)sweepDrainedMask);
}

// isDone returns true if all sweep work has been drained and no more
// outstanding sweepers exist. That is, when the sweep phase is
// completely done.
internal static bool isDone(this ж<activeSweep> Ꮡa) {
    return Ꮡa.of(activeSweep.Ꮡstate).Load() == sweepDrainedMask;
}

// reset sets up the activeSweep for the next sweep cycle.
//
// The world must be stopped.
internal static void reset(this ж<activeSweep> Ꮡa) {
    assertWorldStopped();
    Ꮡa.of(activeSweep.Ꮡstate).Store(0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string activeSweepersFoundAtˢ = "active sweepers found at start of mark phase"u8;

// finishsweep_m ensures that all spans are swept.
//
// The world must be stopped. This ensures there are no sweeps in
// progress.
//
//go:nowritebarrier
internal static void finishsweep_m() {
    assertWorldStopped();
    // Sweeping must be complete before marking commences, so
    // sweep any unswept spans. If this is a concurrent GC, there
    // shouldn't be any spans left to sweep, so this should finish
    // instantly. If GC was forced before the concurrent sweep
    // finished, there may be spans to sweep.
    while (sweepone() != ~(uintptr)0) {
    }
    // Make sure there aren't any outstanding sweepers left.
    // At this point, with the world stopped, it means one of two
    // things. Either we were able to preempt a sweeper, or that
    // a sweeper didn't call sweep.active.end when it should have.
    // Both cases indicate a bug, so throw.
    if (ᏑΔsweep.of(sweepdata.Ꮡactive).sweepers() != 0) {
        @throw(activeSweepersFoundAtˢ);
    }
    // Reset all the unswept buffers, which should be empty.
    // Do this in sweep termination as opposed to mark termination
    // so that we can catch unswept spans and reclaim blocks as
    // soon as possible.
    var sg = mheap_.sweepgen;
    foreach (var (i, _) in mheap_.central) {
        var c = Ꮡmheap_.at(mheap.Ꮡcentral, i).of(mheap_central.Ꮡmcentral);
        c.partialUnswept(sg).reset();
        c.fullUnswept(sg).reset();
    }
    // Sweeping is done, so there won't be any new memory to
    // scavenge for a bit.
    //
    // If the scavenger isn't already awake, wake it up. There's
    // definitely work for it to do at this point.
    Ꮡscavenger.wake();
    nextMarkBitArenaEpoch();
}

internal static void bgsweep(channel<nint> c) {
    Δsweep.g = getg();
    lockInit(ᏑΔsweep.of(sweepdata.Ꮡlock), lockRankSweep);
    @lock(ᏑΔsweep.of(sweepdata.Ꮡlock));
    Δsweep.parked = true;
    c.ᐸꟷ(1);
    goparkunlock(ᏑΔsweep.of(sweepdata.Ꮡlock), waitReasonGCSweepWait, traceBlockGCSweep, 1);
    while (ᐧ) {
        // bgsweep attempts to be a "low priority" goroutine by intentionally
        // yielding time. It's OK if it doesn't run, because goroutines allocating
        // memory will sweep and ensure that all spans are swept before the next
        // GC cycle. We really only want to run when we're idle.
        //
        // However, calling Gosched after each span swept produces a tremendous
        // amount of tracing events, sometimes up to 50% of events in a trace. It's
        // also inefficient to call into the scheduler so much because sweeping a
        // single span is in general a very fast operation, taking as little as 30 ns
        // on modern hardware. (See #54767.)
        //
        // As a result, bgsweep sweeps in batches, and only calls into the scheduler
        // at the end of every batch. Furthermore, it only yields its time if there
        // isn't spare idle time available on other cores. If there's available idle
        // time, helping to sweep can reduce allocation latencies by getting ahead of
        // the proportional sweeper and having spans ready to go for allocation.
        const nint sweepBatchSize = 10;
        nint nSwept = 0;
        while (sweepone() != ~(uintptr)0) {
            nSwept++;
            if (nSwept % sweepBatchSize == 0) {
                goschedIfBusy();
            }
        }
        while (freeSomeWbufs(true)) {
            // N.B. freeSomeWbufs is already batched internally.
            goschedIfBusy();
        }
        @lock(ᏑΔsweep.of(sweepdata.Ꮡlock));
        if (!isSweepDone()) {
            // This can happen if a GC runs between
            // gosweepone returning ^0 above
            // and the lock being acquired.
            unlock(ᏑΔsweep.of(sweepdata.Ꮡlock));
            continue;
        }
        Δsweep.parked = true;
        goparkunlock(ᏑΔsweep.of(sweepdata.Ꮡlock), waitReasonGCSweepWait, traceBlockGCSweep, 1);
    }
}

// sweepLocker acquires sweep ownership of spans.
partial struct sweepLocker {
    // sweepGen is the sweep generation of the heap.
    internal uint32 sweepGen;
    internal bool valid;
}

// sweepLocked represents sweep ownership of a span.
partial struct sweepLocked {
    internal partial ref ж<mspan> mspan { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string useOfInvalidSweepLockerˢ = "use of invalid sweepLocker"u8;

// tryAcquire attempts to acquire sweep ownership of span s. If it
// successfully acquires ownership, it blocks sweep completion.
internal static (sweepLocked, bool) tryAcquire(this ref sweepLocker l, ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (!l.valid) {
        @throw(useOfInvalidSweepLockerˢ);
    }
    // Check before attempting to CAS.
    if (atomic.Load(Ꮡs.of(mspan.Ꮡsweepgen)) != l.sweepGen - 2) {
        return (new sweepLocked(nil), false);
    }
    // Attempt to acquire sweep ownership of s.
    if (!atomic.Cas(Ꮡs.of(mspan.Ꮡsweepgen), l.sweepGen - 2, l.sweepGen - 1)) {
        return (new sweepLocked(nil), false);
    }
    return (new sweepLocked(Ꮡs), true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonInUseSpanInUnsweptˢ = "non in-use span in unswept list"u8;

// sweepone sweeps some unswept heap span and returns the number of pages returned
// to the heap, or ^uintptr(0) if there was nothing to sweep.
internal static uintptr sweepone() {
    var gp = getg();
    // Increment locks to ensure that the goroutine is not preempted
    // in the middle of sweep thus leaving the span in an inconsistent state for next GC
    gp.Value.m.Value.locks++;
    // TODO(austin): sweepone is almost always called in a loop;
    // lift the sweepLocker into its callers.
    var sl = ᏑΔsweep.of(sweepdata.Ꮡactive).begin();
    if (!sl.valid) {
        gp.Value.m.Value.locks--;
        return ~(uintptr)0;
    }
    // Find a span to sweep.
    var npages = ~(uintptr)0;
    bool noMoreWork = default!;
    while (ᐧ) {
        var s = mheap_.nextSpanForSweep();
        if (s == nil) {
            noMoreWork = ᏑΔsweep.of(sweepdata.Ꮡactive).markDrained();
            break;
        }
        {
            var state = s.of(mspan.Ꮡstate).get(); if (state != mSpanInUse) {
                // This can happen if direct sweeping already
                // swept this span, but in that case the sweep
                // generation should always be up-to-date.
                if (!((~s).sweepgen == sl.sweepGen || (~s).sweepgen == sl.sweepGen + 3)) {
                    print((@string)"runtime: bad span s.state="u8, state, (@string)" s.sweepgen="u8, (~s).sweepgen, (@string)" sweepgen="u8, sl.sweepGen, (@string)"\n"u8);
                    @throw(nonInUseSpanInUnsweptˢ);
                }
                continue;
            }
        }
        {
            var (sΔ1, ok) = sl.tryAcquire(s); if (ok) {
                // Sweep the span we found.
                npages = sΔ1.npages;
                if (sΔ1.sweep(false)){
                    // Whole span was freed. Count it toward the
                    // page reclaimer credit since these pages can
                    // now be used for span allocation.
                    Ꮡmheap_.of(mheap.ᏑreclaimCredit).Add(npages);
                } else {
                    // Span is still in-use, so this returned no
                    // pages to the heap and the span needs to
                    // move to the swept in-use list.
                    npages = 0;
                }
                break;
            }
        }
    }
    ᏑΔsweep.of(sweepdata.Ꮡactive).end(sl);
    if (noMoreWork) {
        // The sweep list is empty. There may still be
        // concurrent sweeps running, but we're at least very
        // close to done sweeping.
        // Move the scavenge gen forward (signaling
        // that there's new work to do) and wake the scavenger.
        //
        // The scavenger is signaled by the last sweeper because once
        // sweeping is done, we will definitely have useful work for
        // the scavenger to do, since the scavenger only runs over the
        // heap once per GC cycle. This update is not done during sweep
        // termination because in some cases there may be a long delay
        // between sweep done and sweep termination (e.g. not enough
        // allocations to trigger a GC) which would be nice to fill in
        // with scavenging work.
        if (debug.scavtrace > 0) {
            systemstack(() => {
                @lock(Ꮡmheap_.of(mheap.Ꮡlock));
                // Get released stats.
                var releasedBg = Ꮡmheap_.of(mheap.Ꮡpages).of(pageAlloc.Ꮡscav).of(pageAlloc_scav.ᏑreleasedBg).Load();
                var releasedEager = Ꮡmheap_.of(mheap.Ꮡpages).of(pageAlloc.Ꮡscav).of(pageAlloc_scav.ᏑreleasedEager).Load();
                // Print the line.
                printScavTrace(releasedBg, releasedEager, false);
                // Update the stats.
                Ꮡmheap_.of(mheap.Ꮡpages).of(pageAlloc.Ꮡscav).of(pageAlloc_scav.ᏑreleasedBg).Add(((uintptr)0 - releasedBg));
                Ꮡmheap_.of(mheap.Ꮡpages).of(pageAlloc.Ꮡscav).of(pageAlloc_scav.ᏑreleasedEager).Add(((uintptr)0 - releasedEager));
                unlock(Ꮡmheap_.of(mheap.Ꮡlock));
            });
        }
        Ꮡscavenger.ready();
    }
    gp.Value.m.Value.locks--;
    return npages;
}

// isSweepDone reports whether all spans are swept.
//
// Note that this condition may transition from false to true at any
// time as the sweeper runs. It may transition from true to false if a
// GC runs; to prevent that the caller must be non-preemptible or must
// somehow block GC progress.
internal static bool isSweepDone() {
    return ᏑΔsweep.of(sweepdata.Ꮡactive).isDone();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mspanEnsureSweptMIsNotˢ = "mspan.ensureSwept: m is not locked"u8;

// Returns only when span s has been swept.
//
//go:nowritebarrier
internal static void ensureSwept(this ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    // Caller must disable preemption.
    // Otherwise when this function returns the span can become unswept again
    // (if GC is triggered on another goroutine).
    var gp = getg();
    if ((~(~gp).m).locks == 0 && (~(~gp).m).mallocing == 0 && gp != (~(~gp).m).g0) {
        @throw(mspanEnsureSweptMIsNotˢ);
    }
    // If this operation fails, then that means that there are
    // no more spans to be swept. In this case, either s has already
    // been swept, or is about to be acquired for sweeping and swept.
    var sl = ᏑΔsweep.of(sweepdata.Ꮡactive).begin();
    if (sl.valid) {
        // The caller must be sure that the span is a mSpanInUse span.
        {
            var (sΔ1, ok) = sl.tryAcquire(Ꮡs); if (ok) {
                sΔ1.sweep(false);
                ᏑΔsweep.of(sweepdata.Ꮡactive).end(sl);
                return;
            }
        }
        ᏑΔsweep.of(sweepdata.Ꮡactive).end(sl);
    }
    // Unfortunately we can't sweep the span ourselves. Somebody else
    // got to it first. We don't have efficient means to wait, but that's
    // OK, it will be swept fairly soon.
    while (ᐧ) {
        var spangen = atomic.Load(Ꮡs.of(mspan.Ꮡsweepgen));
        if (spangen == sl.sweepGen || spangen == sl.sweepGen + 3) {
            break;
        }
        osyield();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mspanSweepMIsNotLockedˢ = "mspan.sweep: m is not locked"u8;
internal static readonly @string mspanSweepBadSpanStateˢ = "mspan.sweep: bad span state"u8;
internal static readonly @string sweepIncreasedAllocationˢ = "sweep increased allocation count"u8;
internal static readonly @string mspanSweepBadSpanStateˢ2 = "mspan.sweep: bad span state after sweep"u8;
internal static readonly @string sweptCachedSpanˢ = "swept cached span"u8;
internal static readonly @string sweepTriedToPreserveAˢ = "sweep: tried to preserve a user arena span"u8;
internal static readonly @string userArenaSpanIsOnTheˢ = "user arena span is on the wrong list"u8;

// sweep frees or collects finalizers for blocks not marked in the mark phase.
// It clears the mark bits in preparation for the next GC round.
// Returns true if the span was returned to heap.
// If preserve=true, don't return it to heap nor relink in mcentral lists;
// caller takes care of it.
internal static bool sweep(this ref sweepLocked sl, bool preserve) {
    // It's critical that we enter this function with preemption disabled,
    // GC must not start while we are in the middle of this function.
    var gp = getg();
    if ((~(~gp).m).locks == 0 && (~(~gp).m).mallocing == 0 && gp != (~(~gp).m).g0) {
        @throw(mspanSweepMIsNotLockedˢ);
    }
    var s = sl.mspan;
    if (!preserve) {
        // We'll release ownership of this span. Nil it out to
        // prevent the caller from accidentally using it.
        sl.mspan = default!;
    }
    var sweepgen = mheap_.sweepgen;
    {
        var state = s.of(mspan.Ꮡstate).get(); if (state != mSpanInUse || (~s).sweepgen != sweepgen - 1) {
            print((@string)"mspan.sweep: state="u8, state, (@string)" sweepgen="u8, (~s).sweepgen, (@string)" mheap.sweepgen="u8, sweepgen, (@string)"\n"u8);
            @throw(mspanSweepBadSpanStateˢ);
        }
    }
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepSpan((~s).npages * (uintptr)_PageSize);
        traceRelease(Δtrace);
    }
    Ꮡmheap_.of(mheap.ᏑpagesSwept).Add((int64)(~s).npages);
    var spc = s.Value.spanclass;
    var size = s.Value.elemsize;
    // The allocBits indicate which unmarked objects don't need to be
    // processed since they were free at the end of the last GC cycle
    // and were not allocated since then.
    // If the allocBits index is >= s.freeindex and the bit
    // is not marked then the object remains unallocated
    // since the last GC.
    // This situation is analogous to being on a freelist.
    // Unlink & free special records for any objects we're about to free.
    // Two complications here:
    // 1. An object can have both finalizer and profile special records.
    //    In such case we need to queue finalizer for execution,
    //    mark the object as live and preserve the profile special.
    // 2. A tiny object can have several finalizers setup for different offsets.
    //    If such object is not marked, we need to queue all finalizers at once.
    // Both 1 and 2 are possible at the same time.
    var hadSpecials = (~s).specials != nil;
    var siter = newSpecialsIter(s);
    while (siter.valid()) {
        // A finalizer can be set for an inner byte of an object, find object beginning.
        var objIndex = (uintptr)(~siter.s).offset / size;
        var Δp = s.@base() + objIndex * size;
        var mbits = s.markBitsForIndex(objIndex);
        if (!mbits.isMarked()){
            // This object is not marked and has at least one special record.
            // Pass 1: see if it has a finalizer.
            var hasFinAndRevived = false;
            var endOffset = Δp - s.@base() + size;
            for (var tmp = siter.s; tmp != nil && (uintptr)(~tmp).offset < endOffset; tmp = tmp.Value.next) {
                if ((~tmp).kind == _KindSpecialFinalizer) {
                    // Stop freeing of object if it has a finalizer.
                    mbits.setMarkedNonAtomic();
                    hasFinAndRevived = true;
                    break;
                }
            }
            if (hasFinAndRevived){
                // Pass 2: queue all finalizers and clear any weak handles. Weak handles are cleared
                // before finalization as specified by the weak package. See the documentation
                // for that package for more details.
                while (siter.valid() && (uintptr)(~siter.s).offset < endOffset) {
                    // Find the exact byte for which the special was setup
                    // (as opposed to object beginning).
                    var special = siter.s;
                    var pΔ1 = s.@base() + (uintptr)(~special).offset;
                    if ((~special).kind == _KindSpecialFinalizer || (~special).kind == _KindSpecialWeakHandle){
                        siter.unlinkAndNext();
                        freeSpecial(special, (@unsafe.Pointer)pΔ1, size);
                    } else {
                        // All other specials only apply when an object is freed,
                        // so just keep the special record.
                        siter.next();
                    }
                }
            } else {
                // Pass 2: the object is truly dead, free (and handle) all specials.
                while (siter.valid() && (uintptr)(~siter.s).offset < endOffset) {
                    // Find the exact byte for which the special was setup
                    // (as opposed to object beginning).
                    var special = siter.s;
                    var pΔ2 = s.@base() + (uintptr)(~special).offset;
                    siter.unlinkAndNext();
                    freeSpecial(special, (@unsafe.Pointer)pΔ2, size);
                }
            }
        } else {
            // object is still live
            if ((~siter.s).kind == _KindSpecialReachable){
                var special = siter.unlinkAndNext();
                (special.Reinterpret<special, specialReachable>()).Value.reachable = true;
                freeSpecial(special, (@unsafe.Pointer)Δp, size);
            } else {
                // keep special record
                siter.next();
            }
        }
    }
    if (hadSpecials && (~s).specials == nil) {
        spanHasNoSpecials(s);
    }
    if (traceAllocFreeEnabled() || debug.clobberfree != 0 || raceenabled || msanenabled || asanenabled) {
        // Find all newly freed objects.
        var mbits = s.markBitsForBase();
        var abits = s.allocBitsForIndex(0);
        for (var i = (uintptr)0; i < (uintptr)(~s).nelems; i++) {
            if (!mbits.isMarked() && (abits.index < (uintptr)(~s).freeindex || abits.isMarked())) {
                var x = s.@base() + i * (~s).elemsize;
                if (traceAllocFreeEnabled()) {
                    var traceΔ1 = traceAcquire();
                    if (traceΔ1.ok()) {
                        traceΔ1.HeapObjectFree(x);
                        traceRelease(traceΔ1);
                    }
                }
                if (debug.clobberfree != 0) {
                    clobberfree((@unsafe.Pointer)x, size);
                }
                // User arenas are handled on explicit free.
                if (raceenabled && !(~s).isUserArenaChunk) {
                    racefree((@unsafe.Pointer)x, size);
                }
                if (msanenabled && !(~s).isUserArenaChunk) {
                    msanfree((@unsafe.Pointer)x, size);
                }
                if (asanenabled && !(~s).isUserArenaChunk) {
                    asanpoison((@unsafe.Pointer)x, size);
                }
            }
            mbits.advance();
            abits.advance();
        }
    }
    // Check for zombie objects.
    if ((~s).freeindex < (~s).nelems) {
        // Everything < freeindex is allocated and hence
        // cannot be zombies.
        //
        // Check the first bitmap byte, where we have to be
        // careful with freeindex.
        var obj = (uintptr)(~s).freeindex;
        if ((uint8)((((uint8)((~s).gcmarkBits.bytep(obj / 8).Value & ~(~s).allocBits.bytep(obj / 8).Value)) >> (int)((obj % 8)))) != 0) {
            s.reportZombies();
        }
        // Check remaining bytes.
        for (var i = obj / 8 + 1; i < divRoundUp((uintptr)(~s).nelems, 8); i++) {
            if ((uint8)((~s).gcmarkBits.bytep(i).Value & ~(~s).allocBits.bytep(i).Value) != 0) {
                s.reportZombies();
            }
        }
    }
    // Count the number of free objects in this span.
    var nalloc = (uint16)s.countAlloc();
    var nfreed = (uint16)((~s).allocCount - nalloc);
    if (nalloc > (~s).allocCount) {
        // The zombie check above should have caught this in
        // more detail.
        print((@string)"runtime: nelems="u8, (~s).nelems, (@string)" nalloc="u8, nalloc, (@string)" previous allocCount="u8, (~s).allocCount, (@string)" nfreed="u8, nfreed, (@string)"\n"u8);
        @throw(sweepIncreasedAllocationˢ);
    }
    s.Value.allocCount = nalloc;
    s.Value.freeindex = 0; // reset allocation index to start of span.
    s.Value.freeIndexForScan = 0;
    if (traceEnabled()) {
        (~(~getg()).m).p.ptr().Value.trace.reclaimed += (uintptr)nfreed * (~s).elemsize;
    }
    // gcmarkBits becomes the allocBits.
    // get a fresh cleared gcmarkBits in preparation for next GC
    s.Value.allocBits = s.Value.gcmarkBits;
    s.Value.gcmarkBits = newMarkBits((uintptr)(~s).nelems);
    // refresh pinnerBits if they exists
    if ((~s).pinnerBits != nil) {
        s.refreshPinnerBits();
    }
    // Initialize alloc bits cache.
    s.refillAllocCache(0);
    // The span must be in our exclusive ownership until we update sweepgen,
    // check for potential races.
    {
        var state = s.of(mspan.Ꮡstate).get(); if (state != mSpanInUse || (~s).sweepgen != sweepgen - 1) {
            print((@string)"mspan.sweep: state="u8, state, (@string)" sweepgen="u8, (~s).sweepgen, (@string)" mheap.sweepgen="u8, sweepgen, (@string)"\n"u8);
            @throw(mspanSweepBadSpanStateˢ2);
        }
    }
    if ((~s).sweepgen == sweepgen + 1 || (~s).sweepgen == sweepgen + 3) {
        @throw(sweptCachedSpanˢ);
    }
    // We need to set s.sweepgen = h.sweepgen only when all blocks are swept,
    // because of the potential for a concurrent free/SetFinalizer.
    //
    // But we need to set it before we make the span available for allocation
    // (return it to heap or mcentral), because allocation code assumes that a
    // span is already swept if available for allocation.
    //
    // Serialization point.
    // At this point the mark bits are cleared and allocation ready
    // to go so release the span.
    atomic.Store(s.of(mspan.Ꮡsweepgen), sweepgen);
    if ((~s).isUserArenaChunk) {
        if (preserve) {
            // This is a case that should never be handled by a sweeper that
            // preserves the span for reuse.
            @throw(sweepTriedToPreserveAˢ);
        }
        if (nalloc > 0) {
            // There still exist pointers into the span or the span hasn't been
            // freed yet. It's not ready to be reused. Put it back on the
            // full swept list for the next cycle.
            mheap_.central[spc].mcentral.fullSwept(sweepgen).push(s);
            return false;
        }
        // It's only at this point that the sweeper doesn't actually need to look
        // at this arena anymore, so subtract from pagesInUse now.
        Ꮡmheap_.of(mheap.ᏑpagesInUse).Add(((uintptr)0 - (~s).npages));
        s.of(mspan.Ꮡstate).set(mSpanDead);
        // The arena is ready to be recycled. Remove it from the quarantine list
        // and place it on the ready list. Don't add it back to any sweep lists.
        var sʗ1 = s;
        systemstack(() => {
            // It's the arena code's responsibility to get the chunk on the quarantine
            // list by the time all references to the chunk are gone.
            if ((~sʗ1).list != Ꮡmheap_.of(mheap.ᏑuserArena).of(mheap_userArena.ᏑquarantineList)) {
                @throw(userArenaSpanIsOnTheˢ);
            }
            @lock(Ꮡmheap_.of(mheap.Ꮡlock));
            Ꮡmheap_.of(mheap.ᏑuserArena).of(mheap_userArena.ᏑquarantineList).remove(sʗ1);
            Ꮡmheap_.of(mheap.ᏑuserArena).of(mheap_userArena.ᏑreadyList).insert(sʗ1);
            unlock(Ꮡmheap_.of(mheap.Ꮡlock));
        });
        return false;
    }
    if (spc.sizeclass() != 0){
        // Handle spans for small objects.
        if (nfreed > 0) {
            // Only mark the span as needing zeroing if we've freed any
            // objects, because a fresh span that had been allocated into,
            // wasn't totally filled, but then swept, still has all of its
            // free slots zeroed.
            s.Value.needzero = 1;
            var stats = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
            atomic.Xadd64(stats.at(heapStatsDelta.ᏑsmallFreeCount, (nint)(spc.sizeclass())), (int64)nfreed);
            Ꮡmemstats.of(mstats.ᏑheapStats).release();
            // Count the frees in the inconsistent, internal stats.
            ᏑgcController.of(gcControllerState.ᏑtotalFree).Add((int64)nfreed * (int64)(~s).elemsize);
        }
        if (!preserve) {
            // The caller may not have removed this span from whatever
            // unswept set its on but taken ownership of the span for
            // sweeping by updating sweepgen. If this span still is in
            // an unswept set, then the mcentral will pop it off the
            // set, check its sweepgen, and ignore it.
            if (nalloc == 0) {
                // Free totally free span directly back to the heap.
                Ꮡmheap_.freeSpan(s);
                return true;
            }
            // Return span back to the right mcentral list.
            if (nalloc == (~s).nelems){
                mheap_.central[spc].mcentral.fullSwept(sweepgen).push(s);
            } else {
                mheap_.central[spc].mcentral.partialSwept(sweepgen).push(s);
            }
        }
    } else 
    if (!preserve) {
        // Handle spans for large objects.
        if (nfreed != 0) {
            // Free large object span to heap.
            // Count the free in the consistent, external stats.
            //
            // Do this before freeSpan, which might update heapStats' inHeap
            // value. If it does so, then metrics that subtract object footprint
            // from inHeap might overflow. See #67019.
            var stats = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
            atomic.Xadd64(stats.of(heapStatsDelta.ᏑlargeFreeCount), 1);
            atomic.Xadd64(stats.of(heapStatsDelta.ᏑlargeFree), (int64)size);
            Ꮡmemstats.of(mstats.ᏑheapStats).release();
            // Count the free in the inconsistent, internal stats.
            ᏑgcController.of(gcControllerState.ᏑtotalFree).Add((int64)size);
            // NOTE(rsc,dvyukov): The original implementation of efence
            // in CL 22060046 used sysFree instead of sysFault, so that
            // the operating system would eventually give the memory
            // back to us again, so that an efence program could run
            // longer without running out of memory. Unfortunately,
            // calling sysFree here without any kind of adjustment of the
            // heap data structures means that when the memory does
            // come back to us, we have the wrong metadata for it, either in
            // the mspan structures or in the garbage collection bitmap.
            // Using sysFault here means that the program will run out of
            // memory fairly quickly in efence mode, but at least it won't
            // have mysterious crashes due to confused memory reuse.
            // It should be possible to switch back to sysFree if we also
            // implement and then call some kind of mheap.deleteSpan.
            if (debug.efence > 0){
                s.Value.limit = 0; // prevent mlookup from finding this span
                sysFault((@unsafe.Pointer)s.@base(), size);
            } else {
                Ꮡmheap_.freeSpan(s);
            }
            return true;
        }
        // Add a large span directly onto the full+swept list.
        mheap_.central[spc].mcentral.fullSwept(sweepgen).push(s);
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string foundPointerToFreeObjectˢ = "found pointer to free object"u8;

// reportZombies reports any marked but free objects in s and throws.
//
// This generally means one of the following:
//
// 1. User code converted a pointer to a uintptr and then back
// unsafely, and a GC ran while the uintptr was the only reference to
// an object.
//
// 2. User code (or a compiler bug) constructed a bad pointer that
// points to a free slot, often a past-the-end pointer.
//
// 3. The GC two cycles ago missed a pointer and freed a live object,
// but it was still live in the last cycle, so this GC cycle found a
// pointer to that object and marked it.
internal static void reportZombies(this ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    printlock();
    print((@string)"runtime: marked free object in span "u8, Ꮡs.OrTypedNil(), (@string)", elemsize="u8, s.elemsize, (@string)" freeindex="u8, s.freeindex, (@string)" (bad use of unsafe.Pointer or having race conditions? try -d=checkptr or -race)\n"u8);
    var mbits = s.markBitsForBase();
    var abits = s.allocBitsForIndex(0);
    for (var i = (uintptr)0; i < (uintptr)s.nelems; i++) {
        var addr = s.@base() + i * s.elemsize;
        print(((Δhex)(uint64)addr));
        var alloc = i < (uintptr)s.freeindex || abits.isMarked();
        if (alloc){
            print((@string)" alloc"u8);
        } else {
            print((@string)" free "u8);
        }
        if (mbits.isMarked()){
            print((@string)" marked  "u8);
        } else {
            print((@string)" unmarked"u8);
        }
        var zombie = mbits.isMarked() && !alloc;
        if (zombie) {
            print((@string)" zombie"u8);
        }
        print((@string)"\n"u8);
        if (zombie) {
            var length = s.elemsize;
            if (length > 1024) {
                length = 1024;
            }
            hexdumpWords(addr, addr + length, default!);
        }
        mbits.advance();
        abits.advance();
    }
    @throw(foundPointerToFreeObjectˢ);
}

// deductSweepCredit deducts sweep credit for allocating a span of
// size spanBytes. This must be performed *before* the span is
// allocated to ensure the system has enough credit. If necessary, it
// performs sweeping to prevent going in to debt. If the caller will
// also sweep pages (e.g., for a large allocation), it can pass a
// non-zero callerSweepPages to leave that many pages unswept.
//
// deductSweepCredit makes a worst-case assumption that all spanBytes
// bytes of the ultimately allocated span will be available for object
// allocation.
//
// deductSweepCredit is the core of the "proportional sweep" system.
// It uses statistics gathered by the garbage collector to perform
// enough sweeping so that all pages are swept during the concurrent
// sweep phase between GC cycles.
//
// mheap_ must NOT be locked.
internal static void deductSweepCredit(uintptr spanBytes, uintptr callerSweepPages) {
    if (mheap_.sweepPagesPerByte == 0D) {
        // Proportional sweep is done or disabled.
        return;
    }
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepStart();
        traceRelease(Δtrace);
    }
    // Fix debt if necessary.
retry:
    var sweptBasis = Ꮡmheap_.of(mheap.ᏑpagesSweptBasis).Load();
    var live = ᏑgcController.of(gcControllerState.ᏑheapLive).Load();
    var liveBasis = mheap_.sweepHeapLiveBasis;
    var newHeapLive = spanBytes;
    if (liveBasis < live) {
        // Only do this subtraction when we don't overflow. Otherwise, pagesTarget
        // might be computed as something really huge, causing us to get stuck
        // sweeping here until the next mark phase.
        //
        // Overflow can happen here if gcPaceSweeper is called concurrently with
        // sweeping (i.e. not during a STW, like it usually is) because this code
        // is intentionally racy. A concurrent call to gcPaceSweeper can happen
        // if a GC tuning parameter is modified and we read an older value of
        // heapLive than what was used to set the basis.
        //
        // This state should be transient, so it's fine to just let newHeapLive
        // be a relatively small number. We'll probably just skip this attempt to
        // sweep.
        //
        // See issue #57523.
        newHeapLive += (uintptr)(live - liveBasis);
    }
    var pagesTarget = (int64)(mheap_.sweepPagesPerByte * (float64)newHeapLive) - (int64)callerSweepPages;
    while (pagesTarget > (int64)(Ꮡmheap_.of(mheap.ᏑpagesSwept).Load() - sweptBasis)) {
        if (sweepone() == ~(uintptr)0) {
            mheap_.sweepPagesPerByte = 0D;
            break;
        }
        if (Ꮡmheap_.of(mheap.ᏑpagesSweptBasis).Load() != sweptBasis) {
            // Sweep pacing changed. Recompute debt.
            goto retry;
        }
    }
    Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepDone();
        traceRelease(Δtrace);
    }
}

// clobberfree sets the memory content at x to bad content, for debugging
// purposes.
internal static void clobberfree(@unsafe.Pointer x, uintptr size) {
    // size (span.elemsize) is always a multiple of 4.
    for (var i = (uintptr)0; i < size; i += 4) {
        ((ж<uint32>)(uintptr)(add(x, i))).Value = 0xdeadbeefU;
    }
}

// gcPaceSweeper updates the sweeper's pacing parameters.
//
// Must be called whenever the GC's pacing is updated.
//
// The world must be stopped, or mheap_.lock must be held.
internal static void gcPaceSweeper(uint64 trigger) {
    assertWorldStoppedOrLockHeld(ref mheap_.@lock);
    // Update sweep pacing.
    if (isSweepDone()){
        mheap_.sweepPagesPerByte = 0D;
    } else {
        // Concurrent sweep needs to sweep all of the in-use
        // pages by the time the allocated heap reaches the GC
        // trigger. Compute the ratio of in-use pages to sweep
        // per byte allocated, accounting for the fact that
        // some might already be swept.
        var heapLiveBasis = ᏑgcController.of(gcControllerState.ᏑheapLive).Load();
        var heapDistance = (int64)trigger - (int64)heapLiveBasis;
        // Add a little margin so rounding errors and
        // concurrent sweep are less likely to leave pages
        // unswept when GC starts.
        heapDistance -= 1024 * 1024;
        if (heapDistance < _PageSize) {
            // Avoid setting the sweep ratio extremely high
            heapDistance = _PageSize;
        }
        var pagesSwept = Ꮡmheap_.of(mheap.ᏑpagesSwept).Load();
        var pagesInUse = Ꮡmheap_.of(mheap.ᏑpagesInUse).Load();
        var sweepDistancePages = (int64)pagesInUse - (int64)pagesSwept;
        if (sweepDistancePages <= 0){
            mheap_.sweepPagesPerByte = 0D;
        } else {
            mheap_.sweepPagesPerByte = (float64)sweepDistancePages / (float64)heapDistance;
            mheap_.sweepHeapLiveBasis = heapLiveBasis;
            // Write pagesSweptBasis last, since this
            // signals concurrent sweeps to recompute
            // their debt.
            Ꮡmheap_.of(mheap.ᏑpagesSweptBasis).Store(pagesSwept);
        }
    }
}

} // end runtime_package
