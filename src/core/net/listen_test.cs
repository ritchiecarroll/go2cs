// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !plan9
namespace go;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using testing = testing_package;
using time = time_package;
using @internal;
using static go.net_package;

partial class net_internal_test_package {

internal static @string port(this ref global::go.net_package.TCPListener ln) {
    var (_, port, err) = SplitHostPort(ln.Addr().String());
    if (err != nil) {
        return ""u8;
    }
    return port;
}

internal static @string port(this ж<global::go.net_package.UDPConn> Ꮡc) {
    var (_, port, err) = SplitHostPort(Ꮡc.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr().String());
    if (err != nil) {
        return ""u8;
    }
    return port;
}

internal static slice<prohibitionaryDialArgTestsᴛ1> tcpListenerTests = [
    new("tcp", ""),
    new("tcp", "0.0.0.0"),
    new("tcp", "::ffff:0.0.0.0"),
    new("tcp", "::"),

    new("tcp", "127.0.0.1"),
    new("tcp", "::ffff:127.0.0.1"),
    new("tcp", "::1"),

    new("tcp4", ""),
    new("tcp4", "0.0.0.0"),
    new("tcp4", "::ffff:0.0.0.0"),

    new("tcp4", "127.0.0.1"),
    new("tcp4", "::ffff:127.0.0.1"),

    new("tcp6", ""),
    new("tcp6", "::"),

    new("tcp6", "::1")
];

// TestTCPListener tests both single and double listen to a test
// listener with same address family, same listening address and
// same port.
public static void TestTCPListener(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }}

    foreach (var (_, tt) in range(tcpListenerTests)) {
        if (!testableListenArgs(tt.network, JoinHostPort(tt.address, "0"), ""u8)) {
            t.Logf("skipping %s test", tt.network + " " + tt.address);
            continue;
        }

        var (ln1, err) = Listen(tt.network, JoinHostPort(tt.address, "0"));
        if (err != nil) {
            t.Fatal(err);
        }
        var errΔ1 = checkFirstListener(tt.network, ln1); if (errΔ1 != nil) {
            ln1.Close();
            t.Fatal(errΔ1);
        }
        (var ln2, err) = Listen(tt.network, JoinHostPort(tt.address, ln1._<ж<global::go.net_package.TCPListener>>().port()));
        if (err == nil) {
            ln2.Close();
        }
        var errΔ2 = checkSecondListener(tt.network, tt.address, err); if (errΔ2 != nil) {
            ln1.Close();
            t.Fatal(errΔ2);
        }
        ln1.Close();
    }
}

internal static slice<prohibitionaryDialArgTestsᴛ1> udpListenerTests = [
    new("udp", ""),
    new("udp", "0.0.0.0"),
    new("udp", "::ffff:0.0.0.0"),
    new("udp", "::"),

    new("udp", "127.0.0.1"),
    new("udp", "::ffff:127.0.0.1"),
    new("udp", "::1"),

    new("udp4", ""),
    new("udp4", "0.0.0.0"),
    new("udp4", "::ffff:0.0.0.0"),

    new("udp4", "127.0.0.1"),
    new("udp4", "::ffff:127.0.0.1"),

    new("udp6", ""),
    new("udp6", "::"),

    new("udp6", "::1")
];

// TestUDPListener tests both single and double listen to a test
// listener with same address family, same listening address and
// same port.
public static void TestUDPListener(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }}

    foreach (var (_, tt) in range(udpListenerTests)) {
        if (!testableListenArgs(tt.network, JoinHostPort(tt.address, "0"), ""u8)) {
            t.Logf("skipping %s test", tt.network + " " + tt.address);
            continue;
        }

        var (c1, err) = ListenPacket(tt.network, JoinHostPort(tt.address, "0"));
        if (err != nil) {
            t.Fatal(err);
        }
        var errΔ1 = checkFirstListener(tt.network, c1); if (errΔ1 != nil) {
            c1.Close();
            t.Fatal(errΔ1);
        }
        (var c2, err) = ListenPacket(tt.network, JoinHostPort(tt.address, c1._<ж<global::go.net_package.UDPConn>>().port()));
        if (err == nil) {
            c2.Close();
        }
        var errΔ2 = checkSecondListener(tt.network, tt.address, err); if (errΔ2 != nil) {
            c1.Close();
            t.Fatal(errΔ2);
        }
        c1.Close();
    }
}

