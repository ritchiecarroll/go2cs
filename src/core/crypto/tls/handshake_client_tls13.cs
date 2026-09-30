// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using context = context_package;
using crypto = crypto_package;
using hmac = go.crypto.hmac_package;
using hkdf = go.crypto.@internal.fips140.hkdf_package;
using mlkem = go.crypto.@internal.fips140.mlkem_package;
using tls13 = go.crypto.@internal.fips140.tls13_package;
using rsa = go.crypto.rsa_package;
using subtle = go.crypto.subtle_package;
using errors = errors_package;
using hash = hash_package;
using slices = slices_package;
using time = time_package;
using ecdh = go.crypto.ecdh_package;
using fips140 = go.crypto.@internal.fips140_package;
using go.crypto;
using go.crypto.@internal.fips140;
using go.sync;
using io = io_package;
using Δx509 = go.crypto.x509_package;

partial class tls_package {

[GoType] partial struct clientHandshakeStateTLS13 {
    internal ж<Conn> c;
    internal context.Context ctx;
    internal ж<serverHelloMsg> serverHello;
    internal ж<clientHelloMsg> hello;
    internal ж<keySharePrivateKeys> keyShareKeys;
    internal ж<SessionState> session;
    internal ж<tls13.EarlySecret> earlySecret;
    internal slice<byte> binderKey;
    internal ж<certificateRequestMsgTLS13> certReq;
    internal bool usingPSK;
    internal bool sentDummyCCS;
    internal ж<cipherSuiteTLS13> suite;
    internal hash.Hash transcript;
    internal ж<tls13ꓸMasterSecret> masterSecret;
    internal slice<byte> trafficSecret; // client_application_traffic_secret_0
    internal ж<echClientContext> echContext;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerSelectedTls13Inˢ = "tls: server selected TLS 1.3 in a renegotiation"u8;
internal static readonly @string echAcceptConfirmationˢ = "ech accept confirmation"u8;
internal static readonly @string tlsUnexpectedEncryptedˢ = "tls: unexpected encrypted client hello extension in server hello despite ECH being accepted"u8;
internal static readonly @string tlsUnexpectedServerNameˢ = "tls: unexpected server_name extension in server hello"u8;

// handshake requires hs.c, hs.hello, hs.serverHello, hs.keyShareKeys, and,
// optionally, hs.session, hs.earlySecret and hs.binderKey to be set.
internal static error handshake(this ж<clientHandshakeStateTLS13> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    // The server must not select TLS 1.3 in a renegotiation. See RFC 8446,
    // sections 4.1.2 and 4.1.3.
    if ((~c).handshakes > 0) {
        c.sendAlert(alertProtocolVersion);
        return errors.New(tlsServerSelectedTls13Inˢ);
    }
    // Consistency check on the presence of a keyShare and its parameters.
    if (hs.keyShareKeys == nil || (~hs.keyShareKeys).ecdhe == nil || len((~hs.hello).keyShares) == 0) {
        return c.sendAlert(alertInternalError);
    }
    {
        var err = hs.checkServerHelloOrHRR(); if (err != default!) {
            return err;
        }
    }
    hs.transcript = (~hs.suite).hash.New();
    {
        var err = transcriptMsg(new clientHelloMsgжhandshakeMessage(hs.hello), new hash_HashᴠtranscriptHash(hs.transcript)); if (err != default!) {
            return err;
        }
    }
    if (hs.echContext != nil) {
        hs.echContext.Value.innerTranscript = (~hs.suite).hash.New();
        {
            var err = transcriptMsg(new clientHelloMsgжhandshakeMessage((~hs.echContext).innerHello), new hash_HashᴠtranscriptHash((~hs.echContext).innerTranscript)); if (err != default!) {
                return err;
            }
        }
    }
    if (bytes.Equal((~hs.serverHello).random, helloRetryRequestRandom)) {
        {
            var err = hs.sendDummyChangeCipherSpec(); if (err != default!) {
                return err;
            }
        }
        {
            var err = hs.processHelloRetryRequest(); if (err != default!) {
                return err;
            }
        }
    }
    if (hs.echContext != nil) {
        var confTranscript = cloneHash((~hs.echContext).innerTranscript, (~hs.suite).hash);
        confTranscript.Write((~hs.serverHello).original[..30]);
        confTranscript.Write(new slice<byte>(8));
        confTranscript.Write((~hs.serverHello).original[38..]);
        var recvʗ1 = (~hs.suite).hash;

            var recvʗ2 = (~hs.suite).hash;
        var acceptConfirmation = tls13.ExpandLabel<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => recvʗ1.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)),
            hkdf.Extract<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => recvʗ2.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), (~(~hs.echContext).innerHello).random, default!),
            echAcceptConfirmationˢ,
            confTranscript.Sum(default!),
            8);
        if (subtle.ConstantTimeCompare(acceptConfirmation, (~hs.serverHello).random.slice(len((~hs.serverHello).random) - 8)) == 1){
            hs.hello = hs.echContext.Value.innerHello;
            c.Value.serverName = c.Value.config.Value.ServerName;
            hs.transcript = hs.echContext.Value.innerTranscript;
            c.Value.echAccepted = true;
            if ((~hs.serverHello).encryptedClientHello != default!) {
                c.sendAlert(alertUnsupportedExtension);
                return errors.New(tlsUnexpectedEncryptedˢ);
            }
            if ((~hs.hello).serverName == ""u8 && (~hs.serverHello).serverNameAck) {
                c.sendAlert(alertUnsupportedExtension);
                return errors.New(tlsUnexpectedServerNameˢ);
            }
        } else {
            hs.echContext.Value.echRejected = true;
        }
    }
    {
        var err = transcriptMsg(new serverHelloMsgжhandshakeMessage(hs.serverHello), new hash_HashᴠtranscriptHash(hs.transcript)); if (err != default!) {
            return err;
        }
    }
    c.Value.buffering = true;
    {
        var err = Ꮡhs.processServerHello(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.sendDummyChangeCipherSpec(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.establishHandshakeKeys(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.readServerParameters(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.readServerCertificate(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.readServerFinished(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.sendClientCertificate(); if (err != default!) {
            return err;
        }
    }
    {
        var err = hs.sendClientFinished(); if (err != default!) {
            return err;
        }
    }
    {
        var (_, err) = c.flush(); if (err != default!) {
            return err;
        }
    }
    if (hs.echContext != nil && (~hs.echContext).echRejected) {
        c.sendAlert(alertECHRequired);
        return new ECHRejectionErrorжerror(Ꮡ(new ECHRejectionError((~hs.echContext).retryConfigs)));
    }
    c.of(Conn.ᏑisHandshakeComplete).Store(true);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerSelectedTls13ˢ = "tls: server selected TLS 1.3 using the legacy version field"u8;
internal static readonly @string tlsServerSelectedAnˢ = "tls: server selected an invalid version after a HelloRetryRequest"u8;
internal static readonly @string tlsServerSentAnIncorrectˢ = "tls: server sent an incorrect legacy version"u8;
internal static readonly @string tlsServerSentAˢ = "tls: server sent a ServerHello extension forbidden in TLS 1.3"u8;
internal static readonly @string tlsServerDidNotEchoTheˢ = "tls: server did not echo the legacy session ID"u8;
internal static readonly @string tlsServerChangedCipherˢ = "tls: server changed cipher suite after a HelloRetryRequest"u8;

// checkServerHelloOrHRR does validity checks that apply to both ServerHello and
// HelloRetryRequest messages. It sets hs.suite.
[GoRecv] internal static error checkServerHelloOrHRR(this ref clientHandshakeStateTLS13 hs) {
    var c = hs.c;
    if ((~hs.serverHello).supportedVersion == 0) {
        c.sendAlert(alertMissingExtension);
        return errors.New(tlsServerSelectedTls13ˢ);
    }
    if ((~hs.serverHello).supportedVersion != VersionTLS13) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerSelectedAnˢ);
    }
    if ((~hs.serverHello).vers != VersionTLS12) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerSentAnIncorrectˢ);
    }
    if ((~hs.serverHello).ocspStapling || (~hs.serverHello).ticketSupported || (~hs.serverHello).extendedMasterSecret || (~hs.serverHello).secureRenegotiationSupported || len((~hs.serverHello).secureRenegotiation) != 0 || len((~hs.serverHello).alpnProtocol) != 0 || len((~hs.serverHello).scts) != 0) {
        c.sendAlert(alertUnsupportedExtension);
        return errors.New(tlsServerSentAˢ);
    }
    if (!bytes.Equal((~hs.hello).sessionId, (~hs.serverHello).sessionId)) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerDidNotEchoTheˢ);
    }
    if ((~hs.serverHello).compressionMethod != compressionNone) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerSelectedˢ);
    }
    var selectedSuite = mutualCipherSuiteTLS13((~hs.hello).cipherSuites, (~hs.serverHello).cipherSuite);
    if (hs.suite != nil && selectedSuite != hs.suite) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerChangedCipherˢ);
    }
    if (selectedSuite == nil) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerChoseAnˢ);
    }
    hs.suite = selectedSuite;
    c.Value.cipherSuite = hs.suite.Value.id;
    return default!;
}

