// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Page allocator.
//
// The page allocator manages mapped pages (defined by pageSize, NOT
// physPageSize) for allocation and re-use. It is embedded into mheap.
//
// Pages are managed using a bitmap that is sharded into chunks.
// In the bitmap, 1 means in-use, and 0 means free. The bitmap spans the
// process's address space. Chunks are managed in a sparse-array-style structure
// similar to mheap.arenas, since the bitmap may be large on some systems.
//
// The bitmap is efficiently searched by using a radix tree in combination
// with fast bit-wise intrinsics. Allocation is performed using an address-ordered
// first-fit approach.
//
// Each entry in the radix tree is a summary that describes three properties of
// a particular region of the address space: the number of contiguous free pages
// at the start and end of the region it represents, and the maximum number of
// contiguous free pages found anywhere in that region.
//
// Each level of the radix tree is stored as one contiguous array, which represents
// a different granularity of subdivision of the processes' address space. Thus, this
// radix tree is actually implicit in these large arrays, as opposed to having explicit
// dynamically-allocated pointer-based node structures. Naturally, these arrays may be
// quite large for system with large address spaces, so in these cases they are mapped
// into memory as needed. The leaf summaries of the tree correspond to a bitmap chunk.
//
// The root level (referred to as L0 and index 0 in pageAlloc.summary) has each
// summary represent the largest section of address space (16 GiB on 64-bit systems),
// with each subsequent level representing successively smaller subsections until we
// reach the finest granularity at the leaves, a chunk.
//
// More specifically, each summary in each level (except for leaf summaries)
// represents some number of entries in the following level. For example, each
// summary in the root level may represent a 16 GiB region of address space,
// and in the next level there could be 8 corresponding entries which represent 2
// GiB subsections of that 16 GiB region, each of which could correspond to 8
// entries in the next level which each represent 256 MiB regions, and so on.
//
// Thus, this design only scales to heaps so large, but can always be extended to
// larger heaps by simply adding levels to the radix tree, which mostly costs
// additional virtual address space. The choice of managing large arrays also means
// that a large amount of virtual address space may be reserved by the runtime.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

internal static UntypedInt pallocChunkPages => /* 1 << logPallocChunkPages */ 512;
internal static UntypedInt pallocChunkBytes => /* pallocChunkPages * pageSize */ 4194304;
internal static UntypedInt logPallocChunkPages => 9;
internal static UntypedInt logPallocChunkBytes => /* logPallocChunkPages + pageShift */ 22;
internal static UntypedInt summaryLevelBits => 3;
internal static UntypedInt summaryL0Bits => /* heapAddrBits - logPallocChunkBytes - (summaryLevels-1)*summaryLevelBits */ 14;
internal static UntypedInt pallocChunksL2Bits => /* heapAddrBits - logPallocChunkBytes - pallocChunksL1Bits */ 13;
internal static UntypedInt pallocChunksL1Shift => /* pallocChunksL2Bits */ 13;

// maxSearchAddr returns the maximum searchAddr value, which indicates
// that the heap has no free space.
//
// This function exists just to make it clear that this is the maximum address
// for the page allocator's search space. See maxOffAddr for details.
//
// It's a function (rather than a variable) because it needs to be
// usable before package runtime's dynamic initialization is complete.
// See #51913 for details.
internal static offAddr maxSearchAddr() {
    return maxOffAddr;
}

[GoType("num:nuint")] partial struct chunkIdx;

// chunkIndex returns the global index of the palloc chunk containing the
// pointer p.
internal static chunkIdx chunkIndex(uintptr Δp) {
    return ((chunkIdx)(nuint)((Δp - (uintptr)arenaBaseOffset) / (uintptr)pallocChunkBytes));
}

// chunkBase returns the base address of the palloc chunk at index ci.
internal static uintptr chunkBase(chunkIdx ci) {
    return (uintptr)(nuint)ci * (uintptr)pallocChunkBytes + (uintptr)arenaBaseOffset;
}

// chunkPageIndex computes the index of the page that contains p,
// relative to the chunk which contains p.
internal static nuint chunkPageIndex(uintptr Δp) {
    return (nuint)(Δp % (uintptr)pallocChunkBytes / (uintptr)pageSize);
}

// l1 returns the index into the first level of (*pageAlloc).chunks.
internal static nuint l1(this chunkIdx i) {
    if (pallocChunksL1Bits == 0){
        // Let the compiler optimize this away if there's no
        // L1 map.
        return 0;
    } else {
        return ((nuint)i >> (int)(pallocChunksL1Shift));
    }
}

// l2 returns the index into the second level of (*pageAlloc).chunks.
internal static nuint l2(this chunkIdx i) {
    if (pallocChunksL1Bits == 0){
        return (nuint)i;
    } else {
        return (nuint)((nuint)i & (nuint)((1 << (int)(pallocChunksL2Bits)) - 1));
    }
}

// offAddrToLevelIndex converts an address in the offset address space
// to the index into summary[level] containing addr.
internal static nint offAddrToLevelIndex(nint level, offAddr addr) {
    return (nint)((addr.a - (uintptr)arenaBaseOffset).Rsh(levelShift[level]));
}

// levelIndexToOffAddr converts an index into summary[level] into
// the corresponding address in the offset address space.
internal static offAddr levelIndexToOffAddr(nint level, nint idx) {
    return new offAddr((((uintptr)idx).Lsh(levelShift[level])) + (uintptr)arenaBaseOffset);
}

// addrsToSummaryRange converts base and limit pointers into a range
// of entries for the given summary level.
//
// The returned range is inclusive on the lower bound and exclusive on
// the upper bound.
internal static (nint lo, nint hi) addrsToSummaryRange(nint level, uintptr @base, uintptr limit) {
    nint lo = default!;
    nint hi = default!;

    // This is slightly more nuanced than just a shift for the exclusive
    // upper-bound. Note that the exclusive upper bound may be within a
    // summary at this level, meaning if we just do the obvious computation
    // hi will end up being an inclusive upper bound. Unfortunately, just
    // adding 1 to that is too broad since we might be on the very edge
    // of a summary's max page count boundary for this level
    // (1 << levelLogPages[level]). So, make limit an inclusive upper bound
    // then shift, then add 1, so we get an exclusive upper bound at the end.
    lo = (nint)((@base - (uintptr)arenaBaseOffset).Rsh(levelShift[level]));
    hi = (nint)(((limit - 1) - (uintptr)arenaBaseOffset).Rsh(levelShift[level])) + 1;
    return (lo, hi);
}

