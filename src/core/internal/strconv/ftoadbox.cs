// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class strconv_package {

// Binary to decimal conversion using the Dragonbox algorithm by Junekey Jeon.
//
// Fixed precision format is not supported by the Dragonbox algorithm
// so we continue to use Ryū-printf for this purpose.
// See https://github.com/jk-jeon/dragonbox/issues/38 for more details.
//
// For binary to decimal rounding, uses round to nearest, tie to even.
// For decimal to binary rounding, assumes round to nearest, tie to even.
//
// The original paper by Junekey Jeon can be found at:
// https://github.com/jk-jeon/dragonbox/blob/d5dc40ae6a3f1a4559cda816738df2d6255b4e24/other_files/Dragonbox.pdf
//
// The reference implementation in C++ by Junekey Jeon can be found at:
// https://github.com/jk-jeon/dragonbox/blob/6c7c925b571d54486b9ffae8d9d18a822801cbda/subproject/simple/include/simple_dragonbox.h

// dragonboxFtoa computes the decimal significand and exponent
// from the binary significand and exponent using the Dragonbox algorithm
// and formats the decimal floating point number in d.
internal static void dboxFtoa(ref decimalSlice d, uint64 mant, nint exp, bool denorm, nint bitSize) {
    if (bitSize == 32) {
        dboxFtoa32(ref d, (uint32)mant, exp, denorm);
        return;
    }
    dboxFtoa64(ref d, mant, exp, denorm);
}

internal static void dboxFtoa64(ref decimalSlice d, uint64 mant, nint exp, bool denorm) {
    if (mant == (uint64)((uint64)1 << (int)(float64MantBits)) && !denorm) {
        // Algorithm 5.6 (page 24).
        nint k0Δ1 = -mulLog10_2MinusLog10_4Over3(exp);
        var (φΔ1, βΔ1) = dboxPow64(k0Δ1, exp);
        var (xi, ziΔ1) = dboxRange64(φΔ1, βΔ1);
        if (exp != 2 && exp != 3) {
            xi++;
        }
        var q = ziΔ1 / 10;
        if (xi <= q * 10) {
            var (qΔ1, zeros) = trimZeros(q);
            dboxDigits(ref d, qΔ1, -k0Δ1 + 1 + zeros);
            return;
        }
        var yruΔ1 = dboxRoundUp64(φΔ1, βΔ1);
        if (exp == -77 && yruΔ1 % 2 != 0){
            yruΔ1--;
        } else 
        if (yruΔ1 < xi) {
            yruΔ1++;
        }
        dboxDigits(ref d, yruΔ1, -k0Δ1);
        return;
    }

    // κ = 2 for float64 (section 5.1.3)
    const nint κ = 2;
    UntypedInt p10κ = 100; // 10**κ
    const uint64 p10κ1 = /* p10κ * 10 */ 1000; // 10**(κ+1)

    // Algorithm 5.2 (page 15).
    nint k0 = -mulLog10_2(exp);
    var (φ, β) = dboxPow64(κ + k0, exp);
    var (zi, exact) = dboxMulPow64(((uint64)(mant * 2 + 1)).Lsh((int64)(β)), φ);
    var (s, r) = (zi / p10κ1, (uint32)(zi % p10κ1));
    var δi = dboxDelta64(φ, β);

    if (r < δi){
        if (r != 0 || !exact || mant % 2 == 0) {
            var (sΔ1, zeros) = trimZeros(s);
            dboxDigits(ref d, sΔ1, -k0 + 1 + zeros);
            return;
        }
        s--;
        r = p10κ * 10;
    } else 
    if (r == δi) {
        var (parity, exactΔ1) = dboxParity64((uint64)(mant * 2 - 1), φ, β);
        if (parity || (exactΔ1 && mant % 2 == 0)) {
            var (sΔ2, zeros) = trimZeros(s);
            dboxDigits(ref d, sΔ2, -k0 + 1 + zeros);
            return;
        }
    }

    // Algorithm 5.4 (page 18).
    var D = r + (uint32)(p10κ / 2) - δi / 2;
    var (t, ρ) = (D / (uint32)p10κ, D % (uint32)p10κ);
    var yru = 10 * s + (uint64)t;
    if (ρ == 0) {
        var (parity, exactΔ2) = dboxParity64(mant * 2, φ, β);
        if (parity != ((D - (uint32)(p10κ / 2)) % 2 != 0) || exactΔ2 && yru % 2 != 0) {
            yru--;
        }
    }
    dboxDigits(ref d, yru, -k0);
}

