// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using reflect = reflect_package;
using testing = testing_package;
using static go.net_package;

partial class net_internal_test_package {

internal static Func<global::go.net_package.IPAddr, global::go.net_package.ΔAddr> testInetaddr = (global::go.net_package.IPAddr ip) => new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ip.IP, Port: 5682, Zone: ip.Zone)));

partial struct addrListTestsᴛ1 /*dyn*/ {
    internal Func<global::go.net_package.IPAddr, bool> filter;
    internal slice<global::go.net_package.IPAddr> ips;
    internal Func<global::go.net_package.IPAddr, global::go.net_package.ΔAddr> inetaddr;
    internal global::go.net_package.ΔAddr first;
    internal global::go.net_package.addrList primaries;
    internal global::go.net_package.addrList fallbacks;
    internal error err;
}
internal static slice<addrListTestsᴛ1> addrListTests;
internal static void initᴛaddrListTests() { addrListTests = slice<addrListTestsᴛ1>([
    new(
        default,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv4(127, 0, 0, 1)),
            new(IP: IPv6loopback)
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682)))}.slice()),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682)))}.slice()),
        default
    ),
    new(
        default,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv6loopback),
            new(IP: IPv4(127, 0, 0, 1))
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682)))}.slice()),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682)))}.slice()),
        default
    ),
    new(
        default,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv4(127, 0, 0, 1)),
            new(IP: IPv4(192, 168, 0, 1))
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))), new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(192, 168, 0, 1), Port: 5682)))
        }.slice()),
        default,
        default
    ),
    new(
        default,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv6loopback),
            new(IP: ParseIP("fe80::1"), Zone: "eth0")
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682))), new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("fe80::1"), Port: 5682, Zone: "eth0")))
        }.slice()),
        default,
        default
    ),
    new(
        default,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv4(127, 0, 0, 1)),
            new(IP: IPv4(192, 168, 0, 1)),
            new(IP: IPv6loopback),
            new(IP: ParseIP("fe80::1"), Zone: "eth0")
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))), new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(192, 168, 0, 1), Port: 5682)))
        }.slice()),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682))), new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("fe80::1"), Port: 5682, Zone: "eth0")))
        }.slice()),
        default
    ),
    new(
        default,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv6loopback),
            new(IP: ParseIP("fe80::1"), Zone: "eth0"),
            new(IP: IPv4(127, 0, 0, 1)),
            new(IP: IPv4(192, 168, 0, 1))
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682))), new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("fe80::1"), Port: 5682, Zone: "eth0")))
        }.slice()),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))), new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(192, 168, 0, 1), Port: 5682)))
        }.slice()),
        default
    ),
    new(
        default,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv4(127, 0, 0, 1)),
            new(IP: IPv6loopback),
            new(IP: IPv4(192, 168, 0, 1)),
            new(IP: ParseIP("fe80::1"), Zone: "eth0")
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))), new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(192, 168, 0, 1), Port: 5682)))
        }.slice()),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682))), new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("fe80::1"), Port: 5682, Zone: "eth0")))
        }.slice()),
        default
    ),
    new(
        default,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv6loopback),
            new(IP: IPv4(127, 0, 0, 1)),
            new(IP: ParseIP("fe80::1"), Zone: "eth0"),
            new(IP: IPv4(192, 168, 0, 1))
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682))), new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("fe80::1"), Port: 5682, Zone: "eth0")))
        }.slice()),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))), new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(192, 168, 0, 1), Port: 5682)))
        }.slice()),
        default
    ),

    new(
        ipv4only,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv4(127, 0, 0, 1)),
            new(IP: IPv6loopback)
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682)))}.slice()),
        default,
        default
    ),
    new(
        ipv4only,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv6loopback),
            new(IP: IPv4(127, 0, 0, 1))
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 5682)))}.slice()),
        default,
        default
    ),

    new(
        ipv6only,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv4(127, 0, 0, 1)),
            new(IP: IPv6loopback)
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682)))}.slice()),
        default,
        default
    ),
    new(
        ipv6only,
        slice<global::go.net_package.IPAddr>([
            new(IP: IPv6loopback),
            new(IP: IPv4(127, 0, 0, 1))
        ]),
        testInetaddr,
        new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682))),
        new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback, Port: 5682)))}.slice()),
        default,
        default
    ),

    new(default, default, testInetaddr, default, default, default, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(errNoSuitableAddress.Error(), "ADDR")))),

    new(ipv4only, default, testInetaddr, default, default, default, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(errNoSuitableAddress.Error(), "ADDR")))),
    new(ipv4only, [new(IP: IPv6loopback)], testInetaddr, default, default, default, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(errNoSuitableAddress.Error(), "ADDR")))),

    new(ipv6only, default, testInetaddr, default, default, default, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(errNoSuitableAddress.Error(), "ADDR")))),
    new(ipv6only, slice<global::go.net_package.IPAddr>([new(IP: IPv4(127, 0, 0, 1))]), testInetaddr, default, default, default, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(errNoSuitableAddress.Error(), "ADDR"))))
]); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string addrˢ => ref addrˢᵇ;
private static readonly @string addrˢᵇ = "ADDR"u8;

