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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.net_package;

partial class net_internal_test_package {

internal static @string port(this ref global::go.net_package.TCPListener ln) {
    var (_, port, err) = SplitHostPort(ln.Addr().String());
    if (err != default!) {
        return ""u8;
    }
    return port;
}

internal static @string port(this ж<global::go.net_package.UDPConn> Ꮡc) {
    var (_, port, err) = SplitHostPort(Ꮡc.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr().String());
    if (err != default!) {
        return ""u8;
    }
    return port;
}

internal static slice<prohibitionaryDialArgTestsᴛ1> tcpListenerTests = new prohibitionaryDialArgTestsᴛ1[]{
    new("tcp"u8, ""u8),
    new("tcp"u8, "0.0.0.0"u8),
    new("tcp"u8, "::ffff:0.0.0.0"u8),
    new("tcp"u8, "::"u8),
    new("tcp"u8, "127.0.0.1"u8),
    new("tcp"u8, "::ffff:127.0.0.1"u8),
    new("tcp"u8, "::1"u8),
    new("tcp4"u8, ""u8),
    new("tcp4"u8, "0.0.0.0"u8),
    new("tcp4"u8, "::ffff:0.0.0.0"u8),
    new("tcp4"u8, "127.0.0.1"u8),
    new("tcp4"u8, "::ffff:127.0.0.1"u8),
    new("tcp6"u8, ""u8),
    new("tcp6"u8, "::"u8),
    new("tcp6"u8, "::1"u8)
}.slice();

// TestTCPListener tests both single and double listen to a test
// listener with same address family, same listening address and
// same port.
public static void TestTCPListener(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    foreach (var (_, tt) in tcpListenerTests) {
        if (!testableListenArgs(tt.network, JoinHostPort(tt.address, "0"u8), ""u8)) {
            Ꮡt.Logf("skipping %s test"u8, tt.network + " " + tt.address);
            continue;
        }
        var (ln1, err) = Listen(tt.network, JoinHostPort(tt.address, "0"u8));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            var errΔ1 = checkFirstListener(tt.network, ln1); if (errΔ1 != default!) {
                ln1.Close();
                Ꮡt.Fatal(errΔ1);
            }
        }
        (var ln2, err) = Listen(tt.network, JoinHostPort(tt.address, ln1._<ж<global::go.net_package.TCPListener>>().port()));
        if (err == default!) {
            ln2.Close();
        }
        {
            var errΔ2 = checkSecondListener(tt.network, tt.address, err); if (errΔ2 != default!) {
                ln1.Close();
                Ꮡt.Fatal(errΔ2);
            }
        }
        ln1.Close();
    }
}

internal static slice<prohibitionaryDialArgTestsᴛ1> udpListenerTests = new prohibitionaryDialArgTestsᴛ1[]{
    new("udp"u8, ""u8),
    new("udp"u8, "0.0.0.0"u8),
    new("udp"u8, "::ffff:0.0.0.0"u8),
    new("udp"u8, "::"u8),
    new("udp"u8, "127.0.0.1"u8),
    new("udp"u8, "::ffff:127.0.0.1"u8),
    new("udp"u8, "::1"u8),
    new("udp4"u8, ""u8),
    new("udp4"u8, "0.0.0.0"u8),
    new("udp4"u8, "::ffff:0.0.0.0"u8),
    new("udp4"u8, "127.0.0.1"u8),
    new("udp4"u8, "::ffff:127.0.0.1"u8),
    new("udp6"u8, ""u8),
    new("udp6"u8, "::"u8),
    new("udp6"u8, "::1"u8)
}.slice();

