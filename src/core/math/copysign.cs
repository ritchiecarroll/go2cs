// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Copysign returns a value with the magnitude of f
// and the sign of sign.
public static float64 Copysign(float64 f, float64 sign) {
    const uint64 signBit = /* 1 << 63 */ 9223372036854775808;
    return Float64frombits((uint64)(Float64bits(f) & ~signBit) | (uint64)(Float64bits(sign) & signBit));
}

} // end math_package
