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

internal static slice<global::go.crypto.tls_package.handshakeMessage> tests = new global::go.crypto.tls_package.handshakeMessage[]{new global::go.crypto.tls_package.clientHelloMsgжhandshakeMessage(Ꮡ(new clientHelloMsg(nil))), new global::go.crypto.tls_package.serverHelloMsgжhandshakeMessage(Ꮡ(new serverHelloMsg(nil))), new global::go.crypto.tls_package.finishedMsgжhandshakeMessage(Ꮡ(new finishedMsg(nil))), new global::go.crypto.tls_package.certificateMsgжhandshakeMessage(Ꮡ(new certificateMsg(nil))), new global::go.crypto.tls_package.certificateRequestMsgжhandshakeMessage(Ꮡ(new certificateRequestMsg(nil))), new global::go.crypto.tls_package.certificateVerifyMsgжhandshakeMessage(Ꮡ(new certificateVerifyMsg(
    hasSignatureAlgorithm: true
))), new global::go.crypto.tls_package.certificateStatusMsgжhandshakeMessage(Ꮡ(new certificateStatusMsg(nil))), new global::go.crypto.tls_package.clientKeyExchangeMsgжhandshakeMessage(Ꮡ(new clientKeyExchangeMsg(nil))), new global::go.crypto.tls_package.newSessionTicketMsgжhandshakeMessage(Ꮡ(new newSessionTicketMsg(nil))), new global::go.crypto.tls_package.encryptedExtensionsMsgжhandshakeMessage(Ꮡ(new encryptedExtensionsMsg(nil))), new global::go.crypto.tls_package.endOfEarlyDataMsgжhandshakeMessage(Ꮡ(new endOfEarlyDataMsg(nil))), new global::go.crypto.tls_package.keyUpdateMsgжhandshakeMessage(Ꮡ(new keyUpdateMsg(nil))), new global::go.crypto.tls_package.newSessionTicketMsgTLS13жhandshakeMessage(Ꮡ(new newSessionTicketMsgTLS13(nil))), new global::go.crypto.tls_package.certificateRequestMsgTLS13жhandshakeMessage(Ꮡ(new certificateRequestMsgTLS13(nil))), new global::go.crypto.tls_package.certificateMsgTLS13жhandshakeMessage(Ꮡ(new certificateMsgTLS13(nil))), new tls_test_package.tls_SessionStateжhandshakeMessage(Ꮡ(new SessionState(nil)))
}.slice();

internal static slice<byte> mustMarshal(ж<testing.T> Ꮡt, global::go.crypto.tls_package.handshakeMessage msg) {
    Ꮡt.Helper();
    var (b, err) = msg.marshal();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return b;
}

public static void TestMarshalUnmarshal(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var randΔ1 = go.math.rand_package.New(go.math.rand_package.NewSource(time_package.Now().UnixNano()));
    foreach (var (i, m) in tests) {
        var ty = reflect.ValueOf(m).Type();
        var mʗ1 = m;
        var randʗ1 = randΔ1;
        var tyʗ1 = ty;
        Ꮡt.Run(ty.String(), (ж<testing.T> tΔ1) => {
            nint n = 100;
            if (testing.Short()) {
                n = 5;
            }
            for (nint j = 0; j < n; j++) {
                var (v, ok) = quick.Value(tyʗ1, randʗ1);
                if (!ok) {
                    tΔ1.Errorf("#%d: failed to create value"u8, i);
                    break;
                }
                var m1 = v.Interface()._<handshakeMessage>();
                var marshaled = mustMarshal(tΔ1, m1);
                if (!mʗ1.unmarshal(marshaled)) {
                    tΔ1.Errorf("#%d failed to unmarshal %#v %x"u8, i, m1, marshaled);
                    break;
                }
                {
                    var (mΔ1, okΔ1) = mʗ1._<ж<global::go.crypto.tls_package.SessionState>>(ᐧ); if (okΔ1) {
                        mΔ1.Value.activeCertHandles = default!;
                    }
                }
                {
                    var (ch, okΔ2) = mʗ1._<ж<global::go.crypto.tls_package.clientHelloMsg>>(ᐧ); if (okΔ2) {
                        // extensions is special cased, as it is only populated by the
                        // server-side of a handshake and is not expected to roundtrip
                        // through marshal + unmarshal.  m ends up with the list of
                        // extensions necessary to serialize the other fields of
                        // clientHelloMsg, so check that it is non-empty, then clear it.
                        if (len((~ch).extensions) == 0) {
                            tΔ1.Errorf("expected ch.extensions to be populated on unmarshal"u8);
                        }
                        ch.Value.extensions = default!;
                    }
                }
                // clientHelloMsg and serverHelloMsg, when unmarshalled, store
                // their original representation, for later use in the handshake
                // transcript. In order to prevent DeepEqual from failing since
                // we didn't create the original message via unmarshalling, nil
                // the field.
                switch (mʗ1.type()) {
                case ж<global::go.crypto.tls_package.clientHelloMsg> tΔ2: {
                    tΔ2.Value.original = default!;
                    break;
                }
                case ж<global::go.crypto.tls_package.serverHelloMsg> tΔ2: {
                    tΔ2.Value.original = default!;
                    break;
                }}
                if (!reflect.DeepEqual(m1, mʗ1)) {
                    tΔ1.Errorf("#%d got:%#v want:%#v %x"u8, i, mʗ1, m1, marshaled);
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
                            tΔ1.Errorf("#%d unmarshaled a prefix of length %d of %#v"u8, i, jΔ1, m1);
                            break;
                        }
                    }
                }
            }
        });
    }
}

