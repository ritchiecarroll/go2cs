// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

/*
	Hypot -- sqrt(p*p + q*q), but overflows only if the result does.
*/

// Hypot returns [Sqrt](p*p + q*q), taking care to avoid
// unnecessary overflow and underflow.
//
// Special cases are:
//
//	Hypot(±Inf, q) = +Inf
//	Hypot(p, ±Inf) = +Inf
//	Hypot(NaN, q) = NaN
//	Hypot(p, NaN) = NaN
public static float64 Hypot(float64 p, float64 q) {
    if (haveArchHypot) {
        return archHypot(p, q);
    }
    return hypot(p, q);
}

internal static float64 hypot(float64 p, float64 q) {
    (p, q) = (Abs(p), Abs(q));
    // special cases
    switch (ᐧ) {
    case {} when IsInf(p, 1) || IsInf(q, 1): {
        return Inf(1);
    }
    case {} when IsNaN(p) || IsNaN(q): {
        return NaN();
    }}
    if (p < q) {
        (p, q) = (q, p);
    }
    if (p == 0D) {
        return 0D;
    }
    q = q / p;
    return p * Sqrt(1D + q * q);
}

} // end math_package
