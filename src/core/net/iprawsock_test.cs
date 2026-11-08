// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using reflect = reflect_package;
using testing = testing_package;
using @internal;
using context = context_package;
using static go.net_package;

partial class net_internal_test_package {

// The full stack test cases for IPConn have been moved to the
// following:
//	golang.org/x/net/ipv4
//	golang.org/x/net/ipv6
//	golang.org/x/net/icmp
internal partial struct resolveIPAddrTest {
    internal @string network;
    internal @string litAddrOrName;
    internal ж<global::go.net_package.IPAddr> addr;
    internal error err;
}

internal static slice<resolveIPAddrTest> resolveIPAddrTests;
internal static void initᴛresolveIPAddrTests() { resolveIPAddrTests = slice<resolveIPAddrTest>([
    new("ip", "127.0.0.1", Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), default),
    new("ip4", "127.0.0.1", Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), default),
    new("ip4:icmp", "127.0.0.1", Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), default),

    new("ip", "::1", Ꮡ(new IPAddr(IP: ParseIP("::1"))), default),
    new("ip6", "::1", Ꮡ(new IPAddr(IP: ParseIP("::1"))), default),
    new("ip6:ipv6-icmp", "::1", Ꮡ(new IPAddr(IP: ParseIP("::1"))), default),
    new("ip6:IPv6-ICMP", "::1", Ꮡ(new IPAddr(IP: ParseIP("::1"))), default),

    new("ip", "::1%en0", Ꮡ(new IPAddr(IP: ParseIP("::1"), Zone: "en0")), default),
    new("ip6", "::1%911", Ꮡ(new IPAddr(IP: ParseIP("::1"), Zone: "911")), default),

    new("", "127.0.0.1", Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), default), // Go 1.0 behavior
    new("", "::1", Ꮡ(new IPAddr(IP: ParseIP("::1"))), default),           // Go 1.0 behavior

    new("ip4:icmp", "", Ꮡ(new IPAddr()), default),

    new("l2tp", "127.0.0.1", nil, new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)(@string)"l2tp"))),
    new("l2tp:gre", "127.0.0.1", nil, new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)(@string)"l2tp:gre"))),
    new("tcp", "1.2.3.4:123", nil, new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)(@string)"tcp"))),

    new("ip4", "2001:db8::1", nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "2001:db8::1")))),
    new("ip4:icmp", "2001:db8::1", nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "2001:db8::1")))),
    new("ip6", "127.0.0.1", nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "127.0.0.1")))),
    new("ip6", "::ffff:127.0.0.1", nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "::ffff:127.0.0.1")))),
    new("ip6:ipv6-icmp", "127.0.0.1", nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "127.0.0.1")))),
    new("ip6:ipv6-icmp", "::ffff:127.0.0.1", nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "::ffff:127.0.0.1"))))
]); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ipNoprivˢ => ref ipNoprivˢᵇ;
private static readonly @string ipNoprivˢᵇ = "ip+nopriv"u8;
internal static readonly object ipNoprivTestˢ = (@string)"ip+nopriv test"u8;

