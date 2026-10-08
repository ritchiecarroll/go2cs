// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// IP address manipulations
//
// IPv4 addresses are 4 bytes; IPv6 addresses are 16 bytes.
// An IPv4 address can be converted to an IPv6 address by
// adding a canonical prefix (10 zeros, 2 0xFFs).
// This library accepts either size of byte slice but always
// returns 16-byte addresses.
namespace go;

using bytealg = @internal.bytealg_package;
using itoa = @internal.itoa_package;
using stringslite = @internal.stringslite_package;
using netip = net.netip_package;
using @internal;
using net;

partial class net_package {

// IP address lengths (bytes).
public static UntypedInt IPv4len => 4;

public static UntypedInt IPv6len => 16;

partial struct IP /*[]byte*/;

partial struct IPMask /*[]byte*/;

// An IPNet represents an IP network.
partial struct IPNet {
    public IP IP;     // network number
    public IPMask Mask; // network mask
}

// IPv4 returns the IP address (in 16-byte form) of the
// IPv4 address a.b.c.d.
public static IP IPv4(byte a, byte b, byte c, byte d) {
    var p = new IP(IPv6len);
    copy(p, v4InV6Prefix);
    p[12] = a;
    p[13] = b;
    p[14] = c;
    p[15] = d;
    return p;
}

internal static slice<byte> v4InV6Prefix = new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xff, 0xff}.slice();

// IPv4Mask returns the IP mask (in 4-byte form) of the
// IPv4 mask a.b.c.d.
public static IPMask IPv4Mask(byte a, byte b, byte c, byte d) {
    var p = new IPMask(IPv4len);
    p[0] = a;
    p[1] = b;
    p[2] = c;
    p[3] = d;
    return p;
}

// CIDRMask returns an [IPMask] consisting of 'ones' 1 bits
// followed by 0s up to a total length of 'bits' bits.
// For a mask of this form, CIDRMask is the inverse of [IPMask.Size].
public static IPMask CIDRMask(nint ones, nint bits) {
    if (bits != (nint)(8 * IPv4len) && bits != (nint)(8 * IPv6len)) {
        return default!;
    }
    if (ones < 0 || ones > bits) {
        return default!;
    }
    nint l = bits / 8;
    var m = new IPMask(l);
    nuint n = (nuint)ones;
    for (nint i = 0; i < l; i++) {
        if (n >= 8) {
            m[i] = 0xff;
            n -= 8;
            continue;
        }
        m[i] = (byte)(((byte)(~(byte)(((byte)0xff).Rsh(n)))));
        n = 0;
    }
    return m;
}

// Well-known IPv4 addresses
public static IP IPv4bcast = IPv4(255, 255, 255, 255); // limited broadcast

public static IP IPv4allsys = IPv4(224, 0, 0, 1); // all systems

public static IP IPv4allrouter = IPv4(224, 0, 0, 2); // all routers

public static IP IPv4zero = IPv4(0, 0, 0, 0); // all zeros

// Well-known IPv6 addresses
public static IP IPv6zero = new IP(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice());

public static IP IPv6unspecified = new IP(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice());

public static IP IPv6loopback = new IP(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}.slice());

public static IP IPv6interfacelocalallnodes = new IP(new byte[]{0xff, 0x01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x01}.slice());

public static IP IPv6linklocalallnodes = new IP(new byte[]{0xff, 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x01}.slice());

public static IP IPv6linklocalallrouters = new IP(new byte[]{0xff, 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x02}.slice());

// IsUnspecified reports whether ip is an unspecified address, either
// the IPv4 address "0.0.0.0" or the IPv6 address "::".
public static bool IsUnspecified(this IP ip) {
    return ip.Equal(IPv4zero) || ip.Equal(IPv6unspecified);
}

// IsLoopback reports whether ip is a loopback address.
public static bool IsLoopback(this IP ip) {
    {
        var ip4 = ip.To4(); if (ip4 != default!) {
            return ip4[0] == 127;
        }
    }
    return ip.Equal(IPv6loopback);
}

