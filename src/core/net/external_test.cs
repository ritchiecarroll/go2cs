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
internal static readonly @string wwwGoogleComHttpˢ = "www.google.com:http"u8;

public static void TestResolveGoogle(ж<testing.T> Ꮡt) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
    if (!supportsIPv4() || !supportsIPv6() || !testIPv4.Value || !testIPv6.Value) {
        Ꮡt.Skip(bothIPv4AndIPv6Areˢ);
    }
    foreach (var (_, network) in new @string[]{"tcp"u8, "tcp4"u8, "tcp6"u8}.slice()) {
        var (addr, err) = ResolveTCPAddr(network, wwwGoogleComHttpˢ);
        if (err != default!) {
            Ꮡt.Error(err);
            continue;
        }
        var matchᴛ1 = false;
        if (network == "tcp"u8 && (~addr).IP.To4() == default!) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (network == "tcp4"u8 && (~addr).IP.To4() == default!)) {
            Ꮡt.Errorf("got %v; want an IPv4 address on %s"u8, addr.OrTypedNil(), network);
        }
        else if (network == "tcp6"u8 && ((~addr).IP.To16() == default! || (~addr).IP.To4() != default!)) { matchᴛ1 = true;
            Ꮡt.Errorf("got %v; want an IPv6 address on %s"u8, addr.OrTypedNil(), network);
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
internal static void initᴛdialGoogleTests() { dialGoogleTests = new dialGoogleTestsᴛ1[]{
    new(
        dial: (Ꮡ(new Dialer(DualStack: true))).Dial,
        networks: new @string[]{"tcp"u8, "tcp4"u8, "tcp6"u8}.slice(),
        addrs: new @string[]{"www.google.com:http"u8}.slice()
    ),
    new(
        dial: Dial,
        unreachableNetwork: "tcp6"u8,
        networks: new @string[]{"tcp"u8, "tcp4"u8}.slice()
    ),
    new(
        dial: Dial,
        unreachableNetwork: "tcp4"u8,
        networks: new @string[]{"tcp"u8, "tcp6"u8}.slice()
    )
}.slice(); }

public static void TestDialGoogle(ж<testing.T> Ꮡt) {
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
    if (!supportsIPv4() || !supportsIPv6() || !testIPv4.Value || !testIPv6.Value) {
        Ꮡt.Skip(bothIPv4AndIPv6Areˢ);
    }
    error err = default!;
    (dialGoogleTests[1].addrs, dialGoogleTests[2].addrs, err) = googleLiteralAddrs();
    if (err != default!) {
        Ꮡt.Error(err);
    }
    foreach (var (_, tt) in dialGoogleTests) {
        foreach (var (_, network) in tt.networks) {
            disableSocketConnect(tt.unreachableNetwork);
            foreach (var (_, addr) in tt.addrs) {
                {
                    var errΔ1 = fetchGoogle(tt.dial, network, addr); if (errΔ1 != default!) {
                        Ꮡt.Error(errΔ1);
                    }
                }
            }
            enableSocketConnect();
        }
    }
}

internal static array<@string> literalAddrs4 = new @string[]{
    "%d.%d.%d.%d:80"u8,
    "www.google.com:80"u8,
    "%d.%d.%d.%d:http"u8,
    "www.google.com:http"u8,
    "%03d.%03d.%03d.%03d:0080"u8,
    "[::ffff:%d.%d.%d.%d]:80"u8,
    "[::ffff:%02x%02x:%02x%02x]:80"u8,
    "[0:0:0:0:0000:ffff:%d.%d.%d.%d]:80"u8,
    "[0:0:0:0:000000:ffff:%d.%d.%d.%d]:80"u8,
    "[0:0:0:0::ffff:%d.%d.%d.%d]:80"u8
}.array();
internal static array<@string> literalAddrs6 = new @string[]{
    "[%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x]:80"u8,
    "ipv6.google.com:80"u8,
    "[%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x]:http"u8,
    "ipv6.google.com:http"u8
}.array();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wwwGoogleComˢ = "www.google.com"u8;

internal static (slice<@string> lits4, slice<@string> lits6, error err) googleLiteralAddrs() {
    slice<@string> lits4 = default!;
    slice<@string> lits6 = default!;
    error err = default!;

    (var ips, err) = LookupIP(wwwGoogleComˢ);
    if (err != default!) {
        return (default!, default!, err);
    }
    if (len(ips) == 0) {
        return (default!, default!, default!);
    }
    global::go.net_package.IP ip4 = default!;
    global::go.net_package.IP ip6 = default!;
    foreach (var (_, ip) in ips) {
        if (ip4 == default! && ip.To4() != default!) {
            ip4 = ip.To4();
        }
        if (ip6 == default! && ip.To16() != default! && ip.To4() == default!) {
            ip6 = ip.To16();
        }
        if (ip4 != default! && ip6 != default!) {
            break;
        }
    }
    if (ip4 != default!) {
        foreach (var (i, lit4) in literalAddrs4.ΔRangeSnapshot()) {
            if (strings.Contains(lit4, "%"u8)) {
                literalAddrs4[i] = fmt.Sprintf(lit4, ip4[0], ip4[1], ip4[2], ip4[3]);
            }
        }
        lits4 = literalAddrs4[..];
    }
    if (ip6 != default!) {
        foreach (var (i, lit6) in literalAddrs6.ΔRangeSnapshot()) {
            if (strings.Contains(lit6, "%"u8)) {
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
        if (err != default!) {
            return err;
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        var req = slice<byte>("GET /robots.txt HTTP/1.0\r\nHost: www.google.com\r\n\r\n"u8);
        {
            var (_, errΔ1) = c.Write(req); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        var b = new slice<byte>(1000);
        (var n, err) = Δio.ReadFull(new net_test_package.net_ConnᴠReader(c), b);
        if (err != default!) {
            return err;
        }
        if (n < 1000) {
            return fmt.Errorf("short read from %s:%s->%s"u8, network, c.RemoteAddr(), c.LocalAddr());
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
