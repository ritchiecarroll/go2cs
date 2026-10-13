// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Modf returns integer and fractional floating-point numbers
// that sum to f. Both values have the same sign as f.
//
// Special cases are:
//
//	Modf(±Inf) = ±Inf, NaN
//	Modf(NaN) = NaN, NaN
public static (float64 @int, float64 frac) Modf(float64 f) {
    if (haveArchModf) {
        return archModf(f);
    }
    return modf(f);
}

internal static (float64 @int, float64 frac) modf(float64 f) {
    float64 @int = default!;
    float64 frac = default!;

    if (f < 1D) {
        switch (ᐧ) {
        case {} when f is < 0D: {
            (@int, frac) = Modf(-f);
            return (-@int, -frac);
        }
        case {} when f is 0D: {
            return (f, f); // Return -0, -0 when f == -0
        }}
        return (0D, f);
    }

    var x = Float64bits(f);
    nuint e = (nuint)((nuint)((x >> (int)(shift))) & (nuint)mask) - (nuint)bias;

    // Keep the top 12+e bits, the integer part; clear the rest.
    if (e < 64 - 12) {
        x &= unchecked((uint64)~(uint64)(((uint64)1).Lsh((64 - 12 - e)) - 1));
    }
    @int = Float64frombits(x);
    frac = f - @int;
    return (@int, frac);
}

} // end math_package
