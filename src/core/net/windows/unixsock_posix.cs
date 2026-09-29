// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1 || windows
namespace go;

using context = context_package;
using errors = errors_package;
using os = os_package;
using syscall = syscall_package;

partial class net_package {

internal static (ж<netFD>, error) unixSocket(context.Context ctx, @string net, Δsockaddr laddr, Δsockaddr raddr, @string mode, Func<context.Context, @string, @string, syscall.RawConn, error> ctxCtrlFn) {
    nint sotype = default!;
    var exprᴛ1 = net;
    if (exprᴛ1 == "unix"u8) {
        sotype = syscall.SOCK_STREAM;
    }
    else if (exprᴛ1 == "unixgram"u8) {
        sotype = syscall.SOCK_DGRAM;
    }
    else if (exprᴛ1 == "unixpacket"u8) {
        sotype = syscall.SOCK_SEQPACKET;
    }
    else { /* default: */
        return (default!, ((UnknownNetworkError)net));
    }

    var exprᴛ2 = mode;
    if (exprᴛ2 == "dial"u8) {
        if (laddr != default! && laddr.isWildcard()) {
            laddr = default!;
        }
        if (raddr != default! && raddr.isWildcard()) {
            raddr = default!;
        }
        if (raddr == default! && (sotype != syscall.SOCK_DGRAM || laddr == default!)) {
            return (default!, errMissingAddress);
        }
    }
    else if (exprᴛ2 == "listen"u8) {
    }
    else { /* default: */
        return (default!, errors.New("unknown mode: "u8 + mode));
    }

    var (fd, err) = socket(ctx, net, syscall.AF_UNIX, sotype, 0, false, laddr, raddr, ctxCtrlFn);
    if (err != default!) {
        return (default!, err);
    }
    return (fd, default!);
}

internal static ΔAddr sockaddrToUnix(syscallꓸSockaddr sa) {
    {
        var (s, ok) = sa._<ж<syscall.SockaddrUnix>>(ᐧ); if (ok) {
            return new UnixAddrжΔAddr(Ꮡ(new UnixAddr(Name: (~s).Name, Net: "unix"u8)));
        }
    }
    return default!;
}

internal static ΔAddr sockaddrToUnixgram(syscallꓸSockaddr sa) {
    {
        var (s, ok) = sa._<ж<syscall.SockaddrUnix>>(ᐧ); if (ok) {
            return new UnixAddrжΔAddr(Ꮡ(new UnixAddr(Name: (~s).Name, Net: "unixgram"u8)));
        }
    }
    return default!;
}

internal static ΔAddr sockaddrToUnixpacket(syscallꓸSockaddr sa) {
    {
        var (s, ok) = sa._<ж<syscall.SockaddrUnix>>(ᐧ); if (ok) {
            return new UnixAddrжΔAddr(Ꮡ(new UnixAddr(Name: (~s).Name, Net: "unixpacket"u8)));
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unixˢ = "unix"u8;
internal static readonly @string unixgramˢ = "unixgram"u8;
internal static readonly @string unixpacketˢ = "unixpacket"u8;

internal static @string sotypeToNet(nint sotype) {
    var exprᴛ1 = sotype;
    if (exprᴛ1 == syscall.SOCK_STREAM) {
        return unixˢ;
    }
    if (exprᴛ1 == syscall.SOCK_DGRAM) {
        return unixgramˢ;
    }
    if (exprᴛ1 == syscall.SOCK_SEQPACKET) {
        return unixpacketˢ;
    }
    { /* default: */
        throw panic("sotypeToNet unknown socket type");
    }

}

[GoRecv] internal static nint family(this ref UnixAddr a) {
    return syscall.AF_UNIX;
}

internal static (syscallꓸSockaddr, error) sockaddr(this ж<UnixAddr> Ꮡa, nint family) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil) {
        return (default!, default!);
    }
    return (new syscall.SockaddrUnixжΔSockaddr(Ꮡ(new syscall.SockaddrUnix(Name: a.Name))), default!);
}

internal static Δsockaddr toLocal(this ж<UnixAddr> Ꮡa, @string net) {
    return new UnixAddrжΔsockaddr(Ꮡa);
}

[GoRecv] internal static (nint, ж<UnixAddr>, error) readFrom(this ref UnixConn c, slice<byte> b) {
    ж<UnixAddr> addr = default!;
    var (n, sa, err) = c.fd.readFrom(b);
    switch (sa.type()) {
    case ж<syscall.SockaddrUnix> saΔ1: {
        if ((~saΔ1).Name != ""u8) {
            addr = Ꮡ(new UnixAddr(Name: (~saΔ1).Name, Net: sotypeToNet((~c.fd).sotype)));
        }
        break;
    }}
    return (n, addr, err);
}

[GoRecv] internal static (nint n, nint oobn, nint flags, ж<UnixAddr> addr, error err) readMsg(this ref UnixConn c, slice<byte> b, slice<byte> oob) {
    nint n = default!;
    nint oobn = default!;
    nint flags = default!;
    ж<UnixAddr> addr = default!;
    error err = default!;

    syscallꓸSockaddr sa = default!;
    (n, oobn, flags, sa, err) = c.fd.readMsg(b, oob, readMsgFlags);
    if (readMsgFlags == 0 && err == default! && oobn > 0) {
        setReadMsgCloseOnExec(oob.slice(0, oobn));
    }
    switch (sa.type()) {
    case ж<syscall.SockaddrUnix> saΔ1: {
        if ((~saΔ1).Name != ""u8) {
            addr = Ꮡ(new UnixAddr(Name: (~saΔ1).Name, Net: sotypeToNet((~c.fd).sotype)));
        }
        break;
    }}
    return (n, oobn, flags, addr, err);
}

[GoRecv] internal static (nint, error) writeTo(this ref UnixConn c, slice<byte> b, ж<UnixAddr> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.DerefOrNull();

    if ((~c.fd).isConnected) {
        return (0, ErrWriteToConnected);
    }
    if (Ꮡaddr == nil) {
        return (0, errMissingAddress);
    }
    if (addr.Net != sotypeToNet((~c.fd).sotype)) {
        return (0, syscall.EAFNOSUPPORT);
    }
    var sa = Ꮡ(new syscall.SockaddrUnix(Name: addr.Name));
    return c.fd.writeTo(b, new syscall.SockaddrUnixжΔSockaddr(sa));
}

[GoRecv] internal static (nint n, nint oobn, error err) writeMsg(this ref UnixConn c, slice<byte> b, slice<byte> oob, ж<UnixAddr> Ꮡaddr) {
    ref var addr = ref Ꮡaddr.DerefOrNull();

    if ((~c.fd).sotype == syscall.SOCK_DGRAM && (~c.fd).isConnected) {
        return (0, 0, ErrWriteToConnected);
    }
    syscallꓸSockaddr sa = default!;
    if (Ꮡaddr != nil) {
        if (addr.Net != sotypeToNet((~c.fd).sotype)) {
            return (0, 0, syscall.EAFNOSUPPORT);
        }
        sa = new syscall.SockaddrUnixжΔSockaddr(Ꮡ(new syscall.SockaddrUnix(Name: addr.Name)));
    }
    return c.fd.writeMsg(b, oob, sa);
}

internal static (ж<UnixConn>, error) dialUnix(this ж<sysDialer> Ꮡsd, context.Context ctx, ж<UnixAddr> Ꮡladdr, ж<UnixAddr> Ꮡraddr) {
    ref var sd = ref Ꮡsd.DerefOrNull();

    var ctrlCtxFn = sd.Dialer.ControlContext;
    if (ctrlCtxFn == default! && sd.Dialer.Control != default!) {
        ctrlCtxFn = (context.Context ctxΔ1, @string network, @string address, syscall.RawConn c) => Ꮡsd.Value.Dialer.Control(network, address, c);
    }
    var (fd, err) = unixSocket(ctx, sd.network, new UnixAddrжΔsockaddr(Ꮡladdr), new UnixAddrжΔsockaddr(Ꮡraddr), dialˢ, ctrlCtxFn);
    if (err != default!) {
        return (default!, err);
    }
    return (newUnixConn(fd), default!);
}

[GoRecv] internal static (ж<UnixConn>, error) accept(this ref UnixListener ln) {
    var (fd, err) = ln.fd.accept();
    if (err != default!) {
        return (default!, err);
    }
    return (newUnixConn(fd), default!);
}

internal static error close(this ж<UnixListener> Ꮡln) {
    ref var ln = ref Ꮡln.DerefOrNull();

    // The operating system doesn't clean up
    // the file that announcing created, so
    // we have to clean it up ourselves.
    // There's a race here--we can't know for
    // sure whether someone else has come along
    // and replaced our socket name already--
    // but this sequence (remove then close)
    // is at least compatible with the auto-remove
    // sequence in ListenUnix. It's only non-Go
    // programs that can mess us up.
    // Even if there are racy calls to Close, we want to unlink only for the first one.
    Ꮡln.of(UnixListener.ᏑunlinkOnce).Do(() => {
        if (Ꮡln.Value.path[0] != (rune)'@' && Ꮡln.Value.unlink) {
            syscall.Unlink(Ꮡln.Value.path);
        }
    });
    return ln.fd.Close();
}

[GoRecv] internal static (ж<os.File>, error) @file(this ref UnixListener ln) {
    var (f, err) = ln.fd.dup();
    if (err != default!) {
        return (default!, err);
    }
    return (f, default!);
}

// SetUnlinkOnClose sets whether the underlying socket file should be removed
// from the file system when the listener is closed.
//
// The default behavior is to unlink the socket file only when package net created it.
// That is, when the listener and the underlying socket file were created by a call to
// Listen or ListenUnix, then by default closing the listener will remove the socket file.
// but if the listener was created by a call to FileListener to use an already existing
// socket file, then by default closing the listener will not remove the socket file.
[GoRecv] public static void SetUnlinkOnClose(this ref UnixListener l, bool unlink) {
    l.unlink = unlink;
}

internal static (ж<UnixListener>, error) listenUnix(this ж<sysListener> Ꮡsl, context.Context ctx, ж<UnixAddr> Ꮡladdr) {
    ref var sl = ref Ꮡsl.DerefOrNull();
    ref var laddr = ref Ꮡladdr.DerefOrNull();

    Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn = default!;
    if (sl.ListenConfig.Control != default!) {
        ctrlCtxFn = (context.Context ctxΔ1, @string network, @string address, syscall.RawConn c) => Ꮡsl.Value.ListenConfig.Control(network, address, c);
    }
    var (fd, err) = unixSocket(ctx, sl.network, new UnixAddrжΔsockaddr(Ꮡladdr), default!, listenˢ, ctrlCtxFn);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new UnixListener(fd: fd, path: (~fd).laddr.String(), unlink: true)), default!);
}

internal static (ж<UnixConn>, error) listenUnixgram(this ж<sysListener> Ꮡsl, context.Context ctx, ж<UnixAddr> Ꮡladdr) {
    ref var sl = ref Ꮡsl.DerefOrNull();

    Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn = default!;
    if (sl.ListenConfig.Control != default!) {
        ctrlCtxFn = (context.Context ctxΔ1, @string network, @string address, syscall.RawConn c) => Ꮡsl.Value.ListenConfig.Control(network, address, c);
    }
    var (fd, err) = unixSocket(ctx, sl.network, new UnixAddrжΔsockaddr(Ꮡladdr), default!, listenˢ, ctrlCtxFn);
    if (err != default!) {
        return (default!, err);
    }
    return (newUnixConn(fd), default!);
}

} // end net_package
