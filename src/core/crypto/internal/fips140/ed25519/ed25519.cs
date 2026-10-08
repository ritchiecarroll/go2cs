// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using Δedwards25519 = go.crypto.@internal.fips140.edwards25519_package;
using sha512 = go.crypto.@internal.fips140.sha512_package;
using errors = errors_package;
using strconv = strconv_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class ed25519_package {

// See https://blog.mozilla.org/warner/2011/11/29/ed25519-keys/ for the
// components of the keys and the moving parts of the algorithm.
internal static UntypedInt seedSize => 32;
internal static UntypedInt publicKeySize => 32;
internal static UntypedInt privateKeySize => /* seedSize + publicKeySize */ 64;
internal static UntypedInt signatureSize => 64;
internal static UntypedInt sha512Size => 64;

partial struct PrivateKey {
    internal array<byte> seed = new(seedSize);
    internal array<byte> pub = new(publicKeySize);
    internal Δedwards25519.Scalar s;
    internal array<byte> prefix = new(sha512Size / 2);
}

public static slice<byte> Bytes(this ref PrivateKey priv) {
    var k = new slice<byte>(0, privateKeySize);
    k = appendꓸꓸꓸ(k, priv.seed[..]);
    k = appendꓸꓸꓸ(k, priv.pub[..]);
    return k;
}

public static slice<byte> Seed(this ref PrivateKey priv) {
    var seed = priv.seed.Clone();
    return seed[..];
}

public static slice<byte> PublicKey(this ref PrivateKey priv) {
    var pub = priv.pub.Clone();
    return pub[..];
}

partial struct ΔPublicKey {
    internal Δedwards25519.Point a;
    internal array<byte> aBytes = new(32);
}

public static slice<byte> Bytes(this ref ΔPublicKey pub) {
    var a = pub.aBytes.Clone();
    return a[..];
}

// GenerateKey generates a new Ed25519 private key pair.
public static (ж<PrivateKey>, error) GenerateKey() {
    var priv = Ꮡ(new PrivateKey(nil));
    return generateKey(priv);
}

internal static (ж<PrivateKey>, error) generateKey(ж<PrivateKey> Ꮡpriv) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    fips140.RecordApproved();
    drbg.Read(priv.seed[..]);
    precomputePrivateKey(Ꮡpriv);
    fipsPCT(Ꮡpriv);
    return (Ꮡpriv, default!);
}

public static (ж<PrivateKey>, error) NewPrivateKeyFromSeed(slice<byte> seed) {
    var priv = Ꮡ(new PrivateKey(nil));
    return newPrivateKeyFromSeed(priv, seed);
}

internal static (ж<PrivateKey>, error) newPrivateKeyFromSeed(ж<PrivateKey> Ꮡpriv, slice<byte> seed) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    fips140.RecordApproved();
    {
        nint l = len(seed); if (l != seedSize) {
            return (default!, errors.New("ed25519: bad seed length: "u8 + strconv.Itoa(l)));
        }
    }
    copy(priv.seed[..], seed);
    precomputePrivateKey(Ꮡpriv);
    return (Ꮡpriv, default!);
}

internal static void precomputePrivateKey(ж<PrivateKey> Ꮡpriv) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    var hs = sha512.New();
    hs.Write(priv.seed[..]);
    var h = hs.Sum(new slice<byte>(0, sha512Size));
    var (s, err) = Ꮡpriv.of(PrivateKey.Ꮡs).SetBytesWithClamping(h[..32]);
    if (err != default!) {
        throw panic("ed25519: internal error: setting scalar failed");
    }
    var A = (Ꮡ(new Δedwards25519.Point(nil))).ScalarBaseMult(s);
    copy(priv.pub[..], A.Bytes());
    copy(priv.prefix[..], h[32..]);
}

public static (ж<PrivateKey>, error) NewPrivateKey(slice<byte> priv) {
    var p = Ꮡ(new PrivateKey(nil));
    return newPrivateKey(p, priv);
}

internal static (ж<PrivateKey>, error) newPrivateKey(ж<PrivateKey> Ꮡpriv, slice<byte> privBytes) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    fips140.RecordApproved();
    {
        nint l = len(privBytes); if (l != privateKeySize) {
            return (default!, errors.New("ed25519: bad private key length: "u8 + strconv.Itoa(l)));
        }
    }
    copy(priv.seed[..], privBytes[..32]);
    var hs = sha512.New();
    hs.Write(priv.seed[..]);
    var h = hs.Sum(new slice<byte>(0, sha512Size));
    {
        var (_, err) = Ꮡpriv.of(PrivateKey.Ꮡs).SetBytesWithClamping(h[..32]); if (err != default!) {
            throw panic("ed25519: internal error: setting scalar failed");
        }
    }
    // Note that we are not decompressing the public key point here,
    // because it takes > 20% of the time of a signature generation.
    // Signing doesn't use it as a point anyway.
    copy(priv.pub[..], privBytes[32..]);
    copy(priv.prefix[..], h[32..]);
    return (Ꮡpriv, default!);
}

