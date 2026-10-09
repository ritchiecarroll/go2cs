// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
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
        var (c1, err) = ListenPacket(udp6ˢ, JoinHostPort(lla + "%"u8 + (~ifi).Name, "0"u8));
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        var c1ʗ1 = c1;
        defer(() => c1ʗ1.Close(), ref ᒐ);
        (var c2, err) = ListenPacket(udp6ˢ, JoinHostPort(lla + "%"u8 + (~ifi).Name, "0"u8));
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        var c2ʗ1 = c2;
        defer(() => c2ʗ1.Close(), ref ᒐ);
        array<byte> buf = new(1);
        for (nint i = 0; i < b.N; i++) {
            {
                var (_, errΔ1) = c1.WriteTo(buf[..], c2.LocalAddr()); if (errΔ1 != default!) {
                    Ꮡb.Fatal(errΔ1);
                }
            }
            {
                var (_, _, errΔ2) = c2.ReadFrom(buf[..]); if (errΔ2 != default!) {
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

// Go 1.0 behavior
// Go 1.0 behavior
internal static slice<resolveUDPAddrTest> resolveUDPAddrTests;
internal static void initᴛresolveUDPAddrTests() { resolveUDPAddrTests = new resolveUDPAddrTest[]{
    new("udp"u8, "127.0.0.1:0"u8, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1), Port: 0)), default!),
    new("udp4"u8, "127.0.0.1:65535"u8, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1), Port: 65535)), default!),
    new("udp"u8, "[::1]:0"u8, Ꮡ(new UDPAddr(IP: ParseIP("::1"u8), Port: 0)), default!),
    new("udp6"u8, "[::1]:65535"u8, Ꮡ(new UDPAddr(IP: ParseIP("::1"u8), Port: 65535)), default!),
    new("udp"u8, "[::1%en0]:1"u8, Ꮡ(new UDPAddr(IP: ParseIP("::1"u8), Port: 1, Zone: "en0"u8)), default!),
    new("udp6"u8, "[::1%911]:2"u8, Ꮡ(new UDPAddr(IP: ParseIP("::1"u8), Port: 2, Zone: "911"u8)), default!),
    new(""u8, "127.0.0.1:0"u8, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1), Port: 0)), default!),
    new(""u8, "[::1]:0"u8, Ꮡ(new UDPAddr(IP: ParseIP("::1"u8), Port: 0)), default!),
    new("udp"u8, ":12345"u8, Ꮡ(new UDPAddr(Port: 12345)), default!),
    new("http"u8, "127.0.0.1:0"u8, nil, new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)(@string)"http"u8))),
    new("udp"u8, "127.0.0.1:domain"u8, Ꮡ(new UDPAddr(IP: ParseIP("127.0.0.1"u8), Port: 53)), default!),
    new("udp"u8, "[::ffff:127.0.0.1]:domain"u8, Ꮡ(new UDPAddr(IP: ParseIP("::ffff:127.0.0.1"u8), Port: 53)), default!),
    new("udp"u8, "[2001:db8::1]:domain"u8, Ꮡ(new UDPAddr(IP: ParseIP("2001:db8::1"u8), Port: 53)), default!),
    new("udp4"u8, "127.0.0.1:domain"u8, Ꮡ(new UDPAddr(IP: ParseIP("127.0.0.1"u8), Port: 53)), default!),
    new("udp4"u8, "[::ffff:127.0.0.1]:domain"u8, Ꮡ(new UDPAddr(IP: ParseIP("127.0.0.1"u8), Port: 53)), default!),
    new("udp6"u8, "[2001:db8::1]:domain"u8, Ꮡ(new UDPAddr(IP: ParseIP("2001:db8::1"u8), Port: 53)), default!),
    new("udp4"u8, "[2001:db8::1]:domain"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "2001:db8::1"u8)))),
    new("udp6"u8, "127.0.0.1:domain"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "127.0.0.1"u8)))),
    new("udp6"u8, "[::ffff:127.0.0.1]:domain"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "::ffff:127.0.0.1"u8))))
}.slice(); }

