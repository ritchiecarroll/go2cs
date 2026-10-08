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
using fips140tls = go.crypto.tls.@internal.fips140tls_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using io = io_package;
using go.crypto;
using go.crypto.tls.@internal;

partial class tls_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdsaVerificationFailureˢ = "ECDSA verification failure"u8;
internal static readonly @string ed25519Verificationˢ = "Ed25519 verification failure"u8;
internal static readonly @string internalErrorUnknownˢ = "internal error: unknown signature type"u8;

// verifyHandshakeSignature verifies a signature against pre-hashed
// (if required) handshake contents.
internal static error verifyHandshakeSignature(uint8 sigType, cryptoꓸPublicKey pubkey, crypto.Hash hashFunc, slice<byte> signed, slice<byte> sig) {
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
            var err = rsa.VerifyPKCS1v15(pubKey, hashFunc, signed, sig); if (err != default!) {
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
            var err = rsa.VerifyPSS(pubKey, hashFunc, signed, sig, signOpts); if (err != default!) {
                return err;
            }
        }
        break;
    }
    default: {
        return errors.New(internalErrorUnknownˢ);
    }}

    return default!;
}

internal static readonly @string serverSignatureContext = "TLS 1.3, server CertificateVerify\x00"u8;
internal static readonly @string clientSignatureContext = "TLS 1.3, client CertificateVerify\x00"u8;

internal static slice<byte> signaturePadding = new byte[]{
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
    0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
}.slice();

// signedMessage returns the pre-hashed (if necessary) message to be signed by
// certificate keys in TLS 1.3. See RFC 8446, Section 4.4.3.
internal static slice<byte> signedMessage(crypto.Hash sigHash, @string context, hash.Hash transcript) {
    if (sigHash == directSigning) {
        var b = Ꮡ(new bytes.Buffer(nil));
        b.Write(signaturePadding);
        io.WriteString(new bytes_BufferжWriter(b), context);
        b.Write(transcript.Sum(default!));
        return b.Bytes();
    }
    var h = sigHash.New();
    h.Write(signaturePadding);
    io.WriteString(h, context);
    h.Write(transcript.Sum(default!));
    return h.Sum(default!);
}

// typeAndHashFromSignatureScheme returns the corresponding signature type and
// crypto.Hash for a given TLS SignatureScheme.
internal static (uint8 sigType, crypto.Hash hash, error err) typeAndHashFromSignatureScheme(SignatureScheme signatureAlgorithm) {
    uint8 sigType = default!;
    crypto.Hash hash = default!;

    var exprᴛ1 = signatureAlgorithm;
    if (exprᴛ1 == PKCS1WithSHA1 || exprᴛ1 == PKCS1WithSHA256 || exprᴛ1 == PKCS1WithSHA384 || exprᴛ1 == PKCS1WithSHA512) {
        sigType = signaturePKCS1v15;
    }
    else if (exprᴛ1 == PSSWithSHA256 || exprᴛ1 == PSSWithSHA384 || exprᴛ1 == PSSWithSHA512) {
        sigType = signatureRSAPSS;
    }
    else if (exprᴛ1 == ECDSAWithSHA1 || exprᴛ1 == ECDSAWithP256AndSHA256 || exprᴛ1 == ECDSAWithP384AndSHA384 || exprᴛ1 == ECDSAWithP521AndSHA512) {
        sigType = signatureECDSA;
    }
    else if (exprᴛ1 == Ed25519) {
        sigType = signatureEd25519;
    }
    else { /* default: */
        return (0, 0, fmt.Errorf("unsupported signature algorithm: %v"u8, signatureAlgorithm));
    }

    var exprᴛ2 = signatureAlgorithm;
    if (exprᴛ2 == PKCS1WithSHA1 || exprᴛ2 == ECDSAWithSHA1) {
        hash = crypto.SHA1;
    }
    else if (exprᴛ2 == PKCS1WithSHA256 || exprᴛ2 == PSSWithSHA256 || exprᴛ2 == ECDSAWithP256AndSHA256) {
        hash = crypto.SHA256;
    }
    else if (exprᴛ2 == PKCS1WithSHA384 || exprᴛ2 == PSSWithSHA384 || exprᴛ2 == ECDSAWithP384AndSHA384) {
        hash = crypto.SHA384;
    }
    else if (exprᴛ2 == PKCS1WithSHA512 || exprᴛ2 == PSSWithSHA512 || exprᴛ2 == ECDSAWithP521AndSHA512) {
        hash = crypto.SHA512;
    }
    else if (exprᴛ2 == Ed25519) {
        hash = directSigning;
    }
    else { /* default: */
        return (0, 0, fmt.Errorf("unsupported signature algorithm: %v"u8, signatureAlgorithm));
    }

    return (sigType, hash, default!);
}

