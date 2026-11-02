// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc;

using cpu = go.@internal.cpu_package;
using gc = go.@internal.runtime.gc_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.runtime;

partial class scan_package {

public static int32 /*count*/ ScanSpanPacked(@unsafe.Pointer mem, ж<uintptr> bufp, ж<gc.ObjMask> objMarks, uintptr sizeClass, ж<gc.PtrMask> ptrMask) {
    if (CanAVX512()) {
        return ScanSpanPackedAVX512(mem, bufp, objMarks, sizeClass, ptrMask);
    }
    throw panic("not implemented");
}

public static bool HasFastScanSpanPacked() {
    return avx512ScanPackedReqsMet;
}

// -- AVX512 --
public static bool CanAVX512() {
    return avx512ScanPackedReqsMet;
}

public static int32 /*count*/ ScanSpanPackedAVX512(@unsafe.Pointer mem, ж<uintptr> bufp, ж<gc.ObjMask> objMarks, uintptr sizeClass, ж<gc.PtrMask> ptrMask) {
    return FilterNilAVX512(bufp, scanSpanPackedAVX512(mem, bufp, objMarks, sizeClass, ptrMask));
}

//go:noescape
internal static partial int32 /*count*/ scanSpanPackedAVX512(@unsafe.Pointer mem, ж<uintptr> bufp, ж<gc.ObjMask> objMarks, uintptr sizeClass, ж<gc.PtrMask> ptrMask);

internal static bool avx512ScanPackedReqsMet = cpu.X86.HasAVX512VL && cpu.X86.HasAVX512BW && cpu.X86.HasGFNI && cpu.X86.HasAVX512BITALG && cpu.X86.HasAVX512DQ && // for kmovb, see #79871
    cpu.X86.HasAVX512VBMI;

} // end scan_package
