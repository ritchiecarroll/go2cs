// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux && !386 && !arm
namespace go;

partial class syscall_package {

internal static UntypedInt sys_GETEUID => /* SYS_GETEUID */ 175;

internal static UntypedInt sys_SETGID => /* SYS_SETGID */ 144;
internal static UntypedInt sys_SETUID => /* SYS_SETUID */ 146;

internal static UntypedInt sys_SETREGID => /* SYS_SETREGID */ 143;
internal static UntypedInt sys_SETREUID => /* SYS_SETREUID */ 145;

internal static UntypedInt sys_SETRESGID => /* SYS_SETRESGID */ 149;
internal static UntypedInt sys_SETRESUID => /* SYS_SETRESUID */ 147;

} // end syscall_package
