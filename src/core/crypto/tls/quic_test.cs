// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using strings = strings_package;
using sync = sync_package;
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
    internal error gotError;
    internal bool earlyDataRejected;
    internal bool complete;
}

internal static ж<testQUICConn> newTestQUICClient(ж<testing.T> t, ж<global::go.crypto.tls_package.QUICConfig> config) {
    var q = Ꮡ(new testQUICConn(
        t: t,
        conn: QUICClient(config)
    ));
    var qʗ1 = q;
    t.Cleanup(() => {
        qʗ1.conn.Close();
    });
    return q;
}

internal static ж<testQUICConn> newTestQUICServer(ж<testing.T> t, ж<global::go.crypto.tls_package.QUICConfig> config) {
    var q = Ꮡ(new testQUICConn(
        t: t,
        conn: QUICServer(config)
    ));
    var qʗ1 = q;
    t.Cleanup(() => {
        qʗ1.conn.Close();
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
            q.t.Errorf("SetReadSecret for level %v called before SetWriteSecret", level);
        }
    }
    if (level == QUICEncryptionLevelApplication && !q.complete) {
        q.t.Errorf("SetReadSecret for level %v called before HandshakeComplete", level);
    }
    {
        var (_, ok) = q.readSecret[level, ꟷ]; if (ok) {
            q.t.Errorf("SetReadSecret for level %v called twice", level);
        }
    }
    if (q.readSecret == nil) {
        q.readSecret = new map<global::go.crypto.tls_package.QUICEncryptionLevel, suiteSecret>{};
    }
    switch (level) {
    case var ᴛ when ᴛ == QUICEncryptionLevelHandshake || ᴛ == QUICEncryptionLevelEarly || ᴛ == QUICEncryptionLevelApplication: {
        q.readSecret[level] = new suiteSecret(suite, secret);
        break;
    }
    default: {
        q.t.Errorf("SetReadSecret for unexpected level %v", level);
        break;
    }}
}

internal static void setWriteSecret(this ref testQUICConn q, global::go.crypto.tls_package.QUICEncryptionLevel level, uint16 suite, slice<byte> secret) {
    {
        var (_, ok) = q.writeSecret[level, ꟷ]; if (ok) {
            q.t.Errorf("SetWriteSecret for level %v called twice", level);
        }
    }
    if (q.writeSecret == nil) {
        q.writeSecret = new map<global::go.crypto.tls_package.QUICEncryptionLevel, suiteSecret>{};
    }
    switch (level) {
    case var ᴛ when ᴛ == QUICEncryptionLevelHandshake || ᴛ == QUICEncryptionLevelEarly || ᴛ == QUICEncryptionLevelApplication: {
        q.writeSecret[level] = new suiteSecret(suite, secret);
        break;
    }
    default: {
        q.t.Errorf("SetWriteSecret for unexpected level %v", level);
        break;
    }}
}

internal static error errTransportParametersRequired = errors.New("transport parameters required");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string handshakeIncompleteˢ => ref handshakeIncompleteˢᵇ;
private static readonly @string handshakeIncompleteˢᵇ = "handshake incomplete"u8;
internal static ref readonly @string unexpectedˢ2 => ref unexpectedˢ2ᵇ;
private static readonly @string unexpectedˢ2ᵇ = "unexpected QUICStoreSession event received by server"u8;
internal static ref readonly @string unexpectedQUICErrorEventˢ => ref unexpectedQUICErrorEventˢᵇ;
private static readonly @string unexpectedQUICErrorEventˢᵇ = "unexpected QUICErrorEvent with no Err"u8;

