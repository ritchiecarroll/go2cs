// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using elliptic = go.crypto.elliptic_package;
using fips140only = go.crypto.@internal.fips140only_package;
using errors = errors_package;
using io = io_package;
using big = math.big_package;
using rand = math.rand.rand_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using asn1 = vendor.golang.org.x.crypto.cryptobyte.asn1_package;
using go.crypto;
using go.crypto.@internal;
using math;
using math.rand;
using vendor.golang.org.x.crypto;
using vendor.golang.org.x.crypto.cryptobyte;

partial class ecdsa_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoEcdsaUseOfCustomˢ = "crypto/ecdsa: use of custom curves is not allowed in FIPS 140-only mode"u8;

// This file contains a math/big implementation of ECDSA that is only used for
// deprecated custom curves.
internal static (ж<PrivateKey>, error) generateLegacy(elliptic.Curve c, io.Reader rand) {
    if (fips140only.Enabled) {
        return (default!, errors.New(cryptoEcdsaUseOfCustomˢ));
    }
    var (k, err) = randFieldElement(c, rand);
    if (err != default!) {
        return (default!, err);
    }
    var priv = @new<PrivateKey>();
    priv.Value.PublicKey.Curve = c;
    priv.Value.D = k;
    (priv.Value.PublicKey.X, priv.Value.PublicKey.Y) = c.ScalarBaseMult(k.Bytes());
    return (priv, default!);
}

// hashToInt converts a hash value to an integer. Per FIPS 186-4, Section 6.4,
// we use the left-most bits of the hash to match the bit-length of the order of
// the curve. This also performs Step 5 of SEC 1, Version 2.0, Section 4.1.3.
internal static ж<bigꓸInt> hashToInt(slice<byte> hash, elliptic.Curve c) {
    nint orderBits = (~c.Params()).N.BitLen();
    nint orderBytes = (orderBits + 7) / 8;
    if (len(hash) > orderBytes) {
        hash = hash.slice(0, orderBytes);
    }
    var ret = @new<bigꓸInt>().SetBytes(hash);
    nint excess = len(hash) * 8 - orderBits;
    if (excess > 0) {
        ret.Rsh(ret, (nuint)excess);
    }
    return ret;
}