public static void TestResolveUDPAddr(ж<testing.T> Ꮡt) {
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
                Ꮡt.Errorf("ResolveUDPAddr(%q, %q) = %#v, %v, want %#v, %v"u8, tt.network, tt.litAddrOrName, addr.OrTypedNil(), err, tt.addr.OrTypedNil(), tt.err);
                continue;
            }
            if (err == default!) {
                var (addr2, errΔ1) = ResolveUDPAddr(addr.Network(), addr.String());
                if (!reflect.DeepEqual(addr2.OrTypedNil(), tt.addr.OrTypedNil()) || !AreEqual(errΔ1, tt.err)) {
                    Ꮡt.Errorf("(%q, %q): ResolveUDPAddr(%q, %q) = %#v, %v, want %#v, %v"u8, tt.network, tt.litAddrOrName, addr.Network(), addr.String(), addr2.OrTypedNil(), errΔ1, tt.addr.OrTypedNil(), tt.err);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestWriteToUDP(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        if (!testableNetwork(udpˢ)) {
            Ꮡt.Skipf("skipping: udp not supported"u8);
        }
        var (c, err) = ListenPacket(udpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        testWriteToConn(Ꮡt, c.LocalAddr().String());
        testWriteToPacketConn(Ꮡt, c.LocalAddr().String());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testWriteToConn(ж<testing.T> Ꮡt, @string raddr) {
    GoFrame ᒐ = default;
    try {
        var (c, err) = Dial(udpˢ, raddr);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        (var ra, err) = ResolveUDPAddr(udpˢ, raddr);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var b = slice<byte>("CONNECTED-MODE SOCKET"u8);
        (_, err) = c._<ж<global::go.net_package.UDPConn>>().WriteToUDP(b, ra);
        if (err == default!) {
            Ꮡt.Fatal(shouldFailˢ);
        }
        if (err != default! && !AreEqual((~err._<ж<global::go.net_package.OpError>>()).Err, ErrWriteToConnected)) {
            Ꮡt.Fatalf("should fail as ErrWriteToConnected: %v"u8, err);
        }
        (_, err) = c._<ж<global::go.net_package.UDPConn>>().WriteTo(b, new global::go.net_package.UDPAddrжΔAddr(ra));
        if (err == default!) {
            Ꮡt.Fatal(shouldFailˢ);
        }
        if (err != default! && !AreEqual((~err._<ж<global::go.net_package.OpError>>()).Err, ErrWriteToConnected)) {
            Ꮡt.Fatalf("should fail as ErrWriteToConnected: %v"u8, err);
        }
        (_, err) = c.Write(b);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (_, _, err) = c._<ж<global::go.net_package.UDPConn>>().WriteMsgUDP(b, default!, ra);
        if (err == default!) {
            Ꮡt.Fatal(shouldFailˢ);
        }
        if (err != default! && !AreEqual((~err._<ж<global::go.net_package.OpError>>()).Err, ErrWriteToConnected)) {
            Ꮡt.Fatalf("should fail as ErrWriteToConnected: %v"u8, err);
        }
        (_, _, err) = c._<ж<global::go.net_package.UDPConn>>().WriteMsgUDP(b, default!, nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testWriteToPacketConn(ж<testing.T> Ꮡt, @string raddr) {
    GoFrame ᒐ = default;
    try {
        var (c, err) = ListenPacket(udpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        (var ra, err) = ResolveUDPAddr(udpˢ, raddr);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var b = slice<byte>("UNCONNECTED-MODE SOCKET"u8);
        (_, err) = c._<ж<global::go.net_package.UDPConn>>().WriteToUDP(b, ra);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (_, err) = c.WriteTo(b, new global::go.net_package.UDPAddrжΔAddr(ra));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (_, err) = c._<ж<global::go.net_package.UDPConn>>().of(global::go.net_package.UDPConn.Ꮡconn).Write(b);
        if (err == default!) {
            Ꮡt.Fatal(shouldFailˢ);
        }
        (_, _, err) = c._<ж<global::go.net_package.UDPConn>>().WriteMsgUDP(b, default!, nil);
        if (err == default!) {
            Ꮡt.Fatal(shouldFailˢ);
        }
        if (err != default! && !AreEqual((~err._<ж<global::go.net_package.OpError>>()).Err, errMissingAddress)) {
            Ꮡt.Fatalf("should fail as errMissingAddress: %v"u8, err);
        }
        (_, _, err) = c._<ж<global::go.net_package.UDPConn>>().WriteMsgUDP(b, default!, ra);
        if (err != default!) {
            Ꮡt.Fatal(err);
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
internal static void initᴛudpConnLocalNameTests() { udpConnLocalNameTests = new udpConnLocalNameTestsᴛ1[]{
    new("udp4"u8, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1)))),
    new("udp4"u8, Ꮡ(new UDPAddr(nil))),
    new("udp4"u8, nil)
}.slice(); }

public static void TestUDPConnLocalName(ж<testing.T> Ꮡt) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
    foreach (var (_, vᴛ1) in udpConnLocalNameTests) {
        ref var tt = ref heap(new udpConnLocalNameTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprint(tt.laddr.OrTypedNil()), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableNetwork(ttʗ1.net)) {
                    tΔ1.Skipf("skipping: %s not available"u8, ttʗ1.net);
                }
                var (c, err) = ListenUDP(ttʗ1.net, ttʗ1.laddr);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var cʗ1 = c;
                defer(() => cʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
                var la = c.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr();
                {
                    var (a, ok) = la._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (!ok || (~a).Port == 0) {
                        tΔ1.Fatalf("got %v; expected a proper address with non-zero port number"u8, la);
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

public static void TestUDPConnLocalAndRemoteNames(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(udpˢ)) {
            Ꮡt.Skipf("skipping: udp not available"u8);
        }
        foreach (var (_, laddr) in new @string[]{""u8, "127.0.0.1:0"u8}.slice()) {
            var (c1, err) = ListenPacket(udpˢ, "127.0.0.1:0"u8);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var c1ʗ1 = c1;
            defer(() => c1ʗ1.Close(), ref ᒐ);
            ж<global::go.net_package.UDPAddr> la = default!;
            if (laddr != ""u8) {
                error errΔ1 = default!;
                {
                    (la, errΔ1) = ResolveUDPAddr(udpˢ, laddr); if (errΔ1 != default!) {
                        Ꮡt.Fatal(errΔ1);
                    }
                }
            }
            (var c2, err) = DialUDP(udpˢ, la, c1.LocalAddr()._<ж<global::go.net_package.UDPAddr>>());
            if (err != default!) {
                Ꮡt.Fatal(err);
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
                    var (a, ok) = ca.got._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (ok != ca.ok || ok && (~a).Port == 0) {
                        Ꮡt.Fatalf("got %v; expected a proper address with non-zero port number"u8, ca.got);
                    }
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIPv6LinkLocalUnicastUDP(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
        if (!supportsIPv6()) {
            Ꮡt.Skip(iPv6IsNotSupportedˢ);
        }
        foreach (var (i, tt) in ipv6LinkLocalUnicastUDPTests) {
            var (c1, err) = ListenPacket(tt.network, tt.address);
            if (err != default!) {
                // It might return "LookupHost returned no
                // suitable address" error on some platforms.
                Ꮡt.Log(err);
                continue;
            }
            var ls = (Ꮡ(new packetListener(PacketConn: c1))).newLocalServer();
            var lsʗ1 = ls;
            defer(() => lsʗ1.teardown(), ref ᒐ);
            var ch = new channel<error>(1);
            var chʗ1 = ch;
            var handler = (ж<localPacketServer> lsΔ1, global::go.net_package.PacketConn c) => {
                packetTransponder(c, chʗ1.WithDirection(GoChanDir.Send));
            };
            {
                var errΔ1 = ls.buildup(handler); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            {
                var (la, ok) = c1.LocalAddr()._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (!ok || !tt.nameLookup && (~la).Zone == ""u8) {
                    Ꮡt.Fatalf("got %v; expected a proper address with zone identifier"u8, la.OrTypedNil());
                }
            }
            (var c2, err) = Dial(tt.network, (~ls).PacketConn.LocalAddr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var c2ʗ1 = c2;
            defer(() => c2ʗ1.Close(), ref ᒐ);
            {
                var (la, ok) = c2.LocalAddr()._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (!ok || !tt.nameLookup && (~la).Zone == ""u8) {
                    Ꮡt.Fatalf("got %v; expected a proper address with zone identifier"u8, la.OrTypedNil());
                }
            }
            {
                var (ra, ok) = c2.RemoteAddr()._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (!ok || !tt.nameLookup && (~ra).Zone == ""u8) {
                    Ꮡt.Fatalf("got %v; expected a proper address with zone identifier"u8, ra.OrTypedNil());
                }
            }
            {
                var (_, errΔ2) = c2.Write(slice<byte>("UDP OVER IPV6 LINKLOCAL TEST"u8)); if (errΔ2 != default!) {
                    Ꮡt.Fatal(errΔ2);
                }
            }
            var b = new slice<byte>(32);
            {
                var (_, errΔ3) = c2.Read(b); if (errΔ3 != default!) {
                    Ꮡt.Fatal(errΔ3);
                }
            }
            foreach (var errΔ4 in ch) {
                Ꮡt.Errorf("#%d: %v"u8, i, errΔ4);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readˢ = "Read"u8;
internal static readonly @string readFromˢ = "ReadFrom"u8;

public static void TestUDPZeroBytePayload(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }
        else if (exprᴛ1 == "ios"u8) {
            testenv.SkipFlaky(new net_test_package.testing_TжTB(Ꮡt), 29225);
        }

        if (!testableNetwork(udpˢ)) {
            Ꮡt.Skipf("skipping: udp not available"u8);
        }
        var c = newLocalPacketListener(new net_test_package.testing_TжTB(Ꮡt), udpˢ);
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        foreach (var (_, genericRead) in new bool[]{false, true}.slice()) {
            var (n, err) = c.WriteTo(default!, c.LocalAddr());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            if (n != 0) {
                Ꮡt.Errorf("got %d; want 0"u8, n);
            }
            c.SetReadDeadline(time.Now().Add((time.Duration)(30000000000L)));
            array<byte> b = new(1);
            @string name = default!;
            if (genericRead){
                (_, err) = c._<Conn>().Read(b[..]);
                name = readˢ;
            } else {
                (_, _, err) = c.ReadFrom(b[..]);
                name = readFromˢ;
            }
            if (err != default!) {
                Ꮡt.Errorf("%s of zero byte packet failed: %v"u8, name, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestUDPZeroByteBuffer(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        if (!testableNetwork(udpˢ)) {
            Ꮡt.Skipf("skipping: udp not available"u8);
        }
        var c = newLocalPacketListener(new net_test_package.testing_TжTB(Ꮡt), udpˢ);
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var b = slice<byte>("UDP ZERO BYTE BUFFER TEST"u8);
        foreach (var (_, genericRead) in new bool[]{false, true}.slice()) {
            var (n, err) = c.WriteTo(b, c.LocalAddr());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            if (n != len(b)) {
                Ꮡt.Errorf("got %d; want %d"u8, n, len(b));
            }
            c.SetReadDeadline(time.Now().Add(100 * time.Millisecond));
            if (genericRead){
                (_, err) = c._<Conn>().Read(default!);
            } else {
                (_, _, err) = c.ReadFrom(default!);
            }
            var exprᴛ2 = err;
            if (AreEqual(exprᴛ2, default!)) {
            }
            else { /* default: */
                {
                    var (nerr, ok) = err._<ΔError>(ᐧ); if ((!ok || !nerr.Timeout()) && Δruntime.GOOS != "windows"u8) {
                        // ReadFrom succeeds
                        // Read may timeout, it depends on the platform
                        // Windows returns WSAEMSGSIZE
                        Ꮡt.Fatal(err);
                    }
                }
            }

        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestUDPReadSizeError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        if (!testableNetwork(udpˢ)) {
            Ꮡt.Skipf("skipping: udp not available"u8);
        }
        var c1 = newLocalPacketListener(new net_test_package.testing_TжTB(Ꮡt), udpˢ);
        var c1ʗ1 = c1;
        defer(() => c1ʗ1.Close(), ref ᒐ);
        var (c2, err) = Dial(udpˢ, c1.LocalAddr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var c2ʗ1 = c2;
        defer(() => c2ʗ1.Close(), ref ᒐ);
        var b1 = slice<byte>("READ SIZE ERROR TEST"u8);
        foreach (var (_, genericRead) in new bool[]{false, true}.slice()) {
            var (n, errΔ1) = c2.Write(b1);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            if (n != len(b1)) {
                Ꮡt.Errorf("got %d; want %d"u8, n, len(b1));
            }
            var b2 = new slice<byte>(len(b1) - 1);
            if (genericRead){
                (n, errΔ1) = c1._<Conn>().Read(b2);
            } else {
                (n, _, errΔ1) = c1.ReadFrom(b2);
            }
            if (errΔ1 != default! && Δruntime.GOOS != "windows"u8) {
                // Windows returns WSAEMSGSIZE
                Ꮡt.Fatal(errΔ1);
            }
            if (n != len(b1) - 1) {
                Ꮡt.Fatalf("got %d; want %d"u8, n, len(b1) - 1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestUDPReadTimeout verifies that ReadFromUDP with timeout returns an error
// without data or an address.
public static void TestUDPReadTimeout(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(udp4ˢ)) {
            Ꮡt.Skipf("skipping: udp4 not available"u8);
        }
        var (la, err) = ResolveUDPAddr(udp4ˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var c, err) = ListenUDP(udp4ˢ, la);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        c.of(global::go.net_package.UDPConn.Ꮡconn).SetDeadline(time.Now());
        var b = new slice<byte>(1);
        (var n, var addr, err) = c.ReadFromUDP(b);
        if (!errors.Is(err, Δos.ErrDeadlineExceeded)) {
            Ꮡt.Errorf("ReadFromUDP got err %v want os.ErrDeadlineExceeded"u8, err);
        }
        if (n != 0) {
            Ꮡt.Errorf("ReadFromUDP got n %d want 0"u8, n);
        }
        if (addr != nil) {
            Ꮡt.Errorf("ReadFromUDP got addr %+#v want nil"u8, addr.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;

public static void TestAllocs(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
            Ꮡt.Skipf("skipping on %v"u8, // These implementations have not been optimized.
 Δruntime.GOOS);
        }

        if (!testableNetwork(udp4ˢ)) {
            Ꮡt.Skipf("skipping: udp4 not available"u8);
        }
        if (asan.Enabled) {
            Ꮡt.Skip(testAllocatesMoreWithˢ);
        }
        // Optimizations are required to remove the allocs.
        testenv.SkipIfOptimizationOff(new net_test_package.testing_TжTB(Ꮡt));
        var (conn, err) = ListenUDP(udp4ˢ, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1))));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        var addr = conn.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr();
        ref var addrPort = ref heap<netip.AddrPort>(out var ᏑaddrPort);
        addrPort = addr._<ж<global::go.net_package.UDPAddr>>().AddrPort();
        var buf = new slice<byte>(8);
        var addrPortʗ1 = addrPort;
        var bufʗ1 = buf;
        var connʗ2 = conn;
        var allocs = testing.AllocsPerRun(1000, () => {
            var (_, _, errΔ1) = connʗ2.WriteMsgUDPAddrPort(bufʗ1, default!, addrPortʗ1);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            (_, _, _, _, errΔ1) = connʗ2.ReadMsgUDPAddrPort(bufʗ1, default!);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        });
        {
            nint got = (nint)allocs; if (got != 0) {
                Ꮡt.Errorf("WriteMsgUDPAddrPort/ReadMsgUDPAddrPort allocated %d objects"u8, got);
            }
        }
        var addrPortʗ2 = addrPort;
        var bufʗ2 = buf;
        var connʗ3 = conn;
        allocs = testing.AllocsPerRun(1000, () => {
            var (_, errΔ2) = connʗ3.WriteToUDPAddrPort(bufʗ2, addrPortʗ2);
            if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
            (_, _, errΔ2) = connʗ3.ReadFromUDPAddrPort(bufʗ2);
            if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        });
        {
            nint got = (nint)allocs; if (got != 0) {
                Ꮡt.Errorf("WriteToUDPAddrPort/ReadFromUDPAddrPort allocated %d objects"u8, got);
            }
        }
        var addrʗ1 = addr;
        var bufʗ3 = buf;
        var connʗ4 = conn;
        allocs = testing.AllocsPerRun(1000, () => {
            var (_, errΔ3) = connʗ4.WriteTo(bufʗ3, addrʗ1);
            if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
            (_, _, errΔ3) = connʗ4.ReadFromUDP(bufʗ3);
            if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        });
        {
            nint got = (nint)allocs; if (got != 1) {
                Ꮡt.Errorf("WriteTo/ReadFromUDP allocated %d objects"u8, got);
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
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        var addr = conn.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr()._<ж<global::go.net_package.UDPAddr>>().AddrPort();
        var buf = new slice<byte>(8);
        b.ResetTimer();
        b.ReportAllocs();
        for (nint i = 0; i < b.N; i++) {
            var (_, _, errΔ1) = conn.WriteMsgUDPAddrPort(buf, default!, addr);
            if (errΔ1 != default!) {
                Ꮡb.Fatal(errΔ1);
            }
            (_, _, _, _, errΔ1) = conn.ReadMsgUDPAddrPort(buf, default!);
            if (errΔ1 != default!) {
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
        if (err != default!) {
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
            if (errΔ1 != default!) {
                Ꮡb.Fatal(errΔ1);
            }
            (_, _, errΔ1) = conn.ReadFromUDP(buf);
            if (errΔ1 != default!) {
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
        if (err != default!) {
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
            if (errΔ1 != default!) {
                Ꮡb.Fatal(errΔ1);
            }
            (_, _, errΔ1) = conn.ReadFromUDPAddrPort(buf);
            if (errΔ1 != default!) {
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

public static void TestUDPIPVersionReadMsg(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("skipping on %v"u8, Δruntime.GOOS);
        }

        if (!testableNetwork(udp4ˢ)) {
            Ꮡt.Skipf("skipping: udp4 not available"u8);
        }
        var (conn, err) = ListenUDP(udp4ˢ, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1))));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        var daddr = conn.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr()._<ж<global::go.net_package.UDPAddr>>().AddrPort();
        var buf = new slice<byte>(8);
        (_, err) = conn.WriteToUDPAddrPort(buf, daddr);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (_, _, _, var saddr, err) = conn.ReadMsgUDPAddrPort(buf, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!saddr.Addr().Is4()) {
            Ꮡt.Error(returnedAddrPortIsNotˢ);
        }
        (_, err) = conn.WriteToUDPAddrPort(buf, daddr);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (_, _, _, var soldaddr, err) = conn.ReadMsgUDP(buf, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len((~soldaddr).IP) != 4) {
            Ꮡt.Error(returnedUDPAddrIsNotIPv4ˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ffff127001ˢ = "::ffff:127.0.0.1"u8;

// TestIPv6WriteMsgUDPAddrPortTargetAddrIPVersion verifies that
// WriteMsgUDPAddrPort accepts IPv4, IPv4-mapped IPv6, and IPv6 target addresses
// on a UDPConn listening on "::".
public static void TestIPv6WriteMsgUDPAddrPortTargetAddrIPVersion(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(udp4ˢ)) {
            Ꮡt.Skipf("skipping: udp4 not available"u8);
        }
        if (!testableNetwork(udp6ˢ)) {
            Ꮡt.Skipf("skipping: udp6 not available"u8);
        }
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "openbsd"u8) {
            Ꮡt.Skipf("skipping on %v"u8, // DragonflyBSD's IPv6 sockets are always IPv6-only, according to the man page:
 // https://www.dragonflybsd.org/cgi/web-man?command=ip6 (search for IPV6_V6ONLY).
 // OpenBSD's IPv6 sockets are always IPv6-only, according to the man page:
 // https://man.openbsd.org/ip6#IPV6_V6ONLY
 Δruntime.GOOS);
        }

        var (conn, err) = ListenUDP(udpˢ, nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        var daddr4 = netip.AddrPortFrom(netip.MustParseAddr("127.0.0.1"u8), 12345);
        var daddr4in6 = netip.AddrPortFrom(netip.MustParseAddr(ffff127001ˢ), 12345);
        var daddr6 = netip.AddrPortFrom(netip.MustParseAddr("::1"u8), 12345);
        var buf = new slice<byte>(8);
        (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default!, daddr4);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default!, daddr4in6);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default!, daddr6);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestIPv4WriteMsgUDPAddrPortTargetAddrIPVersion verifies that
// WriteMsgUDPAddrPort accepts IPv4 and IPv4-mapped IPv6 destination addresses,
// and rejects IPv6 destination addresses on a "udp4" connection.
public static void TestIPv4WriteMsgUDPAddrPortTargetAddrIPVersion(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        if (!testableNetwork(udp4ˢ)) {
            Ꮡt.Skipf("skipping: udp4 not available"u8);
        }
        var (conn, err) = ListenUDP(udp4ˢ, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1))));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.of(global::go.net_package.UDPConn.Ꮡconn).Close(), ref ᒐ);
        var daddr4 = netip.AddrPortFrom(netip.MustParseAddr("127.0.0.1"u8), 12345);
        var daddr4in6 = netip.AddrPortFrom(netip.MustParseAddr(ffff127001ˢ), 12345);
        var daddr6 = netip.AddrPortFrom(netip.MustParseAddr("::1"u8), 12345);
        var buf = new slice<byte>(8);
        {
            (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default!, daddr4); if (err != default!) {
                Ꮡt.Errorf("conn.WriteMsgUDPAddrPort(buf, nil, daddr4) failed: %v"u8, err);
            }
        }
        {
            (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default!, daddr4in6); if (err != default!) {
                Ꮡt.Errorf("conn.WriteMsgUDPAddrPort(buf, nil, daddr4in6) failed: %v"u8, err);
            }
        }
        {
            (_, _, err) = conn.WriteMsgUDPAddrPort(buf, default!, daddr6); if (err == default!) {
                Ꮡt.Errorf("conn.WriteMsgUDPAddrPort(buf, nil, daddr6) should have failed, but got no error"u8);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
