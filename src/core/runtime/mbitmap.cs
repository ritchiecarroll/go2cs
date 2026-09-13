// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Garbage collector: type and heap bitmaps.
//
// Stack, data, and bss bitmaps
//
// Stack frames and global variables in the data and bss sections are
// described by bitmaps with 1 bit per pointer-sized word. A "1" bit
// means the word is a live pointer to be visited by the GC (referred to
// as "pointer"). A "0" bit means the word should be ignored by GC
// (referred to as "scalar", though it could be a dead pointer value).
//
// Heap bitmaps
//
// The heap bitmap comprises 1 bit for each pointer-sized word in the heap,
// recording whether a pointer is stored in that word or not. This bitmap
// is stored at the end of a span for small objects and is unrolled at
// runtime from type metadata for all larger objects. Objects without
// pointers have neither a bitmap nor associated type metadata.
//
// Bits in all cases correspond to words in little-endian order.
//
// For small objects, if s is the mspan for the span starting at "start",
// then s.heapBits() returns a slice containing the bitmap for the whole span.
// That is, s.heapBits()[0] holds the goarch.PtrSize*8 bits for the first
// goarch.PtrSize*8 words from "start" through "start+63*ptrSize" in the span.
// On a related note, small objects are always small enough that their bitmap
// fits in goarch.PtrSize*8 bits, so writing out bitmap data takes two bitmap
// writes at most (because object boundaries don't generally lie on
// s.heapBits()[i] boundaries).
//
// For larger objects, if t is the type for the object starting at "start",
// within some span whose mspan is s, then the bitmap at t.GCData is "tiled"
// from "start" through "start+s.elemsize".
// Specifically, the first bit of t.GCData corresponds to the word at "start",
// the second to the word after "start", and so on up to t.PtrBytes. At t.PtrBytes,
// we skip to "start+t.Size_" and begin again from there. This process is
// repeated until we hit "start+s.elemsize".
// This tiling algorithm supports array data, since the type always refers to
// the element type of the array. Single objects are considered the same as
// single-element arrays.
// The tiling algorithm may scan data past the end of the compiler-recognized
// object, but any unused data within the allocation slot (i.e. within s.elemsize)
// is zeroed, so the GC just observes nil pointers.
// Note that this "tiled" bitmap isn't stored anywhere; it is generated on-the-fly.
//
// For objects without their own span, the type metadata is stored in the first
// word before the object at the beginning of the allocation slot. For objects
// with their own span, the type metadata is stored in the mspan.
//
// The bitmap for small unallocated objects in scannable spans is not maintained
// (can be junk).
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

internal static UntypedInt mallocHeaderSize => 8;
internal static UntypedInt minSizeForMallocHeader => /* goarch.PtrSize * ptrBits */ 512;

// heapBitsInSpan returns true if the size of an object implies its ptr/scalar
// data is stored at the end of the span, and is accessible via span.heapBits.
//
// Note: this works for both rounded-up sizes (span.elemsize) and unrounded
// type sizes because minSizeForMallocHeader is guaranteed to be at a size
// class boundary.
//
//go:nosplit
internal static bool heapBitsInSpan(uintptr userSize) {
    // N.B. minSizeForMallocHeader is an exclusive minimum so that this function is
    // invariant under size-class rounding on its input.
    return userSize <= minSizeForMallocHeader;
}

// typePointers is an iterator over the pointers in a heap object.
//
// Iteration through this type implements the tiling algorithm described at the
// top of this file.
[GoType] partial struct typePointers {
    // elem is the address of the current array element of type typ being iterated over.
    // Objects that are not arrays are treated as single-element arrays, in which case
    // this value does not change.
    internal uintptr elem;
    // addr is the address the iterator is currently working from and describes
    // the address of the first word referenced by mask.
    internal uintptr addr;
    // mask is a bitmask where each bit corresponds to pointer-words after addr.
    // Bit 0 is the pointer-word at addr, Bit 1 is the next word, and so on.
    // If a bit is 1, then there is a pointer at that word.
    // nextFast and next mask out bits in this mask as their pointers are processed.
    internal uintptr mask;
    // typ is a pointer to the type information for the heap object's type.
    // This may be nil if the object is in a span where heapBitsInSpan(span.elemsize) is true.
    internal ж<_type> typ;
}

