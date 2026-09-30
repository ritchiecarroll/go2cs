// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go;

using syscall = syscall_package;
using route = vendor.golang.org.x.net.route_package;
using vendor.golang.org.x.net;

partial class net_package {

// If the ifindex is zero, interfaceTable returns mappings of all
// network interfaces. Otherwise it returns a mapping of a specific
// interface.
internal static (slice<Interface>, error) interfaceTable(nint ifindex) {
    var (msgs, err) = interfaceMessages(ifindex);
    if (err != default!) {
        return (default!, err);
    }
    nint n = len(msgs);
    if (ifindex != 0) {
        n = 1;
    }
    var ift = new slice<Interface>(n);
    n = 0;
    foreach (var (_, m) in msgs) {
        switch (m.type()) {
        case ж<route.InterfaceMessage> mΔ1: {
            if (ifindex != 0 && ifindex != (~mΔ1).Index) {
                continue;
            }
            ift[n].Index = mΔ1.Value.Index;
            ift[n].Name = mΔ1.Value.Name;
            ift[n].Flags = linkFlags((~mΔ1).Flags);
            {
                var (sa, ok) = (~mΔ1).Addrs[syscall.RTAX_IFP]._<ж<route.LinkAddr>>(ᐧ); if (ok && len((~sa).Addr) > 0) {
                    ift[n].HardwareAddr = new slice<byte>(len((~sa).Addr));
                    copy(ift[n].HardwareAddr, (~sa).Addr);
                }
            }
            foreach (var (_, sys) in mΔ1.Sys()) {
                {
                    var (imx, ok) = sys._<ж<route.InterfaceMetrics>>(ᐧ); if (ok) {
                        ift[n].MTU = imx.Value.MTU;
                        break;
                    }
                }
            }
            n++;
            if (ifindex == (~mΔ1).Index) {
                return (ift.slice(0, n), default!);
            }
            break;
        }}
    }
    return (ift.slice(0, n), default!);
}

internal static Flags linkFlags(nint rawFlags) {
    Flags f = default!;
    if ((nint)(rawFlags & (nint)syscall.IFF_UP) != 0) {
        f |= (Flags)(FlagUp);
    }
    if ((nint)(rawFlags & (nint)syscall.IFF_RUNNING) != 0) {
        f |= (Flags)(FlagRunning);
    }
    if ((nint)(rawFlags & (nint)syscall.IFF_BROADCAST) != 0) {
        f |= (Flags)(FlagBroadcast);
    }
    if ((nint)(rawFlags & (nint)syscall.IFF_LOOPBACK) != 0) {
        f |= (Flags)(FlagLoopback);
    }
    if ((nint)(rawFlags & (nint)syscall.IFF_POINTOPOINT) != 0) {
        f |= (Flags)(FlagPointToPoint);
    }
    if ((nint)(rawFlags & (nint)syscall.IFF_MULTICAST) != 0) {
        f |= (Flags)(FlagMulticast);
    }
    return f;
}

// If the ifi is nil, interfaceAddrTable returns addresses for all
// network interfaces. Otherwise it returns addresses for a specific
// interface.
internal static (slice<ΔAddr>, error) interfaceAddrTable(ж<Interface> Ꮡifi) {
    ref var ifi = ref Ꮡifi.DerefOrNull();

    nint index = 0;
    if (Ꮡifi != nil) {
        index = ifi.Index;
    }
    var (msgs, err) = interfaceMessages(index);
    if (err != default!) {
        return (default!, err);
    }
    var ifat = new slice<ΔAddr>(0, len(msgs));
    foreach (var (_, m) in msgs) {
        switch (m.type()) {
        case ж<route.InterfaceAddrMessage> mΔ1: {
            if (index != 0 && index != (~mΔ1).Index) {
                continue;
            }
            IPMask mask = default!;
            switch ((~mΔ1).Addrs[syscall.RTAX_NETMASK].type()) {
            case ж<route.Inet4Addr> sa: {
                mask = IPv4Mask((~sa).IP[0], (~sa).IP[1], (~sa).IP[2], (~sa).IP[3]);
                break;
            }
            case ж<route.Inet6Addr> sa: {
                mask = new IPMask(IPv6len);
                copy(mask, (~sa).IP[..]);
                break;
            }}
            IP ip = default!;
            switch ((~mΔ1).Addrs[syscall.RTAX_IFA].type()) {
            case ж<route.Inet4Addr> sa: {
                ip = IPv4((~sa).IP[0], (~sa).IP[1], (~sa).IP[2], (~sa).IP[3]);
                break;
            }
            case ж<route.Inet6Addr> sa: {
                ip = new IP(IPv6len);
                copy(ip, (~sa).IP[..]);
                break;
            }}
            if (ip != default! && mask != default!) {
                // NetBSD may contain route.LinkAddr
                ifat = append(ifat, (ΔAddr)(new IPNetжΔAddr(Ꮡ(new IPNet(IP: ip, Mask: mask)))));
            }
            break;
        }}
    }
    return (ifat, default!);
}

} // end net_package
