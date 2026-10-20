// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux && !(loong64 || riscv64)
namespace go.@internal.syscall;

using syscall = syscall_package;

partial class unix_package {

internal static uintptr renameatTrap => /* syscall.SYS_RENAMEAT */ 264;

} // end unix_package
