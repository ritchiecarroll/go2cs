// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using context = context_package;
using crypto = crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using rsa = go.crypto.rsa_package;
using subtle = go.crypto.subtle_package;
using fips140tls = go.crypto.tls.@internal.fips140tls_package;
using Δx509 = go.crypto.x509_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using byteorder = go.@internal.byteorder_package;
using io = io_package;
using time = time_package;
using go.@internal;
using go.crypto;
using go.crypto.tls.@internal;
using go.sync;
using math;

partial class tls_package {

// serverHandshakeState contains details of a server handshake in progress.
// It's discarded once the handshake has completed.
partial struct serverHandshakeState {
    internal ж<Conn> c;
    internal context.Context ctx;
    internal ж<clientHelloMsg> clientHello;
    internal ж<serverHelloMsg> hello;
    internal ж<cipherSuite> suite;
    internal bool ecdheOk;
    internal bool ecSignOk;
    internal bool rsaDecryptOk;
    internal bool rsaSignOk;
    internal ж<SessionState> sessionState;
    internal ΔfinishedHash finishedHash;
    internal slice<byte> masterSecret;
    internal ж<Certificate> cert;
}

// serverHandshake performs a TLS handshake as a server.
internal static error serverHandshake(this ж<Conn> Ꮡc, context.Context ctx) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (clientHello, ech, err) = Ꮡc.readClientHello(ctx);
    if (err != default!) {
        return err;
    }
    if (c.vers == VersionTLS13) {
        ref var hsΔ1 = ref heap<serverHandshakeStateTLS13>(out var ᏑhsΔ1);
        hsΔ1 = new serverHandshakeStateTLS13(
            c: Ꮡc,
            ctx: ctx,
            clientHello: clientHello,
            echContext: ech
        );
        return ᏑhsΔ1.handshake();
    }
    ref var hs = ref heap<serverHandshakeState>(out var Ꮡhs);
    hs = new serverHandshakeState(
        c: Ꮡc,
        ctx: ctx,
        clientHello: clientHello
    );
    return Ꮡhs.handshake();
}

