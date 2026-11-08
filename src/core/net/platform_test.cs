// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using Δos = os_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using static go.net_package;
using ꓸꓸꓸany = Span<any>;

partial class net_internal_test_package {

// testableNetwork reports whether network is testable on the current
// platform configuration.
internal static bool testableNetwork(@string network) {
    var (net, _, _) = strings.Cut(network, ":");
    var exprᴛ1 = net;
    switch (ᐧ) {
    case {} when exprᴛ1 == "ip+nopriv"u8: {
        break;
    }
    case {} when exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8: {
        var exprᴛ2 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ2 == "plan9"u8: {
            return false;
        }
        default: {
            if (Δos.Getuid() != 0) {
                return false;
            }
            break;
        }}
        break;
    }
    case {} when exprᴛ1 == "unixgram"u8: {
        var exprᴛ3 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ3 == "windows"u8: {
            return false;
        }
        default: {
            return supportsUnixSocket();
        }}
        break;
    }
    case {} when exprᴛ1 == "unix"u8: {
        return supportsUnixSocket();
    }
    case {} when exprᴛ1 == "unixpacket"u8: {
        var exprᴛ4 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ4 == "aix"u8 || exprᴛ4 == "android"u8 || exprᴛ4 == "darwin"u8 || exprᴛ4 == "ios"u8 || exprᴛ4 == "plan9"u8 || exprᴛ4 == "windows"u8: {
            return false;
        }}
        break;
    }}
    var exprᴛ5 = net;
    switch (ᐧ) {
    case {} when exprᴛ5 == "tcp4"u8 || exprᴛ5 == "udp4"u8 || exprᴛ5 == "ip4"u8: {
        if (!supportsIPv4()) {
            return false;
        }
        break;
    }
    case {} when exprᴛ5 == "tcp6"u8 || exprᴛ5 == "udp6"u8 || exprᴛ5 == "ip6"u8: {
        if (!supportsIPv6()) {
            return false;
        }
        break;
    }}
    return true;
}

// testableAddress reports whether address of network is testable on
// the current platform configuration.
internal static bool testableAddress(@string network, @string address) {
    var (net, _, _) = strings.Cut(network, ":");
    var exprᴛ1 = net;
    switch (ᐧ) {
    case {} when exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8: {
        // Abstract unix domain sockets, a Linux-ism.
        if (address[0] == '@' && Δruntime.GOOS != "linux"u8) {
            return false;
        }
        break;
    }}
    return true;
}

// testableListenArgs reports whether arguments are testable on the
// current platform configuration.
internal static bool testableListenArgs(@string network, @string address, @string client) {
    if (!testableNetwork(network) || !testableAddress(network, address)) {
        return false;
    }

    error err = default;
    global::go.net_package.ΔAddr addr = default;
    var (net, _, _) = strings.Cut(network, ":");
    var exprᴛ1 = net;
    switch (ᐧ) {
    case {} when exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8: {
        var (ᴛ1, ᴛ2) = ResolveTCPAddr(tcpˢ, address);
        (addr, err) = (new global::go.net_package.TCPAddrжΔAddr(ᴛ1), ᴛ2);
        break;
    }
    case {} when exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8: {
        var (ᴛ1, ᴛ2) = ResolveUDPAddr(udpˢ, address);
        (addr, err) = (new global::go.net_package.UDPAddrжΔAddr(ᴛ1), ᴛ2);
        break;
    }
    case {} when exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8: {
        var (ᴛ1, ᴛ2) = ResolveIPAddr("ip", address);
        (addr, err) = (new global::go.net_package.IPAddrжΔAddr(ᴛ1), ᴛ2);
        break;
    }
    default: {
        return true;
    }}
    if (err != nil) {
        return false;
    }
    global::go.net_package.IP ip = default;
    bool wildcard = default;
    switch (addr.type()) {
    case ж<global::go.net_package.TCPAddr> addrΔ1: {
        ip = addrΔ1.IP;
        wildcard = addrΔ1.isWildcard();
        break;
    }
    case ж<global::go.net_package.UDPAddr> addrΔ1: {
        ip = addrΔ1.IP;
        wildcard = addrΔ1.isWildcard();
        break;
    }
    case ж<global::go.net_package.IPAddr> addrΔ1: {
        ip = addrΔ1.IP;
        wildcard = addrΔ1.isWildcard();
        break;
    }}

    // Test wildcard IP addresses.
    if (wildcard && !testenv.HasExternalNetwork()) {
        return false;
    }

    // Test functionality of IPv4 communication using AF_INET and
    // IPv6 communication using AF_INET6 sockets.
    if (!supportsIPv4() && ip.To4() != nil) {
        return false;
    }
    if (!supportsIPv6() && ip.To16() != nil && ip.To4() == nil) {
        return false;
    }
    var cip = ParseIP(client);
    if (cip != nil) {
        if (!supportsIPv4() && cip.To4() != nil) {
            return false;
        }
        if (!supportsIPv6() && cip.To16() != nil && cip.To4() == nil) {
            return false;
        }
    }

    // Test functionality of IPv4 communication using AF_INET6
    // sockets.
    if (!supportsIPv4map() && supportsIPv4() && (network == "tcp"u8 || network == "udp"u8 || network == "ip"u8) && wildcard) {
        // At this point, we prefer IPv4 when ip is nil.
        // See favoriteAddrFamily for further information.
        if (ip.To16() != nil && ip.To4() == nil && cip.To4() != nil) { // a pair of IPv6 server and IPv4 client
            return false;
        }
        if ((ip.To4() != nil || ip == nil) && cip.To16() != nil && cip.To4() == nil) { // a pair of IPv4 server and IPv6 client
            return false;
        }
    }

    return true;
}

internal static void condFatalf(ж<testing.T> t, @string network, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    t.Helper();
    // A few APIs like File and Read/WriteMsg{UDP,IP} are not
    // fully implemented yet on Plan 9 and Windows.
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "windows"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8: {
        if (network == "file+net"u8) {
            t.Logf(format, args.ꓸꓸꓸ);
            return;
        }
        break;
    }
    case {} when exprᴛ1 == "plan9"u8: {
        t.Logf(format, args.ꓸꓸꓸ);
        return;
    }}
    t.Fatalf(format, args.ꓸꓸꓸ);
}

} // end net_internal_test_package
