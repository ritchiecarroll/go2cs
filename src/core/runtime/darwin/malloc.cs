// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Memory allocator.
//
// This was originally based on tcmalloc, but has diverged quite a bit.
// http://goog-perftools.sourceforge.net/doc/tcmalloc.html
// The main allocator works in runs of pages.
// Small allocation sizes (up to and including 32 kB) are
// rounded to one of about 70 size classes, each of which
// has its own free set of objects of exactly that size.
// Any free page of memory can be split into a set of objects
// of one size class, which are then managed using a free bitmap.
//
// The allocator's data structures are:
//
//	fixalloc: a free-list allocator for fixed-size off-heap objects,
//		used to manage storage used by the allocator.
//	mheap: the malloc heap, managed at page (8192-byte) granularity.
//	mspan: a run of in-use pages managed by the mheap.
//	mcentral: collects all spans of a given size class.
//	mcache: a per-P cache of mspans with free space.
//	mstats: allocation statistics.
//
// Allocating a small object proceeds up a hierarchy of caches:
//
//	1. Round the size up to one of the small size classes
//	   and look in the corresponding mspan in this P's mcache.
//	   Scan the mspan's free bitmap to find a free slot.
//	   If there is a free slot, allocate it.
//	   This can all be done without acquiring a lock.
//
//	2. If the mspan has no free slots, obtain a new mspan
//	   from the mcentral's list of mspans of the required size
//	   class that have free space.
//	   Obtaining a whole span amortizes the cost of locking
//	   the mcentral.
//
//	3. If the mcentral's mspan list is empty, obtain a run
//	   of pages from the mheap to use for the mspan.
//
//	4. If the mheap is empty or has no page runs large enough,
//	   allocate a new group of pages (at least 1MB) from the
//	   operating system. Allocating a large run of pages
//	   amortizes the cost of talking to the operating system.
//
// Sweeping an mspan and freeing objects on it proceeds up a similar
// hierarchy:
//
//	1. If the mspan is being swept in response to allocation, it
//	   is returned to the mcache to satisfy the allocation.
//
//	2. Otherwise, if the mspan still has allocated objects in it,
//	   it is placed on the mcentral free list for the mspan's size
//	   class.
//
//	3. Otherwise, if all objects in the mspan are free, the mspan's
//	   pages are returned to the mheap and the mspan is now dead.
//
// Allocating and freeing a large object uses the mheap
// directly, bypassing the mcache and mcentral.
//
// If mspan.needzero is false, then free object slots in the mspan are
// already zeroed. Otherwise if needzero is true, objects are zeroed as
// they are allocated. There are various benefits to delaying zeroing
// this way:
//
//	1. Stack frame allocation can avoid zeroing altogether.
//
//	2. It exhibits better temporal locality, since the program is
//	   probably about to write to the memory.
//
//	3. We don't zero pages that never get reused.
// Virtual memory layout
//
// The heap consists of a set of arenas, which are 64MB on 64-bit and
// 4MB on 32-bit (heapArenaBytes). Each arena's start address is also
// aligned to the arena size.
//
// Each arena has an associated heapArena object that stores the
// metadata for that arena: the heap bitmap for all words in the arena
// and the span map for all pages in the arena. heapArena objects are
// themselves allocated off-heap.
//
// Since arenas are aligned, the address space can be viewed as a
// series of arena frames. The arena map (mheap_.arenas) maps from
// arena frame number to *heapArena, or nil for parts of the address
// space not backed by the Go heap. The arena map is structured as a
// two-level array consisting of a "L1" arena map and many "L2" arena
// maps; however, since arenas are large, on many architectures, the
// arena map consists of a single, large L2 map.
//
// The arena map covers the entire possible address space, allowing
// the Go heap to use any part of the address space. The allocator
// attempts to keep arenas contiguous so that large spans (and hence
// large objects) can cross arenas.
namespace go;

using goarch = @internal.goarch_package;
using goexperiment = @internal.goexperiment_package;
using goos = @internal.goos_package;
using atomic = @internal.runtime.atomic_package;
using gc = @internal.runtime.gc_package;
using Δmath = @internal.runtime.math_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using abi = @internal.abi_package;

