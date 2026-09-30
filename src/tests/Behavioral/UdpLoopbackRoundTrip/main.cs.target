namespace go;

using fmt = fmt_package;
using Δnet = net_package;
using time = time_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object doneˢ = (@string)"done"u8;

internal static void Main() {
    unconnectedRoundTrip();
    zeroLengthDatagram();
    connectedRoundTrip();
    ipv6RoundTrip();
    fmt.Println(doneˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string udp4ˢ = "udp4"u8;
private static readonly object ipv4ListenFailedˢ = (@string)"ipv4: listen failed"u8;
private static readonly object ipv4ClientListenFailedˢ = (@string)"ipv4: client listen failed"u8;
private static readonly object ipv4WriteToFailedˢ = (@string)"ipv4: WriteTo failed"u8;
private static readonly object ipv4ReadFromFailedˢ = (@string)"ipv4: ReadFrom failed"u8;
private static readonly object ipv4ReplyWriteToFailedˢ = (@string)"ipv4: reply WriteTo failed"u8;
private static readonly object ipv4ReplyReadFromFailedˢ = (@string)"ipv4: reply ReadFrom failed"u8;

internal static void unconnectedRoundTrip() {
    GoFrame ᒐ = default;
    try {
        var (server, err) = Δnet.ListenPacket(udp4ˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            fmt.Println(ipv4ListenFailedˢ);
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        (var client, err) = Δnet.ListenPacket(udp4ˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            fmt.Println(ipv4ClientListenFailedˢ);
            return;
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);
        var payload = slice<byte>("datagram-payload"u8);
        {
            (_, err) = client.WriteTo(payload, server.LocalAddr()); if (err != default!) {
                fmt.Println(ipv4WriteToFailedˢ);
                return;
            }
        }
        server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var buf = new slice<byte>(64);
        (var n, var from, err) = server.ReadFrom(buf);
        if (err != default!) {
            fmt.Println(ipv4ReadFromFailedˢ);
            return;
        }
        fmt.Printf("ipv4: bytesMatch=%v\n"u8, ((sstring)(buf.slice(0, n))) == ((sstring)payload));
        fmt.Printf("ipv4: senderAddrMatchesClient=%v\n"u8, from.String() == client.LocalAddr().String());
        {
            (_, err) = server.WriteTo(slice<byte>("reply"u8), from); if (err != default!) {
                fmt.Println(ipv4ReplyWriteToFailedˢ);
                return;
            }
        }
        client.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var (rn, rfrom, rerr) = client.ReadFrom(buf);
        if (rerr != default!) {
            fmt.Println(ipv4ReplyReadFromFailedˢ);
            return;
        }
        fmt.Printf("ipv4: replyMatches=%v replyFromServer=%v\n"u8,
            ((sstring)(buf.slice(0, rn))) == "reply"u8, rfrom.String() == server.LocalAddr().String());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object zerolenListenFailedˢ = (@string)"zerolen: listen failed"u8;
private static readonly object zerolenClientListenˢ = (@string)"zerolen: client listen failed"u8;
private static readonly object zerolenWriteToFailedˢ = (@string)"zerolen: WriteTo failed"u8;

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
        (var client, err) = Δnet.ListenPacket(udp4ˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            fmt.Println(zerolenClientListenˢ);
            return;
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);
        {
            (_, err) = client.WriteTo(new byte[]{}.slice(), server.LocalAddr()); if (err != default!) {
                fmt.Println(zerolenWriteToFailedˢ);
                return;
            }
        }
        server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var buf = new slice<byte>(8);
        (var n, var from, err) = server.ReadFrom(buf);
        fmt.Printf("zerolen: arrived=%v length=%v senderKnown=%v\n"u8,
            err == default!, n == 0, err == default! && from.String() == client.LocalAddr().String());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object connectedListenFailedˢ = (@string)"connected: listen failed"u8;
private static readonly object connectedDialFailedˢ = (@string)"connected: dial failed"u8;
private static readonly object connectedWriteFailedˢ = (@string)"connected: write failed"u8;
private static readonly object connectedReadFromFailedˢ = (@string)"connected: ReadFrom failed"u8;
private static readonly object connectedReplyFailedˢ = (@string)"connected: reply failed"u8;

internal static void connectedRoundTrip() {
    GoFrame ᒐ = default;
    try {
        var (server, err) = Δnet.ListenPacket(udp4ˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            fmt.Println(connectedListenFailedˢ);
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        (var conn, err) = Δnet.Dial(udp4ˢ, server.LocalAddr().String());
        if (err != default!) {
            fmt.Println(connectedDialFailedˢ);
            return;
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        {
            (_, err) = conn.Write(slice<byte>("connected-payload"u8)); if (err != default!) {
                fmt.Println(connectedWriteFailedˢ);
                return;
            }
        }
        server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var buf = new slice<byte>(64);
        (var n, var from, err) = server.ReadFrom(buf);
        if (err != default!) {
            fmt.Println(connectedReadFromFailedˢ);
            return;
        }
        fmt.Printf("connected: bytesMatch=%v senderMatchesLocal=%v\n"u8,
            ((sstring)(buf.slice(0, n))) == "connected-payload"u8, from.String() == conn.LocalAddr().String());
        {
            (_, err) = server.WriteTo(slice<byte>("connected-reply"u8), from); if (err != default!) {
                fmt.Println(connectedReplyFailedˢ);
                return;
            }
        }
        conn.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var (rn, rerr) = conn.Read(buf);
        fmt.Printf("connected: replyMatches=%v\n"u8, rerr == default! && ((sstring)(buf.slice(0, rn))) == "connected-reply"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string udp6ˢ = "udp6"u8;
private static readonly object ipv6AvailableFalseˢ = (@string)"ipv6: available=false"u8;
private static readonly object ipv6AvailableTrueˢ = (@string)"ipv6: available=true"u8;
private static readonly object ipv6WriteToFailedˢ = (@string)"ipv6: WriteTo failed"u8;
private static readonly object ipv6ReadFromFailedˢ = (@string)"ipv6: ReadFrom failed"u8;

internal static void ipv6RoundTrip() {
    GoFrame ᒐ = default;
    try {
        var (server, err) = Δnet.ListenPacket(udp6ˢ, "[::1]:0"u8);
        if (err != default!) {
            fmt.Println(ipv6AvailableFalseˢ);
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        (var client, err) = Δnet.ListenPacket(udp6ˢ, "[::1]:0"u8);
        if (err != default!) {
            fmt.Println(ipv6AvailableFalseˢ);
            return;
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);
        fmt.Println(ipv6AvailableTrueˢ);
        {
            (_, err) = client.WriteTo(slice<byte>("v6-payload"u8), server.LocalAddr()); if (err != default!) {
                fmt.Println(ipv6WriteToFailedˢ);
                return;
            }
        }
        server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var buf = new slice<byte>(64);
        (var n, var from, err) = server.ReadFrom(buf);
        if (err != default!) {
            fmt.Println(ipv6ReadFromFailedˢ);
            return;
        }
        fmt.Printf("ipv6: bytesMatch=%v senderAddrMatchesClient=%v\n"u8,
            ((sstring)(buf.slice(0, n))) == "v6-payload"u8, from.String() == client.LocalAddr().String());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