partial struct dualStackTCPListenerTestsᴛ1 /*dyn*/ {
    internal @string network1, address1; // first listener
    internal @string network2, address2; // second listener
    internal error xerr;                 // expected error value, nil or other
}
internal static slice<dualStackTCPListenerTestsᴛ1> dualStackTCPListenerTests = [
    // Test cases and expected results for the attempting 2nd listen on the same port
    // 1st listen                2nd listen                 darwin  freebsd  linux  openbsd
    // ------------------------------------------------------------------------------------
    // "tcp"  ""                 "tcp"  ""                    -        -       -       -
    // "tcp"  ""                 "tcp"  "0.0.0.0"             -        -       -       -
    // "tcp"  "0.0.0.0"          "tcp"  ""                    -        -       -       -
    // ------------------------------------------------------------------------------------
    // "tcp"  ""                 "tcp"  "[::]"                -        -       -       ok
    // "tcp"  "[::]"             "tcp"  ""                    -        -       -       ok
    // "tcp"  "0.0.0.0"          "tcp"  "[::]"                -        -       -       ok
    // "tcp"  "[::]"             "tcp"  "0.0.0.0"             -        -       -       ok
    // "tcp"  "[::ffff:0.0.0.0]" "tcp"  "[::]"                -        -       -       ok
    // "tcp"  "[::]"             "tcp"  "[::ffff:0.0.0.0]"    -        -       -       ok
    // ------------------------------------------------------------------------------------
    // "tcp4" ""                 "tcp6" ""                    ok       ok      ok      ok
    // "tcp6" ""                 "tcp4" ""                    ok       ok      ok      ok
    // "tcp4" "0.0.0.0"          "tcp6" "[::]"                ok       ok      ok      ok
    // "tcp6" "[::]"             "tcp4" "0.0.0.0"             ok       ok      ok      ok
    // ------------------------------------------------------------------------------------
    // "tcp"  "127.0.0.1"        "tcp"  "[::1]"               ok       ok      ok      ok
    // "tcp"  "[::1]"            "tcp"  "127.0.0.1"           ok       ok      ok      ok
    // "tcp4" "127.0.0.1"        "tcp6" "[::1]"               ok       ok      ok      ok
    // "tcp6" "[::1]"            "tcp4" "127.0.0.1"           ok       ok      ok      ok
    //
    // Platform default configurations:
    // darwin, kernel version 11.3.0
    //	net.inet6.ip6.v6only=0 (overridable by sysctl or IPV6_V6ONLY option)
    // freebsd, kernel version 8.2
    //	net.inet6.ip6.v6only=1 (overridable by sysctl or IPV6_V6ONLY option)
    // linux, kernel version 3.0.0
    //	net.ipv6.bindv6only=0 (overridable by sysctl or IPV6_V6ONLY option)
    // openbsd, kernel version 5.0
    //	net.inet6.ip6.v6only=1 (overriding is prohibited)
    new("tcp", "", "tcp", "", syscall.EADDRINUSE),
    new("tcp", "", "tcp", "0.0.0.0", syscall.EADDRINUSE),
    new("tcp", "0.0.0.0", "tcp", "", syscall.EADDRINUSE),

    new("tcp", "", "tcp", "::", syscall.EADDRINUSE),
    new("tcp", "::", "tcp", "", syscall.EADDRINUSE),
    new("tcp", "0.0.0.0", "tcp", "::", syscall.EADDRINUSE),
    new("tcp", "::", "tcp", "0.0.0.0", syscall.EADDRINUSE),
    new("tcp", "::ffff:0.0.0.0", "tcp", "::", syscall.EADDRINUSE),
    new("tcp", "::", "tcp", "::ffff:0.0.0.0", syscall.EADDRINUSE),

    new("tcp4", "", "tcp6", "", default),
    new("tcp6", "", "tcp4", "", default),
    new("tcp4", "0.0.0.0", "tcp6", "::", default),
    new("tcp6", "::", "tcp4", "0.0.0.0", default),

    new("tcp", "127.0.0.1", "tcp", "::1", default),
    new("tcp", "::1", "tcp", "127.0.0.1", default),
    new("tcp4", "127.0.0.1", "tcp6", "::1", default),
    new("tcp6", "::1", "tcp4", "127.0.0.1", default)
];

