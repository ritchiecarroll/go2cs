// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using os = os_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class net_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netlinkribˢ = "netlinkrib"u8;
internal static readonly @string parsenetlinkmessageˢ = "parsenetlinkmessage"u8;
internal static readonly @string parsenetlinkrouteattrˢ = "parsenetlinkrouteattr"u8;

// If the ifindex is zero, interfaceTable returns mappings of all
// network interfaces. Otherwise it returns a mapping of a specific
// interface.
internal static (slice<Interface>, error) interfaceTable(nint ifindex) {
    var (tab, err) = syscall.NetlinkRIB(syscall.RTM_GETLINK, syscall.AF_UNSPEC);
    if (err != default!) {
        return (default!, os.NewSyscallError(netlinkribˢ, err));
    }
    (var msgs, err) = syscall.ParseNetlinkMessage(tab);
    if (err != default!) {
        return (default!, os.NewSyscallError(parsenetlinkmessageˢ, err));
    }
    slice<Interface> ift = default!;
loop:
    foreach (var (_, vᴛ1) in msgs) {
        ref var m = ref heap(new syscall.NetlinkMessage(), out var Ꮡm);
        m = vᴛ1;

        var exprᴛ1 = m.Header.Type;
        if (exprᴛ1 == syscall.NLMSG_DONE) {
            goto break_loop;
        }
        else if (exprᴛ1 == syscall.RTM_NEWLINK) {
            var ifim = Ꮡ(m.Data, 0).Reinterpret<byte, syscall.IfInfomsg>();
            if (ifindex == 0 || ifindex == (nint)(~ifim).Index) {
                var (attrs, errΔ2) = syscall.ParseNetlinkRouteAttr(Ꮡm);
                if (errΔ2 != default!) {
                    return (default!, os.NewSyscallError(parsenetlinkrouteattrˢ, errΔ2));
                }
                ift = append(ift, newLink(ref (ifim).DerefOrNull(), attrs).Value);
                if (ifindex == (nint)(~ifim).Index) {
                    goto break_loop;
                }
            }
        }

continue_loop:;
    }
break_loop:;
    return (ift, default!);
}

internal static UntypedInt sysARPHardwareIPv4IPv4 => 768; // IPv4 over IPv4 tunneling
internal static UntypedInt sysARPHardwareIPv6IPv6 => 769; // IPv6 over IPv6 tunneling
internal static UntypedInt sysARPHardwareIPv6IPv4 => 776; // IPv6 over IPv4 tunneling
internal static UntypedInt sysARPHardwareGREIPv4 => 778; // any over GRE over IPv4 tunneling
internal static UntypedInt sysARPHardwareGREIPv6 => 823; // any over GRE over IPv6 tunneling

internal static ж<Interface> newLink(ref syscall.IfInfomsg ifim, slice<syscall.NetlinkRouteAttr> attrs) {
    var ifi = Ꮡ(new Interface(Index: (nint)ifim.Index, Flags: linkFlags(ifim.Flags)));
    foreach (var (_, a) in attrs) {
        var exprᴛ1 = a.Attr.Type;
        if (exprᴛ1 == syscall.IFLA_ADDRESS) {
            var exprᴛ2 = len(a.Value);
            if (exprᴛ2 == IPv4len) {
                var exprᴛ3 = ifim.Type;
                if (exprᴛ3 == sysARPHardwareIPv4IPv4 || exprᴛ3 == sysARPHardwareGREIPv4 || exprᴛ3 == sysARPHardwareIPv6IPv4) {
                    continue;
                }

            }
            else if (exprᴛ2 == IPv6len) {
                var exprᴛ4 = ifim.Type;
                if (exprᴛ4 == sysARPHardwareIPv6IPv6 || exprᴛ4 == sysARPHardwareGREIPv6) {
                    continue;
                }

            }

// We never return any /32 or /128 IP address
// prefix on any IP tunnel interface as the
// hardware address.
            bool nonzero = default!;
            foreach (var (_, b) in a.Value) {
                if (b != 0) {
                    nonzero = true;
                    break;
                }
            }
            if (nonzero) {
                ifi.Value.HardwareAddr = a.Value[..];
            }
        }
        else if (exprᴛ1 == syscall.IFLA_IFNAME) {
            ifi.Value.Name = ((@string)(a.Value.slice(0, len(a.Value) - 1)));
        }
        else if (exprᴛ1 == syscall.IFLA_MTU) {
            ifi.Value.MTU = (nint)(~Ꮡ(a.Value[..4], 0).Reinterpret<byte, uint32>());
        }

    }
    return ifi;
}

