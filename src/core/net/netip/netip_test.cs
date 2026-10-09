// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using uint128 = go.net.netip_package.uint128;

namespace go.net;

using bytes = bytes_package;
using json = go.encoding.json_package;
using flag = flag_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using testenv = @internal.testenv_package;
using net = net_package;
using static go.net.netip_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using unique = unique_package;
using @internal;
using go.encoding;
using go.net;
using netip = go.net.netip_package;
using ꓸꓸꓸstring = Span<@string>;

partial class netip_test_package {

internal static ж<bool> @long = flag.Bool("long"u8, false, "run long tests"u8);

internal static Func<@string, netipꓸPrefix> mustPrefix = MustParsePrefix;
internal static Func<@string, netipꓸAddr> mustIP = MustParseAddr;
internal static Func<@string, netip.AddrPort> mustIPPort = MustParseAddrPort;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string eth0ˢ = "eth0"u8;
internal static readonly @string eth1ˢ = "eth1"u8;

internal partial struct TestParseAddr_type /*dyn*/ {
    internal @string @in;
    internal netipꓸAddr ip;   // output of ParseAddr()
    internal @string str; // output of String(). If "", use in.
    internal @string wantErr;
}

public static void TestParseAddr(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

// Basic zero IPv4 address.
// Basic non-zero IPv4 address.
// IPv4 address in windows-style "print all the digits" form.
// IPv4 address with a silly amount of leading zeros.
// 4-in-6 with octet with leading zero
// 4-in-6 with octet with unexpected character
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
// 4-in-6 with zone
// IPv6 with capital letters.
    slice<TestParseAddr_type> validIPs = new TestParseAddr_type[]{
        new(
            @in: "0.0.0.0"u8,
            ip: MkAddr(Mk128(0, 0xffff00000000UL), Z4)
        ),
        new(
            @in: "192.168.140.255"u8,
            ip: MkAddr(Mk128(0, 0xffffc0a88cffUL), Z4)
        ),
        new(
            @in: "010.000.015.001"u8,
            wantErr: @"ParseAddr(""010.000.015.001""): IPv4 field has octet with leading zero"u8
        ),
        new(
            @in: "000001.00000002.00000003.000000004"u8,
            wantErr: @"ParseAddr(""000001.00000002.00000003.000000004""): IPv4 field has octet with leading zero"u8
        ),
        new(
            @in: "::ffff:1.2.03.4"u8,
            wantErr: @"ParseAddr(""::ffff:1.2.03.4""): IPv4 field has octet with leading zero"u8
        ),
        new(
            @in: "::ffff:1.2.3.z"u8,
            wantErr: @"ParseAddr(""::ffff:1.2.3.z""): unexpected character (at ""z"")"u8
        ),
        new(
            @in: "::"u8,
            ip: MkAddr(Mk128(0, 0), Z6noz)
        ),
        new(
            @in: "::1"u8,
            ip: MkAddr(Mk128(0, 1), Z6noz)
        ),
        new(
            @in: "fd7a:115c:a1e0:ab12:4843:cd96:626b:430b"u8,
            ip: MkAddr(Mk128(0xfd7a115ca1e0ab12UL, 0x4843cd96626b430bUL), Z6noz)
        ),
        new(
            @in: "fd7a:115c::626b:430b"u8,
            ip: MkAddr(Mk128(0xfd7a115c00000000UL, 0x00000000626b430b), Z6noz)
        ),
        new(
            @in: "fd7a:115c:a1e0:ab12:4843:cd96::"u8,
            ip: MkAddr(Mk128(0xfd7a115ca1e0ab12UL, 0x4843cd9600000000UL), Z6noz)
        ),
        new(
            @in: "fd7a:115c:a1e0:ab12:4843:cd96:626b::"u8,
            ip: MkAddr(Mk128(0xfd7a115ca1e0ab12UL, 0x4843cd96626b0000UL), Z6noz),
            str: "fd7a:115c:a1e0:ab12:4843:cd96:626b:0"u8
        ),
        new(
            @in: "fd7a:115c:a1e0::4843:cd96:626b:430b"u8,
            ip: MkAddr(Mk128(0xfd7a115ca1e00000UL, 0x4843cd96626b430bUL), Z6noz),
            str: "fd7a:115c:a1e0:0:4843:cd96:626b:430b"u8
        ),
        new(
            @in: "::ffff:192.168.140.255"u8,
            ip: MkAddr(Mk128(0, 0x0000ffffc0a88cffUL), Z6noz),
            str: "::ffff:192.168.140.255"u8
        ),
        new(
            @in: "fd7a:115c:a1e0:ab12:4843:cd96:626b:430b%eth0"u8,
            ip: MkAddr(Mk128(0xfd7a115ca1e0ab12UL, 0x4843cd96626b430bUL), unique.Make<AddrDetail>(MakeAddrDetail(true, eth0ˢ)))
        ),
        new(
            @in: "1:2::ffff:192.168.140.255%eth1"u8,
            ip: MkAddr(Mk128(0x0001000200000000UL, 0x0000ffffc0a88cffUL), unique.Make<AddrDetail>(MakeAddrDetail(true, eth1ˢ))),
            str: "1:2::ffff:c0a8:8cff%eth1"u8
        ),
        new(
            @in: "::ffff:192.168.140.255%eth1"u8,
            ip: MkAddr(Mk128(0, 0x0000ffffc0a88cffUL), unique.Make<AddrDetail>(MakeAddrDetail(true, eth1ˢ))),
            str: "::ffff:192.168.140.255%eth1"u8
        ),
        new(
            @in: "FD9E:1A04:F01D::1"u8,
            ip: MkAddr(Mk128(0xfd9e1a04f01d0000UL, 0x1), Z6noz),
            str: "fd9e:1a04:f01d::1"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in validIPs) {
        ref var test = ref heap(new TestParseAddr_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.@in, (ж<testing.T> tΔ1) => {
            var (got, err) = ParseAddr(testʗ1.@in);
            if (err != default!) {
                if (err.Error() == testʗ1.wantErr) {
                    return;
                }
                tΔ1.Fatal(err);
            }
            if (testʗ1.wantErr != ""u8) {
                tΔ1.Fatalf("wanted error %q; got none"u8, testʗ1.wantErr);
            }
            if (got != testʗ1.ip) {
                tΔ1.Errorf("got %#v, want %#v"u8, got, testʗ1.ip);
            }
            // Check that ParseAddr is a pure function.
            (var got2, err) = ParseAddr(testʗ1.@in);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            if (got != got2) {
                tΔ1.Errorf("ParseAddr(%q) got 2 different results: %#v, %#v"u8, testʗ1.@in, got, got2);
            }
            // Check that ParseAddr(ip.String()) is the identity function.
            @string s = got.String();
            (var got3, err) = ParseAddr(s);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            if (got != got3) {
                tΔ1.Errorf("ParseAddr(%q) != ParseAddr(ParseIP(%q).String()). Got %#v, want %#v"u8, testʗ1.@in, testʗ1.@in, got3, got);
            }
            // Check that the slow-but-readable parser produces the same result.
            (var slow, err) = parseIPSlow(testʗ1.@in);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            if (got != slow) {
                tΔ1.Errorf("ParseAddr(%q) = %#v, parseIPSlow(%q) = %#v"u8, testʗ1.@in, got, testʗ1.@in, slow);
            }
            // Check that the parsed IP formats as expected.
            s = got.String();
            @string wants = testʗ1.str;
            if (wants == ""u8) {
                wants = testʗ1.@in;
            }
            if (s != wants) {
                tΔ1.Errorf("ParseAddr(%q).String() got %q, want %q"u8, testʗ1.@in, s, wants);
            }
            // Check that AppendTo matches MarshalText.
            TestAppendToMarshal(tΔ1, got);
            // Check that MarshalText/UnmarshalText work similarly to
            // ParseAddr/String (see TestIPMarshalUnmarshal for
            // marshal-specific behavior that's not common with
            // ParseAddr/String).
            @string js = @""""u8 + testʗ1.@in + @""""u8;
            ref var jsgot = ref heap(new netipꓸAddr(), out var Ꮡjsgot);
            {
                var errΔ1 = json.Unmarshal(slice<byte>(js), Ꮡjsgot); if (errΔ1 != default!) {
                    tΔ1.Fatal(errΔ1);
                }
            }
            if (jsgot != got) {
                tΔ1.Errorf("json.Unmarshal(%q) = %#v, want %#v"u8, testʗ1.@in, jsgot, got);
            }
            (var jsb, err) = json.Marshal(jsgot);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            @string jswant = @""""u8 + wants + @""""u8;
            @string jsback = ((@string)jsb);
            if (jsback != jswant) {
                tΔ1.Errorf("Marshal(Unmarshal(%q)) = %s, want %s"u8, testʗ1.@in, jsback, jswant);
            }
        });
    }
// Empty string
// Garbage non-IP
// Single number. Some parsers accept this as an IPv4 address in
// big-endian uint32 form, but we don't.
// IPv4 with a zone specifier
// IPv4 field must have at least one digit
// IPv4 address too long
// IPv4 in dotted octal form
// IPv4 in dotted hex form
// IPv4 in class B form
// IPv4 in class B form, with a small enough number to be
// parseable as a regular dotted decimal field.
// IPv4 in class A form
// IPv4 in class A form, with a small enough number to be
// parseable as a regular dotted decimal field.
// IPv4 field has value >255
// IPv4 with too many fields
// IPv6 with not enough fields
// IPv6 with too many fields
// IPv6 with 8 fields and a :: expander
// IPv6 with a field bigger than 2b
// IPv6 with non-hex values in field
// IPv6 with a zone delimiter but no zone.
// IPv6 (without ellipsis) with too many fields for trailing embedded IPv4.
// IPv6 (with ellipsis) with too many fields for trailing embedded IPv4.
// IPv6 with invalid embedded IPv4.
// IPv6 with multiple ellipsis ::.
// IPv6 with invalid non hex/colon character.
// IPv6 with truncated bytes after single colon.
// IPv6 with 5 zeros in last group
// IPv6 with 5 zeros in one group and embedded IPv4
    slice<@string> invalidIPs = new @string[]{
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
        "ffff:ffff:ffff:ffff:ffff:ffff:ffff:192.168.140.255"u8,
        "ffff::ffff:ffff:ffff:ffff:ffff:ffff:192.168.140.255"u8,
        "::ffff:192.168.140.bad"u8,
        "fe80::1::1"u8,
        "fe80:1?:1"u8,
        "fe80:"u8,
        "0:0:0:0:0:ffff:0:00000"u8,
        "0:0:0:0:00000:ffff:127.1.2.3"u8
    }.slice();
    foreach (var (_, s) in invalidIPs) {
        Ꮡt.Run(s, (ж<testing.T> tΔ2) => {
            var (got, err) = ParseAddr(s);
            if (err == default!) {
                tΔ2.Errorf("ParseAddr(%q) = %#v, want error"u8, s, got);
            }
            (var slow, err) = parseIPSlow(s);
            if (err == default!) {
                tΔ2.Errorf("parseIPSlow(%q) = %#v, want error"u8, s, slow);
            }
            var std = net.ParseIP(s);
            if (std != default!) {
                tΔ2.Errorf("net.ParseIP(%q) = %#v, want error"u8, s, std);
            }
            if (s == ""u8) {
                // Don't test unmarshaling of "" here, do it in
                // IPMarshalUnmarshal.
                return;
            }
            ref var jsgot = ref heap(new netipꓸAddr(), out var Ꮡjsgot);
            var js = slice<byte>(@"""" + s + @"""");
            {
                var errΔ1 = json.Unmarshal(js, Ꮡjsgot); if (errΔ1 == default!) {
                    tΔ2.Errorf("json.Unmarshal(%q) = %#v, want error"u8, s, jsgot);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fe8001ˢ = "fe80::01"u8;

internal partial struct TestAddrFromSlice_tests /*dyn*/ {
    internal slice<byte> ip;
    internal netipꓸAddr wantAddr;
    internal bool wantOK;
}

public static void TestAddrFromSlice(ж<testing.T> Ꮡt) {
    var tests = new TestAddrFromSlice_tests[]{
        new(
            ip: new byte[]{10, 0, 0, 1}.slice(),
            wantAddr: mustIP("10.0.0.1"u8),
            wantOK: true
        ),
        new(
            ip: new slice<byte>(16){[0] = 0xfe, [1] = 0x80, [15] = 0x01},
            wantAddr: mustIP(fe8001ˢ),
            wantOK: true
        ),
        new(
            ip: new byte[]{0, 1, 2}.slice(),
            wantAddr: new netipꓸAddr(nil),
            wantOK: false
        ),
        new(
            ip: default!,
            wantAddr: new netipꓸAddr(nil),
            wantOK: false
        )
    }.slice();
    foreach (var (_, tt) in tests) {
        var (addr, ok) = AddrFromSlice(tt.ip);
        if (ok != tt.wantOK || addr != tt.wantAddr) {
            Ꮡt.Errorf("AddrFromSlice(%#v) = %#v, %v, want %#v, %v"u8, tt.ip, addr, ok, tt.wantAddr, tt.wantOK);
        }
    }
}

public static void TestIPv4Constructors(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (AddrFrom4(new byte[]{1, 2, 3, 4}.array()) != MustParseAddr("1.2.3.4"u8)) {
        Ꮡt.Errorf("don't match"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fd7a115cA1e0Ab124843Cd96ˢ = "fd7a:115c:a1e0:ab12:4843:cd96:626b:430b"u8;
internal static readonly @string ffff192168140255ˢ = "::ffff:192.168.140.255"u8;
internal static readonly @string ffff192168140255En0ˢ = "::ffff:192.168.140.255%en0"u8;

internal partial struct TestAddrAppendText_tests /*dyn*/ {
    internal netipꓸAddr ip;
    internal @string want;
}

public static void TestAddrAppendText(ж<testing.T> Ꮡt) {
    var tests = new TestAddrAppendText_tests[]{
        new(new netipꓸAddr(nil), ""u8), // zero IP

        new(mustIP("1.2.3.4"u8), "1.2.3.4"u8),
        new(mustIP(fd7a115cA1e0Ab124843Cd96ˢ), "fd7a:115c:a1e0:ab12:4843:cd96:626b:430b"u8),
        new(mustIP(ffff192168140255ˢ), "::ffff:192.168.140.255"u8),
        new(mustIP(ffff192168140255En0ˢ), "::ffff:192.168.140.255%en0"u8)
    }.slice();
    foreach (var (i, tc) in tests) {
        var ip = tc.ip;
        var mtAppend = new slice<byte>(4, 32);
        (mtAppend, var err) = ip.AppendText(mtAppend);
        mtAppend = mtAppend[4..];
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)mtAppend) != tc.want) {
            Ꮡt.Errorf("%d. for (%v) AppendText = %q; want %q"u8, i, ip, mtAppend, tc.want);
        }
    }
}

internal partial struct TestAddrMarshalUnmarshalBinary_tests /*dyn*/ {
    internal @string ip;
    internal nint wantSize;
}

public static void TestAddrMarshalUnmarshalBinary(ж<testing.T> Ꮡt) {
    var tests = new TestAddrMarshalUnmarshalBinary_tests[]{
        new(""u8, 0), // zero IP

        new("1.2.3.4"u8, 4),
        new("fd7a:115c:a1e0:ab12:4843:cd96:626b:430b"u8, 16),
        new("::ffff:c000:0280"u8, 16),
        new("::ffff:c000:0280%eth0"u8, 20)
    }.slice();
    foreach (var (_, tc) in tests) {
        netipꓸAddr ip = default!;
        if (len(tc.ip) > 0) {
            ip = mustIP(tc.ip);
        }
        var (b, err) = ip.MarshalBinary();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len(b) != tc.wantSize) {
            Ꮡt.Fatalf("%q encoded to size %d; want %d"u8, tc.ip, len(b), tc.wantSize);
        }
        netipꓸAddr ip2 = default!;
        {
            var errΔ1 = ip2.UnmarshalBinary(b); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        if (ip != ip2) {
            Ꮡt.Fatalf("got %v; want %v"u8, ip2, ip);
        }
        var bAppend = new slice<byte>(4, 32);
        (bAppend, err) = ip.AppendBinary(bAppend);
        bAppend = bAppend[4..];
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len(bAppend) != tc.wantSize) {
            Ꮡt.Fatalf("%q encoded to size %d; want %d"u8, tc.ip, len(bAppend), tc.wantSize);
        }
        netipꓸAddr ip3 = default!;
        {
            var errΔ2 = ip3.UnmarshalBinary(bAppend); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        if (ip != ip3) {
            Ꮡt.Fatalf("got %v; want %v"u8, ip3, ip);
        }
    }
    // Cannot unmarshal from unexpected IP length.
    foreach (var (_, n) in new nint[]{3, 5}.slice()) {
        netipꓸAddr ip2 = default!;
        {
            var err = ip2.UnmarshalBinary(bytes.Repeat(new byte[]{1}.slice(), n)); if (err == default!) {
                Ꮡt.Fatalf("unmarshaled from unexpected IP length %d"u8, n);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cafe80ˢ = "[1::CAFE]:80"u8;
internal static readonly @string cafeEn080ˢ = "[1::CAFE%en0]:80"u8;
internal static readonly @string ffff19216814025580ˢ = "[::FFFF:192.168.140.255]:80"u8;
internal static readonly @string ffff192168140255En080ˢ = "[::FFFF:192.168.140.255%en0]:80"u8;

internal partial struct TestAddrPortMarshalTextString_tests /*dyn*/ {
    internal netip.AddrPort @in;
    internal @string want;
}

public static void TestAddrPortMarshalTextString(ж<testing.T> Ꮡt) {
    var tests = new TestAddrPortMarshalTextString_tests[]{
        new(mustIPPort("1.2.3.4:80"u8), "1.2.3.4:80"u8),
        new(mustIPPort("[::]:80"u8), "[::]:80"u8),
        new(mustIPPort(cafe80ˢ), "[1::cafe]:80"u8),
        new(mustIPPort(cafeEn080ˢ), "[1::cafe%en0]:80"u8),
        new(mustIPPort(ffff19216814025580ˢ), "[::ffff:192.168.140.255]:80"u8),
        new(mustIPPort(ffff192168140255En080ˢ), "[::ffff:192.168.140.255%en0]:80"u8)
    }.slice();
    foreach (var (i, tt) in tests) {
        {
            @string got = tt.@in.String(); if (got != tt.want) {
                Ꮡt.Errorf("%d. for (%v, %v) String = %q; want %q"u8, i, tt.@in.Addr(), tt.@in.Port(), got, tt.want);
            }
        }
        var (mt, err) = tt.@in.MarshalText();
        if (err != default!) {
            Ꮡt.Errorf("%d. for (%v, %v) MarshalText error: %v"u8, i, tt.@in.Addr(), tt.@in.Port(), err);
            continue;
        }
        if (((sstring)mt) != tt.want) {
            Ꮡt.Errorf("%d. for (%v, %v) MarshalText = %q; want %q"u8, i, tt.@in.Addr(), tt.@in.Port(), mt, tt.want);
        }
        var mtAppend = new slice<byte>(4, 32);
        (mtAppend, err) = tt.@in.AppendText(mtAppend);
        mtAppend = mtAppend[4..];
        if (err != default!) {
            Ꮡt.Errorf("%d. for (%v, %v) AppendText error: %v"u8, i, tt.@in.Addr(), tt.@in.Port(), err);
            continue;
        }
        if (((sstring)mtAppend) != tt.want) {
            Ꮡt.Errorf("%d. for (%v, %v) AppendText = %q; want %q"u8, i, tt.@in.Addr(), tt.@in.Port(), mtAppend, tt.want);
        }
    }
}

internal partial struct TestAddrPortMarshalUnmarshalBinary_tests /*dyn*/ {
    internal @string ipport;
    internal nint wantSize;
}

public static void TestAddrPortMarshalUnmarshalBinary(ж<testing.T> Ꮡt) {
    var tests = new TestAddrPortMarshalUnmarshalBinary_tests[]{
        new("1.2.3.4:51820"u8, 4 + 2),
        new("[fd7a:115c:a1e0:ab12:4843:cd96:626b:430b]:80"u8, 16 + 2),
        new("[::ffff:c000:0280]:65535"u8, 16 + 2),
        new("[::ffff:c000:0280%eth0]:1"u8, 20 + 2)
    }.slice();
    foreach (var (_, tc) in tests) {
        netip.AddrPort ipport = default!;
        if (len(tc.ipport) > 0) {
            ipport = mustIPPort(tc.ipport);
        }
        var (b, err) = ipport.MarshalBinary();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len(b) != tc.wantSize) {
            Ꮡt.Fatalf("%q encoded to size %d; want %d"u8, tc.ipport, len(b), tc.wantSize);
        }
        netip.AddrPort ipport2 = default!;
        {
            var errΔ1 = ipport2.UnmarshalBinary(b); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        if (ipport != ipport2) {
            Ꮡt.Fatalf("got %v; want %v"u8, ipport2, ipport);
        }
        var bAppend = new slice<byte>(4, 32);
        (bAppend, err) = ipport.AppendBinary(bAppend);
        bAppend = bAppend[4..];
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len(bAppend) != tc.wantSize) {
            Ꮡt.Fatalf("%q encoded to size %d; want %d"u8, tc.ipport, len(bAppend), tc.wantSize);
        }
        netip.AddrPort ipport3 = default!;
        {
            var errΔ2 = ipport3.UnmarshalBinary(bAppend); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        if (ipport != ipport3) {
            Ꮡt.Fatalf("got %v; want %v"u8, ipport3, ipport);
        }
    }
    // Cannot unmarshal from unexpected lengths.
    foreach (var (_, n) in new nint[]{3, 7}.slice()) {
        netip.AddrPort ipport2 = default!;
        {
            var err = ipport2.UnmarshalBinary(bytes.Repeat(new byte[]{1}.slice(), n)); if (err == default!) {
                Ꮡt.Fatalf("unmarshaled from unexpected length %d"u8, n);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fd7a115cA1e0Ab124843Cd96ˢ2 = "fd7a:115c:a1e0:ab12:4843:cd96:626b:430b/118"u8;
internal static readonly @string ffffC000028096ˢ = "::ffff:c000:0280/96"u8;
internal static readonly @string ffff1921681402558ˢ = "::ffff:192.168.140.255/8"u8;
internal static readonly @string ffffC0000280ˢ = "::ffff:c000:0280"u8;

internal partial struct TestPrefixMarshalTextString_tests /*dyn*/ {
    internal netipꓸPrefix @in;
    internal @string want;
}

public static void TestPrefixMarshalTextString(ж<testing.T> Ꮡt) {
    var tests = new TestPrefixMarshalTextString_tests[]{
        new(mustPrefix("1.2.3.4/24"u8), "1.2.3.4/24"u8),
        new(mustPrefix(fd7a115cA1e0Ab124843Cd96ˢ2), "fd7a:115c:a1e0:ab12:4843:cd96:626b:430b/118"u8),
        new(mustPrefix(ffffC000028096ˢ), "::ffff:192.0.2.128/96"u8),
        new(mustPrefix(ffff1921681402558ˢ), "::ffff:192.168.140.255/8"u8),
        new(PrefixFrom(mustIP(ffffC0000280ˢ).WithZone(eth0ˢ), 37), "::ffff:192.0.2.128/37"u8)
    }.slice();
    // Zone should be stripped
    foreach (var (i, tt) in tests) {
        {
            @string got = tt.@in.String(); if (got != tt.want) {
                Ꮡt.Errorf("%d. for %v String = %q; want %q"u8, i, tt.@in, got, tt.want);
            }
        }
        var (mt, err) = tt.@in.MarshalText();
        if (err != default!) {
            Ꮡt.Errorf("%d. for %v MarshalText error: %v"u8, i, tt.@in, err);
            continue;
        }
        if (((sstring)mt) != tt.want) {
            Ꮡt.Errorf("%d. for %v MarshalText = %q; want %q"u8, i, tt.@in, mt, tt.want);
        }
        var mtAppend = new slice<byte>(4, 64);
        (mtAppend, err) = tt.@in.AppendText(mtAppend);
        mtAppend = mtAppend[4..];
        if (err != default!) {
            Ꮡt.Errorf("%d. for %v AppendText error: %v"u8, i, tt.@in, err);
            continue;
        }
        if (((sstring)mtAppend) != tt.want) {
            Ꮡt.Errorf("%d. for %v AppendText = %q; want %q"u8, i, tt.@in, mtAppend, tt.want);
        }
    }
}

internal partial struct TestPrefixMarshalUnmarshalBinary_testCase /*dyn*/ {
    internal netipꓸPrefix prefix;
    internal nint wantSize;
}

public static void TestPrefixMarshalUnmarshalBinary(ж<testing.T> Ꮡt) {
    var tests = new TestPrefixMarshalUnmarshalBinary_testCase[]{
        new(mustPrefix("1.2.3.4/24"u8), 4 + 1),
        new(mustPrefix(fd7a115cA1e0Ab124843Cd96ˢ2), 16 + 1),
        new(mustPrefix(ffffC000028096ˢ), 16 + 1),
        new(PrefixFrom(mustIP(ffffC0000280ˢ).WithZone(eth0ˢ), 37), 16 + 1)
    }.slice();
    // Zone should be stripped
    tests = append(tests,
        new TestPrefixMarshalUnmarshalBinary_testCase(PrefixFrom(tests[0].prefix.Addr(), 33), tests[0].wantSize),
        new TestPrefixMarshalUnmarshalBinary_testCase(PrefixFrom(tests[1].prefix.Addr(), 129), tests[1].wantSize));
    foreach (var (_, tc) in tests) {
        var prefix = tc.prefix;
        var (b, err) = prefix.MarshalBinary();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len(b) != tc.wantSize) {
            Ꮡt.Fatalf("%q encoded to size %d; want %d"u8, tc.prefix, len(b), tc.wantSize);
        }
        netipꓸPrefix prefix2 = default!;
        {
            var errΔ1 = prefix2.UnmarshalBinary(b); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        if (prefix != prefix2) {
            Ꮡt.Fatalf("got %v; want %v"u8, prefix2, prefix);
        }
        var bAppend = new slice<byte>(4, 32);
        (bAppend, err) = prefix.AppendBinary(bAppend);
        bAppend = bAppend[4..];
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len(bAppend) != tc.wantSize) {
            Ꮡt.Fatalf("%q encoded to size %d; want %d"u8, tc.prefix, len(bAppend), tc.wantSize);
        }
        netipꓸPrefix prefix3 = default!;
        {
            var errΔ2 = prefix3.UnmarshalBinary(bAppend); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        if (prefix != prefix3) {
            Ꮡt.Fatalf("got %v; want %v"u8, prefix3, prefix);
        }
    }
    // Cannot unmarshal from unexpected lengths.
    foreach (var (_, n) in new nint[]{3, 6}.slice()) {
        netipꓸPrefix prefix2 = default!;
        {
            var err = prefix2.UnmarshalBinary(bytes.Repeat(new byte[]{1}.slice(), n)); if (err == default!) {
                Ꮡt.Fatalf("unmarshaled from unexpected length %d"u8, n);
            }
        }
    }
}

public static void TestAddrMarshalUnmarshal(ж<testing.T> Ꮡt) {
    // This only tests the cases where Marshal/Unmarshal diverges from
    // the behavior of ParseAddr/String. For the rest of the test cases,
    // see TestParseAddr above.
    @string orig = @""""""u8;
    ref var ip = ref heap(new netipꓸAddr(), out var Ꮡip);
    {
        var errΔ1 = json.Unmarshal(slice<byte>(orig), Ꮡip); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Unmarshal(%q) got error %v"u8, orig, errΔ1);
        }
    }
    if (ip != (new netipꓸAddr(nil))) {
        Ꮡt.Errorf("Unmarshal(%q) is not the zero Addr"u8, orig);
    }
    var (jsb, err) = json.Marshal(ip);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal(%v) got error %v"u8, ip, err);
    }
    @string back = ((@string)jsb);
    if (back != orig) {
        Ꮡt.Errorf("Marshal(Unmarshal(%q)) got %q, want %q"u8, orig, back, orig);
    }
}

internal partial struct TestAddrFrom16_tests /*dyn*/ {
    internal @string name;
    internal array<byte> @in = new(16);
    internal netipꓸAddr want;
}

public static void TestAddrFrom16(ж<testing.T> Ꮡt) {
    var tests = new TestAddrFrom16_tests[]{
        new(
            name: "v6-raw"u8,
            @in: new array<byte>(16){[15] = 1},
            want: MkAddr(Mk128(0, 1), Z6noz)
        ),
        new(
            name: "v4-raw"u8,
            @in: new array<byte>(16){[10] = 0xff, [11] = 0xff, [12] = 1, [13] = 2, [14] = 3, [15] = 4},
            want: MkAddr(Mk128(0, 0xffff01020304UL), Z6noz)
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestAddrFrom16_tests(), out var Ꮡtt);
        tt = vᴛ1.ΔClone();

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = AddrFrom16(ttʗ1.@in);
            if (got != ttʗ1.want) {
                tΔ1.Errorf("got %#v; want %#v"u8, got, ttʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string db81ˢ = "2001:db8::1"u8;
internal static readonly @string db81Eth0ˢ = "2001:db8::1%eth0"u8;
internal static readonly @string ff021ˢ = "ff02::1"u8;
internal static readonly @string ff021Eth0ˢ = "ff02::1%eth0"u8;
internal static readonly @string fe801ˢ = "fe80::1"u8;
internal static readonly @string febfFfffFfffFfffFfffFfffˢ = "febf:ffff:ffff:ffff:ffff:ffff:ffff:ffff"u8;
internal static readonly @string fe801Eth0ˢ = "fe80::1%eth0"u8;
internal static readonly @string ff011ˢ = "ff01::1"u8;
internal static readonly @string ff011Eth0ˢ = "ff01::1%eth0"u8;
internal static readonly @string fd001ˢ = "fd00::1"u8;
internal static readonly @string ffff10001ˢ = "::ffff:10.0.0.1"u8;
internal static readonly @string ffff1721601ˢ = "::ffff:172.16.0.1"u8;
internal static readonly @string ffff19216811ˢ = "::ffff:192.168.1.1"u8;

internal partial struct TestIPProperties_tests /*dyn*/ {
    internal @string name;
    internal netipꓸAddr ip;
    internal bool globalUnicast;
    internal bool interfaceLocalMulticast;
    internal bool linkLocalMulticast;
    internal bool linkLocalUnicast;
    internal bool loopback;
    internal bool multicast;
    internal bool @private;
    internal bool unspecified;
}

public static void TestIPProperties(ж<testing.T> Ꮡt) {
    netipꓸAddr nilIP = default!;
    netipꓸAddr unicast4 = mustIP("192.0.2.1"u8);
    netipꓸAddr unicast6 = mustIP(db81ˢ);
    netipꓸAddr unicastZone6 = mustIP(db81Eth0ˢ);
    netipꓸAddr unicast6Unassigned = mustIP("4000::1"u8);             // not in 2000::/3.
    netipꓸAddr multicast4 = mustIP("224.0.0.1"u8);
    netipꓸAddr multicast6 = mustIP(ff021ˢ);
    netipꓸAddr multicastZone6 = mustIP(ff021Eth0ˢ);
    netipꓸAddr llu4 = mustIP("169.254.0.1"u8);
    netipꓸAddr llu6 = mustIP(fe801ˢ);
    netipꓸAddr llu6Last = mustIP(febfFfffFfffFfffFfffFfffˢ);
    netipꓸAddr lluZone6 = mustIP(fe801Eth0ˢ);
    netipꓸAddr loopback4 = mustIP("127.0.0.1"u8);
    netipꓸAddr ilm6 = mustIP(ff011ˢ);
    netipꓸAddr ilmZone6 = mustIP(ff011Eth0ˢ);
    netipꓸAddr private4a = mustIP("10.0.0.1"u8);
    netipꓸAddr private4b = mustIP("172.16.0.1"u8);
    netipꓸAddr private4c = mustIP("192.168.1.1"u8);
    netipꓸAddr private6 = mustIP(fd001ˢ);
    netipꓸAddr private6mapped4a = mustIP(ffff10001ˢ);
    netipꓸAddr private6mapped4b = mustIP(ffff1721601ˢ);
    netipꓸAddr private6mapped4c = mustIP(ffff19216811ˢ);
    var tests = new TestIPProperties_tests[]{
        new(
            name: "nil"u8,
            ip: nilIP
        ),
        new(
            name: "unicast v4Addr"u8,
            ip: unicast4,
            globalUnicast: true
        ),
        new(
            name: "unicast v6 mapped v4Addr"u8,
            ip: AddrFrom16(unicast4.As16()),
            globalUnicast: true
        ),
        new(
            name: "unicast v6Addr"u8,
            ip: unicast6,
            globalUnicast: true
        ),
        new(
            name: "unicast v6AddrZone"u8,
            ip: unicastZone6,
            globalUnicast: true
        ),
        new(
            name: "unicast v6Addr unassigned"u8,
            ip: unicast6Unassigned,
            globalUnicast: true
        ),
        new(
            name: "multicast v4Addr"u8,
            ip: multicast4,
            linkLocalMulticast: true,
            multicast: true
        ),
        new(
            name: "multicast v6 mapped v4Addr"u8,
            ip: AddrFrom16(multicast4.As16()),
            linkLocalMulticast: true,
            multicast: true
        ),
        new(
            name: "multicast v6Addr"u8,
            ip: multicast6,
            linkLocalMulticast: true,
            multicast: true
        ),
        new(
            name: "multicast v6AddrZone"u8,
            ip: multicastZone6,
            linkLocalMulticast: true,
            multicast: true
        ),
        new(
            name: "link-local unicast v4Addr"u8,
            ip: llu4,
            linkLocalUnicast: true
        ),
        new(
            name: "link-local unicast v6 mapped v4Addr"u8,
            ip: AddrFrom16(llu4.As16()),
            linkLocalUnicast: true
        ),
        new(
            name: "link-local unicast v6Addr"u8,
            ip: llu6,
            linkLocalUnicast: true
        ),
        new(
            name: "link-local unicast v6Addr upper bound"u8,
            ip: llu6Last,
            linkLocalUnicast: true
        ),
        new(
            name: "link-local unicast v6AddrZone"u8,
            ip: lluZone6,
            linkLocalUnicast: true
        ),
        new(
            name: "loopback v4Addr"u8,
            ip: loopback4,
            loopback: true
        ),
        new(
            name: "loopback v6Addr"u8,
            ip: IPv6Loopback(),
            loopback: true
        ),
        new(
            name: "loopback v6 mapped v4Addr"u8,
            ip: AddrFrom16(IPv6Loopback().As16()),
            loopback: true
        ),
        new(
            name: "interface-local multicast v6Addr"u8,
            ip: ilm6,
            interfaceLocalMulticast: true,
            multicast: true
        ),
        new(
            name: "interface-local multicast v6AddrZone"u8,
            ip: ilmZone6,
            interfaceLocalMulticast: true,
            multicast: true
        ),
        new(
            name: "private v4Addr 10/8"u8,
            ip: private4a,
            globalUnicast: true,
            @private: true
        ),
        new(
            name: "private v4Addr 172.16/12"u8,
            ip: private4b,
            globalUnicast: true,
            @private: true
        ),
        new(
            name: "private v4Addr 192.168/16"u8,
            ip: private4c,
            globalUnicast: true,
            @private: true
        ),
        new(
            name: "private v6Addr"u8,
            ip: private6,
            globalUnicast: true,
            @private: true
        ),
        new(
            name: "private v6 mapped v4Addr 10/8"u8,
            ip: private6mapped4a,
            globalUnicast: true,
            @private: true
        ),
        new(
            name: "private v6 mapped v4Addr 172.16/12"u8,
            ip: private6mapped4b,
            globalUnicast: true,
            @private: true
        ),
        new(
            name: "private v6 mapped v4Addr 192.168/16"u8,
            ip: private6mapped4c,
            globalUnicast: true,
            @private: true
        ),
        new(
            name: "unspecified v4Addr"u8,
            ip: IPv4Unspecified(),
            unspecified: true
        ),
        new(
            name: "unspecified v6Addr"u8,
            ip: IPv6Unspecified(),
            unspecified: true
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestIPProperties_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var gu = ttʗ1.ip.IsGlobalUnicast();
            if (gu != ttʗ1.globalUnicast) {
                tΔ1.Errorf("IsGlobalUnicast(%v) = %v; want %v"u8, ttʗ1.ip, gu, ttʗ1.globalUnicast);
            }
            var ilm = ttʗ1.ip.IsInterfaceLocalMulticast();
            if (ilm != ttʗ1.interfaceLocalMulticast) {
                tΔ1.Errorf("IsInterfaceLocalMulticast(%v) = %v; want %v"u8, ttʗ1.ip, ilm, ttʗ1.interfaceLocalMulticast);
            }
            var llu = ttʗ1.ip.IsLinkLocalUnicast();
            if (llu != ttʗ1.linkLocalUnicast) {
                tΔ1.Errorf("IsLinkLocalUnicast(%v) = %v; want %v"u8, ttʗ1.ip, llu, ttʗ1.linkLocalUnicast);
            }
            var llm = ttʗ1.ip.IsLinkLocalMulticast();
            if (llm != ttʗ1.linkLocalMulticast) {
                tΔ1.Errorf("IsLinkLocalMulticast(%v) = %v; want %v"u8, ttʗ1.ip, llm, ttʗ1.linkLocalMulticast);
            }
            var lo = ttʗ1.ip.IsLoopback();
            if (lo != ttʗ1.loopback) {
                tΔ1.Errorf("IsLoopback(%v) = %v; want %v"u8, ttʗ1.ip, lo, ttʗ1.loopback);
            }
            var multicast = ttʗ1.ip.IsMulticast();
            if (multicast != ttʗ1.multicast) {
                tΔ1.Errorf("IsMulticast(%v) = %v; want %v"u8, ttʗ1.ip, multicast, ttʗ1.multicast);
            }
            var @private = ttʗ1.ip.IsPrivate();
            if (@private != ttʗ1.@private) {
                tΔ1.Errorf("IsPrivate(%v) = %v; want %v"u8, ttʗ1.ip, @private, ttʗ1.@private);
            }
            var unspecified = ttʗ1.ip.IsUnspecified();
            if (unspecified != ttʗ1.unspecified) {
                tΔ1.Errorf("IsUnspecified(%v) = %v; want %v"u8, ttʗ1.ip, unspecified, ttʗ1.unspecified);
            }
        });
    }
}

internal partial struct TestAddrWellKnown_tests /*dyn*/ {
    internal @string name;
    internal netipꓸAddr ip;
    internal net.IP std;
}

public static void TestAddrWellKnown(ж<testing.T> Ꮡt) {
    var tests = new TestAddrWellKnown_tests[]{
        new(
            name: "IPv4 unspecified"u8,
            ip: IPv4Unspecified(),
            std: net.IPv4zero
        ),
        new(
            name: "IPv6 link-local all nodes"u8,
            ip: IPv6LinkLocalAllNodes(),
            std: net.IPv6linklocalallnodes
        ),
        new(
            name: "IPv6 link-local all routers"u8,
            ip: IPv6LinkLocalAllRouters(),
            std: net.IPv6linklocalallrouters
        ),
        new(
            name: "IPv6 loopback"u8,
            ip: IPv6Loopback(),
            std: net.IPv6loopback
        ),
        new(
            name: "IPv6 unspecified"u8,
            ip: IPv6Unspecified(),
            std: net.IPv6unspecified
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestAddrWellKnown_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            @string want = ttʗ1.std.String();
            @string got = ttʗ1.ip.String();
            if (got != want) {
                tΔ1.Fatalf("got %s, want %s"u8, got, want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "::1%foo"u8;
internal static readonly @string ffff111112ˢ = "::ffff:11.1.1.12"u8;
internal static readonly @string invalidIp1234888811Foo2ˢ = @"[invalid IP 1.2.3.4 8.8.8.8 ::1 ::1%foo ::2]"u8;

internal partial struct TestAddrLessCompare_tests /*dyn*/ {
    internal netipꓸAddr a, b;
    internal bool want;
}

public static void TestAddrLessCompare(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestAddrLessCompare_tests[]{
        new(new netipꓸAddr(nil), new netipꓸAddr(nil), false),
        new(new netipꓸAddr(nil), mustIP("1.2.3.4"u8), true),
        new(mustIP("1.2.3.4"u8), new netipꓸAddr(nil), false),
        new(mustIP("1.2.3.4"u8), mustIP("0102:0304::0"u8), true),
        new(mustIP("0102:0304::0"u8), mustIP("1.2.3.4"u8), false),
        new(mustIP("1.2.3.4"u8), mustIP("1.2.3.4"u8), false),
        new(mustIP("::1"u8), mustIP("::2"u8), true),
        new(mustIP("::1"u8), mustIP(fooˢ), true),
        new(mustIP(fooˢ), mustIP("::2"u8), true),
        new(mustIP("::2"u8), mustIP("::3"u8), true),
        new(mustIP("::"u8), mustIP("0.0.0.0"u8), false),
        new(mustIP("0.0.0.0"u8), mustIP("::"u8), true),
        new(mustIP("::1%a"u8), mustIP("::1%b"u8), true),
        new(mustIP("::1%a"u8), mustIP("::1%a"u8), false),
        new(mustIP("::1%b"u8), mustIP("::1%a"u8), false), // For Issue 68113, verify that an IPv4 address and a
 // v4-mapped-IPv6 address differing only in their zone
 // pointer are unequal via all three of
 // ==/Compare/reflect.DeepEqual. In Go 1.22 and
 // earlier, these were accidentally equal via
 // DeepEqual due to their zone pointers (z) differing
 // but pointing to identical structures.

        new(mustIP(ffff111112ˢ), mustIP("11.1.1.12"u8), false)
    }.slice();
    foreach (var (_, tt) in tests) {
        var gotΔ1 = tt.a.Less(tt.b);
        if (gotΔ1 != tt.want) {
            Ꮡt.Errorf("Less(%q, %q) = %v; want %v"u8, tt.a, tt.b, gotΔ1, tt.want);
        }
        nint cmp = tt.a.Compare(tt.b);
        if (gotΔ1 && cmp != -1) {
            Ꮡt.Errorf("Less(%q, %q) = true, but Compare = %v (not -1)"u8, tt.a, tt.b, cmp);
        }
        if (cmp < -1 || cmp > 1) {
            Ꮡt.Errorf("bogus Compare return value %v"u8, cmp);
        }
        if (cmp == 0 && tt.a != tt.b) {
            Ꮡt.Errorf("Compare(%q, %q) = 0; but not equal"u8, tt.a, tt.b);
        }
        if (cmp == 1 && !tt.b.Less(tt.a)) {
            Ꮡt.Errorf("Compare(%q, %q) = 1; but b.Less(a) isn't true"u8, tt.a, tt.b);
        }
        // Also check inverse.
        if (gotΔ1 == tt.want && gotΔ1) {
            var got2 = tt.b.Less(tt.a);
            if (got2) {
                Ꮡt.Errorf("Less(%q, %q) was correctly %v, but so was Less(%q, %q)"u8, tt.a, tt.b, gotΔ1, tt.b, tt.a);
            }
        }
        // Also check reflect.DeepEqual. See issue 68113.
        var deepEq = reflect.DeepEqual(tt.a, tt.b);
        if ((cmp == 0) != deepEq) {
            Ꮡt.Errorf("%q and %q differ in == (%v) vs reflect.DeepEqual (%v)"u8, tt.a, tt.b, cmp == 0, deepEq);
        }
    }
    // And just sort.
    var values = new netipꓸAddr[]{
        mustIP("::1"u8),
        mustIP("::2"u8),
        new netipꓸAddr(nil),
        mustIP("1.2.3.4"u8),
        mustIP("8.8.8.8"u8),
        mustIP(fooˢ)
    }.slice();
    slices.SortFunc<slice<netipꓸAddr>, netipꓸAddr>(values, ((Func<netipꓸAddr, netipꓸAddr, nint>)(netip.Compare)));
    @string got = fmt.Sprintf("%s"u8, values);
    @string want = invalidIp1234888811Foo2ˢ;
    if (got != want) {
        Ꮡt.Errorf("unexpected sort\n got: %s\nwant: %s\n"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string foo1024ˢ = "[::1%foo]:1024"u8;
internal static readonly @string invalidAddrPort123444388ˢ = @"[invalid AddrPort 1.2.3.4:443 8.8.8.8:8080 [::1]:80 [::1%foo]:1024 [::2]:80]"u8;

internal partial struct TestAddrPortCompare_tests /*dyn*/ {
    internal netip.AddrPort a, b;
    internal nint want;
}

public static void TestAddrPortCompare(ж<testing.T> Ꮡt) {
    var tests = new TestAddrPortCompare_tests[]{
        new(new AddrPort(nil), new AddrPort(nil), 0),
        new(new AddrPort(nil), mustIPPort("1.2.3.4:80"u8), -1),
        new(mustIPPort("1.2.3.4:80"u8), mustIPPort("1.2.3.4:80"u8), 0),
        new(mustIPPort("[::1]:80"u8), mustIPPort("[::1]:80"u8), 0),
        new(mustIPPort("1.2.3.4:80"u8), mustIPPort("2.3.4.5:22"u8), -1),
        new(mustIPPort("[::1]:80"u8), mustIPPort("[::2]:22"u8), -1),
        new(mustIPPort("1.2.3.4:80"u8), mustIPPort("1.2.3.4:443"u8), -1),
        new(mustIPPort("[::1]:80"u8), mustIPPort("[::1]:443"u8), -1),
        new(mustIPPort("1.2.3.4:80"u8), mustIPPort("[0102:0304::0]:80"u8), -1)
    }.slice();
    foreach (var (_, tt) in tests) {
        nint gotΔ1 = tt.a.Compare(tt.b);
        if (gotΔ1 != tt.want) {
            Ꮡt.Errorf("Compare(%q, %q) = %v; want %v"u8, tt.a, tt.b, gotΔ1, tt.want);
        }
        // Also check inverse.
        if (gotΔ1 == tt.want) {
            nint got2 = tt.b.Compare(tt.a);
            {
                nint want2 = -1 * tt.want; if (got2 != want2) {
                    Ꮡt.Errorf("Compare(%q, %q) was correctly %v, but Compare(%q, %q) was %v"u8, tt.a, tt.b, gotΔ1, tt.b, tt.a, got2);
                }
            }
        }
    }
    // And just sort.
    var values = new netip.AddrPort[]{
        mustIPPort("[::1]:80"u8),
        mustIPPort("[::2]:80"u8),
        new AddrPort(nil),
        mustIPPort("1.2.3.4:443"u8),
        mustIPPort("8.8.8.8:8080"u8),
        mustIPPort(foo1024ˢ)
    }.slice();
    slices.SortFunc<slice<netip.AddrPort>, netip.AddrPort>(values, ((Func<netip.AddrPort, netip.AddrPort, nint>)(netip.Compare)));
    @string got = fmt.Sprintf("%s"u8, values);
    @string want = invalidAddrPort123444388ˢ;
    if (got != want) {
        Ꮡt.Errorf("unexpected sort\n got: %s\nwant: %s\n"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fe8064ˢ = "fe80::/64"u8;
internal static readonly @string fe9064ˢ = "fe90::/64"u8;
internal static readonly @string fe8048ˢ = "fe80::/48"u8;
internal static readonly @string fe808ˢ = "fe80::/8"u8;
internal static readonly @string invalidPrefix1200161200ˢ = @"[invalid Prefix 1.2.0.0/16 1.2.0.0/24 1.2.3.0/24 fe80::/48 fe80::/64 fe90::/64]"u8;

internal partial struct TestPrefixCompare_tests /*dyn*/ {
    internal netipꓸPrefix a, b;
    internal nint want;
}

public static void TestPrefixCompare(ж<testing.T> Ꮡt) {
    var tests = new TestPrefixCompare_tests[]{
        new(new netipꓸPrefix(nil), new netipꓸPrefix(nil), 0),
        new(new netipꓸPrefix(nil), mustPrefix("1.2.3.0/24"u8), -1),
        new(mustPrefix("1.2.3.0/24"u8), mustPrefix("1.2.3.0/24"u8), 0),
        new(mustPrefix(fe8064ˢ), mustPrefix(fe8064ˢ), 0),
        new(mustPrefix("1.2.3.0/24"u8), mustPrefix("1.2.4.0/24"u8), -1),
        new(mustPrefix(fe8064ˢ), mustPrefix(fe9064ˢ), -1),
        new(mustPrefix("1.2.0.0/16"u8), mustPrefix("1.2.0.0/24"u8), -1),
        new(mustPrefix(fe8048ˢ), mustPrefix(fe8064ˢ), -1),
        new(mustPrefix("1.2.3.0/24"u8), mustPrefix(fe808ˢ), -1)
    }.slice();
    foreach (var (_, tt) in tests) {
        nint gotΔ1 = tt.a.Compare(tt.b);
        if (gotΔ1 != tt.want) {
            Ꮡt.Errorf("Compare(%q, %q) = %v; want %v"u8, tt.a, tt.b, gotΔ1, tt.want);
        }
        // Also check inverse.
        if (gotΔ1 == tt.want) {
            nint got2 = tt.b.Compare(tt.a);
            {
                nint want2 = -1 * tt.want; if (got2 != want2) {
                    Ꮡt.Errorf("Compare(%q, %q) was correctly %v, but Compare(%q, %q) was %v"u8, tt.a, tt.b, gotΔ1, tt.b, tt.a, got2);
                }
            }
        }
    }
    // And just sort.
    var values = new netipꓸPrefix[]{
        mustPrefix("1.2.3.0/24"u8),
        mustPrefix(fe9064ˢ),
        mustPrefix(fe8064ˢ),
        mustPrefix("1.2.0.0/16"u8),
        new netipꓸPrefix(nil),
        mustPrefix(fe8048ˢ),
        mustPrefix("1.2.0.0/24"u8)
    }.slice();
    slices.SortFunc<slice<netipꓸPrefix>, netipꓸPrefix>(values, ((Func<netipꓸPrefix, netipꓸPrefix, nint>)(netip.Compare)));
    @string got = fmt.Sprintf("%s"u8, values);
    @string want = invalidPrefix1200161200ˢ;
    if (got != want) {
        Ꮡt.Errorf("unexpected sort\n got: %s\nwant: %s\n"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ffff192021ˢ = "::ffff:192.0.2.1"u8;

internal partial struct TestIPStringExpanded_tests /*dyn*/ {
    internal netipꓸAddr ip;
    internal @string s;
}

public static void TestIPStringExpanded(ж<testing.T> Ꮡt) {
    var tests = new TestIPStringExpanded_tests[]{
        new(
            ip: new netipꓸAddr(nil),
            s: "invalid IP"u8
        ),
        new(
            ip: mustIP("192.0.2.1"u8),
            s: "192.0.2.1"u8
        ),
        new(
            ip: mustIP(ffff192021ˢ),
            s: "0000:0000:0000:0000:0000:ffff:c000:0201"u8
        ),
        new(
            ip: mustIP(db81ˢ),
            s: "2001:0db8:0000:0000:0000:0000:0000:0001"u8
        ),
        new(
            ip: mustIP(db81Eth0ˢ),
            s: "2001:0db8:0000:0000:0000:0000:0000:0001%eth0"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestIPStringExpanded_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.ip.String(), (ж<testing.T> tΔ1) => {
            @string want = ttʗ1.s;
            @string got = ttʗ1.ip.StringExpanded();
            if (got != want) {
                tΔ1.Fatalf("got %s, want %s"u8, got, want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string db832ˢ = "2001:db8::/32"u8;
internal static readonly @string fe80DeadBeef0096ˢ = "fe80::dead:beef:0:0/96"u8;
internal static readonly @string a0004ˢ = "a000::/4"u8;
internal static readonly object expectedAnErrorButNoneˢ = (@string)"expected an error, but none occurred"u8;

internal partial struct TestPrefixMasking_subtest /*dyn*/ {
    internal netipꓸAddr ip;
    internal uint8 bits;
    internal netipꓸPrefix p;
    internal bool ok;
}

internal partial struct TestPrefixMasking_tests /*dyn*/ {
    internal @string family;
    internal slice<TestPrefixMasking_subtest> subtests;
}

public static void TestPrefixMasking(ж<testing.T> Ꮡt) {
    // makeIPv6 produces a set of IPv6 subtests with an optional zone identifier.
    slice<TestPrefixMasking_subtest> makeIPv6(@string zone) {
        if (zone != ""u8) {
            zone = "%"u8 + zone;
        }
        return new TestPrefixMasking_subtest[]{
            new(
                ip: mustIP(fmt.Sprintf("2001:db8::1%s"u8, zone)),
                bits: 255
            ),
            new(
                ip: mustIP(fmt.Sprintf("2001:db8::1%s"u8, zone)),
                bits: 32,
                p: mustPrefix(db832ˢ),
                ok: true
            ),
            new(
                ip: mustIP(fmt.Sprintf("fe80::dead:beef:dead:beef%s"u8, zone)),
                bits: 96,
                p: mustPrefix(fe80DeadBeef0096ˢ),
                ok: true
            ),
            new(
                ip: mustIP(fmt.Sprintf("aaaa::%s"u8, zone)),
                bits: 4,
                p: mustPrefix(a0004ˢ),
                ok: true
            ),
            new(
                ip: mustIP(fmt.Sprintf("::%s"u8, zone)),
                bits: 63,
                p: mustPrefix("::/63"u8),
                ok: true
            )
        }.slice();
    }
    var tests = new TestPrefixMasking_tests[]{
        new(
            family: "nil"u8,
            subtests: new TestPrefixMasking_subtest[]{
                new(
                    bits: 255,
                    ok: true
                ),
                new(
                    bits: 16,
                    ok: true
                )
            }.slice()
        ),
        new(
            family: "IPv4"u8,
            subtests: new TestPrefixMasking_subtest[]{
                new(
                    ip: mustIP("192.0.2.0"u8),
                    bits: 255
                ),
                new(
                    ip: mustIP("192.0.2.0"u8),
                    bits: 16,
                    p: mustPrefix("192.0.0.0/16"u8),
                    ok: true
                ),
                new(
                    ip: mustIP("255.255.255.255"u8),
                    bits: 20,
                    p: mustPrefix("255.255.240.0/20"u8),
                    ok: true
                ),
                new(
                    ip: mustIP("100.98.156.66"u8), // Partially masking one byte that contains both
 // 1s and 0s on either side of the mask limit.

                    bits: 10,
                    p: mustPrefix("100.64.0.0/10"u8),
                    ok: true
                )
            }.slice()
        ),
        new(
            family: "IPv6"u8,
            subtests: makeIPv6(""u8)
        ),
        new(
            family: "IPv6 zone"u8,
            subtests: makeIPv6(eth0ˢ)
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestPrefixMasking_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.family, (ж<testing.T> tΔ1) => {
            foreach (var (_, vᴛ2) in ttʗ1.subtests) {
                ref var st = ref heap(new TestPrefixMasking_subtest(), out var Ꮡst);
                st = vᴛ2;

                var stʗ1 = st;
                tΔ1.Run(st.p.String(), (ж<testing.T> tΔ2) => {
                    // Ensure st.ip is not mutated.
                    @string orig = stʗ1.ip.String();
                    var (p, err) = stʗ1.ip.Prefix((nint)stʗ1.bits);
                    if (stʗ1.ok && err != default!) {
                        tΔ2.Fatalf("failed to produce prefix: %v"u8, err);
                    }
                    if (!stʗ1.ok && err == default!) {
                        tΔ2.Fatal(expectedAnErrorButNoneˢ);
                    }
                    if (err != default!) {
                        tΔ2.Logf("err: %v"u8, err);
                        return;
                    }
                    if (!reflect.DeepEqual(p, stʗ1.p)) {
                        tΔ2.Errorf("prefix = %q, want %q"u8, p, stʗ1.p);
                    }
                    {
                        @string got = stʗ1.ip.String(); if (got != orig) {
                            tΔ2.Errorf("IP was mutated: %q, want %q"u8, got, orig);
                        }
                    }
                });
            }
        });
    }
}

public static void TestPrefixMarshalUnmarshal(ж<testing.T> Ꮡt) {
    var tests = new @string[]{
        ""u8,
        "1.2.3.4/32"u8,
        "0.0.0.0/0"u8,
        "::/0"u8,
        "::1/128"u8,
        "2001:db8::/32"u8
    }.slice();
    foreach (var (_, s) in tests) {
        Ꮡt.Run(s, (ж<testing.T> tΔ1) => {
            // Ensure that JSON  (and by extension, text) marshaling is
            // sane by entering quoted input.
            @string orig = @""""u8 + s + @""""u8;
            ref var p = ref heap(new netipꓸPrefix(), out var Ꮡp);
            {
                var errΔ1 = json.Unmarshal(slice<byte>(orig), Ꮡp); if (errΔ1 != default!) {
                    tΔ1.Fatalf("failed to unmarshal: %v"u8, errΔ1);
                }
            }
            var (pb, err) = json.Marshal(p);
            if (err != default!) {
                tΔ1.Fatalf("failed to marshal: %v"u8, err);
            }
            @string back = ((@string)pb);
            if (orig != back) {
                tΔ1.Errorf("Marshal = %q; want %q"u8, back, orig);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshaledIntoNonEmptyˢ = (@string)"unmarshaled into non-empty Prefix"u8;

public static void TestPrefixUnmarshalTextNonZero(ж<testing.T> Ꮡt) {
    var ip = mustPrefix(fe8064ˢ);
    {
        var err = ip.UnmarshalText(slice<byte>("xxx"u8)); if (err == default!) {
            Ꮡt.Fatal(unmarshaledIntoNonEmptyˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ffff19202128ˢ = "::ffff:192.0.2.128"u8;
internal static readonly @string fffeC0000280ˢ = "::fffe:c000:0280"u8;
internal static readonly @string eth0ˢ2 = "::1%eth0"u8;

internal partial struct TestIs4AndIs6_tests /*dyn*/ {
    internal netipꓸAddr ip;
    internal bool is4;
    internal bool is6;
}

public static void TestIs4AndIs6(ж<testing.T> Ꮡt) {
    var tests = new TestIs4AndIs6_tests[]{
        new(new netipꓸAddr(nil), false, false),
        new(mustIP("1.2.3.4"u8), true, false),
        new(mustIP("127.0.0.2"u8), true, false),
        new(mustIP("::1"u8), false, true),
        new(mustIP(ffff19202128ˢ), false, true),
        new(mustIP(fffeC0000280ˢ), false, true),
        new(mustIP(eth0ˢ2), false, true)
    }.slice();
    foreach (var (_, tt) in tests) {
        var got4 = tt.ip.Is4();
        if (got4 != tt.is4) {
            Ꮡt.Errorf("Is4(%q) = %v; want %v"u8, tt.ip, got4, tt.is4);
        }
        var got6 = tt.ip.Is6();
        if (got6 != tt.is6) {
            Ꮡt.Errorf("Is6(%q) = %v; want %v"u8, tt.ip, got6, tt.is6);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ffff19202128Eth0ˢ = "::ffff:192.0.2.128%eth0"u8;
internal static readonly @string ffff127123ˢ = "::ffff:127.1.2.3"u8;
internal static readonly @string ffff7f010203ˢ = "::ffff:7f01:0203"u8;
internal static readonly @string ffff127123ˢ2 = "0:0:0:0:0000:ffff:127.1.2.3"u8;
internal static readonly @string ffff127123ˢ3 = "0:0:0:0::ffff:127.1.2.3"u8;

internal partial struct TestIs4In6_tests /*dyn*/ {
    internal netipꓸAddr ip;
    internal bool want;
    internal netipꓸAddr wantUnmap;
}

public static void TestIs4In6(ж<testing.T> Ꮡt) {
    var tests = new TestIs4In6_tests[]{
        new(new netipꓸAddr(nil), false, new netipꓸAddr(nil)),
        new(mustIP(ffffC0000280ˢ), true, mustIP("192.0.2.128"u8)),
        new(mustIP(ffff19202128ˢ), true, mustIP("192.0.2.128"u8)),
        new(mustIP(ffff19202128Eth0ˢ), true, mustIP("192.0.2.128"u8)),
        new(mustIP(fffeC0000280ˢ), false, mustIP(fffeC0000280ˢ)),
        new(mustIP(ffff127123ˢ), true, mustIP("127.1.2.3"u8)),
        new(mustIP(ffff7f010203ˢ), true, mustIP("127.1.2.3"u8)),
        new(mustIP(ffff127123ˢ2), true, mustIP("127.1.2.3"u8)),
        new(mustIP(ffff127123ˢ3), true, mustIP("127.1.2.3"u8)),
        new(mustIP("::1"u8), false, mustIP("::1"u8)),
        new(mustIP("1.2.3.4"u8), false, mustIP("1.2.3.4"u8))
    }.slice();
    foreach (var (_, tt) in tests) {
        var got = tt.ip.Is4In6();
        if (got != tt.want) {
            Ꮡt.Errorf("Is4In6(%q) = %v; want %v"u8, tt.ip, got, tt.want);
        }
        var u = tt.ip.Unmap();
        if (u != tt.wantUnmap) {
            Ꮡt.Errorf("Unmap(%q) = %v; want %v"u8, tt.ip, u, tt.wantUnmap);
        }
    }
}

internal partial struct TestPrefixMasked_tests /*dyn*/ {
    internal netipꓸPrefix prefix;
    internal netipꓸPrefix masked;
}

public static void TestPrefixMasked(ж<testing.T> Ꮡt) {
    var tests = new TestPrefixMasked_tests[]{
        new(
            prefix: mustPrefix("192.168.0.255/24"u8),
            masked: mustPrefix("192.168.0.0/24"u8)
        ),
        new(
            prefix: mustPrefix("2100::/3"u8),
            masked: mustPrefix("2000::/3"u8)
        ),
        new(
            prefix: PrefixFrom(mustIP("2000::"u8), 129),
            masked: new netipꓸPrefix(nil)
        ),
        new(
            prefix: PrefixFrom(mustIP("1.2.3.4"u8), 33),
            masked: new netipꓸPrefix(nil)
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestPrefixMasked_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.prefix.String(), (ж<testing.T> tΔ1) => {
            var got = testʗ1.prefix.Masked();
            if (got != testʗ1.masked) {
                tΔ1.Errorf("Masked=%s, want %s"u8, got, testʗ1.masked);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string db8ˢ = "2001:db8::"u8;
internal static readonly @string db8AaaaBbbbˢ = "2001:db8::aaaa:bbbb"u8;
internal static readonly @string db81AaaaBbbbˢ = "2001:db8::1:aaaa:bbbb"u8;
internal static readonly @string db9ˢ = "2001:db9::"u8;

internal partial struct TestPrefix_tests /*dyn*/ {
    internal @string prefix;
    internal netipꓸAddr ip;
    internal nint bits;
    internal @string str;
    internal slice<netipꓸAddr> contains;
    internal slice<netipꓸAddr> notContains;
}

public static void TestPrefix(ж<testing.T> Ꮡt) {
    var tests = new TestPrefix_tests[]{
        new(
            prefix: "192.168.0.0/24"u8,
            ip: mustIP("192.168.0.0"u8),
            bits: 24,
            contains: mustIPs("192.168.0.1"u8, "192.168.0.55"),
            notContains: mustIPs("192.168.1.1"u8, "1.1.1.1")
        ),
        new(
            prefix: "192.168.1.1/32"u8,
            ip: mustIP("192.168.1.1"u8),
            bits: 32,
            contains: mustIPs("192.168.1.1"u8),
            notContains: mustIPs("192.168.1.2"u8)
        ),
        new(
            prefix: "100.64.0.0/10"u8, // CGNAT range; prefix not multiple of 8

            ip: mustIP("100.64.0.0"u8),
            bits: 10,
            contains: mustIPs("100.64.0.0"u8, "100.64.0.1", "100.81.251.94", "100.100.100.100", "100.127.255.254", "100.127.255.255"),
            notContains: mustIPs("100.63.255.255"u8, "100.128.0.0")
        ),
        new(
            prefix: "2001:db8::/96"u8,
            ip: mustIP(db8ˢ),
            bits: 96,
            contains: mustIPs(db8AaaaBbbbˢ, db81ˢ),
            notContains: mustIPs(db81AaaaBbbbˢ, db9ˢ)
        ),
        new(
            prefix: "0.0.0.0/0"u8,
            ip: mustIP("0.0.0.0"u8),
            bits: 0,
            contains: mustIPs("192.168.0.1"u8, "1.1.1.1"),
            notContains: append(mustIPs(db81ˢ), new netipꓸAddr(nil))
        ),
        new(
            prefix: "::/0"u8,
            ip: mustIP("::"u8),
            bits: 0,
            contains: mustIPs("::1"u8, db81ˢ),
            notContains: mustIPs("192.0.2.1"u8)
        ),
        new(
            prefix: "2000::/3"u8,
            ip: mustIP("2000::"u8),
            bits: 3,
            contains: mustIPs(db81ˢ),
            notContains: mustIPs(fe801ˢ)
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestPrefix_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.prefix, (ж<testing.T> tΔ1) => {
            var (prefix, err) = ParsePrefix(testʗ1.prefix);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            if (prefix.Addr() != testʗ1.ip) {
                tΔ1.Errorf("IP=%s, want %s"u8, prefix.Addr(), testʗ1.ip);
            }
            if (prefix.Bits() != testʗ1.bits) {
                tΔ1.Errorf("bits=%d, want %d"u8, prefix.Bits(), testʗ1.bits);
            }
            foreach (var (_, ip) in testʗ1.contains) {
                if (!prefix.Contains(ip)) {
                    tΔ1.Errorf("does not contain %s"u8, ip);
                }
            }
            foreach (var (_, ip) in testʗ1.notContains) {
                if (prefix.Contains(ip)) {
                    tΔ1.Errorf("contains %s"u8, ip);
                }
            }
            @string want = testʗ1.str;
            if (want == ""u8) {
                want = testʗ1.prefix;
            }
            {
                @string got = prefix.String(); if (got != want) {
                    tΔ1.Errorf("prefix.String()=%q, want %q"u8, got, want);
                }
            }
            TestAppendToMarshal(tΔ1, prefix);
        });
    }
}

internal partial struct TestPrefixFromInvalidBits_tests /*dyn*/ {
    internal netipꓸAddr ip;
    internal nint @in, want;
}

public static void TestPrefixFromInvalidBits(ж<testing.T> Ꮡt) {
    var v4 = MustParseAddr("1.2.3.4"u8);
    var v6 = MustParseAddr("66::66"u8);
    var tests = new TestPrefixFromInvalidBits_tests[]{
        new(v4, 0, 0),
        new(v6, 0, 0),
        new(v4, 1, 1),
        new(v4, 33, -1),
        new(v6, 33, 33),
        new(v6, 127, 127),
        new(v6, 128, 128),
        new(v4, 254, -1),
        new(v4, 255, -1),
        new(v4, -1, -1),
        new(v6, -1, -1),
        new(v4, -5, -1),
        new(v6, -5, -1)
    }.slice();
    foreach (var (_, tt) in tests) {
        var p = PrefixFrom(tt.ip, tt.@in);
        {
            nint got = p.Bits(); if (got != tt.want) {
                Ꮡt.Errorf("for (%v, %v), Bits out = %v; want %v"u8, tt.ip, tt.@in, got, tt.want);
            }
        }
    }
}

internal partial struct TestParsePrefixAllocs_tests /*dyn*/ {
    internal @string ip;
    internal @string slash;
}

public static void TestParsePrefixAllocs(ж<testing.T> Ꮡt) {
    var tests = new TestParsePrefixAllocs_tests[]{
        new("192.168.1.0"u8, "/24"u8),
        new("aaaa:bbbb:cccc::"u8, "/24"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestParsePrefixAllocs_tests(), out var Ꮡtest);
        test = vᴛ1;

        @string prefix = test.ip + test.slash;
        var testʗ1 = test;
        Ꮡt.Run(prefix, (ж<testing.T> tΔ1) => {
            var testʗ2 = testʗ1;
            nint ipAllocs = (nint)testing.AllocsPerRun(5, () => {
                ParseAddr(testʗ2.ip);
            });
            nint prefixAllocs = (nint)testing.AllocsPerRun(5, () => {
                ParsePrefix(prefix);
            });
            {
                nint got = prefixAllocs - ipAllocs; if (got != 0) {
                    tΔ1.Errorf("allocs=%d, want 0"u8, got);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noErrorˢ = (@string)"no error"u8;

internal partial struct TestParsePrefixError_tests /*dyn*/ {
    internal @string prefix;
    internal @string errstr;
}

public static void TestParsePrefixError(ж<testing.T> Ꮡt) {
    var tests = new TestParsePrefixError_tests[]{
        new(
            prefix: "192.168.0.0"u8,
            errstr: "no '/'"u8
        ),
        new(
            prefix: "1.257.1.1/24"u8,
            errstr: "value >255"u8
        ),
        new(
            prefix: "1.1.1.0/q"u8,
            errstr: "bad bits"u8
        ),
        new(
            prefix: "1.1.1.0/-1"u8,
            errstr: "bad bits"u8
        ),
        new(
            prefix: "1.1.1.0/33"u8,
            errstr: "out of range"u8
        ),
        new(
            prefix: "2001::/129"u8,
            errstr: "out of range"u8
        ), // Zones are not allowed: https://go.dev/issue/51899

        new(
            prefix: "1.1.1.0%a/24"u8,
            errstr: "unexpected character"u8
        ),
        new(
            prefix: "2001:db8::%a/32"u8,
            errstr: "zones cannot be present"u8
        ),
        new(
            prefix: "1.1.1.0/+32"u8,
            errstr: "bad bits"u8
        ),
        new(
            prefix: "1.1.1.0/-32"u8,
            errstr: "bad bits"u8
        ),
        new(
            prefix: "1.1.1.0/032"u8,
            errstr: "bad bits"u8
        ),
        new(
            prefix: "1.1.1.0/0032"u8,
            errstr: "bad bits"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestParsePrefixError_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.prefix, (ж<testing.T> tΔ1) => {
            var (_, err) = ParsePrefix(testʗ1.prefix);
            if (err == default!) {
                tΔ1.Fatal(noErrorˢ);
            }
            {
                @string got = err.Error(); if (!strings.Contains(got, testʗ1.errstr)) {
                    tΔ1.Errorf("error is missing substring %q: %s"u8, testʗ1.errstr, got);
                }
            }
        });
    }
}

internal partial struct TestPrefixIsSingleIP_tests /*dyn*/ {
    internal netipꓸPrefix ipp;
    internal bool want;
}

public static void TestPrefixIsSingleIP(ж<testing.T> Ꮡt) {
    var tests = new TestPrefixIsSingleIP_tests[]{
        new(ipp: mustPrefix("127.0.0.1/32"u8), want: true),
        new(ipp: mustPrefix("127.0.0.1/31"u8), want: false),
        new(ipp: mustPrefix("127.0.0.1/0"u8), want: false),
        new(ipp: mustPrefix("::1/128"u8), want: true),
        new(ipp: mustPrefix("::1/127"u8), want: false),
        new(ipp: mustPrefix("::1/0"u8), want: false),
        new(ipp: new netipꓸPrefix(nil), want: false)
    }.slice();
    foreach (var (_, tt) in tests) {
        var got = tt.ipp.IsSingleIP();
        if (got != tt.want) {
            Ꮡt.Errorf("IsSingleIP(%v) = %v want %v"u8, tt.ipp, got, tt.want);
        }
    }
}

internal static slice<netipꓸAddr> mustIPs(params ꓸꓸꓸstring strsʗp) {
    var strs = strsʗp.sslice();

    slice<netipꓸAddr> res = default!;
    foreach (var (_, s) in strs) {
        res = append(res, mustIP(s));
    }
    return res;
}

internal partial struct BenchmarkBinaryMarshalRoundTrip_tests /*dyn*/ {
    internal @string name;
    internal @string ip;
}

public static void BenchmarkBinaryMarshalRoundTrip(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var tests = new BenchmarkBinaryMarshalRoundTrip_tests[]{
        new("ipv4"u8, "1.2.3.4"u8),
        new("ipv6"u8, "2001:db8::1"u8),
        new("ipv6+zone"u8, "2001:db8::1%eth0"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new BenchmarkBinaryMarshalRoundTrip_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡb.Run(tc.name, (ж<testing.B> bΔ1) => {
            var ip = mustIP(tcʗ1.ip);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var (bt, err) = ip.MarshalBinary();
                if (err != default!) {
                    bΔ1.Fatal(err);
                }
                netipꓸAddr ip2 = default!;
                {
                    var errΔ1 = ip2.UnmarshalBinary(bt); if (errΔ1 != default!) {
                        bΔ1.Fatal(errΔ1);
                    }
                }
            }
        });
    }
}

public static void BenchmarkStdIPv4(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ips = new net.IP[]{}.slice();
    for (nint i = 0; i < b.N; i++) {
        var ip = net.IPv4(8, 8, 8, 8);
        ips = ips[..0];
        for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
            ips = append(ips, ip);
        }
    }
}

public static void BenchmarkIPv4(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ips = new netipꓸAddr[]{}.slice();
    for (nint i = 0; i < b.N; i++) {
        var ip = IPv4(8, 8, 8, 8);
        ips = ips[..0];
        for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
            ips = append(ips, ip);
        }
    }
}

// ip4i was one of the possible representations of IP that came up in
// discussions, inlining IPv4 addresses, but having an "overflow"
// interface for IPv6 or IPv6 + zone. This is here for benchmarking.
partial struct ip4i {
    internal array<byte> ip4 = new(4);
    internal byte flags1;
    internal byte flags2;
    internal byte flags3;
    internal byte flags4;
    internal any ipv6;
}

internal static ip4i newip4i_v4(byte a, byte b, byte c, byte d) {
    return new ip4i(ip4: new byte[]{a, b, c, d}.array());
}

// BenchmarkIPv4_inline benchmarks the candidate representation, ip4i.
public static void BenchmarkIPv4_inline(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ips = new ip4i[]{}.slice();
    for (nint i = 0; i < b.N; i++) {
        var ip = newip4i_v4(8, 8, 8, 8);
        ips = ips[..0];
        for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
            ips = append(ips, ip.ΔClone());
        }
    }
}

public static void BenchmarkStdIPv6(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ips = new net.IP[]{}.slice();
    for (nint i = 0; i < b.N; i++) {
        var ip = net.ParseIP(db81ˢ);
        ips = ips[..0];
        for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
            ips = append(ips, ip);
        }
    }
}

public static void BenchmarkIPv6(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ips = new netipꓸAddr[]{}.slice();
    for (nint i = 0; i < b.N; i++) {
        var ip = mustIP(db81ˢ);
        ips = ips[..0];
        for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
            ips = append(ips, ip);
        }
    }
}

public static void BenchmarkIPv4Contains(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var prefix = PrefixFrom(IPv4(192, 168, 1, 0), 24);
    var ip = IPv4(192, 168, 1, 1);
    for (nint i = 0; i < b.N; i++) {
        prefix.Contains(ip);
    }
}

public static void BenchmarkIPv6Contains(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var prefix = MustParsePrefix("::1/128"u8);
    var ip = MustParseAddr("::1"u8);
    for (nint i = 0; i < b.N; i++) {
        prefix.Contains(ip);
    }
}


partial struct parseBenchInputsᴛ1 /*dyn*/ {
    internal @string name;
    internal @string ip;
}
internal static slice<parseBenchInputsᴛ1> parseBenchInputs = new parseBenchInputsᴛ1[]{
    new("v4"u8, "192.168.1.1"u8),
    new("v6"u8, "fd7a:115c:a1e0:ab12:4843:cd96:626b:430b"u8),
    new("v6_ellipsis"u8, "fd7a:115c::626b:430b"u8),
    new("v6_v4"u8, "::ffff:192.168.140.255"u8),
    new("v6_zone"u8, "1:2::ffff:192.168.140.255%eth1"u8)
}.slice();

public static void BenchmarkParseAddr(ж<testing.B> Ꮡb) {
    sinkInternValue = unique.Make<AddrDetail>(MakeAddrDetail(true, eth1ˢ)); // Pin to not benchmark the intern package
    foreach (var (_, vᴛ1) in parseBenchInputs) {
        ref var test = ref heap(new parseBenchInputsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (sinkIP, _) = ParseAddr(testʗ1.ip);
            }
        });
    }
}

public static void BenchmarkStdParseIP(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in parseBenchInputs) {
        ref var test = ref heap(new parseBenchInputsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                sinkStdIP = net.ParseIP(testʗ1.ip);
            }
        });
    }
}

public static void BenchmarkAddrString(ж<testing.B> Ꮡb) {
    foreach (var (_, test) in parseBenchInputs) {
        ref var ip = ref heap<netipꓸAddr>(out var Ꮡip);
        ip = MustParseAddr(test.ip);
        var ipʗ1 = ip;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                sinkString = ipʗ1.String();
            }
        });
    }
}

public static void BenchmarkIPStringExpanded(ж<testing.B> Ꮡb) {
    foreach (var (_, test) in parseBenchInputs) {
        ref var ip = ref heap<netipꓸAddr>(out var Ꮡip);
        ip = MustParseAddr(test.ip);
        var ipʗ1 = ip;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                sinkString = ipʗ1.StringExpanded();
            }
        });
    }
}

public static void BenchmarkAddrMarshalText(ж<testing.B> Ꮡb) {
    foreach (var (_, test) in parseBenchInputs) {
        ref var ip = ref heap<netipꓸAddr>(out var Ꮡip);
        ip = MustParseAddr(test.ip);
        var ipʗ1 = ip;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (sinkBytes, _) = ipʗ1.MarshalText();
            }
        });
    }
}

public static void BenchmarkAddrPortString(ж<testing.B> Ꮡb) {
    foreach (var (_, test) in parseBenchInputs) {
        var ip = MustParseAddr(test.ip);
        ref var ipp = ref heap<netip.AddrPort>(out var Ꮡipp);
        ipp = AddrPortFrom(ip, 60000);
        var ippʗ1 = ipp;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                sinkString = ippʗ1.String();
            }
        });
    }
}

public static void BenchmarkAddrPortMarshalText(ж<testing.B> Ꮡb) {
    foreach (var (_, test) in parseBenchInputs) {
        var ip = MustParseAddr(test.ip);
        ref var ipp = ref heap<netip.AddrPort>(out var Ꮡipp);
        ipp = AddrPortFrom(ip, 60000);
        var ippʗ1 = ipp;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (sinkBytes, _) = ippʗ1.MarshalText();
            }
        });
    }
}

internal partial struct BenchmarkPrefixMasking_tests /*dyn*/ {
    internal @string name;
    internal netipꓸAddr ip;
    internal nint bits;
}

public static void BenchmarkPrefixMasking(ж<testing.B> Ꮡb) {
    var tests = new BenchmarkPrefixMasking_tests[]{
        new(
            name: "IPv4 /32"u8,
            ip: IPv4(192, 0, 2, 0),
            bits: 32
        ),
        new(
            name: "IPv4 /17"u8,
            ip: IPv4(192, 0, 2, 0),
            bits: 17
        ),
        new(
            name: "IPv4 /0"u8,
            ip: IPv4(192, 0, 2, 0),
            bits: 0
        ),
        new(
            name: "IPv6 /128"u8,
            ip: mustIP(db81ˢ),
            bits: 128
        ),
        new(
            name: "IPv6 /65"u8,
            ip: mustIP(db81ˢ),
            bits: 65
        ),
        new(
            name: "IPv6 /0"u8,
            ip: mustIP(db81ˢ),
            bits: 0
        ),
        new(
            name: "IPv6 zone /128"u8,
            ip: mustIP(db81Eth0ˢ),
            bits: 128
        ),
        new(
            name: "IPv6 zone /65"u8,
            ip: mustIP(db81Eth0ˢ),
            bits: 65
        ),
        new(
            name: "IPv6 zone /0"u8,
            ip: mustIP(db81Eth0ˢ),
            bits: 0
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new BenchmarkPrefixMasking_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡb.Run(tt.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (sinkPrefix, _) = ttʗ1.ip.Prefix(ttʗ1.bits);
            }
        });
    }
}

public static void BenchmarkPrefixMarshalText(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ipp = MustParsePrefix("66.55.44.33/22"u8);
    for (nint i = 0; i < b.N; i++) {
        (sinkBytes, _) = ipp.MarshalText();
    }
}

public static void BenchmarkParseAddrPort(ж<testing.B> Ꮡb) {
    foreach (var (_, test) in parseBenchInputs) {
        @string ipp = default!;
        if (strings.HasPrefix(test.name, "v6"u8)){
            ipp = fmt.Sprintf("[%s]:1234"u8, test.ip);
        } else {
            ipp = fmt.Sprintf("%s:1234"u8, test.ip);
        }
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (sinkAddrPort, _) = ParseAddrPort(ipp);
            }
        });
    }
}

internal partial struct TestAs4_tests /*dyn*/ {
    internal netipꓸAddr ip;
    internal array<byte> want = new(4);
    internal bool wantPanic;
}

public static void TestAs4(ж<testing.T> Ꮡt) {
    var tests = new TestAs4_tests[]{
        new(
            ip: mustIP("1.2.3.4"u8),
            want: new byte[]{1, 2, 3, 4}.array()
        ),
        new(
            ip: AddrFrom16(mustIP("1.2.3.4"u8).As16()), // IPv4-in-IPv6

            want: new byte[]{1, 2, 3, 4}.array()
        ),
        new(
            ip: mustIP("0.0.0.0"u8),
            want: new byte[]{0, 0, 0, 0}.array()
        ),
        new(
            ip: new netipꓸAddr(nil),
            wantPanic: true
        ),
        new(
            ip: mustIP("::1"u8),
            wantPanic: true
        )
    }.slice();
    (array<byte> v, bool gotPanic) as4(netipꓸAddr ip) {
        array<byte> v = new(4);
        bool gotPanic = default!;
        GoFrame ᒐ = default;
        try {
            defer(() => {
                if (recover() != default!) {
                    gotPanic = true;
                    return;
                }
            }, ref ᒐ);
            v = ip.As4();
            goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return (v, gotPanic);
    }
    foreach (var (i, vᴛ1) in tests) {
        var tt = vᴛ1.ΔClone();

        var (got, gotPanic) = as4(tt.ip);
        if (gotPanic != tt.wantPanic) {
            Ꮡt.Errorf("%d. panic on %v = %v; want %v"u8, i, tt.ip, gotPanic, tt.wantPanic);
            continue;
        }
        if (got != tt.want) {
            Ꮡt.Errorf("%d. %v = %v; want %v"u8, i, tt.ip, got, tt.want);
        }
    }
}

internal partial struct TestPrefixOverlaps_tests /*dyn*/ {
    internal netipꓸPrefix a, b;
    internal bool want;
}

public static void TestPrefixOverlaps(ж<testing.T> Ꮡt) {
    var pfx = mustPrefix;
    var tests = new TestPrefixOverlaps_tests[]{
        new(new netipꓸPrefix(nil), pfx("1.2.0.0/16"u8), false), // first zero

        new(pfx("1.2.0.0/16"u8), new netipꓸPrefix(nil), false), // second zero

        new(pfx("::0/3"u8), pfx("0.0.0.0/3"u8), false), // different families

        new(pfx("1.2.0.0/16"u8), pfx("1.2.0.0/16"u8), true), // equal

        new(pfx("1.2.0.0/16"u8), pfx("1.2.3.0/24"u8), true),
        new(pfx("1.2.3.0/24"u8), pfx("1.2.0.0/16"u8), true),
        new(pfx("1.2.0.0/16"u8), pfx("1.2.3.0/32"u8), true),
        new(pfx("1.2.3.0/32"u8), pfx("1.2.0.0/16"u8), true), // Match /0 either order

        new(pfx("1.2.3.0/32"u8), pfx("0.0.0.0/0"u8), true),
        new(pfx("0.0.0.0/0"u8), pfx("1.2.3.0/32"u8), true),
        new(pfx("1.2.3.0/32"u8), pfx("5.5.5.5/0"u8), true), // normalization not required; /0 means true
 // IPv6 overlapping

        new(pfx("5::1/128"u8), pfx("5::0/8"u8), true),
        new(pfx("5::0/8"u8), pfx("5::1/128"u8), true), // IPv6 not overlapping

        new(pfx("1::1/128"u8), pfx("2::2/128"u8), false),
        new(pfx("0100::0/8"u8), pfx("::1/128"u8), false), // IPv4-mapped IPv6 addresses should not overlap with IPv4.

        new(PrefixFrom(AddrFrom16(mustIP("1.2.0.0"u8).As16()), 16), pfx("1.2.3.0/24"u8), false), // Invalid prefixes

        new(PrefixFrom(mustIP("1.2.3.4"u8), 33), pfx("1.2.3.0/24"u8), false),
        new(PrefixFrom(mustIP("2000::"u8), 129), pfx("2000::/64"u8), false)
    }.slice();
    foreach (var (i, tt) in tests) {
        {
            var got = tt.a.Overlaps(tt.b); if (got != tt.want) {
                Ꮡt.Errorf("%d. (%v).Overlaps(%v) = %v; want %v"u8, i, tt.a, tt.b, got, tt.want);
            }
        }
        // Overlaps is commutative
        {
            var got = tt.b.Overlaps(tt.a); if (got != tt.want) {
                Ꮡt.Errorf("%d. (%v).Overlaps(%v) = %v; want %v"u8, i, tt.b, tt.a, got, tt.want);
            }
        }
    }
}

// Sink variables are here to force the compiler to not elide
// seemingly useless work in benchmarks and allocation tests. If you
// were to just `_ = foo()` within a test function, the compiler could
// correctly deduce that foo() does nothing and doesn't need to be
// called. By writing results to a global variable, we hide that fact
// from the compiler and force it to keep the code under test.
internal static netipꓸAddr sinkIP;

internal static net.IP sinkStdIP;

internal static netip.AddrPort sinkAddrPort;

internal static netipꓸPrefix sinkPrefix;

internal static slice<netipꓸPrefix> sinkPrefixSlice;

internal static unique.Handle<AddrDetail> sinkInternValue;

internal static array<byte> sinkIP16 = new(16);

internal static array<byte> sinkIP4 = new(4);

internal static bool sinkBool;

internal static @string sinkString;

internal static slice<byte> sinkBytes;

internal static ж<net.UDPAddr> sinkUDPAddr = Ꮡ(new net.UDPAddr(IP: new net.IP(0, 16)));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;
internal static readonly @string iPv4ˢ = "IPv4"u8;
internal static readonly @string addrFrom4ˢ = "AddrFrom4"u8;
internal static readonly @string addrFrom16ˢ = "AddrFrom16"u8;
internal static readonly @string parseAddr4ˢ = "ParseAddr/4"u8;
internal static readonly @string parseAddr6ˢ = "ParseAddr/6"u8;
internal static readonly @string mustParseAddrˢ = "MustParseAddr"u8;
internal static readonly @string iPv6LinkLocalAllNodesˢ = "IPv6LinkLocalAllNodes"u8;
internal static readonly @string iPv6LinkLocalAllRoutersˢ = "IPv6LinkLocalAllRouters"u8;
internal static readonly @string iPv6Loopbackˢ = "IPv6Loopback"u8;
internal static readonly @string iPv6Unspecifiedˢ = "IPv6Unspecified"u8;
internal static readonly @string addrIsZeroˢ = "Addr.IsZero"u8;
internal static readonly @string addrBitLenˢ = "Addr.BitLen"u8;
internal static readonly @string addrZone4ˢ = "Addr.Zone/4"u8;
internal static readonly @string addrZone6ˢ = "Addr.Zone/6"u8;
internal static readonly @string addrZone6zoneˢ = "Addr.Zone/6zone"u8;
internal static readonly @string fe801Zoneˢ = "fe80::1%zone"u8;
internal static readonly @string addrCompareˢ = "Addr.Compare"u8;
internal static readonly @string addrLessˢ = "Addr.Less"u8;
internal static readonly @string addrIs4ˢ = "Addr.Is4"u8;
internal static readonly @string addrIs6ˢ = "Addr.Is6"u8;
internal static readonly @string addrIs4In6ˢ = "Addr.Is4In6"u8;
internal static readonly @string addrUnmapˢ = "Addr.Unmap"u8;
internal static readonly @string ffff2345ˢ = "ffff::2.3.4.5"u8;
internal static readonly @string addrWithZoneˢ = "Addr.WithZone"u8;
internal static readonly @string addrIsGlobalUnicastˢ = "Addr.IsGlobalUnicast"u8;
internal static readonly @string addrˢ = "Addr.IsInterfaceLocalMulticast"u8;
internal static readonly @string addrIsLinkLocalMulticastˢ = "Addr.IsLinkLocalMulticast"u8;
internal static readonly @string addrIsLinkLocalUnicastˢ = "Addr.IsLinkLocalUnicast"u8;
internal static readonly @string addrIsLoopbackˢ = "Addr.IsLoopback"u8;
internal static readonly @string addrIsMulticastˢ = "Addr.IsMulticast"u8;
internal static readonly @string addrIsPrivateˢ = "Addr.IsPrivate"u8;
internal static readonly @string addrIsUnspecifiedˢ = "Addr.IsUnspecified"u8;
internal static readonly @string addrPrefix4ˢ = "Addr.Prefix/4"u8;
internal static readonly @string addrPrefix6ˢ = "Addr.Prefix/6"u8;
internal static readonly @string addrAs16ˢ = "Addr.As16"u8;
internal static readonly @string addrAs4ˢ = "Addr.As4"u8;
internal static readonly @string addrNextˢ = "Addr.Next"u8;
internal static readonly @string addrPrevˢ = "Addr.Prev"u8;
internal static readonly @string addrPortFromˢ = "AddrPortFrom"u8;
internal static readonly @string parseAddrPortˢ = "ParseAddrPort"u8;
internal static readonly @string mustParseAddrPortˢ = "MustParseAddrPort"u8;
internal static readonly @string prefixFromˢ = "PrefixFrom"u8;
internal static readonly @string parsePrefix4ˢ = "ParsePrefix/4"u8;
internal static readonly @string parsePrefix6ˢ = "ParsePrefix/6"u8;
internal static readonly @string fe80164ˢ = "fe80::1/64"u8;
internal static readonly @string mustParsePrefixˢ = "MustParsePrefix"u8;
internal static readonly @string prefixContainsˢ = "Prefix.Contains"u8;
internal static readonly @string prefixOverlapsˢ = "Prefix.Overlaps"u8;
internal static readonly @string prefixIsZeroˢ = "Prefix.IsZero"u8;
internal static readonly @string prefixIsSingleIPˢ = "Prefix.IsSingleIP"u8;
internal static readonly @string prefixMaskedˢ = "Prefix.Masked"u8;

public static void TestNoAllocs(ж<testing.T> Ꮡt) {
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
    }
    // Wrappers that panic on error, to prove that our alloc-free
    // methods are returning successfully.
    netipꓸAddr panicIP(netipꓸAddr ip, error err) {
        if (err != default!) {
            throw panic(err);
        }
        return ip;
    }
    netipꓸPrefix panicPfx(netipꓸPrefix pfx, error err) {
        if (err != default!) {
            throw panic(err);
        }
        return pfx;
    }
    netip.AddrPort panicIPP(netip.AddrPort ipp, error err) {
        if (err != default!) {
            throw panic(err);
        }
        return ipp;
    }
    void test(@string name, Action f) {
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var n = testing.AllocsPerRun(1000, f);
            if (n != 0D) {
                tΔ1.Fatalf("allocs = %d; want 0"u8, (nint)n);
            }
        });
    }
    // Addr constructors
    test(iPv4ˢ, () => {
        sinkIP = IPv4(1, 2, 3, 4);
    });
    test(addrFrom4ˢ, () => {
        sinkIP = AddrFrom4(new byte[]{1, 2, 3, 4}.array());
    });
    test(addrFrom16ˢ, () => {
        sinkIP = AddrFrom16(new byte[]{}.array(16));
    });
    var panicIPʗ1 = panicIP;
    test(parseAddr4ˢ, () => {
        var (ᴛ1, ᴛ2) = ParseAddr("1.2.3.4"u8);
        sinkIP = panicIPʗ1(ᴛ1, ᴛ2);
    });
    var panicIPʗ2 = panicIP;
    test(parseAddr6ˢ, () => {
        var (ᴛ3, ᴛ4) = ParseAddr("::1"u8);
        sinkIP = panicIPʗ2(ᴛ3, ᴛ4);
    });
    test(mustParseAddrˢ, () => {
        sinkIP = MustParseAddr("1.2.3.4"u8);
    });
    test(iPv6LinkLocalAllNodesˢ, () => {
        sinkIP = IPv6LinkLocalAllNodes();
    });
    test(iPv6LinkLocalAllRoutersˢ, () => {
        sinkIP = IPv6LinkLocalAllRouters();
    });
    test(iPv6Loopbackˢ, () => {
        sinkIP = IPv6Loopback();
    });
    test(iPv6Unspecifiedˢ, () => {
        sinkIP = IPv6Unspecified();
    });
    // Addr methods
    test(addrIsZeroˢ, () => {
        sinkBool = MustParseAddr("1.2.3.4"u8).IsZero();
    });
    test(addrBitLenˢ, () => {
        sinkBool = MustParseAddr("1.2.3.4"u8).BitLen() == 8;
    });
    test(addrZone4ˢ, () => {
        sinkBool = MustParseAddr("1.2.3.4"u8).Zone() == ""u8;
    });
    test(addrZone6ˢ, () => {
        sinkBool = MustParseAddr(fe801ˢ).Zone() == ""u8;
    });
    test(addrZone6zoneˢ, () => {
        sinkBool = MustParseAddr(fe801Zoneˢ).Zone() == ""u8;
    });
    test(addrCompareˢ, () => {
        var a = MustParseAddr("1.2.3.4"u8);
        var b = MustParseAddr("2.3.4.5"u8);
        sinkBool = a.Compare(b) == 0;
    });
    test(addrLessˢ, () => {
        var a = MustParseAddr("1.2.3.4"u8);
        var b = MustParseAddr("2.3.4.5"u8);
        sinkBool = a.Less(b);
    });
    test(addrIs4ˢ, () => {
        sinkBool = MustParseAddr("1.2.3.4"u8).Is4();
    });
    test(addrIs6ˢ, () => {
        sinkBool = MustParseAddr(fe801ˢ).Is6();
    });
    test(addrIs4In6ˢ, () => {
        sinkBool = MustParseAddr(fe801ˢ).Is4In6();
    });
    test(addrUnmapˢ, () => {
        sinkIP = MustParseAddr(ffff2345ˢ).Unmap();
    });
    test(addrWithZoneˢ, () => {
        sinkIP = MustParseAddr(fe801ˢ).WithZone(""u8);
    });
    test(addrIsGlobalUnicastˢ, () => {
        sinkBool = MustParseAddr(db81ˢ).IsGlobalUnicast();
    });
    test(addrˢ, () => {
        sinkBool = MustParseAddr(fe801ˢ).IsInterfaceLocalMulticast();
    });
    test(addrIsLinkLocalMulticastˢ, () => {
        sinkBool = MustParseAddr(fe801ˢ).IsLinkLocalMulticast();
    });
    test(addrIsLinkLocalUnicastˢ, () => {
        sinkBool = MustParseAddr(fe801ˢ).IsLinkLocalUnicast();
    });
    test(addrIsLoopbackˢ, () => {
        sinkBool = MustParseAddr(fe801ˢ).IsLoopback();
    });
    test(addrIsMulticastˢ, () => {
        sinkBool = MustParseAddr(fe801ˢ).IsMulticast();
    });
    test(addrIsPrivateˢ, () => {
        sinkBool = MustParseAddr(fd001ˢ).IsPrivate();
    });
    test(addrIsUnspecifiedˢ, () => {
        sinkBool = IPv6Unspecified().IsUnspecified();
    });
    var panicPfxʗ1 = panicPfx;
    test(addrPrefix4ˢ, () => {
        var (ᴛ5, ᴛ6) = MustParseAddr("1.2.3.4"u8).Prefix(20);
        sinkPrefix = panicPfxʗ1(ᴛ5, ᴛ6);
    });
    var panicPfxʗ2 = panicPfx;
    test(addrPrefix6ˢ, () => {
        var (ᴛ7, ᴛ8) = MustParseAddr(fe801ˢ).Prefix(64);
        sinkPrefix = panicPfxʗ2(ᴛ7, ᴛ8);
    });
    test(addrAs16ˢ, () => {
        sinkIP16 = MustParseAddr("1.2.3.4"u8).As16();
    });
    test(addrAs4ˢ, () => {
        sinkIP4 = MustParseAddr("1.2.3.4"u8).As4();
    });
    test(addrNextˢ, () => {
        sinkIP = MustParseAddr("1.2.3.4"u8).Next();
    });
    test(addrPrevˢ, () => {
        sinkIP = MustParseAddr("1.2.3.4"u8).Prev();
    });
    // AddrPort constructors
    test(addrPortFromˢ, () => {
        sinkAddrPort = AddrPortFrom(IPv4(1, 2, 3, 4), 22);
    });
    var panicIPPʗ1 = panicIPP;
    test(parseAddrPortˢ, () => {
        var (ᴛ9, ᴛ10) = ParseAddrPort("[::1]:1234"u8);
        sinkAddrPort = panicIPPʗ1(ᴛ9, ᴛ10);
    });
    test(mustParseAddrPortˢ, () => {
        sinkAddrPort = MustParseAddrPort("[::1]:1234"u8);
    });
    // Prefix constructors
    test(prefixFromˢ, () => {
        sinkPrefix = PrefixFrom(IPv4(1, 2, 3, 4), 32);
    });
    var panicPfxʗ3 = panicPfx;
    test(parsePrefix4ˢ, () => {
        var (ᴛ11, ᴛ12) = ParsePrefix("1.2.3.4/20"u8);
        sinkPrefix = panicPfxʗ3(ᴛ11, ᴛ12);
    });
    var panicPfxʗ4 = panicPfx;
    test(parsePrefix6ˢ, () => {
        var (ᴛ13, ᴛ14) = ParsePrefix(fe80164ˢ);
        sinkPrefix = panicPfxʗ4(ᴛ13, ᴛ14);
    });
    test(mustParsePrefixˢ, () => {
        sinkPrefix = MustParsePrefix("1.2.3.4/20"u8);
    });
    // Prefix methods
    test(prefixContainsˢ, () => {
        sinkBool = MustParsePrefix("1.2.3.0/24"u8).Contains(MustParseAddr("1.2.3.4"u8));
    });
    test(prefixOverlapsˢ, () => {
        var (a, b) = (MustParsePrefix("1.2.3.0/24"u8), MustParsePrefix("1.2.0.0/16"u8));
        sinkBool = a.Overlaps(b);
    });
    test(prefixIsZeroˢ, () => {
        sinkBool = MustParsePrefix("1.2.0.0/16"u8).IsZero();
    });
    test(prefixIsSingleIPˢ, () => {
        sinkBool = MustParsePrefix("1.2.3.4/32"u8).IsSingleIP();
    });
    test(prefixMaskedˢ, () => {
        sinkPrefix = MustParsePrefix("1.2.3.4/16"u8).Masked();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ffff19216811Eth0ˢ = "::ffff:192.168.1.1%eth0"u8;
internal static readonly @string ipv4InIpv6ˢ = "ipv4-in-ipv6"u8;

internal partial struct TestAddrStringAllocs_tests /*dyn*/ {
    internal @string name;
    internal netipꓸAddr ip;
    internal nint wantAllocs;
}

public static void TestAddrStringAllocs(ж<testing.T> Ꮡt) {
    var tests = new TestAddrStringAllocs_tests[]{
        new("zero"u8, new netipꓸAddr(nil), 0),
        new("ipv4"u8, MustParseAddr("192.168.1.1"u8), 1),
        new("ipv6"u8, MustParseAddr(db81ˢ), 1),
        new("ipv6+zone"u8, MustParseAddr(db81Eth0ˢ), 1),
        new("ipv4-in-ipv6"u8, MustParseAddr(ffff19216811ˢ), 1),
        new("ipv4-in-ipv6+zone"u8, MustParseAddr(ffff19216811Eth0ˢ), 1)
    }.slice();
    var optimizationOff = testenv.OptimizationOff();
    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestAddrStringAllocs_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            if (optimizationOff && strings.HasPrefix(tcʗ1.name, ipv4InIpv6ˢ)) {
                // Optimizations are required to remove some allocs.
                tΔ1.Skipf("skipping on %v"u8, testenv.Builder());
            }
            var tcʗ2 = tcʗ1;
            nint allocs = (nint)testing.AllocsPerRun(1000, () => {
                sinkString = tcʗ2.ip.String();
            });
            if (allocs != tcʗ1.wantAllocs) {
                tΔ1.Errorf("allocs=%d, want %d"u8, allocs, tcʗ1.wantAllocs);
            }
        });
    }
}

internal partial struct TestPrefixString_tests /*dyn*/ {
    internal netipꓸPrefix ipp;
    internal @string want;
}

public static void TestPrefixString(ж<testing.T> Ꮡt) {
    var tests = new TestPrefixString_tests[]{
        new(new netipꓸPrefix(nil), "invalid Prefix"u8),
        new(PrefixFrom(new netipꓸAddr(nil), 8), "invalid Prefix"u8),
        new(PrefixFrom(MustParseAddr("1.2.3.4"u8), 88), "invalid Prefix"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        {
            @string got = tt.ipp.String(); if (got != tt.want) {
                Ꮡt.Errorf("(%#v).String() = %q want %q"u8, tt.ipp, got, tt.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ffff18080ˢ = "[FFFF::1]:8080"u8;

internal partial struct TestAddrPortString_tests /*dyn*/ {
    internal netip.AddrPort ipp;
    internal @string want;
}

public static void TestAddrPortString(ж<testing.T> Ꮡt) {
    var tests = new TestAddrPortString_tests[]{
        new(MustParseAddrPort("127.0.0.1:80"u8), "127.0.0.1:80"u8),
        new(MustParseAddrPort("[0000::0]:8080"u8), "[::]:8080"u8),
        new(MustParseAddrPort(ffff18080ˢ), "[ffff::1]:8080"u8),
        new(new AddrPort(nil), "invalid AddrPort"u8),
        new(AddrPortFrom(new netipꓸAddr(nil), 80), "invalid AddrPort"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        {
            @string got = tt.ipp.String(); if (got != tt.want) {
                Ꮡt.Errorf("(%#v).String() = %q want %q"u8, tt.ipp, got, tt.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ffff1ˢ = "ffff::1"u8;

internal partial struct TestAsSlice_tests /*dyn*/ {
    internal netipꓸAddr @in;
    internal slice<byte> want;
}

public static void TestAsSlice(ж<testing.T> Ꮡt) {
    var tests = new TestAsSlice_tests[]{
        new(@in: new netipꓸAddr(nil), want: default!),
        new(@in: mustIP("1.2.3.4"u8), want: new byte[]{1, 2, 3, 4}.slice()),
        new(@in: mustIP(ffff1ˢ), want: new slice<byte>(16){[0] = 0xff, [1] = 0xff, [15] = 1})
    }.slice();
    foreach (var (_, test) in tests) {
        var got = test.@in.AsSlice();
        if (!bytes.Equal(got, test.want)) {
            Ꮡt.Errorf("%v.AsSlice() = %v want %v"u8, test.@in, got, test.want);
        }
    }
}

internal static array<byte> sink16 = new(16);

public static void BenchmarkAs16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var addr = MustParseAddr("1::10"u8);
    for (nint i = 0; i < b.N; i++) {
        sink16 = addr.As16();
    }
}

} // end netip_test_package
