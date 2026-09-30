// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package netip defines an IP address type that's a small value type.
// Building on that [Addr] type, the package also defines [AddrPort] (an
// IP address and a port) and [Prefix] (an IP address and a bit length
// prefix).
//
// Compared to the [net.IP] type, [Addr] type takes less memory, is immutable,
// and is comparable (supports == and being a map key).
namespace go.net;

using cmp = cmp_package;
using errors = errors_package;
using bytealg = @internal.bytealg_package;
using byteorder = @internal.byteorder_package;
using itoa = @internal.itoa_package;
using math = math_package;
using strconv = strconv_package;
using unique = unique_package;
using @internal;

partial class netip_package {

// Sizes: (64-bit)
//   net.IP:     24 byte slice header + {4, 16} = 28 to 40 bytes
//   net.IPAddr: 40 byte slice header + {4, 16} = 44 to 56 bytes + zone length
//   netip.Addr: 24 bytes (zone is per-name singleton, shared across all users)

// Addr represents an IPv4 or IPv6 address (with or without a scoped
// addressing zone), similar to [net.IP] or [net.IPAddr].
//
// Unlike [net.IP] or [net.IPAddr], Addr is a comparable value
// type (it supports == and can be a map key) and is immutable.
//
// The zero Addr is not a valid IP address.
// Addr{} is distinct from both 0.0.0.0 and ::.
[GoType] partial struct ΔAddr {
    // addr is the hi and lo bits of an IPv6 address. If z==z4,
    // hi and lo contain the IPv4-mapped IPv6 address.
    //
    // hi and lo are constructed by interpreting a 16-byte IPv6
    // address as a big-endian 128-bit number. The most significant
    // bits of that number go into hi, the rest into lo.
    //
    // For example, 0011:2233:4455:6677:8899:aabb:ccdd:eeff is stored as:
    //  addr.hi = 0x0011223344556677
    //  addr.lo = 0x8899aabbccddeeff
    //
    // We store IPs like this, rather than as [16]byte, because it
    // turns most operations on IPs into arithmetic and bit-twiddling
    // operations on 64-bit registers, which is much faster than
    // bytewise processing.
    internal uint128 addr;
    // Details about the address, wrapped up together and canonicalized.
    internal unique.Handle<addrDetail> z;
}

// addrDetail represents the details of an Addr, like address family and IPv6 zone.
[GoType] partial struct addrDetail {
    internal bool isV6;   // IPv4 is false, IPv6 is true.
    internal @string zoneV6; // != "" only if IsV6 is true.
}

// z0, z4, and z6noz are sentinel Addr.z values.
// See the Addr type's field docs.
internal static unique.Handle<addrDetail> z0;

internal static unique.Handle<addrDetail> z4 = unique.Make<addrDetail>(new addrDetail(nil));

internal static unique.Handle<addrDetail> z6noz = unique.Make<addrDetail>(new addrDetail(isV6: true));

// IPv6LinkLocalAllNodes returns the IPv6 link-local all nodes multicast
// address ff02::1.
public static ΔAddr IPv6LinkLocalAllNodes() {
    return AddrFrom16(new array<byte>(16){[0] = 0xff, [1] = 0x02, [15] = 0x01});
}

// IPv6LinkLocalAllRouters returns the IPv6 link-local all routers multicast
// address ff02::2.
public static ΔAddr IPv6LinkLocalAllRouters() {
    return AddrFrom16(new array<byte>(16){[0] = 0xff, [1] = 0x02, [15] = 0x02});
}

// IPv6Loopback returns the IPv6 loopback address ::1.
public static ΔAddr IPv6Loopback() {
    return AddrFrom16(new array<byte>(16){[15] = 0x01});
}

// IPv6Unspecified returns the IPv6 unspecified address "::".
public static ΔAddr IPv6Unspecified() {
    return new ΔAddr(z: z6noz);
}

// IPv4Unspecified returns the IPv4 unspecified address "0.0.0.0".
public static ΔAddr IPv4Unspecified() {
    return AddrFrom4(new byte[]{}.array(4));
}

// AddrFrom4 returns the address of the IPv4 address given by the bytes in addr.
public static ΔAddr AddrFrom4([GoArrayDims(4)] array<byte> addr) {
    addr = addr.Clone();

    return new ΔAddr(
        addr: new uint128(0, (uint64)((uint64)((uint64)((uint64)(0xffff00000000UL | ((uint64)addr[0] << (int)(24))) | ((uint64)addr[1] << (int)(16))) | ((uint64)addr[2] << (int)(8))) | (uint64)addr[3])),
        z: z4
    );
}

// AddrFrom16 returns the IPv6 address given by the bytes in addr.
// An IPv4-mapped IPv6 address is left as an IPv6 address.
// (Use Unmap to convert them if needed.)
public static ΔAddr AddrFrom16([GoArrayDims(16)] array<byte> addr) {
    addr = addr.Clone();

    return new ΔAddr(
        addr: new uint128(
            byteorder.BEUint64(addr[..8]),
            byteorder.BEUint64(addr[8..])
        ),
        z: z6noz
    );
}

// ParseAddr parses s as an IP address, returning the result. The string
// s can be in dotted decimal ("192.0.2.1"), IPv6 ("2001:db8::68"),
// or IPv6 with a scoped addressing zone ("fe80::1cc0:3e8c:119f:c2e1%ens18").
public static (ΔAddr, error) ParseAddr(@string s) {
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        case (rune)'.': {
            return parseIPv4(s);
        }
        case (rune)':': {
            return parseIPv6(s);
        }
        case (rune)'%': {
            return (new ΔAddr(nil), new parseAddrError( // Assume that this was trying to be an IPv6 address with
 // a zone specifier, but the address is missing.
@in: s, msg: "missing IPv6 address"u8));
        }}

    }
    return (new ΔAddr(nil), new parseAddrError(@in: s, msg: "unable to parse IP"u8));
}

// MustParseAddr calls [ParseAddr](s) and panics on error.
// It is intended for use in tests with hard-coded strings.
public static ΔAddr MustParseAddr(@string s) {
    var (ip, err) = ParseAddr(s);
    if (err != default!) {
        throw panic(err);
    }
    return ip;
}

[GoType] partial struct parseAddrError {
    internal @string @in; // the string given to ParseAddr
    internal @string msg; // an explanation of the parse failure
    internal @string at; // optionally, the unparsed portion of in at which the error occurred.
}

internal static @string Error(this parseAddrError err) {
    var q = strconv.Quote;
    if (err.at != ""u8) {
        return "ParseAddr("u8 + q(err.@in) + "): "u8 + err.msg + " (at "u8 + q(err.at) + ")"u8;
    }
    return "ParseAddr("u8 + q(err.@in) + "): "u8 + err.msg;
}

internal static error parseIPv4Fields(@string @in, nint off, nint end, slice<uint8> fields) {
    nint val = default!;
    nint pos = default!;
    nint digLen = default!;  // number of digits in current octet
    @string s = @in.slice(off, end);
    for (nint i = 0; i < len(s); i++) {
        if (s[i] >= (rune)'0' && s[i] <= (rune)'9'){
            if (digLen == 1 && val == 0) {
                return new parseAddrError(@in: @in, msg: "IPv4 field has octet with leading zero"u8);
            }
            val = val * 10 + (nint)s[i] - (rune)'0';
            digLen++;
            if (val > 255) {
                return new parseAddrError(@in: @in, msg: "IPv4 field has value >255"u8);
            }
        } else 
        if (s[i] == (rune)'.'){
            // .1.2.3
            // 1.2.3.
            // 1..2.3
            if (i == 0 || i == len(s) - 1 || s[i - 1] == (rune)'.') {
                return new parseAddrError(@in: @in, msg: "IPv4 field must have at least one digit"u8, at: s.slice(i));
            }
            // 1.2.3.4.5
            if (pos == 3) {
                return new parseAddrError(@in: @in, msg: "IPv4 address too long"u8);
            }
            fields[pos] = (uint8)val;
            pos++;
            val = 0;
            digLen = 0;
        } else {
            return new parseAddrError(@in: @in, msg: "unexpected character"u8, at: s.slice(i));
        }
    }
    if (pos < 3) {
        return new parseAddrError(@in: @in, msg: "IPv4 address too short"u8);
    }
    fields[3] = (uint8)val;
    return default!;
}

