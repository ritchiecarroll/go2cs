// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements multi-precision decimal numbers.
// The implementation is for float to decimal conversion only;
// not general purpose use.
// The only operations are precise conversion from binary to
// decimal and rounding.
//
// The key observation and some code (shr) is borrowed from
// strconv/decimal.go: conversion of binary fractional values can be done
// precisely in multi-precision decimal because 2 divides 10 (required for
// >> of mantissa); but conversion of decimal floating-point values cannot
// be done precisely in binary representation.
//
// In contrast to strconv/decimal.go, only right shift is implemented in
// decimal format - left shift can be done precisely in binary format.
namespace go.math;

partial class big_package {

// A decimal represents an unsigned floating-point number in decimal representation.
// The value of a non-zero decimal d is d.mant * 10**d.exp with 0.1 <= d.mant < 1,
// with the most-significant mantissa digit at index 0. For the zero decimal, the
// mantissa length and exponent are 0.
// The zero value for decimal represents a ready-to-use 0.0.
[GoType] partial struct @decimal {
    internal slice<byte> mant; // mantissa ASCII digits, big-endian
    internal nint exp;   // exponent
}

// at returns the i'th mantissa digit, starting with the most significant digit at 0.
[GoRecv] internal static byte at(this ref @decimal d, nint i) {
    if (0 <= i && i < len(d.mant)) {
        return d.mant[i];
    }
    return (rune)'0';
}

// Maximum shift amount that can be done in one pass without overflow.
// A Word has _W bits and (1<<maxShift - 1)*10 + 9 must fit into Word.
internal static UntypedInt maxShift => /* _W - 4 */ 60;

// TODO(gri) Since we know the desired decimal precision when converting
// a floating-point number, we may be able to limit the number of decimal
// digits that need to be computed by init by providing an additional
// precision argument and keeping track of when a number was truncated early
// (equivalent of "sticky bit" in binary rounding).
// TODO(gri) Along the same lines, enforce some limit to shift magnitudes
// to avoid "infinitely" long running conversions (until we run out of space).

// Init initializes x to the decimal representation of m << shift (for
// shift >= 0), or m >> -shift (for shift < 0).
internal static void init(this ж<@decimal> Ꮡx, nat m, nint shift) {
    ref var x = ref Ꮡx.DerefOrNull();

    // special case 0
    if (len(m) == 0) {
        x.mant = x.mant[..0];
        x.exp = 0;
        return;
    }
    // Optimization: If we need to shift right, first remove any trailing
    // zero bits from m to reduce shift amount that needs to be done in
    // decimal format (since that is likely slower).
    if (shift < 0) {
        nuint ntz = m.trailingZeroBits();
        nuint sΔ1 = (nuint)(-shift);
        if (sΔ1 >= ntz) {
            sΔ1 = ntz; // shift at most ntz bits
        }
        m = ((nat)default!).shr(m, sΔ1);
        shift += (nint)sΔ1;
    }
    // Do any shift left in binary representation.
    if (shift > 0) {
        m = ((nat)default!).shl(m, (nuint)shift);
        shift = 0;
    }
    // Convert mantissa into decimal representation.
    var s = m.utoa(10);
    nint n = len(s);
    x.exp = n;
    // Trim trailing zeros; instead the exponent is tracking
    // the decimal point independent of the number of digits.
    while (n > 0 && s[n - 1] == (rune)'0') {
        n--;
    }
    x.mant = appendꓸꓸꓸ(x.mant[..0], s.slice(0, n));
    // Do any (remaining) shift right in decimal representation.
    if (shift < 0) {
        while (shift < (nint)(-maxShift)) {
            shr(ref (Ꮡx).DerefOrNull(), maxShift);
            shift += maxShift;
        }
        shr(ref (Ꮡx).DerefOrNull(), (nuint)(-shift));
    }
}

// shr implements x >> s, for s <= maxShift.
internal static void shr(ref @decimal x, nuint s) {
    // Division by 1<<s using shift-and-subtract algorithm.
    // pick up enough leading digits to cover first shift
    nint r = 0; // read index
    Word n = default!;
    while ((n >> (int)(s)) == 0 && r < len(x.mant)) {
        Word ch = ((Word)(nuint)x.mant[r]);
        r++;
        n = n * 10 + ch - (rune)'0';
    }
    if (n == 0) {
        // x == 0; shouldn't get here, but handle anyway
        x.mant = x.mant[..0];
        return;
    }
    while ((n >> (int)(s)) == 0) {
        r++;
        n *= 10;
    }
    x.exp += 1 - r;
    // read a digit, write a digit
    nint w = 0; // write index
    Word mask = (((Word)1) << (int)(s)) - 1;
    while (r < len(x.mant)) {
        Word ch = ((Word)(nuint)x.mant[r]);
        r++;
        Word d = (n >> (int)(s));
        n &= (Word)(mask); // n -= d << s
        x.mant[w] = (byte)(nuint)(d + (rune)'0');
        w++;
        n = n * 10 + ch - (rune)'0';
    }
    // write extra digits that still fit
    while (n > 0 && w < len(x.mant)) {
        Word d = (n >> (int)(s));
        n &= (Word)(mask);
        x.mant[w] = (byte)(nuint)(d + (rune)'0');
        w++;
        n = n * 10;
    }
    x.mant = x.mant.slice(0, w); // the number may be shorter (e.g. 1024 >> 10)
    // append additional digits that didn't fit
    while (n > 0) {
        Word d = (n >> (int)(s));
        n &= (Word)(mask);
        x.mant = append(x.mant, (byte)(nuint)(d + (rune)'0'));
        n = n * 10;
    }
    trim(ref x);
}

[GoRecv] internal static @string String(this ref @decimal x) {
    if (len(x.mant) == 0) {
        return "0"u8;
    }
    slice<byte> buf = default!;
    switch (ᐧ) {
    case {} when x.exp is <= 0: {
        buf = new slice<byte>(0, 2 + (-x.exp) + len(x.mant));
        buf = append(buf, ((@string)"0."u8).ꓸꓸꓸ);
        buf = appendZeros(buf, -x.exp);
        buf = appendꓸꓸꓸ(buf, x.mant);
        break;
    }
    case {} when x.exp < len(x.mant): {
        buf = new slice<byte>(0, 1 + len(x.mant));
        buf = appendꓸꓸꓸ(buf, x.mant.slice(0, x.exp));
        buf = append(buf, (byte)((rune)'.'));
        buf = appendꓸꓸꓸ(buf, x.mant.slice(x.exp));
        break;
    }
    default: {
        buf = new slice<byte>(0, x.exp);
        buf = appendꓸꓸꓸ(buf, x.mant);
        buf = appendZeros(buf, x.exp - len(x.mant));
        break;
    }}

    return ((@string)buf);
}

// appendZeros appends n 0 digits to buf and returns buf.
internal static slice<byte> appendZeros(slice<byte> buf, nint n) {
    for (; n > 0; n--) {
        buf = append(buf, (byte)((rune)'0'));
    }
    return buf;
}

// shouldRoundUp reports if x should be rounded up
// if shortened to n digits. n must be a valid index
// for x.mant.
internal static bool shouldRoundUp(ref @decimal x, nint n) {
    if (x.mant[n] == (rune)'5' && n + 1 == len(x.mant)) {
        // exactly halfway - round to even
        return n > 0 && (byte)((x.mant[n - 1] - (rune)'0') & 1) != 0;
    }
    // not halfway - digit tells all (x.mant has no trailing zeros)
    return x.mant[n] >= (rune)'5';
}

// round sets x to (at most) n mantissa digits by rounding it
// to the nearest even value with n (or fever) mantissa digits.
// If n < 0, x remains unchanged.
internal static void round(this ж<@decimal> Ꮡx, nint n) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (n < 0 || n >= len(x.mant)) {
        return; // nothing to do
    }
    if (shouldRoundUp(ref (Ꮡx).DerefOrNull(), n)){
        x.roundUp(n);
    } else {
        Ꮡx.roundDown(n);
    }
}