public static void TestFuzz(ж<testing.T> Ꮡt) {
    var randΔ1 = go.math.rand_package.New(go.math.rand_package.NewSource(0));
    foreach (var (_, m) in tests) {
        for (nint j = 0; j < 1000; j++) {
            nint len = randΔ1.Intn(1000);
            var bytes = randomBytes(len, randΔ1);
            // This just looks for crashes due to bounds errors etc.
            m.unmarshal(bytes);
        }
    }
}

internal static slice<byte> randomBytes(nint n, ж<rand.Rand> Ꮡrand) {
    var r = new slice<byte>(n);
    {
        var (_, err) = Ꮡrand.Read(r); if (err != default!) {
            throw panic("rand.Read failed: " + err.Error());
        }
    }
    return r;
}

internal static @string randomString(nint n, ж<rand.Rand> Ꮡrand) {
    var b = randomBytes(n, Ꮡrand);
    return ((@string)b);
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.clientHelloMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new clientHelloMsg(nil));
    m.Value.vers = (uint16)randΔ1.Intn(65536);
    m.Value.random = randomBytes(32, Ꮡrand);
    m.Value.sessionId = randomBytes(randΔ1.Intn(32), Ꮡrand);
    m.Value.cipherSuites = new slice<uint16>(randΔ1.Intn(63) + 1);
    for (nint i = 0; i < len((~m).cipherSuites); i++) {
        var cs = (uint16)randΔ1.Int31();
        if (cs == scsvRenegotiation) {
            cs += 1;
        }
        m.Value.cipherSuites[i] = cs;
    }
    m.Value.compressionMethods = randomBytes(randΔ1.Intn(63) + 1, Ꮡrand);
    if (randΔ1.Intn(10) > 5) {
        m.Value.serverName = randomString(randΔ1.Intn(255), Ꮡrand);
        while (strings.HasSuffix((~m).serverName, "."u8)) {
            m.Value.serverName = (~m).serverName.slice(0, len((~m).serverName) - 1);
        }
    }
    m.Value.ocspStapling = randΔ1.Intn(10) > 5;
    m.Value.supportedPoints = randomBytes(randΔ1.Intn(5) + 1, Ꮡrand);
    m.Value.supportedCurves = new slice<global::go.crypto.tls_package.CurveID>(randΔ1.Intn(5) + 1);
    foreach (var (i, _) in (~m).supportedCurves) {
        m.Value.supportedCurves[i] = ((global::go.crypto.tls_package.CurveID)(uint16)(randΔ1.Intn(30000) + 1));
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.ticketSupported = true;
        if (randΔ1.Intn(10) > 5){
            m.Value.sessionTicket = randomBytes(randΔ1.Intn(300), Ꮡrand);
        } else {
            m.Value.sessionTicket = new slice<byte>(0);
        }
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.supportedSignatureAlgorithms = supportedSignatureAlgorithms();
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.supportedSignatureAlgorithmsCert = supportedSignatureAlgorithms();
    }
    for (nint i = 0; i < randΔ1.Intn(5); i++) {
        m.Value.alpnProtocols = append((~m).alpnProtocols, randomString(randΔ1.Intn(20) + 1, Ꮡrand));
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.scts = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.secureRenegotiationSupported = true;
        m.Value.secureRenegotiation = randomBytes(randΔ1.Intn(50) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.extendedMasterSecret = true;
    }
    for (nint i = 0; i < randΔ1.Intn(5); i++) {
        m.Value.supportedVersions = append((~m).supportedVersions, (uint16)(randΔ1.Intn(0xffff) + 1));
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.cookie = randomBytes(randΔ1.Intn(500) + 1, Ꮡrand);
    }
    for (nint i = 0; i < randΔ1.Intn(5); i++) {
        global::go.crypto.tls_package.keyShare ks = default!;
        ks.group = ((global::go.crypto.tls_package.CurveID)(uint16)(randΔ1.Intn(30000) + 1));
        ks.data = randomBytes(randΔ1.Intn(200) + 1, Ꮡrand);
        m.Value.keyShares = append((~m).keyShares, ks);
    }
    switch (randΔ1.Intn(3)) {
    case 1: {
        m.Value.pskModes = new uint8[]{pskModeDHE}.slice();
        break;
    }
    case 2: {
        m.Value.pskModes = new uint8[]{pskModeDHE, pskModePlain}.slice();
        break;
    }}

    for (nint i = 0; i < randΔ1.Intn(5); i++) {
        global::go.crypto.tls_package.pskIdentity psk = default!;
        psk.obfuscatedTicketAge = (uint32)randΔ1.Intn(500000);
        psk.label = randomBytes(randΔ1.Intn(500) + 1, Ꮡrand);
        m.Value.pskIdentities = append((~m).pskIdentities, psk);
        m.Value.pskBinders = append((~m).pskBinders, randomBytes(randΔ1.Intn(50) + 32, Ꮡrand));
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.quicTransportParameters = randomBytes(randΔ1.Intn(500), Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.earlyData = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.encryptedClientHello = randomBytes(randΔ1.Intn(50) + 1, Ꮡrand);
    }
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.serverHelloMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new serverHelloMsg(nil));
    m.Value.vers = (uint16)randΔ1.Intn(65536);
    m.Value.random = randomBytes(32, Ꮡrand);
    m.Value.sessionId = randomBytes(randΔ1.Intn(32), Ꮡrand);
    m.Value.cipherSuite = (uint16)randΔ1.Int31();
    m.Value.compressionMethod = (uint8)randΔ1.Intn(256);
    m.Value.supportedPoints = randomBytes(randΔ1.Intn(5) + 1, Ꮡrand);
    if (randΔ1.Intn(10) > 5) {
        m.Value.ocspStapling = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.ticketSupported = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.alpnProtocol = randomString(randΔ1.Intn(32) + 1, Ꮡrand);
    }
    for (nint i = 0; i < randΔ1.Intn(4); i++) {
        m.Value.scts = append((~m).scts, randomBytes(randΔ1.Intn(500) + 1, Ꮡrand));
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.secureRenegotiationSupported = true;
        m.Value.secureRenegotiation = randomBytes(randΔ1.Intn(50) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.extendedMasterSecret = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.supportedVersion = (uint16)(randΔ1.Intn(0xffff) + 1);
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.cookie = randomBytes(randΔ1.Intn(500) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5){
        for (nint i = 0; i < randΔ1.Intn(5); i++) {
            m.Value.serverShare.group = ((global::go.crypto.tls_package.CurveID)(uint16)(randΔ1.Intn(30000) + 1));
            m.Value.serverShare.data = randomBytes(randΔ1.Intn(200) + 1, Ꮡrand);
        }
    } else 
    if (randΔ1.Intn(10) > 5) {
        m.Value.selectedGroup = ((global::go.crypto.tls_package.CurveID)(uint16)(randΔ1.Intn(30000) + 1));
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.selectedIdentityPresent = true;
        m.Value.selectedIdentity = (uint16)randΔ1.Intn(0xffff);
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.encryptedClientHello = randomBytes(randΔ1.Intn(50) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.serverNameAck = randΔ1.Intn(2) == 1;
    }
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.encryptedExtensionsMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new encryptedExtensionsMsg(nil));
    if (randΔ1.Intn(10) > 5) {
        m.Value.alpnProtocol = randomString(randΔ1.Intn(32) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.earlyData = true;
    }
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateMsg(nil));
    nint numCerts = randΔ1.Intn(20);
    m.Value.certificates = new slice<slice<byte>>(numCerts);
    for (nint i = 0; i < numCerts; i++) {
        m.Value.certificates[i] = randomBytes(randΔ1.Intn(10) + 1, Ꮡrand);
    }
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateRequestMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateRequestMsg(nil));
    m.Value.certificateTypes = randomBytes(randΔ1.Intn(5) + 1, Ꮡrand);
    for (nint i = 0; i < randΔ1.Intn(100); i++) {
        m.Value.certificateAuthorities = append((~m).certificateAuthorities, randomBytes(randΔ1.Intn(15) + 1, Ꮡrand));
    }
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateVerifyMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateVerifyMsg(nil));
    m.Value.hasSignatureAlgorithm = true;
    m.Value.signatureAlgorithm = ((global::go.crypto.tls_package.SignatureScheme)(uint16)randΔ1.Intn(30000));
    m.Value.signature = randomBytes(randΔ1.Intn(15) + 1, Ꮡrand);
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateStatusMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateStatusMsg(nil));
    m.Value.response = randomBytes(randΔ1.Intn(10) + 1, Ꮡrand);
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.clientKeyExchangeMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new clientKeyExchangeMsg(nil));
    m.Value.ciphertext = randomBytes(randΔ1.Intn(1000) + 1, Ꮡrand);
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.finishedMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    var m = Ꮡ(new finishedMsg(nil));
    m.Value.verifyData = randomBytes(12, Ꮡrand);
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.newSessionTicketMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new newSessionTicketMsg(nil));
    m.Value.ticket = randomBytes(randΔ1.Intn(4), Ꮡrand);
    return reflect.ValueOf(m.OrTypedNil());
}

