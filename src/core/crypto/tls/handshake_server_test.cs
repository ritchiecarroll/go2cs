// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using context = context_package;
using crypto = crypto_package;
using ecdh = go.crypto.ecdh_package;
using elliptic = go.crypto.elliptic_package;
using rand = go.crypto.rand_package;
using fips140tls = go.crypto.tls.@internal.fips140tls_package;
using Δx509 = go.crypto.x509_package;
using pkix = go.crypto.x509.pkix_package;
using pem = encoding.pem_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using ecdsa = go.crypto.ecdsa_package;
using encoding;
using fs = go.io.fs_package;
using go.crypto;
using go.crypto.tls.@internal;
using go.crypto.x509;
using go.os;
using go.sync;
using hash = hash_package;
using path;
using rsa = go.crypto.rsa_package;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

internal static void testClientHello(ж<testing.T> Ꮡt, ж<global::go.crypto.tls_package.Config> ᏑserverConfig, global::go.crypto.tls_package.handshakeMessage m) {
    Ꮡt.Helper();
    testClientHelloFailure(Ꮡt, ᏑserverConfig, m, ""u8);
}

// testFatal is a hack to prevent the compiler from complaining that there is a
// call to t.Fatal from a non-test goroutine
internal static void testFatal(ж<testing.T> Ꮡt, error err) {
    Ꮡt.Helper();
    Ꮡt.Fatal(err);
}

internal static partial void testClientHelloFailure(ж<testing.T> Ꮡt, ж<global::go.crypto.tls_package.Config> ᏑserverConfig, global::go.crypto.tls_package.handshakeMessage m, @string expectedSubStr) {
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var cʗ1 = c;
    goǃ(() => {
        var cli = Client(cʗ1, testConfig);
        {
            var (chΔ1, ok) = m._<ж<global::go.crypto.tls_package.clientHelloMsg>>(ᐧ); if (ok) {
                cli.Value.vers = chΔ1.Value.vers;
            }
        }
        {
            var (_, errΔ1) = cli.writeHandshakeRecord(m, default!); if (errΔ1 != default!) {
                testFatal(Ꮡt, errΔ1);
            }
        }
        cʗ1.Close();
    });
    var ctx = context.Background();
    var conn = Server(s, ᏑserverConfig);
    var (ch, ech, err) = conn.readClientHello(ctx);
    if ((~conn).vers == VersionTLS13){
        ref var hs = ref heap<global::go.crypto.tls_package.serverHandshakeStateTLS13>(out var Ꮡhs);
        hs = new serverHandshakeStateTLS13(
            c: conn,
            ctx: ctx,
            clientHello: ch,
            echContext: ech
        );
        if (err == default!) {
            err = Ꮡhs.processClientHello();
        }
        if (err == default!) {
            err = hs.checkForResumption();
        }
        if (err == default!) {
            err = hs.pickCertificate();
        }
    } else {
        ref var hs = ref heap<global::go.crypto.tls_package.serverHandshakeState>(out var Ꮡhs);
        hs = new serverHandshakeState(
            c: conn,
            ctx: ctx,
            clientHello: ch
        );
        if (err == default!) {
            err = hs.processClientHello();
        }
        if (err == default!) {
            err = Ꮡhs.pickCipherSuite();
        }
    }
    s.Close();
    Ꮡt.Helper();
    if (len(expectedSubStr) == 0){
        if (err != default! && !AreEqual(err, io.EOF)) {
            Ꮡt.Errorf("Got error: %s; expected to succeed"u8, err);
        }
    } else 
    if (err == default! || !strings.Contains(err.Error(), expectedSubStr)) {
        Ꮡt.Errorf("Got error: %v; expected to match substring '%s'"u8, err, expectedSubStr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedHandshakeˢ = "unexpected handshake message"u8;

public static void TestSimpleError(ж<testing.T> Ꮡt) {
    testClientHelloFailure(Ꮡt, testConfig, new global::go.crypto.tls_package.serverHelloDoneMsgжhandshakeMessage(Ꮡ(new serverHelloDoneMsg(nil))), unexpectedHandshakeˢ);
}

internal static slice<uint16> badProtocolVersions = new uint16[]{0x0000, 0x0005, 0x0100, 0x0105, 0x0200, 0x0205, VersionSSL30}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsupportedVersionsˢ = "unsupported versions"u8;

public static void TestRejectBadProtocolVersion(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.MinVersion = VersionSSL30;
    foreach (var (_, vᴛ1) in badProtocolVersions) {
        ref var v = ref heap(new uint16(), out var Ꮡv);
        v = vᴛ1;

        testClientHelloFailure(Ꮡt, config, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(Ꮡ(new clientHelloMsg(
            vers: v,
            random: new slice<byte>(32)
        ))), unsupportedVersionsˢ);
    }
    testClientHelloFailure(Ꮡt, config, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        supportedVersions: badProtocolVersions,
        random: new slice<byte>(32)
    ))), unsupportedVersionsˢ);
}

public static void TestNoSuiteOverlap(ж<testing.T> Ꮡt) {
    var clientHello = Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuites: new uint16[]{0xff00}.slice(),
        compressionMethods: new uint8[]{compressionNone}.slice()
    ));
    testClientHelloFailure(Ꮡt, testConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHello), noCipherSuiteSupportedByˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string clientDoesNotSupportˢ = "client does not support uncompressed connections"u8;

public static void TestNoCompressionOverlap(ж<testing.T> Ꮡt) {
    var clientHello = Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(),
        compressionMethods: new uint8[]{0xff}.slice()
    ));
    testClientHelloFailure(Ꮡt, testConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHello), clientDoesNotSupportˢ);
}

public static void TestNoRC4ByDefault(ж<testing.T> Ꮡt) {
    var clientHello = Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuites: new uint16[]{TLS_RSA_WITH_RC4_128_SHA}.slice(),
        compressionMethods: new uint8[]{compressionNone}.slice()
    ));
    var serverConfig = testConfig.Clone();
    // Reset the enabled cipher suites to nil in order to test the
    // defaults.
    serverConfig.Value.CipherSuites = default!;
    testClientHelloFailure(Ꮡt, serverConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHello), noCipherSuiteSupportedByˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedMessageˢ = "unexpected message"u8;

public static void TestRejectSNIWithTrailingDot(ж<testing.T> Ꮡt) {
    testClientHelloFailure(Ꮡt, testConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        serverName: "foo.com."u8
    ))), unexpectedMessageˢ);
}

public static void TestDontSelectECDSAWithRSAKey(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test that, even when both sides support an ECDSA cipher suite, it
    // won't be selected if the server's private key doesn't support it.
    var clientHello = Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuites: new uint16[]{TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384}.slice(),
        compressionMethods: new uint8[]{compressionNone}.slice(),
        supportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(),
        supportedPoints: new uint8[]{pointFormatUncompressed}.slice()
    ));
    var serverConfig = testConfig.Clone();
    serverConfig.Value.CipherSuites = clientHello.Value.cipherSuites;
    serverConfig.Value.Certificates = new slice<global::go.crypto.tls_package.Certificate>(1);
    (~serverConfig).Certificates[0].ΔCertificate = new slice<byte>[]{testECDSACertificate}.slice();
    (~serverConfig).Certificates[0].PrivateKey = testECDSAPrivateKey.OrTypedNil();
    serverConfig.BuildNameToCertificate();
    // First test that it *does* work when the server's key is ECDSA.
    testClientHello(Ꮡt, serverConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHello));
    // Now test that switching to an RSA key causes the expected error (and
    // not an internal error about a signing failure).
    serverConfig.Value.Certificates = testConfig.Value.Certificates;
    testClientHelloFailure(Ꮡt, serverConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHello), noCipherSuiteSupportedByˢ);
}

public static void TestDontSelectRSAWithECDSAKey(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test that, even when both sides support an RSA cipher suite, it
    // won't be selected if the server's private key doesn't support it.
    var clientHello = Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(),
        compressionMethods: new uint8[]{compressionNone}.slice(),
        supportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(),
        supportedPoints: new uint8[]{pointFormatUncompressed}.slice()
    ));
    var serverConfig = testConfig.Clone();
    serverConfig.Value.CipherSuites = clientHello.Value.cipherSuites;
    // First test that it *does* work when the server's key is RSA.
    testClientHello(Ꮡt, serverConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHello));
    // Now test that switching to an ECDSA key causes the expected error
    // (and not an internal error about a signing failure).
    serverConfig.Value.Certificates = new slice<global::go.crypto.tls_package.Certificate>(1);
    (~serverConfig).Certificates[0].ΔCertificate = new slice<byte>[]{testECDSACertificate}.slice();
    (~serverConfig).Certificates[0].PrivateKey = testECDSAPrivateKey.OrTypedNil();
    serverConfig.BuildNameToCertificate();
    testClientHelloFailure(Ꮡt, serverConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHello), noCipherSuiteSupportedByˢ);
}

public static partial void TestRenegotiationExtension(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    skipFIPS(Ꮡt); // #70505
    var clientHello = Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        compressionMethods: new uint8[]{compressionNone}.slice(),
        random: new slice<byte>(32),
        secureRenegotiationSupported: true,
        cipherSuites: new uint16[]{TLS_RSA_WITH_RC4_128_SHA}.slice()
    ));
    var bufChan = new channel<slice<byte>>(1);
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var bufChanʗ1 = bufChan;
    var cʗ1 = c;
    var clientHelloʗ1 = clientHello;
    goǃ(() => {
        var cli = Client(cʗ1, testConfig);
        cli.Value.vers = clientHelloʗ1.Value.vers;
        {
            var (_, errΔ1) = cli.writeHandshakeRecord(new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHelloʗ1), default!); if (errΔ1 != default!) {
                testFatal(Ꮡt, errΔ1);
            }
        }
        var bufΔ1 = new slice<byte>(1024);
        var (n, err) = cʗ1.Read(bufΔ1);
        if (err != default!) {
            Ꮡt.Errorf("Server read returned error: %s"u8, err);
            return;
        }
        cʗ1.Close();
        bufChanʗ1.ᐸꟷ(bufΔ1.slice(0, n));
    });
    Server(s, testConfig).Handshake();
    var buf = ᐸꟷ(bufChan);
    if (len(buf) < 5 + 4) {
        Ꮡt.Fatalf("Server returned short message of length %d"u8, len(buf));
    }
    // buf contains a TLS record, with a 5 byte record header and a 4 byte
    // handshake header. The length of the ServerHello is taken from the
    // handshake header.
    nint serverHelloLen = (nint)((nint)(((nint)buf[6] << (int)(16)) | ((nint)buf[7] << (int)(8))) | (nint)buf[8]);
    ref var serverHello = ref heap(new global::go.crypto.tls_package.serverHelloMsg(), out var ᏑserverHello);
    // unmarshal expects to be given the handshake header, but
    // serverHelloLen doesn't include it.
    if (!ᏑserverHello.unmarshal(buf.slice(5, 9 + serverHelloLen))) {
        Ꮡt.Fatalf("Failed to parse ServerHello"u8);
    }
    if (!serverHello.secureRenegotiationSupported) {
        Ꮡt.Errorf("Secure renegotiation extension was not echoed."u8);
    }
}