// TestDualStackTCPListener tests both single and double listen
// to a test listener with various address families, different
// listening address and same port.
//
// On DragonFly BSD, we expect the kernel version of node under test
// to be greater than or equal to 4.4.
public static void TestDualStackTCPListener(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }}
    if (!supportsIPv4() || !supportsIPv6()) {
        t.Skip(bothIPv4AndIPv6Areˢ);
    }

    foreach (var (_, vᴛ1) in range(dualStackTCPListenerTests)) {
        var tt = vᴛ1;

        if (!testableListenArgs(tt.network1, JoinHostPort(tt.address1, "0"), ""u8)) {
            t.Logf("skipping %s test", tt.network1 + " " + tt.address1);
            continue;
        }

        if (!supportsIPv4map() && differentWildcardAddr(tt.address1, tt.address2)) {
            tt.xerr = default;
        }
        error firstErr = default;
        error secondErr = default;
        for (nint i = 0; i < 5; i++) {
            var (lns, err) = newDualStackListener();
            if (err != nil) {
                t.Fatal(err);
            }
            @string port = lns[0].port();
            foreach (var (_, ln) in range(lns)) {
                ln.Close();
            }
            global::go.net_package.Listener ln1 = default;
            (ln1, firstErr) = Listen(tt.network1, JoinHostPort(tt.address1, port));
            if (firstErr != nil) {
                continue;
            }
            var errΔ1 = checkFirstListener(tt.network1, ln1); if (errΔ1 != nil) {
                ln1.Close();
                t.Fatal(errΔ1);
            }
            (var ln2, err) = Listen(tt.network2, JoinHostPort(tt.address2, ln1._<ж<global::go.net_package.TCPListener>>().port()));
            if (err == nil) {
                ln2.Close();
            }
            secondErr = checkDualStackSecondListener(tt.network2, tt.address2, err, tt.xerr); if (secondErr != nil) {
                ln1.Close();
                continue;
            }
            ln1.Close();
            break;
        }
        if (firstErr != nil) {
            t.Error(firstErr);
        }
        if (secondErr != nil) {
            t.Error(secondErr);
        }
    }
}

internal static slice<dualStackTCPListenerTestsᴛ1> dualStackUDPListenerTests = [
    new("udp", "", "udp", "", syscall.EADDRINUSE),
    new("udp", "", "udp", "0.0.0.0", syscall.EADDRINUSE),
    new("udp", "0.0.0.0", "udp", "", syscall.EADDRINUSE),

    new("udp", "", "udp", "::", syscall.EADDRINUSE),
    new("udp", "::", "udp", "", syscall.EADDRINUSE),
    new("udp", "0.0.0.0", "udp", "::", syscall.EADDRINUSE),
    new("udp", "::", "udp", "0.0.0.0", syscall.EADDRINUSE),
    new("udp", "::ffff:0.0.0.0", "udp", "::", syscall.EADDRINUSE),
    new("udp", "::", "udp", "::ffff:0.0.0.0", syscall.EADDRINUSE),

    new("udp4", "", "udp6", "", default),
    new("udp6", "", "udp4", "", default),
    new("udp4", "0.0.0.0", "udp6", "::", default),
    new("udp6", "::", "udp4", "0.0.0.0", default),

    new("udp", "127.0.0.1", "udp", "::1", default),
    new("udp", "::1", "udp", "127.0.0.1", default),
    new("udp4", "127.0.0.1", "udp6", "::1", default),
    new("udp6", "::1", "udp4", "127.0.0.1", default)
];