// legacyTypeAndHashFromPublicKey returns the fixed signature type and crypto.Hash for
// a given public key used with TLS 1.0 and 1.1, before the introduction of
// signature algorithm negotiation.
internal static (uint8 sigType, crypto.Hash hash, error err) legacyTypeAndHashFromPublicKey(cryptoꓸPublicKey pub) {
    switch (pub.type()) {
    case ж<rsa.PublicKey>: {
        return (signaturePKCS1v15, crypto.MD5SHA1, default!);
    }
    case ж<ecdsa.PublicKey>: {
        return (signatureECDSA, crypto.SHA1, default!);
    }
    case ed25519.PublicKey: {
        return (0, 0, fmt.Errorf("tls: Ed25519 public keys are not supported before TLS 1.2"u8));
    }
    default: {
        return (0, 0, fmt.Errorf("tls: unsupported public key: %T"u8, // RFC 8422 specifies support for Ed25519 in TLS 1.0 and 1.1,
 // but it requires holding on to a handshake transcript to do a
 // full signature, and not even OpenSSL bothers with the
 // complexity, so we can't even test it properly.
 pub));
    }}

}

// RSA-PSS is used with PSSSaltLengthEqualsHash, and requires
//    emLen >= hLen + sLen + 2
// PKCS #1 v1.5 uses prefixes from hashPrefixes in crypto/rsa, and requires
//    emLen >= len(prefix) + hLen + 11
// TLS 1.3 dropped support for PKCS #1 v1.5 in favor of RSA-PSS.

partial struct rsaSignatureSchemesᴛ1 /*dyn*/ {
    internal SignatureScheme scheme;
    internal nint minModulusBytes;
    internal uint16 maxVersion;
}
internal static slice<rsaSignatureSchemesᴛ1> rsaSignatureSchemes = new rsaSignatureSchemesᴛ1[]{
    new(PSSWithSHA256, crypto.SHA256.Size() * 2 + 2, VersionTLS13),
    new(PSSWithSHA384, crypto.SHA384.Size() * 2 + 2, VersionTLS13),
    new(PSSWithSHA512, crypto.SHA512.Size() * 2 + 2, VersionTLS13),
    new(PKCS1WithSHA256, 19 + crypto.SHA256.Size() + 11, VersionTLS12),
    new(PKCS1WithSHA384, 19 + crypto.SHA384.Size() + 11, VersionTLS12),
    new(PKCS1WithSHA512, 19 + crypto.SHA512.Size() + 11, VersionTLS12),
    new(PKCS1WithSHA1, 15 + crypto.SHA1.Size() + 11, VersionTLS12)
}.slice();

