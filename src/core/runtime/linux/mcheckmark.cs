// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// GC checkmarks
//
// In a concurrent garbage collector, one worries about failing to mark
// a live object due to mutations without write barriers or bugs in the
// collector implementation. As a sanity check, the GC has a 'checkmark'
// mode that retraverses the object graph with the world stopped, to make
// sure that everything that should be marked is marked.
namespace go;

using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// A checkmarksMap stores the GC marks in "checkmarks" mode. It is a
// per-arena bitmap with a bit for every word in the arena. The mark
// is stored on the bit corresponding to the first word of the marked
// allocation.
[GoType] partial struct checkmarksMap {
    internal sys.NotInHeap _;
    internal array<uint8> b = new(heapArenaBytes / goarch.PtrSize / 8);
}

// If useCheckmark is true, marking of an object uses the checkmark
// bits instead of the standard mark bits.
internal static bool useCheckmark = false;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string outOfMemoryAllocatingˢ4 = "out of memory allocating checkmarks bitmap"u8;

// startCheckmarks prepares for the checkmarks phase.
//
// The world must be stopped.
internal static void startCheckmarks() {
    assertWorldStopped();
    // Clear all checkmarks.
    foreach (var (_, ai) in mheap_.allArenas) {
        var arena = mheap_.arenas[ai.l1()].Value[ai.l2()];
        var bitmap = arena.Value.checkmarks;
        if (bitmap == nil){
            // Allocate bitmap on first use.
            bitmap = (ж<checkmarksMap>)(uintptr)(persistentalloc(/* unsafe.Sizeof(*bitmap) */ (uintptr)1048576, 0, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
            if (bitmap == nil) {
                @throw(outOfMemoryAllocatingˢ4);
            }
            arena.Value.checkmarks = bitmap;
        } else {
            // Otherwise clear the existing bitmap.
            builtin.clear((~bitmap).b[..]);
        }
    }
    // Enable checkmarking.
    useCheckmark = true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcWorkNotFlushedˢ = "GC work not flushed"u8;

// endCheckmarks ends the checkmarks phase.
internal static void endCheckmarks() {
    if (gcMarkWorkAvailable(nil)) {
        @throw(gcWorkNotFlushedˢ);
    }
    useCheckmark = false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string baseˢ = "base"u8;
internal static readonly @string objˢ = "obj"u8;
internal static readonly @string checkmarkFoundUnmarkedˢ = "checkmark found unmarked object"u8;

// setCheckmark throws if marking object is a checkmarks violation,
// and otherwise sets obj's checkmark. It returns true if obj was
// already checkmarked.
internal static bool setCheckmark(uintptr obj, uintptr @base, uintptr off, markBits mbits) {
    if (!mbits.isMarked()) {
        printlock();
        print((@string)"runtime: checkmarks found unexpected unmarked object obj="u8, ((Δhex)(uint64)obj), (@string)"\n"u8);
        print((@string)"runtime: found obj at *("u8, ((Δhex)(uint64)@base), (@string)"+"u8, ((Δhex)(uint64)off), (@string)")\n"u8);
        // Dump the source (base) object
        gcDumpObject(baseˢ, @base, off);
        // Dump the object
        gcDumpObject(objˢ, obj, ~(uintptr)0);
        getg().Value.m.Value.traceback = 2;
        @throw(checkmarkFoundUnmarkedˢ);
    }
    arenaIdx ai = arenaIndex(obj);
    var arena = mheap_.arenas[ai.l1()].Value[ai.l2()];
    var arenaWord = (obj / (uintptr)heapArenaBytes / 8) % (uintptr)len((~(~arena).checkmarks).b);
    var mask = (byte)((byte)(1 << (int)(((obj / (uintptr)heapArenaBytes) % 8))));
    var bytep = (~arena).checkmarks.at(checkmarksMap.Ꮡb, (ulong)(arenaWord));
    if ((uint8)(atomic.Load8(bytep) & mask) != 0) {
        // Already checkmarked.
        return true;
    }
    atomic.Or8(bytep, mask);
    return false;
}

} // end runtime_package