// TestDualStackUDPListener tests both single and double listen
// to a test listener with various address families, different
// listening address and same port.
//
// On DragonFly BSD, we expect the kernel version of node under test
// to be greater than or equal to 4.4.
public static void TestDualStackUDPListener(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }}
    if (!supportsIPv4() || !supportsIPv6()) {
        t.Skip(bothIPv4AndIPv6Areˢ);
    }

    foreach (var (_, vᴛ1) in range(dualStackUDPListenerTests)) {
        var tt = vᴛ1;

        if (!testableListenArgs(tt.network1, JoinHostPort(tt.address1, "0"), ""u8)) {
            t.Logf("skipping %s test", tt.network1 + " " + tt.address1);
            continue;
        }

        if (!supportsIPv4map() && differentWildcardAddr(tt.address1, tt.address2)) {
            tt.xerr = default;
        }
        error firstErr = default;
        error secondErr = default;
        for (nint i = 0; i < 5; i++) {
            var (cs, err) = newDualStackPacketListener();
            if (err != nil) {
                t.Fatal(err);
            }
            @string port = cs[0].port();
            foreach (var (_, c) in range(cs)) {
                c.of(global::go.net_package.UDPConn.Ꮡconn).Close();
            }
            global::go.net_package.PacketConn c1 = default;
            (c1, firstErr) = ListenPacket(tt.network1, JoinHostPort(tt.address1, port));
            if (firstErr != nil) {
                continue;
            }
            var errΔ1 = checkFirstListener(tt.network1, c1); if (errΔ1 != nil) {
                c1.Close();
                t.Fatal(errΔ1);
            }
            (var c2, err) = ListenPacket(tt.network2, JoinHostPort(tt.address2, c1._<ж<global::go.net_package.UDPConn>>().port()));
            if (err == nil) {
                c2.Close();
            }
            secondErr = checkDualStackSecondListener(tt.network2, tt.address2, err, tt.xerr); if (secondErr != nil) {
                c1.Close();
                continue;
            }
            c1.Close();
            break;
        }
        if (firstErr != nil) {
            t.Error(firstErr);
        }
        if (secondErr != nil) {
            t.Error(secondErr);
        }
    }
}

internal static bool differentWildcardAddr(@string i, @string j) {
    if ((i == ""u8 || i == "0.0.0.0"u8 || i == "::ffff:0.0.0.0"u8) && (j == ""u8 || j == "0.0.0.0"u8 || j == "::ffff:0.0.0.0"u8)) {
        return false;
    }
    if (i == "[::]"u8 && j == "[::]"u8) {
        return false;
    }
    return true;
}

