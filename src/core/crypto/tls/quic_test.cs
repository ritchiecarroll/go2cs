// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using reflect = reflect_package;
using testing = testing_package;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

internal partial struct testQUICConn {
    internal ж<testing.T> t;
    internal ж<global::go.crypto.tls_package.QUICConn> conn;
    internal map<global::go.crypto.tls_package.QUICEncryptionLevel, suiteSecret> readSecret;
    internal map<global::go.crypto.tls_package.QUICEncryptionLevel, suiteSecret> writeSecret;
    internal global::go.crypto.tls_package.QUICSessionTicketOptions ticketOpts;
    internal Action<ж<global::go.crypto.tls_package.SessionState>> onResumeSession;
    internal slice<byte> gotParams;
    internal bool earlyDataRejected;
    internal bool complete;
}

internal static ж<testQUICConn> newTestQUICClient(ж<testing.T> Ꮡt, ж<global::go.crypto.tls_package.QUICConfig> Ꮡconfig) {
    var q = Ꮡ(new testQUICConn(
        t: Ꮡt,
        conn: QUICClient(Ꮡconfig)
    ));
    var qʗ1 = q;
    Ꮡt.Cleanup(() => {
        (~qʗ1).conn.Close();
    });
    return q;
}

internal static ж<testQUICConn> newTestQUICServer(ж<testing.T> Ꮡt, ж<global::go.crypto.tls_package.QUICConfig> Ꮡconfig) {
    var q = Ꮡ(new testQUICConn(
        t: Ꮡt,
        conn: QUICServer(Ꮡconfig)
    ));
    var qʗ1 = q;
    Ꮡt.Cleanup(() => {
        (~qʗ1).conn.Close();
    });
    return q;
}

internal partial struct suiteSecret {
    internal uint16 suite;
    internal slice<byte> secret;
}

internal static void setReadSecret(this ref testQUICConn q, global::go.crypto.tls_package.QUICEncryptionLevel level, uint16 suite, slice<byte> secret) {
    {
        var (_, ok) = q.writeSecret[level, ꟷ]; if (!ok && level != QUICEncryptionLevelEarly) {
            q.t.Errorf("SetReadSecret for level %v called before SetWriteSecret"u8, level);
        }
    }
    if (level == QUICEncryptionLevelApplication && !q.complete) {
        q.t.Errorf("SetReadSecret for level %v called before HandshakeComplete"u8, level);
    }
    {
        var (_, ok) = q.readSecret[level, ꟷ]; if (ok) {
            q.t.Errorf("SetReadSecret for level %v called twice"u8, level);
        }
    }
    if (q.readSecret == default!) {
        q.readSecret = new map<global::go.crypto.tls_package.QUICEncryptionLevel, suiteSecret>{};
    }
    var exprᴛ1 = level;
    if (exprᴛ1 == QUICEncryptionLevelHandshake || exprᴛ1 == QUICEncryptionLevelEarly || exprᴛ1 == QUICEncryptionLevelApplication) {
        q.readSecret[level] = new suiteSecret(suite, secret);
    }
    else { /* default: */
        q.t.Errorf("SetReadSecret for unexpected level %v"u8, level);
    }

}

internal static void setWriteSecret(this ref testQUICConn q, global::go.crypto.tls_package.QUICEncryptionLevel level, uint16 suite, slice<byte> secret) {
    {
        var (_, ok) = q.writeSecret[level, ꟷ]; if (ok) {
            q.t.Errorf("SetWriteSecret for level %v called twice"u8, level);
        }
    }
    if (q.writeSecret == default!) {
        q.writeSecret = new map<global::go.crypto.tls_package.QUICEncryptionLevel, suiteSecret>{};
    }
    var exprᴛ1 = level;
    if (exprᴛ1 == QUICEncryptionLevelHandshake || exprᴛ1 == QUICEncryptionLevelEarly || exprᴛ1 == QUICEncryptionLevelApplication) {
        q.writeSecret[level] = new suiteSecret(suite, secret);
    }
    else { /* default: */
        q.t.Errorf("SetWriteSecret for unexpected level %v"u8, level);
    }

}

