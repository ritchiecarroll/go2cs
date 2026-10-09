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

// Go 1.0 behavior
// Go 1.0 behavior
internal static slice<resolveIPAddrTest> resolveIPAddrTests;
internal static void initᴛresolveIPAddrTests() { resolveIPAddrTests = new resolveIPAddrTest[]{
    new("ip"u8, "127.0.0.1"u8, Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), default!),
    new("ip4"u8, "127.0.0.1"u8, Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), default!),
    new("ip4:icmp"u8, "127.0.0.1"u8, Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), default!),
    new("ip"u8, "::1"u8, Ꮡ(new IPAddr(IP: ParseIP("::1"u8))), default!),
    new("ip6"u8, "::1"u8, Ꮡ(new IPAddr(IP: ParseIP("::1"u8))), default!),
    new("ip6:ipv6-icmp"u8, "::1"u8, Ꮡ(new IPAddr(IP: ParseIP("::1"u8))), default!),
    new("ip6:IPv6-ICMP"u8, "::1"u8, Ꮡ(new IPAddr(IP: ParseIP("::1"u8))), default!),
    new("ip"u8, "::1%en0"u8, Ꮡ(new IPAddr(IP: ParseIP("::1"u8), Zone: "en0"u8)), default!),
    new("ip6"u8, "::1%911"u8, Ꮡ(new IPAddr(IP: ParseIP("::1"u8), Zone: "911"u8)), default!),
    new(""u8, "127.0.0.1"u8, Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), default!),
    new(""u8, "::1"u8, Ꮡ(new IPAddr(IP: ParseIP("::1"u8))), default!),
    new("ip4:icmp"u8, ""u8, Ꮡ(new IPAddr(nil)), default!),
    new("l2tp"u8, "127.0.0.1"u8, nil, new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)(@string)"l2tp"u8))),
    new("l2tp:gre"u8, "127.0.0.1"u8, nil, new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)(@string)"l2tp:gre"u8))),
    new("tcp"u8, "1.2.3.4:123"u8, nil, new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)(@string)"tcp"u8))),
    new("ip4"u8, "2001:db8::1"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "2001:db8::1"u8)))),
    new("ip4:icmp"u8, "2001:db8::1"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "2001:db8::1"u8)))),
    new("ip6"u8, "127.0.0.1"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "127.0.0.1"u8)))),
    new("ip6"u8, "::ffff:127.0.0.1"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "::ffff:127.0.0.1"u8)))),
    new("ip6:ipv6-icmp"u8, "127.0.0.1"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "127.0.0.1"u8)))),
    new("ip6:ipv6-icmp"u8, "::ffff:127.0.0.1"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "::ffff:127.0.0.1"u8))))
}.slice(); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ipNoprivˢ = "ip+nopriv"u8;
internal static readonly object ipNoprivTestˢ = (@string)"ip+nopriv test"u8;

