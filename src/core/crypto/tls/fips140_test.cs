// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using fips140 = go.crypto.fips140_package;
using boring = go.crypto.@internal.boring_package;
using ifips140 = go.crypto.@internal.fips140_package;
using rand = go.crypto.rand_package;
using rsa = go.crypto.rsa_package;
using Δx509 = go.crypto.x509_package;
using pkix = go.crypto.x509.pkix_package;
using pem = encoding.pem_package;
using fmt = fmt_package;
using obscuretestdata = go.@internal.obscuretestdata_package;
using testenv = go.@internal.testenv_package;
using big = go.math.big_package;
using net = net_package;
using os = os_package;
using regexp = regexp_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using crypto = crypto_package;
using encoding;
using exec = go.os.exec_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.crypto.x509;
using go.math;
using go.os;
using io = io_package;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

internal static slice<uint16> allCipherSuitesIncludingTLS13() {
    var s = allCipherSuites();
    foreach (var (_, suite) in cipherSuitesTLS13) {
        s = append(s, suite.id);
    }
    return s;
}

internal static bool isTLS13CipherSuite(uint16 id) {
    foreach (var (_, suite) in cipherSuitesTLS13) {
        if (id == suite.id) {
            return true;
        }
    }
    return false;
}

internal static global::go.crypto.tls_package.keyShare generateKeyShare(global::go.crypto.tls_package.CurveID group) {
    var (ke, err) = keyExchangeForCurveID(group);
    if (err != nil) {
        throw panic(err);
    }
    (_, var shares, err) = ke.keyShares(go.crypto.rand_package.Reader);
    if (err != nil) {
        throw panic(err);
    }
    return shares[0];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testVˢ => ref testVˢᵇ;
private static readonly @string testVˢᵇ = "-test.v"u8;

internal static void rerunWithFIPS140Enforced(ж<testing.T> t) {
    t.Helper();
    {
        var errΔ1 = ifips140.Supported(); if (errΔ1 != nil) {
            t.Skipf("test requires FIPS 140 mode: %v", errΔ1);
        }
    }
    @string nameRegex = "^"u8 + regexp.QuoteMeta(t.Name()) + "$"u8;
    var cmd = testenv.Command(new tls_test_package.testing_TжTB(t), testenv.Executable(new tls_test_package.testing_TжTB(t)), "-test.run="u8 + nameRegex, testVˢ);
    cmd.Env = append(cmd.Environ(), (@string)"GODEBUG=fips140=only");
    var (@out, err) = cmd.CombinedOutput();
    t.Logf("running with GODEBUG=fips140=only:\n%s", @out);
    if (err != nil) {
        t.Errorf("fips140=only subprocess failed: %v", err);
    }
}

internal static ж<global::go.crypto.tls_package.Config> testConfigFIPS140;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string versionTLS10ˢ => ref versionTLS10ˢᵇ;
private static readonly @string versionTLS10ˢᵇ = "VersionTLS10"u8;
internal static ref readonly @string versionTLS11ˢ => ref versionTLS11ˢᵇ;
private static readonly @string versionTLS11ˢᵇ = "VersionTLS11"u8;
internal static ref readonly @string versionTLS12ˢ => ref versionTLS12ˢᵇ;
private static readonly @string versionTLS12ˢᵇ = "VersionTLS12"u8;
internal static ref readonly @string versionTLS13ˢ => ref versionTLS13ˢᵇ;
private static readonly @string versionTLS13ˢᵇ = "VersionTLS13"u8;
internal static ref readonly @string supportedVersionsˢ => ref supportedVersionsˢᵇ;
private static readonly @string supportedVersionsˢᵇ = "supported versions"u8;

public static void TestFIPSServerProtocolVersion(ж<testing.T> t) {
    void test(ж<testing.T> tΔ1, @string name, uint16 v, @string msg) {
        tΔ1.Run(name, (ж<testing.T> tΔ2) => {
            var serverConfig = testConfigFIPS140.Clone();
            serverConfig.MinVersion = VersionSSL30;
            var clientConfig = testConfigFIPS140.Clone();
            clientConfig.MinVersion = v;
            clientConfig.MaxVersion = v;
            var (_, _, err) = testHandshake(tΔ2, clientConfig, serverConfig);
            if (msg == ""u8) {
                if (err != nil) {
                    tΔ2.Fatalf("got error: %v, expected success", err);
                }
            } else {
                if (err == nil) {
                    tΔ2.Fatalf("got success, expected error");
                }
                if (!strings.Contains(err.Error(), msg)) {
                    tΔ2.Fatalf("got error %v, expected %q", err, msg);
                }
            }
        });
    }

    var testʗ1 = test;
    runWithFIPSDisabled(t, (ж<testing.T> tΔ3) => {
        testʗ1(tΔ3, versionTLS10ˢ, VersionTLS10, ""u8);
        testʗ1(tΔ3, versionTLS11ˢ, VersionTLS11, ""u8);
        testʗ1(tΔ3, versionTLS12ˢ, VersionTLS12, ""u8);
        testʗ1(tΔ3, versionTLS13ˢ, VersionTLS13, ""u8);
    });

    var testʗ2 = test;
    runWithFIPSEnabled(t, (ж<testing.T> tΔ4) => {
        testʗ2(tΔ4, versionTLS10ˢ, VersionTLS10, supportedVersionsˢ);
        testʗ2(tΔ4, versionTLS11ˢ, VersionTLS11, supportedVersionsˢ);
        testʗ2(tΔ4, versionTLS12ˢ, VersionTLS12, ""u8);
        testʗ2(tΔ4, versionTLS13ˢ, VersionTLS13, ""u8);
    });

    if (!fips140.Enforced()) {
        rerunWithFIPS140Enforced(t);
    }
}

internal static bool isFIPSVersion(uint16 v) {
    return v == VersionTLS12 || v == VersionTLS13;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string chacha20ˢ => ref chacha20ˢᵇ;
private static readonly @string chacha20ˢᵇ = "CHACHA20"u8;
internal static ref readonly @string shaˢ => ref shaˢᵇ;
private static readonly @string shaˢᵇ = "_SHA"u8;
internal static ref readonly @string tlsRsaˢ => ref tlsRsaˢᵇ;
private static readonly @string tlsRsaˢᵇ = "TLS_RSA"u8;

internal static bool isFIPSCipherSuite(uint16 id) {
    @string name = CipherSuiteName(id);
    if (isTLS13CipherSuite(id)) {
        switch (id) {
        case TLS_AES_128_GCM_SHA256 or TLS_AES_256_GCM_SHA384: {
            return true;
        }
        case TLS_CHACHA20_POLY1305_SHA256: {
            return false;
        }
        default: {
            throw panic("unknown TLS 1.3 cipher suite: " + name);
            break;
        }}
    }
    switch (id) {
    case TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 or TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384 or TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256 or TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384: {
        return true;
    }
    case TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256 or TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256: {
        // Only for the native module.
        return !boring.Enabled;
    }}
    switch (ᐧ) {
    case {} when strings.Contains(name, chacha20ˢ): {
        return false;
    }
    case {} when strings.HasSuffix(name, shaˢ): { // SHA-1
        return false;
    }
    case {} when strings.HasPrefix(name, tlsRsaˢ): { // RSA kex
        return false;
    }
    default: {
        throw panic("unknown cipher suite: " + name);
        break;
    }}
}

internal static bool isFIPSCurve(global::go.crypto.tls_package.CurveID id) {
    switch (id) {
    case var ᴛ when ᴛ == CurveP256 || ᴛ == CurveP384 || ᴛ == CurveP521: {
        return true;
    }
    case var ᴛ when ᴛ == X25519MLKEM768 || ᴛ == SecP256r1MLKEM768 || ᴛ == SecP384r1MLKEM1024: {
        // Only for the native module.
        return !boring.Enabled;
    }
    case var ᴛ when ᴛ == X25519: {
        return false;
    }
    default: {
        throw panic("unknown curve: " + id.String());
        break;
    }}
}

internal static bool isECDSA(uint16 id) {
    foreach (var (_, suite) in ΔcipherSuites) {
        if (suite.id == id) {
            return (suite.flags & (nint)suiteECSign) == suiteECSign;
        }
    }
    return false; // TLS 1.3 cipher suites are not tied to the signature algorithm.
}

internal static bool isFIPSSignatureScheme(global::go.crypto.tls_package.SignatureScheme alg) {
    switch (alg) {
    case var ᴛ when ᴛ == PKCS1WithSHA256 || ᴛ == ECDSAWithP256AndSHA256 || ᴛ == PKCS1WithSHA384 || ᴛ == ECDSAWithP384AndSHA384 || ᴛ == PKCS1WithSHA512 || ᴛ == ECDSAWithP521AndSHA512 || ᴛ == PSSWithSHA256 || ᴛ == PSSWithSHA384 || ᴛ == PSSWithSHA512: {
        return true;
    }
    case var ᴛ when ᴛ == Ed25519: {
        // Only for the native module.
        return !boring.Enabled;
    }
    case var ᴛ when ᴛ == PKCS1WithSHA1 || ᴛ == ECDSAWithSHA1: {
        return false;
    }
    default: {
        throw panic("unknown signature scheme: " + alg.String());
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noCipherSuiteSupportedByˢ => ref noCipherSuiteSupportedByˢᵇ;
private static readonly @string noCipherSuiteSupportedByˢᵇ = "no cipher suite supported by both client and server"u8;

public static void TestFIPSServerCipherSuites(ж<testing.T> t) {
    var serverConfig = testConfigFIPS140.Clone();
    serverConfig.Certificates = new slice<global::go.crypto.tls_package.Certificate>(1);

    foreach (var (_, id) in allCipherSuitesIncludingTLS13()) {
        if (isECDSA(id)) {
            serverConfig.Certificates[0].ΔCertificate = slice<slice<byte>>([testECDSACertificate]);
            serverConfig.Certificates[0].PrivateKey = testECDSAPrivateKey.OrTypedNil();
        } else {
            serverConfig.Certificates[0].ΔCertificate = slice<slice<byte>>([testRSACertificate]);
            serverConfig.Certificates[0].PrivateKey = testRSAPrivateKey.OrTypedNil();
        }
        serverConfig.BuildNameToCertificate();
        var serverConfigʗ1 = serverConfig;
        t.Run(fmt.Sprintf("suite=%s"u8, CipherSuiteName(id)), (ж<testing.T> tΔ1) => {
            var clientHello = Ꮡ(new clientHelloMsg(
                vers: VersionTLS12,
                random: new slice<byte>(32),
                cipherSuites: slice<uint16>([id]),
                compressionMethods: slice<uint8>([compressionNone]),
                supportedCurves: defaultCurvePreferences(),
                keyShares: slice<global::go.crypto.tls_package.keyShare>([generateKeyShare(CurveP256)]),
                supportedPoints: slice<uint8>([pointFormatUncompressed]),
                supportedVersions: slice<uint16>([VersionTLS12]),
                supportedSignatureAlgorithms: allowedSignatureAlgorithmsFIPS
            ));
            if (isTLS13CipherSuite(id)) {
                clientHello.supportedVersions = slice<uint16>([VersionTLS13]);
            }

            var clientHelloʗ1 = clientHello;
            var serverConfigʗ2 = serverConfigʗ1;
            runWithFIPSDisabled(tΔ1, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
                testClientHello(tΔ2, serverConfigʗ2, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHelloʗ1));
            });

            var clientHelloʗ2 = clientHello;
            var serverConfigʗ3 = serverConfigʗ1;
            runWithFIPSEnabled(tΔ1, (ж<testing.T> tΔ3) => {
                @string msg = ""u8;
                if (!isFIPSCipherSuite(id)) {
                    msg = noCipherSuiteSupportedByˢ;
                }
                testClientHelloFailure(tΔ3, serverConfigʗ3, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHelloʗ2), msg);
            });
        });
    }

    if (!fips140.Enforced()) {
        rerunWithFIPS140Enforced(t);
    }
}

public static void TestFIPSServerCurves(ж<testing.T> t) {
    var serverConfig = testConfigFIPS140.Clone();
    serverConfig.CurvePreferences = default;
    serverConfig.BuildNameToCertificate();

    foreach (var (_, curveid) in defaultCurvePreferences()) {
        var serverConfigʗ1 = serverConfig;
        t.Run(fmt.Sprintf("curve=%v"u8, curveid), (ж<testing.T> tΔ1) => {
            var clientConfig = testConfigFIPS140.Clone();
            clientConfig.CurvePreferences = slice<global::go.crypto.tls_package.CurveID>([curveid]);

            var clientConfigʗ1 = clientConfig;
            var serverConfigʗ2 = serverConfigʗ1;
            runWithFIPSDisabled(tΔ1, (ж<testing.T> tΔ2) => {
                {
                    var (_, _, err) = testHandshake(tΔ2, clientConfigʗ1, serverConfigʗ2); if (err != nil) {
                        tΔ2.Fatalf("got error: %v, expected success", err);
                    }
                }
            });

            // With fipstls forced, bad curves should be rejected.
            var clientConfigʗ2 = clientConfig;
            var serverConfigʗ3 = serverConfigʗ1;
            runWithFIPSEnabled(tΔ1, (ж<testing.T> tΔ3) => {
                var (_, _, err) = testHandshake(tΔ3, clientConfigʗ2, serverConfigʗ3);
                if (err != nil && isFIPSCurve(curveid)) {
                    tΔ3.Fatalf("got error: %v, expected success", err);
                } else if (err == nil && !isFIPSCurve(curveid)) {
                    tΔ3.Fatalf("got success, expected error");
                }
            });
        });
    }

    if (!fips140.Enforced()) {
        rerunWithFIPS140Enforced(t);
    }
}

internal static partial (error clientErr, error serverErr) fipsHandshake(ж<testing.T> t, ж<global::go.crypto.tls_package.Config> clientConfig, ж<global::go.crypto.tls_package.Config> serverConfig) {
    error clientErr = default;
    error serverErr = default;

    var (c, s) = localPipe(new tls_test_package.testing_TжTB(t));
    var client = Client(c, clientConfig);
    var server = Server(s, serverConfig);
    var done = new channel<error>(1);
    var cʗ1 = c;
    var clientʗ1 = client;
    var doneʗ1 = done;
    goǃ(() => {
        doneʗ1.ᐸꟷ(clientʗ1.Handshake());
        cʗ1.Close();
    });
    serverErr = server.Handshake();
    s.Close();
    clientErr = ᐸꟷ(done);
    return (clientErr, serverErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tlssha11ˢ => ref tlssha11ˢᵇ;
private static readonly @string tlssha11ˢᵇ = "tlssha1=1"u8;

public static void TestFIPSServerSignatureAndHash(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            testingOnlySupportedSignatureAlgorithms = default;
        }, ref ᒐ);
        defer((@string godebug) => {
            os.Setenv(godebugˢ, godebug);
        }, os.Getenv(godebugˢ), ref ᒐ);
        os.Setenv(godebugˢ, tlssha11ˢ);

        foreach (var (_, sigHash) in defaultSupportedSignatureAlgorithms()) {
            t.Run(fmt.Sprintf("%v"u8, sigHash), (ж<testing.T> tΔ1) => {
                var serverConfig = testConfigFIPS140.Clone();
                serverConfig.Certificates = new slice<global::go.crypto.tls_package.Certificate>(1);

                testingOnlySupportedSignatureAlgorithms = slice<global::go.crypto.tls_package.SignatureScheme>([sigHash]);

                var (sigType, _, _) = typeAndHashFromSignatureScheme(sigHash);
                switch (sigType) {
                case signaturePKCS1v15 or signatureRSAPSS: {
                    serverConfig.CipherSuites = slice<uint16>([TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256]);
                    serverConfig.Certificates[0].ΔCertificate = slice<slice<byte>>([testRSAPSS2048Certificate]);
                    serverConfig.Certificates[0].PrivateKey = testRSAPSS2048PrivateKey.OrTypedNil();
                    break;
                }
                case signatureEd25519: {
                    serverConfig.CipherSuites = slice<uint16>([TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256]);
                    serverConfig.Certificates[0].ΔCertificate = slice<slice<byte>>([testEd25519Certificate]);
                    serverConfig.Certificates[0].PrivateKey = testEd25519PrivateKey;
                    break;
                }
                case signatureECDSA: {
                    serverConfig.CipherSuites = slice<uint16>([TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256]);
                    serverConfig.Certificates[0].ΔCertificate = slice<slice<byte>>([testECDSACertificate]);
                    serverConfig.Certificates[0].PrivateKey = testECDSAPrivateKey.OrTypedNil();
                    break;
                }}
                serverConfig.BuildNameToCertificate();
                // PKCS#1 v1.5 signature algorithms can't be used standalone in TLS
                // 1.3, and the ECDSA ones bind to the curve used.
                serverConfig.MaxVersion = VersionTLS12;

                var serverConfigʗ1 = serverConfig;
                runWithFIPSDisabled(tΔ1, (ж<testing.T> tΔ2) => {
                    var (clientErr, serverErr) = fipsHandshake(tΔ2, testConfigFIPS140, serverConfigʗ1);
                    if (clientErr != nil) {
                        tΔ2.Fatalf("expected handshake with %v to succeed; client error: %v; server error: %v", sigHash, clientErr, serverErr);
                    }
                });

                // With fipstls forced, bad curves should be rejected.
                var serverConfigʗ2 = serverConfig;
                runWithFIPSEnabled(tΔ1, (ж<testing.T> tΔ3) => {
                    var (clientErr, _) = fipsHandshake(tΔ3, testConfigFIPS140, serverConfigʗ2);
                    if (isFIPSSignatureScheme(sigHash)) {
                        if (clientErr != nil) {
                            tΔ3.Fatalf("expected handshake with %v to succeed; err=%v", sigHash, clientErr);
                        }
                    } else {
                        if (clientErr == nil) {
                            tΔ3.Fatalf("expected handshake with %v to fail, but it succeeded", sigHash);
                        }
                    }
                });
            });
        }

        if (!fips140.Enforced()) {
            rerunWithFIPS140Enforced(t);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFIPSClientHello(ж<testing.T> t) {
    runWithFIPSEnabled(t, testFIPSClientHello);
}

internal static partial void testFIPSClientHello(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Test that no matter what we put in the client config,
        // the client does not offer non-FIPS configurations.

        var (c, s) = net.Pipe();
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var sʗ1 = s;
        defer(() => sʗ1.Close(), ref ᒐ);

        var clientConfig = testConfigFIPS140.Clone();
        // All sorts of traps for the client to avoid.
        clientConfig.MinVersion = VersionSSL30;
        clientConfig.MaxVersion = VersionTLS13;
        clientConfig.CipherSuites = allCipherSuites();
        clientConfig.CurvePreferences = defaultCurvePreferences();

        var cʗ2 = c;
        var clientConfigʗ1 = clientConfig;
        goǃ(ᴛ0 => ᴛ0.Handshake(), Client(cʗ2, clientConfigʗ1));
        var srv = Server(s, testConfigFIPS140);
        var (msg, err) = srv.readHandshake(default);
        if (err != nil) {
            t.Fatal(err);
        }
        var (hello, ok) = msg._<ж<global::go.crypto.tls_package.clientHelloMsg>>(ᐧ);
        if (!ok) {
            t.Fatalf("unexpected message type %T", msg);
        }

        if (!isFIPSVersion(hello.vers)) {
            t.Errorf("client vers=%#x", hello.vers);
        }
        foreach (var (_, v) in hello.supportedVersions) {
            if (!isFIPSVersion(v)) {
                t.Errorf("client offered disallowed version %#x", v);
            }
        }
        foreach (var (_, id) in hello.cipherSuites) {
            if (!isFIPSCipherSuite(id)) {
                t.Errorf("client offered disallowed suite %v", CipherSuiteName(id));
            }
        }
        foreach (var (_, id) in hello.supportedCurves) {
            if (!isFIPSCurve(id)) {
                t.Errorf("client offered disallowed curve %v", id);
            }
        }
        foreach (var (_, sigHash) in hello.supportedSignatureAlgorithms) {
            if (!isFIPSSignatureScheme(sigHash)) {
                t.Errorf("client offered disallowed signature-and-hash %v", sigHash);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string m1R1ˢ => ref m1R1ˢᵇ;
private static readonly @string m1R1ˢᵇ = "M1_R1"u8;
internal static ref readonly @string m2R1ˢ => ref m2R1ˢᵇ;
private static readonly @string m2R1ˢᵇ = "M2_R1"u8;
internal static ref readonly @string iR1ˢ => ref iR1ˢᵇ;
private static readonly @string iR1ˢᵇ = "I_R1"u8;
internal static ref readonly @string iR2ˢ => ref iR2ˢᵇ;
private static readonly @string iR2ˢᵇ = "I_R2"u8;
internal static ref readonly @string iM1ˢ => ref iM1ˢᵇ;
private static readonly @string iM1ˢᵇ = "I_M1"u8;
internal static ref readonly @string iM2ˢ => ref iM2ˢᵇ;
private static readonly @string iM2ˢᵇ = "I_M2"u8;
internal static ref readonly @string iR3ˢ => ref iR3ˢᵇ;
private static readonly @string iR3ˢᵇ = "I_R3"u8;
internal static ref readonly @string l1Iˢ => ref l1Iˢᵇ;
private static readonly @string l1Iˢᵇ = "L1_I"u8;
internal static ref readonly @string l2Iˢ => ref l2Iˢᵇ;
private static readonly @string l2Iˢᵇ = "L2_I"u8;
internal static ref readonly @string basicˢ => ref basicˢᵇ;
private static readonly @string basicˢᵇ = "basic"u8;
internal static ref readonly @string basicClientCertˢ => ref basicClientCertˢᵇ;
private static readonly @string basicClientCertˢᵇ = "basic (client cert)"u8;
internal static ref readonly @string basicFipsˢ => ref basicFipsˢᵇ;
private static readonly @string basicFipsˢᵇ = "basic (fips)"u8;
internal static ref readonly @string basicFipsClientCertˢ => ref basicFipsClientCertˢᵇ;
private static readonly @string basicFipsClientCertˢᵇ = "basic (fips, client cert)"u8;
internal static readonly object basicTestFailedSkippingˢ = (@string)"basic test failed, skipping exhaustive test"u8;

public static void TestFIPSCertAlgs(ж<testing.T> t) {
    // arm and wasm time out generating keys. Nothing in this test is
    // architecture-specific, so just don't bother on those.
    if (testenv.CPUIsSlow()) {
        t.Skipf("skipping on %s/%s because key generation takes too long", runtime.GOOS, runtime.GOARCH);
    }

    // Set up some roots, intermediate CAs, and leaf certs with various algorithms.
    // X_Y is X signed by Y.
    var R1 = fipsCert(t, "R1", fipsRSAKey(t, 2048).OrTypedNil(), nil, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var R2 = fipsCert(t, "R2", fipsRSAKey(t, 1024).OrTypedNil(), nil, fipsCertCA);
    var R3 = fipsCert(t, "R3", fipsRSAKey(t, 4096).OrTypedNil(), nil, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));

    var M1_R1 = fipsCert(t, m1R1ˢ, fipsECDSAKey(t, elliptic.P256()).OrTypedNil(), R1, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var M2_R1 = fipsCert(t, m2R1ˢ, fipsECDSAKey(t, elliptic.P224()).OrTypedNil(), R1, fipsCertCA);

    var I_R1 = fipsCert(t, iR1ˢ, fipsRSAKey(t, 3072).OrTypedNil(), R1, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var I_R2 = fipsCert(t, iR2ˢ, I_R1.key, R2, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var I_M1 = fipsCert(t, iM1ˢ, I_R1.key, M1_R1, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var I_M2 = fipsCert(t, iM2ˢ, I_R1.key, M2_R1, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));

    var I_R3 = fipsCert(t, iR3ˢ, fipsRSAKey(t, 3072).OrTypedNil(), R3, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    fipsCert(t, iR3ˢ, I_R3.key, R3, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));

    var L1_I = fipsCert(t, l1Iˢ, fipsECDSAKey(t, elliptic.P384()).OrTypedNil(), I_R1, (nint)((nint)fipsCertLeaf | (nint)fipsCertFIPSOK));
    var L2_I = fipsCert(t, l2Iˢ, fipsRSAKey(t, 1024).OrTypedNil(), I_R1, fipsCertLeaf);

    // client verifying server cert
    void testServerCert(ж<testing.T> tΔ1, @string desc, ж<Δx509.CertPool> pool, any key, slice<slice<byte>> list, bool ok) {
        var clientConfig = testConfigFIPS140.Clone();
        clientConfig.RootCAs = pool;
        clientConfig.InsecureSkipVerify = false;
        clientConfig.ServerName = exampleComˢ;

        var serverConfig = testConfigFIPS140.Clone();
        serverConfig.Certificates = slice<global::go.crypto.tls_package.Certificate>([new(ΔCertificate: list, PrivateKey: key)]);
        serverConfig.BuildNameToCertificate();

        var (clientErr, _) = fipsHandshake(tΔ1, clientConfig, serverConfig);

        if ((clientErr == nil) == ok) {
            if (ok) {
                tΔ1.Logf("%s: accept", desc);
            } else {
                tΔ1.Logf("%s: reject", desc);
            }
        } else {
            if (ok) {
                tΔ1.Errorf("%s: BAD reject (%v)", desc, clientErr);
            } else {
                tΔ1.Errorf("%s: BAD accept", desc);
            }
        }
    }

    // server verifying client cert
    void testClientCert(ж<testing.T> tΔ2, @string desc, ж<Δx509.CertPool> pool, any key, slice<slice<byte>> list, bool ok) {
        var clientConfig = testConfigFIPS140.Clone();
        clientConfig.ServerName = exampleComˢ;
        clientConfig.Certificates = slice<global::go.crypto.tls_package.Certificate>([new(ΔCertificate: list, PrivateKey: key)]);

        var serverConfig = testConfigFIPS140.Clone();
        serverConfig.ClientCAs = pool;
        serverConfig.ClientAuth = RequireAndVerifyClientCert;

        var (_, serverErr) = fipsHandshake(tΔ2, clientConfig, serverConfig);

        if ((serverErr == nil) == ok) {
            if (ok) {
                tΔ2.Logf("%s: accept", desc);
            } else {
                tΔ2.Logf("%s: reject", desc);
            }
        } else {
            if (ok) {
                tΔ2.Errorf("%s: BAD reject (%v)", desc, serverErr);
            } else {
                tΔ2.Errorf("%s: BAD accept", desc);
            }
        }
    }

    // Run simple basic test with known answers before proceeding to
    // exhaustive test with computed answers.
    var r1pool = Δx509.NewCertPool();
    r1pool.AddCert(R1.cert);

    var I_R1ʗ1 = I_R1;
    var L2_Iʗ1 = L2_I;
    var r1poolʗ1 = r1pool;
    var testClientCertʗ1 = testClientCert;
    var testServerCertʗ1 = testServerCert;
    runWithFIPSDisabled(t, (ж<testing.T> tΔ3) => {
        testServerCertʗ1(tΔ3, basicˢ, r1poolʗ1, L2_Iʗ1.key, slice<slice<byte>>([L2_Iʗ1.der, I_R1ʗ1.der]), true);
        testClientCertʗ1(tΔ3, basicClientCertˢ, r1poolʗ1, L2_Iʗ1.key, slice<slice<byte>>([L2_Iʗ1.der, I_R1ʗ1.der]), true);
    });

    var I_R1ʗ2 = I_R1;
    var L2_Iʗ2 = L2_I;
    var r1poolʗ2 = r1pool;
    var testClientCertʗ2 = testClientCert;
    var testServerCertʗ2 = testServerCert;
    runWithFIPSEnabled(t, (ж<testing.T> tΔ4) => {
        testServerCertʗ2(tΔ4, basicFipsˢ, r1poolʗ2, L2_Iʗ2.key, slice<slice<byte>>([L2_Iʗ2.der, I_R1ʗ2.der]), false);
        testClientCertʗ2(tΔ4, basicFipsClientCertˢ, r1poolʗ2, L2_Iʗ2.key, slice<slice<byte>>([L2_Iʗ2.der, I_R1ʗ2.der]), false);
    });

    if (t.Failed()) {
        t.Fatal(basicTestFailedSkippingˢ);
    }

    if (testing.Short()) {
        t.Logf("basic test passed; skipping exhaustive test in -short mode");
        return;
    }

    for (nint l = 1; l <= 2; l++) {
        ref var leaf = ref heap<ж<fipsCertificate>>(L1_I, out var Ꮡleaf);
        if (l == 2) {
            leaf = L2_I;
        }
        for (nint i = 0; i < 64; i++) {
            var reachable = new map<@string, bool>{[leaf.parentOrg] = true};
            var reachableFIPS = new map<@string, bool>{[leaf.parentOrg] = leaf.fipsOK};
            ref var list = ref heap<slice<slice<byte>>>(slice<slice<byte>>([leaf.der]), out var Ꮡlist);
            @string listName = leaf.name;
            var reachableʗ1 = reachable;
            var reachableFIPSʗ1 = reachableFIPS;
            void addList(nint cond, ж<fipsCertificate> c) {
                if (cond != 0) {
                    Ꮡlist.ValueSlot = append(Ꮡlist.ValueSlot, c.der);
                    listName += ","u8 + c.name;
                    if (reachableʗ1[c.org]) {
                        reachableʗ1[c.parentOrg] = true;
                    }
                    if (reachableFIPSʗ1[c.org] && c.fipsOK) {
                        reachableFIPSʗ1[c.parentOrg] = true;
                    }
                }
            }
            addList(i & 1, I_R1);
            addList(i & 2, I_R2);
            addList(i & 4, I_M1);
            addList(i & 8, I_M2);
            addList(i & 16, M1_R1);
            addList(i & 32, M2_R1);

            for (nint r = 1; r <= 3; r++) {
                var pool = Δx509.NewCertPool();
                @string rootName = ",";
                var shouldVerify = false;
                var shouldVerifyFIPS = false;
                var poolʗ1 = pool;
                var reachableʗ2 = reachable;
                var reachableFIPSʗ2 = reachableFIPS;
                void addRoot(nint cond, ж<fipsCertificate> c) {
                    if (cond != 0) {
                        rootName += ","u8 + c.name;
                        poolʗ1.AddCert(c.cert);
                        if (reachableʗ2[c.org]) {
                            shouldVerify = true;
                        }
                        if (reachableFIPSʗ2[c.org] && c.fipsOK) {
                            shouldVerifyFIPS = true;
                        }
                    }
                }
                addRoot(r & 1, R1);
                addRoot(r & 2, R2);
                rootName = rootName[1..]; // strip leading comma

                var poolʗ2 = pool;
                var testClientCertʗ3 = testClientCert;
                var testServerCertʗ3 = testServerCert;
                runWithFIPSDisabled(t, (ж<testing.T> tΔ5) => {
                    testServerCertʗ3(tΔ5, listName + "->" + rootName[1..], poolʗ2, Ꮡleaf.ValueSlot.key, Ꮡlist.ValueSlot, shouldVerify);
                    testClientCertʗ3(tΔ5, listName + "->" + rootName[1..] + "(client cert)", poolʗ2, Ꮡleaf.ValueSlot.key, Ꮡlist.ValueSlot, shouldVerify);
                });

                var poolʗ3 = pool;
                var testClientCertʗ4 = testClientCert;
                var testServerCertʗ4 = testServerCert;
                runWithFIPSEnabled(t, (ж<testing.T> tΔ6) => {
                    testServerCertʗ4(tΔ6, listName + "->" + rootName[1..] + " (fips)", poolʗ3, Ꮡleaf.ValueSlot.key, Ꮡlist.ValueSlot, shouldVerifyFIPS);
                    testClientCertʗ4(tΔ6, listName + "->" + rootName[1..] + " (fips, client cert)", poolʗ3, Ꮡleaf.ValueSlot.key, Ꮡlist.ValueSlot, shouldVerifyFIPS);
                });
            }
        }
    }
}

internal static UntypedInt fipsCertCA => iota;
internal static UntypedInt fipsCertLeaf => 1;
internal static UntypedInt fipsCertFIPSOK => 0x80;

internal static ж<rsa.PrivateKey> fipsRSAKey(ж<testing.T> t, nint size) {
    var (k, err) = rsa.GenerateKey(go.crypto.rand_package.Reader, size);
    if (err != nil) {
        t.Fatal(err);
    }
    return k;
}

internal static ж<ecdsa.PrivateKey> fipsECDSAKey(ж<testing.T> t, elliptic.Curve curve) {
    var (k, err) = ecdsa.GenerateKey(curve, go.crypto.rand_package.Reader);
    if (err != nil) {
        t.Fatal(err);
    }
    return k;
}

internal partial struct fipsCertificate {
    internal @string name;
    internal @string org;
    internal @string parentOrg;
    internal slice<byte> der;
    internal ж<Δx509.Certificate> cert;
    internal any key;
    internal bool fipsOK;
}

internal static ж<fipsCertificate> fipsCert(ж<testing.T> t, @string name, any key, ж<fipsCertificate> Ꮡparent, nint mode) {
    ref var parent = ref Ꮡparent.DerefOrNull();

    ref var org = ref heap<@string>(name, out var Ꮡorg);
    ref var parentOrg = ref heap<@string>(""u8, out var ᏑparentOrg);
    {
        nint i = strings.Index(org, "_"); if (i >= 0) {
            org = org.slice(0, i);
            parentOrg = name.slice(i + 1);
        }
    }
    var tmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        Subject: new pkix.Name(
            Organization: slice<@string>([org])
        ),
        NotBefore: time_package.Unix(0, 0),
        NotAfter: time_package.Unix(0, 0),

        KeyUsage: (Δx509.KeyUsage)(Δx509.KeyUsageKeyEncipherment | Δx509.KeyUsageDigitalSignature),
        ExtKeyUsage: slice<Δx509.ExtKeyUsage>([Δx509.ExtKeyUsageServerAuth, Δx509.ExtKeyUsageClientAuth]),
        BasicConstraintsValid: true
    ));
    if ((nint)(mode & ~(nint)(nint)fipsCertFIPSOK) == fipsCertLeaf) {
        tmpl.DNSNames = slice<@string>(["example.com"]);
    } else {
        tmpl.IsCA = true;
        tmpl.KeyUsage |= (Δx509.KeyUsage)(Δx509.KeyUsageCertSign);
    }

    ж<Δx509.Certificate> pcert = default;
    any pkey = default;
    if (Ꮡparent != nil) {
        pcert = parent.cert;
        pkey = parent.key;
    } else {
        pcert = tmpl;
        pkey = key;
    }

    any pub = default;
    @string desc = default;
    switch (key.type()) {
    case ж<rsa.PrivateKey> k: {
        pub = k.ᏑPublicKey;
        desc = fmt.Sprintf("RSA-%d"u8, k.N.BitLen());
        break;
    }
    case ж<ecdsa.PrivateKey> k: {
        pub = k.ᏑPublicKey;
        desc = "ECDSA-"u8 + k.Curve.Params().Name;
        break;
    }
    default: {
        var k = key;
        t.Fatalf("invalid key %T", key);
        break;
    }}

    var (der, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, pcert, pub, pkey);
    if (err != nil) {
        t.Fatal(err);
    }
    (var cert, err) = Δx509.ParseCertificate(der);
    if (err != nil) {
        t.Fatal(err);
    }

    ref var fipsOK = ref heap<bool>((mode & (nint)fipsCertFIPSOK) != 0, out var ᏑfipsOK);
    var certʗ1 = cert;
    var fipsOKʗ1 = fipsOK;
    runWithFIPSEnabled(t, (ж<testing.T> tΔ1) => {
        if (isCertificateAllowedFIPS(ref (certʗ1).DerefOrNull()) != fipsOKʗ1) {
            tΔ1.Errorf("fipsAllowCert(cert with %s key) = %v, want %v", desc, !fipsOKʗ1, fipsOKʗ1);
        }
    });

    return Ꮡ(new fipsCertificate(name, org, parentOrg, der, cert, key, fipsOK));
}

// A self-signed test certificate with an RSA key of size 2048, for testing
// RSA-PSS with SHA512. SAN of example.golang.
internal static slice<byte> testRSAPSS2048Certificate;
internal static ж<rsa.PrivateKey> testRSAPSS2048PrivateKey;

[GoInit] internal static void init() {
    var (block, _) = pem.Decode(obscuretestdata.Rot13(slice<byte>("""

-----ORTVA PREGVSVPNGR-----
ZVVP/mPPNrrtNjVONtVENYUUK/xu4+4mZH9QnemORpDjQDLWXbMVuipANDRYODNj
RwRDZN4TN1HRPuZUDJAgMFOQomNrSj0kZGNkZQRkAGN0ZQInSj0lZQRlZwxkAGN0
ZQInZOVkRQNBOtAIONbGO0SwoJHtD28jttRvZN0TPFdTFVo3QDRONDHNN4VOQjNj
ttRXNbVONDPs8sx0A6vrPOK4VBIVsXvgg4xTpBDYrvzPsfwddUplfZVITRgSFZ6R
4Nl141s/7VdqJ0HgVdAo4CKuEBVQ7lQkE284kY6KoPhi/g5uC3HpruLp3uzYvlIq
ZxMDvMJgsHHWs/1dBgZ+buAt59YEJc4q+6vK0yn1WY3RjPVpxxAwW9uDoS7Co2PF
+RF9Lb55XNnc8XBoycpE8ZOFA38odajwsDqPKiBRBwnz2UHkXmRSK5ZN+sN0zr4P
vbPpPEYJXy+TbA9S8sNOsbM+G+2rny4QYhB95eKE8FeBVIOu3KSBe/EIuwgKpAIS
MXpiQg6q68I6wNXNLXz5ayw9TCcq4i+eNtZONNTwHQOBZN4TN1HqQjRO/jDRNjVS
bQNGOtAIUFHRQQNXOtteOtRSODpQNGNZOtAIUEZONs8RNwNNZOxTN1HqRDDFZOPP
QzI4LJ1joTHhM29fLJ5aZN0TPFdTFVo3QDROPjHNN4VONDPBbLfIpSPOuobdr3JU
qP6I7KKKRPzawu01e8u80li0AE379aFQ3pj2Z+UXinKlfJdey5uwTIXj0igjQ81e
I4WmQh7VsVbt5z8+DAP+7YdQMfm88iQXBefblFIBzHPtzPXSKrj+YN+rB/vDRWGe
7rafqqBrKWRc27Rq5iJ+xzJJ3Dztyp2Tjl8jSeZQVdaeaBmON4bPaQRtgKWg0mbt
aEjosRZNJv1nDEl5qG9XN3FC9zb5FrGSFmTTUvR4f4tUHr7wifNSS2dtgQ6+jU6f
m9o6fukaP7t5VyOXuV7FIO/Hdg2lqW+xU1LowZpVd6ANZ5rAZXtMhWe3+mjfFtju
TAnR
-----RAQ PREGVSVPNGR-----
"""u8.Raw())));
    testRSAPSS2048Certificate = block.Bytes;

    (block, _) = pem.Decode(obscuretestdata.Rot13(slice<byte>("""

-----ORTVA EFN CEVINGR XRL-----
ZVVRcNVONNXPNDRNa/U5AQrbattI+PQyFUlbeorWOaQxP3bcta7V6du3ZeQPSEuY
EHwBuBNZgrAK/+lXaIgSYFXwJ+Q14HGvN+8t8HqiBZF+y2jee/7rLG91UUbJUA4M
v4fyKGWTHVzIeK1SPK/9nweGCdVGLBsF0IdrUshby9WJgFF9kZNvUWWQLlsLHTkr
m29txiuRiJXBrFtTdsPwz5nKRsQNHwq/T6c8V30UDy7muQb2cgu1ZFfkOI+GNCaj
AWahNbdNaNxF1vcsudQsEsUjNK6Tsx/gazcrNl7wirn10sRdmvSDLq1kGd/0ILL7
I3QIEJFaYj7rariSrbjPtTPchM5L/Ew6KrY/djVQNDNONbVONDPAcZMvsq/it42u
UqPiYhMnLF0E7FhaSycbKRfygTqYSfac0VsbWM/htSDOFNVVsYjZhzH6bKN1m7Hi
98nVLI61QrCeGPQIQSOfUoAzC8WNb8JgohfRojq5mlbO7YLT2+pyxWxyJR73XdHd
ezV+HWrlFpy2Tva7MGkOKm1JCOx9IjpajxrnKctNFVOJ23suRPZ9taLRRjnOrm5G
6Zr8q1gUgLDi7ifXr7eb9j9/UXeEKrwdLXX1YkxusSevlI+z8YMWMa2aKBn6T3tS
Ao8Dx1Hx5CHORAOzlZSWuG4Z/hhFd4LgZeeB2tv8D+sCuhTmp5FfuLXEOc0J4C5e
zgIPgRSENbTONZRAOVSYeI2+UfTw0kLSnfXbi/DCr6UFGE1Uu2VMBAc+bX4bfmJR
wOG4IpaVGzcy6gP1Jl4TpekwAtXVSMNw+1k1YHHYqbeKxhT8le0gNuT9mAlsJfFl
CeFbiP0HIome8Wkkyn+xDIkRDDdJDkCyRIhY8xKnVQN6Ylg1Uchn2YiCNbTONADM
p6Yd2G7+OkYkAqv2z8xMmrw5xtmOc/KqIfoSJEyroVK2XeSUfeUmG9CHx3QR1iMX
Z6cmGg94aDuJFxQtPnj1FbuRyW3USVSjphfS1FWNp3cDrcq8ht6VLqycQZYgOw/C
/5C6OIHgtb05R4+V/G3vLngztyDkGgyM0ExFI2yyNbTONYBKxXSK7nuCis0JxfQu
hGshSBGCbbjtDT0RctJ0jEqPkrt/WYvp3yFQ0tfggDI2JfErpelJpknryEt10EzB
38OobtzunS4kitfFihwBsvMGR8bX1G43Z+6AXfVyZY3LVYocH/9nWkCJl0f2QdQe
pDWuMeyx+cmwON7Oas/HEqjkNbTNXE/PAj14Q+zeY3LYoovPKvlqdkIjki5cqMqm
8guv3GApfJP4vTHEqpIdosHvaICqWvKr/Xnp3JTPrEWnSItoXNBkYgv1EO5ZxVut
Q8rlhcOdx4J1Y1txekdfqw4GSykxjZljwy2R2F4LlD8COg6I04QbIEMfVXmdm+CS
HvbaCd0PtLOPLKidvbWuCrjxBd/L5jeQOrMJ1SDX5DQ9J5Z8/5mkq4eqiWgwuoWc
bBegiZqey6hcl9Um4OWQ3SKjISvCSR7wdrAdv0S21ivYkOCZZQ3HBQS6YY5RlYvE
9I4kIZF8XKkit7ekfhdmZCfpIvnJHY6JAIOufQ2+92qUkFKmm5RWXD==
-----RAQ EFN CEVINGR XRL-----
"""u8.Raw())));
    error err = default;
    (testRSAPSS2048PrivateKey, err) = Δx509.ParsePKCS1PrivateKey(block.Bytes);
    if (err != nil) {
        throw panic(err);
    }
}

} // end tls_internal_test_package