public static void TestResolveIPAddr(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(ipNoprivˢ)) {
            t.Skip(ipNoprivTestˢ);
        }

        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = lookupLocalhost;

        foreach (var (_, tt) in range(resolveIPAddrTests)) {
            var (addr, err) = ResolveIPAddr(tt.network, tt.litAddrOrName);
            if (!reflect.DeepEqual(addr.OrTypedNil(), tt.addr.OrTypedNil()) || !reflect.DeepEqual(err, tt.err)) {
                t.Errorf("ResolveIPAddr(%q, %q) = %#v, %v, want %#v, %v", tt.network, tt.litAddrOrName, addr.OrTypedNil(), err, tt.addr.OrTypedNil(), tt.err);
                continue;
            }
            if (err == nil) {
                var (addr2, errΔ1) = ResolveIPAddr(addr.Network(), addr.String());
                if (!reflect.DeepEqual(addr2.OrTypedNil(), tt.addr.OrTypedNil()) || !AreEqual(errΔ1, tt.err)) {
                    t.Errorf("(%q, %q): ResolveIPAddr(%q, %q) = %#v, %v, want %#v, %v", tt.network, tt.litAddrOrName, addr.Network(), addr.String(), addr2.OrTypedNil(), errΔ1, tt.addr.OrTypedNil(), tt.err);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct ipConnLocalNameTestsᴛ1 /*dyn*/ {
    internal @string net;
    internal ж<global::go.net_package.IPAddr> laddr;
}
internal static slice<ipConnLocalNameTestsᴛ1> ipConnLocalNameTests;
internal static void initᴛipConnLocalNameTests() { ipConnLocalNameTests = slice<ipConnLocalNameTestsᴛ1>([
    new("ip4:icmp", Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1)))),
    new("ip4:icmp", Ꮡ(new IPAddr())),
    new("ip4:icmp", nil)
]); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shouldNotFailˢ = (@string)"should not fail"u8;

public static void TestIPConnLocalName(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, tt) in range(ipConnLocalNameTests)) {
            if (!testableNetwork(tt.net)) {
                t.Logf("skipping %s test", tt.net);
                continue;
            }
            var (c, err) = ListenIP(tt.net, tt.laddr);
            if (testenv.SyscallIsNotSupported(err)) {
                // May be inside a container that disallows creating a socket.
                t.Logf("skipping %s test: %v", tt.net, err);
                continue;
            } else if (err != nil) {
                t.Fatal(err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.of(global::go.net_package.IPConn.Ꮡconn).Close(), ref ᒐ);
            var la = c.of(global::go.net_package.IPConn.Ꮡconn).LocalAddr(); if (la == nil) {
                t.Fatal(shouldNotFailˢ);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ipTcpˢ => ref ipTcpˢᵇ;
private static readonly @string ipTcpˢᵇ = "ip:tcp"u8;

public static void TestIPConnRemoteName(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string network = ipTcpˢ;
        if (!testableNetwork(network)) {
            t.Skipf("skipping %s test", network);
        }

        var raddr = Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1).To4()));
        var (c, err) = DialIP(network, Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), raddr);
        if (testenv.SyscallIsNotSupported(err)) {
            // May be inside a container that disallows creating a socket.
            t.Skipf("skipping %s test: %v", network, err);
        } else if (err != nil) {
            t.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.of(global::go.net_package.IPConn.Ꮡconn).Close(), ref ᒐ);
        if (!reflect.DeepEqual(raddr.OrTypedNil(), c.of(global::go.net_package.IPConn.Ꮡconn).RemoteAddr())) {
            t.Fatalf("got %#v; want %#v", c.of(global::go.net_package.IPConn.Ꮡconn).RemoteAddr(), raddr.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestDialListenIPArgs_test /*dyn*/ {
    internal /*[2]*/ slice<array<@string>> argLists;
    internal bool shouldFail;
}

internal partial struct TestDialListenIPArgs_type /*dyn*/ {
    internal @string network, address;
    internal array<@string> args = new(2);
}

public static void TestDialListenIPArgs(ж<testing.T> t) {
    var testsᴷ = new stackAppend<TestDialListenIPArgs_test>();

    var tests = slice<TestDialListenIPArgs_test>([
        new(
            argLists: GoReflect.WithElemDims(new array<@string>[]{
                new @string[]{"ip", "127.0.0.1"}.array(),
                new @string[]{"ip:", "127.0.0.1"}.array(),
                new @string[]{"ip::", "127.0.0.1"}.array(),
                new @string[]{"ip", "::1"}.array(),
                new @string[]{"ip:", "::1"}.array(),
                new @string[]{"ip::", "::1"}.array(),
                new @string[]{"ip4", "127.0.0.1"}.array(),
                new @string[]{"ip4:", "127.0.0.1"}.array(),
                new @string[]{"ip4::", "127.0.0.1"}.array(),
                new @string[]{"ip6", "::1"}.array(),
                new @string[]{"ip6:", "::1"}.array(),
                new @string[]{"ip6::", "::1"}.array()
            }.slice(), 2),
            shouldFail: true
        )
    ]);
    if (testableNetwork("ip")) {
        var priv = new TestDialListenIPArgs_test(shouldFail: false);
        foreach (var (_, vᴛ1) in range(slice<TestDialListenIPArgs_type>([
            new("ip4:47", "127.0.0.1", new @string[]{"ip4:47", "127.0.0.1"}.array()),
            new("ip6:47", "::1", new @string[]{"ip6:47", "::1"}.array())
        ]))) {
            var tt = vᴛ1.ΔClone();

            var (c, err) = ListenPacket(tt.network, tt.address);
            if (err != nil) {
                continue;
            }
            c.Close();
            priv.argLists = append(priv.argLists, tt.args.Clone());
        }
        if (len(priv.argLists) > 0) {
            tests = append(testsᴷ, tests, priv);
        }
    }

    foreach (var (_, tt) in range(tests)) {
        foreach (var (_, vᴛ2) in range(tt.argLists)) {
            var args = vᴛ2.Clone();

            var (_, err) = Dial(args[0], args[1]);
            if (tt.shouldFail != (err != nil)) {
                t.Errorf("Dial(%q, %q) = %v; want (err != nil) is %t", args[0], args[1], err, tt.shouldFail);
            }
            (_, err) = ListenPacket(args[0], args[1]);
            if (tt.shouldFail != (err != nil)) {
                t.Errorf("ListenPacket(%q, %q) = %v; want (err != nil) is %t", args[0], args[1], err, tt.shouldFail);
            }
            (var a, err) = ResolveIPAddr("ip", args[1]);
            if (err != nil) {
                t.Errorf("ResolveIPAddr(\"ip\", %q) = %v", args[1], err);
                continue;
            }
            (_, err) = DialIP(args[0], nil, a);
            if (tt.shouldFail != (err != nil)) {
                t.Errorf("DialIP(%q, %v) = %v; want (err != nil) is %t", args[0], a.OrTypedNil(), err, tt.shouldFail);
            }
            (_, err) = ListenIP(args[0], a);
            if (tt.shouldFail != (err != nil)) {
                t.Errorf("ListenIP(%q, %v) = %v; want (err != nil) is %t", args[0], a.OrTypedNil(), err, tt.shouldFail);
            }
        }
    }
}

} // end net_internal_test_package