internal static error runTestQUICConnection(context.Context ctx, ж<testQUICConn> cli, ж<testQUICConn> Ꮡsrv, Func<global::go.crypto.tls_package.QUICEvent, ж<testQUICConn>, ж<testQUICConn>, bool> onEvent) {
    ref var srv = ref Ꮡsrv.DerefOrNull();

    var (a, b) = (cli, Ꮡsrv);
    foreach (var (_, c) in slice<ж<testQUICConn>>([a, b])) {
        if (!c.conn.conn.quic.started) {
            {
                var err = c.conn.Start(ctx); if (err != nil) {
                    return err;
                }
            }
        }
    }
    nint idleCount = 0;
    for (;;) {
        var e = a.conn.NextEvent();
        if (onEvent != nil && onEvent(e, a, b)) {
            continue;
        }
        if (a.gotError != nil && e.Kind != QUICNoEvent) {
            return fmt.Errorf("unexpected event %v after QUICErrorEvent"u8, e.Kind);
        }
        switch (e.Kind) {
        case var ᴛ when ᴛ == QUICNoEvent: {
            idleCount++;
            if (idleCount == 2) {
                if (!a.complete || !b.complete) {
                    return errors.New(handshakeIncompleteˢ);
                }
                return default;
            }
            (a, b) = (b, a);
            break;
        }
        case var ᴛ when ᴛ == QUICSetReadSecret: {
            a.setReadSecret(e.Level, e.Suite, e.Data);
            break;
        }
        case var ᴛ when ᴛ == QUICSetWriteSecret: {
            a.setWriteSecret(e.Level, e.Suite, e.Data);
            break;
        }
        case var ᴛ when ᴛ == QUICWriteData: {
            {
                var err = b.conn.HandleData(e.Level, e.Data); if (err != nil) {
                    return err;
                }
            }
            break;
        }
        case var ᴛ when ᴛ == QUICTransportParameters: {
            a.gotParams = e.Data;
            if (a.gotParams == nil) {
                a.gotParams = slice<byte>([]);
            }
            break;
        }
        case var ᴛ when ᴛ == QUICTransportParametersRequired: {
            return errTransportParametersRequired;
        }
        case var ᴛ when ᴛ == QUICHandshakeDone: {
            a.complete = true;
            if (a == Ꮡsrv) {
                {
                    var err = srv.conn.SendSessionTicket(srv.ticketOpts); if (err != nil) {
                        return err;
                    }
                }
            }
            break;
        }
        case var ᴛ when ᴛ == QUICStoreSession: {
            if (a != cli) {
                return errors.New(unexpectedˢ2);
            }
            a.conn.StoreSession(e.SessionState);
            break;
        }
        case var ᴛ when ᴛ == QUICResumeSession: {
            if (a.onResumeSession != nil) {
                a.onResumeSession(e.SessionState);
            }
            break;
        }
        case var ᴛ when ᴛ == QUICRejectedEarlyData: {
            a.earlyDataRejected = true;
            break;
        }
        case var ᴛ when ᴛ == QUICErrorEvent: {
            if (e.Err == nil) {
                return errors.New(unexpectedQUICErrorEventˢ);
            }
            a.gotError = e.Err;
            break;
        }}
        if (e.Kind != QUICNoEvent) {
            idleCount = 0;
        }
    }
}

