// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1 || windows
namespace go;

using context = context_package;
using Δio = io_package;
using os = os_package;
using syscall = syscall_package;
using time = time_package;

partial class net_package {

internal static ΔAddr sockaddrToTCP(syscallꓸSockaddr sa) {
    switch (sa.type()) {
    case ж<syscall.SockaddrInet4> saΔ1: {
        return new TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: (~saΔ1).Addr[0..], Port: (~saΔ1).Port)));
    }
    case ж<syscall.SockaddrInet6> saΔ1: {
        return new TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: (~saΔ1).Addr[0..], Port: (~saΔ1).Port, Zone: zoneCache.name((nint)(~saΔ1).ZoneId))));
    }}
    return default!;
}

internal static nint family(this ж<TCPAddr> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil || len(a.IP) <= IPv4len) {
        return syscall.AF_INET;
    }
    if (a.IP.To4() != default!) {
        return syscall.AF_INET;
    }
    return syscall.AF_INET6;
}

internal static (syscallꓸSockaddr, error) sockaddr(this ж<TCPAddr> Ꮡa, nint family) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil) {
        return (default!, default!);
    }
    return ipToSockaddr(family, a.IP, a.Port, a.Zone);
}

internal static Δsockaddr toLocal(this ref TCPAddr a, @string net) {
    return new TCPAddrжΔsockaddr(Ꮡ(new TCPAddr(loopbackIP(net), a.Port, a.Zone)));
}

internal static (int64, error) readFrom(this ж<TCPConn> Ꮡc, Δio.Reader r) {
    ref var c = ref Ꮡc.DerefOrNull();

    {
        var (n, err, handled) = spliceFrom(c.fd, r); if (handled) {
            return (n, err);
        }
    }
    {
        var (n, err, handled) = sendFile(c.fd, r); if (handled) {
            return (n, err);
        }
    }
    return genericReadFrom(Ꮡc, r);
}

internal static (int64, error) writeTo(this ж<TCPConn> Ꮡc, Δio.Writer w) {
    ref var c = ref Ꮡc.DerefOrNull();

    {
        var (n, err, handled) = spliceTo(w, c.fd); if (handled) {
            return (n, err);
        }
    }
    return genericWriteTo(Ꮡc, w);
}

internal static (ж<TCPConn>, error) dialTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> Ꮡraddr) {
    ref var sd = ref Ꮡsd.DerefOrNull();

    {
        var h = sd.testHookDialTCP; if (h != default!) {
            return h(ctx, sd.network, Ꮡladdr, Ꮡraddr);
        }
    }
    {
        var h = testHookDialTCP; if (h != default!) {
            return h(ctx, sd.network, Ꮡladdr, Ꮡraddr);
        }
    }
    return Ꮡsd.doDialTCP(ctx, Ꮡladdr, Ꮡraddr);
}

internal static (ж<TCPConn>, error) doDialTCP(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> Ꮡraddr) {
    return Ꮡsd.doDialTCPProto(ctx, Ꮡladdr, Ꮡraddr, 0);
}

internal static (ж<TCPConn>, error) doDialTCPProto(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<TCPAddr> Ꮡladdr, ж<TCPAddr> Ꮡraddr, nint proto) {
    ref var sd = ref Ꮡsd.DerefOrNull();
    ref var laddr = ref Ꮡladdr.DerefOrNull();

    var ctrlCtxFn = sd.Dialer.ControlContext;
    if (ctrlCtxFn == default! && sd.Dialer.Control != default!) {
        ctrlCtxFn = (context.Context ctxΔ1, @string network, @string address, syscall.RawConn c) => Ꮡsd.Value.Dialer.Control(network, address, c);
    }
    var (fd, err) = internetSocket(ctx, sd.network, new TCPAddrжΔsockaddr(Ꮡladdr), new TCPAddrжΔsockaddr(Ꮡraddr), syscall.SOCK_STREAM, proto, dialˢ, ctrlCtxFn);
    // TCP has a rarely used mechanism called a 'simultaneous connection' in
    // which Dial("tcp", addr1, addr2) run on the machine at addr1 can
    // connect to a simultaneous Dial("tcp", addr2, addr1) run on the machine
    // at addr2, without either machine executing Listen. If laddr == nil,
    // it means we want the kernel to pick an appropriate originating local
    // address. Some Linux kernels cycle blindly through a fixed range of
    // local ports, regardless of destination port. If a kernel happens to
    // pick local port 50001 as the source for a Dial("tcp", "", "localhost:50001"),
    // then the Dial will succeed, having simultaneously connected to itself.
    // This can only happen when we are letting the kernel pick a port (laddr == nil)
    // and when there is no listener for the destination address.
    // It's hard to argue this is anything other than a kernel bug. If we
    // see this happen, rather than expose the buggy effect to users, we
    // close the fd and try again. If it happens twice more, we relent and
    // use the result. See also:
    //	https://golang.org/issue/2690
    //	https://stackoverflow.com/questions/4949858/
    //
    // The opposite can also happen: if we ask the kernel to pick an appropriate
    // originating local address, sometimes it picks one that is already in use.
    // So if the error is EADDRNOTAVAIL, we have to try again too, just for
    // a different reason.
    //
    // The kernel socket code is no doubt enjoying watching us squirm.
    for (nint i = 0; i < 2 && (Ꮡladdr == nil || laddr.Port == 0) && (selfConnect(ref (fd).DerefOrNull(), err) || spuriousENOTAVAIL(err)); i++) {
        if (err == default!) {
            fd.Close();
        }
        (fd, err) = internetSocket(ctx, sd.network, new TCPAddrжΔsockaddr(Ꮡladdr), new TCPAddrжΔsockaddr(Ꮡraddr), syscall.SOCK_STREAM, proto, dialˢ, ctrlCtxFn);
    }
    if (err != default!) {
        return (default!, err);
    }
    return (newTCPConn(fd, sd.Dialer.KeepAlive, sd.Dialer.KeepAliveConfig, testPreHookSetKeepAlive, testHookSetKeepAlive), default!);
}

