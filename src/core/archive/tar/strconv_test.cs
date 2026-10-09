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

public static void TestFitsInBase256(ж<testing.T> Ꮡt) {
    var vectors = new TestFitsInBase256_vectors[]{
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
    }.slice();
    foreach (var (_, v) in vectors) {
        var ok = fitsInBase256(v.width, v.@in);
        if (ok != v.ok) {
            Ꮡt.Errorf("fitsInBase256(%d, %d): got %v, want %v"u8, v.@in, v.width, ok, v.ok);
        }
    }
}

internal partial struct TestParseNumeric_vectors /*dyn*/ {
    internal @string @in;
    internal int64 want;
    internal bool ok;
}

public static void TestParseNumeric(ж<testing.T> Ꮡt) {
    var vectors = new TestParseNumeric_vectors[]{ // Test base-256 (binary) encoded values.

        new(""u8, 0, true),
        new(((@string)(new byte[]{0x80})), 0, true),
        new(((@string)(new byte[]{0x80, 0x00})), 0, true),
        new(((@string)(new byte[]{0x80, 0x00, 0x00})), 0, true),
        new(((@string)(new byte[]{0xbf})), ((1 << (int)(6))) - 1, true),
        new(((@string)(new byte[]{0xbf, 0xff})), ((1 << (int)(14))) - 1, true),
        new(((@string)(new byte[]{0xbf, 0xff, 0xff})), ((1 << (int)(22))) - 1, true),
        new(((@string)(new byte[]{0xff})), -1, true),
        new(((@string)(new byte[]{0xff, 0xff})), -1, true),
        new(((@string)(new byte[]{0xff, 0xff, 0xff})), -1, true),
        new(((@string)(new byte[]{0xc0})), -1 * ((1 << (int)(6))), true),
        new(((@string)(new byte[]{0xc0, 0x00})), -1 * ((1 << (int)(14))), true),
        new(((@string)(new byte[]{0xc0, 0x00, 0x00})), -1 * ((1 << (int)(22))), true),
        new(((@string)(new byte[]{0x87, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), 537795476381659745L, true),
        new(((@string)(new byte[]{0x80, 0x00, 0x00, 0x00, 0x07, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), 537795476381659745L, true),
        new(((@string)(new byte[]{0xf7, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), -615126028225187231L, true),
        new(((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0xf7, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), -615126028225187231L, true),
        new(((@string)(new byte[]{0x80, 0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff})), math.MaxInt64, true),
        new(((@string)(new byte[]{0x80, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})), 0, false),
        new(((@string)(new byte[]{0xff, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})), math.MinInt64, true),
        new(((@string)(new byte[]{0xff, 0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff})), 0, false),
        new(((@string)(new byte[]{0xf5, 0xec, 0xd1, 0xc7, 0x7e, 0x5f, 0x26, 0x48, 0x81, 0x9f, 0x8f, 0x9b})), 0, false), // Test base-8 (octal) encoded values.

        new("0000000\x00"u8, 0, true),
        new(((@string)(new byte[]{0x20, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00})), 0, true),
        new(((@string)(new byte[]{0x20, 0x00, 0x30, 0x30, 0x30, 0x30, 0x33, 0x00})), 3, true),
        new("00000000227\x00"u8, 151, true),
        new("032033\x00 "u8, 13339, true),
        new("320330\x00 "u8, 106712, true),
        new("0000660\x00 "u8, 432, true),
        new("\x00 0000660\x00 "u8, 432, true),
        new("0123456789abcdef"u8, 0, false),
        new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x00, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66})), 0, false),
        new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x00, 0x38, 0x39, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66})), 342391, true),
        new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x7e, 0x5f, 0x26, 0x34, 0x31, 0x32, 0x33})), 0, false)
    }.slice();
    foreach (var (_, v) in vectors) {
        global::go.archive.tar_package.parser p = default!;
        var got = p.parseNumeric(slice<byte>(v.@in));
        var ok = (p.err == default!);
        if (ok != v.ok) {
            if (v.ok){
                Ꮡt.Errorf("parseNumeric(%q): got parsing failure, want success"u8, v.@in);
            } else {
                Ꮡt.Errorf("parseNumeric(%q): got parsing success, want failure"u8, v.@in);
            }
        }
        if (ok && got != v.want) {
            Ꮡt.Errorf("parseNumeric(%q): got %d, want %d"u8, v.@in, got, v.want);
        }
    }
}