// sendDummyChangeCipherSpec sends a ChangeCipherSpec record for compatibility
// with middleboxes that didn't implement TLS correctly. See RFC 8446, Appendix D.4.
[GoRecv] internal static error sendDummyChangeCipherSpec(this ref clientHandshakeStateTLS13 hs) {
    if ((~hs.c).quic != nil) {
        return default!;
    }
    if (hs.sentDummyCCS) {
        return default!;
    }
    hs.sentDummyCCS = true;
    return hs.c.writeChangeCipherRecord();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsMalformedEncryptedˢ = "tls: malformed encrypted client hello extension"u8;
internal static readonly @string hrrEchAcceptConfirmationˢ = "hrr ech accept confirmation"u8;
internal static readonly @string tlsUnexpectedEncryptedˢ2 = "tls: unexpected encrypted client hello extension in serverHello"u8;
internal static readonly @string tlsServerSentAnˢ = "tls: server sent an unnecessary HelloRetryRequest message"u8;
internal static readonly @string tlsReceivedMalformedKeyˢ = "tls: received malformed key_share extension"u8;
internal static readonly @string tlsServerSelectedˢ3 = "tls: server selected unsupported group"u8;
internal static readonly @string tlsServerSentAnˢ2 = "tls: server sent an unnecessary HelloRetryRequest key_share"u8;

// processHelloRetryRequest handles the HRR in hs.serverHello, modifies and
// resends hs.hello, and reads the new ServerHello into hs.serverHello.
[GoRecv] internal static error processHelloRetryRequest(this ref clientHandshakeStateTLS13 hs) {
    var c = hs.c;
    // The first ClientHello gets double-hashed into the transcript upon a
    // HelloRetryRequest. (The idea is that the server might offload transcript
    // storage to the client in the cookie.) See RFC 8446, Section 4.4.1.
    var chHash = hs.transcript.Sum(default!);
    hs.transcript.Reset();
    hs.transcript.Write(new byte[]{typeMessageHash, 0, 0, (uint8)len(chHash)}.slice());
    hs.transcript.Write(chHash);
    {
        var errΔ1 = transcriptMsg(new serverHelloMsgжhandshakeMessage(hs.serverHello), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    bool isInnerHello = default!;
    var hello = hs.hello;
    if (hs.echContext != nil){
        chHash = (~hs.echContext).innerTranscript.Sum(default!);
        (~hs.echContext).innerTranscript.Reset();
        (~hs.echContext).innerTranscript.Write(new byte[]{typeMessageHash, 0, 0, (uint8)len(chHash)}.slice());
        (~hs.echContext).innerTranscript.Write(chHash);
        if ((~hs.serverHello).encryptedClientHello != default!) {
            if (len((~hs.serverHello).encryptedClientHello) != 8) {
                hs.c.sendAlert(alertDecodeError);
                return errors.New(tlsMalformedEncryptedˢ);
            }
            var confTranscript = cloneHash((~hs.echContext).innerTranscript, (~hs.suite).hash);
            var hrrHello = new slice<byte>(len((~hs.serverHello).original));
            copy(hrrHello, (~hs.serverHello).original);
            hrrHello = bytes.Replace(hrrHello, (~hs.serverHello).encryptedClientHello, new slice<byte>(8), 1);
            confTranscript.Write(hrrHello);
            var recvʗ1 = (~hs.suite).hash;

                var recvʗ2 = (~hs.suite).hash;
            var acceptConfirmation = tls13.ExpandLabel<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => recvʗ1.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)),
                hkdf.Extract<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => recvʗ2.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), (~(~hs.echContext).innerHello).random, default!),
                hrrEchAcceptConfirmationˢ,
                confTranscript.Sum(default!),
                8);
            if (subtle.ConstantTimeCompare(acceptConfirmation, (~hs.serverHello).encryptedClientHello) == 1) {
                hello = hs.echContext.Value.innerHello;
                c.Value.serverName = c.Value.config.Value.ServerName;
                isInnerHello = true;
                c.Value.echAccepted = true;
            }
        }
        {
            var errΔ2 = transcriptMsg(new serverHelloMsgжhandshakeMessage(hs.serverHello), new hash_HashᴠtranscriptHash((~hs.echContext).innerTranscript)); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    } else 
    if ((~hs.serverHello).encryptedClientHello != default!) {
        // Unsolicited ECH extension should be rejected
        c.sendAlert(alertUnsupportedExtension);
        return errors.New(tlsUnexpectedEncryptedˢ2);
    }
    // The only HelloRetryRequest extensions we support are key_share and
    // cookie, and clients must abort the handshake if the HRR would not result
    // in any change in the ClientHello.
    if ((~hs.serverHello).selectedGroup == 0 && (~hs.serverHello).cookie == default!) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerSentAnˢ);
    }
    if ((~hs.serverHello).cookie != default!) {
        hello.Value.cookie = hs.serverHello.Value.cookie;
    }
    if ((~hs.serverHello).serverShare.group != 0) {
        c.sendAlert(alertDecodeError);
        return errors.New(tlsReceivedMalformedKeyˢ);
    }
    // If the server sent a key_share extension selecting a group, ensure it's
    // a group we advertised but did not send a key share for, and send a key
    // share for it this time.
    {
        ref var curveID = ref heap<CurveID>(out var ᏑcurveID);
        curveID = hs.serverHello.Value.selectedGroup; if (curveID != 0) {
            if (!slices.Contains((~hello).supportedCurves, curveID)) {
                c.sendAlert(alertIllegalParameter);
                return errors.New(tlsServerSelectedˢ3);
            }
            var curveIDʗ1 = curveID;
            if (slices.ContainsFunc((~hs.hello).keyShares, (keyShare ks) => ks.group == curveIDʗ1)) {
                c.sendAlert(alertIllegalParameter);
                return errors.New(tlsServerSentAnˢ2);
            }
            // Note: we don't support selecting X25519MLKEM768 in a HRR, because it
            // is currently first in preference order, so if it's enabled we'll
            // always send a key share for it.
            //
            // This will have to change once we support multiple hybrid KEMs.
            {
                var (_, okΔ1) = curveForCurveID(curveID); if (!okΔ1) {
                    c.sendAlert(alertInternalError);
                    return errors.New(tlsCurvePreferencesˢ);
                }
            }
            var (key, errΔ3) = generateECDHEKey((~c).config.rand(), curveID);
            if (errΔ3 != default!) {
                c.sendAlert(alertInternalError);
                return errΔ3;
            }
            hs.keyShareKeys = Ꮡ(new keySharePrivateKeys(curveID: curveID, ecdhe: key));
            hello.Value.keyShares = new keyShare[]{new(group: curveID, data: key.PublicKey().Bytes())}.slice();
        }
    }
    if (len((~hello).pskIdentities) > 0) {
        var pskSuite = cipherSuiteTLS13ByID((~hs.session).cipherSuite);
        if (pskSuite == nil) {
            return c.sendAlert(alertInternalError);
        }
        if ((~pskSuite).hash == (~hs.suite).hash){
            // Update binders and obfuscated_ticket_age.
            var ticketAge = (~c).config.time().Sub(time_package.Unix((int64)(~hs.session).createdAt, 0));
            (~hello).pskIdentities[0].obfuscatedTicketAge = (uint32)(int64)(ticketAge / time_package.Millisecond) + (~hs.session).ageAdd;
            var transcript = (~hs.suite).hash.New();
            transcript.Write(new byte[]{typeMessageHash, 0, 0, (uint8)len(chHash)}.slice());
            transcript.Write(chHash);
            {
                var errΔ4 = transcriptMsg(new serverHelloMsgжhandshakeMessage(hs.serverHello), new hash_HashᴠtranscriptHash(transcript)); if (errΔ4 != default!) {
                    return errΔ4;
                }
            }
            {
                var errΔ5 = computeAndUpdatePSK(hello, hs.binderKey, transcript, hs.suite.finishedHash); if (errΔ5 != default!) {
                    return errΔ5;
                }
            }
        } else {
            // Server selected a cipher suite incompatible with the PSK.
            hello.Value.pskIdentities = default!;
            hello.Value.pskBinders = default!;
        }
    }
    if ((~hello).earlyData) {
        hello.Value.earlyData = false;
        c.quicRejectedEarlyData();
    }
    if (isInnerHello){
        // Any extensions which have changed in hello, but are mirrored in the
        // outer hello and compressed, need to be copied to the outer hello, so
        // they can be properly decompressed by the server. For now, the only
        // extension which may have changed is keyShares.
        hs.hello.Value.keyShares = hello.Value.keyShares;
        hs.echContext.Value.innerHello = hello;
        {
            var errΔ6 = transcriptMsg(new clientHelloMsgжhandshakeMessage((~hs.echContext).innerHello), new hash_HashᴠtranscriptHash((~hs.echContext).innerTranscript)); if (errΔ6 != default!) {
                return errΔ6;
            }
        }
        {
            var errΔ7 = computeAndUpdateOuterECHExtension(hs.hello, (~hs.echContext).innerHello, ref (hs.echContext).DerefOrNull(), false); if (errΔ7 != default!) {
                return errΔ7;
            }
        }
    } else {
        hs.hello = hello;
    }
    {
        var (_, errΔ8) = hs.c.writeHandshakeRecord(new clientHelloMsgжhandshakeMessage(hs.hello), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ8 != default!) {
            return errΔ8;
        }
    }
    // serverHelloMsg is not included in the transcript
    var (msg, err) = c.readHandshake(default!);
    if (err != default!) {
        return err;
    }
    var (serverHello, ok) = msg._<ж<serverHelloMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(serverHello.OrTypedNil(), msg);
    }
    hs.serverHello = serverHello;
    {
        var errΔ9 = hs.checkServerHelloOrHRR(); if (errΔ9 != default!) {
            return errΔ9;
        }
    }
    c.Value.didHRR = true;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerSentTwoˢ = "tls: server sent two HelloRetryRequest messages"u8;