internal static error errTransportParametersRequired = errors.New("transport parameters required"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string handshakeIncompleteˢ = "handshake incomplete"u8;
internal static readonly @string unexpectedˢ2 = "unexpected QUICStoreSession event received by server"u8;

internal static error runTestQUICConnection(context.Context ctx, ж<testQUICConn> Ꮡcli, ж<testQUICConn> Ꮡsrv, Func<global::go.crypto.tls_package.QUICEvent, ж<testQUICConn>, ж<testQUICConn>, bool> onEvent) {
    ref var srv = ref Ꮡsrv.DerefOrNull();

    var (a, b) = (Ꮡcli, Ꮡsrv);
    foreach (var (_, c) in new ж<testQUICConn>[]{a, b}.slice()) {
        if (!(~(~(~(~c).conn).conn).quic).started) {
            {
                var err = (~c).conn.Start(ctx); if (err != default!) {
                    return err;
                }
            }
        }
    }
    nint idleCount = 0;
    while (ᐧ) {
        var e = (~a).conn.NextEvent();
        if (onEvent != default! && onEvent(e, a, b)) {
            continue;
        }
        var exprᴛ1 = e.Kind;
        if (exprᴛ1 == QUICNoEvent) {
            idleCount++;
            if (idleCount == 2) {
                if (!(~a).complete || !(~b).complete) {
                    return errors.New(handshakeIncompleteˢ);
                }
                return default!;
            }
            (a, b) = (b, a);
        }
        else if (exprᴛ1 == QUICSetReadSecret) {
            a.setReadSecret(e.Level, e.Suite, e.Data);
        }
        else if (exprᴛ1 == QUICSetWriteSecret) {
            a.setWriteSecret(e.Level, e.Suite, e.Data);
        }
        else if (exprᴛ1 == QUICWriteData) {
            {
                var err = (~b).conn.HandleData(e.Level, e.Data); if (err != default!) {
                    return err;
                }
            }
        }
        else if (exprᴛ1 == QUICTransportParameters) {
            a.Value.gotParams = e.Data;
            if ((~a).gotParams == default!) {
                a.Value.gotParams = new byte[]{}.slice();
            }
        }
        else if (exprᴛ1 == QUICTransportParametersRequired) {
            return errTransportParametersRequired;
        }
        else if (exprᴛ1 == QUICHandshakeDone) {
            a.Value.complete = true;
            if (a == Ꮡsrv) {
                {
                    var err = srv.conn.SendSessionTicket(srv.ticketOpts); if (err != default!) {
                        return err;
                    }
                }
            }
        }
        else if (exprᴛ1 == QUICStoreSession) {
            if (a != Ꮡcli) {
                return errors.New(unexpectedˢ2);
            }
            (~a).conn.StoreSession(e.SessionState);
        }
        else if (exprᴛ1 == QUICResumeSession) {
            if ((~a).onResumeSession != default!) {
                (~a).onResumeSession(e.SessionState);
            }
        }
        else if (exprᴛ1 == QUICRejectedEarlyData) {
            a.Value.earlyDataRejected = true;
        }

        if (e.Kind != QUICNoEvent) {
            idleCount = 0;
        }
    }
}

public static void TestQUICConnection(ж<testing.T> Ꮡt) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(Ꮡt, config);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, config);
    (~srv).conn.SetTransportParameters(default!);
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default!); if (err != default!) {
            Ꮡt.Fatalf("error during connection handshake: %v"u8, err);
        }
    }
    {
        var (_, ok) = (~cli).readSecret[QUICEncryptionLevelHandshake, ꟷ]; if (!ok) {
            Ꮡt.Errorf("client has no Handshake secret"u8);
        }
    }
    {
        var (_, ok) = (~cli).readSecret[QUICEncryptionLevelApplication, ꟷ]; if (!ok) {
            Ꮡt.Errorf("client has no Application secret"u8);
        }
    }
    {
        var (_, ok) = (~srv).readSecret[QUICEncryptionLevelHandshake, ꟷ]; if (!ok) {
            Ꮡt.Errorf("server has no Handshake secret"u8);
        }
    }
    {
        var (_, ok) = (~srv).readSecret[QUICEncryptionLevelApplication, ꟷ]; if (!ok) {
            Ꮡt.Errorf("server has no Application secret"u8);
        }
    }
    foreach (var (_, level) in new global::go.crypto.tls_package.QUICEncryptionLevel[]{QUICEncryptionLevelHandshake, QUICEncryptionLevelApplication}.slice()) {
        {
            var (_, ok) = (~cli).readSecret[level, ꟷ]; if (!ok) {
                Ꮡt.Errorf("client has no %v read secret"u8, level);
            }
        }
        {
            var (_, ok) = (~srv).readSecret[level, ꟷ]; if (!ok) {
                Ꮡt.Errorf("server has no %v read secret"u8, level);
            }
        }
        if (!reflect.DeepEqual((~cli).readSecret[level], (~srv).writeSecret[level])) {
            Ꮡt.Errorf("client read secret does not match server write secret for level %v"u8, level);
        }
        if (!reflect.DeepEqual((~cli).writeSecret[level], (~srv).readSecret[level])) {
            Ꮡt.Errorf("client write secret does not match server read secret for level %v"u8, level);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleGoDevˢ = "example.go.dev"u8;

public static void TestQUICSessionResumption(ж<testing.T> Ꮡt) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    clientConfig.Value.TLSConfig.Value.ClientSessionCache = NewLRUClientSessionCache(1);
    clientConfig.Value.TLSConfig.Value.ServerName = exampleGoDevˢ;
    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(Ꮡt, clientConfig);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, serverConfig);
    (~srv).conn.SetTransportParameters(default!);
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default!); if (err != default!) {
            Ꮡt.Fatalf("error during first connection handshake: %v"u8, err);
        }
    }
    if ((~cli).conn.ConnectionState().DidResume) {
        Ꮡt.Errorf("first connection unexpectedly used session resumption"u8);
    }
    var cli2 = newTestQUICClient(Ꮡt, clientConfig);
    (~cli2).conn.SetTransportParameters(default!);
    var srv2 = newTestQUICServer(Ꮡt, serverConfig);
    (~srv2).conn.SetTransportParameters(default!);
    {
        var err = runTestQUICConnection(context.Background(), cli2, srv2, default!); if (err != default!) {
            Ꮡt.Fatalf("error during second connection handshake: %v"u8, err);
        }
    }
    if (!(~cli2).conn.ConnectionState().DidResume) {
        Ꮡt.Errorf("second connection did not use session resumption"u8);
    }
}

