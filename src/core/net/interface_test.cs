// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using testing = testing_package;
using static go.net_package;

partial class net_internal_test_package {

// loopbackInterface returns an available logical network interface
// for loopback tests. It returns nil if no suitable interface is
// found.
internal static ж<global::go.net_package.Interface> loopbackInterface() {
    var (ift, err) = Interfaces();
    if (err != default!) {
        return default!;
    }
    foreach (var (_, vᴛ1) in ift) {
        ref var ifi = ref heap(new global::go.net_package.Interface(), out var Ꮡifi);
        ifi = vᴛ1;

        if ((global::go.net_package.Flags)(ifi.Flags & FlagLoopback) != 0 && (global::go.net_package.Flags)(ifi.Flags & FlagUp) != 0) {
            return Ꮡifi;
        }
    }
    return default!;
}

// ipv6LinkLocalUnicastAddr returns an IPv6 link-local unicast address
// on the given network interface for tests. It returns "" if no
// suitable address is found.
internal static @string ipv6LinkLocalUnicastAddr(ж<global::go.net_package.Interface> Ꮡifi) {
    if (Ꮡifi == nil) {
        return ""u8;
    }
    var (ifat, err) = Ꮡifi.Addrs();
    if (err != default!) {
        return ""u8;
    }
    foreach (var (_, ifa) in ifat) {
        {
            var (ifaΔ1, ok) = ifa._<ж<global::go.net_package.IPNet>>(ᐧ); if (ok) {
                if ((~ifaΔ1).IP.To4() == default! && (~ifaΔ1).IP.IsLinkLocalUnicast()) {
                    return (~ifaΔ1).IP.String();
                }
            }
        }
    }
    return ""u8;
}

public static void TestInterfaces(ж<testing.T> Ꮡt) {
    var (ift, err) = Interfaces();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, vᴛ1) in ift) {
        ref var ifi = ref heap(new global::go.net_package.Interface(), out var Ꮡifi);
        ifi = vᴛ1;

        var (ifxi, errΔ1) = InterfaceByIndex(ifi.Index);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "solaris"u8 || exprᴛ1 == "illumos"u8) {
            if ((~ifxi).Index != ifi.Index) {
                Ꮡt.Errorf("got %v; want %v"u8, ifxi.OrTypedNil(), ifi);
            }
        }
        else { /* default: */
            if (!reflect.DeepEqual(ifxi.OrTypedNil(), Ꮡifi)) {
                Ꮡt.Errorf("got %v; want %v"u8, ifxi.OrTypedNil(), ifi);
            }
        }

        (var ifxn, errΔ1) = InterfaceByName(ifi.Name);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        if (!reflect.DeepEqual(ifxn.OrTypedNil(), Ꮡifi)) {
            Ꮡt.Errorf("got %v; want %v"u8, ifxn.OrTypedNil(), ifi);
        }
        Ꮡt.Logf("%s: flags=%v index=%d mtu=%d hwaddr=%v"u8, ifi.Name, ifi.Flags, ifi.Index, ifi.MTU, ifi.HardwareAddr);
    }
}

public static void TestInterfaceAddrs(ж<testing.T> Ꮡt) {
    var (ift, err) = Interfaces();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var ifStats = interfaceStats(ift);
    (var ifat, err) = InterfaceAddrs();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var uniStats, err) = validateInterfaceUnicastAddrs(ifat);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = checkUnicastStats(ifStats, uniStats); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
}

public static void TestInterfaceUnicastAddrs(ж<testing.T> Ꮡt) {
    var (ift, err) = Interfaces();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var ifStats = interfaceStats(ift);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var uniStats = ref heap(new routeStats(), out var ᏑuniStats);
    foreach (var (_, vᴛ1) in ift) {
        ref var ifi = ref heap(new global::go.net_package.Interface(), out var Ꮡifi);
        ifi = vᴛ1;

        var (ifat, errΔ1) = Ꮡifi.Addrs();
        if (errΔ1 != default!) {
            Ꮡt.Fatal(ifi, errΔ1);
        }
        (var stats, errΔ1) = validateInterfaceUnicastAddrs(ifat);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(ifi, errΔ1);
        }
        uniStats.ipv4 += stats.Value.ipv4;
        uniStats.ipv6 += stats.Value.ipv6;
    }
    {
        var errΔ2 = checkUnicastStats(ifStats, ᏑuniStats); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
}