internal partial struct TestFormatNumeric_vectors /*dyn*/ {
    internal int64 @in;
    internal @string want;
    internal bool ok;
}

public static void TestFormatNumeric(ж<testing.T> Ꮡt) {
    var vectors = new TestFormatNumeric_vectors[]{ // Test base-8 (octal) encoded values.

        new(0, "0\x00"u8, true),
        new(7, "7\x00"u8, true),
        new(8, ((@string)(new byte[]{0x80, 0x08})), true),
        new(63, "77\x00"u8, true),
        new(64, ((@string)(new byte[]{0x80, 0x00, 0x40})), true),
        new(0, "0000000\x00"u8, true),
        new(83, "0000123\x00"u8, true),
        new(2054353, "7654321\x00"u8, true),
        new(2097151, "7777777\x00"u8, true),
        new(2097152, ((@string)(new byte[]{0x80, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00})), true),
        new(0, "00000000000\x00"u8, true),
        new(342391, "00001234567\x00"u8, true),
        new(8414630097L, "76543210321\x00"u8, true),
        new(1402433619, "12345670123\x00"u8, true),
        new(8589934591L, "77777777777\x00"u8, true),
        new(8589934592L, ((@string)(new byte[]{0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00})), true),
        new(math.MaxInt64, "777777777777777777777\x00"u8, true), // Test base-256 (binary) encoded values.

        new(-1, ((@string)(new byte[]{0xff})), true),
        new(-1, ((@string)(new byte[]{0xff, 0xff})), true),
        new(-1, ((@string)(new byte[]{0xff, 0xff, 0xff})), true),
        new((((int64)1 << (int)(0))), "0"u8, false),
        new(((1 << (int)(8))) - 1, ((@string)(new byte[]{0x80, 0xff})), true),
        new((((int64)1 << (int)(8))), "0\x00"u8, false),
        new(((1 << (int)(16))) - 1, ((@string)(new byte[]{0x80, 0xff, 0xff})), true),
        new((((int64)1 << (int)(16))), "00\x00"u8, false),
        new(-1 * ((1 << (int)(0))), ((@string)(new byte[]{0xff})), true),
        new(-1 * ((1 << (int)(0))) - 1, "0"u8, false),
        new(-1 * ((1 << (int)(8))), ((@string)(new byte[]{0xff, 0x00})), true),
        new(-1 * ((1 << (int)(8))) - 1, "0\x00"u8, false),
        new(-1 * ((1 << (int)(16))), ((@string)(new byte[]{0xff, 0x00, 0x00})), true),
        new(-1 * ((1 << (int)(16))) - 1, "00\x00"u8, false),
        new(537795476381659745L, "0000000\x00"u8, false),
        new(537795476381659745L, ((@string)(new byte[]{0x80, 0x00, 0x00, 0x00, 0x07, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), true),
        new(-615126028225187231L, "0000000\x00"u8, false),
        new(-615126028225187231L, ((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0xf7, 0x76, 0xa2, 0x22, 0xeb, 0x8a, 0x72, 0x61})), true),
        new(math.MaxInt64, "0000000\x00"u8, false),
        new(math.MaxInt64, ((@string)(new byte[]{0x80, 0x00, 0x00, 0x00, 0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff})), true),
        new(math.MinInt64, "0000000\x00"u8, false),
        new(math.MinInt64, ((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})), true),
        new(math.MaxInt64, ((@string)(new byte[]{0x80, 0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff})), true),
        new(math.MinInt64, ((@string)(new byte[]{0xff, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})), true)
    }.slice();
    foreach (var (_, v) in vectors) {
        global::go.archive.tar_package.formatter f = default!;
        var got = new slice<byte>(len(v.want));
        f.formatNumeric(got, v.@in);
        var ok = (f.err == default!);
        if (ok != v.ok) {
            if (v.ok){
                Ꮡt.Errorf("formatNumeric(%d): got formatting failure, want success"u8, v.@in);
            } else {
                Ꮡt.Errorf("formatNumeric(%d): got formatting success, want failure"u8, v.@in);
            }
        }
        if (((sstring)got) != v.want) {
            Ꮡt.Errorf("formatNumeric(%d): got %q, want %q"u8, v.@in, got, v.want);
        }
    }
}

