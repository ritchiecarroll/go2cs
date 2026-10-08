// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1 || windows
namespace go;

using context = context_package;
using netip = net.netip_package;
using syscall = syscall_package;
using net;

partial class net_package {

internal static ΔAddr sockaddrToUDP(syscallꓸSockaddr sa) {
    switch (sa.type()) {
    case ж<syscall.SockaddrInet4> saΔ1: {
        return new UDPAddrжΔAddr(Ꮡ(new UDPAddr(IP: (~saΔ1).Addr[0..], Port: (~saΔ1).Port)));
    }
    case ж<syscall.SockaddrInet6> saΔ1: {
        return new UDPAddrжΔAddr(Ꮡ(new UDPAddr(IP: (~saΔ1).Addr[0..], Port: (~saΔ1).Port, Zone: zoneCache.name((nint)(~saΔ1).ZoneId))));
    }}
    return default!;
}

internal static nint family(this ж<UDPAddr> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil || len(a.IP) <= IPv4len) {
        return syscall.AF_INET;
    }
    if (a.IP.To4() != default!) {
        return syscall.AF_INET;
    }
    return syscall.AF_INET6;
}

internal static (syscallꓸSockaddr, error) sockaddr(this ж<UDPAddr> Ꮡa, nint family) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil) {
        return (default!, default!);
    }
    return ipToSockaddr(family, a.IP, a.Port, a.Zone);
}

internal static Δsockaddr toLocal(this ref UDPAddr a, @string net) {
    return new UDPAddrжΔsockaddr(Ꮡ(new UDPAddr(loopbackIP(net), a.Port, a.Zone)));
}

internal static (nint, ж<UDPAddr>, error) readFrom(this ref UDPConn c, slice<byte> b, ж<UDPAddr> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.DerefOrNull();

    nint n = default!;
    error err = default!;
    var exprᴛ1 = (~c.fd).family;
    if (exprᴛ1 == syscall.AF_INET) {
        ref var from = ref heap(new syscall.SockaddrInet4(), out var Ꮡfrom);
        (n, err) = c.fd.readFromInet4(b, Ꮡfrom);
        if (err == default!) {
            var ip = from.Addr.Clone(); // copy from.Addr; ip escapes, so this line allocates 4 bytes
            addr = new UDPAddr(IP: ip[..], Port: from.Port);
        }
    }
    else if (exprᴛ1 == syscall.AF_INET6) {
        ref var from = ref heap(new syscall.SockaddrInet6(), out var Ꮡfrom);
        (n, err) = c.fd.readFromInet6(b, Ꮡfrom);
        if (err == default!) {
            var ip = from.Addr.Clone(); // copy from.Addr; ip escapes, so this line allocates 16 bytes
            addr = new UDPAddr(IP: ip[..], Port: from.Port, Zone: zoneCache.name((nint)from.ZoneId));
        }
    }

    if (err != default!) {
        // No sockaddr, so don't return UDPAddr.
        Ꮡaddr = default!; addr = ref Ꮡaddr.DerefOrNull();
    }
    return (n, Ꮡaddr, err);
}

internal static (nint n, netip.AddrPort addr, error err) readFromAddrPort(this ref UDPConn c, slice<byte> b) {
    nint n = default!;
    netip.AddrPort addr = default!;
    error err = default!;

    netipꓸAddr ip = default!;
    nint port = default!;
    var exprᴛ1 = (~c.fd).family;
    if (exprᴛ1 == syscall.AF_INET) {
        ref var from = ref heap(new syscall.SockaddrInet4(), out var Ꮡfrom);
        (n, err) = c.fd.readFromInet4(b, Ꮡfrom);
        if (err == default!) {
            ip = netip.AddrFrom4(from.Addr);
            port = from.Port;
        }
    }
    else if (exprᴛ1 == syscall.AF_INET6) {
        ref var from = ref heap(new syscall.SockaddrInet6(), out var Ꮡfrom);
        (n, err) = c.fd.readFromInet6(b, Ꮡfrom);
        if (err == default!) {
            ip = netip.AddrFrom16(from.Addr).WithZone(zoneCache.name((nint)from.ZoneId));
            port = from.Port;
        }
    }

    if (err == default!) {
        addr = netip.AddrPortFrom(ip, (uint16)port);
    }
    return (n, addr, err);
}