internal static error checkFirstListener(@string network, any ln) {
    var exprᴛ1 = network;
    switch (ᐧ) {
    case {} when exprᴛ1 == "tcp"u8: {
        var fd = ln._<ж<global::go.net_package.TCPListener>>().fd;
        {
            var err = checkDualStackAddrFamily(fd); if (err != nil) {
                return err;
            }
        }
        break;
    }
    case {} when exprᴛ1 == "tcp4"u8: {
        var fd = ln._<ж<global::go.net_package.TCPListener>>().fd;
        if (fd.family != syscall.AF_INET) {
            return fmt.Errorf("%v got %v; want %v"u8, fd.laddr, fd.family, (nint)(syscall.AF_INET));
        }
        break;
    }
    case {} when exprᴛ1 == "tcp6"u8: {
        var fd = ln._<ж<global::go.net_package.TCPListener>>().fd;
        if (fd.family != syscall.AF_INET6) {
            return fmt.Errorf("%v got %v; want %v"u8, fd.laddr, fd.family, (nint)(syscall.AF_INET6));
        }
        break;
    }
    case {} when exprᴛ1 == "udp"u8: {
        var fd = ln._<ж<global::go.net_package.UDPConn>>().fd;
        {
            var err = checkDualStackAddrFamily(fd); if (err != nil) {
                return err;
            }
        }
        break;
    }
    case {} when exprᴛ1 == "udp4"u8: {
        var fd = ln._<ж<global::go.net_package.UDPConn>>().fd;
        if (fd.family != syscall.AF_INET) {
            return fmt.Errorf("%v got %v; want %v"u8, fd.laddr, fd.family, (nint)(syscall.AF_INET));
        }
        break;
    }
    case {} when exprᴛ1 == "udp6"u8: {
        var fd = ln._<ж<global::go.net_package.UDPConn>>().fd;
        if (fd.family != syscall.AF_INET6) {
            return fmt.Errorf("%v got %v; want %v"u8, fd.laddr, fd.family, (nint)(syscall.AF_INET6));
        }
        break;
    }
    default: {
        return new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)network));
    }}
    return default;
}

internal static error checkSecondListener(@string network, @string address, error err) {
    var exprᴛ1 = network;
    switch (ᐧ) {
    case {} when exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8: {
        if (err == nil) {
            return fmt.Errorf("%s should fail"u8, network + " " + address);
        }
        break;
    }
    case {} when exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8: {
        if (err == nil) {
            return fmt.Errorf("%s should fail"u8, network + " " + address);
        }
        break;
    }
    default: {
        return new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)network));
    }}
    return default;
}

internal static error checkDualStackSecondListener(@string network, @string address, error err, error xerr) {
    var exprᴛ1 = network;
    switch (ᐧ) {
    case {} when exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8: {
        if (xerr == nil && err != nil || xerr != nil && err == nil) {
            return fmt.Errorf("%s got %v; want %v"u8, network + " " + address, err, xerr);
        }
        break;
    }
    case {} when exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8: {
        if (xerr == nil && err != nil || xerr != nil && err == nil) {
            return fmt.Errorf("%s got %v; want %v"u8, network + " " + address, err, xerr);
        }
        break;
    }
    default: {
        return new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)network));
    }}
    return default;
}

