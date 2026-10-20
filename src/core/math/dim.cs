// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Dim returns the maximum of x-y or 0.
//
// Special cases are:
//
//	Dim(+Inf, +Inf) = NaN
//	Dim(-Inf, -Inf) = NaN
//	Dim(x, NaN) = Dim(NaN, x) = NaN
public static float64 Dim(float64 x, float64 y) {
    // The special cases result in NaN after the subtraction:
    //      +Inf - +Inf = NaN
    //      -Inf - -Inf = NaN
    //       NaN - y    = NaN
    //         x - NaN  = NaN
    var v = x - y;
    if (v <= 0D) {
        // v is negative or 0
        return 0D;
    }
    // v is positive or NaN
    return v;
}

// Max returns the larger of x or y.
//
// Special cases are:
//
//	Max(x, +Inf) = Max(+Inf, x) = +Inf
//	Max(x, NaN) = Max(NaN, x) = NaN
//	Max(+0, ±0) = Max(±0, +0) = +0
//	Max(-0, -0) = -0
//
// Note that this differs from the built-in function max when called
// with NaN and +Inf.
public static float64 Max(float64 x, float64 y) {
    if (haveArchMax) {
        return archMax(x, y);
    }
    return max(x, y);
}

internal static float64 max(float64 x, float64 y) {
    // special cases
    switch (ᐧ) {
    case {} when IsInf(x, 1) || IsInf(y, 1): {
        return Inf(1);
    }
    case {} when IsNaN(x) || IsNaN(y): {
        return NaN();
    }
    case {} when x == 0D && x == y: {
        if (Signbit(x)) {
            return y;
        }
        return x;
    }}
    if (x > y) {
        return x;
    }
    return y;
}

// Min returns the smaller of x or y.
//
// Special cases are:
//
//	Min(x, -Inf) = Min(-Inf, x) = -Inf
//	Min(x, NaN) = Min(NaN, x) = NaN
//	Min(-0, ±0) = Min(±0, -0) = -0
//
// Note that this differs from the built-in function min when called
// with NaN and -Inf.
public static float64 Min(float64 x, float64 y) {
    if (haveArchMin) {
        return archMin(x, y);
    }
    return min(x, y);
}

internal static float64 min(float64 x, float64 y) {
    // special cases
    switch (ᐧ) {
    case {} when IsInf(x, -1) || IsInf(y, -1): {
        return Inf(-1);
    }
    case {} when IsNaN(x) || IsNaN(y): {
        return NaN();
    }
    case {} when x == 0D && x == y: {
        if (Signbit(x)) {
            return x;
        }
        return y;
    }}
    if (x < y) {
        return x;
    }
    return y;
}

} // end math_package