public static void TestQUICFragmentaryData(ж<testing.T> Ꮡt) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    clientConfig.Value.TLSConfig.Value.ClientSessionCache = NewLRUClientSessionCache(1);
    clientConfig.Value.TLSConfig.Value.ServerName = exampleGoDevˢ;
    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(Ꮡt, clientConfig);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, serverConfig);
    (~srv).conn.SetTransportParameters(default!);
    var onEvent = (global::go.crypto.tls_package.QUICEvent e, ж<testQUICConn> src, ж<testQUICConn> dst) => {
        if (e.Kind == QUICWriteData) {
            // Provide the data one byte at a time.
            foreach (var (i, _) in e.Data) {
                {
                    var err = (~dst).conn.HandleData(e.Level, e.Data.slice(i, i + 1)); if (err != default!) {
                        Ꮡt.Errorf("HandleData: %v"u8, err);
                        break;
                    }
                }
            }
            return true;
        }
        return false;
    };
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, onEvent); if (err != default!) {
            Ꮡt.Fatalf("error during first connection handshake: %v"u8, err);
        }
    }
}

public static void TestQUICPostHandshakeClientAuthentication(ж<testing.T> Ꮡt) {
    // RFC 9001, Section 4.4.
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(Ꮡt, config);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, config);
    (~srv).conn.SetTransportParameters(default!);
    {
        var errΔ1 = runTestQUICConnection(context.Background(), cli, srv, default!); if (errΔ1 != default!) {
            Ꮡt.Fatalf("error during connection handshake: %v"u8, errΔ1);
        }
    }
    var certReq = @new<global::go.crypto.tls_package.certificateRequestMsgTLS13>();
    certReq.Value.ocspStapling = true;
    certReq.Value.scts = true;
    certReq.Value.supportedSignatureAlgorithms = supportedSignatureAlgorithms();
    var (certReqBytes, err) = certReq.marshal();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ2 = (~cli).conn.HandleData(QUICEncryptionLevelApplication, appendꓸꓸꓸ(new byte[]{
            (byte)typeCertificateRequest,
            (byte)0, (byte)0, (byte)len(certReqBytes)
        }.slice(), certReqBytes)); if (errΔ2 == default!) {
            Ꮡt.Fatalf("post-handshake authentication request: got no error, want one"u8);
        }
    }
}

