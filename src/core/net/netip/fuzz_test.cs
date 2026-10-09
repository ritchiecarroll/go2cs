// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bytes = bytes_package;
using encoding = encoding_package;
using fmt = fmt_package;
using net = net_package;
using static go.net.netip_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using go.net;
using netip = go.net.netip_package;

partial class netip_test_package {

// Basic zero IPv4 address.
// Basic non-zero IPv4 address.
// IPv4 address in windows-style "print all the digits" form.
// IPv4 address with a silly amount of leading zeros.
// 4-in-6 with octet with leading zero
// Basic zero IPv6 address.
// Localhost IPv6.
// Fully expanded IPv6 address.
// IPv6 with elided fields in the middle.
// IPv6 with elided fields at the end.
// IPv6 with single elided field at the end.
// IPv6 with single elided field in the middle.
// IPv6 with the trailing 32 bits written as IPv4 dotted decimal. (4in6)
// IPv6 with a zone specifier.
// IPv6 with dotted decimal and zone specifier.
// IPv6 with capital letters.
// Empty string.
// Garbage non-IP.
// Single number. Some parsers accept this as an IPv4 address in
// big-endian uint32 form, but we don't.
// IPv4 with a zone specifier.
// IPv4 field must have at least one digit.
// IPv4 address too long.
// IPv4 in dotted octal form.
// IPv4 in dotted hex form.
// IPv4 in class B form.
// IPv4 in class B form, with a small enough number to be
// parseable as a regular dotted decimal field.
// IPv4 in class A form.
// IPv4 in class A form, with a small enough number to be
// parseable as a regular dotted decimal field.
// IPv4 field has value >255.
// IPv4 with too many fields.
// IPv6 with not enough fields.
// IPv6 with too many fields.
// IPv6 with 8 fields and a :: expander.
// IPv6 with a field bigger than 2b.
// IPv6 with non-hex values in field.
// IPv6 with a zone delimiter but no zone.
// IPv6 with a zone specifier of zero.
// IPv6 (without ellipsis) with too many fields for trailing embedded IPv4.
// IPv6 (with ellipsis) with too many fields for trailing embedded IPv4.
// IPv6 with invalid embedded IPv4.
// IPv6 with multiple ellipsis ::.
// IPv6 with invalid non hex/colon character.
// IPv6 with truncated bytes after single colon.
// AddrPort strings.
// Prefix strings.
internal static slice<@string> corpus = new @string[]{
    "0.0.0.0"u8,
    "192.168.140.255"u8,
    "010.000.015.001"u8,
    "000001.00000002.00000003.000000004"u8,
    "::ffff:1.2.03.4"u8,
    "::"u8,
    "::1"u8,
    "fd7a:115c:a1e0:ab12:4843:cd96:626b:430b"u8,
    "fd7a:115c::626b:430b"u8,
    "fd7a:115c:a1e0:ab12:4843:cd96::"u8,
    "fd7a:115c:a1e0:ab12:4843:cd96:626b::"u8,
    "fd7a:115c:a1e0:ab12:4843:cd96:626b:0"u8,
    "fd7a:115c:a1e0::4843:cd96:626b:430b"u8,
    "fd7a:115c:a1e0:0:4843:cd96:626b:430b"u8,
    "::ffff:192.168.140.255"u8,
    "::ffff:192.168.140.255"u8,
    "fd7a:115c:a1e0:ab12:4843:cd96:626b:430b%eth0"u8,
    "1:2::ffff:192.168.140.255%eth1"u8,
    "1:2::ffff:c0a8:8cff%eth1"u8,
    "FD9E:1A04:F01D::1"u8,
    "fd9e:1a04:f01d::1"u8,
    ""u8,
    "bad"u8,
    "1234"u8,
    "1.2.3.4%eth0"u8,
    ".1.2.3"u8,
    "1.2.3."u8,
    "1..2.3"u8,
    "1.2.3.4.5"u8,
    "0300.0250.0214.0377"u8,
    "0xc0.0xa8.0x8c.0xff"u8,
    "192.168.12345"u8,
    "127.0.1"u8,
    "192.1234567"u8,
    "127.1"u8,
    "192.168.300.1"u8,
    "192.168.0.1.5.6"u8,
    "1:2:3:4:5:6:7"u8,
    "1:2:3:4:5:6:7:8:9"u8,
    "1:2:3:4::5:6:7:8"u8,
    "fe801::1"u8,
    "fe80:tail:scal:e::"u8,
    "fe80::1%"u8,
    "::ffff:0:0%0"u8,
    "ffff:ffff:ffff:ffff:ffff:ffff:ffff:192.168.140.255"u8,
    "ffff::ffff:ffff:ffff:ffff:ffff:ffff:192.168.140.255"u8,
    "::ffff:192.168.140.bad"u8,
    "fe80::1::1"u8,
    "fe80:1?:1"u8,
    "fe80:"u8,
    "1.2.3.4:51820"u8,
    "[fd7a:115c:a1e0:ab12:4843:cd96:626b:430b]:80"u8,
    "[::ffff:c000:0280]:65535"u8,
    "[::ffff:c000:0280%eth0]:1"u8,
    "1.2.3.4/24"u8,
    "fd7a:115c:a1e0:ab12:4843:cd96:626b:430b/118"u8,
    "::ffff:c000:0280/96"u8,
    "::ffff:c000:0280%eth0/37"u8
}.slice();

public static void FuzzParse(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, seed) in corpus) {
        f.Add(seed);
    }
    Ꮡf.Fuzz((ж<testing.T> t, @string s) => {
        var (ip, _) = ParseAddr(s);
        checkStringParseRoundTrip<netipꓸAddr>(t, ip, ParseAddr);
        checkEncoding(t, ip);
        // Check that we match the net's IP parser, modulo zones.
        if (!strings.Contains(s, "%"u8)) {
            var stdip = net.ParseIP(s);
            if (!ip.IsValid() != (stdip == default!)) {
                t.Errorf("ParseAddr zero != net.ParseIP nil: ip=%q stdip=%q"u8, ip, stdip);
            }
            if (ip.IsValid() && !ip.Is4In6()) {
                var (buf, errΔ1) = ip.MarshalText();
                if (errΔ1 != default!) {
                    t.Fatal(errΔ1);
                }
                (var buf2, errΔ1) = stdip.MarshalText();
                if (errΔ1 != default!) {
                    t.Fatal(errΔ1);
                }
                if (!bytes.Equal(buf, buf2)) {
                    t.Errorf("Addr.MarshalText() != net.IP.MarshalText(): ip=%q stdip=%q"u8, ip, stdip);
                }
                if (ip.String() != stdip.String()) {
                    t.Errorf("Addr.String() != net.IP.String(): ip=%q stdip=%q"u8, ip, stdip);
                }
                if (ip.IsGlobalUnicast() != stdip.IsGlobalUnicast()) {
                    t.Errorf("Addr.IsGlobalUnicast() != net.IP.IsGlobalUnicast(): ip=%q stdip=%q"u8, ip, stdip);
                }
                if (ip.IsInterfaceLocalMulticast() != stdip.IsInterfaceLocalMulticast()) {
                    t.Errorf("Addr.IsInterfaceLocalMulticast() != net.IP.IsInterfaceLocalMulticast(): ip=%q stdip=%q"u8, ip, stdip);
                }
                if (ip.IsLinkLocalMulticast() != stdip.IsLinkLocalMulticast()) {
                    t.Errorf("Addr.IsLinkLocalMulticast() != net.IP.IsLinkLocalMulticast(): ip=%q stdip=%q"u8, ip, stdip);
                }
                if (ip.IsLinkLocalUnicast() != stdip.IsLinkLocalUnicast()) {
                    t.Errorf("Addr.IsLinkLocalUnicast() != net.IP.IsLinkLocalUnicast(): ip=%q stdip=%q"u8, ip, stdip);
                }
                if (ip.IsLoopback() != stdip.IsLoopback()) {
                    t.Errorf("Addr.IsLoopback() != net.IP.IsLoopback(): ip=%q stdip=%q"u8, ip, stdip);
                }
                if (ip.IsMulticast() != stdip.IsMulticast()) {
                    t.Errorf("Addr.IsMulticast() != net.IP.IsMulticast(): ip=%q stdip=%q"u8, ip, stdip);
                }
                if (ip.IsPrivate() != stdip.IsPrivate()) {
                    t.Errorf("Addr.IsPrivate() != net.IP.IsPrivate(): ip=%q stdip=%q"u8, ip, stdip);
                }
                if (ip.IsUnspecified() != stdip.IsUnspecified()) {
                    t.Errorf("Addr.IsUnspecified() != net.IP.IsUnspecified(): ip=%q stdip=%q"u8, ip, stdip);
                }
            }
        }
        // Check that .Next().Prev() and .Prev().Next() preserve the IP.
        if (ip.IsValid() && ip.Next().IsValid() && ip.Next().Prev() != ip) {
            t.Errorf(".Next.Prev did not round trip: ip=%q .next=%q .next.prev=%q"u8, ip, ip.Next(), ip.Next().Prev());
        }
        if (ip.IsValid() && ip.Prev().IsValid() && ip.Prev().Next() != ip) {
            t.Errorf(".Prev.Next did not round trip: ip=%q .prev=%q .prev.next=%q"u8, ip, ip.Prev(), ip.Prev().Next());
        }
        var (port, err) = ParseAddrPort(s);
        if (err == default!) {
            checkStringParseRoundTrip<netip.AddrPort>(t, port, ParseAddrPort);
            checkEncoding(t, port);
        }
        port = AddrPortFrom(ip, 80);
        checkStringParseRoundTrip<netip.AddrPort>(t, port, ParseAddrPort);
        checkEncoding(t, port);
        (var ipp, err) = ParsePrefix(s);
        if (err == default!) {
            checkStringParseRoundTrip<netipꓸPrefix>(t, ipp, ParsePrefix);
            checkEncoding(t, ipp);
        }
        ipp = PrefixFrom(ip, 8);
        checkStringParseRoundTrip<netipꓸPrefix>(t, ipp, ParsePrefix);
        checkEncoding(t, ipp);
    });
}

