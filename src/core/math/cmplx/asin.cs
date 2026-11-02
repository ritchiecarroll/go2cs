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
// Complex circular arc sine
//
// DESCRIPTION:
//
// Inverse complex sine:
//                               2
// w = -i clog( iz + csqrt( 1 - z ) ).
//
// casin(z) = -i casinh(iz)
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    DEC       -10,+10     10100       2.1e-15     3.4e-16
//    IEEE      -10,+10     30000       2.2e-14     2.7e-15
// Larger relative error can be observed for z near zero.
// Also tested by csin(casin(z)) = z.

// Asin returns the inverse sine of x.
public static complex128 Asin(complex128 x) {
    {
        var (re, im) = (real(x), imag(x));
        switch (ᐧ) {
        case {} when im == 0D && math.Abs(re) <= 1D: {
            return complex(math.Asin(re), im);
        }
        case {} when re == 0D && math.Abs(im) <= 1D: {
            return complex(re, math.Asinh(im));
        }
        case {} when math.IsNaN(im): {
            switch (ᐧ) {
            case {} when re is 0D: {
                return complex(re, math.NaN());
            }
            case {} when math.IsInf(re, 0): {
                return complex(math.NaN(), re);
            }
            default: {
                return NaN();
            }}
            break;
        }
        case {} when math.IsInf(im, 0): {
            switch (ᐧ) {
            case {} when math.IsNaN(re): {
                return x;
            }
            case {} when math.IsInf(re, 0): {
                return complex(math.Copysign(math.Pi / 4D, re), im);
            }
            default: {
                return complex(math.Copysign(0D, re), im);
            }}
            break;
        }
        case {} when math.IsInf(re, 0): {
            return complex(math.Copysign(math.Pi / 2D, re), math.Copysign(re, im));
        }}
    }
    var ct = complex(-imag(x), real(x)); // i * x
    var xx = x * x;
    var x1 = complex(1D - real(xx), -imag(xx)); // 1 - x*x
    var x2 = Sqrt(x1);                          // x2 = sqrt(1 - x*x)
    var w = Log(ct + x2);
    return complex(imag(w), -real(w)); // -i * w
}

// Asinh returns the inverse hyperbolic sine of x.
public static complex128 Asinh(complex128 x) {
    {
        var (re, im) = (real(x), imag(x));
        switch (ᐧ) {
        case {} when im == 0D && math.Abs(re) <= 1D: {
            return complex(math.Asinh(re), im);
        }
        case {} when re == 0D && math.Abs(im) <= 1D: {
            return complex(re, math.Asin(im));
        }
        case {} when math.IsInf(re, 0): {
            switch (ᐧ) {
            case {} when math.IsInf(im, 0): {
                return complex(re, math.Copysign(math.Pi / 4D, im));
            }
            case {} when math.IsNaN(im): {
                return x;
            }
            default: {
                return complex(re, math.Copysign(0.0D, im));
            }}
            break;
        }
        case {} when math.IsNaN(re): {
            switch (ᐧ) {
            case {} when im is 0D: {
                return x;
            }
            case {} when math.IsInf(im, 0): {
                return complex(im, re);
            }
            default: {
                return NaN();
            }}
            break;
        }
        case {} when math.IsInf(im, 0): {
            return complex(math.Copysign(im, re), math.Copysign(math.Pi / 2D, im));
        }}
    }
    var xx = x * x;
    var x1 = complex(1D + real(xx), imag(xx)); // 1 + x*x
    return Log(x + Sqrt(x1));                  // log(x + sqrt(1 + x*x))
}

// Complex circular arc cosine
//
// DESCRIPTION:
//
// w = arccos z  =  PI/2 - arcsin z.
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    DEC       -10,+10      5200      1.6e-15      2.8e-16
//    IEEE      -10,+10     30000      1.8e-14      2.2e-15

// Acos returns the inverse cosine of x.
public static complex128 Acos(complex128 x) {
    var w = Asin(x);
    return complex(/* math.Pi / 2 */ 1.5707963267948966D - real(w), -imag(w));
}

// Acosh returns the inverse hyperbolic cosine of x.
public static complex128 Acosh(complex128 x) {
    if (x == 0D) {
        return complex(0D, math.Copysign(math.Pi / 2D, imag(x)));
    }
    var w = Acos(x);
    if (imag(w) <= 0D) {
        return complex(-imag(w), real(w)); // i * w
    }
    return complex(imag(w), -real(w)); // -i * w
}

// Complex circular arc tangent
//
// DESCRIPTION:
//
// If
//     z = x + iy,
//
// then
//          1       (    2x     )
// Re w  =  - arctan(-----------)  +  k PI
//          2       (     2    2)
//                  (1 - x  - y )
//
//               ( 2         2)
//          1    (x  +  (y+1) )
// Im w  =  - log(------------)
//          4    ( 2         2)
//               (x  +  (y-1) )
//
// Where k is an arbitrary integer.
//
// catan(z) = -i catanh(iz).
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    DEC       -10,+10      5900       1.3e-16     7.8e-18
//    IEEE      -10,+10     30000       2.3e-15     8.5e-17
// The check catan( ctan(z) )  =  z, with |x| and |y| < PI/2,
// had peak relative error 1.5e-16, rms relative error
// 2.9e-17.  See also clog().

// Atan returns the inverse tangent of x.
public static complex128 Atan(complex128 x) {
    {
        var (re, im) = (real(x), imag(x));
        switch (ᐧ) {
        case {} when im is 0D: {
            return complex(math.Atan(re), im);
        }
        case {} when re == 0D && math.Abs(im) <= 1D: {
            return complex(re, math.Atanh(im));
        }
        case {} when math.IsInf(im, 0) || math.IsInf(re, 0): {
            if (math.IsNaN(re)) {
                return complex(math.NaN(), math.Copysign(0D, im));
            }
            return complex(math.Copysign(math.Pi / 2D, re), math.Copysign(0D, im));
        }
        case {} when math.IsNaN(re) || math.IsNaN(im): {
            return NaN();
        }}
    }
    var x2 = real(x) * real(x);
    var a = 1D - x2 - imag(x) * imag(x);
    if (a == 0D) {
        return NaN();
    }
    var t = 0.5D * math.Atan2(2D * real(x), a);
    var w = reducePi(t);

    t = imag(x) - 1D;
    var b = x2 + t * t;
    if (b == 0D) {
        return NaN();
    }
    t = imag(x) + 1D;
    var c = (x2 + t * t) / b;
    return complex(w, 0.25D * math.Log(c));
}

// Atanh returns the inverse hyperbolic tangent of x.
public static complex128 Atanh(complex128 x) {
    var z = complex(-imag(x), real(x)); // z = i * x
    z = Atan(z);
    return complex(imag(z), -real(z)); // z = -i * z
}

} // end cmplx_package
