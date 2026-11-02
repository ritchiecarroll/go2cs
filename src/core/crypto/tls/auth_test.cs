// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using crypto = crypto_package;
using fips140tls = go.crypto.tls.@internal.fips140tls_package;
using os = os_package;
using testing = testing_package;
using go.crypto;
using go.crypto.tls.@internal;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;

internal partial struct TestSignatureSelection_tests /*dyn*/ {
    internal ж<global::go.crypto.tls_package.Certificate> cert;
    internal slice<global::go.crypto.tls_package.SignatureScheme> peerSigAlgs;
    internal uint16 tlsVersion;
    internal @string godebug;
    internal global::go.crypto.tls_package.SignatureScheme expectedSigAlg;
    internal uint8 expectedSigType;
    internal crypto.Hash expectedHash;
}

internal partial struct TestSignatureSelection_badTests /*dyn*/ {
    internal ж<global::go.crypto.tls_package.Certificate> cert;
    internal slice<global::go.crypto.tls_package.SignatureScheme> peerSigAlgs;
    internal uint16 tlsVersion;
}

public static void TestSignatureSelection(ж<testing.T> t) {
    var rsaCert = Ꮡ(new Certificate(
        ΔCertificate: slice<slice<byte>>([testRSACertificate]),
        PrivateKey: testRSAPrivateKey.OrTypedNil()
    ));
    var pkcs1Cert = Ꮡ(new Certificate(
        ΔCertificate: slice<slice<byte>>([testRSACertificate]),
        PrivateKey: testRSAPrivateKey.OrTypedNil(),
        SupportedSignatureAlgorithms: slice<global::go.crypto.tls_package.SignatureScheme>([PKCS1WithSHA1, PKCS1WithSHA256])
    ));
    var ecdsaCert = Ꮡ(new Certificate(
        ΔCertificate: slice<slice<byte>>([testP256Certificate]),
        PrivateKey: testP256PrivateKey.OrTypedNil()
    ));
    var ed25519Cert = Ꮡ(new Certificate(
        ΔCertificate: slice<slice<byte>>([testEd25519Certificate]),
        PrivateKey: testEd25519PrivateKey
    ));

    var tests = slice<TestSignatureSelection_tests>([
        new(rsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([PKCS1WithSHA1, PKCS1WithSHA256]), VersionTLS12, "", PKCS1WithSHA256, signaturePKCS1v15, crypto.SHA256),
        new(rsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([PKCS1WithSHA1, PKCS1WithSHA256]), VersionTLS12, "tlssha1=1", PKCS1WithSHA1, signaturePKCS1v15, crypto.SHA1),
        new(rsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([PKCS1WithSHA512, PKCS1WithSHA1]), VersionTLS12, "", PKCS1WithSHA512, signaturePKCS1v15, crypto.SHA512),
        new(rsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([PSSWithSHA256, PKCS1WithSHA256]), VersionTLS12, "", PSSWithSHA256, signatureRSAPSS, crypto.SHA256),
        new(pkcs1Cert, slice<global::go.crypto.tls_package.SignatureScheme>([PSSWithSHA256, PKCS1WithSHA256]), VersionTLS12, "", PKCS1WithSHA256, signaturePKCS1v15, crypto.SHA256),
        new(rsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([PSSWithSHA384, PKCS1WithSHA1]), VersionTLS13, "", PSSWithSHA384, signatureRSAPSS, crypto.SHA384),
        new(rsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([PKCS1WithSHA1, PSSWithSHA384]), VersionTLS13, "", PSSWithSHA384, signatureRSAPSS, crypto.SHA384),
        new(ecdsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([ECDSAWithSHA1, ECDSAWithP256AndSHA256]), VersionTLS12, "", ECDSAWithP256AndSHA256, signatureECDSA, crypto.SHA256),
        new(ecdsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([ECDSAWithSHA1]), VersionTLS12, "tlssha1=1", ECDSAWithSHA1, signatureECDSA, crypto.SHA1),
        new(ecdsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([ECDSAWithP256AndSHA256]), VersionTLS12, "", ECDSAWithP256AndSHA256, signatureECDSA, crypto.SHA256),
        new(ecdsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([ECDSAWithP256AndSHA256]), VersionTLS13, "", ECDSAWithP256AndSHA256, signatureECDSA, crypto.SHA256),
        new(ed25519Cert, slice<global::go.crypto.tls_package.SignatureScheme>([Ed25519]), VersionTLS12, "", Ed25519, signatureEd25519, directSigning),
        new(ed25519Cert, slice<global::go.crypto.tls_package.SignatureScheme>([Ed25519]), VersionTLS13, "", Ed25519, signatureEd25519, directSigning),

        // TLS 1.2 without signature_algorithms extension
        new(rsaCert, default, VersionTLS12, "tlssha1=1", PKCS1WithSHA1, signaturePKCS1v15, crypto.SHA1),
        new(ecdsaCert, default, VersionTLS12, "tlssha1=1", ECDSAWithSHA1, signatureECDSA, crypto.SHA1),

        // TLS 1.2 does not restrict the ECDSA curve (our ecdsaCert is P-256)
        new(ecdsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([ECDSAWithP384AndSHA384]), VersionTLS12, "", ECDSAWithP384AndSHA384, signatureECDSA, crypto.SHA384)
    ]);

    foreach (var (testNo, test) in tests) {
        if (fips140tls.Required() && test.expectedHash == crypto.SHA1) {
            t.Logf("skipping test[%d] - not compatible with TLS FIPS mode", testNo);
            continue;
        }
        @string savedGODEBUG = os.Getenv(godebugˢ);
        os.Setenv(godebugˢ, savedGODEBUG + ","u8 + test.godebug);

        var (sigAlg, err) = selectSignatureScheme(test.tlsVersion, test.cert, test.peerSigAlgs);
        if (err != nil) {
            t.Errorf("test[%d]: unexpected selectSignatureScheme error: %v", testNo, err);
        }
        if (test.expectedSigAlg != sigAlg) {
            t.Errorf("test[%d]: expected signature scheme %v, got %v", testNo, test.expectedSigAlg, sigAlg);
        }
        (var sigType, var hashFunc, err) = typeAndHashFromSignatureScheme(sigAlg);
        if (err != nil) {
            t.Errorf("test[%d]: unexpected typeAndHashFromSignatureScheme error: %v", testNo, err);
        }
        if (test.expectedSigType != sigType) {
            t.Errorf("test[%d]: expected signature algorithm %#x, got %#x", testNo, test.expectedSigType, sigType);
        }
        if (test.expectedHash != hashFunc) {
            t.Errorf("test[%d]: expected hash function %#x, got %#x", testNo, test.expectedHash, hashFunc);
        }

        os.Setenv(godebugˢ, savedGODEBUG);
    }

    var brokenCert = Ꮡ(new Certificate(
        ΔCertificate: slice<slice<byte>>([testRSACertificate]),
        PrivateKey: testRSAPrivateKey.OrTypedNil(),
        SupportedSignatureAlgorithms: slice<global::go.crypto.tls_package.SignatureScheme>([Ed25519])
    ));

    var badTests = slice<TestSignatureSelection_badTests>([
        new(rsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([ECDSAWithP256AndSHA256, ECDSAWithSHA1]), VersionTLS12),
        new(ecdsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([PKCS1WithSHA256, PKCS1WithSHA1]), VersionTLS12),
        new(rsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([0]), VersionTLS12),
        new(ed25519Cert, slice<global::go.crypto.tls_package.SignatureScheme>([ECDSAWithP256AndSHA256, ECDSAWithSHA1]), VersionTLS12),
        new(ecdsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([Ed25519]), VersionTLS12),
        new(brokenCert, slice<global::go.crypto.tls_package.SignatureScheme>([Ed25519]), VersionTLS12),
        new(brokenCert, slice<global::go.crypto.tls_package.SignatureScheme>([PKCS1WithSHA256]), VersionTLS12),
        // RFC 5246, Section 7.4.1.4.1, says to only consider {sha1,ecdsa} as
        // default when the extension is missing, and RFC 8422 does not update
        // it. Anyway, if a stack supports Ed25519 it better support sigalgs.
        new(ed25519Cert, default, VersionTLS12),
        // TLS 1.3 has no default signature_algorithms.
        new(rsaCert, default, VersionTLS13),
        new(ecdsaCert, default, VersionTLS13),
        new(ed25519Cert, default, VersionTLS13),
        // Wrong curve, which TLS 1.3 checks
        new(ecdsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([ECDSAWithP384AndSHA384]), VersionTLS13),
        // TLS 1.3 does not support PKCS1v1.5 or SHA-1.
        new(rsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([PKCS1WithSHA256]), VersionTLS13),
        new(pkcs1Cert, slice<global::go.crypto.tls_package.SignatureScheme>([PSSWithSHA256, PKCS1WithSHA256]), VersionTLS13),
        new(ecdsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([ECDSAWithSHA1]), VersionTLS13),
        // The key can be too small for the hash.
        new(rsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([PSSWithSHA512]), VersionTLS12),
        // SHA-1 requires tlssha1=1
        new(rsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([PKCS1WithSHA1]), VersionTLS12),
        new(ecdsaCert, slice<global::go.crypto.tls_package.SignatureScheme>([ECDSAWithSHA1]), VersionTLS12),
        new(rsaCert, default, VersionTLS12),
        new(ecdsaCert, default, VersionTLS12)
    ]);

    foreach (var (testNo, test) in badTests) {
        var (sigAlg, err) = selectSignatureScheme(test.tlsVersion, test.cert, test.peerSigAlgs);
        if (err == nil) {
            t.Errorf("test[%d]: unexpected success, got %v", testNo, sigAlg);
        }
    }
}