public static partial void TestTLS12OnlyCipherSuites(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    skipFIPS(Ꮡt); // No TLS 1.1 in FIPS mode.
    // Test that a Server doesn't select a TLS 1.2-only cipher suite when
    // the client negotiates TLS 1.1.
    var clientHello = Ꮡ(new clientHelloMsg(
        vers: VersionTLS11,
        random: new slice<byte>(32),
        cipherSuites: new uint16[]{ // The Server, by default, will use the client's
 // preference order. So the GCM cipher suite
 // will be selected unless it's excluded because
 // of the version in this ClientHello.

            TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
            TLS_RSA_WITH_RC4_128_SHA
        }.slice(),
        compressionMethods: new uint8[]{compressionNone}.slice(),
        supportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256, CurveP384, CurveP521}.slice(),
        supportedPoints: new uint8[]{pointFormatUncompressed}.slice()
    ));
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var replyChan = new channel<any>(0);
    var cʗ1 = c;
    var clientHelloʗ1 = clientHello;
    var replyChanʗ1 = replyChan;
    goǃ(() => {
        var cli = Client(cʗ1, testConfig);
        cli.Value.vers = clientHelloʗ1.Value.vers;
        {
            var (_, errΔ1) = cli.writeHandshakeRecord(new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHelloʗ1), default!); if (errΔ1 != default!) {
                testFatal(Ꮡt, errΔ1);
            }
        }
        var (replyΔ1, err) = cli.readHandshake(default!);
        cʗ1.Close();
        if (err != default!){
            replyChanʗ1.ᐸꟷ(err);
        } else {
            replyChanʗ1.ᐸꟷ(replyΔ1);
        }
    });
    var config = testConfig.Clone();
    config.Value.CipherSuites = clientHello.Value.cipherSuites;
    Server(s, config).Handshake();
    s.Close();
    var reply = ᐸꟷ(replyChan);
    {
        var (err, okΔ1) = reply._<error>(ᐧ); if (okΔ1) {
            Ꮡt.Fatal(err);
        }
    }
    var (serverHello, ok) = reply._<ж<global::go.crypto.tls_package.serverHelloMsg>>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("didn't get ServerHello message in reply. Got %v\n"u8, reply);
    }
    {
        var sΔ1 = serverHello.Value.cipherSuite; if (sΔ1 != TLS_RSA_WITH_RC4_128_SHA) {
            Ꮡt.Fatalf("bad cipher suite from server: %x"u8, sΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rsaˢ = "RSA"u8;
internal static readonly object incorrectEcPointFormatˢ = (@string)"incorrect ec_point_format extension from server"u8;

internal partial struct TestTLSPointFormats_tests /*dyn*/ {
    internal @string name;
    internal slice<uint16> cipherSuites;
    internal slice<global::go.crypto.tls_package.CurveID> supportedCurves;
    internal slice<uint8> supportedPoints;
    internal bool wantSupportedPoints;
}

public static void TestTLSPointFormats(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test that a Server returns the ec_point_format extension when ECC is
    // negotiated, and not on a RSA handshake or if ec_point_format is missing.
    var tests = new TestTLSPointFormats_tests[]{
        new("ECC"u8, new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(), new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(), new uint8[]{pointFormatUncompressed}.slice(), true),
        new("ECC without ec_point_format"u8, new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(), new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(), default!, false),
        new("ECC with extra values"u8, new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(), new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(), new uint8[]{13, 37, pointFormatUncompressed, 42}.slice(), true),
        new("RSA"u8, new uint16[]{TLS_RSA_WITH_AES_256_GCM_SHA384}.slice(), default!, default!, false),
        new("RSA with ec_point_format"u8, new uint16[]{TLS_RSA_WITH_AES_256_GCM_SHA384}.slice(), default!, new uint8[]{pointFormatUncompressed}.slice(), false)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestTLSPointFormats_tests(), out var Ꮡtt);
        tt = vᴛ1;

        // The RSA subtests should be enabled for FIPS 140 required mode: #70505
        if (strings.HasPrefix(tt.name, rsaˢ) && fips140tls.Required()) {
            Ꮡt.Logf("skipping in FIPS mode."u8);
            continue;
        }
        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            var clientHello = Ꮡ(new clientHelloMsg(
                vers: VersionTLS12,
                random: new slice<byte>(32),
                cipherSuites: ttʗ1.cipherSuites,
                compressionMethods: new uint8[]{compressionNone}.slice(),
                supportedCurves: ttʗ1.supportedCurves,
                supportedPoints: ttʗ1.supportedPoints
            ));
            var (c, s) = localPipe(new tls_test_package.testing_TжTB(tΔ1));
            var replyChan = new channel<any>(0);
            var cʗ1 = c;
            var clientHelloʗ1 = clientHello;
            var replyChanʗ1 = replyChan;
            goǃ(() => {
                var clientConfig = testConfig.Clone();
                clientConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(ΔCertificate: new slice<byte>[]{testRSA2048Certificate}.slice(), PrivateKey: testRSA2048PrivateKey.OrTypedNil())}.slice();
                var cli = Client(cʗ1, clientConfig);
                cli.Value.vers = clientHelloʗ1.Value.vers;
                {
                    var (_, errΔ1) = cli.writeHandshakeRecord(new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHelloʗ1), default!); if (errΔ1 != default!) {
                        testFatal(tΔ1, errΔ1);
                    }
                }
                var (replyΔ1, err) = cli.readHandshake(default!);
                cʗ1.Close();
                if (err != default!){
                    replyChanʗ1.ᐸꟷ(err);
                } else {
                    replyChanʗ1.ᐸꟷ(replyΔ1);
                }
            });
            var serverConfig = testConfig.Clone();
            serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(ΔCertificate: new slice<byte>[]{testRSA2048Certificate}.slice(), PrivateKey: testRSA2048PrivateKey.OrTypedNil())}.slice();
            serverConfig.Value.CipherSuites = clientHello.Value.cipherSuites;
            Server(s, serverConfig).Handshake();
            s.Close();
            var reply = ᐸꟷ(replyChan);
            {
                var (err, okΔ1) = reply._<error>(ᐧ); if (okΔ1) {
                    tΔ1.Fatal(err);
                }
            }
            var (serverHello, ok) = reply._<ж<global::go.crypto.tls_package.serverHelloMsg>>(ᐧ);
            if (!ok) {
                tΔ1.Fatalf("didn't get ServerHello message in reply. Got %v\n"u8, reply);
            }
            if (ttʗ1.wantSupportedPoints){
                if (!bytes.Equal((~serverHello).supportedPoints, new uint8[]{pointFormatUncompressed}.slice())) {
                    tΔ1.Fatal(incorrectEcPointFormatˢ);
                }
            } else {
                if (len((~serverHello).supportedPoints) != 0) {
                    tΔ1.Fatalf("unexpected ec_point_format extension from server: %v"u8, (~serverHello).supportedPoints);
                }
            }
        });
    }
}

public static partial void TestAlertForwarding(ж<testing.T> Ꮡt) {
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var cʗ1 = c;
    goǃ(() => {
        Client(cʗ1, testConfig).sendAlert(alertUnknownCA);
        cʗ1.Close();
    });
    var err = Server(s, testConfig).Handshake();
    s.Close();
    ref var opErr = ref heap<ж<net.OpError>>(out var ᏑopErr);
    if (!errors.As(err, ᏑopErr) || !AreEqual((~opErr).Err, ((error)alertUnknownCA))) {
        Ꮡt.Errorf("Got error: %s; expected: %s"u8, err, ((error)alertUnknownCA));
    }
}

public static partial void TestClose(ж<testing.T> Ꮡt) {
    var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var cʗ1 = c;
    goǃ(() => cʗ1.Close());
    var err = Server(s, testConfig).Handshake();
    s.Close();
    if (!AreEqual(err, io.EOF)) {
        Ꮡt.Errorf("Got error: %s; expected: %s"u8, err, io.EOF);
    }
}

public static void TestVersion(ж<testing.T> Ꮡt) {
    var serverConfig = Ꮡ(new Config(
        Certificates: (~testConfig).Certificates,
        MaxVersion: VersionTLS13
    ));
    var clientConfig = Ꮡ(new Config(
        InsecureSkipVerify: true,
        MinVersion: VersionTLS12
    ));
    var (state, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("handshake failed: %s"u8, err);
    }
    if (state.Version != VersionTLS13) {
        Ꮡt.Fatalf("incorrect version %x, should be %x"u8, state.Version, (nint)(VersionTLS11));
    }
    clientConfig.Value.MinVersion = 0;
    serverConfig.Value.MaxVersion = VersionTLS11;
    (_, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err == default!) {
        Ꮡt.Fatalf("expected failure to connect with TLS 1.0/1.1"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object theAdvertisedOrderShouldˢ = (@string)"the advertised order should not depend on Config.CipherSuites"u8;
internal static readonly object theAdvertisedTls12Suitesˢ = (@string)"the advertised TLS 1.2 suites should be filtered by Config.CipherSuites"u8;
internal static readonly object thePreferenceOrderShouldˢ = (@string)"the preference order should not depend on Config.CipherSuites"u8;

public static void TestCipherSuitePreference(ж<testing.T> Ꮡt) {
    skipFIPS(Ꮡt); // No RC4 or CHACHA20_POLY1305 in FIPS mode.
    var serverConfig = Ꮡ(new Config(
        CipherSuites: new uint16[]{TLS_RSA_WITH_RC4_128_SHA, TLS_AES_128_GCM_SHA256,
            TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256}.slice(),
        Certificates: (~testConfig).Certificates,
        MaxVersion: VersionTLS12,
        GetConfigForClient: (ж<global::go.crypto.tls_package.ClientHelloInfo> chi) => {
            if ((~chi).CipherSuites[0] != TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256) {
                Ꮡt.Error(theAdvertisedOrderShouldˢ);
            }
            if (len((~chi).CipherSuites) != 2 + len(defaultCipherSuitesTLS13)) {
                Ꮡt.Error(theAdvertisedTls12Suitesˢ);
            }
            return (default!, default!);
        }
    ));
    var clientConfig = Ꮡ(new Config(
        CipherSuites: new uint16[]{TLS_RSA_WITH_AES_128_CBC_SHA, TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256}.slice(),
        InsecureSkipVerify: true
    ));
    var (state, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("handshake failed: %s"u8, err);
    }
    if (state.CipherSuite != TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256) {
        Ꮡt.Error(thePreferenceOrderShouldˢ);
    }
}

public static void TestSCTHandshake(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tlSv12ˢ, (ж<testing.T> tΔ1) => {
        testSCTHandshake(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tlSv13ˢ, (ж<testing.T> tΔ2) => {
        testSCTHandshake(tΔ2, VersionTLS13);
    });
}

internal static void testSCTHandshake(ж<testing.T> Ꮡt, uint16 version) {
    var expected = new slice<byte>[]{slice<byte>("certificate"u8), slice<byte>("transparency"u8)}.slice();
    var serverConfig = Ꮡ(new Config(
        Certificates: new global::go.crypto.tls_package.Certificate[]{new(
            ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
            PrivateKey: testRSAPrivateKey.OrTypedNil(),
            SignedCertificateTimestamps: expected
        )
        }.slice(),
        MaxVersion: version
    ));
    var clientConfig = Ꮡ(new Config(
        InsecureSkipVerify: true
    ));
    var (_, state, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("handshake failed: %s"u8, err);
    }
    var actual = state.SignedCertificateTimestamps;
    if (len(actual) != len(expected)) {
        Ꮡt.Fatalf("got %d scts, want %d"u8, len(actual), len(expected));
    }
    foreach (var (i, sct) in expected) {
        if (!bytes.Equal(sct, actual[i])) {
            Ꮡt.Fatalf("SCT #%d was %x, but expected %x"u8, i, actual[i], sct);
        }
    }
}

public static void TestCrossVersionResume(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tlSv12ˢ, (ж<testing.T> tΔ1) => {
        testCrossVersionResume(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tlSv13ˢ, (ж<testing.T> tΔ2) => {
        testCrossVersionResume(tΔ2, VersionTLS13);
    });
}

internal static void testCrossVersionResume(ж<testing.T> Ꮡt, uint16 version) {
    var serverConfig = Ꮡ(new Config(
        CipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(),
        Certificates: (~testConfig).Certificates,
        Time: testTime
    ));
    var clientConfig = Ꮡ(new Config(
        CipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(),
        InsecureSkipVerify: true,
        ClientSessionCache: NewLRUClientSessionCache(1),
        ServerName: "servername"u8,
        MinVersion: VersionTLS12,
        Time: testTime
    ));
    // Establish a session at TLS 1.3.
    clientConfig.Value.MaxVersion = VersionTLS13;
    var (_, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("handshake failed: %s"u8, err);
    }
    // The client session cache now contains a TLS 1.3 session.
    (var state, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("handshake failed: %s"u8, err);
    }
    if (!state.DidResume) {
        Ꮡt.Fatalf("handshake did not resume at the same version"u8);
    }
    // Test that the server will decline to resume at a lower version.
    clientConfig.Value.MaxVersion = VersionTLS12;
    (state, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("handshake failed: %s"u8, err);
    }
    if (state.DidResume) {
        Ꮡt.Fatalf("handshake resumed at a lower version"u8);
    }
    // The client session cache now contains a TLS 1.2 session.
    (state, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("handshake failed: %s"u8, err);
    }
    if (!state.DidResume) {
        Ꮡt.Fatalf("handshake did not resume at the same version"u8);
    }
    // Test that the server will decline to resume at a higher version.
    clientConfig.Value.MaxVersion = VersionTLS13;
    (state, _, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatalf("handshake failed: %s"u8, err);
    }
    if (state.DidResume) {
        Ꮡt.Fatalf("handshake resumed at a higher version"u8);
    }
}

// Note: see comment in handshake_test.go for details of how the reference
// tests work.

// serverTest represents a test of the TLS server handshake against a reference
// implementation.
internal partial struct serverTest {
    // name is a freeform string identifying the test and the file in which
    // the expected results will be stored.
    internal @string name;
    // command, if not empty, contains a series of arguments for the
    // command to run for the reference server.
    internal slice<@string> command;
    // expectedPeerCerts contains a list of PEM blocks of expected
    // certificates from the client.
    internal slice<@string> expectedPeerCerts;
    // config, if not nil, contains a custom Config to use for this test.
    internal ж<global::go.crypto.tls_package.Config> config;
    // expectHandshakeErrorIncluding, when not empty, contains a string
    // that must be a substring of the error resulting from the handshake.
    internal @string expectHandshakeErrorIncluding;
    // validate, if not nil, is a function that will be called with the
    // ConnectionState of the resulting connection. It returns false if the
    // ConnectionState is unacceptable.
    internal Func<global::go.crypto.tls_package.ΔConnectionState, error> validate;
    // wait, if true, prevents this subtest from calling t.Parallel.
    // If false, runServerTest* returns immediately.
    internal bool wait;
}

internal static slice<@string> defaultClientCommand = new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timedOutWaitingForˢ2 = "timed out waiting for connection from child process"u8;

// connFromCommand starts opens a listening socket and starts the reference
// client to connect to it. It returns a recordingConn that wraps the resulting
// connection.
internal static partial (ж<recordingConn> conn, ж<exec.Cmd> child, error err) connFromCommand(this ж<serverTest> Ꮡtest) {
    ж<recordingConn> conn = default!;
    ж<exec.Cmd> child = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var test = ref Ꮡtest.DerefOrNull();

        (var l, err) = net.ListenTCP(tcpˢ, Ꮡ(new net.TCPAddr(
            IP: net.IPv4(127, 0, 0, 1),
            Port: 0
        )));
        if (err != default!) {
            (conn, child, err) = (default!, default!, err); goto ᒐdone;
        }
        var lʗ1 = l;
        defer(() => lʗ1.Close(), ref ᒐ);
        nint port = l.Addr()._<ж<net.TCPAddr>>().Value.Port;
        slice<@string> command = default!;
        command = appendꓸꓸꓸ(command, test.command);
        if (len(command) == 0) {
            command = defaultClientCommand;
        }
        command = append(command, "-connect"u8);
        command = append(command, fmt.Sprintf("127.0.0.1:%d"u8, port));
        var cmd = exec.Command(command[0], command[1..].ꓸꓸꓸ);
        cmd.Value.Stdin = default!;
        ref var output = ref heap(new bytes.Buffer(), out var Ꮡoutput);
        cmd.Value.Stdout = new tls_test_package.bytes_BufferжWriter(Ꮡoutput);
        cmd.Value.Stderr = new tls_test_package.bytes_BufferжWriter(Ꮡoutput);
        {
            var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                (conn, child, err) = (default!, default!, errΔ1); goto ᒐdone;
            }
        }
        var connChan = new channel<any>(1);
        var connChanʗ1 = connChan;
        var lʗ2 = l;
        goǃ(() => {
            var (tcpConnΔ1, errΔ2) = lʗ2.Accept();
            if (errΔ2 != default!) {
                connChanʗ1.ᐸꟷ(errΔ2);
                return;
            }
            connChanʗ1.ᐸꟷ(tcpConnΔ1);
        });
        net.Conn tcpConn = default!;
        var selᴛ2 = connChan;
        var selᴛ3 = time_package.After(2 * time_package.ΔSecond);
        switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
        case 0 when selᴛ2.ꟷᐳ(out var connOrError): {
            {
                var (errΔ3, ok) = connOrError._<error>(ᐧ); if (ok) {
                    (conn, child, err) = (default!, default!, errΔ3); goto ᒐdone;
                }
            }
            tcpConn = connOrError._<net.Conn>();
            break;
        }
        case 1 when selᴛ3.ꟷᐳ(out _): {
            (conn, child, err) = (default!, default!, errors.New(timedOutWaitingForˢ2)); goto ᒐdone;
        }}
        var record = Ꮡ(new recordingConn(
            Conn: tcpConn
        ));
        (conn, child, err) = (record, cmd, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (conn, child, err);
}

internal static @string dataPath(this ref serverTest test) {
    return filepath.Join(testdataˢ, "Server-" + test.name);
}

internal static (slice<slice<byte>> flows, error err) loadData(this ж<serverTest> Ꮡtest) {
    slice<slice<byte>> flows = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var test = ref Ꮡtest.DerefOrNull();

        (var @in, err) = os.Open(test.dataPath());
        if (err != default!) {
            (flows, err) = (default!, err); goto ᒐdone;
        }
        var inʗ1 = @in;
        defer(() => inʗ1.Close(), ref ᒐ);
        (flows, err) = parseTestData(new tls_test_package.os_FileжReader(@in));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (flows, err);
}

internal static void run(this ж<serverTest> Ꮡtest, ж<testing.T> Ꮡt, bool write) {
    GoFrame ᒐ = default;
    try {
        ref var test = ref Ꮡtest.DerefOrNull();

        net.Conn serverConn = default!;
        ж<recordingConn> recordingConn = default!;
        ж<exec.Cmd> childProcess = default!;
        if (write){
            error errΔ1 = default!;
            (recordingConn, childProcess, errΔ1) = Ꮡtest.connFromCommand();
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("Failed to start subcommand: %s"u8, errΔ1);
            }
            serverConn = new tls_internal_test_package.recordingConnжConn(recordingConn);
            var childProcessʗ1 = childProcess;
            defer(() => {
                if (Ꮡt.Failed()) {
                    Ꮡt.Logf("OpenSSL output:\n\n%s"u8, (~childProcessʗ1).Stdout);
                }
            }, ref ᒐ);
        } else {
            var (flows, errΔ2) = Ꮡtest.loadData();
            if (errΔ2 != default!) {
                Ꮡt.Fatalf("Failed to load data from %s"u8, test.dataPath());
            }
            serverConn = new tls_internal_test_package.replayingConnжConn(Ꮡ(new replayingConn(t: new tls_test_package.testing_TжTB(Ꮡt), flows: flows, reading: true)));
        }
        var config = test.config;
        if (config == nil) {
            config = testConfig;
        }
        var server = Server(serverConn, config);
        var (_, err) = server.Write(slice<byte>("hello, world\n"u8));
        if (len(test.expectHandshakeErrorIncluding) > 0){
            if (err == default!){
                Ꮡt.Errorf("Error expected, but no error returned"u8);
            } else 
            {
                @string s = err.Error(); if (!strings.Contains(s, test.expectHandshakeErrorIncluding)) {
                    Ꮡt.Errorf("Error expected containing '%s' but got '%s'"u8, test.expectHandshakeErrorIncluding, s);
                }
            }
        } else {
            if (err != default!) {
                Ꮡt.Logf("Error from Server.Write: '%s'"u8, err);
            }
        }
        server.Close();
        var connState = server.ConnectionState();
        var peerCerts = connState.PeerCertificates;
        if (len(peerCerts) == len(test.expectedPeerCerts)){
            foreach (var (i, peerCert) in peerCerts) {
                var (block, _) = pem.Decode(slice<byte>(test.expectedPeerCerts[i]));
                if (!bytes.Equal((~block).Bytes, (~peerCert).Raw)) {
                    Ꮡt.Fatalf("%s: mismatch on peer cert %d"u8, test.name, i + 1);
                }
            }
        } else {
            Ꮡt.Fatalf("%s: mismatch on peer list length: %d (wanted) != %d (got)"u8, test.name, len(test.expectedPeerCerts), len(peerCerts));
        }
        if (test.validate != default!) {
            {
                var errΔ3 = test.validate(connState); if (errΔ3 != default!) {
                    Ꮡt.Fatalf("validate callback returned error: %s"u8, errΔ3);
                }
            }
        }
        if (write) {
            serverConn.Close();
            @string path = test.dataPath();
            var (@out, errΔ4) = os.OpenFile(path, (nint)((nint)(nint)(os.O_WRONLY | os.O_CREATE) | os.O_TRUNC), 420);
            if (errΔ4 != default!) {
                Ꮡt.Fatalf("Failed to create output file: %s"u8, errΔ4);
            }
            var outʗ1 = @out;
            defer(() => outʗ1.Close(), ref ᒐ);
            (~recordingConn).Conn.Close();
            if (len((~recordingConn).flows) < 3) {
                if (len(test.expectHandshakeErrorIncluding) == 0) {
                    Ꮡt.Fatalf("Handshake failed"u8);
                }
            }
            recordingConn.WriteTo(new os.FileжWriter(@out));
            Ꮡt.Logf("Wrote %s\n"u8, path);
            childProcess.Wait();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void runServerTestForVersion(ж<testing.T> Ꮡt, ж<serverTest> Ꮡtemplate, @string version, @string option) {
    ref var template = ref Ꮡtemplate.DerefOrNull();

    // Make a deep copy of the template before going parallel.
    ref var test = ref heap<serverTest>(out var Ꮡtest);
    test = template;
    if (template.config != nil) {
        test.config = template.config.Clone();
    }
    test.name = version + "-"u8 + test.name;
    if (len(test.command) == 0) {
        test.command = defaultClientCommand;
    }
    test.command = appendꓸꓸꓸ(slice<@string>(default!), test.command);
    test.command = append(test.command, option);
    runTestAndUpdateIfNeeded(Ꮡt, version, Ꮡtest.run, test.wait);
}

internal static void runServerTestTLS10(ж<testing.T> Ꮡt, ж<serverTest> Ꮡtemplate) {
    runServerTestForVersion(Ꮡt, Ꮡtemplate, tlSv10ˢ, tls1ˢ);
}

internal static void runServerTestTLS11(ж<testing.T> Ꮡt, ж<serverTest> Ꮡtemplate) {
    runServerTestForVersion(Ꮡt, Ꮡtemplate, tlSv11ˢ, tls11ˢ2);
}

internal static void runServerTestTLS12(ж<testing.T> Ꮡt, ж<serverTest> Ꮡtemplate) {
    runServerTestForVersion(Ꮡt, Ꮡtemplate, tlSv12ˢ, tls12ˢ2);
}

internal static void runServerTestTLS13(ж<testing.T> Ꮡt, ж<serverTest> Ꮡtemplate) {
    runServerTestForVersion(Ꮡt, Ꮡtemplate, tlSv13ˢ, tls13ˢ2);
}

public static void TestHandshakeServerRSARC4(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new serverTest(
        name: "RSA-RC4"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "RC4-SHA"u8}.slice()
    ));
    runServerTestTLS10(Ꮡt, test);
    runServerTestTLS11(Ꮡt, test);
    runServerTestTLS12(Ꮡt, test);
}

public static void TestHandshakeServerRSA3DES(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new serverTest(
        name: "RSA-3DES"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "DES-CBC3-SHA"u8}.slice()
    ));
    runServerTestTLS10(Ꮡt, test);
    runServerTestTLS12(Ꮡt, test);
}

public static void TestHandshakeServerRSAAES(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new serverTest(
        name: "RSA-AES"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "AES128-SHA"u8}.slice()
    ));
    runServerTestTLS10(Ꮡt, test);
    runServerTestTLS12(Ꮡt, test);
}

