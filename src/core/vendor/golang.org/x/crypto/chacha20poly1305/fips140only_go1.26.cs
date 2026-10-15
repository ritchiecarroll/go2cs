// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build go1.26
namespace go.vendor.golang.org.x.crypto;

using fips140 = go.crypto.fips140_package;
using go.crypto;

partial class chacha20poly1305_package {

internal static bool fips140Enforced() {
    return fips140.Enforced();
}

} // end chacha20poly1305_package
