namespace go;

using fmt = fmt_package;
using Δio = io_package;
using Δnet = net_package;
using Δsync = sync_package;
using syscall = syscall_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static error writeRawConn(syscall.RawConn c, slice<byte> b) {
    ref var operr = ref heap<error>(out var Ꮡoperr);
    var bʗ1 = b;
    var err = c.Write((uintptr s) => {
        ref var written = ref heap(new uint32(), out var Ꮡwritten);
        ref var buf = ref heap<syscall.WSABuf>(out var Ꮡbuf);
        buf = new syscall.WSABuf(Len: (uint32)len(bʗ1), Buf: Ꮡ(bʗ1, 0));
        Ꮡoperr.ValueSlot = syscall.WSASend((syscallꓸHandle)s, Ꮡbuf, 1, Ꮡwritten, 0, nil, nil);
        return true;
    });
    if (err != nil) {
        return err;
    }
    return operr;
}

internal static (nint, error) readRawConn(syscall.RawConn c, slice<byte> b) {
    ref var operr = ref heap<error>(out var Ꮡoperr);
    nint n = default;
    var bʗ1 = b;
    var err = c.Read((uintptr s) => {
        ref var read = ref heap(new uint32(), out var Ꮡread);
        ref var flags = ref heap(new uint32(), out var Ꮡflags);
        ref var buf = ref heap(new syscall.WSABuf(), out var Ꮡbuf);
        buf.Buf = Ꮡ(bʗ1, 0);
        buf.Len = (uint32)len(bʗ1);
        Ꮡoperr.ValueSlot = syscall.WSARecv((syscallꓸHandle)s, Ꮡbuf, 1, Ꮡread, Ꮡflags, nil, nil);
        n = (nint)read;
        return true;
    });
    if (err != nil) {
        return (n, err);
    }
    return (n, operr);
}

