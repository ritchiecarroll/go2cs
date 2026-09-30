// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// TLS low level connection and record layer
namespace go.crypto;

using bytes = bytes_package;
using context = context_package;
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.subtle_package;
using Δx509 = go.crypto.x509_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using godebug = go.@internal.godebug_package;
using io = io_package;
using net = net_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using go.@internal;
using go.crypto;
using go.sync;

partial class tls_package {

// A Conn represents a secured connection.
// It implements the net.Conn interface.
[GoType] partial struct Conn {
    // constant
    internal net.Conn conn;
    internal bool isClient;
    internal Func<context.Context, error> handshakeFn; // (*Conn).clientHandshake or serverHandshake
    internal ж<quicState> quic;               // nil for non-QUIC connections
    // isHandshakeComplete is true if the connection is currently transferring
    // application data (i.e. is not currently processing a handshake).
    // isHandshakeComplete is true implies handshakeErr == nil.
    internal atomic.Bool isHandshakeComplete;
    // constant after handshake; protected by handshakeMutex
    internal sync.Mutex handshakeMutex;
    internal error handshakeErr;   // error resulting from handshake
    internal uint16 vers;  // TLS version
    internal bool haveVers;    // version has been negotiated
    internal ж<Config> config; // configuration passed to constructor
    // handshakes counts the number of handshakes performed on the
    // connection so far. If renegotiation is disabled then this is either
    // zero or one.
    internal nint handshakes;
    internal bool extMasterSecret;
    internal bool didResume; // whether this connection was a session resumption
    internal bool didHRR; // whether a HelloRetryRequest was sent/received
    internal uint16 cipherSuite;
    internal CurveID curveID;
    internal slice<byte> ocspResponse; // stapled OCSP response
    internal slice<slice<byte>> scts; // signed certificate timestamps from server
    internal slice<ж<Δx509.Certificate>> peerCertificates;
    // activeCertHandles contains the cache handles to certificates in
    // peerCertificates that are used to track active references.
    internal slice<ж<activeCert>> activeCertHandles;
    // verifiedChains contains the certificate chains that we built, as
    // opposed to the ones presented by the server.
    internal slice<slice<ж<Δx509.Certificate>>> verifiedChains;
    // serverName contains the server name indicated by the client, if any.
    internal @string serverName;
    // secureRenegotiation is true if the server echoed the secure
    // renegotiation extension. (This is meaningless as a server because
    // renegotiation is not supported in that case.)
    internal bool secureRenegotiation;
    // ekm is a closure for exporting keying material.
    internal Func<@string, slice<byte>, nint, (slice<byte>, error)> ekm;
    // resumptionSecret is the resumption_master_secret for handling
    // or sending NewSessionTicket messages.
    internal slice<byte> resumptionSecret;
    internal bool echAccepted;
    // ticketKeys is the set of active session ticket keys for this
    // connection. The first one is used to encrypt new tickets and
    // all are tried to decrypt tickets.
    internal slice<ticketKey> ticketKeys;
    // clientFinishedIsFirst is true if the client sent the first Finished
    // message during the most recent handshake. This is recorded because
    // the first transmitted Finished message is the tls-unique
    // channel-binding value.
    internal bool clientFinishedIsFirst;
    // closeNotifyErr is any error from sending the alertCloseNotify record.
    internal error closeNotifyErr;
    // closeNotifySent is true if the Conn attempted to send an
    // alertCloseNotify record.
    internal bool closeNotifySent;
    // clientFinished and serverFinished contain the Finished message sent
    // by the client or server in the most recent handshake. This is
    // retained to support the renegotiation extension and tls-unique
    // channel-binding.
    internal array<byte> clientFinished = new(12);
    internal array<byte> serverFinished = new(12);
    // clientProtocol is the negotiated ALPN protocol.
    internal @string clientProtocol;
    // input/output
    internal halfConn @in, @out;
    internal bytes.Buffer rawInput; // raw input, starting with a record header
    internal bytes.Reader input; // application data waiting to be read, from rawInput.Next
    internal bytes.Buffer hand; // handshake data waiting to be read
    internal bool buffering;         // whether records are buffered in sendBuf
    internal slice<byte> sendBuf;  // a buffer of records waiting to be sent
    // bytesSent counts the bytes of application data sent.
    // packetsSent counts packets.
    internal int64 bytesSent;
    internal int64 packetsSent;
    // retryCount counts the number of consecutive non-advancing records
    // received by Conn.readRecord. That is, records that neither advance the
    // handshake, nor deliver application data. Protected by in.Mutex.
    internal nint retryCount;
    // activeCall indicates whether Close has been call in the low bit.
    // the rest of the bits are the number of goroutines in Conn.Write.
    internal atomic.Int32 activeCall;
    internal array<byte> tmp = new(16);
}

// Access to net.Conn methods.
// Cannot just embed net.Conn because that would
// export the struct field too.

// LocalAddr returns the local network address.
[GoRecv] public static netꓸAddr LocalAddr(this ref Conn c) {
    return c.conn.LocalAddr();
}

// RemoteAddr returns the remote network address.
[GoRecv] public static netꓸAddr RemoteAddr(this ref Conn c) {
    return c.conn.RemoteAddr();
}

// SetDeadline sets the read and write deadlines associated with the connection.
// A zero value for t means [Conn.Read] and [Conn.Write] will not time out.
// After a Write has timed out, the TLS state is corrupt and all future writes will return the same error.
[GoRecv] public static error SetDeadline(this ref Conn c, time.Time t) {
    return c.conn.SetDeadline(t);
}

// SetReadDeadline sets the read deadline on the underlying connection.
// A zero value for t means [Conn.Read] will not time out.
[GoRecv] public static error SetReadDeadline(this ref Conn c, time.Time t) {
    return c.conn.SetReadDeadline(t);
}

// SetWriteDeadline sets the write deadline on the underlying connection.
// A zero value for t means [Conn.Write] will not time out.
// After a [Conn.Write] has timed out, the TLS state is corrupt and all future writes will return the same error.
[GoRecv] public static error SetWriteDeadline(this ref Conn c, time.Time t) {
    return c.conn.SetWriteDeadline(t);
}

// NetConn returns the underlying connection that is wrapped by c.
// Note that writing to or reading from this connection directly will corrupt the
// TLS session.
[GoRecv] public static net.Conn NetConn(this ref Conn c) {
    return c.conn;
}

// A halfConn represents one direction of the record layer
// connection, either sending or receiving.
[GoType] partial struct halfConn {
    public partial ref sync_package.Mutex Mutex { get; }
    internal error err;  // first permanent error
    internal uint16 version; // protocol version
    internal any cipher;    // cipher algorithm
    internal hash.Hash mac;
    internal array<byte> seq = new(8); // 64-bit sequence number
    internal array<byte> scratchBuf = new(13); // to avoid allocs; interface method args escape
    internal any nextCipher;       // next encryption state
    internal hash.Hash nextMac; // next MAC algorithm
    internal QUICEncryptionLevel level; // current QUIC encryption level
    internal slice<byte> trafficSecret;         // current TLS 1.3 traffic secret
}

[GoType] partial struct permanentError {
    internal netꓸError err;
}

[GoRecv] internal static @string Error(this ref permanentError e) {
    return e.err.Error();
}

[GoRecv] internal static error Unwrap(this ref permanentError e) {
    return new net_ΔErrorᴠerror(e.err);
}

[GoRecv] internal static bool Timeout(this ref permanentError e) {
    return e.err.Timeout();
}

[GoRecv] internal static bool Temporary(this ref permanentError e) {
    return false;
}

[GoRecv] internal static error setErrorLocked(this ref halfConn hc, error err) {
    {
        var (e, ok) = err._<netꓸError>(ᐧ); if (ok){
            hc.err = new permanentErrorжerror(Ꮡ(new permanentError(err: e)));
        } else {
            hc.err = err;
        }
    }
    return hc.err;
}

// prepareCipherSpec sets the encryption and MAC states
// that a subsequent changeCipherSpec will use.
[GoRecv] internal static void prepareCipherSpec(this ref halfConn hc, uint16 version, any cipher, hash.Hash mac) {
    hc.version = version;
    hc.nextCipher = cipher;
    hc.nextMac = mac;
}

// changeCipherSpec changes the encryption and MAC states
// to the ones previously passed to prepareCipherSpec.
[GoRecv] internal static error changeCipherSpec(this ref halfConn hc) {
    if (hc.nextCipher == default! || hc.version == VersionTLS13) {
        return alertInternalError;
    }
    hc.cipher = hc.nextCipher;
    hc.mac = hc.nextMac;
    hc.nextCipher = default!;
    hc.nextMac = default!;
    foreach (var (i, _) in hc.seq) {
        hc.seq[i] = 0;
    }
    return default!;
}

// setTrafficSecret sets the traffic secret for the given encryption level. setTrafficSecret
// should not be called directly, but rather through the Conn setWriteTrafficSecret and
// setReadTrafficSecret wrapper methods.
[GoRecv] internal static void setTrafficSecret(this ref halfConn hc, ж<cipherSuiteTLS13> Ꮡsuite, QUICEncryptionLevel level, slice<byte> secret) {
    ref var suite = ref Ꮡsuite.DerefOrNull();

    hc.trafficSecret = secret;
    hc.level = level;
    var (key, iv) = Ꮡsuite.trafficKey(secret);
    hc.cipher = suite.aead(key, iv);
    foreach (var (i, _) in hc.seq) {
        hc.seq[i] = 0;
    }
}

// incSeq increments the sequence number.
[GoRecv] internal static void incSeq(this ref halfConn hc) {
    for (nint i = 7; i >= 0; i--) {
        hc.seq[i]++;
        if (hc.seq[i] != 0) {
            return;
        }
    }
    // Not allowed to let sequence number wrap.
    // Instead, must renegotiate before it does.
    // Not likely enough to bother.
    throw panic("TLS: sequence number wraparound");
}

// explicitNonceLen returns the number of bytes of explicit nonce or IV included
// in each record. Explicit nonces are present only in CBC modes after TLS 1.0
// and in certain AEAD modes in TLS 1.2.
[GoRecv] internal static nint explicitNonceLen(this ref halfConn hc) {
    if (hc.cipher == default!) {
        return 0;
    }
    switch (hc.cipher.type()) {
    case {} Δc when Δc._<cipher.Stream>(out var c): {
        return 0;
    }
    case {} Δc when Δc._<aead>(out var c): {
        return c.explicitNonceLen();
    }
    case {} Δc when Δc._<cbcMode>(out var c): {
        if (hc.version >= VersionTLS11) {
            // TLS 1.1 introduced a per-record explicit IV to fix the BEAST attack.
            return c.BlockSize();
        }
        return 0;
    }
    default: {
        var c = hc.cipher;
        throw panic("unknown cipher type");
        break;
    }}
}

// extractPadding returns, in constant time, the length of the padding to remove
// from the end of payload. It also returns a byte which is equal to 255 if the
// padding was valid and 0 otherwise. See RFC 2246, Section 6.2.3.2.
internal static (nint toRemove, byte good) extractPadding(slice<byte> payload) {
    nint toRemove = default!;
    byte good = default!;

    if (len(payload) < 1) {
        return (0, 0);
    }
    var paddingLen = payload[len(payload) - 1];
    nuint t = (nuint)(len(payload) - 1) - (nuint)paddingLen;
    // if len(payload) >= (paddingLen - 1) then the MSB of t is zero
    good = (byte)(((int32)(~t) >> (int)(31)));
    // The maximum possible padding length plus the actual length field
    nint toCheck = 256;
    // The length of the padded data is public, so we can use an if here
    if (toCheck > len(payload)) {
        toCheck = len(payload);
    }
    for (nint i = 0; i < toCheck; i++) {
        nuint tΔ1 = (nuint)paddingLen - (nuint)i;
        // if i <= paddingLen then the MSB of t is zero
        var mask = (byte)(((int32)(~tΔ1) >> (int)(31)));
        var b = payload[len(payload) - 1 - i];
        good &= unchecked((byte)~(byte)((byte)((byte)(mask & paddingLen) ^ (byte)(mask & b))));
    }
    // We AND together the bits of good and replicate the result across
    // all the bits.
    good &= (byte)((byte)(good << (int)(4)));
    good &= (byte)((byte)(good << (int)(2)));
    good &= (byte)((byte)(good << (int)(1)));
    good = (uint8)(((int8)good >> (int)(7)));
    // Zero the padding length on error. This ensures any unchecked bytes
    // are included in the MAC. Otherwise, an attacker that could
    // distinguish MAC failures from padding failures could mount an attack
    // similar to POODLE in SSL 3.0: given a good ciphertext that uses a
    // full block's worth of padding, replace the final block with another
    // block. If the MAC check passed but the padding check failed, the
    // last byte of that block decrypted to the block size.
    //
    // See also macAndPaddingGood logic below.
    paddingLen &= (byte)(good);
    toRemove = (nint)paddingLen + 1;
    return (toRemove, good);
}

internal static nint roundUp(nint a, nint b) {
    return a + rem((b - rem(a, b)), b);
}

// cbcMode is an interface for block ciphers using cipher block chaining.
[GoType] partial interface cbcMode :
    cipher.BlockMode
{
    void SetIV(slice<byte> _);
}

// decrypt authenticates and decrypts the record if protection is active at
// this stage. The returned plaintext might overlap with the input.
[GoRecv] internal static (slice<byte>, recordType, error) decrypt(this ref halfConn hc, slice<byte> record) {
    slice<byte> plaintext = default!;
    var typ = ((recordType)record[0]);
    var payload = record[(int)(recordHeaderLen)..];
    // In TLS 1.3, change_cipher_spec messages are to be ignored without being
    // decrypted. See RFC 8446, Appendix D.4.
    if (hc.version == VersionTLS13 && typ == recordTypeChangeCipherSpec) {
        return (payload, typ, default!);
    }
    var paddingGood = (byte)255;
    nint paddingLen = 0;
    nint explicitNonceLen = hc.explicitNonceLen();
    if (hc.cipher != default!){
        switch (hc.cipher.type()) {
        case {} Δc when Δc._<cipher.Stream>(out var c): {
            c.XORKeyStream(payload, payload);
            break;
        }
        case {} Δc when Δc._<aead>(out var c): {
            if (len(payload) < explicitNonceLen) {
                return (default!, 0, alertBadRecordMAC);
            }
            var nonce = payload.slice(0, explicitNonceLen);
            if (len(nonce) == 0) {
                nonce = hc.seq[..];
            }
            payload = payload.slice(explicitNonceLen);
            slice<byte> additionalData = default!;
            if (hc.version == VersionTLS13){
                additionalData = record[..(int)(recordHeaderLen)];
            } else {
                additionalData = appendꓸꓸꓸ(hc.scratchBuf[..0], hc.seq[..]);
                additionalData = appendꓸꓸꓸ(additionalData, record[..3]);
                nint n = len(payload) - c.Overhead();
                additionalData = append(additionalData, (byte)((n >> (int)(8))), (byte)n);
            }
            error err = default!;
            (plaintext, err) = c.Open(payload[..0], nonce, payload, additionalData);
            if (err != default!) {
                return (default!, 0, alertBadRecordMAC);
            }
            break;
        }
        case {} Δc when Δc._<cbcMode>(out var c): {
            nint blockSize = c.BlockSize();
            nint minPayload = explicitNonceLen + roundUp(hc.mac.Size() + 1, blockSize);
            if (len(payload) % blockSize != 0 || len(payload) < minPayload) {
                return (default!, 0, alertBadRecordMAC);
            }
            if (explicitNonceLen > 0) {
                c.SetIV(payload.slice(0, explicitNonceLen));
                payload = payload.slice(explicitNonceLen);
            }
            c.CryptBlocks(payload, payload);
            (paddingLen, paddingGood) = extractPadding(payload);
            break;
        }
        default: {
            var c = hc.cipher;
            throw panic("unknown cipher type");
            break;
        }}
        // In a limited attempt to protect against CBC padding oracles like
        // Lucky13, the data past paddingLen (which is secret) is passed to
        // the MAC function as extra data, to be fed into the HMAC after
        // computing the digest. This makes the MAC roughly constant time as
        // long as the digest computation is constant time and does not
        // affect the subsequent write, modulo cache effects.
        if (hc.version == VersionTLS13) {
            if (typ != recordTypeApplicationData) {
                return (default!, 0, alertUnexpectedMessage);
            }
            if (len(plaintext) > (nint)(maxPlaintext + 1)) {
                return (default!, 0, alertRecordOverflow);
            }
            // Remove padding and find the ContentType scanning from the end.
            for (nint i = len(plaintext) - 1; i >= 0; i--) {
                if (plaintext[i] != 0) {
                    typ = ((recordType)plaintext[i]);
                    plaintext = plaintext.slice(0, i);
                    break;
                }
                if (i == 0) {
                    return (default!, 0, alertUnexpectedMessage);
                }
            }
        }
    } else {
        plaintext = payload;
    }
    if (hc.mac != default!) {
        nint macSize = hc.mac.Size();
        if (len(payload) < macSize) {
            return (default!, 0, alertBadRecordMAC);
        }
        nint n = len(payload) - macSize - paddingLen;
        n = subtle.ConstantTimeSelect((nint)(((uint32)n >> (int)(31))), 0, n); // if n < 0 { n = 0 }
        record[3] = (byte)((n >> (int)(8)));
        record[4] = (byte)n;
        var remoteMAC = payload.slice(n, n + macSize);
        var localMAC = tls10MAC(hc.mac, hc.scratchBuf[..0], hc.seq[..], record[..(int)(recordHeaderLen)], payload.slice(0, n), payload.slice(n + macSize));
        // This is equivalent to checking the MACs and paddingGood
        // separately, but in constant-time to prevent distinguishing
        // padding failures from MAC failures. Depending on what value
        // of paddingLen was returned on bad padding, distinguishing
        // bad MAC from bad padding can lead to an attack.
        //
        // See also the logic at the end of extractPadding.
        nint macAndPaddingGood = (nint)(subtle.ConstantTimeCompare(localMAC, remoteMAC) & (nint)paddingGood);
        if (macAndPaddingGood != 1) {
            return (default!, 0, alertBadRecordMAC);
        }
        plaintext = payload.slice(0, n);
    }
    hc.incSeq();
    return (plaintext, typ, default!);
}

// sliceForAppend extends the input slice by n bytes. head is the full extended
// slice, while tail is the appended part. If the original slice has sufficient
// capacity no allocation is performed.
internal static (slice<byte> head, slice<byte> tail) sliceForAppend(slice<byte> @in, nint n) {
    slice<byte> head = default!;
    slice<byte> tail = default!;

    {
        nint total = len(@in) + n; if (cap(@in) >= total){
            head = @in.slice(0, total);
        } else {
            head = new slice<byte>(total);
            copy(head, @in);
        }
    }
    tail = head.slice(len(@in));
    return (head, tail);
}

// encrypt encrypts payload, adding the appropriate nonce and/or MAC, and
// appends it to record, which must already contain the record header.
[GoRecv] internal static (slice<byte>, error) encrypt(this ref halfConn hc, slice<byte> record, slice<byte> payload, io.Reader rand) {
    if (hc.cipher == default!) {
        return (appendꓸꓸꓸ(record, payload), default!);
    }
    slice<byte> explicitNonce = default!;
    {
        nint explicitNonceLen = hc.explicitNonceLen(); if (explicitNonceLen > 0) {
            (record, explicitNonce) = sliceForAppend(record, explicitNonceLen);
            {
                var (_, isCBC) = hc.cipher._<cbcMode>(ᐧ); if (!isCBC && explicitNonceLen < 16){
                    // The AES-GCM construction in TLS has an explicit nonce so that the
                    // nonce can be random. However, the nonce is only 8 bytes which is
                    // too small for a secure, random nonce. Therefore we use the
                    // sequence number as the nonce. The 3DES-CBC construction also has
                    // an 8 bytes nonce but its nonces must be unpredictable (see RFC
                    // 5246, Appendix F.3), forcing us to use randomness. That's not
                    // 3DES' biggest problem anyway because the birthday bound on block
                    // collision is reached first due to its similarly small block size
                    // (see the Sweet32 attack).
                    copy(explicitNonce, hc.seq[..]);
                } else {
                    {
                        var (_, err) = io.ReadFull(rand, explicitNonce); if (err != default!) {
                            return (default!, err);
                        }
                    }
                }
            }
        }
    }
    slice<byte> dst = default!;
    switch (hc.cipher.type()) {
    case {} Δc when Δc._<cipher.Stream>(out var c): {
        var mac = tls10MAC(hc.mac, hc.scratchBuf[..0], hc.seq[..], record[..(int)(recordHeaderLen)], payload, default!);
        (record, dst) = sliceForAppend(record, len(payload) + len(mac));
        c.XORKeyStream(dst.slice(0, len(payload)), payload);
        c.XORKeyStream(dst.slice(len(payload)), mac);
        break;
    }
    case {} Δc when Δc._<aead>(out var c): {
        var nonce = explicitNonce;
        if (len(nonce) == 0) {
            nonce = hc.seq[..];
        }
        if (hc.version == VersionTLS13){
            record = appendꓸꓸꓸ(record, payload);
            // Encrypt the actual ContentType and replace the plaintext one.
            record = append(record, record[0]);
            record[0] = (byte)recordTypeApplicationData;
            nint nΔ1 = len(payload) + 1 + c.Overhead();
            record[3] = (byte)((nΔ1 >> (int)(8)));
            record[4] = (byte)nΔ1;
            record = c.Seal(record[..(int)(recordHeaderLen)],
                nonce, record[(int)(recordHeaderLen)..], record[..(int)(recordHeaderLen)]);
        } else {
            var additionalData = appendꓸꓸꓸ(hc.scratchBuf[..0], hc.seq[..]);
            additionalData = appendꓸꓸꓸ(additionalData, record[..(int)(recordHeaderLen)]);
            record = c.Seal(record, nonce, payload, additionalData);
        }
        break;
    }
    case {} Δc when Δc._<cbcMode>(out var c): {
        var mac = tls10MAC(hc.mac, hc.scratchBuf[..0], hc.seq[..], record[..(int)(recordHeaderLen)], payload, default!);
        nint blockSize = c.BlockSize();
        nint plaintextLen = len(payload) + len(mac);
        nint paddingLen = blockSize - rem(plaintextLen, blockSize);
        (record, dst) = sliceForAppend(record, plaintextLen + paddingLen);
        copy(dst, payload);
        copy(dst.slice(len(payload)), mac);
        for (nint i = plaintextLen; i < len(dst); i++) {
            dst[i] = (byte)(paddingLen - 1);
        }
        if (len(explicitNonce) > 0) {
            c.SetIV(explicitNonce);
        }
        c.CryptBlocks(dst, dst);
        break;
    }
    default: {
        var c = hc.cipher;
        throw panic("unknown cipher type");
        break;
    }}
    // Update length to include nonce, MAC and any block padding needed.
    nint n = len(record) - (nint)recordHeaderLen;
    record[3] = (byte)((n >> (int)(8)));
    record[4] = (byte)n;
    hc.incSeq();
    return (record, default!);
}

// RecordHeaderError is returned when a TLS record header is invalid.
[GoType] partial struct RecordHeaderError {
    // Msg contains a human readable string that describes the error.
    public @string Msg;
    // RecordHeader contains the five bytes of TLS record header that
    // triggered the error.
    public array<byte> RecordHeader = new(5);
    // Conn provides the underlying net.Conn in the case that a client
    // sent an initial handshake that didn't look like TLS.
    // It is nil if there's already been a handshake or a TLS alert has
    // been written to the connection.
    public net.Conn Conn;
}

public static @string Error(this RecordHeaderError e) {
    e = e.ΔClone();

    return "tls: "u8 + e.Msg;
}

[GoRecv] internal static RecordHeaderError /*err*/ newRecordHeaderError(this ref Conn c, net.Conn conn, @string msg) {
    RecordHeaderError err = new();

    err.Msg = msg;
    err.Conn = conn;
    copy(err.RecordHeader[..], c.rawInput.Bytes());
    return err.ΔClone();
}

internal static error readRecord(this ж<Conn> Ꮡc) {
    return Ꮡc.readRecordOrCCS(false);
}

internal static error readChangeCipherSpec(this ж<Conn> Ꮡc) {
    return Ꮡc.readRecordOrCCS(true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInternalErrorˢ = "tls: internal error: attempted to read record with pending application data"u8;
internal static readonly @string tlsInternalErrorˢ2 = "tls: internal error: attempted to read record with QUIC transport"u8;
internal static readonly @string unsupportedSSLv2ˢ = "unsupported SSLv2 handshake received"u8;
internal static readonly @string firstRecordDoesNotLookˢ = "first record does not look like a TLS handshake"u8;

// readRecordOrCCS reads one or more TLS records from the connection and
// updates the record layer state. Some invariants:
//   - c.in must be locked
//   - c.input must be empty
//
// During the handshake one and only one of the following will happen:
//   - c.hand grows
//   - c.in.changeCipherSpec is called
//   - an error is returned
//
// After the handshake one and only one of the following will happen:
//   - c.hand grows
//   - c.input is set
//   - an error is returned
internal static error readRecordOrCCS(this ж<Conn> Ꮡc, bool expectChangeCipherSpec) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.@in.err != default!) {
        return c.@in.err;
    }
    var handshakeComplete = Ꮡc.of(Conn.ᏑisHandshakeComplete).Load();
    // This function modifies c.rawInput, which owns the c.input memory.
    if (c.input.Len() != 0) {
        return c.@in.setErrorLocked(errors.New(tlsInternalErrorˢ));
    }
    c.input.Reset(default!);
    if (c.quic != nil) {
        return c.@in.setErrorLocked(errors.New(tlsInternalErrorˢ2));
    }
    // Read header, payload.
    {
        var errΔ1 = c.readFromUntil(new net_ConnᴠReader(c.conn), recordHeaderLen); if (errΔ1 != default!) {
            // RFC 8446, Section 6.1 suggests that EOF without an alertCloseNotify
            // is an error, but popular web sites seem to do this, so we accept it
            // if and only if at the record boundary.
            if (AreEqual(errΔ1, io.ErrUnexpectedEOF) && c.rawInput.Len() == 0) {
                errΔ1 = io.EOF;
            }
            {
                var (e, ok) = errΔ1._<netꓸError>(ᐧ); if (!ok || !e.Temporary()) {
                    c.@in.setErrorLocked(errΔ1);
                }
            }
            return errΔ1;
        }
    }
    var hdr = c.rawInput.Bytes()[..(int)(recordHeaderLen)];
    var typ = ((recordType)hdr[0]);
    // No valid TLS record has a type of 0x80, however SSLv2 handshakes
    // start with a uint16 length where the MSB is set and the first record
    // is always < 256 bytes long. Therefore typ == 0x80 strongly suggests
    // an SSLv2 client.
    if (!handshakeComplete && typ == 0x80) {
        Ꮡc.sendAlert(alertProtocolVersion);
        return c.@in.setErrorLocked(c.newRecordHeaderError(default!, unsupportedSSLv2ˢ));
    }
    var vers = (uint16)((uint16)((uint16)hdr[1] << (int)(8)) | (uint16)hdr[2]);
    var expectedVers = c.vers;
    if (expectedVers == VersionTLS13) {
        // All TLS 1.3 records are expected to have 0x0303 (1.2) after
        // the initial hello (RFC 8446 Section 5.1).
        expectedVers = VersionTLS12;
    }
    nint n = (nint)(((nint)hdr[3] << (int)(8)) | (nint)hdr[4]);
    if (c.haveVers && vers != expectedVers) {
        Ꮡc.sendAlert(alertProtocolVersion);
        @string msg = fmt.Sprintf("received record with version %x when expecting version %x"u8, vers, expectedVers);
        return c.@in.setErrorLocked(c.newRecordHeaderError(default!, msg));
    }
    if (!c.haveVers) {
        // First message, be extra suspicious: this might not be a TLS
        // client. Bail out before reading a full 'body', if possible.
        // The current max version is 3.3 so if the version is >= 16.0,
        // it's probably not real.
        if ((typ != recordTypeAlert && typ != recordTypeHandshake) || vers >= 0x1000) {
            return c.@in.setErrorLocked(c.newRecordHeaderError(c.conn, firstRecordDoesNotLookˢ));
        }
    }
    if (c.vers == VersionTLS13 && n > maxCiphertextTLS13 || n > maxCiphertext) {
        Ꮡc.sendAlert(alertRecordOverflow);
        @string msg = fmt.Sprintf("oversized record received with length %d"u8, n);
        return c.@in.setErrorLocked(c.newRecordHeaderError(default!, msg));
    }
    {
        var errΔ2 = c.readFromUntil(new net_ConnᴠReader(c.conn), (nint)recordHeaderLen + n); if (errΔ2 != default!) {
            {
                var (e, ok) = errΔ2._<netꓸError>(ᐧ); if (!ok || !e.Temporary()) {
                    c.@in.setErrorLocked(errΔ2);
                }
            }
            return errΔ2;
        }
    }
    // Process message.
    var record = c.rawInput.Next((nint)recordHeaderLen + n);
    (var data, typ, var err) = c.@in.decrypt(record);
    if (err != default!) {
        return c.@in.setErrorLocked(Ꮡc.sendAlert(err._<alert>()));
    }
    if (len(data) > maxPlaintext) {
        return c.@in.setErrorLocked(Ꮡc.sendAlert(alertRecordOverflow));
    }
    // Application Data messages are always protected.
    if (c.@in.cipher == default! && typ == recordTypeApplicationData) {
        return c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage));
    }
    if (typ != recordTypeAlert && typ != recordTypeChangeCipherSpec && len(data) > 0) {
        // This is a state-advancing message: reset the retry count.
        c.retryCount = 0;
    }
    // Handshake messages MUST NOT be interleaved with other record types in TLS 1.3.
    if (c.vers == VersionTLS13 && typ != recordTypeHandshake && c.hand.Len() > 0) {
        return c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage));
    }
    var exprᴛ1 = typ;
    if (exprᴛ1 == recordTypeAlert) {
        if (c.quic != nil) {
            return c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage));
        }
        if (len(data) != 2) {
            return c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage));
        }
        if (((alert)data[1]) == alertCloseNotify) {
            return c.@in.setErrorLocked(io.EOF);
        }
        if (c.vers == VersionTLS13) {
            return c.@in.setErrorLocked(new net.OpErrorжerror(Ꮡ(new net.OpError(Op: "remote error"u8, Err: ((alert)data[1])))));
        }
        var exprᴛ2 = data[0];
        if (exprᴛ2 == alertLevelWarning) {
            return Ꮡc.retryReadRecord(expectChangeCipherSpec);
        }
        if (exprᴛ2 == alertLevelError) {
            return c.@in.setErrorLocked(new net.OpErrorжerror(Ꮡ(new net.OpError( // Drop the record on the floor and retry.
Op: "remote error"u8, Err: ((alert)data[1])))));
        }
        { /* default: */
            return c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage));
        }

    }
    else if (exprᴛ1 == recordTypeChangeCipherSpec) {
        if (len(data) != 1 || data[0] != 1) {
            return c.@in.setErrorLocked(Ꮡc.sendAlert(alertDecodeError));
        }
        if (c.hand.Len() > 0) {
            // Handshake messages are not allowed to fragment across the CCS.
            return c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage));
        }
        if (c.vers == VersionTLS13) {
            // In TLS 1.3, change_cipher_spec records are ignored until the
            // Finished. See RFC 8446, Appendix D.4. Note that according to Section
            // 5, a server can send a ChangeCipherSpec before its ServerHello, when
            // c.vers is still unset. That's not useful though and suspicious if the
            // server then selects a lower protocol version, so don't allow that.
            return Ꮡc.retryReadRecord(expectChangeCipherSpec);
        }
        if (!expectChangeCipherSpec) {
            return c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage));
        }
        {
            var errΔ4 = c.@in.changeCipherSpec(); if (errΔ4 != default!) {
                return c.@in.setErrorLocked(Ꮡc.sendAlert(errΔ4._<alert>()));
            }
        }
    }
    else if (exprᴛ1 == recordTypeApplicationData) {
        if (!handshakeComplete || expectChangeCipherSpec) {
            return c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage));
        }
        if (len(data) == 0) {
            // Some OpenSSL servers send empty records in order to randomize the
            // CBC IV. Ignore a limited number of empty records.
            return Ꮡc.retryReadRecord(expectChangeCipherSpec);
        }
        c.input.Reset(data);
    }
    else if (exprᴛ1 == recordTypeHandshake) {
        if (len(data) == 0 || expectChangeCipherSpec) {
            // Note that data is owned by c.rawInput, following the Next call above,
            // to avoid copying the plaintext. This is safe because c.rawInput is
            // not read from or written to until c.input is drained.
            return c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage));
        }
        c.hand.Write(data);
    }
    else { /* default: */
        return c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage));
    }

    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsTooManyIgnoredRecordsˢ = "tls: too many ignored records"u8;

