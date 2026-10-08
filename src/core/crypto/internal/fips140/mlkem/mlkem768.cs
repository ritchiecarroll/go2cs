// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package mlkem implements the quantum-resistant key encapsulation method
// ML-KEM (formerly known as Kyber), as specified in [NIST FIPS 203].
//
// [NIST FIPS 203]: https://doi.org/10.6028/NIST.FIPS.203
namespace go.crypto.@internal.fips140;

// This package targets security, correctness, simplicity, readability, and
// reviewability as its primary goals. All critical operations are performed in
// constant time.
//
// Variable and function names, as well as code layout, are selected to
// facilitate reviewing the implementation against the NIST FIPS 203 document.
//
// Reviewers unfamiliar with polynomials or linear algebra might find the
// background at https://words.filippo.io/kyber-math/ useful.
//
// This file implements the recommended parameter set ML-KEM-768. The ML-KEM-1024
// parameter set implementation is auto-generated from this file.
//
//go:generate go run generate1024.go -input mlkem768.go -output mlkem1024.go
using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using errors = errors_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class mlkem_package {

internal static UntypedInt n => 256;
internal static UntypedInt q => 3329;
internal static UntypedInt encodingSize12 => /* n * 12 / 8 */ 384;
internal static UntypedInt encodingSize11 => /* n * 11 / 8 */ 352;
internal static UntypedInt encodingSize10 => /* n * 10 / 8 */ 320;
internal static UntypedInt encodingSize5 => /* n * 5 / 8 */ 160;
internal static UntypedInt encodingSize4 => /* n * 4 / 8 */ 128;
internal static UntypedInt encodingSize1 => /* n * 1 / 8 */ 32;
internal static UntypedInt messageSize => /* encodingSize1 */ 32;
public static UntypedInt SharedKeySize => 32;
public static UntypedInt SeedSize => /* 32 + 32 */ 64;

// ML-KEM-768 parameters.
internal static UntypedInt k => 3;

public static UntypedInt CiphertextSize768 => /* k*encodingSize10 + encodingSize4 */ 1088;

public static UntypedInt EncapsulationKeySize768 => /* k*encodingSize12 + 32 */ 1184;

internal static UntypedInt decapsulationKeySize768 => /* k*encodingSize12 + EncapsulationKeySize768 + 32 + 32 */ 2400;

// ML-KEM-1024 parameters.
internal static UntypedInt k1024 => 4;

public static UntypedInt CiphertextSize1024 => /* k1024*encodingSize11 + encodingSize5 */ 1568;

public static UntypedInt EncapsulationKeySize1024 => /* k1024*encodingSize12 + 32 */ 1568;

internal static UntypedInt decapsulationKeySize1024 => /* k1024*encodingSize12 + EncapsulationKeySize1024 + 32 + 32 */ 3168;

// A DecapsulationKey768 is the secret key used to decapsulate a shared key from a
// ciphertext. It includes various precomputed values.
partial struct DecapsulationKey768 {
    internal array<byte> d = new(32); // decapsulation key seed
    internal array<byte> z = new(32); // implicit rejection sampling seed
    internal array<byte> ρ = new(32); // sampleNTT seed for A, stored for the encapsulation key
    internal array<byte> h = new(32); // H(ek), stored for ML-KEM.Decaps_internal
    internal partial ref encryptionKey encryptionKey { get; }
    internal partial ref decryptionKey decryptionKey { get; }
}

// Bytes returns the decapsulation key as a 64-byte seed in the "d || z" form.
//
// The decapsulation key must be kept secret.
public static slice<byte> Bytes(this ref DecapsulationKey768 dk) {
    array<byte> b = new(64); /* SeedSize */
    copy(b[..], dk.d[..]);
    copy(b[32..], dk.z[..]);
    return b[..];
}

// TestingOnlyExpandedBytes768 returns the decapsulation key as a byte slice
// using the full expanded NIST encoding.
//
// This should only be used for ACVP testing. For all other purposes prefer
// the Bytes method that returns the (much smaller) seed.
public static slice<byte> TestingOnlyExpandedBytes768(ж<DecapsulationKey768> Ꮡdk) {
    ref var dk = ref Ꮡdk.DerefOrNull();

    var b = new slice<byte>(0, decapsulationKeySize768);
    // ByteEncode₁₂(s)
    foreach (var (i, _) in dk.s) {
        b = polyByteEncode(b, dk.s[i]);
    }
    // ByteEncode₁₂(t) || ρ
    foreach (var (i, _) in dk.t) {
        b = polyByteEncode(b, dk.t[i]);
    }
    b = appendꓸꓸꓸ(b, dk.ρ[..]);
    // H(ek) || z
    b = appendꓸꓸꓸ(b, dk.h[..]);
    b = appendꓸꓸꓸ(b, dk.z[..]);
    return b;
}

// EncapsulationKey returns the public encapsulation key necessary to produce
// ciphertexts.
public static ж<EncapsulationKey768> EncapsulationKey(this ref DecapsulationKey768 dk) {
    return Ꮡ(new EncapsulationKey768(
        ρ: dk.ρ.Clone(),
        h: dk.h.Clone(),
        encryptionKey: dk.encryptionKey.ΔClone()
    ));
}

// An EncapsulationKey768 is the public key used to produce ciphertexts to be
// decapsulated by the corresponding [DecapsulationKey768].
partial struct EncapsulationKey768 {
    internal array<byte> ρ = new(32); // sampleNTT seed for A
    internal array<byte> h = new(32); // H(ek)
    internal partial ref encryptionKey encryptionKey { get; }
}

// Bytes returns the encapsulation key as a byte slice.
public static slice<byte> Bytes(this ref EncapsulationKey768 ek) {
    // The actual logic is in a separate function to outline this allocation.
    var b = new slice<byte>(0, EncapsulationKeySize768);
    return ek.bytes(b);
}

internal static slice<byte> bytes(this ref EncapsulationKey768 ek, slice<byte> b) {
    foreach (var (i, _) in ek.t) {
        b = polyByteEncode(b, ek.t[i]);
    }
    b = appendꓸꓸꓸ(b, ek.ρ[..]);
    return b;
}

// encryptionKey is the parsed and expanded form of a PKE encryption key.
partial struct encryptionKey {
    internal array<nttElement> t = new(k); // ByteDecode₁₂(ek[:384k])
    internal array<nttElement> a = new(k * k); // A[i*k+j] = sampleNTT(ρ, j, i)
}

// decryptionKey is the parsed and expanded form of a PKE decryption key.
partial struct decryptionKey {
    internal array<nttElement> s = new(k); // ByteDecode₁₂(dk[:decryptionKeySize])
}

// GenerateKey768 generates a new decapsulation key, drawing random bytes from
// a DRBG. The decapsulation key must be kept secret.
public static (ж<DecapsulationKey768>, error) GenerateKey768() {
    // The actual logic is in a separate function to outline this allocation.
    var dk = Ꮡ(new DecapsulationKey768(nil));
    return generateKey(dk);
}

internal static (ж<DecapsulationKey768>, error) generateKey(ж<DecapsulationKey768> Ꮡdk) {
    ref var d = ref heap(new array<byte>(32), out var Ꮡd);
    drbg.Read(d[..]);
    ref var z = ref heap(new array<byte>(32), out var Ꮡz);
    drbg.Read(z[..]);
    kemKeyGen(Ꮡdk, Ꮡd, ref z);
    fips140.PCT(mlKemPctˢ, () => kemPCT(Ꮡdk));
    fips140.RecordApproved();
    return (Ꮡdk, default!);
}

// GenerateKeyInternal768 is a derandomized version of GenerateKey768,
// exclusively for use in tests.
public static ж<DecapsulationKey768> GenerateKeyInternal768(/*[32]*/ ж<array<byte>> Ꮡd, /*[32]*/ ж<array<byte>> Ꮡz) {
    var dk = Ꮡ(new DecapsulationKey768(nil));
    kemKeyGen(dk, Ꮡd, ref (Ꮡz).DerefOrNull());
    return dk;
}

// NewDecapsulationKey768 parses a decapsulation key from a 64-byte
// seed in the "d || z" form. The seed must be uniformly random.
public static (ж<DecapsulationKey768>, error) NewDecapsulationKey768(slice<byte> seed) {
    // The actual logic is in a separate function to outline this allocation.
    var dk = Ꮡ(new DecapsulationKey768(nil));
    return newKeyFromSeed(dk, seed);
}

internal static (ж<DecapsulationKey768>, error) newKeyFromSeed(ж<DecapsulationKey768> Ꮡdk, slice<byte> seed) {
    if (len(seed) != SeedSize) {
        return (default!, errors.New(mlkemInvalidSeedLengthˢ));
    }
    var d = Ꮡ(array<byte>.Alias(seed[..32], 32));
    var z = Ꮡ(array<byte>.Alias(seed[32..], 32));
    kemKeyGen(Ꮡdk, d, ref (z).DerefOrNull());
    fips140.RecordApproved();
    return (Ꮡdk, default!);
}

// TestingOnlyNewDecapsulationKey768 parses a decapsulation key from its expanded NIST format.
//
// Bytes() must not be called on the returned key, as it will not produce the
// original seed.
//
// This function should only be used for ACVP testing. Prefer NewDecapsulationKey768 for all
// other purposes.
public static (ж<DecapsulationKey768>, error) TestingOnlyNewDecapsulationKey768(slice<byte> b) {
    if (len(b) != decapsulationKeySize768) {
        return (default!, errors.New(mlkemInvalidNistˢ));
    }
    var dk = Ꮡ(new DecapsulationKey768(nil));
    foreach (var (i, _) in (~dk).s) {
        error errΔ1 = default!;
        (dk.Value.s[i], errΔ1) = polyByteDecode<nttElement>(b[..(int)(encodingSize12)]);
        if (errΔ1 != default!) {
            return (default!, errors.New(mlkemInvalidSecretKeyˢ));
        }
        b = b[(int)(encodingSize12)..];
    }
    var (ek, err) = NewEncapsulationKey768(b[..(int)(EncapsulationKeySize768)]);
    if (err != default!) {
        return (default!, err);
    }
    dk.Value.ρ = ek.Value.ρ.Clone();
    dk.Value.h = ek.Value.h.Clone();
    dk.Value.encryptionKey = ek.Value.encryptionKey.ΔClone();
    b = b[(int)(EncapsulationKeySize768)..];
    if (!bytes_package.Equal((~dk).h[..], b[..32])) {
        return (default!, errors.New(mlkemInconsistentHEkInˢ));
    }
    b = b[32..];
    copy((~dk).z[..], b);
    // Generate a random d value for use in Bytes(). This is a safety mechanism
    // that avoids returning a broken key vs a random key if this function is
    // called in contravention of the TestingOnlyNewDecapsulationKey768 function
    // comment advising against it.
    drbg.Read((~dk).d[..]);
    return (dk, default!);
}

// kemKeyGen generates a decapsulation key.
//
// It implements ML-KEM.KeyGen_internal according to FIPS 203, Algorithm 16, and
// K-PKE.KeyGen according to FIPS 203, Algorithm 13. The two are merged to save
// copies and allocations.
internal static void kemKeyGen(ж<DecapsulationKey768> Ꮡdk, /*[32]*/ ж<array<byte>> Ꮡd, ref array<byte> z) {
    ref var dk = ref Ꮡdk.DerefOrNull();
    ref var d = ref Ꮡd.DerefOrNull();

    dk.d = d.Clone();
    dk.z = z.Clone();
    var g = sha3.New512();
    g.Write(d[..]);
    g.Write(new byte[]{k}.slice()); // Module dimension as a domain separator.
    var G = g.Sum(new slice<byte>(0, 64));
    var (ρ, σ) = (G[..32], G[32..]);
    dk.ρ = new array<byte>(ρ, 32);
    var A = Ꮡdk.of(DecapsulationKey768.Ꮡa);
    for (var i = (byte)0; i < k; i++) {
        for (var j = (byte)0; j < k; j++) {
            A.Value[(byte)(i * (byte)k + j)] = sampleNTT(ρ, j, i);
        }
    }
    byte N = default!;
    var s = Ꮡdk.of(DecapsulationKey768.Ꮡs);
    foreach (var (i, _) in s.Value) {
        s.Value[i] = ntt(samplePolyCBD(σ, N));
        N++;
    }
    var e = GoReflect.WithElemDims(new slice<nttElement>(k), 256);
    foreach (var (i, _) in e) {
        e[i] = ntt(samplePolyCBD(σ, N));
        N++;
    }
    var t = Ꮡdk.of(DecapsulationKey768.Ꮡt);
    foreach (var (i, _) in t.Value) {
        // t = A ◦ s + e
        t.Value[i] = e[i].Clone();
        foreach (var (j, _) in s.Value) {
            t.Value[i] = polyAdd<nttElement>(t.Value[i], nttMul(A.Value[i * (nint)k + j], s.Value[j]));
        }
    }
    var H = sha3.New256();
    var ek = dk.EncapsulationKey().Bytes();
    H.Write(ek);
    H.Sum(dk.h[..0]);
}

// kemPCT performs a Pairwise Consistency Test per FIPS 140-3 IG 10.3.A
// Additional Comment 1: "For key pairs generated for use with approved KEMs in
// FIPS 203, the PCT shall consist of applying the encapsulation key ek to
// encapsulate a shared secret K leading to ciphertext c, and then applying
// decapsulation key dk to retrieve the same shared secret K. The PCT passes if
// the two shared secret K values are equal. The PCT shall be performed either
// when keys are generated/imported, prior to the first exportation, or prior to
// the first operational use (if not exported before the first use)."
internal static error kemPCT(ж<DecapsulationKey768> Ꮡdk) {
    ref var dk = ref Ꮡdk.DerefOrNull();

    var ek = dk.EncapsulationKey();
    var (K, c) = ek.Encapsulate();
    var (K1, err) = Ꮡdk.Decapsulate(c);
    if (err != default!) {
        return err;
    }
    if (subtle.ConstantTimeCompare(K, K1) != 1) {
        return errors.New(mlkemPctFailedˢ);
    }
    return default!;
}

// Encapsulate generates a shared key and an associated ciphertext from an
// encapsulation key, drawing random bytes from a DRBG.
//
// The shared key must be kept secret.
public static (slice<byte> sharedKey, slice<byte> ciphertext) Encapsulate(this ж<EncapsulationKey768> Ꮡek) {
    // The actual logic is in a separate function to outline this allocation.
    ref var cc = ref heap(new array<byte>(1088), out var Ꮡcc);
    return Ꮡek.encapsulate(Ꮡcc);
}

internal static (slice<byte> sharedKey, slice<byte> ciphertext) encapsulate(this ж<EncapsulationKey768> Ꮡek, /*[1088]*/ ж<array<byte>> Ꮡcc) {
    ref var m = ref heap(new array<byte>(32), out var Ꮡm);
    drbg.Read(m[..]);
    // Note that the modulus check (step 2 of the encapsulation key check from
    // FIPS 203, Section 7.2) is performed by polyByteDecode in parseEK.
    fips140.RecordApproved();
    return kemEncaps(Ꮡcc, ref (Ꮡek).DerefOrNull(), Ꮡm);
}

// EncapsulateInternal is a derandomized version of Encapsulate, exclusively for
// use in tests.
public static (slice<byte> sharedKey, slice<byte> ciphertext) EncapsulateInternal(this ж<EncapsulationKey768> Ꮡek, /*[32]*/ ж<array<byte>> Ꮡm) {
    var cc = Ꮡ(new byte[]{}.array(1088));
    return kemEncaps(cc, ref (Ꮡek).DerefOrNull(), Ꮡm);
}

// kemEncaps generates a shared key and an associated ciphertext.
//
// It implements ML-KEM.Encaps_internal according to FIPS 203, Algorithm 17.
internal static (slice<byte> K, slice<byte> c) kemEncaps(/*[1088]*/ ж<array<byte>> Ꮡcc, ref EncapsulationKey768 ek, /*[32]*/ ж<array<byte>> Ꮡm) {
    slice<byte> K = default!;
    slice<byte> c = default!;

    ref var m = ref Ꮡm.DerefOrNull();
    var g = sha3.New512();
    g.Write(m[..]);
    g.Write(ek.h[..]);
    var G = g.Sum(default!);
    K = G[..(int)(SharedKeySize)];
    var r = G[(int)(SharedKeySize)..];
    c = pkeEncrypt(Ꮡcc, ref nonnil(ref ek).encryptionKey, Ꮡm, r);
    return (K, c);
}

// NewEncapsulationKey768 parses an encapsulation key from its encoded form.
// If the encapsulation key is not valid, NewEncapsulationKey768 returns an error.
public static (ж<EncapsulationKey768>, error) NewEncapsulationKey768(slice<byte> encapsulationKey) {
    // The actual logic is in a separate function to outline this allocation.
    var ek = Ꮡ(new EncapsulationKey768(nil));
    return parseEK(ek, encapsulationKey);
}

// parseEK parses an encryption key from its encoded form.
//
// It implements the initial stages of K-PKE.Encrypt according to FIPS 203,
// Algorithm 14.
internal static (ж<EncapsulationKey768>, error) parseEK(ж<EncapsulationKey768> Ꮡek, slice<byte> ekPKE) {
    ref var ek = ref Ꮡek.DerefOrNull();

    if (len(ekPKE) != EncapsulationKeySize768) {
        return (default!, errors.New(mlkemInvalidˢ));
    }
    var h = sha3.New256();
    h.Write(ekPKE);
    h.Sum(ek.h[..0]);
    foreach (var (i, _) in ek.t) {
        error err = default!;
        (ek.t[i], err) = polyByteDecode<nttElement>(ekPKE[..(int)(encodingSize12)]);
        if (err != default!) {
            return (default!, err);
        }
        ekPKE = ekPKE[(int)(encodingSize12)..];
    }
    copy(ek.ρ[..], ekPKE);
    for (var i = (byte)0; i < k; i++) {
        for (var j = (byte)0; j < k; j++) {
            ek.a[(byte)(i * (byte)k + j)] = sampleNTT(ek.ρ[..], j, i);
        }
    }
    return (Ꮡek, default!);
}

// pkeEncrypt encrypt a plaintext message.
//
// It implements K-PKE.Encrypt according to FIPS 203, Algorithm 14, although the
// computation of t and AT is done in parseEK.
internal static slice<byte> pkeEncrypt(/*[1088]*/ ж<array<byte>> Ꮡcc, ref encryptionKey ex, /*[32]*/ ж<array<byte>> Ꮡm, slice<byte> rnd) {
    ref var cc = ref Ꮡcc.DerefOrNull();

    byte N = default!;
    var (r, e1) = (GoReflect.WithElemDims(new slice<nttElement>(k), 256), GoReflect.WithElemDims(new slice<ringElement>(k), 256));
    foreach (var (i, _) in r) {
        r[i] = ntt(samplePolyCBD(rnd, N));
        N++;
    }
    foreach (var (i, _) in e1) {
        e1[i] = samplePolyCBD(rnd, N);
        N++;
    }
    var e2 = samplePolyCBD(rnd, N);
    var u = GoReflect.WithElemDims(new slice<ringElement>(k), 256); // NTT⁻¹(AT ◦ r) + e1
    foreach (var (i, _) in u) {
        u[i] = e1[i].Clone();
        foreach (var (j, _) in r) {
            // Note that i and j are inverted, as we need the transposed of A.
            u[i] = polyAdd<ringElement>(u[i], inverseNTT(nttMul(ex.a[j * (nint)k + i], r[j])));
        }
    }
    var μ = ringDecodeAndDecompress1(ref (Ꮡm).DerefOrNull());
    nttElement vNTT = default!;                  // t⊺ ◦ r
    foreach (var (i, _) in ex.t) {
        vNTT = polyAdd(vNTT, nttMul(ex.t[i], r[i]));
    }
    var v = polyAdd(polyAdd(inverseNTT(vNTT), e2), μ);
    var c = cc[..0];
    foreach (var (_, vᴛ1) in u) {
        var f = vᴛ1.Clone();

        c = ringCompressAndEncode10(c, f);
    }
    c = ringCompressAndEncode4(c, v);
    return c;
}

// Decapsulate generates a shared key from a ciphertext and a decapsulation key.
// If the ciphertext is not valid, Decapsulate returns an error.
//
// The shared key must be kept secret.
public static (slice<byte> sharedKey, error err) Decapsulate(this ж<DecapsulationKey768> Ꮡdk, slice<byte> ciphertext) {
    if (len(ciphertext) != CiphertextSize768) {
        return (default!, errors.New(mlkemInvalidCiphertextˢ));
    }
    var c = Ꮡ(array<byte>.Alias(ciphertext, 1088));
    // Note that the hash check (step 3 of the decapsulation input check from
    // FIPS 203, Section 7.3) is foregone as a DecapsulationKey is always
    // validly generated by ML-KEM.KeyGen_internal.
    return (kemDecaps(ref (Ꮡdk).DerefOrNull(), c), default!);
}

// kemDecaps produces a shared key from a ciphertext.
//
// It implements ML-KEM.Decaps_internal according to FIPS 203, Algorithm 18.
internal static slice<byte> /*K*/ kemDecaps(ref DecapsulationKey768 dk, /*[1088]*/ ж<array<byte>> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    fips140.RecordApproved();
    var m = pkeDecrypt(ref nonnil(ref dk).decryptionKey, Ꮡc);
    var g = sha3.New512();
    g.Write(m[..]);
    g.Write(dk.h[..]);
    var G = g.Sum(new slice<byte>(0, 64));
    var (Kprime, r) = (G[..(int)(SharedKeySize)], G[(int)(SharedKeySize)..]);
    var J = sha3.NewShake256();
    J.Write(dk.z[..]);
    J.Write(c[..]);
    var Kout = new slice<byte>(SharedKeySize);
    J.Read(Kout);
    ref var cc = ref heap(new array<byte>(1088), out var Ꮡcc);
    var c1 = pkeEncrypt(Ꮡcc, ref nonnil(ref dk).encryptionKey, Ꮡ(array<byte>.Alias(m, 32)), r);
    subtle.ConstantTimeCopy(subtle.ConstantTimeCompare(c[..], c1), Kout, Kprime);
    return Kout;
}

// pkeDecrypt decrypts a ciphertext.
//
// It implements K-PKE.Decrypt according to FIPS 203, Algorithm 15,
// although s is retained from kemKeyGen.
internal static slice<byte> pkeDecrypt(ref decryptionKey dx, /*[1088]*/ ж<array<byte>> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var u = GoReflect.WithElemDims(new slice<ringElement>(k), 256);
    foreach (var (i, _) in u) {
        var bΔ1 = Ꮡ(array<byte>.Alias(c.slice((nint)encodingSize10 * i, (nint)encodingSize10 * (i + 1)), 320));
        u[i] = ringDecodeAndDecompress10(bΔ1);
    }
    var b = Ꮡ(array<byte>.Alias(c[(int)(encodingSize10 * k)..], 128));
    var v = ringDecodeAndDecompress4(ref (b).DerefOrNull());
    nttElement mask = default!;                  // s⊺ ◦ NTT(u)
    foreach (var (i, _) in dx.s) {
        mask = polyAdd(mask, nttMul(dx.s[i], ntt(u[i])));
    }
    var w = polySub(v, inverseNTT(mask));
    return ringCompressAndEncode1(default!, w);
}

} // end mlkem_package
