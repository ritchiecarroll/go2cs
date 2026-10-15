// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !ppc64 && !ppc64le && !riscv64 && !s390x
namespace go;

using @unsafe = unsafe_package;

partial class reflect_package {

// This file implements a straightforward conversion of a float32
// value into its representation in a register. This conversion
// applies for amd64 and arm64. It is also chosen for the case of
// zero argument registers, but is not used.
internal static float32 archFloat32FromReg(uint64 reg) {
    var i = (uint32)reg;
    return bitcast<uint32, float32>(i);
}

internal static uint64 archFloat32ToReg(float32 val) {
    return (uint64)(bitcast<float32, uint32>(val));
}

} // end reflect_package