public static void TestQUICConnection(ж<testing.T> t) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.TLSConfig.MinVersion = VersionTLS13;

    var cli = newTestQUICClient(t, config);
    cli.conn.SetTransportParameters(default);

    var srv = newTestQUICServer(t, config);
    srv.conn.SetTransportParameters(default);

    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default); if (err != nil) {
            t.Fatalf("error during connection handshake: %v", err);
        }
    }

    {
        var (_, ok) = cli.readSecret[QUICEncryptionLevelHandshake, ꟷ]; if (!ok) {
            t.Errorf("client has no Handshake secret");
        }
    }
    {
        var (_, ok) = cli.readSecret[QUICEncryptionLevelApplication, ꟷ]; if (!ok) {
            t.Errorf("client has no Application secret");
        }
    }
    {
        var (_, ok) = srv.readSecret[QUICEncryptionLevelHandshake, ꟷ]; if (!ok) {
            t.Errorf("server has no Handshake secret");
        }
    }
    {
        var (_, ok) = srv.readSecret[QUICEncryptionLevelApplication, ꟷ]; if (!ok) {
            t.Errorf("server has no Application secret");
        }
    }
    foreach (var (_, level) in slice<global::go.crypto.tls_package.QUICEncryptionLevel>([QUICEncryptionLevelHandshake, QUICEncryptionLevelApplication])) {
        {
            var (_, ok) = cli.readSecret[level, ꟷ]; if (!ok) {
                t.Errorf("client has no %v read secret", level);
            }
        }
        {
            var (_, ok) = srv.readSecret[level, ꟷ]; if (!ok) {
                t.Errorf("server has no %v read secret", level);
            }
        }
        if (!reflect.DeepEqual(cli.readSecret[level], srv.writeSecret[level])) {
            t.Errorf("client read secret does not match server write secret for level %v", level);
        }
        if (!reflect.DeepEqual(cli.writeSecret[level], srv.readSecret[level])) {
            t.Errorf("client write secret does not match server read secret for level %v", level);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exampleGoDevˢ => ref exampleGoDevˢᵇ;
private static readonly @string exampleGoDevˢᵇ = "example.go.dev"u8;

public static void TestQUICSessionResumption(ж<testing.T> t) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.TLSConfig.MinVersion = VersionTLS13;
    clientConfig.TLSConfig.ClientSessionCache = NewLRUClientSessionCache(1);
    clientConfig.TLSConfig.ServerName = exampleGoDevˢ;

    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.TLSConfig.MinVersion = VersionTLS13;

    var cli = newTestQUICClient(t, clientConfig);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, serverConfig);
    srv.conn.SetTransportParameters(default);
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default); if (err != nil) {
            t.Fatalf("error during first connection handshake: %v", err);
        }
    }
    if (cli.conn.ConnectionState().DidResume) {
        t.Errorf("first connection unexpectedly used session resumption");
    }

    var cli2 = newTestQUICClient(t, clientConfig);
    cli2.conn.SetTransportParameters(default);
    var srv2 = newTestQUICServer(t, serverConfig);
    srv2.conn.SetTransportParameters(default);
    {
        var err = runTestQUICConnection(context.Background(), cli2, srv2, default); if (err != nil) {
            t.Fatalf("error during second connection handshake: %v", err);
        }
    }
    if (!cli2.conn.ConnectionState().DidResume) {
        t.Errorf("second connection did not use session resumption");
    }

    clientConfig.TLSConfig.SessionTicketsDisabled = true;
    var cli3 = newTestQUICClient(t, clientConfig);
    cli3.conn.SetTransportParameters(default);
    var srv3 = newTestQUICServer(t, serverConfig);
    srv3.conn.SetTransportParameters(default);
    {
        var err = runTestQUICConnection(context.Background(), cli3, srv3, default); if (err != nil) {
            t.Fatalf("error during third connection handshake: %v", err);
        }
    }
    if (cli3.conn.ConnectionState().DidResume) {
        t.Errorf("third connection unexpectedly used session resumption");
    }
}

public static void TestQUICFragmentaryData(ж<testing.T> t) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.TLSConfig.MinVersion = VersionTLS13;
    clientConfig.TLSConfig.ClientSessionCache = NewLRUClientSessionCache(1);
    clientConfig.TLSConfig.ServerName = exampleGoDevˢ;

    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.TLSConfig.MinVersion = VersionTLS13;

    var cli = newTestQUICClient(t, clientConfig);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, serverConfig);
    srv.conn.SetTransportParameters(default);
    var onEvent = (global::go.crypto.tls_package.QUICEvent e, ж<testQUICConn> src, ж<testQUICConn> dst) => {
        if (e.Kind == QUICWriteData) {
            // Provide the data one byte at a time.
            foreach (var i in range(e.Data)) {
                {
                    var err = dst.conn.HandleData(e.Level, e.Data.slice(i, i + 1)); if (err != nil) {
                        t.Errorf("HandleData: %v", err);
                        break;
                    }
                }
            }
            return true;
        }
        return false;
    };
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, onEvent); if (err != nil) {
            t.Fatalf("error during first connection handshake: %v", err);
        }
    }
}

