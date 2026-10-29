// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (unix && !linux) || windows
namespace go;

using bytealg = @internal.bytealg_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

internal static error joinIPv4Group(ж<netFD> fd, ж<Interface> ifi, IP ip) {
    var mreq = Ꮡ(new syscall.IPMreq(Multiaddr: new byte[]{ip[0], ip[1], ip[2], ip[3]}.array()));
    {
        var errΔ1 = setIPv4MreqToInterface(ref (mreq).DerefOrNull(), ifi); if (errΔ1 != nil) {
            return errΔ1;
        }
    }
    var err = fd.of(netFD.Ꮡpfd).SetsockoptIPMreq(syscall.IPPROTO_IP, syscall.IP_ADD_MEMBERSHIP, mreq);
    runtime.KeepAlive(fd.OrTypedNil());
    return wrapSyscallError(setsockoptˢ, err);
}

internal static error setIPv4MreqToInterface(ref syscall.IPMreq mreq, ж<Interface> ifi) {
    if (ifi == nil) {
        return default;
    }
    var (ifat, err) = ifi.Addrs();
    if (err != nil) {
        return err;
    }
    foreach (var (_, ifa) in ifat) {
        switch (ifa.type()) {
        case ж<IPAddr> v: {
            {
                var a = v.IP.To4(); if (a != nil) {
                    copy(mreq.Interface[..], a);
                    goto done;
                }
            }
            break;
        }
        case ж<IPNet> v: {
            {
                var a = v.IP.To4(); if (a != nil) {
                    copy(mreq.Interface[..], a);
                    goto done;
                }
            }
            break;
        }}
    }
done:
    if (bytealg.Equal(mreq.Interface[..], IPv4zero.To4())) {
        return errNoSuchMulticastInterface;
    }
    return default;
}

} // end net_package