public static void TestHandshakeServerAESGCM(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new serverTest(
        name: "RSA-AES-GCM"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "ECDHE-RSA-AES128-GCM-SHA256"u8}.slice()
    ));
    runServerTestTLS12(Ꮡt, test);
}

public static void TestHandshakeServerAES256GCMSHA384(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new serverTest(
        name: "RSA-AES256-GCM-SHA384"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "ECDHE-RSA-AES256-GCM-SHA384"u8}.slice()
    ));
    runServerTestTLS12(Ꮡt, test);
}

public static void TestHandshakeServerAES128SHA256(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new serverTest(
        name: "AES128-SHA256"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-ciphersuites"u8, "TLS_AES_128_GCM_SHA256"u8}.slice()
    ));
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerAES256SHA384(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new serverTest(
        name: "AES256-SHA384"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-ciphersuites"u8, "TLS_AES_256_GCM_SHA384"u8}.slice()
    ));
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerCHACHA20SHA256(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new serverTest(
        name: "CHACHA20-SHA256"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8}.slice()
    ));
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerECDHEECDSAAES(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.Certificates = new slice<global::go.crypto.tls_package.Certificate>(1);
    (~config).Certificates[0].ΔCertificate = new slice<byte>[]{testECDSACertificate}.slice();
    (~config).Certificates[0].PrivateKey = testECDSAPrivateKey.OrTypedNil();
    config.BuildNameToCertificate();
    var test = Ꮡ(new serverTest(
        name: "ECDHE-ECDSA-AES"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "ECDHE-ECDSA-AES256-SHA"u8, "-ciphersuites"u8, "TLS_AES_128_GCM_SHA256"u8}.slice(),
        config: config
    ));
    runServerTestTLS10(Ꮡt, test);
    runServerTestTLS12(Ꮡt, test);
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerX25519(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{X25519}.slice();
    var test = Ꮡ(new serverTest(
        name: "X25519"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "ECDHE-RSA-CHACHA20-POLY1305"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8, "-curves"u8, "X25519"u8}.slice(),
        config: config
    ));
    runServerTestTLS12(Ꮡt, test);
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerP256(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice();
    var test = Ꮡ(new serverTest(
        name: "P256"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "ECDHE-RSA-CHACHA20-POLY1305"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8, "-curves"u8, "P-256"u8}.slice(),
        config: config
    ));
    runServerTestTLS12(Ꮡt, test);
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerHelloRetryRequest(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice();
    var test = Ꮡ(new serverTest(
        name: "HelloRetryRequest"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8, "-curves"u8, "X25519:P-256"u8}.slice(),
        config: config,
        validate: error (global::go.crypto.tls_package.ΔConnectionState cs) => {
            if (!cs.testingOnlyDidHRR) {
                return errors.New(expectedˢ);
            }
            return default!;
        }
    ));
    runServerTestTLS13(Ꮡt, test);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedˢ = "unexpected HelloRetryRequest"u8;

// TestHandshakeServerKeySharePreference checks that we prefer a key share even
// if it's later in the CurvePreferences order.
public static void TestHandshakeServerKeySharePreference(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{X25519, CurveP256}.slice();
    var test = Ꮡ(new serverTest(
        name: "KeySharePreference"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8, "-curves"u8, "P-256:X25519"u8}.slice(),
        config: config,
        validate: error (global::go.crypto.tls_package.ΔConnectionState cs) => {
            if (cs.testingOnlyDidHRR) {
                return errors.New(unexpectedˢ);
            }
            return default!;
        }
    ));
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerALPN(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.NextProtos = new @string[]{"proto1"u8, "proto2"u8}.slice();
    var test = Ꮡ(new serverTest(
        name: "ALPN"u8, // Note that this needs OpenSSL 1.0.2 because that is the first
 // version that supports the -alpn flag.

        command: new @string[]{"openssl"u8, "s_client"u8, "-alpn"u8, "proto2,proto1"u8, "-cipher"u8, "ECDHE-RSA-CHACHA20-POLY1305"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8}.slice(),
        config: config,
        validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
            // The server's preferences should override the client.
            if (state.NegotiatedProtocol != "proto1"u8) {
                return fmt.Errorf("Got protocol %q, wanted proto1"u8, state.NegotiatedProtocol);
            }
            return default!;
        }
    ));
    runServerTestTLS12(Ꮡt, test);
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerALPNNoMatch(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.NextProtos = new @string[]{"proto3"u8}.slice();
    var test = Ꮡ(new serverTest(
        name: "ALPN-NoMatch"u8, // Note that this needs OpenSSL 1.0.2 because that is the first
 // version that supports the -alpn flag.

        command: new @string[]{"openssl"u8, "s_client"u8, "-alpn"u8, "proto2,proto1"u8, "-cipher"u8, "ECDHE-RSA-CHACHA20-POLY1305"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8}.slice(),
        config: config,
        expectHandshakeErrorIncluding: "client requested unsupported application protocol"u8
    ));
    runServerTestTLS12(Ꮡt, test);
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerALPNNotConfigured(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.NextProtos = default!;
    var test = Ꮡ(new serverTest(
        name: "ALPN-NotConfigured"u8, // Note that this needs OpenSSL 1.0.2 because that is the first
 // version that supports the -alpn flag.

        command: new @string[]{"openssl"u8, "s_client"u8, "-alpn"u8, "proto2,proto1"u8, "-cipher"u8, "ECDHE-RSA-CHACHA20-POLY1305"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8}.slice(),
        config: config,
        validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
            if (state.NegotiatedProtocol != ""u8) {
                return fmt.Errorf("Got protocol %q, wanted nothing"u8, state.NegotiatedProtocol);
            }
            return default!;
        }
    ));
    runServerTestTLS12(Ꮡt, test);
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerALPNFallback(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.NextProtos = new @string[]{"proto1"u8, "h2"u8, "proto2"u8}.slice();
    var test = Ꮡ(new serverTest(
        name: "ALPN-Fallback"u8, // Note that this needs OpenSSL 1.0.2 because that is the first
 // version that supports the -alpn flag.

        command: new @string[]{"openssl"u8, "s_client"u8, "-alpn"u8, "proto3,http/1.1,proto4"u8, "-cipher"u8, "ECDHE-RSA-CHACHA20-POLY1305"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8}.slice(),
        config: config,
        validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
            if (state.NegotiatedProtocol != ""u8) {
                return fmt.Errorf("Got protocol %q, wanted nothing"u8, state.NegotiatedProtocol);
            }
            return default!;
        }
    ));
    runServerTestTLS12(Ꮡt, test);
    runServerTestTLS13(Ꮡt, test);
}

