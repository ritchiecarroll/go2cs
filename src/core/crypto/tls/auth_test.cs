// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using crypto = crypto_package;
using fips140tls = go.crypto.tls.@internal.fips140tls_package;
using testing = testing_package;
using go.crypto;
using go.crypto.tls.@internal;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

internal partial struct TestSignatureSelection_tests /*dyn*/ {
    internal ж<global::go.crypto.tls_package.Certificate> cert;
    internal slice<global::go.crypto.tls_package.SignatureScheme> peerSigAlgs;
    internal uint16 tlsVersion;
    internal global::go.crypto.tls_package.SignatureScheme expectedSigAlg;
    internal uint8 expectedSigType;
    internal crypto.Hash expectedHash;
}

internal partial struct TestSignatureSelection_badTests /*dyn*/ {
    internal ж<global::go.crypto.tls_package.Certificate> cert;
    internal slice<global::go.crypto.tls_package.SignatureScheme> peerSigAlgs;
    internal uint16 tlsVersion;
}

public static void TestSignatureSelection(ж<testing.T> Ꮡt) {
    var rsaCert = Ꮡ(new Certificate(
        ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
        PrivateKey: testRSAPrivateKey.OrTypedNil()
    ));
    var pkcs1Cert = Ꮡ(new Certificate(
        ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
        PrivateKey: testRSAPrivateKey.OrTypedNil(),
        SupportedSignatureAlgorithms: new global::go.crypto.tls_package.SignatureScheme[]{PKCS1WithSHA1, PKCS1WithSHA256}.slice()
    ));
    var ecdsaCert = Ꮡ(new Certificate(
        ΔCertificate: new slice<byte>[]{testP256Certificate}.slice(),
        PrivateKey: testP256PrivateKey.OrTypedNil()
    ));
    var ed25519Cert = Ꮡ(new Certificate(
        ΔCertificate: new slice<byte>[]{testEd25519Certificate}.slice(),
        PrivateKey: testEd25519PrivateKey
    ));
    var tests = new TestSignatureSelection_tests[]{
        new(rsaCert, new global::go.crypto.tls_package.SignatureScheme[]{PKCS1WithSHA1, PKCS1WithSHA256}.slice(), VersionTLS12, PKCS1WithSHA1, signaturePKCS1v15, crypto.SHA1),
        new(rsaCert, new global::go.crypto.tls_package.SignatureScheme[]{PKCS1WithSHA512, PKCS1WithSHA1}.slice(), VersionTLS12, PKCS1WithSHA512, signaturePKCS1v15, crypto.SHA512),
        new(rsaCert, new global::go.crypto.tls_package.SignatureScheme[]{PSSWithSHA256, PKCS1WithSHA256}.slice(), VersionTLS12, PSSWithSHA256, signatureRSAPSS, crypto.SHA256),
        new(pkcs1Cert, new global::go.crypto.tls_package.SignatureScheme[]{PSSWithSHA256, PKCS1WithSHA256}.slice(), VersionTLS12, PKCS1WithSHA256, signaturePKCS1v15, crypto.SHA256),
        new(rsaCert, new global::go.crypto.tls_package.SignatureScheme[]{PSSWithSHA384, PKCS1WithSHA1}.slice(), VersionTLS13, PSSWithSHA384, signatureRSAPSS, crypto.SHA384),
        new(ecdsaCert, new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithSHA1}.slice(), VersionTLS12, ECDSAWithSHA1, signatureECDSA, crypto.SHA1),
        new(ecdsaCert, new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP256AndSHA256}.slice(), VersionTLS12, ECDSAWithP256AndSHA256, signatureECDSA, crypto.SHA256),
        new(ecdsaCert, new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP256AndSHA256}.slice(), VersionTLS13, ECDSAWithP256AndSHA256, signatureECDSA, crypto.SHA256),
        new(ed25519Cert, new global::go.crypto.tls_package.SignatureScheme[]{Ed25519}.slice(), VersionTLS12, Ed25519, signatureEd25519, directSigning),
        new(ed25519Cert, new global::go.crypto.tls_package.SignatureScheme[]{Ed25519}.slice(), VersionTLS13, Ed25519, signatureEd25519, directSigning), // TLS 1.2 without signature_algorithms extension

        new(rsaCert, default!, VersionTLS12, PKCS1WithSHA1, signaturePKCS1v15, crypto.SHA1),
        new(ecdsaCert, default!, VersionTLS12, ECDSAWithSHA1, signatureECDSA, crypto.SHA1), // TLS 1.2 does not restrict the ECDSA curve (our ecdsaCert is P-256)

        new(ecdsaCert, new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP384AndSHA384}.slice(), VersionTLS12, ECDSAWithP384AndSHA384, signatureECDSA, crypto.SHA384)
    }.slice();
    foreach (var (testNo, test) in tests) {
        if (fips140tls.Required() && (test.expectedHash == crypto.SHA1 || test.expectedSigAlg == Ed25519)) {
            Ꮡt.Logf("skipping test[%d] - not compatible with TLS FIPS mode"u8, testNo);
            continue;
        }
        var (sigAlg, err) = selectSignatureScheme(test.tlsVersion, ref (test.cert).DerefOrNull(), test.peerSigAlgs);
        if (err != default!) {
            Ꮡt.Errorf("test[%d]: unexpected selectSignatureScheme error: %v"u8, testNo, err);
        }
        if (test.expectedSigAlg != sigAlg) {
            Ꮡt.Errorf("test[%d]: expected signature scheme %v, got %v"u8, testNo, test.expectedSigAlg, sigAlg);
        }
        (var sigType, var hashFunc, err) = typeAndHashFromSignatureScheme(sigAlg);
        if (err != default!) {
            Ꮡt.Errorf("test[%d]: unexpected typeAndHashFromSignatureScheme error: %v"u8, testNo, err);
        }
        if (test.expectedSigType != sigType) {
            Ꮡt.Errorf("test[%d]: expected signature algorithm %#x, got %#x"u8, testNo, test.expectedSigType, sigType);
        }
        if (test.expectedHash != hashFunc) {
            Ꮡt.Errorf("test[%d]: expected hash function %#x, got %#x"u8, testNo, test.expectedHash, hashFunc);
        }
    }
    var brokenCert = Ꮡ(new Certificate(
        ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
        PrivateKey: testRSAPrivateKey.OrTypedNil(),
        SupportedSignatureAlgorithms: new global::go.crypto.tls_package.SignatureScheme[]{Ed25519}.slice()
    ));
    var badTests = new TestSignatureSelection_badTests[]{
        new(rsaCert, new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP256AndSHA256, ECDSAWithSHA1}.slice(), VersionTLS12),
        new(ecdsaCert, new global::go.crypto.tls_package.SignatureScheme[]{PKCS1WithSHA256, PKCS1WithSHA1}.slice(), VersionTLS12),
        new(rsaCert, new global::go.crypto.tls_package.SignatureScheme[]{0}.slice(), VersionTLS12),
        new(ed25519Cert, new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP256AndSHA256, ECDSAWithSHA1}.slice(), VersionTLS12),
        new(ecdsaCert, new global::go.crypto.tls_package.SignatureScheme[]{Ed25519}.slice(), VersionTLS12),
        new(brokenCert, new global::go.crypto.tls_package.SignatureScheme[]{Ed25519}.slice(), VersionTLS12),
        new(brokenCert, new global::go.crypto.tls_package.SignatureScheme[]{PKCS1WithSHA256}.slice(), VersionTLS12), // RFC 5246, Section 7.4.1.4.1, says to only consider {sha1,ecdsa} as
 // default when the extension is missing, and RFC 8422 does not update
 // it. Anyway, if a stack supports Ed25519 it better support sigalgs.

        new(ed25519Cert, default!, VersionTLS12), // TLS 1.3 has no default signature_algorithms.

        new(rsaCert, default!, VersionTLS13),
        new(ecdsaCert, default!, VersionTLS13),
        new(ed25519Cert, default!, VersionTLS13), // Wrong curve, which TLS 1.3 checks

        new(ecdsaCert, new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP384AndSHA384}.slice(), VersionTLS13), // TLS 1.3 does not support PKCS1v1.5 or SHA-1.

        new(rsaCert, new global::go.crypto.tls_package.SignatureScheme[]{PKCS1WithSHA256}.slice(), VersionTLS13),
        new(pkcs1Cert, new global::go.crypto.tls_package.SignatureScheme[]{PSSWithSHA256, PKCS1WithSHA256}.slice(), VersionTLS13),
        new(ecdsaCert, new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithSHA1}.slice(), VersionTLS13), // The key can be too small for the hash.

        new(rsaCert, new global::go.crypto.tls_package.SignatureScheme[]{PSSWithSHA512}.slice(), VersionTLS12)
    }.slice();
    foreach (var (testNo, test) in badTests) {
        var (sigAlg, err) = selectSignatureScheme(test.tlsVersion, ref (test.cert).DerefOrNull(), test.peerSigAlgs);
        if (err == default!) {
            Ꮡt.Errorf("test[%d]: unexpected success, got %v"u8, testNo, sigAlg);
        }
    }
}