// TestUDPListener tests both single and double listen to a test
// listener with same address family, same listening address and
// same port.
public static void TestUDPListener(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    foreach (var (_, tt) in udpListenerTests) {
        if (!testableListenArgs(tt.network, JoinHostPort(tt.address, "0"u8), ""u8)) {
            Ꮡt.Logf("skipping %s test"u8, tt.network + " " + tt.address);
            continue;
        }
        var (c1, err) = ListenPacket(tt.network, JoinHostPort(tt.address, "0"u8));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            var errΔ1 = checkFirstListener(tt.network, c1); if (errΔ1 != default!) {
                c1.Close();
                Ꮡt.Fatal(errΔ1);
            }
        }
        (var c2, err) = ListenPacket(tt.network, JoinHostPort(tt.address, c1._<ж<global::go.net_package.UDPConn>>().port()));
        if (err == default!) {
            c2.Close();
        }
        {
            var errΔ2 = checkSecondListener(tt.network, tt.address, err); if (errΔ2 != default!) {
                c1.Close();
                Ꮡt.Fatal(errΔ2);
            }
        }
        c1.Close();
    }
}

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

partial struct dualStackTCPListenerTestsᴛ1 /*dyn*/ {
    internal @string network1, address1; // first listener
    internal @string network2, address2; // second listener
    internal error xerr;  // expected error value, nil or other
}
internal static slice<dualStackTCPListenerTestsᴛ1> dualStackTCPListenerTests = new dualStackTCPListenerTestsᴛ1[]{
    new("tcp"u8, ""u8, "tcp"u8, ""u8, syscall.EADDRINUSE),
    new("tcp"u8, ""u8, "tcp"u8, "0.0.0.0"u8, syscall.EADDRINUSE),
    new("tcp"u8, "0.0.0.0"u8, "tcp"u8, ""u8, syscall.EADDRINUSE),
    new("tcp"u8, ""u8, "tcp"u8, "::"u8, syscall.EADDRINUSE),
    new("tcp"u8, "::"u8, "tcp"u8, ""u8, syscall.EADDRINUSE),
    new("tcp"u8, "0.0.0.0"u8, "tcp"u8, "::"u8, syscall.EADDRINUSE),
    new("tcp"u8, "::"u8, "tcp"u8, "0.0.0.0"u8, syscall.EADDRINUSE),
    new("tcp"u8, "::ffff:0.0.0.0"u8, "tcp"u8, "::"u8, syscall.EADDRINUSE),
    new("tcp"u8, "::"u8, "tcp"u8, "::ffff:0.0.0.0"u8, syscall.EADDRINUSE),
    new("tcp4"u8, ""u8, "tcp6"u8, ""u8, default!),
    new("tcp6"u8, ""u8, "tcp4"u8, ""u8, default!),
    new("tcp4"u8, "0.0.0.0"u8, "tcp6"u8, "::"u8, default!),
    new("tcp6"u8, "::"u8, "tcp4"u8, "0.0.0.0"u8, default!),
    new("tcp"u8, "127.0.0.1"u8, "tcp"u8, "::1"u8, default!),
    new("tcp"u8, "::1"u8, "tcp"u8, "127.0.0.1"u8, default!),
    new("tcp4"u8, "127.0.0.1"u8, "tcp6"u8, "::1"u8, default!),
    new("tcp6"u8, "::1"u8, "tcp4"u8, "127.0.0.1"u8, default!)
}.slice();

