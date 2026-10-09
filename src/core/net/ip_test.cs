// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using rand = math.rand_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using testing = testing_package;
using math;
using static go.net_package;

partial class net_internal_test_package {

//6 zeroes in one group
//5 zeroes in one group edge case
// Issue 6628

partial struct parseIPTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal global::go.net_package.IP @out;
}
internal static slice<parseIPTestsᴛ1> parseIPTests;
internal static void initᴛparseIPTests() { parseIPTests = new parseIPTestsᴛ1[]{
    new("127.0.1.2"u8, IPv4(127, 0, 1, 2)),
    new("127.0.0.1"u8, IPv4(127, 0, 0, 1)),
    new("::ffff:127.1.2.3"u8, IPv4(127, 1, 2, 3)),
    new("::ffff:7f01:0203"u8, IPv4(127, 1, 2, 3)),
    new("0:0:0:0:0000:ffff:127.1.2.3"u8, IPv4(127, 1, 2, 3)),
    new("0:0:0:0::ffff:127.1.2.3"u8, IPv4(127, 1, 2, 3)),
    new("2001:4860:0:2001::68"u8, new IP(new byte[]{0x20, 0x01, 0x48, 0x60, 0, 0, 0x20, 0x01, 0, 0, 0, 0, 0, 0, 0x00, 0x68}.slice())),
    new("2001:4860:0000:2001:0000:0000:0000:0068"u8, new IP(new byte[]{0x20, 0x01, 0x48, 0x60, 0, 0, 0x20, 0x01, 0, 0, 0, 0, 0, 0, 0x00, 0x68}.slice())),
    new("-0.0.0.0"u8, default!),
    new("0.-1.0.0"u8, default!),
    new("0.0.-2.0"u8, default!),
    new("0.0.0.-3"u8, default!),
    new("127.0.0.256"u8, default!),
    new("abc"u8, default!),
    new("123:"u8, default!),
    new("fe80::1%lo0"u8, default!),
    new("fe80::1%911"u8, default!),
    new(""u8, default!),
    new("0:0:0:0:000000:ffff:127.1.2.3"u8, default!),
    new("0:0:0:0:00000:ffff:127.1.2.3"u8, default!),
    new("a1:a2:a3:a4::b1:b2:b3:b4"u8, default!),
    new("127.001.002.003"u8, default!),
    new("::ffff:127.001.002.003"u8, default!),
    new("123.000.000.000"u8, default!),
    new("1.2..4"u8, default!),
    new("0123.0.0.1"u8, default!)
}.slice(); }

public static void TestParseIP(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in parseIPTests) {
        {
            var outΔ1 = ParseIP(tt.@in); if (!reflect.DeepEqual(outΔ1, tt.@out)) {
                Ꮡt.Errorf("ParseIP(%q) = %v, want %v"u8, tt.@in, outΔ1, tt.@out);
            }
        }
        if (tt.@in == ""u8) {
            // Tested in TestMarshalEmptyIP below.
            continue;
        }
        global::go.net_package.IP @out = default!;
        {
            var err = @out.UnmarshalText(slice<byte>(tt.@in)); if (!reflect.DeepEqual(@out, tt.@out) || (tt.@out == default!) != (err != default!)) {
                Ꮡt.Errorf("IP.UnmarshalText(%q) = %v, %v, want %v"u8, tt.@in, @out, err, tt.@out);
            }
        }
    }
}

public static void TestLookupWithIP(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (_, err) = LookupIP(""u8);
    if (err == default!) {
        Ꮡt.Errorf(@"LookupIP("""") succeeded, should fail"u8);
    }
    (_, err) = LookupHost(""u8);
    if (err == default!) {
        Ꮡt.Errorf(@"LookupIP("""") succeeded, should fail"u8);
    }
    // Test that LookupHost and LookupIP, which normally
    // expect host names, work with IP addresses.
    foreach (var (_, tt) in parseIPTests) {
        if (tt.@out != default!){
            var (addrs, errΔ1) = LookupHost(tt.@in);
            if (len(addrs) != 1 || addrs[0] != tt.@in || errΔ1 != default!) {
                Ꮡt.Errorf("LookupHost(%q) = %v, %v, want %v, nil"u8, tt.@in, addrs, errΔ1, new @string[]{tt.@in}.slice());
            }
        } else 
        if (!testing.Short()) {
            // We can't control what the host resolver does; if it can resolve, say,
            // 127.0.0.256 or fe80::1%911 or a host named 'abc', who are we to judge?
            // Warn about these discrepancies but don't fail the test.
            var (addrs, errΔ2) = LookupHost(tt.@in);
            if (errΔ2 == default!) {
                Ꮡt.Logf("warning: LookupHost(%q) = %v, want error"u8, tt.@in, addrs);
            }
        }
        if (tt.@out != default!){
            var (ips, errΔ3) = LookupIP(tt.@in);
            if (len(ips) != 1 || !reflect.DeepEqual(ips[0], tt.@out) || errΔ3 != default!) {
                Ꮡt.Errorf("LookupIP(%q) = %v, %v, want %v, nil"u8, tt.@in, ips, errΔ3, new global::go.net_package.IP[]{tt.@out}.slice());
            }
        } else 
        if (!testing.Short()) {
            var (ips, errΔ4) = LookupIP(tt.@in);
            // We can't control what the host resolver does. See above.
            if (errΔ4 == default!) {
                Ꮡt.Logf("warning: LookupIP(%q) = %v, want error"u8, tt.@in, ips);
            }
        }
    }
}

public static void BenchmarkParseIP(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, tt) in parseIPTests) {
            ParseIP(tt.@in);
        }
    }
}

