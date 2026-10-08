// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go;

using context = context_package;
using poll = @internal.poll_package;
using os = os_package;
using syscall = syscall_package;
using @internal;

partial class net_package {

// socket returns a network file descriptor that is ready for
// asynchronous I/O using the network poller.
internal static (ж<netFD> fd, error err) socket(context.Context ctx, @string net, nint family, nint sotype, nint proto, bool ipv6only, Δsockaddr laddr, Δsockaddr raddr, Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn) {
    ж<netFD> fd = default!;
    error err = default!;

    (var s, err) = sysSocket(family, sotype, proto);
    if (err != default!) {
        return (default!, err);
    }
    {
        err = setDefaultSockopts(s, family, sotype, ipv6only); if (err != default!) {
            poll.CloseFunc(s);
            return (default!, err);
        }
    }
    {
        (fd, err) = newFD(s, family, sotype, net); if (err != default!) {
            poll.CloseFunc(s);
            return (default!, err);
        }
    }
    // This function makes a network file descriptor for the
    // following applications:
    //
    // - An endpoint holder that opens a passive stream
    //   connection, known as a stream listener
    //
    // - An endpoint holder that opens a destination-unspecific
    //   datagram connection, known as a datagram listener
    //
    // - An endpoint holder that opens an active stream or a
    //   destination-specific datagram connection, known as a
    //   dialer
    //
    // - An endpoint holder that opens the other connection, such
    //   as talking to the protocol stack inside the kernel
    //
    // For stream and datagram listeners, they will only require
    // named sockets, so we can assume that it's just a request
    // from stream or datagram listeners when laddr is not nil but
    // raddr is nil. Otherwise we assume it's just for dialers or
    // the other connection holders.
    if (laddr != default! && raddr == default!) {
        var exprᴛ1 = sotype;
        if (exprᴛ1 == syscall.SOCK_STREAM || exprᴛ1 == syscall.SOCK_SEQPACKET) {
            {
                var errΔ3 = fd.listenStream(ctx, laddr, listenerBacklog(), ctrlCtxFn); if (errΔ3 != default!) {
                    fd.Close();
                    return (default!, errΔ3);
                }
            }
            return (fd, default!);
        }
        if (exprᴛ1 == syscall.SOCK_DGRAM) {
            {
                var errΔ4 = fd.listenDatagram(ctx, laddr, ctrlCtxFn); if (errΔ4 != default!) {
                    fd.Close();
                    return (default!, errΔ4);
                }
            }
            return (fd, default!);
        }

    }
    {
        var errΔ5 = fd.dial(ctx, laddr, raddr, ctrlCtxFn); if (errΔ5 != default!) {
            fd.Close();
            return (default!, errΔ5);
        }
    }
    return (fd, default!);
}

internal static @string ctrlNetwork(this ref netFD fd) {
    var exprᴛ1 = fd.net;
    if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
        return fd.net;
    }

    switch (fd.net[len(fd.net) - 1]) {
    case (rune)'4' or (rune)'6': {
        return fd.net;
    }}