// TestDualStackTCPListener tests both single and double listen
// to a test listener with various address families, different
// listening address and same port.
//
// On DragonFly BSD, we expect the kernel version of node under test
// to be greater than or equal to 4.4.
public static void TestDualStackTCPListener(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    if (!supportsIPv4() || !supportsIPv6()) {
        Ꮡt.Skip(bothIPv4AndIPv6Areˢ);
    }
    foreach (var (_, vᴛ1) in dualStackTCPListenerTests) {
        var tt = vᴛ1;

        if (!testableListenArgs(tt.network1, JoinHostPort(tt.address1, "0"u8), ""u8)) {
            Ꮡt.Logf("skipping %s test"u8, tt.network1 + " " + tt.address1);
            continue;
        }
        if (!supportsIPv4map() && differentWildcardAddr(tt.address1, tt.address2)) {
            tt.xerr = default!;
        }
        error firstErr = default!;
        error secondErr = default!;
        for (nint i = 0; i < 5; i++) {
            var (lns, err) = newDualStackListener();
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            @string port = lns[0].port();
            foreach (var (_, ln) in lns) {
                ln.Close();
            }
            global::go.net_package.Listener ln1 = default!;
            (ln1, firstErr) = Listen(tt.network1, JoinHostPort(tt.address1, port));
            if (firstErr != default!) {
                continue;
            }
            {
                var errΔ1 = checkFirstListener(tt.network1, ln1); if (errΔ1 != default!) {
                    ln1.Close();
                    Ꮡt.Fatal(errΔ1);
                }
            }
            (var ln2, err) = Listen(tt.network2, JoinHostPort(tt.address2, ln1._<ж<global::go.net_package.TCPListener>>().port()));
            if (err == default!) {
                ln2.Close();
            }
            {
                secondErr = checkDualStackSecondListener(tt.network2, tt.address2, err, tt.xerr); if (secondErr != default!) {
                    ln1.Close();
                    continue;
                }
            }
            ln1.Close();
            break;
        }
        if (firstErr != default!) {
            Ꮡt.Error(firstErr);
        }
        if (secondErr != default!) {
            Ꮡt.Error(secondErr);
        }
    }
}

internal static slice<dualStackTCPListenerTestsᴛ1> dualStackUDPListenerTests = new dualStackTCPListenerTestsᴛ1[]{
    new("udp"u8, ""u8, "udp"u8, ""u8, syscall.EADDRINUSE),
    new("udp"u8, ""u8, "udp"u8, "0.0.0.0"u8, syscall.EADDRINUSE),
    new("udp"u8, "0.0.0.0"u8, "udp"u8, ""u8, syscall.EADDRINUSE),
    new("udp"u8, ""u8, "udp"u8, "::"u8, syscall.EADDRINUSE),
    new("udp"u8, "::"u8, "udp"u8, ""u8, syscall.EADDRINUSE),
    new("udp"u8, "0.0.0.0"u8, "udp"u8, "::"u8, syscall.EADDRINUSE),
    new("udp"u8, "::"u8, "udp"u8, "0.0.0.0"u8, syscall.EADDRINUSE),
    new("udp"u8, "::ffff:0.0.0.0"u8, "udp"u8, "::"u8, syscall.EADDRINUSE),
    new("udp"u8, "::"u8, "udp"u8, "::ffff:0.0.0.0"u8, syscall.EADDRINUSE),
    new("udp4"u8, ""u8, "udp6"u8, ""u8, default!),
    new("udp6"u8, ""u8, "udp4"u8, ""u8, default!),
    new("udp4"u8, "0.0.0.0"u8, "udp6"u8, "::"u8, default!),
    new("udp6"u8, "::"u8, "udp4"u8, "0.0.0.0"u8, default!),
    new("udp"u8, "127.0.0.1"u8, "udp"u8, "::1"u8, default!),
    new("udp"u8, "::1"u8, "udp"u8, "127.0.0.1"u8, default!),
    new("udp4"u8, "127.0.0.1"u8, "udp6"u8, "::1"u8, default!),
    new("udp6"u8, "::1"u8, "udp4"u8, "127.0.0.1"u8, default!)
}.slice();