// retryReadRecord recurs into readRecordOrCCS to drop a non-advancing record, like
// a warning alert, empty application_data, or a change_cipher_spec in TLS 1.3.
internal static error retryReadRecord(this ж<Conn> Ꮡc, bool expectChangeCipherSpec) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.retryCount++;
    if (c.retryCount > maxUselessRecords) {
        Ꮡc.sendAlert(alertUnexpectedMessage);
        return c.@in.setErrorLocked(errors.New(tlsTooManyIgnoredRecordsˢ));
    }
    return Ꮡc.readRecordOrCCS(expectChangeCipherSpec);
}

// atLeastReader reads from R, stopping with EOF once at least N bytes have been
// read. It is different from an io.LimitedReader in that it doesn't cut short
// the last Read call, and in that it considers an early EOF an error.
[GoType] partial struct atLeastReader {
    public io.Reader R;
    public int64 N;
}

[GoRecv] internal static (nint, error) Read(this ref atLeastReader r, slice<byte> p) {
    if (r.N <= 0) {
        return (0, io.EOF);
    }
    var (n, err) = r.R.Read(p);
    r.N -= (int64)n; // won't underflow unless len(p) >= n > 9223372036854775809
    if (r.N > 0 && AreEqual(err, io.EOF)) {
        return (n, io.ErrUnexpectedEOF);
    }
    if (r.N <= 0 && err == default!) {
        return (n, io.EOF);
    }
    return (n, err);
}

