// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using hmac = go.crypto.hmac_package;
using sha256 = go.crypto.sha256_package;
using subtle = go.crypto.subtle_package;
using Δx509 = go.crypto.x509_package;
using errors = errors_package;
using io = io_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using go.crypto;
using hash = hash_package;
using time = time_package;
using vendor.golang.org.x.crypto;

partial class tls_package {

// A SessionState is a resumable session.
[GoType] partial struct SessionState {
// Encoded as a SessionState (in the language of RFC 8446, Section 3).
//
//   enum { server(1), client(2) } SessionStateType;
//
//   opaque Certificate<1..2^24-1>;
//
//   Certificate CertificateChain<0..2^24-1>;
//
//   opaque Extra<0..2^24-1>;
//
//   struct {
//       uint16 version;
//       SessionStateType type;
//       uint16 cipher_suite;
//       uint64 created_at;
//       opaque secret<1..2^8-1>;
//       Extra extra<0..2^24-1>;
//       uint8 ext_master_secret = { 0, 1 };
//       uint8 early_data = { 0, 1 };
//       CertificateEntry certificate_list<0..2^24-1>;
//       CertificateChain verified_chains<0..2^24-1>; /* excluding leaf */
//       select (SessionState.early_data) {
//           case 0: Empty;
//           case 1: opaque alpn<1..2^8-1>;
//       };
//       select (SessionState.type) {
//           case server: Empty;
//           case client: struct {
//               select (SessionState.version) {
//                   case VersionTLS10..VersionTLS12: Empty;
//                   case VersionTLS13: struct {
//                       uint64 use_by;
//                       uint32 age_add;
//                   };
//               };
//           };
//       };
//   } SessionState;
//

    // Extra is ignored by crypto/tls, but is encoded by [SessionState.Bytes]
    // and parsed by [ParseSessionState].
    //
    // This allows [Config.UnwrapSession]/[Config.WrapSession] and
    // [ClientSessionCache] implementations to store and retrieve additional
    // data alongside this session.
    //
    // To allow different layers in a protocol stack to share this field,
    // applications must only append to it, not replace it, and must use entries
    // that can be recognized even if out of order (for example, by starting
    // with an id and version prefix).
    public slice<slice<byte>> Extra;
    // EarlyData indicates whether the ticket can be used for 0-RTT in a QUIC
    // connection. The application may set this to false if it is true to
    // decline to offer 0-RTT even if supported.
    public bool EarlyData;
    internal uint16 version;
    internal bool isClient;
    internal uint16 cipherSuite;
    // createdAt is the generation time of the secret on the sever (which for
    // TLS 1.0–1.2 might be earlier than the current session) and the time at
    // which the ticket was received on the client.
    internal uint64 createdAt; // seconds since UNIX epoch
    internal slice<byte> secret; // master secret for TLS 1.2, or the PSK for TLS 1.3
    internal bool extMasterSecret;
    internal slice<ж<Δx509.Certificate>> peerCertificates;
    internal slice<ж<activeCert>> activeCertHandles;
    internal slice<byte> ocspResponse;
    internal slice<slice<byte>> scts;
    internal slice<slice<ж<Δx509.Certificate>>> verifiedChains;
    internal @string alpnProtocol; // only set if EarlyData is true
    // Client-side TLS 1.3-only fields.
    internal uint64 useBy; // seconds since UNIX epoch
    internal uint32 ageAdd;
    internal slice<byte> ticket;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInternalErrorEmptyˢ = "tls: internal error: empty verified chain"u8;

// Bytes encodes the session, including any private fields, so that it can be
// parsed by [ParseSessionState]. The encoding contains secret values critical
// to the security of future and possibly past sessions.
//
// The specific encoding should be considered opaque and may change incompatibly
// between Go versions.
public static (slice<byte>, error) Bytes(this ж<SessionState> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
    b.AddUint16(s.version);
    if (s.isClient){
        b.AddUint8(2); // client
    } else {
        b.AddUint8(1); // server
    }
    b.AddUint16(s.cipherSuite);
    addUint64(Ꮡb, s.createdAt);
    Ꮡb.AddUint8LengthPrefixed((ж<cryptobyte.Builder> bΔ1) => {
        bΔ1.AddBytes(Ꮡs.Value.secret);
    });
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ2) => {
        foreach (var (_, extra) in Ꮡs.Value.Extra) {
            var extraʗ1 = extra;
            bΔ2.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ3) => {
                bΔ3.AddBytes(extraʗ1);
            });
        }
    });
    if (s.extMasterSecret){
        b.AddUint8(1);
    } else {
        b.AddUint8(0);
    }
    if (s.EarlyData){
        b.AddUint8(1);
    } else {
        b.AddUint8(0);
    }
    marshalCertificate(Ꮡb, new Certificate(
        ΔCertificate: certificatesToBytesSlice(s.peerCertificates),
        OCSPStaple: s.ocspResponse,
        SignedCertificateTimestamps: s.scts
    ));
    Ꮡb.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ4) => {
        foreach (var (_, chain) in Ꮡs.Value.verifiedChains) {
            var chainʗ1 = chain;
            bΔ4.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ5) => {
                // We elide the first certificate because it's always the leaf.
                if (len(chainʗ1) == 0) {
                    bΔ5.SetError(errors.New(tlsInternalErrorEmptyˢ));
                    return;
                }
                foreach (var (_, cert) in chainʗ1[1..]) {
                    var certʗ1 = cert;
                    bΔ5.AddUint24LengthPrefixed((ж<cryptobyte.Builder> bΔ6) => {
                        bΔ6.AddBytes((~certʗ1).Raw);
                    });
                }
            });
        }
    });
    if (s.EarlyData) {
        Ꮡb.AddUint8LengthPrefixed((ж<cryptobyte.Builder> bΔ7) => {
            bΔ7.AddBytes(slice<byte>(Ꮡs.Value.alpnProtocol));
        });
    }
    if (s.isClient) {
        if (s.version >= VersionTLS13) {
            addUint64(Ꮡb, s.useBy);
            b.AddUint32(s.ageAdd);
        }
    }
    return b.Bytes();
}