// blockAlignSummaryRange aligns indices into the given level to that
// level's block width (1 << levelBits[level]). It assumes lo is inclusive
// and hi is exclusive, and so aligns them down and up respectively.
internal static (nint, nint) blockAlignSummaryRange(nint level, nint lo, nint hi) {
    var e = ((uintptr)1).Lsh(levelBits[level]);
    return ((nint)alignDown((uintptr)lo, e), (nint)alignUp((uintptr)hi, e));
}

[GoType("dyn")] partial struct pageAlloc_scav {
    // index is an efficient index of chunks that have pages available to
    // scavenge.
    internal scavengeIndex index;
    // releasedBg is the amount of memory released in the background this
    // scavenge cycle.
    internal atomic.Uintptr releasedBg;
    // releasedEager is the amount of memory released eagerly this scavenge
    // cycle.
    internal atomic.Uintptr releasedEager;
}

[GoType] partial struct pageAlloc {
    // Radix tree of summaries.
    //
    // Each slice's cap represents the whole memory reservation.
    // Each slice's len reflects the allocator's maximum known
    // mapped heap address for that level.
    //
    // The backing store of each summary level is reserved in init
    // and may or may not be committed in grow (small address spaces
    // may commit all the memory in init).
    //
    // The purpose of keeping len <= cap is to enforce bounds checks
    // on the top end of the slice so that instead of an unknown
    // runtime segmentation fault, we get a much friendlier out-of-bounds
    // error.
    //
    // To iterate over a summary level, use inUse to determine which ranges
    // are currently available. Otherwise one might try to access
    // memory which is only Reserved which may result in a hard fault.
    //
    // We may still get segmentation faults < len since some of that
    // memory may not be committed yet.
    internal array<slice<pallocSum>> summary = new(summaryLevels);
    // chunks is a slice of bitmap chunks.
    //
    // The total size of chunks is quite large on most 64-bit platforms
    // (O(GiB) or more) if flattened, so rather than making one large mapping
    // (which has problems on some platforms, even when PROT_NONE) we use a
    // two-level sparse array approach similar to the arena index in mheap.
    //
    // To find the chunk containing a memory address `a`, do:
    //   chunkOf(chunkIndex(a))
    //
    // Below is a table describing the configuration for chunks for various
    // heapAddrBits supported by the runtime.
    //
    // heapAddrBits | L1 Bits | L2 Bits | L2 Entry Size
    // ------------------------------------------------
    // 32           | 0       | 10      | 128 KiB
    // 33 (iOS)     | 0       | 11      | 256 KiB
    // 48           | 13      | 13      | 1 MiB
    //
    // There's no reason to use the L1 part of chunks on 32-bit, the
    // address space is small so the L2 is small. For platforms with a
    // 48-bit address space, we pick the L1 such that the L2 is 1 MiB
    // in size, which is a good balance between low granularity without
    // making the impact on BSS too high (note the L1 is stored directly
    // in pageAlloc).
    //
    // To iterate over the bitmap, use inUse to determine which ranges
    // are currently available. Otherwise one might iterate over unused
    // ranges.
    //
    // Protected by mheapLock.
    //
    // TODO(mknyszek): Consider changing the definition of the bitmap
    // such that 1 means free and 0 means in-use so that summaries and
    // the bitmaps align better on zero-values.
    internal array<ж<array<pallocData>>> chunks = new((1 << (int)(pallocChunksL1Bits)));
    // The address to start an allocation search with. It must never
    // point to any memory that is not contained in inUse, i.e.
    // inUse.contains(searchAddr.addr()) must always be true. The one
    // exception to this rule is that it may take on the value of
    // maxOffAddr to indicate that the heap is exhausted.
    //
    // We guarantee that all valid heap addresses below this value
    // are allocated and not worth searching.
    internal offAddr searchAddr;
    // start and end represent the chunk indices
    // which pageAlloc knows about. It assumes
    // chunks in the range [start, end) are
    // currently ready to use.
    internal chunkIdx start, end;
    // inUse is a slice of ranges of address space which are
    // known by the page allocator to be currently in-use (passed
    // to grow).
    //
    // We care much more about having a contiguous heap in these cases
    // and take additional measures to ensure that, so in nearly all
    // cases this should have just 1 element.
    //
    // All access is protected by the mheapLock.
    internal addrRanges inUse;
    // scav stores the scavenger state.
    internal pageAlloc_scav scav;
    // mheap_.lock. This level of indirection makes it possible
    // to test pageAlloc independently of the runtime allocator.
    internal ж<mutex> mheapLock;
    // sysStat is the runtime memstat to update when new system
    // memory is committed by the pageAlloc for allocation metadata.
    internal ж<sysMemStat> sysStat;
    // summaryMappedReady is the number of bytes mapped in the Ready state
    // in the summary structure. Used only for testing currently.
    //
    // Protected by mheapLock.
    internal uintptr summaryMappedReady;
    // chunkHugePages indicates whether page bitmap chunks should be backed
    // by huge pages.
    internal bool chunkHugePages;
    // Whether or not this struct is being used in tests.
    internal bool test;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootLevelMaxPagesDoesnTˢ = "root level max pages doesn't fit in summary"u8;

internal static void init(this ж<pageAlloc> Ꮡp, ж<mutex> ᏑmheapLock, ж<sysMemStat> ᏑsysStat, bool test) {
    ref var Δp = ref Ꮡp.DerefOrNull();
    ref var mheapLock = ref ᏑmheapLock.DerefOrNull();
    ref var sysStat = ref ᏑsysStat.DerefOrNull();

    if (levelLogPages[0] > logMaxPackedValue) {
        // We can't represent 1<<levelLogPages[0] pages, the maximum number
        // of pages we need to represent at the root level, in a summary, which
        // is a big problem. Throw.
        print((@string)"runtime: root level max pages = "u8, ((nint)1).Lsh(levelLogPages[0]), (@string)"\n"u8);
        print((@string)"runtime: summary max pages = "u8, (nint)(maxPackedValue), (@string)"\n"u8);
        @throw(rootLevelMaxPagesDoesnTˢ);
    }
    Δp.sysStat = ᏑsysStat;
    // Initialize p.inUse.
    Ꮡp.of(pageAlloc.ᏑinUse).init(ᏑsysStat);
    // System-dependent initialization.
    Δp.sysInit(test);
    // Start with the searchAddr in a state indicating there's no free memory.
    Δp.searchAddr = maxSearchAddr();
    // Set the mheapLock.
    Δp.mheapLock = ᏑmheapLock;
    // Initialize the scavenge index.
    Δp.summaryMappedReady += Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).init(test, ᏑsysStat);
    // Set if we're in a test.
    Δp.test = test;
}

