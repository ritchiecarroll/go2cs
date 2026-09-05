// mranges_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// addrRanges.init, addrRanges.add and addrRanges.cloneInto, hand-owned (increment 7 of the runtime
// row, Q-memory W2a; the three are displaced through manualConversionFuncs, so mranges.cs carries a
// placeholder where each converted body was).
//
// WHY. Go grows an addrRanges' backing array outside the heap and installs it by writing the slice
// HEADER field by field through a notInHeapSlice view of the managed a.ranges:
//
//     ranges := (*notInHeapSlice)(unsafe.Pointer(&a.ranges))
//     ranges.len = len(oldRanges) + 1
//     ranges.cap = cap(oldRanges) * 2
//     ranges.array = (*notInHeap)(persistentalloc(unsafe.Sizeof(addrRange{})*uintptr(ranges.cap), goarch.PtrSize, a.sysStat))
//
// The converter emits that view as `Ꮡa.of(addrRanges.Ꮡranges).Reinterpret<slice<addrRange>, notInHeapSlice>()`
// and the three stores as writes through the box's `Value` ref. golib's SliceHeaderBox serves the READ
// side of that pair (it materializes Go's (array, len, cap) from the live slice); the WRITE side cannot
// be served by any materializing box, and the reason is the sequence itself, not the box: nothing
// accesses the box after the `array` store — the last of the three — so there is no point at which a
// box can observe the completed header and re-base the slice, and the header is uncommittable EARLIER
// because between the `cap` store and the `array` store it names a nil array with a non-zero capacity.
// A `ref` returned from a property cannot intercept the store that lands on it. So the slice is
// re-based AT the write: each body below is the emission line for line, with the three header stores
// replaced by one native-backed slice<addrRange> over the persistentalloc'd block — golib's single
// creation door (unsafe.Slice → slice<T>.OverNativeMemory, with Go's cap as the third word), the
// DESIGN-native-backed-slice model whose element addresses are the real ones and whose copy/reslice
// arithmetic is Go's. addrRange is two uintptrs, so the door's unmanaged-element precondition holds by
// construction; persistentalloc's block is mmap'd memory outside the CLR heap (increment 6), so the
// slice's lifetime is the mapping's own, exactly as in Go.
//
// RECEIVER FORMS — measured, not chosen (2026-09-05). The emission declared init and add as
// box-receiver primaries (`this ж<addrRanges> Ꮡa`) because Go's bodies take &a.ranges; these bodies
// do not, so the natural form is the [GoRecv] value receiver — and it is also the REQUIRED one: the
// converter emits every call site of a DISPLACED method in the promoted/box default form (measured
// on export_test.go's `a.add(r.addrRange)` through the value embed `AddrRanges{addrRanges}`: the base
// converter emits `Ꮡa.of(AddrRanges.ᏑaddrRanges).add(…)`, the box hop for a box-receiver callee; with
// add displaced it emits `Ꮡa.add(…)`, which only a generated promoted forwarder can bind — and the
// TypeGenerator promotes value-receiver methods only, through a value embed). A [GoRecv] ref receiver
// gets both: the promoted forwarder and RecvGenerator's `ж<addrRanges>` twin, so the corpus's box-form
// call sites (mpagealloc.cs's `Ꮡp.of(pageAlloc.ᏑinUse).init/add`) and the test's promoted call bind
// unchanged. cloneInto was a [GoRecv] ref receiver already.
//
// What this file does NOT cover: the other writers of the same notInHeapSlice view — traceMap.newTraceMapNode
// (tracemap.cs), mheap.sysAlloc's allspans growth and allArenas (mheap.cs / malloc.cs) — write the
// WHOLE header from another header in one assignment and sit behind sysAlloc/mheap.init, which no row
// reaches; they keep their converted bodies and the sibling box's refusal-by-name. The header→slice
// READ direction (`*(*[]T)(unsafe.Pointer(&sl))`, mpagealloc_64bit.cs / mbitmap.cs) is golib's
// HeaderSliceBox, landed with this increment, not a hand-own.

