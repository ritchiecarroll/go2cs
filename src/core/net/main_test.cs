// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using flag = flag_package;
using fmt = fmt_package;
using socktest = net.@internal.socktest_package;
using Δos = os_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using net.@internal;
using static go.net_package;
using syscall = syscall_package;
using Δio = io_package;

partial class net_internal_test_package {

internal static ж<socktest.Switch> Ꮡsw = new StandardBox<socktest.Switch>(default(socktest.Switch));
internal static ref socktest.Switch sw => ref Ꮡsw.Value;
internal static ж<Δsync.Once> ᏑtestHookUninstaller = new StandardBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once testHookUninstaller => ref ᏑtestHookUninstaller.Value;

internal static ж<bool> testTCPBig = flag.Bool("tcpbig"u8, false, "whether to test massive size of data per read or write call on TCP connection"u8);
internal static ж<bool> testDNSFlood = flag.Bool("dnsflood"u8, false, "whether to test DNS query flooding"u8);
internal static ж<bool> testIPv4 = flag.Bool("ipv4"u8, true, "assume external IPv4 connectivity exists"u8);
internal static ж<bool> testIPv6 = flag.Bool("ipv6"u8, false, "assume external IPv6 connectivity exists"u8);

public static void TestMain(ж<testing.M> Ꮡm) {
    setupTestData();
    installTestHooks();
    nint st = Ꮡm.Run();
    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    if (testing.Verbose()) {
        printRunningGoroutines();
        printInflightSockets();
        printSocketStats();
    }
    forceCloseSockets();
    Δos.Exit(st);
}

// mustSetDeadline calls the bound method m to set a deadline on a Conn.
// If the call fails, mustSetDeadline skips t if the current GOOS is believed
// not to support deadlines, or fails the test otherwise.
internal static void mustSetDeadline(testing.TB t, Func<time.Time, error> m, time.Duration d) {
    var err = m(time.Now().Add(d));
    if (err != default!) {
        t.Helper();
        if (Δruntime.GOOS == "plan9"u8) {
            t.Skipf("skipping: %s does not support deadlines"u8, Δruntime.GOOS);
        }
        t.Fatal(err);
    }
}

[GoType] internal partial struct ipv6LinkLocalUnicastTest {
    internal @string network, address;
    internal bool nameLookup;
}

internal static slice<ipv6LinkLocalUnicastTest> ipv6LinkLocalUnicastTCPTests;
internal static slice<ipv6LinkLocalUnicastTest> ipv6LinkLocalUnicastUDPTests;

internal static void setupTestData() {
    if (supportsIPv4()) {
        resolveTCPAddrTests = appendꓸꓸꓸ(resolveTCPAddrTests, new resolveTCPAddrTest[]{
            new("tcp"u8, "localhost:1"u8, Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 1)), default!),
            new("tcp4"u8, "localhost:2"u8, Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 2)), default!)
        }.slice());
        resolveUDPAddrTests = appendꓸꓸꓸ(resolveUDPAddrTests, new resolveUDPAddrTest[]{
            new("udp"u8, "localhost:1"u8, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1), Port: 1)), default!),
            new("udp4"u8, "localhost:2"u8, Ꮡ(new UDPAddr(IP: IPv4(127, 0, 0, 1), Port: 2)), default!)
        }.slice());
        resolveIPAddrTests = appendꓸꓸꓸ(resolveIPAddrTests, new resolveIPAddrTest[]{
            new("ip"u8, "localhost"u8, Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), default!),
            new("ip4"u8, "localhost"u8, Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), default!)
        }.slice());
    }
    if (supportsIPv6()) {
        resolveTCPAddrTests = append(resolveTCPAddrTests, new resolveTCPAddrTest("tcp6"u8, "localhost:3"u8, Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 3)), default!));
        resolveUDPAddrTests = append(resolveUDPAddrTests, new resolveUDPAddrTest("udp6"u8, "localhost:3"u8, Ꮡ(new UDPAddr(IP: IPv6loopback, Port: 3)), default!));
        resolveIPAddrTests = append(resolveIPAddrTests, new resolveIPAddrTest("ip6"u8, "localhost"u8, Ꮡ(new IPAddr(IP: IPv6loopback)), default!));
        // Issue 20911: don't return IPv4 addresses for
        // Resolve*Addr calls of the IPv6 unspecified address.
        resolveTCPAddrTests = append(resolveTCPAddrTests, new resolveTCPAddrTest("tcp"u8, "[::]:4"u8, Ꮡ(new TCPAddr(IP: IPv6unspecified, Port: 4)), default!));
        resolveUDPAddrTests = append(resolveUDPAddrTests, new resolveUDPAddrTest("udp"u8, "[::]:4"u8, Ꮡ(new UDPAddr(IP: IPv6unspecified, Port: 4)), default!));
        resolveIPAddrTests = append(resolveIPAddrTests, new resolveIPAddrTest("ip"u8, "::"u8, Ꮡ(new IPAddr(IP: IPv6unspecified)), default!));
    }
    var ifi = loopbackInterface();
    if (ifi != nil) {
        ref var index = ref heap<@string>(out var Ꮡindex);
        index = fmt.Sprintf("%v"u8, (~ifi).Index);
        resolveTCPAddrTests = appendꓸꓸꓸ(resolveTCPAddrTests, new resolveTCPAddrTest[]{
            new("tcp6"u8, "[fe80::1%"u8 + (~ifi).Name + "]:1"u8, Ꮡ(new TCPAddr(IP: ParseIP(fe801ˢ), Port: 1, Zone: zoneCache.name((~ifi).Index))), default!),
            new("tcp6"u8, "[fe80::1%"u8 + index + "]:2"u8, Ꮡ(new TCPAddr(IP: ParseIP(fe801ˢ), Port: 2, Zone: index)), default!)
        }.slice());
        resolveUDPAddrTests = appendꓸꓸꓸ(resolveUDPAddrTests, new resolveUDPAddrTest[]{
            new("udp6"u8, "[fe80::1%"u8 + (~ifi).Name + "]:1"u8, Ꮡ(new UDPAddr(IP: ParseIP(fe801ˢ), Port: 1, Zone: zoneCache.name((~ifi).Index))), default!),
            new("udp6"u8, "[fe80::1%"u8 + index + "]:2"u8, Ꮡ(new UDPAddr(IP: ParseIP(fe801ˢ), Port: 2, Zone: index)), default!)
        }.slice());
        resolveIPAddrTests = appendꓸꓸꓸ(resolveIPAddrTests, new resolveIPAddrTest[]{
            new("ip6"u8, "fe80::1%"u8 + (~ifi).Name, Ꮡ(new IPAddr(IP: ParseIP(fe801ˢ), Zone: zoneCache.name((~ifi).Index))), default!),
            new("ip6"u8, "fe80::1%"u8 + index, Ꮡ(new IPAddr(IP: ParseIP(fe801ˢ), Zone: index)), default!)
        }.slice());
    }
    @string addr = ipv6LinkLocalUnicastAddr(ifi);
    if (addr != ""u8) {
        if (Δruntime.GOOS != "dragonfly"u8) {
            ipv6LinkLocalUnicastTCPTests = appendꓸꓸꓸ(ipv6LinkLocalUnicastTCPTests, new ipv6LinkLocalUnicastTest[]{
                new("tcp"u8, "["u8 + addr + "%"u8 + (~ifi).Name + "]:0"u8, false)
            }.slice());
            ipv6LinkLocalUnicastUDPTests = appendꓸꓸꓸ(ipv6LinkLocalUnicastUDPTests, new ipv6LinkLocalUnicastTest[]{
                new("udp"u8, "["u8 + addr + "%"u8 + (~ifi).Name + "]:0"u8, false)
            }.slice());
        }
        ipv6LinkLocalUnicastTCPTests = appendꓸꓸꓸ(ipv6LinkLocalUnicastTCPTests, new ipv6LinkLocalUnicastTest[]{
            new("tcp6"u8, "["u8 + addr + "%"u8 + (~ifi).Name + "]:0"u8, false)
        }.slice());
        ipv6LinkLocalUnicastUDPTests = appendꓸꓸꓸ(ipv6LinkLocalUnicastUDPTests, new ipv6LinkLocalUnicastTest[]{
            new("udp6"u8, "["u8 + addr + "%"u8 + (~ifi).Name + "]:0"u8, false)
        }.slice());
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "netbsd"u8) {
            ipv6LinkLocalUnicastTCPTests = appendꓸꓸꓸ(ipv6LinkLocalUnicastTCPTests, new ipv6LinkLocalUnicastTest[]{
                new("tcp"u8, "[localhost%"u8 + (~ifi).Name + "]:0"u8, true),
                new("tcp6"u8, "[localhost%"u8 + (~ifi).Name + "]:0"u8, true)
            }.slice());
            ipv6LinkLocalUnicastUDPTests = appendꓸꓸꓸ(ipv6LinkLocalUnicastUDPTests, new ipv6LinkLocalUnicastTest[]{
                new("udp"u8, "[localhost%"u8 + (~ifi).Name + "]:0"u8, true),
                new("udp6"u8, "[localhost%"u8 + (~ifi).Name + "]:0"u8, true)
            }.slice());
        }
        else if (exprᴛ1 == "linux"u8) {
            ipv6LinkLocalUnicastTCPTests = appendꓸꓸꓸ(ipv6LinkLocalUnicastTCPTests, new ipv6LinkLocalUnicastTest[]{
                new("tcp"u8, "[ip6-localhost%"u8 + (~ifi).Name + "]:0"u8, true),
                new("tcp6"u8, "[ip6-localhost%"u8 + (~ifi).Name + "]:0"u8, true)
            }.slice());
            ipv6LinkLocalUnicastUDPTests = appendꓸꓸꓸ(ipv6LinkLocalUnicastUDPTests, new ipv6LinkLocalUnicastTest[]{
                new("udp"u8, "[ip6-localhost%"u8 + (~ifi).Name + "]:0"u8, true),
                new("udp6"u8, "[ip6-localhost%"u8 + (~ifi).Name + "]:0"u8, true)
            }.slice());
        }

    }
}

