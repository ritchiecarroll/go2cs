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
// Complex square root
//
// DESCRIPTION:
//
// If z = x + iy,  r = |z|, then
//
//                       1/2
// Re w  =  [ (r + x)/2 ]   ,
//
//                       1/2
// Im w  =  [ (r - x)/2 ]   .
//
// Cancellation error in r-x or r+x is avoided by using the
// identity  2 Re w Im w  =  y.
//
// Note that -w is also a square root of z. The root chosen
// is always in the right half plane and Im w has the same sign as y.
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain     # trials      peak         rms
//    DEC       -10,+10     25000       3.2e-17     9.6e-18
//    IEEE      -10,+10   1,000,000     2.9e-16     6.1e-17

// Sqrt returns the square root of x.
// The result r is chosen so that real(r) ≥ 0 and imag(r) has the same sign as imag(x).
public static complex128 Sqrt(complex128 x) {
    if (imag(x) == 0D) {
        // Ensure that imag(r) has the same sign as imag(x) for imag(x) == signed zero.
        if (real(x) == 0D) {
            return complex(0D, imag(x));
        }
        if (real(x) < 0D) {
            return complex(0D, math.Copysign(math.Sqrt(-real(x)), imag(x)));
        }
        return complex(math.Sqrt(real(x)), imag(x));
    } else if (math.IsInf(imag(x), 0)) {
        return complex(math.Inf(1), imag(x));
    }
    if (real(x) == 0D) {
        if (imag(x) < 0D) {
            var rΔ1 = math.Sqrt(-0.5D * imag(x));
            return complex(rΔ1, -rΔ1);
        }
        var rΔ2 = math.Sqrt(0.5D * imag(x));
        return complex(rΔ2, rΔ2);
    }
    var a = real(x);
    var b = imag(x);
    float64 scale = default;
    // Rescale to avoid internal overflow or underflow.
    if (math.Abs(a) > 4D || math.Abs(b) > 4D) {
        a *= 0.25D;
        b *= 0.25D;
        scale = 2D;
    } else {
        a *= 1.8014398509481984e16D; // 2**54
        b *= 1.8014398509481984e16D;
        scale = 7.450580596923828125e-9D; // 2**-27
    }
    var r = math.Hypot(a, b);
    float64 t = default;
    if (a > 0D) {
        t = math.Sqrt(0.5D * r + 0.5D * a);
        r = scale * math.Abs((0.5D * b) / t);
        t *= scale;
    } else {
        r = math.Sqrt(0.5D * r - 0.5D * a);
        t = scale * math.Abs((0.5D * b) / r);
        r *= scale;
    }
    if (b < 0D) {
        return complex(t, -r);
    }
    return complex(t, r);
}

} // end cmplx_package
