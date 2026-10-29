// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !arm64) || purego
namespace go.crypto.@internal.fips140;

using errors = errors_package;

partial class nistec_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unimplementedˢ = "unimplemented"u8;

public static (slice<byte>, error) P256OrdInverse(slice<byte> k) {
    return (default, errors.New(unimplementedˢ));
}

} // end nistec_package