internal static readonly @string tlsServerSentACookieInAˢ = "tls: server sent a cookie in a normal ServerHello"u8;
internal static readonly @string tlsMalformedKeyShareˢ = "tls: malformed key_share extension"u8;
internal static readonly @string tlsServerDidNotSendAKeyˢ = "tls: server did not send a key share"u8;
internal static readonly @string tlsServerSelectedAnˢ2 = "tls: server selected an invalid PSK"u8;
internal static readonly @string tlsServerSelectedAnˢ3 = "tls: server selected an invalid PSK and cipher suite pair"u8;

internal static error processServerHello(this ж<clientHandshakeStateTLS13> Ꮡhs) {
    ref var hs = ref Ꮡhs.DerefOrNull();

    var c = hs.c;
    if (bytes.Equal((~hs.serverHello).random, helloRetryRequestRandom)) {
        c.sendAlert(alertUnexpectedMessage);
        return errors.New(tlsServerSentTwoˢ);
    }
    if (len((~hs.serverHello).cookie) != 0) {
        c.sendAlert(alertUnsupportedExtension);
        return errors.New(tlsServerSentACookieInAˢ);
    }
    if ((~hs.serverHello).selectedGroup != 0) {
        c.sendAlert(alertDecodeError);
        return errors.New(tlsMalformedKeyShareˢ);
    }
    if ((~hs.serverHello).serverShare.group == 0) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerDidNotSendAKeyˢ);
    }
    if (!slices.ContainsFunc((~hs.hello).keyShares, (keyShare ks) => ks.group == (~Ꮡhs.Value.serverHello).serverShare.group)) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerSelectedˢ3);
    }
    if (!(~hs.serverHello).selectedIdentityPresent) {
        return default!;
    }
    if ((nint)(~hs.serverHello).selectedIdentity >= len((~hs.hello).pskIdentities)) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerSelectedAnˢ2);
    }
    if (len((~hs.hello).pskIdentities) != 1 || hs.session == nil) {
        return c.sendAlert(alertInternalError);
    }
    var pskSuite = cipherSuiteTLS13ByID((~hs.session).cipherSuite);
    if (pskSuite == nil) {
        return c.sendAlert(alertInternalError);
    }
    if ((~pskSuite).hash != (~hs.suite).hash) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsServerSelectedAnˢ3);
    }
    hs.usingPSK = true;
    c.Value.didResume = true;
    c.Value.peerCertificates = hs.session.Value.peerCertificates;
    c.Value.activeCertHandles = hs.session.Value.activeCertHandles;
    c.Value.verifiedChains = hs.session.Value.verifiedChains;
    c.Value.ocspResponse = hs.session.Value.ocspResponse;
    c.Value.scts = hs.session.Value.scts;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInvalidServerˢ = "tls: invalid server X25519MLKEM768 key share"u8;
