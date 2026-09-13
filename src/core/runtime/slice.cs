// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using Δmath = runtime.@internal.math_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using runtime.@internal;

partial class runtime_package {

[GoType] partial struct Δsliceᴛ {
    internal @unsafe.Pointer Δarray;
    internal nint len;
    internal nint cap;
}

// A notInHeapSlice is a slice backed by runtime/internal/sys.NotInHeap memory.
[GoType] partial struct notInHeapSlice {
    internal ж<notInHeap> Δarray;
    internal nint len;
    internal nint cap;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string makesliceLenOutOfRangeˢ = "makeslice: len out of range"u8;

internal static void panicmakeslicelen() {
    throw panic(((errorString)(@string)makesliceLenOutOfRangeˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string makesliceCapOutOfRangeˢ = "makeslice: cap out of range"u8;

internal static void panicmakeslicecap() {
    throw panic(((errorString)(@string)makesliceCapOutOfRangeˢ));
}

// makeslicecopy allocates a slice of "tolen" elements of type "et",
// then copies "fromlen" elements of type "et" into that new allocation from "from".
internal static @unsafe.Pointer makeslicecopy(ж<_type> Ꮡet, nint tolen, nint fromlen, @unsafe.Pointer from) {
    ref var et = ref Ꮡet.DerefOrNull();

    uintptr tomem = default!;
    uintptr copymem = default!;
    if ((uintptr)tolen > (uintptr)fromlen){
        bool overflow = default!;
        (tomem, overflow) = Δmath.MulUintptr(et.Size_, (uintptr)tolen);
        if (overflow || tomem > maxAlloc || tolen < 0) {
            panicmakeslicelen();
        }
        copymem = et.Size_ * (uintptr)fromlen;
    } else {
        // fromlen is a known good length providing and equal or greater than tolen,
        // thereby making tolen a good slice length too as from and to slices have the
        // same element width.
        tomem = et.Size_ * (uintptr)tolen;
        copymem = tomem;
    }
    @unsafe.Pointer to = default!;
    if (!et.Pointers()){
        to = (uintptr)mallocgc(tomem, nil, false);
        if (copymem < tomem) {
            memclrNoHeapPointers((uintptr)add(to, copymem), tomem - copymem);
        }
    } else {
        // Note: can't use rawmem (which avoids zeroing of memory), because then GC can scan uninitialized memory.
        to = (uintptr)mallocgc(tomem, Ꮡet, true);
        if (copymem > 0 && writeBarrier.enabled) {
            // Only shade the pointers in old.array since we know the destination slice to
            // only contains nil pointers because it has been cleared during alloc.
            //
            // It's safe to pass a type to this function as an optimization because
            // from and to only ever refer to memory representing whole values of
            // type et. See the comment on bulkBarrierPreWrite.
            bulkBarrierPreWriteSrcOnly((uintptr)to, (uintptr)from, copymem, Ꮡet);
        }
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        var pc = abi.FuncPCABIInternal(makeslicecopy);
        racereadrangepc(from, copymem, callerpc, pc);
    }
    if (msanenabled) {
        msanread(from, copymem);
    }
    if (asanenabled) {
        asanread(from, copymem);
    }
    memmove(to, from, copymem);
    return to;
}

// makeslice should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname makeslice
internal static @unsafe.Pointer makeslice(ж<_type> Ꮡet, nint len, nint cap) {
    ref var et = ref Ꮡet.DerefOrNull();

    var (mem, overflow) = Δmath.MulUintptr(et.Size_, (uintptr)cap);
    if (overflow || mem > maxAlloc || len < 0 || len > cap) {
        // NOTE: Produce a 'len out of range' error instead of a
        // 'cap out of range' error when someone does make([]T, bignumber).
        // 'cap out of range' is true too, but since the cap is only being
        // supplied implicitly, saying len is clearer.
        // See golang.org/issue/4085.
        var (memΔ1, overflowΔ1) = Δmath.MulUintptr(et.Size_, (uintptr)len);
        if (overflowΔ1 || memΔ1 > maxAlloc || len < 0) {
            panicmakeslicelen();
        }
        panicmakeslicecap();
    }
    return (uintptr)mallocgc(mem, Ꮡet, true);
}

internal static @unsafe.Pointer makeslice64(ж<_type> Ꮡet, int64 len64, int64 cap64) {
    nint len = (nint)len64;
    if ((int64)len != len64) {
        panicmakeslicelen();
    }
    nint cap = (nint)cap64;
    if ((int64)cap != cap64) {
        panicmakeslicecap();
    }
    return (uintptr)makeslice(Ꮡet, len, cap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string growsliceLenOutOfRangeˢ = "growslice: len out of range"u8;

// growslice allocates new backing store for a slice.
//
// arguments:
//
//	oldPtr = pointer to the slice's backing array
//	newLen = new length (= oldLen + num)
//	oldCap = original slice's capacity.
//	   num = number of elements being added
//	    et = element type
//
// return values:
//
//	newPtr = pointer to the new backing store
//	newLen = same value as the argument
//	newCap = capacity of the new backing store
//
// Requires that uint(newLen) > uint(oldCap).
// Assumes the original slice length is newLen - num
//
// A new backing store is allocated with space for at least newLen elements.
// Existing entries [0, oldLen) are copied over to the new backing store.
// Added entries [oldLen, newLen) are not initialized by growslice
// (although for pointer-containing element types, they are zeroed). They
// must be initialized by the caller.
// Trailing entries [newLen, newCap) are zeroed.
//
// growslice's odd calling convention makes the generated code that calls
// this function simpler. In particular, it accepts and returns the
// new length so that the old length is not live (does not need to be
// spilled/restored) and the new length is returned (also does not need
// to be spilled/restored).
//
// growslice should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/chenzhuoyu/iasm
//   - github.com/cloudwego/dynamicgo
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname growslice
internal static Δsliceᴛ growslice(@unsafe.Pointer oldPtr, nint newLen, nint oldCap, nint num, ж<_type> Ꮡet) {
    ref var et = ref Ꮡet.DerefOrNull();

    nint oldLen = newLen - num;
    if (raceenabled) {
        var callerpc = getcallerpc();
        racereadrangepc(oldPtr, (uintptr)(oldLen * (nint)et.Size_), callerpc, abi.FuncPCABIInternal(growslice));
    }
    if (msanenabled) {
        msanread(oldPtr, (uintptr)(oldLen * (nint)et.Size_));
    }
    if (asanenabled) {
        asanread(oldPtr, (uintptr)(oldLen * (nint)et.Size_));
    }
    if (newLen < 0) {
        throw panic(((errorString)(@string)growsliceLenOutOfRangeˢ));
    }
    if (et.Size_ == 0) {
        // append should not create a slice with nil pointer but non-zero len.
        // We assume that append doesn't need to preserve oldPtr in this case.
        return new Δsliceᴛ(@unsafe.Pointer.FromBox(Ꮡzerobase), newLen, newLen);
    }
    nint newcap = nextslicecap(newLen, oldCap);
    bool overflow = default!;
    uintptr lenmem = default!;
    uintptr newlenmem = default!;
    uintptr capmem = default!;
    // Specialize for common values of et.Size.
    // For 1 we don't need any division/multiplication.
    // For goarch.PtrSize, compiler will optimize division/multiplication into a shift by a constant.
    // For powers of 2, use a variable shift.
    var noscan = !et.Pointers();
    switch (ᐧ) {
    case {} when et.Size_ == 1: {
        lenmem = (uintptr)oldLen;
        newlenmem = (uintptr)newLen;
        capmem = roundupsize((uintptr)newcap, noscan);
        overflow = (uintptr)newcap > maxAlloc;
        newcap = (nint)capmem;
        break;
    }
    case {} when et.Size_ == goarch.PtrSize: {
        lenmem = (uintptr)oldLen * (uintptr)goarch.PtrSize;
        newlenmem = (uintptr)newLen * (uintptr)goarch.PtrSize;
        capmem = roundupsize((uintptr)newcap * (uintptr)goarch.PtrSize, noscan);
        overflow = (uintptr)newcap > (uintptr)(maxAlloc / goarch.PtrSize);
        newcap = (nint)(capmem / (uintptr)goarch.PtrSize);
        break;
    }
    case {} when isPowerOfTwo(et.Size_): {
        uintptr shift = default!;
        if (goarch.PtrSize == 8){
            // Mask shift for better code generation.
            shift = (uintptr)((uintptr)sys.TrailingZeros64((uint64)et.Size_) & 63);
        } else {
            shift = (uintptr)((uintptr)sys.TrailingZeros32((uint32)et.Size_) & 31);
        }
        lenmem = ((uintptr)oldLen).Lsh((uint64)(shift));
        newlenmem = ((uintptr)newLen).Lsh((uint64)(shift));
        capmem = roundupsize(((uintptr)newcap).Lsh((uint64)(shift)), noscan);
        overflow = (uintptr)newcap > (((uintptr)maxAlloc).Rsh((uint64)(shift)));
        newcap = (nint)(capmem.Rsh((uint64)(shift)));
        capmem = ((uintptr)newcap).Lsh((uint64)(shift));
        break;
    }
    default: {
        lenmem = (uintptr)oldLen * et.Size_;
        newlenmem = (uintptr)newLen * et.Size_;
        (capmem, overflow) = Δmath.MulUintptr(et.Size_, (uintptr)newcap);
        capmem = roundupsize(capmem, noscan);
        newcap = (nint)(capmem / et.Size_);
        capmem = (uintptr)newcap * et.Size_;
        break;
    }}

    // The check of overflow in addition to capmem > maxAlloc is needed
    // to prevent an overflow which can be used to trigger a segfault
    // on 32bit architectures with this example program:
    //
    // type T [1<<27 + 1]int64
    //
    // var d T
    // var s []T
    //
    // func main() {
    //   s = append(s, d, d, d, d)
    //   print(len(s), "\n")
    // }
    if (overflow || capmem > maxAlloc) {
        throw panic(((errorString)(@string)growsliceLenOutOfRangeˢ));
    }
    @unsafe.Pointer Δp = default!;
    if (!et.Pointers()){
        Δp = (uintptr)mallocgc(capmem, nil, false);
        // The append() that calls growslice is going to overwrite from oldLen to newLen.
        // Only clear the part that will not be overwritten.
        // The reflect_growslice() that calls growslice will manually clear
        // the region not cleared here.
        memclrNoHeapPointers((uintptr)add(Δp, newlenmem), capmem - newlenmem);
    } else {
        // Note: can't use rawmem (which avoids zeroing of memory), because then GC can scan uninitialized memory.
        Δp = (uintptr)mallocgc(capmem, Ꮡet, true);
        if (lenmem > 0 && writeBarrier.enabled) {
            // Only shade the pointers in oldPtr since we know the destination slice p
            // only contains nil pointers because it has been cleared during alloc.
            //
            // It's safe to pass a type to this function as an optimization because
            // from and to only ever refer to memory representing whole values of
            // type et. See the comment on bulkBarrierPreWrite.
            bulkBarrierPreWriteSrcOnly((uintptr)Δp, (uintptr)oldPtr, lenmem - et.Size_ + et.PtrBytes, Ꮡet);
        }
    }
    memmove(Δp, oldPtr, lenmem);
    return new Δsliceᴛ(Δp.Value, newLen, newcap);
}

// nextslicecap computes the next appropriate slice length.
internal static nint nextslicecap(nint newLen, nint oldCap) {
    nint newcap = oldCap;
    nint doublecap = newcap + newcap;
    if (newLen > doublecap) {
        return newLen;
    }
    UntypedInt threshold = 256;
    if (oldCap < threshold) {
        return doublecap;
    }
    while (ᐧ) {
        // Transition from growing 2x for small slices
        // to growing 1.25x for large slices. This formula
        // gives a smooth-ish transition between the two.
        newcap += ((newcap + (nint)(3 * threshold)) >> (int)(2));
        // We need to check `newcap >= newLen` and whether `newcap` overflowed.
        // newLen is guaranteed to be larger than zero, hence
        // when newcap overflows then `uint(newcap) > uint(newLen)`.
        // This allows to check for both with the same comparison.
        if ((nuint)newcap >= (nuint)newLen) {
            break;
        }
    }
    // Set newcap to the requested cap when
    // the newcap calculation overflowed.
    if (newcap <= 0) {
        return newLen;
    }
    return newcap;
}

// reflect_growslice should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/dynamicgo
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_growslice reflect.growslice
internal static Δsliceᴛ reflect_growslice(ж<_type> Ꮡet, Δsliceᴛ old, nint num) {
    ref var et = ref Ꮡet.DerefOrNull();

    // Semantically equivalent to slices.Grow, except that the caller
    // is responsible for ensuring that old.len+num > old.cap.
    num -= old.cap - old.len; // preserve memory of old[old.len:old.cap]
    var @new = growslice(old.Δarray, old.cap + num, old.cap, num, Ꮡet);
    // growslice does not zero out new[old.cap:new.len] since it assumes that
    // the memory will be overwritten by an append() that called growslice.
    // Since the caller of reflect_growslice is not append(),
    // zero out this region before returning the slice to the reflect package.
    if (!et.Pointers()) {
        var oldcapmem = (uintptr)old.cap * et.Size_;
        var newlenmem = (uintptr)@new.len * et.Size_;
        memclrNoHeapPointers((uintptr)add(@new.Δarray, oldcapmem), newlenmem - oldcapmem);
    }
    @new.len = old.len; // preserve the old length
    return @new;
}

internal static bool isPowerOfTwo(uintptr x) {
    return (uintptr)(x & (x - 1)) == 0;
}

// slicecopy is used to copy from a string or slice of pointerless elements into a slice.
internal static nint slicecopy(@unsafe.Pointer toPtr, nint toLen, @unsafe.Pointer fromPtr, nint fromLen, uintptr width) {
    if (fromLen == 0 || toLen == 0) {
        return 0;
    }
    nint n = fromLen;
    if (toLen < n) {
        n = toLen;
    }
    if (width == 0) {
        return n;
    }
    var size = (uintptr)n * width;
    if (raceenabled) {
        var callerpc = getcallerpc();
        var pc = abi.FuncPCABIInternal(slicecopy);
        racereadrangepc(fromPtr, size, callerpc, pc);
        racewriterangepc(toPtr, size, callerpc, pc);
    }
    if (msanenabled) {
        msanread(fromPtr, size);
        msanwrite(toPtr, size);
    }
    if (asanenabled) {
        asanread(fromPtr, size);
        asanwrite(toPtr, size);
    }
    if (size == 1){
        // common case worth about 2x to do here
        // TODO: is this still worth it with new memmove impl?
        ((ж<byte>)(uintptr)(toPtr)).Value = ((ж<byte>)(uintptr)(fromPtr)).Value; // known to be a byte pointer
    } else {
        memmove(toPtr, fromPtr, size);
    }
    return n;
}

//go:linkname bytealg_MakeNoZero internal/bytealg.MakeNoZero
internal static slice<byte> bytealg_MakeNoZero(nint len) {
    if ((uintptr)len > maxAlloc) {
        panicmakeslicelen();
    }
    var cap = roundupsize((uintptr)len, true);
    return @unsafe.Slice((ж<byte>)(uintptr)(mallocgc((uintptr)cap, nil, false)), cap)[..(int)(len)];
}

} // end runtime_package
