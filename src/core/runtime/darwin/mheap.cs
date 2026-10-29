// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Page heap.
//
// See malloc.go for overview.
namespace go;

using abi = @internal.abi_package;
using cpu = @internal.cpu_package;
using goarch = @internal.goarch_package;
using goexperiment = @internal.goexperiment_package;
using atomic = @internal.runtime.atomic_package;
using gc = @internal.runtime.gc_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using System.Runtime.InteropServices;

partial class runtime_package {

// minPhysPageSize is a lower-bound on the physical page size. The
// true physical page size may be larger than this. In contrast,
// sys.PhysPageSize is an upper-bound on the physical page size.
internal static UntypedInt minPhysPageSize => 4096;

// maxPhysPageSize is the maximum page size the runtime supports.
internal static UntypedInt maxPhysPageSize => /* 512 << 10 */ 524288;

// maxPhysHugePageSize sets an upper-bound on the maximum huge page size
// that the runtime supports.
internal static UntypedInt maxPhysHugePageSize => /* pallocChunkBytes */ 4194304;

// pagesPerReclaimerChunk indicates how many pages to scan from the
// pageInUse bitmap at a time. Used by the page reclaimer.
//
// Higher values reduce contention on scanning indexes (such as
// h.reclaimIndex), but increase the minimum latency of the
// operation.
//
// The time required to scan this many pages can vary a lot depending
// on how many spans are actually freed. Experimentally, it can
// scan for pages at ~300 GB/ms on a 2.6GHz Core i7, but can only
// free spans at ~32 MB/ms. Using 512 pages bounds this at
// roughly 100µs.
//
// Must be a multiple of the pageInUse bitmap element size and
// must also evenly divide pagesPerArena.
internal static UntypedInt pagesPerReclaimerChunk => /* min(512, pagesPerArena) */ 512;

// physPageAlignedStacks indicates whether stack allocations must be
// physical page aligned. This is a requirement for MAP_STACK on
// OpenBSD.
internal const bool physPageAlignedStacks = /* GOOS == "openbsd" */ false;

partial struct mheap_curArena /*dyn*/ {
    internal uintptr @base, end;
}

partial struct mheap_central /*dyn*/ {
    internal mcentral mcentral;
    internal array<byte> pad = new(24);
}

partial struct mheap_userArena /*dyn*/ {
    // arenaHints is a list of addresses at which to attempt to
    // add more heap arenas for user arena chunks. This is initially
    // populated with a set of general hint addresses, and grown with
    // the bounds of actual heap arena ranges.
    internal ж<arenaHint> arenaHints;
    // quarantineList is a list of user arena spans that have been set to fault, but
    // are waiting for all pointers into them to go away. Sweeping handles
    // identifying when this is true, and moves the span to the ready list.
    internal mSpanList quarantineList;
    // readyList is a list of empty user arena spans that are ready for reuse.
    internal mSpanList readyList;
}

// Main malloc heap.
// The heap use pageAlloc to manage free and scavenged pages,
// but all the other global data is here too.
//
// mheap must not be heap-allocated because it contains mSpanLists,
// which must not be heap-allocated.
partial struct mheap {
    internal sys.NotInHeap _;
    // lock must only be acquired on the system stack, otherwise a g
    // could self-deadlock if its stack grows with the lock held.
    internal mutex @lock;
    internal pageAlloc pages; // page allocation data structure
    internal uint32 sweepgen; // sweep generation, see comment in mspan; written during STW
    // allspans is a slice of all mspans ever created. Each mspan
    // appears exactly once.
    //
    // The memory for allspans is manually managed and can be
    // reallocated and move as the heap grows.
    //
    // In general, allspans is protected by mheap_.lock, which
    // prevents concurrent access as well as freeing the backing
    // store. Accesses during STW might not hold the lock, but
    // must ensure that allocation cannot happen around the
    // access (since that may free the backing store).
    internal slice<ж<mspan>> allspans; // all spans out there
    // Proportional sweep
    //
    // These parameters represent a linear function from gcController.heapLive
    // to page sweep count. The proportional sweep system works to
    // stay in the black by keeping the current page sweep count
    // above this line at the current gcController.heapLive.
    //
    // The line has slope sweepPagesPerByte and passes through a
    // basis point at (sweepHeapLiveBasis, pagesSweptBasis). At
    // any given time, the system is at (gcController.heapLive,
    // pagesSwept) in this space.
    //
    // It is important that the line pass through a point we
    // control rather than simply starting at a 0,0 origin
    // because that lets us adjust sweep pacing at any time while
    // accounting for current progress. If we could only adjust
    // the slope, it would create a discontinuity in debt if any
    // progress has already been made.
    internal atomic.Uintptr pagesInUse;     // pages of spans in stats mSpanInUse
    internal atomic.Uint64 pagesSwept;      // pages swept this cycle
    internal atomic.Uint64 pagesSweptBasis; // pagesSwept to use as the origin of the sweep ratio
    internal uint64 sweepHeapLiveBasis;     // value of gcController.heapLive to use as the origin of sweep ratio; written with lock, read without
    internal float64 sweepPagesPerByte;     // proportional sweep ratio; written with lock, read without
// Page reclaimer state