internal static (nint n, nint oobn, nint flags, netip.AddrPort addr, error err) readMsg(this ref UDPConn c, slice<byte> b, slice<byte> oob) {
    nint n = default!;
    nint oobn = default!;
    nint flags = default!;
    netip.AddrPort addr = default!;
    error err = default!;

    var exprᴛ1 = (~c.fd).family;
    if (exprᴛ1 == syscall.AF_INET) {
        ref var sa = ref heap(new syscall.SockaddrInet4(), out var Ꮡsa);
        (n, oobn, flags, err) = c.fd.readMsgInet4(b, oob, 0, Ꮡsa);
        var ip = netip.AddrFrom4(sa.Addr);
        addr = netip.AddrPortFrom(ip, (uint16)sa.Port);
    }
    else if (exprᴛ1 == syscall.AF_INET6) {
        ref var sa = ref heap(new syscall.SockaddrInet6(), out var Ꮡsa);
        (n, oobn, flags, err) = c.fd.readMsgInet6(b, oob, 0, Ꮡsa);
        var ip = netip.AddrFrom16(sa.Addr).WithZone(zoneCache.name((nint)sa.ZoneId));
        addr = netip.AddrPortFrom(ip, (uint16)sa.Port);
    }

    return (n, oobn, flags, addr, err);
}

internal static (nint, error) writeTo(this ref UDPConn c, slice<byte> b, ж<UDPAddr> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.DerefOrNull();

    if ((~c.fd).isConnected) {
        return (0, ErrWriteToConnected);
    }
    if (Ꮡaddr == nil) {
        return (0, errMissingAddress);
    }
    var exprᴛ1 = (~c.fd).family;
    if (exprᴛ1 == syscall.AF_INET) {
        ref var sa = ref heap<syscall.SockaddrInet4>(out var Ꮡsa);
        (sa, var err) = ipToSockaddrInet4(addr.IP, addr.Port);
        if (err != default!) {
            return (0, err);
        }
        return c.fd.writeToInet4(b, Ꮡsa);
    }
    if (exprᴛ1 == syscall.AF_INET6) {
        ref var sa = ref heap<syscall.SockaddrInet6>(out var Ꮡsa);
        (sa, var err) = ipToSockaddrInet6(addr.IP, addr.Port, addr.Zone);
        if (err != default!) {
            return (0, err);
        }
        return c.fd.writeToInet6(b, Ꮡsa);
    }
    { /* default: */
        return (0, new AddrErrorжerror(Ꮡ(new AddrError(Err: "invalid address family"u8, Addr: addr.IP.String()))));
    }

}

internal static (nint, error) writeToAddrPort(this ref UDPConn c, slice<byte> b, netip.AddrPort addr) {
    if ((~c.fd).isConnected) {
        return (0, ErrWriteToConnected);
    }
    if (!addr.IsValid()) {
        return (0, errMissingAddress);
    }
    var exprᴛ1 = (~c.fd).family;
    if (exprᴛ1 == syscall.AF_INET) {
        ref var sa = ref heap<syscall.SockaddrInet4>(out var Ꮡsa);
        (sa, var err) = addrPortToSockaddrInet4(addr);
        if (err != default!) {
            return (0, err);
        }
        return c.fd.writeToInet4(b, Ꮡsa);
    }
    if (exprᴛ1 == syscall.AF_INET6) {
        ref var sa = ref heap<syscall.SockaddrInet6>(out var Ꮡsa);
        (sa, var err) = addrPortToSockaddrInet6(addr);
        if (err != default!) {
            return (0, err);
        }
        return c.fd.writeToInet6(b, Ꮡsa);
    }
    { /* default: */
        return (0, new AddrErrorжerror(Ꮡ(new AddrError(Err: "invalid address family"u8, Addr: addr.Addr().String()))));
    }

}

internal static (nint n, nint oobn, error err) writeMsg(this ref UDPConn c, slice<byte> b, slice<byte> oob, ж<UDPAddr> Ꮡaddr) {
    error err = default!;

    if ((~c.fd).isConnected && Ꮡaddr != nil) {
        return (0, 0, ErrWriteToConnected);
    }
    if (!(~c.fd).isConnected && Ꮡaddr == nil) {
        return (0, 0, errMissingAddress);
    }
    (var sa, err) = Ꮡaddr.sockaddr((~c.fd).family);
    if (err != default!) {
        return (0, 0, err);
    }
    return c.fd.writeMsg(b, oob, sa);
}

internal static (nint n, nint oobn, error err) writeMsgAddrPort(this ref UDPConn c, slice<byte> b, slice<byte> oob, netip.AddrPort addr) {
    if ((~c.fd).isConnected && addr.IsValid()) {
        return (0, 0, ErrWriteToConnected);
    }
    if (!(~c.fd).isConnected && !addr.IsValid()) {
        return (0, 0, errMissingAddress);
    }
    var exprᴛ1 = (~c.fd).family;
    if (exprᴛ1 == syscall.AF_INET) {
        ref var sa = ref heap<syscall.SockaddrInet4>(out var Ꮡsa);
        (sa, var errΔ2) = addrPortToSockaddrInet4(addr);
        if (errΔ2 != default!) {
            return (0, 0, errΔ2);
        }
        return c.fd.writeMsgInet4(b, oob, Ꮡsa);
    }
    if (exprᴛ1 == syscall.AF_INET6) {
        ref var sa = ref heap<syscall.SockaddrInet6>(out var Ꮡsa);
        (sa, var errΔ3) = addrPortToSockaddrInet6(addr);
        if (errΔ3 != default!) {
            return (0, 0, errΔ3);
        }
        return c.fd.writeMsgInet6(b, oob, Ꮡsa);
    }
    { /* default: */
        return (0, 0, new AddrErrorжerror(Ꮡ(new AddrError(Err: "invalid address family"u8, Addr: addr.Addr().String()))));
    }

}