public static void TestInterfaceMulticastAddrs(ж<testing.T> Ꮡt) {
    var (ift, err) = Interfaces();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var ifStats = interfaceStats(ift);
    (var ifat, err) = InterfaceAddrs();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var uniStats, err) = validateInterfaceUnicastAddrs(ifat);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var multiStats = ref heap(new routeStats(), out var ᏑmultiStats);
    foreach (var (_, vᴛ1) in ift) {
        ref var ifi = ref heap(new global::go.net_package.Interface(), out var Ꮡifi);
        ifi = vᴛ1;

        var (ifmat, errΔ1) = Ꮡifi.MulticastAddrs();
        if (errΔ1 != default!) {
            Ꮡt.Fatal(ifi, errΔ1);
        }
        (var stats, errΔ1) = validateInterfaceMulticastAddrs(ifmat);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(ifi, errΔ1);
        }
        multiStats.ipv4 += stats.Value.ipv4;
        multiStats.ipv6 += stats.Value.ipv6;
    }
    {
        var errΔ2 = checkMulticastStats(ifStats, uniStats, ᏑmultiStats); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
}

internal partial struct ifStats {
    internal nint loop; // # of active loopback interfaces
    internal nint other; // # of active other interfaces
}

internal static ж<ifStats> interfaceStats(slice<global::go.net_package.Interface> ift) {
    ref var stats = ref heap(new ifStats(), out var Ꮡstats);
    foreach (var (_, ifi) in ift) {
        if ((global::go.net_package.Flags)(ifi.Flags & FlagUp) != 0) {
            if ((global::go.net_package.Flags)(ifi.Flags & FlagLoopback) != 0){
                stats.loop++;
            } else {
                stats.other++;
            }
        }
    }
    return Ꮡstats;
}

internal partial struct routeStats {
    internal nint ipv4, ipv6; // # of active connected unicast, anycast or multicast routes
}

