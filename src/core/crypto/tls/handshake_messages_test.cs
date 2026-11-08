// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using Δx509 = go.crypto.x509_package;
using hex = encoding.hex_package;
using math = math_package;
using rand = go.math.rand_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using quick = go.testing.quick_package;
using time = time_package;
using encoding;
using go.crypto;
using go.math;
using go.testing;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

internal static slice<global::go.crypto.tls_package.handshakeMessage> tests = [new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(Ꮡ(new clientHelloMsg())), new global::go.crypto.tls_package.serverHelloMsgжhandshakeMessage(Ꮡ(new serverHelloMsg())), new global::go.crypto.tls_package.finishedMsgжhandshakeMessage(Ꮡ(new finishedMsg())), new global::go.crypto.tls_package.certificateMsgжhandshakeMessage(Ꮡ(new certificateMsg())), new global::go.crypto.tls_package.certificateRequestMsgжhandshakeMessage(Ꮡ(new certificateRequestMsg())), new global::go.crypto.tls_package.certificateVerifyMsgжhandshakeMessage(Ꮡ(new certificateVerifyMsg(
    hasSignatureAlgorithm: true
))), new global::go.crypto.tls_package.certificateStatusMsgжhandshakeMessage(Ꮡ(new certificateStatusMsg())), new global::go.crypto.tls_package.clientKeyExchangeMsgжhandshakeMessage(Ꮡ(new clientKeyExchangeMsg())), new global::go.crypto.tls_package.newSessionTicketMsgжhandshakeMessage(Ꮡ(new newSessionTicketMsg())), new global::go.crypto.tls_package.encryptedExtensionsMsgжhandshakeMessage(Ꮡ(new encryptedExtensionsMsg())), new global::go.crypto.tls_package.endOfEarlyDataMsgжhandshakeMessage(Ꮡ(new endOfEarlyDataMsg())), new global::go.crypto.tls_package.keyUpdateMsgжhandshakeMessage(Ꮡ(new keyUpdateMsg())), new global::go.crypto.tls_package.newSessionTicketMsgTLS13жhandshakeMessage(Ꮡ(new newSessionTicketMsgTLS13())), new global::go.crypto.tls_package.certificateRequestMsgTLS13жhandshakeMessage(Ꮡ(new certificateRequestMsgTLS13())), new global::go.crypto.tls_package.certificateMsgTLS13жhandshakeMessage(Ꮡ(new certificateMsgTLS13())), new tls_test_package.tls_SessionStateжhandshakeMessage(Ꮡ(new SessionState()))
];

internal static slice<byte> mustMarshal(ж<testing.T> t, global::go.crypto.tls_package.handshakeMessage msg) {
    t.Helper();
    var (b, err) = msg.marshal();
    if (err != nil) {
        t.Fatal(err);
    }
    return b;
}

