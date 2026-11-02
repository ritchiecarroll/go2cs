// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// The original C code, the long comment, and the constants
// below are from http://netlib.sandia.gov/cephes/cprob/gamma.c.
// The go code is a simplified version of the original C.
//
//      tgamma.c
//
//      Gamma function
//
// SYNOPSIS:
//
// double x, y, tgamma();
// extern int signgam;
//
// y = tgamma( x );
//
// DESCRIPTION:
//
// Returns gamma function of the argument. The result is
// correctly signed, and the sign (+1 or -1) is also
// returned in a global (extern) variable named signgam.
// This variable is also filled in by the logarithmic gamma
// function lgamma().
//
// Arguments |x| <= 34 are reduced by recurrence and the function
// approximated by a rational function of degree 6/7 in the
// interval (2,3).  Large arguments are handled by Stirling's
// formula. Large negative arguments are made positive using
// a reflection formula.
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    DEC      -34, 34      10000       1.3e-16     2.5e-17
//    IEEE    -170,-33      20000       2.3e-15     3.3e-16
//    IEEE     -33,  33     20000       9.4e-16     2.2e-16
//    IEEE      33, 171.6   20000       2.3e-15     3.2e-16
//
// Error for arguments outside the test range will be larger
// owing to error amplification by the exponential function.
//
// Cephes Math Library Release 2.8:  June, 2000
// Copyright 1984, 1987, 1989, 1992, 2000 by Stephen L. Moshier
//
// The readme file at http://netlib.sandia.gov/cephes/ says:
//    Some software in this archive may be from the book _Methods and
// Programs for Mathematical Functions_ (Prentice-Hall or Simon & Schuster
// International, 1989) or from the Cephes Mathematical Library, a
// commercial product. In either event, it is copyrighted by the author.
// What you see here may be used freely but it comes with no support or
// guarantee.
//
//   The two known misprints in the book are repaired here in the
// source listings for the gamma function and the incomplete beta
// integral.
//
//   Stephen L. Moshier
//   moshier@na-net.ornl.gov
internal static array<float64> _gamP = new float64[]{
    1.60119522476751861407e-04D,
    1.19135147006586384913e-03D,
    1.04213797561761569935e-02D,
    4.76367800457137231464e-02D,
    2.07448227648435975150e-01D,
    4.94214826801497100753e-01D,
    9.99999999999999996796e-01D
}.array();

internal static array<float64> _gamQ = new float64[]{
    -2.31581873324120129819e-05D,
    5.39605580493303397842e-04D,
    -4.45641913851797240494e-03D,
    1.18139785222060435552e-02D,
    3.58236398605498653373e-02D,
    -2.34591795718243348568e-01D,
    7.14304917030273074085e-02D,
    1.00000000000000000320e+00D
}.array();

internal static array<float64> _gamS = new float64[]{
    7.87311395793093628397e-04D,
    -2.29549961613378126380e-04D,
    -2.68132617805781232825e-03D,
    3.47222221605458667310e-03D,
    8.33333333333482257126e-02D
}.array();

// Gamma function computed by Stirling's formula.
// The pair of results must be multiplied together to get the actual answer.
// The multiplication is left to the caller so that, if careful, the caller can avoid
// infinity for 172 <= x <= 180.
// The polynomial is valid for 33 <= x <= 172; larger values are only used
// in reciprocal and produce denormalized floats. The lower precision there
// masks any imprecision in the polynomial.
internal static (float64, float64) stirling(float64 x) {
    if (x > 200D) {
        return (Inf(1), 1D);
    }
    const float64 SqrtTwoPi = 2.506628274631000502417;
    const float64 MaxStirling = 143.01608;
    var w = 1D / x;
    w = 1D + w * ((((_gamS[0] * w + _gamS[1]) * w + _gamS[2]) * w + _gamS[3]) * w + _gamS[4]);
    var y1 = Exp(x);
    var y2 = 1.0D;
    if (x > MaxStirling) { // avoid Pow() overflow
        var v = Pow(x, 0.5D * x - 0.25D);
        (y1, y2) = (v, v / y1);
    } else {
        y1 = Pow(x, x - 0.5D) / y1;
    }
    return (y1, SqrtTwoPi * w * y2);
}

// Gamma returns the Gamma function of x.
//
// Special cases are:
//
//	Gamma(+Inf) = +Inf
//	Gamma(+0) = +Inf
//	Gamma(-0) = -Inf
//	Gamma(x) = NaN for integer x < 0
//	Gamma(-Inf) = NaN
//	Gamma(NaN) = NaN
public static float64 Gamma(float64 x) {
    const float64 Euler = 0.57721566490153286060651209008240243104215933593992; // A001620
    // special cases
    switch (ᐧ) {
    case {} when isNegInt(x) || IsInf(x, -1) || IsNaN(x): {
        return NaN();
    }
    case {} when IsInf(x, 1): {
        return Inf(1);
    }
    case {} when x is 0D: {
        if (Signbit(x)) {
            return Inf(-1);
        }
        return Inf(1);
    }}
    var q = Abs(x);
    var p = Floor(q);
    if (q > 33D) {
        if (x >= 0D) {
            var (y1, y2) = stirling(x);
            return y1 * y2;
        }
        // Note: x is negative but (checked above) not a negative integer,
        // so x must be small enough to be in range for conversion to int64.
        // If |x| were >= 2⁶³ it would have to be an integer.
        nint signgam = 1;
        {
            var ip = (int64)p; if ((ip & 1) == 0) {
                signgam = -1;
            }
        }
        var zΔ1 = q - p;
        if (zΔ1 > 0.5D) {
            p = p + 1D;
            zΔ1 = q - p;
        }
        zΔ1 = q * Sin((float64)Pi * zΔ1);
        if (zΔ1 == 0D) {
            return Inf(signgam);
        }
        var (sq1, sq2) = stirling(q);
        var absz = Abs(zΔ1);
        var d = absz * sq1 * sq2;
        if (IsInf(d, 0)) {
            zΔ1 = (float64)Pi / absz / sq1 / sq2;
        } else {
            zΔ1 = (float64)Pi / d;
        }
        return (float64)signgam * zΔ1;
    }

    // Reduce argument
    var z = 1.0D;
    while (x >= 3D) {
        x = x - 1D;
        z = z * x;
    }
    while (x < 0D) {
        if (x > -1e-09D) {
            goto small;
        }
        z = z / x;
        x = x + 1D;
    }
    while (x < 2D) {
        if (x < 1e-09D) {
            goto small;
        }
        z = z / x;
        x = x + 1D;
    }

    if (x == 2D) {
        return z;
    }

    x = x - 2D;
    p = (((((x * _gamP[0] + _gamP[1]) * x + _gamP[2]) * x + _gamP[3]) * x + _gamP[4]) * x + _gamP[5]) * x + _gamP[6];
    q = ((((((x * _gamQ[0] + _gamQ[1]) * x + _gamQ[2]) * x + _gamQ[3]) * x + _gamQ[4]) * x + _gamQ[5]) * x + _gamQ[6]) * x + _gamQ[7];
    return z * p / q;

small:
    if (x == 0D) {
        return Inf(1);
    }
    return z / ((1D + Euler * x) * x);
}

internal static bool isNegInt(float64 x) {
    if (x < 0D) {
        var (_, xf) = Modf(x);
        return xf == 0D;
    }
    return false;
}

} // end math_package
