// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using boring = go.crypto.@internal.boring_package;
using rsa = go.crypto.@internal.fips140.rsa_package;
using fips140only = go.crypto.@internal.fips140only_package;
using randutil = go.crypto.@internal.randutil_package;
using subtle = go.crypto.subtle_package;
using errors = errors_package;
using io = io_package;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class rsa_package {

// This file implements encryption and decryption using PKCS #1 v1.5 padding.

// PKCS1v15DecryptOptions is for passing options to PKCS #1 v1.5 decryption using
// the [crypto.Decrypter] interface.
[GoType] partial struct PKCS1v15DecryptOptions {
    // SessionKeyLen is the length of the session key that is being
    // decrypted. If not zero, then a padding error during decryption will
    // cause a random plaintext of this length to be returned rather than
    // an error. These alternatives happen in constant time.
    public nint SessionKeyLen;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRsaUseOfPkcs1V15ˢ = "crypto/rsa: use of PKCS#1 v1.5 encryption is not allowed in FIPS 140-only mode"u8;

// EncryptPKCS1v15 encrypts the given message with RSA and the padding
// scheme from PKCS #1 v1.5.  The message must be no longer than the
// length of the public modulus minus 11 bytes.
//
// The random parameter is used as a source of entropy to ensure that
// encrypting the same message twice doesn't result in the same
// ciphertext. Most applications should use [crypto/rand.Reader]
// as random. Note that the returned ciphertext does not depend
// deterministically on the bytes read from random, and may change
// between calls and/or between versions.
//
// WARNING: use of this function to encrypt plaintexts other than
// session keys is dangerous. Use RSA OAEP in new protocols.
public static (slice<byte>, error) EncryptPKCS1v15(io.Reader random, ж<PublicKey> Ꮡpub, slice<byte> msg) {
    ref var pub = ref Ꮡpub.DerefOrNull();

    if (fips140only.Enabled) {
        return (default!, errors.New(cryptoRsaUseOfPkcs1V15ˢ));
    }
    {
        var errΔ1 = checkPublicKeySize(ref (Ꮡpub).DerefOrNull()); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    randutil.MaybeReadByte(random);
    nint k = pub.Size();
    if (len(msg) > k - 11) {
        return (default!, ErrMessageTooLong);
    }
    if (boring.Enabled && AreEqual(random, boring.RandReader)) {
        var (bkey, errΔ2) = boringPublicKey(Ꮡpub);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        return boring.EncryptRSAPKCS1(bkey, msg);
    }
    boring.UnreachableExceptTests();
    // EM = 0x00 || 0x02 || PS || 0x00 || M
    var em = new slice<byte>(k);
    em[1] = 2;
    var (ps, mm) = (em.slice(2, len(em) - len(msg) - 1), em.slice(len(em) - len(msg)));
    var err = nonZeroRandomBytes(ps, random);
    if (err != default!) {
        return (default!, err);
    }
    em[len(em) - len(msg) - 1] = 0;
    copy(mm, msg);
    if (boring.Enabled) {
        ж<boring.PublicKeyRSA> bkey = default!;
        (bkey, err) = boringPublicKey(Ꮡpub);
        if (err != default!) {
            return (default!, err);
        }
        return boring.EncryptRSANoPadding(bkey, em);
    }
    (var fk, err) = fipsPublicKey(ref (Ꮡpub).DerefOrNull());
    if (err != default!) {
        return (default!, err);
    }
    return rsa.Encrypt(fk, em);
}

// DecryptPKCS1v15 decrypts a plaintext using RSA and the padding scheme from PKCS #1 v1.5.
// The random parameter is legacy and ignored, and it can be nil.
//
// Note that whether this function returns an error or not discloses secret
// information. If an attacker can cause this function to run repeatedly and
// learn whether each instance returned an error then they can decrypt and
// forge signatures as if they had the private key. See
// DecryptPKCS1v15SessionKey for a way of solving this problem.
public static (slice<byte>, error) DecryptPKCS1v15(io.Reader random, ж<PrivateKey> Ꮡpriv, slice<byte> ciphertext) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    {
        var errΔ1 = checkPublicKeySize(ref nonnil(ref priv).PublicKey); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    if (boring.Enabled) {
        var (bkey, errΔ2) = boringPrivateKey(Ꮡpriv);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        (var outΔ1, errΔ2) = boring.DecryptRSAPKCS1(bkey, ciphertext);
        if (errΔ2 != default!) {
            return (default!, ErrDecryption);
        }
        return (outΔ1, default!);
    }
    var (valid, @out, index, err) = decryptPKCS1v15(Ꮡpriv, ciphertext);
    if (err != default!) {
        return (default!, err);
    }
    if (valid == 0) {
        return (default!, ErrDecryption);
    }
    return (@out.slice(index), default!);
}

// DecryptPKCS1v15SessionKey decrypts a session key using RSA and the padding
// scheme from PKCS #1 v1.5. The random parameter is legacy and ignored, and it
// can be nil.
//
// DecryptPKCS1v15SessionKey returns an error if the ciphertext is the wrong
// length or if the ciphertext is greater than the public modulus. Otherwise, no
// error is returned. If the padding is valid, the resulting plaintext message
// is copied into key. Otherwise, key is unchanged. These alternatives occur in
// constant time. It is intended that the user of this function generate a
// random session key beforehand and continue the protocol with the resulting
// value.
//
// Note that if the session key is too small then it may be possible for an
// attacker to brute-force it. If they can do that then they can learn whether a
// random value was used (because it'll be different for the same ciphertext)
// and thus whether the padding was correct. This also defeats the point of this
// function. Using at least a 16-byte key will protect against this attack.
//
// This method implements protections against Bleichenbacher chosen ciphertext
// attacks [0] described in RFC 3218 Section 2.3.2 [1]. While these protections
// make a Bleichenbacher attack significantly more difficult, the protections
// are only effective if the rest of the protocol which uses
// DecryptPKCS1v15SessionKey is designed with these considerations in mind. In
// particular, if any subsequent operations which use the decrypted session key
// leak any information about the key (e.g. whether it is a static or random
// key) then the mitigations are defeated. This method must be used extremely
// carefully, and typically should only be used when absolutely necessary for
// compatibility with an existing protocol (such as TLS) that is designed with
// these properties in mind.
//
//   - [0] “Chosen Ciphertext Attacks Against Protocols Based on the RSA Encryption
//     Standard PKCS #1”, Daniel Bleichenbacher, Advances in Cryptology (Crypto '98)
//   - [1] RFC 3218, Preventing the Million Message Attack on CMS,
//     https://www.rfc-editor.org/rfc/rfc3218.html
public static error DecryptPKCS1v15SessionKey(io.Reader random, ж<PrivateKey> Ꮡpriv, slice<byte> ciphertext, slice<byte> key) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    {
        var errΔ1 = checkPublicKeySize(ref nonnil(ref priv).PublicKey); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    nint k = Ꮡpriv.of(PrivateKey.ᏑPublicKey).Size();
    if (k - (len(key) + 3 + 8) < 0) {
        return ErrDecryption;
    }
    var (valid, em, index, err) = decryptPKCS1v15(Ꮡpriv, ciphertext);
    if (err != default!) {
        return err;
    }
    if (len(em) != k) {
        // This should be impossible because decryptPKCS1v15 always
        // returns the full slice.
        return ErrDecryption;
    }
    valid &= (nint)(subtle.ConstantTimeEq((int32)(len(em) - index), (int32)len(key)));
    subtle.ConstantTimeCopy(valid, key, em.slice(len(em) - len(key)));
    return default!;
}

// decryptPKCS1v15 decrypts ciphertext using priv. It returns one or zero in
// valid that indicates whether the plaintext was correctly structured.
// In either case, the plaintext is returned in em so that it may be read
// independently of whether it was valid in order to maintain constant memory
// access patterns. If the plaintext was valid then index contains the index of
// the original message in em, to allow constant time padding removal.
internal static (nint valid, slice<byte> em, nint index, error err) decryptPKCS1v15(ж<PrivateKey> Ꮡpriv, slice<byte> ciphertext) {
    nint valid = default!;
    slice<byte> em = default!;
    nint index = default!;
    error err = default!;

    if (fips140only.Enabled) {
        return (0, default!, 0, errors.New(cryptoRsaUseOfPkcs1V15ˢ));
    }
    nint k = Ꮡpriv.of(PrivateKey.ᏑPublicKey).Size();
    if (k < 11) {
        err = ErrDecryption;
        return (0, default!, 0, err);
    }
    if (boring.Enabled){
        ж<boring.PrivateKeyRSA> bkey = default!;
        (bkey, err) = boringPrivateKey(Ꮡpriv);
        if (err != default!) {
            return (0, default!, 0, err);
        }
        (em, err) = boring.DecryptRSANoPadding(bkey, ciphertext);
        if (err != default!) {
            return (0, default!, 0, ErrDecryption);
        }
    } else {
        var (fk, errΔ1) = fipsPrivateKey(Ꮡpriv);
        if (errΔ1 != default!) {
            return (0, default!, 0, errΔ1);
        }
        (em, errΔ1) = rsa.DecryptWithoutCheck(fk, ciphertext);
        if (errΔ1 != default!) {
            return (0, default!, 0, ErrDecryption);
        }
    }
    nint firstByteIsZero = subtle.ConstantTimeByteEq(em[0], 0);
    nint secondByteIsTwo = subtle.ConstantTimeByteEq(em[1], 2);
    // The remainder of the plaintext must be a string of non-zero random
    // octets, followed by a 0, followed by the message.
    //   lookingForIndex: 1 iff we are still looking for the zero.
    //   index: the offset of the first zero byte.
    nint lookingForIndex = 1;
    for (nint i = 2; i < len(em); i++) {
        nint equals0 = subtle.ConstantTimeByteEq(em[i], 0);
        index = subtle.ConstantTimeSelect((nint)(lookingForIndex & equals0), i, index);
        lookingForIndex = subtle.ConstantTimeSelect(equals0, 0, lookingForIndex);
    }
    // The PS padding must be at least 8 bytes long, and it starts two
    // bytes into em.
    nint validPS = subtle.ConstantTimeLessOrEq(2 + 8, index);
    valid = (nint)((nint)((nint)(firstByteIsZero & secondByteIsTwo) & ((nint)(~lookingForIndex & 1))) & validPS);
    index = subtle.ConstantTimeSelect(valid, index + 1, 0);
    return (valid, em, index, default!);
}

// nonZeroRandomBytes fills the given slice with non-zero random octets.
internal static error /*err*/ nonZeroRandomBytes(slice<byte> s, io.Reader random) {
    error err = default!;

    (_, err) = io.ReadFull(random, s);
    if (err != default!) {
        return err;
    }
    for (nint i = 0; i < len(s); i++) {
        while (s[i] == 0) {
            (_, err) = io.ReadFull(random, s.slice(i, i + 1));
            if (err != default!) {
                return err;
            }
            // In tests, the PRNG may return all zeros so we do
            // this to break the loop.
            s[i] ^= (byte)(0x42);
        }
    }
    return err;
}

} // end rsa_package