[GoRecv] internal static void roundUp(this ref @decimal x, nint n) {
    if (n < 0 || n >= len(x.mant)) {
        return; // nothing to do
    }
    // 0 <= n < len(x.mant)
    // find first digit < '9'
    while (n > 0 && x.mant[n - 1] >= (rune)'9') {
        n--;
    }
    if (n == 0) {
        // all digits are '9's => round up to '1' and update exponent
        x.mant[0] = (rune)'1'; // ok since len(x.mant) > n
        x.mant = x.mant[..1];
        x.exp++;
        return;
    }
    // n > 0 && x.mant[n-1] < '9'
    x.mant[n - 1]++;
    x.mant = x.mant.slice(0, n);
}

// x already trimmed
internal static void roundDown(this ж<@decimal> Ꮡx, nint n) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (n < 0 || n >= len(x.mant)) {
        return; // nothing to do
    }
    x.mant = x.mant.slice(0, n);
    trim(ref (Ꮡx).DerefOrNull());
}

// trim cuts off any trailing zeros from x's mantissa;
// they are meaningless for the value of x.
internal static void trim(ref @decimal x) {
    nint i = len(x.mant);
    while (i > 0 && x.mant[i - 1] == (rune)'0') {
        i--;
    }
    x.mant = x.mant.slice(0, i);
    if (i == 0) {
        x.exp = 0;
    }
}

} // end big_package
