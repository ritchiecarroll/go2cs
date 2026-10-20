// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// The original C code, the long comment, and the constants
// below are from FreeBSD's /usr/src/lib/msun/src/s_asinh.c
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
// asinh(x)
// Method :
//	Based on
//	        asinh(x) = sign(x) * log [ |x| + sqrt(x*x+1) ]
//	we have
//	asinh(x) := x  if  1+x*x=1,
//	         := sign(x)*(log(x)+ln2) for large |x|, else
//	         := sign(x)*log(2|x|+1/(|x|+sqrt(x*x+1))) if|x|>2, else
//	         := sign(x)*log1p(|x| + x**2/(1 + sqrt(1+x**2)))
//

// Asinh returns the inverse hyperbolic sine of x.
//
// Special cases are:
//
//	Asinh(±0) = ±0
//	Asinh(±Inf) = ±Inf
//	Asinh(NaN) = NaN
public static float64 Asinh(float64 x) {
    if (haveArchAsinh) {
        return archAsinh(x);
    }
    return asinh(x);
}

internal static float64 asinh(float64 x) {
    const float64 Ln2 = 6.93147180559945286227e-01; // 0x3FE62E42FEFA39EF
    const float64 NearZero = /* 1.0 / (1 << 28) */ 3.725290298461914e-09; // 2**-28
    const float64 Large = /* 1 << 28 */ 268435456; // 2**28
    // special cases
    if (IsNaN(x) || IsInf(x, 0)) {
        return x;
    }
    var sign = false;
    if (x < 0D) {
        x = -x;
        sign = true;
    }
    float64 temp = default!;
    switch (ᐧ) {
    case {} when x > Large: {
        temp = Log(x) + Ln2; // |x| > 2**28
        break;
    }
    case {} when x is > 2D: {
        temp = Log(2D * x + 1D / (Sqrt(x * x + 1D) + x)); // 2**28 > |x| > 2.0
        break;
    }
    case {} when x < NearZero: {
        temp = x; // |x| < 2**-28
        break;
    }
    default: {
        temp = Log1p(x + x * x / (1D + Sqrt(1D + x * x))); // 2.0 > |x| > 2**-28
        break;
    }}
    if (sign) {
        temp = -temp;
    }
    return temp;
}

} // end math_package
