// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Signbit reports whether x is negative or negative zero.
public static bool Signbit(float64 x) {
    return (int64)Float64bits(x) < 0;
}

} // end math_package
