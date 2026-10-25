// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using testing = testing_package;
using quick = go.testing.quick_package;
using go.testing;
using static go.crypto.@internal.fips140.edwards25519_package;

partial class edwards25519_internal_test_package {

// a random scalar generated using dalek.
internal static ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> dalekScalar;
internal static error _ᴛ2ʗ;
internal static void initᴛdalekScalar() { dalekScalar = (Ꮡ(new Scalar(nil))).SetCanonicalBytes(new byte[]{219, 106, 114, 9, 174, 249, 155, 89, 69, 203, 201, 93, 92, 116, 234, 187, 78, 115, 103, 172, 182, 98, 62, 103, 187, 136, 13, 100, 248, 110, 12, 4}.slice()).Item1; }
// the above, times the edwards25519 basepoint.
internal static ж<global::go.crypto.@internal.fips140.edwards25519_package.Point> dalekScalarBasepoint;
internal static error _ᴛ3ʗ;
internal static void initᴛdalekScalarBasepoint() { dalekScalarBasepoint = @new<global::go.crypto.@internal.fips140.edwards25519_package.Point>().SetBytes(new byte[]{0xf4, 0xef, 0x7c, 0xa, 0x34, 0x55, 0x7b, 0x9f, 0x72, 0x3b, 0xb6, 0x1e, 0xf9, 0x46, 0x9, 0x91, 0x1c, 0xb9, 0xc0, 0x6c, 0x17, 0x28, 0x2d, 0x8b, 0x43, 0x2b, 0x5, 0x18, 0x6a, 0x54, 0x3e, 0x48}.slice()).Item1; }

public static void TestScalarMultSmallScalars(ж<testing.T> Ꮡt) {
    ref var z = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Scalar(), out var Ꮡz);
    ref var p = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡp);
    Ꮡp.ScalarMult(Ꮡz, B);
    if (I.Equal(Ꮡp) != 1) {
        Ꮡt.Error((@string)"0*B != 0"u8);
    }
    checkOnCurve(Ꮡt, Ꮡp);

    var (scEight, _) = (Ꮡ(new Scalar(nil))).SetCanonicalBytes(new byte[]{1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice());
    Ꮡp.ScalarMult(scEight, B);
    if (B.Equal(Ꮡp) != 1) {
        Ꮡt.Error((@string)"1*B != 1"u8);
    }
    checkOnCurve(Ꮡt, Ꮡp);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object scalarMulDoesNotMatchˢ = (@string)"Scalar mul does not match dalek"u8;

public static void TestScalarMultVsDalek(ж<testing.T> Ꮡt) {
    ref var p = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡp);
    Ꮡp.ScalarMult(dalekScalar, B);
    if (dalekScalarBasepoint.Equal(Ꮡp) != 1) {
        Ꮡt.Error(scalarMulDoesNotMatchˢ);
    }
    checkOnCurve(Ꮡt, Ꮡp);
}

public static void TestBaseMultVsDalek(ж<testing.T> Ꮡt) {
    ref var p = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡp);
    Ꮡp.ScalarBaseMult(dalekScalar);
    if (dalekScalarBasepoint.Equal(Ꮡp) != 1) {
        Ꮡt.Error(scalarMulDoesNotMatchˢ);
    }
    checkOnCurve(Ꮡt, Ꮡp);
}

public static void TestVarTimeDoubleBaseMultVsDalek(ж<testing.T> Ꮡt) {
    ref var p = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡp);
    ref var z = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Scalar(), out var Ꮡz);
    Ꮡp.VarTimeDoubleScalarBaseMult(dalekScalar, B, Ꮡz);
    if (dalekScalarBasepoint.Equal(Ꮡp) != 1) {
        Ꮡt.Error((@string)"VarTimeDoubleScalarBaseMult fails with b=0"u8);
    }
    checkOnCurve(Ꮡt, Ꮡp);
    Ꮡp.VarTimeDoubleScalarBaseMult(Ꮡz, B, dalekScalar);
    if (dalekScalarBasepoint.Equal(Ꮡp) != 1) {
        Ꮡt.Error((@string)"VarTimeDoubleScalarBaseMult fails with a=0"u8);
    }
    checkOnCurve(Ꮡt, Ꮡp);
}

