// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using context = context_package;
using crypto = crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using hpke = go.crypto.hpke_package;
using tls13 = go.crypto.@internal.fips140.tls13_package;
using rsa = go.crypto.rsa_package;
using subtle = go.crypto.subtle_package;
using fips140tls = go.crypto.tls.@internal.fips140tls_package;
using Δx509 = go.crypto.x509_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using godebug = go.@internal.godebug_package;
using io = io_package;
using net = net_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal.fips140;
using go.crypto.tls.@internal;
using go.sync;
using math;

partial class tls_package {

partial struct clientHandshakeState {
    internal ж<Conn> c;
    internal context.Context ctx;
    internal ж<serverHelloMsg> serverHello;
    internal ж<clientHelloMsg> hello;
    internal ж<cipherSuite> suite;
    internal ΔfinishedHash finishedHash;
    internal slice<byte> masterSecret;
    internal ж<SessionState> session; // the session being resumed
    internal slice<byte> ticket;      // a fresh ticket received during this handshake
}

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
    if (len(config.ServerName) == 0 && !config.InsecureSkipVerify) {
        return (default, default, default, errors.New(tlsEitherServerNameOrˢ));
    }

    nint nextProtosLength = 0;
    foreach (var (_, proto) in config.NextProtos) {
        {
            nint l = len(proto); if (l == 0 || l > 255) {
                return (default, default, default, errors.New(tlsInvalidNextProtosˢ));
            } else {
                nextProtosLength += 1 + l;
            }
        }
    }
    if (nextProtosLength > 0xffff) {
        return (default, default, default, errors.New(tlsNextProtosValuesTooˢ));
    }

    var ΔsupportedVersions = config.supportedVersions(roleClient);
    if (len(ΔsupportedVersions) == 0) {
        return (default, default, default, errors.New(tlsNoSupportedVersionsˢ));
    }
    // Since supportedVersions is sorted in descending order, the first element
    // is the maximum version and the last element is the minimum version.
    ref var maxVersion = ref heap<uint16>(ΔsupportedVersions[0], out var ᏑmaxVersion);
    var minVersion = ΔsupportedVersions[len(ΔsupportedVersions) - 1];

    var hello = Ꮡ(new clientHelloMsg(
        vers: maxVersion,
        compressionMethods: slice<uint8>([compressionNone]),
        random: new slice<byte>(32),
        extendedMasterSecret: true,
        ocspStapling: true,
        scts: true,
        serverName: hostnameInSNI(config.ServerName),
        supportedCurves: config.curvePreferences(maxVersion),
        supportedPoints: slice<uint8>([pointFormatUncompressed]),
        secureRenegotiationSupported: true,
        alpnProtocols: config.NextProtos,
        supportedVersions: ΔsupportedVersions
    ));

    // The version at the beginning of the ClientHello was capped at TLS 1.2
    // for compatibility reasons. The supported_versions extension is used
    // to negotiate versions now. See RFC 8446, Section 4.2.1.
    if (hello.vers > VersionTLS12) {
        hello.vers = VersionTLS12;
    }

    if (c.handshakes > 0) {
        hello.secureRenegotiation = c.clientFinished[..];
    }

    hello.cipherSuites = config.cipherSuites(hasAESGCMHardwareSupport);
    // Don't advertise TLS 1.2-only cipher suites unless we're attempting TLS 1.2.
    if (maxVersion < VersionTLS12) {
        hello.cipherSuites = slices.DeleteFunc(hello.cipherSuites, (uint16 id) => (cipherSuiteByID(id).flags & (nint)suiteTLS12) != 0);
    }

    var (_, err) = io.ReadFull(config.rand(), hello.random);
    if (err != nil) {
        return (default, default, default, errors.New("tls: short read from Rand: "u8 + err.Error()));
    }

    // A random session ID is used to detect when the server accepted a ticket
    // and is resuming a session (see RFC 5077). In TLS 1.3, it's always set as
    // a compatibility measure (see RFC 8446, Section 4.1.2).
    //
    // The session ID is not set for QUIC connections (see RFC 9001, Section 8.4).
    if (c.quic == nil) {
        hello.sessionId = new slice<byte>(32);
        {
            var (_, errΔ1) = io.ReadFull(config.rand(), hello.sessionId); if (errΔ1 != nil) {
                return (default, default, default, errors.New("tls: short read from Rand: "u8 + errΔ1.Error()));
            }
        }
    }

    if (maxVersion >= VersionTLS12) {
        hello.supportedSignatureAlgorithms = supportedSignatureAlgorithms(minVersion);
        hello.supportedSignatureAlgorithmsCert = supportedSignatureAlgorithmsCert();
    }

    ж<keySharePrivateKeys> keyShareKeys = default;
    if (maxVersion >= VersionTLS13) {
        // Reset the list of ciphers when the client only supports TLS 1.3.
        if (minVersion >= VersionTLS13) {
            hello.cipherSuites = default;
        }

        if (fips140tls.Required()) {
            hello.cipherSuites = appendꓸꓸꓸ(hello.cipherSuites, allowedCipherSuitesTLS13FIPS);
        } else if (hasAESGCMHardwareSupport) {
            hello.cipherSuites = appendꓸꓸꓸ(hello.cipherSuites, defaultCipherSuitesTLS13);
        } else {
            hello.cipherSuites = appendꓸꓸꓸ(hello.cipherSuites, defaultCipherSuitesTLS13NoAES);
        }

        if (len(hello.supportedCurves) == 0) {
            return (default, default, default, errors.New(tlsNoSupportedEllipticˢ));
        }
        // Since the order is fixed, the first one is always the one to send a
        // key share for. All the PQ hybrids sort first, and produce a fallback
        // ECDH share.
        var curveID = hello.supportedCurves[0];
        var (ke, errΔ2) = keyExchangeForCurveID(curveID);
        if (errΔ2 != nil) {
            return (default, default, default, errors.New(tlsCurvePreferencesˢ));
        }
        (keyShareKeys, hello.keyShares, errΔ2) = ke.keyShares(config.rand());
        if (errΔ2 != nil) {
            return (default, default, default, errΔ2);
        }
        // Only send the fallback ECDH share if the corresponding CurveID is enabled.
        if (len(hello.keyShares) == 2 && !slices.Contains(hello.supportedCurves, hello.keyShares[1].group)) {
            hello.keyShares = hello.keyShares[..1];
        }
    }

