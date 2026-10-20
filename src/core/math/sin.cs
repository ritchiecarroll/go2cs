// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

/*
	Floating-point sine and cosine.
*/
// The original C code, the long comment, and the constants
// below were from http://netlib.sandia.gov/cephes/cmath/sin.c,
// available from http://www.netlib.org/cephes/cmath.tgz.
// The go code is a simplified version of the original C.
//
//      sin.c
//
//      Circular sine
//
// SYNOPSIS:
//
// double x, y, sin();
// y = sin( x );
//
// DESCRIPTION:
//
// Range reduction is into intervals of pi/4.  The reduction error is nearly
// eliminated by contriving an extended precision modular arithmetic.
//
// Two polynomial approximating functions are employed.
// Between 0 and pi/4 the sine is approximated by
//      x  +  x**3 P(x**2).
// Between pi/4 and pi/2 the cosine is represented as
//      1  -  x**2 Q(x**2).
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain      # trials      peak         rms
//    DEC       0, 10       150000       3.0e-17     7.8e-18
//    IEEE -1.07e9,+1.07e9  130000       2.1e-16     5.4e-17
//
// Partial loss of accuracy begins to occur at x = 2**30 = 1.074e9.  The loss
// is not gradual, but jumps suddenly to about 1 part in 10e7.  Results may
// be meaningless for x > 2**49 = 5.6e14.
//
//      cos.c
//
//      Circular cosine
//
// SYNOPSIS:
//
// double x, y, cos();
// y = cos( x );
//
// DESCRIPTION:
//
// Range reduction is into intervals of pi/4.  The reduction error is nearly
// eliminated by contriving an extended precision modular arithmetic.
//
// Two polynomial approximating functions are employed.
// Between 0 and pi/4 the cosine is approximated by
//      1  -  x**2 Q(x**2).
// Between pi/4 and pi/2 the sine is represented as
//      x  +  x**3 P(x**2).
//
// ACCURACY:
//
//                      Relative error:
// arithmetic   domain      # trials      peak         rms
//    IEEE -1.07e9,+1.07e9  130000       2.1e-16     5.4e-17
//    DEC        0,+1.07e9   17000       3.0e-17     7.2e-18
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

// sin coefficients
internal static array<float64> _sin = new float64[]{
    1.58962301576546568060e-10D, // 0x3de5d8fd1fd19ccd
    -2.50507477628578072866e-8D, // 0xbe5ae5e5a9291f5d
    2.75573136213857245213e-6D, // 0x3ec71de3567d48a1
    -1.98412698295895385996e-4D, // 0xbf2a01a019bfdf03
    8.33333333332211858878e-3D, // 0x3f8111111110f7d0
    -1.66666666666666307295e-1D // 0xbfc5555555555548
}.array();

// cos coefficients
internal static array<float64> _cos = new float64[]{
    -1.13585365213876817300e-11D, // 0xbda8fa49a0861a9b
    2.08757008419747316778e-9D, // 0x3e21ee9d7b4e3f05
    -2.75573141792967388112e-7D, // 0xbe927e4f7eac4bc6
    2.48015872888517045348e-5D, // 0x3efa01a019c844f5
    -1.38888888888730564116e-3D, // 0xbf56c16c16c14f91
    4.16666666666665929218e-2D // 0x3fa555555555554b
}.array();

// Cos returns the cosine of the radian argument x.
//
// Special cases are:
//
//	Cos(±Inf) = NaN
//	Cos(NaN) = NaN
public static float64 Cos(float64 x) {
    if (haveArchCos) {
        return archCos(x);
    }
    return cos(x);
}