public static void TestLegacyTypeAndHash(ж<testing.T> Ꮡt) {
    var (sigType, hashFunc, err) = legacyTypeAndHashFromPublicKey(testRSAPrivateKey.Public());
    if (err != default!) {
        Ꮡt.Errorf("RSA: unexpected error: %v"u8, err);
    }
    {
        var expectedSigType = signaturePKCS1v15; if (expectedSigType != sigType) {
            Ꮡt.Errorf("RSA: expected signature type %#x, got %#x"u8, expectedSigType, sigType);
        }
    }
    {
        crypto.Hash expectedHashFunc = crypto.MD5SHA1; if (expectedHashFunc != hashFunc) {
            Ꮡt.Errorf("RSA: expected hash %#x, got %#x"u8, expectedHashFunc, hashFunc);
        }
    }
    (sigType, hashFunc, err) = legacyTypeAndHashFromPublicKey(testECDSAPrivateKey.Public());
    if (err != default!) {
        Ꮡt.Errorf("ECDSA: unexpected error: %v"u8, err);
    }
    {
        var expectedSigType = signatureECDSA; if (expectedSigType != sigType) {
            Ꮡt.Errorf("ECDSA: expected signature type %#x, got %#x"u8, expectedSigType, sigType);
        }
    }
    {
        crypto.Hash expectedHashFunc = crypto.SHA1; if (expectedHashFunc != hashFunc) {
            Ꮡt.Errorf("ECDSA: expected hash %#x, got %#x"u8, expectedHashFunc, hashFunc);
        }
    }
    // Ed25519 is not supported by TLS 1.0 and 1.1.
    (_, _, err) = legacyTypeAndHashFromPublicKey(testEd25519PrivateKey.Public());
    if (err == default!) {
        Ꮡt.Errorf("Ed25519: unexpected success"u8);
    }
}

// TestSupportedSignatureAlgorithms checks that all supportedSignatureAlgorithms
// have valid type and hash information.
public static void TestSupportedSignatureAlgorithms(ж<testing.T> Ꮡt) {
    foreach (var (_, sigAlg) in supportedSignatureAlgorithms()) {
        var (sigType, hash, err) = typeAndHashFromSignatureScheme(sigAlg);
        if (err != default!) {
            Ꮡt.Errorf("%v: unexpected error: %v"u8, sigAlg, err);
        }
        if (sigType == 0) {
            Ꮡt.Errorf("%v: missing signature type"u8, sigAlg);
        }
        if (hash == 0 && sigAlg != Ed25519) {
            Ꮡt.Errorf("%v: missing hash"u8, sigAlg);
        }
    }
}

} // end tls_internal_test_package
