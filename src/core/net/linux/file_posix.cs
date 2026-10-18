// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go;

using poll = @internal.poll_package;
using os = os_package;
using syscall = syscall_package;
using @internal;
using time = time_package;

partial class net_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getsocknameˢ = "getsockname"u8;

internal static (ж<netFD>, error) newFileFD(ж<os.File> Ꮡf) {
    var (s, err) = dupFileSocket(Ꮡf);
    if (err != default!) {
        return (default!, err);
    }
    nint family = syscall.AF_UNSPEC;
    (var sotype, err) = syscall.GetsockoptInt(s, syscall.SOL_SOCKET, _SO_TYPE);
    if (err != default!) {
        poll.CloseFunc(s);
        return (default!, os.NewSyscallError(getsockoptˢ, err));
    }
    (var lsa, err) = syscall.Getsockname(s);
    if (err != default!) {
        poll.CloseFunc(s);
        return (default!, os.NewSyscallError(getsocknameˢ, err));
    }
    var (rsa, _) = syscall.Getpeername(s);
    switch (lsa.type()) {
    case ж<syscall.SockaddrInet4>: {
        family = syscall.AF_INET;
        break;
    }
    case ж<syscall.SockaddrInet6>: {
        family = syscall.AF_INET6;
        break;
    }
    case ж<syscall.SockaddrUnix>: {
        family = syscall.AF_UNIX;
        break;
    }
    default: {
        poll.CloseFunc(s);
        return (default!, syscall.EPROTONOSUPPORT);
    }}
    (var fd, err) = newFD(s, family, sotype, ""u8);
    if (err != default!) {
        poll.CloseFunc(s);
        return (default!, err);
    }
    var laddr = fd.addrFunc()(lsa);
    var raddr = fd.addrFunc()(rsa);
    fd.Value.net = laddr.Network();
    {
        var errΔ1 = fd.init(); if (errΔ1 != default!) {
            fd.Close();
            return (default!, errΔ1);
        }
    }
    fd.setAddr(laddr, raddr);
    return (fd, default!);
}

internal static (Conn, error) fileConn(ж<os.File> Ꮡf) {
    var (fd, err) = newFileFD(Ꮡf);
    if (err != default!) {
        return (default!, err);
    }
    switch ((~fd).laddr.type()) {
    case ж<TCPAddr>: {
        return (new TCPConnжConn(newTCPConn(fd, defaultTCPKeepAliveIdle, new KeepAliveConfig(nil), testPreHookSetKeepAlive, testHookSetKeepAlive)), default!);
    }
    case ж<UDPAddr>: {
        return (new UDPConnжConn(newUDPConn(fd)), default!);
    }
    case ж<IPAddr>: {
        return (new IPConnжConn(newIPConn(fd)), default!);
    }
    case ж<UnixAddr>: {
        return (new UnixConnжConn(newUnixConn(fd)), default!);
    }}
    fd.Close();
    return (default!, syscall.EINVAL);
}

internal static (Listener, error) fileListener(ж<os.File> Ꮡf) {
    var (fd, err) = newFileFD(Ꮡf);
    if (err != default!) {
        return (default!, err);
    }
    switch ((~fd).laddr.type()) {
    case ж<TCPAddr> laddr: {
        return (new TCPListenerжListener(Ꮡ(new TCPListener(fd: fd))), default!);
    }
    case ж<UnixAddr> laddr: {
        return (new UnixListenerжListener(Ꮡ(new UnixListener(fd: fd, path: (~laddr).Name, unlink: false))), default!);
    }}
    fd.Close();
    return (default!, syscall.EINVAL);
}

internal static (PacketConn, error) filePacketConn(ж<os.File> Ꮡf) {
    var (fd, err) = newFileFD(Ꮡf);
    if (err != default!) {
        return (default!, err);
    }
    switch ((~fd).laddr.type()) {
    case ж<UDPAddr>: {
        return (new UDPConnжPacketConn(newUDPConn(fd)), default!);
    }
    case ж<IPAddr>: {
        return (new IPConnжPacketConn(newIPConn(fd)), default!);
    }
    case ж<UnixAddr>: {
        return (new UnixConnжPacketConn(newUnixConn(fd)), default!);
    }}
    fd.Close();
    return (default!, syscall.EINVAL);
}

} // end net_package
