// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bits = math.bits_package;
using math;

partial class strconv_package {

internal static array<uint64> uint64pow10 = new uint64[]{
    1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000,
    10000000000, 100000000000, 1000000000000, 10000000000000, 100000000000000, 1000000000000000, 10000000000000000, 100000000000000000, 1000000000000000000, 10000000000000000000
}.array();

// fixedFtoa formats a number of decimal digits of mant*(2^exp) into d,
// where mant > 0 and 1 ≤ digits ≤ 18.
// If fmt == 'f', digits is a conservative overestimate, and the final
// number of digits is prec past the decimal point.
internal static void fixedFtoa(ref decimalSlice d, uint64 mant, nint exp, nint digits, nint prec, byte fmt) {
    // The strategy here is to multiply (mant * 2^exp) by a power of 10
    // to make the resulting integer be the number of digits we want.
    //
    // Adams proved in the Ryu paper that 128-bit precision in the
    // power-of-10 constant is sufficient to produce correctly
    // rounded output for all float64s, up to 18 digits.
    // https://dl.acm.org/doi/10.1145/3192366.3192369
    //
    // TODO(rsc): The paper is not focused on, nor terribly clear about,
    // this fact in this context, and the proof seems too complicated.
    // Post a shorter, more direct proof and link to it here.

    if (digits > 18) {
        throw panic("fixedFtoa called with digits > 18");
    }

    // Shift mantissa to have 64 bits,
    // so that the 192-bit product below will
    // have at least 63 bits in its top word.
    nint b = 64 - bits.Len64(mant);
    mant.LshAssign((int64)(b));
    exp -= b;

    // We have f = mant * 2^exp ≥ 2^(63+exp)
    // and we want to multiply it by some 10^p
    // to make it have the number of digits plus one rounding bit:
    //
    //	2 * 10^(digits-1) ≤ f * 10^p < ~2 * 10^digits
    //
    // The lower bound is required, but the upper bound is approximate:
    // we must not have too few digits, but we can round away extra ones.
    //
    //	f * 10^p ≥ 2 * 10^(digits-1)
    //	10^p ≥ 2 * 10^(digits-1) / f                         [dividing by f]
    //	p ≥ (log₁₀ 2) + (digits-1) - log₁₀ f                 [taking log₁₀]
    //	p ≥ (log₁₀ 2) + (digits-1) - log₁₀ (mant * 2^exp)    [expanding f]
    //	p ≥ (log₁₀ 2) + (digits-1) - (log₁₀ 2) * (64 + exp)  [mant < 2⁶⁴]
    //	p ≥ (digits - 1) - (log₁₀ 2) * (63 + exp)            [refactoring]
    //
    // Once we have p, we can compute the scaled value:
    //
    //	dm * 2^de = mant * 2^exp * 10^p
    //	          = mant * 2^exp * pow/2^128 * 2^exp2.
    //	          = (mant * pow/2^128) * 2^(exp+exp2).
    nint p = (digits - 1) - mulLog10_2(63 + exp);
    var (pow, exp2, ok) = pow10(p);
    if (!ok) {
        // This never happens due to the range of float32/float64 exponent
        throw panic("fixedFtoa: pow10 out of range");
    }
    if (-22 <= p && p < 0) {
        // Special case: Let q=-p. q is in [1,22]. We are dividing by 10^q
        // and the mantissa may be a multiple of 5^q (5^22 < 2^53),
        // in which case the division must be computed exactly and
        // recorded as exact for correct rounding. Our normal computation is:
        //
        //	dm = floor(mant * floor(10^p * 2^s))
        //
        // for some scaling shift s. To make this an exact division,
        // it suffices to change the inner floor to a ceil:
        //
        //	dm = floor(mant * ceil(10^p * 2^s))
        //
        // In the range of values we are using, the floor and ceil
        // cancel each other out and the high 64 bits of the product
        // come out exactly right.
        // (This is the same trick compilers use for division by constants.
        // See Hacker's Delight, 2nd ed., Chapter 10.)
        pow.Lo++;
    }
    var (dm, lo1, lo0) = umul192(mant, pow);
    nint de = exp + exp2;

    // Check whether any bits have been truncated from dm.
    // If so, set dt != 0. If not, leave dt == 0 (meaning dm is exact).
    nuint dt = default!;
    switch (ᐧ) {
    default: {
        // Most powers of 10 use a truncated constant,
        // meaning the result is also truncated.
        dt = 1;
        break;
    }
    case {} when 0 <= p && p <= 55: {
        // Small positive powers of 10 (up to 10⁵⁵) can be represented
        // precisely in a 128-bit mantissa (5⁵⁵ ≤ 2¹²⁸), so the only truncation
        // comes from discarding the low bits of the 192-bit product.
        //
        // TODO(rsc): The new proof mentioned above should also
        // prove that we can't have lo1 == 0 and lo0 != 0.
        // After proving that, drop computation and use of lo0 here.
        dt = bool2uint((lo1 | lo0) != 0);
        break;
    }
    case {} when -22 <= p && p < 0 && divisiblePow5(mant, -p): {
        // If the original mantissa was a multiple of 5^p,
        // the result is exact. (See comment above for pow.Lo++.)
        dt = 0;
        break;
    }}

    // The value we want to format is dm * 2^de, where de < 0.
    // Multply by 2^de by shifting, but leave one extra bit for rounding.
    // After the shift, the "integer part" of dm is dm>>1,
    // the "rounding bit" (the first fractional bit) is dm&1,
    // and the "truncated bit" (have any bits been discarded?) is dt.
    nint shift = -de - 1;
    dt |= (nuint)(bool2uint((dm & (((uint64)1).Lsh((int64)(shift)) - 1)) != 0));
    dm.RshAssign((int64)(shift));

    // Set decimal point in eventual formatted digits,
    // so we can update it as we adjust the digits.
    d.dp = digits - p;

    // Trim excess digit if any, updating truncation and decimal point.
    // The << 1 is leaving room for the rounding bit.
    var max = uint64pow10[digits] << 1;
    if (dm >= max) {
        nuint r = default!;
        (dm, r) = (dm / 10, (nuint)(dm % 10));
        dt |= (nuint)(bool2uint(r != 0));
        d.dp++;
    }

    // If this is %.*f we may have overestimated the digits needed.
    // Now that we know where the decimal point is,
    // trim to the actual number of digits, which is d.dp+prec.
    if (fmt == (rune)'f' && digits != d.dp + prec) {
        while (digits > d.dp + prec) {
            nuint r = default!;
            (dm, r) = (dm / 10, (nuint)(dm % 10));
            dt |= (nuint)(bool2uint(r != 0));
            digits--;
        }

        // Dropping those digits can create a new leftmost
        // non-zero digit, like if we are formatting %.1f and
        // convert 0.09 -> 0.1. Detect and adjust for that.
        if (digits <= 0) {
            digits = 1;
            d.dp++;
        }

        max = uint64pow10[digits] << 1;
    }

    // Round and shift away rounding bit.
    // We want to round up when
    // (a) the fractional part is > 0.5 (dm&1 != 0 and dt == 1)
    // (b) or the fractional part is ≥ 0.5 and the integer part is odd
    //     (dm&1 != 0 and dm&2 != 0).
    // The bitwise expression encodes that logic.
    dm += (uint64)((nuint)dm & (dt | (nuint)dm >> 1) & 1);
    dm >>= (int)(1);
    if (dm == max >> 1) {
        // 999... rolled over to 1000...
        dm = uint64pow10[digits - 1];
        d.dp++;
    }

    // Format digits into d.
    if (dm != 0) {
        if (formatBase10(d.d.slice(0, digits), dm) != 0) {
            throw panic("formatBase10");
        }
        d.nd = digits;
        while (d.d[d.nd - 1] == (rune)'0') {
            d.nd--;
        }
    }
}

} // end strconv_package
