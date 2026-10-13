// Copyright 2009-2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

/*
	Floating-point mod function.
*/

// Mod returns the floating-point remainder of x/y.
// The magnitude of the result is less than y and its
// sign agrees with that of x.
//
// Special cases are:
//
//	Mod(±Inf, y) = NaN
//	Mod(NaN, y) = NaN
//	Mod(x, 0) = NaN
//	Mod(x, ±Inf) = x
//	Mod(x, NaN) = NaN
public static float64 Mod(float64 x, float64 y) {
    if (haveArchMod) {
        return archMod(x, y);
    }
    return mod(x, y);
}

internal static float64 mod(float64 x, float64 y) {
    if (y == 0D || IsInf(x, 0) || IsNaN(x) || IsNaN(y)) {
        return NaN();
    }
    y = Abs(y);

    var (yfr, yexp) = Frexp(y);
    var r = x;
    if (x < 0D) {
        r = -x;
    }

    while (r >= y) {
        var (rfr, rexp) = Frexp(r);
        if (rfr < yfr) {
            rexp = rexp - 1;
        }
        r = r - Ldexp(y, rexp - yexp);
    }
    if (x < 0D) {
        r = -r;
    }
    return r;
}

} // end math_package
