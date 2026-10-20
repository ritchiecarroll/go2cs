// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !boringcrypto
namespace go.crypto;

using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using elliptic = go.crypto.elliptic_package;
using rsa = go.crypto.rsa_package;
using Δx509 = go.crypto.x509_package;
using go.crypto;
using math;

partial class tls_package {

// These FIPS 140-3 policies allow anything approved by SP 800-140C
// and SP 800-140D, and tested as part of the Go Cryptographic Module.
//
// Notably, not SHA-1, 3DES, RC4, ChaCha20Poly1305, RSA PKCS #1 v1.5 key
// transport, or TLS 1.0—1.1 (because we don't test its KDF).
//
// These are not default lists, but filters to apply to the default or
// configured lists. Missing items are treated as if they were not implemented.
//
// They are applied when the fips140 GODEBUG is "on" or "only".
internal static slice<uint16> allowedSupportedVersionsFIPS = new uint16[]{
    VersionTLS12,
    VersionTLS13
}.slice();
internal static slice<CurveID> allowedCurvePreferencesFIPS = new CurveID[]{
    X25519MLKEM768,
    SecP256r1MLKEM768,
    SecP384r1MLKEM1024,
    CurveP256,
    CurveP384,
    CurveP521
}.slice();
internal static slice<SignatureScheme> allowedSignatureAlgorithmsFIPS = new SignatureScheme[]{
    PSSWithSHA256,
    ECDSAWithP256AndSHA256,
    Ed25519,
    PSSWithSHA384,
    PSSWithSHA512,
    PKCS1WithSHA256,
    PKCS1WithSHA384,
    PKCS1WithSHA512,
    ECDSAWithP384AndSHA384,
    ECDSAWithP521AndSHA512
}.slice();
internal static slice<uint16> allowedCipherSuitesFIPS = new uint16[]{
    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
    TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
    TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
    TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,
    TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256,
    TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256
}.slice();
internal static slice<uint16> allowedCipherSuitesTLS13FIPS = new uint16[]{
    TLS_AES_128_GCM_SHA256,
    TLS_AES_256_GCM_SHA384
}.slice();

internal static bool isCertificateAllowedFIPS(ref Δx509.Certificate c) {
    switch (c.PublicKey.type()) {
    case ж<rsa.PublicKey> k: {
        return (~k).N.BitLen() >= 2048;
    }
    case ж<ecdsa.PublicKey> k: {
        return AreEqual((~k).Curve, elliptic.P256()) || AreEqual((~k).Curve, elliptic.P384()) || AreEqual((~k).Curve, elliptic.P521());
    }
    case ed25519.PublicKey k: {
        return true;
    }
    default: {
        var k = c.PublicKey;
        return false;
    }}
}

} // end tls_package
