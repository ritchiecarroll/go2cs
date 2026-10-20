// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using errors = errors_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140deps;

partial class mldsa_package {

public partial struct parameters {
    internal nint k, l; // dimensions of A
    internal nint η; // bound for secret coefficients
    internal nint γ1; // log₂(γ₁), where [-γ₁+1, γ₁] is the bound of y
    internal nint γ2; // denominator of γ₂ = (q - 1) / γ2
    internal nint λ; // collison strength
    internal nint τ; // number of non-zero coefficients in challenge
    internal nint ω; // max number of hints in MakeHint
}

internal static parameters params44 = new parameters(k: 4, l: 4, η: 2, γ1: 17, γ2: 88, λ: 128, τ: 39, ω: 80);
internal static parameters params65 = new parameters(k: 6, l: 5, η: 4, γ1: 19, γ2: 32, λ: 192, τ: 49, ω: 55);
internal static parameters params87 = new parameters(k: 8, l: 7, η: 2, γ1: 19, γ2: 32, λ: 256, τ: 60, ω: 75);

internal static nint pubKeySize(parameters p) {
    // ρ + k × n × 10-bit coefficients of t₁
    return 32 + p.k * (nint)n * 10 / 8;
}

internal static nint sigSize(parameters p) {
    // challenge + l × n × (γ₁+1)-bit coefficients of z + hint
    return (p.λ / 4) + p.l * (nint)n * (p.γ1 + 1) / 8 + p.ω + p.k;
}

public static UntypedInt PrivateKeySize => 32;

public static UntypedInt PublicKeySize44 => /* 32 + 4*n*10/8 */ 1312;
public static UntypedInt PublicKeySize65 => /* 32 + 6*n*10/8 */ 1952;
public static UntypedInt PublicKeySize87 => /* 32 + 8*n*10/8 */ 2592;

public static UntypedInt SignatureSize44 => /* 128/4 + 4*n*(17+1)/8 + 80 + 4 */ 2420;
public static UntypedInt SignatureSize65 => /* 192/4 + 5*n*(19+1)/8 + 55 + 6 */ 3309;
public static UntypedInt SignatureSize87 => /* 256/4 + 7*n*(19+1)/8 + 75 + 8 */ 4627;

internal static UntypedInt maxK => 8;
internal static UntypedInt maxL => 7;
internal static UntypedInt maxλ => 256;
internal static UntypedInt maxγ1 => 19;

internal static UntypedInt maxPubKeySize => /* PublicKeySize87 */ 2592;

partial struct PrivateKey {
    internal array<byte> seed = new(32);
    internal ΔPublicKey pub;
    internal array<nttElement> s1 = new(maxL);
    internal array<nttElement> s2 = new(maxK);
    internal array<nttElement> t0 = new(maxK);
    internal array<byte> k = new(32);
}

public static bool Equal(this ref PrivateKey priv, ж<PrivateKey> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    return priv.pub.p == x.pub.p && subtle.ConstantTimeCompare(priv.seed[..], x.seed[..]) == 1;
}

public static slice<byte> Bytes(this ref PrivateKey priv) {
    var seed = priv.seed.Clone();
    return seed[..];
}

public static ж<ΔPublicKey> PublicKey(this ж<PrivateKey> Ꮡpriv) {
    // Note that this is likely to keep the entire PrivateKey reachable for
    // the lifetime of the PublicKey, which may be undesirable.
    return Ꮡpriv.of(PrivateKey.Ꮡpub);
}

partial struct ΔPublicKey {
    internal array<byte> raw = new(maxPubKeySize);
    internal parameters p;
    internal array<nttElement> a = new(maxK * maxL);
    internal array<nttElement> t1 = new(maxK); // NTT(t₁ ⋅ 2ᵈ)
    internal array<byte> tr = new(64);      // public key hash
}

public static bool Equal(this ref ΔPublicKey pub, ж<ΔPublicKey> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    nint size = pubKeySize(pub.p);
    return pub.p == x.p && subtle.ConstantTimeCompare(pub.raw.slice(0, size), x.raw.slice(0, size)) == 1;
}

public static slice<byte> Bytes(this ref ΔPublicKey pub) {
    nint size = pubKeySize(pub.p);
    return bytes.Clone(pub.raw.slice(0, size));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mlDsa44ˢ = "ML-DSA-44"u8;
internal static readonly @string mlDsa65ˢ = "ML-DSA-65"u8;
internal static readonly @string mlDsa87ˢ = "ML-DSA-87"u8;

public static @string Parameters(this ref ΔPublicKey pub) {
    var exprᴛ1 = pub.p;
    if (exprᴛ1 == params44) {
        return mlDsa44ˢ;
    }
    if (exprᴛ1 == params65) {
        return mlDsa65ˢ;
    }
    if (exprᴛ1 == params87) {
        return mlDsa87ˢ;
    }
    { /* default: */
        throw panic("mldsa: internal error: unknown parameters");
    }
}

public static ж<PrivateKey> GenerateKey44() {
    fipsSelfTest();
    fips140.RecordApproved();
    ref var seed = ref heap(new array<byte>(32), out var Ꮡseed);
    drbg.Read(seed[..]);
    var priv = newPrivateKey(Ꮡseed, params44);
    fipsPCT(priv);
    return priv;
}

public static ж<PrivateKey> GenerateKey65() {
    fipsSelfTest();
    fips140.RecordApproved();
    ref var seed = ref heap(new array<byte>(32), out var Ꮡseed);
    drbg.Read(seed[..]);
    var priv = newPrivateKey(Ꮡseed, params65);
    fipsPCT(priv);
    return priv;
}

public static ж<PrivateKey> GenerateKey87() {
    fipsSelfTest();
    fips140.RecordApproved();
    ref var seed = ref heap(new array<byte>(32), out var Ꮡseed);
    drbg.Read(seed[..]);
    var priv = newPrivateKey(Ꮡseed, params87);
    fipsPCT(priv);
    return priv;
}

internal static error errInvalidSeedLength = errors.New("mldsa: invalid seed length"u8);

public static (ж<PrivateKey>, error) NewPrivateKey44(slice<byte> seed) {
    fipsSelfTest();
    fips140.RecordApproved();
    if (len(seed) != 32) {
        return (default!, errInvalidSeedLength);
    }
    return (newPrivateKey(Ꮡ(array<byte>.Alias(seed, 32)), params44), default!);
}

public static (ж<PrivateKey>, error) NewPrivateKey65(slice<byte> seed) {
    fipsSelfTest();
    fips140.RecordApproved();
    if (len(seed) != 32) {
        return (default!, errInvalidSeedLength);
    }
    return (newPrivateKey(Ꮡ(array<byte>.Alias(seed, 32)), params65), default!);
}

public static (ж<PrivateKey>, error) NewPrivateKey87(slice<byte> seed) {
    fipsSelfTest();
    fips140.RecordApproved();
    if (len(seed) != 32) {
        return (default!, errInvalidSeedLength);
    }
    return (newPrivateKey(Ꮡ(array<byte>.Alias(seed, 32)), params87), default!);
}

internal static ж<PrivateKey> newPrivateKey(/*[32]*/ ж<array<byte>> Ꮡseed, parameters p) {
    ref var seed = ref Ꮡseed.DerefOrNull();

    nint k = p.k;
    nint l = p.l;

    var priv = Ꮡ(new PrivateKey(pub: new ΔPublicKey(p: p)));
    priv.Value.seed = seed.Clone();

    var ξ = sha3.NewShake256();
    ξ.Write(seed[..]);
    ξ.Write(new byte[]{(byte)k, (byte)l}.slice());
    var (ρ, ρs) = (new slice<byte>(32), new slice<byte>(64));
    ξ.Read(ρ);
    ξ.Read(ρs);
    ξ.Read((~priv).k[..]);

    var A = GoReflect.WithElemDims((~priv).pub.a.slice(0, k * l), 256);
    computeMatrixA(A, ρ, p);

    var s1 = GoReflect.WithElemDims((~priv).s1.slice(0, l), 256);
    foreach (var r in range(l)) {
        s1[r] = ntt(sampleBoundedPoly(ρs, (byte)r, p));
    }
    var s2 = GoReflect.WithElemDims((~priv).s2.slice(0, k), 256);
    foreach (var r in range(k)) {
        s2[r] = ntt(sampleBoundedPoly(ρs, (byte)(l + r), p));
    }

    // ˆt = Â ∘ ŝ₁ + ŝ₂
    var tHat = GoReflect.WithElemDims(new slice<nttElement>(k, maxK), 256);
    foreach (var (i, _) in tHat) {
        tHat[i] = s2[i].Clone();
        foreach (var (j, _) in s1) {
            tHat[i] = polyAdd(tHat[i], nttMul(A[i * l + j], s1[j]));
        }
    }
    // t = NTT⁻¹(ˆt)
    var t = GoReflect.WithElemDims(new slice<ringElement>(k, maxK), 256);
    foreach (var (i, _) in tHat) {
        t[i] = inverseNTT(tHat[i]);
    }
    // (t₁, _) = Power2Round(t)
    // (_, ˆt₀) = NTT(Power2Round(t))
    var (t1, t0) = (GoReflect.WithElemDims(new slice<array<uint16>>(k, () => new(256), maxK), 256), GoReflect.WithElemDims((~priv).t0.slice(0, k), 256));
    foreach (var (i, _) in t) {
        ringElement w = default!;
        foreach (var (j, _) in t[i]) {
            (t1[i][j], w[j]) = power2Round(t[i][j]);
        }
        t0[i] = ntt(w);
    }

    // The computations below (and their storage in the PrivateKey struct) are
    // not strictly necessary and could be deferred to PrivateKey.PublicKey().
    // That would require keeping or re-deriving ρ and t/t1, though.

    var pk = pkEncode((~priv).pub.raw[..0], ρ, t1, p);
    priv.Value.pub.tr = computePublicKeyHash(pk);
    computeT1Hat(GoReflect.WithElemDims((~priv).pub.t1.slice(0, k), 256), t1); // NTT(t₁ ⋅ 2ᵈ)

    return priv;
}

internal static void computeMatrixA(slice<nttElement> A, slice<byte> ρ, parameters p) {
    nint k = p.k;
    nint l = p.l;
    foreach (var r in range(k)) {
        foreach (var s in range(l)) {
            A[r * l + s] = sampleNTT(ρ, (byte)s, (byte)r);
        }
    }
}

internal static array<byte> computePublicKeyHash(slice<byte> pk) {
    var H = sha3.NewShake256();
    H.Write(pk);
    array<byte> tr = new(64);
    H.Read(tr[..]);
    return tr.Clone();
}

internal static void computeT1Hat(slice<nttElement> t1Hat, slice<array<uint16>> t1) {
    foreach (var (i, _) in t1) {
        ringElement w = default!;
        foreach (var (j, _) in t1[i]) {
            // t₁ <= 2¹⁰ - 1
            // t₁ ⋅ 2ᵈ <= 2ᵈ(2¹⁰ - 1) = 2²³ - 2¹³ < q = 2²³ - 2¹³ + 1
            var (z, _) = fieldToMontgomery((uint32)t1[i][j] << 13);
            w[j] = z;
        }
        t1Hat[i] = ntt(w);
    }
}

internal static slice<byte> pkEncode(slice<byte> buf, slice<byte> ρ, slice<array<uint16>> t1, parameters p) {
    var pk = appendꓸꓸꓸ(buf, ρ);
    foreach (var (_, vᴛ1) in GoReflect.WithElemDims(t1.slice(0, p.k), 256)) {
        var w = vᴛ1.Clone();

        // Encode four at a time into 4 * 10 bits = 5 bytes.
        for (nint i = 0; i < n; i += 4) {
            var c0 = w[i];
            var c1 = w[i + 1];
            var c2 = w[i + 2];
            var c3 = w[i + 3];
            var b0 = (byte)(c0 >> 0);
            var b1 = (byte)((uint16)((c0 >> 8) | ((uint16)(c1 << 2))));
            var b2 = (byte)((uint16)((c1 >> 6) | ((uint16)(c2 << 4))));
            var b3 = (byte)((uint16)((c2 >> 4) | ((uint16)(c3 << 6))));
            var b4 = (byte)(c3 >> 2);
            pk = append(pk, b0, b1, b2, b3, b4);
        }
    }
    return pk;
}

internal static (slice<byte> ρ, error err) pkDecode(slice<byte> pk, slice<array<uint16>> t1, parameters p) {
    slice<byte> ρ = default!;

    if (len(pk) != pubKeySize(p)) {
        return (default!, errInvalidPublicKeyLength);
    }
    (ρ, pk) = (pk[..32], pk[32..]);
    foreach (var (r, _) in t1) {
        // Decode four at a time from 4 * 10 bits = 5 bytes.
        for (nint i = 0; i < n; i += 4) {
            var (b0, b1, b2, b3, b4) = (pk[0], pk[1], pk[2], pk[3], pk[4]);
            t1[r][i + 0] = (uint16)((uint16)(b0 >> 0) | (uint16)((uint16)((byte)(b1 & 0b0000_0011)) << 8));
            t1[r][i + 1] = (uint16)((uint16)(b1 >> 2) | (uint16)((uint16)((byte)(b2 & 0b0000_1111)) << 6));
            t1[r][i + 2] = (uint16)((uint16)(b2 >> 4) | (uint16)((uint16)((byte)(b3 & 0b0011_1111)) << 4));
            t1[r][i + 3] = (uint16)((uint16)(b3 >> 6) | (uint16)((uint16)((byte)(b4 & 0b1111_1111)) << 2));
            pk = pk[5..];
        }
    }
    return (ρ, default!);
}

internal static error errInvalidPublicKeyLength = errors.New("mldsa: invalid public key length"u8);

public static (ж<ΔPublicKey>, error) NewPublicKey44(slice<byte> pk) {
    return newPublicKey(pk, params44);
}

public static (ж<ΔPublicKey>, error) NewPublicKey65(slice<byte> pk) {
    return newPublicKey(pk, params65);
}

public static (ж<ΔPublicKey>, error) NewPublicKey87(slice<byte> pk) {
    return newPublicKey(pk, params87);
}

internal static (ж<ΔPublicKey>, error) newPublicKey(slice<byte> pk, parameters p) {
    nint k = p.k;
    nint l = p.l;

    var t1 = GoReflect.WithElemDims(new slice<array<uint16>>(k, () => new(256), maxK), 256);
    var (ρ, err) = pkDecode(pk, t1, p);
    if (err != default!) {
        return (default!, err);
    }

    var pub = Ꮡ(new ΔPublicKey(p: p));
    copy((~pub).raw[..], pk);
    computeMatrixA(GoReflect.WithElemDims((~pub).a.slice(0, k * l), 256), ρ, p);
    pub.Value.tr = computePublicKeyHash(pk);
    computeT1Hat(GoReflect.WithElemDims((~pub).t1.slice(0, k), 256), t1); // NTT(t₁ ⋅ 2ᵈ)

    return (pub, default!);
}

internal static error errContextTooLong = errors.New("mldsa: context too long"u8);
internal static error errMessageHashLength = errors.New("mldsa: invalid message hash length"u8);
internal static error errRandomLength = errors.New("mldsa: invalid random length"u8);

public static (slice<byte>, error) Sign(ж<PrivateKey> Ꮡpriv, slice<byte> msg, @string context) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    fipsSelfTest();
    fips140.RecordApproved();
    ref var random = ref heap(new array<byte>(32), out var Ꮡrandom);
    drbg.Read(random[..]);
    ref var μ = ref heap<array<byte>>(out var Ꮡμ);
    (μ, var err) = computeMessageHash(priv.pub.tr[..], msg, context);
    if (err != default!) {
        return (default!, err);
    }
    return (signInternal(ref (Ꮡpriv).DerefOrNull(), Ꮡμ, Ꮡrandom), default!);
}

public static (slice<byte>, error) SignDeterministic(ж<PrivateKey> Ꮡpriv, slice<byte> msg, @string context) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    fipsSelfTest();
    fips140.RecordApproved();
    ref var random = ref heap(new array<byte>(32), out var Ꮡrandom);
    ref var μ = ref heap<array<byte>>(out var Ꮡμ);
    (μ, var err) = computeMessageHash(priv.pub.tr[..], msg, context);
    if (err != default!) {
        return (default!, err);
    }
    return (signInternal(ref (Ꮡpriv).DerefOrNull(), Ꮡμ, Ꮡrandom), default!);
}

