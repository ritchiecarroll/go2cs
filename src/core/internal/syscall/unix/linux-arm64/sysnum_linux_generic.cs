// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux && (arm64 || loong64 || riscv64)
namespace go.@internal.syscall;

partial class unix_package {

// This file is named "generic" because at a certain point Linux started
// standardizing on system call numbers across architectures. So far this
// means only arm64 loong64 and riscv64 use the standard numbers.
internal static uintptr getrandomTrap => 278;
internal static uintptr copyFileRangeTrap => 285;
internal static uintptr pidfdSendSignalTrap => 424;
internal static uintptr pidfdOpenTrap => 434;
internal static uintptr openat2Trap => 437;

} // end unix_package
