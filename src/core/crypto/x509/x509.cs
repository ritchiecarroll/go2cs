// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package x509 implements a subset of the X.509 standard.
//
// It allows parsing and generating certificates, certificate signing
// requests, certificate revocation lists, and encoded public and private keys.
// It provides a certificate verifier, complete with a chain builder.
//
// The package targets the X.509 technical profile defined by the IETF (RFC
// 2459/3280/5280), and as further restricted by the CA/Browser Forum Baseline
// Requirements. There is minimal support for features outside of these
// profiles, as the primary goal of the package is to provide compatibility
// with the publicly trusted TLS certificate ecosystem and its policies and
// constraints.
//
// On macOS and Windows, certificate verification is handled by system APIs, but
// the package aims to apply consistent validation rules across operating
// systems.
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using ecdh = go.crypto.ecdh_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using elliptic = go.crypto.elliptic_package;
using cryptorand = go.crypto.rand_package;
using rsa = go.crypto.rsa_package;
using sha1 = go.crypto.sha1_package;
using pkix = go.crypto.x509.pkix_package;
using asn1 = encoding.asn1_package;
using pem = encoding.pem_package;
using errors = errors_package;
using fmt = fmt_package;
using godebug = go.@internal.godebug_package;
using io = io_package;
using big = go.math.big_package;
using net = net_package;
using url = go.net.url_package;
using strconv = strconv_package;
using time = time_package;
using unicode = unicode_package;
// blank import: go.crypto.sha1_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.crypto.sha256_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.crypto.sha512_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using cryptobyte_asn1 = vendor.golang.org.x.crypto.cryptobyte.asn1_package;
using encoding;
using go.@internal;
using go.crypto;
using go.crypto.x509;
using go.math;
using go.net;
using hash = hash_package;
using vendor.golang.org.x.crypto;
using vendor.golang.org.x.crypto.cryptobyte;

partial class x509_package {

// pkixPublicKey reflects a PKIX public key structure. See SubjectPublicKeyInfo
// in RFC 3280.
[GoType] partial struct pkixPublicKey {
    public pkix.AlgorithmIdentifier Algo;
    public asn1.BitString BitString;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509FailedToParsePublicˢ2 = "x509: failed to parse public key (use ParsePKCS1PublicKey instead for this key format)"u8;
internal static readonly @string x509TrailingDataAfterAsnˢ = "x509: trailing data after ASN.1 of public-key"u8;

// ParsePKIXPublicKey parses a public key in PKIX, ASN.1 DER form. The encoded
// public key is a SubjectPublicKeyInfo structure (see RFC 5280, Section 4.1).
//
// It returns a *[rsa.PublicKey], *[dsa.PublicKey], *[ecdsa.PublicKey],
// [ed25519.PublicKey] (not a pointer), or *[ecdh.PublicKey] (for X25519).
// More types might be supported in the future.
//
// This kind of key is commonly encoded in PEM blocks of type "PUBLIC KEY".
public static (any pub, error err) ParsePKIXPublicKey(slice<byte> derBytes) {
    ref var pki = ref heap(new publicKeyInfo(), out var Ꮡpki);
    {
        var (rest, errΔ1) = asn1.Unmarshal(derBytes, Ꮡpki); if (errΔ1 != default!){
            {
                var (_, errΔ2) = asn1.Unmarshal(derBytes, Ꮡ(new pkcs1PublicKey(nil))); if (errΔ2 == default!) {
                    return (default!, errors.New(x509FailedToParsePublicˢ2));
                }
            }
            return (default!, errΔ1);
        } else 
        if (builtin.len(rest) != 0) {
            return (default!, errors.New(x509TrailingDataAfterAsnˢ));
        }
    }
    return parsePublicKey(ref pki);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InvalidEllipticCurveˢ2 = "x509: invalid elliptic curve public key"u8;

internal static (slice<byte> publicKeyBytes, pkix.AlgorithmIdentifier publicKeyAlgorithm, error err) marshalPublicKey(any pub) {
    slice<byte> publicKeyBytes = default!;
    pkix.AlgorithmIdentifier publicKeyAlgorithm = default!;
    error err = default!;

    switch (pub.type()) {
    case ж<rsa.PublicKey> pubΔ1: {
        (publicKeyBytes, err) = asn1.Marshal(new pkcs1PublicKey(
            N: (~pubΔ1).N,
            E: (~pubΔ1).E
        ));
        if (err != default!) {
            return (default!, new pkix.AlgorithmIdentifier(nil), err);
        }
        publicKeyAlgorithm.Algorithm = oidPublicKeyRSA;
        publicKeyAlgorithm.Parameters = asn1.NullRawValue;
        break;
    }
    case ж<ecdsa.PublicKey> pubΔ1: {
        var (oid, ok) = oidFromNamedCurve((~pubΔ1).Curve);
        if (!ok) {
            // This is a NULL parameters value which is required by
            // RFC 3279, Section 2.3.1.
            return (default!, new pkix.AlgorithmIdentifier(nil), errors.New(x509UnsupportedEllipticˢ));
        }
        if (!(~pubΔ1).Curve.IsOnCurve((~pubΔ1).X, (~pubΔ1).Y)) {
            return (default!, new pkix.AlgorithmIdentifier(nil), errors.New(x509InvalidEllipticCurveˢ2));
        }
        publicKeyBytes = elliptic.Marshal((~pubΔ1).Curve, (~pubΔ1).X, (~pubΔ1).Y);
        publicKeyAlgorithm.Algorithm = oidPublicKeyECDSA;
        slice<byte> paramBytes = default!;
        (paramBytes, err) = asn1.Marshal(oid);
        if (err != default!) {
            return (publicKeyBytes, publicKeyAlgorithm, err);
        }
        publicKeyAlgorithm.Parameters.FullBytes = paramBytes;
        break;
    }
    case ed25519.PublicKey pubΔ1: {
        publicKeyBytes = pubΔ1;
        publicKeyAlgorithm.Algorithm = oidPublicKeyEd25519;
        break;
    }
    case ж<ecdhꓸPublicKey> pubΔ1: {
        publicKeyBytes = pubΔ1.Bytes();
        if (AreEqual(pubΔ1.Curve(), ecdh.X25519())){
            publicKeyAlgorithm.Algorithm = oidPublicKeyX25519;
        } else {
            var (oid, ok) = oidFromECDHCurve(pubΔ1.Curve());
            if (!ok) {
                return (default!, new pkix.AlgorithmIdentifier(nil), errors.New(x509UnsupportedEllipticˢ));
            }
            publicKeyAlgorithm.Algorithm = oidPublicKeyECDSA;
            slice<byte> paramBytes = default!;
            (paramBytes, err) = asn1.Marshal(oid);
            if (err != default!) {
                return (publicKeyBytes, publicKeyAlgorithm, err);
            }
            publicKeyAlgorithm.Parameters.FullBytes = paramBytes;
        }
        break;
    }
    default: {
        var pubΔ1 = pub;
        return (default!, new pkix.AlgorithmIdentifier(nil), fmt.Errorf("x509: unsupported public key type: %T"u8, pubΔ1));
    }}
    return (publicKeyBytes, publicKeyAlgorithm, default!);
}

// MarshalPKIXPublicKey converts a public key to PKIX, ASN.1 DER form.
// The encoded public key is a SubjectPublicKeyInfo structure
// (see RFC 5280, Section 4.1).
//
// The following key types are currently supported: *[rsa.PublicKey],
// *[ecdsa.PublicKey], [ed25519.PublicKey] (not a pointer), and *[ecdh.PublicKey].
// Unsupported key types result in an error.
//
// This kind of key is commonly encoded in PEM blocks of type "PUBLIC KEY".
public static (slice<byte>, error) MarshalPKIXPublicKey(any pub) {
    slice<byte> publicKeyBytes = default!;
    pkix.AlgorithmIdentifier publicKeyAlgorithm = default!;
    error err = default!;
    {
        (publicKeyBytes, publicKeyAlgorithm, err) = marshalPublicKey(pub); if (err != default!) {
            return (default!, err);
        }
    }
    var pkixΔ1 = new pkixPublicKey(
        Algo: publicKeyAlgorithm,
        BitString: new asn1.BitString(
            Bytes: publicKeyBytes,
            BitLength: 8 * builtin.len(publicKeyBytes)
        )
    );
    var (ret, _) = asn1.Marshal(pkixΔ1);
    return (ret, default!);
}

// These structures reflect the ASN.1 structure of X.509 certificates.:
[GoType] partial struct certificate {
    public tbsCertificate TBSCertificate;
    public pkix.AlgorithmIdentifier SignatureAlgorithm;
    public asn1.BitString SignatureValue;
}

[GoType] public partial struct tbsCertificate {
    public asn1.RawContent Raw;
    [GoTag(@"asn1:""optional,explicit,default:0,tag:0""")]
    public nint Version;
    public ж<bigꓸInt> SerialNumber;
    public pkix.AlgorithmIdentifier SignatureAlgorithm;
    public asn1.RawValue Issuer;
    public validity Validity;
    public asn1.RawValue Subject;
    public publicKeyInfo PublicKey;
    [GoTag(@"asn1:""optional,tag:1""")]
    public asn1.BitString UniqueId;
    [GoTag(@"asn1:""optional,tag:2""")]
    public asn1.BitString SubjectUniqueId;
    [GoTag(@"asn1:""omitempty,optional,explicit,tag:3""")]
    public slice<pkix.Extension> Extensions;
}

[GoType] partial struct dsaAlgorithmParameters {
    public ж<bigꓸInt> P, Q, G;
}

[GoType] public partial struct validity {
    public time.Time NotBefore, NotAfter;
}

[GoType] public partial struct publicKeyInfo {
    public asn1.RawContent Raw;
    public pkix.AlgorithmIdentifier Algorithm;
    public asn1.BitString PublicKey;
}

// RFC 5280,  4.2.1.1
[GoType] partial struct authKeyId {
    [GoTag(@"asn1:""optional,tag:0""")]
    public slice<byte> Id;
}

[GoType("num:nint")] partial struct SignatureAlgorithm;

public static SignatureAlgorithm UnknownSignatureAlgorithm => /* iota */ 0;
public static SignatureAlgorithm MD2WithRSA => 1; // Unsupported.
public static SignatureAlgorithm MD5WithRSA => 2; // Only supported for signing, not verification.
public static SignatureAlgorithm SHA1WithRSA => 3; // Only supported for signing, and verification of CRLs, CSRs, and OCSP responses.
public static SignatureAlgorithm SHA256WithRSA => 4;
public static SignatureAlgorithm SHA384WithRSA => 5;
public static SignatureAlgorithm SHA512WithRSA => 6;
public static SignatureAlgorithm DSAWithSHA1 => 7; // Unsupported.
public static SignatureAlgorithm DSAWithSHA256 => 8; // Unsupported.
public static SignatureAlgorithm ECDSAWithSHA1 => 9; // Only supported for signing, and verification of CRLs, CSRs, and OCSP responses.
public static SignatureAlgorithm ECDSAWithSHA256 => 10;
public static SignatureAlgorithm ECDSAWithSHA384 => 11;
public static SignatureAlgorithm ECDSAWithSHA512 => 12;
public static SignatureAlgorithm SHA256WithRSAPSS => 13;
public static SignatureAlgorithm SHA384WithRSAPSS => 14;
public static SignatureAlgorithm SHA512WithRSAPSS => 15;
public static SignatureAlgorithm PureEd25519 => 16;

internal static bool isRSAPSS(this SignatureAlgorithm algo) {
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == algo) {
            return details.isRSAPSS;
        }
    }
    return false;
}

internal static crypto.Hash hashFunc(this SignatureAlgorithm algo) {
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == algo) {
            return details.hash;
        }
    }
    return ((crypto.Hash)0);
}

public static @string String(this SignatureAlgorithm algo) {
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == algo) {
            return details.name;
        }
    }
    return strconv.Itoa((nint)algo);
}

[GoType("num:nint")] partial struct PublicKeyAlgorithm;

public static PublicKeyAlgorithm UnknownPublicKeyAlgorithm => /* iota */ 0;
public static PublicKeyAlgorithm RSA => 1;
public static PublicKeyAlgorithm DSA => 2; // Only supported for parsing.
public static PublicKeyAlgorithm ECDSA => 3;
public static PublicKeyAlgorithm Ed25519 => 4;

internal static array<@string> publicKeyAlgoName = new golib.SparseArray<@string>{
    [(int)RSA] = "RSA"u8,
    [(int)DSA] = "DSA"u8,
    [(int)ECDSA] = "ECDSA"u8,
    [(int)Ed25519] = "Ed25519"u8
}.array(5);

public static @string String(this PublicKeyAlgorithm algo) {
    if (0 < algo && (nint)algo < builtin.len(publicKeyAlgoName)) {
        return publicKeyAlgoName[algo];
    }
    return strconv.Itoa((nint)algo);
}

// OIDs for signature algorithms
//
//	pkcs-1 OBJECT IDENTIFIER ::= {
//		iso(1) member-body(2) us(840) rsadsi(113549) pkcs(1) 1 }
//
// RFC 3279 2.2.1 RSA Signature Algorithms
//
//	md5WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 4 }
//
//	sha-1WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 5 }
//
//	dsaWithSha1 OBJECT IDENTIFIER ::= {
//		iso(1) member-body(2) us(840) x9-57(10040) x9cm(4) 3 }
//
// RFC 3279 2.2.3 ECDSA Signature Algorithm
//
//	ecdsa-with-SHA1 OBJECT IDENTIFIER ::= {
//		iso(1) member-body(2) us(840) ansi-x962(10045)
//		signatures(4) ecdsa-with-SHA1(1)}
//
// RFC 4055 5 PKCS #1 Version 1.5
//
//	sha256WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 11 }
//
//	sha384WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 12 }
//
//	sha512WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 13 }
//
// RFC 5758 3.1 DSA Signature Algorithms
//
//	dsaWithSha256 OBJECT IDENTIFIER ::= {
//		joint-iso-ccitt(2) country(16) us(840) organization(1) gov(101)
//		csor(3) algorithms(4) id-dsa-with-sha2(3) 2}
//
// RFC 5758 3.2 ECDSA Signature Algorithm
//
//	ecdsa-with-SHA256 OBJECT IDENTIFIER ::= { iso(1) member-body(2)
//		us(840) ansi-X9-62(10045) signatures(4) ecdsa-with-SHA2(3) 2 }
//
//	ecdsa-with-SHA384 OBJECT IDENTIFIER ::= { iso(1) member-body(2)
//		us(840) ansi-X9-62(10045) signatures(4) ecdsa-with-SHA2(3) 3 }
//
//	ecdsa-with-SHA512 OBJECT IDENTIFIER ::= { iso(1) member-body(2)
//		us(840) ansi-X9-62(10045) signatures(4) ecdsa-with-SHA2(3) 4 }
//
// RFC 8410 3 Curve25519 and Curve448 Algorithm Identifiers
//
//	id-Ed25519   OBJECT IDENTIFIER ::= { 1 3 101 112 }
internal static asn1.ObjectIdentifier oidSignatureMD5WithRSA = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 4}.slice());

internal static asn1.ObjectIdentifier oidSignatureSHA1WithRSA = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 5}.slice());

