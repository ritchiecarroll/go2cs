// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Simple not-in-heap bump-pointer traceRegion allocator.
namespace go;

using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// traceRegionAlloc is a thread-safe region allocator.
// It holds a linked list of traceRegionAllocBlock.
partial struct traceRegionAlloc {
    internal mutex @lock;
    internal atomic.Bool dropping;         // For checking invariants.
    internal atomic.UnsafePointer current; // *traceRegionAllocBlock
    internal ж<traceRegionAllocBlock> full;
}

// traceRegionAllocBlock is a block in traceRegionAlloc.
//
// traceRegionAllocBlock is allocated from non-GC'd memory, so it must not
// contain heap pointers. Writes to pointers to traceRegionAllocBlocks do
// not need write barriers.
partial struct traceRegionAllocBlock {
    internal sys.NotInHeap _;
    /*embed*/ internal traceRegionAllocBlockHeader traceRegionAllocBlockHeader;
    internal array<byte> data = new(traceRegionAllocBlockData);
}

partial struct traceRegionAllocBlockHeader {
    internal ж<traceRegionAllocBlock> next;
    internal atomic.Uintptr off;
}

internal static uintptr traceRegionAllocBlockData => /* 64<<10 - unsafe.Sizeof(traceRegionAllocBlockHeader{}) */ 65520;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string traceRegionAllocTooLargeˢ => ref traceRegionAllocTooLargeˢᵇ;
private static readonly @string traceRegionAllocTooLargeˢᵇ = "traceRegion: alloc too large"u8;
internal static ref readonly @string traceRegionAllocWithˢ => ref traceRegionAllocWithˢᵇ;
private static readonly @string traceRegionAllocWithˢᵇ = "traceRegion: alloc with concurrent drop"u8;
internal static ref readonly @string traceArenaAllocˢ => ref traceArenaAllocˢᵇ;
private static readonly @string traceArenaAllocˢᵇ = "trace arena alloc"u8;
internal static ref readonly @string traceRegionOutOfMemoryˢ => ref traceRegionOutOfMemoryˢᵇ;
private static readonly @string traceRegionOutOfMemoryˢᵇ = "traceRegion: out of memory"u8;

// alloc allocates n-byte block. The block is always aligned to 8 bytes, regardless of platform.
internal static ж<notInHeap> alloc(this ж<traceRegionAlloc> Ꮡa, uintptr n) {
    n = alignUp(n, 8);
    if (n > traceRegionAllocBlockData) {
        @throw(traceRegionAllocTooLargeˢ);
    }
    if (Ꮡa.Ꮡdropping.Load()) {
        @throw(traceRegionAllocWithˢ);
    }

    // Try to bump-pointer allocate into the current block.
    ref var block = ref heap<ж<traceRegionAllocBlock>>((ж<traceRegionAllocBlock>)(uintptr)(Ꮡa.Ꮡcurrent.Load()), out var Ꮡblock);
    if (block != nil) {
        var r = block.of(traceRegionAllocBlock.Ꮡoff).Add(n);
        if (r <= (uintptr)len(block.data)) {
            return block.at(traceRegionAllocBlock.Ꮡdata, (ulong)(r - n)).Reinterpret<byte, notInHeap>();
        }
    }

    // Try to install a new block.
    ref var x = ref heap<ж<notInHeap>>(out var Ꮡx);
    systemstack(() => {
        // Acquire a.lock on the systemstack to avoid stack growth
        // and accidentally entering the tracer again.
        @lock(Ꮡa.Ꮡlock);

        // Check block again under the lock. Someone may
        // have gotten here first.
        Ꮡblock.ValueSlot = (ж<traceRegionAllocBlock>)(uintptr)(Ꮡa.Ꮡcurrent.Load());
        if (Ꮡblock.ValueSlot != nil) {
            var r = Ꮡblock.ValueSlot.of(traceRegionAllocBlock.Ꮡoff).Add(n);
            if (r <= (uintptr)len(Ꮡblock.ValueSlot.data)) {
                unlock(Ꮡa.Ꮡlock);
                Ꮡx.ValueSlot = Ꮡblock.ValueSlot.at(traceRegionAllocBlock.Ꮡdata, (ulong)(r - n)).Reinterpret<byte, notInHeap>();
                return;
            }

            // Add the existing block to the full list.
            Ꮡblock.ValueSlot.traceRegionAllocBlockHeader.next = Ꮡa.Value.full;
            Ꮡa.Value.full = Ꮡblock.ValueSlot;
        }

        // Allocate a new block.
        Ꮡblock.ValueSlot = (ж<traceRegionAllocBlock>)(uintptr)(sysAlloc(/* unsafe.Sizeof(traceRegionAllocBlock{}) */ (uintptr)65536, Ꮡmemstats.Ꮡother_sys, traceArenaAllocˢ));
        if (Ꮡblock.ValueSlot == nil) {
            @throw(traceRegionOutOfMemoryˢ);
        }

        // Allocate space for our current request, so we always make
        // progress.
        Ꮡblock.ValueSlot.of(traceRegionAllocBlock.Ꮡoff).Store(n);
        Ꮡx.ValueSlot = Ꮡblock.ValueSlot.at(traceRegionAllocBlock.Ꮡdata, 0).Reinterpret<byte, notInHeap>();

        // Publish the new block.
        Ꮡa.Ꮡcurrent.Store(@unsafe.Pointer.FromPinnedBox(Ꮡblock.ValueSlot));
        unlock(Ꮡa.Ꮡlock);
    });
    return x;
}

// drop frees all previously allocated memory and resets the allocator.
//
// drop is not safe to call concurrently with other calls to drop or with calls to alloc. The caller
// must ensure that it is not possible for anything else to be using the same structure.
internal static void drop(this ж<traceRegionAlloc> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    Ꮡa.Ꮡdropping.Store(true);
    while (a.full != nil) {
        var block = a.full;
        a.full = block.next;
        sysFree(@unsafe.Pointer.FromPinnedBox(block), /* unsafe.Sizeof(traceRegionAllocBlock{}) */ (uintptr)65536, Ꮡmemstats.Ꮡother_sys);
    }
    @unsafe.Pointer current = (uintptr)Ꮡa.Ꮡcurrent.Load(); if (current != nil) {
        sysFree(current, /* unsafe.Sizeof(traceRegionAllocBlock{}) */ (uintptr)65536, Ꮡmemstats.Ꮡother_sys);
        Ꮡa.Ꮡcurrent.Store(nil);
    }
    Ꮡa.Ꮡdropping.Store(false);
}

} // end runtime_package
