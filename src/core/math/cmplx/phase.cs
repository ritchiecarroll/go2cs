// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using math = math_package;

partial class cmplx_package {

// Phase returns the phase (also called the argument) of x.
// The returned value is in the range [-Pi, Pi].
public static float64 Phase(complex128 x) { return math.Atan2(imag(x), real(x)); }

} // end cmplx_package
