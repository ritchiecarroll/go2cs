// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

internal static UntypedInt float64Mask => 0x7FF;
internal static UntypedInt float64Shift => /* 64 - 11 - 1 */ 52;
internal static UntypedInt float64Bias => 1023;

internal static float64 inf = float64frombits(0x7FF0000000000000UL);

// isNaN reports whether f is an IEEE 754 “not-a-number” value.
internal static bool /*is*/ isNaN(float64 f) {
    // IEEE 754 says that only NaNs satisfy f != f.
    return f != f;
}

// isFinite reports whether f is neither NaN nor an infinity.
internal static bool isFinite(float64 f) {
    return !isNaN(f - f);
}

// isInf reports whether f is an infinity.
internal static bool isInf(float64 f) {
    return !isNaN(f) && !isFinite(f);
}

// abs returns the absolute value of x.
//
// Special cases are:
//
//	abs(±Inf) = +Inf
//	abs(NaN) = NaN
internal static float64 abs(float64 x) {
    const uint64 sign = /* 1 << 63 */ 9223372036854775808;
    return float64frombits((uint64)(float64bits(x) & ~sign));
}

// copysign returns a value with the magnitude
// of x and the sign of y.
internal static float64 copysign(float64 x, float64 y) {
    const uint64 sign = /* 1 << 63 */ 9223372036854775808;
    return float64frombits((uint64)(float64bits(x) & ~sign) | (uint64)(float64bits(y) & sign));
}

// float64bits returns the IEEE 754 binary representation of f.
internal static uint64 float64bits(float64 f) {
    return bitcast<float64, uint64>(f);
}

// float64frombits returns the floating point number corresponding
// the IEEE 754 binary representation b.
internal static float64 float64frombits(uint64 b) {
    return bitcast<uint64, float64>(b);
}

// floor returns the greatest integer value less than or equal to x.
//
// Special cases are:
//
//	floor(±0) = ±0
//	floor(±Inf) = ±Inf
//	floor(NaN) = NaN
//
// N.B. Portable floor copied from math. math also has optimized arch-specific
// implementations.
internal static float64 floor(float64 x) {
    if (x == 0D || isNaN(x) || isInf(x)) {
        return x;
    }
    if (x < 0D) {
        var (dΔ1, fract) = modf(-x);
        if (fract != 0.0D) {
            dΔ1 = dΔ1 + 1D;
        }
        return -dΔ1;
    }
    var (d, _) = modf(x);
    return d;
}

// ceil returns the least integer value greater than or equal to x.
//
// Special cases are:
//
//	Ceil(±0) = ±0
//	Ceil(±Inf) = ±Inf
//	Ceil(NaN) = NaN
//
// N.B. Portable ceil copied from math. math also has optimized arch-specific
// implementations.
internal static float64 ceil(float64 x) {
    return -floor(-x);
}

// modf returns integer and fractional floating-point numbers
// that sum to f. Both values have the same sign as f.
//
// Special cases are:
//
//	Modf(±Inf) = ±Inf, NaN
//	Modf(NaN) = NaN, NaN
//
// N.B. Portable modf copied from math. math also has optimized arch-specific
// implementations.
internal static (float64 @int, float64 frac) modf(float64 f) {
    float64 @int = default;
    float64 frac = default;

    if (f < 1D) {
        switch (ᐧ) {
        case {} when f is < 0D: {
            (@int, frac) = modf(-f);
            return (-@int, -frac);
        }
        case {} when f is 0D: {
            return (f, f); // Return -0, -0 when f == -0
        }}
        return (0D, f);
    }

    var x = float64bits(f);
    nuint e = ((nuint)(x >> (int)(float64Shift)) & (nuint)float64Mask) - (nuint)float64Bias;

    // Keep the top 12+e bits, the integer part; clear the rest.
    if (e < 64 - 12) {
        x &= unchecked((uint64)~(uint64)(((uint64)1).Lsh((64 - 12 - e)) - 1));
    }
    @int = float64frombits(x);
    frac = f - @int;
    return (@int, frac);
}

} // end runtime_package
