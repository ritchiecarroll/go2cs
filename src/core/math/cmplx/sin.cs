// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using math = math_package;

partial class cmplx_package {

// The original C code, the long comment, and the constants
// below are from http://netlib.sandia.gov/cephes/c9x-complex/clog.c.
// The go code is a simplified version of the original C.
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
// Complex circular sine
//
// DESCRIPTION:
//
// If
//     z = x + iy,
//
// then
//
//     w = sin x  cosh y  +  i cos x sinh y.
//
// csin(z) = -i csinh(iz).
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    DEC       -10,+10      8400       5.3e-17     1.3e-17
//    IEEE      -10,+10     30000       3.8e-16     1.0e-16
// Also tested by csin(casin(z)) = z.

// Sin returns the sine of x.
public static complex128 Sin(complex128 x) {
    {
        var (re, im) = (real(x), imag(x));
        switch (ᐧ) {
        case {} when im == 0D && (math.IsInf(re, 0) || math.IsNaN(re)): {
            return complex(math.NaN(), im);
        }
        case {} when math.IsInf(im, 0): {
            switch (ᐧ) {
            case {} when re is 0D: {
                return x;
            }
            case {} when math.IsInf(re, 0) || math.IsNaN(re): {
                return complex(math.NaN(), im);
            }}
            break;
        }
        case {} when re == 0D && math.IsNaN(im): {
            return x;
        }}
    }
    var (s, c) = math.Sincos(real(x));
    var (sh, ch) = sinhcosh(imag(x));
    return complex(s * ch, c * sh);
}

// Complex hyperbolic sine
//
// DESCRIPTION:
//
// csinh z = (cexp(z) - cexp(-z))/2
//         = sinh x * cos y  +  i cosh x * sin y .
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    IEEE      -10,+10     30000       3.1e-16     8.2e-17

// Sinh returns the hyperbolic sine of x.
public static complex128 Sinh(complex128 x) {
    {
        var (re, im) = (real(x), imag(x));
        switch (ᐧ) {
        case {} when re == 0D && (math.IsInf(im, 0) || math.IsNaN(im)): {
            return complex(re, math.NaN());
        }
        case {} when math.IsInf(re, 0): {
            switch (ᐧ) {
            case {} when im is 0D: {
                return complex(re, im);
            }
            case {} when math.IsInf(im, 0) || math.IsNaN(im): {
                return complex(re, math.NaN());
            }}
            break;
        }
        case {} when im == 0D && math.IsNaN(re): {
            return complex(math.NaN(), im);
        }}
    }
    var (s, c) = math.Sincos(imag(x));
    var (sh, ch) = sinhcosh(real(x));
    return complex(c * sh, s * ch);
}

// Complex circular cosine
//
// DESCRIPTION:
//
// If
//     z = x + iy,
//
// then
//
//     w = cos x  cosh y  -  i sin x sinh y.
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    DEC       -10,+10      8400       4.5e-17     1.3e-17
//    IEEE      -10,+10     30000       3.8e-16     1.0e-16

// Cos returns the cosine of x.
public static complex128 Cos(complex128 x) {
    {
        var (re, im) = (real(x), imag(x));
        switch (ᐧ) {
        case {} when im == 0D && (math.IsInf(re, 0) || math.IsNaN(re)): {
            return complex(math.NaN(), -im * math.Copysign(0D, re));
        }
        case {} when math.IsInf(im, 0): {
            switch (ᐧ) {
            case {} when re is 0D: {
                return complex(math.Inf(1), -re * math.Copysign(0D, im));
            }
            case {} when math.IsInf(re, 0) || math.IsNaN(re): {
                return complex(math.Inf(1), math.NaN());
            }}
            break;
        }
        case {} when re == 0D && math.IsNaN(im): {
            return complex(math.NaN(), 0D);
        }}
    }
    var (s, c) = math.Sincos(real(x));
    var (sh, ch) = sinhcosh(imag(x));
    return complex(c * ch, -s * sh);
}

// Complex hyperbolic cosine
//
// DESCRIPTION:
//
// ccosh(z) = cosh x  cos y + i sinh x sin y .
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    IEEE      -10,+10     30000       2.9e-16     8.1e-17

// Cosh returns the hyperbolic cosine of x.
public static complex128 Cosh(complex128 x) {
    {
        var (re, im) = (real(x), imag(x));
        switch (ᐧ) {
        case {} when re == 0D && (math.IsInf(im, 0) || math.IsNaN(im)): {
            return complex(math.NaN(), re * math.Copysign(0D, im));
        }
        case {} when math.IsInf(re, 0): {
            switch (ᐧ) {
            case {} when im is 0D: {
                return complex(math.Inf(1), im * math.Copysign(0D, re));
            }
            case {} when math.IsInf(im, 0) || math.IsNaN(im): {
                return complex(math.Inf(1), math.NaN());
            }}
            break;
        }
        case {} when im == 0D && math.IsNaN(re): {
            return complex(math.NaN(), im);
        }}
    }
    var (s, c) = math.Sincos(imag(x));
    var (sh, ch) = sinhcosh(real(x));
    return complex(c * ch, s * sh);
}

// calculate sinh and cosh.
internal static (float64 sh, float64 ch) sinhcosh(float64 x) {
    if (math.Abs(x) <= 0.5D) {
        return (math.Sinh(x), math.Cosh(x));
    }
    var e = math.Exp(x);
    var ei = 0.5D / e;
    e *= 0.5D;
    return (e - ei, e + ei);
}

} // end cmplx_package
