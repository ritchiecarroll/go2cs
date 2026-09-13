// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Implementation of (safe) user arenas.
//
// This file contains the implementation of user arenas wherein Go values can
// be manually allocated and freed in bulk. The act of manually freeing memory,
// potentially before a GC cycle, means that a garbage collection cycle can be
// delayed, improving efficiency by reducing GC cycle frequency. There are other
// potential efficiency benefits, such as improved locality and access to a more
// efficient allocation strategy.
//
// What makes the arenas here safe is that once they are freed, accessing the
// arena's memory will cause an explicit program fault, and the arena's address
// space will not be reused until no more pointers into it are found. There's one
// exception to this: if an arena allocated memory that isn't exhausted, it's placed
// back into a pool for reuse. This means that a crash is not always guaranteed.
//
// While this may seem unsafe, it still prevents memory corruption, and is in fact
// necessary in order to make new(T) a valid implementation of arenas. Such a property
// is desirable to allow for a trivial implementation. (It also avoids complexities
// that arise from synchronization with the GC when trying to set the arena chunks to
// fault while the GC is active.)
//
// The implementation works in layers. At the bottom, arenas are managed in chunks.
// Each chunk must be a multiple of the heap arena size, or the heap arena size must
// be divisible by the arena chunks. The address space for each chunk, and each
// corresponding heapArena for that address space, are eternally reserved for use as
// arena chunks. That is, they can never be used for the general heap. Each chunk
// is also represented by a single mspan, and is modeled as a single large heap
// allocation. It must be, because each chunk contains ordinary Go values that may
// point into the heap, so it must be scanned just like any other object. Any
// pointer into a chunk will therefore always cause the whole chunk to be scanned
// while its corresponding arena is still live.
//
// Chunks may be allocated either from new memory mapped by the OS on our behalf,
// or by reusing old freed chunks. When chunks are freed, their underlying memory
// is returned to the OS, set to fault on access, and may not be reused until the
// program doesn't point into the chunk anymore (the code refers to this state as
// "quarantined"), a property checked by the GC.
//
// The sweeper handles moving chunks out of this quarantine state to be ready for
// reuse. When the chunk is placed into the quarantine state, its corresponding
// span is marked as noscan so that the GC doesn't try to scan memory that would
// cause a fault.
//
// At the next layer are the user arenas themselves. They consist of a single
// active chunk which new Go values are bump-allocated into and a list of chunks
// that were exhausted when allocating into the arena. Once the arena is freed,
// it frees all full chunks it references, and places the active one onto a reuse
// list for a future arena to use. Each arena keeps its list of referenced chunks
// explicitly live until it is freed. Each user arena also maps to an object which
// has a finalizer attached that ensures the arena's chunks are all freed even if
// the arena itself is never explicitly freed.
//
// Pointer-ful memory is bump-allocated from low addresses to high addresses in each
// chunk, while pointer-free memory is bump-allocated from high address to low
// addresses. The reason for this is to take advantage of a GC optimization wherein
// the GC will stop scanning an object when there are no more pointers in it, which
// also allows us to elide clearing the heap bitmap for pointer-free Go values
// allocated into arenas.
//
// Note that arenas are not safe to use concurrently.
//
// In summary, there are 2 resources: arenas, and arena chunks. They exist in the
// following lifecycle:
//
// (1) A new arena is created via newArena.
// (2) Chunks are allocated to hold memory allocated into the arena with new or slice.
//    (a) Chunks are first allocated from the reuse list of partially-used chunks.
//    (b) If there are no such chunks, then chunks on the ready list are taken.
//    (c) Failing all the above, memory for a new chunk is mapped.
// (3) The arena is freed, or all references to it are dropped, triggering its finalizer.
//    (a) If the GC is not active, exhausted chunks are set to fault and placed on a
//        quarantine list.
//    (b) If the GC is active, exhausted chunks are placed on a fault list and will
//        go through step (a) at a later point in time.
//    (c) Any remaining partially-used chunk is placed on a reuse list.
// (4) Once no more pointers are found into quarantined arena chunks, the sweeper
//     takes these chunks out of quarantine and places them on the ready list.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using Δmath = runtime.@internal.math_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// Functions starting with arena_ are meant to be exported to downstream users
// of arenas. They should wrap these functions in a higher-lever API.
//
// The underlying arena and its resources are managed through an opaque unsafe.Pointer.

