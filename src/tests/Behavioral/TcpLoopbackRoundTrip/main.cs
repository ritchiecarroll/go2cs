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
    foreach (var i in range(buf)) {
        buf[i] = (byte)(i * 7 + 11);
    }
    return buf;
}

internal static uint32 checksum(slice<byte> b) {
    uint32 sum = default;
    foreach (var (_, c) in b) {
        sum = sum * 31 + (uint32)c;
    }
    return sum;
}

internal partial struct roundTrip_accepted /*dyn*/ {
    internal Δnet.Conn conn;
    internal error err;
}

internal static partial void roundTrip(@string label, @string network, @string address) {
    GoFrame ᒐ = default;
    try {
        var (listener, err) = Δnet.Listen(network, address);
        if (err != nil) {
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
        if (err != nil) {
            fmt.Printf("%s: dial failed\n"u8, label);
            return;
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);

        var got = ᐸꟷ(accepts);
        if (got.err != nil) {
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
            if (errΔ2 != nil) {
                echoedʗ1.ᐸꟷ(-1);
                return;
            }
            (var m, errΔ2) = serverʗ2.Write(buf.slice(0, n));
            if (errΔ2 != nil) {
                echoedʗ1.ᐸꟷ(-2);
                return;
            }
            echoedʗ1.ᐸꟷ(m);
        });

        (var written, err) = client.Write(payload);
        if (err != nil) {
            fmt.Printf("%s: client write failed\n"u8, label);
            return;
        }

        var back = new slice<byte>(payloadSize);
        (var read, err) = Δio.ReadFull(new net_ConnᴠReader(client), back);
        if (err != nil) {
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

internal static partial void closeBreaksBlockedRead() {
    GoFrame ᒐ = default;
    try {
        var (listener, err) = Δnet.Listen("tcp", "127.0.0.1:0");
        if (err != nil) {
            fmt.Println("closeRead: listen failed");
            return;
        }
        var listenerʗ1 = listener;
        defer(() => listenerʗ1.Close(), ref ᒐ);

        var accepts = new channel<Δnet.Conn>(1);
        var acceptsʗ1 = accepts;
        var listenerʗ2 = listener;
        goǃ(() => {
            var (conn, errΔ1) = listenerʗ2.Accept();
            if (errΔ1 != nil) {
                acceptsʗ1.ᐸꟷ(default);
                return;
            }
            acceptsʗ1.ᐸꟷ(conn);
        });

        (var client, err) = Δnet.Dial("tcp", listener.Addr().String());
        if (err != nil) {
            fmt.Println("closeRead: dial failed");
            return;
        }
        var server = ᐸꟷ(accepts);
        if (server == nil) {
            fmt.Println("closeRead: accept failed");
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
            fmt.Printf("closeRead: brokeBlockedRead=%v\n"u8, errΔ3 != nil);
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            fmt.Println("closeRead: brokeBlockedRead=false (timed out)");
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void closeBreaksBlockedWrite() {
    GoFrame ᒐ = default;
    try {
        var (listener, err) = Δnet.Listen("tcp", "127.0.0.1:0");
        if (err != nil) {
            fmt.Println("closeWrite: listen failed");
            return;
        }
        var listenerʗ1 = listener;
        defer(() => listenerʗ1.Close(), ref ᒐ);

        var accepts = new channel<Δnet.Conn>(1);
        var acceptsʗ1 = accepts;
        var listenerʗ2 = listener;
        goǃ(() => {
            var (conn, errΔ1) = listenerʗ2.Accept();
            if (errΔ1 != nil) {
                acceptsʗ1.ᐸꟷ(default);
                return;
            }
            acceptsʗ1.ᐸꟷ(conn);
        });

        (var client, err) = Δnet.Dial("tcp", listener.Addr().String());
        if (err != nil) {
            fmt.Println("closeWrite: dial failed");
            return;
        }
        var server = ᐸꟷ(accepts);
        if (server == nil) {
            fmt.Println("closeWrite: accept failed");
            return;
        }
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);

        var done = new channel<error>(1);
        var clientʗ1 = client;
        var doneʗ1 = done;
        goǃ(() => {
            var buf = new slice<byte>(64 * 1024);
            for (;;) {
                {
                    var (_, errΔ2) = clientʗ1.Write(buf); if (errΔ2 != nil) {
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
            fmt.Printf("closeWrite: brokeBlockedWrite=%v\n"u8, errΔ3 != nil);
            break;
        }
        case 1 when selᴛ4.ꟷᐳ(out _): {
            fmt.Println("closeWrite: brokeBlockedWrite=false (timed out)");
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    roundTrip("ipv4", "tcp", "127.0.0.1:0");

    {
        var (probe, err) = Δnet.Listen("tcp", "[::1]:0"); if (err == nil) {
            probe.Close();
            fmt.Println("ipv6: available=true");
            roundTrip("ipv6", "tcp", "[::1]:0");
        } else {
            fmt.Println("ipv6: available=false");
        }
    }

    closeBreaksBlockedRead();
    closeBreaksBlockedWrite();

    fmt.Println("done");
}

} // end main_package
