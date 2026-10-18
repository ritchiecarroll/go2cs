// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go.@internal;

using netip = net.netip_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using net;

partial class routebsd_package {

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

// An InetAddr represent an internet address using IPv4 or IPv6.
partial struct InetAddr {
    public netipꓸAddr IP;
}

public static nint Family(this ref InetAddr a) {
    if (a.IP.Is4()){
        return syscall.AF_INET;
    } else {
        return syscall.AF_INET6;
    }
}

// parseInetAddr parses b as an internet address for IPv4 or IPv6.
internal static (Addr, error) parseInetAddr(nint af, slice<byte> b) {
    UntypedInt off4 = 4; // offset of in_addr
    UntypedInt off6 = 8; // offset of in6_addr
    UntypedInt ipv4Len = 4; // length of IPv4 address in bytes
    UntypedInt ipv6Len = 16; // length of IPv6 address in bytes
    var exprᴛ1 = af;
    if (exprᴛ1 == syscall.AF_INET) {
        if (len(b) < (nint)(off4 + 1) || len(b) < (nint)b[0]) {
            return (default!, errInvalidAddr);
        }
        nint sockAddrLen = (nint)b[0];
        array<byte> ip = new(4); /* ipv4Len */
        if (sockAddrLen != 0) {
            // Calculate how many bytes of the address to copy:
            // either full IPv4 length or the available length.
            nint n = off4 + ipv4Len;
            if (sockAddrLen < n) {
                n = sockAddrLen;
            }
            copy(ip[..], b.slice(off4, n));
        }
        var a = Ꮡ(new InetAddr(
            IP: netip.AddrFrom4(ip)
        ));
        return (new InetAddrжAddr(a), default!);
    }
    if (exprᴛ1 == syscall.AF_INET6) {
        if (len(b) < (nint)(off6 + 1) || len(b) < (nint)b[0]) {
            return (default!, errInvalidAddr);
        }
        array<byte> ip = new(16); /* ipv6Len */
        nint sockAddrLen = (nint)b[0];
        if (sockAddrLen != 0) {
            nint n = off6 + ipv6Len;
            if (sockAddrLen < n) {
                n = sockAddrLen;
            }
            copy(ip[..], b.slice(off6, n));
            if (ip[0] == 0xfe && (byte)(ip[1] & 0xc0) == 0x80 || ip[0] == 0xff && ((byte)(ip[1] & 0x0f) == 0x01 || (byte)(ip[1] & 0x0f) == 0x02)) {
                // KAME based IPv6 protocol stack usually
                // embeds the interface index in the
                // interface-local or link-local address as
                // the kernel-internal form.
                nint id = (nint)bigEndian.Uint16(ip[2..4]);
                if (id != 0) {
                    (ip[2], ip[3]) = (0, 0);
                }
            }
        }
        // The kernel can provide an integer zone ID.
        // We ignore it.
        var a = Ꮡ(new InetAddr(
            IP: netip.AddrFrom16(ip)
        ));
        return (new InetAddrжAddr(a), default!);
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
    if (Δruntime.GOOS == "darwin"u8 || Δruntime.GOOS == "ios"u8){
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
    case {} when b[0] == syscall.SizeofSockaddrInet6: {
        var a = Ꮡ(new InetAddr(
            IP: netip.AddrFrom16(new array<byte>(b[(int)(off6)..(int)(off6 + 16)], 16))
        ));
        return ((nint)b[0], new InetAddrжAddr(a), default!);
    }
    case {} when af == syscall.AF_INET6: {
        array<byte> ab = new(16);
        if (l - 1 < off6){
            copy(ab[..], b.slice(1, l));
        } else {
            copy(ab[..], b.slice(l - (nint)off6, l));
        }
        var a = Ꮡ(new InetAddr(
            IP: netip.AddrFrom16(ab)
        ));
        return ((nint)b[0], new InetAddrжAddr(a), default!);
    }
    case {} when b[0] == syscall.SizeofSockaddrInet4: {
        var a = Ꮡ(new InetAddr(
            IP: netip.AddrFrom4(new array<byte>(b[(int)(off4)..(int)(off4 + 4)], 4))
        ));
        return ((nint)b[0], new InetAddrжAddr(a), default!);
    }
    default: { // an old fashion, AF_UNSPEC or unknown means AF_INET
        array<byte> ab = new(4);
        if (l - 1 < off4){
            copy(ab[..], b.slice(1, l));
        } else {
            copy(ab[..], b.slice(l - (nint)off4, l));
        }
        var a = Ꮡ(new InetAddr(
            IP: netip.AddrFrom4(ab)
        ));
        return ((nint)b[0], new InetAddrжAddr(a), default!);
    }}
}

internal static (slice<Addr>, error) parseAddrs(nuint attrs, slice<byte> b) {
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
                af = (nint)b[1];
                var (a, err) = parseInetAddr(af, b);
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
            else { /* default: */
                var (l, a, err) = parseKernelInetAddr(af, b);
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
            // Skip unknown addresses.
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

} // end routebsd_package
