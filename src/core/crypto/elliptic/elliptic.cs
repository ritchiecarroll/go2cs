// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package elliptic implements the standard NIST P-224, P-256, P-384, and P-521
// elliptic curves over prime fields.
//
// Direct use of this package is deprecated, beyond the [P224], [P256], [P384],
// and [P521] values necessary to use [crypto/ecdsa]. Most other uses
// should migrate to the more efficient and safer [crypto/ecdh], or to
// third-party modules for lower-level functionality.
namespace go.crypto;

using io = io_package;
using big = math.big_package;
using sync = sync_package;
using math;

partial class elliptic_package {

// A Curve represents a short-form Weierstrass curve with a=-3.
//
// The behavior of Add, Double, and ScalarMult when the input is not a point on
// the curve is undefined.
//
// Note that the conventional point at infinity (0, 0) is not considered on the
// curve, although it can be returned by Add, Double, ScalarMult, or
// ScalarBaseMult (but not the [Unmarshal] or [UnmarshalCompressed] functions).
//
// Using Curve implementations besides those returned by [P224], [P256], [P384],
// and [P521] is deprecated.
partial interface Curve {
    // Params returns the parameters for the curve.
    ж<CurveParams> Params();
    // IsOnCurve reports whether the given (x,y) lies on the curve.
    //
    // Deprecated: this is a low-level unsafe API. For ECDH, use the crypto/ecdh
    // package. The NewPublicKey methods of NIST curves in crypto/ecdh accept
    // the same encoding as the Unmarshal function, and perform on-curve checks.
    bool IsOnCurve(ж<bigꓸInt> x, ж<bigꓸInt> y);
    // Add returns the sum of (x1,y1) and (x2,y2).
    //
    // Deprecated: this is a low-level unsafe API.
    (ж<bigꓸInt> x, ж<bigꓸInt> y) Add(ж<bigꓸInt> x1, ж<bigꓸInt> y1, ж<bigꓸInt> x2, ж<bigꓸInt> y2);
    // Double returns 2*(x,y).
    //
    // Deprecated: this is a low-level unsafe API.
    (ж<bigꓸInt> x, ж<bigꓸInt> y) Double(ж<bigꓸInt> x1, ж<bigꓸInt> y1);
    // ScalarMult returns k*(x,y) where k is an integer in big-endian form.
    //
    // Deprecated: this is a low-level unsafe API. For ECDH, use the crypto/ecdh
    // package. Most uses of ScalarMult can be replaced by a call to the ECDH
    // methods of NIST curves in crypto/ecdh.
    (ж<bigꓸInt> x, ж<bigꓸInt> y) ScalarMult(ж<bigꓸInt> x1, ж<bigꓸInt> y1, slice<byte> k);
    // ScalarBaseMult returns k*G, where G is the base point of the group
    // and k is an integer in big-endian form.
    //
    // Deprecated: this is a low-level unsafe API. For ECDH, use the crypto/ecdh
    // package. Most uses of ScalarBaseMult can be replaced by a call to the
    // PrivateKey.PublicKey method in crypto/ecdh.
    (ж<bigꓸInt> x, ж<bigꓸInt> y) ScalarBaseMult(slice<byte> k);
}

internal static slice<byte> mask = new byte[]{0xff, 0x1, 0x3, 0x7, 0xf, 0x1f, 0x3f, 0x7f}.slice();

// GenerateKey returns a public/private key pair. The private key is
// generated using the given reader, which must return random data.
//
// Deprecated: for ECDH, use the GenerateKey methods of the [crypto/ecdh] package;
// for ECDSA, use the GenerateKey function of the crypto/ecdsa package.
public static (slice<byte> priv, ж<bigꓸInt> x, ж<bigꓸInt> y, error err) GenerateKey(Curve curve, io.Reader rand) {
    slice<byte> priv = default!;
    ж<bigꓸInt> x = default!;
    ж<bigꓸInt> y = default!;
    error err = default!;

    var N = curve.Params().Value.N;
    nint bitSize = N.BitLen();
    nint byteLen = (bitSize + 7) / 8;
    priv = new slice<byte>(byteLen);
    while (x == nil) {
        (_, err) = io.ReadFull(rand, priv);
        if (err != default!) {
            return (priv, x, y, err);
        }
        // We have to mask off any excess bits in the case that the size of the
        // underlying field is not a whole number of bytes.
        priv[0] &= (byte)(mask[bitSize % 8]);
        // This is because, in tests, rand will return all zeros and we don't
        // want to get the point at infinity and loop forever.
        priv[1] ^= (byte)(0x42);
        // If the scalar is out of range, sample another random number.
        if (@new<bigꓸInt>().SetBytes(priv).Cmp(N) >= 0) {
            continue;
        }
        (x, y) = curve.ScalarBaseMult(priv);
    }
    return (priv, x, y, err);
}

// Marshal converts a point on the curve into the uncompressed form specified in
// SEC 1, Version 2.0, Section 2.3.3. If the point is not on the curve (or is
// the conventional point at infinity), the behavior is undefined.
//
// Deprecated: for ECDH, use the crypto/ecdh package. This function returns an
// encoding equivalent to that of PublicKey.Bytes in crypto/ecdh.
public static slice<byte> Marshal(Curve curve, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    panicIfNotOnCurve(curve, Ꮡx, Ꮡy);
    nint byteLen = ((~curve.Params()).BitSize + 7) / 8;
    var ret = new slice<byte>(1 + 2 * byteLen);
    ret[0] = 4; // uncompressed point
    x.FillBytes(ret.slice(1, 1 + byteLen));
    y.FillBytes(ret.slice(1 + byteLen, 1 + 2 * byteLen));
    return ret;
}

// MarshalCompressed converts a point on the curve into the compressed form
// specified in SEC 1, Version 2.0, Section 2.3.3. If the point is not on the
// curve (or is the conventional point at infinity), the behavior is undefined.
public static slice<byte> MarshalCompressed(Curve curve, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    panicIfNotOnCurve(curve, Ꮡx, Ꮡy);
    nint byteLen = ((~curve.Params()).BitSize + 7) / 8;
    var compressed = new slice<byte>(1 + byteLen);
    compressed[0] = (byte)((byte)y.Bit(0) | 2);
    x.FillBytes(compressed[1..]);
    return compressed;
}

// unmarshaler is implemented by curves with their own constant-time Unmarshal.
//
// There isn't an equivalent interface for Marshal/MarshalCompressed because
// that doesn't involve any mathematical operations, only FillBytes and Bit.
partial interface unmarshaler {
    (ж<bigꓸInt> x, ж<bigꓸInt> y) Unmarshal(slice<byte> _);
    (ж<bigꓸInt> x, ж<bigꓸInt> y) UnmarshalCompressed(slice<byte> _);
}

// Assert that the known curves implement unmarshaler.
internal static slice<unmarshaler> _ᴛ1ʗ = new unmarshaler[]{new nistCurveжunmarshaler<P224PointжnistPoint>(p224), new p256Curveжunmarshaler(p256), new nistCurveжunmarshaler<P384PointжnistPoint>(p384), new nistCurveжunmarshaler<P521PointжnistPoint>(p521)}.slice();

// Unmarshal converts a point, serialized by [Marshal], into an x, y pair. It is
// an error if the point is not in uncompressed form, is not on the curve, or is
// the point at infinity. On error, x = nil.
//
// Deprecated: for ECDH, use the crypto/ecdh package. This function accepts an
// encoding equivalent to that of the NewPublicKey methods in crypto/ecdh.
public static (ж<bigꓸInt> x, ж<bigꓸInt> y) Unmarshal(Curve curve, slice<byte> data) {
    ж<bigꓸInt> x = default!;
    ж<bigꓸInt> y = default!;

    {
        var (c, ok) = curve._<unmarshaler>(ᐧ); if (ok) {
            return c.Unmarshal(data);
        }
    }
    nint byteLen = ((~curve.Params()).BitSize + 7) / 8;
    if (len(data) != 1 + 2 * byteLen) {
        return (default!, default!);
    }
    if (data[0] != 4) {
        // uncompressed form
        return (default!, default!);
    }
    var p = curve.Params().Value.P;
    x = @new<bigꓸInt>().SetBytes(data.slice(1, 1 + byteLen));
    y = @new<bigꓸInt>().SetBytes(data.slice(1 + byteLen));
    if (x.Cmp(p) >= 0 || y.Cmp(p) >= 0) {
        return (default!, default!);
    }
    if (!curve.IsOnCurve(x, y)) {
        return (default!, default!);
    }
    return (x, y);
}

// UnmarshalCompressed converts a point, serialized by [MarshalCompressed], into
// an x, y pair. It is an error if the point is not in compressed form, is not
// on the curve, or is the point at infinity. On error, x = nil.
public static (ж<bigꓸInt> x, ж<bigꓸInt> y) UnmarshalCompressed(Curve curve, slice<byte> data) {
    ж<bigꓸInt> x = default!;
    ж<bigꓸInt> y = default!;

    {
        var (c, ok) = curve._<unmarshaler>(ᐧ); if (ok) {
            return c.UnmarshalCompressed(data);
        }
    }
    nint byteLen = ((~curve.Params()).BitSize + 7) / 8;
    if (len(data) != 1 + byteLen) {
        return (default!, default!);
    }
    if (data[0] != 2 && data[0] != 3) {
        // compressed form
        return (default!, default!);
    }
    var p = curve.Params().Value.P;
    x = @new<bigꓸInt>().SetBytes(data[1..]);
    if (x.Cmp(p) >= 0) {
        return (default!, default!);
    }
    // y² = x³ - 3x + b
    y = curve.Params().polynomial(x);
    y = y.ModSqrt(y, p);
    if (y == nil) {
        return (default!, default!);
    }
    if ((byte)y.Bit(0) != (byte)(data[0] & 1)) {
        y.Neg(y).Mod(y, p);
    }
    if (!curve.IsOnCurve(x, y)) {
        return (default!, default!);
    }
    return (x, y);
}

internal static void panicIfNotOnCurve(Curve curve, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    // (0, 0) is the point at infinity by convention. It's ok to operate on it,
    // although IsOnCurve is documented to return false for it. See Issue 37294.
    if (x.Sign() == 0 && y.Sign() == 0) {
        return;
    }
    if (!curve.IsOnCurve(Ꮡx, Ꮡy)) {
        throw panic("crypto/elliptic: attempted operation on invalid point");
    }
}

internal static ж<sync.Once> Ꮡinitonce = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once initonce => ref Ꮡinitonce.Value;

internal static void initAll() {
    initP224();
    initP256();
    initP384();
    initP521();
}

// P224 returns a [Curve] which implements NIST P-224 (FIPS 186-3, section D.2.2),
// also known as secp224r1. The CurveParams.Name of this [Curve] is "P-224".
//
// Multiple invocations of this function will return the same value, so it can
// be used for equality checks and switch statements.
//
// The cryptographic operations are implemented using constant-time algorithms.
public static Curve P224() {
    Ꮡinitonce.Do(initAll);
    return new nistCurveжCurve<P224PointжnistPoint>(p224);
}

// P256 returns a [Curve] which implements NIST P-256 (FIPS 186-3, section D.2.3),
// also known as secp256r1 or prime256v1. The CurveParams.Name of this [Curve] is
// "P-256".
//
// Multiple invocations of this function will return the same value, so it can
// be used for equality checks and switch statements.
//
// The cryptographic operations are implemented using constant-time algorithms.
public static Curve P256() {
    Ꮡinitonce.Do(initAll);
    return new p256CurveжCurve(p256);
}

// P384 returns a [Curve] which implements NIST P-384 (FIPS 186-3, section D.2.4),
// also known as secp384r1. The CurveParams.Name of this [Curve] is "P-384".
//
// Multiple invocations of this function will return the same value, so it can
// be used for equality checks and switch statements.
//
// The cryptographic operations are implemented using constant-time algorithms.
public static Curve P384() {
    Ꮡinitonce.Do(initAll);
    return new nistCurveжCurve<P384PointжnistPoint>(p384);
}

// P521 returns a [Curve] which implements NIST P-521 (FIPS 186-3, section D.2.5),
// also known as secp521r1. The CurveParams.Name of this [Curve] is "P-521".
//
// Multiple invocations of this function will return the same value, so it can
// be used for equality checks and switch statements.
//
// The cryptographic operations are implemented using constant-time algorithms.
public static Curve P521() {
    Ꮡinitonce.Do(initAll);
    return new nistCurveжCurve<P521PointжnistPoint>(p521);
}

} // end elliptic_package
