// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

// This file implements signing and verification using PKCS #1 v1.5 signatures.
using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using errors = errors_package;
using go.crypto.@internal;

partial class rsa_package {

// A special TLS case which doesn't use an ASN1 prefix.
// These are ASN1 DER structures:
//
//	DigestInfo ::= SEQUENCE {
//	  digestAlgorithm AlgorithmIdentifier,
//	  digest OCTET STRING
//	}
//
// For performance, we don't use the generic ASN1 encoder. Rather, we
// precompute a prefix of the digest value that makes a valid ASN1 DER string
// with the correct contents.
internal static map<@string, slice<byte>> hashPrefixes = new map<@string, slice<byte>>{
    ["MD5"u8] = new byte[]{0x30, 0x20, 0x30, 0x0c, 0x06, 0x08, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x02, 0x05, 0x05, 0x00, 0x04, 0x10}.slice(),
    ["SHA-1"u8] = new byte[]{0x30, 0x21, 0x30, 0x09, 0x06, 0x05, 0x2b, 0x0e, 0x03, 0x02, 0x1a, 0x05, 0x00, 0x04, 0x14}.slice(),
    ["SHA-224"u8] = new byte[]{0x30, 0x2d, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x04, 0x05, 0x00, 0x04, 0x1c}.slice(),
    ["SHA-256"u8] = new byte[]{0x30, 0x31, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x01, 0x05, 0x00, 0x04, 0x20}.slice(),
    ["SHA-384"u8] = new byte[]{0x30, 0x41, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x02, 0x05, 0x00, 0x04, 0x30}.slice(),
    ["SHA-512"u8] = new byte[]{0x30, 0x51, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x03, 0x05, 0x00, 0x04, 0x40}.slice(),
    ["SHA-512/224"u8] = new byte[]{0x30, 0x2d, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x05, 0x05, 0x00, 0x04, 0x1C}.slice(),
    ["SHA-512/256"u8] = new byte[]{0x30, 0x31, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x06, 0x05, 0x00, 0x04, 0x20}.slice(),
    ["SHA3-224"u8] = new byte[]{0x30, 0x2d, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x07, 0x05, 0x00, 0x04, 0x1C}.slice(),
    ["SHA3-256"u8] = new byte[]{0x30, 0x31, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x08, 0x05, 0x00, 0x04, 0x20}.slice(),
    ["SHA3-384"u8] = new byte[]{0x30, 0x41, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x09, 0x05, 0x00, 0x04, 0x30}.slice(),
    ["SHA3-512"u8] = new byte[]{0x30, 0x51, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x0a, 0x05, 0x00, 0x04, 0x40}.slice(),
    ["MD5+SHA1"u8] = new byte[]{}.slice(),
    ["RIPEMD-160"u8] = new byte[]{0x30, 0x20, 0x30, 0x08, 0x06, 0x06, 0x28, 0xcf, 0x06, 0x03, 0x00, 0x31, 0x04, 0x14}.slice()
};

// SignPKCS1v15 calculates an RSASSA-PKCS1-v1.5 signature.
//
// hash is the name of the hash function as returned by [crypto.Hash.String]
// or the empty string to indicate that the message is signed directly.
public static (slice<byte>, error) SignPKCS1v15(ж<PrivateKey> Ꮡpriv, @string hash, slice<byte> hashed) {
    fipsSelfTest();
    fips140.RecordApproved();
    checkApprovedHashName(hash);
    return signPKCS1v15(Ꮡpriv, hash, hashed);
}

internal static (slice<byte>, error) signPKCS1v15(ж<PrivateKey> Ꮡpriv, @string hash, slice<byte> hashed) {
    var (em, err) = pkcs1v15ConstructEM(Ꮡpriv.of(PrivateKey.Ꮡpub), hash, hashed);
    if (err != default!) {
        return (default!, err);
    }
    return decrypt(ref (Ꮡpriv).DerefOrNull(), em, withCheck);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRsaUnsupportedHashˢ = "crypto/rsa: unsupported hash function"u8;

internal static (slice<byte>, error) pkcs1v15ConstructEM(ж<ΔPublicKey> Ꮡpub, @string hash, slice<byte> hashed) {
    ref var pub = ref Ꮡpub.DerefOrNull();

    // Special case: "" is used to indicate that the data is signed directly.
    slice<byte> prefix = default!;
    if (hash != ""u8) {
        bool ok = default!;
        (prefix, ok) = hashPrefixes[hash, ꟷ];
        if (!ok) {
            return (default!, errors.New(cryptoRsaUnsupportedHashˢ));
        }
    }
    // EM = 0x00 || 0x01 || PS || 0x00 || T
    nint k = pub.Size();
    if (k < len(prefix) + len(hashed) + 2 + 8 + 1) {
        return (default!, ErrMessageTooLong);
    }
    var em = new slice<byte>(k);
    em[1] = 1;
    for (nint i = 2; i < k - len(prefix) - len(hashed) - 1; i++) {
        em[i] = 0xff;
    }
    copy(em.slice(k - len(prefix) - len(hashed)), prefix);
    copy(em.slice(k - len(hashed)), hashed);
    return (em, default!);
}

// VerifyPKCS1v15 verifies an RSASSA-PKCS1-v1.5 signature.
//
// hash is the name of the hash function as returned by [crypto.Hash.String]
// or the empty string to indicate that the message is signed directly.
public static error VerifyPKCS1v15(ж<ΔPublicKey> Ꮡpub, @string hash, slice<byte> hashed, slice<byte> sig) {
    fipsSelfTest();
    fips140.RecordApproved();
    checkApprovedHashName(hash);
    return verifyPKCS1v15(Ꮡpub, hash, hashed, sig);
}

internal static error verifyPKCS1v15(ж<ΔPublicKey> Ꮡpub, @string hash, slice<byte> hashed, slice<byte> sig) {
    ref var pub = ref Ꮡpub.DerefOrNull();

    {
        var (fipsApproved, errΔ1) = checkPublicKey(ref (Ꮡpub).DerefOrNull()); if (errΔ1 != default!){
            return errΔ1;
        } else 
        if (!fipsApproved) {
            fips140.RecordNonApproved();
        }
    }
    // RFC 8017 Section 8.2.2: If the length of the signature S is not k
    // octets (where k is the length in octets of the RSA modulus n), output
    // "invalid signature" and stop.
    if (pub.Size() != len(sig)) {
        return ErrVerification;
    }
    var (em, err) = encrypt(ref (Ꮡpub).DerefOrNull(), sig);
    if (err != default!) {
        return ErrVerification;
    }
    (var expected, err) = pkcs1v15ConstructEM(Ꮡpub, hash, hashed);
    if (err != default!) {
        return ErrVerification;
    }
    if (!bytes.Equal(em, expected)) {
        return ErrVerification;
    }
    return default!;
}

internal static void checkApprovedHashName(@string hash) {
    var exprᴛ1 = hash;
    if (exprᴛ1 == "SHA-224"u8 || exprᴛ1 == "SHA-256"u8 || exprᴛ1 == "SHA-384"u8 || exprᴛ1 == "SHA-512"u8 || exprᴛ1 == "SHA-512/224"u8 || exprᴛ1 == "SHA-512/256"u8 || exprᴛ1 == "SHA3-224"u8 || exprᴛ1 == "SHA3-256"u8 || exprᴛ1 == "SHA3-384"u8 || exprᴛ1 == "SHA3-512"u8) {
    }
    else { /* default: */
        fips140.RecordNonApproved();
    }

}

} // end rsa_package