internal static float64 cos(float64 x) {
    const float64 PI4A = 7.85398125648498535156e-1; // 0x3fe921fb40000000, Pi/4 split into three parts
    const float64 PI4B = 3.77489470793079817668e-8; // 0x3e64442d00000000,
    const float64 PI4C = 2.69515142907905952645e-15; // 0x3ce8469898cc5170,
    // special cases
    switch (ᐧ) {
    case {} when IsNaN(x) || IsInf(x, 0): {
        return NaN();
    }}

    // make argument positive
    var sign = false;
    x = Abs(x);

    uint64 j = default!;
    float64 y = default!;
    float64 z = default!;
    if (x >= reduceThreshold){
        (j, z) = trigReduce(x);
    } else {
        j = (uint64)(x * /* (4 / Pi) */ 1.2732395447351628D); // integer part of x/(Pi/4), as integer for tests on the phase angle
        y = (float64)j; // integer part of x/(Pi/4), as float

        // map zeros to origin
        if ((j & 1) == 1) {
            j++;
            y++;
        }
        j &= (uint64)(7); // octant modulo 2Pi radians (360 degrees)
        z = ((x - y * PI4A) - y * PI4B) - y * PI4C; // Extended precision modular arithmetic
    }

    if (j > 3) {
        j -= 4;
        sign = !sign;
    }
    if (j > 1) {
        sign = !sign;
    }

    var zz = z * z;
    if (j == 1 || j == 2){
        y = z + z * zz * ((((((_sin[0] * zz) + _sin[1]) * zz + _sin[2]) * zz + _sin[3]) * zz + _sin[4]) * zz + _sin[5]);
    } else {
        y = 1.0D - 0.5D * zz + zz * zz * ((((((_cos[0] * zz) + _cos[1]) * zz + _cos[2]) * zz + _cos[3]) * zz + _cos[4]) * zz + _cos[5]);
    }
    if (sign) {
        y = -y;
    }
    return y;
}

// Sin returns the sine of the radian argument x.
//
// Special cases are:
//
//	Sin(±0) = ±0
//	Sin(±Inf) = NaN
//	Sin(NaN) = NaN
public static float64 Sin(float64 x) {
    if (haveArchSin) {
        return archSin(x);
    }
    return sin(x);
}

internal static float64 sin(float64 x) {
    const float64 PI4A = 7.85398125648498535156e-1; // 0x3fe921fb40000000, Pi/4 split into three parts
    const float64 PI4B = 3.77489470793079817668e-8; // 0x3e64442d00000000,
    const float64 PI4C = 2.69515142907905952645e-15; // 0x3ce8469898cc5170,
    // special cases
    switch (ᐧ) {
    case {} when x == 0D || IsNaN(x): {
        return x; // return ±0 || NaN()
    }
    case {} when IsInf(x, 0): {
        return NaN();
    }}

    // make argument positive but save the sign
    var sign = false;
    if (x < 0D) {
        x = -x;
        sign = true;
    }

    uint64 j = default!;
    float64 y = default!;
    float64 z = default!;
    if (x >= reduceThreshold){
        (j, z) = trigReduce(x);
    } else {
        j = (uint64)(x * /* (4 / Pi) */ 1.2732395447351628D); // integer part of x/(Pi/4), as integer for tests on the phase angle
        y = (float64)j; // integer part of x/(Pi/4), as float

        // map zeros to origin
        if ((j & 1) == 1) {
            j++;
            y++;
        }
        j &= (uint64)(7); // octant modulo 2Pi radians (360 degrees)
        z = ((x - y * PI4A) - y * PI4B) - y * PI4C; // Extended precision modular arithmetic
    }
    // reflect in x axis
    if (j > 3) {
        sign = !sign;
        j -= 4;
    }
    var zz = z * z;
    if (j == 1 || j == 2){
        y = 1.0D - 0.5D * zz + zz * zz * ((((((_cos[0] * zz) + _cos[1]) * zz + _cos[2]) * zz + _cos[3]) * zz + _cos[4]) * zz + _cos[5]);
    } else {
        y = z + z * zz * ((((((_sin[0] * zz) + _sin[1]) * zz + _sin[2]) * zz + _sin[3]) * zz + _sin[4]) * zz + _sin[5]);
    }
    if (sign) {
        y = -y;
    }
    return y;
}

} // end math_package
