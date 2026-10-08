// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using bytealg = @internal.bytealg_package;
using runtime = runtime_package;
using Δsync = sync_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal;

partial class net_package {

// BUG(rsc,mikio): On DragonFly BSD and OpenBSD, listening on the
// "tcp" and "udp" networks does not listen for both IPv4 and IPv6
// connections. This is due to the fact that IPv4 traffic will not be
// routed to an IPv6 socket - two separate sockets are required if
// both address families are to be supported.
// See inet6(4) for details.
partial struct ipStackCapabilities {
    public partial ref sync_package.Once Once { get; }             // guards following
    internal bool ipv4Enabled;
    internal bool ipv6Enabled;
    internal bool ipv4MappedIPv6Enabled;
}

internal static ж<ipStackCapabilities> ᏑipStackCaps = new StandardBox<ipStackCapabilities>(new ipStackCapabilities(nil));
internal static ref ipStackCapabilities ipStackCaps => ref ᏑipStackCaps.Value;

// supportsIPv4 reports whether the platform supports IPv4 networking
// functionality.
internal static bool supportsIPv4() {
    ᏑipStackCaps.of(ipStackCapabilities.ᏑOnce).Do(ᏑipStackCaps.probe);
    return ipStackCaps.ipv4Enabled;
}

// supportsIPv6 reports whether the platform supports IPv6 networking
// functionality.
internal static bool supportsIPv6() {
    ᏑipStackCaps.of(ipStackCapabilities.ᏑOnce).Do(ᏑipStackCaps.probe);
    return ipStackCaps.ipv6Enabled;
}

// supportsIPv4map reports whether the platform supports mapping an
// IPv4 address inside an IPv6 address at transport layer
// protocols. See RFC 4291, RFC 4038 and RFC 3493.
internal static bool supportsIPv4map() {
    // Some operating systems provide no support for mapping IPv4
    // addresses to IPv6, and a runtime check is unnecessary.
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "openbsd"u8) {
        return false;
    }

    ᏑipStackCaps.of(ipStackCapabilities.ᏑOnce).Do(ᏑipStackCaps.probe);
    return ipStackCaps.ipv4MappedIPv6Enabled;
}

partial struct addrList /*[]ΔAddr*/;

// isIPv4 reports whether addr contains an IPv4 address.
internal static bool isIPv4(ΔAddr addr) {
    switch (addr.type()) {
    case ж<TCPAddr> addrΔ1: {
        return (~addrΔ1).IP.To4() != default!;
    }
    case ж<UDPAddr> addrΔ1: {
        return (~addrΔ1).IP.To4() != default!;
    }
    case ж<IPAddr> addrΔ1: {
        return (~addrΔ1).IP.To4() != default!;
    }}
    return false;
}

// isNotIPv4 reports whether addr does not contain an IPv4 address.
internal static bool isNotIPv4(ΔAddr addr) {
    return !isIPv4(addr);
}

// forResolve returns the most appropriate address in address for
// a call to ResolveTCPAddr, ResolveUDPAddr, or ResolveIPAddr.
// IPv4 is preferred, unless addr contains an IPv6 literal.
internal static ΔAddr forResolve(this addrList addrs, @string network, @string addr) {
    bool want6 = default!;
    var exprᴛ1 = network;
    if (exprᴛ1 == "ip"u8) {
        want6 = bytealg.CountString(addr, // IPv6 literal (addr does NOT contain a port)
 (rune)':') > 0;
    }
    else if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "udp"u8) {
        want6 = bytealg.CountString(addr, // IPv6 literal. (addr contains a port, so look for '[')
 (rune)'[') > 0;
    }

    if (want6) {
        return addrs.first(isNotIPv4);
    }
    return addrs.first(isIPv4);
}

// first returns the first address which satisfies strategy, or if
// none do, then the first address of any kind.
internal static ΔAddr first(this addrList addrs, Func<ΔAddr, bool> strategy) {
    foreach (var (_, addr) in addrs) {
        if (strategy(addr)) {
            return addr;
        }
    }
    return addrs[0];
}

