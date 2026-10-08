// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go.vendor.golang.org.x.net;

using runtime = runtime_package;
using syscall = syscall_package;

partial class route_package {

// An Addr represents an address associated with packet routing.
partial interface Addr {
    // Family returns an address family.
    nint Family();
}

// A LinkAddr represents a link-layer address.
partial struct LinkAddr {
    public nint Index;   // interface index when attached
    public @string Name; // interface name when attached
    public slice<byte> Addr; // link-layer address when attached
}

// Family implements the Family method of Addr interface.
public static nint Family(this ref LinkAddr a) {
    return syscall.AF_LINK;
}

internal static (nint, nint) lenAndSpace(this ref LinkAddr a) {
    nint l = 8 + len(a.Name) + len(a.Addr);
    return (l, roundup(l));
}

internal static (nint, error) marshal(this ref LinkAddr a, slice<byte> b) {
    var (l, ll) = a.lenAndSpace();
    if (len(b) < ll) {
        return (0, errShortBuffer);
    }
    nint nlen = len(a.Name);
    nint alen = len(a.Addr);
    if (nlen > 255 || alen > 255) {
        return (0, errInvalidAddr);
    }
    b[0] = (byte)l;
    b[1] = syscall.AF_LINK;
    if (a.Index > 0) {
        nativeEndian.PutUint16(b[2..4], (uint16)a.Index);
    }
    var data = b[8..];
    if (nlen > 0) {
        b[5] = (byte)nlen;
        copy(data.slice(0, nlen), a.Name);
        data = data.slice(nlen);
    }
    if (alen > 0) {
        b[6] = (byte)alen;
        copy(data.slice(0, alen), a.Addr);
        data = data.slice(alen);
    }
    return (ll, default!);
}

internal static (Addr, error) parseLinkAddr(slice<byte> b) {
    if (len(b) < 8) {
        return (default!, errInvalidAddr);
    }
    var (_, a, err) = parseKernelLinkAddr(syscall.AF_LINK, b[4..]);
    if (err != default!) {
        return (default!, err);
    }
    a._<ж<LinkAddr>>().Value.Index = (nint)nativeEndian.Uint16(b[2..4]);
    return (a, default!);
}

// parseKernelLinkAddr parses b as a link-layer address in
// conventional BSD kernel form.
internal static (nint, Addr, error) parseKernelLinkAddr(nint _, slice<byte> b) {
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
    //
    // On some platforms, all-bit-one of length field means "don't
    // care".
    nint nlen = (nint)b[1];
    nint alen = (nint)b[2];
    nint slen = (nint)b[3];
    if (nlen == 0xff) {
        nlen = 0;
    }
    if (alen == 0xff) {
        alen = 0;
    }
    if (slen == 0xff) {
        slen = 0;
    }
    nint l = 4 + nlen + alen + slen;
    if (len(b) < l) {
        return (0, default!, errInvalidAddr);
    }
    var data = b[4..];
    ref var name = ref heap(new @string(), out var Ꮡname);
    slice<byte> addr = default!;
    if (nlen > 0) {
        name = ((@string)(data.slice(0, nlen)));
        data = data.slice(nlen);
    }
    if (alen > 0) {
        addr = data.slice(0, alen);
        data = data.slice(alen);
    }
    return (l, new LinkAddrжAddr(Ꮡ(new LinkAddr(Name: name, Addr: addr))), default!);
}

// An Inet4Addr represents an internet address for IPv4.
partial struct Inet4Addr {
    public array<byte> IP = new(4); // IP address
}

// Family implements the Family method of Addr interface.
public static nint Family(this ref Inet4Addr a) {
    return syscall.AF_INET;
}

internal static (nint, nint) lenAndSpace(this ref Inet4Addr a) {
    return (sizeofSockaddrInet, roundup(sizeofSockaddrInet));
}

internal static (nint, error) marshal(this ref Inet4Addr a, slice<byte> b) {
    var (l, ll) = a.lenAndSpace();
    if (len(b) < ll) {
        return (0, errShortBuffer);
    }
    b[0] = (byte)l;
    b[1] = syscall.AF_INET;
    copy(b[4..8], a.IP[..]);
    return (ll, default!);
}

// An Inet6Addr represents an internet address for IPv6.
partial struct Inet6Addr {
    public array<byte> IP = new(16); // IP address
    public nint ZoneID;     // zone identifier
}

// Family implements the Family method of Addr interface.
public static nint Family(this ref Inet6Addr a) {
    return syscall.AF_INET6;
}

internal static (nint, nint) lenAndSpace(this ref Inet6Addr a) {
    return (sizeofSockaddrInet6, roundup(sizeofSockaddrInet6));
}

internal static (nint, error) marshal(this ref Inet6Addr a, slice<byte> b) {
    var (l, ll) = a.lenAndSpace();
    if (len(b) < ll) {
        return (0, errShortBuffer);
    }
    b[0] = (byte)l;
    b[1] = syscall.AF_INET6;
    copy(b[8..24], a.IP[..]);
    if (a.ZoneID > 0) {
        nativeEndian.PutUint32(b[24..28], (uint32)a.ZoneID);
    }
    return (ll, default!);
}

// parseInetAddr parses b as an internet address for IPv4 or IPv6.
internal static (Addr, error) parseInetAddr(nint af, slice<byte> b) {
    UntypedInt off4 = 4; // offset of in_addr
    UntypedInt off6 = 8; // offset of in6_addr
    var exprᴛ1 = af;
    if (exprᴛ1 == syscall.AF_INET) {
        if (len(b) < (nint)(off4 + 1) || len(b) < (nint)b[0] || b[0] == 0) {
            return (default!, errInvalidAddr);
        }
        nint sockAddrLen = (nint)b[0];
        var a = Ꮡ(new Inet4Addr(nil));
        nint n = off4 + 4;
        if (sockAddrLen < n) {
            n = sockAddrLen;
        }
        copy((~a).IP[..], b.slice(off4, n));
        return (new Inet4AddrжAddr(a), default!);
    }
    if (exprᴛ1 == syscall.AF_INET6) {
        if (len(b) < (nint)(off6 + 1) || len(b) < (nint)b[0] || b[0] == 0) {
            return (default!, errInvalidAddr);
        }
        nint sockAddrLen = (nint)b[0];
        nint n = off6 + 16;
        if (sockAddrLen < n) {
            n = sockAddrLen;
        }
        var a = Ꮡ(new Inet6Addr(nil));
        if (sockAddrLen == sizeofSockaddrInet6) {
            a.Value.ZoneID = (nint)nativeEndian.Uint32(b[24..28]);
        }
        copy((~a).IP[..], b.slice(off6, n));
        if ((~a).IP[0] == 0xfe && (byte)((~a).IP[1] & 0xc0) == 0x80 || (~a).IP[0] == 0xff && ((byte)((~a).IP[1] & 0x0f) == 0x01 || (byte)((~a).IP[1] & 0x0f) == 0x02)) {
            // KAME based IPv6 protocol stack usually
            // embeds the interface index in the
            // interface-local or link-local address as
            // the kernel-internal form.
            nint id = (nint)bigEndian.Uint16((~a).IP[2..4]);
            if (id != 0) {
                a.Value.ZoneID = id;
                (a.Value.IP[2], a.Value.IP[3]) = (0, 0);
            }
        }
        return (new Inet6AddrжAddr(a), default!);
    }
    { /* default: */
        return (default!, errInvalidAddr);
    }

}

// parseKernelInetAddr parses b as an internet address in conventional
// BSD kernel form.
internal static (nint, Addr, error) parseKernelInetAddr(nint af, slice<byte> b) {
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
    nint l = (nint)b[0];
    if (runtime.GOOS == "darwin"u8 || runtime.GOOS == "ios"u8){
        // On Darwin, an address in the kernel form is also
        // used as a message filler.
        if (l == 0 || len(b) > roundup(l)) {
            l = roundup(l);
        }
    } else {
        l = roundup(l);
    }
    if (len(b) < l) {
        return (0, default!, errInvalidAddr);
    }
    // Don't reorder case expressions.
    // The case expressions for IPv6 must come first.
    UntypedInt off4 = 4; // offset of in_addr
    
    UntypedInt off6 = 8; // offset of in6_addr
    switch (ᐧ) {
    case {} when b[0] == sizeofSockaddrInet6: {
        var a = Ꮡ(new Inet6Addr(nil));
        copy((~a).IP[..], b[(int)(off6)..(int)(off6 + 16)]);
        return ((nint)b[0], new Inet6AddrжAddr(a), default!);
    }
    case {} when af == syscall.AF_INET6: {
        var a = Ꮡ(new Inet6Addr(nil));
        if (l - 1 < off6){
            copy((~a).IP[..], b.slice(1, l));
        } else {
            copy((~a).IP[..], b.slice(l - (nint)off6, l));
        }
        return ((nint)b[0], new Inet6AddrжAddr(a), default!);
    }
    case {} when b[0] == sizeofSockaddrInet: {
        var a = Ꮡ(new Inet4Addr(nil));
        copy((~a).IP[..], b[(int)(off4)..(int)(off4 + 4)]);
        return ((nint)b[0], new Inet4AddrжAddr(a), default!);
    }
    default: {
        var a = Ꮡ(new Inet4Addr(nil));
        if (l - 1 < off4){
            // an old fashion, AF_UNSPEC or unknown means AF_INET
            copy((~a).IP[..], b.slice(1, l));
        } else {
            copy((~a).IP[..], b.slice(l - (nint)off4, l));
        }
        return ((nint)b[0], new Inet4AddrжAddr(a), default!);
    }}

}

// A DefaultAddr represents an address of various operating
// system-specific features.
partial struct DefaultAddr {
    internal nint af;
    public slice<byte> Raw; // raw format of address
}

// Family implements the Family method of Addr interface.
public static nint Family(this ref DefaultAddr a) {
    return a.af;
}

internal static (nint, nint) lenAndSpace(this ref DefaultAddr a) {
    nint l = len(a.Raw);
    return (l, roundup(l));
}

internal static (nint, error) marshal(this ref DefaultAddr a, slice<byte> b) {
    var (l, ll) = a.lenAndSpace();
    if (len(b) < ll) {
        return (0, errShortBuffer);
    }
    if (l > 255) {
        return (0, errInvalidAddr);
    }
    b[1] = (byte)l;
    copy(b.slice(0, l), a.Raw);
    return (ll, default!);
}

internal static (Addr, error) parseDefaultAddr(slice<byte> b) {
    if (len(b) < 2 || len(b) < (nint)b[0]) {
        return (default!, errInvalidAddr);
    }
    var a = Ꮡ(new DefaultAddr(af: (nint)b[1], Raw: b.slice(0, b[0])));
    return (new DefaultAddrжAddr(a), default!);
}

internal static nint addrsSpace(slice<Addr> @as) {
    nint l = default!;
    foreach (var (_, a) in @as) {
        switch (a.type()) {
        case ж<LinkAddr> aΔ1: {
            var (_, ll) = aΔ1.lenAndSpace();
            l += ll;
            break;
        }
        case ж<Inet4Addr> aΔ1: {
            var (_, ll) = aΔ1.lenAndSpace();
            l += ll;
            break;
        }
        case ж<Inet6Addr> aΔ1: {
            var (_, ll) = aΔ1.lenAndSpace();
            l += ll;
            break;
        }
        case ж<DefaultAddr> aΔ1: {
            var (_, ll) = aΔ1.lenAndSpace();
            l += ll;
            break;
        }}
    }
    return l;
}

// marshalAddrs marshals as and returns a bitmap indicating which
// address is stored in b.
internal static (nuint, error) marshalAddrs(slice<byte> b, slice<Addr> @as) {
    nuint attrs = default!;
    foreach (var (i, a) in @as) {
        switch (a.type()) {
        case ж<LinkAddr> aΔ1: {
            var (l, err) = aΔ1.marshal(b);
            if (err != default!) {
                return (0, err);
            }
            b = b.slice(l);
            attrs |= (nuint)(((nuint)1).Lsh((nuint)i));
            break;
        }
        case ж<Inet4Addr> aΔ1: {
            var (l, err) = aΔ1.marshal(b);
            if (err != default!) {
                return (0, err);
            }
            b = b.slice(l);
            attrs |= (nuint)(((nuint)1).Lsh((nuint)i));
            break;
        }
        case ж<Inet6Addr> aΔ1: {
            var (l, err) = aΔ1.marshal(b);
            if (err != default!) {
                return (0, err);
            }
            b = b.slice(l);
            attrs |= (nuint)(((nuint)1).Lsh((nuint)i));
            break;
        }
        case ж<DefaultAddr> aΔ1: {
            var (l, err) = aΔ1.marshal(b);
            if (err != default!) {
                return (0, err);
            }
            b = b.slice(l);
            attrs |= (nuint)(((nuint)1).Lsh((nuint)i));
            break;
        }}
    }
    return (attrs, default!);
}

internal static (slice<Addr>, error) parseAddrs(nuint attrs, Func<nint, slice<byte>, (nint, Addr, error)> fn, slice<byte> b) {
    array<Addr> @as = new(8); /* syscall.RTAX_MAX */
    nint af = (nint)syscall.AF_UNSPEC;
    for (nuint i = (nuint)0; i < syscall.RTAX_MAX && len(b) >= roundup(0); i++) {
        if ((nuint)(attrs & (((nuint)1).Lsh(i))) == 0) {
            continue;
        }
        if (i <= syscall.RTAX_BRD){
            var exprᴛ1 = b[1];
            if (exprᴛ1 == syscall.AF_LINK) {
                var (a, err) = parseLinkAddr(b);
                if (err != default!) {
                    return (default!, err);
                }
                @as[i] = a;
                nint l = roundup((nint)b[0]);
                if (len(b) < l) {
                    return (default!, errMessageTooShort);
                }
                b = b.slice(l);
            }
            else if (exprᴛ1 == syscall.AF_INET || exprᴛ1 == syscall.AF_INET6) {
                if (b[0] > 0) {
                    // #70528: if the sockaddrlen is 0, no address to parse inside,
                    // skip over the record.
                    af = (nint)b[1];
                    var (a, err) = parseInetAddr(af, b);
                    if (err != default!) {
                        return (default!, err);
                    }
                    @as[i] = a;
                }
                nint l = roundup((nint)b[0]);
                if (len(b) < l) {
                    return (default!, errMessageTooShort);
                }
                b = b.slice(l);
            }
            else { /* default: */
                var (l, a, err) = fn(af, b);
                if (err != default!) {
                    return (default!, err);
                }
                @as[i] = a;
                nint ll = roundup(l);
                if (len(b) < ll){
                    b = b.slice(l);
                } else {
                    b = b.slice(ll);
                }
            }

        } else {
            var (a, err) = parseDefaultAddr(b);
            if (err != default!) {
                return (default!, err);
            }
            @as[i] = a;
            nint l = roundup((nint)b[0]);
            if (len(b) < l) {
                return (default!, errMessageTooShort);
            }
            b = b.slice(l);
        }
    }
    // The only remaining bytes in b should be alignment.
    // However, under some circumstances DragonFly BSD appears to put
    // more addresses in the message than are indicated in the address
    // bitmask, so don't check for this.
    return (@as[..], default!);
}

} // end route_package
