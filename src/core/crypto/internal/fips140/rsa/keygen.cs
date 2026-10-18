// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using bigmod = go.crypto.@internal.fips140.bigmod_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using errors = errors_package;
using io = io_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class rsa_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rsaKeyTooSmallˢ = "rsa: key too small"u8;
internal static readonly @string rsaGeneratedPQRandomˢ = "rsa: generated p == q, random source is broken"u8;
internal static readonly @string rsaInternalErrorModulusˢ = "rsa: internal error: modulus size incorrect"u8;
internal static readonly @string rsaInternalErrorED1ModNˢ = "rsa: internal error: e*d != 1 mod λ(N)"u8;
internal static readonly @string rsaSignAndVerifyPctˢ = "RSA sign and verify PCT"u8;
internal static readonly @string sha256ˢ = "SHA-256"u8;

// GenerateKey generates a new RSA key pair of the given bit size.
// bits must be at least 32.
public static (ж<PrivateKey>, error) GenerateKey(io.Reader rand, nint bits) {
    if (bits < 32) {
        return (default!, errors.New(rsaKeyTooSmallˢ));
    }
    fips140.RecordApproved();
    if (bits < 2048 || bits % 2 == 1) {
        fips140.RecordNonApproved();
    }

    while (ᐧ) {
        var (p, err) = randomPrime(rand, (bits + 1) / 2);
        if (err != default!) {
            return (default!, err);
        }
        (var q, err) = randomPrime(rand, bits / 2);
        if (err != default!) {
            return (default!, err);
        }

        (var P, err) = bigmod.NewModulus(p);
        if (err != default!) {
            return (default!, err);
        }
        (var Q, err) = bigmod.NewModulus(q);
        if (err != default!) {
            return (default!, err);
        }

        if (Q.Nat().ExpandFor(P).Equal(P.Nat()) == 1) {
            return (default!, errors.New(rsaGeneratedPQRandomˢ));
        }

        (var N, err) = bigmod.NewModulusProduct(p, q);
        if (err != default!) {
            return (default!, err);
        }
        if (N.BitLen() != bits) {
            return (default!, errors.New(rsaInternalErrorModulusˢ));
        }

        // d can be safely computed as e⁻¹ mod φ(N) where φ(N) = (p-1)(q-1), and
        // indeed that's what both the original RSA paper and the pre-FIPS
        // crypto/rsa implementation did.
        //
        // However, FIPS 186-5, A.1.1(3) requires computing it as e⁻¹ mod λ(N)
        // where λ(N) = lcm(p-1, q-1).
        //
        // This makes d smaller by 1.5 bits on average, which is irrelevant both
        // because we exclusively use the CRT for private operations and because
        // we use constant time windowed exponentiation. On the other hand, it
        // requires computing a GCD of two values that are not coprime, and then
        // a division, both complex variable-time operations.
        (var λ, err) = totient(P, Q);
        if (AreEqual(err, errDivisorTooLarge)) {
            // The divisor is too large, try again with different primes.
            continue;
        }
        if (err != default!) {
            return (default!, err);
        }

        var e = bigmod.NewNat().SetUint(65537);
        var (d, ok) = bigmod.NewNat().InverseVarTime(e, λ);
        if (!ok) {
            // This checks that GCD(e, lcm(p-1, q-1)) = 1, which is equivalent
            // to checking GCD(e, p-1) = 1 and GCD(e, q-1) = 1 separately in
            // FIPS 186-5, Appendix A.1.3, steps 4.5 and 5.6.
            //
            // We waste a prime by retrying the whole process, since 65537 is
            // probably only a factor of one of p-1 or q-1, but the probability
            // of this check failing is only 1/65537, so it doesn't matter.
            continue;
        }

        if (e.ExpandFor(λ).Mul(d, λ).IsOne() == 0) {
            return (default!, errors.New(rsaInternalErrorED1ModNˢ));
        }

        // FIPS 186-5, A.1.1(3) requires checking that d > 2^(nlen / 2).
        //
        // The probability of this check failing when d is derived from
        // (e, p, q) is roughly
        //
        //   2^(nlen/2) / 2^nlen = 2^(-nlen/2)
        //
        // so less than 2⁻¹²⁸ for keys larger than 256 bits.
        //
        // We still need to check to comply with FIPS 186-5, but knowing it has
        // negligible chance of failure we can defer the check to the end of key
        // generation and return an error if it fails. See [checkPrivateKey].

        (var k, err) = newPrivateKey(N, 65537, d, P, Q);
        if (err != default!) {
            return (default!, err);
        }

        if ((~k).fipsApproved) {
            var kʗ1 = k;
            fips140.PCT(rsaSignAndVerifyPctˢ, () => {
                var hash = new byte[]{
                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                    0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10,
                    0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
                    0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20
                }.slice();
                var (sig, errΔ1) = signPKCS1v15(kʗ1, sha256ˢ, hash);
                if (errΔ1 != default!) {
                    return errΔ1;
                }
                return verifyPKCS1v15(kʗ1.PublicKey(), sha256ˢ, hash, sig);
            });
        }

        return (k, default!);
    }
}

