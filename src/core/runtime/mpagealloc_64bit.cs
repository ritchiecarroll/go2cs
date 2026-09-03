// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 || arm64 || loong64 || mips64 || mips64le || ppc64 || ppc64le || riscv64 || s390x
namespace go;

using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

internal static UntypedInt summaryLevels => 5;
internal static UntypedInt pageAlloc32Bit => 0;
internal static UntypedInt pageAlloc64Bit => 1;
internal static UntypedInt pallocChunksL1Bits => 13;

// levelBits is the number of bits in the radix for a given level in the super summary
// structure.
//
// The sum of all the entries of levelBits should equal heapAddrBits.
internal static array<nuint> levelBits = new nuint[]{
    summaryL0Bits,
    summaryLevelBits,
    summaryLevelBits,
    summaryLevelBits,
    summaryLevelBits
}.array();

// levelShift is the number of bits to shift to acquire the radix for a given level
// in the super summary structure.
//
// With levelShift, one can compute the index of the summary at level l related to a
// pointer p by doing:
//
//	p >> levelShift[l]
internal static array<nuint> levelShift = new nuint[]{
    heapAddrBits - summaryL0Bits,
    heapAddrBits - summaryL0Bits - 1 * summaryLevelBits,
    heapAddrBits - summaryL0Bits - 2 * summaryLevelBits,
    heapAddrBits - summaryL0Bits - 3 * summaryLevelBits,
    heapAddrBits - summaryL0Bits - 4 * summaryLevelBits
}.array();

// levelLogPages is log2 the maximum number of runtime pages in the address space
// a summary in the given level represents.
//
// The leaf level always represents exactly log2 of 1 chunk's worth of pages.
internal static array<nuint> levelLogPages = new nuint[]{
    logPallocChunkPages + 4 * summaryLevelBits,
    logPallocChunkPages + 3 * summaryLevelBits,
    logPallocChunkPages + 2 * summaryLevelBits,
    logPallocChunkPages + 1 * summaryLevelBits,
    logPallocChunkPages
}.array();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string failedToReservePageˢ = "failed to reserve page summary memory"u8;

