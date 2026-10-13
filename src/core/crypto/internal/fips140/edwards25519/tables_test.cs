// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using testing = testing_package;
using static go.crypto.@internal.fips140.edwards25519_package;

partial class edwards25519_internal_test_package {

public static void TestProjLookupTable(ж<testing.T> Ꮡt) {
    global::go.crypto.@internal.fips140.edwards25519_package.projLookupTable table = new();
    table.FromP3(B);

    ref var tmp1 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.projCached(), out var Ꮡtmp1);
    ref var tmp2 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.projCached(), out var Ꮡtmp2);
    ref var tmp3 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.projCached(), out var Ꮡtmp3);
    table.SelectInto(Ꮡtmp1, 6);
    table.SelectInto(Ꮡtmp2, (int8)(-2));
    table.SelectInto(Ꮡtmp3, (int8)(-4));
    // Expect T1 + T2 + T3 = identity

    ref var accP1xP1 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.projP1xP1(), out var ᏑaccP1xP1);
    var accP3 = NewIdentityPoint();

    ᏑaccP1xP1.Add(accP3, Ꮡtmp1);
    accP3.fromP1xP1(ᏑaccP1xP1);
    ᏑaccP1xP1.Add(accP3, Ꮡtmp2);
    accP3.fromP1xP1(ᏑaccP1xP1);
    ᏑaccP1xP1.Add(accP3, Ꮡtmp3);
    accP3.fromP1xP1(ᏑaccP1xP1);

    if (accP3.Equal(I) != 1) {
        Ꮡt.Errorf("Consistency check on ProjLookupTable.SelectInto failed!  %x %x %x"u8, tmp1, tmp2, tmp3);
    }
}

public static void TestAffineLookupTable(ж<testing.T> Ꮡt) {
    global::go.crypto.@internal.fips140.edwards25519_package.affineLookupTable table = new();
    table.FromP3(B);

    ref var tmp1 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.affineCached(), out var Ꮡtmp1);
    ref var tmp2 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.affineCached(), out var Ꮡtmp2);
    ref var tmp3 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.affineCached(), out var Ꮡtmp3);
    table.SelectInto(Ꮡtmp1, 3);
    table.SelectInto(Ꮡtmp2, (int8)(-7));
    table.SelectInto(Ꮡtmp3, 4);
    // Expect T1 + T2 + T3 = identity

    ref var accP1xP1 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.projP1xP1(), out var ᏑaccP1xP1);
    var accP3 = NewIdentityPoint();

    ᏑaccP1xP1.AddAffine(accP3, Ꮡtmp1);
    accP3.fromP1xP1(ᏑaccP1xP1);
    ᏑaccP1xP1.AddAffine(accP3, Ꮡtmp2);
    accP3.fromP1xP1(ᏑaccP1xP1);
    ᏑaccP1xP1.AddAffine(accP3, Ꮡtmp3);
    accP3.fromP1xP1(ᏑaccP1xP1);

    if (accP3.Equal(I) != 1) {
        Ꮡt.Errorf("Consistency check on ProjLookupTable.SelectInto failed!  %x %x %x"u8, tmp1, tmp2, tmp3);
    }
}

public static void TestNafLookupTable5(ж<testing.T> Ꮡt) {
    global::go.crypto.@internal.fips140.edwards25519_package.nafLookupTable5 table = new();
    table.FromP3(B);

    ref var tmp1 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.projCached(), out var Ꮡtmp1);
    ref var tmp2 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.projCached(), out var Ꮡtmp2);
    ref var tmp3 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.projCached(), out var Ꮡtmp3);
    ref var tmp4 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.projCached(), out var Ꮡtmp4);
    table.SelectInto(Ꮡtmp1, 9);
    table.SelectInto(Ꮡtmp2, 11);
    table.SelectInto(Ꮡtmp3, 7);
    table.SelectInto(Ꮡtmp4, 13);
    // Expect T1 + T2 = T3 + T4

    ref var accP1xP1 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.projP1xP1(), out var ᏑaccP1xP1);
    var lhs = NewIdentityPoint();
    var rhs = NewIdentityPoint();

    ᏑaccP1xP1.Add(lhs, Ꮡtmp1);
    lhs.fromP1xP1(ᏑaccP1xP1);
    ᏑaccP1xP1.Add(lhs, Ꮡtmp2);
    lhs.fromP1xP1(ᏑaccP1xP1);

    ᏑaccP1xP1.Add(rhs, Ꮡtmp3);
    rhs.fromP1xP1(ᏑaccP1xP1);
    ᏑaccP1xP1.Add(rhs, Ꮡtmp4);
    rhs.fromP1xP1(ᏑaccP1xP1);

    if (lhs.Equal(rhs) != 1) {
        Ꮡt.Errorf("Consistency check on nafLookupTable5 failed"u8);
    }
}

public static void TestNafLookupTable8(ж<testing.T> Ꮡt) {
    global::go.crypto.@internal.fips140.edwards25519_package.nafLookupTable8 table = new();
    table.FromP3(B);

    ref var tmp1 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.affineCached(), out var Ꮡtmp1);
    ref var tmp2 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.affineCached(), out var Ꮡtmp2);
    ref var tmp3 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.affineCached(), out var Ꮡtmp3);
    ref var tmp4 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.affineCached(), out var Ꮡtmp4);
    table.SelectInto(Ꮡtmp1, 49);
    table.SelectInto(Ꮡtmp2, 11);
    table.SelectInto(Ꮡtmp3, 35);
    table.SelectInto(Ꮡtmp4, 25);
    // Expect T1 + T2 = T3 + T4

    ref var accP1xP1 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.projP1xP1(), out var ᏑaccP1xP1);
    var lhs = NewIdentityPoint();
    var rhs = NewIdentityPoint();

    ᏑaccP1xP1.AddAffine(lhs, Ꮡtmp1);
    lhs.fromP1xP1(ᏑaccP1xP1);
    ᏑaccP1xP1.AddAffine(lhs, Ꮡtmp2);
    lhs.fromP1xP1(ᏑaccP1xP1);

    ᏑaccP1xP1.AddAffine(rhs, Ꮡtmp3);
    rhs.fromP1xP1(ᏑaccP1xP1);
    ᏑaccP1xP1.AddAffine(rhs, Ꮡtmp4);
    rhs.fromP1xP1(ᏑaccP1xP1);

    if (lhs.Equal(rhs) != 1) {
        Ꮡt.Errorf("Consistency check on nafLookupTable8 failed"u8);
    }
}

} // end edwards25519_internal_test_package
