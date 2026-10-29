// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using os = os_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;

partial class net_package {

// Default values of KeepAliveTime and KeepAliveInterval on Windows,
// check out https://learn.microsoft.com/en-us/windows/win32/winsock/sio-keepalive-vals#remarks for details.
internal static time.Duration defaultKeepAliveIdle => /* 2 * time.Hour */ 7200000000000;
internal static time.Duration defaultKeepAliveInterval => /* time.Second */ 1000000000;

internal static error setKeepAliveIdle(ж<netFD> fd, time.Duration d) {
    if (!windows.SupportTCPKeepAliveIdle()) {
        return setKeepAliveIdleAndInterval(fd, d, -1);
    }

    if (d == 0) {
        d = defaultTCPKeepAliveIdle;
    } else if (d < 0) {
        return default;
    }
    // The kernel expects seconds so round to next highest second.
    nint secs = (nint)roundDurationUp(d, time.ΔSecond);
    var err = fd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_TCP, windows.TCP_KEEPIDLE, secs);
    runtime.KeepAlive(fd.OrTypedNil());
    return os.NewSyscallError(setsockoptˢ, err);
}

internal static error setKeepAliveInterval(ж<netFD> fd, time.Duration d) {
    if (!windows.SupportTCPKeepAliveInterval()) {
        return setKeepAliveIdleAndInterval(fd, -1, d);
    }

    if (d == 0) {
        d = defaultTCPKeepAliveInterval;
    } else if (d < 0) {
        return default;
    }
    // The kernel expects seconds so round to next highest second.
    nint secs = (nint)roundDurationUp(d, time.ΔSecond);
    var err = fd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_TCP, windows.TCP_KEEPINTVL, secs);
    runtime.KeepAlive(fd.OrTypedNil());
    return os.NewSyscallError(setsockoptˢ, err);
}

internal static error setKeepAliveCount(ж<netFD> fd, nint n) {
    if (n == 0) {
        n = defaultTCPKeepAliveCount;
    } else if (n < 0) {
        return default;
    }

    var err = fd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_TCP, windows.TCP_KEEPCNT, n);
    runtime.KeepAlive(fd.OrTypedNil());
    return os.NewSyscallError(setsockoptˢ, err);
}

// setKeepAliveIdleAndInterval serves for kernels prior to Windows 10, version 1709.
internal static error setKeepAliveIdleAndInterval(ж<netFD> fd, time.Duration idle, time.Duration interval) {
    // WSAIoctl with SIO_KEEPALIVE_VALS control code requires all fields in
    // `tcp_keepalive` struct to be provided.
    // Otherwise, if any of the fields were not provided, just leaving them
    // zero will knock off any existing values of keep-alive.
    // Unfortunately, Windows doesn't support retrieving current keep-alive
    // settings in any form programmatically, which disable us to first retrieve
    // the current keep-alive settings, then set it without unwanted corruption.
    switch (ᐧ) {
    case {} when idle < 0 && interval >= 0: {
        // Given that we can't set KeepAliveInterval alone, and this code path
        // is new, it doesn't exist before, so we just return an error.
        return syscall.WSAENOPROTOOPT;
    }
    case {} when idle >= 0 && interval < 0: {
        // Although we can't set KeepAliveTime alone either, this existing code
        // path had been backing up [SetKeepAlivePeriod] which used to be set both
        // KeepAliveTime and KeepAliveInterval to 15 seconds.
        // Now we will use the default of KeepAliveInterval on Windows if user doesn't
        // provide one.
        interval = defaultKeepAliveInterval;
        break;
    }
    case {} when idle < 0 && interval < 0: {
        // Nothing to do, just bail out.
        return default;
    }
    case {} when idle >= 0 && interval >= 0: {
        // Go ahead.
        break;
    }}

    if (idle == 0) {
        idle = defaultTCPKeepAliveIdle;
    }
    if (interval == 0) {
        interval = defaultTCPKeepAliveInterval;
    }

    // The kernel expects milliseconds so round to next highest
    // millisecond.
    var tcpKeepAliveIdle = (uint32)roundDurationUp(idle, time.Millisecond);
    var tcpKeepAliveInterval = (uint32)roundDurationUp(interval, time.Millisecond);
    ref var ka = ref heap<syscall.TCPKeepalive>(new syscall.TCPKeepalive(
        OnOff: 1,
        Time: tcpKeepAliveIdle,
        Interval: tcpKeepAliveInterval
    ), out var Ꮡka);
    ref var ret = ref heap<uint32>((uint32)0, out var Ꮡret);
    var size = (uint32)/* unsafe.Sizeof(ka) */ (uintptr)12;
    var err = fd.of(netFD.Ꮡpfd).WSAIoctl(syscall.SIO_KEEPALIVE_VALS, Ꮡka.Reinterpret<syscall.TCPKeepalive, byte>(), size, nil, 0, Ꮡret, nil, 0);
    runtime.KeepAlive(fd.OrTypedNil());
    return os.NewSyscallError(wsaioctlˢ, err);
}

} // end net_package