// parseIPv4 parses s as an IPv4 address (in form "192.168.0.1").
internal static (ΔAddr ip, error err) parseIPv4(@string s) {
    error err = default!;

    array<uint8> fields = new(4);
    err = parseIPv4Fields(s, 0, len(s), fields[..]);
    if (err != default!) {
        return (new ΔAddr(nil), err);
    }
    return (AddrFrom4(fields), default!);
}

// parseIPv6 parses s as an IPv6 address (in form "2001:db8::68").
internal static (ΔAddr, error) parseIPv6(@string @in) {
    @string s = @in;
    // Split off the zone right from the start. Yes it's a second scan
    // of the string, but trying to handle it inline makes a bunch of
    // other inner loop conditionals more expensive, and it ends up
    // being slower.
    @string zone = ""u8;
    nint i = bytealg.IndexByteString(s, (rune)'%');
    if (i != -1) {
        (s, zone) = (s.slice(0, i), s.slice(i + 1));
        if (zone == ""u8) {
            // Not allowed to have an empty zone if explicitly specified.
            return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "zone must be a non-empty string"u8));
        }
    }
    array<byte> ip = new(16);
    nint ellipsis = -1; // position of ellipsis in ip
    // Might have leading ellipsis
    if (len(s) >= 2 && s[0] == (rune)':' && s[1] == (rune)':') {
        ellipsis = 0;
        s = s[2..];
        // Might be only ellipsis
        if (len(s) == 0) {
            return (IPv6Unspecified().WithZone(zone), default!);
        }
    }
    // Loop, parsing hex numbers followed by colon.
    i = 0;
    while (i < 16) {
        // Hex number. Similar to parseIPv4, inlining the hex number
        // parsing yields a significant performance increase.
        nint off = 0;
        var acc = (uint32)0;
        for (; off < len(s); off++) {
            var c = s[off];
            if (c >= (rune)'0' && c <= (rune)'9'){
                acc = ((acc << (int)(4))) + (uint32)((byte)(c - (rune)'0'));
            } else 
            if (c >= (rune)'a' && c <= (rune)'f'){
                acc = ((acc << (int)(4))) + (uint32)((byte)(c - (rune)'a' + 10));
            } else 
            if (c >= (rune)'A' && c <= (rune)'F'){
                acc = ((acc << (int)(4))) + (uint32)((byte)(c - (rune)'A' + 10));
            } else {
                break;
            }
            if (off > 3) {
                //more than 4 digits in group, fail.
                return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "each group must have 4 or less digits"u8, at: s));
            }
            if (acc > math.MaxUint16) {
                // Overflow, fail.
                return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "IPv6 field has value >=2^16"u8, at: s));
            }
        }
        if (off == 0) {
            // No digits found, fail.
            return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "each colon-separated field must have at least one digit"u8, at: s));
        }
        // If followed by dot, might be in trailing IPv4.
        if (off < len(s) && s[off] == (rune)'.') {
            if (ellipsis < 0 && i != 12) {
                // Not the right place.
                return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "embedded IPv4 address must replace the final 2 fields of the address"u8, at: s));
            }
            if (i + 4 > 16) {
                // Not enough room.
                return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "too many hex fields to fit an embedded IPv4 at the end of the address"u8, at: s));
            }
            nint end = len(@in);
            if (len(zone) > 0) {
                end -= len(zone) + 1;
            }
            var err = parseIPv4Fields(@in, end - len(s), end, ip.slice(i, i + 4));
            if (err != default!) {
                return (new ΔAddr(nil), err);
            }
            s = ""u8;
            i += 4;
            break;
        }
        // Save this 16-bit chunk.
        ip[i] = (byte)((acc >> (int)(8)));
        ip[i + 1] = (byte)acc;
        i += 2;
        // Stop at end of string.
        s = s.slice(off);
        if (len(s) == 0) {
            break;
        }
        // Otherwise must be followed by colon and more.
        if (s[0] != (rune)':'){
            return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "unexpected character, want colon"u8, at: s));
        } else 
        if (len(s) == 1) {
            return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "colon must be followed by more characters"u8, at: s));
        }
        s = s[1..];
        // Look for ellipsis.
        if (s[0] == (rune)':') {
            if (ellipsis >= 0) {
                // already have one
                return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "multiple :: in address"u8, at: s));
            }
            ellipsis = i;
            s = s[1..];
            if (len(s) == 0) {
                // can be at end
                break;
            }
        }
    }
    // Must have used entire string.
    if (len(s) != 0) {
        return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "trailing garbage after address"u8, at: s));
    }
    // If didn't parse enough, expand ellipsis.
    if (i < 16){
        if (ellipsis < 0) {
            return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "address string too short"u8));
        }
        nint n = 16 - i;
        for (nint j = i - 1; j >= ellipsis; j--) {
            ip[j + n] = ip[j];
        }
        clear(ip.slice(ellipsis, ellipsis + n));
    } else 
    if (ellipsis >= 0) {
        // Ellipsis must represent at least one 0 group.
        return (new ΔAddr(nil), new parseAddrError(@in: @in, msg: "the :: must expand to at least one field of zeros"u8));
    }
    return (AddrFrom16(ip).WithZone(zone), default!);
}

// AddrFromSlice parses the 4- or 16-byte byte slice as an IPv4 or IPv6 address.
// Note that a [net.IP] can be passed directly as the []byte argument.
// If slice's length is not 4 or 16, AddrFromSlice returns [Addr]{}, false.
public static (ΔAddr ip, bool ok) AddrFromSlice(slice<byte> Δslice) {
    switch (len(Δslice)) {
    case 4: {
        return (AddrFrom4(new array<byte>(Δslice, 4)), true);
    }
    case 16: {
        return (AddrFrom16(new array<byte>(Δslice, 16)), true);
    }}

    return (new ΔAddr(nil), false);
}

// v4 returns the i'th byte of ip. If ip is not an IPv4, v4 returns
// unspecified garbage.
internal static uint8 v4(this ΔAddr ip, uint8 i) {
    return (uint8)(ip.addr.lo.Rsh((uint64)((uint8)((3 - i) * 8))));
}

// v6 returns the i'th byte of ip. If ip is an IPv4 address, this
// accesses the IPv4-mapped IPv6 address form of the IP.
internal static uint8 v6(this ΔAddr ip, uint8 i) {
    return (uint8)(((Ꮡ(ip).of(netip_package.ΔAddr.Ꮡaddr).halves()[(i / 8) % 2]).Value).Rsh((uint64)((uint8)((7 - i % 8) * 8))));
}

// v6u16 returns the i'th 16-bit word of ip. If ip is an IPv4 address,
// this accesses the IPv4-mapped IPv6 address form of the IP.
internal static uint16 v6u16(this ΔAddr ip, uint8 i) {
    return (uint16)(((Ꮡ(ip).of(netip_package.ΔAddr.Ꮡaddr).halves()[(i / 4) % 2]).Value).Rsh((uint64)((uint8)((3 - i % 4) * 16))));
}

// isZero reports whether ip is the zero value of the IP type.
// The zero value is not a valid IP address of any type.
//
// Note that "0.0.0.0" and "::" are not the zero value. Use IsUnspecified to
// check for these values instead.
internal static bool isZero(this ΔAddr ip) {
    // Faster than comparing ip == Addr{}, but effectively equivalent,
    // as there's no way to make an IP with a nil z from this package.
    return ip.z == z0;
}

// IsValid reports whether the [Addr] is an initialized address (not the zero Addr).
//
// Note that "0.0.0.0" and "::" are both valid values.
public static bool IsValid(this ΔAddr ip) {
    return ip.z != z0;
}