[GoFieldAccess(typeof(arenaHint))]
partial class runtime_package {

internal static UntypedInt maxTinySize => /* _TinySize */ 16;
internal const int8 tinySizeClass = /* _TinySizeClass */ 2;
internal static UntypedInt maxSmallSize => /* gc.MaxSmallSize */ 32768;
internal static UntypedInt pageSize => /* 1 << gc.PageShift */ 8192;
internal static UntypedInt pageMask => /* pageSize - 1 */ 8191;

// Unused. Left for viewcore.
internal static UntypedInt _PageSize => /* pageSize */ 8192;
internal static UntypedInt minSizeForMallocHeader => /* gc.MinSizeForMallocHeader */ 512;
internal static UntypedInt mallocHeaderSize => /* gc.MallocHeaderSize */ 8;

// _64bit = 1 on 64-bit systems, 0 on 32-bit systems
internal static UntypedInt _64bit => /* 1 << (^uintptr(0) >> 63) / 2 */ 1;

// Tiny allocator parameters, see "Tiny allocator" comment in malloc.go.
internal static UntypedInt _TinySize => /* gc.TinySize */ 16;
internal const int8 _TinySizeClass = /* int8(gc.TinySizeClass) */ 2;

internal static UntypedInt _FixAllocChunk => /* 16 << 10 */ 16384; // Chunk size for FixAlloc

// Per-P, per order stack segment cache size.
internal static UntypedInt _StackCacheSize => /* 32 * 1024 */ 32768;

// Number of orders that get caching. Order 0 is FixedStack
// and each successive order is twice as large.
// We want to cache 2KB, 4KB, 8KB, and 16KB stacks. Larger stacks
// will be allocated directly.
// Since FixedStack is different on different systems, we
// must vary NumStackOrders to keep the same maximum cached size.
//   OS               | FixedStack | NumStackOrders
//   -----------------+------------+---------------
//   linux/darwin/bsd | 2KB        | 4
//   windows/32       | 4KB        | 3
//   windows/64       | 8KB        | 2
//   plan9            | 4KB        | 3
internal static UntypedInt _NumStackOrders => /* 4 - goarch.PtrSize/4*goos.IsWindows - 1*goos.IsPlan9 */ 4;

// heapAddrBits is the number of bits in a heap address. On
// amd64, addresses are sign-extended beyond heapAddrBits. On
// other arches, they are zero-extended.
//
// On most 64-bit platforms, we limit this to 48 bits based on a
// combination of hardware and OS limitations.
//
// amd64 hardware limits addresses to 48 bits, sign-extended
// to 64 bits. Addresses where the top 16 bits are not either
// all 0 or all 1 are "non-canonical" and invalid. Because of
// these "negative" addresses, we offset addresses by 1<<47
// (arenaBaseOffset) on amd64 before computing indexes into
// the heap arenas index. In 2017, amd64 hardware added
// support for 57 bit addresses; however, currently only Linux
// supports this extension and the kernel will never choose an
// address above 1<<47 unless mmap is called with a hint
// address above 1<<47 (which we never do).
//
// arm64 hardware (as of ARMv8) limits user addresses to 48
// bits, in the range [0, 1<<48).
//
// ppc64, mips64, and s390x support arbitrary 64 bit addresses
// in hardware. On Linux, Go leans on stricter OS limits. Based
// on Linux's processor.h, the user address space is limited as
// follows on 64-bit architectures:
//
// Architecture  Name              Maximum Value (exclusive)
// ---------------------------------------------------------------------
// amd64         TASK_SIZE_MAX     0x007ffffffff000 (47 bit addresses)
// arm64         TASK_SIZE_64      0x01000000000000 (48 bit addresses)
// ppc64{,le}    TASK_SIZE_USER64  0x00400000000000 (46 bit addresses)
// mips64{,le}   TASK_SIZE64       0x00010000000000 (40 bit addresses)
// s390x         TASK_SIZE         1<<64 (64 bit addresses)
//
// These limits may increase over time, but are currently at
// most 48 bits except on s390x. On all architectures, Linux
// starts placing mmap'd regions at addresses that are
// significantly below 48 bits, so even if it's possible to
// exceed Go's 48 bit limit, it's extremely unlikely in
// practice.
//
// On 32-bit platforms, we accept the full 32-bit address
// space because doing so is cheap.
// mips32 only has access to the low 2GB of virtual memory, so
// we further limit it to 31 bits.
//
// On ios/arm64, although 64-bit pointers are presumably
// available, pointers are truncated to 33 bits in iOS <14.
// Furthermore, only the top 4 GiB of the address space are
// actually available to the application. In iOS >=14, more
// of the address space is available, and the OS can now
// provide addresses outside of those 33 bits. Pick 40 bits
// as a reasonable balance between address space usage by the
// page allocator, and flexibility for what mmap'd regions
// we'll accept for the heap. We can't just move to the full
// 48 bits because this uses too much address space for older
// iOS versions.
// TODO(mknyszek): Once iOS <14 is deprecated, promote ios/arm64
// to a 48-bit address space like every other arm64 platform.
//
// WebAssembly currently has a limit of 4GB linear memory.
internal static UntypedInt heapAddrBits => /* (_64bit*(1-goarch.IsWasm)*(1-goos.IsIos*goarch.IsArm64))*48 + (1-_64bit+goarch.IsWasm)*(32-(goarch.IsMips+goarch.IsMipsle)) + 40*goos.IsIos*goarch.IsArm64 */ 48;

// maxAlloc is the maximum size of an allocation. On 64-bit,
// it's theoretically possible to allocate 1<<heapAddrBits bytes. On
// 32-bit, however, this is one less than 1<<32 because the
// number of bytes in the address space doesn't actually fit
// in a uintptr.
internal static UntypedInt maxAlloc => /* (1 << heapAddrBits) - (1-_64bit)*1 */ 281474976710656;

// The number of bits in a heap address, the size of heap
// arenas, and the L1 and L2 arena map sizes are related by
//
//   (1 << addr bits) = arena size * L1 entries * L2 entries
//
// Currently, we balance these as follows:
//
//       Platform  Addr bits  Arena size  L1 entries   L2 entries
// --------------  ---------  ----------  ----------  -----------
//       */64-bit         48        64MB           1    4M (32MB)
// windows/64-bit         48         4MB          64    1M  (8MB)
//      ios/arm64         40         4MB           1  256K  (2MB)
//       */32-bit         32         4MB           1  1024  (4KB)
//     */mips(le)         31         4MB           1   512  (2KB)
//           wasm         32       512KB           1  8192 (64KB)

// heapArenaBytes is the size of a heap arena. The heap
// consists of mappings of size heapArenaBytes, aligned to
// heapArenaBytes. The initial heap mapping is one arena.
//
// This is currently 64MB on 64-bit non-Windows, 4MB on
// 32-bit and on Windows, and 512KB on Wasm. We use smaller
// arenas on Windows because all committed memory is charged
// to the process, even if it's not touched. Hence, for
// processes with small heaps, the mapped arena space needs
// to be commensurate. This is particularly important with
// the race detector, since it significantly amplifies the
// cost of committed memory. We use smaller arenas on Wasm
// because some Wasm programs have very small heap, and
// everything in the Wasm linear memory is charged.
internal static UntypedInt heapArenaBytes => /* 1 << logHeapArenaBytes */ 67108864;

internal static UntypedInt heapArenaWords => /* heapArenaBytes / goarch.PtrSize */ 8388608;

// logHeapArenaBytes is log_2 of heapArenaBytes. For clarity,
// prefer using heapArenaBytes where possible (we need the
// constant to compute some other constants).
internal static UntypedInt logHeapArenaBytes => /* (6+20)*(_64bit*(1-goos.IsWindows)*(1-goarch.IsWasm)*(1-goos.IsIos*goarch.IsArm64)) + (2+20)*(_64bit*goos.IsWindows) + (2+20)*(1-_64bit) + (9+10)*goarch.IsWasm + (2+20)*goos.IsIos*goarch.IsArm64 */ 26;

// heapArenaBitmapWords is the size of each heap arena's bitmap in uintptrs.
internal static UntypedInt heapArenaBitmapWords => /* heapArenaWords / (8 * goarch.PtrSize) */ 131072;

internal static UntypedInt pagesPerArena => /* heapArenaBytes / pageSize */ 8192;

// arenaL1Bits is the number of bits of the arena number
// covered by the first level arena map.
//
// This number should be small, since the first level arena
// map requires PtrSize*(1<<arenaL1Bits) of space in the
// binary's BSS. It can be zero, in which case the first level
// index is effectively unused. There is a performance benefit
// to this, since the generated code can be more efficient,
// but comes at the cost of having a large L2 mapping.
//
// We use the L1 map on 64-bit Windows because the arena size
// is small, but the address space is still 48 bits, and
// there's a high cost to having a large L2.
internal static UntypedInt arenaL1Bits => /* 6 * (_64bit * goos.IsWindows) */ 0;

// arenaL2Bits is the number of bits of the arena number
// covered by the second level arena index.
//
// The size of each arena map allocation is proportional to
// 1<<arenaL2Bits, so it's important that this not be too
// large. 48 bits leads to 32MB arena index allocations, which
// is about the practical threshold.
internal static UntypedInt arenaL2Bits => /* heapAddrBits - logHeapArenaBytes - arenaL1Bits */ 22;

// arenaL1Shift is the number of bits to shift an arena frame
// number by to compute an index into the first level arena map.
internal static UntypedInt arenaL1Shift => /* arenaL2Bits */ 22;

// arenaBits is the total bits in a combined arena map index.
// This is split between the index into the L1 arena map and
// the L2 arena map.
internal static UntypedInt arenaBits => /* arenaL1Bits + arenaL2Bits */ 22;

// arenaBaseOffset is the pointer value that corresponds to
// index 0 in the heap arena map.
//
// On amd64, the address space is 48 bits, sign extended to 64
// bits. This offset lets us handle "negative" addresses (or
// high addresses if viewed as unsigned).
//
// On aix/ppc64, this offset allows to keep the heapAddrBits to
// 48. Otherwise, it would be 60 in order to handle mmap addresses
// (in range 0x0a00000000000000 - 0x0afffffffffffff). But in this
// case, the memory reserved in (s *pageAlloc).init for chunks
// is causing important slowdowns.
//
// On other platforms, the user address space is contiguous
// and starts at 0, so no offset is necessary.
internal static UntypedInt arenaBaseOffset => /* 0xffff800000000000*goarch.IsAmd64 + 0x0a00000000000000*goos.IsAix */ 18446603336221196288;
// A typed version of this constant that will make it into DWARF (for viewcore).
internal static uintptr arenaBaseOffsetUintptr => /* uintptr(arenaBaseOffset) */ unchecked((uintptr)18446603336221196288);

// Max number of threads to run garbage collection.
// 2, 3, and 4 are all plausible maximums depending
// on the hardware details of the machine. The garbage
// collector scales well to 32 cpus.
internal static UntypedInt _MaxGcproc => 32;

// minLegalPointer is the smallest possible legal pointer.
// This is the smallest possible architectural page size,
// since we assume that the first page is never mapped.
//
// This should agree with minZeroPage in the compiler.
internal static uintptr minLegalPointer => 4096;

// minHeapForMetadataHugePages sets a threshold on when certain kinds of
// heap metadata, currently the arenas map L2 entries and page alloc bitmap
// mappings, are allowed to be backed by huge pages. If the heap goal ever
// exceeds this threshold, then huge pages are enabled.
//
// These numbers are chosen with the assumption that huge pages are on the
// order of a few MiB in size.
//
// The kind of metadata this applies to has a very low overhead when compared
// to address space used, but their constant overheads for small heaps would
// be very high if they were to be backed by huge pages (e.g. a few MiB makes
// a huge difference for an 8 MiB heap, but barely any difference for a 1 GiB
// heap). The benefit of huge pages is also not worth it for small heaps,
// because only a very, very small part of the metadata is used for small heaps.
//
// N.B. If the heap goal exceeds the threshold then shrinks to a very small size
// again, then huge pages will still be enabled for this mapping. The reason is that
// there's no point unless we're also returning the physical memory for these
// metadata mappings back to the OS. That would be quite complex to do in general
// as the heap is likely fragmented after a reduction in heap size.
internal static UntypedInt minHeapForMetadataHugePages => /* 1 << 30 */ 1073741824;

// randomizeHeapBase indicates if the heap base address should be randomized.
// See comment in mallocinit for how the randomization is performed.
internal const bool randomizeHeapBase = /* goexperiment.RandomizedHeapBase64 && goarch.PtrSize == 8 && !isSbrkPlatform && !raceenabled && !msanenabled && !asanenabled */ true;

// randHeapBasePrefixMask is used to extract the top byte of the randomized
// heap base address.
internal static uintptr randHeapBasePrefixMask => /* ^uintptr(0xff << (heapAddrBits - 8)) */ unchecked((uintptr)18446463698244468735);

// physPageSize is the size in bytes of the OS's physical pages.
// Mapping and unmapping operations must be done at multiples of
// physPageSize.
//
// This must be set by the OS init code (typically in osinit) before
// mallocinit.
internal static uintptr physPageSize;

// physHugePageSize is the size in bytes of the OS's default physical huge
// page size whose allocation is opaque to the application. It is assumed
// and verified to be a power of two.
//
// If set, this must be set by the OS init code (typically in osinit) before
// mallocinit. However, setting it at all is optional, and leaving the default
// value is always safe (though potentially less efficient).
//
// Since physHugePageSize is always assumed to be a power of two,
// physHugePageShift is defined as physHugePageSize == 1 << physHugePageShift.
// The purpose of physHugePageShift is to avoid doing divisions in
// performance critical functions.
internal static uintptr physHugePageSize;
internal static nuint physHugePageShift;

// heapRandSeed is a random value that is populated in mallocinit if
// randomizeHeapBase is set. It is used in mallocinit, and mheap.grow, to
// randomize the base heap address.
internal static uintptr heapRandSeed;
internal static nint heapRandSeedBitsRemaining;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notEnoughHeapRandSeedˢ = "not enough heapRandSeed bits remaining"u8;

internal static uintptr nextHeapRandBits(nint bits) {
    if (bits > heapRandSeedBitsRemaining) {
        @throw(notEnoughHeapRandSeedˢ);
    }
    var r = heapRandSeed.Rsh((int64)(64 - bits));
    heapRandSeed.LshAssign((int64)(bits));
    heapRandSeedBitsRemaining -= bits;
    return r;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badTinySizeClassˢ = "bad TinySizeClass"u8;
internal static readonly @string heapArenaBitmapWordsNotAˢ = "heapArenaBitmapWords not a power of 2"u8;
internal static readonly @string failedToGetSystemPageˢ = "failed to get system page size"u8;
internal static readonly @string badSystemPageSizeˢ = "bad system page size"u8;
internal static readonly @string badSystemHugePageSizeˢ = "bad system huge page size"u8;
internal static readonly @string badPagesPerSpanRootˢ = "bad pagesPerSpanRoot"u8;
internal static readonly @string badˢ = "bad pagesPerReclaimerChunk"u8;
internal static readonly @string minSizeOfMallocHeaderIsˢ = "min size of malloc header is not a size class boundary"u8;
internal static readonly @string expectedAllSizeClassesUpˢ = "expected all size classes up to min size for malloc header to fit in one-page spans"u8;
internal static readonly @string maxPointerScanBitmapSizeˢ = "max pointer/scan bitmap size for headerless objects is too large"u8;
internal static readonly @string tagBitsTooSmallˢ = "tagBits too small"u8;
internal static readonly @string riscv64VmaSizeIsUnknownˢ = "riscv64 vma size is unknown and race mode is enabled"u8;
internal static readonly @string heapReservationˢ = "heap reservation"u8;

internal static void mallocinit() {
    if (gc.SizeClassToSize[tinySizeClass] != maxTinySize) {
        @throw(badTinySizeClassˢ);
    }

    if ((UntypedInt)(heapArenaBitmapWords & (heapArenaBitmapWords - 1)) != 0) {
        // heapBits expects modular arithmetic on bitmap
        // addresses to work.
        @throw(heapArenaBitmapWordsNotAˢ);
    }

    // Check physPageSize.
    if (physPageSize == 0) {
        // The OS init code failed to fetch the physical page size.
        @throw(failedToGetSystemPageˢ);
    }
    if (physPageSize > maxPhysPageSize) {
        print((@string)"system page size ("u8, physPageSize, (@string)") is larger than maximum page size ("u8, (nint)(maxPhysPageSize), (@string)")\n"u8);
        @throw(badSystemPageSizeˢ);
    }
    if (physPageSize < minPhysPageSize) {
        print((@string)"system page size ("u8, physPageSize, (@string)") is smaller than minimum page size ("u8, (nint)(minPhysPageSize), (@string)")\n"u8);
        @throw(badSystemPageSizeˢ);
    }
    if ((uintptr)(physPageSize & (physPageSize - 1)) != 0) {
        print((@string)"system page size ("u8, physPageSize, (@string)") must be a power of 2\n"u8);
        @throw(badSystemPageSizeˢ);
    }
    if ((uintptr)(physHugePageSize & (physHugePageSize - 1)) != 0) {
        print((@string)"system huge page size ("u8, physHugePageSize, (@string)") must be a power of 2\n"u8);
        @throw(badSystemHugePageSizeˢ);
    }
    if (physHugePageSize > maxPhysHugePageSize) {
        // physHugePageSize is greater than the maximum supported huge page size.
        // Don't throw here, like in the other cases, since a system configured
        // in this way isn't wrong, we just don't have the code to support them.
        // Instead, silently set the huge page size to zero.
        physHugePageSize = 0;
    }
    if (physHugePageSize != 0) {
        // Since physHugePageSize is a power of 2, it suffices to increase
        // physHugePageShift until 1<<physHugePageShift == physHugePageSize.
        while (((uintptr)1).Lsh(physHugePageShift) != physHugePageSize) {
            physHugePageShift++;
        }
    }
    if (pagesPerArena % pagesPerSpanRoot != 0) {
        print((@string)"pagesPerArena ("u8, (nint)(pagesPerArena), (@string)") is not divisible by pagesPerSpanRoot ("u8, (nint)(pagesPerSpanRoot), (@string)")\n"u8);
        @throw(badPagesPerSpanRootˢ);
    }
    if (pagesPerArena % pagesPerReclaimerChunk != 0) {
        print((@string)"pagesPerArena ("u8, (nint)(pagesPerArena), (@string)") is not divisible by pagesPerReclaimerChunk ("u8, (nint)(pagesPerReclaimerChunk), (@string)")\n"u8);
        @throw(badˢ);
    }
    // Check that the minimum size (exclusive) for a malloc header is also
    // a size class boundary. This is important to making sure checks align
    // across different parts of the runtime.
    //
    // While we're here, also check to make sure all these size classes'
    // span sizes are one page. Some code relies on this.
    var minSizeForMallocHeaderIsSizeClass = false;
    var sizeClassesUpToMinSizeForMallocHeaderAreOnePage = true;
    for (nint i = 0; i < len(gc.SizeClassToSize); i++) {
        if (gc.SizeClassToNPages[i] > 1) {
            sizeClassesUpToMinSizeForMallocHeaderAreOnePage = false;
        }
        if (gc.MinSizeForMallocHeader == (uintptr)gc.SizeClassToSize[i]) {
            minSizeForMallocHeaderIsSizeClass = true;
            break;
        }
    }
    if (!minSizeForMallocHeaderIsSizeClass) {
        @throw(minSizeOfMallocHeaderIsˢ);
    }
    if (!sizeClassesUpToMinSizeForMallocHeaderAreOnePage) {
        @throw(expectedAllSizeClassesUpˢ);
    }
    // Check that the pointer bitmap for all small sizes without a malloc header
    // fits in a word.
    if (gc.MinSizeForMallocHeader / goarch.PtrSize > 8 * goarch.PtrSize) {
        @throw(maxPointerScanBitmapSizeˢ);
    }

    if (minTagBits > tagBits) {
        @throw(tagBitsTooSmallˢ);
    }

    // Initialize the heap.
    Ꮡmheap_.init();
    mcache0 = allocmcache();
    lockInit(ref gcBitsArenas.@lock, lockRankGcBitsArenas);
    lockInit(ref profInsertLock, lockRankProfInsert);
    lockInit(ref profBlockLock, lockRankProfBlock);
    lockInit(ref profMemActiveLock, lockRankProfMemActive);
    foreach (var (i, _) in profMemFutureLock) {
        lockInit(ref profMemFutureLock[i], lockRankProfMemFuture);
    }
    lockInit(ref globalAlloc.mutex, lockRankGlobalAlloc);

    // Create initial arena growth hints.
    if (isSbrkPlatform){
        // Don't generate hints on sbrk platforms. We can
        // only grow the break sequentially.
    } else 
    if (goarch.PtrSize == 8){
        // On a 64-bit machine, we pick the following hints
        // because:
        //
        // 1. Starting from the middle of the address space
        // makes it easier to grow out a contiguous range
        // without running in to some other mapping.
        //
        // 2. This makes Go heap addresses more easily
        // recognizable when debugging.
        //
        // 3. Stack scanning in gccgo is still conservative,
        // so it's important that addresses be distinguishable
        // from other data.
        //
        // Starting at 0x00c0 means that the valid memory addresses
        // will begin 0x00c0, 0x00c1, ...
        // In little-endian, that's c0 00, c1 00, ... None of those are valid
        // UTF-8 sequences, and they are otherwise as far away from
        // ff (likely a common byte) as possible. If that fails, we try other 0xXXc0
        // addresses. An earlier attempt to use 0x11f8 caused out of memory errors
        // on OS X during thread allocations.  0x00c0 causes conflicts with
        // AddressSanitizer which reserves all memory up to 0x0100.
        // These choices reduce the odds of a conservative garbage collector
        // not collecting memory because some non-pointer block of memory
        // had a bit pattern that matched a memory address.
        //
        // However, on arm64, we ignore all this advice above and slam the
        // allocation at 0x40 << 32 because when using 4k pages with 3-level
        // translation buffers, the user address space is limited to 39 bits
        // On ios/arm64, the address space is even smaller.
        //
        // On AIX, mmaps starts at 0x0A00000000000000 for 64-bit.
        // processes.
        //
        // Space mapped for user arenas comes immediately after the range
        // originally reserved for the regular heap when race mode is not
        // enabled because user arena chunks can never be used for regular heap
        // allocations and we want to avoid fragmenting the address space.
        //
        // In race mode we have no choice but to just use the same hints because
        // the race detector requires that the heap be mapped contiguously.
        //
        // If randomizeHeapBase is set, we attempt to randomize the base address
        // as much as possible. We do this by generating a random uint64 via
        // bootstrapRand and using it's bits to randomize portions of the base
        // address as follows:
        //   * We first generate a random heapArenaBytes aligned address that we use for
        //     generating the hints.
        //   * On the first call to mheap.grow, we then generate a random PallocChunkBytes
        //     aligned offset into the mmap'd heap region, which we use as the base for
        //     the heap region.
        //   * We then select a page offset in that PallocChunkBytes region to start the
        //     heap at, and mark all the pages up to that offset as allocated.
        //
        // Our final randomized "heap base address" becomes the first byte of
        // the first available page returned by the page allocator. This results
        // in an address with at least heapAddrBits-gc.PageShift-2-(1*goarch.IsAmd64)
        // bits of entropy.

        uintptr randHeapBase = default!;
        byte randHeapBasePrefix = default!;
        // heapAddrBits is 48 on most platforms, but we only use 47 of those
        // bits in order to provide a good amount of room for the heap to grow
        // contiguously. On amd64, there are 48 bits, but the top bit is sign
        // extended, so we throw away another bit, just to be safe.
        nint randHeapAddrBits = heapAddrBits - 1 - (goarch.IsAmd64 * 1);
        if (randomizeHeapBase) {
            // Generate a random value, and take the bottom heapAddrBits-logHeapArenaBytes
            // bits, using them as the top bits for randHeapBase.
            (heapRandSeed, heapRandSeedBitsRemaining) = ((uintptr)bootstrapRand(), 64);

            nint topBits = (randHeapAddrBits - (nint)logHeapArenaBytes);
            randHeapBase = nextHeapRandBits(topBits).Lsh((int64)(randHeapAddrBits - topBits));
            randHeapBase = alignUp(randHeapBase, heapArenaBytes);
            randHeapBasePrefix = (byte)(randHeapBase.Rsh((int64)(randHeapAddrBits - 8)));
        }

        nint vmaSize = default!;
        if (GOARCH == "riscv64"u8) {
            // Identify which memory layout is in use based on the system
            // stack address, knowing that the bottom half of virtual memory
            // is user space. This should result in 39, 48 or 57. It may be
            // possible to use RISCV_HWPROBE_KEY_HIGHEST_VIRT_ADDRESS at some
            // point in the future - for now use the system stack address.
            vmaSize = sys.Len64((uint64)(~(~(~getg()).m).g0).stack.hi) + 1;
            if (raceenabled && vmaSize != 39 && vmaSize != 48) {
                println((@string)"vma size = "u8, vmaSize);
                @throw(riscv64VmaSizeIsUnknownˢ);
            }
        }

        for (nint i = 0x7f; i >= 0; i--) {
            uintptr Δp = default!;
            switch (ᐧ) {
            case {} when raceenabled && GOARCH == "riscv64"u8 && vmaSize == 39: {
                Δp = (uintptr)((uintptr)i << 28 | (uintptr)(uintptr)((uintptr)uintptrMask & (uintptr)(5100273664L)));
                if (Δp >= (uintptr)((uintptr)uintptrMask & (uintptr)unchecked((nint)0x000f00000000L))) {
                    continue;
                }
                break;
            }
            case {} when raceenabled: {
                // The TSAN runtime requires the heap
                // to be in the range [0x00c000000000,
                // 0x00e000000000).
                Δp = (uintptr)((uintptr)i << 32 | (uintptr)(uintptr)((uintptr)uintptrMask & (uintptr)(824633720832L)));
                if (Δp >= (uintptr)((uintptr)uintptrMask & (uintptr)unchecked((nint)0x00e000000000L))) {
                    continue;
                }
                break;
            }
            case {} when ᐧᐧ: {
                var prefix = ((uintptr)((byte)(randHeapBasePrefix + (byte)i))).Lsh((int64)(randHeapAddrBits - 8));
                Δp = (uintptr)(prefix | ((uintptr)(randHeapBase & randHeapBasePrefixMask)));
                break;
            }
            case {} when GOARCH == "arm64"u8 && GOOS == "ios"u8: {
                Δp = (uintptr)((uintptr)i << 40 | (uintptr)(uintptr)((uintptr)uintptrMask & (uintptr)(5100273664L)));
                break;
            }
            case {} when GOARCH == "arm64"u8: {
                Δp = (uintptr)((uintptr)i << 40 | (uintptr)(uintptr)((uintptr)uintptrMask & (uintptr)(274877906944L)));
                break;
            }
            case {} when GOARCH == "riscv64"u8 && vmaSize == 39: {
                Δp = (uintptr)((uintptr)i << 32 | (uintptr)(uintptr)((uintptr)uintptrMask & (uintptr)(5100273664L)));
                break;
            }
            case {} when GOOS == "aix"u8: {
                if (i == 0) {
                    // We don't use addresses directly after 0x0A00000000000000
                    // to avoid collisions with others mmaps done by non-go programs.
                    continue;
                }
                Δp = (uintptr)((uintptr)i << 40 | (uintptr)(uintptr)((uintptr)uintptrMask & (uintptr)(720575940379279360L)));
                break;
            }
            default: {
                Δp = (uintptr)((uintptr)i << 40 | (uintptr)(uintptr)((uintptr)uintptrMask & (uintptr)(824633720832L)));
                break;
            }}
            // Switch to generating hints for user arenas if we've gone
            // through about half the hints. In race mode, take only about
            // a quarter; we don't have very much space to work with.
            var hintList = Ꮡmheap_.of(mheap.ᏑarenaHints);
            if ((!raceenabled && i > 0x3f) || (raceenabled && i > 0x5f)) {
                hintList = Ꮡmheap_.of(mheap.ᏑuserArena).of(mheap_userArena.ᏑarenaHints);
            }
            var hint = (ж<arenaHint>)(uintptr)(Ꮡmheap_.of(mheap.ᏑarenaHintAlloc).alloc());
            hint.addr = Δp;
            (hint.next, hintList.ValueSlot) = (hintList.ValueSlot, hint);
        }
    } else {
        // On a 32-bit machine, we're much more concerned
        // about keeping the usable heap contiguous.
        // Hence:
        //
        // 1. We reserve space for all heapArenas up front so
        // they don't get interleaved with the heap. They're
        // ~258MB, so this isn't too bad. (We could reserve a
        // smaller amount of space up front if this is a
        // problem.)
        //
        // 2. We hint the heap to start right above the end of
        // the binary so we have the best chance of keeping it
        // contiguous.
        //
        // 3. We try to stake out a reasonably large initial
        // heap reservation.

        uintptr arenaMetaSize = /* (1 << arenaBits) * unsafe.Sizeof(heapArena{}) */ unchecked((uintptr)292124884992);
        var meta = (uintptr)(uintptr)sysReserve(nil, arenaMetaSize, heapReservationˢ);
        if (meta != 0) {
            Ꮡmheap_.of(mheap.ᏑheapArenaAlloc).init(meta, arenaMetaSize, true);
        }

        // We want to start the arena low, but if we're linked
        // against C code, it's possible global constructors
        // have called malloc and adjusted the process' brk.
        // Query the brk so we can avoid trying to map the
        // region over it (which will cause the kernel to put
        // the region somewhere else, likely at a high
        // address).
        var procBrk = sbrk0();

        // If we ask for the end of the data segment but the
        // operating system requires a little more space
        // before we can start allocating, it will give out a
        // slightly higher pointer. Except QEMU, which is
        // buggy, as usual: it won't adjust the pointer
        // upward. So adjust it upward a little bit ourselves:
        // 1/4 MB to get away from the running binary image.
        var Δp = firstmoduledata.end;
        if (Δp < procBrk) {
            Δp = procBrk;
        }
        if (mheap_.heapArenaAlloc.next <= Δp && Δp < mheap_.heapArenaAlloc.end) {
            Δp = mheap_.heapArenaAlloc.end;
        }
        Δp = alignUp(Δp + ((uintptr)256 << 10), heapArenaBytes);
        // Because we're worried about fragmentation on
        // 32-bit, we try to make a large initial reservation.
        var arenaSizes = new uintptr[]{
            (uintptr)512 << 20,
            (uintptr)256 << 20,
            (uintptr)128 << 20
        }.slice();
        foreach (var (_, arenaSize) in arenaSizes) {
            var (a, size) = sysReserveAligned((@unsafe.Pointer)Δp, arenaSize, heapArenaBytes, heapReservationˢ);
            if (a != nil) {
                Ꮡmheap_.of(mheap.Ꮡarena).init((uintptr)a, size, false);
                Δp = mheap_.arena.end; // For hint below
                break;
            }
        }
        var hint = (ж<arenaHint>)(uintptr)(Ꮡmheap_.of(mheap.ᏑarenaHintAlloc).alloc());
        hint.addr = Δp;
        (hint.next, mheap_.arenaHints) = (mheap_.arenaHints, hint);

        // Place the hint for user arenas just after the large reservation.
        //
        // While this potentially competes with the hint above, in practice we probably
        // aren't going to be getting this far anyway on 32-bit platforms.
        var userArenaHint = (ж<arenaHint>)(uintptr)(Ꮡmheap_.of(mheap.ᏑarenaHintAlloc).alloc());
        userArenaHint.addr = Δp;
        (userArenaHint.next, mheap_.userArena.arenaHints) = (mheap_.userArena.arenaHints, userArenaHint);
    }
    // Initialize the memory limit here because the allocator is going to look at it
    // but we haven't called gcinit yet and we're definitely going to allocate memory before then.
    ᏑgcController.of(gcControllerState.ᏑmemoryLimit).Store(Δmath.MaxInt64);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string heapˢ = "heap"u8;
internal static readonly @string tooManyAddressSpaceˢ = "too many address space collisions for -race mode"u8;
internal static readonly @string regionExceedsUintptrˢ = "region exceeds uintptr range"u8;
internal static readonly @string baseOutsideUsableAddressˢ = "base outside usable address space"u8;
internal static readonly @string endOutsideUsableAddressˢ = "end outside usable address space"u8;
internal static readonly @string memoryReservationExceedsˢ = "memory reservation exceeds address space limit"u8;
internal static readonly @string misroundedAllocationInˢ = "misrounded allocation in sysAlloc"u8;
internal static readonly @string heapIndexˢ = "heap index"u8;
internal static readonly @string outOfMemoryAllocatingˢ = "out of memory allocating heap arena map"u8;
internal static readonly @string arenaAlreadyInitializedˢ = "arena already initialized"u8;
internal static readonly @string heapMetadataˢ = "heap metadata"u8;
internal static readonly @string outOfMemoryAllocatingˢ2 = "out of memory allocating heap arena metadata"u8;
internal static readonly @string outOfMemoryAllocatingˢ3 = "out of memory allocating allArenas"u8;

// sysAlloc allocates heap arena space for at least n bytes. The
// returned pointer is always heapArenaBytes-aligned and backed by
// h.arenas metadata. The returned size is always a multiple of
// heapArenaBytes. sysAlloc returns nil on failure.
// There is no corresponding free function.
//
// hintList is a list of hint addresses for where to allocate new
// heap arenas. It must be non-nil.
//
// sysAlloc returns a memory region in the Reserved state. This region must
// be transitioned to Prepared and then Ready before use.
//
// arenaList is the list the arena should be added to.
//
// h must be locked.
internal static (@unsafe.Pointer v, uintptr size) sysAlloc(this ж<mheap> Ꮡh, uintptr n, ж<ж<arenaHint>> ᏑhintList, ж<slice<arenaIdx>> ᏑarenaList) {
    @unsafe.Pointer v = default!;
    uintptr size = default!;
    ref var h = ref Ꮡh.DerefOrNull();
    ref var hintList = ref ᏑhintList.DerefOrNull();
    ref var arenaList = ref ᏑarenaList.DerefOrNull();

    assertLockHeld(Ꮡh.of(mheap.Ꮡlock));

    n = alignUp(n, heapArenaBytes);

    if (ᏑhintList == Ꮡh.of(mheap.ᏑarenaHints)) {
        // First, try the arena pre-reservation.
        // Newly-used mappings are considered released.
        //
        // Only do this if we're using the regular heap arena hints.
        // This behavior is only for the heap.
        v = (uintptr)h.arena.alloc(n, heapArenaBytes, ᏑgcController.of(gcControllerState.ᏑheapReleased), heapˢ);
        if (v != nil) {
            size = n;
            goto mapped;
        }
    }

    // Try to grow the heap at a hint address.
    while (hintList != nil) {
        var hint = hintList;
        var Δp = hint.Value.addr;
        if ((~hint).down) {
            Δp -= n;
        }
        if (Δp + n < Δp){
            // We can't use this, so don't ask.
            v = default!;
        } else 
        if (arenaIndex(Δp + n - 1) >= (nuint)((arenaIdx)((nuint)1 << (int)(arenaBits)))){
            // Outside addressable heap. Can't use.
            v = default!;
        } else {
            v = (uintptr)sysReserve((@unsafe.Pointer)Δp, n, heapReservationˢ);
        }
        if (Δp == (uintptr)v) {
            // Success. Update the hint.
            if (!(~hint).down) {
                Δp += n;
            }
            hint.Value.addr = Δp;
            size = n;
            break;
        }
        // Failed. Discard this hint and try the next.
        //
        // TODO: This would be cleaner if sysReserve could be
        // told to only return the requested address. In
        // particular, this is already how Windows behaves, so
        // it would simplify things there.
        if (v != nil) {
            sysUnreserve(v, n);
        }
        hintList = hint.Value.next;
        h.arenaHintAlloc.free(@unsafe.Pointer.FromPinnedBox(hint));
    }

    if (size == 0) {
        if (raceenabled) {
            // The race detector assumes the heap lives in
            // [0x00c000000000, 0x00e000000000), but we
            // just ran out of hints in this region. Give
            // a nice failure.
            @throw(tooManyAddressSpaceˢ);
        }

        // All of the hints failed, so we'll take any
        // (sufficiently aligned) address the kernel will give
        // us.
        (v, size) = sysReserveAligned(nil, n, heapArenaBytes, heapˢ);
        if (v == nil) {
            return (default!, 0);
        }

        // Create new hints for extending this region.
        var hint = (ж<arenaHint>)(uintptr)(h.arenaHintAlloc.alloc());
        (hint.addr, hint.down) = ((uintptr)v, true);
        (hint.next, mheap_.arenaHints) = (mheap_.arenaHints, hint);
        hint = (ж<arenaHint>)(uintptr)(h.arenaHintAlloc.alloc());
        hint.addr = (uintptr)v + size;
        (hint.next, mheap_.arenaHints) = (mheap_.arenaHints, hint);
    }

    // Check for bad pointers or pointers we can't use.
    {
        @string bad = default!;
        var Δp = (uintptr)v;
        if (Δp + size < Δp){
            bad = regionExceedsUintptrˢ;
        } else 
        if (arenaIndex(Δp) >= (nuint)((arenaIdx)((nuint)1 << (int)(arenaBits)))){
            bad = baseOutsideUsableAddressˢ;
        } else 
        if (arenaIndex(Δp + size - 1) >= (nuint)((arenaIdx)((nuint)1 << (int)(arenaBits)))) {
            bad = endOutsideUsableAddressˢ;
        }
        if (bad != ""u8) {
            // This should be impossible on most architectures,
            // but it would be really confusing to debug.
            print((@string)"runtime: memory allocated by OS ["u8, (Δhex)(uint64)Δp, (@string)", "u8, (Δhex)(uint64)(Δp + size), (@string)") not in usable address space: "u8, bad, (@string)"\n"u8);
            @throw(memoryReservationExceedsˢ);
        }
    }

    if ((uintptr)((uintptr)v & (uintptr)(heapArenaBytes - 1)) != 0) {
        @throw(misroundedAllocationInˢ);
    }

mapped:
    if (valgrindenabled) {
        valgrindCreateMempool(v);
        valgrindMakeMemNoAccess(v, size);
    }

    // Create arena metadata.
    for (arenaIdx ri = arenaIndex((uintptr)v); ri <= arenaIndex((uintptr)v + size - 1); ri++) {
        var l2 = h.arenas[ri.l1()];
        if (l2 == nil) {
            // Allocate an L2 arena map.
            //
            // Use sysAllocOS instead of sysAlloc or persistentalloc because there's no
            // statistic we can comfortably account for this space in. With this structure,
            // we rely on demand paging to avoid large overheads, but tracking which memory
            // is paged in is too expensive. Trying to account for the whole region means
            // that it will appear like an enormous memory overhead in statistics, even though
            // it is not.
            l2 = (ж<array<ж<heapArena>>>)(uintptr)(sysAllocOS(/* unsafe.Sizeof(*l2) */ (uintptr)33554432, heapIndexˢ));
            if (l2 == nil) {
                @throw(outOfMemoryAllocatingˢ);
            }
            if (h.arenasHugePages){
                sysHugePage(@unsafe.Pointer.FromPinnedBox(l2), /* unsafe.Sizeof(*l2) */ (uintptr)33554432);
            } else {
                sysNoHugePage(@unsafe.Pointer.FromPinnedBox(l2), /* unsafe.Sizeof(*l2) */ (uintptr)33554432);
            }
            atomic.StorepNoWB(@unsafe.Pointer.FromBox(Ꮡ(h.arenas, ri.l1())), @unsafe.Pointer.FromPinnedBox(l2));
        }

        if (l2.Value[ri.l2()] != nil) {
            @throw(arenaAlreadyInitializedˢ);
        }
        ж<heapArena> r = default!;
        r = (ж<heapArena>)(uintptr)(h.heapArenaAlloc.alloc(/* unsafe.Sizeof(*r) */ (uintptr)69648, goarch.PtrSize, Ꮡmemstats.of(mstats.ᏑgcMiscSys), heapMetadataˢ));
        if (r == nil) {
            r = (ж<heapArena>)(uintptr)(persistentalloc(/* unsafe.Sizeof(*r) */ (uintptr)69648, goarch.PtrSize, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
            if (r == nil) {
                @throw(outOfMemoryAllocatingˢ2);
            }
        }

        // Register the arena in allArenas if requested.
        if (len((arenaList)) == cap((arenaList))) {
            var sizeΔ1 = 2 * (uintptr)cap((arenaList)) * (uintptr)goarch.PtrSize;
            if (sizeΔ1 == 0) {
                sizeΔ1 = physPageSize;
            }
            var newArray = (ж<notInHeap>)(uintptr)(persistentalloc(sizeΔ1, goarch.PtrSize, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
            if (newArray == nil) {
                @throw(outOfMemoryAllocatingˢ3);
            }
            var oldSlice = (arenaList);
            (Ꮡ((arenaList)).Reinterpret<slice<arenaIdx>, notInHeapSlice>()).Value = new notInHeapSlice(newArray, len((arenaList)), (nint)(sizeΔ1 / (uintptr)goarch.PtrSize));
            copy((arenaList), oldSlice);
            // Do not free the old backing array because
            // there may be concurrent readers. Since we
            // double the array each time, this can lead
            // to at most 2x waste.
        }
        (arenaList) = (arenaList).slice(0, len((arenaList)) + 1);
        (arenaList)[len((arenaList)) - 1] = ri;

        // Store atomically just in case an object from the
        // new heap arena becomes visible before the heap lock
        // is released (which shouldn't happen, but there's
        // little downside to this).
        atomic.StorepNoWB(@unsafe.Pointer.FromBox(l2.at<ж<heapArena>>((ulong)(ri.l2()))), @unsafe.Pointer.FromPinnedBox(r));
    }

    // Tell the race detector about the new heap memory.
    if (raceenabled) {
        racemapshadow(v, size);
    }

    return (v, size);
}

// enableMetadataHugePages enables huge pages for various sources of heap metadata.
//
// A note on latency: for sufficiently small heaps (<10s of GiB) this function will take constant
// time, but may take time proportional to the size of the mapped heap beyond that.
//
// This function is idempotent.
//
// The heap lock must not be held over this operation, since it will briefly acquire
// the heap lock.
//
// Must be called on the system stack because it acquires the heap lock.
//
//go:systemstack
internal static void enableMetadataHugePages(this ж<mheap> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    // Enable huge pages for page structure.
    h.pages.enableChunkHugePages();

    // Grab the lock and set arenasHugePages if it's not.
    //
    // Once arenasHugePages is set, all new L2 entries will be eligible for
    // huge pages. We'll set all the old entries after we release the lock.
    @lock(Ꮡh.of(mheap.Ꮡlock));
    if (h.arenasHugePages) {
        unlock(Ꮡh.of(mheap.Ꮡlock));
        return;
    }
    h.arenasHugePages = true;
    unlock(Ꮡh.of(mheap.Ꮡlock));

    // N.B. The arenas L1 map is quite small on all platforms, so it's fine to
    // just iterate over the whole thing.
    foreach (var (i, _) in h.arenas) {
        var l2 = (ж<array<ж<heapArena>>>)(uintptr)(atomic.Loadp(@unsafe.Pointer.FromBox(Ꮡ(h.arenas, i))));
        if (l2 == nil) {
            continue;
        }
        sysHugePage(@unsafe.Pointer.FromPinnedBox(l2), /* unsafe.Sizeof(*l2) */ (uintptr)33554432);
    }
}

// base address for all 0-byte allocations
// go2cs generated this placeholder — var zerobase is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// nextFreeFast returns the next free object if one is quickly available.
// Otherwise it returns 0.
internal static gclinkptr nextFreeFast(ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    nint theBit = sys.TrailingZeros64(s.allocCache); // Is there a free object in the allocCache?
    if (theBit < 64) {
        var result = (uint16)(s.freeindex + (uint16)theBit);
        if (result < s.nelems) {
            var freeidx = (uint16)(result + 1);
            if ((uint16)(freeidx % 64) == 0 && freeidx != s.nelems) {
                return 0;
            }
            s.allocCache.RshAssign((nuint)(theBit + 1));
            s.freeindex = freeidx;
            s.allocCount++;
            return (gclinkptr)((uintptr)result * s.elemsize + s.@base());
        }
    }
    return 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sAllocCountSNelemsˢ = "s.allocCount != s.nelems && freeIndex == s.nelems"u8;
internal static readonly @string freeIndexIsNotValidˢ = "freeIndex is not valid"u8;
internal static readonly @string sAllocCountSNelemsˢ2 = "s.allocCount > s.nelems"u8;

// nextFree returns the next free object from the cached span if one is available.
// Otherwise it refills the cache with a span with an available object and
// returns that object along with a flag indicating that this was a heavy
// weight allocation. If it is a heavy weight allocation the caller must
// determine whether a new GC cycle needs to be started or if the GC is active
// whether this goroutine needs to assist the GC.
//
// Must run in a non-preemptible context since otherwise the owner of
// c could change.
internal static (gclinkptr v, ж<mspan> s, bool checkGCTrigger) nextFree(this ref mcache c, spanClass spc) {
    gclinkptr v = default!;
    ж<mspan> s = default!;
    bool checkGCTrigger = default!;

    s = c.alloc[spc];
    checkGCTrigger = false;
    var freeIndex = s.nextFreeIndex();
    if (freeIndex == (~s).nelems) {
        // The span is full.
        if ((~s).allocCount != (~s).nelems) {
            println((@string)"runtime: s.allocCount="u8, (~s).allocCount, (@string)"s.nelems="u8, (~s).nelems);
            @throw(sAllocCountSNelemsˢ);
        }
        c.refill(spc);
        checkGCTrigger = true;
        s = c.alloc[spc];

        freeIndex = s.nextFreeIndex();
    }

    if (freeIndex >= (~s).nelems) {
        @throw(freeIndexIsNotValidˢ);
    }

    v = (gclinkptr)((uintptr)freeIndex * (~s).elemsize + s.@base());
    s.Value.allocCount++;
    if ((~s).allocCount > (~s).nelems) {
        println((@string)"s.allocCount="u8, (~s).allocCount, (@string)"s.nelems="u8, (~s).nelems);
        @throw(sAllocCountSNelemsˢ2);
    }
    return (v, s, checkGCTrigger);
}

// doubleCheckMalloc enables a bunch of extra checks to malloc to double-check
// that various invariants are upheld.
//
// We might consider turning these on by default; many of them previously were.
// They account for a few % of mallocgc's cost though, which does matter somewhat
// at scale. (When testing changes to malloc, consider enabling this, and also
// some function-local 'doubleCheck' consts such as in mbitmap.go currently.)
internal const bool doubleCheckMalloc = false;

// sizeSpecializedMallocEnabled is the set of conditions where we enable the size-specialized
// mallocgc implementation: the experiment must be enabled, and none of the sanitizers should
// be enabled. The tables used to select the size-specialized malloc function do not compile
// properly on plan9, so size-specialized malloc is also disabled on plan9.
internal const bool sizeSpecializedMallocEnabled = /* goexperiment.SizeSpecializedMalloc && GOOS != "plan9" && !asanenabled && !raceenabled && !msanenabled && !valgrindenabled */ false;

// runtimeFreegcEnabled is the set of conditions where we enable the runtime.freegc
// implementation and the corresponding allocation-related changes: the experiment must be
// enabled, and none of the memory sanitizers should be enabled. We allow the race detector,
// in contrast to sizeSpecializedMallocEnabled.
// TODO(thepudds): it would be nice to check Valgrind integration, though there are some hints
// there might not be any canned tests in tree for Go's integration with Valgrind.
internal const bool runtimeFreegcEnabled = /* goexperiment.RuntimeFreegc && !asanenabled && !msanenabled && !valgrindenabled */ false;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mallocgcCalledWithˢ = "mallocgc called with gcphase == _GCmarktermination"u8;
internal static readonly @string objectsWithPointersMustˢ = "objects with pointers must be zeroed"u8;

// Allocate an object of size bytes.
// Small objects are allocated from the per-P cache's free lists.
// Large objects (> 32 kB) are allocated straight from the heap.
//
// mallocgc should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/gopkg
//   - github.com/bytedance/sonic
//   - github.com/cloudwego/frugal
//   - github.com/cockroachdb/cockroach
//   - github.com/cockroachdb/pebble
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mallocgc
internal static @unsafe.Pointer mallocgc(uintptr size, ж<_type> Ꮡtyp, bool needzero) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (doubleCheckMalloc) {
        if (gcphase == _GCmarktermination) {
            @throw(mallocgcCalledWithˢ);
        }
    }

    // Short-circuit zero-sized allocation requests.
    if (size == 0) {
        return @unsafe.Pointer.FromBox(Ꮡzerobase);
    }

    if (sizeSpecializedMallocEnabled && heapBitsInSpan(size)) {
        if (Ꮡtyp == nil || !typ.Pointers()){
            return (uintptr)mallocNoScanTable[size](size, Ꮡtyp, needzero);
        } else {
            if (!needzero) {
                @throw(objectsWithPointersMustˢ);
            }
            return (uintptr)mallocScanTable[size](size, Ꮡtyp, needzero);
        }
    }

    // It's possible for any malloc to trigger sweeping, which may in
    // turn queue finalizers. Record this dynamic lock edge.
    // N.B. Compiled away if lockrank experiment is not enabled.
    lockRankMayQueueFinalizer();

    // Pre-malloc debug hooks.
    if (debug.malloc) {
        {
            @unsafe.Pointer xΔ1 = (uintptr)preMallocgcDebug(size, Ꮡtyp); if (xΔ1 != nil) {
                return xΔ1;
            }
        }
    }

    // For ASAN, we allocate extra memory around each allocation called the "redzone."
    // These "redzones" are marked as unaddressable.
    uintptr asanRZ = default!;
    if (asanenabled) {
        asanRZ = redZoneSize(size);
        size += asanRZ;
    }

    // Assist the GC if needed. (On the reuse path, we currently compensate for this;
    // changes here might require changes there.)
    if (gcBlackenEnabled != 0) {
        deductAssistCredit(size);
    }

    // Actually do the allocation.
    @unsafe.Pointer x = default!;
    uintptr elemsize = default!;
    if (sizeSpecializedMallocEnabled){
        // we know that heapBitsInSpan is false.
        if (size <= (uintptr)(maxSmallSize - gc.MallocHeaderSize)){
            if (Ꮡtyp == nil || !typ.Pointers()){
                (x, elemsize) = mallocgcSmallNoscan(size, Ꮡtyp, needzero);
            } else {
                if (!needzero) {
                    @throw(objectsWithPointersMustˢ);
                }
                (x, elemsize) = mallocgcSmallScanHeader(size, Ꮡtyp);
            }
        } else {
            (x, elemsize) = mallocgcLarge(size, Ꮡtyp, needzero);
        }
    } else {
        if (size <= (uintptr)(maxSmallSize - gc.MallocHeaderSize)){
            if (Ꮡtyp == nil || !typ.Pointers()){
                // tiny allocations might be kept alive by other co-located values.
                // Make sure secret allocations get zeroed by avoiding the tiny allocator
                // See go.dev/issue/76356
                var gpΔ1 = getg();
                if (size < maxTinySize && (~gpΔ1).secret == 0){
                    (x, elemsize) = mallocgcTiny(size, Ꮡtyp);
                } else {
                    (x, elemsize) = mallocgcSmallNoscan(size, Ꮡtyp, needzero);
                }
            } else {
                if (!needzero) {
                    @throw(objectsWithPointersMustˢ);
                }
                if (heapBitsInSpan(size)){
                    (x, elemsize) = mallocgcSmallScanNoHeader(size, Ꮡtyp);
                } else {
                    (x, elemsize) = mallocgcSmallScanHeader(size, Ꮡtyp);
                }
            }
        } else {
            (x, elemsize) = mallocgcLarge(size, Ꮡtyp, needzero);
        }
    }

    var gp = getg();
    if (goexperiment.RuntimeSecret && (~gp).secret > 0) {
        // Mark any object allocated while in secret mode as secret.
        // This ensures we zero it immediately when freeing it.
        addSecret(x, size);
    }

    // Notify sanitizers, if enabled.
    if (raceenabled) {
        racemalloc(x, size - asanRZ);
    }
    if (msanenabled) {
        msanmalloc(x, size - asanRZ);
    }
    if (asanenabled) {
        // Poison the space between the end of the requested size of x
        // and the end of the slot. Unpoison the requested allocation.
        var frag = elemsize - size;
        if (Ꮡtyp != nil && typ.Pointers() && !heapBitsInSpan(elemsize) && size <= (uintptr)(maxSmallSize - gc.MallocHeaderSize)) {
            frag -= gc.MallocHeaderSize;
        }
        asanpoison((uintptr)@unsafe.Add(x, size - asanRZ), asanRZ);
        asanunpoison(x, size - asanRZ);
    }
    if (valgrindenabled) {
        valgrindMalloc(x, size - asanRZ);
    }

    // Adjust our GC assist debt to account for internal fragmentation.
    if (gcBlackenEnabled != 0 && elemsize != 0) {
        {
            var assistG = getg().Value.m.Value.curg; if (assistG != nil) {
                assistG.Value.gcAssistBytes -= (int64)(elemsize - size);
            }
        }
    }

    // Post-malloc debug hooks.
    if (debug.malloc) {
        postMallocgcDebug(x, elemsize, Ꮡtyp);
    }
    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expectedNoscanForTinyˢ = "expected noscan for tiny alloc"u8;

internal static (@unsafe.Pointer, uintptr) mallocgcTiny(uintptr size, ж<_type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    // Set mp.mallocing to keep from being preempted by GC.
    var mp = acquirem();
    if (doubleCheckMalloc) {
        if ((~mp).mallocing != 0) {
            @throw(mallocDeadlockˢ);
        }
        if ((~mp).gsignal == getg()) {
            @throw(mallocDuringSignalˢ);
        }
        if (Ꮡtyp != nil && typ.Pointers()) {
            @throw(expectedNoscanForTinyˢ);
        }
    }
    mp.Value.mallocing = 1;

    // Tiny allocator.
    //
    // Tiny allocator combines several tiny allocation requests
    // into a single memory block. The resulting memory block
    // is freed when all subobjects are unreachable. The subobjects
    // must be noscan (don't have pointers), this ensures that
    // the amount of potentially wasted memory is bounded.
    //
    // Size of the memory block used for combining (maxTinySize) is tunable.
    // Current setting is 16 bytes, which relates to 2x worst case memory
    // wastage (when all but one subobjects are unreachable).
    // 8 bytes would result in no wastage at all, but provides less
    // opportunities for combining.
    // 32 bytes provides more opportunities for combining,
    // but can lead to 4x worst case wastage.
    // The best case winning is 8x regardless of block size.
    //
    // Objects obtained from tiny allocator must not be freed explicitly.
    // So when an object will be freed explicitly, we ensure that
    // its size >= maxTinySize.
    //
    // SetFinalizer has a special case for objects potentially coming
    // from tiny allocator, it such case it allows to set finalizers
    // for an inner byte of a memory block.
    //
    // The main targets of tiny allocator are small strings and
    // standalone escaping variables. On a json benchmark
    // the allocator reduces number of allocations by ~12% and
    // reduces heap size by ~20%.
    var c = getMCache(ref (mp).DerefOrNull());
    var off = c.Value.tinyoffset;
    // Align tiny pointer for required (conservative) alignment.
    if ((uintptr)(size & 7) == 0){
        off = alignUp(off, 8);
    } else 
    if (goarch.PtrSize == 4 && size == 12){
        // Conservatively align 12-byte objects to 8 bytes on 32-bit
        // systems so that objects whose first field is a 64-bit
        // value is aligned to 8 bytes and does not cause a fault on
        // atomic access. See issue 37262.
        // TODO(mknyszek): Remove this workaround if/when issue 36606
        // is resolved.
        off = alignUp(off, 8);
    } else 
    if ((uintptr)(size & 3) == 0){
        off = alignUp(off, 4);
    } else 
    if ((uintptr)(size & 1) == 0) {
        off = alignUp(off, 2);
    }
    if (off + size <= maxTinySize && (~c).tiny != 0) {
        // The object fits into existing tiny block.
        @unsafe.Pointer xΔ1 = (@unsafe.Pointer)((~c).tiny + off);
        c.Value.tinyoffset = off + size;
        c.Value.tinyAllocs++;
        mp.Value.mallocing = 0;
        releasem(ref (mp).DerefOrNull());
        return (xΔ1, 0);
    }
    // Allocate a new maxTinySize block.
    var checkGCTrigger = false;
    var span = (~c).alloc[tinySpanClass];
    var v = nextFreeFast(span);
    if (v == 0) {
        (v, span, checkGCTrigger) = c.nextFree(tinySpanClass);
    }
    @unsafe.Pointer x = (@unsafe.Pointer)(uintptr)v;
    ((ж<array<uint64>>)(uintptr)(x)).Value[0] = 0; // Always zero
    ((ж<array<uint64>>)(uintptr)(x)).Value[1] = 0;
    // See if we need to replace the existing tiny block with the new one
    // based on amount of remaining free space.
    if (!raceenabled && (size < (~c).tinyoffset || (~c).tiny == 0)) {
        // Note: disabled when race detector is on, see comment near end of this function.
        c.Value.tiny = (uintptr)x;
        c.Value.tinyoffset = size;
    }

    // Ensure that the stores above that initialize x to
    // type-safe memory and set the heap bits occur before
    // the caller can make x observable to the garbage
    // collector. Otherwise, on weakly ordered machines,
    // the garbage collector could follow a pointer to x,
    // but see uninitialized memory or stale heap bits.
    publicationBarrier();

    if (writeBarrier.enabled){
        // Allocate black during GC.
        // All slots hold nil so no scanning is needed.
        // This may be racing with GC so do it atomically if there can be
        // a race marking the bit.
        gcmarknewobject(span, (uintptr)x);
    } else {
        // Track the last free index before the mark phase. This field
        // is only used by the garbage collector. During the mark phase
        // this is used by the conservative scanner to filter out objects
        // that are both free and recently-allocated. It's safe to do that
        // because we allocate-black if the GC is enabled. The conservative
        // scanner produces pointers out of thin air, so without additional
        // synchronization it might otherwise observe a partially-initialized
        // object, which could crash the program.
        span.Value.freeIndexForScan = span.Value.freeindex;
    }

    // Note cache c only valid while m acquired; see #47302
    //
    // N.B. Use the full size because that matches how the GC
    // will update the mem profile on the "free" side.
    //
    // TODO(mknyszek): We should really count the header as part
    // of gc_sys or something. The code below just pretends it is
    // internal fragmentation and matches the GC's accounting by
    // using the whole allocation slot.
    c.Value.nextSample -= (int64)(~span).elemsize;
    if ((~c).nextSample < 0 || MemProfileRate != (~c).memProfRate) {
        profilealloc(ref (mp).DerefOrNull(), x, (~span).elemsize);
    }
    mp.Value.mallocing = 0;
    releasem(ref (mp).DerefOrNull());

    if (checkGCTrigger) {
        {
            var t = (new gcTrigger(kind: gcTriggerHeap)); if (t.test()) {
                gcStart(t);
            }
        }
    }

    if (raceenabled) {
        // Pad tinysize allocations so they are aligned with the end
        // of the tinyalloc region. This ensures that any arithmetic
        // that goes off the top end of the object will be detectable
        // by checkptr (issue 38872).
        // Note that we disable tinyalloc when raceenabled for this to work.
        // TODO: This padding is only performed when the race detector
        // is enabled. It would be nice to enable it if any package
        // was compiled with checkptr, but there's no easy way to
        // detect that (especially at compile time).
        // TODO: enable this padding for all allocations, not just
        // tinyalloc ones. It's tricky because of pointer maps.
        // Maybe just all noscan objects?
        x = (uintptr)add(x, (~span).elemsize - size);
    }
    return (x, (~span).elemsize);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expectedNoscanTypeForˢ = "expected noscan type for noscan alloc"u8;

internal static (@unsafe.Pointer, uintptr) mallocgcSmallNoscan(uintptr size, ж<_type> Ꮡtyp, bool needzero) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    // Set mp.mallocing to keep from being preempted by GC.
    var mp = acquirem();
    if (doubleCheckMalloc) {
        if ((~mp).mallocing != 0) {
            @throw(mallocDeadlockˢ);
        }
        if ((~mp).gsignal == getg()) {
            @throw(mallocDuringSignalˢ);
        }
        if (Ꮡtyp != nil && typ.Pointers()) {
            @throw(expectedNoscanTypeForˢ);
        }
    }
    mp.Value.mallocing = 1;

    var checkGCTrigger = false;
    var c = getMCache(ref (mp).DerefOrNull());
    uint8 sizeclass = default!;
    if (size <= (uintptr)(gc.SmallSizeMax - 8)){
        sizeclass = gc.SizeToSizeClass8[divRoundUp(size, gc.SmallSizeDiv)];
    } else {
        sizeclass = gc.SizeToSizeClass128[divRoundUp(size - (uintptr)gc.SmallSizeMax, gc.LargeSizeDiv)];
    }
    size = (uintptr)gc.SizeClassToSize[sizeclass];
    var spc = makeSpanClass(sizeclass, true);
    var span = (~c).alloc[spc];

    // First, check for a reusable object.
    if (runtimeFreegcEnabled && c.hasReusableNoscan(spc)) {
        // We have a reusable object, use it.
        @unsafe.Pointer xΔ1 = (uintptr)mallocgcSmallNoscanReuse(c, span, spc, size, needzero);
        mp.Value.mallocing = 0;
        releasem(ref (mp).DerefOrNull());
        return (xΔ1, size);
    }

    var v = nextFreeFast(span);
    if (v == 0) {
        (v, span, checkGCTrigger) = c.nextFree(spc);
    }
    @unsafe.Pointer x = (@unsafe.Pointer)(uintptr)v;
    if (needzero && (~span).needzero != 0) {
        memclrNoHeapPointers(x, size);
    }

    // Ensure that the stores above that initialize x to
    // type-safe memory and set the heap bits occur before
    // the caller can make x observable to the garbage
    // collector. Otherwise, on weakly ordered machines,
    // the garbage collector could follow a pointer to x,
    // but see uninitialized memory or stale heap bits.
    publicationBarrier();

    if (writeBarrier.enabled){
        // Allocate black during GC.
        // All slots hold nil so no scanning is needed.
        // This may be racing with GC so do it atomically if there can be
        // a race marking the bit.
        gcmarknewobject(span, (uintptr)x);
    } else {
        // Track the last free index before the mark phase. This field
        // is only used by the garbage collector. During the mark phase
        // this is used by the conservative scanner to filter out objects
        // that are both free and recently-allocated. It's safe to do that
        // because we allocate-black if the GC is enabled. The conservative
        // scanner produces pointers out of thin air, so without additional
        // synchronization it might otherwise observe a partially-initialized
        // object, which could crash the program.
        span.Value.freeIndexForScan = span.Value.freeindex;
    }

    // Note cache c only valid while m acquired; see #47302
    //
    // N.B. Use the full size because that matches how the GC
    // will update the mem profile on the "free" side.
    //
    // TODO(mknyszek): We should really count the header as part
    // of gc_sys or something. The code below just pretends it is
    // internal fragmentation and matches the GC's accounting by
    // using the whole allocation slot.
    c.Value.nextSample -= (int64)size;
    if ((~c).nextSample < 0 || MemProfileRate != (~c).memProfRate) {
        profilealloc(ref (mp).DerefOrNull(), x, size);
    }
    mp.Value.mallocing = 0;
    releasem(ref (mp).DerefOrNull());

    if (checkGCTrigger) {
        {
            var t = (new gcTrigger(kind: gcTriggerHeap)); if (t.test()) {
                gcStart(t);
            }
        }
    }
    return (x, size);
}

// mallocgcSmallNoscanReuse returns a previously freed noscan object after preparing it for reuse.
// It must only be called if hasReusableNoscan returned true.
internal static @unsafe.Pointer mallocgcSmallNoscanReuse(ж<mcache> Ꮡc, ж<mspan> Ꮡspan, spanClass spc, uintptr size, bool needzero) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var span = ref Ꮡspan.DerefOrNull();

    // TODO(thepudds): could nextFreeFast, nextFree and nextReusable return unsafe.Pointer?
    // Maybe doesn't matter. gclinkptr might be for historical reasons.
    (var v, Ꮡspan) = c.nextReusableNoScan(Ꮡspan, spc); span = ref Ꮡspan.DerefOrNull();
    @unsafe.Pointer x = (@unsafe.Pointer)(uintptr)v;

    // Compensate for the GC assist credit deducted in mallocgc (before calling us and
    // after we return) because this is not a newly allocated object. We use the full slot
    // size (elemsize) here because that's what mallocgc deducts overall. Note we only
    // adjust this when gcBlackenEnabled is true, which follows mallocgc behavior.
    // TODO(thepudds): a follow-up CL adds a more specific test of our assist credit
    // handling, including for validating internal fragmentation handling.
    if (gcBlackenEnabled != 0) {
        addAssistCredit(size);
    }

    // This is a previously used object, so only check needzero (and not span.needzero)
    // for clearing.
    if (needzero) {
        memclrNoHeapPointers(x, size);
    }

    // See publicationBarrier comment in mallocgcSmallNoscan.
    publicationBarrier();

    // Finish and return. Note that we do not update span.freeIndexForScan, profiling info,
    // nor do we check gcTrigger.
    // TODO(thepudds): the current approach is viable for a GOEXPERIMENT, but
    // means we do not profile reused heap objects. Ultimately, we will need a better
    // approach for profiling, or at least ensure we are not introducing bias in the
    // profiled allocations.
    // TODO(thepudds): related, we probably want to adjust how allocs and frees are counted
    // in the existing stats. Currently, reused objects are not counted as allocs nor
    // frees, but instead roughly appear as if the original heap object lived on. We
    // probably will also want some additional runtime/metrics, and generally think about
    // user-facing observability & diagnostics, though all this likely can wait for an
    // official proposal.
    if (writeBarrier.enabled) {
        // Allocate black during GC.
        // All slots hold nil so no scanning is needed.
        // This may be racing with GC so do it atomically if there can be
        // a race marking the bit.
        gcmarknewobject(Ꮡspan, (uintptr)x);
    }
    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noscanAllocatedInScanˢ = "noscan allocated in scan-only path"u8;
internal static readonly @string heapBitsInNotInSpanForˢ = "heap bits in not in span for non-header-only path"u8;

internal static (@unsafe.Pointer, uintptr) mallocgcSmallScanNoHeader(uintptr size, ж<_type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    // Set mp.mallocing to keep from being preempted by GC.
    var mp = acquirem();
    if (doubleCheckMalloc) {
        if ((~mp).mallocing != 0) {
            @throw(mallocDeadlockˢ);
        }
        if ((~mp).gsignal == getg()) {
            @throw(mallocDuringSignalˢ);
        }
        if (Ꮡtyp == nil || !typ.Pointers()) {
            @throw(noscanAllocatedInScanˢ);
        }
        if (!heapBitsInSpan(size)) {
            @throw(heapBitsInNotInSpanForˢ);
        }
    }
    mp.Value.mallocing = 1;

    var checkGCTrigger = false;
    var c = getMCache(ref (mp).DerefOrNull());
    var sizeclass = gc.SizeToSizeClass8[divRoundUp(size, gc.SmallSizeDiv)];
    var spc = makeSpanClass(sizeclass, false);
    var span = (~c).alloc[spc];
    var v = nextFreeFast(span);
    if (v == 0) {
        (v, span, checkGCTrigger) = c.nextFree(spc);
    }
    @unsafe.Pointer x = (@unsafe.Pointer)(uintptr)v;
    if ((~span).needzero != 0) {
        memclrNoHeapPointers(x, size);
    }
    if (goarch.PtrSize == 8 && sizeclass == 1){
        // initHeapBits already set the pointer bits for the 8-byte sizeclass
        // on 64-bit platforms.
        c.Value.scanAlloc += 8;
    } else {
        c.Value.scanAlloc += heapSetTypeNoHeader((uintptr)x, size, Ꮡtyp, span);
    }
    size = (uintptr)gc.SizeClassToSize[sizeclass];

    // Ensure that the stores above that initialize x to
    // type-safe memory and set the heap bits occur before
    // the caller can make x observable to the garbage
    // collector. Otherwise, on weakly ordered machines,
    // the garbage collector could follow a pointer to x,
    // but see uninitialized memory or stale heap bits.
    publicationBarrier();

    if (writeBarrier.enabled){
        // Allocate black during GC.
        // All slots hold nil so no scanning is needed.
        // This may be racing with GC so do it atomically if there can be
        // a race marking the bit.
        gcmarknewobject(span, (uintptr)x);
    } else {
        // Track the last free index before the mark phase. This field
        // is only used by the garbage collector. During the mark phase
        // this is used by the conservative scanner to filter out objects
        // that are both free and recently-allocated. It's safe to do that
        // because we allocate-black if the GC is enabled. The conservative
        // scanner produces pointers out of thin air, so without additional
        // synchronization it might otherwise observe a partially-initialized
        // object, which could crash the program.
        span.Value.freeIndexForScan = span.Value.freeindex;
    }

    // Note cache c only valid while m acquired; see #47302
    //
    // N.B. Use the full size because that matches how the GC
    // will update the mem profile on the "free" side.
    //
    // TODO(mknyszek): We should really count the header as part
    // of gc_sys or something. The code below just pretends it is
    // internal fragmentation and matches the GC's accounting by
    // using the whole allocation slot.
    c.Value.nextSample -= (int64)size;
    if ((~c).nextSample < 0 || MemProfileRate != (~c).memProfRate) {
        profilealloc(ref (mp).DerefOrNull(), x, size);
    }
    mp.Value.mallocing = 0;
    releasem(ref (mp).DerefOrNull());

    if (checkGCTrigger) {
        {
            var t = (new gcTrigger(kind: gcTriggerHeap)); if (t.test()) {
                gcStart(t);
            }
        }
    }
    return (x, size);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string heapBitsInSpanForHeaderˢ = "heap bits in span for header-only path"u8;

internal static (@unsafe.Pointer, uintptr) mallocgcSmallScanHeader(uintptr size, ж<_type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    // Set mp.mallocing to keep from being preempted by GC.
    var mp = acquirem();
    if (doubleCheckMalloc) {
        if ((~mp).mallocing != 0) {
            @throw(mallocDeadlockˢ);
        }
        if ((~mp).gsignal == getg()) {
            @throw(mallocDuringSignalˢ);
        }
        if (Ꮡtyp == nil || !typ.Pointers()) {
            @throw(noscanAllocatedInScanˢ);
        }
        if (heapBitsInSpan(size)) {
            @throw(heapBitsInSpanForHeaderˢ);
        }
    }
    mp.Value.mallocing = 1;

    var checkGCTrigger = false;
    var c = getMCache(ref (mp).DerefOrNull());
    size += gc.MallocHeaderSize;
    uint8 sizeclass = default!;
    if (size <= (uintptr)(gc.SmallSizeMax - 8)){
        sizeclass = gc.SizeToSizeClass8[divRoundUp(size, gc.SmallSizeDiv)];
    } else {
        sizeclass = gc.SizeToSizeClass128[divRoundUp(size - (uintptr)gc.SmallSizeMax, gc.LargeSizeDiv)];
    }
    size = (uintptr)gc.SizeClassToSize[sizeclass];
    var spc = makeSpanClass(sizeclass, false);
    var span = (~c).alloc[spc];
    var v = nextFreeFast(span);
    if (v == 0) {
        (v, span, checkGCTrigger) = c.nextFree(spc);
    }
    @unsafe.Pointer x = (@unsafe.Pointer)(uintptr)v;
    if ((~span).needzero != 0) {
        memclrNoHeapPointers(x, size);
    }
    var header = (ж<ж<_type>>)(uintptr)(x);
    x = (uintptr)add(x, gc.MallocHeaderSize);
    c.Value.scanAlloc += heapSetTypeSmallHeader((uintptr)x, size - (uintptr)gc.MallocHeaderSize, Ꮡtyp, header, span);

    // Ensure that the stores above that initialize x to
    // type-safe memory and set the heap bits occur before
    // the caller can make x observable to the garbage
    // collector. Otherwise, on weakly ordered machines,
    // the garbage collector could follow a pointer to x,
    // but see uninitialized memory or stale heap bits.
    publicationBarrier();

    if (writeBarrier.enabled){
        // Allocate black during GC.
        // All slots hold nil so no scanning is needed.
        // This may be racing with GC so do it atomically if there can be
        // a race marking the bit.
        gcmarknewobject(span, (uintptr)x);
    } else {
        // Track the last free index before the mark phase. This field
        // is only used by the garbage collector. During the mark phase
        // this is used by the conservative scanner to filter out objects
        // that are both free and recently-allocated. It's safe to do that
        // because we allocate-black if the GC is enabled. The conservative
        // scanner produces pointers out of thin air, so without additional
        // synchronization it might otherwise observe a partially-initialized
        // object, which could crash the program.
        span.Value.freeIndexForScan = span.Value.freeindex;
    }

    // Note cache c only valid while m acquired; see #47302
    //
    // N.B. Use the full size because that matches how the GC
    // will update the mem profile on the "free" side.
    //
    // TODO(mknyszek): We should really count the header as part
    // of gc_sys or something. The code below just pretends it is
    // internal fragmentation and matches the GC's accounting by
    // using the whole allocation slot.
    c.Value.nextSample -= (int64)size;
    if ((~c).nextSample < 0 || MemProfileRate != (~c).memProfRate) {
        profilealloc(ref (mp).DerefOrNull(), x, size);
    }
    mp.Value.mallocing = 0;
    releasem(ref (mp).DerefOrNull());

    if (checkGCTrigger) {
        {
            var t = (new gcTrigger(kind: gcTriggerHeap)); if (t.test()) {
                gcStart(t);
            }
        }
    }
    return (x, size);
}

internal static (@unsafe.Pointer, uintptr) mallocgcLarge(uintptr size, ж<_type> Ꮡtyp, bool needzero) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    // Set mp.mallocing to keep from being preempted by GC.
    var mp = acquirem();
    if (doubleCheckMalloc) {
        if ((~mp).mallocing != 0) {
            @throw(mallocDeadlockˢ);
        }
        if ((~mp).gsignal == getg()) {
            @throw(mallocDuringSignalˢ);
        }
    }
    mp.Value.mallocing = 1;

    var c = getMCache(ref (mp).DerefOrNull());
    // For large allocations, keep track of zeroed state so that
    // bulk zeroing can be happen later in a preemptible context.
    var span = c.allocLarge(size, Ꮡtyp == nil || !typ.Pointers());
    span.Value.freeindex = 1;
    span.Value.allocCount = 1;
    span.Value.largeType = default!; // Tell the GC not to look at this yet.
    size = span.Value.elemsize;
    @unsafe.Pointer x = (@unsafe.Pointer)span.@base();

    // Ensure that the store above that sets largeType to
    // nil happens before the caller can make x observable
    // to the garbage collector.
    //
    // Otherwise, on weakly ordered machines, the garbage
    // collector could follow a pointer to x, but see a stale
    // largeType value.
    publicationBarrier();

    if (writeBarrier.enabled){
        // Allocate black during GC.
        // All slots hold nil so no scanning is needed.
        // This may be racing with GC so do it atomically if there can be
        // a race marking the bit.
        gcmarknewobject(span, (uintptr)x);
    } else {
        // Track the last free index before the mark phase. This field
        // is only used by the garbage collector. During the mark phase
        // this is used by the conservative scanner to filter out objects
        // that are both free and recently-allocated. It's safe to do that
        // because we allocate-black if the GC is enabled. The conservative
        // scanner produces pointers out of thin air, so without additional
        // synchronization it might otherwise observe a partially-initialized
        // object, which could crash the program.
        span.Value.freeIndexForScan = span.Value.freeindex;
    }

    // Note cache c only valid while m acquired; see #47302
    //
    // N.B. Use the full size because that matches how the GC
    // will update the mem profile on the "free" side.
    //
    // TODO(mknyszek): We should really count the header as part
    // of gc_sys or something. The code below just pretends it is
    // internal fragmentation and matches the GC's accounting by
    // using the whole allocation slot.
    c.Value.nextSample -= (int64)size;
    if ((~c).nextSample < 0 || MemProfileRate != (~c).memProfRate) {
        profilealloc(ref (mp).DerefOrNull(), x, size);
    }
    mp.Value.mallocing = 0;
    releasem(ref (mp).DerefOrNull());

    // Check to see if we need to trigger the GC.
    {
        var t = (new gcTrigger(kind: gcTriggerHeap)); if (t.test()) {
            gcStart(t);
        }
    }

    // Objects can be zeroed late in a context where preemption can occur.
    //
    // x will keep the memory alive.
    if (needzero && (~span).needzero != 0) {
        // N.B. size == fullSize always in this case.
        memclrNoHeapPointersChunked(size, x); // This is a possible preemption point: see #47302
    }

    // Set the type and run the publication barrier while non-preemptible. We need to make
    // sure that between heapSetTypeLarge and publicationBarrier we cannot get preempted,
    // otherwise the GC could potentially observe non-zeroed memory but largeType set on weak
    // memory architectures.
    //
    // The GC can also potentially observe non-zeroed memory if conservative scanning spuriously
    // observes a partially-allocated object, see the freeIndexForScan update above. This case is
    // handled by synchronization inside heapSetTypeLarge.
    mp = acquirem();
    if (Ꮡtyp != nil && typ.Pointers()) {
        // Finish storing the type information, now that we're certain the memory is zeroed.
        getMCache(ref (mp).DerefOrNull()).Value.scanAlloc += heapSetTypeLarge((uintptr)x, size, Ꮡtyp, span);
    }
    // Publish the object again, now with zeroed memory and initialized type information.
    //
    // Even if we didn't update any type information, this is necessary to ensure that, for example,
    // x written to a global without any synchronization still results in other goroutines observing
    // zeroed memory.
    publicationBarrier();
    releasem(ref (mp).DerefOrNull());
    return (x, size);
}

internal static @unsafe.Pointer preMallocgcDebug(uintptr size, ж<_type> Ꮡtyp) {
    if (debug.sbrk != 0) {
        var align = (uintptr)16;
        if (Ꮡtyp != nil) {
            // TODO(austin): This should be just
            //   align = uintptr(typ.align)
            // but that's only 4 on 32-bit platforms,
            // even if there's a uint64 field in typ (see #599).
            // This causes 64-bit atomic accesses to panic.
            // Hence, we use stricter alignment that matches
            // the normal allocator better.
            if ((uintptr)(size & 7) == 0){
                align = 8;
            } else 
            if ((uintptr)(size & 3) == 0){
                align = 4;
            } else 
            if ((uintptr)(size & 1) == 0){
                align = 2;
            } else {
                align = 1;
            }
        }
        return (uintptr)persistentalloc(size, align, Ꮡmemstats.of(mstats.Ꮡother_sys));
    }
    if (inittrace.active && inittrace.id == (~getg()).goid) {
        // Init functions are executed sequentially in a single goroutine.
        inittrace.allocs += 1;
    }
    return default!;
}

internal static void postMallocgcDebug(@unsafe.Pointer x, uintptr elemsize, ж<_type> Ꮡtyp) {
    if (inittrace.active && inittrace.id == (~getg()).goid) {
        // Init functions are executed sequentially in a single goroutine.
        inittrace.bytes += (uint64)elemsize;
    }

    if (traceAllocFreeEnabled()) {
        var Δtrace = traceAcquire();
        if (Δtrace.ok()) {
            Δtrace.HeapObjectAlloc((uintptr)x, Ꮡtyp);
            traceRelease(Δtrace);
        }
    }

    // N.B. elemsize == 0 indicates a tiny allocation, since no new slot was
    // allocated to fulfill this call to mallocgc. This means checkfinalizer
    // will only flag an error if there is actually any risk. If an allocation
    // has the tiny block to itself, it will not get flagged, because we won't
    // mark the block as a tiny block.
    if (debug.checkfinalizers != 0 && elemsize == 0) {
        setTinyBlockContext((@unsafe.Pointer)alignDown((uintptr)x, maxTinySize));
    }
}

// deductAssistCredit reduces the current G's assist credit
// by size bytes, and assists the GC if necessary.
//
// Caller must be preemptible.
internal static void deductAssistCredit(uintptr size) {
    // Charge the current user G for this allocation.
    var assistG = getg();
    if ((~(~assistG).m).curg != nil) {
        assistG = assistG.Value.m.Value.curg;
    }
    // Charge the allocation against the G. We'll account
    // for internal fragmentation at the end of mallocgc.
    assistG.Value.gcAssistBytes -= (int64)size;

    if ((~assistG).gcAssistBytes < 0) {
        // This G is in debt. Assist the GC to correct
        // this before allocating. This must happen
        // before disabling preemption.
        gcAssistAlloc(assistG);
    }
}

// addAssistCredit is like deductAssistCredit,
// but adds credit rather than removes,
// and never calls gcAssistAlloc.
internal static void addAssistCredit(uintptr size) {
    // Credit the current user G.
    var assistG = getg();
    if ((~(~assistG).m).curg != nil) { // TODO(thepudds): do we need to do this?
        assistG = assistG.Value.m.Value.curg;
    }
    // Credit the size against the G.
    assistG.Value.gcAssistBytes += (int64)size;
}

// doubleCheckReusable enables some additional invariant checks for the
// runtime.freegc and reusable objects. Note that some of these checks alter timing,
// and it is good to test changes with and without this enabled.
internal const bool doubleCheckReusable = false;

// debugReusableLog enables some printlns for runtime.freegc and reusable objects.
internal const bool debugReusableLog = false;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string freegcNilˢ = "freegc nil"u8;
internal static readonly @string freegcDeadlockˢ = "freegc deadlock"u8;
internal static readonly @string freegcDuringSignalˢ = "freegc during signal"u8;
internal static readonly @string nextReusableNilSpanForˢ = "nextReusable: nil span for pointer in free list"u8;
internal static readonly @string nextReusableSpanIsNotInˢ = "nextReusable: span is not in use"u8;
internal static readonly @string freegcCalledWithoutAPOrˢ = "freegc called without a P or outside bootstrapping"u8;
internal static readonly @string freegcCalledForObjectˢ = "freegc called for object with pointers, not yet implemented"u8;

// freegc records that a heap object is reusable and available for
// immediate reuse in a subsequent mallocgc allocation, without
// needing to wait for the GC cycle to progress.
//
// The information is recorded in a free list stored in the
// current P's mcache. The caller must pass in the user size
// and whether the object has pointers, which allows a faster free
// operation.
//
// freegc must be called by the effective owner of ptr who knows
// the pointer is logically dead, with no possible aliases that might
// be used past that moment. In other words, ptr must be the
// last and only pointer to its referent.
//
// The intended caller is the compiler.
//
// Note: please do not send changes that attempt to add freegc calls
// to the standard library.
//
// ptr must point to a heap object or into the current g's stack,
// in which case freegc is a no-op. In particular, ptr must not point
// to memory in the data or bss sections, which is partially enforced.
// For objects with a malloc header, ptr should point mallocHeaderSize bytes
// past the base; otherwise, ptr should point to the base of the heap object.
// In other words, ptr should be the same pointer that was returned by mallocgc.
//
// In addition, the caller must know that ptr's object has no specials, such
// as might have been created by a call to SetFinalizer or AddCleanup.
// (Internally, the runtime deals appropriately with internally-created
// specials, such as specials for memory profiling).
//
// If the size of ptr's object is less than 16 bytes or greater than
// 32KiB - gc.MallocHeaderSize bytes, freegc is currently a no-op. It must only
// be called in alloc-safe places. It currently throws if noscan is false
// (support for which is implemented in a later CL in our stack).
//
// Note that freegc accepts an unsafe.Pointer and hence keeps the pointer
// alive. It therefore could be a pessimization in some cases (such
// as a long-lived function) if the caller does not call freegc before
// or roughly when the liveness analysis of the compiler
// would otherwise have determined ptr's object is reclaimable by the GC.
internal static bool freegc(@unsafe.Pointer ptr, uintptr size, bool noscan) {
    if (!runtimeFreegcEnabled || !reusableSize(size)) {
        return false;
    }
    if (sizeSpecializedMallocEnabled && !noscan) {
        // TODO(thepudds): temporarily disable freegc with SizeSpecializedMalloc for pointer types
        // until we finish integrating.
        return false;
    }

    if (ptr == nil) {
        @throw(freegcNilˢ);
    }

    // Set mp.mallocing to keep from being preempted by GC.
    // Otherwise, the GC could flush our mcache or otherwise cause problems.
    var mp = acquirem();
    if ((~mp).mallocing != 0) {
        @throw(freegcDeadlockˢ);
    }
    if ((~mp).gsignal == getg()) {
        @throw(freegcDuringSignalˢ);
    }
    mp.Value.mallocing = 1;

    if ((~(~mp).curg).stack.lo <= (uintptr)ptr && (uintptr)ptr < (~(~mp).curg).stack.hi) {
        // This points into our stack, so free is a no-op.
        mp.Value.mallocing = 0;
        releasem(ref (mp).DerefOrNull());
        return false;
    }

    if (doubleCheckReusable) {
        // TODO(thepudds): we could enforce no free on globals in bss or data. Maybe by
        // checking span via spanOf or spanOfHeap, or maybe walk from firstmoduledata
        // like isGoPointerWithoutSpan, or activeModules, or something. If so, we might
        // be able to delay checking until reuse (e.g., check span just before reusing,
        // though currently we don't always need to lookup a span on reuse). If we think
        // no usage patterns could result in globals, maybe enforcement for globals could
        // be behind -d=checkptr=1 or similar. The compiler can have knowledge of where
        // a variable is allocated, but stdlib does not, although there are certain
        // usage patterns that cannot result in a global.
        // TODO(thepudds): separately, consider a local debugReusableMcacheOnly here
        // to ignore freed objects if not in mspan in mcache,  maybe when freeing and reading,
        // by checking something like s.base() <= uintptr(v) && uintptr(v) < s.limit. Or
        // maybe a GODEBUG or compiler debug flag.
        var span = spanOf((uintptr)ptr);
        if (span == nil) {
            @throw(nextReusableNilSpanForˢ);
        }
        {
            var state = span.of(mspan.Ꮡstate).get(); if (state != mSpanInUse) {
                @throw(nextReusableSpanIsNotInˢ);
            }
        }
    }

    if (debug.clobberfree != 0) {
        clobberfree(ptr, size);
    }

    // We first check if p is still in our per-P cache.
    // Get our per-P cache for small objects.
    var c = getMCache(ref (mp).DerefOrNull());
    if (c == nil) {
        @throw(freegcCalledWithoutAPOrˢ);
    }

    var v = (uintptr)ptr;
    if (!noscan && !heapBitsInSpan(size)) {
        // mallocgcSmallScanHeader expects to get the base address of the object back
        // from the findReusable funcs (as well as from nextFreeFast and nextFree), and
        // not mallocHeaderSize bytes into a object, so adjust that here.
        v -= mallocHeaderSize;

        // The size class lookup wants size to be adjusted by mallocHeaderSize.
        size += mallocHeaderSize;
    }

    // TODO(thepudds): should verify (behind doubleCheckReusable constant) that our calculated
    // sizeclass here matches what's in span found via spanOf(ptr) or findObject(ptr).
    uint8 sizeclass = default!;
    if (size <= (uintptr)(gc.SmallSizeMax - 8)){
        sizeclass = gc.SizeToSizeClass8[divRoundUp(size, gc.SmallSizeDiv)];
    } else {
        sizeclass = gc.SizeToSizeClass128[divRoundUp(size - (uintptr)gc.SmallSizeMax, gc.LargeSizeDiv)];
    }

    var spc = makeSpanClass(sizeclass, noscan);
    var s = (~c).alloc[spc];

    if (debugReusableLog) {
        if (s.@base() <= (uintptr)v && (uintptr)v < (~s).limit){
            println((@string)"freegc [in mcache]:"u8, (Δhex)(uint64)(uintptr)v, (@string)"sweepgen:"u8, mheap_.sweepgen, (@string)"writeBarrier.enabled:"u8, writeBarrier.enabled);
        } else {
            println((@string)"freegc [NOT in mcache]:"u8, (Δhex)(uint64)(uintptr)v, (@string)"sweepgen:"u8, mheap_.sweepgen, (@string)"writeBarrier.enabled:"u8, writeBarrier.enabled);
        }
    }

    if (noscan){
        c.addReusableNoscan(spc, (uintptr)v);
    } else {
        // TODO(thepudds): implemented in later CL in our stack.
        @throw(freegcCalledForObjectˢ);
    }

    // For stats, for now we leave allocCount alone, roughly pretending to the rest
    // of the system that this potential reuse never happened.

    mp.Value.mallocing = 0;
    releasem(ref (mp).DerefOrNull());

    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nextReusableNoScanNilˢ = "nextReusableNoScan: nil span for pointer in reusable object free list"u8;

// nextReusableNoScan returns the next reusable object for a noscan span,
// or 0 if no reusable object is found.
internal static (gclinkptr, ж<mspan>) nextReusableNoScan(this ref mcache c, ж<mspan> Ꮡs, spanClass spc) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (!runtimeFreegcEnabled) {
        return (0, Ꮡs);
    }

    // Pop a reusable pointer from the free list for this span class.
    var v = c.reusableNoscan[spc];
    if (v == 0) {
        return (0, Ꮡs);
    }
    c.reusableNoscan[spc] = v.ptr().Value.next;

    if (debugReusableLog) {
        println((@string)"reusing from ptr free list:"u8, (Δhex)(uint64)(uintptr)v, (@string)"sweepgen:"u8, mheap_.sweepgen, (@string)"writeBarrier.enabled:"u8, writeBarrier.enabled);
    }
    if (doubleCheckReusable) {
        doubleCheckNextReusable(v); // debug only sanity check
    }

    // For noscan spans, we only need the span if the write barrier is enabled (so that our caller
    // can call gcmarknewobject to allocate black). If the write barrier is enabled, we can skip
    // looking up the span when the pointer is in a span in the mcache.
    if (!writeBarrier.enabled) {
        return (v, default!);
    }
    if (s.@base() <= (uintptr)v && (uintptr)v < s.limit) {
        // Return the original span.
        return (v, Ꮡs);
    }

    // We must find and return the span.
    var span = spanOf((uintptr)v);
    if (span == nil) {
        // TODO(thepudds): construct a test that triggers this throw.
        @throw(nextReusableNoScanNilˢ);
    }

    return (v, span);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nextReusableSpanIsNotInˢ2 = "nextReusable: span is not in range"u8;
internal static readonly @string nextReusableVIsNonBaseˢ = "nextReusable: v is non-base-address for object found on pointer free list"u8;
internal static readonly @string nextReusablePointerOnˢ = "nextReusable: pointer on free list is free"u8;
internal static readonly @string nextReusablePointerOnˢ2 = "nextReusable: pointer on free list is free after ensureSwept"u8;

// doubleCheckNextReusable checks some invariants.
// TODO(thepudds): will probably delete some of this. Can mostly be ignored for review.
internal static void doubleCheckNextReusable(gclinkptr v) {
    // TODO(thepudds): should probably take the spanClass as well to confirm expected
    // sizeclass match.
    var (_, span, objIndex) = findObject((uintptr)v, 0, 0);
    if (span == nil) {
        @throw(nextReusableNilSpanForˢ);
    }
    {
        var state = span.of(mspan.Ꮡstate).get(); if (state != mSpanInUse) {
            @throw(nextReusableSpanIsNotInˢ);
        }
    }
    if ((uintptr)v < span.@base() || (uintptr)v >= (~span).limit) {
        @throw(nextReusableSpanIsNotInˢ2);
    }
    if (span.objBase((uintptr)v) != (uintptr)v) {
        print((@string)"nextReusable: v="u8, (Δhex)(uint64)(uintptr)v, (@string)" base="u8, (Δhex)(uint64)span.objBase((uintptr)v), (@string)"\n"u8);
        @throw(nextReusableVIsNonBaseˢ);
    }
    if (span.isFree(objIndex)) {
        @throw(nextReusablePointerOnˢ);
    }

    const bool debugReusableEnsureSwept = false;
    if (debugReusableEnsureSwept) {
        // Currently disabled.
        // Note: ensureSwept here alters behavior (not just an invariant check).
        span.ensureSwept();
        if (span.isFree(objIndex)) {
            @throw(nextReusablePointerOnˢ2);
        }
    }
}

// reusableSize reports if size is a currently supported size for a reusable object.
internal static bool reusableSize(uintptr size) {
    if (size < maxTinySize || size > (uintptr)(maxSmallSize - mallocHeaderSize)) {
        return false;
    }
    return true;
}

// memclrNoHeapPointersChunked repeatedly calls memclrNoHeapPointers
// on chunks of the buffer to be zeroed, with opportunities for preemption
// along the way.  memclrNoHeapPointers contains no safepoints and also
// cannot be preemptively scheduled, so this provides a still-efficient
// block copy that can also be preempted on a reasonable granularity.
//
// Use this with care; if the data being cleared is tagged to contain
// pointers, this allows the GC to run before it is all cleared.
internal static void memclrNoHeapPointersChunked(uintptr size, @unsafe.Pointer x) {
    var v = (uintptr)x;
    // got this from benchmarking. 128k is too small, 512k is too large.
    uintptr chunkBytes = /* 256 * 1024 */ 262144;
    var vsize = v + size;
    for (var voff = v; voff < vsize; voff = voff + chunkBytes) {
        if ((~getg()).preempt) {
            // may hold locks, e.g., profiling
            goschedguarded();
        }
        // clear min(avail, lump) bytes
        var n = vsize - voff;
        if (n > chunkBytes) {
            n = chunkBytes;
        }
        memclrNoHeapPointers((@unsafe.Pointer)voff, n);
    }
}

// implementation of new builtin
// compiler (both frontend and SSA backend) knows the signature
// of this function.
internal static @unsafe.Pointer newobject(ж<_type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    return (uintptr)mallocgc(typ.Size_, Ꮡtyp, true);
}

//go:linkname maps_newobject internal/runtime/maps.newobject
internal static @unsafe.Pointer maps_newobject(ж<_type> Ꮡtyp) {
    return (uintptr)newobject(Ꮡtyp);
}

// reflect_unsafe_New is meant for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/goccy/json
//   - github.com/modern-go/reflect2
//   - github.com/v2pro/plz
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_unsafe_New reflect.unsafe_New
internal static @unsafe.Pointer reflect_unsafe_New(ж<_type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    return (uintptr)mallocgc(typ.Size_, Ꮡtyp, true);
}

//go:linkname reflectlite_unsafe_New internal/reflectlite.unsafe_New
internal static @unsafe.Pointer reflectlite_unsafe_New(ж<_type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    return (uintptr)mallocgc(typ.Size_, Ꮡtyp, true);
}

// newarray allocates an array of n elements of type typ.
//
// newarray should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/RomiChan/protobuf
//   - github.com/segmentio/encoding
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname newarray
internal static @unsafe.Pointer newarray(ж<_type> Ꮡtyp, nint n) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (n == 1) {
        return (uintptr)mallocgc(typ.Size_, Ꮡtyp, true);
    }
    var (mem, overflow) = Δmath.MulUintptr(typ.Size_, (uintptr)n);
    if (overflow || mem > maxAlloc || n < 0) {
        throw panic(((plainError)(@string)runtimeAllocationSizeOutˢ));
    }
    return (uintptr)mallocgc(mem, Ꮡtyp, true);
}

// reflect_unsafe_NewArray is meant for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/bytedance/sonic
//   - github.com/goccy/json
//   - github.com/modern-go/reflect2
//   - github.com/segmentio/encoding
//   - github.com/segmentio/kafka-go
//   - github.com/v2pro/plz
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_unsafe_NewArray reflect.unsafe_NewArray
internal static @unsafe.Pointer reflect_unsafe_NewArray(ж<_type> Ꮡtyp, nint n) {
    return (uintptr)newarray(Ꮡtyp, n);
}

//go:linkname maps_newarray internal/runtime/maps.newarray
internal static @unsafe.Pointer maps_newarray(ж<_type> Ꮡtyp, nint n) {
    return (uintptr)newarray(Ꮡtyp, n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string profileallocCalledˢ = "profilealloc called without a P or outside bootstrapping"u8;

// profilealloc resets the current mcache's nextSample counter and
// records a memory profile sample.
//
// The caller must be non-preemptible and have a P.
internal static void profilealloc(ref m mp, @unsafe.Pointer x, uintptr size) {
    var c = getMCache(ref mp);
    if (c == nil) {
        @throw(profileallocCalledˢ);
    }
    c.Value.memProfRate = MemProfileRate;
    c.Value.nextSample = nextSample();
    mProf_Malloc(ref mp, x, size);
}

// nextSample returns the next sampling point for heap profiling. The goal is
// to sample allocations on average every MemProfileRate bytes, but with a
// completely random distribution over the allocation timeline; this
// corresponds to a Poisson process with parameter MemProfileRate. In Poisson
// processes, the distance between two samples follows the exponential
// distribution (exp(MemProfileRate)), so the best return value is a random
// number taken from an exponential distribution whose mean is MemProfileRate.
internal static int64 nextSample() {
    if (MemProfileRate == 0) {
        // Basically never sample.
        return Δmath.MaxInt64;
    }
    if (MemProfileRate == 1) {
        // Sample immediately.
        return 0;
    }
    return (int64)fastexprand(MemProfileRate);
}

// fastexprand returns a random number from an exponential distribution with
// the specified mean.
internal static int32 fastexprand(nint mean) {
    // Avoid overflow. Maximum possible step is
    // -ln(1/(1<<randomBitCount)) * mean, approximately 20 * mean.
    switch (ᐧ) {
    case {} when mean is > 0x7000000: {
        mean = 0x7000000;
        break;
    }
    case {} when mean is 0: {
        return 0;
    }}

    // Take a random sample of the exponential distribution exp(-mean*x).
    // The probability distribution function is mean*exp(-mean*x), so the CDF is
    // p = 1 - exp(-mean*x), so
    // q = 1 - p == exp(-mean*x)
    // log_e(q) = -mean*x
    // -log_e(q)/mean = x
    // x = -log_e(q) * mean
    // x = log_2(q) * (-log_e(2)) * mean    ; Using log_2 for efficiency
    UntypedInt randomBitCount = 26;
    var q = cheaprandn((uint32)1 << (int)(randomBitCount)) + 1;
    var qlog = fastlog2((float64)q) - (float64)randomBitCount;
    if (qlog > 0D) {
        qlog = 0D;
    }
    const float64 minusLog2 = -0.6931471805599453; // -ln(2)
    return (int32)(qlog * (minusLog2 * (float64)mean)) + 1;
}

partial struct persistentAlloc {
    internal ж<notInHeap> @base;
    internal uintptr off;
}

partial struct globalAllocᴛ1 /*dyn*/ {
    /*embed*/ internal mutex mutex;
    /*embed*/ internal persistentAlloc persistentAlloc;
}
internal static ж<globalAllocᴛ1> ᏑglobalAlloc = new StandardBox<globalAllocᴛ1>(new globalAllocᴛ1(nil));
internal static ref globalAllocᴛ1 globalAlloc => ref ᏑglobalAlloc.Value;

// persistentChunkSize is the number of bytes we allocate when we grow
// a persistentAlloc.
internal static UntypedInt persistentChunkSize => /* 256 << 10 */ 262144;

// persistentChunks is a list of all the persistent chunks we have
// allocated. The list is maintained through the first word in the
// persistent chunk. This is updated atomically.
internal static ж<ж<notInHeap>> ᏑpersistentChunks = new StandardBox<ж<notInHeap>>(default(ж<notInHeap>));
internal static ref ж<notInHeap> persistentChunks => ref ᏑpersistentChunks.ValueSlot;

// Wrapper around sysAlloc that can allocate small chunks.
// There is no associated free operation.
// Intended for things like function/type/debug-related persistent data.
// If align is 0, uses default align (currently 8).
// The returned memory will be zeroed.
// sysStat must be non-nil.
//
// Consider marking persistentalloc'd types not in heap by embedding
// internal/runtime/sys.NotInHeap.
//
// nosplit because it is used during write barriers and must not be preempted.
//
//go:nosplit
internal static @unsafe.Pointer persistentalloc(uintptr size, uintptr align, ж<sysMemStat> ᏑsysStat) {
    ref var Δp = ref heap<ж<notInHeap>>(out var Ꮡp);
    systemstack(() => {
        Ꮡp.ValueSlot = persistentalloc1(size, align, ᏑsysStat);
    });
    return @unsafe.Pointer.FromPinnedBox(Δp);
}

// go2cs generated this placeholder — func persistentalloc1 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func inPersistentAlloc is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// linearAlloc is a simple linear allocator that pre-reserves a region
// of memory and then optionally maps that region into the Ready state
// as needed.
//
// The caller is responsible for locking.
partial struct linearAlloc {
    internal uintptr next; // next free byte
    internal uintptr mapped; // one byte past end of mapped space
    internal uintptr end; // end of reserved space
    internal bool mapMemory; // transition memory from Reserved to Ready if true
}

internal static void init(this ref linearAlloc l, uintptr @base, uintptr size, bool mapMemory) {
    if (@base + size < @base) {
        // Chop off the last byte. The runtime isn't prepared
        // to deal with situations where the bounds could overflow.
        // Leave that memory reserved, though, so we don't map it
        // later.
        size -= 1;
    }
    (l.next, l.mapped) = (@base, @base);
    l.end = @base + size;
    l.mapMemory = mapMemory;
}

internal static @unsafe.Pointer alloc(this ref linearAlloc l, uintptr size, uintptr align, ж<sysMemStat> ᏑsysStat, @string vmaName) {
    var Δp = alignUp(l.next, align);
    if (Δp + size > l.end) {
        return default!;
    }
    l.next = Δp + size;
    {
        var pEnd = alignUp(l.next - 1, physPageSize); if (pEnd > l.mapped) {
            if (l.mapMemory) {
                // Transition from Reserved to Prepared to Ready.
                var n = pEnd - l.mapped;
                sysMap((@unsafe.Pointer)l.mapped, n, ᏑsysStat, vmaName);
                sysUsed((@unsafe.Pointer)l.mapped, n, n);
            }
            l.mapped = pEnd;
        }
    }
    return (@unsafe.Pointer)Δp;
}

// notInHeap is off-heap memory allocated by a lower-level allocator
// like sysAlloc or persistentAlloc.
//
// In general, it's better to use real types which embed
// internal/runtime/sys.NotInHeap, but this serves as a generic type
// for situations where that isn't possible (like in the allocators).
//
// TODO: Use this as the return type of sysAlloc, persistentAlloc, etc?
partial struct notInHeap {
    internal sys.NotInHeap _;
}

internal static ж<notInHeap> add(this ж<notInHeap> Ꮡp, uintptr bytes) {
    ref var Δp = ref Ꮡp.DerefOrNull();

    return (ж<notInHeap>)(uintptr)((@unsafe.Pointer)((uintptr)(uintptr)@unsafe.Pointer.FromRef(ref Δp) + bytes));
}

// redZoneSize computes the size of the redzone for a given allocation.
// Refer to the implementation of the compiler-rt.
internal static uintptr redZoneSize(uintptr userSize) {
    switch (ᐧ) {
    case {} when userSize <= (64 - 16): {
        return (uintptr)16 << 0;
    }
    case {} when userSize <= (128 - 32): {
        return (uintptr)16 << 1;
    }
    case {} when userSize <= (512 - 64): {
        return (uintptr)16 << 2;
    }
    case {} when userSize <= (4096 - 128): {
        return (uintptr)16 << 3;
    }
    case {} when userSize <= (1 << 14) - 256: {
        return (uintptr)16 << 4;
    }
    case {} when userSize <= (1 << 15) - 512: {
        return (uintptr)16 << 5;
    }
    case {} when userSize <= (1 << 16) - 1024: {
        return (uintptr)16 << 6;
    }
    default: {
        return (uintptr)16 << 7;
    }}
}

} // end runtime_package