// Almost identical to dragonboxFtoa64.
// This is kept as a separate copy to minimize runtime overhead.
internal static void dboxFtoa32(ref decimalSlice d, uint32 mant, nint exp, bool denorm) {
    if (mant == (uint32)((uint32)1 << (int)(float32MantBits)) && !denorm) {
        // Algorithm 5.6 (page 24).
        nint k0Δ1 = -mulLog10_2MinusLog10_4Over3(exp);
        var (φΔ1, βΔ1) = dboxPow32(k0Δ1, exp);
        var (xi, ziΔ1) = dboxRange32(φΔ1, βΔ1);
        if (exp != 2 && exp != 3) {
            xi++;
        }
        var q = ziΔ1 / 10;
        if (xi <= q * 10) {
            var (qΔ1, zeros) = trimZeros((uint64)q);
            dboxDigits(ref d, qΔ1, -k0Δ1 + 1 + zeros);
            return;
        }
        var yruΔ1 = dboxRoundUp32(φΔ1, βΔ1);
        if (exp == -77 && yruΔ1 % 2 != 0){
            yruΔ1--;
        } else 
        if (yruΔ1 < xi) {
            yruΔ1++;
        }
        dboxDigits(ref d, (uint64)yruΔ1, -k0Δ1);
        return;
    }

    // κ = 1 for float32 (section 5.1.3)
    const nint κ = 1;
    UntypedInt p10κ = 10;
    const uint32 p10κ1 = /* p10κ * 10 */ 100;

    // Algorithm 5.2 (page 15).
    nint k0 = -mulLog10_2(exp);
    var (φ, β) = dboxPow32(κ + k0, exp);
    var (zi, exact) = dboxMulPow32(((uint32)(mant * 2 + 1)).Lsh((int64)(β)), φ);
    var (s, r) = (zi / p10κ1, (uint32)(zi % p10κ1));
    var δi = dboxDelta32(φ, β);

    if (r < δi){
        if (r != 0 || !exact || mant % 2 == 0) {
            var (sΔ1, zeros) = trimZeros((uint64)s);
            dboxDigits(ref d, sΔ1, -k0 + 1 + zeros);
            return;
        }
        s--;
        r = p10κ * 10;
    } else 
    if (r == δi) {
        var (parity, exactΔ1) = dboxParity32((uint32)(mant * 2 - 1), φ, β);
        if (parity || (exactΔ1 && mant % 2 == 0)) {
            var (sΔ2, zeros) = trimZeros((uint64)s);
            dboxDigits(ref d, sΔ2, -k0 + 1 + zeros);
            return;
        }
    }

    // Algorithm 5.4 (page 18).
    var D = r + (uint32)(p10κ / 2) - δi / 2;
    var (t, ρ) = (D / (uint32)p10κ, D % (uint32)p10κ);
    var yru = 10 * s + (uint32)t;
    if (ρ == 0) {
        var (parity, exactΔ2) = dboxParity32(mant * 2, φ, β);
        if (parity != ((D - (uint32)(p10κ / 2)) % 2 != 0) || exactΔ2 && yru % 2 != 0) {
            yru--;
        }
    }
    dboxDigits(ref d, (uint64)yru, -k0);
}

// dboxDigits emits decimal digits of mant in d for float64
// and adjusts the decimal point based on exp.
internal static void dboxDigits(ref decimalSlice d, uint64 mant, nint exp) {
    nint i = formatBase10(d.d, mant);
    d.d = d.d.slice(i);
    d.nd = len(d.d);
    d.dp = d.nd + exp;
}