internal static asn1.ObjectIdentifier oidSignatureSHA256WithRSA = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 11}.slice());

internal static asn1.ObjectIdentifier oidSignatureSHA384WithRSA = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 12}.slice());

internal static asn1.ObjectIdentifier oidSignatureSHA512WithRSA = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 13}.slice());

internal static asn1.ObjectIdentifier oidSignatureRSAPSS = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 10}.slice());

internal static asn1.ObjectIdentifier oidSignatureDSAWithSHA1 = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 10040, 4, 3}.slice());

internal static asn1.ObjectIdentifier oidSignatureDSAWithSHA256 = new asn1.ObjectIdentifier(new nint[]{2, 16, 840, 1, 101, 3, 4, 3, 2}.slice());

internal static asn1.ObjectIdentifier oidSignatureECDSAWithSHA1 = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 10045, 4, 1}.slice());

internal static asn1.ObjectIdentifier oidSignatureECDSAWithSHA256 = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 10045, 4, 3, 2}.slice());

internal static asn1.ObjectIdentifier oidSignatureECDSAWithSHA384 = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 10045, 4, 3, 3}.slice());

internal static asn1.ObjectIdentifier oidSignatureECDSAWithSHA512 = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 10045, 4, 3, 4}.slice());

internal static asn1.ObjectIdentifier oidSignatureEd25519 = new asn1.ObjectIdentifier(new nint[]{1, 3, 101, 112}.slice());

internal static asn1.ObjectIdentifier oidSHA256 = new asn1.ObjectIdentifier(new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 1}.slice());

internal static asn1.ObjectIdentifier oidSHA384 = new asn1.ObjectIdentifier(new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 2}.slice());

internal static asn1.ObjectIdentifier oidSHA512 = new asn1.ObjectIdentifier(new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 3}.slice());

internal static asn1.ObjectIdentifier oidMGF1 = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 8}.slice());

internal static asn1.ObjectIdentifier oidISOSignatureSHA1WithRSA = new asn1.ObjectIdentifier(new nint[]{1, 3, 14, 3, 2, 29}.slice());

/* no pre-hashing */

[GoType("dyn")] partial struct signatureAlgorithmDetailsᴛ1 {
    internal SignatureAlgorithm algo;
    internal @string name;
    internal asn1.ObjectIdentifier oid;
    internal asn1.RawValue @params;
    internal PublicKeyAlgorithm pubKeyAlgo;
    internal crypto.Hash hash;
    internal bool isRSAPSS;
}
internal static slice<signatureAlgorithmDetailsᴛ1> signatureAlgorithmDetails;
internal static void initᴛsignatureAlgorithmDetails() { signatureAlgorithmDetails = new signatureAlgorithmDetailsᴛ1[]{
    new(MD5WithRSA, "MD5-RSA"u8, oidSignatureMD5WithRSA, asn1.NullRawValue, RSA, crypto.MD5, false),
    new(SHA1WithRSA, "SHA1-RSA"u8, oidSignatureSHA1WithRSA, asn1.NullRawValue, RSA, crypto.SHA1, false),
    new(SHA1WithRSA, "SHA1-RSA"u8, oidISOSignatureSHA1WithRSA, asn1.NullRawValue, RSA, crypto.SHA1, false),
    new(SHA256WithRSA, "SHA256-RSA"u8, oidSignatureSHA256WithRSA, asn1.NullRawValue, RSA, crypto.SHA256, false),
    new(SHA384WithRSA, "SHA384-RSA"u8, oidSignatureSHA384WithRSA, asn1.NullRawValue, RSA, crypto.SHA384, false),
    new(SHA512WithRSA, "SHA512-RSA"u8, oidSignatureSHA512WithRSA, asn1.NullRawValue, RSA, crypto.SHA512, false),
    new(SHA256WithRSAPSS, "SHA256-RSAPSS"u8, oidSignatureRSAPSS, pssParametersSHA256, RSA, crypto.SHA256, true),
    new(SHA384WithRSAPSS, "SHA384-RSAPSS"u8, oidSignatureRSAPSS, pssParametersSHA384, RSA, crypto.SHA384, true),
    new(SHA512WithRSAPSS, "SHA512-RSAPSS"u8, oidSignatureRSAPSS, pssParametersSHA512, RSA, crypto.SHA512, true),
    new(DSAWithSHA1, "DSA-SHA1"u8, oidSignatureDSAWithSHA1, emptyRawValue, DSA, crypto.SHA1, false),
    new(DSAWithSHA256, "DSA-SHA256"u8, oidSignatureDSAWithSHA256, emptyRawValue, DSA, crypto.SHA256, false),
    new(ECDSAWithSHA1, "ECDSA-SHA1"u8, oidSignatureECDSAWithSHA1, emptyRawValue, ECDSA, crypto.SHA1, false),
    new(ECDSAWithSHA256, "ECDSA-SHA256"u8, oidSignatureECDSAWithSHA256, emptyRawValue, ECDSA, crypto.SHA256, false),
    new(ECDSAWithSHA384, "ECDSA-SHA384"u8, oidSignatureECDSAWithSHA384, emptyRawValue, ECDSA, crypto.SHA384, false),
    new(ECDSAWithSHA512, "ECDSA-SHA512"u8, oidSignatureECDSAWithSHA512, emptyRawValue, ECDSA, crypto.SHA512, false),
    new(PureEd25519, "Ed25519"u8, oidSignatureEd25519, emptyRawValue, Ed25519, ((crypto.Hash)0), false)
}.slice(); }

internal static asn1.RawValue emptyRawValue = new asn1.RawValue(nil);

// DER encoded RSA PSS parameters for the
// SHA256, SHA384, and SHA512 hashes as defined in RFC 3447, Appendix A.2.3.
// The parameters contain the following values:
//   - hashAlgorithm contains the associated hash identifier with NULL parameters
//   - maskGenAlgorithm always contains the default mgf1SHA1 identifier
//   - saltLength contains the length of the associated hash
//   - trailerField always contains the default trailerFieldBC value
internal static asn1.RawValue pssParametersSHA256 = new asn1.RawValue(FullBytes: new byte[]{48, 52, 160, 15, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 1, 5, 0, 161, 28, 48, 26, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 8, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 1, 5, 0, 162, 3, 2, 1, 32}.slice());

internal static asn1.RawValue pssParametersSHA384 = new asn1.RawValue(FullBytes: new byte[]{48, 52, 160, 15, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 2, 5, 0, 161, 28, 48, 26, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 8, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 2, 5, 0, 162, 3, 2, 1, 48}.slice());

internal static asn1.RawValue pssParametersSHA512 = new asn1.RawValue(FullBytes: new byte[]{48, 52, 160, 15, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 3, 5, 0, 161, 28, 48, 26, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 8, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 3, 5, 0, 162, 3, 2, 1, 64}.slice());

// pssParameters reflects the parameters in an AlgorithmIdentifier that
// specifies RSA PSS. See RFC 3447, Appendix A.2.3.
[GoType] partial struct pssParameters {
    // The following three fields are not marked as
    // optional because the default values specify SHA-1,
    // which is no longer suitable for use in signatures.
    [GoTag(@"asn1:""explicit,tag:0""")]
    public pkix.AlgorithmIdentifier Hash;
    [GoTag(@"asn1:""explicit,tag:1""")]
    public pkix.AlgorithmIdentifier MGF;
    [GoTag(@"asn1:""explicit,tag:2""")]
    public nint SaltLength;
    [GoTag(@"asn1:""optional,explicit,tag:3,default:1""")]
    public nint TrailerField;
}

internal static SignatureAlgorithm getSignatureAlgorithmFromAI(pkix.AlgorithmIdentifier ai) {
    if (ai.Algorithm.Equal(oidSignatureEd25519)) {
        // RFC 8410, Section 3
        // > For all of the OIDs, the parameters MUST be absent.
        if (builtin.len(ai.Parameters.FullBytes) != 0) {
            return UnknownSignatureAlgorithm;
        }
    }
    if (!ai.Algorithm.Equal(oidSignatureRSAPSS)) {
        foreach (var (_, details) in signatureAlgorithmDetails) {
            if (ai.Algorithm.Equal(details.oid)) {
                return details.algo;
            }
        }
        return UnknownSignatureAlgorithm;
    }
    // RSA PSS is special because it encodes important parameters
    // in the Parameters.
    ref var @params = ref heap(new pssParameters(), out var Ꮡparams);
    {
        var (_, err) = asn1.Unmarshal(ai.Parameters.FullBytes, Ꮡparams); if (err != default!) {
            return UnknownSignatureAlgorithm;
        }
    }
    ref var mgf1HashFunc = ref heap(new pkix.AlgorithmIdentifier(), out var Ꮡmgf1HashFunc);
    {
        var (_, err) = asn1.Unmarshal(@params.MGF.Parameters.FullBytes, Ꮡmgf1HashFunc); if (err != default!) {
            return UnknownSignatureAlgorithm;
        }
    }
    // PSS is greatly overburdened with options. This code forces them into
    // three buckets by requiring that the MGF1 hash function always match the
    // message hash function (as recommended in RFC 3447, Section 8.1), that the
    // salt length matches the hash length, and that the trailer field has the
    // default value.
    if ((builtin.len(@params.Hash.Parameters.FullBytes) != 0 && !bytes.Equal(@params.Hash.Parameters.FullBytes, asn1.NullBytes)) || !@params.MGF.Algorithm.Equal(oidMGF1) || !mgf1HashFunc.Algorithm.Equal(@params.Hash.Algorithm) || (builtin.len(mgf1HashFunc.Parameters.FullBytes) != 0 && !bytes.Equal(mgf1HashFunc.Parameters.FullBytes, asn1.NullBytes)) || @params.TrailerField != 1) {
        return UnknownSignatureAlgorithm;
    }
    switch (ᐧ) {
    case {} when @params.Hash.Algorithm.Equal(oidSHA256) && @params.SaltLength == 32: {
        return SHA256WithRSAPSS;
    }
    case {} when @params.Hash.Algorithm.Equal(oidSHA384) && @params.SaltLength == 48: {
        return SHA384WithRSAPSS;
    }
    case {} when @params.Hash.Algorithm.Equal(oidSHA512) && @params.SaltLength == 64: {
        return SHA512WithRSAPSS;
    }}

    return UnknownSignatureAlgorithm;
}

internal static asn1.ObjectIdentifier oidPublicKeyRSA = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 1}.slice());
internal static asn1.ObjectIdentifier oidPublicKeyDSA = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 10040, 4, 1}.slice());
internal static asn1.ObjectIdentifier oidPublicKeyECDSA = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 10045, 2, 1}.slice());
internal static asn1.ObjectIdentifier oidPublicKeyX25519 = new asn1.ObjectIdentifier(new nint[]{1, 3, 101, 110}.slice());
internal static asn1.ObjectIdentifier oidPublicKeyEd25519 = new asn1.ObjectIdentifier(new nint[]{1, 3, 101, 112}.slice());

// getPublicKeyAlgorithmFromOID returns the exposed PublicKeyAlgorithm
// identifier for public key types supported in certificates and CSRs. Marshal
// and Parse functions may support a different set of public key types.
internal static PublicKeyAlgorithm getPublicKeyAlgorithmFromOID(asn1.ObjectIdentifier oid) {
    switch (ᐧ) {
    case {} when oid.Equal(oidPublicKeyRSA): {
        return RSA;
    }
    case {} when oid.Equal(oidPublicKeyDSA): {
        return DSA;
    }
    case {} when oid.Equal(oidPublicKeyECDSA): {
        return ECDSA;
    }
    case {} when oid.Equal(oidPublicKeyEd25519): {
        return Ed25519;
    }}

    return UnknownPublicKeyAlgorithm;
}

// RFC 5480, 2.1.1.1. Named Curve
//
//	secp224r1 OBJECT IDENTIFIER ::= {
//	  iso(1) identified-organization(3) certicom(132) curve(0) 33 }
//
//	secp256r1 OBJECT IDENTIFIER ::= {
//	  iso(1) member-body(2) us(840) ansi-X9-62(10045) curves(3)
//	  prime(1) 7 }
//
//	secp384r1 OBJECT IDENTIFIER ::= {
//	  iso(1) identified-organization(3) certicom(132) curve(0) 34 }
//
//	secp521r1 OBJECT IDENTIFIER ::= {
//	  iso(1) identified-organization(3) certicom(132) curve(0) 35 }
//
// NB: secp256r1 is equivalent to prime256v1
internal static asn1.ObjectIdentifier oidNamedCurveP224 = new asn1.ObjectIdentifier(new nint[]{1, 3, 132, 0, 33}.slice());

internal static asn1.ObjectIdentifier oidNamedCurveP256 = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 10045, 3, 1, 7}.slice());

internal static asn1.ObjectIdentifier oidNamedCurveP384 = new asn1.ObjectIdentifier(new nint[]{1, 3, 132, 0, 34}.slice());

internal static asn1.ObjectIdentifier oidNamedCurveP521 = new asn1.ObjectIdentifier(new nint[]{1, 3, 132, 0, 35}.slice());

internal static elliptic.Curve namedCurveFromOID(asn1.ObjectIdentifier oid) {
    switch (ᐧ) {
    case {} when oid.Equal(oidNamedCurveP224): {
        return elliptic.P224();
    }
    case {} when oid.Equal(oidNamedCurveP256): {
        return elliptic.P256();
    }
    case {} when oid.Equal(oidNamedCurveP384): {
        return elliptic.P384();
    }
    case {} when oid.Equal(oidNamedCurveP521): {
        return elliptic.P521();
    }}

    return default!;
}

internal static (asn1.ObjectIdentifier, bool) oidFromNamedCurve(elliptic.Curve curve) {
    var exprᴛ1 = curve;
    if (AreEqual(exprᴛ1, elliptic.P224())) {
        return (oidNamedCurveP224, true);
    }
    if (AreEqual(exprᴛ1, elliptic.P256())) {
        return (oidNamedCurveP256, true);
    }
    if (AreEqual(exprᴛ1, elliptic.P384())) {
        return (oidNamedCurveP384, true);
    }
    if (AreEqual(exprᴛ1, elliptic.P521())) {
        return (oidNamedCurveP521, true);
    }

    return (default!, false);
}

internal static (asn1.ObjectIdentifier, bool) oidFromECDHCurve(ecdhꓸCurve curve) {
    var exprᴛ1 = curve;
    if (AreEqual(exprᴛ1, ecdh.X25519())) {
        return (oidPublicKeyX25519, true);
    }
    if (AreEqual(exprᴛ1, ecdh.P256())) {
        return (oidNamedCurveP256, true);
    }
    if (AreEqual(exprᴛ1, ecdh.P384())) {
        return (oidNamedCurveP384, true);
    }
    if (AreEqual(exprᴛ1, ecdh.P521())) {
        return (oidNamedCurveP521, true);
    }

    return (default!, false);
}

[GoType("num:nint")] partial struct KeyUsage;

public static KeyUsage KeyUsageDigitalSignature => /* 1 << iota */ 1;
public static KeyUsage KeyUsageContentCommitment => 2;
public static KeyUsage KeyUsageKeyEncipherment => 4;
public static KeyUsage KeyUsageDataEncipherment => 8;
public static KeyUsage KeyUsageKeyAgreement => 16;
public static KeyUsage KeyUsageCertSign => 32;
public static KeyUsage KeyUsageCRLSign => 64;
public static KeyUsage KeyUsageEncipherOnly => 128;
public static KeyUsage KeyUsageDecipherOnly => 256;

