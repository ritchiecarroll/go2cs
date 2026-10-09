// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bytes = bytes_package;
using encoding = encoding_package;
using json = go.encoding.json_package;
using strings = strings_package;
using testing = testing_package;
using go.encoding;

partial class netip_package {

internal static Func<@string, ΔPrefix> mustPrefix;
internal static void initᴛmustPrefix() { mustPrefix = MustParsePrefix; }
internal static Func<@string, ΔAddr> mustIP;
internal static void initᴛmustIP() { mustIP = MustParseAddr; }

internal partial struct TestPrefixValid_tests /*dyn*/ {
    internal ΔPrefix ipp;
    internal bool want;
}

public static void TestPrefixValid(ж<testing.T> Ꮡt) {
    var v4 = MustParseAddr("1.2.3.4"u8);
    var v6 = MustParseAddr("::1"u8);
    var tests = new TestPrefixValid_tests[]{
        new(PrefixFrom(v4, -2), false),
        new(PrefixFrom(v4, -1), false),
        new(PrefixFrom(v4, 0), true),
        new(PrefixFrom(v4, 32), true),
        new(PrefixFrom(v4, 33), false),
        new(PrefixFrom(v6, -2), false),
        new(PrefixFrom(v6, -1), false),
        new(PrefixFrom(v6, 0), true),
        new(PrefixFrom(v6, 32), true),
        new(PrefixFrom(v6, 128), true),
        new(PrefixFrom(v6, 129), false),
        new(PrefixFrom(new ΔAddr(nil), -2), false),
        new(PrefixFrom(new ΔAddr(nil), -1), false),
        new(PrefixFrom(new ΔAddr(nil), 0), false),
        new(PrefixFrom(new ΔAddr(nil), 32), false),
        new(PrefixFrom(new ΔAddr(nil), 128), false)
    }.slice();
    foreach (var (_, tt) in tests) {
        var got = tt.ipp.IsValid();
        if (got != tt.want) {
            Ꮡt.Errorf("(%v).IsValid() = %v want %v"u8, tt.ipp, got, tt.want);
        }
        // Test that there is only one invalid Prefix representation per Addr.
        var invalid = PrefixFrom(tt.ipp.Addr(), -1);
        if (!got && tt.ipp != invalid) {
            Ꮡt.Errorf("(%v == %v) = false, want true"u8, tt.ipp, invalid);
        }
    }
}


partial struct nextPrevTestsᴛ1 /*dyn*/ {
    internal ΔAddr ip;
    internal ΔAddr next;
    internal ΔAddr prev;
}
internal static slice<nextPrevTestsᴛ1> nextPrevTests;
internal static void initᴛnextPrevTests() { nextPrevTests = new nextPrevTestsᴛ1[]{
    new(mustIP("10.0.0.1"u8), mustIP("10.0.0.2"u8), mustIP("10.0.0.0"u8)),
    new(mustIP("10.0.0.255"u8), mustIP("10.0.1.0"u8), mustIP("10.0.0.254"u8)),
    new(mustIP("127.0.0.1"u8), mustIP("127.0.0.2"u8), mustIP("127.0.0.0"u8)),
    new(mustIP("254.255.255.255"u8), mustIP("255.0.0.0"u8), mustIP("254.255.255.254"u8)),
    new(mustIP("255.255.255.255"u8), new ΔAddr(nil), mustIP("255.255.255.254"u8)),
    new(mustIP("0.0.0.0"u8), mustIP("0.0.0.1"u8), new ΔAddr(nil)),
    new(mustIP("::"u8), mustIP("::1"u8), new ΔAddr(nil)),
    new(mustIP("::%x"u8), mustIP("::1%x"u8), new ΔAddr(nil)),
    new(mustIP("::1"u8), mustIP("::2"u8), mustIP("::"u8)),
    new(mustIP("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"u8), new ΔAddr(nil), mustIP("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe"u8))
}.slice(); }

public static void TestIPNextPrev(ж<testing.T> Ꮡt) {
    doNextPrev(new testing_TжTB(Ꮡt));
    foreach (var (_, ip) in new ΔAddr[]{
        mustIP("0.0.0.0"u8),
        mustIP("::"u8)
    }.slice()) {
        var got = ip.Prev();
        if (!got.isZero()) {
            Ꮡt.Errorf("IP(%v).Prev = %v; want zero"u8, ip, got);
        }
    }
    array<byte> allFF = new(16);
    foreach (var (i, _) in allFF) {
        allFF[i] = 0xff;
    }
    foreach (var (_, ip) in new ΔAddr[]{
        mustIP("255.255.255.255"u8),
        AddrFrom16(allFF)
    }.slice()) {
        var got = ip.Next();
        if (!got.isZero()) {
            Ꮡt.Errorf("IP(%v).Next = %v; want zero"u8, ip, got);
        }
    }
}

public static void BenchmarkIPNextPrev(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        doNextPrev(new testing_BжTB(Ꮡb));
    }
}

