// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δlog = log_package;
using Δos = os_package;
using filepath = path.filepath_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using @internal;
using exec = go.os.exec_package;
using go.os;
using path;
using static go.net_package;

partial class net_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sockˢ = "sock"u8;

// testUnixAddr uses os.MkdirTemp to get a name that is unique.
internal static @string testUnixAddr(testing.TB t) {
    // Pass an empty pattern to get a directory name that is as short as possible.
    // If we end up with a name longer than the sun_path field in the sockaddr_un
    // struct, we won't be able to make the syscall to open the socket.
    var (d, err) = Δos.MkdirTemp(""u8, ""u8);
    if (err != default!) {
        t.Fatal(err);
    }
    t.Cleanup(() => {
        {
            var errΔ1 = Δos.RemoveAll(d); if (errΔ1 != default!) {
                t.Error(errΔ1);
            }
        }
    });
    return filepath.Join(d, sockˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object tooManyListenConfigsˢ = (@string)"too many ListenConfigs passed to newLocalListener: want 0 or 1"u8;

internal static global::go.net_package.Listener newLocalListener(testing.TB t, @string network, params Span<ж<global::go.net_package.ListenConfig>> lcOptʗp) {
    var lcOpt = lcOptʗp.sslice();

    ж<global::go.net_package.ListenConfig> lc = default!;
    switch (len(lcOpt)) {
    case 0: {
        lc = @new<global::go.net_package.ListenConfig>();
        break;
    }
    case 1: {
        lc = lcOpt[0];
        break;
    }
    default: {
        t.Helper();
        t.Fatal(tooManyListenConfigsˢ);
        break;
    }}

    var lcʗ1 = lc;
    global::go.net_package.Listener listen(@string net, @string addr) {
        var (ln, err) = lcʗ1.Listen(context.Background(), net, addr);
        if (err != default!) {
            t.Helper();
            t.Fatal(err);
        }
        return ln;
    }
    var exprᴛ1 = network;
    if (exprᴛ1 == "tcp"u8) {
        if (supportsIPv4()) {
            return listen(tcp4ˢ, "127.0.0.1:0"u8);
        }
        if (supportsIPv6()) {
            return listen(tcp6ˢ, "[::1]:0"u8);
        }
    }
    else if (exprᴛ1 == "tcp4"u8) {
        if (supportsIPv4()) {
            return listen(tcp4ˢ, "127.0.0.1:0"u8);
        }
    }
    else if (exprᴛ1 == "tcp6"u8) {
        if (supportsIPv6()) {
            return listen(tcp6ˢ, "[::1]:0"u8);
        }
    }
    else if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixpacket"u8) {
        return listen(network, testUnixAddr(t));
    }

    t.Helper();
    t.Fatalf("%s is not supported"u8, network);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noDualstackPortAvailableˢ = "no dualstack port available"u8;

[GoType("dyn")] internal partial struct newDualStackListener_type {
    internal @string network;
    public partial ref global::go.net_package.TCPAddr TCPAddr { get; }
}

internal static (slice<ж<global::go.net_package.TCPListener>> lns, error err) newDualStackListener() {
    slice<newDualStackListener_type> args = new newDualStackListener_type[]{
        new("tcp4"u8, new TCPAddr(IP: IPv4(127, 0, 0, 1))),
        new("tcp6"u8, new TCPAddr(IP: IPv6loopback))
    }.slice();
    for (nint i = 0; i < 64; i++) {
        nint port = default!;
        slice<ж<global::go.net_package.TCPListener>> lnsΔ1 = default!;
        foreach (var (_, vᴛ1) in args) {
            ref var arg = ref heap(new newDualStackListener_type(), out var Ꮡarg);
            arg = vᴛ1;

            arg.TCPAddr.Port = port;
            var (ln, errΔ1) = ListenTCP(arg.network, Ꮡarg.of(newDualStackListener_type.ᏑTCPAddr));
            if (errΔ1 != default!) {
                continue;
            }
            port = ln.Addr()._<ж<global::go.net_package.TCPAddr>>().Value.Port;
            lnsΔ1 = append(lnsΔ1, ln);
        }
        if (len(lnsΔ1) != len(args)) {
            foreach (var (_, ln) in lnsΔ1) {
                ln.Close();
            }
            continue;
        }
        return (lnsΔ1, default!);
    }
    return (default!, errors.New(noDualstackPortAvailableˢ));
}

[GoType] internal partial struct localServer {
    internal Δsync.RWMutex lnmu;
    [GoEmbedded] public global::go.net_package.Listener Listener;
    internal channel<bool> done; // signal that indicates server stopped
    internal slice<global::go.net_package.Conn> cl; // accepted connection list
}

internal static error buildup(this ж<localServer> Ꮡls, Action<ж<localServer>, global::go.net_package.Listener> handler) {
    goǃ(() => {
        handler(Ꮡls, Ꮡls.Value.Listener);
        builtin.close(Ꮡls.Value.done);
    });
    return default!;
}

internal static error teardown(this ж<localServer> Ꮡls) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var ls = ref Ꮡls.DerefOrNull();

        Ꮡls.of(localServer.Ꮡlnmu).Lock();
        ᒐd1 = true;
        if (ls.Listener != default!) {
            @string network = ls.Listener.Addr().Network();
            @string address = ls.Listener.Addr().String();
            ls.Listener.Close();
            foreach (var (_, c) in ls.cl) {
                {
                    var err = c.Close(); if (err != default!) {
                        return err;
                    }
                }
            }
            ᐸꟷ(ls.done);
            ls.Listener = default!;
            var exprᴛ1 = network;
            if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixpacket"u8) {
                Δos.Remove(address);
            }

        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡls.of(localServer.Ꮡlnmu).Unlock(); ᒐ.Run(); }
}