public static void TestScalarMultDistributesOverAdd(ж<testing.T> Ꮡt) {
    var scalarMultDistributesOverAdd = (global::go.crypto.@internal.fips140.edwards25519_package.Scalar xʗp, global::go.crypto.@internal.fips140.edwards25519_package.Scalar yʗp) => {
        ref var x = ref heap(xʗp.ΔClone(), out var Ꮡx);
        ref var y = ref heap(yʗp.ΔClone(), out var Ꮡy);
        ref var z = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Scalar(), out var Ꮡz);
        Ꮡz.Add(Ꮡx, Ꮡy);
        ref var p = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡp);
        ref var q = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡq);
        ref var r = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡr);
        ref var check = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡcheck);
        Ꮡp.ScalarMult(Ꮡx, B);
        Ꮡq.ScalarMult(Ꮡy, B);
        Ꮡr.ScalarMult(Ꮡz, B);
        Ꮡcheck.Add(Ꮡp, Ꮡq);
        checkOnCurve(Ꮡt, Ꮡp, Ꮡq, Ꮡr, Ꮡcheck);
        return Ꮡcheck.Equal(Ꮡr) == 1;
    };

    {
        var err = quick.Check((scalarMultDistributesOverAdd).OrTypedNilFunc(), quickCheckConfig(32)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestScalarMultNonIdentityPoint(ж<testing.T> Ꮡt) {
    // Check whether p.ScalarMult and q.ScalaBaseMult give the same,
    // when p and q are originally set to the base point.

    var scalarMultNonIdentityPoint = (global::go.crypto.@internal.fips140.edwards25519_package.Scalar xʗp) => {
        ref var x = ref heap(xʗp.ΔClone(), out var Ꮡx);
        ref var p = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡp);
        ref var q = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡq);
        Ꮡp.Set(B);
        Ꮡq.Set(B);

        Ꮡp.ScalarMult(Ꮡx, B);
        Ꮡq.ScalarBaseMult(Ꮡx);

        checkOnCurve(Ꮡt, Ꮡp, Ꮡq);

        return Ꮡp.Equal(Ꮡq) == 1;
    };

    {
        var err = quick.Check((scalarMultNonIdentityPoint).OrTypedNilFunc(), quickCheckConfig(32)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestBasepointTableGeneration(ж<testing.T> Ꮡt) {
    // The basepoint table is 32 affineLookupTables,
    // corresponding to (16^2i)*B for table i.
    var basepointTableΔ1 = basepointTable();

    var tmp1 = Ꮡ(new projP1xP1(nil));
    var tmp2 = Ꮡ(new projP2(nil));
    var tmp3 = Ꮡ(new Point(nil));
    tmp3.Set(B);
    var table = new slice<global::go.crypto.@internal.fips140.edwards25519_package.affineLookupTable>(32, () => new());
    for (nint i = 0; i < 32; i++) {
        // Build the table
        table[i].FromP3(tmp3);
        // Assert equality with the hardcoded one
        if (table[i] != basepointTableΔ1.Value[i]) {
            Ꮡt.Errorf("Basepoint table %d does not match"u8, i);
        }

        // Set p = (16^2)*p = 256*p = 2^8*p
        tmp2.FromP3(tmp3);
        for (nint j = 0; j < 7; j++) {
            tmp1.Double(tmp2);
            tmp2.FromP1xP1(tmp1);
        }
        tmp1.Double(tmp2);
        tmp3.fromP1xP1(tmp1);
        checkOnCurve(Ꮡt, tmp3);
    }
}

public static void TestScalarMultMatchesBaseMult(ж<testing.T> Ꮡt) {
    var scalarMultMatchesBaseMult = (global::go.crypto.@internal.fips140.edwards25519_package.Scalar xʗp) => {
        ref var x = ref heap(xʗp.ΔClone(), out var Ꮡx);
        ref var p = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡp);
        ref var q = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡq);
        Ꮡp.ScalarMult(Ꮡx, B);
        Ꮡq.ScalarBaseMult(Ꮡx);
        checkOnCurve(Ꮡt, Ꮡp, Ꮡq);
        return Ꮡp.Equal(Ꮡq) == 1;
    };

    {
        var err = quick.Check((scalarMultMatchesBaseMult).OrTypedNilFunc(), quickCheckConfig(32)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object basepointNafTableDoesNotˢ = (@string)"BasepointNafTable does not match"u8;

public static void TestBasepointNafTableGeneration(ж<testing.T> Ꮡt) {
    global::go.crypto.@internal.fips140.edwards25519_package.nafLookupTable8 table = new();
    table.FromP3(B);

    if (table != basepointNafTable().Value) {
        Ꮡt.Error(basepointNafTableDoesNotˢ);
    }
}

public static void TestVarTimeDoubleBaseMultMatchesBaseMult(ж<testing.T> Ꮡt) {
    var varTimeDoubleBaseMultMatchesBaseMult = (global::go.crypto.@internal.fips140.edwards25519_package.Scalar xʗp, global::go.crypto.@internal.fips140.edwards25519_package.Scalar yʗp) => {
        ref var x = ref heap(xʗp.ΔClone(), out var Ꮡx);
        ref var y = ref heap(yʗp.ΔClone(), out var Ꮡy);
        ref var p = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡp);
        ref var q1 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡq1);
        ref var q2 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡq2);
        ref var check = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡcheck);

        Ꮡp.VarTimeDoubleScalarBaseMult(Ꮡx, B, Ꮡy);

        Ꮡq1.ScalarBaseMult(Ꮡx);
        Ꮡq2.ScalarBaseMult(Ꮡy);
        Ꮡcheck.Add(Ꮡq1, Ꮡq2);

        checkOnCurve(Ꮡt, Ꮡp, Ꮡcheck, Ꮡq1, Ꮡq2);
        return Ꮡp.Equal(Ꮡcheck) == 1;
    };

    {
        var err = quick.Check((varTimeDoubleBaseMultMatchesBaseMult).OrTypedNilFunc(), quickCheckConfig(32)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

// Benchmarks.
public static void BenchmarkScalarBaseMult(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var p = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡp);

    for (nint i = 0; i < b.N; i++) {
        Ꮡp.ScalarBaseMult(dalekScalar);
    }
}

public static void BenchmarkScalarMult(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var p = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡp);

    for (nint i = 0; i < b.N; i++) {
        Ꮡp.ScalarMult(dalekScalar, B);
    }
}

public static void BenchmarkVarTimeDoubleScalarBaseMult(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var p = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Point(), out var Ꮡp);

    for (nint i = 0; i < b.N; i++) {
        Ꮡp.VarTimeDoubleScalarBaseMult(dalekScalar, B, dalekScalar);
    }
}

} // end edwards25519_internal_test_package