public static void TestQUICPostHandshakeKeyUpdate(ж<testing.T> Ꮡt) {
    // RFC 9001, Section 6.
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(Ꮡt, config);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, config);
    (~srv).conn.SetTransportParameters(default!);
    {
        var errΔ1 = runTestQUICConnection(context.Background(), cli, srv, default!); if (errΔ1 != default!) {
            Ꮡt.Fatalf("error during connection handshake: %v"u8, errΔ1);
        }
    }
    var keyUpdate = @new<global::go.crypto.tls_package.keyUpdateMsg>();
    var (keyUpdateBytes, err) = keyUpdate.marshal();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ2 = (~cli).conn.HandleData(QUICEncryptionLevelApplication, appendꓸꓸꓸ(new byte[]{
            (byte)typeKeyUpdate,
            (byte)0, (byte)0, (byte)len(keyUpdateBytes)
        }.slice(), keyUpdateBytes)); if (!errors.Is(errΔ2, alertUnexpectedMessage)) {
            Ꮡt.Fatalf("key update request: got error %v, want alertUnexpectedMessage"u8, errΔ2);
        }
    }
}

public static void TestQUICPostHandshakeMessageTooLarge(ж<testing.T> Ꮡt) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(Ꮡt, config);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, config);
    (~srv).conn.SetTransportParameters(default!);
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default!); if (err != default!) {
            Ꮡt.Fatalf("error during connection handshake: %v"u8, err);
        }
    }
    nint size = maxHandshake + 1;
    {
        var err = (~cli).conn.HandleData(QUICEncryptionLevelApplication, new byte[]{
            (byte)typeNewSessionTicket,
            (byte)((size >> (int)(16))),
            (byte)((size >> (int)(8))),
            (byte)size
        }.slice()); if (err == default!) {
            Ꮡt.Fatalf("%v-byte post-handshake message: got no error, want one"u8, size);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nameˢ = "name"u8;

public static void TestQUICHandshakeError(ж<testing.T> Ꮡt) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    clientConfig.Value.TLSConfig.Value.InsecureSkipVerify = false;
    clientConfig.Value.TLSConfig.Value.ServerName = nameˢ;
    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(Ꮡt, clientConfig);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, serverConfig);
    (~srv).conn.SetTransportParameters(default!);
    var err = runTestQUICConnection(context.Background(), cli, srv, default!);
    if (!errors.Is(err, new tls_test_package.tls_AlertErrorᴠerror(((global::go.crypto.tls_package.AlertError)(uint8)alertBadCertificate)))) {
        Ꮡt.Errorf("connection handshake terminated with error %q, want alertBadCertificate"u8, err);
    }
    ref var e = ref heap<ж<global::go.crypto.tls_package.CertificateVerificationError>>(out var Ꮡe);
    if (!errors.As(err, Ꮡe)) {
        Ꮡt.Errorf("connection handshake terminated with error %q, want CertificateVerificationError"u8, err);
    }
}

