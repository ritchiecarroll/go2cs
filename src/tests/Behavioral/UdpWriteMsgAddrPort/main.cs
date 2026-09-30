namespace go;

using fmt = fmt_package;
using Δnet = net_package;
using netip = go.net.netip_package;
using time = time_package;
using go.net;

partial class main_package {

internal static UntypedInt rounds => 200;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string udp4ˢ = "udp4"u8;
private static readonly @string udp6ˢ = "udp6"u8;
private static readonly object doneˢ = (@string)"done"u8;

internal static void Main() {
    writeMsgAddrPortRoundTrip(udp4ˢ, "127.0.0.1"u8);
    writeMsgAddrPortRoundTrip(udp6ˢ, "::1"u8);
    writeMsgUDPRoundTrip(udp4ˢ, "127.0.0.1"u8);
    writeMsgUDPRoundTrip(udp6ˢ, "::1"u8);
    fmt.Println(doneˢ);
}

internal static (ж<Δnet.UDPConn> server, ж<Δnet.UDPConn> client, bool ok) listenPair(@string network, @string host) {
    ж<Δnet.UDPConn> server = default!;
    ж<Δnet.UDPConn> client = default!;

    (server, var err) = Δnet.ListenUDP(network, Ꮡ(new Δnet.UDPAddr(IP: Δnet.ParseIP(host), Port: 0)));
    if (err != default!) {
        return (default!, default!, false);
    }
    (client, err) = Δnet.ListenUDP(network, Ꮡ(new Δnet.UDPAddr(IP: Δnet.ParseIP(host), Port: 0)));
    if (err != default!) {
        server.Close();
        return (default!, default!, false);
    }
    return (server, client, true);
}

internal static void writeMsgAddrPortRoundTrip(@string network, @string host) {
    GoFrame ᒐ = default;
    try {
        var (server, client, ok) = listenPair(network, host);
        if (!ok) {
            fmt.Printf("addrport %s: available=false\n"u8, network);
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);
        fmt.Printf("addrport %s: available=true\n"u8, network);
        var target = netip.AddrPortFrom(netip.MustParseAddr(host), (uint16)(~server.LocalAddr()._<ж<Δnet.UDPAddr>>()).Port);
        var clientPort = (uint16)(~client.LocalAddr()._<ж<Δnet.UDPAddr>>()).Port;
        var payload = slice<byte>("write-msg-addr-port-payload"u8);
        var buf = new slice<byte>(64);
        nint writes = 0;
        nint reads = 0;
        nint bytes = 0;
        nint sender = 0;
        nint oob0 = 0;
        nint flag0 = 0;
        for (nint i = 0; i < rounds; i++) {
            _ = new slice<byte>(512);
            var (n, oobn, err) = client.WriteMsgUDPAddrPort(payload, default!, target);
            if (err != default! || n != len(payload) || oobn != 0) {
                break;
            }
            writes++;
            server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
            (var rn, var roobn, var flags, var from, err) = server.ReadMsgUDPAddrPort(buf, default!);
            if (err != default!) {
                break;
            }
            reads++;
            if (((sstring)(buf.slice(0, rn))) == ((sstring)payload)) {
                bytes++;
            }
            if (from.Port() == clientPort) {
                sender++;
            }
            if (roobn == 0) {
                oob0++;
            }
            if (flags == 0) {
                flag0++;
            }
        }
        fmt.Printf("addrport %s: writes=%d reads=%d bytes=%d sender=%d oobn0=%d flags0=%d\n"u8,
            network, writes, reads, bytes, sender, oob0, flag0);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void writeMsgUDPRoundTrip(@string network, @string host) {
    GoFrame ᒐ = default;
    try {
        var (server, client, ok) = listenPair(network, host);
        if (!ok) {
            fmt.Printf("udpaddr %s: available=false\n"u8, network);
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);
        fmt.Printf("udpaddr %s: available=true\n"u8, network);
        var target = server.LocalAddr()._<ж<Δnet.UDPAddr>>();
        nint clientPort = client.LocalAddr()._<ж<Δnet.UDPAddr>>().Value.Port;
        var payload = slice<byte>("write-msg-udpaddr-payload"u8);
        var buf = new slice<byte>(64);
        nint writes = 0;
        nint reads = 0;
        nint bytes = 0;
        nint sender = 0;
        nint oob0 = 0;
        nint flag0 = 0;
        for (nint i = 0; i < rounds; i++) {
            _ = new slice<byte>(512);
            var (n, oobn, err) = client.WriteMsgUDP(payload, default!, target);
            if (err != default! || n != len(payload) || oobn != 0) {
                break;
            }
            writes++;
            server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
            (var rn, var roobn, var flags, var from, err) = server.ReadMsgUDP(buf, default!);
            if (err != default!) {
                break;
            }
            reads++;
            if (((sstring)(buf.slice(0, rn))) == ((sstring)payload)) {
                bytes++;
            }
            if ((~from).Port == clientPort) {
                sender++;
            }
            if (roobn == 0) {
                oob0++;
            }
            if (flags == 0) {
                flag0++;
            }
        }
        fmt.Printf("udpaddr %s: writes=%d reads=%d bytes=%d sender=%d oobn0=%d flags0=%d\n"u8,
            network, writes, reads, bytes, sender, oob0, flag0);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
