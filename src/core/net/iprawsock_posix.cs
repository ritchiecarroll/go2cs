// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1 || windows
namespace go;

using context = context_package;
using syscall = syscall_package;
using static net_package;

partial class net_package {

internal static Addr sockaddrToIP(syscall.Sockaddr sa) {
    switch (sa.type()) {
    case ж<syscall.SockaddrInet4> saΔ1: {
        return new IPAddrжAddr(Ꮡ(new IPAddr(IP: saΔ1.Addr[0..])));
    }
    case ж<syscall.SockaddrInet6> saΔ1: {
        return new IPAddrжAddr(Ꮡ(new IPAddr(IP: saΔ1.Addr[0..], Zone: zoneCache.name((nint)saΔ1.ZoneId))));
    }}
    return default;
}

} partial class net_package {

[GoMethodBody("family")] internal static nint familyᴍ(ж<IPAddr> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil || len(a.IP) <= IPv4len) {
        return syscall.AF_INET;
    }
    if (a.IP.To4() != nil) {
        return syscall.AF_INET;
    }
    return syscall.AF_INET6;
}

[GoMethodBody("sockaddr")] internal static (syscall.Sockaddr, error) sockaddrᴍ(ж<IPAddr> Ꮡa, nint family) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil) {
        return (default, default);
    }
    return ipToSockaddr(family, a.IP, 0, a.Zone);
}

[GoMethodBody("toLocal")] internal static sockaddr toLocalᴍ(ref IPAddr a, @string net) {
    return new IPAddrжsockaddr(Ꮡ(new IPAddr(loopbackIP(net), a.Zone)));
}

} partial class net_package {

[GoMethodBody("readFrom")] internal static (nint, ж<IPAddr>, error) readFromᴍ(ref IPConn c, slice<byte> b) {
    // TODO(cw,rsc): consider using readv if we know the family
    // type to avoid the header trim/copy
    ж<IPAddr> addr = default;
    var (n, sa, err) = c.fd.readFrom(b);
    switch (sa.type()) {
    case ж<syscall.SockaddrInet4> saΔ1: {
        addr = Ꮡ(new IPAddr(IP: saΔ1.Addr[0..]));
        n = stripIPv4Header(n, b);
        break;
    }
    case ж<syscall.SockaddrInet6> saΔ1: {
        addr = Ꮡ(new IPAddr(IP: saΔ1.Addr[0..], Zone: zoneCache.name((nint)saΔ1.ZoneId)));
        break;
    }}
    return (n, addr, err);
}

} partial class net_package {

internal static nint stripIPv4Header(nint n, slice<byte> b) {
    if (len(b) < 20) {
        return n;
    }
    nint l = (nint)((byte)(b[0] & 0x0f)) << 2;
    if (20 > l || l > len(b)) {
        return n;
    }
    if ((byte)((b[0] >> 4)) != 4) {
        return n;
    }
    copy(b, b.slice(l));
    return n - l;
}

} partial class net_package {

[GoMethodBody("readMsg")] internal static (nint n, nint oobn, nint flags, ж<IPAddr> addr, error err) readMsgᴍ(ref IPConn c, slice<byte> b, slice<byte> oob) {
    nint n = default;
    nint oobn = default;
    nint flags = default;
    ж<IPAddr> addr = default;
    error err = default;

    syscall.Sockaddr sa = default;
    (n, oobn, flags, sa, err) = c.fd.readMsg(b, oob, 0);
    switch (sa.type()) {
    case ж<syscall.SockaddrInet4> saΔ1: {
        addr = Ꮡ(new IPAddr(IP: saΔ1.Addr[0..]));
        break;
    }
    case ж<syscall.SockaddrInet6> saΔ1: {
        addr = Ꮡ(new IPAddr(IP: saΔ1.Addr[0..], Zone: zoneCache.name((nint)saΔ1.ZoneId)));
        break;
    }}
    return (n, oobn, flags, addr, err);
}

[GoMethodBody("writeTo")] internal static (nint, error) writeToᴍ(ref IPConn c, slice<byte> b, ж<IPAddr> addr) {
    if (c.fd.isConnected) {
        return (0, ErrWriteToConnected);
    }
    if (addr == nil) {
        return (0, errMissingAddress);
    }
    var (sa, err) = addr.sockaddr(c.fd.family);
    if (err != nil) {
        return (0, err);
    }
    return c.fd.writeTo(b, sa);
}

[GoMethodBody("writeMsg")] internal static (nint n, nint oobn, error err) writeMsgᴍ(ref IPConn c, slice<byte> b, slice<byte> oob, ж<IPAddr> addr) {
    error err = default;

    if (c.fd.isConnected) {
        return (0, 0, ErrWriteToConnected);
    }
    if (addr == nil) {
        return (0, 0, errMissingAddress);
    }
    (var sa, err) = addr.sockaddr(c.fd.family);
    if (err != nil) {
        return (0, 0, err);
    }
    return c.fd.writeMsg(b, oob, sa);
}

} partial class net_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dialˢᴍsysDialer => ref dialˢᵇᴍsysDialer;
private static readonly @string dialˢᵇᴍsysDialer = "dial"u8;