public static void TestMarshalUnmarshal(ж<testing.T> t) {
    var randΔ1 = go.math.rand_package.New(go.math.rand_package.NewSource(time_package.Now().UnixNano()));

    foreach (var (i, m) in range(tests)) {
        var ty = reflect.ValueOf(m).Type();
        var mʗ1 = m;
        var randʗ1 = randΔ1;
        var tyʗ1 = ty;
        t.Run(ty.String(), (ж<testing.T> tΔ1) => {
            nint n = 100;
            if (testing.Short()) {
                n = 5;
            }
            for (nint j = 0; j < n; j++) {
                var (v, ok) = quick.Value(tyʗ1, randʗ1);
                if (!ok) {
                    tΔ1.Errorf("#%d: failed to create value", i);
                    break;
                }

                var m1 = v.Interface()._<handshakeMessage>();
                var marshaled = mustMarshal(tΔ1, m1);
                if (!mʗ1.unmarshal(marshaled)) {
                    tΔ1.Errorf("#%d failed to unmarshal %#v %x", i, m1, marshaled);
                    break;
                }

                var (ch, okΔ1) = mʗ1._<ж<global::go.crypto.tls_package.clientHelloMsg>>(ᐧ); if (okΔ1) {
                    // extensions is special cased, as it is only populated by the
                    // server-side of a handshake and is not expected to roundtrip
                    // through marshal + unmarshal.  m ends up with the list of
                    // extensions necessary to serialize the other fields of
                    // clientHelloMsg, so check that it is non-empty, then clear it.
                    if (len(ch.extensions) == 0) {
                        tΔ1.Errorf("expected ch.extensions to be populated on unmarshal");
                    }
                    ch.extensions = default;
                }

                // clientHelloMsg and serverHelloMsg, when unmarshalled, store
                // their original representation, for later use in the handshake
                // transcript. In order to prevent DeepEqual from failing since
                // we didn't create the original message via unmarshalling, nil
                // the field.
                switch (mʗ1.type()) {
                case ж<global::go.crypto.tls_package.clientHelloMsg> tΔ2: {
                    tΔ2.original = default;
                    break;
                }
                case ж<global::go.crypto.tls_package.serverHelloMsg> tΔ2: {
                    tΔ2.original = default;
                    break;
                }}

                if (!reflect.DeepEqual(m1, mʗ1)) {
                    tΔ1.Errorf("#%d got:%#v want:%#v %x", i, mʗ1, m1, marshaled);
                    break;
                }

                if (i >= 3) {
                    // The first three message types (ClientHello,
                    // ServerHello and Finished) are allowed to
                    // have parsable prefixes because the extension
                    // data is optional and the length of the
                    // Finished varies across versions.
                    for (nint jΔ1 = 0; jΔ1 < len(marshaled); jΔ1++) {
                        if (mʗ1.unmarshal(marshaled.slice(0, jΔ1))) {
                            tΔ1.Errorf("#%d unmarshaled a prefix of length %d of %#v", i, jΔ1, m1);
                            break;
                        }
                    }
                }
            }
        });
    }
}

public static void TestFuzz(ж<testing.T> t) {
    var randΔ1 = go.math.rand_package.New(go.math.rand_package.NewSource(0));
    foreach (var (_, m) in range(tests)) {
        for (nint j = 0; j < 1000; j++) {
            nint len = randΔ1.Intn(1000);
            var bytes = randomBytes(len, randΔ1);
            // This just looks for crashes due to bounds errors etc.
            m.unmarshal(bytes);
        }
    }
}

internal static slice<byte> randomBytes(nint n, ж<rand.Rand> randΔ1) {
    var r = new slice<byte>(n);
    var (_, err) = randΔ1.Read(r); if (err != nil) {
        throw panic("rand.Read failed: " + err.Error());
    }
    return r;
}