internal static Flags linkFlags(uint32 rawFlags) {
    Flags f = default!;
    if ((uint32)(rawFlags & (uint32)syscall.IFF_UP) != 0) {
        f |= (Flags)(FlagUp);
    }
    if ((uint32)(rawFlags & (uint32)syscall.IFF_RUNNING) != 0) {
        f |= (Flags)(FlagRunning);
    }
    if ((uint32)(rawFlags & (uint32)syscall.IFF_BROADCAST) != 0) {
        f |= (Flags)(FlagBroadcast);
    }
    if ((uint32)(rawFlags & (uint32)syscall.IFF_LOOPBACK) != 0) {
        f |= (Flags)(FlagLoopback);
    }
    if ((uint32)(rawFlags & (uint32)syscall.IFF_POINTOPOINT) != 0) {
        f |= (Flags)(FlagPointToPoint);
    }
    if ((uint32)(rawFlags & (uint32)syscall.IFF_MULTICAST) != 0) {
        f |= (Flags)(FlagMulticast);
    }
    return f;
}

// If the ifi is nil, interfaceAddrTable returns addresses for all
// network interfaces. Otherwise it returns addresses for a specific
// interface.
internal static (slice<ΔAddr>, error) interfaceAddrTable(ж<Interface> Ꮡifi) {
    var (tab, err) = syscall.NetlinkRIB(syscall.RTM_GETADDR, syscall.AF_UNSPEC);
    if (err != default!) {
        return (default!, os.NewSyscallError(netlinkribˢ, err));
    }
    (var msgs, err) = syscall.ParseNetlinkMessage(tab);
    if (err != default!) {
        return (default!, os.NewSyscallError(parsenetlinkmessageˢ, err));
    }
    (var ifat, err) = addrTable(Ꮡifi, msgs);
    if (err != default!) {
        return (default!, err);
    }
    return (ifat, default!);
}

internal static (slice<ΔAddr>, error) addrTable(ж<Interface> Ꮡifi, slice<syscall.NetlinkMessage> msgs) {
    ref var ifi = ref Ꮡifi.DerefOrNull();

    slice<ΔAddr> ifat = default!;
loop:
    foreach (var (_, vᴛ1) in msgs) {
        ref var m = ref heap(new syscall.NetlinkMessage(), out var Ꮡm);
        m = vᴛ1;

        var exprᴛ1 = m.Header.Type;
        if (exprᴛ1 == syscall.NLMSG_DONE) {
            goto break_loop;
        }
        else if (exprᴛ1 == syscall.RTM_NEWADDR) {
            var ifam = Ꮡ(m.Data, 0).Reinterpret<byte, syscall.IfAddrmsg>();
            if (Ꮡifi == nil || ifi.Index == (nint)(~ifam).Index) {
                var (attrs, err) = syscall.ParseNetlinkRouteAttr(Ꮡm);
                if (err != default!) {
                    return (default!, os.NewSyscallError(parsenetlinkrouteattrˢ, err));
                }
                var ifa = newAddr(ref (ifam).DerefOrNull(), attrs);
                if (ifa != default!) {
                    ifat = append(ifat, ifa);
                }
            }
        }

continue_loop:;
    }
break_loop:;
    return (ifat, default!);
}