// readFromUntil reads from r into c.rawInput until c.rawInput contains
// at least n bytes or else returns an error.
[GoRecv] internal static error readFromUntil(this ref Conn c, io.Reader r, nint n) {
    if (c.rawInput.Len() >= n) {
        return default!;
    }
    nint needs = n - c.rawInput.Len();
    // There might be extra input waiting on the wire. Make a best effort
    // attempt to fetch it so that it can be used in (*Conn).Read to
    // "predict" closeNotify alerts.
    c.rawInput.Grow(needs + (nint)bytes.MinRead);
    var (_, err) = c.rawInput.ReadFrom(new atLeastReaderжReader(Ꮡ(new atLeastReader(r, (int64)needs))));
    return err;
}

// sendAlertLocked sends a TLS alert message.
internal static error sendAlertLocked(this ж<Conn> Ꮡc, alert err) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.quic != nil) {
        return c.@out.setErrorLocked(new net.OpErrorжerror(Ꮡ(new net.OpError(Op: "local error"u8, Err: err))));
    }
    var exprᴛ1 = err;
    if (exprᴛ1 == alertNoRenegotiation || exprᴛ1 == alertCloseNotify) {
        c.tmp[0] = alertLevelWarning;
    }
    else { /* default: */
        c.tmp[0] = alertLevelError;
    }

    c.tmp[1] = (byte)err;
    var (_, writeErr) = Ꮡc.writeRecordLocked(recordTypeAlert, c.tmp[0..2]);
    if (err == alertCloseNotify) {
        // closeNotify is a special case in that it isn't an error.
        return writeErr;
    }
    return c.@out.setErrorLocked(new net.OpErrorжerror(Ꮡ(new net.OpError(Op: "local error"u8, Err: err))));
}

