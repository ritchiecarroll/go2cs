// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cpu = @internal.cpu_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// A spanSet is a set of *mspans.
//
// spanSet is safe for concurrent push and pop operations.
[GoType] partial struct spanSet {
// A spanSet is a two-level data structure consisting of a
// growable spine that points to fixed-sized blocks. The spine
// can be accessed without locks, but adding a block or
// growing it requires taking the spine lock.
//
// Because each mspan covers at least 8K of heap and takes at
// most 8 bytes in the spanSet, the growth of the spine is
// quite limited.
//
// The spine and all blocks are allocated off-heap, which
// allows this to be used in the memory manager and avoids the
// need for write barriers on all of these. spanSetBlocks are
// managed in a pool, though never freed back to the operating
// system. We never release spine memory because there could be
// concurrent lock-free access and we're likely to reuse it
// anyway. (In principle, we could do this during STW.)
    internal mutex spineLock;
    internal atomicSpanSetSpinePointer spine; // *[N]atomic.Pointer[spanSetBlock]
    internal atomic.Uintptr spineLen;            // Spine array length
    internal uintptr spineCap;                   // Spine array cap, accessed under spineLock
    // index is the head and tail of the spanSet in a single field.
    // The head and the tail both represent an index into the logical
    // concatenation of all blocks, with the head always behind or
    // equal to the tail (indicating an empty set). This field is
    // always accessed atomically.
    //
    // The head and the tail are only 32 bits wide, which means we
    // can only support up to 2^32 pushes before a reset. If every
    // span in the heap were stored in this set, and each span were
    // the minimum size (1 runtime page, 8 KiB), then roughly the
    // smallest heap which would be unrepresentable is 32 TiB in size.
    internal atomicHeadTailIndex index;
}

internal static UntypedInt spanSetBlockEntries => 512; // 4KB on 64-bit
internal static UntypedInt spanSetInitSpineCap => 256; // Enough for 1GB heap on 64-bit

[GoType] partial struct spanSetBlock {
    // Free spanSetBlocks are managed via a lock-free stack.
    internal partial ref lfnode lfnode { get; }
    // popped is the number of pop operations that have occurred on
    // this block. This number is used to help determine when a block
    // may be safely recycled.
    internal atomic.Uint32 popped;
    // spans is the set of spans in this block.
    internal array<atomicMSpanPointer> spans = new(spanSetBlockEntries);
}

// push adds span s to buffer b. push is safe to call concurrently
// with other push and pop operations.
internal static void push(this ж<spanSet> Ꮡb, ж<mspan> Ꮡs) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Obtain our slot.
    var cursor = (uintptr)(Ꮡb.of(spanSet.Ꮡindex).incTail().tail() - 1);
    var (top, bottom) = (cursor / (uintptr)spanSetBlockEntries, cursor % (uintptr)spanSetBlockEntries);
    // Do we need to add a block?
    var spineLen = Ꮡb.of(spanSet.ᏑspineLen).Load();
    ж<spanSetBlock> block = default!;
retry:
    if (top < spineLen){
        block = Ꮡb.of(spanSet.Ꮡspine).Load().lookup(top).Load();
    } else {
        // Add a new block to the spine, potentially growing
        // the spine.
        @lock(Ꮡb.of(spanSet.ᏑspineLock));
        // spineLen cannot change until we release the lock,
        // but may have changed while we were waiting.
        spineLen = Ꮡb.of(spanSet.ᏑspineLen).Load();
        if (top < spineLen) {
            unlock(Ꮡb.of(spanSet.ᏑspineLock));
            goto retry;
        }
        var spine = Ꮡb.of(spanSet.Ꮡspine).Load();
        if (spineLen == b.spineCap) {
            // Grow the spine.
            var newCap = b.spineCap * 2;
            if (newCap == 0) {
                newCap = spanSetInitSpineCap;
            }
            @unsafe.Pointer newSpine = (uintptr)persistentalloc(newCap * (uintptr)goarch.PtrSize, cpu.CacheLineSize, Ꮡmemstats.of(mstats.ᏑgcMiscSys));
            if (b.spineCap != 0) {
                // Blocks are allocated off-heap, so
                // no write barriers.
                memmove(newSpine, spine.p, b.spineCap * (uintptr)goarch.PtrSize);
            }
            spine = new spanSetSpinePointer(newSpine.Value);
            // Spine is allocated off-heap, so no write barrier.
            Ꮡb.of(spanSet.Ꮡspine).StoreNoWB(spine);
            b.spineCap = newCap;
        }
        // We can't immediately free the old spine
        // since a concurrent push with a lower index
        // could still be reading from it. We let it
        // leak because even a 1TB heap would waste
        // less than 2MB of memory on old spines. If
        // this is a problem, we could free old spines
        // during STW.
        // Allocate a new block from the pool.
        block = ᏑspanSetBlockPool.alloc();
        // Add it to the spine.
        // Blocks are allocated off-heap, so no write barrier.
        spine.lookup(top).StoreNoWB(block);
        Ꮡb.of(spanSet.ᏑspineLen).Store(spineLen + 1);
        unlock(Ꮡb.of(spanSet.ᏑspineLock));
    }
    // We have a block. Insert the span atomically, since there may be
    // concurrent readers via the block API.
    block.at(spanSetBlock.Ꮡspans, (ulong)(bottom)).StoreNoWB(Ꮡs);
}

