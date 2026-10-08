// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using big = math.big_package;
using math;

partial class elliptic_package {

// CurveParams contains the parameters of an elliptic curve and also provides
// a generic, non-constant time implementation of [Curve].
//
// The generic Curve implementation is deprecated, and using custom curves
// (those not returned by [P224], [P256], [P384], and [P521]) is not guaranteed
// to provide any security property.
partial struct CurveParams {
    public ж<bigꓸInt> P; // the order of the underlying field
    public ж<bigꓸInt> N; // the order of the base point
    public ж<bigꓸInt> B; // the constant of the curve equation
    public ж<bigꓸInt> Gx, Gy; // (x,y) of the base point
    public nint BitSize;     // the size of the underlying field
    public @string Name;  // the canonical name of the curve
}

public static ж<CurveParams> Params(this ж<CurveParams> Ꮡcurve) {
    return Ꮡcurve;
}

// CurveParams operates, internally, on Jacobian coordinates. For a given
// (x, y) position on the curve, the Jacobian coordinates are (x1, y1, z1)
// where x = x1/z1² and y = y1/z1³. The greatest speedups come when the whole
// calculation can be performed within the transform (as in ScalarMult and
// ScalarBaseMult). But even for Add and Double, it's faster to apply and
// reverse the transform than to operate in affine coordinates.

// polynomial returns x³ - 3x + b.
internal static ж<bigꓸInt> polynomial(this ref CurveParams curve, ж<bigꓸInt> Ꮡx) {
    var x3 = @new<bigꓸInt>().Mul(Ꮡx, Ꮡx);
    x3.Mul(x3, Ꮡx);
    var threeX = @new<bigꓸInt>().Lsh(Ꮡx, 1);
    threeX.Add(threeX, Ꮡx);
    x3.Sub(x3, threeX);
    x3.Add(x3, curve.B);
    x3.Mod(x3, curve.P);
    return x3;
}

// IsOnCurve implements [Curve.IsOnCurve].
//
// Deprecated: the [CurveParams] methods are deprecated and are not guaranteed to
// provide any security property. For ECDH, use the [crypto/ecdh] package.
// For ECDSA, use the [crypto/ecdsa] package with a [Curve] value returned directly
// from [P224], [P256], [P384], or [P521].
public static bool IsOnCurve(this ж<CurveParams> Ꮡcurve, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy) {
    ref var curve = ref Ꮡcurve.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    {
        var (specific, ok) = matchesSpecificCurve(Ꮡcurve); if (ok) {
            return specific.IsOnCurve(Ꮡx, Ꮡy);
        }
    }
    if (x.Sign() < 0 || Ꮡx.Cmp(curve.P) >= 0 || y.Sign() < 0 || Ꮡy.Cmp(curve.P) >= 0) {
        return false;
    }
    // y² = x³ - 3x + b
    var y2 = @new<bigꓸInt>().Mul(Ꮡy, Ꮡy);
    y2.Mod(y2, curve.P);
    return curve.polynomial(Ꮡx).Cmp(y2) == 0;
}

// zForAffine returns a Jacobian Z value for the affine point (x, y). If x and
// y are zero, it assumes that they represent the point at infinity because (0,
// 0) is not on the any of the curves handled here.
internal static ж<bigꓸInt> zForAffine(ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    var z = @new<bigꓸInt>();
    if (x.Sign() != 0 || y.Sign() != 0) {
        z.SetInt64(1);
    }
    return z;
}

// affineFromJacobian reverses the Jacobian transform. See the comment at the
// top of the file. If the point is ∞ it returns 0, 0.
internal static (ж<bigꓸInt> xOut, ж<bigꓸInt> yOut) affineFromJacobian(this ref CurveParams curve, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy, ж<bigꓸInt> Ꮡz) {
    ж<bigꓸInt> xOut = default!;
    ж<bigꓸInt> yOut = default!;

    ref var z = ref Ꮡz.DerefOrNull();
    if (z.Sign() == 0) {
        return (@new<bigꓸInt>(), @new<bigꓸInt>());
    }
    var zinv = @new<bigꓸInt>().ModInverse(Ꮡz, curve.P);
    var zinvsq = @new<bigꓸInt>().Mul(zinv, zinv);
    xOut = @new<bigꓸInt>().Mul(Ꮡx, zinvsq);
    xOut.Mod(xOut, curve.P);
    zinvsq.Mul(zinvsq, zinv);
    yOut = @new<bigꓸInt>().Mul(Ꮡy, zinvsq);
    yOut.Mod(yOut, curve.P);
    return (xOut, yOut);
}

// Add implements [Curve.Add].
//
// Deprecated: the [CurveParams] methods are deprecated and are not guaranteed to
// provide any security property. For ECDH, use the [crypto/ecdh] package.
// For ECDSA, use the [crypto/ecdsa] package with a [Curve] value returned directly
// from [P224], [P256], [P384], or [P521].
public static (ж<bigꓸInt>, ж<bigꓸInt>) Add(this ж<CurveParams> Ꮡcurve, ж<bigꓸInt> Ꮡx1, ж<bigꓸInt> Ꮡy1, ж<bigꓸInt> Ꮡx2, ж<bigꓸInt> Ꮡy2) {
    ref var curve = ref Ꮡcurve.DerefOrNull();

    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    {
        var (specific, ok) = matchesSpecificCurve(Ꮡcurve); if (ok) {
            return specific.Add(Ꮡx1, Ꮡy1, Ꮡx2, Ꮡy2);
        }
    }
    panicIfNotOnCurve(new CurveParamsжCurve(Ꮡcurve), Ꮡx1, Ꮡy1);
    panicIfNotOnCurve(new CurveParamsжCurve(Ꮡcurve), Ꮡx2, Ꮡy2);
    var z1 = zForAffine(Ꮡx1, Ꮡy1);
    var z2 = zForAffine(Ꮡx2, Ꮡy2);
    var (ᴛ1, ᴛ2, ᴛ3) = curve.addJacobian(Ꮡx1, Ꮡy1, z1, Ꮡx2, Ꮡy2, z2);
    return curve.affineFromJacobian(ᴛ1, ᴛ2, ᴛ3);
}

// addJacobian takes two points in Jacobian coordinates, (x1, y1, z1) and
// (x2, y2, z2) and returns their sum, also in Jacobian form.
internal static (ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>) addJacobian(this ref CurveParams curve, ж<bigꓸInt> Ꮡx1, ж<bigꓸInt> Ꮡy1, ж<bigꓸInt> Ꮡz1, ж<bigꓸInt> Ꮡx2, ж<bigꓸInt> Ꮡy2, ж<bigꓸInt> Ꮡz2) {
    ref var z1 = ref Ꮡz1.DerefOrNull();
    ref var z2 = ref Ꮡz2.DerefOrNull();

    // See https://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#addition-add-2007-bl
    var (x3, y3, z3) = (@new<bigꓸInt>(), @new<bigꓸInt>(), @new<bigꓸInt>());
    if (z1.Sign() == 0) {
        x3.Set(Ꮡx2);
        y3.Set(Ꮡy2);
        z3.Set(Ꮡz2);
        return (x3, y3, z3);
    }
    if (z2.Sign() == 0) {
        x3.Set(Ꮡx1);
        y3.Set(Ꮡy1);
        z3.Set(Ꮡz1);
        return (x3, y3, z3);
    }
    var z1z1 = @new<bigꓸInt>().Mul(Ꮡz1, Ꮡz1);
    z1z1.Mod(z1z1, curve.P);
    var z2z2 = @new<bigꓸInt>().Mul(Ꮡz2, Ꮡz2);
    z2z2.Mod(z2z2, curve.P);
    var u1 = @new<bigꓸInt>().Mul(Ꮡx1, z2z2);
    u1.Mod(u1, curve.P);
    var u2 = @new<bigꓸInt>().Mul(Ꮡx2, z1z1);
    u2.Mod(u2, curve.P);
    var h = @new<bigꓸInt>().Sub(u2, u1);
    var xEqual = h.Sign() == 0;
    if (h.Sign() == -1) {
        h.Add(h, curve.P);
    }
    var i = @new<bigꓸInt>().Lsh(h, 1);
    i.Mul(i, i);
    var j = @new<bigꓸInt>().Mul(h, i);
    var s1 = @new<bigꓸInt>().Mul(Ꮡy1, Ꮡz2);
    s1.Mul(s1, z2z2);
    s1.Mod(s1, curve.P);
    var s2 = @new<bigꓸInt>().Mul(Ꮡy2, Ꮡz1);
    s2.Mul(s2, z1z1);
    s2.Mod(s2, curve.P);
    var r = @new<bigꓸInt>().Sub(s2, s1);
    if (r.Sign() == -1) {
        r.Add(r, curve.P);
    }
    var yEqual = r.Sign() == 0;
    if (xEqual && yEqual) {
        return curve.doubleJacobian(Ꮡx1, Ꮡy1, Ꮡz1);
    }
    r.Lsh(r, 1);
    var v = @new<bigꓸInt>().Mul(u1, i);
    x3.Set(r);
    x3.Mul(x3, x3);
    x3.Sub(x3, j);
    x3.Sub(x3, v);
    x3.Sub(x3, v);
    x3.Mod(x3, curve.P);
    y3.Set(r);
    v.Sub(v, x3);
    y3.Mul(y3, v);
    s1.Mul(s1, j);
    s1.Lsh(s1, 1);
    y3.Sub(y3, s1);
    y3.Mod(y3, curve.P);
    z3.Add(Ꮡz1, Ꮡz2);
    z3.Mul(z3, z3);
    z3.Sub(z3, z1z1);
    z3.Sub(z3, z2z2);
    z3.Mul(z3, h);
    z3.Mod(z3, curve.P);
    return (x3, y3, z3);
}

// Double implements [Curve.Double].
//
// Deprecated: the [CurveParams] methods are deprecated and are not guaranteed to
// provide any security property. For ECDH, use the [crypto/ecdh] package.
// For ECDSA, use the [crypto/ecdsa] package with a [Curve] value returned directly
// from [P224], [P256], [P384], or [P521].
public static (ж<bigꓸInt>, ж<bigꓸInt>) Double(this ж<CurveParams> Ꮡcurve, ж<bigꓸInt> Ꮡx1, ж<bigꓸInt> Ꮡy1) {
    ref var curve = ref Ꮡcurve.DerefOrNull();

    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    {
        var (specific, ok) = matchesSpecificCurve(Ꮡcurve); if (ok) {
            return specific.Double(Ꮡx1, Ꮡy1);
        }
    }
    panicIfNotOnCurve(new CurveParamsжCurve(Ꮡcurve), Ꮡx1, Ꮡy1);
    var z1 = zForAffine(Ꮡx1, Ꮡy1);
    var (ᴛ4, ᴛ5, ᴛ6) = curve.doubleJacobian(Ꮡx1, Ꮡy1, z1);
    return curve.affineFromJacobian(ᴛ4, ᴛ5, ᴛ6);
}

// doubleJacobian takes a point in Jacobian coordinates, (x, y, z), and
// returns its double, also in Jacobian form.
internal static (ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>) doubleJacobian(this ref CurveParams curve, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy, ж<bigꓸInt> Ꮡz) {
    // See https://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#doubling-dbl-2001-b
    var delta = @new<bigꓸInt>().Mul(Ꮡz, Ꮡz);
    delta.Mod(delta, curve.P);
    var gamma = @new<bigꓸInt>().Mul(Ꮡy, Ꮡy);
    gamma.Mod(gamma, curve.P);
    var alpha = @new<bigꓸInt>().Sub(Ꮡx, delta);
    if (alpha.Sign() == -1) {
        alpha.Add(alpha, curve.P);
    }
    var alpha2 = @new<bigꓸInt>().Add(Ꮡx, delta);
    alpha.Mul(alpha, alpha2);
    alpha2.Set(alpha);
    alpha.Lsh(alpha, 1);
    alpha.Add(alpha, alpha2);
    var beta = alpha2.Mul(Ꮡx, gamma);
    var x3 = @new<bigꓸInt>().Mul(alpha, alpha);
    var beta8 = @new<bigꓸInt>().Lsh(beta, 3);
    beta8.Mod(beta8, curve.P);
    x3.Sub(x3, beta8);
    if (x3.Sign() == -1) {
        x3.Add(x3, curve.P);
    }
    x3.Mod(x3, curve.P);
    var z3 = @new<bigꓸInt>().Add(Ꮡy, Ꮡz);
    z3.Mul(z3, z3);
    z3.Sub(z3, gamma);
    if (z3.Sign() == -1) {
        z3.Add(z3, curve.P);
    }
    z3.Sub(z3, delta);
    if (z3.Sign() == -1) {
        z3.Add(z3, curve.P);
    }
    z3.Mod(z3, curve.P);
    beta.Lsh(beta, 2);
    beta.Sub(beta, x3);
    if (beta.Sign() == -1) {
        beta.Add(beta, curve.P);
    }
    var y3 = alpha.Mul(alpha, beta);
    gamma.Mul(gamma, gamma);
    gamma.Lsh(gamma, 3);
    gamma.Mod(gamma, curve.P);
    y3.Sub(y3, gamma);
    if (y3.Sign() == -1) {
        y3.Add(y3, curve.P);
    }
    y3.Mod(y3, curve.P);
    return (x3, y3, z3);
}

// ScalarMult implements [Curve.ScalarMult].
//
// Deprecated: the [CurveParams] methods are deprecated and are not guaranteed to
// provide any security property. For ECDH, use the [crypto/ecdh] package.
// For ECDSA, use the [crypto/ecdsa] package with a [Curve] value returned directly
// from [P224], [P256], [P384], or [P521].
public static (ж<bigꓸInt>, ж<bigꓸInt>) ScalarMult(this ж<CurveParams> Ꮡcurve, ж<bigꓸInt> ᏑBx, ж<bigꓸInt> ᏑBy, slice<byte> k) {
    ref var curve = ref Ꮡcurve.DerefOrNull();

    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    {
        var (specific, ok) = matchesSpecificCurve(Ꮡcurve); if (ok) {
            return specific.ScalarMult(ᏑBx, ᏑBy, k);
        }
    }
    panicIfNotOnCurve(new CurveParamsжCurve(Ꮡcurve), ᏑBx, ᏑBy);
    var Bz = @new<bigꓸInt>().SetInt64(1);
    var (x, y, z) = (@new<bigꓸInt>(), @new<bigꓸInt>(), @new<bigꓸInt>());
    foreach (var (_, vᴛ1) in k) {
        var @byte = vᴛ1;

        for (nint bitNum = 0; bitNum < 8; bitNum++) {
            (x, y, z) = curve.doubleJacobian(x, y, z);
            if ((byte)(@byte & 0x80) == 0x80) {
                (x, y, z) = curve.addJacobian(ᏑBx, ᏑBy, Bz, x, y, z);
            }
            @byte <<= (int)(1);
        }
    }
    return curve.affineFromJacobian(x, y, z);
}

// ScalarBaseMult implements [Curve.ScalarBaseMult].
//
// Deprecated: the [CurveParams] methods are deprecated and are not guaranteed to
// provide any security property. For ECDH, use the [crypto/ecdh] package.
// For ECDSA, use the [crypto/ecdsa] package with a [Curve] value returned directly
// from [P224], [P256], [P384], or [P521].
public static (ж<bigꓸInt>, ж<bigꓸInt>) ScalarBaseMult(this ж<CurveParams> Ꮡcurve, slice<byte> k) {
    ref var curve = ref Ꮡcurve.DerefOrNull();

    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    {
        var (specific, ok) = matchesSpecificCurve(Ꮡcurve); if (ok) {
            return specific.ScalarBaseMult(k);
        }
    }
    return Ꮡcurve.ScalarMult(curve.Gx, curve.Gy, k);
}

internal static (Curve, bool) matchesSpecificCurve(ж<CurveParams> Ꮡparams) {
    foreach (var (_, c) in new Curve[]{new nistCurveжCurve<P224PointжnistPoint>(p224), new p256CurveжCurve(p256), new nistCurveжCurve<P384PointжnistPoint>(p384), new nistCurveжCurve<P521PointжnistPoint>(p521)}.slice()) {
        if (Ꮡparams == c.Params()) {
            return (c, true);
        }
    }
    return (default!, false);
}

} // end elliptic_package