internal static ж<localServer> newLocalServer(testing.TB t, @string network) {
    t.Helper();
    var ln = newLocalListener(t, network);
    return Ꮡ(new localServer(Listener: ln, done: new channel<bool>(0)));
}

[GoType] internal partial struct streamListener {
    internal @string network, address;
    [GoEmbedded] public global::go.net_package.Listener Listener;
    internal channel<bool> done; // signal that indicates server stopped
}

[GoRecv] internal static ж<localServer> newLocalServer(this ref streamListener sl) {
    return Ꮡ(new localServer(Listener: sl.Listener, done: new channel<bool>(0)));
}

[GoType] internal partial struct dualStackServer {
    internal Δsync.RWMutex lnmu;
    internal slice<streamListener> lns;
    internal @string port;
    internal Δsync.RWMutex cmu;
    internal slice<global::go.net_package.Conn> cs; // established connections at the passive open side
}

internal static error buildup(this ж<dualStackServer> Ꮡdss, Action<ж<dualStackServer>, global::go.net_package.Listener> handler) {
    ref var dss = ref Ꮡdss.DerefOrNull();

    foreach (var (i, _) in dss.lns) {
        goǃ((nint iΔ1) => {
            handler(Ꮡdss, Ꮡdss.Value.lns[iΔ1].Listener);
            builtin.close(Ꮡdss.Value.lns[iΔ1].done);
        }, i);
    }
    return default!;
}

internal static error teardownNetwork(this ж<dualStackServer> Ꮡdss, @string network) {
    ref var dss = ref Ꮡdss.DerefOrNull();

    Ꮡdss.of(dualStackServer.Ꮡlnmu).Lock();
    foreach (var (i, _) in dss.lns) {
        if (network == dss.lns[i].network && dss.lns[i].Listener != default!) {
            dss.lns[i].Listener.Close();
            ᐸꟷ(dss.lns[i].done);
            dss.lns[i].Listener = default!;
        }
    }
    Ꮡdss.of(dualStackServer.Ꮡlnmu).Unlock();
    return default!;
}