// checkTextMarshaler checks that x's MarshalText and UnmarshalText functions round trip correctly.
internal static void checkTextMarshaler(ж<testing.T> Ꮡt, encoding.TextMarshaler x) {
    var (buf, err) = x.MarshalText();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var y = reflect.New(reflect.TypeOf(x)).Interface()._<encoding.TextUnmarshaler>();
    err = y.UnmarshalText(buf);
    if (err != default!) {
        Ꮡt.Logf("(%v).MarshalText() = %q"u8, x, buf);
        Ꮡt.Fatalf("(%T).UnmarshalText(%q) = %v"u8, y, buf, err);
    }
    var e = reflect.ValueOf(y).Elem().Interface();
    if (!reflect.DeepEqual(x, e)) {
        Ꮡt.Logf("(%v).MarshalText() = %q"u8, x, buf);
        Ꮡt.Logf("(%T).UnmarshalText(%q) = %v"u8, y, buf, y);
        Ꮡt.Fatalf("MarshalText/UnmarshalText failed to round trip: %#v != %#v"u8, x, e);
    }
    (var buf2, err) = y._<encoding.TextMarshaler>().MarshalText();
    if (err != default!) {
        Ꮡt.Logf("(%v).MarshalText() = %q"u8, x, buf);
        Ꮡt.Logf("(%T).UnmarshalText(%q) = %v"u8, y, buf, y);
        Ꮡt.Fatalf("failed to MarshalText a second time: %v"u8, err);
    }
    if (!bytes.Equal(buf, buf2)) {
        Ꮡt.Logf("(%v).MarshalText() = %q"u8, x, buf);
        Ꮡt.Logf("(%T).UnmarshalText(%q) = %v"u8, y, buf, y);
        Ꮡt.Logf("(%v).MarshalText() = %q"u8, y, buf2);
        Ꮡt.Fatalf("second MarshalText differs from first: %q != %q"u8, buf, buf2);
    }
}

