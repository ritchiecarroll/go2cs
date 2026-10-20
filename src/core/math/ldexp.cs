// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Ldexp is the inverse of [Frexp].
// It returns frac × 2**exp.
//
// Special cases are:
//
//	Ldexp(±0, exp) = ±0
//	Ldexp(±Inf, exp) = ±Inf
//	Ldexp(NaN, exp) = NaN
public static float64 Ldexp(float64 frac, nint exp) {
    if (haveArchLdexp) {
        return archLdexp(frac, exp);
    }
    return ldexp(frac, exp);
}

internal static float64 ldexp(float64 frac, nint exp) {
    // special cases
    switch (ᐧ) {
    case {} when frac is 0D: {
        return frac; // correctly return -0
    }
    case {} when IsInf(frac, 0) || IsNaN(frac): {
        return frac;
    }}
    (frac, var e) = normalize(frac);
    exp += e;
    var x = Float64bits(frac);
    exp += ((nint)(x >> (int)(shift)) & (nint)mask) - (nint)bias;
    if (exp < -1075) {
        return Copysign(0D, frac); // underflow
    }
    if (exp > 1023) { // overflow
        if (frac < 0D) {
            return Inf(-1);
        }
        return Inf(1);
    }
    float64 m = 1D;
    if (exp < -1022) { // denormal
        exp += 53;
        m = 1.0D / (9007199254740992D); // 2**-53
    }
    x &= unchecked((uint64)~(uint64)(((uint64)mask << (int)(shift))));
    x |= (uint64)(((uint64)(exp + (nint)bias) << (int)(shift)));
    return m * Float64frombits(x);
}

} // end math_package
