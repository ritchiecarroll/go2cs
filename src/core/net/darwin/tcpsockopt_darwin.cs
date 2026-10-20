// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @internal;

partial class net_package {

// syscall.TCP_KEEPINTVL and syscall.TCP_KEEPCNT might be missing on some darwin architectures.
internal static UntypedInt sysTCP_KEEPINTVL => 0x101;
internal static UntypedInt sysTCP_KEEPCNT => 0x102;

internal static error setKeepAliveIdle(ж<netFD> Ꮡfd, time.Duration d) {
    if (d == 0){
        d = defaultTCPKeepAliveIdle;
    } else 
    if (d < 0) {
        return default!;
    }

    // The kernel expects seconds so round to next highest second.
    nint secs = (nint)(int64)roundDurationUp(d, time.ΔSecond);
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_TCP, syscall.TCP_KEEPALIVE, secs);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setKeepAliveInterval(ж<netFD> Ꮡfd, time.Duration d) {
    if (d == 0){
        d = defaultTCPKeepAliveInterval;
    } else 
    if (d < 0) {
        return default!;
    }

    // The kernel expects seconds so round to next highest second.
    nint secs = (nint)(int64)roundDurationUp(d, time.ΔSecond);
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_TCP, sysTCP_KEEPINTVL, secs);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setKeepAliveCount(ж<netFD> Ꮡfd, nint n) {
    if (n == 0){
        n = defaultTCPKeepAliveCount;
    } else 
    if (n < 0) {
        return default!;
    }

    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_TCP, sysTCP_KEEPCNT, n);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

} // end net_package
