// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

// fastlog2 implements a fast approximation to the base 2 log of a
// float64. This is used to compute a geometric distribution for heap
// sampling, without introducing dependencies into package math. This
// uses a very rough approximation using the float64 exponent and the
// first 25 bits of the mantissa. The top 5 bits of the mantissa are
// used to load limits from a table of constants and the rest are used
// to scale linearly between them.
internal static float64 fastlog2(float64 x) {
    UntypedInt fastlogScaleBits = 20;
    const float64 fastlogScaleRatio = /* 1.0 / (1 << fastlogScaleBits) */ 9.5367431640625e-07;

    var xBits = float64bits(x);
    // Extract the exponent from the IEEE float64, and index a constant
    // table with the first 10 bits from the mantissa.
    var xExp = (int64)((xBits >> 52) & 0x7FF) - 1023;
    var xManIndex = (xBits >> (int)((52 - fastlogNumBits))) % (uint64)((uint64)1 << (int)(fastlogNumBits));
    var xManScale = (xBits >> (int)((52 - fastlogNumBits - fastlogScaleBits))) % (uint64)((uint64)1 << (int)(fastlogScaleBits));

    var (low, high) = (fastlog2Table[xManIndex], fastlog2Table[xManIndex + 1]);
    return (float64)xExp + low + (high - low) * (float64)xManScale * fastlogScaleRatio;
}

} // end runtime_package