// TestHandshakeServerSNI involves a client sending an SNI extension of
// "snitest.com", which happens to match the CN of testSNICertificate. The test
// verifies that the server correctly selects that certificate.
public static void TestHandshakeServerSNI(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new serverTest(
        name: "SNI"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "AES128-SHA"u8, "-servername"u8, "snitest.com"u8}.slice()
    ));
    runServerTestTLS12(Ꮡt, test);
}

// TestHandshakeServerSNIGetCertificate is similar to TestHandshakeServerSNI, but
// tests the dynamic GetCertificate method
public static void TestHandshakeServerSNIGetCertificate(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    // Replace the NameToCertificate map with a GetCertificate function
    var nameToCert = config.Value.NameToCertificate;
    config.Value.NameToCertificate = default!;
    var nameToCertʗ1 = nameToCert;
    config.Value.GetCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHello) => {
        var cert = nameToCertʗ1[(~clientHello).ServerName];
        return (cert, default!);
    };
    var test = Ꮡ(new serverTest(
        name: "SNI-GetCertificate"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "AES128-SHA"u8, "-servername"u8, "snitest.com"u8}.slice(),
        config: config
    ));
    runServerTestTLS12(Ꮡt, test);
}

// TestHandshakeServerSNIGetCertificateNotFound is similar to
// TestHandshakeServerSNICertForName, but tests to make sure that when the
// GetCertificate method doesn't return a cert, we fall back to what's in
// the NameToCertificate map.
public static void TestHandshakeServerSNIGetCertificateNotFound(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.GetCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHello) => (default!, default!);
    var test = Ꮡ(new serverTest(
        name: "SNI-GetCertificateNotFound"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "AES128-SHA"u8, "-servername"u8, "snitest.com"u8}.slice(),
        config: config
    ));
    runServerTestTLS12(Ꮡt, test);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedOurˢ = (@string)"expected our GetCertificate test to be called twice"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string errMsgᶜ = "TestHandshakeServerGetCertificateExtensions error"u8;

// TestHandshakeServerGetCertificateExtensions tests to make sure that the
// Extensions passed to GetCertificate match what we expect based on the
// clientHelloMsg
public static void TestHandshakeServerGetCertificateExtensions(ж<testing.T> Ꮡt) {
    @string errMsg = errMsgᶜ;
    // ensure the test condition inside our GetCertificate callback
    // is actually invoked
    ref var called = ref heap(new atomic.Int32(), out var Ꮡcalled);
    var testVersions = new uint16[]{VersionTLS12, VersionTLS13}.slice();
    foreach (var (_, vᴛ1) in testVersions) {
        ref var vers = ref heap(new uint16(), out var Ꮡvers);
        vers = vᴛ1;

        var versʗ1 = vers;
        Ꮡt.Run(fmt.Sprintf("TLS version %04x"u8, vers), (ж<testing.T> tΔ1) => {
            var (pk, _) = ecdh.P256().GenerateKey(go.crypto.rand_package.Reader);
            var clientHello = Ꮡ(new clientHelloMsg(
                vers: versʗ1,
                random: new slice<byte>(32),
                cipherSuites: new uint16[]{TLS_AES_128_GCM_SHA256}.slice(),
                compressionMethods: new uint8[]{compressionNone}.slice(),
                serverName: "test"u8,
                keyShares: new global::go.crypto.tls_package.keyShare[]{new(group: CurveP256, data: pk.PublicKey().Bytes())}.slice(),
                supportedCurves: new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice(),
                supportedSignatureAlgorithms: new global::go.crypto.tls_package.SignatureScheme[]{ECDSAWithP256AndSHA256}.slice()
            ));
            // the clientHelloMsg initialized just above is serialized with
            // two extensions: server_name(0) and application_layer_protocol_negotiation(16)
            ref var expectedExtensions = ref heap<slice<uint16>>(out var ᏑexpectedExtensions);
            ᏑexpectedExtensions.ValueSlot = new uint16[]{
                extensionServerName,
                extensionSupportedCurves,
                extensionSignatureAlgorithms,
                extensionKeyShare
            }.slice();
            if (versʗ1 == VersionTLS13) {
                clientHello.Value.supportedVersions = new uint16[]{VersionTLS13}.slice();
                ᏑexpectedExtensions.ValueSlot = append(ᏑexpectedExtensions.ValueSlot, extensionSupportedVersions);
            }
            // Go's TLS client presents extensions in the ClientHello sorted by extension ID
            slices.Sort<slice<uint16>, uint16>(ᏑexpectedExtensions.ValueSlot);
            var serverConfig = testConfig.Clone();
            serverConfig.Value.GetCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHelloΔ1) => {
                if (!slices.Equal<slice<uint16>, uint16>(ᏑexpectedExtensions.ValueSlot, (~clientHelloΔ1).Extensions)) {
                    tΔ1.Errorf("expected extensions on ClientHelloInfo (%v) to match clientHelloMsg (%v)"u8, ᏑexpectedExtensions.ValueSlot, (~clientHelloΔ1).Extensions);
                }
                Ꮡcalled.Add(1);
                return (default!, errors.New(errMsg));
            };
            testClientHelloFailure(tΔ1, serverConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHello), errMsg);
        });
    }
    if ((nint)Ꮡcalled.Load() != len(testVersions)) {
        Ꮡt.Error(expectedOurˢ);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string errMsgᶜ1 = "TestHandshakeServerSNIGetCertificateError error"u8;

// TestHandshakeServerSNIGetCertificateError tests to make sure that errors in
// GetCertificate result in a tls alert.
public static void TestHandshakeServerSNIGetCertificateError(ж<testing.T> Ꮡt) {
    @string errMsg = errMsgᶜ1;
    var serverConfig = testConfig.Clone();
    serverConfig.Value.GetCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHelloΔ1) => (default!, errors.New(errMsg));
    var clientHello = Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(),
        compressionMethods: new uint8[]{compressionNone}.slice(),
        serverName: "test"u8
    ));
    testClientHelloFailure(Ꮡt, serverConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHello), errMsg);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noCertificatesˢ = "no certificates"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string errMsgᶜ2 = "TestHandshakeServerEmptyCertificates error"u8;

// TestHandshakeServerEmptyCertificates tests that GetCertificates is called in
// the case that Certificates is empty, even without SNI.
public static void TestHandshakeServerEmptyCertificates(ж<testing.T> Ꮡt) {
    @string errMsg = errMsgᶜ2;
    var serverConfig = testConfig.Clone();
    serverConfig.Value.GetCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHelloΔ1) => (default!, errors.New(errMsg));
    serverConfig.Value.Certificates = default!;
    var clientHello = Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(),
        compressionMethods: new uint8[]{compressionNone}.slice()
    ));
    testClientHelloFailure(Ꮡt, serverConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHello), errMsg);
    // With an empty Certificates and a nil GetCertificate, the server
    // should always return a “no certificates” error.
    serverConfig.Value.GetCertificate = default!;
    clientHello = Ꮡ(new clientHelloMsg(
        vers: VersionTLS12,
        random: new slice<byte>(32),
        cipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(),
        compressionMethods: new uint8[]{compressionNone}.slice()
    ));
    testClientHelloFailure(Ꮡt, serverConfig, new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHello), noCertificatesˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string didNotResumeˢ = "did not resume"u8;

public static void TestServerResumption(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string sessionFilePath = tempFile(""u8);
        defer(os.Remove, sessionFilePath, ref ᒐ);
        var testIssue = Ꮡ(new serverTest(
            name: "IssueTicket"u8,
            command: new @string[]{"openssl"u8, "s_client"u8, "-cipher"u8, "AES128-SHA"u8, "-ciphersuites"u8, "TLS_AES_128_GCM_SHA256"u8, "-sess_out"u8, sessionFilePath}.slice(),
            wait: true
        ));
        var testResume = Ꮡ(new serverTest(
            name: "Resume"u8,
            command: new @string[]{"openssl"u8, "s_client"u8, "-cipher"u8, "AES128-SHA"u8, "-ciphersuites"u8, "TLS_AES_128_GCM_SHA256"u8, "-sess_in"u8, sessionFilePath}.slice(),
            validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
                if (!state.DidResume) {
                    return errors.New(didNotResumeˢ);
                }
                return default!;
            }
        ));
        runServerTestTLS12(Ꮡt, testIssue);
        runServerTestTLS12(Ꮡt, testResume);
        runServerTestTLS13(Ꮡt, testIssue);
        runServerTestTLS13(Ꮡt, testResume);
        var config = testConfig.Clone();
        config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{CurveP256}.slice();
        var testResumeHRR = Ꮡ(new serverTest(
            name: "Resume-HelloRetryRequest"u8,
            command: new @string[]{"openssl"u8, "s_client"u8, "-curves"u8, "X25519:P-256"u8, "-cipher"u8, "AES128-SHA"u8, "-ciphersuites"u8,
                "TLS_AES_128_GCM_SHA256"u8, "-sess_in"u8, sessionFilePath}.slice(),
            config: config,
            validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
                if (!state.DidResume) {
                    return errors.New(didNotResumeˢ);
                }
                return default!;
            }
        ));
        runServerTestTLS13(Ꮡt, testResumeHRR);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string resumedWithˢ = "resumed with SessionTicketsDisabled"u8;

public static void TestServerResumptionDisabled(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string sessionFilePath = tempFile(""u8);
        defer(os.Remove, sessionFilePath, ref ᒐ);
        var config = testConfig.Clone();
        var testIssue = Ꮡ(new serverTest(
            name: "IssueTicketPreDisable"u8,
            command: new @string[]{"openssl"u8, "s_client"u8, "-cipher"u8, "AES128-SHA"u8, "-ciphersuites"u8, "TLS_AES_128_GCM_SHA256"u8, "-sess_out"u8, sessionFilePath}.slice(),
            config: config,
            wait: true
        ));
        var testResume = Ꮡ(new serverTest(
            name: "ResumeDisabled"u8,
            command: new @string[]{"openssl"u8, "s_client"u8, "-cipher"u8, "AES128-SHA"u8, "-ciphersuites"u8, "TLS_AES_128_GCM_SHA256"u8, "-sess_in"u8, sessionFilePath}.slice(),
            config: config,
            validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
                if (state.DidResume) {
                    return errors.New(resumedWithˢ);
                }
                return default!;
            }
        ));
        config.Value.SessionTicketsDisabled = false;
        runServerTestTLS12(Ꮡt, testIssue);
        config.Value.SessionTicketsDisabled = true;
        runServerTestTLS12(Ꮡt, testResume);
        config.Value.SessionTicketsDisabled = false;
        runServerTestTLS13(Ꮡt, testIssue);
        config.Value.SessionTicketsDisabled = true;
        runServerTestTLS13(Ꮡt, testResume);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFallbackSCSV(ж<testing.T> Ꮡt) {
    ref var serverConfig = ref heap<global::go.crypto.tls_package.Config>(out var ᏑserverConfig);
    serverConfig = new Config(
        Certificates: (~testConfig).Certificates,
        MinVersion: VersionTLS11
    );
    var test = Ꮡ(new serverTest(
        name: "FallbackSCSV"u8,
        config: ᏑserverConfig, // OpenSSL 1.0.1j is needed for the -fallback_scsv option.

        command: new @string[]{"openssl"u8, "s_client"u8, "-fallback_scsv"u8}.slice(),
        expectHandshakeErrorIncluding: "inappropriate protocol fallback"u8
    ));
    runServerTestTLS11(Ꮡt, test);
}

public static void TestHandshakeServerExportKeyingMaterial(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new serverTest(
        name: "ExportKeyingMaterial"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-cipher"u8, "ECDHE-RSA-AES256-SHA"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8}.slice(),
        config: testConfig.Clone(),
        validate: error (global::go.crypto.tls_package.ΔConnectionState state) => {
            {
                var (km, err) = state.ExportKeyingMaterial(testˢ, default!, 42); if (err != default!){
                    return fmt.Errorf("ExportKeyingMaterial failed: %v"u8, err);
                } else 
                if (len(km) != 42) {
                    return fmt.Errorf("Got %d bytes from ExportKeyingMaterial, wanted %d"u8, len(km), (nint)(42));
                }
            }
            return default!;
        }
    ));
    runServerTestTLS10(Ꮡt, test);
    runServerTestTLS12(Ꮡt, test);
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerRSAPKCS1v15(ж<testing.T> Ꮡt) {
    var test = Ꮡ(new serverTest(
        name: "RSA-RSAPKCS1v15"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "ECDHE-RSA-CHACHA20-POLY1305"u8, "-sigalgs"u8, "rsa_pkcs1_sha256"u8}.slice()
    ));
    runServerTestTLS12(Ꮡt, test);
}

public static void TestHandshakeServerRSAPSS(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // We send rsa_pss_rsae_sha512 first, as the test key won't fit, and we
    // verify the server implementation will disregard the client preference in
    // that case. See Issue 29793.
    var test = Ꮡ(new serverTest(
        name: "RSA-RSAPSS"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "ECDHE-RSA-CHACHA20-POLY1305"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8, "-sigalgs"u8, "rsa_pss_rsae_sha512:rsa_pss_rsae_sha256"u8}.slice()
    ));
    runServerTestTLS12(Ꮡt, test);
    runServerTestTLS13(Ꮡt, test);
    test = Ꮡ(new serverTest(
        name: "RSA-RSAPSS-TooSmall"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8, "-sigalgs"u8, "rsa_pss_rsae_sha512"u8}.slice(),
        expectHandshakeErrorIncluding: "peer doesn't support any of the certificate's signature algorithms"u8
    ));
    runServerTestTLS13(Ꮡt, test);
}

