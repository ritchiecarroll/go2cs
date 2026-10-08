// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using list = container.list_package;
using context = context_package;
using crypto = crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using elliptic = go.crypto.elliptic_package;
using rand = go.crypto.rand_package;
using rsa = go.crypto.rsa_package;
using sha512 = go.crypto.sha512_package;
using fips140tls = go.crypto.tls.@internal.fips140tls_package;
using Δx509 = go.crypto.x509_package;
using errors = errors_package;
using fmt = fmt_package;
using godebug = go.@internal.godebug_package;
using io = io_package;
using net = net_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using container;
using go.@internal;
using go.crypto;
using go.crypto.tls.@internal;
using math;

partial class tls_package {

public static UntypedInt VersionTLS10 => 0x0301;
public static UntypedInt VersionTLS11 => 0x0302;
public static UntypedInt VersionTLS12 => 0x0303;
public static UntypedInt VersionTLS13 => 0x0304;
public static UntypedInt VersionSSL30 => 0x0300;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ssLv3ˢ = "SSLv3"u8;
internal static readonly @string tls10ˢ = "TLS 1.0"u8;
internal static readonly @string tls11ˢ = "TLS 1.1"u8;
internal static readonly @string tls12ˢ = "TLS 1.2"u8;
internal static readonly @string tls13ˢ = "TLS 1.3"u8;

// VersionName returns the name for the provided TLS version number
// (e.g. "TLS 1.3"), or a fallback representation of the value if the
// version is not implemented by this package.
public static @string VersionName(uint16 version) {
    var exprᴛ1 = version;
    if (exprᴛ1 == VersionSSL30) {
        return ssLv3ˢ;
    }
    if (exprᴛ1 == VersionTLS10) {
        return tls10ˢ;
    }
    if (exprᴛ1 == VersionTLS11) {
        return tls11ˢ;
    }
    if (exprᴛ1 == VersionTLS12) {
        return tls12ˢ;
    }
    if (exprᴛ1 == VersionTLS13) {
        return tls13ˢ;
    }
    { /* default: */
        return fmt.Sprintf("0x%04X"u8, version);
    }

}

internal static UntypedInt maxPlaintext => 16384; // maximum plaintext payload length
internal static UntypedInt maxCiphertext => /* 16384 + 2048 */ 18432; // maximum ciphertext payload length
internal static UntypedInt maxCiphertextTLS13 => /* 16384 + 256 */ 16640; // maximum ciphertext length in TLS 1.3
internal static UntypedInt recordHeaderLen => 5; // record header length
internal static UntypedInt maxHandshake => 65536; // maximum handshake we support (protocol max is 16 MB)
internal static UntypedInt maxHandshakeCertificateMsg => 262144; // maximum certificate message size (256 KiB)
internal static UntypedInt maxUselessRecords => 16; // maximum number of consecutive non-advancing records

partial struct recordType /*num:uint8*/;

internal static recordType recordTypeChangeCipherSpec => 20;
internal static recordType recordTypeAlert => 21;
internal static recordType recordTypeHandshake => 22;
internal static recordType recordTypeApplicationData => 23;

// TLS handshake message types.
internal const uint8 typeHelloRequest = 0;

internal const uint8 typeClientHello = 1;

internal const uint8 typeServerHello = 2;

internal const uint8 typeNewSessionTicket = 4;

internal const uint8 typeEndOfEarlyData = 5;

internal const uint8 typeEncryptedExtensions = 8;

internal const uint8 typeCertificate = 11;

internal const uint8 typeServerKeyExchange = 12;

internal const uint8 typeCertificateRequest = 13;

internal const uint8 typeServerHelloDone = 14;

internal const uint8 typeCertificateVerify = 15;

internal const uint8 typeClientKeyExchange = 16;

internal const uint8 typeFinished = 20;

internal const uint8 typeCertificateStatus = 22;

internal const uint8 typeKeyUpdate = 24;

internal const uint8 typeMessageHash = 254;       // synthetic message

// TLS compression types.
internal const uint8 compressionNone = 0;

// TLS extension numbers
internal const uint16 extensionServerName = 0;

internal const uint16 extensionStatusRequest = 5;

internal const uint16 extensionSupportedCurves = 10;      // supported_groups in TLS 1.3, see RFC 8446, Section 4.2.7

internal const uint16 extensionSupportedPoints = 11;

internal const uint16 extensionSignatureAlgorithms = 13;

internal const uint16 extensionALPN = 16;

internal const uint16 extensionSCT = 18;

internal const uint16 extensionExtendedMasterSecret = 23;

internal const uint16 extensionSessionTicket = 35;

internal const uint16 extensionPreSharedKey = 41;

internal const uint16 extensionEarlyData = 42;

internal const uint16 extensionSupportedVersions = 43;

internal const uint16 extensionCookie = 44;

internal const uint16 extensionPSKModes = 45;

internal const uint16 extensionCertificateAuthorities = 47;

internal const uint16 extensionSignatureAlgorithmsCert = 50;

internal const uint16 extensionKeyShare = 51;

internal const uint16 extensionQUICTransportParameters = 57;

internal const uint16 extensionRenegotiationInfo = 0xff01;

internal const uint16 extensionECHOuterExtensions = 0xfd00;

internal const uint16 extensionEncryptedClientHello = 0xfe0d;

// TLS signaling cipher suite values
internal const uint16 scsvRenegotiation = 0x00ff;

partial struct CurveID /*num:uint16*/;

public static CurveID CurveP256 => 23;
public static CurveID CurveP384 => 24;
public static CurveID CurveP521 => 25;
public static CurveID X25519 => 29;
public static CurveID X25519MLKEM768 => 4588;

internal static bool isTLS13OnlyKeyExchange(CurveID curve) {
    return curve == X25519MLKEM768;
}

internal static bool isPQKeyExchange(CurveID curve) {
    return curve == X25519MLKEM768;
}

// TLS 1.3 Key Share. See RFC 8446, Section 4.2.8.
partial struct keyShare {
    internal CurveID group;
    internal slice<byte> data;
}

// TLS 1.3 PSK Key Exchange Modes. See RFC 8446, Section 4.2.9.
internal const uint8 pskModePlain = 0;

internal const uint8 pskModeDHE = 1;

// TLS 1.3 PSK Identity. Can be a Session Ticket, or a reference to a saved
// session. See RFC 8446, Section 4.2.11.
partial struct pskIdentity {
    internal slice<byte> label;
    internal uint32 obfuscatedTicketAge;
}

// TLS Elliptic Curve Point Formats
// https://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-9
internal const uint8 pointFormatUncompressed = 0;

// TLS CertificateStatusType (RFC 3546)
internal const uint8 statusTypeOCSP = 1;

// Certificate types (for certificateRequestMsg)
internal static UntypedInt certTypeRSASign => 1;

internal static UntypedInt certTypeECDSASign => 64; // ECDSA or EdDSA keys, see RFC 8422, Section 3.

// Signature algorithms (for internal signaling use). Starting at 225 to avoid overlap with
// TLS 1.2 codepoints (RFC 5246, Appendix A.4.1), with which these have nothing to do.
internal const uint8 signaturePKCS1v15 = /* iota + 225 */ 225;

internal const uint8 signatureRSAPSS = 226;

internal const uint8 signatureECDSA = 227;

internal const uint8 signatureEd25519 = 228;

// directSigning is a standard Hash value that signals that no pre-hashing
// should be performed, and that the input should be signed directly. It is the
// hash function associated with the Ed25519 signature scheme.
internal static crypto.Hash directSigning = 0;

// See RFC 8446, Section 4.1.3.
// helloRetryRequestRandom is set as the Random value of a ServerHello
// to signal that the message is actually a HelloRetryRequest.
internal static slice<byte> helloRetryRequestRandom = new byte[]{
    0xCF, 0x21, 0xAD, 0x74, 0xE5, 0x9A, 0x61, 0x11,
    0xBE, 0x1D, 0x8C, 0x02, 0x1E, 0x65, 0xB8, 0x91,
    0xC2, 0xA2, 0x11, 0x16, 0x7A, 0xBB, 0x8C, 0x5E,
    0x07, 0x9E, 0x09, 0xE2, 0xC8, 0xA8, 0x33, 0x9C
}.slice();

internal static readonly @string downgradeCanaryTLS12 = "DOWNGRD\x01"u8;
internal static readonly @string downgradeCanaryTLS11 = "DOWNGRD\x00"u8;

// testingOnlyForceDowngradeCanary is set in tests to force the server side to
// include downgrade canaries even if it's using its highers supported version.
internal static bool testingOnlyForceDowngradeCanary;

// ConnectionState records basic TLS details about the connection.
partial struct ΔConnectionState {
    // Version is the TLS version used by the connection (e.g. VersionTLS12).
    public uint16 Version;
    // HandshakeComplete is true if the handshake has concluded.
    public bool HandshakeComplete;
    // DidResume is true if this connection was successfully resumed from a
    // previous session with a session ticket or similar mechanism.
    public bool DidResume;
    // CipherSuite is the cipher suite negotiated for the connection (e.g.
    // TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, TLS_AES_128_GCM_SHA256).
    public uint16 CipherSuite;
    // NegotiatedProtocol is the application protocol negotiated with ALPN.
    public @string NegotiatedProtocol;
    // NegotiatedProtocolIsMutual used to indicate a mutual NPN negotiation.
    //
    // Deprecated: this value is always true.
    public bool NegotiatedProtocolIsMutual;
    // ServerName is the value of the Server Name Indication extension sent by
    // the client. It's available both on the server and on the client side.
    public @string ServerName;
    // PeerCertificates are the parsed certificates sent by the peer, in the
    // order in which they were sent. The first element is the leaf certificate
    // that the connection is verified against.
    //
    // On the client side, it can't be empty. On the server side, it can be
    // empty if Config.ClientAuth is not RequireAnyClientCert or
    // RequireAndVerifyClientCert.
    //
    // PeerCertificates and its contents should not be modified.
    public slice<ж<Δx509.Certificate>> PeerCertificates;
    // VerifiedChains is a list of one or more chains where the first element is
    // PeerCertificates[0] and the last element is from Config.RootCAs (on the
    // client side) or Config.ClientCAs (on the server side).
    //
    // On the client side, it's set if Config.InsecureSkipVerify is false. On
    // the server side, it's set if Config.ClientAuth is VerifyClientCertIfGiven
    // (and the peer provided a certificate) or RequireAndVerifyClientCert.
    //
    // VerifiedChains and its contents should not be modified.
    public slice<slice<ж<Δx509.Certificate>>> VerifiedChains;
    // SignedCertificateTimestamps is a list of SCTs provided by the peer
    // through the TLS handshake for the leaf certificate, if any.
    public slice<slice<byte>> SignedCertificateTimestamps;
    // OCSPResponse is a stapled Online Certificate Status Protocol (OCSP)
    // response provided by the peer for the leaf certificate, if any.
    public slice<byte> OCSPResponse;
    // TLSUnique contains the "tls-unique" channel binding value (see RFC 5929,
    // Section 3). This value will be nil for TLS 1.3 connections and for
    // resumed connections that don't support Extended Master Secret (RFC 7627).
    public slice<byte> TLSUnique;
    // ECHAccepted indicates if Encrypted Client Hello was offered by the client
    // and accepted by the server. Currently, ECH is supported only on the
    // client side.
    public bool ECHAccepted;
    // ekm is a closure exposed via ExportKeyingMaterial.
    internal Func<@string, slice<byte>, nint, (slice<byte>, error)> ekm;
    // testingOnlyDidHRR is true if a HelloRetryRequest was sent/received.
    internal bool testingOnlyDidHRR;
    // testingOnlyCurveID is the selected CurveID, or zero if an RSA exchanges
    // is performed.
    internal CurveID testingOnlyCurveID;
}

// ExportKeyingMaterial returns length bytes of exported key material in a new
// slice as defined in RFC 5705. If context is nil, it is not used as part of
// the seed. If the connection was set to allow renegotiation via
// Config.Renegotiation, or if the connections supports neither TLS 1.3 nor
// Extended Master Secret, this function will return an error.
//
// Exporting key material without Extended Master Secret or TLS 1.3 was disabled
// in Go 1.22 due to security issues (see the Security Considerations sections
// of RFC 5705 and RFC 7627), but can be re-enabled with the GODEBUG setting
// tlsunsafeekm=1.
public static (slice<byte>, error) ExportKeyingMaterial(this ref ΔConnectionState cs, @string label, slice<byte> context, nint length) {
    return cs.ekm(label, context, length);
}

partial struct ClientAuthType /*num:nint*/;

public static ClientAuthType NoClientCert => /* iota */ 0;
public static ClientAuthType RequestClientCert => 1;
public static ClientAuthType RequireAnyClientCert => 2;
public static ClientAuthType VerifyClientCertIfGiven => 3;
public static ClientAuthType RequireAndVerifyClientCert => 4;

// requiresClientCert reports whether the ClientAuthType requires a client
// certificate to be provided.
internal static bool requiresClientCert(ClientAuthType c) {
    var exprᴛ1 = c;
    if (exprᴛ1 == RequireAnyClientCert || exprᴛ1 == RequireAndVerifyClientCert) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// ClientSessionCache is a cache of ClientSessionState objects that can be used
// by a client to resume a TLS session with a given server. ClientSessionCache
// implementations should expect to be called concurrently from different
// goroutines. Up to TLS 1.2, only ticket-based resumption is supported, not
// SessionID-based resumption. In TLS 1.3 they were merged into PSK modes, which
// are supported via this interface.
partial interface ClientSessionCache {
    // Get searches for a ClientSessionState associated with the given key.
    // On return, ok is true if one was found.
    (ж<ClientSessionState> session, bool ok) Get(@string sessionKey);
    // Put adds the ClientSessionState to the cache with the given key. It might
    // get called multiple times in a connection if a TLS 1.3 server provides
    // more than one session ticket. If called with a nil *ClientSessionState,
    // it should remove the cache entry.
    void Put(@string sessionKey, ж<ClientSessionState> cs);
}

partial struct SignatureScheme /*num:uint16*/;

//go:generate stringer -linecomment -type=SignatureScheme,CurveID,ClientAuthType -output=common_string.go
public static SignatureScheme PKCS1WithSHA256 => 0x0401;
public static SignatureScheme PKCS1WithSHA384 => 0x0501;
public static SignatureScheme PKCS1WithSHA512 => 0x0601;
public static SignatureScheme PSSWithSHA256 => 0x0804;
public static SignatureScheme PSSWithSHA384 => 0x0805;
public static SignatureScheme PSSWithSHA512 => 0x0806;
public static SignatureScheme ECDSAWithP256AndSHA256 => 0x0403;
public static SignatureScheme ECDSAWithP384AndSHA384 => 0x0503;
public static SignatureScheme ECDSAWithP521AndSHA512 => 0x0603;
public static SignatureScheme Ed25519 => 0x0807;
public static SignatureScheme PKCS1WithSHA1 => 0x0201;
public static SignatureScheme ECDSAWithSHA1 => 0x0203;

// ClientHelloInfo contains information from a ClientHello message in order to
// guide application logic in the GetCertificate and GetConfigForClient callbacks.
partial struct ClientHelloInfo {
    // CipherSuites lists the CipherSuites supported by the client (e.g.
    // TLS_AES_128_GCM_SHA256, TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256).
    public slice<uint16> CipherSuites;
    // ServerName indicates the name of the server requested by the client
    // in order to support virtual hosting. ServerName is only set if the
    // client is using SNI (see RFC 4366, Section 3.1).
    public @string ServerName;
    // SupportedCurves lists the key exchange mechanisms supported by the
    // client. It was renamed to "supported groups" in TLS 1.3, see RFC 8446,
    // Section 4.2.7 and [CurveID].
    //
    // SupportedCurves may be nil in TLS 1.2 and lower if the Supported Elliptic
    // Curves Extension is not being used (see RFC 4492, Section 5.1.1).
    public slice<CurveID> SupportedCurves;
    // SupportedPoints lists the point formats supported by the client.
    // SupportedPoints is set only if the Supported Point Formats Extension
    // is being used (see RFC 4492, Section 5.1.2).
    public slice<uint8> SupportedPoints;
    // SignatureSchemes lists the signature and hash schemes that the client
    // is willing to verify. SignatureSchemes is set only if the Signature
    // Algorithms Extension is being used (see RFC 5246, Section 7.4.1.4.1).
    public slice<SignatureScheme> SignatureSchemes;
    // SupportedProtos lists the application protocols supported by the client.
    // SupportedProtos is set only if the Application-Layer Protocol
    // Negotiation Extension is being used (see RFC 7301, Section 3.1).
    //
    // Servers can select a protocol by setting Config.NextProtos in a
    // GetConfigForClient return value.
    public slice<@string> SupportedProtos;
    // SupportedVersions lists the TLS versions supported by the client.
    // For TLS versions less than 1.3, this is extrapolated from the max
    // version advertised by the client, so values other than the greatest
    // might be rejected if used.
    public slice<uint16> SupportedVersions;
    // Extensions lists the IDs of the extensions presented by the client
    // in the ClientHello.
    public slice<uint16> Extensions;
    // Conn is the underlying net.Conn for the connection. Do not read
    // from, or write to, this connection; that will cause the TLS
    // connection to fail.
    public net.Conn Conn;
    // config is embedded by the GetCertificate or GetConfigForClient caller,
    // for use with SupportsCertificate.
    internal ж<Config> config;
    // ctx is the context of the handshake that is in progress.
    internal context.Context ctx;
}

// Context returns the context of the handshake that is in progress.
// This context is a child of the context passed to HandshakeContext,
// if any, and is canceled when the handshake concludes.
public static context.Context Context(this ref ClientHelloInfo c) {
    return c.ctx;
}

// CertificateRequestInfo contains information from a server's
// CertificateRequest message, which is used to demand a certificate and proof
// of control from a client.
partial struct CertificateRequestInfo {
    // AcceptableCAs contains zero or more, DER-encoded, X.501
    // Distinguished Names. These are the names of root or intermediate CAs
    // that the server wishes the returned certificate to be signed by. An
    // empty slice indicates that the server has no preference.
    public slice<slice<byte>> AcceptableCAs;
    // SignatureSchemes lists the signature schemes that the server is
    // willing to verify.
    public slice<SignatureScheme> SignatureSchemes;
    // Version is the TLS version that was negotiated for this connection.
    public uint16 Version;
    // ctx is the context of the handshake that is in progress.
    internal context.Context ctx;
}

// Context returns the context of the handshake that is in progress.
// This context is a child of the context passed to HandshakeContext,
// if any, and is canceled when the handshake concludes.
public static context.Context Context(this ref CertificateRequestInfo c) {
    return c.ctx;
}

partial struct RenegotiationSupport /*num:nint*/;

public static RenegotiationSupport RenegotiateNever => /* iota */ 0;
public static RenegotiationSupport RenegotiateOnceAsClient => 1;
public static RenegotiationSupport RenegotiateFreelyAsClient => 2;

// A Config structure is used to configure a TLS client or server.
// After one has been passed to a TLS function it must not be
// modified. A Config may be reused; the tls package will also not
// modify it.
partial struct Config {
    // Rand provides the source of entropy for nonces and RSA blinding.
    // If Rand is nil, TLS uses the cryptographic random reader in package
    // crypto/rand.
    // The Reader must be safe for use by multiple goroutines.
    public io.Reader Rand;
    // Time returns the current time as the number of seconds since the epoch.
    // If Time is nil, TLS uses time.Now.
    public Func<time.Time> Time;
    // Certificates contains one or more certificate chains to present to the
    // other side of the connection. The first certificate compatible with the
    // peer's requirements is selected automatically.
    //
    // Server configurations must set one of Certificates, GetCertificate or
    // GetConfigForClient. Clients doing client-authentication may set either
    // Certificates or GetClientCertificate.
    //
    // Note: if there are multiple Certificates, and they don't have the
    // optional field Leaf set, certificate selection will incur a significant
    // per-handshake performance cost.
    public slice<Certificate> Certificates;
    // NameToCertificate maps from a certificate name to an element of
    // Certificates. Note that a certificate name can be of the form
    // '*.example.com' and so doesn't have to be a domain name as such.
    //
    // Deprecated: NameToCertificate only allows associating a single
    // certificate with a given name. Leave this field nil to let the library
    // select the first compatible chain from Certificates.
    public map<@string, ж<Certificate>> NameToCertificate;
    // GetCertificate returns a Certificate based on the given
    // ClientHelloInfo. It will only be called if the client supplies SNI
    // information or if Certificates is empty.
    //
    // If GetCertificate is nil or returns nil, then the certificate is
    // retrieved from NameToCertificate. If NameToCertificate is nil, the
    // best element of Certificates will be used.
    //
    // Once a Certificate is returned it should not be modified.
    public Func<ж<ClientHelloInfo>, (ж<Certificate>, error)> GetCertificate;
    // GetClientCertificate, if not nil, is called when a server requests a
    // certificate from a client. If set, the contents of Certificates will
    // be ignored.
    //
    // If GetClientCertificate returns an error, the handshake will be
    // aborted and that error will be returned. Otherwise
    // GetClientCertificate must return a non-nil Certificate. If
    // Certificate.Certificate is empty then no certificate will be sent to
    // the server. If this is unacceptable to the server then it may abort
    // the handshake.
    //
    // GetClientCertificate may be called multiple times for the same
    // connection if renegotiation occurs or if TLS 1.3 is in use.
    //
    // Once a Certificate is returned it should not be modified.
    public Func<ж<CertificateRequestInfo>, (ж<Certificate>, error)> GetClientCertificate;
    // GetConfigForClient, if not nil, is called after a ClientHello is
    // received from a client. It may return a non-nil Config in order to
    // change the Config that will be used to handle this connection. If
    // the returned Config is nil, the original Config will be used. The
    // Config returned by this callback may not be subsequently modified.
    //
    // If GetConfigForClient is nil, the Config passed to Server() will be
    // used for all connections.
    //
    // If SessionTicketKey is explicitly set on the returned Config, or if
    // SetSessionTicketKeys is called on the returned Config, those keys will
    // be used. Otherwise, the original Config keys will be used (and possibly
    // rotated if they are automatically managed). WARNING: this allows session
    // resumtion of connections originally established with the parent (or a
    // sibling) Config, which may bypass the [Config.VerifyPeerCertificate]
    // value of the returned Config.
    public Func<ж<ClientHelloInfo>, (ж<Config>, error)> GetConfigForClient;
    // VerifyPeerCertificate, if not nil, is called after normal
    // certificate verification by either a TLS client or server. It
    // receives the raw ASN.1 certificates provided by the peer and also
    // any verified chains that normal processing found. If it returns a
    // non-nil error, the handshake is aborted and that error results.
    //
    // If normal verification fails then the handshake will abort before
    // considering this callback. If normal verification is disabled (on the
    // client when InsecureSkipVerify is set, or on a server when ClientAuth is
    // RequestClientCert or RequireAnyClientCert), then this callback will be
    // considered but the verifiedChains argument will always be nil. When
    // ClientAuth is NoClientCert, this callback is not called on the server.
    // rawCerts may be empty on the server if ClientAuth is RequestClientCert or
    // VerifyClientCertIfGiven.
    //
    // This callback is not invoked on resumed connections. WARNING: this
    // includes connections resumed across Configs returned by [Config.Clone] or
    // [Config.GetConfigForClient] and their parents. If that is not intended,
    // use [Config.VerifyConnection] instead, or set [Config.SessionTicketsDisabled].
    //
    // verifiedChains and its contents should not be modified.
    public Func<slice<slice<byte>>, slice<slice<ж<Δx509.Certificate>>>, error> VerifyPeerCertificate;
    // VerifyConnection, if not nil, is called after normal certificate
    // verification and after VerifyPeerCertificate by either a TLS client
    // or server. If it returns a non-nil error, the handshake is aborted
    // and that error results.
    //
    // If normal verification fails then the handshake will abort before
    // considering this callback. This callback will run for all connections,
    // including resumptions, regardless of InsecureSkipVerify or ClientAuth
    // settings.
    public Func<ΔConnectionState, error> VerifyConnection;
    // RootCAs defines the set of root certificate authorities
    // that clients use when verifying server certificates.
    // If RootCAs is nil, TLS uses the host's root CA set.
    public ж<Δx509.CertPool> RootCAs;
    // NextProtos is a list of supported application level protocols, in
    // order of preference. If both peers support ALPN, the selected
    // protocol will be one from this list, and the connection will fail
    // if there is no mutually supported protocol. If NextProtos is empty
    // or the peer doesn't support ALPN, the connection will succeed and
    // ConnectionState.NegotiatedProtocol will be empty.
    public slice<@string> NextProtos;
    // ServerName is used to verify the hostname on the returned
    // certificates unless InsecureSkipVerify is given. It is also included
    // in the client's handshake to support virtual hosting unless it is
    // an IP address.
    public @string ServerName;
    // ClientAuth determines the server's policy for
    // TLS Client Authentication. The default is NoClientCert.
    public ClientAuthType ClientAuth;
    // ClientCAs defines the set of root certificate authorities
    // that servers use if required to verify a client certificate
    // by the policy in ClientAuth.
    public ж<Δx509.CertPool> ClientCAs;
    // InsecureSkipVerify controls whether a client verifies the server's
    // certificate chain and host name. If InsecureSkipVerify is true, crypto/tls
    // accepts any certificate presented by the server and any host name in that
    // certificate. In this mode, TLS is susceptible to machine-in-the-middle
    // attacks unless custom verification is used. This should be used only for
    // testing or in combination with VerifyConnection or VerifyPeerCertificate.
    public bool InsecureSkipVerify;
    // CipherSuites is a list of enabled TLS 1.0–1.2 cipher suites. The order of
    // the list is ignored. Note that TLS 1.3 ciphersuites are not configurable.
    //
    // If CipherSuites is nil, a safe default list is used. The default cipher
    // suites might change over time. In Go 1.22 RSA key exchange based cipher
    // suites were removed from the default list, but can be re-added with the
    // GODEBUG setting tlsrsakex=1. In Go 1.23 3DES cipher suites were removed
    // from the default list, but can be re-added with the GODEBUG setting
    // tls3des=1.
    public slice<uint16> CipherSuites;
    // PreferServerCipherSuites is a legacy field and has no effect.
    //
    // It used to control whether the server would follow the client's or the
    // server's preference. Servers now select the best mutually supported
    // cipher suite based on logic that takes into account inferred client
    // hardware, server hardware, and security.
    //
    // Deprecated: PreferServerCipherSuites is ignored.
    public bool PreferServerCipherSuites;
    // SessionTicketsDisabled may be set to true to disable session ticket and
    // PSK (resumption) support. Note that on clients, session ticket support is
    // also disabled if ClientSessionCache is nil.
    public bool SessionTicketsDisabled;
    // SessionTicketKey is used by TLS servers to provide session resumption.
    // See RFC 5077 and the PSK mode of RFC 8446. If zero, it will be filled
    // with random data before the first server handshake.
    //
    // Deprecated: if this field is left at zero, session ticket keys will be
    // automatically rotated every day and dropped after seven days. For
    // customizing the rotation schedule or synchronizing servers that are
    // terminating connections for the same host, use SetSessionTicketKeys.
    public array<byte> SessionTicketKey = new(32);
    // ClientSessionCache is a cache of ClientSessionState entries for TLS
    // session resumption. It is only used by clients.
    public ClientSessionCache ClientSessionCache;
    // UnwrapSession is called on the server to turn a ticket/identity
    // previously produced by [WrapSession] into a usable session.
    //
    // UnwrapSession will usually either decrypt a session state in the ticket
    // (for example with [Config.EncryptTicket]), or use the ticket as a handle
    // to recover a previously stored state. It must use [ParseSessionState] to
    // deserialize the session state.
    //
    // If UnwrapSession returns an error, the connection is terminated. If it
    // returns (nil, nil), the session is ignored. crypto/tls may still choose
    // not to resume the returned session.
    public Func<slice<byte>, ΔConnectionState, (ж<SessionState>, error)> UnwrapSession;
    // WrapSession is called on the server to produce a session ticket/identity.
    //
    // WrapSession must serialize the session state with [SessionState.Bytes].
    // It may then encrypt the serialized state (for example with
    // [Config.DecryptTicket]) and use it as the ticket, or store the state and
    // return a handle for it.
    //
    // If WrapSession returns an error, the connection is terminated.
    //
    // Warning: the return value will be exposed on the wire and to clients in
    // plaintext. The application is in charge of encrypting and authenticating
    // it (and rotating keys) or returning high-entropy identifiers. Failing to
    // do so correctly can compromise current, previous, and future connections
    // depending on the protocol version.
    public Func<ΔConnectionState, ж<SessionState>, (slice<byte>, error)> WrapSession;
    // MinVersion contains the minimum TLS version that is acceptable.
    //
    // By default, TLS 1.2 is currently used as the minimum. TLS 1.0 is the
    // minimum supported by this package.
    //
    // The server-side default can be reverted to TLS 1.0 by including the value
    // "tls10server=1" in the GODEBUG environment variable.
    public uint16 MinVersion;
    // MaxVersion contains the maximum TLS version that is acceptable.
    //
    // By default, the maximum version supported by this package is used,
    // which is currently TLS 1.3.
    public uint16 MaxVersion;
    // CurvePreferences contains a set of supported key exchange mechanisms.
    // The name refers to elliptic curves for legacy reasons, see [CurveID].
    // The order of the list is ignored, and key exchange mechanisms are chosen
    // from this list using an internal preference order. If empty, the default
    // will be used.
    //
    // From Go 1.24, the default includes the [X25519MLKEM768] hybrid
    // post-quantum key exchange. To disable it, set CurvePreferences explicitly
    // or use the GODEBUG=tlsmlkem=0 environment variable.
    public slice<CurveID> CurvePreferences;
    // DynamicRecordSizingDisabled disables adaptive sizing of TLS records.
    // When true, the largest possible TLS record size is always used. When
    // false, the size of TLS records may be adjusted in an attempt to
    // improve latency.
    public bool DynamicRecordSizingDisabled;
    // Renegotiation controls what types of renegotiation are supported.
    // The default, none, is correct for the vast majority of applications.
    public RenegotiationSupport Renegotiation;
    // KeyLogWriter optionally specifies a destination for TLS master secrets
    // in NSS key log format that can be used to allow external programs
    // such as Wireshark to decrypt TLS connections.
    // See https://developer.mozilla.org/en-US/docs/Mozilla/Projects/NSS/Key_Log_Format.
    // Use of KeyLogWriter compromises security and should only be
    // used for debugging.
    public io.Writer KeyLogWriter;
    // EncryptedClientHelloConfigList is a serialized ECHConfigList. If
    // provided, clients will attempt to connect to servers using Encrypted
    // Client Hello (ECH) using one of the provided ECHConfigs.
    //
    // Servers do not use this field. In order to configure ECH for servers, see
    // the EncryptedClientHelloKeys field.
    //
    // If the list contains no valid ECH configs, the handshake will fail
    // and return an error.
    //
    // If EncryptedClientHelloConfigList is set, MinVersion, if set, must
    // be VersionTLS13.
    //
    // When EncryptedClientHelloConfigList is set, the handshake will only
    // succeed if ECH is successfully negotiated. If the server rejects ECH,
    // an ECHRejectionError error will be returned, which may contain a new
    // ECHConfigList that the server suggests using.
    //
    // How this field is parsed may change in future Go versions, if the
    // encoding described in the final Encrypted Client Hello RFC changes.
    public slice<byte> EncryptedClientHelloConfigList;
    // EncryptedClientHelloRejectionVerify, if not nil, is called when ECH is
    // rejected by the remote server, in order to verify the ECH provider
    // certificate in the outer ClientHello. If it returns a non-nil error, the
    // handshake is aborted and that error results.
    //
    // On the server side this field is not used.
    //
    // Unlike VerifyPeerCertificate and VerifyConnection, normal certificate
    // verification will not be performed before calling
    // EncryptedClientHelloRejectionVerify.
    //
    // If EncryptedClientHelloRejectionVerify is nil and ECH is rejected, the
    // roots in RootCAs will be used to verify the ECH providers public
    // certificate. VerifyPeerCertificate and VerifyConnection are not called
    // when ECH is rejected, even if set, and InsecureSkipVerify is ignored.
    public Func<ΔConnectionState, error> EncryptedClientHelloRejectionVerify;
    // EncryptedClientHelloKeys are the ECH keys to use when a client
    // attempts ECH.
    //
    // If EncryptedClientHelloKeys is set, MinVersion, if set, must be
    // VersionTLS13.
    //
    // If a client attempts ECH, but it is rejected by the server, the server
    // will send a list of configs to retry based on the set of
    // EncryptedClientHelloKeys which have the SendAsRetry field set.
    //
    // On the client side, this field is ignored. In order to configure ECH for
    // clients, see the EncryptedClientHelloConfigList field.
    public slice<EncryptedClientHelloKey> EncryptedClientHelloKeys;
    // mutex protects sessionTicketKeys and autoSessionTicketKeys.
    internal sync.RWMutex mutex;
    // sessionTicketKeys contains zero or more ticket keys. If set, it means
    // the keys were set with SessionTicketKey or SetSessionTicketKeys. The
    // first key is used for new tickets and any subsequent keys can be used to
    // decrypt old tickets. The slice contents are not protected by the mutex
    // and are immutable.
    internal slice<ticketKey> sessionTicketKeys;
    // autoSessionTicketKeys is like sessionTicketKeys but is owned by the
    // auto-rotation logic. See Config.ticketKeys.
    internal slice<ticketKey> autoSessionTicketKeys;
}

// EncryptedClientHelloKey holds a private key that is associated
// with a specific ECH config known to a client.
partial struct EncryptedClientHelloKey {
    // Config should be a marshalled ECHConfig associated with PrivateKey. This
    // must match the config provided to clients byte-for-byte. The config
    // should only specify the DHKEM(X25519, HKDF-SHA256) KEM ID (0x0020), the
    // HKDF-SHA256 KDF ID (0x0001), and a subset of the following AEAD IDs:
    // AES-128-GCM (0x0000), AES-256-GCM (0x0001), ChaCha20Poly1305 (0x0002).
    public slice<byte> Config;
    // PrivateKey should be a marshalled private key. Currently, we expect
    // this to be the output of [ecdh.PrivateKey.Bytes].
    public slice<byte> PrivateKey;
    // SendAsRetry indicates if Config should be sent as part of the list of
    // retry configs when ECH is requested by the client but rejected by the
    // server.
    public bool SendAsRetry;
}

internal static time.Duration ticketKeyLifetime => /* 7 * 24 * time.Hour */ 604800000000000; // 7 days
internal static time.Duration ticketKeyRotation => /* 24 * time.Hour */ 86400000000000;

// ticketKey is the internal representation of a session ticket key.
partial struct ticketKey {
    internal array<byte> aesKey = new(16);
    internal array<byte> hmacKey = new(16);
    // created is the time at which this ticket key was created. See Config.ticketKeys.
    internal time.Time created;
}

// ticketKeyFromBytes converts from the external representation of a session
// ticket key to a ticketKey. Externally, session ticket keys are 32 random
// bytes and this function expands that into sufficient name and key material.
internal static ticketKey /*key*/ ticketKeyFromBytes(this ref Config c, /*[32]*/ array<byte> b) {
    ticketKey key = new();

    b = b.Clone();
    var hashed = sha512.Sum512(b[..]);
    // The first 16 bytes of the hash used to be exposed on the wire as a ticket
    // prefix. They MUST NOT be used as a secret. In the future, it would make
    // sense to use a proper KDF here, like HKDF with a fixed salt.
    UntypedInt legacyTicketKeyNameLen = 16;
    copy(key.aesKey[..], hashed[(int)(legacyTicketKeyNameLen)..]);
    copy(key.hmacKey[..], hashed[(int)((nint)legacyTicketKeyNameLen + len(key.aesKey))..]);
    key.created = c.time();
    return key.ΔClone();
}

// maxSessionTicketLifetime is the maximum allowed lifetime of a TLS 1.3 session
// ticket, and the lifetime we set for all tickets we send.
internal static time.Duration maxSessionTicketLifetime => /* 7 * 24 * time.Hour */ 604800000000000;

// Clone returns a shallow clone of c or nil if c is nil. It is safe to clone a
// [Config] that is being used concurrently by a TLS client or server.
//
// The returned Config can share session ticket keys with the original Config,
// which means connections could be resumed across the two Configs. WARNING:
// [Config.VerifyPeerCertificate] does not get called on resumed connections,
// including connections that were originally established on the parent Config.
// If that is not intended, use [Config.VerifyConnection] instead, or set
// [Config.SessionTicketsDisabled].
public static ж<Config> Clone(this ж<Config> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (Ꮡc == nil) {
            return default!;
        }
        Ꮡc.of(Config.Ꮡmutex).RLock();
        ᒐd1 = true;
        return Ꮡ(new Config(
            Rand: c.Rand,
            Time: c.Time,
            Certificates: c.Certificates,
            NameToCertificate: c.NameToCertificate,
            GetCertificate: c.GetCertificate,
            GetClientCertificate: c.GetClientCertificate,
            GetConfigForClient: c.GetConfigForClient,
            VerifyPeerCertificate: c.VerifyPeerCertificate,
            VerifyConnection: c.VerifyConnection,
            RootCAs: c.RootCAs,
            NextProtos: c.NextProtos,
            ServerName: c.ServerName,
            ClientAuth: c.ClientAuth,
            ClientCAs: c.ClientCAs,
            InsecureSkipVerify: c.InsecureSkipVerify,
            CipherSuites: c.CipherSuites,
            PreferServerCipherSuites: c.PreferServerCipherSuites,
            SessionTicketsDisabled: c.SessionTicketsDisabled,
            SessionTicketKey: c.SessionTicketKey.Clone(),
            ClientSessionCache: c.ClientSessionCache,
            UnwrapSession: c.UnwrapSession,
            WrapSession: c.WrapSession,
            MinVersion: c.MinVersion,
            MaxVersion: c.MaxVersion,
            CurvePreferences: c.CurvePreferences,
            DynamicRecordSizingDisabled: c.DynamicRecordSizingDisabled,
            Renegotiation: c.Renegotiation,
            KeyLogWriter: c.KeyLogWriter,
            EncryptedClientHelloConfigList: c.EncryptedClientHelloConfigList,
            EncryptedClientHelloRejectionVerify: c.EncryptedClientHelloRejectionVerify,
            EncryptedClientHelloKeys: c.EncryptedClientHelloKeys,
            sessionTicketKeys: c.sessionTicketKeys,
            autoSessionTicketKeys: c.autoSessionTicketKeys
        ));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.of(Config.Ꮡmutex).RUnlock(); ᒐ.Run(); }
}

// deprecatedSessionTicketKey is set as the prefix of SessionTicketKey if it was
// randomized for backwards compatibility but is not in use.
internal static slice<byte> deprecatedSessionTicketKey = slice<byte>("DEPRECATED"u8);

// initLegacySessionTicketKeyRLocked ensures the legacy SessionTicketKey field is
// randomized if empty, and that sessionTicketKeys is populated from it otherwise.
internal static void initLegacySessionTicketKeyRLocked(this ж<Config> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    bool ᒐd2 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        // Don't write if SessionTicketKey is already defined as our deprecated string,
        // or if it is defined by the user but sessionTicketKeys is already set.
        if (c.SessionTicketKey != new byte[]{}.array(32) && (bytes.HasPrefix(c.SessionTicketKey[..], deprecatedSessionTicketKey) || len(c.sessionTicketKeys) > 0)) {
            return;
        }
        // We need to write some data, so get an exclusive lock and re-check any conditions.
        Ꮡc.of(Config.Ꮡmutex).RUnlock();
        ᒐd1 = true;
        Ꮡc.of(Config.Ꮡmutex).Lock();
        ᒐd2 = true;
        if (c.SessionTicketKey == new byte[]{}.array(32)){
            {
                var (_, err) = io.ReadFull(c.rand(), c.SessionTicketKey[..]); if (err != default!) {
                    throw panic(fmt.Sprintf("tls: unable to generate random session ticket key: %v"u8, err));
                }
            }
            // Write the deprecated prefix at the beginning so we know we created
            // it. This key with the DEPRECATED prefix isn't used as an actual
            // session ticket key, and is only randomized in case the application
            // reuses it for some reason.
            copy(c.SessionTicketKey[..], deprecatedSessionTicketKey);
        } else 
        if (!bytes.HasPrefix(c.SessionTicketKey[..], deprecatedSessionTicketKey) && len(c.sessionTicketKeys) == 0) {
            c.sessionTicketKeys = new ticketKey[]{c.ticketKeyFromBytes(c.SessionTicketKey)}.slice();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd2) Ꮡc.of(Config.Ꮡmutex).Unlock(); if (ᒐd1) Ꮡc.of(Config.Ꮡmutex).RLock(); ᒐ.Run(); }
}

