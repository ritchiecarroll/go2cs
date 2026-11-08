// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using rand = go.crypto.rand_package;
using hex = encoding.hex_package;
using big = math.big_package;
using testing = testing_package;
using encoding;
using go.crypto;
using io = io_package;
using math;
using static go.crypto.elliptic_package;

partial class elliptic_internal_test_package {

// genericParamsForCurve returns the dereferenced CurveParams for
// the specified curve. This is used to avoid the logic for
// upgrading a curve to its specific implementation, forcing
// usage of the generic implementation.
internal static ж<global::go.crypto.elliptic_package.CurveParams> genericParamsForCurve(global::go.crypto.elliptic_package.Curve c) {
    ref var d = ref heap<global::go.crypto.elliptic_package.CurveParams>((c.Params()).Value, out var Ꮡd);
    return Ꮡd;
}

internal partial struct testAllCurves_tests /*dyn*/ {
    internal @string name;
    internal global::go.crypto.elliptic_package.Curve curve;
}

internal static void testAllCurves(ж<testing.T> t, Action<ж<testing.T>, global::go.crypto.elliptic_package.Curve> f) {
    var tests = slice<testAllCurves_tests>([
        new("P256", P256()),
        new("P256/Params", new global::go.crypto.elliptic_package.CurveParamsжCurve(genericParamsForCurve(P256()))),
        new("P224", P224()),
        new("P224/Params", new global::go.crypto.elliptic_package.CurveParamsжCurve(genericParamsForCurve(P224()))),
        new("P384", P384()),
        new("P384/Params", new global::go.crypto.elliptic_package.CurveParamsжCurve(genericParamsForCurve(P384()))),
        new("P521", P521()),
        new("P521/Params", new global::go.crypto.elliptic_package.CurveParamsжCurve(genericParamsForCurve(P521())))
    ]);
    if (testing.Short()) {
        tests = tests[..1];
    }
    foreach (var (_, test) in range(tests)) {
        var curve = test.curve;
        var curveʗ1 = curve;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            f(tΔ1, curveʗ1);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object basepointIsNotOnTheCurveˢ = (@string)"basepoint is not on the curve"u8;

public static void TestOnCurve(ж<testing.T> t) {
    t.Parallel();
    testAllCurves(t, (ж<testing.T> tΔ1, global::go.crypto.elliptic_package.Curve curve) => {
        if (!curve.IsOnCurve(curve.Params().Gx, curve.Params().Gy)) {
            tΔ1.Error(basepointIsNotOnTheCurveˢ);
        }
    });
}

public static void TestOffCurve(ж<testing.T> t) {
    t.Parallel();
    testAllCurves(t, (ж<testing.T> tΔ1, global::go.crypto.elliptic_package.Curve curve) => {
        var (x, y) = (@new<bigꓸInt>().SetInt64(1), @new<bigꓸInt>().SetInt64(1));
        if (curve.IsOnCurve(x, y)) {
            tΔ1.Errorf("point off curve is claimed to be on the curve");
        }

        nint byteLen = (curve.Params().BitSize + 7) / 8;
        var b = new slice<byte>(1 + 2 * byteLen);
        b[0] = 4; // uncompressed point
        x.FillBytes(b.slice(1, 1 + byteLen));
        y.FillBytes(b.slice(1 + byteLen, 1 + 2 * byteLen));

        var (x1, y1) = Unmarshal(curve, b);
        if (x1 != nil || y1 != nil) {
            tΔ1.Errorf("unmarshaling a point not on the curve succeeded");
        }
    });
}

public static void TestInfinity(ж<testing.T> t) {
    t.Parallel();
    testAllCurves(t, testInfinity);
}

internal static bool isInfinity(ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    return x.Sign() == 0 && y.Sign() == 0;
}

internal static void testInfinity(ж<testing.T> t, global::go.crypto.elliptic_package.Curve curve) {
    var (x0, y0) = (@new<bigꓸInt>(), @new<bigꓸInt>());
    var (xG, yG) = (curve.Params().Gx, curve.Params().Gy);

    var (ᴛ1, ᴛ2) = curve.ScalarMult(xG, yG, curve.Params().N.Bytes());
    if (!isInfinity(ᴛ1, ᴛ2)) {
        t.Errorf("x^q != ∞");
    }
    var (ᴛ3, ᴛ4) = curve.ScalarMult(xG, yG, [0]);
    if (!isInfinity(ᴛ3, ᴛ4)) {
        t.Errorf("x^0 != ∞");
    }

    var (ᴛ5, ᴛ6) = curve.ScalarMult(x0, y0, [1, 2, 3]);
    if (!isInfinity(ᴛ5, ᴛ6)) {
        t.Errorf("∞^k != ∞");
    }
    var (ᴛ7, ᴛ8) = curve.ScalarMult(x0, y0, [0]);
    if (!isInfinity(ᴛ7, ᴛ8)) {
        t.Errorf("∞^0 != ∞");
    }

    var (ᴛ9, ᴛ10) = curve.ScalarBaseMult(curve.Params().N.Bytes());
    if (!isInfinity(ᴛ9, ᴛ10)) {
        t.Errorf("b^q != ∞");
    }
    var (ᴛ11, ᴛ12) = curve.ScalarBaseMult([0]);
    if (!isInfinity(ᴛ11, ᴛ12)) {
        t.Errorf("b^0 != ∞");
    }

    var (ᴛ13, ᴛ14) = curve.Double(x0, y0);
    if (!isInfinity(ᴛ13, ᴛ14)) {
        t.Errorf("2∞ != ∞");
    }
    // There is no other point of order two on the NIST curves (as they have
    // cofactor one), so Double can't otherwise return the point at infinity.

    var nMinusOne = @new<bigꓸInt>().Sub(curve.Params().N, big.NewInt(1));
    var (x, y) = curve.ScalarMult(xG, yG, nMinusOne.Bytes());
    (x, y) = curve.Add(x, y, xG, yG);
    if (!isInfinity(x, y)) {
        t.Errorf("x^(q-1) + x != ∞");
    }
    (x, y) = curve.Add(xG, yG, x0, y0);
    if (x.Cmp(xG) != 0 || y.Cmp(yG) != 0) {
        t.Errorf("x+∞ != x");
    }
    (x, y) = curve.Add(x0, y0, xG, yG);
    if (x.Cmp(xG) != 0 || y.Cmp(yG) != 0) {
        t.Errorf("∞+x != x");
    }

    if (curve.IsOnCurve(x0, y0)) {
        t.Errorf("IsOnCurve(∞) == true");
    }

    {
        var (xx, yy) = Unmarshal(curve, Marshal(curve, x0, y0)); if (xx != nil || yy != nil) {
            t.Errorf("Unmarshal(Marshal(∞)) did not return an error");
        }
    }
    // We don't test UnmarshalCompressed(MarshalCompressed(∞)) because there are
    // two valid points with x = 0.
    {
        var (xx, yy) = Unmarshal(curve, [0x00]); if (xx != nil || yy != nil) {
            t.Errorf("Unmarshal(∞) did not return an error");
        }
    }
    nint byteLen = (curve.Params().BitSize + 7) / 8;
    var buf = new slice<byte>(byteLen * 2 + 1);
    buf[0] = 4; // Uncompressed format.
    {
        var (xx, yy) = Unmarshal(curve, buf); if (xx != nil || yy != nil) {
            t.Errorf("Unmarshal((0,0)) did not return an error");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToUnmarshalˢ = (@string)"failed to unmarshal"u8;
internal static readonly object unmarshalReturnedˢ = (@string)"unmarshal returned different values"u8;

public static void TestMarshal(ж<testing.T> t) {
    t.Parallel();
    testAllCurves(t, (ж<testing.T> tΔ1, global::go.crypto.elliptic_package.Curve curve) => {
        var (_, x, y, err) = GenerateKey(curve, rand.Reader);
        if (err != nil) {
            tΔ1.Fatal(err);
        }
        var serialized = Marshal(curve, x, y);
        var (xx, yy) = Unmarshal(curve, serialized);
        if (xx == nil) {
            tΔ1.Fatal(failedToUnmarshalˢ);
        }
        if (xx.Cmp(x) != 0 || yy.Cmp(y) != 0) {
            tΔ1.Fatal(unmarshalReturnedˢ);
        }
    });
}

public static void TestUnmarshalToLargeCoordinates(ж<testing.T> t) {
    t.Parallel();
    // See https://golang.org/issues/20482.
    testAllCurves(t, testUnmarshalToLargeCoordinates);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object yNotWithinExpectedRangeˢ = (@string)"y not within expected range"u8;

internal static void testUnmarshalToLargeCoordinates(ж<testing.T> t, global::go.crypto.elliptic_package.Curve curve) {
    var p = curve.Params().P;
    nint byteLen = (p.BitLen() + 7) / 8;

    // Set x to be greater than curve's parameter P – specifically, to P+5.
    // Set y to mod_sqrt(x^3 - 3x + B)) so that (x mod P = 5 , y) is on the
    // curve.
    var x = @new<bigꓸInt>().Add(p, big.NewInt(5));
    var y = curve.Params().polynomial(x);
    y.ModSqrt(y, p);

    var invalid = new slice<byte>(byteLen * 2 + 1);
    invalid[0] = 4; // uncompressed encoding
    x.FillBytes(invalid.slice(1, 1 + byteLen));
    y.FillBytes(invalid.slice(1 + byteLen));

    {
        var (X, Y) = Unmarshal(curve, invalid); if (X != nil || Y != nil) {
            t.Errorf("Unmarshal accepts invalid X coordinate");
        }
    }

    if (AreEqual(curve, p256)) {
        // This is a point on the curve with a small y value, small enough that
        // we can add p and still be within 32 bytes.
        (x, _) = @new<bigꓸInt>().SetString("31931927535157963707678568152204072984517581467226068221761862915403492091210", 10);
        (y, _) = @new<bigꓸInt>().SetString("5208467867388784005506817585327037698770365050895731383201516607147", 10);
        y.Add(y, p);

        if (p.Cmp(y) > 0 || y.BitLen() != 256) {
            t.Fatal(yNotWithinExpectedRangeˢ);
        }

        // marshal
        x.FillBytes(invalid.slice(1, 1 + byteLen));
        y.FillBytes(invalid.slice(1 + byteLen));

        var (X, Y) = Unmarshal(curve, invalid); if (X != nil || Y != nil) {
            t.Errorf("Unmarshal accepts invalid Y coordinate");
        }
    }
}

// TestInvalidCoordinates tests big.Int values that are not valid field elements
// (negative or bigger than P). They are expected to return false from
// IsOnCurve, all other behavior is undefined.
public static void TestInvalidCoordinates(ж<testing.T> t) {
    t.Parallel();
    testAllCurves(t, testInvalidCoordinates);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xPYˢ => ref xPYˢᵇ;
private static readonly @string xPYˢᵇ = "x-P, y"u8;
internal static ref readonly @string xYPˢ => ref xYPˢᵇ;
private static readonly @string xYPˢᵇ = "x, y-P"u8;
internal static ref readonly @string xPYˢ2 => ref xPYˢ2ᵇ;
private static readonly @string xPYˢ2ᵇ = "x+P, y"u8;
internal static ref readonly @string xYPˢ2 => ref xYPˢ2ᵇ;
private static readonly @string xYPˢ2ᵇ = "x, y+P"u8;
internal static ref readonly @string x2Yˢ => ref x2Yˢᵇ;
private static readonly @string x2Yˢᵇ = "x+2⁵³⁵, y"u8;
internal static ref readonly @string xY2ˢ => ref xY2ˢᵇ;
private static readonly @string xY2ˢᵇ = "x, y+2⁵³⁵"u8;
internal static readonly object modSqrtBIsNotOnTheCurveˢ = (@string)"(0, mod_sqrt(B)) is not on the curve?"u8;

internal static void testInvalidCoordinates(ж<testing.T> t, global::go.crypto.elliptic_package.Curve curve) {
    void checkIsOnCurveFalse(@string name, ж<bigꓸInt> xΔ1, ж<bigꓸInt> yΔ1) {
        if (curve.IsOnCurve(xΔ1, yΔ1)) {
            t.Errorf("IsOnCurve(%s) unexpectedly returned true", name);
        }
    }

    var p = curve.Params().P;
    var (_, x, y, _) = GenerateKey(curve, rand.Reader);
    var (xx, yy) = (@new<bigꓸInt>(), @new<bigꓸInt>());

    // Check if the sign is getting dropped.
    xx.Neg(x);
    checkIsOnCurveFalse("-x, y", xx, y);
    yy.Neg(y);
    checkIsOnCurveFalse("x, -y", x, yy);

    // Check if negative values are reduced modulo P.
    xx.Sub(x, p);
    checkIsOnCurveFalse(xPYˢ, xx, y);
    yy.Sub(y, p);
    checkIsOnCurveFalse(xYPˢ, x, yy);

    // Check if positive values are reduced modulo P.
    xx.Add(x, p);
    checkIsOnCurveFalse(xPYˢ2, xx, y);
    yy.Add(y, p);
    checkIsOnCurveFalse(xYPˢ2, x, yy);

    // Check if the overflow is dropped.
    xx.Add(x, @new<bigꓸInt>().Lsh(big.NewInt(1), 535));
    checkIsOnCurveFalse(x2Yˢ, xx, y);
    yy.Add(y, @new<bigꓸInt>().Lsh(big.NewInt(1), 535));
    checkIsOnCurveFalse(xY2ˢ, x, yy);

    // Check if P is treated like zero (if possible).
    // y^2 = x^3 - 3x + B
    // y = mod_sqrt(x^3 - 3x + B)
    // y = mod_sqrt(B) if x = 0
    // If there is no modsqrt, there is no point with x = 0, can't test x = P.
    var yyΔ1 = @new<bigꓸInt>().ModSqrt(curve.Params().B, p); if (yyΔ1 != nil) {
        if (!curve.IsOnCurve(big.NewInt(0), yyΔ1)) {
            t.Fatal(modSqrtBIsNotOnTheCurveˢ);
        }
        checkIsOnCurveFalse("P, y", p, yyΔ1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string p25603ˢ => ref p25603ˢᵇ;
private static readonly @string p25603ˢᵇ = "P-256/03"u8;
internal static ref readonly @string p25602ˢ => ref p25602ˢᵇ;
private static readonly @string p25602ˢᵇ = "P-256/02"u8;
internal static ref readonly @string invalidˢ => ref invalidˢᵇ;
private static readonly @string invalidˢᵇ = "Invalid"u8;
internal static readonly object expectedAnErrorForˢ = (@string)"expected an error for invalid encoding"u8;
internal static readonly object skippingOtherCurvesOnˢ = (@string)"skipping other curves on short test"u8;

public static void TestMarshalCompressed(ж<testing.T> t) {
    t.Parallel();
    t.Run(p25603ˢ, (ж<testing.T> tΔ1) => {
        var (data, _) = hex.DecodeString("031e3987d9f9ea9d7dd7155a56a86b2009e1e0ab332f962d10d8beb6406ab1ad79");
        var (x, _) = @new<bigꓸInt>().SetString("13671033352574878777044637384712060483119675368076128232297328793087057702265", 10);
        var (y, _) = @new<bigꓸInt>().SetString("66200849279091436748794323380043701364391950689352563629885086590854940586447", 10);
        testMarshalCompressed(tΔ1, P256(), x, y, data);
    });
    t.Run(p25602ˢ, (ж<testing.T> tΔ2) => {
        var (data, _) = hex.DecodeString("021e3987d9f9ea9d7dd7155a56a86b2009e1e0ab332f962d10d8beb6406ab1ad79");
        var (x, _) = @new<bigꓸInt>().SetString("13671033352574878777044637384712060483119675368076128232297328793087057702265", 10);
        var (y, _) = @new<bigꓸInt>().SetString("49591239931264812013903123569363872165694192725937750565648544718012157267504", 10);
        testMarshalCompressed(tΔ2, P256(), x, y, data);
    });

    t.Run(invalidˢ, (ж<testing.T> tΔ3) => {
        var (data, _) = hex.DecodeString("02fd4bf61763b46581fd9174d623516cf3c81edd40e29ffa2777fb6cb0ae3ce535");
        var (X, Y) = UnmarshalCompressed(P256(), data);
        if (X != nil || Y != nil) {
            tΔ3.Error(expectedAnErrorForˢ);
        }
    });

    if (testing.Short()) {
        t.Skip(skippingOtherCurvesOnˢ);
    }

    testAllCurves(t, (ж<testing.T> tΔ4, global::go.crypto.elliptic_package.Curve curve) => {
        var (_, x, y, err) = GenerateKey(curve, rand.Reader);
        if (err != nil) {
            tΔ4.Fatal(err);
        }
        testMarshalCompressed(tΔ4, curve, x, y, default);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object invalidTestPointˢ = (@string)"invalid test point"u8;
internal static readonly object unmarshalCompressedˢ = (@string)"UnmarshalCompressed returned a point not on the curve"u8;

internal static void testMarshalCompressed(ж<testing.T> t, global::go.crypto.elliptic_package.Curve curve, ж<bigꓸInt> x, ж<bigꓸInt> y, slice<byte> want) {
    if (!curve.IsOnCurve(x, y)) {
        t.Fatal(invalidTestPointˢ);
    }
    var got = MarshalCompressed(curve, x, y);
    if (want != nil && !bytes.Equal(got, want)) {
        t.Errorf("got unexpected MarshalCompressed result: got %x, want %x", got, want);
    }

    var (X, Y) = UnmarshalCompressed(curve, got);
    if (X == nil || Y == nil) {
        t.Fatalf("UnmarshalCompressed failed unexpectedly");
    }

    if (!curve.IsOnCurve(X, Y)) {
        t.Error(unmarshalCompressedˢ);
    }
    if (X.Cmp(x) != 0 || Y.Cmp(y) != 0) {
        t.Errorf("point did not round-trip correctly: got (%v, %v), want (%v, %v)", X.OrTypedNil(), Y.OrTypedNil(), x.OrTypedNil(), y.OrTypedNil());
    }
}

public static void TestLargeIsOnCurve(ж<testing.T> t) {
    t.Parallel();
    testAllCurves(t, (ж<testing.T> tΔ1, global::go.crypto.elliptic_package.Curve curve) => {
        var large = big.NewInt(1);
        large.Lsh(large, 1000);
        if (curve.IsOnCurve(large, large)) {
            tΔ1.Errorf("(2^1000, 2^1000) is reported on the curve");
        }
    });
}

internal partial struct benchmarkAllCurves_tests /*dyn*/ {
    internal @string name;
    internal global::go.crypto.elliptic_package.Curve curve;
}

internal static void benchmarkAllCurves(ж<testing.B> b, Action<ж<testing.B>, global::go.crypto.elliptic_package.Curve> f) {
    var tests = slice<benchmarkAllCurves_tests>([
        new("P256", P256()),
        new("P224", P224()),
        new("P384", P384()),
        new("P521", P521())
    ]);
    foreach (var (_, test) in range(tests)) {
        var curve = test.curve;
        var curveʗ1 = curve;
        b.Run(test.name, (ж<testing.B> bΔ1) => {
            f(bΔ1, curveʗ1);
        });
    }
}

public static void BenchmarkScalarBaseMult(ж<testing.B> b) {
    benchmarkAllCurves(b, (ж<testing.B> bΔ1, global::go.crypto.elliptic_package.Curve curve) => {
        var (priv, _, _, _) = GenerateKey(curve, rand.Reader);
        bΔ1.ReportAllocs();
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var (x, _) = curve.ScalarBaseMult(priv);
            // Prevent the compiler from optimizing out the operation.
            priv[0] ^= (byte)((byte)x.Bits()[0]);
        }
    });
}

public static void BenchmarkScalarMult(ж<testing.B> b) {
    benchmarkAllCurves(b, (ж<testing.B> bΔ1, global::go.crypto.elliptic_package.Curve curve) => {
        var (_, x, y, _) = GenerateKey(curve, rand.Reader);
        var (priv, _, _, _) = GenerateKey(curve, rand.Reader);
        bΔ1.ReportAllocs();
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            (x, y) = curve.ScalarMult(x, y, priv);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string uncompressedˢ => ref uncompressedˢᵇ;
private static readonly @string uncompressedˢᵇ = "Uncompressed"u8;
internal static readonly object unmarshalOutputDifferentˢ = (@string)"Unmarshal output different from Marshal input"u8;
internal static ref readonly @string compressedˢ => ref compressedˢᵇ;
private static readonly @string compressedˢᵇ = "Compressed"u8;

public static void BenchmarkMarshalUnmarshal(ж<testing.B> b) {
    benchmarkAllCurves(b, (ж<testing.B> bΔ1, global::go.crypto.elliptic_package.Curve curve) => {
        var (_, x, y, _) = GenerateKey(curve, rand.Reader);
        var xʗ1 = x;
        var yʗ1 = y;
        bΔ1.Run(uncompressedˢ, (ж<testing.B> bΔ2) => {
            bΔ2.ReportAllocs();
            for (nint i = 0; i < (~bΔ2).N; i++) {
                var buf = Marshal(curve, xʗ1, yʗ1);
                var (xx, yy) = Unmarshal(curve, buf);
                if (xx.Cmp(xʗ1) != 0 || yy.Cmp(yʗ1) != 0) {
                    bΔ2.Error(unmarshalOutputDifferentˢ);
                }
            }
        });
        var xʗ2 = x;
        var yʗ2 = y;
        bΔ1.Run(compressedˢ, (ж<testing.B> bΔ3) => {
            bΔ3.ReportAllocs();
            for (nint i = 0; i < (~bΔ3).N; i++) {
                var buf = MarshalCompressed(curve, xʗ2, yʗ2);
                var (xx, yy) = UnmarshalCompressed(curve, buf);
                if (xx.Cmp(xʗ2) != 0 || yy.Cmp(yʗ2) != 0) {
                    bΔ3.Error(unmarshalOutputDifferentˢ);
                }
            }
        });
    });
}

} // end elliptic_internal_test_package
