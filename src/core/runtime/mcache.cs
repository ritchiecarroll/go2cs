// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// Per-thread (in Go, per-P) cache for small objects.
// This includes a small object cache and local allocation stats.
// No locking needed because it is per-thread (per-P).
//
// mcaches are allocated from non-GC'd memory, so any heap pointers
// must be specially handled.
partial struct mcache {
    internal sys.NotInHeap _;
    // The following members are accessed on every malloc,
    // so they are grouped here for better caching.
    internal int64 nextSample;   // trigger heap sample after allocating this many bytes
    internal nint memProfRate;    // cached mem profile rate, used to detect changes
    internal uintptr scanAlloc; // bytes of scannable heap allocated
// Allocator cache for tiny objects w/o pointers.
// See "Tiny allocator" comment in malloc.go.

    // tiny points to the beginning of the current tiny block, or
    // nil if there is no current tiny block.
    //
    // tiny is a heap pointer. Since mcache is in non-GC'd memory,
    // we handle it by clearing it in releaseAll during mark
    // termination.
    //
    // tinyAllocs is the number of tiny allocations performed
    // by the P that owns this mcache.
    internal uintptr tiny;
    internal uintptr tinyoffset;
    internal uintptr tinyAllocs;
// The rest is not accessed on every malloc.
    internal array<ж<mspan>> alloc = new(numSpanClasses); // spans to allocate from, indexed by spanClass
    internal array<stackfreelist> stackcache = new(_NumStackOrders);
    // flushGen indicates the sweepgen during which this mcache
    // was last flushed. If flushGen != mheap_.sweepgen, the spans
    // in this mcache are stale and need to the flushed so they
    // can be swept. This is done in acquirep.
    internal atomic.Uint32 flushGen;
}

// A gclink is a node in a linked list of blocks, like mlink,
// but it is opaque to the garbage collector.
// The GC does not trace the pointers during collection,
// and the compiler does not emit write barriers for assignments
// of gclinkptr values. Code should store references to gclinks
// as gclinkptr, not as *gclink.
partial struct gclink {
    internal gclinkptr next;
}

partial struct gclinkptr /*num:uintptr*/;

// ptr returns the *gclink form of p.
// The result should be used for accessing fields, not stored
// in other data structures.
internal static ж<gclink> ptr(this gclinkptr Δp) {
    return (ж<gclink>)(uintptr)(((@unsafe.Pointer)(uintptr)Δp));
}

partial struct stackfreelist {
    internal gclinkptr list; // linked list of free stacks
    internal uintptr size;   // total size of stacks in list
}

// dummy mspan that contains no free objects.
internal static ж<mspan> Ꮡemptymspan = new StandardBox<mspan>(new mspan());
internal static ref mspan emptymspan => ref Ꮡemptymspan.Value;

internal static ж<mcache> allocmcache() {
    ref var c = ref heap<ж<mcache>>(out var Ꮡc);
    systemstack(() => {
        @lock(Ꮡmheap_.of(mheap.Ꮡlock));
        Ꮡc.ValueSlot = (ж<mcache>)(uintptr)(Ꮡmheap_.of(mheap.Ꮡcachealloc).alloc());
        Ꮡc.ValueSlot.of(mcache.ᏑflushGen).Store(mheap_.sweepgen);
        unlock(Ꮡmheap_.of(mheap.Ꮡlock));
    });
    foreach (var (i, _) in (~c).alloc) {
        c.Value.alloc[i] = Ꮡemptymspan;
    }
    c.Value.nextSample = nextSample();
    return c;
}

// freemcache releases resources associated with this
// mcache and puts the object onto a free list.
//
// In some cases there is no way to simply release
// resources, such as statistics, so donate them to
// a different mcache (the recipient).
internal static void freemcache(ж<mcache> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    systemstack(() => {
        Ꮡc.Value.releaseAll();
        stackcache_clear(ref (Ꮡc).DerefOrNull());
        // NOTE(rsc,rlh): If gcworkbuffree comes back, we need to coordinate
        // with the stealing of gcworkbufs during garbage collection to avoid
        // a race where the workbuf is double-freed.
        // gcworkbuffree(c.gcworkbuf)
        @lock(Ꮡmheap_.of(mheap.Ꮡlock));
        Ꮡmheap_.of(mheap.Ꮡcachealloc).free(@unsafe.Pointer.FromPinnedBox(Ꮡc));
        unlock(Ꮡmheap_.of(mheap.Ꮡlock));
    });
}

