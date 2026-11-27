// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !amd64 && !s390x
namespace go;

partial class math_package {

internal const bool haveArchLog = false;

internal static float64 archLog(float64 x) {
    throw panic("not implemented");
}

} // end math_package