    // reclaimIndex is the page index in heapArenas of next page to
    // reclaim. Specifically, it refers to page (i %
    // pagesPerArena) of arena heapArenas[i / pagesPerArena].
    //
    // If this is >= 1<<63, the page reclaimer is done scanning
    // the page marks.
    internal atomic.Uint64 reclaimIndex;
    // reclaimCredit is spare credit for extra pages swept. Since
    // the page reclaimer works in large chunks, it may reclaim
    // more than requested. Any spare pages released go to this
    // credit pool.
    internal atomic.Uintptr reclaimCredit;
    internal cpu.CacheLinePad __; // prevents false-sharing between arenas and preceding variables
    // arenas is the heap arena map. It points to the metadata for
    // the heap for every arena frame of the entire usable virtual
    // address space.
    //
    // Use arenaIndex to compute indexes into this array.
    //
    // For regions of the address space that are not backed by the
    // Go heap, the arena map contains nil.
    //
    // Modifications are protected by mheap_.lock. Reads can be
    // performed without locking; however, a given entry can
    // transition from nil to non-nil at any time when the lock
    // isn't held. (Entries never transitions back to nil.)
    //
    // In general, this is a two-level mapping consisting of an L1
    // map and possibly many L2 maps. This saves space when there
    // are a huge number of arena frames. However, on many
    // platforms (even 64-bit), arenaL1Bits is 0, making this
    // effectively a single-level map. In this case, arenas[0]
    // will never be nil.
    internal array<ж<array<ж<heapArena>>>> arenas = new((1 << (int)(arenaL1Bits)));
    // arenasHugePages indicates whether arenas' L2 entries are eligible
    // to be backed by huge pages.
    internal bool arenasHugePages;
    // heapArenaAlloc is pre-reserved space for allocating heapArena
    // objects. This is only used on 32-bit, where we pre-reserve
    // this space to avoid interleaving it with the heap itself.
    internal linearAlloc heapArenaAlloc;
    // arenaHints is a list of addresses at which to attempt to
    // add more heap arenas. This is initially populated with a
    // set of general hint addresses, and grown with the bounds of
    // actual heap arena ranges.
    internal ж<arenaHint> arenaHints;
    // arena is a pre-reserved space for allocating heap arenas
    // (the actual arenas). This is only used on 32-bit.
    internal linearAlloc arena;
    // heapArenas is the arenaIndex of every mapped arena mapped for the heap.
    // This can be used to iterate through the heap address space.
    //
    // Access is protected by mheap_.lock. However, since this is
    // append-only and old backing arrays are never freed, it is
    // safe to acquire mheap_.lock, copy the slice header, and
    // then release mheap_.lock.
    internal slice<arenaIdx> heapArenas;
    // userArenaArenas is the arenaIndex of every mapped arena mapped for
    // user arenas.
    //
    // Access is protected by mheap_.lock. However, since this is
    // append-only and old backing arrays are never freed, it is
    // safe to acquire mheap_.lock, copy the slice header, and
    // then release mheap_.lock.
    internal slice<arenaIdx> userArenaArenas;
    // sweepArenas is a snapshot of heapArenas taken at the
    // beginning of the sweep cycle. This can be read safely by
    // simply blocking GC (by disabling preemption).
    internal slice<arenaIdx> sweepArenas;
    // markArenas is a snapshot of heapArenas taken at the beginning
    // of the mark cycle. Because heapArenas is append-only, neither
    // this slice nor its contents will change during the mark, so
    // it can be read safely.
    internal slice<arenaIdx> markArenas;
    // curArena is the arena that the heap is currently growing
    // into. This should always be physPageSize-aligned.
    internal mheap_curArena curArena;
    // central free lists for small size classes.
    // the padding makes sure that the mcentrals are
    // spaced CacheLinePadSize bytes apart, so that each mcentral.lock
    // gets its own cache line.
    // central is indexed by spanClass.
    internal array<mheap_central> central = new(numSpanClasses, () => new());
    internal fixalloc spanalloc;                  // allocator for span
    internal fixalloc spanSPMCAlloc;              // allocator for spanSPMC, protected by work.spanSPMCs.lock
    internal fixalloc cachealloc;                 // allocator for mcache
    internal fixalloc specialfinalizeralloc;      // allocator for specialfinalizer
    internal fixalloc specialCleanupAlloc;        // allocator for specialCleanup
    internal fixalloc specialCheckFinalizerAlloc; // allocator for specialCheckFinalizer
    internal fixalloc specialTinyBlockAlloc;      // allocator for specialTinyBlock
    internal fixalloc specialprofilealloc;        // allocator for specialprofile
    internal fixalloc specialReachableAlloc;      // allocator for specialReachable
    internal fixalloc specialPinCounterAlloc;     // allocator for specialPinCounter
    internal fixalloc specialWeakHandleAlloc;     // allocator for specialWeakHandle
    internal fixalloc specialBubbleAlloc;         // allocator for specialBubble
    internal fixalloc specialSecretAlloc;         // allocator for specialSecret
    internal mutex speciallock;                   // lock for special record allocators.
    internal fixalloc arenaHintAlloc;             // allocator for arenaHints
    // User arena state.
    //
    // Protected by mheap_.lock.
    internal mheap_userArena userArena;
    // cleanupID is a counter which is incremented each time a cleanup special is added
    // to a span. It's used to create globally unique identifiers for individual cleanup.
    // cleanupID is protected by mheap_.speciallock. It must only be incremented while holding
    // the lock. ID 0 is reserved. Users should increment first, then read the value.
    internal uint64 cleanupID;
    internal cpu.CacheLinePad ___;
    internal immortalWeakHandleMap immortalWeakHandles;
    internal ж<specialfinalizer> unused; // never set, just here to force the specialfinalizer type into DWARF
}

internal static readonly ж<mheap> Ꮡmheap_ = new StaticBox<mheap>(new mheap());
internal static ref mheap mheap_ => ref ((StaticBox<mheap>)Ꮡmheap_).ValueSlot;

// A heapArena stores metadata for a heap arena. heapArenas are stored
// outside of the Go heap and accessed via the mheap_.arenas index.
partial struct heapArena {
    internal sys.NotInHeap _;
    // spans maps from virtual address page ID within this arena to *mspan.
    // For allocated spans, their pages map to the span itself.
    // For free spans, only the lowest and highest pages map to the span itself.
    // Internal pages map to an arbitrary span.
    // For pages that have never been allocated, spans entries are nil.
    //
    // Modifications are protected by mheap.lock. Reads can be
    // performed without locking, but ONLY from indexes that are
    // known to contain in-use or stack spans. This means there
    // must not be a safe-point between establishing that an
    // address is live and looking it up in the spans array.
    internal array<ж<mspan>> spans = new(pagesPerArena);
    // pageInUse is a bitmap that indicates which spans are in
    // state mSpanInUse. This bitmap is indexed by page number,
    // but only the bit corresponding to the first page in each
    // span is used.
    //
    // Reads and writes are atomic.
    internal array<uint8> pageInUse = new(pagesPerArena / 8);
    // pageMarks is a bitmap that indicates which spans have any
    // marked objects on them. Like pageInUse, only the bit
    // corresponding to the first page in each span is used.
    //
    // Writes are done atomically during marking. Reads are
    // non-atomic and lock-free since they only occur during
    // sweeping (and hence never race with writes).
    //
    // This is used to quickly find whole spans that can be freed.
    //
    // TODO(austin): It would be nice if this was uint64 for
    // faster scanning, but we don't have 64-bit atomic bit
    // operations.
    internal array<uint8> pageMarks = new(pagesPerArena / 8);
    // pageSpecials is a bitmap that indicates which spans have
    // specials (finalizers or other). Like pageInUse, only the bit
    // corresponding to the first page in each span is used.
    //
    // Writes are done atomically whenever a special is added to
    // a span and whenever the last special is removed from a span.
    // Reads are done atomically to find spans containing specials
    // during marking.
    internal array<uint8> pageSpecials = new(pagesPerArena / 8);
    // pageUseSpanInlineMarkBits is a bitmap where each bit corresponds
    // to a span, as only spans one page in size can have inline mark bits.
    // The bit indicates that the span has a spanInlineMarkBits struct
    // stored directly at the top end of the span's memory.
    internal array<uint8> pageUseSpanInlineMarkBits = new(pagesPerArena / 8);
    // checkmarks stores the debug.gccheckmark state. It is only
    // used if debug.gccheckmark > 0 or debug.checkfinalizers > 0.
    internal ж<checkmarksMap> checkmarks;
    // zeroedBase marks the first byte of the first page in this
    // arena which hasn't been used yet and is therefore already
    // zero. zeroedBase is relative to the arena base.
    // Increases monotonically until it hits heapArenaBytes.
    //
    // This field is sufficient to determine if an allocation
    // needs to be zeroed because the page allocator follows an
    // address-ordered first-fit policy.
    //
    // Read atomically and written with an atomic CAS.
    internal uintptr zeroedBase;
}

// arenaHint is a hint for where to grow the heap arenas. See
// mheap_.arenaHints.
partial struct arenaHint {
    internal sys.NotInHeap _;
    internal uintptr addr;
    internal bool down;
    internal ж<arenaHint> next;
}

// An mspan is a run of pages.
//
// When a mspan is in the heap free treap, state == mSpanFree
// and heapmap(s->start) == span, heapmap(s->start+s->npages-1) == span.
// If the mspan is in the heap scav treap, then in addition to the
// above scavenged == true. scavenged == false in all other cases.
//
// When a mspan is allocated, state == mSpanInUse or mSpanManual
// and heapmap(i) == span for all s->start <= i < s->start+s->npages.
// Every mspan is in one doubly-linked list, either in the mheap's
// busy list or one of the mcentral's span lists.

// An mspan representing actual memory has state mSpanInUse,
// mSpanManual, or mSpanFree. Transitions between these states are
// constrained as follows:
//
//   - A span may transition from free to in-use or manual during any GC
//     phase.
//
//   - During sweeping (gcphase == _GCoff), a span may transition from
//     in-use to free (as a result of sweeping) or manual to free (as a
//     result of stacks being freed).
//
//   - During GC (gcphase != _GCoff), a span *must not* transition from
//     manual or in-use to free. Because concurrent GC may read a pointer
//     and then look up its span, the span state must be monotonic.
//
// Setting mspan.state to mSpanInUse or mSpanManual must be done
// atomically and only after all other span fields are valid.
// Likewise, if inspecting a span is contingent on it being
// mSpanInUse, the state should be loaded atomically and checked
// before depending on other fields. This allows the garbage collector
// to safely deal with potentially invalid pointers, since resolving
// such pointers may race with a span being allocated.
partial struct mSpanState /*num:uint8*/;

internal static mSpanState mSpanDead => /* iota */ 0;
internal static mSpanState mSpanInUse => 1;  // allocated for garbage collected heap
internal static mSpanState mSpanManual => 2; // allocated for manual management (e.g., stack allocator)

// mSpanStateNames are the names of the span states, indexed by
// mSpanState.
internal static slice<@string> mSpanStateNames = slice<@string>([
    "mSpanDead"u8,
    "mSpanInUse"u8,
    "mSpanManual"u8
]);

// mSpanStateBox holds an atomic.Uint8 to provide atomic operations on
// an mSpanState. This is a separate type to disallow accidental comparison
// or assignment with mSpanState.
partial struct mSpanStateBox {
    internal atomic.Uint8 s;
}

// It is nosplit to match get, below.

//go:nosplit
internal static void set(this ж<mSpanStateBox> Ꮡb, mSpanState s) {
    Ꮡb.of(mSpanStateBox.Ꮡs).Store((uint8)s);
}

// It is nosplit because it's called indirectly by typedmemclr,
// which must not be preempted.

//go:nosplit
internal static mSpanState get(this ж<mSpanStateBox> Ꮡb) {
    return (mSpanState)Ꮡb.of(mSpanStateBox.Ꮡs).Load();
}

partial struct mspan {
    internal sys.NotInHeap _;
    internal ж<mspan> next;     // next span in list, or nil if none
    internal ж<mspan> prev;     // previous span in list, or nil if none
    internal ж<mSpanList> list; // For debugging.
    internal uintptr startAddr; // address of first byte of span aka s.base()
    internal uintptr npages;    // number of pages in span
    internal gclinkptr manualFreeList; // list of free objects in mSpanManual spans
    // freeindex is the slot index between 0 and nelems at which to begin scanning
    // for the next free object in this span.
    // Each allocation scans allocBits starting at freeindex until it encounters a 0
    // indicating a free object. freeindex is then adjusted so that subsequent scans begin
    // just past the newly discovered free object.
    //
    // If freeindex == nelems, this span has no free objects, though might have reusable objects.
    //
    // allocBits is a bitmap of objects in this span.
    // If n >= freeindex and allocBits[n/8] & (1<<(n%8)) is 0
    // then object n is free;
    // otherwise, object n is allocated. Bits starting at nelems are
    // undefined and should never be referenced.
    //
    // Object n starts at address n*elemsize + (start << pageShift).
    internal uint16 freeindex;
    // TODO: Look up nelems from sizeclass and remove this field if it
    // helps performance.
    internal uint16 nelems; // number of object in the span.
    // freeIndexForScan is like freeindex, except that freeindex is
    // used by the allocator whereas freeIndexForScan is used by the
    // GC scanner. They are two fields so that the GC sees the object
    // is allocated only when the object and the heap bits are
    // initialized (see also the assignment of freeIndexForScan in
    // mallocgc, and issue 54596).
    internal uint16 freeIndexForScan;
    // Cache of the allocBits at freeindex. allocCache is shifted
    // such that the lowest bit corresponds to the bit freeindex.
    // allocCache holds the complement of allocBits, thus allowing
    // ctz (count trailing zero) to use it directly.
    // allocCache may contain bits beyond s.nelems; the caller must ignore
    // these.
    internal uint64 allocCache;
    // allocBits and gcmarkBits hold pointers to a span's mark and
    // allocation bits. The pointers are 8 byte aligned.
    // There are three arenas where this data is held.
    // free: Dirty arenas that are no longer accessed
    //       and can be reused.
    // next: Holds information to be used in the next GC cycle.
    // current: Information being used during this GC cycle.
    // previous: Information being used during the last GC cycle.
    // A new GC cycle starts with the call to finishsweep_m.
    // finishsweep_m moves the previous arena to the free arena,
    // the current arena to the previous arena, and
    // the next arena to the current arena.
    // The next arena is populated as the spans request
    // memory to hold gcmarkBits for the next GC cycle as well
    // as allocBits for newly allocated spans.
    //
    // The pointer arithmetic is done "by hand" instead of using
    // arrays to avoid bounds checks along critical performance
    // paths.
    // The sweep will free the old allocBits and set allocBits to the
    // gcmarkBits. The gcmarkBits are replaced with a fresh zeroed
    // out memory.
    internal ж<gcBits> allocBits;
    internal ж<gcBits> gcmarkBits;
    internal ж<gcBits> pinnerBits; // bitmap for pinned objects; accessed atomically
// sweep generation:
// if sweepgen == h->sweepgen - 2, the span needs sweeping
// if sweepgen == h->sweepgen - 1, the span is currently being swept
// if sweepgen == h->sweepgen, the span is swept and ready to use
// if sweepgen == h->sweepgen + 1, the span was cached before sweep began and is still cached, and needs sweeping
// if sweepgen == h->sweepgen + 3, the span was swept and then cached and is still cached
// h->sweepgen is incremented by 2 after every GC
    internal uint32 sweepgen;
    internal uint32 divMul;                // for divide by elemsize
    internal uint16 allocCount;            // number of allocated objects
    internal spanClass spanclass;          // size class and noscan (uint8)
    internal mSpanStateBox state;          // mSpanInUse etc; accessed atomically (get/set methods)
    internal uint8 needzero;               // needs to be zeroed before allocation
    internal bool isUserArenaChunk;        // whether or not this span represents a user arena
    internal uint16 allocCountBeforeCache; // a copy of allocCount that is stored just before this span is cached
    internal uintptr elemsize;             // computed from sizeclass or from npages
    internal uintptr limit;                // end of data in span
    internal mutex speciallock;            // guards specials list and changes to pinnerBits
    internal ж<special> specials;          // linked list of special records sorted by offset.
    internal addrRange userArenaChunkFree; // interval for managing chunk allocation
    internal ж<_type> largeType;           // malloc header for large objects.
}

internal static uintptr @base(this ref mspan s) {
    return s.startAddr;
}

internal static (uintptr size, uintptr n, uintptr total) layout(this ref mspan s) {
    uintptr size = default;
    uintptr n = default;
    uintptr total = default;

    total = s.npages << (int)(gc.PageShift);
    size = s.elemsize;
    if (size > 0) {
        n = total / size;
    }
    return (size, n, total);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string allspansArrayˢ = "allspans array"u8;
internal static readonly @string runtimeCannotAllocateˢ = "runtime: cannot allocate memory"u8;

// recordspan adds a newly allocated span to h.allspans.
//
// This only happens the first time a span is allocated from
// mheap.spanalloc (it is not called when a span is reused).
//
// Write barriers are disallowed here because it can be called from
// gcWork when allocating new workbufs. However, because it's an
// indirect call from the fixalloc initializer, the compiler can't see
// this.
//
// The heap lock must be held.
//
//go:nowritebarrierrec
internal static void recordspan(@unsafe.Pointer vh, @unsafe.Pointer Δp) {
    var h = (ж<mheap>)(uintptr)(vh);
    var s = (ж<mspan>)(uintptr)(Δp);

    assertLockHeld(h.of(mheap.Ꮡlock));

    if (len(h.allspans) >= cap(h.allspans)) {
        nint n = 64 * 1024 / goarch.PtrSize;
        if (n < cap(h.allspans) * 3 / 2) {
            n = cap(h.allspans) * 3 / 2;
        }
        ref var @new = ref heap<slice<ж<mspan>>>(out var Ꮡnew);
        var sp = Ꮡnew.Reinterpret<slice<ж<mspan>>, Δsliceᴛ>();
        sp.Δarray = (uintptr)sysAlloc((uintptr)n * (uintptr)goarch.PtrSize, Ꮡmemstats.of(mstats.Ꮡother_sys), allspansArrayˢ);
        if (sp.Δarray == nil) {
            @throw(runtimeCannotAllocateˢ);
        }
        sp.len = len(h.allspans);
        sp.cap = n;
        if (len(h.allspans) > 0) {
            copy(@new, h.allspans);
        }
        var oldAllspans = h.allspans;
        (h.of(mheap.Ꮡallspans).Reinterpret<slice<ж<mspan>>, notInHeapSlice>()).Value = (Ꮡnew.Reinterpret<slice<ж<mspan>>, notInHeapSlice>()).Value;
        if (len(oldAllspans) != 0) {
            sysFree(@unsafe.Pointer.FromBox(Ꮡ(oldAllspans, 0)), (uintptr)cap(oldAllspans) * /* unsafe.Sizeof(oldAllspans[0]) */ (uintptr)8, Ꮡmemstats.of(mstats.Ꮡother_sys));
        }
    }
    h.allspans = h.allspans.slice(0, len(h.allspans) + 1);
    h.allspans[len(h.allspans) - 1] = s;
}

// A spanClass represents the size class and noscan-ness of a span.
//
// Each size class has a noscan spanClass and a scan spanClass. The
// noscan spanClass contains only noscan objects, which do not contain
// pointers and thus do not need to be scanned by the garbage
// collector.
partial struct spanClass /*num:uint8*/;

internal static UntypedInt numSpanClasses => /* gc.NumSizeClasses << 1 */ 136;
internal static spanClass tinySpanClass => /* spanClass(tinySizeClass<<1 | 1) */ 5;

internal static spanClass makeSpanClass(uint8 sizeclass, bool noscan) {
    return (spanClass)(((spanClass)((uint8)(sizeclass << 1))) | ((spanClass)(uint8)bool2int(noscan)));
}

//go:nosplit
internal static int8 sizeclass(this spanClass sc) {
    return (int8)(sc >> 1);
}

//go:nosplit
internal static bool noscan(this spanClass sc) {
    return (spanClass)(sc & 1) != 0;
}

// arenaIndex returns the index into mheap_.arenas of the arena
// containing metadata for p. This index combines of an index into the
// L1 map and an index into the L2 map and should be used as
// mheap_.arenas[ai.l1()][ai.l2()].
//
// If p is outside the range of valid heap addresses, either l1() or
// l2() will be out of bounds.
//
// It is nosplit because it's called by spanOf and several other
// nosplit functions.
//
//go:nosplit
internal static arenaIdx arenaIndex(uintptr Δp) {
    return (arenaIdx)(nuint)((Δp - (uintptr)arenaBaseOffset) / (uintptr)heapArenaBytes);
}

// arenaBase returns the low address of the region covered by heap
// arena i.
internal static uintptr arenaBase(arenaIdx i) {
    return (uintptr)i * (uintptr)heapArenaBytes + (uintptr)arenaBaseOffset;
}

partial struct arenaIdx /*num:nuint*/;

// l1 returns the "l1" portion of an arenaIdx.
//
// Marked nosplit because it's called by spanOf and other nosplit
// functions.
//
//go:nosplit
internal static nuint l1(this arenaIdx i) {
    if (arenaL1Bits == 0) {
        // Let the compiler optimize this away if there's no
        // L1 map.
        return 0;
    } else {
        return (nuint)i >> (int)(arenaL1Shift);
    }
}

// l2 returns the "l2" portion of an arenaIdx.
//
// Marked nosplit because it's called by spanOf and other nosplit funcs.
// functions.
//
//go:nosplit
internal static nuint l2(this arenaIdx i) {
    if (arenaL1Bits == 0) {
        return (nuint)i;
    } else {
        return (nuint)i & (nuint)((1 << (int)(arenaL2Bits)) - 1);
    }
}

// inheap reports whether b is a pointer into a (potentially dead) heap object.
// It returns false for pointers into mSpanManual spans.
// Non-preemptible because it is used by write barriers.
//
//go:nowritebarrier
//go:nosplit
internal static bool inheap(uintptr b) {
    return spanOfHeap(b) != nil;
}

// inHeapOrStack is a variant of inheap that returns true for pointers
// into any allocated heap span.
//
//go:nowritebarrier
//go:nosplit
internal static bool inHeapOrStack(uintptr b) {
    var s = spanOf(b);
    if (s == nil || b < s.@base()) {
        return false;
    }
    switch (s.of(mspan.Ꮡstate).get()) {
    case var ᴛ when ᴛ == mSpanInUse || ᴛ == mSpanManual: {
        return b < s.limit;
    }
    default: {
        return false;
    }}
}

// go2cs generated this placeholder — func spanOf is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// spanOfUnchecked is equivalent to spanOf, but the caller must ensure
// that p points into an allocated heap arena.
//
// Must be nosplit because it has callers that are nosplit.
//
//go:nosplit
internal static ж<mspan> spanOfUnchecked(uintptr Δp) {
    arenaIdx ai = arenaIndex(Δp);
    return mheap_.arenas[ai.l1()].Value[ai.l2()].spans[(Δp / (uintptr)pageSize) % (uintptr)pagesPerArena];
}

// spanOfHeap is like spanOf, but returns nil if p does not point to a
// heap object.
//
// Must be nosplit because it has callers that are nosplit.
//
//go:nosplit
internal static ж<mspan> spanOfHeap(uintptr Δp) {
    var s = spanOf(Δp);
    // s is nil if it's never been allocated. Otherwise, we check
    // its state first because we don't trust this pointer, so we
    // have to synchronize with span initialization. Then, it's
    // still possible we picked up a stale span pointer, so we
    // have to check the span's bounds.
    if (s == nil || s.of(mspan.Ꮡstate).get() != mSpanInUse || Δp < s.@base() || Δp >= s.limit) {
        return default;
    }
    return s;
}

// pageIndexOf returns the arena, page index, and page mask for pointer p.
// The caller must ensure p is in the heap.
internal static (ж<heapArena> arena, uintptr pageIdx, uint8 pageMask) pageIndexOf(uintptr Δp) {
    ж<heapArena> arena = default;
    uintptr pageIdx = default;
    uint8 pageMask = default;

    arenaIdx ai = arenaIndex(Δp);
    arena = mheap_.arenas[ai.l1()].Value[ai.l2()];
    pageIdx = ((Δp / (uintptr)pageSize) / 8) % (uintptr)len(arena.pageInUse);
    pageMask = (byte)((byte)(1 << (int)((Δp / (uintptr)pageSize) % 8)));
    return (arena, pageIdx, pageMask);
}

// heapArenaOf returns the heap arena for p, if one exists.
internal static ж<heapArena> heapArenaOf(uintptr Δp) {
    arenaIdx ri = arenaIndex(Δp);
    if (arenaL1Bits == 0) {
        // If there's no L1, then ri.l1() can't be out of bounds but ri.l2() can.
        if (ri.l2() >= (nuint)4194304) {
            return default;
        }
    } else {
        // If there's an L1, then ri.l1() can be out of bounds but ri.l2() can't.
        if (ri.l1() >= (nuint)len(mheap_.arenas)) {
            return default;
        }
    }
    var l2 = mheap_.arenas[ri.l1()];
    if (arenaL1Bits != 0 && l2 == nil) { // Should never happen if there's no L1.
        return default;
    }
    return l2.Value[ri.l2()];
}

// Initialize the heap.
internal static void init(this ж<mheap> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    lockInit(ref nonnil(ref h).@lock, lockRankMheap);
    lockInit(ref nonnil(ref h).speciallock, lockRankMheapSpecial);

    h.spanalloc.init(/* unsafe.Sizeof(mspan{}) */ (uintptr)160, recordspan, (uintptr)@unsafe.Pointer.FromRef(ref h), Ꮡmemstats.of(mstats.Ꮡmspan_sys));
    h.spanSPMCAlloc.init(/* unsafe.Sizeof(spanSPMC{}) */ (uintptr)56, default, nil, Ꮡmemstats.of(mstats.ᏑgcMiscSys));
    h.cachealloc.init(/* unsafe.Sizeof(mcache{}) */ (uintptr)2296, default, nil, Ꮡmemstats.of(mstats.Ꮡmcache_sys));
    h.specialfinalizeralloc.init(/* unsafe.Sizeof(specialfinalizer{}) */ (uintptr)56, default, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialCleanupAlloc.init(/* unsafe.Sizeof(specialCleanup{}) */ (uintptr)56, default, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialCheckFinalizerAlloc.init(/* unsafe.Sizeof(specialCheckFinalizer{}) */ (uintptr)56, default, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialTinyBlockAlloc.init(/* unsafe.Sizeof(specialTinyBlock{}) */ (uintptr)24, default, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialprofilealloc.init(/* unsafe.Sizeof(specialprofile{}) */ (uintptr)32, default, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialReachableAlloc.init(/* unsafe.Sizeof(specialReachable{}) */ (uintptr)32, default, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialPinCounterAlloc.init(/* unsafe.Sizeof(specialPinCounter{}) */ (uintptr)32, default, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialSecretAlloc.init(/* unsafe.Sizeof(specialSecret{}) */ (uintptr)32, default, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.specialWeakHandleAlloc.init(/* unsafe.Sizeof(specialWeakHandle{}) */ (uintptr)32, default, nil, Ꮡmemstats.of(mstats.ᏑgcMiscSys));
    h.specialBubbleAlloc.init(/* unsafe.Sizeof(specialBubble{}) */ (uintptr)32, default, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
    h.arenaHintAlloc.init(/* unsafe.Sizeof(arenaHint{}) */ (uintptr)24, default, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));

    // Don't zero mspan allocations. Background sweeping can
    // inspect a span concurrently with allocating it, so it's
    // important that the span's sweepgen survive across freeing
    // and re-allocating a span to prevent background sweeping
    // from improperly cas'ing it from 0.
    //
    // This is safe because mspan contains no heap pointers.
    h.spanalloc.zero = false;

    // h->mapcache needs no init

    foreach (var i in range(h.central)) {
        h.central[i].mcentral.init((spanClass)(uint8)i);
    }

    Ꮡh.of(mheap.Ꮡpages).init(Ꮡh.of(mheap.Ꮡlock), Ꮡmemstats.of(mstats.ᏑgcMiscSys), false);

    xRegInitAlloc();
}

// reclaim sweeps and reclaims at least npage pages into the heap.
// It is called before allocating npage pages to keep growth in check.
//
// reclaim implements the page-reclaimer half of the sweeper.
//
// h.lock must NOT be held.
internal static void reclaim(this ж<mheap> Ꮡh, uintptr npage) {
    ref var h = ref Ꮡh.DerefOrNull();

    // TODO(austin): Half of the time spent freeing spans is in
    // locking/unlocking the heap (even with low contention). We
    // could make the slow path here several times faster by
    // batching heap frees.

    // Bail early if there's no more reclaim work.
    if (Ꮡh.of(mheap.ᏑreclaimIndex).Load() >= (uint64)1 << 63) {
        return;
    }

    // Disable preemption so the GC can't start while we're
    // sweeping, so we can read h.sweepArenas, and so
    // traceGCSweepStart/Done pair on the P.
    var mp = acquirem();

    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepStart();
        traceRelease(Δtrace);
    }

    var arenas = h.sweepArenas;
    var locked = false;
    while (npage > 0) {
        // Pull from accumulated credit first.
        {
            var credit = Ꮡh.of(mheap.ᏑreclaimCredit).Load(); if (credit > 0) {
                var take = credit;
                if (take > npage) {
                    // Take only what we need.
                    take = npage;
                }
                if (Ꮡh.of(mheap.ᏑreclaimCredit).CompareAndSwap(credit, credit - take)) {
                    npage -= take;
                }
                continue;
            }
        }

        // Claim a chunk of work.
        var idx = (uintptr)(Ꮡh.of(mheap.ᏑreclaimIndex).Add(pagesPerReclaimerChunk) - (uint64)pagesPerReclaimerChunk);
        if (idx / (uintptr)pagesPerArena >= (uintptr)len(arenas)) {
            // Page reclaiming is done.
            Ꮡh.of(mheap.ᏑreclaimIndex).Store((uint64)1 << 63);
            break;
        }

        if (!locked) {
            // Lock the heap for reclaimChunk.
            @lock(Ꮡh.of(mheap.Ꮡlock));
            locked = true;
        }

        // Scan this chunk.
        var nfound = Ꮡh.reclaimChunk(arenas, idx, pagesPerReclaimerChunk);
        if (nfound <= npage) {
            npage -= nfound;
        } else {
            // Put spare pages toward global credit.
            Ꮡh.of(mheap.ᏑreclaimCredit).Add(nfound - npage);
            npage = 0;
        }
    }
    if (locked) {
        unlock(Ꮡh.of(mheap.Ꮡlock));
    }

    Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        Δtrace.GCSweepDone();
        traceRelease(Δtrace);
    }
    releasem(ref (mp).DerefOrNull());
}

// reclaimChunk sweeps unmarked spans that start at page indexes [pageIdx, pageIdx+n).
// It returns the number of pages returned to the heap.
//
// h.lock must be held and the caller must be non-preemptible. Note: h.lock may be
// temporarily unlocked and re-locked in order to do sweeping or if tracing is
// enabled.
internal static uintptr reclaimChunk(this ж<mheap> Ꮡh, slice<arenaIdx> arenas, uintptr pageIdx, uintptr n) {
    ref var h = ref Ꮡh.DerefOrNull();

    // The heap lock must be held because this accesses the
    // heapArena.spans arrays using potentially non-live pointers.
    // In particular, if a span were freed and merged concurrently
    // with this probing heapArena.spans, it would be possible to
    // observe arbitrary, stale span pointers.
    assertLockHeld(Ꮡh.of(mheap.Ꮡlock));

    var n0 = n;
    uintptr nFreed = default;
    var sl = ᏑΔsweep.of(sweepdata.Ꮡactive).begin();
    if (!sl.valid) {
        return 0;
    }
    while (n > 0) {
        arenaIdx ai = arenas[pageIdx / (uintptr)pagesPerArena];
        var ha = h.arenas[ai.l1()].Value[ai.l2()];

        // Get a chunk of the bitmap to work on.
        nuint arenaPage = (nuint)(pageIdx % (uintptr)pagesPerArena);
        var inUse = ha.pageInUse.slice((nint)(arenaPage / 8));
        var marked = ha.pageMarks.slice((nint)(arenaPage / 8));
        if ((uintptr)len(inUse) > n / 8) {
            inUse = inUse.slice(0, (nint)(n / 8));
            marked = marked.slice(0, (nint)(n / 8));
        }

        // Scan this bitmap chunk for spans that are in-use
        // but have no marked objects on them.
        foreach (var i in range(inUse)) {
            var inUseUnmarked = (uint8)(atomic.Load8(Ꮡ(inUse, i)) & ~marked[i]);
            if (inUseUnmarked == 0) {
                continue;
            }

            for (nuint j = (nuint)0; j < 8; j++) {
                if ((uint8)(inUseUnmarked & (((uint8)1).Lsh(j))) != 0) {
                    var s = ha.spans[arenaPage + (nuint)i * 8 + j];
                    {
                        var (sΔ1, ok) = sl.tryAcquire(s); if (ok) {
                            var npages = sΔ1.npages;
                            unlock(Ꮡh.of(mheap.Ꮡlock));
                            if (sΔ1.sweep(false)) {
                                nFreed += npages;
                            }
                            @lock(Ꮡh.of(mheap.Ꮡlock));
                            // Reload inUse. It's possible nearby
                            // spans were freed when we dropped the
                            // lock and we don't want to get stale
                            // pointers from the spans array.
                            inUseUnmarked = (uint8)(atomic.Load8(Ꮡ(inUse, i)) & ~marked[i]);
                        }
                    }
                }
            }
        }

        // Advance.
        pageIdx += (uintptr)(len(inUse) * 8);
        n -= (uintptr)(len(inUse) * 8);
    }
    ᏑΔsweep.of(sweepdata.Ꮡactive).end(sl);
    var Δtrace = traceAcquire();
    if (Δtrace.ok()) {
        unlock(Ꮡh.of(mheap.Ꮡlock));
        // Account for pages scanned but not reclaimed.
        Δtrace.GCSweepSpan((n0 - nFreed) * (uintptr)pageSize);
        traceRelease(Δtrace);
        @lock(Ꮡh.of(mheap.Ꮡlock));
    }

    assertLockHeld(Ꮡh.of(mheap.Ꮡlock)); // Must be locked on return.
    return nFreed;
}

// spanAllocType represents the type of allocation to make, or
// the type of allocation to be freed.
partial struct spanAllocType /*num:uint8*/;

internal static spanAllocType spanAllocHeap => /* iota */ 0; // heap span
internal static spanAllocType spanAllocStack => 1;           // stack span
internal static spanAllocType spanAllocWorkBuf => 2;         // work buf span

// manual returns true if the span allocation is manually managed.
internal static bool manual(this spanAllocType s) {
    return s != spanAllocHeap;
}

// alloc allocates a new span of npage pages from the GC'd heap.
//
// spanclass indicates the span's size class and scannability.
//
// Returns a span that has been fully initialized. span.needzero indicates
// whether the span has been zeroed. Note that it may not be.
internal static ж<mspan> alloc(this ж<mheap> Ꮡh, uintptr npages, spanClass spanclass) {
    // Don't do any operations that lock the heap on the G stack.
    // It might trigger stack growth, and the stack growth code needs
    // to be able to allocate heap.
    ref var s = ref heap<ж<mspan>>(out var Ꮡs);
    systemstack(() => {
        // To prevent excessive heap growth, before allocating n pages
        // we need to sweep and reclaim at least n pages.
        if (!isSweepDone()) {
            Ꮡh.reclaim(npages);
        }
        Ꮡs.ValueSlot = Ꮡh.allocSpan(npages, spanAllocHeap, spanclass);
    });
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string manualSpanAllocationˢ = "manual span allocation called with non-manually-managed type"u8;

// allocManual allocates a manually-managed span of npage pages.
// allocManual returns nil if allocation fails.
//
// allocManual adds the bytes used to *stat, which should be a
// memstats in-use field. Unlike allocations in the GC'd heap, the
// allocation does *not* count toward heapInUse.
//
// The memory backing the returned span may not be zeroed if
// span.needzero is set.
//
// allocManual must be called on the system stack because it may
// acquire the heap lock via allocSpan. See mheap for details.
//
// If new code is written to call allocManual, do NOT use an
// existing spanAllocType value and instead declare a new one.
//
//go:systemstack
internal static ж<mspan> allocManual(this ж<mheap> Ꮡh, uintptr npages, spanAllocType typ) {
    if (!typ.manual()) {
        @throw(manualSpanAllocationˢ);
    }
    return Ꮡh.allocSpan(npages, typ, 0);
}

// setSpans modifies the span map so [spanOf(base), spanOf(base+npage*pageSize))
// is s.
internal static void setSpans(this ref mheap h, uintptr @base, uintptr npage, ж<mspan> s) {
    var Δp = @base / (uintptr)pageSize;
    arenaIdx ai = arenaIndex(@base);
    var ha = h.arenas[ai.l1()].Value[ai.l2()];
    for (var n = (uintptr)0; n < npage; n++) {
        var i = (Δp + n) % (uintptr)pagesPerArena;
        if (i == 0) {
            ai = arenaIndex(@base + n * (uintptr)pageSize);
            ha = h.arenas[ai.l1()].Value[ai.l2()];
        }
        ha.spans[i] = s;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string potentiallyOverlappingInˢ = "potentially overlapping in-use allocations detected"u8;

// allocNeedsZero checks if the region of address space [base, base+npage*pageSize),
// assumed to be allocated, needs to be zeroed, updating heap arena metadata for
// future allocations.
//
// This must be called each time pages are allocated from the heap, even if the page
// allocator can otherwise prove the memory it's allocating is already zero because
// they're fresh from the operating system. It updates heapArena metadata that is
// critical for future page allocations.
//
// There are no locking constraints on this method.
internal static bool /*needZero*/ allocNeedsZero(this ref mheap h, uintptr @base, uintptr npage) {
    bool needZero = default;

    while (npage > 0) {
        arenaIdx ai = arenaIndex(@base);
        var ha = h.arenas[ai.l1()].Value[ai.l2()];

        var zeroedBase = atomic.Loaduintptr(ha.of(heapArena.ᏑzeroedBase));
        var arenaBase = @base % (uintptr)heapArenaBytes;
        if (arenaBase < zeroedBase) {
            // We extended into the non-zeroed part of the
            // arena, so this region needs to be zeroed before use.
            //
            // zeroedBase is monotonically increasing, so if we see this now then
            // we can be sure we need to zero this memory region.
            //
            // We still need to update zeroedBase for this arena, and
            // potentially more arenas.
            needZero = true;
        }
        // We may observe arenaBase > zeroedBase if we're racing with one or more
        // allocations which are acquiring memory directly before us in the address
        // space. But, because we know no one else is acquiring *this* memory, it's
        // still safe to not zero.

        // Compute how far into the arena we extend into, capped
        // at heapArenaBytes.
        var arenaLimit = arenaBase + npage * (uintptr)pageSize;
        if (arenaLimit > heapArenaBytes) {
            arenaLimit = heapArenaBytes;
        }
        // Increase ha.zeroedBase so it's >= arenaLimit.
        // We may be racing with other updates.
        while (arenaLimit > zeroedBase) {
            if (atomic.Casuintptr(ha.of(heapArena.ᏑzeroedBase), zeroedBase, arenaLimit)) {
                break;
            }
            zeroedBase = atomic.Loaduintptr(ha.of(heapArena.ᏑzeroedBase));
            // Double check basic conditions of zeroedBase.
            if (zeroedBase <= arenaLimit && zeroedBase > arenaBase) {
                // The zeroedBase moved into the space we were trying to
                // claim. That's very bad, and indicates someone allocated
                // the same region we did.
                @throw(potentiallyOverlappingInˢ);
            }
        }

        // Move base forward and subtract from npage to move into
        // the next arena, or finish.
        @base += arenaLimit - arenaBase;
        npage -= (arenaLimit - arenaBase) / (uintptr)pageSize;
    }
    return needZero;
}

// tryAllocMSpan attempts to allocate an mspan object from
// the P-local cache, but may fail.
//
// h.lock need not be held.
//
// This caller must ensure that its P won't change underneath
// it during this function. Currently to ensure that we enforce
// that the function is run on the system stack, because that's
// the only place it is used now. In the future, this requirement
// may be relaxed if its use is necessary elsewhere.
//
//go:systemstack
internal static ж<mspan> tryAllocMSpan(this ref mheap h) {
    var pp = (~(~getg()).m).p.ptr();
    // If we don't have a p or the cache is empty, we can't do
    // anything here.
    if (pp == nil || (~pp).mspancache.len == 0) {
        return default;
    }
    // Pull off the last entry in the cache.
    var s = (~pp).mspancache.buf[(~pp).mspancache.len - 1];
    pp.Value.mspancache.len--;
    return s;
}

// allocMSpanLocked allocates an mspan object.
//
// h.lock must be held.
//
// allocMSpanLocked must be called on the system stack because
// its caller holds the heap lock. See mheap for details.
// Running on the system stack also ensures that we won't
// switch Ps during this function. See tryAllocMSpan for details.
//
//go:systemstack
internal static ж<mspan> allocMSpanLocked(this ж<mheap> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    assertLockHeld(Ꮡh.of(mheap.Ꮡlock));

    var pp = (~(~getg()).m).p.ptr();
    if (pp == nil) {
        // We don't have a p so just do the normal thing.
        return (ж<mspan>)(uintptr)(h.spanalloc.alloc());
    }
    // Refill the cache if necessary.
    if ((~pp).mspancache.len == 0) {
        const nint refillCount = /* len(pp.mspancache.buf) / 2 */ 64;
        for (nint i = 0; i < refillCount; i++) {
            pp.Value.mspancache.buf[i] = (ж<mspan>)(uintptr)(h.spanalloc.alloc());
        }
        pp.Value.mspancache.len = refillCount;
    }
    // Pull off the last entry in the cache.
    var s = (~pp).mspancache.buf[(~pp).mspancache.len - 1];
    pp.Value.mspancache.len--;
    return s;
}

// freeMSpanLocked free an mspan object.
//
// h.lock must be held.
//
// freeMSpanLocked must be called on the system stack because
// its caller holds the heap lock. See mheap for details.
// Running on the system stack also ensures that we won't
// switch Ps during this function. See tryAllocMSpan for details.
//
//go:systemstack
internal static void freeMSpanLocked(this ж<mheap> Ꮡh, ж<mspan> s) {
    ref var h = ref Ꮡh.DerefOrNull();

    assertLockHeld(Ꮡh.of(mheap.Ꮡlock));

    var pp = (~(~getg()).m).p.ptr();
    // First try to free the mspan directly to the cache.
    if (pp != nil && (~pp).mspancache.len < len((~pp).mspancache.buf)) {
        pp.Value.mspancache.buf[(~pp).mspancache.len] = s;
        pp.Value.mspancache.len++;
        return;
    }
    // Failing that (or if we don't have a p), just free it to
    // the heap.
    h.spanalloc.free(@unsafe.Pointer.FromPinnedBox(s));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string grewHeapButNoAdequateˢ = "grew heap, but no adequate free space found"u8;

// allocSpan allocates an mspan which owns npages worth of memory.
//
// If typ.manual() == false, allocSpan allocates a heap span of class spanclass
// and updates heap accounting. If manual == true, allocSpan allocates a
// manually-managed span (spanclass is ignored), and the caller is
// responsible for any accounting related to its use of the span. Either
// way, allocSpan will atomically add the bytes in the newly allocated
// span to *sysStat.
//
// The returned span is fully initialized.
//
// h.lock must not be held.
//
// allocSpan must be called on the system stack both because it acquires
// the heap lock and because it must block GC transitions.
//
//go:systemstack
internal static ж<mspan> /*s*/ allocSpan(this ж<mheap> Ꮡh, uintptr npages, spanAllocType typ, spanClass spanclass) {
    ж<mspan> s = default;
    ref var h = ref Ꮡh.DerefOrNull();

    // Function-global state.
    var gp = getg();
    var (@base, scav) = ((uintptr)0, (uintptr)0);
    var growth = (uintptr)0;

    // On some platforms we need to provide physical page aligned stack
    // allocations. Where the page size is less than the physical page
    // size, we already manage to do this by default.
    var needPhysPageAlign = physPageAlignedStacks && typ == spanAllocStack && pageSize < physPageSize;

    // If the allocation is small enough, try the page cache!
    // The page cache does not support aligned allocations, so we cannot use
    // it if we need to provide a physical page aligned stack allocation.
    var pp = (~(~gp).m).p.ptr();
    if (!needPhysPageAlign && pp != nil && npages < pageCachePages / 4) {
        var c = pp.of(runtime_package.Δp.Ꮡpcache);

        // If the cache is empty, refill it.
        if (c.empty()) {
            @lock(Ꮡh.of(mheap.Ꮡlock));
            c.Value = h.pages.allocToCache();
            unlock(Ꮡh.of(mheap.Ꮡlock));
        }

        // Try to allocate from the cache.
        (@base, scav) = c.alloc(npages);
        if (@base != 0) {
            s = h.tryAllocMSpan();
            if (s != nil) {
                goto HaveSpan;
            }
            // We have a base but no mspan, so we need
            // to lock the heap.
        }
    }

    // For one reason or another, we couldn't get the
    // whole job done without the heap lock.
    @lock(Ꮡh.of(mheap.Ꮡlock));

    if (needPhysPageAlign) {
        // Overallocate by a physical page to allow for later alignment.
        var extraPages = physPageSize / (uintptr)pageSize;

        // Find a big enough region first, but then only allocate the
        // aligned portion. We can't just allocate and then free the
        // edges because we need to account for scavenged memory, and
        // that's difficult with alloc.
        //
        // Note that we skip updates to searchAddr here. It's OK if
        // it's stale and higher than normal; it'll operate correctly,
        // just come with a performance cost.
        (@base, _) = h.pages.find(npages + extraPages);
        if (@base == 0) {
            bool ok = default;
            (growth, ok) = Ꮡh.grow(npages + extraPages);
            if (!ok) {
                unlock(Ꮡh.of(mheap.Ꮡlock));
                return default;
            }
            (@base, _) = h.pages.find(npages + extraPages);
            if (@base == 0) {
                @throw(grewHeapButNoAdequateˢ);
            }
        }
        @base = alignUp(@base, physPageSize);
        scav = h.pages.allocRange(@base, npages);
    }

    if (@base == 0) {
        // Try to acquire a base address.
        (@base, scav) = h.pages.alloc(npages);
        if (@base == 0) {
            bool ok = default;
            (growth, ok) = Ꮡh.grow(npages);
            if (!ok) {
                unlock(Ꮡh.of(mheap.Ꮡlock));
                return default;
            }
            (@base, scav) = h.pages.alloc(npages);
            if (@base == 0) {
                @throw(grewHeapButNoAdequateˢ);
            }
        }
    }
    if (s == nil) {
        // We failed to get an mspan earlier, so grab
        // one now that we have the heap lock.
        s = Ꮡh.allocMSpanLocked();
    }
    unlock(Ꮡh.of(mheap.Ꮡlock));

HaveSpan:
    var bytesToScavenge = (uintptr)0;
    // Decide if we need to scavenge in response to what we just allocated.
    // Specifically, we track the maximum amount of memory to scavenge of all
    // the alternatives below, assuming that the maximum satisfies *all*
    // conditions we check (e.g. if we need to scavenge X to satisfy the
    // memory limit and Y to satisfy heap-growth scavenging, and Y > X, then
    // it's fine to pick Y, because the memory limit is still satisfied).
    //
    // It's fine to do this after allocating because we expect any scavenged
    // pages not to get touched until we return. Simultaneously, it's important
    // to do this before calling sysUsed because that may commit address space.
    var forceScavenge = false;
    {
        var limit = ᏑgcController.of(gcControllerState.ᏑmemoryLimit).Load(); if (!ᏑgcCPULimiter.limiting()) {
            // Assist with scavenging to maintain the memory limit by the amount
            // that we expect to page in.
            var inuse = ᏑgcController.of(gcControllerState.ᏑmappedReady).Load();
            // Be careful about overflow, especially with uintptrs. Even on 32-bit platforms
            // someone can set a really big memory limit that isn't math.MaxInt64.
            if ((uint64)scav + inuse > (uint64)limit) {
                bytesToScavenge = (uintptr)((uint64)scav + inuse - (uint64)limit);
                forceScavenge = true;
            }
        }
    }
    {
        var goal = ᏑΔscavenge.of(runtime_package.Δscavengeᴛ1.ᏑgcPercentGoal).Load(); if (goal != ~(uint64)0 && growth > 0) {
            // We just caused a heap growth, so scavenge down what will soon be used.
            // By scavenging inline we deal with the failure to allocate out of
            // memory fragments by scavenging the memory fragments that are least
            // likely to be re-used.
            //
            // Only bother with this because we're not using a memory limit. We don't
            // care about heap growths as long as we're under the memory limit, and the
            // previous check for scaving already handles that.
            {
                var retained = heapRetained(); if (retained + (uint64)growth > goal) {
                    // The scavenging algorithm requires the heap lock to be dropped so it
                    // can acquire it only sparingly. This is a potentially expensive operation
                    // so it frees up other goroutines to allocate in the meanwhile. In fact,
                    // they can make use of the growth we just created.
                    var todo = growth;
                    {
                        var overage = (uintptr)(retained + (uint64)growth - goal); if (todo > overage) {
                            todo = overage;
                        }
                    }
                    if (todo > bytesToScavenge) {
                        bytesToScavenge = todo;
                    }
                }
            }
        }
    }
    // There are a few very limited circumstances where we won't have a P here.
    // It's OK to simply skip scavenging in these cases. Something else will notice
    // and pick up the tab.
    int64 now = default;
    if (pp != nil && bytesToScavenge > 0) {
        // Measure how long we spent scavenging and add that measurement to the assist
        // time so we can track it for the GC CPU limiter.
        //
        // Limiter event tracking might be disabled if we end up here
        // while on a mark worker.
        var start = nanotime();
        var track = pp.of(runtime_package.Δp.ᏑlimiterEvent).start(limiterEventScavengeAssist, start);

        // Scavenge, but back out if the limiter turns on.
        var released = Ꮡh.of(mheap.Ꮡpages).scavenge(bytesToScavenge, () => ᏑgcCPULimiter.limiting(), forceScavenge);

        Ꮡmheap_.of(mheap.Ꮡpages).of(pageAlloc.Ꮡscav).of(pageAlloc_scav.ᏑreleasedEager).Add(released);

        // Finish up accounting.
        now = nanotime();
        if (track) {
            pp.of(runtime_package.Δp.ᏑlimiterEvent).stop(limiterEventScavengeAssist, now);
        }
        ᏑΔscavenge.of(runtime_package.Δscavengeᴛ1.ᏑassistTime).Add(now - start);
    }

    // Initialize the span.
    Ꮡh.initSpan(s, typ, spanclass, @base, npages, scav);

    if (valgrindenabled) {
        valgrindMempoolMalloc((@unsafe.Pointer)arenaBase(arenaIndex(@base)), (@unsafe.Pointer)@base, npages * (uintptr)pageSize);
    }

    // Commit and account for any scavenged memory that the span now owns.
    var nbytes = npages * (uintptr)pageSize;
    if (scav != 0) {
        // sysUsed all the pages that are actually available
        // in the span since some of them might be scavenged.
        sysUsed((@unsafe.Pointer)@base, nbytes, scav);
        ᏑgcController.of(gcControllerState.ᏑheapReleased).add(-(int64)scav);
    }
    // Update stats.
    ᏑgcController.of(gcControllerState.ᏑheapFree).add(-(int64)(nbytes - scav));
    if (typ == spanAllocHeap) {
        ᏑgcController.of(gcControllerState.ᏑheapInUse).add((int64)nbytes);
    }
    // Update consistent stats.
    var stats = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
    atomic.Xaddint64(stats.of(heapStatsDelta.Ꮡcommitted), (int64)scav);
    atomic.Xaddint64(stats.of(heapStatsDelta.Ꮡreleased), -(int64)scav);
    switch (typ) {
    case var ᴛ when ᴛ == spanAllocHeap: {
        atomic.Xaddint64(stats.of(heapStatsDelta.ᏑinHeap), (int64)nbytes);
        break;
    }
    case var ᴛ when ᴛ == spanAllocStack: {
        atomic.Xaddint64(stats.of(heapStatsDelta.ᏑinStacks), (int64)nbytes);
        break;
    }
    case var ᴛ when ᴛ == spanAllocWorkBuf: {
        atomic.Xaddint64(stats.of(heapStatsDelta.ᏑinWorkBufs), (int64)nbytes);
        break;
    }}
    Ꮡmemstats.of(mstats.ᏑheapStats).release();

    // Trace the span alloc.
    if (traceAllocFreeEnabled()) {
        var Δtrace = traceAcquire();
        if (Δtrace.ok()) {
            Δtrace.SpanAlloc(s);
            traceRelease(Δtrace);
        }
    }
    return s;
}

// initSpan initializes a blank span s which will represent the range
// [base, base+npages*pageSize). typ is the type of span being allocated.
internal static void initSpan(this ж<mheap> Ꮡh, ж<mspan> Ꮡs, spanAllocType typ, spanClass spanclass, uintptr @base, uintptr npages, uintptr scav) {
    ref var h = ref Ꮡh.DerefOrNull();
    ref var s = ref Ꮡs.DerefOrNull();

    // At this point, both s != nil and base != 0, and the heap
    // lock is no longer held. Initialize the span.
    Ꮡs.init(@base, npages);
    // Always call allocNeedsZero to update the arena's zeroedBase watermark
    // and determine if the memory is considered dirty.
    var needZero = h.allocNeedsZero(@base, npages);
    // If these pages were scavenged (returned to the OS), the kernel guarantees
    // they will be zero-filled on next use (fault-in), so we can treat them as
    // already zeroed and skip explicit clearing.
    if ((needZeroAfterSysUnused() || scav != npages * (uintptr)pageSize) && needZero) {
        s.needzero = 1;
    }
    var nbytes = npages * (uintptr)pageSize;
    if (typ.manual()) {
        s.manualFreeList = 0;
        s.nelems = 0;
        Ꮡs.of(mspan.Ꮡstate).set(mSpanManual);
    } else {
        // We must set span properties before the span is published anywhere
        // since we're not holding the heap lock.
        s.spanclass = spanclass;
        {
            var sizeclass = spanclass.sizeclass(); if (sizeclass == 0) {
                s.elemsize = nbytes;
                s.nelems = 1;
                s.divMul = 0;
            } else {
                s.elemsize = (uintptr)gc.SizeClassToSize[sizeclass];
                if (goexperiment.GreenTeaGC) {
                    uintptr reserve = default;
                    if (gcUsesSpanInlineMarkBits(s.elemsize)) {
                        // Reserve space for the inline mark bits.
                        reserve += /* unsafe.Sizeof(spanInlineMarkBits{}) */ (uintptr)128;
                    }
                    if (heapBitsInSpan(s.elemsize) && !s.spanclass.noscan()) {
                        // Reserve space for the pointer/scan bitmap at the end.
                        reserve += nbytes / (uintptr)goarch.PtrSize / 8;
                    }
                    s.nelems = (uint16)((nbytes - reserve) / s.elemsize);
                } else {
                    if (!s.spanclass.noscan() && heapBitsInSpan(s.elemsize)) {
                        // Reserve space for the pointer/scan bitmap at the end.
                        s.nelems = (uint16)((nbytes - (nbytes / (uintptr)goarch.PtrSize / 8)) / s.elemsize);
                    } else {
                        s.nelems = (uint16)(nbytes / s.elemsize);
                    }
                }
                s.divMul = gc.SizeClassToDivMagic[sizeclass];
            }
        }

        // Initialize mark and allocation structures.
        s.freeindex = 0;
        s.freeIndexForScan = 0;
        s.allocCache = ~(uint64)0; // all 1s indicating all free.
        s.gcmarkBits = newMarkBits((uintptr)s.nelems);
        s.allocBits = newAllocBits((uintptr)s.nelems);

        // Adjust s.limit down to the object-containing part of the span.
        s.limit = s.@base() + s.elemsize * (uintptr)s.nelems;

        // It's safe to access h.sweepgen without the heap lock because it's
        // only ever updated with the world stopped and we run on the
        // systemstack which blocks a STW transition.
        atomic.Store(Ꮡs.of(mspan.Ꮡsweepgen), h.sweepgen);

        // Now that the span is filled in, set its state. This
        // is a publication barrier for the other fields in
        // the span. While valid pointers into this span
        // should never be visible until the span is returned,
        // if the garbage collector finds an invalid pointer,
        // access to the span may race with initialization of
        // the span. We resolve this race by atomically
        // setting the state after the span is fully
        // initialized, and atomically checking the state in
        // any situation where a pointer is suspect.
        Ꮡs.of(mspan.Ꮡstate).set(mSpanInUse);
    }

    // Publish the span in various locations.

    // This is safe to call without the lock held because the slots
    // related to this span will only ever be read or modified by
    // this thread until pointers into the span are published (and
    // we execute a publication barrier at the end of this function
    // before that happens) or pageInUse is updated.
    h.setSpans(s.@base(), npages, Ꮡs);

    if (!typ.manual()) {
        // Mark in-use span in arena page bitmap.
        //
        // This publishes the span to the page sweeper, so
        // it's imperative that the span be completely initialized
        // prior to this line.
        var (arena, pageIdx, pageMask) = pageIndexOf(s.@base());
        atomic.Or8(arena.at(heapArena.ᏑpageInUse, (ulong)(pageIdx)), pageMask);

        // Mark packed span.
        if (gcUsesSpanInlineMarkBits(s.elemsize)) {
            atomic.Or8(arena.at(heapArena.ᏑpageUseSpanInlineMarkBits, (ulong)(pageIdx)), pageMask);
        }

        // Update related page sweeper stats.
        Ꮡh.of(mheap.ᏑpagesInUse).Add(npages);
    }

    // Make sure the newly allocated span will be observed
    // by the GC before pointers into the span are published.
    publicationBarrier();
}

// Try to add at least npage pages of memory to the heap,
// returning how much the heap grew by and whether it worked.
//
// h.lock must be held.
internal static (uintptr, bool) grow(this ж<mheap> Ꮡh, uintptr npage) {
    ref var h = ref Ꮡh.DerefOrNull();

    assertLockHeld(Ꮡh.of(mheap.Ꮡlock));

    var firstGrow = h.curArena.@base == 0;

    // We must grow the heap in whole palloc chunks.
    // We call sysMap below but note that because we
    // round up to pallocChunkPages which is on the order
    // of MiB (generally >= to the huge page size) we
    // won't be calling it too much.
    var ask = alignUp(npage, pallocChunkPages) * (uintptr)pageSize;

    var totalGrowth = (uintptr)0;
    // This may overflow because ask could be very large
    // and is otherwise unrelated to h.curArena.base.
    var end = h.curArena.@base + ask;
    var nBase = alignUp(end, physPageSize);
    if (nBase > h.curArena.end || /* overflow */ end < h.curArena.@base) {
        // Not enough room in the current arena. Allocate more
        // arena space. This may not be contiguous with the
        // current arena, so we have to request the full ask.
        var (av, asize) = Ꮡh.sysAlloc(ask, Ꮡh.of(mheap.ᏑarenaHints), Ꮡh.of(mheap.ᏑheapArenas));
        if (av == nil) {
            var inUse = ᏑgcController.of(gcControllerState.ᏑheapFree).load() + ᏑgcController.of(gcControllerState.ᏑheapReleased).load() + ᏑgcController.of(gcControllerState.ᏑheapInUse).load();
            print((@string)"runtime: out of memory: cannot allocate "u8, ask, (@string)"-byte block ("u8, inUse, (@string)" in use)\n"u8);
            return (0, false);
        }

        if ((uintptr)av == h.curArena.end) {
            // The new space is contiguous with the old
            // space, so just extend the current space.
            h.curArena.end = (uintptr)av + asize;
        } else {
            // The new space is discontiguous. Track what
            // remains of the current space and switch to
            // the new space. This should be rare.
            {
                var size = h.curArena.end - h.curArena.@base; if (size != 0) {
                    // Transition this space from Reserved to Prepared and mark it
                    // as released since we'll be able to start using it after updating
                    // the page allocator and releasing the lock at any time.
                    sysMap((@unsafe.Pointer)h.curArena.@base, size, ᏑgcController.of(gcControllerState.ᏑheapReleased), heapˢ);
                    // Update stats.
                    var statsΔ1 = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
                    atomic.Xaddint64(statsΔ1.of(heapStatsDelta.Ꮡreleased), (int64)size);
                    Ꮡmemstats.of(mstats.ᏑheapStats).release();
                    // Update the page allocator's structures to make this
                    // space ready for allocation.
                    Ꮡh.of(mheap.Ꮡpages).grow(h.curArena.@base, size);
                    totalGrowth += size;
                }
            }
            // Switch to the new space.
            h.curArena.@base = (uintptr)av;
            h.curArena.end = (uintptr)av + asize;

            if (firstGrow && randomizeHeapBase) {
                // The top heapAddrBits-logHeapArenaBytes are randomized, we now
                // want to randomize the next
                // logHeapArenaBytes-log2(pallocChunkBytes) bits, making sure
                // h.curArena.base is aligned to pallocChunkBytes.
                nint bits = logHeapArenaBytes - logPallocChunkBytes;
                var offset = nextHeapRandBits(bits);
                h.curArena.@base = alignDown((uintptr)(h.curArena.@base | (offset << (int)(logPallocChunkBytes))), pallocChunkBytes);
            }
        }

        // Recalculate nBase.
        // We know this won't overflow, because sysAlloc returned
        // a valid region starting at h.curArena.base which is at
        // least ask bytes in size.
        nBase = alignUp(h.curArena.@base + ask, physPageSize);
    }

    // Grow into the current arena.
    var v = h.curArena.@base;
    h.curArena.@base = nBase;

    // Transition the space we're going to use from Reserved to Prepared.
    //
    // The allocation is always aligned to the heap arena
    // size which is always > physPageSize, so its safe to
    // just add directly to heapReleased.
    sysMap((@unsafe.Pointer)v, nBase - v, ᏑgcController.of(gcControllerState.ᏑheapReleased), heapˢ);

    // The memory just allocated counts as both released
    // and idle, even though it's not yet backed by spans.
    var stats = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
    atomic.Xaddint64(stats.of(heapStatsDelta.Ꮡreleased), (int64)(nBase - v));
    Ꮡmemstats.of(mstats.ᏑheapStats).release();

    // Update the page allocator's structures to make this
    // space ready for allocation.
    Ꮡh.of(mheap.Ꮡpages).grow(v, nBase - v);
    totalGrowth += nBase - v;

    if (firstGrow && randomizeHeapBase) {
        // The top heapAddrBits-log2(pallocChunkBytes) bits are now randomized,
        // we finally want to randomize the next
        // log2(pallocChunkBytes)-log2(pageSize) bits, while maintaining
        // alignment to pageSize. We do this by calculating a random number of
        // pages into the current arena, and marking them as allocated. The
        // address of the next available page becomes our fully randomized base
        // heap address.
        var randOffset = nextHeapRandBits(logPallocChunkBytes);
        var randNumPages = alignDown(randOffset, pageSize) / (uintptr)pageSize;
        if (randNumPages != 0) {
            h.pages.markRandomPaddingPages(v, randNumPages);
        }
    }

    return (totalGrowth, true);
}

// Free the span back into the heap.
internal static void freeSpan(this ж<mheap> Ꮡh, ж<mspan> s) {
    systemstack(() => {
        // Trace the span free.
        if (traceAllocFreeEnabled()) {
            var Δtrace = traceAcquire();
            if (Δtrace.ok()) {
                Δtrace.SpanFree(s);
                traceRelease(Δtrace);
            }
        }

        @lock(Ꮡh.of(mheap.Ꮡlock));
        if (msanenabled) {
            // Tell msan that this entire span is no longer in use.
            @unsafe.Pointer @base = (@unsafe.Pointer)s.Value.@base();
            var bytes = s.Value.npages << (int)(gc.PageShift);
            msanfree(@base, bytes);
        }
        if (asanenabled) {
            // Tell asan that this entire span is no longer in use.
            @unsafe.Pointer @base = (@unsafe.Pointer)s.Value.@base();
            var bytes = s.Value.npages << (int)(gc.PageShift);
            asanpoison(@base, bytes);
        }
        if (valgrindenabled) {
            var @base = s.Value.@base();
            valgrindMempoolFree((@unsafe.Pointer)arenaBase(arenaIndex(@base)), (@unsafe.Pointer)@base);
        }
        Ꮡh.freeSpanLocked(s, spanAllocHeap);
        unlock(Ꮡh.of(mheap.Ꮡlock));
    });
}

// freeManual frees a manually-managed span returned by allocManual.
// typ must be the same as the spanAllocType passed to the allocManual that
// allocated s.
//
// This must only be called when gcphase == _GCoff. See mSpanState for
// an explanation.
//
// freeManual must be called on the system stack because it acquires
// the heap lock. See mheap for details.
//
//go:systemstack
internal static void freeManual(this ж<mheap> Ꮡh, ж<mspan> Ꮡs, spanAllocType typ) {
    ref var s = ref Ꮡs.DerefOrNull();

    // Trace the span free.
    if (traceAllocFreeEnabled()) {
        var Δtrace = traceAcquire();
        if (Δtrace.ok()) {
            Δtrace.SpanFree(Ꮡs);
            traceRelease(Δtrace);
        }
    }

    s.needzero = 1;
    @lock(Ꮡh.of(mheap.Ꮡlock));
    if (valgrindenabled) {
        var @base = s.@base();
        valgrindMempoolFree((@unsafe.Pointer)arenaBase(arenaIndex(@base)), (@unsafe.Pointer)@base);
    }
    Ꮡh.freeSpanLocked(Ꮡs, typ);
    unlock(Ꮡh.of(mheap.Ꮡlock));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mheapFreeSpanLockedˢ = "mheap.freeSpanLocked - invalid stack free"u8;
internal static readonly @string mheapFreeSpanLockedˢ2 = "mheap.freeSpanLocked - invalid free of user arena chunk"u8;
internal static readonly @string mheapFreeSpanLockedˢ3 = "mheap.freeSpanLocked - invalid free"u8;
internal static readonly @string mheapFreeSpanLockedˢ4 = "mheap.freeSpanLocked - invalid span state"u8;

internal static void freeSpanLocked(this ж<mheap> Ꮡh, ж<mspan> Ꮡs, spanAllocType typ) {
    ref var h = ref Ꮡh.DerefOrNull();
    ref var s = ref Ꮡs.DerefOrNull();

    assertLockHeld(Ꮡh.of(mheap.Ꮡlock));

    switch (Ꮡs.of(mspan.Ꮡstate).get()) {
    case var ᴛ when ᴛ == mSpanManual: {
        if (s.allocCount != 0) {
            @throw(mheapFreeSpanLockedˢ);
        }
        break;
    }
    case var ᴛ when ᴛ == mSpanInUse: {
        if (s.isUserArenaChunk) {
            @throw(mheapFreeSpanLockedˢ2);
        }
        if (s.allocCount != 0 || s.sweepgen != h.sweepgen) {
            print((@string)"mheap.freeSpanLocked - span "u8, Ꮡs.OrTypedNil(), (@string)" ptr "u8, (Δhex)(uint64)s.@base(), (@string)" allocCount "u8, s.allocCount, (@string)" sweepgen "u8, s.sweepgen, (@string)"/"u8, h.sweepgen, (@string)"\n"u8);
            @throw(mheapFreeSpanLockedˢ3);
        }
        Ꮡh.of(mheap.ᏑpagesInUse).Add(((uintptr)0 - s.npages));

        // Clear in-use bit in arena page bitmap.
        var (arena, pageIdx, pageMask) = pageIndexOf(s.@base());
        atomic.And8(arena.at(heapArena.ᏑpageInUse, (ulong)(pageIdx)), (uint8)((uint8)(~pageMask)));

        // Clear small heap span bit if necessary.
        if (gcUsesSpanInlineMarkBits(s.elemsize)) {
            atomic.And8(arena.at(heapArena.ᏑpageUseSpanInlineMarkBits, (ulong)(pageIdx)), (uint8)((uint8)(~pageMask)));
        }
        break;
    }
    default: {
        @throw(mheapFreeSpanLockedˢ4);
        break;
    }}

    // Update stats.
    //
    // Mirrors the code in allocSpan.
    var nbytes = s.npages * (uintptr)pageSize;
    ᏑgcController.of(gcControllerState.ᏑheapFree).add((int64)nbytes);
    if (typ == spanAllocHeap) {
        ᏑgcController.of(gcControllerState.ᏑheapInUse).add(-(int64)nbytes);
    }
    // Update consistent stats.
    var stats = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
    switch (typ) {
    case var ᴛ when ᴛ == spanAllocHeap: {
        atomic.Xaddint64(stats.of(heapStatsDelta.ᏑinHeap), -(int64)nbytes);
        break;
    }
    case var ᴛ when ᴛ == spanAllocStack: {
        atomic.Xaddint64(stats.of(heapStatsDelta.ᏑinStacks), -(int64)nbytes);
        break;
    }
    case var ᴛ when ᴛ == spanAllocWorkBuf: {
        atomic.Xaddint64(stats.of(heapStatsDelta.ᏑinWorkBufs), -(int64)nbytes);
        break;
    }}
    Ꮡmemstats.of(mstats.ᏑheapStats).release();

    // Mark the space as free.
    Ꮡh.of(mheap.Ꮡpages).free(s.@base(), s.npages);

    // Free the span structure. We no longer have a use for it.
    Ꮡs.of(mspan.Ꮡstate).set(mSpanDead);
    Ꮡh.freeMSpanLocked(Ꮡs);
}

// scavengeAll acquires the heap lock (blocking any additional
// manipulation of the page allocator) and iterates over the whole
// heap, scavenging every free page available.
//
// Must run on the system stack because it acquires the heap lock.
//
//go:systemstack
internal static void scavengeAll(this ж<mheap> Ꮡh) {
    // Disallow malloc or panic while holding the heap lock. We do
    // this here because this is a non-mallocgc entry-point to
    // the mheap API.
    var gp = getg();
    gp.Value.m.Value.mallocing++;

    // Force scavenge everything.
    var released = Ꮡh.of(mheap.Ꮡpages).scavenge(~(uintptr)0, default, true);

    gp.Value.m.Value.mallocing--;

    if (debug.scavtrace > 0) {
        printScavTrace(0, released, true);
    }
}

//go:linkname runtime_debug_freeOSMemory runtime/debug.freeOSMemory
internal static void runtime_debug_freeOSMemory() {
    GC();
    systemstack(() => {
        Ꮡmheap_.scavengeAll();
    });
}

// Initialize a new span with the given start and npages.
internal static void init(this ж<mspan> Ꮡspan, uintptr @base, uintptr npages) {
    ref var span = ref Ꮡspan.DerefOrNull();

    // span is *not* zeroed.
    span.next = default;
    span.prev = default;
    span.list = default;
    span.startAddr = @base;
    span.npages = npages;
    span.limit = @base + npages * (uintptr)gc.PageSize; // see go.dev/issue/74288; adjusted later for heap spans
    span.allocCount = 0;
    span.spanclass = 0;
    span.elemsize = 0;
    span.speciallock.key = 0;
    span.specials = default;
    span.needzero = 0;
    span.freeindex = 0;
    span.freeIndexForScan = 0;
    span.allocBits = default;
    span.gcmarkBits = default;
    span.pinnerBits = default;
    Ꮡspan.of(mspan.Ꮡstate).set(mSpanDead);
    lockInit(ref nonnil(ref span).speciallock, lockRankMspanSpecial);
}

internal static bool inList(this ref mspan span) {
    return span.list != nil;
}

// mSpanList heads a linked list of spans.
partial struct mSpanList {
    internal sys.NotInHeap _;
    internal ж<mspan> first; // first span in list, or nil if none
    internal ж<mspan> last;  // last span in list, or nil if none
}

// Initialize an empty doubly-linked list.
internal static void init(this ref mSpanList list) {
    list.first = default;
    list.last = default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mSpanListRemoveˢ = "mSpanList.remove"u8;

internal static void remove(this ж<mSpanList> Ꮡlist, ж<mspan> Ꮡspan) {
    ref var list = ref Ꮡlist.DerefOrNull();
    ref var span = ref Ꮡspan.DerefOrNull();

    if (span.list != Ꮡlist) {
        print((@string)"runtime: failed mSpanList.remove span.npages="u8, span.npages,
            (@string)" span="u8, Ꮡspan.OrTypedNil(), (@string)" prev="u8, span.prev.OrTypedNil(), (@string)" span.list="u8, span.list.OrTypedNil(), (@string)" list="u8, Ꮡlist.OrTypedNil(), (@string)"\n"u8);
        @throw(mSpanListRemoveˢ);
    }
    if (list.first == Ꮡspan) {
        list.first = span.next;
    } else {
        span.prev.next = span.next;
    }
    if (list.last == Ꮡspan) {
        list.last = span.prev;
    } else {
        span.next.prev = span.prev;
    }
    span.next = default;
    span.prev = default;
    span.list = default;
}

internal static bool isEmpty(this ref mSpanList list) {
    return list.first == nil;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mSpanListInsertˢ = "mSpanList.insert"u8;

internal static void insert(this ж<mSpanList> Ꮡlist, ж<mspan> Ꮡspan) {
    ref var list = ref Ꮡlist.DerefOrNull();
    ref var span = ref Ꮡspan.DerefOrNull();

    if (span.next != nil || span.prev != nil || span.list != nil) {
        println((@string)"runtime: failed mSpanList.insert"u8, Ꮡspan.OrTypedNil(), span.next.OrTypedNil(), span.prev.OrTypedNil(), span.list.OrTypedNil());
        @throw(mSpanListInsertˢ);
    }
    span.next = list.first;
    if (list.first != nil) {
        // The list contains at least one span; link it in.
        // The last span in the list doesn't change.
        list.first.prev = Ꮡspan;
    } else {
        // The list contains no spans, so this is also the last span.
        list.last = Ꮡspan;
    }
    list.first = Ꮡspan;
    span.list = Ꮡlist;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mSpanListInsertBackˢ = "mSpanList.insertBack"u8;

internal static void insertBack(this ж<mSpanList> Ꮡlist, ж<mspan> Ꮡspan) {
    ref var list = ref Ꮡlist.DerefOrNull();
    ref var span = ref Ꮡspan.DerefOrNull();

    if (span.next != nil || span.prev != nil || span.list != nil) {
        println((@string)"runtime: failed mSpanList.insertBack"u8, Ꮡspan.OrTypedNil(), span.next.OrTypedNil(), span.prev.OrTypedNil(), span.list.OrTypedNil());
        @throw(mSpanListInsertBackˢ);
    }
    span.prev = list.last;
    if (list.last != nil) {
        // The list contains at least one span.
        list.last.next = Ꮡspan;
    } else {
        // The list contains no spans, so this is also the first span.
        list.first = Ꮡspan;
    }
    list.last = Ꮡspan;
    span.list = Ꮡlist;
}

// takeAll removes all spans from other and inserts them at the front
// of list.
internal static void takeAll(this ж<mSpanList> Ꮡlist, ж<mSpanList> Ꮡother) {
    ref var list = ref Ꮡlist.DerefOrNull();
    ref var other = ref Ꮡother.DerefOrNull();

    if (other.isEmpty()) {
        return;
    }

    // Reparent everything in other to list.
    for (var s = other.first; s != nil; s = s.next) {
        s.list = Ꮡlist;
    }

    // Concatenate the lists.
    if (list.isEmpty()) {
        list = other;
    } else {
        // Neither list is empty. Put other before list.
        other.last.next = list.first;
        list.first.prev = other.last;
        list.first = other.first;
    }

    (other.first, other.last) = (default, default);
}

// _KindSpecialTinyBlock indicates that a given allocation is a tiny block.
// Ordered before KindSpecialFinalizer and KindSpecialCleanup so that it
// always appears first in the specials list.
// Used only if debug.checkfinalizers != 0.
internal static UntypedInt _KindSpecialTinyBlock => 1;
// _KindSpecialFinalizer is for tracking finalizers.
internal static UntypedInt _KindSpecialFinalizer => 2;
// _KindSpecialWeakHandle is used for creating weak pointers.
internal static UntypedInt _KindSpecialWeakHandle => 3;
// _KindSpecialProfile is for memory profiling.
internal static UntypedInt _KindSpecialProfile => 4;
// _KindSpecialReachable is a special used for tracking
// reachability during testing.
internal static UntypedInt _KindSpecialReachable => 5;
// _KindSpecialPinCounter is a special used for objects that are pinned
// multiple times
internal static UntypedInt _KindSpecialPinCounter => 6;
// _KindSpecialCleanup is for tracking cleanups.
internal static UntypedInt _KindSpecialCleanup => 7;
// _KindSpecialCheckFinalizer adds additional context to a finalizer or cleanup.
// Used only if debug.checkfinalizers != 0.
internal static UntypedInt _KindSpecialCheckFinalizer => 8;
// _KindSpecialBubble is used to associate objects with synctest bubbles.
internal static UntypedInt _KindSpecialBubble => 9;
// _KindSpecialSecret is a special used to mark an object
// as needing zeroing immediately upon freeing.
internal static UntypedInt _KindSpecialSecret => 10;

partial struct special {
    internal sys.NotInHeap _;
    internal ж<special> next; // linked list in span
    internal uintptr offset;  // span offset of object
    internal byte kind;       // kind of special
}

// spanHasSpecials marks a span as having specials in the arena bitmap.
internal static void spanHasSpecials(ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    var arenaPage = (s.@base() / (uintptr)pageSize) % (uintptr)pagesPerArena;
    arenaIdx ai = arenaIndex(s.@base());
    var ha = mheap_.arenas[ai.l1()].Value[ai.l2()];
    atomic.Or8(ha.at(heapArena.ᏑpageSpecials, (ulong)(arenaPage / 8)), (uint8)((uint8)1 << (int)(arenaPage % 8)));
}

// spanHasNoSpecials marks a span as having no specials in the arena bitmap.
internal static void spanHasNoSpecials(ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    var arenaPage = (s.@base() / (uintptr)pageSize) % (uintptr)pagesPerArena;
    arenaIdx ai = arenaIndex(s.@base());
    var ha = mheap_.arenas[ai.l1()].Value[ai.l2()];
    atomic.And8(ha.at(heapArena.ᏑpageSpecials, (ulong)(arenaPage / 8)), (uint8)((uint8)(~((uint8)((uint8)1 << (int)(arenaPage % 8))))));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addspecialOnInvalidˢ = "addspecial on invalid pointer"u8;

// addspecial adds the special record s to the list of special records for
// the object p. All fields of s should be filled in except for
// offset & next, which this routine will fill in.
// Returns true if the special was successfully added, false otherwise.
// (The add will fail only if a record with the same p and s->kind
// already exists unless force is set to true.)
internal static bool addspecial(@unsafe.Pointer Δp, ж<special> Ꮡs, bool force) {
    ref var s = ref Ꮡs.DerefOrNull();

    var span = spanOfHeap((uintptr)Δp);
    if (span == nil) {
        @throw(addspecialOnInvalidˢ);
    }

    // Ensure that the span is swept.
    // Sweeping accesses the specials list w/o locks, so we have
    // to synchronize with it. And it's just much safer.
    var mp = acquirem();
    span.ensureSwept();

    var offset = (uintptr)Δp - span.@base();
    var kind = s.kind;

    @lock(span.of(mspan.Ꮡspeciallock));

    // Find splice point, check for existing record.
    var (iter, exists) = span.specialFindSplicePoint(offset, kind);
    if (!exists || force) {
        // Splice in record, fill in offset.
        s.offset = offset;
        s.next = iter.ValueSlot;
        iter.ValueSlot = Ꮡs;
        spanHasSpecials(span);
    }

    unlock(span.of(mspan.Ꮡspeciallock));
    releasem(ref (mp).DerefOrNull());
    // We're converting p to a uintptr and looking it up, and we
    // don't want it to die and get swept while we're doing so.
    KeepAlive(@unsafe.Pointer.OrTypedNil(Δp));
    return !exists || force; // already exists or addition was forced
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string removespecialOnInvalidˢ = "removespecial on invalid pointer"u8;

// Removes the Special record of the given kind for the object p.
// Returns the record if the record existed, nil otherwise.
// The caller must FixAlloc_Free the result.
internal static ж<special> removespecial(@unsafe.Pointer Δp, uint8 kind) {
    var span = spanOfHeap((uintptr)Δp);
    if (span == nil) {
        @throw(removespecialOnInvalidˢ);
    }

    // Ensure that the span is swept.
    // Sweeping accesses the specials list w/o locks, so we have
    // to synchronize with it. And it's just much safer.
    var mp = acquirem();
    span.ensureSwept();

    var offset = (uintptr)Δp - span.@base();

    ж<special> result = default;
    @lock(span.of(mspan.Ꮡspeciallock));

    var (iter, exists) = span.specialFindSplicePoint(offset, kind);
    if (exists) {
        var s = iter.ValueSlot;
        iter.ValueSlot = s.next;
        result = s;
    }
    if (span.specials == nil) {
        spanHasNoSpecials(span);
    }
    unlock(span.of(mspan.Ꮡspeciallock));
    releasem(ref (mp).DerefOrNull());
    return result;
}

// Find a splice point in the sorted list and check for an already existing
// record. Returns a pointer to the next-reference in the list predecessor.
// Returns true, if the referenced item is an exact match.
internal static (ж<ж<special>>, bool) specialFindSplicePoint(this ж<mspan> Ꮡspan, uintptr offset, byte kind) {
    // Find splice point, check for existing record.
    var iter = Ꮡspan.of(mspan.Ꮡspecials);
    var found = false;
    for (;;) {
        var s = iter.ValueSlot;
        if (s == nil) {
            break;
        }
        if (offset == s.offset && kind == s.kind) {
            found = true;
            break;
        }
        if (offset < s.offset || (offset == s.offset && kind < s.kind)) {
            break;
        }
        iter = s.of(special.Ꮡnext);
    }
    return (iter, found);
}

// The described object has a finalizer set for it.
//
// specialfinalizer is allocated from non-GC'd memory, so any heap
// pointers must be specially handled.
partial struct specialfinalizer {
    internal sys.NotInHeap _;
    internal special special;
    internal ж<funcval> fn; // May be a heap pointer.
    internal uintptr nret;
    internal ж<_type> fint; // May be a heap pointer, but always live.
    internal ж<ptrtype> ot; // May be a heap pointer, but always live.
}

// Adds a finalizer to the object p. Returns true if it succeeded.
internal static bool addfinalizer(@unsafe.Pointer Δp, ж<funcval> f, uintptr nret, ж<_type> Ꮡfint, ж<ptrtype> Ꮡot) {
    ref var fint = ref Ꮡfint.DerefOrNull();
    ref var ot = ref Ꮡot.DerefOrNull();

    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    var s = (ж<specialfinalizer>)(uintptr)(Ꮡmheap_.of(mheap.Ꮡspecialfinalizeralloc).alloc());
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    s.special.kind = _KindSpecialFinalizer;
    s.fn = f;
    s.nret = nret;
    s.fint = Ꮡfint;
    s.ot = Ꮡot;
    if (addspecial(Δp, s.of(specialfinalizer.Ꮡspecial), false)) {
        // This is responsible for maintaining the same
        // GC-related invariants as markrootSpans in any
        // situation where it's possible that markrootSpans
        // has already run but mark termination hasn't yet.
        if (gcphase != _GCoff) {
            var (@base, span, _) = findObject((uintptr)Δp, 0, 0);
            var mp = acquirem();
            var gcw = (~mp).p.ptr().of(runtime_package.Δp.Ꮡgcw);
            // Mark everything reachable from the object
            // so it's retained for the finalizer.
            if (!span.spanclass.noscan()) {
                scanObject(@base, gcw);
            }
            // Mark the finalizer itself, since the
            // special isn't part of the GC'd heap.
            scanblock((uintptr)@unsafe.Pointer.FromBox(s.of(specialfinalizer.Ꮡfn)), goarch.PtrSize, Ꮡoneptrmask.at<uint8>(0), gcw, nil);
            releasem(ref (mp).DerefOrNull());
        }
        return true;
    }

    // There was an old finalizer
    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    Ꮡmheap_.of(mheap.Ꮡspecialfinalizeralloc).free(@unsafe.Pointer.FromPinnedBox(s));
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    return false;
}

// Removes the finalizer (if any) from the object p.
internal static void removefinalizer(@unsafe.Pointer Δp) {
    var s = removespecial(Δp, _KindSpecialFinalizer).Reinterpret<special, specialfinalizer>();
    if (s == nil) {
        return; // there wasn't a finalizer to remove
    }
    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    Ꮡmheap_.of(mheap.Ꮡspecialfinalizeralloc).free(@unsafe.Pointer.FromPinnedBox(s));
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
}

// The described object has a cleanup set for it.
partial struct specialCleanup {
    internal sys.NotInHeap _;
    internal special special;
    internal cleanupFn cleanup;
    // Globally unique ID for the cleanup, obtained from mheap_.cleanupID.
    internal uint64 id;
}

// addCleanup attaches a cleanup function to the object. Multiple
// cleanups are allowed on an object, and even the same pointer.
// A cleanup id is returned which can be used to uniquely identify
// the cleanup.
internal static uint64 addCleanup(@unsafe.Pointer Δp, cleanupFn c) {
    // TODO(mknyszek): Consider pooling specialCleanups on the P
    // so we don't have to take the lock every time. Just locking
    // is a considerable part of the cost of AddCleanup. This
    // would also require reserving some cleanup IDs on the P.
    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    var s = (ж<specialCleanup>)(uintptr)(Ꮡmheap_.of(mheap.ᏑspecialCleanupAlloc).alloc());
    mheap_.cleanupID++; // Increment first. ID 0 is reserved.
    var id = mheap_.cleanupID;
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    s.special.kind = _KindSpecialCleanup;
    s.cleanup = c;
    s.id = id;

    var mp = acquirem();
    addspecial(Δp, s.of(specialCleanup.Ꮡspecial), true);
    // This is responsible for maintaining the same
    // GC-related invariants as markrootSpans in any
    // situation where it's possible that markrootSpans
    // has already run but mark termination hasn't yet.
    if (gcphase != _GCoff) {
        // Mark the cleanup itself, since the
        // special isn't part of the GC'd heap.
        gcScanCleanup(s, (~mp).p.ptr().of(runtime_package.Δp.Ꮡgcw));
    }
    releasem(ref (mp).DerefOrNull());
    // Keep c and its referents alive. There's a window in this function
    // where it's only reachable via the special while the special hasn't
    // been added to the specials list yet. This is similar to a bug
    // discovered for weak handles, see #70455.
    KeepAlive(c);
    return id;
}

// Always paired with a specialCleanup or specialfinalizer, adds context.
partial struct specialCheckFinalizer {
    internal sys.NotInHeap _;
    internal special special;
    internal uint64 cleanupID; // Needed to disambiguate cleanups.
    internal uintptr createPC;
    internal uintptr funcPC;
    internal ж<_type> ptrType;
}

// setFinalizerContext adds a specialCheckFinalizer to ptr. ptr must already have a
// finalizer special attached.
internal static void setFinalizerContext(@unsafe.Pointer ptr, ж<_type> ptrType, uintptr createPC, uintptr funcPC) {
    setCleanupContext(ptr, ptrType, createPC, funcPC, 0);
}

// setCleanupContext adds a specialCheckFinalizer to ptr. ptr must already have a
// finalizer or cleanup special attached. Pass 0 for the cleanupID to indicate
// a finalizer.
internal static void setCleanupContext(@unsafe.Pointer ptr, ж<_type> ᏑptrType, uintptr createPC, uintptr funcPC, uint64 cleanupID) {
    ref var ptrType = ref ᏑptrType.DerefOrNull();

    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    var s = (ж<specialCheckFinalizer>)(uintptr)(Ꮡmheap_.of(mheap.ᏑspecialCheckFinalizerAlloc).alloc());
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    s.special.kind = _KindSpecialCheckFinalizer;
    s.cleanupID = cleanupID;
    s.createPC = createPC;
    s.funcPC = funcPC;
    s.ptrType = ᏑptrType;

    var mp = acquirem();
    addspecial(ptr, s.of(specialCheckFinalizer.Ꮡspecial), true);
    releasem(ref (mp).DerefOrNull());
    KeepAlive(@unsafe.Pointer.OrTypedNil(ptr));
}

internal static ж<specialCheckFinalizer> getCleanupContext(uintptr ptr, uint64 cleanupID) {
    assertWorldStopped();

    var span = spanOfHeap(ptr);
    if (span == nil) {
        return default;
    }
    ж<specialCheckFinalizer> found = default;
    var offset = ptr - span.@base();
    var (iter, exists) = span.specialFindSplicePoint(offset, _KindSpecialCheckFinalizer);
    if (exists) {
        for (;;) {
            var s = iter.ValueSlot;
            if (s == nil) {
                // Reached the end of the linked list. Stop searching at this point.
                break;
            }
            if (offset == s.offset && _KindSpecialCheckFinalizer == s.kind && (s.Reinterpret<special, specialCheckFinalizer>()).Value.cleanupID == cleanupID) {
                // The special is a cleanup and contains a matching cleanup id.
                iter.ValueSlot = s.next;
                found = s.Reinterpret<special, specialCheckFinalizer>();
                break;
            }
            if (offset < s.offset || (offset == s.offset && _KindSpecialCheckFinalizer < s.kind)) {
                // The special is outside the region specified for that kind of
                // special. The specials are sorted by kind.
                break;
            }
            // Try the next special.
            iter = s.of(special.Ꮡnext);
        }
    }
    return found;
}

// clearFinalizerContext removes the specialCheckFinalizer for the given pointer, if any.
internal static void clearFinalizerContext(uintptr ptr) {
    clearCleanupContext(ptr, 0);
}

// clearFinalizerContext removes the specialCheckFinalizer for the given pointer and cleanup ID, if any.
internal static void clearCleanupContext(uintptr ptr, uint64 cleanupID) {
    // The following block removes the Special record of type cleanup for the object c.ptr.
    var span = spanOfHeap(ptr);
    if (span == nil) {
        return;
    }
    // Ensure that the span is swept.
    // Sweeping accesses the specials list w/o locks, so we have
    // to synchronize with it. And it's just much safer.
    var mp = acquirem();
    span.ensureSwept();

    var offset = ptr - span.@base();

    ж<special> found = default;
    @lock(span.of(mspan.Ꮡspeciallock));

    var (iter, exists) = span.specialFindSplicePoint(offset, _KindSpecialCheckFinalizer);
    if (exists) {
        for (;;) {
            var s = iter.ValueSlot;
            if (s == nil) {
                // Reached the end of the linked list. Stop searching at this point.
                break;
            }
            if (offset == s.offset && _KindSpecialCheckFinalizer == s.kind && (s.Reinterpret<special, specialCheckFinalizer>()).Value.cleanupID == cleanupID) {
                // The special is a cleanup and contains a matching cleanup id.
                iter.ValueSlot = s.next;
                found = s;
                break;
            }
            if (offset < s.offset || (offset == s.offset && _KindSpecialCheckFinalizer < s.kind)) {
                // The special is outside the region specified for that kind of
                // special. The specials are sorted by kind.
                break;
            }
            // Try the next special.
            iter = s.of(special.Ꮡnext);
        }
    }
    if (span.specials == nil) {
        spanHasNoSpecials(span);
    }
    unlock(span.of(mspan.Ꮡspeciallock));
    releasem(ref (mp).DerefOrNull());

    if (found == nil) {
        return;
    }
    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    Ꮡmheap_.of(mheap.ᏑspecialCheckFinalizerAlloc).free(@unsafe.Pointer.FromPinnedBox(found));
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
}

// Indicates that an allocation is a tiny block.
// Used only if debug.checkfinalizers != 0.
partial struct specialTinyBlock {
    internal sys.NotInHeap _;
    internal special special;
}

// setTinyBlockContext marks an allocation as a tiny block to diagnostics like
// checkfinalizer.
//
// A tiny block is only marked if it actually contains more than one distinct
// value, since we're using this for debugging.
internal static void setTinyBlockContext(@unsafe.Pointer ptr) {
    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    var s = (ж<specialTinyBlock>)(uintptr)(Ꮡmheap_.of(mheap.ᏑspecialTinyBlockAlloc).alloc());
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    s.special.kind = _KindSpecialTinyBlock;

    var mp = acquirem();
    addspecial(ptr, s.of(specialTinyBlock.Ꮡspecial), false);
    releasem(ref (mp).DerefOrNull());
    KeepAlive(@unsafe.Pointer.OrTypedNil(ptr));
}

// inTinyBlock returns whether ptr is in a tiny alloc block, at one point grouped
// with other distinct values.
internal static bool inTinyBlock(uintptr ptr) {
    assertWorldStopped();

    ptr = alignDown(ptr, maxTinySize);
    var span = spanOfHeap(ptr);
    if (span == nil) {
        return false;
    }
    var offset = ptr - span.@base();
    var (_, exists) = span.specialFindSplicePoint(offset, _KindSpecialTinyBlock);
    return exists;
}

// The described object has a weak pointer.
//
// Weak pointers in the GC have the following invariants:
//
//   - Strong-to-weak conversions must ensure the strong pointer
//     remains live until the weak handle is installed. This ensures
//     that creating a weak pointer cannot fail.
//
//   - Weak-to-strong conversions require the weakly-referenced
//     object to be swept before the conversion may proceed. This
//     ensures that weak-to-strong conversions cannot resurrect
//     dead objects by sweeping them before that happens.
//
//   - Weak handles are unique and canonical for each byte offset into
//     an object that a strong pointer may point to, until an object
//     becomes unreachable.
//
//   - Weak handles contain nil as soon as an object becomes unreachable
//     the first time, before a finalizer makes it reachable again. New
//     weak handles created after resurrection are newly unique.
//
// specialWeakHandle is allocated from non-GC'd memory, so any heap
// pointers must be specially handled.
partial struct specialWeakHandle {
    internal sys.NotInHeap _;
    internal special special;
    // handle is a reference to the actual weak pointer.
    // It is always heap-allocated and must be explicitly kept
    // live so long as this special exists.
    internal ж<atomic.Uintptr> handle;
}

//go:linkname internal_weak_runtime_registerWeakPointer weak.runtime_registerWeakPointer
internal static @unsafe.Pointer internal_weak_runtime_registerWeakPointer(@unsafe.Pointer Δp) {
    return @unsafe.Pointer.FromPinnedBox(getOrAddWeakHandle(Δp));
}

//go:linkname internal_weak_runtime_makeStrongFromWeak weak.runtime_makeStrongFromWeak
internal static @unsafe.Pointer internal_weak_runtime_makeStrongFromWeak(@unsafe.Pointer u) {
    var handle = (ж<atomic.Uintptr>)(uintptr)(u);

    // Prevent preemption. We want to make sure that another GC cycle can't start
    // and that work.strongFromWeak.block can't change out from under us.
    var mp = acquirem();

    // Yield to the GC if necessary.
    if (work.strongFromWeak.block) {
        releasem(ref (mp).DerefOrNull());

        // Try to park and wait for mark termination.
        // N.B. gcParkStrongFromWeak calls acquirem before returning.
        mp = gcParkStrongFromWeak();
    }

    var Δp = handle.Load();
    if (Δp == 0) {
        releasem(ref (mp).DerefOrNull());
        return default;
    }
    // Be careful. p may or may not refer to valid memory anymore, as it could've been
    // swept and released already. It's always safe to ensure a span is swept, though,
    // even if it's just some random span.
    var span = spanOfHeap(Δp);
    if (span == nil) {
        // If it's immortal, then just return the pointer.
        //
        // Stay non-preemptible so the GC can't see us convert this potentially
        // completely bogus value to an unsafe.Pointer.
        if (isGoPointerWithoutSpan((@unsafe.Pointer)Δp)) {
            releasem(ref (mp).DerefOrNull());
            return (@unsafe.Pointer)Δp;
        }
        // It's heap-allocated, so the span probably just got swept and released.
        releasem(ref (mp).DerefOrNull());
        return default;
    }
    // Ensure the span is swept.
    span.ensureSwept();

    // Now we can trust whatever we get from handle, so make a strong pointer.
    //
    // Even if we just swept some random span that doesn't contain this object, because
    // this object is long dead and its memory has since been reused, we'll just observe nil.
    @unsafe.Pointer ptr = (@unsafe.Pointer)handle.Load();

    // This is responsible for maintaining the same GC-related
    // invariants as the Yuasa part of the write barrier. During
    // the mark phase, it's possible that we just created the only
    // valid pointer to the object pointed to by ptr. If it's only
    // ever referenced from our stack, and our stack is blackened
    // already, we could fail to mark it. So, mark it now.
    if (gcphase != _GCoff) {
        shade((uintptr)ptr);
    }
    releasem(ref (mp).DerefOrNull());

    // Explicitly keep ptr alive. This seems unnecessary since we return ptr,
    // but let's be explicit since it's important we keep ptr alive across the
    // call to shade.
    KeepAlive(@unsafe.Pointer.OrTypedNil(ptr));
    return ptr;
}

// gcParkStrongFromWeak puts the current goroutine on the weak->strong queue and parks.
internal static ж<m> gcParkStrongFromWeak() {
    // Prevent preemption as we check strongFromWeak, so it can't change out from under us.
    var mp = acquirem();

    while (work.strongFromWeak.block) {
        @lock(Ꮡwork.of(workType.ᏑstrongFromWeak).of(workType_strongFromWeak.Ꮡlock));
        releasem(ref (mp).DerefOrNull()); // N.B. Holding the lock prevents preemption.

        // Queue ourselves up.
        Ꮡwork.of(workType.ᏑstrongFromWeak).of(workType_strongFromWeak.Ꮡq).pushBack(getg());

        // Park.
        goparkunlock(Ꮡwork.of(workType.ᏑstrongFromWeak).of(workType_strongFromWeak.Ꮡlock), waitReasonGCWeakToStrongWait, traceBlockGCWeakToStrongWait, 2);

        // Re-acquire the current M since we're going to check the condition again.
        mp = acquirem();

        // Re-check condition. We may have awoken in the next GC's mark termination phase.
    }
    return mp;
}

// gcWakeAllStrongFromWeak wakes all currently blocked weak->strong
// conversions. This is used at the end of a GC cycle.
//
// work.strongFromWeak.block must be false to prevent woken goroutines
// from immediately going back to sleep.
internal static void gcWakeAllStrongFromWeak() {
    @lock(Ꮡwork.of(workType.ᏑstrongFromWeak).of(workType_strongFromWeak.Ꮡlock));
    ref var list = ref heap<gList>(Ꮡwork.of(workType.ᏑstrongFromWeak).of(workType_strongFromWeak.Ꮡq).popList(), out var Ꮡlist);
    injectglist(Ꮡlist);
    unlock(Ꮡwork.of(workType.ᏑstrongFromWeak).of(workType_strongFromWeak.Ꮡlock));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string failedToGetOrCreateWeakˢ = "failed to get or create weak handle"u8;

// N.B. Pad the weak handle to ensure it doesn't share a tiny
// block with any other allocations. This can lead to leaks, such
// as in go.dev/issue/76007. As an alternative, we could consider
// using the currently-unused 8-byte noscan size class.
internal partial struct getOrAddWeakHandle_weakHandleBox /*dyn*/ {
    internal atomic.Uintptr h;
    internal array<byte> _ = new(8);
}

// Retrieves or creates a weak pointer handle for the object p.
internal static ж<atomic.Uintptr> getOrAddWeakHandle(@unsafe.Pointer Δp) {
    if (debug.sbrk != 0) {
        // debug.sbrk never frees memory, so it'll never go nil. However, we do still
        // need a weak handle that's specific to p. Use the immortal weak handle map.
        // Keep p alive across the call to getOrAdd defensively, though it doesn't
        // really matter in this particular case.
        var handleΔ1 = Ꮡmheap_.of(mheap.ᏑimmortalWeakHandles).getOrAdd((uintptr)Δp);
        KeepAlive(@unsafe.Pointer.OrTypedNil(Δp));
        return handleΔ1;
    }

    // First try to retrieve without allocating.
    {
        var handleΔ2 = getWeakHandle(Δp); if (handleΔ2 != nil) {
            // Keep p alive for the duration of the function to ensure
            // that it cannot die while we're trying to do this.
            KeepAlive(@unsafe.Pointer.OrTypedNil(Δp));
            return handleΔ2;
        }
    }

    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    var s = (ж<specialWeakHandle>)(uintptr)(Ꮡmheap_.of(mheap.ᏑspecialWeakHandleAlloc).alloc());
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    var handle = Ꮡ((@new<getOrAddWeakHandle_weakHandleBox>().h));
    s.special.kind = _KindSpecialWeakHandle;
    s.handle = handle;
    handle.Store((uintptr)Δp);
    if (addspecial(Δp, s.of(specialWeakHandle.Ꮡspecial), false)) {
        // This is responsible for maintaining the same
        // GC-related invariants as markrootSpans in any
        // situation where it's possible that markrootSpans
        // has already run but mark termination hasn't yet.
        if (gcphase != _GCoff) {
            var mp = acquirem();
            var gcw = (~mp).p.ptr().of(runtime_package.Δp.Ꮡgcw);
            // Mark the weak handle itself, since the
            // special isn't part of the GC'd heap.
            scanblock((uintptr)@unsafe.Pointer.FromBox(s.of(specialWeakHandle.Ꮡhandle)), goarch.PtrSize, Ꮡoneptrmask.at<uint8>(0), gcw, nil);
            releasem(ref (mp).DerefOrNull());
        }

        // Keep p alive for the duration of the function to ensure
        // that it cannot die while we're trying to do this.
        //
        // Same for handle, which is only stored in the special.
        // There's a window where it might die if we don't keep it
        // alive explicitly. Returning it here is probably good enough,
        // but let's be defensive and explicit. See #70455.
        KeepAlive(@unsafe.Pointer.OrTypedNil(Δp));
        KeepAlive(handle.OrTypedNil());
        return handle;
    }

    // There was an existing handle. Free the special
    // and try again. We must succeed because we're explicitly
    // keeping p live until the end of this function. Either
    // we, or someone else, must have succeeded, because we can
    // only fail in the event of a race, and p will still be
    // be valid no matter how much time we spend here.
    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    Ꮡmheap_.of(mheap.ᏑspecialWeakHandleAlloc).free(@unsafe.Pointer.FromPinnedBox(s));
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));

    handle = getWeakHandle(Δp);
    if (handle == nil) {
        @throw(failedToGetOrCreateWeakˢ);
    }

    // Keep p alive for the duration of the function to ensure
    // that it cannot die while we're trying to do this.
    //
    // Same for handle, just to be defensive.
    KeepAlive(@unsafe.Pointer.OrTypedNil(Δp));
    KeepAlive(handle.OrTypedNil());
    return handle;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getWeakHandleOnInvalidˢ = "getWeakHandle on invalid pointer"u8;

internal static ж<atomic.Uintptr> getWeakHandle(@unsafe.Pointer Δp) {
    var span = spanOfHeap((uintptr)Δp);
    if (span == nil) {
        if (isGoPointerWithoutSpan(Δp)) {
            return Ꮡmheap_.of(mheap.ᏑimmortalWeakHandles).getOrAdd((uintptr)Δp);
        }
        @throw(getWeakHandleOnInvalidˢ);
    }

    // Ensure that the span is swept.
    // Sweeping accesses the specials list w/o locks, so we have
    // to synchronize with it. And it's just much safer.
    var mp = acquirem();
    span.ensureSwept();

    var offset = (uintptr)Δp - span.@base();

    @lock(span.of(mspan.Ꮡspeciallock));

    // Find the existing record and return the handle if one exists.
    ж<atomic.Uintptr> handle = default;
    var (iter, exists) = span.specialFindSplicePoint(offset, _KindSpecialWeakHandle);
    if (exists) {
        handle = ((iter.ValueSlot.Reinterpret<special, specialWeakHandle>())).Value.handle;
    }
    unlock(span.of(mspan.Ꮡspeciallock));
    releasem(ref (mp).DerefOrNull());

    // Keep p alive for the duration of the function to ensure
    // that it cannot die while we're trying to do this.
    KeepAlive(@unsafe.Pointer.OrTypedNil(Δp));
    return handle;
}

partial struct immortalWeakHandleMap {
    internal atomic.UnsafePointer root; // *immortalWeakHandle (can't use generics because it's notinheap)
}

// immortalWeakHandle is a lock-free append-only hash-trie.
//
// Key features:
//   - 2-ary trie. Child nodes are indexed by the highest bit (remaining) of the hash of the address.
//   - New nodes are placed at the first empty level encountered.
//   - When the first child is added to a node, the existing value is not moved into a child.
//     This means that we must check the value at each level, not just at the leaf.
//   - No deletion or rebalancing.
//   - Intentionally devolves into a linked list on hash collisions (the hash bits will all
//     get shifted out during iteration, and new nodes will just be appended to the 0th child).
partial struct immortalWeakHandle {
    internal sys.NotInHeap _;
    internal array<atomic.UnsafePointer> children = new(2); // *immortalObjectMapNode (can't use generics because it's notinheap)
    internal uintptr ptr;                                   // &ptr is the weak handle
}

// handle returns a canonical weak handle.
internal static ж<atomic.Uintptr> handle(this ж<immortalWeakHandle> Ꮡh) {
    // N.B. Since we just need an *atomic.Uintptr that never changes, we can trivially
    // reference ptr to save on some memory in immortalWeakHandle and avoid extra atomics
    // in getOrAdd.
    return Ꮡh.of(immortalWeakHandle.Ꮡptr).Reinterpret<uintptr, atomic.Uintptr>();
}

// getOrAdd introduces p, which must be a pointer to immortal memory (for example, a linker-allocated
// object) and returns a weak handle. The weak handle will never become nil.
internal static ж<atomic.Uintptr> getOrAdd(this ж<immortalWeakHandleMap> Ꮡtab, uintptr Δpʗp) {
    ref var Δp = ref heap(Δpʗp, out var Ꮡp);

    ж<immortalWeakHandle> newNode = default;
    var m = Ꮡtab.of(immortalWeakHandleMap.Ꮡroot);
    var hash = memhash((uintptr)abi.NoEscape(@unsafe.Pointer.FromBox(Ꮡp)), 0, goarch.PtrSize);
    System.GC.KeepAlive(Ꮡp);
    var hashIter = hash;
    for (;;) {
        var n = (ж<immortalWeakHandle>)(uintptr)(m.Load());
        if (n == nil) {
            // Try to insert a new map node. We may end up discarding
            // this node if we fail to insert because it turns out the
            // value is already in the map.
            //
            // The discard will only happen if two threads race on inserting
            // the same value. Both might create nodes, but only one will
            // succeed on insertion. If two threads race to insert two
            // different values, then both nodes will *always* get inserted,
            // because the equality checking below will always fail.
            //
            // Performance note: contention on insertion is likely to be
            // higher for small maps, but since this data structure is
            // append-only, either the map stays small because there isn't
            // much activity, or the map gets big and races to insert on
            // the same node are much less likely.
            if (newNode == nil) {
                newNode = (ж<immortalWeakHandle>)(uintptr)(persistentalloc(/* unsafe.Sizeof(immortalWeakHandle{}) */ (uintptr)24, goarch.PtrSize, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
                newNode.ptr = Δp;
            }
            if (m.CompareAndSwapNoWB(nil, @unsafe.Pointer.FromPinnedBox(newNode))) {
                return newNode.handle();
            }
            // Reload n. Because pointers are only stored once,
            // we must have lost the race, and therefore n is not nil
            // anymore.
            n = (ж<immortalWeakHandle>)(uintptr)(m.Load());
        }
        if (n.ptr == Δp) {
            return n.handle();
        }
        m = n.at(immortalWeakHandle.Ꮡchildren, (ulong)((hashIter >> (int)(8 * goarch.PtrSize - 1))));
        hashIter <<= 1;
    }
}

// The described object is being heap profiled.
partial struct specialprofile {
    internal sys.NotInHeap _;
    internal special special;
    internal ж<bucket> b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setprofilebucketProfileˢ = "setprofilebucket: profile already set"u8;

// Set the heap profile bucket associated with addr to b.
internal static void setprofilebucket(@unsafe.Pointer Δp, ж<bucket> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    var s = (ж<specialprofile>)(uintptr)(Ꮡmheap_.of(mheap.Ꮡspecialprofilealloc).alloc());
    unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
    s.special.kind = _KindSpecialProfile;
    s.b = Ꮡb;
    if (!addspecial(Δp, s.of(specialprofile.Ꮡspecial), false)) {
        @throw(setprofilebucketProfileˢ);
    }
}

// specialReachable tracks whether an object is reachable on the next
// GC cycle. This is used by testing.
partial struct specialReachable {
    internal special special;
    internal bool done;
    internal bool reachable;
}

// specialPinCounter tracks whether an object is pinned multiple times.
partial struct specialPinCounter {
    internal special special;
    internal uintptr counter;
}

// specialSecret tracks whether we need to zero an object immediately
// upon freeing.
partial struct specialSecret {
    internal sys.NotInHeap _;
    internal special special;
    internal uintptr size;
}

// specialsIter helps iterate over specials lists.
partial struct specialsIter {
    internal ж<ж<special>> pprev;
    internal ж<special> s;
}

internal static specialsIter newSpecialsIter(ж<mspan> Ꮡspan) {
    ref var span = ref Ꮡspan.DerefOrNull();

    return new specialsIter(Ꮡspan.of(mspan.Ꮡspecials), span.specials);
}

internal static bool valid(this ref specialsIter i) {
    return i.s != nil;
}

internal static void next(this ref specialsIter i) {
    i.pprev = i.s.of(special.Ꮡnext);
    i.s = i.pprev.ValueSlot;
}

// unlinkAndNext removes the current special from the list and moves
// the iterator to the next special. It returns the unlinked special.
internal static ж<special> unlinkAndNext(this ref specialsIter i) {
    var cur = i.s;
    i.s = cur.next;
    i.pprev.ValueSlot = i.s;
    return cur;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badSpecialKindˢ = "bad special kind"u8;

// freeSpecial performs any cleanup on special s and deallocates it.
// s must already be unlinked from the specials list.
// TODO(mknyszek): p and size together DO NOT represent a valid allocation.
// size is the size of the allocation block in the span (mspan.elemsize), and p is
// whatever pointer the special was attached to, which need not point to the
// beginning of the block, though it may.
// Consider passing the arguments differently to avoid giving the impression
// that p and size together represent an address range.
internal static void freeSpecial(ж<special> Ꮡs, @unsafe.Pointer Δp, uintptr size) {
    ref var s = ref Ꮡs.DerefOrNull();

    switch (s.kind) {
    case var ᴛ when ᴛ == _KindSpecialFinalizer: {
        var sf = Ꮡs.Reinterpret<special, specialfinalizer>();
        queuefinalizer(Δp, sf.fn, sf.nret, sf.fint, sf.ot);
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        Ꮡmheap_.of(mheap.Ꮡspecialfinalizeralloc).free(@unsafe.Pointer.FromPinnedBox(sf));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        break;
    }
    case var ᴛ when ᴛ == _KindSpecialWeakHandle: {
        var sw = Ꮡs.Reinterpret<special, specialWeakHandle>();
        sw.handle.Store(0);
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        Ꮡmheap_.of(mheap.ᏑspecialWeakHandleAlloc).free(@unsafe.Pointer.FromPinnedBox(Ꮡs));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        break;
    }
    case var ᴛ when ᴛ == _KindSpecialProfile: {
        var sp = Ꮡs.Reinterpret<special, specialprofile>();
        mProf_Free(sp.b, size);
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        Ꮡmheap_.of(mheap.Ꮡspecialprofilealloc).free(@unsafe.Pointer.FromPinnedBox(sp));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        break;
    }
    case var ᴛ when ᴛ == _KindSpecialReachable: {
        var sp = Ꮡs.Reinterpret<special, specialReachable>();
        sp.done = true;
        // The creator frees these.
        break;
    }
    case var ᴛ when ᴛ == _KindSpecialPinCounter: {
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        Ꮡmheap_.of(mheap.ᏑspecialPinCounterAlloc).free(@unsafe.Pointer.FromPinnedBox(Ꮡs));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        break;
    }
    case var ᴛ when ᴛ == _KindSpecialCleanup: {
        var sc = Ꮡs.Reinterpret<special, specialCleanup>();
        // Cleanups, unlike finalizers, do not resurrect the objects
        // they're attached to, so we only need to pass the cleanup
        // function, not the object.
        ᏑgcCleanups.enqueue(sc.cleanup);
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        Ꮡmheap_.of(mheap.ᏑspecialCleanupAlloc).free(@unsafe.Pointer.FromPinnedBox(sc));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        break;
    }
    case var ᴛ when ᴛ == _KindSpecialCheckFinalizer: {
        var sc = Ꮡs.Reinterpret<special, specialCheckFinalizer>();
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        Ꮡmheap_.of(mheap.ᏑspecialCheckFinalizerAlloc).free(@unsafe.Pointer.FromPinnedBox(sc));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        break;
    }
    case var ᴛ when ᴛ == _KindSpecialTinyBlock: {
        var st = Ꮡs.Reinterpret<special, specialTinyBlock>();
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        Ꮡmheap_.of(mheap.ᏑspecialTinyBlockAlloc).free(@unsafe.Pointer.FromPinnedBox(st));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        break;
    }
    case var ᴛ when ᴛ == _KindSpecialBubble: {
        var st = Ꮡs.Reinterpret<special, specialBubble>();
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        Ꮡmheap_.of(mheap.ᏑspecialBubbleAlloc).free(@unsafe.Pointer.FromPinnedBox(st));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        break;
    }
    case var ᴛ when ᴛ == _KindSpecialSecret: {
        var ss = Ꮡs.Reinterpret<special, specialSecret>();
        // p is the actual byte location that the special was
        // attached to, but the size argument is the span
        // element size. If we were to zero out using the size
        // argument, we'd trounce over adjacent memory in cases
        // where the allocation contains a header. Hence, we use
        // the user-visible size which we stash in the special itself.
        //
        // p always points to the beginning of the user-visible
        // allocation since the only way to attach a secret special
        // is via the allocation path. This isn't universal for
        // tiny allocs, but we avoid them in mallocgc anyway.
        memclrNoHeapPointers(Δp, ss.size);
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        Ꮡmheap_.of(mheap.ᏑspecialSecretAlloc).free(@unsafe.Pointer.FromPinnedBox(Ꮡs));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        break;
    }
    default: {
        @throw(badSpecialKindˢ);
        throw panic("not reached");
        break;
    }}
}

// gcBits is an alloc/mark bitmap. This is always used as gcBits.x.
[StructLayout(LayoutKind.Explicit, Size = 1)] partial struct gcBits {
    [FieldOffset(0)] internal readonly sys.NotInHeap _;
    [FieldOffset(0)] internal uint8 x;
}

// bytep returns a pointer to the n'th byte of b.
internal static ж<uint8> bytep(this ж<gcBits> Ꮡb, uintptr n) {
    return addb(Ꮡb.of(gcBits.Ꮡx), n);
}

// bitp returns a pointer to the byte containing bit n and a mask for
// selecting that bit from *bytep.
internal static (ж<uint8> bytep, uint8 mask) bitp(this ж<gcBits> Ꮡb, uintptr n) {
    return (Ꮡb.bytep(n / 8), (uint8)(1 << (int)(n % 8)));
}

internal static uintptr gcBitsChunkBytes => /* uintptr(64 << 10) */ 65536;

internal static uintptr gcBitsHeaderBytes => /* unsafe.Sizeof(gcBitsHeader{}) */ 16;

partial struct gcBitsHeader {
    internal uintptr free; // free is the index into bits of the next free byte.
    internal uintptr next; // *gcBits triggers recursive type bug. (issue 14620)
}

partial struct gcBitsArena {
    internal sys.NotInHeap _;
    // gcBitsHeader // side step recursive type bug (issue 14620) by including fields by hand.
    internal uintptr free; // free is the index into bits of the next free byte; read/write atomically
    internal ж<gcBitsArena> next;
    internal array<gcBits> bits = new(gcBitsChunkBytes - gcBitsHeaderBytes);
}

partial struct gcBitsArenasᴛ1 /*dyn*/ {
    internal mutex @lock;
    internal ж<gcBitsArena> free;
    internal ж<gcBitsArena> next; // Read atomically. Write atomically under lock.
    internal ж<gcBitsArena> current;
    internal ж<gcBitsArena> previous;
}
internal static readonly ж<gcBitsArenasᴛ1> ᏑgcBitsArenas = new StaticBox<gcBitsArenasᴛ1>(new gcBitsArenasᴛ1());
internal static ref gcBitsArenasᴛ1 gcBitsArenas => ref ((StaticBox<gcBitsArenasᴛ1>)ᏑgcBitsArenas).ValueSlot;

// tryAlloc allocates from b or returns nil if b does not have enough room.
// This is safe to call concurrently.
internal static ж<gcBits> tryAlloc(this ж<gcBitsArena> Ꮡb, uintptr bytes) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡb == nil || atomic.Loaduintptr(Ꮡb.of(gcBitsArena.Ꮡfree)) + bytes > (uintptr)len(b.bits)) {
        return default;
    }
    // Try to allocate from this block.
    var end = atomic.Xadduintptr(Ꮡb.of(gcBitsArena.Ꮡfree), bytes);
    if (end > (uintptr)len(b.bits)) {
        return default;
    }
    // There was enough room.
    var start = end - bytes;
    return Ꮡ(b.bits, start);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string markBitsOverflowˢ = "markBits overflow"u8;

// newMarkBits returns a pointer to 8 byte aligned bytes
// to be used for a span's mark bits.
internal static ж<gcBits> newMarkBits(uintptr nelems) {
    var blocksNeeded = (nelems + 63) / 64;
    var bytesNeeded = blocksNeeded * 8;

    // Try directly allocating from the current head arena.
    var head = (ж<gcBitsArena>)(uintptr)(atomic.Loadp(@unsafe.Pointer.FromBox(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡnext))));
    {
        var pΔ1 = head.tryAlloc(bytesNeeded); if (pΔ1 != nil) {
            return pΔ1;
        }
    }

    // There's not enough room in the head arena. We may need to
    // allocate a new arena.
    @lock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
    // Try the head arena again, since it may have changed. Now
    // that we hold the lock, the list head can't change, but its
    // free position still can.
    {
        var pΔ2 = gcBitsArenas.next.tryAlloc(bytesNeeded); if (pΔ2 != nil) {
            unlock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
            return pΔ2;
        }
    }

    // Allocate a new arena. This may temporarily drop the lock.
    var fresh = newArenaMayUnlock();
    // If newArenaMayUnlock dropped the lock, another thread may
    // have put a fresh arena on the "next" list. Try allocating
    // from next again.
    {
        var pΔ3 = gcBitsArenas.next.tryAlloc(bytesNeeded); if (pΔ3 != nil) {
            // Put fresh back on the free list.
            // TODO: Mark it "already zeroed"
            fresh.next = gcBitsArenas.free;
            gcBitsArenas.free = fresh;
            unlock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
            return pΔ3;
        }
    }

    // Allocate from the fresh arena. We haven't linked it in yet, so
    // this cannot race and is guaranteed to succeed.
    var Δp = fresh.tryAlloc(bytesNeeded);
    if (Δp == nil) {
        @throw(markBitsOverflowˢ);
    }

    // Add the fresh arena to the "next" list.
    fresh.next = gcBitsArenas.next;
    atomic.StorepNoWB(@unsafe.Pointer.FromBox(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡnext)), @unsafe.Pointer.FromPinnedBox(fresh));

    unlock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
    return Δp;
}

// newAllocBits returns a pointer to 8 byte aligned bytes
// to be used for this span's alloc bits.
// newAllocBits is used to provide newly initialized spans
// allocation bits. For spans not being initialized the
// mark bits are repurposed as allocation bits when
// the span is swept.
internal static ж<gcBits> newAllocBits(uintptr nelems) {
    return newMarkBits(nelems);
}

// nextMarkBitArenaEpoch establishes a new epoch for the arenas
// holding the mark bits. The arenas are named relative to the
// current GC cycle which is demarcated by the call to finishweep_m.
//
// All current spans have been swept.
// During that sweep each span allocated room for its gcmarkBits in
// gcBitsArenas.next block. gcBitsArenas.next becomes the gcBitsArenas.current
// where the GC will mark objects and after each span is swept these bits
// will be used to allocate objects.
// gcBitsArenas.current becomes gcBitsArenas.previous where the span's
// gcAllocBits live until all the spans have been swept during this GC cycle.
// The span's sweep extinguishes all the references to gcBitsArenas.previous
// by pointing gcAllocBits into the gcBitsArenas.current.
// The gcBitsArenas.previous is released to the gcBitsArenas.free list.
internal static void nextMarkBitArenaEpoch() {
    @lock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
    if (gcBitsArenas.previous != nil) {
        if (gcBitsArenas.free == nil) {
            gcBitsArenas.free = gcBitsArenas.previous;
        } else {
            // Find end of previous arenas.
            var last = gcBitsArenas.previous;
            for (last = gcBitsArenas.previous; last.next != nil; last = last.next) {
            }
            last.next = gcBitsArenas.free;
            gcBitsArenas.free = gcBitsArenas.previous;
        }
    }
    gcBitsArenas.previous = gcBitsArenas.current;
    gcBitsArenas.current = gcBitsArenas.next;
    atomic.StorepNoWB(@unsafe.Pointer.FromBox(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡnext)), nil); // newMarkBits calls newArena when needed
    unlock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcBitsˢ = "gc bits"u8;

// newArenaMayUnlock allocates and zeroes a gcBits arena.
// The caller must hold gcBitsArena.lock. This may temporarily release it.
internal static ж<gcBitsArena> newArenaMayUnlock() {
    ж<gcBitsArena> result = default;
    if (gcBitsArenas.free == nil) {
        unlock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
        result = (ж<gcBitsArena>)(uintptr)(sysAlloc(gcBitsChunkBytes, Ꮡmemstats.of(mstats.ᏑgcMiscSys), gcBitsˢ));
        if (result == nil) {
            @throw(runtimeCannotAllocateˢ);
        }
        @lock(ᏑgcBitsArenas.of(gcBitsArenasᴛ1.Ꮡlock));
    } else {
        result = gcBitsArenas.free;
        gcBitsArenas.free = gcBitsArenas.free.next;
        memclrNoHeapPointers(@unsafe.Pointer.FromPinnedBox(result), gcBitsChunkBytes);
    }
    result.next = default;
    // If result.bits is not 8 byte aligned adjust index so
    // that &result.bits[result.free] is 8 byte aligned.
    if ((uintptr)((uintptr)/* unsafe.Offsetof(gcBitsArena{}.bits) */ (uintptr)16 & 7) == 0) {
        result.free = 0;
    } else {
        result.free = 8 - ((uintptr)((uintptr)result.at(gcBitsArena.Ꮡbits, 0) & 7));
    }
    return result;
}

} // end runtime_package