// tryChunkOf returns the bitmap data for the given chunk.
//
// Returns nil if the chunk data has not been mapped.
[GoRecv] internal static ж<pallocData> tryChunkOf(this ref pageAlloc Δp, chunkIdx ci) {
    var l2 = Δp.chunks[(nint)(ci.l1())];
    if (l2 == nil) {
        return default!;
    }
    return l2.at<pallocData>((nint)(ci.l2()));
}

// chunkOf returns the chunk at the given chunk index.
//
// The chunk index must be valid or this method may throw.
[GoRecv] internal static ж<pallocData> chunkOf(this ref pageAlloc Δp, chunkIdx ci) {
    return Δp.chunks[(nint)(ci.l1())].at<pallocData>((nint)(ci.l2()));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pageAllocOutOfMemoryˢ = "pageAlloc: out of memory"u8;

// grow sets up the metadata for the address range [base, base+size).
// It may allocate metadata, in which case *p.sysStat will be updated.
//
// p.mheapLock must be held.
internal static void grow(this ж<pageAlloc> Ꮡp, uintptr @base, uintptr size) {
    ref var Δp = ref Ꮡp.DerefOrNull();

    assertLockHeld(Δp.mheapLock);
    // Round up to chunks, since we can't deal with increments smaller
    // than chunks. Also, sysGrow expects aligned values.
    var limit = alignUp(@base + size, pallocChunkBytes);
    @base = alignDown(@base, pallocChunkBytes);
    // Grow the summary levels in a system-dependent manner.
    // We just update a bunch of additional metadata here.
    Ꮡp.sysGrow(@base, limit);
    // Grow the scavenge index.
    Δp.summaryMappedReady += Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).grow(@base, limit, Δp.sysStat);
    // Update p.start and p.end.
    // If no growth happened yet, start == 0. This is generally
    // safe since the zero page is unmapped.
    var firstGrowth = Δp.start == 0;
    chunkIdx start = chunkIndex(@base);
    chunkIdx end = chunkIndex(limit);
    if (firstGrowth || start < Δp.start) {
        Δp.start = start;
    }
    if (end > Δp.end) {
        Δp.end = end;
    }
    // Note that [base, limit) will never overlap with any existing
    // range inUse because grow only ever adds never-used memory
    // regions to the page allocator.
    Ꮡp.of(pageAlloc.ᏑinUse).add(makeAddrRange(@base, limit));
    // A grow operation is a lot like a free operation, so if our
    // chunk ends up below p.searchAddr, update p.searchAddr to the
    // new address, just like in free.
    {
        var b = (new offAddr(@base)); if (b.lessThan(Δp.searchAddr)) {
            Δp.searchAddr = b;
        }
    }
    // Add entries into chunks, which is sparse, if needed. Then,
    // initialize the bitmap.
    //
    // Newly-grown memory is always considered scavenged.
    // Set all the bits in the scavenged bitmaps high.
    for (chunkIdx c = chunkIndex(@base); c < chunkIndex(limit); c++) {
        if (Δp.chunks[(nint)(c.l1())] == nil) {
            // Create the necessary l2 entry.
            uintptr l2Size = /* unsafe.Sizeof(*p.chunks[0]) */ 1048576;
            @unsafe.Pointer r = (uintptr)sysAlloc(l2Size, Δp.sysStat);
            if (r == nil) {
                @throw(pageAllocOutOfMemoryˢ);
            }
            if (!Δp.test) {
                // Make the chunk mapping eligible or ineligible
                // for huge pages, depending on what our current
                // state is.
                if (Δp.chunkHugePages){
                    sysHugePage(r, l2Size);
                } else {
                    sysNoHugePage(r, l2Size);
                }
            }
            // Store the new chunk block but avoid a write barrier.
            // grow is used in call chains that disallow write barriers.
            Ꮡ(Δp.chunks, (int)(c.l1())).Value = NativeArrayPointer<pallocData>((nuint)((uintptr)r), 8192);
        }
        Δp.chunkOf(c).of(pallocData.Ꮡscavenged).setRange(0, pallocChunkPages);
    }
    // Update summaries accordingly. The grow acts like a free, so
    // we need to ensure this newly-free memory is visible in the
    // summaries.
    Δp.update(@base, size / (uintptr)pageSize, true, false);
}

