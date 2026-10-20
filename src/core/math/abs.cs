// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Abs returns the absolute value of x.
//
// Special cases are:
//
//	Abs(±Inf) = +Inf
//	Abs(NaN) = NaN
public static float64 Abs(float64 x) {
    return Float64frombits((uint64)(Float64bits(x) & ~((uint64)1 << 63)));
}

} // end math_package
