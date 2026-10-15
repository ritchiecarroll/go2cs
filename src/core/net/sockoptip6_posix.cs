// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

internal static error setIPv6MulticastInterface(ж<netFD> Ꮡfd, ж<Interface> Ꮡifi) {
    ref var ifi = ref Ꮡifi.DerefOrNull();

    nint v = default!;
    if (Ꮡifi != nil) {
        v = ifi.Index;
    }
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_IPV6, syscall.IPV6_MULTICAST_IF, v);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setIPv6MulticastLoopback(ж<netFD> Ꮡfd, bool v) {
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_IPV6, syscall.IPV6_MULTICAST_LOOP, boolint(v));
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error joinIPv6Group(ж<netFD> Ꮡfd, ж<Interface> Ꮡifi, IP ip) {
    ref var ifi = ref Ꮡifi.DerefOrNull();

    var mreq = Ꮡ(new syscall.IPv6Mreq(nil));
    copy((~mreq).Multiaddr[..], ip);
    if (Ꮡifi != nil) {
        mreq.Value.Interface = (uint32)ifi.Index;
    }
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptIPv6Mreq(syscall.IPPROTO_IPV6, syscall.IPV6_JOIN_GROUP, mreq);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

} // end net_package