// enableChunkHugePages enables huge pages for the chunk bitmap mappings (disabled by default).
//
// This function is idempotent.
//
// A note on latency: for sufficiently small heaps (<10s of GiB) this function will take constant
// time, but may take time proportional to the size of the mapped heap beyond that.
//
// The heap lock must not be held over this operation, since it will briefly acquire
// the heap lock.
//
// Must be called on the system stack because it acquires the heap lock.
//
//go:systemstack
[GoRecv] internal static void enableChunkHugePages(this ref pageAlloc Δp) {
    // Grab the heap lock to turn on huge pages for new chunks and clone the current
    // heap address space ranges.
    //
    // After the lock is released, we can be sure that bitmaps for any new chunks may
    // be backed with huge pages, and we have the address space for the rest of the
    // chunks. At the end of this function, all chunk metadata should be backed by huge
    // pages.
    @lock(Ꮡmheap_.of(mheap.Ꮡlock));
    if (Δp.chunkHugePages) {
        unlock(Ꮡmheap_.of(mheap.Ꮡlock));
        return;
    }
    Δp.chunkHugePages = true;
    ref var inUse = ref heap(new addrRanges(), out var ᏑinUse);
    inUse.sysStat = Δp.sysStat;
    Δp.inUse.cloneInto(ᏑinUse);
    unlock(Ꮡmheap_.of(mheap.Ꮡlock));
    // This might seem like a lot of work, but all these loops are for generality.
    //
    // For a 1 GiB contiguous heap, a 48-bit address space, 13 L1 bits, a palloc chunk size
    // of 4 MiB, and adherence to the default set of heap address hints, this will result in
    // exactly 1 call to sysHugePage.
    foreach (var (_, r) in Δp.inUse.ranges) {
        for (nuint i = chunkIndex(r.@base.addr()).l1(); i < chunkIndex(r.limit.addr() - 1).l1(); i++) {
            // N.B. We can assume that p.chunks[i] is non-nil and in a mapped part of p.chunks
            // because it's derived from inUse, which never shrinks.
            sysHugePage(@unsafe.Pointer.FromPinnedBox(Δp.chunks[(nint)(i)]), /* unsafe.Sizeof(*p.chunks[0]) */ (uintptr)1048576);
        }
    }
}

// update updates heap metadata. It must be called each time the bitmap
// is updated.
//
// If contig is true, update does some optimizations assuming that there was
// a contiguous allocation or free between addr and addr+npages. alloc indicates
// whether the operation performed was an allocation or a free.
//
// p.mheapLock must be held.
[GoRecv] internal static void update(this ref pageAlloc Δp, uintptr @base, uintptr npages, bool contig, bool alloc) {
    assertLockHeld(Δp.mheapLock);
    // base, limit, start, and end are inclusive.
    var limit = @base + npages * (uintptr)pageSize - 1;
    chunkIdx sc = chunkIndex(@base);
    chunkIdx ec = chunkIndex(limit);
    // Handle updating the lowest level first.
    if (sc == ec){
        // Fast path: the allocation doesn't span more than one chunk,
        // so update this one and if the summary didn't change, return.
        var x = Δp.summary[len(Δp.summary) - 1][sc];
        var y = Δp.chunkOf(sc).of(pallocData.ᏑpallocBits).summarize();
        if (x == y) {
            return;
        }
        Δp.summary[len(Δp.summary) - 1][sc] = y;
    } else 
    if (contig){
        // Slow contiguous path: the allocation spans more than one chunk
        // and at least one summary is guaranteed to change.
        var summary = Δp.summary[len(Δp.summary) - 1];
        // Update the summary for chunk sc.
        summary[sc] = Δp.chunkOf(sc).of(pallocData.ᏑpallocBits).summarize();
        // Update the summaries for chunks in between, which are
        // either totally allocated or freed.
        var whole = Δp.summary[len(Δp.summary) - 1][(int)(nuint)(sc + 1)..(int)(nuint)(ec)];
        if (alloc){
            builtin.clear(whole);
        } else {
            foreach (var (i, _) in whole) {
                whole[i] = freeChunkSum;
            }
        }
        // Update the summary for chunk ec.
        summary[ec] = Δp.chunkOf(ec).of(pallocData.ᏑpallocBits).summarize();
    } else {
        // Slow general path: the allocation spans more than one chunk
        // and at least one summary is guaranteed to change.
        //
        // We can't assume a contiguous allocation happened, so walk over
        // every chunk in the range and manually recompute the summary.
        var summary = Δp.summary[len(Δp.summary) - 1];
        for (chunkIdx c = sc; c <= ec; c++) {
            summary[c] = Δp.chunkOf(c).of(pallocData.ᏑpallocBits).summarize();
        }
    }
    // Walk up the radix tree and update the summaries appropriately.
    var changed = true;
    for (nint l = len(Δp.summary) - 2; l >= 0 && changed; l--) {
        // Update summaries at level l from summaries at level l+1.
        changed = false;
        // "Constants" for the previous level which we
        // need to compute the summary from that level.
        nuint logEntriesPerBlock = levelBits[l + 1];
        nuint logMaxPages = levelLogPages[l + 1];
        // lo and hi describe all the parts of the level we need to look at.
        var (lo, hi) = addrsToSummaryRange(l, @base, limit + 1);
        // Iterate over each block, updating the corresponding summary in the less-granular level.
        for (nint i = lo; i < hi; i++) {
            var children = Δp.summary[l + 1][(int)(i.Lsh(logEntriesPerBlock))..(int)((i + 1).Lsh(logEntriesPerBlock))];
            var sum = mergeSummaries(children, logMaxPages);
            var old = Δp.summary[l][i];
            if (old != sum) {
                changed = true;
                Δp.summary[l][i] = sum;
            }
        }
    }
}