public static void TestAddrList(ж<testing.T> t) {
    if (!supportsIPv4() || !supportsIPv6()) {
        t.Skip(bothIPv4AndIPv6Areˢ);
    }

    foreach (var (i, tt) in range(addrListTests)) {
        var (addrs, err) = filterAddrList(tt.filter, tt.ips, tt.inetaddr, addrˢ);
        if (!reflect.DeepEqual(err, tt.err)) {
            t.Errorf("#%v: got %v; want %v", i, err, tt.err);
        }
        if (tt.err != nil) {
            if (len(addrs) != 0) {
                t.Errorf("#%v: got %v; want 0", i, len(addrs));
            }
            continue;
        }
        var first = addrs.first(isIPv4);
        if (!reflect.DeepEqual(first, tt.first)) {
            t.Errorf("#%v: got %v; want %v", i, first, tt.first);
        }
        var (primaries, fallbacks) = addrs.partition(isIPv4);
        if (!reflect.DeepEqual(primaries, tt.primaries)) {
            t.Errorf("#%v: got %v; want %v", i, primaries, tt.primaries);
        }
        if (!reflect.DeepEqual(fallbacks, tt.fallbacks)) {
            t.Errorf("#%v: got %v; want %v", i, fallbacks, tt.fallbacks);
        }
        nint expectedLen = len(primaries) + len(fallbacks);
        if (len(addrs) != expectedLen) {
            t.Errorf("#%v: got %v; want %v", i, len(addrs), expectedLen);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fe80ˢ => ref fe80ˢᵇ;
private static readonly @string fe80ˢᵇ = "fe80::"u8;
internal static ref readonly @string fe801ˢ => ref fe801ˢᵇ;
private static readonly @string fe801ˢᵇ = "fe80::1"u8;
internal static ref readonly @string fe802ˢ => ref fe802ˢᵇ;
private static readonly @string fe802ˢᵇ = "fe80::2"u8;

internal partial struct TestAddrListPartition_cases /*dyn*/ {
    internal byte lastByte;
    internal global::go.net_package.addrList primaries;
    internal global::go.net_package.addrList fallbacks;
}

public static void TestAddrListPartition(ж<testing.T> t) {
    var addrs = new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.IPAddrжΔAddr(Ꮡ(new IPAddr(IP: ParseIP(fe80ˢ), Zone: "eth0"))), new global::go.net_package.IPAddrжΔAddr(Ꮡ(new IPAddr(IP: ParseIP(fe801ˢ), Zone: "eth0"))), new global::go.net_package.IPAddrжΔAddr(Ꮡ(new IPAddr(IP: ParseIP(fe802ˢ), Zone: "eth0")))
    }.slice());
    var cases = slice<TestAddrListPartition_cases>([
        new(0, new addrList(new global::go.net_package.ΔAddr[]{addrs[0]}.slice()), new addrList(new global::go.net_package.ΔAddr[]{addrs[1], addrs[2]}.slice())),
        new(1, new addrList(new global::go.net_package.ΔAddr[]{addrs[0], addrs[2]}.slice()), new addrList(new global::go.net_package.ΔAddr[]{addrs[1]}.slice())),
        new(2, new addrList(new global::go.net_package.ΔAddr[]{addrs[0], addrs[1]}.slice()), new addrList(new global::go.net_package.ΔAddr[]{addrs[2]}.slice())),
        new(3, new addrList(new global::go.net_package.ΔAddr[]{addrs[0], addrs[1], addrs[2]}.slice()), default)
    ]);
    foreach (var (i, vᴛ1) in range(cases)) {
        ref var tt = ref heap(new TestAddrListPartition_cases(), out var Ꮡtt);
        tt = vᴛ1;

        // Inverting the function's output should not affect the outcome.
        foreach (var (_, invert) in range(slice<bool>([false, true]))) {
            var ttʗ1 = tt;
            var (primaries, fallbacks) = addrs.partition((global::go.net_package.ΔAddr a) => {
                var ip = a._<ж<global::go.net_package.IPAddr>>().IP;
                return (ip[len(ip) - 1] == ttʗ1.lastByte) != invert;
            });
            if (!reflect.DeepEqual(primaries, tt.primaries)) {
                t.Errorf("#%v: got %v; want %v", i, primaries, tt.primaries);
            }
            if (!reflect.DeepEqual(fallbacks, tt.fallbacks)) {
                t.Errorf("#%v: got %v; want %v", i, fallbacks, tt.fallbacks);
            }
        }
    }
}

} // end net_internal_test_package
