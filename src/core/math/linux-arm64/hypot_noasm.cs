// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !386 && !amd64
namespace go;

partial class math_package {

internal const bool haveArchHypot = false;

internal static float64 archHypot(float64 p, float64 q) {
    throw panic("not implemented");
}

} // end math_package