// errDivisorTooLarge is returned by [totient] when gcd(p-1, q-1) is too large.
internal static error errDivisorTooLarge = errors.New("divisor too large"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rsaInternalErrorGcdABIsˢ = "rsa: internal error: gcd(a, b) is even"u8;
internal static readonly @string rsaInternalErrorGcdABIsˢ2 = "rsa: internal error: gcd(a, b) is zero"u8;
internal static readonly @string rsaInternalErrorBIsNotˢ = "rsa: internal error: b is not divisible by gcd(a, b)"u8;

// totient computes the Carmichael totient function λ(N) = lcm(p-1, q-1).
internal static (ж<bigmod.Modulus>, error) totient(ж<bigmod.Modulus> Ꮡp, ж<bigmod.Modulus> Ꮡq) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var q = ref Ꮡq.DerefOrNull();

    var (a, b) = (p.Nat().SubOne(Ꮡp), q.Nat().SubOne(Ꮡq));

    // lcm(a, b) = a×b / gcd(a, b) = a × (b / gcd(a, b))

    // Our GCD requires at least one of the numbers to be odd. For LCM we only
    // need to preserve the larger prime power of each prime factor, so we can
    // right-shift the number with the fewest trailing zeros until it's odd.
    // For odd a, b and m >= n, lcm(a×2ᵐ, b×2ⁿ) = lcm(a×2ᵐ, b).
    nuint az = a.TrailingZeroBitsVarTime();
    nuint bz = b.TrailingZeroBitsVarTime();
    if (az < bz){
        a = a.ShiftRightVarTime(az);
    } else {
        b = b.ShiftRightVarTime(bz);
    }

    var (gcd, err) = bigmod.NewNat().GCDVarTime(a, b);
    if (err != default!) {
        return (default!, err);
    }
    if (gcd.IsOdd() == 0) {
        return (default!, errors.New(rsaInternalErrorGcdABIsˢ));
    }

    // To avoid implementing multiple-precision division, we just try again if
    // the divisor doesn't fit in a single word. This would have a chance of
    // 2⁻⁶⁴ on 64-bit platforms, and 2⁻³² on 32-bit platforms, but testing 2⁻⁶⁴
    // edge cases is impractical, and we'd rather not behave differently on
    // different platforms, so we reject divisors above 2³²-1.
    if (gcd.BitLenVarTime() > 32) {
        return (default!, errDivisorTooLarge);
    }
    if (gcd.IsZero() == 1 || gcd.Bits()[0] == 0) {
        return (default!, errors.New(rsaInternalErrorGcdABIsˢ2));
    }
    {
        nuint rem = b.DivShortVarTime(gcd.Bits()[0]); if (rem != 0) {
            return (default!, errors.New(rsaInternalErrorBIsNotˢ));
        }
    }

    return bigmod.NewModulusProduct(a.Bytes(Ꮡp), b.Bytes(Ꮡq));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rsaPrimeSizeMustBeAtˢ = "rsa: prime size must be at least 16 bits"u8;

// randomPrime returns a random prime number of the given bit size following
// the process in FIPS 186-5, Appendix A.1.3.
internal static (slice<byte>, error) randomPrime(io.Reader rand, nint bits) {
    if (bits < 16) {
        return (default!, errors.New(rsaPrimeSizeMustBeAtˢ));
    }

    var b = new slice<byte>((bits + 7) / 8);
    while (ᐧ) {
        {
            var err = drbg.ReadWithReader(rand, b); if (err != default!) {
                return (default!, err);
            }
        }
        // Clear the most significant bits to reach the desired size. We use a
        // mask rather than right-shifting b[0] to make it easier to inject test
        // candidates, which can be represented as simple big-endian integers.
        nint excess = len(b) * 8 - bits;
        b[0] &= (byte)(((byte)0b1111_1111).Rsh((int64)(excess)));

        // Don't let the value be too small: set the most significant two bits.
        // Setting the top two bits, rather than just the top bit, means that
        // when two of these values are multiplied together, the result isn't
        // ever one bit short.
        if (excess < 7){
            b[0] |= (byte)(((byte)0b1100_0000).Rsh((int64)(excess)));
        } else {
            b[0] |= (byte)(0b0000_0001);
            b[1] |= (byte)(0b1000_0000);
        }

        // Make the value odd since an even number certainly isn't prime.
        b[len(b) - 1] |= (byte)(1);

        // We don't need to check for p >= √2 × 2^(bits-1) (steps 4.4 and 5.4)
        // because we set the top two bits above, so
        //
        //   p > 2^(bits-1) + 2^(bits-2) = 3⁄2 × 2^(bits-1) > √2 × 2^(bits-1)
        //

        // Step 5.5 requires checking that |p - q| > 2^(nlen/2 - 100).
        //
        // The probability of |p - q| ≤ k where p and q are uniformly random in
        // the range (a, b) is 1 - (b-a-k)^2 / (b-a)^2, so the probability of
        // this check failing during key generation is 2⁻⁹⁷.
        //
        // We still need to check to comply with FIPS 186-5, but knowing it has
        // negligible chance of failure we can defer the check to the end of key
        // generation and return an error if it fails. See [checkPrivateKey].

        if (isPrime(b)) {
            return (b, default!);
        }
    }
}

// isPrime runs the Miller-Rabin Probabilistic Primality Test from
// FIPS 186-5, Appendix B.3.1.
//
// w must be a random odd integer greater than three in big-endian order.
// isPrime might return false positives for adversarially chosen values.
//
// isPrime is not constant-time.
internal static bool isPrime(slice<byte> w) {
    var (mr, err) = millerRabinSetup(w);
    if (err != default!) {
        // w is zero, one, or even.
        return false;
    }

    // Before Miller-Rabin, rule out most composites with trial divisions.
    for (nint i = 0; i < len(primes); i += 3) {
        nuint p1 = primes[i];
        nuint p2 = primes[i + 1];
        nuint p3 = primes[i + 2];
        nuint r = (~mr).w.Nat().DivShortVarTime(p1 * p2 * p3);
        if (r % p1 == 0 || r % p2 == 0 || r % p3 == 0) {
            return false;
        }
    }

    // iterations is the number of Miller-Rabin rounds, each with a
    // randomly-selected base.
    //
    // The worst case false positive rate for a single iteration is 1/4 per
    // https://eprint.iacr.org/2018/749, so if w were selected adversarially, we
    // would need up to 64 iterations to get to a negligible (2⁻¹²⁸) chance of
    // false positive.
    //
    // However, since this function is only used for randomly-selected w in the
    // context of RSA key generation, we can use a smaller number of iterations.
    // The exact number depends on the size of the prime (and the implied
    // security level). See BoringSSL for the full formula.
    // https://cs.opensource.google/boringssl/boringssl/+/master:crypto/fipsmodule/bn/prime.c.inc;l=208-283;drc=3a138e43
    nint bits = (~mr).w.BitLen();
    nint iterations = default!;
    switch (ᐧ) {
    case {} when bits is >= 3747: {
        iterations = 3;
        break;
    }
    case {} when bits is >= 1345: {
        iterations = 4;
        break;
    }
    case {} when bits is >= 476: {
        iterations = 5;
        break;
    }
    case {} when bits is >= 400: {
        iterations = 6;
        break;
    }
    case {} when bits is >= 347: {
        iterations = 7;
        break;
    }
    case {} when bits is >= 308: {
        iterations = 8;
        break;
    }
    case {} when bits is >= 55: {
        iterations = 27;
        break;
    }
    default: {
        iterations = 34;
        break;
    }}

    var b = new slice<byte>((bits + 7) / 8);
    while (ᐧ) {
        drbg.Read(b);
        nint excess = len(b) * 8 - bits;
        b[0] &= (byte)(((byte)0b1111_1111).Rsh((int64)(excess)));
        var (result, errΔ1) = millerRabinIteration(ref (mr).DerefOrNull(), b);
        if (errΔ1 != default!) {
            // b was rejected.
            continue;
        }
        if (result == millerRabinCOMPOSITE) {
            return false;
        }
        iterations--;
        if (iterations == 0) {
            return true;
        }
    }
}

// primes are the first prime numbers (except 2), such that the product of any
// three primes fits in a uint32.
//
// More primes cause fewer Miller-Rabin tests of composites (nothing can help
// with the final test on the actual prime) but have diminishing returns: these
// 255 primes catch 84.9% of composites, the next 255 would catch 1.5% more.
// Adding primes can still be marginally useful since they only compete with the
// (much more expensive) first Miller-Rabin round for candidates that were not
// rejected by the previous primes.
internal static slice<nuint> primes = new nuint[]{
    3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53,
    59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127,
    131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199,
    211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283,
    293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383,
    389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467,
    479, 487, 491, 499, 503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577,
    587, 593, 599, 601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661,
    673, 677, 683, 691, 701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769,
    773, 787, 797, 809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877,
    881, 883, 887, 907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983,
    991, 997, 1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069,
    1087, 1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181,
    1187, 1193, 1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283,
    1289, 1291, 1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373, 1381, 1399,
    1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487,
    1489, 1493, 1499, 1511, 1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583,
    1597, 1601, 1607, 1609, 1613, 1619
}.slice();

partial struct millerRabin {
    internal ж<bigmod.Modulus> w;
    internal nuint a;
    internal slice<byte> m;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string candidateIsEvenˢ = "candidate is even"u8;
internal static readonly @string candidateIsOneˢ = "candidate is one"u8;

// millerRabinSetup prepares state that's reused across multiple iterations of
// the Miller-Rabin test.
internal static (ж<millerRabin>, error) millerRabinSetup(slice<byte> w) {
    var mr = Ꮡ(new millerRabin(nil));

    // Check that w is odd, and precompute Montgomery parameters.
    var (wm, err) = bigmod.NewModulus(w);
    if (err != default!) {
        return (default!, err);
    }
    if (wm.Nat().IsOdd() == 0) {
        return (default!, errors.New(candidateIsEvenˢ));
    }
    mr.Value.w = wm;

    // Compute m = (w-1)/2^a, where m is odd.
    var wMinus1 = (~mr).w.Nat().SubOne((~mr).w);
    if (wMinus1.IsZero() == 1) {
        return (default!, errors.New(candidateIsOneˢ));
    }
    mr.Value.a = wMinus1.TrailingZeroBitsVarTime();

    // Store mr.m as a big-endian byte slice with leading zero bytes removed,
    // for use with [bigmod.Nat.Exp].
    var m = wMinus1.ShiftRightVarTime((~mr).a);
    mr.Value.m = m.Bytes((~mr).w);
    while ((~mr).m[0] == 0) {
        mr.Value.m = (~mr).m[1..];
    }

    return (mr, default!);
}

internal const bool millerRabinCOMPOSITE = false;

internal const bool millerRabinPOSSIBLYPRIME = true;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string incorrectLengthˢ = "incorrect length"u8;
internal static readonly @string outOfRangeCandidateˢ = "out-of-range candidate"u8;

internal static (bool, error) millerRabinIteration(ref millerRabin mr, slice<byte> bb) {
    // Reject b ≤ 1 or b ≥ w − 1.
    if (len(bb) != (mr.w.BitLen() + 7) / 8) {
        return (false, errors.New(incorrectLengthˢ));
    }
    var b = bigmod.NewNat();
    {
        var (_, err) = b.SetBytes(bb, mr.w); if (err != default!) {
            return (false, err);
        }
    }
    if (b.IsZero() == 1 || b.IsOne() == 1 || b.IsMinusOne(mr.w) == 1) {
        return (false, errors.New(outOfRangeCandidateˢ));
    }

    // Compute b^(m*2^i) mod w for successive i.
    // If b^m mod w = 1, b is a possible prime.
    // If b^(m*2^i) mod w = -1 for some 0 <= i < a, b is a possible prime.
    // Otherwise b is composite.

    // Start by computing and checking b^m mod w (also the i = 0 case).
    var z = bigmod.NewNat().Exp(b, mr.m, mr.w);
    if (z.IsOne() == 1 || z.IsMinusOne(mr.w) == 1) {
        return (millerRabinPOSSIBLYPRIME, default!);
    }

    // Check b^(m*2^i) mod w = -1 for 0 < i < a.
    foreach (var _ᴛ1 in range<nuint>(mr.a - 1)) {
        z.Mul(z, mr.w);
        if (z.IsMinusOne(mr.w) == 1) {
            return (millerRabinPOSSIBLYPRIME, default!);
        }
        if (z.IsOne() == 1) {
            // Future squaring will not turn z == 1 into -1.
            break;
        }
    }

    return (millerRabinCOMPOSITE, default!);
}

} // end rsa_package