public static (slice<byte>, error) TestingOnlySignWithRandom(ж<PrivateKey> Ꮡpriv, slice<byte> msg, @string context, slice<byte> random) {
    ref var priv = ref Ꮡpriv.DerefOrNull();

    fipsSelfTest();
    fips140.RecordApproved();
    ref var μ = ref heap<array<byte>>(out var Ꮡμ);
    (μ, var err) = computeMessageHash(priv.pub.tr[..], msg, context);
    if (err != default!) {
        return (default!, err);
    }
    if (len(random) != 32) {
        return (default!, errRandomLength);
    }
    return (signInternal(ref (Ꮡpriv).DerefOrNull(), Ꮡμ, Ꮡ(array<byte>.Alias(random, 32))), default!);
}

public static (slice<byte>, error) SignExternalMu(ж<PrivateKey> Ꮡpriv, slice<byte> μ) {
    fipsSelfTest();
    fips140.RecordApproved();
    ref var random = ref heap(new array<byte>(32), out var Ꮡrandom);
    drbg.Read(random[..]);
    if (len(μ) != 64) {
        return (default!, errMessageHashLength);
    }
    return (signInternal(ref (Ꮡpriv).DerefOrNull(), Ꮡ(array<byte>.Alias(μ, 64)), Ꮡrandom), default!);
}