// sysInit performs architecture-dependent initialization of fields
// in pageAlloc. pageAlloc should be uninitialized except for sysStat
// if any runtime statistic should be updated.
[GoRecv] internal static void sysInit(this ref pageAlloc Δp, bool test) {
    // Reserve memory for each level. This will get mapped in
    // as R/W by setArenas.
    foreach (var (l, shift) in levelShift.ΔRangeSnapshot()) {
        nint entries = ((nint)1).Lsh(((nuint)heapAddrBits - shift));
        // Reserve b bytes of memory anywhere in the address space.
        var b = alignUp((uintptr)entries * pallocSumBytes, physPageSize);
        @unsafe.Pointer r = (uintptr)sysReserve(nil, b);
        if (r == nil) {
            @throw(failedToReservePageˢ);
        }
        // Put this reservation into a slice.
        ref var sl = ref heap<notInHeapSlice>(out var Ꮡsl);
        sl = new notInHeapSlice((ж<notInHeap>)(uintptr)(r), 0, entries);
        Δp.summary[l] = ~Ꮡsl.Reinterpret<notInHeapSlice, slice<pallocSum>>();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sysGrowBoundsNotAlignedˢ = "sysGrow bounds not aligned to pallocChunkBytes"u8;

// sysGrow performs architecture-dependent operations on heap
// growth for the page allocator, such as mapping in new memory
// for summaries. It also updates the length of the slices in
// p.summary.
//
// base is the base of the newly-added heap memory and limit is
// the first address past the end of the newly-added heap memory.
// Both must be aligned to pallocChunkBytes.
//
// The caller must update p.start and p.end after calling sysGrow.
internal static void sysGrow(this ж<pageAlloc> Ꮡp, uintptr @base, uintptr limit) {
    ref var Δp = ref Ꮡp.DerefOrNull();

    if (@base % (uintptr)pallocChunkBytes != 0 || limit % (uintptr)pallocChunkBytes != 0) {
        print((@string)"runtime: base = "u8, ((Δhex)(uint64)@base), (@string)", limit = "u8, ((Δhex)(uint64)limit), (@string)"\n"u8);
        @throw(sysGrowBoundsNotAlignedˢ);
    }
    // addrRangeToSummaryRange converts a range of addresses into a range
    // of summary indices which must be mapped to support those addresses
    // in the summary range.
    (nint, nint) addrRangeToSummaryRange(nint level, addrRange r) {
        var (sumIdxBase, sumIdxLimit) = addrsToSummaryRange(level, r.@base.addr(), r.limit.addr());
        return blockAlignSummaryRange(level, sumIdxBase, sumIdxLimit);
    }
    // summaryRangeToSumAddrRange converts a range of indices in any
    // level of p.summary into page-aligned addresses which cover that
    // range of indices.
    addrRange summaryRangeToSumAddrRange(nint level, nint sumIdxBase, nint sumIdxLimit) {
        var baseOffset = alignDown((uintptr)sumIdxBase * pallocSumBytes, physPageSize);
        var limitOffset = alignUp((uintptr)sumIdxLimit * pallocSumBytes, physPageSize);
        @unsafe.Pointer baseΔ1 = new @unsafe.Pointer(Ꮡ(Ꮡp.Value.summary[level], 0));
        return new addrRange(
            new offAddr((uintptr)(uintptr)add(baseΔ1, baseOffset)),
            new offAddr((uintptr)(uintptr)add(baseΔ1, limitOffset))
        );
    }
    // addrRangeToSumAddrRange is a convenience function that converts
    // an address range r to the address range of the given summary level
    // that stores the summaries for r.
    var addrRangeToSummaryRangeʗ1 = addrRangeToSummaryRange;
    var summaryRangeToSumAddrRangeʗ1 = summaryRangeToSumAddrRange;
    addrRange addrRangeToSumAddrRange(nint level, addrRange r) {
        var (sumIdxBase, sumIdxLimit) = addrRangeToSummaryRangeʗ1(level, r);
        return summaryRangeToSumAddrRangeʗ1(level, sumIdxBase, sumIdxLimit);
    }
    // Find the first inUse index which is strictly greater than base.
    //
    // Because this function will never be asked remap the same memory
    // twice, this index is effectively the index at which we would insert
    // this new growth, and base will never overlap/be contained within
    // any existing range.
    //
    // This will be used to look at what memory in the summary array is already
    // mapped before and after this new range.
    nint inUseIndex = Δp.inUse.findSucc(@base);
    // Walk up the radix tree and map summaries in as needed.
    foreach (var (l, _) in Δp.summary) {
        // Figure out what part of the summary array this new address space needs.
        var (needIdxBase, needIdxLimit) = addrRangeToSummaryRange(l, makeAddrRange(@base, limit));
        // Update the summary slices with a new upper-bound. This ensures
        // we get tight bounds checks on at least the top bound.
        //
        // We must do this regardless of whether we map new memory.
        if (needIdxLimit > len(Δp.summary[l])) {
            Δp.summary[l] = Δp.summary[l][..(int)(needIdxLimit)];
        }
        // Compute the needed address range in the summary array for level l.
        var need = summaryRangeToSumAddrRange(l, needIdxBase, needIdxLimit);
        // Prune need down to what needs to be newly mapped. Some parts of it may
        // already be mapped by what inUse describes due to page alignment requirements
        // for mapping. Because this function will never be asked to remap the same
        // memory twice, it should never be possible to prune in such a way that causes
        // need to be split.
        if (inUseIndex > 0) {
            need = need.subtract(addrRangeToSumAddrRange(l, Δp.inUse.ranges[inUseIndex - 1]));
        }
        if (inUseIndex < len(Δp.inUse.ranges)) {
            need = need.subtract(addrRangeToSumAddrRange(l, Δp.inUse.ranges[inUseIndex]));
        }
        // It's possible that after our pruning above, there's nothing new to map.
        if (need.size() == 0) {
            continue;
        }
        // Map and commit need.
        sysMap((@unsafe.Pointer)need.@base.addr(), need.size(), Δp.sysStat);
        sysUsed((@unsafe.Pointer)need.@base.addr(), need.size(), need.size());
        Δp.summaryMappedReady += need.size();
    }
    // Update the scavenge index.
    Δp.summaryMappedReady += Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).sysGrow(@base, limit, Δp.sysStat);
}

// sysGrow increases the index's backing store in response to a heap growth.
//
// Returns the amount of memory added to sysStat.
internal static uintptr sysGrow(this ж<scavengeIndex> Ꮡs, uintptr @base, uintptr limit, ж<sysMemStat> ᏑsysStat) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (@base % (uintptr)pallocChunkBytes != 0 || limit % (uintptr)pallocChunkBytes != 0) {
        print((@string)"runtime: base = "u8, ((Δhex)(uint64)@base), (@string)", limit = "u8, ((Δhex)(uint64)limit), (@string)"\n"u8);
        @throw(sysGrowBoundsNotAlignedˢ);
    }
    var scSize = /* unsafe.Sizeof(atomicScavChunkData{}) */ (uintptr)8;
    // Map and commit the pieces of chunks that we need.
    //
    // We always map the full range of the minimum heap address to the
    // maximum heap address. We don't do this for the summary structure
    // because it's quite large and a discontiguous heap could cause a
    // lot of memory to be used. In this situation, the worst case overhead
    // is in the single-digit MiB if we map the whole thing.
    //
    // The base address of the backing store is always page-aligned,
    // because it comes from the OS, so it's sufficient to align the
    // index.
    var haveMin = Ꮡs.of(scavengeIndex.Ꮡmin).Load();
    var haveMax = Ꮡs.of(scavengeIndex.Ꮡmax).Load();
    var needMin = alignDown((uintptr)(nuint)chunkIndex(@base), physPageSize / scSize);
    var needMax = alignUp((uintptr)(nuint)chunkIndex(limit), physPageSize / scSize);
    // We need a contiguous range, so extend the range if there's no overlap.
    if (needMax < haveMin) {
        needMax = haveMin;
    }
    if (haveMax != 0 && needMin > haveMax) {
        needMin = haveMax;
    }
    // Avoid a panic from indexing one past the last element.
    var chunksBase = (uintptr)Ꮡ(s.chunks, 0);
    var have = makeAddrRange(chunksBase + haveMin * scSize, chunksBase + haveMax * scSize);
    var need = makeAddrRange(chunksBase + needMin * scSize, chunksBase + needMax * scSize);
    // Subtract any overlap from rounding. We can't re-map memory because
    // it'll be zeroed.
    need = need.subtract(have);
    // If we've got something to map, map it, and update the slice bounds.
    if (need.size() != 0) {
        sysMap((@unsafe.Pointer)need.@base.addr(), need.size(), ᏑsysStat);
        sysUsed((@unsafe.Pointer)need.@base.addr(), need.size(), need.size());
        // Update the indices only after the new memory is valid.
        if (haveMax == 0 || needMin < haveMin) {
            Ꮡs.of(scavengeIndex.Ꮡmin).Store(needMin);
        }
        if (needMax > haveMax) {
            Ꮡs.of(scavengeIndex.Ꮡmax).Store(needMax);
        }
    }
    return need.size();
}

// sysInit initializes the scavengeIndex' chunks array.
//
// Returns the amount of memory added to sysStat.
[GoRecv] internal static uintptr sysInit(this ref scavengeIndex s, bool test, ж<sysMemStat> ᏑsysStat) {
    var n = (uintptr)(((uintptr)1 << (int)(heapAddrBits))) / (uintptr)pallocChunkBytes;
    var nbytes = n * /* unsafe.Sizeof(atomicScavChunkData{}) */ (uintptr)8;
    @unsafe.Pointer r = (uintptr)sysReserve(nil, nbytes);
    ref var sl = ref heap<notInHeapSlice>(out var Ꮡsl);
    sl = new notInHeapSlice((ж<notInHeap>)(uintptr)(r), (nint)n, (nint)n);
    s.chunks = ~Ꮡsl.Reinterpret<notInHeapSlice, slice<atomicScavChunkData>>();
    return 0; // All memory above is mapped Reserved.
}

} // end runtime_package