// sendAlert sends a TLS alert message.
internal static error sendAlert(this ж<Conn> Ꮡc, alert err) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Lock();
        ᒐd1 = true;
        return Ꮡc.sendAlertLocked(err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Unlock(); ᒐ.Run(); }
}

internal static UntypedInt tcpMSSEstimate => 1208;
internal static UntypedInt recordSizeBoostThreshold => /* 128 * 1024 */ 131072;

// maxPayloadSizeForWrite returns the maximum TLS payload size to use for the
// next application data record. There is the following trade-off:
//
//   - For latency-sensitive applications, such as web browsing, each TLS
//     record should fit in one TCP segment.
//   - For throughput-sensitive applications, such as large file transfers,
//     larger TLS records better amortize framing and encryption overheads.
//
// A simple heuristic that works well in practice is to use small records for
// the first 1MB of data, then use larger records for subsequent data, and
// reset back to smaller records after the connection becomes idle. See "High
// Performance Web Networking", Chapter 4, or:
// https://www.igvita.com/2013/10/24/optimizing-tls-record-size-and-buffering-latency/
//
// In the interests of simplicity and determinism, this code does not attempt
// to reset the record size once the connection is idle, however.
[GoRecv] internal static nint maxPayloadSizeForWrite(this ref Conn c, recordType typ) {
    if ((~c.config).DynamicRecordSizingDisabled || typ != recordTypeApplicationData) {
        return maxPlaintext;
    }
    if (c.bytesSent >= recordSizeBoostThreshold) {
        return maxPlaintext;
    }
    // Subtract TLS overheads to get the maximum payload size.
    nint payloadBytes = (nint)(tcpMSSEstimate - recordHeaderLen) - c.@out.explicitNonceLen();
    if (c.@out.cipher != default!) {
        switch (c.@out.cipher.type()) {
        case {} Δciph when Δciph._<cipher.Stream>(out var ciph): {
            payloadBytes -= c.@out.mac.Size();
            break;
        }
        case {} Δciph when Δciph._<cipher.AEAD>(out var ciph): {
            payloadBytes -= ciph.Overhead();
            break;
        }
        case {} Δciph when Δciph._<cbcMode>(out var ciph): {
            nint blockSize = ciph.BlockSize();
            payloadBytes = ((nint)(payloadBytes & ~(blockSize - 1))) - 1;
            payloadBytes -= c.@out.mac.Size();
            break;
        }
        default: {
            var ciph = c.@out.cipher;
            throw panic("unknown cipher type");
            break;
        }}
    }
    // The payload must fit in a multiple of blockSize, with
    // room for at least one padding byte.
    // The MAC is appended before padding so affects the
    // payload size directly.
    if (c.vers == VersionTLS13) {
        payloadBytes--; // encrypted ContentType
    }
    // Allow packet growth in arithmetic progression up to max.
    var pkt = c.packetsSent;
    c.packetsSent++;
    if (pkt > 1000) {
        return maxPlaintext; // avoid overflow in multiply below
    }
    nint n = payloadBytes * (nint)(pkt + 1);
    if (n > maxPlaintext) {
        n = maxPlaintext;
    }
    return n;
}

