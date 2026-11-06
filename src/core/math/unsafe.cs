// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class math_package {

// Despite being an exported symbol,
// Float32bits is linknamed by widely used packages.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/num
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
// Note that this comment is not part of the doc comment.
//
//go:linkname Float32bits

// Float32bits returns the IEEE 754 binary representation of f,
// with the sign bit of f and the result in the same bit position.
// Float32bits(Float32frombits(x)) == x.
public static uint32 Float32bits(float32 f) { return bitcast<float32, uint32>(f); }

// Float32frombits returns the floating-point number corresponding
// to the IEEE 754 binary representation b, with the sign bit of b
// and the result in the same bit position.
// Float32frombits(Float32bits(x)) == x.
public static float32 Float32frombits(uint32 b) { return bitcast<uint32, float32>(b); }

// Float64bits returns the IEEE 754 binary representation of f,
// with the sign bit of f and the result in the same bit position,
// and Float64bits(Float64frombits(x)) == x.
public static uint64 Float64bits(float64 f) { return bitcast<float64, uint64>(f); }

// Float64frombits returns the floating-point number corresponding
// to the IEEE 754 binary representation b, with the sign bit of b
// and the result in the same bit position.
// Float64frombits(Float64bits(x)) == x.
public static float64 Float64frombits(uint64 b) { return bitcast<uint64, float64>(b); }

} // end math_package
