// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using big = math.big_package;
using math;
using static elliptic_package;

partial class elliptic_package {

// CurveParams contains the parameters of an elliptic curve and also provides
// a generic, non-constant time implementation of [Curve].
//
// The generic Curve implementation is deprecated, and using custom curves
// (those not returned by [P224], [P256], [P384], and [P521]) is not guaranteed
// to provide any security property.
partial struct CurveParams {
    public ж<big.Int> P;      // the order of the underlying field
    public ж<big.Int> N;      // the order of the base point
    public ж<big.Int> B;      // the constant of the curve equation
    public ж<big.Int> Gx, Gy; // (x,y) of the base point
    public nint BitSize;      // the size of the underlying field
    public @string Name;      // the canonical name of the curve
}

} partial class ellipticᴛCurveParams {

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
internal static ж<big.Int> polynomial(this ref CurveParams curve, ж<big.Int> x) {
    var x3 = @new<big.Int>().Mul(x, x);
    x3.Mul(x3, x);

    var threeX = @new<big.Int>().Lsh(x, 1);
    threeX.Add(threeX, x);

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
public static bool IsOnCurve(this ж<CurveParams> Ꮡcurve, ж<big.Int> Ꮡx, ж<big.Int> Ꮡy) {
    ref var curve = ref Ꮡcurve.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    var (specific, ok) = matchesSpecificCurve(Ꮡcurve); if (ok) {
        return specific.IsOnCurve(Ꮡx, Ꮡy);
    }

    if (x.Sign() < 0 || Ꮡx.Cmp(curve.P) >= 0 || y.Sign() < 0 || Ꮡy.Cmp(curve.P) >= 0) {
        return false;
    }

    // y² = x³ - 3x + b
    var y2 = @new<big.Int>().Mul(Ꮡy, Ꮡy);
    y2.Mod(y2, curve.P);

    return curve.polynomial(Ꮡx).Cmp(y2) == 0;
}

} partial class elliptic_package {

// zForAffine returns a Jacobian Z value for the affine point (x, y). If x and
// y are zero, it assumes that they represent the point at infinity because (0,
// 0) is not on the any of the curves handled here.
internal static ж<big.Int> zForAffine(ж<big.Int> Ꮡx, ж<big.Int> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    var z = @new<big.Int>();
    if (x.Sign() != 0 || y.Sign() != 0) {
        z.SetInt64(1);
    }
    return z;
}

} partial class ellipticᴛCurveParams {

// affineFromJacobian reverses the Jacobian transform. See the comment at the
// top of the file. If the point is ∞ it returns 0, 0.
internal static (ж<big.Int> xOut, ж<big.Int> yOut) affineFromJacobian(this ref CurveParams curve, ж<big.Int> x, ж<big.Int> y, ж<big.Int> Ꮡz) {
    ж<big.Int> xOut = default;
    ж<big.Int> yOut = default;
    ref var z = ref Ꮡz.DerefOrNull();

    if (z.Sign() == 0) {
        return (@new<big.Int>(), @new<big.Int>());
    }

    var zinv = @new<big.Int>().ModInverse(Ꮡz, curve.P);
    var zinvsq = @new<big.Int>().Mul(zinv, zinv);

    xOut = @new<big.Int>().Mul(x, zinvsq);
    xOut.Mod(xOut, curve.P);
    zinvsq.Mul(zinvsq, zinv);
    yOut = @new<big.Int>().Mul(y, zinvsq);
    yOut.Mod(yOut, curve.P);
    return (xOut, yOut);
}

// Add implements [Curve.Add].
//
// Deprecated: the [CurveParams] methods are deprecated and are not guaranteed to
// provide any security property. For ECDH, use the [crypto/ecdh] package.
// For ECDSA, use the [crypto/ecdsa] package with a [Curve] value returned directly
// from [P224], [P256], [P384], or [P521].
public static (ж<big.Int>, ж<big.Int>) Add(this ж<CurveParams> Ꮡcurve, ж<big.Int> x1, ж<big.Int> y1, ж<big.Int> x2, ж<big.Int> y2) {
    ref var curve = ref Ꮡcurve.DerefOrNull();

    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    var (specific, ok) = matchesSpecificCurve(Ꮡcurve); if (ok) {
        return specific.Add(x1, y1, x2, y2);
    }
    panicIfNotOnCurve(new CurveParamsжCurve(Ꮡcurve), x1, y1);
    panicIfNotOnCurve(new CurveParamsжCurve(Ꮡcurve), x2, y2);

    var z1 = zForAffine(x1, y1);
    var z2 = zForAffine(x2, y2);
    var (ᴛ1, ᴛ2, ᴛ3) = curve.addJacobian(x1, y1, z1, x2, y2, z2);
    return curve.affineFromJacobian(ᴛ1, ᴛ2, ᴛ3);
}

// addJacobian takes two points in Jacobian coordinates, (x1, y1, z1) and
// (x2, y2, z2) and returns their sum, also in Jacobian form.
internal static (ж<big.Int>, ж<big.Int>, ж<big.Int>) addJacobian(this ref CurveParams curve, ж<big.Int> x1, ж<big.Int> y1, ж<big.Int> Ꮡz1, ж<big.Int> x2, ж<big.Int> y2, ж<big.Int> Ꮡz2) {
    ref var z1 = ref Ꮡz1.DerefOrNull();
    ref var z2 = ref Ꮡz2.DerefOrNull();

    // See https://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#addition-add-2007-bl
    var (x3, y3, z3) = (@new<big.Int>(), @new<big.Int>(), @new<big.Int>());
    if (z1.Sign() == 0) {
        x3.Set(x2);
        y3.Set(y2);
        z3.Set(Ꮡz2);
        return (x3, y3, z3);
    }
    if (z2.Sign() == 0) {
        x3.Set(x1);
        y3.Set(y1);
        z3.Set(Ꮡz1);
        return (x3, y3, z3);
    }

    var z1z1 = @new<big.Int>().Mul(Ꮡz1, Ꮡz1);
    z1z1.Mod(z1z1, curve.P);
    var z2z2 = @new<big.Int>().Mul(Ꮡz2, Ꮡz2);
    z2z2.Mod(z2z2, curve.P);

    var u1 = @new<big.Int>().Mul(x1, z2z2);
    u1.Mod(u1, curve.P);
    var u2 = @new<big.Int>().Mul(x2, z1z1);
    u2.Mod(u2, curve.P);
    var h = @new<big.Int>().Sub(u2, u1);
    var xEqual = h.Sign() == 0;
    if (h.Sign() == -1) {
        h.Add(h, curve.P);
    }
    var i = @new<big.Int>().Lsh(h, 1);
    i.Mul(i, i);
    var j = @new<big.Int>().Mul(h, i);

    var s1 = @new<big.Int>().Mul(y1, Ꮡz2);
    s1.Mul(s1, z2z2);
    s1.Mod(s1, curve.P);
    var s2 = @new<big.Int>().Mul(y2, Ꮡz1);
    s2.Mul(s2, z1z1);
    s2.Mod(s2, curve.P);
    var r = @new<big.Int>().Sub(s2, s1);
    if (r.Sign() == -1) {
        r.Add(r, curve.P);
    }
    var yEqual = r.Sign() == 0;
    if (xEqual && yEqual) {
        return curve.doubleJacobian(x1, y1, Ꮡz1);
    }
    r.Lsh(r, 1);
    var v = @new<big.Int>().Mul(u1, i);

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
public static (ж<big.Int>, ж<big.Int>) Double(this ж<CurveParams> Ꮡcurve, ж<big.Int> x1, ж<big.Int> y1) {
    ref var curve = ref Ꮡcurve.DerefOrNull();

    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    var (specific, ok) = matchesSpecificCurve(Ꮡcurve); if (ok) {
        return specific.Double(x1, y1);
    }
    panicIfNotOnCurve(new CurveParamsжCurve(Ꮡcurve), x1, y1);

    var z1 = zForAffine(x1, y1);
    var (ᴛ4, ᴛ5, ᴛ6) = curve.doubleJacobian(x1, y1, z1);
    return curve.affineFromJacobian(ᴛ4, ᴛ5, ᴛ6);
}

// doubleJacobian takes a point in Jacobian coordinates, (x, y, z), and
// returns its double, also in Jacobian form.
internal static (ж<big.Int>, ж<big.Int>, ж<big.Int>) doubleJacobian(this ref CurveParams curve, ж<big.Int> x, ж<big.Int> y, ж<big.Int> z) {
    // See https://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#doubling-dbl-2001-b
    var delta = @new<big.Int>().Mul(z, z);
    delta.Mod(delta, curve.P);
    var gamma = @new<big.Int>().Mul(y, y);
    gamma.Mod(gamma, curve.P);
    var alpha = @new<big.Int>().Sub(x, delta);
    if (alpha.Sign() == -1) {
        alpha.Add(alpha, curve.P);
    }
    var alpha2 = @new<big.Int>().Add(x, delta);
    alpha.Mul(alpha, alpha2);
    alpha2.Set(alpha);
    alpha.Lsh(alpha, 1);
    alpha.Add(alpha, alpha2);

    var beta = alpha2.Mul(x, gamma);

    var x3 = @new<big.Int>().Mul(alpha, alpha);
    var beta8 = @new<big.Int>().Lsh(beta, 3);
    beta8.Mod(beta8, curve.P);
    x3.Sub(x3, beta8);
    if (x3.Sign() == -1) {
        x3.Add(x3, curve.P);
    }
    x3.Mod(x3, curve.P);

    var z3 = @new<big.Int>().Add(y, z);
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
public static (ж<big.Int>, ж<big.Int>) ScalarMult(this ж<CurveParams> Ꮡcurve, ж<big.Int> Bx, ж<big.Int> By, slice<byte> k) {
    ref var curve = ref Ꮡcurve.DerefOrNull();

    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    var (specific, ok) = matchesSpecificCurve(Ꮡcurve); if (ok) {
        return specific.ScalarMult(Bx, By, k);
    }
    panicIfNotOnCurve(new CurveParamsжCurve(Ꮡcurve), Bx, By);

    var Bz = @new<big.Int>().SetInt64(1);
    var (x, y, z) = (@new<big.Int>(), @new<big.Int>(), @new<big.Int>());

    foreach (var (_, vᴛ1) in range(k)) {
        var b = vᴛ1;

        foreach (var _ᴛ1 in range(8)) {
            (x, y, z) = curve.doubleJacobian(x, y, z);
            if ((byte)(b & 0x80) == 0x80) {
                (x, y, z) = curve.addJacobian(Bx, By, Bz, x, y, z);
            }
            b <<= 1;
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
public static (ж<big.Int>, ж<big.Int>) ScalarBaseMult(this ж<CurveParams> Ꮡcurve, slice<byte> k) {
    ref var curve = ref Ꮡcurve.DerefOrNull();

    // If there is a dedicated constant-time implementation for this curve operation,
    // use that instead of the generic one.
    var (specific, ok) = matchesSpecificCurve(Ꮡcurve); if (ok) {
        return specific.ScalarBaseMult(k);
    }

    return Ꮡcurve.ScalarMult(curve.Gx, curve.Gy, k);
}

} partial class elliptic_package {

internal static (Curve, bool) matchesSpecificCurve(ж<CurveParams> @params) {
    foreach (var (_, c) in range(slice<Curve>([new nistCurveжCurve<P224PointжnistPoint>(p224), new nistCurveжCurve<P256PointжnistPoint>(p256), new nistCurveжCurve<P384PointжnistPoint>(p384), new nistCurveжCurve<P521PointжnistPoint>(p521)]))) {
        if (@params == c.Params()) {
            return (c, true);
        }
    }
    return (default, false);
}

} // end elliptic_package