internal static @string randomString(nint n, ж<rand.Rand> randΔ1) {
    var b = randomBytes(n, randΔ1);
    return (@string)b;
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.clientHelloMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new clientHelloMsg());
    m.vers = (uint16)randΔ1.Intn(65536);
    m.random = randomBytes(32, Ꮡrand);
    m.sessionId = randomBytes(randΔ1.Intn(32), Ꮡrand);
    m.cipherSuites = new slice<uint16>(randΔ1.Intn(63) + 1);
    for (nint i = 0; i < len(m.cipherSuites); i++) {
        var cs = (uint16)randΔ1.Int31();
        if (cs == scsvRenegotiation) {
            cs += 1;
        }
        m.cipherSuites[i] = cs;
    }
    m.compressionMethods = randomBytes(randΔ1.Intn(63) + 1, Ꮡrand);
    if (randΔ1.Intn(10) > 5) {
        m.serverName = randomString(randΔ1.Intn(255), Ꮡrand);
        while (strings.HasSuffix(m.serverName, ".")) {
            m.serverName = m.serverName.slice(0, len(m.serverName) - 1);
        }
    }
    m.ocspStapling = randΔ1.Intn(10) > 5;
    m.supportedPoints = randomBytes(randΔ1.Intn(5) + 1, Ꮡrand);
    m.supportedCurves = new slice<global::go.crypto.tls_package.CurveID>(randΔ1.Intn(5) + 1);
    foreach (var i in range(len(m.supportedCurves))) {
        m.supportedCurves[i] = (global::go.crypto.tls_package.CurveID)(uint16)(randΔ1.Intn(30000) + 1);
    }
    if (randΔ1.Intn(10) > 5) {
        m.ticketSupported = true;
        if (randΔ1.Intn(10) > 5) {
            m.sessionTicket = randomBytes(randΔ1.Intn(300), Ꮡrand);
        } else {
            m.sessionTicket = new slice<byte>(0);
        }
    }
    if (randΔ1.Intn(10) > 5) {
        m.supportedSignatureAlgorithms = supportedSignatureAlgorithms(VersionTLS12);
    }
    if (randΔ1.Intn(10) > 5) {
        m.supportedSignatureAlgorithmsCert = supportedSignatureAlgorithms(VersionTLS12);
    }
    for (nint i = 0; i < randΔ1.Intn(5); i++) {
        m.alpnProtocols = append(m.alpnProtocols, randomString(randΔ1.Intn(20) + 1, Ꮡrand));
    }
    if (randΔ1.Intn(10) > 5) {
        m.scts = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.secureRenegotiationSupported = true;
        m.secureRenegotiation = randomBytes(randΔ1.Intn(50) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.extendedMasterSecret = true;
    }
    for (nint i = 0; i < randΔ1.Intn(5); i++) {
        m.supportedVersions = append(m.supportedVersions, (uint16)(randΔ1.Intn(0xffff) + 1));
    }
    if (randΔ1.Intn(10) > 5) {
        m.cookie = randomBytes(randΔ1.Intn(500) + 1, Ꮡrand);
    }
    for (nint i = 0; i < randΔ1.Intn(5); i++) {
        global::go.crypto.tls_package.keyShare ks = default;
        ks.group = (global::go.crypto.tls_package.CurveID)(uint16)(randΔ1.Intn(30000) + 1);
        ks.data = randomBytes(randΔ1.Intn(200) + 1, Ꮡrand);
        m.keyShares = append(m.keyShares, ks);
    }
    switch (randΔ1.Intn(3)) {
    case 1: {
        m.pskModes = [pskModeDHE];
        break;
    }
    case 2: {
        m.pskModes = [pskModeDHE, pskModePlain];
        break;
    }}
    // clientHelloMsg.marshal uses echInner == false. If m.encryptedClientHello > 0 and
    // does not equal []byte{innerECHExt}, then the psk extension will be omitted,
    // so either only emit empty encryptedClientHello and psk, encryptedClientHello with
    // []byte{innerECHExt} and psk, or random encryptedClientHello and no psk.
    if (randΔ1.Intn(10) > 5) {
        m.encryptedClientHello = randomBytes(randΔ1.Intn(50) + 1, Ꮡrand);
    } else {
        if (randΔ1.Intn(10) > 5) {
            m.encryptedClientHello = [(byte)innerECHExt];
        }
        global::go.crypto.tls_package.pskIdentity psk = default;
        psk.obfuscatedTicketAge = (uint32)randΔ1.Intn(500000);
        psk.label = randomBytes(randΔ1.Intn(500) + 1, Ꮡrand);
        m.pskIdentities = append(m.pskIdentities, psk);
        m.pskBinders = append(m.pskBinders, randomBytes(randΔ1.Intn(50) + 32, Ꮡrand));
    }
    if (randΔ1.Intn(10) > 5) {
        m.quicTransportParameters = randomBytes(randΔ1.Intn(500), Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.earlyData = true;
    }

    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.serverHelloMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new serverHelloMsg());
    m.vers = (uint16)randΔ1.Intn(65536);
    m.random = randomBytes(32, Ꮡrand);
    m.sessionId = randomBytes(randΔ1.Intn(32), Ꮡrand);
    m.cipherSuite = (uint16)randΔ1.Int31();
    m.compressionMethod = (uint8)randΔ1.Intn(256);
    m.supportedPoints = randomBytes(randΔ1.Intn(5) + 1, Ꮡrand);

    if (randΔ1.Intn(10) > 5) {
        m.ocspStapling = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.ticketSupported = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.alpnProtocol = randomString(randΔ1.Intn(32) + 1, Ꮡrand);
    }

    for (nint i = 0; i < randΔ1.Intn(4); i++) {
        m.scts = append(m.scts, randomBytes(randΔ1.Intn(500) + 1, Ꮡrand));
    }

    if (randΔ1.Intn(10) > 5) {
        m.secureRenegotiationSupported = true;
        m.secureRenegotiation = randomBytes(randΔ1.Intn(50) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.extendedMasterSecret = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.supportedVersion = (uint16)(randΔ1.Intn(0xffff) + 1);
    }
    if (randΔ1.Intn(10) > 5) {
        m.cookie = randomBytes(randΔ1.Intn(500) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        for (nint i = 0; i < randΔ1.Intn(5); i++) {
            m.serverShare.group = (global::go.crypto.tls_package.CurveID)(uint16)(randΔ1.Intn(30000) + 1);
            m.serverShare.data = randomBytes(randΔ1.Intn(200) + 1, Ꮡrand);
        }
    } else if (randΔ1.Intn(10) > 5) {
        m.selectedGroup = (global::go.crypto.tls_package.CurveID)(uint16)(randΔ1.Intn(30000) + 1);
    }
    if (randΔ1.Intn(10) > 5) {
        m.selectedIdentityPresent = true;
        m.selectedIdentity = (uint16)randΔ1.Intn(0xffff);
    }
    if (randΔ1.Intn(10) > 5) {
        m.encryptedClientHello = randomBytes(randΔ1.Intn(50) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.serverNameAck = randΔ1.Intn(2) == 1;
    }

    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.encryptedExtensionsMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new encryptedExtensionsMsg());

    if (randΔ1.Intn(10) > 5) {
        m.alpnProtocol = randomString(randΔ1.Intn(32) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.earlyData = true;
    }

    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateMsg());
    nint numCerts = randΔ1.Intn(20);
    m.certificates = new slice<slice<byte>>(numCerts);
    for (nint i = 0; i < numCerts; i++) {
        m.certificates[i] = randomBytes(randΔ1.Intn(10) + 1, Ꮡrand);
    }
    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateRequestMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateRequestMsg());
    m.certificateTypes = randomBytes(randΔ1.Intn(5) + 1, Ꮡrand);
    for (nint i = 0; i < randΔ1.Intn(100); i++) {
        m.certificateAuthorities = append(m.certificateAuthorities, randomBytes(randΔ1.Intn(15) + 1, Ꮡrand));
    }
    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateVerifyMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateVerifyMsg());
    m.hasSignatureAlgorithm = true;
    m.signatureAlgorithm = (global::go.crypto.tls_package.SignatureScheme)(uint16)randΔ1.Intn(30000);
    m.signature = randomBytes(randΔ1.Intn(15) + 1, Ꮡrand);
    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateStatusMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateStatusMsg());
    m.response = randomBytes(randΔ1.Intn(10) + 1, Ꮡrand);
    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.clientKeyExchangeMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new clientKeyExchangeMsg());
    m.ciphertext = randomBytes(randΔ1.Intn(1000) + 1, Ꮡrand);
    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.finishedMsg _, ж<rand.Rand> randΔ1, nint size) {
    var m = Ꮡ(new finishedMsg());
    m.verifyData = randomBytes(12, randΔ1);
    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.newSessionTicketMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new newSessionTicketMsg());
    m.ticket = randomBytes(randΔ1.Intn(4), Ꮡrand);
    return reflect.ValueOf(m.OrTypedNil());
}

