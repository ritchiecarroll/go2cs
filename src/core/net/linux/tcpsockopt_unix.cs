// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || dragonfly || freebsd || illumos || linux || netbsd
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @internal;

partial class net_package {

internal static error setKeepAliveIdle(ж<netFD> fd, time.Duration d) {
    if (d == 0) {
        d = defaultTCPKeepAliveIdle;
    } else if (d < 0) {
        return default;
    }

    // The kernel expects seconds so round to next highest second.
    nint secs = (nint)roundDurationUp(d, time.ΔSecond);
    var err = fd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_TCP, syscall.TCP_KEEPIDLE, secs);
    runtime.KeepAlive(fd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setKeepAliveInterval(ж<netFD> fd, time.Duration d) {
    if (d == 0) {
        d = defaultTCPKeepAliveInterval;
    } else if (d < 0) {
        return default;
    }

    // The kernel expects seconds so round to next highest second.
    nint secs = (nint)roundDurationUp(d, time.ΔSecond);
    var err = fd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_TCP, syscall.TCP_KEEPINTVL, secs);
    runtime.KeepAlive(fd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setKeepAliveCount(ж<netFD> fd, nint n) {
    if (n == 0) {
        n = defaultTCPKeepAliveCount;
    } else if (n < 0) {
        return default;
    }

    var err = fd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_TCP, syscall.TCP_KEEPCNT, n);
    runtime.KeepAlive(fd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

} // end net_package