internal static slice<ж<Δx509.Certificate>> sessionTestCerts;

[GoInit] internal static void initΔ1() {
    var (cert, err) = Δx509.ParseCertificate(testRSACertificate);
    if (err != default!) {
        throw panic(err);
    }
    sessionTestCerts = append(sessionTestCerts, cert);
    (cert, err) = Δx509.ParseCertificate(testRSACertificateIssuer);
    if (err != default!) {
        throw panic(err);
    }
    sessionTestCerts = append(sessionTestCerts, cert);
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.SessionState _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var s = Ꮡ(new SessionState(nil));
    var isTLS13 = randΔ1.Intn(10) > 5;
    if (isTLS13){
        s.Value.version = VersionTLS13;
    } else {
        s.Value.version = (uint16)randΔ1.Intn(VersionTLS13);
    }
    s.Value.isClient = randΔ1.Intn(10) > 5;
    s.Value.cipherSuite = (uint16)randΔ1.Intn(math.MaxUint16);
    s.Value.createdAt = (uint64)randΔ1.Int63();
    s.Value.secret = randomBytes(randΔ1.Intn(100) + 1, Ꮡrand);
    for ((nint n, nint i) = (randΔ1.Intn(3), 0); i < n; i++) {
        s.Value.Extra = append((~s).Extra, randomBytes(randΔ1.Intn(100), Ꮡrand));
    }
    if (randΔ1.Intn(10) > 5) {
        s.Value.EarlyData = true;
    }
    if (randΔ1.Intn(10) > 5) {
        s.Value.extMasterSecret = true;
    }
    if ((~s).isClient || randΔ1.Intn(10) > 5) {
        if (randΔ1.Intn(10) > 5){
            s.Value.peerCertificates = sessionTestCerts;
        } else {
            s.Value.peerCertificates = sessionTestCerts[..1];
        }
    }
    if (randΔ1.Intn(10) > 5 && (~s).peerCertificates != default!) {
        s.Value.ocspResponse = randomBytes(randΔ1.Intn(100) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5 && (~s).peerCertificates != default!) {
        for (nint i = 0; i < randΔ1.Intn(2) + 1; i++) {
            s.Value.scts = append((~s).scts, randomBytes(randΔ1.Intn(500) + 1, Ꮡrand));
        }
    }
    if (len((~s).peerCertificates) > 0) {
        for (nint i = 0; i < randΔ1.Intn(3); i++) {
            if (randΔ1.Intn(10) > 5){
                s.Value.verifiedChains = append((~s).verifiedChains, (~s).peerCertificates);
            } else {
                s.Value.verifiedChains = append((~s).verifiedChains, (~s).peerCertificates[..1]);
            }
        }
    }
    if (randΔ1.Intn(10) > 5 && (~s).EarlyData) {
        s.Value.alpnProtocol = ((@string)randomBytes(randΔ1.Intn(10), Ꮡrand));
    }
    if ((~s).isClient) {
        if (isTLS13) {
            s.Value.useBy = (uint64)randΔ1.Int63();
            s.Value.ageAdd = (uint32)((int64)(randΔ1.Int63() & (int64)math.MaxUint32));
        }
    }
    return reflect.ValueOf(s.OrTypedNil());
}

internal static (slice<byte>, error) marshal(this ж<global::go.crypto.tls_package.SessionState> Ꮡs) {
    return Ꮡs.Bytes();
}

[GoRecv] internal static bool unmarshal(this ref global::go.crypto.tls_package.SessionState s, slice<byte> b) {
    var (ss, err) = ParseSessionState(b);
    if (err != default!) {
        return false;
    }
    s = ss.Value;
    return true;
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.endOfEarlyDataMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    var m = Ꮡ(new endOfEarlyDataMsg(nil));
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.keyUpdateMsg _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new keyUpdateMsg(nil));
    m.Value.updateRequested = randΔ1.Intn(10) > 5;
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.newSessionTicketMsgTLS13 _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new newSessionTicketMsgTLS13(nil));
    m.Value.lifetime = (uint32)randΔ1.Intn(500000);
    m.Value.ageAdd = (uint32)randΔ1.Intn(500000);
    m.Value.nonce = randomBytes(randΔ1.Intn(100), Ꮡrand);
    m.Value.label = randomBytes(randΔ1.Intn(1000), Ꮡrand);
    if (randΔ1.Intn(10) > 5) {
        m.Value.maxEarlyData = (uint32)randΔ1.Intn(500000);
    }
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateRequestMsgTLS13 _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateRequestMsgTLS13(nil));
    if (randΔ1.Intn(10) > 5) {
        m.Value.ocspStapling = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.scts = true;
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.supportedSignatureAlgorithms = supportedSignatureAlgorithms();
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.supportedSignatureAlgorithmsCert = supportedSignatureAlgorithms();
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.certificateAuthorities = new slice<slice<byte>>(3);
        for (nint i = 0; i < 3; i++) {
            m.Value.certificateAuthorities[i] = randomBytes(randΔ1.Intn(10) + 1, Ꮡrand);
        }
    }
    return reflect.ValueOf(m.OrTypedNil());
}