// RFC 5280, 4.2.1.12  Extended Key Usage
//
//	anyExtendedKeyUsage OBJECT IDENTIFIER ::= { id-ce-extKeyUsage 0 }
//
//	id-kp OBJECT IDENTIFIER ::= { id-pkix 3 }
//
//	id-kp-serverAuth             OBJECT IDENTIFIER ::= { id-kp 1 }
//	id-kp-clientAuth             OBJECT IDENTIFIER ::= { id-kp 2 }
//	id-kp-codeSigning            OBJECT IDENTIFIER ::= { id-kp 3 }
//	id-kp-emailProtection        OBJECT IDENTIFIER ::= { id-kp 4 }
//	id-kp-timeStamping           OBJECT IDENTIFIER ::= { id-kp 8 }
//	id-kp-OCSPSigning            OBJECT IDENTIFIER ::= { id-kp 9 }
internal static asn1.ObjectIdentifier oidExtKeyUsageAny = new asn1.ObjectIdentifier(new nint[]{2, 5, 29, 37, 0}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageServerAuth = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 5, 5, 7, 3, 1}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageClientAuth = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 5, 5, 7, 3, 2}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageCodeSigning = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 5, 5, 7, 3, 3}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageEmailProtection = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 5, 5, 7, 3, 4}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageIPSECEndSystem = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 5, 5, 7, 3, 5}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageIPSECTunnel = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 5, 5, 7, 3, 6}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageIPSECUser = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 5, 5, 7, 3, 7}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageTimeStamping = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 5, 5, 7, 3, 8}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageOCSPSigning = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 5, 5, 7, 3, 9}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftServerGatedCrypto = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 4, 1, 311, 10, 3, 3}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageNetscapeServerGatedCrypto = new asn1.ObjectIdentifier(new nint[]{2, 16, 840, 1, 113730, 4, 1}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftCommercialCodeSigning = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 4, 1, 311, 2, 1, 22}.slice());

internal static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftKernelCodeSigning = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 4, 1, 311, 61, 1, 1}.slice());

[GoType("num:nint")] partial struct ExtKeyUsage;

public static ExtKeyUsage ExtKeyUsageAny => /* iota */ 0;
public static ExtKeyUsage ExtKeyUsageServerAuth => 1;
public static ExtKeyUsage ExtKeyUsageClientAuth => 2;
public static ExtKeyUsage ExtKeyUsageCodeSigning => 3;
public static ExtKeyUsage ExtKeyUsageEmailProtection => 4;
public static ExtKeyUsage ExtKeyUsageIPSECEndSystem => 5;
public static ExtKeyUsage ExtKeyUsageIPSECTunnel => 6;
public static ExtKeyUsage ExtKeyUsageIPSECUser => 7;
public static ExtKeyUsage ExtKeyUsageTimeStamping => 8;
public static ExtKeyUsage ExtKeyUsageOCSPSigning => 9;
public static ExtKeyUsage ExtKeyUsageMicrosoftServerGatedCrypto => 10;
public static ExtKeyUsage ExtKeyUsageNetscapeServerGatedCrypto => 11;
public static ExtKeyUsage ExtKeyUsageMicrosoftCommercialCodeSigning => 12;
public static ExtKeyUsage ExtKeyUsageMicrosoftKernelCodeSigning => 13;

// extKeyUsageOIDs contains the mapping between an ExtKeyUsage and its OID.

[GoType("dyn")] partial struct extKeyUsageOIDsᴛ1 {
    internal ExtKeyUsage extKeyUsage;
    internal asn1.ObjectIdentifier oid;
}
internal static slice<extKeyUsageOIDsᴛ1> extKeyUsageOIDs = new extKeyUsageOIDsᴛ1[]{
    new(ExtKeyUsageAny, oidExtKeyUsageAny),
    new(ExtKeyUsageServerAuth, oidExtKeyUsageServerAuth),
    new(ExtKeyUsageClientAuth, oidExtKeyUsageClientAuth),
    new(ExtKeyUsageCodeSigning, oidExtKeyUsageCodeSigning),
    new(ExtKeyUsageEmailProtection, oidExtKeyUsageEmailProtection),
    new(ExtKeyUsageIPSECEndSystem, oidExtKeyUsageIPSECEndSystem),
    new(ExtKeyUsageIPSECTunnel, oidExtKeyUsageIPSECTunnel),
    new(ExtKeyUsageIPSECUser, oidExtKeyUsageIPSECUser),
    new(ExtKeyUsageTimeStamping, oidExtKeyUsageTimeStamping),
    new(ExtKeyUsageOCSPSigning, oidExtKeyUsageOCSPSigning),
    new(ExtKeyUsageMicrosoftServerGatedCrypto, oidExtKeyUsageMicrosoftServerGatedCrypto),
    new(ExtKeyUsageNetscapeServerGatedCrypto, oidExtKeyUsageNetscapeServerGatedCrypto),
    new(ExtKeyUsageMicrosoftCommercialCodeSigning, oidExtKeyUsageMicrosoftCommercialCodeSigning),
    new(ExtKeyUsageMicrosoftKernelCodeSigning, oidExtKeyUsageMicrosoftKernelCodeSigning)
}.slice();

internal static (ExtKeyUsage eku, bool ok) extKeyUsageFromOID(asn1.ObjectIdentifier oid) {
    ExtKeyUsage eku = default!;
    bool ok = default!;

    foreach (var (_, pair) in extKeyUsageOIDs) {
        if (oid.Equal(pair.oid)) {
            return (pair.extKeyUsage, true);
        }
    }
    return (eku, ok);
}

internal static (asn1.ObjectIdentifier oid, bool ok) oidFromExtKeyUsage(ExtKeyUsage eku) {
    asn1.ObjectIdentifier oid = default!;
    bool ok = default!;

    foreach (var (_, pair) in extKeyUsageOIDs) {
        if (eku == pair.extKeyUsage) {
            return (pair.oid, true);
        }
    }
    return (oid, ok);
}

// A Certificate represents an X.509 certificate.
[GoType] partial struct Certificate {
    public slice<byte> Raw; // Complete ASN.1 DER content (certificate, signature algorithm and signature).
    public slice<byte> RawTBSCertificate; // Certificate part of raw ASN.1 DER content.
    public slice<byte> RawSubjectPublicKeyInfo; // DER encoded SubjectPublicKeyInfo.
    public slice<byte> RawSubject; // DER encoded Subject
    public slice<byte> RawIssuer; // DER encoded Issuer
    public slice<byte> Signature;
    public SignatureAlgorithm SignatureAlgorithm;
    public PublicKeyAlgorithm PublicKeyAlgorithm;
    public any PublicKey;
    public nint Version;
    public ж<bigꓸInt> SerialNumber;
    public pkix.Name Issuer;
    public pkix.Name Subject;
    public time.Time NotBefore, NotAfter; // Validity bounds.
    public KeyUsage KeyUsage;
    // Extensions contains raw X.509 extensions. When parsing certificates,
    // this can be used to extract non-critical extensions that are not
    // parsed by this package. When marshaling certificates, the Extensions
    // field is ignored, see ExtraExtensions.
    public slice<pkix.Extension> Extensions;
    // ExtraExtensions contains extensions to be copied, raw, into any
    // marshaled certificates. Values override any extensions that would
    // otherwise be produced based on the other fields. The ExtraExtensions
    // field is not populated when parsing certificates, see Extensions.
    public slice<pkix.Extension> ExtraExtensions;
    // UnhandledCriticalExtensions contains a list of extension IDs that
    // were not (fully) processed when parsing. Verify will fail if this
    // slice is non-empty, unless verification is delegated to an OS
    // library which understands all the critical extensions.
    //
    // Users can access these extensions using Extensions and can remove
    // elements from this slice if they believe that they have been
    // handled.
    public slice<asn1.ObjectIdentifier> UnhandledCriticalExtensions;
    public slice<ExtKeyUsage> ExtKeyUsage;      // Sequence of extended key usages.
    public slice<asn1.ObjectIdentifier> UnknownExtKeyUsage; // Encountered extended key usages unknown to this package.
    // BasicConstraintsValid indicates whether IsCA, MaxPathLen,
    // and MaxPathLenZero are valid.
    public bool BasicConstraintsValid;
    public bool IsCA;
    // MaxPathLen and MaxPathLenZero indicate the presence and
    // value of the BasicConstraints' "pathLenConstraint".
    //
    // When parsing a certificate, a positive non-zero MaxPathLen
    // means that the field was specified, -1 means it was unset,
    // and MaxPathLenZero being true mean that the field was
    // explicitly set to zero. The case of MaxPathLen==0 with MaxPathLenZero==false
    // should be treated equivalent to -1 (unset).
    //
    // When generating a certificate, an unset pathLenConstraint
    // can be requested with either MaxPathLen == -1 or using the
    // zero value for both MaxPathLen and MaxPathLenZero.
    public nint MaxPathLen;
    // MaxPathLenZero indicates that BasicConstraintsValid==true
    // and MaxPathLen==0 should be interpreted as an actual
    // maximum path length of zero. Otherwise, that combination is
    // interpreted as MaxPathLen not being set.
    public bool MaxPathLenZero;
    public slice<byte> SubjectKeyId;
    public slice<byte> AuthorityKeyId;
    // RFC 5280, 4.2.2.1 (Authority Information Access)
    public slice<@string> OCSPServer;
    public slice<@string> IssuingCertificateURL;
    // Subject Alternate Name values. (Note that these values may not be valid
    // if invalid values were contained within a parsed certificate. For
    // example, an element of DNSNames may not be a valid DNS domain name.)
    public slice<@string> DNSNames;
    public slice<@string> EmailAddresses;
    public slice<net.IP> IPAddresses;
    public slice<ж<url.URL>> URIs;
    // Name constraints
    public bool PermittedDNSDomainsCritical; // if true then the name constraints are marked critical.
    public slice<@string> PermittedDNSDomains;
    public slice<@string> ExcludedDNSDomains;
    public slice<ж<net.IPNet>> PermittedIPRanges;
    public slice<ж<net.IPNet>> ExcludedIPRanges;
    public slice<@string> PermittedEmailAddresses;
    public slice<@string> ExcludedEmailAddresses;
    public slice<@string> PermittedURIDomains;
    public slice<@string> ExcludedURIDomains;
    // CRL Distribution Points
    public slice<@string> CRLDistributionPoints;
    // PolicyIdentifiers contains asn1.ObjectIdentifiers, the components
    // of which are limited to int32. If a certificate contains a policy which
    // cannot be represented by asn1.ObjectIdentifier, it will not be included in
    // PolicyIdentifiers, but will be present in Policies, which contains all parsed
    // policy OIDs.
    // See CreateCertificate for context about how this field and the Policies field
    // interact.
    public slice<asn1.ObjectIdentifier> PolicyIdentifiers;
    // Policies contains all policy identifiers included in the certificate.
    // See CreateCertificate for context about how this field and the PolicyIdentifiers field
    // interact.
    // In Go 1.22, encoding/gob cannot handle and ignores this field.
    public slice<OID> Policies;
    // InhibitAnyPolicy and InhibitAnyPolicyZero indicate the presence and value
    // of the inhibitAnyPolicy extension.
    //
    // The value of InhibitAnyPolicy indicates the number of additional
    // certificates in the path after this certificate that may use the
    // anyPolicy policy OID to indicate a match with any other policy.
    //
    // When parsing a certificate, a positive non-zero InhibitAnyPolicy means
    // that the field was specified, -1 means it was unset, and
    // InhibitAnyPolicyZero being true mean that the field was explicitly set to
    // zero. The case of InhibitAnyPolicy==0 with InhibitAnyPolicyZero==false
    // should be treated equivalent to -1 (unset).
    public nint InhibitAnyPolicy;
    // InhibitAnyPolicyZero indicates that InhibitAnyPolicy==0 should be
    // interpreted as an actual maximum path length of zero. Otherwise, that
    // combination is interpreted as InhibitAnyPolicy not being set.
    public bool InhibitAnyPolicyZero;
    // InhibitPolicyMapping and InhibitPolicyMappingZero indicate the presence
    // and value of the inhibitPolicyMapping field of the policyConstraints
    // extension.
    //
    // The value of InhibitPolicyMapping indicates the number of additional
    // certificates in the path after this certificate that may use policy
    // mapping.
    //
    // When parsing a certificate, a positive non-zero InhibitPolicyMapping
    // means that the field was specified, -1 means it was unset, and
    // InhibitPolicyMappingZero being true mean that the field was explicitly
    // set to zero. The case of InhibitPolicyMapping==0 with
    // InhibitPolicyMappingZero==false should be treated equivalent to -1
    // (unset).
    public nint InhibitPolicyMapping;
    // InhibitPolicyMappingZero indicates that InhibitPolicyMapping==0 should be
    // interpreted as an actual maximum path length of zero. Otherwise, that
    // combination is interpreted as InhibitAnyPolicy not being set.
    public bool InhibitPolicyMappingZero;
    // RequireExplicitPolicy and RequireExplicitPolicyZero indicate the presence
    // and value of the requireExplicitPolicy field of the policyConstraints
    // extension.
    //
    // The value of RequireExplicitPolicy indicates the number of additional
    // certificates in the path after this certificate before an explicit policy
    // is required for the rest of the path. When an explicit policy is required,
    // each subsequent certificate in the path must contain a required policy OID,
    // or a policy OID which has been declared as equivalent through the policy
    // mapping extension.
    //
    // When parsing a certificate, a positive non-zero RequireExplicitPolicy
    // means that the field was specified, -1 means it was unset, and
    // RequireExplicitPolicyZero being true mean that the field was explicitly
    // set to zero. The case of RequireExplicitPolicy==0 with
    // RequireExplicitPolicyZero==false should be treated equivalent to -1
    // (unset).
    public nint RequireExplicitPolicy;
    // RequireExplicitPolicyZero indicates that RequireExplicitPolicy==0 should be
    // interpreted as an actual maximum path length of zero. Otherwise, that
    // combination is interpreted as InhibitAnyPolicy not being set.
    public bool RequireExplicitPolicyZero;
    // PolicyMappings contains a list of policy mappings included in the certificate.
    public slice<PolicyMapping> PolicyMappings;
}

// PolicyMapping represents a policy mapping entry in the policyMappings extension.
[GoType] partial struct PolicyMapping {
    // IssuerDomainPolicy contains a policy OID the issuing certificate considers
    // equivalent to SubjectDomainPolicy in the subject certificate.
    public OID IssuerDomainPolicy;
    // SubjectDomainPolicy contains a OID the issuing certificate considers
    // equivalent to IssuerDomainPolicy in the subject certificate.
    public OID SubjectDomainPolicy;
}

// ErrUnsupportedAlgorithm results from attempting to perform an operation that
// involves algorithms that are not currently implemented.
public static error ErrUnsupportedAlgorithm = errors.New("x509: cannot verify signature: algorithm unimplemented"u8);

[GoType("num:nint")] partial struct InsecureAlgorithmError;