internal static error checkDualStackAddrFamily(ж<global::go.net_package.netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    switch (fd.laddr.type()) {
    case ж<global::go.net_package.TCPAddr> a: {
        // If a node under test supports both IPv6 capability
        // and IPv6 IPv4-mapping capability, we can assume
        // that the node listens on a wildcard address with an
        // AF_INET6 socket.
        if (supportsIPv4map() && fd.laddr._<ж<global::go.net_package.TCPAddr>>().isWildcard()) {
            if (fd.family != syscall.AF_INET6) {
                return fmt.Errorf("Listen(%s, %v) returns %v; want %v"u8, fd.net, fd.laddr, fd.family, (nint)(syscall.AF_INET6));
            }
        } else {
            if (fd.family != a.family()) {
                return fmt.Errorf("Listen(%s, %v) returns %v; want %v"u8, fd.net, fd.laddr, fd.family, a.family());
            }
        }
        break;
    }
    case ж<global::go.net_package.UDPAddr> a: {
        // If a node under test supports both IPv6 capability
        // and IPv6 IPv4-mapping capability, we can assume
        // that the node listens on a wildcard address with an
        // AF_INET6 socket.
        if (supportsIPv4map() && fd.laddr._<ж<global::go.net_package.UDPAddr>>().isWildcard()) {
            if (fd.family != syscall.AF_INET6) {
                return fmt.Errorf("ListenPacket(%s, %v) returns %v; want %v"u8, fd.net, fd.laddr, fd.family, (nint)(syscall.AF_INET6));
            }
        } else {
            if (fd.family != a.family()) {
                return fmt.Errorf("ListenPacket(%s, %v) returns %v; want %v"u8, fd.net, fd.laddr, fd.family, a.family());
            }
        }
        break;
    }
    default: {
        var a = fd.laddr;
        return fmt.Errorf("unexpected protocol address type: %T"u8, a);
    }}
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ipIcmpˢ => ref ipIcmpˢᵇ;
private static readonly @string ipIcmpˢᵇ = "ip:icmp"u8;

public static void TestWildWildcardListener(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("not supported on %s", Δruntime.GOOS);
            break;
        }}

        defer(() => {
            var p = recover(); if (p != nil) {
                t.Fatalf("panicked: %v", p);
            }
        }, ref ᒐ);

        {
            var (ln, err) = Listen(tcpˢ, ""u8); if (err == nil) {
                ln.Close();
            }
        }
        {
            var (ln, err) = ListenPacket(udpˢ, ""u8); if (err == nil) {
                ln.Close();
            }
        }
        {
            var (ln, err) = ListenTCP(tcpˢ, nil); if (err == nil) {
                ln.Close();
            }
        }
        {
            var (ln, err) = ListenUDP(udpˢ, nil); if (err == nil) {
                ln.of(global::go.net_package.UDPConn.Ꮡconn).Close();
            }
        }
        {
            var (ln, err) = ListenIP(ipIcmpˢ, nil); if (err == nil) {
                ln.of(global::go.net_package.IPConn.Ꮡconn).Close();
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct ipv4MulticastListenerTestsᴛ1 /*dyn*/ {
    internal @string net;
    internal ж<global::go.net_package.UDPAddr> gaddr; // see RFC 4727
}
internal static slice<ipv4MulticastListenerTestsᴛ1> ipv4MulticastListenerTests;
internal static void initᴛipv4MulticastListenerTests() { ipv4MulticastListenerTests = slice<ipv4MulticastListenerTestsᴛ1>([
    new("udp", Ꮡ(new UDPAddr(IP: IPv4(224, 0, 0, 254), Port: 12345))),

    new("udp4", Ꮡ(new UDPAddr(IP: IPv4(224, 0, 0, 254), Port: 12345)))
]); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object iPv4IsNotSupportedˢ = (@string)"IPv4 is not supported"u8;

// TestIPv4MulticastListener tests both single and double listen to a
// test listener with same address family, same group address and same
// port.
public static void TestIPv4MulticastListener(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "android"u8 || exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }}
    if (!supportsIPv4()) {
        t.Skip(iPv4IsNotSupportedˢ);
    }

    void closer(slice<ж<global::go.net_package.UDPConn>> cs) {
        foreach (var (_, c) in range(cs)) {
            if (c != nil) {
                c.of(global::go.net_package.UDPConn.Ꮡconn).Close();
            }
        }
    }

    foreach (var (_, ifi) in range(slice<ж<global::go.net_package.Interface>>([loopbackInterface(), default]))) {
        // Note that multicast interface assignment by system
        // is not recommended because it usually relies on
        // routing stuff for finding out an appropriate
        // nexthop containing both network and link layer
        // adjacencies.
        if (ifi == nil || !testIPv4.Value) {
            continue;
        }
        foreach (var (_, tt) in range(ipv4MulticastListenerTests)) {
            error err = default;
            var cs = new slice<ж<global::go.net_package.UDPConn>>(2);
            (cs[0], err) = ListenMulticastUDP(tt.net, ifi, tt.gaddr); if (err != nil) {
                t.Fatal(err);
            }
            var errΔ1 = checkMulticastListener(cs[0], tt.gaddr.IP); if (errΔ1 != nil) {
                closer(cs);
                t.Fatal(errΔ1);
            }
            (cs[1], err) = ListenMulticastUDP(tt.net, ifi, tt.gaddr); if (err != nil) {
                closer(cs);
                t.Fatal(err);
            }
            var errΔ2 = checkMulticastListener(cs[1], tt.gaddr.IP); if (errΔ2 != nil) {
                closer(cs);
                t.Fatal(errΔ2);
            }
            closer(cs);
        }
    }
}

internal static slice<ipv4MulticastListenerTestsᴛ1> ipv6MulticastListenerTests = slice<ipv4MulticastListenerTestsᴛ1>([
    new("udp", Ꮡ(new UDPAddr(IP: ParseIP("ff01::114"), Port: 12345))),
    new("udp", Ꮡ(new UDPAddr(IP: ParseIP("ff02::114"), Port: 12345))),
    new("udp", Ꮡ(new UDPAddr(IP: ParseIP("ff04::114"), Port: 12345))),
    new("udp", Ꮡ(new UDPAddr(IP: ParseIP("ff05::114"), Port: 12345))),
    new("udp", Ꮡ(new UDPAddr(IP: ParseIP("ff08::114"), Port: 12345))),
    new("udp", Ꮡ(new UDPAddr(IP: ParseIP("ff0e::114"), Port: 12345))),

    new("udp6", Ꮡ(new UDPAddr(IP: ParseIP("ff01::114"), Port: 12345))),
    new("udp6", Ꮡ(new UDPAddr(IP: ParseIP("ff02::114"), Port: 12345))),
    new("udp6", Ꮡ(new UDPAddr(IP: ParseIP("ff04::114"), Port: 12345))),
    new("udp6", Ꮡ(new UDPAddr(IP: ParseIP("ff05::114"), Port: 12345))),
    new("udp6", Ꮡ(new UDPAddr(IP: ParseIP("ff08::114"), Port: 12345))),
    new("udp6", Ꮡ(new UDPAddr(IP: ParseIP("ff0e::114"), Port: 12345)))
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object iPv6IsNotSupportedˢ = (@string)"IPv6 is not supported"u8;
internal static readonly object mustBeRootˢ = (@string)"must be root"u8;

// TestIPv6MulticastListener tests both single and double listen to a
// test listener with same address family, same group address and same
// port.
public static void TestIPv6MulticastListener(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }}
    if (!supportsIPv6()) {
        t.Skip(iPv6IsNotSupportedˢ);
    }
    // On Windows, the test can be run by non-admin users.
    if (Δruntime.GOOS != "windows"u8 && Δos.Getuid() != 0) {
        t.Skip(mustBeRootˢ);
    }

    void closer(slice<ж<global::go.net_package.UDPConn>> cs) {
        foreach (var (_, c) in range(cs)) {
            if (c != nil) {
                c.of(global::go.net_package.UDPConn.Ꮡconn).Close();
            }
        }
    }

    foreach (var (_, ifi) in range(slice<ж<global::go.net_package.Interface>>([loopbackInterface(), default]))) {
        // Note that multicast interface assignment by system
        // is not recommended because it usually relies on
        // routing stuff for finding out an appropriate
        // nexthop containing both network and link layer
        // adjacencies.
        if (ifi == nil && !testIPv6.Value) {
            continue;
        }
        foreach (var (_, tt) in range(ipv6MulticastListenerTests)) {
            error err = default;
            var cs = new slice<ж<global::go.net_package.UDPConn>>(2);
            (cs[0], err) = ListenMulticastUDP(tt.net, ifi, tt.gaddr); if (err != nil) {
                t.Fatal(err);
            }
            var errΔ1 = checkMulticastListener(cs[0], tt.gaddr.IP); if (errΔ1 != nil) {
                closer(cs);
                t.Fatal(errΔ1);
            }
            (cs[1], err) = ListenMulticastUDP(tt.net, ifi, tt.gaddr); if (err != nil) {
                closer(cs);
                t.Fatal(err);
            }
            var errΔ2 = checkMulticastListener(cs[1], tt.gaddr.IP); if (errΔ2 != nil) {
                closer(cs);
                t.Fatal(errΔ2);
            }
            closer(cs);
        }
    }
}

internal static error checkMulticastListener(ж<global::go.net_package.UDPConn> c, global::go.net_package.IP ip) {
    {
        var (ok, err) = multicastRIBContains(ip); if (err != nil) {
            return err;
        } else if (!ok) {
            return fmt.Errorf("%s not found in multicast rib"u8, ip.String());
        }
    }
    var la = c.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr();
    {
        var (laΔ1, ok) = la._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (!ok || laΔ1.Port == 0) {
            return fmt.Errorf("got %v; want a proper address with non-zero port number"u8, laΔ1.OrTypedNil());
        }
    }
    return default;
}

internal static (bool, error) multicastRIBContains(global::go.net_package.IP ip) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "aix"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "plan9"u8 || exprᴛ1 == "solaris"u8 || exprᴛ1 == "illumos"u8: {
        return (true, default); // not implemented yet
    }
    case {} when exprᴛ1 == "linux"u8: {
        if (Δruntime.GOARCH == "arm"u8 || Δruntime.GOARCH == "alpha"u8) {
            return (true, default); // not implemented yet
        }
        break;
    }}
    var (ift, err) = Interfaces();
    if (err != nil) {
        return (false, err);
    }
    foreach (var (_, vᴛ1) in range(ift)) {
        ref var ifi = ref heap(new global::go.net_package.Interface(), out var Ꮡifi);
        ifi = vᴛ1;

        var (ifmat, errΔ1) = Ꮡifi.MulticastAddrs();
        if (errΔ1 != nil) {
            return (false, errΔ1);
        }
        foreach (var (_, ifma) in range(ifmat)) {
            if (ifma._<ж<global::go.net_package.IPAddr>>().IP.Equal(ip)) {
                return (true, default);
            }
        }
    }
    return (false, default);
}

