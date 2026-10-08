// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using Δnistec = go.crypto.@internal.fips140.nistec_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using errors = errors_package;
using io = io_package;
using bits = math.bits_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140deps;
using math;

partial class ecdh_package {

// PrivateKey and PublicKey are not generic to make it possible to use them
// in other types without instantiating them with a specific point type.
// They are tied to one of the Curve types below through the curveID field.
// All this is duplicated from crypto/internal/fips/ecdsa, but the standards are
// different and FIPS 140 does not allow reusing keys across them.
partial struct PrivateKey {
    internal ΔPublicKey pub;
    internal slice<byte> d; // bigmod.(*Nat).Bytes output (fixed length)
}

public static slice<byte> Bytes(this ref PrivateKey priv) {
    return priv.d;
}

public static ж<ΔPublicKey> PublicKey(this ж<PrivateKey> Ꮡpriv) {
    return Ꮡpriv.of(PrivateKey.Ꮡpub);
}

partial struct ΔPublicKey {
    internal curveID curve;
    internal slice<byte> q; // uncompressed nistec Point.Bytes output
}

public static slice<byte> Bytes(this ref ΔPublicKey pub) {
    return pub.q;
}

partial struct curveID /*@string*/;

internal static readonly curveID p224 = "P-224"u8;
internal static readonly curveID p256 = "P-256"u8;
internal static readonly curveID p384 = "P-384"u8;
internal static readonly curveID p521 = "P-521"u8;

partial struct Curve<P>
    where P : Point<P>
{
    internal curveID curve;
    internal Func<P> newPoint;
    public slice<byte> N;
}

// Point is a generic constraint for the [nistec] Point types.
partial interface Point<P> {
    //  Type constraints: *nistec.P224Point | *nistec.P256Point | *nistec.P384Point | *nistec.P521Point
    // Derived operators: none
    slice<byte> Bytes();
    (slice<byte>, error) BytesX();
    (P, error) SetBytes(slice<byte> _);
    (P, error) ScalarMult(P _Δp0, slice<byte> _Δp1);
    (P, error) ScalarBaseMult(slice<byte> _);
}

public static ж<Curve<P224PointжPoint>> P224() {
    return Ꮡ(new Curve<P224PointжPoint>(
        curve: p224,
        newPoint: () => Δnistec.NewP224Point(),
        N: p224Order
    ));
}

internal static slice<byte> p224Order = new byte[]{
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x16, 0xa2,
    0xe0, 0xb8, 0xf0, 0x3e, 0x13, 0xdd, 0x29, 0x45,
    0x5c, 0x5c, 0x2a, 0x3d
}.slice();

public static ж<Curve<P256PointжPoint>> P256() {
    return Ꮡ(new Curve<P256PointжPoint>(
        curve: p256,
        newPoint: () => Δnistec.NewP256Point(),
        N: p256Order
    ));
}

internal static slice<byte> p256Order = new byte[]{
    0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xbc, 0xe6, 0xfa, 0xad, 0xa7, 0x17, 0x9e, 0x84,
    0xf3, 0xb9, 0xca, 0xc2, 0xfc, 0x63, 0x25, 0x51
}.slice();

public static ж<Curve<P384PointжPoint>> P384() {
    return Ꮡ(new Curve<P384PointжPoint>(
        curve: p384,
        newPoint: () => Δnistec.NewP384Point(),
        N: p384Order
    ));
}

internal static slice<byte> p384Order = new byte[]{
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xc7, 0x63, 0x4d, 0x81, 0xf4, 0x37, 0x2d, 0xdf,
    0x58, 0x1a, 0x0d, 0xb2, 0x48, 0xb0, 0xa7, 0x7a,
    0xec, 0xec, 0x19, 0x6a, 0xcc, 0xc5, 0x29, 0x73
}.slice();

public static ж<Curve<P521PointжPoint>> P521() {
    return Ꮡ(new Curve<P521PointжPoint>(
        curve: p521,
        newPoint: () => Δnistec.NewP521Point(),
        N: p521Order
    ));
}

internal static slice<byte> p521Order = new byte[]{0x01, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfa,
    0x51, 0x86, 0x87, 0x83, 0xbf, 0x2f, 0x96, 0x6b,
    0x7f, 0xcc, 0x01, 0x48, 0xf7, 0x09, 0xa5, 0xd0,
    0x3b, 0xb5, 0xc9, 0xb8, 0x89, 0x9c, 0x47, 0xae,
    0xbb, 0x6f, 0xb7, 0x1e, 0x91, 0x38, 0x64, 0x09
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdhPctˢ = "ECDH PCT"u8;
internal static readonly @string cryptoEcdhPublicKeyDoesˢ = "crypto/ecdh: public key does not match private key"u8;

// GenerateKey generates a new ECDSA private key pair for the specified curve.
public static (ж<PrivateKey>, error) GenerateKey<P>(ж<Curve<P>> Ꮡc, io.Reader rand)
    where P : Point<P>
{
    ref var c = ref Ꮡc.DerefOrNull();

    fips140.RecordApproved();
    // This procedure is equivalent to Key Pair Generation by Testing
    // Candidates, specified in NIST SP 800-56A Rev. 3, Section 5.6.1.2.2.
    while (ᐧ) {
        var key = new slice<byte>(len(c.N));
        {
            var errΔ1 = drbg.ReadWithReader(rand, key); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        // In tests, rand will return all zeros and NewPrivateKey will reject
        // the zero key as it generates the identity as a public key. This also
        // makes this function consistent with crypto/elliptic.GenerateKey.
        key[1] ^= (byte)(0x42);
        // Mask off any excess bits if the size of the underlying field is not a
        // whole number of bytes, which is only the case for P-521.
        if (c.curve == p521 && (byte)(c.N[0] & 0b1111_1110) == 0) {
            key[0] &= (byte)(0b0000_0001);
        }
        var (privateKey, err) = NewPrivateKey(Ꮡc, key);
        if (err != default!) {
            continue;
        }
        // A "Pairwise Consistency Test" makes no sense if we just generated the
        // public key from an ephemeral private key. Moreover, there is no way to
        // check it aside from redoing the exact same computation again. SP 800-56A
        // Rev. 3, Section 5.6.2.1.4 acknowledges that, and doesn't require it.
        // However, ISO 19790:2012, Section 7.10.3.3 has a blanket requirement for a
        // PCT for all generated keys (AS10.35) and FIPS 140-3 IG 10.3.A, Additional
        // Comment 1 goes out of its way to say that "the PCT shall be performed
        // consistent [...], even if the underlying standard does not require a
        // PCT". So we do it. And make ECDH nearly 50% slower (only) in FIPS mode.
        var privateKeyʗ1 = privateKey;
        fips140.PCT(ecdhPctˢ, error () => {
            var (p1, errΔ1) = Ꮡc.Value.newPoint().ScalarBaseMult((~privateKeyʗ1).d);
            if (errΔ1 != default!) {
                return errΔ1;
            }
            if (!bytes.Equal(p1.Bytes(), (~privateKeyʗ1).pub.q)) {
                return errors.New(cryptoEcdhPublicKeyDoesˢ);
            }
            return default!;
        });
        return (privateKey, default!);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoEcdhInvalidPrivateˢ = "crypto/ecdh: invalid private key"u8;

public static (ж<PrivateKey>, error) NewPrivateKey<P>(ж<Curve<P>> Ꮡc, slice<byte> key)
    where P : Point<P>
{
    ref var c = ref Ꮡc.DerefOrNull();

    // SP 800-56A Rev. 3, Section 5.6.1.2.2 checks that c <= n – 2 and then
    // returns d = c + 1. Note that it follows that 0 < d < n. Equivalently,
    // we check that 0 < d < n, and return d.
    if (len(key) != len(c.N) || isZero(key) || !isLess(key, c.N)) {
        return (default!, errors.New(cryptoEcdhInvalidPrivateˢ));
    }
    var (p, err) = c.newPoint().ScalarBaseMult(key);
    if (err != default!) {
        // This is unreachable because the only error condition of
        // ScalarBaseMult is if the input is not the right size.
        throw panic("crypto/ecdh: internal error: nistec ScalarBaseMult failed for a fixed-size input");
    }
    var publicKey = p.Bytes();
    if (len(publicKey) == 1) {
        // The encoding of the identity is a single 0x00 byte. This is
        // unreachable because the only scalar that generates the identity is
        // zero, which is rejected above.
        throw panic("crypto/ecdh: internal error: public key is the identity element");
    }
    var k = Ꮡ(new PrivateKey(d: bytes.Clone(key), pub: new ΔPublicKey(curve: c.curve, q: publicKey)));
    return (k, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoEcdhInvalidPublicˢ = "crypto/ecdh: invalid public key"u8;

public static (ж<ΔPublicKey>, error) NewPublicKey<P>(ж<Curve<P>> Ꮡc, slice<byte> key)
    where P : Point<P>
{
    ref var c = ref Ꮡc.DerefOrNull();

    // Reject the point at infinity and compressed encodings.
    if (len(key) == 0 || key[0] != 4) {
        return (default!, errors.New(cryptoEcdhInvalidPublicˢ));
    }
    // SetBytes checks that x and y are in the interval [0, p - 1], and that
    // the point is on the curve. Along with the rejection of the point at
    // infinity (the identity element) above, this fulfills the requirements
    // of NIST SP 800-56A Rev. 3, Section 5.6.2.3.4.
    {
        var (_, err) = c.newPoint().SetBytes(key); if (err != default!) {
            return (default!, err);
        }
    }
    return (Ꮡ(new ΔPublicKey(curve: c.curve, q: bytes.Clone(key))), default!);
}

public static (slice<byte>, error) ECDH<P>(ж<Curve<P>> Ꮡc, ж<PrivateKey> Ꮡk, ж<ΔPublicKey> Ꮡpeer)
    where P : Point<P>
{
    fipsSelfTest();
    fips140.RecordApproved();
    return ecdh(ref (Ꮡc).DerefOrNull(), ref (Ꮡk).DerefOrNull(), ref (Ꮡpeer).DerefOrNull());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoEcdhMismatchedˢ = "crypto/ecdh: mismatched curves"u8;
internal static readonly @string cryptoEcdhPublicKeyIsTheˢ = "crypto/ecdh: public key is the identity element"u8;

internal static (slice<byte>, error) ecdh<P>(ref Curve<P> c, ref PrivateKey k, ref ΔPublicKey peer)
    where P : Point<P>
{
    if (c.curve != k.pub.curve) {
        return (default!, errors.New(cryptoEcdhMismatchedˢ));
    }
    if (k.pub.curve != peer.curve) {
        return (default!, errors.New(cryptoEcdhMismatchedˢ));
    }
    // This applies the Shared Secret Computation of the Ephemeral Unified Model
    // scheme specified in NIST SP 800-56A Rev. 3, Section 6.1.2.2.
    // Per Section 5.6.2.3.4, Step 1, reject the identity element (0x00).
    if (len(k.pub.q) == 1) {
        return (default!, errors.New(cryptoEcdhPublicKeyIsTheˢ));
    }
    // SetBytes checks that (x, y) are reduced modulo p, and that they are on
    // the curve, performing Steps 2-3 of Section 5.6.2.3.4.
    var (p, err) = c.newPoint().SetBytes(peer.q);
    if (err != default!) {
        return (default!, err);
    }
    // Compute P according to Section 5.7.1.2.
    {
        var (_, errΔ1) = p.ScalarMult(p, k.d); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    // BytesX checks that the result is not the identity element, and returns the
    // x-coordinate of the result, performing Steps 2-5 of Section 5.7.1.2.
    return p.BytesX();
}

// isZero reports whether x is all zeroes in constant time.
internal static bool isZero(slice<byte> x) {
    byte acc = default!;
    foreach (var (_, b) in x) {
        acc |= (byte)(b);
    }
    return acc == 0;
}

// isLess reports whether a < b, where a and b are big-endian buffers of the
// same length and shorter than 72 bytes.
internal static bool isLess(slice<byte> a, slice<byte> b) {
    if (len(a) != len(b)) {
        throw panic("crypto/ecdh: internal error: mismatched isLess inputs");
    }
    // Copy the values into a fixed-size preallocated little-endian buffer.
    // 72 bytes is enough for every scalar in this package, and having a fixed
    // size lets us avoid heap allocations.
    if (len(a) > 72) {
        throw panic("crypto/ecdh: internal error: isLess input too large");
    }
    var (bufA, bufB) = (new slice<byte>(72), new slice<byte>(72));
    foreach (var (i, _) in a) {
        (bufA[i], bufB[i]) = (a[len(a) - i - 1], b[len(b) - i - 1]);
    }
    // Perform a subtraction with borrow.
    uint64 borrow = default!;
    for (nint i = 0; i < len(bufA); i += 8) {
        var (limbA, limbB) = (byteorder.LEUint64(bufA.slice(i)), byteorder.LEUint64(bufB.slice(i)));
        (_, borrow) = bits.Sub64(limbA, limbB, borrow);
    }
    // If there is a borrow at the end of the operation, then a < b.
    return borrow == 1;
}

} // end ecdh_package
