// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements sysSocket for platforms that do not provide a fast path
// for setting SetNonblock and CloseOnExec.
//go:build aix || darwin
namespace go;

using poll = @internal.poll_package;
using os = os_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string socketˢ = "socket"u8;

// Wrapper around the socket system call that marks the returned file
// descriptor as nonblocking and close-on-exec.
internal static (nint, error) sysSocket(nint family, nint sotype, nint proto) {
    // See ../syscall/exec_unix.go for description of ForkLock.
    syscall.ᏑForkLock.RLock();
    var (s, err) = socketFunc(family, sotype, proto);
    if (err == nil) {
        syscall.CloseOnExec(s);
    }
    syscall.ᏑForkLock.RUnlock();
    if (err != nil) {
        return (-1, os.NewSyscallError(socketˢ, err));
    }
    {
        err = syscall.SetNonblock(s, true); if (err != nil) {
            poll.CloseFunc(s);
            return (-1, os.NewSyscallError(setnonblockˢ, err));
        }
    }
    return (s, default);
}

} // end net_package
