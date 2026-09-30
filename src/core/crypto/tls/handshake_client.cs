// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using context = context_package;
using crypto = crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using mlkem = go.crypto.@internal.fips140.mlkem_package;
using tls13 = go.crypto.@internal.fips140.tls13_package;
using hpke = go.crypto.@internal.hpke_package;
using rsa = go.crypto.rsa_package;
using subtle = go.crypto.subtle_package;
using fips140tls = go.crypto.tls.@internal.fips140tls_package;
using Δx509 = go.crypto.x509_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using byteorder = go.@internal.byteorder_package;
using godebug = go.@internal.godebug_package;
using io = io_package;
using net = net_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using ecdh = go.crypto.ecdh_package;
using fips140 = go.crypto.@internal.fips140_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.tls.@internal;
using go.sync;
using math;

partial class tls_package {

[GoType] partial struct clientHandshakeState {
    internal ж<Conn> c;
    internal context.Context ctx;
    internal ж<serverHelloMsg> serverHello;
    internal ж<clientHelloMsg> hello;
    internal ж<cipherSuite> suite;
    internal ΔfinishedHash finishedHash;
    internal slice<byte> masterSecret;
    internal ж<SessionState> session; // the session being resumed
    internal slice<byte> ticket;   // a fresh ticket received during this handshake
}

internal static slice<SignatureScheme> testingOnlyForceClientHelloSignatureAlgorithms;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsEitherServerNameOrˢ = "tls: either ServerName or InsecureSkipVerify must be specified in the tls.Config"u8;
internal static readonly @string tlsInvalidNextProtosˢ = "tls: invalid NextProtos value"u8;
internal static readonly @string tlsNextProtosValuesTooˢ = "tls: NextProtos values too large"u8;
internal static readonly @string tlsNoSupportedVersionsˢ = "tls: no supported versions satisfy MinVersion and MaxVersion"u8;
internal static readonly @string tlsNoSupportedEllipticˢ = "tls: no supported elliptic curves for ECDHE"u8;
internal static readonly @string tlsCurvePreferencesˢ = "tls: CurvePreferences includes unsupported curve"u8;
internal static readonly @string tlsMinVersionMustBeˢ = "tls: MinVersion must be >= VersionTLS13 if EncryptedClientHelloConfigList is populated"u8;
internal static readonly @string tlsMaxVersionMustBeˢ = "tls: MaxVersion must be >= VersionTLS13 if EncryptedClientHelloConfigList is populated"u8;
internal static readonly @string tlsˢ = "tls: EncryptedClientHelloConfigList contains no valid configs"u8;

internal static (ж<clientHelloMsg>, ж<keySharePrivateKeys>, ж<echClientContext>, error) makeClientHello(this ж<Conn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var config = c.config;
    if (len((~config).ServerName) == 0 && !(~config).InsecureSkipVerify) {
        return (default!, default!, default!, errors.New(tlsEitherServerNameOrˢ));
    }
    nint nextProtosLength = 0;
    foreach (var (_, proto) in (~config).NextProtos) {
        {
            nint l = len(proto); if (l == 0 || l > 255){
                return (default!, default!, default!, errors.New(tlsInvalidNextProtosˢ));
            } else {
                nextProtosLength += 1 + l;
            }
        }
    }
    if (nextProtosLength > 0xffff) {
        return (default!, default!, default!, errors.New(tlsNextProtosValuesTooˢ));
    }
    var ΔsupportedVersions = config.supportedVersions(roleClient);
    if (len(ΔsupportedVersions) == 0) {
        return (default!, default!, default!, errors.New(tlsNoSupportedVersionsˢ));
    }
    ref var maxVersion = ref heap<uint16>(out var ᏑmaxVersion);
    maxVersion = config.maxSupportedVersion(roleClient);
    var hello = Ꮡ(new clientHelloMsg(
        vers: maxVersion,
        compressionMethods: new uint8[]{compressionNone}.slice(),
        random: new slice<byte>(32),
        extendedMasterSecret: true,
        ocspStapling: true,
        scts: true,
        serverName: hostnameInSNI((~config).ServerName),
        supportedCurves: config.curvePreferences(maxVersion),
        supportedPoints: new uint8[]{pointFormatUncompressed}.slice(),
        secureRenegotiationSupported: true,
        alpnProtocols: (~config).NextProtos,
        supportedVersions: ΔsupportedVersions
    ));
    // The version at the beginning of the ClientHello was capped at TLS 1.2
    // for compatibility reasons. The supported_versions extension is used
    // to negotiate versions now. See RFC 8446, Section 4.2.1.
    if ((~hello).vers > VersionTLS12) {
        hello.Value.vers = VersionTLS12;
    }
    if (c.handshakes > 0) {
        hello.Value.secureRenegotiation = c.clientFinished[..];
    }
    var preferenceOrder = cipherSuitesPreferenceOrder;
    if (!hasAESGCMHardwareSupport) {
        preferenceOrder = cipherSuitesPreferenceOrderNoAES;
    }
    var configCipherSuites = config.cipherSuites();
    hello.Value.cipherSuites = new slice<uint16>(0, len(configCipherSuites));
    foreach (var (_, suiteId) in preferenceOrder) {
        var suite = mutualCipherSuite(configCipherSuites, suiteId);
        if (suite == nil) {
            continue;
        }
        // Don't advertise TLS 1.2-only cipher suites unless
        // we're attempting TLS 1.2.
        if (maxVersion < VersionTLS12 && (nint)((~suite).flags & (nint)suiteTLS12) != 0) {
            continue;
        }
        hello.Value.cipherSuites = append((~hello).cipherSuites, suiteId);
    }
    var (_, err) = io.ReadFull(config.rand(), (~hello).random);
    if (err != default!) {
        return (default!, default!, default!, errors.New("tls: short read from Rand: "u8 + err.Error()));
    }
    // A random session ID is used to detect when the server accepted a ticket
    // and is resuming a session (see RFC 5077). In TLS 1.3, it's always set as
    // a compatibility measure (see RFC 8446, Section 4.1.2).
    //
    // The session ID is not set for QUIC connections (see RFC 9001, Section 8.4).
    if (c.quic == nil) {
        hello.Value.sessionId = new slice<byte>(32);
        {
            var (_, errΔ1) = io.ReadFull(config.rand(), (~hello).sessionId); if (errΔ1 != default!) {
                return (default!, default!, default!, errors.New("tls: short read from Rand: "u8 + errΔ1.Error()));
            }
        }
    }
    if (maxVersion >= VersionTLS12) {
        hello.Value.supportedSignatureAlgorithms = supportedSignatureAlgorithms();
    }
    if (testingOnlyForceClientHelloSignatureAlgorithms != default!) {
        hello.Value.supportedSignatureAlgorithms = testingOnlyForceClientHelloSignatureAlgorithms;
    }
    ж<keySharePrivateKeys> keyShareKeys = default!;
    if ((~hello).supportedVersions[0] == VersionTLS13) {
        // Reset the list of ciphers when the client only supports TLS 1.3.
        if (len((~hello).supportedVersions) == 1) {
            hello.Value.cipherSuites = default!;
        }
        if (fips140tls.Required()){
            hello.Value.cipherSuites = appendꓸꓸꓸ((~hello).cipherSuites, defaultCipherSuitesTLS13FIPS);
        } else 
        if (hasAESGCMHardwareSupport){
            hello.Value.cipherSuites = appendꓸꓸꓸ((~hello).cipherSuites, defaultCipherSuitesTLS13);
        } else {
            hello.Value.cipherSuites = appendꓸꓸꓸ((~hello).cipherSuites, defaultCipherSuitesTLS13NoAES);
        }
        if (len((~hello).supportedCurves) == 0) {
            return (default!, default!, default!, errors.New(tlsNoSupportedEllipticˢ));
        }
        ref var curveID = ref heap<CurveID>(out var ᏑcurveID);
        curveID = (~hello).supportedCurves[0];
        keyShareKeys = Ꮡ(new keySharePrivateKeys(curveID: curveID));
        // Note that if X25519MLKEM768 is supported, it will be first because
        // the preference order is fixed.
        if (curveID == X25519MLKEM768){
            (keyShareKeys.Value.ecdhe, err) = generateECDHEKey(config.rand(), X25519);
            if (err != default!) {
                return (default!, default!, default!, err);
            }
            var seed = new slice<byte>(mlkem.SeedSize);
            {
                var (_, errΔ2) = io.ReadFull(config.rand(), seed); if (errΔ2 != default!) {
                    return (default!, default!, default!, errΔ2);
                }
            }
            (keyShareKeys.Value.mlkem, err) = mlkem.NewDecapsulationKey768(seed);
            if (err != default!) {
                return (default!, default!, default!, err);
            }
            var mlkemEncapsulationKey = (~keyShareKeys).mlkem.EncapsulationKey().Bytes();
            var x25519EphemeralKey = (~keyShareKeys).ecdhe.PublicKey().Bytes();
            hello.Value.keyShares = new keyShare[]{
                new(group: X25519MLKEM768, data: appendꓸꓸꓸ(mlkemEncapsulationKey, x25519EphemeralKey))
            }.slice();
            // If both X25519MLKEM768 and X25519 are supported, we send both key
            // shares (as a fallback) and we reuse the same X25519 ephemeral
            // key, as allowed by draft-ietf-tls-hybrid-design-09, Section 3.2.
            if (slices.Contains((~hello).supportedCurves, X25519)) {
                hello.Value.keyShares = append((~hello).keyShares, new keyShare(group: X25519, data: x25519EphemeralKey));
            }
        } else {
            {
                var (_, ok) = curveForCurveID(curveID); if (!ok) {
                    return (default!, default!, default!, errors.New(tlsCurvePreferencesˢ));
                }
            }
            (keyShareKeys.Value.ecdhe, err) = generateECDHEKey(config.rand(), curveID);
            if (err != default!) {
                return (default!, default!, default!, err);
            }
            hello.Value.keyShares = new keyShare[]{new(group: curveID, data: (~keyShareKeys).ecdhe.PublicKey().Bytes())}.slice();
        }
    }
    if (c.quic != nil) {
        var (p, errΔ3) = Ꮡc.quicGetTransportParameters();
        if (errΔ3 != default!) {
            return (default!, default!, default!, errΔ3);
        }
        if (p == default!) {
            p = new byte[]{}.slice();
        }
        hello.Value.quicTransportParameters = p;
    }
    ж<echClientContext> ech = default!;
    if ((~c.config).EncryptedClientHelloConfigList != default!) {
        if ((~c.config).MinVersion != 0 && (~c.config).MinVersion < VersionTLS13) {
            return (default!, default!, default!, errors.New(tlsMinVersionMustBeˢ));
        }
        if ((~c.config).MaxVersion != 0 && (~c.config).MaxVersion <= VersionTLS12) {
            return (default!, default!, default!, errors.New(tlsMaxVersionMustBeˢ));
        }
        var (echConfigs, errΔ4) = parseECHConfigList((~c.config).EncryptedClientHelloConfigList);
        if (errΔ4 != default!) {
            return (default!, default!, default!, errΔ4);
        }
        var echConfig = pickECHConfig(echConfigs);
        if (echConfig == nil) {
            return (default!, default!, default!, errors.New(tlsˢ));
        }
        ech = Ꮡ(new echClientContext(config: echConfig));
        hello.Value.encryptedClientHello = new byte[]{1}.slice(); // indicate inner hello
        // We need to explicitly set these 1.2 fields to nil, as we do not
        // marshal them when encoding the inner hello, otherwise transcripts
        // will later mismatch.
        hello.Value.supportedPoints = default!;
        hello.Value.ticketSupported = false;
        hello.Value.secureRenegotiationSupported = false;
        hello.Value.extendedMasterSecret = false;
        (var echPK, errΔ4) = hpke.ParseHPKEPublicKey((~(~ech).config).KemID, (~(~ech).config).PublicKey);
        if (errΔ4 != default!) {
            return (default!, default!, default!, errΔ4);
        }
        (var suite, errΔ4) = pickECHCipherSuite((~(~ech).config).SymmetricCipherSuite);
        if (errΔ4 != default!) {
            return (default!, default!, default!, errΔ4);
        }
        (ech.Value.kdfID, ech.Value.aeadID) = (suite.KDFID, suite.AEADID);
        var info = appendꓸꓸꓸ(slice<byte>("tls ech\x00"u8), (~(~ech).config).raw);
        (ech.Value.encapsulatedKey, ech.Value.hpkeContext, errΔ4) = hpke.SetupSender((~(~ech).config).KemID, suite.KDFID, suite.AEADID, echPK, info);
        if (errΔ4 != default!) {
            return (default!, default!, default!, errΔ4);
        }
    }
    return (hello, keyShareKeys, ech, default!);
}

[GoType] partial struct echClientContext {
    internal ж<echConfig> config;
    internal ж<hpke.Sender> hpkeContext;
    internal slice<byte> encapsulatedKey;
    internal ж<clientHelloMsg> innerHello;
    internal hash.Hash innerTranscript;
    internal uint16 kdfID;
    internal uint16 aeadID;
    internal bool echRejected;
    internal slice<byte> retryConfigs;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsDowngradeAttemptˢ = "tls: downgrade attempt detected, possibly due to a MitM attack or a broken middlebox"u8;

internal static error /*err*/ clientHandshake(this ж<Conn> Ꮡc, context.Context ctx) {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;
        if (c.config == nil) {
            c.config = defaultConfig();
        }
        // This may be a renegotiation handshake, in which case some fields
        // need to be reset.
        c.didResume = false;
        (var hello, var keyShareKeys, var ech, err) = Ꮡc.makeClientHello();
        if (err != default!) {
            goto ᒐdone;
        }
        (var session, var earlySecret, var binderKey, err) = Ꮡc.loadSession(hello);
        if (err != default!) {
            goto ᒐdone;
        }
        if (session != nil) {
            defer(() => {
                // If we got a handshake failure when resuming a session, throw away
                // the session ticket. See RFC 5077, Section 3.2.
                //
                // RFC 8446 makes no mention of dropping tickets on failure, but it
                // does require servers to abort on invalid binders, so we need to
                // delete tickets to recover from a corrupted PSK.
                if (Ꮡerr.ValueSlot != default!) {
                    {
                        @string cacheKey = Ꮡc.Value.clientSessionCacheKey(); if (cacheKey != ""u8) {
                            (~Ꮡc.Value.config).ClientSessionCache.Put(cacheKey, nil);
                        }
                    }
                }
            }, ref ᒐ);
        }
        if (ech != nil) {
            // Split hello into inner and outer
            ech.Value.innerHello = hello.clone();
            // Overwrite the server name in the outer hello with the public facing
            // name.
            hello.Value.serverName = ((@string)(~(~ech).config).PublicName);
            // Generate a new random for the outer hello.
            hello.Value.random = new slice<byte>(32);
            (_, err) = io.ReadFull(c.config.rand(), (~hello).random);
            if (err != default!) {
                err = errors.New("tls: short read from Rand: "u8 + err.Error()); goto ᒐdone;
            }
            // NOTE: we don't do PSK GREASE, in line with boringssl, it's meant to
            // work around _possibly_ broken middleboxes, but there is little-to-no
            // evidence that this is actually a problem.
            {
                var errΔ1 = computeAndUpdateOuterECHExtension(hello, (~ech).innerHello, ref (ech).DerefOrNull(), true); if (errΔ1 != default!) {
                    err = errΔ1; goto ᒐdone;
                }
            }
        }
        c.serverName = hello.Value.serverName;
        {
            var (_, errΔ2) = Ꮡc.writeHandshakeRecord(new clientHelloMsgжhandshakeMessage(hello), default!); if (errΔ2 != default!) {
                err = errΔ2; goto ᒐdone;
            }
        }
        if ((~hello).earlyData) {
            var suite = cipherSuiteTLS13ByID((~session).cipherSuite);
            var transcript = (~suite).hash.New();
            var transcriptHello = hello;
            if (ech != nil) {
                transcriptHello = ech.Value.innerHello;
            }
            {
                var errΔ3 = transcriptMsg(new clientHelloMsgжhandshakeMessage(transcriptHello), new hash_HashᴠtranscriptHash(transcript)); if (errΔ3 != default!) {
                    err = errΔ3; goto ᒐdone;
                }
            }
            var earlyTrafficSecret = earlySecret.ClientEarlyTrafficSecret(new hash_HashᴠHash(transcript));
            c.quicSetWriteSecret(QUICEncryptionLevelEarly, (~suite).id, earlyTrafficSecret);
        }
        // serverHelloMsg is not included in the transcript
        (var msg, err) = Ꮡc.readHandshake(default!);
        if (err != default!) {
            goto ᒐdone;
        }
        var (serverHello, ok) = msg._<ж<serverHelloMsg>>(ᐧ);
        if (!ok) {
            Ꮡc.sendAlert(alertUnexpectedMessage);
            err = unexpectedMessageError(serverHello.OrTypedNil(), msg); goto ᒐdone;
        }
        {
            var errΔ4 = Ꮡc.pickTLSVersion(serverHello); if (errΔ4 != default!) {
                err = errΔ4; goto ᒐdone;
            }
        }
        // If we are negotiating a protocol version that's lower than what we
        // support, check for the server downgrade canaries.
        // See RFC 8446, Section 4.1.3.
        var maxVers = c.config.maxSupportedVersion(roleClient);
        var tls12Downgrade = ((sstring)((~serverHello).random[24..])) == downgradeCanaryTLS12;
        var tls11Downgrade = ((sstring)((~serverHello).random[24..])) == downgradeCanaryTLS11;
        if (maxVers == VersionTLS13 && c.vers <= VersionTLS12 && (tls12Downgrade || tls11Downgrade) || maxVers == VersionTLS12 && c.vers <= VersionTLS11 && tls11Downgrade) {
            Ꮡc.sendAlert(alertIllegalParameter);
            err = errors.New(tlsDowngradeAttemptˢ); goto ᒐdone;
        }
        if (c.vers == VersionTLS13) {
            var hsΔ1 = Ꮡ(new clientHandshakeStateTLS13(
                c: Ꮡc,
                ctx: ctx,
                serverHello: serverHello,
                hello: hello,
                keyShareKeys: keyShareKeys,
                session: session,
                earlySecret: earlySecret,
                binderKey: binderKey,
                echContext: ech
            ));
            err = hsΔ1.handshake(); goto ᒐdone;
        }
        var hs = Ꮡ(new clientHandshakeState(
            c: Ꮡc,
            ctx: ctx,
            serverHello: serverHello,
            hello: hello,
            session: session
        ));
        err = hs.handshake();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return Ꮡerr.ValueSlot;
}

internal static (ж<SessionState> session, ж<tls13.EarlySecret> earlySecret, slice<byte> binderKey, error err) loadSession(this ж<Conn> Ꮡc, ж<clientHelloMsg> Ꮡhello) {
    ж<SessionState> session = default!;
    ж<tls13.EarlySecret> earlySecret = default!;
    slice<byte> binderKey = default!;
    error err = default!;

    ref var c = ref Ꮡc.DerefOrNull();
    ref var hello = ref Ꮡhello.DerefOrNull();
    if ((~c.config).SessionTicketsDisabled || (~c.config).ClientSessionCache == default!) {
        return (default!, default!, default!, default!);
    }
    var echInner = bytes.Equal(hello.encryptedClientHello, new byte[]{1}.slice());
    // ticketSupported is a TLS 1.2 extension (as TLS 1.3 replaced tickets with PSK
    // identities) and ECH requires and forces TLS 1.3.
    hello.ticketSupported = true && !echInner;
    if (hello.supportedVersions[0] == VersionTLS13) {
        // Require DHE on resumption as it guarantees forward secrecy against
        // compromise of the session ticket key. See RFC 8446, Section 4.2.9.
        hello.pskModes = new uint8[]{pskModeDHE}.slice();
    }
    // Session resumption is not allowed if renegotiating because
    // renegotiation is primarily used to allow a client to send a client
    // certificate, which would be skipped if session resumption occurred.
    if (c.handshakes != 0) {
        return (default!, default!, default!, default!);
    }
    // Try to resume a previously negotiated TLS session, if available.
    @string cacheKey = c.clientSessionCacheKey();
    if (cacheKey == ""u8) {
        return (default!, default!, default!, default!);
    }
    var (cs, ok) = (~c.config).ClientSessionCache.Get(cacheKey);
    if (!ok || cs == nil) {
        return (default!, default!, default!, default!);
    }
    session = cs.Value.session;
    // Check that version used for the previous session is still valid.
    var versOk = false;
    foreach (var (_, v) in hello.supportedVersions) {
        if (v == (~session).version) {
            versOk = true;
            break;
        }
    }
    if (!versOk) {
        return (default!, default!, default!, default!);
    }
    if (c.config.time().After((~(~session).peerCertificates[0]).NotAfter)) {
        // Expired certificate, delete the entry.
        (~c.config).ClientSessionCache.Put(cacheKey, nil);
        return (default!, default!, default!, default!);
    }
    if (!(~c.config).InsecureSkipVerify) {
        if (len((~session).verifiedChains) == 0) {
            // The original connection had InsecureSkipVerify, while this doesn't.
            return (default!, default!, default!, default!);
        }
        {
            var errΔ1 = (~session).peerCertificates[0].VerifyHostname((~c.config).ServerName); if (errΔ1 != default!) {
                // This should be ensured by the cache key, but protect the
                // application from a faulty ClientSessionCache implementation.
                return (default!, default!, default!, default!);
            }
        }
        var opts = new Δx509.VerifyOptions(
            CurrentTime: c.config.time(),
            Roots: (~c.config).RootCAs,
            KeyUsages: new Δx509.ExtKeyUsage[]{Δx509.ExtKeyUsageServerAuth}.slice()
        );
        if (!anyValidVerifiedChain((~session).verifiedChains, opts)) {
            // No valid chains, delete the entry.
            (~c.config).ClientSessionCache.Put(cacheKey, nil);
            return (default!, default!, default!, default!);
        }
    }
    if ((~session).version != VersionTLS13) {
        // In TLS 1.2 the cipher suite must match the resumed session. Ensure we
        // are still offering it.
        if (mutualCipherSuite(hello.cipherSuites, (~session).cipherSuite) == nil) {
            return (default!, default!, default!, default!);
        }
        hello.sessionTicket = session.Value.ticket;
        return (session, earlySecret, binderKey, err);
    }
    // Check that the session ticket is not expired.
    if (c.config.time().After(time_package.Unix((int64)(~session).useBy, 0))) {
        (~c.config).ClientSessionCache.Put(cacheKey, nil);
        return (default!, default!, default!, default!);
    }
    // In TLS 1.3 the KDF hash must match the resumed session. Ensure we
    // offer at least one cipher suite with that hash.
    var cipherSuite = cipherSuiteTLS13ByID((~session).cipherSuite);
    if (cipherSuite == nil) {
        return (default!, default!, default!, default!);
    }
    var cipherSuiteOk = false;
    foreach (var (_, offeredID) in hello.cipherSuites) {
        var offeredSuite = cipherSuiteTLS13ByID(offeredID);
        if (offeredSuite != nil && (~offeredSuite).hash == (~cipherSuite).hash) {
            cipherSuiteOk = true;
            break;
        }
    }
    if (!cipherSuiteOk) {
        return (default!, default!, default!, default!);
    }
    if (c.quic != nil) {
        if ((~c.quic).enableSessionEvents) {
            Ꮡc.quicResumeSession(session);
        }
        // For 0-RTT, the cipher suite has to match exactly, and we need to be
        // offering the same ALPN.
        if ((~session).EarlyData && mutualCipherSuiteTLS13(hello.cipherSuites, (~session).cipherSuite) != nil) {
            foreach (var (_, alpn) in hello.alpnProtocols) {
                if (alpn == (~session).alpnProtocol) {
                    hello.earlyData = true;
                    break;
                }
            }
        }
    }
    // Set the pre_shared_key extension. See RFC 8446, Section 4.2.11.1.
    var ticketAge = c.config.time().Sub(time_package.Unix((int64)(~session).createdAt, 0));
    var identity = new pskIdentity(
        label: (~session).ticket,
        obfuscatedTicketAge: (uint32)(int64)(ticketAge / time_package.Millisecond) + (~session).ageAdd
    );
    hello.pskIdentities = new pskIdentity[]{identity}.slice();
    hello.pskBinders = new slice<byte>[]{new slice<byte>((~cipherSuite).hash.Size())}.slice();
    // Compute the PSK binders. See RFC 8446, Section 4.2.11.2.
    earlySecret = tls13.NewEarlySecret<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => (~cipherSuite).hash.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), (~session).secret);
    binderKey = earlySecret.ResumptionBinderKey();
    var transcript = (~cipherSuite).hash.New();
    {
        var errΔ2 = computeAndUpdatePSK(Ꮡhello, binderKey, transcript, cipherSuite.finishedHash); if (errΔ2 != default!) {
            return (default!, default!, default!, errΔ2);
        }
    }
    return (session, earlySecret, binderKey, err);
}

