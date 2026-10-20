// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements sysSocket for platforms that provide a fast path for
// setting SetNonblock and CloseOnExec.
//go:build dragonfly || freebsd || linux || netbsd || openbsd
namespace go;

using os = os_package;
using syscall = syscall_package;

partial class net_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string socketˢ = "socket"u8;

// Wrapper around the socket system call that marks the returned file
// descriptor as nonblocking and close-on-exec.
internal static (nint, error) sysSocket(nint family, nint sotype, nint proto) {
    var (s, err) = socketFunc(family, sotype | (nint)syscall.SOCK_NONBLOCK | (nint)syscall.SOCK_CLOEXEC, proto);
    if (err != default!) {
        return (-1, os.NewSyscallError(socketˢ, err));
    }
    return (s, default!);
}

} // end net_package