// BitLen returns the number of bits in the IP address:
// 128 for IPv6, 32 for IPv4, and 0 for the zero [Addr].
//
// Note that IPv4-mapped IPv6 addresses are considered IPv6 addresses
// and therefore have bit length 128.
public static nint BitLen(this ΔAddr ip) {
    var exprᴛ1 = ip.z;
    if (exprᴛ1 == z0) {
        return 0;
    }
    if (exprᴛ1 == z4) {
        return 32;
    }

    return 128;
}

// Zone returns ip's IPv6 scoped addressing zone, if any.
public static @string Zone(this ΔAddr ip) {
    if (ip.z == z0) {
        return ""u8;
    }
    return ip.z.Value().zoneV6;
}

// Compare returns an integer comparing two IPs.
// The result will be 0 if ip == ip2, -1 if ip < ip2, and +1 if ip > ip2.
// The definition of "less than" is the same as the [Addr.Less] method.
public static nint Compare(this ΔAddr ip, ΔAddr ip2) {
    nint f1 = ip.BitLen();
    nint f2 = ip2.BitLen();
    if (f1 < f2) {
        return -1;
    }
    if (f1 > f2) {
        return 1;
    }
    var (hi1, hi2) = (ip.addr.hi, ip2.addr.hi);
    if (hi1 < hi2) {
        return -1;
    }
    if (hi1 > hi2) {
        return 1;
    }
    var (lo1, lo2) = (ip.addr.lo, ip2.addr.lo);
    if (lo1 < lo2) {
        return -1;
    }
    if (lo1 > lo2) {
        return 1;
    }
    if (ip.Is6()) {
        @string za = ip.Zone();
        @string zb = ip2.Zone();
        if (za < zb) {
            return -1;
        }
        if (za > zb) {
            return 1;
        }
    }
    return 0;
}

// Less reports whether ip sorts before ip2.
// IP addresses sort first by length, then their address.
// IPv6 addresses with zones sort just after the same address without a zone.
public static bool Less(this ΔAddr ip, ΔAddr ip2) {
    return ip.Compare(ip2) == -1;
}

// Is4 reports whether ip is an IPv4 address.
//
// It returns false for IPv4-mapped IPv6 addresses. See [Addr.Unmap].
public static bool Is4(this ΔAddr ip) {
    return ip.z == z4;
}

// Is4In6 reports whether ip is an "IPv4-mapped IPv6 address"
// as defined by RFC 4291.
// That is, it reports whether ip is in ::ffff:0:0/96.
public static bool Is4In6(this ΔAddr ip) {
    return ip.Is6() && ip.addr.hi == 0 && (ip.addr.lo >> (int)(32)) == 0xffff;
}

// Is6 reports whether ip is an IPv6 address, including IPv4-mapped
// IPv6 addresses.
public static bool Is6(this ΔAddr ip) {
    return ip.z != z0 && ip.z != z4;
}

// Unmap returns ip with any IPv4-mapped IPv6 address prefix removed.
//
// That is, if ip is an IPv6 address wrapping an IPv4 address, it
// returns the wrapped IPv4 address. Otherwise it returns ip unmodified.
public static ΔAddr Unmap(this ΔAddr ip) {
    if (ip.Is4In6()) {
        ip.z = z4;
    }
    return ip;
}

// WithZone returns an IP that's the same as ip but with the provided
// zone. If zone is empty, the zone is removed. If ip is an IPv4
// address, WithZone is a no-op and returns ip unchanged.
public static ΔAddr WithZone(this ΔAddr ip, @string zone) {
    if (!ip.Is6()) {
        return ip;
    }
    if (zone == ""u8) {
        ip.z = z6noz;
        return ip;
    }
    ip.z = unique.Make<addrDetail>(new addrDetail(isV6: true, zoneV6: zone));
    return ip;
}

// withoutZone unconditionally strips the zone from ip.
// It's similar to WithZone, but small enough to be inlinable.
internal static ΔAddr withoutZone(this ΔAddr ip) {
    if (!ip.Is6()) {
        return ip;
    }
    ip.z = z6noz;
    return ip;
}

// hasZone reports whether ip has an IPv6 zone.
internal static bool hasZone(this ΔAddr ip) {
    return ip.z != z0 && ip.z != z4 && ip.z != z6noz;
}

// IsLinkLocalUnicast reports whether ip is a link-local unicast address.
public static bool IsLinkLocalUnicast(this ΔAddr ip) {
    if (ip.Is4In6()) {
        ip = ip.Unmap();
    }
    // Dynamic Configuration of IPv4 Link-Local Addresses
    // https://datatracker.ietf.org/doc/html/rfc3927#section-2.1
    if (ip.Is4()) {
        return ip.v4(0) == 169 && ip.v4(1) == 254;
    }
    // IP Version 6 Addressing Architecture (2.4 Address Type Identification)
    // https://datatracker.ietf.org/doc/html/rfc4291#section-2.4
    if (ip.Is6()) {
        return (uint16)(ip.v6u16(0) & 0xffc0) == 0xfe80;
    }
    return false; // zero value
}

// IsLoopback reports whether ip is a loopback address.
public static bool IsLoopback(this ΔAddr ip) {
    if (ip.Is4In6()) {
        ip = ip.Unmap();
    }
    // Requirements for Internet Hosts -- Communication Layers (3.2.1.3 Addressing)
    // https://datatracker.ietf.org/doc/html/rfc1122#section-3.2.1.3
    if (ip.Is4()) {
        return ip.v4(0) == 127;
    }
    // IP Version 6 Addressing Architecture (2.4 Address Type Identification)
    // https://datatracker.ietf.org/doc/html/rfc4291#section-2.4
    if (ip.Is6()) {
        return ip.addr.hi == 0 && ip.addr.lo == 1;
    }
    return false; // zero value
}

// IsMulticast reports whether ip is a multicast address.
public static bool IsMulticast(this ΔAddr ip) {
    if (ip.Is4In6()) {
        ip = ip.Unmap();
    }
    // Host Extensions for IP Multicasting (4. HOST GROUP ADDRESSES)
    // https://datatracker.ietf.org/doc/html/rfc1112#section-4
    if (ip.Is4()) {
        return (uint8)(ip.v4(0) & 0xf0) == 0xe0;
    }
    // IP Version 6 Addressing Architecture (2.4 Address Type Identification)
    // https://datatracker.ietf.org/doc/html/rfc4291#section-2.4
    if (ip.Is6()) {
        return (ip.addr.hi >> (int)((64 - 8))) == 0xff; // ip.v6(0) == 0xff
    }
    return false; // zero value
}

// IsInterfaceLocalMulticast reports whether ip is an IPv6 interface-local
// multicast address.
public static bool IsInterfaceLocalMulticast(this ΔAddr ip) {
    // IPv6 Addressing Architecture (2.7.1. Pre-Defined Multicast Addresses)
    // https://datatracker.ietf.org/doc/html/rfc4291#section-2.7.1
    if (ip.Is6() && !ip.Is4In6()) {
        return (uint16)(ip.v6u16(0) & 0xff0f) == 0xff01;
    }
    return false; // zero value
}

// IsLinkLocalMulticast reports whether ip is a link-local multicast address.
public static bool IsLinkLocalMulticast(this ΔAddr ip) {
    if (ip.Is4In6()) {
        ip = ip.Unmap();
    }
    // IPv4 Multicast Guidelines (4. Local Network Control Block (224.0.0/24))
    // https://datatracker.ietf.org/doc/html/rfc5771#section-4
    if (ip.Is4()) {
        return ip.v4(0) == 224 && ip.v4(1) == 0 && ip.v4(2) == 0;
    }
    // IPv6 Addressing Architecture (2.7.1. Pre-Defined Multicast Addresses)
    // https://datatracker.ietf.org/doc/html/rfc4291#section-2.7.1
    if (ip.Is6()) {
        return (uint16)(ip.v6u16(0) & 0xff0f) == 0xff02;
    }
    return false; // zero value
}