// typePointersOf returns an iterator over all heap pointers in the range [addr, addr+size).
//
// addr and addr+size must be in the range [span.base(), span.limit).
//
// Note: addr+size must be passed as the limit argument to the iterator's next method on
// each iteration. This slightly awkward API is to allow typePointers to be destructured
// by the compiler.
//
// nosplit because it is used during write barriers and must not be preempted.
//
//go:nosplit
[GoRecv] internal static typePointers typePointersOf(this ref mspan span, uintptr addr, uintptr size) {
    var @base = span.objBase(addr);
    var tp = span.typePointersOfUnchecked(@base);
    if (@base == addr && size == span.elemsize) {
        return tp;
    }
    return tp.fastForward(addr - tp.addr, addr + size);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string typePointersOfUncheckedˢ = "typePointersOfUnchecked consisting of non-base-address for object"u8;

// typePointersOfUnchecked is like typePointersOf, but assumes addr is the base
// of an allocation slot in a span (the start of the object if no header, the
// header otherwise). It returns an iterator that generates all pointers
// in the range [addr, addr+span.elemsize).
//
// nosplit because it is used during write barriers and must not be preempted.
//
//go:nosplit
[GoRecv] internal static typePointers typePointersOfUnchecked(this ref mspan span, uintptr addr) {
    const bool doubleCheck = false;
    if (doubleCheck && span.objBase(addr) != addr) {
        print((@string)"runtime: addr="u8, addr, (@string)" base="u8, span.objBase(addr), (@string)"\n"u8);
        @throw(typePointersOfUncheckedˢ);
    }
    var spc = span.spanclass;
    if (spc.noscan()) {
        return new typePointers(nil);
    }
    if (heapBitsInSpan(span.elemsize)) {
        // Handle header-less objects.
        return new typePointers(elem: addr, addr: addr, mask: span.heapBitsSmallForAddr(addr));
    }
    // All of these objects have a header.
    ж<_type> typ = default!;
    if (spc.sizeclass() != 0){
        // Pull the allocation header from the first word of the object.
        typ = ~(ж<ж<_type>>)(uintptr)((@unsafe.Pointer)addr);
        addr += mallocHeaderSize;
    } else {
        typ = span.largeType;
        if (typ == nil) {
            // Allow a nil type here for delayed zeroing. See mallocgc.
            return new typePointers(nil);
        }
    }
    var gcdata = typ.Value.GCData;
    return new typePointers(elem: addr, addr: addr, mask: readUintptr(gcdata), typ: typ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badTypePassedToˢ = "bad type passed to typePointersOfType"u8;

// typePointersOfType is like typePointersOf, but assumes addr points to one or more
// contiguous instances of the provided type. The provided type must not be nil and
// it must not have its type metadata encoded as a gcprog.
//
// It returns an iterator that tiles typ.GCData starting from addr. It's the caller's
// responsibility to limit iteration.
//
// nosplit because its callers are nosplit and require all their callees to be nosplit.
//
//go:nosplit
[GoRecv] internal static typePointers typePointersOfType(this ref mspan span, ж<abi.Type> Ꮡtyp, uintptr addr) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    const bool doubleCheck = false;
    if (doubleCheck && (Ꮡtyp == nil || (abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0)) {
        @throw(badTypePassedToˢ);
    }
    if (span.spanclass.noscan()) {
        return new typePointers(nil);
    }
    // Since we have the type, pretend we have a header.
    var gcdata = typ.GCData;
    return new typePointers(elem: addr, addr: addr, mask: readUintptr(gcdata), typ: Ꮡtyp);
}

// nextFast is the fast path of next. nextFast is written to be inlineable and,
// as the name implies, fast.
//
// Callers that are performance-critical should iterate using the following
// pattern:
//
//	for {
//		var addr uintptr
//		if tp, addr = tp.nextFast(); addr == 0 {
//			if tp, addr = tp.next(limit); addr == 0 {
//				break
//			}
//		}
//		// Use addr.
//		...
//	}
//
// nosplit because it is used during write barriers and must not be preempted.
//
//go:nosplit
internal static (typePointers, uintptr) nextFast(this typePointers tp) {
    // TESTQ/JEQ
    if (tp.mask == 0) {
        return (tp, 0);
    }
    // BSFQ
    nint i = default!;
    if (goarch.PtrSize == 8){
        i = sys.TrailingZeros64((uint64)tp.mask);
    } else {
        i = sys.TrailingZeros32((uint32)tp.mask);
    }
    // BTCQ
    tp.mask ^= (uintptr)(((uintptr)1 << (int)(((nint)(i & (nint)(ptrBits - 1))))));
    // LEAQ (XX)(XX*8)
    return (tp, tp.addr + (uintptr)i * (uintptr)goarch.PtrSize);
}

// next advances the pointers iterator, returning the updated iterator and
// the address of the next pointer.
//
// limit must be the same each time it is passed to next.
//
// nosplit because it is used during write barriers and must not be preempted.
//
//go:nosplit
internal static (typePointers, uintptr) next(this typePointers tp, uintptr limit) {
    while (ᐧ) {
        if (tp.mask != 0) {
            return tp.nextFast();
        }
        // Stop if we don't actually have type information.
        if (tp.typ == nil) {
            return (new typePointers(nil), 0);
        }
        // Advance to the next element if necessary.
        if (tp.addr + (uintptr)(goarch.PtrSize * ptrBits) >= tp.elem + (~tp.typ).PtrBytes){
            tp.elem += tp.typ.Value.Size_;
            tp.addr = tp.elem;
        } else {
            tp.addr += ptrBits * goarch.PtrSize;
        }
        // Check if we've exceeded the limit with the last update.
        if (tp.addr >= limit) {
            return (new typePointers(nil), 0);
        }
        // Grab more bits and try again.
        tp.mask = readUintptr(addb((~tp.typ).GCData, (tp.addr - tp.elem) / (uintptr)goarch.PtrSize / 8));
        if (tp.addr + (uintptr)(goarch.PtrSize * ptrBits) > limit) {
            var bits = (tp.addr + (uintptr)(goarch.PtrSize * ptrBits) - limit) / (uintptr)goarch.PtrSize;
            tp.mask &= unchecked((uintptr)~(uintptr)(((((uintptr)1).Lsh((uint64)((bits)))) - 1).Lsh((uint64)(((uintptr)ptrBits - bits)))));
        }
    }
}

// fastForward moves the iterator forward by n bytes. n must be a multiple
// of goarch.PtrSize. limit must be the same limit passed to next for this
// iterator.
//
// nosplit because it is used during write barriers and must not be preempted.
//
//go:nosplit
internal static typePointers fastForward(this typePointers tp, uintptr n, uintptr limit) {
    // Basic bounds check.
    var target = tp.addr + n;
    if (target >= limit) {
        return new typePointers(nil);
    }
    if (tp.typ == nil) {
        // Handle small objects.
        // Clear any bits before the target address.
        tp.mask &= unchecked((uintptr)~(uintptr)((((uintptr)1).Lsh((uint64)(((target - tp.addr) / (uintptr)goarch.PtrSize)))) - 1));
        // Clear any bits past the limit.
        if (tp.addr + (uintptr)(goarch.PtrSize * ptrBits) > limit) {
            var bits = (tp.addr + (uintptr)(goarch.PtrSize * ptrBits) - limit) / (uintptr)goarch.PtrSize;
            tp.mask &= unchecked((uintptr)~(uintptr)(((((uintptr)1).Lsh((uint64)((bits)))) - 1).Lsh((uint64)(((uintptr)ptrBits - bits)))));
        }
        return tp;
    }
    // Move up elem and addr.
    // Offsets within an element are always at a ptrBits*goarch.PtrSize boundary.
    if (n >= (~tp.typ).Size_){
        // elem needs to be moved to the element containing
        // tp.addr + n.
        var oldelem = tp.elem;
        tp.elem += (tp.addr - tp.elem + n) / (~tp.typ).Size_ * (~tp.typ).Size_;
        tp.addr = tp.elem + alignDown(n - (tp.elem - oldelem), ptrBits * goarch.PtrSize);
    } else {
        tp.addr += alignDown(n, ptrBits * goarch.PtrSize);
    }
    if (tp.addr - tp.elem >= (~tp.typ).PtrBytes){
        // We're starting in the non-pointer area of an array.
        // Move up to the next element.
        tp.elem += tp.typ.Value.Size_;
        tp.addr = tp.elem;
        tp.mask = readUintptr((~tp.typ).GCData);
        // We may have exceeded the limit after this. Bail just like next does.
        if (tp.addr >= limit) {
            return new typePointers(nil);
        }
    } else {
        // Grab the mask, but then clear any bits before the target address and any
        // bits over the limit.
        tp.mask = readUintptr(addb((~tp.typ).GCData, (tp.addr - tp.elem) / (uintptr)goarch.PtrSize / 8));
        tp.mask &= unchecked((uintptr)~(uintptr)((((uintptr)1).Lsh((uint64)(((target - tp.addr) / (uintptr)goarch.PtrSize)))) - 1));
    }
    if (tp.addr + (uintptr)(goarch.PtrSize * ptrBits) > limit) {
        var bits = (tp.addr + (uintptr)(goarch.PtrSize * ptrBits) - limit) / (uintptr)goarch.PtrSize;
        tp.mask &= unchecked((uintptr)~(uintptr)(((((uintptr)1).Lsh((uint64)((bits)))) - 1).Lsh((uint64)(((uintptr)ptrBits - bits)))));
    }
    return tp;
}

// objBase returns the base pointer for the object containing addr in span.
//
// Assumes that addr points into a valid part of span (span.base() <= addr < span.limit).
//
//go:nosplit
[GoRecv] internal static uintptr objBase(this ref mspan span, uintptr addr) {
    return span.@base() + span.objIndex(addr) * span.elemsize;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bulkBarrierPreWriteˢ = "bulkBarrierPreWrite: unaligned arguments"u8;

// bulkBarrierPreWrite executes a write barrier
// for every pointer slot in the memory range [src, src+size),
// using pointer/scalar information from [dst, dst+size).
// This executes the write barriers necessary before a memmove.
// src, dst, and size must be pointer-aligned.
// The range [dst, dst+size) must lie within a single object.
// It does not perform the actual writes.
//
// As a special case, src == 0 indicates that this is being used for a
// memclr. bulkBarrierPreWrite will pass 0 for the src of each write
// barrier.
//
// Callers should call bulkBarrierPreWrite immediately before
// calling memmove(dst, src, size). This function is marked nosplit
// to avoid being preempted; the GC must not stop the goroutine
// between the memmove and the execution of the barriers.
// The caller is also responsible for cgo pointer checks if this
// may be writing Go pointers into non-Go memory.
//
// Pointer data is not maintained for allocations containing
// no pointers at all; any caller of bulkBarrierPreWrite must first
// make sure the underlying allocation contains pointers, usually
// by checking typ.PtrBytes.
//
// The typ argument is the type of the space at src and dst (and the
// element type if src and dst refer to arrays) and it is optional.
// If typ is nil, the barrier will still behave as expected and typ
// is used purely as an optimization. However, it must be used with
// care.
//
// If typ is not nil, then src and dst must point to one or more values
// of type typ. The caller must ensure that the ranges [src, src+size)
// and [dst, dst+size) refer to one or more whole values of type src and
// dst (leaving off the pointerless tail of the space is OK). If this
// precondition is not followed, this function will fail to scan the
// right pointers.
//
// When in doubt, pass nil for typ. That is safe and will always work.
//
// Callers must perform cgo checks if goexperiment.CgoCheck2.
//
//go:nosplit
internal static void bulkBarrierPreWrite(uintptr dst, uintptr src, uintptr size, ж<abi.Type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if ((uintptr)(((uintptr)((uintptr)(dst | src) | size)) & (uintptr)(goarch.PtrSize - 1)) != 0) {
        @throw(bulkBarrierPreWriteˢ);
    }
    if (!writeBarrier.enabled) {
        return;
    }
    var s = spanOf(dst);
    if (s == nil){
        // If dst is a global, use the data or BSS bitmaps to
        // execute write barriers.
        foreach (var (_, datap) in activeModules()) {
            if ((~datap).data <= dst && dst < (~datap).edata) {
                bulkBarrierBitmap(dst, src, size, dst - (~datap).data, (~datap).gcdatamask.bytedata);
                return;
            }
        }
        foreach (var (_, datap) in activeModules()) {
            if ((~datap).bss <= dst && dst < (~datap).ebss) {
                bulkBarrierBitmap(dst, src, size, dst - (~datap).bss, (~datap).gcbssmask.bytedata);
                return;
            }
        }
        return;
    } else 
    if (s.of(mspan.Ꮡstate).get() != mSpanInUse || dst < s.@base() || (~s).limit <= dst) {
        // dst was heap memory at some point, but isn't now.
        // It can't be a global. It must be either our stack,
        // or in the case of direct channel sends, it could be
        // another stack. Either way, no need for barriers.
        // This will also catch if dst is in a freed span,
        // though that should never have.
        return;
    }
    var buf = (~(~getg()).m).p.ptr().of(runtime_package.Δp.ᏑwbBuf);
    // Double-check that the bitmaps generated in the two possible paths match.
    const bool doubleCheck = false;
    if (doubleCheck) {
        doubleCheckTypePointersOfType(s, Ꮡtyp, dst, size);
    }
    typePointers tp = default!;
    if (Ꮡtyp != nil && (abiꓸKind)(typ.Kind_ & abi.KindGCProg) == 0){
        tp = s.typePointersOfType(Ꮡtyp, dst);
    } else {
        tp = s.typePointersOf(dst, size);
    }
    if (src == 0){
        while (ᐧ) {
            uintptr addr = default!;
            {
                (tp, addr) = tp.next(dst + size); if (addr == 0) {
                    break;
                }
            }
            var dstx = (ж<uintptr>)(uintptr)((@unsafe.Pointer)addr);
            var Δp = buf.get1();
            Δp.Value[0] = dstx.Value;
        }
    } else {
        while (ᐧ) {
            uintptr addr = default!;
            {
                (tp, addr) = tp.next(dst + size); if (addr == 0) {
                    break;
                }
            }
            var dstx = (ж<uintptr>)(uintptr)((@unsafe.Pointer)addr);
            var srcx = (ж<uintptr>)(uintptr)((@unsafe.Pointer)(src + (addr - dst)));
            var Δp = buf.get2();
            Δp.Value[0] = dstx.Value;
            Δp.Value[1] = srcx.Value;
        }
    }
}

// bulkBarrierPreWriteSrcOnly is like bulkBarrierPreWrite but
// does not execute write barriers for [dst, dst+size).
//
// In addition to the requirements of bulkBarrierPreWrite
// callers need to ensure [dst, dst+size) is zeroed.
//
// This is used for special cases where e.g. dst was just
// created and zeroed with malloc.
//
// The type of the space can be provided purely as an optimization.
// See bulkBarrierPreWrite's comment for more details -- use this
// optimization with great care.
//
//go:nosplit
internal static void bulkBarrierPreWriteSrcOnly(uintptr dst, uintptr src, uintptr size, ж<abi.Type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if ((uintptr)(((uintptr)((uintptr)(dst | src) | size)) & (uintptr)(goarch.PtrSize - 1)) != 0) {
        @throw(bulkBarrierPreWriteˢ);
    }
    if (!writeBarrier.enabled) {
        return;
    }
    var buf = (~(~getg()).m).p.ptr().of(runtime_package.Δp.ᏑwbBuf);
    var s = spanOf(dst);
    // Double-check that the bitmaps generated in the two possible paths match.
    const bool doubleCheck = false;
    if (doubleCheck) {
        doubleCheckTypePointersOfType(s, Ꮡtyp, dst, size);
    }
    typePointers tp = default!;
    if (Ꮡtyp != nil && (abiꓸKind)(typ.Kind_ & abi.KindGCProg) == 0){
        tp = s.typePointersOfType(Ꮡtyp, dst);
    } else {
        tp = s.typePointersOf(dst, size);
    }
    while (ᐧ) {
        uintptr addr = default!;
        {
            (tp, addr) = tp.next(dst + size); if (addr == 0) {
                break;
            }
        }
        var srcx = (ж<uintptr>)(uintptr)((@unsafe.Pointer)(addr - dst + src));
        var Δp = buf.get1();
        Δp.Value[0] = srcx.Value;
    }
}

// initHeapBits initializes the heap bitmap for a span.
//
// TODO(mknyszek): This should set the heap bits for single pointer
// allocations eagerly to avoid calling heapSetType at allocation time,
// just to write one bit.
[GoRecv] internal static void initHeapBits(this ref mspan s, bool forceClear) {
    if ((!s.spanclass.noscan() && heapBitsInSpan(s.elemsize)) || s.isUserArenaChunk) {
        var b = s.heapBits();
        builtin.clear(b);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string heapBitsCalledForNoscanˢ = "heapBits called for noscan"u8;
internal static readonly @string heapBitsCalledForSpanˢ = "heapBits called for span class that should have a malloc header"u8;

// heapBits returns the heap ptr/scalar bits stored at the end of the span for
// small object spans and heap arena spans.
//
// Note that the uintptr of each element means something different for small object
// spans and for heap arena spans. Small object spans are easy: they're never interpreted
// as anything but uintptr, so they're immune to differences in endianness. However, the
// heapBits for user arena spans is exposed through a dummy type descriptor, so the byte
// ordering needs to match the same byte ordering the compiler would emit. The compiler always
// emits the bitmap data in little endian byte ordering, so on big endian platforms these
// uintptrs will have their byte orders swapped from what they normally would be.
//
// heapBitsInSpan(span.elemsize) or span.isUserArenaChunk must be true.
//
//go:nosplit
[GoRecv] internal static slice<uintptr> heapBits(this ref mspan span) {
    const bool doubleCheck = false;
    if (doubleCheck && !span.isUserArenaChunk) {
        if (span.spanclass.noscan()) {
            @throw(heapBitsCalledForNoscanˢ);
        }
        if (span.elemsize > minSizeForMallocHeader) {
            @throw(heapBitsCalledForSpanˢ);
        }
    }
    // Find the bitmap at the end of the span.
    //
    // Nearly every span with heap bits is exactly one page in size. Arenas are the only exception.
    if (span.npages == 1) {
        // This will be inlined and constant-folded down.
        return heapBitsSlice(span.@base(), pageSize);
    }
    return heapBitsSlice(span.@base(), span.npages * (uintptr)pageSize);
}

// Helper for constructing a slice for the span's heap bits.
//
//go:nosplit
internal static slice<uintptr> heapBitsSlice(uintptr spanBase, uintptr spanSize) {
    var bitmapSize = spanSize / (uintptr)goarch.PtrSize / 8;
    nint elems = (nint)(bitmapSize / (uintptr)goarch.PtrSize);
    ref var sl = ref heap(new notInHeapSlice(), out var Ꮡsl);
    sl = new notInHeapSlice((ж<notInHeap>)(uintptr)((@unsafe.Pointer)(spanBase + spanSize - bitmapSize)), elems, elems);
    return ~Ꮡsl.Reinterpret<notInHeapSlice, slice<uintptr>>();
}

// heapBitsSmallForAddr loads the heap bits for the object stored at addr from span.heapBits.
//
// addr must be the base pointer of an object in the span. heapBitsInSpan(span.elemsize)
// must be true.
//
//go:nosplit
[GoRecv] internal static uintptr heapBitsSmallForAddr(this ref mspan span, uintptr addr) {
    var spanSize = span.npages * (uintptr)pageSize;
    var bitmapSize = spanSize / (uintptr)goarch.PtrSize / 8;
    var hbits = (ж<byte>)(uintptr)((@unsafe.Pointer)(span.@base() + spanSize - bitmapSize));
    // These objects are always small enough that their bitmaps
    // fit in a single word, so just load the word or two we need.
    //
    // Mirrors mspan.writeHeapBitsSmall.
    //
    // We should be using heapBits(), but unfortunately it introduces
    // both bounds checks panics and throw which causes us to exceed
    // the nosplit limit in quite a few cases.
    var i = (addr - span.@base()) / (uintptr)goarch.PtrSize / (uintptr)ptrBits;
    var j = (addr - span.@base()) / (uintptr)goarch.PtrSize % (uintptr)ptrBits;
    var bits = span.elemsize / (uintptr)goarch.PtrSize;
    var word0 = addb(hbits, (uintptr)goarch.PtrSize * (i + 0)).Reinterpret<byte, uintptr>();
    var word1 = addb(hbits, (uintptr)goarch.PtrSize * (i + 1)).Reinterpret<byte, uintptr>();
    uintptr read = default!;
    if (j + bits > ptrBits){
        // Two reads.
        var bits0 = (uintptr)ptrBits - j;
        var bits1 = bits - bits0;
        read = (word0.Value).Rsh((uint64)(j));
        read |= (uintptr)(((uintptr)(word1.Value & ((((uintptr)1).Lsh((uint64)(bits1))) - 1))).Lsh((uint64)(bits0)));
    } else {
        // One read.
        read = (uintptr)(((word0.Value).Rsh((uint64)(j))) & ((((uintptr)1).Lsh((uint64)(bits))) - 1));
    }
    return read;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badPointerBitsWrittenForˢ = "bad pointer bits written for small object"u8;

// writeHeapBitsSmall writes the heap bits for small objects whose ptr/scalar data is
// stored as a bitmap at the end of the span.
//
// Assumes dataSize is <= ptrBits*goarch.PtrSize. x must be a pointer into the span.
// heapBitsInSpan(dataSize) must be true. dataSize must be >= typ.Size_.
//
//go:nosplit
[GoRecv] internal static uintptr /*scanSize*/ writeHeapBitsSmall(this ref mspan span, uintptr x, uintptr dataSize, ж<_type> Ꮡtyp) {
    uintptr scanSize = default!;

    ref var typ = ref Ꮡtyp.DerefOrNull();
    // The objects here are always really small, so a single load is sufficient.
    var src0 = readUintptr(typ.GCData);
    // Create repetitions of the bitmap if we have a small array.
    var bits = span.elemsize / (uintptr)goarch.PtrSize;
    scanSize = typ.PtrBytes;
    var src = src0;
    var exprᴛ1 = typ.Size_;
    if (exprᴛ1 == goarch.PtrSize) {
        src = (((uintptr)1).Lsh((uint64)((dataSize / (uintptr)goarch.PtrSize)))) - 1;
    }
    else { /* default: */
        for (var iΔ2 = typ.Size_; iΔ2 < dataSize; iΔ2 += typ.Size_) {
            src |= (uintptr)(src0.Lsh((uint64)((iΔ2 / (uintptr)goarch.PtrSize))));
            scanSize += typ.Size_;
        }
    }

    // Since we're never writing more than one uintptr's worth of bits, we're either going
    // to do one or two writes.
    var dst = span.heapBits();
    var o = (x - span.@base()) / (uintptr)goarch.PtrSize;
    var i = o / (uintptr)ptrBits;
    var j = o % (uintptr)ptrBits;
    if (j + bits > ptrBits){
        // Two writes.
        var bits0 = (uintptr)ptrBits - j;
        var bits1 = bits - bits0;
        dst[(nint)(i + 0)] = (uintptr)((uintptr)(dst[(nint)(i + 0)] & ((~(uintptr)0).Rsh((uint64)(bits0)))) | (src.Lsh((uint64)(j))));
        dst[(nint)(i + 1)] = (uintptr)((uintptr)(dst[(nint)(i + 1)] & ~((((uintptr)1).Lsh((uint64)(bits1))) - 1)) | (src.Rsh((uint64)(bits0))));
    } else {
        // One write.
        dst[(nint)(i)] = (uintptr)(((uintptr)(dst[(nint)(i)] & ~(((((uintptr)1).Lsh((uint64)(bits))) - 1).Lsh((uint64)(j))))) | (src.Lsh((uint64)(j))));
    }
    const bool doubleCheck = false;
    if (doubleCheck) {
        var srcRead = span.heapBitsSmallForAddr(x);
        if (srcRead != src) {
            print((@string)"runtime: x="u8, ((Δhex)(uint64)x), (@string)" i="u8, i, (@string)" j="u8, j, (@string)" bits="u8, bits, (@string)"\n"u8);
            print((@string)"runtime: dataSize="u8, dataSize, (@string)" typ.Size_="u8, typ.Size_, (@string)" typ.PtrBytes="u8, typ.PtrBytes, (@string)"\n"u8);
            print((@string)"runtime: src0="u8, ((Δhex)(uint64)src0), (@string)" src="u8, ((Δhex)(uint64)src), (@string)" srcRead="u8, ((Δhex)(uint64)srcRead), (@string)"\n"u8);
            @throw(badPointerBitsWrittenForˢ);
        }
    }
    return scanSize;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string triedToWriteHeapBitsButˢ = "tried to write heap bits, but no heap bits in span"u8;
internal static readonly @string gcProgForTypeThatIsnTˢ = "GCProg for type that isn't large"u8;

// heapSetType records that the new allocation [x, x+size)
// holds in [x, x+dataSize) one or more values of type typ.
// (The number of values is given by dataSize / typ.Size.)
// If dataSize < size, the fragment [x+dataSize, x+size) is
// recorded as non-pointer data.
// It is known that the type has pointers somewhere;
// malloc does not call heapSetType when there are no pointers.
//
// There can be read-write races between heapSetType and things
// that read the heap metadata like scanobject. However, since
// heapSetType is only used for objects that have not yet been
// made reachable, readers will ignore bits being modified by this
// function. This does mean this function cannot transiently modify
// shared memory that belongs to neighboring objects. Also, on weakly-ordered
// machines, callers must execute a store/store (publication) barrier
// between calling this function and making the object reachable.
internal static uintptr /*scanSize*/ heapSetType(uintptr x, uintptr dataSize, ж<_type> Ꮡtyp, ж<ж<_type>> Ꮡheader, ж<mspan> Ꮡspan) {
    uintptr scanSize = default!;

    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var header = ref Ꮡheader.DerefOrNull();
    ref var span = ref Ꮡspan.DerefOrNull();
    const bool doubleCheck = false;
    var gctyp = Ꮡtyp;
    if (Ꮡheader == nil){
        if (doubleCheck && (!heapBitsInSpan(dataSize) || !heapBitsInSpan(span.elemsize))) {
            @throw(triedToWriteHeapBitsButˢ);
        }
        // Handle the case where we have no malloc header.
        scanSize = span.writeHeapBitsSmall(x, dataSize, Ꮡtyp);
    } else {
        if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0) {
            // Allocate space to unroll the gcprog. This space will consist of
            // a dummy _type value and the unrolled gcprog. The dummy _type will
            // refer to the bitmap, and the mspan will refer to the dummy _type.
            if (span.spanclass.sizeclass() != 0) {
                @throw(gcProgForTypeThatIsnTˢ);
            }
            var spaceNeeded = alignUp(/* unsafe.Sizeof(_type{}) */ (uintptr)48, goarch.PtrSize);
            var heapBitsOff = spaceNeeded;
            spaceNeeded += alignUp(typ.PtrBytes / (uintptr)goarch.PtrSize / 8, goarch.PtrSize);
            var npages = alignUp(spaceNeeded, pageSize) / (uintptr)pageSize;
            ref var progSpan = ref heap<ж<mspan>>(out var ᏑprogSpan);
            systemstack(() => {
                ᏑprogSpan.ValueSlot = Ꮡmheap_.allocManual(npages, spanAllocPtrScalarBits);
                memclrNoHeapPointers((@unsafe.Pointer)ᏑprogSpan.ValueSlot.@base(), (~ᏑprogSpan.ValueSlot).npages * (uintptr)pageSize);
            });
            // Write a dummy _type in the new space.
            //
            // We only need to write size, PtrBytes, and GCData, since that's all
            // the GC cares about.
            gctyp = (ж<_type>)(uintptr)((@unsafe.Pointer)progSpan.@base());
            gctyp.Value.Size_ = typ.Size_;
            gctyp.Value.PtrBytes = typ.PtrBytes;
            gctyp.Value.GCData = (ж<byte>)(uintptr)(add((@unsafe.Pointer)progSpan.@base(), heapBitsOff));
            gctyp.Value.TFlag = abi.TFlagUnrolledBitmap;
            // Expand the GC program into space reserved at the end of the new span.
            runGCProg(addb(typ.GCData, 4), (~gctyp).GCData);
        }
        // Write out the header.
        header = gctyp;
        scanSize = span.elemsize;
    }
    if (doubleCheck) {
        doubleCheckHeapPointers(x, dataSize, gctyp, Ꮡheader, Ꮡspan);
        // To exercise the less common path more often, generate
        // a random interior pointer and make sure iterating from
        // that point works correctly too.
        var maxIterBytes = span.elemsize;
        if (Ꮡheader == nil) {
            maxIterBytes = dataSize;
        }
        var off = alignUp((uintptr)cheaprand() % dataSize, goarch.PtrSize);
        var size = dataSize - off;
        if (size == 0) {
            off -= goarch.PtrSize;
            size += goarch.PtrSize;
        }
        var interior = x + off;
        size -= alignDown((uintptr)cheaprand() % size, goarch.PtrSize);
        if (size == 0) {
            size = goarch.PtrSize;
        }
        // Round up the type to the size of the type.
        size = (size + (~gctyp).Size_ - 1) / (~gctyp).Size_ * (~gctyp).Size_;
        if (interior + size > x + maxIterBytes) {
            size = x + maxIterBytes - interior;
        }
        doubleCheckHeapPointersInterior(x, interior, size, dataSize, ref (gctyp).DerefOrNull(), Ꮡheader, Ꮡspan);
    }
    return scanSize;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string heapSetTypePointerEntryˢ = "heapSetType: pointer entry not correct"u8;

internal static void doubleCheckHeapPointers(uintptr x, uintptr dataSize, ж<_type> Ꮡtyp, ж<ж<_type>> Ꮡheader, ж<mspan> Ꮡspan) {
    ref var typ = ref Ꮡtyp.DerefOrNull();
    ref var span = ref Ꮡspan.DerefOrNull();

    // Check that scanning the full object works.
    var tp = span.typePointersOfUnchecked(span.objBase(x));
    var maxIterBytes = span.elemsize;
    if (Ꮡheader == nil) {
        maxIterBytes = dataSize;
    }
    var bad = false;
    for (var i = (uintptr)0; i < maxIterBytes; i += goarch.PtrSize) {
        // Compute the pointer bit we want at offset i.
        var want = false;
        if (i < span.elemsize) {
            var off = i % typ.Size_;
            if (off < typ.PtrBytes) {
                var j = off / (uintptr)goarch.PtrSize;
                want = (byte)((addb(typ.GCData, j / 8).Value >> (int)((j % 8))) & 1) != 0;
            }
        }
        if (want) {
            uintptr addr = default!;
            (tp, addr) = tp.next(x + span.elemsize);
            if (addr == 0) {
                println((@string)"runtime: found bad iterator"u8);
            }
            if (addr != x + i) {
                print((@string)"runtime: addr="u8, ((Δhex)(uint64)addr), (@string)" x+i="u8, ((Δhex)(uint64)(x + i)), (@string)"\n"u8);
                bad = true;
            }
        }
    }
    if (!bad) {
        uintptr addr = default!;
        (tp, addr) = tp.next(x + span.elemsize);
        if (addr == 0) {
            return;
        }
        println((@string)"runtime: extra pointer:"u8, ((Δhex)(uint64)addr));
    }
    print((@string)"runtime: hasHeader="u8, Ꮡheader != nil, (@string)" typ.Size_="u8, typ.Size_, (@string)" hasGCProg="u8, (abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0, (@string)"\n"u8);
    print((@string)"runtime: x="u8, ((Δhex)(uint64)x), (@string)" dataSize="u8, dataSize, (@string)" elemsize="u8, span.elemsize, (@string)"\n"u8);
    print((@string)"runtime: typ="u8, new @unsafe.Pointer(Ꮡtyp), (@string)" typ.PtrBytes="u8, typ.PtrBytes, (@string)"\n"u8);
    print((@string)"runtime: limit="u8, ((Δhex)(uint64)(x + span.elemsize)), (@string)"\n"u8);
    tp = span.typePointersOfUnchecked(x);
    dumpTypePointers(tp);
    while (ᐧ) {
        uintptr addr = default!;
        {
            (tp, addr) = tp.next(x + span.elemsize); if (addr == 0) {
                println((@string)"runtime: would've stopped here"u8);
                dumpTypePointers(tp);
                break;
            }
        }
        print((@string)"runtime: addr="u8, ((Δhex)(uint64)addr), (@string)"\n"u8);
        dumpTypePointers(tp);
    }
    @throw(heapSetTypePointerEntryˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string foundBadInteriorPointerˢ = "found bad interior pointer"u8;

internal static void doubleCheckHeapPointersInterior(uintptr x, uintptr interior, uintptr size, uintptr dataSize, ref _type typ, ж<ж<_type>> Ꮡheader, ж<mspan> Ꮡspan) {
    ref var span = ref Ꮡspan.DerefOrNull();

    var bad = false;
    if (interior < x) {
        print((@string)"runtime: interior="u8, ((Δhex)(uint64)interior), (@string)" x="u8, ((Δhex)(uint64)x), (@string)"\n"u8);
        @throw(foundBadInteriorPointerˢ);
    }
    var off = interior - x;
    var tp = span.typePointersOf(interior, size);
    for (var i = off; i < off + size; i += goarch.PtrSize) {
        // Compute the pointer bit we want at offset i.
        var want = false;
        if (i < span.elemsize) {
            var offΔ1 = i % typ.Size_;
            if (offΔ1 < typ.PtrBytes) {
                var j = offΔ1 / (uintptr)goarch.PtrSize;
                want = (byte)((addb(typ.GCData, j / 8).Value >> (int)((j % 8))) & 1) != 0;
            }
        }
        if (want) {
            uintptr addr = default!;
            (tp, addr) = tp.next(interior + size);
            if (addr == 0) {
                println((@string)"runtime: found bad iterator"u8);
                bad = true;
            }
            if (addr != x + i) {
                print((@string)"runtime: addr="u8, ((Δhex)(uint64)addr), (@string)" x+i="u8, ((Δhex)(uint64)(x + i)), (@string)"\n"u8);
                bad = true;
            }
        }
    }
    if (!bad) {
        uintptr addr = default!;
        (tp, addr) = tp.next(interior + size);
        if (addr == 0) {
            return;
        }
        println((@string)"runtime: extra pointer:"u8, ((Δhex)(uint64)addr));
    }
    print((@string)"runtime: hasHeader="u8, Ꮡheader != nil, (@string)" typ.Size_="u8, typ.Size_, (@string)"\n"u8);
    print((@string)"runtime: x="u8, ((Δhex)(uint64)x), (@string)" dataSize="u8, dataSize, (@string)" elemsize="u8, span.elemsize, (@string)" interior="u8, ((Δhex)(uint64)interior), (@string)" size="u8, size, (@string)"\n"u8);
    print((@string)"runtime: limit="u8, ((Δhex)(uint64)(interior + size)), (@string)"\n"u8);
    tp = span.typePointersOf(interior, size);
    dumpTypePointers(tp);
    while (ᐧ) {
        uintptr addr = default!;
        {
            (tp, addr) = tp.next(interior + size); if (addr == 0) {
                println((@string)"runtime: would've stopped here"u8);
                dumpTypePointers(tp);
                break;
            }
        }
        print((@string)"runtime: addr="u8, ((Δhex)(uint64)addr), (@string)"\n"u8);
        dumpTypePointers(tp);
    }
    print((@string)"runtime: want: "u8);
    for (var i = off; i < off + size; i += goarch.PtrSize) {
        // Compute the pointer bit we want at offset i.
        var want = false;
        if (i < dataSize) {
            var offΔ2 = i % typ.Size_;
            if (offΔ2 < typ.PtrBytes) {
                var j = offΔ2 / (uintptr)goarch.PtrSize;
                want = (byte)((addb(typ.GCData, j / 8).Value >> (int)((j % 8))) & 1) != 0;
            }
        }
        if (want){
            print((@string)"1"u8);
        } else {
            print((@string)"0"u8);
        }
    }
    println();
    @throw(heapSetTypePointerEntryˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mismatchBetweenˢ = "mismatch between typePointersOfType and typePointersOf"u8;

//go:nosplit
internal static void doubleCheckTypePointersOfType(ж<mspan> Ꮡs, ж<_type> Ꮡtyp, uintptr addr, uintptr size) {
    ref var s = ref Ꮡs.DerefOrNull();
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (Ꮡtyp == nil || (abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0) {
        return;
    }
    if ((abiꓸKind)(typ.Kind_ & abi.KindMask) == abi.Interface) {
        // Interfaces are unfortunately inconsistently handled
        // when it comes to the type pointer, so it's easy to
        // produce a lot of false positives here.
        return;
    }
    var tp0 = s.typePointersOfType(Ꮡtyp, addr);
    var tp1 = s.typePointersOf(addr, size);
    var failed = false;
    while (ᐧ) {
        uintptr addr0 = default!;
        uintptr addr1 = default!;
        (tp0, addr0) = tp0.next(addr + size);
        (tp1, addr1) = tp1.next(addr + size);
        if (addr0 != addr1) {
            failed = true;
            break;
        }
        if (addr0 == 0) {
            break;
        }
    }
    if (failed) {
        var tp0Δ1 = s.typePointersOfType(Ꮡtyp, addr);
        var tp1Δ1 = s.typePointersOf(addr, size);
        print((@string)"runtime: addr="u8, ((Δhex)(uint64)addr), (@string)" size="u8, size, (@string)"\n"u8);
        print((@string)"runtime: type="u8, toRType(Ꮡtyp).@string(), (@string)"\n"u8);
        dumpTypePointers(tp0Δ1);
        dumpTypePointers(tp1Δ1);
        while (ᐧ) {
            uintptr addr0 = default!;
            uintptr addr1 = default!;
            (tp0Δ1, addr0) = tp0Δ1.next(addr + size);
            (tp1Δ1, addr1) = tp1Δ1.next(addr + size);
            print((@string)"runtime: "u8, ((Δhex)(uint64)addr0), (@string)" "u8, ((Δhex)(uint64)addr1), (@string)"\n"u8);
            if (addr0 == 0 && addr1 == 0) {
                break;
            }
        }
        @throw(mismatchBetweenˢ);
    }
}

internal static void dumpTypePointers(typePointers tp) {
    print((@string)"runtime: tp.elem="u8, ((Δhex)(uint64)tp.elem), (@string)" tp.typ="u8, new @unsafe.Pointer(tp.typ), (@string)"\n"u8);
    print((@string)"runtime: tp.addr="u8, ((Δhex)(uint64)tp.addr), (@string)" tp.mask="u8);
    for (var i = (uintptr)0; i < ptrBits; i++) {
        if ((uintptr)(tp.mask & (((uintptr)1).Lsh((uint64)(i)))) != 0){
            print((@string)"1"u8);
        } else {
            print((@string)"0"u8);
        }
    }
    println();
}

// addb returns the byte pointer p+n.
//
//go:nowritebarrier
//go:nosplit
internal static ж<byte> addb(ж<byte> Ꮡp, uintptr n) {
    // Note: wrote out full expression instead of calling add(p, n)
    // to reduce the number of temporaries generated by the
    // compiler for this trivial expression during inlining.
    return (ж<byte>)(uintptr)((@unsafe.Pointer)((uintptr)Ꮡp + n));
}

// subtractb returns the byte pointer p-n.
//
//go:nowritebarrier
//go:nosplit
internal static ж<byte> subtractb(ж<byte> Ꮡp, uintptr n) {
    // Note: wrote out full expression instead of calling add(p, -n)
    // to reduce the number of temporaries generated by the
    // compiler for this trivial expression during inlining.
    return (ж<byte>)(uintptr)((@unsafe.Pointer)((uintptr)Ꮡp - n));
}

// add1 returns the byte pointer p+1.
//
//go:nowritebarrier
//go:nosplit
internal static ж<byte> add1(ж<byte> Ꮡp) {
    // Note: wrote out full expression instead of calling addb(p, 1)
    // to reduce the number of temporaries generated by the
    // compiler for this trivial expression during inlining.
    return (ж<byte>)(uintptr)((@unsafe.Pointer)((uintptr)Ꮡp + 1));
}

// subtract1 returns the byte pointer p-1.
//
// nosplit because it is used during write barriers and must not be preempted.
//
//go:nowritebarrier
//go:nosplit
internal static ж<byte> subtract1(ж<byte> Ꮡp) {
    // Note: wrote out full expression instead of calling subtractb(p, 1)
    // to reduce the number of temporaries generated by the
    // compiler for this trivial expression during inlining.
    return (ж<byte>)(uintptr)((@unsafe.Pointer)((uintptr)Ꮡp - 1));
}

// markBits provides access to the mark bit for an object in the heap.
// bytep points to the byte holding the mark bit.
// mask is a byte with a single bit set that can be &ed with *bytep
// to see if the bit has been set.
// *m.byte&m.mask != 0 indicates the mark bit is set.
// index can be used along with span information to generate
// the address of the object in the heap.
// We maintain one set of mark bits for allocation and one for
// marking purposes.
[GoType] partial struct markBits {
    internal ж<uint8> bytep;
    internal uint8 mask;
    internal uintptr index;
}

//go:nosplit
[GoRecv] internal static markBits allocBitsForIndex(this ref mspan s, uintptr allocBitIndex) {
    var (bytep, mask) = s.allocBits.bitp(allocBitIndex);
    return new markBits(bytep, mask, allocBitIndex);
}

// refillAllocCache takes 8 bytes s.allocBits starting at whichByte
// and negates them so that ctz (count trailing zeros) instructions
// can be used. It then places these 8 bytes into the cached 64 bit
// s.allocCache.
[GoRecv] internal static void refillAllocCache(this ref mspan s, uint16 whichByte) {
    var bytes = array<uint8>.AliasPointer(s.allocBits.bytep((uintptr)whichByte), 8);
    var aCache = (uint64)0;
    aCache |= (uint64)((uint64)bytes.Value[0]);
    aCache |= (uint64)(((uint64)bytes.Value[1] << (int)((1 * 8))));
    aCache |= (uint64)(((uint64)bytes.Value[2] << (int)((2 * 8))));
    aCache |= (uint64)(((uint64)bytes.Value[3] << (int)((3 * 8))));
    aCache |= (uint64)(((uint64)bytes.Value[4] << (int)((4 * 8))));
    aCache |= (uint64)(((uint64)bytes.Value[5] << (int)((5 * 8))));
    aCache |= (uint64)(((uint64)bytes.Value[6] << (int)((6 * 8))));
    aCache |= (uint64)(((uint64)bytes.Value[7] << (int)((7 * 8))));
    s.allocCache = ~aCache;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sFreeindexSNelemsˢ = "s.freeindex > s.nelems"u8;

// nextFreeIndex returns the index of the next free object in s at
// or after s.freeindex.
// There are hardware instructions that can be used to make this
// faster if profiling warrants it.
[GoRecv] internal static uint16 nextFreeIndex(this ref mspan s) {
    var sfreeindex = s.freeindex;
    var snelems = s.nelems;
    if (sfreeindex == snelems) {
        return sfreeindex;
    }
    if (sfreeindex > snelems) {
        @throw(sFreeindexSNelemsˢ);
    }
    var aCache = s.allocCache;
    nint bitIndex = sys.TrailingZeros64(aCache);
    while (bitIndex == 64) {
        // Move index to start of next cached bits.
        sfreeindex = (uint16)((sfreeindex + 64) & ~(64 - 1));
        if (sfreeindex >= snelems) {
            s.freeindex = snelems;
            return snelems;
        }
        var whichByte = (uint16)(sfreeindex / 8);
        // Refill s.allocCache with the next 64 alloc bits.
        s.refillAllocCache(whichByte);
        aCache = s.allocCache;
        bitIndex = sys.TrailingZeros64(aCache);
    }
    // nothing available in cached bits
    // grab the next 8 bytes and try again.
    var result = (uint16)(sfreeindex + (uint16)bitIndex);
    if (result >= snelems) {
        s.freeindex = snelems;
        return snelems;
    }
    s.allocCache >>= (int)((nuint)(bitIndex + 1));
    sfreeindex = (uint16)(result + 1);
    if ((uint16)(sfreeindex % 64) == 0 && sfreeindex != snelems) {
        // We just incremented s.freeindex so it isn't 0.
        // As each 1 in s.allocCache was encountered and used for allocation
        // it was shifted away. At this point s.allocCache contains all 0s.
        // Refill s.allocCache so that it corresponds
        // to the bits at s.allocBits starting at s.freeindex.
        var whichByte = (uint16)(sfreeindex / 8);
        s.refillAllocCache(whichByte);
    }
    s.freeindex = sfreeindex;
    return result;
}

// isFree reports whether the index'th object in s is unallocated.
//
// The caller must ensure s.state is mSpanInUse, and there must have
// been no preemption points since ensuring this (which could allow a
// GC transition, which would allow the state to change).
[GoRecv] internal static bool isFree(this ref mspan s, uintptr index) {
    if (index < (uintptr)s.freeIndexForScan) {
        return false;
    }
    var (bytep, mask) = s.allocBits.bitp(index);
    return (uint8)(bytep.Value & mask) == 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badMagicDivisionˢ = "bad magic division"u8;

// divideByElemSize returns n/s.elemsize.
// n must be within [0, s.npages*_PageSize),
// or may be exactly s.npages*_PageSize
// if s.elemsize is from sizeclasses.go.
//
// nosplit, because it is called by objIndex, which is nosplit
//
//go:nosplit
[GoRecv] internal static uintptr divideByElemSize(this ref mspan s, uintptr n) {
    const bool doubleCheck = false;
    // See explanation in mksizeclasses.go's computeDivMagic.
    var q = (uintptr)((((uint64)n * (uint64)s.divMul) >> (int)(32)));
    if (doubleCheck && q != n / s.elemsize) {
        println(n, (@string)"/"u8, s.elemsize, (@string)"should be"u8, n / s.elemsize, (@string)"but got"u8, q);
        @throw(badMagicDivisionˢ);
    }
    return q;
}

// nosplit, because it is called by other nosplit code like findObject
//
//go:nosplit
[GoRecv] internal static uintptr objIndex(this ref mspan s, uintptr Δp) {
    return s.divideByElemSize(Δp - s.@base());
}

internal static markBits markBitsForAddr(uintptr Δp) {
    var s = spanOf(Δp);
    var objIndex = s.objIndex(Δp);
    return s.markBitsForIndex(objIndex);
}

[GoRecv] internal static markBits markBitsForIndex(this ref mspan s, uintptr objIndex) {
    var (bytep, mask) = s.gcmarkBits.bitp(objIndex);
    return new markBits(bytep, mask, objIndex);
}

[GoRecv] internal static markBits markBitsForBase(this ref mspan s) {
    return new markBits(s.gcmarkBits.of(gcBits.Ꮡx), (uint8)1, 0);
}

// isMarked reports whether mark bit m is set.
internal static bool isMarked(this markBits m) {
    return (uint8)(m.bytep.Value & m.mask) != 0;
}

// setMarked sets the marked bit in the markbits, atomically.
internal static void setMarked(this markBits m) {
    // Might be racing with other updates, so use atomic update always.
    // We used to be clever here and use a non-atomic update in certain
    // cases, but it's not worth the risk.
    atomic.Or8(m.bytep, m.mask);
}

// setMarkedNonAtomic sets the marked bit in the markbits, non-atomically.
internal static void setMarkedNonAtomic(this markBits m) {
    m.bytep.Value |= (uint8)(m.mask);
}

// clearMarked clears the marked bit in the markbits, atomically.
internal static void clearMarked(this markBits m) {
    // Might be racing with other updates, so use atomic update always.
    // We used to be clever here and use a non-atomic update in certain
    // cases, but it's not worth the risk.
    atomic.And8(m.bytep, (uint8)(((uint8)(~m.mask))));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string markBitsForSpanUnalignedˢ = "markBitsForSpan: unaligned start"u8;

// markBitsForSpan returns the markBits for the span base address base.
internal static markBits /*mbits*/ markBitsForSpan(uintptr @base) {
    markBits mbits = default!;

    mbits = markBitsForAddr(@base);
    if (mbits.mask != 1) {
        @throw(markBitsForSpanUnalignedˢ);
    }
    return mbits;
}

// advance advances the markBits to the next object in the span.
[GoRecv] internal static void advance(this ref markBits m) {
    if (m.mask == (uint8)(1 << (int)(7))){
        m.bytep = (ж<uint8>)(uintptr)((@unsafe.Pointer)((uintptr)m.bytep + 1));
        m.mask = 1;
    } else {
        m.mask = (uint8)(m.mask << (int)(1));
    }
    m.index++;
}

// clobberdeadPtr is a special value that is used by the compiler to
// clobber dead stack slots, when -clobberdead flag is set.
internal static uintptr clobberdeadPtr => /* uintptr(0xdeaddead | 0xdeaddead<<((^uintptr(0)>>63)*32)) */ unchecked((uintptr)16045725885737590445);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string objectˢ = "object"u8;
internal static readonly @string foundBadPointerInGoHeapˢ = "found bad pointer in Go heap (incorrect use of unsafe or cgo?)"u8;

// badPointer throws bad pointer in heap panic.
internal static void badPointer(ж<mspan> Ꮡs, uintptr Δp, uintptr refBase, uintptr refOff) {
    ref var s = ref Ꮡs.DerefOrNull();

    // Typically this indicates an incorrect use
    // of unsafe or cgo to store a bad pointer in
    // the Go heap. It may also indicate a runtime
    // bug.
    //
    // TODO(austin): We could be more aggressive
    // and detect pointers to unallocated objects
    // in allocated spans.
    printlock();
    print((@string)"runtime: pointer "u8, ((Δhex)(uint64)Δp));
    if (Ꮡs != nil) {
        var state = Ꮡs.of(mspan.Ꮡstate).get();
        if (state != mSpanInUse){
            print((@string)" to unallocated span"u8);
        } else {
            print((@string)" to unused region of span"u8);
        }
        print((@string)" span.base()="u8, ((Δhex)(uint64)s.@base()), (@string)" span.limit="u8, ((Δhex)(uint64)s.limit), (@string)" span.state="u8, state);
    }
    print((@string)"\n"u8);
    if (refBase != 0) {
        print((@string)"runtime: found in object at *("u8, ((Δhex)(uint64)refBase), (@string)"+"u8, ((Δhex)(uint64)refOff), (@string)")\n"u8);
        gcDumpObject(objectˢ, refBase, refOff);
    }
    getg().Value.m.Value.traceback = 2;
    @throw(foundBadPointerInGoHeapˢ);
}

// findObject returns the base address for the heap object containing
// the address p, the object's span, and the index of the object in s.
// If p does not point into a heap object, it returns base == 0.
//
// If p points is an invalid heap pointer and debug.invalidptr != 0,
// findObject panics.
//
// refBase and refOff optionally give the base address of the object
// in which the pointer p was found and the byte offset at which it
// was found. These are used for error reporting.
//
// It is nosplit so it is safe for p to be a pointer to the current goroutine's stack.
// Since p is a uintptr, it would not be adjusted if the stack were to move.
//
// findObject should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname findObject
//go:nosplit
internal static (uintptr @base, ж<mspan> s, uintptr objIndex) findObject(uintptr Δp, uintptr refBase, uintptr refOff) {
    uintptr @base = default!;
    ж<mspan> s = default!;
    uintptr objIndex = default!;

    s = spanOf(Δp);
    // If s is nil, the virtual address has never been part of the heap.
    // This pointer may be to some mmap'd region, so we allow it.
    if (s == nil) {
        if ((GOARCH == "amd64"u8 || GOARCH == "arm64"u8) && Δp == clobberdeadPtr && debug.invalidptr != 0) {
            // Crash if clobberdeadPtr is seen. Only on AMD64 and ARM64 for now,
            // as they are the only platform where compiler's clobberdead mode is
            // implemented. On these platforms clobberdeadPtr cannot be a valid address.
            badPointer(s, Δp, refBase, refOff);
        }
        return (@base, s, objIndex);
    }
    // If p is a bad pointer, it may not be in s's bounds.
    //
    // Check s.state to synchronize with span initialization
    // before checking other fields. See also spanOfHeap.
    {
        var state = s.of(mspan.Ꮡstate).get(); if (state != mSpanInUse || Δp < s.@base() || Δp >= (~s).limit) {
            // Pointers into stacks are also ok, the runtime manages these explicitly.
            if (state == mSpanManual) {
                return (@base, s, objIndex);
            }
            // The following ensures that we are rigorous about what data
            // structures hold valid pointers.
            if (debug.invalidptr != 0) {
                badPointer(s, Δp, refBase, refOff);
            }
            return (@base, s, objIndex);
        }
    }
    objIndex = s.objIndex(Δp);
    @base = s.@base() + objIndex * (~s).elemsize;
    return (@base, s, objIndex);
}

// reflect_verifyNotInHeapPtr reports whether converting the not-in-heap pointer into a unsafe.Pointer is ok.
//
//go:linkname reflect_verifyNotInHeapPtr reflect.verifyNotInHeapPtr
internal static bool reflect_verifyNotInHeapPtr(uintptr Δp) {
    // Conversion to a pointer is ok as long as findObject above does not call badPointer.
    // Since we're already promised that p doesn't point into the heap, just disallow heap
    // pointers and the special clobbered pointer.
    return spanOf(Δp) == nil && Δp != clobberdeadPtr;
}

internal static UntypedInt ptrBits => /* 8 * goarch.PtrSize */ 64;

// bulkBarrierBitmap executes write barriers for copying from [src,
// src+size) to [dst, dst+size) using a 1-bit pointer bitmap. src is
// assumed to start maskOffset bytes into the data covered by the
// bitmap in bits (which may not be a multiple of 8).
//
// This is used by bulkBarrierPreWrite for writes to data and BSS.
//
//go:nosplit
internal static void bulkBarrierBitmap(uintptr dst, uintptr src, uintptr size, uintptr maskOffset, ж<uint8> Ꮡbits) {
    ref var bits = ref Ꮡbits.DerefOrNull();

    var word = maskOffset / (uintptr)goarch.PtrSize;
    Ꮡbits = addb(Ꮡbits, word / 8); bits = ref Ꮡbits.DerefOrNull();
    var mask = (uint8)((uint8)1 << (int)((word % 8)));
    var buf = (~(~getg()).m).p.ptr().of(runtime_package.Δp.ᏑwbBuf);
    for (var i = (uintptr)0; i < size; i += goarch.PtrSize) {
        if (mask == 0) {
            Ꮡbits = addb(Ꮡbits, 1); bits = ref Ꮡbits.DerefOrNull();
            if (bits == 0) {
                // Skip 8 words.
                i += 7 * goarch.PtrSize;
                continue;
            }
            mask = 1;
        }
        if ((uint8)(bits & mask) != 0) {
            var dstx = (ж<uintptr>)(uintptr)((@unsafe.Pointer)(dst + i));
            if (src == 0){
                var Δp = buf.get1();
                Δp.Value[0] = dstx.Value;
            } else {
                var srcx = (ж<uintptr>)(uintptr)((@unsafe.Pointer)(src + i));
                var Δp = buf.get2();
                Δp.Value[0] = dstx.Value;
                Δp.Value[1] = srcx.Value;
            }
        }
        mask <<= (int)(1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeˢ2 = "runtime: typeBitsBulkBarrier without type"u8;
internal static readonly @string runtimeInvalidˢ = "runtime: invalid typeBitsBulkBarrier"u8;

// typeBitsBulkBarrier executes a write barrier for every
// pointer that would be copied from [src, src+size) to [dst,
// dst+size) by a memmove using the type bitmap to locate those
// pointer slots.
//
// The type typ must correspond exactly to [src, src+size) and [dst, dst+size).
// dst, src, and size must be pointer-aligned.
// The type typ must have a plain bitmap, not a GC program.
// The only use of this function is in channel sends, and the
// 64 kB channel element limit takes care of this for us.
//
// Must not be preempted because it typically runs right before memmove,
// and the GC must observe them as an atomic action.
//
// Callers must perform cgo checks if goexperiment.CgoCheck2.
//
//go:nosplit
internal static void typeBitsBulkBarrier(ж<_type> Ꮡtyp, uintptr dst, uintptr src, uintptr size) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (Ꮡtyp == nil) {
        @throw(runtimeˢ2);
    }
    if (typ.Size_ != size) {
        println((@string)"runtime: typeBitsBulkBarrier with type "u8, toRType(Ꮡtyp).@string(), (@string)" of size "u8, typ.Size_, (@string)" but memory size"u8, size);
        @throw(runtimeInvalidˢ);
    }
    if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0) {
        println((@string)"runtime: typeBitsBulkBarrier with type "u8, toRType(Ꮡtyp).@string(), (@string)" with GC prog"u8);
        @throw(runtimeInvalidˢ);
    }
    if (!writeBarrier.enabled) {
        return;
    }
    var ptrmask = typ.GCData;
    var buf = (~(~getg()).m).p.ptr().of(runtime_package.Δp.ᏑwbBuf);
    uint32 bits = default!;
    for (var i = (uintptr)0; i < typ.PtrBytes; i += goarch.PtrSize) {
        if ((uintptr)(i & (uintptr)(goarch.PtrSize * 8 - 1)) == 0){
            bits = (uint32)(ptrmask.Value);
            ptrmask = addb(ptrmask, 1);
        } else {
            bits = (bits >> (int)(1));
        }
        if ((uint32)(bits & 1) != 0) {
            var dstx = (ж<uintptr>)(uintptr)((@unsafe.Pointer)(dst + i));
            var srcx = (ж<uintptr>)(uintptr)((@unsafe.Pointer)(src + i));
            var Δp = buf.get2();
            Δp.Value[0] = dstx.Value;
            Δp.Value[1] = srcx.Value;
        }
    }
}

// countAlloc returns the number of objects allocated in span s by
// scanning the mark bitmap.
[GoRecv] internal static nint countAlloc(this ref mspan s) {
    nint count = 0;
    var bytes = divRoundUp((uintptr)s.nelems, 8);
    // Iterate over each 8-byte chunk and count allocations
    // with an intrinsic. Note that newMarkBits guarantees that
    // gcmarkBits will be 8-byte aligned, so we don't have to
    // worry about edge cases, irrelevant bits will simply be zero.
    for (var i = (uintptr)0; i < bytes; i += 8) {
        // Extract 64 bits from the byte pointer and get a OnesCount.
        // Note that the unsafe cast here doesn't preserve endianness,
        // but that's OK. We only care about how many bits are 1, not
        // about the order we discover them in.
        var mrkBits = ~s.gcmarkBits.bytep(i).Reinterpret<uint8, uint64>();
        count += sys.OnesCount64(mrkBits);
    }
    return count;
}

// Read the bytes starting at the aligned pointer p into a uintptr.
// Read is little-endian.
internal static uintptr readUintptr(ж<byte> Ꮡp) {
    var x = ~Ꮡp.Reinterpret<byte, uintptr>();
    if (goarch.BigEndian) {
        if (goarch.PtrSize == 8) {
            return (uintptr)sys.Bswap64((uint64)x);
        }
        return (uintptr)sys.Bswap32((uint32)x);
    }
    return x;
}


[GoType("dyn")] partial struct debugPtrmaskᴛ1 {
    internal mutex @lock;
    internal ж<byte> data;
}
internal static debugPtrmaskᴛ1 debugPtrmask = new();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string progToPointerMaskˢ = "progToPointerMask: overflow"u8;

// progToPointerMask returns the 1-bit pointer mask output by the GC program prog.
// size the size of the region described by prog, in bytes.
// The resulting bitvector will have no more than size/goarch.PtrSize bits.
internal static unsafe bitvector progToPointerMask(ж<byte> Ꮡprog, uintptr size) {
    var n = (size / (uintptr)goarch.PtrSize + 7) / 8;
    var x = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(persistentalloc(n + 1, 1, Ꮡmemstats.of(mstats.Ꮡbuckhash_sys))), (int)(n + 1)));
    x[len(x) - 1] = 0xa1; // overflow check sentinel
    n = runGCProg(Ꮡprog, Ꮡ(x, 0));
    if (x[len(x) - 1] != 0xa1) {
        @throw(progToPointerMaskˢ);
    }
    return new bitvector((int32)n, Ꮡ(x, 0));
}

// Packed GC pointer bitmaps, aka GC programs.
//
// For large types containing arrays, the type information has a
// natural repetition that can be encoded to save space in the
// binary and in the memory representation of the type information.
//
// The encoding is a simple Lempel-Ziv style bytecode machine
// with the following instructions:
//
//	00000000: stop
//	0nnnnnnn: emit n bits copied from the next (n+7)/8 bytes
//	10000000 n c: repeat the previous n bits c times; n, c are varints
//	1nnnnnnn c: repeat the previous n bits c times; c is a varint

// runGCProg returns the number of 1-bit entries written to memory.
internal static uintptr runGCProg(ж<byte> Ꮡprog, ж<byte> Ꮡdst) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    var dstStart = Ꮡdst;
    // Bits waiting to be written to memory.
    uintptr bits = default!;
    uintptr nbits = default!;
    var Δp = Ꮡprog;
Run:
    while (ᐧ) {
        // Flush accumulated full bytes.
        // The rest of the loop assumes that nbits <= 7.
        for (; nbits >= 8; nbits -= 8) {
            dst = (uint8)bits;
            Ꮡdst = add1(Ꮡdst); dst = ref Ꮡdst.DerefOrNull();
            bits >>= (int)(8);
        }
        // Process one instruction.
        var inst = (uintptr)(Δp.Value);
        Δp = add1(Δp);
        var n = (uintptr)(inst & 0x7F);
        if ((uintptr)(inst & 0x80) == 0) {
            // Literal bits; n == 0 means end of program.
            if (n == 0) {
                // Program is over.
                goto break_Run;
            }
            var nbyte = n / 8;
            for (var i = (uintptr)0; i < nbyte; i++) {
                bits |= (uintptr)(((uintptr)(Δp.Value)).Lsh((uint64)(nbits)));
                Δp = add1(Δp);
                dst = (uint8)bits;
                Ꮡdst = add1(Ꮡdst); dst = ref Ꮡdst.DerefOrNull();
                bits >>= (int)(8);
            }
            {
                n %= 8; if (n > 0) {
                    bits |= (uintptr)(((uintptr)(Δp.Value)).Lsh((uint64)(nbits)));
                    Δp = add1(Δp);
                    nbits += n;
                }
            }
            goto continue_Run;
        }
        // Repeat. If n == 0, it is encoded in a varint in the next bytes.
        if (n == 0) {
            for (nuint offΔ1 = (nuint)0; ᐧ ; offΔ1 += 7) {
                var x = (uintptr)(Δp.Value);
                Δp = add1(Δp);
                n |= (uintptr)(((uintptr)(x & 0x7F)).Lsh(offΔ1));
                if ((uintptr)(x & 0x80) == 0) {
                    break;
                }
            }
        }
        // Count is encoded in a varint in the next bytes.
        var c = (uintptr)0;
        for (nuint offΔ2 = (nuint)0; ᐧ ; offΔ2 += 7) {
            var x = (uintptr)(Δp.Value);
            Δp = add1(Δp);
            c |= (uintptr)(((uintptr)(x & 0x7F)).Lsh(offΔ2));
            if ((uintptr)(x & 0x80) == 0) {
                break;
            }
        }
        c *= n; // now total number of bits to copy
        // If the number of bits being repeated is small, load them
        // into a register and use that register for the entire loop
        // instead of repeatedly reading from memory.
        // Handling fewer than 8 bits here makes the general loop simpler.
        // The cutoff is goarch.PtrSize*8 - 7 to guarantee that when we add
        // the pattern to a bit buffer holding at most 7 bits (a partial byte)
        // it will not overflow.
        var src = Ꮡdst;
        UntypedInt maxBits = /* goarch.PtrSize*8 - 7 */ 57;
        if (n <= maxBits) {
            // Start with bits in output buffer.
            var pattern = bits;
            var npattern = nbits;
            // If we need more bits, fetch them from memory.
            src = subtract1(src);
            while (npattern < n) {
                pattern <<= (int)(8);
                pattern |= (uintptr)((uintptr)(src.Value));
                src = subtract1(src);
                npattern += 8;
            }
            // We started with the whole bit output buffer,
            // and then we loaded bits from whole bytes.
            // Either way, we might now have too many instead of too few.
            // Discard the extra.
            if (npattern > n) {
                pattern >>= (int)(npattern - n);
                npattern = n;
            }
            // Replicate pattern to at most maxBits.
            if (npattern == 1){
                // One bit being repeated.
                // If the bit is 1, make the pattern all 1s.
                // If the bit is 0, the pattern is already all 0s,
                // but we can claim that the number of bits
                // in the word is equal to the number we need (c),
                // because right shift of bits will zero fill.
                if (pattern == 1){
                    pattern = (uintptr)(144115188075855872L - 1);
                    npattern = maxBits;
                } else {
                    npattern = c;
                }
            } else {
                var b = pattern;
                var nb = npattern;
                if (nb + nb <= maxBits) {
                    // Double pattern until the whole uintptr is filled.
                    while (nb <= (uintptr)(goarch.PtrSize * 8)) {
                        b |= (uintptr)(b.Lsh((uint64)(nb)));
                        nb += nb;
                    }
                    // Trim away incomplete copy of original pattern in high bits.
                    // TODO(rsc): Replace with table lookup or loop on systems without divide?
                    nb = (uintptr)maxBits / npattern * npattern;
                    b &= (uintptr)(((uintptr)1).Lsh((uint64)(nb)) - 1);
                    pattern = b;
                    npattern = nb;
                }
            }
            // Add pattern to bit buffer and flush bit buffer, c/npattern times.
            // Since pattern contains >8 bits, there will be full bytes to flush
            // on each iteration.
            for (; c >= npattern; c -= npattern) {
                bits |= (uintptr)(pattern.Lsh((uint64)(nbits)));
                nbits += npattern;
                while (nbits >= 8) {
                    dst = (uint8)bits;
                    Ꮡdst = add1(Ꮡdst); dst = ref Ꮡdst.DerefOrNull();
                    bits >>= (int)(8);
                    nbits -= 8;
                }
            }
            // Add final fragment to bit buffer.
            if (c > 0) {
                pattern &= (uintptr)(((uintptr)1).Lsh((uint64)(c)) - 1);
                bits |= (uintptr)(pattern.Lsh((uint64)(nbits)));
                nbits += c;
            }
            goto continue_Run;
        }
        // Repeat; n too large to fit in a register.
        // Since nbits <= 7, we know the first few bytes of repeated data
        // are already written to memory.
        var off = n - nbits; // n > nbits because n > maxBits and nbits <= 7
        // Leading src fragment.
        src = subtractb(src, (off + 7) / 8);
        {
            var frag = (uintptr)(off & 7); if (frag != 0) {
                bits |= (uintptr)((((uintptr)(src.Value)).Rsh((uint64)((8 - frag)))).Lsh((uint64)(nbits)));
                src = add1(src);
                nbits += frag;
                c -= frag;
            }
        }
        // Main loop: load one byte, write another.
        // The bits are rotating through the bit buffer.
        for (var i = c / 8; i > 0; i--) {
            bits |= (uintptr)(((uintptr)(src.Value)).Lsh((uint64)(nbits)));
            src = add1(src);
            dst = (uint8)bits;
            Ꮡdst = add1(Ꮡdst); dst = ref Ꮡdst.DerefOrNull();
            bits >>= (int)(8);
        }
        // Final src fragment.
        {
            c %= 8; if (c > 0) {
                bits |= (uintptr)(((uintptr)((uintptr)(src.Value) & (((uintptr)1).Lsh((uint64)(c)) - 1))).Lsh((uint64)(nbits)));
                nbits += c;
            }
        }
continue_Run:;
    }
break_Run:;
    // Write any final bits out, using full-byte writes, even for the final byte.
    var totalBits = ((uintptr)Ꮡdst - (uintptr)dstStart) * 8 + nbits;
    nbits += (uintptr)(((uintptr)0 - nbits) & 7);
    for (; nbits > 0; nbits -= 8) {
        dst = (uint8)bits;
        Ꮡdst = add1(Ꮡdst); dst = ref Ꮡdst.DerefOrNull();
        bits >>= (int)(8);
    }
    return totalBits;
}

// materializeGCProg allocates space for the (1-bit) pointer bitmask
// for an object of size ptrdata.  Then it fills that space with the
// pointer bitmask specified by the program prog.
// The bitmask starts at s.startAddr.
// The result must be deallocated with dematerializeGCProg.
internal static ж<mspan> materializeGCProg(uintptr ptrdata, ж<byte> Ꮡprog) {
    // Each word of ptrdata needs one bit in the bitmap.
    var bitmapBytes = divRoundUp(ptrdata, 8 * goarch.PtrSize);
    // Compute the number of pages needed for bitmapBytes.
    var pages = divRoundUp(bitmapBytes, pageSize);
    var s = Ꮡmheap_.allocManual(pages, spanAllocPtrScalarBits);
    runGCProg(addb(Ꮡprog, 4), (ж<byte>)(uintptr)((@unsafe.Pointer)(~s).startAddr));
    return s;
}

internal static void dematerializeGCProg(ж<mspan> Ꮡs) {
    Ꮡmheap_.freeManual(Ꮡs, spanAllocPtrScalarBits);
}

internal static void dumpGCProg(ж<byte> Ꮡp) {
    ref var Δp = ref Ꮡp.DerefOrNull();

    nint nptr = 0;
    while (ᐧ) {
        var x = Δp;
        Ꮡp = add1(Ꮡp); Δp = ref Ꮡp.DerefOrNull();
        if (x == 0) {
            print((@string)"\t"u8, nptr, (@string)" end\n"u8);
            break;
        }
        if ((byte)(x & 0x80) == 0){
            print((@string)"\t"u8, nptr, (@string)" lit "u8, x, (@string)":"u8);
            nint n = (nint)(x + 7) / 8;
            for (nint i = 0; i < n; i++) {
                print((@string)" "u8, ((Δhex)(uint64)(Δp)));
                Ꮡp = add1(Ꮡp); Δp = ref Ꮡp.DerefOrNull();
            }
            print((@string)"\n"u8);
            nptr += (nint)x;
        } else {
            nint nbit = (nint)((byte)(x & ~0x80));
            if (nbit == 0) {
                for (nuint nb = (nuint)0; ᐧ ; nb += 7) {
                    var xΔ1 = Δp;
                    Ꮡp = add1(Ꮡp); Δp = ref Ꮡp.DerefOrNull();
                    nbit |= (nint)(((nint)((byte)(xΔ1 & 0x7f))).Lsh(nb));
                    if ((byte)(xΔ1 & 0x80) == 0) {
                        break;
                    }
                }
            }
            nint count = 0;
            for (nuint nb = (nuint)0; ᐧ ; nb += 7) {
                var xΔ2 = Δp;
                Ꮡp = add1(Ꮡp); Δp = ref Ꮡp.DerefOrNull();
                count |= (nint)(((nint)((byte)(xΔ2 & 0x7f))).Lsh(nb));
                if ((byte)(xΔ2 & 0x80) == 0) {
                    break;
                }
            }
            print((@string)"\t"u8, nptr, (@string)" repeat "u8, nbit, (@string)" × "u8, count, (@string)"\n"u8);
            nptr += nbit * count;
        }
    }
}

// Testing.

// reflect_gcbits returns the GC type info for x, for testing.
// The result is the bitmap entries (0 or 1), one entry per byte.
//
//go:linkname reflect_gcbits reflect.gcbits
internal static slice<byte> reflect_gcbits(any x) {
    return getgcmask(x);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badArgumentToGetgcmaskˢ = "bad argument to getgcmask: expected type to be a pointer to the value type whose mask is being queried"u8;
internal static readonly @string foundNonZeroedTailOfˢ = "found non-zeroed tail of allocation"u8;
internal static readonly @string foundTwoDifferentMasksˢ = "found two different masks from two different methods"u8;

// Returns GC type info for the pointer stored in ep for testing.
// If ep points to the stack, only static live information will be returned
// (i.e. not for objects which are only dynamically live stack objects).
internal static slice<byte> /*mask*/ getgcmask(any epʗp) {
    slice<byte> mask = default!;

    ref var ep = ref heap(epʗp, out var Ꮡep);
    var e = efaceOf(Ꮡep).Value;
    @unsafe.Pointer Δp = e.data;
    var t = e._type;
    ж<_type> et = default!;
    if ((abiꓸKind)((~t).Kind_ & abi.KindMask) != abi.Pointer) {
        @throw(badArgumentToGetgcmaskˢ);
    }
    et = (t.Reinterpret<_type, ptrtype>()).Value.Elem;
    // data or bss
    foreach (var (_, datap) in activeModules()) {
        // data
        if ((~datap).data <= (uintptr)Δp && (uintptr)Δp < (~datap).edata) {
            var bitmap = datap.Value.gcdatamask.bytedata;
            var n = et.Value.Size_;
            mask = new slice<byte>((nint)(n / (uintptr)goarch.PtrSize));
            for (var i = (uintptr)0; i < n; i += goarch.PtrSize) {
                var off = ((uintptr)Δp + i - (~datap).data) / (uintptr)goarch.PtrSize;
                mask[(nint)(i / (uintptr)goarch.PtrSize)] = (byte)(((addb(bitmap, off / 8).Value >> (int)((off % 8)))) & 1);
            }
            return mask;
        }
        // bss
        if ((~datap).bss <= (uintptr)Δp && (uintptr)Δp < (~datap).ebss) {
            var bitmap = datap.Value.gcbssmask.bytedata;
            var n = et.Value.Size_;
            mask = new slice<byte>((nint)(n / (uintptr)goarch.PtrSize));
            for (var i = (uintptr)0; i < n; i += goarch.PtrSize) {
                var off = ((uintptr)Δp + i - (~datap).bss) / (uintptr)goarch.PtrSize;
                mask[(nint)(i / (uintptr)goarch.PtrSize)] = (byte)(((addb(bitmap, off / 8).Value >> (int)((off % 8)))) & 1);
            }
            return mask;
        }
    }
    // heap
    {
        var (@base, s, _) = findObject((uintptr)Δp, 0, 0); if (@base != 0) {
            if ((~s).spanclass.noscan()) {
                return default!;
            }
            var limit = @base + (~s).elemsize;
            // Move the base up to the iterator's start, because
            // we want to hide evidence of a malloc header from the
            // caller.
            var tp = s.typePointersOfUnchecked(@base);
            @base = tp.addr;
            // Unroll the full bitmap the GC would actually observe.
            var maskFromHeap = new slice<byte>((nint)((limit - @base) / (uintptr)goarch.PtrSize));
            while (ᐧ) {
                uintptr addr = default!;
                {
                    (tp, addr) = tp.next(limit); if (addr == 0) {
                        break;
                    }
                }
                maskFromHeap[(nint)((addr - @base) / (uintptr)goarch.PtrSize)] = 1;
            }
            // Double-check that every part of the ptr/scalar we're not
            // showing the caller is zeroed. This keeps us honest that
            // that information is actually irrelevant.
            for (var i = limit; i < (~s).elemsize; i++) {
                if (~(ж<byte>)(uintptr)((@unsafe.Pointer)i) != 0) {
                    @throw(foundNonZeroedTailOfˢ);
                }
            }
            // Callers (and a check we're about to run) expects this mask
            // to end at the last pointer.
            while (len(maskFromHeap) > 0 && maskFromHeap[len(maskFromHeap) - 1] == 0) {
                maskFromHeap = maskFromHeap[..(int)(len(maskFromHeap) - 1)];
            }
            if ((abiꓸKind)((~et).Kind_ & abi.KindGCProg) == 0) {
                // Unroll again, but this time from the type information.
                var maskFromType = new slice<byte>((nint)((limit - @base) / (uintptr)goarch.PtrSize));
                tp = s.typePointersOfType(et, @base);
                while (ᐧ) {
                    uintptr addr = default!;
                    {
                        (tp, addr) = tp.next(limit); if (addr == 0) {
                            break;
                        }
                    }
                    maskFromType[(nint)((addr - @base) / (uintptr)goarch.PtrSize)] = 1;
                }
                // Validate that the prefix of maskFromType is equal to
                // maskFromHeap. maskFromType may contain more pointers than
                // maskFromHeap produces because maskFromHeap may be able to
                // get exact type information for certain classes of objects.
                // With maskFromType, we're always just tiling the type bitmap
                // through to the elemsize.
                //
                // It's OK if maskFromType has pointers in elemsize that extend
                // past the actual populated space; we checked above that all
                // that space is zeroed, so just the GC will just see nil pointers.
                var differs = false;
                foreach (var (i, _) in maskFromHeap) {
                    if (maskFromHeap[i] != maskFromType[i]) {
                        differs = true;
                        break;
                    }
                }
                if (differs) {
                    print((@string)"runtime: heap mask="u8);
                    foreach (var (_, b) in maskFromHeap) {
                        print(b);
                    }
                    println();
                    print((@string)"runtime: type mask="u8);
                    foreach (var (_, b) in maskFromType) {
                        print(b);
                    }
                    println();
                    print((@string)"runtime: type="u8, toRType(et).@string(), (@string)"\n"u8);
                    @throw(foundTwoDifferentMasksˢ);
                }
            }
            // Select the heap mask to return. We may not have a type mask.
            mask = maskFromHeap;
            // Make sure we keep ep alive. We may have stopped referencing
            // ep's data pointer sometime before this point and it's possible
            // for that memory to get freed.
            KeepAlive(ep);
            return mask;
        }
    }
    // stack
    {
        var gp = getg(); if ((~(~(~gp).m).curg).stack.lo <= (uintptr)Δp && (uintptr)Δp < (~(~(~gp).m).curg).stack.hi) {
            var found = false;
            ref var u = ref heap(new unwinder(), out var Ꮡu);
            for (Ꮡu.initAt((~(~(~gp).m).curg).sched.pc, (~(~(~gp).m).curg).sched.sp, 0, (~(~gp).m).curg, 0); u.valid(); Ꮡu.next()) {
                if (u.frame.sp <= (uintptr)Δp && (uintptr)Δp < u.frame.varp) {
                    found = true;
                    break;
                }
            }
            if (found) {
                var (locals, _, _) = u.frame.getStackMap(false);
                if (locals.n == 0) {
                    return mask;
                }
                var size = (uintptr)locals.n * (uintptr)goarch.PtrSize;
                var n = (t.Reinterpret<_type, ptrtype>()).Value.Elem.Value.Size_;
                mask = new slice<byte>((nint)(n / (uintptr)goarch.PtrSize));
                for (var i = (uintptr)0; i < n; i += goarch.PtrSize) {
                    var off = ((uintptr)Δp + i - u.frame.varp + size) / (uintptr)goarch.PtrSize;
                    mask[(nint)(i / (uintptr)goarch.PtrSize)] = locals.ptrbit(off);
                }
            }
            return mask;
        }
    }
    // otherwise, not something the GC knows about.
    // possibly read-only data, like malloc(0).
    // must not have pointers
    return mask;
}

} // end runtime_package
