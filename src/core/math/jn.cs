// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst Two302ᶜ = GoBigConst.Parse("8148143905337944345073782753637512644205873574663745002544561797417525199053346824733589504");

/*
	Bessel function of the first and second kinds of order n.
*/
// The original C code and the long comment below are
// from FreeBSD's /usr/src/lib/msun/src/e_jn.c and
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
// __ieee754_jn(n, x), __ieee754_yn(n, x)
// floating point Bessel's function of the 1st and 2nd kind
// of order n
//
// Special cases:
//      y0(0)=y1(0)=yn(n,0) = -inf with division by zero signal;
//      y0(-ve)=y1(-ve)=yn(n,-ve) are NaN with invalid signal.
// Note 2. About jn(n,x), yn(n,x)
//      For n=0, j0(x) is called,
//      for n=1, j1(x) is called,
//      for n<x, forward recursion is used starting
//      from values of j0(x) and j1(x).
//      for n>x, a continued fraction approximation to
//      j(n,x)/j(n-1,x) is evaluated and then backward
//      recursion is used starting from a supposed value
//      for j(n,x). The resulting value of j(0,x) is
//      compared with the actual value to correct the
//      supposed value of j(n,x).
//
//      yn(n,x) is similar in all respects, except
//      that forward recursion is used for all
//      values of n>1.

// Jn returns the order-n Bessel function of the first kind.
//
// Special cases are:
//
//	Jn(n, ±Inf) = 0
//	Jn(n, NaN) = NaN
public static float64 Jn(nint n, float64 x) {
    const float64 TwoM29 = /* 1.0 / (1 << 29) */ 1.862645149230957e-09; // 2**-29 0x3e10000000000000
    GoBigConst Two302 = /* 1 << 302 */                                  // 2**302 0x52D0000000000000
            Two302ᶜ;
    // special cases
    switch (ᐧ) {
    case {} when IsNaN(x): {
        return x;
    }
    case {} when IsInf(x, 0): {
        return 0D;
    }}
    // J(-n, x) = (-1)**n * J(n, x), J(n, -x) = (-1)**n * J(n, x)
    // Thus, J(-n, x) = J(n, -x)

    if (n == 0) {
        return J0(x);
    }
    if (x == 0D) {
        return 0D;
    }
    if (n < 0) {
        (n, x) = (-n, -x);
    }
    if (n == 1) {
        return J1(x);
    }
    var sign = false;
    if (x < 0D) {
        x = -x;
        if ((n & 1) == 1) {
            sign = true; // odd n and negative x
        }
    }
    float64 b = default;
    if ((float64)n <= x) {
        // Safe to use J(n+1,x)=2n/x *J(n,x)-J(n-1,x)
        if (x >= (float64)Two302) { // x > 2**302
            // (x >> n**2)
            //          Jn(x) = cos(x-(2n+1)*pi/4)*sqrt(2/x*pi)
            //          Yn(x) = sin(x-(2n+1)*pi/4)*sqrt(2/x*pi)
            //          Let s=sin(x), c=cos(x),
            //              xn=x-(2n+1)*pi/4, sqt2 = sqrt(2),then
            //
            //                 n    sin(xn)*sqt2    cos(xn)*sqt2
            //              ----------------------------------
            //                 0     s-c             c+s
            //                 1    -s-c            -c+s
            //                 2    -s+c            -c-s
            //                 3     s+c             c-s

            float64 temp = default;
            {
                var (s, c) = Sincos(x);
                switch ((n & 3)) {
                case 0: {
                    temp = c + s;
                    break;
                }
                case 1: {
                    temp = -c + s;
                    break;
                }
                case 2: {
                    temp = -c - s;
                    break;
                }
                case 3: {
                    temp = c - s;
                    break;
                }}
            }
            b = /* (1 / SqrtPi) */ 0.5641895835477563D * temp / Sqrt(x);
        } else {
            b = J1(x);
            for ((nint i, var a) = (1, J0(x)); i < n; i++) {
                (a, b) = (b, b * ((float64)(i + i) / x) - a); // avoid underflow
            }
        }
    } else {
        if (x < TwoM29) { // x < 2**-29
            // x is tiny, return the first Taylor expansion of J(n,x)
            // J(n,x) = 1/n!*(x/2)**n  - ...

            if (n > 33) { // underflow
                b = 0D;
            } else {
                var temp = x * 0.5D;
                b = temp;
                var a = 1.0D;
                for (nint i = 2; i <= n; i++) {
                    a *= (float64)i; // a = n!
                    b *= temp;       // b = (x/2)**n
                }
                b /= a;
            }
        } else {
            // use backward recurrence
            //                      x      x**2      x**2
            //  J(n,x)/J(n-1,x) =  ----   ------   ------   .....
            //                      2n  - 2(n+1) - 2(n+2)
            //
            //                      1      1        1
            //  (for large x)   =  ----  ------   ------   .....
            //                      2n   2(n+1)   2(n+2)
            //                      -- - ------ - ------ -
            //                       x     x         x
            //
            // Let w = 2n/x and h=2/x, then the above quotient
            // is equal to the continued fraction:
            //                  1
            //      = -----------------------
            //                     1
            //         w - -----------------
            //                        1
            //              w+h - ---------
            //                     w+2h - ...
            //
            // To determine how many terms needed, let
            // Q(0) = w, Q(1) = w(w+h) - 1,
            // Q(k) = (w+k*h)*Q(k-1) - Q(k-2),
            // When Q(k) > 1e4	good for single
            // When Q(k) > 1e9	good for double
            // When Q(k) > 1e17	good for quadruple

            // determine k
            var w = (float64)(n + n) / x;
            var h = 2D / x;
            var q0 = w;
            var z = w + h;
            var q1 = w * z - 1D;
            nint k = 1;
            while (q1 < 1e9D) {
                k++;
                z += h;
                (q0, q1) = (q1, z * q1 - q0);
            }
            nint m = n + n;
            var t = 0.0D;
            for (nint i = 2 * (n + k); i >= m; i -= 2) {
                t = 1D / ((float64)i / x - t);
            }
            var a = t;
            b = 1D;
            //  estimate log((2/x)**n*n!) = n*log(2/x)+n*ln(n)
            //  Hence, if n*(log(2n/x)) > ...
            //  single 8.8722839355e+01
            //  double 7.09782712893383973096e+02
            //  long double 1.1356523406294143949491931077970765006170e+04
            //  then recurrent value may overflow and the result is
            //  likely underflow to zero

            var tmp = (float64)n;
            var v = 2D / x;
            tmp = tmp * Log(Abs(v * tmp));
            if (tmp < 7.09782712893383973096e+02D) {
                for (nint i = n - 1; i > 0; i--) {
                    var di = (float64)(i + i);
                    (a, b) = (b, b * di / x - a);
                }
            } else {
                for (nint i = n - 1; i > 0; i--) {
                    var di = (float64)(i + i);
                    (a, b) = (b, b * di / x - a);
                    // scale b to avoid spurious overflow
                    if (b > 1e100D) {
                        a /= b;
                        t /= b;
                        b = 1D;
                    }
                }
            }
            b = t * J0(x) / b;
        }
    }
    if (sign) {
        return -b;
    }
    return b;
}

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst Two302ᶜ1 = GoBigConst.Parse("8148143905337944345073782753637512644205873574663745002544561797417525199053346824733589504");

