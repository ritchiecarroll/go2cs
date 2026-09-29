// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using nistec = go.crypto.@internal.fips140.nistec_package;
using errors = errors_package;
using big = math.big_package;
using go.crypto.@internal.fips140;
using math;

partial class elliptic_package {

internal static ж<nistCurve<P224PointжnistPoint>> p224 = Ꮡ(new nistCurve<P224PointжnistPoint>(
    newPoint: () => nistec.NewP224Point()
));

internal static void initP224() {
    p224.Value.@params = Ꮡ(new CurveParams(
        Name: "P-224"u8,
        BitSize: 224, // SP 800-186, Section 3.2.1.2

        P: bigFromDecimal("26959946667150639794667015087019630673557916260026308143510066298881"u8),
        N: bigFromDecimal("26959946667150639794667015087019625940457807714424391721682722368061"u8),
        B: bigFromHex("b4050a850c04b3abf54132565044b0b7d7bfd8ba270b39432355ffb4"u8),
        Gx: bigFromHex("b70e0cbd6bb4bf7f321390b94a03c1d356c21122343280d6115c1d21"u8),
        Gy: bigFromHex("bd376388b5f723fb4c22dfe6cd4375a05a07476444d5819985007e34"u8)
    ));
}

[GoType] partial struct p256Curve {
    internal partial ref nistCurve<P256PointжnistPoint> nistCurve { get; }
}

internal static ж<p256Curve> p256 = Ꮡ(new p256Curve(new nistCurve<P256PointжnistPoint>(
    newPoint: () => nistec.NewP256Point()
)
));

internal static void initP256() {
    p256.Value.@params = Ꮡ(new CurveParams(
        Name: "P-256"u8,
        BitSize: 256, // SP 800-186, Section 3.2.1.3

        P: bigFromDecimal("115792089210356248762697446949407573530086143415290314195533631308867097853951"u8),
        N: bigFromDecimal("115792089210356248762697446949407573529996955224135760342422259061068512044369"u8),
        B: bigFromHex("5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b"u8),
        Gx: bigFromHex("6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296"u8),
        Gy: bigFromHex("4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5"u8)
    ));
}

internal static ж<nistCurve<P384PointжnistPoint>> p384 = Ꮡ(new nistCurve<P384PointжnistPoint>(
    newPoint: () => nistec.NewP384Point()
));

internal static void initP384() {
    p384.Value.@params = Ꮡ(new CurveParams(
        Name: "P-384"u8,
        BitSize: 384, // SP 800-186, Section 3.2.1.4

        P: bigFromDecimal("394020061963944792122790401001436138050797392704654"u8 + "46667948293404245721771496870329047266088258938001861606973112319"u8),
        N: bigFromDecimal("394020061963944792122790401001436138050797392704654"u8 + "46667946905279627659399113263569398956308152294913554433653942643"u8),
        B: bigFromHex("b3312fa7e23ee7e4988e056be3f82d19181d9c6efe8141120314088"u8 + "f5013875ac656398d8a2ed19d2a85c8edd3ec2aef"u8),
        Gx: bigFromHex("aa87ca22be8b05378eb1c71ef320ad746e1d3b628ba79b9859f741"u8 + "e082542a385502f25dbf55296c3a545e3872760ab7"u8),
        Gy: bigFromHex("3617de4a96262c6f5d9e98bf9292dc29f8f41dbd289a147ce9da31"u8 + "13b5f0b8c00a60b1ce1d7e819d7a431d7c90ea0e5f"u8)
    ));
}

internal static ж<nistCurve<P521PointжnistPoint>> p521 = Ꮡ(new nistCurve<P521PointжnistPoint>(
    newPoint: () => nistec.NewP521Point()
));

internal static void initP521() {
    p521.Value.@params = Ꮡ(new CurveParams(
        Name: "P-521"u8,
        BitSize: 521, // SP 800-186, Section 3.2.1.5

        P: bigFromDecimal("68647976601306097149819007990813932172694353001433"u8 + "0540939446345918554318339765605212255964066145455497729631139148"u8 + "0858037121987999716643812574028291115057151"u8),
        N: bigFromDecimal("68647976601306097149819007990813932172694353001433"u8 + "0540939446345918554318339765539424505774633321719753296399637136"u8 + "3321113864768612440380340372808892707005449"u8),
        B: bigFromHex("0051953eb9618e1c9a1f929a21a0b68540eea2da725b99b315f3b8"u8 + "b489918ef109e156193951ec7e937b1652c0bd3bb1bf073573df883d2c34f1ef"u8 + "451fd46b503f00"u8),
        Gx: bigFromHex("00c6858e06b70404e9cd9e3ecb662395b4429c648139053fb521f8"u8 + "28af606b4d3dbaa14b5e77efe75928fe1dc127a2ffa8de3348b3c1856a429bf9"u8 + "7e7e31c2e5bd66"u8),
        Gy: bigFromHex("011839296a789a3bc0045c8a5fb42c7d1bd998f54449579b446817"u8 + "afbd17273e662c97ee72995ef42640c550b9013fad0761353c7086a272c24088"u8 + "be94769fd16650"u8)
    ));
}

// nistCurve is a Curve implementation based on a nistec Point.
//
// It's a wrapper that exposes the big.Int-based Curve interface and encodes the
// legacy idiosyncrasies it requires, such as invalid and infinity point
// handling.
//
// To interact with the nistec package, points are encoded into and decoded from
// properly formatted byte slices. All big.Int use is limited to this package.
// Encoding and decoding is 1/1000th of the runtime of a scalar multiplication,
// so the overhead is acceptable.
[GoType] partial struct nistCurve<Point>
    where Point : nistPoint<Point>
{
    internal Func<Point> newPoint;
    internal ж<CurveParams> @params;
}

// nistPoint is a generic constraint for the nistec Point types.
[GoType] partial interface nistPoint<T> {
    slice<byte> Bytes();
    (T, error) SetBytes(slice<byte> _);
    T Add(T _Δp0, T _Δp1);
    T Double(T _);
    (T, error) ScalarMult(T _Δp0, slice<byte> _Δp1);
    (T, error) ScalarBaseMult(slice<byte> _);
}

[GoRecv] internal static ж<CurveParams> Params<Point>(this ref nistCurve<Point> curve)
    where Point : nistPoint<Point>
{
    return curve.@params;
}

[GoRecv] internal static bool IsOnCurve<Point>(this ref nistCurve<Point> curve, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy)
    where Point : nistPoint<Point>
{
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    // IsOnCurve is documented to reject (0, 0), the conventional point at
    // infinity, which however is accepted by pointFromAffine.
    if (x.Sign() == 0 && y.Sign() == 0) {
        return false;
    }
    var (_, err) = curve.pointFromAffine(Ꮡx, Ꮡy);
    return err == default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string negativeCoordinateˢ = "negative coordinate"u8;
internal static readonly @string overflowingCoordinateˢ = "overflowing coordinate"u8;

[GoRecv] internal static (Point p, error err) pointFromAffine<Point>(this ref nistCurve<Point> curve, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy)
    where Point : nistPoint<Point>
{
    Point p = GoZero<Point>();

    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();
    // (0, 0) is by convention the point at infinity, which can't be represented
    // in affine coordinates. See Issue 37294.
    if (x.Sign() == 0 && y.Sign() == 0) {
        return (curve.newPoint(), default!);
    }
    // Reject values that would not get correctly encoded.
    if (x.Sign() < 0 || y.Sign() < 0) {
        return (p, errors.New(negativeCoordinateˢ));
    }
    if (x.BitLen() > (~curve.@params).BitSize || y.BitLen() > (~curve.@params).BitSize) {
        return (p, errors.New(overflowingCoordinateˢ));
    }
    // Encode the coordinates and let SetBytes reject invalid points.
    nint byteLen = ((~curve.@params).BitSize + 7) / 8;
    var buf = new slice<byte>(1 + 2 * byteLen);
    buf[0] = 4; // uncompressed point
    x.FillBytes(buf[1..(int)(1 + byteLen)]);
    y.FillBytes(buf[(int)(1 + byteLen)..(int)(1 + 2 * byteLen)]);
    return curve.newPoint().SetBytes(buf);
}

[GoRecv] internal static (ж<bigꓸInt> x, ж<bigꓸInt> y) pointToAffine<Point>(this ref nistCurve<Point> curve, Point p)
    where Point : nistPoint<Point>
{
    ж<bigꓸInt> x = default!;
    ж<bigꓸInt> y = default!;

    var @out = p.Bytes();
    if (len(@out) == 1 && @out[0] == 0) {
        // This is the encoding of the point at infinity, which the affine
        // coordinates API represents as (0, 0) by convention.
        return (@new<bigꓸInt>(), @new<bigꓸInt>());
    }
    nint byteLen = ((~curve.@params).BitSize + 7) / 8;
    x = @new<bigꓸInt>().SetBytes(@out[1..(int)(1 + byteLen)]);
    y = @new<bigꓸInt>().SetBytes(@out[(int)(1 + byteLen)..]);
    return (x, y);
}

[GoRecv] internal static (ж<bigꓸInt>, ж<bigꓸInt>) Add<Point>(this ref nistCurve<Point> curve, ж<bigꓸInt> Ꮡx1, ж<bigꓸInt> Ꮡy1, ж<bigꓸInt> Ꮡx2, ж<bigꓸInt> Ꮡy2)
    where Point : nistPoint<Point>
{
    var (p1, err) = curve.pointFromAffine(Ꮡx1, Ꮡy1);
    if (err != default!) {
        throw panic("crypto/elliptic: Add was called on an invalid point");
    }
    (var p2, err) = curve.pointFromAffine(Ꮡx2, Ꮡy2);
    if (err != default!) {
        throw panic("crypto/elliptic: Add was called on an invalid point");
    }
    return curve.pointToAffine(p1.Add(p1, p2));
}

[GoRecv] internal static (ж<bigꓸInt>, ж<bigꓸInt>) Double<Point>(this ref nistCurve<Point> curve, ж<bigꓸInt> Ꮡx1, ж<bigꓸInt> Ꮡy1)
    where Point : nistPoint<Point>
{
    var (p, err) = curve.pointFromAffine(Ꮡx1, Ꮡy1);
    if (err != default!) {
        throw panic("crypto/elliptic: Double was called on an invalid point");
    }
    return curve.pointToAffine(p.Double(p));
}

// normalizeScalar brings the scalar within the byte size of the order of the
// curve, as expected by the nistec scalar multiplication functions.
[GoRecv] internal static slice<byte> normalizeScalar<Point>(this ref nistCurve<Point> curve, slice<byte> scalar)
    where Point : nistPoint<Point>
{
    nint byteSize = ((~curve.@params).N.BitLen() + 7) / 8;
    if (len(scalar) == byteSize) {
        return scalar;
    }
    var s = @new<bigꓸInt>().SetBytes(scalar);
    if (len(scalar) > byteSize) {
        s.Mod(s, (~curve.@params).N);
    }
    var @out = new slice<byte>(byteSize);
    return s.FillBytes(@out);
}

[GoRecv] internal static (ж<bigꓸInt>, ж<bigꓸInt>) ScalarMult<Point>(this ref nistCurve<Point> curve, ж<bigꓸInt> ᏑBx, ж<bigꓸInt> ᏑBy, slice<byte> scalar)
    where Point : nistPoint<Point>
{
    var (p, err) = curve.pointFromAffine(ᏑBx, ᏑBy);
    if (err != default!) {
        throw panic("crypto/elliptic: ScalarMult was called on an invalid point");
    }
    scalar = curve.normalizeScalar(scalar);
    (p, err) = p.ScalarMult(p, scalar);
    if (err != default!) {
        throw panic("crypto/elliptic: nistec rejected normalized scalar");
    }
    return curve.pointToAffine(p);
}

[GoRecv] internal static (ж<bigꓸInt>, ж<bigꓸInt>) ScalarBaseMult<Point>(this ref nistCurve<Point> curve, slice<byte> scalar)
    where Point : nistPoint<Point>
{
    scalar = curve.normalizeScalar(scalar);
    var (p, err) = curve.newPoint().ScalarBaseMult(scalar);
    if (err != default!) {
        throw panic("crypto/elliptic: nistec rejected normalized scalar");
    }
    return curve.pointToAffine(p);
}

// CombinedMult returns [s1]G + [s2]P where G is the generator. It's used
// through an interface upgrade in crypto/ecdsa.
[GoRecv] internal static (ж<bigꓸInt> x, ж<bigꓸInt> y) CombinedMult<Point>(this ref nistCurve<Point> curve, ж<bigꓸInt> ᏑPx, ж<bigꓸInt> ᏑPy, slice<byte> s1, slice<byte> s2)
    where Point : nistPoint<Point>
{
    s1 = curve.normalizeScalar(s1);
    var (q, err) = curve.newPoint().ScalarBaseMult(s1);
    if (err != default!) {
        throw panic("crypto/elliptic: nistec rejected normalized scalar");
    }
    (var p, err) = curve.pointFromAffine(ᏑPx, ᏑPy);
    if (err != default!) {
        throw panic("crypto/elliptic: CombinedMult was called on an invalid point");
    }
    s2 = curve.normalizeScalar(s2);
    (p, err) = p.ScalarMult(p, s2);
    if (err != default!) {
        throw panic("crypto/elliptic: nistec rejected normalized scalar");
    }
    return curve.pointToAffine(p.Add(p, q));
}

[GoRecv] internal static (ж<bigꓸInt> x, ж<bigꓸInt> y) Unmarshal<Point>(this ref nistCurve<Point> curve, slice<byte> data)
    where Point : nistPoint<Point>
{
    ж<bigꓸInt> x = default!;
    ж<bigꓸInt> y = default!;

    if (len(data) == 0 || data[0] != 4) {
        return (default!, default!);
    }
    // Use SetBytes to check that data encodes a valid point.
    var (_, err) = curve.newPoint().SetBytes(data);
    if (err != default!) {
        return (default!, default!);
    }
    // We don't use pointToAffine because it involves an expensive field
    // inversion to convert from Jacobian to affine coordinates, which we
    // already have.
    nint byteLen = ((~curve.@params).BitSize + 7) / 8;
    x = @new<bigꓸInt>().SetBytes(data[1..(int)(1 + byteLen)]);
    y = @new<bigꓸInt>().SetBytes(data[(int)(1 + byteLen)..]);
    return (x, y);
}

[GoRecv] internal static (ж<bigꓸInt> x, ж<bigꓸInt> y) UnmarshalCompressed<Point>(this ref nistCurve<Point> curve, slice<byte> data)
    where Point : nistPoint<Point>
{
    if (len(data) == 0 || (data[0] != 2 && data[0] != 3)) {
        return (default!, default!);
    }
    var (p, err) = curve.newPoint().SetBytes(data);
    if (err != default!) {
        return (default!, default!);
    }
    return curve.pointToAffine(p);
}

internal static ж<bigꓸInt> bigFromDecimal(@string s) {
    var (b, ok) = @new<bigꓸInt>().SetString(s, 10);
    if (!ok) {
        throw panic("crypto/elliptic: internal error: invalid encoding");
    }
    return b;
}

internal static ж<bigꓸInt> bigFromHex(@string s) {
    var (b, ok) = @new<bigꓸInt>().SetString(s, 16);
    if (!ok) {
        throw panic("crypto/elliptic: internal error: invalid encoding");
    }
    return b;
}

} // end elliptic_package