[GoRecv] internal static (nint, error) write(this ref Conn c, slice<byte> data) {
    if (c.buffering) {
        c.sendBuf = appendꓸꓸꓸ(c.sendBuf, data);
        return (len(data), default!);
    }
    var (n, err) = c.conn.Write(data);
    c.bytesSent += (int64)n;
    return (n, err);
}

[GoRecv] internal static (nint, error) flush(this ref Conn c) {
    if (len(c.sendBuf) == 0) {
        return (0, default!);
    }
    var (n, err) = c.conn.Write(c.sendBuf);
    c.bytesSent += (int64)n;
    c.sendBuf = default!;
    c.buffering = false;
    return (n, err);
}

// outBufPool pools the record-sized scratch buffers used by writeRecordLocked.
internal static ж<sync.Pool> ᏑoutBufPool = new StandardBox<sync.Pool>(new sync.Pool(
    New: () => @new<slice<byte>>()
));
internal static ref sync.Pool outBufPool => ref ᏑoutBufPool.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInternalErrorSendingˢ = "tls: internal error: sending non-handshake message to QUIC transport"u8;

// writeRecordLocked writes a TLS record with the given type and payload to the
// connection and updates the record layer state.
internal static (nint, error) writeRecordLocked(this ж<Conn> Ꮡc, recordType typ, slice<byte> data) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (c.quic != nil) {
            if (typ != recordTypeHandshake) {
                return (0, errors.New(tlsInternalErrorSendingˢ));
            }
            c.quicWriteCryptoData(c.@out.level, data);
            if (!c.buffering) {
                {
                    var (_, err) = c.flush(); if (err != default!) {
                        return (0, err);
                    }
                }
            }
            return (len(data), default!);
        }
        var outBufPtr = ᏑoutBufPool.Get()._<ж<slice<byte>>>();
        ref var outBuf = ref heap<slice<byte>>(out var ᏑoutBuf);
        outBuf = outBufPtr.ValueSlot;
        var outBufPtrʗ1 = outBufPtr;
        defer(() => {
            // You might be tempted to simplify this by just passing &outBuf to Put,
            // but that would make the local copy of the outBuf slice header escape
            // to the heap, causing an allocation. Instead, we keep around the
            // pointer to the slice header returned by Get, which is already on the
            // heap, and overwrite and return that.
            outBufPtrʗ1.ValueSlot = ᏑoutBuf.ValueSlot;
            ᏑoutBufPool.Put(outBufPtrʗ1.OrTypedNil());
        }, ref ᒐ);
        nint n = default!;
        while (len(data) > 0) {
            nint m = len(data);
            {
                nint maxPayload = c.maxPayloadSizeForWrite(typ); if (m > maxPayload) {
                    m = maxPayload;
                }
            }
            (_, outBuf) = sliceForAppend(outBuf[..0], recordHeaderLen);
            outBuf[0] = (byte)typ;
            var vers = c.vers;
            if (vers == 0){
                // Some TLS servers fail if the record version is
                // greater than TLS 1.0 for the initial ClientHello.
                vers = VersionTLS10;
            } else 
            if (vers == VersionTLS13) {
                // TLS 1.3 froze the record layer version to 1.2.
                // See RFC 8446, Section 5.1.
                vers = VersionTLS12;
            }
            outBuf[1] = (byte)((vers >> (int)(8)));
            outBuf[2] = (byte)vers;
            outBuf[3] = (byte)((m >> (int)(8)));
            outBuf[4] = (byte)m;
            error err = default!;
            (outBuf, err) = c.@out.encrypt(outBuf, data.slice(0, m), c.config.rand());
            if (err != default!) {
                return (n, err);
            }
            {
                var (_, errΔ1) = c.write(outBuf); if (errΔ1 != default!) {
                    return (n, errΔ1);
                }
            }
            n += m;
            data = data.slice(m);
        }
        if (typ == recordTypeChangeCipherSpec && c.vers != VersionTLS13) {
            {
                var err = c.@out.changeCipherSpec(); if (err != default!) {
                    return (n, Ꮡc.sendAlertLocked(err._<alert>()));
                }
            }
        }
        return (n, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// writeHandshakeRecord writes a handshake message to the connection and updates
// the record layer state. If transcript is non-nil the marshaled message is
// written to it.
internal static (nint, error) writeHandshakeRecord(this ж<Conn> Ꮡc, handshakeMessage msg, transcriptHash transcript) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Lock();
        ᒐd1 = true;
        var (data, err) = msg.marshal();
        if (err != default!) {
            return (0, err);
        }
        if (transcript != default!) {
            transcript.Write(data);
        }
        return Ꮡc.writeRecordLocked(recordTypeHandshake, data);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Unlock(); ᒐ.Run(); }
}

// writeChangeCipherRecord writes a ChangeCipherSpec message to the connection and
// updates the record layer state.
internal static error writeChangeCipherRecord(this ж<Conn> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Lock();
        ᒐd1 = true;
        var (_, err) = Ꮡc.writeRecordLocked(recordTypeChangeCipherSpec, new byte[]{1}.slice());
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Unlock(); ᒐ.Run(); }
}

