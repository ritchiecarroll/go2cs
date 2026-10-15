// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

// Boolean to int.
internal static nint boolint(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

internal static (IP, error) interfaceToIPv4Addr(ж<Interface> Ꮡifi) {
    if (Ꮡifi == nil) {
        return (IPv4zero, default!);
    }
    var (ifat, err) = Ꮡifi.Addrs();
    if (err != default!) {
        return (default!, err);
    }
    foreach (var (_, ifa) in ifat) {
        switch (ifa.type()) {
        case ж<IPAddr> v: {
            if ((~v).IP.To4() != default!) {
                return ((~v).IP, default!);
            }
            break;
        }
        case ж<IPNet> v: {
            if ((~v).IP.To4() != default!) {
                return ((~v).IP, default!);
            }
            break;
        }}
    }
    return (default!, errNoSuchInterface);
}

internal static error setReadBuffer(ж<netFD> Ꮡfd, nint bytes) {
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_RCVBUF, bytes);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setWriteBuffer(ж<netFD> Ꮡfd, nint bytes) {
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_SNDBUF, bytes);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setKeepAlive(ж<netFD> Ꮡfd, bool keepalive) {
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.SOL_SOCKET, syscall.SO_KEEPALIVE, boolint(keepalive));
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setLinger(ж<netFD> Ꮡfd, nint sec) {
    ref var l = ref heap(new syscall.Linger(), out var Ꮡl);
    if (sec >= 0){
        l.Onoff = 1;
        l.ΔLinger = (int32)sec;
    } else {
        l.Onoff = 0;
        l.ΔLinger = 0;
    }
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptLinger(syscall.SOL_SOCKET, syscall.SO_LINGER, Ꮡl);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

} // end net_package