internal static bool selfConnect(ref netFD fd, error err) {
    // If the connect failed, we clearly didn't connect to ourselves.
    if (err != default!) {
        return false;
    }
    // The socket constructor can return an fd with raddr nil under certain
    // unknown conditions. The errors in the calls there to Getpeername
    // are discarded, but we can't catch the problem there because those
    // calls are sometimes legally erroneous with a "socket not connected".
    // Since this code (selfConnect) is already trying to work around
    // a problem, we make sure if this happens we recognize trouble and
    // ask the DialTCP routine to try again.
    // TODO: try to understand what's really going on.
    if (fd.laddr == default! || fd.raddr == default!) {
        return true;
    }
    var l = fd.laddr._<ж<TCPAddr>>();
    var r = fd.raddr._<ж<TCPAddr>>();
    return (~l).Port == (~r).Port && (~l).IP.Equal((~r).IP);
}

internal static bool spuriousENOTAVAIL(error err) {
    {
        var (op, ok) = err._<ж<OpError>>(ᐧ); if (ok) {
            err = op.Value.Err;
        }
    }
    {
        var (sys, ok) = err._<ж<os.SyscallError>>(ᐧ); if (ok) {
            err = sys.Value.Err;
        }
    }
    return AreEqual(err, syscall.EADDRNOTAVAIL);
}

internal static bool ok(this ж<TCPListener> Ꮡln) {
    ref var ln = ref Ꮡln.DerefOrNull();

    return Ꮡln != nil && ln.fd != nil;
}

internal static (ж<TCPConn>, error) accept(this ref TCPListener ln) {
    var (fd, err) = ln.fd.accept();
    if (err != default!) {
        return (default!, err);
    }
    return (newTCPConn(fd, ln.lc.KeepAlive, ln.lc.KeepAliveConfig, testPreHookSetKeepAlive, testHookSetKeepAlive), default!);
}

internal static error close(this ref TCPListener ln) {
    return ln.fd.Close();
}

internal static (ж<os.File>, error) @file(this ref TCPListener ln) {
    var (f, err) = ln.fd.dup();
    if (err != default!) {
        return (default!, err);
    }
    return (f, default!);
}

internal static (ж<TCPListener>, error) listenTCP(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> Ꮡladdr) {
    return Ꮡsl.listenTCPProto(ctx, Ꮡladdr, 0);
}

internal static (ж<TCPListener>, error) listenTCPProto(this ж<sysListener> Ꮡsl, context.Context ctx, ж<TCPAddr> Ꮡladdr, nint proto) {
    ref var sl = ref Ꮡsl.DerefOrNull();

    Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn = default!;
    if (sl.ListenConfig.Control != default!) {
        ctrlCtxFn = (context.Context ctxΔ1, @string network, @string address, syscall.RawConn c) => Ꮡsl.Value.ListenConfig.Control(network, address, c);
    }
    var (fd, err) = internetSocket(ctx, sl.network, new TCPAddrжΔsockaddr(Ꮡladdr), default!, syscall.SOCK_STREAM, proto, listenˢ, ctrlCtxFn);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new TCPListener(fd: fd, lc: sl.ListenConfig)), default!);
}

} // end net_package