internal static slice<ж<Δx509.Certificate>> sessionTestCerts;

[GoInit] internal static void initΔ1() {
    var (cert, err) = Δx509.ParseCertificate(testRSACertificate);
    if (err != nil) {
        throw panic(err);
    }
    sessionTestCerts = append(sessionTestCerts, cert);
    (cert, err) = Δx509.ParseCertificate(testRSACertificateIssuer);
    if (err != nil) {
        throw panic(err);
    }
    sessionTestCerts = append(sessionTestCerts, cert);
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.SessionState _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var s = Ꮡ(new SessionState());
    var isTLS13 = randΔ1.Intn(10) > 5;
    if (isTLS13) {
        s.version = VersionTLS13;
    } else {
        s.version = (uint16)randΔ1.Intn(VersionTLS13);
    }
    s.isClient = randΔ1.Intn(10) > 5;
    s.cipherSuite = (uint16)randΔ1.Intn(math.MaxUint16);
    s.createdAt = (uint64)randΔ1.Int63();
    s.secret = randomBytes(randΔ1.Intn(100) + 1, Ꮡrand);
    for ((nint n, nint i) = (randΔ1.Intn(3), 0); i < n; i++) {
        s.Extra = append(s.Extra, randomBytes(randΔ1.Intn(100), Ꮡrand));
    }
    if (randΔ1.Intn(10) > 5) {
        s.EarlyData = true;
    }
    if (randΔ1.Intn(10) > 5) {
        s.extMasterSecret = true;
    }
    if (s.isClient || randΔ1.Intn(10) > 5) {
        if (randΔ1.Intn(10) > 5) {
            s.peerCertificates = sessionTestCerts;
        } else {
            s.peerCertificates = sessionTestCerts[..1];
        }
    }
    if (randΔ1.Intn(10) > 5 && s.peerCertificates != nil) {
        s.ocspResponse = randomBytes(randΔ1.Intn(100) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5 && s.peerCertificates != nil) {
        for (nint i = 0; i < randΔ1.Intn(2) + 1; i++) {
            s.scts = append(s.scts, randomBytes(randΔ1.Intn(500) + 1, Ꮡrand));
        }
    }
    if (len(s.peerCertificates) > 0) {
        for (nint i = 0; i < randΔ1.Intn(3); i++) {
            if (randΔ1.Intn(10) > 5) {
                s.verifiedChains = append(s.verifiedChains, s.peerCertificates);
            } else {
                s.verifiedChains = append(s.verifiedChains, s.peerCertificates[..1]);
            }
        }
    }
    if (randΔ1.Intn(10) > 5 && s.EarlyData) {
        s.alpnProtocol = (@string)randomBytes(randΔ1.Intn(10), Ꮡrand);
    }
    if (isTLS13) {
        if (s.isClient) {
            s.useBy = (uint64)randΔ1.Int63();
            s.ageAdd = (uint32)(randΔ1.Int63() & (int64)math.MaxUint32);
        }
    } else {
        s.curveID = (global::go.crypto.tls_package.CurveID)(uint16)(randΔ1.Intn(30000) + 1);
    }
    return reflect.ValueOf(s.OrTypedNil());
}

internal static (slice<byte>, error) marshal(this ж<global::go.crypto.tls_package.SessionState> Ꮡs) { return Ꮡs.Bytes(); }
internal static bool unmarshal(this ref global::go.crypto.tls_package.SessionState s, slice<byte> b) {
    var (ss, err) = ParseSessionState(b);
    if (err != nil) {
        return false;
    }
    s = ss.Value;
    return true;
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.endOfEarlyDataMsg _, ж<rand.Rand> randΔ1, nint size) {
    var m = Ꮡ(new endOfEarlyDataMsg());
    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.keyUpdateMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new keyUpdateMsg());
    m.updateRequested = randΔ1.Intn(10) > 5;
    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.newSessionTicketMsgTLS13 _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new newSessionTicketMsgTLS13());
    m.lifetime = (uint32)randΔ1.Intn(500000);
    m.ageAdd = (uint32)randΔ1.Intn(500000);
    m.nonce = randomBytes(randΔ1.Intn(100), Ꮡrand);
    m.label = randomBytes(randΔ1.Intn(1000), Ꮡrand);
    if (randΔ1.Intn(10) > 5) {
        m.maxEarlyData = (uint32)randΔ1.Intn(500000);
    }
    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateRequestMsgTLS13 _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateRequestMsgTLS13());
    if (randΔ1.Intn(10) > 5) {
        m.ocspStapling = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.scts = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.supportedSignatureAlgorithms = supportedSignatureAlgorithms(VersionTLS12);
    }
    if (randΔ1.Intn(10) > 5) {
        m.supportedSignatureAlgorithmsCert = supportedSignatureAlgorithms(VersionTLS12);
    }
    if (randΔ1.Intn(10) > 5) {
        m.certificateAuthorities = new slice<slice<byte>>(3);
        for (nint i = 0; i < 3; i++) {
            m.certificateAuthorities[i] = randomBytes(randΔ1.Intn(10) + 1, Ꮡrand);
        }
    }
    return reflect.ValueOf(m.OrTypedNil());
}

internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateMsgTLS13 _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateMsgTLS13());
    for (nint i = 0; i < randΔ1.Intn(2) + 1; i++) {
        m.certificate.ΔCertificate = append(
            m.certificate.ΔCertificate, randomBytes(randΔ1.Intn(500) + 1, Ꮡrand));
    }
    if (randΔ1.Intn(10) > 5) {
        m.ocspStapling = true;
        m.certificate.OCSPStaple = randomBytes(randΔ1.Intn(100) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.scts = true;
        for (nint i = 0; i < randΔ1.Intn(2) + 1; i++) {
            m.certificate.SignedCertificateTimestamps = append(
                m.certificate.SignedCertificateTimestamps, randomBytes(randΔ1.Intn(500) + 1, Ꮡrand));
        }
    }
    return reflect.ValueOf(m.OrTypedNil());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToUnmarshalInitialˢ = (@string)"Failed to unmarshal initial message"u8;
internal static readonly object cannotFindSctInˢ = (@string)"Cannot find SCT in ServerHello"u8;
internal static readonly object unmarshaledServerHelloˢ = (@string)"Unmarshaled ServerHello with empty SCT list"u8;

public static void TestRejectEmptySCTList(ж<testing.T> t) {
    // RFC 6962, Section 3.3.1 specifies that empty SCT lists are invalid.

    array<byte> random = new(32);
    var sct = slice<byte>([0x42, 0x42, 0x42, 0x42]);
    var serverHello = Ꮡ(new serverHelloMsg(
        vers: VersionTLS12,
        random: random[..],
        scts: [sct]
    ));
    var serverHelloBytes = mustMarshal(t, new global::go.crypto.tls_package.serverHelloMsgжhandshakeMessage(serverHello));

    ref var serverHelloCopy = ref heap(new global::go.crypto.tls_package.serverHelloMsg(), out var ᏑserverHelloCopy);
    if (!ᏑserverHelloCopy.unmarshal(serverHelloBytes)) {
        t.Fatal(failedToUnmarshalInitialˢ);
    }

    // Change serverHelloBytes so that the SCT list is empty
    nint i = bytes.Index(serverHelloBytes, sct);
    if (i < 0) {
        t.Fatal(cannotFindSctInˢ);
    }

    slice<byte> serverHelloEmptySCT = default;
    serverHelloEmptySCT = appendꓸꓸꓸ(serverHelloEmptySCT, serverHelloBytes.slice(0, i - 6));
    // Append the extension length and SCT list length for an empty list.
    serverHelloEmptySCT = appendꓸꓸꓸ(serverHelloEmptySCT, slice<byte>([0, 2, 0, 0]));
    serverHelloEmptySCT = appendꓸꓸꓸ(serverHelloEmptySCT, serverHelloBytes.slice(i + 4));

    // Update the handshake message length.
    serverHelloEmptySCT[1] = (byte)((len(serverHelloEmptySCT) - 4) >> 16);
    serverHelloEmptySCT[2] = (byte)((len(serverHelloEmptySCT) - 4) >> 8);
    serverHelloEmptySCT[3] = (byte)(len(serverHelloEmptySCT) - 4);

    // Update the extensions length
    serverHelloEmptySCT[42] = (byte)((len(serverHelloEmptySCT) - 44) >> 8);
    serverHelloEmptySCT[43] = (byte)(len(serverHelloEmptySCT) - 44);

    if (ᏑserverHelloCopy.unmarshal(serverHelloEmptySCT)) {
        t.Fatal(unmarshaledServerHelloˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshaledServerHelloˢ2 = (@string)"Unmarshaled ServerHello with zero-length SCT"u8;

public static void TestRejectEmptySCT(ж<testing.T> t) {
    // Not only must the SCT list be non-empty, but the SCT elements must
    // not be zero length.

    array<byte> random = new(32);
    var serverHello = Ꮡ(new serverHelloMsg(
        vers: VersionTLS12,
        random: random[..],
        scts: [default]
    ));
    var serverHelloBytes = mustMarshal(t, new global::go.crypto.tls_package.serverHelloMsgжhandshakeMessage(serverHello));

    ref var serverHelloCopy = ref heap(new global::go.crypto.tls_package.serverHelloMsg(), out var ᏑserverHelloCopy);
    if (ᏑserverHelloCopy.unmarshal(serverHelloBytes)) {
        t.Fatal(unmarshaledServerHelloˢ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshaledClientHelloˢ = (@string)"Unmarshaled ClientHello with duplicate extensions"u8;
internal static readonly object unmarshaledServerHelloˢ3 = (@string)"Unmarshaled ServerHello with duplicate extensions"u8;

public static void TestRejectDuplicateExtensions(ж<testing.T> t) {
    var (clientHelloBytes, err) = hex.DecodeString("010000440303000000000000000000000000000000000000000000000000000000000000000000000000001c0000000a000800000568656c6c6f0000000a000800000568656c6c6f");
    if (err != nil) {
        t.Fatalf("failed to decode test ClientHello: %s", err);
    }
    ref var clientHelloCopy = ref heap(new global::go.crypto.tls_package.clientHelloMsg(), out var ᏑclientHelloCopy);
    if (ᏑclientHelloCopy.unmarshal(clientHelloBytes)) {
        t.Error(unmarshaledClientHelloˢ);
    }

    (var serverHelloBytes, err) = hex.DecodeString("02000030030300000000000000000000000000000000000000000000000000000000000000000000000000080005000000050000");
    if (err != nil) {
        t.Fatalf("failed to decode test ServerHello: %s", err);
    }
    ref var serverHelloCopy = ref heap(new global::go.crypto.tls_package.serverHelloMsg(), out var ᏑserverHelloCopy);
    if (ᏑserverHelloCopy.unmarshal(serverHelloBytes)) {
        t.Fatal(unmarshaledServerHelloˢ3);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToUnmarshalˢ = (@string)"Failed to unmarshal ClientHello"u8;
internal static readonly object expectedPskIdentitiesToˢ = (@string)"expected PSK identities to be removed"u8;
internal static readonly object expectedPskBindersToBeˢ = (@string)"expected PSK binders to be removed"u8;
internal static readonly object expectedPskIdentitiesToˢ2 = (@string)"expected PSK identities to be present"u8;
internal static readonly object expectedPskBindersToBeˢ2 = (@string)"expected PSK binders to be present"u8;

internal partial struct TestECHRemoveOuterPSK_type /*dyn*/ {
    internal @string name;
    internal bool echInner;
    internal slice<byte> echExt;
    internal bool expectRemoved;
}

public static void TestECHRemoveOuterPSK(ж<testing.T> t) {
    var r = go.math.rand_package.New(go.math.rand_package.NewSource(0));

    foreach (var (_, vᴛ1) in range(slice<TestECHRemoveOuterPSK_type>([
        new(
            name: "echInner true",
            echInner: true,
            expectRemoved: false
        ),
        new(
            name: "echInner true, no ech ext",
            echInner: true,
            expectRemoved: false
        ),
        new(
            name: "echInner true, ech ext present",
            echInner: true,
            echExt: [254],
            expectRemoved: false
        ),
        new(
            name: "echInner true, ech ext present, inner ech sentinel",
            echInner: true,
            echExt: [(byte)innerECHExt],
            expectRemoved: false
        ),
        new(
            name: "echInner false, no ech ext",
            echInner: false,
            expectRemoved: false
        ),
        new(
            name: "echInner false, ech ext present",
            echInner: false,
            echExt: [254],
            expectRemoved: true
        ),
        new(
            name: "echInner false, ech ext present, inner ech sentinel",
            echInner: false,
            echExt: [(byte)innerECHExt],
            expectRemoved: false
        )
    ]))) {
        ref var tc = ref heap(new TestECHRemoveOuterPSK_type(), out var Ꮡtc);
        tc = vᴛ1;

        var rʗ1 = r;
        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var ch = (Ꮡ(new clientHelloMsg())).Generate(rʗ1, 0).Interface()._<ж<global::go.crypto.tls_package.clientHelloMsg>>();

            ch.pskBinders = [slice<byte>("test"u8)];
            ch.pskIdentities = [new(label: slice<byte>("test"u8))];
            ch.encryptedClientHello = tcʗ1.echExt;

            var (b, err) = ch.marshalMsg(tcʗ1.echInner);
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            ref var rch = ref heap(new global::go.crypto.tls_package.clientHelloMsg(), out var Ꮡrch);
            if (!Ꮡrch.unmarshal(b)) {
                tΔ1.Fatal(failedToUnmarshalˢ);
            }

            if (tcʗ1.expectRemoved) {
                if (rch.pskIdentities != nil) {
                    tΔ1.Error(expectedPskIdentitiesToˢ);
                }
                if (rch.pskBinders != nil) {
                    tΔ1.Error(expectedPskBindersToBeˢ);
                }
            } else {
                if (rch.pskIdentities == nil) {
                    tΔ1.Error(expectedPskIdentitiesToˢ2);
                }
                if (rch.pskBinders == nil) {
                    tΔ1.Error(expectedPskBindersToBeˢ2);
                }
            }
        });
    }
}

} // end tls_internal_test_package