// TestDualStackUDPListener tests both single and double listen
// to a test listener with various address families, different
// listening address and same port.
//
// On DragonFly BSD, we expect the kernel version of node under test
// to be greater than or equal to 4.4.
public static void TestDualStackUDPListener(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    if (!supportsIPv4() || !supportsIPv6()) {
        Ꮡt.Skip(bothIPv4AndIPv6Areˢ);
    }
    foreach (var (_, vᴛ1) in dualStackUDPListenerTests) {
        var tt = vᴛ1;

        if (!testableListenArgs(tt.network1, JoinHostPort(tt.address1, "0"u8), ""u8)) {
            Ꮡt.Logf("skipping %s test"u8, tt.network1 + " " + tt.address1);
            continue;
        }
        if (!supportsIPv4map() && differentWildcardAddr(tt.address1, tt.address2)) {
            tt.xerr = default!;
        }
        error firstErr = default!;
        error secondErr = default!;
        for (nint i = 0; i < 5; i++) {
            var (cs, err) = newDualStackPacketListener();
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            @string port = cs[0].port();
            foreach (var (_, c) in cs) {
                c.of(global::go.net_package.UDPConn.Ꮡconn).Close();
            }
            global::go.net_package.PacketConn c1 = default!;
            (c1, firstErr) = ListenPacket(tt.network1, JoinHostPort(tt.address1, port));
            if (firstErr != default!) {
                continue;
            }
            {
                var errΔ1 = checkFirstListener(tt.network1, c1); if (errΔ1 != default!) {
                    c1.Close();
                    Ꮡt.Fatal(errΔ1);
                }
            }
            (var c2, err) = ListenPacket(tt.network2, JoinHostPort(tt.address2, c1._<ж<global::go.net_package.UDPConn>>().port()));
            if (err == default!) {
                c2.Close();
            }
            {
                secondErr = checkDualStackSecondListener(tt.network2, tt.address2, err, tt.xerr); if (secondErr != default!) {
                    c1.Close();
                    continue;
                }
            }
            c1.Close();
            break;
        }
        if (firstErr != default!) {
            Ꮡt.Error(firstErr);
        }
        if (secondErr != default!) {
            Ꮡt.Error(secondErr);
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
    if (exprᴛ1 == "tcp"u8) {
        var fd = ln._<ж<global::go.net_package.TCPListener>>().Value.fd;
        {
            var err = checkDualStackAddrFamily(fd); if (err != default!) {
                return err;
            }
        }
    }
    else if (exprᴛ1 == "tcp4"u8) {
        var fd = ln._<ж<global::go.net_package.TCPListener>>().Value.fd;
        if ((~fd).family != syscall.AF_INET) {
            return fmt.Errorf("%v got %v; want %v"u8, (~fd).laddr, (~fd).family, (nint)(syscall.AF_INET));
        }
    }
    else if (exprᴛ1 == "tcp6"u8) {
        var fd = ln._<ж<global::go.net_package.TCPListener>>().Value.fd;
        if ((~fd).family != syscall.AF_INET6) {
            return fmt.Errorf("%v got %v; want %v"u8, (~fd).laddr, (~fd).family, (nint)(syscall.AF_INET6));
        }
    }
    else if (exprᴛ1 == "udp"u8) {
        var fd = ln._<ж<global::go.net_package.UDPConn>>().Value.fd;
        {
            var err = checkDualStackAddrFamily(fd); if (err != default!) {
                return err;
            }
        }
    }
    else if (exprᴛ1 == "udp4"u8) {
        var fd = ln._<ж<global::go.net_package.UDPConn>>().Value.fd;
        if ((~fd).family != syscall.AF_INET) {
            return fmt.Errorf("%v got %v; want %v"u8, (~fd).laddr, (~fd).family, (nint)(syscall.AF_INET));
        }
    }
    else if (exprᴛ1 == "udp6"u8) {
        var fd = ln._<ж<global::go.net_package.UDPConn>>().Value.fd;
        if ((~fd).family != syscall.AF_INET6) {
            return fmt.Errorf("%v got %v; want %v"u8, (~fd).laddr, (~fd).family, (nint)(syscall.AF_INET6));
        }
    }
    else { /* default: */
        return new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)network));
    }

    return default!;
}