internal static error errZeroParam = errors.New("zero parameter"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidAsn1FromSignASN1ˢ = "invalid ASN.1 from SignASN1"u8;

// Sign signs a hash (which should be the result of hashing a larger message)
// using the private key, priv. If the hash is longer than the bit-length of the
// private key's curve order, the hash will be truncated to that length. It
// returns the signature as a pair of integers. Most applications should use
// [SignASN1] instead of dealing directly with r, s.
public static (ж<bigꓸInt> r, ж<bigꓸInt> s, error err) Sign(io.Reader rand, ж<PrivateKey> Ꮡpriv, slice<byte> hash) {
    ж<bigꓸInt> r = default!;
    ж<bigꓸInt> s = default!;
    error err = default!;

    (var sig, err) = SignASN1(rand, Ꮡpriv, hash);
    if (err != default!) {
        return (default!, default!, err);
    }
    (r, s) = (@new<bigꓸInt>(), @new<bigꓸInt>());
    ref var inner = ref heap<cryptobyte.String>(out var Ꮡinner);
    var input = ((cryptobyte.String)sig);
    if (!input.ReadASN1(Ꮡinner, asn1.SEQUENCE) || !input.Empty() || !inner.ReadASN1Integer(r.OrTypedNil()) || !inner.ReadASN1Integer(s.OrTypedNil()) || !inner.Empty()) {
        return (default!, default!, errors.New(invalidAsn1FromSignASN1ˢ));
    }
    return (r, s, default!);
}

internal static (slice<byte> sig, error err) signLegacy(ref PrivateKey priv, io.Reader csprng, slice<byte> hash) {
    error err = default!;

    if (fips140only.Enabled) {
        return (default!, errors.New(cryptoEcdsaUseOfCustomˢ));
    }
    var c = priv.Curve;
    // A cheap version of hedged signatures, for the deprecated path.
    array<byte> seed = new(32);
    {
        var (_, errΔ1) = io.ReadFull(csprng, seed[..]); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    foreach (var (i, b) in priv.D.Bytes()) {
        seed[i % 32] ^= (byte)(b);
    }
    foreach (var (i, b) in hash) {
        seed[i % 32] ^= (byte)(b);
    }
    csprng = new rand_ChaCha8жReader(rand.NewChaCha8(seed));
    // SEC 1, Version 2.0, Section 4.1.3
    var N = c.Params().Value.N;
    if (N.Sign() == 0) {
        return (default!, errZeroParam);
    }
    ж<bigꓸInt> k = default!;
    ж<bigꓸInt> kInv = default!;
    ж<bigꓸInt> r = default!;
    ж<bigꓸInt> s = default!;
    while (ᐧ) {
        while (ᐧ) {
            (k, err) = randFieldElement(c, csprng);
            if (err != default!) {
                return (default!, err);
            }
            kInv = @new<bigꓸInt>().ModInverse(k, N);
            (r, _) = c.ScalarBaseMult(k.Bytes());
            r.Mod(r, N);
            if (r.Sign() != 0) {
                break;
            }
        }
        var e = hashToInt(hash, c);
        s = @new<bigꓸInt>().Mul(priv.D, r);
        s.Add(s, e);
        s.Mul(s, kInv);
        s.Mod(s, N); // N != 0
        if (s.Sign() != 0) {
            break;
        }
    }
    return encodeSignature(r.Bytes(), s.Bytes());
}

// Verify verifies the signature in r, s of hash using the public key, pub. Its
// return value records whether the signature is valid. Most applications should
// use VerifyASN1 instead of dealing directly with r, s.
//
// The inputs are not considered confidential, and may leak through timing side
// channels, or if an attacker has control of part of the inputs.
public static bool Verify(ж<PublicKey> Ꮡpub, slice<byte> hash, ж<bigꓸInt> Ꮡr, ж<bigꓸInt> Ꮡs) {
    ref var r = ref Ꮡr.DerefOrNull();
    ref var s = ref Ꮡs.DerefOrNull();

    if (r.Sign() <= 0 || s.Sign() <= 0) {
        return false;
    }
    var (sig, err) = encodeSignature(r.Bytes(), s.Bytes());
    if (err != default!) {
        return false;
    }
    return VerifyASN1(Ꮡpub, hash, sig);
}

internal static bool verifyLegacy(ref PublicKey pub, slice<byte> hash, slice<byte> sig) {
    if (fips140only.Enabled) {
        throw panic("crypto/ecdsa: use of custom curves is not allowed in FIPS 140-only mode");
    }
    var (rBytes, sBytes, err) = parseSignature(sig);
    if (err != default!) {
        return false;
    }
    var (r, s) = (@new<bigꓸInt>().SetBytes(rBytes), @new<bigꓸInt>().SetBytes(sBytes));
    var c = pub.Curve;
    var N = c.Params().Value.N;
    if (r.Sign() <= 0 || s.Sign() <= 0) {
        return false;
    }
    if (r.Cmp(N) >= 0 || s.Cmp(N) >= 0) {
        return false;
    }
    // SEC 1, Version 2.0, Section 4.1.4
    var e = hashToInt(hash, c);
    var w = @new<bigꓸInt>().ModInverse(s, N);
    var u1 = e.Mul(e, w);
    u1.Mod(u1, N);
    var u2 = w.Mul(r, w);
    u2.Mod(u2, N);
    var (x1, y1) = c.ScalarBaseMult(u1.Bytes());
    var (x2, y2) = c.ScalarMult(pub.X, pub.Y, u2.Bytes());
    var (x, y) = c.Add(x1, y1, x2, y2);
    if (x.Sign() == 0 && y.Sign() == 0) {
        return false;
    }
    x.Mod(x, N);
    return x.Cmp(r) == 0;
}

internal static ж<bigꓸInt> one = @new<bigꓸInt>().SetInt64(1);

// randFieldElement returns a random element of the order of the given
// curve using the procedure given in FIPS 186-4, Appendix B.5.2.
internal static (ж<bigꓸInt> k, error err) randFieldElement(elliptic.Curve c, io.Reader rand) {
    ж<bigꓸInt> k = default!;
    error err = default!;

    while (ᐧ) {
        var N = c.Params().Value.N;
        var b = new slice<byte>((N.BitLen() + 7) / 8);
        {
            (_, err) = io.ReadFull(rand, b); if (err != default!) {
                return (k, err);
            }
        }
        {
            nint excess = len(b) * 8 - N.BitLen(); if (excess > 0) {
                b[0].RshAssign((int64)(excess));
            }
        }
        k = @new<bigꓸInt>().SetBytes(b);
        if (k.Sign() != 0 && k.Cmp(N) < 0) {
            return (k, err);
        }
    }
}

} // end ecdsa_package