// Test that QUICConn.ConnectionState can be used during the handshake,
// and that it reports the application protocol as soon as it has been
// negotiated.
public static void TestQUICConnectionState(ж<testing.T> Ꮡt) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    config.Value.TLSConfig.Value.NextProtos = new @string[]{"h3"u8}.slice();
    var cli = newTestQUICClient(Ꮡt, config);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, config);
    (~srv).conn.SetTransportParameters(default!);
    var cliʗ1 = cli;
    var srvʗ1 = srv;
    var onEvent = (global::go.crypto.tls_package.QUICEvent e, ж<testQUICConn> src, ж<testQUICConn> dst) => {
        var cliCS = (~cliʗ1).conn.ConnectionState();
        {
            var (_, ok) = (~cliʗ1).readSecret[QUICEncryptionLevelApplication, ꟷ]; if (ok) {
                {
                    @string want = cliCS.NegotiatedProtocol;
                    @string got = "h3"u8; if (want != got) {
                        Ꮡt.Errorf("cli.ConnectionState().NegotiatedProtocol = %q, want %q"u8, want, got);
                    }
                }
            }
        }
        var srvCS = (~srvʗ1).conn.ConnectionState();
        {
            var (_, ok) = (~srvʗ1).readSecret[QUICEncryptionLevelHandshake, ꟷ]; if (ok) {
                {
                    @string want = srvCS.NegotiatedProtocol;
                    @string got = "h3"u8; if (want != got) {
                        Ꮡt.Errorf("srv.ConnectionState().NegotiatedProtocol = %q, want %q"u8, want, got);
                    }
                }
            }
        }
        return false;
    };
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, onEvent); if (err != default!) {
            Ꮡt.Fatalf("error during connection handshake: %v"u8, err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string keyᶜ = "key"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string valueᶜ = "value"u8;

public static void TestQUICStartContextPropagation(ж<testing.T> Ꮡt) {
    @string key = keyᶜ;
    @string value = valueᶜ;
    var ctx = context.WithValue(context.Background(), key, value);
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    nint calls = 0;
    config.Value.TLSConfig.Value.GetConfigForClient = (ж<global::go.crypto.tls_package.Config>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> info) => {
        calls++;
        var (got, _) = info.Context().Value(key)._<@string>(ᐧ);
        if (got != value) {
            Ꮡt.Errorf("GetConfigForClient context key %q has value %q, want %q"u8, key, got, value);
        }
        return (default!, default!);
    };
    var cli = newTestQUICClient(Ꮡt, config);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, config);
    (~srv).conn.SetTransportParameters(default!);
    {
        var err = runTestQUICConnection(ctx, cli, srv, default!); if (err != default!) {
            Ꮡt.Fatalf("error during connection handshake: %v"u8, err);
        }
    }
    if (calls != 1) {
        Ꮡt.Errorf("GetConfigForClient called %v times, want 1"u8, calls);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string clientParamsˢ = "client params"u8;
internal static readonly @string serverParamsˢ = "server params"u8;

public static void TestQUICDelayedTransportParameters(ж<testing.T> Ꮡt) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    clientConfig.Value.TLSConfig.Value.ClientSessionCache = NewLRUClientSessionCache(1);
    clientConfig.Value.TLSConfig.Value.ServerName = exampleGoDevˢ;
    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    @string cliParams = clientParamsˢ;
    @string srvParams = serverParamsˢ;
    var cli = newTestQUICClient(Ꮡt, clientConfig);
    var srv = newTestQUICServer(Ꮡt, serverConfig);
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default!); if (!AreEqual(err, errTransportParametersRequired)) {
            Ꮡt.Fatalf("handshake with no client parameters: %v; want errTransportParametersRequired"u8, err);
        }
    }
    (~cli).conn.SetTransportParameters(slice<byte>(cliParams));
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default!); if (!AreEqual(err, errTransportParametersRequired)) {
            Ꮡt.Fatalf("handshake with no server parameters: %v; want errTransportParametersRequired"u8, err);
        }
    }
    (~srv).conn.SetTransportParameters(slice<byte>(srvParams));
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default!); if (err != default!) {
            Ꮡt.Fatalf("error during connection handshake: %v"u8, err);
        }
    }
    {
        @string got = ((@string)(~cli).gotParams);
        @string want = srvParams; if (got != want) {
            Ꮡt.Errorf("client got transport params: %q, want %q"u8, got, want);
        }
    }
    {
        @string got = ((@string)(~srv).gotParams);
        @string want = cliParams; if (got != want) {
            Ꮡt.Errorf("server got transport params: %q, want %q"u8, got, want);
        }
    }
}