// ticketKeys returns the ticketKeys for this connection.
// If configForClient has explicitly set keys, those will
// be returned. Otherwise, the keys on c will be used and
// may be rotated if auto-managed.
// During rotation, any expired session ticket keys are deleted from
// c.sessionTicketKeys. If the session ticket key that is currently
// encrypting tickets (ie. the first ticketKey in c.sessionTicketKeys)
// is not fresh, then a new session ticket key will be
// created and prepended to c.sessionTicketKeys.
internal static slice<ticketKey> ticketKeys(this ж<Config> Ꮡc, ж<Config> ᏑconfigForClient) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    bool ᒐd2 = false;
    bool ᒐd3 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var configForClient = ref ᏑconfigForClient.DerefOrNull();

        // If the ConfigForClient callback returned a Config with explicitly set
        // keys, use those, otherwise just use the original Config.
        if (ᏑconfigForClient != nil) {
            ᏑconfigForClient.of(Config.Ꮡmutex).RLock();
            if (configForClient.SessionTicketsDisabled) {
                return default!;
            }
            ᏑconfigForClient.initLegacySessionTicketKeyRLocked();
            if (len(configForClient.sessionTicketKeys) != 0) {
                var ret = configForClient.sessionTicketKeys;
                ᏑconfigForClient.of(Config.Ꮡmutex).RUnlock();
                return ret;
            }
            ᏑconfigForClient.of(Config.Ꮡmutex).RUnlock();
        }
        Ꮡc.of(Config.Ꮡmutex).RLock();
        ᒐd1 = true;
        if (c.SessionTicketsDisabled) {
            return default!;
        }
        Ꮡc.initLegacySessionTicketKeyRLocked();
        if (len(c.sessionTicketKeys) != 0) {
            return c.sessionTicketKeys;
        }
        // Fast path for the common case where the key is fresh enough.
        if (len(c.autoSessionTicketKeys) > 0 && c.time().Sub(c.autoSessionTicketKeys[0].created) < ticketKeyRotation) {
            return c.autoSessionTicketKeys;
        }
        // autoSessionTicketKeys are managed by auto-rotation.
        Ꮡc.of(Config.Ꮡmutex).RUnlock();
        ᒐd2 = true;
        Ꮡc.of(Config.Ꮡmutex).Lock();
        ᒐd3 = true;
        // Re-check the condition in case it changed since obtaining the new lock.
        if (len(c.autoSessionTicketKeys) == 0 || c.time().Sub(c.autoSessionTicketKeys[0].created) >= ticketKeyRotation) {
            array<byte> newKey = new(32);
            {
                var (_, err) = io.ReadFull(c.rand(), newKey[..]); if (err != default!) {
                    throw panic(fmt.Sprintf("unable to generate random session ticket key: %v"u8, err));
                }
            }
            var valid = new slice<ticketKey>(0, () => new(), len(c.autoSessionTicketKeys) + 1);
            valid = append(valid, c.ticketKeyFromBytes(newKey));
            foreach (var (_, vᴛ1) in c.autoSessionTicketKeys) {
                var k = vᴛ1.ΔClone();

                // While rotating the current key, also remove any expired ones.
                if (c.time().Sub(k.created) < ticketKeyLifetime) {
                    valid = append(valid, k.ΔClone());
                }
            }
            c.autoSessionTicketKeys = valid;
        }
        return c.autoSessionTicketKeys;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd3) Ꮡc.of(Config.Ꮡmutex).Unlock(); if (ᒐd2) Ꮡc.of(Config.Ꮡmutex).RLock(); if (ᒐd1) Ꮡc.of(Config.Ꮡmutex).RUnlock(); ᒐ.Run(); }
}

