// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || js || wasip1 || windows
namespace go;

using context = context_package;
using poll = @internal.poll_package;
using netip = net.netip_package;
using runtime = runtime_package;
using syscall = syscall_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal;
using net;

partial class net_package {

internal partial struct probe_type /*dyn*/ {
    internal TCPAddr laddr;
    internal nint value;
}

// probe probes IPv4, IPv6 and IPv4-mapped IPv6 communication
// capabilities which are controlled by the IPV6_V6ONLY socket option
// and kernel configuration.
//
// Should we try to use the IPv4 socket interface if we're only
// dealing with IPv4 sockets? As long as the host system understands
// IPv4-mapped IPv6, it's okay to pass IPv4-mapped IPv6 addresses to
// the IPv6 interface. That simplifies our code and is most
// general. Unfortunately, we need to run on kernels built without
// IPv6 support too. So probe the kernel to figure it out.
internal static void probe(this ж<ipStackCapabilities> Ꮡp) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        var exprᴛ1 = runtime.GOOS;
        if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
            p.ipv4Enabled = true;
            p.ipv6Enabled = true;
            p.ipv4MappedIPv6Enabled = true;
            return;
        }

        // Both ipv4 and ipv6 are faked; see net_fake.go.
        var (s, err) = sysSocket(syscall.AF_INET, syscall.SOCK_STREAM, syscall.IPPROTO_TCP);
        var exprᴛ2 = err;
        if (AreEqual(exprᴛ2, syscall.EAFNOSUPPORT) || AreEqual(exprᴛ2, syscall.EPROTONOSUPPORT)) {
        }
        else if (AreEqual(exprᴛ2, default!)) {
            poll.CloseFunc(s);
            p.ipv4Enabled = true;
        }