public static void TestQUICPostHandshakeClientAuthentication(ж<testing.T> t) {
    // RFC 9001, Section 4.4.
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.TLSConfig.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(t, config);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, config);
    srv.conn.SetTransportParameters(default);
    {
        var errΔ1 = runTestQUICConnection(context.Background(), cli, srv, default); if (errΔ1 != nil) {
            t.Fatalf("error during connection handshake: %v", errΔ1);
        }
    }

    var certReq = @new<global::go.crypto.tls_package.certificateRequestMsgTLS13>();
    certReq.ocspStapling = true;
    certReq.scts = true;
    certReq.supportedSignatureAlgorithms = supportedSignatureAlgorithms(VersionTLS13);
    var (certReqBytes, err) = certReq.marshal();
    if (err != nil) {
        t.Fatal(err);
    }
    {
        var errΔ2 = cli.conn.HandleData(QUICEncryptionLevelApplication, appendꓸꓸꓸ(slice<byte>([
            (byte)typeCertificateRequest,
            (byte)0, (byte)0, (byte)len(certReqBytes)
        ]), certReqBytes)); if (errΔ2 == nil) {
            t.Fatalf("post-handshake authentication request: got no error, want one");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedKeyUpdateˢ => ref unexpectedKeyUpdateˢᵇ;
private static readonly @string unexpectedKeyUpdateˢᵇ = "unexpected key update message"u8;

public static void TestQUICPostHandshakeKeyUpdate(ж<testing.T> t) {
    // RFC 9001, Section 6.
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.TLSConfig.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(t, config);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, config);
    srv.conn.SetTransportParameters(default);
    {
        var errΔ1 = runTestQUICConnection(context.Background(), cli, srv, default); if (errΔ1 != nil) {
            t.Fatalf("error during connection handshake: %v", errΔ1);
        }
    }

    var keyUpdate = @new<global::go.crypto.tls_package.keyUpdateMsg>();
    var (keyUpdateBytes, err) = keyUpdate.marshal();
    if (err != nil) {
        t.Fatal(err);
    }
    @string expectedErr = unexpectedKeyUpdateˢ;
    {
        err = cli.conn.HandleData(QUICEncryptionLevelApplication, keyUpdateBytes); if (err == nil) {
            t.Fatalf("key update request: expected error from post-handshake key update, got nil");
        } else if (!strings.Contains(err.Error(), expectedErr)) {
            t.Fatalf("key update request: got error %v, expected substring %q", err, expectedErr);
        }
    }
}

public static void TestQUICPostHandshakeMessageTooLarge(ж<testing.T> t) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.TLSConfig.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(t, config);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, config);
    srv.conn.SetTransportParameters(default);
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default); if (err != nil) {
            t.Fatalf("error during connection handshake: %v", err);
        }
    }

    nint size = maxHandshake + 1;
    {
        var err = cli.conn.HandleData(QUICEncryptionLevelApplication, slice<byte>([
            (byte)typeNewSessionTicket,
            (byte)(size >> 16),
            (byte)(size >> 8),
            (byte)size
        ])); if (err == nil) {
            t.Fatalf("%v-byte post-handshake message: got no error, want one", size);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nameˢ => ref nameˢᵇ;
private static readonly @string nameˢᵇ = "name"u8;

public static void TestQUICHandshakeError(ж<testing.T> t) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.TLSConfig.MinVersion = VersionTLS13;
    clientConfig.TLSConfig.InsecureSkipVerify = false;
    clientConfig.TLSConfig.ServerName = nameˢ;

    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.TLSConfig.MinVersion = VersionTLS13;

    var cli = newTestQUICClient(t, clientConfig);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, serverConfig);
    srv.conn.SetTransportParameters(default);
    var err = runTestQUICConnection(context.Background(), cli, srv, default);
    if (!errors.Is(err, new tls_test_package.tls_AlertErrorᴠerror(((global::go.crypto.tls_package.AlertError)(uint8)alertBadCertificate)))) {
        t.Errorf("connection handshake terminated with error %q, want alertBadCertificate", err);
    }
    {
        var (_, ok) = errors.AsType<ж<global::go.crypto.tls_package.CertificateVerificationError>>(err); if (!ok) {
            t.Errorf("connection handshake terminated with error %q, want CertificateVerificationError", err);
        }
    }

    var ev = cli.conn.NextEvent();
    if (ev.Kind != QUICErrorEvent) {
        t.Errorf("client.NextEvent: no QUICErrorEvent, want one");
    }
    if (!AreEqual(ev.Err, err)) {
        t.Errorf("client.NextEvent: want same error returned by Start, got %v", ev.Err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string errorReturnedByˢ => ref errorReturnedByˢᵇ;
private static readonly @string errorReturnedByˢᵇ = "error returned by GetEncryptedClientHelloKeys"u8;

// Test that we can report an error produced by the GetEncryptedClientHelloKeys function.
public static void TestQUICECHKeyError(ж<testing.T> t) {
    var getECHKeysError = errors.New(errorReturnedByˢ);
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.TLSConfig.MinVersion = VersionTLS13;
    config.TLSConfig.NextProtos = slice<@string>(["h3"]);
    var getECHKeysErrorʗ1 = getECHKeysError;
    config.TLSConfig.GetEncryptedClientHelloKeys = (slice<global::go.crypto.tls_package.EncryptedClientHelloKey>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> _Δp0) => (default, getECHKeysErrorʗ1);
    var cli = newTestQUICClient(t, config);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, config);

    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default); if (!AreEqual(err, errTransportParametersRequired)) {
            t.Fatalf("handshake with no client parameters: %v; want errTransportParametersRequired", err);
        }
    }
    srv.conn.SetTransportParameters(default);
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default); if (err == nil) {
            t.Fatalf("handshake with GetEncryptedClientHelloKeys errors: nil, want error");
        }
    }
    if (srv.gotError == nil) {
        t.Fatalf("after GetEncryptedClientHelloKeys error, server did not see QUICErrorEvent");
    }
    {
        var (_, ok) = errors.AsType<global::go.crypto.tls_package.AlertError>(srv.gotError); if (!ok) {
            t.Errorf("connection handshake terminated with error %T, want AlertError", srv.gotError);
        }
    }
    if (!errors.Is(srv.gotError, getECHKeysError)) {
        t.Errorf("connection handshake terminated with error %v, want error returned by GetEncryptedClientHelloKeys", srv.gotError);
    }
}