public static @string Error(this InsecureAlgorithmError e) {
    return fmt.Sprintf("x509: cannot verify signature: insecure algorithm %v"u8, ((SignatureAlgorithm)(nint)e));
}

// ConstraintViolationError results when a requested usage is not permitted by
// a certificate. For example: checking a signature when the public key isn't a
// certificate signing key.
[GoType] partial struct ConstraintViolationError {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InvalidSignatureˢ = "x509: invalid signature: parent certificate cannot sign this kind of certificate"u8;

public static @string Error(this ConstraintViolationError _) {
    return x509InvalidSignatureˢ;
}

public static bool Equal(this ж<Certificate> Ꮡc, ж<Certificate> Ꮡother) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var other = ref Ꮡother.DerefOrNull();

    if (Ꮡc == nil || Ꮡother == nil) {
        return Ꮡc == Ꮡother;
    }
    return bytes.Equal(c.Raw, other.Raw);
}

[GoRecv] internal static bool hasSANExtension(this ref Certificate c) {
    return oidInExtensions(oidExtensionSubjectAltName, c.Extensions);
}

// CheckSignatureFrom verifies that the signature on c is a valid signature from parent.
//
// This is a low-level API that performs very limited checks, and not a full
// path verifier. Most users should use [Certificate.Verify] instead.
[GoRecv] public static error CheckSignatureFrom(this ref Certificate c, ж<Certificate> Ꮡparent) {
    ref var parent = ref Ꮡparent.DerefOrNull();

    // RFC 5280, 4.2.1.9:
    // "If the basic constraints extension is not present in a version 3
    // certificate, or the extension is present but the cA boolean is not
    // asserted, then the certified public key MUST NOT be used to verify
    // certificate signatures."
    if (parent.Version == 3 && !parent.BasicConstraintsValid || parent.BasicConstraintsValid && !parent.IsCA) {
        return new ConstraintViolationError(nil);
    }
    if (parent.KeyUsage != 0 && (KeyUsage)(parent.KeyUsage & KeyUsageCertSign) == 0) {
        return new ConstraintViolationError(nil);
    }
    if (parent.PublicKeyAlgorithm == UnknownPublicKeyAlgorithm) {
        return ErrUnsupportedAlgorithm;
    }
    return checkSignature(c.SignatureAlgorithm, c.RawTBSCertificate, c.Signature, parent.PublicKey, false);
}

// CheckSignature verifies that signature is a valid signature over signed from
// c's public key.
//
// This is a low-level API that performs no validity checks on the certificate.
//
// [MD5WithRSA] signatures are rejected, while [SHA1WithRSA] and [ECDSAWithSHA1]
// signatures are currently accepted.
[GoRecv] public static error CheckSignature(this ref Certificate c, SignatureAlgorithm algo, slice<byte> signed, slice<byte> signature) {
    return checkSignature(algo, signed, signature, c.PublicKey, true);
}

[GoRecv] internal static bool hasNameConstraints(this ref Certificate c) {
    return oidInExtensions(oidExtensionNameConstraints, c.Extensions);
}

[GoRecv] internal static slice<byte> getSANExtension(this ref Certificate c) {
    foreach (var (_, e) in c.Extensions) {
        if (e.Id.Equal(oidExtensionSubjectAltName)) {
            return e.Value;
        }
    }
    return default!;
}

internal static error signaturePublicKeyAlgoMismatchError(PublicKeyAlgorithm expectedPubKeyAlgo, any pubKey) {
    return fmt.Errorf("x509: signature algorithm specifies an %s public key, but have public key of type %T"u8, expectedPubKeyAlgo.String(), pubKey);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509EcdsaVerificationˢ = "x509: ECDSA verification failure"u8;
internal static readonly @string x509Ed25519Verificationˢ = "x509: Ed25519 verification failure"u8;

// checkSignature verifies that signature is a valid signature over signed from
// a crypto.PublicKey.
internal static error /*err*/ checkSignature(SignatureAlgorithm algo, slice<byte> signed, slice<byte> signature, cryptoꓸPublicKey publicKey, bool allowSHA1) {
    error err = default!;

    crypto.Hash hashType = default!;
    PublicKeyAlgorithm pubKeyAlgo = default!;
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == algo) {
            hashType = details.hash;
            pubKeyAlgo = details.pubKeyAlgo;
            break;
        }
    }
    var exprᴛ1 = hashType;
    var matchᴛ1 = false;
    if (exprᴛ1 == ((crypto.Hash)0)) { matchᴛ1 = true;
        if (pubKeyAlgo != Ed25519) {
            return ErrUnsupportedAlgorithm;
        }
    }
    else if (exprᴛ1 == crypto.MD5) { matchᴛ1 = true;
        return ((InsecureAlgorithmError)(nint)algo);
    }
    else if (exprᴛ1 == crypto.SHA1) { matchᴛ1 = true;
        if (!allowSHA1) {
            // SHA-1 signatures are only allowed for CRLs and CSRs.
            return ((InsecureAlgorithmError)(nint)algo);
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        if (!hashType.Available()) {
            return ErrUnsupportedAlgorithm;
        }
        var h = hashType.New();
        h.Write(signed);
        signed = h.Sum(default!);
    }

    switch (publicKey.type()) {
    case ж<rsa.PublicKey> pub: {
        if (pubKeyAlgo != RSA) {
            return signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub.OrTypedNil());
        }
        if (algo.isRSAPSS()){
            return rsa.VerifyPSS(pub, hashType, signed, signature, Ꮡ(new rsa.PSSOptions(SaltLength: rsa.PSSSaltLengthEqualsHash)));
        } else {
            return rsa.VerifyPKCS1v15(pub, hashType, signed, signature);
        }
        break;
    }
    case ж<ecdsa.PublicKey> pub: {
        if (pubKeyAlgo != ECDSA) {
            return signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub.OrTypedNil());
        }
        if (!ecdsa.VerifyASN1(pub, signed, signature)) {
            return errors.New(x509EcdsaVerificationˢ);
        }
        return err;
    }
    case ed25519.PublicKey pub: {
        if (pubKeyAlgo != Ed25519) {
            return signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub);
        }
        if (!ed25519.Verify(pub, signed, signature)) {
            return errors.New(x509Ed25519Verificationˢ);
        }
        return err;
    }}
    return ErrUnsupportedAlgorithm;
}

// CheckCRLSignature checks that the signature in crl is from c.
//
// Deprecated: Use [RevocationList.CheckSignatureFrom] instead.
[GoRecv] public static error CheckCRLSignature(this ref Certificate c, ж<pkix.CertificateList> Ꮡcrl) {
    ref var crl = ref Ꮡcrl.DerefOrNull();

    SignatureAlgorithm algo = getSignatureAlgorithmFromAI(crl.SignatureAlgorithm);
    return c.CheckSignature(algo, crl.TBSCertList.Raw, crl.SignatureValue.RightAlign());
}

[GoType] partial struct UnhandledCriticalExtension {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509UnhandledCriticalˢ = "x509: unhandled critical extension"u8;

public static @string Error(this UnhandledCriticalExtension h) {
    return x509UnhandledCriticalˢ;
}

[GoType] partial struct basicConstraints {
    [GoTag(@"asn1:""optional""")]
    public bool IsCA;
    [GoTag(@"asn1:""optional,default:-1""")]
    public nint MaxPathLen;
}

// RFC 5280 4.2.1.4
[GoType] partial struct policyInformation {
    public asn1.ObjectIdentifier Policy;
}

// policyQualifiers omitted
internal static UntypedInt nameTypeEmail => 1;
internal static UntypedInt nameTypeDNS => 2;
internal static UntypedInt nameTypeURI => 6;
internal static UntypedInt nameTypeIP => 7;

// RFC 5280, 4.2.2.1
[GoType] partial struct authorityInfoAccess {
    public asn1.ObjectIdentifier Method;
    public asn1.RawValue Location;
}

// RFC 5280, 4.2.1.14
[GoType] partial struct distributionPoint {
    [GoTag(@"asn1:""optional,tag:0""")]
    public distributionPointName DistributionPoint;
    [GoTag(@"asn1:""optional,tag:1""")]
    public asn1.BitString Reason;
    [GoTag(@"asn1:""optional,tag:2""")]
    public asn1.RawValue CRLIssuer;
}

[GoType] public partial struct distributionPointName {
    [GoTag(@"asn1:""optional,tag:0""")]
    public slice<asn1.RawValue> FullName;
    [GoTag(@"asn1:""optional,tag:1""")]
    public pkix.RDNSequence RelativeName;
}

internal static byte reverseBitsInAByte(byte @in) {
    var b1 = (byte)((@in >> (int)(4)) | (byte)(@in << (int)(4)));
    var b2 = (byte)((byte)((b1 >> (int)(2)) & 0x33) | (byte)((byte)(b1 << (int)(2)) & 0xcc));
    var b3 = (byte)((byte)((b2 >> (int)(1)) & 0x55) | (byte)((byte)(b2 << (int)(1)) & 0xaa));
    return b3;
}

// asn1BitLength returns the bit-length of bitString by considering the
// most-significant bit in a byte to be the "first" bit. This convention
// matches ASN.1, but differs from almost everything else.
internal static nint asn1BitLength(slice<byte> bitString) {
    nint bitLen = builtin.len(bitString) * 8;
    foreach (var (i, _) in bitString) {
        var b = bitString[builtin.len(bitString) - i - 1];
        for (nuint bit = (nuint)0; bit < 8; bit++) {
            if ((byte)((b.Rsh(bit)) & 1) == 1) {
                return bitLen;
            }
            bitLen--;
        }
    }
    return 0;
}

internal static slice<nint> oidExtensionSubjectKeyId = new nint[]{2, 5, 29, 14}.slice();
internal static slice<nint> oidExtensionKeyUsage = new nint[]{2, 5, 29, 15}.slice();
internal static slice<nint> oidExtensionExtendedKeyUsage = new nint[]{2, 5, 29, 37}.slice();
internal static slice<nint> oidExtensionAuthorityKeyId = new nint[]{2, 5, 29, 35}.slice();
internal static slice<nint> oidExtensionBasicConstraints = new nint[]{2, 5, 29, 19}.slice();
internal static slice<nint> oidExtensionSubjectAltName = new nint[]{2, 5, 29, 17}.slice();
internal static slice<nint> oidExtensionCertificatePolicies = new nint[]{2, 5, 29, 32}.slice();
internal static slice<nint> oidExtensionNameConstraints = new nint[]{2, 5, 29, 30}.slice();
internal static slice<nint> oidExtensionCRLDistributionPoints = new nint[]{2, 5, 29, 31}.slice();
internal static slice<nint> oidExtensionAuthorityInfoAccess = new nint[]{1, 3, 6, 1, 5, 5, 7, 1, 1}.slice();
internal static slice<nint> oidExtensionCRLNumber = new nint[]{2, 5, 29, 20}.slice();
internal static slice<nint> oidExtensionReasonCode = new nint[]{2, 5, 29, 21}.slice();

internal static asn1.ObjectIdentifier oidAuthorityInfoAccessOcsp = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 5, 5, 7, 48, 1}.slice());
internal static asn1.ObjectIdentifier oidAuthorityInfoAccessIssuers = new asn1.ObjectIdentifier(new nint[]{1, 3, 6, 1, 5, 5, 7, 48, 2}.slice());

// oidInExtensions reports whether an extension with the given oid exists in
// extensions.
internal static bool oidInExtensions(asn1.ObjectIdentifier oid, slice<pkix.Extension> extensions) {
    foreach (var (_, e) in extensions) {
        if (e.Id.Equal(oid)) {
            return true;
        }
    }
    return false;
}

