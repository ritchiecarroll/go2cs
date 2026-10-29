// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using elliptic = go.crypto.elliptic_package;
using rsa = go.crypto.rsa_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using io = io_package;
using slices = slices_package;
using go.@internal;
using go.crypto;

partial class tls_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdsaVerificationFailureˢ = "ECDSA verification failure"u8;
internal static readonly @string ed25519Verificationˢ = "Ed25519 verification failure"u8;
internal static readonly @string internalErrorUnknownˢ = "internal error: unknown signature type"u8;

// verifyHandshakeSignature verifies a signature against unhashed handshake contents.
internal static error verifyHandshakeSignature(uint8 sigType, cryptoꓸPublicKey pubkey, crypto.Hash hashFunc, slice<byte> signed, slice<byte> sig) {
    if (hashFunc != directSigning) {
        var h = hashFunc.New();
        h.Write(signed);
        signed = h.Sum(default);
    }
    switch (sigType) {
    case signatureECDSA: {
        var (pubKey, ok) = pubkey._<ж<ecdsa.PublicKey>>(ᐧ);
        if (!ok) {
            return fmt.Errorf("expected an ECDSA public key, got %T"u8, pubkey);
        }
        if (!ecdsa.VerifyASN1(pubKey, signed, sig)) {
            return errors.New(ecdsaVerificationFailureˢ);
        }
        break;
    }
    case signatureEd25519: {
        var (pubKey, ok) = pubkey._<ed25519.PublicKey>(ᐧ);
        if (!ok) {
            return fmt.Errorf("expected an Ed25519 public key, got %T"u8, pubkey);
        }
        if (!ed25519.Verify(pubKey, signed, sig)) {
            return errors.New(ed25519Verificationˢ);
        }
        break;
    }
    case signaturePKCS1v15: {
        var (pubKey, ok) = pubkey._<ж<rsa.PublicKey>>(ᐧ);
        if (!ok) {
            return fmt.Errorf("expected an RSA public key, got %T"u8, pubkey);
        }
        {
            var err = rsa.VerifyPKCS1v15(pubKey, hashFunc, signed, sig); if (err != nil) {
                return err;
            }
        }
        break;
    }
    case signatureRSAPSS: {
        var (pubKey, ok) = pubkey._<ж<rsa.PublicKey>>(ᐧ);
        if (!ok) {
            return fmt.Errorf("expected an RSA public key, got %T"u8, pubkey);
        }
        var signOpts = Ꮡ(new rsa.PSSOptions(SaltLength: rsa.PSSSaltLengthEqualsHash));
        {
            var err = rsa.VerifyPSS(pubKey, hashFunc, signed, sig, signOpts); if (err != nil) {
                return err;
            }
        }
        break;
    }
    default: {
        return errors.New(internalErrorUnknownˢ);
    }}
    return default;
}

// verifyLegacyHandshakeSignature verifies a TLS 1.0 and 1.1 signature against
// pre-hashed handshake contents.
internal static error verifyLegacyHandshakeSignature(uint8 sigType, cryptoꓸPublicKey pubkey, crypto.Hash hashFunc, slice<byte> hashed, slice<byte> sig) {
    switch (sigType) {
    case signatureECDSA: {
        var (pubKey, ok) = pubkey._<ж<ecdsa.PublicKey>>(ᐧ);
        if (!ok) {
            return fmt.Errorf("expected an ECDSA public key, got %T"u8, pubkey);
        }
        if (!ecdsa.VerifyASN1(pubKey, hashed, sig)) {
            return errors.New(ecdsaVerificationFailureˢ);
        }
        break;
    }
    case signaturePKCS1v15: {
        var (pubKey, ok) = pubkey._<ж<rsa.PublicKey>>(ᐧ);
        if (!ok) {
            return fmt.Errorf("expected an RSA public key, got %T"u8, pubkey);
        }
        {
            var err = rsa.VerifyPKCS1v15(pubKey, hashFunc, hashed, sig); if (err != nil) {
                return err;
            }
        }
        break;
    }
    default: {
        return errors.New(internalErrorUnknownˢ);
    }}
    return default;
}

