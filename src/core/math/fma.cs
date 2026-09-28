// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bits = math.bits_package;
using math;

partial class math_package {

internal static uint64 zero(uint64 x) {
    if (x == 0) {
        return 1;
    }
    return 0;
}

// branchless:
// return ((x>>1 | x&1) - 1) >> 63
internal static uint64 nonzero(uint64 x) {
    if (x != 0) {
        return 1;
    }
    return 0;
}

// branchless:
// return 1 - ((x>>1|x&1)-1)>>63
internal static (uint64 r1, uint64 r2) shl(uint64 u1, uint64 u2, nuint n) {
    uint64 r1 = default!;
    uint64 r2 = default!;

    r1 = (uint64)((uint64)(u1.Lsh(n) | u2.Rsh((64 - n))) | u2.Lsh((n - 64)));
    r2 = u2.Lsh(n);
    return (r1, r2);
}

internal static (uint64 r1, uint64 r2) shr(uint64 u1, uint64 u2, nuint n) {
    uint64 r1 = default!;
    uint64 r2 = default!;

    r2 = (uint64)((uint64)(u2.Rsh(n) | u1.Lsh((64 - n))) | u1.Rsh((n - 64)));
    r1 = u1.Rsh(n);
    return (r1, r2);
}

// shrcompress compresses the bottom n+1 bits of the two-word
// value into a single bit. the result is equal to the value
// shifted to the right by n, except the result's 0th bit is
// set to the bitwise OR of the bottom n+1 bits.
internal static (uint64 r1, uint64 r2) shrcompress(uint64 u1, uint64 u2, nuint n) {
    uint64 r1 = default!;
    uint64 r2 = default!;

    // TODO: Performance here is really sensitive to the
    // order/placement of these branches. n == 0 is common
    // enough to be in the fast path. Perhaps more measurement
    // needs to be done to find the optimal order/placement?
    switch (ᐧ) {
    case {} when n is 0: {
        return (u1, u2);
    }
    case {} when n is 64: {
        return (0, (uint64)(u1 | nonzero(u2)));
    }
    case {} when n is >= 128: {
        return (0, nonzero((uint64)(u1 | u2)));
    }
    case {} when n is < 64: {
        (r1, r2) = shr(u1, u2, n);
        r2 |= (uint64)(nonzero((uint64)(u2 & (((uint64)1).Lsh(n) - 1))));
        break;
    }
    case {} when n is < 128: {
        (r1, r2) = shr(u1, u2, n);
        r2 |= (uint64)(nonzero((uint64)((uint64)(u1 & (((uint64)1).Lsh((n - 64)) - 1)) | u2)));
        break;
    }}

    return (r1, r2);
}

internal static int32 /*l*/ lz(uint64 u1, uint64 u2) {
    int32 l = default!;

    l = (int32)bits.LeadingZeros64(u1);
    if (l == 64) {
        l += (int32)bits.LeadingZeros64(u2);
    }
    return l;
}

// split splits b into sign, biased exponent, and mantissa.
// It adds the implicit 1 bit to the mantissa for normal values,
// and normalizes subnormal values.
internal static (uint32 sign, int32 exp, uint64 mantissa) split(uint64 b) {
    uint32 sign = default!;
    int32 exp = default!;
    uint64 mantissa = default!;

    sign = (uint32)((b >> (int)(63)));
    exp = (int32)((int32)((b >> (int)(52))) & (int32)mask);
    mantissa = (uint64)(b & (uint64)fracMask);
    if (exp == 0){
        // Normalize value if subnormal.
        nuint shift = (nuint)(bits.LeadingZeros64(mantissa) - 11);
        mantissa.LshAssign(shift);
        exp = 1 - (int32)shift;
    } else {
        // Add implicit 1 bit
        mantissa |= (uint64)(((uint64)1 << (int)(52)));
    }
    return (sign, exp, mantissa);
}

// FMA returns x * y + z, computed with only one rounding.
// (That is, FMA returns the fused multiply-add of x, y, and z.)
public static float64 FMA(float64 x, float64 y, float64 z) {
    var (bx, by, bz) = (Float64bits(x), Float64bits(y), Float64bits(z));
    // Inf or NaN or zero involved. At most one rounding will occur.
    if (x == 0.0D || y == 0.0D || z == 0.0D || (uint64)(bx & (uint64)uvinf) == uvinf || (uint64)(by & (uint64)uvinf) == uvinf) {
        return x * y + z;
    }
    // Handle non-finite z separately. Evaluating x*y+z where
    // x and y are finite, but z is infinite, should always result in z.
    if ((uint64)(bz & (uint64)uvinf) == uvinf) {
        return z;
    }
    // Inputs are (sub)normal.
    // Split x, y, z into sign, exponent, mantissa.
    var (xs, xe, xm) = split(bx);
    var (ys, ye, ym) = split(by);
    var (zs, ze, zm) = split(bz);
    // Compute product p = x*y as sign, exponent, two-word mantissa.
    // Start with exponent. "is normal" bit isn't subtracted yet.
    var pe = xe + ye - (int32)bias + 1;
    // pm1:pm2 is the double-word mantissa for the product p.
    // Shift left to leave top bit in product. Effectively
    // shifts the 106-bit product to the left by 21.
    var (pm1, pm2) = bits.Mul64((xm << (int)(10)), (ym << (int)(11)));
    var (zm1, zm2) = ((zm << (int)(10)), (uint64)0);
    var ps = (uint32)(xs ^ ys); // product sign
    // normalize to 62nd bit
    nuint is62zero = (nuint)((uint64)(((~pm1 >> (int)(62))) & 1));
    (pm1, pm2) = shl(pm1, pm2, is62zero);
    pe -= (int32)is62zero;
    // Swap addition operands so |p| >= |z|
    if (pe < ze || pe == ze && pm1 < zm1) {
        (ps, pe, pm1, pm2, zs, ze, zm1, zm2) = (zs, ze, zm1, zm2, ps, pe, pm1, pm2);
    }
    // Special case: if p == -z the result is always +0 since neither operand is zero.
    if (ps != zs && pe == ze && pm1 == zm1 && pm2 == zm2) {
        return 0D;
    }
    // Align significands
    (zm1, zm2) = shrcompress(zm1, zm2, (nuint)(pe - ze));
    // Compute resulting significands, normalizing if necessary.
    uint64 m = default!;
    uint64 c = default!;
    if (ps == zs){
        // Adding (pm1:pm2) + (zm1:zm2)
        (pm2, c) = bits.Add64(pm2, zm2, 0);
        (pm1, _) = bits.Add64(pm1, zm1, c);
        pe -= (int32)((~pm1 >> (int)(63)));
        (pm1, m) = shrcompress(pm1, pm2, (nuint)(64 + (pm1 >> (int)(63))));
    } else {
        // Subtracting (pm1:pm2) - (zm1:zm2)
        // TODO: should we special-case cancellation?
        (pm2, c) = bits.Sub64(pm2, zm2, 0);
        (pm1, _) = bits.Sub64(pm1, zm1, c);
        var nz = lz(pm1, pm2);
        pe -= nz;
        (m, pm2) = shl(pm1, pm2, (nuint)(nz - 1));
        m |= (uint64)(nonzero(pm2));
    }
    // Round and break ties to even
    if (pe > (int32)(1022 + bias) || pe == (int32)(1022 + bias) && ((m + ((uint64)1 << (int)(9))) >> (int)(63)) == 1) {
        // rounded value overflows exponent range
        return Float64frombits((uint64)(((uint64)ps << (int)(63)) | (uint64)uvinf));
    }
    if (pe < 0) {
        nuint n = (nuint)(-pe);
        m = (uint64)(m.Rsh(n) | nonzero((uint64)(m & (((uint64)1).Lsh(n) - 1))));
        pe = 0;
    }
    m = (uint64)((((m + ((uint64)1 << (int)(9))) >> (int)(10))) & ~zero((uint64)(((uint64)(m & ((1 << (int)(10)) - 1))) ^ ((uint64)1 << (int)(9)))));
    pe &= (int32)(-(int32)nonzero(m));
    return Float64frombits(((uint64)ps << (int)(63)) + ((uint64)pe << (int)(52)) + m);
}

} // end math_package