// signatureSchemesForCertificate returns the list of supported SignatureSchemes
// for a given certificate, based on the public key and the protocol version,
// and optionally filtered by its explicit SupportedSignatureAlgorithms.
//
// This function must be kept in sync with supportedSignatureAlgorithms.
// FIPS filtering is applied in the caller, selectSignatureScheme.
internal static slice<SignatureScheme> signatureSchemesForCertificate(uint16 version, ref Certificate cert) {
    var (priv, ok) = cert.PrivateKey._<crypto.Signer>(ᐧ);
    if (!ok) {
        return default!;
    }
    slice<SignatureScheme> sigAlgs = default!;
    var switchᴛ1 = priv.Public();
    switch (switchᴛ1.type()) {
    case ж<ecdsa.PublicKey> pub: {
        if (version != VersionTLS13) {
            // In TLS 1.2 and earlier, ECDSA algorithms are not
            // constrained to a single curve.
            sigAlgs = new SignatureScheme[]{
                ECDSAWithP256AndSHA256,
                ECDSAWithP384AndSHA384,
                ECDSAWithP521AndSHA512,
                ECDSAWithSHA1
            }.slice();
            break;
        }
        var exprᴛ1 = (~pub).Curve;
        if (AreEqual(exprᴛ1, elliptic.P256())) {
            sigAlgs = new SignatureScheme[]{ECDSAWithP256AndSHA256}.slice();
        }
        else if (AreEqual(exprᴛ1, elliptic.P384())) {
            sigAlgs = new SignatureScheme[]{ECDSAWithP384AndSHA384}.slice();
        }
        else if (AreEqual(exprᴛ1, elliptic.P521())) {
            sigAlgs = new SignatureScheme[]{ECDSAWithP521AndSHA512}.slice();
        }
        else { /* default: */
            return default!;
        }

        break;
    }
    case ж<rsa.PublicKey> pub: {
        nint size = pub.Size();
        sigAlgs = new slice<SignatureScheme>(0, len(rsaSignatureSchemes));
        foreach (var (_, candidate) in rsaSignatureSchemes) {
            if (size >= candidate.minModulusBytes && version <= candidate.maxVersion) {
                sigAlgs = append(sigAlgs, candidate.scheme);
            }
        }
        break;
    }
    case ed25519.PublicKey pub: {
        sigAlgs = new SignatureScheme[]{Ed25519}.slice();
        break;
    }
    default: {
        var pub = switchᴛ1;
        return default!;
    }}
    if (cert.SupportedSignatureAlgorithms != default!) {
        slice<SignatureScheme> filteredSigAlgs = default!;
        foreach (var (_, sigAlg) in sigAlgs) {
            if (isSupportedSignatureAlgorithm(sigAlg, cert.SupportedSignatureAlgorithms)) {
                filteredSigAlgs = append(filteredSigAlgs, sigAlg);
            }
        }
        return filteredSigAlgs;
    }
    return sigAlgs;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsPeerDoesnTSupportAnyˢ = "tls: peer doesn't support any of the certificate's signature algorithms"u8;

// selectSignatureScheme picks a SignatureScheme from the peer's preference list
// that works with the selected certificate. It's only called for protocol
// versions that support signature algorithms, so TLS 1.2 and 1.3.
internal static (SignatureScheme, error) selectSignatureScheme(uint16 vers, ref Certificate c, slice<SignatureScheme> peerAlgs) {
    var supportedAlgs = signatureSchemesForCertificate(vers, ref c);
    if (len(supportedAlgs) == 0) {
        return (0, unsupportedCertificateError(ref c));
    }
    if (len(peerAlgs) == 0 && vers == VersionTLS12) {
        // For TLS 1.2, if the client didn't send signature_algorithms then we
        // can assume that it supports SHA1. See RFC 5246, Section 7.4.1.4.1.
        peerAlgs = new SignatureScheme[]{PKCS1WithSHA1, ECDSAWithSHA1}.slice();
    }
    // Pick signature scheme in the peer's preference order, as our
    // preference order is not configurable.
    foreach (var (_, preferredAlg) in peerAlgs) {
        if (fips140tls.Required() && !isSupportedSignatureAlgorithm(preferredAlg, defaultSupportedSignatureAlgorithmsFIPS)) {
            continue;
        }
        if (isSupportedSignatureAlgorithm(preferredAlg, supportedAlgs)) {
            return (preferredAlg, default!);
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
    var switchᴛ2 = signer.Public();
    switch (switchᴛ2.type()) {
    case ж<ecdsa.PublicKey> pub: {
        var exprᴛ1 = (~pub).Curve;
        if (AreEqual(exprᴛ1, elliptic.P256())) {
        }
        else if (AreEqual(exprᴛ1, elliptic.P384())) {
        }
        else if (AreEqual(exprᴛ1, elliptic.P521())) {
        }
        else { /* default: */
            return fmt.Errorf("tls: unsupported certificate curve (%s)"u8, (~(~pub).Curve.Params()).Name);
        }

        break;
    }
    case ж<rsa.PublicKey> pub: {
        return fmt.Errorf("tls: certificate RSA key size too small for supported signature algorithms"u8);
    }
    case ed25519.PublicKey pub: {
        break;
    }
    default: {
        var pub = switchᴛ2;
        return fmt.Errorf("tls: unsupported certificate key (%T)"u8, pub);
    }}
    if (cert.SupportedSignatureAlgorithms != default!) {
        return fmt.Errorf("tls: peer doesn't support the certificate custom signature algorithms"u8);
    }
    return fmt.Errorf("tls: internal error: unsupported key (%T)"u8, cert.PrivateKey);
}

} // end tls_package