public static (ж<ΔPublicKey>, error) NewPublicKey(slice<byte> pub) {
    var p = Ꮡ(new ΔPublicKey(nil));
    return newPublicKey(p, pub);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string ed25519BadPublicKeyˢ = "ed25519: bad public key"u8;

internal static (ж<ΔPublicKey>, error) newPublicKey(ж<ΔPublicKey> Ꮡpub, slice<byte> pubBytes) {
    ref var pub = ref Ꮡpub.DerefOrNull();

    {
        nint l = len(pubBytes); if (l != publicKeySize) {
            return (default!, errors.New("ed25519: bad public key length: "u8 + strconv.Itoa(l)));
        }
    }
    // SetBytes checks that the point is on the curve.
    {
        var (_, err) = Ꮡpub.of(ed25519_package.ΔPublicKey.Ꮡa).SetBytes(pubBytes); if (err != default!) {
            return (default!, errors.New(ed25519BadPublicKeyˢ));
        }
    }
    copy(pub.aBytes[..], pubBytes);
    return (Ꮡpub, default!);
}

// Domain separation prefixes used to disambiguate Ed25519/Ed25519ph/Ed25519ctx.
// See RFC 8032, Section 2 and Section 5.1.
internal static readonly @string domPrefixPure = ""u8;

internal static readonly @string domPrefixPh = "SigEd25519 no Ed25519 collisions\x01"u8;

internal static readonly @string domPrefixCtx = "SigEd25519 no Ed25519 collisions\x00"u8;

public static slice<byte> Sign(ж<PrivateKey> Ꮡpriv, slice<byte> message) {
    // Outline the function body so that the returned signature can be
    // stack-allocated.
    var signature = new slice<byte>(signatureSize);
    return sign(signature, Ꮡpriv, message);
}

internal static slice<byte> sign(slice<byte> signature, ж<PrivateKey> Ꮡpriv, slice<byte> message) {
    fipsSelfTest();
    fips140.RecordApproved();
    return signWithDom(signature, Ꮡpriv, message, domPrefixPure, ""u8);
}

public static (slice<byte>, error) SignPH(ж<PrivateKey> Ꮡpriv, slice<byte> message, @string context) {
    // Outline the function body so that the returned signature can be
    // stack-allocated.
    var signature = new slice<byte>(signatureSize);
    return signPH(signature, Ꮡpriv, message, context);
}

internal static (slice<byte>, error) signPH(slice<byte> signature, ж<PrivateKey> Ꮡpriv, slice<byte> message, @string context) {
    fipsSelfTest();
    fips140.RecordApproved();
    {
        nint l = len(message); if (l != sha512Size) {
            return (default!, errors.New("ed25519: bad Ed25519ph message hash length: "u8 + strconv.Itoa(l)));
        }
    }
    {
        nint l = len(context); if (l > 255) {
            return (default!, errors.New("ed25519: bad Ed25519ph context length: "u8 + strconv.Itoa(l)));
        }
    }
    return (signWithDom(signature, Ꮡpriv, message, domPrefixPh, context), default!);
}

public static (slice<byte>, error) SignCtx(ж<PrivateKey> Ꮡpriv, slice<byte> message, @string context) {
    // Outline the function body so that the returned signature can be
    // stack-allocated.
    var signature = new slice<byte>(signatureSize);
    return signCtx(signature, Ꮡpriv, message, context);
}

internal static (slice<byte>, error) signCtx(slice<byte> signature, ж<PrivateKey> Ꮡpriv, slice<byte> message, @string context) {
    fipsSelfTest();
    // FIPS 186-5 specifies Ed25519 and Ed25519ph (with context), but not Ed25519ctx.
    fips140.RecordNonApproved();
    // Note that per RFC 8032, Section 5.1, the context SHOULD NOT be empty.
    {
        nint l = len(context); if (l > 255) {
            return (default!, errors.New("ed25519: bad Ed25519ctx context length: "u8 + strconv.Itoa(l)));
        }
    }
    return (signWithDom(signature, Ꮡpriv, message, domPrefixCtx, context), default!);
}

internal static slice<byte> signWithDom(slice<byte> signature, ж<PrivateKey> Ꮡpriv, slice<byte> message, @string domPrefix, @string context) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    var mh = sha512.New();
    if (domPrefix != domPrefixPure) {
        mh.Write(slice<byte>(domPrefix));
        mh.Write(new byte[]{(byte)len(context)}.slice());
        mh.Write(slice<byte>(context));
    }
    mh.Write(priv.prefix[..]);
    mh.Write(message);
    var messageDigest = new slice<byte>(0, sha512Size);
    messageDigest = mh.Sum(messageDigest);
    var (r, err) = Δedwards25519.NewScalar().SetUniformBytes(messageDigest);
    if (err != default!) {
        throw panic("ed25519: internal error: setting scalar failed");
    }
    var R = (Ꮡ(new Δedwards25519.Point(nil))).ScalarBaseMult(r);
    var kh = sha512.New();
    if (domPrefix != domPrefixPure) {
        kh.Write(slice<byte>(domPrefix));
        kh.Write(new byte[]{(byte)len(context)}.slice());
        kh.Write(slice<byte>(context));
    }
    kh.Write(R.Bytes());
    kh.Write(priv.pub[..]);
    kh.Write(message);
    var hramDigest = new slice<byte>(0, sha512Size);
    hramDigest = kh.Sum(hramDigest);
    (var k, err) = Δedwards25519.NewScalar().SetUniformBytes(hramDigest);
    if (err != default!) {
        throw panic("ed25519: internal error: setting scalar failed");
    }
    var S = Δedwards25519.NewScalar().MultiplyAdd(k, Ꮡpriv.of(PrivateKey.Ꮡs), r);
    copy(signature[..32], R.Bytes());
    copy(signature[32..], S.Bytes());
    return signature;
}

