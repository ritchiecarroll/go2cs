// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using boring = go.crypto.@internal.boring_package;
using ecdh = go.crypto.@internal.fips140.ecdh_package;
using fips140only = go.crypto.@internal.fips140only_package;
using errors = errors_package;
using io = io_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using nistec = go.crypto.@internal.fips140.nistec_package;

partial class ecdh_package {

partial struct nistCurve {
    internal @string name;
    internal Func<io.Reader, (ж<ecdh.PrivateKey>, error)> generate;
    internal Func<slice<byte>, (ж<ecdh.PrivateKey>, error)> newPrivateKey;
    internal Func<slice<byte>, (ж<ecdhꓸPublicKey>, error)> newPublicKey;
    internal Func<ж<ecdh.PrivateKey>, ж<ecdhꓸPublicKey>, (slice<byte> sharedSecret, error err)> sharedSecret;
}

internal static @string String(this ref nistCurve c) {
    return c.name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoEcdhOnlyCryptoRandˢ = "crypto/ecdh: only crypto/rand.Reader is allowed in FIPS 140-only mode"u8;

internal static (ж<PrivateKey>, error) GenerateKey(this ж<nistCurve> Ꮡc, io.Reader rand) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (boring.Enabled && AreEqual(rand, boring.RandReader)) {
        var (key, bytes, errΔ1) = boring.GenerateKeyECDH(c.name);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        (var pub, errΔ1) = key.PublicKey();
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        var kΔ1 = Ꮡ(new PrivateKey(
            curve: new nistCurveжΔCurve(Ꮡc),
            privateKey: bytes,
            publicKey: Ꮡ(new ΔPublicKey(curve: new nistCurveжΔCurve(Ꮡc), publicKey: pub.Bytes(), boring: pub)),
            boring: key
        ));
        return (kΔ1, default!);
    }
    if (fips140only.Enabled && !fips140only.ApprovedRandomReader(rand)) {
        return (default!, errors.New(cryptoEcdhOnlyCryptoRandˢ));
    }
    var (privateKey, err) = c.generate(rand);
    if (err != default!) {
        return (default!, err);
    }
    var k = Ꮡ(new PrivateKey(
        curve: new nistCurveжΔCurve(Ꮡc),
        privateKey: privateKey.Bytes(),
        fips: privateKey,
        publicKey: Ꮡ(new ΔPublicKey(
            curve: new nistCurveжΔCurve(Ꮡc),
            publicKey: privateKey.PublicKey().Bytes(),
            fips: privateKey.PublicKey()
        ))
    ));
    if (boring.Enabled) {
        var (bk, errΔ2) = boring.NewPrivateKeyECDH(c.name, (~k).privateKey);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        (var pub, errΔ2) = bk.PublicKey();
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        k.Value.boring = bk;
        k.Value.publicKey.Value.boring = pub;
    }
    return (k, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoEcdhInvalidPrivateˢ = "crypto/ecdh: invalid private key"u8;

internal static (ж<PrivateKey>, error) NewPrivateKey(this ж<nistCurve> Ꮡc, slice<byte> key) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (boring.Enabled) {
        var (bk, errΔ1) = boring.NewPrivateKeyECDH(c.name, key);
        if (errΔ1 != default!) {
            return (default!, errors.New(cryptoEcdhInvalidPrivateˢ));
        }
        (var pub, errΔ1) = bk.PublicKey();
        if (errΔ1 != default!) {
            return (default!, errors.New(cryptoEcdhInvalidPrivateˢ));
        }
        var kΔ1 = Ꮡ(new PrivateKey(
            curve: new nistCurveжΔCurve(Ꮡc),
            privateKey: bytes.Clone(key),
            publicKey: Ꮡ(new ΔPublicKey(curve: new nistCurveжΔCurve(Ꮡc), publicKey: pub.Bytes(), boring: pub)),
            boring: bk
        ));
        return (kΔ1, default!);
    }
    var (fk, err) = c.newPrivateKey(key);
    if (err != default!) {
        return (default!, err);
    }
    var k = Ꮡ(new PrivateKey(
        curve: new nistCurveжΔCurve(Ꮡc),
        privateKey: bytes.Clone(key),
        fips: fk,
        publicKey: Ꮡ(new ΔPublicKey(
            curve: new nistCurveжΔCurve(Ꮡc),
            publicKey: fk.PublicKey().Bytes(),
            fips: fk.PublicKey()
        ))
    ));
    return (k, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoEcdhInvalidPublicˢ = "crypto/ecdh: invalid public key"u8;

internal static (ж<ΔPublicKey>, error) NewPublicKey(this ж<nistCurve> Ꮡc, slice<byte> key) {
    ref var c = ref Ꮡc.DerefOrNull();

    // Reject the point at infinity and compressed encodings.
    // Note that boring.NewPublicKeyECDH would accept them.
    if (len(key) == 0 || key[0] != 4) {
        return (default!, errors.New(cryptoEcdhInvalidPublicˢ));
    }
    var k = Ꮡ(new ΔPublicKey(
        curve: new nistCurveжΔCurve(Ꮡc),
        publicKey: bytes.Clone(key)
    ));
    if (boring.Enabled){
        var (bk, err) = boring.NewPublicKeyECDH(c.name, (~k).publicKey);
        if (err != default!) {
            return (default!, errors.New(cryptoEcdhInvalidPublicˢ));
        }
        k.Value.boring = bk;
    } else {
        var (fk, err) = c.newPublicKey(key);
        if (err != default!) {
            return (default!, err);
        }
        k.Value.fips = fk;
    }
    return (k, default!);
}

internal static (slice<byte>, error) ecdh(this ref nistCurve c, ж<PrivateKey> Ꮡlocal, ж<ΔPublicKey> Ꮡremote) {
    ref var local = ref Ꮡlocal.DerefOrNull();
    ref var remote = ref Ꮡremote.DerefOrNull();

    // Note that this function can't return an error, as NewPublicKey rejects
    // invalid points and the point at infinity, and NewPrivateKey rejects
    // invalid scalars and the zero value. BytesX returns an error for the point
    // at infinity, but in a prime order group such as the NIST curves that can
    // only be the result of a scalar multiplication if one of the inputs is the
    // zero scalar or the point at infinity.
    if (boring.Enabled) {
        return boring.ECDH(local.boring, remote.boring);
    }
    return c.sharedSecret(local.fips, remote.fips);
}

// P256 returns a [Curve] which implements NIST P-256 (FIPS 186-3, section D.2.3),
// also known as secp256r1 or prime256v1.
//
// Multiple invocations of this function will return the same value, which can
// be used for equality checks and switch statements.
public static ΔCurve P256() {
    return new nistCurveжΔCurve(p256);
}

internal static ж<nistCurve> p256 = Ꮡ(new nistCurve(
    name: "P-256"u8,
    generate: (io.Reader r) => go.crypto.@internal.fips140.ecdh_package.GenerateKey<ecdh.P256PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P256(), r),
    newPrivateKey: (slice<byte> b) => go.crypto.@internal.fips140.ecdh_package.NewPrivateKey<ecdh.P256PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P256(), b),
    newPublicKey: (slice<byte> publicKey) => go.crypto.@internal.fips140.ecdh_package.NewPublicKey<ecdh.P256PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P256(), publicKey),
    sharedSecret: (ж<ecdh.PrivateKey> priv, ж<ecdhꓸPublicKey> pub) => {
        return go.crypto.@internal.fips140.ecdh_package.ECDH<ecdh.P256PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P256(), priv, pub);
    }
));