    if (c.quic != nil) {
        var (p, errΔ3) = Ꮡc.quicGetTransportParameters();
        if (errΔ3 != nil) {
            return (default, default, default, errΔ3);
        }
        if (p == nil) {
            p = slice<byte>([]);
        }
        hello.quicTransportParameters = p;
    }

    ж<echClientContext> ech = default;
    if (c.config.EncryptedClientHelloConfigList != nil) {
        if (c.config.MinVersion != 0 && c.config.MinVersion < VersionTLS13) {
            return (default, default, default, errors.New(tlsMinVersionMustBeˢ));
        }
        if (c.config.MaxVersion != 0 && c.config.MaxVersion <= VersionTLS12) {
            return (default, default, default, errors.New(tlsMaxVersionMustBeˢ));
        }
        var (echConfigs, errΔ4) = parseECHConfigList(c.config.EncryptedClientHelloConfigList);
        if (errΔ4 != nil) {
            return (default, default, default, errΔ4);
        }
        var (echConfig, echPK, kdf, aead) = pickECHConfig(echConfigs);
        if (echConfig == nil) {
            return (default, default, default, errors.New(tlsˢ));
        }
        ech = Ꮡ(new echClientContext(config: echConfig, kdfID: kdf.ID(), aeadID: aead.ID()));
        hello.encryptedClientHello = slice<byte>([1]); // indicate inner hello
        // We need to explicitly set these 1.2 fields to nil, as we do not
        // marshal them when encoding the inner hello, otherwise transcripts
        // will later mismatch.
        hello.supportedPoints = default;
        hello.ticketSupported = false;
        hello.secureRenegotiationSupported = false;
        hello.extendedMasterSecret = false;

        var info = appendꓸꓸꓸ(slice<byte>("tls ech\x00"u8), ech.config.raw);
        (ech.encapsulatedKey, ech.hpkeContext, errΔ4) = hpke.NewSender(echPK, kdf, aead, info);
        if (errΔ4 != nil) {
            return (default, default, default, errΔ4);
        }
    }

    return (hello, keyShareKeys, ech, default);
}

partial struct echClientContext {
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
        c.curveID = 0;

        (var hello, var keyShareKeys, var ech, err) = Ꮡc.makeClientHello();
        if (err != nil) {
            goto ᒐdone;
        }

        (var session, var earlySecret, var binderKey, err) = Ꮡc.loadSession(hello);
        if (err != nil) {
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
                if (Ꮡerr.ValueSlot != nil) {
                    {
                        @string cacheKey = Ꮡc.Value.clientSessionCacheKey(); if (cacheKey != ""u8) {
                            Ꮡc.Value.config.ClientSessionCache.Put(cacheKey, nil);
                        }
                    }
                }
            }, ref ᒐ);
        }

        if (ech != nil) {
            // Split hello into inner and outer
            ech.innerHello = hello.clone();

            // Overwrite the server name in the outer hello with the public facing
            // name.
            hello.serverName = (@string)ech.config.PublicName;
            // Generate a new random for the outer hello.
            hello.random = new slice<byte>(32);
            (_, err) = io.ReadFull(c.config.rand(), hello.random);
            if (err != nil) {
                err = errors.New("tls: short read from Rand: "u8 + err.Error()); goto ᒐdone;
            }

            // NOTE: we don't do PSK GREASE, in line with boringssl, it's meant to
            // work around _possibly_ broken middleboxes, but there is little-to-no
            // evidence that this is actually a problem.

            {
                var errΔ1 = computeAndUpdateOuterECHExtension(hello, ech.innerHello, ref (ech).DerefOrNull(), true); if (errΔ1 != nil) {
                    err = errΔ1; goto ᒐdone;
                }
            }
        }

        c.serverName = hello.serverName;

        {
            var (_, errΔ2) = Ꮡc.writeHandshakeRecord(new clientHelloMsgжhandshakeMessage(hello), default); if (errΔ2 != nil) {
                err = errΔ2; goto ᒐdone;
            }
        }

        if (hello.earlyData) {
            var suite = cipherSuiteTLS13ByID(session.cipherSuite);
            var transcript = suite.hash.New();
            var transcriptHello = hello;
            if (ech != nil) {
                transcriptHello = ech.innerHello;
            }
            {
                var errΔ3 = transcriptMsg(new clientHelloMsgжhandshakeMessage(transcriptHello), new hash_HashᴠtranscriptHash(transcript)); if (errΔ3 != nil) {
                    err = errΔ3; goto ᒐdone;
                }
            }
            var earlyTrafficSecret = earlySecret.ClientEarlyTrafficSecret(transcript);
            c.quicSetWriteSecret(QUICEncryptionLevelEarly, suite.id, earlyTrafficSecret);
        }

        // serverHelloMsg is not included in the transcript
        (var msg, err) = Ꮡc.readHandshake(default);
        if (err != nil) {
            goto ᒐdone;
        }

        var (serverHello, ok) = msg._<ж<serverHelloMsg>>(ᐧ);
        if (!ok) {
            Ꮡc.sendAlert(alertUnexpectedMessage);
            err = unexpectedMessageError(serverHello.OrTypedNil(), msg); goto ᒐdone;
        }

        {
            var errΔ4 = Ꮡc.pickTLSVersion(serverHello); if (errΔ4 != nil) {
                err = errΔ4; goto ᒐdone;
            }
        }

        // If we are negotiating a protocol version that's lower than what we
        // support, check for the server downgrade canaries.
        // See RFC 8446, Section 4.1.3.
        var maxVers = c.config.maxSupportedVersion(roleClient);
        var tls12Downgrade = ((sstring)(serverHello.random[24..])) == downgradeCanaryTLS12;
        var tls11Downgrade = ((sstring)(serverHello.random[24..])) == downgradeCanaryTLS11;
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

// fips140ems is a GODEBUG variable that can be set to 0 to disable the
// enforcement of Extended Master Secret in FIPS 140-3 mode.
internal static ж<godebug.Setting> fips140ems = godebug.New("fips140ems"u8);