internal static error handshake(this ж<serverHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    {
        var err = hs.processClientHello(); if (err != default!) {
            return err;
        }
    }
    // For an overview of TLS handshaking, see RFC 5246, Section 7.3.
    c.Value.buffering = true;
    {
        var err = Ꮡhs.checkForResumption(); if (err != default!) {
            return err;
        }
    }
    if (hs.sessionState != nil){
        // The client has included a session ticket and so we do an abbreviated handshake.
        {
            var err = Ꮡhs.doResumeHandshake(); if (err != default!) {
                return err;
            }
        }
        {
            var err = hs.establishKeys(); if (err != default!) {
                return err;
            }
        }
        {
            var err = Ꮡhs.sendSessionTicket(); if (err != default!) {
                return err;
            }
        }
        {
            var err = Ꮡhs.sendFinished((~c).serverFinished[..]); if (err != default!) {
                return err;
            }
        }
        {
            var (_, err) = c.flush(); if (err != default!) {
                return err;
            }
        }
        c.Value.clientFinishedIsFirst = false;
        {
            var err = Ꮡhs.readFinished(default!); if (err != default!) {
                return err;
            }
        }
    } else {
        // The client didn't include a session ticket, or it wasn't
        // valid so we do a full handshake.
        {
            var err = Ꮡhs.pickCipherSuite(); if (err != default!) {
                return err;
            }
        }
        {
            var err = Ꮡhs.doFullHandshake(); if (err != default!) {
                return err;
            }
        }
        {
            var err = hs.establishKeys(); if (err != default!) {
                return err;
            }
        }
        {
            var err = Ꮡhs.readFinished((~c).clientFinished[..]); if (err != default!) {
                return err;
            }
        }
        c.Value.clientFinishedIsFirst = true;
        c.Value.buffering = true;
        {
            var err = Ꮡhs.sendSessionTicket(); if (err != default!) {
                return err;
            }
        }
        {
            var err = Ꮡhs.sendFinished(default!); if (err != default!) {
                return err;
            }
        }
        {
            var (_, err) = c.flush(); if (err != default!) {
                return err;
            }
        }
    }
    c.Value.ekm = ekmFromMasterSecret((~c).vers, hs.suite, hs.masterSecret, (~hs.clientHello).random, (~hs.hello).random);
    c.of(Conn.ᏑisHandshakeComplete).Store(true);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsEncryptedClientHelloˢ = "tls: Encrypted Client Hello cannot be used pre-TLS 1.3"u8;

// readClientHello reads a ClientHello message and selects the protocol version.
internal static (ж<clientHelloMsg>, ж<echServerContext>, error) readClientHello(this ж<Conn> Ꮡc, context.Context ctx) {
    ref var c = ref Ꮡc.DerefOrNull();

    // clientHelloMsg is included in the transcript, but we haven't initialized
    // it yet. The respective handshake functions will record it themselves.
    var (msg, err) = Ꮡc.readHandshake(default!);
    if (err != default!) {
        return (default!, default!, err);
    }
    var (clientHello, ok) = msg._<ж<clientHelloMsg>>(ᐧ);
    if (!ok) {
        Ꮡc.sendAlert(alertUnexpectedMessage);
        return (default!, default!, unexpectedMessageError(clientHello.OrTypedNil(), msg));
    }
    // ECH processing has to be done before we do any other negotiation based on
    // the contents of the client hello, since we may swap it out completely.
    ж<echServerContext> ech = default!;
    if (len((~clientHello).encryptedClientHello) != 0) {
        (clientHello, ech, err) = Ꮡc.processECHClientHello(clientHello);
        if (err != default!) {
            return (default!, default!, err);
        }
    }
    ж<Config> configForClient = default!;
    var originalConfig = c.config;
    if ((~c.config).GetConfigForClient != default!) {
        var chi = clientHelloInfo(ctx, ref (Ꮡc).DerefOrNull(), ref (clientHello).DerefOrNull());
        {
            (configForClient, err) = (~c.config).GetConfigForClient(chi); if (err != default!){
                Ꮡc.sendAlert(alertInternalError);
                return (default!, default!, err);
            } else 
            if (configForClient != nil) {
                c.config = configForClient;
            }
        }
    }
    c.ticketKeys = originalConfig.ticketKeys(configForClient);
    var clientVersions = clientHello.Value.supportedVersions;
    if (len((~clientHello).supportedVersions) == 0) {
        clientVersions = supportedVersionsFromMax((~clientHello).vers);
    }
    (c.vers, ok) = c.config.mutualVersion(roleServer, clientVersions);
    if (!ok) {
        Ꮡc.sendAlert(alertProtocolVersion);
        return (default!, default!, fmt.Errorf("tls: client offered only unsupported versions: %x"u8, clientVersions));
    }
    c.haveVers = true;
    c.@in.version = c.vers;
    c.@out.version = c.vers;
    // This check reflects some odd specification implied behavior. Client-facing servers
    // are supposed to reject hellos with outer ECH and inner ECH that offers 1.2, but
    // backend servers are allowed to accept hellos with inner ECH that offer 1.2, since
    // they cannot expect client-facing servers to behave properly. Since we act as both
    // a client-facing and backend server, we only enforce 1.3 being negotiated if we
    // saw a hello with outer ECH first. The spec probably should've made this an error,
    // but it didn't, and this matches the boringssl behavior.
    if (c.vers != VersionTLS13 && (ech != nil && !(~ech).inner)) {
        Ꮡc.sendAlert(alertIllegalParameter);
        return (default!, default!, errors.New(tlsEncryptedClientHelloˢ));
    }
    if ((~c.config).MinVersion == 0 && c.vers < VersionTLS12) {
        tls10server.Value(); // ensure godebug is initialized
        tls10server.IncNonDefault();
    }
    return (clientHello, ech, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsClientDoesNotSupportˢ = "tls: client does not support uncompressed connections"u8;

internal static error processClientHello(this ref serverHandshakeState hs) {
    var c = hs.c;
    hs.hello = @new<serverHelloMsg>();
    hs.hello.Value.vers = c.Value.vers;
    var foundCompression = false;
    // We only support null compression, so check that the client offered it.
    foreach (var (_, compression) in (~hs.clientHello).compressionMethods) {
        if (compression == compressionNone) {
            foundCompression = true;
            break;
        }
    }
    if (!foundCompression) {
        c.sendAlert(alertHandshakeFailure);
        return errors.New(tlsClientDoesNotSupportˢ);
    }
    hs.hello.Value.random = new slice<byte>(32);
    var serverRandom = hs.hello.Value.random;
    // Downgrade protection canaries. See RFC 8446, Section 4.1.3.
    var maxVers = (~c).config.maxSupportedVersion(roleServer);
    if (maxVers >= VersionTLS12 && (~c).vers < maxVers || testingOnlyForceDowngradeCanary) {
        if ((~c).vers == VersionTLS12){
            copy(serverRandom[24..], downgradeCanaryTLS12);
        } else {
            copy(serverRandom[24..], downgradeCanaryTLS11);
        }
        serverRandom = serverRandom[..24];
    }
    var (_, err) = io.ReadFull((~c).config.rand(), serverRandom);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    if (len((~hs.clientHello).secureRenegotiation) != 0) {
        c.sendAlert(alertHandshakeFailure);
        return errors.New(tlsInitialHandshakeHadˢ);
    }
    hs.hello.Value.extendedMasterSecret = hs.clientHello.Value.extendedMasterSecret;
    hs.hello.Value.secureRenegotiationSupported = hs.clientHello.Value.secureRenegotiationSupported;
    hs.hello.Value.compressionMethod = compressionNone;
    if (len((~hs.clientHello).serverName) > 0) {
        c.Value.serverName = hs.clientHello.Value.serverName;
    }
    (var selectedProto, err) = negotiateALPN((~(~c).config).NextProtos, (~hs.clientHello).alpnProtocols, false);
    if (err != default!) {
        c.sendAlert(alertNoApplicationProtocol);
        return err;
    }
    hs.hello.Value.alpnProtocol = selectedProto;
    c.Value.clientProtocol = selectedProto;
    (hs.cert, err) = (~c).config.getCertificate(clientHelloInfo(hs.ctx, ref (c).DerefOrNull(), ref (hs.clientHello).DerefOrNull()));
    if (err != default!) {
        if (AreEqual(err, errNoCertificates)){
            c.sendAlert(alertUnrecognizedName);
        } else {
            c.sendAlert(alertInternalError);
        }
        return err;
    }
    if ((~hs.clientHello).scts) {
        hs.hello.Value.scts = hs.cert.Value.SignedCertificateTimestamps;
    }
    hs.ecdheOk = supportsECDHE((~c).config, (~c).vers, (~hs.clientHello).supportedCurves, (~hs.clientHello).supportedPoints);
    if (hs.ecdheOk && len((~hs.clientHello).supportedPoints) > 0) {
        // Although omitting the ec_point_formats extension is permitted, some
        // old OpenSSL version will refuse to handshake if not present.
        //
        // Per RFC 4492, section 5.1.2, implementations MUST support the
        // uncompressed point format. See golang.org/issue/31943.
        hs.hello.Value.supportedPoints = new uint8[]{pointFormatUncompressed}.slice();
    }
    {
        var (priv, ok) = (~hs.cert).PrivateKey._<crypto.Signer>(ᐧ); if (ok) {
            switch (priv.Public().type()) {
            case ж<ecdsa.PublicKey>: {
                hs.ecSignOk = true;
                break;
            }
            case ed25519.PublicKey: {
                hs.ecSignOk = true;
                break;
            }
            case ж<rsa.PublicKey>: {
                hs.rsaSignOk = true;
                break;
            }
            default: {
                c.sendAlert(alertInternalError);
                return fmt.Errorf("tls: unsupported signing key type (%T)"u8, priv.Public());
            }}

        }
    }
    {
        var (priv, ok) = (~hs.cert).PrivateKey._<crypto.Decrypter>(ᐧ); if (ok) {
            switch (priv.Public().type()) {
            case ж<rsa.PublicKey>: {
                hs.rsaDecryptOk = true;
                break;
            }
            default: {
                c.sendAlert(alertInternalError);
                return fmt.Errorf("tls: unsupported decryption key type (%T)"u8, priv.Public());
            }}

        }
    }
    return default!;
}

// negotiateALPN picks a shared ALPN protocol that both sides support in server
// preference order. If ALPN is not configured or the peer doesn't support it,
// it returns "" and no error.
internal static (@string, error) negotiateALPN(slice<@string> serverProtos, slice<@string> clientProtos, bool quic) {
    if (len(serverProtos) == 0 || len(clientProtos) == 0) {
        if (quic && len(serverProtos) != 0) {
            // RFC 9001, Section 8.1
            return ("", fmt.Errorf("tls: client did not request an application protocol"u8));
        }
        return ("", default!);
    }
    bool http11fallback = default!;
    foreach (var (_, s) in serverProtos) {
        foreach (var (_, c) in clientProtos) {
            if (s == c) {
                return (s, default!);
            }
            if (s == "h2"u8 && c == "http/1.1"u8) {
                http11fallback = true;
            }
        }
    }
    // As a special case, let http/1.1 clients connect to h2 servers as if they
    // didn't support ALPN. We used not to enforce protocol overlap, so over
    // time a number of HTTP servers were configured with only "h2", but
    // expected to accept connections from "http/1.1" clients. See Issue 46310.
    if (http11fallback) {
        return ("", default!);
    }
    return ("", fmt.Errorf("tls: client requested unsupported application protocols (%q)"u8, clientProtos));
}

// supportsECDHE returns whether ECDHE key exchanges can be used with this
// pre-TLS 1.3 client.
internal static bool supportsECDHE(ж<Config> Ꮡc, uint16 version, slice<CurveID> supportedCurves, slice<uint8> supportedPoints) {
    var supportsCurve = false;
    foreach (var (_, curve) in supportedCurves) {
        if (Ꮡc.supportsCurve(version, curve)) {
            supportsCurve = true;
            break;
        }
    }
    var supportsPointFormat = false;
    foreach (var (_, pointFormat) in supportedPoints) {
        if (pointFormat == pointFormatUncompressed) {
            supportsPointFormat = true;
            break;
        }
    }
    // Per RFC 8422, Section 5.1.2, if the Supported Point Formats extension is
    // missing, uncompressed points are supported. If supportedPoints is empty,
    // the extension must be missing, as an empty extension body is rejected by
    // the parser. See https://go.dev/issue/49126.
    if (len(supportedPoints) == 0) {
        supportsPointFormat = true;
    }
    return supportsCurve && supportsPointFormat;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsNoCipherSuiteˢ = "tls: no cipher suite supported by both client and server"u8;
internal static readonly @string tlsClientUsingˢ = "tls: client using inappropriate protocol fallback"u8;

internal static error pickCipherSuite(this ж<serverHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    var preferenceOrder = cipherSuitesPreferenceOrder;
    if (!hasAESGCMHardwareSupport || !aesgcmPreferred((~hs.clientHello).cipherSuites)) {
        preferenceOrder = cipherSuitesPreferenceOrderNoAES;
    }
    var configCipherSuites = (~c).config.cipherSuites();
    var preferenceList = new slice<uint16>(0, len(configCipherSuites));
    foreach (var (_, suiteID) in preferenceOrder) {
        foreach (var (_, id) in configCipherSuites) {
            if (id == suiteID) {
                preferenceList = append(preferenceList, id);
                break;
            }
        }
    }
    hs.suite = selectCipherSuite(preferenceList, (~hs.clientHello).cipherSuites, Ꮡhs.cipherSuiteOk);
    if (hs.suite == nil) {
        c.sendAlert(alertHandshakeFailure);
        return errors.New(tlsNoCipherSuiteˢ);
    }
    c.Value.cipherSuite = hs.suite.Value.id;
    if ((~(~c).config).CipherSuites == default! && !fips140tls.Required() && rsaKexCiphers[(~hs.suite).id]) {
        tlsrsakex.Value(); // ensure godebug is initialized
        tlsrsakex.IncNonDefault();
    }
    if ((~(~c).config).CipherSuites == default! && !fips140tls.Required() && tdesCiphers[(~hs.suite).id]) {
        tls3des.Value(); // ensure godebug is initialized
        tls3des.IncNonDefault();
    }
    foreach (var (_, id) in (~hs.clientHello).cipherSuites) {
        if (id == TLS_FALLBACK_SCSV) {
            // The client is doing a fallback connection. See RFC 7507.
            if ((~hs.clientHello).vers < (~c).config.maxSupportedVersion(roleServer)) {
                c.sendAlert(alertInappropriateFallback);
                return errors.New(tlsClientUsingˢ);
            }
            break;
        }
    }
    return default!;
}

internal static bool cipherSuiteOk(this ref serverHandshakeState hs, ж<cipherSuite> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if ((nint)(c.flags & (nint)suiteECDHE) != 0){
        if (!hs.ecdheOk) {
            return false;
        }
        if ((nint)(c.flags & (nint)suiteECSign) != 0){
            if (!hs.ecSignOk) {
                return false;
            }
        } else 
        if (!hs.rsaSignOk) {
            return false;
        }
    } else 
    if (!hs.rsaDecryptOk) {
        return false;
    }
    if ((~hs.c).vers < VersionTLS12 && (nint)(c.flags & (nint)suiteTLS12) != 0) {
        return false;
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsSessionSupportedˢ = "tls: session supported extended_master_secret but client does not"u8;

// checkForResumption reports whether we should perform resumption on this connection.
internal static error checkForResumption(this ж<serverHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    if ((~(~c).config).SessionTicketsDisabled) {
        return default!;
    }
    ж<SessionState> sessionState = default!;
    if ((~(~c).config).UnwrapSession != default!){
        var (ss, err) = (~(~c).config).UnwrapSession((~hs.clientHello).sessionTicket, c.connectionStateLocked());
        if (err != default!) {
            return err;
        }
        if (ss == nil) {
            return default!;
        }
        sessionState = ss;
    } else {
        var plaintext = (~c).config.decryptTicket((~hs.clientHello).sessionTicket, (~c).ticketKeys);
        if (plaintext == default!) {
            return default!;
        }
        var (ss, err) = ParseSessionState(plaintext);
        if (err != default!) {
            return default!;
        }
        sessionState = ss;
    }
    // TLS 1.2 tickets don't natively have a lifetime, but we want to avoid
    // re-wrapping the same master secret in different tickets over and over for
    // too long, weakening forward secrecy.
    var createdAt = time_package.Unix((int64)(~sessionState).createdAt, 0);
    if ((~c).config.time().Sub(createdAt) > maxSessionTicketLifetime) {
        return default!;
    }
    // Never resume a session for a different TLS version.
    if ((~c).vers != (~sessionState).version) {
        return default!;
    }
    var cipherSuiteOk = false;
    // Check that the client is still offering the ciphersuite in the session.
    foreach (var (_, id) in (~hs.clientHello).cipherSuites) {
        if (id == (~sessionState).cipherSuite) {
            cipherSuiteOk = true;
            break;
        }
    }
    if (!cipherSuiteOk) {
        return default!;
    }
    // Check that we also support the ciphersuite from the session.
    var suite = selectCipherSuite(new uint16[]{(~sessionState).cipherSuite}.slice(),
        (~c).config.cipherSuites(), Ꮡhs.cipherSuiteOk);
    if (suite == nil) {
        return default!;
    }
    var sessionHasClientCerts = len((~sessionState).peerCertificates) != 0;
    var needClientCerts = requiresClientCert((~(~c).config).ClientAuth);
    if (needClientCerts && !sessionHasClientCerts) {
        return default!;
    }
    if (sessionHasClientCerts && (~(~c).config).ClientAuth == NoClientCert) {
        return default!;
    }
    if (sessionHasClientCerts && (~c).config.time().After((~(~sessionState).peerCertificates[0]).NotAfter)) {
        return default!;
    }
    var opts = new Δx509.VerifyOptions(
        CurrentTime: (~c).config.time(),
        Roots: (~(~c).config).ClientCAs,
        KeyUsages: new Δx509.ExtKeyUsage[]{Δx509.ExtKeyUsageClientAuth}.slice()
    );
    if (sessionHasClientCerts && (~(~c).config).ClientAuth >= VerifyClientCertIfGiven && !anyValidVerifiedChain((~sessionState).verifiedChains, opts)) {
        return default!;
    }
    // RFC 7627, Section 5.3
    if (!(~sessionState).extMasterSecret && (~hs.clientHello).extendedMasterSecret) {
        return default!;
    }
    if ((~sessionState).extMasterSecret && !(~hs.clientHello).extendedMasterSecret) {
        // Aborting is somewhat harsh, but it's a MUST and it would indicate a
        // weird downgrade in client capabilities.
        return errors.New(tlsSessionSupportedˢ);
    }
    c.Value.peerCertificates = sessionState.Value.peerCertificates;
    c.Value.ocspResponse = sessionState.Value.ocspResponse;
    c.Value.scts = sessionState.Value.scts;
    c.Value.verifiedChains = sessionState.Value.verifiedChains;
    c.Value.extMasterSecret = sessionState.Value.extMasterSecret;
    hs.sessionState = sessionState;
    hs.suite = suite;
    c.Value.didResume = true;
    return default!;
}

internal static error doResumeHandshake(this ж<serverHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    hs.hello.Value.cipherSuite = hs.suite.Value.id;
    c.Value.cipherSuite = hs.suite.Value.id;
    // We echo the client's session ID in the ServerHello to let it know
    // that we're doing a resumption.
    hs.hello.Value.sessionId = hs.clientHello.Value.sessionId;
    // We always send a new session ticket, even if it wraps the same master
    // secret and it's potentially encrypted with the same key, to help the
    // client avoid cross-connection tracking from a network observer.
    hs.hello.Value.ticketSupported = true;
    hs.finishedHash = newFinishedHash((~c).vers, ref (hs.suite).DerefOrNull());
    hs.finishedHash.discardHandshakeBuffer();
    {
        var err = transcriptMsg(new clientHelloMsgжhandshakeMessage(hs.clientHello), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (err != default!) {
            return err;
        }
    }
    {
        var (_, err) = hs.c.writeHandshakeRecord(new serverHelloMsgжhandshakeMessage(hs.hello), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (err != default!) {
            return err;
        }
    }
    if ((~(~c).config).VerifyConnection != default!) {
        {
            var err = (~(~c).config).VerifyConnection(c.connectionStateLocked()); if (err != default!) {
                c.sendAlert(alertBadCertificate);
                return err;
            }
        }
    }
    hs.masterSecret = hs.sessionState.Value.secret;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsClientCertificateUsedˢ = "tls: client certificate used with invalid signature algorithm"u8;

internal static error doFullHandshake(this ж<serverHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    if ((~hs.clientHello).ocspStapling && len((~hs.cert).OCSPStaple) > 0) {
        hs.hello.Value.ocspStapling = true;
    }
    hs.hello.Value.ticketSupported = (~hs.clientHello).ticketSupported && !(~(~c).config).SessionTicketsDisabled;
    hs.hello.Value.cipherSuite = hs.suite.Value.id;
    hs.finishedHash = newFinishedHash((~hs.c).vers, ref (hs.suite).DerefOrNull());
    if ((~(~c).config).ClientAuth == NoClientCert) {
        // No need to keep a full record of the handshake if client
        // certificates won't be used.
        hs.finishedHash.discardHandshakeBuffer();
    }
    {
        var errΔ1 = transcriptMsg(new clientHelloMsgжhandshakeMessage(hs.clientHello), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var (_, errΔ2) = hs.c.writeHandshakeRecord(new serverHelloMsgжhandshakeMessage(hs.hello), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    var certMsg = @new<certificateMsg>();
    certMsg.Value.certificates = hs.cert.Value.ΔCertificate;
    {
        var (_, errΔ3) = hs.c.writeHandshakeRecord(new certificateMsgжhandshakeMessage(certMsg), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (errΔ3 != default!) {
            return errΔ3;
        }
    }
    if ((~hs.hello).ocspStapling) {
        var certStatus = @new<certificateStatusMsg>();
        certStatus.Value.response = hs.cert.Value.OCSPStaple;
        {
            var (_, errΔ4) = hs.c.writeHandshakeRecord(new certificateStatusMsgжhandshakeMessage(certStatus), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (errΔ4 != default!) {
                return errΔ4;
            }
        }
    }
    var keyAgreement = (~hs.suite).ka((~c).vers);
    var (skx, err) = keyAgreement.generateServerKeyExchange((~c).config, hs.cert, hs.clientHello, hs.hello);
    if (err != default!) {
        c.sendAlert(alertHandshakeFailure);
        return err;
    }
    if (skx != nil) {
        if (len((~skx).key) >= 3 && (~skx).key[0] == 3) {
            /* named curve */
            c.Value.curveID = ((CurveID)byteorder.BEUint16((~skx).key[1..]));
        }
        {
            var (_, errΔ5) = hs.c.writeHandshakeRecord(new serverKeyExchangeMsgжhandshakeMessage(skx), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (errΔ5 != default!) {
                return errΔ5;
            }
        }
    }
    ж<certificateRequestMsg> certReq = default!;
    if ((~(~c).config).ClientAuth >= RequestClientCert) {
        // Request a client certificate
        certReq = @new<certificateRequestMsg>();
        certReq.Value.certificateTypes = new byte[]{
            (byte)certTypeRSASign,
            (byte)certTypeECDSASign
        }.slice();
        if ((~c).vers >= VersionTLS12) {
            certReq.Value.hasSignatureAlgorithm = true;
            certReq.Value.supportedSignatureAlgorithms = supportedSignatureAlgorithms();
        }
        // An empty list of certificateAuthorities signals to
        // the client that it may send any certificate in response
        // to our request. When we know the CAs we trust, then
        // we can send them down, so that the client can choose
        // an appropriate certificate to give to us.
        if ((~(~c).config).ClientCAs != nil) {
            certReq.Value.certificateAuthorities = (~(~c).config).ClientCAs.Subjects();
        }
        {
            var (_, errΔ6) = hs.c.writeHandshakeRecord(new certificateRequestMsgжhandshakeMessage(certReq), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (errΔ6 != default!) {
                return errΔ6;
            }
        }
    }
    var helloDone = @new<serverHelloDoneMsg>();
    {
        var (_, errΔ7) = hs.c.writeHandshakeRecord(new serverHelloDoneMsgжhandshakeMessage(helloDone), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (errΔ7 != default!) {
            return errΔ7;
        }
    }
    {
        var (_, errΔ8) = c.flush(); if (errΔ8 != default!) {
            return errΔ8;
        }
    }
    cryptoꓸPublicKey pub = default!;                               // public key for client auth, if any
    (var msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash)));
    if (err != default!) {
        return err;
    }
    // If we requested a client certificate, then the client must send a
    // certificate message, even if it's empty.
    if ((~(~c).config).ClientAuth >= RequestClientCert) {
        var (certMsgΔ1, okΔ1) = msg._<ж<certificateMsg>>(ᐧ);
        if (!okΔ1) {
            c.sendAlert(alertUnexpectedMessage);
            return unexpectedMessageError(certMsgΔ1.OrTypedNil(), msg);
        }
        {
            var errΔ9 = c.processCertsFromClient(new Certificate(
                ΔCertificate: (~certMsgΔ1).certificates
            )); if (errΔ9 != default!) {
                return errΔ9;
            }
        }
        if (len((~certMsgΔ1).certificates) != 0) {
            pub = (~c).peerCertificates[0].Value.PublicKey;
        }
        (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash)));
        if (err != default!) {
            return err;
        }
    }
    if ((~(~c).config).VerifyConnection != default!) {
        {
            var errΔ10 = (~(~c).config).VerifyConnection(c.connectionStateLocked()); if (errΔ10 != default!) {
                c.sendAlert(alertBadCertificate);
                return errΔ10;
            }
        }
    }
    // Get client key exchange
    var (ckx, ok) = msg._<ж<clientKeyExchangeMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(ckx.OrTypedNil(), msg);
    }
    (var preMasterSecret, err) = keyAgreement.processClientKeyExchange((~c).config, hs.cert, ckx, (~c).vers);
    if (err != default!) {
        c.sendAlert(alertIllegalParameter);
        return err;
    }
    if ((~hs.hello).extendedMasterSecret){
        c.Value.extMasterSecret = true;
        hs.masterSecret = extMasterFromPreMasterSecret((~c).vers, ref (hs.suite).DerefOrNull(), preMasterSecret,
            hs.finishedHash.Sum());
    } else {
        hs.masterSecret = masterFromPreMasterSecret((~c).vers, ref (hs.suite).DerefOrNull(), preMasterSecret,
            (~hs.clientHello).random, (~hs.hello).random);
    }
    {
        var errΔ11 = (~c).config.writeKeyLog(keyLogLabelTLS12, (~hs.clientHello).random, hs.masterSecret); if (errΔ11 != default!) {
            c.sendAlert(alertInternalError);
            return errΔ11;
        }
    }
    // If we received a client cert in response to our certificate request message,
    // the client will send us a certificateVerifyMsg immediately after the
    // clientKeyExchangeMsg. This message is a digest of all preceding
    // handshake-layer messages that is signed using the private key corresponding
    // to the client's certificate. This allows us to verify that the client is in
    // possession of the private key of the certificate.
    if (len((~c).peerCertificates) > 0) {
        // certificateVerifyMsg is included in the transcript, but not until
        // after we verify the handshake signature, since the state before
        // this message was sent is used.
        (msg, err) = c.readHandshake(default!);
        if (err != default!) {
            return err;
        }
        var (certVerify, okΔ2) = msg._<ж<certificateVerifyMsg>>(ᐧ);
        if (!okΔ2) {
            c.sendAlert(alertUnexpectedMessage);
            return unexpectedMessageError(certVerify.OrTypedNil(), msg);
        }
        uint8 sigType = default!;
        crypto.Hash sigHash = default!;
        if ((~c).vers >= VersionTLS12){
            if (!isSupportedSignatureAlgorithm((~certVerify).signatureAlgorithm, (~certReq).supportedSignatureAlgorithms)) {
                c.sendAlert(alertIllegalParameter);
                return errors.New(tlsClientCertificateUsedˢ);
            }
            (sigType, sigHash, err) = typeAndHashFromSignatureScheme((~certVerify).signatureAlgorithm);
            if (err != default!) {
                return c.sendAlert(alertInternalError);
            }
        } else {
            (sigType, sigHash, err) = legacyTypeAndHashFromPublicKey(pub);
            if (err != default!) {
                c.sendAlert(alertIllegalParameter);
                return err;
            }
        }
        var signed = hs.finishedHash.hashForClientCertificate(sigType, sigHash);
        {
            var errΔ12 = verifyHandshakeSignature(sigType, pub, sigHash, signed, (~certVerify).signature); if (errΔ12 != default!) {
                c.sendAlert(alertDecryptError);
                return errors.New("tls: invalid signature by the client certificate: "u8 + errΔ12.Error());
            }
        }
        {
            var errΔ13 = transcriptMsg(new certificateVerifyMsgжhandshakeMessage(certVerify), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (errΔ13 != default!) {
                return errΔ13;
            }
        }
    }
    hs.finishedHash.discardHandshakeBuffer();
    return default!;
}

internal static error establishKeys(this ref serverHandshakeState hs) {
    var c = hs.c;
    var (clientMAC, serverMAC, clientKey, serverKey, clientIV, serverIV) = keysFromMasterSecret((~c).vers, ref (hs.suite).DerefOrNull(), hs.masterSecret, (~hs.clientHello).random, (~hs.hello).random, (~hs.suite).macLen, (~hs.suite).keyLen, (~hs.suite).ivLen);
    any clientCipher = default!;
    any serverCipher = default!;
    hash.Hash clientHash = default!;
    hash.Hash serverHash = default!;
    if ((~hs.suite).aead == default!){
        clientCipher = (~hs.suite).cipher(clientKey, clientIV, true);
        /* for reading */
        clientHash = (~hs.suite).mac(clientMAC);
        serverCipher = (~hs.suite).cipher(serverKey, serverIV, false);
        /* not for reading */
        serverHash = (~hs.suite).mac(serverMAC);
    } else {
        clientCipher = (~hs.suite).aead(clientKey, clientIV);
        serverCipher = (~hs.suite).aead(serverKey, serverIV);
    }
    c.of(Conn.Ꮡin).prepareCipherSpec((~c).vers, clientCipher, clientHash);
    c.of(Conn.Ꮡout).prepareCipherSpec((~c).vers, serverCipher, serverHash);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsClientSFinishedˢ = "tls: client's Finished message is incorrect"u8;

internal static error readFinished(this ж<serverHandshakeState> Ꮡhs, slice<byte> @out) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    {
        var errΔ1 = c.readChangeCipherSpec(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    // finishedMsg is included in the transcript, but not until after we
    // check the client version, since the state before this message was
    // sent is used during verification.
    var (msg, err) = c.readHandshake(default!);
    if (err != default!) {
        return err;
    }
    var (clientFinished, ok) = msg._<ж<finishedMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(clientFinished.OrTypedNil(), msg);
    }
    var verify = hs.finishedHash.clientSum(hs.masterSecret);
    if (len(verify) != len((~clientFinished).verifyData) || subtle.ConstantTimeCompare(verify, (~clientFinished).verifyData) != 1) {
        c.sendAlert(alertHandshakeFailure);
        return errors.New(tlsClientSFinishedˢ);
    }
    {
        var errΔ2 = transcriptMsg(new finishedMsgжhandshakeMessage(clientFinished), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    copy(@out, verify);
    return default!;
}

internal static error sendSessionTicket(this ж<serverHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    if (!(~hs.hello).ticketSupported) {
        return default!;
    }
    var c = hs.c;
    var m = @new<newSessionTicketMsg>();
    var state = c.sessionState();
    state.Value.secret = hs.masterSecret;
    if (hs.sessionState != nil) {
        // If this is re-wrapping an old key, then keep
        // the original time it was created.
        state.Value.createdAt = hs.sessionState.Value.createdAt;
    }
    if ((~(~c).config).WrapSession != default!){
        error err = default!;
        (m.Value.ticket, err) = (~(~c).config).WrapSession(c.connectionStateLocked(), state);
        if (err != default!) {
            return err;
        }
    } else {
        var (stateBytes, err) = state.Bytes();
        if (err != default!) {
            return err;
        }
        (m.Value.ticket, err) = (~c).config.encryptTicket(stateBytes, (~c).ticketKeys);
        if (err != default!) {
            return err;
        }
    }
    {
        var (_, err) = hs.c.writeHandshakeRecord(new newSessionTicketMsgжhandshakeMessage(m), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (err != default!) {
            return err;
        }
    }
    return default!;
}

internal static error sendFinished(this ж<serverHandshakeState> Ꮡhs, slice<byte> @out) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    {
        var err = c.writeChangeCipherRecord(); if (err != default!) {
            return err;
        }
    }
    var finished = @new<finishedMsg>();
    finished.Value.verifyData = hs.finishedHash.serverSum(hs.masterSecret);
    {
        var (_, err) = hs.c.writeHandshakeRecord(new finishedMsgжhandshakeMessage(finished), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(serverHandshakeState.ᏑfinishedHash))); if (err != default!) {
            return err;
        }
    }
    copy(@out, (~finished).verifyData);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsClientDidnTProvideAˢ = "tls: client didn't provide a certificate"u8;

// processCertsFromClient takes a chain of client certificates either from a
// Certificates message and verifies them.
internal static error processCertsFromClient(this ж<Conn> Ꮡc, Certificate certificate) {
    ref var c = ref Ꮡc.DerefOrNull();

    var certificates = certificate.ΔCertificate;
    var certs = new slice<ж<Δx509.Certificate>>(len(certificates));
    error err = default!;
    foreach (var (i, asn1Data) in certificates) {
        {
            (certs[i], err) = Δx509.ParseCertificate(asn1Data); if (err != default!) {
                Ꮡc.sendAlert(alertBadCertificate);
                return errors.New("tls: failed to parse client certificate: "u8 + err.Error());
            }
        }
        if ((~certs[i]).PublicKeyAlgorithm == Δx509.RSA) {
            nint n = (~(~certs[i]).PublicKey._<ж<rsa.PublicKey>>()).N.BitLen();
            {
                var (max, ok) = checkKeySize(n); if (!ok) {
                    Ꮡc.sendAlert(alertBadCertificate);
                    return fmt.Errorf("tls: client sent certificate containing RSA key larger than %d bits"u8, max);
                }
            }
        }
    }
    if (len(certs) == 0 && requiresClientCert((~c.config).ClientAuth)) {
        if (c.vers == VersionTLS13){
            Ꮡc.sendAlert(alertCertificateRequired);
        } else {
            Ꮡc.sendAlert(alertBadCertificate);
        }
        return errors.New(tlsClientDidnTProvideAˢ);
    }
    if ((~c.config).ClientAuth >= VerifyClientCertIfGiven && len(certs) > 0) {
        var opts = new Δx509.VerifyOptions(
            Roots: (~c.config).ClientCAs,
            CurrentTime: c.config.time(),
            Intermediates: Δx509.NewCertPool(),
            KeyUsages: new Δx509.ExtKeyUsage[]{Δx509.ExtKeyUsageClientAuth}.slice()
        );
        foreach (var (_, cert) in certs[1..]) {
            opts.Intermediates.AddCert(cert);
        }
        var (chains, errΔ1) = certs[0].Verify(opts);
        if (errΔ1 != default!) {
            ref var errCertificateInvalid = ref heap(new Δx509.CertificateInvalidError(), out var ᏑerrCertificateInvalid);
            if (errors.As(errΔ1, Ꮡ(new Δx509.UnknownAuthorityError(nil)))){
                Ꮡc.sendAlert(alertUnknownCA);
            } else 
            if (errors.As(errΔ1, ᏑerrCertificateInvalid) && errCertificateInvalid.Reason == Δx509.Expired){
                Ꮡc.sendAlert(alertCertificateExpired);
            } else {
                Ꮡc.sendAlert(alertBadCertificate);
            }
            return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: errΔ1)));
        }
        (c.verifiedChains, errΔ1) = fipsAllowedChains(chains);
        if (errΔ1 != default!) {
            Ꮡc.sendAlert(alertBadCertificate);
            return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: errΔ1)));
        }
    }
    c.peerCertificates = certs;
    c.ocspResponse = certificate.OCSPStaple;
    c.scts = certificate.SignedCertificateTimestamps;
    if (len(certs) > 0) {
        switch ((~certs[0]).PublicKey.type()) {
        case ж<ecdsa.PublicKey> _:
        case ж<rsa.PublicKey> _:
        case ed25519.PublicKey _: {
            break;
        }
        default: {
            Ꮡc.sendAlert(alertUnsupportedCertificate);
            return fmt.Errorf("tls: client certificate contains an unsupported public key of type %T"u8, (~certs[0]).PublicKey);
        }}

    }
    if ((~c.config).VerifyPeerCertificate != default!) {
        {
            var errΔ2 = (~c.config).VerifyPeerCertificate(certificates, c.verifiedChains); if (errΔ2 != default!) {
                Ꮡc.sendAlert(alertBadCertificate);
                return errΔ2;
            }
        }
    }
    return default!;
}

internal static ж<ClientHelloInfo> clientHelloInfo(context.Context ctx, ref Conn c, ref clientHelloMsg clientHello) {
    var ΔsupportedVersions = clientHello.supportedVersions;
    if (len(clientHello.supportedVersions) == 0) {
        ΔsupportedVersions = supportedVersionsFromMax(clientHello.vers);
    }
    return Ꮡ(new ClientHelloInfo(
        CipherSuites: clientHello.cipherSuites,
        ServerName: clientHello.serverName,
        SupportedCurves: clientHello.supportedCurves,
        SupportedPoints: clientHello.supportedPoints,
        SignatureSchemes: clientHello.supportedSignatureAlgorithms,
        SupportedProtos: clientHello.alpnProtocols,
        SupportedVersions: ΔsupportedVersions,
        Extensions: clientHello.extensions,
        Conn: c.conn,
        config: c.config,
        ctx: ctx
    ));
}

} // end tls_package