// SetSessionTicketKeys updates the session ticket keys for a server.
//
// The first key will be used when creating new tickets, while all keys can be
// used for decrypting tickets. It is safe to call this function while the
// server is running in order to rotate the session ticket keys. The function
// will panic if keys is empty.
//
// Calling this function will turn off automatic session ticket key rotation.
//
// If multiple servers are terminating connections for the same host they should
// all have the same session ticket keys. If the session ticket keys leaks,
// previously recorded and future TLS connections using those keys might be
// compromised.
public static void SetSessionTicketKeys(this ж<Config> Ꮡc, slice<array<byte>> keys) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (len(keys) == 0) {
        throw panic("tls: keys must have at least one key");
    }
    var newKeys = new slice<ticketKey>(len(keys), () => new());
    foreach (var (i, vᴛ1) in keys) {
        var bytes = vᴛ1.Clone();

        newKeys[i] = c.ticketKeyFromBytes(bytes);
    }
    Ꮡc.of(Config.Ꮡmutex).Lock();
    c.sessionTicketKeys = newKeys;
    Ꮡc.of(Config.Ꮡmutex).Unlock();
}

internal static io.Reader rand(this ref Config c) {
    var r = c.Rand;
    if (r == default!) {
        return go.crypto.rand_package.Reader;
    }
    return r;
}