internal static error checkSecondListener(@string network, @string address, error err) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
        if (err == default!) {
            return fmt.Errorf("%s should fail"u8, network + " " + address);
        }
    }
    else if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
        if (err == default!) {
            return fmt.Errorf("%s should fail"u8, network + " " + address);
        }
    }
    else { /* default: */
        return new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)network));
    }

    return default!;
}

internal static error checkDualStackSecondListener(@string network, @string address, error err, error xerr) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
        if (xerr == default! && err != default! || xerr != default! && err == default!) {
            return fmt.Errorf("%s got %v; want %v"u8, network + " " + address, err, xerr);
        }
    }
    else if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
        if (xerr == default! && err != default! || xerr != default! && err == default!) {
            return fmt.Errorf("%s got %v; want %v"u8, network + " " + address, err, xerr);
        }
    }
    else { /* default: */
        return new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)network));
    }

    return default!;
}

internal static error checkDualStackAddrFamily(ж<global::go.net_package.netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    switch (fd.laddr.type()) {
    case ж<global::go.net_package.TCPAddr> a: {
        if (supportsIPv4map() && fd.laddr._<ж<global::go.net_package.TCPAddr>>().isWildcard()){
            // If a node under test supports both IPv6 capability
            // and IPv6 IPv4-mapping capability, we can assume
            // that the node listens on a wildcard address with an
            // AF_INET6 socket.
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
        if (supportsIPv4map() && fd.laddr._<ж<global::go.net_package.UDPAddr>>().isWildcard()){
            // If a node under test supports both IPv6 capability
            // and IPv6 IPv4-mapping capability, we can assume
            // that the node listens on a wildcard address with an
            // AF_INET6 socket.
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
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ipIcmpˢ = "ip:icmp"u8;

public static void TestWildWildcardListener(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        defer(() => {
            {
                var p = recover(); if (p != default!) {
                    Ꮡt.Fatalf("panicked: %v"u8, p);
                }
            }
        }, ref ᒐ);
        {
            var (ln, err) = Listen(tcpˢ, ""u8); if (err == default!) {
                ln.Close();
            }
        }
        {
            var (ln, err) = ListenPacket(udpˢ, ""u8); if (err == default!) {
                ln.Close();
            }
        }
        {
            var (ln, err) = ListenTCP(tcpˢ, nil); if (err == default!) {
                ln.Close();
            }
        }
        {
            var (ln, err) = ListenUDP(udpˢ, nil); if (err == default!) {
                ln.of(global::go.net_package.UDPConn.Ꮡconn).Close();
            }
        }
        {
            var (ln, err) = ListenIP(ipIcmpˢ, nil); if (err == default!) {
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
internal static void initᴛipv4MulticastListenerTests() { ipv4MulticastListenerTests = new ipv4MulticastListenerTestsᴛ1[]{
    new("udp"u8, Ꮡ(new UDPAddr(IP: IPv4(224, 0, 0, 254), Port: 12345))),
    new("udp4"u8, Ꮡ(new UDPAddr(IP: IPv4(224, 0, 0, 254), Port: 12345)))
}.slice(); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object iPv4IsNotSupportedˢ = (@string)"IPv4 is not supported"u8;

// TestIPv4MulticastListener tests both single and double listen to a
// test listener with same address family, same group address and same
// port.
public static void TestIPv4MulticastListener(ж<testing.T> Ꮡt) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "android"u8 || exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    if (!supportsIPv4()) {
        Ꮡt.Skip(iPv4IsNotSupportedˢ);
    }
    void closer(slice<ж<global::go.net_package.UDPConn>> cs) {
        foreach (var (_, c) in cs) {
            if (c != nil) {
                c.of(global::go.net_package.UDPConn.Ꮡconn).Close();
            }
        }
    }
    foreach (var (_, ifi) in new ж<global::go.net_package.Interface>[]{loopbackInterface(), default!}.slice()) {
        // Note that multicast interface assignment by system
        // is not recommended because it usually relies on
        // routing stuff for finding out an appropriate
        // nexthop containing both network and link layer
        // adjacencies.
        if (ifi == nil || !testIPv4.Value) {
            continue;
        }
        foreach (var (_, tt) in ipv4MulticastListenerTests) {
            error err = default!;
            var cs = new slice<ж<global::go.net_package.UDPConn>>(2);
            {
                (cs[0], err) = ListenMulticastUDP(tt.net, ifi, tt.gaddr); if (err != default!) {
                    Ꮡt.Fatal(err);
                }
            }
            {
                var errΔ1 = checkMulticastListener(cs[0], (~tt.gaddr).IP); if (errΔ1 != default!) {
                    closer(cs);
                    Ꮡt.Fatal(errΔ1);
                }
            }
            {
                (cs[1], err) = ListenMulticastUDP(tt.net, ifi, tt.gaddr); if (err != default!) {
                    closer(cs);
                    Ꮡt.Fatal(err);
                }
            }
            {
                var errΔ2 = checkMulticastListener(cs[1], (~tt.gaddr).IP); if (errΔ2 != default!) {
                    closer(cs);
                    Ꮡt.Fatal(errΔ2);
                }
            }
            closer(cs);
        }
    }
}

internal static slice<ipv4MulticastListenerTestsᴛ1> ipv6MulticastListenerTests = new ipv4MulticastListenerTestsᴛ1[]{
    new("udp"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff01::114"u8), Port: 12345))),
    new("udp"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff02::114"u8), Port: 12345))),
    new("udp"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff04::114"u8), Port: 12345))),
    new("udp"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff05::114"u8), Port: 12345))),
    new("udp"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff08::114"u8), Port: 12345))),
    new("udp"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff0e::114"u8), Port: 12345))),
    new("udp6"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff01::114"u8), Port: 12345))),
    new("udp6"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff02::114"u8), Port: 12345))),
    new("udp6"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff04::114"u8), Port: 12345))),
    new("udp6"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff05::114"u8), Port: 12345))),
    new("udp6"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff08::114"u8), Port: 12345))),
    new("udp6"u8, Ꮡ(new UDPAddr(IP: ParseIP("ff0e::114"u8), Port: 12345)))
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object iPv6IsNotSupportedˢ = (@string)"IPv6 is not supported"u8;
internal static readonly object mustBeRootˢ = (@string)"must be root"u8;

// TestIPv6MulticastListener tests both single and double listen to a
// test listener with same address family, same group address and same
// port.
public static void TestIPv6MulticastListener(ж<testing.T> Ꮡt) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    if (!supportsIPv6()) {
        Ꮡt.Skip(iPv6IsNotSupportedˢ);
    }
    if (Δos.Getuid() != 0) {
        Ꮡt.Skip(mustBeRootˢ);
    }
    void closer(slice<ж<global::go.net_package.UDPConn>> cs) {
        foreach (var (_, c) in cs) {
            if (c != nil) {
                c.of(global::go.net_package.UDPConn.Ꮡconn).Close();
            }
        }
    }
    foreach (var (_, ifi) in new ж<global::go.net_package.Interface>[]{loopbackInterface(), default!}.slice()) {
        // Note that multicast interface assignment by system
        // is not recommended because it usually relies on
        // routing stuff for finding out an appropriate
        // nexthop containing both network and link layer
        // adjacencies.
        if (ifi == nil && !testIPv6.Value) {
            continue;
        }
        foreach (var (_, tt) in ipv6MulticastListenerTests) {
            error err = default!;
            var cs = new slice<ж<global::go.net_package.UDPConn>>(2);
            {
                (cs[0], err) = ListenMulticastUDP(tt.net, ifi, tt.gaddr); if (err != default!) {
                    Ꮡt.Fatal(err);
                }
            }
            {
                var errΔ1 = checkMulticastListener(cs[0], (~tt.gaddr).IP); if (errΔ1 != default!) {
                    closer(cs);
                    Ꮡt.Fatal(errΔ1);
                }
            }
            {
                (cs[1], err) = ListenMulticastUDP(tt.net, ifi, tt.gaddr); if (err != default!) {
                    closer(cs);
                    Ꮡt.Fatal(err);
                }
            }
            {
                var errΔ2 = checkMulticastListener(cs[1], (~tt.gaddr).IP); if (errΔ2 != default!) {
                    closer(cs);
                    Ꮡt.Fatal(errΔ2);
                }
            }
            closer(cs);
        }
    }
}

internal static error checkMulticastListener(ж<global::go.net_package.UDPConn> Ꮡc, global::go.net_package.IP ip) {
    {
        var (ok, err) = multicastRIBContains(ip); if (err != default!){
            return err;
        } else 
        if (!ok) {
            return fmt.Errorf("%s not found in multicast rib"u8, ip.String());
        }
    }
    var la = Ꮡc.of(global::go.net_package.UDPConn.Ꮡconn).LocalAddr();
    {
        var (laΔ1, ok) = la._<ж<global::go.net_package.UDPAddr>>(ᐧ); if (!ok || (~laΔ1).Port == 0) {
            return fmt.Errorf("got %v; want a proper address with non-zero port number"u8, laΔ1.OrTypedNil());
        }
    }
    return default!;
}

internal static (bool, error) multicastRIBContains(global::go.net_package.IP ip) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "aix"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "plan9"u8 || exprᴛ1 == "solaris"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "windows"u8) {
        return (true, default!); // not implemented yet
    }
    if (exprᴛ1 == "linux"u8) {
        if (Δruntime.GOARCH == "arm"u8 || Δruntime.GOARCH == "alpha"u8) {
            return (true, default!); // not implemented yet
        }
    }

    var (ift, err) = Interfaces();
    if (err != default!) {
        return (false, err);
    }
    foreach (var (_, vᴛ1) in ift) {
        ref var ifi = ref heap(new global::go.net_package.Interface(), out var Ꮡifi);
        ifi = vᴛ1;

        var (ifmat, errΔ1) = Ꮡifi.MulticastAddrs();
        if (errΔ1 != default!) {
            return (false, errΔ1);
        }
        foreach (var (_, ifma) in ifmat) {
            if ((~ifma._<ж<global::go.net_package.IPAddr>>()).IP.Equal(ip)) {
                return (true, default!);
            }
        }
    }
    return (false, default!);
}

// Issue 21856.
public static partial void TestClosingListener(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
    var addr = ln.Addr();
    var lnʗ1 = ln;
    goǃ(() => {
        while (ᐧ) {
            var (c, errΔ1) = lnʗ1.Accept();
            if (errΔ1 != default!) {
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
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ln2.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string streamListenˢ = "StreamListen"u8;
internal static readonly @string packetListenˢ = "PacketListen"u8;

public static void TestListenConfigControl(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    Ꮡt.Run(streamListenˢ, (ж<testing.T> tΔ1) => {
        foreach (var (_, network) in new @string[]{"tcp"u8, "tcp4"u8, "tcp6"u8, "unix"u8, "unixpacket"u8}.slice()) {
            if (!testableNetwork(network)) {
                continue;
            }
            var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), network, Ꮡ(new ListenConfig(Control: controlOnConnSetup)));
            ln.Close();
        }
    });
    Ꮡt.Run(packetListenˢ, (ж<testing.T> tΔ2) => {
        foreach (var (_, network) in new @string[]{"udp"u8, "udp4"u8, "udp6"u8, "unixgram"u8}.slice()) {
            if (!testableNetwork(network)) {
                continue;
            }
            var c = newLocalPacketListener(new net_test_package.testing_TжTB(tΔ2), network, Ꮡ(new ListenConfig(Control: controlOnConnSetup)));
            c.Close();
        }
    });
}

} // end net_internal_test_package
