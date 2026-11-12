// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using poll = @internal.poll_package;
using windows = @internal.syscall.windows_package;
using os = os_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;
using time = time_package;
using static net_package;

partial class net_package {

internal static ref readonly @string readSyscallName => ref readSyscallNameᵇ;
private static readonly @string readSyscallNameᵇ = "wsarecv"u8;
internal static ref readonly @string readFromSyscallName => ref readFromSyscallNameᵇ;
private static readonly @string readFromSyscallNameᵇ = "wsarecvfrom"u8;
internal static ref readonly @string readMsgSyscallName => ref readMsgSyscallNameᵇ;
private static readonly @string readMsgSyscallNameᵇ = "wsarecvmsg"u8;
internal static ref readonly @string writeSyscallName => ref writeSyscallNameᵇ;
private static readonly @string writeSyscallNameᵇ = "wsasend"u8;
internal static ref readonly @string writeToSyscallName => ref writeToSyscallNameᵇ;
private static readonly @string writeToSyscallNameᵇ = "wsasendto"u8;
internal static ref readonly @string writeMsgSyscallName => ref writeMsgSyscallNameᵇ;
private static readonly @string writeMsgSyscallNameᵇ = "wsasendmsg"u8;

[GoInit] internal static void init() {
    poll.InitWSA();
}

// canUseConnectEx reports whether we can use the ConnectEx Windows API call
// for the given network type.
internal static bool canUseConnectEx(@string net) {
    var exprᴛ1 = net;
    switch (ᐧ) {
    case {} when exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8: {
        return true;
    }}
    // ConnectEx windows API does not support connectionless sockets.
    return false;
}

internal static (ж<netFD>, error) newFD(syscall.Handle sysfd, nint family, nint sotype, @string net) {
    var ret = Ꮡ(new netFD(
        pfd: new poll.FD(
            Sysfd: sysfd,
            IsStream: sotype == syscall.SOCK_STREAM,
            ZeroReadIsEOF: sotype != syscall.SOCK_DGRAM && sotype != syscall.SOCK_RAW
        ),
        family: family,
        sotype: sotype,
        net: net
    ));
    return (ret, default);
}

} partial class netᴛnetFD {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string wsaioctlˢ => ref wsaioctlˢᵇ;
private static readonly @string wsaioctlˢᵇ = "wsaioctl"u8;

internal static error init(this ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    {
        var err = Ꮡfd.Ꮡpfd.Init(fd.net, true); if (err != nil) {
            return err;
        }
    }
    var exprᴛ1 = fd.net;
    switch (ᐧ) {
    case {} when exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8: {
        // Disable reporting of PORT_UNREACHABLE errors.
        // See https://go.dev/issue/5834.
        ref var ret = ref heap<uint32>((uint32)0, out var Ꮡret);
        ref var flag = ref heap<uint32>((uint32)0, out var Ꮡflag);
        var size = (uint32)/* unsafe.Sizeof(flag) */ (uintptr)4;
        var err = syscall.WSAIoctl(fd.pfd.Sysfd, syscall.SIO_UDP_CONNRESET, Ꮡflag.Reinterpret<uint32, byte>(), size, nil, 0, Ꮡret, nil, 0);
        if (err != nil) {
            return wrapSyscallError(wsaioctlˢ, err);
        }
        // Disable reporting of NET_UNREACHABLE errors.
        // See https://go.dev/issue/68614.
        ret = 0;
        flag = 0;
        size = (uint32)/* unsafe.Sizeof(flag) */ (uintptr)4;
        err = syscall.WSAIoctl(fd.pfd.Sysfd, windows.SIO_UDP_NETRESET, Ꮡflag.Reinterpret<uint32, byte>(), size, nil, 0, Ꮡret, nil, 0);
        if (err != nil) {
            return wrapSyscallError(wsaioctlˢ, err);
        }
        break;
    }}
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string connectˢ => ref connectˢᵇ;
private static readonly @string connectˢᵇ = "connect"u8;
internal static ref readonly @string bindˢ => ref bindˢᵇ;
private static readonly @string bindˢᵇ = "bind"u8;
internal static ref readonly @string connectexˢ => ref connectexˢᵇ;
private static readonly @string connectexˢᵇ = "connectex"u8;
internal static ref readonly @string setsockoptˢ => ref setsockoptˢᵇ;
private static readonly @string setsockoptˢᵇ = "setsockopt"u8;

// Always returns nil for connected peer address result.
internal static (syscall.Sockaddr, error) connect(this ж<netFD> Ꮡfd, context.Context ctx, syscall.Sockaddr la, syscall.Sockaddr ra) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        // Do not need to call fd.writeLock here,
        // because fd is not yet accessible to user,
        // so no concurrent operations are possible.
        {
            var err = Ꮡfd.init(); if (err != nil) {
                return (default, err);
            }
        }

        if (ctx.Done() != nil) {
            // Propagate the Context's deadline and cancellation.
            // If the context is already done, or if it has a nonzero deadline,
            // ensure that that is applied before the call to ConnectEx begins
            // so that we don't return spurious connections.
            defer(Ꮡfd.Ꮡpfd.SetWriteDeadline, noDeadline, ref ᒐ);

            if (ctx.Err() != nil) {
                Ꮡfd.Ꮡpfd.SetWriteDeadline(aLongTimeAgo);
            } else {
                var (deadline, ok) = ctx.Deadline(); if (ok && !deadline.IsZero()) {
                    Ꮡfd.Ꮡpfd.SetWriteDeadline(deadline);
                }

                var done = new channel<EmptyStruct>(0);
                var doneʗ1 = done;
                var stop = context.AfterFunc(ctx, () => {
                    // Force the runtime's poller to immediately give
                    // up waiting for writability.
                    Ꮡfd.Ꮡpfd.SetWriteDeadline(aLongTimeAgo);
                    close(doneʗ1);
                });
                var doneʗ2 = done;
                var stopʗ1 = stop;
                defer(() => {
                    if (!stopʗ1()) {
                        // Wait for the call to SetWriteDeadline to complete so that we can
                        // reset the deadline if everything else succeeded.
                        ᐸꟷ(doneʗ2);
                    }
                }, ref ᒐ);
            }
        }

        if (!canUseConnectEx(fd.net)) {
            var err = connectFunc(fd.pfd.Sysfd, ra);
            return (default, os.NewSyscallError(connectˢ, err));
        }
        // ConnectEx windows API requires an unconnected, previously bound socket.
        if (la == nil) {
            switch (ra.type()) {
            case ж<syscall.SockaddrInet4>: {
                la = new syscall.SockaddrInet4жSockaddr(Ꮡ(new syscall.SockaddrInet4()));
                break;
            }
            case ж<syscall.SockaddrInet6>: {
                la = new syscall.SockaddrInet6жSockaddr(Ꮡ(new syscall.SockaddrInet6()));
                break;
            }
            default: {
                throw panic("unexpected type in connect");
                break;
            }}
            var err = syscall.Bind(fd.pfd.Sysfd, la); if (err != nil) {
                return (default, os.NewSyscallError(bindˢ, err));
            }
        }

        bool isloopback = default;
        switch (ra.type()) {
        case ж<syscall.SockaddrInet4> raΔ1: {
            isloopback = raΔ1.Addr[0] == 127;
            break;
        }
        case ж<syscall.SockaddrInet6> raΔ1: {
            isloopback = raΔ1.Addr == new array<byte>(IPv6loopback, 16);
            break;
        }
        default: {
            var raΔ1 = ra;
            throw panic("unexpected type in connect");
            break;
        }}
        if (isloopback) {
            // This makes ConnectEx() fails faster if the target port on the localhost
            // is not reachable, instead of waiting for 2s.
            ref var @params = ref heap<windows.TCP_INITIAL_RTO_PARAMETERS>(new windows.TCP_INITIAL_RTO_PARAMETERS(
                Rtt: windows.TCP_INITIAL_RTO_UNSPECIFIED_RTT, // use the default or overridden by the Administrator
                MaxSynRetransmissions: 1                      // minimum possible value before Windows 10.0.16299
            ), out var Ꮡparams);
            if (windows.SupportTCPInitialRTONoSYNRetransmissions()) {
                // In Windows 10.0.16299 TCP_INITIAL_RTO_NO_SYN_RETRANSMISSIONS makes ConnectEx() fails instantly.
                @params.MaxSynRetransmissions = windows.TCP_INITIAL_RTO_NO_SYN_RETRANSMISSIONS;
            }
            ref var @out = ref heap(new uint32(), out var Ꮡout);
            // Don't abort the connection if WSAIoctl fails, as it is only an optimization.
            // If it fails reliably, we expect TestDialClosedPortFailFast to detect it.
            _ = Ꮡfd.Ꮡpfd.WSAIoctl(windows.SIO_TCP_INITIAL_RTO, Ꮡparams.Reinterpret<windows.TCP_INITIAL_RTO_PARAMETERS, byte>(), (uint32)/* unsafe.Sizeof(params) */ (uintptr)4, nil, 0, Ꮡout, nil, 0);
        }

        // Call ConnectEx API.
        {
            var err = Ꮡfd.Ꮡpfd.ConnectEx(ra); if (err != nil) {
                var selᴛ9 = ctx.Done();
                switch (trySelect(ᐸꟷ(selᴛ9, ꓸꓸꓸ))) {
                case 0 when selᴛ9.ꟷᐳ(out _): {
                    return (default, mapErr(ctx.Err()));
                }
                default: {
                    var (_, ok) = err._<syscall.Errno>(ᐧ); if (ok) {
                        err = os.NewSyscallError(connectexˢ, err);
                    }
                    return (default, err);
                }}
            }
        }
        // Refresh socket properties.
        return (default, os.NewSyscallError(setsockoptˢ, syscall.Setsockopt(fd.pfd.Sysfd, syscall.SOL_SOCKET, syscall.SO_UPDATE_CONNECT_CONTEXT, Ꮡfd.Ꮡpfd.ᏑSysfd.Reinterpret<syscall.Handle, byte>(), (int32)/* unsafe.Sizeof(fd.pfd.Sysfd) */ (uintptr)8)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} partial class netᴛconn {

internal static (int64, error) writeBuffers(this ж<conn> Ꮡc, ж<Buffers> v) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!Ꮡc.ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.fd.writeBuffers(v);
    if (err != nil) {
        return (n, new OpErrorжerror(Ꮡ(new OpError(Op: "wsasend", Net: c.fd.net, Source: c.fd.laddr, Addr: c.fd.raddr, Err: err))));
    }
    return (n, default);
}

} partial class netᴛnetFD {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string wsasendˢ => ref wsasendˢᵇ;
private static readonly @string wsasendˢᵇ = "wsasend"u8;

internal static (int64, error) writeBuffers(this ж<netFD> Ꮡfd, ж<Buffers> buf) {
    var (n, err) = Ꮡfd.Ꮡpfd.Writev(buf.of(Buffers.Ꮡm_value));
    runtime.KeepAlive(Ꮡfd.OrTypedNil());
    return (n, wrapSyscallError(wsasendˢ, err));
}

internal static (ж<netFD>, error) accept(this ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    var (s, rawsa, rsan, errcall, err) = Ꮡfd.Ꮡpfd.Accept(() => sysSocket(Ꮡfd.Value.family, Ꮡfd.Value.sotype, 0));

    if (err != nil) {
        if (errcall != ""u8) {
            err = wrapSyscallError(errcall, err);
        }
        return (default, err);
    }

    // Associate our new socket with IOCP.
    (var netfd, err) = newFD(s, fd.family, fd.sotype, fd.net);
    if (err != nil) {
        poll.CloseFunc(s);
        return (default, err);
    }
    var errΔ1 = netfd.init(); if (errΔ1 != nil) {
        Ꮡfd.Close();
        return (default, errΔ1);
    }

    // Get local and peer addr out of AcceptEx buffer.
    ref var lrsa = ref heap<ж<syscall.RawSockaddrAny>>(out var Ꮡlrsa);
    ref var rrsa = ref heap<ж<syscall.RawSockaddrAny>>(out var Ꮡrrsa);
    ref var llen = ref heap(new int32(), out var Ꮡllen);
    ref var rlen = ref heap(new int32(), out var Ꮡrlen);
    syscall.GetAcceptExSockaddrs(Ꮡ(rawsa, 0).Reinterpret<syscall.RawSockaddrAny, byte>(),
        0, rsan, rsan, Ꮡlrsa, Ꮡllen, Ꮡrrsa, Ꮡrlen);
    var (lsa, _) = lrsa.Sockaddr();
    var (rsa, _) = rrsa.Sockaddr();

    netfd.setAddr(netfd.addrFunc()(lsa), netfd.addrFunc()(rsa));
    return (netfd, default);
}

} partial class net_package {

// Defined in os package.
/*linkname*/ internal static partial ж<os.File> newWindowsFile(syscall.Handle h, @string name) {
    return os.net_newWindowsFile((uintptr)h, name);
}

} partial class netᴛnetFD {

internal static (ж<os.File>, error) dup(this ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    // Disassociate the IOCP from the socket,
    // it is not safe to share a duplicated handle
    // that is associated with IOCP.
    var errΔ1 = Ꮡfd.Ꮡpfd.DisassociateIOCP(); if (errΔ1 != nil) {
        return (default, errΔ1);
    }
    syscall.Handle h = default;
    ref var syserr = ref heap<error>(out var Ꮡsyserr);
    var err = Ꮡfd.Ꮡpfd.RawControl((uintptr fdΔ1) => {
        (h, Ꮡsyserr.ValueSlot) = dupSocket((syscall.Handle)fdΔ1);
    });
    if (err == nil) {
        err = syserr;
    }
    if (err != nil) {
        return (default, err);
    }
    // All WSASocket calls must be match with a syscall.Closesocket call,
    // but os.NewFile calls syscall.CloseHandle instead. We need to use
    // a hidden function so that the returned file is aware of this fact.
    return (newWindowsFile(h, fd.name()), default);
}

} // end netᴛnetFD