// Issue 21856.
public static partial void TestClosingListener(ж<testing.T> t) {
    var ln = newLocalListener(new net_test_package.testing_TжTB(t), tcpˢ);
    var addr = ln.Addr();

    var lnʗ1 = ln;
    goǃ(() => {
        for (;;) {
            var (c, errΔ1) = lnʗ1.Accept();
            if (errΔ1 != nil) {
                return;
            }
            c.Close();
        }
    });

    // Let the goroutine start. We don't sleep long: if the
    // goroutine doesn't start, the test will pass without really
    // testing anything, which is OK.
    time.Sleep(time.Millisecond);

    ln.Close();

    var (ln2, err) = Listen(tcpˢ, addr.String());
    if (err != nil) {
        t.Fatal(err);
    }
    ln2.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string streamListenˢ => ref streamListenˢᵇ;
private static readonly @string streamListenˢᵇ = "StreamListen"u8;
internal static ref readonly @string packetListenˢ => ref packetListenˢᵇ;
private static readonly @string packetListenˢᵇ = "PacketListen"u8;

public static void TestListenConfigControl(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }}

    t.Run(streamListenˢ, (ж<testing.T> tΔ1) => {
        foreach (var (_, network) in range(slice<@string>(["tcp", "tcp4", "tcp6", "unix", "unixpacket"]))) {
            if (!testableNetwork(network)) {
                continue;
            }
            var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), network, Ꮡ(new ListenConfig(Control: controlOnConnSetup)));
            ln.Close();
        }
    });
    t.Run(packetListenˢ, (ж<testing.T> tΔ2) => {
        foreach (var (_, network) in range(slice<@string>(["udp", "udp4", "udp6", "unixgram"]))) {
            if (!testableNetwork(network)) {
                continue;
            }
            var c = newLocalPacketListener(new net_test_package.testing_TжTB(tΔ2), network, Ꮡ(new ListenConfig(Control: controlOnConnSetup)));
            c.Close();
        }
    });
}

} // end net_internal_test_package