internal static (ж<SessionState> session, ж<tls13.EarlySecret> earlySecret, slice<byte> binderKey, error err) loadSession(this ж<Conn> Ꮡc, ж<clientHelloMsg> Ꮡhello) {
    ж<SessionState> session = default;
    ж<tls13.EarlySecret> earlySecret = default;
    slice<byte> binderKey = default;
    error err = default;
    ref var c = ref Ꮡc.DerefOrNull();
    ref var hello = ref Ꮡhello.DerefOrNull();

    if (c.config.SessionTicketsDisabled || c.config.ClientSessionCache == nil) {
        return (default, default, default, default);
    }

    var echInner = bytes.Equal(hello.encryptedClientHello, slice<byte>([1]));

    // ticketSupported is a TLS 1.2 extension (as TLS 1.3 replaced tickets with PSK
    // identities) and ECH requires and forces TLS 1.3.
    hello.ticketSupported = true && !echInner;

    if (hello.supportedVersions[0] == VersionTLS13) {
        // Require DHE on resumption as it guarantees forward secrecy against
        // compromise of the session ticket key. See RFC 8446, Section 4.2.9.
        hello.pskModes = slice<uint8>([pskModeDHE]);
    }

    // Session resumption is not allowed if renegotiating because
    // renegotiation is primarily used to allow a client to send a client
    // certificate, which would be skipped if session resumption occurred.
    if (c.handshakes != 0) {
        return (default, default, default, default);
    }

    // Try to resume a previously negotiated TLS session, if available.
    @string cacheKey = c.clientSessionCacheKey();
    if (cacheKey == ""u8) {
        return (default, default, default, default);
    }
    var (cs, ok) = c.config.ClientSessionCache.Get(cacheKey);
    if (!ok || cs == nil) {
        return (default, default, default, default);
    }
    session = cs.session;

    // Check that version used for the previous session is still valid.
    var versOk = false;
    foreach (var (_, v) in hello.supportedVersions) {
        if (v == session.version) {
            versOk = true;
            break;
        }
    }
    if (!versOk) {
        return (default, default, default, default);
    }

    if (c.config.time().After(session.peerCertificates[0].NotAfter)) {
        // Expired certificate, delete the entry.
        c.config.ClientSessionCache.Put(cacheKey, nil);
        return (default, default, default, default);
    }
    if (!c.config.InsecureSkipVerify) {
        if (len(session.verifiedChains) == 0) {
            // The original connection had InsecureSkipVerify, while this doesn't.
            return (default, default, default, default);
        }
        {
            var errΔ1 = session.peerCertificates[0].VerifyHostname(c.config.ServerName); if (errΔ1 != nil) {
                // This should be ensured by the cache key, but protect the
                // application from a faulty ClientSessionCache implementation.
                return (default, default, default, default);
            }
        }
        var opts = new Δx509.VerifyOptions(
            CurrentTime: c.config.time(),
            Roots: c.config.RootCAs,
            KeyUsages: slice<Δx509.ExtKeyUsage>([Δx509.ExtKeyUsageServerAuth])
        );
        if (!anyValidVerifiedChain(session.verifiedChains, opts)) {
            // No valid chains, delete the entry.
            c.config.ClientSessionCache.Put(cacheKey, nil);
            return (default, default, default, default);
        }
    }

    if (session.version != VersionTLS13) {
        // In TLS 1.2 the cipher suite must match the resumed session. Ensure we
        // are still offering it.
        if (mutualCipherSuite(hello.cipherSuites, session.cipherSuite) == nil) {
            return (default, default, default, default);
        }

        // FIPS 140-3 requires the use of Extended Master Secret.
        if (!session.extMasterSecret && fips140tls.Required()) {
            if (fips140ems.Value() != "0"u8) {
                return (default, default, default, default);
            }
            fips140ems.IncNonDefault();
        }

        hello.sessionTicket = session.ticket;
        return (session, earlySecret, binderKey, err);
    }

    // Check that the session ticket is not expired.
    if (c.config.time().After(time_package.Unix((int64)session.useBy, 0))) {
        c.config.ClientSessionCache.Put(cacheKey, nil);
        return (default, default, default, default);
    }

    // In TLS 1.3 the KDF hash must match the resumed session. Ensure we
    // offer at least one cipher suite with that hash.
    var cipherSuite = cipherSuiteTLS13ByID(session.cipherSuite);
    if (cipherSuite == nil) {
        return (default, default, default, default);
    }
    var cipherSuiteOk = false;
    foreach (var (_, offeredID) in hello.cipherSuites) {
        var offeredSuite = cipherSuiteTLS13ByID(offeredID);
        if (offeredSuite != nil && offeredSuite.hash == cipherSuite.hash) {
            cipherSuiteOk = true;
            break;
        }
    }
    if (!cipherSuiteOk) {
        return (default, default, default, default);
    }

    if (c.quic != nil) {
        if (c.quic.enableSessionEvents) {
            Ꮡc.quicResumeSession(session);
        }

        // For 0-RTT, the cipher suite has to match exactly, and we need to be
        // offering the same ALPN.
        if (session.EarlyData && mutualCipherSuiteTLS13(hello.cipherSuites, session.cipherSuite) != nil) {
            foreach (var (_, alpn) in hello.alpnProtocols) {
                if (alpn == session.alpnProtocol) {
                    hello.earlyData = true;
                    break;
                }
            }
        }
    }

    // Set the pre_shared_key extension. See RFC 8446, Section 4.2.11.1.
    var ticketAge = c.config.time().Sub(time_package.Unix((int64)session.createdAt, 0));
    var identity = new pskIdentity(
        label: session.ticket,
        obfuscatedTicketAge: (uint32)(ticketAge / time_package.Millisecond) + session.ageAdd
    );
    hello.pskIdentities = slice<pskIdentity>([identity]);
    hello.pskBinders = slice<slice<byte>>([new slice<byte>(cipherSuite.hash.Size())]);

    // Compute the PSK binders. See RFC 8446, Section 4.2.11.2.
    earlySecret = tls13.NewEarlySecret<hash.Hash>(() => cipherSuite.hash.New(), session.secret);
    binderKey = earlySecret.ResumptionBinderKey();
    var transcript = cipherSuite.hash.New();
    {
        var errΔ2 = computeAndUpdatePSK(Ꮡhello, binderKey, transcript, cipherSuite.finishedHash); if (errΔ2 != nil) {
            return (default, default, default, errΔ2);
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

    var (vers, ok) = c.config.mutualVersion(roleClient, slice<uint16>([peerVersion]));
    if (!ok) {
        Ꮡc.sendAlert(alertProtocolVersion);
        return fmt.Errorf("tls: server selected unsupported protocol version %x"u8, peerVersion);
    }

    c.vers = vers;
    c.haveVers = true;
    c.@in.version = vers;
    c.@out.version = vers;

    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerEchoedTls13ˢ = "tls: server echoed TLS 1.3 compatibility session ID in TLS 1.2"u8;

// Does the handshake, either a full one or resumes old session. Requires hs.c,
// hs.hello, hs.serverHello, and, optionally, hs.session to be set.
internal static error handshake(this ж<clientHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;

    // If we did not load a session (hs.session == nil), but we did set a
    // session ID in the transmitted client hello (hs.hello.sessionId != nil),
    // it means we tried to negotiate TLS 1.3 and sent a random session ID as a
    // compatibility measure (see RFC 8446, Section 4.1.2).
    //
    // Since we're now handshaking for TLS 1.2, if the server echoed the
    // transmitted ID back to us, we know mischief is afoot: the session ID
    // was random and can't possibly be recognized by the server.
    if (hs.session == nil && hs.hello.sessionId != nil && bytes.Equal(hs.hello.sessionId, hs.serverHello.sessionId)) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerEchoedTls13ˢ);
    }

    var (isResume, err) = hs.processServerHello();
    if (err != nil) {
        return err;
    }

    hs.finishedHash = newFinishedHash(c.vers, ref (hs.suite).DerefOrNull());

    // No signatures of the handshake are needed in a resumption.
    // Otherwise, in a full handshake, if we don't have any certificates
    // configured then we will never send a CertificateVerify message and
    // thus no signatures are needed in that case either.
    if (isResume || (len(c.config.Certificates) == 0 && c.config.GetClientCertificate == nil)) {
        hs.finishedHash.discardHandshakeBuffer();
    }

    {
        var errΔ1 = transcriptMsg(new clientHelloMsgжhandshakeMessage(hs.hello), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ1 != nil) {
            return errΔ1;
        }
    }
    {
        var errΔ2 = transcriptMsg(new serverHelloMsgжhandshakeMessage(hs.serverHello), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ2 != nil) {
            return errΔ2;
        }
    }

    c.buffering = true;
    c.didResume = isResume;
    if (isResume) {
        {
            var errΔ3 = hs.establishKeys(); if (errΔ3 != nil) {
                return errΔ3;
            }
        }
        {
            var errΔ4 = Ꮡhs.readSessionTicket(); if (errΔ4 != nil) {
                return errΔ4;
            }
        }
        {
            var errΔ5 = Ꮡhs.readFinished(c.serverFinished[..]); if (errΔ5 != nil) {
                return errΔ5;
            }
        }
        c.clientFinishedIsFirst = false;
        // Make sure the connection is still being verified whether or not this
        // is a resumption. Resumptions currently don't reverify certificates so
        // they don't call verifyServerCertificate. See Issue 31641.
        if (c.config.VerifyConnection != nil) {
            {
                var errΔ6 = c.config.VerifyConnection(c.connectionStateLocked()); if (errΔ6 != nil) {
                    c.sendAlert(alertBadCertificate);
                    return errΔ6;
                }
            }
        }
        {
            var errΔ7 = Ꮡhs.sendFinished(c.clientFinished[..]); if (errΔ7 != nil) {
                return errΔ7;
            }
        }
        {
            var (_, errΔ8) = c.flush(); if (errΔ8 != nil) {
                return errΔ8;
            }
        }
    } else {
        {
            var errΔ9 = Ꮡhs.doFullHandshake(); if (errΔ9 != nil) {
                return errΔ9;
            }
        }
        {
            var errΔ10 = hs.establishKeys(); if (errΔ10 != nil) {
                return errΔ10;
            }
        }
        {
            var errΔ11 = Ꮡhs.sendFinished(c.clientFinished[..]); if (errΔ11 != nil) {
                return errΔ11;
            }
        }
        {
            var (_, errΔ12) = c.flush(); if (errΔ12 != nil) {
                return errΔ12;
            }
        }
        c.clientFinishedIsFirst = true;
        {
            var errΔ13 = Ꮡhs.readSessionTicket(); if (errΔ13 != nil) {
                return errΔ13;
            }
        }
        {
            var errΔ14 = Ꮡhs.readFinished(c.serverFinished[..]); if (errΔ14 != nil) {
                return errΔ14;
            }
        }
    }
    {
        var errΔ15 = hs.saveSessionTicket(); if (errΔ15 != nil) {
            return errΔ15;
        }
    }

    c.ekm = ekmFromMasterSecret(c.vers, hs.suite, hs.masterSecret, hs.hello.random, hs.serverHello.random);
    c.of(Conn.ᏑisHandshakeComplete).Store(true);

    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerChoseAnˢ = "tls: server chose an unconfigured cipher suite"u8;

internal static error pickCipherSuite(this ref clientHandshakeState hs) {
    {
        hs.suite = mutualCipherSuite(hs.hello.cipherSuites, hs.serverHello.cipherSuite); if (hs.suite == nil) {
            hs.c.sendAlert(alertHandshakeFailure);
            return errors.New(tlsServerChoseAnˢ);
        }
    }

    if (hs.c.config.CipherSuites == nil && !fips140tls.Required() && rsaKexCiphers[hs.suite.id]) {
        tlsrsakex.Value(); // ensure godebug is initialized
        tlsrsakex.IncNonDefault();
    }
    if (hs.c.config.CipherSuites == nil && !fips140tls.Required() && tdesCiphers[hs.suite.id]) {
        tls3des.Value(); // ensure godebug is initialized
        tls3des.IncNonDefault();
    }

    hs.c.cipherSuite = hs.suite.id;
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsReceivedUnexpectedˢ = "tls: received unexpected CertificateStatus message"u8;
internal static readonly @string tlsServerSIdentityˢ = "tls: server's identity changed during renegotiation"u8;
internal static readonly @string tlsFips1403RequiresTheˢ = "tls: FIPS 140-3 requires the use of Extended Master Secret"u8;
internal static readonly @string tlsInternalErrorDidNotˢ = "tls: internal error: did not keep handshake transcript for TLS 1.2"u8;

internal static error doFullHandshake(this ж<clientHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;

    var (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
    if (err != nil) {
        return err;
    }
    var (certMsg, ok) = msg._<ж<certificateMsg>>(ᐧ);
    if (!ok || len(certMsg.certificates) == 0) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(certMsg.OrTypedNil(), msg);
    }

    (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
    if (err != nil) {
        return err;
    }

    (var cs, ok) = msg._<ж<certificateStatusMsg>>(ᐧ);
    if (ok) {
        // RFC4366 on Certificate Status Request:
        // The server MAY return a "certificate_status" message.

        if (!hs.serverHello.ocspStapling) {
            // If a server returns a "CertificateStatus" message, then the
            // server MUST have included an extension of type "status_request"
            // with empty "extension_data" in the extended server hello.

            c.sendAlert(alertUnexpectedMessage);
            return errors.New(tlsReceivedUnexpectedˢ);
        }

        c.ocspResponse = cs.response;

        (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
        if (err != nil) {
            return err;
        }
    }

    if (c.handshakes == 0) {
        // If this is the first handshake on a connection, process and
        // (optionally) verify the server's certificates.
        {
            var errΔ1 = c.verifyServerCertificate(certMsg.certificates); if (errΔ1 != nil) {
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
        if (!bytes.Equal(c.peerCertificates[0].Raw, certMsg.certificates[0])) {
            c.sendAlert(alertBadCertificate);
            return errors.New(tlsServerSIdentityˢ);
        }
    }

    var keyAgreement = hs.suite.ka(c.vers);

    (var skx, ok) = msg._<ж<serverKeyExchangeMsg>>(ᐧ);
    if (ok) {
        err = keyAgreement.processServerKeyExchange(c.config, hs.hello, hs.serverHello, c.peerCertificates[0], skx);
        if (err != nil) {
            c.sendAlert(alertIllegalParameter);
            return err;
        }
        {
            var (keyAgreementΔ1, okΔ1) = keyAgreement._<ж<ecdheKeyAgreement>>(ᐧ); if (okΔ1) {
                c.curveID = keyAgreementΔ1.curveID;
                c.peerSigAlg = keyAgreementΔ1.signatureAlgorithm;
            }
        }

        (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
        if (err != nil) {
            return err;
        }
    }

    ж<Certificate> chainToSend = default;
    bool certRequested = default;
    (var certReq, ok) = msg._<ж<certificateRequestMsg>>(ᐧ);
    if (ok) {
        certRequested = true;

        var cri = certificateRequestInfoFromMsg(hs.ctx, c.vers, ref (certReq).DerefOrNull());
        {
            (chainToSend, err) = c.getClientCertificate(cri); if (err != nil) {
                c.sendAlert(alertInternalError);
                return err;
            }
        }

        (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
        if (err != nil) {
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
        certMsg.certificates = chainToSend.ΔCertificate;
        {
            var (_, errΔ2) = hs.c.writeHandshakeRecord(new certificateMsgжhandshakeMessage(certMsg), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ2 != nil) {
                return errΔ2;
            }
        }
    }

    (var preMasterSecret, var ckx, err) = keyAgreement.generateClientKeyExchange(c.config, hs.hello, c.peerCertificates[0]);
    if (err != nil) {
        c.sendAlert(alertInternalError);
        return err;
    }
    if (ckx != nil) {
        {
            var (_, errΔ3) = hs.c.writeHandshakeRecord(new clientKeyExchangeMsgжhandshakeMessage(ckx), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ3 != nil) {
                return errΔ3;
            }
        }
    }

    if (hs.serverHello.extendedMasterSecret) {
        c.extMasterSecret = true;
        hs.masterSecret = extMasterFromPreMasterSecret(c.vers, ref (hs.suite).DerefOrNull(), preMasterSecret,
            hs.finishedHash.Sum());
    } else {
        if (fips140tls.Required()) {
            if (fips140ems.Value() != "0"u8) {
                c.sendAlert(alertHandshakeFailure);
                return errors.New(tlsFips1403RequiresTheˢ);
            }
            fips140ems.IncNonDefault();
        }
        hs.masterSecret = masterFromPreMasterSecret(c.vers, ref (hs.suite).DerefOrNull(), preMasterSecret,
            hs.hello.random, hs.serverHello.random);
    }
    {
        var errΔ4 = c.config.writeKeyLog(keyLogLabelTLS12, hs.hello.random, hs.masterSecret); if (errΔ4 != nil) {
            c.sendAlert(alertInternalError);
            return errors.New("tls: failed to write to key log: "u8 + errΔ4.Error());
        }
    }

    if (chainToSend != nil && len(chainToSend.ΔCertificate) > 0) {
        var certVerify = Ꮡ(new certificateVerifyMsg());

        var (key, okΔ2) = chainToSend.PrivateKey._<crypto.Signer>(ᐧ);
        if (!okΔ2) {
            c.sendAlert(alertInternalError);
            return fmt.Errorf("tls: client certificate private key of type %T does not implement crypto.Signer"u8, chainToSend.PrivateKey);
        }

        if (c.vers >= VersionTLS12) {
            var (signatureAlgorithm, errΔ5) = selectSignatureScheme(c.vers, chainToSend, certReq.supportedSignatureAlgorithms);
            if (errΔ5 != nil) {
                c.sendAlert(alertHandshakeFailure);
                return errΔ5;
            }
            ref var sigHash = ref heap<crypto.Hash>(out var ᏑsigHash);
            (var sigType, sigHash, errΔ5) = typeAndHashFromSignatureScheme(signatureAlgorithm);
            if (errΔ5 != nil) {
                return c.sendAlert(alertInternalError);
            }
            certVerify.hasSignatureAlgorithm = true;
            certVerify.signatureAlgorithm = signatureAlgorithm;
            if (sigHash == crypto.SHA1) {
                tlssha1.Value(); // ensure godebug is initialized
                tlssha1.IncNonDefault();
            }
            if (hs.finishedHash.buffer == nil) {
                c.sendAlert(alertInternalError);
                return errors.New(tlsInternalErrorDidNotˢ);
            }
            var signOpts = ((crypto.SignerOpts)sigHash);
            if (sigType == signatureRSAPSS) {
                signOpts = new rsa_PSSOptionsжSignerOpts(Ꮡ(new rsa.PSSOptions(SaltLength: rsa.PSSSaltLengthEqualsHash, Hash: sigHash)));
            }
            (certVerify.signature, errΔ5) = crypto.SignMessage(key, c.config.rand(), hs.finishedHash.buffer, signOpts);
            if (errΔ5 != nil) {
                c.sendAlert(alertInternalError);
                return errΔ5;
            }
        } else {
            var (sigType, sigHash, errΔ6) = legacyTypeAndHashFromPublicKey(key.Public());
            if (errΔ6 != nil) {
                c.sendAlert(alertIllegalParameter);
                return errΔ6;
            }
            var signed = hs.finishedHash.hashForClientCertificate(sigType);
            (certVerify.signature, errΔ6) = key.Sign(c.config.rand(), signed, sigHash);
            if (errΔ6 != nil) {
                c.sendAlert(alertInternalError);
                return errΔ6;
            }
        }

        {
            var (_, errΔ7) = hs.c.writeHandshakeRecord(new certificateVerifyMsgжhandshakeMessage(certVerify), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ7 != nil) {
                return errΔ7;
            }
        }
    }

    hs.finishedHash.discardHandshakeBuffer();

    return default;
}

internal static error establishKeys(this ref clientHandshakeState hs) {
    var c = hs.c;

    var (clientMAC, serverMAC, clientKey, serverKey, clientIV, serverIV) = keysFromMasterSecret(c.vers, ref (hs.suite).DerefOrNull(), hs.masterSecret, hs.hello.random, hs.serverHello.random, hs.suite.macLen, hs.suite.keyLen, hs.suite.ivLen);
    any clientCipher = default;
    any serverCipher = default;
    hash.Hash clientHash = default;
    hash.Hash serverHash = default;
    if (hs.suite.cipher != nil) {
        clientCipher = hs.suite.cipher(clientKey, clientIV, false /* not for reading */);
        clientHash = hs.suite.mac(clientMAC);
        serverCipher = hs.suite.cipher(serverKey, serverIV, true /* for reading */);
        serverHash = hs.suite.mac(serverMAC);
    } else {
        clientCipher = hs.suite.aead(clientKey, clientIV);
        serverCipher = hs.suite.aead(serverKey, serverIV);
    }

    c.of(Conn.Ꮡin).prepareCipherSpec(c.vers, serverCipher, serverHash);
    c.of(Conn.Ꮡout).prepareCipherSpec(c.vers, clientCipher, clientHash);
    return default;
}

internal static bool serverResumedSession(this ref clientHandshakeState hs) {
    // If the server responded with the same sessionId then it means the
    // sessionTicket is being used to resume a TLS session.
    return hs.session != nil && hs.hello.sessionId != nil && bytes.Equal(hs.serverHello.sessionId, hs.hello.sessionId);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerSelectedˢ = "tls: server selected unsupported compression format"u8;
internal static readonly @string tlsServerOfferedOnlyˢ = "tls: server offered only incompatible point formats"u8;
internal static readonly @string tlsInitialHandshakeHadˢ = "tls: initial handshake had non-empty renegotiation extension"u8;
internal static readonly @string tlsIncorrectˢ = "tls: incorrect renegotiation extension contents"u8;
internal static readonly @string tlsServerResumedASessionˢ = "tls: server resumed a session with a different version"u8;
internal static readonly @string tlsServerResumedASessionˢ2 = "tls: server resumed a session with a different cipher suite"u8;
internal static readonly @string tlsServerResumedASessionˢ3 = "tls: server resumed a session with a different EMS extension"u8;

internal static (bool, error) processServerHello(this ref clientHandshakeState hs) {
    var c = hs.c;

    {
        var err = hs.pickCipherSuite(); if (err != nil) {
            return (false, err);
        }
    }

    if (hs.serverHello.compressionMethod != compressionNone) {
        c.sendAlert(alertIllegalParameter);
        return (false, errors.New(tlsServerSelectedˢ));
    }

    var supportsPointFormat = false;
    var offeredNonCompressedFormat = false;
    foreach (var (_, format) in hs.serverHello.supportedPoints) {
        if (format == pointFormatUncompressed) {
            supportsPointFormat = true;
        } else {
            offeredNonCompressedFormat = true;
        }
    }
    if (!supportsPointFormat && offeredNonCompressedFormat) {
        return (false, errors.New(tlsServerOfferedOnlyˢ));
    }

    if (c.handshakes == 0 && hs.serverHello.secureRenegotiationSupported) {
        c.secureRenegotiation = true;
        if (len(hs.serverHello.secureRenegotiation) != 0) {
            c.sendAlert(alertHandshakeFailure);
            return (false, errors.New(tlsInitialHandshakeHadˢ));
        }
    }

    if (c.handshakes > 0 && c.secureRenegotiation) {
        array<byte> expectedSecureRenegotiation = new(24);
        copy(expectedSecureRenegotiation[..], c.clientFinished[..]);
        copy(expectedSecureRenegotiation[12..], c.serverFinished[..]);
        if (!bytes.Equal(hs.serverHello.secureRenegotiation, expectedSecureRenegotiation[..])) {
            c.sendAlert(alertHandshakeFailure);
            return (false, errors.New(tlsIncorrectˢ));
        }
    }

    {
        var err = checkALPN(hs.hello.alpnProtocols, hs.serverHello.alpnProtocol, false); if (err != nil) {
            c.sendAlert(alertUnsupportedExtension);
            return (false, err);
        }
    }
    c.clientProtocol = hs.serverHello.alpnProtocol;

    c.scts = hs.serverHello.scts;

    if (!hs.serverResumedSession()) {
        return (false, default);
    }

    if (hs.session.version != c.vers) {
        c.sendAlert(alertHandshakeFailure);
        return (false, errors.New(tlsServerResumedASessionˢ));
    }

    if (hs.session.cipherSuite != hs.suite.id) {
        c.sendAlert(alertHandshakeFailure);
        return (false, errors.New(tlsServerResumedASessionˢ2));
    }

    // RFC 7627, Section 5.3
    if (hs.session.extMasterSecret != hs.serverHello.extendedMasterSecret) {
        c.sendAlert(alertHandshakeFailure);
        return (false, errors.New(tlsServerResumedASessionˢ3));
    }

    // Restore master secret and certificates from previous state
    hs.masterSecret = hs.session.secret;
    c.extMasterSecret = hs.session.extMasterSecret;
    c.peerCertificates = hs.session.peerCertificates;
    c.verifiedChains = hs.session.verifiedChains;
    c.ocspResponse = hs.session.ocspResponse;
    // Let the ServerHello SCTs override the session SCTs from the original
    // connection, if any are provided.
    if (len(c.scts) == 0 && len(hs.session.scts) != 0) {
        c.scts = hs.session.scts;
    }
    c.curveID = hs.session.curveID;

    return (true, default);
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
        return default;
    }
    if (len(clientProtos) == 0) {
        return errors.New(tlsServerAdvertisedˢ);
    }
    foreach (var (_, proto) in clientProtos) {
        if (proto == serverProto) {
            return default;
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
        var errΔ1 = c.readChangeCipherSpec(); if (errΔ1 != nil) {
            return errΔ1;
        }
    }

    // finishedMsg is included in the transcript, but not until after we
    // check the client version, since the state before this message was
    // sent is used during verification.
    var (msg, err) = c.readHandshake(default);
    if (err != nil) {
        return err;
    }
    var (serverFinished, ok) = msg._<ж<finishedMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(serverFinished.OrTypedNil(), msg);
    }

    var verify = hs.finishedHash.serverSum(hs.masterSecret);
    if (len(verify) != len(serverFinished.verifyData) || subtle.ConstantTimeCompare(verify, serverFinished.verifyData) != 1) {
        c.sendAlert(alertHandshakeFailure);
        return errors.New(tlsServerSFinishedˢ);
    }

    {
        var errΔ2 = transcriptMsg(new finishedMsgжhandshakeMessage(serverFinished), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (errΔ2 != nil) {
            return errΔ2;
        }
    }

    copy(@out, verify);
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerSentUnrequestedˢ = "tls: server sent unrequested session ticket"u8;

internal static error readSessionTicket(this ж<clientHandshakeState> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    if (!hs.serverHello.ticketSupported) {
        return default;
    }
    var c = hs.c;

    if (!hs.hello.ticketSupported) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerSentUnrequestedˢ);
    }

    var (msg, err) = c.readHandshake(new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash)));
    if (err != nil) {
        return err;
    }
    var (sessionTicketMsg, ok) = msg._<ж<newSessionTicketMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(sessionTicketMsg.OrTypedNil(), msg);
    }

    hs.ticket = sessionTicketMsg.ticket;
    return default;
}

internal static error saveSessionTicket(this ref clientHandshakeState hs) {
    if (hs.ticket == nil) {
        return default;
    }
    var c = hs.c;

    @string cacheKey = c.clientSessionCacheKey();
    if (cacheKey == ""u8) {
        return default;
    }

    var session = c.sessionState();
    session.secret = hs.masterSecret;
    session.ticket = hs.ticket;

    var cs = Ꮡ(new ClientSessionState(session: session));
    c.config.ClientSessionCache.Put(cacheKey, cs);
    return default;
}

internal static error sendFinished(this ж<clientHandshakeState> Ꮡhs, slice<byte> @out) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;

    {
        var err = c.writeChangeCipherRecord(); if (err != nil) {
            return err;
        }
    }

    var finished = @new<finishedMsg>();
    finished.verifyData = hs.finishedHash.clientSum(hs.masterSecret);
    {
        var (_, err) = hs.c.writeHandshakeRecord(new finishedMsgжhandshakeMessage(finished), new ΔfinishedHashжtranscriptHash(Ꮡhs.of(clientHandshakeState.ᏑfinishedHash))); if (err != nil) {
            return err;
        }
    }
    copy(@out, finished.verifyData);
    return default;
}

// defaultMaxRSAKeySize is the maximum RSA key size in bits that we are willing
// to verify the signatures of during a TLS handshake.
internal static UntypedInt defaultMaxRSAKeySize => 8192;

internal static ж<godebug.Setting> tlsmaxrsasize = godebug.New("tlsmaxrsasize"u8);

internal static (nint max, bool ok) checkKeySize(nint n) {
    {
        @string v = tlsmaxrsasize.Value(); if (v != ""u8) {
            {
                var (maxΔ1, err) = strconv.Atoi(v); if (err == nil) {
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

    var certs = new slice<ж<Δx509.Certificate>>(len(certificates));
    foreach (var (i, asn1Data) in certificates) {
        var (cert, err) = globalCertCache.newCert(asn1Data);
        if (err != nil) {
            Ꮡc.sendAlert(alertDecodeError);
            return errors.New("tls: failed to parse certificate from server: "u8 + err.Error());
        }
        if (cert.PublicKeyAlgorithm == Δx509.RSA) {
            nint n = cert.PublicKey._<ж<rsa.PublicKey>>().N.BitLen();
            {
                var (max, ok) = checkKeySize(n); if (!ok) {
                    Ꮡc.sendAlert(alertBadCertificate);
                    return fmt.Errorf("tls: server sent certificate containing RSA key larger than %d bits"u8, max);
                }
            }
        }
        certs[i] = cert;
    }

    var echRejected = c.config.EncryptedClientHelloConfigList != nil && !c.echAccepted;
    if (echRejected) {
        if (c.config.EncryptedClientHelloRejectionVerify != nil) {
            {
                var err = c.config.EncryptedClientHelloRejectionVerify(Ꮡc.connectionStateLocked()); if (err != nil) {
                    Ꮡc.sendAlert(alertBadCertificate);
                    return err;
                }
            }
        } else {
            var opts = new Δx509.VerifyOptions(
                Roots: c.config.RootCAs,
                CurrentTime: c.config.time(),
                DNSName: c.serverName,
                Intermediates: Δx509.NewCertPool()
            );

            foreach (var (_, cert) in certs[1..]) {
                opts.Intermediates.AddCert(cert);
            }
            var (chains, err) = certs[0].Verify(opts);
            if (err != nil) {
                Ꮡc.sendAlert(alertBadCertificate);
                return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: err)));
            }

            (c.verifiedChains, err) = fipsAllowedChains(chains);
            if (err != nil) {
                Ꮡc.sendAlert(alertBadCertificate);
                return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: err)));
            }
        }
    } else if (!c.config.InsecureSkipVerify) {
        var opts = new Δx509.VerifyOptions(
            Roots: c.config.RootCAs,
            CurrentTime: c.config.time(),
            DNSName: c.config.ServerName,
            Intermediates: Δx509.NewCertPool()
        );

        foreach (var (_, cert) in certs[1..]) {
            opts.Intermediates.AddCert(cert);
        }
        var (chains, err) = certs[0].Verify(opts);
        if (err != nil) {
            Ꮡc.sendAlert(alertBadCertificate);
            return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: err)));
        }

        (c.verifiedChains, err) = fipsAllowedChains(chains);
        if (err != nil) {
            Ꮡc.sendAlert(alertBadCertificate);
            return new CertificateVerificationErrorжerror(Ꮡ(new CertificateVerificationError(UnverifiedCertificates: certs, Err: err)));
        }
    }

    switch (certs[0].PublicKey.type()) {
    case ж<rsa.PublicKey> _:
    case ж<ecdsa.PublicKey> _:
    case ed25519.PublicKey _: {
        break;
        break;
    }
    default: {
        Ꮡc.sendAlert(alertUnsupportedCertificate);
        return fmt.Errorf("tls: server's certificate contains an unsupported type of public key: %T"u8, certs[0].PublicKey);
    }}

    c.peerCertificates = certs;

    if (c.config.VerifyPeerCertificate != nil && !echRejected) {
        {
            var err = c.config.VerifyPeerCertificate(certificates, c.verifiedChains); if (err != nil) {
                Ꮡc.sendAlert(alertBadCertificate);
                return err;
            }
        }
    }

    if (c.config.VerifyConnection != nil && !echRejected) {
        {
            var err = c.config.VerifyConnection(Ꮡc.connectionStateLocked()); if (err != nil) {
                Ꮡc.sendAlert(alertBadCertificate);
                return err;
            }
        }
    }

    return default;
}

// certificateRequestInfoFromMsg generates a CertificateRequestInfo from a TLS
// <= 1.2 CertificateRequest, making an effort to fill in missing information.
internal static ж<CertificateRequestInfo> certificateRequestInfoFromMsg(context.Context ctx, uint16 vers, ref certificateRequestMsg certReq) {
    var cri = Ꮡ(new CertificateRequestInfo(
        AcceptableCAs: certReq.certificateAuthorities,
        Version: vers,
        ctx: ctx
    ));

    bool rsaAvail = default;
    bool ecAvail = default;
    foreach (var (_, certType) in certReq.certificateTypes) {
        switch (certType) {
        case var ᴛ when ᴛ == certTypeRSASign: {
            rsaAvail = true;
            break;
        }
        case var ᴛ when ᴛ == certTypeECDSASign: {
            ecAvail = true;
            break;
        }}
    }

    if (!certReq.hasSignatureAlgorithm) {
        // Prior to TLS 1.2, signature schemes did not exist. In this case we
        // make up a list based on the acceptable certificate types, to help
        // GetClientCertificate and SupportsCertificate select the right certificate.
        // The hash part of the SignatureScheme is a lie here, because
        // TLS 1.0 and 1.1 always use MD5+SHA1 for RSA and SHA1 for ECDSA.
        switch (ᐧ) {
        case {} when rsaAvail && ecAvail: {
            cri.SignatureSchemes = slice<SignatureScheme>([
                ECDSAWithP256AndSHA256, ECDSAWithP384AndSHA384, ECDSAWithP521AndSHA512,
                PKCS1WithSHA256, PKCS1WithSHA384, PKCS1WithSHA512, PKCS1WithSHA1
            ]);
            break;
        }
        case {} when rsaAvail: {
            cri.SignatureSchemes = slice<SignatureScheme>([
                PKCS1WithSHA256, PKCS1WithSHA384, PKCS1WithSHA512, PKCS1WithSHA1
            ]);
            break;
        }
        case {} when ecAvail: {
            cri.SignatureSchemes = slice<SignatureScheme>([
                ECDSAWithP256AndSHA256, ECDSAWithP384AndSHA384, ECDSAWithP521AndSHA512
            ]);
            break;
        }}
        return cri;
    }

    // Filter the signature schemes based on the certificate types.
    // See RFC 5246, Section 7.4.4 (where it calls this "somewhat complicated").
    cri.SignatureSchemes = new slice<SignatureScheme>(0, len(certReq.supportedSignatureAlgorithms));
    foreach (var (_, sigScheme) in certReq.supportedSignatureAlgorithms) {
        var (sigType, _, err) = typeAndHashFromSignatureScheme(sigScheme);
        if (err != nil) {
            continue;
        }
        switch (sigType) {
        case signatureECDSA or signatureEd25519: {
            if (ecAvail) {
                cri.SignatureSchemes = append(cri.SignatureSchemes, sigScheme);
            }
            break;
        }
        case signatureRSAPSS or signaturePKCS1v15: {
            if (rsaAvail) {
                cri.SignatureSchemes = append(cri.SignatureSchemes, sigScheme);
            }
            break;
        }}
    }

    return cri;
}

internal static (ж<Certificate>, error) getClientCertificate(this ref Conn c, ж<CertificateRequestInfo> Ꮡcri) {
    ref var cri = ref Ꮡcri.DerefOrNull();

    if (c.config.GetClientCertificate != nil) {
        return c.config.GetClientCertificate(Ꮡcri);
    }

    foreach (var (_, vᴛ1) in c.config.Certificates) {
        ref var chain = ref heap(new Certificate(), out var Ꮡchain);
        chain = vᴛ1;

        {
            var err = cri.SupportsCertificate(Ꮡchain); if (err != nil) {
                continue;
            }
        }
        return (Ꮡchain, default);
    }

    // No acceptable certificate found. Don't send a certificate.
    return (@new<Certificate>(), default);
}

// clientSessionCacheKey returns a key used to cache sessionTickets that could
// be used to resume previously negotiated TLS sessions with a server.
internal static @string clientSessionCacheKey(this ref Conn c) {
    if (len(c.config.ServerName) > 0) {
        return c.config.ServerName;
    }
    if (c.conn != nil) {
        return c.conn.RemoteAddr().String();
    }
    return ""u8;
}

// hostnameInSNI converts name into an appropriate hostname for SNI.
// Literal IP addresses and absolute FQDNs are not permitted as SNI values.
// See RFC 6066, Section 3.
internal static @string hostnameInSNI(@string name) {
    @string host = name;
    if (len(host) > 0 && host[0] == '[' && host[len(host) - 1] == ']') {
        host = host.slice(1, len(host) - 1);
    }
    {
        nint i = strings.LastIndex(host, "%"u8); if (i > 0) {
            host = host.slice(0, i);
        }
    }
    if (net.ParseIP(host) != nil) {
        return ""u8;
    }
    while (len(name) > 0 && name[len(name) - 1] == '.') {
        name = name.slice(0, len(name) - 1);
    }
    return name;
}

internal static error computeAndUpdatePSK(ж<clientHelloMsg> Ꮡm, slice<byte> binderKey, hash.Hash transcript, Func<slice<byte>, hash.Hash, slice<byte>> ΔfinishedHash) {
    ref var m = ref Ꮡm.DerefOrNull();

    var (helloBytes, err) = Ꮡm.marshalWithoutBinders();
    if (err != nil) {
        return err;
    }
    transcript.Write(helloBytes);
    var pskBinders = slice<slice<byte>>([ΔfinishedHash(binderKey, transcript)]);
    return m.updateBinders(pskBinders);
}

} // end tls_package