internal static void printRunningGoroutines() {
    var gss = runningGoroutines();
    if (len(gss) == 0) {
        return;
    }
    fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "Running goroutines:\n"u8);
    foreach (var (_, gs) in gss) {
        fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "%v\n"u8, gs);
    }
    fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "\n"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string createdByNetˢ = "created by net"u8;

// runningGoroutines returns a list of remaining goroutines.
internal static partial slice<@string> runningGoroutines() {
    slice<@string> gss = default!;
    var b = new slice<byte>((2 << (int)(20)));
    b = b.slice(0, Δruntime.Stack(b, true));
    foreach (var (_, s) in strings.Split(((@string)b), "\n\n"u8)) {
        var (_, stack, _) = strings.Cut(s, "\n"u8);
        stack = strings.TrimSpace(stack);
        if (!strings.Contains(stack, createdByNetˢ)) {
            continue;
        }
        gss = append(gss, stack);
    }
    slices.Sort<slice<@string>, @string>(gss);
    return gss;
}

internal static void printInflightSockets() {
    var sos = Ꮡsw.Sockets();
    if (len(sos) == 0) {
        return;
    }
    fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "Inflight sockets:\n"u8);
    foreach (var (s, so) in sos) {
        fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "%v: %v\n"u8, s, so);
    }
    fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "\n"u8);
}

internal static void printSocketStats() {
    var sts = Ꮡsw.Stats();
    if (len(sts) == 0) {
        return;
    }
    fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "Socket statistical information:\n"u8);
    foreach (var (_, st) in sts) {
        fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "%v\n"u8, st);
    }
    fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "\n"u8);
}

} // end net_internal_test_package