internal static slice<slice<byte>> certificatesToBytesSlice(slice<ж<Δx509.Certificate>> certs) {
    var s = new slice<slice<byte>>(0, len(certs));
    foreach (var (_, c) in certs) {
        s = append(s, (~c).Raw);
    }
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInvalidSessionˢ = "tls: invalid session encoding"u8;
internal static readonly @string tlsNoServerCertificatesˢ = "tls: no server certificates in client session"u8;

// ParseSessionState parses a [SessionState] encoded by [SessionState.Bytes].
public static (ж<SessionState>, error) ParseSessionState(slice<byte> data) {
    var ss = Ꮡ(new SessionState(nil));
    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
    s = ((cryptobyte.String)data);
    ref var typ = ref heap(new uint8(), out var Ꮡtyp);
    ref var extMasterSecret = ref heap(new uint8(), out var ᏑextMasterSecret);
    ref var earlyData = ref heap(new uint8(), out var ᏑearlyData);
    ref var cert = ref heap(new Certificate(), out var Ꮡcert);
    ref var extra = ref heap<cryptobyte.String>(out var Ꮡextra);
    if (!s.ReadUint16(ss.of(SessionState.Ꮡversion)) || !s.ReadUint8(Ꮡtyp) || (typ != 1 && typ != 2) || !s.ReadUint16(ss.of(SessionState.ᏑcipherSuite)) || !readUint64(Ꮡs, ref (ss.of(SessionState.ᏑcreatedAt)).DerefOrNull()) || !readUint8LengthPrefixed(Ꮡs, ss.of(SessionState.Ꮡsecret)) || !s.ReadUint24LengthPrefixed(Ꮡextra) || !s.ReadUint8(ᏑextMasterSecret) || !s.ReadUint8(ᏑearlyData) || len((~ss).secret) == 0 || !unmarshalCertificate(Ꮡs, Ꮡcert)) {
        return (default!, errors.New(tlsInvalidSessionˢ));
    }
    while (!extra.Empty()) {
        ref var e = ref heap<slice<byte>>(out var Ꮡe);
        if (!readUint24LengthPrefixed(Ꮡextra, Ꮡe)) {
            return (default!, errors.New(tlsInvalidSessionˢ));
        }
        ss.Value.Extra = append((~ss).Extra, e);
    }
    switch (extMasterSecret) {
    case 0: {
        ss.Value.extMasterSecret = false;
        break;
    }
    case 1: {
        ss.Value.extMasterSecret = true;
        break;
    }
    default: {
        return (default!, errors.New(tlsInvalidSessionˢ));
    }}

    switch (earlyData) {
    case 0: {
        ss.Value.EarlyData = false;
        break;
    }
    case 1: {
        ss.Value.EarlyData = true;
        break;
    }
    default: {
        return (default!, errors.New(tlsInvalidSessionˢ));
    }}

    foreach (var (_, certΔ1) in cert.ΔCertificate) {
        var (c, err) = globalCertCache.newCert(certΔ1);
        if (err != default!) {
            return (default!, err);
        }
        ss.Value.activeCertHandles = append((~ss).activeCertHandles, c);
        ss.Value.peerCertificates = append((~ss).peerCertificates, (~c).cert);
    }
    ss.Value.ocspResponse = cert.OCSPStaple;
    ss.Value.scts = cert.SignedCertificateTimestamps;
    ref var chainList = ref heap<cryptobyte.String>(out var ᏑchainList);
    if (!s.ReadUint24LengthPrefixed(ᏑchainList)) {
        return (default!, errors.New(tlsInvalidSessionˢ));
    }
    while (!chainList.Empty()) {
        ref var certList = ref heap<cryptobyte.String>(out var ᏑcertList);
        if (!chainList.ReadUint24LengthPrefixed(ᏑcertList)) {
            return (default!, errors.New(tlsInvalidSessionˢ));
        }
        slice<ж<Δx509.Certificate>> chain = default!;
        if (len((~ss).peerCertificates) == 0) {
            return (default!, errors.New(tlsInvalidSessionˢ));
        }
        chain = append(chain, (~ss).peerCertificates[0]);
        while (!certList.Empty()) {
            ref var certΔ2 = ref heap<slice<byte>>(out var ᏑcertΔ2);
            if (!readUint24LengthPrefixed(ᏑcertList, ᏑcertΔ2)) {
                return (default!, errors.New(tlsInvalidSessionˢ));
            }
            var (c, err) = globalCertCache.newCert(certΔ2);
            if (err != default!) {
                return (default!, err);
            }
            ss.Value.activeCertHandles = append((~ss).activeCertHandles, c);
            chain = append(chain, (~c).cert);
        }
        ss.Value.verifiedChains = append((~ss).verifiedChains, chain);
    }
    if ((~ss).EarlyData) {
        ref var alpn = ref heap<slice<byte>>(out var Ꮡalpn);
        if (!readUint8LengthPrefixed(Ꮡs, Ꮡalpn)) {
            return (default!, errors.New(tlsInvalidSessionˢ));
        }
        ss.Value.alpnProtocol = ((@string)alpn);
    }
    {
        var isClient = typ == 2; if (!isClient) {
            if (!s.Empty()) {
                return (default!, errors.New(tlsInvalidSessionˢ));
            }
            return (ss, default!);
        }
    }
    ss.Value.isClient = true;
    if (len((~ss).peerCertificates) == 0) {
        return (default!, errors.New(tlsNoServerCertificatesˢ));
    }
    if ((~ss).version < VersionTLS13) {
        if (!s.Empty()) {
            return (default!, errors.New(tlsInvalidSessionˢ));
        }
        return (ss, default!);
    }
    if (!s.ReadUint64(ss.of(SessionState.ᏑuseBy)) || !s.ReadUint32(ss.of(SessionState.ᏑageAdd)) || !s.Empty()) {
        return (default!, errors.New(tlsInvalidSessionˢ));
    }
    return (ss, default!);
}

// sessionState returns a partially filled-out [SessionState] with information
// from the current connection.
[GoRecv] internal static ж<SessionState> sessionState(this ref Conn c) {
    return Ꮡ(new SessionState(
        version: c.vers,
        cipherSuite: c.cipherSuite,
        createdAt: (uint64)c.config.time().Unix(),
        alpnProtocol: c.clientProtocol,
        peerCertificates: c.peerCertificates,
        activeCertHandles: c.activeCertHandles,
        ocspResponse: c.ocspResponse,
        scts: c.scts,
        isClient: c.isClient,
        extMasterSecret: c.extMasterSecret,
        verifiedChains: c.verifiedChains
    ));
}

// EncryptTicket encrypts a ticket with the [Config]'s configured (or default)
// session ticket keys. It can be used as a [Config.WrapSession] implementation.
public static (slice<byte>, error) EncryptTicket(this ж<Config> Ꮡc, ΔConnectionState cs, ж<SessionState> Ꮡss) {
    ref var c = ref Ꮡc.DerefOrNull();

    var ticketKeys = Ꮡc.ticketKeys(nil);
    var (stateBytes, err) = Ꮡss.Bytes();
    if (err != default!) {
        return (default!, err);
    }
    return c.encryptTicket(stateBytes, ticketKeys);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInternalErrorSessionˢ = "tls: internal error: session ticket keys unavailable"u8;

[GoRecv] internal static (slice<byte>, error) encryptTicket(this ref Config c, slice<byte> state, slice<ticketKey> ticketKeys) {
    if (len(ticketKeys) == 0) {
        return (default!, errors.New(tlsInternalErrorSessionˢ));
    }
    var encrypted = new slice<byte>((nint)aes.BlockSize + len(state) + (nint)sha256.Size);
    var iv = encrypted[..(int)(aes.BlockSize)];
    var ciphertext = encrypted.slice(aes.BlockSize, len(encrypted) - (nint)sha256.Size);
    var authenticated = encrypted.slice(0, len(encrypted) - (nint)sha256.Size);
    var macBytes = encrypted.slice(len(encrypted) - (nint)sha256.Size);
    {
        var (_, errΔ1) = io.ReadFull(c.rand(), iv); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    var key = ticketKeys[0].ΔClone();
    var (block, err) = aes.NewCipher(key.aesKey[..]);
    if (err != default!) {
        return (default!, errors.New("tls: failed to create cipher while encrypting ticket: "u8 + err.Error()));
    }
    cipher.NewCTR(block, iv).XORKeyStream(ciphertext, state);
    var mac = hmac.New(sha256.New, key.hmacKey[..]);
    mac.Write(authenticated);
    mac.Sum(macBytes[..0]);
    return (encrypted, default!);
}

// DecryptTicket decrypts a ticket encrypted by [Config.EncryptTicket]. It can
// be used as a [Config.UnwrapSession] implementation.
//
// If the ticket can't be decrypted or parsed, DecryptTicket returns (nil, nil).
public static (ж<SessionState>, error) DecryptTicket(this ж<Config> Ꮡc, slice<byte> identity, ΔConnectionState cs) {
    ref var c = ref Ꮡc.DerefOrNull();

    var ticketKeys = Ꮡc.ticketKeys(nil);
    var stateBytes = c.decryptTicket(identity, ticketKeys);
    if (stateBytes == default!) {
        return (default!, default!);
    }
    var (s, err) = ParseSessionState(stateBytes);
    if (err != default!) {
        return (default!, default!); // drop unparsable tickets on the floor
    }
    return (s, default!);
}

[GoRecv] internal static slice<byte> decryptTicket(this ref Config c, slice<byte> encrypted, slice<ticketKey> ticketKeys) {
    if (len(encrypted) < (nint)(aes.BlockSize + sha256.Size)) {
        return default!;
    }
    var iv = encrypted[..(int)(aes.BlockSize)];
    var ciphertext = encrypted.slice(aes.BlockSize, len(encrypted) - (nint)sha256.Size);
    var authenticated = encrypted.slice(0, len(encrypted) - (nint)sha256.Size);
    var macBytes = encrypted.slice(len(encrypted) - (nint)sha256.Size);
    foreach (var (_, vᴛ1) in ticketKeys) {
        var key = vᴛ1.ΔClone();

        var mac = hmac.New(sha256.New, key.hmacKey[..]);
        mac.Write(authenticated);
        var expected = mac.Sum(default!);
        if (subtle.ConstantTimeCompare(macBytes, expected) != 1) {
            continue;
        }
        var (block, err) = aes.NewCipher(key.aesKey[..]);
        if (err != default!) {
            return default!;
        }
        var plaintext = new slice<byte>(len(ciphertext));
        cipher.NewCTR(block, iv).XORKeyStream(plaintext, ciphertext);
        return plaintext;
    }
    return default!;
}

// ClientSessionState contains the state needed by a client to
// resume a previous TLS session.
[GoType] partial struct ClientSessionState {
    internal ж<SessionState> session;
}

// ResumptionState returns the session ticket sent by the server (also known as
// the session's identity) and the state necessary to resume this session.
//
// It can be called by [ClientSessionCache.Put] to serialize (with
// [SessionState.Bytes]) and store the session.
public static (slice<byte> ticket, ж<SessionState> state, error err) ResumptionState(this ж<ClientSessionState> Ꮡcs) {
    ref var cs = ref Ꮡcs.DerefOrNull();

    if (Ꮡcs == nil || cs.session == nil) {
        return (default!, default!, default!);
    }
    return ((~cs.session).ticket, cs.session, default!);
}

// NewResumptionState returns a state value that can be returned by
// [ClientSessionCache.Get] to resume a previous session.
//
// state needs to be returned by [ParseSessionState], and the ticket and session
// state must have been returned by [ClientSessionState.ResumptionState].
public static (ж<ClientSessionState>, error) NewResumptionState(slice<byte> ticket, ж<SessionState> Ꮡstate) {
    ref var state = ref Ꮡstate.DerefOrNull();

    state.ticket = ticket;
    return (Ꮡ(new ClientSessionState(
        session: Ꮡstate
    )), default!);
}

} // end tls_package