// Test that QUICConn.ConnectionState can be used during the handshake,
// and that it reports the application protocol as soon as it has been
// negotiated.
public static void TestQUICConnectionState(ж<testing.T> t) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.TLSConfig.MinVersion = VersionTLS13;
    config.TLSConfig.NextProtos = slice<@string>(["h3"]);
    var cli = newTestQUICClient(t, config);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, config);
    srv.conn.SetTransportParameters(default);
    var cliʗ1 = cli;
    var srvʗ1 = srv;
    var onEvent = (global::go.crypto.tls_package.QUICEvent e, ж<testQUICConn> src, ж<testQUICConn> dst) => {
        var cliCS = cliʗ1.conn.ConnectionState();
        {
            var (_, ok) = cliʗ1.readSecret[QUICEncryptionLevelApplication, ꟷ]; if (ok) {
                {
                    @string want = cliCS.NegotiatedProtocol;
                    @string got = "h3"; if (want != got) {
                        t.Errorf("cli.ConnectionState().NegotiatedProtocol = %q, want %q", want, got);
                    }
                }
            }
        }
        var srvCS = srvʗ1.conn.ConnectionState();
        {
            var (_, ok) = srvʗ1.readSecret[QUICEncryptionLevelHandshake, ꟷ]; if (ok) {
                {
                    @string want = srvCS.NegotiatedProtocol;
                    @string got = "h3"; if (want != got) {
                        t.Errorf("srv.ConnectionState().NegotiatedProtocol = %q, want %q", want, got);
                    }
                }
            }
        }
        return false;
    };
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, onEvent); if (err != nil) {
            t.Fatalf("error during connection handshake: %v", err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string keyᶜ => ref keyᶜᵇ;
private static readonly @string keyᶜᵇ = "key"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string valueᶜ => ref valueᶜᵇ;
private static readonly @string valueᶜᵇ = "value"u8;

public static void TestQUICStartContextPropagation(ж<testing.T> t) {
    @string key = keyᶜ;
    @string value = valueᶜ;
    var ctx = context.WithValue(context.Background(), key, value);
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.TLSConfig.MinVersion = VersionTLS13;
    nint calls = 0;
    config.TLSConfig.GetConfigForClient = (ж<global::go.crypto.tls_package.Config>, error) (ж<global::go.crypto.tls_package.ClientHelloInfo> info) => {
        calls++;
        var (got, _) = info.Context().Value(key)._<@string>(ᐧ);
        if (got != value) {
            t.Errorf("GetConfigForClient context key %q has value %q, want %q", key, got, value);
        }
        return (default, default);
    };
    var cli = newTestQUICClient(t, config);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, config);
    srv.conn.SetTransportParameters(default);
    {
        var err = runTestQUICConnection(ctx, cli, srv, default); if (err != nil) {
            t.Fatalf("error during connection handshake: %v", err);
        }
    }
    if (calls != 1) {
        t.Errorf("GetConfigForClient called %v times, want 1", calls);
    }
}

public static void TestQUICContextCancelation(ж<testing.T> t) {
    var (ctx, cancel) = context.WithCancel(context.Background());
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.TLSConfig.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(t, config);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, config);
    srv.conn.SetTransportParameters(default);
    // Verify that canceling the connection context concurrently does not cause any races.
    // See https://go.dev/issue/77274.
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    var cliʗ1 = cli;
    var ctxʗ1 = ctx;
    var srvʗ1 = srv;
    Ꮡwg.Go(() => {
        _ = runTestQUICConnection(ctxʗ1, cliʗ1, srvʗ1, default);
    });
    Ꮡwg.Go(cancel);
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string clientParamsˢ => ref clientParamsˢᵇ;
private static readonly @string clientParamsˢᵇ = "client params"u8;
internal static ref readonly @string serverParamsˢ => ref serverParamsˢᵇ;
private static readonly @string serverParamsˢᵇ = "server params"u8;

public static void TestQUICDelayedTransportParameters(ж<testing.T> t) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.TLSConfig.MinVersion = VersionTLS13;
    clientConfig.TLSConfig.ClientSessionCache = NewLRUClientSessionCache(1);
    clientConfig.TLSConfig.ServerName = exampleGoDevˢ;

    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.TLSConfig.MinVersion = VersionTLS13;

    @string cliParams = clientParamsˢ;
    @string srvParams = serverParamsˢ;

    var cli = newTestQUICClient(t, clientConfig);
    var srv = newTestQUICServer(t, serverConfig);
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default); if (!AreEqual(err, errTransportParametersRequired)) {
            t.Fatalf("handshake with no client parameters: %v; want errTransportParametersRequired", err);
        }
    }
    cli.conn.SetTransportParameters(slice<byte>(cliParams));
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default); if (!AreEqual(err, errTransportParametersRequired)) {
            t.Fatalf("handshake with no server parameters: %v; want errTransportParametersRequired", err);
        }
    }
    srv.conn.SetTransportParameters(slice<byte>(srvParams));
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default); if (err != nil) {
            t.Fatalf("error during connection handshake: %v", err);
        }
    }

    {
        @string got = (@string)cli.gotParams;
        @string want = srvParams; if (got != want) {
            t.Errorf("client got transport params: %q, want %q", got, want);
        }
    }
    {
        @string got = (@string)srv.gotParams;
        @string want = cliParams; if (got != want) {
            t.Errorf("server got transport params: %q, want %q", got, want);
        }
    }
}

