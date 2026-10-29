// Copyright (c) 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

// blank import: go.crypto.@internal.fips140.check_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using field = go.crypto.@internal.fips140.edwards25519.field_package;
using errors = errors_package;
using go.crypto.@internal.fips140.edwards25519;
using ꓸꓸꓸжPoint = Span<ж<edwards25519_package.Point>>;

partial class edwards25519_package {

// Point types.
partial struct projP1xP1 {
    public field.Element X, Y, Z, T;
}

partial struct projP2 {
    public field.Element X, Y, Z;
}

// Point represents a point on the edwards25519 curve.
//
// This type works similarly to math/big.Int, and all arguments and receivers
// are allowed to alias.
//
// The zero value is NOT valid, and it may be used only as a receiver.
partial struct Point {
    // Make the type not comparable (i.e. used with == or as a map key), as
    // equivalent points can be represented by different Go values.
    internal incomparable _;
    // The point is internally represented in extended coordinates (X, Y, Z, T)
    // where x = X/Z, y = Y/Z, and xy = T/Z per https://eprint.iacr.org/2008/522.
    internal field.Element x, y, z, t;
}

partial struct incomparable /*[0]Action*/;

internal static void checkInitialized(params ꓸꓸꓸжPoint pointsʗp) {
    var points = pointsʗp.sslice();

    foreach (var (_, p) in points) {
        if (p.x == (new field.Element()) && p.y == (new field.Element())) {
            throw panic("edwards25519: use of uninitialized Point");
        }
    }
}

partial struct projCached {
    public field.Element YplusX, YminusX, Z, T2d;
}

partial struct affineCached {
    public field.Element YplusX, YminusX, T2d;
}

// Constructors.
internal static ж<projP2> Zero(this ж<projP2> Ꮡv) {
    Ꮡv.of(projP2.ᏑX).Zero();
    Ꮡv.of(projP2.ᏑY).One();
    Ꮡv.of(projP2.ᏑZ).One();
    return Ꮡv;
}

// identity is the point at infinity.
internal static ж<Point> identity;
internal static error _ᴛ1ʗ;
internal static void initᴛidentity() { identity = @new<Point>().SetBytes(slice<byte>([
    1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0])).Item1; }

// NewIdentityPoint returns a new Point set to the identity.
public static ж<Point> NewIdentityPoint() {
    return @new<Point>().Set(identity);
}

// generator is the canonical curve basepoint. See TestGenerator for the
// correspondence of this encoding with the values in RFC 8032.
internal static ж<Point> generator;
internal static error _ᴛ2ʗ;
internal static void initᴛgenerator() { generator = @new<Point>().SetBytes(slice<byte>([
    0x58, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66,
    0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66,
    0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66,
    0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66])).Item1; }

// NewGeneratorPoint returns a new Point set to the canonical generator.
public static ж<Point> NewGeneratorPoint() {
    return @new<Point>().Set(generator);
}

internal static ж<projCached> Zero(this ж<projCached> Ꮡv) {
    Ꮡv.of(projCached.ᏑYplusX).One();
    Ꮡv.of(projCached.ᏑYminusX).One();
    Ꮡv.of(projCached.ᏑZ).One();
    Ꮡv.of(projCached.ᏑT2d).Zero();
    return Ꮡv;
}

internal static ж<affineCached> Zero(this ж<affineCached> Ꮡv) {
    Ꮡv.of(affineCached.ᏑYplusX).One();
    Ꮡv.of(affineCached.ᏑYminusX).One();
    Ꮡv.of(affineCached.ᏑT2d).Zero();
    return Ꮡv;
}

// Assignments.

// Set sets v = u, and returns v.
public static ж<Point> Set(this ж<Point> Ꮡv, ж<Point> Ꮡu) {
    ref var v = ref Ꮡv.DerefOrNull();
    ref var u = ref Ꮡu.DerefOrNull();

    v = u;
    return Ꮡv;
}

// Encoding.

// Bytes returns the canonical 32-byte encoding of v, according to RFC 8032,
// Section 5.1.2.
public static slice<byte> Bytes(this ж<Point> Ꮡv) {
    // This function is outlined to make the allocations inline in the caller
    // rather than happen on the heap.
    ref var buf = ref heap(new array<byte>(32), out var Ꮡbuf);
    return Ꮡv.bytes(Ꮡbuf);
}

internal static slice<byte> bytes(this ж<Point> Ꮡv, /*[32]*/ ж<array<byte>> buf) {
    checkInitialized(Ꮡv);

    ref var zInv = ref heap(new field.Element(), out var ᏑzInv);
    ref var x = ref heap(new field.Element(), out var Ꮡx);
    ref var y = ref heap(new field.Element(), out var Ꮡy);
    ᏑzInv.Invert(Ꮡv.of(Point.Ꮡz));       // zInv = 1 / Z
    Ꮡx.Multiply(Ꮡv.of(Point.Ꮡx), ᏑzInv); // x = X / Z
    Ꮡy.Multiply(Ꮡv.of(Point.Ꮡy), ᏑzInv); // y = Y / Z

    var @out = copyFieldElement(buf, Ꮡy);
    @out[31] |= (byte)((byte)(x.IsNegative() << 7));
    return @out;
}

internal static ж<field.Element> feOne = @new<field.Element>().One();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string edwards25519InvalidPointˢ = "edwards25519: invalid point encoding length"u8;
internal static readonly @string edwards25519InvalidPointˢ2 = "edwards25519: invalid point encoding"u8;

// SetBytes sets v = x, where x is a 32-byte encoding of v. If x does not
// represent a valid point on the curve, SetBytes returns nil and an error and
// the receiver is unchanged. Otherwise, SetBytes returns v.
//
// Note that SetBytes accepts all non-canonical encodings of valid points.
// That is, it follows decoding rules that match most implementations in
// the ecosystem rather than RFC 8032.
public static (ж<Point>, error) SetBytes(this ж<Point> Ꮡv, slice<byte> x) {
    // Specifically, the non-canonical encodings that are accepted are
    //   1) the ones where the field element is not reduced (see the
    //      (*field.Element).SetBytes docs) and
    //   2) the ones where the x-coordinate is zero and the sign bit is set.
    //
    // Read more at https://hdevalence.ca/blog/2020-10-04-its-25519am,
    // specifically the "Canonical A, R" section.

    var (y, err) = @new<field.Element>().SetBytes(x);
    if (err != nil) {
        return (default, errors.New(edwards25519InvalidPointˢ));
    }

    // -x² + y² = 1 + dx²y²
    // x² + dx²y² = x²(dy² + 1) = y² - 1
    // x² = (y² - 1) / (dy² + 1)

    // u = y² - 1
    var y2 = @new<field.Element>().Square(y);
    var u = @new<field.Element>().Subtract(y2, feOne);

    // v = dy² + 1
    var vv = @new<field.Element>().Multiply(y2, d);
    vv = vv.Add(vv, feOne);

    // x = +√(u/v)
    var (xx, wasSquare) = @new<field.Element>().SqrtRatio(u, vv);
    if (wasSquare == 0) {
        return (default, errors.New(edwards25519InvalidPointˢ2));
    }

    // Select the negative square root if the sign bit is set.
    var xxNeg = @new<field.Element>().Negate(xx);
    xx = xx.Select(xxNeg, xx, (nint)(x[31] >> 7));

    Ꮡv.of(Point.Ꮡx).Set(xx);
    Ꮡv.of(Point.Ꮡy).Set(y);
    Ꮡv.of(Point.Ꮡz).One();
    Ꮡv.of(Point.Ꮡt).Multiply(xx, y); // xy = T / Z

    return (Ꮡv, default);
}

internal static slice<byte> copyFieldElement(/*[32]*/ ж<array<byte>> Ꮡbuf, ж<field.Element> Ꮡv) {
    ref var buf = ref Ꮡbuf.DerefOrNull();
    ref var v = ref Ꮡv.DerefOrNull();

    copy(buf[..], v.Bytes());
    return buf[..];
}

// Conversions.
internal static ж<projP2> FromP1xP1(this ж<projP2> Ꮡv, ж<projP1xP1> p) {
    Ꮡv.of(projP2.ᏑX).Multiply(p.of(projP1xP1.ᏑX), p.of(projP1xP1.ᏑT));
    Ꮡv.of(projP2.ᏑY).Multiply(p.of(projP1xP1.ᏑY), p.of(projP1xP1.ᏑZ));
    Ꮡv.of(projP2.ᏑZ).Multiply(p.of(projP1xP1.ᏑZ), p.of(projP1xP1.ᏑT));
    return Ꮡv;
}

internal static ж<projP2> FromP3(this ж<projP2> Ꮡv, ж<Point> p) {
    Ꮡv.of(projP2.ᏑX).Set(p.of(Point.Ꮡx));
    Ꮡv.of(projP2.ᏑY).Set(p.of(Point.Ꮡy));
    Ꮡv.of(projP2.ᏑZ).Set(p.of(Point.Ꮡz));
    return Ꮡv;
}

internal static ж<Point> fromP1xP1(this ж<Point> Ꮡv, ж<projP1xP1> p) {
    Ꮡv.of(Point.Ꮡx).Multiply(p.of(projP1xP1.ᏑX), p.of(projP1xP1.ᏑT));
    Ꮡv.of(Point.Ꮡy).Multiply(p.of(projP1xP1.ᏑY), p.of(projP1xP1.ᏑZ));
    Ꮡv.of(Point.Ꮡz).Multiply(p.of(projP1xP1.ᏑZ), p.of(projP1xP1.ᏑT));
    Ꮡv.of(Point.Ꮡt).Multiply(p.of(projP1xP1.ᏑX), p.of(projP1xP1.ᏑY));
    return Ꮡv;
}

internal static ж<Point> fromP2(this ж<Point> Ꮡv, ж<projP2> p) {
    Ꮡv.of(Point.Ꮡx).Multiply(p.of(projP2.ᏑX), p.of(projP2.ᏑZ));
    Ꮡv.of(Point.Ꮡy).Multiply(p.of(projP2.ᏑY), p.of(projP2.ᏑZ));
    Ꮡv.of(Point.Ꮡz).Square(p.of(projP2.ᏑZ));
    Ꮡv.of(Point.Ꮡt).Multiply(p.of(projP2.ᏑX), p.of(projP2.ᏑY));
    return Ꮡv;
}

// d is a constant in the curve equation.
internal static ж<field.Element> d = @new<field.Element>().SetBytes(slice<byte>([
    0xa3, 0x78, 0x59, 0x13, 0xca, 0x4d, 0xeb, 0x75,
    0xab, 0xd8, 0x41, 0x41, 0x4d, 0x0a, 0x70, 0x00,
    0x98, 0xe8, 0x79, 0x77, 0x79, 0x40, 0xc7, 0x8c,
    0x73, 0xfe, 0x6f, 0x2b, 0xee, 0x6c, 0x03, 0x52])).Item1;
internal static error _ᴛ3ʗ;

internal static ж<field.Element> d2 = @new<field.Element>().Add(d, d);

internal static ж<projCached> FromP3(this ж<projCached> Ꮡv, ж<Point> p) {
    Ꮡv.of(projCached.ᏑYplusX).Add(p.of(Point.Ꮡy), p.of(Point.Ꮡx));
    Ꮡv.of(projCached.ᏑYminusX).Subtract(p.of(Point.Ꮡy), p.of(Point.Ꮡx));
    Ꮡv.of(projCached.ᏑZ).Set(p.of(Point.Ꮡz));
    Ꮡv.of(projCached.ᏑT2d).Multiply(p.of(Point.Ꮡt), d2);
    return Ꮡv;
}

internal static ж<affineCached> FromP3(this ж<affineCached> Ꮡv, ж<Point> p) {
    Ꮡv.of(affineCached.ᏑYplusX).Add(p.of(Point.Ꮡy), p.of(Point.Ꮡx));
    Ꮡv.of(affineCached.ᏑYminusX).Subtract(p.of(Point.Ꮡy), p.of(Point.Ꮡx));
    Ꮡv.of(affineCached.ᏑT2d).Multiply(p.of(Point.Ꮡt), d2);

    ref var invZ = ref heap(new field.Element(), out var ᏑinvZ);
    ᏑinvZ.Invert(p.of(Point.Ꮡz));
    Ꮡv.of(affineCached.ᏑYplusX).Multiply(Ꮡv.of(affineCached.ᏑYplusX), ᏑinvZ);
    Ꮡv.of(affineCached.ᏑYminusX).Multiply(Ꮡv.of(affineCached.ᏑYminusX), ᏑinvZ);
    Ꮡv.of(affineCached.ᏑT2d).Multiply(Ꮡv.of(affineCached.ᏑT2d), ᏑinvZ);
    return Ꮡv;
}

// (Re)addition and subtraction.

// Add sets v = p + q, and returns v.
public static ж<Point> Add(this ж<Point> Ꮡv, ж<Point> p, ж<Point> q) {
    checkInitialized(p, q);
    var qCached = @new<projCached>().FromP3(q);
    var result = @new<projP1xP1>().Add(p, qCached);
    return Ꮡv.fromP1xP1(result);
}

// Subtract sets v = p - q, and returns v.
public static ж<Point> Subtract(this ж<Point> Ꮡv, ж<Point> p, ж<Point> q) {
    checkInitialized(p, q);
    var qCached = @new<projCached>().FromP3(q);
    var result = @new<projP1xP1>().Sub(p, qCached);
    return Ꮡv.fromP1xP1(result);
}

internal static ж<projP1xP1> Add(this ж<projP1xP1> Ꮡv, ж<Point> p, ж<projCached> q) {
    ref var YplusX = ref heap(new field.Element(), out var ᏑYplusX);
    ref var YminusX = ref heap(new field.Element(), out var ᏑYminusX);
    ref var PP = ref heap(new field.Element(), out var ᏑPP);
    ref var MM = ref heap(new field.Element(), out var ᏑMM);
    ref var TT2d = ref heap(new field.Element(), out var ᏑTT2d);
    ref var ZZ2 = ref heap(new field.Element(), out var ᏑZZ2);

    ᏑYplusX.Add(p.of(Point.Ꮡy), p.of(Point.Ꮡx));
    ᏑYminusX.Subtract(p.of(Point.Ꮡy), p.of(Point.Ꮡx));

    ᏑPP.Multiply(ᏑYplusX, q.of(projCached.ᏑYplusX));
    ᏑMM.Multiply(ᏑYminusX, q.of(projCached.ᏑYminusX));
    ᏑTT2d.Multiply(p.of(Point.Ꮡt), q.of(projCached.ᏑT2d));
    ᏑZZ2.Multiply(p.of(Point.Ꮡz), q.of(projCached.ᏑZ));

    ᏑZZ2.Add(ᏑZZ2, ᏑZZ2);

    Ꮡv.of(projP1xP1.ᏑX).Subtract(ᏑPP, ᏑMM);
    Ꮡv.of(projP1xP1.ᏑY).Add(ᏑPP, ᏑMM);
    Ꮡv.of(projP1xP1.ᏑZ).Add(ᏑZZ2, ᏑTT2d);
    Ꮡv.of(projP1xP1.ᏑT).Subtract(ᏑZZ2, ᏑTT2d);
    return Ꮡv;
}

internal static ж<projP1xP1> Sub(this ж<projP1xP1> Ꮡv, ж<Point> p, ж<projCached> q) {
    ref var YplusX = ref heap(new field.Element(), out var ᏑYplusX);
    ref var YminusX = ref heap(new field.Element(), out var ᏑYminusX);
    ref var PP = ref heap(new field.Element(), out var ᏑPP);
    ref var MM = ref heap(new field.Element(), out var ᏑMM);
    ref var TT2d = ref heap(new field.Element(), out var ᏑTT2d);
    ref var ZZ2 = ref heap(new field.Element(), out var ᏑZZ2);

    ᏑYplusX.Add(p.of(Point.Ꮡy), p.of(Point.Ꮡx));
    ᏑYminusX.Subtract(p.of(Point.Ꮡy), p.of(Point.Ꮡx));

    ᏑPP.Multiply(ᏑYplusX, q.of(projCached.ᏑYminusX)); // flipped sign
    ᏑMM.Multiply(ᏑYminusX, q.of(projCached.ᏑYplusX)); // flipped sign
    ᏑTT2d.Multiply(p.of(Point.Ꮡt), q.of(projCached.ᏑT2d));
    ᏑZZ2.Multiply(p.of(Point.Ꮡz), q.of(projCached.ᏑZ));

    ᏑZZ2.Add(ᏑZZ2, ᏑZZ2);

    Ꮡv.of(projP1xP1.ᏑX).Subtract(ᏑPP, ᏑMM);
    Ꮡv.of(projP1xP1.ᏑY).Add(ᏑPP, ᏑMM);
    Ꮡv.of(projP1xP1.ᏑZ).Subtract(ᏑZZ2, ᏑTT2d); // flipped sign
    Ꮡv.of(projP1xP1.ᏑT).Add(ᏑZZ2, ᏑTT2d);      // flipped sign
    return Ꮡv;
}

internal static ж<projP1xP1> AddAffine(this ж<projP1xP1> Ꮡv, ж<Point> p, ж<affineCached> q) {
    ref var YplusX = ref heap(new field.Element(), out var ᏑYplusX);
    ref var YminusX = ref heap(new field.Element(), out var ᏑYminusX);
    ref var PP = ref heap(new field.Element(), out var ᏑPP);
    ref var MM = ref heap(new field.Element(), out var ᏑMM);
    ref var TT2d = ref heap(new field.Element(), out var ᏑTT2d);
    ref var Z2 = ref heap(new field.Element(), out var ᏑZ2);

    ᏑYplusX.Add(p.of(Point.Ꮡy), p.of(Point.Ꮡx));
    ᏑYminusX.Subtract(p.of(Point.Ꮡy), p.of(Point.Ꮡx));

    ᏑPP.Multiply(ᏑYplusX, q.of(affineCached.ᏑYplusX));
    ᏑMM.Multiply(ᏑYminusX, q.of(affineCached.ᏑYminusX));
    ᏑTT2d.Multiply(p.of(Point.Ꮡt), q.of(affineCached.ᏑT2d));

    ᏑZ2.Add(p.of(Point.Ꮡz), p.of(Point.Ꮡz));

    Ꮡv.of(projP1xP1.ᏑX).Subtract(ᏑPP, ᏑMM);
    Ꮡv.of(projP1xP1.ᏑY).Add(ᏑPP, ᏑMM);
    Ꮡv.of(projP1xP1.ᏑZ).Add(ᏑZ2, ᏑTT2d);
    Ꮡv.of(projP1xP1.ᏑT).Subtract(ᏑZ2, ᏑTT2d);
    return Ꮡv;
}

internal static ж<projP1xP1> SubAffine(this ж<projP1xP1> Ꮡv, ж<Point> p, ж<affineCached> q) {
    ref var YplusX = ref heap(new field.Element(), out var ᏑYplusX);
    ref var YminusX = ref heap(new field.Element(), out var ᏑYminusX);
    ref var PP = ref heap(new field.Element(), out var ᏑPP);
    ref var MM = ref heap(new field.Element(), out var ᏑMM);
    ref var TT2d = ref heap(new field.Element(), out var ᏑTT2d);
    ref var Z2 = ref heap(new field.Element(), out var ᏑZ2);

    ᏑYplusX.Add(p.of(Point.Ꮡy), p.of(Point.Ꮡx));
    ᏑYminusX.Subtract(p.of(Point.Ꮡy), p.of(Point.Ꮡx));

    ᏑPP.Multiply(ᏑYplusX, q.of(affineCached.ᏑYminusX)); // flipped sign
    ᏑMM.Multiply(ᏑYminusX, q.of(affineCached.ᏑYplusX)); // flipped sign
    ᏑTT2d.Multiply(p.of(Point.Ꮡt), q.of(affineCached.ᏑT2d));

    ᏑZ2.Add(p.of(Point.Ꮡz), p.of(Point.Ꮡz));

    Ꮡv.of(projP1xP1.ᏑX).Subtract(ᏑPP, ᏑMM);
    Ꮡv.of(projP1xP1.ᏑY).Add(ᏑPP, ᏑMM);
    Ꮡv.of(projP1xP1.ᏑZ).Subtract(ᏑZ2, ᏑTT2d); // flipped sign
    Ꮡv.of(projP1xP1.ᏑT).Add(ᏑZ2, ᏑTT2d);      // flipped sign
    return Ꮡv;
}

// Doubling.
internal static ж<projP1xP1> Double(this ж<projP1xP1> Ꮡv, ж<projP2> p) {
    ref var XX = ref heap(new field.Element(), out var ᏑXX);
    ref var YY = ref heap(new field.Element(), out var ᏑYY);
    ref var ZZ2 = ref heap(new field.Element(), out var ᏑZZ2);
    ref var XplusYsq = ref heap(new field.Element(), out var ᏑXplusYsq);

    ᏑXX.Square(p.of(projP2.ᏑX));
    ᏑYY.Square(p.of(projP2.ᏑY));
    ᏑZZ2.Square(p.of(projP2.ᏑZ));
    ᏑZZ2.Add(ᏑZZ2, ᏑZZ2);
    ᏑXplusYsq.Add(p.of(projP2.ᏑX), p.of(projP2.ᏑY));
    ᏑXplusYsq.Square(ᏑXplusYsq);

    Ꮡv.of(projP1xP1.ᏑY).Add(ᏑYY, ᏑXX);
    Ꮡv.of(projP1xP1.ᏑZ).Subtract(ᏑYY, ᏑXX);

    Ꮡv.of(projP1xP1.ᏑX).Subtract(ᏑXplusYsq, Ꮡv.of(projP1xP1.ᏑY));
    Ꮡv.of(projP1xP1.ᏑT).Subtract(ᏑZZ2, Ꮡv.of(projP1xP1.ᏑZ));
    return Ꮡv;
}

// Negation.

// Negate sets v = -p, and returns v.
public static ж<Point> Negate(this ж<Point> Ꮡv, ж<Point> p) {
    checkInitialized(p);
    Ꮡv.of(Point.Ꮡx).Negate(p.of(Point.Ꮡx));
    Ꮡv.of(Point.Ꮡy).Set(p.of(Point.Ꮡy));
    Ꮡv.of(Point.Ꮡz).Set(p.of(Point.Ꮡz));
    Ꮡv.of(Point.Ꮡt).Negate(p.of(Point.Ꮡt));
    return Ꮡv;
}

// Equal returns 1 if v is equivalent to u, and 0 otherwise.
public static nint Equal(this ж<Point> Ꮡv, ж<Point> u) {
    checkInitialized(Ꮡv, u);

    ref var t1 = ref heap(new field.Element(), out var Ꮡt1);
    ref var t2 = ref heap(new field.Element(), out var Ꮡt2);
    ref var t3 = ref heap(new field.Element(), out var Ꮡt3);
    ref var t4 = ref heap(new field.Element(), out var Ꮡt4);
    Ꮡt1.Multiply(Ꮡv.of(Point.Ꮡx), u.of(Point.Ꮡz));
    Ꮡt2.Multiply(u.of(Point.Ꮡx), Ꮡv.of(Point.Ꮡz));
    Ꮡt3.Multiply(Ꮡv.of(Point.Ꮡy), u.of(Point.Ꮡz));
    Ꮡt4.Multiply(u.of(Point.Ꮡy), Ꮡv.of(Point.Ꮡz));

    return t1.Equal(Ꮡt2) & t3.Equal(Ꮡt4);
}

// Constant-time operations

// Select sets v to a if cond == 1 and to b if cond == 0.
internal static ж<projCached> Select(this ж<projCached> Ꮡv, ж<projCached> a, ж<projCached> b, nint cond) {
    Ꮡv.of(projCached.ᏑYplusX).Select(a.of(projCached.ᏑYplusX), b.of(projCached.ᏑYplusX), cond);
    Ꮡv.of(projCached.ᏑYminusX).Select(a.of(projCached.ᏑYminusX), b.of(projCached.ᏑYminusX), cond);
    Ꮡv.of(projCached.ᏑZ).Select(a.of(projCached.ᏑZ), b.of(projCached.ᏑZ), cond);
    Ꮡv.of(projCached.ᏑT2d).Select(a.of(projCached.ᏑT2d), b.of(projCached.ᏑT2d), cond);
    return Ꮡv;
}

// Select sets v to a if cond == 1 and to b if cond == 0.
internal static ж<affineCached> Select(this ж<affineCached> Ꮡv, ж<affineCached> a, ж<affineCached> b, nint cond) {
    Ꮡv.of(affineCached.ᏑYplusX).Select(a.of(affineCached.ᏑYplusX), b.of(affineCached.ᏑYplusX), cond);
    Ꮡv.of(affineCached.ᏑYminusX).Select(a.of(affineCached.ᏑYminusX), b.of(affineCached.ᏑYminusX), cond);
    Ꮡv.of(affineCached.ᏑT2d).Select(a.of(affineCached.ᏑT2d), b.of(affineCached.ᏑT2d), cond);
    return Ꮡv;
}

// CondNeg negates v if cond == 1 and leaves it unchanged if cond == 0.
internal static ж<projCached> CondNeg(this ж<projCached> Ꮡv, nint cond) {
    ref var v = ref Ꮡv.DerefOrNull();

    v.YplusX.Swap(Ꮡv.of(projCached.ᏑYminusX), cond);
    Ꮡv.of(projCached.ᏑT2d).Select(@new<field.Element>().Negate(Ꮡv.of(projCached.ᏑT2d)), Ꮡv.of(projCached.ᏑT2d), cond);
    return Ꮡv;
}

// CondNeg negates v if cond == 1 and leaves it unchanged if cond == 0.
internal static ж<affineCached> CondNeg(this ж<affineCached> Ꮡv, nint cond) {
    ref var v = ref Ꮡv.DerefOrNull();

    v.YplusX.Swap(Ꮡv.of(affineCached.ᏑYminusX), cond);
    Ꮡv.of(affineCached.ᏑT2d).Select(@new<field.Element>().Negate(Ꮡv.of(affineCached.ᏑT2d)), Ꮡv.of(affineCached.ᏑT2d), cond);
    return Ꮡv;
}

} // end edwards25519_package
