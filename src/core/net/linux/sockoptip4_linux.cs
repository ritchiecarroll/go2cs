// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

internal static error joinIPv4Group(ж<netFD> Ꮡfd, ж<Interface> Ꮡifi, IP ip) {
    ref var ifi = ref Ꮡifi.DerefOrNull();

    var mreq = Ꮡ(new syscall.IPMreqn(Multiaddr: new byte[]{ip[0], ip[1], ip[2], ip[3]}.array()));
    if (Ꮡifi != nil) {
        mreq.Value.Ifindex = (int32)ifi.Index;
    }
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptIPMreqn(syscall.IPPROTO_IP, syscall.IP_ADD_MEMBERSHIP, mreq);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setIPv4MulticastInterface(ж<netFD> Ꮡfd, ж<Interface> Ꮡifi) {
    ref var ifi = ref Ꮡifi.DerefOrNull();

    ref var v = ref heap(new int32(), out var Ꮡv);
    if (Ꮡifi != nil) {
        v = (int32)ifi.Index;
    }
    var mreq = Ꮡ(new syscall.IPMreqn(Ifindex: v));
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptIPMreqn(syscall.IPPROTO_IP, syscall.IP_MULTICAST_IF, mreq);
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setIPv4MulticastLoopback(ж<netFD> Ꮡfd, bool v) {
    var err = Ꮡfd.of(netFD.Ꮡpfd).SetsockoptInt(syscall.IPPROTO_IP, syscall.IP_MULTICAST_LOOP, boolint(v));
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

} // end net_package
