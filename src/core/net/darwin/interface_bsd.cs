// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go;

using routebsd = @internal.routebsd_package;
using syscall = syscall_package;
using @internal;
using net;

partial class net_package {

// If the ifindex is zero, interfaceTable returns mappings of all
// network interfaces. Otherwise it returns a mapping of a specific
// interface.
internal static (slice<Interface>, error) interfaceTable(nint ifindex) {
    var (msgs, err) = interfaceMessages(ifindex);
    if (err != nil) {
        return (default, err);
    }
    nint n = len(msgs);
    if (ifindex != 0) {
        n = 1;
    }
    var ift = new slice<Interface>(n);
    n = 0;
    foreach (var (_, m) in msgs) {
        switch (m.type()) {
        case ж<routebsd.InterfaceMessage> mΔ1: {
            if (ifindex != 0 && ifindex != mΔ1.Index) {
                continue;
            }
            ift[n].Index = mΔ1.Index;
            ift[n].Name = mΔ1.Name;
            ift[n].Flags = linkFlags(mΔ1.Flags);
            {
                var (sa, ok) = mΔ1.Addrs[syscall.RTAX_IFP]._<ж<routebsd.LinkAddr>>(ᐧ); if (ok && len(sa.Addr) > 0) {
                    ift[n].HardwareAddr = new slice<byte>(len(sa.Addr));
                    copy(ift[n].HardwareAddr, sa.Addr);
                }
            }
            ift[n].MTU = mΔ1.MTU();
            n++;
            if (ifindex == mΔ1.Index) {
                return (ift.slice(0, n), default);
            }
            break;
        }}
    }
    return (ift.slice(0, n), default);
}

internal static Flags linkFlags(nint rawFlags) {
    Flags f = default;
    if ((rawFlags & (nint)syscall.IFF_UP) != 0) {
        f |= (Flags)(FlagUp);
    }
    if ((rawFlags & (nint)syscall.IFF_RUNNING) != 0) {
        f |= (Flags)(FlagRunning);
    }
    if ((rawFlags & (nint)syscall.IFF_BROADCAST) != 0) {
        f |= (Flags)(FlagBroadcast);
    }
    if ((rawFlags & (nint)syscall.IFF_LOOPBACK) != 0) {
        f |= (Flags)(FlagLoopback);
    }
    if ((rawFlags & (nint)syscall.IFF_POINTOPOINT) != 0) {
        f |= (Flags)(FlagPointToPoint);
    }
    if ((rawFlags & (nint)syscall.IFF_MULTICAST) != 0) {
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
    if (err != nil) {
        return (default, err);
    }
    var ifat = new slice<ΔAddr>(0, len(msgs));
    foreach (var (_, m) in msgs) {
        switch (m.type()) {
        case ж<routebsd.InterfaceAddrMessage> mΔ1: {
            if (index != 0 && index != mΔ1.Index) {
                continue;
            }
            IPMask mask = default;
            switch (mΔ1.Addrs[syscall.RTAX_NETMASK].type()) {
            case ж<routebsd.InetAddr> sa: {
                if (sa.IP.Is4()) {
                    var a = sa.IP.As4();
                    mask = IPv4Mask(a[0], a[1], a[2], a[3]);
                } else if (sa.IP.Is6()) {
                    var a = sa.IP.As16();
                    mask = new IPMask(IPv6len);
                    copy(mask, a[..]);
                }
                break;
            }}
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
            if (ip != nil && mask != nil) { // NetBSD may contain routebsd.LinkAddr
                ifat = append(ifat, (ΔAddr)(new IPNetжΔAddr(Ꮡ(new IPNet(IP: ip, Mask: mask)))));
            }
            break;
        }}
    }
    return (ifat, default);
}

} // end net_package
