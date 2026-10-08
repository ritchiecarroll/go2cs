// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Central free lists.
//
// See malloc.go for an overview.
//
// The mcentral doesn't actually contain the list of free objects; the mspan does.
// Each mcentral is two lists of mspans: those with free objects (c->nonempty)
// and those that are completely allocated (c->empty).
namespace go;

using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @internal.runtime;

partial class runtime_package {

// Central list of free objects of a given size.
partial struct mcentral {
    internal sys.NotInHeap _;
    internal spanClass spanclass;
    // partial and full contain two mspan sets: one of swept in-use
    // spans, and one of unswept in-use spans. These two trade
    // roles on each GC cycle. The unswept set is drained either by
    // allocation or by the background sweeper in every GC cycle,
    // so only two roles are necessary.
    //
    // sweepgen is increased by 2 on each GC cycle, so the swept
    // spans are in partial[sweepgen/2%2] and the unswept spans are in
    // partial[1-sweepgen/2%2]. Sweeping pops spans from the
    // unswept set and pushes spans that are still in-use on the
    // swept set. Likewise, allocating an in-use span pushes it
    // on the swept set.
    //
    // Some parts of the sweeper can sweep arbitrary spans, and hence
    // can't remove them from the unswept set, but will add the span
    // to the appropriate swept list. As a result, the parts of the
    // sweeper and mcentral that do consume from the unswept list may
    // encounter swept spans, and these should be ignored.
    internal array<spanSet> partial = new(2, () => new()); // list of spans with a free object
    internal array<spanSet> full = new(2, () => new()); // list of spans with no free objects
}

// Initialize a single central free list.
internal static void init(this ref mcentral c, spanClass spc) {
    c.spanclass = spc;
    lockInit(Ꮡ(c.partial, 0).of(spanSet.ᏑspineLock), lockRankSpanSetSpine);
    lockInit(Ꮡ(c.partial, 1).of(spanSet.ᏑspineLock), lockRankSpanSetSpine);
    lockInit(Ꮡ(c.full, 0).of(spanSet.ᏑspineLock), lockRankSpanSetSpine);
    lockInit(Ꮡ(c.full, 1).of(spanSet.ᏑspineLock), lockRankSpanSetSpine);
}

// partialUnswept returns the spanSet which holds partially-filled
// unswept spans for this sweepgen.
internal static ж<spanSet> partialUnswept(this ref mcentral c, uint32 sweepgen) {
    return Ꮡ(c.partial, 1 - sweepgen / 2 % 2);
}

// partialSwept returns the spanSet which holds partially-filled
// swept spans for this sweepgen.
internal static ж<spanSet> partialSwept(this ref mcentral c, uint32 sweepgen) {
    return Ꮡ(c.partial, sweepgen / 2 % 2);
}

// fullUnswept returns the spanSet which holds unswept spans without any
// free slots for this sweepgen.
internal static ж<spanSet> fullUnswept(this ref mcentral c, uint32 sweepgen) {
    return Ꮡ(c.full, 1 - sweepgen / 2 % 2);
}

// fullSwept returns the spanSet which holds swept spans without any
// free slots for this sweepgen.
internal static ж<spanSet> fullSwept(this ref mcentral c, uint32 sweepgen) {
    return Ꮡ(c.full, sweepgen / 2 % 2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string spanHasNoFreeObjectsˢ = "span has no free objects"u8;

// Allocate a span to use in an mcache.
internal static ж<mspan> cacheSpan(this ref mcentral c) {
    // Deduct credit for this span allocation and sweep if necessary.
    var spanBytes = (uintptr)class_to_allocnpages[c.spanclass.sizeclass()] * (uintptr)_PageSize;
    deductSweepCredit(spanBytes, 0);
    var traceDone = false;
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepStart();
        traceRelease(Δtrace);
    }
    // If we sweep spanBudget spans without finding any free
    // space, just allocate a fresh span. This limits the amount
    // of time we can spend trying to find free space and
    // amortizes the cost of small object sweeping over the
    // benefit of having a full free span to allocate from. By
    // setting this to 100, we limit the space overhead to 1%.
    //
    // TODO(austin,mknyszek): This still has bad worst-case
    // throughput. For example, this could find just one free slot
    // on the 100th swept span. That limits allocation latency, but
    // still has very poor throughput. We could instead keep a
    // running free-to-used budget and switch to fresh span
    // allocation if the budget runs low.
    nint spanBudget = 100;
    ж<mspan> s = default!;
    sweepLocker sl = default!;
    // Try partial swept spans first.
    var sg = mheap_.sweepgen;
    {
        s = c.partialSwept(sg).pop(); if (s != nil) {
            goto havespan;
        }
    }
    sl = ᏑΔsweep.of(sweepdata.Ꮡactive).begin();
    if (sl.valid) {
        // Now try partial unswept spans.
        for (; spanBudget >= 0; spanBudget--) {
            s = c.partialUnswept(sg).pop();
            if (s == nil) {
                break;
            }
            {
                var (sΔ1, ok) = sl.tryAcquire(s); if (ok) {
                    // We got ownership of the span, so let's sweep it and use it.
                    sΔ1.sweep(true);
                    ᏑΔsweep.of(sweepdata.Ꮡactive).end(sl);
                    goto havespan;
                }
            }
        }
        // We failed to get ownership of the span, which means it's being or
        // has been swept by an asynchronous sweeper that just couldn't remove it
        // from the unswept list. That sweeper took ownership of the span and
        // responsibility for either freeing it to the heap or putting it on the
        // right swept list. Either way, we should just ignore it (and it's unsafe
        // for us to do anything else).
        // Now try full unswept spans, sweeping them and putting them into the
        // right list if we fail to get a span.
        for (; spanBudget >= 0; spanBudget--) {
            s = c.fullUnswept(sg).pop();
            if (s == nil) {
                break;
            }
            {
                var (sΔ2, ok) = sl.tryAcquire(s); if (ok) {
                    // We got ownership of the span, so let's sweep it.
                    sΔ2.sweep(true);
                    // Check if there's any free space.
                    var freeIndex = sΔ2.nextFreeIndex();
                    if (freeIndex != sΔ2.nelems) {
                        sΔ2.freeindex = freeIndex;
                        ᏑΔsweep.of(sweepdata.Ꮡactive).end(sl);
                        goto havespan;
                    }
                    // Add it to the swept list, because sweeping didn't give us any free space.
                    c.fullSwept(sg).push(sΔ2.mspan);
                }
            }
        }
        // See comment for partial unswept spans.
        ᏑΔsweep.of(sweepdata.Ꮡactive).end(sl);
    }
    Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepDone();
        traceDone = true;
        traceRelease(Δtrace);
    }
    // We failed to get a span from the mcentral so get one from mheap.
    s = c.grow();
    if (s == nil) {
        return default!;
    }
    // At this point s is a span that should have free slots.
havespan:
    if (!traceDone) {
        var traceΔ1 = traceAcquire();
        if (traceΔ1.ok()) {
            traceΔ1.GCSweepDone();
            traceRelease(traceΔ1);
        }
    }
    nint n = (nint)(~s).nelems - (nint)(~s).allocCount;
    if (n == 0 || (~s).freeindex == (~s).nelems || (~s).allocCount == (~s).nelems) {
        @throw(spanHasNoFreeObjectsˢ);
    }
    var freeByteBase = (uint16)((~s).freeindex & ~(64 - 1));
    var whichByte = (uint16)(freeByteBase / 8);
    // Init alloc bits cache.
    s.refillAllocCache(whichByte);
    // Adjust the allocCache so that s.freeindex corresponds to the low bit in
    // s.allocCache.
    s.Value.allocCache >>= (int)((~s).freeindex % 64);
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string uncachingSpanButSˢ = "uncaching span but s.allocCount == 0"u8;

// Return span from an mcache.
//
// s must have a span class corresponding to this
// mcentral and it must not be empty.
internal static void uncacheSpan(this ref mcentral c, ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (s.allocCount == 0) {
        @throw(uncachingSpanButSˢ);
    }
    var sg = mheap_.sweepgen;
    var stale = s.sweepgen == sg + 1;
    // Fix up sweepgen.
    if (stale){
        // Span was cached before sweep began. It's our
        // responsibility to sweep it.
        //
        // Set sweepgen to indicate it's not cached but needs
        // sweeping and can't be allocated from. sweep will
        // set s.sweepgen to indicate s is swept.
        atomic.Store(Ꮡs.of(mspan.Ꮡsweepgen), sg - 1);
    } else {
        // Indicate that s is no longer cached.
        atomic.Store(Ꮡs.of(mspan.Ꮡsweepgen), sg);
    }
    // Put the span in the appropriate place.
    if (stale){
        // It's stale, so just sweep it. Sweeping will put it on
        // the right list.
        //
        // We don't use a sweepLocker here. Stale cached spans
        // aren't in the global sweep lists, so mark termination
        // itself holds up sweep completion until all mcaches
        // have been swept.
        var ss = new sweepLocked(Ꮡs);
        ss.sweep(false);
    } else {
        if ((nint)s.nelems - (nint)s.allocCount > 0){
            // Put it back on the partial swept list.
            c.partialSwept(sg).push(Ꮡs);
        } else {
            // There's no free space and it's not stale, so put it on the
            // full swept list.
            c.fullSwept(sg).push(Ꮡs);
        }
    }
}

// grow allocates a new empty span from the heap and initializes it for c's size class.
internal static ж<mspan> grow(this ref mcentral c) {
    var npages = (uintptr)class_to_allocnpages[c.spanclass.sizeclass()];
    var s = Ꮡmheap_.alloc(npages, c.spanclass);
    if (s == nil) {
        return default!;
    }
    s.initHeapBits();
    return s;
}

} // end runtime_package
