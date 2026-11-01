// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || freebsd || netbsd || openbsd
namespace go.@internal;

using syscall = syscall_package;

partial class sysinfo_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string machdepCpuBrandStringˢ => ref machdepCpuBrandStringˢᵇ;
private static readonly @string machdepCpuBrandStringˢᵇ = "machdep.cpu.brand_string"u8;

internal static @string osCPUInfoName() {
    var (cpu, _) = syscall.Sysctl(machdepCpuBrandStringˢ);
    return cpu;
}

} // end sysinfo_package