// IPv6 communication capability
// IPv4-mapped IPv6 address communication capability
        slice<probe_type> probes = new probe_type[]{
            new(laddr: new TCPAddr(IP: ParseIP("::1"u8)), value: 1),
            new(laddr: new TCPAddr(IP: IPv4(127, 0, 0, 1)), value: 0)
        }.slice();
        var exprᴛ3 = runtime.GOOS;
        if (exprᴛ3 == "dragonfly"u8 || exprᴛ3 == "openbsd"u8) {
            probes = probes[..1];
        }

        // The latest DragonFly BSD and OpenBSD kernels don't
        // support IPV6_V6ONLY=0. They always return an error
        // and we don't need to probe the capability.
        foreach (var (i, _) in probes) {
            var (sΔ1, errΔ1) = sysSocket(syscall.AF_INET6, syscall.SOCK_STREAM, syscall.IPPROTO_TCP);
            if (errΔ1 != default!) {
                continue;
            }
            defer(poll.CloseFunc, sΔ1, ref ᒐ);
            syscall.SetsockoptInt(sΔ1, syscall.IPPROTO_IPV6, syscall.IPV6_V6ONLY, probes[i].value);
            (var sa, errΔ1) = Ꮡ(probes, i).of(probe_type.Ꮡladdr).sockaddr(syscall.AF_INET6);
            if (errΔ1 != default!) {
                continue;
            }
            {
                var errΔ2 = syscall.Bind(sΔ1, sa); if (errΔ2 != default!) {
                    continue;
                }
            }
            if (i == 0){
                p.ipv6Enabled = true;
            } else {
                p.ipv4MappedIPv6Enabled = true;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// favoriteAddrFamily returns the appropriate address family for the
// given network, laddr, raddr and mode.
//
// If mode indicates "listen" and laddr is a wildcard, we assume that
// the user wants to make a passive-open connection with a wildcard
// address family, both AF_INET and AF_INET6, and a wildcard address
// like the following:
//
//   - A listen for a wildcard communication domain, "tcp" or
//     "udp", with a wildcard address: If the platform supports
//     both IPv6 and IPv4-mapped IPv6 communication capabilities,
//     or does not support IPv4, we use a dual stack, AF_INET6 and
//     IPV6_V6ONLY=0, wildcard address listen. The dual stack
//     wildcard address listen may fall back to an IPv6-only,
//     AF_INET6 and IPV6_V6ONLY=1, wildcard address listen.
//     Otherwise we prefer an IPv4-only, AF_INET, wildcard address
//     listen.
//
//   - A listen for a wildcard communication domain, "tcp" or
//     "udp", with an IPv4 wildcard address: same as above.
//
//   - A listen for a wildcard communication domain, "tcp" or
//     "udp", with an IPv6 wildcard address: same as above.
//
//   - A listen for an IPv4 communication domain, "tcp4" or "udp4",
//     with an IPv4 wildcard address: We use an IPv4-only, AF_INET,
//     wildcard address listen.
//
//   - A listen for an IPv6 communication domain, "tcp6" or "udp6",
//     with an IPv6 wildcard address: We use an IPv6-only, AF_INET6
//     and IPV6_V6ONLY=1, wildcard address listen.
//
// Otherwise guess: If the addresses are IPv4 then returns AF_INET,
// or else returns AF_INET6. It also returns a boolean value what
// designates IPV6_V6ONLY option.
//
// Note that the latest DragonFly BSD and OpenBSD kernels allow
// neither "net.inet6.ip6.v6only=1" change nor IPPROTO_IPV6 level
// IPV6_V6ONLY socket option setting.
//
// favoriteAddrFamily should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/database64128/tfo-go/v2
//   - github.com/metacubex/tfo-go
//   - github.com/sagernet/tfo-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname favoriteAddrFamily
internal static (nint family, bool ipv6only) favoriteAddrFamily(@string network, Δsockaddr laddr, Δsockaddr raddr, @string mode) {
    switch (network[len(network) - 1]) {
    case (rune)'4': {
        return (syscall.AF_INET, false);
    }
    case (rune)'6': {
        return (syscall.AF_INET6, true);
    }}

    if (mode == "listen"u8 && (laddr == default! || laddr.isWildcard())) {
        if (supportsIPv4map() || !supportsIPv4()) {
            return (syscall.AF_INET6, false);
        }
        if (laddr == default!) {
            return (syscall.AF_INET, false);
        }
        return (laddr.family(), false);
    }
    if ((laddr == default! || laddr.family() == syscall.AF_INET) && (raddr == default! || raddr.family() == syscall.AF_INET)) {
        return (syscall.AF_INET, false);
    }
    return (syscall.AF_INET6, false);
}

internal static (ж<netFD> fd, error err) internetSocket(context.Context ctx, @string net, Δsockaddr laddr, Δsockaddr raddr, nint sotype, nint proto, @string mode, Func<context.Context, @string, @string, syscall.RawConn, error> ctrlCtxFn) {
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "aix"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        if (mode == "dial"u8 && raddr.isWildcard()) {
            raddr = raddr.toLocal(net);
        }
    }

    var (family, ipv6only) = favoriteAddrFamily(net, laddr, raddr, mode);
    return socket(ctx, net, family, sotype, proto, ipv6only, laddr, raddr, ctrlCtxFn);
}

internal static (syscall.SockaddrInet4, error) ipToSockaddrInet4(IP ip, nint port) {
    if (len(ip) == 0) {
        ip = IPv4zero;
    }
    var ip4 = ip.To4();
    if (ip4 == default!) {
        return (new syscall.SockaddrInet4(nil), new AddrErrorжerror(Ꮡ(new AddrError(Err: "non-IPv4 address"u8, Addr: ip.String()))));
    }
    var sa = new syscall.SockaddrInet4(Port: port);
    copy(sa.Addr[..], ip4);
    return (sa.ΔClone(), default!);
}

internal static (syscall.SockaddrInet6, error) ipToSockaddrInet6(IP ip, nint port, @string zone) {
    // In general, an IP wildcard address, which is either
    // "0.0.0.0" or "::", means the entire IP addressing
    // space. For some historical reason, it is used to
    // specify "any available address" on some operations
    // of IP node.
    //
    // When the IP node supports IPv4-mapped IPv6 address,
    // we allow a listener to listen to the wildcard
    // address of both IP addressing spaces by specifying
    // IPv6 wildcard address.
    if (len(ip) == 0 || ip.Equal(IPv4zero)) {
        ip = IPv6zero;
    }
    // We accept any IPv6 address including IPv4-mapped
    // IPv6 address.
    var ip6 = ip.To16();
    if (ip6 == default!) {
        return (new syscall.SockaddrInet6(nil), new AddrErrorжerror(Ꮡ(new AddrError(Err: "non-IPv6 address"u8, Addr: ip.String()))));
    }
    var sa = new syscall.SockaddrInet6(Port: port, ZoneId: (uint32)zoneCache.index(zone));
    copy(sa.Addr[..], ip6);
    return (sa.ΔClone(), default!);
}

// ipToSockaddr should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/database64128/tfo-go/v2
//   - github.com/metacubex/tfo-go
//   - github.com/sagernet/tfo-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname ipToSockaddr
internal static (syscall.Sockaddr, error) ipToSockaddr(nint family, IP ip, nint port, @string zone) {
    var exprᴛ1 = family;
    if (exprᴛ1 == syscall.AF_INET) {
        ref var sa = ref heap<syscall.SockaddrInet4>(out var Ꮡsa);
        (sa, var err) = ipToSockaddrInet4(ip, port);
        if (err != default!) {
            return (default!, err);
        }
        return (new syscall.SockaddrInet4жSockaddr(Ꮡsa), default!);
    }
    if (exprᴛ1 == syscall.AF_INET6) {
        ref var sa = ref heap<syscall.SockaddrInet6>(out var Ꮡsa);
        (sa, var err) = ipToSockaddrInet6(ip, port, zone);
        if (err != default!) {
            return (default!, err);
        }
        return (new syscall.SockaddrInet6жSockaddr(Ꮡsa), default!);
    }

    return (default!, new AddrErrorжerror(Ꮡ(new AddrError(Err: "invalid address family"u8, Addr: ip.String()))));
}

internal static (syscall.SockaddrInet4, error) addrPortToSockaddrInet4(netip.AddrPort ap) {
    // ipToSockaddrInet4 has special handling here for zero length slices.
    // We do not, because netip has no concept of a generic zero IP address.
    //
    // addr is allowed to be an IPv4-mapped IPv6 address.
    // As4 will unmap it to an IPv4 address.
    // The error message is kept consistent with ipToSockaddrInet4.
    var addr = ap.Addr();
    if (!addr.Is4() && !addr.Is4In6()) {
        return (new syscall.SockaddrInet4(nil), new AddrErrorжerror(Ꮡ(new AddrError(Err: "non-IPv4 address"u8, Addr: addr.String()))));
    }
    var sa = new syscall.SockaddrInet4(
        Addr: addr.As4(),
        Port: (nint)ap.Port()
    );
    return (sa.ΔClone(), default!);
}

internal static (syscall.SockaddrInet6, error) addrPortToSockaddrInet6(netip.AddrPort ap) {
    // ipToSockaddrInet6 has special handling here for zero length slices.
    // We do not, because netip has no concept of a generic zero IP address.
    //
    // addr is allowed to be an IPv4 address, because As16 will convert it
    // to an IPv4-mapped IPv6 address.
    // The error message is kept consistent with ipToSockaddrInet6.
    var addr = ap.Addr();
    if (!addr.IsValid()) {
        return (new syscall.SockaddrInet6(nil), new AddrErrorжerror(Ꮡ(new AddrError(Err: "non-IPv6 address"u8, Addr: addr.String()))));
    }
    var sa = new syscall.SockaddrInet6(
        Addr: addr.As16(),
        Port: (nint)ap.Port(),
        ZoneId: (uint32)zoneCache.index(addr.Zone())
    );
    return (sa.ΔClone(), default!);
}

} // end net_package
