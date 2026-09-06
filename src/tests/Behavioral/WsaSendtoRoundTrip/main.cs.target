namespace go;

using fmt = fmt_package;
using Δnet = net_package;
using syscall = syscall_package;
using time = time_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object doneˢ = (@string)"done"u8;

internal static void Main() {
    roundTrip();
    zeroLengthDatagram();
    fmt.Println(doneˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string udp4ˢ = "udp4"u8;
private static readonly object roundtripListenFailedˢ = (@string)"roundtrip: listen failed"u8;
private static readonly object roundtripServerAddressIsˢ = (@string)"roundtrip: server address is not IPv4"u8;
private static readonly object roundtripSenderSetupˢ = (@string)"roundtrip: sender setup failed"u8;
private static readonly object roundtripGetsocknameˢ = (@string)"roundtrip: getsockname failed"u8;
private static readonly object roundtripSenderAddressIsˢ = (@string)"roundtrip: sender address is not IPv4"u8;
private static readonly object roundtripSendReportedNoˢ = (@string)"roundtrip: send reported no error:"u8;
private static readonly object roundtripByteCountEqualsˢ = (@string)"roundtrip: byte count equals payload:"u8;
private static readonly object roundtripDeadlineFailedˢ = (@string)"roundtrip: deadline failed"u8;
private static readonly object roundtripReadfromFailedˢ = (@string)"roundtrip: readfrom failed"u8;
private static readonly object roundtripBytesArrivedˢ = (@string)"roundtrip: bytes arrived intact:"u8;
private static readonly object roundtripPeerAddressIsˢ = (@string)"roundtrip: peer address is not UDP"u8;
private static readonly object roundtripPeerHostMatchesˢ = (@string)"roundtrip: peer host matches sender's own:"u8;
private static readonly object roundtripPeerPortMatchesˢ = (@string)"roundtrip: peer port matches sender's own:"u8;

internal static void roundTrip() {
    GoFrame ᒐ = default;
    try {
        var (server, err) = Δnet.ListenPacket(udp4ˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            fmt.Println(roundtripListenFailedˢ);
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        var (dst, ok) = sockaddrOf(server.LocalAddr());
        if (!ok) {
            fmt.Println(roundtripServerAddressIsˢ);
            return;
        }
        (var sender, err) = newSender();
        if (err != default!) {
            fmt.Println(roundtripSenderSetupˢ);
            return;
        }
        defer(syscall.Closesocket, sender, ref ᒐ);
        (var local, err) = syscall.Getsockname(sender);
        if (err != default!) {
            fmt.Println(roundtripGetsocknameˢ);
            return;
        }
        (var mine, ok) = local._<ж<syscall.SockaddrInet4>>(ᐧ);
        if (!ok) {
            fmt.Println(roundtripSenderAddressIsˢ);
            return;
        }
        var payload = slice<byte>("wsasendto-payload"u8);
        (var sent, err) = send(sender, payload, new syscall.SockaddrInet4жΔSockaddr(dst));
        fmt.Println(roundtripSendReportedNoˢ, err == default!);
        fmt.Println(roundtripByteCountEqualsˢ, (nint)sent == len(payload));
        {
            var errΔ1 = server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L))); if (errΔ1 != default!) {
                fmt.Println(roundtripDeadlineFailedˢ);
                return;
            }
        }
        var buf = new slice<byte>(64);
        (var n, var from, err) = server.ReadFrom(buf);
        if (err != default!) {
            fmt.Println(roundtripReadfromFailedˢ);
            return;
        }
        fmt.Println(roundtripBytesArrivedˢ, ((sstring)(buf[..(int)(n)])) == ((sstring)payload));
        (var peer, ok) = from._<ж<Δnet.UDPAddr>>(ᐧ);
        if (!ok) {
            fmt.Println(roundtripPeerAddressIsˢ);
            return;
        }
        fmt.Println(roundtripPeerHostMatchesˢ, sameIPv4((~peer).IP, (~mine).Addr));
        fmt.Println(roundtripPeerPortMatchesˢ, (~peer).Port == (~mine).Port);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object zerolenListenFailedˢ = (@string)"zerolen: listen failed"u8;
private static readonly object zerolenServerAddressIsˢ = (@string)"zerolen: server address is not IPv4"u8;
private static readonly object zerolenSenderSetupFailedˢ = (@string)"zerolen: sender setup failed"u8;
private static readonly object zerolenSendReportedNoˢ = (@string)"zerolen: send reported no error:"u8;
private static readonly object zerolenByteCountIsZeroˢ = (@string)"zerolen: byte count is zero:"u8;
private static readonly object zerolenDeadlineFailedˢ = (@string)"zerolen: deadline failed"u8;
private static readonly object zerolenADatagramArrivedˢ = (@string)"zerolen: a datagram arrived:"u8;
private static readonly object zerolenItCarriedNoBytesˢ = (@string)"zerolen: it carried no bytes:"u8;

internal static void zeroLengthDatagram() {
    GoFrame ᒐ = default;
    try {
        var (server, err) = Δnet.ListenPacket(udp4ˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            fmt.Println(zerolenListenFailedˢ);
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        var (dst, ok) = sockaddrOf(server.LocalAddr());
        if (!ok) {
            fmt.Println(zerolenServerAddressIsˢ);
            return;
        }
        (var sender, err) = newSender();
        if (err != default!) {
            fmt.Println(zerolenSenderSetupFailedˢ);
            return;
        }
        defer(syscall.Closesocket, sender, ref ᒐ);
        var one = new byte[]{0}.slice();
        ref var buf = ref heap<syscall.WSABuf>(out var Ꮡbuf);
        buf = new syscall.WSABuf(Len: 0, Buf: Ꮡ(one, 0));
        ref var sent = ref heap(new uint32(), out var Ꮡsent);
        err = syscall.WSASendto(sender, Ꮡbuf, 1, Ꮡsent, 0, new syscall.SockaddrInet4жΔSockaddr(dst), nil, nil);
        fmt.Println(zerolenSendReportedNoˢ, err == default!);
        fmt.Println(zerolenByteCountIsZeroˢ, sent == 0);
        {
            var errΔ1 = server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L))); if (errΔ1 != default!) {
                fmt.Println(zerolenDeadlineFailedˢ);
                return;
            }
        }
        (var n, _, err) = server.ReadFrom(new slice<byte>(8));
        fmt.Println(zerolenADatagramArrivedˢ, err == default!);
        fmt.Println(zerolenItCarriedNoBytesˢ, n == 0);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static (syscallꓸHandle, error) newSender() {
    var (s, err) = syscall.Socket(syscall.AF_INET, syscall.SOCK_DGRAM, 0);
    if (err != default!) {
        return (syscall.InvalidHandle, err);
    }
    {
        err = syscall.Bind(s, new syscall.SockaddrInet4жΔSockaddr(Ꮡ(new syscall.SockaddrInet4(Port: 0, Addr: new byte[]{127, 0, 0, 1}.array())))); if (err != default!) {
            syscall.Closesocket(s);
            return (syscall.InvalidHandle, err);
        }
    }
    return (s, default!);
}

internal static (uint32, error) send(syscallꓸHandle s, slice<byte> payload, syscallꓸSockaddr to) {
    ref var buf = ref heap<syscall.WSABuf>(out var Ꮡbuf);
    buf = new syscall.WSABuf(Len: (uint32)len(payload), Buf: Ꮡ(payload, 0));
    ref var sent = ref heap(new uint32(), out var Ꮡsent);
    var err = syscall.WSASendto(s, Ꮡbuf, 1, Ꮡsent, 0, to, nil, nil);
    return (sent, err);
}

internal static (ж<syscall.SockaddrInet4>, bool) sockaddrOf(netꓸAddr addr) {
    var (ua, ok) = addr._<ж<Δnet.UDPAddr>>(ᐧ);
    if (!ok) {
        return (default!, false);
    }
    var ip = (~ua).IP.To4();
    if (ip == default!) {
        return (default!, false);
    }
    var sa = Ꮡ(new syscall.SockaddrInet4(Port: (~ua).Port));
    copy((~sa).Addr[..], ip);
    return (sa, true);
}

internal static bool sameIPv4(Δnet.IP ip, [GoArrayDims(4)] array<byte> raw) {
    raw = raw.Clone();

    var four = ip.To4();
    if (four == default!) {
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
