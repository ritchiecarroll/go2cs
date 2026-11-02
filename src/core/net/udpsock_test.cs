// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using race = @internal.race_package;
using testenv = @internal.testenv_package;
using netip = net.netip_package;
using Δos = os_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using testing = testing_package;
using time = time_package;
using @internal;
using context = context_package;
using net;
using static go.net_package;

partial class net_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object iPv6LinkLocalUnicastˢ = (@string)"IPv6 link-local unicast address not found"u8;

public static void BenchmarkUDP6LinkLocalUnicast(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        ᏑtestHookUninstaller.Do(uninstallTestHooks);

        if (!supportsIPv6()) {
            Ꮡb.Skip(iPv6IsNotSupportedˢ);
        }
        var ifi = loopbackInterface();
        if (ifi == nil) {
            Ꮡb.Skip(loopbackInterfaceNotˢ);
        }
        @string lla = ipv6LinkLocalUnicastAddr(ifi);
        if (lla == ""u8) {
            Ꮡb.Skip(iPv6LinkLocalUnicastˢ);
        }

        var (c1, err) = ListenPacket(udp6ˢ, JoinHostPort(lla + "%"u8 + ifi.Name, "0"));
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        var c1ʗ1 = c1;
        defer(() => c1ʗ1.Close(), ref ᒐ);
        (var c2, err) = ListenPacket(udp6ˢ, JoinHostPort(lla + "%"u8 + ifi.Name, "0"));
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        var c2ʗ1 = c2;
        defer(() => c2ʗ1.Close(), ref ᒐ);

        array<byte> buf = new(1);
        for (nint i = 0; i < b.N; i++) {
            {
                var (_, errΔ1) = c1.WriteTo(buf[..], c2.LocalAddr()); if (errΔ1 != nil) {
                    Ꮡb.Fatal(errΔ1);
                }
            }
            {
                var (_, _, errΔ2) = c2.ReadFrom(buf[..]); if (errΔ2 != nil) {
                    Ꮡb.Fatal(errΔ2);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct resolveUDPAddrTest {
    internal @string network;
    internal @string litAddrOrName;
    internal ж<global::go.net_package.UDPAddr> addr;
    internal error err;
}

internal static slice<resolveUDPAddrTest> resolveUDPAddrTests;
internal static void initᴛresolveUDPAddrTests() { resolveUDPAddrTests = slice<resolveUDPAddrTest>([
    new("udp", "127.0.0.1:0", Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1), Port: 0)), default),
    new("udp4", "127.0.0.1:65535", Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1), Port: 65535)), default),

    new("udp", "[::1]:0", Ꮡ(new UDPAddr(IP: ParseIP("::1"), Port: 0)), default),
    new("udp6", "[::1]:65535", Ꮡ(new UDPAddr(IP: ParseIP("::1"), Port: 65535)), default),

    new("udp", "[::1%en0]:1", Ꮡ(new UDPAddr(IP: ParseIP("::1"), Port: 1, Zone: "en0")), default),
    new("udp6", "[::1%911]:2", Ꮡ(new UDPAddr(IP: ParseIP("::1"), Port: 2, Zone: "911")), default),

    new("", "127.0.0.1:0", Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1), Port: 0)), default), // Go 1.0 behavior
    new("", "[::1]:0", Ꮡ(new UDPAddr(IP: ParseIP("::1"), Port: 0)), default),         // Go 1.0 behavior

    new("udp", ":12345", Ꮡ(new UDPAddr(Port: 12345)), default),

    new("http", "127.0.0.1:0", nil, new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)(@string)"http"))),

    new("udp", "127.0.0.1:domain", Ꮡ(new UDPAddr(IP: ParseIP("127.0.0.1"), Port: 53)), default),
    new("udp", "[::ffff:127.0.0.1]:domain", Ꮡ(new UDPAddr(IP: ParseIP("::ffff:127.0.0.1"), Port: 53)), default),
    new("udp", "[2001:db8::1]:domain", Ꮡ(new UDPAddr(IP: ParseIP("2001:db8::1"), Port: 53)), default),
    new("udp4", "127.0.0.1:domain", Ꮡ(new UDPAddr(IP: ParseIP("127.0.0.1"), Port: 53)), default),
    new("udp4", "[::ffff:127.0.0.1]:domain", Ꮡ(new UDPAddr(IP: ParseIP("127.0.0.1"), Port: 53)), default),
    new("udp6", "[2001:db8::1]:domain", Ꮡ(new UDPAddr(IP: ParseIP("2001:db8::1"), Port: 53)), default),

    new("udp4", "[2001:db8::1]:domain", nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "2001:db8::1")))),
    new("udp6", "127.0.0.1:domain", nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "127.0.0.1")))),
    new("udp6", "[::ffff:127.0.0.1]:domain", nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "::ffff:127.0.0.1"))))
]); }