// P384 returns a [Curve] which implements NIST P-384 (FIPS 186-3, section D.2.4),
// also known as secp384r1.
//
// Multiple invocations of this function will return the same value, which can
// be used for equality checks and switch statements.
public static ΔCurve P384() {
    return new nistCurveжΔCurve(p384);
}

internal static ж<nistCurve> p384 = Ꮡ(new nistCurve(
    name: "P-384"u8,
    generate: (io.Reader r) => go.crypto.@internal.fips140.ecdh_package.GenerateKey<ecdh.P384PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P384(), r),
    newPrivateKey: (slice<byte> b) => go.crypto.@internal.fips140.ecdh_package.NewPrivateKey<ecdh.P384PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P384(), b),
    newPublicKey: (slice<byte> publicKey) => go.crypto.@internal.fips140.ecdh_package.NewPublicKey<ecdh.P384PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P384(), publicKey),
    sharedSecret: (ж<ecdh.PrivateKey> priv, ж<ecdhꓸPublicKey> pub) => {
        return go.crypto.@internal.fips140.ecdh_package.ECDH<ecdh.P384PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P384(), priv, pub);
    }
));

// P521 returns a [Curve] which implements NIST P-521 (FIPS 186-3, section D.2.5),
// also known as secp521r1.
//
// Multiple invocations of this function will return the same value, which can
// be used for equality checks and switch statements.
public static ΔCurve P521() {
    return new nistCurveжΔCurve(p521);
}

internal static ж<nistCurve> p521 = Ꮡ(new nistCurve(
    name: "P-521"u8,
    generate: (io.Reader r) => go.crypto.@internal.fips140.ecdh_package.GenerateKey<ecdh.P521PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P521(), r),
    newPrivateKey: (slice<byte> b) => go.crypto.@internal.fips140.ecdh_package.NewPrivateKey<ecdh.P521PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P521(), b),
    newPublicKey: (slice<byte> publicKey) => go.crypto.@internal.fips140.ecdh_package.NewPublicKey<ecdh.P521PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P521(), publicKey),
    sharedSecret: (ж<ecdh.PrivateKey> priv, ж<ecdhꓸPublicKey> pub) => {
        return go.crypto.@internal.fips140.ecdh_package.ECDH<ecdh.P521PointжPoint>(go.crypto.@internal.fips140.ecdh_package.P521(), priv, pub);
    }
));

} // end ecdh_package