public static error Verify(ж<ΔPublicKey> Ꮡpub, slice<byte> message, slice<byte> sig) {
    return verify(Ꮡpub, message, sig);
}

internal static error verify(ж<ΔPublicKey> Ꮡpub, slice<byte> message, slice<byte> sig) {
    fipsSelfTest();
    fips140.RecordApproved();
    return verifyWithDom(Ꮡpub, message, sig, domPrefixPure, ""u8);
}

public static error VerifyPH(ж<ΔPublicKey> Ꮡpub, slice<byte> message, slice<byte> sig, @string context) {
    fipsSelfTest();
    fips140.RecordApproved();
    {
        nint l = len(message); if (l != sha512Size) {
            return errors.New("ed25519: bad Ed25519ph message hash length: "u8 + strconv.Itoa(l));
        }
    }
    {
        nint l = len(context); if (l > 255) {
            return errors.New("ed25519: bad Ed25519ph context length: "u8 + strconv.Itoa(l));
        }
    }
    return verifyWithDom(Ꮡpub, message, sig, domPrefixPh, context);
}

public static error VerifyCtx(ж<ΔPublicKey> Ꮡpub, slice<byte> message, slice<byte> sig, @string context) {
    fipsSelfTest();
    // FIPS 186-5 specifies Ed25519 and Ed25519ph (with context), but not Ed25519ctx.
    fips140.RecordNonApproved();
    {
        nint l = len(context); if (l > 255) {
            return errors.New("ed25519: bad Ed25519ctx context length: "u8 + strconv.Itoa(l));
        }
    }
    return verifyWithDom(Ꮡpub, message, sig, domPrefixCtx, context);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string ed25519InvalidSignatureˢ = "ed25519: invalid signature"u8;

internal static error verifyWithDom(ж<ΔPublicKey> Ꮡpub, slice<byte> message, slice<byte> sig, @string domPrefix, @string context) {
    ref var pub = ref Ꮡpub.DerefOrNull();

    {
        nint l = len(sig); if (l != signatureSize) {
            return errors.New("ed25519: bad signature length: "u8 + strconv.Itoa(l));
        }
    }
    if ((byte)(sig[63] & 224) != 0) {
        return errors.New(ed25519InvalidSignatureˢ);
    }
    var kh = sha512.New();
    if (domPrefix != domPrefixPure) {
        kh.Write(slice<byte>(domPrefix));
        kh.Write(new byte[]{(byte)len(context)}.slice());
        kh.Write(slice<byte>(context));
    }
    kh.Write(sig[..32]);
    kh.Write(pub.aBytes[..]);
    kh.Write(message);
    var hramDigest = new slice<byte>(0, sha512Size);
    hramDigest = kh.Sum(hramDigest);
    var (k, err) = Δedwards25519.NewScalar().SetUniformBytes(hramDigest);
    if (err != default!) {
        throw panic("ed25519: internal error: setting scalar failed");
    }
    (var S, err) = Δedwards25519.NewScalar().SetCanonicalBytes(sig[32..]);
    if (err != default!) {
        return errors.New(ed25519InvalidSignatureˢ);
    }
    // [S]B = R + [k]A --> [k](-A) + [S]B = R
    var minusA = (Ꮡ(new Δedwards25519.Point(nil))).Negate(Ꮡpub.of(ed25519_package.ΔPublicKey.Ꮡa));
    var R = (Ꮡ(new Δedwards25519.Point(nil))).VarTimeDoubleScalarBaseMult(k, minusA, S);
    if (!bytes.Equal(sig[..32], R.Bytes())) {
        return errors.New(ed25519InvalidSignatureˢ);
    }
    return default!;
}

} // end ed25519_package
