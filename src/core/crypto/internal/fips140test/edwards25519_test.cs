// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140test;

using cryptotest = go.crypto.@internal.cryptotest_package;
using static go.crypto.@internal.fips140.edwards25519_package;
using testing = testing_package;
using edwards25519 = go.crypto.@internal.fips140.edwards25519_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class fipstest_internal_test_package {

internal static byte testAllocationsSink;

public static void TestEdwards25519Allocations(ж<testing.T> Ꮡt) {
    cryptotest.SkipTestAllocations(Ꮡt);
    {
        var allocs = testing.AllocsPerRun(100, () => {
            var p = NewIdentityPoint();
            p.Add(p, NewGeneratorPoint());
            var s = NewScalar();
            testAllocationsSink ^= (byte)(s.Bytes()[0]);
            testAllocationsSink ^= (byte)(p.Bytes()[0]);
        }); if (allocs > 0D) {
            Ꮡt.Errorf("expected zero allocations, got %0.1v"u8, allocs);
        }
    }
}

} // end fipstest_internal_test_package
