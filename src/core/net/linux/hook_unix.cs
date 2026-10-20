// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1
namespace go;

using syscall = syscall_package;

partial class net_package {

internal static Action testHookCanceledDial = () => {
}; // for golang.org/issue/16523

internal static @string hostsFilePath = "/etc/hosts"u8;

// Placeholders for socket system calls.
internal static Func<nint, nint, nint, (nint, error)> socketFunc = syscall.Socket;
internal static Func<nint, syscall.Sockaddr, error> connectFunc = syscall.Connect;
internal static Func<nint, nint, error> listenFunc = syscall.Listen;
internal static Func<nint, nint, nint, (nint, error)> getsockoptIntFunc = syscall.GetsockoptInt;

} // end net_package