internal static (ж<routeStats>, error) validateInterfaceUnicastAddrs(slice<global::go.net_package.ΔAddr> ifat) {
    // Note: BSD variants allow assigning any IPv4/IPv6 address
    // prefix to IP interface. For example,
    //   - 0.0.0.0/0 through 255.255.255.255/32
    //   - ::/0 through ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff/128
    // In other words, there is no tightly-coupled combination of
    // interface address prefixes and connected routes.
    var stats = @new<routeStats>();
    foreach (var (_, ifa) in ifat) {
        switch (ifa.type()) {
        case ж<global::go.net_package.IPNet> ifaΔ1: {
            if (ifaΔ1 == nil || (~ifaΔ1).IP == default! || (~ifaΔ1).IP.IsMulticast() || (~ifaΔ1).Mask == default!) {
                return (default!, fmt.Errorf("unexpected value: %#v"u8, ifaΔ1.OrTypedNil()));
            }
            if (len((~ifaΔ1).IP) != IPv6len) {
                return (default!, fmt.Errorf("should be internal representation either IPv6 or IPv4-mapped IPv6 address: %#v"u8, ifaΔ1.OrTypedNil()));
            }
            var (prefixLen, maxPrefixLen) = (~ifaΔ1).Mask.Size();
            if ((~ifaΔ1).IP.To4() != default!) {
                if (0 >= prefixLen || prefixLen > (nint)(8 * IPv4len) || maxPrefixLen != (nint)(8 * IPv4len)) {
                    return (default!, fmt.Errorf("unexpected prefix length: %d/%d for %#v"u8, prefixLen, maxPrefixLen, ifaΔ1.OrTypedNil()));
                }
                if ((~ifaΔ1).IP.IsLoopback() && prefixLen < 8) {
                    // see RFC 1122
                    return (default!, fmt.Errorf("unexpected prefix length: %d/%d for %#v"u8, prefixLen, maxPrefixLen, ifaΔ1.OrTypedNil()));
                }
                stats.Value.ipv4++;
            }
            if ((~ifaΔ1).IP.To16() != default! && (~ifaΔ1).IP.To4() == default!) {
                if (0 >= prefixLen || prefixLen > (nint)(8 * IPv6len) || maxPrefixLen != (nint)(8 * IPv6len)) {
                    return (default!, fmt.Errorf("unexpected prefix length: %d/%d for %#v"u8, prefixLen, maxPrefixLen, ifaΔ1.OrTypedNil()));
                }
                if ((~ifaΔ1).IP.IsLoopback() && prefixLen != (nint)(8 * IPv6len)) {
                    // see RFC 4291
                    return (default!, fmt.Errorf("unexpected prefix length: %d/%d for %#v"u8, prefixLen, maxPrefixLen, ifaΔ1.OrTypedNil()));
                }
                stats.Value.ipv6++;
            }
            break;
        }
        case ж<global::go.net_package.IPAddr> ifaΔ1: {
            if (ifaΔ1 == nil || (~ifaΔ1).IP == default! || (~ifaΔ1).IP.IsMulticast()) {
                return (default!, fmt.Errorf("unexpected value: %#v"u8, ifaΔ1.OrTypedNil()));
            }
            if (len((~ifaΔ1).IP) != IPv6len) {
                return (default!, fmt.Errorf("should be internal representation either IPv6 or IPv4-mapped IPv6 address: %#v"u8, ifaΔ1.OrTypedNil()));
            }
            if ((~ifaΔ1).IP.To4() != default!) {
                stats.Value.ipv4++;
            }
            if ((~ifaΔ1).IP.To16() != default! && (~ifaΔ1).IP.To4() == default!) {
                stats.Value.ipv6++;
            }
            break;
        }
        default: {
            var ifaΔ1 = ifa;
            return (default!, fmt.Errorf("unexpected type: %T"u8, ifaΔ1));
        }}
    }
    return (stats, default!);
}

internal static (ж<routeStats>, error) validateInterfaceMulticastAddrs(slice<global::go.net_package.ΔAddr> ifat) {
    var stats = @new<routeStats>();
    foreach (var (_, ifa) in ifat) {
        switch (ifa.type()) {
        case ж<global::go.net_package.IPAddr> ifaΔ1: {
            if (ifaΔ1 == nil || (~ifaΔ1).IP == default! || (~ifaΔ1).IP.IsUnspecified() || !(~ifaΔ1).IP.IsMulticast()) {
                return (default!, fmt.Errorf("unexpected value: %#v"u8, ifaΔ1.OrTypedNil()));
            }
            if (len((~ifaΔ1).IP) != IPv6len) {
                return (default!, fmt.Errorf("should be internal representation either IPv6 or IPv4-mapped IPv6 address: %#v"u8, ifaΔ1.OrTypedNil()));
            }
            if ((~ifaΔ1).IP.To4() != default!) {
                stats.Value.ipv4++;
            }
            if ((~ifaΔ1).IP.To16() != default! && (~ifaΔ1).IP.To4() == default!) {
                stats.Value.ipv6++;
            }
            break;
        }
        default: {
            var ifaΔ1 = ifa;
            return (default!, fmt.Errorf("unexpected type: %T"u8, ifaΔ1));
        }}
    }
    return (stats, default!);
}

internal static error checkUnicastStats(ж<ifStats> ᏑifStats, ж<routeStats> ᏑuniStats) {
    ref var ifStats = ref ᏑifStats.DerefOrNull();
    ref var uniStats = ref ᏑuniStats.DerefOrNull();

    // Test the existence of connected unicast routes for IPv4.
    if (supportsIPv4() && ifStats.loop + ifStats.other > 0 && uniStats.ipv4 == 0) {
        return fmt.Errorf("num IPv4 unicast routes = 0; want >0; summary: %+v, %+v"u8, ᏑifStats.OrTypedNil(), ᏑuniStats.OrTypedNil());
    }
    // Test the existence of connected unicast routes for IPv6.
    // We can assume the existence of ::1/128 when at least one
    // loopback interface is installed.
    if (supportsIPv6() && ifStats.loop > 0 && uniStats.ipv6 == 0) {
        return fmt.Errorf("num IPv6 unicast routes = 0; want >0; summary: %+v, %+v"u8, ᏑifStats.OrTypedNil(), ᏑuniStats.OrTypedNil());
    }
    return default!;
}