// arena_newArena is a wrapper around newUserArena.
//
//go:linkname arena_newArena arena.runtime_arena_newArena
internal static @unsafe.Pointer arena_newArena() {
    return @unsafe.Pointer.FromPinnedBox(newUserArena());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string arenaNewNonPointerTypeˢ = "arena_New: non-pointer type"u8;

// arena_arena_New is a wrapper around (*userArena).new, except that typ
// is an any (must be a *_type, still) and typ must be a type descriptor
// for a pointer to the type to actually be allocated, i.e. pass a *T
// to allocate a T. This is necessary because this function returns a *T.
//
//go:linkname arena_arena_New arena.runtime_arena_arena_New
internal static any arena_arena_New(@unsafe.Pointer arena, any typʗp) {
    ref var typ = ref heap(typʗp, out var Ꮡtyp);

    var t = (ж<_type>)(uintptr)((~efaceOf(Ꮡtyp)).data);
    if ((abiꓸKind)((~t).Kind_ & abi.KindMask) != abi.Pointer) {
        @throw(arenaNewNonPointerTypeˢ);
    }
    var te = (t.Reinterpret<_type, ptrtype>()).Value.Elem;
    @unsafe.Pointer x = (uintptr)(((ж<userArena>)(uintptr)(arena))).@new(te);
    ref var result = ref heap<any>(out var Ꮡresult);
    var e = efaceOf(Ꮡresult);
    e.Value._type = t;
    e.Value.data = x;
    return result;
}

// arena_arena_Slice is a wrapper around (*userArena).slice.
//
//go:linkname arena_arena_Slice arena.runtime_arena_arena_Slice
internal static void arena_arena_Slice(@unsafe.Pointer arena, any Δsliceᴛ, nint cap) {
    (((ж<userArena>)(uintptr)(arena))).Δslice(Δsliceᴛ, cap);
}

// arena_arena_Free is a wrapper around (*userArena).free.
//
//go:linkname arena_arena_Free arena.runtime_arena_arena_Free
internal static void arena_arena_Free(@unsafe.Pointer arena) {
    (((ж<userArena>)(uintptr)(arena))).free();
}

// arena_heapify takes a value that lives in an arena and makes a copy
// of it on the heap. Values that don't live in an arena are returned unmodified.
//
//go:linkname arena_heapify arena.runtime_arena_heapify
internal static any arena_heapify(any sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    @unsafe.Pointer v = default!;
    var e = efaceOf(Ꮡs);
    var t = e.Value._type;
    var exprᴛ1 = (abiꓸKind)((~t).Kind_ & abi.KindMask);
    if (exprᴛ1 == abi.ΔString) {
        v = stringStructOf((ж<@string>)(uintptr)((~e).data)).Value.str;
    }
    else if (exprᴛ1 == abi.Slice) {
        v = ((ж<Δsliceᴛ>)(uintptr)((~e).data)).Value.Δarray;
    }
    else if (exprᴛ1 == abi.Pointer) {
        v = e.Value.data;
    }
    else { /* default: */
        throw panic("arena: Clone only supports pointers, slices, and strings");
    }

    var span = spanOf((uintptr)v);
    if (span == nil || !(~span).isUserArenaChunk) {
        // Not stored in a user arena chunk.
        return s;
    }
    // Heap-allocate storage for a copy.
    ref var x = ref heap<any>(out var Ꮡx);
    var exprᴛ2 = (abiꓸKind)((~t).Kind_ & abi.KindMask);
    if (exprᴛ2 == abi.ΔString) {
        @string s1 = s._<@string>();
        var (s2, b) = rawstring(len(s1));
        copy(b, s1);
        x = s2;
    }
    else if (exprᴛ2 == abi.Slice) {
        nint lenΔ2 = ((ж<Δsliceᴛ>)(uintptr)((~e).data)).Value.len;
        var et = (t.Reinterpret<_type, slicetype>()).Value.Elem;
        var sl = @new<Δsliceᴛ>();
        sl.Value = new Δsliceᴛ((uintptr)makeslicecopy(et, lenΔ2, lenΔ2, ((ж<Δsliceᴛ>)(uintptr)((~e).data)).Value.Δarray), lenΔ2, lenΔ2);
        var xe = efaceOf(Ꮡx);
        xe.Value._type = t;
        xe.Value.data = @unsafe.Pointer.FromPinnedBox(sl);
    }
    else if (exprᴛ2 == abi.Pointer) {
        var et = (t.Reinterpret<_type, ptrtype>()).Value.Elem;
        @unsafe.Pointer e2 = (uintptr)newobject(et);
        typedmemmove(et, e2, (~e).data);
        var xe = efaceOf(Ꮡx);
        xe.Value._type = t;
        xe.Value.data = e2;
    }

    return x;
}

internal static UntypedInt userArenaChunkBytesMax => /* 8 << 20 */ 8388608;
internal static uintptr userArenaChunkBytes => /* uintptr(int64(userArenaChunkBytesMax-heapArenaBytes)&(int64(userArenaChunkBytesMax-heapArenaBytes)>>63) + heapArenaBytes) */ 4194304; // min(userArenaChunkBytesMax, heapArenaBytes)
internal static uintptr userArenaChunkPages => /* userArenaChunkBytes / pageSize */ 512;
internal static uintptr userArenaChunkMaxAllocBytes => /* userArenaChunkBytes / 4 */ 1048576;

/* [GoInit] runtime bootstrap init - not run; .NET is the runtime */ internal static void init() {
    if ((uintptr)(userArenaChunkPages * (uintptr)pageSize) != userArenaChunkBytes) {
        @throw("user arena chunk size is not a multiple of the page size"u8);
    }
    if (userArenaChunkBytes % physPageSize != 0) {
        @throw("user arena chunk size is not a multiple of the physical page size"u8);
    }
    if (userArenaChunkBytes < heapArenaBytes){
        if ((uintptr)((uintptr)heapArenaBytes % userArenaChunkBytes) != 0) {
            @throw("user arena chunk size is smaller than a heap arena, but doesn't divide it"u8);
        }
    } else {
        if ((uintptr)(userArenaChunkBytes % (uintptr)heapArenaBytes) != 0) {
            @throw("user arena chunks size is larger than a heap arena, but not a multiple"u8);
        }
    }
    lockInit(ref userArenaState.@lock, lockRankUserArenaState);
}

// userArenaChunkReserveBytes returns the amount of additional bytes to reserve for
// heap metadata.
internal static uintptr userArenaChunkReserveBytes() {
    // In the allocation headers experiment, we reserve the end of the chunk for
    // a pointer/scalar bitmap. We also reserve space for a dummy _type that
    // refers to the bitmap. The PtrBytes field of the dummy _type indicates how
    // many of those bits are valid.
    return (uintptr)((uintptr)(userArenaChunkBytes / (uintptr)goarch.PtrSize) / 8) + /* unsafe.Sizeof(_type{}) */ (uintptr)48;
}

[GoType] partial struct userArena {
    // fullList is a list of full chunks that have not enough free memory left, and
    // that we'll free once this user arena is freed.
    //
    // Can't use mSpanList here because it's not-in-heap.
    internal ж<mspan> fullList;
    // active is the user arena chunk we're currently allocating into.
    internal ж<mspan> active;
    // refs is a set of references to the arena chunks so that they're kept alive.
    //
    // The last reference in the list always refers to active, while the rest of
    // them correspond to fullList. Specifically, the head of fullList is the
    // second-to-last one, fullList.next is the third-to-last, and so on.
    //
    // In other words, every time a new chunk becomes active, its appended to this
    // list.
    internal slice<@unsafe.Pointer> refs;
    // defunct is true if free has been called on this arena.
    //
    // This is just a best-effort way to discover a concurrent allocation
    // and free. Also used to detect a double-free.
    internal atomic.Bool defunct;
}

// newUserArena creates a new userArena ready to be used.
internal static ж<userArena> newUserArena() {
    var a = @new<userArena>();
    SetFinalizer(a.OrTypedNil(), (ж<userArena> aΔ1) => {
        // If arena handle is dropped without being freed, then call
        // free on the arena, so the arena chunks are never reclaimed
        // by the garbage collector.
        aΔ1.free();
    });
    a.refill();
    return a;
}

// new allocates a new object of the provided type into the arena, and returns
// its pointer.
//
// This operation is not safe to call concurrently with other operations on the
// same arena.
[GoRecv] internal static @unsafe.Pointer @new(this ref userArena a, ж<_type> Ꮡtyp) {
    return (uintptr)a.alloc(Ꮡtyp, -1);
}

// slice allocates a new slice backing store. slice must be a pointer to a slice
// (i.e. *[]T), because userArenaSlice will update the slice directly.
//
// cap determines the capacity of the slice backing store and must be non-negative.
//
// This operation is not safe to call concurrently with other operations on the
// same arena.
[GoRecv] internal static void Δslice(this ref userArena a, any slʗp, nint cap) {
    ref var sl = ref heap(slʗp, out var Ꮡsl);

    if (cap < 0) {
        throw panic("userArena.slice: negative cap");
    }
    var i = efaceOf(Ꮡsl);
    var typ = i.Value._type;
    if ((abiꓸKind)((~typ).Kind_ & abi.KindMask) != abi.Pointer) {
        throw panic("slice result of non-ptr type");
    }
    typ = (typ.Reinterpret<_type, ptrtype>()).Value.Elem;
    if ((abiꓸKind)((~typ).Kind_ & abi.KindMask) != abi.Slice) {
        throw panic("slice of non-ptr-to-slice type");
    }
    typ = (typ.Reinterpret<_type, slicetype>()).Value.Elem;
    // t is now the element type of the slice we want to allocate.
    ((ж<Δsliceᴛ>)(uintptr)((~i).data)).Value = new Δsliceᴛ((uintptr)a.alloc(typ, cap), cap, cap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fullListDoesnTMatchRefsˢ = "full list doesn't match refs list in length"u8;

// free returns the userArena's chunks back to mheap and marks it as defunct.
//
// Must be called at most once for any given arena.
//
// This operation is not safe to call concurrently with other operations on the
// same arena.
internal static void free(this ж<userArena> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    // Check for a double-free.
    if (Ꮡa.of(userArena.Ꮡdefunct).Load()) {
        throw panic("arena double free");
    }
    // Mark ourselves as defunct.
    Ꮡa.of(userArena.Ꮡdefunct).Store(true);
    SetFinalizer(Ꮡa.OrTypedNil(), default!);
    // Free all the full arenas.
    //
    // The refs on this list are in reverse order from the second-to-last.
    var s = a.fullList;
    nint i = len(a.refs) - 2;
    while (s != nil) {
        a.fullList = s.Value.next;
        s.Value.next = default!;
        freeUserArenaChunk(s, a.refs[i]);
        s = a.fullList;
        i--;
    }
    if (a.fullList != nil || i >= 0) {
        // There's still something left on the full list, or we
        // failed to actually iterate over the entire refs list.
        @throw(fullListDoesnTMatchRefsˢ);
    }
    // Put the active chunk onto the reuse list.
    //
    // Note that active's reference is always the last reference in refs.
    s = a.active;
    if (s != nil) {
        if (raceenabled || msanenabled || asanenabled){
            // Don't reuse arenas with sanitizers enabled. We want to catch
            // any use-after-free errors aggressively.
            freeUserArenaChunk(s, a.refs[len(a.refs) - 1]);
        } else {
            @lock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
            userArenaState.reuse = append(userArenaState.reuse, new liveUserArenaChunk(s, a.refs[len(a.refs) - 1]));
            unlock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
        }
    }
    // nil out a.active so that a race with freeing will more likely cause a crash.
    a.active = default!;
    a.refs = default!;
}

// alloc reserves space in the current chunk or calls refill and reserves space
// in a new chunk. If cap is negative, the type will be taken literally, otherwise
// it will be considered as an element type for a slice backing store with capacity
// cap.
[GoRecv] internal static @unsafe.Pointer alloc(this ref userArena a, ж<_type> Ꮡtyp, nint cap) {
    var s = a.active;
    @unsafe.Pointer x = default!;
    while (ᐧ) {
        x = (uintptr)s.userArenaNextFree(Ꮡtyp, cap);
        if (x != nil) {
            break;
        }
        s = a.refill();
    }
    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wastedTooMuchMemoryInAnˢ = "wasted too much memory in an arena chunk"u8;
internal static readonly @string outOfMemoryˢ = "out of memory"u8;

// refill inserts the current arena chunk onto the full list and obtains a new
// one, either from the partial list or allocating a new one, both from mheap.
[GoRecv] internal static ж<mspan> refill(this ref userArena a) {
    // If there's an active chunk, assume it's full.
    var s = a.active;
    if (s != nil) {
        if ((~s).userArenaChunkFree.size() > userArenaChunkMaxAllocBytes) {
            // It's difficult to tell when we're actually out of memory
            // in a chunk because the allocation that failed may still leave
            // some free space available. However, that amount of free space
            // should never exceed the maximum allocation size.
            @throw(wastedTooMuchMemoryInAnˢ);
        }
        s.Value.next = a.fullList;
        a.fullList = s;
        a.active = default!;
        s = default!;
    }
    @unsafe.Pointer x = default!;
    // Check the partially-used list.
    @lock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
    if (len(userArenaState.reuse) > 0) {
        // Pick off the last arena chunk from the list.
        nint n = len(userArenaState.reuse) - 1;
        x = userArenaState.reuse[n].x;
        s = userArenaState.reuse[n].mspan;
        userArenaState.reuse[n].x = default!;
        userArenaState.reuse[n].mspan = default!;
        userArenaState.reuse = userArenaState.reuse[..(int)(n)];
    }
    unlock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
    if (s == nil) {
        // Allocate a new one.
        (x, s) = newUserArenaChunk();
        if (s == nil) {
            @throw(outOfMemoryˢ);
        }
    }
    a.refs = append(a.refs, x);
    a.active = s;
    return s;
}

[GoType] partial struct liveUserArenaChunk {
    internal partial ref ж<mspan> mspan { get; } // Must represent a user arena chunk.
    // Reference to mspan.base() to keep the chunk alive.
    internal @unsafe.Pointer x;
}


[GoType("dyn")] partial struct userArenaStateᴛ1 {
    internal mutex @lock;
    // reuse contains a list of partially-used and already-live
    // user arena chunks that can be quickly reused for another
    // arena.
    //
    // Protected by lock.
    internal slice<liveUserArenaChunk> reuse;
    // fault contains full user arena chunks that need to be faulted.
    //
    // Protected by lock.
    internal slice<liveUserArenaChunk> fault;
}
internal static ж<userArenaStateᴛ1> ᏑuserArenaState = new StandardBox<userArenaStateᴛ1>(new userArenaStateᴛ1());
internal static ref userArenaStateᴛ1 userArenaState => ref ᏑuserArenaState.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mallocDeadlockˢ = "malloc deadlock"u8;
internal static readonly @string mallocDuringSignalˢ = "malloc during signal"u8;
internal static readonly @string arenaChunkNeedsZeroingˢ = "arena chunk needs zeroing, but should already be zeroed"u8;
internal static readonly @string mallocgcCalledWithoutAPˢ = "mallocgc called without a P or outside bootstrapping"u8;

// userArenaNextFree reserves space in the user arena for an item of the specified
// type. If cap is not -1, this is for an array of cap elements of type t.
internal static @unsafe.Pointer userArenaNextFree(this ж<mspan> Ꮡs, ж<_type> Ꮡtyp, nint cap) {
    ref var s = ref Ꮡs.DerefOrNull();
    ref var typ = ref Ꮡtyp.DerefOrNull();

    var size = typ.Size_;
    if (cap > 0) {
        if (size > ~(uintptr)0 / (uintptr)cap) {
            // Overflow.
            @throw(outOfMemoryˢ);
        }
        size *= (uintptr)cap;
    }
    if (size == 0 || cap == 0) {
        return @unsafe.Pointer.FromBox(Ꮡzerobase);
    }
    if (size > userArenaChunkMaxAllocBytes) {
        // Redirect allocations that don't fit into a chunk well directly
        // from the heap.
        if (cap >= 0) {
            return (uintptr)newarray(Ꮡtyp, cap);
        }
        return (uintptr)newobject(Ꮡtyp);
    }
    // Prevent preemption as we set up the space for a new object.
    //
    // Act like we're allocating.
    var mp = acquirem();
    if ((~mp).mallocing != 0) {
        @throw(mallocDeadlockˢ);
    }
    if ((~mp).gsignal == getg()) {
        @throw(mallocDuringSignalˢ);
    }
    mp.Value.mallocing = 1;
    @unsafe.Pointer ptr = default!;
    if (!typ.Pointers()){
        // Allocate pointer-less objects from the tail end of the chunk.
        var (v, ok) = s.userArenaChunkFree.takeFromBack(size, typ.Align_);
        if (ok) {
            ptr = (@unsafe.Pointer)v;
        }
    } else {
        var (v, ok) = s.userArenaChunkFree.takeFromFront(size, typ.Align_);
        if (ok) {
            ptr = (@unsafe.Pointer)v;
        }
    }
    if (ptr == nil) {
        // Failed to allocate.
        mp.Value.mallocing = 0;
        releasem(ref (mp).DerefOrNull());
        return default!;
    }
    if (s.needzero != 0) {
        @throw(arenaChunkNeedsZeroingˢ);
    }
    // Set up heap bitmap and do extra accounting.
    if (typ.Pointers()) {
        if (cap >= 0){
            userArenaHeapBitsSetSliceType(ref (Ꮡtyp).DerefOrNull(), cap, ptr, Ꮡs);
        } else {
            userArenaHeapBitsSetType(ref (Ꮡtyp).DerefOrNull(), ptr, Ꮡs);
        }
        var c = getMCache(ref (mp).DerefOrNull());
        if (c == nil) {
            @throw(mallocgcCalledWithoutAPˢ);
        }
        if (cap > 0){
            c.Value.scanAlloc += size - (typ.Size_ - typ.PtrBytes);
        } else {
            c.Value.scanAlloc += typ.PtrBytes;
        }
    }
    // Ensure that the stores above that initialize x to
    // type-safe memory and set the heap bits occur before
    // the caller can make ptr observable to the garbage
    // collector. Otherwise, on weakly ordered machines,
    // the garbage collector could follow a pointer to x,
    // but see uninitialized memory or stale heap bits.
    publicationBarrier();
    mp.Value.mallocing = 0;
    releasem(ref (mp).DerefOrNull());
    return ptr;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeAllocationSizeOutˢ = "runtime: allocation size out of range"u8;

// userArenaHeapBitsSetSliceType is the equivalent of heapBitsSetType but for
// Go slice backing store values allocated in a user arena chunk. It sets up the
// heap bitmap for n consecutive values with type typ allocated at address ptr.
internal static void userArenaHeapBitsSetSliceType(ref _type typ, nint n, @unsafe.Pointer ptr, ж<mspan> Ꮡs) {
    var (mem, overflow) = Δmath.MulUintptr(typ.Size_, (uintptr)n);
    if (overflow || n < 0 || mem > maxAlloc) {
        throw panic(((plainError)(@string)runtimeAllocationSizeOutˢ));
    }
    for (nint i = 0; i < n; i++) {
        userArenaHeapBitsSetType(ref typ, (uintptr)add(ptr, (uintptr)i * typ.Size_), Ꮡs);
    }
}

// userArenaHeapBitsSetType is the equivalent of heapSetType but for
// non-slice-backing-store Go values allocated in a user arena chunk. It
// sets up the type metadata for the value with type typ allocated at address ptr.
// base is the base address of the arena chunk.
internal static void userArenaHeapBitsSetType(ref _type typ, @unsafe.Pointer ptr, ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    var @base = s.@base();
    var h = s.writeUserArenaHeapBits((uintptr)ptr);
    var Δp = typ.GCData; // start of 1-bit pointer mask (or GC program)
    uintptr gcProgBits = default!;
    if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0) {
        // Expand gc program, using the object itself for storage.
        gcProgBits = runGCProg(addb(Δp, 4), (ж<byte>)(uintptr)(ptr));
        Δp = (ж<byte>)(uintptr)(ptr);
    }
    var nb = typ.PtrBytes / (uintptr)goarch.PtrSize;
    for (var i = (uintptr)0; i < nb; i += ptrBits) {
        var k = nb - i;
        if (k > ptrBits) {
            k = ptrBits;
        }
        // N.B. On big endian platforms we byte swap the data that we
        // read from GCData, which is always stored in little-endian order
        // by the compiler. writeUserArenaHeapBits handles data in
        // a platform-ordered way for efficiency, but stores back the
        // data in little endian order, since we expose the bitmap through
        // a dummy type.
        h = h.write(Ꮡs, readUintptr(addb(Δp, i / 8)), k);
    }
    // Note: we call pad here to ensure we emit explicit 0 bits
    // for the pointerless tail of the object. This ensures that
    // there's only a single noMorePtrs mark for the next object
    // to clear. We don't need to do this to clear stale noMorePtrs
    // markers from previous uses because arena chunk pointer bitmaps
    // are always fully cleared when reused.
    h = h.pad(Ꮡs, typ.Size_ - typ.PtrBytes);
    h.flush(Ꮡs, (uintptr)ptr, typ.Size_);
    if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0) {
        // Zero out temporary ptrmask buffer inside object.
        memclrNoHeapPointers(ptr, (gcProgBits + 7) / 8);
    }
    // Update the PtrBytes value in the type information. After this
    // point, the GC will observe the new bitmap.
    s.largeType.Value.PtrBytes = (uintptr)ptr - @base + typ.PtrBytes;
    // Double-check that the bitmap was written out correctly.
    const bool doubleCheck = false;
    if (doubleCheck) {
        doubleCheckHeapPointersInterior((uintptr)ptr, (uintptr)ptr, typ.Size_, typ.Size_, ref typ, Ꮡs.of(mspan.ᏑlargeType), Ꮡs);
    }
}

[GoType] partial struct ΔwriteUserArenaHeapBits {
    internal uintptr offset; // offset in span that the low bit of mask represents the pointer state of.
    internal uintptr mask; // some pointer bits starting at the address addr.
    internal uintptr valid; // number of bits in buf that are valid (including low)
    internal uintptr low; // number of low-order bits to not overwrite
}

[GoRecv] internal static ΔwriteUserArenaHeapBits /*h*/ writeUserArenaHeapBits(this ref mspan s, uintptr addr) {
    ΔwriteUserArenaHeapBits h = default!;

    var offset = addr - s.@base();
    // We start writing bits maybe in the middle of a heap bitmap word.
    // Remember how many bits into the word we started, so we can be sure
    // not to overwrite the previous bits.
    h.low = offset / (uintptr)goarch.PtrSize % (uintptr)ptrBits;
    // round down to heap word that starts the bitmap word.
    h.offset = offset - h.low * (uintptr)goarch.PtrSize;
    // We don't have any bits yet.
    h.mask = 0;
    h.valid = h.low;
    return h;
}

// write appends the pointerness of the next valid pointer slots
// using the low valid bits of bits. 1=pointer, 0=scalar.
internal static ΔwriteUserArenaHeapBits write(this ΔwriteUserArenaHeapBits h, ж<mspan> Ꮡs, uintptr bits, uintptr valid) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (h.valid + valid <= ptrBits) {
        // Fast path - just accumulate the bits.
        h.mask |= (uintptr)(bits.Lsh((uint64)(h.valid)));
        h.valid += valid;
        return h;
    }
    // Too many bits to fit in this word. Write the current word
    // out and move on to the next word.
    var data = (uintptr)(h.mask | bits.Lsh((uint64)(h.valid))); // mask for this word
    h.mask = bits.Rsh((uint64)(((uintptr)ptrBits - h.valid))); // leftover for next word
    h.valid += valid - (uintptr)ptrBits; // have h.valid+valid bits, writing ptrBits of them
    // Flush mask to the memory bitmap.
    var idx = h.offset / (uintptr)(ptrBits * goarch.PtrSize);
    var m = ((uintptr)1).Lsh((uint64)(h.low)) - 1;
    var bitmap = s.heapBits();
    bitmap[(nint)(idx)] = bswapIfBigEndian((uintptr)((uintptr)(bswapIfBigEndian(bitmap[(nint)(idx)]) & m) | data));
    // Note: no synchronization required for this write because
    // the allocator has exclusive access to the page, and the bitmap
    // entries are all for a single page. Also, visibility of these
    // writes is guaranteed by the publication barrier in mallocgc.
    // Move to next word of bitmap.
    h.offset += ptrBits * goarch.PtrSize;
    h.low = 0;
    return h;
}

// Add padding of size bytes.
internal static ΔwriteUserArenaHeapBits pad(this ΔwriteUserArenaHeapBits h, ж<mspan> Ꮡs, uintptr size) {
    if (size == 0) {
        return h;
    }
    var words = size / (uintptr)goarch.PtrSize;
    while (words > ptrBits) {
        h = h.write(Ꮡs, 0, ptrBits);
        words -= ptrBits;
    }
    return h.write(Ꮡs, 0, words);
}

// Flush the bits that have been written, and add zeros as needed
// to cover the full object [addr, addr+size).
internal static void flush(this ΔwriteUserArenaHeapBits h, ж<mspan> Ꮡs, uintptr addr, uintptr size) {
    ref var s = ref Ꮡs.DerefOrNull();

    var offset = addr - s.@base();
    // zeros counts the number of bits needed to represent the object minus the
    // number of bits we've already written. This is the number of 0 bits
    // that need to be added.
    var zeros = (offset + size - h.offset) / (uintptr)goarch.PtrSize - h.valid;
    // Add zero bits up to the bitmap word boundary
    if (zeros > 0) {
        var z = (uintptr)ptrBits - h.valid;
        if (z > zeros) {
            z = zeros;
        }
        h.valid += z;
        zeros -= z;
    }
    // Find word in bitmap that we're going to write.
    var bitmap = s.heapBits();
    var idx = h.offset / (uintptr)(ptrBits * goarch.PtrSize);
    // Write remaining bits.
    if (h.valid != h.low) {
        var m = ((uintptr)1).Lsh((uint64)(h.low)) - 1; // don't clear existing bits below "low"
        m |= (uintptr)(~(((uintptr)1).Lsh((uint64)(h.valid)) - 1)); // don't clear existing bits above "valid"
        bitmap[(nint)(idx)] = bswapIfBigEndian((uintptr)((uintptr)(bswapIfBigEndian(bitmap[(nint)(idx)]) & m) | h.mask));
    }
    if (zeros == 0) {
        return;
    }
    // Advance to next bitmap word.
    h.offset += ptrBits * goarch.PtrSize;
    // Continue on writing zeros for the rest of the object.
    // For standard use of the ptr bits this is not required, as
    // the bits are read from the beginning of the object. Some uses,
    // like noscan spans, oblets, bulk write barriers, and cgocheck, might
    // start mid-object, so these writes are still required.
    while (ᐧ) {
        // Write zero bits.
        var idxΔ1 = h.offset / (uintptr)(ptrBits * goarch.PtrSize);
        if (zeros < ptrBits){
            bitmap[(nint)(idxΔ1)] = bswapIfBigEndian((uintptr)(bswapIfBigEndian(bitmap[(nint)(idxΔ1)]) & ~(((uintptr)1).Lsh((uint64)(zeros)) - 1)));
            break;
        } else 
        if (zeros == ptrBits){
            bitmap[(nint)(idxΔ1)] = 0;
            break;
        } else {
            bitmap[(nint)(idxΔ1)] = 0;
            zeros -= ptrBits;
        }
        h.offset += ptrBits * goarch.PtrSize;
    }
}

// bswapIfBigEndian swaps the byte order of the uintptr on goarch.BigEndian platforms,
// and leaves it alone elsewhere.
internal static uintptr bswapIfBigEndian(uintptr x) {
    if (goarch.BigEndian) {
        if (goarch.PtrSize == 8) {
            return (uintptr)sys.Bswap64((uint64)x);
        }
        return (uintptr)sys.Bswap32((uint32)x);
    }
    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string newUserArenaChunkCalledˢ = "newUserArenaChunk called with gcphase == _GCmarktermination"u8;
internal static readonly @string newUserArenaChunkCalledˢ2 = "newUserArenaChunk called without a P or outside bootstrapping"u8;
internal static readonly @string userArenaChunkIsNotˢ = "user arena chunk is not aligned to the physical page size"u8;

// newUserArenaChunk allocates a user arena chunk, which maps to a single
// heap arena and single span. Returns a pointer to the base of the chunk
// (this is really important: we need to keep the chunk alive) and the span.
internal static (@unsafe.Pointer, ж<mspan>) newUserArenaChunk() {
    if (gcphase == _GCmarktermination) {
        @throw(newUserArenaChunkCalledˢ);
    }
    // Deduct assist credit. Because user arena chunks are modeled as one
    // giant heap object which counts toward heapLive, we're obligated to
    // assist the GC proportionally (and it's worth noting that the arena
    // does represent additional work for the GC, but we also have no idea
    // what that looks like until we actually allocate things into the
    // arena).
    deductAssistCredit(userArenaChunkBytes);
    // Set mp.mallocing to keep from being preempted by GC.
    var mp = acquirem();
    if ((~mp).mallocing != 0) {
        @throw(mallocDeadlockˢ);
    }
    if ((~mp).gsignal == getg()) {
        @throw(mallocDuringSignalˢ);
    }
    mp.Value.mallocing = 1;
    // Allocate a new user arena.
    ref var span = ref heap<ж<mspan>>(out var Ꮡspan);
    systemstack(() => {
        Ꮡspan.ValueSlot = Ꮡmheap_.allocUserArenaChunk();
    });
    if (span == nil) {
        @throw(outOfMemoryˢ);
    }
    @unsafe.Pointer x = (@unsafe.Pointer)span.@base();
    // Allocate black during GC.
    // All slots hold nil so no scanning is needed.
    // This may be racing with GC so do it atomically if there can be
    // a race marking the bit.
    if (gcphase != _GCoff) {
        gcmarknewobject(span, span.@base());
    }
    if (raceenabled) {
        // TODO(mknyszek): Track individual objects.
        racemalloc((@unsafe.Pointer)span.@base(), (~span).elemsize);
    }
    if (msanenabled) {
        // TODO(mknyszek): Track individual objects.
        msanmalloc((@unsafe.Pointer)span.@base(), (~span).elemsize);
    }
    if (asanenabled) {
        // TODO(mknyszek): Track individual objects.
        var rzSize = computeRZlog((~span).elemsize);
        span.Value.elemsize -= rzSize;
        span.Value.largeType.Value.Size_ = span.Value.elemsize;
        var rzStart = span.@base() + (~span).elemsize;
        span.Value.userArenaChunkFree = makeAddrRange(span.@base(), rzStart);
        asanpoison((@unsafe.Pointer)rzStart, (~span).limit - rzStart);
        asanunpoison((@unsafe.Pointer)span.@base(), (~span).elemsize);
    }
    {
        nint rate = MemProfileRate; if (rate > 0) {
            var c = getMCache(ref (mp).DerefOrNull());
            if (c == nil) {
                @throw(newUserArenaChunkCalledˢ2);
            }
            // Note cache c only valid while m acquired; see #47302
            if (rate != 1 && userArenaChunkBytes < (~c).nextSample){
                c.Value.nextSample -= userArenaChunkBytes;
            } else {
                profilealloc(ref (mp).DerefOrNull(), (@unsafe.Pointer)span.@base(), userArenaChunkBytes);
            }
        }
    }
    mp.Value.mallocing = 0;
    releasem(ref (mp).DerefOrNull());
    // Again, because this chunk counts toward heapLive, potentially trigger a GC.
    {
        var t = (new gcTrigger(kind: gcTriggerHeap)); if (t.test()) {
            gcStart(t);
        }
    }
    if (debug.malloc) {
        if (inittrace.active && inittrace.id == (~getg()).goid) {
            // Init functions are executed sequentially in a single goroutine.
            inittrace.bytes += (uint64)userArenaChunkBytes;
        }
    }
    // Double-check it's aligned to the physical page size. Based on the current
    // implementation this is trivially true, but it need not be in the future.
    // However, if it's not aligned to the physical page size then we can't properly
    // set it to fault later.
    if ((uintptr)x % physPageSize != 0) {
        @throw(userArenaChunkIsNotˢ);
    }
    return (x, span);
}

// isUnusedUserArenaChunk indicates that the arena chunk has been set to fault
// and doesn't contain any scannable memory anymore. However, it might still be
// mSpanInUse as it sits on the quarantine list, since it needs to be swept.
//
// This is not safe to execute unless the caller has ownership of the mspan or
// the world is stopped (preemption is prevented while the relevant state changes).
//
// This is really only meant to be used by accounting tests in the runtime to
// distinguish when a span shouldn't be counted (since mSpanInUse might not be
// enough).
[GoRecv] internal static bool isUnusedUserArenaChunk(this ref mspan s) {
    return s.isUserArenaChunk && s.spanclass == makeSpanClass(0, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidSpanInHeapArenaˢ = "invalid span in heapArena for user arena"u8;
internal static readonly @string spanOnUserArenaFaultListˢ = "span on userArena.faultList has invalid size"u8;

// setUserArenaChunkToFault sets the address space for the user arena chunk to fault
// and releases any underlying memory resources.
//
// Must be in a non-preemptible state to ensure the consistency of statistics
// exported to MemStats.
internal static void setUserArenaChunkToFault(this ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (!s.isUserArenaChunk) {
        @throw(invalidSpanInHeapArenaˢ);
    }
    if (s.npages * (uintptr)pageSize != userArenaChunkBytes) {
        @throw(spanOnUserArenaFaultListˢ);
    }
    // Update the span class to be noscan. What we want to happen is that
    // any pointer into the span keeps it from getting recycled, so we want
    // the mark bit to get set, but we're about to set the address space to fault,
    // so we have to prevent the GC from scanning this memory.
    //
    // It's OK to set it here because (1) a GC isn't in progress, so the scanning code
    // won't make a bad decision, (2) we're currently non-preemptible and in the runtime,
    // so a GC is blocked from starting. We might race with sweeping, which could
    // put it on the "wrong" sweep list, but really don't care because the chunk is
    // treated as a large object span and there's no meaningful difference between scan
    // and noscan large objects in the sweeper. The STW at the start of the GC acts as a
    // barrier for this update.
    s.spanclass = makeSpanClass(0, true);
    // Actually set the arena chunk to fault, so we'll get dangling pointer errors.
    // sysFault currently uses a method on each OS that forces it to evacuate all
    // memory backing the chunk.
    sysFault((@unsafe.Pointer)s.@base(), s.npages * (uintptr)pageSize);
    // Everything on the list is counted as in-use, however sysFault transitions to
    // Reserved, not Prepared, so we skip updating heapFree or heapReleased and just
    // remove the memory from the total altogether; it's just address space now.
    ᏑgcController.of(gcControllerState.ᏑheapInUse).add(-(int64)(s.npages * (uintptr)pageSize));
    // Count this as a free of an object right now as opposed to when
    // the span gets off the quarantine list. The main reason is so that the
    // amount of bytes allocated doesn't exceed how much is counted as
    // "mapped ready," which could cause a deadlock in the pacer.
    ᏑgcController.of(gcControllerState.ᏑtotalFree).Add((int64)s.elemsize);
    // Update consistent stats to match.
    //
    // We're non-preemptible, so it's safe to update consistent stats (our P
    // won't change out from under us).
    var stats = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
    atomic.Xaddint64(stats.of(heapStatsDelta.Ꮡcommitted), -(int64)(s.npages * (uintptr)pageSize));
    atomic.Xaddint64(stats.of(heapStatsDelta.ᏑinHeap), -(int64)(s.npages * (uintptr)pageSize));
    atomic.Xadd64(stats.of(heapStatsDelta.ᏑlargeFreeCount), 1);
    atomic.Xadd64(stats.of(heapStatsDelta.ᏑlargeFree), (int64)s.elemsize);
    Ꮡmemstats.of(mstats.ᏑheapStats).release();
    // This counts as a free, so update heapLive.
    ᏑgcController.update(-(int64)s.elemsize, 0);
    // Mark it as free for the race detector.
    if (raceenabled) {
        racefree((@unsafe.Pointer)s.@base(), s.elemsize);
    }
    systemstack(() => {
        // Add the user arena to the quarantine list.
        @lock(Ꮡmheap_.of(mheap.Ꮡlock));
        Ꮡmheap_.of(mheap.ᏑuserArena).of(mheap_userArena.ᏑquarantineList).insert(Ꮡs);
        unlock(Ꮡmheap_.of(mheap.Ꮡlock));
    });
}

// inUserArenaChunk returns true if p points to a user arena chunk.
internal static bool inUserArenaChunk(uintptr Δp) {
    var s = spanOf(Δp);
    if (s == nil) {
        return false;
    }
    return (~s).isUserArenaChunk;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string spanIsNotForAUserArenaˢ = "span is not for a user arena"u8;
internal static readonly @string invalidUserArenaSpanSizeˢ = "invalid user arena span size"u8;

// freeUserArenaChunk releases the user arena represented by s back to the runtime.
//
// x must be a live pointer within s.
//
// The runtime will set the user arena to fault once it's safe (the GC is no longer running)
// and then once the user arena is no longer referenced by the application, will allow it to
// be reused.
internal static void freeUserArenaChunk(ж<mspan> Ꮡs, @unsafe.Pointer x) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (!s.isUserArenaChunk) {
        @throw(spanIsNotForAUserArenaˢ);
    }
    if (s.npages * (uintptr)pageSize != userArenaChunkBytes) {
        @throw(invalidUserArenaSpanSizeˢ);
    }
    // Mark the region as free to various sanitizers immediately instead
    // of handling them at sweep time.
    if (raceenabled) {
        racefree((@unsafe.Pointer)s.@base(), s.elemsize);
    }
    if (msanenabled) {
        msanfree((@unsafe.Pointer)s.@base(), s.elemsize);
    }
    if (asanenabled) {
        asanpoison((@unsafe.Pointer)s.@base(), s.elemsize);
    }
    // Make ourselves non-preemptible as we manipulate state and statistics.
    //
    // Also required by setUserArenaChunksToFault.
    var mp = acquirem();
    // We can only set user arenas to fault if we're in the _GCoff phase.
    if (gcphase == _GCoff){
        @lock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
        var faultList = userArenaState.fault;
        userArenaState.fault = default!;
        unlock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
        Ꮡs.setUserArenaChunkToFault();
        foreach (var (_, lc) in faultList) {
            lc.mspan.setUserArenaChunkToFault();
        }
        // Until the chunks are set to fault, keep them alive via the fault list.
        KeepAlive(x);
        KeepAlive(faultList);
    } else {
        // Put the user arena on the fault list.
        @lock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
        userArenaState.fault = append(userArenaState.fault, new liveUserArenaChunk(Ꮡs, x.Value));
        unlock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
    }
    releasem(ref (mp).DerefOrNull());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sysAllocSizeIsNotˢ = "sysAlloc size is not divisible by userArenaChunkBytes"u8;

// allocUserArenaChunk attempts to reuse a free user arena chunk represented
// as a span.
//
// Must be in a non-preemptible state to ensure the consistency of statistics
// exported to MemStats.
//
// Acquires the heap lock. Must run on the system stack for that reason.
//
//go:systemstack
internal static ж<mspan> allocUserArenaChunk(this ж<mheap> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    ж<mspan> s = default!;
    uintptr @base = default!;
    // First check the free list.
    @lock(Ꮡh.of(mheap.Ꮡlock));
    if (!h.userArena.readyList.isEmpty()){
        s = h.userArena.readyList.first;
        Ꮡh.of(mheap.ᏑuserArena).of(mheap_userArena.ᏑreadyList).remove(s);
        @base = s.@base();
    } else {
        // Free list was empty, so allocate a new arena.
        var hintList = Ꮡh.of(mheap.ᏑuserArena).of(mheap_userArena.ᏑarenaHints);
        if (raceenabled) {
            // In race mode just use the regular heap hints. We might fragment
            // the address space, but the race detector requires that the heap
            // is mapped contiguously.
            hintList = Ꮡh.of(mheap.ᏑarenaHints);
        }
        var (v, size) = Ꮡh.sysAlloc(userArenaChunkBytes, hintList, false);
        if (size % userArenaChunkBytes != 0) {
            @throw(sysAllocSizeIsNotˢ);
        }
        if (size > userArenaChunkBytes) {
            // We got more than we asked for. This can happen if
            // heapArenaSize > userArenaChunkSize, or if sysAlloc just returns
            // some extra as a result of trying to find an aligned region.
            //
            // Divide it up and put it on the ready list.
            for (var i = userArenaChunkBytes; i < size; i += userArenaChunkBytes) {
                var sΔ1 = Ꮡh.allocMSpanLocked();
                sΔ1.init((uintptr)v + i, userArenaChunkPages);
                Ꮡh.of(mheap.ᏑuserArena).of(mheap_userArena.ᏑreadyList).insertBack(sΔ1);
            }
            size = userArenaChunkBytes;
        }
        @base = (uintptr)v;
        if (@base == 0) {
            // Out of memory.
            unlock(Ꮡh.of(mheap.Ꮡlock));
            return default!;
        }
        s = Ꮡh.allocMSpanLocked();
    }
    unlock(Ꮡh.of(mheap.Ꮡlock));
    // sysAlloc returns Reserved address space, and any span we're
    // reusing is set to fault (so, also Reserved), so transition
    // it to Prepared and then Ready.
    //
    // Unlike (*mheap).grow, just map in everything that we
    // asked for. We're likely going to use it all.
    sysMap((@unsafe.Pointer)@base, userArenaChunkBytes, ᏑgcController.of(gcControllerState.ᏑheapReleased));
    sysUsed((@unsafe.Pointer)@base, userArenaChunkBytes, userArenaChunkBytes);
    // Model the user arena as a heap span for a large object.
    var spc = makeSpanClass(0, false);
    Ꮡh.initSpan(s, spanAllocHeap, spc, @base, userArenaChunkPages);
    s.Value.isUserArenaChunk = true;
    s.Value.elemsize -= userArenaChunkReserveBytes();
    s.Value.freeindex = 1;
    s.Value.allocCount = 1;
    // Adjust s.limit down to the object-containing part of the span.
    //
    // This is just to create a slightly tighter bound on the limit.
    // It's totally OK if the garbage collector, in particular
    // conservative scanning, can temporarily observes an inflated
    // limit. It will simply mark the whole chunk or just skip it
    // since we're in the mark phase anyway.
    s.Value.limit = s.@base() + (~s).elemsize;
    // Account for this new arena chunk memory.
    ᏑgcController.of(gcControllerState.ᏑheapInUse).add((int64)userArenaChunkBytes);
    ᏑgcController.of(gcControllerState.ᏑheapReleased).add(-(int64)userArenaChunkBytes);
    var stats = Ꮡmemstats.of(mstats.ᏑheapStats).acquire();
    atomic.Xaddint64(stats.of(heapStatsDelta.ᏑinHeap), (int64)userArenaChunkBytes);
    atomic.Xaddint64(stats.of(heapStatsDelta.Ꮡcommitted), (int64)userArenaChunkBytes);
    // Model the arena as a single large malloc.
    atomic.Xadd64(stats.of(heapStatsDelta.ᏑlargeAlloc), (int64)(~s).elemsize);
    atomic.Xadd64(stats.of(heapStatsDelta.ᏑlargeAllocCount), 1);
    Ꮡmemstats.of(mstats.ᏑheapStats).release();
    // Count the alloc in inconsistent, internal stats.
    ᏑgcController.of(gcControllerState.ᏑtotalAlloc).Add((int64)(~s).elemsize);
    // Update heapLive.
    ᏑgcController.update((int64)(~s).elemsize, 0);
    // This must clear the entire heap bitmap so that it's safe
    // to allocate noscan data without writing anything out.
    s.initHeapBits(true);
    // Clear the span preemptively. It's an arena chunk, so let's assume
    // everything is going to be used.
    //
    // This also seems to make a massive difference as to whether or
    // not Linux decides to back this memory with transparent huge
    // pages. There's latency involved in this zeroing, but the hugepage
    // gains are almost always worth it. Note: it's important that we
    // clear even if it's freshly mapped and we know there's no point
    // to zeroing as *that* is the critical signal to use huge pages.
    memclrNoHeapPointers((@unsafe.Pointer)s.@base(), (~s).elemsize);
    s.Value.needzero = 0;
    s.Value.freeIndexForScan = 1;
    // Set up the range for allocation.
    s.Value.userArenaChunkFree = makeAddrRange(@base, @base + (~s).elemsize);
    // Put the large span in the mcentral swept list so that it's
    // visible to the background sweeper.
    h.central[spc].mcentral.fullSwept(h.sweepgen).push(s);
    // Set up an allocation header. Avoid write barriers here because this type
    // is not a real type, and it exists in an invalid location.
    (s.of(mspan.ᏑlargeType).Reinterpret<ж<_type>, uintptr>()).Value = (uintptr)(@unsafe.Pointer)(~s).limit;
    ((~s).largeType.of(_type.ᏑGCData).Reinterpret<ж<byte>, uintptr>()).Value = (~s).limit + /* unsafe.Sizeof(_type{}) */ (uintptr)48;
    s.Value.largeType.Value.PtrBytes = 0;
    s.Value.largeType.Value.Size_ = s.Value.elemsize;
    return s;
}

} // end runtime_package