// IsPrivate reports whether ip is a private address, according to
// RFC 1918 (IPv4 addresses) and RFC 4193 (IPv6 addresses).
public static bool IsPrivate(this IP ip) {
    {
        var ip4 = ip.To4(); if (ip4 != default!) {
            // Following RFC 1918, Section 3. Private Address Space which says:
            //   The Internet Assigned Numbers Authority (IANA) has reserved the
            //   following three blocks of the IP address space for private internets:
            //     10.0.0.0        -   10.255.255.255  (10/8 prefix)
            //     172.16.0.0      -   172.31.255.255  (172.16/12 prefix)
            //     192.168.0.0     -   192.168.255.255 (192.168/16 prefix)
            return ip4[0] == 10 || (ip4[0] == 172 && (byte)(ip4[1] & 0xf0) == 16) || (ip4[0] == 192 && ip4[1] == 168);
        }
    }
    // Following RFC 4193, Section 8. IANA Considerations which says:
    //   The IANA has assigned the FC00::/7 prefix to "Unique Local Unicast".
    return len(ip) == IPv6len && (byte)(ip[0] & 0xfe) == 0xfc;
}

// IsMulticast reports whether ip is a multicast address.
public static bool IsMulticast(this IP ip) {
    {
        var ip4 = ip.To4(); if (ip4 != default!) {
            return (byte)(ip4[0] & 0xf0) == 0xe0;
        }
    }
    return len(ip) == IPv6len && ip[0] == 0xff;
}

// IsInterfaceLocalMulticast reports whether ip is
// an interface-local multicast address.
public static bool IsInterfaceLocalMulticast(this IP ip) {
    return len(ip) == IPv6len && ip[0] == 0xff && (byte)(ip[1] & 0x0f) == 0x01;
}

// IsLinkLocalMulticast reports whether ip is a link-local
// multicast address.
public static bool IsLinkLocalMulticast(this IP ip) {
    {
        var ip4 = ip.To4(); if (ip4 != default!) {
            return ip4[0] == 224 && ip4[1] == 0 && ip4[2] == 0;
        }
    }
    return len(ip) == IPv6len && ip[0] == 0xff && (byte)(ip[1] & 0x0f) == 0x02;
}

// IsLinkLocalUnicast reports whether ip is a link-local
// unicast address.
public static bool IsLinkLocalUnicast(this IP ip) {
    {
        var ip4 = ip.To4(); if (ip4 != default!) {
            return ip4[0] == 169 && ip4[1] == 254;
        }
    }
    return len(ip) == IPv6len && ip[0] == 0xfe && (byte)(ip[1] & 0xc0) == 0x80;
}

// IsGlobalUnicast reports whether ip is a global unicast
// address.
//
// The identification of global unicast addresses uses address type
// identification as defined in RFC 1122, RFC 4632 and RFC 4291 with
// the exception of IPv4 directed broadcast addresses.
// It returns true even if ip is in IPv4 private address space or
// local IPv6 unicast address space.
public static bool IsGlobalUnicast(this IP ip) {
    return (len(ip) == IPv4len || len(ip) == IPv6len) && !ip.Equal(IPv4bcast) && !ip.IsUnspecified() && !ip.IsLoopback() && !ip.IsMulticast() && !ip.IsLinkLocalUnicast();
}

// Is p all zeros?
internal static bool isZeros(IP p) {
    for (nint i = 0; i < len(p); i++) {
        if (p[i] != 0) {
            return false;
        }
    }
    return true;
}

// To4 converts the IPv4 address ip to a 4-byte representation.
// If ip is not an IPv4 address, To4 returns nil.
public static IP To4(this IP ip) {
    if (len(ip) == IPv4len) {
        return ip;
    }
    if (len(ip) == IPv6len && isZeros(ip[0..10]) && ip[10] == 0xff && ip[11] == 0xff) {
        return ip[12..16];
    }
    return default!;
}

// To16 converts the IP address ip to a 16-byte representation.
// If ip is not an IP address (it is the wrong length), To16 returns nil.
public static IP To16(this IP ip) {
    if (len(ip) == IPv4len) {
        return IPv4(ip[0], ip[1], ip[2], ip[3]);
    }
    if (len(ip) == IPv6len) {
        return ip;
    }
    return default!;
}

