// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Logb returns the binary exponent of x.
//
// Special cases are:
//
//	Logb(±Inf) = +Inf
//	Logb(0) = -Inf
//	Logb(NaN) = NaN
public static float64 Logb(float64 x) {
    // special cases
    switch (ᐧ) {
    case {} when x is 0D: {
        return Inf(-1);
    }
    case {} when IsInf(x, 0): {
        return Inf(1);
    }
    case {} when IsNaN(x): {
        return x;
    }}
    return (float64)ilogb(x);
}

// Ilogb returns the binary exponent of x as an integer.
//
// Special cases are:
//
//	Ilogb(±Inf) = MaxInt32
//	Ilogb(0) = MinInt32
//	Ilogb(NaN) = MaxInt32
public static nint Ilogb(float64 x) {
    // special cases
    switch (ᐧ) {
    case {} when x is 0D: {
        return MinInt32;
    }
    case {} when IsNaN(x): {
        return MaxInt32;
    }
    case {} when IsInf(x, 0): {
        return MaxInt32;
    }}
    return ilogb(x);
}

// ilogb returns the binary exponent of x. It assumes x is finite and
// non-zero.
internal static nint ilogb(float64 x) {
    (x, var exp) = normalize(x);
    return (nint)((Float64bits(x) >> (int)(shift)) & (uint64)mask) - (nint)bias + exp;
}

} // end math_package
