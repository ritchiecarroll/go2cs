// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using drbg = go.crypto.@internal.fips140.drbg_package;
using errors = errors_package;
using bits = math.bits_package;
using go.crypto.@internal.fips140;
using math;

partial class mldsa_package {

// FIPS 204 defines a needless semi-expanded format for private keys. This is
// not a good format for key storage and exchange, because it is large and
// requires careful parsing to reject malformed keys. Seeds instead are just 32
// bytes, are always valid, and always expand to valid keys in memory. It is
// *also* a poor in-memory format, because it defers computing the NTT of s1,
// s2, and t0 and the expansion of A until signing time, which is inefficient.
// For a hot second, it looked like we could have all agreed to only use seeds,
// but unfortunately OpenSSL and BouncyCastle lobbied hard against that during
// the WGLC of the LAMPS IETF working group. Also, ACVP tests provide and expect
// semi-expanded keys, so we implement them here for testing purposes.
internal static nint semiExpandedPrivKeySize(parameters p) {
    nint k = p.k;
    nint l = p.l;
    nint ηBitlen = bits.Len((nuint)p.η) + 1;
    // ρ + K + tr + l × n × η-bit coefficients of s₁ +
    // k × n × η-bit coefficients of s₂ + k × n × 13-bit coefficients of t₀
    return 32 + 32 + 64 + l * (nint)n * ηBitlen / 8 + k * (nint)n * ηBitlen / 8 + k * (nint)n * 13 / 8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mldsaInvalidSemiExpandedˢ = "mldsa: invalid semi-expanded private key size"u8;
internal static readonly @string mldsaSemiExpandedPrivateˢ = "mldsa: semi-expanded private key inconsistent with t0"u8;
internal static readonly @string mldsaSemiExpandedPrivateˢ2 = "mldsa: semi-expanded private key inconsistent with public key hash"u8;

// TestingOnlyNewPrivateKeyFromSemiExpanded creates a PrivateKey from a
// semi-expanded private key encoding, for testing purposes. It rejects
// inconsistent keys.
//
// [PrivateKey.Bytes] must NOT be called on the resulting key, as it will
// produce a random value.
public static (ж<PrivateKey>, error) TestingOnlyNewPrivateKeyFromSemiExpanded(slice<byte> sk) {
    parameters p = default!;
    var exprᴛ1 = len(sk);
    if (exprᴛ1 == semiExpandedPrivKeySize(params44)) {
        p = params44;
    }
    else if (exprᴛ1 == semiExpandedPrivKeySize(params65)) {
        p = params65;
    }
    else if (exprᴛ1 == semiExpandedPrivKeySize(params87)) {
        p = params87;
    }
    else { /* default: */
        return (default!, errors.New(mldsaInvalidSemiExpandedˢ));
    }
    nint k = p.k;
    nint l = p.l;

    var (ρ, K, tr, s1, s2, t0, err) = skDecode(sk, p);
    if (err != default!) {
        return (default!, err);
    }

    var priv = Ꮡ(new PrivateKey(pub: new ΔPublicKey(p: p)));
    priv.Value.k = K.Clone();
    priv.Value.pub.tr = tr.Clone();
    var A = GoReflect.WithElemDims((~priv).pub.a.slice(0, k * l), 256);
    computeMatrixA(A, ρ[..], p);
    foreach (var r in range(l)) {
        priv.Value.s1[r] = ntt(s1[r]);
    }
    foreach (var r in range(k)) {
        priv.Value.s2[r] = ntt(s2[r]);
    }
    foreach (var r in range(k)) {
        priv.Value.t0[r] = ntt(t0[r]);
    }

    // We need to put something in priv.seed, and putting random bytes feels
    // safer than putting anything predictable.
    drbg.Read((~priv).seed[..]);

    // Making this format *even more* annoying, we need to recompute t1 from ρ,
    // s1, and s2 if we want to generate the public key. This is essentially as
    // much work as regenerating everything from seed.
    //
    // You might also notice that the semi-expanded format also stores t0 and a
    // hash of the public key, though. How are we supposed to check they are
    // consistent without regenerating the public key? Do we even need to check?
    // Who knows! FIPS 204 says
    //
    //  > Note that there exist malformed inputs that can cause skDecode to
    //  > return values that are not in the correct range. Hence, skDecode
    //  > should only be run on inputs that come from trusted sources.
    //
    // so it sounds like it doesn't even want us to check the coefficients are
    // within bounds, but especially if using this format for key exchange, that
    // sounds like a bad idea. So we check everything.

    var t1 = GoReflect.WithElemDims(new slice<array<uint16>>(k, () => new(256), maxK), 256);
    foreach (var i in range(k)) {
        var tHat = (~priv).s2[i].Clone();
        foreach (var j in range(l)) {
            tHat = polyAdd(tHat, nttMul(A[i * l + j], (~priv).s1[j]));
        }
        var t = inverseNTT(tHat);
        foreach (var j in range(n)) {
            var (r1, r0) = power2Round(t[j]);
            t1[i][j] = r1;
            if (r0 != t0[i][j]) {
                return (default!, errors.New(mldsaSemiExpandedPrivateˢ));
            }
        }
    }

    var pk = pkEncode((~priv).pub.raw[..0], ρ[..], t1, p);
    if (computePublicKeyHash(pk) != tr) {
        return (default!, errors.New(mldsaSemiExpandedPrivateˢ2));
    }
    computeT1Hat(GoReflect.WithElemDims((~priv).pub.t1.slice(0, k), 256), t1); // NTT(t₁ ⋅ 2ᵈ)

    return (priv, default!);
}

public static slice<byte> TestingOnlyPrivateKeySemiExpandedBytes(ж<PrivateKey> Ꮡpriv) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    nint k = priv.pub.p.k;
    nint l = priv.pub.p.l;
    nint η = priv.pub.p.η;
    var sk = new slice<byte>(0, semiExpandedPrivKeySize(priv.pub.p));
    sk = appendꓸꓸꓸ(sk, priv.pub.raw[..32]); // ρ
    sk = appendꓸꓸꓸ(sk, priv.k[..]); // K
    sk = appendꓸꓸꓸ(sk, priv.pub.tr[..]); // tr
    foreach (var i in range(l)) {
        sk = bitPackSlow(sk, inverseNTT(priv.s1[i]), η, η);
    }
    foreach (var i in range(k)) {
        sk = bitPackSlow(sk, inverseNTT(priv.s2[i]), η, η);
    }
    UntypedInt bound = /* 1 << (13 - 1) */ 4096; // 2^(d-1)
    foreach (var i in range(k)) {
        sk = bitPackSlow(sk, inverseNTT(priv.t0[i]), bound - 1, bound);
    }
    return sk;
}

internal static (array<byte> ρ, array<byte> K, array<byte> tr, slice<ringElement> s1, slice<ringElement> s2, slice<ringElement> t0, error err) skDecode(slice<byte> sk, parameters p) {
    array<byte> ρ = new(32);
    array<byte> K = new(32);
    array<byte> tr = new(64);
    slice<ringElement> s1 = default!;
    slice<ringElement> s2 = default!;
    slice<ringElement> t0 = default!;
    error err = default!;

    nint k = p.k;
    nint l = p.l;
    nint η = p.η;
    if (len(sk) != semiExpandedPrivKeySize(p)) {
        err = errors.New(mldsaInvalidSemiExpandedˢ);
        return (ρ, K, tr, s1, s2, t0, err);
    }
    copy(ρ[..], sk[..32]);
    sk = sk[32..];
    copy(K[..], sk[..32]);
    sk = sk[32..];
    copy(tr[..], sk[..64]);
    sk = sk[64..];

    s1 = GoReflect.WithElemDims(new slice<ringElement>(l), 256);
    foreach (var i in range(l)) {
        nint length = (nint)n * bits.Len((nuint)η * 2) / 8;
        (s1[i], err) = bitUnpackSlow(sk.slice(0, length), η, η);
        if (err != default!) {
            return (ρ, K, tr, s1, s2, t0, err);
        }
        sk = sk.slice(length);
    }

    s2 = GoReflect.WithElemDims(new slice<ringElement>(k), 256);
    foreach (var i in range(k)) {
        nint length = (nint)n * bits.Len((nuint)η * 2) / 8;
        (s2[i], err) = bitUnpackSlow(sk.slice(0, length), η, η);
        if (err != default!) {
            return (ρ, K, tr, s1, s2, t0, err);
        }
        sk = sk.slice(length);
    }

    UntypedInt bound = /* 1 << (13 - 1) */ 4096; // 2^(d-1)
    t0 = GoReflect.WithElemDims(new slice<ringElement>(k), 256);
    foreach (var i in range(k)) {
        nint length = n * 13 / 8;
        (t0[i], err) = bitUnpackSlow(sk.slice(0, length), bound - 1, bound);
        if (err != default!) {
            return (ρ, K, tr, s1, s2, t0, err);
        }
        sk = sk.slice(length);
    }

    return (ρ, K, tr, s1, s2, t0, err);
}

internal static slice<byte> bitPackSlow(slice<byte> buf, ringElement r, nint a, nint b) {
    r = r.Clone();

    nint bitlen = bits.Len((nuint)(a + b));
    if (bitlen <= 0 || bitlen > 16) {
        throw panic("mldsa: internal error: invalid bitlen");
    }
    var (@out, v) = sliceForAppend(buf, (nint)n * bitlen / 8);
    uint32 acc = default!;
    nuint accBits = default!;
    foreach (var (i, _) in r) {
        var w = (int32)b - fieldCenteredMod(r[i]);
        acc |= (uint32)(((uint32)w).Lsh(accBits));
        accBits += (nuint)bitlen;
        while (accBits >= 8) {
            v[0] = (byte)acc;
            v = v[1..];
            acc >>= (int)(8);
            accBits -= 8;
        }
    }
    if (accBits > 0) {
        v[0] = (byte)acc;
    }
    return @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mldsaInvalidInputLengthˢ = "mldsa: invalid input length for bitUnpackSlow"u8;
internal static readonly @string mldsaCoefficientOutOfˢ = "mldsa: coefficient out of range"u8;

internal static (ringElement, error) bitUnpackSlow(slice<byte> v, nint a, nint b) {
    nint bitlen = bits.Len((nuint)(a + b));
    if (bitlen <= 0 || bitlen > 16) {
        throw panic("mldsa: internal error: invalid bitlen");
    }
    if (len(v) != (nint)n * bitlen / 8) {
        return (new ringElement(new fieldElement[256].array()), errors.New(mldsaInvalidInputLengthˢ));
    }

    var mask = (uint32)((((uint32)1).Lsh((int64)(bitlen))) - 1);
    var maxValue = (uint32)(a + b);

    ringElement r = default!;
    uint32 acc = default!;
    nuint accBits = default!;
    nint vIdx = 0;

    foreach (var (i, _) in r) {
        while (accBits < (nuint)bitlen) {
            if (vIdx < len(v)) {
                acc |= (uint32)(((uint32)v[vIdx]).Lsh(accBits));
                vIdx++;
                accBits += 8;
            }
        }
        var w = acc & mask;
        if (w > maxValue) {
            return (new ringElement(new fieldElement[256].array()), errors.New(mldsaCoefficientOutOfˢ));
        }
        r[i] = fieldSubToMontgomery((uint32)b, w);
        acc.RshAssign((int64)(bitlen));
        accBits -= (nuint)bitlen;
    }

    return (r.Clone(), default!);
}

} // end mldsa_package