// checkBinaryMarshaler checks that x's MarshalText and UnmarshalText functions round trip correctly.
internal static void checkBinaryMarshaler(ж<testing.T> Ꮡt, encoding.BinaryMarshaler x) {
    var (buf, err) = x.MarshalBinary();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var y = reflect.New(reflect.TypeOf(x)).Interface()._<encoding.BinaryUnmarshaler>();
    err = y.UnmarshalBinary(buf);
    if (err != default!) {
        Ꮡt.Logf("(%v).MarshalBinary() = %q"u8, x, buf);
        Ꮡt.Fatalf("(%T).UnmarshalBinary(%q) = %v"u8, y, buf, err);
    }
    var e = reflect.ValueOf(y).Elem().Interface();
    if (!reflect.DeepEqual(x, e)) {
        Ꮡt.Logf("(%v).MarshalBinary() = %q"u8, x, buf);
        Ꮡt.Logf("(%T).UnmarshalBinary(%q) = %v"u8, y, buf, y);
        Ꮡt.Fatalf("MarshalBinary/UnmarshalBinary failed to round trip: %#v != %#v"u8, x, e);
    }
    (var buf2, err) = y._<encoding.BinaryMarshaler>().MarshalBinary();
    if (err != default!) {
        Ꮡt.Logf("(%v).MarshalBinary() = %q"u8, x, buf);
        Ꮡt.Logf("(%T).UnmarshalBinary(%q) = %v"u8, y, buf, y);
        Ꮡt.Fatalf("failed to MarshalBinary a second time: %v"u8, err);
    }
    if (!bytes.Equal(buf, buf2)) {
        Ꮡt.Logf("(%v).MarshalBinary() = %q"u8, x, buf);
        Ꮡt.Logf("(%T).UnmarshalBinary(%q) = %v"u8, y, buf, y);
        Ꮡt.Logf("(%v).MarshalBinary() = %q"u8, y, buf2);
        Ꮡt.Fatalf("second MarshalBinary differs from first: %q != %q"u8, buf, buf2);
    }
}