public static void TestQUICEmptyTransportParameters(ж<testing.T> t) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.TLSConfig.MinVersion = VersionTLS13;

    var cli = newTestQUICClient(t, config);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, config);
    srv.conn.SetTransportParameters(default);
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default); if (err != nil) {
            t.Fatalf("error during connection handshake: %v", err);
        }
    }

    if (cli.gotParams == nil) {
        t.Errorf("client did not get transport params");
    }
    if (srv.gotParams == nil) {
        t.Errorf("server did not get transport params");
    }
    if (len(cli.gotParams) != 0) {
        t.Errorf("client got transport params: %v, want empty", cli.gotParams);
    }
    if (len(srv.gotParams) != 0) {
        t.Errorf("server got transport params: %v, want empty", srv.gotParams);
    }
}

public static void TestQUICCanceledWaitingForData(ж<testing.T> t) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.TLSConfig.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(t, config);
    cli.conn.SetTransportParameters(default);
    cli.conn.Start(context.Background());
    while (cli.conn.NextEvent().Kind != QUICNoEvent) {
    }
    var err = cli.conn.Close();
    if (!errors.Is(err, alertCloseNotify)) {
        t.Errorf("conn.Close() = %v, want alertCloseNotify", err);
    }
}

public static void TestQUICCanceledWaitingForTransportParams(ж<testing.T> t) {
    var config = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    config.TLSConfig.MinVersion = VersionTLS13;
    var cli = newTestQUICClient(t, config);
    cli.conn.Start(context.Background());
    while (cli.conn.NextEvent().Kind != QUICTransportParametersRequired) {
    }
    var err = cli.conn.Close();
    if (!errors.Is(err, alertCloseNotify)) {
        t.Errorf("conn.Close() = %v, want alertCloseNotify", err);
    }
}

