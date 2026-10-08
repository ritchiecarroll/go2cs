// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using field = go.crypto.@internal.fips140.edwards25519.field_package;
using fips140only = go.crypto.@internal.fips140only_package;
using randutil = go.crypto.@internal.randutil_package;
using errors = errors_package;
using io = io_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140.edwards25519;

partial class ecdh_package {

internal static nint x25519PublicKeySize = 32;
internal static nint x25519PrivateKeySize = 32;
internal static nint x25519SharedSecretSize = 32;

// X25519 returns a [Curve] which implements the X25519 function over Curve25519
// (RFC 7748, Section 5).
//
// Multiple invocations of this function will return the same value, so it can
// be used for equality checks and switch statements.
public static ΔCurve X25519() {
    return new x25519CurveжΔCurve(x25519);
}

internal static ж<x25519Curve> x25519 = Ꮡ(new x25519Curve(nil));

partial struct x25519Curve {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string x25519ˢ = "X25519"u8;

internal static @string String(this ref x25519Curve c) {
    return x25519ˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoEcdhUseOfX25519Isˢ = "crypto/ecdh: use of X25519 is not allowed in FIPS 140-only mode"u8;

internal static (ж<PrivateKey>, error) GenerateKey(this ж<x25519Curve> Ꮡc, io.Reader rand) {
    if (fips140only.Enabled) {
        return (default!, errors.New(cryptoEcdhUseOfX25519Isˢ));
    }
    var key = new slice<byte>(x25519PrivateKeySize);
    randutil.MaybeReadByte(rand);
    {
        var (_, err) = io.ReadFull(rand, key); if (err != default!) {
            return (default!, err);
        }
    }
    return Ꮡc.NewPrivateKey(key);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoEcdhInvalidPrivateˢ2 = "crypto/ecdh: invalid private key size"u8;

internal static (ж<PrivateKey>, error) NewPrivateKey(this ж<x25519Curve> Ꮡc, slice<byte> key) {
    if (fips140only.Enabled) {
        return (default!, errors.New(cryptoEcdhUseOfX25519Isˢ));
    }
    if (len(key) != x25519PrivateKeySize) {
        return (default!, errors.New(cryptoEcdhInvalidPrivateˢ2));
    }
    var publicKey = new slice<byte>(x25519PublicKeySize);
    var x25519Basepoint = new byte[]{9}.array(32);
    x25519ScalarMult(publicKey, key, x25519Basepoint[..]);
    // We don't check for the all-zero public key here because the scalar is
    // never zero because of clamping, and the basepoint is not the identity in
    // the prime-order subgroup(s).
    return (Ꮡ(new PrivateKey(
        curve: new x25519CurveжΔCurve(Ꮡc),
        privateKey: bytes.Clone(key),
        publicKey: Ꮡ(new ΔPublicKey(curve: new x25519CurveжΔCurve(Ꮡc), publicKey: publicKey))
    )), default!);
}

internal static (ж<ΔPublicKey>, error) NewPublicKey(this ж<x25519Curve> Ꮡc, slice<byte> key) {
    if (fips140only.Enabled) {
        return (default!, errors.New(cryptoEcdhUseOfX25519Isˢ));
    }
    if (len(key) != x25519PublicKeySize) {
        return (default!, errors.New(cryptoEcdhInvalidPublicˢ));
    }
    return (Ꮡ(new ΔPublicKey(
        curve: new x25519CurveжΔCurve(Ꮡc),
        publicKey: bytes.Clone(key)
    )), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoEcdhBadX25519ˢ = "crypto/ecdh: bad X25519 remote ECDH input: low order point"u8;

internal static (slice<byte>, error) ecdh(this ref x25519Curve c, ж<PrivateKey> Ꮡlocal, ж<ΔPublicKey> Ꮡremote) {
    ref var local = ref Ꮡlocal.DerefOrNull();
    ref var remote = ref Ꮡremote.DerefOrNull();

    var @out = new slice<byte>(x25519SharedSecretSize);
    x25519ScalarMult(@out, local.privateKey, remote.publicKey);
    if (isZero(@out)) {
        return (default!, errors.New(cryptoEcdhBadX25519ˢ));
    }
    return (@out, default!);
}

internal static void x25519ScalarMult(slice<byte> dst, slice<byte> scalar, slice<byte> point) {
    array<byte> e = new(32);
    copy(e[..], scalar[..]);
    e[0] &= (byte)(248);
    e[31] &= (byte)(127);
    e[31] |= (byte)(64);
    ref var x1 = ref heap(new field.Element(), out var Ꮡx1);
    ref var x2 = ref heap(new field.Element(), out var Ꮡx2);
    ref var z2 = ref heap(new field.Element(), out var Ꮡz2);
    ref var x3 = ref heap(new field.Element(), out var Ꮡx3);
    ref var z3 = ref heap(new field.Element(), out var Ꮡz3);
    ref var tmp0 = ref heap(new field.Element(), out var Ꮡtmp0);
    ref var tmp1 = ref heap(new field.Element(), out var Ꮡtmp1);
    Ꮡx1.SetBytes(point[..]);
    Ꮡx2.One();
    Ꮡx3.Set(Ꮡx1);
    Ꮡz3.One();
    nint swap = 0;
    for (nint pos = 254; pos >= 0; pos--) {
        var b = (byte)(e[pos / 8].Rsh((nuint)((nint)(pos & 7))));
        b &= (byte)(1);
        swap ^= (nint)((nint)b);
        x2.Swap(Ꮡx3, swap);
        z2.Swap(Ꮡz3, swap);
        swap = (nint)b;
        Ꮡtmp0.Subtract(Ꮡx3, Ꮡz3);
        Ꮡtmp1.Subtract(Ꮡx2, Ꮡz2);
        Ꮡx2.Add(Ꮡx2, Ꮡz2);
        Ꮡz2.Add(Ꮡx3, Ꮡz3);
        Ꮡz3.Multiply(Ꮡtmp0, Ꮡx2);
        Ꮡz2.Multiply(Ꮡz2, Ꮡtmp1);
        Ꮡtmp0.Square(Ꮡtmp1);
        Ꮡtmp1.Square(Ꮡx2);
        Ꮡx3.Add(Ꮡz3, Ꮡz2);
        Ꮡz2.Subtract(Ꮡz3, Ꮡz2);
        Ꮡx2.Multiply(Ꮡtmp1, Ꮡtmp0);
        Ꮡtmp1.Subtract(Ꮡtmp1, Ꮡtmp0);
        Ꮡz2.Square(Ꮡz2);
        Ꮡz3.Mult32(Ꮡtmp1, 121666);
        Ꮡx3.Square(Ꮡx3);
        Ꮡtmp0.Add(Ꮡtmp0, Ꮡz3);
        Ꮡz3.Multiply(Ꮡx1, Ꮡz2);
        Ꮡz2.Multiply(Ꮡtmp1, Ꮡtmp0);
    }
    x2.Swap(Ꮡx3, swap);
    z2.Swap(Ꮡz3, swap);
    Ꮡz2.Invert(Ꮡz2);
    Ꮡx2.Multiply(Ꮡx2, Ꮡz2);
    copy(dst[..], x2.Bytes());
}

// isZero reports whether x is all zeroes in constant time.
internal static bool isZero(slice<byte> x) {
    byte acc = default!;
    foreach (var (_, b) in x) {
        acc |= (byte)(b);
    }
    return acc == 0;
}

} // end ecdh_package
