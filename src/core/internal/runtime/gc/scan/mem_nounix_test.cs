// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !unix
namespace go.@internal.runtime.gc;

using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;

partial class scan_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mmapUnsupportedˢ = (@string)"mmap unsupported"u8;

internal static partial (slice<uintptr>, Action) makeMem(testing.TB t, nint nPages) {
    t.Skip(mmapUnsupportedˢ);
    return (default!, default!);
}

} // end scan_test_package