// IsGlobalUnicast reports whether ip is a global unicast address.
//
// It returns true for IPv6 addresses which fall outside of the current
// IANA-allocated 2000::/3 global unicast space, with the exception of the
// link-local address space. It also returns true even if ip is in the IPv4
// private address space or IPv6 unique local address space.
// It returns false for the zero [Addr].
//
// For reference, see RFC 1122, RFC 4291, and RFC 4632.
public static bool IsGlobalUnicast(this ΔAddr ip) {
    if (ip.z == z0) {
        // Invalid or zero-value.
        return false;
    }
    if (ip.Is4In6()) {
        ip = ip.Unmap();
    }
    // Match package net's IsGlobalUnicast logic. Notably private IPv4 addresses
    // and ULA IPv6 addresses are still considered "global unicast".
    if (ip.Is4() && (ip == IPv4Unspecified() || ip == AddrFrom4(new byte[]{255, 255, 255, 255}.array()))) {
        return false;
    }
    return ip != IPv6Unspecified() && !ip.IsLoopback() && !ip.IsMulticast() && !ip.IsLinkLocalUnicast();
}

// IsPrivate reports whether ip is a private address, according to RFC 1918
// (IPv4 addresses) and RFC 4193 (IPv6 addresses). That is, it reports whether
// ip is in 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16, or fc00::/7. This is the
// same as [net.IP.IsPrivate].
public static bool IsPrivate(this ΔAddr ip) {
    if (ip.Is4In6()) {
        ip = ip.Unmap();
    }
    // Match the stdlib's IsPrivate logic.
    if (ip.Is4()) {
        // RFC 1918 allocates 10.0.0.0/8, 172.16.0.0/12, and 192.168.0.0/16 as
        // private IPv4 address subnets.
        return ip.v4(0) == 10 || (ip.v4(0) == 172 && (uint8)(ip.v4(1) & 0xf0) == 16) || (ip.v4(0) == 192 && ip.v4(1) == 168);
    }
    if (ip.Is6()) {
        // RFC 4193 allocates fc00::/7 as the unique local unicast IPv6 address
        // subnet.
        return (uint8)(ip.v6(0) & 0xfe) == 0xfc;
    }
    return false; // zero value
}