// marshalSANs marshals a list of addresses into a the contents of an X.509
// SubjectAlternativeName extension.
internal static (slice<byte> derBytes, error err) marshalSANs(slice<@string> dnsNames, slice<@string> emailAddresses, slice<net.IP> ipAddresses, slice<ж<url.URL>> uris) {
    slice<asn1.RawValue> rawValues = default!;
    foreach (var (_, name) in dnsNames) {
        {
            var errΔ1 = isIA5String(name); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        rawValues = append(rawValues, new asn1.RawValue(Tag: nameTypeDNS, Class: 2, Bytes: slice<byte>(name)));
    }
    foreach (var (_, email) in emailAddresses) {
        {
            var errΔ2 = isIA5String(email); if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
        }
        rawValues = append(rawValues, new asn1.RawValue(Tag: nameTypeEmail, Class: 2, Bytes: slice<byte>(email)));
    }
    foreach (var (_, rawIP) in ipAddresses) {
        // If possible, we always want to encode IPv4 addresses in 4 bytes.
        var ip = rawIP.To4();
        if (ip == default!) {
            ip = rawIP;
        }
        rawValues = append(rawValues, new asn1.RawValue(Tag: nameTypeIP, Class: 2, Bytes: ip));
    }
    foreach (var (_, uri) in uris) {
        @string uriStr = uri.String();
        {
            var errΔ3 = isIA5String(uriStr); if (errΔ3 != default!) {
                return (default!, errΔ3);
            }
        }
        rawValues = append(rawValues, new asn1.RawValue(Tag: nameTypeURI, Class: 2, Bytes: slice<byte>(uriStr)));
    }
    return asn1.Marshal(rawValues);
}

internal static error isIA5String(@string s) {
    foreach (var (_, r) in s) {
        // Per RFC5280 "IA5String is limited to the set of ASCII characters"
        if (r > unicode.MaxASCII) {
            return fmt.Errorf("x509: %q cannot be encoded as an IA5String"u8, s);
        }
    }
    return default!;
}

internal static ж<godebug.Setting> x509usepolicies = godebug.New("x509usepolicies"u8);

internal static (slice<pkix.Extension> ret, error err) buildCertExtensions(ref Certificate template, bool subjectIsEmpty, slice<byte> authorityKeyId, slice<byte> subjectKeyId) {
    slice<pkix.Extension> ret = default!;
    error err = default!;

    ret = new slice<pkix.Extension>(10);
    /* maximum number of elements. */
    nint n = 0;
    if (template.KeyUsage != 0 && !oidInExtensions(oidExtensionKeyUsage, template.ExtraExtensions)) {
        (ret[n], err) = marshalKeyUsage(template.KeyUsage);
        if (err != default!) {
            return (default!, err);
        }
        n++;
    }
    if ((builtin.len(template.ExtKeyUsage) > 0 || builtin.len(template.UnknownExtKeyUsage) > 0) && !oidInExtensions(oidExtensionExtendedKeyUsage, template.ExtraExtensions)) {
        (ret[n], err) = marshalExtKeyUsage(template.ExtKeyUsage, template.UnknownExtKeyUsage);
        if (err != default!) {
            return (default!, err);
        }
        n++;
    }
    if (template.BasicConstraintsValid && !oidInExtensions(oidExtensionBasicConstraints, template.ExtraExtensions)) {
        (ret[n], err) = marshalBasicConstraints(template.IsCA, template.MaxPathLen, template.MaxPathLenZero);
        if (err != default!) {
            return (default!, err);
        }
        n++;
    }
    if (builtin.len(subjectKeyId) > 0 && !oidInExtensions(oidExtensionSubjectKeyId, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionSubjectKeyId;
        (ret[n].Value, err) = asn1.Marshal(subjectKeyId);
        if (err != default!) {
            return (ret, err);
        }
        n++;
    }
    if (builtin.len(authorityKeyId) > 0 && !oidInExtensions(oidExtensionAuthorityKeyId, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionAuthorityKeyId;
        (ret[n].Value, err) = asn1.Marshal(new authKeyId(authorityKeyId));
        if (err != default!) {
            return (ret, err);
        }
        n++;
    }
    if ((builtin.len(template.OCSPServer) > 0 || builtin.len(template.IssuingCertificateURL) > 0) && !oidInExtensions(oidExtensionAuthorityInfoAccess, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionAuthorityInfoAccess;
        slice<authorityInfoAccess> aiaValues = default!;
        foreach (var (_, name) in template.OCSPServer) {
            aiaValues = append(aiaValues, new authorityInfoAccess(
                Method: oidAuthorityInfoAccessOcsp,
                Location: new asn1.RawValue(Tag: 6, Class: 2, Bytes: slice<byte>(name))
            ));
        }
        foreach (var (_, name) in template.IssuingCertificateURL) {
            aiaValues = append(aiaValues, new authorityInfoAccess(
                Method: oidAuthorityInfoAccessIssuers,
                Location: new asn1.RawValue(Tag: 6, Class: 2, Bytes: slice<byte>(name))
            ));
        }
        (ret[n].Value, err) = asn1.Marshal(aiaValues);
        if (err != default!) {
            return (ret, err);
        }
        n++;
    }
    if ((builtin.len(template.DNSNames) > 0 || builtin.len(template.EmailAddresses) > 0 || builtin.len(template.IPAddresses) > 0 || builtin.len(template.URIs) > 0) && !oidInExtensions(oidExtensionSubjectAltName, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionSubjectAltName;
        // From RFC 5280, Section 4.2.1.6:
        // “If the subject field contains an empty sequence ... then
        // subjectAltName extension ... is marked as critical”
        ret[n].Critical = subjectIsEmpty;
        (ret[n].Value, err) = marshalSANs(template.DNSNames, template.EmailAddresses, template.IPAddresses, template.URIs);
        if (err != default!) {
            return (ret, err);
        }
        n++;
    }
    var usePolicies = x509usepolicies.Value() != "0"u8;
    if (((!usePolicies && builtin.len(template.PolicyIdentifiers) > 0) || (usePolicies && builtin.len(template.Policies) > 0)) && !oidInExtensions(oidExtensionCertificatePolicies, template.ExtraExtensions)) {
        (ret[n], err) = marshalCertificatePolicies(template.Policies, template.PolicyIdentifiers);
        if (err != default!) {
            return (default!, err);
        }
        n++;
    }
    if ((builtin.len(template.PermittedDNSDomains) > 0 || builtin.len(template.ExcludedDNSDomains) > 0 || builtin.len(template.PermittedIPRanges) > 0 || builtin.len(template.ExcludedIPRanges) > 0 || builtin.len(template.PermittedEmailAddresses) > 0 || builtin.len(template.ExcludedEmailAddresses) > 0 || builtin.len(template.PermittedURIDomains) > 0 || builtin.len(template.ExcludedURIDomains) > 0) && !oidInExtensions(oidExtensionNameConstraints, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionNameConstraints;
        ret[n].Critical = template.PermittedDNSDomainsCritical;
        slice<byte> ipAndMask(ж<net.IPNet> ipNet) {
            var maskedIP = (~ipNet).IP.Mask((~ipNet).Mask);
            var ipAndMaskΔ1 = new slice<byte>(0, builtin.len(maskedIP) + builtin.len((~ipNet).Mask));
            ipAndMaskΔ1 = appendꓸꓸꓸ(ipAndMaskΔ1, maskedIP);
            ipAndMaskΔ1 = appendꓸꓸꓸ(ipAndMaskΔ1, (~ipNet).Mask);
            return ipAndMaskΔ1;
        }
        var ipAndMaskʗ1 = ipAndMask;
        (slice<byte> der, error err) serialiseConstraints(slice<@string> dns, slice<ж<net.IPNet>> ips, slice<@string> emails, slice<@string> uriDomains) {
            error errΔ1 = default!;
            ref var bΔ1 = ref heap(new cryptobyte.Builder(), out var ᏑbΔ1);
            foreach (var (_, name) in dns) {
                {
                    errΔ1 = isIA5String(name); if (errΔ1 != default!) {
                        return (default!, errΔ1);
                    }
                }
                ᏑbΔ1.AddASN1(cryptobyte_asn1.SEQUENCE, (ж<cryptobyte.Builder> bΔ2) => {
                    bΔ2.AddASN1(((cryptobyte_asn1.Tag)2).ContextSpecific(), (ж<cryptobyte.Builder> bΔ3) => {
                        bΔ3.AddBytes(slice<byte>(name));
                    });
                });
            }
            foreach (var (_, ipNet) in ips) {
                var ipAndMaskʗ2 = ipAndMaskʗ1;
                var ipNetʗ1 = ipNet;
                ᏑbΔ1.AddASN1(cryptobyte_asn1.SEQUENCE, (ж<cryptobyte.Builder> bΔ4) => {
                    var ipAndMaskʗ3 = ipAndMaskʗ2;
                    var ipNetʗ2 = ipNetʗ1;
                    bΔ4.AddASN1(((cryptobyte_asn1.Tag)7).ContextSpecific(), (ж<cryptobyte.Builder> bΔ5) => {
                        bΔ5.AddBytes(ipAndMaskʗ3(ipNetʗ2));
                    });
                });
            }
            foreach (var (_, email) in emails) {
                {
                    errΔ1 = isIA5String(email); if (errΔ1 != default!) {
                        return (default!, errΔ1);
                    }
                }
                ᏑbΔ1.AddASN1(cryptobyte_asn1.SEQUENCE, (ж<cryptobyte.Builder> bΔ6) => {
                    bΔ6.AddASN1(((cryptobyte_asn1.Tag)1).ContextSpecific(), (ж<cryptobyte.Builder> bΔ7) => {
                        bΔ7.AddBytes(slice<byte>(email));
                    });
                });
            }
            foreach (var (_, uriDomain) in uriDomains) {
                {
                    errΔ1 = isIA5String(uriDomain); if (errΔ1 != default!) {
                        return (default!, errΔ1);
                    }
                }
                ᏑbΔ1.AddASN1(cryptobyte_asn1.SEQUENCE, (ж<cryptobyte.Builder> bΔ8) => {
                    bΔ8.AddASN1(((cryptobyte_asn1.Tag)6).ContextSpecific(), (ж<cryptobyte.Builder> bΔ9) => {
                        bΔ9.AddBytes(slice<byte>(uriDomain));
                    });
                });
            }
            return bΔ1.Bytes();
        }
        var (permitted, errΔ2) = serialiseConstraints(template.PermittedDNSDomains, template.PermittedIPRanges, template.PermittedEmailAddresses, template.PermittedURIDomains);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        (var excluded, errΔ2) = serialiseConstraints(template.ExcludedDNSDomains, template.ExcludedIPRanges, template.ExcludedEmailAddresses, template.ExcludedURIDomains);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        ref var b = ref heap(new cryptobyte.Builder(), out var Ꮡb);
        var excludedʗ1 = excluded;
        var permittedʗ1 = permitted;
        Ꮡb.AddASN1(cryptobyte_asn1.SEQUENCE, (ж<cryptobyte.Builder> bΔ1) => {
            if (builtin.len(permittedʗ1) > 0) {
                var permittedʗ2 = permittedʗ1;
                bΔ1.AddASN1(((cryptobyte_asn1.Tag)0).ContextSpecific().Constructed(), (ж<cryptobyte.Builder> bΔ2) => {
                    bΔ2.AddBytes(permittedʗ2);
                });
            }
            if (builtin.len(excludedʗ1) > 0) {
                var excludedʗ2 = excludedʗ1;
                bΔ1.AddASN1(((cryptobyte_asn1.Tag)1).ContextSpecific().Constructed(), (ж<cryptobyte.Builder> bΔ3) => {
                    bΔ3.AddBytes(excludedʗ2);
                });
            }
        });
        (ret[n].Value, errΔ2) = b.Bytes();
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        n++;
    }
    if (builtin.len(template.CRLDistributionPoints) > 0 && !oidInExtensions(oidExtensionCRLDistributionPoints, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionCRLDistributionPoints;
        slice<distributionPoint> crlDp = default!;
        foreach (var (_, name) in template.CRLDistributionPoints) {
            var dp = new distributionPoint(
                DistributionPoint: new distributionPointName(
                    FullName: new asn1.RawValue[]{
                        new(Tag: 6, Class: 2, Bytes: slice<byte>(name))
                    }.slice()
                )
            );
            crlDp = append(crlDp, dp);
        }
        (ret[n].Value, err) = asn1.Marshal(crlDp);
        if (err != default!) {
            return (ret, err);
        }
        n++;
    }
    // Adding another extension here? Remember to update the maximum number
    // of elements in the make() at the top of the function and the list of
    // template fields used in CreateCertificate documentation.
    return (appendꓸꓸꓸ(ret.slice(0, n), template.ExtraExtensions), default!);
}

internal static (pkix.Extension, error) marshalKeyUsage(KeyUsage ku) {
    var ext = new pkix.Extension(Id: oidExtensionKeyUsage, Critical: true);
    array<byte> a = new(2);
    a[0] = reverseBitsInAByte((byte)(nint)ku);
    a[1] = reverseBitsInAByte((byte)(nint)((ku >> (int)(8))));
    nint l = 1;
    if (a[1] != 0) {
        l = 2;
    }
    var bitString = a.slice(0, l);
    error err = default!;
    (ext.Value, err) = asn1.Marshal(new asn1.BitString(Bytes: bitString, BitLength: asn1BitLength(bitString)));
    return (ext, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509UnknownExtendedKeyˢ = "x509: unknown extended key usage"u8;

internal static (pkix.Extension, error) marshalExtKeyUsage(slice<ExtKeyUsage> extUsages, slice<asn1.ObjectIdentifier> unknownUsages) {
    var ext = new pkix.Extension(Id: oidExtensionExtendedKeyUsage);
    var oids = new slice<asn1.ObjectIdentifier>(builtin.len(extUsages) + builtin.len(unknownUsages));
    foreach (var (i, u) in extUsages) {
        {
            var (oid, ok) = oidFromExtKeyUsage(u); if (ok){
                oids[i] = oid;
            } else {
                return (ext, errors.New(x509UnknownExtendedKeyˢ));
            }
        }
    }
    copy(oids.slice(builtin.len(extUsages)), unknownUsages);
    error err = default!;
    (ext.Value, err) = asn1.Marshal(oids);
    return (ext, err);
}

internal static (pkix.Extension, error) marshalBasicConstraints(bool isCA, nint maxPathLen, bool maxPathLenZero) {
    var ext = new pkix.Extension(Id: oidExtensionBasicConstraints, Critical: true);
    // Leaving MaxPathLen as zero indicates that no maximum path
    // length is desired, unless MaxPathLenZero is set. A value of
    // -1 causes encoding/asn1 to omit the value as desired.
    if (maxPathLen == 0 && !maxPathLenZero) {
        maxPathLen = -1;
    }
    error err = default!;
    (ext.Value, err) = asn1.Marshal(new basicConstraints(isCA, maxPathLen));
    return (ext, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidPolicyObjectˢ = "invalid policy object identifier"u8;

internal static (pkix.Extension, error) marshalCertificatePolicies(slice<OID> policies, slice<asn1.ObjectIdentifier> policyIdentifiers) {
    var ext = new pkix.Extension(Id: oidExtensionCertificatePolicies);
    var b = cryptobyte.NewBuilder(new slice<byte>(0, 128));
    var policiesʗ1 = policies;
    var policyIdentifiersʗ1 = policyIdentifiers;
    b.AddASN1(cryptobyte_asn1.SEQUENCE, (ж<cryptobyte.Builder> child) => {
        if (x509usepolicies.Value() != "0"u8){
            x509usepolicies.IncNonDefault();
            foreach (var (_, vᴛ1) in policiesʗ1) {
                ref var v = ref heap(new OID(), out var Ꮡv);
                v = vᴛ1;

                var vʗ1 = v;
                child.AddASN1(cryptobyte_asn1.SEQUENCE, (ж<cryptobyte.Builder> childΔ1) => {
                    var vʗ2 = vʗ1;
                    childΔ1.AddASN1(cryptobyte_asn1.OBJECT_IDENTIFIER, (ж<cryptobyte.Builder> childΔ2) => {
                        if (builtin.len(vʗ2.der) == 0) {
                            childΔ2.SetError(errors.New(invalidPolicyObjectˢ));
                            return;
                        }
                        childΔ2.AddBytes(vʗ2.der);
                    });
                });
            }
        } else {
            foreach (var (_, v) in policyIdentifiersʗ1) {
                var vʗ3 = v;
                child.AddASN1(cryptobyte_asn1.SEQUENCE, (ж<cryptobyte.Builder> childΔ3) => {
                    childΔ3.AddASN1ObjectIdentifier(vʗ3);
                });
            }
        }
    });
    error err = default!;
    (ext.Value, err) = b.Bytes();
    return (ext, err);
}

internal static (slice<pkix.Extension>, error) buildCSRExtensions(ref CertificateRequest template) {
    slice<pkix.Extension> ret = default!;
    if ((builtin.len(template.DNSNames) > 0 || builtin.len(template.EmailAddresses) > 0 || builtin.len(template.IPAddresses) > 0 || builtin.len(template.URIs) > 0) && !oidInExtensions(oidExtensionSubjectAltName, template.ExtraExtensions)) {
        var (sanBytes, err) = marshalSANs(template.DNSNames, template.EmailAddresses, template.IPAddresses, template.URIs);
        if (err != default!) {
            return (default!, err);
        }
        ret = append(ret, new pkix.Extension(
            Id: oidExtensionSubjectAltName,
            Value: sanBytes
        ));
    }
    return (appendꓸꓸꓸ(ret, template.ExtraExtensions), default!);
}

internal static (slice<byte>, error) subjectBytes(ref Certificate cert) {
    if (builtin.len(cert.RawSubject) > 0) {
        return (cert.RawSubject, default!);
    }
    return asn1.Marshal(cert.Subject.ToRDNSequence());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509OnlyRsaEcdsaAndˢ = "x509: only RSA, ECDSA and Ed25519 keys supported"u8;
internal static readonly @string x509Requestedˢ = "x509: requested SignatureAlgorithm does not match private key type"u8;
internal static readonly @string x509SigningWithMd5IsNotˢ = "x509: signing with MD5 is not supported"u8;
internal static readonly @string x509Unknownˢ = "x509: unknown SignatureAlgorithm"u8;

// signingParamsForKey returns the signature algorithm and its Algorithm
// Identifier to use for signing, based on the key type. If sigAlgo is not zero
// then it overrides the default.
internal static (SignatureAlgorithm, pkix.AlgorithmIdentifier, error) signingParamsForKey(crypto.Signer key, SignatureAlgorithm sigAlgo) {
    pkix.AlgorithmIdentifier ai = default!;
    PublicKeyAlgorithm pubType = default!;
    SignatureAlgorithm defaultAlgo = default!;
    var switchᴛ1 = key.Public();
    switch (switchᴛ1.type()) {
    case ж<rsa.PublicKey> pub: {
        pubType = RSA;
        defaultAlgo = SHA256WithRSA;
        break;
    }
    case ж<ecdsa.PublicKey> pub: {
        pubType = ECDSA;
        var exprᴛ1 = (~pub).Curve;
        if (AreEqual(exprᴛ1, elliptic.P224()) || AreEqual(exprᴛ1, elliptic.P256())) {
            defaultAlgo = ECDSAWithSHA256;
        }
        else if (AreEqual(exprᴛ1, elliptic.P384())) {
            defaultAlgo = ECDSAWithSHA384;
        }
        else if (AreEqual(exprᴛ1, elliptic.P521())) {
            defaultAlgo = ECDSAWithSHA512;
        }
        else { /* default: */
            return (0, ai, errors.New(x509UnsupportedEllipticˢ));
        }

        break;
    }
    case ed25519.PublicKey pub: {
        pubType = Ed25519;
        defaultAlgo = PureEd25519;
        break;
    }
    default: {
        var pub = switchᴛ1;
        return (0, ai, errors.New(x509OnlyRsaEcdsaAndˢ));
    }}
    if (sigAlgo == 0) {
        sigAlgo = defaultAlgo;
    }
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == sigAlgo) {
            if (details.pubKeyAlgo != pubType) {
                return (0, ai, errors.New(x509Requestedˢ));
            }
            if (details.hash == crypto.MD5) {
                return (0, ai, errors.New(x509SigningWithMd5IsNotˢ));
            }
            return (sigAlgo, new pkix.AlgorithmIdentifier(
                Algorithm: details.oid,
                Parameters: details.@params
            ), default!);
        }
    }
    return (0, ai, errors.New(x509Unknownˢ));
}

internal static (slice<byte>, error) signTBS(slice<byte> tbs, crypto.Signer key, SignatureAlgorithm sigAlg, io.Reader rand) {
    var signed = tbs;
    ref var hashFunc = ref heap<crypto.Hash>(out var ᏑhashFunc);
    hashFunc = sigAlg.hashFunc();
    if (hashFunc != 0) {
        var h = hashFunc.New();
        h.Write(signed);
        signed = h.Sum(default!);
    }
    crypto.SignerOpts signerOpts = hashFunc;
    if (sigAlg.isRSAPSS()) {
        signerOpts = new rsa_PSSOptionsжSignerOpts(Ꮡ(new rsa.PSSOptions(
            SaltLength: rsa.PSSSaltLengthEqualsHash,
            Hash: hashFunc
        )));
    }
    var (signature, err) = key.Sign(rand, signed, signerOpts);
    if (err != default!) {
        return (default!, err);
    }
    // Check the signature to ensure the crypto.Signer behaved correctly.
    {
        var errΔ1 = checkSignature(sigAlg, tbs, signature, key.Public(), true); if (errΔ1 != default!) {
            return (default!, fmt.Errorf("x509: signature returned by signer is invalid: %w"u8, errΔ1));
        }
    }
    return (signature, default!);
}

// emptyASN1Subject is the ASN.1 DER encoding of an empty Subject, which is
// just an empty SEQUENCE.
internal static slice<byte> emptyASN1Subject = new byte[]{0x30, 0}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509CertificatePrivateˢ = "x509: certificate private key does not implement crypto.Signer"u8;
internal static readonly @string x509SerialNumberMustBeˢ = "x509: serial number must be positive"u8;
internal static readonly @string x509OnlyCAsAreAllowedToˢ = "x509: only CAs are allowed to specify MaxPathLen"u8;
internal static readonly @string x509InternalErrorˢ = "x509: internal error: supported public key does not implement Equal"u8;
internal static readonly @string x509ProvidedPrivateKeyˢ = "x509: provided PrivateKey doesn't match parent's PublicKey"u8;

// CreateCertificate creates a new X.509 v3 certificate based on a template.
// The following members of template are currently used:
//
//   - AuthorityKeyId
//   - BasicConstraintsValid
//   - CRLDistributionPoints
//   - DNSNames
//   - EmailAddresses
//   - ExcludedDNSDomains
//   - ExcludedEmailAddresses
//   - ExcludedIPRanges
//   - ExcludedURIDomains
//   - ExtKeyUsage
//   - ExtraExtensions
//   - IPAddresses
//   - IsCA
//   - IssuingCertificateURL
//   - KeyUsage
//   - MaxPathLen
//   - MaxPathLenZero
//   - NotAfter
//   - NotBefore
//   - OCSPServer
//   - PermittedDNSDomains
//   - PermittedDNSDomainsCritical
//   - PermittedEmailAddresses
//   - PermittedIPRanges
//   - PermittedURIDomains
//   - PolicyIdentifiers (see note below)
//   - Policies (see note below)
//   - SerialNumber
//   - SignatureAlgorithm
//   - Subject
//   - SubjectKeyId
//   - URIs
//   - UnknownExtKeyUsage
//
// The certificate is signed by parent. If parent is equal to template then the
// certificate is self-signed. The parameter pub is the public key of the
// certificate to be generated and priv is the private key of the signer.
//
// The returned slice is the certificate in DER encoding.
//
// The currently supported key types are *rsa.PublicKey, *ecdsa.PublicKey and
// ed25519.PublicKey. pub must be a supported key type, and priv must be a
// crypto.Signer with a supported public key.
//
// The AuthorityKeyId will be taken from the SubjectKeyId of parent, if any,
// unless the resulting certificate is self-signed. Otherwise the value from
// template will be used.
//
// If SubjectKeyId from template is empty and the template is a CA, SubjectKeyId
// will be generated from the hash of the public key.
//
// If template.SerialNumber is nil, a serial number will be generated which
// conforms to RFC 5280, Section 4.1.2.2 using entropy from rand.
//
// The PolicyIdentifier and Policies fields can both be used to marshal certificate
// policy OIDs. By default, only the Policies is marshaled, but if the
// GODEBUG setting "x509usepolicies" has the value "0", the PolicyIdentifiers field will
// be marshaled instead of the Policies field. This changed in Go 1.24. The Policies field can
// be used to marshal policy OIDs which have components that are larger than 31
// bits.
public static (slice<byte>, error) CreateCertificate(io.Reader rand, ж<Certificate> Ꮡtemplate, ж<Certificate> Ꮡparent, any pub, any priv) {
    ref var template = ref Ꮡtemplate.DerefOrNull();
    ref var parent = ref Ꮡparent.DerefOrNull();

    var (key, ok) = priv._<crypto.Signer>(ᐧ);
    if (!ok) {
        return (default!, errors.New(x509CertificatePrivateˢ));
    }
    var serialNumber = template.SerialNumber;
    if (serialNumber == nil) {
        // Generate a serial number following RFC 5280 Section 4.1.2.2 if one is not provided.
        // Requirements:
        //   - serial number must be positive
        //   - at most 20 octets when encoded
        var maxSerial = big.NewInt(1).Lsh(big.NewInt(1), 20 * 8);
        while (ᐧ) {
            error errΔ1 = default!;
            (serialNumber, errΔ1) = cryptorand.Int(rand, maxSerial);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            // If the serial is exactly 20 octets, check if the high bit of the first byte is set.
            // If so, generate a new serial, since it will be padded with a leading 0 byte during
            // encoding so that the serial is not interpreted as a negative integer, making it
            // 21 octets.
            {
                var serialBytes = serialNumber.Bytes(); if (builtin.len(serialBytes) > 0 && (builtin.len(serialBytes) < 20 || (byte)(serialBytes[0] & 0x80) == 0)) {
                    break;
                }
            }
        }
    }
    // RFC 5280 Section 4.1.2.2: serial number must be positive
    //
    // We _should_ also restrict serials to <= 20 octets, but it turns out a lot of people
    // get this wrong, in part because the encoding can itself alter the length of the
    // serial. For now we accept these non-conformant serials.
    if (serialNumber.Sign() == -1) {
        return (default!, errors.New(x509SerialNumberMustBeˢ));
    }
    if (template.BasicConstraintsValid && !template.IsCA && template.MaxPathLen != -1 && (template.MaxPathLen != 0 || template.MaxPathLenZero)) {
        return (default!, errors.New(x509OnlyCAsAreAllowedToˢ));
    }
    var (signatureAlgorithm, algorithmIdentifier, err) = signingParamsForKey(key, template.SignatureAlgorithm);
    if (err != default!) {
        return (default!, err);
    }
    (var publicKeyBytes, var publicKeyAlgorithm, err) = marshalPublicKey(pub);
    if (err != default!) {
        return (default!, err);
    }
    if (getPublicKeyAlgorithmFromOID(publicKeyAlgorithm.Algorithm) == UnknownPublicKeyAlgorithm) {
        return (default!, fmt.Errorf("x509: unsupported public key type: %T"u8, pub));
    }
    (var asn1Issuer, err) = subjectBytes(ref (Ꮡparent).DerefOrNull());
    if (err != default!) {
        return (default!, err);
    }
    (var asn1Subject, err) = subjectBytes(ref (Ꮡtemplate).DerefOrNull());
    if (err != default!) {
        return (default!, err);
    }
    var authorityKeyId = template.AuthorityKeyId;
    if (!bytes.Equal(asn1Issuer, asn1Subject) && builtin.len(parent.SubjectKeyId) > 0) {
        authorityKeyId = parent.SubjectKeyId;
    }
    var subjectKeyId = template.SubjectKeyId;
    if (builtin.len(subjectKeyId) == 0 && template.IsCA) {
        // SubjectKeyId generated using method 1 in RFC 5280, Section 4.2.1.2:
        //   (1) The keyIdentifier is composed of the 160-bit SHA-1 hash of the
        //   value of the BIT STRING subjectPublicKey (excluding the tag,
        //   length, and number of unused bits).
        var h = sha1.Sum(publicKeyBytes);
        subjectKeyId = h[..];
    }
    {
        var (privPub, okΔ1) = key.Public()._<alreadyInChain_pubKeyEqual>(ᐧ); if (!okΔ1){
            return (default!, errors.New(x509InternalErrorˢ));
        } else 
        if (parent.PublicKey != default! && !privPub.Equal(parent.PublicKey)) {
            return (default!, errors.New(x509ProvidedPrivateKeyˢ));
        }
    }
    (var extensions, err) = buildCertExtensions(ref (Ꮡtemplate).DerefOrNull(), bytes.Equal(asn1Subject, emptyASN1Subject), authorityKeyId, subjectKeyId);
    if (err != default!) {
        return (default!, err);
    }
    var encodedPublicKey = new asn1.BitString(BitLength: builtin.len(publicKeyBytes) * 8, Bytes: publicKeyBytes);
    var c = new tbsCertificate(
        Version: 2,
        SerialNumber: serialNumber,
        SignatureAlgorithm: algorithmIdentifier,
        Issuer: new asn1.RawValue(FullBytes: asn1Issuer),
        Validity: new validity(template.NotBefore.UTC(), template.NotAfter.UTC()),
        Subject: new asn1.RawValue(FullBytes: asn1Subject),
        PublicKey: new publicKeyInfo(default!, publicKeyAlgorithm, encodedPublicKey),
        Extensions: extensions
    );
    (var tbsCertContents, err) = asn1.Marshal(c);
    if (err != default!) {
        return (default!, err);
    }
    c.Raw = tbsCertContents;
    (var signature, err) = signTBS(tbsCertContents, key, signatureAlgorithm, rand);
    if (err != default!) {
        return (default!, err);
    }
    return asn1.Marshal(new certificate(
        TBSCertificate: c,
        SignatureAlgorithm: algorithmIdentifier,
        SignatureValue: new asn1.BitString(Bytes: signature, BitLength: builtin.len(signature) * 8)
    ));
}

// pemCRLPrefix is the magic string that indicates that we have a PEM encoded
// CRL.
internal static slice<byte> pemCRLPrefix = slice<byte>("-----BEGIN X509 CRL"u8);

// pemType is the type of a PEM encoded CRL.
internal static @string pemType = "X509 CRL"u8;

// ParseCRL parses a CRL from the given bytes. It's often the case that PEM
// encoded CRLs will appear where they should be DER encoded, so this function
// will transparently handle PEM encoding as long as there isn't any leading
// garbage.
//
// Deprecated: Use [ParseRevocationList] instead.
public static (ж<pkix.CertificateList>, error) ParseCRL(slice<byte> crlBytes) {
    if (bytes.HasPrefix(crlBytes, pemCRLPrefix)) {
        var (block, _) = pem.Decode(crlBytes);
        if (block != nil && (~block).Type == pemType) {
            crlBytes = block.Value.Bytes;
        }
    }
    return ParseDERCRL(crlBytes);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509TrailingDataAfterCrlˢ = "x509: trailing data after CRL"u8;

// ParseDERCRL parses a DER encoded CRL from the given bytes.
//
// Deprecated: Use [ParseRevocationList] instead.
public static (ж<pkix.CertificateList>, error) ParseDERCRL(slice<byte> derBytes) {
    var certList = @new<pkix.CertificateList>();
    {
        var (rest, err) = asn1.Unmarshal(derBytes, certList.OrTypedNil()); if (err != default!){
            return (default!, err);
        } else 
        if (builtin.len(rest) != 0) {
            return (default!, errors.New(x509TrailingDataAfterCrlˢ));
        }
    }
    return (certList, default!);
}

// CreateCRL returns a DER encoded CRL, signed by this Certificate, that
// contains the given list of revoked certificates.
//
// Deprecated: this method does not generate an RFC 5280 conformant X.509 v2 CRL.
// To generate a standards compliant CRL, use [CreateRevocationList] instead.
[GoRecv] public static (slice<byte> crlBytes, error err) CreateCRL(this ref Certificate c, io.Reader rand, any priv, slice<pkix.RevokedCertificate> revokedCerts, time.Time now, time.Time expiry) {
    error err = default!;

    var (key, ok) = priv._<crypto.Signer>(ᐧ);
    if (!ok) {
        return (default!, errors.New(x509CertificatePrivateˢ));
    }
    (var signatureAlgorithm, var algorithmIdentifier, err) = signingParamsForKey(key, 0);
    if (err != default!) {
        return (default!, err);
    }
    // Force revocation times to UTC per RFC 5280.
    var revokedCertsUTC = new slice<pkix.RevokedCertificate>(builtin.len(revokedCerts));
    foreach (var (i, vᴛ1) in revokedCerts) {
        var rc = vᴛ1;

        rc.RevocationTime = rc.RevocationTime.UTC();
        revokedCertsUTC[i] = rc;
    }
    var tbsCertList = new pkix.TBSCertificateList(
        Version: 1,
        Signature: algorithmIdentifier,
        Issuer: c.Subject.ToRDNSequence(),
        ThisUpdate: now.UTC(),
        NextUpdate: expiry.UTC(),
        RevokedCertificates: revokedCertsUTC
    );
    // Authority Key Id
    if (builtin.len(c.SubjectKeyId) > 0) {
        pkix.Extension aki = default!;
        aki.Id = oidExtensionAuthorityKeyId;
        (aki.Value, err) = asn1.Marshal(new authKeyId(Id: c.SubjectKeyId));
        if (err != default!) {
            return (default!, err);
        }
        tbsCertList.Extensions = append(tbsCertList.Extensions, aki);
    }
    (var tbsCertListContents, err) = asn1.Marshal(tbsCertList);
    if (err != default!) {
        return (default!, err);
    }
    tbsCertList.Raw = tbsCertListContents;
    (var signature, err) = signTBS(tbsCertListContents, key, signatureAlgorithm, rand);
    if (err != default!) {
        return (default!, err);
    }
    return asn1.Marshal(new pkix.CertificateList(
        TBSCertList: tbsCertList,
        SignatureAlgorithm: algorithmIdentifier,
        SignatureValue: new asn1.BitString(Bytes: signature, BitLength: builtin.len(signature) * 8)
    ));
}

// CertificateRequest represents a PKCS #10, certificate signature request.
[GoType] partial struct CertificateRequest {
    public slice<byte> Raw; // Complete ASN.1 DER content (CSR, signature algorithm and signature).
    public slice<byte> RawTBSCertificateRequest; // Certificate request info part of raw ASN.1 DER content.
    public slice<byte> RawSubjectPublicKeyInfo; // DER encoded SubjectPublicKeyInfo.
    public slice<byte> RawSubject; // DER encoded Subject.
    public nint Version;
    public slice<byte> Signature;
    public SignatureAlgorithm SignatureAlgorithm;
    public PublicKeyAlgorithm PublicKeyAlgorithm;
    public any PublicKey;
    public pkix.Name Subject;
    // Attributes contains the CSR attributes that can parse as
    // pkix.AttributeTypeAndValueSET.
    //
    // Deprecated: Use Extensions and ExtraExtensions instead for parsing and
    // generating the requestedExtensions attribute.
    public slice<pkix.AttributeTypeAndValueSET> Attributes;
    // Extensions contains all requested extensions, in raw form. When parsing
    // CSRs, this can be used to extract extensions that are not parsed by this
    // package.
    public slice<pkix.Extension> Extensions;
    // ExtraExtensions contains extensions to be copied, raw, into any CSR
    // marshaled by CreateCertificateRequest. Values override any extensions
    // that would otherwise be produced based on the other fields but are
    // overridden by any extensions specified in Attributes.
    //
    // The ExtraExtensions field is not populated by ParseCertificateRequest,
    // see Extensions instead.
    public slice<pkix.Extension> ExtraExtensions;
    // Subject Alternate Name values.
    public slice<@string> DNSNames;
    public slice<@string> EmailAddresses;
    public slice<net.IP> IPAddresses;
    public slice<ж<url.URL>> URIs;
}

// These structures reflect the ASN.1 structure of X.509 certificate
// signature requests (see RFC 2986):
[GoType] public partial struct tbsCertificateRequest {
    public asn1.RawContent Raw;
    public nint Version;
    public asn1.RawValue Subject;
    public publicKeyInfo PublicKey;
    [GoTag(@"asn1:""tag:0""")]
    public slice<asn1.RawValue> RawAttributes;
}

[GoType] partial struct certificateRequest {
    public asn1.RawContent Raw;
    public tbsCertificateRequest TBSCSR;
    public pkix.AlgorithmIdentifier SignatureAlgorithm;
    public asn1.BitString SignatureValue;
}

// oidExtensionRequest is a PKCS #9 OBJECT IDENTIFIER that indicates requested
// extensions in a CSR.
internal static asn1.ObjectIdentifier oidExtensionRequest = new asn1.ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 9, 14}.slice());

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509FailedToUnmarshalRawˢ = "x509: failed to unmarshal raw CSR Attributes"u8;

// newRawAttributes converts AttributeTypeAndValueSETs from a template
// CertificateRequest's Attributes into tbsCertificateRequest RawAttributes.
internal static (slice<asn1.RawValue>, error) newRawAttributes(slice<pkix.AttributeTypeAndValueSET> attributes) {
    ref var rawAttributes = ref heap<slice<asn1.RawValue>>(out var ᏑrawAttributes);
    var (b, err) = asn1.Marshal(attributes);
    if (err != default!) {
        return (default!, err);
    }
    (var rest, err) = asn1.Unmarshal(b, ᏑrawAttributes);
    if (err != default!) {
        return (default!, err);
    }
    if (builtin.len(rest) != 0) {
        return (default!, errors.New(x509FailedToUnmarshalRawˢ));
    }
    return (rawAttributes, default!);
}

// parseRawAttributes Unmarshals RawAttributes into AttributeTypeAndValueSETs.
internal static slice<pkix.AttributeTypeAndValueSET> parseRawAttributes(slice<asn1.RawValue> rawAttributes) {
    slice<pkix.AttributeTypeAndValueSET> attributes = default!;
    foreach (var (_, rawAttr) in rawAttributes) {
        ref var attr = ref heap(new pkix.AttributeTypeAndValueSET(), out var Ꮡattr);
        var (rest, err) = asn1.Unmarshal(rawAttr.FullBytes, Ꮡattr);
        // Ignore attributes that don't parse into pkix.AttributeTypeAndValueSET
        // (i.e.: challengePassword or unstructuredName).
        if (err == default! && builtin.len(rest) == 0) {
            attributes = append(attributes, attr);
        }
    }
    return attributes;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509CertificateRequestˢ = "x509: certificate request contains duplicate requested extensions"u8;

// pkcs10Attribute reflects the Attribute structure from RFC 2986, Section 4.1.
[GoType("dyn")] internal partial struct parseCSRExtensions_pkcs10Attribute {
    public asn1.ObjectIdentifier Id;
    [GoTag(@"asn1:""set""")]
    public slice<asn1.RawValue> Values;
}

// parseCSRExtensions parses the attributes from a CSR and extracts any
// requested extensions.
internal static (slice<pkix.Extension>, error) parseCSRExtensions(slice<asn1.RawValue> rawAttributes) {
    slice<pkix.Extension> ret = default!;
    var requestedExts = new map<@string, bool>();
    foreach (var (_, rawAttr) in rawAttributes) {
        ref var attr = ref heap(new parseCSRExtensions_pkcs10Attribute(), out var Ꮡattr);
        {
            var (rest, err) = asn1.Unmarshal(rawAttr.FullBytes, Ꮡattr); if (err != default! || builtin.len(rest) != 0 || builtin.len(attr.Values) == 0) {
                // Ignore attributes that don't parse.
                continue;
            }
        }
        if (!attr.Id.Equal(oidExtensionRequest)) {
            continue;
        }
        ref var extensions = ref heap<slice<pkix.Extension>>(out var Ꮡextensions);
        {
            var (_, err) = asn1.Unmarshal(attr.Values[0].FullBytes, Ꮡextensions); if (err != default!) {
                return (default!, err);
            }
        }
        foreach (var (_, ext) in extensions) {
            @string oidStr = ext.Id.String();
            if (requestedExts[oidStr]) {
                return (default!, errors.New(x509CertificateRequestˢ));
            }
            requestedExts[oidStr] = true;
        }
        ret = appendꓸꓸꓸ(ret, extensions);
    }
    return (ret, default!);
}

[GoType("dyn")] internal partial struct CreateCertificateRequest_attr {
    public asn1.ObjectIdentifier Type;
    [GoTag(@"asn1:""set""")]
    public slice<slice<pkix.Extension>> Value;
}

// CreateCertificateRequest creates a new certificate request based on a
// template. The following members of template are used:
//
//   - SignatureAlgorithm
//   - Subject
//   - DNSNames
//   - EmailAddresses
//   - IPAddresses
//   - URIs
//   - ExtraExtensions
//   - Attributes (deprecated)
//
// priv is the private key to sign the CSR with, and the corresponding public
// key will be included in the CSR. It must implement crypto.Signer and its
// Public() method must return a *rsa.PublicKey or a *ecdsa.PublicKey or a
// ed25519.PublicKey. (A *rsa.PrivateKey, *ecdsa.PrivateKey or
// ed25519.PrivateKey satisfies this.)
//
// The returned slice is the certificate request in DER encoding.
public static (slice<byte> csr, error err) CreateCertificateRequest(io.Reader rand, ж<CertificateRequest> Ꮡtemplate, any priv) {
    error err = default!;

    ref var template = ref Ꮡtemplate.DerefOrNull();
    var (key, ok) = priv._<crypto.Signer>(ᐧ);
    if (!ok) {
        return (default!, errors.New(x509CertificatePrivateˢ));
    }
    (var signatureAlgorithm, var algorithmIdentifier, err) = signingParamsForKey(key, template.SignatureAlgorithm);
    if (err != default!) {
        return (default!, err);
    }
    slice<byte> publicKeyBytes = default!;
    pkix.AlgorithmIdentifier publicKeyAlgorithm = default!;
    (publicKeyBytes, publicKeyAlgorithm, err) = marshalPublicKey(key.Public());
    if (err != default!) {
        return (default!, err);
    }
    (var extensions, err) = buildCSRExtensions(ref (Ꮡtemplate).DerefOrNull());
    if (err != default!) {
        return (default!, err);
    }
    // Make a copy of template.Attributes because we may alter it below.
    var attributes = new slice<pkix.AttributeTypeAndValueSET>(0, builtin.len(template.Attributes));
    foreach (var (_, attr) in template.Attributes) {
        var values = new slice<slice<pkix.AttributeTypeAndValue>>(builtin.len(attr.Value));
        copy(values, attr.Value);
        attributes = append(attributes, new pkix.AttributeTypeAndValueSET(
            Type: attr.Type,
            Value: values
        ));
    }
    var extensionsAppended = false;
    if (builtin.len(extensions) > 0) {
        // Append the extensions to an existing attribute if possible.
        foreach (var (_, atvSet) in attributes) {
            if (!atvSet.Type.Equal(oidExtensionRequest) || builtin.len(atvSet.Value) == 0) {
                continue;
            }
            // specifiedExtensions contains all the extensions that we
            // found specified via template.Attributes.
            var specifiedExtensions = new map<@string, bool>();
            foreach (var (_, atvs) in atvSet.Value) {
                foreach (var (_, atv) in atvs) {
                    specifiedExtensions[atv.Type.String()] = true;
                }
            }
            var newValue = new slice<pkix.AttributeTypeAndValue>(0, builtin.len(atvSet.Value[0]) + builtin.len(extensions));
            newValue = appendꓸꓸꓸ(newValue, atvSet.Value[0]);
            foreach (var (_, e) in extensions) {
                if (specifiedExtensions[e.Id.String()]) {
                    // Attributes already contained a value for
                    // this extension and it takes priority.
                    continue;
                }
                newValue = append(newValue, new pkix.AttributeTypeAndValue( // There is no place for the critical
 // flag in an AttributeTypeAndValue.

                    Type: e.Id,
                    Value: e.Value
                ));
            }
            atvSet.Value[0] = newValue;
            extensionsAppended = true;
            break;
        }
    }
    (var rawAttributes, err) = newRawAttributes(attributes);
    if (err != default!) {
        return (default!, err);
    }
    // If not included in attributes, add a new attribute for the
    // extensions.
    if (builtin.len(extensions) > 0 && !extensionsAppended) {
        var attr = new CreateCertificateRequest_attr(
            Type: oidExtensionRequest,
            Value: new slice<pkix.Extension>[]{extensions}.slice()
        );
        var (b, errΔ1) = asn1.Marshal(attr);
        if (errΔ1 != default!) {
            return (default!, errors.New("x509: failed to serialise extensions attribute: "u8 + errΔ1.Error()));
        }
        ref var rawValue = ref heap(new asn1.RawValue(), out var ᏑrawValue);
        {
            var (_, errΔ2) = asn1.Unmarshal(b, ᏑrawValue); if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
        }
        rawAttributes = append(rawAttributes, rawValue);
    }
    var asn1Subject = template.RawSubject;
    if (builtin.len(asn1Subject) == 0) {
        (asn1Subject, err) = asn1.Marshal(template.Subject.ToRDNSequence());
        if (err != default!) {
            return (default!, err);
        }
    }
    var tbsCSR = new tbsCertificateRequest(
        Version: 0, // PKCS #10, RFC 2986

        Subject: new asn1.RawValue(FullBytes: asn1Subject),
        PublicKey: new publicKeyInfo(
            Algorithm: publicKeyAlgorithm,
            PublicKey: new asn1.BitString(
                Bytes: publicKeyBytes,
                BitLength: builtin.len(publicKeyBytes) * 8
            )
        ),
        RawAttributes: rawAttributes
    );
    (var tbsCSRContents, err) = asn1.Marshal(tbsCSR);
    if (err != default!) {
        return (default!, err);
    }
    tbsCSR.Raw = tbsCSRContents;
    (var signature, err) = signTBS(tbsCSRContents, key, signatureAlgorithm, rand);
    if (err != default!) {
        return (default!, err);
    }
    return asn1.Marshal(new certificateRequest(
        TBSCSR: tbsCSR,
        SignatureAlgorithm: algorithmIdentifier,
        SignatureValue: new asn1.BitString(Bytes: signature, BitLength: builtin.len(signature) * 8)
    ));
}

// ParseCertificateRequest parses a single certificate request from the
// given ASN.1 DER data.
public static (ж<CertificateRequest>, error) ParseCertificateRequest(slice<byte> asn1Data) {
    ref var csr = ref heap(new certificateRequest(), out var Ꮡcsr);
    var (rest, err) = asn1.Unmarshal(asn1Data, Ꮡcsr);
    if (err != default!){
        return (default!, err);
    } else 
    if (builtin.len(rest) != 0) {
        return (default!, new asn1.SyntaxError(Msg: "trailing data"u8));
    }
    return parseCertificateRequest(ref csr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509TrailingDataAfterXˢ = "x509: trailing data after X.509 Subject"u8;

internal static (ж<CertificateRequest>, error) parseCertificateRequest(ref certificateRequest @in) {
    var @out = Ꮡ(new CertificateRequest(
        Raw: @in.Raw,
        RawTBSCertificateRequest: @in.TBSCSR.Raw,
        RawSubjectPublicKeyInfo: @in.TBSCSR.PublicKey.Raw,
        RawSubject: @in.TBSCSR.Subject.FullBytes,
        Signature: @in.SignatureValue.RightAlign(),
        SignatureAlgorithm: getSignatureAlgorithmFromAI(@in.SignatureAlgorithm),
        PublicKeyAlgorithm: getPublicKeyAlgorithmFromOID(@in.TBSCSR.PublicKey.Algorithm.Algorithm),
        Version: @in.TBSCSR.Version,
        Attributes: parseRawAttributes(@in.TBSCSR.RawAttributes)
    ));
    error err = default!;
    if ((~@out).PublicKeyAlgorithm != UnknownPublicKeyAlgorithm) {
        (@out.Value.PublicKey, err) = parsePublicKey(ref nonnil(ref @in).TBSCSR.PublicKey);
        if (err != default!) {
            return (default!, err);
        }
    }
    ref var subject = ref heap<pkix.RDNSequence>(out var Ꮡsubject);
    {
        var (rest, errΔ1) = asn1.Unmarshal(@in.TBSCSR.Subject.FullBytes, Ꮡsubject); if (errΔ1 != default!){
            return (default!, errΔ1);
        } else 
        if (builtin.len(rest) != 0) {
            return (default!, errors.New(x509TrailingDataAfterXˢ));
        }
    }
    @out.of(CertificateRequest.ᏑSubject).FillFromRDNSequence(Ꮡsubject);
    {
        (@out.Value.Extensions, err) = parseCSRExtensions(@in.TBSCSR.RawAttributes); if (err != default!) {
            return (default!, err);
        }
    }
    foreach (var (_, extension) in (~@out).Extensions) {
        switch (ᐧ) {
        case {} when extension.Id.Equal(oidExtensionSubjectAltName): {
            (@out.Value.DNSNames, @out.Value.EmailAddresses, @out.Value.IPAddresses, @out.Value.URIs, err) = parseSANExtension(extension.Value);
            if (err != default!) {
                return (default!, err);
            }
            break;
        }}

    }
    return (@out, default!);
}

// CheckSignature reports whether the signature on c is valid.
[GoRecv] public static error CheckSignature(this ref CertificateRequest c) {
    return checkSignature(c.SignatureAlgorithm, c.RawTBSCertificateRequest, c.Signature, c.PublicKey, true);
}

// RevocationListEntry represents an entry in the revokedCertificates
// sequence of a CRL.
[GoType] partial struct RevocationListEntry {
    // Raw contains the raw bytes of the revokedCertificates entry. It is set when
    // parsing a CRL; it is ignored when generating a CRL.
    public slice<byte> Raw;
    // SerialNumber represents the serial number of a revoked certificate. It is
    // both used when creating a CRL and populated when parsing a CRL. It must not
    // be nil.
    public ж<bigꓸInt> SerialNumber;
    // RevocationTime represents the time at which the certificate was revoked. It
    // is both used when creating a CRL and populated when parsing a CRL. It must
    // not be the zero time.
    public time.Time RevocationTime;
    // ReasonCode represents the reason for revocation, using the integer enum
    // values specified in RFC 5280 Section 5.3.1. When creating a CRL, the zero
    // value will result in the reasonCode extension being omitted. When parsing a
    // CRL, the zero value may represent either the reasonCode extension being
    // absent (which implies the default revocation reason of 0/Unspecified), or
    // it may represent the reasonCode extension being present and explicitly
    // containing a value of 0/Unspecified (which should not happen according to
    // the DER encoding rules, but can and does happen anyway).
    public nint ReasonCode;
    // Extensions contains raw X.509 extensions. When parsing CRL entries,
    // this can be used to extract non-critical extensions that are not
    // parsed by this package. When marshaling CRL entries, the Extensions
    // field is ignored, see ExtraExtensions.
    public slice<pkix.Extension> Extensions;
    // ExtraExtensions contains extensions to be copied, raw, into any
    // marshaled CRL entries. Values override any extensions that would
    // otherwise be produced based on the other fields. The ExtraExtensions
    // field is not populated when parsing CRL entries, see Extensions.
    public slice<pkix.Extension> ExtraExtensions;
}

// RevocationList represents a [Certificate] Revocation List (CRL) as specified
// by RFC 5280.
[GoType] partial struct RevocationList {
    // Raw contains the complete ASN.1 DER content of the CRL (tbsCertList,
    // signatureAlgorithm, and signatureValue.)
    public slice<byte> Raw;
    // RawTBSRevocationList contains just the tbsCertList portion of the ASN.1
    // DER.
    public slice<byte> RawTBSRevocationList;
    // RawIssuer contains the DER encoded Issuer.
    public slice<byte> RawIssuer;
    // Issuer contains the DN of the issuing certificate.
    public pkix.Name Issuer;
    // AuthorityKeyId is used to identify the public key associated with the
    // issuing certificate. It is populated from the authorityKeyIdentifier
    // extension when parsing a CRL. It is ignored when creating a CRL; the
    // extension is populated from the issuing certificate itself.
    public slice<byte> AuthorityKeyId;
    public slice<byte> Signature;
    // SignatureAlgorithm is used to determine the signature algorithm to be
    // used when signing the CRL. If 0 the default algorithm for the signing
    // key will be used.
    public SignatureAlgorithm SignatureAlgorithm;
    // RevokedCertificateEntries represents the revokedCertificates sequence in
    // the CRL. It is used when creating a CRL and also populated when parsing a
    // CRL. When creating a CRL, it may be empty or nil, in which case the
    // revokedCertificates ASN.1 sequence will be omitted from the CRL entirely.
    public slice<RevocationListEntry> RevokedCertificateEntries;
    // RevokedCertificates is used to populate the revokedCertificates
    // sequence in the CRL if RevokedCertificateEntries is empty. It may be empty
    // or nil, in which case an empty CRL will be created.
    //
    // Deprecated: Use RevokedCertificateEntries instead.
    public slice<pkix.RevokedCertificate> RevokedCertificates;
    // Number is used to populate the X.509 v2 cRLNumber extension in the CRL,
    // which should be a monotonically increasing sequence number for a given
    // CRL scope and CRL issuer. It is also populated from the cRLNumber
    // extension when parsing a CRL.
    public ж<bigꓸInt> Number;
    // ThisUpdate is used to populate the thisUpdate field in the CRL, which
    // indicates the issuance date of the CRL.
    public time.Time ThisUpdate;
    // NextUpdate is used to populate the nextUpdate field in the CRL, which
    // indicates the date by which the next CRL will be issued. NextUpdate
    // must be greater than ThisUpdate.
    public time.Time NextUpdate;
    // Extensions contains raw X.509 extensions. When creating a CRL,
    // the Extensions field is ignored, see ExtraExtensions.
    public slice<pkix.Extension> Extensions;
    // ExtraExtensions contains any additional extensions to add directly to
    // the CRL.
    public slice<pkix.Extension> ExtraExtensions;
}

// These structures reflect the ASN.1 structure of X.509 CRLs better than
// the existing crypto/x509/pkix variants do. These mirror the existing
// certificate structs in this file.
//
// Notably, we include issuer as an asn1.RawValue, mirroring the behavior of
// tbsCertificate and allowing raw (unparsed) subjects to be passed cleanly.
[GoType] partial struct certificateList {
    public tbsCertificateList TBSCertList;
    public pkix.AlgorithmIdentifier SignatureAlgorithm;
    public asn1.BitString SignatureValue;
}

[GoType] public partial struct tbsCertificateList {
    public asn1.RawContent Raw;
    [GoTag(@"asn1:""optional,default:0""")]
    public nint Version;
    public pkix.AlgorithmIdentifier Signature;
    public asn1.RawValue Issuer;
    public time.Time ThisUpdate;
    [GoTag(@"asn1:""optional""")]
    public time.Time NextUpdate;
    [GoTag(@"asn1:""optional""")]
    public slice<pkix.RevokedCertificate> RevokedCertificates;
    [GoTag(@"asn1:""tag:0,optional,explicit""")]
    public slice<pkix.Extension> Extensions;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509TemplateCanNotBeNilˢ = "x509: template can not be nil"u8;
internal static readonly @string x509IssuerCanNotBeNilˢ = "x509: issuer can not be nil"u8;
internal static readonly @string x509IssuerMustHaveTheˢ = "x509: issuer must have the crlSign key usage bit set"u8;
internal static readonly @string x509IssuerCertificateˢ = "x509: issuer certificate doesn't contain a subject key identifier"u8;
internal static readonly @string x509TemplateThisUpdateIsˢ = "x509: template.ThisUpdate is after template.NextUpdate"u8;
internal static readonly @string x509TemplateContainsNilˢ = "x509: template contains nil Number field"u8;
internal static readonly @string x509TemplateContainsˢ = "x509: template contains entry with nil SerialNumber field"u8;
internal static readonly @string x509TemplateContainsˢ2 = "x509: template contains entry with zero RevocationTime field"u8;
internal static readonly @string x509TemplateContainsˢ3 = "x509: template contains entry with ReasonCode ExtraExtension; use ReasonCode field instead"u8;
internal static readonly @string x509CrlNumberExceeds20ˢ = "x509: CRL number exceeds 20 octets"u8;

// CreateRevocationList creates a new X.509 v2 [Certificate] Revocation List,
// according to RFC 5280, based on template.
//
// The CRL is signed by priv which should be the private key associated with
// the public key in the issuer certificate.
//
// The issuer may not be nil, and the crlSign bit must be set in [KeyUsage] in
// order to use it as a CRL issuer.
//
// The issuer distinguished name CRL field and authority key identifier
// extension are populated using the issuer certificate. issuer must have
// SubjectKeyId set.
public static (slice<byte>, error) CreateRevocationList(io.Reader rand, ж<RevocationList> Ꮡtemplate, ж<Certificate> Ꮡissuer, crypto.Signer priv) {
    ref var template = ref Ꮡtemplate.DerefOrNull();
    ref var issuer = ref Ꮡissuer.DerefOrNull();

    if (Ꮡtemplate == nil) {
        return (default!, errors.New(x509TemplateCanNotBeNilˢ));
    }
    if (Ꮡissuer == nil) {
        return (default!, errors.New(x509IssuerCanNotBeNilˢ));
    }
    if (((KeyUsage)(issuer.KeyUsage & KeyUsageCRLSign)) == 0) {
        return (default!, errors.New(x509IssuerMustHaveTheˢ));
    }
    if (builtin.len(issuer.SubjectKeyId) == 0) {
        return (default!, errors.New(x509IssuerCertificateˢ));
    }
    if (template.NextUpdate.Before(template.ThisUpdate)) {
        return (default!, errors.New(x509TemplateThisUpdateIsˢ));
    }
    if (template.Number == nil) {
        return (default!, errors.New(x509TemplateContainsNilˢ));
    }
    var (signatureAlgorithm, algorithmIdentifier, err) = signingParamsForKey(priv, template.SignatureAlgorithm);
    if (err != default!) {
        return (default!, err);
    }
    slice<pkix.RevokedCertificate> revokedCerts = default!;
    // Only process the deprecated RevokedCertificates field if it is populated
    // and the new RevokedCertificateEntries field is not populated.
    if (builtin.len(template.RevokedCertificates) > 0 && builtin.len(template.RevokedCertificateEntries) == 0){
        // Force revocation times to UTC per RFC 5280.
        revokedCerts = new slice<pkix.RevokedCertificate>(builtin.len(template.RevokedCertificates));
        foreach (var (i, vᴛ1) in template.RevokedCertificates) {
            var rc = vᴛ1;

            rc.RevocationTime = rc.RevocationTime.UTC();
            revokedCerts[i] = rc;
        }
    } else {
        // Convert the ReasonCode field to a proper extension, and force revocation
        // times to UTC per RFC 5280.
        revokedCerts = new slice<pkix.RevokedCertificate>(builtin.len(template.RevokedCertificateEntries));
        foreach (var (i, rce) in template.RevokedCertificateEntries) {
            if (rce.SerialNumber == nil) {
                return (default!, errors.New(x509TemplateContainsˢ));
            }
            if (rce.RevocationTime.IsZero()) {
                return (default!, errors.New(x509TemplateContainsˢ2));
            }
            var rc = new pkix.RevokedCertificate(
                SerialNumber: rce.SerialNumber,
                RevocationTime: rce.RevocationTime.UTC()
            );
            // Copy over any extra extensions, except for a Reason Code extension,
            // because we'll synthesize that ourselves to ensure it is correct.
            var exts = new slice<pkix.Extension>(0, builtin.len(rce.ExtraExtensions));
            foreach (var (_, ext) in rce.ExtraExtensions) {
                if (ext.Id.Equal(oidExtensionReasonCode)) {
                    return (default!, errors.New(x509TemplateContainsˢ3));
                }
                exts = append(exts, ext);
            }
            // Only add a reasonCode extension if the reason is non-zero, as per
            // RFC 5280 Section 5.3.1.
            if (rce.ReasonCode != 0) {
                var (reasonBytes, errΔ1) = asn1.Marshal(((asn1.Enumerated)rce.ReasonCode));
                if (errΔ1 != default!) {
                    return (default!, errΔ1);
                }
                exts = append(exts, new pkix.Extension(
                    Id: oidExtensionReasonCode,
                    Value: reasonBytes
                ));
            }
            if (builtin.len(exts) > 0) {
                rc.Extensions = exts;
            }
            revokedCerts[i] = rc;
        }
    }
    (var aki, err) = asn1.Marshal(new authKeyId(Id: issuer.SubjectKeyId));
    if (err != default!) {
        return (default!, err);
    }
    {
        var numBytes = template.Number.Bytes(); if (builtin.len(numBytes) > 20 || (builtin.len(numBytes) == 20 && (byte)(numBytes[0] & 0x80) != 0)) {
            return (default!, errors.New(x509CrlNumberExceeds20ˢ));
        }
    }
    (var crlNum, err) = asn1.Marshal(template.Number.OrTypedNil());
    if (err != default!) {
        return (default!, err);
    }
    // Correctly use the issuer's subject sequence if one is specified.
    (var issuerSubject, err) = subjectBytes(ref (Ꮡissuer).DerefOrNull());
    if (err != default!) {
        return (default!, err);
    }
    var tbsCertList = new tbsCertificateList(
        Version: 1, // v2

        Signature: algorithmIdentifier,
        Issuer: new asn1.RawValue(FullBytes: issuerSubject),
        ThisUpdate: template.ThisUpdate.UTC(),
        NextUpdate: template.NextUpdate.UTC(),
        Extensions: new pkix.Extension[]{
            new(
                Id: oidExtensionAuthorityKeyId,
                Value: aki
            ),
            new(
                Id: oidExtensionCRLNumber,
                Value: crlNum
            )
        }.slice()
    );
    if (builtin.len(revokedCerts) > 0) {
        tbsCertList.RevokedCertificates = revokedCerts;
    }
    if (builtin.len(template.ExtraExtensions) > 0) {
        tbsCertList.Extensions = appendꓸꓸꓸ(tbsCertList.Extensions, template.ExtraExtensions);
    }
    (var tbsCertListContents, err) = asn1.Marshal(tbsCertList);
    if (err != default!) {
        return (default!, err);
    }
    // Optimization to only marshal this struct once, when signing and
    // then embedding in certificateList below.
    tbsCertList.Raw = tbsCertListContents;
    (var signature, err) = signTBS(tbsCertListContents, priv, signatureAlgorithm, rand);
    if (err != default!) {
        return (default!, err);
    }
    return asn1.Marshal(new certificateList(
        TBSCertList: tbsCertList,
        SignatureAlgorithm: algorithmIdentifier,
        SignatureValue: new asn1.BitString(Bytes: signature, BitLength: builtin.len(signature) * 8)
    ));
}

// CheckSignatureFrom verifies that the signature on rl is a valid signature
// from issuer.
[GoRecv] public static error CheckSignatureFrom(this ref RevocationList rl, ж<Certificate> Ꮡparent) {
    ref var parent = ref Ꮡparent.DerefOrNull();

    if (parent.Version == 3 && !parent.BasicConstraintsValid || parent.BasicConstraintsValid && !parent.IsCA) {
        return new ConstraintViolationError(nil);
    }
    if (parent.KeyUsage != 0 && (KeyUsage)(parent.KeyUsage & KeyUsageCRLSign) == 0) {
        return new ConstraintViolationError(nil);
    }
    if (parent.PublicKeyAlgorithm == UnknownPublicKeyAlgorithm) {
        return ErrUnsupportedAlgorithm;
    }
    return parent.CheckSignature(rl.SignatureAlgorithm, rl.RawTBSRevocationList, rl.Signature);
}

} // end x509_package
