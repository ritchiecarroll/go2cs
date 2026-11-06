// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cmplx provides basic constants and mathematical functions for
// complex numbers. Special case handling conforms to the C99 standard
// Annex G IEC 60559-compatible complex arithmetic.
namespace go.math;

using math = math_package;

partial class cmplx_package {

// Abs returns the absolute value (also called the modulus) of x.
public static float64 Abs(complex128 x) { return math.Hypot(real(x), imag(x)); }

} // end cmplx_package
