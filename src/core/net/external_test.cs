// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using static go.net_package;

partial class net_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string wwwGoogleComHttpˢ => ref wwwGoogleComHttpˢᵇ;
private static readonly @string wwwGoogleComHttpˢᵇ = "www.google.com:http"u8;

public static void TestResolveGoogle(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

    if (!supportsIPv4() || !supportsIPv6() || !testIPv4.Value || !testIPv6.Value) {
        t.Skip(bothIPv4AndIPv6Areˢ);
    }

    foreach (var (_, network) in range(slice<@string>(["tcp", "tcp4", "tcp6"]))) {
        var (addr, err) = ResolveTCPAddr(network, wwwGoogleComHttpˢ);
        if (err != nil) {
            t.Error(err);
            continue;
        }
        var matchᴛ1 = false;
        if (network == "tcp"u8 && addr.IP.To4() == nil) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (network == "tcp4"u8 && addr.IP.To4() == nil)) {
            t.Errorf("got %v; want an IPv4 address on %s", addr.OrTypedNil(), network);
        }
        else if (network == "tcp6"u8 && (addr.IP.To16() == nil || addr.IP.To4() != nil)) { matchᴛ1 = true;
            t.Errorf("got %v; want an IPv6 address on %s", addr.OrTypedNil(), network);
        }
    }
}

partial struct dialGoogleTestsᴛ1 /*dyn*/ {
    internal Func<@string, @string, (global::go.net_package.Conn, error)> dial;
    internal @string unreachableNetwork;
    internal slice<@string> networks;
    internal slice<@string> addrs;
}
internal static slice<dialGoogleTestsᴛ1> dialGoogleTests;
internal static void initᴛdialGoogleTests() { dialGoogleTests = [
    new(
        dial: (Ꮡ(new Dialer(DualStack: true))).Dial,
        networks: ["tcp", "tcp4", "tcp6"],
        addrs: ["www.google.com:http"]
    ),
    new(
        dial: Dial,
        unreachableNetwork: "tcp6",
        networks: ["tcp", "tcp4"]
    ),
    new(
        dial: Dial,
        unreachableNetwork: "tcp4",
        networks: ["tcp", "tcp6"]
    )
]; }

public static void TestDialGoogle(ж<testing.T> t) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

    if (!supportsIPv4() || !supportsIPv6() || !testIPv4.Value || !testIPv6.Value) {
        t.Skip(bothIPv4AndIPv6Areˢ);
    }

    error err = default;
    (dialGoogleTests[1].addrs, dialGoogleTests[2].addrs, err) = googleLiteralAddrs();
    if (err != nil) {
        t.Error(err);
    }
    foreach (var (_, tt) in range(dialGoogleTests)) {
        foreach (var (_, network) in range(tt.networks)) {
            disableSocketConnect(tt.unreachableNetwork);
            foreach (var (_, addr) in range(tt.addrs)) {
                var errΔ1 = fetchGoogle(tt.dial, network, addr); if (errΔ1 != nil) {
                    t.Error(errΔ1);
                }
            }
            enableSocketConnect();
        }
    }
}

internal static array<@string> literalAddrs4 = new @string[]{
    "%d.%d.%d.%d:80",
    "www.google.com:80",
    "%d.%d.%d.%d:http",
    "www.google.com:http",
    "[::ffff:%d.%d.%d.%d]:80",
    "[::ffff:%02x%02x:%02x%02x]:80",
    "[0:0:0:0:0000:ffff:%d.%d.%d.%d]:80",
    "[0:0:0:0:000000:ffff:%d.%d.%d.%d]:80",
    "[0:0:0:0::ffff:%d.%d.%d.%d]:80"
}.array();
internal static array<@string> literalAddrs6 = new @string[]{
    "[%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x]:80",
    "ipv6.google.com:80",
    "[%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x]:http",
    "ipv6.google.com:http"
}.array();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string wwwGoogleComˢ => ref wwwGoogleComˢᵇ;
private static readonly @string wwwGoogleComˢᵇ = "www.google.com"u8;

internal static (slice<@string> lits4, slice<@string> lits6, error err) googleLiteralAddrs() {
    slice<@string> lits4 = default;
    slice<@string> lits6 = default;
    error err = default;

    (var ips, err) = LookupIP(wwwGoogleComˢ);
    if (err != nil) {
        return (default, default, err);
    }
    if (len(ips) == 0) {
        return (default, default, default);
    }
    global::go.net_package.IP ip4 = default;
    global::go.net_package.IP ip6 = default;
    foreach (var (_, ip) in range(ips)) {
        if (ip4 == nil && ip.To4() != nil) {
            ip4 = ip.To4();
        }
        if (ip6 == nil && ip.To16() != nil && ip.To4() == nil) {
            ip6 = ip.To16();
        }
        if (ip4 != nil && ip6 != nil) {
            break;
        }
    }
    if (ip4 != nil) {
        foreach (var (i, lit4) in range(literalAddrs4.ΔRangeSnapshot())) {
            if (strings.Contains(lit4, "%")) {
                literalAddrs4[i] = fmt.Sprintf(lit4, ip4[0], ip4[1], ip4[2], ip4[3]);
            }
        }
        lits4 = literalAddrs4[..];
    }
    if (ip6 != nil) {
        foreach (var (i, lit6) in range(literalAddrs6.ΔRangeSnapshot())) {
            if (strings.Contains(lit6, "%")) {
                literalAddrs6[i] = fmt.Sprintf(lit6, ip6[0], ip6[1], ip6[2], ip6[3], ip6[4], ip6[5], ip6[6], ip6[7], ip6[8], ip6[9], ip6[10], ip6[11], ip6[12], ip6[13], ip6[14], ip6[15]);
            }
        }
        lits6 = literalAddrs6[..];
    }
    return (lits4, lits6, err);
}

internal static error fetchGoogle(Func<@string, @string, (global::go.net_package.Conn, error)> dial, @string network, @string address) {
    GoFrame ᒐ = default;
    try {
        var (c, err) = dial(network, address);
        if (err != nil) {
            return err;
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var req = slice<byte>("GET /robots.txt HTTP/1.0\r\nHost: www.google.com\r\n\r\n"u8);
        var (_, errΔ1) = c.Write(req); if (errΔ1 != nil) {
            return errΔ1;
        }
        var b = new slice<byte>(1000);
        (var n, err) = Δio.ReadFull(new net_test_package.net_ConnᴠReader(c), b);
        if (err != nil) {
            return err;
        }
        if (n < 1000) {
            return fmt.Errorf("short read from %s:%s->%s"u8, network, c.RemoteAddr(), c.LocalAddr());
        }
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
