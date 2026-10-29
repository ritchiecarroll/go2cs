namespace go;

using fmt = fmt_package;
using Δnet = net_package;
using syscall = syscall_package;
using time = time_package;

partial class main_package {

internal static void Main() {
    roundTrip();
    zeroLengthDatagram();
    fmt.Println("done");
}

internal static void roundTrip() {
    GoFrame ᒐ = default;
    try {
        var (server, err) = Δnet.ListenPacket("udp4", "127.0.0.1:0");
        if (err != nil) {
            fmt.Println("roundtrip: listen failed");
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);

        var (dst, ok) = sockaddrOf(server.LocalAddr());
        if (!ok) {
            fmt.Println("roundtrip: server address is not IPv4");
            return;
        }

        (var sender, err) = newSender();
        if (err != nil) {
            fmt.Println("roundtrip: sender setup failed");
            return;
        }
        defer(syscall.Closesocket, sender, ref ᒐ);

        (var local, err) = syscall.Getsockname(sender);
        if (err != nil) {
            fmt.Println("roundtrip: getsockname failed");
            return;
        }
        (var mine, ok) = local._<ж<syscall.SockaddrInet4>>(ᐧ);
        if (!ok) {
            fmt.Println("roundtrip: sender address is not IPv4");
            return;
        }

        var payload = slice<byte>("wsasendto-payload"u8);
        (var sent, err) = send(sender, payload, new syscall.SockaddrInet4жΔSockaddr(dst));
        fmt.Println("roundtrip: send reported no error:", err == nil);
        fmt.Println("roundtrip: byte count equals payload:", (nint)sent == len(payload));

        {
            var errΔ1 = server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L))); if (errΔ1 != nil) {
                fmt.Println("roundtrip: deadline failed");
                return;
            }
        }
        var buf = new slice<byte>(64);
        (var n, var from, err) = server.ReadFrom(buf);
        if (err != nil) {
            fmt.Println("roundtrip: readfrom failed");
            return;
        }
        fmt.Println("roundtrip: bytes arrived intact:", ((sstring)(buf.slice(0, n))) == ((sstring)payload));

        (var peer, ok) = from._<ж<Δnet.UDPAddr>>(ᐧ);
        if (!ok) {
            fmt.Println("roundtrip: peer address is not UDP");
            return;
        }
        fmt.Println("roundtrip: peer host matches sender's own:", sameIPv4(peer.IP, mine.Addr));
        fmt.Println("roundtrip: peer port matches sender's own:", peer.Port == mine.Port);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void zeroLengthDatagram() {
    GoFrame ᒐ = default;
    try {
        var (server, err) = Δnet.ListenPacket("udp4", "127.0.0.1:0");
        if (err != nil) {
            fmt.Println("zerolen: listen failed");
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);

        var (dst, ok) = sockaddrOf(server.LocalAddr());
        if (!ok) {
            fmt.Println("zerolen: server address is not IPv4");
            return;
        }

        (var sender, err) = newSender();
        if (err != nil) {
            fmt.Println("zerolen: sender setup failed");
            return;
        }
        defer(syscall.Closesocket, sender, ref ᒐ);

        var one = new byte[]{0}.slice();
        ref var buf = ref heap<syscall.WSABuf>(new syscall.WSABuf(Len: 0, Buf: Ꮡ(one, 0)), out var Ꮡbuf);
        ref var sent = ref heap(new uint32(), out var Ꮡsent);
        err = syscall.WSASendto(sender, Ꮡbuf, 1, Ꮡsent, 0, new syscall.SockaddrInet4жΔSockaddr(dst), nil, nil);
        fmt.Println("zerolen: send reported no error:", err == nil);
        fmt.Println("zerolen: byte count is zero:", sent == 0);

        {
            var errΔ1 = server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L))); if (errΔ1 != nil) {
                fmt.Println("zerolen: deadline failed");
                return;
            }
        }
        (var n, _, err) = server.ReadFrom(new slice<byte>(8));
        fmt.Println("zerolen: a datagram arrived:", err == nil);
        fmt.Println("zerolen: it carried no bytes:", n == 0);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static (syscallꓸHandle, error) newSender() {
    var (s, err) = syscall.Socket(syscall.AF_INET, syscall.SOCK_DGRAM, 0);
    if (err != nil) {
        return (syscall.InvalidHandle, err);
    }
    {
        err = syscall.Bind(s, new syscall.SockaddrInet4жΔSockaddr(Ꮡ(new syscall.SockaddrInet4(Port: 0, Addr: new byte[]{127, 0, 0, 1}.array())))); if (err != nil) {
            syscall.Closesocket(s);
            return (syscall.InvalidHandle, err);
        }
    }
    return (s, default);
}

internal static (uint32, error) send(syscallꓸHandle s, slice<byte> payload, syscallꓸSockaddr to) {
    ref var buf = ref heap<syscall.WSABuf>(new syscall.WSABuf(Len: (uint32)len(payload), Buf: Ꮡ(payload, 0)), out var Ꮡbuf);
    ref var sent = ref heap(new uint32(), out var Ꮡsent);
    var err = syscall.WSASendto(s, Ꮡbuf, 1, Ꮡsent, 0, to, nil, nil);
    return (sent, err);
}

internal static (ж<syscall.SockaddrInet4>, bool) sockaddrOf(netꓸAddr addr) {
    var (ua, ok) = addr._<ж<Δnet.UDPAddr>>(ᐧ);
    if (!ok) {
        return (default, false);
    }
    var ip = ua.IP.To4();
    if (ip == nil) {
        return (default, false);
    }
    var sa = Ꮡ(new syscall.SockaddrInet4(Port: ua.Port));
    copy(sa.Addr[..], ip);
    return (sa, true);
}

internal static bool sameIPv4(Δnet.IP ip, /*[4]*/ array<byte> raw) {
    raw = raw.Clone();

    var four = ip.To4();
    if (four == nil) {
        return false;
    }
    for (nint i = 0; i < 4; i++) {
        if (four[i] != raw[i]) {
            return false;
        }
    }
    return true;
}

} // end main_package
