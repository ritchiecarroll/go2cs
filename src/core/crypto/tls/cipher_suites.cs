// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using crypto = crypto_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using des = go.crypto.des_package;
using hmac = go.crypto.hmac_package;
using boring = go.crypto.@internal.boring_package;
using fipsaes = go.crypto.@internal.fips140.aes_package;
using gcm = go.crypto.@internal.fips140.aes.gcm_package;
using rc4 = go.crypto.rc4_package;
using sha1 = go.crypto.sha1_package;
using sha256 = go.crypto.sha256_package;
using fmt = fmt_package;
using hash = hash_package;
using cpu = go.@internal.cpu_package;
using runtime = runtime_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using chacha20poly1305 = vendor.golang.org.x.crypto.chacha20poly1305_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140.aes;
using vendor.golang.org.x.crypto;
using Δx509 = go.crypto.x509_package;

partial class tls_package {

// CipherSuite is a TLS cipher suite. Note that most functions in this package
// accept and expose cipher suite IDs instead of this type.
partial struct CipherSuite {
    public uint16 ID;
    public @string Name;
    // Supported versions is the list of TLS protocol versions that can
    // negotiate this cipher suite.
    public slice<uint16> SupportedVersions;
    // Insecure is true if the cipher suite has known security issues
    // due to its primitives, design, or implementation.
    public bool Insecure;
}

internal static slice<uint16> supportedUpToTLS12 = new uint16[]{VersionTLS10, VersionTLS11, VersionTLS12}.slice();
internal static slice<uint16> supportedOnlyTLS12 = new uint16[]{VersionTLS12}.slice();
internal static slice<uint16> supportedOnlyTLS13 = new uint16[]{VersionTLS13}.slice();

// CipherSuites returns a list of cipher suites currently implemented by this
// package, excluding those with security issues, which are returned by
// [InsecureCipherSuites].
//
// The list is sorted by ID. Note that the default cipher suites selected by
// this package might depend on logic that can't be captured by a static list,
// and might not match those returned by this function.
public static slice<ж<CipherSuite>> CipherSuites() {
    return new ж<CipherSuite>[]{
        Ꮡ(new CipherSuite(TLS_AES_128_GCM_SHA256, "TLS_AES_128_GCM_SHA256"u8, supportedOnlyTLS13, false)),
        Ꮡ(new CipherSuite(TLS_AES_256_GCM_SHA384, "TLS_AES_256_GCM_SHA384"u8, supportedOnlyTLS13, false)),
        Ꮡ(new CipherSuite(TLS_CHACHA20_POLY1305_SHA256, "TLS_CHACHA20_POLY1305_SHA256"u8, supportedOnlyTLS13, false)),
        Ꮡ(new CipherSuite(TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA, "TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA"u8, supportedUpToTLS12, false)),
        Ꮡ(new CipherSuite(TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA, "TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA"u8, supportedUpToTLS12, false)),
        Ꮡ(new CipherSuite(TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA, "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA"u8, supportedUpToTLS12, false)),
        Ꮡ(new CipherSuite(TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA, "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA"u8, supportedUpToTLS12, false)),
        Ꮡ(new CipherSuite(TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256"u8, supportedOnlyTLS12, false)),
        Ꮡ(new CipherSuite(TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384, "TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384"u8, supportedOnlyTLS12, false)),
        Ꮡ(new CipherSuite(TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, "TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256"u8, supportedOnlyTLS12, false)),
        Ꮡ(new CipherSuite(TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384, "TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384"u8, supportedOnlyTLS12, false)),
        Ꮡ(new CipherSuite(TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256, "TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256"u8, supportedOnlyTLS12, false)),
        Ꮡ(new CipherSuite(TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256, "TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256"u8, supportedOnlyTLS12, false))
    }.slice();
}

// InsecureCipherSuites returns a list of cipher suites currently implemented by
// this package and which have security issues.
//
// Most applications should not use the cipher suites in this list, and should
// only use those returned by [CipherSuites].
public static slice<ж<CipherSuite>> InsecureCipherSuites() {
    // This list includes RC4, CBC_SHA256, and 3DES cipher suites. See
    // cipherSuitesPreferenceOrder for details.
    return new ж<CipherSuite>[]{
        Ꮡ(new CipherSuite(TLS_RSA_WITH_RC4_128_SHA, "TLS_RSA_WITH_RC4_128_SHA"u8, supportedUpToTLS12, true)),
        Ꮡ(new CipherSuite(TLS_RSA_WITH_3DES_EDE_CBC_SHA, "TLS_RSA_WITH_3DES_EDE_CBC_SHA"u8, supportedUpToTLS12, true)),
        Ꮡ(new CipherSuite(TLS_RSA_WITH_AES_128_CBC_SHA, "TLS_RSA_WITH_AES_128_CBC_SHA"u8, supportedUpToTLS12, true)),
        Ꮡ(new CipherSuite(TLS_RSA_WITH_AES_256_CBC_SHA, "TLS_RSA_WITH_AES_256_CBC_SHA"u8, supportedUpToTLS12, true)),
        Ꮡ(new CipherSuite(TLS_RSA_WITH_AES_128_CBC_SHA256, "TLS_RSA_WITH_AES_128_CBC_SHA256"u8, supportedOnlyTLS12, true)),
        Ꮡ(new CipherSuite(TLS_RSA_WITH_AES_128_GCM_SHA256, "TLS_RSA_WITH_AES_128_GCM_SHA256"u8, supportedOnlyTLS12, true)),
        Ꮡ(new CipherSuite(TLS_RSA_WITH_AES_256_GCM_SHA384, "TLS_RSA_WITH_AES_256_GCM_SHA384"u8, supportedOnlyTLS12, true)),
        Ꮡ(new CipherSuite(TLS_ECDHE_ECDSA_WITH_RC4_128_SHA, "TLS_ECDHE_ECDSA_WITH_RC4_128_SHA"u8, supportedUpToTLS12, true)),
        Ꮡ(new CipherSuite(TLS_ECDHE_RSA_WITH_RC4_128_SHA, "TLS_ECDHE_RSA_WITH_RC4_128_SHA"u8, supportedUpToTLS12, true)),
        Ꮡ(new CipherSuite(TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA, "TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA"u8, supportedUpToTLS12, true)),
        Ꮡ(new CipherSuite(TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256, "TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256"u8, supportedOnlyTLS12, true)),
        Ꮡ(new CipherSuite(TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256, "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256"u8, supportedOnlyTLS12, true))
    }.slice();
}

// CipherSuiteName returns the standard name for the passed cipher suite ID
// (e.g. "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256"), or a fallback representation
// of the ID value if the cipher suite is not implemented by this package.
public static @string CipherSuiteName(uint16 id) {
    foreach (var (_, c) in CipherSuites()) {
        if ((~c).ID == id) {
            return (~c).Name;
        }
    }
    foreach (var (_, c) in InsecureCipherSuites()) {
        if ((~c).ID == id) {
            return (~c).Name;
        }
    }
    return fmt.Sprintf("0x%04X"u8, id);
}

internal static UntypedInt suiteECDHE => /* 1 << iota */ 1;
internal static UntypedInt suiteECSign => 2;
internal static UntypedInt suiteTLS12 => 4;
internal static UntypedInt suiteSHA384 => 8;

// A cipherSuite is a TLS 1.0–1.2 cipher suite, and defines the key exchange
// mechanism, as well as the cipher+MAC pair or the AEAD.
partial struct cipherSuite {
    internal uint16 id;
    // the lengths, in bytes, of the key material needed for each component.
    internal nint keyLen;
    internal nint macLen;
    internal nint ivLen;
    internal Func<uint16, keyAgreement> ka;
    // flags is a bitmask of the suite* values, above.
    internal nint flags;
    internal Func<slice<byte>, slice<byte>, bool, any> cipher;
    internal Func<slice<byte>, hash.Hash> mac;
    internal Func<slice<byte>, slice<byte>, aead> aead;
}

// TODO: replace with a map, since the order doesn't matter.
internal static slice<ж<cipherSuite>> ΔcipherSuites = new ж<cipherSuite>[]{
    Ꮡ(new cipherSuite(TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305, 32, 0, 12, ecdheRSAKA, (nint)((nint)suiteECDHE | (nint)suiteTLS12), default!, default!, aeadChaCha20Poly1305)),
    Ꮡ(new cipherSuite(TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305, 32, 0, 12, ecdheECDSAKA, (nint)((nint)(UntypedInt)(suiteECDHE | suiteECSign) | (nint)suiteTLS12), default!, default!, aeadChaCha20Poly1305)),
    Ꮡ(new cipherSuite(TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, 16, 0, 4, ecdheRSAKA, (nint)((nint)suiteECDHE | (nint)suiteTLS12), default!, default!, aeadAESGCM)),
    Ꮡ(new cipherSuite(TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, 16, 0, 4, ecdheECDSAKA, (nint)((nint)(UntypedInt)(suiteECDHE | suiteECSign) | (nint)suiteTLS12), default!, default!, aeadAESGCM)),
    Ꮡ(new cipherSuite(TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384, 32, 0, 4, ecdheRSAKA, (nint)((nint)(UntypedInt)(suiteECDHE | suiteTLS12) | (nint)suiteSHA384), default!, default!, aeadAESGCM)),
    Ꮡ(new cipherSuite(TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384, 32, 0, 4, ecdheECDSAKA, (nint)((nint)(UntypedInt)((UntypedInt)(suiteECDHE | suiteECSign) | suiteTLS12) | (nint)suiteSHA384), default!, default!, aeadAESGCM)),
    Ꮡ(new cipherSuite(TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256, 16, 32, 16, ecdheRSAKA, (nint)((nint)suiteECDHE | (nint)suiteTLS12), cipherAES, macSHA256, default!)),
    Ꮡ(new cipherSuite(TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA, 16, 20, 16, ecdheRSAKA, suiteECDHE, cipherAES, macSHA1, default!)),
    Ꮡ(new cipherSuite(TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256, 16, 32, 16, ecdheECDSAKA, (nint)((nint)(UntypedInt)(suiteECDHE | suiteECSign) | (nint)suiteTLS12), cipherAES, macSHA256, default!)),
    Ꮡ(new cipherSuite(TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA, 16, 20, 16, ecdheECDSAKA, (nint)((nint)suiteECDHE | (nint)suiteECSign), cipherAES, macSHA1, default!)),
    Ꮡ(new cipherSuite(TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA, 32, 20, 16, ecdheRSAKA, suiteECDHE, cipherAES, macSHA1, default!)),
    Ꮡ(new cipherSuite(TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA, 32, 20, 16, ecdheECDSAKA, (nint)((nint)suiteECDHE | (nint)suiteECSign), cipherAES, macSHA1, default!)),
    Ꮡ(new cipherSuite(TLS_RSA_WITH_AES_128_GCM_SHA256, 16, 0, 4, rsaKA, suiteTLS12, default!, default!, aeadAESGCM)),
    Ꮡ(new cipherSuite(TLS_RSA_WITH_AES_256_GCM_SHA384, 32, 0, 4, rsaKA, (nint)((nint)suiteTLS12 | (nint)suiteSHA384), default!, default!, aeadAESGCM)),
    Ꮡ(new cipherSuite(TLS_RSA_WITH_AES_128_CBC_SHA256, 16, 32, 16, rsaKA, suiteTLS12, cipherAES, macSHA256, default!)),
    Ꮡ(new cipherSuite(TLS_RSA_WITH_AES_128_CBC_SHA, 16, 20, 16, rsaKA, 0, cipherAES, macSHA1, default!)),
    Ꮡ(new cipherSuite(TLS_RSA_WITH_AES_256_CBC_SHA, 32, 20, 16, rsaKA, 0, cipherAES, macSHA1, default!)),
    Ꮡ(new cipherSuite(TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA, 24, 20, 8, ecdheRSAKA, suiteECDHE, cipher3DES, macSHA1, default!)),
    Ꮡ(new cipherSuite(TLS_RSA_WITH_3DES_EDE_CBC_SHA, 24, 20, 8, rsaKA, 0, cipher3DES, macSHA1, default!)),
    Ꮡ(new cipherSuite(TLS_RSA_WITH_RC4_128_SHA, 16, 20, 0, rsaKA, 0, cipherRC4, macSHA1, default!)),
    Ꮡ(new cipherSuite(TLS_ECDHE_RSA_WITH_RC4_128_SHA, 16, 20, 0, ecdheRSAKA, suiteECDHE, cipherRC4, macSHA1, default!)),
    Ꮡ(new cipherSuite(TLS_ECDHE_ECDSA_WITH_RC4_128_SHA, 16, 20, 0, ecdheECDSAKA, (nint)((nint)suiteECDHE | (nint)suiteECSign), cipherRC4, macSHA1, default!))
}.slice();

// selectCipherSuite returns the first TLS 1.0–1.2 cipher suite from ids which
// is also in supportedIDs and passes the ok filter.
internal static ж<cipherSuite> selectCipherSuite(slice<uint16> ids, slice<uint16> supportedIDs, Func<ж<cipherSuite>, bool> ok) {
    foreach (var (_, id) in ids) {
        var candidate = cipherSuiteByID(id);
        if (candidate == nil || !ok(candidate)) {
            continue;
        }
        foreach (var (_, suppID) in supportedIDs) {
            if (id == suppID) {
                return candidate;
            }
        }
    }
    return default!;
}

// A cipherSuiteTLS13 defines only the pair of the AEAD algorithm and hash
// algorithm to be used with HKDF. See RFC 8446, Appendix B.4.
partial struct cipherSuiteTLS13 {
    internal uint16 id;
    internal nint keyLen;
    internal Func<slice<byte>, slice<byte>, aead> aead;
    internal crypto.Hash hash;
}

// TODO: replace with a map.
// cipherSuitesTLS13 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/quic-go/quic-go
//   - github.com/sagernet/quic-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cipherSuitesTLS13
internal static slice<ж<cipherSuiteTLS13>> cipherSuitesTLS13 = new ж<cipherSuiteTLS13>[]{
    Ꮡ(new cipherSuiteTLS13(TLS_AES_128_GCM_SHA256, 16, aeadAESGCMTLS13, crypto.SHA256)),
    Ꮡ(new cipherSuiteTLS13(TLS_CHACHA20_POLY1305_SHA256, 32, aeadChaCha20Poly1305, crypto.SHA256)),
    Ꮡ(new cipherSuiteTLS13(TLS_AES_256_GCM_SHA384, 32, aeadAESGCMTLS13, crypto.SHA384))
}.slice();

// AEADs w/ ECDHE
// CBC w/ ECDHE
// AEADs w/o ECDHE
// CBC w/o ECDHE
// 3DES
// CBC_SHA256
// RC4
// cipherSuitesPreferenceOrder is the order in which we'll select (on the
// server) or advertise (on the client) TLS 1.0–1.2 cipher suites.
//
// Cipher suites are filtered but not reordered based on the application and
// peer's preferences, meaning we'll never select a suite lower in this list if
// any higher one is available. This makes it more defensible to keep weaker
// cipher suites enabled, especially on the server side where we get the last
// word, since there are no known downgrade attacks on cipher suites selection.
//
// The list is sorted by applying the following priority rules, stopping at the
// first (most important) applicable one:
//
//   - Anything else comes before RC4
//
//     RC4 has practically exploitable biases. See https://www.rc4nomore.com.
//
//   - Anything else comes before CBC_SHA256
//
//     SHA-256 variants of the CBC ciphersuites don't implement any Lucky13
//     countermeasures. See https://www.isg.rhul.ac.uk/tls/Lucky13.html and
//     https://www.imperialviolet.org/2013/02/04/luckythirteen.html.
//
//   - Anything else comes before 3DES
//
//     3DES has 64-bit blocks, which makes it fundamentally susceptible to
//     birthday attacks. See https://sweet32.info.
//
//   - ECDHE comes before anything else
//
//     Once we got the broken stuff out of the way, the most important
//     property a cipher suite can have is forward secrecy. We don't
//     implement FFDHE, so that means ECDHE.
//
//   - AEADs come before CBC ciphers
//
//     Even with Lucky13 countermeasures, MAC-then-Encrypt CBC cipher suites
//     are fundamentally fragile, and suffered from an endless sequence of
//     padding oracle attacks. See https://eprint.iacr.org/2015/1129,
//     https://www.imperialviolet.org/2014/12/08/poodleagain.html, and
//     https://blog.cloudflare.com/yet-another-padding-oracle-in-openssl-cbc-ciphersuites/.
//
//   - AES comes before ChaCha20
//
//     When AES hardware is available, AES-128-GCM and AES-256-GCM are faster
//     than ChaCha20Poly1305.
//
//     When AES hardware is not available, AES-128-GCM is one or more of: much
//     slower, way more complex, and less safe (because not constant time)
//     than ChaCha20Poly1305.
//
//     We use this list if we think both peers have AES hardware, and
//     cipherSuitesPreferenceOrderNoAES otherwise.
//
//   - AES-128 comes before AES-256
//
//     The only potential advantages of AES-256 are better multi-target
//     margins, and hypothetical post-quantum properties. Neither apply to
//     TLS, and AES-256 is slower due to its four extra rounds (which don't
//     contribute to the advantages above).
//
//   - ECDSA comes before RSA
//
//     The relative order of ECDSA and RSA cipher suites doesn't matter,
//     as they depend on the certificate. Pick one to get a stable order.
internal static slice<uint16> cipherSuitesPreferenceOrder = new uint16[]{
    TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
    TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
    TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305, TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,
    TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA, TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,
    TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA, TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,
    TLS_RSA_WITH_AES_128_GCM_SHA256,
    TLS_RSA_WITH_AES_256_GCM_SHA384,
    TLS_RSA_WITH_AES_128_CBC_SHA,
    TLS_RSA_WITH_AES_256_CBC_SHA,
    TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA,
    TLS_RSA_WITH_3DES_EDE_CBC_SHA,
    TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256, TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,
    TLS_RSA_WITH_AES_128_CBC_SHA256,
    TLS_ECDHE_ECDSA_WITH_RC4_128_SHA, TLS_ECDHE_RSA_WITH_RC4_128_SHA,
    TLS_RSA_WITH_RC4_128_SHA
}.slice();

// ChaCha20Poly1305
// AES-GCM w/ ECDHE
// The rest of cipherSuitesPreferenceOrder.
internal static slice<uint16> cipherSuitesPreferenceOrderNoAES = new uint16[]{
    TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305, TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,
    TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
    TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
    TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA, TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,
    TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA, TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,
    TLS_RSA_WITH_AES_128_GCM_SHA256,
    TLS_RSA_WITH_AES_256_GCM_SHA384,
    TLS_RSA_WITH_AES_128_CBC_SHA,
    TLS_RSA_WITH_AES_256_CBC_SHA,
    TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA,
    TLS_RSA_WITH_3DES_EDE_CBC_SHA,
    TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256, TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,
    TLS_RSA_WITH_AES_128_CBC_SHA256,
    TLS_ECDHE_ECDSA_WITH_RC4_128_SHA, TLS_ECDHE_RSA_WITH_RC4_128_SHA,
    TLS_RSA_WITH_RC4_128_SHA
}.slice();

// CBC_SHA256
// RC4
// disabledCipherSuites are not used unless explicitly listed in Config.CipherSuites.
internal static map<uint16, bool> disabledCipherSuites = new map<uint16, bool>{
    [TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256] = true,
    [TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256] = true,
    [TLS_RSA_WITH_AES_128_CBC_SHA256] = true,
    [TLS_ECDHE_ECDSA_WITH_RC4_128_SHA] = true,
    [TLS_ECDHE_RSA_WITH_RC4_128_SHA] = true,
    [TLS_RSA_WITH_RC4_128_SHA] = true
};

// rsaKexCiphers contains the ciphers which use RSA based key exchange,
// which we also disable by default unless a GODEBUG is set.
internal static map<uint16, bool> rsaKexCiphers = new map<uint16, bool>{
    [TLS_RSA_WITH_RC4_128_SHA] = true,
    [TLS_RSA_WITH_3DES_EDE_CBC_SHA] = true,
    [TLS_RSA_WITH_AES_128_CBC_SHA] = true,
    [TLS_RSA_WITH_AES_256_CBC_SHA] = true,
    [TLS_RSA_WITH_AES_128_CBC_SHA256] = true,
    [TLS_RSA_WITH_AES_128_GCM_SHA256] = true,
    [TLS_RSA_WITH_AES_256_GCM_SHA384] = true
};

// tdesCiphers contains 3DES ciphers,
// which we also disable by default unless a GODEBUG is set.
internal static map<uint16, bool> tdesCiphers = new map<uint16, bool>{
    [TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA] = true,
    [TLS_RSA_WITH_3DES_EDE_CBC_SHA] = true
};

internal static bool hasGCMAsmAMD64 = cpu.X86.HasAES && cpu.X86.HasPCLMULQDQ && cpu.X86.HasSSE41 && cpu.X86.HasSSSE3;
internal static bool hasGCMAsmARM64 = cpu.ARM64.HasAES && cpu.ARM64.HasPMULL;
internal static bool hasGCMAsmS390X = cpu.S390X.HasAES && cpu.S390X.HasAESCTR && cpu.S390X.HasGHASH;
internal static bool hasGCMAsmPPC64 = runtime.GOARCH == "ppc64"u8 || runtime.GOARCH == "ppc64le"u8;
internal static bool hasAESGCMHardwareSupport = hasGCMAsmAMD64 || hasGCMAsmARM64 || hasGCMAsmS390X || hasGCMAsmPPC64;

// TLS 1.2
// TLS 1.3
internal static map<uint16, bool> aesgcmCiphers = new map<uint16, bool>{
    [TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256] = true,
    [TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384] = true,
    [TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256] = true,
    [TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384] = true,
    [TLS_AES_128_GCM_SHA256] = true,
    [TLS_AES_256_GCM_SHA384] = true
};

// aesgcmPreferred returns whether the first known cipher in the preference list
// is an AES-GCM cipher, implying the peer has hardware support for it.
internal static bool aesgcmPreferred(slice<uint16> ciphers) {
    foreach (var (_, cID) in ciphers) {
        {
            var c = cipherSuiteByID(cID); if (c != nil) {
                return aesgcmCiphers[cID];
            }
        }
        {
            var c = cipherSuiteTLS13ByID(cID); if (c != nil) {
                return aesgcmCiphers[cID];
            }
        }
    }
    return false;
}

internal static any cipherRC4(slice<byte> key, slice<byte> iv, bool isRead) {
    var (cipher, _) = rc4.NewCipher(key);
    return cipher.OrTypedNil();
}

internal static any cipher3DES(slice<byte> key, slice<byte> iv, bool isRead) {
    var (block, _) = des.NewTripleDESCipher(key);
    if (isRead) {
        return cipher.NewCBCDecrypter(block, iv);
    }
    return cipher.NewCBCEncrypter(block, iv);
}

internal static any cipherAES(slice<byte> key, slice<byte> iv, bool isRead) {
    var (block, _) = aes.NewCipher(key);
    if (isRead) {
        return cipher.NewCBCDecrypter(block, iv);
    }
    return cipher.NewCBCEncrypter(block, iv);
}

// macSHA1 returns a SHA-1 based constant time MAC.
internal static hash.Hash macSHA1(slice<byte> key) {
    var h = sha1.New;
    // The BoringCrypto SHA1 does not have a constant-time
    // checksum function, so don't try to use it.
    if (!boring.Enabled) {
        h = newConstantTimeHash(h);
    }
    return hmac.New(h, key);
}

// macSHA256 returns a SHA-256 based MAC. This is only supported in TLS 1.2 and
// is currently only used in disabled-by-default cipher suites.
internal static hash.Hash macSHA256(slice<byte> key) {
    return hmac.New(sha256.New, key);
}

partial interface aead :
    cipher.AEAD
{
    // explicitNonceLen returns the number of bytes of explicit nonce
    // included in each record. This is eight for older AEADs and
    // zero for modern ones.
    nint explicitNonceLen();
}

internal static UntypedInt aeadNonceLength => 12;
internal static UntypedInt noncePrefixLength => 4;

// prefixNonceAEAD wraps an AEAD and prefixes a fixed portion of the nonce to
// each call.
partial struct prefixNonceAEAD {
    // nonce contains the fixed part of the nonce in the first four bytes.
    internal array<byte> nonce = new(aeadNonceLength);
    internal cipher.AEAD aead;
}

internal static nint NonceSize(this ref prefixNonceAEAD f) {
    return aeadNonceLength - noncePrefixLength;
}

internal static nint Overhead(this ref prefixNonceAEAD f) {
    return f.aead.Overhead();
}

internal static nint explicitNonceLen(this ref prefixNonceAEAD f) {
    return f.NonceSize();
}

internal static slice<byte> Seal(this ref prefixNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    copy(f.nonce[4..], nonce);
    return f.aead.Seal(@out, f.nonce[..], plaintext, additionalData);
}

internal static (slice<byte>, error) Open(this ref prefixNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    copy(f.nonce[4..], nonce);
    return f.aead.Open(@out, f.nonce[..], ciphertext, additionalData);
}

// xorNonceAEAD wraps an AEAD by XORing in a fixed pattern to the nonce
// before each call.
partial struct xorNonceAEAD {
    internal array<byte> nonceMask = new(aeadNonceLength);
    internal cipher.AEAD aead;
}

internal static nint NonceSize(this ref xorNonceAEAD f) {
    return 8; // 64-bit sequence number
}

internal static nint Overhead(this ref xorNonceAEAD f) {
    return f.aead.Overhead();
}

internal static nint explicitNonceLen(this ref xorNonceAEAD f) {
    return 0;
}

internal static slice<byte> Seal(this ref xorNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    foreach (var (i, b) in nonce) {
        f.nonceMask[4 + i] ^= (byte)(b);
    }
    var result = f.aead.Seal(@out, f.nonceMask[..], plaintext, additionalData);
    foreach (var (i, b) in nonce) {
        f.nonceMask[4 + i] ^= (byte)(b);
    }
    return result;
}

internal static (slice<byte>, error) Open(this ref xorNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    foreach (var (i, b) in nonce) {
        f.nonceMask[4 + i] ^= (byte)(b);
    }
    var (result, err) = f.aead.Open(@out, f.nonceMask[..], ciphertext, additionalData);
    foreach (var (i, b) in nonce) {
        f.nonceMask[4 + i] ^= (byte)(b);
    }
    return (result, err);
}

internal static aead aeadAESGCM(slice<byte> key, slice<byte> noncePrefix) {
    if (len(noncePrefix) != noncePrefixLength) {
        throw panic("tls: internal error: wrong nonce length");
    }
    var (aesΔ1, err) = aes.NewCipher(key);
    if (err != default!) {
        throw panic(err);
    }
    cipher.AEAD aead = default!;
    if (boring.Enabled){
        (aead, err) = boring.NewGCMTLS(aesΔ1);
    } else {
        boring.Unreachable();
        var (ᴛ1, ᴛ2) = gcm.NewGCMForTLS12(aesΔ1._<ж<fipsaes.Block>>());
        (aead, err) = (new gcm_GCMForTLS12жAEAD(ᴛ1), ᴛ2);
    }
    if (err != default!) {
        throw panic(err);
    }
    var ret = Ꮡ(new prefixNonceAEAD(aead: aead));
    copy((~ret).nonce[..], noncePrefix);
    return new prefixNonceAEADжaead(ret);
}

// aeadAESGCMTLS13 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/xtls/xray-core
//   - github.com/v2fly/v2ray-core
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname aeadAESGCMTLS13
internal static aead aeadAESGCMTLS13(slice<byte> key, slice<byte> nonceMask) {
    if (len(nonceMask) != aeadNonceLength) {
        throw panic("tls: internal error: wrong nonce length");
    }
    var (aesΔ1, err) = aes.NewCipher(key);
    if (err != default!) {
        throw panic(err);
    }
    cipher.AEAD aead = default!;
    if (boring.Enabled){
        (aead, err) = boring.NewGCMTLS13(aesΔ1);
    } else {
        boring.Unreachable();
        var (ᴛ1, ᴛ2) = gcm.NewGCMForTLS13(aesΔ1._<ж<fipsaes.Block>>());
        (aead, err) = (new gcm_GCMForTLS13жAEAD(ᴛ1), ᴛ2);
    }
    if (err != default!) {
        throw panic(err);
    }
    var ret = Ꮡ(new xorNonceAEAD(aead: aead));
    copy((~ret).nonceMask[..], nonceMask);
    return new xorNonceAEADжaead(ret);
}

internal static aead aeadChaCha20Poly1305(slice<byte> key, slice<byte> nonceMask) {
    if (len(nonceMask) != aeadNonceLength) {
        throw panic("tls: internal error: wrong nonce length");
    }
    var (aead, err) = chacha20poly1305.New(key);
    if (err != default!) {
        throw panic(err);
    }
    var ret = Ꮡ(new xorNonceAEAD(aead: aead));
    copy((~ret).nonceMask[..], nonceMask);
    return new xorNonceAEADжaead(ret);
}

partial interface constantTimeHash :
    hash.Hash
{
    slice<byte> ConstantTimeSum(slice<byte> b);
}

// cthWrapper wraps any hash.Hash that implements ConstantTimeSum, and replaces
// with that all calls to Sum. It's used to obtain a ConstantTimeSum-based HMAC.
partial struct cthWrapper {
    internal constantTimeHash h;
}

internal static nint Size(this ref cthWrapper c) {
    return c.h.Size();
}

internal static nint BlockSize(this ref cthWrapper c) {
    return c.h.BlockSize();
}

internal static void Reset(this ref cthWrapper c) {
    c.h.Reset();
}

internal static (nint, error) Write(this ref cthWrapper c, slice<byte> p) {
    return c.h.Write(p);
}

internal static slice<byte> Sum(this ref cthWrapper c, slice<byte> b) {
    return c.h.ConstantTimeSum(b);
}

internal static Func<hash.Hash> newConstantTimeHash(Func<hash.Hash> h) {
    boring.Unreachable();
    return () => new cthWrapperжHash(Ꮡ(new cthWrapper(h()._<constantTimeHash>())));
}

// tls10MAC implements the TLS 1.0 MAC function. RFC 2246, Section 6.2.3.
internal static slice<byte> tls10MAC(hash.Hash h, slice<byte> @out, slice<byte> seq, slice<byte> header, slice<byte> data, slice<byte> extra) {
    h.Reset();
    h.Write(seq);
    h.Write(header);
    h.Write(data);
    var res = h.Sum(@out);
    if (extra != default!) {
        h.Write(extra);
    }
    return res;
}

internal static keyAgreement rsaKA(uint16 version) {
    return new rsaKeyAgreement(nil);
}

internal static keyAgreement ecdheECDSAKA(uint16 version) {
    return new ecdheKeyAgreementжkeyAgreement(Ꮡ(new ecdheKeyAgreement(
        isRSA: false,
        version: version
    )));
}

internal static keyAgreement ecdheRSAKA(uint16 version) {
    return new ecdheKeyAgreementжkeyAgreement(Ꮡ(new ecdheKeyAgreement(
        isRSA: true,
        version: version
    )));
}

// mutualCipherSuite returns a cipherSuite given a list of supported
// ciphersuites and the id requested by the peer.
internal static ж<cipherSuite> mutualCipherSuite(slice<uint16> have, uint16 want) {
    foreach (var (_, id) in have) {
        if (id == want) {
            return cipherSuiteByID(id);
        }
    }
    return default!;
}

internal static ж<cipherSuite> cipherSuiteByID(uint16 id) {
    foreach (var (_, cipherSuite) in ΔcipherSuites) {
        if ((~cipherSuite).id == id) {
            return cipherSuite;
        }
    }
    return default!;
}

internal static ж<cipherSuiteTLS13> mutualCipherSuiteTLS13(slice<uint16> have, uint16 want) {
    foreach (var (_, id) in have) {
        if (id == want) {
            return cipherSuiteTLS13ByID(id);
        }
    }
    return default!;
}

internal static ж<cipherSuiteTLS13> cipherSuiteTLS13ByID(uint16 id) {
    foreach (var (_, cipherSuite) in cipherSuitesTLS13) {
        if ((~cipherSuite).id == id) {
            return cipherSuite;
        }
    }
    return default!;
}

// A list of cipher suite IDs that are, or have been, implemented by this
// package.
//
// See https://www.iana.org/assignments/tls-parameters/tls-parameters.xml
public const uint16 TLS_RSA_WITH_RC4_128_SHA = 0x0005;

public const uint16 TLS_RSA_WITH_3DES_EDE_CBC_SHA = 0x000a;

public const uint16 TLS_RSA_WITH_AES_128_CBC_SHA = 0x002f;

public const uint16 TLS_RSA_WITH_AES_256_CBC_SHA = 0x0035;

public const uint16 TLS_RSA_WITH_AES_128_CBC_SHA256 = 0x003c;

public const uint16 TLS_RSA_WITH_AES_128_GCM_SHA256 = 0x009c;

public const uint16 TLS_RSA_WITH_AES_256_GCM_SHA384 = 0x009d;

public const uint16 TLS_ECDHE_ECDSA_WITH_RC4_128_SHA = 0xc007;

public const uint16 TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA = 0xc009;

public const uint16 TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA = 0xc00a;

public const uint16 TLS_ECDHE_RSA_WITH_RC4_128_SHA = 0xc011;

public const uint16 TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA = 0xc012;

public const uint16 TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA = 0xc013;

public const uint16 TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA = 0xc014;

public const uint16 TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256 = 0xc023;

public const uint16 TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256 = 0xc027;

public const uint16 TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 = 0xc02f;

public const uint16 TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256 = 0xc02b;

public const uint16 TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384 = 0xc030;

public const uint16 TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384 = 0xc02c;

public const uint16 TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256 = 0xcca8;

public const uint16 TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256 = 0xcca9;

public const uint16 TLS_AES_128_GCM_SHA256 = 0x1301;

public const uint16 TLS_AES_256_GCM_SHA384 = 0x1302;

public const uint16 TLS_CHACHA20_POLY1305_SHA256 = 0x1303;

public const uint16 TLS_FALLBACK_SCSV = 0x5600;

public const uint16 TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305 = /* TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256 */ 52392;

public const uint16 TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305 = /* TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256 */ 52393;

} // end tls_package