public static void TestResolveIPAddr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(ipNoprivˢ)) {
            Ꮡt.Skip(ipNoprivTestˢ);
        }
        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = lookupLocalhost;
        foreach (var (_, tt) in resolveIPAddrTests) {
            var (addr, err) = ResolveIPAddr(tt.network, tt.litAddrOrName);
            if (!reflect.DeepEqual(addr.OrTypedNil(), tt.addr.OrTypedNil()) || !reflect.DeepEqual(err, tt.err)) {
                Ꮡt.Errorf("ResolveIPAddr(%q, %q) = %#v, %v, want %#v, %v"u8, tt.network, tt.litAddrOrName, addr.OrTypedNil(), err, tt.addr.OrTypedNil(), tt.err);
                continue;
            }
            if (err == default!) {
                var (addr2, errΔ1) = ResolveIPAddr(addr.Network(), addr.String());
                if (!reflect.DeepEqual(addr2.OrTypedNil(), tt.addr.OrTypedNil()) || !AreEqual(errΔ1, tt.err)) {
                    Ꮡt.Errorf("(%q, %q): ResolveIPAddr(%q, %q) = %#v, %v, want %#v, %v"u8, tt.network, tt.litAddrOrName, addr.Network(), addr.String(), addr2.OrTypedNil(), errΔ1, tt.addr.OrTypedNil(), tt.err);
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
internal static void initᴛipConnLocalNameTests() { ipConnLocalNameTests = new ipConnLocalNameTestsᴛ1[]{
    new("ip4:icmp"u8, Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1)))),
    new("ip4:icmp"u8, Ꮡ(new IPAddr(nil))),
    new("ip4:icmp"u8, nil)
}.slice(); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shouldNotFailˢ = (@string)"should not fail"u8;

public static void TestIPConnLocalName(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, tt) in ipConnLocalNameTests) {
            if (!testableNetwork(tt.net)) {
                Ꮡt.Logf("skipping %s test"u8, tt.net);
                continue;
            }
            var (c, err) = ListenIP(tt.net, tt.laddr);
            if (testenv.SyscallIsNotSupported(err)){
                // May be inside a container that disallows creating a socket.
                Ꮡt.Logf("skipping %s test: %v"u8, tt.net, err);
                continue;
            } else 
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.of(global::go.net_package.IPConn.Ꮡconn).Close(), ref ᒐ);
            {
                var la = c.of(global::go.net_package.IPConn.Ꮡconn).LocalAddr(); if (la == default!) {
                    Ꮡt.Fatal(shouldNotFailˢ);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ipTcpˢ = "ip:tcp"u8;

public static void TestIPConnRemoteName(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string network = ipTcpˢ;
        if (!testableNetwork(network)) {
            Ꮡt.Skipf("skipping %s test"u8, network);
        }
        var raddr = Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1).To4()));
        var (c, err) = DialIP(network, Ꮡ(new IPAddr(IP: IPv4(127, 0, 0, 1))), raddr);
        if (testenv.SyscallIsNotSupported(err)){
            // May be inside a container that disallows creating a socket.
            Ꮡt.Skipf("skipping %s test: %v"u8, network, err);
        } else 
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.of(global::go.net_package.IPConn.Ꮡconn).Close(), ref ᒐ);
        if (!reflect.DeepEqual(raddr.OrTypedNil(), c.of(global::go.net_package.IPConn.Ꮡconn).RemoteAddr())) {
            Ꮡt.Fatalf("got %#v; want %#v"u8, c.of(global::go.net_package.IPConn.Ꮡconn).RemoteAddr(), raddr.OrTypedNil());
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

public static void TestDialListenIPArgs(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestDialListenIPArgs_test[]{
        new(
            argLists: GoReflect.WithElemDims(new array<@string>[]{
                new @string[]{"ip"u8, "127.0.0.1"u8}.array(),
                new @string[]{"ip:"u8, "127.0.0.1"u8}.array(),
                new @string[]{"ip::"u8, "127.0.0.1"u8}.array(),
                new @string[]{"ip"u8, "::1"u8}.array(),
                new @string[]{"ip:"u8, "::1"u8}.array(),
                new @string[]{"ip::"u8, "::1"u8}.array(),
                new @string[]{"ip4"u8, "127.0.0.1"u8}.array(),
                new @string[]{"ip4:"u8, "127.0.0.1"u8}.array(),
                new @string[]{"ip4::"u8, "127.0.0.1"u8}.array(),
                new @string[]{"ip6"u8, "::1"u8}.array(),
                new @string[]{"ip6:"u8, "::1"u8}.array(),
                new @string[]{"ip6::"u8, "::1"u8}.array()
            }.slice(), 2),
            shouldFail: true
        )
    }.slice();
    if (testableNetwork("ip"u8)) {
        var priv = new TestDialListenIPArgs_test(shouldFail: false);
        foreach (var (_, vᴛ1) in new TestDialListenIPArgs_type[]{
            new("ip4:47"u8, "127.0.0.1"u8, new @string[]{"ip4:47"u8, "127.0.0.1"u8}.array()),
            new("ip6:47"u8, "::1"u8, new @string[]{"ip6:47"u8, "::1"u8}.array())
        }.slice()) {
            var tt = vᴛ1.ΔClone();

            var (c, err) = ListenPacket(tt.network, tt.address);
            if (err != default!) {
                continue;
            }
            c.Close();
            priv.argLists = append(priv.argLists, tt.args.Clone());
        }
        if (len(priv.argLists) > 0) {
            tests = append(tests, priv);
        }
    }
    foreach (var (_, tt) in tests) {
        foreach (var (_, vᴛ2) in tt.argLists) {
            var args = vᴛ2.Clone();

            var (_, err) = Dial(args[0], args[1]);
            if (tt.shouldFail != (err != default!)) {
                Ꮡt.Errorf("Dial(%q, %q) = %v; want (err != nil) is %t"u8, args[0], args[1], err, tt.shouldFail);
            }
            (_, err) = ListenPacket(args[0], args[1]);
            if (tt.shouldFail != (err != default!)) {
                Ꮡt.Errorf("ListenPacket(%q, %q) = %v; want (err != nil) is %t"u8, args[0], args[1], err, tt.shouldFail);
            }
            (var a, err) = ResolveIPAddr("ip"u8, args[1]);
            if (err != default!) {
                Ꮡt.Errorf("ResolveIPAddr(\"ip\", %q) = %v"u8, args[1], err);
                continue;
            }
            (_, err) = DialIP(args[0], nil, a);
            if (tt.shouldFail != (err != default!)) {
                Ꮡt.Errorf("DialIP(%q, %v) = %v; want (err != nil) is %t"u8, args[0], a.OrTypedNil(), err, tt.shouldFail);
            }
            (_, err) = ListenIP(args[0], a);
            if (tt.shouldFail != (err != default!)) {
                Ꮡt.Errorf("ListenIP(%q, %v) = %v; want (err != nil) is %t"u8, args[0], a.OrTypedNil(), err, tt.shouldFail);
            }
        }
    }
}

} // end net_internal_test_package
