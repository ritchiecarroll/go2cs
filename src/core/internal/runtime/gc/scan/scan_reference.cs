// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc;

using goarch = go.@internal.goarch_package;
using gc = go.@internal.runtime.gc_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.runtime;

partial class scan_package {

// ScanSpanPackedReference is the reference implementation of ScanScanPacked. It prioritizes clarity over performance.
//
// Concretely, ScanScanPacked functions read pointers from mem, assumed to be gc.PageSize-aligned and gc.PageSize in size,
// and writes them to bufp, which is large enough to guarantee that even if pointer-word of mem is a pointer, it will fit.
// Therefore bufp, is always at least gc.PageSize in size.
//
// ScanSpanPacked is supposed to identify pointers by first filtering words by objMarks, where each bit of the mask
// represents gc.SizeClassToSize[sizeClass] bytes of memory, and then filtering again by the bits in ptrMask.
public static int32 /*count*/ ScanSpanPackedReference(@unsafe.Pointer mem, ж<uintptr> Ꮡbufp, ж<gc.ObjMask> ᏑobjMarks, uintptr sizeClass, ж<gc.PtrMask> ᏑptrMask) {
    int32 count = default!;
    ref var objMarks = ref ᏑobjMarks.DerefOrNull();
    ref var ptrMask = ref ᏑptrMask.DerefOrNull();

    var buf = @unsafe.Slice(Ꮡbufp, gc.PageWords);
    var expandBy = (uintptr)gc.SizeClassToSize[sizeClass] / (uintptr)goarch.PtrSize;
    foreach (var word in range(gc.PageWords)) {
        var objI = (uintptr)word / expandBy;
        if ((uintptr)(objMarks[objI / (uintptr)goarch.PtrBits] & (((uintptr)1 << (int)((objI % (uintptr)goarch.PtrBits))))) == 0) {
            continue;
        }
        if ((uintptr)(ptrMask[word / (nint)goarch.PtrBits] & (((uintptr)1).Lsh((int64)((word % (nint)goarch.PtrBits))))) == 0) {
            continue;
        }
        var ptr = ~(ж<uintptr>)(uintptr)((uintptr)@unsafe.Add(mem, word * (nint)goarch.PtrSize));
        if (ptr == 0) {
            continue;
        }
        buf[count] = ptr;
        count++;
    }
    return count;
}

} // end scan_package
