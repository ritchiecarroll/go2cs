// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = go.os.exec_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using go.os;
using static go.net_package;
using ꓸꓸꓸany = Span<any>;

partial class net_internal_test_package {

internal static bool unixEnabledOnAIX;

[GoInit] internal static void initΔ1() {
    if (Δruntime.GOOS == "aix"u8) {
        // Unix network isn't properly working on AIX 7.2 with
        // Technical Level < 2.
        // The information is retrieved only once in this init()
        // instead of everytime testableNetwork is called.
        var (@out, _) = exec.Command("oslevel"u8, "-s"u8).Output();
        if (len(@out) >= len("7200-XX-ZZ-YYMM")) {
            // AIX 7.2, Tech Level XX, Service Pack ZZ, date YYMM
            sstring aixVer = ((sstring)(@out[..4]));
            var (tl, _) = strconv.Atoi(((@string)(@out[5..7])));
            unixEnabledOnAIX = aixVer > "7200"u8 || (aixVer == "7200"u8 && tl >= 2);
        }
    }
}

// testableNetwork reports whether network is testable on the current
// platform configuration.
internal static bool testableNetwork(@string network) {
    var (net, _, _) = strings.Cut(network, ":"u8);
    var exprᴛ1 = net;
    if (exprᴛ1 == "ip+nopriv"u8) {
    }
    else if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
        var exprᴛ2 = Δruntime.GOOS;
        if (exprᴛ2 == "plan9"u8) {
            return false;
        }
        { /* default: */
            if (Δos.Getuid() != 0) {
                return false;
            }
        }

    }
    else if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8) {
        var exprᴛ3 = Δruntime.GOOS;
        if (exprᴛ3 == "android"u8 || exprᴛ3 == "ios"u8 || exprᴛ3 == "plan9"u8 || exprᴛ3 == "windows"u8) {
            return false;
        }
        if (exprᴛ3 == "aix"u8) {
            return unixEnabledOnAIX;
        }

    }
    else if (exprᴛ1 == "unixpacket"u8) {
        var exprᴛ4 = Δruntime.GOOS;
        if (exprᴛ4 == "aix"u8 || exprᴛ4 == "android"u8 || exprᴛ4 == "darwin"u8 || exprᴛ4 == "ios"u8 || exprᴛ4 == "plan9"u8 || exprᴛ4 == "windows"u8) {
            return false;
        }

    }

    var exprᴛ5 = net;
    if (exprᴛ5 == "tcp4"u8 || exprᴛ5 == "udp4"u8 || exprᴛ5 == "ip4"u8) {
        if (!supportsIPv4()) {
            return false;
        }
    }
    else if (exprᴛ5 == "tcp6"u8 || exprᴛ5 == "udp6"u8 || exprᴛ5 == "ip6"u8) {
        if (!supportsIPv6()) {
            return false;
        }
    }

    return true;
}

// testableAddress reports whether address of network is testable on
// the current platform configuration.
internal static bool testableAddress(@string network, @string address) {
    {
        var (net, _, _) = strings.Cut(network, ":"u8);
        var exprᴛ1 = net;
        if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
            if (address[0] == (rune)'@' && Δruntime.GOOS != "linux"u8) {
                // Abstract unix domain sockets, a Linux-ism.
                return false;
            }
        }
    }

    return true;
}

// testableListenArgs reports whether arguments are testable on the
// current platform configuration.
internal static bool testableListenArgs(@string network, @string address, @string client) {
    if (!testableNetwork(network) || !testableAddress(network, address)) {
        return false;
    }
    error err = default!;
    global::go.net_package.ΔAddr addr = default!;
    {
        var (net, _, _) = strings.Cut(network, ":"u8);
        var exprᴛ1 = net;
        if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
            var (ᴛ1, ᴛ2) = ResolveTCPAddr(tcpˢ, address);
            (addr, err) = (new global::go.net_package.TCPAddrжΔAddr(ᴛ1), ᴛ2);
        }
        else if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
            var (ᴛ1, ᴛ2) = ResolveUDPAddr(udpˢ, address);
            (addr, err) = (new global::go.net_package.UDPAddrжΔAddr(ᴛ1), ᴛ2);
        }
        else if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
            var (ᴛ1, ᴛ2) = ResolveIPAddr("ip"u8, address);
            (addr, err) = (new global::go.net_package.IPAddrжΔAddr(ᴛ1), ᴛ2);
        }
        else { /* default: */
            return true;
        }
    }

    if (err != default!) {
        return false;
    }
    global::go.net_package.IP ip = default!;
    bool wildcard = default!;
    switch (addr.type()) {
    case ж<global::go.net_package.TCPAddr> addrΔ1: {
        ip = addrΔ1.Value.IP;
        wildcard = addrΔ1.isWildcard();
        break;
    }
    case ж<global::go.net_package.UDPAddr> addrΔ1: {
        ip = addrΔ1.Value.IP;
        wildcard = addrΔ1.isWildcard();
        break;
    }
    case ж<global::go.net_package.IPAddr> addrΔ1: {
        ip = addrΔ1.Value.IP;
        wildcard = addrΔ1.isWildcard();
        break;
    }}
    // Test wildcard IP addresses.
    if (wildcard && !testenv.HasExternalNetwork()) {
        return false;
    }
    // Test functionality of IPv4 communication using AF_INET and
    // IPv6 communication using AF_INET6 sockets.
    if (!supportsIPv4() && ip.To4() != default!) {
        return false;
    }
    if (!supportsIPv6() && ip.To16() != default! && ip.To4() == default!) {
        return false;
    }
    var cip = ParseIP(client);
    if (cip != default!) {
        if (!supportsIPv4() && cip.To4() != default!) {
            return false;
        }
        if (!supportsIPv6() && cip.To16() != default! && cip.To4() == default!) {
            return false;
        }
    }
    // Test functionality of IPv4 communication using AF_INET6
    // sockets.
    if (!supportsIPv4map() && supportsIPv4() && (network == "tcp"u8 || network == "udp"u8 || network == "ip"u8) && wildcard) {
        // At this point, we prefer IPv4 when ip is nil.
        // See favoriteAddrFamily for further information.
        if (ip.To16() != default! && ip.To4() == default! && cip.To4() != default!) {
            // a pair of IPv6 server and IPv4 client
            return false;
        }
        if ((ip.To4() != default! || ip == default!) && cip.To16() != default! && cip.To4() == default!) {
            // a pair of IPv4 server and IPv6 client
            return false;
        }
    }
    return true;
}

internal static void condFatalf(ж<testing.T> Ꮡt, @string network, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    Ꮡt.Helper();
    // A few APIs like File and Read/WriteMsg{UDP,IP} are not
    // fully implemented yet on Plan 9 and Windows.
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "windows"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        if (network == "file+net"u8) {
            Ꮡt.Logf(format, args.ꓸꓸꓸ);
            return;
        }
    }
    else if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Logf(format, args.ꓸꓸꓸ);
        return;
    }

    Ꮡt.Fatalf(format, args.ꓸꓸꓸ);
}

} // end net_internal_test_package
