// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || dragonfly || freebsd || netbsd || openbsd || solaris
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

internal static error setIPv4MulticastInterface(ж<netFD> Ꮡfd, ж<Interface> Ꮡifi) {
    var (ip, err) = interfaceToIPv4Addr(Ꮡifi);
    if (err != default!) {
        return wrapSyscallError(setsockoptˢ, err);
    }
    array<byte> a = new(4);
    copy(a[..], ip.To4());
    err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInet4Addr(syscall.IPPROTO_IP, syscall.IP_MULTICAST_IF, a);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setIPv4MulticastLoopback(ж<netFD> Ꮡfd, bool v) {
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptByte(syscall.IPPROTO_IP, syscall.IP_MULTICAST_LOOP, (byte)boolint(v));
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

} // end net_package
