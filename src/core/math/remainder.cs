// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// The original C code and the comment below are from
// FreeBSD's /usr/src/lib/msun/src/e_remainder.c and came
// with this notice. The go code is a simplified version of
// the original C.
//
// ====================================================
// Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
//
// Developed at SunPro, a Sun Microsystems, Inc. business.
// Permission to use, copy, modify, and distribute this
// software is freely granted, provided that this notice
// is preserved.
// ====================================================
//
// __ieee754_remainder(x,y)
// Return :
//      returns  x REM y  =  x - [x/y]*y  as if in infinite
//      precision arithmetic, where [x/y] is the (infinite bit)
//      integer nearest x/y (in half way cases, choose the even one).
// Method :
//      Based on Mod() returning  x - [x/y]chopped * y  exactly.

// Remainder returns the IEEE 754 floating-point remainder of x/y.
//
// Special cases are:
//
//	Remainder(±Inf, y) = NaN
//	Remainder(NaN, y) = NaN
//	Remainder(x, 0) = NaN
//	Remainder(x, ±Inf) = x
//	Remainder(x, NaN) = NaN
public static float64 Remainder(float64 x, float64 y) {
    if (haveArchRemainder) {
        return archRemainder(x, y);
    }
    return remainder(x, y);
}

internal static float64 remainder(float64 x, float64 y) {
    const float64 Tiny = 4.45014771701440276618e-308; // 0x0020000000000000
    const float64 HalfMax = /* MaxFloat64 / 2 */ 8.988465674311579e+307;
    // special cases
    switch (ᐧ) {
    case {} when IsNaN(x) || IsNaN(y) || IsInf(x, 0) || y == 0D: {
        return NaN();
    }
    case {} when IsInf(y, 0): {
        return x;
    }}
    var sign = false;
    if (x < 0D) {
        x = -x;
        sign = true;
    }
    if (y < 0D) {
        y = -y;
    }
    if (x == y) {
        if (sign) {
            var zero = 0.0D;
            return -zero;
        }
        return 0D;
    }
    if (y <= HalfMax) {
        x = Mod(x, y + y); // now x < 2y
    }
    if (y < Tiny){
        if (x + x > y) {
            x -= y;
            if (x + x >= y) {
                x -= y;
            }
        }
    } else {
        var yHalf = 0.5D * y;
        if (x > yHalf) {
            x -= y;
            if (x >= yHalf) {
                x -= y;
            }
        }
    }
    if (sign) {
        x = -x;
    }
    return x;
}

} // end math_package
