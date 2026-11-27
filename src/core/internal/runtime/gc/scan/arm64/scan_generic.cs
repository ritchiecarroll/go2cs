// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !amd64
namespace go.@internal.runtime.gc;

using gc = go.@internal.runtime.gc_package;
using @unsafe = unsafe_package;
using go.@internal.runtime;

partial class scan_package {

public static bool HasFastScanSpanPacked() {
    // N.B. ScanSpanPackedGeneric isn't actually fast enough to serve as a general-purpose implementation.
    // The runtime's alternative of jumping between each object is still substantially better, even at
    // relatively high object densities.
    return false;
}

public static int32 /*count*/ ScanSpanPacked(@unsafe.Pointer mem, ж<uintptr> bufp, ж<gc.ObjMask> objMarks, uintptr sizeClass, ж<gc.PtrMask> ptrMask) {
    return ScanSpanPackedGo(mem, bufp, objMarks, sizeClass, ptrMask);
}

} // end scan_package