public static void TestLegacyTypeAndHash(ж<testing.T> t) {
    var (sigType, hashFunc, err) = legacyTypeAndHashFromPublicKey(testRSAPrivateKey.Public());
    if (err != nil) {
        t.Errorf("RSA: unexpected error: %v", err);
    }
    {
        var expectedSigType = signaturePKCS1v15; if (expectedSigType != sigType) {
            t.Errorf("RSA: expected signature type %#x, got %#x", expectedSigType, sigType);
        }
    }
    {
        crypto.Hash expectedHashFunc = crypto.MD5SHA1; if (expectedHashFunc != hashFunc) {
            t.Errorf("RSA: expected hash %#x, got %#x", expectedHashFunc, hashFunc);
        }
    }

    (sigType, hashFunc, err) = legacyTypeAndHashFromPublicKey(testECDSAPrivateKey.Public());
    if (err != nil) {
        t.Errorf("ECDSA: unexpected error: %v", err);
    }
    {
        var expectedSigType = signatureECDSA; if (expectedSigType != sigType) {
            t.Errorf("ECDSA: expected signature type %#x, got %#x", expectedSigType, sigType);
        }
    }
    {
        crypto.Hash expectedHashFunc = crypto.SHA1; if (expectedHashFunc != hashFunc) {
            t.Errorf("ECDSA: expected hash %#x, got %#x", expectedHashFunc, hashFunc);
        }
    }

    // Ed25519 is not supported by TLS 1.0 and 1.1.
    (_, _, err) = legacyTypeAndHashFromPublicKey(testEd25519PrivateKey.Public());
    if (err == nil) {
        t.Errorf("Ed25519: unexpected success");
    }
}

// TestSupportedSignatureAlgorithms checks that all supportedSignatureAlgorithms
// have valid type and hash information.
public static void TestSupportedSignatureAlgorithms(ж<testing.T> t) {
    foreach (var (_, sigAlg) in supportedSignatureAlgorithms(VersionTLS12)) {
        var (sigType, hash, err) = typeAndHashFromSignatureScheme(sigAlg);
        if (err != nil) {
            t.Errorf("%v: unexpected error: %v", sigAlg, err);
        }
        if (sigType == 0) {
            t.Errorf("%v: missing signature type", sigAlg);
        }
        if (hash == 0 && sigAlg != Ed25519) {
            t.Errorf("%v: missing hash", sigAlg);
        }
    }
}

} // end tls_internal_test_package
