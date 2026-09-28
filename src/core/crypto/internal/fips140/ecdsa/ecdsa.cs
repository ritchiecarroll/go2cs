// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using bigmod = go.crypto.@internal.fips140.bigmod_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using Δnistec = go.crypto.@internal.fips140.nistec_package;
using errors = errors_package;
using io = io_package;
using sync = sync_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class ecdsa_package {

// PrivateKey and PublicKey are not generic to make it possible to use them
// in other types without instantiating them with a specific point type.
// They are tied to one of the Curve types below through the curveID field.
[GoType] partial struct PrivateKey {
    internal ΔPublicKey pub;
    internal slice<byte> d; // bigmod.(*Nat).Bytes output (same length as the curve order)
}

[GoRecv] public static slice<byte> Bytes(this ref PrivateKey priv) {
    return priv.d;
}

public static ж<ΔPublicKey> PublicKey(this ж<PrivateKey> Ꮡpriv) {
    return Ꮡpriv.of(PrivateKey.Ꮡpub);
}

[GoType] partial struct ΔPublicKey {
    internal curveID curve;
    internal slice<byte> q; // uncompressed nistec Point.Bytes output
}

[GoRecv] public static slice<byte> Bytes(this ref ΔPublicKey pub) {
    return pub.q;
}

[GoType("@string")] partial struct curveID;

internal static readonly curveID p224 = "P-224"u8;
internal static readonly curveID p256 = "P-256"u8;
internal static readonly curveID p384 = "P-384"u8;
internal static readonly curveID p521 = "P-521"u8;

[GoType] partial struct Curve<P>
    where P : Point<P>
{
    internal curveID curve;
    internal Func<P> newPoint;
    internal Func<slice<byte>, (slice<byte>, error)> ordInverse;
    public ж<bigmod.Modulus> N;
    internal slice<byte> nMinus2;
}

// Point is a generic constraint for the [nistec] Point types.
[GoType] partial interface Point<P> {
    //  Type constraints: *nistec.P224Point | *nistec.P256Point | *nistec.P384Point | *nistec.P521Point
    // Derived operators: none
    slice<byte> Bytes();
    (slice<byte>, error) BytesX();
    (P, error) SetBytes(slice<byte> _);
    (P, error) ScalarMult(P _Δp0, slice<byte> _Δp1);
    (P, error) ScalarBaseMult(slice<byte> _);
    P Add(P p1, P p2);
}

internal static void precomputeParams<P>(ref Curve<P> c, slice<byte> order)
    where P : Point<P>
{
    error err = default!;
    (c.N, err) = bigmod.NewModulus(order);
    if (err != default!) {
        throw panic(err);
    }
    var (two, _) = bigmod.NewNat().SetBytes(new byte[]{2}.slice(), c.N);
    c.nMinus2 = bigmod.NewNat().ExpandFor(c.N).Sub(two, c.N).Bytes(c.N);
}

public static ж<Curve<P224PointжPoint>> P224() {
    return _P224();
}

internal static Func<ж<Curve<P224PointжPoint>>> _P224;
internal static void initᴛ_P224() { _P224 = sync.OnceValue(ж<Curve<P224PointжPoint>> () => {
    var c = Ꮡ(new Curve<P224PointжPoint>(
        curve: p224,
        newPoint: () => Δnistec.NewP224Point()
    ));
    precomputeParams<P224PointжPoint>(ref (c).DerefOrNull(), p224Order);
    return c;
}); }

internal static slice<byte> p224Order = new byte[]{
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x16, 0xa2,
    0xe0, 0xb8, 0xf0, 0x3e, 0x13, 0xdd, 0x29, 0x45,
    0x5c, 0x5c, 0x2a, 0x3d
}.slice();

public static ж<Curve<P256PointжPoint>> P256() {
    return _P256();
}

internal static Func<ж<Curve<P256PointжPoint>>> _P256;
internal static void initᴛ_P256() { _P256 = sync.OnceValue(ж<Curve<P256PointжPoint>> () => {
    var c = Ꮡ(new Curve<P256PointжPoint>(
        curve: p256,
        newPoint: () => Δnistec.NewP256Point(),
        ordInverse: (Δp0) => Δnistec.P256OrdInverse(Δp0)
    ));
    precomputeParams<P256PointжPoint>(ref (c).DerefOrNull(), p256Order);
    return c;
}); }

internal static slice<byte> p256Order = new byte[]{
    0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xbc, 0xe6, 0xfa, 0xad, 0xa7, 0x17, 0x9e, 0x84,
    0xf3, 0xb9, 0xca, 0xc2, 0xfc, 0x63, 0x25, 0x51}.slice();

public static ж<Curve<P384PointжPoint>> P384() {
    return _P384();
}

internal static Func<ж<Curve<P384PointжPoint>>> _P384;
internal static void initᴛ_P384() { _P384 = sync.OnceValue(ж<Curve<P384PointжPoint>> () => {
    var c = Ꮡ(new Curve<P384PointжPoint>(
        curve: p384,
        newPoint: () => Δnistec.NewP384Point()
    ));
    precomputeParams<P384PointжPoint>(ref (c).DerefOrNull(), p384Order);
    return c;
}); }

internal static slice<byte> p384Order = new byte[]{
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xc7, 0x63, 0x4d, 0x81, 0xf4, 0x37, 0x2d, 0xdf,
    0x58, 0x1a, 0x0d, 0xb2, 0x48, 0xb0, 0xa7, 0x7a,
    0xec, 0xec, 0x19, 0x6a, 0xcc, 0xc5, 0x29, 0x73}.slice();

public static ж<Curve<P521PointжPoint>> P521() {
    return _P521();
}

internal static Func<ж<Curve<P521PointжPoint>>> _P521;
internal static void initᴛ_P521() { _P521 = sync.OnceValue(ж<Curve<P521PointжPoint>> () => {
    var c = Ꮡ(new Curve<P521PointжPoint>(
        curve: p521,
        newPoint: () => Δnistec.NewP521Point()
    ));
    precomputeParams<P521PointжPoint>(ref (c).DerefOrNull(), p521Order);
    return c;
}); }

internal static slice<byte> p521Order = new byte[]{0x01, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfa,
    0x51, 0x86, 0x87, 0x83, 0xbf, 0x2f, 0x96, 0x6b,
    0x7f, 0xcc, 0x01, 0x48, 0xf7, 0x09, 0xa5, 0xd0,
    0x3b, 0xb5, 0xc9, 0xb8, 0x89, 0x9c, 0x47, 0xae,
    0xbb, 0x6f, 0xb7, 0x1e, 0x91, 0x38, 0x64, 0x09}.slice();

public static (ж<PrivateKey>, error) NewPrivateKey<P>(ж<Curve<P>> Ꮡc, slice<byte> D, slice<byte> Q)
    where P : Point<P>
{
    ref var c = ref Ꮡc.DerefOrNull();

    fips140.RecordApproved();
    var (pub, err) = NewPublicKey(Ꮡc, Q);
    if (err != default!) {
        return (default!, err);
    }
    (var d, err) = bigmod.NewNat().SetBytes(D, c.N);
    if (err != default!) {
        return (default!, err);
    }
    var priv = Ꮡ(new PrivateKey(pub: pub.Value, d: d.Bytes(c.N)));
    return (priv, default!);
}

public static (ж<ΔPublicKey>, error) NewPublicKey<P>(ж<Curve<P>> Ꮡc, slice<byte> Q)
    where P : Point<P>
{
    ref var c = ref Ꮡc.DerefOrNull();

    // SetBytes checks that Q is a valid point on the curve, and that its
    // coordinates are reduced modulo p, fulfilling the requirements of SP
    // 800-89, Section 5.3.2.
    var (_, err) = c.newPoint().SetBytes(Q);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new ΔPublicKey(curve: c.curve, q: Q)), default!);
}