// getMCache is a convenience function which tries to obtain an mcache.
//
// Returns nil if we're not bootstrapping or we don't have a P. The caller's
// P must not change, so we must be in a non-preemptible state.
internal static ж<mcache> getMCache(ref m mp) {
    // Grab the mcache, since that's where stats live.
    var pp = mp.p.ptr();
    ж<mcache> c = default!;
    if (pp == nil){
        // We will be called without a P while bootstrapping,
        // in which case we use mcache0, which is set in mallocinit.
        // mcache0 is cleared when bootstrapping is complete,
        // by procresize.
        c = mcache0;
    } else {
        c = pp.Value.mcache;
    }
    return c;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string refillOfSpanWithFreeˢ = "refill of span with free space remaining"u8;
internal static readonly @string badSweepgenInRefillˢ = "bad sweepgen in refill"u8;
internal static readonly @string spanHasNoFreeSpaceˢ = "span has no free space"u8;

// refill acquires a new span of span class spc for c. This span will
// have at least one free object. The current span in c must be full.
//
// Must run in a non-preemptible context since otherwise the owner of
// c could change.
internal static void refill(this ref mcache c, spanClass spc) {
    // Return the current cached span to the central lists.
    var s = c.alloc[spc];
    if ((~s).allocCount != (~s).nelems) {
        @throw(refillOfSpanWithFreeˢ);
    }
    if (s != Ꮡemptymspan) {
        // Mark this span as no longer cached.
        if ((~s).sweepgen != mheap_.sweepgen + 3) {
            @throw(badSweepgenInRefillˢ);
        }
        mheap_.central[spc].mcentral.uncacheSpan(s);
        // Count up how many slots were used and record it.
        var stats = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
        var slotsUsed = (int64)(~s).allocCount - (int64)(~s).allocCountBeforeCache;
        atomic.Xadd64(stats.at(heapStatsDelta.ᏑsmallAllocCount, (nint)(spc.sizeclass())), slotsUsed);
        // Flush tinyAllocs.
        if (spc == tinySpanClass) {
            atomic.Xadd64(stats.of(heapStatsDelta.ᏑtinyAllocCount), (int64)c.tinyAllocs);
            c.tinyAllocs = 0;
        }
        Ꮡmemstats.of(mstats.ᏑheapStats).release();
        // Count the allocs in inconsistent, internal stats.
        var bytesAllocated = slotsUsed * (int64)(~s).elemsize;
        ᏑgcController.of(gcControllerState.ᏑtotalAlloc).Add(bytesAllocated);
        // Clear the second allocCount just to be safe.
        s.Value.allocCountBeforeCache = 0;
    }
    // Get a new cached span from the central lists.
    s = mheap_.central[spc].mcentral.cacheSpan();
    if (s == nil) {
        @throw(outOfMemoryˢ);
    }
    if ((~s).allocCount == (~s).nelems) {
        @throw(spanHasNoFreeSpaceˢ);
    }
    // Indicate that this span is cached and prevent asynchronous
    // sweeping in the next sweep phase.
    s.Value.sweepgen = mheap_.sweepgen + 3;
    // Store the current alloc count for accounting later.
    s.Value.allocCountBeforeCache = s.Value.allocCount;
    // Update heapLive and flush scanAlloc.
    //
    // We have not yet allocated anything new into the span, but we
    // assume that all of its slots will get used, so this makes
    // heapLive an overestimate.
    //
    // When the span gets uncached, we'll fix up this overestimate
    // if necessary (see releaseAll).
    //
    // We pick an overestimate here because an underestimate leads
    // the pacer to believe that it's in better shape than it is,
    // which appears to lead to more memory used. See #53738 for
    // more details.
    var usedBytes = (uintptr)(~s).allocCount * (~s).elemsize;
    ᏑgcController.update((int64)((~s).npages * (uintptr)pageSize) - (int64)usedBytes, (int64)c.scanAlloc);
    c.scanAlloc = 0;
    c.alloc[spc] = s;
}

// allocLarge allocates a span for a large object.
internal static ж<mspan> allocLarge(this ref mcache c, uintptr size, bool noscan) {
    if (size + (uintptr)_PageSize < size) {
        @throw(outOfMemoryˢ);
    }
    var npages = (size >> (int)(_PageShift));
    if ((uintptr)(size & (uintptr)_PageMask) != 0) {
        npages++;
    }
    // Deduct credit for this span allocation and sweep if
    // necessary. mHeap_Alloc will also sweep npages, so this only
    // pays the debt down to npage pages.
    deductSweepCredit(npages * (uintptr)_PageSize, npages);
    var spc = makeSpanClass(0, noscan);
    var s = Ꮡmheap_.alloc(npages, spc);
    if (s == nil) {
        @throw(outOfMemoryˢ);
    }
    // Count the alloc in consistent, external stats.
    var stats = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
    atomic.Xadd64(stats.of(heapStatsDelta.ᏑlargeAlloc), (int64)(npages * (uintptr)pageSize));
    atomic.Xadd64(stats.of(heapStatsDelta.ᏑlargeAllocCount), 1);
    Ꮡmemstats.of(mstats.ᏑheapStats).release();
    // Count the alloc in inconsistent, internal stats.
    ᏑgcController.of(gcControllerState.ᏑtotalAlloc).Add((int64)(npages * (uintptr)pageSize));
    // Update heapLive.
    ᏑgcController.update((int64)((~s).npages * (uintptr)pageSize), 0);
    // Put the large span in the mcentral swept list so that it's
    // visible to the background sweeper.
    mheap_.central[spc].mcentral.fullSwept(mheap_.sweepgen).push(s);
    // Adjust s.limit down to the object-containing part of the span.
    //
    // This is just to create a slightly tighter bound on the limit.
    // It's totally OK if the garbage collector, in particular
    // conservative scanning, can temporarily observes an inflated
    // limit. It will simply mark the whole object or just skip it
    // since we're in the mark phase anyway.
    s.Value.limit = s.@base() + size;
    s.initHeapBits();
    return s;
}

internal static void releaseAll(this ref mcache c) {
    // Take this opportunity to flush scanAlloc.
    var scanAlloc = (int64)c.scanAlloc;
    c.scanAlloc = 0;
    var sg = mheap_.sweepgen;
    var dHeapLive = (int64)0;
    foreach (var (i, _) in c.alloc) {
        var s = c.alloc[i];
        if (s != Ꮡemptymspan) {
            var slotsUsed = (int64)(~s).allocCount - (int64)(~s).allocCountBeforeCache;
            s.Value.allocCountBeforeCache = 0;
            // Adjust smallAllocCount for whatever was allocated.
            var statsΔ1 = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
            atomic.Xadd64(statsΔ1.at(heapStatsDelta.ᏑsmallAllocCount, (nint)(((spanClass)(uint8)i).sizeclass())), slotsUsed);
            Ꮡmemstats.of(mstats.ᏑheapStats).release();
            // Adjust the actual allocs in inconsistent, internal stats.
            // We assumed earlier that the full span gets allocated.
            ᏑgcController.of(gcControllerState.ᏑtotalAlloc).Add(slotsUsed * (int64)(~s).elemsize);
            if ((~s).sweepgen != sg + 1) {
                // refill conservatively counted unallocated slots in gcController.heapLive.
                // Undo this.
                //
                // If this span was cached before sweep, then gcController.heapLive was totally
                // recomputed since caching this span, so we don't do this for stale spans.
                dHeapLive -= (int64)((uint16)((~s).nelems - (~s).allocCount)) * (int64)(~s).elemsize;
            }
            // Release the span to the mcentral.
            mheap_.central[i].mcentral.uncacheSpan(s);
            c.alloc[i] = Ꮡemptymspan;
        }
    }
    // Clear tinyalloc pool.
    c.tiny = 0;
    c.tinyoffset = 0;
    // Flush tinyAllocs.
    var stats = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
    atomic.Xadd64(stats.of(heapStatsDelta.ᏑtinyAllocCount), (int64)c.tinyAllocs);
    c.tinyAllocs = 0;
    Ꮡmemstats.of(mstats.ᏑheapStats).release();
    // Update heapLive and heapScan.
    ᏑgcController.update(dHeapLive, scanAlloc);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badFlushGenˢ = "bad flushGen"u8;

// prepareForSweep flushes c if the system has entered a new sweep phase
// since c was populated. This must happen between the sweep phase
// starting and the first allocation from c.
internal static void prepareForSweep(this ж<mcache> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    // Alternatively, instead of making sure we do this on every P
    // between starting the world and allocating on that P, we
    // could leave allocate-black on, allow allocation to continue
    // as usual, use a ragged barrier at the beginning of sweep to
    // ensure all cached spans are swept, and then disable
    // allocate-black. However, with this approach it's difficult
    // to avoid spilling mark bits into the *next* GC cycle.
    var sg = mheap_.sweepgen;
    var flushGen = Ꮡc.of(mcache.ᏑflushGen).Load();
    if (flushGen == sg){
        return;
    } else 
    if (flushGen != sg - 2) {
        println((@string)"bad flushGen"u8, flushGen, (@string)"in prepareForSweep; sweepgen"u8, sg);
        @throw(badFlushGenˢ);
    }
    c.releaseAll();
    stackcache_clear(ref (Ꮡc).DerefOrNull());
    Ꮡc.of(mcache.ᏑflushGen).Store(mheap_.sweepgen); // Synchronizes with gcStart
}

} // end runtime_package