internal static error checkMulticastStats(ж<ifStats> ᏑifStats, ж<routeStats> ᏑuniStats, ж<routeStats> ᏑmultiStats) {
    ref var ifStats = ref ᏑifStats.DerefOrNull();
    ref var uniStats = ref ᏑuniStats.DerefOrNull();
    ref var multiStats = ref ᏑmultiStats.DerefOrNull();

    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "aix"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "plan9"u8 || exprᴛ1 == "solaris"u8 || exprᴛ1 == "illumos"u8) {
    }
    else { /* default: */
        if (supportsIPv6() && ifStats.loop > 0 && uniStats.ipv6 > 1 && multiStats.ipv6 == 0) {
            // Test the existence of connected multicast route
            // clones for IPv4. Unlike IPv6, IPv4 multicast
            // capability is not a mandatory feature, and so IPv4
            // multicast validation is ignored and we only check
            // IPv6 below.
            //
            // Test the existence of connected multicast route
            // clones for IPv6. Some platform never uses loopback
            // interface as the nexthop for multicast routing.
            // We can assume the existence of connected multicast
            // route clones when at least two connected unicast
            // routes, ::1/128 and other, are installed.
            return fmt.Errorf("num IPv6 multicast route clones = 0; want >0; summary: %+v, %+v, %+v"u8, ᏑifStats.OrTypedNil(), ᏑuniStats.OrTypedNil(), ᏑmultiStats.OrTypedNil());
        }
    }

    return default!;
}

public static void BenchmarkInterfaces(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    for (nint i = 0; i < b.N; i++) {
        {
            var (_, err) = Interfaces(); if (err != default!) {
                Ꮡb.Fatal(err);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object loopbackInterfaceNotˢ = (@string)"loopback interface not found"u8;

public static void BenchmarkInterfaceByIndex(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    var ifi = loopbackInterface();
    if (ifi == nil) {
        Ꮡb.Skip(loopbackInterfaceNotˢ);
    }
    for (nint i = 0; i < b.N; i++) {
        {
            var (_, err) = InterfaceByIndex((~ifi).Index); if (err != default!) {
                Ꮡb.Fatal(err);
            }
        }
    }
}

public static void BenchmarkInterfaceByName(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    var ifi = loopbackInterface();
    if (ifi == nil) {
        Ꮡb.Skip(loopbackInterfaceNotˢ);
    }
    for (nint i = 0; i < b.N; i++) {
        {
            var (_, err) = InterfaceByName((~ifi).Name); if (err != default!) {
                Ꮡb.Fatal(err);
            }
        }
    }
}

public static void BenchmarkInterfaceAddrs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    for (nint i = 0; i < b.N; i++) {
        {
            var (_, err) = InterfaceAddrs(); if (err != default!) {
                Ꮡb.Fatal(err);
            }
        }
    }
}

public static void BenchmarkInterfacesAndAddrs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    var ifi = loopbackInterface();
    if (ifi == nil) {
        Ꮡb.Skip(loopbackInterfaceNotˢ);
    }
    for (nint i = 0; i < b.N; i++) {
        {
            var (_, err) = ifi.Addrs(); if (err != default!) {
                Ꮡb.Fatal(err);
            }
        }
    }
}

public static void BenchmarkInterfacesAndMulticastAddrs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    var ifi = loopbackInterface();
    if (ifi == nil) {
        Ꮡb.Skip(loopbackInterfaceNotˢ);
    }
    for (nint i = 0; i < b.N; i++) {
        {
            var (_, err) = ifi.MulticastAddrs(); if (err != default!) {
                Ꮡb.Fatal(err);
            }
        }
    }
}

} // end net_internal_test_package