internal static (ж<UDPConn>, error) dialUDP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<UDPAddr> Ꮡladdr, ж<UDPAddr> Ꮡraddr) {
    ref var sd = ref Ꮡsd.DerefOrNull();

    var ctrlCtxFn = sd.Dialer.ControlContext;
    if (ctrlCtxFn == default! && sd.Dialer.Control != default!) {
        ctrlCtxFn = (context.Context ctxΔ1, @string network, @string address, syscall.RawConn c) => Ꮡsd.Value.Dialer.Control(network, address, c);
    }
    var (fd, err) = internetSocket(ctx, sd.network, new UDPAddrжΔsockaddr(Ꮡladdr), new UDPAddrжΔsockaddr(Ꮡraddr), syscall.SOCK_DGRAM, 0, dialˢ, ctrlCtxFn);
    if (err != default!) {
        return (default!, err);
    }
    return (newUDPConn(fd), default!);
}

internal static (ж<UDPConn>, error) listenUDP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<UDPAddr> Ꮡladdr) {
    ref var sl = ref Ꮡsl.DerefOrNull();

    Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn = default!;
    if (sl.ListenConfig.Control != default!) {
        ctrlCtxFn = (context.Context ctxΔ1, @string network, @string address, syscall.RawConn c) => Ꮡsl.Value.ListenConfig.Control(network, address, c);
    }
    var (fd, err) = internetSocket(ctx, sl.network, new UDPAddrжΔsockaddr(Ꮡladdr), default!, syscall.SOCK_DGRAM, 0, listenˢ, ctrlCtxFn);
    if (err != default!) {
        return (default!, err);
    }
    return (newUDPConn(fd), default!);
}

internal static (ж<UDPConn>, error) listenMulticastUDP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<Interface> Ꮡifi, ж<UDPAddr> Ꮡgaddr) {
    ref var sl = ref Ꮡsl.DerefOrNull();
    ref var gaddr = ref Ꮡgaddr.DerefOrNull();

    Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn = default!;
    if (sl.ListenConfig.Control != default!) {
        ctrlCtxFn = (context.Context ctxΔ1, @string network, @string address, syscall.RawConn cΔ1) => Ꮡsl.Value.ListenConfig.Control(network, address, cΔ1);
    }
    var (fd, err) = internetSocket(ctx, sl.network, new UDPAddrжΔsockaddr(Ꮡgaddr), default!, syscall.SOCK_DGRAM, 0, listenˢ, ctrlCtxFn);
    if (err != default!) {
        return (default!, err);
    }
    var c = newUDPConn(fd);
    {
        var ip4 = gaddr.IP.To4(); if (ip4 != default!){
            {
                var errΔ1 = listenIPv4MulticastUDP(ref (c).DerefOrNull(), Ꮡifi, ip4); if (errΔ1 != default!) {
                    c.of(UDPConn.Ꮡconn).Close();
                    return (default!, errΔ1);
                }
            }
        } else {
            {
                var errΔ2 = listenIPv6MulticastUDP(ref (c).DerefOrNull(), Ꮡifi, gaddr.IP); if (errΔ2 != default!) {
                    c.of(UDPConn.Ꮡconn).Close();
                    return (default!, errΔ2);
                }
            }
        }
    }
    return (c, default!);
}

internal static error listenIPv4MulticastUDP(ref UDPConn c, ж<Interface> Ꮡifi, IP ip) {
    if (Ꮡifi != nil) {
        {
            var err = setIPv4MulticastInterface(c.fd, Ꮡifi); if (err != default!) {
                return err;
            }
        }
    }
    {
        var err = setIPv4MulticastLoopback(c.fd, false); if (err != default!) {
            return err;
        }
    }
    {
        var err = joinIPv4Group(c.fd, Ꮡifi, ip); if (err != default!) {
            return err;
        }
    }
    return default!;
}

internal static error listenIPv6MulticastUDP(ref UDPConn c, ж<Interface> Ꮡifi, IP ip) {
    if (Ꮡifi != nil) {
        {
            var err = setIPv6MulticastInterface(c.fd, Ꮡifi); if (err != default!) {
                return err;
            }
        }
    }
    {
        var err = setIPv6MulticastLoopback(c.fd, false); if (err != default!) {
            return err;
        }
    }
    {
        var err = joinIPv6Group(c.fd, Ꮡifi, ip); if (err != default!) {
            return err;
        }
    }
    return default!;
}

} // end net_package