// allocRange marks the range of memory [base, base+npages*pageSize) as
// allocated. It also updates the summaries to reflect the newly-updated
// bitmap.
//
// Returns the amount of scavenged memory in bytes present in the
// allocated range.
//
// p.mheapLock must be held.
[GoRecv] internal static uintptr allocRange(this ref pageAlloc Δp, uintptr @base, uintptr npages) {
    assertLockHeld(Δp.mheapLock);
    var limit = @base + npages * (uintptr)pageSize - 1;
    chunkIdx sc = chunkIndex(@base);
    chunkIdx ec = chunkIndex(limit);
    nuint si = chunkPageIndex(@base);
    nuint ei = chunkPageIndex(limit);
    nuint scav = (nuint)0;
    if (sc == ec){
        // The range doesn't cross any chunk boundaries.
        var chunk = Δp.chunkOf(sc);
        scav += chunk.of(pallocData.Ꮡscavenged).popcntRange(si, ei + 1 - si);
        chunk.allocRange(si, ei + 1 - si);
        Δp.scav.index.alloc(sc, ei + 1 - si);
    } else {
        // The range crosses at least one chunk boundary.
        var chunk = Δp.chunkOf(sc);
        scav += chunk.of(pallocData.Ꮡscavenged).popcntRange(si, (nuint)pallocChunkPages - si);
        chunk.allocRange(si, (nuint)pallocChunkPages - si);
        Δp.scav.index.alloc(sc, (nuint)pallocChunkPages - si);
        for (chunkIdx c = sc + 1; c < ec; c++) {
            var chunkΔ1 = Δp.chunkOf(c);
            scav += chunkΔ1.of(pallocData.Ꮡscavenged).popcntRange(0, pallocChunkPages);
            chunkΔ1.allocAll();
            Δp.scav.index.alloc(c, pallocChunkPages);
        }
        chunk = Δp.chunkOf(ec);
        scav += chunk.of(pallocData.Ꮡscavenged).popcntRange(0, ei + 1);
        chunk.allocRange(0, ei + 1);
        Δp.scav.index.alloc(ec, ei + 1);
    }
    Δp.update(@base, npages, true, true);
    return (uintptr)scav * (uintptr)pageSize;
}