// uadd128 returns the full 128 bits of u + n.
internal static uint128 uadd128(uint128 u, uint64 n) {
    var sum = (uint64)(u.Lo + n);
    // Check if lo is wrapped around.
    if (sum < u.Lo) {
        u.Hi++;
    }
    u.Lo = sum;
    return u;
}

// umul64 returns the full 64 bits of x * y.
internal static uint64 umul64(uint32 x, uint32 y) {
    return (uint64)x * (uint64)y;
}

// umul96Upper64 returns the upper 64 bits (out of 96 bits) of x * y.
internal static uint64 umul96Upper64(uint32 x, uint64 y) {
    var yh = (uint32)(y >> 32);
    var yl = (uint32)y;

    var xyh = umul64(x, yh);
    var xyl = umul64(x, yl);

    return xyh + (xyl >> 32);
}

// umul96Lower64 returns the lower 64 bits (out of 96 bits) of x * y.
internal static uint64 umul96Lower64(uint32 x, uint64 y) {
    return (uint64)((uint64)x * y);
}

// umul128Upper64 returns the upper 64 bits (out of 128 bits) of x * y.
internal static uint64 umul128Upper64(uint64 x, uint64 y) {
    var a = (uint32)(x >> 32);
    var b = (uint32)x;
    var c = (uint32)(y >> 32);
    var d = (uint32)y;

    var ac = umul64(a, c);
    var bc = umul64(b, c);
    var ad = umul64(a, d);
    var bd = umul64(b, d);

    var intermediate = (bd >> 32) + (uint64)(uint32)ad + (uint64)(uint32)bc;

    return ac + (intermediate >> 32) + (ad >> 32) + (bc >> 32);
}

// umul192Upper128 returns the upper 128 bits (out of 192 bits) of x * y.
internal static uint128 umul192Upper128(uint64 x, uint128 y) {
    var r = umul128(x, y.Hi);
    var t = umul128Upper64(x, y.Lo);
    return uadd128(r, t);
}

// umul192Lower128 returns the lower 128 bits (out of 192 bits) of x * y.
internal static uint128 umul192Lower128(uint64 x, uint128 y) {
    var high = x * y.Hi;
    var highLow = umul128(x, y.Lo);
    return new uint128((uint64)(high + highLow.Hi), highLow.Lo);
}

// dboxMulPow64 computes x^(i), y^(i), z^(i)
// from the precomputed value of φ̃k for float64
// and also checks if x^(f), y^(f), z^(f) == 0 (section 5.2.1).
internal static (uint64 intPart, bool isInt) dboxMulPow64(uint64 u, uint128 phi) {
    uint64 intPart = default!;
    bool isInt = default!;

    var r = umul192Upper128(u, phi);
    intPart = r.Hi;
    isInt = r.Lo == 0;
    return (intPart, isInt);
}

// dboxMulPow32 computes x^(i), y^(i), z^(i)
// from the precomputed value of φ̃k for float32
// and also checks if x^(f), y^(f), z^(f) == 0 (section 5.2.1).
internal static (uint32 intPart, bool isInt) dboxMulPow32(uint32 u, uint64 phi) {
    uint32 intPart = default!;
    bool isInt = default!;

    var r = umul96Upper64(u, phi);
    intPart = (uint32)(r >> 32);
    isInt = (uint32)r == 0;
    return (intPart, isInt);
}

// dboxParity64 computes only the parity of x^(i), y^(i), z^(i)
// from the precomputed value of φ̃k for float64
// and also checks if x^(f), y^(f), z^(f) = 0 (section 5.2.1).
internal static (bool parity, bool isInt) dboxParity64(uint64 mant2, uint128 phi, nint beta) {
    bool parity = default!;
    bool isInt = default!;

    var r = umul192Lower128(mant2, phi);
    parity = ((r.Hi.Rsh((int64)(64 - beta))) & 1) != 0;
    isInt = (((uint64)(r.Hi.Lsh((int64)(beta)))) | (r.Lo.Rsh((int64)(64 - beta)))) == 0;
    return (parity, isInt);
}

