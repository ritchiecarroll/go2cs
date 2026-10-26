// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

internal static bool isOddInt(float64 x) {
    if (Abs(x) >= (9007199254740992D)) {
        // 1 << 53 is the largest exact integer in the float64 format.
        // Any number outside this range will be truncated before the decimal point and therefore will always be
        // an even integer.
        // Without this check and if x overflows int64 the int64(xi) conversion below may produce incorrect results
        // on some architectures (and does so on arm64). See issue #57465.
        return false;
    }

    var (xi, xf) = Modf(x);
    return xf == 0D && ((int64)xi & 1) == 1;
}

// Special cases taken from FreeBSD's /usr/src/lib/msun/src/e_pow.c
// updated by IEEE Std. 754-2008 "Section 9.2.1 Special values".

// Pow returns x**y, the base-x exponential of y.
//
// Special cases are (in order):
//
//	Pow(x, ±0) = 1 for any x
//	Pow(1, y) = 1 for any y
//	Pow(x, 1) = x for any x
//	Pow(NaN, y) = NaN
//	Pow(x, NaN) = NaN
//	Pow(±0, y) = ±Inf for y an odd integer < 0
//	Pow(±0, -Inf) = +Inf
//	Pow(±0, +Inf) = +0
//	Pow(±0, y) = +Inf for finite y < 0 and not an odd integer
//	Pow(±0, y) = ±0 for y an odd integer > 0
//	Pow(±0, y) = +0 for finite y > 0 and not an odd integer
//	Pow(-1, ±Inf) = 1
//	Pow(x, +Inf) = +Inf for |x| > 1
//	Pow(x, -Inf) = +0 for |x| > 1
//	Pow(x, +Inf) = +0 for |x| < 1
//	Pow(x, -Inf) = +Inf for |x| < 1
//	Pow(+Inf, y) = +Inf for y > 0
//	Pow(+Inf, y) = +0 for y < 0
//	Pow(-Inf, y) = Pow(-0, -y)
//	Pow(x, y) = NaN for finite x < 0 and finite non-integer y
public static float64 Pow(float64 x, float64 y) {
    if (haveArchPow) {
        return archPow(x, y);
    }
    return pow(x, y);
}

internal static float64 pow(float64 x, float64 y) {
    switch (ᐧ) {
    case {} when y == 0D || x == 1D: {
        return 1D;
    }
    case {} when y is 1D: {
        return x;
    }
    case {} when IsNaN(x) || IsNaN(y): {
        return NaN();
    }
    case {} when x is 0D: {
        switch (ᐧ) {
        case {} when y is < 0D: {
            if (Signbit(x) && isOddInt(y)) {
                return Inf(-1);
            }
            return Inf(1);
        }
        case {} when y is > 0D: {
            if (Signbit(x) && isOddInt(y)) {
                return x;
            }
            return 0D;
        }}
        break;
    }
    case {} when IsInf(y, 0): {
        switch (ᐧ) {
        case {} when x == -1D: {
            return 1D;
        }
        case {} when (Abs(x) < 1D) == IsInf(y, 1): {
            return 0D;
        }
        default: {
            return Inf(1);
        }}
        break;
    }
    case {} when IsInf(x, 0): {
        if (IsInf(x, -1)) {
            return Pow(1D / x, -y); // Pow(-0, -y)
        }
        switch (ᐧ) {
        case {} when y is < 0D: {
            return 0D;
        }
        case {} when y is > 0D: {
            return Inf(1);
        }}
        break;
    }
    case {} when y is 0.5D: {
        return Sqrt(x);
    }
    case {} when y == -0.5D: {
        return 1D / Sqrt(x);
    }}

    var (yi, yf) = Modf(Abs(y));
    if (yf != 0D && x < 0D) {
        return NaN();
    }
    if (yi >= 9223372036854775808D) {
        // yi is a large even int that will lead to overflow (or underflow to 0)
        // for all x except -1 (x == 1 was handled earlier)
        switch (ᐧ) {
        case {} when x == -1D: {
            return 1D;
        }
        case {} when (Abs(x) < 1D) == (y > 0D): {
            return 0D;
        }
        default: {
            return Inf(1);
        }}
    }

    // ans = a1 * 2**ae (= 1 for now).
    var a1 = 1.0D;
    nint ae = 0;

    // ans *= x**yf
    if (yf != 0D) {
        if (yf > 0.5D) {
            yf--;
            yi++;
        }
        a1 = Exp(yf * Log(x));
    }

    // ans *= x**yi
    // by multiplying in successive squarings
    // of x according to bits of yi.
    // accumulate powers of two into exp.
    var (x1, xe) = Frexp(x);
    for (var i = (int64)yi; i != 0; i >>= (int)(1)) {
        if (xe < -1 << 12 || 1 << 12 < xe) {
            // catch xe before it overflows the left shift below
            // Since i !=0 it has at least one bit still set, so ae will accumulate xe
            // on at least one more iteration, ae += xe is a lower bound on ae
            // the lower bound on ae exceeds the size of a float64 exp
            // so the final call to Ldexp will produce under/overflow (0/Inf)
            ae += xe;
            break;
        }
        if ((i & 1) == 1) {
            a1 *= x1;
            ae += xe;
        }
        x1 *= x1;
        xe <<= (int)(1);
        if (x1 < .5D) {
            x1 += x1;
            xe--;
        }
    }

    // ans = a1*2**ae
    // if y < 0 { ans = 1 / ans }
    // but in the opposite order
    if (y < 0D) {
        a1 = 1D / a1;
        ae = -ae;
    }
    return Ldexp(a1, ae);
}

} // end math_package
