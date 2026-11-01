// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using elliptic = go.crypto.elliptic_package;
using testing = testing_package;
using go.crypto;
using math;
using static go.crypto.@internal.fips140.ecdh_package;
using Δnistec = go.crypto.@internal.fips140.nistec_package;

partial class ecdh_internal_test_package {

public static void TestOrders(ж<testing.T> t) {
    if (!bytes.Equal(elliptic.P224().Params().N.Bytes(), P224().N)) {
        t.Errorf("P-224 order mismatch");
    }
    if (!bytes.Equal(elliptic.P256().Params().N.Bytes(), P256().N)) {
        t.Errorf("P-256 order mismatch");
    }
    if (!bytes.Equal(elliptic.P384().Params().N.Bytes(), P384().N)) {
        t.Errorf("P-384 order mismatch");
    }
    if (!bytes.Equal(elliptic.P521().Params().N.Bytes(), P521().N)) {
        t.Errorf("P-521 order mismatch");
    }
}

} // end ecdh_internal_test_package
