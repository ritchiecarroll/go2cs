namespace go;

using fmt = fmt_package;
using Δio = io_package;
using Δnet = net_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static UntypedInt payloadSize => /* 64 * 1024 */ 65536;

internal static slice<byte> makePayload() {
    var buf = new slice<byte>(payloadSize);
    foreach (var (i, _) in buf) {
        buf[i] = (byte)(i * 7 + 11);
    }
    return buf;
}

internal static uint32 checksum(slice<byte> b) {
    uint32 sum = default!;
    foreach (var (_, c) in b) {
        sum = sum * 31 + (uint32)c;
    }
    return sum;
}

[GoType("dyn")] internal partial struct roundTrip_accepted {
    internal Δnet.Conn conn;
    internal error err;
}

internal static partial void roundTrip(@string label, @string network, @string address) {
    GoFrame ᒐ = default;
    try {
        var (listener, err) = Δnet.Listen(network, address);
        if (err != default!) {
            fmt.Printf("%s: listen failed\n"u8, label);
            return;
        }
        var listenerʗ1 = listener;
        defer(() => listenerʗ1.Close(), ref ᒐ);
        var payload = makePayload();
        var want = checksum(payload);
        var accepts = new channel<roundTrip_accepted>(1);
        var acceptsʗ1 = accepts;
        var listenerʗ2 = listener;
        goǃ(() => {
            var (conn, errΔ1) = listenerʗ2.Accept();
            acceptsʗ1.ᐸꟷ(new roundTrip_accepted(conn, errΔ1));
        });
        (var client, err) = Δnet.Dial(network, listener.Addr().String());
        if (err != default!) {
            fmt.Printf("%s: dial failed\n"u8, label);
            return;
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);
        var got = ᐸꟷ(accepts);
        if (got.err != default!) {
            fmt.Printf("%s: accept failed\n"u8, label);
            return;
        }
        var server = got.conn;
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        fmt.Printf("%s: serverSawClientAddr=%v\n"u8, label, server.RemoteAddr().String() == client.LocalAddr().String());
        fmt.Printf("%s: clientSawServerAddr=%v\n"u8, label, client.RemoteAddr().String() == listener.Addr().String());
        fmt.Printf("%s: serverLocalIsListenAddr=%v\n"u8, label, server.LocalAddr().String() == listener.Addr().String());
        var echoed = new channel<nint>(1);
        var echoedʗ1 = echoed;
        var serverʗ2 = server;
        goǃ(() => {
            var buf = new slice<byte>(payloadSize);
            var (n, errΔ2) = Δio.ReadFull(new net_ConnᴠReader(serverʗ2), buf);
            if (errΔ2 != default!) {
                echoedʗ1.ᐸꟷ(-1);
                return;
            }
            (var m, errΔ2) = serverʗ2.Write(buf.slice(0, n));
            if (errΔ2 != default!) {
                echoedʗ1.ᐸꟷ(-2);
                return;
            }
            echoedʗ1.ᐸꟷ(m);
        });
        (var written, err) = client.Write(payload);
        if (err != default!) {
            fmt.Printf("%s: client write failed\n"u8, label);
            return;
        }
        var back = new slice<byte>(payloadSize);
        (var read, err) = Δio.ReadFull(new net_ConnᴠReader(client), back);
        if (err != default!) {
            fmt.Printf("%s: client read failed\n"u8, label);
            return;
        }
        nint serverEchoed = ᐸꟷ(echoed);
        fmt.Printf("%s: clientWroteAll=%v serverEchoedAll=%v clientReadAll=%v\n"u8, label, written == payloadSize, serverEchoed == payloadSize, read == payloadSize);
        fmt.Printf("%s: payloadMatches=%v\n"u8, label, checksum(back) == want);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string tcpˢ = "tcp"u8;
private static readonly object closeReadListenFailedˢ = (@string)"closeRead: listen failed"u8;
private static readonly object closeReadDialFailedˢ = (@string)"closeRead: dial failed"u8;
private static readonly object closeReadAcceptFailedˢ = (@string)"closeRead: accept failed"u8;
private static readonly object closeReadˢ = (@string)"closeRead: brokeBlockedRead=false (timed out)"u8;

internal static partial void closeBreaksBlockedRead() {
    GoFrame ᒐ = default;
    try {
        var (listener, err) = Δnet.Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            fmt.Println(closeReadListenFailedˢ);
            return;
        }
        var listenerʗ1 = listener;
        defer(() => listenerʗ1.Close(), ref ᒐ);
        var accepts = new channel<Δnet.Conn>(1);
        var acceptsʗ1 = accepts;
        var listenerʗ2 = listener;
        goǃ(() => {
            var (conn, errΔ1) = listenerʗ2.Accept();
            if (errΔ1 != default!) {
                acceptsʗ1.ᐸꟷ(default!);
                return;
            }
            acceptsʗ1.ᐸꟷ(conn);
        });
        (var client, err) = Δnet.Dial(tcpˢ, listener.Addr().String());
        if (err != default!) {
            fmt.Println(closeReadDialFailedˢ);
            return;
        }
        var server = ᐸꟷ(accepts);
        if (server == default!) {
            fmt.Println(closeReadAcceptFailedˢ);
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        var done = new channel<error>(1);
        var clientʗ1 = client;
        var doneʗ1 = done;
        goǃ(() => {
            var buf = new slice<byte>(16);
            var (_, errΔ2) = clientʗ1.Read(buf);
            doneʗ1.ᐸꟷ(errΔ2);
        });
        time.Sleep(200 * time.Millisecond);
        client.Close();
        var selᴛ1 = done;
        var selᴛ2 = time.After((time.Duration)(10000000000L));
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out var errΔ3): {
            fmt.Printf("closeRead: brokeBlockedRead=%v\n"u8, errΔ3 != default!);
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            fmt.Println(closeReadˢ);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object closeWriteListenFailedˢ = (@string)"closeWrite: listen failed"u8;
private static readonly object closeWriteDialFailedˢ = (@string)"closeWrite: dial failed"u8;
private static readonly object closeWriteAcceptFailedˢ = (@string)"closeWrite: accept failed"u8;
private static readonly object closeWriteˢ = (@string)"closeWrite: brokeBlockedWrite=false (timed out)"u8;

internal static partial void closeBreaksBlockedWrite() {
    GoFrame ᒐ = default;
    try {
        var (listener, err) = Δnet.Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            fmt.Println(closeWriteListenFailedˢ);
            return;
        }
        var listenerʗ1 = listener;
        defer(() => listenerʗ1.Close(), ref ᒐ);
        var accepts = new channel<Δnet.Conn>(1);
        var acceptsʗ1 = accepts;
        var listenerʗ2 = listener;
        goǃ(() => {
            var (conn, errΔ1) = listenerʗ2.Accept();
            if (errΔ1 != default!) {
                acceptsʗ1.ᐸꟷ(default!);
                return;
            }
            acceptsʗ1.ᐸꟷ(conn);
        });
        (var client, err) = Δnet.Dial(tcpˢ, listener.Addr().String());
        if (err != default!) {
            fmt.Println(closeWriteDialFailedˢ);
            return;
        }
        var server = ᐸꟷ(accepts);
        if (server == default!) {
            fmt.Println(closeWriteAcceptFailedˢ);
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);
        var done = new channel<error>(1);
        var clientʗ1 = client;
        var doneʗ1 = done;
        goǃ(() => {
            var buf = new slice<byte>(64 * 1024);
            while (ᐧ) {
                {
                    var (_, errΔ2) = clientʗ1.Write(buf); if (errΔ2 != default!) {
                        doneʗ1.ᐸꟷ(errΔ2);
                        return;
                    }
                }
            }
        });
        time.Sleep(500 * time.Millisecond);
        client.Close();
        var selᴛ3 = done;
        var selᴛ4 = time.After((time.Duration)(20000000000L));
        switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
        case 0 when selᴛ3.ꟷᐳ(out var errΔ3): {
            fmt.Printf("closeWrite: brokeBlockedWrite=%v\n"u8, errΔ3 != default!);
            break;
        }
        case 1 when selᴛ4.ꟷᐳ(out _): {
            fmt.Println(closeWriteˢ);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string ipv4ˢ = "ipv4"u8;
private static readonly object ipv6AvailableTrueˢ = (@string)"ipv6: available=true"u8;
private static readonly @string ipv6ˢ = "ipv6"u8;
private static readonly object ipv6AvailableFalseˢ = (@string)"ipv6: available=false"u8;
private static readonly object doneˢ = (@string)"done"u8;

internal static void Main() {
    roundTrip(ipv4ˢ, tcpˢ, "127.0.0.1:0"u8);
    {
        var (probe, err) = Δnet.Listen(tcpˢ, "[::1]:0"u8); if (err == default!){
            probe.Close();
            fmt.Println(ipv6AvailableTrueˢ);
            roundTrip(ipv6ˢ, tcpˢ, "[::1]:0"u8);
        } else {
            fmt.Println(ipv6AvailableFalseˢ);
        }
    }
    closeBreaksBlockedRead();
    closeBreaksBlockedWrite();
    fmt.Println(doneˢ);
}

} // end main_package
