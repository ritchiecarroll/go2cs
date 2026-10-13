// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

/*
	Floating-point arcsine and arccosine.

	They are implemented by computing the arctangent
	after appropriate range reduction.
*/

// Asin returns the arcsine, in radians, of x.
//
// Special cases are:
//
//	Asin(±0) = ±0
//	Asin(x) = NaN if x < -1 or x > 1
public static float64 Asin(float64 x) {
    if (haveArchAsin) {
        return archAsin(x);
    }
    return asin(x);
}

internal static float64 asin(float64 x) {
    if (x == 0D) {
        return x; // special case
    }
    var sign = false;
    if (x < 0D) {
        x = -x;
        sign = true;
    }
    if (x > 1D) {
        return NaN(); // special case
    }

    var temp = Sqrt(1D - x * x);
    if (x > 0.7D){
        temp = /* Pi / 2 */ 1.5707963267948966D - satan(temp / x);
    } else {
        temp = satan(x / temp);
    }

    if (sign) {
        temp = -temp;
    }
    return temp;
}

// Acos returns the arccosine, in radians, of x.
//
// Special case is:
//
//	Acos(x) = NaN if x < -1 or x > 1
public static float64 Acos(float64 x) {
    if (haveArchAcos) {
        return archAcos(x);
    }
    return acos(x);
}

internal static float64 acos(float64 x) {
    return /* Pi / 2 */ 1.5707963267948966D - Asin(x);
}

} // end math_package
