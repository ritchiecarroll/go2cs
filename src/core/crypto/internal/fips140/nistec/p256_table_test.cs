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

public static void TestP256PrecomputedTable(ж<testing.T> Ꮡt) {
    var @base = NewP256Point().SetGenerator();

    for (nint iᴛ1 = 0; iᴛ1 < 43; iᴛ1++) {
        var i = iᴛ1;
        var baseʗ1 = @base;
        Ꮡt.Run(fmt.Sprintf("table[%d]"u8, i), (ж<testing.T> tΔ1) => {
            testP256AffineTable(tΔ1, baseʗ1, p256GeneratorTables.at<global::go.crypto.@internal.fips140.nistec_package.p256AffineTable>(i));
        });

        for (nint k = 0; k < 6; k++) {
            @base.Double(@base);
        }
    }
}

internal static void testP256AffineTable(ж<testing.T> Ꮡt, ж<global::go.crypto.@internal.fips140.nistec_package.P256Point> Ꮡbase, ж<global::go.crypto.@internal.fips140.nistec_package.p256AffineTable> Ꮡtable) {
    ref var table = ref Ꮡtable.DerefOrNull();

    var p = NewP256Point();
    var zInv = @new<fiat.P256Element>();

    for (nint j = 0; j < 32; j++) {
        p.Add(p, Ꮡbase);

        // Convert p to affine coordinates.
        zInv.Invert(p.of(global::go.crypto.@internal.fips140.nistec_package.P256Point.Ꮡz));
        p.of(global::go.crypto.@internal.fips140.nistec_package.P256Point.Ꮡx).Mul(p.of(global::go.crypto.@internal.fips140.nistec_package.P256Point.Ꮡx), zInv);
        p.of(global::go.crypto.@internal.fips140.nistec_package.P256Point.Ꮡy).Mul(p.of(global::go.crypto.@internal.fips140.nistec_package.P256Point.Ꮡy), zInv);
        p.of(global::go.crypto.@internal.fips140.nistec_package.P256Point.Ꮡz).One();

        if (!bytes_package.Equal(Ꮡtable.at<global::go.crypto.@internal.fips140.nistec_package.p256AffinePoint>(j).of(global::go.crypto.@internal.fips140.nistec_package.p256AffinePoint.Ꮡx).Bytes(), p.of(global::go.crypto.@internal.fips140.nistec_package.P256Point.Ꮡx).Bytes()) || !bytes_package.Equal(Ꮡtable.at<global::go.crypto.@internal.fips140.nistec_package.p256AffinePoint>(j).of(global::go.crypto.@internal.fips140.nistec_package.p256AffinePoint.Ꮡy).Bytes(), p.of(global::go.crypto.@internal.fips140.nistec_package.P256Point.Ꮡy).Bytes())) {
            Ꮡt.Fatalf("incorrect table entry at index %d"u8, j);
        }
    }
}

} // end nistec_internal_test_package
