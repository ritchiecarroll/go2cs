// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using Δfips140 = go.crypto.@internal.fips140_package;
using testing = testing_package;
using go.crypto.@internal;

partial class cryptotest_package {

public static void MustSupportFIPS140(ж<testing.T> Ꮡt) {
    Ꮡt.Helper();
    {
        var err = Δfips140.Supported(); if (err != default!) {
            Ꮡt.Skipf("test requires FIPS 140 mode: %v"u8, err);
        }
    }
}

} // end cryptotest_package