public static void TestHandshakeServerEd25519(ж<testing.T> Ꮡt) {
    var config = testConfig.Clone();
    config.Value.Certificates = new slice<global::go.crypto.tls_package.Certificate>(1);
    (~config).Certificates[0].ΔCertificate = new slice<byte>[]{testEd25519Certificate}.slice();
    (~config).Certificates[0].PrivateKey = testEd25519PrivateKey;
    config.BuildNameToCertificate();
    var test = Ꮡ(new serverTest(
        name: "Ed25519"u8,
        command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "ECDHE-ECDSA-CHACHA20-POLY1305"u8, "-ciphersuites"u8, "TLS_CHACHA20_POLY1305_SHA256"u8}.slice(),
        config: config
    ));
    runServerTestTLS12(Ꮡt, test);
    runServerTestTLS13(Ꮡt, test);
}

internal static partial void benchmarkHandshakeServer(ж<testing.B> Ꮡb, uint16 version, uint16 cipherSuite, global::go.crypto.tls_package.CurveID curve, slice<byte> cert, cryptoꓸPrivateKey key) {
    ref var b = ref Ꮡb.DerefOrNull();

    var config = testConfig.Clone();
    config.Value.CipherSuites = new uint16[]{cipherSuite}.slice();
    config.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{curve}.slice();
    config.Value.Certificates = new slice<global::go.crypto.tls_package.Certificate>(1);
    (~config).Certificates[0].ΔCertificate = new slice<byte>[]{cert}.slice();
    (~config).Certificates[0].PrivateKey = key;
    config.BuildNameToCertificate();
    var (clientConn, serverConn) = localPipe(new tls_test_package.testing_BжTB(Ꮡb));
    serverConn = new tls_internal_test_package.recordingConnжConn(Ꮡ(new recordingConn(Conn: serverConn)));
    var clientConnʗ1 = clientConn;
    goǃ(() => {
        var configΔ1 = testConfig.Clone();
        configΔ1.Value.MaxVersion = version;
        configΔ1.Value.CurvePreferences = new global::go.crypto.tls_package.CurveID[]{curve}.slice();
        var client = Client(clientConnʗ1, configΔ1);
        client.Handshake();
    });
    var server = Server(serverConn, config);
    {
        var err = server.Handshake(); if (err != default!) {
            Ꮡb.Fatalf("handshake failed: %v"u8, err);
        }
    }
    serverConn.Close();
    var flows = serverConn._<ж<recordingConn>>().Value.flows;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var replay = Ꮡ(new replayingConn(t: new tls_test_package.testing_BжTB(Ꮡb), flows: slices.Clone<slice<slice<byte>>, slice<byte>>(flows), reading: true));
        var serverΔ1 = Server(new tls_internal_test_package.replayingConnжConn(replay), config);
        {
            var err = serverΔ1.Handshake(); if (err != default!) {
                Ꮡb.Fatalf("handshake failed: %v"u8, err);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdheP256Rsaˢ = "ECDHE-P256-RSA"u8;
internal static readonly @string ecdheP256EcdsaP256ˢ = "ECDHE-P256-ECDSA-P256"u8;
internal static readonly @string ecdheX25519EcdsaP256ˢ = "ECDHE-X25519-ECDSA-P256"u8;
internal static readonly @string ecdheP521EcdsaP521ˢ = "ECDHE-P521-ECDSA-P521"u8;
internal static readonly object testEcdsaKeyDoesnTUseˢ = (@string)"test ECDSA key doesn't use curve P-521"u8;

public static void BenchmarkHandshakeServer(ж<testing.B> Ꮡb) {
    Ꮡb.Run(rsaˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ1) => {
        benchmarkHandshakeServer(bΔ1, VersionTLS12, TLS_RSA_WITH_AES_128_GCM_SHA256,
            0, testRSACertificate, testRSAPrivateKey.OrTypedNil());
    });
    Ꮡb.Run(ecdheP256Rsaˢ, (ж<testing.B> bΔ2) => {
        bΔ2.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ3) => {
            benchmarkHandshakeServer(bΔ3, VersionTLS13, TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,
                CurveP256, testRSACertificate, testRSAPrivateKey.OrTypedNil());
        });
        bΔ2.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ4) => {
            benchmarkHandshakeServer(bΔ4, VersionTLS12, TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,
                CurveP256, testRSACertificate, testRSAPrivateKey.OrTypedNil());
        });
    });
    Ꮡb.Run(ecdheP256EcdsaP256ˢ, (ж<testing.B> bΔ5) => {
        bΔ5.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ6) => {
            benchmarkHandshakeServer(bΔ6, VersionTLS13, TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305,
                CurveP256, testP256Certificate, testP256PrivateKey.OrTypedNil());
        });
        bΔ5.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ7) => {
            benchmarkHandshakeServer(bΔ7, VersionTLS12, TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305,
                CurveP256, testP256Certificate, testP256PrivateKey.OrTypedNil());
        });
    });
    Ꮡb.Run(ecdheX25519EcdsaP256ˢ, (ж<testing.B> bΔ8) => {
        bΔ8.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ9) => {
            benchmarkHandshakeServer(bΔ9, VersionTLS13, TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305,
                X25519, testP256Certificate, testP256PrivateKey.OrTypedNil());
        });
        bΔ8.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ10) => {
            benchmarkHandshakeServer(bΔ10, VersionTLS12, TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305,
                X25519, testP256Certificate, testP256PrivateKey.OrTypedNil());
        });
    });
    Ꮡb.Run(ecdheP521EcdsaP521ˢ, (ж<testing.B> bΔ11) => {
        if (!AreEqual((~testECDSAPrivateKey).PublicKey.Curve, elliptic.P521())) {
            bΔ11.Fatal(testEcdsaKeyDoesnTUseˢ);
        }
        bΔ11.Run(tlSv13ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ12) => {
            benchmarkHandshakeServer(bΔ12, VersionTLS13, TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305,
                CurveP521, testECDSACertificate, testECDSAPrivateKey.OrTypedNil());
        });
        bΔ11.Run(tlSv12ˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ13) => {
            benchmarkHandshakeServer(bΔ13, VersionTLS12, TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305,
                CurveP521, testECDSACertificate, testECDSAPrivateKey.OrTypedNil());
        });
    });
}

public static void TestClientAuth(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string certPath = default!;
        @string keyPath = default!;
        @string ecdsaCertPath = default!;
        @string ecdsaKeyPath = default!;
        @string ed25519CertPath = default!;
        @string ed25519KeyPath = default!;
        if (update.Value){
            certPath = tempFile(clientCertificatePEM);
            defer(os.Remove, certPath, ref ᒐ);
            keyPath = tempFile(clientKeyPEM);
            defer(os.Remove, keyPath, ref ᒐ);
            ecdsaCertPath = tempFile(clientECDSACertificatePEM);
            defer(os.Remove, ecdsaCertPath, ref ᒐ);
            ecdsaKeyPath = tempFile(clientECDSAKeyPEM);
            defer(os.Remove, ecdsaKeyPath, ref ᒐ);
            ed25519CertPath = tempFile(clientEd25519CertificatePEM);
            defer(os.Remove, ed25519CertPath, ref ᒐ);
            ed25519KeyPath = tempFile(clientEd25519KeyPEM);
            defer(os.Remove, ed25519KeyPath, ref ᒐ);
        } else {
            Ꮡt.Parallel();
        }
        var config = testConfig.Clone();
        config.Value.ClientAuth = RequestClientCert;
        var test = Ꮡ(new serverTest(
            name: "ClientAuthRequestedNotGiven"u8,
            command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "AES128-SHA"u8, "-ciphersuites"u8, "TLS_AES_128_GCM_SHA256"u8}.slice(),
            config: config
        ));
        runServerTestTLS12(Ꮡt, test);
        runServerTestTLS13(Ꮡt, test);
        test = Ꮡ(new serverTest(
            name: "ClientAuthRequestedAndGiven"u8,
            command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "AES128-SHA"u8, "-ciphersuites"u8, "TLS_AES_128_GCM_SHA256"u8,
                "-cert"u8, certPath, "-key"u8, keyPath, "-client_sigalgs"u8, "rsa_pss_rsae_sha256"u8}.slice(),
            config: config,
            expectedPeerCerts: new @string[]{clientCertificatePEM}.slice()
        ));
        runServerTestTLS12(Ꮡt, test);
        runServerTestTLS13(Ꮡt, test);
        test = Ꮡ(new serverTest(
            name: "ClientAuthRequestedAndECDSAGiven"u8,
            command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "AES128-SHA"u8, "-ciphersuites"u8, "TLS_AES_128_GCM_SHA256"u8,
                "-cert"u8, ecdsaCertPath, "-key"u8, ecdsaKeyPath}.slice(),
            config: config,
            expectedPeerCerts: new @string[]{clientECDSACertificatePEM}.slice()
        ));
        runServerTestTLS12(Ꮡt, test);
        runServerTestTLS13(Ꮡt, test);
        test = Ꮡ(new serverTest(
            name: "ClientAuthRequestedAndEd25519Given"u8,
            command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "AES128-SHA"u8, "-ciphersuites"u8, "TLS_AES_128_GCM_SHA256"u8,
                "-cert"u8, ed25519CertPath, "-key"u8, ed25519KeyPath}.slice(),
            config: config,
            expectedPeerCerts: new @string[]{clientEd25519CertificatePEM}.slice()
        ));
        runServerTestTLS12(Ꮡt, test);
        runServerTestTLS13(Ꮡt, test);
        test = Ꮡ(new serverTest(
            name: "ClientAuthRequestedAndPKCS1v15Given"u8,
            command: new @string[]{"openssl"u8, "s_client"u8, "-no_ticket"u8, "-cipher"u8, "AES128-SHA"u8,
                "-cert"u8, certPath, "-key"u8, keyPath, "-client_sigalgs"u8, "rsa_pkcs1_sha256"u8}.slice(),
            config: config,
            expectedPeerCerts: new @string[]{clientCertificatePEM}.slice()
        ));
        runServerTestTLS12(Ꮡt, test);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noErrorReportedFromˢ = (@string)"No error reported from server"u8;
internal static readonly object handshakeRegisteredAsˢ = (@string)"Handshake registered as complete"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedServerNameᶜ = "test.testing"u8;