internal static error teardown(this ж<dualStackServer> Ꮡdss) {
    ref var dss = ref Ꮡdss.DerefOrNull();

    Ꮡdss.of(dualStackServer.Ꮡlnmu).Lock();
    foreach (var (i, _) in dss.lns) {
        if (dss.lns[i].Listener != default!) {
            dss.lns[i].Listener.Close();
            ᐸꟷ(dss.lns[i].done);
        }
    }
    dss.lns = dss.lns[..0];
    Ꮡdss.of(dualStackServer.Ꮡlnmu).Unlock();
    Ꮡdss.of(dualStackServer.Ꮡcmu).Lock();
    foreach (var (_, c) in dss.cs) {
        c.Close();
    }
    dss.cs = dss.cs[..0];
    Ꮡdss.of(dualStackServer.Ꮡcmu).Unlock();
    return default!;
}

internal static (ж<dualStackServer>, error) newDualStackServer() {
    var (lns, err) = newDualStackListener();
    if (err != default!) {
        return (default!, err);
    }
    ref var port = ref heap<@string>(out var Ꮡport);
    (_, port, err) = SplitHostPort(lns[0].Addr().String());
    if (err != default!) {
        lns[0].Close();
        lns[1].Close();
        return (default!, err);
    }
    return (Ꮡ(new dualStackServer(
        lns: new streamListener[]{
            new(network: "tcp4"u8, address: lns[0].Addr().String(), Listener: new global::go.net_package.TCPListenerжListener(lns[0]), done: new channel<bool>(0)),
            new(network: "tcp6"u8, address: lns[1].Addr().String(), Listener: new global::go.net_package.TCPListenerжListener(lns[1]), done: new channel<bool>(0))
        }.slice(),
        port: port
    )), default!);
}