// findMappedAddr returns the smallest mapped offAddr that is
// >= addr. That is, if addr refers to mapped memory, then it is
// returned. If addr is higher than any mapped region, then
// it returns maxOffAddr.
//
// p.mheapLock must be held.
[GoRecv] internal static offAddr findMappedAddr(this ref pageAlloc Δp, offAddr addr) {
    assertLockHeld(Δp.mheapLock);
    // If we're not in a test, validate first by checking mheap_.arenas.
    // This is a fast path which is only safe to use outside of testing.
    arenaIdx ai = arenaIndex(addr.addr());
    if (Δp.test || mheap_.arenas[(nint)(ai.l1())] == nil || mheap_.arenas[(nint)(ai.l1())].Value[ai.l2()] == nil) {
        var (vAddr, ok) = Δp.inUse.findAddrGreaterEqual(addr.addr());
        if (ok){
            return new offAddr(vAddr);
        } else {
            // The candidate search address is greater than any
            // known address, which means we definitely have no
            // free memory left.
            return maxOffAddr;
        }
    }
    return addr;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rangePartiallyOverlapsˢ = "range partially overlaps"u8;
internal static readonly @string badSummaryDataˢ = "bad summary data"u8;

[GoType("dyn")] internal partial struct find_firstFree {
    internal offAddr @base, bound;
}

// find searches for the first (address-ordered) contiguous free region of
// npages in size and returns a base address for that region.
//
// It uses p.searchAddr to prune its search and assumes that no palloc chunks
// below chunkIndex(p.searchAddr) contain any free memory at all.
//
// find also computes and returns a candidate p.searchAddr, which may or
// may not prune more of the address space than p.searchAddr already does.
// This candidate is always a valid p.searchAddr.
//
// find represents the slow path and the full radix tree search.
//
// Returns a base address of 0 on failure, in which case the candidate
// searchAddr returned is invalid and must be ignored.
//
// p.mheapLock must be held.
[GoRecv] internal static (uintptr, offAddr) find(this ref pageAlloc Δp, uintptr npages) {
    assertLockHeld(Δp.mheapLock);
    // Search algorithm.
    //
    // This algorithm walks each level l of the radix tree from the root level
    // to the leaf level. It iterates over at most 1 << levelBits[l] of entries
    // in a given level in the radix tree, and uses the summary information to
    // find either:
    //  1) That a given subtree contains a large enough contiguous region, at
    //     which point it continues iterating on the next level, or
    //  2) That there are enough contiguous boundary-crossing bits to satisfy
    //     the allocation, at which point it knows exactly where to start
    //     allocating from.
    //
    // i tracks the index into the current level l's structure for the
    // contiguous 1 << levelBits[l] entries we're actually interested in.
    //
    // NOTE: Technically this search could allocate a region which crosses
    // the arenaBaseOffset boundary, which when arenaBaseOffset != 0, is
    // a discontinuity. However, the only way this could happen is if the
    // page at the zero address is mapped, and this is impossible on
    // every system we support where arenaBaseOffset != 0. So, the
    // discontinuity is already encoded in the fact that the OS will never
    // map the zero page for us, and this function doesn't try to handle
    // this case in any way.
    // i is the beginning of the block of entries we're searching at the
    // current level.
    nint i = 0;
    // firstFree is the region of address space that we are certain to
    // find the first free page in the heap. base and bound are the inclusive
    // bounds of this window, and both are addresses in the linearized, contiguous
    // view of the address space (with arenaBaseOffset pre-added). At each level,
    // this window is narrowed as we find the memory region containing the
    // first free page of memory. To begin with, the range reflects the
    // full process address space.
    //
    // firstFree is updated by calling foundFree each time free space in the
    // heap is discovered.
    //
    // At the end of the search, base.addr() is the best new
    // searchAddr we could deduce in this search.
    ref var firstFree = ref heap<find_firstFree>(out var ᏑfirstFree);
    firstFree = new find_firstFree(
        @base: minOffAddr,
        bound: maxOffAddr
    );
    // foundFree takes the given address range [addr, addr+size) and
    // updates firstFree if it is a narrower range. The input range must
    // either be fully contained within firstFree or not overlap with it
    // at all.
    //
    // This way, we'll record the first summary we find with any free
    // pages on the root level and narrow that down if we descend into
    // that summary. But as soon as we need to iterate beyond that summary
    // in a level to find a large enough range, we'll stop narrowing.
    void foundFree(offAddr addrΔ1, uintptr size) {
        if (ᏑfirstFree.Value.@base.lessEqual(addrΔ1) && addrΔ1.add(size - 1).lessEqual(ᏑfirstFree.Value.bound)){
            // This range fits within the current firstFree window, so narrow
            // down the firstFree window to the base and bound of this range.
            ᏑfirstFree.Value.@base = addrΔ1;
            ᏑfirstFree.Value.bound = addrΔ1.add(size - 1);
        } else 
        if (!(addrΔ1.add(size - 1).lessThan(ᏑfirstFree.Value.@base) || ᏑfirstFree.Value.bound.lessThan(addrΔ1))) {
            // This range only partially overlaps with the firstFree range,
            // so throw.
            print((@string)"runtime: addr = "u8, ((Δhex)(uint64)addrΔ1.addr()), (@string)", size = "u8, size, (@string)"\n"u8);
            print((@string)"runtime: base = "u8, ((Δhex)(uint64)ᏑfirstFree.Value.@base.addr()), (@string)", bound = "u8, ((Δhex)(uint64)ᏑfirstFree.Value.bound.addr()), (@string)"\n"u8);
            @throw(rangePartiallyOverlapsˢ);
        }
    }
    // lastSum is the summary which we saw on the previous level that made us
    // move on to the next level. Used to print additional information in the
    // case of a catastrophic failure.
    // lastSumIdx is that summary's index in the previous level.
    var lastSum = packPallocSum(0, 0, 0);
    nint lastSumIdx = -1;
nextLevel:
    for (nint l = 0; l < len(Δp.summary); l++) {
        // For the root level, entriesPerBlock is the whole level.
        nint entriesPerBlock = ((nint)1).Lsh(levelBits[l]);
        nuint logMaxPages = levelLogPages[l];
        // We've moved into a new level, so let's update i to our new
        // starting index. This is a no-op for level 0.
        i <<= (int)(levelBits[l]);
        // Slice out the block of entries we care about.
        var entries = Δp.summary[l][(int)(i)..(int)(i + entriesPerBlock)];
        // Determine j0, the first index we should start iterating from.
        // The searchAddr may help us eliminate iterations if we followed the
        // searchAddr on the previous level or we're on the root level, in which
        // case the searchAddr should be the same as i after levelShift.
        nint j0 = 0;
        {
            nint searchIdxΔ1 = offAddrToLevelIndex(l, Δp.searchAddr); if ((nint)(searchIdxΔ1 & ~(entriesPerBlock - 1)) == i) {
                j0 = (nint)(searchIdxΔ1 & (entriesPerBlock - 1));
            }
        }
        // Run over the level entries looking for
        // a contiguous run of at least npages either
        // within an entry or across entries.
        //
        // base contains the page index (relative to
        // the first entry's first page) of the currently
        // considered run of consecutive pages.
        //
        // size contains the size of the currently considered
        // run of consecutive pages.
        nuint @base = default!;
        nuint size = default!;
        for (nint jΔ1 = j0; jΔ1 < len(entries); jΔ1++) {
            var sum = entries[jΔ1];
            if (sum == 0) {
                // A full entry means we broke any streak and
                // that we should skip it altogether.
                size = 0;
                continue;
            }
            // We've encountered a non-zero summary which means
            // free memory, so update firstFree.
            foundFree(levelIndexToOffAddr(l, i + jΔ1), (((uintptr)1).Lsh(logMaxPages)) * (uintptr)pageSize);
            nuint s = sum.start();
            if (size + s >= (nuint)npages) {
                // If size == 0 we don't have a run yet,
                // which means base isn't valid. So, set
                // base to the first page in this block.
                if (size == 0) {
                    @base = ((nuint)jΔ1).Lsh(logMaxPages);
                }
                // We hit npages; we're done!
                size += s;
                break;
            }
            if (sum.max() >= (nuint)npages) {
                // The entry itself contains npages contiguous
                // free pages, so continue on the next level
                // to find that run.
                i += jΔ1;
                lastSumIdx = i;
                lastSum = sum;
                goto continue_nextLevel;
            }
            if (size == 0 || s < ((nuint)1).Lsh(logMaxPages)) {
                // We either don't have a current run started, or this entry
                // isn't totally free (meaning we can't continue the current
                // one), so try to begin a new run by setting size and base
                // based on sum.end.
                size = sum.end();
                @base = ((nuint)(jΔ1 + 1)).Lsh(logMaxPages) - size;
                continue;
            }
            // The entry is completely free, so continue the run.
            size += ((nuint)1).Lsh(logMaxPages);
        }
        if (size >= (nuint)npages) {
            // We found a sufficiently large run of free pages straddling
            // some boundary, so compute the address and return it.
            var addrΔ2 = levelIndexToOffAddr(l, i).add((uintptr)@base * (uintptr)pageSize).addr();
            return (addrΔ2, Δp.findMappedAddr(firstFree.@base));
        }
        if (l == 0) {
            // We're at level zero, so that means we've exhausted our search.
            return (0, maxSearchAddr());
        }
        // We're not at level zero, and we exhausted the level we were looking in.
        // This means that either our calculations were wrong or the level above
        // lied to us. In either case, dump some useful state and throw.
        print((@string)"runtime: summary["u8, l - 1, (@string)"]["u8, lastSumIdx, (@string)"] = "u8, lastSum.start(), (@string)", "u8, lastSum.max(), (@string)", "u8, lastSum.end(), (@string)"\n"u8);
        print((@string)"runtime: level = "u8, l, (@string)", npages = "u8, npages, (@string)", j0 = "u8, j0, (@string)"\n"u8);
        print((@string)"runtime: p.searchAddr = "u8, ((Δhex)(uint64)Δp.searchAddr.addr()), (@string)", i = "u8, i, (@string)"\n"u8);
        print((@string)"runtime: levelShift[level] = "u8, levelShift[l], (@string)", levelBits[level] = "u8, levelBits[l], (@string)"\n"u8);
        for (nint jΔ2 = 0; jΔ2 < len(entries); jΔ2++) {
            var sum = entries[jΔ2];
            print((@string)"runtime: summary["u8, l, (@string)"]["u8, i + jΔ2, (@string)"] = ("u8, sum.start(), (@string)", "u8, sum.max(), (@string)", "u8, sum.end(), (@string)")\n"u8);
        }
        @throw(badSummaryDataˢ);
continue_nextLevel:;
    }
break_nextLevel:;
    // Since we've gotten to this point, that means we haven't found a
    // sufficiently-sized free region straddling some boundary (chunk or larger).
    // This means the last summary we inspected must have had a large enough "max"
    // value, so look inside the chunk to find a suitable run.
    //
    // After iterating over all levels, i must contain a chunk index which
    // is what the final level represents.
    chunkIdx ci = ((chunkIdx)(nuint)i);
    var (j, searchIdx) = Δp.chunkOf(ci).of(pallocData.ᏑpallocBits).find(npages, 0);
    if (j == ~(nuint)0) {
        // We couldn't find any space in this chunk despite the summaries telling
        // us it should be there. There's likely a bug, so dump some state and throw.
        var sum = Δp.summary[len(Δp.summary) - 1][i];
        print((@string)"runtime: summary["u8, (nint)(len(Δp.summary) - 1), (@string)"]["u8, i, (@string)"] = ("u8, sum.start(), (@string)", "u8, sum.max(), (@string)", "u8, sum.end(), (@string)")\n"u8);
        print((@string)"runtime: npages = "u8, npages, (@string)"\n"u8);
        @throw(badSummaryDataˢ);
    }
    // Compute the address at which the free space starts.
    var addr = chunkBase(ci) + (uintptr)j * (uintptr)pageSize;
    // Since we actually searched the chunk, we may have
    // found an even narrower free window.
    var searchAddr = chunkBase(ci) + (uintptr)searchIdx * (uintptr)pageSize;
    foundFree(new offAddr(searchAddr), chunkBase(ci + 1) - searchAddr);
    return (addr, Δp.findMappedAddr(firstFree.@base));
}

// alloc allocates npages worth of memory from the page heap, returning the base
// address for the allocation and the amount of scavenged memory in bytes
// contained in the region [base address, base address + npages*pageSize).
//
// Returns a 0 base address on failure, in which case other returned values
// should be ignored.
//
// p.mheapLock must be held.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
[GoRecv] internal static (uintptr addr, uintptr scav) alloc(this ref pageAlloc Δp, uintptr npages) {
    uintptr addr = default!;
    uintptr scav = default!;

    assertLockHeld(Δp.mheapLock);
    // If the searchAddr refers to a region which has a higher address than
    // any known chunk, then we know we're out of memory.
    if (chunkIndex(Δp.searchAddr.addr()) >= Δp.end) {
        return (0, 0);
    }
    // If npages has a chance of fitting in the chunk where the searchAddr is,
    // search it directly.
    var searchAddr = minOffAddr;
    if ((nuint)pallocChunkPages - chunkPageIndex(Δp.searchAddr.addr()) >= (nuint)npages) {
        // npages is guaranteed to be no greater than pallocChunkPages here.
        chunkIdx i = chunkIndex(Δp.searchAddr.addr());
        {
            nuint max = Δp.summary[len(Δp.summary) - 1][i].max(); if (max >= (nuint)npages) {
                var (j, searchIdx) = Δp.chunkOf(i).of(pallocData.ᏑpallocBits).find(npages, chunkPageIndex(Δp.searchAddr.addr()));
                if (j == ~(nuint)0) {
                    print((@string)"runtime: max = "u8, max, (@string)", npages = "u8, npages, (@string)"\n"u8);
                    print((@string)"runtime: searchIdx = "u8, chunkPageIndex(Δp.searchAddr.addr()), (@string)", p.searchAddr = "u8, ((Δhex)(uint64)Δp.searchAddr.addr()), (@string)"\n"u8);
                    @throw(badSummaryDataˢ);
                }
                addr = chunkBase(i) + (uintptr)j * (uintptr)pageSize;
                searchAddr = new offAddr(chunkBase(i) + (uintptr)searchIdx * (uintptr)pageSize);
                goto Found;
            }
        }
    }
    // We failed to use a searchAddr for one reason or another, so try
    // the slow path.
    (addr, searchAddr) = Δp.find(npages);
    if (addr == 0) {
        if (npages == 1) {
            // We failed to find a single free page, the smallest unit
            // of allocation. This means we know the heap is completely
            // exhausted. Otherwise, the heap still might have free
            // space in it, just not enough contiguous space to
            // accommodate npages.
            Δp.searchAddr = maxSearchAddr();
        }
        return (0, 0);
    }
Found:
    scav = Δp.allocRange(addr, // Go ahead and actually mark the bits now that we have an address.
 npages);
    // If we found a higher searchAddr, we know that all the
    // heap memory before that searchAddr in an offset address space is
    // allocated, so bump p.searchAddr up to the new one.
    if (Δp.searchAddr.lessThan(searchAddr)) {
        Δp.searchAddr = searchAddr;
    }
    return (addr, scav);
}

// free returns npages worth of memory starting at base back to the page heap.
//
// p.mheapLock must be held.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
internal static void free(this ж<pageAlloc> Ꮡp, uintptr @base, uintptr npages) {
    ref var Δp = ref Ꮡp.DerefOrNull();

    assertLockHeld(Δp.mheapLock);
    // If we're freeing pages below the p.searchAddr, update searchAddr.
    {
        var b = (new offAddr(@base)); if (b.lessThan(Δp.searchAddr)) {
            Δp.searchAddr = b;
        }
    }
    var limit = @base + npages * (uintptr)pageSize - 1;
    if (npages == 1){
        // Fast path: we're clearing a single bit, and we know exactly
        // where it is, so mark it directly.
        chunkIdx i = chunkIndex(@base);
        nuint pi = chunkPageIndex(@base);
        Δp.chunkOf(i).of(pallocData.ᏑpallocBits).free1(pi);
        Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).free(i, pi, 1);
    } else {
        // Slow path: we're clearing more bits so we may need to iterate.
        chunkIdx sc = chunkIndex(@base);
        chunkIdx ec = chunkIndex(limit);
        nuint si = chunkPageIndex(@base);
        nuint ei = chunkPageIndex(limit);
        if (sc == ec){
            // The range doesn't cross any chunk boundaries.
            Δp.chunkOf(sc).of(pallocData.ᏑpallocBits).free(si, ei + 1 - si);
            Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).free(sc, si, ei + 1 - si);
        } else {
            // The range crosses at least one chunk boundary.
            Δp.chunkOf(sc).of(pallocData.ᏑpallocBits).free(si, (nuint)pallocChunkPages - si);
            Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).free(sc, si, (nuint)pallocChunkPages - si);
            for (chunkIdx c = sc + 1; c < ec; c++) {
                Δp.chunkOf(c).of(pallocData.ᏑpallocBits).freeAll();
                Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).free(c, 0, pallocChunkPages);
            }
            Δp.chunkOf(ec).of(pallocData.ᏑpallocBits).free(0, ei + 1);
            Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).free(ec, 0, ei + 1);
        }
    }
    Δp.update(@base, npages, true, false);
}

