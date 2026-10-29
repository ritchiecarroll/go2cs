// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Nextafter32 returns the next representable float32 value after x towards y.
//
// Special cases are:
//
//	Nextafter32(x, x)   = x
//	Nextafter32(NaN, y) = NaN
//	Nextafter32(x, NaN) = NaN
public static float32 /*r*/ Nextafter32(float32 x, float32 y) {
    float32 r = default;

    switch (ᐧ) {
    case {} when IsNaN((float64)x) || IsNaN((float64)y): { // special case
        r = (float32)NaN();
        break;
    }
    case {} when x == y: {
        r = x;
        break;
    }
    case {} when x is 0F: {
        r = (float32)Copysign((float64)Float32frombits(1), (float64)y);
        break;
    }
    case {} when (y > x) == (x > 0F): {
        r = Float32frombits(Float32bits(x) + 1);
        break;
    }
    default: {
        r = Float32frombits(Float32bits(x) - 1);
        break;
    }}
    return r;
}

// Nextafter returns the next representable float64 value after x towards y.
//
// Special cases are:
//
//	Nextafter(x, x)   = x
//	Nextafter(NaN, y) = NaN
//	Nextafter(x, NaN) = NaN
public static float64 /*r*/ Nextafter(float64 x, float64 y) {
    float64 r = default;

    switch (ᐧ) {
    case {} when IsNaN(x) || IsNaN(y): { // special case
        r = NaN();
        break;
    }
    case {} when x == y: {
        r = x;
        break;
    }
    case {} when x is 0D: {
        r = Copysign(Float64frombits(1), y);
        break;
    }
    case {} when (y > x) == (x > 0D): {
        r = Float64frombits(Float64bits(x) + 1);
        break;
    }
    default: {
        r = Float64frombits(Float64bits(x) - 1);
        break;
    }}
    return r;
}

} // end math_package