public static void BenchmarkParseIPValidIPv4(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    for (nint i = 0; i < b.N; i++) {
        ParseIP("192.0.2.1"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string db81ˢ = "2001:DB8::1"u8;

public static void BenchmarkParseIPValidIPv6(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    for (nint i = 0; i < b.N; i++) {
        ParseIP(db81ˢ);
    }
}

// Issue 6339
public static void TestMarshalEmptyIP(ж<testing.T> Ꮡt) {
    foreach (var (_, @in) in new slice<byte>[]{default!, slice<byte>(""u8)}.slice()) {
        global::go.net_package.IP @out = new IP(new byte[]{1, 2, 3, 4}.slice());
        {
            var errΔ1 = @out.UnmarshalText(@in); if (errΔ1 != default! || @out != default!) {
                Ꮡt.Errorf("UnmarshalText(%v) = %v, %v; want nil, nil"u8, @in, @out, errΔ1);
            }
        }
    }
    global::go.net_package.IP ip = default!;
    var (got, err) = ip.MarshalText();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!reflect.DeepEqual(got, slice<byte>(""u8))) {
        Ꮡt.Errorf(@"got %#v, want []byte("""")"u8, got);
    }
    var buf = new slice<byte>(4);
    (got, err) = ip.AppendText(buf);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!reflect.DeepEqual(got, slice<byte>("\x00\x00\x00\x00"u8))) {
        Ꮡt.Errorf(@"got %#v, want []byte(""\x00\x00\x00\x00"")"u8, got);
    }
}

// IPv4 address
// IPv4-mapped IPv6 address
// IPv6 address
// IP wildcard equivalent address in Dial/Listen API
// Opaque byte sequence

partial struct ipStringTestsᴛ1 /*dyn*/ {
    internal global::go.net_package.IP @in;     // see RFC 791 and RFC 4291
    internal @string str; // see RFC 791, RFC 4291 and RFC 5952
    internal slice<byte> byt;
    /*embed*/ internal error error;
}
internal static slice<ж<ipStringTestsᴛ1>> ipStringTests;
internal static void initᴛipStringTests() { ipStringTests = new ж<ipStringTestsᴛ1>[]{
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{192, 0, 2, 1}.slice()),
        "192.0.2.1"u8,
        slice<byte>("192.0.2.1"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0, 0, 0, 0}.slice()),
        "0.0.0.0"u8,
        slice<byte>("0.0.0.0"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xff, 0xff, 192, 0, 2, 1}.slice()),
        "192.0.2.1"u8,
        slice<byte>("192.0.2.1"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xff, 0xff, 0, 0, 0, 0}.slice()),
        "0.0.0.0"u8,
        slice<byte>("0.0.0.0"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0x20, 0x1, 0xd, 0xb8, 0, 0, 0, 0, 0, 0, 0x1, 0x23, 0, 0x12, 0, 0x1}.slice()),
        "2001:db8::123:12:1"u8,
        slice<byte>("2001:db8::123:12:1"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0x20, 0x1, 0xd, 0xb8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x1}.slice()),
        "2001:db8::1"u8,
        slice<byte>("2001:db8::1"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0x20, 0x1, 0xd, 0xb8, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x1}.slice()),
        "2001:db8:0:1:0:1:0:1"u8,
        slice<byte>("2001:db8:0:1:0:1:0:1"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0x20, 0x1, 0xd, 0xb8, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0}.slice()),
        "2001:db8:1:0:1:0:1:0"u8,
        slice<byte>("2001:db8:1:0:1:0:1:0"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0x20, 0x1, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0x1}.slice()),
        "2001::1:0:0:1"u8,
        slice<byte>("2001::1:0:0:1"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0x20, 0x1, 0xd, 0xb8, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0}.slice()),
        "2001:db8:0:0:1::"u8,
        slice<byte>("2001:db8:0:0:1::"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0x20, 0x1, 0xd, 0xb8, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0x1}.slice()),
        "2001:db8::1:0:0:1"u8,
        slice<byte>("2001:db8::1:0:0:1"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0x20, 0x1, 0xd, 0xb8, 0, 0, 0, 0, 0, 0xa, 0, 0xb, 0, 0xc, 0, 0xd}.slice()),
        "2001:db8::a:b:c:d"u8,
        slice<byte>("2001:db8::a:b:c:d"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        IPv6unspecified,
        "::"u8,
        slice<byte>("::"u8),
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        default!,
        "<nil>"u8,
        default!,
        default!)),
    Ꮡ(new ipStringTestsᴛ1(
        new IP(new byte[]{0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef}.slice()),
        "?0123456789abcdef"u8,
        default!,
        new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "invalid IP address"u8, Addr: "0123456789abcdef"u8)))))
}.slice(); }

public static void TestIPString(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in ipStringTests) {
        {
            @string @out = (~tt).@in.String(); if (@out != (~tt).str) {
                Ꮡt.Errorf("IP.String(%v) = %q, want %q"u8, (~tt).@in, @out, (~tt).str);
            }
        }
        {
            var (@out, err) = (~tt).@in.MarshalText(); if (!bytes.Equal(@out, (~tt).byt) || !reflect.DeepEqual(err, (~tt).error)) {
                Ꮡt.Errorf("IP.MarshalText(%v) = %v, %v, want %v, %v"u8, (~tt).@in, @out, err, (~tt).byt, (~tt).error);
            }
        }
        var buf = new slice<byte>(4, 32);
        {
            var (@out, err) = (~tt).@in.AppendText(buf); if (!bytes.Equal(@out[4..], (~tt).byt) || !reflect.DeepEqual(err, (~tt).error)) {
                Ꮡt.Errorf("IP.AppendText(%v) = %v, %v, want %v, %v"u8, (~tt).@in, @out[4..], err, (~tt).byt, (~tt).error);
            }
        }
    }
}

public static void TestIPAppendTextNoAllocs(ж<testing.T> Ꮡt) {
    // except the invalid IP
    foreach (var (_, tt) in ipStringTests.slice(0, len(ipStringTests) - 1)) {
        var ttʗ1 = tt;
        nint allocs = (nint)testing.AllocsPerRun(1000, () => {
            var buf = new slice<byte>(0, 64);
            (_, _) = (~ttʗ1).@in.AppendText(buf);
        });
        if (allocs != 0) {
            Ꮡt.Errorf("IP(%q) AppendText allocs: %d times, want 0"u8, (~tt).@in, allocs);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string iPv4ˢ = "IPv4"u8;
internal static readonly @string iPv6ˢ = "IPv6"u8;
internal static readonly @string iPv6Longˢ = "IPv6_long"u8;

public static void BenchmarkIPMarshalText(ж<testing.B> Ꮡb) {
    Ꮡb.Run(iPv4ˢ, (ж<testing.B> bΔ1) => {
        bΔ1.ReportAllocs();
        bΔ1.ResetTimer();
        var ip = new IP(new byte[]{192, 0, 2, 1}.slice());
        foreach (var _ᴛ1 in range((~bΔ1).N)) {
            (_, _) = ip.MarshalText();
        }
    });
    Ꮡb.Run(iPv6ˢ, (ж<testing.B> bΔ2) => {
        bΔ2.ReportAllocs();
        bΔ2.ResetTimer();
        var ip = new IP(new byte[]{0x20, 0x1, 0xd, 0xb8, 0, 0, 0, 0, 0, 0xa, 0, 0xb, 0, 0xc, 0, 0xd}.slice());
        foreach (var _ᴛ2 in range((~bΔ2).N)) {
            (_, _) = ip.MarshalText();
        }
    });
    Ꮡb.Run(iPv6Longˢ, (ж<testing.B> bΔ3) => {
        bΔ3.ReportAllocs();
        bΔ3.ResetTimer();
        // fd7a:115c:a1e0:ab12:4843:cd96:626b:430b
        var ip = new IP(new byte[]{253, 122, 17, 92, 161, 224, 171, 18, 72, 67, 205, 150, 98, 107, 67, 11}.slice());
        foreach (var _ᴛ3 in range((~bΔ3).N)) {
            (_, _) = ip.MarshalText();
        }
    });
}

internal static @string sink;

public static void BenchmarkIPString(ж<testing.B> Ꮡb) {
    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    Ꮡb.Run(iPv4ˢ, (ж<testing.B> bΔ1) => {
        benchmarkIPString(bΔ1, IPv4len);
    });
    Ꮡb.Run(iPv6ˢ, (ж<testing.B> bΔ2) => {
        benchmarkIPString(bΔ2, IPv6len);
    });
}

internal static void benchmarkIPString(ж<testing.B> Ꮡb, nint size) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, tt) in ipStringTests) {
            if ((~tt).@in != default! && len((~tt).@in) == size) {
                sink = (~tt).@in.String();
            }
        }
    }
}


partial struct ipMaskTestsᴛ1 /*dyn*/ {
    internal global::go.net_package.IP @in;
    internal global::go.net_package.IPMask mask;
    internal global::go.net_package.IP @out;
}
internal static slice<ipMaskTestsᴛ1> ipMaskTests;
internal static void initᴛipMaskTests() { ipMaskTests = new ipMaskTestsᴛ1[]{
    new(IPv4(192, 168, 1, 127), IPv4Mask(255, 255, 255, 128), IPv4(192, 168, 1, 0)),
    new(IPv4(192, 168, 1, 127), ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("255.255.255.192"u8))), IPv4(192, 168, 1, 64)),
    new(IPv4(192, 168, 1, 127), ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffe0"u8))), IPv4(192, 168, 1, 96)),
    new(IPv4(192, 168, 1, 127), IPv4Mask(255, 0, 255, 0), IPv4(192, 0, 1, 0)),
    new(ParseIP("2001:db8::1"u8), ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ff80::"u8))), ParseIP("2001:d80::"u8)),
    new(ParseIP("2001:db8::1"u8), ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("f0f0:0f0f::"u8))), ParseIP("2000:d08::"u8))
}.slice(); }

public static void TestIPMask(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in ipMaskTests) {
        {
            var @out = tt.@in.Mask(tt.mask); if (@out == default! || !tt.@out.Equal(@out)) {
                Ꮡt.Errorf("IP(%v).Mask(%v) = %v, want %v"u8, tt.@in, tt.mask, @out, tt.@out);
            }
        }
    }
}


partial struct ipMaskStringTestsᴛ1 /*dyn*/ {
    internal global::go.net_package.IPMask @in;
    internal @string @out;
}
internal static slice<ipMaskStringTestsᴛ1> ipMaskStringTests = new ipMaskStringTestsᴛ1[]{
    new(IPv4Mask(255, 255, 255, 240), "fffffff0"u8),
    new(IPv4Mask(255, 0, 128, 0), "ff008000"u8),
    new(((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ff80::"u8))), "ffffff80000000000000000000000000"u8),
    new(((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ef00:ff80::cafe:0"u8))), "ef00ff800000000000000000cafe0000"u8),
    new(default!, "<nil>"u8)
}.slice();

public static void TestIPMaskString(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in ipMaskStringTests) {
        {
            @string @out = tt.@in.String(); if (@out != tt.@out) {
                Ꮡt.Errorf("IPMask.String(%v) = %q, want %q"u8, tt.@in, @out, tt.@out);
            }
        }
    }
}

public static void BenchmarkIPMaskString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ᏑtestHookUninstaller.Do(uninstallTestHooks);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, tt) in ipMaskStringTests) {
            sink = tt.@in.String();
        }
    }
}


partial struct parseCIDRTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal global::go.net_package.IP ip;
    internal ж<global::go.net_package.IPNet> net;
    internal error err;
}
internal static slice<parseCIDRTestsᴛ1> parseCIDRTests;
internal static void initᴛparseCIDRTests() { parseCIDRTests = new parseCIDRTestsᴛ1[]{
    new("135.104.0.0/32"u8, IPv4(135, 104, 0, 0), Ꮡ(new IPNet(IP: IPv4(135, 104, 0, 0), Mask: IPv4Mask(255, 255, 255, 255))), default!),
    new("0.0.0.0/24"u8, IPv4(0, 0, 0, 0), Ꮡ(new IPNet(IP: IPv4(0, 0, 0, 0), Mask: IPv4Mask(255, 255, 255, 0))), default!),
    new("135.104.0.0/24"u8, IPv4(135, 104, 0, 0), Ꮡ(new IPNet(IP: IPv4(135, 104, 0, 0), Mask: IPv4Mask(255, 255, 255, 0))), default!),
    new("135.104.0.1/32"u8, IPv4(135, 104, 0, 1), Ꮡ(new IPNet(IP: IPv4(135, 104, 0, 1), Mask: IPv4Mask(255, 255, 255, 255))), default!),
    new("135.104.0.1/24"u8, IPv4(135, 104, 0, 1), Ꮡ(new IPNet(IP: IPv4(135, 104, 0, 0), Mask: IPv4Mask(255, 255, 255, 0))), default!),
    new("::1/128"u8, ParseIP("::1"u8), Ꮡ(new IPNet(IP: ParseIP("::1"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"u8))))), default!),
    new("abcd:2345::/127"u8, ParseIP("abcd:2345::"u8), Ꮡ(new IPNet(IP: ParseIP("abcd:2345::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe"u8))))), default!),
    new("abcd:2345::/65"u8, ParseIP("abcd:2345::"u8), Ꮡ(new IPNet(IP: ParseIP("abcd:2345::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ffff:ffff:ffff:8000::"u8))))), default!),
    new("abcd:2345::/64"u8, ParseIP("abcd:2345::"u8), Ꮡ(new IPNet(IP: ParseIP("abcd:2345::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ffff:ffff:ffff::"u8))))), default!),
    new("abcd:2345::/63"u8, ParseIP("abcd:2345::"u8), Ꮡ(new IPNet(IP: ParseIP("abcd:2345::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ffff:ffff:fffe::"u8))))), default!),
    new("abcd:2345::/33"u8, ParseIP("abcd:2345::"u8), Ꮡ(new IPNet(IP: ParseIP("abcd:2345::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ffff:8000::"u8))))), default!),
    new("abcd:2345::/32"u8, ParseIP("abcd:2345::"u8), Ꮡ(new IPNet(IP: ParseIP("abcd:2345::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ffff::"u8))))), default!),
    new("abcd:2344::/31"u8, ParseIP("abcd:2344::"u8), Ꮡ(new IPNet(IP: ParseIP("abcd:2344::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:fffe::"u8))))), default!),
    new("abcd:2300::/24"u8, ParseIP("abcd:2300::"u8), Ꮡ(new IPNet(IP: ParseIP("abcd:2300::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ff00::"u8))))), default!),
    new("abcd:2345::/24"u8, ParseIP("abcd:2345::"u8), Ꮡ(new IPNet(IP: ParseIP("abcd:2300::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ff00::"u8))))), default!),
    new("2001:DB8::/48"u8, ParseIP("2001:DB8::"u8), Ꮡ(new IPNet(IP: ParseIP("2001:DB8::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ffff:ffff::"u8))))), default!),
    new("2001:DB8::1/48"u8, ParseIP("2001:DB8::1"u8), Ꮡ(new IPNet(IP: ParseIP("2001:DB8::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:ffff:ffff::"u8))))), default!),
    new("192.168.1.1/255.255.255.0"u8, default!, nil, new global::go.net_package.ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: "192.168.1.1/255.255.255.0"u8)))),
    new("192.168.1.1/35"u8, default!, nil, new global::go.net_package.ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: "192.168.1.1/35"u8)))),
    new("2001:db8::1/-1"u8, default!, nil, new global::go.net_package.ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: "2001:db8::1/-1"u8)))),
    new("2001:db8::1/-0"u8, default!, nil, new global::go.net_package.ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: "2001:db8::1/-0"u8)))),
    new("-0.0.0.0/32"u8, default!, nil, new global::go.net_package.ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: "-0.0.0.0/32"u8)))),
    new("0.-1.0.0/32"u8, default!, nil, new global::go.net_package.ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: "0.-1.0.0/32"u8)))),
    new("0.0.-2.0/32"u8, default!, nil, new global::go.net_package.ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: "0.0.-2.0/32"u8)))),
    new("0.0.0.-3/32"u8, default!, nil, new global::go.net_package.ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: "0.0.0.-3/32"u8)))),
    new("0.0.0.0/-0"u8, default!, nil, new global::go.net_package.ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: "0.0.0.0/-0"u8)))),
    new("127.000.000.001/32"u8, default!, nil, new global::go.net_package.ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: "127.000.000.001/32"u8)))),
    new(""u8, default!, nil, new global::go.net_package.ParseErrorжerror(Ꮡ(new ParseError(Type: "CIDR address"u8, Text: ""u8))))
}.slice(); }

public static void TestParseCIDR(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in parseCIDRTests) {
        var (ip, net, err) = ParseCIDR(tt.@in);
        if (!reflect.DeepEqual(err, tt.err)) {
            Ꮡt.Errorf("ParseCIDR(%q) = %v, %v; want %v, %v"u8, tt.@in, ip, net.OrTypedNil(), tt.ip, tt.net.OrTypedNil());
        }
        if (err == default! && (!tt.ip.Equal(ip) || !(~tt.net).IP.Equal((~net).IP) || !reflect.DeepEqual((~net).Mask, (~tt.net).Mask))) {
            Ꮡt.Errorf("ParseCIDR(%q) = %v, {%v, %v}; want %v, {%v, %v}"u8, tt.@in, ip, (~net).IP, (~net).Mask, tt.ip, (~tt.net).IP, (~tt.net).Mask);
        }
    }
}


partial struct ipNetContainsTestsᴛ1 /*dyn*/ {
    internal global::go.net_package.IP ip;
    internal ж<global::go.net_package.IPNet> net;
    internal bool ok;
}
internal static slice<ipNetContainsTestsᴛ1> ipNetContainsTests;
internal static void initᴛipNetContainsTests() { ipNetContainsTests = new ipNetContainsTestsᴛ1[]{
    new(IPv4(172, 16, 1, 1), Ꮡ(new IPNet(IP: IPv4(172, 16, 0, 0), Mask: CIDRMask(12, 32))), true),
    new(IPv4(172, 24, 0, 1), Ꮡ(new IPNet(IP: IPv4(172, 16, 0, 0), Mask: CIDRMask(13, 32))), false),
    new(IPv4(192, 168, 0, 3), Ꮡ(new IPNet(IP: IPv4(192, 168, 0, 0), Mask: IPv4Mask(0, 0, 255, 252))), true),
    new(IPv4(192, 168, 0, 4), Ꮡ(new IPNet(IP: IPv4(192, 168, 0, 0), Mask: IPv4Mask(0, 255, 0, 252))), false),
    new(ParseIP("2001:db8:1:2::1"u8), Ꮡ(new IPNet(IP: ParseIP("2001:db8:1::"u8), Mask: CIDRMask(47, 128))), true),
    new(ParseIP("2001:db8:1:2::1"u8), Ꮡ(new IPNet(IP: ParseIP("2001:db8:2::"u8), Mask: CIDRMask(47, 128))), false),
    new(ParseIP("2001:db8:1:2::1"u8), Ꮡ(new IPNet(IP: ParseIP("2001:db8:1::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("ffff:0:ffff::"u8))))), true),
    new(ParseIP("2001:db8:1:2::1"u8), Ꮡ(new IPNet(IP: ParseIP("2001:db8:1::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("0:0:0:ffff::"u8))))), false)
}.slice(); }

public static void TestIPNetContains(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in ipNetContainsTests) {
        {
            var ok = tt.net.Contains(tt.ip); if (ok != tt.ok) {
                Ꮡt.Errorf("IPNet(%v).Contains(%v) = %v, want %v"u8, tt.net.OrTypedNil(), tt.ip, ok, tt.ok);
            }
        }
    }
}


partial struct ipNetStringTestsᴛ1 /*dyn*/ {
    internal ж<global::go.net_package.IPNet> @in;
    internal @string @out;
}
internal static slice<ipNetStringTestsᴛ1> ipNetStringTests;
internal static void initᴛipNetStringTests() { ipNetStringTests = new ipNetStringTestsᴛ1[]{
    new(Ꮡ(new IPNet(IP: IPv4(192, 168, 1, 0), Mask: CIDRMask(26, 32))), "192.168.1.0/26"u8),
    new(Ꮡ(new IPNet(IP: IPv4(192, 168, 1, 0), Mask: IPv4Mask(255, 0, 255, 0))), "192.168.1.0/ff00ff00"u8),
    new(Ꮡ(new IPNet(IP: ParseIP("2001:db8::"u8), Mask: CIDRMask(55, 128))), "2001:db8::/55"u8),
    new(Ꮡ(new IPNet(IP: ParseIP("2001:db8::"u8), Mask: ((global::go.net_package.IPMask)(slice<byte>)(ParseIP("8000:f123:0:cafe::"u8))))), "2001:db8::/8000f1230000cafe0000000000000000"u8),
    new(nil, "<nil>"u8)
}.slice(); }

public static void TestIPNetString(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in ipNetStringTests) {
        {
            @string @out = tt.@in.String(); if (@out != tt.@out) {
                Ꮡt.Errorf("IPNet.String(%v) = %q, want %q"u8, tt.@in.OrTypedNil(), @out, tt.@out);
            }
        }
    }
}


partial struct cidrMaskTestsᴛ1 /*dyn*/ {
    internal nint ones;
    internal nint bits;
    internal global::go.net_package.IPMask @out;
}
internal static slice<cidrMaskTestsᴛ1> cidrMaskTests = new cidrMaskTestsᴛ1[]{
    new(0, 32, IPv4Mask(0, 0, 0, 0)),
    new(12, 32, IPv4Mask(255, 240, 0, 0)),
    new(24, 32, IPv4Mask(255, 255, 255, 0)),
    new(32, 32, IPv4Mask(255, 255, 255, 255)),
    new(0, 128, new IPMask(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice())),
    new(4, 128, new IPMask(new byte[]{0xf0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice())),
    new(48, 128, new IPMask(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice())),
    new(128, 128, new IPMask(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice())),
    new(33, 32, default!),
    new(32, 33, default!),
    new(-1, 128, default!),
    new(128, -1, default!)
}.slice();

public static void TestCIDRMask(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in cidrMaskTests) {
        {
            var @out = CIDRMask(tt.ones, tt.bits); if (!reflect.DeepEqual(@out, tt.@out)) {
                Ꮡt.Errorf("CIDRMask(%v, %v) = %v, want %v"u8, tt.ones, tt.bits, @out, tt.@out);
            }
        }
    }
}

internal static global::go.net_package.IP v4addr = new IP(new byte[]{192, 168, 0, 1}.slice());
internal static global::go.net_package.IP v4mappedv6addr = new IP(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xff, 0xff, 192, 168, 0, 1}.slice());
internal static global::go.net_package.IP v6addr = new IP(new byte[]{0x20, 0x1, 0xd, 0xb8, 0, 0, 0, 0, 0, 0, 0x1, 0x23, 0, 0x12, 0, 0x1}.slice());
internal static global::go.net_package.IPMask v4mask = new IPMask(new byte[]{255, 255, 255, 0}.slice());
internal static global::go.net_package.IPMask v4mappedv6mask = new IPMask(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 255, 255, 255, 0}.slice());
internal static global::go.net_package.IPMask v6mask = new IPMask(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0, 0, 0, 0, 0, 0, 0, 0}.slice());
internal static global::go.net_package.IP badaddr = new IP(new byte[]{192, 168, 0}.slice());
internal static global::go.net_package.IPMask badmask = new IPMask(new byte[]{255, 255, 0}.slice());
internal static global::go.net_package.IPMask v4maskzero = new IPMask(new byte[]{0, 0, 0, 0}.slice());


partial struct networkNumberAndMaskTestsᴛ1 /*dyn*/ {
    internal global::go.net_package.IPNet @in;
    internal global::go.net_package.IPNet @out;
}
internal static slice<networkNumberAndMaskTestsᴛ1> networkNumberAndMaskTests = new networkNumberAndMaskTestsᴛ1[]{
    new(new IPNet(IP: v4addr, Mask: v4mask), new IPNet(IP: v4addr, Mask: v4mask)),
    new(new IPNet(IP: v4addr, Mask: v4mappedv6mask), new IPNet(IP: v4addr, Mask: v4mask)),
    new(new IPNet(IP: v4mappedv6addr, Mask: v4mappedv6mask), new IPNet(IP: v4addr, Mask: v4mask)),
    new(new IPNet(IP: v4mappedv6addr, Mask: v6mask), new IPNet(IP: v4addr, Mask: v4maskzero)),
    new(new IPNet(IP: v4addr, Mask: v6mask), new IPNet(IP: v4addr, Mask: v4maskzero)),
    new(new IPNet(IP: v6addr, Mask: v6mask), new IPNet(IP: v6addr, Mask: v6mask)),
    new(new IPNet(IP: v6addr, Mask: v4mappedv6mask), new IPNet(IP: v6addr, Mask: v4mappedv6mask)),
    new(@in: new IPNet(IP: v6addr, Mask: v4mask)),
    new(@in: new IPNet(IP: v4addr, Mask: badmask)),
    new(@in: new IPNet(IP: v4mappedv6addr, Mask: badmask)),
    new(@in: new IPNet(IP: v6addr, Mask: badmask)),
    new(@in: new IPNet(IP: badaddr, Mask: v4mask)),
    new(@in: new IPNet(IP: badaddr, Mask: v4mappedv6mask)),
    new(@in: new IPNet(IP: badaddr, Mask: v6mask)),
    new(@in: new IPNet(IP: badaddr, Mask: badmask))
}.slice();

public static void TestNetworkNumberAndMask(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in networkNumberAndMaskTests) {
        ref var tt = ref heap(new networkNumberAndMaskTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var (ip, m) = networkNumberAndMask(ref tt.@in);
        var @out = Ꮡ(new IPNet(IP: ip, Mask: m));
        if (!reflect.DeepEqual(Ꮡtt.of(networkNumberAndMaskTestsᴛ1.Ꮡout), @out.OrTypedNil())) {
            Ꮡt.Errorf("networkNumberAndMask(%v) = %v, want %v"u8, tt.@in, @out.OrTypedNil(), Ꮡtt.of(networkNumberAndMaskTestsᴛ1.Ꮡout));
        }
    }
}

internal partial struct TestSplitHostPort_type /*dyn*/ {
    internal @string hostPort;
    internal @string host;
    internal @string port;
}

internal partial struct TestSplitHostPort_typeᴛ1 /*dyn*/ {
    internal @string hostPort;
    internal @string err;
}

public static void TestSplitHostPort(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in new TestSplitHostPort_type[]{ // Host name

        new("localhost:http"u8, "localhost"u8, "http"u8),
        new("localhost:80"u8, "localhost"u8, "80"u8), // Go-specific host name with zone identifier

        new("localhost%lo0:http"u8, "localhost%lo0"u8, "http"u8),
        new("localhost%lo0:80"u8, "localhost%lo0"u8, "80"u8),
        new("[localhost%lo0]:http"u8, "localhost%lo0"u8, "http"u8), // Go 1 behavior

        new("[localhost%lo0]:80"u8, "localhost%lo0"u8, "80"u8), // Go 1 behavior
 // IP literal

        new("127.0.0.1:http"u8, "127.0.0.1"u8, "http"u8),
        new("127.0.0.1:80"u8, "127.0.0.1"u8, "80"u8),
        new("[::1]:http"u8, "::1"u8, "http"u8),
        new("[::1]:80"u8, "::1"u8, "80"u8), // IP literal with zone identifier

        new("[::1%lo0]:http"u8, "::1%lo0"u8, "http"u8),
        new("[::1%lo0]:80"u8, "::1%lo0"u8, "80"u8), // Go-specific wildcard for host name

        new(":http"u8, ""u8, "http"u8), // Go 1 behavior

        new(":80"u8, ""u8, "80"u8), // Go 1 behavior
 // Go-specific wildcard for service name or transport port number

        new("golang.org:"u8, "golang.org"u8, ""u8), // Go 1 behavior

        new("127.0.0.1:"u8, "127.0.0.1"u8, ""u8), // Go 1 behavior

        new("[::1]:"u8, "::1"u8, ""u8), // Go 1 behavior
 // Opaque service name

        new("golang.org:https%foo"u8, "golang.org"u8, "https%foo"u8)
    }.slice()) {
        // Go 1 behavior
        {
            var (host, port, err) = SplitHostPort(tt.hostPort); if (host != tt.host || port != tt.port || err != default!) {
                Ꮡt.Errorf("SplitHostPort(%q) = %q, %q, %v; want %q, %q, nil"u8, tt.hostPort, host, port, err, tt.host, tt.port);
            }
        }
    }
    foreach (var (_, tt) in new TestSplitHostPort_typeᴛ1[]{
        new("golang.org"u8, "missing port in address"u8),
        new("127.0.0.1"u8, "missing port in address"u8),
        new("[::1]"u8, "missing port in address"u8),
        new("[fe80::1%lo0]"u8, "missing port in address"u8),
        new("[localhost%lo0]"u8, "missing port in address"u8),
        new("localhost%lo0"u8, "missing port in address"u8),
        new("::1"u8, "too many colons in address"u8),
        new("fe80::1%lo0"u8, "too many colons in address"u8),
        new("fe80::1%lo0:80"u8, "too many colons in address"u8), // Test cases that didn't fail in Go 1

        new("[foo:bar]"u8, "missing port in address"u8),
        new("[foo:bar]baz"u8, "missing port in address"u8),
        new("[foo]bar:baz"u8, "missing port in address"u8),
        new("[foo]:[bar]:baz"u8, "too many colons in address"u8),
        new("[foo]:[bar]baz"u8, "unexpected '[' in address"u8),
        new("foo[bar]:baz"u8, "unexpected '[' in address"u8),
        new("foo]bar:baz"u8, "unexpected ']' in address"u8)
    }.slice()) {
        {
            var (host, port, err) = SplitHostPort(tt.hostPort); if (err == default!){
                Ꮡt.Errorf("SplitHostPort(%q) should have failed"u8, tt.hostPort);
            } else {
                var e = err._<ж<global::go.net_package.AddrError>>();
                if ((~e).Err != tt.err) {
                    Ꮡt.Errorf("SplitHostPort(%q) = _, _, %q; want %q"u8, tt.hostPort, (~e).Err, tt.err);
                }
                if (host != ""u8 || port != ""u8) {
                    Ꮡt.Errorf("SplitHostPort(%q) = %q, %q, err; want %q, %q, err on failure"u8, tt.hostPort, host, port, (@string)""u8, (@string)""u8);
                }
            }
        }
    }
}

internal partial struct TestJoinHostPort_type /*dyn*/ {
    internal @string host;
    internal @string port;
    internal @string hostPort;
}

public static void TestJoinHostPort(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in new TestJoinHostPort_type[]{ // Host name

        new("localhost"u8, "http"u8, "localhost:http"u8),
        new("localhost"u8, "80"u8, "localhost:80"u8), // Go-specific host name with zone identifier

        new("localhost%lo0"u8, "http"u8, "localhost%lo0:http"u8),
        new("localhost%lo0"u8, "80"u8, "localhost%lo0:80"u8), // IP literal

        new("127.0.0.1"u8, "http"u8, "127.0.0.1:http"u8),
        new("127.0.0.1"u8, "80"u8, "127.0.0.1:80"u8),
        new("::1"u8, "http"u8, "[::1]:http"u8),
        new("::1"u8, "80"u8, "[::1]:80"u8), // IP literal with zone identifier

        new("::1%lo0"u8, "http"u8, "[::1%lo0]:http"u8),
        new("::1%lo0"u8, "80"u8, "[::1%lo0]:80"u8), // Go-specific wildcard for host name

        new(""u8, "http"u8, ":http"u8), // Go 1 behavior

        new(""u8, "80"u8, ":80"u8), // Go 1 behavior
 // Go-specific wildcard for service name or transport port number

        new("golang.org"u8, ""u8, "golang.org:"u8), // Go 1 behavior

        new("127.0.0.1"u8, ""u8, "127.0.0.1:"u8), // Go 1 behavior

        new("::1"u8, ""u8, "[::1]:"u8), // Go 1 behavior
 // Opaque service name

        new("golang.org"u8, "https%foo"u8, "golang.org:https%foo"u8)
    }.slice()) {
        // Go 1 behavior
        {
            @string hostPort = JoinHostPort(tt.host, tt.port); if (hostPort != tt.hostPort) {
                Ꮡt.Errorf("JoinHostPort(%q, %q) = %q; want %q"u8, tt.host, tt.port, hostPort, tt.hostPort);
            }
        }
    }
}


partial struct ipAddrFamilyTestsᴛ1 /*dyn*/ {
    internal global::go.net_package.IP @in;
    internal bool af4;
    internal bool af6;
}
internal static slice<ipAddrFamilyTestsᴛ1> ipAddrFamilyTests;
internal static void initᴛipAddrFamilyTests() { ipAddrFamilyTests = new ipAddrFamilyTestsᴛ1[]{
    new(IPv4bcast, true, false),
    new(IPv4allsys, true, false),
    new(IPv4allrouter, true, false),
    new(IPv4zero, true, false),
    new(IPv4(224, 0, 0, 1), true, false),
    new(IPv4(127, 0, 0, 1), true, false),
    new(IPv4(240, 0, 0, 1), true, false),
    new(IPv6unspecified, false, true),
    new(IPv6loopback, false, true),
    new(IPv6interfacelocalallnodes, false, true),
    new(IPv6linklocalallnodes, false, true),
    new(IPv6linklocalallrouters, false, true),
    new(ParseIP("ff05::a:b:c:d"u8), false, true),
    new(ParseIP("fe80::1:2:3:4"u8), false, true),
    new(ParseIP("2001:db8::123:12:1"u8), false, true)
}.slice(); }

public static void TestIPAddrFamily(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in ipAddrFamilyTests) {
        {
            var af = tt.@in.To4() != default!; if (af != tt.af4) {
                Ꮡt.Errorf("verifying IPv4 address family for %q = %v, want %v"u8, tt.@in, af, tt.af4);
            }
        }
        {
            var af = len(tt.@in) == IPv6len && tt.@in.To4() == default!; if (af != tt.af6) {
                Ꮡt.Errorf("verifying IPv6 address family for %q = %v, want %v"u8, tt.@in, af, tt.af6);
            }
        }
    }
}


partial struct ipAddrScopeTestsᴛ1 /*dyn*/ {
    internal Func<global::go.net_package.IP, bool> scope;
    internal global::go.net_package.IP @in;
    internal bool ok;
}
internal static slice<ipAddrScopeTestsᴛ1> ipAddrScopeTests;
internal static void initᴛipAddrScopeTests() { ipAddrScopeTests = new ipAddrScopeTestsᴛ1[]{
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsUnspecified)), IPv4zero, true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsUnspecified)), IPv4(127, 0, 0, 1), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsUnspecified)), IPv6unspecified, true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsUnspecified)), IPv6interfacelocalallnodes, false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsUnspecified)), default!, false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLoopback)), IPv4(127, 0, 0, 1), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLoopback)), IPv4(127, 255, 255, 254), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLoopback)), IPv4(128, 1, 2, 3), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLoopback)), IPv6loopback, true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLoopback)), IPv6linklocalallrouters, false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLoopback)), default!, false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsMulticast)), IPv4(224, 0, 0, 0), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsMulticast)), IPv4(239, 0, 0, 0), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsMulticast)), IPv4(240, 0, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsMulticast)), IPv6linklocalallnodes, true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsMulticast)), new IP(new byte[]{0xff, 0x05, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice()), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsMulticast)), new IP(new byte[]{0xfe, 0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice()), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsMulticast)), default!, false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsInterfaceLocalMulticast)), IPv4(224, 0, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsInterfaceLocalMulticast)), IPv4(0xff, 0x01, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsInterfaceLocalMulticast)), IPv6interfacelocalallnodes, true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsInterfaceLocalMulticast)), default!, false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalMulticast)), IPv4(224, 0, 0, 0), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalMulticast)), IPv4(239, 0, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalMulticast)), IPv4(0xff, 0x02, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalMulticast)), IPv6linklocalallrouters, true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalMulticast)), new IP(new byte[]{0xff, 0x05, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice()), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalMulticast)), default!, false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalUnicast)), IPv4(169, 254, 0, 0), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalUnicast)), IPv4(169, 255, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalUnicast)), IPv4(0xfe, 0x80, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalUnicast)), new IP(new byte[]{0xfe, 0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice()), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalUnicast)), new IP(new byte[]{0xfe, 0xc0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice()), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsLinkLocalUnicast)), default!, false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsGlobalUnicast)), IPv4(240, 0, 0, 0), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsGlobalUnicast)), IPv4(232, 0, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsGlobalUnicast)), IPv4(169, 254, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsGlobalUnicast)), IPv4bcast, false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsGlobalUnicast)), new IP(new byte[]{0x20, 0x1, 0xd, 0xb8, 0, 0, 0, 0, 0, 0, 0x1, 0x23, 0, 0x12, 0, 0x1}.slice()), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsGlobalUnicast)), new IP(new byte[]{0xfe, 0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice()), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsGlobalUnicast)), new IP(new byte[]{0xff, 0x05, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice()), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsGlobalUnicast)), default!, false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), default!, false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(1, 1, 1, 1), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(9, 255, 255, 255), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(10, 0, 0, 0), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(10, 255, 255, 255), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(11, 0, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(172, 15, 255, 255), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(172, 16, 0, 0), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(172, 16, 255, 255), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(172, 23, 18, 255), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(172, 31, 255, 255), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(172, 31, 0, 0), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(172, 32, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(192, 167, 255, 255), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(192, 168, 0, 0), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(192, 168, 255, 255), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), IPv4(192, 169, 0, 0), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), new IP(new byte[]{0xfb, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice()), false),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), new IP(new byte[]{0xfc, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice()), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), new IP(new byte[]{0xfc, 0xff, 0x12, 0, 0, 0, 0, 0x44, 0, 0, 0, 0, 0, 0, 0, 0}.slice()), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), new IP(new byte[]{0xfd, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice()), true),
    new(((Func<global::go.net_package.IP, bool>)(global::go.net_package.IsPrivate)), new IP(new byte[]{0xfe, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice()), false)
}.slice(); }

internal static @string name(any f) {
    return Δruntime.FuncForPC(reflect.ValueOf(f).Pointer()).Name();
}

public static void TestIPAddrScope(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in ipAddrScopeTests) {
        {
            var ok = tt.scope(tt.@in); if (ok != tt.ok) {
                Ꮡt.Errorf("%s(%q) = %v, want %v"u8, name((tt.scope).OrTypedNilFunc()), tt.@in, ok, tt.ok);
            }
        }
        var ip = tt.@in.To4();
        if (ip == default!) {
            continue;
        }
        {
            var ok = tt.scope(ip); if (ok != tt.ok) {
                Ꮡt.Errorf("%s(%q) = %v, want %v"u8, name((tt.scope).OrTypedNilFunc()), ip, ok, tt.ok);
            }
        }
    }
}

public static void BenchmarkIPEqual(ж<testing.B> Ꮡb) {
    Ꮡb.Run(iPv4ˢ, (ж<testing.B> bΔ1) => {
        benchmarkIPEqual(bΔ1, IPv4len);
    });
    Ꮡb.Run(iPv6ˢ, (ж<testing.B> bΔ2) => {
        benchmarkIPEqual(bΔ2, IPv6len);
    });
}

internal static void benchmarkIPEqual(ж<testing.B> Ꮡb, nint size) {
    ref var b = ref Ꮡb.DerefOrNull();

    var ips = new slice<global::go.net_package.IP>(1000);
    foreach (var (i, _) in ips) {
        ips[i] = new global::go.net_package.IP(size);
        rand.Read(ips[i]);
    }
    // Half of the N are equal.
    for (nint i = 0; i < b.N / 2; i++) {
        var x = ips[i % len(ips)];
        var y = ips[i % len(ips)];
        x.Equal(y);
    }
    // The other half are not equal.
    for (nint i = 0; i < b.N / 2; i++) {
        var x = ips[i % len(ips)];
        var y = ips[(i + 1) % len(ips)];
        x.Equal(y);
    }
}

} // end net_internal_test_package