internal static uintptr pallocSumBytes => /* unsafe.Sizeof(pallocSum(0)) */ 8;
internal static UntypedInt maxPackedValue => /* 1 << logMaxPackedValue */ 2097152;
internal static UntypedInt logMaxPackedValue => /* logPallocChunkPages + (summaryLevels-1)*summaryLevelBits */ 21;
internal static pallocSum freeChunkSum => /* pallocSum(uint64(pallocChunkPages) |
	uint64(pallocChunkPages<<logMaxPackedValue) |
	uint64(pallocChunkPages<<(2*logMaxPackedValue))) */ unchecked((pallocSum)2251800887427584);

[GoType("num:uint64")] partial struct pallocSum;

// packPallocSum takes a start, max, and end value and produces a pallocSum.
internal static pallocSum packPallocSum(nuint start, nuint max, nuint end) {
    if (max == maxPackedValue) {
        return ((pallocSum)(uint64)(((uint64)1 << (int)(63))));
    }
    return ((pallocSum)((uint64)((uint64)(((uint64)((uint64)start & (uint64)((maxPackedValue - 1)))) | ((((uint64)((uint64)max & (uint64)((maxPackedValue - 1)))) << (int)(logMaxPackedValue)))) | ((((uint64)((uint64)end & (uint64)((maxPackedValue - 1)))) << (int)((2 * logMaxPackedValue)))))));
}

