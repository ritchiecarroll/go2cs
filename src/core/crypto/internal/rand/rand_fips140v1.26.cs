// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !fips140v1.0
namespace go.crypto.@internal;

using drbg = go.crypto.@internal.fips140.drbg_package;
using io = io_package;
using go.crypto.@internal.fips140;

partial class rand_package {

internal static void fips140SetTestingReader(io.Reader r) {
    drbg.SetTestingReader(r);
}

} // end rand_package