public static void TestQUICEarlyData(ж<testing.T> t) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.TLSConfig.MinVersion = VersionTLS13;
    clientConfig.TLSConfig.ClientSessionCache = NewLRUClientSessionCache(1);
    clientConfig.TLSConfig.ServerName = exampleGoDevˢ;
    clientConfig.TLSConfig.NextProtos = slice<@string>(["h3"]);

    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.TLSConfig.MinVersion = VersionTLS13;
    serverConfig.TLSConfig.NextProtos = slice<@string>(["h3"]);

    var cli = newTestQUICClient(t, clientConfig);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, serverConfig);
    srv.conn.SetTransportParameters(default);
    srv.ticketOpts.EarlyData = true;
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default); if (err != nil) {
            t.Fatalf("error during first connection handshake: %v", err);
        }
    }
    if (cli.conn.ConnectionState().DidResume) {
        t.Errorf("first connection unexpectedly used session resumption");
    }

    var cli2 = newTestQUICClient(t, clientConfig);
    cli2.conn.SetTransportParameters(default);
    var srv2 = newTestQUICServer(t, serverConfig);
    srv2.conn.SetTransportParameters(default);
    var onEvent = (global::go.crypto.tls_package.QUICEvent e, ж<testQUICConn> src, ж<testQUICConn> dst) => {
        switch (e.Kind) {
        case var ᴛ when ᴛ == QUICStoreSession || ᴛ == QUICResumeSession: {
            t.Errorf("with EnableSessionEvents=false, got unexpected event %v", e.Kind);
            break;
        }}
        return false;
    };
    {
        var err = runTestQUICConnection(context.Background(), cli2, srv2, onEvent); if (err != nil) {
            t.Fatalf("error during second connection handshake: %v", err);
        }
    }
    if (!cli2.conn.ConnectionState().DidResume) {
        t.Errorf("second connection did not use session resumption");
    }
    var cliSecret = cli2.writeSecret[QUICEncryptionLevelEarly];
    if (cliSecret.secret == nil) {
        t.Errorf("client did not receive early data write secret");
    }
    var srvSecret = srv2.readSecret[QUICEncryptionLevelEarly];
    if (srvSecret.secret == nil) {
        t.Errorf("server did not receive early data read secret");
    }
    if (cliSecret.suite != srvSecret.suite || !bytes.Equal(cliSecret.secret, srvSecret.secret)) {
        t.Errorf("client early data secret does not match server");
    }
}