    if (fd.family == syscall.AF_INET) {
        return fd.net + "4"u8;
    }
    return fd.net + "6"u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bindˢ = "bind"u8;

internal static error dial(this ж<netFD> Ꮡfd, context.Context ctx, Δsockaddr laddr, Δsockaddr raddr, Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    ж<rawConn> c = default!;
    if (ctrlCtxFn != default!) {
        c = newRawConn(Ꮡfd);
        @string ctrlAddr = default!;
        if (raddr != default!){
            ctrlAddr = raddr.String();
        } else 
        if (laddr != default!) {
            ctrlAddr = laddr.String();
        }
        {
            var errΔ1 = ctrlCtxFn(ctx, fd.ctrlNetwork(), ctrlAddr, new rawConnжRawConn(c)); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
    }
    syscall.Sockaddr lsa = default!;
    error err = default!;
    if (laddr != default!) {
        {
            (lsa, err) = laddr.sockaddr(fd.family); if (err != default!){
                return err;
            } else 
            if (lsa != default!) {
                {
                    err = syscall.Bind(fd.pfd.Sysfd, lsa); if (err != default!) {
                        return os.NewSyscallError(bindˢ, err);
                    }
                }
            }
        }
    }
    syscall.Sockaddr rsa = default!;                                // remote address from the user
    syscall.Sockaddr crsa = default!;                              // remote address we actually connected to
    if (raddr != default!){
        {
            (rsa, err) = raddr.sockaddr(fd.family); if (err != default!) {
                return err;
            }
        }
        {
            (crsa, err) = Ꮡfd.connect(ctx, lsa, rsa); if (err != default!) {
                return err;
            }
        }
        fd.isConnected = true;
    } else {
        {
            var errΔ2 = Ꮡfd.init(); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    // Record the local and remote addresses from the actual socket.
    // Get the local address by calling Getsockname.
    // For the remote address, use
    // 1) the one returned by the connect method, if any; or
    // 2) the one from Getpeername, if it succeeds; or
    // 3) the one passed to us as the raddr parameter.
    (lsa, _) = syscall.Getsockname(fd.pfd.Sysfd);
    if (crsa != default!){
        Ꮡfd.setAddr(fd.addrFunc()(lsa), fd.addrFunc()(crsa));
    } else 
    {
        (rsa, _) = syscall.Getpeername(fd.pfd.Sysfd); if (rsa != default!){
            Ꮡfd.setAddr(fd.addrFunc()(lsa), fd.addrFunc()(rsa));
        } else {
            Ꮡfd.setAddr(fd.addrFunc()(lsa), raddr);
        }
    }
    return default!;
}

internal static error listenStream(this ж<netFD> Ꮡfd, context.Context ctx, Δsockaddr laddr, nint backlog, Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    error err = default!;
    {
        err = setDefaultListenerSockopts(fd.pfd.Sysfd); if (err != default!) {
            return err;
        }
    }
    syscall.Sockaddr lsa = default!;
    {
        (lsa, err) = laddr.sockaddr(fd.family); if (err != default!) {
            return err;
        }
    }
    if (ctrlCtxFn != default!) {
        var c = newRawConn(Ꮡfd);
        {
            var errΔ1 = ctrlCtxFn(ctx, fd.ctrlNetwork(), laddr.String(), new rawConnжRawConn(c)); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
    }
    {
        err = syscall.Bind(fd.pfd.Sysfd, lsa); if (err != default!) {
            return os.NewSyscallError(bindˢ, err);
        }
    }
    {
        err = listenFunc(fd.pfd.Sysfd, backlog); if (err != default!) {
            return os.NewSyscallError(listenˢ, err);
        }
    }
    {
        err = Ꮡfd.init(); if (err != default!) {
            return err;
        }
    }
    (lsa, _) = syscall.Getsockname(fd.pfd.Sysfd);
    Ꮡfd.setAddr(fd.addrFunc()(lsa), default!);
    return default!;
}

internal static error listenDatagram(this ж<netFD> Ꮡfd, context.Context ctx, Δsockaddr laddr, Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    switch (laddr.type()) {
    case ж<UDPAddr> addr: {
        if ((~addr).IP != default! && (~addr).IP.IsMulticast()) {
            // We provide a socket that listens to a wildcard
            // address with reusable UDP port when the given laddr
            // is an appropriate UDP multicast address prefix.
            // This makes it possible for a single UDP listener to
            // join multiple different group addresses, for
            // multiple UDP listeners that listen on the same UDP
            // port to join the same group address.
            {
                var errΔ1 = setDefaultMulticastSockopts(fd.pfd.Sysfd); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
            ref var addrΔ1 = ref heap<UDPAddr>(out var ᏑaddrΔ1);
            addrΔ1 = addr.Value;
            var exprᴛ1 = fd.family;
            if (exprᴛ1 == syscall.AF_INET) {
                addrΔ1.IP = IPv4zero;
            }
            else if (exprᴛ1 == syscall.AF_INET6) {
                addrΔ1.IP = IPv6unspecified;
            }

            laddr = new UDPAddrжΔsockaddr(ᏑaddrΔ1);
        }
        break;
    }}
    error err = default!;
    syscall.Sockaddr lsa = default!;
    {
        (lsa, err) = laddr.sockaddr(fd.family); if (err != default!) {
            return err;
        }
    }
    if (ctrlCtxFn != default!) {
        var c = newRawConn(Ꮡfd);
        {
            var errΔ2 = ctrlCtxFn(ctx, fd.ctrlNetwork(), laddr.String(), new rawConnжRawConn(c)); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    {
        err = syscall.Bind(fd.pfd.Sysfd, lsa); if (err != default!) {
            return os.NewSyscallError(bindˢ, err);
        }
    }
    {
        err = Ꮡfd.init(); if (err != default!) {
            return err;
        }
    }
    (lsa, _) = syscall.Getsockname(fd.pfd.Sysfd);
    Ꮡfd.setAddr(fd.addrFunc()(lsa), default!);
    return default!;
}

} // end net_package