internal partial struct TestFitsInOctal_vectors /*dyn*/ {
    internal int64 input;
    internal nint width;
    internal bool ok;
}

public static void TestFitsInOctal(ж<testing.T> Ꮡt) {
    var vectors = new TestFitsInOctal_vectors[]{
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
    }.slice();
    foreach (var (_, v) in vectors) {
        var ok = fitsInOctal(v.width, v.input);
        if (ok != v.ok) {
            Ꮡt.Errorf("checkOctal(%d, %d): got %v, want %v"u8, v.input, v.width, ok, v.ok);
        }
    }
}

internal partial struct TestParsePAXTime_vectors /*dyn*/ {
    internal @string @in;
    internal time.Time want;
    internal bool ok;
}

public static void TestParsePAXTime(ж<testing.T> Ꮡt) {
    var vectors = new TestParsePAXTime_vectors[]{
        new("1350244992.023960108"u8, time.Unix(1350244992, 23960108), true),
        new("1350244992.02396010"u8, time.Unix(1350244992, 23960100), true),
        new("1350244992.0239601089"u8, time.Unix(1350244992, 23960108), true),
        new("1350244992.3"u8, time.Unix(1350244992, 300000000), true),
        new("1350244992"u8, time.Unix(1350244992, 0), true),
        new("-1.000000001"u8, time.Unix(-1, -1 + 0), true),
        new("-1.000001"u8, time.Unix(-1, -1000 + 0), true),
        new("-1.001000"u8, time.Unix(-1, -1000000 + 0), true),
        new("-1"u8, time.Unix(-1, -0 + 0), true),
        new("-1.999000"u8, time.Unix(-1, -1000000000 + 1000000), true),
        new("-1.999999"u8, time.Unix(-1, -1000000000 + 1000), true),
        new("-1.999999999"u8, time.Unix(-1, -1000000000 + 1), true),
        new("0.000000001"u8, time.Unix(0, 1 + 0), true),
        new("0.000001"u8, time.Unix(0, 1000 + 0), true),
        new("0.001000"u8, time.Unix(0, 1000000 + 0), true),
        new("0"u8, time.Unix(0, 0), true),
        new("0.999000"u8, time.Unix(0, 1000000000 - 1000000), true),
        new("0.999999"u8, time.Unix(0, 1000000000 - 1000), true),
        new("0.999999999"u8, time.Unix(0, 1000000000 - 1), true),
        new("1.000000001"u8, time.Unix(+1, +1 - 0), true),
        new("1.000001"u8, time.Unix(+1, +1000 - 0), true),
        new("1.001000"u8, time.Unix(+1, +1000000 - 0), true),
        new("1"u8, time.Unix(+1, +0 - 0), true),
        new("1.999000"u8, time.Unix(+1, +1000000000 - 1000000), true),
        new("1.999999"u8, time.Unix(+1, +1000000000 - 1000), true),
        new("1.999999999"u8, time.Unix(+1, +1000000000 - 1), true),
        new("-1350244992.023960108"u8, time.Unix(-1350244992, -23960108), true),
        new("-1350244992.02396010"u8, time.Unix(-1350244992, -23960100), true),
        new("-1350244992.0239601089"u8, time.Unix(-1350244992, -23960108), true),
        new("-1350244992.3"u8, time.Unix(-1350244992, -300000000), true),
        new("-1350244992"u8, time.Unix(-1350244992, 0), true),
        new(""u8, new time.Time(nil), false),
        new("0"u8, time.Unix(0, 0), true),
        new("1."u8, time.Unix(1, 0), true),
        new("0.0"u8, time.Unix(0, 0), true),
        new(".5"u8, new time.Time(nil), false),
        new("-1.3"u8, time.Unix(-1, -300000000), true),
        new("-1.0"u8, time.Unix(-1, 0), true),
        new("-0.0"u8, time.Unix(-0, 0), true),
        new("-0.1"u8, time.Unix(-0, -100000000), true),
        new("-0.01"u8, time.Unix(-0, -10000000), true),
        new("-0.99"u8, time.Unix(-0, -990000000), true),
        new("-0.98"u8, time.Unix(-0, -980000000), true),
        new("-1.1"u8, time.Unix(-1, -100000000), true),
        new("-1.01"u8, time.Unix(-1, -10000000), true),
        new("-2.99"u8, time.Unix(-2, -990000000), true),
        new("-5.98"u8, time.Unix(-5, -980000000), true),
        new("-"u8, new time.Time(nil), false),
        new("+"u8, new time.Time(nil), false),
        new("-1.-1"u8, new time.Time(nil), false),
        new("99999999999999999999999999999999999999999999999"u8, new time.Time(nil), false),
        new("0.123456789abcdef"u8, new time.Time(nil), false),
        new("foo"u8, new time.Time(nil), false),
        new("\x00"u8, new time.Time(nil), false),
        new("𝟵𝟴𝟳𝟲𝟱.𝟰𝟯𝟮𝟭𝟬"u8, new time.Time(nil), false), // Unicode numbers (U+1D7EC to U+1D7F5)

        new("98765﹒43210"u8, new time.Time(nil), false)
    }.slice();
    // Unicode period (U+FE52)
    foreach (var (_, v) in vectors) {
        var (ts, err) = parsePAXTime(v.@in);
        var ok = (err == default!);
        if (v.ok != ok) {
            if (v.ok){
                Ꮡt.Errorf("parsePAXTime(%q): got parsing failure, want success"u8, v.@in);
            } else {
                Ꮡt.Errorf("parsePAXTime(%q): got parsing success, want failure"u8, v.@in);
            }
        }
        if (ok && !ts.Equal(v.want)) {
            Ꮡt.Errorf("parsePAXTime(%q): got (%ds %dns), want (%ds %dns)"u8,
                v.@in, ts.Unix(), ts.Nanosecond(), v.want.Unix(), v.want.Nanosecond());
        }
    }
}

