// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ecdh = go.crypto.ecdh_package;
using rand = go.crypto.@internal.rand_package;
using errors = errors_package;
using byteorder = go.@internal.byteorder_package;
using slices = slices_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using io = io_package;

partial class hpke_package {

// A KEM is a Key Encapsulation Mechanism, one of the three components of an
// HPKE ciphersuite.
partial interface ΔKEM {
    // ID returns the HPKE KEM identifier.
    uint16 ID();
    // GenerateKey generates a new key pair.
    (PrivateKey, error) GenerateKey();
    // NewPublicKey deserializes a public key from bytes.
    //
    // It implements DeserializePublicKey, as defined in RFC 9180.
    (ΔPublicKey, error) NewPublicKey(slice<byte> _);
    // NewPrivateKey deserializes a private key from bytes.
    //
    // It implements DeserializePrivateKey, as defined in RFC 9180.
    (PrivateKey, error) NewPrivateKey(slice<byte> _);
    // DeriveKeyPair derives a key pair from the given input keying material.
    //
    // It implements DeriveKeyPair, as defined in RFC 9180.
    (PrivateKey, error) DeriveKeyPair(slice<byte> ikm);
    nint encSize();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsupportedKemˢ = "unsupported KEM"u8;

// NewKEM returns the KEM implementation for the given KEM ID.
//
// Applications are encouraged to use specific implementations like [DHKEM] or
// [MLKEM768X25519] instead, unless runtime agility is required.
public static (ΔKEM, error) NewKEM(uint16 id) {
    switch (id) {
    case 0x0010: { // DHKEM(P-256, HKDF-SHA256)
        return (DHKEM(ecdh.P256()), default);
    }
    case 0x0011: { // DHKEM(P-384, HKDF-SHA384)
        return (DHKEM(ecdh.P384()), default);
    }
    case 0x0012: { // DHKEM(P-521, HKDF-SHA512)
        return (DHKEM(ecdh.P521()), default);
    }
    case 0x0020: { // DHKEM(X25519, HKDF-SHA256)
        return (DHKEM(ecdh.X25519()), default);
    }
    case 0x0041: { // ML-KEM-768
        return (MLKEM768(), default);
    }
    case 0x0042: { // ML-KEM-1024
        return (MLKEM1024(), default);
    }
    case 0x647a: { // MLKEM768-X25519
        return (MLKEM768X25519(), default);
    }
    case 0x0050: { // MLKEM768-P256
        return (MLKEM768P256(), default);
    }
    case 0x0051: { // MLKEM1024-P384
        return (MLKEM1024P384(), default);
    }
    default: {
        return (default, errors.New(unsupportedKemˢ));
    }}
}

// A PublicKey is an instantiation of a KEM (one of the three components of an
// HPKE ciphersuite) with an encapsulation key (i.e. the public key).
//
// A PublicKey is usually obtained from a method of the corresponding [KEM] or
// [PrivateKey], such as [KEM.NewPublicKey] or [PrivateKey.PublicKey].
partial interface ΔPublicKey {
    // KEM returns the instantiated KEM.
    ΔKEM KEM();
    // Bytes returns the public key as the output of SerializePublicKey.
    slice<byte> Bytes();
    (slice<byte> sharedSecret, slice<byte> enc, error err) encap();
}

// A PrivateKey is an instantiation of a KEM (one of the three components of
// an HPKE ciphersuite) with a decapsulation key (i.e. the secret key).
//
// A PrivateKey is usually obtained from a method of the corresponding [KEM],
// such as [KEM.GenerateKey] or [KEM.NewPrivateKey].
partial interface PrivateKey {
    // KEM returns the instantiated KEM.
    ΔKEM KEM();
    // Bytes returns the private key as the output of SerializePrivateKey, as
    // defined in RFC 9180.
    //
    // Note that for X25519 this might not match the input to NewPrivateKey.
    // This is a requirement of RFC 9180, Section 7.1.2.
    (slice<byte>, error) Bytes();
    // PublicKey returns the corresponding PublicKey.
    ΔPublicKey PublicKey();
    (slice<byte> sharedSecret, error err) decap(slice<byte> enc);
}

partial struct dhKEM {
    internal KDF kdf;
    internal uint16 id;
    internal ecdhꓸCurve curve;
    public uint16 Nsecret;
    public uint16 Nsk;
    public nint Nenc;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string eaePrkˢ = "eae_prk"u8;
internal static readonly @string sharedSecretˢ = "shared_secret"u8;

internal static (slice<byte>, error) extractAndExpand(this ref dhKEM kem, slice<byte> dhKey, slice<byte> kemContext) {
    var suiteID = byteorder.BEAppendUint16(slice<byte>("KEM"u8), kem.id);
    var (eaePRK, err) = kem.kdf.labeledExtract(suiteID, default, eaePrkˢ, dhKey);
    if (err != nil) {
        return (default, err);
    }
    return kem.kdf.labeledExpand(suiteID, eaePRK, sharedSecretˢ, kemContext, kem.Nsecret);
}

internal static uint16 ID(this ref dhKEM kem) {
    return kem.id;
}

internal static nint encSize(this ref dhKEM kem) {
    return kem.Nenc;
}

internal static ж<dhKEM> dhKEMP256;
internal static void initᴛdhKEMP256() { dhKEMP256 = Ꮡ(new dhKEM(HKDFSHA256(), 0x0010, ecdh.P256(), 32, 32, 65)); }

internal static ж<dhKEM> dhKEMP384;
internal static void initᴛdhKEMP384() { dhKEMP384 = Ꮡ(new dhKEM(HKDFSHA384(), 0x0011, ecdh.P384(), 48, 48, 97)); }

internal static ж<dhKEM> dhKEMP521;
internal static void initᴛdhKEMP521() { dhKEMP521 = Ꮡ(new dhKEM(HKDFSHA512(), 0x0012, ecdh.P521(), 64, 66, 133)); }

internal static ж<dhKEM> dhKEMX25519;
internal static void initᴛdhKEMX25519() { dhKEMX25519 = Ꮡ(new dhKEM(HKDFSHA256(), 0x0020, ecdh.X25519(), 32, 32, 32)); }

// DHKEM returns a KEM implementing one of
//
//   - DHKEM(P-256, HKDF-SHA256)
//   - DHKEM(P-384, HKDF-SHA384)
//   - DHKEM(P-521, HKDF-SHA512)
//   - DHKEM(X25519, HKDF-SHA256)
//
// depending on curve.
public static ΔKEM DHKEM(ecdhꓸCurve curve) {
    switch (curve) {
    case var ᴛ when AreEqual(ᴛ, ecdh.P256()): {
        return new dhKEMжΔKEM(dhKEMP256);
    }
    case var ᴛ when AreEqual(ᴛ, ecdh.P384()): {
        return new dhKEMжΔKEM(dhKEMP384);
    }
    case var ᴛ when AreEqual(ᴛ, ecdh.P521()): {
        return new dhKEMжΔKEM(dhKEMP521);
    }
    case var ᴛ when AreEqual(ᴛ, ecdh.X25519()): {
        return new dhKEMжΔKEM(dhKEMX25519);
    }
    default: {
        // The set of ecdh.Curve implementations is closed, because the
        // interface has unexported methods. Therefore, this default case is
        // only hit if a new curve is added that DHKEM doesn't support.
        return new unsupportedCurveKEM();
    }}
}

partial struct unsupportedCurveKEM {
}

internal static uint16 ID(this unsupportedCurveKEM _) {
    return 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsupportedCurveˢ = "unsupported curve"u8;

internal static (PrivateKey, error) GenerateKey(this unsupportedCurveKEM _) {
    return (default, errors.New(unsupportedCurveˢ));
}

internal static (ΔPublicKey, error) NewPublicKey(this unsupportedCurveKEM _Δp0, slice<byte> _Δp1) {
    return (default, errors.New(unsupportedCurveˢ));
}

internal static (PrivateKey, error) NewPrivateKey(this unsupportedCurveKEM _Δp0, slice<byte> _Δp1) {
    return (default, errors.New(unsupportedCurveˢ));
}

internal static (PrivateKey, error) DeriveKeyPair(this unsupportedCurveKEM _Δp0, slice<byte> _Δp1) {
    return (default, errors.New(unsupportedCurveˢ));
}

internal static nint encSize(this unsupportedCurveKEM _) {
    return 0;
}

partial struct dhKEMPublicKey {
    internal ж<dhKEM> kem;
    internal ж<ecdhꓸPublicKey> pub;
}

// NewDHKEMPublicKey returns a PublicKey implementing
//
//   - DHKEM(P-256, HKDF-SHA256)
//   - DHKEM(P-384, HKDF-SHA384)
//   - DHKEM(P-521, HKDF-SHA512)
//   - DHKEM(X25519, HKDF-SHA256)
//
// depending on the underlying curve of pub ([ecdh.X25519], [ecdh.P256],
// [ecdh.P384], or [ecdh.P521]).
//
// This function is meant for applications that already have an instantiated
// crypto/ecdh public key. Otherwise, applications should use the
// [KEM.NewPublicKey] method of [DHKEM].
public static (ΔPublicKey, error) NewDHKEMPublicKey(ж<ecdhꓸPublicKey> Ꮡpub) {
    ref var pub = ref Ꮡpub.DerefOrNull();

    var (kem, ok) = DHKEM(pub.Curve())._<ж<dhKEM>>(ᐧ);
    if (!ok) {
        return (default, errors.New(unsupportedCurveˢ));
    }
    return (new dhKEMPublicKeyжΔPublicKey(Ꮡ(new dhKEMPublicKey(
        kem: kem,
        pub: Ꮡpub
    ))), default);
}

internal static (ΔPublicKey, error) NewPublicKey(this ref dhKEM kem, slice<byte> data) {
    var (pub, err) = kem.curve.NewPublicKey(data);
    if (err != nil) {
        return (default, err);
    }
    return NewDHKEMPublicKey(pub);
}

internal static ΔKEM KEM(this ref dhKEMPublicKey pk) {
    return new dhKEMжΔKEM(pk.kem);
}

internal static slice<byte> Bytes(this ref dhKEMPublicKey pk) {
    return pk.pub.Bytes();
}

// testingOnlyGenerateKey is only used during testing, to provide
// a fixed test key to use when checking the RFC 9180 vectors.
internal static Func<ж<ecdh.PrivateKey>> testingOnlyGenerateKey;

internal static (slice<byte> sharedSecret, slice<byte> encapPub, error err) encap(this ref dhKEMPublicKey pk) {
    slice<byte> sharedSecret = default;
    error err = default;

    (var privEph, err) = pk.pub.Curve().GenerateKey(rand.Reader);
    if (err != nil) {
        return (default, default, err);
    }
    if (testingOnlyGenerateKey != nil) {
        privEph = testingOnlyGenerateKey();
    }
    (var dhVal, err) = privEph.ECDH(pk.pub);
    if (err != nil) {
        return (default, default, err);
    }
    var encPubEph = privEph.PublicKey().Bytes();

    var encPubRecip = pk.pub.Bytes();
    var kemContext = appendꓸꓸꓸ(encPubEph, encPubRecip);
    (sharedSecret, err) = pk.kem.extractAndExpand(dhVal, kemContext);
    if (err != nil) {
        return (default, default, err);
    }
    return (sharedSecret, encPubEph, default);
}

partial struct dhKEMPrivateKey {
    internal ж<dhKEM> kem;
    internal ecdh.KeyExchanger priv;
}

// NewDHKEMPrivateKey returns a PrivateKey implementing
//
//   - DHKEM(P-256, HKDF-SHA256)
//   - DHKEM(P-384, HKDF-SHA384)
//   - DHKEM(P-521, HKDF-SHA512)
//   - DHKEM(X25519, HKDF-SHA256)
//
// depending on the underlying curve of priv ([ecdh.X25519], [ecdh.P256],
// [ecdh.P384], or [ecdh.P521]).
//
// This function is meant for applications that already have an instantiated
// crypto/ecdh private key, or another implementation of a [ecdh.KeyExchanger]
// (e.g. a hardware key). Otherwise, applications should use the
// [KEM.NewPrivateKey] method of [DHKEM].
public static (PrivateKey, error) NewDHKEMPrivateKey(ecdh.KeyExchanger priv) {
    var (kem, ok) = DHKEM(priv.Curve())._<ж<dhKEM>>(ᐧ);
    if (!ok) {
        return (default, errors.New(unsupportedCurveˢ));
    }
    return (new dhKEMPrivateKeyжPrivateKey(Ꮡ(new dhKEMPrivateKey(
        kem: kem,
        priv: priv
    ))), default);
}

internal static (PrivateKey, error) GenerateKey(this ref dhKEM kem) {
    var (priv, err) = kem.curve.GenerateKey(rand.Reader);
    if (err != nil) {
        return (default, err);
    }
    return NewDHKEMPrivateKey(new ecdh.PrivateKeyжKeyExchanger(priv));
}

internal static (PrivateKey, error) NewPrivateKey(this ref dhKEM kem, slice<byte> ikm) {
    var (priv, err) = kem.curve.NewPrivateKey(ikm);
    if (err != nil) {
        return (default, err);
    }
    return NewDHKEMPrivateKey(new ecdh.PrivateKeyжKeyExchanger(priv));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dkpPrkˢ = "dkp_prk"u8;
internal static readonly @string candidateˢ = "candidate"u8;

internal static (PrivateKey, error) DeriveKeyPair(this ж<dhKEM> Ꮡkem, slice<byte> ikm) {
    ref var kem = ref Ꮡkem.DerefOrNull();

    // DeriveKeyPair from RFC 9180 Section 7.1.3.
    var suiteID = byteorder.BEAppendUint16(slice<byte>("KEM"u8), kem.id);
    var (prk, err) = kem.kdf.labeledExtract(suiteID, default, dkpPrkˢ, ikm);
    if (err != nil) {
        return (default, err);
    }
    if (Ꮡkem == dhKEMX25519) {
        var (s, errΔ1) = kem.kdf.labeledExpand(suiteID, prk, "sk"u8, default, kem.Nsk);
        if (errΔ1 != nil) {
            return (default, errΔ1);
        }
        return kem.NewPrivateKey(s);
    }
    uint8 counter = default;
    while (counter < 4) {
        var (s, errΔ2) = kem.kdf.labeledExpand(suiteID, prk, candidateˢ, slice<byte>([counter]), kem.Nsk);
        if (errΔ2 != nil) {
            return (default, errΔ2);
        }
        if (Ꮡkem == dhKEMP521) {
            s[0] &= (byte)(0x01);
        }
        (var r, errΔ2) = kem.NewPrivateKey(s);
        if (errΔ2 != nil) {
            counter++;
            continue;
        }
        return (r, default);
    }
    throw panic("chance of four rejections is < 2^-128");
}

internal static ΔKEM KEM(this ref dhKEMPrivateKey k) {
    return new dhKEMжΔKEM(k.kem);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdhPrivateKeyDoesNotˢ = "ecdh: private key does not support Bytes"u8;

internal static (slice<byte>, error) Bytes(this ref dhKEMPrivateKey k) {
    // Bizarrely, RFC 9180, Section 7.1.2 says SerializePrivateKey MUST clamp
    // the output, which I thought we all agreed to instead do as part of the DH
    // function, letting private keys be random bytes.
    //
    // At the same time, it says DeserializePrivateKey MUST also clamp, implying
    // that the input doesn't have to be clamped, so Bytes by spec doesn't
    // necessarily match the NewPrivateKey input.
    //
    // I'm sure this will not lead to any unexpected behavior or interop issue.
    var (priv, ok) = k.priv._<ж<ecdh.PrivateKey>>(ᐧ);
    if (!ok) {
        return (default, errors.New(ecdhPrivateKeyDoesNotˢ));
    }
    if (k.kem == dhKEMX25519) {
        var b = priv.Bytes();
        b[0] &= (byte)(248);
        b[31] &= (byte)(127);
        b[31] |= (byte)(64);
        return (b, default);
    }
    return (priv.Bytes(), default);
}

internal static ΔPublicKey PublicKey(this ref dhKEMPrivateKey k) {
    return new dhKEMPublicKeyжΔPublicKey(Ꮡ(new dhKEMPublicKey(
        kem: k.kem,
        pub: k.priv.PublicKey()
    )));
}

internal static (slice<byte>, error) decap(this ref dhKEMPrivateKey k, slice<byte> encPubEph) {
    var (pubEph, err) = k.priv.Curve().NewPublicKey(encPubEph);
    if (err != nil) {
        return (default, err);
    }
    (var dhVal, err) = k.priv.ECDH(pubEph);
    if (err != nil) {
        return (default, err);
    }
    var kemContext = appendꓸꓸꓸ(slices.Clip<slice<byte>, byte>(encPubEph), k.priv.PublicKey().Bytes());
    return k.kem.extractAndExpand(dhVal, kemContext);
}

} // end hpke_package
