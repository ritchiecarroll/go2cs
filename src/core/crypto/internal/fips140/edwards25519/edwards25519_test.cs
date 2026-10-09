// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using field = go.crypto.@internal.fips140.edwards25519.field_package;
using hex = encoding.hex_package;
using reflect = reflect_package;
using testing = testing_package;
using encoding;
using go.crypto.@internal.fips140.edwards25519;
using static go.crypto.@internal.fips140.edwards25519_package;

partial class edwards25519_internal_test_package {

public static ж<global::go.crypto.@internal.fips140.edwards25519_package.Point> B;
internal static void initᴛB() { B = NewGeneratorPoint(); }

public static ж<global::go.crypto.@internal.fips140.edwards25519_package.Point> I;
internal static void initᴛI() { I = NewIdentityPoint(); }

internal static void checkOnCurve(ж<testing.T> Ꮡt, params Span<ж<global::go.crypto.@internal.fips140.edwards25519_package.Point>> pointsʗp) {
    var points = pointsʗp.sslice();

    Ꮡt.Helper();
    foreach (var (i, p) in points) {
        ref var XX = ref heap(new field.Element(), out var ᏑXX);
        ref var YY = ref heap(new field.Element(), out var ᏑYY);
        ref var ZZ = ref heap(new field.Element(), out var ᏑZZ);
        ref var ZZZZ = ref heap(new field.Element(), out var ᏑZZZZ);
        ᏑXX.Square(p.of(global::go.crypto.@internal.fips140.edwards25519_package.Point.Ꮡx));
        ᏑYY.Square(p.of(global::go.crypto.@internal.fips140.edwards25519_package.Point.Ꮡy));
        ᏑZZ.Square(p.of(global::go.crypto.@internal.fips140.edwards25519_package.Point.Ꮡz));
        ᏑZZZZ.Square(ᏑZZ);
        // -x² + y² = 1 + dx²y²
        // -(X/Z)² + (Y/Z)² = 1 + d(X/Z)²(Y/Z)²
        // (-X² + Y²)/Z² = 1 + (dX²Y²)/Z⁴
        // (-X² + Y²)*Z² = Z⁴ + dX²Y²
        ref var lhs = ref heap(new field.Element(), out var Ꮡlhs);
        ref var rhs = ref heap(new field.Element(), out var Ꮡrhs);
        Ꮡlhs.Subtract(ᏑYY, ᏑXX).Multiply(Ꮡlhs, ᏑZZ);
        Ꮡrhs.Multiply(d, ᏑXX).Multiply(Ꮡrhs, ᏑYY).Add(Ꮡrhs, ᏑZZZZ);
        if (lhs.Equal(Ꮡrhs) != 1) {
            Ꮡt.Errorf("X, Y, and Z do not specify a point on the curve\nX = %v\nY = %v\nZ = %v"u8, (~p).x, (~p).y, (~p).z);
        }
        // xy = T/Z
        Ꮡlhs.Multiply(p.of(global::go.crypto.@internal.fips140.edwards25519_package.Point.Ꮡx), p.of(global::go.crypto.@internal.fips140.edwards25519_package.Point.Ꮡy));
        Ꮡrhs.Multiply(p.of(global::go.crypto.@internal.fips140.edwards25519_package.Point.Ꮡz), p.of(global::go.crypto.@internal.fips140.edwards25519_package.Point.Ꮡt));
        if (lhs.Equal(Ꮡrhs) != 1) {
            Ꮡt.Errorf("point %d is not valid\nX = %v\nY = %v\nZ = %v"u8, i, (~p).x, (~p).y, (~p).z);
        }
    }
}

public static void TestGenerator(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // These are the coordinates of B from RFC 8032, Section 5.1, converted to
    // little endian hex.
    @string x = "1ad5258f602d56c9b2a7259560c72c695cdcd6fd31e2a4c0fe536ecdd3366921"u8;
    @string y = "5866666666666666666666666666666666666666666666666666666666666666"u8;
    {
        @string got = hex.EncodeToString(B.of(global::go.crypto.@internal.fips140.edwards25519_package.Point.Ꮡx).Bytes()); if (got != x) {
            Ꮡt.Errorf("wrong B.x: got %s, expected %s"u8, got, x);
        }
    }
    {
        @string got = hex.EncodeToString(B.of(global::go.crypto.@internal.fips140.edwards25519_package.Point.Ꮡy).Bytes()); if (got != y) {
            Ꮡt.Errorf("wrong B.y: got %s, expected %s"u8, got, y);
        }
    }
    if (B.of(global::go.crypto.@internal.fips140.edwards25519_package.Point.Ꮡz).Equal(feOne) != 1) {
        Ꮡt.Errorf("wrong B.z: got %v, expected 1"u8, (~B).z);
    }
    // Check that t is correct.
    checkOnCurve(Ꮡt, B);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bB2Bˢ = (@string)"B + B != [2]B"u8;
internal static readonly object bBBBˢ = (@string)"B - B != B + (-B)"u8;
internal static readonly object bB0ˢ = (@string)"B - B != 0"u8;
internal static readonly object bB0ˢ2 = (@string)"B + (-B) != 0"u8;

public static void TestAddSubNegOnBasePoint(ж<testing.T> Ꮡt) {
    var (checkLhs, checkRhs) = (Ꮡ(new Point(nil)), Ꮡ(new Point(nil)));
    checkLhs.Add(B, B);
    var tmpP2 = @new<global::go.crypto.@internal.fips140.edwards25519_package.projP2>().FromP3(B);
    var tmpP1xP1 = @new<global::go.crypto.@internal.fips140.edwards25519_package.projP1xP1>().Double(tmpP2);
    checkRhs.fromP1xP1(tmpP1xP1);
    if (checkLhs.Equal(checkRhs) != 1) {
        Ꮡt.Error(bB2Bˢ);
    }
    checkOnCurve(Ꮡt, checkLhs, checkRhs);
    checkLhs.Subtract(B, B);
    var Bneg = @new<global::go.crypto.@internal.fips140.edwards25519_package.Point>().Negate(B);
    checkRhs.Add(B, Bneg);
    if (checkLhs.Equal(checkRhs) != 1) {
        Ꮡt.Error(bBBBˢ);
    }
    if (I.Equal(checkLhs) != 1) {
        Ꮡt.Error(bB0ˢ);
    }
    if (I.Equal(checkRhs) != 1) {
        Ꮡt.Error(bB0ˢ2);
    }
    checkOnCurve(Ꮡt, checkLhs, checkRhs, Bneg);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object pointIsUnexpectedlyˢ = (@string)"Point is unexpectedly comparable"u8;

public static void TestComparable(ж<testing.T> Ꮡt) {
    if (reflect.TypeOf(new Point(nil)).Comparable()) {
        Ꮡt.Error(pointIsUnexpectedlyˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorForInvalidˢ = (@string)"expected error for invalid point"u8;
internal static readonly object setBytesDidNotReturnNilˢ = (@string)"SetBytes did not return nil on an invalid encoding"u8;
internal static readonly object thePointWasModifiedWhileˢ = (@string)"the Point was modified while decoding an invalid encoding"u8;

public static void TestInvalidEncodings(ж<testing.T> Ꮡt) {
    // An invalid point, that also happens to have y > p.
    @string invalid = "efffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8;
    var p = NewGeneratorPoint();
    {
        var (@out, err) = p.SetBytes(decodeHex(invalid)); if (err == default!){
            Ꮡt.Error(expectedErrorForInvalidˢ);
        } else 
        if (@out != nil){
            Ꮡt.Error(setBytesDidNotReturnNilˢ);
        } else 
        if (p.Equal(B) != 1) {
            Ꮡt.Error(thePointWasModifiedWhileˢ);
        }
    }
    checkOnCurve(Ꮡt, p);
}

internal partial struct TestNonCanonicalPoints_test /*dyn*/ {
    internal @string name;
    internal @string encoding, canonical;
}

public static void TestNonCanonicalPoints(ж<testing.T> Ꮡt) {
    var tests = new TestNonCanonicalPoints_test[]{ // Points with x = 0 and the sign bit set. With x = 0 the curve equation
 // gives y² = 1, so y = ±1. 1 has two valid encodings.

        new(
            "y=1,sign-"u8,
            "0100000000000000000000000000000000000000000000000000000000000080"u8,
            "0100000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p+1,sign-"u8,
            "eeffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "0100000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p-1,sign-"u8,
            "ecffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "ecffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8
        ), // Non-canonical y encodings with values 2²⁵⁵-19 (p) to 2²⁵⁵-1 (p+18).

        new(
            "y=p,sign+"u8,
            "edffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "0000000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p,sign-"u8,
            "edffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "0000000000000000000000000000000000000000000000000000000000000080"u8
        ),
        new(
            "y=p+1,sign+"u8,
            "eeffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "0100000000000000000000000000000000000000000000000000000000000000"u8
        ), // "y=p+1,sign-" is already tested above.
 // p+2 is not a valid y-coordinate.

        new(
            "y=p+3,sign+"u8,
            "f0ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "0300000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p+3,sign-"u8,
            "f0ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "0300000000000000000000000000000000000000000000000000000000000080"u8
        ),
        new(
            "y=p+4,sign+"u8,
            "f1ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "0400000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p+4,sign-"u8,
            "f1ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "0400000000000000000000000000000000000000000000000000000000000080"u8
        ),
        new(
            "y=p+5,sign+"u8,
            "f2ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "0500000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p+5,sign-"u8,
            "f2ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "0500000000000000000000000000000000000000000000000000000000000080"u8
        ),
        new(
            "y=p+6,sign+"u8,
            "f3ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "0600000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p+6,sign-"u8,
            "f3ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "0600000000000000000000000000000000000000000000000000000000000080"u8
        ), // p+7 is not a valid y-coordinate.
 // p+8 is not a valid y-coordinate.

        new(
            "y=p+9,sign+"u8,
            "f6ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "0900000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p+9,sign-"u8,
            "f6ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "0900000000000000000000000000000000000000000000000000000000000080"u8
        ),
        new(
            "y=p+10,sign+"u8,
            "f7ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "0a00000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p+10,sign-"u8,
            "f7ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "0a00000000000000000000000000000000000000000000000000000000000080"u8
        ), // p+11 is not a valid y-coordinate.
 // p+12 is not a valid y-coordinate.
 // p+13 is not a valid y-coordinate.

        new(
            "y=p+14,sign+"u8,
            "fbffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "0e00000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p+14,sign-"u8,
            "fbffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "0e00000000000000000000000000000000000000000000000000000000000080"u8
        ),
        new(
            "y=p+15,sign+"u8,
            "fcffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "0f00000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p+15,sign-"u8,
            "fcffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "0f00000000000000000000000000000000000000000000000000000000000080"u8
        ),
        new(
            "y=p+16,sign+"u8,
            "fdffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "1000000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p+16,sign-"u8,
            "fdffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "1000000000000000000000000000000000000000000000000000000000000080"u8
        ), // p+17 is not a valid y-coordinate.

        new(
            "y=p+18,sign+"u8,
            "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"u8,
            "1200000000000000000000000000000000000000000000000000000000000000"u8
        ),
        new(
            "y=p+18,sign-"u8,
            "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8,
            "1200000000000000000000000000000000000000000000000000000000000080"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestNonCanonicalPoints_test(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var (p1, err) = @new<global::go.crypto.@internal.fips140.edwards25519_package.Point>().SetBytes(decodeHex(ttʗ1.encoding));
            if (err != default!) {
                tΔ1.Fatalf("error decoding non-canonical point: %v"u8, err);
            }
            (var p2, err) = @new<global::go.crypto.@internal.fips140.edwards25519_package.Point>().SetBytes(decodeHex(ttʗ1.canonical));
            if (err != default!) {
                tΔ1.Fatalf("error decoding canonical point: %v"u8, err);
            }
            if (p1.Equal(p2) != 1) {
                tΔ1.Errorf("equivalent points are not equal: %v, %v"u8, p1.OrTypedNil(), p2.OrTypedNil());
            }
            {
                @string encoding = hex.EncodeToString(p1.Bytes()); if (encoding != ttʗ1.canonical) {
                    tΔ1.Errorf("re-encoding does not match canonical; got %q, expected %q"u8, encoding, ttʗ1.canonical);
                }
            }
            checkOnCurve(tΔ1, p1, p2);
        });
    }
}

internal static slice<byte> decodeHex(@string s) {
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        throw panic(err);
    }
    return b;
}

public static void BenchmarkEncodingDecoding(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = @new<global::go.crypto.@internal.fips140.edwards25519_package.Point>().Set(dalekScalarBasepoint);
    for (nint i = 0; i < b.N; i++) {
        var buf = p.Bytes();
        var (_, err) = p.SetBytes(buf);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
    }
}

} // end edwards25519_internal_test_package