// start extracts the start value from a packed sum.
internal static nuint start(this pallocSum Δp) {
    if ((uint64)((uint64)Δp & (uint64)(((uint64)1 << (int)(63)))) != 0) {
        return maxPackedValue;
    }
    return (nuint)((uint64)((uint64)Δp & (uint64)((maxPackedValue - 1))));
}

// max extracts the max value from a packed sum.
internal static nuint max(this pallocSum Δp) {
    if ((uint64)((uint64)Δp & (uint64)(((uint64)1 << (int)(63)))) != 0) {
        return maxPackedValue;
    }
    return (nuint)((uint64)((((uint64)Δp >> (int)(logMaxPackedValue))) & (uint64)((maxPackedValue - 1))));
}

// end extracts the end value from a packed sum.
internal static nuint end(this pallocSum Δp) {
    if ((uint64)((uint64)Δp & (uint64)(((uint64)1 << (int)(63)))) != 0) {
        return maxPackedValue;
    }
    return (nuint)((uint64)((((uint64)Δp >> (int)((2 * logMaxPackedValue)))) & (uint64)((maxPackedValue - 1))));
}

// unpack unpacks all three values from the summary.
internal static (nuint, nuint, nuint) unpack(this pallocSum Δp) {
    if ((uint64)((uint64)Δp & (uint64)(((uint64)1 << (int)(63)))) != 0) {
        return (maxPackedValue, maxPackedValue, maxPackedValue);
    }
    return ((nuint)((uint64)((uint64)Δp & (uint64)((maxPackedValue - 1)))), (nuint)((uint64)((((uint64)Δp >> (int)(logMaxPackedValue))) & (uint64)((maxPackedValue - 1)))), (nuint)((uint64)((((uint64)Δp >> (int)((2 * logMaxPackedValue)))) & (uint64)((maxPackedValue - 1)))));
}

// mergeSummaries merges consecutive summaries which may each represent at
// most 1 << logMaxPagesPerSum pages each together into one.
internal static pallocSum mergeSummaries(slice<pallocSum> sums, nuint logMaxPagesPerSum) {
    // Merge the summaries in sums into one.
    //
    // We do this by keeping a running summary representing the merged
    // summaries of sums[:i] in start, most, and end.
    var (start, most, end) = sums[0].unpack();
    for (nint i = 1; i < len(sums); i++) {
        // Merge in sums[i].
        var (si, mi, ei) = sums[i].unpack();
        // Merge in sums[i].start only if the running summary is
        // completely free, otherwise this summary's start
        // plays no role in the combined sum.
        if (start == ((nuint)i).Lsh(logMaxPagesPerSum)) {
            start += si;
        }
        // Recompute the max value of the running sum by looking
        // across the boundary between the running sum and sums[i]
        // and at the max sums[i], taking the greatest of those two
        // and the max of the running sum.
        most = builtin.max(most, end + si, mi);
        // Merge in end by checking if this new summary is totally
        // free. If it is, then we want to extend the running sum's
        // end by the new summary. If not, then we have some alloc'd
        // pages in there and we just want to take the end value in
        // sums[i].
        if (ei == ((nuint)1).Lsh(logMaxPagesPerSum)){
            end += ((nuint)1).Lsh(logMaxPagesPerSum);
        } else {
            end = ei;
        }
    }
    return packPallocSum(start, most, end);
}

} // end runtime_package
