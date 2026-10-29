namespace go;

using fmt = fmt_package;
using Δnet = net_package;
using time = time_package;

partial class main_package {

internal static void Main() {
    unconnectedRoundTrip();
    zeroLengthDatagram();
    connectedRoundTrip();
    ipv6RoundTrip();
    fmt.Println("done");
}

internal static void unconnectedRoundTrip() {
    GoFrame ᒐ = default;
    try {
        var (server, err) = Δnet.ListenPacket("udp4", "127.0.0.1:0");
        if (err != nil) {
            fmt.Println("ipv4: listen failed");
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);

        (var client, err) = Δnet.ListenPacket("udp4", "127.0.0.1:0");
        if (err != nil) {
            fmt.Println("ipv4: client listen failed");
            return;
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        var payload = slice<byte>("datagram-payload"u8);
        {
            (_, err) = client.WriteTo(payload, server.LocalAddr()); if (err != nil) {
                fmt.Println("ipv4: WriteTo failed");
                return;
            }
        }

        server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var buf = new slice<byte>(64);
        (var n, var from, err) = server.ReadFrom(buf);
        if (err != nil) {
            fmt.Println("ipv4: ReadFrom failed");
            return;
        }

        fmt.Printf("ipv4: bytesMatch=%v\n"u8, ((sstring)(buf.slice(0, n))) == ((sstring)payload));
        fmt.Printf("ipv4: senderAddrMatchesClient=%v\n"u8, from.String() == client.LocalAddr().String());

        {
            (_, err) = server.WriteTo(slice<byte>("reply"u8), from); if (err != nil) {
                fmt.Println("ipv4: reply WriteTo failed");
                return;
            }
        }
        client.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var (rn, rfrom, rerr) = client.ReadFrom(buf);
        if (rerr != nil) {
            fmt.Println("ipv4: reply ReadFrom failed");
            return;
        }
        fmt.Printf("ipv4: replyMatches=%v replyFromServer=%v\n"u8,
            ((sstring)(buf.slice(0, rn))) == "reply"u8, rfrom.String() == server.LocalAddr().String());
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

        (var client, err) = Δnet.ListenPacket("udp4", "127.0.0.1:0");
        if (err != nil) {
            fmt.Println("zerolen: client listen failed");
            return;
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        {
            (_, err) = client.WriteTo(slice<byte>([]), server.LocalAddr()); if (err != nil) {
                fmt.Println("zerolen: WriteTo failed");
                return;
            }
        }
        server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var buf = new slice<byte>(8);
        (var n, var from, err) = server.ReadFrom(buf);
        fmt.Printf("zerolen: arrived=%v length=%v senderKnown=%v\n"u8,
            err == nil, n == 0, err == nil && from.String() == client.LocalAddr().String());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void connectedRoundTrip() {
    GoFrame ᒐ = default;
    try {
        var (server, err) = Δnet.ListenPacket("udp4", "127.0.0.1:0");
        if (err != nil) {
            fmt.Println("connected: listen failed");
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);

        (var conn, err) = Δnet.Dial("udp4", server.LocalAddr().String());
        if (err != nil) {
            fmt.Println("connected: dial failed");
            return;
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);

        {
            (_, err) = conn.Write(slice<byte>("connected-payload"u8)); if (err != nil) {
                fmt.Println("connected: write failed");
                return;
            }
        }
        server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var buf = new slice<byte>(64);
        (var n, var from, err) = server.ReadFrom(buf);
        if (err != nil) {
            fmt.Println("connected: ReadFrom failed");
            return;
        }
        fmt.Printf("connected: bytesMatch=%v senderMatchesLocal=%v\n"u8,
            ((sstring)(buf.slice(0, n))) == "connected-payload"u8, from.String() == conn.LocalAddr().String());

        {
            (_, err) = server.WriteTo(slice<byte>("connected-reply"u8), from); if (err != nil) {
                fmt.Println("connected: reply failed");
                return;
            }
        }
        conn.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var (rn, rerr) = conn.Read(buf);
        fmt.Printf("connected: replyMatches=%v\n"u8, rerr == nil && ((sstring)(buf.slice(0, rn))) == "connected-reply"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void ipv6RoundTrip() {
    GoFrame ᒐ = default;
    try {
        var (server, err) = Δnet.ListenPacket("udp6", "[::1]:0");
        if (err != nil) {
            fmt.Println("ipv6: available=false");
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);

        (var client, err) = Δnet.ListenPacket("udp6", "[::1]:0");
        if (err != nil) {
            fmt.Println("ipv6: available=false");
            return;
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        fmt.Println("ipv6: available=true");

        {
            (_, err) = client.WriteTo(slice<byte>("v6-payload"u8), server.LocalAddr()); if (err != nil) {
                fmt.Println("ipv6: WriteTo failed");
                return;
            }
        }
        server.SetReadDeadline(time.Now().Add((time.Duration)(5000000000L)));
        var buf = new slice<byte>(64);
        (var n, var from, err) = server.ReadFrom(buf);
        if (err != nil) {
            fmt.Println("ipv6: ReadFrom failed");
            return;
        }
        fmt.Printf("ipv6: bytesMatch=%v senderAddrMatchesClient=%v\n"u8,
            ((sstring)(buf.slice(0, n))) == "v6-payload"u8, from.String() == client.LocalAddr().String());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