// readHandshakeBytes reads handshake data until c.hand contains at least n bytes.
internal static error readHandshakeBytes(this ж<Conn> Ꮡc, nint n) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.quic != nil) {
        return Ꮡc.quicReadHandshakeBytes(n);
    }
    while (c.hand.Len() < n) {
        {
            var err = Ꮡc.readRecord(); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

// readHandshake reads the next handshake message from
// the record layer. If transcript is non-nil, the message
// is written to the passed transcriptHash.
internal static (any, error) readHandshake(this ж<Conn> Ꮡc, transcriptHash transcript) {
    ref var c = ref Ꮡc.DerefOrNull();

    {
        var err = Ꮡc.readHandshakeBytes(4); if (err != default!) {
            return (default!, err);
        }
    }
    var data = c.hand.Bytes();
    nint maxHandshakeSize = maxHandshake;
    // hasVers indicates we're past the first message, forcing someone trying to
    // make us just allocate a large buffer to at least do the initial part of
    // the handshake first.
    if (c.haveVers && data[0] == typeCertificate) {
        // Since certificate messages are likely to be the only messages that
        // can be larger than maxHandshake, we use a special limit for just
        // those messages.
        maxHandshakeSize = maxHandshakeCertificateMsg;
    }
    nint n = (nint)((nint)(((nint)data[1] << (int)(16)) | ((nint)data[2] << (int)(8))) | (nint)data[3]);
    if (n > maxHandshakeSize) {
        Ꮡc.sendAlertLocked(alertInternalError);
        return (default!, c.@in.setErrorLocked(fmt.Errorf("tls: handshake message of length %d bytes exceeds maximum of %d bytes"u8, n, maxHandshakeSize)));
    }
    {
        var err = Ꮡc.readHandshakeBytes(4 + n); if (err != default!) {
            return (default!, err);
        }
    }
    data = c.hand.Next(4 + n);
    return Ꮡc.unmarshalHandshakeMessage(data, transcript);
}

internal static (handshakeMessage, error) unmarshalHandshakeMessage(this ж<Conn> Ꮡc, slice<byte> data, transcriptHash transcript) {
    ref var c = ref Ꮡc.DerefOrNull();

    handshakeMessage m = default!;
    switch (data[0]) {
    case typeHelloRequest: {
        m = new helloRequestMsgжhandshakeMessage(@new<helloRequestMsg>());
        break;
    }
    case typeClientHello: {
        m = new clientHelloMsgжhandshakeMessage(@new<clientHelloMsg>());
        break;
    }
    case typeServerHello: {
        m = new serverHelloMsgжhandshakeMessage(@new<serverHelloMsg>());
        break;
    }
    case typeNewSessionTicket: {
        if (c.vers == VersionTLS13){
            m = new newSessionTicketMsgTLS13жhandshakeMessage(@new<newSessionTicketMsgTLS13>());
        } else {
            m = new newSessionTicketMsgжhandshakeMessage(@new<newSessionTicketMsg>());
        }
        break;
    }
    case typeCertificate: {
        if (c.vers == VersionTLS13){
            m = new certificateMsgTLS13жhandshakeMessage(@new<certificateMsgTLS13>());
        } else {
            m = new certificateMsgжhandshakeMessage(@new<certificateMsg>());
        }
        break;
    }
    case typeCertificateRequest: {
        if (c.vers == VersionTLS13){
            m = new certificateRequestMsgTLS13жhandshakeMessage(@new<certificateRequestMsgTLS13>());
        } else {
            m = new certificateRequestMsgжhandshakeMessage(Ꮡ(new certificateRequestMsg(
                hasSignatureAlgorithm: c.vers >= VersionTLS12
            )));
        }
        break;
    }
    case typeCertificateStatus: {
        m = new certificateStatusMsgжhandshakeMessage(@new<certificateStatusMsg>());
        break;
    }
    case typeServerKeyExchange: {
        m = new serverKeyExchangeMsgжhandshakeMessage(@new<serverKeyExchangeMsg>());
        break;
    }
    case typeServerHelloDone: {
        m = new serverHelloDoneMsgжhandshakeMessage(@new<serverHelloDoneMsg>());
        break;
    }
    case typeClientKeyExchange: {
        m = new clientKeyExchangeMsgжhandshakeMessage(@new<clientKeyExchangeMsg>());
        break;
    }
    case typeCertificateVerify: {
        m = new certificateVerifyMsgжhandshakeMessage(Ꮡ(new certificateVerifyMsg(
            hasSignatureAlgorithm: c.vers >= VersionTLS12
        )));
        break;
    }
    case typeFinished: {
        m = new finishedMsgжhandshakeMessage(@new<finishedMsg>());
        break;
    }
    case typeEncryptedExtensions: {
        m = new encryptedExtensionsMsgжhandshakeMessage(@new<encryptedExtensionsMsg>());
        break;
    }
    case typeEndOfEarlyData: {
        m = new endOfEarlyDataMsgжhandshakeMessage(@new<endOfEarlyDataMsg>());
        break;
    }
    case typeKeyUpdate: {
        m = new keyUpdateMsgжhandshakeMessage(@new<keyUpdateMsg>());
        break;
    }
    default: {
        return (default!, c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage)));
    }}

    // The handshake message unmarshalers
    // expect to be able to keep references to data,
    // so pass in a fresh copy that won't be overwritten.
    data = appendꓸꓸꓸ(slice<byte>(default!), data);
    if (!m.unmarshal(data)) {
        return (default!, c.@in.setErrorLocked(Ꮡc.sendAlert(alertUnexpectedMessage)));
    }
    if (transcript != default!) {
        transcript.Write(data);
    }
    return (m, default!);
}

internal static error errShutdown = errors.New("tls: protocol is shutdown"u8);

// Write writes data to the connection.
//
// As Write calls [Conn.Handshake], in order to prevent indefinite blocking a deadline
// must be set for both [Conn.Read] and Write before Write is called when the handshake
// has not yet completed. See [Conn.SetDeadline], [Conn.SetReadDeadline], and
// [Conn.SetWriteDeadline].
public static (nint, error) Write(this ж<Conn> Ꮡc, slice<byte> b) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        // interlock with Close below
        while (ᐧ) {
            var x = Ꮡc.of(Conn.ᏑactiveCall).Load();
            if ((int32)(x & 1) != 0) {
                return (0, net.ErrClosed);
            }
            if (Ꮡc.of(Conn.ᏑactiveCall).CompareAndSwap(x, x + 2)) {
                break;
            }
        }
        defer(Ꮡc.of(Conn.ᏑactiveCall).Add, (int32)(-2), ref ᒐ);
        {
            var errΔ1 = Ꮡc.Handshake(); if (errΔ1 != default!) {
                return (0, errΔ1);
            }
        }
        Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Lock();
        defer(Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Unlock, ref ᒐ);
        {
            var errΔ2 = c.@out.err; if (errΔ2 != default!) {
                return (0, errΔ2);
            }
        }
        if (!Ꮡc.of(Conn.ᏑisHandshakeComplete).Load()) {
            return (0, alertInternalError);
        }
        if (c.closeNotifySent) {
            return (0, errShutdown);
        }
        // TLS 1.0 is susceptible to a chosen-plaintext
        // attack when using block mode ciphers due to predictable IVs.
        // This can be prevented by splitting each Application Data
        // record into two records, effectively randomizing the IV.
        //
        // https://www.openssl.org/~bodo/tls-cbc.txt
        // https://bugzilla.mozilla.org/show_bug.cgi?id=665814
        // https://www.imperialviolet.org/2012/01/15/beastfollowup.html
        nint m = default!;
        if (len(b) > 1 && c.vers == VersionTLS10) {
            {
                var (_, ok) = c.@out.cipher._<cipher.BlockMode>(ᐧ); if (ok) {
                    var (nΔ1, errΔ3) = Ꮡc.writeRecordLocked(recordTypeApplicationData, b[..1]);
                    if (errΔ3 != default!) {
                        return (nΔ1, c.@out.setErrorLocked(errΔ3));
                    }
                    (m, b) = (1, b[1..]);
                }
            }
        }
        var (n, err) = Ꮡc.writeRecordLocked(recordTypeApplicationData, b);
        return (n + m, c.@out.setErrorLocked(err));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInternalErrorˢ3 = "tls: internal error: unexpected renegotiation"u8;
internal static readonly @string tlsUnknownRenegotiationˢ = "tls: unknown Renegotiation value"u8;