internal static time.Time time(this ref Config c) {
    var t = c.Time;
    if (t == default!) {
        t = time_package.Now;
    }
    return t();
}

internal static slice<uint16> cipherSuites(this ref Config c) {
    if (c.CipherSuites == default!) {
        if (fips140tls.Required()) {
            return defaultCipherSuitesFIPS;
        }
        return defaultCipherSuites();
    }
    if (fips140tls.Required()) {
        var ΔcipherSuites = slices.Clone<slice<uint16>, uint16>(c.CipherSuites);
        return slices.DeleteFunc(ΔcipherSuites, (uint16 id) => !slices.Contains(defaultCipherSuitesFIPS, id));
    }
    return c.CipherSuites;
}

internal static slice<uint16> ΔsupportedVersions = new uint16[]{
    VersionTLS13,
    VersionTLS12,
    VersionTLS11,
    VersionTLS10
}.slice();

// roleClient and roleServer are meant to call supportedVersions and parents
// with more readability at the callsite.
internal const bool roleClient = true;

internal const bool roleServer = false;

internal static ж<godebug.Setting> tls10server = godebug.New("tls10server"u8);

internal static slice<uint16> supportedVersions(this ж<Config> Ꮡc, bool isClient) {
    ref var c = ref Ꮡc.DerefOrNull();

    var versions = new slice<uint16>(0, len(ΔsupportedVersions));
    foreach (var (_, v) in ΔsupportedVersions) {
        if (fips140tls.Required() && !slices.Contains(defaultSupportedVersionsFIPS, v)) {
            continue;
        }
        if ((Ꮡc == nil || c.MinVersion == 0) && v < VersionTLS12) {
            if (isClient || tls10server.Value() != "1"u8) {
                continue;
            }
        }
        if (isClient && c.EncryptedClientHelloConfigList != default! && v < VersionTLS13) {
            continue;
        }
        if (Ꮡc != nil && c.MinVersion != 0 && v < c.MinVersion) {
            continue;
        }
        if (Ꮡc != nil && c.MaxVersion != 0 && v > c.MaxVersion) {
            continue;
        }
        versions = append(versions, v);
    }
    return versions;
}