internal partial struct TestFormatPAXTime_vectors /*dyn*/ {
    internal int64 sec, nsec;
    internal @string want;
}

public static void TestFormatPAXTime(ж<testing.T> Ꮡt) {
    var vectors = new TestFormatPAXTime_vectors[]{
        new(1350244992, 0, "1350244992"u8),
        new(1350244992, 300000000, "1350244992.3"u8),
        new(1350244992, 23960100, "1350244992.0239601"u8),
        new(1350244992, 23960108, "1350244992.023960108"u8),
        new(+1, +1000000000 - 1, "1.999999999"u8),
        new(+1, +1000000000 - 1000, "1.999999"u8),
        new(+1, +1000000000 - 1000000, "1.999"u8),
        new(+1, +0 - 0, "1"u8),
        new(+1, +1000000 - 0, "1.001"u8),
        new(+1, +1000 - 0, "1.000001"u8),
        new(+1, +1 - 0, "1.000000001"u8),
        new(0, 1000000000 - 1, "0.999999999"u8),
        new(0, 1000000000 - 1000, "0.999999"u8),
        new(0, 1000000000 - 1000000, "0.999"u8),
        new(0, 0, "0"u8),
        new(0, 1000000 + 0, "0.001"u8),
        new(0, 1000 + 0, "0.000001"u8),
        new(0, 1 + 0, "0.000000001"u8),
        new(-1, -1000000000 + 1, "-1.999999999"u8),
        new(-1, -1000000000 + 1000, "-1.999999"u8),
        new(-1, -1000000000 + 1000000, "-1.999"u8),
        new(-1, -0 + 0, "-1"u8),
        new(-1, -1000000 + 0, "-1.001"u8),
        new(-1, -1000 + 0, "-1.000001"u8),
        new(-1, -1 + 0, "-1.000000001"u8),
        new(-1350244992, 0, "-1350244992"u8),
        new(-1350244992, -300000000, "-1350244992.3"u8),
        new(-1350244992, -23960100, "-1350244992.0239601"u8),
        new(-1350244992, -23960108, "-1350244992.023960108"u8)
    }.slice();
    foreach (var (_, v) in vectors) {
        @string got = formatPAXTime(time.Unix(v.sec, v.nsec));
        if (got != v.want) {
            Ꮡt.Errorf("formatPAXTime(%ds, %dns): got %q, want %q"u8,
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

public static void TestParsePAXRecord(ж<testing.T> Ꮡt) {
    @string medName = strings.Repeat("CD"u8, 50);
    @string longName = strings.Repeat("AB"u8, 100);
    var vectors = new TestParsePAXRecord_vectors[]{
        new("6 k=v\n\n"u8, "\n"u8, "k"u8, "v"u8, true),
        new("19 path=/etc/hosts\n"u8, ""u8, "path"u8, "/etc/hosts"u8, true),
        new("210 path="u8 + longName + "\nabc"u8, "abc"u8, "path"u8, longName, true),
        new("110 path="u8 + medName + "\n"u8, ""u8, "path"u8, medName, true),
        new("9 foo=ba\n"u8, ""u8, "foo"u8, "ba"u8, true),
        new("11 foo=bar\n\x00"u8, "\x00"u8, "foo"u8, "bar"u8, true),
        new("18 foo=b=\nar=\n==\x00\n"u8, ""u8, "foo"u8, "b=\nar=\n==\x00"u8, true),
        new("27 foo=hello9 foo=ba\nworld\n"u8, ""u8, "foo"u8, "hello9 foo=ba\nworld"u8, true),
        new("27 ☺☻☹=日a本b語ç\nmeow mix"u8, "meow mix"u8, "☺☻☹"u8, "日a本b語ç"u8, true),
        new("17 \x00hello=\x00world\n"u8, "17 \x00hello=\x00world\n"u8, ""u8, ""u8, false),
        new("1 k=1\n"u8, "1 k=1\n"u8, ""u8, ""u8, false),
        new("6 k~1\n"u8, "6 k~1\n"u8, ""u8, ""u8, false),
        new("6_k=1\n"u8, "6_k=1\n"u8, ""u8, ""u8, false),
        new("6 k=1 "u8, "6 k=1 "u8, ""u8, ""u8, false),
        new("632 k=1\n"u8, "632 k=1\n"u8, ""u8, ""u8, false),
        new("16 longkeyname=hahaha\n"u8, "16 longkeyname=hahaha\n"u8, ""u8, ""u8, false),
        new("3 somelongkey=\n"u8, "3 somelongkey=\n"u8, ""u8, ""u8, false),
        new("50 tooshort=\n"u8, "50 tooshort=\n"u8, ""u8, ""u8, false),
        new("0000000000000000000000000000000030 mtime=1432668921.098285006\n30 ctime=2147483649.15163319"u8, "0000000000000000000000000000000030 mtime=1432668921.098285006\n30 ctime=2147483649.15163319"u8, "mtime"u8, "1432668921.098285006"u8, false),
        new("06 k=v\n"u8, "06 k=v\n"u8, ""u8, ""u8, false),
        new("00006 k=v\n"u8, "00006 k=v\n"u8, ""u8, ""u8, false),
        new("000006 k=v\n"u8, "000006 k=v\n"u8, ""u8, ""u8, false),
        new("000000 k=v\n"u8, "000000 k=v\n"u8, ""u8, ""u8, false),
        new("0 k=v\n"u8, "0 k=v\n"u8, ""u8, ""u8, false),
        new("+0000005 x=\n"u8, "+0000005 x=\n"u8, ""u8, ""u8, false)
    }.slice();
    foreach (var (_, v) in vectors) {
        var (key, val, res, err) = parsePAXRecord(v.@in);
        var ok = (err == default!);
        if (ok != v.ok) {
            if (v.ok){
                Ꮡt.Errorf("parsePAXRecord(%q): got parsing failure, want success"u8, v.@in);
            } else {
                Ꮡt.Errorf("parsePAXRecord(%q): got parsing success, want failure"u8, v.@in);
            }
        }
        if (v.ok && (key != v.wantKey || val != v.wantVal)) {
            Ꮡt.Errorf("parsePAXRecord(%q): got (%q: %q), want (%q: %q)"u8,
                v.@in, key, val, v.wantKey, v.wantVal);
        }
        if (res != v.wantRes) {
            Ꮡt.Errorf("parsePAXRecord(%q): got residual %q, want residual %q"u8,
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

public static void TestFormatPAXRecord(ж<testing.T> Ꮡt) {
    @string medName = strings.Repeat("CD"u8, 50);
    @string longName = strings.Repeat("AB"u8, 100);
    var vectors = new TestFormatPAXRecord_vectors[]{
        new("k"u8, "v"u8, "6 k=v\n"u8, true),
        new("path"u8, "/etc/hosts"u8, "19 path=/etc/hosts\n"u8, true),
        new("path"u8, longName, "210 path="u8 + longName + "\n"u8, true),
        new("path"u8, medName, "110 path="u8 + medName + "\n"u8, true),
        new("foo"u8, "ba"u8, "9 foo=ba\n"u8, true),
        new("foo"u8, "bar"u8, "11 foo=bar\n"u8, true),
        new("foo"u8, "b=\nar=\n==\x00"u8, "18 foo=b=\nar=\n==\x00\n"u8, true),
        new("foo"u8, "hello9 foo=ba\nworld"u8, "27 foo=hello9 foo=ba\nworld\n"u8, true),
        new("☺☻☹"u8, "日a本b語ç"u8, "27 ☺☻☹=日a本b語ç\n"u8, true),
        new("xhello"u8, "\x00world"u8, "17 xhello=\x00world\n"u8, true),
        new("path"u8, "null\x00"u8, ""u8, false),
        new("null\x00"u8, "value"u8, ""u8, false),
        new(paxSchilyXattr + "key", "null\x00"u8, "26 SCHILY.xattr.key=null\x00\n"u8, true)
    }.slice();
    foreach (var (_, v) in vectors) {
        var (got, err) = formatPAXRecord(v.inKey, v.inVal);
        var ok = (err == default!);
        if (ok != v.ok) {
            if (v.ok){
                Ꮡt.Errorf("formatPAXRecord(%q, %q): got format failure, want success"u8, v.inKey, v.inVal);
            } else {
                Ꮡt.Errorf("formatPAXRecord(%q, %q): got format success, want failure"u8, v.inKey, v.inVal);
            }
        }
        if (got != v.want) {
            Ꮡt.Errorf("formatPAXRecord(%q, %q): got %q, want %q"u8,
                v.inKey, v.inVal, got, v.want);
        }
    }
}

} // end tar_internal_test_package
