// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
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
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using crypto = crypto_package;
using ecdh = go.crypto.ecdh_package;
using encoding;
using go.@internal;
using go.crypto;
using go.crypto.x509;
using go.math;
using io = io_package;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

internal static slice<uint16> allCipherSuitesIncludingTLS13() {
    var s = allCipherSuites();
    foreach (var (_, suite) in cipherSuitesTLS13) {
        s = append(s, (~suite).id);
    }
    return s;
}

internal static bool isTLS13CipherSuite(uint16 id) {
    foreach (var (_, suite) in cipherSuitesTLS13) {
        if (id == (~suite).id) {
            return true;
        }
    }
    return false;
}

internal static global::go.crypto.tls_package.keyShare generateKeyShare(global::go.crypto.tls_package.CurveID group) {
    var (key, err) = generateECDHEKey(go.crypto.rand_package.Reader, group);
    if (err != default!) {
        throw panic(err);
    }
    return new keyShare(group: group, data: key.PublicKey().Bytes());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string versionTLS10ˢ = "VersionTLS10"u8;
internal static readonly @string versionTLS11ˢ = "VersionTLS11"u8;
internal static readonly @string versionTLS12ˢ = "VersionTLS12"u8;
internal static readonly @string versionTLS13ˢ = "VersionTLS13"u8;
internal static readonly @string supportedVersionsˢ = "supported versions"u8;

public static void TestFIPSServerProtocolVersion(ж<testing.T> Ꮡt) {
    void test(ж<testing.T> tΔ1, @string name, uint16 v, @string msg) {
        tΔ1.Run(name, (ж<testing.T> tΔ2) => {
            var serverConfig = testConfig.Clone();
            serverConfig.Value.MinVersion = VersionSSL30;
            var clientConfig = testConfig.Clone();
            clientConfig.Value.MinVersion = v;
            clientConfig.Value.MaxVersion = v;
            var (_, _, err) = testHandshake(tΔ2, clientConfig, serverConfig);
            if (msg == ""u8){
                if (err != default!) {
                    tΔ2.Fatalf("got error: %v, expected success"u8, err);
                }
            } else {
                if (err == default!) {
                    tΔ2.Fatalf("got success, expected error"u8);
                }
                if (!strings.Contains(err.Error(), msg)) {
                    tΔ2.Fatalf("got error %v, expected %q"u8, err, msg);
                }
            }
        });
    }
    var testʗ1 = test;
    runWithFIPSDisabled(Ꮡt, (ж<testing.T> tΔ3) => {
        testʗ1(tΔ3, versionTLS10ˢ, VersionTLS10, ""u8);
        testʗ1(tΔ3, versionTLS11ˢ, VersionTLS11, ""u8);
        testʗ1(tΔ3, versionTLS12ˢ, VersionTLS12, ""u8);
        testʗ1(tΔ3, versionTLS13ˢ, VersionTLS13, ""u8);
    });
    var testʗ2 = test;
    runWithFIPSEnabled(Ꮡt, (ж<testing.T> tΔ4) => {
        testʗ2(tΔ4, versionTLS10ˢ, VersionTLS10, supportedVersionsˢ);
        testʗ2(tΔ4, versionTLS11ˢ, VersionTLS11, supportedVersionsˢ);
        testʗ2(tΔ4, versionTLS12ˢ, VersionTLS12, ""u8);
        testʗ2(tΔ4, versionTLS13ˢ, VersionTLS13, ""u8);
    });
}

internal static bool isFIPSVersion(uint16 v) {
    return v == VersionTLS12 || v == VersionTLS13;
}

internal static bool isFIPSCipherSuite(uint16 id) {
    switch (id) {
    case TLS_AES_128_GCM_SHA256 or TLS_AES_256_GCM_SHA384 or TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 or TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384 or TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256 or TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384: {
        return true;
    }}

    return false;
}

internal static bool isFIPSCurve(global::go.crypto.tls_package.CurveID id) {
    var exprᴛ1 = id;
    if (exprᴛ1 == CurveP256 || exprᴛ1 == CurveP384 || exprᴛ1 == CurveP521) {
        return true;
    }

    return false;
}

internal static bool isECDSA(uint16 id) {
    foreach (var (_, suite) in ΔcipherSuites) {
        if ((~suite).id == id) {
            return (nint)((~suite).flags & (nint)suiteECSign) == suiteECSign;
        }
    }
    return false; // TLS 1.3 cipher suites are not tied to the signature algorithm.
}

internal static bool isFIPSSignatureScheme(global::go.crypto.tls_package.SignatureScheme alg) {
    var exprᴛ1 = alg;
    if (exprᴛ1 == PKCS1WithSHA256 || exprᴛ1 == ECDSAWithP256AndSHA256 || exprᴛ1 == PKCS1WithSHA384 || exprᴛ1 == ECDSAWithP384AndSHA384 || exprᴛ1 == PKCS1WithSHA512 || exprᴛ1 == ECDSAWithP521AndSHA512 || exprᴛ1 == PSSWithSHA256 || exprᴛ1 == PSSWithSHA384 || exprᴛ1 == PSSWithSHA512) {
    }
    else { /* default: */
        return false;
    }

    // ok
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noCipherSuiteSupportedByˢ = "no cipher suite supported by both client and server"u8;

public static void TestFIPSServerCipherSuites(ж<testing.T> Ꮡt) {
    var serverConfig = testConfig.Clone();
    serverConfig.Value.Certificates = new slice<global::go.crypto.tls_package.Certificate>(1);
    foreach (var (_, id) in allCipherSuitesIncludingTLS13()) {
        if (isECDSA(id)){
            (~serverConfig).Certificates[0].ΔCertificate = new slice<byte>[]{testECDSACertificate}.slice();
            (~serverConfig).Certificates[0].PrivateKey = testECDSAPrivateKey.OrTypedNil();
        } else {
            (~serverConfig).Certificates[0].ΔCertificate = new slice<byte>[]{testRSACertificate}.slice();
            (~serverConfig).Certificates[0].PrivateKey = testRSAPrivateKey.OrTypedNil();
        }
        serverConfig.BuildNameToCertificate();
        var serverConfigʗ1 = serverConfig;
        Ꮡt.Run(fmt.Sprintf("suite=%s"u8, CipherSuiteName(id)), (ж<testing.T> tΔ1) => {
            var clientHello = Ꮡ(new clientHelloMsg(
                vers: VersionTLS12,
                random: new slice<byte>(32),
                cipherSuites: new uint16[]{id}.slice(),
                compressionMethods: new uint8[]{compressionNone}.slice(),
                supportedCurves: defaultCurvePreferences(),
                keyShares: new global::go.crypto.tls_package.keyShare[]{generateKeyShare(CurveP256)}.slice(),
                supportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
                supportedVersions: new uint16[]{VersionTLS12}.slice(),
                supportedSignatureAlgorithms: defaultSupportedSignatureAlgorithmsFIPS
            ));
            if (isTLS13CipherSuite(id)) {
                clientHello.Value.supportedVersions = new uint16[]{VersionTLS13}.slice();
            }
            var clientHelloʗ1 = clientHello;
            var serverConfigʗ2 = serverConfigʗ1;
            runWithFIPSDisabled(tΔ1, (ж<testing.T> tΔ2) => {
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
}

public static void TestFIPSServerCurves(ж<testing.T> Ꮡt) {
    var serverConfig = testConfig.Clone();
    serverConfig.Value.CurvePreferences = default!;
    serverConfig.BuildNameToCertificate();
    foreach (var (_, curveid) in defaultCurvePreferences()) {
        var serverConfigʗ1 = serverConfig;
        Ꮡt.Run(fmt.Sprintf("curve=%d"u8, curveid), (ж<testing.T> tΔ1) => {
            var clientConfig = testConfig.Clone();
            clientConfig.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{curveid}.slice();
            var clientConfigʗ1 = clientConfig;
            var serverConfigʗ2 = serverConfigʗ1;
            runWithFIPSDisabled(tΔ1, (ж<testing.T> tΔ2) => {
                {
                    var (_, _, err) = testHandshake(tΔ2, clientConfigʗ1, serverConfigʗ2); if (err != default!) {
                        tΔ2.Fatalf("got error: %v, expected success"u8, err);
                    }
                }
            });
            // With fipstls forced, bad curves should be rejected.
            var clientConfigʗ2 = clientConfig;
            var serverConfigʗ3 = serverConfigʗ1;
            runWithFIPSEnabled(tΔ1, (ж<testing.T> tΔ3) => {
                var (_, _, err) = testHandshake(tΔ3, clientConfigʗ2, serverConfigʗ3);
                if (err != default! && isFIPSCurve(curveid)){
                    tΔ3.Fatalf("got error: %v, expected success"u8, err);
                } else 
                if (err == default! && !isFIPSCurve(curveid)) {
                    tΔ3.Fatalf("got success, expected error"u8);
                }
            });
        });
    }
}

internal static partial (error clientErr, error serverErr) fipsHandshake(ж<testing.T> Ꮡt, ж<global::go.crypto.tls_package.Config> ᏑclientConfig, ж<global::go.crypto.tls_package.Config> ᏑserverConfig) {
    error clientErr = default!;
    error serverErr = default!;

    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var client = Client(c, ᏑclientConfig);
    var server = Server(s, ᏑserverConfig);
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

public static void TestFIPSServerSignatureAndHash(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(() => {
            testingOnlyForceClientHelloSignatureAlgorithms = default!;
        }, ref ᒐ);
        foreach (var (_, sigHash) in defaultSupportedSignatureAlgorithms) {
            Ꮡt.Run(fmt.Sprintf("%v"u8, sigHash), (ж<testing.T> tΔ1) => {
                var serverConfig = testConfig.Clone();
                serverConfig.Value.Certificates = new slice<global::go.crypto.tls_package.Certificate>(1);
                testingOnlyForceClientHelloSignatureAlgorithms = new global::go.crypto.tls_package.SignatureScheme[]{sigHash}.slice();
                var (sigType, _, _) = typeAndHashFromSignatureScheme(sigHash);
                switch (sigType) {
                case signaturePKCS1v15 or signatureRSAPSS: {
                    serverConfig.Value.CipherSuites = new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice();
                    (~serverConfig).Certificates[0].ΔCertificate = new slice<byte>[]{testRSAPSS2048Certificate}.slice();
                    (~serverConfig).Certificates[0].PrivateKey = testRSAPSS2048PrivateKey.OrTypedNil();
                    break;
                }
                case signatureEd25519: {
                    serverConfig.Value.CipherSuites = new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice();
                    (~serverConfig).Certificates[0].ΔCertificate = new slice<byte>[]{testEd25519Certificate}.slice();
                    (~serverConfig).Certificates[0].PrivateKey = testEd25519PrivateKey;
                    break;
                }
                case signatureECDSA: {
                    serverConfig.Value.CipherSuites = new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256}.slice();
                    (~serverConfig).Certificates[0].ΔCertificate = new slice<byte>[]{testECDSACertificate}.slice();
                    (~serverConfig).Certificates[0].PrivateKey = testECDSAPrivateKey.OrTypedNil();
                    break;
                }}

                serverConfig.BuildNameToCertificate();
                // PKCS#1 v1.5 signature algorithms can't be used standalone in TLS
                // 1.3, and the ECDSA ones bind to the curve used.
                serverConfig.Value.MaxVersion = VersionTLS12;
                var serverConfigʗ1 = serverConfig;
                runWithFIPSDisabled(tΔ1, (ж<testing.T> tΔ2) => {
                    var (clientErr, serverErr) = fipsHandshake(tΔ2, testConfig, serverConfigʗ1);
                    if (clientErr != default!) {
                        tΔ2.Fatalf("expected handshake with %#x to succeed; client error: %v; server error: %v"u8, sigHash, clientErr, serverErr);
                    }
                });
                // With fipstls forced, bad curves should be rejected.
                var serverConfigʗ2 = serverConfig;
                runWithFIPSEnabled(tΔ1, (ж<testing.T> tΔ3) => {
                    var (clientErr, _) = fipsHandshake(tΔ3, testConfig, serverConfigʗ2);
                    if (isFIPSSignatureScheme(sigHash)){
                        if (clientErr != default!) {
                            tΔ3.Fatalf("expected handshake with %#x to succeed; err=%v"u8, sigHash, clientErr);
                        }
                    } else {
                        if (clientErr == default!) {
                            tΔ3.Fatalf("expected handshake with %#x to fail, but it succeeded"u8, sigHash);
                        }
                    }
                });
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFIPSClientHello(ж<testing.T> Ꮡt) {
    runWithFIPSEnabled(Ꮡt, testFIPSClientHello);
}

internal static partial void testFIPSClientHello(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Test that no matter what we put in the client config,
        // the client does not offer non-FIPS configurations.
        var (c, s) = net.Pipe();
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var sʗ1 = s;
        defer(() => sʗ1.Close(), ref ᒐ);
        var clientConfig = testConfig.Clone();
        // All sorts of traps for the client to avoid.
        clientConfig.Value.MinVersion = VersionSSL30;
        clientConfig.Value.MaxVersion = VersionTLS13;
        clientConfig.Value.CipherSuites = allCipherSuites();
        clientConfig.Value.CurvePreferences = defaultCurvePreferences();
        var cʗ2 = c;
        var clientConfigʗ1 = clientConfig;
        goǃ(() => Client(cʗ2, clientConfigʗ1).Handshake());
        var srv = Server(s, testConfig);
        var (msg, err) = srv.readHandshake(default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var (hello, ok) = msg._<ж<global::go.crypto.tls_package.clientHelloMsg>>(ᐧ);
        if (!ok) {
            Ꮡt.Fatalf("unexpected message type %T"u8, msg);
        }
        if (!isFIPSVersion((~hello).vers)) {
            Ꮡt.Errorf("client vers=%#x"u8, (~hello).vers);
        }
        foreach (var (_, v) in (~hello).supportedVersions) {
            if (!isFIPSVersion(v)) {
                Ꮡt.Errorf("client offered disallowed version %#x"u8, v);
            }
        }
        foreach (var (_, id) in (~hello).cipherSuites) {
            if (!isFIPSCipherSuite(id)) {
                Ꮡt.Errorf("client offered disallowed suite %#x"u8, id);
            }
        }
        foreach (var (_, id) in (~hello).supportedCurves) {
            if (!isFIPSCurve(id)) {
                Ꮡt.Errorf("client offered disallowed curve %d"u8, id);
            }
        }
        foreach (var (_, sigHash) in (~hello).supportedSignatureAlgorithms) {
            if (!isFIPSSignatureScheme(sigHash)) {
                Ꮡt.Errorf("client offered disallowed signature-and-hash %v"u8, sigHash);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string m1R1ˢ = "M1_R1"u8;
internal static readonly @string m2R1ˢ = "M2_R1"u8;
internal static readonly @string iR1ˢ = "I_R1"u8;
internal static readonly @string iR2ˢ = "I_R2"u8;
internal static readonly @string iM1ˢ = "I_M1"u8;
internal static readonly @string iM2ˢ = "I_M2"u8;
internal static readonly @string iR3ˢ = "I_R3"u8;
internal static readonly @string l1Iˢ = "L1_I"u8;
internal static readonly @string l2Iˢ = "L2_I"u8;
internal static readonly @string basicˢ = "basic"u8;
internal static readonly @string basicClientCertˢ = "basic (client cert)"u8;
internal static readonly @string basicFipsˢ = "basic (fips)"u8;
internal static readonly @string basicFipsClientCertˢ = "basic (fips, client cert)"u8;
internal static readonly object basicTestFailedSkippingˢ = (@string)"basic test failed, skipping exhaustive test"u8;

public static void TestFIPSCertAlgs(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // arm and wasm time out generating keys. Nothing in this test is
    // architecture-specific, so just don't bother on those.
    if (testenv.CPUIsSlow()) {
        Ꮡt.Skipf("skipping on %s/%s because key generation takes too long"u8, runtime.GOOS, runtime.GOARCH);
    }
    // Set up some roots, intermediate CAs, and leaf certs with various algorithms.
    // X_Y is X signed by Y.
    var R1 = fipsCert(Ꮡt, "R1"u8, fipsRSAKey(Ꮡt, 2048).OrTypedNil(), nil, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var R2 = fipsCert(Ꮡt, "R2"u8, fipsRSAKey(Ꮡt, 1024).OrTypedNil(), nil, fipsCertCA);
    var R3 = fipsCert(Ꮡt, "R3"u8, fipsRSAKey(Ꮡt, 4096).OrTypedNil(), nil, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var M1_R1 = fipsCert(Ꮡt, m1R1ˢ, fipsECDSAKey(Ꮡt, elliptic.P256()).OrTypedNil(), R1, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var M2_R1 = fipsCert(Ꮡt, m2R1ˢ, fipsECDSAKey(Ꮡt, elliptic.P224()).OrTypedNil(), R1, fipsCertCA);
    var I_R1 = fipsCert(Ꮡt, iR1ˢ, fipsRSAKey(Ꮡt, 3072).OrTypedNil(), R1, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var I_R2 = fipsCert(Ꮡt, iR2ˢ, (~I_R1).key, R2, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var I_M1 = fipsCert(Ꮡt, iM1ˢ, (~I_R1).key, M1_R1, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var I_M2 = fipsCert(Ꮡt, iM2ˢ, (~I_R1).key, M2_R1, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var I_R3 = fipsCert(Ꮡt, iR3ˢ, fipsRSAKey(Ꮡt, 3072).OrTypedNil(), R3, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    fipsCert(Ꮡt, iR3ˢ, (~I_R3).key, R3, (nint)((nint)fipsCertCA | (nint)fipsCertFIPSOK));
    var L1_I = fipsCert(Ꮡt, l1Iˢ, fipsECDSAKey(Ꮡt, elliptic.P384()).OrTypedNil(), I_R1, (nint)((nint)fipsCertLeaf | (nint)fipsCertFIPSOK));
    var L2_I = fipsCert(Ꮡt, l2Iˢ, fipsRSAKey(Ꮡt, 1024).OrTypedNil(), I_R1, fipsCertLeaf);
    // client verifying server cert
    void testServerCert(ж<testing.T> tΔ1, @string desc, ж<Δx509.CertPool> pool, any key, slice<slice<byte>> list, bool ok) {
        var clientConfig = testConfig.Clone();
        clientConfig.Value.RootCAs = pool;
        clientConfig.Value.InsecureSkipVerify = false;
        clientConfig.Value.ServerName = exampleComˢ;
        var serverConfig = testConfig.Clone();
        serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(ΔCertificate: list, PrivateKey: key)}.slice();
        serverConfig.BuildNameToCertificate();
        var (clientErr, _) = fipsHandshake(tΔ1, clientConfig, serverConfig);
        if ((clientErr == default!) == ok){
            if (ok){
                tΔ1.Logf("%s: accept"u8, desc);
            } else {
                tΔ1.Logf("%s: reject"u8, desc);
            }
        } else {
            if (ok){
                tΔ1.Errorf("%s: BAD reject (%v)"u8, desc, clientErr);
            } else {
                tΔ1.Errorf("%s: BAD accept"u8, desc);
            }
        }
    }
    // server verifying client cert
    void testClientCert(ж<testing.T> tΔ2, @string desc, ж<Δx509.CertPool> pool, any key, slice<slice<byte>> list, bool ok) {
        var clientConfig = testConfig.Clone();
        clientConfig.Value.ServerName = exampleComˢ;
        clientConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(ΔCertificate: list, PrivateKey: key)}.slice();
        var serverConfig = testConfig.Clone();
        serverConfig.Value.ClientCAs = pool;
        serverConfig.Value.ClientAuth = RequireAndVerifyClientCert;
        var (_, serverErr) = fipsHandshake(tΔ2, clientConfig, serverConfig);
        if ((serverErr == default!) == ok){
            if (ok){
                tΔ2.Logf("%s: accept"u8, desc);
            } else {
                tΔ2.Logf("%s: reject"u8, desc);
            }
        } else {
            if (ok){
                tΔ2.Errorf("%s: BAD reject (%v)"u8, desc, serverErr);
            } else {
                tΔ2.Errorf("%s: BAD accept"u8, desc);
            }
        }
    }
    // Run simple basic test with known answers before proceeding to
    // exhaustive test with computed answers.
    var r1pool = Δx509.NewCertPool();
    r1pool.AddCert((~R1).cert);
    var I_R1ʗ1 = I_R1;
    var L2_Iʗ1 = L2_I;
    var r1poolʗ1 = r1pool;
    var testClientCertʗ1 = testClientCert;
    var testServerCertʗ1 = testServerCert;
    runWithFIPSDisabled(Ꮡt, (ж<testing.T> tΔ3) => {
        testServerCertʗ1(tΔ3, basicˢ, r1poolʗ1, (~L2_Iʗ1).key, new slice<byte>[]{(~L2_Iʗ1).der, (~I_R1ʗ1).der}.slice(), true);
        testClientCertʗ1(tΔ3, basicClientCertˢ, r1poolʗ1, (~L2_Iʗ1).key, new slice<byte>[]{(~L2_Iʗ1).der, (~I_R1ʗ1).der}.slice(), true);
    });
    var I_R1ʗ2 = I_R1;
    var L2_Iʗ2 = L2_I;
    var r1poolʗ2 = r1pool;
    var testClientCertʗ2 = testClientCert;
    var testServerCertʗ2 = testServerCert;
    runWithFIPSEnabled(Ꮡt, (ж<testing.T> tΔ4) => {
        testServerCertʗ2(tΔ4, basicFipsˢ, r1poolʗ2, (~L2_Iʗ2).key, new slice<byte>[]{(~L2_Iʗ2).der, (~I_R1ʗ2).der}.slice(), false);
        testClientCertʗ2(tΔ4, basicFipsClientCertˢ, r1poolʗ2, (~L2_Iʗ2).key, new slice<byte>[]{(~L2_Iʗ2).der, (~I_R1ʗ2).der}.slice(), false);
    });
    if (Ꮡt.Failed()) {
        Ꮡt.Fatal(basicTestFailedSkippingˢ);
    }
    if (testing.Short()) {
        Ꮡt.Logf("basic test passed; skipping exhaustive test in -short mode"u8);
        return;
    }
    for (nint l = 1; l <= 2; l++) {
        ref var leaf = ref heap<ж<fipsCertificate>>(out var Ꮡleaf);
        leaf = L1_I;
        if (l == 2) {
            leaf = L2_I;
        }
        for (nint i = 0; i < 64; i++) {
            var reachable = new map<@string, bool>{[(~leaf).parentOrg] = true};
            var reachableFIPS = new map<@string, bool>{[(~leaf).parentOrg] = (~leaf).fipsOK};
            ref var list = ref heap<slice<slice<byte>>>(out var Ꮡlist);
            list = new slice<byte>[]{(~leaf).der}.slice();
            @string listName = leaf.Value.name;
            var reachableʗ1 = reachable;
            var reachableFIPSʗ1 = reachableFIPS;
            void addList(nint cond, ж<fipsCertificate> c) {
                if (cond != 0) {
                    Ꮡlist.ValueSlot = append(Ꮡlist.ValueSlot, (~c).der);
                    listName += ","u8 + (~c).name;
                    if (reachableʗ1[(~c).org]) {
                        reachableʗ1[(~c).parentOrg] = true;
                    }
                    if (reachableFIPSʗ1[(~c).org] && (~c).fipsOK) {
                        reachableFIPSʗ1[(~c).parentOrg] = true;
                    }
                }
            }
            addList((nint)(i & 1), I_R1);
            addList((nint)(i & 2), I_R2);
            addList((nint)(i & 4), I_M1);
            addList((nint)(i & 8), I_M2);
            addList((nint)(i & 16), M1_R1);
            addList((nint)(i & 32), M2_R1);
            for (nint r = 1; r <= 3; r++) {
                var pool = Δx509.NewCertPool();
                @string rootName = ","u8;
                var shouldVerify = false;
                var shouldVerifyFIPS = false;
                var poolʗ1 = pool;
                var reachableʗ2 = reachable;
                var reachableFIPSʗ2 = reachableFIPS;
                void addRoot(nint cond, ж<fipsCertificate> c) {
                    if (cond != 0) {
                        rootName += ","u8 + (~c).name;
                        poolʗ1.AddCert((~c).cert);
                        if (reachableʗ2[(~c).org]) {
                            shouldVerify = true;
                        }
                        if (reachableFIPSʗ2[(~c).org] && (~c).fipsOK) {
                            shouldVerifyFIPS = true;
                        }
                    }
                }
                addRoot((nint)(r & 1), R1);
                addRoot((nint)(r & 2), R2);
                rootName = rootName[1..]; // strip leading comma
                var poolʗ2 = pool;
                var testClientCertʗ3 = testClientCert;
                var testServerCertʗ3 = testServerCert;
                runWithFIPSDisabled(Ꮡt, (ж<testing.T> tΔ5) => {
                    testServerCertʗ3(tΔ5, listName + "->" + rootName[1..], poolʗ2, (~Ꮡleaf.ValueSlot).key, Ꮡlist.ValueSlot, shouldVerify);
                    testClientCertʗ3(tΔ5, listName + "->" + rootName[1..] + "(client cert)", poolʗ2, (~Ꮡleaf.ValueSlot).key, Ꮡlist.ValueSlot, shouldVerify);
                });
                var poolʗ3 = pool;
                var testClientCertʗ4 = testClientCert;
                var testServerCertʗ4 = testServerCert;
                runWithFIPSEnabled(Ꮡt, (ж<testing.T> tΔ6) => {
                    testServerCertʗ4(tΔ6, listName + "->" + rootName[1..] + " (fips)", poolʗ3, (~Ꮡleaf.ValueSlot).key, Ꮡlist.ValueSlot, shouldVerifyFIPS);
                    testClientCertʗ4(tΔ6, listName + "->" + rootName[1..] + " (fips, client cert)", poolʗ3, (~Ꮡleaf.ValueSlot).key, Ꮡlist.ValueSlot, shouldVerifyFIPS);
                });
            }
        }
    }
}

internal static UntypedInt fipsCertCA => iota;
internal static UntypedInt fipsCertLeaf => 1;
internal static UntypedInt fipsCertFIPSOK => 0x80;

internal static ж<rsa.PrivateKey> fipsRSAKey(ж<testing.T> Ꮡt, nint size) {
    var (k, err) = rsa.GenerateKey(go.crypto.rand_package.Reader, size);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return k;
}

internal static ж<ecdsa.PrivateKey> fipsECDSAKey(ж<testing.T> Ꮡt, elliptic.Curve curve) {
    var (k, err) = ecdsa.GenerateKey(curve, go.crypto.rand_package.Reader);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return k;
}

[GoType] internal partial struct fipsCertificate {
    internal @string name;
    internal @string org;
    internal @string parentOrg;
    internal slice<byte> der;
    internal ж<Δx509.Certificate> cert;
    internal any key;
    internal bool fipsOK;
}

internal static ж<fipsCertificate> fipsCert(ж<testing.T> Ꮡt, @string name, any key, ж<fipsCertificate> Ꮡparent, nint mode) {
    ref var parent = ref Ꮡparent.DerefOrNull();

    ref var org = ref heap<@string>(out var Ꮡorg);
    org = name;
    ref var parentOrg = ref heap<@string>(out var ᏑparentOrg);
    parentOrg = ""u8;
    {
        nint i = strings.Index(org, "_"u8); if (i >= 0) {
            org = org.slice(0, i);
            parentOrg = name.slice(i + 1);
        }
    }
    var tmpl = Ꮡ(new Δx509.Certificate(
        SerialNumber: big.NewInt(1),
        Subject: new pkix.Name(
            Organization: new @string[]{org}.slice()
        ),
        NotBefore: time_package.Unix(0, 0),
        NotAfter: time_package.Unix(0, 0),
        KeyUsage: (Δx509.KeyUsage)(Δx509.KeyUsageKeyEncipherment | Δx509.KeyUsageDigitalSignature),
        ExtKeyUsage: new Δx509.ExtKeyUsage[]{Δx509.ExtKeyUsageServerAuth, Δx509.ExtKeyUsageClientAuth}.slice(),
        BasicConstraintsValid: true
    ));
    if ((nint)(mode & ~(nint)(nint)fipsCertFIPSOK) == fipsCertLeaf){
        tmpl.Value.DNSNames = new @string[]{"example.com"u8}.slice();
    } else {
        tmpl.Value.IsCA = true;
        tmpl.Value.KeyUsage |= (Δx509.KeyUsage)(Δx509.KeyUsageCertSign);
    }
    ж<Δx509.Certificate> pcert = default!;
    any pkey = default!;
    if (Ꮡparent != nil){
        pcert = parent.cert;
        pkey = parent.key;
    } else {
        pcert = tmpl;
        pkey = key;
    }
    any pub = default!;
    @string desc = default!;
    switch (key.type()) {
    case ж<rsa.PrivateKey> k: {
        pub = k.of(rsa.PrivateKey.ᏑPublicKey);
        desc = fmt.Sprintf("RSA-%d"u8, (~k).N.BitLen());
        break;
    }
    case ж<ecdsa.PrivateKey> k: {
        pub = k.of(ecdsa.PrivateKey.ᏑPublicKey);
        desc = "ECDSA-"u8 + (~(~k).Curve.Params()).Name;
        break;
    }
    default: {
        var k = key;
        Ꮡt.Fatalf("invalid key %T"u8, key);
        break;
    }}
    var (der, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, pcert, pub, pkey);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var cert, err) = Δx509.ParseCertificate(der);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var fipsOK = ref heap<bool>(out var ᏑfipsOK);
    fipsOK = (nint)(mode & (nint)fipsCertFIPSOK) != 0;
    var certʗ1 = cert;
    var fipsOKʗ1 = fipsOK;
    runWithFIPSEnabled(Ꮡt, (ж<testing.T> tΔ1) => {
        if (fipsAllowCert(ref (certʗ1).DerefOrNull()) != fipsOKʗ1) {
            tΔ1.Errorf("fipsAllowCert(cert with %s key) = %v, want %v"u8, desc, !fipsOKʗ1, fipsOKʗ1);
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
"""u8)));
    testRSAPSS2048Certificate = block.Value.Bytes;
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
"""u8)));
    error err = default!;
    (testRSAPSS2048PrivateKey, err) = Δx509.ParsePKCS1PrivateKey((~block).Bytes);
    if (err != default!) {
        throw panic(err);
    }
}

} // end tls_internal_test_package