internal static uint16 maxSupportedVersion(this ж<Config> Ꮡc, bool isClient) {
    var ΔsupportedVersions = Ꮡc.supportedVersions(isClient);
    if (len(ΔsupportedVersions) == 0) {
        return 0;
    }
    return ΔsupportedVersions[0];
}

// supportedVersionsFromMax returns a list of supported versions derived from a
// legacy maximum version value. Note that only versions supported by this
// library are returned. Any newer peer will use supportedVersions anyway.
internal static slice<uint16> supportedVersionsFromMax(uint16 maxVersion) {
    var versions = new slice<uint16>(0, len(ΔsupportedVersions));
    foreach (var (_, v) in ΔsupportedVersions) {
        if (v > maxVersion) {
            continue;
        }
        versions = append(versions, v);
    }
    return versions;
}

internal static slice<CurveID> curvePreferences(this ж<Config> Ꮡc, uint16 version) {
    ref var c = ref Ꮡc.DerefOrNull();

    slice<CurveID> curvePreferences = default!;
    if (fips140tls.Required()){
        curvePreferences = slices.Clone<slice<CurveID>, CurveID>(defaultCurvePreferencesFIPS);
    } else {
        curvePreferences = defaultCurvePreferences();
    }
    if (Ꮡc != nil && len(c.CurvePreferences) != 0) {
        curvePreferences = slices.DeleteFunc(curvePreferences, (CurveID x) => !slices.Contains(Ꮡc.Value.CurvePreferences, x));
    }
    if (version < VersionTLS13) {
        curvePreferences = slices.DeleteFunc<slice<CurveID>, CurveID>(curvePreferences, isTLS13OnlyKeyExchange);
    }
    return curvePreferences;
}

