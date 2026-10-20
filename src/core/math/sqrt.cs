// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// The original C code and the long comment below are
// from FreeBSD's /usr/src/lib/msun/src/e_sqrt.c and
// came with this notice. The go code is a simplified
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
// __ieee754_sqrt(x)
// Return correctly rounded sqrt.
//           -----------------------------------------
//           | Use the hardware sqrt if you have one |
//           -----------------------------------------
// Method:
//   Bit by bit method using integer arithmetic. (Slow, but portable)
//   1. Normalization
//      Scale x to y in [1,4) with even powers of 2:
//      find an integer k such that  1 <= (y=x*2**(2k)) < 4, then
//              sqrt(x) = 2**k * sqrt(y)
//   2. Bit by bit computation
//      Let q  = sqrt(y) truncated to i bit after binary point (q = 1),
//           i                                                   0
//                                     i+1         2
//          s  = 2*q , and      y  =  2   * ( y - q  ).          (1)
//           i      i            i                 i
//
//      To compute q    from q , one checks whether
//                  i+1       i
//
//                            -(i+1) 2
//                      (q + 2      )  <= y.                     (2)
//                        i
//                                                            -(i+1)
//      If (2) is false, then q   = q ; otherwise q   = q  + 2      .
//                             i+1   i             i+1   i
//
//      With some algebraic manipulation, it is not difficult to see
//      that (2) is equivalent to
//                             -(i+1)
//                      s  +  2       <= y                       (3)
//                       i                i
//
//      The advantage of (3) is that s  and y  can be computed by
//                                    i      i
//      the following recurrence formula:
//          if (3) is false
//
//          s     =  s  ,       y    = y   ;                     (4)
//           i+1      i          i+1    i
//
//      otherwise,
//                         -i                      -(i+1)
//          s     =  s  + 2  ,  y    = y  -  s  - 2              (5)
//           i+1      i          i+1    i     i
//
//      One may easily use induction to prove (4) and (5).
//      Note. Since the left hand side of (3) contain only i+2 bits,
//            it is not necessary to do a full (53-bit) comparison
//            in (3).
//   3. Final rounding
//      After generating the 53 bits result, we compute one more bit.
//      Together with the remainder, we can decide whether the
//      result is exact, bigger than 1/2ulp, or less than 1/2ulp
//      (it will never equal to 1/2ulp).
//      The rounding mode can be detected by checking whether
//      huge + tiny is equal to huge, and whether huge - tiny is
//      equal to huge for some floating point number "huge" and "tiny".
//
//
// Notes:  Rounding mode detection omitted. The constants "mask", "shift",
// and "bias" are found in src/math/bits.go

// Sqrt returns the square root of x.
//
// Special cases are:
//
//	Sqrt(+Inf) = +Inf
//	Sqrt(±0) = ±0
//	Sqrt(x < 0) = NaN
//	Sqrt(NaN) = NaN
public static float64 Sqrt(float64 x) {
    return sqrt(x);
}

// Note: On systems where Sqrt is a single instruction, the compiler
// may turn a direct call into a direct use of that instruction instead.
internal static float64 sqrt(float64 x) {
    // special cases
    switch (ᐧ) {
    case {} when x == 0D || IsNaN(x) || IsInf(x, 1): {
        return x;
    }
    case {} when x is < 0D: {
        return NaN();
    }}
    var ix = Float64bits(x);
    // normalize x
    nint exp = (nint)((ix >> (int)(shift)) & (uint64)mask);
    if (exp == 0) { // subnormal x
        while ((uint64)(ix & ((uint64)1 << (int)(shift))) == 0) {
            ix <<= (int)(1);
            exp--;
        }
        exp++;
    }
    exp -= bias; // unbias exponent
    ix &= unchecked((uint64)~(uint64)(((uint64)mask << (int)(shift))));
    ix |= (uint64)(((uint64)1 << (int)(shift)));
    if ((exp & 1) == 1) { // odd exp, double x to make it even
        ix <<= (int)(1);
    }
    exp >>= (int)(1); // exp = exp/2, exponent of square root
    // generate sqrt(x) bit by bit
    ix <<= (int)(1);
    uint64 q = default!;                              // q = sqrt(x)
    uint64 s = default!;
    var r = (uint64)((uint64)1 << (int)(shift + 1)); // r = moving bit from MSB to LSB
    while (r != 0) {
        var t = s + r;
        if (t <= ix) {
            s = t + r;
            ix -= t;
            q += r;
        }
        ix <<= (int)(1);
        r >>= (int)(1);
    }
    // final rounding
    if (ix != 0) { // remainder, result not exact
        q += (q & 1); // round according to extra bit
    }
    ix = (q >> 1) + ((uint64)(exp - 1 + (nint)bias) << (int)(shift)); // significand + biased exponent
    return Float64frombits(ix);
}

} // end math_package