// handleRenegotiation processes a HelloRequest handshake message.
internal static error handleRenegotiation(this ж<Conn> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (c.vers == VersionTLS13) {
            return errors.New(tlsInternalErrorˢ3);
        }
        var (msg, err) = Ꮡc.readHandshake(default!);
        if (err != default!) {
            return err;
        }
        var (helloReq, ok) = msg._<ж<helloRequestMsg>>(ᐧ);
        if (!ok) {
            Ꮡc.sendAlert(alertUnexpectedMessage);
            return unexpectedMessageError(helloReq.OrTypedNil(), msg);
        }
        if (!c.isClient) {
            return Ꮡc.sendAlert(alertNoRenegotiation);
        }
        var exprᴛ1 = (~c.config).Renegotiation;
        if (exprᴛ1 == RenegotiateNever) {
            return Ꮡc.sendAlert(alertNoRenegotiation);
        }
        if (exprᴛ1 == RenegotiateOnceAsClient) {
            if (c.handshakes > 1) {
                return Ꮡc.sendAlert(alertNoRenegotiation);
            }
        }
        else if (exprᴛ1 == RenegotiateFreelyAsClient) {
        }
        else { /* default: */
            Ꮡc.sendAlert(alertInternalError);
            return errors.New(tlsUnknownRenegotiationˢ);
        }

        // Ok.
        c.handshakeMutex.Lock();
        ᒐd1 = true;
        Ꮡc.of(Conn.ᏑisHandshakeComplete).Store(false);
        {
            c.handshakeErr = Ꮡc.clientHandshake(context.Background()); if (c.handshakeErr == default!) {
                c.handshakes++;
            }
        }
        return c.handshakeErr;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().handshakeMutex.Unlock(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsTooManyNonAdvancingˢ = "tls: too many non-advancing records"u8;

// handlePostHandshakeMessage processes a handshake message arrived after the
// handshake is complete. Up to TLS 1.2, it indicates the start of a renegotiation.
internal static error handlePostHandshakeMessage(this ж<Conn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.vers != VersionTLS13) {
        return Ꮡc.handleRenegotiation();
    }
    var (msg, err) = Ꮡc.readHandshake(default!);
    if (err != default!) {
        return err;
    }
    c.retryCount++;
    if (c.retryCount > maxUselessRecords) {
        Ꮡc.sendAlert(alertUnexpectedMessage);
        return c.@in.setErrorLocked(errors.New(tlsTooManyNonAdvancingˢ));
    }
    switch (msg.type()) {
    case ж<newSessionTicketMsgTLS13> msgΔ1: {
        return Ꮡc.handleNewSessionTicket(msgΔ1);
    }
    case ж<keyUpdateMsg> msgΔ1: {
        return Ꮡc.handleKeyUpdate(msgΔ1);
    }}
    // The QUIC layer is supposed to treat an unexpected post-handshake CertificateRequest
    // as a QUIC-level PROTOCOL_VIOLATION error (RFC 9001, Section 4.4). Returning an
    // unexpected_message alert here doesn't provide it with enough information to distinguish
    // this condition from other unexpected messages. This is probably fine.
    Ꮡc.sendAlert(alertUnexpectedMessage);
    return fmt.Errorf("tls: received unexpected handshake message of type %T"u8, msg);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsReceivedUnexpectedKeyˢ = "tls: received unexpected key update message"u8;

internal static error handleKeyUpdate(this ж<Conn> Ꮡc, ж<keyUpdateMsg> ᏑkeyUpdate) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var keyUpdate = ref ᏑkeyUpdate.DerefOrNull();

        if (c.quic != nil) {
            Ꮡc.sendAlert(alertUnexpectedMessage);
            return c.@in.setErrorLocked(errors.New(tlsReceivedUnexpectedKeyˢ));
        }
        var cipherSuite = cipherSuiteTLS13ByID(c.cipherSuite);
        if (cipherSuite == nil) {
            return c.@in.setErrorLocked(Ꮡc.sendAlert(alertInternalError));
        }
        if (keyUpdate.updateRequested) {
            Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Lock();
            ᒐd1 = true;
            var msg = Ꮡ(new keyUpdateMsg(nil));
            var (msgBytes, err) = msg.marshal();
            if (err != default!) {
                return err;
            }
            (_, err) = Ꮡc.writeRecordLocked(recordTypeHandshake, msgBytes);
            if (err != default!) {
                // Surface the error at the next write.
                c.@out.setErrorLocked(err);
                return default!;
            }
            var newSecretΔ1 = cipherSuite.nextTrafficSecret(c.@out.trafficSecret);
            c.setWriteTrafficSecret(cipherSuite, QUICEncryptionLevelInitial, newSecretΔ1);
        }
        var newSecret = cipherSuite.nextTrafficSecret(c.@in.trafficSecret);
        {
            var err = Ꮡc.setReadTrafficSecret(cipherSuite, QUICEncryptionLevelInitial, newSecret); if (err != default!) {
                return err;
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Unlock(); ᒐ.Run(); }
}

// Read reads data from the connection.
//
// As Read calls [Conn.Handshake], in order to prevent indefinite blocking a deadline
// must be set for both Read and [Conn.Write] before Read is called when the handshake
// has not yet completed. See [Conn.SetDeadline], [Conn.SetReadDeadline], and
// [Conn.SetWriteDeadline].
public static (nint, error) Read(this ж<Conn> Ꮡc, slice<byte> b) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        {
            var err = Ꮡc.Handshake(); if (err != default!) {
                return (0, err);
            }
        }
        if (len(b) == 0) {
            // Put this after Handshake, in case people were calling
            // Read(nil) for the side effect of the Handshake.
            return (0, default!);
        }
        Ꮡc.of(Conn.Ꮡin).of(halfConn.ᏑMutex).Lock();
        ᒐd1 = true;
        while (c.input.Len() == 0) {
            {
                var err = Ꮡc.readRecord(); if (err != default!) {
                    return (0, err);
                }
            }
            while (c.hand.Len() > 0) {
                {
                    var err = Ꮡc.handlePostHandshakeMessage(); if (err != default!) {
                        return (0, err);
                    }
                }
            }
        }
        var (n, _) = c.input.Read(b);
        // If a close-notify alert is waiting, read it so that we can return (n,
        // EOF) instead of (n, nil), to signal to the HTTP response reading
        // goroutine that the connection is now closed. This eliminates a race
        // where the HTTP response reading goroutine would otherwise not observe
        // the EOF until its next read, by which time a client goroutine might
        // have already tried to reuse the HTTP connection for a new request.
        // See https://golang.org/cl/76400046 and https://golang.org/issue/3514
        if (n != 0 && c.input.Len() == 0 && c.rawInput.Len() > 0 && ((recordType)c.rawInput.Bytes()[0]) == recordTypeAlert) {
            {
                var err = Ꮡc.readRecord(); if (err != default!) {
                    return (n, err); // will be io.EOF on closeNotify
                }
            }
        }
        return (n, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.of(Conn.Ꮡin).of(halfConn.ᏑMutex).Unlock(); ᒐ.Run(); }
}

// Close closes the connection.
public static error Close(this ж<Conn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    // Interlock with Conn.Write above.
    int32 x = default!;
    while (ᐧ) {
        x = Ꮡc.of(Conn.ᏑactiveCall).Load();
        if ((int32)(x & 1) != 0) {
            return net.ErrClosed;
        }
        if (Ꮡc.of(Conn.ᏑactiveCall).CompareAndSwap(x, (int32)(x | 1))) {
            break;
        }
    }
    if (x != 0) {
        // io.Writer and io.Closer should not be used concurrently.
        // If Close is called while a Write is currently in-flight,
        // interpret that as a sign that this Close is really just
        // being used to break the Write and/or clean up resources and
        // avoid sending the alertCloseNotify, which may block
        // waiting on handshakeMutex or the c.out mutex.
        return c.conn.Close();
    }
    error alertErr = default!;
    if (Ꮡc.of(Conn.ᏑisHandshakeComplete).Load()) {
        {
            var err = Ꮡc.closeNotify(); if (err != default!) {
                alertErr = fmt.Errorf("tls: failed to send closeNotify alert (but connection was closed anyway): %w"u8, err);
            }
        }
    }
    {
        var err = c.conn.Close(); if (err != default!) {
            return err;
        }
    }
    return alertErr;
}

internal static error errEarlyCloseWrite = errors.New("tls: CloseWrite called before handshake complete"u8);

// CloseWrite shuts down the writing side of the connection. It should only be
// called once the handshake has completed and does not call CloseWrite on the
// underlying connection. Most callers should just use [Conn.Close].
public static error CloseWrite(this ж<Conn> Ꮡc) {
    if (!Ꮡc.of(Conn.ᏑisHandshakeComplete).Load()) {
        return errEarlyCloseWrite;
    }
    return Ꮡc.closeNotify();
}

internal static error closeNotify(this ж<Conn> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Lock();
        ᒐd1 = true;
        if (!c.closeNotifySent) {
            // Set a Write Deadline to prevent possibly blocking forever.
            c.SetWriteDeadline(time_package.Now().Add((time.Duration)(5000000000L)));
            c.closeNotifyErr = Ꮡc.sendAlertLocked(alertCloseNotify);
            c.closeNotifySent = true;
            // Any subsequent writes will fail.
            c.SetWriteDeadline(time_package.Now());
        }
        return c.closeNotifyErr;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Unlock(); ᒐ.Run(); }
}

// Handshake runs the client or server handshake
// protocol if it has not yet been run.
//
// Most uses of this package need not call Handshake explicitly: the
// first [Conn.Read] or [Conn.Write] will call it automatically.
//
// For control over canceling or setting a timeout on a handshake, use
// [Conn.HandshakeContext] or the [Dialer]'s DialContext method instead.
//
// In order to avoid denial of service attacks, the maximum RSA key size allowed
// in certificates sent by either the TLS server or client is limited to 8192
// bits. This limit can be overridden by setting tlsmaxrsasize in the GODEBUG
// environment variable (e.g. GODEBUG=tlsmaxrsasize=4096).
public static error Handshake(this ж<Conn> Ꮡc) {
    return Ꮡc.HandshakeContext(context.Background());
}