internal static readonly @string serverSignatureContext = "TLS 1.3, server CertificateVerify\x00"u8;
internal static readonly @string clientSignatureContext = "TLS 1.3, client CertificateVerify\x00"u8;

[GoLazy] partial class lazyᴛsignaturePadding { internal static slice<byte> signaturePadding = slice<byte>([
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
]); }

// signedMessage returns the (unhashed) message to be signed by certificate keys
// in TLS 1.3. See RFC 8446, Section 4.4.3.
internal static slice<byte> signedMessage(@string context, hash.Hash transcript) {
/* signaturePadding */
    const nint maxSize = /* 64 + len(serverSignatureContext) + 512/8 */ 162; /* SHA-512 */
    var b = bytes.NewBuffer(new slice<byte>(0, maxSize));
    b.Write(signaturePadding);
    io.WriteString(new bytes_BufferжWriter(b), context);
    b.Write(transcript.Sum(default));
    return b.Bytes();
}

// typeAndHashFromSignatureScheme returns the corresponding signature type and
// crypto.Hash for a given TLS SignatureScheme.
internal static (uint8 sigType, crypto.Hash hash, error err) typeAndHashFromSignatureScheme(SignatureScheme signatureAlgorithm) {
    uint8 sigType = default;
    crypto.Hash hash = default;

    switch (signatureAlgorithm) {
    case var ᴛ when ᴛ == PKCS1WithSHA1 || ᴛ == PKCS1WithSHA256 || ᴛ == PKCS1WithSHA384 || ᴛ == PKCS1WithSHA512: {
        sigType = signaturePKCS1v15;
        break;
    }
    case var ᴛ when ᴛ == PSSWithSHA256 || ᴛ == PSSWithSHA384 || ᴛ == PSSWithSHA512: {
        sigType = signatureRSAPSS;
        break;
    }
    case var ᴛ when ᴛ == ECDSAWithSHA1 || ᴛ == ECDSAWithP256AndSHA256 || ᴛ == ECDSAWithP384AndSHA384 || ᴛ == ECDSAWithP521AndSHA512: {
        sigType = signatureECDSA;
        break;
    }
    case var ᴛ when ᴛ == Ed25519: {
        sigType = signatureEd25519;
        break;
    }
    default: {
        return (0, 0, fmt.Errorf("unsupported signature algorithm: %v"u8, signatureAlgorithm));
    }}
    switch (signatureAlgorithm) {
    case var ᴛ when ᴛ == PKCS1WithSHA1 || ᴛ == ECDSAWithSHA1: {
        hash = crypto.SHA1;
        break;
    }
    case var ᴛ when ᴛ == PKCS1WithSHA256 || ᴛ == PSSWithSHA256 || ᴛ == ECDSAWithP256AndSHA256: {
        hash = crypto.SHA256;
        break;
    }
    case var ᴛ when ᴛ == PKCS1WithSHA384 || ᴛ == PSSWithSHA384 || ᴛ == ECDSAWithP384AndSHA384: {
        hash = crypto.SHA384;
        break;
    }
    case var ᴛ when ᴛ == PKCS1WithSHA512 || ᴛ == PSSWithSHA512 || ᴛ == ECDSAWithP521AndSHA512: {
        hash = crypto.SHA512;
        break;
    }
    case var ᴛ when ᴛ == Ed25519: {
        hash = directSigning;
        break;
    }
    default: {
        return (0, 0, fmt.Errorf("unsupported signature algorithm: %v"u8, signatureAlgorithm));
    }}
    return (sigType, hash, default);
}

// legacyTypeAndHashFromPublicKey returns the fixed signature type and crypto.Hash for
// a given public key used with TLS 1.0 and 1.1, before the introduction of
// signature algorithm negotiation.
internal static (uint8 sigType, crypto.Hash hash, error err) legacyTypeAndHashFromPublicKey(cryptoꓸPublicKey pub) {
    switch (pub.type()) {
    case ж<rsa.PublicKey>: {
        return (signaturePKCS1v15, crypto.MD5SHA1, default);
    }
    case ж<ecdsa.PublicKey>: {
        return (signatureECDSA, crypto.SHA1, default);
    }
    case ed25519.PublicKey: {
        // RFC 8422 specifies support for Ed25519 in TLS 1.0 and 1.1,
        // but it requires holding on to a handshake transcript to do a
        // full signature, and not even OpenSSL bothers with the
        // complexity, so we can't even test it properly.
        return (0, 0, fmt.Errorf("tls: Ed25519 public keys are not supported before TLS 1.2"u8));
    }
    default: {
        return (0, 0, fmt.Errorf("tls: unsupported public key: %T"u8, pub));
    }}
}