// Default route masks for IPv4.
internal static IPMask classAMask = IPv4Mask(0xff, 0, 0, 0);

internal static IPMask classBMask = IPv4Mask(0xff, 0xff, 0, 0);

internal static IPMask classCMask = IPv4Mask(0xff, 0xff, 0xff, 0);

// DefaultMask returns the default IP mask for the IP address ip.
// Only IPv4 addresses have default masks; DefaultMask returns
// nil if ip is not a valid IPv4 address.
public static IPMask DefaultMask(this IP ip) {
    {
        ip = ip.To4(); if (ip == default!) {
            return default!;
        }
    }
    switch (ᐧ) {
    case {} when ip[0] is < 0x80: {
        return classAMask;
    }
    case {} when ip[0] is < 0xC0: {
        return classBMask;
    }
    default: {
        return classCMask;
    }}

}

internal static bool allFF(slice<byte> b) {
    foreach (var (_, c) in b) {
        if (c != 0xff) {
            return false;
        }
    }
    return true;
}

// Mask returns the result of masking the IP address ip with mask.
public static IP Mask(this IP ip, IPMask mask) {
    if (len(mask) == IPv6len && len(ip) == IPv4len && allFF(mask[..12])) {
        mask = mask[12..];
    }
    if (len(mask) == IPv4len && len(ip) == IPv6len && bytealg.Equal(ip[..12], v4InV6Prefix)) {
        ip = ip[12..];
    }
    nint n = len(ip);
    if (n != len(mask)) {
        return default!;
    }
    var @out = new IP(n);
    for (nint i = 0; i < n; i++) {
        @out[i] = (byte)(ip[i] & mask[i]);
    }
    return @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "<nil>"u8;

// String returns the string form of the IP address ip.
// It returns one of 4 forms:
//   - "<nil>", if ip has length 0
//   - dotted decimal ("192.0.2.1"), if ip is an IPv4 or IP4-mapped IPv6 address
//   - IPv6 conforming to RFC 5952 ("2001:db8::1"), if ip is a valid IPv6 address
//   - the hexadecimal form of ip, without punctuation, if no other cases apply
public static @string String(this IP ip) {
    if (len(ip) == 0) {
        return nilˢ;
    }
    if (len(ip) != IPv4len && len(ip) != IPv6len) {
        return "?"u8 + hexString(ip);
    }
    slice<byte> buf = default!;
    var exprᴛ1 = len(ip);
    if (exprᴛ1 == IPv4len) {
        const nint maxCap = /* len("255.255.255.255") */ 15;
        buf = new slice<byte>(0, maxCap);
    }
    else if (exprᴛ1 == IPv6len) {
        const nint maxCap = /* len("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff") */ 39;
        buf = new slice<byte>(0, maxCap);
    }

    buf = ip.appendTo(buf);
    return ((@string)buf);
}

internal static @string hexString(slice<byte> b) {
    var s = new slice<byte>(len(b) * 2);
    foreach (var (i, tn) in b) {
        (s[i * 2], s[i * 2 + 1]) = (hexDigit[(tn >> (int)(4))], hexDigit[(byte)(tn & 0xf)]);
    }
    return ((@string)s);
}

// ipEmptyString is like ip.String except that it returns
// an empty string when ip is unset.
internal static @string ipEmptyString(IP ip) {
    if (len(ip) == 0) {
        return ""u8;
    }
    return ip.String();
}

// appendTo appends the string representation of ip to b and returns the expanded b
// If len(ip) != IPv4len or IPv6len, it appends nothing.
internal static slice<byte> appendTo(this IP ip, slice<byte> b) {
    // If IPv4, use dotted notation.
    {
        var p4 = ip.To4(); if (len(p4) == IPv4len) {
            ip = p4;
        }
    }
    var (addr, _) = netip.AddrFromSlice(ip);
    return addr.AppendTo(b);
}

// AppendText implements the [encoding.TextAppender] interface.
// The encoding is the same as returned by [IP.String], with one exception:
// When len(ip) is zero, it appends nothing.
public static (slice<byte>, error) AppendText(this IP ip, slice<byte> b) {
    if (len(ip) == 0) {
        return (b, default!);
    }
    if (len(ip) != IPv4len && len(ip) != IPv6len) {
        return (b, new AddrErrorжerror(Ꮡ(new AddrError(Err: "invalid IP address"u8, Addr: hexString(ip)))));
    }
    return (ip.appendTo(b), default!);
}

// MarshalText implements the [encoding.TextMarshaler] interface.
// The encoding is the same as returned by [IP.String], with one exception:
// When len(ip) is zero, it returns an empty slice.
public static (slice<byte>, error) MarshalText(this IP ip) {
    // 24 is satisfied with all IPv4 addresses and short IPv6 addresses
    var (b, err) = ip.AppendText(new slice<byte>(0, 24));
    if (err != default!) {
        return (default!, err);
    }
    return (b, default!);
}

// UnmarshalText implements the [encoding.TextUnmarshaler] interface.
// The IP address is expected in a form accepted by [ParseIP].
public static error UnmarshalText(this ref IP ip, slice<byte> text) {
    if (len(text) == 0) {
        ip = default!;
        return default!;
    }
    ref var s = ref heap<@string>(out var Ꮡs);
    s = ((@string)text);
    var x = ParseIP(s);
    if (x == default!) {
        return new ParseErrorжerror(Ꮡ(new ParseError(Type: "IP address"u8, Text: s)));
    }
    ip = x;
    return default!;
}

// Equal reports whether ip and x are the same IP address.
// An IPv4 address and that same address in IPv6 form are
// considered to be equal.
public static bool Equal(this IP ip, IP x) {
    if (len(ip) == len(x)) {
        return bytealg.Equal(ip, x);
    }
    if (len(ip) == IPv4len && len(x) == IPv6len) {
        return bytealg.Equal(x[0..12], v4InV6Prefix) && bytealg.Equal(ip, x[12..]);
    }
    if (len(ip) == IPv6len && len(x) == IPv4len) {
        return bytealg.Equal(ip[0..12], v4InV6Prefix) && bytealg.Equal(ip[12..], x);
    }
    return false;
}

internal static bool matchAddrFamily(this IP ip, IP x) {
    return ip.To4() != default! && x.To4() != default! || ip.To16() != default! && ip.To4() == default! && x.To16() != default! && x.To4() == default!;
}

// If mask is a sequence of 1 bits followed by 0 bits,
// return the number of 1 bits.
internal static nint simpleMaskLength(IPMask mask) {
    nint n = default!;
    foreach (var (iᴛ1, vᴛ1) in mask) {
        var i = iᴛ1;
        var v = vᴛ1;

        if (v == 0xff) {
            n += 8;
            continue;
        }
        // found non-ff byte
        // count 1 bits
        while ((byte)(v & 0x80) != 0) {
            n++;
            v <<= (int)(1);
        }
        // rest must be 0 bits
        if (v != 0) {
            return -1;
        }
        for (i++; i < len(mask); i++) {
            if (mask[i] != 0) {
                return -1;
            }
        }
        break;
    }
    return n;
}

// Size returns the number of leading ones and total bits in the mask.
// If the mask is not in the canonical form--ones followed by zeros--then
// Size returns 0, 0.
public static (nint ones, nint bits) Size(this IPMask m) {
    nint ones = default!;
    nint bits = default!;

    (ones, bits) = (simpleMaskLength(m), len(m) * 8);
    if (ones == -1) {
        return (0, 0);
    }
    return (ones, bits);
}

// String returns the hexadecimal form of m, with no punctuation.
public static @string String(this IPMask m) {
    if (len(m) == 0) {
        return nilˢ;
    }
    return hexString(m);
}

internal static (IP ip, IPMask m) networkNumberAndMask(ref IPNet n) {
    IP ip = default!;
    IPMask m = default!;

    {
        ip = n.IP.To4(); if (ip == default!) {
            ip = n.IP;
            if (len(ip) != IPv6len) {
                return (default!, default!);
            }
        }
    }
    m = n.Mask;
    var exprᴛ1 = len(m);
    if (exprᴛ1 == IPv4len) {
        if (len(ip) != IPv4len) {
            return (default!, default!);
        }
    }
    else if (exprᴛ1 == IPv6len) {
        if (len(ip) == IPv4len) {
            m = m[12..];
        }
    }
    else { /* default: */
        return (default!, default!);
    }

    return (ip, m);
}

// Contains reports whether the network includes ip.
public static bool Contains(this ж<IPNet> Ꮡn, IP ip) {
    var (nn, m) = networkNumberAndMask(ref (Ꮡn).DerefOrNull());
    {
        var x = ip.To4(); if (x != default!) {
            ip = x;
        }
    }
    nint l = len(ip);
    if (l != len(nn)) {
        return false;
    }
    for (nint i = 0; i < l; i++) {
        if ((byte)(nn[i] & m[i]) != (byte)(ip[i] & m[i])) {
            return false;
        }
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ipNetˢ = "ip+net"u8;

// Network returns the address's network name, "ip+net".
public static @string Network(this ref IPNet n) {
    return ipNetˢ;
}

// String returns the CIDR notation of n like "192.0.2.0/24"
// or "2001:db8::/48" as defined in RFC 4632 and RFC 4291.
// If the mask is not in the canonical form, it returns the
// string which consists of an IP address, followed by a slash
// character and a mask expressed as hexadecimal form with no
// punctuation like "198.51.100.0/c000ff00".
public static @string String(this ж<IPNet> Ꮡn) {
    if (Ꮡn == nil) {
        return nilˢ;
    }
    var (nn, m) = networkNumberAndMask(ref (Ꮡn).DerefOrNull());
    if (nn == default! || m == default!) {
        return nilˢ;
    }
    nint l = simpleMaskLength(m);
    if (l == -1) {
        return nn.String() + "/"u8 + m.String();
    }
    return nn.String() + "/"u8 + itoa.Uitoa((nuint)l);
}

// ParseIP parses s as an IP address, returning the result.
// The string s can be in IPv4 dotted decimal ("192.0.2.1"), IPv6
// ("2001:db8::68"), or IPv4-mapped IPv6 ("::ffff:192.0.2.1") form.
// If s is not a valid textual representation of an IP address,
// ParseIP returns nil. The returned address is always 16 bytes,
// IPv4 addresses are returned in IPv4-mapped IPv6 form.
public static IP ParseIP(@string s) {
    {
        var (addr, valid) = parseIP(s); if (valid) {
            return ((IP)(addr[..]));
        }
    }
    return default!;
}

internal static (array<byte>, bool) parseIP(@string s) {
    var (ip, err) = netip.ParseAddr(s);
    if (err != default! || ip.Zone() != ""u8) {
        return (new byte[]{}.array(16), false);
    }
    return (ip.As16(), true);
}

// ParseCIDR parses s as a CIDR notation IP address and prefix length,
// like "192.0.2.0/24" or "2001:db8::/32", as defined in
// RFC 4632 and RFC 4291.
//
// It returns the IP address and the network implied by the IP and
// prefix length.
// For example, ParseCIDR("192.0.2.1/24") returns the IP address
// 192.0.2.1 and the network 192.0.2.0/24.
public static (IP, ж<IPNet>, error) ParseCIDR(@string s) {
    var (addr, mask, found) = stringslite.Cut(s, "/"u8);
    if (!found) {
        return (default!, default!, new ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: s))));
    }
    var (ipAddr, err) = netip.ParseAddr(addr);
    if (err != default! || ipAddr.Zone() != ""u8) {
        return (default!, default!, new ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: s))));
    }
    var (n, i, ok) = dtoi(mask);
    if (!ok || i != len(mask) || n < 0 || n > ipAddr.BitLen()) {
        return (default!, default!, new ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: s))));
    }
    var m = CIDRMask(n, ipAddr.BitLen());
    var addr16 = ipAddr.As16();
    return (((IP)(addr16[..])), Ꮡ(new IPNet(IP: ((IP)(addr16[..])).Mask(m), Mask: m)), default!);
}

internal static IP copyIP(IP x) {
    var y = new IP(len(x));
    copy(y, x);
    return y;
}

} // end net_package
