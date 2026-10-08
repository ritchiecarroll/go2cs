// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1 || windows
namespace go;

using context = context_package;
using syscall = syscall_package;

partial class net_package {

internal static ΔAddr sockaddrToIP(syscallꓸSockaddr sa) {
    switch (sa.type()) {
    case ж<syscall.SockaddrInet4> saΔ1: {
        return new IPAddrжΔAddr(Ꮡ(new IPAddr(IP: (~saΔ1).Addr[0..])));
    }
    case ж<syscall.SockaddrInet6> saΔ1: {
        return new IPAddrжΔAddr(Ꮡ(new IPAddr(IP: (~saΔ1).Addr[0..], Zone: zoneCache.name((nint)(~saΔ1).ZoneId))));
    }}
    return default!;
}

internal static nint family(this ж<IPAddr> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil || len(a.IP) <= IPv4len) {
        return syscall.AF_INET;
    }
    if (a.IP.To4() != default!) {
        return syscall.AF_INET;
    }
    return syscall.AF_INET6;
}

internal static (syscallꓸSockaddr, error) sockaddr(this ж<IPAddr> Ꮡa, nint family) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil) {
        return (default!, default!);
    }
    return ipToSockaddr(family, a.IP, 0, a.Zone);
}

internal static Δsockaddr toLocal(this ref IPAddr a, @string net) {
    return new IPAddrжΔsockaddr(Ꮡ(new IPAddr(loopbackIP(net), a.Zone)));
}

internal static (nint, ж<IPAddr>, error) readFrom(this ref IPConn c, slice<byte> b) {
    // TODO(cw,rsc): consider using readv if we know the family
    // type to avoid the header trim/copy
    ж<IPAddr> addr = default!;
    var (n, sa, err) = c.fd.readFrom(b);
    switch (sa.type()) {
    case ж<syscall.SockaddrInet4> saΔ1: {
        addr = Ꮡ(new IPAddr(IP: (~saΔ1).Addr[0..]));
        n = stripIPv4Header(n, b);
        break;
    }
    case ж<syscall.SockaddrInet6> saΔ1: {
        addr = Ꮡ(new IPAddr(IP: (~saΔ1).Addr[0..], Zone: zoneCache.name((nint)(~saΔ1).ZoneId)));
        break;
    }}
    return (n, addr, err);
}

internal static nint stripIPv4Header(nint n, slice<byte> b) {
    if (len(b) < 20) {
        return n;
    }
    nint l = ((nint)((byte)(b[0] & 0x0f)) << (int)(2));
    if (20 > l || l > len(b)) {
        return n;
    }
    if ((byte)((b[0] >> (int)(4))) != 4) {
        return n;
    }
    copy(b, b.slice(l));
    return n - l;
}

internal static (nint n, nint oobn, nint flags, ж<IPAddr> addr, error err) readMsg(this ref IPConn c, slice<byte> b, slice<byte> oob) {
    nint n = default!;
    nint oobn = default!;
    nint flags = default!;
    ж<IPAddr> addr = default!;
    error err = default!;

    syscallꓸSockaddr sa = default!;
    (n, oobn, flags, sa, err) = c.fd.readMsg(b, oob, 0);
    switch (sa.type()) {
    case ж<syscall.SockaddrInet4> saΔ1: {
        addr = Ꮡ(new IPAddr(IP: (~saΔ1).Addr[0..]));
        break;
    }
    case ж<syscall.SockaddrInet6> saΔ1: {
        addr = Ꮡ(new IPAddr(IP: (~saΔ1).Addr[0..], Zone: zoneCache.name((nint)(~saΔ1).ZoneId)));
        break;
    }}
    return (n, oobn, flags, addr, err);
}

internal static (nint, error) writeTo(this ref IPConn c, slice<byte> b, ж<IPAddr> Ꮡaddr) {
    if ((~c.fd).isConnected) {
        return (0, ErrWriteToConnected);
    }
    if (Ꮡaddr == nil) {
        return (0, errMissingAddress);
    }
    var (sa, err) = Ꮡaddr.sockaddr((~c.fd).family);
    if (err != default!) {
        return (0, err);
    }
    return c.fd.writeTo(b, sa);
}

internal static (nint n, nint oobn, error err) writeMsg(this ref IPConn c, slice<byte> b, slice<byte> oob, ж<IPAddr> Ꮡaddr) {
    error err = default!;

    if ((~c.fd).isConnected) {
        return (0, 0, ErrWriteToConnected);
    }
    if (Ꮡaddr == nil) {
        return (0, 0, errMissingAddress);
    }
    (var sa, err) = Ꮡaddr.sockaddr((~c.fd).family);
    if (err != default!) {
        return (0, 0, err);
    }
    return c.fd.writeMsg(b, oob, sa);
}

internal static (ж<IPConn>, error) dialIP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<IPAddr> Ꮡladdr, ж<IPAddr> Ꮡraddr) {
    ref var sd = ref Ꮡsd.DerefOrNull();

    var (network, proto, err) = parseNetwork(ctx, sd.network, true);
    if (err != default!) {
        return (default!, err);
    }
    var exprᴛ1 = network;
    if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
    }
    else { /* default: */
        return (default!, ((UnknownNetworkError)sd.network));
    }

    var ctrlCtxFn = sd.Dialer.ControlContext;
    if (ctrlCtxFn == default! && sd.Dialer.Control != default!) {
        ctrlCtxFn = (context.Context ctxΔ1, @string networkΔ1, @string address, syscall.RawConn c) => Ꮡsd.Value.Dialer.Control(networkΔ1, address, c);
    }
    (var fd, err) = internetSocket(ctx, network, new IPAddrжΔsockaddr(Ꮡladdr), new IPAddrжΔsockaddr(Ꮡraddr), syscall.SOCK_RAW, proto, dialˢ, ctrlCtxFn);
    if (err != default!) {
        return (default!, err);
    }
    return (newIPConn(fd), default!);
}

internal static (ж<IPConn>, error) listenIP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<IPAddr> Ꮡladdr) {
    ref var sl = ref Ꮡsl.DerefOrNull();

    var (network, proto, err) = parseNetwork(ctx, sl.network, true);
    if (err != default!) {
        return (default!, err);
    }
    var exprᴛ1 = network;
    if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
    }
    else { /* default: */
        return (default!, ((UnknownNetworkError)sl.network));
    }

    Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn = default!;
    if (sl.ListenConfig.Control != default!) {
        ctrlCtxFn = (context.Context ctxΔ1, @string networkΔ1, @string address, syscall.RawConn c) => Ꮡsl.Value.ListenConfig.Control(networkΔ1, address, c);
    }
    (var fd, err) = internetSocket(ctx, network, new IPAddrжΔsockaddr(Ꮡladdr), default!, syscall.SOCK_RAW, proto, listenˢ, ctrlCtxFn);
    if (err != default!) {
        return (default!, err);
    }
    return (newIPConn(fd), default!);
}

} // end net_package