// GenerateKey generates a new ECDSA private key pair for the specified curve.
public static (ж<PrivateKey>, error) GenerateKey<P>(ж<Curve<P>> Ꮡc, io.Reader rand)
    where P : Point<P>
{
    ref var c = ref Ꮡc.DerefOrNull();

    fips140.RecordApproved();
    var (k, Q, err) = randomPoint(ref (Ꮡc).DerefOrNull(), (slice<byte> b) => drbg.ReadWithReader(rand, b));
    if (err != default!) {
        return (default!, err);
    }
    var priv = Ꮡ(new PrivateKey(
        pub: new ΔPublicKey(
            curve: c.curve,
            q: Q.Bytes()
        ),
        d: k.Bytes(c.N)
    ));
    fipsPCT(Ꮡc, priv);
    return (priv, default!);
}

// randomPoint returns a random scalar and the corresponding point using a
// procedure equivalent to FIPS 186-5, Appendix A.2.2 (ECDSA Key Pair Generation
// by Rejection Sampling) and to Appendix A.3.2 (Per-Message Secret Number
// Generation of Private Keys by Rejection Sampling) or Appendix A.3.3
// (Per-Message Secret Number Generation for Deterministic ECDSA) followed by
// Step 5 of Section 6.4.1.
internal static (ж<bigmodꓸNat> k, P p, error err) randomPoint<P>(ref Curve<P> c, Func<slice<byte>, error> generate)
    where P : Point<P>
{
    while (ᐧ) {
        var b = new slice<byte>(c.N.Size());
        {
            var errΔ1 = generate(b); if (errΔ1 != default!) {
                return (default!, default!, errΔ1);
            }
        }
        // Take only the leftmost bits of the generated random value. This is
        // both necessary to increase the chance of the random value being in
        // the correct range and to match the specification. It's unfortunate
        // that we need to do a shift instead of a mask, but see the comment on
        // rightShift.
        //
        // These are the most dangerous lines in the package and maybe in the
        // library: a single bit of bias in the selection of nonces would likely
        // lead to key recovery, but no tests would fail. Look but DO NOT TOUCH.
        {
            nint excess = len(b) * 8 - c.N.BitLen(); if (excess > 0) {
                // Just to be safe, assert that this only happens for the one curve that
                // doesn't have a round number of bits.
                if (c.curve != p521) {
                    throw panic("ecdsa: internal error: unexpectedly masking off bits");
                }
                b = rightShift(b, excess);
            }
        }
        // FIPS 186-5, Appendix A.4.2 makes us check x <= N - 2 and then return
        // x + 1. Note that it follows that 0 < x + 1 < N. Instead, SetBytes
        // checks that k < N, and we explicitly check 0 != k. Since k can't be
        // negative, this is strictly equivalent. None of this matters anyway
        // because the chance of selecting zero is cryptographically negligible.
        {
            var (kΔ1, errΔ2) = bigmod.NewNat().SetBytes(b, c.N); if (errΔ2 == default! && kΔ1.IsZero() == 0) {
                var (pΔ1, errΔ3) = c.newPoint().ScalarBaseMult(kΔ1.Bytes(c.N));
                return (kΔ1, pΔ1, errΔ3);
            }
        }
        if (testingOnlyRejectionSamplingLooped != default!) {
            testingOnlyRejectionSamplingLooped();
        }
    }
}

