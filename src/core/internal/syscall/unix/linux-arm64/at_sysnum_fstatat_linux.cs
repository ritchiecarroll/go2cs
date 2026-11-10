// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build arm64 || riscv64
namespace go.@internal.syscall;

using syscall = syscall_package;

partial class unix_package {

internal static uintptr fstatatTrap => /* syscall.SYS_FSTATAT */ 79;

} // end unix_package
