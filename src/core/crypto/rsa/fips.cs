// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using crypto = crypto_package;
using boring = go.crypto.@internal.boring_package;
using rsa = go.crypto.@internal.fips140.rsa_package;
using fips140hash = go.crypto.@internal.fips140hash_package;
using fips140only = go.crypto.@internal.fips140only_package;
using errors = errors_package;
using hash = hash_package;
using io = io_package;
using big = go.math.big_package;
using fips140 = go.crypto.@internal.fips140_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.math;

partial class rsa_package {

public static UntypedInt PSSSaltLengthAuto => 0;
public static UntypedInt PSSSaltLengthEqualsHash => -1;

// PSSOptions contains options for creating and verifying PSS signatures.
partial struct PSSOptions {
    // SaltLength controls the length of the salt used in the PSS signature. It
    // can either be a positive number of bytes, or one of the special
    // PSSSaltLength constants.
    public nint SaltLength;
    // Hash is the hash function used to generate the message digest. If not
    // zero, it overrides the hash function passed to SignPSS. It's required
    // when using PrivateKey.Sign.
    public crypto.Hash Hash;
}

// HashFunc returns opts.Hash so that [PSSOptions] implements [crypto.SignerOpts].
public static crypto.Hash HashFunc(this ref PSSOptions opts) {
    return opts.Hash;
}

internal static nint saltLength(this ж<PSSOptions> Ꮡopts) {
    ref var opts = ref Ꮡopts.DerefOrNull();

    if (Ꮡopts == nil) {
        return PSSSaltLengthAuto;
    }
    return opts.SaltLength;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRsaUseOfHashˢ = "crypto/rsa: use of hash functions other than SHA-2 or SHA-3 is not allowed in FIPS 140-only mode"u8;
internal static readonly @string cryptoRsaOnlyCryptoRandˢ = "crypto/rsa: only crypto/rand.Reader is allowed in FIPS 140-only mode"u8;
internal static readonly @string cryptoRsaUseOfPssSaltˢ = "crypto/rsa: use of PSS salt longer than the hash is not allowed in FIPS 140-only mode"u8;
internal static readonly @string cryptoRsaInvalidPssSaltˢ = "crypto/rsa: invalid PSS salt length"u8;

// SignPSS calculates the signature of digest using PSS.
//
// digest must be the result of hashing the input message using the given hash
// function. The opts argument may be nil, in which case sensible defaults are
// used. If opts.Hash is set, it overrides hash.
//
// The signature is randomized depending on the message, key, and salt size,
// using bytes from rand. Most applications should use [crypto/rand.Reader] as
// rand.
public static (slice<byte>, error) SignPSS(io.Reader rand, ж<PrivateKey> Ꮡpriv, crypto.Hash hash, slice<byte> digest, ж<PSSOptions> Ꮡopts) {
    ref var priv = ref Ꮡpriv.DerefOrNull();
    ref var opts = ref Ꮡopts.DerefOrNull();

    {
        var errΔ1 = checkPublicKeySize(ref nonnil(ref priv).PublicKey); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    if (Ꮡopts != nil && opts.Hash != 0) {
        hash = opts.Hash;
    }
    if (boring.Enabled && AreEqual(rand, boring.RandReader)) {
        var (bkey, errΔ2) = boringPrivateKey(Ꮡpriv);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        return boring.SignRSAPSS(bkey, hash, digest, Ꮡopts.saltLength());
    }
    boring.UnreachableExceptTests();
    var h = fips140hash.Unwrap(hash.New());
    {
        var errΔ3 = checkFIPS140OnlyPrivateKey(ref (Ꮡpriv).DerefOrNull()); if (errΔ3 != default!) {
            return (default!, errΔ3);
        }
    }
    if (fips140only.Enabled && !fips140only.ApprovedHash(h)) {
        return (default!, errors.New(cryptoRsaUseOfHashˢ));
    }
    if (fips140only.Enabled && !fips140only.ApprovedRandomReader(rand)) {
        return (default!, errors.New(cryptoRsaOnlyCryptoRandˢ));
    }
    var (k, err) = fipsPrivateKey(Ꮡpriv);
    if (err != default!) {
        return (default!, err);
    }
    nint saltLength = Ꮡopts.saltLength();
    if (fips140only.Enabled && saltLength > h.Size()) {
        return (default!, errors.New(cryptoRsaUseOfPssSaltˢ));
    }
    var exprᴛ1 = saltLength;
    if (exprᴛ1 == PSSSaltLengthAuto) {
        (saltLength, err) = rsa.PSSMaxSaltLength(k.PublicKey(), new hash_HashᴠHash(h));
        if (err != default!) {
            return (default!, fipsError(err));
        }
    }
    else if (exprᴛ1 == PSSSaltLengthEqualsHash) {
        saltLength = h.Size();
    }
    else { /* default: */
        if (saltLength <= 0) {
            // If we get here saltLength is either > 0 or < -1, in the
            // latter case we fail out.
            return (default!, errors.New(cryptoRsaInvalidPssSaltˢ));
        }
    }

    var (ᴛ1, ᴛ2) = rsa.SignPSS(rand, k, new hash_HashᴠHash(h), digest, saltLength);
    return fipsError2(ᴛ1, ᴛ2);
}

// VerifyPSS verifies a PSS signature.
//
// A valid signature is indicated by returning a nil error. digest must be the
// result of hashing the input message using the given hash function. The opts
// argument may be nil, in which case sensible defaults are used. opts.Hash is
// ignored.
//
// The inputs are not considered confidential, and may leak through timing side
// channels, or if an attacker has control of part of the inputs.
public static error VerifyPSS(ж<PublicKey> Ꮡpub, crypto.Hash hash, slice<byte> digest, slice<byte> sig, ж<PSSOptions> Ꮡopts) {
    {
        var errΔ1 = checkPublicKeySize(ref (Ꮡpub).DerefOrNull()); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    if (boring.Enabled) {
        var (bkey, errΔ2) = boringPublicKey(Ꮡpub);
        if (errΔ2 != default!) {
            return errΔ2;
        }
        {
            var errΔ3 = boring.VerifyRSAPSS(bkey, hash, digest, sig, Ꮡopts.saltLength()); if (errΔ3 != default!) {
                return ErrVerification;
            }
        }
        return default!;
    }
    var h = fips140hash.Unwrap(hash.New());
    {
        var errΔ4 = checkFIPS140OnlyPublicKey(ref (Ꮡpub).DerefOrNull()); if (errΔ4 != default!) {
            return errΔ4;
        }
    }
    if (fips140only.Enabled && !fips140only.ApprovedHash(h)) {
        return errors.New(cryptoRsaUseOfHashˢ);
    }
    var (k, err) = fipsPublicKey(ref (Ꮡpub).DerefOrNull());
    if (err != default!) {
        return err;
    }
    nint saltLength = Ꮡopts.saltLength();
    if (fips140only.Enabled && saltLength > h.Size()) {
        return errors.New(cryptoRsaUseOfPssSaltˢ);
    }
    var exprᴛ1 = saltLength;
    if (exprᴛ1 == PSSSaltLengthAuto) {
        return fipsError(rsa.VerifyPSS(k, new hash_HashᴠHash(h), digest, sig));
    }
    if (exprᴛ1 == PSSSaltLengthEqualsHash) {
        return fipsError(rsa.VerifyPSSWithSaltLength(k, new hash_HashᴠHash(h), digest, sig, h.Size()));
    }
    { /* default: */
        return fipsError(rsa.VerifyPSSWithSaltLength(k, new hash_HashᴠHash(h), digest, sig, saltLength));
    }

}

// EncryptOAEP encrypts the given message with RSA-OAEP.
//
// OAEP is parameterised by a hash function that is used as a random oracle.
// Encryption and decryption of a given message must use the same hash function
// and sha256.New() is a reasonable choice.
//
// The random parameter is used as a source of entropy to ensure that
// encrypting the same message twice doesn't result in the same ciphertext.
// Most applications should use [crypto/rand.Reader] as random.
//
// The label parameter may contain arbitrary data that will not be encrypted,
// but which gives important context to the message. For example, if a given
// public key is used to encrypt two types of messages then distinct label
// values could be used to ensure that a ciphertext for one purpose cannot be
// used for another by an attacker. If not required it can be empty.
//
// The message must be no longer than the length of the public modulus minus
// twice the hash length, minus a further 2.
public static (slice<byte>, error) EncryptOAEP(hash.Hash hashΔ1, io.Reader random, ж<PublicKey> Ꮡpub, slice<byte> msg, slice<byte> label) {
    GoFrame ᒐ = default;
    try {
        ref var pub = ref Ꮡpub.DerefOrNull();

        {
            var errΔ1 = checkPublicKeySize(ref (Ꮡpub).DerefOrNull()); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        defer(hashΔ1.Reset, ref ᒐ);
        if (boring.Enabled && AreEqual(random, boring.RandReader)) {
            hashΔ1.Reset();
            nint kΔ1 = pub.Size();
            if (len(msg) > kΔ1 - 2 * hashΔ1.Size() - 2) {
                return (default!, ErrMessageTooLong);
            }
            var (bkey, errΔ2) = boringPublicKey(Ꮡpub);
            if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
            return boring.EncryptRSAOAEP(hashΔ1, hashΔ1, bkey, msg, label);
        }
        boring.UnreachableExceptTests();
        hashΔ1 = fips140hash.Unwrap(hashΔ1);
        {
            var errΔ3 = checkFIPS140OnlyPublicKey(ref (Ꮡpub).DerefOrNull()); if (errΔ3 != default!) {
                return (default!, errΔ3);
            }
        }
        if (fips140only.Enabled && !fips140only.ApprovedHash(hashΔ1)) {
            return (default!, errors.New(cryptoRsaUseOfHashˢ));
        }
        if (fips140only.Enabled && !fips140only.ApprovedRandomReader(random)) {
            return (default!, errors.New(cryptoRsaOnlyCryptoRandˢ));
        }
        var (k, err) = fipsPublicKey(ref (Ꮡpub).DerefOrNull());
        if (err != default!) {
            return (default!, err);
        }
        var (ᴛ3, ᴛ4) = rsa.EncryptOAEP(new hash_HashᴠHash(hashΔ1), new hash_HashᴠHash(hashΔ1), random, k, msg, label);
        return fipsError2(ᴛ3, ᴛ4);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// DecryptOAEP decrypts ciphertext using RSA-OAEP.
//
// OAEP is parameterised by a hash function that is used as a random oracle.
// Encryption and decryption of a given message must use the same hash function
// and sha256.New() is a reasonable choice.
//
// The random parameter is legacy and ignored, and it can be nil.
//
// The label parameter must match the value given when encrypting. See
// [EncryptOAEP] for details.
public static (slice<byte>, error) DecryptOAEP(hash.Hash hashΔ1, io.Reader random, ж<PrivateKey> Ꮡpriv, slice<byte> ciphertext, slice<byte> label) {
    GoFrame ᒐ = default;
    try {
        defer(hashΔ1.Reset, ref ᒐ);
        return decryptOAEP(hashΔ1, hashΔ1, Ꮡpriv, ciphertext, label);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (slice<byte>, error) decryptOAEP(hash.Hash hashΔ1, hash.Hash mgfHash, ж<PrivateKey> Ꮡpriv, slice<byte> ciphertext, slice<byte> label) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    {
        var errΔ1 = checkPublicKeySize(ref nonnil(ref priv).PublicKey); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    if (boring.Enabled) {
        nint kΔ1 = Ꮡpriv.of(PrivateKey.ᏑPublicKey).Size();
        if (len(ciphertext) > kΔ1 || kΔ1 < hashΔ1.Size() * 2 + 2) {
            return (default!, ErrDecryption);
        }
        var (bkey, errΔ2) = boringPrivateKey(Ꮡpriv);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        (var @out, errΔ2) = boring.DecryptRSAOAEP(hashΔ1, mgfHash, bkey, ciphertext, label);
        if (errΔ2 != default!) {
            return (default!, ErrDecryption);
        }
        return (@out, default!);
    }
    hashΔ1 = fips140hash.Unwrap(hashΔ1);
    mgfHash = fips140hash.Unwrap(mgfHash);
    {
        var errΔ3 = checkFIPS140OnlyPrivateKey(ref (Ꮡpriv).DerefOrNull()); if (errΔ3 != default!) {
            return (default!, errΔ3);
        }
    }
    if (fips140only.Enabled) {
        if (!fips140only.ApprovedHash(hashΔ1) || !fips140only.ApprovedHash(mgfHash)) {
            return (default!, errors.New(cryptoRsaUseOfHashˢ));
        }
    }
    var (k, err) = fipsPrivateKey(Ꮡpriv);
    if (err != default!) {
        return (default!, err);
    }
    var (ᴛ5, ᴛ6) = rsa.DecryptOAEP(new hash_HashᴠHash(hashΔ1), new hash_HashᴠHash(mgfHash), k, ciphertext, label);
    return fipsError2(ᴛ5, ᴛ6);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRsaInputMustBeˢ = "crypto/rsa: input must be hashed message"u8;

// SignPKCS1v15 calculates the signature of hashed using
// RSASSA-PKCS1-V1_5-SIGN from RSA PKCS #1 v1.5.  Note that hashed must
// be the result of hashing the input message using the given hash
// function. If hash is zero, hashed is signed directly. This isn't
// advisable except for interoperability.
//
// The random parameter is legacy and ignored, and it can be nil.
//
// This function is deterministic. Thus, if the set of possible
// messages is small, an attacker may be able to build a map from
// messages to signatures and identify the signed messages. As ever,
// signatures provide authenticity, not confidentiality.
public static (slice<byte>, error) SignPKCS1v15(io.Reader random, ж<PrivateKey> Ꮡpriv, crypto.Hash hash, slice<byte> hashed) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    @string hashName = default!;
    if (hash != ((crypto.Hash)0)) {
        if (len(hashed) != hash.Size()) {
            return (default!, errors.New(cryptoRsaInputMustBeˢ));
        }
        hashName = hash.String();
    }
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
        return boring.SignRSAPKCS1v15(bkey, hash, hashed);
    }
    {
        var errΔ3 = checkFIPS140OnlyPrivateKey(ref (Ꮡpriv).DerefOrNull()); if (errΔ3 != default!) {
            return (default!, errΔ3);
        }
    }
    if (fips140only.Enabled && !fips140only.ApprovedHash(fips140hash.Unwrap(hash.New()))) {
        return (default!, errors.New(cryptoRsaUseOfHashˢ));
    }
    var (k, err) = fipsPrivateKey(Ꮡpriv);
    if (err != default!) {
        return (default!, err);
    }
    var (ᴛ7, ᴛ8) = rsa.SignPKCS1v15(k, hashName, hashed);
    return fipsError2(ᴛ7, ᴛ8);
}

// VerifyPKCS1v15 verifies an RSA PKCS #1 v1.5 signature.
// hashed is the result of hashing the input message using the given hash
// function and sig is the signature. A valid signature is indicated by
// returning a nil error. If hash is zero then hashed is used directly. This
// isn't advisable except for interoperability.
//
// The inputs are not considered confidential, and may leak through timing side
// channels, or if an attacker has control of part of the inputs.
public static error VerifyPKCS1v15(ж<PublicKey> Ꮡpub, crypto.Hash hash, slice<byte> hashed, slice<byte> sig) {
    @string hashName = default!;
    if (hash != ((crypto.Hash)0)) {
        if (len(hashed) != hash.Size()) {
            return errors.New(cryptoRsaInputMustBeˢ);
        }
        hashName = hash.String();
    }
    {
        var errΔ1 = checkPublicKeySize(ref (Ꮡpub).DerefOrNull()); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    if (boring.Enabled) {
        var (bkey, errΔ2) = boringPublicKey(Ꮡpub);
        if (errΔ2 != default!) {
            return errΔ2;
        }
        {
            var errΔ3 = boring.VerifyRSAPKCS1v15(bkey, hash, hashed, sig); if (errΔ3 != default!) {
                return ErrVerification;
            }
        }
        return default!;
    }
    {
        var errΔ4 = checkFIPS140OnlyPublicKey(ref (Ꮡpub).DerefOrNull()); if (errΔ4 != default!) {
            return errΔ4;
        }
    }
    if (fips140only.Enabled && !fips140only.ApprovedHash(fips140hash.Unwrap(hash.New()))) {
        return errors.New(cryptoRsaUseOfHashˢ);
    }
    var (k, err) = fipsPublicKey(ref (Ꮡpub).DerefOrNull());
    if (err != default!) {
        return err;
    }
    return fipsError(rsa.VerifyPKCS1v15(k, hashName, hashed, sig));
}

internal static error fipsError(error err) {
    var exprᴛ1 = err;
    if (AreEqual(exprᴛ1, rsa.ErrDecryption)) {
        return ErrDecryption;
    }
    if (AreEqual(exprᴛ1, rsa.ErrVerification)) {
        return ErrVerification;
    }
    if (AreEqual(exprᴛ1, rsa.ErrMessageTooLong)) {
        return ErrMessageTooLong;
    }

    return err;
}

internal static (T, error) fipsError2<T>(T x, error err) {
    return (x, fipsError(err));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRsaPublicKeyˢ = "crypto/rsa: public key missing N"u8;
internal static readonly @string cryptoRsaUseOfKeysˢ = "crypto/rsa: use of keys smaller than 2048 bits is not allowed in FIPS 140-only mode"u8;
internal static readonly @string cryptoRsaUseOfKeysWithˢ = "crypto/rsa: use of keys with odd size is not allowed in FIPS 140-only mode"u8;
internal static readonly @string cryptoRsaUseOfPublicˢ = "crypto/rsa: use of public exponent <= 2¹⁶ is not allowed in FIPS 140-only mode"u8;
internal static readonly @string cryptoRsaUseOfEvenPublicˢ = "crypto/rsa: use of even public exponent is not allowed in FIPS 140-only mode"u8;

internal static error checkFIPS140OnlyPublicKey(ref PublicKey pub) {
    if (!fips140only.Enabled) {
        return default!;
    }
    if (pub.N == nil) {
        return errors.New(cryptoRsaPublicKeyˢ);
    }
    if (pub.N.BitLen() < 2048) {
        return errors.New(cryptoRsaUseOfKeysˢ);
    }
    if (pub.N.BitLen() % 2 == 1) {
        return errors.New(cryptoRsaUseOfKeysWithˢ);
    }
    if (pub.E <= (1 << (int)(16))) {
        return errors.New(cryptoRsaUseOfPublicˢ);
    }
    if ((nint)(pub.E & 1) == 0) {
        return errors.New(cryptoRsaUseOfEvenPublicˢ);
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRsaUseOfMultiPrimeˢ = "crypto/rsa: use of multi-prime keys is not allowed in FIPS 140-only mode"u8;
internal static readonly @string cryptoRsaUseOfPrimesOfˢ = "crypto/rsa: use of primes of different sizes is not allowed in FIPS 140-only mode"u8;

internal static error checkFIPS140OnlyPrivateKey(ref PrivateKey priv) {
    if (!fips140only.Enabled) {
        return default!;
    }
    {
        var err = checkFIPS140OnlyPublicKey(ref nonnil(ref priv).PublicKey); if (err != default!) {
            return err;
        }
    }
    if (len(priv.Primes) != 2) {
        return errors.New(cryptoRsaUseOfMultiPrimeˢ);
    }
    if (priv.Primes[0] == nil || priv.Primes[1] == nil || priv.Primes[0].BitLen() != priv.Primes[1].BitLen()) {
        return errors.New(cryptoRsaUseOfPrimesOfˢ);
    }
    return default!;
}

} // end rsa_package