internal static ΔAddr newAddr(ref syscall.IfAddrmsg ifam, slice<syscall.NetlinkRouteAttr> attrs) {
    bool ipPointToPoint = default!;
    // Seems like we need to make sure whether the IP interface
    // stack consists of IP point-to-point numbered or unnumbered
    // addressing.
    foreach (var (_, a) in attrs) {
        if (a.Attr.Type == syscall.IFA_LOCAL) {
            ipPointToPoint = true;
            break;
        }
    }
    foreach (var (_, a) in attrs) {
        if (ipPointToPoint && a.Attr.Type == syscall.IFA_ADDRESS) {
            continue;
        }
        var exprᴛ1 = ifam.Family;
        if (exprᴛ1 == syscall.AF_INET) {
            return new IPNetжΔAddr(Ꮡ(new IPNet(IP: IPv4(a.Value[0], a.Value[1], a.Value[2], a.Value[3]), Mask: CIDRMask((nint)ifam.Prefixlen, 8 * IPv4len))));
        }
        if (exprᴛ1 == syscall.AF_INET6) {
            var ifa = Ꮡ(new IPNet(IP: new IP(IPv6len), Mask: CIDRMask((nint)ifam.Prefixlen, 8 * IPv6len)));
            copy((~ifa).IP, a.Value[..]);
            return new IPNetжΔAddr(ifa);
        }

    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string procNetIgmpˢ = "/proc/net/igmp"u8;
internal static readonly @string procNetIgmp6ˢ = "/proc/net/igmp6"u8;

// interfaceMulticastAddrTable returns addresses for a specific
// interface.
internal static (slice<ΔAddr>, error) interfaceMulticastAddrTable(ж<Interface> Ꮡifi) {
    var ifmat4 = parseProcNetIGMP(procNetIgmpˢ, Ꮡifi);
    var ifmat6 = parseProcNetIGMP6(procNetIgmp6ˢ, Ꮡifi);
    return (appendꓸꓸꓸ(ifmat4, ifmat6), default!);
}

internal static slice<ΔAddr> parseProcNetIGMP(@string path, ж<Interface> Ꮡifi) {
    GoFrame ᒐ = default;
    try {
        ref var ifi = ref Ꮡifi.DerefOrNull();

        var (fd, err) = open(path);
        if (err != default!) {
            return default!;
        }
        var fdʗ1 = fd;
        defer(fdʗ1.close, ref ᒐ);
        slice<ΔAddr> ifmat = default!;
        @string name = default!;
        fd.readLine(); // skip first line
        var b = new slice<byte>(IPv4len);
        for (var (l, ok) = fd.readLine(); ok; (l, ok) = fd.readLine()) {
            var f = splitAtBytes(l, " :\r\t\n"u8);
            if (len(f) < 4) {
                continue;
            }
            switch (ᐧ) {
            case {} when l[0] != (rune)' ' && l[0] != (rune)'\t': {
                name = f[1];
                break;
            }
            case {} when len(f[0]) is 8: {
                if (Ꮡifi == nil || name == ifi.Name) {
                    // new interface line
                    // The Linux kernel puts the IP
                    // address in /proc/net/igmp in native
                    // endianness.
                    for (nint iΔ1 = 0; iΔ1 + 1 < len(f[0]); iΔ1 += 2) {
                        (b[iΔ1 / 2], _) = xtoi2(f[0].slice(iΔ1, iΔ1 + 2), 0);
                    }
                    var i = ~Ꮡ(b[..4], 0).Reinterpret<byte, uint32>();
                    var ifma = Ꮡ(new IPAddr(IP: IPv4((byte)((i >> (int)(24))), (byte)((i >> (int)(16))), (byte)((i >> (int)(8))), (byte)i)));
                    ifmat = append(ifmat, (ΔAddr)(new IPAddrжΔAddr(ifma)));
                }
                break;
            }}

        }
        return ifmat;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static slice<ΔAddr> parseProcNetIGMP6(@string path, ж<Interface> Ꮡifi) {
    GoFrame ᒐ = default;
    try {
        ref var ifi = ref Ꮡifi.DerefOrNull();

        var (fd, err) = open(path);
        if (err != default!) {
            return default!;
        }
        var fdʗ1 = fd;
        defer(fdʗ1.close, ref ᒐ);
        slice<ΔAddr> ifmat = default!;
        var b = new slice<byte>(IPv6len);
        for (var (l, ok) = fd.readLine(); ok; (l, ok) = fd.readLine()) {
            var f = splitAtBytes(l, " \r\t\n"u8);
            if (len(f) < 6) {
                continue;
            }
            if (Ꮡifi == nil || f[1] == ifi.Name) {
                for (nint i = 0; i + 1 < len(f[2]); i += 2) {
                    (b[i / 2], _) = xtoi2(f[2].slice(i, i + 2), 0);
                }
                var ifma = Ꮡ(new IPAddr(IP: new IP(new byte[]{b[0], b[1], b[2], b[3], b[4], b[5], b[6], b[7], b[8], b[9], b[10], b[11], b[12], b[13], b[14], b[15]}.slice())));
                ifmat = append(ifmat, (ΔAddr)(new IPAddrжΔAddr(ifma)));
            }
        }
        return ifmat;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end net_package