[GoRecv] internal static reflectꓸValue Generate(this ref global::go.crypto.tls_package.certificateMsgTLS13 _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var m = Ꮡ(new certificateMsgTLS13(nil));
    for (nint i = 0; i < randΔ1.Intn(2) + 1; i++) {
        m.Value.certificate.ΔCertificate = append(
            (~m).certificate.ΔCertificate, randomBytes(randΔ1.Intn(500) + 1, Ꮡrand));
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.ocspStapling = true;
        m.Value.certificate.OCSPStaple = randomBytes(randΔ1.Intn(100) + 1, Ꮡrand);
    }
    if (randΔ1.Intn(10) > 5) {
        m.Value.scts = true;
        for (nint i = 0; i < randΔ1.Intn(2) + 1; i++) {
            m.Value.certificate.SignedCertificateTimestamps = append(
                (~m).certificate.SignedCertificateTimestamps, randomBytes(randΔ1.Intn(500) + 1, Ꮡrand));
        }
    }
    return reflect.ValueOf(m.OrTypedNil());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToUnmarshalInitialˢ = (@string)"Failed to unmarshal initial message"u8;
internal static readonly object cannotFindSctInˢ = (@string)"Cannot find SCT in ServerHello"u8;
internal static readonly object unmarshaledServerHelloˢ = (@string)"Unmarshaled ServerHello with empty SCT list"u8;

public static void TestRejectEmptySCTList(ж<testing.T> Ꮡt) {
    // RFC 6962, Section 3.3.1 specifies that empty SCT lists are invalid.
    array<byte> random = new(32);
    var sct = new byte[]{0x42, 0x42, 0x42, 0x42}.slice();
    var serverHello = Ꮡ(new serverHelloMsg(
        vers: VersionTLS12,
        random: random[..],
        scts: new slice<byte>[]{sct}.slice()
    ));
    var serverHelloBytes = mustMarshal(Ꮡt, new global::go.crypto.tls_package.serverHelloMsgжhandshakeMessage(serverHello));
    ref var serverHelloCopy = ref heap(new global::go.crypto.tls_package.serverHelloMsg(), out var ᏑserverHelloCopy);
    if (!ᏑserverHelloCopy.unmarshal(serverHelloBytes)) {
        Ꮡt.Fatal(failedToUnmarshalInitialˢ);
    }
    // Change serverHelloBytes so that the SCT list is empty
    nint i = bytes.Index(serverHelloBytes, sct);
    if (i < 0) {
        Ꮡt.Fatal(cannotFindSctInˢ);
    }
    slice<byte> serverHelloEmptySCT = default!;
    serverHelloEmptySCT = appendꓸꓸꓸ(serverHelloEmptySCT, serverHelloBytes.slice(0, i - 6));
    // Append the extension length and SCT list length for an empty list.
    serverHelloEmptySCT = appendꓸꓸꓸ(serverHelloEmptySCT, new byte[]{0, 2, 0, 0}.slice());
    serverHelloEmptySCT = appendꓸꓸꓸ(serverHelloEmptySCT, serverHelloBytes.slice(i + 4));
    // Update the handshake message length.
    serverHelloEmptySCT[1] = (byte)(((len(serverHelloEmptySCT) - 4) >> (int)(16)));
    serverHelloEmptySCT[2] = (byte)(((len(serverHelloEmptySCT) - 4) >> (int)(8)));
    serverHelloEmptySCT[3] = (byte)(len(serverHelloEmptySCT) - 4);
    // Update the extensions length
    serverHelloEmptySCT[42] = (byte)(((len(serverHelloEmptySCT) - 44) >> (int)(8)));
    serverHelloEmptySCT[43] = (byte)(len(serverHelloEmptySCT) - 44);
    if (ᏑserverHelloCopy.unmarshal(serverHelloEmptySCT)) {
        Ꮡt.Fatal(unmarshaledServerHelloˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshaledServerHelloˢ2 = (@string)"Unmarshaled ServerHello with zero-length SCT"u8;

public static void TestRejectEmptySCT(ж<testing.T> Ꮡt) {
    // Not only must the SCT list be non-empty, but the SCT elements must
    // not be zero length.
    array<byte> random = new(32);
    var serverHello = Ꮡ(new serverHelloMsg(
        vers: VersionTLS12,
        random: random[..],
        scts: new slice<byte>[]{default!}.slice()
    ));
    var serverHelloBytes = mustMarshal(Ꮡt, new global::go.crypto.tls_package.serverHelloMsgжhandshakeMessage(serverHello));
    ref var serverHelloCopy = ref heap(new global::go.crypto.tls_package.serverHelloMsg(), out var ᏑserverHelloCopy);
    if (ᏑserverHelloCopy.unmarshal(serverHelloBytes)) {
        Ꮡt.Fatal(unmarshaledServerHelloˢ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshaledClientHelloˢ = (@string)"Unmarshaled ClientHello with duplicate extensions"u8;
internal static readonly object unmarshaledServerHelloˢ3 = (@string)"Unmarshaled ServerHello with duplicate extensions"u8;

public static void TestRejectDuplicateExtensions(ж<testing.T> Ꮡt) {
    var (clientHelloBytes, err) = hex.DecodeString("010000440303000000000000000000000000000000000000000000000000000000000000000000000000001c0000000a000800000568656c6c6f0000000a000800000568656c6c6f"u8);
    if (err != default!) {
        Ꮡt.Fatalf("failed to decode test ClientHello: %s"u8, err);
    }
    ref var clientHelloCopy = ref heap(new global::go.crypto.tls_package.clientHelloMsg(), out var ᏑclientHelloCopy);
    if (ᏑclientHelloCopy.unmarshal(clientHelloBytes)) {
        Ꮡt.Error(unmarshaledClientHelloˢ);
    }
    (var serverHelloBytes, err) = hex.DecodeString("02000030030300000000000000000000000000000000000000000000000000000000000000000000000000080005000000050000"u8);
    if (err != default!) {
        Ꮡt.Fatalf("failed to decode test ServerHello: %s"u8, err);
    }
    ref var serverHelloCopy = ref heap(new global::go.crypto.tls_package.serverHelloMsg(), out var ᏑserverHelloCopy);
    if (ᏑserverHelloCopy.unmarshal(serverHelloBytes)) {
        Ꮡt.Fatal(unmarshaledServerHelloˢ3);
    }
}

} // end tls_internal_test_package
