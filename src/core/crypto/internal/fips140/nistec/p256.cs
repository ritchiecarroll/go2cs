// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !arm64 && !ppc64le && !s390x) || purego
namespace go.crypto.@internal.fips140;

using fiat = go.crypto.@internal.fips140.nistec.fiat_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using cpu = go.crypto.@internal.fips140deps.cpu_package;
using errors = errors_package;
using bits = math.bits_package;
using sync = sync_package;
using @unsafe = unsafe_package;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140.nistec;
using go.crypto.@internal.fips140deps;
using math;

partial class nistec_package {

// P256Point is a P-256 point. The zero value is NOT valid.
partial struct P256Point {
    // The point is represented in projective coordinates (X:Y:Z), where x = X/Z
    // and y = Y/Z. Infinity is (0:1:0).
    //
    // fiat.P256Element is a base field element in [0, P-1] in the Montgomery
    // domain (with R 2²⁵⁶ and P 2²⁵⁶ - 2²²⁴ + 2¹⁹² + 2⁹⁶ - 1) as four limbs in
    // little-endian order value.
    internal fiat.P256Element x, y, z;
}

// NewP256Point returns a new P256Point representing the point at infinity point.
public static ж<P256Point> NewP256Point() {
    var p = Ꮡ(new P256Point(nil));
    p.of(P256Point.Ꮡy).One();
    return p;
}

// SetGenerator sets p to the canonical generator and returns p.
public static ж<P256Point> SetGenerator(this ж<P256Point> Ꮡp) {
    Ꮡp.of(P256Point.Ꮡx).SetBytes(new byte[]{0x6b, 0x17, 0xd1, 0xf2, 0xe1, 0x2c, 0x42, 0x47, 0xf8, 0xbc, 0xe6, 0xe5, 0x63, 0xa4, 0x40, 0xf2, 0x77, 0x3, 0x7d, 0x81, 0x2d, 0xeb, 0x33, 0xa0, 0xf4, 0xa1, 0x39, 0x45, 0xd8, 0x98, 0xc2, 0x96}.slice());
    Ꮡp.of(P256Point.Ꮡy).SetBytes(new byte[]{0x4f, 0xe3, 0x42, 0xe2, 0xfe, 0x1a, 0x7f, 0x9b, 0x8e, 0xe7, 0xeb, 0x4a, 0x7c, 0xf, 0x9e, 0x16, 0x2b, 0xce, 0x33, 0x57, 0x6b, 0x31, 0x5e, 0xce, 0xcb, 0xb6, 0x40, 0x68, 0x37, 0xbf, 0x51, 0xf5}.slice());
    Ꮡp.of(P256Point.Ꮡz).One();
    return Ꮡp;
}

// Set sets p = q and returns p.
public static ж<P256Point> Set(this ж<P256Point> Ꮡp, ж<P256Point> Ꮡq) {
    Ꮡp.of(P256Point.Ꮡx).Set(Ꮡq.of(P256Point.Ꮡx));
    Ꮡp.of(P256Point.Ꮡy).Set(Ꮡq.of(P256Point.Ꮡy));
    Ꮡp.of(P256Point.Ꮡz).Set(Ꮡq.of(P256Point.Ꮡz));
    return Ꮡp;
}

internal static UntypedInt p256ElementLength => 32;

internal static UntypedInt p256UncompressedLength => /* 1 + 2*p256ElementLength */ 65;

internal static UntypedInt p256CompressedLength => /* 1 + p256ElementLength */ 33;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidP256Compressedˢ = "invalid P256 compressed point encoding"u8;
internal static readonly @string invalidP256PointEncodingˢ = "invalid P256 point encoding"u8;

// SetBytes sets p to the compressed, uncompressed, or infinity value encoded in
// b, as specified in SEC 1, Version 2.0, Section 2.3.4. If the point is not on
// the curve, it returns nil and an error, and the receiver is unchanged.
// Otherwise, it returns p.
public static (ж<P256Point>, error) SetBytes(this ж<P256Point> Ꮡp, slice<byte> b) {
    switch (ᐧ) {
    case {} when len(b) == 1 && b[0] == 0: {
        return (Ꮡp.Set(NewP256Point()), default!);
    }
    case {} when len(b) == p256UncompressedLength && b[0] == 4: {
        var (x, err) = @new<fiat.P256Element>().SetBytes(b[1..(int)(1 + p256ElementLength)]);
        if (err != default!) {
            // Point at infinity.
            // Uncompressed form.
            return (default!, err);
        }
        (var y, err) = @new<fiat.P256Element>().SetBytes(b[(int)(1 + p256ElementLength)..]);
        if (err != default!) {
            return (default!, err);
        }
        {
            var errΔ1 = p256CheckOnCurve(x, y); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        Ꮡp.of(P256Point.Ꮡx).Set(x);
        Ꮡp.of(P256Point.Ꮡy).Set(y);
        Ꮡp.of(P256Point.Ꮡz).One();
        return (Ꮡp, default!);
    }
    case {} when len(b) == p256CompressedLength && (b[0] == 2 || b[0] == 3): {
        var (x, err) = @new<fiat.P256Element>().SetBytes(b[1..]);
        if (err != default!) {
            // Compressed form.
            return (default!, err);
        }
        var y = p256Polynomial(@new<fiat.P256Element>(), // y² = x³ - 3x + b
 x);
        if (!p256Sqrt(y, y)) {
            return (default!, errors.New(invalidP256Compressedˢ));
        }
        var otherRoot = @new<fiat.P256Element>();
        otherRoot.Sub(otherRoot, // Select the positive or negative root, as indicated by the least
 // significant bit, based on the encoding type byte.
 y);
        var cond = (byte)((byte)(y.Bytes()[p256ElementLength - 1] & 1) ^ (byte)(b[0] & 1));
        y.Select(otherRoot, y, (nint)cond);
        Ꮡp.of(P256Point.Ꮡx).Set(x);
        Ꮡp.of(P256Point.Ꮡy).Set(y);
        Ꮡp.of(P256Point.Ꮡz).One();
        return (Ꮡp, default!);
    }
    default: {
        return (default!, errors.New(invalidP256PointEncodingˢ));
    }}

}

internal static ж<fiat.P256Element> _p256B;

internal static ж<sync.Once> Ꮡ_p256BOnce = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once _p256BOnce => ref Ꮡ_p256BOnce.Value;

internal static ж<fiat.P256Element> p256B() {
    Ꮡ_p256BOnce.Do(() => {
        (_p256B, _) = @new<fiat.P256Element>().SetBytes(new byte[]{0x5a, 0xc6, 0x35, 0xd8, 0xaa, 0x3a, 0x93, 0xe7, 0xb3, 0xeb, 0xbd, 0x55, 0x76, 0x98, 0x86, 0xbc, 0x65, 0x1d, 0x6, 0xb0, 0xcc, 0x53, 0xb0, 0xf6, 0x3b, 0xce, 0x3c, 0x3e, 0x27, 0xd2, 0x60, 0x4b}.slice());
    });
    return _p256B;
}

// p256Polynomial sets y2 to x³ - 3x + b, and returns y2.
internal static ж<fiat.P256Element> p256Polynomial(ж<fiat.P256Element> Ꮡy2, ж<fiat.P256Element> Ꮡx) {
    Ꮡy2.Square(Ꮡx);
    Ꮡy2.Mul(Ꮡy2, Ꮡx);
    var threeX = @new<fiat.P256Element>().Add(Ꮡx, Ꮡx);
    threeX.Add(threeX, Ꮡx);
    Ꮡy2.Sub(Ꮡy2, threeX);
    return Ꮡy2.Add(Ꮡy2, p256B());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string p256PointNotOnCurveˢ = "P256 point not on curve"u8;

internal static error p256CheckOnCurve(ж<fiat.P256Element> Ꮡx, ж<fiat.P256Element> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    // y² = x³ - 3x + b
    var rhs = p256Polynomial(@new<fiat.P256Element>(), Ꮡx);
    var lhs = @new<fiat.P256Element>().Square(Ꮡy);
    if (rhs.Equal(lhs) != 1) {
        return errors.New(p256PointNotOnCurveˢ);
    }
    return default!;
}

// Bytes returns the uncompressed or infinity encoding of p, as specified in
// SEC 1, Version 2.0, Section 2.3.3. Note that the encoding of the point at
// infinity is shorter than all other encodings.
public static slice<byte> Bytes(this ж<P256Point> Ꮡp) {
    // This function is outlined to make the allocations inline in the caller
    // rather than happen on the heap.
    ref var @out = ref heap(new array<byte>(65), out var Ꮡout);
    return Ꮡp.bytes(Ꮡout);
}

internal static slice<byte> bytes(this ж<P256Point> Ꮡp, /*[65]*/ ж<array<byte>> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    // The SEC 1 representation of the point at infinity is a single zero byte,
    // and only infinity has z = 0.
    if (Ꮡp.of(P256Point.Ꮡz).IsZero() == 1) {
        return append(@out[..0], (byte)(0));
    }
    var zinv = @new<fiat.P256Element>().Invert(Ꮡp.of(P256Point.Ꮡz));
    var x = @new<fiat.P256Element>().Mul(Ꮡp.of(P256Point.Ꮡx), zinv);
    var y = @new<fiat.P256Element>().Mul(Ꮡp.of(P256Point.Ꮡy), zinv);
    var buf = append(@out[..0], (byte)(4));
    buf = appendꓸꓸꓸ(buf, x.Bytes());
    buf = appendꓸꓸꓸ(buf, y.Bytes());
    return buf;
}

// BytesX returns the encoding of the x-coordinate of p, as specified in SEC 1,
// Version 2.0, Section 2.3.5, or an error if p is the point at infinity.
public static (slice<byte>, error) BytesX(this ж<P256Point> Ꮡp) {
    // This function is outlined to make the allocations inline in the caller
    // rather than happen on the heap.
    ref var @out = ref heap(new array<byte>(32), out var Ꮡout);
    return Ꮡp.bytesX(Ꮡout);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string p256PointIsThePointAtˢ = "P256 point is the point at infinity"u8;

internal static (slice<byte>, error) bytesX(this ж<P256Point> Ꮡp, /*[32]*/ ж<array<byte>> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    if (Ꮡp.of(P256Point.Ꮡz).IsZero() == 1) {
        return (default!, errors.New(p256PointIsThePointAtˢ));
    }
    var zinv = @new<fiat.P256Element>().Invert(Ꮡp.of(P256Point.Ꮡz));
    var x = @new<fiat.P256Element>().Mul(Ꮡp.of(P256Point.Ꮡx), zinv);
    return (appendꓸꓸꓸ(@out[..0], x.Bytes()), default!);
}

// BytesCompressed returns the compressed or infinity encoding of p, as
// specified in SEC 1, Version 2.0, Section 2.3.3. Note that the encoding of the
// point at infinity is shorter than all other encodings.
public static slice<byte> BytesCompressed(this ж<P256Point> Ꮡp) {
    // This function is outlined to make the allocations inline in the caller
    // rather than happen on the heap.
    ref var @out = ref heap(new array<byte>(33), out var Ꮡout);
    return Ꮡp.bytesCompressed(Ꮡout);
}

internal static slice<byte> bytesCompressed(this ж<P256Point> Ꮡp, /*[33]*/ ж<array<byte>> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    if (Ꮡp.of(P256Point.Ꮡz).IsZero() == 1) {
        return append(@out[..0], (byte)(0));
    }
    var zinv = @new<fiat.P256Element>().Invert(Ꮡp.of(P256Point.Ꮡz));
    var x = @new<fiat.P256Element>().Mul(Ꮡp.of(P256Point.Ꮡx), zinv);
    var y = @new<fiat.P256Element>().Mul(Ꮡp.of(P256Point.Ꮡy), zinv);
    // Encode the sign of the y coordinate (indicated by the least significant
    // bit) as the encoding type (2 or 3).
    var buf = append(@out[..0], (byte)(2));
    buf[0] |= (byte)((byte)(y.Bytes()[p256ElementLength - 1] & 1));
    buf = appendꓸꓸꓸ(buf, x.Bytes());
    return buf;
}

// Add sets q = p1 + p2, and returns q. The points may overlap.
public static ж<P256Point> Add(this ж<P256Point> Ꮡq, ж<P256Point> Ꮡp1, ж<P256Point> Ꮡp2) {
    // Complete addition formula for a = -3 from "Complete addition formulas for
    // prime order elliptic curves" (https://eprint.iacr.org/2015/1060), §A.2.
    var t0 = @new<fiat.P256Element>().Mul(Ꮡp1.of(P256Point.Ꮡx), Ꮡp2.of(P256Point.Ꮡx)); // t0 := X1 * X2
    var t1 = @new<fiat.P256Element>().Mul(Ꮡp1.of(P256Point.Ꮡy), Ꮡp2.of(P256Point.Ꮡy)); // t1 := Y1 * Y2
    var t2 = @new<fiat.P256Element>().Mul(Ꮡp1.of(P256Point.Ꮡz), Ꮡp2.of(P256Point.Ꮡz)); // t2 := Z1 * Z2
    var t3 = @new<fiat.P256Element>().Add(Ꮡp1.of(P256Point.Ꮡx), Ꮡp1.of(P256Point.Ꮡy)); // t3 := X1 + Y1
    var t4 = @new<fiat.P256Element>().Add(Ꮡp2.of(P256Point.Ꮡx), Ꮡp2.of(P256Point.Ꮡy)); // t4 := X2 + Y2
    t3.Mul(t3, t4); // t3 := t3 * t4
    t4.Add(t0, t1); // t4 := t0 + t1
    t3.Sub(t3, t4); // t3 := t3 - t4
    t4.Add(Ꮡp1.of(P256Point.Ꮡy), Ꮡp1.of(P256Point.Ꮡz)); // t4 := Y1 + Z1
    var x3 = @new<fiat.P256Element>().Add(Ꮡp2.of(P256Point.Ꮡy), Ꮡp2.of(P256Point.Ꮡz)); // X3 := Y2 + Z2
    t4.Mul(t4, x3); // t4 := t4 * X3
    x3.Add(t1, t2); // X3 := t1 + t2
    t4.Sub(t4, x3); // t4 := t4 - X3
    x3.Add(Ꮡp1.of(P256Point.Ꮡx), Ꮡp1.of(P256Point.Ꮡz)); // X3 := X1 + Z1
    var y3 = @new<fiat.P256Element>().Add(Ꮡp2.of(P256Point.Ꮡx), Ꮡp2.of(P256Point.Ꮡz)); // Y3 := X2 + Z2
    x3.Mul(x3, y3); // X3 := X3 * Y3
    y3.Add(t0, t2); // Y3 := t0 + t2
    y3.Sub(x3, y3); // Y3 := X3 - Y3
    var z3 = @new<fiat.P256Element>().Mul(p256B(), t2); // Z3 := b * t2
    x3.Sub(y3, z3); // X3 := Y3 - Z3
    z3.Add(x3, x3); // Z3 := X3 + X3
    x3.Add(x3, z3); // X3 := X3 + Z3
    z3.Sub(t1, x3); // Z3 := t1 - X3
    x3.Add(t1, x3); // X3 := t1 + X3
    y3.Mul(p256B(), y3); // Y3 := b * Y3
    t1.Add(t2, t2); // t1 := t2 + t2
    t2.Add(t1, t2); // t2 := t1 + t2
    y3.Sub(y3, t2); // Y3 := Y3 - t2
    y3.Sub(y3, t0); // Y3 := Y3 - t0
    t1.Add(y3, y3); // t1 := Y3 + Y3
    y3.Add(t1, y3); // Y3 := t1 + Y3
    t1.Add(t0, t0); // t1 := t0 + t0
    t0.Add(t1, t0); // t0 := t1 + t0
    t0.Sub(t0, t2); // t0 := t0 - t2
    t1.Mul(t4, y3); // t1 := t4 * Y3
    t2.Mul(t0, y3); // t2 := t0 * Y3
    y3.Mul(x3, z3); // Y3 := X3 * Z3
    y3.Add(y3, t2); // Y3 := Y3 + t2
    x3.Mul(t3, x3); // X3 := t3 * X3
    x3.Sub(x3, t1); // X3 := X3 - t1
    z3.Mul(t4, z3); // Z3 := t4 * Z3
    t1.Mul(t3, t0); // t1 := t3 * t0
    z3.Add(z3, t1); // Z3 := Z3 + t1
    Ꮡq.of(P256Point.Ꮡx).Set(x3);
    Ꮡq.of(P256Point.Ꮡy).Set(y3);
    Ꮡq.of(P256Point.Ꮡz).Set(z3);
    return Ꮡq;
}

// Double sets q = p + p, and returns q. The points may overlap.
public static ж<P256Point> Double(this ж<P256Point> Ꮡq, ж<P256Point> Ꮡp) {
    // Complete addition formula for a = -3 from "Complete addition formulas for
    // prime order elliptic curves" (https://eprint.iacr.org/2015/1060), §A.2.
    var t0 = @new<fiat.P256Element>().Square(Ꮡp.of(P256Point.Ꮡx)); // t0 := X ^ 2
    var t1 = @new<fiat.P256Element>().Square(Ꮡp.of(P256Point.Ꮡy)); // t1 := Y ^ 2
    var t2 = @new<fiat.P256Element>().Square(Ꮡp.of(P256Point.Ꮡz)); // t2 := Z ^ 2
    var t3 = @new<fiat.P256Element>().Mul(Ꮡp.of(P256Point.Ꮡx), Ꮡp.of(P256Point.Ꮡy)); // t3 := X * Y
    t3.Add(t3, t3); // t3 := t3 + t3
    var z3 = @new<fiat.P256Element>().Mul(Ꮡp.of(P256Point.Ꮡx), Ꮡp.of(P256Point.Ꮡz)); // Z3 := X * Z
    z3.Add(z3, z3); // Z3 := Z3 + Z3
    var y3 = @new<fiat.P256Element>().Mul(p256B(), t2); // Y3 := b * t2
    y3.Sub(y3, z3); // Y3 := Y3 - Z3
    var x3 = @new<fiat.P256Element>().Add(y3, y3); // X3 := Y3 + Y3
    y3.Add(x3, y3); // Y3 := X3 + Y3
    x3.Sub(t1, y3); // X3 := t1 - Y3
    y3.Add(t1, y3); // Y3 := t1 + Y3
    y3.Mul(x3, y3); // Y3 := X3 * Y3
    x3.Mul(x3, t3); // X3 := X3 * t3
    t3.Add(t2, t2); // t3 := t2 + t2
    t2.Add(t2, t3); // t2 := t2 + t3
    z3.Mul(p256B(), z3); // Z3 := b * Z3
    z3.Sub(z3, t2); // Z3 := Z3 - t2
    z3.Sub(z3, t0); // Z3 := Z3 - t0
    t3.Add(z3, z3); // t3 := Z3 + Z3
    z3.Add(z3, t3); // Z3 := Z3 + t3
    t3.Add(t0, t0); // t3 := t0 + t0
    t0.Add(t3, t0); // t0 := t3 + t0
    t0.Sub(t0, t2); // t0 := t0 - t2
    t0.Mul(t0, z3); // t0 := t0 * Z3
    y3.Add(y3, t0); // Y3 := Y3 + t0
    t0.Mul(Ꮡp.of(P256Point.Ꮡy), Ꮡp.of(P256Point.Ꮡz)); // t0 := Y * Z
    t0.Add(t0, t0); // t0 := t0 + t0
    z3.Mul(t0, z3); // Z3 := t0 * Z3
    x3.Sub(x3, z3); // X3 := X3 - Z3
    z3.Mul(t0, t1); // Z3 := t0 * t1
    z3.Add(z3, z3); // Z3 := Z3 + Z3
    z3.Add(z3, z3); // Z3 := Z3 + Z3
    Ꮡq.of(P256Point.Ꮡx).Set(x3);
    Ꮡq.of(P256Point.Ꮡy).Set(y3);
    Ꮡq.of(P256Point.Ꮡz).Set(z3);
    return Ꮡq;
}

// p256AffinePoint is a point in affine coordinates (x, y). x and y are still
// Montgomery domain elements. The point can't be the point at infinity.
public partial struct p256AffinePoint {
    internal fiat.P256Element x, y;
}

public static ж<P256Point> Projective(this ref p256AffinePoint p) {
    var pp = Ꮡ(new P256Point(x: p.x.ΔClone(), y: p.y.ΔClone()));
    pp.of(P256Point.Ꮡz).One();
    return pp;
}

// AddAffine sets q = p1 + p2, if infinity == 0, and to p1 if infinity == 1.
// p2 can't be the point at infinity as it can't be represented in affine
// coordinates, instead callers can set p2 to an arbitrary point and set
// infinity to 1.
public static ж<P256Point> AddAffine(this ж<P256Point> Ꮡq, ж<P256Point> Ꮡp1, ж<p256AffinePoint> Ꮡp2, nint infinity) {
    // Complete mixed addition formula for a = -3 from "Complete addition
    // formulas for prime order elliptic curves"
    // (https://eprint.iacr.org/2015/1060), Algorithm 5.
    var t0 = @new<fiat.P256Element>().Mul(Ꮡp1.of(P256Point.Ꮡx), Ꮡp2.of(p256AffinePoint.Ꮡx)); // t0 ← X1 · X2
    var t1 = @new<fiat.P256Element>().Mul(Ꮡp1.of(P256Point.Ꮡy), Ꮡp2.of(p256AffinePoint.Ꮡy)); // t1 ← Y1 · Y2
    var t3 = @new<fiat.P256Element>().Add(Ꮡp2.of(p256AffinePoint.Ꮡx), Ꮡp2.of(p256AffinePoint.Ꮡy)); // t3 ← X2 + Y2
    var t4 = @new<fiat.P256Element>().Add(Ꮡp1.of(P256Point.Ꮡx), Ꮡp1.of(P256Point.Ꮡy)); // t4 ← X1 + Y1
    t3.Mul(t3, t4); // t3 ← t3 · t4
    t4.Add(t0, t1); // t4 ← t0 + t1
    t3.Sub(t3, t4); // t3 ← t3 − t4
    t4.Mul(Ꮡp2.of(p256AffinePoint.Ꮡy), Ꮡp1.of(P256Point.Ꮡz)); // t4 ← Y2 · Z1
    t4.Add(t4, Ꮡp1.of(P256Point.Ꮡy)); // t4 ← t4 + Y1
    var y3 = @new<fiat.P256Element>().Mul(Ꮡp2.of(p256AffinePoint.Ꮡx), Ꮡp1.of(P256Point.Ꮡz)); // Y3 ← X2 · Z1
    y3.Add(y3, Ꮡp1.of(P256Point.Ꮡx)); // Y3 ← Y3 + X1
    var z3 = @new<fiat.P256Element>().Mul(p256B(), Ꮡp1.of(P256Point.Ꮡz)); // Z3 ← b  · Z1
    var x3 = @new<fiat.P256Element>().Sub(y3, z3); // X3 ← Y3 − Z3
    z3.Add(x3, x3); // Z3 ← X3 + X3
    x3.Add(x3, z3); // X3 ← X3 + Z3
    z3.Sub(t1, x3); // Z3 ← t1 − X3
    x3.Add(t1, x3); // X3 ← t1 + X3
    y3.Mul(p256B(), y3); // Y3 ← b  · Y3
    t1.Add(Ꮡp1.of(P256Point.Ꮡz), Ꮡp1.of(P256Point.Ꮡz)); // t1 ← Z1 + Z1
    var t2 = @new<fiat.P256Element>().Add(t1, Ꮡp1.of(P256Point.Ꮡz)); // t2 ← t1 + Z1
    y3.Sub(y3, t2); // Y3 ← Y3 − t2
    y3.Sub(y3, t0); // Y3 ← Y3 − t0
    t1.Add(y3, y3); // t1 ← Y3 + Y3
    y3.Add(t1, y3); // Y3 ← t1 + Y3
    t1.Add(t0, t0); // t1 ← t0 + t0
    t0.Add(t1, t0); // t0 ← t1 + t0
    t0.Sub(t0, t2); // t0 ← t0 − t2
    t1.Mul(t4, y3); // t1 ← t4 · Y3
    t2.Mul(t0, y3); // t2 ← t0 · Y3
    y3.Mul(x3, z3); // Y3 ← X3 · Z3
    y3.Add(y3, t2); // Y3 ← Y3 + t2
    x3.Mul(t3, x3); // X3 ← t3 · X3
    x3.Sub(x3, t1); // X3 ← X3 − t1
    z3.Mul(t4, z3); // Z3 ← t4 · Z3
    t1.Mul(t3, t0); // t1 ← t3 · t0
    z3.Add(z3, t1); // Z3 ← Z3 + t1
    Ꮡq.of(P256Point.Ꮡx).Select(Ꮡp1.of(P256Point.Ꮡx), x3, infinity);
    Ꮡq.of(P256Point.Ꮡy).Select(Ꮡp1.of(P256Point.Ꮡy), y3, infinity);
    Ꮡq.of(P256Point.Ꮡz).Select(Ꮡp1.of(P256Point.Ꮡz), z3, infinity);
    return Ꮡq;
}

// Select sets q to p1 if cond == 1, and to p2 if cond == 0.
public static ж<P256Point> Select(this ж<P256Point> Ꮡq, ж<P256Point> Ꮡp1, ж<P256Point> Ꮡp2, nint cond) {
    Ꮡq.of(P256Point.Ꮡx).Select(Ꮡp1.of(P256Point.Ꮡx), Ꮡp2.of(P256Point.Ꮡx), cond);
    Ꮡq.of(P256Point.Ꮡy).Select(Ꮡp1.of(P256Point.Ꮡy), Ꮡp2.of(P256Point.Ꮡy), cond);
    Ꮡq.of(P256Point.Ꮡz).Select(Ꮡp1.of(P256Point.Ꮡz), Ꮡp2.of(P256Point.Ꮡz), cond);
    return Ꮡq;
}

partial struct p256OrdElement /*[4]uint64*/;

// SetBytes sets s to the big-endian value of x, reducing it as necessary.
internal static (ж<p256OrdElement>, error) SetBytes(this ж<p256OrdElement> Ꮡs, slice<byte> x) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (len(x) != 32) {
        return (ж<p256OrdElement>.NilBoxOfDims(4L), errors.New(invalidScalarLengthˢ));
    }
    s.Value[0] = byteorder.BEUint64(x[24..]);
    s.Value[1] = byteorder.BEUint64(x[16..]);
    s.Value[2] = byteorder.BEUint64(x[8..]);
    s.Value[3] = byteorder.BEUint64(x[..]);
    // Ensure s is in the range [0, ord(G)-1]. Since 2 * ord(G) > 2²⁵⁶, we can
    // just conditionally subtract ord(G), keeping the result if it doesn't
    // underflow.
    var (t0, b) = bits.Sub64(s.Value[0], 0xf3b9cac2fc632551UL, 0);
    (var t1, b) = bits.Sub64(s.Value[1], 0xbce6faada7179e84UL, b);
    (var t2, b) = bits.Sub64(s.Value[2], 0xffffffffffffffffUL, b);
    (var t3, b) = bits.Sub64(s.Value[3], 0xffffffff00000000UL, b);
    var tMask = b - 1; // zero if subtraction underflowed
    s.Value[0] ^= (uint64)((uint64)(((uint64)(t0 ^ s.Value[0])) & tMask));
    s.Value[1] ^= (uint64)((uint64)(((uint64)(t1 ^ s.Value[1])) & tMask));
    s.Value[2] ^= (uint64)((uint64)(((uint64)(t2 ^ s.Value[2])) & tMask));
    s.Value[3] ^= (uint64)((uint64)(((uint64)(t3 ^ s.Value[3])) & tMask));
    return (Ꮡs, default!);
}

internal static slice<byte> Bytes(this ref p256OrdElement s) {
    array<byte> @out = new(32);
    byteorder.BEPutUint64(@out[24..], s.Value[0]);
    byteorder.BEPutUint64(@out[16..], s.Value[1]);
    byteorder.BEPutUint64(@out[8..], s.Value[2]);
    byteorder.BEPutUint64(@out[..], s.Value[3]);
    return @out[..];
}

// Rsh returns the 64 least significant bits of x >> n. n must be lower
// than 256. The value of n leaks through timing side-channels.
internal static uint64 Rsh(this ж<p256OrdElement> Ꮡs, nint n) {
    ref var s = ref Ꮡs.DerefOrNull();

    nint i = n / 64;
    n = n % 64;
    var res = s.Value[i].Rsh((int64)(n));
    // Shift in the more significant limb, if present.
    {
        nint iΔ1 = i + 1; if (iΔ1 < 4) {
            res |= (uint64)(s.Value[iΔ1].Lsh((int64)((64 - n))));
        }
    }
    return res;
}

partial struct p256Table /*[16]P256Point*/;

// Select selects the n-th multiple of the table base point into p. It works in
// constant time. n must be in [0, 16]. If n is 0, p is set to the identity point.
internal static void Select(this ref p256Table table, ж<P256Point> Ꮡp, uint8 n) {
    if (n > 16) {
        throw panic("nistec: internal error: p256Table called with out-of-bounds value");
    }
    Ꮡp.Set(NewP256Point());
    for (var i = (uint8)1; i <= 16; i++) {
        nint cond = subtle.ConstantTimeByteEq(i, n);
        Ꮡp.Select(Ꮡ(table.Value, (uint8)(i - 1)), Ꮡp, cond);
    }
}

// Compute populates the table to the first 16 multiples of q.
internal static ж<p256Table> Compute(this ж<p256Table> Ꮡtable, ж<P256Point> Ꮡq) {
    ref var table = ref Ꮡtable.DerefOrNull();

    Ꮡtable.at<P256Point>(0).Set(Ꮡq);
    for (nint i = 1; i < 16; i += 2) {
        Ꮡtable.at<P256Point>(i).Double(Ꮡ(table.Value, i / 2));
        if (i + 1 < 16) {
            Ꮡtable.at<P256Point>(i + 1).Add(Ꮡ(table.Value, i), Ꮡq);
        }
    }
    return Ꮡtable;
}

internal static (uint8, nint) boothW5(uint64 @in) {
    var s = ~(((@in >> (int)(5))) - 1);
    var d = (((uint64)1 << (int)(6))) - @in - 1;
    d = (uint64)(((uint64)(d & s)) | ((uint64)(@in & (~s))));
    d = ((d >> (int)(1))) + ((uint64)(d & 1));
    return ((uint8)d, (nint)((uint64)(s & 1)));
}

// ScalarMult sets r = scalar * q, where scalar is a 32-byte big endian value,
// and returns r. If scalar is not 32 bytes long, ScalarMult returns an error
// and the receiver is unchanged.
public static (ж<P256Point>, error) ScalarMult(this ж<P256Point> Ꮡp, ж<P256Point> Ꮡq, slice<byte> scalar) {
    var (s, err) = @new<p256OrdElement>().SetBytes(scalar);
    if (err != default!) {
        return (default!, err);
    }
    // Start scanning the window from the most significant bits. We move by
    // 5 bits at a time and need to finish at -1, so -1 + 5 * 51 = 254.
    nint index = 254;
    var (sel, sign) = boothW5(s.Rsh(index));
    // sign is always zero because the boothW5 input here is at
    // most two bits long, so the top bit is never set.
    _ = sign;
    // Neither Select nor Add have exceptions for the point at infinity /
    // selector zero, so we don't need to check for it here or in the loop.
    var table = @new<p256Table>().Compute(Ꮡq);
    table.Select(Ꮡp, sel);
    var t = NewP256Point();
    while (index >= 4) {
        index -= 5;
        Ꮡp.Double(Ꮡp);
        Ꮡp.Double(Ꮡp);
        Ꮡp.Double(Ꮡp);
        Ꮡp.Double(Ꮡp);
        Ꮡp.Double(Ꮡp);
        if (index >= 0){
            (sel, sign) = boothW5((uint64)(s.Rsh(index) & 0b111111));
        } else {
            // Booth encoding considers a virtual zero bit at index -1,
            // so we shift left the least significant limb.
            var wvalue = (uint64)(((s.Value[0] << (int)(1))) & 0b111111);
            (sel, sign) = boothW5(wvalue);
        }
        table.Select(t, sel);
        t.Negate(sign);
        Ꮡp.Add(Ꮡp, t);
    }
    return (Ꮡp, default!);
}

// Negate sets p to -p, if cond == 1, and to p if cond == 0.
public static ж<P256Point> Negate(this ж<P256Point> Ꮡp, nint cond) {
    var negY = @new<fiat.P256Element>();
    negY.Sub(negY, Ꮡp.of(P256Point.Ꮡy));
    Ꮡp.of(P256Point.Ꮡy).Select(negY, Ꮡp.of(P256Point.Ꮡy), cond);
    return Ꮡp;
}

partial struct p256AffineTable /*[32]p256AffinePoint*/;

// Select selects the n-th multiple of the table base point into p. It works in
// constant time. n can be in [0, 32], but (unlike p256Table.Select) if n is 0,
// p is set to an undefined value.
internal static void Select(this ref p256AffineTable table, ж<p256AffinePoint> Ꮡp, uint8 n) {
    if (n > 32) {
        throw panic("nistec: internal error: p256AffineTable.Select called with out-of-bounds value");
    }
    for (var i = (uint8)1; i <= 32; i++) {
        nint cond = subtle.ConstantTimeByteEq(i, n);
        Ꮡp.of(p256AffinePoint.Ꮡx).Select(Ꮡ(table.Value, (uint8)(i - 1)).of(p256AffinePoint.Ꮡx), Ꮡp.of(p256AffinePoint.Ꮡx), cond);
        Ꮡp.of(p256AffinePoint.Ꮡy).Select(Ꮡ(table.Value, (uint8)(i - 1)).of(p256AffinePoint.Ꮡy), Ꮡp.of(p256AffinePoint.Ꮡy), cond);
    }
}

// p256GeneratorTables is a series of precomputed multiples of G, the canonical
// generator. The first p256AffineTable contains multiples of G. The second one
// multiples of [2⁶]G, the third one of [2¹²]G, and so on, where each successive
// table is the previous table doubled six times. Six is the width of the
// sliding window used in ScalarBaseMult, and having each table already
// pre-doubled lets us avoid the doublings between windows entirely. This table
// aliases into p256PrecomputedEmbed.
internal static ж<ж<array<p256AffineTable>>> Ꮡp256GeneratorTables = new StandardBox<ж<array<p256AffineTable>>>(default(ж<array<p256AffineTable>>));
internal static ref ж<array<p256AffineTable>> p256GeneratorTables => ref Ꮡp256GeneratorTables.ValueSlot;

// go2cs generated this placeholder — func init is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (uint8, nint) boothW6(uint64 @in) {
    var s = ~(((@in >> (int)(6))) - 1);
    var d = (((uint64)1 << (int)(7))) - @in - 1;
    d = (uint64)(((uint64)(d & s)) | ((uint64)(@in & (~s))));
    d = ((d >> (int)(1))) + ((uint64)(d & 1));
    return ((uint8)d, (nint)((uint64)(s & 1)));
}

// ScalarBaseMult sets p = scalar * generator, where scalar is a 32-byte big
// endian value, and returns r. If scalar is not 32 bytes long, ScalarBaseMult
// returns an error and the receiver is unchanged.
public static (ж<P256Point>, error) ScalarBaseMult(this ж<P256Point> Ꮡp, slice<byte> scalar) {
    ref var p = ref Ꮡp.DerefOrNull();

    // This function works like ScalarMult above, but the table is fixed and
    // "pre-doubled" for each iteration, so instead of doubling we move to the
    // next table at each iteration.
    var (s, err) = @new<p256OrdElement>().SetBytes(scalar);
    if (err != default!) {
        return (default!, err);
    }
    // Start scanning the window from the most significant bits. We move by
    // 6 bits at a time and need to finish at -1, so -1 + 6 * 42 = 251.
    nint index = 251;
    var (sel, sign) = boothW6(s.Rsh(index));
    // sign is always zero because the boothW6 input here is at
    // most five bits long, so the top bit is never set.
    _ = sign;
    var t = Ꮡ(new p256AffinePoint(nil));
    var table = p256GeneratorTables.at<p256AffineTable>((index + 1) / 6);
    table.Select(t, sel);
    // Select's output is undefined if the selector is zero, when it should be
    // the point at infinity (because infinity can't be represented in affine
    // coordinates). Here we conditionally set p to the infinity if sel is zero.
    // In the loop, that's handled by AddAffine.
    nint selIsZero = subtle.ConstantTimeByteEq(sel, 0);
    Ꮡp.Select(NewP256Point(), t.Projective(), selIsZero);
    while (index >= 5) {
        index -= 6;
        if (index >= 0){
            (sel, sign) = boothW6((uint64)(s.Rsh(index) & 0b1111111));
        } else {
            // Booth encoding considers a virtual zero bit at index -1,
            // so we shift left the least significant limb.
            var wvalue = (uint64)(((s.Value[0] << (int)(1))) & 0b1111111);
            (sel, sign) = boothW6(wvalue);
        }
        var tableΔ1 = p256GeneratorTables.at<p256AffineTable>((index + 1) / 6);
        tableΔ1.Select(t, sel);
        t.Negate(sign);
        nint selIsZeroΔ1 = subtle.ConstantTimeByteEq(sel, 0);
        Ꮡp.AddAffine(Ꮡp, t, selIsZeroΔ1);
    }
    return (Ꮡp, default!);
}

// Negate sets p to -p, if cond == 1, and to p if cond == 0.
public static ж<p256AffinePoint> Negate(this ж<p256AffinePoint> Ꮡp, nint cond) {
    var negY = @new<fiat.P256Element>();
    negY.Sub(negY, Ꮡp.of(p256AffinePoint.Ꮡy));
    Ꮡp.of(p256AffinePoint.Ꮡy).Select(negY, Ꮡp.of(p256AffinePoint.Ꮡy), cond);
    return Ꮡp;
}

// p256Sqrt sets e to a square root of x. If x is not a square, p256Sqrt returns
// false and e is unchanged. e and x can overlap.
internal static bool /*isSquare*/ p256Sqrt(ж<fiat.P256Element> Ꮡe, ж<fiat.P256Element> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    var (t0, t1) = (@new<fiat.P256Element>(), @new<fiat.P256Element>());
    // Since p = 3 mod 4, exponentiation by (p + 1) / 4 yields a square root candidate.
    //
    // The sequence of 7 multiplications and 253 squarings is derived from the
    // following addition chain generated with github.com/mmcloughlin/addchain v0.4.0.
    //
    //	_10       = 2*1
    //	_11       = 1 + _10
    //	_1100     = _11 << 2
    //	_1111     = _11 + _1100
    //	_11110000 = _1111 << 4
    //	_11111111 = _1111 + _11110000
    //	x16       = _11111111 << 8 + _11111111
    //	x32       = x16 << 16 + x16
    //	return      ((x32 << 32 + 1) << 96 + 1) << 94
    //
    p256Square(t0, Ꮡx, 1);
    t0.Mul(Ꮡx, t0);
    p256Square(t1, t0, 2);
    t0.Mul(t0, t1);
    p256Square(t1, t0, 4);
    t0.Mul(t0, t1);
    p256Square(t1, t0, 8);
    t0.Mul(t0, t1);
    p256Square(t1, t0, 16);
    t0.Mul(t0, t1);
    p256Square(t0, t0, 32);
    t0.Mul(Ꮡx, t0);
    p256Square(t0, t0, 96);
    t0.Mul(Ꮡx, t0);
    p256Square(t0, t0, 94);
    // Check if the candidate t0 is indeed a square root of x.
    t1.Square(t0);
    if (t1.Equal(Ꮡx) != 1) {
        return false;
    }
    Ꮡe.Set(t0);
    return true;
}

// p256Square sets e to the square of x, repeated n times > 1.
internal static void p256Square(ж<fiat.P256Element> Ꮡe, ж<fiat.P256Element> Ꮡx, nint n) {
    Ꮡe.Square(Ꮡx);
    for (nint i = 1; i < n; i++) {
        Ꮡe.Square(Ꮡe);
    }
}

} // end nistec_package
