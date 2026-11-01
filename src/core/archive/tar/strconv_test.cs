// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using math = math_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using static go.archive.tar_package;

partial class tar_internal_test_package {

internal partial struct TestFitsInBase256_vectors /*dyn*/ {
    internal int64 @in;
    internal nint width;
    internal bool ok;
}

public static void TestFitsInBase256(ж<testing.T> t) {
    var vectors = slice<TestFitsInBase256_vectors>([
        new(+1, 8, true),
        new(0, 8, true),
        new(-1, 8, true),
        new(72057594037927936L, 8, false),
        new(72057594037927935L, 8, true),
        new(-72057594037927936L, 8, true),
        new(-72057594037927937L, 8, false),
        new(121654, 8, true),
        new(-9849849, 8, true),
        new(math.MaxInt64, 9, true),
        new(0, 9, true),
        new(math.MinInt64, 9, true),
        new(math.MaxInt64, 12, true),
        new(0, 12, true),
        new(math.MinInt64, 12, true)
    ]);

    foreach (var (_, v) in vectors) {
        var ok = fitsInBase256(v.width, v.@in);
        if (ok != v.ok) {
            t.Errorf("fitsInBase256(%d, %d): got %v, want %v", v.@in, v.width, ok, v.ok);
        }
    }
}

internal partial struct TestParseNumeric_vectors /*dyn*/ {
    internal @string @in;
    internal int64 want;
    internal bool ok;
}

public static void TestParseNumeric(ж<testing.T> t) {
    var vectors = slice<TestParseNumeric_vectors>([
        // Test base-256 (binary) encoded values.
        new("", 0, true),
        new(((@string)(new byte[]{0x80})), 0, true),
        new(((@string)(new byte[]{0x80, 0x00})), 0, true),
        new(((@string)(new byte[]{0x80, 0x00, 0x00})), 0, true),
        new(((@string)(new byte[]{0xbf})), (1 << 6) - 1, true),
        new(((@string)(new byte[]{0xbf, 0xff})), (1 << 14) - 1, true),
        new(((@string)(new byte[]{0xbf, 0xff, 0xff})), (1 << 22) - 1, true),
        new(((@string)(new byte[]{0xff})), -1, true),
        new(((@string)(new byte[]{0xff, 0xff})), -1, true),
        new(((@string)(new byte[]{0xff, 0xff, 0xff})), -1, true),
        new(((@string)(new byte[]{0xc0})), -1 * (1 << 6), true),
        new(((@string)(new byte[]{0xc0, 0x00})), -1 * (1 << 14), true),
        new(((@string)(new byte[]{0xc0, 0x00, 0x00})), -1 * (1 << 22), true),
        new(((@string)(new byte[]{0x87, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), 537795476381659745L, true),
        new(((@string)(new byte[]{0x80, 0x00, 0x00, 0x00, 0x07, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), 537795476381659745L, true),
        new(((@string)(new byte[]{0xf7, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), -615126028225187231L, true),
        new(((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0xf7, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), -615126028225187231L, true),
        new(((@string)(new byte[]{0x80, 0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff})), math.MaxInt64, true),
        new(((@string)(new byte[]{0x80, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})), 0, false),
        new(((@string)(new byte[]{0xff, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})), math.MinInt64, true),
        new(((@string)(new byte[]{0xff, 0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff})), 0, false),
        new(((@string)(new byte[]{0xf5, 0xec, 0xd1, 0xc7, 0x7e, 0x5f, 0x26, 0x48, 0x81, 0x9f, 0x8f, 0x9b})), 0, false),

        // Test base-8 (octal) encoded values.
        new("0000000\x00", 0, true),
        new(((@string)(new byte[]{0x20, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00})), 0, true),
        new(((@string)(new byte[]{0x20, 0x00, 0x30, 0x30, 0x30, 0x30, 0x33, 0x00})), 3, true),
        new("00000000227\x00", 151, true),
        new("032033\x00 ", 13339, true),
        new("320330\x00 ", 106712, true),
        new("0000660\x00 ", 432, true),
        new("\x00 0000660\x00 ", 432, true),
        new("0123456789abcdef", 0, false),
        new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x00, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66})), 0, false),
        new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x00, 0x38, 0x39, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66})), 342391, true),
        new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x7e, 0x5f, 0x26, 0x34, 0x31, 0x32, 0x33})), 0, false)
    ]);

    foreach (var (_, v) in vectors) {
        global::go.archive.tar_package.parser p = default;
        var got = p.parseNumeric(slice<byte>(v.@in));
        var ok = (p.err == nil);
        if (ok != v.ok) {
            if (v.ok) {
                t.Errorf("parseNumeric(%q): got parsing failure, want success", v.@in);
            } else {
                t.Errorf("parseNumeric(%q): got parsing success, want failure", v.@in);
            }
        }
        if (ok && got != v.want) {
            t.Errorf("parseNumeric(%q): got %d, want %d", v.@in, got, v.want);
        }
    }
}

