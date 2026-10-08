// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

internal static uintptr pageCachePages => /* 8 * unsafe.Sizeof(pageCache{}.cache) */ 64;

// pageCache represents a per-p cache of pages the allocator can
// allocate from without a lock. More specifically, it represents
// a pageCachePages*pageSize chunk of memory with 0 or more free
// pages in it.
partial struct pageCache {
    internal uintptr @base; // base address of the chunk
    internal uint64 cache;  // 64-bit bitmap representing free pages (1 means free)
    internal uint64 scav;  // 64-bit bitmap representing scavenged pages (1 means scavenged)
}

// empty reports whether the page cache has no free pages.
internal static bool empty(this ref pageCache c) {
    return c.cache == 0;
}

// alloc allocates npages from the page cache and is the main entry
// point for allocation.
//
// Returns a base address and the amount of scavenged memory in the
// allocated region in bytes.
//
// Returns a base address of zero on failure, in which case the
// amount of scavenged memory should be ignored.
internal static (uintptr, uintptr) alloc(this ref pageCache c, uintptr npages) {
    if (c.cache == 0) {
        return (0, 0);
    }
    if (npages == 1) {
        var i = (uintptr)sys.TrailingZeros64(c.cache);
        var scav = (uint64)((c.scav.Rsh((uint64)(i))) & 1);
        c.cache &= unchecked((uint64)~(uint64)(((uint64)1).Lsh((uint64)(i)))); // set bit to mark in-use
        c.scav &= unchecked((uint64)~(uint64)(((uint64)1).Lsh((uint64)(i)))); // clear bit to mark unscavenged
        return (c.@base + i * (uintptr)pageSize, (uintptr)scav * (uintptr)pageSize);
    }
    return c.allocN(npages);
}

// allocN is a helper which attempts to allocate npages worth of pages
// from the cache. It represents the general case for allocating from
// the page cache.
//
// Returns a base address and the amount of scavenged memory in the
// allocated region in bytes.
internal static (uintptr, uintptr) allocN(this ref pageCache c, uintptr npages) {
    nuint i = findBitRange64(c.cache, (nuint)npages);
    if (i >= 64) {
        return (0, 0);
    }
    var mask = ((((uint64)1).Lsh((uint64)(npages))) - 1).Lsh(i);
    nint scav = sys.OnesCount64((uint64)(c.scav & mask));
    c.cache &= unchecked((uint64)~(uint64)(mask)); // mark in-use bits
    c.scav &= unchecked((uint64)~(uint64)(mask)); // clear scavenged bits
    return (c.@base + (uintptr)(i * (nuint)pageSize), (uintptr)scav * (uintptr)pageSize);
}

// flush empties out unallocated free pages in the given cache
// into s. Then, it clears the cache, such that empty returns
// true.
//
// p.mheapLock must be held.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
internal static void flush(this ref pageCache c, ж<pageAlloc> Ꮡp) {
    ref var Δp = ref Ꮡp.DerefOrNull();

    assertLockHeld(Δp.mheapLock);
    if (c.empty()) {
        return;
    }
    chunkIdx ci = chunkIndex(c.@base);
    nuint pi = chunkPageIndex(c.@base);
    // This method is called very infrequently, so just do the
    // slower, safer thing by iterating over each bit individually.
    for (nuint i = (nuint)0; i < 64; i++) {
        if ((uint64)(c.cache & (((uint64)1).Lsh(i))) != 0) {
            Δp.chunkOf(ci).of(pallocData.ᏑpallocBits).free1(pi + i);
            // Update density statistics.
            Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).free(ci, pi + i, 1);
        }
        if ((uint64)(c.scav & (((uint64)1).Lsh(i))) != 0) {
            Δp.chunkOf(ci).of(pallocData.Ꮡscavenged).setRange(pi + i, 1);
        }
    }
    // Since this is a lot like a free, we need to make sure
    // we update the searchAddr just like free does.
    {
        var b = (new offAddr(c.@base)); if (b.lessThan(Δp.searchAddr)) {
            Δp.searchAddr = b;
        }
    }
    Δp.update(c.@base, pageCachePages, false, false);
    c = new pageCache(nil);
}

// allocToCache acquires a pageCachePages-aligned chunk of free pages which
// may not be contiguous, and returns a pageCache structure which owns the
// chunk.
//
// p.mheapLock must be held.
//
// Must run on the system stack because p.mheapLock must be held.
//
//go:systemstack
internal static pageCache allocToCache(this ref pageAlloc Δp) {
    assertLockHeld(Δp.mheapLock);
    // If the searchAddr refers to a region which has a higher address than
    // any known chunk, then we know we're out of memory.
    if (chunkIndex(Δp.searchAddr.addr()) >= Δp.end) {
        return new pageCache(nil);
    }
    var c = new pageCache(nil);
    chunkIdx ci = chunkIndex(Δp.searchAddr.addr()); // chunk index
    ж<pallocData> chunk = default!;
    if (Δp.summary[len(Δp.summary) - 1][ci] != 0){
        // Fast path: there's free pages at or near the searchAddr address.
        chunk = Δp.chunkOf(ci);
        var (j, _) = chunk.of(pallocData.ᏑpallocBits).find(1, chunkPageIndex(Δp.searchAddr.addr()));
        if (j == ~(nuint)0) {
            @throw(badSummaryDataˢ);
        }
        c = new pageCache(
            @base: chunkBase(ci) + alignDown((uintptr)j, 64) * (uintptr)pageSize,
            cache: ~chunk.of(pallocData.ᏑpallocBits).pages64(j),
            scav: chunk.of(pallocData.Ꮡscavenged).block64(j)
        );
    } else {
        // Slow path: the searchAddr address had nothing there, so go find
        // the first free page the slow way.
        var (addr, _) = Δp.find(1);
        if (addr == 0) {
            // We failed to find adequate free space, so mark the searchAddr as OoM
            // and return an empty pageCache.
            Δp.searchAddr = maxSearchAddr();
            return new pageCache(nil);
        }
        ci = chunkIndex(addr);
        chunk = Δp.chunkOf(ci);
        c = new pageCache(
            @base: alignDown(addr, 64 * pageSize),
            cache: ~chunk.of(pallocData.ᏑpallocBits).pages64(chunkPageIndex(addr)),
            scav: chunk.of(pallocData.Ꮡscavenged).block64(chunkPageIndex(addr))
        );
    }
    // Set the page bits as allocated and clear the scavenged bits, but
    // be careful to only set and clear the relevant bits.
    nuint cpi = chunkPageIndex(c.@base);
    chunk.of(pallocData.ᏑpallocBits).allocPages64(cpi, c.cache);
    chunk.of(pallocData.Ꮡscavenged).clearBlock64(cpi, (uint64)(c.cache & c.scav));
    /* free and scavenged */
    // Update as an allocation, but note that it's not contiguous.
    Δp.update(c.@base, pageCachePages, false, true);
    // Update density statistics.
    Δp.scav.index.alloc(ci, (nuint)sys.OnesCount64(c.cache));
    // Set the search address to the last page represented by the cache.
    // Since all of the pages in this block are going to the cache, and we
    // searched for the first free page, we can confidently start at the
    // next page.
    //
    // However, p.searchAddr is not allowed to point into unmapped heap memory
    // unless it is maxSearchAddr, so make it the last page as opposed to
    // the page after.
    Δp.searchAddr = new offAddr(c.@base + (uintptr)((uintptr)pageSize * (pageCachePages - 1)));
    return c;
}

} // end runtime_package