public static partial void TestSNIGivenOnFailure(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string expectedServerName = expectedServerNameᶜ;
        var clientHello = Ꮡ(new clientHelloMsg(
            vers: VersionTLS12,
            random: new slice<byte>(32),
            cipherSuites: new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice(),
            compressionMethods: new uint8[]{compressionNone}.slice(),
            serverName: expectedServerName
        ));
        var serverConfig = testConfig.Clone();
        // Erase the server's cipher suites to ensure the handshake fails.
        serverConfig.Value.CipherSuites = default!;
        var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
        var cʗ1 = c;
        var clientHelloʗ1 = clientHello;
        goǃ(() => {
            var cli = Client(cʗ1, testConfig);
            cli.Value.vers = clientHelloʗ1.Value.vers;
            {
                var (_, errΔ1) = cli.writeHandshakeRecord(new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(clientHelloʗ1), default!); if (errΔ1 != default!) {
                    testFatal(Ꮡt, errΔ1);
                }
            }
            cʗ1.Close();
        });
        var conn = Server(s, serverConfig);
        var ctx = context.Background();
        var (ch, _, err) = conn.readClientHello(ctx);
        ref var hs = ref heap<global::go.crypto.tls_package.serverHandshakeState>(out var Ꮡhs);
        hs = new serverHandshakeState(
            c: conn,
            ctx: ctx,
            clientHello: ch
        );
        if (err == default!) {
            err = hs.processClientHello();
        }
        if (err == default!) {
            err = Ꮡhs.pickCipherSuite();
        }
        var sʗ1 = s;
        defer(() => sʗ1.Close(), ref ᒐ);
        if (err == default!) {
            Ꮡt.Error(noErrorReportedFromˢ);
        }
        var cs = hs.c.ConnectionState();
        if (cs.HandshakeComplete) {
            Ꮡt.Error(handshakeRegisteredAsˢ);
        }
        if (cs.ServerName != expectedServerName) {
            Ꮡt.Errorf("Expected ServerName of %q, but got %q"u8, expectedServerName, cs.ServerName);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Setting a maximum version of TLS 1.1 should cause
// the handshake to fail, as the client MinVersion is TLS 1.2.

partial struct getConfigForClientTestsᴛ1 /*dyn*/ {
    internal Action<ж<global::go.crypto.tls_package.Config>> setup;
    internal Func<ж<global::go.crypto.tls_package.ClientHelloInfo>, (ж<global::go.crypto.tls_package.Config>, error)> callback;
    internal @string errorSubstring;
    internal Func<ж<global::go.crypto.tls_package.Config>, error> verify;
}
internal static slice<getConfigForClientTestsᴛ1> getConfigForClientTests;
internal static void initᴛgetConfigForClientTests() { getConfigForClientTests = new getConfigForClientTestsᴛ1[]{
    new(
        default!,
        (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHello) => (default!, default!),
        ""u8,
        default!
    ),
    new(
        default!,
        (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHello) => (default!, errors.New("should bubble up"u8)),
        "should bubble up"u8,
        default!
    ),
    new(
        default!,
        (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHello) => {
            var config = testConfig.Clone();
            config.Value.MaxVersion = VersionTLS11;
            return (config, default!);
        },
        "client offered only unsupported versions"u8,
        default!
    ),
    new(
        (ж<global::go.crypto.tls_package.Config> config) => {
            foreach (var (i, _) in (~config).SessionTicketKey) {
                config.Value.SessionTicketKey[i] = (byte)i;
            }
            config.Value.sessionTicketKeys = default!;
        },
        (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHello) => {
            var config = testConfig.Clone();
            foreach (var (i, _) in (~config).SessionTicketKey) {
                config.Value.SessionTicketKey[i] = 0;
            }
            config.Value.sessionTicketKeys = default!;
            return (config, default!);
        },
        ""u8,
        error (ж<global::go.crypto.tls_package.Config> config) => {
            if ((~config).SessionTicketKey == new byte[]{}.array(32)) {
                return fmt.Errorf("expected SessionTicketKey to be set"u8);
            }
            return default!;
        }
    ),
    new(
        (ж<global::go.crypto.tls_package.Config> config) => {
            array<byte> dummyKey = new(32);
            foreach (var (i, _) in dummyKey) {
                dummyKey[i] = (byte)i;
            }
            config.SetSessionTicketKeys(GoReflect.WithElemDims(new array<byte>[]{dummyKey.Clone()}.slice(), 32));
        },
        (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHello) => {
            var config = testConfig.Clone();
            config.Value.sessionTicketKeys = default!;
            return (config, default!);
        },
        ""u8,
        error (ж<global::go.crypto.tls_package.Config> config) => {
            if ((~config).SessionTicketKey == new byte[]{}.array(32)) {
                return fmt.Errorf("expected SessionTicketKey to be set"u8);
            }
            return default!;
        }
    )
}.slice(); }

public static partial void TestGetConfigForClient(ж<testing.T> Ꮡt) {
    var serverConfig = testConfig.Clone();
    var clientConfig = testConfig.Clone();
    clientConfig.Value.MinVersion = VersionTLS12;
    foreach (var (i, vᴛ1) in getConfigForClientTests) {
        ref var test = ref heap(new getConfigForClientTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        if (test.setup != default!) {
            test.setup(serverConfig);
        }
        ref var configReturned = ref heap<ж<global::go.crypto.tls_package.Config>>(out var ᏑconfigReturned);
        var testʗ1 = test;
        serverConfig.Value.GetConfigForClient = (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHello) => {
            var (config, err) = testʗ1.callback(clientHello);
            ᏑconfigReturned.ValueSlot = config;
            return (config, err);
        };
        var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
        var done = new channel<error>(0);
        var doneʗ1 = done;
        var sʗ1 = s;
        var serverConfigʗ1 = serverConfig;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var sʗ2 = sʗ1;
                defer(() => sʗ2.Close(), ref ᒐ);
                doneʗ1.ᐸꟷ(Server(sʗ1, serverConfigʗ1).Handshake());
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var clientErr = Client(c, clientConfig).Handshake();
        c.Close();
        var serverErr = ᐸꟷ(done);
        if (len(test.errorSubstring) == 0){
            if (serverErr != default! || clientErr != default!) {
                Ꮡt.Errorf("test[%d]: expected no error but got serverErr: %q, clientErr: %q"u8, i, serverErr, clientErr);
            }
            if (test.verify != default!) {
                {
                    var err = test.verify(configReturned); if (err != default!) {
                        Ꮡt.Errorf("test[%d]: verify returned error: %v"u8, i, err);
                    }
                }
            }
        } else {
            if (serverErr == default!){
                Ꮡt.Errorf("test[%d]: expected error containing %q but got no error"u8, i, test.errorSubstring);
            } else 
            if (!strings.Contains(serverErr.Error(), test.errorSubstring)) {
                Ꮡt.Errorf("test[%d]: expected error to contain %q but it was %q"u8, i, test.errorSubstring, serverErr);
            }
        }
    }
}

public static partial void TestCloseServerConnectionOnIdleClient(ж<testing.T> Ꮡt) {
    var (clientConn, serverConn) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var server = Server(serverConn, testConfig.Clone());
    var clientConnʗ1 = clientConn;
    var serverʗ1 = server;
    goǃ(() => {
        clientConnʗ1.Write(new byte[]{(rune)'0'}.slice());
        serverʗ1.Close();
    });
    server.SetReadDeadline(time_package.Now().Add(time_package.ΔMinute));
    var err = server.Handshake();
    if (err != default!){
        {
            var (errΔ1, ok) = err._<netꓸError>(ᐧ); if (ok && errΔ1.Timeout()) {
                Ꮡt.Errorf("Expected a closed network connection error but got '%s'"u8, errΔ1.Error());
            }
        }
    } else {
        Ꮡt.Errorf("Error expected, but no error returned"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object clonedHashGeneratedAˢ = (@string)"cloned hash generated a different sum"u8;

public static void TestCloneHash(ж<testing.T> Ꮡt) {
    var h1 = crypto.SHA256.New();
    h1.Write(slice<byte>("test"u8));
    var s1 = h1.Sum(default!);
    var h2 = cloneHash(h1, crypto.SHA256);
    var s2 = h2.Sum(default!);
    if (!bytes.Equal(s1, s2)) {
        Ꮡt.Error(clonedHashGeneratedAˢ);
    }
}

internal static void expectError(ж<testing.T> Ꮡt, error err, @string sub) {
    if (err == default!){
        Ꮡt.Errorf(@"expected error %q, got nil"u8, sub);
    } else 
    if (!strings.Contains(err.Error(), sub)) {
        Ꮡt.Errorf(@"expected error %q, got %q"u8, sub, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string beginRsaTestingKeyˢ = """
-----BEGIN RSA TESTING KEY-----
MIIBOgIBAAJBAN17PWsVQPBrHYdPFtycVQ/0CFyAQYwdVXaefhVURYUkHojwL82T
HRfLJCWuYVgHMRCcg+EqWzhPSEWgu+MmdekCAwEAAQJBALjQYNTdXF4CFBbXwUz/
yt9QFDYT9B5WT/12jeGAe653gtYS6OOi/+eAkGmzg1GlRnw6fOfn+HYNFDORST7z
4j0CIQDn2xz9hVWQEu9ee3vecNT3f60huDGTNoRhtqgweQGX0wIhAPSLj1VcRZEz
nKpbtU22+PbIMSJ+e80fmY9LIPx5N4HTAiAthGSimMR9bloz0EY3GyuUEyqoDgMd
hXxjuno2WesoJQIgemilbcALXpxsLmZLgcQ2KSmaVr7jb5ECx9R+hYKTw1sCIG4s
T+E0J8wlH24pgwQHzy7Ko2qLwn1b5PW8ecrlvP1g
-----END RSA TESTING KEY-----
"""u8;
internal static readonly @string keySizeTooSmallˢ = "key size too small"u8;
internal static readonly @string handshakeFailureˢ = "handshake failure"u8;

public static partial void TestKeyTooSmallForRSAPSS(ж<testing.T> Ꮡt) {
    ref var cert = ref heap<global::go.crypto.tls_package.Certificate>(out var Ꮡcert);
    (cert, var err) = X509KeyPair(slice<byte>("""
-----BEGIN CERTIFICATE-----
MIIBcTCCARugAwIBAgIQGjQnkCFlUqaFlt6ixyz/tDANBgkqhkiG9w0BAQsFADAS
MRAwDgYDVQQKEwdBY21lIENvMB4XDTE5MDExODIzMjMyOFoXDTIwMDExODIzMjMy
OFowEjEQMA4GA1UEChMHQWNtZSBDbzBcMA0GCSqGSIb3DQEBAQUAA0sAMEgCQQDd
ez1rFUDwax2HTxbcnFUP9AhcgEGMHVV2nn4VVEWFJB6I8C/Nkx0XyyQlrmFYBzEQ
nIPhKls4T0hFoLvjJnXpAgMBAAGjTTBLMA4GA1UdDwEB/wQEAwIFoDATBgNVHSUE
DDAKBggrBgEFBQcDATAMBgNVHRMBAf8EAjAAMBYGA1UdEQQPMA2CC2V4YW1wbGUu
Y29tMA0GCSqGSIb3DQEBCwUAA0EAxDuUS+BrrS3c+h+k+fQPOmOScy6yTX9mHw0Q
KbucGamXYEy0URIwOdO0tQ3LHPc1YGvYSPwkDjkjqECs2Vm/AA==
-----END CERTIFICATE-----
"""u8), slice<byte>(testingKey(beginRsaTestingKeyˢ)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (clientConn, serverConn) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
    var client = Client(clientConn, testConfig);
    var done = new channel<EmptyStruct>(0);
    var certʗ1 = cert;
    var doneʗ1 = done;
    var serverConnʗ1 = serverConn;
    goǃ(() => {
        var config = testConfig.Clone();
        config.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{certʗ1}.slice();
        config.Value.MinVersion = VersionTLS13;
        var server = Server(serverConnʗ1, config);
        var errΔ1 = server.Handshake();
        expectError(Ꮡt, errΔ1, keySizeTooSmallˢ);
        close(doneʗ1);
    });
    err = client.Handshake();
    expectError(Ꮡt, err, handshakeFailureˢ);
    ᐸꟷ(done);
}

public static void TestMultipleCertificates(ж<testing.T> Ꮡt) {
    var clientConfig = testConfig.Clone();
    clientConfig.Value.CipherSuites = new uint16[]{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice();
    clientConfig.Value.MaxVersion = VersionTLS12;
    var serverConfig = testConfig.Clone();
    serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(
        ΔCertificate: new slice<byte>[]{testECDSACertificate}.slice(),
        PrivateKey: testECDSAPrivateKey.OrTypedNil()
    ), new(
        ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
        PrivateKey: testRSAPrivateKey.OrTypedNil()
    )
    }.slice();
    var (_, clientState, err) = testHandshake(Ꮡt, clientConfig, serverConfig);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        Δx509.PublicKeyAlgorithm got = clientState.PeerCertificates[0].Value.PublicKeyAlgorithm; if (got != Δx509.RSA) {
            Ꮡt.Errorf("expected RSA certificate, got %v"u8, got);
        }
    }
}

internal partial struct TestAESCipherReordering_tests /*dyn*/ {
    internal @string name;
    internal slice<uint16> clientCiphers;
    internal bool serverHasAESGCM;
    internal slice<uint16> serverCiphers;
    internal uint16 expectedCipher;
}

public static void TestAESCipherReordering(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        skipFIPS(Ꮡt); // No CHACHA20_POLY1305 for FIPS.
        var currentAESSupport = hasAESGCMHardwareSupport;
        defer(() => {
            hasAESGCMHardwareSupport = currentAESSupport;
        }, ref ᒐ);
        var tests = new TestAESCipherReordering_tests[]{
            new(
                name: "server has hardware AES, client doesn't (pick ChaCha)"u8,
                clientCiphers: new uint16[]{
                    TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,
                    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                    TLS_RSA_WITH_AES_128_CBC_SHA
                }.slice(),
                serverHasAESGCM: true,
                expectedCipher: TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305
            ),
            new(
                name: "client prefers AES-GCM, server doesn't have hardware AES (pick ChaCha)"u8,
                clientCiphers: new uint16[]{
                    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                    TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,
                    TLS_RSA_WITH_AES_128_CBC_SHA
                }.slice(),
                serverHasAESGCM: false,
                expectedCipher: TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305
            ),
            new(
                name: "client prefers AES-GCM, server has hardware AES (pick AES-GCM)"u8,
                clientCiphers: new uint16[]{
                    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                    TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,
                    TLS_RSA_WITH_AES_128_CBC_SHA
                }.slice(),
                serverHasAESGCM: true,
                expectedCipher: TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256
            ),
            new(
                name: "client prefers AES-GCM and sends GREASE, server has hardware AES (pick AES-GCM)"u8,
                clientCiphers: new uint16[]{
                    0x0A0A, // GREASE value

                    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                    TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,
                    TLS_RSA_WITH_AES_128_CBC_SHA
                }.slice(),
                serverHasAESGCM: true,
                expectedCipher: TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256
            ),
            new(
                name: "client prefers AES-GCM and doesn't support ChaCha, server doesn't have hardware AES (pick AES-GCM)"u8,
                clientCiphers: new uint16[]{
                    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                    TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
                    TLS_RSA_WITH_AES_128_CBC_SHA
                }.slice(),
                serverHasAESGCM: false,
                expectedCipher: TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256
            ),
            new(
                name: "client prefers AES-GCM and AES-CBC over ChaCha, server doesn't have hardware AES (pick ChaCha)"u8,
                clientCiphers: new uint16[]{
                    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                    TLS_RSA_WITH_AES_128_CBC_SHA,
                    TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305
                }.slice(),
                serverHasAESGCM: false,
                expectedCipher: TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305
            ),
            new(
                name: "client prefers AES-GCM over ChaCha and sends GREASE, server doesn't have hardware AES (pick ChaCha)"u8,
                clientCiphers: new uint16[]{
                    0x0A0A, // GREASE value

                    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                    TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,
                    TLS_RSA_WITH_AES_128_CBC_SHA
                }.slice(),
                serverHasAESGCM: false,
                expectedCipher: TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305
            ),
            new(
                name: "client supports multiple AES-GCM, server doesn't have hardware AES and doesn't support ChaCha (AES-GCM)"u8,
                clientCiphers: new uint16[]{
                    TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
                    TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,
                    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256
                }.slice(),
                serverHasAESGCM: false,
                serverCiphers: new uint16[]{
                    TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
                    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256
                }.slice(),
                expectedCipher: TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256
            ),
            new(
                name: "client prefers AES-GCM, server has hardware but doesn't support AES (pick ChaCha)"u8,
                clientCiphers: new uint16[]{
                    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                    TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,
                    TLS_RSA_WITH_AES_128_CBC_SHA
                }.slice(),
                serverHasAESGCM: true,
                serverCiphers: new uint16[]{
                    TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305
                }.slice(),
                expectedCipher: TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305
            )
        }.slice();
        foreach (var (_, vᴛ1) in tests) {
            ref var tc = ref heap(new TestAESCipherReordering_tests(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
                hasAESGCMHardwareSupport = tcʗ1.serverHasAESGCM;
                var hs = Ꮡ(new serverHandshakeState(
                    c: Ꮡ(new Conn(
                        config: Ꮡ(new Config(
                            CipherSuites: tcʗ1.serverCiphers
                        )),
                        vers: VersionTLS12
                    )),
                    clientHello: Ꮡ(new clientHelloMsg(
                        cipherSuites: tcʗ1.clientCiphers,
                        vers: VersionTLS12
                    )),
                    ecdheOk: true,
                    rsaSignOk: true,
                    rsaDecryptOk: true
                ));
                var err = hs.pickCipherSuite();
                if (err != default!) {
                    tΔ1.Errorf("pickCipherSuite failed: %s"u8, err);
                }
                if (tcʗ1.expectedCipher != (~(~hs).suite).id) {
                    tΔ1.Errorf("unexpected cipher chosen: want %d, got %d"u8, tcʗ1.expectedCipher, (~(~hs).suite).id);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestAESCipherReorderingTLS13_tests /*dyn*/ {
    internal @string name;
    internal slice<uint16> clientCiphers;
    internal bool serverHasAESGCM;
    internal uint16 expectedCipher;
}

public static void TestAESCipherReorderingTLS13(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        skipFIPS(Ꮡt); // No CHACHA20_POLY1305 for FIPS.
        var currentAESSupport = hasAESGCMHardwareSupport;
        defer(() => {
            hasAESGCMHardwareSupport = currentAESSupport;
        }, ref ᒐ);
        var tests = new TestAESCipherReorderingTLS13_tests[]{
            new(
                name: "server has hardware AES, client doesn't (pick ChaCha)"u8,
                clientCiphers: new uint16[]{
                    TLS_CHACHA20_POLY1305_SHA256,
                    TLS_AES_128_GCM_SHA256
                }.slice(),
                serverHasAESGCM: true,
                expectedCipher: TLS_CHACHA20_POLY1305_SHA256
            ),
            new(
                name: "neither server nor client have hardware AES (pick ChaCha)"u8,
                clientCiphers: new uint16[]{
                    TLS_CHACHA20_POLY1305_SHA256,
                    TLS_AES_128_GCM_SHA256
                }.slice(),
                serverHasAESGCM: false,
                expectedCipher: TLS_CHACHA20_POLY1305_SHA256
            ),
            new(
                name: "client prefers AES, server doesn't have hardware (pick ChaCha)"u8,
                clientCiphers: new uint16[]{
                    TLS_AES_128_GCM_SHA256,
                    TLS_CHACHA20_POLY1305_SHA256
                }.slice(),
                serverHasAESGCM: false,
                expectedCipher: TLS_CHACHA20_POLY1305_SHA256
            ),
            new(
                name: "client prefers AES and sends GREASE, server doesn't have hardware (pick ChaCha)"u8,
                clientCiphers: new uint16[]{
                    0x0A0A, // GREASE value

                    TLS_AES_128_GCM_SHA256,
                    TLS_CHACHA20_POLY1305_SHA256
                }.slice(),
                serverHasAESGCM: false,
                expectedCipher: TLS_CHACHA20_POLY1305_SHA256
            ),
            new(
                name: "client prefers AES, server has hardware AES (pick AES)"u8,
                clientCiphers: new uint16[]{
                    TLS_AES_128_GCM_SHA256,
                    TLS_CHACHA20_POLY1305_SHA256
                }.slice(),
                serverHasAESGCM: true,
                expectedCipher: TLS_AES_128_GCM_SHA256
            ),
            new(
                name: "client prefers AES and sends GREASE, server has hardware AES (pick AES)"u8,
                clientCiphers: new uint16[]{
                    0x0A0A, // GREASE value

                    TLS_AES_128_GCM_SHA256,
                    TLS_CHACHA20_POLY1305_SHA256
                }.slice(),
                serverHasAESGCM: true,
                expectedCipher: TLS_AES_128_GCM_SHA256
            )
        }.slice();
        foreach (var (_, vᴛ1) in tests) {
            ref var tc = ref heap(new TestAESCipherReorderingTLS13_tests(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
                hasAESGCMHardwareSupport = tcʗ1.serverHasAESGCM;
                var (pk, _) = ecdh.X25519().GenerateKey(go.crypto.rand_package.Reader);
                var hs = Ꮡ(new serverHandshakeStateTLS13(
                    c: Ꮡ(new Conn(
                        config: Ꮡ(new Config(nil)),
                        vers: VersionTLS13
                    )),
                    clientHello: Ꮡ(new clientHelloMsg(
                        cipherSuites: tcʗ1.clientCiphers,
                        supportedVersions: new uint16[]{VersionTLS13}.slice(),
                        compressionMethods: new uint8[]{compressionNone}.slice(),
                        keyShares: new global::go.crypto.tls_package.keyShare[]{new(group: X25519, data: pk.PublicKey().Bytes())}.slice(),
                        supportedCurves: new global::go.crypto.tls_package.CurveID[]{X25519}.slice()
                    ))
                ));
                var err = hs.processClientHello();
                if (err != default!) {
                    tΔ1.Errorf("pickCipherSuite failed: %s"u8, err);
                }
                if (tcʗ1.expectedCipher != (~(~hs).suite).id) {
                    tΔ1.Errorf("unexpected cipher chosen: want %d, got %d"u8, tcʗ1.expectedCipher, (~(~hs).suite).id);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object serverHandshakeDidNotˢ = (@string)"Server handshake did not error when the context was canceled"u8;
internal static readonly object serverConnectionWasNotˢ = (@string)"Server connection was not closed when the context was canceled"u8;

// TestServerHandshakeContextCancellation tests that canceling
// the context given to the server side conn.HandshakeContext
// interrupts the in-progress handshake.
public static partial void TestServerHandshakeContextCancellation(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
        var (ctx, cancel) = context.WithCancel(context.Background());
        var unblockClient = new channel<EmptyStruct>(0);
        defer(ᴛ1 => close(ᴛ1), unblockClient, ref ᒐ);
        var cʗ1 = c;
        var cancelʗ1 = cancel;
        var unblockClientʗ1 = unblockClient;
        goǃ(() => {
            cancelʗ1();
            ᐸꟷ(unblockClientʗ1);
            _ = cʗ1.Close();
        });
        var conn = Server(s, testConfig);
        // Initiates server side handshake, which will block until a client hello is read
        // unless the cancellation works.
        var err = conn.HandshakeContext(ctx);
        if (err == default!) {
            Ꮡt.Fatal(serverHandshakeDidNotˢ);
        }
        if (!AreEqual(err, context.Canceled)) {
            Ꮡt.Errorf("Unexpected server handshake error: %v"u8, err);
        }
        if (runtime.GOARCH == "wasm"u8) {
            Ꮡt.Skip(connCloseDoesNotErrorAsˢ);
        }
        err = conn.Close();
        if (err == default!) {
            Ꮡt.Error(serverConnectionWasNotˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestHandshakeContextHierarchy tests whether the contexts
// available to GetClientCertificate and GetCertificate are
// derived from the context provided to HandshakeContext, and
// that those contexts are canceled after HandshakeContext has
// returned.
public static partial void TestHandshakeContextHierarchy(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (c, s) = localPipe(new tls_test_package.testing_TжTB(Ꮡt));
        var clientErr = new channel<error>(1);
        var clientConfig = testConfig.Clone();
        var serverConfig = testConfig.Clone();
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        ref var key = ref heap<EmptyStruct>(out var Ꮡkey);
        key = new EmptyStruct();
        ctx = context.WithValue(ctx, key, true);
        var cʗ1 = c;
        var clientConfigʗ1 = clientConfig;
        var clientErrʗ1 = clientErr;
        var ctxʗ1 = ctx;
        var keyʗ1 = key;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => close(ᴛ1), clientErrʗ1, ref ᒐ);
                var cʗ2 = cʗ1;
                defer(() => cʗ2.Close(), ref ᒐ);
                ref var innerCtxΔ1 = ref heap<context.Context>(out var ᏑinnerCtxΔ1);
                clientConfigʗ1.Value.Certificates = default!;
                var keyʗ2 = keyʗ1;
                clientConfigʗ1.Value.GetClientCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.CertificateRequestInfo> certificateRequest) => {
                    {
                        var (val, ok) = certificateRequest.Context().Value(keyʗ2)._<bool>(ᐧ); if (!ok || !val) {
                            Ꮡt.Errorf("GetClientCertificate context was not child of HandshakeContext"u8);
                        }
                    }
                    ᏑinnerCtxΔ1.ValueSlot = certificateRequest.Context();
                    return (Ꮡ(new Certificate(
                        ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
                        PrivateKey: testRSAPrivateKey.OrTypedNil()
                    )), default!);
                };
                var cli = Client(cʗ1, clientConfigʗ1);
                var errΔ1 = cli.HandshakeContext(ctxʗ1);
                if (errΔ1 != default!) {
                    clientErrʗ1.ᐸꟷ(errΔ1);
                    return;
                }
                var selᴛ4 = ᏑinnerCtxΔ1.ValueSlot.Done();
                switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
                case 0 when selᴛ4.ꟷᐳ(out _): {
                    break;
                }
                default: {
                    Ꮡt.Errorf("GetClientCertificate context was not canceled after HandshakeContext returned."u8);
                    break;
                }}
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        ref var innerCtx = ref heap<context.Context>(out var ᏑinnerCtx);
        serverConfig.Value.Certificates = default!;
        serverConfig.Value.ClientAuth = RequestClientCert;
        var keyʗ3 = key;
        serverConfig.Value.GetCertificate = (ж<global::go.crypto.tls_package.Certificate>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHello) => {
            {
                var (val, ok) = clientHello.Context().Value(keyʗ3)._<bool>(ᐧ); if (!ok || !val) {
                    Ꮡt.Errorf("GetClientCertificate context was not child of HandshakeContext"u8);
                }
            }
            ᏑinnerCtx.ValueSlot = clientHello.Context();
            return (Ꮡ(new Certificate(
                ΔCertificate: new slice<byte>[]{testRSACertificate}.slice(),
                PrivateKey: testRSAPrivateKey.OrTypedNil()
            )), default!);
        };
        var conn = Server(s, serverConfig);
        var err = conn.HandshakeContext(ctx);
        if (err != default!) {
            Ꮡt.Errorf("Unexpected server handshake error: %v"u8, err);
        }
        var selᴛ5 = innerCtx.Done();
        switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
        case 0 when selᴛ5.ꟷᐳ(out _): {
            break;
        }
        default: {
            Ꮡt.Errorf("GetCertificate context was not canceled after HandshakeContext returned."u8);
            break;
        }}
        {
            var errΔ2 = ᐸꟷ(clientErr); if (errΔ2 != default!) {
                Ꮡt.Errorf("Unexpected client error: %v"u8, errΔ2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tls12ˢ3 = "TLS1.2"u8;
internal static readonly @string tls13ˢ3 = "TLS1.3"u8;

public static void TestHandshakeChainExpiryResumption(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tls12ˢ3, (ж<testing.T> tΔ1) => {
        testHandshakeChainExpiryResumption(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tls13ˢ3, (ж<testing.T> tΔ2) => {
        testHandshakeChainExpiryResumption(tΔ2, VersionTLS13);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expiredResumeExampleComˢ = "expired-resume.example.com"u8;
internal static readonly @string leafExpiresBeforeRootˢ = "LeafExpiresBeforeRoot"u8;
internal static readonly @string leafExpiresAfterRootˢ = "LeafExpiresAfterRoot"u8;

internal static void testHandshakeChainExpiryResumption(ж<testing.T> Ꮡt, uint16 version) {
    ref var now = ref heap<time.Time>(out var Ꮡnow);
    now = time_package.Now();
    (slice<byte> leafDER, slice<byte> expiredLeafDER, ж<Δx509.Certificate> root) createChain(time.Time leafNotAfter, time.Time rootNotAfter) {
        ж<Δx509.Certificate> root = default!;
        var tmpl = Ꮡ(new Δx509.Certificate(
            Subject: new pkix.Name(CommonName: "root"u8),
            NotBefore: rootNotAfter.Add((time.Duration)(-86400000000000L)),
            NotAfter: rootNotAfter,
            IsCA: true,
            BasicConstraintsValid: true
        ));
        var (rootDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, testECDSAPrivateKey.of(ecdsa.PrivateKey.ᏑPublicKey), testECDSAPrivateKey.OrTypedNil());
        if (err != default!) {
            Ꮡt.Fatalf("CreateCertificate: %v"u8, err);
        }
        (root, err) = Δx509.ParseCertificate(rootDER);
        if (err != default!) {
            Ꮡt.Fatalf("ParseCertificate: %v"u8, err);
        }
        tmpl = Ꮡ(new Δx509.Certificate(
            Subject: new pkix.Name(nil),
            DNSNames: new @string[]{"expired-resume.example.com"u8}.slice(),
            NotBefore: leafNotAfter.Add((time.Duration)(-86400000000000L)),
            NotAfter: leafNotAfter,
            KeyUsage: Δx509.KeyUsageDigitalSignature
        ));
        (var leafCertDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, root, testECDSAPrivateKey.of(ecdsa.PrivateKey.ᏑPublicKey), testECDSAPrivateKey.OrTypedNil());
        if (err != default!) {
            Ꮡt.Fatalf("CreateCertificate: %v"u8, err);
        }
        (tmpl.Value.NotBefore, tmpl.Value.NotAfter) = (leafNotAfter.Add((time.Duration)(-31536000000000000L)), leafNotAfter.Add((time.Duration)(-31449600000000000L)));
        (var expiredLeafDERCertDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, root, testECDSAPrivateKey.of(ecdsa.PrivateKey.ᏑPublicKey), testECDSAPrivateKey.OrTypedNil());
        if (err != default!) {
            Ꮡt.Fatalf("CreateCertificate: %v"u8, err);
        }
        return (leafCertDER, expiredLeafDERCertDER, root);
    }
    var createChainʗ1 = createChain;
    var nowʗ1 = now;
    void testExpiration(@string name, time.Time leafNotAfter, time.Time rootNotAfter) {
        var createChainʗ2 = createChainʗ1;
        var leafNotAfterʗ1 = leafNotAfter;
        var nowʗ2 = nowʗ1;
        var rootNotAfterʗ1 = rootNotAfter;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var (initialLeafDER, expiredLeafDER, initialRoot) = createChainʗ2(leafNotAfterʗ1, rootNotAfterʗ1);
            var serverConfig = testConfig.Clone();
            serverConfig.Value.MaxVersion = version;
            serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(
                ΔCertificate: new slice<byte>[]{initialLeafDER, expiredLeafDER}.slice(),
                PrivateKey: testECDSAPrivateKey.OrTypedNil()
            )
            }.slice();
            serverConfig.Value.ClientCAs = Δx509.NewCertPool();
            (~serverConfig).ClientCAs.AddCert(initialRoot);
            serverConfig.Value.ClientAuth = RequireAndVerifyClientCert;
            var nowʗ3 = nowʗ2;
            serverConfig.Value.Time = () => nowʗ3;
            serverConfig.Value.InsecureSkipVerify = false;
            serverConfig.Value.ServerName = expiredResumeExampleComˢ;
            var clientConfig = testConfig.Clone();
            clientConfig.Value.MaxVersion = version;
            clientConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(
                ΔCertificate: new slice<byte>[]{initialLeafDER, expiredLeafDER}.slice(),
                PrivateKey: testECDSAPrivateKey.OrTypedNil()
            )
            }.slice();
            clientConfig.Value.RootCAs = Δx509.NewCertPool();
            (~clientConfig).RootCAs.AddCert(initialRoot);
            clientConfig.Value.ServerName = expiredResumeExampleComˢ;
            clientConfig.Value.ClientSessionCache = NewLRUClientSessionCache(32);
            clientConfig.Value.InsecureSkipVerify = false;
            clientConfig.Value.ServerName = expiredResumeExampleComˢ;
            var nowʗ4 = nowʗ2;
            clientConfig.Value.Time = () => nowʗ4;
            void testResume(ж<testing.T> tΔ2, ж<global::go.crypto.tls_package.Config> sc, ж<global::go.crypto.tls_package.Config> cc, bool expectResume) {
                tΔ2.Helper();
                var (ss, cs, err) = testHandshake(tΔ2, cc, sc);
                if (err != default!) {
                    tΔ2.Fatalf("handshake: %v"u8, err);
                }
                if (cs.DidResume != expectResume) {
                    tΔ2.Fatalf("DidResume = %v; want %v"u8, cs.DidResume, expectResume);
                }
                if (ss.DidResume != expectResume) {
                    tΔ2.Fatalf("DidResume = %v; want %v"u8, cs.DidResume, expectResume);
                }
            }
            testResume(tΔ1, serverConfig, clientConfig, false);
            testResume(tΔ1, serverConfig, clientConfig, true);
            ref var expiredNow = ref heap<time.Time>(out var ᏑexpiredNow);
            expiredNow = time_package.Unix(0, min(leafNotAfterʗ1.UnixNano(), rootNotAfterʗ1.UnixNano())).Add(time_package.ΔMinute);
            (var freshLeafDER, expiredLeafDER, var freshRoot) = createChainʗ2(expiredNow.Add(time_package.ΔHour), expiredNow.Add(time_package.ΔHour));
            clientConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(
                ΔCertificate: new slice<byte>[]{freshLeafDER, expiredLeafDER}.slice(),
                PrivateKey: testECDSAPrivateKey.OrTypedNil()
            )
            }.slice();
            var expiredNowʗ1 = expiredNow;
            serverConfig.Value.Time = () => expiredNowʗ1;
            serverConfig.Value.ClientCAs = Δx509.NewCertPool();
            (~serverConfig).ClientCAs.AddCert(freshRoot);
            testResume(tΔ1, serverConfig, clientConfig, false);
        });
    }
    testExpiration(leafExpiresBeforeRootˢ, now.Add((time.Duration)(7200000000000L)), now.Add((time.Duration)(10800000000000L)));
    testExpiration(leafExpiresAfterRootˢ, now.Add((time.Duration)(7200000000000L)), now.Add(time_package.ΔHour));
}

public static void TestHandshakeGetConfigForClientDifferentClientCAs(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tls12ˢ3, (ж<testing.T> tΔ1) => {
        testHandshakeGetConfigForClientDifferentClientCAs(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tls13ˢ3, (ж<testing.T> tΔ2) => {
        testHandshakeGetConfigForClientDifferentClientCAs(tΔ2, VersionTLS13);
    });
}

internal static void testHandshakeGetConfigForClientDifferentClientCAs(ж<testing.T> Ꮡt, uint16 version) {
    ref var now = ref heap<time.Time>(out var Ꮡnow);
    now = time_package.Now();
    var tmpl = Ꮡ(new Δx509.Certificate(
        Subject: new pkix.Name(CommonName: "root"u8),
        NotBefore: now.Add((time.Duration)(-86400000000000L)),
        NotAfter: now.Add((time.Duration)(86400000000000L)),
        IsCA: true,
        BasicConstraintsValid: true
    ));
    var (rootDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, testECDSAPrivateKey.of(ecdsa.PrivateKey.ᏑPublicKey), testECDSAPrivateKey.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("CreateCertificate: %v"u8, err);
    }
    (var rootA, err) = Δx509.ParseCertificate(rootDER);
    if (err != default!) {
        Ꮡt.Fatalf("ParseCertificate: %v"u8, err);
    }
    (rootDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, testRSA2048PrivateKey.of(rsa.PrivateKey.ᏑPublicKey), testRSA2048PrivateKey.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("CreateCertificate: %v"u8, err);
    }
    (var rootB, err) = Δx509.ParseCertificate(rootDER);
    if (err != default!) {
        Ꮡt.Fatalf("ParseCertificate: %v"u8, err);
    }
    tmpl = Ꮡ(new Δx509.Certificate(
        Subject: new pkix.Name(nil),
        DNSNames: new @string[]{"example.com"u8}.slice(),
        NotBefore: now.Add((time.Duration)(-86400000000000L)),
        NotAfter: now.Add((time.Duration)(86400000000000L)),
        KeyUsage: Δx509.KeyUsageDigitalSignature
    ));
    (var certA, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, rootA, testECDSAPrivateKey.of(ecdsa.PrivateKey.ᏑPublicKey), testECDSAPrivateKey.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("CreateCertificate: %v"u8, err);
    }
    (var certB, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, rootB, testECDSAPrivateKey.of(ecdsa.PrivateKey.ᏑPublicKey), testRSA2048PrivateKey.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("CreateCertificate: %v"u8, err);
    }
    var serverConfig = testConfig.Clone();
    serverConfig.Value.MaxVersion = version;
    serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(
        ΔCertificate: new slice<byte>[]{certA}.slice(),
        PrivateKey: testECDSAPrivateKey.OrTypedNil()
    )
    }.slice();
    var nowʗ1 = now;
    serverConfig.Value.Time = () => nowʗ1;
    serverConfig.Value.ClientCAs = Δx509.NewCertPool();
    (~serverConfig).ClientCAs.AddCert(rootA);
    serverConfig.Value.ClientAuth = RequireAndVerifyClientCert;
    var switchConfig = false;
    var rootBʗ1 = rootB;
    var serverConfigʗ1 = serverConfig;
    serverConfig.Value.GetConfigForClient = (ж<global::go.crypto.tls_package.Config>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> clientHello) => {
        if (!switchConfig) {
            return (default!, default!);
        }
        var cfg = serverConfigʗ1.Clone();
        cfg.Value.ClientCAs = Δx509.NewCertPool();
        (~cfg).ClientCAs.AddCert(rootBʗ1);
        return (cfg, default!);
    };
    serverConfig.Value.InsecureSkipVerify = false;
    serverConfig.Value.ServerName = exampleComˢ;
    var clientConfig = testConfig.Clone();
    clientConfig.Value.MaxVersion = version;
    clientConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(
        ΔCertificate: new slice<byte>[]{certA}.slice(),
        PrivateKey: testECDSAPrivateKey.OrTypedNil()
    )
    }.slice();
    clientConfig.Value.ClientSessionCache = NewLRUClientSessionCache(32);
    clientConfig.Value.RootCAs = Δx509.NewCertPool();
    (~clientConfig).RootCAs.AddCert(rootA);
    var nowʗ2 = now;
    clientConfig.Value.Time = () => nowʗ2;
    clientConfig.Value.InsecureSkipVerify = false;
    clientConfig.Value.ServerName = exampleComˢ;
    void testResume(ж<testing.T> tΔ1, ж<global::go.crypto.tls_package.Config> sc, ж<global::go.crypto.tls_package.Config> cc, bool expectResume) {
        tΔ1.Helper();
        var (ss, cs, errΔ1) = testHandshake(tΔ1, cc, sc);
        if (errΔ1 != default!) {
            tΔ1.Fatalf("handshake: %v"u8, errΔ1);
        }
        if (cs.DidResume != expectResume) {
            tΔ1.Fatalf("DidResume = %v; want %v"u8, cs.DidResume, expectResume);
        }
        if (ss.DidResume != expectResume) {
            tΔ1.Fatalf("DidResume = %v; want %v"u8, cs.DidResume, expectResume);
        }
    }
    testResume(Ꮡt, serverConfig, clientConfig, false);
    testResume(Ꮡt, serverConfig, clientConfig, true);
    (~clientConfig).Certificates[0].ΔCertificate = new slice<byte>[]{certB}.slice();
    // Cause GetConfigForClient to return a config cloned from the base config,
    // but with a different ClientCAs pool. This should cause resumption to fail.
    switchConfig = true;
    testResume(Ꮡt, serverConfig, clientConfig, false);
    testResume(Ꮡt, serverConfig, clientConfig, true);
}

public static void TestHandshakeChangeRootCAsResumption(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tls12ˢ3, (ж<testing.T> tΔ1) => {
        testHandshakeChangeRootCAsResumption(tΔ1, VersionTLS12);
    });
    Ꮡt.Run(tls13ˢ3, (ж<testing.T> tΔ2) => {
        testHandshakeChangeRootCAsResumption(tΔ2, VersionTLS13);
    });
}

internal static void testHandshakeChangeRootCAsResumption(ж<testing.T> Ꮡt, uint16 version) {
    ref var now = ref heap<time.Time>(out var Ꮡnow);
    now = time_package.Now();
    var tmpl = Ꮡ(new Δx509.Certificate(
        Subject: new pkix.Name(CommonName: "root"u8),
        NotBefore: now.Add((time.Duration)(-86400000000000L)),
        NotAfter: now.Add((time.Duration)(86400000000000L)),
        IsCA: true,
        BasicConstraintsValid: true
    ));
    var (rootDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, testECDSAPrivateKey.of(ecdsa.PrivateKey.ᏑPublicKey), testECDSAPrivateKey.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("CreateCertificate: %v"u8, err);
    }
    (var rootA, err) = Δx509.ParseCertificate(rootDER);
    if (err != default!) {
        Ꮡt.Fatalf("ParseCertificate: %v"u8, err);
    }
    (rootDER, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, tmpl, testRSA2048PrivateKey.of(rsa.PrivateKey.ᏑPublicKey), testRSA2048PrivateKey.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("CreateCertificate: %v"u8, err);
    }
    (var rootB, err) = Δx509.ParseCertificate(rootDER);
    if (err != default!) {
        Ꮡt.Fatalf("ParseCertificate: %v"u8, err);
    }
    tmpl = Ꮡ(new Δx509.Certificate(
        Subject: new pkix.Name(nil),
        DNSNames: new @string[]{"example.com"u8}.slice(),
        NotBefore: now.Add((time.Duration)(-86400000000000L)),
        NotAfter: now.Add((time.Duration)(86400000000000L)),
        KeyUsage: Δx509.KeyUsageDigitalSignature
    ));
    (var certA, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, rootA, testECDSAPrivateKey.of(ecdsa.PrivateKey.ᏑPublicKey), testECDSAPrivateKey.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("CreateCertificate: %v"u8, err);
    }
    (var certB, err) = Δx509.CreateCertificate(go.crypto.rand_package.Reader, tmpl, rootB, testECDSAPrivateKey.of(ecdsa.PrivateKey.ᏑPublicKey), testRSA2048PrivateKey.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("CreateCertificate: %v"u8, err);
    }
    var serverConfig = testConfig.Clone();
    serverConfig.Value.MaxVersion = version;
    serverConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(
        ΔCertificate: new slice<byte>[]{certA}.slice(),
        PrivateKey: testECDSAPrivateKey.OrTypedNil()
    )
    }.slice();
    var nowʗ1 = now;
    serverConfig.Value.Time = () => nowʗ1;
    serverConfig.Value.ClientCAs = Δx509.NewCertPool();
    (~serverConfig).ClientCAs.AddCert(rootA);
    serverConfig.Value.ClientAuth = RequireAndVerifyClientCert;
    serverConfig.Value.InsecureSkipVerify = false;
    serverConfig.Value.ServerName = exampleComˢ;
    var clientConfig = testConfig.Clone();
    clientConfig.Value.MaxVersion = version;
    clientConfig.Value.Certificates = new global::go.crypto.tls_package.Certificate[]{new(
        ΔCertificate: new slice<byte>[]{certA}.slice(),
        PrivateKey: testECDSAPrivateKey.OrTypedNil()
    )
    }.slice();
    clientConfig.Value.ClientSessionCache = NewLRUClientSessionCache(32);
    clientConfig.Value.RootCAs = Δx509.NewCertPool();
    (~clientConfig).RootCAs.AddCert(rootA);
    var nowʗ2 = now;
    clientConfig.Value.Time = () => nowʗ2;
    clientConfig.Value.InsecureSkipVerify = false;
    clientConfig.Value.ServerName = exampleComˢ;
    void testResume(ж<testing.T> tΔ1, ж<global::go.crypto.tls_package.Config> sc, ж<global::go.crypto.tls_package.Config> cc, bool expectResume) {
        tΔ1.Helper();
        var (ss, cs, errΔ1) = testHandshake(tΔ1, cc, sc);
        if (errΔ1 != default!) {
            tΔ1.Fatalf("handshake: %v"u8, errΔ1);
        }
        if (cs.DidResume != expectResume) {
            tΔ1.Fatalf("DidResume = %v; want %v"u8, cs.DidResume, expectResume);
        }
        if (ss.DidResume != expectResume) {
            tΔ1.Fatalf("DidResume = %v; want %v"u8, cs.DidResume, expectResume);
        }
    }
    testResume(Ꮡt, serverConfig, clientConfig, false);
    testResume(Ꮡt, serverConfig, clientConfig, true);
    clientConfig = clientConfig.Clone();
    clientConfig.Value.RootCAs = Δx509.NewCertPool();
    (~clientConfig).RootCAs.AddCert(rootB);
    (~serverConfig).Certificates[0].ΔCertificate = new slice<byte>[]{certB}.slice();
    testResume(Ꮡt, serverConfig, clientConfig, false);
    testResume(Ꮡt, serverConfig, clientConfig, true);
}

} // end tls_internal_test_package
