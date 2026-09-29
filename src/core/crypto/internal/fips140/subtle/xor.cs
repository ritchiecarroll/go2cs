// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using alias = go.crypto.@internal.fips140.alias_package;
using go.crypto.@internal.fips140;

partial class subtle_package {

// XORBytes sets dst[i] = x[i] ^ y[i] for all i < n = min(len(x), len(y)),
// returning n, the number of bytes written to dst.
//
// If dst does not have length at least n,
// XORBytes panics without writing anything to dst.
//
// dst and x or y may overlap exactly or not at all,
// otherwise XORBytes may panic.
public static nint XORBytes(slice<byte> dst, slice<byte> x, slice<byte> y) {
    nint n = min(len(x), len(y));
    if (n == 0) {
        return 0;
    }
    if (n > len(dst)) {
        throw panic("subtle.XORBytes: dst too short");
    }
    if (alias.InexactOverlap(dst.slice(0, n), x.slice(0, n)) || alias.InexactOverlap(dst.slice(0, n), y.slice(0, n))) {
        throw panic("subtle.XORBytes: invalid overlap");
    }
    xorBytes(Ꮡ(dst, 0), Ꮡ(x, 0), Ꮡ(y, 0), n); // arch-specific
    return n;
}

} // end subtle_package