internal static void transponder(this ж<localServer> Ꮡls, global::go.net_package.Listener ln, channel/*<-*/<error> ch) {
    GoFrame ᒐ = default;
    try {
        ref var ls = ref Ꮡls.DerefOrNull();

        defer(ᴛ1 => builtin.close(ᴛ1), ch, ref ᒐ);
        switch (ln.type()) {
        case ж<global::go.net_package.TCPListener> lnΔ1: {
            lnΔ1.SetDeadline(time.Now().Add(someTimeout));
            break;
        }
        case ж<global::go.net_package.UnixListener> lnΔ1: {
            lnΔ1.SetDeadline(time.Now().Add(someTimeout));
            break;
        }}
        var (c, err) = ln.Accept();
        if (err != default!) {
            {
                var perr = parseAcceptError(err); if (perr != default!) {
                    ch.ᐸꟷ(perr);
                }
            }
            ch.ᐸꟷ(err);
            return;
        }
        ls.cl = append(ls.cl, c);
        @string network = ln.Addr().Network();
        if (c.LocalAddr().Network() != network || c.RemoteAddr().Network() != network) {
            ch.ᐸꟷ(fmt.Errorf("got %v->%v; expected %v->%v"u8, c.LocalAddr().Network(), c.RemoteAddr().Network(), network, network));
            return;
        }
        c.SetDeadline(time.Now().Add(someTimeout));
        c.SetReadDeadline(time.Now().Add(someTimeout));
        c.SetWriteDeadline(time.Now().Add(someTimeout));
        var b = new slice<byte>(256);
        (var n, err) = c.Read(b);
        if (err != default!) {
            {
                var perr = parseReadError(err); if (perr != default!) {
                    ch.ᐸꟷ(perr);
                }
            }
            ch.ᐸꟷ(err);
            return;
        }
        {
            var (_, errΔ1) = c.Write(b[..(int)(n)]); if (errΔ1 != default!) {
                {
                    var perr = parseWriteError(errΔ1); if (perr != default!) {
                        ch.ᐸꟷ(perr);
                    }
                }
                ch.ᐸꟷ(errΔ1);
                return;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void transceiver(global::go.net_package.Conn c, slice<byte> wb, channel/*<-*/<error> ch) {
    GoFrame ᒐ = default;
    try {
        defer(ᴛ1 => builtin.close(ᴛ1), ch, ref ᒐ);
        c.SetDeadline(time.Now().Add(someTimeout));
        c.SetReadDeadline(time.Now().Add(someTimeout));
        c.SetWriteDeadline(time.Now().Add(someTimeout));
        var (n, err) = c.Write(wb);
        if (err != default!) {
            {
                var perr = parseWriteError(err); if (perr != default!) {
                    ch.ᐸꟷ(perr);
                }
            }
            ch.ᐸꟷ(err);
            return;
        }
        if (n != len(wb)) {
            ch.ᐸꟷ(fmt.Errorf("wrote %d; want %d"u8, n, len(wb)));
        }
        var rb = new slice<byte>(len(wb));
        (n, err) = c.Read(rb);
        if (err != default!) {
            {
                var perr = parseReadError(err); if (perr != default!) {
                    ch.ᐸꟷ(perr);
                }
            }
            ch.ᐸꟷ(err);
            return;
        }
        if (n != len(wb)) {
            ch.ᐸꟷ(fmt.Errorf("read %d; want %d"u8, n, len(wb)));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string udp4ˢ = "udp4"u8;
internal static readonly @string udp6ˢ = "udp6"u8;

internal static global::go.net_package.PacketConn newLocalPacketListener(testing.TB t, @string network, params Span<ж<global::go.net_package.ListenConfig>> lcOptʗp) {
    var lcOpt = lcOptʗp.sslice();

    ж<global::go.net_package.ListenConfig> lc = default!;
    switch (len(lcOpt)) {
    case 0: {
        lc = @new<global::go.net_package.ListenConfig>();
        break;
    }
    case 1: {
        lc = lcOpt[0];
        break;
    }
    default: {
        t.Helper();
        t.Fatal(tooManyListenConfigsˢ);
        break;
    }}

    var lcʗ1 = lc;
    global::go.net_package.PacketConn listenPacket(@string net, @string addr) {
        var (c, err) = lcʗ1.ListenPacket(context.Background(), net, addr);
        if (err != default!) {
            t.Helper();
            t.Fatal(err);
        }
        return c;
    }
    t.Helper();
    var exprᴛ1 = network;
    if (exprᴛ1 == "udp"u8) {
        if (supportsIPv4()) {
            return listenPacket(udp4ˢ, "127.0.0.1:0"u8);
        }
        if (supportsIPv6()) {
            return listenPacket(udp6ˢ, "[::1]:0"u8);
        }
    }
    else if (exprᴛ1 == "udp4"u8) {
        if (supportsIPv4()) {
            return listenPacket(udp4ˢ, "127.0.0.1:0"u8);
        }
    }
    else if (exprᴛ1 == "udp6"u8) {
        if (supportsIPv6()) {
            return listenPacket(udp6ˢ, "[::1]:0"u8);
        }
    }
    else if (exprᴛ1 == "unixgram"u8) {
        return listenPacket(network, testUnixAddr(t));
    }

    t.Fatalf("%s is not supported"u8, network);
    return default!;
}

[GoType("dyn")] internal partial struct newDualStackPacketListener_type {
    internal @string network;
    public partial ref global::go.net_package.UDPAddr UDPAddr { get; }
}

internal static (slice<ж<global::go.net_package.UDPConn>> cs, error err) newDualStackPacketListener() {
    slice<newDualStackPacketListener_type> args = new newDualStackPacketListener_type[]{
        new("udp4"u8, new UDPAddr(IP: IPv4(127, 0, 0, 1))),
        new("udp6"u8, new UDPAddr(IP: IPv6loopback))
    }.slice();
    for (nint i = 0; i < 64; i++) {
        nint port = default!;
        slice<ж<global::go.net_package.UDPConn>> csΔ1 = default!;
        foreach (var (_, vᴛ1) in args) {
            ref var arg = ref heap(new newDualStackPacketListener_type(), out var Ꮡarg);
            arg = vᴛ1;

            arg.UDPAddr.Port = port;
            var (c, errΔ1) = ListenUDP(arg.network, Ꮡarg.of(newDualStackPacketListener_type.ᏑUDPAddr));
            if (errΔ1 != default!) {
                continue;
            }
            port = c.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr()._<ж<global::go.net_package.UDPAddr>>().Value.Port;
            csΔ1 = append(csΔ1, c);
        }
        if (len(csΔ1) != len(args)) {
            foreach (var (_, c) in csΔ1) {
                c.of(global::go.net_package.UDPConn.Ꮡconn).Close();
            }
            continue;
        }
        return (csΔ1, default!);
    }
    return (default!, errors.New(noDualstackPortAvailableˢ));
}

[GoType] internal partial struct localPacketServer {
    internal Δsync.RWMutex pcmu;
    [GoEmbedded] public global::go.net_package.PacketConn PacketConn;
    internal channel<bool> done; // signal that indicates server stopped
}

internal static error buildup(this ж<localPacketServer> Ꮡls, Action<ж<localPacketServer>, global::go.net_package.PacketConn> handler) {
    goǃ(() => {
        handler(Ꮡls, Ꮡls.Value.PacketConn);
        builtin.close(Ꮡls.Value.done);
    });
    return default!;
}

internal static error teardown(this ж<localPacketServer> Ꮡls) {
    ref var ls = ref Ꮡls.DerefOrNull();

    Ꮡls.of(localPacketServer.Ꮡpcmu).Lock();
    if (ls.PacketConn != default!) {
        @string network = ls.PacketConn.LocalAddr().Network();
        @string address = ls.PacketConn.LocalAddr().String();
        ls.PacketConn.Close();
        ᐸꟷ(ls.done);
        ls.PacketConn = default!;
        var exprᴛ1 = network;
        if (exprᴛ1 == "unixgram"u8) {
            Δos.Remove(address);
        }

    }
    Ꮡls.of(localPacketServer.Ꮡpcmu).Unlock();
    return default!;
}

internal static ж<localPacketServer> newLocalPacketServer(testing.TB t, @string network) {
    t.Helper();
    var c = newLocalPacketListener(t, network);
    return Ꮡ(new localPacketServer(PacketConn: c, done: new channel<bool>(0)));
}

[GoType] internal partial struct packetListener {
    [GoEmbedded] public global::go.net_package.PacketConn PacketConn;
}

[GoRecv] internal static ж<localPacketServer> newLocalServer(this ref packetListener pl) {
    return Ꮡ(new localPacketServer(PacketConn: pl.PacketConn, done: new channel<bool>(0)));
}

internal static void packetTransponder(global::go.net_package.PacketConn c, channel/*<-*/<error> ch) {
    GoFrame ᒐ = default;
    try {
        defer(ᴛ1 => builtin.close(ᴛ1), ch, ref ᒐ);
        c.SetDeadline(time.Now().Add(someTimeout));
        c.SetReadDeadline(time.Now().Add(someTimeout));
        c.SetWriteDeadline(time.Now().Add(someTimeout));
        var b = new slice<byte>(256);
        var (n, peer, err) = c.ReadFrom(b);
        if (err != default!) {
            {
                var perr = parseReadError(err); if (perr != default!) {
                    ch.ᐸꟷ(perr);
                }
            }
            ch.ᐸꟷ(err);
            return;
        }
        if (peer == default!) {
            // for connected-mode sockets
            var exprᴛ1 = c.LocalAddr().Network();
            if (exprᴛ1 == "udp"u8) {
                var (ᴛ1, ᴛ2) = ResolveUDPAddr(udpˢ, ((@string)(b[..(int)(n)])));
                (peer, err) = (new global::go.net_package.UDPAddrжΔAddr(ᴛ1), ᴛ2);
            }
            else if (exprᴛ1 == "unixgram"u8) {
                var (ᴛ1, ᴛ2) = ResolveUnixAddr(unixgramˢ, ((@string)(b[..(int)(n)])));
                (peer, err) = (new global::go.net_package.UnixAddrжΔAddr(ᴛ1), ᴛ2);
            }

            if (err != default!) {
                ch.ᐸꟷ(err);
                return;
            }
        }
        {
            var (_, errΔ1) = c.WriteTo(b[..(int)(n)], peer); if (errΔ1 != default!) {
                {
                    var perr = parseWriteError(errΔ1); if (perr != default!) {
                        ch.ᐸꟷ(perr);
                    }
                }
                ch.ᐸꟷ(errΔ1);
                return;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void packetTransceiver(global::go.net_package.PacketConn c, slice<byte> wb, global::go.net_package.ΔAddr dst, channel/*<-*/<error> ch) {
    GoFrame ᒐ = default;
    try {
        defer(ᴛ1 => builtin.close(ᴛ1), ch, ref ᒐ);
        c.SetDeadline(time.Now().Add(someTimeout));
        c.SetReadDeadline(time.Now().Add(someTimeout));
        c.SetWriteDeadline(time.Now().Add(someTimeout));
        var (n, err) = c.WriteTo(wb, dst);
        if (err != default!) {
            {
                var perr = parseWriteError(err); if (perr != default!) {
                    ch.ᐸꟷ(perr);
                }
            }
            ch.ᐸꟷ(err);
            return;
        }
        if (n != len(wb)) {
            ch.ᐸꟷ(fmt.Errorf("wrote %d; want %d"u8, n, len(wb)));
        }
        var rb = new slice<byte>(len(wb));
        (n, _, err) = c.ReadFrom(rb);
        if (err != default!) {
            {
                var perr = parseReadError(err); if (perr != default!) {
                    ch.ᐸꟷ(perr);
                }
            }
            ch.ᐸꟷ(err);
            return;
        }
        if (n != len(wb)) {
            ch.ᐸꟷ(fmt.Errorf("read %d; want %d"u8, n, len(wb)));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static (global::go.net_package.Conn client, global::go.net_package.Conn server) spawnTestSocketPair(testing.TB t, @string net) {
    global::go.net_package.Conn client = default!;
    global::go.net_package.Conn server = default!;
    GoFrame ᒐ = default;
    try {
        t.Helper();
        var ln = newLocalListener(t, net);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        error cerr = default!;
        ref var serr = ref heap<error>(out var Ꮡserr);
        var acceptDone = new channel<EmptyStruct>(0);
        var acceptDoneʗ1 = acceptDone;
        var lnʗ2 = ln;
        goǃ(() => {
            (server, Ꮡserr.ValueSlot) = lnʗ2.Accept();
            acceptDoneʗ1.ᐸꟷ(new EmptyStruct());
        });
        (client, cerr) = Dial(ln.Addr().Network(), ln.Addr().String());
        ᐸꟷ(acceptDone);
        if (cerr != default!) {
            if (server != default!) {
                server.Close();
            }
            t.Fatal(cerr);
        }
        if (serr != default!) {
            if (client != default!) {
                client.Close();
            }
            t.Fatal(serr);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (client, server);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tmpdirˢ = "TMPDIR"u8;

[GoType("dyn")] internal partial interface startTestSocketPeer_type {
    (ж<Δos.File>, error) File();
}

internal static (Action<testing.TB>, error) startTestSocketPeer(testing.TB t, global::go.net_package.Conn conn, @string op, nint chunkSize, nint totalSize) {
    t.Helper();
    if (Δruntime.GOOS == "windows"u8) {
        // TODO(panjf2000): Windows has not yet implemented FileConn,
        //		remove this when it's implemented in https://go.dev/issues/9503.
        t.Fatalf("startTestSocketPeer is not supported on %s"u8, Δruntime.GOOS);
    }
    var (f, err) = conn._<startTestSocketPeer_type>().File();
    if (err != default!) {
        return (default!, err);
    }
    var cmd = testenv.Command(t, Δos.Args[0]);
    cmd.Value.Env = new @string[]{
        "GO_NET_TEST_TRANSFER=1"u8,
        "GO_NET_TEST_TRANSFER_OP="u8 + op,
        "GO_NET_TEST_TRANSFER_CHUNK_SIZE="u8 + strconv.Itoa(chunkSize),
        "GO_NET_TEST_TRANSFER_TOTAL_SIZE="u8 + strconv.Itoa(totalSize),
        "TMPDIR="u8 + Δos.Getenv(tmpdirˢ)
    }.slice();
    cmd.Value.ExtraFiles = append((~cmd).ExtraFiles, f);
    cmd.Value.Stdout = new Δos.FileжWriter(Δos.Stdout);
    cmd.Value.Stderr = new Δos.FileжWriter(Δos.Stderr);
    {
        var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    var cmdCh = new channel<error>(1);
    var cmdʗ1 = cmd;
    var cmdChʗ1 = cmdCh;
    var fʗ1 = f;
    goǃ(() => {
        var errΔ2 = cmdʗ1.Wait();
        conn.Close();
        fʗ1.Close();
        cmdChʗ1.ᐸꟷ(errΔ2);
    });
    var cmdChʗ2 = cmdCh;
    return ((testing.TB tb) => {
        var errΔ3 = ᐸꟷ(cmdChʗ2);
        if (errΔ3 != default!) {
            tb.Errorf("process exited with error: %v"u8, errΔ3);
        }
    }, default!);
}

[GoInit] internal static void init() {
    GoFrame ᒐ = default;
    try {
        if (Δos.Getenv("GO_NET_TEST_TRANSFER"u8) == ""u8) {
            return;
        }
        defer(Δos.Exit, (nint)(0), ref ᒐ);
        var f = Δos.NewFile((uintptr)3, "splice-test-conn"u8);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var (conn, err) = FileConn(f);
        if (err != default!) {
            Δlog.Fatal(err);
        }
        nint chunkSize = default!;
        {
            (chunkSize, err) = strconv.Atoi(Δos.Getenv("GO_NET_TEST_TRANSFER_CHUNK_SIZE"u8)); if (err != default!) {
                Δlog.Fatal(err);
            }
        }
        var buf = new slice<byte>(chunkSize);
        nint totalSize = default!;
        {
            (totalSize, err) = strconv.Atoi(Δos.Getenv("GO_NET_TEST_TRANSFER_TOTAL_SIZE"u8)); if (err != default!) {
                Δlog.Fatal(err);
            }
        }
        Func<slice<byte>, (nint, error)> fn = default!;
        {
            @string op = Δos.Getenv("GO_NET_TEST_TRANSFER_OP"u8);
            var exprᴛ1 = op;
            if (exprᴛ1 == "r"u8) {
                var connʗ1 = conn;
                                fn = connʗ1.Read;
            }
            else if (exprᴛ1 == "w"u8) {
                var connʗ2 = conn;
                defer(() => connʗ2.Close(), ref ᒐ);
                var connʗ3 = conn;
                                fn = connʗ3.Write;
            }
            else { /* default: */
                Δlog.Fatalf("unknown op %q"u8, op);
            }
        }

        nint n = default!;
        for (nint count = 0; count < totalSize; count += n) {
            if (count + chunkSize > totalSize) {
                buf = buf[..(int)(totalSize - count)];
            }
            error errΔ1 = default!;
            {
                (n, errΔ1) = fn(buf); if (errΔ1 != default!) {
                    return;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
