// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using rsa = go.crypto.rsa_package;
using asn1 = encoding.asn1_package;
using errors = errors_package;
using godebug = go.@internal.godebug_package;
using big = go.math.big_package;
using encoding;
using go.@internal;
using go.crypto;
using go.math;

partial class x509_package {

// pkcs1PrivateKey is a structure which mirrors the PKCS #1 ASN.1 for an RSA private key.
partial struct pkcs1PrivateKey {
    public nint Version;
    public ж<bigꓸInt> N;
    public nint E;
    public ж<bigꓸInt> D;
    public ж<bigꓸInt> P;
    public ж<bigꓸInt> Q;
    public ж<bigꓸInt> Dp; /*`asn1:"optional"`*/
    public ж<bigꓸInt> Dq; /*`asn1:"optional"`*/
    public ж<bigꓸInt> Qinv; /*`asn1:"optional"`*/
    public slice<pkcs1AdditionalRSAPrime> AdditionalPrimes; /*`asn1:"optional,omitempty"`*/
}

public partial struct pkcs1AdditionalRSAPrime {
    public ж<bigꓸInt> Prime;
    // We ignore these values because rsa will calculate them.
    public ж<bigꓸInt> Exp;
    public ж<bigꓸInt> Coeff;
}

// pkcs1PublicKey reflects the ASN.1 structure of a PKCS #1 public key.
partial struct pkcs1PublicKey {
    public ж<bigꓸInt> N;
    public nint E;
}

// x509rsacrt, if zero, makes ParsePKCS1PrivateKey ignore and recompute invalid
// CRT values in the RSA private key.
internal static ж<godebug.Setting> x509rsacrt = godebug.New("x509rsacrt"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509FailedToParsePrivateˢ = "x509: failed to parse private key (use ParseECPrivateKey instead for this key format)"u8;
internal static readonly @string x509FailedToParsePrivateˢ2 = "x509: failed to parse private key (use ParsePKCS8PrivateKey instead for this key format)"u8;
internal static readonly @string x509UnsupportedPrivateˢ = "x509: unsupported private key version"u8;
internal static readonly @string x509PrivateKeyContainsˢ = "x509: private key contains zero or negative value"u8;
internal static readonly @string x509PrivateKeyContainsˢ2 = "x509: private key contains zero or negative prime"u8;

// ParsePKCS1PrivateKey parses an [RSA] private key in PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PRIVATE KEY".
//
// Before Go 1.24, the CRT parameters were ignored and recomputed. To restore
// the old behavior, use the GODEBUG=x509rsacrt=0 environment variable.
public static (ж<rsa.PrivateKey>, error) ParsePKCS1PrivateKey(slice<byte> der) {
    ref var priv = ref heap(new pkcs1PrivateKey(), out var Ꮡpriv);
    var (rest, err) = asn1.Unmarshal(der, Ꮡpriv);
    if (builtin.len(rest) > 0) {
        return (default!, new asn1.SyntaxError(Msg: "trailing data"u8));
    }
    if (err != default!) {
        {
            var (_, errΔ1) = asn1.Unmarshal(der, Ꮡ(new ecPrivateKey(nil))); if (errΔ1 == default!) {
                return (default!, errors.New(x509FailedToParsePrivateˢ));
            }
        }
        {
            var (_, errΔ2) = asn1.Unmarshal(der, Ꮡ(new pkcs8(nil))); if (errΔ2 == default!) {
                return (default!, errors.New(x509FailedToParsePrivateˢ2));
            }
        }
        return (default!, err);
    }
    if (priv.Version > 1) {
        return (default!, errors.New(x509UnsupportedPrivateˢ));
    }
    if (priv.N.Sign() <= 0 || priv.D.Sign() <= 0 || priv.P.Sign() <= 0 || priv.Q.Sign() <= 0 || priv.Dp != nil && priv.Dp.Sign() <= 0 || priv.Dq != nil && priv.Dq.Sign() <= 0 || priv.Qinv != nil && priv.Qinv.Sign() <= 0) {
        return (default!, errors.New(x509PrivateKeyContainsˢ));
    }
    var key = @new<rsa.PrivateKey>();
    key.Value.PublicKey = new rsa.PublicKey(
        E: priv.E,
        N: priv.N
    );
    key.Value.D = priv.D;
    key.Value.Primes = new slice<ж<bigꓸInt>>(2 + builtin.len(priv.AdditionalPrimes));
    key.Value.Primes[0] = priv.P;
    key.Value.Primes[1] = priv.Q;
    key.Value.Precomputed.Dp = priv.Dp;
    key.Value.Precomputed.Dq = priv.Dq;
    key.Value.Precomputed.Qinv = priv.Qinv;
    foreach (var (i, a) in priv.AdditionalPrimes) {
        if (a.Prime.Sign() <= 0) {
            return (default!, errors.New(x509PrivateKeyContainsˢ2));
        }
        key.Value.Primes[i + 2] = a.Prime;
    }
    // We ignore the other two values because rsa will calculate
    // them as needed.
    key.Precompute();
    {
        var errΔ3 = key.Validate(); if (errΔ3 != default!) {
            // If x509rsacrt=0 is set, try dropping the CRT values and
            // rerunning precomputation and key validation.
            if (x509rsacrt.Value() == "0"u8) {
                key.Value.Precomputed.Dp = default!;
                key.Value.Precomputed.Dq = default!;
                key.Value.Precomputed.Qinv = default!;
                key.Precompute();
                {
                    var errΔ4 = key.Validate(); if (errΔ4 == default!) {
                        x509rsacrt.IncNonDefault();
                        return (key, default!);
                    }
                }
            }
            return (default!, errΔ3);
        }
    }
    return (key, default!);
}

// MarshalPKCS1PrivateKey converts an [RSA] private key to PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PRIVATE KEY".
// For a more flexible key format which is not [RSA] specific, use
// [MarshalPKCS8PrivateKey].
//
// The key must have passed validation by calling [rsa.PrivateKey.Validate]
// first. MarshalPKCS1PrivateKey calls [rsa.PrivateKey.Precompute], which may
// modify the key if not already precomputed.
public static slice<byte> MarshalPKCS1PrivateKey(ж<rsa.PrivateKey> Ꮡkey) {
    ref var key = ref Ꮡkey.DerefOrNull();

    key.Precompute();
    nint version = 0;
    if (builtin.len(key.Primes) > 2) {
        version = 1;
    }
    var priv = new pkcs1PrivateKey(
        Version: version,
        N: key.N,
        E: key.PublicKey.E,
        D: key.D,
        P: key.Primes[0],
        Q: key.Primes[1],
        Dp: key.Precomputed.Dp,
        Dq: key.Precomputed.Dq,
        Qinv: key.Precomputed.Qinv
    );
    priv.AdditionalPrimes = new slice<pkcs1AdditionalRSAPrime>(builtin.len(key.Precomputed.CRTValues));
    foreach (var (i, values) in key.Precomputed.CRTValues) {
        priv.AdditionalPrimes[i].Prime = key.Primes[2 + i];
        priv.AdditionalPrimes[i].Exp = values.Exp;
        priv.AdditionalPrimes[i].Coeff = values.Coeff;
    }
    var (b, _) = asn1.Marshal(priv);
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509FailedToParsePublicˢ = "x509: failed to parse public key (use ParsePKIXPublicKey instead for this key format)"u8;
internal static readonly @string x509PublicKeyContainsˢ = "x509: public key contains zero or negative value"u8;
internal static readonly @string x509PublicKeyContainsˢ2 = "x509: public key contains large public exponent"u8;

// ParsePKCS1PublicKey parses an [RSA] public key in PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PUBLIC KEY".
public static (ж<rsa.PublicKey>, error) ParsePKCS1PublicKey(slice<byte> der) {
    ref var pub = ref heap(new pkcs1PublicKey(), out var Ꮡpub);
    var (rest, err) = asn1.Unmarshal(der, Ꮡpub);
    if (err != default!) {
        {
            var (_, errΔ1) = asn1.Unmarshal(der, Ꮡ(new publicKeyInfo(nil))); if (errΔ1 == default!) {
                return (default!, errors.New(x509FailedToParsePublicˢ));
            }
        }
        return (default!, err);
    }
    if (builtin.len(rest) > 0) {
        return (default!, new asn1.SyntaxError(Msg: "trailing data"u8));
    }
    if (pub.N.Sign() <= 0 || pub.E <= 0) {
        return (default!, errors.New(x509PublicKeyContainsˢ));
    }
    if (pub.E > (nint)(2147483648L - 1)) {
        return (default!, errors.New(x509PublicKeyContainsˢ2));
    }
    return (Ꮡ(new rsa.PublicKey(
        E: pub.E,
        N: pub.N
    )), default!);
}

// MarshalPKCS1PublicKey converts an [RSA] public key to PKCS #1, ASN.1 DER form.
//
// This kind of key is commonly encoded in PEM blocks of type "RSA PUBLIC KEY".
public static slice<byte> MarshalPKCS1PublicKey(ж<rsa.PublicKey> Ꮡkey) {
    ref var key = ref Ꮡkey.DerefOrNull();

    var (derBytes, _) = asn1.Marshal(new pkcs1PublicKey(
        N: key.N,
        E: key.E
    ));
    return derBytes;
}

} // end x509_package