// IsUnspecified reports whether ip is an unspecified address, either the IPv4
// address "0.0.0.0" or the IPv6 address "::".
//
// Note that the zero [Addr] is not an unspecified address.
public static bool IsUnspecified(this ΔAddr ip) {
    return ip == IPv4Unspecified() || ip == IPv6Unspecified();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string negativePrefixBitsˢ = "negative Prefix bits"u8;

// Prefix keeps only the top b bits of IP, producing a Prefix
// of the specified length.
// If ip is a zero [Addr], Prefix always returns a zero Prefix and a nil error.
// Otherwise, if bits is less than zero or greater than ip.BitLen(),
// Prefix returns an error.
public static (ΔPrefix, error) Prefix(this ΔAddr ip, nint b) {
    if (b < 0) {
        return (new ΔPrefix(nil), errors.New(negativePrefixBitsˢ));
    }
    nint effectiveBits = b;
    var exprᴛ1 = ip.z;
    if (exprᴛ1 == z0) {
        return (new ΔPrefix(nil), default!);
    }
    if (exprᴛ1 == z4) {
        if (b > 32) {
            return (new ΔPrefix(nil), errors.New("prefix length "u8 + itoa.Itoa(b) + " too large for IPv4"u8));
        }
        effectiveBits += 96;
    }
    else { /* default: */
        if (b > 128) {
            return (new ΔPrefix(nil), errors.New("prefix length "u8 + itoa.Itoa(b) + " too large for IPv6"u8));
        }
    }

    ip.addr = ip.addr.and(mask6(effectiveBits));
    return (PrefixFrom(ip, b), default!);
}

// As16 returns the IP address in its 16-byte representation.
// IPv4 addresses are returned as IPv4-mapped IPv6 addresses.
// IPv6 addresses with zones are returned without their zone (use the
// [Addr.Zone] method to get it).
// The ip zero value returns all zeroes.
public static array<byte> /*a16*/ As16(this ΔAddr ip) {
    array<byte> a16 = new(16);

    byteorder.BEPutUint64(a16[..8], ip.addr.hi);
    byteorder.BEPutUint64(a16[8..], ip.addr.lo);
    return a16.Clone();
}

// As4 returns an IPv4 or IPv4-in-IPv6 address in its 4-byte representation.
// If ip is the zero [Addr] or an IPv6 address, As4 panics.
// Note that 0.0.0.0 is not the zero Addr.
public static array<byte> /*a4*/ As4(this ΔAddr ip) {
    array<byte> a4 = new(4);

    if (ip.z == z4 || ip.Is4In6()) {
        byteorder.BEPutUint32(a4[..], (uint32)ip.addr.lo);
        return a4.Clone();
    }
    if (ip.z == z0) {
        throw panic("As4 called on IP zero value");
    }
    throw panic("As4 called on IPv6 address");
}

// AsSlice returns an IPv4 or IPv6 address in its respective 4-byte or 16-byte representation.
public static slice<byte> AsSlice(this ΔAddr ip) {
    var exprᴛ1 = ip.z;
    if (exprᴛ1 == z0) {
        return default!;
    }
    if (exprᴛ1 == z4) {
        array<byte> ret = new(4);
        byteorder.BEPutUint32(ret[..], (uint32)ip.addr.lo);
        return ret[..];
    }
    { /* default: */
        array<byte> ret = new(16);
        byteorder.BEPutUint64(ret[..8], ip.addr.hi);
        byteorder.BEPutUint64(ret[8..], ip.addr.lo);
        return ret[..];
    }

}

// Next returns the address following ip.
// If there is none, it returns the zero [Addr].
public static ΔAddr Next(this ΔAddr ip) {
    ip.addr = ip.addr.addOne();
    if (ip.Is4()){
        if ((uint32)ip.addr.lo == 0) {
            // Overflowed.
            return new ΔAddr(nil);
        }
    } else {
        if (ip.addr.isZero()) {
            // Overflowed
            return new ΔAddr(nil);
        }
    }
    return ip;
}

// Prev returns the IP before ip.
// If there is none, it returns the IP zero value.
public static ΔAddr Prev(this ΔAddr ip) {
    if (ip.Is4()){
        if ((uint32)ip.addr.lo == 0) {
            return new ΔAddr(nil);
        }
    } else 
    if (ip.addr.isZero()) {
        return new ΔAddr(nil);
    }
    ip.addr = ip.addr.subOne();
    return ip;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidIpˢ = "invalid IP"u8;

// String returns the string form of the IP address ip.
// It returns one of 5 forms:
//
//   - "invalid IP", if ip is the zero [Addr]
//   - IPv4 dotted decimal ("192.0.2.1")
//   - IPv6 ("2001:db8::1")
//   - "::ffff:1.2.3.4" (if [Addr.Is4In6])
//   - IPv6 with zone ("fe80:db8::1%eth0")
//
// Note that unlike package net's IP.String method,
// IPv4-mapped IPv6 addresses format with a "::ffff:"
// prefix before the dotted quad.
public static @string String(this ΔAddr ip) {
    var exprᴛ1 = ip.z;
    if (exprᴛ1 == z0) {
        return invalidIpˢ;
    }
    if (exprᴛ1 == z4) {
        return ip.string4();
    }
    { /* default: */
        if (ip.Is4In6()) {
            return ip.string4In6();
        }
        return ip.string6();
    }

}

// AppendTo appends a text encoding of ip,
// as generated by [Addr.MarshalText],
// to b and returns the extended buffer.
public static slice<byte> AppendTo(this ΔAddr ip, slice<byte> b) {
    var exprᴛ1 = ip.z;
    if (exprᴛ1 == z0) {
        return b;
    }
    if (exprᴛ1 == z4) {
        return ip.appendTo4(b);
    }
    { /* default: */
        if (ip.Is4In6()) {
            return ip.appendTo4In6(b);
        }
        return ip.appendTo6(b);
    }

}

// digits is a string of the hex digits from 0 to f. It's used in
// appendDecimal and appendHex to format IP addresses.
internal static readonly @string digits = "0123456789abcdef"u8;

// appendDecimal appends the decimal string representation of x to b.
internal static slice<byte> appendDecimal(slice<byte> b, uint8 x) {
    // Using this function rather than strconv.AppendUint makes IPv4
    // string building 2x faster.
    if (x >= 100) {
        b = append(b, digits[x / 100]);
    }
    if (x >= 10) {
        b = append(b, digits[x / 10 % 10]);
    }
    return append(b, digits[x % 10]);
}

// appendHex appends the hex string representation of x to b.
internal static slice<byte> appendHex(slice<byte> b, uint16 x) {
    // Using this function rather than strconv.AppendUint makes IPv6
    // string building 2x faster.
    if (x >= 0x1000) {
        b = append(b, digits[(x >> (int)(12))]);
    }
    if (x >= 0x100) {
        b = append(b, digits[(uint16)((x >> (int)(8)) & 0xf)]);
    }
    if (x >= 0x10) {
        b = append(b, digits[(uint16)((x >> (int)(4)) & 0xf)]);
    }
    return append(b, digits[(uint16)(x & 0xf)]);
}

// appendHexPad appends the fully padded hex string representation of x to b.
internal static slice<byte> appendHexPad(slice<byte> b, uint16 x) {
    return append(b, digits[(x >> (int)(12))], digits[(uint16)((x >> (int)(8)) & 0xf)], digits[(uint16)((x >> (int)(4)) & 0xf)], digits[(uint16)(x & 0xf)]);
}

internal static @string string4(this ΔAddr ip) {
    const nint max = /* len("255.255.255.255") */ 15;
    var ret = new slice<byte>(0, max);
    ret = ip.appendTo4(ret);
    return ((@string)ret);
}

internal static slice<byte> appendTo4(this ΔAddr ip, slice<byte> ret) {
    ret = appendDecimal(ret, ip.v4(0));
    ret = append(ret, (byte)((rune)'.'));
    ret = appendDecimal(ret, ip.v4(1));
    ret = append(ret, (byte)((rune)'.'));
    ret = appendDecimal(ret, ip.v4(2));
    ret = append(ret, (byte)((rune)'.'));
    ret = appendDecimal(ret, ip.v4(3));
    return ret;
}

internal static @string string4In6(this ΔAddr ip) {
    const nint max = /* len("::ffff:255.255.255.255%enp5s0") */ 29;
    var ret = new slice<byte>(0, max);
    ret = ip.appendTo4In6(ret);
    return ((@string)ret);
}

internal static slice<byte> appendTo4In6(this ΔAddr ip, slice<byte> ret) {
    ret = append(ret, ((@string)"::ffff:"u8).ꓸꓸꓸ);
    ret = ip.Unmap().appendTo4(ret);
    if (ip.z != z6noz) {
        ret = append(ret, (byte)((rune)'%'));
        ret = append(ret, ip.Zone().ꓸꓸꓸ);
    }
    return ret;
}

// string6 formats ip in IPv6 textual representation. It follows the
// guidelines in section 4 of RFC 5952
// (https://tools.ietf.org/html/rfc5952#section-4): no unnecessary
// zeros, use :: to elide the longest run of zeros, and don't use ::
// to compact a single zero field.
internal static @string string6(this ΔAddr ip) {
    // Use a zone with a "plausibly long" name, so that most zone-ful
    // IP addresses won't require additional allocation.
    //
    // The compiler does a cool optimization here, where ret ends up
    // stack-allocated and so the only allocation this function does
    // is to construct the returned string. As such, it's okay to be a
    // bit greedy here, size-wise.
    const nint max = /* len("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff%enp5s0") */ 46;
    var ret = new slice<byte>(0, max);
    ret = ip.appendTo6(ret);
    return ((@string)ret);
}

internal static slice<byte> appendTo6(this ΔAddr ip, slice<byte> ret) {
    var (zeroStart, zeroEnd) = ((uint8)255, (uint8)255);
    for (var i = (uint8)0; i < 8; i++) {
        var j = i;
        while (j < 8 && ip.v6u16(j) == 0) {
            j++;
        }
        {
            var l = (uint8)(j - i); if (l >= 2 && l > (uint8)(zeroEnd - zeroStart)) {
                (zeroStart, zeroEnd) = (i, j);
            }
        }
    }
    for (var i = (uint8)0; i < 8; i++) {
        if (i == zeroStart){
            ret = append(ret, (byte)((rune)':'), (byte)((rune)':'));
            i = zeroEnd;
            if (i >= 8) {
                break;
            }
        } else 
        if (i > 0) {
            ret = append(ret, (byte)((rune)':'));
        }
        ret = appendHex(ret, ip.v6u16(i));
    }
    if (ip.z != z6noz) {
        ret = append(ret, (byte)((rune)'%'));
        ret = append(ret, ip.Zone().ꓸꓸꓸ);
    }
    return ret;
}

// StringExpanded is like [Addr.String] but IPv6 addresses are expanded with leading
// zeroes and no "::" compression. For example, "2001:db8::1" becomes
// "2001:0db8:0000:0000:0000:0000:0000:0001".
public static @string StringExpanded(this ΔAddr ip) {
    var exprᴛ1 = ip.z;
    if (exprᴛ1 == z0 || exprᴛ1 == z4) {
        return ip.String();
    }

    const nint size = /* len("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff") */ 39;
    var ret = new slice<byte>(0, size);
    for (var i = (uint8)0; i < 8; i++) {
        if (i > 0) {
            ret = append(ret, (byte)((rune)':'));
        }
        ret = appendHexPad(ret, ip.v6u16(i));
    }
    if (ip.z != z6noz) {
        // The addition of a zone will cause a second allocation, but when there
        // is no zone the ret slice will be stack allocated.
        ret = append(ret, (byte)((rune)'%'));
        ret = append(ret, ip.Zone().ꓸꓸꓸ);
    }
    return ((@string)ret);
}

// AppendText implements the [encoding.TextAppender] interface,
// It is the same as [Addr.AppendTo].
public static (slice<byte>, error) AppendText(this ΔAddr ip, slice<byte> b) {
    return (ip.AppendTo(b), default!);
}

// MarshalText implements the [encoding.TextMarshaler] interface,
// The encoding is the same as returned by [Addr.String], with one exception:
// If ip is the zero [Addr], the encoding is the empty string.
public static (slice<byte>, error) MarshalText(this ΔAddr ip) {
    var buf = new byte[]{}.slice();
    var exprᴛ1 = ip.z;
    if (exprᴛ1 == z0) {
    }
    else if (exprᴛ1 == z4) {
        const nint maxCap = /* len("255.255.255.255") */ 15;
        buf = new slice<byte>(0, maxCap);
    }
    else { /* default: */
        do {
            if (ip.Is4In6()) {
                const nint maxCapΔ1 = /* len("::ffff:255.255.255.255%enp5s0") */ 29;
                buf = new slice<byte>(0, maxCapΔ1);
                break;
            }
            const nint maxCap = /* len("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff%enp5s0") */ 46;
            buf = new slice<byte>(0, maxCap);
        } while (false);
    }

    return ip.AppendText(buf);
}

// UnmarshalText implements the encoding.TextUnmarshaler interface.
// The IP address is expected in a form accepted by [ParseAddr].
//
// If text is empty, UnmarshalText sets *ip to the zero [Addr] and
// returns no error.
[GoRecv] public static error UnmarshalText(this ref ΔAddr ip, slice<byte> text) {
    if (len(text) == 0) {
        ip = new ΔAddr(nil);
        return default!;
    }
    error err = default!;
    (ip, err) = ParseAddr(((@string)text));
    return err;
}

// AppendBinary implements the [encoding.BinaryAppender] interface.
public static (slice<byte>, error) AppendBinary(this ΔAddr ip, slice<byte> b) {
    var exprᴛ1 = ip.z;
    if (exprᴛ1 == z0) {
    }
    else if (exprᴛ1 == z4) {
        b = byteorder.BEAppendUint32(b, (uint32)ip.addr.lo);
    }
    else { /* default: */
        b = byteorder.BEAppendUint64(b, ip.addr.hi);
        b = byteorder.BEAppendUint64(b, ip.addr.lo);
        b = append(b, ip.Zone().ꓸꓸꓸ);
    }

    return (b, default!);
}

internal static nint marshalBinarySize(this ΔAddr ip) {
    var exprᴛ1 = ip.z;
    if (exprᴛ1 == z0) {
        return 0;
    }
    if (exprᴛ1 == z4) {
        return 4;
    }
    { /* default: */
        return 16 + len(ip.Zone());
    }

}

// MarshalBinary implements the [encoding.BinaryMarshaler] interface.
// It returns a zero-length slice for the zero [Addr],
// the 4-byte form for an IPv4 address,
// and the 16-byte form with zone appended for an IPv6 address.
public static (slice<byte>, error) MarshalBinary(this ΔAddr ip) {
    return ip.AppendBinary(new slice<byte>(0, ip.marshalBinarySize()));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedSliceSizeˢ = "unexpected slice size"u8;

// UnmarshalBinary implements the [encoding.BinaryUnmarshaler] interface.
// It expects data in the form generated by MarshalBinary.
[GoRecv] public static error UnmarshalBinary(this ref ΔAddr ip, slice<byte> b) {
    nint n = len(b);
    switch (ᐧ) {
    case {} when n is 0: {
        ip = new ΔAddr(nil);
        return default!;
    }
    case {} when n is 4: {
        ip = AddrFrom4(new array<byte>(b, 4));
        return default!;
    }
    case {} when n is 16: {
        ip = AddrFrom16(new array<byte>(b, 16));
        return default!;
    }
    case {} when n is > 16: {
        ip = AddrFrom16(new array<byte>(b[..16], 16)).WithZone(((@string)(b[16..])));
        return default!;
    }}

    return errors.New(unexpectedSliceSizeˢ);
}

// AddrPort is an IP and a port number.
[GoType] partial struct AddrPort {
    internal ΔAddr ip;
    internal uint16 port;
}

// AddrPortFrom returns an [AddrPort] with the provided IP and port.
// It does not allocate.
public static AddrPort AddrPortFrom(ΔAddr ip, uint16 port) {
    return new AddrPort(ip: ip, port: port);
}

// Addr returns p's IP address.
public static ΔAddr Addr(this AddrPort p) {
    return p.ip;
}

// Port returns p's port.
public static uint16 Port(this AddrPort p) {
    return p.port;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notAnIpPortˢ = "not an ip:port"u8;
internal static readonly @string noIpˢ = "no IP"u8;
internal static readonly @string noPortˢ = "no port"u8;
internal static readonly @string missingˢ = "missing ]"u8;

// splitAddrPort splits s into an IP address string and a port
// string. It splits strings shaped like "foo:bar" or "[foo]:bar",
// without further validating the substrings. v6 indicates whether the
// ip string should parse as an IPv6 address or an IPv4 address, in
// order for s to be a valid ip:port string.
internal static (@string ip, @string port, bool v6, error err) splitAddrPort(@string s) {
    @string ip = default!;
    @string port = default!;
    bool v6 = default!;

    nint i = bytealg.LastIndexByteString(s, (rune)':');
    if (i == -1) {
        return ("", "", false, errors.New(notAnIpPortˢ));
    }
    (ip, port) = (s.slice(0, i), s.slice(i + 1));
    if (len(ip) == 0) {
        return ("", "", false, errors.New(noIpˢ));
    }
    if (len(port) == 0) {
        return ("", "", false, errors.New(noPortˢ));
    }
    if (ip[0] == (rune)'[') {
        if (len(ip) < 2 || ip[len(ip) - 1] != (rune)']') {
            return ("", "", false, errors.New(missingˢ));
        }
        ip = ip.slice(1, len(ip) - 1);
        v6 = true;
    }
    return (ip, port, v6, default!);
}

// ParseAddrPort parses s as an [AddrPort].
//
// It doesn't do any name resolution: both the address and the port
// must be numeric.
public static (AddrPort, error) ParseAddrPort(@string s) {
    AddrPort ipp = default!;
    var (ip, port, v6, err) = splitAddrPort(s);
    if (err != default!) {
        return (ipp, err);
    }
    (var port16, err) = strconv.ParseUint(port, 10, 16);
    if (err != default!) {
        return (ipp, errors.New("invalid port "u8 + strconv.Quote(port) + " parsing "u8 + strconv.Quote(s)));
    }
    ipp.port = (uint16)port16;
    (ipp.ip, err) = ParseAddr(ip);
    if (err != default!) {
        return (new AddrPort(nil), err);
    }
    if (v6 && ipp.ip.Is4()){
        return (new AddrPort(nil), errors.New("invalid ip:port "u8 + strconv.Quote(s) + ", square brackets can only be used with IPv6 addresses"u8));
    } else 
    if (!v6 && ipp.ip.Is6()) {
        return (new AddrPort(nil), errors.New("invalid ip:port "u8 + strconv.Quote(s) + ", IPv6 addresses must be surrounded by square brackets"u8));
    }
    return (ipp, default!);
}

// MustParseAddrPort calls [ParseAddrPort](s) and panics on error.
// It is intended for use in tests with hard-coded strings.
public static AddrPort MustParseAddrPort(@string s) {
    var (ip, err) = ParseAddrPort(s);
    if (err != default!) {
        throw panic(err);
    }
    return ip;
}

// IsValid reports whether p.Addr() is valid.
// All ports are valid, including zero.
public static bool IsValid(this AddrPort p) {
    return p.ip.IsValid();
}

// Compare returns an integer comparing two AddrPorts.
// The result will be 0 if p == p2, -1 if p < p2, and +1 if p > p2.
// AddrPorts sort first by IP address, then port.
public static nint Compare(this AddrPort p, AddrPort p2) {
    {
        nint c = p.Addr().Compare(p2.Addr()); if (c != 0) {
            return c;
        }
    }
    return cmp.Compare(p.Port(), p2.Port());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidAddrPortˢ = "invalid AddrPort"u8;

public static @string String(this AddrPort p) {
    slice<byte> b = default!;
    var exprᴛ1 = p.ip.z;
    if (exprᴛ1 == z0) {
        return invalidAddrPortˢ;
    }
    if (exprᴛ1 == z4) {
        const nint max = /* len("255.255.255.255:65535") */ 21;
        b = new slice<byte>(0, max);
        b = p.ip.appendTo4(b);
    }
    else { /* default: */
        if (p.ip.Is4In6()){
            const nint max = /* len("[::ffff:255.255.255.255%enp5s0]:65535") */ 37;
            b = new slice<byte>(0, max);
            b = append(b, (byte)((rune)'['));
            b = p.ip.appendTo4In6(b);
        } else {
            const nint max = /* len("[ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff%enp5s0]:65535") */ 54;
            b = new slice<byte>(0, max);
            b = append(b, (byte)((rune)'['));
            b = p.ip.appendTo6(b);
        }
        b = append(b, (byte)((rune)']'));
    }

    b = append(b, (byte)((rune)':'));
    b = strconv.AppendUint(b, (uint64)p.port, 10);
    return ((@string)b);
}

// AppendTo appends a text encoding of p,
// as generated by [AddrPort.MarshalText],
// to b and returns the extended buffer.
public static slice<byte> AppendTo(this AddrPort p, slice<byte> b) {
    var exprᴛ1 = p.ip.z;
    if (exprᴛ1 == z0) {
        return b;
    }
    if (exprᴛ1 == z4) {
        b = p.ip.appendTo4(b);
    }
    else { /* default: */
        b = append(b, (byte)((rune)'['));
        if (p.ip.Is4In6()){
            b = p.ip.appendTo4In6(b);
        } else {
            b = p.ip.appendTo6(b);
        }
        b = append(b, (byte)((rune)']'));
    }

    b = append(b, (byte)((rune)':'));
    b = strconv.AppendUint(b, (uint64)p.port, 10);
    return b;
}

// AppendText implements the [encoding.TextAppender] interface. The
// encoding is the same as returned by [AddrPort.AppendTo].
public static (slice<byte>, error) AppendText(this AddrPort p, slice<byte> b) {
    return (p.AppendTo(b), default!);
}

// MarshalText implements the [encoding.TextMarshaler] interface. The
// encoding is the same as returned by [AddrPort.String], with one exception: if
// p.Addr() is the zero [Addr], the encoding is the empty string.
public static (slice<byte>, error) MarshalText(this AddrPort p) {
    var buf = new byte[]{}.slice();
    var exprᴛ1 = p.ip.z;
    if (exprᴛ1 == z0) {
    }
    else if (exprᴛ1 == z4) {
        const nint maxCap = /* len("255.255.255.255:65535") */ 21;
        buf = new slice<byte>(0, maxCap);
    }
    else { /* default: */
        const nint maxCap = /* len("[ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff%enp5s0]:65535") */ 54;
        buf = new slice<byte>(0, maxCap);
    }

    return p.AppendText(buf);
}

// UnmarshalText implements the encoding.TextUnmarshaler
// interface. The [AddrPort] is expected in a form
// generated by [AddrPort.MarshalText] or accepted by [ParseAddrPort].
[GoRecv] public static error UnmarshalText(this ref AddrPort p, slice<byte> text) {
    if (len(text) == 0) {
        p = new AddrPort(nil);
        return default!;
    }
    error err = default!;
    (p, err) = ParseAddrPort(((@string)text));
    return err;
}

// AppendBinary implements the [encoding.BinaryAppendler] interface.
// It returns [Addr.AppendBinary] with an additional two bytes appended
// containing the port in little-endian.
public static (slice<byte>, error) AppendBinary(this AddrPort p, slice<byte> b) {
    (b, var err) = p.Addr().AppendBinary(b);
    if (err != default!) {
        return (default!, err);
    }
    return (byteorder.LEAppendUint16(b, p.Port()), default!);
}

// MarshalBinary implements the [encoding.BinaryMarshaler] interface.
// It returns [Addr.MarshalBinary] with an additional two bytes appended
// containing the port in little-endian.
public static (slice<byte>, error) MarshalBinary(this AddrPort p) {
    return p.AppendBinary(new slice<byte>(0, p.Addr().marshalBinarySize() + 2));
}

// UnmarshalBinary implements the [encoding.BinaryUnmarshaler] interface.
// It expects data in the form generated by [AddrPort.MarshalBinary].
[GoRecv] public static error UnmarshalBinary(this ref AddrPort p, slice<byte> b) {
    if (len(b) < 2) {
        return errors.New(unexpectedSliceSizeˢ);
    }
    ΔAddr addr = default!;
    var err = addr.UnmarshalBinary(b.slice(0, len(b) - 2));
    if (err != default!) {
        return err;
    }
    p = AddrPortFrom(addr, byteorder.LEUint16(b.slice(len(b) - 2)));
    return default!;
}

// Prefix is an IP address prefix (CIDR) representing an IP network.
//
// The first [Prefix.Bits]() of [Addr]() are specified. The remaining bits match any address.
// The range of Bits() is [0,32] for IPv4 or [0,128] for IPv6.
[GoType] partial struct ΔPrefix {
    internal ΔAddr ip;
    // bitsPlusOne stores the prefix bit length plus one.
    // A Prefix is valid if and only if bitsPlusOne is non-zero.
    internal uint8 bitsPlusOne;
}

// PrefixFrom returns a [Prefix] with the provided IP address and bit
// prefix length.
//
// It does not allocate. Unlike [Addr.Prefix], [PrefixFrom] does not mask
// off the host bits of ip.
//
// If bits is less than zero or greater than ip.BitLen, [Prefix.Bits]
// will return an invalid value -1.
public static ΔPrefix PrefixFrom(ΔAddr ip, nint bits) {
    uint8 bitsPlusOne = default!;
    if (!ip.isZero() && bits >= 0 && bits <= ip.BitLen()) {
        bitsPlusOne = (uint8)((uint8)bits + 1);
    }
    return new ΔPrefix(
        ip: ip.withoutZone(),
        bitsPlusOne: bitsPlusOne
    );
}

// Addr returns p's IP address.
public static ΔAddr Addr(this ΔPrefix p) {
    return p.ip;
}

// Bits returns p's prefix length.
//
// It reports -1 if invalid.
public static nint Bits(this ΔPrefix p) {
    return (nint)p.bitsPlusOne - 1;
}

// IsValid reports whether p.Bits() has a valid range for p.Addr().
// If p.Addr() is the zero [Addr], IsValid returns false.
// Note that if p is the zero [Prefix], then p.IsValid() == false.
public static bool IsValid(this ΔPrefix p) {
    return p.bitsPlusOne > 0;
}

internal static bool isZero(this ΔPrefix p) {
    return p == new ΔPrefix(nil);
}

// IsSingleIP reports whether p contains exactly one IP.
public static bool IsSingleIP(this ΔPrefix p) {
    return p.IsValid() && p.Bits() == p.ip.BitLen();
}

// compare returns an integer comparing two prefixes.
// The result will be 0 if p == p2, -1 if p < p2, and +1 if p > p2.
// Prefixes sort first by validity (invalid before valid), then
// address family (IPv4 before IPv6), then prefix length, then
// address.
//
// Unexported for Go 1.22 because we may want to compare by p.Addr first.
// See post-acceptance discussion on go.dev/issue/61642.
internal static nint compare(this ΔPrefix p, ΔPrefix p2) {
    {
        nint c = cmp.Compare(p.Addr().BitLen(), p2.Addr().BitLen()); if (c != 0) {
            return c;
        }
    }
    {
        nint c = cmp.Compare(p.Bits(), p2.Bits()); if (c != 0) {
            return c;
        }
    }
    return p.Addr().Compare(p2.Addr());
}

[GoType] partial struct parsePrefixError {
    internal @string @in; // the string given to ParsePrefix
    internal @string msg; // an explanation of the parse failure
}

internal static @string Error(this parsePrefixError err) {
    return "netip.ParsePrefix("u8 + strconv.Quote(err.@in) + "): "u8 + err.msg;
}

// ParsePrefix parses s as an IP address prefix.
// The string can be in the form "192.168.1.0/24" or "2001:db8::/32",
// the CIDR notation defined in RFC 4632 and RFC 4291.
// IPv6 zones are not permitted in prefixes, and an error will be returned if a
// zone is present.
//
// Note that masked address bits are not zeroed. Use Masked for that.
public static (ΔPrefix, error) ParsePrefix(@string s) {
    nint i = bytealg.LastIndexByteString(s, (rune)'/');
    if (i < 0) {
        return (new ΔPrefix(nil), new parsePrefixError(@in: s, msg: "no '/'"u8));
    }
    var (ip, err) = ParseAddr(s.slice(0, i));
    if (err != default!) {
        return (new ΔPrefix(nil), new parsePrefixError(@in: s, msg: err.Error()));
    }
    // IPv6 zones are not allowed: https://go.dev/issue/51899
    if (ip.Is6() && ip.z != z6noz) {
        return (new ΔPrefix(nil), new parsePrefixError(@in: s, msg: "IPv6 zones cannot be present in a prefix"u8));
    }
    @string bitsStr = s.slice(i + 1);
    // strconv.Atoi accepts a leading sign and leading zeroes, but we don't want that.
    if (len(bitsStr) > 1 && (bitsStr[0] < (rune)'1' || bitsStr[0] > (rune)'9')) {
        return (new ΔPrefix(nil), new parsePrefixError(@in: s, msg: "bad bits after slash: "u8 + strconv.Quote(bitsStr)));
    }
    (var bits, err) = strconv.Atoi(bitsStr);
    if (err != default!) {
        return (new ΔPrefix(nil), new parsePrefixError(@in: s, msg: "bad bits after slash: "u8 + strconv.Quote(bitsStr)));
    }
    nint maxBits = 32;
    if (ip.Is6()) {
        maxBits = 128;
    }
    if (bits < 0 || bits > maxBits) {
        return (new ΔPrefix(nil), new parsePrefixError(@in: s, msg: "prefix length out of range"u8));
    }
    return (PrefixFrom(ip, bits), default!);
}

// MustParsePrefix calls [ParsePrefix](s) and panics on error.
// It is intended for use in tests with hard-coded strings.
public static ΔPrefix MustParsePrefix(@string s) {
    var (ip, err) = ParsePrefix(s);
    if (err != default!) {
        throw panic(err);
    }
    return ip;
}

// Masked returns p in its canonical form, with all but the high
// p.Bits() bits of p.Addr() masked off.
//
// If p is zero or otherwise invalid, Masked returns the zero [Prefix].
public static ΔPrefix Masked(this ΔPrefix p) {
    var (m, _) = p.ip.Prefix(p.Bits());
    return m;
}

// Contains reports whether the network p includes ip.
//
// An IPv4 address will not match an IPv6 prefix.
// An IPv4-mapped IPv6 address will not match an IPv4 prefix.
// A zero-value IP will not match any prefix.
// If ip has an IPv6 zone, Contains returns false,
// because Prefixes strip zones.
public static bool Contains(this ΔPrefix p, ΔAddr ip) {
    if (!p.IsValid() || ip.hasZone()) {
        return false;
    }
    {
        nint f1 = p.ip.BitLen();
        nint f2 = ip.BitLen(); if (f1 == 0 || f2 == 0 || f1 != f2) {
            return false;
        }
    }
    if (ip.Is4()){
        // xor the IP addresses together; mismatched bits are now ones.
        // Shift away the number of bits we don't care about.
        // Shifts in Go are more efficient if the compiler can prove
        // that the shift amount is smaller than the width of the shifted type (64 here).
        // We know that p.bits is in the range 0..32 because p is Valid;
        // the compiler doesn't know that, so mask with 63 to help it.
        // Now truncate to 32 bits, because this is IPv4.
        // If all the bits we care about are equal, the result will be zero.
        return (uint32)((((uint64)(ip.addr.lo ^ p.ip.addr.lo)) >> (int)(((nint)((32 - p.Bits()) & 63))))) == 0;
    } else {
        // xor the IP addresses together.
        // Mask away the bits we don't care about.
        // If all the bits we care about are equal, the result will be zero.
        return ip.addr.xor(p.ip.addr).and(mask6(p.Bits())).isZero();
    }
}

// Overlaps reports whether p and o contain any IP addresses in common.
//
// If p and o are of different address families or either have a zero
// IP, it reports false. Like the Contains method, a prefix with an
// IPv4-mapped IPv6 address is still treated as an IPv6 mask.
public static bool Overlaps(this ΔPrefix p, ΔPrefix o) {
    if (!p.IsValid() || !o.IsValid()) {
        return false;
    }
    if (p == o) {
        return true;
    }
    if (p.ip.Is4() != o.ip.Is4()) {
        return false;
    }
    nint minBits = default!;
    {
        nint pb = p.Bits();
        nint ob = o.Bits(); if (pb < ob){
            minBits = pb;
        } else {
            minBits = ob;
        }
    }
    if (minBits == 0) {
        return true;
    }
    // One of these Prefix calls might look redundant, but we don't require
    // that p and o values are normalized (via Prefix.Masked) first,
    // so the Prefix call on the one that's already minBits serves to zero
    // out any remaining bits in IP.
    error err = default!;
    {
        (p, err) = p.ip.Prefix(minBits); if (err != default!) {
            return false;
        }
    }
    {
        (o, err) = o.ip.Prefix(minBits); if (err != default!) {
            return false;
        }
    }
    return p.ip == o.ip;
}

// AppendTo appends a text encoding of p,
// as generated by [Prefix.MarshalText],
// to b and returns the extended buffer.
public static slice<byte> AppendTo(this ΔPrefix p, slice<byte> b) {
    if (p.isZero()) {
        return b;
    }
    if (!p.IsValid()) {
        return append(b, ((@string)"invalid Prefix"u8).ꓸꓸꓸ);
    }
    // p.ip is non-nil, because p is valid.
    if (p.ip.z == z4){
        b = p.ip.appendTo4(b);
    } else {
        if (p.ip.Is4In6()){
            b = append(b, ((@string)"::ffff:"u8).ꓸꓸꓸ);
            b = p.ip.Unmap().appendTo4(b);
        } else {
            b = p.ip.appendTo6(b);
        }
    }
    b = append(b, (byte)((rune)'/'));
    b = appendDecimal(b, (uint8)p.Bits());
    return b;
}

// AppendText implements the [encoding.TextAppender] interface.
// It is the same as [Prefix.AppendTo].
public static (slice<byte>, error) AppendText(this ΔPrefix p, slice<byte> b) {
    return (p.AppendTo(b), default!);
}

// MarshalText implements the [encoding.TextMarshaler] interface,
// The encoding is the same as returned by [Prefix.String], with one exception:
// If p is the zero value, the encoding is the empty string.
public static (slice<byte>, error) MarshalText(this ΔPrefix p) {
    var buf = new byte[]{}.slice();
    var exprᴛ1 = p.ip.z;
    if (exprᴛ1 == z0) {
    }
    else if (exprᴛ1 == z4) {
        const nint maxCap = /* len("255.255.255.255/32") */ 18;
        buf = new slice<byte>(0, maxCap);
    }
    else { /* default: */
        const nint maxCap = /* len("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff%enp5s0/128") */ 50;
        buf = new slice<byte>(0, maxCap);
    }

    return p.AppendText(buf);
}

// UnmarshalText implements the encoding.TextUnmarshaler interface.
// The IP address is expected in a form accepted by [ParsePrefix]
// or generated by [Prefix.MarshalText].
[GoRecv] public static error UnmarshalText(this ref ΔPrefix p, slice<byte> text) {
    if (len(text) == 0) {
        p = new ΔPrefix(nil);
        return default!;
    }
    error err = default!;
    (p, err) = ParsePrefix(((@string)text));
    return err;
}

// AppendBinary implements the [encoding.AppendMarshaler] interface.
// It returns [Addr.AppendBinary] with an additional byte appended
// containing the prefix bits.
public static (slice<byte>, error) AppendBinary(this ΔPrefix p, slice<byte> b) {
    (b, var err) = p.Addr().withoutZone().AppendBinary(b);
    if (err != default!) {
        return (default!, err);
    }
    return (append(b, (uint8)p.Bits()), default!);
}

// MarshalBinary implements the [encoding.BinaryMarshaler] interface.
// It returns [Addr.MarshalBinary] with an additional byte appended
// containing the prefix bits.
public static (slice<byte>, error) MarshalBinary(this ΔPrefix p) {
    // without the zone the max length is 16, plus an additional byte is 17
    return p.AppendBinary(new slice<byte>(0, p.Addr().withoutZone().marshalBinarySize() + 1));
}

// UnmarshalBinary implements the [encoding.BinaryUnmarshaler] interface.
// It expects data in the form generated by [Prefix.MarshalBinary].
[GoRecv] public static error UnmarshalBinary(this ref ΔPrefix p, slice<byte> b) {
    if (len(b) < 1) {
        return errors.New(unexpectedSliceSizeˢ);
    }
    ΔAddr addr = default!;
    var err = addr.UnmarshalBinary(b.slice(0, len(b) - 1));
    if (err != default!) {
        return err;
    }
    p = PrefixFrom(addr, (nint)b[len(b) - 1]);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidPrefixˢ = "invalid Prefix"u8;

// String returns the CIDR notation of p: "<ip>/<bits>".
public static @string String(this ΔPrefix p) {
    if (!p.IsValid()) {
        return invalidPrefixˢ;
    }
    return p.ip.String() + "/"u8 + itoa.Itoa(p.Bits());
}

} // end netip_package