// Yn returns the order-n Bessel function of the second kind.
//
// Special cases are:
//
//	Yn(n, +Inf) = 0
//	Yn(n ≥ 0, 0) = -Inf
//	Yn(n < 0, 0) = +Inf if n is odd, -Inf if n is even
//	Yn(n, x < 0) = NaN
//	Yn(n, NaN) = NaN
public static float64 Yn(nint n, float64 x) {
    GoBigConst Two302 = /* 1 << 302 */ // 2**302 0x52D0000000000000
            Two302ᶜ1;
    // special cases
    switch (ᐧ) {
    case {} when x < 0D || IsNaN(x): {
        return NaN();
    }
    case {} when IsInf(x, 1): {
        return 0D;
    }}

    if (n == 0) {
        return Y0(x);
    }
    if (x == 0D) {
        if (n < 0 && (n & 1) == 1) {
            return Inf(1);
        }
        return Inf(-1);
    }
    var sign = false;
    if (n < 0) {
        n = -n;
        if ((n & 1) == 1) {
            sign = true; // sign true if n < 0 && |n| odd
        }
    }
    if (n == 1) {
        if (sign) {
            return -Y1(x);
        }
        return Y1(x);
    }
    float64 b = default;
    if (x >= (float64)Two302) { // x > 2**302
        // (x >> n**2)
        //	    Jn(x) = cos(x-(2n+1)*pi/4)*sqrt(2/x*pi)
        //	    Yn(x) = sin(x-(2n+1)*pi/4)*sqrt(2/x*pi)
        //	    Let s=sin(x), c=cos(x),
        //		xn=x-(2n+1)*pi/4, sqt2 = sqrt(2),then
        //
        //		   n	sin(xn)*sqt2	cos(xn)*sqt2
        //		----------------------------------
        //		   0	 s-c		 c+s
        //		   1	-s-c 		-c+s
        //		   2	-s+c		-c-s
        //		   3	 s+c		 c-s

        float64 temp = default;
        {
            var (s, c) = Sincos(x);
            switch ((n & 3)) {
            case 0: {
                temp = s - c;
                break;
            }
            case 1: {
                temp = -s - c;
                break;
            }
            case 2: {
                temp = -s + c;
                break;
            }
            case 3: {
                temp = s + c;
                break;
            }}
        }
        b = /* (1 / SqrtPi) */ 0.5641895835477563D * temp / Sqrt(x);
    } else {
        var a = Y0(x);
        b = Y1(x);
        // quit if b is -inf
        for (nint i = 1; i < n && !IsInf(b, -1); i++) {
            (a, b) = (b, ((float64)(i + i) / x) * b - a);
        }
    }
    if (sign) {
        return -b;
    }
    return b;
}

} // end math_package