[GoMethodBody("dialIP")] internal static (ж<IPConn>, error) dialIPᴍ(ж<sysDialer> Ꮡsd, context.Context ctx, ж<IPAddr> laddr, ж<IPAddr> raddr) {
    ref var sd = ref Ꮡsd.DerefOrNull();

    var (network, proto, err) = parseNetwork(ctx, sd.network, true);
    if (err != nil) {
        return (default, err);
    }
    switch (ᐧ) {
    case {} when network == "ip"u8 || network == "ip4"u8 || network == "ip6"u8: {
        break;
    }
    default: {
        return (default, ((UnknownNetworkError)sd.network));
    }}
    var ctrlCtxFn = sd.Dialer.ControlContext;
    if (ctrlCtxFn == nil && sd.Dialer.Control != nil) {
        ctrlCtxFn = (context.Context ctxΔ1, @string networkΔ1, @string address, syscall.RawConn c) => Ꮡsd.Value.Dialer.Control(networkΔ1, address, c);
    }
    (var fd, err) = internetSocket(ctx, network, new IPAddrжsockaddr(laddr), new IPAddrжsockaddr(raddr), syscall.SOCK_RAW, proto, dialˢᴍsysDialer, ctrlCtxFn);
    if (err != nil) {
        return (default, err);
    }
    return (newIPConn(fd), default);
}

} partial class net_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string listenˢᴍsysListener => ref listenˢᵇᴍsysListener;
private static readonly @string listenˢᵇᴍsysListener = "listen"u8;

[GoMethodBody("listenIP")] internal static (ж<IPConn>, error) listenIPᴍ(ж<sysListener> Ꮡsl, context.Context ctx, ж<IPAddr> laddr) {
    ref var sl = ref Ꮡsl.DerefOrNull();

    var (network, proto, err) = parseNetwork(ctx, sl.network, true);
    if (err != nil) {
        return (default, err);
    }
    switch (ᐧ) {
    case {} when network == "ip"u8 || network == "ip4"u8 || network == "ip6"u8: {
        break;
    }
    default: {
        return (default, ((UnknownNetworkError)sl.network));
    }}
    Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn = default;
    if (sl.ListenConfig.Control != nil) {
        ctrlCtxFn = (context.Context ctxΔ1, @string networkΔ1, @string address, syscall.RawConn c) => Ꮡsl.Value.ListenConfig.Control(networkΔ1, address, c);
    }
    (var fd, err) = internetSocket(ctx, network, new IPAddrжsockaddr(laddr), default, syscall.SOCK_RAW, proto, listenˢᴍsysListener, ctrlCtxFn);
    if (err != nil) {
        return (default, err);
    }
    return (newIPConn(fd), default);
}

}
