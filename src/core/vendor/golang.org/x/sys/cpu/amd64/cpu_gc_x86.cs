// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (386 || amd64 || amd64p32) && gc
namespace go.vendor.golang.org.x.sys;

partial class cpu_package {

// cpuid is implemented in cpu_gc_x86.s for gc compiler
// and in cpu_gccgo.c for gccgo.
internal static partial (uint32 eax, uint32 ebx, uint32 ecx, uint32 edx) cpuid(uint32 eaxArg, uint32 ecxArg);

// xgetbv with ecx = 0 is implemented in cpu_gc_x86.s for gc compiler
// and in cpu_gccgo.c for gccgo.
internal static partial (uint32 eax, uint32 edx) xgetbv();

} // end cpu_package
