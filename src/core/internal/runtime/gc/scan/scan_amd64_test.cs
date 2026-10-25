// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64
namespace go.@internal.runtime.gc;

using scan = go.@internal.runtime.gc.scan_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using gc = go.@internal.runtime.gc_package;
using go.@internal.runtime.gc;

partial class scan_test_package {

public static partial void TestScanSpanPackedAVX512(ж<testing.T> Ꮡt) {
    if (!scan.CanAVX512()) {
        Ꮡt.Skip(noAvx512ˢ);
    }
    testScanSpanPacked(Ꮡt, new Func<@unsafe.Pointer, ж<uintptr>, ж<gc.ObjMask>, uintptr, ж<gc.PtrMask>, int32>(scan.ScanSpanPackedAVX512));
}

} // end scan_test_package