public static void TestQUICEmptyTransportParameters(ж<testing.T> Ꮡt) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(Ꮡt, config);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, config);
    (~srv).conn.SetTransportParameters(default!);
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default!); if (err != default!) {
            Ꮡt.Fatalf("error during connection handshake: %v"u8, err);
        }
    }
    if ((~cli).gotParams == default!) {
        Ꮡt.Errorf("client did not get transport params"u8);
    }
    if ((~srv).gotParams == default!) {
        Ꮡt.Errorf("server did not get transport params"u8);
    }
    if (len((~cli).gotParams) != 0) {
        Ꮡt.Errorf("client got transport params: %v, want empty"u8, (~cli).gotParams);
    }
    if (len((~srv).gotParams) != 0) {
        Ꮡt.Errorf("server got transport params: %v, want empty"u8, (~srv).gotParams);
    }
}

public static void TestQUICCanceledWaitingForData(ж<testing.T> Ꮡt) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(Ꮡt, config);
    (~cli).conn.SetTransportParameters(default!);
    (~cli).conn.Start(context.Background());
    while ((~cli).conn.NextEvent().Kind != QUICNoEvent) {
    }
    var err = (~cli).conn.Close();
    if (!errors.Is(err, alertCloseNotify)) {
        Ꮡt.Errorf("conn.Close() = %v, want alertCloseNotify"u8, err);
    }
}

public static void TestQUICCanceledWaitingForTransportParams(ж<testing.T> Ꮡt) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(Ꮡt, config);
    (~cli).conn.Start(context.Background());
    while ((~cli).conn.NextEvent().Kind != QUICTransportParametersRequired) {
    }
    var err = (~cli).conn.Close();
    if (!errors.Is(err, alertCloseNotify)) {
        Ꮡt.Errorf("conn.Close() = %v, want alertCloseNotify"u8, err);
    }
}

public static void TestQUICEarlyData(ж<testing.T> Ꮡt) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    clientConfig.Value.TLSConfig.Value.ClientSessionCache = NewLRUClientSessionCache(1);
    clientConfig.Value.TLSConfig.Value.ServerName = exampleGoDevˢ;
    clientConfig.Value.TLSConfig.Value.NextProtos = new @string[]{"h3"u8}.slice();
    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    serverConfig.Value.TLSConfig.Value.NextProtos = new @string[]{"h3"u8}.slice();
    var cli = newTestQUICClient(Ꮡt, clientConfig);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, serverConfig);
    (~srv).conn.SetTransportParameters(default!);
    srv.Value.ticketOpts.EarlyData = true;
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default!); if (err != default!) {
            Ꮡt.Fatalf("error during first connection handshake: %v"u8, err);
        }
    }
    if ((~cli).conn.ConnectionState().DidResume) {
        Ꮡt.Errorf("first connection unexpectedly used session resumption"u8);
    }
    var cli2 = newTestQUICClient(Ꮡt, clientConfig);
    (~cli2).conn.SetTransportParameters(default!);
    var srv2 = newTestQUICServer(Ꮡt, serverConfig);
    (~srv2).conn.SetTransportParameters(default!);
    var onEvent = (global::go.crypto.tls_package.QUICEvent e, ж<testQUICConn> src, ж<testQUICConn> dst) => {
        var exprᴛ1 = e.Kind;
        if (exprᴛ1 == QUICStoreSession || exprᴛ1 == QUICResumeSession) {
            Ꮡt.Errorf("with EnableSessionEvents=false, got unexpected event %v"u8, e.Kind);
        }

        return false;
    };
    {
        var err = runTestQUICConnection(context.Background(), cli2, srv2, onEvent); if (err != default!) {
            Ꮡt.Fatalf("error during second connection handshake: %v"u8, err);
        }
    }
    if (!(~cli2).conn.ConnectionState().DidResume) {
        Ꮡt.Errorf("second connection did not use session resumption"u8);
    }
    var cliSecret = (~cli2).writeSecret[QUICEncryptionLevelEarly];
    if (cliSecret.secret == default!) {
        Ꮡt.Errorf("client did not receive early data write secret"u8);
    }
    var srvSecret = (~srv2).readSecret[QUICEncryptionLevelEarly];
    if (srvSecret.secret == default!) {
        Ꮡt.Errorf("server did not receive early data read secret"u8);
    }
    if (cliSecret.suite != srvSecret.suite || !bytes.Equal(cliSecret.secret, srvSecret.secret)) {
        Ꮡt.Errorf("client early data secret does not match server"u8);
    }
}

