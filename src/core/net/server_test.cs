// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δos = os_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.net_package;
using time = time_package;

partial class net_internal_test_package {

partial struct tcpServerTestsᴛ1 /*dyn*/ {
    internal @string snet, saddr; // server endpoint
    internal @string tnet, taddr; // target endpoint for client
}
internal static slice<tcpServerTestsᴛ1> tcpServerTests = slice<tcpServerTestsᴛ1>([
    new(snet: "tcp", saddr: ":0", tnet: "tcp", taddr: "127.0.0.1"),
    new(snet: "tcp", saddr: "0.0.0.0:0", tnet: "tcp", taddr: "127.0.0.1"),
    new(snet: "tcp", saddr: "[::ffff:0.0.0.0]:0", tnet: "tcp", taddr: "127.0.0.1"),
    new(snet: "tcp", saddr: "[::]:0", tnet: "tcp", taddr: "::1"),

    new(snet: "tcp", saddr: ":0", tnet: "tcp", taddr: "::1"),
    new(snet: "tcp", saddr: "0.0.0.0:0", tnet: "tcp", taddr: "::1"),
    new(snet: "tcp", saddr: "[::ffff:0.0.0.0]:0", tnet: "tcp", taddr: "::1"),
    new(snet: "tcp", saddr: "[::]:0", tnet: "tcp", taddr: "127.0.0.1"),

    new(snet: "tcp", saddr: ":0", tnet: "tcp4", taddr: "127.0.0.1"),
    new(snet: "tcp", saddr: "0.0.0.0:0", tnet: "tcp4", taddr: "127.0.0.1"),
    new(snet: "tcp", saddr: "[::ffff:0.0.0.0]:0", tnet: "tcp4", taddr: "127.0.0.1"),
    new(snet: "tcp", saddr: "[::]:0", tnet: "tcp6", taddr: "::1"),

    new(snet: "tcp", saddr: ":0", tnet: "tcp6", taddr: "::1"),
    new(snet: "tcp", saddr: "0.0.0.0:0", tnet: "tcp6", taddr: "::1"),
    new(snet: "tcp", saddr: "[::ffff:0.0.0.0]:0", tnet: "tcp6", taddr: "::1"),
    new(snet: "tcp", saddr: "[::]:0", tnet: "tcp4", taddr: "127.0.0.1"),

    new(snet: "tcp", saddr: "127.0.0.1:0", tnet: "tcp", taddr: "127.0.0.1"),
    new(snet: "tcp", saddr: "[::ffff:127.0.0.1]:0", tnet: "tcp", taddr: "127.0.0.1"),
    new(snet: "tcp", saddr: "[::1]:0", tnet: "tcp", taddr: "::1"),

    new(snet: "tcp4", saddr: ":0", tnet: "tcp4", taddr: "127.0.0.1"),
    new(snet: "tcp4", saddr: "0.0.0.0:0", tnet: "tcp4", taddr: "127.0.0.1"),
    new(snet: "tcp4", saddr: "[::ffff:0.0.0.0]:0", tnet: "tcp4", taddr: "127.0.0.1"),

    new(snet: "tcp4", saddr: "127.0.0.1:0", tnet: "tcp4", taddr: "127.0.0.1"),

    new(snet: "tcp6", saddr: ":0", tnet: "tcp6", taddr: "::1"),
    new(snet: "tcp6", saddr: "[::]:0", tnet: "tcp6", taddr: "::1"),

    new(snet: "tcp6", saddr: "[::1]:0", tnet: "tcp6", taddr: "::1")
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notTestableˢ = (@string)"not testable"u8;

// TestTCPServer tests concurrent accept-read-write servers.
public static void TestTCPServer(ж<testing.T> t) {
    const nint N = 3;

    foreach (var (i, vᴛ1) in tcpServerTests) {
        ref var tt = ref heap(new tcpServerTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.snet + " "u8 + tt.saddr + "<-"u8 + tt.taddr, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var tpchsᴷ = new stackAppend<channel<error>>(moves: true);
                var trchsᴷ = new stackAppend<channel<error>>(moves: true);
                if (!testableListenArgs(ttʗ1.snet, ttʗ1.saddr, ttʗ1.taddr)) {
                    tΔ1.Skip(notTestableˢ);
                }

                var (ln, err) = Listen(ttʗ1.snet, ttʗ1.saddr);
                if (err != nil) {
                    {
                        var perr = parseDialError(err); if (perr != nil) {
                            tΔ1.Error(perr);
                        }
                    }
                    tΔ1.Fatal(err);
                }

                ref var lss = ref heap<slice<ж<localServer>>>(out var Ꮡlss);
                slice<channel<error>> tpchs = default;
                defer(() => {
                    foreach (var (_, ls) in Ꮡlss.ValueSlot) {
                        ls.teardown();
                    }
                }, ref ᒐ);
                for (nint iΔ1 = 0; iΔ1 < N; iΔ1++) {
                    var ls = (Ꮡ(new streamListener(Listener: ln))).newLocalServer();
                    Ꮡlss.ValueSlot = append(Ꮡlss.ValueSlot, ls);
                    tpchs = append(tpchsᴷ, tpchs, new channel<error>(1));
                }
                for (nint iΔ2 = 0; iΔ2 < N; iΔ2++) {
                    var ch = tpchs[iΔ2];
                    var chʗ1 = ch;
                    var handler = (ж<localServer> ls, global::go.net_package.Listener lnΔ1) => {
                        ls.transponder(lnΔ1, chʗ1.AsSendOnly());
                    };
                    {
                        var errΔ1 = Ꮡlss.ValueSlot[iΔ2].buildup(handler); if (errΔ1 != nil) {
                            tΔ1.Fatal(errΔ1);
                        }
                    }
                }

                slice<channel<error>> trchs = default;
                for (nint iΔ3 = 0; iΔ3 < N; iΔ3++) {
                    var (_, port, errΔ2) = SplitHostPort(Ꮡlss.ValueSlot[iΔ3].Listener.Addr().String());
                    if (errΔ2 != nil) {
                        tΔ1.Fatal(errΔ2);
                    }
                    ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(Timeout: someTimeout), out var Ꮡd);
                    (var c, errΔ2) = Ꮡd.Dial(ttʗ1.tnet, JoinHostPort(ttʗ1.taddr, port));
                    if (errΔ2 != nil) {
                        {
                            var perr = parseDialError(errΔ2); if (perr != nil) {
                                tΔ1.Error(perr);
                            }
                        }
                        tΔ1.Fatal(errΔ2);
                    }
                    var cʗ1 = c;
                    defer(() => cʗ1.Close(), ref ᒐ);
                    trchs = append(trchsᴷ, trchs, new channel<error>(1));
                    goǃ(transceiver, c, slice<byte>("TCP SERVER TEST"u8), trchs[iΔ3].AsSendOnly());
                }

                trchs = move2heap(trchsᴷ, trchs);
                foreach (var (_, ch) in trchs) {
                    foreach (var errΔ3 in ch) {
                        tΔ1.Errorf("#%d: %v", i, errΔ3);
                    }
                }
                tpchs = move2heap(tpchsᴷ, tpchs);
                foreach (var (_, ch) in tpchs) {
                    foreach (var errΔ4 in ch) {
                        tΔ1.Errorf("#%d: %v", i, errΔ4);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// TestUnixAndUnixpacketServer tests concurrent accept-read-write
// servers
public static partial void TestUnixAndUnixpacketServer(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var tpchsᴷ = new stackAppend<channel<error>>(moves: true);
        var trchsᴷ = new stackAppend<channel<error>>(moves: true);

        slice<prohibitionaryDialArgTestsᴛ1> unixAndUnixpacketServerTests = slice<prohibitionaryDialArgTestsᴛ1>([
            new("unix", testUnixAddr(new net_test_package.testing_TжTB(t))),
            new("unix", "@nettest/go/unix"),

            new("unixpacket", testUnixAddr(new net_test_package.testing_TжTB(t))),
            new("unixpacket", "@nettest/go/unixpacket")
        ]);

        const nint N = 3;

        foreach (var (i, tt) in unixAndUnixpacketServerTests) {
            if (!testableListenArgs(tt.network, tt.address, ""u8)) {
                t.Logf("skipping %s test", tt.network + " " + tt.address);
                continue;
            }

            var (ln, err) = Listen(tt.network, tt.address);
            if (err != nil) {
                {
                    var perr = parseDialError(err); if (perr != nil) {
                        t.Error(perr);
                    }
                }
                t.Fatal(err);
            }

            ref var lss = ref heap<slice<ж<localServer>>>(out var Ꮡlss);
            slice<channel<error>> tpchs = default;
            defer(() => {
                foreach (var (_, ls) in Ꮡlss.ValueSlot) {
                    ls.teardown();
                }
            }, ref ᒐ);
            for (nint iΔ1 = 0; iΔ1 < N; iΔ1++) {
                var ls = (Ꮡ(new streamListener(Listener: ln))).newLocalServer();
                lss = append(lss, ls);
                tpchs = append(tpchsᴷ, tpchs, new channel<error>(1));
            }
            for (nint iΔ2 = 0; iΔ2 < N; iΔ2++) {
                var ch = tpchs[iΔ2];
                var chʗ1 = ch;
                var handler = (ж<localServer> ls, global::go.net_package.Listener lnΔ1) => {
                    ls.transponder(lnΔ1, chʗ1.AsSendOnly());
                };
                {
                    var errΔ1 = lss[iΔ2].buildup(handler); if (errΔ1 != nil) {
                        t.Fatal(errΔ1);
                    }
                }
            }

            slice<channel<error>> trchs = default;
            for (nint iΔ3 = 0; iΔ3 < N; iΔ3++) {
                ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(Timeout: someTimeout), out var Ꮡd);
                var (c, errΔ2) = Ꮡd.Dial(lss[iΔ3].Listener.Addr().Network(), lss[iΔ3].Listener.Addr().String());
                if (errΔ2 != nil) {
                    {
                        var perr = parseDialError(errΔ2); if (perr != nil) {
                            t.Error(perr);
                        }
                    }
                    t.Fatal(errΔ2);
                }

                {
                    var addr = c.LocalAddr(); if (addr != nil) {
                        t.Logf("connected %s->%s", addr, lss[iΔ3].Listener.Addr());
                    }
                }

                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                trchs = append(trchsᴷ, trchs, new channel<error>(1));
                goǃ(transceiver, c, slice<byte>("UNIX AND UNIXPACKET SERVER TEST"u8), trchs[iΔ3].AsSendOnly());
            }

            trchs = move2heap(trchsᴷ, trchs);
            foreach (var (_, ch) in trchs) {
                foreach (var errΔ3 in ch) {
                    t.Errorf("#%d: %v", i, errΔ3);
                }
            }
            tpchs = move2heap(tpchsᴷ, tpchs);
            foreach (var (_, ch) in tpchs) {
                foreach (var errΔ4 in ch) {
                    t.Errorf("#%d: %v", i, errΔ4);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct udpServerTestsᴛ1 /*dyn*/ {
    internal @string snet, saddr; // server endpoint
    internal @string tnet, taddr; // target endpoint for client
    internal bool dial;           // test with Dial
}
internal static slice<udpServerTestsᴛ1> udpServerTests = slice<udpServerTestsᴛ1>([
    new(snet: "udp", saddr: ":0", tnet: "udp", taddr: "127.0.0.1"),
    new(snet: "udp", saddr: "0.0.0.0:0", tnet: "udp", taddr: "127.0.0.1"),
    new(snet: "udp", saddr: "[::ffff:0.0.0.0]:0", tnet: "udp", taddr: "127.0.0.1"),
    new(snet: "udp", saddr: "[::]:0", tnet: "udp", taddr: "::1"),

    new(snet: "udp", saddr: ":0", tnet: "udp", taddr: "::1"),
    new(snet: "udp", saddr: "0.0.0.0:0", tnet: "udp", taddr: "::1"),
    new(snet: "udp", saddr: "[::ffff:0.0.0.0]:0", tnet: "udp", taddr: "::1"),
    new(snet: "udp", saddr: "[::]:0", tnet: "udp", taddr: "127.0.0.1"),

    new(snet: "udp", saddr: ":0", tnet: "udp4", taddr: "127.0.0.1"),
    new(snet: "udp", saddr: "0.0.0.0:0", tnet: "udp4", taddr: "127.0.0.1"),
    new(snet: "udp", saddr: "[::ffff:0.0.0.0]:0", tnet: "udp4", taddr: "127.0.0.1"),
    new(snet: "udp", saddr: "[::]:0", tnet: "udp6", taddr: "::1"),

    new(snet: "udp", saddr: ":0", tnet: "udp6", taddr: "::1"),
    new(snet: "udp", saddr: "0.0.0.0:0", tnet: "udp6", taddr: "::1"),
    new(snet: "udp", saddr: "[::ffff:0.0.0.0]:0", tnet: "udp6", taddr: "::1"),
    new(snet: "udp", saddr: "[::]:0", tnet: "udp4", taddr: "127.0.0.1"),

    new(snet: "udp", saddr: "127.0.0.1:0", tnet: "udp", taddr: "127.0.0.1"),
    new(snet: "udp", saddr: "[::ffff:127.0.0.1]:0", tnet: "udp", taddr: "127.0.0.1"),
    new(snet: "udp", saddr: "[::1]:0", tnet: "udp", taddr: "::1"),

    new(snet: "udp4", saddr: ":0", tnet: "udp4", taddr: "127.0.0.1"),
    new(snet: "udp4", saddr: "0.0.0.0:0", tnet: "udp4", taddr: "127.0.0.1"),
    new(snet: "udp4", saddr: "[::ffff:0.0.0.0]:0", tnet: "udp4", taddr: "127.0.0.1"),

    new(snet: "udp4", saddr: "127.0.0.1:0", tnet: "udp4", taddr: "127.0.0.1"),

    new(snet: "udp6", saddr: ":0", tnet: "udp6", taddr: "::1"),
    new(snet: "udp6", saddr: "[::]:0", tnet: "udp6", taddr: "::1"),

    new(snet: "udp6", saddr: "[::1]:0", tnet: "udp6", taddr: "::1"),

    new(snet: "udp", saddr: "127.0.0.1:0", tnet: "udp", taddr: "127.0.0.1", dial: true),

    new(snet: "udp", saddr: "[::1]:0", tnet: "udp", taddr: "::1", dial: true)
]);

public static void TestUDPServer(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in udpServerTests) {
        ref var tt = ref heap(new udpServerTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(fmt.Sprint(i), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableListenArgs(ttʗ1.snet, ttʗ1.saddr, ttʗ1.taddr)) {
                    tΔ1.Skipf("skipping %s %s<-%s test", ttʗ1.snet, ttʗ1.saddr, ttʗ1.taddr);
                }
                tΔ1.Logf("%s %s<-%s", ttʗ1.snet, ttʗ1.saddr, ttʗ1.taddr);

                var (c1, err) = ListenPacket(ttʗ1.snet, ttʗ1.saddr);
                if (err != nil) {
                    {
                        var perr = parseDialError(err); if (perr != nil) {
                            tΔ1.Error(perr);
                        }
                    }
                    tΔ1.Fatal(err);
                }

                var ls = (Ꮡ(new packetListener(PacketConn: c1))).newLocalServer();
                var lsʗ1 = ls;
                defer(() => lsʗ1.teardown(), ref ᒐ);
                ref var tpch = ref heap<channel<error>>(new channel<error>(1), out var Ꮡtpch);
                var handler = (ж<localPacketServer> lsΔ1, global::go.net_package.PacketConn c) => {
                    packetTransponder(c, Ꮡtpch.ValueSlot.AsSendOnly());
                };
                {
                    var errΔ1 = ls.buildup(handler); if (errΔ1 != nil) {
                        tΔ1.Fatal(errΔ1);
                    }
                }

                var trch = new channel<error>(1);
                (_, var port, err) = SplitHostPort(ls.PacketConn.LocalAddr().String());
                if (err != nil) {
                    tΔ1.Fatal(err);
                }
                if (ttʗ1.dial) {
                    ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(Timeout: someTimeout), out var Ꮡd);
                    var (c2, errΔ2) = Ꮡd.Dial(ttʗ1.tnet, JoinHostPort(ttʗ1.taddr, port));
                    if (errΔ2 != nil) {
                        {
                            var perr = parseDialError(errΔ2); if (perr != nil) {
                                tΔ1.Error(perr);
                            }
                        }
                        tΔ1.Fatal(errΔ2);
                    }
                    var c2ʗ1 = c2;
                    defer(() => c2ʗ1.Close(), ref ᒐ);
                    goǃ(transceiver, c2, slice<byte>("UDP SERVER TEST"u8), trch.AsSendOnly());
                } else {
                    var (c2, errΔ3) = ListenPacket(ttʗ1.tnet, JoinHostPort(ttʗ1.taddr, "0"));
                    if (errΔ3 != nil) {
                        {
                            var perr = parseDialError(errΔ3); if (perr != nil) {
                                tΔ1.Error(perr);
                            }
                        }
                        tΔ1.Fatal(errΔ3);
                    }
                    var c2ʗ2 = c2;
                    defer(() => c2ʗ2.Close(), ref ᒐ);
                    (var dst, errΔ3) = ResolveUDPAddr(ttʗ1.tnet, JoinHostPort(ttʗ1.taddr, port));
                    if (errΔ3 != nil) {
                        tΔ1.Fatal(errΔ3);
                    }
                    goǃ(packetTransceiver, c2, slice<byte>("UDP SERVER TEST"u8), new global::go.net_package.UDPAddrжΔAddr(dst), trch.AsSendOnly());
                }

                while (trch != nil || Ꮡtpch.ValueSlot != nil) {
                    var selᴛ11 = trch;
                    var selᴛ12 = Ꮡtpch.ValueSlot;
                    switch (select(ᐸꟷ(selᴛ11, ꓸꓸꓸ), ᐸꟷ(selᴛ12, ꓸꓸꓸ))) {
                    case 0 when selᴛ11.ꟷᐳ(out var errΔ4, out var ok): {
                        if (!ok) {
                            trch = default;
                        }
                        if (errΔ4 != nil) {
                            tΔ1.Errorf("client: %v", errΔ4);
                        }
                        break;
                    }
                    case 1 when selᴛ12.ꟷᐳ(out var errΔ5, out var ok): {
                        if (!ok) {
                            Ꮡtpch.ValueSlot = default;
                        }
                        if (errΔ5 != nil) {
                            tΔ1.Errorf("server: %v", errΔ5);
                        }
                        break;
                    }}
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

internal partial struct TestUnixgramServer_type /*dyn*/ {
    internal @string saddr; // server endpoint
    internal @string caddr; // client endpoint
    internal bool dial;     // test with Dial
}

public static void TestUnixgramServer(ж<testing.T> t) {
    slice<TestUnixgramServer_type> unixgramServerTests = slice<TestUnixgramServer_type>([
        new(saddr: testUnixAddr(new net_test_package.testing_TжTB(t)), caddr: testUnixAddr(new net_test_package.testing_TжTB(t))),
        new(saddr: testUnixAddr(new net_test_package.testing_TжTB(t)), caddr: testUnixAddr(new net_test_package.testing_TжTB(t)), dial: true),

        new(saddr: "@nettest/go/unixgram/server", caddr: "@nettest/go/unixgram/client")
    ]);

    foreach (var (i, vᴛ1) in unixgramServerTests) {
        ref var tt = ref heap(new TestUnixgramServer_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(fmt.Sprint(i), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableListenArgs(unixgramˢ, ttʗ1.saddr, ""u8)) {
                    tΔ1.Skipf("skipping unixgram %s<-%s test", ttʗ1.saddr, ttʗ1.caddr);
                }
                tΔ1.Logf("unixgram %s<-%s", ttʗ1.saddr, ttʗ1.caddr);

                var (c1, err) = ListenPacket(unixgramˢ, ttʗ1.saddr);
                if (err != nil) {
                    {
                        var perr = parseDialError(err); if (perr != nil) {
                            tΔ1.Error(perr);
                        }
                    }
                    tΔ1.Fatal(err);
                }

                var ls = (Ꮡ(new packetListener(PacketConn: c1))).newLocalServer();
                var lsʗ1 = ls;
                defer(() => lsʗ1.teardown(), ref ᒐ);
                ref var tpch = ref heap<channel<error>>(new channel<error>(1), out var Ꮡtpch);
                var handler = (ж<localPacketServer> lsΔ1, global::go.net_package.PacketConn c) => {
                    packetTransponder(c, Ꮡtpch.ValueSlot.AsSendOnly());
                };
                {
                    var errΔ1 = ls.buildup(handler); if (errΔ1 != nil) {
                        tΔ1.Fatal(errΔ1);
                    }
                }

                var trch = new channel<error>(1);
                if (ttʗ1.dial) {
                    ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(Timeout: someTimeout, LocalAddr: new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr(Net: "unixgram", Name: ttʗ1.caddr)))), out var Ꮡd);
                    var (c2, errΔ2) = Ꮡd.Dial(unixgramˢ, ls.PacketConn.LocalAddr().String());
                    if (errΔ2 != nil) {
                        {
                            var perr = parseDialError(errΔ2); if (perr != nil) {
                                tΔ1.Error(perr);
                            }
                        }
                        tΔ1.Fatal(errΔ2);
                    }
                    defer(Δos.Remove, c2.LocalAddr().String(), ref ᒐ);
                    var c2ʗ1 = c2;
                    defer(() => c2ʗ1.Close(), ref ᒐ);
                    goǃ(transceiver, c2, slice<byte>(c2.LocalAddr().String()), trch.AsSendOnly());
                } else {
                    var (c2, errΔ3) = ListenPacket(unixgramˢ, ttʗ1.caddr);
                    if (errΔ3 != nil) {
                        {
                            var perr = parseDialError(errΔ3); if (perr != nil) {
                                tΔ1.Error(perr);
                            }
                        }
                        tΔ1.Fatal(errΔ3);
                    }
                    defer(Δos.Remove, c2.LocalAddr().String(), ref ᒐ);
                    var c2ʗ2 = c2;
                    defer(() => c2ʗ2.Close(), ref ᒐ);
                    goǃ(packetTransceiver, c2, slice<byte>("UNIXGRAM SERVER TEST"u8), ls.PacketConn.LocalAddr(), trch.AsSendOnly());
                }

                while (trch != nil || Ꮡtpch.ValueSlot != nil) {
                    var selᴛ13 = trch;
                    var selᴛ14 = Ꮡtpch.ValueSlot;
                    switch (select(ᐸꟷ(selᴛ13, ꓸꓸꓸ), ᐸꟷ(selᴛ14, ꓸꓸꓸ))) {
                    case 0 when selᴛ13.ꟷᐳ(out var errΔ4, out var ok): {
                        if (!ok) {
                            trch = default;
                        }
                        if (errΔ4 != nil) {
                            tΔ1.Errorf("client: %v", errΔ4);
                        }
                        break;
                    }
                    case 1 when selᴛ14.ꟷᐳ(out var errΔ5, out var ok): {
                        if (!ok) {
                            Ꮡtpch.ValueSlot = default;
                        }
                        if (errΔ5 != nil) {
                            tΔ1.Errorf("server: %v", errΔ5);
                        }
                        break;
                    }}
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

} // end net_internal_test_package
