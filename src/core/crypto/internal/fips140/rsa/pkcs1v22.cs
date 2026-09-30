// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

// This file implements the RSASSA-PSS signature scheme and the RSAES-OAEP
// encryption scheme according to RFC 8017, aka PKCS #1 v2.2.
using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using sha256 = go.crypto.@internal.fips140.sha256_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using sha512 = go.crypto.@internal.fips140.sha512_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using errors = errors_package;
using io = io_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class rsa_package {

// Per RFC 8017, Section 9.1
//
//     EM = MGF1 xor DB || H( 8*0x00 || mHash || salt ) || 0xbc
//
// where
//
//     DB = PS || 0x01 || salt
//
// and PS can be empty so
//
//     emLen = dbLen + hLen + 1 = psLen + sLen + hLen + 2
//

// incCounter increments a four byte, big-endian counter.
internal static void incCounter(ref array<byte> c) {
    {
        c[3]++; if (c[3] != 0) {
            return;
        }
    }
    {
        c[2]++; if (c[2] != 0) {
            return;
        }
    }
    {
        c[1]++; if (c[1] != 0) {
            return;
        }
    }
    c[0]++;
}

// mgf1XOR XORs the bytes in out with a mask generated using the MGF1 function
// specified in PKCS #1 v2.1.
internal static void mgf1XOR(slice<byte> @out, fips140.Hash hash, slice<byte> seed) {
    ref var counter = ref heap(new array<byte>(4), out var Ꮡcounter);
    slice<byte> digest = default!;
    nint done = 0;
    while (done < len(@out)) {
        hash.Reset();
        hash.Write(seed);
        hash.Write(counter[0..4]);
        digest = hash.Sum(digest[..0]);
        for (nint i = 0; i < len(digest) && done < len(@out); i++) {
            @out[done] ^= (byte)(digest[i]);
            done++;
        }
        incCounter(ref counter);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRsaInputMustBeˢ = "crypto/rsa: input must be hashed with given hash"u8;

internal static (slice<byte>, error) emsaPSSEncode(slice<byte> mHash, nint emBits, slice<byte> salt, fips140.Hash hash) {
    // See RFC 8017, Section 9.1.1.
    nint hLen = hash.Size();
    nint sLen = len(salt);
    nint emLen = (emBits + 7) / 8;
    // 1.  If the length of M is greater than the input limitation for the
    //     hash function (2^61 - 1 octets for SHA-1), output "message too
    //     long" and stop.
    //
    // 2.  Let mHash = Hash(M), an octet string of length hLen.
    if (len(mHash) != hLen) {
        return (default!, errors.New(cryptoRsaInputMustBeˢ));
    }
    // 3.  If emLen < hLen + sLen + 2, output "encoding error" and stop.
    if (emLen < hLen + sLen + 2) {
        return (default!, ErrMessageTooLong);
    }
    var em = new slice<byte>(emLen);
    nint psLen = emLen - sLen - hLen - 2;
    var db = em.slice(0, psLen + 1 + sLen);
    var h = em.slice(psLen + 1 + sLen, emLen - 1);
    // 4.  Generate a random octet string salt of length sLen; if sLen = 0,
    //     then salt is the empty string.
    //
    // 5.  Let
    //       M' = (0x)00 00 00 00 00 00 00 00 || mHash || salt;
    //
    //     M' is an octet string of length 8 + hLen + sLen with eight
    //     initial zero octets.
    //
    // 6.  Let H = Hash(M'), an octet string of length hLen.
    array<byte> prefix = new(8);
    hash.Reset();
    hash.Write(prefix[..]);
    hash.Write(mHash);
    hash.Write(salt);
    h = hash.Sum(h[..0]);
    // 7.  Generate an octet string PS consisting of emLen - sLen - hLen - 2
    //     zero octets. The length of PS may be 0.
    //
    // 8.  Let DB = PS || 0x01 || salt; DB is an octet string of length
    //     emLen - hLen - 1.
    db[psLen] = 0x01;
    copy(db.slice(psLen + 1), salt);
    // 9.  Let dbMask = MGF(H, emLen - hLen - 1).
    //
    // 10. Let maskedDB = DB \xor dbMask.
    mgf1XOR(db, hash, h);
    // 11. Set the leftmost 8 * emLen - emBits bits of the leftmost octet in
    //     maskedDB to zero.
    db[0] &= (byte)(((byte)0xff).Rsh((int64)((8 * emLen - emBits))));
    // 12. Let EM = maskedDB || H || 0xbc.
    em[emLen - 1] = 0xbc;
    // 13. Output EM.
    return (em, default!);
}

internal static UntypedInt pssSaltLengthAutodetect => -1;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rsaInternalErrorˢ = "rsa: internal error: inconsistent length"u8;

internal static error emsaPSSVerify(slice<byte> mHash, slice<byte> em, nint emBits, nint sLen, fips140.Hash hash) {
    // See RFC 8017, Section 9.1.2.
    nint hLen = hash.Size();
    nint emLen = (emBits + 7) / 8;
    if (emLen != len(em)) {
        return errors.New(rsaInternalErrorˢ);
    }
    // 1.  If the length of M is greater than the input limitation for the
    //     hash function (2^61 - 1 octets for SHA-1), output "inconsistent"
    //     and stop.
    //
    // 2.  Let mHash = Hash(M), an octet string of length hLen.
    if (hLen != len(mHash)) {
        return ErrVerification;
    }
    // 3.  If emLen < hLen + sLen + 2, output "inconsistent" and stop.
    if (emLen < hLen + sLen + 2) {
        return ErrVerification;
    }
    // 4.  If the rightmost octet of EM does not have hexadecimal value
    //     0xbc, output "inconsistent" and stop.
    if (em[emLen - 1] != 0xbc) {
        return ErrVerification;
    }
    // 5.  Let maskedDB be the leftmost emLen - hLen - 1 octets of EM, and
    //     let H be the next hLen octets.
    var db = em.slice(0, emLen - hLen - 1);
    var h = em.slice(emLen - hLen - 1, emLen - 1);
    // 6.  If the leftmost 8 * emLen - emBits bits of the leftmost octet in
    //     maskedDB are not all equal to zero, output "inconsistent" and
    //     stop.
    byte bitMask = (byte)(((byte)0xff).Rsh((int64)((8 * emLen - emBits))));
    if ((byte)(em[0] & ((byte)(~bitMask))) != 0) {
        return ErrVerification;
    }
    // 7.  Let dbMask = MGF(H, emLen - hLen - 1).
    //
    // 8.  Let DB = maskedDB \xor dbMask.
    mgf1XOR(db, hash, h);
    // 9.  Set the leftmost 8 * emLen - emBits bits of the leftmost octet in DB
    //     to zero.
    db[0] &= (byte)(bitMask);
    // If we don't know the salt length, look for the 0x01 delimiter.
    if (sLen == pssSaltLengthAutodetect) {
        nint psLenΔ1 = bytes.IndexByte(db, 0x01);
        if (psLenΔ1 < 0) {
            return ErrVerification;
        }
        sLen = len(db) - psLenΔ1 - 1;
    }
    // FIPS 186-5, Section 5.4(g): "the length (in bytes) of the salt (sLen)
    // shall satisfy 0 ≤ sLen ≤ hLen".
    if (sLen > hLen) {
        fips140.RecordNonApproved();
    }
    // 10. If the emLen - hLen - sLen - 2 leftmost octets of DB are not zero
    //     or if the octet at position emLen - hLen - sLen - 1 (the leftmost
    //     position is "position 1") does not have hexadecimal value 0x01,
    //     output "inconsistent" and stop.
    nint psLen = emLen - hLen - sLen - 2;
    foreach (var (_, e) in db.slice(0, psLen)) {
        if (e != 0x00) {
            return ErrVerification;
        }
    }
    if (db[psLen] != 0x01) {
        return ErrVerification;
    }
    // 11.  Let salt be the last sLen octets of DB.
    var salt = db.slice(len(db) - sLen);
    // 12.  Let
    //          M' = (0x)00 00 00 00 00 00 00 00 || mHash || salt ;
    //     M' is an octet string of length 8 + hLen + sLen with eight
    //     initial zero octets.
    //
    // 13. Let H' = Hash(M'), an octet string of length hLen.
    hash.Reset();
    array<byte> prefix = new(8);
    hash.Write(prefix[..]);
    hash.Write(mHash);
    hash.Write(salt);
    var h0 = hash.Sum(default!);
    // 14. If H = H', output "consistent." Otherwise, output "inconsistent."
    if (!bytes.Equal(h0, h)) {
        // TODO: constant time?
        return ErrVerification;
    }
    return default!;
}

// PSSMaxSaltLength returns the maximum salt length for a given public key and
// hash function.
public static (nint, error) PSSMaxSaltLength(ж<ΔPublicKey> Ꮡpub, fips140.Hash hash) {
    ref var pub = ref Ꮡpub.DerefOrNull();

    nint saltLength = (pub.N.BitLen() - 1 + 7) / 8 - 2 - hash.Size();
    if (saltLength < 0) {
        return (0, ErrMessageTooLong);
    }
    // FIPS 186-5, Section 5.4(g): "the length (in bytes) of the salt (sLen)
    // shall satisfy 0 ≤ sLen ≤ hLen".
    if (fips140.Enabled && saltLength > hash.Size()) {
        return (hash.Size(), default!);
    }
    return (saltLength, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRsaSaltLengthˢ = "crypto/rsa: salt length cannot be negative"u8;

// SignPSS calculates the signature of hashed using RSASSA-PSS.
public static (slice<byte>, error) SignPSS(io.Reader rand, ж<PrivateKey> Ꮡpriv, fips140.Hash hash, slice<byte> hashed, nint saltLength) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    fipsSelfTest();
    fips140.RecordApproved();
    checkApprovedHash(hash);
    // Note that while we don't commit to deterministic execution with respect
    // to the rand stream, we also don't apply MaybeReadByte, so per Hyrum's Law
    // it's probably relied upon by some. It's a tolerable promise because a
    // well-specified number of random bytes is included in the signature, in a
    // well-specified way.
    if (saltLength < 0) {
        return (default!, errors.New(cryptoRsaSaltLengthˢ));
    }
    // FIPS 186-5, Section 5.4(g): "the length (in bytes) of the salt (sLen)
    // shall satisfy 0 ≤ sLen ≤ hLen".
    if (saltLength > hash.Size()) {
        fips140.RecordNonApproved();
    }
    var salt = new slice<byte>(saltLength);
    {
        var errΔ1 = drbg.ReadWithReaderDeterministic(rand, salt); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    nint emBits = priv.pub.N.BitLen() - 1;
    var (em, err) = emsaPSSEncode(hashed, emBits, salt, hash);
    if (err != default!) {
        return (default!, err);
    }
    // RFC 8017: "Note that the octet length of EM will be one less than k if
    // modBits - 1 is divisible by 8 and equal to k otherwise, where k is the
    // length in octets of the RSA modulus n." 🙄
    //
    // This is extremely annoying, as all other encrypt and decrypt inputs are
    // always the exact same size as the modulus. Since it only happens for
    // weird modulus sizes, fix it by padding inefficiently.
    {
        nint emLen = len(em);
        nint k = priv.pub.Size(); if (emLen < k) {
            var emNew = new slice<byte>(k);
            copy(emNew.slice(k - emLen), em);
            em = emNew;
        }
    }
    return decrypt(ref (Ꮡpriv).DerefOrNull(), em, withCheck);
}

// VerifyPSS verifies sig with RSASSA-PSS automatically detecting the salt length.
public static error VerifyPSS(ж<ΔPublicKey> Ꮡpub, fips140.Hash hash, slice<byte> digest, slice<byte> sig) {
    return verifyPSS(Ꮡpub, hash, digest, sig, pssSaltLengthAutodetect);
}

// VerifyPSS verifies sig with RSASSA-PSS and an expected salt length.
public static error VerifyPSSWithSaltLength(ж<ΔPublicKey> Ꮡpub, fips140.Hash hash, slice<byte> digest, slice<byte> sig, nint saltLength) {
    if (saltLength < 0) {
        return errors.New(cryptoRsaSaltLengthˢ);
    }
    return verifyPSS(Ꮡpub, hash, digest, sig, saltLength);
}

internal static error verifyPSS(ж<ΔPublicKey> Ꮡpub, fips140.Hash hash, slice<byte> digest, slice<byte> sig, nint saltLength) {
    ref var pub = ref Ꮡpub.DerefOrNull();

    fipsSelfTest();
    fips140.RecordApproved();
    checkApprovedHash(hash);
    {
        var (fipsApproved, errΔ1) = checkPublicKey(ref (Ꮡpub).DerefOrNull()); if (errΔ1 != default!){
            return errΔ1;
        } else 
        if (!fipsApproved) {
            fips140.RecordNonApproved();
        }
    }
    if (len(sig) != pub.Size()) {
        return ErrVerification;
    }
    nint emBits = pub.N.BitLen() - 1;
    nint emLen = (emBits + 7) / 8;
    var (em, err) = encrypt(ref (Ꮡpub).DerefOrNull(), sig);
    if (err != default!) {
        return ErrVerification;
    }
    // Like in signPSSWithSalt, deal with mismatches between emLen and the size
    // of the modulus. The spec would have us wire emLen into the encoding
    // function, but we'd rather always encode to the size of the modulus and
    // then strip leading zeroes if necessary. This only happens for weird
    // modulus sizes anyway.
    while (len(em) > emLen && len(em) > 0) {
        if (em[0] != 0) {
            return ErrVerification;
        }
        em = em[1..];
    }
    return emsaPSSVerify(digest, em, emBits, saltLength, hash);
}

internal static void checkApprovedHash(fips140.Hash hash) {
    switch (hash.type()) {
    case ж<sha256.Digest> _:
    case ж<sha512.Digest> _:
    case ж<sha3.Digest> _: {
        break;
    }
    default: {
        fips140.RecordNonApproved();
        break;
    }}

}

// EncryptOAEP encrypts the given message with RSAES-OAEP.
public static (slice<byte>, error) EncryptOAEP(fips140.Hash hash, fips140.Hash mgfHash, io.Reader random, ж<ΔPublicKey> Ꮡpub, slice<byte> msg, slice<byte> label) {
    ref var pub = ref Ꮡpub.DerefOrNull();

    // Note that while we don't commit to deterministic execution with respect
    // to the random stream, we also don't apply MaybeReadByte, so per Hyrum's
    // Law it's probably relied upon by some. It's a tolerable promise because a
    // well-specified number of random bytes is included in the ciphertext, in a
    // well-specified way.
    fipsSelfTest();
    fips140.RecordApproved();
    checkApprovedHash(hash);
    {
        var (fipsApproved, err) = checkPublicKey(ref (Ꮡpub).DerefOrNull()); if (err != default!){
            return (default!, err);
        } else 
        if (!fipsApproved) {
            fips140.RecordNonApproved();
        }
    }
    nint k = pub.Size();
    if (len(msg) > k - 2 * hash.Size() - 2) {
        return (default!, ErrMessageTooLong);
    }
    hash.Reset();
    hash.Write(label);
    var lHash = hash.Sum(default!);
    var em = new slice<byte>(k);
    var seed = em.slice(1, 1 + hash.Size());
    var db = em.slice(1 + hash.Size());
    copy(db.slice(0, hash.Size()), lHash);
    db[len(db) - len(msg) - 1] = 1;
    copy(db.slice(len(db) - len(msg)), msg);
    {
        var err = drbg.ReadWithReaderDeterministic(random, seed); if (err != default!) {
            return (default!, err);
        }
    }
    mgf1XOR(db, mgfHash, seed);
    mgf1XOR(seed, mgfHash, db);
    return encrypt(ref (Ꮡpub).DerefOrNull(), em);
}

// DecryptOAEP decrypts ciphertext using RSAES-OAEP.
public static (slice<byte>, error) DecryptOAEP(fips140.Hash hash, fips140.Hash mgfHash, ж<PrivateKey> Ꮡpriv, slice<byte> ciphertext, slice<byte> label) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    fipsSelfTest();
    fips140.RecordApproved();
    checkApprovedHash(hash);
    nint k = priv.pub.Size();
    if (len(ciphertext) > k || k < hash.Size() * 2 + 2) {
        return (default!, ErrDecryption);
    }
    var (em, err) = decrypt(ref (Ꮡpriv).DerefOrNull(), ciphertext, noCheck);
    if (err != default!) {
        return (default!, err);
    }
    hash.Reset();
    hash.Write(label);
    var lHash = hash.Sum(default!);
    nint firstByteIsZero = subtle.ConstantTimeByteEq(em[0], 0);
    var seed = em.slice(1, hash.Size() + 1);
    var db = em.slice(hash.Size() + 1);
    mgf1XOR(seed, mgfHash, db);
    mgf1XOR(db, mgfHash, seed);
    var lHash2 = db.slice(0, hash.Size());
    // We have to validate the plaintext in constant time in order to avoid
    // attacks like: J. Manger. A Chosen Ciphertext Attack on RSA Optimal
    // Asymmetric Encryption Padding (OAEP) as Standardized in PKCS #1
    // v2.0. In J. Kilian, editor, Advances in Cryptology.
    nint lHash2Good = subtle.ConstantTimeCompare(lHash, lHash2);
    // The remainder of the plaintext must be zero or more 0x00, followed
    // by 0x01, followed by the message.
    //   lookingForIndex: 1 iff we are still looking for the 0x01
    //   index: the offset of the first 0x01 byte
    //   invalid: 1 iff we saw a non-zero byte before the 0x01.
    nint lookingForIndex = default!;
    nint index = default!;
    nint invalid = default!;
    lookingForIndex = 1;
    var rest = db.slice(hash.Size());
    for (nint i = 0; i < len(rest); i++) {
        nint equals0 = subtle.ConstantTimeByteEq(rest[i], 0);
        nint equals1 = subtle.ConstantTimeByteEq(rest[i], 1);
        index = subtle.ConstantTimeSelect((nint)(lookingForIndex & equals1), i, index);
        lookingForIndex = subtle.ConstantTimeSelect(equals1, 0, lookingForIndex);
        invalid = subtle.ConstantTimeSelect((nint)(lookingForIndex & ~equals0), 1, invalid);
    }
    if ((nint)((nint)((nint)(firstByteIsZero & lHash2Good) & ~invalid) & ~lookingForIndex) != 1) {
        return (default!, ErrDecryption);
    }
    return (rest.slice(index + 1), default!);
}

} // end rsa_package