public static void TestQUICEarlyDataDeclined(ж<testing.T> Ꮡt) {
    Ꮡt.Run(serverˢ, (ж<testing.T> tΔ1) => {
        testQUICEarlyDataDeclined(tΔ1, true);
    });
    Ꮡt.Run(clientˢ, (ж<testing.T> tΔ2) => {
        testQUICEarlyDataDeclined(tΔ2, false);
    });
}

internal static void testQUICEarlyDataDeclined(ж<testing.T> Ꮡt, bool server) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.Value.EnableSessionEvents = true;
    clientConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    clientConfig.Value.TLSConfig.Value.ClientSessionCache = NewLRUClientSessionCache(1);
    clientConfig.Value.TLSConfig.Value.ServerName = exampleGoDevˢ;
    clientConfig.Value.TLSConfig.Value.NextProtos = new @string[]{"h3"u8}.slice();
    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.Value.EnableSessionEvents = true;
    serverConfig.Value.TLSConfig.Value.MinVersion = VersionTLS13;
    serverConfig.Value.TLSConfig.Value.NextProtos = new @string[]{"h3"u8}.slice();
    var cli = newTestQUICClient(Ꮡt, clientConfig);
    (~cli).conn.SetTransportParameters(default!);
    var srv = newTestQUICServer(Ꮡt, serverConfig);
    (~srv).conn.SetTransportParameters(default!);
    srv.Value.ticketOpts.EarlyData = true;
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default!); if (err != default!) {
            Ꮡt.Fatalf("error during first connection handshake: %v"u8, err);
        }
    }
    if ((~cli).conn.ConnectionState().DidResume) {
        Ꮡt.Errorf("first connection unexpectedly used session resumption"u8);
    }
    var cli2 = newTestQUICClient(Ꮡt, clientConfig);
    (~cli2).conn.SetTransportParameters(default!);
    var srv2 = newTestQUICServer(Ꮡt, serverConfig);
    (~srv2).conn.SetTransportParameters(default!);
    var declineEarlyData = (ж<global::go.crypto.tls_package.SessionState> state) => {
        state.Value.EarlyData = false;
    };
    if (server){
        srv2.Value.onResumeSession = declineEarlyData;
    } else {
        cli2.Value.onResumeSession = declineEarlyData;
    }
    {
        var err = runTestQUICConnection(context.Background(), cli2, srv2, default!); if (err != default!) {
            Ꮡt.Fatalf("error during second connection handshake: %v"u8, err);
        }
    }
    if (!(~cli2).conn.ConnectionState().DidResume) {
        Ꮡt.Errorf("second connection did not use session resumption"u8);
    }
    var (_, cliEarlyData) = (~cli2).writeSecret[QUICEncryptionLevelEarly, ꟷ];
    if (server) {
        if (!cliEarlyData) {
            Ꮡt.Errorf("client did not receive early data write secret"u8);
        }
        if (!(~cli2).earlyDataRejected) {
            Ꮡt.Errorf("client did not receive QUICEarlyDataRejected"u8);
        }
    }
    {
        var (_, srvEarlyData) = (~srv2).readSecret[QUICEncryptionLevelEarly, ꟷ]; if (srvEarlyData) {
            Ꮡt.Errorf("server received early data read secret"u8);
        }
    }
}

} // end tls_internal_test_package