internal static void doNextPrev(testing.TB t) {
    foreach (var (_, tt) in nextPrevTests) {
        var (gnext, gprev) = (tt.ip.Next(), tt.ip.Prev());
        if (gnext != tt.next) {
            t.Errorf("IP(%v).Next = %v; want %v"u8, tt.ip, gnext, tt.next);
        }
        if (gprev != tt.prev) {
            t.Errorf("IP(%v).Prev = %v; want %v"u8, tt.ip, gprev, tt.prev);
        }
        if (!tt.ip.Next().isZero() && tt.ip.Next().Prev() != tt.ip) {
            t.Errorf("IP(%v).Next.Prev = %v; want %v"u8, tt.ip, tt.ip.Next().Prev(), tt.ip);
        }
        if (!tt.ip.Prev().isZero() && tt.ip.Prev().Next() != tt.ip) {
            t.Errorf("IP(%v).Prev.Next = %v; want %v"u8, tt.ip, tt.ip.Prev().Next(), tt.ip);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fed01ˢ = "fed0::1"u8;
internal static readonly @string ffff10001ˢ = "::ffff:10.0.0.1"u8;

internal partial struct TestIPBitLen_tests /*dyn*/ {
    internal ΔAddr ip;
    internal nint want;
}

public static void TestIPBitLen(ж<testing.T> Ꮡt) {
    var tests = new TestIPBitLen_tests[]{
        new(new ΔAddr(nil), 0),
        new(mustIP("0.0.0.0"u8), 32),
        new(mustIP("10.0.0.1"u8), 32),
        new(mustIP("::"u8), 128),
        new(mustIP(fed01ˢ), 128),
        new(mustIP(ffff10001ˢ), 128)
    }.slice();
    foreach (var (_, tt) in tests) {
        nint got = tt.ip.BitLen();
        if (got != tt.want) {
            Ꮡt.Errorf("BitLen(%v) = %d; want %d"u8, tt.ip, got, tt.want);
        }
    }
}

internal partial struct TestPrefixContains_tests /*dyn*/ {
    internal ΔPrefix ipp;
    internal ΔAddr ip;
    internal bool want;
}

public static void TestPrefixContains(ж<testing.T> Ꮡt) {
    var tests = new TestPrefixContains_tests[]{
        new(mustPrefix("9.8.7.6/0"u8), mustIP("9.8.7.6"u8), true),
        new(mustPrefix("9.8.7.6/16"u8), mustIP("9.8.7.6"u8), true),
        new(mustPrefix("9.8.7.6/16"u8), mustIP("9.8.6.4"u8), true),
        new(mustPrefix("9.8.7.6/16"u8), mustIP("9.9.7.6"u8), false),
        new(mustPrefix("9.8.7.6/32"u8), mustIP("9.8.7.6"u8), true),
        new(mustPrefix("9.8.7.6/32"u8), mustIP("9.8.7.7"u8), false),
        new(mustPrefix("9.8.7.6/32"u8), mustIP("9.8.7.7"u8), false),
        new(mustPrefix("::1/0"u8), mustIP("::1"u8), true),
        new(mustPrefix("::1/0"u8), mustIP("::2"u8), true),
        new(mustPrefix("::1/127"u8), mustIP("::1"u8), true),
        new(mustPrefix("::1/127"u8), mustIP("::2"u8), false),
        new(mustPrefix("::1/128"u8), mustIP("::1"u8), true),
        new(mustPrefix("::1/127"u8), mustIP("::2"u8), false), // Zones ignored: https://go.dev/issue/51899

        new(new ΔPrefix(mustIP("1.2.3.4"u8).WithZone("a"u8), 32), mustIP("1.2.3.4"u8), true),
        new(new ΔPrefix(mustIP("::1"u8).WithZone("a"u8), 128), mustIP("::1"u8), true), // invalid IP

        new(mustPrefix("::1/0"u8), new ΔAddr(nil), false),
        new(mustPrefix("1.2.3.4/0"u8), new ΔAddr(nil), false), // invalid Prefix

        new(PrefixFrom(mustIP("::1"u8), 129), mustIP("::1"u8), false),
        new(PrefixFrom(mustIP("1.2.3.4"u8), 33), mustIP("1.2.3.4"u8), false),
        new(PrefixFrom(new ΔAddr(nil), 0), mustIP("1.2.3.4"u8), false),
        new(PrefixFrom(new ΔAddr(nil), 32), mustIP("1.2.3.4"u8), false),
        new(PrefixFrom(new ΔAddr(nil), 128), mustIP("::1"u8), false), // wrong IP family

        new(mustPrefix("::1/0"u8), mustIP("1.2.3.4"u8), false),
        new(mustPrefix("1.2.3.4/0"u8), mustIP("::1"u8), false)
    }.slice();
    foreach (var (_, tt) in tests) {
        var got = tt.ipp.Contains(tt.ip);
        if (got != tt.want) {
            Ꮡt.Errorf("(%v).Contains(%v) = %v want %v"u8, tt.ipp, tt.ip, got, tt.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noErrorˢ = (@string)"no error"u8;
internal static readonly @string unableToParseIpˢ = "unable to parse IP"u8;

internal partial struct TestParseIPError_tests /*dyn*/ {
    internal @string ip;
    internal @string errstr;
}

public static void TestParseIPError(ж<testing.T> Ꮡt) {
    var tests = new TestParseIPError_tests[]{
        new(
            ip: "localhost"u8
        ),
        new(
            ip: "500.0.0.1"u8,
            errstr: "field has value >255"u8
        ),
        new(
            ip: "::gggg%eth0"u8,
            errstr: "must have at least one digit"u8
        ),
        new(
            ip: "fe80::1cc0:3e8c:119f:c2e1%"u8,
            errstr: "zone must be a non-empty string"u8
        ),
        new(
            ip: "%eth0"u8,
            errstr: "missing IPv6 address"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestParseIPError_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.ip, (ж<testing.T> tΔ1) => {
            var (_, err) = ParseAddr(testʗ1.ip);
            if (err == default!) {
                tΔ1.Fatal(noErrorˢ);
            }
            {
                var (_, ok) = err._<parseAddrError>(ᐧ); if (!ok) {
                    tΔ1.Errorf("error type is %T, want parseIPError"u8, err);
                }
            }
            if (testʗ1.errstr == ""u8) {
                testʗ1.errstr = unableToParseIpˢ;
            }
            {
                @string got = err.Error(); if (!strings.Contains(got, testʗ1.errstr)) {
                    tΔ1.Errorf("error is missing substring %q: %s"u8, testʗ1.errstr, got);
                }
            }
        });
    }
}

internal partial struct TestParseAddrPort_tests /*dyn*/ {
    internal @string @in;
    internal AddrPort want;
    internal bool wantErr;
}

public static void TestParseAddrPort(ж<testing.T> Ꮡt) {
    var tests = new TestParseAddrPort_tests[]{
        new(@in: "1.2.3.4:1234"u8, want: new AddrPort(mustIP("1.2.3.4"u8), 1234)),
        new(@in: "1.1.1.1:123456"u8, wantErr: true),
        new(@in: "1.1.1.1:-123"u8, wantErr: true),
        new(@in: "[::1]:1234"u8, want: new AddrPort(mustIP("::1"u8), 1234)),
        new(@in: "[1.2.3.4]:1234"u8, wantErr: true),
        new(@in: "fe80::1:1234"u8, wantErr: true),
        new(@in: ":0"u8, wantErr: true)
    }.slice();
    // if we need to parse this form, there should be a separate function that explicitly allows it
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestParseAddrPort_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.@in, (ж<testing.T> tΔ1) => {
            var (got, err) = ParseAddrPort(testʗ1.@in);
            if (err != default!) {
                if (testʗ1.wantErr) {
                    return;
                }
                tΔ1.Fatal(err);
            }
            if (got != testʗ1.want) {
                tΔ1.Errorf("got %v; want %v"u8, got, testʗ1.want);
            }
            if (got.String() != testʗ1.@in) {
                tΔ1.Errorf("String = %q; want %q"u8, got.String(), testʗ1.@in);
            }
        });
        var testʗ2 = test;
        Ꮡt.Run(test.@in + "/AppendTo"u8, (ж<testing.T> tΔ2) => {
            var (got, err) = ParseAddrPort(testʗ2.@in);
            if (err == default!) {
                testAppendToMarshal(tΔ2, got);
            }
        });
        // TextMarshal and TextUnmarshal mostly behave like
        // ParseAddrPort and String. Divergent behavior are handled in
        // TestAddrPortMarshalUnmarshal.
        var testʗ3 = test;
        Ꮡt.Run(test.@in + "/Marshal"u8, (ж<testing.T> tΔ3) => {
            ref var got = ref heap(new AddrPort(), out var Ꮡgot);
            @string jsin = @""""u8 + testʗ3.@in + @""""u8;
            var err = json.Unmarshal(slice<byte>(jsin), Ꮡgot);
            if (err != default!) {
                if (testʗ3.wantErr) {
                    return;
                }
                tΔ3.Fatal(err);
            }
            if (got != testʗ3.want) {
                tΔ3.Errorf("got %v; want %v"u8, got, testʗ3.want);
            }
            (var gotb, err) = json.Marshal(got);
            if (err != default!) {
                tΔ3.Fatal(err);
            }
            if (((sstring)gotb) != jsin) {
                tΔ3.Errorf("Marshal = %q; want %q"u8, ((@string)gotb), jsin);
            }
        });
    }
}

internal partial struct TestAddrPortMarshalUnmarshal_tests /*dyn*/ {
    internal @string @in;
    internal AddrPort want;
}

public static void TestAddrPortMarshalUnmarshal(ж<testing.T> Ꮡt) {
    var tests = new TestAddrPortMarshalUnmarshal_tests[]{
        new(""u8, new AddrPort(nil))
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestAddrPortMarshalUnmarshal_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.@in, (ж<testing.T> tΔ1) => {
            @string orig = @""""u8 + testʗ1.@in + @""""u8;
            ref var ipp = ref heap(new AddrPort(), out var Ꮡipp);
            {
                var errΔ1 = json.Unmarshal(slice<byte>(orig), Ꮡipp); if (errΔ1 != default!) {
                    tΔ1.Fatalf("failed to unmarshal: %v"u8, errΔ1);
                }
            }
            var (ippb, err) = json.Marshal(ipp);
            if (err != default!) {
                tΔ1.Fatalf("failed to marshal: %v"u8, err);
            }
            @string back = ((@string)ippb);
            if (orig != back) {
                tΔ1.Errorf("Marshal = %q; want %q"u8, back, orig);
            }
            testAppendToMarshal(tΔ1, ipp);
        });
    }
}

public partial interface appendMarshaler :
    encoding.TextMarshaler
{
    slice<byte> AppendTo(slice<byte> _);
}

// testAppendToMarshal tests that x's AppendTo and MarshalText methods yield the same results.
// x's MarshalText method must not return an error.
internal static void testAppendToMarshal(ж<testing.T> Ꮡt, appendMarshaler x) {
    Ꮡt.Helper();
    var (m, err) = x.MarshalText();
    if (err != default!) {
        Ꮡt.Fatalf("(%v).MarshalText: %v"u8, x, err);
    }
    var a = new slice<byte>(0, len(m));
    a = x.AppendTo(a);
    if (!bytes.Equal(m, a)) {
        Ꮡt.Errorf("(%v).MarshalText = %q, (%v).AppendTo = %q"u8, x, m, x, a);
    }
}

public static void TestIPv6Accessor(ж<testing.T> Ꮡt) {
    array<byte> a = new(16);
    foreach (var (i, _) in a) {
        a[i] = (byte)((uint8)i + 1);
    }
    var ip = AddrFrom16(a);
    foreach (var (i, _) in a) {
        {
            var (got, want) = (ip.v6((uint8)i), (uint8)((uint8)i + 1)); if (got != want) {
                Ꮡt.Errorf("v6(%v) = %v; want %v"u8, i, got, want);
            }
        }
    }
}

} // end netip_package
