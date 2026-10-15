// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Floor returns the greatest integer value less than or equal to x.
//
// Special cases are:
//
//	Floor(±0) = ±0
//	Floor(±Inf) = ±Inf
//	Floor(NaN) = NaN
public static float64 Floor(float64 x) {
    if (haveArchFloor) {
        return archFloor(x);
    }
    return floor(x);
}

internal static float64 floor(float64 x) {
    if (x == 0D || IsNaN(x) || IsInf(x, 0)) {
        return x;
    }
    if (x < 0D) {
        var (dΔ1, fract) = Modf(-x);
        if (fract != 0.0D) {
            dΔ1 = dΔ1 + 1D;
        }
        return -dΔ1;
    }
    var (d, _) = Modf(x);
    return d;
}

// Ceil returns the least integer value greater than or equal to x.
//
// Special cases are:
//
//	Ceil(±0) = ±0
//	Ceil(±Inf) = ±Inf
//	Ceil(NaN) = NaN
public static float64 Ceil(float64 x) {
    if (haveArchCeil) {
        return archCeil(x);
    }
    return ceil(x);
}

internal static float64 ceil(float64 x) {
    return -Floor(-x);
}

// Trunc returns the integer value of x.
//
// Special cases are:
//
//	Trunc(±0) = ±0
//	Trunc(±Inf) = ±Inf
//	Trunc(NaN) = NaN
public static float64 Trunc(float64 x) {
    if (haveArchTrunc) {
        return archTrunc(x);
    }
    return trunc(x);
}

internal static float64 trunc(float64 x) {
    if (Abs(x) < 1D) {
        return Copysign(0D, x);
    }

    var b = Float64bits(x);
    nuint e = (nuint)((nuint)((b >> (int)(shift))) & (nuint)mask) - (nuint)bias;

    // Keep the top 12+e bits, the integer part; clear the rest.
    if (e < 64 - 12) {
        b &= unchecked((uint64)~(uint64)(((uint64)1).Lsh((64 - 12 - e)) - 1));
    }
    return Float64frombits(b);
}

// Round returns the nearest integer, rounding half away from zero.
//
// Special cases are:
//
//	Round(±0) = ±0
//	Round(±Inf) = ±Inf
//	Round(NaN) = NaN
public static float64 Round(float64 x) {
    // Round is a faster implementation of:
    //
    // func Round(x float64) float64 {
    //   t := Trunc(x)
    //   if Abs(x-t) >= 0.5 {
    //     return t + Copysign(1, x)
    //   }
    //   return t
    // }
    var bits = Float64bits(x);
    nuint e = (nuint)((nuint)((bits >> (int)(shift))) & (nuint)mask);
    if (e < bias){
        // Round abs(x) < 1 including denormals.
        bits &= (uint64)(signMask); // +-0
        if (e == (nuint)(bias - 1)) {
            bits |= (uint64)(uvone); // +-1
        }
    } else 
    if (e < (nuint)(bias + shift)) {
        // Round any abs(x) >= 1 containing a fractional component [0,1).
        //
        // Numbers with larger exponents are returned unchanged since they
        // must be either an integer, infinity, or NaN.
        const uint64 half = /* 1 << (shift - 1) */ 2251799813685248;
        e -= bias;
        bits += half.Rsh(e);
        bits &= unchecked((uint64)~(uint64)(((uint64)fracMask).Rsh(e)));
    }
    return Float64frombits(bits);
}

// RoundToEven returns the nearest integer, rounding ties to even.
//
// Special cases are:
//
//	RoundToEven(±0) = ±0
//	RoundToEven(±Inf) = ±Inf
//	RoundToEven(NaN) = NaN
public static float64 RoundToEven(float64 x) {
    // RoundToEven is a faster implementation of:
    //
    // func RoundToEven(x float64) float64 {
    //   t := math.Trunc(x)
    //   odd := math.Remainder(t, 2) != 0
    //   if d := math.Abs(x - t); d > 0.5 || (d == 0.5 && odd) {
    //     return t + math.Copysign(1, x)
    //   }
    //   return t
    // }
    var bits = Float64bits(x);
    nuint e = (nuint)((nuint)((bits >> (int)(shift))) & (nuint)mask);
    if (e >= bias){
        // Round abs(x) >= 1.
        // - Large numbers without fractional components, infinity, and NaN are unchanged.
        // - Add 0.499.. or 0.5 before truncating depending on whether the truncated
        //   number is even or odd (respectively).
        const uint64 halfMinusULP = /* (1 << (shift - 1)) - 1 */ 2251799813685247;
        e -= bias;
        bits += (halfMinusULP + (uint64)((bits.Rsh(((nuint)shift - e))) & 1)).Rsh(e);
        bits &= unchecked((uint64)~(uint64)(((uint64)fracMask).Rsh(e)));
    } else 
    if (e == (nuint)(bias - 1) && (uint64)(bits & (uint64)fracMask) != 0){
        // Round 0.5 < abs(x) < 1.
        bits = (uint64)((uint64)(bits & (uint64)signMask) | (uint64)uvone); // +-1
    } else {
        // Round abs(x) <= 0.5 including denormals.
        bits &= (uint64)(signMask); // +-0
    }
    return Float64frombits(bits);
}

} // end math_package
