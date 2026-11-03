// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !arm64 && !ppc64le && !s390x) || purego
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fiat = go.crypto.@internal.fips140.nistec.fiat_package;
using fmt = fmt_package;
using testing = testing_package;
using go.crypto.@internal.fips140.nistec;
using static go.crypto.@internal.fips140.nistec_package;

partial class nistec_internal_test_package {

public static void TestP256PrecomputedTable(ж<testing.T> t) {
    var @base = NewP256Point().SetGenerator();

    for (nint iᴛ1 = 0; iᴛ1 < 43; iᴛ1++) {
        var i = iᴛ1;
        var baseʗ1 = @base;
        t.Run(fmt.Sprintf("table[%d]"u8, i), (ж<testing.T> tΔ1) => {
            testP256AffineTable(tΔ1, baseʗ1, p256GeneratorTables.at<global::go.crypto.@internal.fips140.nistec_package.p256AffineTable>(i));
        });

        for (nint k = 0; k < 6; k++) {
            @base.Double(@base);
        }
    }
}

internal static void testP256AffineTable(ж<testing.T> t, ж<global::go.crypto.@internal.fips140.nistec_package.P256Point> @base, ж<global::go.crypto.@internal.fips140.nistec_package.p256AffineTable> table) {
    var p = NewP256Point();
    var zInv = @new<fiat.P256Element>();

    for (nint j = 0; j < 32; j++) {
        p.Add(p, @base);

        // Convert p to affine coordinates.
        zInv.Invert(p.Ꮡz);
        p.Ꮡx.Mul(p.Ꮡx, zInv);
        p.Ꮡy.Mul(p.Ꮡy, zInv);
        p.Ꮡz.One();

        if (!bytes_package.Equal(table.at<global::go.crypto.@internal.fips140.nistec_package.p256AffinePoint>(j).Ꮡx.Bytes(), p.Ꮡx.Bytes()) || !bytes_package.Equal(table.at<global::go.crypto.@internal.fips140.nistec_package.p256AffinePoint>(j).Ꮡy.Bytes(), p.Ꮡy.Bytes())) {
            t.Fatalf("incorrect table entry at index %d", j);
        }
    }
}

} // end nistec_internal_test_package
