// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ed25519 implements the Ed25519 signature algorithm. See
// https://ed25519.cr.yp.to/.
//
// These functions are also compatible with the “Ed25519” function defined in
// RFC 8032. However, unlike RFC 8032's formulation, this package's private key
// representation includes a public key suffix to make multiple signing
// operations with the same key more efficient. This package refers to the RFC
// 8032 private key as the “seed”.
//
// Operations involving private keys are implemented using constant-time
// algorithms.
namespace go.crypto;

using crypto = crypto_package;
using ed25519 = go.crypto.@internal.fips140.ed25519_package;
using fips140only = go.crypto.@internal.fips140only_package;
using cryptorand = go.crypto.rand_package;
using subtle = go.crypto.subtle_package;
using errors = errors_package;
using io = io_package;
using strconv = strconv_package;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class ed25519_package {

public static UntypedInt PublicKeySize => 32;
public static UntypedInt PrivateKeySize => 64;
public static UntypedInt SignatureSize => 64;
public static UntypedInt SeedSize => 32;

partial struct PublicKey /*[]byte*/;

// Any methods implemented on PublicKey might need to also be implemented on
// PrivateKey, as the latter embeds the former and will expose its methods.

// Equal reports whether pub and x have the same value.
public static bool Equal(this PublicKey pub, cryptoꓸPublicKey x) {
    var (xx, ok) = x._<PublicKey>(ᐧ);
    if (!ok) {
        return false;
    }
    return subtle.ConstantTimeCompare(pub, xx) == 1;
}

partial struct PrivateKey /*[]byte*/;

// Public returns the [PublicKey] corresponding to priv.
public static cryptoꓸPublicKey Public(this PrivateKey priv) {
    var publicKey = new slice<byte>(PublicKeySize);
    copy(publicKey, priv[32..]);
    return ((PublicKey)publicKey);
}

// Equal reports whether priv and x have the same value.
public static bool Equal(this PrivateKey priv, cryptoꓸPrivateKey x) {
    var (xx, ok) = x._<PrivateKey>(ᐧ);
    if (!ok) {
        return false;
    }
    return subtle.ConstantTimeCompare(priv, xx) == 1;
}

// Seed returns the private key seed corresponding to priv. It is provided for
// interoperability with RFC 8032. RFC 8032's private keys correspond to seeds
// in this package.
public static slice<byte> Seed(this PrivateKey priv) {
    return appendꓸꓸꓸ(new slice<byte>(0, SeedSize), priv[..(int)(SeedSize)]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoEd25519UseOfˢ = "crypto/ed25519: use of Ed25519ctx is not allowed in FIPS 140-only mode"u8;
internal static readonly @string ed25519ExpectedOptsˢ = "ed25519: expected opts.HashFunc() zero (unhashed message, for standard Ed25519) or SHA-512 (for Ed25519ph)"u8;

// Sign signs the given message with priv. rand is ignored and can be nil.
//
// If opts.HashFunc() is [crypto.SHA512], the pre-hashed variant Ed25519ph is used
// and message is expected to be a SHA-512 hash, otherwise opts.HashFunc() must
// be [crypto.Hash](0) and the message must not be hashed, as Ed25519 performs two
// passes over messages to be signed.
//
// A value of type [Options] can be used as opts, or crypto.Hash(0) or
// crypto.SHA512 directly to select plain Ed25519 or Ed25519ph, respectively.
public static (slice<byte> signature, error err) Sign(this PrivateKey priv, io.Reader rand, slice<byte> message, crypto.SignerOpts opts) {
    error err = default!;

    // NewPrivateKey is very slow in FIPS mode because it performs a
    // Sign+Verify cycle per FIPS 140-3 IG 10.3.A. We should find a way to cache
    // it or attach it to the PrivateKey.
    (var k, err) = ed25519.NewPrivateKey(priv);
    if (err != default!) {
        return (default!, err);
    }
    crypto.Hash hash = opts.HashFunc();
    @string context = ""u8;
    {
        var (optsΔ1, ok) = opts._<ж<Options>>(ᐧ); if (ok) {
            context = optsΔ1.Value.Context;
        }
    }
    switch (ᐧ) {
    case {} when hash == crypto.SHA512: {
        return ed25519.SignPH(k, // Ed25519ph
 message, context);
    }
    case {} when hash == ((crypto.Hash)0) && context != ""u8: {
        if (fips140only.Enabled) {
            // Ed25519ctx
            return (default!, errors.New(cryptoEd25519UseOfˢ));
        }
        return ed25519.SignCtx(k, message, context);
    }
    case {} when hash == ((crypto.Hash)0): {
        return (ed25519.Sign(k, // Ed25519
 message), default!);
    }
    default: {
        return (default!, errors.New(ed25519ExpectedOptsˢ));
    }}

}

// Options can be used with [PrivateKey.Sign] or [VerifyWithOptions]
// to select Ed25519 variants.
partial struct Options {
    // Hash can be zero for regular Ed25519, or crypto.SHA512 for Ed25519ph.
    public crypto.Hash Hash;
    // Context, if not empty, selects Ed25519ctx or provides the context string
    // for Ed25519ph. It can be at most 255 bytes in length.
    public @string Context;
}

// HashFunc returns o.Hash.
public static crypto.Hash HashFunc(this ref Options o) {
    return o.Hash;
}

// GenerateKey generates a public/private key pair using entropy from rand.
// If rand is nil, [crypto/rand.Reader] will be used.
//
// The output of this function is deterministic, and equivalent to reading
// [SeedSize] bytes from rand, and passing them to [NewKeyFromSeed].
public static (PublicKey, PrivateKey, error) GenerateKey(io.Reader rand) {
    if (rand == default!) {
        rand = cryptorand.Reader;
    }
    var seed = new slice<byte>(SeedSize);
    {
        var (_, err) = io.ReadFull(rand, seed); if (err != default!) {
            return (default!, default!, err);
        }
    }
    var privateKey = NewKeyFromSeed(seed);
    var publicKey = privateKey.Public()._<PublicKey>();
    return (publicKey, privateKey, default!);
}

// NewKeyFromSeed calculates a private key from a seed. It will panic if
// len(seed) is not [SeedSize]. This function is provided for interoperability
// with RFC 8032. RFC 8032's private keys correspond to seeds in this
// package.
public static PrivateKey NewKeyFromSeed(slice<byte> seed) {
    // Outline the function body so that the returned key can be stack-allocated.
    var privateKey = new slice<byte>(PrivateKeySize);
    newKeyFromSeed(privateKey, seed);
    return privateKey;
}

internal static void newKeyFromSeed(slice<byte> privateKey, slice<byte> seed) {
    var (k, err) = ed25519.NewPrivateKeyFromSeed(seed);
    if (err != default!) {
        // NewPrivateKeyFromSeed only returns an error if the seed length is incorrect.
        throw panic("ed25519: bad seed length: " + strconv.Itoa(len(seed)));
    }
    copy(privateKey, k.Bytes());
}

// Sign signs the message with privateKey and returns a signature. It will
// panic if len(privateKey) is not [PrivateKeySize].
public static slice<byte> Sign(PrivateKey privateKey, slice<byte> message) {
    // Outline the function body so that the returned signature can be
    // stack-allocated.
    var signature = new slice<byte>(SignatureSize);
    sign(signature, privateKey, message);
    return signature;
}

internal static void sign(slice<byte> signature, PrivateKey privateKey, slice<byte> message) {
    // NewPrivateKey is very slow in FIPS mode because it performs a
    // Sign+Verify cycle per FIPS 140-3 IG 10.3.A. We should find a way to cache
    // it or attach it to the PrivateKey.
    var (k, err) = ed25519.NewPrivateKey(privateKey);
    if (err != default!) {
        throw panic("ed25519: bad private key: " + err.Error());
    }
    var sig = ed25519.Sign(k, message);
    copy(signature, sig);
}

// Verify reports whether sig is a valid signature of message by publicKey. It
// will panic if len(publicKey) is not [PublicKeySize].
//
// The inputs are not considered confidential, and may leak through timing side
// channels, or if an attacker has control of part of the inputs.
public static bool Verify(PublicKey publicKey, slice<byte> message, slice<byte> sig) {
    return VerifyWithOptions(publicKey, message, sig, Ꮡ(new Options(Hash: ((crypto.Hash)0)))) == default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ed25519ExpectedOptsHashˢ = "ed25519: expected opts.Hash zero (unhashed message, for standard Ed25519) or SHA-512 (for Ed25519ph)"u8;

// VerifyWithOptions reports whether sig is a valid signature of message by
// publicKey. A valid signature is indicated by returning a nil error. It will
// panic if len(publicKey) is not [PublicKeySize].
//
// If opts.Hash is [crypto.SHA512], the pre-hashed variant Ed25519ph is used and
// message is expected to be a SHA-512 hash, otherwise opts.Hash must be
// [crypto.Hash](0) and the message must not be hashed, as Ed25519 performs two
// passes over messages to be signed.
//
// The inputs are not considered confidential, and may leak through timing side
// channels, or if an attacker has control of part of the inputs.
public static error VerifyWithOptions(PublicKey publicKey, slice<byte> message, slice<byte> sig, ж<Options> Ꮡopts) {
    ref var opts = ref Ꮡopts.DerefOrNull();

    {
        nint l = len(publicKey); if (l != PublicKeySize) {
            throw panic("ed25519: bad public key length: " + strconv.Itoa(l));
        }
    }
    var (k, err) = ed25519.NewPublicKey(publicKey);
    if (err != default!) {
        return err;
    }
    switch (ᐧ) {
    case {} when opts.Hash == crypto.SHA512: {
        return ed25519.VerifyPH(k, // Ed25519ph
 message, sig, opts.Context);
    }
    case {} when opts.Hash == ((crypto.Hash)0) && opts.Context != ""u8: {
        if (fips140only.Enabled) {
            // Ed25519ctx
            return errors.New(cryptoEd25519UseOfˢ);
        }
        return ed25519.VerifyCtx(k, message, sig, opts.Context);
    }
    case {} when opts.Hash == ((crypto.Hash)0): {
        return ed25519.Verify(k, // Ed25519
 message, sig);
    }
    default: {
        return errors.New(ed25519ExpectedOptsHashˢ);
    }}

}

} // end ed25519_package