internal static void checkTextMarshalMatchesString(ж<testing.T> Ꮡt, netipType x) {
    var (buf, err) = x.MarshalText();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string str = x.String();
    if (((sstring)buf) != str) {
        Ꮡt.Fatalf("%v: MarshalText = %q, String = %q"u8, x, buf, str);
    }
}

partial interface appendMarshaler :
    encoding.TextMarshaler
{
    slice<byte> AppendTo(slice<byte> _);
}

// checkTextMarshalMatchesAppendTo checks that x's MarshalText matches x's AppendTo.
internal static void checkTextMarshalMatchesAppendTo(ж<testing.T> Ꮡt, appendMarshaler x) {
    var (buf, err) = x.MarshalText();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var buf2 = new slice<byte>(0, len(buf));
    buf2 = x.AppendTo(buf2);
    if (!bytes.Equal(buf, buf2)) {
        Ꮡt.Fatalf("%v: MarshalText = %q, AppendTo = %q"u8, x, buf, buf2);
    }
}

partial interface netipType :
    encoding.BinaryMarshaler,
    encoding.TextMarshaler,
    fmt.Stringer
{
    bool IsValid();
}

partial interface netipTypeCmp :
    netipType
{
}

// checkStringParseRoundTrip checks that x's String method and the provided parse function can round trip correctly.
internal static void checkStringParseRoundTrip<P>(ж<testing.T> Ꮡt, P x, Func<@string, (P, error)> parse)
    where P : netipTypeCmp
{
    if (!x.IsValid()) {
        // Ignore invalid values.
        return;
    }
    @string s = x.String();
    var (y, err) = parse(s);
    if (err != default!) {
        Ꮡt.Fatalf("s=%q err=%v"u8, s, err);
    }
    if (!AreEqual(x, y)) {
        Ꮡt.Fatalf("%T round trip identity failure: s=%q x=%#v y=%#v"u8, x, s, x, y);
    }
    @string s2 = y.String();
    if (s != s2) {
        Ꮡt.Fatalf("%T String round trip identity failure: s=%#v s2=%#v"u8, x, s, s2);
    }
}

internal static void checkEncoding(ж<testing.T> Ꮡt, netipType x) {
    if (x.IsValid()) {
        checkTextMarshaler(Ꮡt, x);
        checkBinaryMarshaler(Ꮡt, x);
        checkTextMarshalMatchesString(Ꮡt, x);
    }
    {
        var (am, ok) = x._<appendMarshaler>(ᐧ); if (ok) {
            checkTextMarshalMatchesAppendTo(Ꮡt, am);
        }
    }
}

} // end netip_test_package
