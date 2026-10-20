// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc;

using goarch = go.@internal.goarch_package;
using gc = go.@internal.runtime.gc_package;
using sys = go.@internal.runtime.sys_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.runtime;

partial class scan_package {

// ScanSpanPackedGo is an optimized pure Go implementation of ScanSpanPacked.
public static int32 /*count*/ ScanSpanPackedGo(@unsafe.Pointer mem, ж<uintptr> Ꮡbufp, ж<gc.ObjMask> ᏑobjMarks, uintptr sizeClass, ж<gc.PtrMask> ᏑptrMask) {
    ref var objMarks = ref ᏑobjMarks.DerefOrNull();
    ref var ptrMask = ref ᏑptrMask.DerefOrNull();

    var buf = newUnsafeBuf<uintptr>(Ꮡbufp);
    var objBytes = (uintptr)gc.SizeClassToSize[sizeClass];
    // TODO(austin): Trim objMarks to the number of objects in this size class?
    foreach (var (markI, vᴛ1) in objMarks) {
        var markWord = vᴛ1;

        foreach (var _ᴛ1 in range(sys.OnesCount64((uint64)markWord))) {
            nint bitI = sys.TrailingZeros64((uint64)markWord);
            markWord &= unchecked((uintptr)~(uintptr)(((uintptr)1).Lsh((int64)(bitI))));

            nint objIndex = markI * (nint)goarch.PtrBits + bitI;

            // objStartInSpan is the index of the word from mem where the
            // object stats. objEndInSpan points to the next object, i.e.
            // it's an exclusive upper bound.
            var objStartInSpan = objBytes * (uintptr)objIndex / (uintptr)goarch.PtrSize;
            var objEndInSpan = objStartInSpan + objBytes / (uintptr)goarch.PtrSize;

            // TODO: Another way to do this would be to extract the pointer mask
            // for this object (it's at most 64 bits) and do a bit iteration
            // over that.

            for (var wordI = objStartInSpan; wordI < objEndInSpan; wordI++) {
                var val = ~(ж<uintptr>)(uintptr)((uintptr)@unsafe.Add(mem, wordI * (uintptr)goarch.PtrSize));
                // Check if we should enqueue this word.
                //
                // We load the word before the check because, even though this
                // can lead to loading much more than necessary, it's faster.
                // Most likely this is because it warms up the hardware
                // prefetcher much better, and gives us more time before we need
                // the value.
                //
                // We discard values that can't possibly be useful pointers
                // here, too, because this filters out a lot of words and does
                // so with as little processing as possible.
                //
                // TODO: This is close to, but not entirely branchless.
                nint isPtr = bool2int((uintptr)(ptrMask[wordI / (uintptr)goarch.PtrBits] & ((uintptr)1 << (int)((wordI % (uintptr)goarch.PtrBits)))) != 0);
                nint isNonNil = bool2int(val >= 4096);
                var pred = (isPtr & isNonNil) != 0;
                buf.addIf(val, pred);
            }
        }
    }
    // We don't know the true size of bufp, but we can at least catch obvious errors
    // in this function by making sure we didn't write more than gc.PageWords pointers
    // into the buffer.
    buf.check(gc.PageWords);
    return (int32)buf.n;
}

// unsafeBuf allows for appending to a buffer without bounds-checks or branches.
partial struct unsafeBuf<T> {
    internal ж<T> @base;
    internal nint n;
}

internal static unsafeBuf<T> newUnsafeBuf<T>(ж<T> Ꮡbase) {
    return new unsafeBuf<T>(Ꮡbase, 0);
}

// addIf appends a value to the buffer if the predicate is true.
//
// addIf speculatively writes to the next index of the buffer, so the caller
// must be certain that such a write will still be in-bounds with respect
// to the buffer's true capacity.
internal static void addIf<T>(this ref unsafeBuf<T> b, T val, bool pred) {
    ((ж<T>)(uintptr)(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(b.@base), b.n * (nint)@unsafe.Sizeof(val)))).ValueSlot = val;
    b.n += bool2int(pred);
}

// check performs a bounds check on speculative writes into the buffer.
// Calling this shortly after a series of addIf calls is important to
// catch any misuse as fast as possible. Separating the bounds check from
// the append is more efficient, but one check to cover several appends is
// still efficient and much more memory safe.
internal static void check<T>(this unsafeBuf<T> b, nint cap) {
    // We fail even if b.n == cap because addIf speculatively writes one past b.n.
    if (b.n >= cap) {
        throw panic("unsafeBuf overflow");
    }
}

internal static nint bool2int(bool x) {
    // This particular pattern gets optimized by the compiler.
    nint b = default!;
    if (x) {
        b = 1;
    }
    return b;
}

} // end scan_package