internal partial struct TestFormatNumeric_vectors /*dyn*/ {
    internal int64 @in;
    internal @string want;
    internal bool ok;
}

public static void TestFormatNumeric(ж<testing.T> t) {
    var vectors = slice<TestFormatNumeric_vectors>([
        // Test base-8 (octal) encoded values.
        new(0, "0\x00", true),
        new(7, "7\x00", true),
        new(8, ((@string)(new byte[]{0x80, 0x08})), true),
        new(63, "77\x00", true),
        new(64, ((@string)(new byte[]{0x80, 0x00, 0x40})), true),
        new(0, "0000000\x00", true),
        new(83, "0000123\x00", true),
        new(2054353, "7654321\x00", true),
        new(2097151, "7777777\x00", true),
        new(2097152, ((@string)(new byte[]{0x80, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00})), true),
        new(0, "00000000000\x00", true),
        new(342391, "00001234567\x00", true),
        new(8414630097L, "76543210321\x00", true),
        new(1402433619, "12345670123\x00", true),
        new(8589934591L, "77777777777\x00", true),
        new(8589934592L, ((@string)(new byte[]{0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00})), true),
        new(math.MaxInt64, "777777777777777777777\x00", true),

        // Test base-256 (binary) encoded values.
        new(-1, ((@string)(new byte[]{0xff})), true),
        new(-1, ((@string)(new byte[]{0xff, 0xff})), true),
        new(-1, ((@string)(new byte[]{0xff, 0xff, 0xff})), true),
        new(((int64)1 << 0), "0", false),
        new((1 << 8) - 1, ((@string)(new byte[]{0x80, 0xff})), true),
        new(((int64)1 << 8), "0\x00", false),
        new((1 << 16) - 1, ((@string)(new byte[]{0x80, 0xff, 0xff})), true),
        new(((int64)1 << 16), "00\x00", false),
        new(-1 * (1 << 0), ((@string)(new byte[]{0xff})), true),
        new(-1 * (1 << 0) - 1, "0", false),
        new(-1 * (1 << 8), ((@string)(new byte[]{0xff, 0x00})), true),
        new(-1 * (1 << 8) - 1, "0\x00", false),
        new(-1 * (1 << 16), ((@string)(new byte[]{0xff, 0x00, 0x00})), true),
        new(-1 * (1 << 16) - 1, "00\x00", false),
        new(537795476381659745L, "0000000\x00", false),
        new(537795476381659745L, ((@string)(new byte[]{0x80, 0x00, 0x00, 0x00, 0x07, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), true),
        new(-615126028225187231L, "0000000\x00", false),
        new(-615126028225187231L, ((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0xf7, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), true),
        new(math.MaxInt64, "0000000\x00", false),
        new(math.MaxInt64, ((@string)(new byte[]{0x80, 0x00, 0x00, 0x00, 0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff})), true),
        new(math.MinInt64, "0000000\x00", false),
        new(math.MinInt64, ((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})), true),
        new(math.MaxInt64, ((@string)(new byte[]{0x80, 0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff})), true),
        new(math.MinInt64, ((@string)(new byte[]{0xff, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})), true)
    ]);

    foreach (var (_, v) in vectors) {
        global::go.archive.tar_package.formatter f = default;
        var got = new slice<byte>(len(v.want));
        f.formatNumeric(got, v.@in);
        var ok = (f.err == nil);
        if (ok != v.ok) {
            if (v.ok) {
                t.Errorf("formatNumeric(%d): got formatting failure, want success", v.@in);
            } else {
                t.Errorf("formatNumeric(%d): got formatting success, want failure", v.@in);
            }
        }
        if (((sstring)got) != v.want) {
            t.Errorf("formatNumeric(%d): got %q, want %q", v.@in, got, v.want);
        }
    }
}

internal partial struct TestFitsInOctal_vectors /*dyn*/ {
    internal int64 input;
    internal nint width;
    internal bool ok;
}

public static void TestFitsInOctal(ж<testing.T> t) {
    var vectors = slice<TestFitsInOctal_vectors>([
        new(-1, 1, false),
        new(-1, 2, false),
        new(-1, 3, false),
        new(0, 1, true),
        new(0 + 1, 1, false),
        new(0, 2, true),
        new(7, 2, true),
        new(7 + 1, 2, false),
        new(0, 4, true),
        new(511, 4, true),
        new(511 + 1, 4, false),
        new(0, 8, true),
        new(2097151, 8, true),
        new(2097151 + 1, 8, false),
        new(0, 12, true),
        new(8589934591L, 12, true),
        new(8589934592L, 12, false),
        new(math.MaxInt64, 22, true),
        new(1402433619, 12, true),
        new(452724, 12, true),
        new(-1402433619, 12, false),
        new(-452724, 12, false),
        new(-1564164, 30, false)
    ]);

    foreach (var (_, v) in vectors) {
        var ok = fitsInOctal(v.width, v.input);
        if (ok != v.ok) {
            t.Errorf("checkOctal(%d, %d): got %v, want %v", v.input, v.width, ok, v.ok);
        }
    }
}

internal partial struct TestParsePAXTime_vectors /*dyn*/ {
    internal @string @in;
    internal time.Time want;
    internal bool ok;
}

public static void TestParsePAXTime(ж<testing.T> t) {
    var vectors = slice<TestParsePAXTime_vectors>([
        new("1350244992.023960108", time.Unix(1350244992, 23960108), true),
        new("1350244992.02396010", time.Unix(1350244992, 23960100), true),
        new("1350244992.0239601089", time.Unix(1350244992, 23960108), true),
        new("1350244992.3", time.Unix(1350244992, 300000000), true),
        new("1350244992", time.Unix(1350244992, 0), true),
        new("-1.000000001", time.Unix(-1, -1 + 0), true),
        new("-1.000001", time.Unix(-1, -1000 + 0), true),
        new("-1.001000", time.Unix(-1, -1000000 + 0), true),
        new("-1", time.Unix(-1, -0 + 0), true),
        new("-1.999000", time.Unix(-1, -1000000000 + 1000000), true),
        new("-1.999999", time.Unix(-1, -1000000000 + 1000), true),
        new("-1.999999999", time.Unix(-1, -1000000000 + 1), true),
        new("0.000000001", time.Unix(0, 1 + 0), true),
        new("0.000001", time.Unix(0, 1000 + 0), true),
        new("0.001000", time.Unix(0, 1000000 + 0), true),
        new("0", time.Unix(0, 0), true),
        new("0.999000", time.Unix(0, 1000000000 - 1000000), true),
        new("0.999999", time.Unix(0, 1000000000 - 1000), true),
        new("0.999999999", time.Unix(0, 1000000000 - 1), true),
        new("1.000000001", time.Unix(+1, +1 - 0), true),
        new("1.000001", time.Unix(+1, +1000 - 0), true),
        new("1.001000", time.Unix(+1, +1000000 - 0), true),
        new("1", time.Unix(+1, +0 - 0), true),
        new("1.999000", time.Unix(+1, +1000000000 - 1000000), true),
        new("1.999999", time.Unix(+1, +1000000000 - 1000), true),
        new("1.999999999", time.Unix(+1, +1000000000 - 1), true),
        new("-1350244992.023960108", time.Unix(-1350244992, -23960108), true),
        new("-1350244992.02396010", time.Unix(-1350244992, -23960100), true),
        new("-1350244992.0239601089", time.Unix(-1350244992, -23960108), true),
        new("-1350244992.3", time.Unix(-1350244992, -300000000), true),
        new("-1350244992", time.Unix(-1350244992, 0), true),
        new("", new time.Time(), false),
        new("0", time.Unix(0, 0), true),
        new("1.", time.Unix(1, 0), true),
        new("0.0", time.Unix(0, 0), true),
        new(".5", new time.Time(), false),
        new("-1.3", time.Unix(-1, -300000000), true),
        new("-1.0", time.Unix(-1, 0), true),
        new("-0.0", time.Unix(-0, 0), true),
        new("-0.1", time.Unix(-0, -100000000), true),
        new("-0.01", time.Unix(-0, -10000000), true),
        new("-0.99", time.Unix(-0, -990000000), true),
        new("-0.98", time.Unix(-0, -980000000), true),
        new("-1.1", time.Unix(-1, -100000000), true),
        new("-1.01", time.Unix(-1, -10000000), true),
        new("-2.99", time.Unix(-2, -990000000), true),
        new("-5.98", time.Unix(-5, -980000000), true),
        new("-", new time.Time(), false),
        new("+", new time.Time(), false),
        new("-1.-1", new time.Time(), false),
        new("99999999999999999999999999999999999999999999999", new time.Time(), false),
        new("0.123456789abcdef", new time.Time(), false),
        new("foo", new time.Time(), false),
        new("\x00", new time.Time(), false),
        new("𝟵𝟴𝟳𝟲𝟱.𝟰𝟯𝟮𝟭𝟬", new time.Time(), false), // Unicode numbers (U+1D7EC to U+1D7F5)
        new("98765﹒43210", new time.Time(), false)  // Unicode period (U+FE52)
    ]);

    foreach (var (_, v) in vectors) {
        var (ts, err) = parsePAXTime(v.@in);
        var ok = (err == nil);
        if (v.ok != ok) {
            if (v.ok) {
                t.Errorf("parsePAXTime(%q): got parsing failure, want success", v.@in);
            } else {
                t.Errorf("parsePAXTime(%q): got parsing success, want failure", v.@in);
            }
        }
        if (ok && !ts.Equal(v.want)) {
            t.Errorf("parsePAXTime(%q): got (%ds %dns), want (%ds %dns)",
                v.@in, ts.Unix(), ts.Nanosecond(), v.want.Unix(), v.want.Nanosecond());
        }
    }
}

internal partial struct TestFormatPAXTime_vectors /*dyn*/ {
    internal int64 sec, nsec;
    internal @string want;
}

public static void TestFormatPAXTime(ж<testing.T> t) {
    var vectors = slice<TestFormatPAXTime_vectors>([
        new(1350244992, 0, "1350244992"),
        new(1350244992, 300000000, "1350244992.3"),
        new(1350244992, 23960100, "1350244992.0239601"),
        new(1350244992, 23960108, "1350244992.023960108"),
        new(+1, +1000000000 - 1, "1.999999999"),
        new(+1, +1000000000 - 1000, "1.999999"),
        new(+1, +1000000000 - 1000000, "1.999"),
        new(+1, +0 - 0, "1"),
        new(+1, +1000000 - 0, "1.001"),
        new(+1, +1000 - 0, "1.000001"),
        new(+1, +1 - 0, "1.000000001"),
        new(0, 1000000000 - 1, "0.999999999"),
        new(0, 1000000000 - 1000, "0.999999"),
        new(0, 1000000000 - 1000000, "0.999"),
        new(0, 0, "0"),
        new(0, 1000000 + 0, "0.001"),
        new(0, 1000 + 0, "0.000001"),
        new(0, 1 + 0, "0.000000001"),
        new(-1, -1000000000 + 1, "-1.999999999"),
        new(-1, -1000000000 + 1000, "-1.999999"),
        new(-1, -1000000000 + 1000000, "-1.999"),
        new(-1, -0 + 0, "-1"),
        new(-1, -1000000 + 0, "-1.001"),
        new(-1, -1000 + 0, "-1.000001"),
        new(-1, -1 + 0, "-1.000000001"),
        new(-1350244992, 0, "-1350244992"),
        new(-1350244992, -300000000, "-1350244992.3"),
        new(-1350244992, -23960100, "-1350244992.0239601"),
        new(-1350244992, -23960108, "-1350244992.023960108")
    ]);

    foreach (var (_, v) in vectors) {
        @string got = formatPAXTime(time.Unix(v.sec, v.nsec));
        if (got != v.want) {
            t.Errorf("formatPAXTime(%ds, %dns): got %q, want %q",
                v.sec, v.nsec, got, v.want);
        }
    }
}

internal partial struct TestParsePAXRecord_vectors /*dyn*/ {
    internal @string @in;
    internal @string wantRes;
    internal @string wantKey;
    internal @string wantVal;
    internal bool ok;
}

public static void TestParsePAXRecord(ж<testing.T> t) {
    @string medName = strings.Repeat("CD", 50);
    @string longName = strings.Repeat("AB", 100);

    var vectors = slice<TestParsePAXRecord_vectors>([
        new("6 k=v\n\n", "\n", "k", "v", true),
        new("19 path=/etc/hosts\n", "", "path", "/etc/hosts", true),
        new("210 path="u8 + longName + "\nabc"u8, "abc", "path", longName, true),
        new("110 path="u8 + medName + "\n"u8, "", "path", medName, true),
        new("9 foo=ba\n", "", "foo", "ba", true),
        new("11 foo=bar\n\x00", "\x00", "foo", "bar", true),
        new("18 foo=b=\nar=\n==\x00\n", "", "foo", "b=\nar=\n==\x00", true),
        new("27 foo=hello9 foo=ba\nworld\n", "", "foo", "hello9 foo=ba\nworld", true),
        new("27 ☺☻☹=日a本b語ç\nmeow mix", "meow mix", "☺☻☹", "日a本b語ç", true),
        new("17 \x00hello=\x00world\n", "17 \x00hello=\x00world\n", "", "", false),
        new("1 k=1\n", "1 k=1\n", "", "", false),
        new("6 k~1\n", "6 k~1\n", "", "", false),
        new("6_k=1\n", "6_k=1\n", "", "", false),
        new("6 k=1 ", "6 k=1 ", "", "", false),
        new("632 k=1\n", "632 k=1\n", "", "", false),
        new("16 longkeyname=hahaha\n", "16 longkeyname=hahaha\n", "", "", false),
        new("3 somelongkey=\n", "3 somelongkey=\n", "", "", false),
        new("50 tooshort=\n", "50 tooshort=\n", "", "", false),
        new("0000000000000000000000000000000030 mtime=1432668921.098285006\n30 ctime=2147483649.15163319", "0000000000000000000000000000000030 mtime=1432668921.098285006\n30 ctime=2147483649.15163319", "mtime", "1432668921.098285006", false),
        new("06 k=v\n", "06 k=v\n", "", "", false),
        new("00006 k=v\n", "00006 k=v\n", "", "", false),
        new("000006 k=v\n", "000006 k=v\n", "", "", false),
        new("000000 k=v\n", "000000 k=v\n", "", "", false),
        new("0 k=v\n", "0 k=v\n", "", "", false),
        new("+0000005 x=\n", "+0000005 x=\n", "", "", false)
    ]);

    foreach (var (_, v) in vectors) {
        var (key, val, res, err) = parsePAXRecord(v.@in);
        var ok = (err == nil);
        if (ok != v.ok) {
            if (v.ok) {
                t.Errorf("parsePAXRecord(%q): got parsing failure, want success", v.@in);
            } else {
                t.Errorf("parsePAXRecord(%q): got parsing success, want failure", v.@in);
            }
        }
        if (v.ok && (key != v.wantKey || val != v.wantVal)) {
            t.Errorf("parsePAXRecord(%q): got (%q: %q), want (%q: %q)",
                v.@in, key, val, v.wantKey, v.wantVal);
        }
        if (res != v.wantRes) {
            t.Errorf("parsePAXRecord(%q): got residual %q, want residual %q",
                v.@in, res, v.wantRes);
        }
    }
}

internal partial struct TestFormatPAXRecord_vectors /*dyn*/ {
    internal @string inKey;
    internal @string inVal;
    internal @string want;
    internal bool ok;
}

public static void TestFormatPAXRecord(ж<testing.T> t) {
    @string medName = strings.Repeat("CD", 50);
    @string longName = strings.Repeat("AB", 100);

    var vectors = slice<TestFormatPAXRecord_vectors>([
        new("k", "v", "6 k=v\n", true),
        new("path", "/etc/hosts", "19 path=/etc/hosts\n", true),
        new("path", longName, "210 path="u8 + longName + "\n"u8, true),
        new("path", medName, "110 path="u8 + medName + "\n"u8, true),
        new("foo", "ba", "9 foo=ba\n", true),
        new("foo", "bar", "11 foo=bar\n", true),
        new("foo", "b=\nar=\n==\x00", "18 foo=b=\nar=\n==\x00\n", true),
        new("foo", "hello9 foo=ba\nworld", "27 foo=hello9 foo=ba\nworld\n", true),
        new("☺☻☹", "日a本b語ç", "27 ☺☻☹=日a本b語ç\n", true),
        new("xhello", "\x00world", "17 xhello=\x00world\n", true),
        new("path", "null\x00", "", false),
        new("null\x00", "value", "", false),
        new(paxSchilyXattr + "key", "null\x00", "26 SCHILY.xattr.key=null\x00\n", true)
    ]);

    foreach (var (_, v) in vectors) {
        var (got, err) = formatPAXRecord(v.inKey, v.inVal);
        var ok = (err == nil);
        if (ok != v.ok) {
            if (v.ok) {
                t.Errorf("formatPAXRecord(%q, %q): got format failure, want success", v.inKey, v.inVal);
            } else {
                t.Errorf("formatPAXRecord(%q, %q): got format success, want failure", v.inKey, v.inVal);
            }
        }
        if (got != v.want) {
            t.Errorf("formatPAXRecord(%q, %q): got %q, want %q",
                v.inKey, v.inVal, got, v.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;

internal partial struct BenchmarkParsePAXTime_tests /*dyn*/ {
    internal @string name;
    internal @string @in;
    internal time.Time want;
    internal bool ok;
}

public static void BenchmarkParsePAXTime(ж<testing.B> b) {
    var tests = slice<BenchmarkParsePAXTime_tests>([
        new(
            name: "NoNanos",
            @in: "123456",
            want: time.Unix(123456, 0),
            ok: true
        ),
        new(
            name: "ExactNanos",
            @in: "1.123456789",
            want: time.Unix(1, 123456789),
            ok: true
        ),
        new(
            name: "WithNanoPadding",
            @in: "1.123",
            want: time.Unix(1, 123000000),
            ok: true
        ),
        new(
            name: "WithNanoTruncate",
            @in: "1.123456789123",
            want: time.Unix(1, 123456789),
            ok: true
        ),
        new(
            name: "TrailingError",
            @in: "1.123abc",
            want: new time.Time(),
            ok: false
        ),
        new(
            name: "LeadingError",
            @in: "1.abc123",
            want: new time.Time(),
            ok: false
        )
    ]);
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new BenchmarkParsePAXTime_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        b.Run(tt.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            while (bΔ1.Loop()) {
                var (ts, err) = parsePAXTime(ttʗ1.@in);
                if ((err == nil) != ttʗ1.ok) {
                    if (err != nil) {
                        bΔ1.Fatal(err);
                    }
                    bΔ1.Fatal(expectedErrorˢ);
                }
                if (!ts.Equal(ttʗ1.want)) {
                    bΔ1.Fatalf("time mismatch: got %v, want %v", ts, ttʗ1.want);
                }
            }
        });
    }
}

} // end tar_internal_test_package