internal static bool supportsCurve(this ж<Config> Ꮡc, uint16 version, CurveID curve) {
    foreach (var (_, cc) in Ꮡc.curvePreferences(version)) {
        if (cc == curve) {
            return true;
        }
    }
    return false;
}

// mutualVersion returns the protocol version to use given the advertised
// versions of the peer. Priority is given to the peer preference order.
internal static (uint16, bool) mutualVersion(this ж<Config> Ꮡc, bool isClient, slice<uint16> peerVersions) {
    var ΔsupportedVersions = Ꮡc.supportedVersions(isClient);
    foreach (var (_, peerVersion) in peerVersions) {
        foreach (var (_, v) in ΔsupportedVersions) {
            if (v == peerVersion) {
                return (v, true);
            }
        }
    }
    return (0, false);
}

// errNoCertificates should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/xtls/xray-core
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname errNoCertificates
public static error errNoCertificates = errors.New("tls: no certificates configured"u8);

// getCertificate returns the best certificate for the given ClientHelloInfo,
// defaulting to the first element of c.Certificates.
internal static (ж<Certificate>, error) getCertificate(this ref Config c, ж<ClientHelloInfo> ᏑclientHello) {
    ref var clientHello = ref ᏑclientHello.DerefOrNull();

    if (c.GetCertificate != default! && (len(c.Certificates) == 0 || len(clientHello.ServerName) > 0)) {
        var (cert, err) = c.GetCertificate(ᏑclientHello);
        if (cert != nil || err != default!) {
            return (cert, err);
        }
    }
    if (len(c.Certificates) == 0) {
        return (default!, errNoCertificates);
    }
    if (len(c.Certificates) == 1) {
        // There's only one choice, so no point doing any work.
        return (Ꮡ(c.Certificates, 0), default!);
    }
    if (c.NameToCertificate != default!) {
        @string name = strings.ToLower(clientHello.ServerName);
        {
            var (cert, ok) = c.NameToCertificate[name, ꟷ]; if (ok) {
                return (cert, default!);
            }
        }
        if (len(name) > 0) {
            var labels = strings.Split(name, "."u8);
            labels[0] = "*"u8;
            @string wildcardName = strings.Join(labels, "."u8);
            {
                var (cert, ok) = c.NameToCertificate[wildcardName, ꟷ]; if (ok) {
                    return (cert, default!);
                }
            }
        }
    }
    foreach (var (_, vᴛ1) in c.Certificates) {
        ref var cert = ref heap(new Certificate(), out var Ꮡcert);
        cert = vᴛ1;

        {
            var err = ᏑclientHello.SupportsCertificate(Ꮡcert); if (err == default!) {
                return (Ꮡcert, default!);
            }
        }
    }
    // If nothing matches, return the first certificate.
    return (Ꮡ(c.Certificates, 0), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noMutuallySupportedˢ = "no mutually supported protocol versions"u8;
internal static readonly @string clientDoesnTSupportEcdheˢ = "client doesn't support ECDHE, can only use legacy RSA key exchange"u8;
internal static readonly @string clientDoesnTSupportˢ = "client doesn't support certificate curve"u8;
internal static readonly @string connectionDoesnTSupportˢ = "connection doesn't support Ed25519"u8;
internal static readonly @string clientDoesnTSupportAnyˢ = "client doesn't support any cipher suites compatible with the certificate"u8;

// SupportsCertificate returns nil if the provided certificate is supported by
// the client that sent the ClientHello. Otherwise, it returns an error
// describing the reason for the incompatibility.
//
// If this [ClientHelloInfo] was passed to a GetConfigForClient or GetCertificate
// callback, this method will take into account the associated [Config]. Note that
// if GetConfigForClient returns a different [Config], the change can't be
// accounted for by this method.
//
// This function will call x509.ParseCertificate unless c.Leaf is set, which can
// incur a significant performance cost.
public static error SupportsCertificate(this ж<ClientHelloInfo> Ꮡchi, ж<Certificate> Ꮡc) {
    ref var chi = ref Ꮡchi.DerefOrNull();
    ref var c = ref Ꮡc.DerefOrNull();

    // Note we don't currently support certificate_authorities nor
    // signature_algorithms_cert, and don't check the algorithms of the
    // signatures on the chain (which anyway are a SHOULD, see RFC 8446,
    // Section 4.4.2.2).
    var config = chi.config;
    if (config == nil) {
        config = Ꮡ(new Config(nil));
    }
    var (vers, ok) = config.mutualVersion(roleServer, chi.SupportedVersions);
    if (!ok) {
        return errors.New(noMutuallySupportedˢ);
    }
    // If the client specified the name they are trying to connect to, the
    // certificate needs to be valid for it.
    if (chi.ServerName != ""u8) {
        var (x509Cert, err) = c.leaf();
        if (err != default!) {
            return fmt.Errorf("failed to parse certificate: %w"u8, err);
        }
        {
            var errΔ1 = x509Cert.VerifyHostname(chi.ServerName); if (errΔ1 != default!) {
                return fmt.Errorf("certificate is not valid for requested server name: %w"u8, errΔ1);
            }
        }
    }
    // supportsRSAFallback returns nil if the certificate and connection support
    // the static RSA key exchange, and unsupported otherwise. The logic for
    // supporting static RSA is completely disjoint from the logic for
    // supporting signed key exchanges, so we just check it as a fallback.
    var configʗ1 = config;
    error supportsRSAFallback(error unsupported) {
        // TLS 1.3 dropped support for the static RSA key exchange.
        if (vers == VersionTLS13) {
            return unsupported;
        }
        // The static RSA key exchange works by decrypting a challenge with the
        // RSA private key, not by signing, so check the PrivateKey implements
        // crypto.Decrypter, like *rsa.PrivateKey does.
        {
            var (priv, okΔ1) = Ꮡc.Value.PrivateKey._<crypto.Decrypter>(ᐧ); if (okΔ1){
                {
                    var (_, okΔ2) = priv.Public()._<ж<rsa.PublicKey>>(ᐧ); if (!okΔ2) {
                        return unsupported;
                    }
                }
            } else {
                return unsupported;
            }
        }
        // Finally, there needs to be a mutual cipher suite that uses the static
        // RSA key exchange instead of ECDHE.
        var rsaCipherSuite = selectCipherSuite(Ꮡchi.Value.CipherSuites, configʗ1.cipherSuites(), (ж<cipherSuite> cΔ1) => {
            if ((nint)((~cΔ1).flags & (nint)suiteECDHE) != 0) {
                return false;
            }
            if (vers < VersionTLS12 && (nint)((~cΔ1).flags & (nint)suiteTLS12) != 0) {
                return false;
            }
            return true;
        });
        if (rsaCipherSuite == nil) {
            return unsupported;
        }
        return default!;
    }
    // If the client sent the signature_algorithms extension, ensure it supports
    // schemes we can use with this certificate and TLS version.
    if (len(chi.SignatureSchemes) > 0) {
        {
            var (_, err) = selectSignatureScheme(vers, ref (Ꮡc).DerefOrNull(), chi.SignatureSchemes); if (err != default!) {
                return supportsRSAFallback(err);
            }
        }
    }
    // In TLS 1.3 we are done because supported_groups is only relevant to the
    // ECDHE computation, point format negotiation is removed, cipher suites are
    // only relevant to the AEAD choice, and static RSA does not exist.
    if (vers == VersionTLS13) {
        return default!;
    }
    // The only signed key exchange we support is ECDHE.
    if (!supportsECDHE(config, vers, chi.SupportedCurves, chi.SupportedPoints)) {
        return supportsRSAFallback(errors.New(clientDoesnTSupportEcdheˢ));
    }
    bool ecdsaCipherSuite = default!;
    {
        var (priv, okΔ3) = c.PrivateKey._<crypto.Signer>(ᐧ); if (okΔ3){
            var switchᴛ3 = priv.Public();
            switch (switchᴛ3.type()) {
            case ж<ecdsa.PublicKey> pub: {
                CurveID curve = default!;
                var exprᴛ1 = (~pub).Curve;
                if (AreEqual(exprᴛ1, elliptic.P256())) {
                    curve = CurveP256;
                }
                else if (AreEqual(exprᴛ1, elliptic.P384())) {
                    curve = CurveP384;
                }
                else if (AreEqual(exprᴛ1, elliptic.P521())) {
                    curve = CurveP521;
                }
                else { /* default: */
                    return supportsRSAFallback(unsupportedCertificateError(ref (Ꮡc).DerefOrNull()));
                }

                bool curveOk = default!;
                foreach (var (_, cΔ2) in chi.SupportedCurves) {
                    if (cΔ2 == curve && config.supportsCurve(vers, cΔ2)) {
                        curveOk = true;
                        break;
                    }
                }
                if (!curveOk) {
                    return errors.New(clientDoesnTSupportˢ);
                }
                ecdsaCipherSuite = true;
                break;
            }
            case ed25519.PublicKey pub: {
                if (vers < VersionTLS12 || len(chi.SignatureSchemes) == 0) {
                    return errors.New(connectionDoesnTSupportˢ);
                }
                ecdsaCipherSuite = true;
                break;
            }
            case ж<rsa.PublicKey> pub: {
                break;
            }
            default: {
                var pub = switchᴛ3;
                return supportsRSAFallback(unsupportedCertificateError(ref (Ꮡc).DerefOrNull()));
            }}
        } else {
            return supportsRSAFallback(unsupportedCertificateError(ref (Ꮡc).DerefOrNull()));
        }
    }
    // Make sure that there is a mutually supported cipher suite that works with
    // this certificate. Cipher suite selection will then apply the logic in
    // reverse to pick it. See also serverHandshakeState.cipherSuiteOk.
    var cipherSuite = selectCipherSuite(chi.CipherSuites, config.cipherSuites(), (ж<cipherSuite> cΔ3) => {
        if ((nint)((~cΔ3).flags & (nint)suiteECDHE) == 0) {
            return false;
        }
        if ((nint)((~cΔ3).flags & (nint)suiteECSign) != 0){
            if (!ecdsaCipherSuite) {
                return false;
            }
        } else {
            if (ecdsaCipherSuite) {
                return false;
            }
        }
        if (vers < VersionTLS12 && (nint)((~cΔ3).flags & (nint)suiteTLS12) != 0) {
            return false;
        }
        return true;
    });
    if (cipherSuite == nil) {
        return supportsRSAFallback(errors.New(clientDoesnTSupportAnyˢ));
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chainIsNotSignedByAnˢ = "chain is not signed by an acceptable CA"u8;

// SupportsCertificate returns nil if the provided certificate is supported by
// the server that sent the CertificateRequest. Otherwise, it returns an error
// describing the reason for the incompatibility.
public static error SupportsCertificate(this ref CertificateRequestInfo cri, ж<Certificate> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    {
        var (_, err) = selectSignatureScheme(cri.Version, ref (Ꮡc).DerefOrNull(), cri.SignatureSchemes); if (err != default!) {
            return err;
        }
    }
    if (len(cri.AcceptableCAs) == 0) {
        return default!;
    }
    foreach (var (j, cert) in c.ΔCertificate) {
        var x509Cert = c.Leaf;
        // Parse the certificate if this isn't the leaf node, or if
        // chain.Leaf was nil.
        if (j != 0 || x509Cert == nil) {
            error err = default!;
            {
                (x509Cert, err) = Δx509.ParseCertificate(cert); if (err != default!) {
                    return fmt.Errorf("failed to parse certificate #%d in the chain: %w"u8, j, err);
                }
            }
        }
        foreach (var (_, ca) in cri.AcceptableCAs) {
            if (bytes.Equal((~x509Cert).RawIssuer, ca)) {
                return default!;
            }
        }
    }
    return errors.New(chainIsNotSignedByAnˢ);
}

// BuildNameToCertificate parses c.Certificates and builds c.NameToCertificate
// from the CommonName and SubjectAlternateName fields of each of the leaf
// certificates.
//
// Deprecated: NameToCertificate only allows associating a single certificate
// with a given name. Leave that field nil to let the library select the first
// compatible chain from Certificates.
public static void BuildNameToCertificate(this ref Config c) {
    c.NameToCertificate = new map<@string, ж<Certificate>>();
    foreach (var (i, _) in c.Certificates) {
        var cert = Ꮡ(c.Certificates, i);
        var (x509Cert, err) = cert.leaf();
        if (err != default!) {
            continue;
        }
        // If SANs are *not* present, some clients will consider the certificate
        // valid for the name in the Common Name.
        if ((~x509Cert).Subject.CommonName != ""u8 && len((~x509Cert).DNSNames) == 0) {
            c.NameToCertificate[(~x509Cert).Subject.CommonName] = cert;
        }
        foreach (var (_, san) in (~x509Cert).DNSNames) {
            c.NameToCertificate[san] = cert;
        }
    }
}

internal static readonly @string keyLogLabelTLS12 = "CLIENT_RANDOM"u8;
internal static readonly @string keyLogLabelClientHandshake = "CLIENT_HANDSHAKE_TRAFFIC_SECRET"u8;
internal static readonly @string keyLogLabelServerHandshake = "SERVER_HANDSHAKE_TRAFFIC_SECRET"u8;
internal static readonly @string keyLogLabelClientTraffic = "CLIENT_TRAFFIC_SECRET_0"u8;
internal static readonly @string keyLogLabelServerTraffic = "SERVER_TRAFFIC_SECRET_0"u8;

internal static error writeKeyLog(this ref Config c, @string label, slice<byte> clientRandom, slice<byte> secret) {
    if (c.KeyLogWriter == default!) {
        return default!;
    }
    var logLine = fmt.Appendf(default!, "%s %x %x\n"u8, label, clientRandom, secret);
    ᏑwriterMutex.Lock();
    var (_, err) = c.KeyLogWriter.Write(logLine);
    ᏑwriterMutex.Unlock();
    return err;
}

// writerMutex protects all KeyLogWriters globally. It is rarely enabled,
// and is only for debugging, so a global mutex saves space.
internal static ж<sync.Mutex> ᏑwriterMutex = new StandardBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex writerMutex => ref ᏑwriterMutex.Value;

// A Certificate is a chain of one or more certificates, leaf first.
partial struct Certificate {
    public slice<slice<byte>> ΔCertificate;
    // PrivateKey contains the private key corresponding to the public key in
    // Leaf. This must implement crypto.Signer with an RSA, ECDSA or Ed25519 PublicKey.
    // For a server up to TLS 1.2, it can also implement crypto.Decrypter with
    // an RSA PublicKey.
    public cryptoꓸPrivateKey PrivateKey;
    // SupportedSignatureAlgorithms is an optional list restricting what
    // signature algorithms the PrivateKey can be used for.
    public slice<SignatureScheme> SupportedSignatureAlgorithms;
    // OCSPStaple contains an optional OCSP response which will be served
    // to clients that request it.
    public slice<byte> OCSPStaple;
    // SignedCertificateTimestamps contains an optional list of Signed
    // Certificate Timestamps which will be served to clients that request it.
    public slice<slice<byte>> SignedCertificateTimestamps;
    // Leaf is the parsed form of the leaf certificate, which may be initialized
    // using x509.ParseCertificate to reduce per-handshake processing. If nil,
    // the leaf certificate will be parsed as needed.
    public ж<Δx509.Certificate> Leaf;
}

// leaf returns the parsed leaf certificate, either from c.Leaf or by parsing
// the corresponding c.Certificate[0].
internal static (ж<Δx509.Certificate>, error) leaf(this ref Certificate c) {
    if (c.Leaf != nil) {
        return (c.Leaf, default!);
    }
    return Δx509.ParseCertificate(c.ΔCertificate[0]);
}

partial interface handshakeMessage {
    (slice<byte>, error) marshal();
    bool unmarshal(slice<byte> _);
}

partial interface handshakeMessageWithOriginalBytes :
    handshakeMessage
{
    // originalBytes should return the original bytes that were passed to
    // unmarshal to create the message. If the message was not produced by
    // unmarshal, it should return nil.
    slice<byte> originalBytes();
}

// lruSessionCache is a ClientSessionCache implementation that uses an LRU
// caching strategy.
partial struct lruSessionCache {
    public partial ref sync_package.Mutex Mutex { get; }
    internal map<@string, ж<list.Element>> m;
    internal ж<list.List> q;
    internal nint capacity;
}

partial struct lruSessionCacheEntry {
    internal @string sessionKey;
    internal ж<ClientSessionState> state;
}

// NewLRUClientSessionCache returns a [ClientSessionCache] with the given
// capacity that uses an LRU strategy. If capacity is < 1, a default capacity
// is used instead.
public static ClientSessionCache NewLRUClientSessionCache(nint capacity) {
    const nint defaultSessionCacheCapacity = 64;
    if (capacity < 1) {
        capacity = defaultSessionCacheCapacity;
    }
    return new lruSessionCacheжClientSessionCache(Ꮡ(new lruSessionCache(
        m: new map<@string, ж<list.Element>>(),
        q: list.New(),
        capacity: capacity
    )));
}

// Put adds the provided (sessionKey, cs) pair to the cache. If cs is nil, the entry
// corresponding to sessionKey is removed from the cache instead.
internal static void Put(this ж<lruSessionCache> Ꮡc, @string sessionKey, ж<ClientSessionState> Ꮡcs) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        Ꮡc.of(lruSessionCache.ᏑMutex).Lock();
        ᒐd1 = true;
        {
            var (elemΔ1, ok) = c.m[sessionKey, ꟷ]; if (ok) {
                if (Ꮡcs == nil){
                    c.q.Remove(elemΔ1);
                    delete(c.m, sessionKey);
                } else {
                    var entryΔ1 = (~elemΔ1).Value._<ж<lruSessionCacheEntry>>();
                    entryΔ1.Value.state = Ꮡcs;
                    c.q.MoveToFront(elemΔ1);
                }
                return;
            }
        }
        if (c.q.Len() < c.capacity) {
            var entryΔ2 = Ꮡ(new lruSessionCacheEntry(sessionKey, Ꮡcs));
            c.m[sessionKey] = c.q.PushFront(entryΔ2.OrTypedNil());
            return;
        }
        var elem = c.q.Back();
        var entry = (~elem).Value._<ж<lruSessionCacheEntry>>();
        delete(c.m, (~entry).sessionKey);
        entry.Value.sessionKey = sessionKey;
        entry.Value.state = Ꮡcs;
        c.q.MoveToFront(elem);
        c.m[sessionKey] = elem;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡc.of(lruSessionCache.ᏑMutex).Unlock(); ᒐ.Run(); }
}

// Get returns the [ClientSessionState] value associated with a given key. It
// returns (nil, false) if no value is found.
internal static (ж<ClientSessionState>, bool) Get(this ж<lruSessionCache> Ꮡc, @string sessionKey) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        Ꮡc.of(lruSessionCache.ᏑMutex).Lock();
        ᒐd1 = true;
        {
            var (elem, ok) = c.m[sessionKey, ꟷ]; if (ok) {
                c.q.MoveToFront(elem);
                return ((~(~elem).Value._<ж<lruSessionCacheEntry>>()).state, true);
            }
        }
        return (default!, false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.of(lruSessionCache.ᏑMutex).Unlock(); ᒐ.Run(); }
}

internal static ж<Config> ᏑemptyConfig = new StandardBox<Config>(new Config());
internal static ref Config emptyConfig => ref ᏑemptyConfig.Value;

internal static ж<Config> defaultConfig() {
    return ᏑemptyConfig;
}

internal static error unexpectedMessageError(any wanted, any got) {
    return fmt.Errorf("tls: received unexpected handshake message of type %T when waiting for %T"u8, got, wanted);
}

// supportedSignatureAlgorithms returns the supported signature algorithms.
internal static slice<SignatureScheme> supportedSignatureAlgorithms() {
    if (!fips140tls.Required()) {
        return defaultSupportedSignatureAlgorithms;
    }
    return defaultSupportedSignatureAlgorithmsFIPS;
}

internal static bool isSupportedSignatureAlgorithm(SignatureScheme sigAlg, slice<SignatureScheme> supportedSignatureAlgorithms) {
    foreach (var (_, s) in supportedSignatureAlgorithms) {
        if (s == sigAlg) {
            return true;
        }
    }
    return false;
}

// CertificateVerificationError is returned when certificate verification fails during the handshake.
partial struct CertificateVerificationError {
    // UnverifiedCertificates and its contents should not be modified.
    public slice<ж<Δx509.Certificate>> UnverifiedCertificates;
    public error Err;
}

public static @string Error(this ref CertificateVerificationError e) {
    return fmt.Sprintf("tls: failed to verify certificate: %s"u8, e.Err);
}

public static error Unwrap(this ref CertificateVerificationError e) {
    return e.Err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsNoFipsCompatibleˢ = "tls: no FIPS compatible certificate chains found"u8;

// fipsAllowedChains returns chains that are allowed to be used in a TLS connection
// based on the current fips140tls enforcement setting.
//
// If fips140tls is not required, the chains are returned as-is with no processing.
// Otherwise, the returned chains are filtered to only those allowed by FIPS 140-3.
// If this results in no chains it returns an error.
internal static (slice<slice<ж<Δx509.Certificate>>>, error) fipsAllowedChains(slice<slice<ж<Δx509.Certificate>>> chains) {
    if (!fips140tls.Required()) {
        return (chains, default!);
    }
    var permittedChains = new slice<slice<ж<Δx509.Certificate>>>(0, len(chains));
    foreach (var (_, chain) in chains) {
        if (fipsAllowChain(chain)) {
            permittedChains = append(permittedChains, chain);
        }
    }
    if (len(permittedChains) == 0) {
        return (default!, errors.New(tlsNoFipsCompatibleˢ));
    }
    return (permittedChains, default!);
}

internal static bool fipsAllowChain(slice<ж<Δx509.Certificate>> chain) {
    if (len(chain) == 0) {
        return false;
    }
    foreach (var (_, cert) in chain) {
        if (!fipsAllowCert(ref (cert).DerefOrNull())) {
            return false;
        }
    }
    return true;
}

internal static bool fipsAllowCert(ref Δx509.Certificate c) {
    // The key must be RSA 2048, RSA 3072, RSA 4096,
    // or ECDSA P-256, P-384, P-521.
    switch (c.PublicKey.type()) {
    case ж<rsa.PublicKey> k: {
        nint size = (~k).N.BitLen();
        return size == 2048 || size == 3072 || size == 4096;
    }
    case ж<ecdsa.PublicKey> k: {
        return AreEqual((~k).Curve, elliptic.P256()) || AreEqual((~k).Curve, elliptic.P384()) || AreEqual((~k).Curve, elliptic.P521());
    }}
    return false;
}

// anyValidVerifiedChain reports if at least one of the chains in verifiedChains
// is valid, as indicated by none of the certificates being expired and the root
// being in opts.Roots (or in the system root pool if opts.Roots is nil). If
// verifiedChains is empty, it returns false.
internal static bool anyValidVerifiedChain(slice<slice<ж<Δx509.Certificate>>> verifiedChains, Δx509.VerifyOptions opts) {
    foreach (var (_, chain) in verifiedChains) {
        if (len(chain) == 0) {
            continue;
        }
        if (slices.ContainsFunc(chain, (ж<Δx509.Certificate> cert) => opts.CurrentTime.Before((~cert).NotBefore) || opts.CurrentTime.After((~cert).NotAfter))) {
            continue;
        }
        // Since we already validated the chain, we only care that it is rooted
        // in a CA in opts.Roots. On platforms where we control chain validation
        // (e.g. not Windows or macOS) this is a simple lookup in the CertPool
        // internal hash map, which we can simulate by running Verify on the
        // root. On other platforms, we have to do full verification again,
        // because EKU handling might differ. We will want to replace this with
        // CertPool.Contains if/once that is available. See go.dev/issue/77376.
        if (runtime.GOOS == "windows"u8 || runtime.GOOS == "darwin"u8 || runtime.GOOS == "ios"u8){
            opts.Intermediates = Δx509.NewCertPool();
            foreach (var (_, cert) in chain.slice(1, max(1, len(chain) - 1))) {
                opts.Intermediates.AddCert(cert);
            }
            var leaf = chain[0];
            {
                var (_, err) = leaf.Verify(opts); if (err == default!) {
                    return true;
                }
            }
        } else {
            var root = chain[len(chain) - 1];
            {
                var (_, err) = root.Verify(opts); if (err == default!) {
                    return true;
                }
            }
        }
    }
    return false;
}

} // end tls_package