// HandshakeContext runs the client or server handshake
// protocol if it has not yet been run.
//
// The provided Context must be non-nil. If the context is canceled before
// the handshake is complete, the handshake is interrupted and an error is returned.
// Once the handshake has completed, cancellation of the context will not affect the
// connection.
//
// Most uses of this package need not call HandshakeContext explicitly: the
// first [Conn.Read] or [Conn.Write] will call it automatically.
public static error HandshakeContext(this ж<Conn> Ꮡc, context.Context ctx) {
    // Delegate to unexported method for named return
    // without confusing documented signature.
    return Ꮡc.handshakeContext(ctx);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInternalErrorˢ4 = "tls: internal error: handshake should have had a result"u8;

internal static error /*ret*/ handshakeContext(this ж<Conn> Ꮡc, context.Context ctx) {
    error ret = default!;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        // Fast sync/atomic-based exit if there is no handshake in flight and the
        // last one succeeded without an error. Avoids the expensive context setup
        // and mutex for most Read and Write calls.
        if (Ꮡc.of(Conn.ᏑisHandshakeComplete).Load()) {
            ret = default!; goto ᒐdone;
        }
        var (handshakeCtx, cancel) = context.WithCancel(ctx);
        // Note: defer this before starting the "interrupter" goroutine
        // so that we can tell the difference between the input being canceled and
        // this cancellation. In the former case, we need to close the connection.
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        if (c.quic != nil){
            c.quic.Value.cancelc = handshakeCtx.Done();
            c.quic.Value.cancel = cancel;
        } else 
        if (ctx.Done() != default!) {
            // Start the "interrupter" goroutine, if this context might be canceled.
            // (The background context cannot).
            //
            // The interrupter goroutine waits for the input context to be done and
            // closes the connection if this happens before the function returns.
            var done = new channel<EmptyStruct>(0);
            var interruptRes = new channel<error>(1);
            var doneʗ1 = done;
            var interruptResʗ1 = interruptRes;
            defer(() => {
                close(doneʗ1);
                {
                    var ctxErr = ᐸꟷ(interruptResʗ1); if (ctxErr != default!) {
                        // Return context error to user.
                        ret = ctxErr;
                    }
                }
            }, ref ᒐ);
            var doneʗ2 = done;
            var handshakeCtxʗ1 = handshakeCtx;
            var interruptResʗ2 = interruptRes;
            goǃ(() => {
                var selᴛ1 = handshakeCtxʗ1.Done();
                var selᴛ2 = doneʗ2;
                switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
                case 0 when selᴛ1.ꟷᐳ(out _): {
                    _ = Ꮡc.Value.conn.Close();
                    interruptResʗ2.ᐸꟷ(handshakeCtxʗ1.Err());
                    break;
                }
                case 1 when selᴛ2.ꟷᐳ(out _): {
                    interruptResʗ2.ᐸꟷ(default!);
                    break;
                }}
            });
        }
        // Close the connection, discarding the error
        c.handshakeMutex.Lock();
        defer(Ꮡc.of(Conn.ᏑhandshakeMutex).Unlock, ref ᒐ);
        {
            var err = c.handshakeErr; if (err != default!) {
                ret = err; goto ᒐdone;
            }
        }
        if (Ꮡc.of(Conn.ᏑisHandshakeComplete).Load()) {
            ret = default!; goto ᒐdone;
        }
        Ꮡc.of(Conn.Ꮡin).of(halfConn.ᏑMutex).Lock();
        defer(Ꮡc.of(Conn.Ꮡin).of(halfConn.ᏑMutex).Unlock, ref ᒐ);
        c.handshakeErr = c.handshakeFn(handshakeCtx);
        if (c.handshakeErr == default!){
            c.handshakes++;
        } else {
            // If an error occurred during the handshake try to flush the
            // alert that might be left in the buffer.
            c.flush();
        }
        if (c.handshakeErr == default! && !Ꮡc.of(Conn.ᏑisHandshakeComplete).Load()) {
            c.handshakeErr = errors.New(tlsInternalErrorˢ4);
        }
        if (c.handshakeErr != default! && Ꮡc.of(Conn.ᏑisHandshakeComplete).Load()) {
            throw panic("tls: internal error: handshake returned an error but is marked successful");
        }
        if (c.quic != nil) {
            if (c.handshakeErr == default!){
                c.quicHandshakeComplete();
                // Provide the 1-RTT read secret now that the handshake is complete.
                // The QUIC layer MUST NOT decrypt 1-RTT packets prior to completing
                // the handshake (RFC 9001, Section 5.7).
                {
                    var err = Ꮡc.quicSetReadSecret(QUICEncryptionLevelApplication, c.cipherSuite, c.@in.trafficSecret); if (err != default!) {
                        ret = err; goto ᒐdone;
                    }
                }
            } else {
                ref var a = ref heap(new alert(), out var Ꮡa);
                Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Lock();
                if (!errors.As(c.@out.err, Ꮡa)) {
                    a = alertInternalError;
                }
                Ꮡc.of(Conn.Ꮡout).of(halfConn.ᏑMutex).Unlock();
                // Return an error which wraps both the handshake error and
                // any alert error we may have sent, or alertInternalError
                // if we didn't send an alert.
                // Truncate the text of the alert to 0 characters.
                c.handshakeErr = fmt.Errorf("%w%.0w"u8, c.handshakeErr, ((AlertError)(uint8)a));
            }
            close((~c.quic).blockedc);
            close((~c.quic).signalc);
        }
        ret = c.handshakeErr;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return ret;
}

// ConnectionState returns basic TLS details about the connection.
public static ΔConnectionState ConnectionState(this ж<Conn> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.handshakeMutex.Lock();
        ᒐd1 = true;
        return Ꮡc.connectionStateLocked();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().handshakeMutex.Unlock(); ᒐ.Run(); }
}

internal static ж<godebug.Setting> tlsunsafeekm = godebug.New("tlsunsafeekm"u8);

internal static ΔConnectionState connectionStateLocked(this ж<Conn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    ΔConnectionState state = default!;
    state.HandshakeComplete = Ꮡc.of(Conn.ᏑisHandshakeComplete).Load();
    state.Version = c.vers;
    state.NegotiatedProtocol = c.clientProtocol;
    state.DidResume = c.didResume;
    state.testingOnlyDidHRR = c.didHRR;
    // c.curveID is not set on TLS 1.0–1.2 resumptions. Fix that before exposing it.
    state.testingOnlyCurveID = c.curveID;
    state.NegotiatedProtocolIsMutual = true;
    state.ServerName = c.serverName;
    state.CipherSuite = c.cipherSuite;
    state.PeerCertificates = c.peerCertificates;
    state.VerifiedChains = c.verifiedChains;
    state.SignedCertificateTimestamps = c.scts;
    state.OCSPResponse = c.ocspResponse;
    if ((!c.didResume || c.extMasterSecret) && c.vers != VersionTLS13) {
        if (c.clientFinishedIsFirst){
            state.TLSUnique = c.clientFinished[..];
        } else {
            state.TLSUnique = c.serverFinished[..];
        }
    }
    if ((~c.config).Renegotiation != RenegotiateNever){
        state.ekm = noEKMBecauseRenegotiation;
    } else 
    if (c.vers != VersionTLS13 && !c.extMasterSecret){
        state.ekm = (@string label, slice<byte> context, nint length) => {
            if (tlsunsafeekm.Value() == "1"u8) {
                tlsunsafeekm.IncNonDefault();
                return Ꮡc.Value.ekm(label, context, length);
            }
            return noEKMBecauseNoEMS(label, context, length);
        };
    } else {
        state.ekm = c.ekm;
    }
    state.ECHAccepted = c.echAccepted;
    return state;
}

// OCSPResponse returns the stapled OCSP response from the TLS server, if
// any. (Only valid for client connections.)
public static slice<byte> OCSPResponse(this ж<Conn> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.handshakeMutex.Lock();
        ᒐd1 = true;
        return c.ocspResponse;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().handshakeMutex.Unlock(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsVerifyHostnameCalledˢ = "tls: VerifyHostname called on TLS server connection"u8;
internal static readonly @string tlsHandshakeHasNotYetˢ = "tls: handshake has not yet been performed"u8;
internal static readonly @string tlsHandshakeDidNotVerifyˢ = "tls: handshake did not verify certificate chain"u8;

// VerifyHostname checks that the peer certificate chain is valid for
// connecting to host. If so, it returns nil; if not, it returns an error
// describing the problem.
public static error VerifyHostname(this ж<Conn> Ꮡc, @string host) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.handshakeMutex.Lock();
        ᒐd1 = true;
        if (!c.isClient) {
            return errors.New(tlsVerifyHostnameCalledˢ);
        }
        if (!Ꮡc.of(Conn.ᏑisHandshakeComplete).Load()) {
            return errors.New(tlsHandshakeHasNotYetˢ);
        }
        if (len(c.verifiedChains) == 0) {
            return errors.New(tlsHandshakeDidNotVerifyˢ);
        }
        return c.peerCertificates[0].VerifyHostname(host);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().handshakeMutex.Unlock(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsHandshakeBufferNotˢ = "tls: handshake buffer not empty before setting read traffic secret"u8;

// setReadTrafficSecret sets the read traffic secret for the given encryption level. If
// being called at the same time as setWriteTrafficSecret, the caller must ensure the call
// to setWriteTrafficSecret happens first so any alerts are sent at the write level.
internal static error setReadTrafficSecret(this ж<Conn> Ꮡc, ж<cipherSuiteTLS13> Ꮡsuite, QUICEncryptionLevel level, slice<byte> secret) {
    ref var c = ref Ꮡc.DerefOrNull();

    // Ensure that there are no buffered handshake messages before changing the
    // read keys, since that can cause messages to be parsed that were encrypted
    // using old keys which are no longer appropriate.
    if (c.hand.Len() != 0) {
        Ꮡc.sendAlert(alertUnexpectedMessage);
        return errors.New(tlsHandshakeBufferNotˢ);
    }
    c.@in.setTrafficSecret(Ꮡsuite, level, secret);
    return default!;
}

// setWriteTrafficSecret sets the write traffic secret for the given encryption level. If
// being called at the same time as setReadTrafficSecret, the caller must ensure the call
// to setWriteTrafficSecret happens first so any alerts are sent at the write level.
[GoRecv] internal static void setWriteTrafficSecret(this ref Conn c, ж<cipherSuiteTLS13> Ꮡsuite, QUICEncryptionLevel level, slice<byte> secret) {
    c.@out.setTrafficSecret(Ꮡsuite, level, secret);
}

} // end tls_package
