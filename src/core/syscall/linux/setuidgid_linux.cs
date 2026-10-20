// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux && !386 && !arm
namespace go;

partial class syscall_package {

internal static UntypedInt sys_GETEUID => /* SYS_GETEUID */ 107;

internal static UntypedInt sys_SETGID => /* SYS_SETGID */ 106;
internal static UntypedInt sys_SETUID => /* SYS_SETUID */ 105;

internal static UntypedInt sys_SETREGID => /* SYS_SETREGID */ 114;
internal static UntypedInt sys_SETREUID => /* SYS_SETREUID */ 113;

internal static UntypedInt sys_SETRESGID => /* SYS_SETRESGID */ 119;
internal static UntypedInt sys_SETRESUID => /* SYS_SETRESUID */ 117;

} // end syscall_package