// testingOnlyRejectionSamplingLooped is called when rejection sampling in
// randomPoint rejects a candidate for being higher than the modulus.
internal static Action testingOnlyRejectionSamplingLooped;

// Signature is an ECDSA signature, where r and s are represented as big-endian
// byte slices of the same length as the curve order.
[GoType] partial struct Signature {
    public slice<byte> R, S;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdsaPrivateKeyDoesNotˢ = "ecdsa: private key does not match curve"u8;

// Sign signs a hash (which shall be the result of hashing a larger message with
// the hash function H) using the private key, priv. If the hash is longer than
// the bit-length of the private key's curve order, the hash will be truncated
// to that length.
public static (ж<Signature>, error) Sign<P, H>(ж<Curve<P>> Ꮡc, Func<H> h, ж<PrivateKey> Ꮡpriv, io.Reader rand, slice<byte> hash)
    where P : Point<P>
    where H : fips140.Hash
{
    ref var c = ref Ꮡc.DerefOrNull();
    ref var priv = ref Ꮡpriv.DerefOrNull();

    if (priv.pub.curve != c.curve) {
        return (default!, errors.New(ecdsaPrivateKeyDoesNotˢ));
    }
    fips140.RecordApproved();
    fipsSelfTest();
    // Random ECDSA is dangerous, because a failure of the RNG would immediately
    // leak the private key. Instead, we use a "hedged" approach, as specified
    // in draft-irtf-cfrg-det-sigs-with-noise-04, Section 4. This has also the
    // advantage of closely resembling Deterministic ECDSA.
    var Z = new slice<byte>(len(priv.d));
    {
        var err = drbg.ReadWithReader(rand, Z); if (err != default!) {
            return (default!, err);
        }
    }
    // See https://github.com/cfrg/draft-irtf-cfrg-det-sigs-with-noise/issues/6
    // for the FIPS compliance of this method. In short Z is entropy from the
    // main DRBG, of length 3/2 of security_strength, so the nonce is optional
    // per SP 800-90Ar1, Section 8.6.7, and the rest is a personalization
    // string, which per SP 800-90Ar1, Section 8.7.1 may contain secret
    // information.
    var drbgΔ1 = newDRBG(h, Z, default!, new blockAlignedPersonalizationString(new slice<byte>[]{priv.d, bits2octets(ref (Ꮡc).DerefOrNull(), hash)}.slice()));
    return sign(ref (Ꮡc).DerefOrNull(), ref (Ꮡpriv).DerefOrNull(), drbgΔ1, hash);
}

// SignDeterministic signs a hash (which shall be the result of hashing a
// larger message with the hash function H) using the private key, priv. If the
// hash is longer than the bit-length of the private key's curve order, the hash
// will be truncated to that length. This applies Deterministic ECDSA as
// specified in FIPS 186-5 and RFC 6979.
public static (ж<Signature>, error) SignDeterministic<P, H>(ж<Curve<P>> Ꮡc, Func<H> h, ж<PrivateKey> Ꮡpriv, slice<byte> hash)
    where P : Point<P>
    where H : fips140.Hash
{
    ref var c = ref Ꮡc.DerefOrNull();
    ref var priv = ref Ꮡpriv.DerefOrNull();

    if (priv.pub.curve != c.curve) {
        return (default!, errors.New(ecdsaPrivateKeyDoesNotˢ));
    }
    fips140.RecordApproved();
    fipsSelfTestDeterministic();
    var drbg = newDRBG(h, priv.d, bits2octets(ref (Ꮡc).DerefOrNull(), hash), default!); // RFC 6979, Section 3.3
    return sign(ref (Ꮡc).DerefOrNull(), ref (Ꮡpriv).DerefOrNull(), drbg, hash);
}

// bits2octets as specified in FIPS 186-5, Appendix B.2.4 or RFC 6979,
// Section 2.3.4. See RFC 6979, Section 3.5 for the rationale.
internal static slice<byte> bits2octets<P>(ref Curve<P> c, slice<byte> hash)
    where P : Point<P>
{
    var e = bigmod.NewNat();
    hashToNat(ref c, e, hash);
    return e.Bytes(c.N);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdsaInternalErrorRIsˢ = "ecdsa: internal error: r is zero"u8;
internal static readonly @string ecdsaInternalErrorSIsˢ = "ecdsa: internal error: s is zero"u8;

internal static (ж<Signature>, error) signGeneric<P>(ref Curve<P> c, ref PrivateKey priv, ж<hmacDRBG> Ꮡdrbg, slice<byte> hash)
    where P : Point<P>
{
    // FIPS 186-5, Section 6.4.1
    var (k, R, err) = randomPoint(ref c, (slice<byte> b) => {
        Ꮡdrbg.Value.Generate(b);
        return default!;
    });
    if (err != default!) {
        return (default!, err);
    }
    // kInv = k⁻¹
    var kInv = bigmod.NewNat();
    inverse(ref c, kInv, k);
    (var Rx, err) = R.BytesX();
    if (err != default!) {
        return (default!, err);
    }
    (var r, err) = bigmod.NewNat().SetOverflowingBytes(Rx, c.N);
    if (err != default!) {
        return (default!, err);
    }
    // The spec wants us to retry here, but the chance of hitting this condition
    // on a large prime-order group like the NIST curves we support is
    // cryptographically negligible. If we hit it, something is awfully wrong.
    if (r.IsZero() == 1) {
        return (default!, errors.New(ecdsaInternalErrorRIsˢ));
    }
    var e = bigmod.NewNat();
    hashToNat(ref c, e, hash);
    (var s, err) = bigmod.NewNat().SetBytes(priv.d, c.N);
    if (err != default!) {
        return (default!, err);
    }
    s.Mul(r, c.N);
    s.Add(e, c.N);
    s.Mul(kInv, c.N);
    // Again, the chance of this happening is cryptographically negligible.
    if (s.IsZero() == 1) {
        return (default!, errors.New(ecdsaInternalErrorSIsˢ));
    }
    return (Ꮡ(new Signature(r.Bytes(c.N), s.Bytes(c.N))), default!);
}

// inverse sets kInv to the inverse of k modulo the order of the curve.
internal static void inverse<P>(ref Curve<P> c, ж<bigmodꓸNat> ᏑkInv, ж<bigmodꓸNat> Ꮡk)
    where P : Point<P>
{
    ref var k = ref Ꮡk.DerefOrNull();

    if (c.ordInverse != default!) {
        var (kBytes, err) = c.ordInverse(k.Bytes(c.N));
        // Some platforms don't implement ordInverse, and always return an error.
        if (err == default!) {
            var (_, errΔ1) = ᏑkInv.SetBytes(kBytes, c.N);
            if (errΔ1 != default!) {
                throw panic("ecdsa: internal error: ordInverse produced an invalid value");
            }
            return;
        }
    }
    // Calculate the inverse of s in GF(N) using Fermat's method
    // (exponentiation modulo P - 2, per Euler's theorem)
    ᏑkInv.Exp(Ꮡk, c.nMinus2, c.N);
}

// hashToNat sets e to the left-most bits of hash, according to
// FIPS 186-5, Section 6.4.1, point 2 and Section 6.4.2, point 3.
internal static void hashToNat<P>(ref Curve<P> c, ж<bigmodꓸNat> Ꮡe, slice<byte> hash)
    where P : Point<P>
{
    // ECDSA asks us to take the left-most log2(N) bits of hash, and use them as
    // an integer modulo N. This is the absolute worst of all worlds: we still
    // have to reduce, because the result might still overflow N, but to take
    // the left-most bits for P-521 we have to do a right shift.
    {
        nint size = c.N.Size(); if (len(hash) >= size) {
            hash = hash[..(int)(size)];
            {
                nint excess = len(hash) * 8 - c.N.BitLen(); if (excess > 0) {
                    hash = rightShift(hash, excess);
                }
            }
        }
    }
    var (_, err) = Ꮡe.SetOverflowingBytes(hash, c.N);
    if (err != default!) {
        throw panic("ecdsa: internal error: truncated hash is too long");
    }
}

// rightShift implements the right shift necessary for bits2int, which takes the
// leftmost bits of either the hash or HMAC_DRBG output.
//
// Note how taking the rightmost bits would have been as easy as masking the
// first byte, but we can't have nice things.
internal static slice<byte> rightShift(slice<byte> b, nint shift) {
    if (shift <= 0 || shift >= 8) {
        throw panic("ecdsa: internal error: shift can only be by 1 to 7 bits");
    }
    b = bytes.Clone(b);
    for (nint i = len(b) - 1; i >= 0; i--) {
        b[i].RshAssign((int64)(shift));
        if (i > 0) {
            b[i] |= (byte)(b[i - 1].Lsh((int64)((8 - shift))));
        }
    }
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdsaPublicKeyDoesNotˢ = "ecdsa: public key does not match curve"u8;

// Verify verifies the signature, sig, of hash (which should be the result of
// hashing a larger message) using the public key, pub. If the hash is longer
// than the bit-length of the private key's curve order, the hash will be
// truncated to that length.
//
// The inputs are not considered confidential, and may leak through timing side
// channels, or if an attacker has control of part of the inputs.
public static error Verify<P>(ж<Curve<P>> Ꮡc, ж<ΔPublicKey> Ꮡpub, slice<byte> hash, ж<Signature> Ꮡsig)
    where P : Point<P>
{
    ref var c = ref Ꮡc.DerefOrNull();
    ref var pub = ref Ꮡpub.DerefOrNull();

    if (pub.curve != c.curve) {
        return errors.New(ecdsaPublicKeyDoesNotˢ);
    }
    fips140.RecordApproved();
    fipsSelfTest();
    return verify(ref (Ꮡc).DerefOrNull(), ref (Ꮡpub).DerefOrNull(), hash, ref (Ꮡsig).DerefOrNull());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdsaInvalidSignatureRIsˢ = "ecdsa: invalid signature: r is zero"u8;
internal static readonly @string ecdsaInvalidSignatureSIsˢ = "ecdsa: invalid signature: s is zero"u8;
internal static readonly @string ecdsaSignatureDidNotˢ = "ecdsa: signature did not verify"u8;

internal static error verifyGeneric<P>(ref Curve<P> c, ref ΔPublicKey pub, slice<byte> hash, ref Signature sig)
    where P : Point<P>
{
    // FIPS 186-5, Section 6.4.2
    var (Q, err) = c.newPoint().SetBytes(pub.q);
    if (err != default!) {
        return err;
    }
    (var r, err) = bigmod.NewNat().SetBytes(sig.R, c.N);
    if (err != default!) {
        return err;
    }
    if (r.IsZero() == 1) {
        return errors.New(ecdsaInvalidSignatureRIsˢ);
    }
    (var s, err) = bigmod.NewNat().SetBytes(sig.S, c.N);
    if (err != default!) {
        return err;
    }
    if (s.IsZero() == 1) {
        return errors.New(ecdsaInvalidSignatureSIsˢ);
    }
    var e = bigmod.NewNat();
    hashToNat(ref c, e, hash);
    // w = s⁻¹
    var w = bigmod.NewNat();
    inverse(ref c, w, s);
    // p₁ = [e * s⁻¹]G
    (var p1, err) = c.newPoint().ScalarBaseMult(e.Mul(w, c.N).Bytes(c.N));
    if (err != default!) {
        return err;
    }
    // p₂ = [r * s⁻¹]Q
    (var p2, err) = Q.ScalarMult(Q, w.Mul(r, c.N).Bytes(c.N));
    if (err != default!) {
        return err;
    }
    // BytesX returns an error for the point at infinity.
    (var Rx, err) = p1.Add(p1, p2).BytesX();
    if (err != default!) {
        return err;
    }
    (var v, err) = bigmod.NewNat().SetOverflowingBytes(Rx, c.N);
    if (err != default!) {
        return err;
    }
    if (v.Equal(r) != 1) {
        return errors.New(ecdsaSignatureDidNotˢ);
    }
    return default!;
}

} // end ecdsa_package