internal static error pickTLSVersion(this ж<Conn> Ꮡc, ж<serverHelloMsg> ᏑserverHello) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var serverHello = ref ᏑserverHello.DerefOrNull();

    var peerVersion = serverHello.vers;
    if (serverHello.supportedVersion != 0) {
        peerVersion = serverHello.supportedVersion;
    }
    var (vers, ok) = c.config.mutualVersion(roleClient, new uint16[]{peerVersion}.slice());
    if (!ok) {
        Ꮡc.sendAlert(alertProtocolVersion);
        return fmt.Errorf("tls: server selected unsupported protocol version %x"u8, peerVersion);
    }
    c.vers = vers;
    c.haveVers = true;
    c.@in.version = vers;
    c.@out.version = vers;
    return default!;
}

// Does the handshake, either a full one or resumes old session. Requires hs.c,
// hs.hello, hs.serverHello, and, optionally, hs.session to be set.
internal static error handshake(this ж<clientHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    var (isResume, err) = hs.processServerHello();
    if (err != default!) {
        return err;
    }
    hs.finishedHash = newFinishedHash((~c).vers, ref (hs.suite).DerefOrNull());
    // No signatures of the handshake are needed in a resumption.
    // Otherwise, in a full handshake, if we don't have any certificates
    // configured then we will never send a CertificateVerify message and
    // thus no signatures are needed in that case either.
    if (isResume || (len((~(~c).config).Certificates) == 0 && (~(~c).config).GetClientCertificate == default!)) {
        hs.finishedHash.discardHandshakeBuffer();
    }
    {
        var errΔ1 = transcriptMsg(new clientHelloMsgжhandshakeMessage(hs.hello), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var errΔ2 = transcriptMsg(new serverHelloMsgжhandshakeMessage(hs.serverHello), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    c.Value.buffering = true;
    c.Value.didResume = isResume;
    if (isResume){
        {
            var errΔ3 = hs.establishKeys(); if (errΔ3 != default!) {
                return errΔ3;
            }
        }
        {
            var errΔ4 = Ꮡhs.readSessionTicket(); if (errΔ4 != default!) {
                return errΔ4;
            }
        }
        {
            var errΔ5 = Ꮡhs.readFinished((~c).serverFinished[..]); if (errΔ5 != default!) {
                return errΔ5;
            }
        }
        c.Value.clientFinishedIsFirst = false;
        // Make sure the connection is still being verified whether or not this
        // is a resumption. Resumptions currently don't reverify certificates so
        // they don't call verifyServerCertificate. See Issue 31641.
        if ((~(~c).config).VerifyConnection != default!) {
            {
                var errΔ6 = (~(~c).config).VerifyConnection(c.connectionStateLocked()); if (errΔ6 != default!) {
                    c.sendAlert(alertBadCertificate);
                    return errΔ6;
                }
            }
        }
        {
            var errΔ7 = Ꮡhs.sendFinished((~c).clientFinished[..]); if (errΔ7 != default!) {
                return errΔ7;
            }
        }
        {
            var (_, errΔ8) = c.flush(); if (errΔ8 != default!) {
                return errΔ8;
            }
        }
    } else {
        {
            var errΔ9 = Ꮡhs.doFullHandshake(); if (errΔ9 != default!) {
                return errΔ9;
            }
        }
        {
            var errΔ10 = hs.establishKeys(); if (errΔ10 != default!) {
                return errΔ10;
            }
        }
        {
            var errΔ11 = Ꮡhs.sendFinished((~c).clientFinished[..]); if (errΔ11 != default!) {
                return errΔ11;
            }
        }
        {
            var (_, errΔ12) = c.flush(); if (errΔ12 != default!) {
                return errΔ12;
            }
        }
        c.Value.clientFinishedIsFirst = true;
        {
            var errΔ13 = Ꮡhs.readSessionTicket(); if (errΔ13 != default!) {
                return errΔ13;
            }
        }
        {
            var errΔ14 = Ꮡhs.readFinished((~c).serverFinished[..]); if (errΔ14 != default!) {
                return errΔ14;
            }
        }
    }
    {
        var errΔ15 = hs.saveSessionTicket(); if (errΔ15 != default!) {
            return errΔ15;
        }
    }
    c.Value.ekm = ekmFromMasterSecret((~c).vers, hs.suite, hs.masterSecret, (~hs.hello).random, (~hs.serverHello).random);
    c.of(Conn.ᏑisHandshakeComplete).Store(true);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerChoseAnˢ = "tls: server chose an unconfigured cipher suite"u8;

[GoRecv] internal static error pickCipherSuite(this ref clientHandshakeState hs) {
    {
        hs.suite = mutualCipherSuite((~hs.hello).cipherSuites, (~hs.serverHello).cipherSuite); if (hs.suite == nil) {
            hs.c.sendAlert(alertHandshakeFailure);
            return errors.New(tlsServerChoseAnˢ);
        }
    }
    if ((~(~hs.c).config).CipherSuites == default! && !fips140tls.Required() && rsaKexCiphers[(~hs.suite).id]) {
        tlsrsakex.Value(); // ensure godebug is initialized
        tlsrsakex.IncNonDefault();
    }
    if ((~(~hs.c).config).CipherSuites == default! && !fips140tls.Required() && tdesCiphers[(~hs.suite).id]) {
        tls3des.Value(); // ensure godebug is initialized
        tls3des.IncNonDefault();
    }
    hs.c.Value.cipherSuite = hs.suite.Value.id;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsReceivedUnexpectedˢ = "tls: received unexpected CertificateStatus message"u8;
internal static readonly @string tlsServerSIdentityˢ = "tls: server's identity changed during renegotiation"u8;

internal static error doFullHandshake(this ж<clientHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    var (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
    if (err != default!) {
        return err;
    }
    var (certMsg, ok) = msg._<ж<certificateMsg>>(ᐧ);
    if (!ok || len((~certMsg).certificates) == 0) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(certMsg.OrTypedNil(), msg);
    }
    (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
    if (err != default!) {
        return err;
    }
    (var cs, ok) = msg._<ж<certificateStatusMsg>>(ᐧ);
    if (ok) {
        // RFC4366 on Certificate Status Request:
        // The server MAY return a "certificate_status" message.
        if (!(~hs.serverHello).ocspStapling) {
            // If a server returns a "CertificateStatus" message, then the
            // server MUST have included an extension of type "status_request"
            // with empty "extension_data" in the extended server hello.
            c.sendAlert(alertUnexpectedMessage);
            return errors.New(tlsReceivedUnexpectedˢ);
        }
        c.Value.ocspResponse = cs.Value.response;
        (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
        if (err != default!) {
            return err;
        }
    }
    if ((~c).handshakes == 0){
        // If this is the first handshake on a connection, process and
        // (optionally) verify the server's certificates.
        {
            var errΔ1 = c.verifyServerCertificate((~certMsg).certificates); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
    } else {
        // This is a renegotiation handshake. We require that the
        // server's identity (i.e. leaf certificate) is unchanged and
        // thus any previous trust decision is still valid.
        //
        // See https://mitls.org/pages/attacks/3SHAKE for the
        // motivation behind this requirement.
        if (!bytes.Equal((~(~c).peerCertificates[0]).Raw, (~certMsg).certificates[0])) {
            c.sendAlert(alertBadCertificate);
            return errors.New(tlsServerSIdentityˢ);
        }
    }
    var keyAgreement = (~hs.suite).ka((~c).vers);
    (var skx, ok) = msg._<ж<serverKeyExchangeMsg>>(ᐧ);
    if (ok) {
        err = keyAgreement.processServerKeyExchange((~c).config, hs.hello, hs.serverHello, (~c).peerCertificates[0], skx);
        if (err != default!) {
            c.sendAlert(alertIllegalParameter);
            return err;
        }
        if (len((~skx).key) >= 3 && (~skx).key[0] == 3) {
            /* named curve */
            c.Value.curveID = ((CurveID)byteorder.BEUint16((~skx).key[1..]));
        }
        (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
        if (err != default!) {
            return err;
        }
    }
    ж<Certificate> chainToSend = default!;
    bool certRequested = default!;
    (var certReq, ok) = msg._<ж<certificateRequestMsg>>(ᐧ);
    if (ok) {
        certRequested = true;
        var cri = certificateRequestInfoFromMsg(hs.ctx, (~c).vers, ref (certReq).DerefOrNull());
        {
            (chainToSend, err) = c.getClientCertificate(cri); if (err != default!) {
                c.sendAlert(alertInternalError);
                return err;
            }
        }
        (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
        if (err != default!) {
            return err;
        }
    }
    (var shd, ok) = msg._<ж<serverHelloDoneMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(shd.OrTypedNil(), msg);
    }
    // If the server requested a certificate then we have to send a
    // Certificate message, even if it's empty because we don't have a
    // certificate to send.
    if (certRequested) {
        certMsg = @new<certificateMsg>();
        certMsg.Value.certificates = chainToSend.Value.ΔCertificate;
        {
            var (_, errΔ2) = hs.c.writeHandshakeRecord(new certificateMsgжhandshakeMessage(certMsg), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    (var preMasterSecret, var ckx, err) = keyAgreement.generateClientKeyExchange((~c).config, hs.hello, (~c).peerCertificates[0]);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    if (ckx != nil) {
        {
            var (_, errΔ3) = hs.c.writeHandshakeRecord(new clientKeyExchangeMsgжhandshakeMessage(ckx), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ3 != default!) {
                return errΔ3;
            }
        }
    }
    if ((~hs.serverHello).extendedMasterSecret){
        c.Value.extMasterSecret = true;
        hs.masterSecret = extMasterFromPreMasterSecret((~c).vers, ref (hs.suite).DerefOrNull(), preMasterSecret,
            hs.finishedHash.Sum());
    } else {
        hs.masterSecret = masterFromPreMasterSecret((~c).vers, ref (hs.suite).DerefOrNull(), preMasterSecret,
            (~hs.hello).random, (~hs.serverHello).random);
    }
    {
        var errΔ4 = (~c).config.writeKeyLog(keyLogLabelTLS12, (~hs.hello).random, hs.masterSecret); if (errΔ4 != default!) {
            c.sendAlert(alertInternalError);
            return errors.New("tls: failed to write to key log: "u8 + errΔ4.Error());
        }
    }
    if (chainToSend != nil && len((~chainToSend).ΔCertificate) > 0) {
        var certVerify = Ꮡ(new certificateVerifyMsg(nil));
        var (key, okΔ1) = (~chainToSend).PrivateKey._<crypto.Signer>(ᐧ);
        if (!okΔ1) {
            c.sendAlert(alertInternalError);
            return fmt.Errorf("tls: client certificate private key of type %T does not implement crypto.Signer"u8, (~chainToSend).PrivateKey);
        }
        uint8 sigType = default!;
        ref var sigHash = ref heap(new crypto.Hash(), out var ᏑsigHash);
        if ((~c).vers >= VersionTLS12){
            var (signatureAlgorithm, errΔ5) = selectSignatureScheme((~c).vers, ref (chainToSend).DerefOrNull(), (~certReq).supportedSignatureAlgorithms);
            if (errΔ5 != default!) {
                c.sendAlert(alertIllegalParameter);
                return errΔ5;
            }
            (sigType, sigHash, errΔ5) = typeAndHashFromSignatureScheme(signatureAlgorithm);
            if (errΔ5 != default!) {
                return c.sendAlert(alertInternalError);
            }
            certVerify.Value.hasSignatureAlgorithm = true;
            certVerify.Value.signatureAlgorithm = signatureAlgorithm;
        } else {
            (sigType, sigHash, err) = legacyTypeAndHashFromPublicKey(key.Public());
            if (err != default!) {
                c.sendAlert(alertIllegalParameter);
                return err;
            }
        }
        var signed = hs.finishedHash.hashForClientCertificate(sigType, sigHash);
        var signOpts = ((crypto.SignerOpts)sigHash);
        if (sigType == signatureRSAPSS) {
            signOpts = new rsa_PSSOptionsжSignerOpts(Ꮡ(new rsa.PSSOptions(SaltLength: rsa.PSSSaltLengthEqualsHash, Hash: sigHash)));
        }
        (certVerify.Value.signature, err) = key.Sign((~c).config.rand(), signed, signOpts);
        if (err != default!) {
            c.sendAlert(alertInternalError);
            return err;
        }
        {
            var (_, errΔ6) = hs.c.writeHandshakeRecord(new certificateVerifyMsgжhandshakeMessage(certVerify), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ6 != default!) {
                return errΔ6;
            }
        }
    }
    hs.finishedHash.discardHandshakeBuffer();
    return default!;
}

[GoRecv] internal static error establishKeys(this ref clientHandshakeState hs) {
    var c = hs.c;
    var (clientMAC, serverMAC, clientKey, serverKey, clientIV, serverIV) = keysFromMasterSecret((~c).vers, ref (hs.suite).DerefOrNull(), hs.masterSecret, (~hs.hello).random, (~hs.serverHello).random, (~hs.suite).macLen, (~hs.suite).keyLen, (~hs.suite).ivLen);
    any clientCipher = default!;
    any serverCipher = default!;
    hash.Hash clientHash = default!;
    hash.Hash serverHash = default!;
    if ((~hs.suite).cipher != default!){
        clientCipher = (~hs.suite).cipher(clientKey, clientIV, false);
        /* not for reading */
        clientHash = (~hs.suite).mac(clientMAC);
        serverCipher = (~hs.suite).cipher(serverKey, serverIV, true);
        /* for reading */
        serverHash = (~hs.suite).mac(serverMAC);
    } else {
        clientCipher = (~hs.suite).aead(clientKey, clientIV);
        serverCipher = (~hs.suite).aead(serverKey, serverIV);
    }
    c.of(Conn.Ꮡin).prepareCipherSpec((~c).vers, serverCipher, serverHash);
    c.of(Conn.Ꮡout).prepareCipherSpec((~c).vers, clientCipher, clientHash);
    return default!;
}

[GoRecv] internal static bool serverResumedSession(this ref clientHandshakeState hs) {
    // If the server responded with the same sessionId then it means the
    // sessionTicket is being used to resume a TLS session.
    return hs.session != nil && (~hs.hello).sessionId != default! && bytes.Equal((~hs.serverHello).sessionId, (~hs.hello).sessionId);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerSelectedˢ = "tls: server selected unsupported compression format"u8;
internal static readonly @string tlsInitialHandshakeHadˢ = "tls: initial handshake had non-empty renegotiation extension"u8;
internal static readonly @string tlsIncorrectˢ = "tls: incorrect renegotiation extension contents"u8;
internal static readonly @string tlsServerResumedASessionˢ = "tls: server resumed a session with a different version"u8;
internal static readonly @string tlsServerResumedASessionˢ2 = "tls: server resumed a session with a different cipher suite"u8;
internal static readonly @string tlsServerResumedASessionˢ3 = "tls: server resumed a session with a different EMS extension"u8;

[GoRecv] internal static (bool, error) processServerHello(this ref clientHandshakeState hs) {
    var c = hs.c;
    {
        var err = hs.pickCipherSuite(); if (err != default!) {
            return (false, err);
        }
    }
    if ((~hs.serverHello).compressionMethod != compressionNone) {
        c.sendAlert(alertUnexpectedMessage);
        return (false, errors.New(tlsServerSelectedˢ));
    }
    if ((~c).handshakes == 0 && (~hs.serverHello).secureRenegotiationSupported) {
        c.Value.secureRenegotiation = true;
        if (len((~hs.serverHello).secureRenegotiation) != 0) {
            c.sendAlert(alertHandshakeFailure);
            return (false, errors.New(tlsInitialHandshakeHadˢ));
        }
    }
    if ((~c).handshakes > 0 && (~c).secureRenegotiation) {
        array<byte> expectedSecureRenegotiation = new(24);
        copy(expectedSecureRenegotiation[..], (~c).clientFinished[..]);
        copy(expectedSecureRenegotiation[12..], (~c).serverFinished[..]);
        if (!bytes.Equal((~hs.serverHello).secureRenegotiation, expectedSecureRenegotiation[..])) {
            c.sendAlert(alertHandshakeFailure);
            return (false, errors.New(tlsIncorrectˢ));
        }
    }
    {
        var err = checkALPN((~hs.hello).alpnProtocols, (~hs.serverHello).alpnProtocol, false); if (err != default!) {
            c.sendAlert(alertUnsupportedExtension);
            return (false, err);
        }
    }
    c.Value.clientProtocol = hs.serverHello.Value.alpnProtocol;
    c.Value.scts = hs.serverHello.Value.scts;
    if (!hs.serverResumedSession()) {
        return (false, default!);
    }
    if ((~hs.session).version != (~c).vers) {
        c.sendAlert(alertHandshakeFailure);
        return (false, errors.New(tlsServerResumedASessionˢ));
    }
    if ((~hs.session).cipherSuite != (~hs.suite).id) {
        c.sendAlert(alertHandshakeFailure);
        return (false, errors.New(tlsServerResumedASessionˢ2));
    }
    // RFC 7627, Section 5.3
    if ((~hs.session).extMasterSecret != (~hs.serverHello).extendedMasterSecret) {
        c.sendAlert(alertHandshakeFailure);
        return (false, errors.New(tlsServerResumedASessionˢ3));
    }
    // Restore master secret and certificates from previous state
    hs.masterSecret = hs.session.Value.secret;
    c.Value.extMasterSecret = hs.session.Value.extMasterSecret;
    c.Value.peerCertificates = hs.session.Value.peerCertificates;
    c.Value.activeCertHandles = hs.c.Value.activeCertHandles;
    c.Value.verifiedChains = hs.session.Value.verifiedChains;
    c.Value.ocspResponse = hs.session.Value.ocspResponse;
    // Let the ServerHello SCTs override the session SCTs from the original
    // connection, if any are provided
    if (len((~c).scts) == 0 && len((~hs.session).scts) != 0) {
        c.Value.scts = hs.session.Value.scts;
    }
    return (true, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerDidNotSelectAnˢ = "tls: server did not select an ALPN protocol"u8;
internal static readonly @string tlsServerAdvertisedˢ = "tls: server advertised unrequested ALPN extension"u8;
internal static readonly @string tlsServerSelectedˢ2 = "tls: server selected unadvertised ALPN protocol"u8;

// checkALPN ensure that the server's choice of ALPN protocol is compatible with
// the protocols that we advertised in the ClientHello.
internal static error checkALPN(slice<@string> clientProtos, @string serverProto, bool quic) {
    if (serverProto == ""u8) {
        if (quic && len(clientProtos) > 0) {
            // RFC 9001, Section 8.1
            return errors.New(tlsServerDidNotSelectAnˢ);
        }
        return default!;
    }
    if (len(clientProtos) == 0) {
        return errors.New(tlsServerAdvertisedˢ);
    }
    foreach (var (_, proto) in clientProtos) {
        if (proto == serverProto) {
            return default!;
        }
    }
    return errors.New(tlsServerSelectedˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerSFinishedˢ = "tls: server's Finished message was incorrect"u8;

internal static error readFinished(this ж<clientHandshakeState> Ꮡhs, slice<byte> @out) {
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
    var (serverFinished, ok) = msg._<ж<finishedMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(serverFinished.OrTypedNil(), msg);
    }
    var verify = hs.finishedHash.serverSum(hs.masterSecret);
    if (len(verify) != len((~serverFinished).verifyData) || subtle.ConstantTimeCompare(verify, (~serverFinished).verifyData) != 1) {
        c.sendAlert(alertHandshakeFailure);
        return errors.New(tlsServerSFinishedˢ);
    }
    {
        var errΔ2 = transcriptMsg(new finishedMsgжhandshakeMessage(serverFinished), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    copy(@out, verify);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerSentUnrequestedˢ = "tls: server sent unrequested session ticket"u8;

internal static error readSessionTicket(this ж<clientHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    if (!(~hs.serverHello).ticketSupported) {
        return default!;
    }
    var c = hs.c;
    if (!(~hs.hello).ticketSupported) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerSentUnrequestedˢ);
    }
    var (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
    if (err != default!) {
        return err;
    }
    var (sessionTicketMsg, ok) = msg._<ж<newSessionTicketMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(sessionTicketMsg.OrTypedNil(), msg);
    }
    hs.ticket = sessionTicketMsg.Value.ticket;
    return default!;
}

[GoRecv] internal static error saveSessionTicket(this ref clientHandshakeState hs) {
    if (hs.ticket == default!) {
        return default!;
    }
    var c = hs.c;
    @string cacheKey = c.clientSessionCacheKey();
    if (cacheKey == ""u8) {
        return default!;
    }
    var session = c.sessionState();
    session.Value.secret = hs.masterSecret;
    session.Value.ticket = hs.ticket;
    var cs = Ꮡ(new ClientSessionState(session: session));
    (~(~c).config).ClientSessionCache.Put(cacheKey, cs);
    return default!;
}

internal static error sendFinished(this ж<clientHandshakeState> Ꮡhs, slice<byte> @out) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    {
        var err = c.writeChangeCipherRecord(); if (err != default!) {
            return err;
        }
    }
    var finished = @new<finishedMsg>();
    finished.Value.verifyData = hs.finishedHash.clientSum(hs.masterSecret);
    {
        var (_, err) = hs.c.writeHandshakeRecord(new finishedMsgжhandshakeMessage(finished), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (err != default!) {
            return err;
        }
    }
    copy(@out, (~finished).verifyData);
    return default!;
}

// defaultMaxRSAKeySize is the maximum RSA key size in bits that we are willing
// to verify the signatures of during a TLS handshake.
internal static UntypedInt defaultMaxRSAKeySize => 8192;

internal static ж<godebug.Setting> tlsmaxrsasize = godebug.New("tlsmaxrsasize"u8);

internal static (nint max, bool ok) checkKeySize(nint n) {
    {
        @string v = tlsmaxrsasize.Value(); if (v != ""u8) {
            {
                var (maxΔ1, err) = strconv.Atoi(v); if (err == default!) {
                    if ((n <= maxΔ1) != (n <= defaultMaxRSAKeySize)) {
                        tlsmaxrsasize.IncNonDefault();
                    }
                    return (maxΔ1, n <= maxΔ1);
                }
            }
        }
    }
    return (defaultMaxRSAKeySize, n <= defaultMaxRSAKeySize);
}

// verifyServerCertificate parses and verifies the provided chain, setting
// c.verifiedChains and c.peerCertificates or sending the appropriate alert.
internal static error verifyServerCertificate(this ж<Conn> Ꮡc, slice<slice<byte>> certificates) {
    ref var c = ref Ꮡc.DerefOrNull();

    var activeHandles = new slice<ж<activeCert>>(len(certificates));
    var certs = new slice<ж<Δx509.Certificate>>(len(certificates));
    foreach (var (i, asn1Data) in certificates) {
        var (cert, err) = globalCertCache.newCert(asn1Data);
        if (err != default!) {
            Ꮡc.sendAlert(alertBadCertificate);
            return errors.New("tls: failed to parse certificate from server: "u8 + err.Error());
        }
        if ((~(~cert).cert).PublicKeyAlgorithm == Δx509.RSA) {
            nint n = (~(~(~cert).cert).PublicKey._<ж<rsa.PublicKey>>()).N.BitLen();
            {
                var (max, ok) = checkKeySize(n); if (!ok) {
                    Ꮡc.sendAlert(alertBadCertificate);
                    return fmt.Errorf("tls: server sent certificate containing RSA key larger than %d bits"u8, max);
                }
            }
        }
        activeHandles[i] = cert;
        certs[i] = cert.Value.cert;
    }
    var echRejected = (~c.config).EncryptedClientHelloConfigList != default! && !c.echAccepted;
    if (echRejected){
        if ((~c.config).EncryptedClientHelloRejectionVerify != default!){
            {
                var err = (~c.config).EncryptedClientHelloRejectionVerify(Ꮡc.connectionStateLocked()); if (err != default!) {
                    Ꮡc.sendAlert(alertBadCertificate);
                    return err;
                }
            }
        } else {
            var opts = new Δx509.VerifyOptions(
                Roots: (~c.config).RootCAs,
                CurrentTime: c.config.time(),
                DNSName: c.serverName,
                Intermediates: Δx509.NewCertPool()
            );
            foreach (var (_, cert) in certs[1..]) {
                opts.Intermediates.AddCert(cert);
            }
            var (chains, err) = certs[0].Verify(opts);
            if (err != default!) {
                Ꮡc.sendAlert(alertBadCertificate);
                return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: err)));
            }
            (c.verifiedChains, err) = fipsAllowedChains(chains);
            if (err != default!) {
                Ꮡc.sendAlert(alertBadCertificate);
                return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: err)));
            }
        }
    } else 
    if (!(~c.config).InsecureSkipVerify) {
        var opts = new Δx509.VerifyOptions(
            Roots: (~c.config).RootCAs,
            CurrentTime: c.config.time(),
            DNSName: (~c.config).ServerName,
            Intermediates: Δx509.NewCertPool()
        );
        foreach (var (_, cert) in certs[1..]) {
            opts.Intermediates.AddCert(cert);
        }
        var (chains, err) = certs[0].Verify(opts);
        if (err != default!) {
            Ꮡc.sendAlert(alertBadCertificate);
            return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: err)));
        }
        (c.verifiedChains, err) = fipsAllowedChains(chains);
        if (err != default!) {
            Ꮡc.sendAlert(alertBadCertificate);
            return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: err)));
        }
    }
    switch ((~certs[0]).PublicKey.type()) {
    case ж<rsa.PublicKey> _:
    case ж<ecdsa.PublicKey> _:
    case ed25519.PublicKey _: {
        break;
        break;
    }
    default: {
        Ꮡc.sendAlert(alertUnsupportedCertificate);
        return fmt.Errorf("tls: server's certificate contains an unsupported type of public key: %T"u8, (~certs[0]).PublicKey);
    }}

    c.activeCertHandles = activeHandles;
    c.peerCertificates = certs;
    if ((~c.config).VerifyPeerCertificate != default! && !echRejected) {
        {
            var err = (~c.config).VerifyPeerCertificate(certificates, c.verifiedChains); if (err != default!) {
                Ꮡc.sendAlert(alertBadCertificate);
                return err;
            }
        }
    }
    if ((~c.config).VerifyConnection != default! && !echRejected) {
        {
            var err = (~c.config).VerifyConnection(Ꮡc.connectionStateLocked()); if (err != default!) {
                Ꮡc.sendAlert(alertBadCertificate);
                return err;
            }
        }
    }
    return default!;
}

// certificateRequestInfoFromMsg generates a CertificateRequestInfo from a TLS
// <= 1.2 CertificateRequest, making an effort to fill in missing information.
internal static ж<CertificateRequestInfo> certificateRequestInfoFromMsg(context.Context ctx, uint16 vers, ref certificateRequestMsg certReq) {
    var cri = Ꮡ(new CertificateRequestInfo(
        AcceptableCAs: certReq.certificateAuthorities,
        Version: vers,
        ctx: ctx
    ));
    bool rsaAvail = default!;
    bool ecAvail = default!;
    foreach (var (_, certType) in certReq.certificateTypes) {
        var exprᴛ1 = certType;
        if (exprᴛ1 == certTypeRSASign) {
            rsaAvail = true;
        }
        else if (exprᴛ1 == certTypeECDSASign) {
            ecAvail = true;
        }

    }
    if (!certReq.hasSignatureAlgorithm) {
        // Prior to TLS 1.2, signature schemes did not exist. In this case we
        // make up a list based on the acceptable certificate types, to help
        // GetClientCertificate and SupportsCertificate select the right certificate.
        // The hash part of the SignatureScheme is a lie here, because
        // TLS 1.0 and 1.1 always use MD5+SHA1 for RSA and SHA1 for ECDSA.
        switch (ᐧ) {
        case {} when rsaAvail && ecAvail: {
            cri.Value.SignatureSchemes = new SignatureScheme[]{
                ECDSAWithP256AndSHA256, ECDSAWithP384AndSHA384, ECDSAWithP521AndSHA512,
                PKCS1WithSHA256, PKCS1WithSHA384, PKCS1WithSHA512, PKCS1WithSHA1
            }.slice();
            break;
        }
        case {} when rsaAvail: {
            cri.Value.SignatureSchemes = new SignatureScheme[]{
                PKCS1WithSHA256, PKCS1WithSHA384, PKCS1WithSHA512, PKCS1WithSHA1
            }.slice();
            break;
        }
        case {} when ecAvail: {
            cri.Value.SignatureSchemes = new SignatureScheme[]{
                ECDSAWithP256AndSHA256, ECDSAWithP384AndSHA384, ECDSAWithP521AndSHA512
            }.slice();
            break;
        }}

        return cri;
    }
    // Filter the signature schemes based on the certificate types.
    // See RFC 5246, Section 7.4.4 (where it calls this "somewhat complicated").
    cri.Value.SignatureSchemes = new slice<SignatureScheme>(0, len(certReq.supportedSignatureAlgorithms));
    foreach (var (_, sigScheme) in certReq.supportedSignatureAlgorithms) {
        var (sigType, _, err) = typeAndHashFromSignatureScheme(sigScheme);
        if (err != default!) {
            continue;
        }
        switch (sigType) {
        case signatureECDSA or signatureEd25519: {
            if (ecAvail) {
                cri.Value.SignatureSchemes = append((~cri).SignatureSchemes, sigScheme);
            }
            break;
        }
        case signatureRSAPSS or signaturePKCS1v15: {
            if (rsaAvail) {
                cri.Value.SignatureSchemes = append((~cri).SignatureSchemes, sigScheme);
            }
            break;
        }}

    }
    return cri;
}

[GoRecv] internal static (ж<Certificate>, error) getClientCertificate(this ref Conn c, ж<CertificateRequestInfo> Ꮡcri) {
    ref var cri = ref Ꮡcri.DerefOrNull();

    if ((~c.config).GetClientCertificate != default!) {
        return (~c.config).GetClientCertificate(Ꮡcri);
    }
    foreach (var (_, vᴛ1) in (~c.config).Certificates) {
        ref var chain = ref heap(new Certificate(), out var Ꮡchain);
        chain = vᴛ1;

        {
            var err = cri.SupportsCertificate(Ꮡchain); if (err != default!) {
                continue;
            }
        }
        return (Ꮡchain, default!);
    }
    // No acceptable certificate found. Don't send a certificate.
    return (@new<Certificate>(), default!);
}

// clientSessionCacheKey returns a key used to cache sessionTickets that could
// be used to resume previously negotiated TLS sessions with a server.
[GoRecv] internal static @string clientSessionCacheKey(this ref Conn c) {
    if (len((~c.config).ServerName) > 0) {
        return (~c.config).ServerName;
    }
    if (c.conn != default!) {
        return c.conn.RemoteAddr().String();
    }
    return ""u8;
}

// hostnameInSNI converts name into an appropriate hostname for SNI.
// Literal IP addresses and absolute FQDNs are not permitted as SNI values.
// See RFC 6066, Section 3.
internal static @string hostnameInSNI(@string name) {
    @string host = name;
    if (len(host) > 0 && host[0] == (rune)'[' && host[len(host) - 1] == (rune)']') {
        host = host.slice(1, len(host) - 1);
    }
    {
        nint i = strings.LastIndex(host, "%"u8); if (i > 0) {
            host = host.slice(0, i);
        }
    }
    if (net.ParseIP(host) != default!) {
        return ""u8;
    }
    while (len(name) > 0 && name[len(name) - 1] == (rune)'.') {
        name = name.slice(0, len(name) - 1);
    }
    return name;
}

internal static error computeAndUpdatePSK(ж<clientHelloMsg> Ꮡm, slice<byte> binderKey, hash.Hash transcript, Func<slice<byte>, hash.Hash, slice<byte>> ΔfinishedHash) {
    ref var m = ref Ꮡm.DerefOrNull();

    var (helloBytes, err) = Ꮡm.marshalWithoutBinders();
    if (err != default!) {
        return err;
    }
    transcript.Write(helloBytes);
    var pskBinders = new slice<byte>[]{ΔfinishedHash(binderKey, transcript)}.slice();
    return m.updateBinders(pskBinders);
}

} // end tls_package