internal static readonly @string tlsInvalidServerKeyShareˢ = "tls: invalid server key share"u8;
internal static readonly @string tlsInvalidX25519mlkem768ˢ = "tls: invalid X25519MLKEM768 server key share"u8;

[GoRecv] internal static error establishHandshakeKeys(this ref clientHandshakeStateTLS13 hs) {
    var c = hs.c;
    var ecdhePeerData = hs.serverHello.Value.serverShare.data;
    if ((~hs.serverHello).serverShare.group == X25519MLKEM768) {
        if (len(ecdhePeerData) != (nint)(mlkem.CiphertextSize768 + x25519PublicKeySize)) {
            c.sendAlert(alertIllegalParameter);
            return errors.New(tlsInvalidServerˢ);
        }
        ecdhePeerData = (~hs.serverHello).serverShare.data[(int)(mlkem.CiphertextSize768)..];
    }
    var (peerKey, err) = (~hs.keyShareKeys).ecdhe.Curve().NewPublicKey(ecdhePeerData);
    if (err != default!) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsInvalidServerKeyShareˢ);
    }
    (var sharedKey, err) = (~hs.keyShareKeys).ecdhe.ECDH(peerKey);
    if (err != default!) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsInvalidServerKeyShareˢ);
    }
    if ((~hs.serverHello).serverShare.group == X25519MLKEM768) {
        if ((~hs.keyShareKeys).mlkem == nil) {
            return c.sendAlert(alertInternalError);
        }
        var ciphertext = (~hs.serverHello).serverShare.data[..(int)(mlkem.CiphertextSize768)];
        var (mlkemShared, errΔ1) = (~hs.keyShareKeys).mlkem.Decapsulate(ciphertext);
        if (errΔ1 != default!) {
            c.sendAlert(alertIllegalParameter);
            return errors.New(tlsInvalidX25519mlkem768ˢ);
        }
        sharedKey = appendꓸꓸꓸ(mlkemShared, sharedKey);
    }
    c.Value.curveID = hs.serverHello.Value.serverShare.group;
    var earlySecret = hs.earlySecret;
    if (!hs.usingPSK) {
        var recvʗ1 = (~hs.suite).hash;
        earlySecret = tls13.NewEarlySecret<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => recvʗ1.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), default!);
    }
    var handshakeSecret = earlySecret.HandshakeSecret(sharedKey);
    var clientSecret = handshakeSecret.ClientHandshakeTrafficSecret(new hash_HashᴠHash(hs.transcript));
    c.setWriteTrafficSecret(hs.suite, QUICEncryptionLevelHandshake, clientSecret);
    var serverSecret = handshakeSecret.ServerHandshakeTrafficSecret(new hash_HashᴠHash(hs.transcript));
    {
        var errΔ2 = c.setReadTrafficSecret(hs.suite, QUICEncryptionLevelHandshake, serverSecret); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    if ((~c).quic != nil) {
        c.quicSetWriteSecret(QUICEncryptionLevelHandshake, (~hs.suite).id, clientSecret);
        {
            var errΔ3 = c.quicSetReadSecret(QUICEncryptionLevelHandshake, (~hs.suite).id, serverSecret); if (errΔ3 != default!) {
                return errΔ3;
            }
        }
    }
    err = (~c).config.writeKeyLog(keyLogLabelClientHandshake, (~hs.hello).random, clientSecret);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    err = (~c).config.writeKeyLog(keyLogLabelServerHandshake, (~hs.hello).random, serverSecret);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    hs.masterSecret = handshakeSecret.MasterSecret();
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerDidNotSendAQuicˢ = "tls: server did not send a quic_transport_parameters extension"u8;
internal static readonly @string tlsServerSentAnˢ3 = "tls: server sent an unexpected quic_transport_parameters extension"u8;
internal static readonly @string tlsServerSentAnˢ4 = "tls: server sent an unexpected early_data extension"u8;
internal static readonly @string tlsServerAccepted0Rttˢ = "tls: server accepted 0-RTT with the wrong cipher suite"u8;
internal static readonly @string tlsServerAccepted0Rttˢ2 = "tls: server accepted 0-RTT with the wrong ALPN"u8;
internal static readonly @string tlsServerSentEncryptedˢ = "tls: server sent encrypted client hello retry configs after accepting encrypted client hello"u8;

[GoRecv] internal static error readServerParameters(this ref clientHandshakeStateTLS13 hs) {
    var c = hs.c;
    var (msg, err) = c.readHandshake(new hash_HashᴠtranscriptHash(hs.transcript));
    if (err != default!) {
        return err;
    }
    var (encryptedExtensions, ok) = msg._<ж<encryptedExtensionsMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(encryptedExtensions.OrTypedNil(), msg);
    }
    {
        var errΔ1 = checkALPN((~hs.hello).alpnProtocols, (~encryptedExtensions).alpnProtocol, (~c).quic != nil); if (errΔ1 != default!) {
            // RFC 8446 specifies that no_application_protocol is sent by servers, but
            // does not specify how clients handle the selection of an incompatible protocol.
            // RFC 9001 Section 8.1 specifies that QUIC clients send no_application_protocol
            // in this case. Always sending no_application_protocol seems reasonable.
            c.sendAlert(alertNoApplicationProtocol);
            return errΔ1;
        }
    }
    c.Value.clientProtocol = encryptedExtensions.Value.alpnProtocol;
    if ((~c).quic != nil){
        if ((~encryptedExtensions).quicTransportParameters == default!) {
            // RFC 9001 Section 8.2.
            c.sendAlert(alertMissingExtension);
            return errors.New(tlsServerDidNotSendAQuicˢ);
        }
        c.quicSetTransportParameters((~encryptedExtensions).quicTransportParameters);
    } else {
        if ((~encryptedExtensions).quicTransportParameters != default!) {
            c.sendAlert(alertUnsupportedExtension);
            return errors.New(tlsServerSentAnˢ3);
        }
    }
    if (!(~hs.hello).earlyData && (~encryptedExtensions).earlyData) {
        c.sendAlert(alertUnsupportedExtension);
        return errors.New(tlsServerSentAnˢ4);
    }
    if ((~hs.hello).earlyData && !(~encryptedExtensions).earlyData) {
        c.quicRejectedEarlyData();
    }
    if ((~encryptedExtensions).earlyData) {
        if ((~hs.session).cipherSuite != (~c).cipherSuite) {
            c.sendAlert(alertHandshakeFailure);
            return errors.New(tlsServerAccepted0Rttˢ);
        }
        if ((~hs.session).alpnProtocol != (~c).clientProtocol) {
            c.sendAlert(alertHandshakeFailure);
            return errors.New(tlsServerAccepted0Rttˢ2);
        }
    }
    if (hs.echContext != nil) {
        if ((~hs.echContext).echRejected){
            hs.echContext.Value.retryConfigs = encryptedExtensions.Value.echRetryConfigs;
        } else 
        if ((~encryptedExtensions).echRetryConfigs != default!) {
            c.sendAlert(alertUnsupportedExtension);
            return errors.New(tlsServerSentEncryptedˢ);
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsReceivedEmptyˢ = "tls: received empty certificates message"u8;
internal static readonly @string tlsCertificateUsedWithˢ = "tls: certificate used with invalid signature algorithm"u8;

[GoRecv] internal static error readServerCertificate(this ref clientHandshakeStateTLS13 hs) {
    var c = hs.c;
    // Either a PSK or a certificate is always used, but not both.
    // See RFC 8446, Section 4.1.1.
    if (hs.usingPSK) {
        // Make sure the connection is still being verified whether or not this
        // is a resumption. Resumptions currently don't reverify certificates so
        // they don't call verifyServerCertificate. See Issue 31641.
        if ((~(~c).config).VerifyConnection != default!) {
            {
                var errΔ1 = (~(~c).config).VerifyConnection(c.connectionStateLocked()); if (errΔ1 != default!) {
                    c.sendAlert(alertBadCertificate);
                    return errΔ1;
                }
            }
        }
        return default!;
    }
    var (msg, err) = c.readHandshake(new hash_HashᴠtranscriptHash(hs.transcript));
    if (err != default!) {
        return err;
    }
    var (certReq, ok) = msg._<ж<certificateRequestMsgTLS13>>(ᐧ);
    if (ok) {
        hs.certReq = certReq;
        (msg, err) = c.readHandshake(new hash_HashᴠtranscriptHash(hs.transcript));
        if (err != default!) {
            return err;
        }
    }
    (var certMsg, ok) = msg._<ж<certificateMsgTLS13>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(certMsg.OrTypedNil(), msg);
    }
    if (len((~certMsg).certificate.ΔCertificate) == 0) {
        c.sendAlert(alertDecodeError);
        return errors.New(tlsReceivedEmptyˢ);
    }
    c.Value.scts = certMsg.Value.certificate.SignedCertificateTimestamps;
    c.Value.ocspResponse = certMsg.Value.certificate.OCSPStaple;
    {
        var errΔ2 = c.verifyServerCertificate((~certMsg).certificate.ΔCertificate); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    // certificateVerifyMsg is included in the transcript, but not until
    // after we verify the handshake signature, since the state before
    // this message was sent is used.
    (msg, err) = c.readHandshake(default!);
    if (err != default!) {
        return err;
    }
    (var certVerify, ok) = msg._<ж<certificateVerifyMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(certVerify.OrTypedNil(), msg);
    }
    // See RFC 8446, Section 4.4.3.
    if (!isSupportedSignatureAlgorithm((~certVerify).signatureAlgorithm, supportedSignatureAlgorithms())) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsCertificateUsedWithˢ);
    }
    (var sigType, var sigHash, err) = typeAndHashFromSignatureScheme((~certVerify).signatureAlgorithm);
    if (err != default!) {
        return c.sendAlert(alertInternalError);
    }
    if (sigType == signaturePKCS1v15 || sigHash == crypto.SHA1) {
        c.sendAlert(alertIllegalParameter);
        return errors.New(tlsCertificateUsedWithˢ);
    }
    var signed = signedMessage(sigHash, serverSignatureContext, hs.transcript);
    {
        var errΔ3 = verifyHandshakeSignature(sigType, (~(~c).peerCertificates[0]).PublicKey,
            sigHash, signed, (~certVerify).signature); if (errΔ3 != default!) {
            c.sendAlert(alertDecryptError);
            return errors.New("tls: invalid signature by the server certificate: "u8 + errΔ3.Error());
        }
    }
    {
        var errΔ4 = transcriptMsg(new certificateVerifyMsgжhandshakeMessage(certVerify), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ4 != default!) {
            return errΔ4;
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInvalidServerFinishedˢ = "tls: invalid server finished hash"u8;

[GoRecv] internal static error readServerFinished(this ref clientHandshakeStateTLS13 hs) {
    var c = hs.c;
    // finishedMsg is included in the transcript, but not until after we
    // check the client version, since the state before this message was
    // sent is used during verification.
    var (msg, err) = c.readHandshake(default!);
    if (err != default!) {
        return err;
    }
    var (finished, ok) = msg._<ж<finishedMsg>>(ᐧ);
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return unexpectedMessageError(finished.OrTypedNil(), msg);
    }
    var expectedMAC = hs.suite.finishedHash((~c).@in.trafficSecret, hs.transcript);
    if (!hmac.Equal(expectedMAC, (~finished).verifyData)) {
        c.sendAlert(alertDecryptError);
        return errors.New(tlsInvalidServerFinishedˢ);
    }
    {
        var errΔ1 = transcriptMsg(new finishedMsgжhandshakeMessage(finished), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    // Derive secrets that take context through the server Finished.
    hs.trafficSecret = hs.masterSecret.ClientApplicationTrafficSecret(new hash_HashᴠHash(hs.transcript));
    var serverSecret = hs.masterSecret.ServerApplicationTrafficSecret(new hash_HashᴠHash(hs.transcript));
    {
        var errΔ2 = c.setReadTrafficSecret(hs.suite, QUICEncryptionLevelApplication, serverSecret); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    err = (~c).config.writeKeyLog(keyLogLabelClientTraffic, (~hs.hello).random, hs.trafficSecret);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    err = (~c).config.writeKeyLog(keyLogLabelServerTraffic, (~hs.hello).random, serverSecret);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return err;
    }
    c.Value.ekm = hs.suite.exportKeyingMaterial(hs.masterSecret, hs.transcript);
    return default!;
}

[GoRecv] internal static error sendClientCertificate(this ref clientHandshakeStateTLS13 hs) {
    var c = hs.c;
    if (hs.certReq == nil) {
        return default!;
    }
    if (hs.echContext != nil && (~hs.echContext).echRejected) {
        {
            var (_, errΔ1) = hs.c.writeHandshakeRecord(new certificateMsgTLS13жhandshakeMessage(Ꮡ(new certificateMsgTLS13(nil))), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        return default!;
    }
    var (cert, err) = c.getClientCertificate(Ꮡ(new CertificateRequestInfo(
        AcceptableCAs: (~hs.certReq).certificateAuthorities,
        SignatureSchemes: (~hs.certReq).supportedSignatureAlgorithms,
        Version: (~c).vers,
        ctx: hs.ctx
    )));
    if (err != default!) {
        return err;
    }
    var certMsg = @new<certificateMsgTLS13>();
    certMsg.Value.certificate = cert.Value;
    certMsg.Value.scts = (~hs.certReq).scts && len((~cert).SignedCertificateTimestamps) > 0;
    certMsg.Value.ocspStapling = (~hs.certReq).ocspStapling && len((~cert).OCSPStaple) > 0;
    {
        var (_, errΔ2) = hs.c.writeHandshakeRecord(new certificateMsgTLS13жhandshakeMessage(certMsg), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    // If we sent an empty certificate message, skip the CertificateVerify.
    if (len((~cert).ΔCertificate) == 0) {
        return default!;
    }
    var certVerifyMsg = @new<certificateVerifyMsg>();
    certVerifyMsg.Value.hasSignatureAlgorithm = true;
    (certVerifyMsg.Value.signatureAlgorithm, err) = selectSignatureScheme((~c).vers, ref (cert).DerefOrNull(), (~hs.certReq).supportedSignatureAlgorithms);
    if (err != default!) {
        // getClientCertificate returned a certificate incompatible with the
        // CertificateRequestInfo supported signature algorithms.
        c.sendAlert(alertHandshakeFailure);
        return err;
    }
    ref var sigHash = ref heap<crypto.Hash>(out var ᏑsigHash);
    (var sigType, sigHash, err) = typeAndHashFromSignatureScheme((~certVerifyMsg).signatureAlgorithm);
    if (err != default!) {
        return c.sendAlert(alertInternalError);
    }
    var signed = signedMessage(sigHash, clientSignatureContext, hs.transcript);
    var signOpts = ((crypto.SignerOpts)sigHash);
    if (sigType == signatureRSAPSS) {
        signOpts = new rsa_PSSOptionsжSignerOpts(Ꮡ(new rsa.PSSOptions(SaltLength: rsa.PSSSaltLengthEqualsHash, Hash: sigHash)));
    }
    (var sig, err) = (~cert).PrivateKey._<crypto.Signer>().Sign((~c).config.rand(), signed, signOpts);
    if (err != default!) {
        c.sendAlert(alertInternalError);
        return errors.New("tls: failed to sign handshake: "u8 + err.Error());
    }
    certVerifyMsg.Value.signature = sig;
    {
        var (_, errΔ3) = hs.c.writeHandshakeRecord(new certificateVerifyMsgжhandshakeMessage(certVerifyMsg), new hash_HashᴠtranscriptHash(hs.transcript)); if (errΔ3 != default!) {
            return errΔ3;
        }
    }
    return default!;
}

[GoRecv] internal static error sendClientFinished(this ref clientHandshakeStateTLS13 hs) {
    var c = hs.c;
    var finished = Ꮡ(new finishedMsg(
        verifyData: hs.suite.finishedHash((~c).@out.trafficSecret, hs.transcript)
    ));
    {
        var (_, err) = hs.c.writeHandshakeRecord(new finishedMsgжhandshakeMessage(finished), new hash_HashᴠtranscriptHash(hs.transcript)); if (err != default!) {
            return err;
        }
    }
    c.setWriteTrafficSecret(hs.suite, QUICEncryptionLevelApplication, hs.trafficSecret);
    if (!(~(~c).config).SessionTicketsDisabled && (~(~c).config).ClientSessionCache != default!) {
        c.Value.resumptionSecret = hs.masterSecret.ResumptionMasterSecret(new hash_HashᴠHash(hs.transcript));
    }
    if ((~c).quic != nil) {
        c.quicSetWriteSecret(QUICEncryptionLevelApplication, (~hs.suite).id, hs.trafficSecret);
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsReceivedNewSessionˢ = "tls: received new session ticket from a client"u8;
internal static readonly @string tlsReceivedASessionˢ = "tls: received a session ticket with invalid lifetime"u8;
internal static readonly @string tlsInvalidEarlyDataForˢ = "tls: invalid early data for QUIC connection"u8;
internal static readonly @string resumptionˢ = "resumption"u8;

internal static error handleNewSessionTicket(this ж<Conn> Ꮡc, ж<newSessionTicketMsgTLS13> Ꮡmsg) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var msg = ref Ꮡmsg.DerefOrNull();

    if (!c.isClient) {
        Ꮡc.sendAlert(alertUnexpectedMessage);
        return errors.New(tlsReceivedNewSessionˢ);
    }
    if ((~c.config).SessionTicketsDisabled || (~c.config).ClientSessionCache == default!) {
        return default!;
    }
    // See RFC 8446, Section 4.6.1.
    if (msg.lifetime == 0) {
        return default!;
    }
    var lifetime = ((time.Duration)(int64)msg.lifetime) * time_package.ΔSecond;
    if (lifetime > maxSessionTicketLifetime) {
        Ꮡc.sendAlert(alertIllegalParameter);
        return errors.New(tlsReceivedASessionˢ);
    }
    // RFC 9001, Section 4.6.1
    if (c.quic != nil && msg.maxEarlyData != 0 && msg.maxEarlyData != 0xffffffffU) {
        Ꮡc.sendAlert(alertIllegalParameter);
        return errors.New(tlsInvalidEarlyDataForˢ);
    }
    var cipherSuite = cipherSuiteTLS13ByID(c.cipherSuite);
    if (cipherSuite == nil || c.resumptionSecret == default!) {
        return Ꮡc.sendAlert(alertInternalError);
    }
    var psk = tls13.ExpandLabel<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => (~cipherSuite).hash.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), c.resumptionSecret, resumptionˢ,
        msg.nonce, (~cipherSuite).hash.Size());
    var session = c.sessionState();
    session.Value.secret = psk;
    session.Value.useBy = (uint64)c.config.time().Add(lifetime).Unix();
    session.Value.ageAdd = msg.ageAdd;
    session.Value.EarlyData = c.quic != nil && msg.maxEarlyData == 0xffffffffU; // RFC 9001, Section 4.6.1
    session.Value.ticket = msg.label;
    if (c.quic != nil && (~c.quic).enableSessionEvents) {
        c.quicStoreSession(session);
        return default!;
    }
    var cs = Ꮡ(new ClientSessionState(session: session));
    {
        @string cacheKey = c.clientSessionCacheKey(); if (cacheKey != ""u8) {
            (~c.config).ClientSessionCache.Put(cacheKey, cs);
        }
    }
    return default!;
}

} // end tls_package