// partition divides an address list into two categories, using a
// strategy function to assign a boolean label to each address.
// The first address, and any with a matching label, are returned as
// primaries, while addresses with the opposite label are returned
// as fallbacks. For non-empty inputs, primaries is guaranteed to be
// non-empty.
internal static (addrList primaries, addrList fallbacks) partition(this addrList addrs, Func<ΔAddr, bool> strategy) {
    addrList primaries = default!;
    addrList fallbacks = default!;

    bool primaryLabel = default!;
    foreach (var (i, addr) in addrs) {
        var label = strategy(addr);
        if (i == 0 || label == primaryLabel){
            primaryLabel = label;
            primaries = append(primaries, addr);
        } else {
            fallbacks = append(fallbacks, addr);
        }
    }
    return (primaries, fallbacks);
}

// filterAddrList applies a filter to a list of IP addresses,
// yielding a list of Addr objects. Known filters are nil, ipv4only,
// and ipv6only. It returns every address when the filter is nil.
// The result contains at least one address when error is nil.
internal static (addrList, error) filterAddrList(Func<IPAddr, bool> filter, slice<IPAddr> ips, Func<IPAddr, ΔAddr> inetaddr, @string originalAddr) {
    addrList addrs = default!;
    foreach (var (_, ip) in ips) {
        if (filter == default! || filter(ip)) {
            addrs = append(addrs, inetaddr(ip));
        }
    }
    if (len(addrs) == 0) {
        return (default!, new AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: originalAddr))));
    }
    return (addrs, default!);
}

// ipv4only reports whether addr is an IPv4 address.
internal static bool ipv4only(IPAddr addr) {
    return addr.IP.To4() != default!;
}