public static (slice<byte>, error) SignExternalMuDeterministic(ж<PrivateKey> Ꮡpriv, slice<byte> μ) {
    fipsSelfTest();
    fips140.RecordApproved();
    ref var random = ref heap(new array<byte>(32), out var Ꮡrandom);
    if (len(μ) != 64) {
        return (default!, errMessageHashLength);
    }
    return (signInternal(ref (Ꮡpriv).DerefOrNull(), Ꮡ(array<byte>.Alias(μ, 64)), Ꮡrandom), default!);
}

public static (slice<byte>, error) TestingOnlySignExternalMuWithRandom(ж<PrivateKey> Ꮡpriv, slice<byte> μ, slice<byte> random) {
    fipsSelfTest();
    fips140.RecordApproved();
    if (len(μ) != 64) {
        return (default!, errMessageHashLength);
    }
    if (len(random) != 32) {
        return (default!, errRandomLength);
    }
    return (signInternal(ref (Ꮡpriv).DerefOrNull(), Ꮡ(array<byte>.Alias(μ, 64)), Ꮡ(array<byte>.Alias(random, 32))), default!);
}

internal static (array<byte>, error) computeMessageHash(slice<byte> tr, slice<byte> msg, @string context) {
    if (len(context) > 255) {
        return (new byte[]{}.array(64), errContextTooLong);
    }
    var H = sha3.NewShake256();
    H.Write(tr);
    H.Write(new byte[]{0}.slice()); // ML-DSA / HashML-DSA domain separator
    H.Write(new byte[]{(byte)len(context)}.slice());
    H.Write(slice<byte>(context));
    H.Write(msg);
    array<byte> μ = new(64);
    H.Read(μ[..]);
    return (μ.Clone(), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ct0ˢ = "ct0"u8;

internal static slice<byte> signInternal(ref PrivateKey priv, /*[64]*/ ж<array<byte>> Ꮡμ, /*[32]*/ ж<array<byte>> Ꮡrandom) {
    ref var μ = ref Ꮡμ.DerefOrNull();
    ref var random = ref Ꮡrandom.DerefOrNull();

    var p = priv.pub.p;
    nint k = priv.pub.p.k;
    nint l = priv.pub.p.l;
    var (A, s1, s2, t0) = (GoReflect.WithElemDims(priv.pub.a.slice(0, k * l), 256), GoReflect.WithElemDims(priv.s1.slice(0, l), 256), GoReflect.WithElemDims(priv.s2.slice(0, k), 256), GoReflect.WithElemDims(priv.t0.slice(0, k), 256));

    nint β = p.τ * p.η;
    var γ1 = (uint32)(((uint32)1).Lsh((int64)(p.γ1)));
    var γ1β = γ1 - (uint32)β;
    var γ2 = (uint32)(q - 1) / (uint32)p.γ2;
    var γ2β = γ2 - (uint32)β;

    var H = sha3.NewShake256();
    H.Write(priv.k[..]);
    H.Write(random[..]);
    H.Write(μ[..]);
    var nonce = new slice<byte>(64);
    H.Read(nonce);

    nint κ = 0;
sign:
    while (ᐧ) {
        // Main rejection sampling loop. Note that leaking rejected signatures
        // leaks information about the private key. However, as explained in
        // https://pq-crystals.org/dilithium/data/dilithium-specification-round3.pdf
        // Section 5.5, we are free to leak rejected ch values, as well as which
        // check causes the rejection and which coefficient failed the check
        // (but not the value or sign of the coefficient).
        var y = GoReflect.WithElemDims(new slice<ringElement>(l, maxL), 256);
        foreach (var (r, _) in y) {
            var counter = new slice<byte>(2);
            byteorder.LEPutUint16(counter, (uint16)κ);
            κ++;

            H.Reset();
            H.Write(nonce);
            H.Write(counter);
            var v = new slice<byte>((p.γ1 + 1) * (nint)n / 8, (maxγ1 + 1) * n / 8);
            H.Read(v);

            y[r] = bitUnpack(v, p);
        }

        // w = NTT⁻¹(Â ∘ NTT(y))
        var yHat = GoReflect.WithElemDims(new slice<nttElement>(l, maxL), 256);
        foreach (var (i, _) in y) {
            yHat[i] = ntt(y[i]);
        }
        var w = GoReflect.WithElemDims(new slice<ringElement>(k, maxK), 256);
        foreach (var (i, _) in w) {
            nttElement wHat = default!;
            foreach (var j in range(l)) {
                wHat = polyAdd(wHat, nttMul(A[i * l + j], yHat[j]));
            }
            w[i] = inverseNTT(wHat);
        }

        H.Reset();
        H.Write(μ[..]);
        foreach (var (i, _) in w) {
            w1Encode(H, highBits(w[i], p), p);
        }
        var ch = new slice<byte>(p.λ / 4, maxλ / 4);
        H.Read(ch);

        // sampleInBall is not constant time, but see comment above about
        // leaking rejected ch values being acceptable.
        var c = ntt(sampleInBall(ch, p));

        var cs1 = GoReflect.WithElemDims(new slice<ringElement>(l, maxL), 256);
        foreach (var (i, _) in cs1) {
            cs1[i] = inverseNTT(nttMul(c, s1[i]));
        }
        var cs2 = GoReflect.WithElemDims(new slice<ringElement>(k, maxK), 256);
        foreach (var (i, _) in cs2) {
            cs2[i] = inverseNTT(nttMul(c, s2[i]));
        }

        var z = GoReflect.WithElemDims(new slice<ringElement>(l, maxL), 256);
        foreach (var (i, _) in y) {
            z[i] = polyAdd(y[i], cs1[i]);

            // Reject if ||z||∞ ≥ γ1 − β
            if (coefficientsExceedBound(z[i], γ1β)) {
                if (testingOnlyRejectionReason != default!) {
                    testingOnlyRejectionReason("z"u8);
                }
                goto continue_sign;
            }
        }

        foreach (var (i, _) in w) {
            var r0 = polySub(w[i], cs2[i]);

            // Reject if ||LowBits(r0)||∞ ≥ γ2 − β
            if (lowBitsExceedBound(r0, γ2β, p)) {
                if (testingOnlyRejectionReason != default!) {
                    testingOnlyRejectionReason("r0"u8);
                }
                goto continue_sign;
            }
        }

        var ct0 = GoReflect.WithElemDims(new slice<ringElement>(k, maxK), 256);
        foreach (var (i, _) in ct0) {
            ct0[i] = inverseNTT(nttMul(c, t0[i]));

            // Reject if ||ct0||∞ ≥ γ2
            if (coefficientsExceedBound(ct0[i], γ2)) {
                if (testingOnlyRejectionReason != default!) {
                    testingOnlyRejectionReason(ct0ˢ);
                }
                goto continue_sign;
            }
        }

        nint count1s = 0;
        var h = GoReflect.WithElemDims(new slice<array<byte>>(k, () => new(256), maxK), 256);
        foreach (var (i, _) in w) {
            nint count = default!;
            (h[i], count) = makeHint(ct0[i], w[i], cs2[i], p);
            count1s += count;
        }
        // Reject if number of hints > ω
        if (count1s > p.ω) {
            if (testingOnlyRejectionReason != default!) {
                testingOnlyRejectionReason("h"u8);
            }
            goto continue_sign;
        }

        return sigEncode(ch, z, h, p);
continue_sign:;
    }
break_sign:;
}

// testingOnlyRejectionReason is set in tests, to ensure that all rejection
// paths are covered. If not nil, it is called with a string describing the
// reason for rejection: "z", "r0", "ct0", or "h".
internal static Action<@string> testingOnlyRejectionReason;

// w1Encode implements w1Encode from FIPS 204, writing directly into H.
internal static void w1Encode(ж<sha3.SHAKE> ᏑH, /*[256]*/ array<byte> w, parameters p) {
    w = w.Clone();

    switch (p.γ2) {
    case 32: {
        // Coefficients are <= (q − 1)/(2γ2) − 1 = 15, four bits each.
        var buf = new slice<byte>(4 * n / 8);
        for (nint i = 0; i < n; i += 2) {
            var b0 = w[i];
            var b1 = w[i + 1];
            buf[i / 2] = (byte)(b0 | (byte)(b1 << 4));
        }
        ᏑH.Write(buf);
        break;
    }
    case 88: {
        // Coefficients are <= (q − 1)/(2γ2) − 1 = 43, six bits each.
        var buf = new slice<byte>(6 * n / 8);
        for (nint i = 0; i < n; i += 4) {
            var b0 = w[i];
            var b1 = w[i + 1];
            var b2 = w[i + 2];
            var b3 = w[i + 3];
            buf[3 * i / 4 + 0] = (byte)((b0 >> 0) | ((byte)(b1 << 6)));
            buf[3 * i / 4 + 1] = (byte)((b1 >> 2) | ((byte)(b2 << 4)));
            buf[3 * i / 4 + 2] = (byte)((b2 >> 4) | ((byte)(b3 << 2)));
        }
        ᏑH.Write(buf);
        break;
    }
    default: {
        throw panic("mldsa: internal error: unsupported γ2");
        break;
    }}
}

internal static bool coefficientsExceedBound(ringElement w, uint32 bound) {
    w = w.Clone();

    // If this function appears in profiles, it might be possible to deduplicate
    // the work of fieldFromMontgomery inside fieldInfinityNorm with the
    // subsequent encoding of w.
    foreach (var (i, _) in w) {
        if (fieldInfinityNorm(w[i]) >= bound) {
            return true;
        }
    }
    return false;
}

internal static bool lowBitsExceedBound(ringElement w, uint32 bound, parameters p) {
    w = w.Clone();

    switch (p.γ2) {
    case 32: {
        foreach (var (i, _) in w) {
            var (_, r0) = decompose32(w[i]);
            if (constantTimeAbs(r0) >= bound) {
                return true;
            }
        }
        break;
    }
    case 88: {
        foreach (var (i, _) in w) {
            var (_, r0) = decompose88(w[i]);
            if (constantTimeAbs(r0) >= bound) {
                return true;
            }
        }
        break;
    }
    default: {
        throw panic("mldsa: internal error: unsupported γ2");
        break;
    }}
    return false;
}

internal static error errInvalidSignatureLength = errors.New("mldsa: invalid signature length"u8);
internal static error errInvalidSignatureCoeffBounds = errors.New("mldsa: invalid signature"u8);
internal static error errInvalidSignatureChallenge = errors.New("mldsa: invalid signature"u8);
internal static error errInvalidSignatureHintLimits = errors.New("mldsa: invalid signature encoding"u8);
internal static error errInvalidSignatureHintIndexOrder = errors.New("mldsa: invalid signature encoding"u8);
internal static error errInvalidSignatureHintExtraIndices = errors.New("mldsa: invalid signature encoding"u8);

public static error Verify(ж<ΔPublicKey> Ꮡpub, slice<byte> msg, slice<byte> sig, @string context) {
    ref var pub = ref Ꮡpub.DerefOrNull();

    fipsSelfTest();
    fips140.RecordApproved();
    ref var μ = ref heap<array<byte>>(out var Ꮡμ);
    (μ, var err) = computeMessageHash(pub.tr[..], msg, context);
    if (err != default!) {
        return err;
    }
    return verifyInternal(ref (Ꮡpub).DerefOrNull(), Ꮡμ, sig);
}

public static error VerifyExternalMu(ж<ΔPublicKey> Ꮡpub, slice<byte> μ, slice<byte> sig) {
    fipsSelfTest();
    fips140.RecordApproved();
    if (len(μ) != 64) {
        return errMessageHashLength;
    }
    return verifyInternal(ref (Ꮡpub).DerefOrNull(), Ꮡ(array<byte>.Alias(μ, 64)), sig);
}

internal static error verifyInternal(ref ΔPublicKey pub, /*[64]*/ ж<array<byte>> Ꮡμ, slice<byte> sig) {
    ref var μ = ref Ꮡμ.DerefOrNull();

    var p = pub.p;
    nint k = pub.p.k;
    nint l = pub.p.l;
    var (t1, A) = (GoReflect.WithElemDims(pub.t1.slice(0, k), 256), GoReflect.WithElemDims(pub.a.slice(0, k * l), 256));

    nint β = p.τ * p.η;
    var γ1 = (uint32)(((uint32)1).Lsh((int64)(p.γ1)));
    var γ1β = γ1 - (uint32)β;

    var z = GoReflect.WithElemDims(new slice<ringElement>(l, maxL), 256);
    var h = GoReflect.WithElemDims(new slice<array<byte>>(k, () => new(256), maxK), 256);
    var (ch, err) = sigDecode(sig, z, h, p);
    if (err != default!) {
        return err;
    }

    var c = ntt(sampleInBall(ch, p));

    // w = Â ∘ NTT(z) − NTT(c) ∘ NTT(t₁ ⋅ 2ᵈ)
    var zHat = GoReflect.WithElemDims(new slice<nttElement>(l, maxL), 256);
    foreach (var (i, _) in zHat) {
        zHat[i] = ntt(z[i]);
    }
    var w = GoReflect.WithElemDims(new slice<ringElement>(k, maxK), 256);
    foreach (var (i, _) in w) {
        nttElement wHat = default!;
        foreach (var j in range(l)) {
            wHat = polyAdd(wHat, nttMul(A[i * l + j], zHat[j]));
        }
        wHat = polySub(wHat, nttMul(c, t1[i]));
        w[i] = inverseNTT(wHat);
    }

    // Use hints h to compute w₁ from w(approx).
    var w1 = GoReflect.WithElemDims(new slice<array<byte>>(k, () => new(256), maxK), 256);
    foreach (var (i, _) in w) {
        w1[i] = useHint(w[i], h[i], p);
    }

    var H = sha3.NewShake256();
    H.Write(μ[..]);
    foreach (var (i, _) in w) {
        w1Encode(H, w1[i], p);
    }
    var computedCH = new slice<byte>(p.λ / 4, maxλ / 4);
    H.Read(computedCH);

    foreach (var (i, _) in z) {
        if (coefficientsExceedBound(z[i], γ1β)) {
            return errInvalidSignatureCoeffBounds;
        }
    }

    if (!bytes.Equal(ch, computedCH)) {
        return errInvalidSignatureChallenge;
    }

    return default!;
}

internal static slice<byte> sigEncode(slice<byte> ch, slice<ringElement> z, slice<array<byte>> h, parameters p) {
    var sig = new slice<byte>(0, sigSize(p));
    sig = appendꓸꓸꓸ(sig, ch);
    foreach (var (i, _) in z) {
        sig = bitPack(sig, z[i], p);
    }
    sig = hintEncode(sig, h, p);
    return sig;
}

internal static (slice<byte> ch, error err) sigDecode(slice<byte> sig, slice<ringElement> z, slice<array<byte>> h, parameters p) {
    slice<byte> ch = default!;

    if (len(sig) != sigSize(p)) {
        return (default!, errInvalidSignatureLength);
    }
    (ch, sig) = (sig.slice(0, p.λ / 4), sig.slice(p.λ / 4));
    foreach (var (i, _) in z) {
        nint length = (p.γ1 + 1) * (nint)n / 8;
        z[i] = bitUnpack(sig.slice(0, length), p);
        sig = sig.slice(length);
    }
    {
        var errΔ1 = hintDecode(sig, h, p); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    return (ch, default!);
}

internal static slice<byte> hintEncode(slice<byte> buf, slice<array<byte>> h, parameters p) {
    nint ω = p.ω;
    nint k = p.k;
    var (@out, y) = sliceForAppend(buf, ω + k);
    byte idx = default!;
    foreach (var i in range(k)) {
        foreach (var j in range(n)) {
            if (h[i][j] != 0) {
                y[idx] = (byte)j;
                idx++;
            }
        }
        y[ω + i] = idx;
    }
    return @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mldsaInternalErrorˢ = "mldsa: internal error: invalid signature hint length"u8;

internal static error hintDecode(slice<byte> y, slice<array<byte>> h, parameters p) {
    nint ω = p.ω;
    nint k = p.k;
    if (len(y) != ω + k) {
        return errors.New(mldsaInternalErrorˢ);
    }
    byte idx = default!;
    foreach (var i in range(k)) {
        var limit = y[ω + i];
        if (limit < idx || limit > (byte)ω) {
            return errInvalidSignatureHintLimits;
        }
        var first = idx;
        while (idx < limit) {
            if (idx > first && y[(byte)(idx - 1)] >= y[idx]) {
                return errInvalidSignatureHintIndexOrder;
            }
            h[i][y[idx]] = 1;
            idx++;
        }
    }
    for (var i = idx; i < (byte)ω; i++) {
        if (y[i] != 0) {
            return errInvalidSignatureHintExtraIndices;
        }
    }
    return default!;
}

} // end mldsa_package
