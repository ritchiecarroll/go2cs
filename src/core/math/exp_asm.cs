// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 || arm64 || loong64 || s390x
namespace go;

partial class math_package {

internal const bool haveArchExp = true;

internal static partial float64 archExp(float64 x);

} // end math_package