public static void TestResolveUDPAddr(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = lookupLocalhost;

        foreach (var (_, tt) in resolveUDPAddrTests) {
            var (addr, err) = ResolveUDPAddr(tt.network, tt.litAddrOrName);
            if (!reflect.DeepEqual(addr.OrTypedNil(), tt.addr.OrTypedNil()) || !reflect.DeepEqual(err, tt.err)) {
                t.Errorf("ResolveUDPAddr(%q, %q) = %#v, %v, want %#v, %v", tt.network, tt.litAddrOrName, addr.OrTypedNil(), err, tt.addr.OrTypedNil(), tt.err);
                continue;
            }
            if (err == nil) {
                var (addr2, errΔ1) = ResolveUDPAddr(addr.Network(), addr.String());
                if (!reflect.DeepEqual(addr2.OrTypedNil(), tt.addr.OrTypedNil()) || !AreEqual(errΔ1, tt.err)) {
                    t.Errorf("(%q, %q): ResolveUDPAddr(%q, %q) = %#v, %v, want %#v, %v", tt.network, tt.litAddrOrName, addr.Network(), addr.String(), addr2.OrTypedNil(), errΔ1, tt.addr.OrTypedNil(), tt.err);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestWriteToUDP(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("not supported on %s", Δruntime.GOOS);
            break;
        }}

        if (!testableNetwork(udpˢ)) {
            t.Skipf("skipping: udp not supported");
        }

        var (c, err) = ListenPacket(udpˢ, "127.0.0.1:0");
        if (err != nil) {
            t.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        testWriteToConn(t, c.LocalAddr().String());
        testWriteToPacketConn(t, c.LocalAddr().String());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testWriteToConn(ж<testing.T> t, @string raddr) {
    GoFrame ᒐ = default;
    try {
        var (c, err) = Dial(udpˢ, raddr);
        if (err != nil) {
            t.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        (var ra, err) = ResolveUDPAddr(udpˢ, raddr);
        if (err != nil) {
            t.Fatal(err);
        }
        var rap = ra.AddrPort();

        void assertErrWriteToConnected(ж<testing.T> tΔ1, error errΔ1) {
            tΔ1.Helper();
            {
                var (e, ok) = errΔ1._<ж<global::go.net_package.OpError>>(ᐧ); if (!ok || !AreEqual(e.Err, ErrWriteToConnected)) {
                    tΔ1.Errorf("got %v; want ErrWriteToConnected", errΔ1);
                }
            }
        }

        var b = slice<byte>("CONNECTED-MODE SOCKET"u8);
        (_, err) = c._<ж<global::go.net_package.UDPConn>>().WriteToUDPAddrPort(b, rap);
        assertErrWriteToConnected(t, err);
        (_, err) = c._<ж<global::go.net_package.UDPConn>>().WriteToUDP(b, ra);
        assertErrWriteToConnected(t, err);
        (_, err) = c._<ж<global::go.net_package.UDPConn>>().WriteTo(b, new global::go.net_package.UDPAddrжΔAddr(ra));
        assertErrWriteToConnected(t, err);
        (_, err) = c.Write(b);
        if (err != nil) {
            t.Errorf("c.Write(b) = %v; want nil", err);
        }
        (_, _, err) = c._<ж<global::go.net_package.UDPConn>>().WriteMsgUDP(b, default, ra);
        assertErrWriteToConnected(t, err);
        (_, _, err) = c._<ж<global::go.net_package.UDPConn>>().WriteMsgUDP(b, default, nil);
        if (err != nil) {
            t.Errorf("c.WriteMsgUDP(b, nil, nil) = %v; want nil", err);
        }
        (_, _, err) = c._<ж<global::go.net_package.UDPConn>>().WriteMsgUDPAddrPort(b, default, rap);
        assertErrWriteToConnected(t, err);
        (_, _, err) = c._<ж<global::go.net_package.UDPConn>>().WriteMsgUDPAddrPort(b, default, new netip.AddrPort());
        if (err != nil) {
            t.Errorf("c.WriteMsgUDPAddrPort(b, nil, netip.AddrPort{}) = %v; want nil", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testWriteToPacketConn(ж<testing.T> t, @string raddr) {
    GoFrame ᒐ = default;
    try {
        var (c, err) = ListenPacket(udpˢ, "127.0.0.1:0");
        if (err != nil) {
            t.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        (var ra, err) = ResolveUDPAddr(udpˢ, raddr);
        if (err != nil) {
            t.Fatal(err);
        }

        var b = slice<byte>("UNCONNECTED-MODE SOCKET"u8);
        (_, err) = c._<ж<global::go.net_package.UDPConn>>().WriteToUDP(b, ra);
        if (err != nil) {
            t.Fatal(err);
        }
        (_, err) = c.WriteTo(b, new global::go.net_package.UDPAddrжΔAddr(ra));
        if (err != nil) {
            t.Fatal(err);
        }
        (_, err) = c._<ж<global::go.net_package.UDPConn>>().of(global::go.net_package.UDPConn.Ꮡconn).Write(b);
        if (err == nil) {
            t.Fatal(shouldFailˢ);
        }
        (_, _, err) = c._<ж<global::go.net_package.UDPConn>>().WriteMsgUDP(b, default, nil);
        if (err == nil) {
            t.Fatal(shouldFailˢ);
        }
        if (err != nil && !AreEqual(err._<ж<global::go.net_package.OpError>>().Err, errMissingAddress)) {
            t.Fatalf("should fail as errMissingAddress: %v", err);
        }
        (_, _, err) = c._<ж<global::go.net_package.UDPConn>>().WriteMsgUDP(b, default, ra);
        if (err != nil) {
            t.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct udpConnLocalNameTestsᴛ1 /*dyn*/ {
    internal @string net;
    internal ж<global::go.net_package.UDPAddr> laddr;
}
internal static slice<udpConnLocalNameTestsᴛ1> udpConnLocalNameTests;
internal static void initᴛudpConnLocalNameTests() { udpConnLocalNameTests = slice<udpConnLocalNameTestsᴛ1>([
    new("udp4", Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1)))),
    new("udp4", Ꮡ(new UDPAddr())),
    new("udp4", nil)
]); }

public static void TestUDPConnLocalName(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

    foreach (var (_, vᴛ1) in udpConnLocalNameTests) {
        ref var tt = ref heap(new udpConnLocalNameTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(fmt.Sprint(tt.laddr.OrTypedNil()), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableNetwork(ttʗ1.net)) {
                    tΔ1.Skipf("skipping: %s not available", ttʗ1.net);
                }

                var (c, err) = ListenUDP(ttʗ1.net, ttʗ1.laddr);
                if (err != nil) {
                    tΔ1.Fatal(err);
                }
                var cʗ1 = c;
                defer(() => cʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
                var la = c.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr();
                {
                    var (a, ok) = la._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (!ok || a.Port == 0) {
                        tΔ1.Fatalf("got %v; expected a proper address with non-zero port number", la);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

internal partial struct TestUDPConnLocalAndRemoteNames_type /*dyn*/ {
    internal global::go.net_package.ΔAddr got;
    internal bool ok;
}

public static void TestUDPConnLocalAndRemoteNames(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(udpˢ)) {
            t.Skipf("skipping: udp not available");
        }

        foreach (var (_, laddr) in slice<@string>(["", "127.0.0.1:0"])) {
            var (c1, err) = ListenPacket(udpˢ, "127.0.0.1:0");
            if (err != nil) {
                t.Fatal(err);
            }
            var c1ʗ1 = c1;
            defer(() => c1ʗ1.Close(), ref ᒐ);

            ж<global::go.net_package.UDPAddr> la = default;
            if (laddr != ""u8) {
                error errΔ1 = default;
                {
                    (la, errΔ1) = ResolveUDPAddr(udpˢ, laddr); if (errΔ1 != nil) {
                        t.Fatal(errΔ1);
                    }
                }
            }
            (var c2, err) = DialUDP(udpˢ, la, c1.LocalAddr()._<ж<global::go.net_package.UDPAddr>>());
            if (err != nil) {
                t.Fatal(err);
            }
            var c2ʗ1 = c2;
            defer(() => c2ʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);

            array<TestUDPConnLocalAndRemoteNames_type> connAddrs = new TestUDPConnLocalAndRemoteNames_type[]{
                new(c1.LocalAddr(), true),
                new(c1._<ж<global::go.net_package.UDPConn>>().of(global::go.net_package.UDPConn.Ꮡconn).RemoteAddr(), false),
                new(c2.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr(), true),
                new(c2.of(global::go.net_package.UDPConn.Ꮡconn).RemoteAddr(), true)
            }.array();
            foreach (var (_, ca) in connAddrs.ΔRangeSnapshot()) {
                {
                    var (a, ok) = ca.got._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (ok != ca.ok || ok && a.Port == 0) {
                        t.Fatalf("got %v; expected a proper address with non-zero port number", ca.got);
                    }
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIPv6LinkLocalUnicastUDP(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

        if (!supportsIPv6()) {
            t.Skip(iPv6IsNotSupportedˢ);
        }

        foreach (var (i, tt) in ipv6LinkLocalUnicastUDPTests) {
            var (c1, err) = ListenPacket(tt.network, tt.address);
            if (err != nil) {
                // It might return "LookupHost returned no
                // suitable address" error on some platforms.
                t.Log(err);
                continue;
            }
            var ls = (Ꮡ(new packetListener(PacketConn: c1))).newLocalServer();
            var lsʗ1 = ls;
            defer(() => lsʗ1.teardown(), ref ᒐ);
            var ch = new channel<error>(1);
            var chʗ1 = ch;
            var handler = (ж<localPacketServer> lsΔ1, global::go.net_package.PacketConn c) => {
                packetTransponder(c, chʗ1.AsSendOnly());
            };
            {
                var errΔ1 = ls.buildup(handler); if (errΔ1 != nil) {
                    t.Fatal(errΔ1);
                }
            }
            {
                var (la, ok) = c1.LocalAddr()._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (!ok || !tt.nameLookup && la.Zone == ""u8) {
                    t.Fatalf("got %v; expected a proper address with zone identifier", la.OrTypedNil());
                }
            }

            (var c2, err) = Dial(tt.network, ls.PacketConn.LocalAddr().String());
            if (err != nil) {
                t.Fatal(err);
            }
            var c2ʗ1 = c2;
            defer(() => c2ʗ1.Close(), ref ᒐ);
            {
                var (la, ok) = c2.LocalAddr()._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (!ok || !tt.nameLookup && la.Zone == ""u8) {
                    t.Fatalf("got %v; expected a proper address with zone identifier", la.OrTypedNil());
                }
            }
            {
                var (ra, ok) = c2.RemoteAddr()._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (!ok || !tt.nameLookup && ra.Zone == ""u8) {
                    t.Fatalf("got %v; expected a proper address with zone identifier", ra.OrTypedNil());
                }
            }

            {
                var (_, errΔ2) = c2.Write(slice<byte>("UDP OVER IPV6 LINKLOCAL TEST"u8)); if (errΔ2 != nil) {
                    t.Fatal(errΔ2);
                }
            }
            var b = new slice<byte>(32);
            {
                var (_, errΔ3) = c2.Read(b); if (errΔ3 != nil) {
                    t.Fatal(errΔ3);
                }
            }

            foreach (var errΔ4 in ch) {
                t.Errorf("#%d: %v", i, errΔ4);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string readˢ => ref readˢᵇ;
private static readonly @string readˢᵇ = "Read"u8;
internal static ref readonly @string readFromˢ => ref readFromˢᵇ;
private static readonly @string readFromˢᵇ = "ReadFrom"u8;

public static void TestUDPZeroBytePayload(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("not supported on %s", Δruntime.GOOS);
            break;
        }
        case {} when exprᴛ1 == "ios"u8: {
            testenv.SkipFlaky(new net_test_package.testing_TжTB(t), 29225);
            break;
        }}
        if (!testableNetwork(udpˢ)) {
            t.Skipf("skipping: udp not available");
        }

        var c = newLocalPacketListener(new net_test_package.testing_TжTB(t), udpˢ);
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        foreach (var (_, genericRead) in slice<bool>([false, true])) {
            var (n, err) = c.WriteTo(default, c.LocalAddr());
            if (err != nil) {
                t.Fatal(err);
            }
            if (n != 0) {
                t.Errorf("got %d; want 0", n);
            }
            c.SetReadDeadline(time.Now().Add((time.Duration)(30000000000L)));
            array<byte> b = new(1);
            @string name = default;
            if (genericRead) {
                (_, err) = c._<Conn>().Read(b[..]);
                name = readˢ;
            } else {
                (_, _, err) = c.ReadFrom(b[..]);
                name = readFromˢ;
            }
            if (err != nil) {
                t.Errorf("%s of zero byte packet failed: %v", name, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestUDPZeroByteBuffer(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("not supported on %s", Δruntime.GOOS);
            break;
        }}
        if (!testableNetwork(udpˢ)) {
            t.Skipf("skipping: udp not available");
        }

        var c = newLocalPacketListener(new net_test_package.testing_TжTB(t), udpˢ);
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        var b = slice<byte>("UDP ZERO BYTE BUFFER TEST"u8);
        foreach (var (_, genericRead) in slice<bool>([false, true])) {
            var (n, err) = c.WriteTo(b, c.LocalAddr());
            if (err != nil) {
                t.Fatal(err);
            }
            if (n != len(b)) {
                t.Errorf("got %d; want %d", n, len(b));
            }
            c.SetReadDeadline(time.Now().Add(100 * time.Millisecond));
            if (genericRead) {
                (_, err) = c._<Conn>().Read(default);
            } else {
                (_, _, err) = c.ReadFrom(default);
            }
            switch (err) {
            case var ᴛ when AreEqual(ᴛ, default): { // ReadFrom succeeds
                break;
            }
            default: { // Read may timeout, it depends on the platform
                {
                    var (nerr, ok) = err._<ΔError>(ᐧ); if ((!ok || !nerr.Timeout()) && Δruntime.GOOS != "windows"u8) { // Windows returns WSAEMSGSIZE
                        t.Fatal(err);
                    }
                }
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestUDPReadSizeError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("not supported on %s", Δruntime.GOOS);
            break;
        }}
        if (!testableNetwork(udpˢ)) {
            t.Skipf("skipping: udp not available");
        }

        var c1 = newLocalPacketListener(new net_test_package.testing_TжTB(t), udpˢ);
        var c1ʗ1 = c1;
        defer(() => c1ʗ1.Close(), ref ᒐ);

        var (c2, err) = Dial(udpˢ, c1.LocalAddr().String());
        if (err != nil) {
            t.Fatal(err);
        }
        var c2ʗ1 = c2;
        defer(() => c2ʗ1.Close(), ref ᒐ);

        var b1 = slice<byte>("READ SIZE ERROR TEST"u8);
        foreach (var (_, genericRead) in slice<bool>([false, true])) {
            var (n, errΔ1) = c2.Write(b1);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            if (n != len(b1)) {
                t.Errorf("got %d; want %d", n, len(b1));
            }
            var b2 = new slice<byte>(len(b1) - 1);
            if (genericRead) {
                (n, errΔ1) = c1._<Conn>().Read(b2);
            } else {
                (n, _, errΔ1) = c1.ReadFrom(b2);
            }
            if (errΔ1 != nil && Δruntime.GOOS != "windows"u8) { // Windows returns WSAEMSGSIZE
                t.Fatal(errΔ1);
            }
            if (n != len(b1) - 1) {
                t.Fatalf("got %d; want %d", n, len(b1) - 1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestUDPReadTimeout verifies that ReadFromUDP with timeout returns an error
// without data or an address.
public static void TestUDPReadTimeout(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(udp4ˢ)) {
            t.Skipf("skipping: udp4 not available");
        }

        var (la, err) = ResolveUDPAddr(udp4ˢ, "127.0.0.1:0");
        if (err != nil) {
            t.Fatal(err);
        }
        (var c, err) = ListenUDP(udp4ˢ, la);
        if (err != nil) {
            t.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);

        c.of(global::go.net_package.UDPConn.Ꮡconn).SetDeadline(time.Now());
        var b = new slice<byte>(1);
        (var n, var addr, err) = c.ReadFromUDP(b);
        if (!errors.Is(err, Δos.ErrDeadlineExceeded)) {
            t.Errorf("ReadFromUDP got err %v want os.ErrDeadlineExceeded", err);
        }
        if (n != 0) {
            t.Errorf("ReadFromUDP got n %d want 0", n);
        }
        if (addr != nil) {
            t.Errorf("ReadFromUDP got addr %+#v want nil", addr.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInRaceˢ = (@string)"skipping test in race"u8;
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;

public static void TestAllocs(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8: {
            // These implementations have not been optimized.
            t.Skipf("skipping on %v", Δruntime.GOOS);
            break;
        }
        case {} when exprᴛ1 == "windows"u8: {
            if (race.Enabled) {
                // The Windows implementation make use of sync.Pool,
                // which randomly drops cached items when race is enabled.
                t.Skip(skippingTestInRaceˢ);
            }
            break;
        }}
        if (!testableNetwork(udp4ˢ)) {
            t.Skipf("skipping: udp4 not available");
        }
        if (asan.Enabled) {
            t.Skip(testAllocatesMoreWithˢ);
        }

        // Optimizations are required to remove the allocs.
        testenv.SkipIfOptimizationOff(new net_test_package.testing_TжTB(t));

        var (conn, err) = ListenUDP(udp4ˢ, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1))));
        if (err != nil) {
            t.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        var addr = conn.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr();
        ref var addrPort = ref heap<netip.AddrPort>(addr._<ж<global::go.net_package.UDPAddr>>().AddrPort(), out var ᏑaddrPort);
        var buf = new slice<byte>(8);

        var addrPortʗ1 = addrPort;
        var bufʗ1 = buf;
        var connʗ2 = conn;
        var allocs = testing.AllocsPerRun(1000, () => {
            var (_, _, errΔ1) = connʗ2.WriteMsgUDPAddrPort(bufʗ1, default, addrPortʗ1);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            (_, _, _, _, errΔ1) = connʗ2.ReadMsgUDPAddrPort(bufʗ1, default);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        });
        {
            nint got = (nint)allocs; if (got != 0) {
                t.Errorf("WriteMsgUDPAddrPort/ReadMsgUDPAddrPort allocated %d objects", got);
            }
        }

        var addrPortʗ2 = addrPort;
        var bufʗ2 = buf;
        var connʗ3 = conn;
        allocs = testing.AllocsPerRun(1000, () => {
            var (_, errΔ2) = connʗ3.WriteToUDPAddrPort(bufʗ2, addrPortʗ2);
            if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }
            (_, _, errΔ2) = connʗ3.ReadFromUDPAddrPort(bufʗ2);
            if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }
        });
        {
            nint got = (nint)allocs; if (got != 0) {
                t.Errorf("WriteToUDPAddrPort/ReadFromUDPAddrPort allocated %d objects", got);
            }
        }

        var addrʗ1 = addr;
        var bufʗ3 = buf;
        var connʗ4 = conn;
        allocs = testing.AllocsPerRun(1000, () => {
            var (_, errΔ3) = connʗ4.WriteTo(bufʗ3, addrʗ1);
            if (errΔ3 != nil) {
                t.Fatal(errΔ3);
            }
            (_, _, errΔ3) = connʗ4.ReadFromUDP(bufʗ3);
            if (errΔ3 != nil) {
                t.Fatal(errΔ3);
            }
        });
        {
            nint got = (nint)allocs; if (got != 1) {
                t.Errorf("WriteTo/ReadFromUDP allocated %d objects", got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkReadWriteMsgUDPAddrPort(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        var (conn, err) = ListenUDP(udp4ˢ, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1))));
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        var addr = conn.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr()._<ж<global::go.net_package.UDPAddr>>().AddrPort();
        var buf = new slice<byte>(8);
        b.ResetTimer();
        b.ReportAllocs();
        for (nint i = 0; i < b.N; i++) {
            var (_, _, errΔ1) = conn.WriteMsgUDPAddrPort(buf, default, addr);
            if (errΔ1 != nil) {
                Ꮡb.Fatal(errΔ1);
            }
            (_, _, _, _, errΔ1) = conn.ReadMsgUDPAddrPort(buf, default);
            if (errΔ1 != nil) {
                Ꮡb.Fatal(errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkWriteToReadFromUDP(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        var (conn, err) = ListenUDP(udp4ˢ, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1))));
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        var addr = conn.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr();
        var buf = new slice<byte>(8);
        b.ResetTimer();
        b.ReportAllocs();
        for (nint i = 0; i < b.N; i++) {
            var (_, errΔ1) = conn.WriteTo(buf, addr);
            if (errΔ1 != nil) {
                Ꮡb.Fatal(errΔ1);
            }
            (_, _, errΔ1) = conn.ReadFromUDP(buf);
            if (errΔ1 != nil) {
                Ꮡb.Fatal(errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkWriteToReadFromUDPAddrPort(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        var (conn, err) = ListenUDP(udp4ˢ, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1))));
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        var addr = conn.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr()._<ж<global::go.net_package.UDPAddr>>().AddrPort();
        var buf = new slice<byte>(8);
        b.ResetTimer();
        b.ReportAllocs();
        for (nint i = 0; i < b.N; i++) {
            var (_, errΔ1) = conn.WriteToUDPAddrPort(buf, addr);
            if (errΔ1 != nil) {
                Ꮡb.Fatal(errΔ1);
            }
            (_, _, errΔ1) = conn.ReadFromUDPAddrPort(buf);
            if (errΔ1 != nil) {
                Ꮡb.Fatal(errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object returnedAddrPortIsNotˢ = (@string)"returned AddrPort is not IPv4"u8;
internal static readonly object returnedUDPAddrIsNotIPv4ˢ = (@string)"returned UDPAddr is not IPv4"u8;

public static void TestUDPIPVersionReadMsg(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("skipping on %v", Δruntime.GOOS);
            break;
        }}
        if (!testableNetwork(udp4ˢ)) {
            t.Skipf("skipping: udp4 not available");
        }

        var (conn, err) = ListenUDP(udp4ˢ, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1))));
        if (err != nil) {
            t.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        var daddr = conn.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr()._<ж<global::go.net_package.UDPAddr>>().AddrPort();
        var buf = new slice<byte>(8);
        (_, err) = conn.WriteToUDPAddrPort(buf, daddr);
        if (err != nil) {
            t.Fatal(err);
        }
        (_, _, _, var saddr, err) = conn.ReadMsgUDPAddrPort(buf, default);
        if (err != nil) {
            t.Fatal(err);
        }
        if (!saddr.Addr().Is4()) {
            t.Error(returnedAddrPortIsNotˢ);
        }
        (_, err) = conn.WriteToUDPAddrPort(buf, daddr);
        if (err != nil) {
            t.Fatal(err);
        }
        (_, _, _, var soldaddr, err) = conn.ReadMsgUDP(buf, default);
        if (err != nil) {
            t.Fatal(err);
        }
        if (len(soldaddr.IP) != 4) {
            t.Error(returnedUDPAddrIsNotIPv4ˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ffff127001ˢ => ref ffff127001ˢᵇ;
private static readonly @string ffff127001ˢᵇ = "::ffff:127.0.0.1"u8;

// TestIPv6WriteMsgUDPAddrPortTargetAddrIPVersion verifies that
// WriteMsgUDPAddrPort accepts IPv4, IPv4-mapped IPv6, and IPv6 target addresses
// on a UDPConn listening on "::".
public static void TestIPv6WriteMsgUDPAddrPortTargetAddrIPVersion(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(udp4ˢ)) {
            t.Skipf("skipping: udp4 not available");
        }
        if (!testableNetwork(udp6ˢ)) {
            t.Skipf("skipping: udp6 not available");
        }

        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "openbsd"u8: {
            // DragonflyBSD's IPv6 sockets are always IPv6-only, according to the man page:
            // https://www.dragonflybsd.org/cgi/web-man?command=ip6 (search for IPV6_V6ONLY).
            // OpenBSD's IPv6 sockets are always IPv6-only, according to the man page:
            // https://man.openbsd.org/ip6#IPV6_V6ONLY
            t.Skipf("skipping on %v", Δruntime.GOOS);
            break;
        }}

        var (conn, err) = ListenUDP(udpˢ, nil);
        if (err != nil) {
            t.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);

        var daddr4 = netip.AddrPortFrom(netip.MustParseAddr("127.0.0.1"), 12345);
        var daddr4in6 = netip.AddrPortFrom(netip.MustParseAddr(ffff127001ˢ), 12345);
        var daddr6 = netip.AddrPortFrom(netip.MustParseAddr("::1"), 12345);
        var buf = new slice<byte>(8);

        (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default, daddr4);
        if (err != nil) {
            t.Fatal(err);
        }

        (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default, daddr4in6);
        if (err != nil) {
            t.Fatal(err);
        }

        (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default, daddr6);
        if (err != nil) {
            t.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestIPv4WriteMsgUDPAddrPortTargetAddrIPVersion verifies that
// WriteMsgUDPAddrPort accepts IPv4 and IPv4-mapped IPv6 destination addresses,
// and rejects IPv6 destination addresses on a "udp4" connection.
public static void TestIPv4WriteMsgUDPAddrPortTargetAddrIPVersion(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("not supported on %s", Δruntime.GOOS);
            break;
        }}

        if (!testableNetwork(udp4ˢ)) {
            t.Skipf("skipping: udp4 not available");
        }

        var (conn, err) = ListenUDP(udp4ˢ, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1))));
        if (err != nil) {
            t.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);

        var daddr4 = netip.AddrPortFrom(netip.MustParseAddr("127.0.0.1"), 12345);
        var daddr4in6 = netip.AddrPortFrom(netip.MustParseAddr(ffff127001ˢ), 12345);
        var daddr6 = netip.AddrPortFrom(netip.MustParseAddr("::1"), 12345);
        var buf = new slice<byte>(8);

        {
            (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default, daddr4); if (err != nil) {
                t.Errorf("conn.WriteMsgUDPAddrPort(buf, nil, daddr4) failed: %v", err);
            }
        }

        {
            (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default, daddr4in6); if (err != nil) {
                t.Errorf("conn.WriteMsgUDPAddrPort(buf, nil, daddr4in6) failed: %v", err);
            }
        }

        {
            (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default, daddr6); if (err == nil) {
                t.Errorf("conn.WriteMsgUDPAddrPort(buf, nil, daddr6) should have failed, but got no error");
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestReadWriteMsgUDPAddrPortEmptyCmsg verifies that ReadMsgUDPAddrPort and
// WriteMsgUDPAddrPort won't fail when the cmsg buffer (the oob parameter) is
// non-nil but empty.
//
// This is a regression test for https://golang.org/issue/77875.
public static void TestReadWriteMsgUDPAddrPortEmptyCmsg(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("not supported on %s", Δruntime.GOOS);
            break;
        }}

        if (!testableNetwork(udp4ˢ)) {
            t.Skipf("skipping: udp4 not available");
        }

        var (conn, err) = ListenUDP(udp4ˢ, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1))));
        if (err != nil) {
            t.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);

        var buf = new slice<byte>(8);
        var cmsgBuf = new slice<byte>(8)[..0];
        var daddr = conn.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr()._<ж<global::go.net_package.UDPAddr>>().AddrPort();

        (_, var cmsgn, err) = conn.WriteMsgUDPAddrPort(buf, cmsgBuf, daddr);
        if (err != nil) {
            t.Fatalf("WriteMsgUDPAddrPort failed: %v", err);
        }
        if (cmsgn != 0) {
            t.Errorf("WriteMsgUDPAddrPort wrote %d cmsg bytes; want 0", cmsgn);
        }

        (_, cmsgn, _, _, err) = conn.ReadMsgUDPAddrPort(buf, cmsgBuf);
        if (err != nil) {
            t.Fatalf("ReadMsgUDPAddrPort failed: %v", err);
        }
        if (cmsgn != 0) {
            t.Errorf("ReadMsgUDPAddrPort read %d cmsg bytes; want 0", cmsgn);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
