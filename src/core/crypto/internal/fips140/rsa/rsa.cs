// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using bigmod = go.crypto.@internal.fips140.bigmod_package;
using errors = errors_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class rsa_package {

partial struct ΔPublicKey {
    public ж<bigmod.Modulus> N;
    public nint E;
}

// Size returns the modulus size in bytes. Raw signatures and ciphertexts
// for or by this public key will have the same size.
public static nint Size(this ref ΔPublicKey pub) {
    return (pub.N.BitLen() + 7) / 8;
}

partial struct PrivateKey {
    // pub has already been checked with checkPublicKey.
    internal ΔPublicKey pub;
    internal ж<bigmodꓸNat> d;
    // The following values are not set for deprecated multi-prime keys.
    //
    // Since they are always set for keys in FIPS mode, for SP 800-56B Rev. 2
    // purposes we always use the Chinese Remainder Theorem (CRT) format.
    internal ж<bigmod.Modulus> p, q; // p × q = n
    // dP and dQ are used as exponents, so we store them as big-endian byte
    // slices to be passed to [bigmod.Nat.Exp].
    internal slice<byte> dP; // d mod (p - 1)
    internal slice<byte> dQ; // d mod (q - 1)
    internal ж<bigmodꓸNat> qInv; // qInv = q⁻¹ mod p
    // fipsApproved is false if this key does not comply with FIPS 186-5 or
    // SP 800-56B Rev. 2.
    internal bool fipsApproved;
}

public static ж<ΔPublicKey> PublicKey(this ж<PrivateKey> Ꮡpriv) {
    return Ꮡpriv.of(PrivateKey.Ꮡpub);
}

// NewPrivateKey creates a new RSA private key from the given parameters.
//
// All values are in big-endian byte slice format, and may have leading zeros
// or be shorter if leading zeroes were trimmed.
public static (ж<PrivateKey>, error) NewPrivateKey(slice<byte> N, nint e, slice<byte> d, slice<byte> P, slice<byte> Q) {
    var (n, err) = bigmod.NewModulus(N);
    if (err != default!) {
        return (default!, err);
    }
    (var p, err) = bigmod.NewModulus(P);
    if (err != default!) {
        return (default!, err);
    }
    (var q, err) = bigmod.NewModulus(Q);
    if (err != default!) {
        return (default!, err);
    }
    (var dN, err) = bigmod.NewNat().SetBytes(d, n);
    if (err != default!) {
        return (default!, err);
    }
    return newPrivateKey(n, e, dN, p, q);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRsaPIsEvenˢ = "crypto/rsa: p is even"u8;

internal static (ж<PrivateKey>, error) newPrivateKey(ж<bigmod.Modulus> Ꮡn, nint e, ж<bigmodꓸNat> Ꮡd, ж<bigmod.Modulus> Ꮡp, ж<bigmod.Modulus> Ꮡq) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var q = ref Ꮡq.DerefOrNull();

    var pMinusOne = p.Nat().SubOne(Ꮡp);
    var (pMinusOneMod, err) = bigmod.NewModulus(pMinusOne.Bytes(Ꮡp));
    if (err != default!) {
        return (default!, err);
    }
    var dP = bigmod.NewNat().Mod(Ꮡd, pMinusOneMod).Bytes(pMinusOneMod);
    var qMinusOne = q.Nat().SubOne(Ꮡq);
    (var qMinusOneMod, err) = bigmod.NewModulus(qMinusOne.Bytes(Ꮡq));
    if (err != default!) {
        return (default!, err);
    }
    var dQ = bigmod.NewNat().Mod(Ꮡd, qMinusOneMod).Bytes(qMinusOneMod);
    // Constant-time modular inversion with prime modulus by Fermat's Little
    // Theorem: qInv = q⁻¹ mod p = q^(p-2) mod p.
    if (p.Nat().IsOdd() == 0) {
        // [bigmod.Nat.Exp] requires an odd modulus.
        return (default!, errors.New(cryptoRsaPIsEvenˢ));
    }
    var pMinusTwo = p.Nat().SubOne(Ꮡp).SubOne(Ꮡp).Bytes(Ꮡp);
    var qInv = bigmod.NewNat().Mod(q.Nat(), Ꮡp);
    qInv.Exp(qInv, pMinusTwo, Ꮡp);
    var pk = Ꮡ(new PrivateKey(
        pub: new ΔPublicKey(
            N: Ꮡn, E: e
        ),
        d: Ꮡd, p: Ꮡp, q: Ꮡq,
        dP: dP, dQ: dQ, qInv: qInv
    ));
    {
        var errΔ1 = checkPrivateKey(ref (pk).DerefOrNull()); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    return (pk, default!);
}

// NewPrivateKeyWithPrecomputation creates a new RSA private key from the given
// parameters, which include precomputed CRT values.
public static (ж<PrivateKey>, error) NewPrivateKeyWithPrecomputation(slice<byte> N, nint e, slice<byte> d, slice<byte> P, slice<byte> Q, slice<byte> dP, slice<byte> dQ, slice<byte> qInv) {
    var (n, err) = bigmod.NewModulus(N);
    if (err != default!) {
        return (default!, err);
    }
    (var p, err) = bigmod.NewModulus(P);
    if (err != default!) {
        return (default!, err);
    }
    (var q, err) = bigmod.NewModulus(Q);
    if (err != default!) {
        return (default!, err);
    }
    (var dN, err) = bigmod.NewNat().SetBytes(d, n);
    if (err != default!) {
        return (default!, err);
    }
    (var qInvNat, err) = bigmod.NewNat().SetBytes(qInv, p);
    if (err != default!) {
        return (default!, err);
    }
    var pk = Ꮡ(new PrivateKey(
        pub: new ΔPublicKey(
            N: n, E: e
        ),
        d: dN, p: p, q: q,
        dP: dP, dQ: dQ, qInv: qInvNat
    ));
    {
        var errΔ1 = checkPrivateKey(ref (pk).DerefOrNull()); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    return (pk, default!);
}

// NewPrivateKeyWithoutCRT creates a new RSA private key from the given parameters.
//
// This is meant for deprecated multi-prime keys, and is not FIPS 140 compliant.
public static (ж<PrivateKey>, error) NewPrivateKeyWithoutCRT(slice<byte> N, nint e, slice<byte> d) {
    var (n, err) = bigmod.NewModulus(N);
    if (err != default!) {
        return (default!, err);
    }
    (var dN, err) = bigmod.NewNat().SetBytes(d, n);
    if (err != default!) {
        return (default!, err);
    }
    var pk = Ꮡ(new PrivateKey(
        pub: new ΔPublicKey(
            N: n, E: e
        ),
        d: dN
    ));
    {
        var errΔ1 = checkPrivateKey(ref (pk).DerefOrNull()); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    return (pk, default!);
}

// Export returns the key parameters in big-endian byte slice format.
//
// P, Q, dP, dQ, and qInv may be nil if the key was created with
// NewPrivateKeyWithoutCRT.
public static (slice<byte> N, nint e, slice<byte> d, slice<byte> P, slice<byte> Q, slice<byte> dP, slice<byte> dQ, slice<byte> qInv) Export(this ref PrivateKey priv) {
    slice<byte> N = default!;
    nint e = default!;
    slice<byte> d = default!;
    slice<byte> P = default!;
    slice<byte> Q = default!;
    slice<byte> dP = default!;
    slice<byte> dQ = default!;
    slice<byte> qInv = default!;

    N = priv.pub.N.Nat().Bytes(priv.pub.N);
    e = priv.pub.E;
    d = priv.d.Bytes(priv.pub.N);
    if (priv.dP == default!) {
        return (N, e, d, P, Q, dP, dQ, qInv);
    }
    P = priv.p.Nat().Bytes(priv.p);
    Q = priv.q.Nat().Bytes(priv.q);
    dP = bytes.Clone(priv.dP);
    dQ = bytes.Clone(priv.dQ);
    qInv = priv.qInv.Bytes(priv.p);
    return (N, e, d, P, Q, dP, dQ, qInv);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRsaInvalidPrimeˢ = "crypto/rsa: invalid prime"u8;
internal static readonly @string cryptoRsaPQNˢ = "crypto/rsa: p * q != n"u8;
internal static readonly @string cryptoRsaInvalidCrtˢ = "crypto/rsa: invalid CRT exponent"u8;
internal static readonly @string cryptoRsaInvalidCrtˢ2 = "crypto/rsa: invalid CRT coefficient"u8;
internal static readonly @string cryptoRsaPQˢ = "crypto/rsa: p == q"u8;
internal static readonly @string cryptoRsaPQTooSmallˢ = "crypto/rsa: |p - q| too small"u8;
internal static readonly @string cryptoRsaDTooSmallˢ = "crypto/rsa: d too small"u8;

// checkPrivateKey is called by the NewPrivateKey and GenerateKey functions, and
// is allowed to modify priv.fipsApproved.
internal static error checkPrivateKey(ref PrivateKey priv) {
    priv.fipsApproved = true;
    {
        var (fipsApproved, errΔ1) = checkPublicKey(ref nonnil(ref priv).pub); if (errΔ1 != default!){
            return errΔ1;
        } else 
        if (!fipsApproved) {
            priv.fipsApproved = false;
        }
    }
    if (priv.dP == default!) {
        // Legacy and deprecated multi-prime keys.
        priv.fipsApproved = false;
        return default!;
    }
    var N = priv.pub.N;
    var p = priv.p;
    var q = priv.q;
    // FIPS 186-5, Section 5.1 requires "that p and q be of the same bit length."
    if (p.BitLen() != q.BitLen()) {
        priv.fipsApproved = false;
    }
    // Check that pq ≡ 1 mod N (and that p < N and q < N).
    var pN = bigmod.NewNat().ExpandFor(N);
    {
        var (_, errΔ2) = pN.SetBytes(p.Nat().Bytes(p), N); if (errΔ2 != default!) {
            return errors.New(cryptoRsaInvalidPrimeˢ);
        }
    }
    var qN = bigmod.NewNat().ExpandFor(N);
    {
        var (_, errΔ3) = qN.SetBytes(q.Nat().Bytes(q), N); if (errΔ3 != default!) {
            return errors.New(cryptoRsaInvalidPrimeˢ);
        }
    }
    if (pN.Mul(qN, N).IsZero() != 1) {
        return errors.New(cryptoRsaPQNˢ);
    }
    // Check that de ≡ 1 mod p-1, and de ≡ 1 mod q-1.
    //
    // This implies that e is coprime to each p-1 as e has a multiplicative
    // inverse. Therefore e is coprime to lcm(p-1,q-1) = λ(N).
    // It also implies that a^de ≡ a mod p as a^(p-1) ≡ 1 mod p. Thus a^de ≡ a
    // mod n for all a coprime to n, as required.
    //
    // This checks dP, dQ, and e. We don't check d because it is not actually
    // used in the RSA private key operation.
    var (pMinus1, err) = bigmod.NewModulus(p.Nat().SubOne(p).Bytes(p));
    if (err != default!) {
        return errors.New(cryptoRsaInvalidPrimeˢ);
    }
    (var dP, err) = bigmod.NewNat().SetBytes(priv.dP, pMinus1);
    if (err != default!) {
        return errors.New(cryptoRsaInvalidCrtˢ);
    }
    var de = bigmod.NewNat();
    de.SetUint((nuint)priv.pub.E).ExpandFor(pMinus1);
    de.Mul(dP, pMinus1);
    if (de.IsOne() != 1) {
        return errors.New(cryptoRsaInvalidCrtˢ);
    }
    (var qMinus1, err) = bigmod.NewModulus(q.Nat().SubOne(q).Bytes(q));
    if (err != default!) {
        return errors.New(cryptoRsaInvalidPrimeˢ);
    }
    (var dQ, err) = bigmod.NewNat().SetBytes(priv.dQ, qMinus1);
    if (err != default!) {
        return errors.New(cryptoRsaInvalidCrtˢ);
    }
    de.SetUint((nuint)priv.pub.E).ExpandFor(qMinus1);
    de.Mul(dQ, qMinus1);
    if (de.IsOne() != 1) {
        return errors.New(cryptoRsaInvalidCrtˢ);
    }
    // Check that qInv * q ≡ 1 mod p.
    (var qP, err) = bigmod.NewNat().SetOverflowingBytes(q.Nat().Bytes(q), p);
    if (err != default!) {
        // q >= 2^⌈log2(p)⌉
        qP = bigmod.NewNat().Mod(q.Nat(), p);
    }
    if (qP.Mul(priv.qInv, p).IsOne() != 1) {
        return errors.New(cryptoRsaInvalidCrtˢ2);
    }
    // Check that |p - q| > 2^(nlen/2 - 100).
    //
    // If p and q are very close to each other, then N=pq can be trivially
    // factored using Fermat's factorization method. Broken RSA implementations
    // do generate such keys. See Hanno Böck, Fermat Factorization in the Wild,
    // https://eprint.iacr.org/2023/026.pdf.
    var diff = bigmod.NewNat();
    {
        var (qPΔ1, errΔ4) = bigmod.NewNat().SetBytes(q.Nat().Bytes(q), p); if (errΔ4 != default!){
            // q > p
            var (pQ, errΔ5) = bigmod.NewNat().SetBytes(p.Nat().Bytes(p), q);
            if (errΔ5 != default!) {
                return errors.New(cryptoRsaPQˢ);
            }
            // diff = 0 - p mod q = q - p
            diff.ExpandFor(q).Sub(pQ, q);
        } else {
            // p > q
            // diff = 0 - q mod p = p - q
            diff.ExpandFor(p).Sub(qPΔ1, p);
        }
    }
    // A tiny bit of leakage is acceptable because it's not adaptive, an
    // attacker only learns the magnitude of p - q.
    if (diff.BitLenVarTime() <= N.BitLen() / 2 - 100) {
        return errors.New(cryptoRsaPQTooSmallˢ);
    }
    // Check that d > 2^(nlen/2).
    //
    // See section 3 of https://crypto.stanford.edu/~dabo/papers/RSA-survey.pdf
    // for more details about attacks on small d values.
    //
    // Likewise, the leakage of the magnitude of d is not adaptive.
    if (priv.d.BitLenVarTime() <= N.BitLen() / 2) {
        return errors.New(cryptoRsaDTooSmallˢ);
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRsaMissingPublicˢ = "crypto/rsa: missing public modulus"u8;
internal static readonly @string cryptoRsaPublicModulusIsˢ = "crypto/rsa: public modulus is even"u8;
internal static readonly @string cryptoRsaPublicExponentˢ = "crypto/rsa: public exponent too small or negative"u8;
internal static readonly @string cryptoRsaPublicExponentˢ2 = "crypto/rsa: public exponent is even"u8;
internal static readonly @string cryptoRsaPublicExponentˢ3 = "crypto/rsa: public exponent too large"u8;

internal static (bool fipsApproved, error err) checkPublicKey(ref ΔPublicKey pub) {
    bool fipsApproved = default!;

    fipsApproved = true;
    if (pub.N == nil) {
        return (false, errors.New(cryptoRsaMissingPublicˢ));
    }
    if (pub.N.Nat().IsOdd() == 0) {
        return (false, errors.New(cryptoRsaPublicModulusIsˢ));
    }
    // FIPS 186-5, Section 5.1: "This standard specifies the use of a modulus
    // whose bit length is an even integer and greater than or equal to 2048
    // bits."
    if (pub.N.BitLen() < 2048) {
        fipsApproved = false;
    }
    if (pub.N.BitLen() % 2 == 1) {
        fipsApproved = false;
    }
    if (pub.E < 2) {
        return (false, errors.New(cryptoRsaPublicExponentˢ));
    }
    // e needs to be coprime with p-1 and q-1, since it must be invertible
    // modulo λ(pq). Since p and q are prime, this means e needs to be odd.
    if ((nint)(pub.E & 1) == 0) {
        return (false, errors.New(cryptoRsaPublicExponentˢ2));
    }
    // FIPS 186-5, Section 5.5(e): "The exponent e shall be an odd, positive
    // integer such that 2¹⁶ < e < 2²⁵⁶."
    if (pub.E <= (1 << (int)(16))) {
        fipsApproved = false;
    }
    // We require pub.E to fit into a 32-bit integer so that we
    // do not have different behavior depending on whether
    // int is 32 or 64 bits. See also
    // https://www.imperialviolet.org/2012/03/16/rsae.html.
    if (pub.E > (nint)(2147483648L - 1)) {
        return (false, errors.New(cryptoRsaPublicExponentˢ3));
    }
    return (fipsApproved, default!);
}

// Encrypt performs the RSA public key operation.
public static (slice<byte>, error) Encrypt(ж<ΔPublicKey> Ꮡpub, slice<byte> plaintext) {
    fips140.RecordNonApproved();
    {
        var (_, err) = checkPublicKey(ref (Ꮡpub).DerefOrNull()); if (err != default!) {
            return (default!, err);
        }
    }
    return encrypt(ref (Ꮡpub).DerefOrNull(), plaintext);
}

internal static (slice<byte>, error) encrypt(ref ΔPublicKey pub, slice<byte> plaintext) {
    var (m, err) = bigmod.NewNat().SetBytes(plaintext, pub.N);
    if (err != default!) {
        return (default!, err);
    }
    return (bigmod.NewNat().ExpShortVarTime(m, (nuint)pub.E, pub.N).Bytes(pub.N), default!);
}

public static error ErrMessageTooLong = errors.New("crypto/rsa: message too long for RSA key size"u8);

public static error ErrDecryption = errors.New("crypto/rsa: decryption error"u8);

public static error ErrVerification = errors.New("crypto/rsa: verification error"u8);

internal const bool withCheck = true;

internal const bool noCheck = false;

// DecryptWithoutCheck performs the RSA private key operation.
public static (slice<byte>, error) DecryptWithoutCheck(ж<PrivateKey> Ꮡpriv, slice<byte> ciphertext) {
    fips140.RecordNonApproved();
    return decrypt(ref (Ꮡpriv).DerefOrNull(), ciphertext, noCheck);
}

// DecryptWithCheck performs the RSA private key operation and checks the
// result to defend against errors in the CRT computation.
public static (slice<byte>, error) DecryptWithCheck(ж<PrivateKey> Ꮡpriv, slice<byte> ciphertext) {
    fips140.RecordNonApproved();
    return decrypt(ref (Ꮡpriv).DerefOrNull(), ciphertext, withCheck);
}

// decrypt performs an RSA decryption of ciphertext into out. If check is true,
// m^e is calculated and compared with ciphertext, in order to defend against
// errors in the CRT computation.
internal static (slice<byte>, error) decrypt(ref PrivateKey priv, slice<byte> ciphertext, bool check) {
    if (!priv.fipsApproved) {
        fips140.RecordNonApproved();
    }
    ж<bigmodꓸNat> m = default!;
    var N = priv.pub.N;
    nint E = priv.pub.E;
    var (c, err) = bigmod.NewNat().SetBytes(ciphertext, N);
    if (err != default!) {
        return (default!, ErrDecryption);
    }
    if (priv.dP == default!){
        // Legacy codepath for deprecated multi-prime keys.
        fips140.RecordNonApproved();
        m = bigmod.NewNat().Exp(c, priv.d.Bytes(N), N);
    } else {
        var (P, Q) = (priv.p, priv.q);
        var t0 = bigmod.NewNat();
        // m = c ^ Dp mod p
        m = bigmod.NewNat().Exp(t0.Mod(c, P), priv.dP, P);
        // m2 = c ^ Dq mod q
        var m2 = bigmod.NewNat().Exp(t0.Mod(c, Q), priv.dQ, Q);
        // m = m - m2 mod p
        m.Sub(t0.Mod(m2, P), P);
        // m = m * Qinv mod p
        m.Mul(priv.qInv, P);
        // m = m * q mod N
        m.ExpandFor(N).Mul(t0.Mod(Q.Nat(), N), N);
        // m = m + m2 mod N
        m.Add(m2.ExpandFor(N), N);
    }
    if (check) {
        var c1 = bigmod.NewNat().ExpShortVarTime(m, (nuint)E, N);
        if (c1.Equal(c) != 1) {
            return (default!, ErrDecryption);
        }
    }
    return (m.Bytes(N), default!);
}

} // end rsa_package