// pop removes and returns a span from buffer b, or nil if b is empty.
// pop is safe to call concurrently with other pop and push operations.
internal static ж<mspan> pop(this ж<spanSet> Ꮡb) {
    uint32 head = default!;
    uint32 tail = default!;
claimLoop:
    while (ᐧ) {
        var headtail = Ꮡb.of(spanSet.Ꮡindex).load();
        (head, tail) = headtail.split();
        if (head >= tail) {
            // The buf is empty, as far as we can tell.
            return default!;
        }
        // Check if the head position we want to claim is actually
        // backed by a block.
        var spineLen = Ꮡb.of(spanSet.ᏑspineLen).Load();
        if (spineLen <= (uintptr)head / (uintptr)spanSetBlockEntries) {
            // We're racing with a spine growth and the allocation of
            // a new block (and maybe a new spine!), and trying to grab
            // the span at the index which is currently being pushed.
            // Instead of spinning, let's just notify the caller that
            // there's nothing currently here. Spinning on this is
            // almost definitely not worth it.
            return default!;
        }
        // Try to claim the current head by CASing in an updated head.
        // This may fail transiently due to a push which modifies the
        // tail, so keep trying while the head isn't changing.
        var want = head;
        while (want == head) {
            if (Ꮡb.of(spanSet.Ꮡindex).cas(headtail, makeHeadTailIndex(want + 1, tail))) {
                goto break_claimLoop;
            }
            headtail = Ꮡb.of(spanSet.Ꮡindex).load();
            (head, tail) = headtail.split();
        }
continue_claimLoop:;
    }
break_claimLoop:;
    // We failed to claim the spot we were after and the head changed,
    // meaning a popper got ahead of us. Try again from the top because
    // the buf may not be empty.
    var (top, bottom) = (head / (uint32)spanSetBlockEntries, head % (uint32)spanSetBlockEntries);
    // We may be reading a stale spine pointer, but because the length
    // grows monotonically and we've already verified it, we'll definitely
    // be reading from a valid block.
    var blockp = Ꮡb.of(spanSet.Ꮡspine).Load().lookup((uintptr)top);
    // Given that the spine length is correct, we know we will never
    // see a nil block here, since the length is always updated after
    // the block is set.
    var block = blockp.Load();
    var s = block.at(spanSetBlock.Ꮡspans, (ulong)(bottom)).Load();
    while (s == nil) {
        // We raced with the span actually being set, but given that we
        // know a block for this span exists, the race window here is
        // extremely small. Try again.
        s = block.at(spanSetBlock.Ꮡspans, (ulong)(bottom)).Load();
    }
    // Clear the pointer. This isn't strictly necessary, but defensively
    // avoids accidentally re-using blocks which could lead to memory
    // corruption. This way, we'll get a nil pointer access instead.
    block.at(spanSetBlock.Ꮡspans, (ulong)(bottom)).StoreNoWB(nil);
    // Increase the popped count. If we are the last possible popper
    // in the block (note that bottom need not equal spanSetBlockEntries-1
    // due to races) then it's our responsibility to free the block.
    //
    // If we increment popped to spanSetBlockEntries, we can be sure that
    // we're the last popper for this block, and it's thus safe to free it.
    // Every other popper must have crossed this barrier (and thus finished
    // popping its corresponding mspan) by the time we get here. Because
    // we're the last popper, we also don't have to worry about concurrent
    // pushers (there can't be any). Note that we may not be the popper
    // which claimed the last slot in the block, we're just the last one
    // to finish popping.
    if (block.of(spanSetBlock.Ꮡpopped).Add(1) == spanSetBlockEntries) {
        // Clear the block's pointer.
        blockp.StoreNoWB(nil);
        // Return the block to the block pool.
        ᏑspanSetBlockPool.free(block);
    }
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string attemptToClearNonEmptyˢ = "attempt to clear non-empty span set"u8;
internal static readonly @string spanSetBlockWithUnpoppedˢ = "span set block with unpopped elements found in reset"u8;
internal static readonly @string fullyEmptyUnfreedSpanSetˢ = "fully empty unfreed span set block found in reset"u8;

// reset resets a spanSet which is empty. It will also clean up
// any left over blocks.
//
// Throws if the buf is not empty.
//
// reset may not be called concurrently with any other operations
// on the span set.
internal static void reset(this ж<spanSet> Ꮡb) {
    var (head, tail) = Ꮡb.of(spanSet.Ꮡindex).load().split();
    if (head < tail) {
        print((@string)"head = "u8, head, (@string)", tail = "u8, tail, (@string)"\n"u8);
        @throw(attemptToClearNonEmptyˢ);
    }
    var top = head / (uint32)spanSetBlockEntries;
    if ((uintptr)top < Ꮡb.of(spanSet.ᏑspineLen).Load()) {
        // If the head catches up to the tail and the set is empty,
        // we may not clean up the block containing the head and tail
        // since it may be pushed into again. In order to avoid leaking
        // memory since we're going to reset the head and tail, clean
        // up such a block now, if it exists.
        var blockp = Ꮡb.of(spanSet.Ꮡspine).Load().lookup((uintptr)top);
        var block = blockp.Load();
        if (block != nil) {
            // Check the popped value.
            if (block.of(spanSetBlock.Ꮡpopped).Load() == 0) {
                // popped should never be zero because that means we have
                // pushed at least one value but not yet popped if this
                // block pointer is not nil.
                @throw(spanSetBlockWithUnpoppedˢ);
            }
            if (block.of(spanSetBlock.Ꮡpopped).Load() == spanSetBlockEntries) {
                // popped should also never be equal to spanSetBlockEntries
                // because the last popper should have made the block pointer
                // in this slot nil.
                @throw(fullyEmptyUnfreedSpanSetˢ);
            }
            // Clear the pointer to the block.
            blockp.StoreNoWB(nil);
            // Return the block to the block pool.
            ᏑspanSetBlockPool.free(block);
        }
    }
    Ꮡb.of(spanSet.Ꮡindex).reset();
    Ꮡb.of(spanSet.ᏑspineLen).Store(0);
}

// atomicSpanSetSpinePointer is an atomically-accessed spanSetSpinePointer.
//
// It has the same semantics as atomic.UnsafePointer.
[GoType] partial struct atomicSpanSetSpinePointer {
    internal atomic.UnsafePointer a;
}

// Loads the spanSetSpinePointer and returns it.
//
// It has the same semantics as atomic.UnsafePointer.
internal static spanSetSpinePointer Load(this ж<atomicSpanSetSpinePointer> Ꮡs) {
    return new spanSetSpinePointer((uintptr)Ꮡs.of(atomicSpanSetSpinePointer.Ꮡa).Load());
}

// Stores the spanSetSpinePointer.
//
// It has the same semantics as [atomic.UnsafePointer].
internal static void StoreNoWB(this ж<atomicSpanSetSpinePointer> Ꮡs, spanSetSpinePointer Δp) {
    Ꮡs.of(atomicSpanSetSpinePointer.Ꮡa).StoreNoWB(Δp.p);
}

// spanSetSpinePointer represents a pointer to a contiguous block of atomic.Pointer[spanSetBlock].
[GoType] partial struct spanSetSpinePointer {
    internal @unsafe.Pointer p;
}

// lookup returns &s[idx].
internal static ж<atomic.Pointer<spanSetBlock>> lookup(this spanSetSpinePointer s, uintptr idx) {
    return (ж<atomic.Pointer<spanSetBlock>>)(uintptr)(add(s.p, (uintptr)goarch.PtrSize * idx));
}

// spanSetBlockPool is a global pool of spanSetBlocks.
internal static ж<spanSetBlockAlloc> ᏑspanSetBlockPool = new StandardBox<spanSetBlockAlloc>(default(spanSetBlockAlloc));
internal static ref spanSetBlockAlloc spanSetBlockPool => ref ᏑspanSetBlockPool.Value;

// spanSetBlockAlloc represents a concurrent pool of spanSetBlocks.
[GoType] partial struct spanSetBlockAlloc {
    internal lfstack stack;
}

// alloc tries to grab a spanSetBlock out of the pool, and if it fails
// persistentallocs a new one and returns it.
internal static ж<spanSetBlock> alloc(this ж<spanSetBlockAlloc> Ꮡp) {
    {
        var s = (ж<spanSetBlock>)(uintptr)(Ꮡp.of(spanSetBlockAlloc.Ꮡstack).pop()); if (s != nil) {
            return s;
        }
    }
    return (ж<spanSetBlock>)(uintptr)(persistentalloc(/* unsafe.Sizeof(spanSetBlock{}) */ (uintptr)4120, cpu.CacheLineSize, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
}

// free returns a spanSetBlock back to the pool.
internal static void free(this ж<spanSetBlockAlloc> Ꮡp, ж<spanSetBlock> Ꮡblock) {
    Ꮡblock.of(spanSetBlock.Ꮡpopped).Store(0);
    Ꮡp.of(spanSetBlockAlloc.Ꮡstack).push(Ꮡblock.of(spanSetBlock.Ꮡlfnode));
}

[GoType("num:uint64")] partial struct headTailIndex;

// makeHeadTailIndex creates a headTailIndex value from a separate
// head and tail.
internal static headTailIndex makeHeadTailIndex(uint32 head, uint32 tail) {
    return ((headTailIndex)((uint64)(((uint64)head << (int)(32)) | (uint64)tail)));
}

// head returns the head of a headTailIndex value.
internal static uint32 head(this headTailIndex h) {
    return (uint32)(uint64)((h >> (int)(32)));
}

// tail returns the tail of a headTailIndex value.
internal static uint32 tail(this headTailIndex h) {
    return (uint32)(uint64)h;
}

// split splits the headTailIndex value into its parts.
internal static (uint32 head, uint32 tail) split(this headTailIndex h) {
    return (h.head(), h.tail());
}

// atomicHeadTailIndex is an atomically-accessed headTailIndex.
[GoType] partial struct atomicHeadTailIndex {
    internal atomic.Uint64 u;
}

// load atomically reads a headTailIndex value.
internal static headTailIndex load(this ж<atomicHeadTailIndex> Ꮡh) {
    return ((headTailIndex)Ꮡh.of(atomicHeadTailIndex.Ꮡu).Load());
}

// cas atomically compares-and-swaps a headTailIndex value.
internal static bool cas(this ж<atomicHeadTailIndex> Ꮡh, headTailIndex old, headTailIndex @new) {
    return Ꮡh.of(atomicHeadTailIndex.Ꮡu).CompareAndSwap((uint64)old, (uint64)@new);
}

// incHead atomically increments the head of a headTailIndex.
internal static headTailIndex incHead(this ж<atomicHeadTailIndex> Ꮡh) {
    return ((headTailIndex)Ꮡh.of(atomicHeadTailIndex.Ꮡu).Add(4294967296L));
}

// decHead atomically decrements the head of a headTailIndex.
internal static headTailIndex decHead(this ж<atomicHeadTailIndex> Ꮡh) {
    return ((headTailIndex)Ꮡh.of(atomicHeadTailIndex.Ꮡu).Add(-(4294967296L)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string headTailIndexOverflowˢ = "headTailIndex overflow"u8;

// incTail atomically increments the tail of a headTailIndex.
internal static headTailIndex incTail(this ж<atomicHeadTailIndex> Ꮡh) {
    var ht = ((headTailIndex)Ꮡh.of(atomicHeadTailIndex.Ꮡu).Add(1));
    // Check for overflow.
    if (ht.tail() == 0) {
        print((@string)"runtime: head = "u8, ht.head(), (@string)", tail = "u8, ht.tail(), (@string)"\n"u8);
        @throw(headTailIndexOverflowˢ);
    }
    return ht;
}

// reset clears the headTailIndex to (0, 0).
internal static void reset(this ж<atomicHeadTailIndex> Ꮡh) {
    Ꮡh.of(atomicHeadTailIndex.Ꮡu).Store(0);
}

// atomicMSpanPointer is an atomic.Pointer[mspan]. Can't use generics because it's NotInHeap.
[GoType] partial struct atomicMSpanPointer {
    internal atomic.UnsafePointer p;
}

// Load returns the *mspan.
internal static ж<mspan> Load(this ж<atomicMSpanPointer> Ꮡp) {
    return (ж<mspan>)(uintptr)(Ꮡp.of(atomicMSpanPointer.Ꮡp).Load());
}

// Store stores an *mspan.
internal static void StoreNoWB(this ж<atomicMSpanPointer> Ꮡp, ж<mspan> Ꮡs) {
    Ꮡp.of(atomicMSpanPointer.Ꮡp).StoreNoWB(@unsafe.Pointer.FromPinnedBox(Ꮡs));
}

} // end runtime_package
