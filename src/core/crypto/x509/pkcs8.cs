// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ecdh = go.crypto.ecdh_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using rsa = go.crypto.rsa_package;
using pkix = go.crypto.x509.pkix_package;
using asn1 = encoding.asn1_package;
using errors = errors_package;
using fmt = fmt_package;
using elliptic = go.crypto.elliptic_package;
using encoding;
using go.crypto;
using go.crypto.x509;

partial class x509_package {

// pkcs8 reflects an ASN.1, PKCS #8 PrivateKey. See
// ftp://ftp.rsasecurity.com/pub/pkcs/pkcs-8/pkcs-8v1_2.asn
// and RFC 5208.
partial struct pkcs8 {
    public nint Version;
    public pkix.AlgorithmIdentifier Algo;
    public slice<byte> PrivateKey;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509FailedToParsePrivateˢ3 = "x509: failed to parse private key (use ParsePKCS1PrivateKey instead for this key format)"u8;
internal static readonly @string x509InvalidEd25519ˢ = "x509: invalid Ed25519 private key parameters"u8;
internal static readonly @string x509InvalidX25519Privateˢ = "x509: invalid X25519 private key parameters"u8;

// optional attributes omitted.

// ParsePKCS8PrivateKey parses an unencrypted private key in PKCS #8, ASN.1 DER form.
//
// It returns a *[rsa.PrivateKey], an *[ecdsa.PrivateKey], an [ed25519.PrivateKey] (not
// a pointer), or an *[ecdh.PrivateKey] (for X25519). More types might be supported
// in the future.
//
// This kind of key is commonly encoded in PEM blocks of type "PRIVATE KEY".
//
// Before Go 1.24, the CRT parameters of RSA keys were ignored and recomputed.
// To restore the old behavior, use the GODEBUG=x509rsacrt=0 environment variable.
public static (any key, error err) ParsePKCS8PrivateKey(slice<byte> der) {
    any key = default!;
    error err = default!;

    ref var privKey = ref heap(new pkcs8(), out var ᏑprivKey);
    {
        var (_, errΔ1) = asn1.Unmarshal(der, ᏑprivKey); if (errΔ1 != default!) {
            {
                var (_, errΔ2) = asn1.Unmarshal(der, Ꮡ(new ecPrivateKey(nil))); if (errΔ2 == default!) {
                    return (default!, errors.New(x509FailedToParsePrivateˢ));
                }
            }
            {
                var (_, errΔ3) = asn1.Unmarshal(der, Ꮡ(new pkcs1PrivateKey(nil))); if (errΔ3 == default!) {
                    return (default!, errors.New(x509FailedToParsePrivateˢ3));
                }
            }
            return (default!, errΔ1);
        }
    }
    switch (ᐧ) {
    case {} when privKey.Algo.Algorithm.Equal(oidPublicKeyRSA): {
        (key, err) = ParsePKCS1PrivateKey(privKey.PrivateKey);
        if (err != default!) {
            return (default!, errors.New("x509: failed to parse RSA private key embedded in PKCS#8: "u8 + err.Error()));
        }
        return (key, default!);
    }
    case {} when privKey.Algo.Algorithm.Equal(oidPublicKeyECDSA): {
        var bytes = privKey.Algo.Parameters.FullBytes;
        var namedCurveOID = @new<asn1.ObjectIdentifier>();
        {
            var (_, errΔ7) = asn1.Unmarshal(bytes, namedCurveOID.OrTypedNil()); if (errΔ7 != default!) {
                namedCurveOID = default!;
            }
        }
        (key, err) = parseECPrivateKey(namedCurveOID, privKey.PrivateKey);
        if (err != default!) {
            return (default!, errors.New("x509: failed to parse EC private key embedded in PKCS#8: "u8 + err.Error()));
        }
        return (key, default!);
    }
    case {} when privKey.Algo.Algorithm.Equal(oidPublicKeyEd25519): {
        {
            nint l = builtin.len(privKey.Algo.Parameters.FullBytes); if (l != 0) {
                return (default!, errors.New(x509InvalidEd25519ˢ));
            }
        }
        ref var curvePrivateKey = ref heap<slice<byte>>(out var ᏑcurvePrivateKey);
        {
            var (_, errΔ8) = asn1.Unmarshal(privKey.PrivateKey, ᏑcurvePrivateKey); if (errΔ8 != default!) {
                return (default!, fmt.Errorf("x509: invalid Ed25519 private key: %v"u8, errΔ8));
            }
        }
        {
            nint l = builtin.len(curvePrivateKey); if (l != ed25519.SeedSize) {
                return (default!, fmt.Errorf("x509: invalid Ed25519 private key length: %d"u8, l));
            }
        }
        return (ed25519.NewKeyFromSeed(curvePrivateKey), default!);
    }
    case {} when privKey.Algo.Algorithm.Equal(oidPublicKeyX25519): {
        {
            nint l = builtin.len(privKey.Algo.Parameters.FullBytes); if (l != 0) {
                return (default!, errors.New(x509InvalidX25519Privateˢ));
            }
        }
        ref var curvePrivateKey = ref heap<slice<byte>>(out var ᏑcurvePrivateKey);
        {
            var (_, errΔ9) = asn1.Unmarshal(privKey.PrivateKey, ᏑcurvePrivateKey); if (errΔ9 != default!) {
                return (default!, fmt.Errorf("x509: invalid X25519 private key: %v"u8, errΔ9));
            }
        }
        var (ᴛ1, ᴛ2) = ecdh.X25519().NewPrivateKey(curvePrivateKey);
        return (ᴛ1.OrTypedNil(), ᴛ2);
    }
    default: {
        return (default!, fmt.Errorf("x509: PKCS#8 wrapping contained private key with unknown algorithm: %v"u8, privKey.Algo.Algorithm));
    }}

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509UnknownCurveWhileˢ = "x509: unknown curve while marshaling to PKCS#8"u8;

// MarshalPKCS8PrivateKey converts a private key to PKCS #8, ASN.1 DER form.
//
// The following key types are currently supported: *[rsa.PrivateKey],
// *[ecdsa.PrivateKey], [ed25519.PrivateKey] (not a pointer), and *[ecdh.PrivateKey].
// Unsupported key types result in an error.
//
// This kind of key is commonly encoded in PEM blocks of type "PRIVATE KEY".
//
// MarshalPKCS8PrivateKey runs [rsa.PrivateKey.Precompute] on RSA keys.
public static (slice<byte>, error) MarshalPKCS8PrivateKey(any key) {
    pkcs8 privKey = default!;
    switch (key.type()) {
    case ж<rsa.PrivateKey> k: {
        privKey.Algo = new pkix.AlgorithmIdentifier(
            Algorithm: oidPublicKeyRSA,
            Parameters: asn1.NullRawValue
        );
        k.Precompute();
        {
            var err = k.Validate(); if (err != default!) {
                return (default!, err);
            }
        }
        privKey.PrivateKey = MarshalPKCS1PrivateKey(k);
        break;
    }
    case ж<ecdsa.PrivateKey> k: {
        var (oid, ok) = oidFromNamedCurve((~k).Curve);
        if (!ok) {
            return (default!, errors.New(x509UnknownCurveWhileˢ));
        }
        var (oidBytes, err) = asn1.Marshal(oid);
        if (err != default!) {
            return (default!, errors.New("x509: failed to marshal curve OID: "u8 + err.Error()));
        }
        privKey.Algo = new pkix.AlgorithmIdentifier(
            Algorithm: oidPublicKeyECDSA,
            Parameters: new asn1.RawValue(
                FullBytes: oidBytes
            )
        );
        {
            (privKey.PrivateKey, err) = marshalECPrivateKeyWithOID(ref (k).DerefOrNull(), default!); if (err != default!) {
                return (default!, errors.New("x509: failed to marshal EC private key while building PKCS#8: "u8 + err.Error()));
            }
        }
        break;
    }
    case ed25519.PrivateKey k: {
        privKey.Algo = new pkix.AlgorithmIdentifier(
            Algorithm: oidPublicKeyEd25519
        );
        var (curvePrivateKey, err) = asn1.Marshal(k.Seed());
        if (err != default!) {
            return (default!, fmt.Errorf("x509: failed to marshal private key: %v"u8, err));
        }
        privKey.PrivateKey = curvePrivateKey;
        break;
    }
    case ж<ecdh.PrivateKey> k: {
        if (AreEqual(k.Curve(), ecdh.X25519())){
            privKey.Algo = new pkix.AlgorithmIdentifier(
                Algorithm: oidPublicKeyX25519
            );
            error err = default!;
            {
                (privKey.PrivateKey, err) = asn1.Marshal(k.Bytes()); if (err != default!) {
                    return (default!, fmt.Errorf("x509: failed to marshal private key: %v"u8, err));
                }
            }
        } else {
            var (oid, ok) = oidFromECDHCurve(k.Curve());
            if (!ok) {
                return (default!, errors.New(x509UnknownCurveWhileˢ));
            }
            var (oidBytes, err) = asn1.Marshal(oid);
            if (err != default!) {
                return (default!, errors.New("x509: failed to marshal curve OID: "u8 + err.Error()));
            }
            privKey.Algo = new pkix.AlgorithmIdentifier(
                Algorithm: oidPublicKeyECDSA,
                Parameters: new asn1.RawValue(
                    FullBytes: oidBytes
                )
            );
            {
                (privKey.PrivateKey, err) = marshalECDHPrivateKey(k); if (err != default!) {
                    return (default!, errors.New("x509: failed to marshal EC private key while building PKCS#8: "u8 + err.Error()));
                }
            }
        }
        break;
    }
    default: {
        var k = key;
        return (default!, fmt.Errorf("x509: unknown key type while marshaling PKCS#8: %T"u8, key));
    }}
    return asn1.Marshal(privKey);
}

} // end x509_package
