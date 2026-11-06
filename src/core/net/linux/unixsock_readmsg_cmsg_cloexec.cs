// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build dragonfly || linux || netbsd || openbsd
namespace go;

using syscall = syscall_package;

partial class net_package {

internal static UntypedInt readMsgFlags => /* syscall.MSG_CMSG_CLOEXEC */ 1073741824;

internal static void setReadMsgCloseOnExec(slice<byte> oob) { }

} // end net_package
