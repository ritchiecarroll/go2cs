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
public static (float64 integer, float64 fractional) Modf(float64 f) {
    float64 integer = default!;
    float64 fractional = default!;

    integer = Trunc(f);
    fractional = Copysign(f - integer, f);
    return (integer, fractional);
}

} // end math_package
