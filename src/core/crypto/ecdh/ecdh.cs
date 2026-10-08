// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ecdh implements Elliptic Curve Diffie-Hellman over
// NIST curves and Curve25519.
namespace go.crypto;

using crypto = crypto_package;
using boring = go.crypto.@internal.boring_package;
using ecdh = go.crypto.@internal.fips140.ecdh_package;
using subtle = go.crypto.subtle_package;
using errors = errors_package;
using io = io_package;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class ecdh_package {

partial interface ΔCurve {
    // GenerateKey generates a random PrivateKey.
    //
    // Most applications should use [crypto/rand.Reader] as rand. Note that the
    // returned key does not depend deterministically on the bytes read from rand,
    // and may change between calls and/or between versions.
    (ж<PrivateKey>, error) GenerateKey(io.Reader rand);
    // NewPrivateKey checks that key is valid and returns a PrivateKey.
    //
    // For NIST curves, this follows SEC 1, Version 2.0, Section 2.3.6, which
    // amounts to decoding the bytes as a fixed length big endian integer and
    // checking that the result is lower than the order of the curve. The zero
    // private key is also rejected, as the encoding of the corresponding public
    // key would be irregular.
    //
    // For X25519, this only checks the scalar length.
    (ж<PrivateKey>, error) NewPrivateKey(slice<byte> key);
    // NewPublicKey checks that key is valid and returns a PublicKey.
    //
    // For NIST curves, this decodes an uncompressed point according to SEC 1,
    // Version 2.0, Section 2.3.4. Compressed encodings and the point at
    // infinity are rejected.
    //
    // For X25519, this only checks the u-coordinate length. Adversarially
    // selected public keys can cause ECDH to return an error.
    (ж<ΔPublicKey>, error) NewPublicKey(slice<byte> key);
    // ecdh performs an ECDH exchange and returns the shared secret. It's exposed
    // as the PrivateKey.ECDH method.
    //
    // The private method also allow us to expand the ECDH interface with more
    // methods in the future without breaking backwards compatibility.
    (slice<byte>, error) ecdh(ж<PrivateKey> local, ж<ΔPublicKey> remote);
}

// PublicKey is an ECDH public key, usually a peer's ECDH share sent over the wire.
//
// These keys can be parsed with [crypto/x509.ParsePKIXPublicKey] and encoded
// with [crypto/x509.MarshalPKIXPublicKey]. For NIST curves, they then need to
// be converted with [crypto/ecdsa.PublicKey.ECDH] after parsing.
partial struct ΔPublicKey {
    internal ΔCurve curve;
    internal slice<byte> publicKey;
    internal ж<boring.PublicKeyECDH> boring;
    internal ж<ecdhꓸPublicKey> fips;
}

// Bytes returns a copy of the encoding of the public key.
public static slice<byte> Bytes(this ref ΔPublicKey k) {
    // Copy the public key to a fixed size buffer that can get allocated on the
    // caller's stack after inlining.
    array<byte> buf = new(133);
    return appendꓸꓸꓸ(buf[..0], k.publicKey);
}

// Equal returns whether x represents the same public key as k.
//
// Note that there can be equivalent public keys with different encodings which
// would return false from this check but behave the same way as inputs to ECDH.
//
// This check is performed in constant time as long as the key types and their
// curve match.
public static bool Equal(this ref ΔPublicKey k, cryptoꓸPublicKey x) {
    var (xx, ok) = x._<ж<ΔPublicKey>>(ᐧ);
    if (!ok) {
        return false;
    }
    return AreEqual(k.curve, (~xx).curve) && subtle.ConstantTimeCompare(k.publicKey, (~xx).publicKey) == 1;
}

public static ΔCurve Curve(this ref ΔPublicKey k) {
    return k.curve;
}

// PrivateKey is an ECDH private key, usually kept secret.
//
// These keys can be parsed with [crypto/x509.ParsePKCS8PrivateKey] and encoded
// with [crypto/x509.MarshalPKCS8PrivateKey]. For NIST curves, they then need to
// be converted with [crypto/ecdsa.PrivateKey.ECDH] after parsing.
partial struct PrivateKey {
    internal ΔCurve curve;
    internal slice<byte> privateKey;
    internal ж<ΔPublicKey> publicKey;
    internal ж<boring.PrivateKeyECDH> boring;
    internal ж<ecdh.PrivateKey> fips;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoEcdhPrivateKeyAndˢ = "crypto/ecdh: private key and public key curves do not match"u8;

// ECDH performs an ECDH exchange and returns the shared secret. The [PrivateKey]
// and [PublicKey] must use the same curve.
//
// For NIST curves, this performs ECDH as specified in SEC 1, Version 2.0,
// Section 3.3.1, and returns the x-coordinate encoded according to SEC 1,
// Version 2.0, Section 2.3.5. The result is never the point at infinity.
// This is also known as the Shared Secret Computation of the Ephemeral Unified
// Model scheme specified in NIST SP 800-56A Rev. 3, Section 6.1.2.2.
//
// For [X25519], this performs ECDH as specified in RFC 7748, Section 6.1. If
// the result is the all-zero value, ECDH returns an error.
public static (slice<byte>, error) ECDH(this ж<PrivateKey> Ꮡk, ж<ΔPublicKey> Ꮡremote) {
    ref var k = ref Ꮡk.DerefOrNull();
    ref var remote = ref Ꮡremote.DerefOrNull();

    if (!AreEqual(k.curve, remote.curve)) {
        return (default!, errors.New(cryptoEcdhPrivateKeyAndˢ));
    }
    return k.curve.ecdh(Ꮡk, Ꮡremote);
}

// Bytes returns a copy of the encoding of the private key.
public static slice<byte> Bytes(this ref PrivateKey k) {
    // Copy the private key to a fixed size buffer that can get allocated on the
    // caller's stack after inlining.
    array<byte> buf = new(66);
    return appendꓸꓸꓸ(buf[..0], k.privateKey);
}

// Equal returns whether x represents the same private key as k.
//
// Note that there can be equivalent private keys with different encodings which
// would return false from this check but behave the same way as inputs to [ECDH].
//
// This check is performed in constant time as long as the key types and their
// curve match.
public static bool Equal(this ref PrivateKey k, cryptoꓸPrivateKey x) {
    var (xx, ok) = x._<ж<PrivateKey>>(ᐧ);
    if (!ok) {
        return false;
    }
    return AreEqual(k.curve, (~xx).curve) && subtle.ConstantTimeCompare(k.privateKey, (~xx).privateKey) == 1;
}

public static ΔCurve Curve(this ref PrivateKey k) {
    return k.curve;
}

public static ж<ΔPublicKey> PublicKey(this ref PrivateKey k) {
    return k.publicKey;
}

// Public implements the implicit interface of all standard library private
// keys. See the docs of [crypto.PrivateKey].
public static cryptoꓸPublicKey Public(this ref PrivateKey k) {
    return k.PublicKey().OrTypedNil();
}

} // end ecdh_package
