// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64
namespace go.@internal.runtime.gc;

using scan = go.@internal.runtime.gc.scan_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.@internal.runtime.gc;

partial class scan_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object avx512IsRequiredForˢ = (@string)"AVX512 is required for TestFilterNilAVX512"u8;

public static partial void TestFilterNilAVX512(ж<testing.T> Ꮡt) {
    if (!scan.CanAVX512()) {
        Ꮡt.Skip(avx512IsRequiredForˢ);
    }
    runTestFilterNil(Ꮡt, scan.FilterNilAVX512);
}

} // end scan_test_package
