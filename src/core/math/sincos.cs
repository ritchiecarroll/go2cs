// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Coefficients _sin[] and _cos[] are found in pkg/math/sin.go.

// Sincos returns Sin(x), Cos(x).
//
// Special cases are:
//
//	Sincos(±0) = ±0, 1
//	Sincos(±Inf) = NaN, NaN
//	Sincos(NaN) = NaN, NaN
public static (float64 sin, float64 cos) Sincos(float64 x) {
    float64 sin = default!;
    float64 cos = default!;

    const float64 PI4A = 7.85398125648498535156e-1; // 0x3fe921fb40000000, Pi/4 split into three parts
    const float64 PI4B = 3.77489470793079817668e-8; // 0x3e64442d00000000,
    const float64 PI4C = 2.69515142907905952645e-15; // 0x3ce8469898cc5170,
    // special cases
    switch (ᐧ) {
    case {} when x is 0D: {
        return (x, 1D); // return ±0.0, 1.0
    }
    case {} when IsNaN(x) || IsInf(x, 0): {
        return (NaN(), NaN());
    }}

    // make argument positive
    var (sinSign, cosSign) = (false, false);
    if (x < 0D) {
        x = -x;
        sinSign = true;
    }

    uint64 j = default!;
    float64 y = default!;
    float64 z = default!;
    if (x >= reduceThreshold){
        (j, z) = trigReduce(x);
    } else {
        j = (uint64)(x * /* (4 / Pi) */ 1.2732395447351628D); // integer part of x/(Pi/4), as integer for tests on the phase angle
        y = (float64)j; // integer part of x/(Pi/4), as float

        if ((j & 1) == 1) { // map zeros to origin
            j++;
            y++;
        }
        j &= (uint64)(7); // octant modulo 2Pi radians (360 degrees)
        z = ((x - y * PI4A) - y * PI4B) - y * PI4C; // Extended precision modular arithmetic
    }
    if (j > 3) { // reflect in x axis
        j -= 4;
        (sinSign, cosSign) = (!sinSign, !cosSign);
    }
    if (j > 1) {
        cosSign = !cosSign;
    }

    var zz = z * z;
    cos = 1.0D - 0.5D * zz + zz * zz * ((((((_cos[0] * zz) + _cos[1]) * zz + _cos[2]) * zz + _cos[3]) * zz + _cos[4]) * zz + _cos[5]);
    sin = z + z * zz * ((((((_sin[0] * zz) + _sin[1]) * zz + _sin[2]) * zz + _sin[3]) * zz + _sin[4]) * zz + _sin[5]);
    if (j == 1 || j == 2) {
        (sin, cos) = (cos, sin);
    }
    if (cosSign) {
        cos = -cos;
    }
    if (sinSign) {
        sin = -sin;
    }
    return (sin, cos);
}

} // end math_package