// dboxParity32 computes only the parity of x^(i), y^(i), z^(i)
// from the precomputed value of φ̃k for float32
// and also checks if x^(f), y^(f), z^(f) = 0 (section 5.2.1).
internal static (bool parity, bool isInt) dboxParity32(uint32 mant2, uint64 phi, nint beta) {
    bool parity = default!;
    bool isInt = default!;

    var r = umul96Lower64(mant2, phi);
    parity = ((r.Rsh((int64)(64 - beta))) & 1) != 0;
    isInt = (uint32)(r.Rsh((int64)(32 - beta))) == 0;
    return (parity, isInt);
}

// dboxDelta64 returns δ^(i) from the precomputed value of φ̃k for float64.
internal static uint32 dboxDelta64(uint128 φ, nint β) {
    return (uint32)(φ.Hi.Rsh((int64)(64 - 1 - β)));
}

// dboxDelta32 returns δ^(i) from the precomputed value of φ̃k for float32.
internal static uint32 dboxDelta32(uint64 φ, nint β) {
    return (uint32)(φ.Rsh((int64)(64 - 1 - β)));
}

// mulLog10_2MinusLog10_4Over3 computes
// ⌊e*log10(2)-log10(4/3)⌋ = ⌊log10(2^e)-log10(4/3)⌋ (section 6.3).
internal static nint mulLog10_2MinusLog10_4Over3(nint e) {
    // e should be in the range [-2985, 2936].
    return (e * 631305 - 261663) >> 21;
}

internal static UntypedInt floatMantBits64 => 52; // p = 52 for float64.
internal static UntypedInt floatMantBits32 => 23; // p = 23 for float32.

// dboxRange64 returns the left and right float64 endpoints.
internal static (uint64 left, uint64 right) dboxRange64(uint128 φ, nint β) {
    uint64 left = default!;
    uint64 right = default!;

    left = (φ.Hi - (φ.Hi >> (int)(float64MantBits + 2))).Rsh((int64)((nint)(64 - float64MantBits - 1) - β));
    right = (φ.Hi + (φ.Hi >> (int)(float64MantBits + 1))).Rsh((int64)((nint)(64 - float64MantBits - 1) - β));
    return (left, right);
}

// dboxRange32 returns the left and right float32 endpoints.
internal static (uint32 left, uint32 right) dboxRange32(uint64 φ, nint β) {
    uint32 left = default!;
    uint32 right = default!;

    left = (uint32)((φ - (φ >> (int)(floatMantBits32 + 2))).Rsh((int64)((nint)(64 - floatMantBits32 - 1) - β)));
    right = (uint32)((φ + (φ >> (int)(floatMantBits32 + 1))).Rsh((int64)((nint)(64 - floatMantBits32 - 1) - β)));
    return (left, right);
}

// dboxRoundUp64 computes the round up of y (i.e., y^(ru)).
internal static uint64 dboxRoundUp64(uint128 phi, nint beta) {
    return (phi.Hi.Rsh((int64)((nint)(128 / 2 - floatMantBits64 - 2) - beta)) + 1) / 2;
}

// dboxRoundUp32 computes the round up of y (i.e., y^(ru)).
internal static uint32 dboxRoundUp32(uint64 phi, nint beta) {
    return (uint32)(phi.Rsh((int64)((nint)(64 - floatMantBits32 - 2) - beta)) + 1) / 2;
}

// dboxPow64 gets the precomputed value of φ̃̃k for float64.
internal static (uint128 φ, nint β) dboxPow64(nint k, nint e) {
    uint128 φ = default!;
    nint β = default!;

    (φ, var e1, _) = pow10(k);
    if (k < 0 || k > 55) {
        φ.Lo++;
    }
    β = e + e1 - 1;
    return (φ, β);
}

// dboxPow32 gets the precomputed value of φ̃̃k for float32.
internal static (uint64 mant, nint exp) dboxPow32(nint k, nint e) {
    nint exp = default!;

    var (m, e1, _) = pow10(k);
    if (k < 0 || k > 27) {
        m.Hi++;
    }
    exp = e + e1 - 1;
    return (m.Hi, exp);
}

} // end strconv_package
