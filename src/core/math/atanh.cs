// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// The original C code, the long comment, and the constants
// below are from FreeBSD's /usr/src/lib/msun/src/e_atanh.c
// and came with this notice. The go code is a simplified
// version of the original C.
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
//
// __ieee754_atanh(x)
// Method :
//	1. Reduce x to positive by atanh(-x) = -atanh(x)
//	2. For x>=0.5
//	            1              2x                          x
//	atanh(x) = --- * log(1 + -------) = 0.5 * log1p(2 * --------)
//	            2             1 - x                      1 - x
//
//	For x<0.5
//	atanh(x) = 0.5*log1p(2x+2x*x/(1-x))
//
// Special cases:
//	atanh(x) is NaN if |x| > 1 with signal;
//	atanh(NaN) is that NaN with no signal;
//	atanh(+-1) is +-INF with signal.
//

// Atanh returns the inverse hyperbolic tangent of x.
//
// Special cases are:
//
//	Atanh(1) = +Inf
//	Atanh(±0) = ±0
//	Atanh(-1) = -Inf
//	Atanh(x) = NaN if x < -1 or x > 1
//	Atanh(NaN) = NaN
public static float64 Atanh(float64 x) {
    if (haveArchAtanh) {
        return archAtanh(x);
    }
    return atanh(x);
}

internal static float64 atanh(float64 x) {
    const float64 NearZero = /* 1.0 / (1 << 28) */ 3.725290298461914e-09; // 2**-28
    // special cases
    switch (ᐧ) {
    case {} when x < -1D || x > 1D || IsNaN(x): {
        return NaN();
    }
    case {} when x is 1D: {
        return Inf(1);
    }
    case {} when x == -1D: {
        return Inf(-1);
    }}
    var sign = false;
    if (x < 0D) {
        x = -x;
        sign = true;
    }
    float64 temp = default;
    switch (ᐧ) {
    case {} when x < NearZero: {
        temp = x;
        break;
    }
    case {} when x is < 0.5D: {
        temp = x + x;
        temp = 0.5D * Log1p(temp + temp * x / (1D - x));
        break;
    }
    default: {
        temp = 0.5D * Log1p((x + x) / (1D - x));
        break;
    }}
    if (sign) {
        temp = -temp;
    }
    return temp;
}

} // end math_package
