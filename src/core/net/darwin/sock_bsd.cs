// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;

partial class net_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string kernIpcSomaxconnˢ = "kern.ipc.somaxconn"u8;
internal static readonly @string kernIpcSoacceptqueueˢ = "kern.ipc.soacceptqueue"u8;
internal static readonly @string kernSomaxconnˢ = "kern.somaxconn"u8;

internal static nint maxListenerBacklog() {
    uint32 n = default;
    error err = default;
    var exprᴛ1 = runtime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8: {
        (n, err) = syscall.SysctlUint32(kernIpcSomaxconnˢ);
        break;
    }
    case {} when exprᴛ1 == "freebsd"u8: {
        (n, err) = syscall.SysctlUint32(kernIpcSoacceptqueueˢ);
        break;
    }
    case {} when exprᴛ1 == "netbsd"u8: {
        // NOTE: NetBSD has no somaxconn-like kernel state so far
        break;
    }
    case {} when exprᴛ1 == "openbsd"u8: {
        (n, err) = syscall.SysctlUint32(kernSomaxconnˢ);
        break;
    }}
    if (n == 0 || err != nil) {
        return syscall.SOMAXCONN;
    }
    // FreeBSD stores the backlog in a uint16, as does Linux.
    // Assume the other BSDs do too. Truncate number to avoid wrapping.
    // See issue 5030.
    if (n > (1 << 16) - 1) {
        n = (1 << 16) - 1;
    }
    return (nint)n;
}

} // end net_package