partial struct rsaSignatureSchemesᴛ1 /*dyn*/ {
    internal SignatureScheme scheme;
    internal nint minModulusBytes;
}
internal static slice<rsaSignatureSchemesᴛ1> rsaSignatureSchemes = slice<rsaSignatureSchemesᴛ1>([
    // RSA-PSS is used with PSSSaltLengthEqualsHash, and requires
    //    emLen >= hLen + sLen + 2
    new(PSSWithSHA256, crypto.SHA256.Size() * 2 + 2),
    new(PSSWithSHA384, crypto.SHA384.Size() * 2 + 2),
    new(PSSWithSHA512, crypto.SHA512.Size() * 2 + 2),
    // PKCS #1 v1.5 uses prefixes from hashPrefixes in crypto/rsa, and requires
    //    emLen >= len(prefix) + hLen + 11
    new(PKCS1WithSHA256, 19 + crypto.SHA256.Size() + 11),
    new(PKCS1WithSHA384, 19 + crypto.SHA384.Size() + 11),
    new(PKCS1WithSHA512, 19 + crypto.SHA512.Size() + 11),
    new(PKCS1WithSHA1, 15 + crypto.SHA1.Size() + 11)
]);

internal static slice<SignatureScheme> signatureSchemesForPublicKey(uint16 version, cryptoꓸPublicKey pub) {
    switch (pub.type()) {
    case ж<ecdsa.PublicKey> pubΔ1: {
        if (version < VersionTLS13) {
            // In TLS 1.2 and earlier, ECDSA algorithms are not
            // constrained to a single curve.
            return slice<SignatureScheme>([
                ECDSAWithP256AndSHA256,
                ECDSAWithP384AndSHA384,
                ECDSAWithP521AndSHA512,
                ECDSAWithSHA1
            ]);
        }
        switch (pubΔ1.Curve) {
        case var ᴛ when AreEqual(ᴛ, elliptic.P256()): {
            return slice<SignatureScheme>([ECDSAWithP256AndSHA256]);
        }
        case var ᴛ when AreEqual(ᴛ, elliptic.P384()): {
            return slice<SignatureScheme>([ECDSAWithP384AndSHA384]);
        }
        case var ᴛ when AreEqual(ᴛ, elliptic.P521()): {
            return slice<SignatureScheme>([ECDSAWithP521AndSHA512]);
        }
        default: {
            return default;
        }}
        break;
    }
    case ж<rsa.PublicKey> pubΔ1: {
        nint size = pubΔ1.Size();
        var sigAlgs = new slice<SignatureScheme>(0, len(rsaSignatureSchemes));
        foreach (var (_, candidate) in rsaSignatureSchemes) {
            if (size >= candidate.minModulusBytes) {
                sigAlgs = append(sigAlgs, candidate.scheme);
            }
        }
        return sigAlgs;
    }
    case ed25519.PublicKey pubΔ1: {
        return slice<SignatureScheme>([Ed25519]);
    }
    default: {
        var pubΔ1 = pub;
        return default;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsMissingSignatureˢ = "tls: missing signature_algorithms from TLS 1.2 peer"u8;
internal static readonly @string tlsPeerDoesnTSupportAnyˢ = "tls: peer doesn't support any of the certificate's signature algorithms"u8;

// selectSignatureScheme picks a SignatureScheme from the peer's preference list
// that works with the selected certificate. It's only called for protocol
// versions that support signature algorithms, so TLS 1.2 and 1.3.
internal static (SignatureScheme, error) selectSignatureScheme(uint16 vers, ж<Certificate> Ꮡc, slice<SignatureScheme> peerAlgs) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (priv, ok) = c.PrivateKey._<crypto.Signer>(ᐧ);
    if (!ok) {
        return (0, unsupportedCertificateError(ref (Ꮡc).DerefOrNull()));
    }
    var supportedAlgs = signatureSchemesForPublicKey(vers, priv.Public());
    if (c.SupportedSignatureAlgorithms != nil) {
        supportedAlgs = slices.DeleteFunc(supportedAlgs, (SignatureScheme sigAlg) => !isSupportedSignatureAlgorithm(sigAlg, Ꮡc.Value.SupportedSignatureAlgorithms));
    }
    // Filter out any unsupported signature algorithms, for example due to
    // FIPS 140-3 policy, tlssha1=0, or protocol version.
    supportedAlgs = slices.DeleteFunc(supportedAlgs, (SignatureScheme sigAlg) => isDisabledSignatureAlgorithm(vers, sigAlg, false));
    if (len(supportedAlgs) == 0) {
        return (0, unsupportedCertificateError(ref (Ꮡc).DerefOrNull()));
    }
    if (len(peerAlgs) == 0 && vers == VersionTLS12) {
        // For TLS 1.2, if the client didn't send signature_algorithms then we
        // can assume that it supports SHA1. See RFC 5246, Section 7.4.1.4.1.
        // RFC 9155 made signature_algorithms mandatory in TLS 1.2, and we gated
        // it behind the tlssha1 GODEBUG setting.
        if (tlssha1.Value() != "1"u8) {
            return (0, errors.New(tlsMissingSignatureˢ));
        }
        peerAlgs = slice<SignatureScheme>([PKCS1WithSHA1, ECDSAWithSHA1]);
    }
    // Pick signature scheme in the peer's preference order, as our
    // preference order is not configurable.
    foreach (var (_, preferredAlg) in peerAlgs) {
        if (isSupportedSignatureAlgorithm(preferredAlg, supportedAlgs)) {
            return (preferredAlg, default);
        }
    }
    return (0, errors.New(tlsPeerDoesnTSupportAnyˢ));
}

// unsupportedCertificateError returns a helpful error for certificates with
// an unsupported private key.
internal static error unsupportedCertificateError(ref Certificate cert) {
    switch (cert.PrivateKey.type()) {
    case rsa.PrivateKey _:
    case ecdsa.PrivateKey _: {
        return fmt.Errorf("tls: unsupported certificate: private key is %T, expected *%T"u8,
            cert.PrivateKey, cert.PrivateKey);
    }
    case ж<ed25519.PrivateKey>: {
        return fmt.Errorf("tls: unsupported certificate: private key is *ed25519.PrivateKey, expected ed25519.PrivateKey"u8);
    }}

    var (signer, ok) = cert.PrivateKey._<crypto.Signer>(ᐧ);
    if (!ok) {
        return fmt.Errorf("tls: certificate private key (%T) does not implement crypto.Signer"u8,
            cert.PrivateKey);
    }

    var switchᴛ1 = signer.Public();
    switch (switchᴛ1.type()) {
    case ж<ecdsa.PublicKey> pub: {
        switch (pub.Curve) {
        case var ᴛ when AreEqual(ᴛ, elliptic.P256()): {
            break;
        }
        case var ᴛ when AreEqual(ᴛ, elliptic.P384()): {
            break;
        }
        case var ᴛ when AreEqual(ᴛ, elliptic.P521()): {
            break;
        }
        default: {
            return fmt.Errorf("tls: unsupported certificate curve (%s)"u8, pub.Curve.Params().Name);
        }}
        break;
    }
    case ж<rsa.PublicKey> pub: {
        return fmt.Errorf("tls: certificate RSA key size too small for supported signature algorithms"u8);
    }
    case ed25519.PublicKey pub: {
        break;
    }
    default: {
        var pub = switchᴛ1;
        return fmt.Errorf("tls: unsupported certificate key (%T)"u8, pub);
    }}

    if (cert.SupportedSignatureAlgorithms != nil) {
        return fmt.Errorf("tls: peer doesn't support the certificate custom signature algorithms"u8);
    }

    return fmt.Errorf("tls: internal error: unsupported key (%T)"u8, cert.PrivateKey);
}

} // end tls_package