internal static partial (ж<Δnet.TCPConn>, ж<Δnet.TCPConn>) pair() {
    GoFrame ᒐ = default;
    try {
        var (ln, err) = Δnet.Listen("tcp", "127.0.0.1:0");
        if (err != nil) {
            throw panic(fmt.Sprintf("Listen: %v"u8, err));
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        var accepted = new channel<Δnet.Conn>(1);
        var acceptedʗ1 = accepted;
        var lnʗ2 = ln;
        goǃ(() => {
            var (c, errΔ1) = lnʗ2.Accept();
            if (errΔ1 != nil) {
                throw panic(fmt.Sprintf("Accept: %v"u8, errΔ1));
            }
            acceptedʗ1.ᐸꟷ(c);
        });

        (var client, err) = Δnet.Dial("tcp", ln.Addr().String());
        if (err != nil) {
            throw panic(fmt.Sprintf("Dial: %v"u8, err));
        }

        return (client._<ж<Δnet.TCPConn>>(), (ᐸꟷ(accepted))._<ж<Δnet.TCPConn>>());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static syscall.RawConn rawOf(ж<Δnet.TCPConn> c) {
    var (rc, err) = c.SyscallConn();
    if (err != nil) {
        throw panic(fmt.Sprintf("SyscallConn: %v"u8, err));
    }
    return rc;
}

internal static @string readFull(ж<Δnet.TCPConn> c, nint n) {
    var b = new slice<byte>(n);
    {
        var (_, err) = Δio.ReadFull(new net_TCPConnжReader(c), b); if (err != nil) {
            return "<"u8 + err.Error() + ">"u8;
        }
    }
    return (@string)b;
}

internal static @string rawReadFull(ж<Δnet.TCPConn> c, nint n) {
    var rc = rawOf(c);
    var got = new slice<byte>(0, n);
    var b = new slice<byte>(n);

    while (len(got) < n) {
        var (k, err) = readRawConn(rc, b.slice(0, n - len(got)));
        if (err != nil) {
            return ((sstring)got) + "<"u8 + err.Error() + ">"u8;
        }
        if (k == 0) {
            return ((sstring)got) + "<EOF>"u8;
        }
        got = appendꓸꓸꓸ(got, b.slice(0, k));
    }

    return (@string)got;
}

internal static partial void step(@string name, Func<@string> body) {
    var done = new channel<@string>(1);
    var doneʗ1 = done;
    goǃ(() => {
        doneʗ1.ᐸꟷ(body());
    });

    var selᴛ1 = done;
    var selᴛ2 = time.After((time.Duration)(10000000000L));
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var got): {
        fmt.Printf("%s: %s\n"u8, name, got);
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        fmt.Printf("%s: HANG (no return in 10s)\n"u8, name);
        break;
    }}
}

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var (client, server) = pair();
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);
        var serverʗ1 = server;
        defer(() => serverʗ1.Close(), ref ᒐ);

        var clientʗ2 = client;
        var serverʗ2 = server;
        step("1 raw write", () => {
            {
                var err = writeRawConn(rawOf(clientʗ2), slice<byte>("raw hello"u8)); if (err != nil) {
                    return "write: "u8 + err.Error();
                }
            }
            return readFull(serverʗ2, len("raw hello"));
        });

        var clientʗ3 = client;
        var serverʗ3 = server;
        step("2 raw read", () => {
            {
                var (_, err) = serverʗ3.Write(slice<byte>("plain reply"u8)); if (err != nil) {
                    return "write: "u8 + err.Error();
                }
            }
            return rawReadFull(clientʗ3, len("plain reply"));
        });

        var clientʗ4 = client;
        var serverʗ4 = server;
        step("3 ordinary after raw", () => {
            {
                var (_, err) = clientʗ4.Write(slice<byte>("ping"u8)); if (err != nil) {
                    return "client write: "u8 + err.Error();
                }
            }
            @string got = readFull(serverʗ4, 4);
            {
                var (_, err) = serverʗ4.Write(slice<byte>("pong"u8)); if (err != nil) {
                    return "server write: "u8 + err.Error();
                }
            }
            return got + " "u8 + readFull(clientʗ4, 4);
        });

        step("4 two connections at once", [MethodImpl(MethodImplOptions.NoInlining)] () => {
            GoFrame ᒐ = default;
            try {
                const nint rounds = 200;
                var (a1, a2) = pair();
                var (b1, b2) = pair();
                var a1ʗ1 = a1;
                defer(() => a1ʗ1.Close(), ref ᒐ);
                var a2ʗ1 = a2;
                defer(() => a2ʗ1.Close(), ref ᒐ);
                var b1ʗ1 = b1;
                defer(() => b1ʗ1.Close(), ref ᒐ);
                var b2ʗ1 = b2;
                defer(() => b2ʗ1.Close(), ref ᒐ);

                var results = new slice<@string>(2);
                ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
                var resultsʗ1 = results;
                void run(nint i, ж<Δnet.TCPConn> from, ж<Δnet.TCPConn> to, byte tag) {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        var msg = slice<byte>([tag, tag, tag, tag]);
                        for (nint r = 0; r < rounds; r++) {
                            {
                                var err = writeRawConn(rawOf(from), msg); if (err != nil) {
                                    resultsʗ1[i] = fmt.Sprintf("round %d write: %v"u8, r, err);
                                    return;
                                }
                            }
                            {
                                @string got = rawReadFull(to, len(msg)); if (got != ((sstring)msg)) {
                                    resultsʗ1[i] = fmt.Sprintf("round %d read %q"u8, r, got);
                                    return;
                                }
                            }
                        }
                        resultsʗ1[i] = fmt.Sprintf("%c ok"u8, tag);
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }

                Ꮡwg.Add(2);
                var runʗ1 = run;
                goǃ(runʗ1, (nint)0, a1, a2, (byte)'a');
                var runʗ2 = run;
                goǃ(runʗ2, (nint)1, b1, b2, (byte)'b');
                Ꮡwg.Wait();

                return results[0] + ", " + results[1];
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
            finally { ᒐ.Run(); }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
