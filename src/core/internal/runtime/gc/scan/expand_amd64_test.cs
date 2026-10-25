// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64
namespace go.@internal.runtime.gc;

using scan = go.@internal.runtime.gc.scan_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using gc = go.@internal.runtime.gc_package;
using go.@internal.runtime.gc;

partial class scan_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object noAvx512ˢ = (@string)"no AVX512"u8;

public static partial void TestExpandAVX512(ж<testing.T> Ꮡt) {
    if (!scan.CanAVX512()) {
        Ꮡt.Skip(noAvx512ˢ);
    }
    testExpand(Ꮡt, new Action<nint, ж<gc.ObjMask>, ж<gc.PtrMask>>(scan.ExpandAVX512));
}

} // end scan_test_package
