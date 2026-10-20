// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using @unsafe = unsafe_package;

partial class strconv_package {

// Implementations to avoid importing other dependencies.
// package math
internal static float64 float64frombits(uint64 b) {
    return bitcast<uint64, float64>(b);
}

internal static float32 float32frombits(uint32 b) {
    return bitcast<uint32, float32>(b);
}

internal static uint64 float64bits(float64 f) {
    return bitcast<float64, uint64>(f);
}

internal static uint32 float32bits(float32 f) {
    return bitcast<float32, uint32>(f);
}

internal static float64 inf(nint sign) {
    uint64 v = default!;
    if (sign >= 0){
        v = 0x7FF0000000000000UL;
    } else {
        v = 0xFFF0000000000000UL;
    }
    return float64frombits(v);
}

internal static bool /*is*/ isNaN(float64 f) {
    return f != f;
}

internal static float64 nan() {
    return float64frombits(0x7FF8000000000001UL);
}

} // end strconv_package
