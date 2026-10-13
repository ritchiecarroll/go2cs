// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Atan2 returns the arc tangent of y/x, using
// the signs of the two to determine the quadrant
// of the return value.
//
// Special cases are (in order):
//
//	Atan2(y, NaN) = NaN
//	Atan2(NaN, x) = NaN
//	Atan2(+0, x>=0) = +0
//	Atan2(-0, x>=0) = -0
//	Atan2(+0, x<=-0) = +Pi
//	Atan2(-0, x<=-0) = -Pi
//	Atan2(y>0, 0) = +Pi/2
//	Atan2(y<0, 0) = -Pi/2
//	Atan2(+Inf, +Inf) = +Pi/4
//	Atan2(-Inf, +Inf) = -Pi/4
//	Atan2(+Inf, -Inf) = 3Pi/4
//	Atan2(-Inf, -Inf) = -3Pi/4
//	Atan2(y, +Inf) = 0
//	Atan2(y>0, -Inf) = +Pi
//	Atan2(y<0, -Inf) = -Pi
//	Atan2(+Inf, x) = +Pi/2
//	Atan2(-Inf, x) = -Pi/2
public static float64 Atan2(float64 y, float64 x) {
    if (haveArchAtan2) {
        return archAtan2(y, x);
    }
    return atan2(y, x);
}

internal static float64 atan2(float64 y, float64 x) {
    // special cases
    switch (ᐧ) {
    case {} when IsNaN(y) || IsNaN(x): {
        return NaN();
    }
    case {} when y is 0D: {
        if (x >= 0D && !Signbit(x)) {
            return Copysign(0D, y);
        }
        return Copysign(Pi, y);
    }
    case {} when x is 0D: {
        return Copysign(Pi / 2D, y);
    }
    case {} when IsInf(x, 0): {
        if (IsInf(x, 1)) {
            switch (ᐧ) {
            case {} when IsInf(y, 0): {
                return Copysign(Pi / 4D, y);
            }
            default: {
                return Copysign(0D, y);
            }}
        }
        switch (ᐧ) {
        case {} when IsInf(y, 0): {
            return Copysign(3D * Pi / 4D, y);
        }
        default: {
            return Copysign(Pi, y);
        }}
        break;
    }
    case {} when IsInf(y, 0): {
        return Copysign(Pi / 2D, y);
    }}

    // Call atan and determine the quadrant.
    var q = Atan(y / x);
    if (x < 0D) {
        if (q <= 0D) {
            return q + (float64)Pi;
        }
        return q - (float64)Pi;
    }
    return q;
}

} // end math_package