using go.golib;
using goarch = go.@internal.goarch_package;
using @unsafe = go.unsafe_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // Hoisted @string literals (single allocation; Go keeps these in RODATA) — the one literal add's
    // body carries, spelled by this file because a displaced body's hoists travel with it.
    internal static readonly @string attemptedToAddZeroSizedˢ = "attempted to add zero-sized address range"u8;

    // The three header stores as one slice: Go's (array, len, cap) over a block persistentalloc has
    // just returned. `array` is a native address by construction (persistentalloc hands out mmap'd
    // memory), so the door's provenance consult answers MISS and the mapping arm builds the window.
    private static slice<addrRange> rebaseRanges(@unsafe.Pointer array, nint len, nint cap)
    {
        return @unsafe.Slice((ж<addrRange>)(uintptr)array, (uintptr)cap)[..(int)len];
    }

    // init initializes a with 16 slots and an initial capacity of 16 (the emission's body with the
    // three header stores re-based: `ranges.len = 0; ranges.cap = 16; ranges.array = persistentalloc(…)`).
    [GoRecv] internal static void init(this ref addrRanges a, ж<sysMemStat> ᏑsysStat)
    {
        ref var sysStat = ref ᏑsysStat.DerefOrNull();

        a.ranges = rebaseRanges(persistentalloc(/* unsafe.Sizeof(addrRange{}) */ (uintptr)16 * (uintptr)16, goarch.PtrSize, ᏑsysStat), 0, 16);
        a.sysStat = ᏑsysStat;
        a.totalBytes = 0;
    }

    // add inserts a new address range to a.
    //
    // r must not overlap with any address range in a and r.size() must be > 0.
    [GoRecv] internal static void add(this ref addrRanges a, addrRange r)
    {
        // The copies in this function are potentially expensive, but this data
        // structure is meant to represent the Go heap. At worst, copying this
        // would take ~160µs assuming a conservative copying rate of 25 GiB/s (the
        // copy will almost never trigger a page fault) for a 1 TiB heap with 4 MiB
        // arenas which is completely discontiguous. ~160µs is still a lot, but in
        // practice most platforms have 64 MiB arenas (which cuts this by a factor
        // of 16) and Go heaps are usually mostly contiguous, so the chance that
        // an addrRanges even grows to that size is extremely low.
        // An empty range has no effect on the set of addresses represented
        // by a, but passing a zero-sized range is almost always a bug.
        if (r.size() == 0) {
            print((@string)"runtime: range = {"u8, ((Δhex)(uint64)r.@base.addr()), (@string)", "u8, ((Δhex)(uint64)r.limit.addr()), (@string)"}\n"u8);
            @throw(attemptedToAddZeroSizedˢ);
        }
        // Because we assume r is not currently represented in a,
        // findSucc gives us our insertion index.
        nint i = a.findSucc(r.@base.addr());
        var coalescesDown = i > 0 && a.ranges[i - 1].limit.equal(r.@base);
        var coalescesUp = i < len(a.ranges) && r.limit.equal(a.ranges[i].@base);
        if (coalescesUp && coalescesDown){
            // We have neighbors and they both border us.
            // Merge a.ranges[i-1], r, and a.ranges[i] together into a.ranges[i-1].
            a.ranges[i - 1].limit = a.ranges[i].limit;
            // Delete a.ranges[i].
            copy(a.ranges[(int)(i)..], a.ranges[(int)(i + 1)..]);
            a.ranges = a.ranges[..(int)(len(a.ranges) - 1)];
        } else
        if (coalescesDown){
            // We have a neighbor at a lower address only and it borders us.
            // Merge the new space into a.ranges[i-1].
            a.ranges[i - 1].limit = r.limit;
        } else
        if (coalescesUp){
            // We have a neighbor at a higher address only and it borders us.
            // Merge the new space into a.ranges[i].
            a.ranges[i].@base = r.@base;
        } else {
            // We may or may not have neighbors which don't border us.
            // Add the new range.
            if (len(a.ranges) + 1 > cap(a.ranges)){
                // Grow the array. Note that this leaks the old array, but since
                // we're doubling we have at most 2x waste. For a 1 TiB heap and
                // 4 MiB arenas which are all discontiguous (both very conservative
                // assumptions), this would waste at most 4 MiB of memory.
                var oldRanges = a.ranges;
                // Go: ranges.len = len(oldRanges) + 1; ranges.cap = cap(oldRanges) * 2;
                //     ranges.array = (*notInHeap)(persistentalloc(unsafe.Sizeof(addrRange{})*uintptr(ranges.cap), goarch.PtrSize, a.sysStat))
                nint newLen = len(oldRanges) + 1;
                nint newCap = cap(oldRanges) * 2;
                a.ranges = rebaseRanges(persistentalloc(/* unsafe.Sizeof(addrRange{}) */ (uintptr)16 * (uintptr)newCap, goarch.PtrSize, a.sysStat), newLen, newCap);
                // Copy in the old array, but make space for the new range.
                copy(a.ranges[..(int)(i)], oldRanges[..(int)(i)]);
                copy(a.ranges[(int)(i + 1)..], oldRanges[(int)(i)..]);
            } else {
                a.ranges = a.ranges[..(int)(len(a.ranges) + 1)];
                copy(a.ranges[(int)(i + 1)..], a.ranges[(int)(i)..]);
            }
            a.ranges[i] = r;
        }
        a.totalBytes += r.size();
    }

    // cloneInto makes a deep clone of a's state into b, re-using
    // b's ranges if able.
    [GoRecv] internal static void cloneInto(this ref addrRanges a, ж<addrRanges> Ꮡb)
    {
        ref var b = ref Ꮡb.DerefOrNull();

        if (len(a.ranges) > cap(b.ranges)) {
            // Grow the array.
            // Go: ranges.len = 0; ranges.cap = cap(a.ranges);
            //     ranges.array = (*notInHeap)(persistentalloc(unsafe.Sizeof(addrRange{})*uintptr(ranges.cap), goarch.PtrSize, b.sysStat))
            nint newCap = cap(a.ranges);
            b.ranges = rebaseRanges(persistentalloc(/* unsafe.Sizeof(addrRange{}) */ (uintptr)16 * (uintptr)newCap, goarch.PtrSize, b.sysStat), 0, newCap);
        }
        b.ranges = b.ranges[..(int)(len(a.ranges))];
        b.totalBytes = a.totalBytes;
        copy(b.ranges, a.ranges);
    }

    // ---- probes for the standing guard (GolibTests.RuntimeAddrRangesTests); the runtime row is not banked ----

    /// <summary>
    /// Builds a fresh addrRanges through the hand-owned init/add, adding <paramref name="count"/>
    /// ranges of 4 KiB at <paramref name="stride"/> apart from a fixed base (a stride of 4 KiB makes
    /// every range coalesce with its predecessor; a larger one keeps them distinct), then clones it
    /// into a second fresh set. Returns the set's words, whether its ranges are sorted and disjoint,
    /// and the clone's length plus elementwise equality.
    /// </summary>
    public static (nint len, nint cap, nuint totalBytes, bool sortedDisjoint, nint cloneLen, bool cloneEqual) GoAddrRangesProbe(int count, nuint stride)
    {
        ref var stat = ref heap(new sysMemStat(), out var Ꮡstat);
        ref var a = ref heap(new addrRanges(), out var Ꮡa);
        a.init(Ꮡstat);

        for (int n = 0; n < count; n++)
        {
            nuint @base = 0x10000 + (nuint)n * stride;
            a.add(makeAddrRange(@base, @base + 4096));
        }

        bool sortedDisjoint = true;

        for (nint n = 1; n < len(a.ranges); n++)
        {
            if (a.ranges[n - 1].limit.addr() >= a.ranges[n].@base.addr())
                sortedDisjoint = false;
        }

        ref var b = ref heap(new addrRanges(), out var Ꮡb);
        b.init(Ꮡstat);
        a.cloneInto(Ꮡb);

        bool cloneEqual = len(b.ranges) == len(a.ranges);

        for (nint n = 0; cloneEqual && n < len(a.ranges); n++)
        {
            if (b.ranges[n].@base.addr() != a.ranges[n].@base.addr() || b.ranges[n].limit.addr() != a.ranges[n].limit.addr())
                cloneEqual = false;
        }

        return (len(a.ranges), cap(a.ranges), a.totalBytes, sortedDisjoint, len(b.ranges), cloneEqual);
    }
}
