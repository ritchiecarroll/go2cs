// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64
namespace go;

using cpu = @internal.cpu_package;
using @internal;

partial class math_package {

internal static bool useFMA = cpu.X86.HasAVX && cpu.X86.HasFMA;

} // end math_package