// ipv6only reports whether addr is an IPv6 address except IPv4-mapped IPv6 address.
internal static bool ipv6only(IPAddr addr) {
    return len(addr.IP) == IPv6len && addr.IP.To4() == default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingInAddressˢ = "missing ']' in address"u8;
internal static readonly @string unexpectedInAddressˢ = "unexpected '[' in address"u8;
internal static readonly @string unexpectedInAddressˢ2 = "unexpected ']' in address"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string missingPortᶜ = "missing port in address"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string tooManyColonsᶜ = "too many colons in address"u8;

// SplitHostPort splits a network address of the form "host:port",
// "host%zone:port", "[host]:port" or "[host%zone]:port" into host or
// host%zone and port.
//
// A literal IPv6 address in hostport must be enclosed in square
// brackets, as in "[::1]:80", "[::1%lo0]:80".
//
// See func Dial for a description of the hostport parameter, and host
// and port results.
public static (@string host, @string port, error err) SplitHostPort(@string hostport) {
    @string host = default!;
    @string port = default!;

    @string missingPort = missingPortᶜ;
    @string tooManyColons = tooManyColonsᶜ;
    (@string host, @string port, error err) addrErr(@string addr, @string why) {
        return ("", "", new AddrErrorжerror(Ꮡ(new AddrError(Err: why, Addr: addr))));
    }
    nint j = 0;
    nint k = 0;
    // The port starts after the last colon.
    nint i = bytealg.LastIndexByteString(hostport, (rune)':');
    if (i < 0) {
        return addrErr(hostport, missingPort);
    }
    if (hostport[0] == (rune)'['){
        // Expect the first ']' just before the last ':'.
        nint end = bytealg.IndexByteString(hostport, (rune)']');
        if (end < 0) {
            return addrErr(hostport, missingInAddressˢ);
        }
        var exprᴛ1 = end + 1;
        if (exprᴛ1 == len(hostport)) {
            return addrErr(hostport, // There can't be a ':' behind the ']' now.
 missingPort);
        }
        if (exprᴛ1 == i) {
        }
        else { /* default: */
            if (hostport[end + 1] == (rune)':') {
                // The expected result.
                // Either ']' isn't followed by a colon, or it is
                // followed by a colon that is not the last one.
                return addrErr(hostport, tooManyColons);
            }
            return addrErr(hostport, missingPort);
        }

        host = hostport.slice(1, end);
        (j, k) = (1, end + 1); // there can't be a '[' resp. ']' before these positions
    } else {
        host = hostport.slice(0, i);
        if (bytealg.IndexByteString(host, (rune)':') >= 0) {
            return addrErr(hostport, tooManyColons);
        }
    }
    if (bytealg.IndexByteString(hostport.slice(j), (rune)'[') >= 0) {
        return addrErr(hostport, unexpectedInAddressˢ);
    }
    if (bytealg.IndexByteString(hostport.slice(k), (rune)']') >= 0) {
        return addrErr(hostport, unexpectedInAddressˢ2);
    }
    port = hostport.slice(i + 1);
    return (host, port, default!);
}

internal static (@string host, @string zone) splitHostZone(@string s) {
    @string host = default!;
    @string zone = default!;

    // The IPv6 scoped addressing zone identifier starts after the
    // last percent sign.
    {
        nint i = bytealg.LastIndexByteString(s, (rune)'%'); if (i > 0){
            (host, zone) = (s.slice(0, i), s.slice(i + 1));
        } else {
            host = s;
        }
    }
    return (host, zone);
}

// JoinHostPort combines host and port into a network address of the
// form "host:port". If host contains a colon, as found in literal
// IPv6 addresses, then JoinHostPort returns "[host]:port".
//
// See func Dial for a description of the host and port parameters.
public static @string JoinHostPort(@string host, @string port) {
    // We assume that host is a literal IPv6 address if host has
    // colons.
    if (bytealg.IndexByteString(host, (rune)':') >= 0) {
        return "["u8 + host + "]:"u8 + port;
    }
    return host + ":"u8 + port;
}

// internetAddrList resolves addr, which may be a literal IP
// address or a DNS name, and returns a list of internet protocol
// family addresses. The result contains at least one address when
// error is nil.
internal static (addrList, error) internetAddrList(this ж<Resolver> Ꮡr, context.Context ctx, @string net, @string addr) {
    error err = default!;
    @string host = default!;
    @string port = default!;
    ref var portnum = ref heap(new nint(), out var Ꮡportnum);
    var exprᴛ1 = net;
    if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8 || exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
        if (addr != ""u8) {
            {
                (host, port, err) = SplitHostPort(addr); if (err != default!) {
                    return (default!, err);
                }
            }
            {
                (portnum, err) = Ꮡr.LookupPort(ctx, net, port); if (err != default!) {
                    return (default!, err);
                }
            }
        }
    }
    else if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
        if (addr != ""u8) {
            host = addr;
        }
    }
    else { /* default: */
        return (default!, ((UnknownNetworkError)net));
    }

    var portnumʗ1 = portnum;
    var inetaddr = ΔAddr (IPAddr ip) => {
        var exprᴛ2 = net;
        if (exprᴛ2 == "tcp"u8 || exprᴛ2 == "tcp4"u8 || exprᴛ2 == "tcp6"u8) {
            return new TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ip.IP, Port: portnumʗ1, Zone: ip.Zone)));
        }
        if (exprᴛ2 == "udp"u8 || exprᴛ2 == "udp4"u8 || exprᴛ2 == "udp6"u8) {
            return new UDPAddrжΔAddr(Ꮡ(new UDPAddr(IP: ip.IP, Port: portnumʗ1, Zone: ip.Zone)));
        }
        if (exprᴛ2 == "ip"u8 || exprᴛ2 == "ip4"u8 || exprᴛ2 == "ip6"u8) {
            return new IPAddrжΔAddr(Ꮡ(new IPAddr(IP: ip.IP, Zone: ip.Zone)));
        }
        { /* default: */
            throw panic("unexpected network: " + net);
        }

    };
    if (host == ""u8) {
        return (new addrList(new ΔAddr[]{inetaddr(new IPAddr(nil))}.slice()), default!);
    }
    // Try as a literal IP address, then as a DNS name.
    (var ips, err) = Ꮡr.lookupIPAddr(ctx, net, host);
    if (err != default!) {
        return (default!, err);
    }
    // Issue 18806: if the machine has halfway configured
    // IPv6 such that it can bind on "::" (IPv6unspecified)
    // but not connect back to that same address, fall
    // back to dialing 0.0.0.0.
    if (len(ips) == 1 && ips[0].IP.Equal(IPv6unspecified)) {
        ips = append(ips, new IPAddr(IP: IPv4zero));
    }
    Func<IPAddr, bool> filter = default!;
    if (net != ""u8 && net[len(net) - 1] == (rune)'4') {
        filter = ipv4only;
    }
    if (net != ""u8 && net[len(net) - 1] == (rune)'6') {
        filter = ipv6only;
    }
    return filterAddrList(filter, ips, inetaddr, host);
}

// loopbackIP should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/database64128/tfo-go/v2
//   - github.com/metacubex/tfo-go
//   - github.com/sagernet/tfo-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname loopbackIP
internal static IP loopbackIP(@string net) {
    if (net != ""u8 && net[len(net) - 1] == (rune)'6') {
        return IPv6loopback;
    }
    return new IP(new byte[]{127, 0, 0, 1}.slice());
}

} // end net_package