public static void TestQUICEarlyDataDeclined(ж<testing.T> t) {
    t.Run(serverˢ, (ж<testing.T> tΔ1) => {
        testQUICEarlyDataDeclined(tΔ1, true);
    });
    t.Run(clientˢ, (ж<testing.T> tΔ2) => {
        testQUICEarlyDataDeclined(tΔ2, false);
    });
}

internal static void testQUICEarlyDataDeclined(ж<testing.T> t, bool server) {
    var clientConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    clientConfig.EnableSessionEvents = true;
    clientConfig.TLSConfig.MinVersion = VersionTLS13;
    clientConfig.TLSConfig.ClientSessionCache = NewLRUClientSessionCache(1);
    clientConfig.TLSConfig.ServerName = exampleGoDevˢ;
    clientConfig.TLSConfig.NextProtos = slice<@string>(["h3"]);

    var serverConfig = Ꮡ(new QUICConfig(TLSConfig: testConfig.Clone()));
    serverConfig.EnableSessionEvents = true;
    serverConfig.TLSConfig.MinVersion = VersionTLS13;
    serverConfig.TLSConfig.NextProtos = slice<@string>(["h3"]);

    var cli = newTestQUICClient(t, clientConfig);
    cli.conn.SetTransportParameters(default);
    var srv = newTestQUICServer(t, serverConfig);
    srv.conn.SetTransportParameters(default);
    srv.ticketOpts.EarlyData = true;
    {
        var err = runTestQUICConnection(context.Background(), cli, srv, default); if (err != nil) {
            t.Fatalf("error during first connection handshake: %v", err);
        }
    }
    if (cli.conn.ConnectionState().DidResume) {
        t.Errorf("first connection unexpectedly used session resumption");
    }

    var cli2 = newTestQUICClient(t, clientConfig);
    cli2.conn.SetTransportParameters(default);
    var srv2 = newTestQUICServer(t, serverConfig);
    srv2.conn.SetTransportParameters(default);
    var declineEarlyData = (ж<global::go.crypto.tls_package.SessionState> state) => {
        state.EarlyData = false;
    };
    if (server) {
        srv2.onResumeSession = declineEarlyData;
    } else {
        cli2.onResumeSession = declineEarlyData;
    }
    {
        var err = runTestQUICConnection(context.Background(), cli2, srv2, default); if (err != nil) {
            t.Fatalf("error during second connection handshake: %v", err);
        }
    }
    if (!cli2.conn.ConnectionState().DidResume) {
        t.Errorf("second connection did not use session resumption");
    }
    var (_, cliEarlyData) = cli2.writeSecret[QUICEncryptionLevelEarly, ꟷ];
    if (server) {
        if (!cliEarlyData) {
            t.Errorf("client did not receive early data write secret");
        }
        if (!cli2.earlyDataRejected) {
            t.Errorf("client did not receive QUICEarlyDataRejected");
        }
    }
    {
        var (_, srvEarlyData) = srv2.readSecret[QUICEncryptionLevelEarly, ꟷ]; if (srvEarlyData) {
            t.Errorf("server received early data read secret");
        }
    }
}

} // end tls_internal_test_package
