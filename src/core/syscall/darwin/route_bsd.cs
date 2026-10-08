// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go;

using runtime = runtime_package;
using @unsafe = unsafe_package;

partial class syscall_package {

internal static @string freebsdConfArch; // "machine $arch" line in kern.conftxt on freebsd
internal static nint minRoutingSockaddrLen = rsaAlignOf(0);

// Round the length of a raw sockaddr up to align it properly.
internal static nint rsaAlignOf(nint salen) {
    nint salign = sizeofPtr;
    if (darwin64Bit){
        // Darwin kernels require 32-bit aligned access to
        // routing facilities.
        salign = 4;
    } else 
    if (netbsd32Bit){
        // NetBSD 6 and beyond kernels require 64-bit aligned
        // access to routing facilities.
        salign = 8;
    } else 
    if (runtime.GOOS == "freebsd"u8) {
        // In the case of kern.supported_archs="amd64 i386",
        // we need to know the underlying kernel's
        // architecture because the alignment for routing
        // facilities are set at the build time of the kernel.
        if (freebsdConfArch == "amd64"u8) {
            salign = 8;
        }
    }
    if (salen == 0) {
        return salign;
    }
    return (nint)((salen + salign - 1) & ~(salign - 1));
}

// parseSockaddrLink parses b as a datalink socket address.
internal static (ж<SockaddrDatalink>, error) parseSockaddrLink(slice<byte> b) {
    if (len(b) < 8) {
        return (default!, EINVAL);
    }
    var (sa, _, err) = parseLinkLayerAddr(b[4..]);
    if (err != default!) {
        return (default!, err);
    }
    var rsa = Ꮡ(b, 0).Reinterpret<byte, RawSockaddrDatalink>();
    sa.Value.Len = rsa.Value.Len;
    sa.Value.Family = rsa.Value.Family;
    sa.Value.Index = rsa.Value.Index;
    return (sa, default!);
}

// The encoding looks like the following:
// +----------------------------+
// | Type             (1 octet) |
// +----------------------------+
// | Name length      (1 octet) |
// +----------------------------+
// | Address length   (1 octet) |
// +----------------------------+
// | Selector length  (1 octet) |
// +----------------------------+
// | Data            (variable) |
// +----------------------------+
internal partial struct parseLinkLayerAddr_linkLayerAddr /*dyn*/ {
    public byte Type;
    public byte Nlen;
    public byte Alen;
    public byte Slen;
}

// parseLinkLayerAddr parses b as a datalink socket address in
// conventional BSD kernel form.
internal static (ж<SockaddrDatalink>, nint, error) parseLinkLayerAddr(slice<byte> b) {
    var lla = Ꮡ(b, 0).Reinterpret<byte, parseLinkLayerAddr_linkLayerAddr>();
    nint l = 4 + (nint)(~lla).Nlen + (nint)(~lla).Alen + (nint)(~lla).Slen;
    if (len(b) < l) {
        return (default!, 0, EINVAL);
    }
    b = b[4..];
    var sa = Ꮡ(new SockaddrDatalink(Type: (~lla).Type, Nlen: (~lla).Nlen, Alen: (~lla).Alen, Slen: (~lla).Slen));
    for (nint i = 0; len((~sa).Data) > i && i < l - 4; i++) {
        sa.Value.Data[i] = (int8)b[i];
    }
    return (sa, rsaAlignOf(l), default!);
}

// parseSockaddrInet parses b as an internet socket address.
internal static (Sockaddr, error) parseSockaddrInet(slice<byte> b, byte family) {
    var exprᴛ1 = family;
    if (exprᴛ1 == AF_INET) {
        if (len(b) < SizeofSockaddrInet4) {
            return (default!, EINVAL);
        }
        var rsa = Ꮡ(b, 0).Reinterpret<byte, RawSockaddrAny>();
        return anyToSockaddr(rsa);
    }
    if (exprᴛ1 == AF_INET6) {
        if (len(b) < SizeofSockaddrInet6) {
            return (default!, EINVAL);
        }
        var rsa = Ꮡ(b, 0).Reinterpret<byte, RawSockaddrAny>();
        return anyToSockaddr(rsa);
    }
    { /* default: */
        return (default!, EINVAL);
    }

}

internal const nint offsetofInet4 = /* int(unsafe.Offsetof(RawSockaddrInet4{}.Addr)) */ 4;
internal const nint offsetofInet6 = /* int(unsafe.Offsetof(RawSockaddrInet6{}.Addr)) */ 8;

// parseNetworkLayerAddr parses b as an internet socket address in
// conventional BSD kernel form.
internal static (Sockaddr, error) parseNetworkLayerAddr(slice<byte> b, byte family) {
    // The encoding looks similar to the NLRI encoding.
    // +----------------------------+
    // | Length           (1 octet) |
    // +----------------------------+
    // | Address prefix  (variable) |
    // +----------------------------+
    //
    // The differences between the kernel form and the NLRI
    // encoding are:
    //
    // - The length field of the kernel form indicates the prefix
    //   length in bytes, not in bits
    //
    // - In the kernel form, zero value of the length field
    //   doesn't mean 0.0.0.0/0 or ::/0
    //
    // - The kernel form appends leading bytes to the prefix field
    //   to make the <length, prefix> tuple to be conformed with
    //   the routing message boundary
    nint l = (nint)rsaAlignOf((nint)b[0]);
    if (len(b) < l) {
        return (default!, EINVAL);
    }
    // Don't reorder case expressions.
    // The case expressions for IPv6 must come first.
    switch (ᐧ) {
    case {} when b[0] == SizeofSockaddrInet6: {
        var sa = Ꮡ(new SockaddrInet6(nil));
        copy((~sa).Addr[..], b[(int)(offsetofInet6)..]);
        return (new SockaddrInet6жSockaddr(sa), default!);
    }
    case {} when family == AF_INET6: {
        var sa = Ꮡ(new SockaddrInet6(nil));
        if (l - 1 < offsetofInet6){
            copy((~sa).Addr[..], b.slice(1, l));
        } else {
            copy((~sa).Addr[..], b.slice(l - offsetofInet6, l));
        }
        return (new SockaddrInet6жSockaddr(sa), default!);
    }
    case {} when b[0] == SizeofSockaddrInet4: {
        var sa = Ꮡ(new SockaddrInet4(nil));
        copy((~sa).Addr[..], b[(int)(offsetofInet4)..]);
        return (new SockaddrInet4жSockaddr(sa), default!);
    }
    default: {
        var sa = Ꮡ(new SockaddrInet4(nil));
        if (l - 1 < offsetofInet4){
            // an old fashion, AF_UNSPEC or unknown means AF_INET
            copy((~sa).Addr[..], b.slice(1, l));
        } else {
            copy((~sa).Addr[..], b.slice(l - offsetofInet4, l));
        }
        return (new SockaddrInet4жSockaddr(sa), default!);
    }}

}

// RouteRIB returns routing information base, as known as RIB,
// which consists of network facility information, states and
// parameters.
//
// Deprecated: Use golang.org/x/net/route instead.
public static (slice<byte>, error) RouteRIB(nint facility, nint param) {
    var mib = new _C_int[]{CTL_NET, AF_ROUTE, 0, 0, ((_C_int)(int32)facility), ((_C_int)(int32)param)}.slice();
    // Find size.
    ref var n = ref heap<uintptr>(out var Ꮡn);
    n = (uintptr)0;
    {
        var err = sysctl(mib, nil, Ꮡn, nil, 0); if (err != default!) {
            return (default!, err);
        }
    }
    if (n == 0) {
        return (default!, default!);
    }
    var tab = new slice<byte>((nint)(n));
    {
        var err = sysctl(mib, Ꮡ(tab, 0), Ꮡn, nil, 0); if (err != default!) {
            return (default!, err);
        }
    }
    return (tab.slice(0, (nint)(n)), default!);
}

// RoutingMessage represents a routing message.
//
// Deprecated: Use golang.org/x/net/route instead.
partial interface RoutingMessage {
    (slice<Sockaddr>, error) sockaddr();
}

internal const nint anyMessageLen = /* int(unsafe.Sizeof(anyMessage{})) */ 4;

partial struct anyMessage {
    public uint16 Msglen;
    public uint8 Version;
    public uint8 Type;
}

// RouteMessage represents a routing message containing routing
// entries.
//
// Deprecated: Use golang.org/x/net/route instead.
partial struct RouteMessage {
    public RtMsghdr Header;
    public slice<byte> Data;
}

internal static (slice<Sockaddr>, error) sockaddr(this ref RouteMessage m) {
    array<Sockaddr> sas = new(8); /* RTAX_MAX */
    var b = m.Data[..];
    var family = (uint8)AF_UNSPEC;
    for (nuint i = (nuint)0; i < RTAX_MAX && len(b) >= minRoutingSockaddrLen; i++) {
        if ((int32)(m.Header.Addrs & (((int32)1).Lsh(i))) == 0) {
            continue;
        }
        var rsa = Ꮡ(b, 0).Reinterpret<byte, RawSockaddr>();
        var exprᴛ1 = (~rsa).Family;
        if (exprᴛ1 == AF_LINK) {
            var (sa, err) = parseSockaddrLink(b);
            if (err != default!) {
                return (default!, err);
            }
            sas[i] = new SockaddrDatalinkжSockaddr(sa);
            b = b.slice(rsaAlignOf((nint)(~rsa).Len));
        }
        else if (exprᴛ1 == AF_INET || exprᴛ1 == AF_INET6) {
            var (sa, err) = parseSockaddrInet(b, (~rsa).Family);
            if (err != default!) {
                return (default!, err);
            }
            sas[i] = sa;
            b = b.slice(rsaAlignOf((nint)(~rsa).Len));
            family = rsa.Value.Family;
        }
        else { /* default: */
            var (sa, err) = parseNetworkLayerAddr(b, family);
            if (err != default!) {
                return (default!, err);
            }
            sas[i] = sa;
            b = b.slice(rsaAlignOf((nint)b[0]));
        }

    }
    return (sas[..], default!);
}

// InterfaceMessage represents a routing message containing
// network interface entries.
//
// Deprecated: Use golang.org/x/net/route instead.
partial struct InterfaceMessage {
    public IfMsghdr Header;
    public slice<byte> Data;
}

internal static (slice<Sockaddr>, error) sockaddr(this ref InterfaceMessage m) {
    array<Sockaddr> sas = new(8); /* RTAX_MAX */
    if ((int32)(m.Header.Addrs & (int32)RTA_IFP) == 0) {
        return (default!, default!);
    }
    var (sa, err) = parseSockaddrLink(m.Data[..]);
    if (err != default!) {
        return (default!, err);
    }
    sas[RTAX_IFP] = new SockaddrDatalinkжSockaddr(sa);
    return (sas[..], default!);
}

// InterfaceAddrMessage represents a routing message containing
// network interface address entries.
//
// Deprecated: Use golang.org/x/net/route instead.
partial struct InterfaceAddrMessage {
    public IfaMsghdr Header;
    public slice<byte> Data;
}

internal static (slice<Sockaddr>, error) sockaddr(this ref InterfaceAddrMessage m) {
    array<Sockaddr> sas = new(8); /* RTAX_MAX */
    var b = m.Data[..];
    var family = (uint8)AF_UNSPEC;
    for (nuint i = (nuint)0; i < RTAX_MAX && len(b) >= minRoutingSockaddrLen; i++) {
        if ((int32)(m.Header.Addrs & (((int32)1).Lsh(i))) == 0) {
            continue;
        }
        var rsa = Ꮡ(b, 0).Reinterpret<byte, RawSockaddr>();
        var exprᴛ1 = (~rsa).Family;
        if (exprᴛ1 == AF_LINK) {
            var (sa, err) = parseSockaddrLink(b);
            if (err != default!) {
                return (default!, err);
            }
            sas[i] = new SockaddrDatalinkжSockaddr(sa);
            b = b.slice(rsaAlignOf((nint)(~rsa).Len));
        }
        else if (exprᴛ1 == AF_INET || exprᴛ1 == AF_INET6) {
            var (sa, err) = parseSockaddrInet(b, (~rsa).Family);
            if (err != default!) {
                return (default!, err);
            }
            sas[i] = sa;
            b = b.slice(rsaAlignOf((nint)(~rsa).Len));
            family = rsa.Value.Family;
        }
        else { /* default: */
            var (sa, err) = parseNetworkLayerAddr(b, family);
            if (err != default!) {
                return (default!, err);
            }
            sas[i] = sa;
            b = b.slice(rsaAlignOf((nint)b[0]));
        }

    }
    return (sas[..], default!);
}

// ParseRoutingMessage parses b as routing messages and returns the
// slice containing the [RoutingMessage] interfaces.
//
// Deprecated: Use golang.org/x/net/route instead.
public static (slice<RoutingMessage> msgs, error err) ParseRoutingMessage(slice<byte> b) {
    slice<RoutingMessage> msgs = default!;

    nint nmsgs = 0;
    nint nskips = 0;
    while (len(b) >= anyMessageLen) {
        nmsgs++;
        var any = Ꮡ(b, 0).Reinterpret<byte, anyMessage>();
        if ((~any).Version != RTM_VERSION) {
            b = b.slice((~any).Msglen);
            continue;
        }
        {
            var m = any.toRoutingMessage(b); if (m == default!){
                nskips++;
            } else {
                msgs = append(msgs, m);
            }
        }
        b = b.slice((~any).Msglen);
    }
    // We failed to parse any of the messages - version mismatch?
    if (nmsgs != len(msgs) + nskips) {
        return (default!, EINVAL);
    }
    return (msgs, default!);
}

// ParseRoutingSockaddr parses msg's payload as raw sockaddrs and
// returns the slice containing the [Sockaddr] interfaces.
//
// Deprecated: Use golang.org/x/net/route instead.
public static (slice<Sockaddr>, error) ParseRoutingSockaddr(RoutingMessage msg) {
    var (sas, err) = msg.sockaddr();
    if (err != default!) {
        return (default!, err);
    }
    return (sas, default!);
}

} // end syscall_package
