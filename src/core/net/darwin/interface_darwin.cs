// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using routebsd = @internal.routebsd_package;
using syscall = syscall_package;
using @internal;
using net;

partial class net_package {

internal static (slice<routebsd.Message>, error) interfaceMessages(nint ifindex) {
    return routebsd.FetchRIBMessages(syscall.NET_RT_IFLIST, ifindex);
}

// interfaceMulticastAddrTable returns addresses for a specific
// interface.
internal static (slice<ΔAddr>, error) interfaceMulticastAddrTable(ref Interface ifi) {
    var (msgs, err) = routebsd.FetchRIBMessages(syscall.NET_RT_IFLIST2, ifi.Index);
    if (err != nil) {
        return (default, err);
    }
    var ifmat = new slice<ΔAddr>(0, len(msgs));
    foreach (var (_, m) in msgs) {
        switch (m.type()) {
        case ж<routebsd.InterfaceMulticastAddrMessage> mΔ1: {
            if (ifi.Index != mΔ1.Index) {
                continue;
            }
            IP ip = default;
            switch (mΔ1.Addrs[syscall.RTAX_IFA].type()) {
            case ж<routebsd.InetAddr> sa: {
                if (sa.IP.Is4()) {
                    var a = sa.IP.As4();
                    ip = IPv4(a[0], a[1], a[2], a[3]);
                } else if (sa.IP.Is6()) {
                    var a = sa.IP.As16();
                    ip = new IP(IPv6len);
                    copy(ip, a[..]);
                }
                break;
            }}
            if (ip != nil) {
                ifmat = append(ifmat, (ΔAddr)(new IPAddrжΔAddr(Ꮡ(new IPAddr(IP: ip)))));
            }
            break;
        }}
    }
    return (ifmat, default);
}

} // end net_package
