// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using Δmath = math_package;
using strconv = strconv_package;
using strings = strings_package;
using Δtesting = testing_package;
using quick = go.testing.quick_package;
using static time_package;
using go.testing;
using static go.time_internal_test_package;
using Δtime = time_package;

partial class time_test_package {

internal static slice<@string> nextStdChunkTests = new @string[]{
    "(2006)-(01)-(02)T(15):(04):(05)(Z07:00)"u8,
    "(2006)-(01)-(02) (002) (15):(04):(05)"u8,
    "(2006)-(01) (002) (15):(04):(05)"u8,
    "(2006)-(002) (15):(04):(05)"u8,
    "(2006)(002)(01) (15):(04):(05)"u8,
    "(2006)(002)(04) (15):(04):(05)"u8
}.slice();

public static void TestNextStdChunk(ж<Δtesting.T> Ꮡt) {
    // Most bugs in Parse or Format boil down to problems with
    // the exact detection of format chunk boundaries in the
    // helper function nextStdChunk (here called as NextStdChunk).
    // This test checks nextStdChunk's behavior directly,
    // instead of needing to test it only indirectly through Parse/Format.
    // markChunks returns format with each detected
    // 'format chunk' parenthesized.
    // For example showChunks("2006-01-02") == "(2006)-(01)-(02)".
    @string markChunks(@string format) {
        // Note that NextStdChunk and StdChunkNames
        // are not part of time's public API.
        // They are exported in export_test for this test.
        @string @out = ""u8;
        for (@string s = format; s != ""u8; ) {
            var (prefix, std, suffix) = time_internal_test_package.NextStdChunk(s);
            @out += prefix;
            if (std > 0) {
                @out += "(" + time_internal_test_package.StdChunkNames[std] + ")";
            }
            s = suffix;
        }
        return @out;
    }
    var noParens = (rune r) => {
        if (r == (rune)'(' || r == (rune)')') {
            return -1;
        }
        return r;
    };
    foreach (var (_, marked) in nextStdChunkTests) {
        // marked is an expected output from markChunks.
        // If we delete the parens and pass it through markChunks,
        // we should get the original back.
        @string format = strings.Map(noParens, marked);
        @string @out = markChunks(format);
        if (@out != marked) {
            Ꮡt.Errorf("nextStdChunk parses %q as %q, want %q"u8, format, @out, marked);
        }
    }
}

partial struct TimeFormatTest {
    internal Δtime.Time time;
    internal @string formattedValue;
}

internal static slice<TimeFormatTest> rfc3339Formats = new TimeFormatTest[]{
    new(Date(2008, 9, 17, 20, 4, 26, 0, ΔUTC), "2008-09-17T20:04:26Z"u8),
    new(Date(1994, 9, 17, 20, 4, 26, 0, FixedZone("EST"u8, -18000)), "1994-09-17T20:04:26-05:00"u8),
    new(Date(2000, 12, 26, 1, 15, 6, 0, FixedZone("OTO"u8, 15600)), "2000-12-26T01:15:06+04:20"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rfc3339ˢ = (@string)"RFC3339:"u8;

public static void TestRFC3339Conversion(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, f) in rfc3339Formats) {
        if (f.time.Format(RFC3339) != f.formattedValue) {
            Ꮡt.Error(rfc3339ˢ);
            Ꮡt.Errorf("  want=%+v"u8, f.formattedValue);
            Ꮡt.Errorf("  have=%+v"u8, f.time.Format(RFC3339));
        }
    }
}

internal partial struct TestAppendInt_tests /*dyn*/ {
    internal nint @in;
    internal nint width;
    internal @string want;
}

public static void TestAppendInt(ж<Δtesting.T> Ꮡt) {
    var tests = new TestAppendInt_tests[]{
        new(0, 0, "0"u8),
        new(0, 1, "0"u8),
        new(0, 2, "00"u8),
        new(0, 3, "000"u8),
        new(1, 0, "1"u8),
        new(1, 1, "1"u8),
        new(1, 2, "01"u8),
        new(1, 3, "001"u8),
        new(-1, 0, "-1"u8),
        new(-1, 1, "-1"u8),
        new(-1, 2, "-01"u8),
        new(-1, 3, "-001"u8),
        new(99, 2, "99"u8),
        new(100, 2, "100"u8),
        new(1, 4, "0001"u8),
        new(12, 4, "0012"u8),
        new(123, 4, "0123"u8),
        new(1234, 4, "1234"u8),
        new(12345, 4, "12345"u8),
        new(1, 5, "00001"u8),
        new(12, 5, "00012"u8),
        new(123, 5, "00123"u8),
        new(1234, 5, "01234"u8),
        new(12345, 5, "12345"u8),
        new(123456, 5, "123456"u8),
        new(0, 9, "000000000"u8),
        new(123, 9, "000000123"u8),
        new(123456, 9, "000123456"u8),
        new(123456789, 9, "123456789"u8)
    }.slice();
    slice<byte> got = default!;
    foreach (var (_, tt) in tests) {
        got = time_internal_test_package.AppendInt(got[..0], tt.@in, tt.width);
        if (((sstring)got) != tt.want) {
            Ꮡt.Errorf("appendInt(%d, %d) = %s, want %s"u8, tt.@in, tt.width, got, tt.want);
        }
    }
}

partial struct FormatTest {
    internal @string name;
    internal @string format;
    internal @string result;
}

// Three-letter months and days must not be followed by lower-case letter.
// Time stamps, Fractional seconds.
internal static slice<FormatTest> formatTests = new FormatTest[]{
    new("ANSIC"u8, ANSIC, "Wed Feb  4 21:00:57 2009"u8),
    new("UnixDate"u8, UnixDate, "Wed Feb  4 21:00:57 PST 2009"u8),
    new("RubyDate"u8, RubyDate, "Wed Feb 04 21:00:57 -0800 2009"u8),
    new("RFC822"u8, RFC822, "04 Feb 09 21:00 PST"u8),
    new("RFC850"u8, RFC850, "Wednesday, 04-Feb-09 21:00:57 PST"u8),
    new("RFC1123"u8, RFC1123, "Wed, 04 Feb 2009 21:00:57 PST"u8),
    new("RFC1123Z"u8, RFC1123Z, "Wed, 04 Feb 2009 21:00:57 -0800"u8),
    new("RFC3339"u8, RFC3339, "2009-02-04T21:00:57-08:00"u8),
    new("RFC3339Nano"u8, RFC3339Nano, "2009-02-04T21:00:57.0123456-08:00"u8),
    new("Kitchen"u8, Kitchen, "9:00PM"u8),
    new("am/pm"u8, "3pm"u8, "9pm"u8),
    new("AM/PM"u8, "3PM"u8, "9PM"u8),
    new("two-digit year"u8, "06 01 02"u8, "09 02 04"u8),
    new("Janet"u8, "Hi Janet, the Month is January"u8, "Hi Janet, the Month is February"u8),
    new("Stamp"u8, Stamp, "Feb  4 21:00:57"u8),
    new("StampMilli"u8, StampMilli, "Feb  4 21:00:57.012"u8),
    new("StampMicro"u8, StampMicro, "Feb  4 21:00:57.012345"u8),
    new("StampNano"u8, StampNano, "Feb  4 21:00:57.012345600"u8),
    new("DateTime"u8, DateTime, "2009-02-04 21:00:57"u8),
    new("DateOnly"u8, DateOnly, "2009-02-04"u8),
    new("TimeOnly"u8, TimeOnly, "21:00:57"u8),
    new("YearDay"u8, "Jan  2 002 __2 2"u8, "Feb  4 035  35 4"u8),
    new("Year"u8, "2006 6 06 _6 __6 ___6"u8, "2009 6 09 _6 __6 ___6"u8),
    new("Month"u8, "Jan January 1 01 _1"u8, "Feb February 2 02 _2"u8),
    new("DayOfMonth"u8, "2 02 _2 __2"u8, "4 04  4  35"u8),
    new("DayOfWeek"u8, "Mon Monday"u8, "Wed Wednesday"u8),
    new("Hour"u8, "15 3 03 _3"u8, "21 9 09 _9"u8),
    new("Minute"u8, "4 04 _4"u8, "0 00 _0"u8),
    new("Second"u8, "5 05 _5"u8, "57 57 _57"u8)
}.slice();

public static void TestFormat(ж<Δtesting.T> Ꮡt) {
    // The numeric time represents Thu Feb  4 21:00:57.012345600 PST 2009
    var time = Unix(0, 1233810057012345600L);
    foreach (var (_, test) in formatTests) {
        @string result = time.Format(test.format);
        if (result != test.result) {
            Ꮡt.Errorf("%s expected %q got %q"u8, test.name, test.result, result);
        }
    }
}


partial struct goStringTestsᴛ1 /*dyn*/ {
    internal Δtime.Time @in;
    internal @string want;
}
internal static slice<goStringTestsᴛ1> goStringTests = new goStringTestsᴛ1[]{
    new(Date(2009, February, 5, 5, 0, 57, 12345600, ΔUTC),
        "time.Date(2009, time.February, 5, 5, 0, 57, 12345600, time.UTC)"u8),
    new(Date(2009, February, 5, 5, 0, 57, 12345600, ΔLocal),
        "time.Date(2009, time.February, 5, 5, 0, 57, 12345600, time.Local)"u8),
    new(Date(2009, February, 5, 5, 0, 57, 12345600, FixedZone("Europe/Berlin"u8, 3 * 60 * 60)),
        @"time.Date(2009, time.February, 5, 5, 0, 57, 12345600, time.Location(""Europe/Berlin""))"u8
    ),
    new(Date(2009, February, 5, 5, 0, 57, 12345600, FixedZone("Non-ASCII character ⏰"u8, 3 * 60 * 60)),
        @"time.Date(2009, time.February, 5, 5, 0, 57, 12345600, time.Location(""Non-ASCII character \xe2\x8f\xb0""))"u8
    )
}.slice();

public static void TestGoString(ж<Δtesting.T> Ꮡt) {
    // The numeric time represents Thu Feb  4 21:00:57.012345600 PST 2009
    foreach (var (_, tt) in goStringTests) {
        if (tt.@in.GoString() != tt.want) {
            Ꮡt.Errorf("GoString (%q): got %q want %q"u8, tt.@in, tt.@in.GoString(), tt.want);
        }
    }
}

// issue 12440.
public static void TestFormatSingleDigits(ж<Δtesting.T> Ꮡt) {
    var time = Date(2001, 2, 3, 4, 5, 6, 700000000, ΔUTC);
    var test = new FormatTest("single digit format"u8, "3:4:5"u8, "4:5:6"u8);
    @string result = time.Format(test.format);
    if (result != test.result) {
        Ꮡt.Errorf("%s expected %q got %q"u8, test.name, test.result, result);
    }
}

public static void TestFormatShortYear(ж<Δtesting.T> Ꮡt) {
    var years = new nint[]{
        -100001, -100000, -99999,
        -10001, -10000, -9999,
        -1001, -1000, -999,
        -101, -100, -99,
        -11, -10, -9,
        -1, 0, 1,
        9, 10, 11,
        99, 100, 101,
        999, 1000, 1001,
        9999, 10000, 10001,
        99999, 100000, 100001
    }.slice();
    foreach (var (_, y) in years) {
        var time = Date(y, January, 1, 0, 0, 0, 0, ΔUTC);
        @string result = time.Format("2006.01.02"u8);
        @string want = default!;
        if (y < 0){
            // The 4 in %04d counts the - sign, so print -y instead
            // and introduce our own - sign.
            want = fmt.Sprintf("-%04d.%02d.%02d"u8, -y, (nint)(1), (nint)(1));
        } else {
            want = fmt.Sprintf("%04d.%02d.%02d"u8, y, (nint)(1), (nint)(1));
        }
        if (result != want) {
            Ꮡt.Errorf("(jan 1 %d).Format(\"2006.01.02\") = %q, want %q"u8, y, result, want);
        }
    }
}

partial struct ParseTest {
    internal @string name;
    internal @string format;
    internal @string value;
    internal bool hasTZ; // contains a time zone
    internal bool hasWD; // contains a weekday
    internal nint yearSign; // sign of year, -1 indicates the year is not present in the format
    internal nint fracDigits; // number of digits of fractional second
}

// Optional fractional seconds.
// Amount of white space should not matter.
// Case should not matter
// Fractional seconds.
// Leading zeros in other places should not be taken as fractional seconds.
// Month and day names only match when not followed by a lower-case letter.
// GMT with offset.
// Accept any number of fractional second digits (including none) for .999...
// In Go 1, .999... was completely ignored in the format, meaning the first two
// cases would succeed, but the next four would not. Go 1.1 accepts all six.
// decimal "." separator.
// comma "," separator.
// issue 4502.
// Day of year.
// Time zone offsets
internal static slice<ParseTest> parseTests = new ParseTest[]{
    new("ANSIC"u8, ANSIC, "Thu Feb  4 21:00:57 2010"u8, false, true, 1, 0),
    new("UnixDate"u8, UnixDate, "Thu Feb  4 21:00:57 PST 2010"u8, true, true, 1, 0),
    new("RubyDate"u8, RubyDate, "Thu Feb 04 21:00:57 -0800 2010"u8, true, true, 1, 0),
    new("RFC850"u8, RFC850, "Thursday, 04-Feb-10 21:00:57 PST"u8, true, true, 1, 0),
    new("RFC1123"u8, RFC1123, "Thu, 04 Feb 2010 21:00:57 PST"u8, true, true, 1, 0),
    new("RFC1123"u8, RFC1123, "Thu, 04 Feb 2010 22:00:57 PDT"u8, true, true, 1, 0),
    new("RFC1123Z"u8, RFC1123Z, "Thu, 04 Feb 2010 21:00:57 -0800"u8, true, true, 1, 0),
    new("RFC3339"u8, RFC3339, "2010-02-04T21:00:57-08:00"u8, true, false, 1, 0),
    new("custom: \"2006-01-02 15:04:05-07\""u8, "2006-01-02 15:04:05-07"u8, "2010-02-04 21:00:57-08"u8, true, false, 1, 0),
    new("ANSIC"u8, ANSIC, "Thu Feb  4 21:00:57.0 2010"u8, false, true, 1, 1),
    new("UnixDate"u8, UnixDate, "Thu Feb  4 21:00:57.01 PST 2010"u8, true, true, 1, 2),
    new("RubyDate"u8, RubyDate, "Thu Feb 04 21:00:57.012 -0800 2010"u8, true, true, 1, 3),
    new("RFC850"u8, RFC850, "Thursday, 04-Feb-10 21:00:57.0123 PST"u8, true, true, 1, 4),
    new("RFC1123"u8, RFC1123, "Thu, 04 Feb 2010 21:00:57.01234 PST"u8, true, true, 1, 5),
    new("RFC1123Z"u8, RFC1123Z, "Thu, 04 Feb 2010 21:00:57.01234 -0800"u8, true, true, 1, 5),
    new("RFC3339"u8, RFC3339, "2010-02-04T21:00:57.012345678-08:00"u8, true, false, 1, 9),
    new("custom: \"2006-01-02 15:04:05\""u8, "2006-01-02 15:04:05"u8, "2010-02-04 21:00:57.0"u8, false, false, 1, 0),
    new("ANSIC"u8, ANSIC, "Thu Feb 4 21:00:57 2010"u8, false, true, 1, 0),
    new("ANSIC"u8, ANSIC, "Thu      Feb     4     21:00:57     2010"u8, false, true, 1, 0),
    new("ANSIC"u8, ANSIC, "THU FEB 4 21:00:57 2010"u8, false, true, 1, 0),
    new("ANSIC"u8, ANSIC, "thu feb 4 21:00:57 2010"u8, false, true, 1, 0),
    new("millisecond:: dot separator"u8, "Mon Jan _2 15:04:05.000 2006"u8, "Thu Feb  4 21:00:57.012 2010"u8, false, true, 1, 3),
    new("microsecond:: dot separator"u8, "Mon Jan _2 15:04:05.000000 2006"u8, "Thu Feb  4 21:00:57.012345 2010"u8, false, true, 1, 6),
    new("nanosecond:: dot separator"u8, "Mon Jan _2 15:04:05.000000000 2006"u8, "Thu Feb  4 21:00:57.012345678 2010"u8, false, true, 1, 9),
    new("millisecond:: comma separator"u8, "Mon Jan _2 15:04:05,000 2006"u8, "Thu Feb  4 21:00:57.012 2010"u8, false, true, 1, 3),
    new("microsecond:: comma separator"u8, "Mon Jan _2 15:04:05,000000 2006"u8, "Thu Feb  4 21:00:57.012345 2010"u8, false, true, 1, 6),
    new("nanosecond:: comma separator"u8, "Mon Jan _2 15:04:05,000000000 2006"u8, "Thu Feb  4 21:00:57.012345678 2010"u8, false, true, 1, 9),
    new("zero1"u8, "2006.01.02.15.04.05.0"u8, "2010.02.04.21.00.57.0"u8, false, false, 1, 1),
    new("zero2"u8, "2006.01.02.15.04.05.00"u8, "2010.02.04.21.00.57.01"u8, false, false, 1, 2),
    new("Janet"u8, "Hi Janet, the Month is January: Jan _2 15:04:05 2006"u8, "Hi Janet, the Month is February: Feb  4 21:00:57 2010"u8, false, true, 1, 0),
    new("GMT-8"u8, UnixDate, "Fri Feb  5 05:00:57 GMT-8 2010"u8, true, true, 1, 0),
    new(""u8, "2006-01-02 15:04:05.9999 -0700 MST"u8, "2010-02-04 21:00:57 -0800 PST"u8, true, false, 1, 0),
    new(""u8, "2006-01-02 15:04:05.999999999 -0700 MST"u8, "2010-02-04 21:00:57 -0800 PST"u8, true, false, 1, 0),
    new(""u8, "2006-01-02 15:04:05.9999 -0700 MST"u8, "2010-02-04 21:00:57.0123 -0800 PST"u8, true, false, 1, 4),
    new(""u8, "2006-01-02 15:04:05.999999999 -0700 MST"u8, "2010-02-04 21:00:57.0123 -0800 PST"u8, true, false, 1, 4),
    new(""u8, "2006-01-02 15:04:05.9999 -0700 MST"u8, "2010-02-04 21:00:57.012345678 -0800 PST"u8, true, false, 1, 9),
    new(""u8, "2006-01-02 15:04:05.999999999 -0700 MST"u8, "2010-02-04 21:00:57.012345678 -0800 PST"u8, true, false, 1, 9),
    new(""u8, "2006-01-02 15:04:05,9999 -0700 MST"u8, "2010-02-04 21:00:57 -0800 PST"u8, true, false, 1, 0),
    new(""u8, "2006-01-02 15:04:05,999999999 -0700 MST"u8, "2010-02-04 21:00:57 -0800 PST"u8, true, false, 1, 0),
    new(""u8, "2006-01-02 15:04:05,9999 -0700 MST"u8, "2010-02-04 21:00:57.0123 -0800 PST"u8, true, false, 1, 4),
    new(""u8, "2006-01-02 15:04:05,999999999 -0700 MST"u8, "2010-02-04 21:00:57.0123 -0800 PST"u8, true, false, 1, 4),
    new(""u8, "2006-01-02 15:04:05,9999 -0700 MST"u8, "2010-02-04 21:00:57.012345678 -0800 PST"u8, true, false, 1, 9),
    new(""u8, "2006-01-02 15:04:05,999999999 -0700 MST"u8, "2010-02-04 21:00:57.012345678 -0800 PST"u8, true, false, 1, 9),
    new(""u8, StampNano, "Feb  4 21:00:57.012345678"u8, false, false, -1, 9),
    new(""u8, "Jan _2 15:04:05.999"u8, "Feb  4 21:00:57.012300000"u8, false, false, -1, 4),
    new(""u8, "Jan _2 15:04:05.999"u8, "Feb  4 21:00:57.012345678"u8, false, false, -1, 9),
    new(""u8, "Jan _2 15:04:05.999999999"u8, "Feb  4 21:00:57.0123"u8, false, false, -1, 4),
    new(""u8, "Jan _2 15:04:05.999999999"u8, "Feb  4 21:00:57.012345678"u8, false, false, -1, 9),
    new(""u8, "2006-01-02 002 15:04:05"u8, "2010-02-04 035 21:00:57"u8, false, false, 1, 0),
    new(""u8, "2006-01 002 15:04:05"u8, "2010-02 035 21:00:57"u8, false, false, 1, 0),
    new(""u8, "2006-002 15:04:05"u8, "2010-035 21:00:57"u8, false, false, 1, 0),
    new(""u8, "200600201 15:04:05"u8, "201003502 21:00:57"u8, false, false, 1, 0),
    new(""u8, "200600204 15:04:05"u8, "201003504 21:00:57"u8, false, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z07"u8, "2010-02-04T21:00:57Z"u8, false, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z07"u8, "2010-02-04T21:00:57+08"u8, false, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z07"u8, "2010-02-04T21:00:57-08"u8, true, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z0700"u8, "2010-02-04T21:00:57Z"u8, false, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z0700"u8, "2010-02-04T21:00:57+0800"u8, false, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z0700"u8, "2010-02-04T21:00:57-0800"u8, true, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z07:00"u8, "2010-02-04T21:00:57Z"u8, false, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z07:00"u8, "2010-02-04T21:00:57+08:00"u8, false, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z07:00"u8, "2010-02-04T21:00:57-08:00"u8, true, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z070000"u8, "2010-02-04T21:00:57Z"u8, false, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z070000"u8, "2010-02-04T21:00:57+080000"u8, false, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z070000"u8, "2010-02-04T21:00:57-080000"u8, true, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z07:00:00"u8, "2010-02-04T21:00:57Z"u8, false, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z07:00:00"u8, "2010-02-04T21:00:57+08:00:00"u8, false, false, 1, 0),
    new(""u8, "2006-01-02T15:04:05Z07:00:00"u8, "2010-02-04T21:00:57-08:00:00"u8, true, false, 1, 0)
}.slice();

public static void TestParse(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in parseTests) {
        ref var test = ref heap(new ParseTest(), out var Ꮡtest);
        test = vᴛ1;

        var (time, err) = Parse(test.format, test.value);
        if (err != default!){
            Ꮡt.Errorf("%s error: %v"u8, test.name, err);
        } else {
            checkTime(time, Ꮡtest, Ꮡt);
        }
    }
}

// All parsed with ANSIC.

partial struct dayOutOfRangeTestsᴛ1 /*dyn*/ {
    internal @string date;
    internal bool ok;
}
internal static slice<dayOutOfRangeTestsᴛ1> dayOutOfRangeTests = new dayOutOfRangeTestsᴛ1[]{
    new("Thu Jan 99 21:00:57 2010"u8, false),
    new("Thu Jan 31 21:00:57 2010"u8, true),
    new("Thu Jan 32 21:00:57 2010"u8, false),
    new("Thu Feb 28 21:00:57 2012"u8, true),
    new("Thu Feb 29 21:00:57 2012"u8, true),
    new("Thu Feb 29 21:00:57 2010"u8, false),
    new("Thu Mar 31 21:00:57 2010"u8, true),
    new("Thu Mar 32 21:00:57 2010"u8, false),
    new("Thu Apr 30 21:00:57 2010"u8, true),
    new("Thu Apr 31 21:00:57 2010"u8, false),
    new("Thu May 31 21:00:57 2010"u8, true),
    new("Thu May 32 21:00:57 2010"u8, false),
    new("Thu Jun 30 21:00:57 2010"u8, true),
    new("Thu Jun 31 21:00:57 2010"u8, false),
    new("Thu Jul 31 21:00:57 2010"u8, true),
    new("Thu Jul 32 21:00:57 2010"u8, false),
    new("Thu Aug 31 21:00:57 2010"u8, true),
    new("Thu Aug 32 21:00:57 2010"u8, false),
    new("Thu Sep 30 21:00:57 2010"u8, true),
    new("Thu Sep 31 21:00:57 2010"u8, false),
    new("Thu Oct 31 21:00:57 2010"u8, true),
    new("Thu Oct 32 21:00:57 2010"u8, false),
    new("Thu Nov 30 21:00:57 2010"u8, true),
    new("Thu Nov 31 21:00:57 2010"u8, false),
    new("Thu Dec 31 21:00:57 2010"u8, true),
    new("Thu Dec 32 21:00:57 2010"u8, false),
    new("Thu Dec 00 21:00:57 2010"u8, false)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dayOutOfRangeˢ = "day out of range"u8;

public static void TestParseDayOutOfRange(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in dayOutOfRangeTests) {
        var (_, err) = Parse(ANSIC, test.date);
        switch (ᐧ) {
        case {} when test.ok && err == default!: {
            break;
        }
        case {} when !test.ok && err != default!: {
            if (!strings.Contains(err.Error(), // OK
 dayOutOfRangeˢ)) {
                Ꮡt.Errorf("%q: expected 'day' error, got %v"u8, test.date, err);
            }
            break;
        }
        case {} when test.ok && err != default!: {
            Ꮡt.Errorf("%q: unexpected error: %v"u8, test.date, err);
            break;
        }
        case {} when !test.ok && err == default!: {
            Ꮡt.Errorf("%q: expected 'day' error, got none"u8, test.date);
            break;
        }}

    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string asiaBaghdadˢ = "Asia/Baghdad"u8;
internal static readonly @string jan022006Mstˢ = "Jan 02 2006 MST"u8;
internal static readonly @string feb012013Astˢ = "Feb 01 2013 AST"u8;
internal static readonly @string americaBlancSablonˢ = "America/Blanc-Sablon"u8;

// TestParseInLocation checks that the Parse and ParseInLocation
// functions do not get confused by the fact that AST (Arabia Standard
// Time) and AST (Atlantic Standard Time) are different time zones,
// even though they have the same abbreviation.
//
// ICANN has been slowly phasing out invented abbreviation in favor of
// numeric time zones (for example, the Asia/Baghdad time zone
// abbreviation got changed from AST to +03 in the 2017a tzdata
// release); but we still want to make sure that the time package does
// not get confused on systems with slightly older tzdata packages.
public static void TestParseInLocation(ж<Δtesting.T> Ꮡt) {
    var (baghdad, err) = LoadLocation(asiaBaghdadˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    Δtime.Time t1 = default!;
    Δtime.Time t2 = default!;
    (t1, err) = ParseInLocation(jan022006Mstˢ, feb012013Astˢ, baghdad);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (_, offset) = t1.Zone();
    // A zero offset means that ParseInLocation did not recognize the
    // 'AST' abbreviation as matching the current location (Baghdad,
    // where we'd expect a +03 hrs offset); likely because we're using
    // a recent tzdata release (2017a or newer).
    // If it happens, skip the Baghdad test.
    if (offset != 0) {
        t2 = Date(2013, February, 1, 0, 0, 0, 0, baghdad);
        if (t1 != t2) {
            Ꮡt.Fatalf("ParseInLocation(Feb 01 2013 AST, Baghdad) = %v, want %v"u8, t1, t2);
        }
        if (offset != 3 * 60 * 60) {
            Ꮡt.Fatalf("ParseInLocation(Feb 01 2013 AST, Baghdad).Zone = _, %d, want _, %d"u8, offset, (nint)(3 * 60 * 60));
        }
    }
    (var blancSablon, err) = LoadLocation(americaBlancSablonˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // In this case 'AST' means 'Atlantic Standard Time', and we
    // expect the abbreviation to correctly match the american
    // location.
    (t1, err) = ParseInLocation(jan022006Mstˢ, feb012013Astˢ, blancSablon);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    t2 = Date(2013, February, 1, 0, 0, 0, 0, blancSablon);
    if (t1 != t2) {
        Ꮡt.Fatalf("ParseInLocation(Feb 01 2013 AST, Blanc-Sablon) = %v, want %v"u8, t1, t2);
    }
    (_, offset) = t1.Zone();
    if (offset != -4 * 60 * 60) {
        Ꮡt.Fatalf("ParseInLocation(Feb 01 2013 AST, Blanc-Sablon).Zone = _, %d, want _, %d"u8, offset, (nint)(-4 * 60 * 60));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string australiaSydneyˢ = "Australia/Sydney"u8;

public static void TestLoadLocationZipFile(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);
        var (_, err) = LoadLocation(australiaSydneyˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Ignore the time zone in the test. If it parses, it'll be OK.
internal static slice<ParseTest> rubyTests = new ParseTest[]{
    new("RubyDate"u8, RubyDate, "Thu Feb 04 21:00:57 -0800 2010"u8, true, true, 1, 0),
    new("RubyDate"u8, RubyDate, "Thu Feb 04 21:00:57 -0000 2010"u8, false, true, 1, 0),
    new("RubyDate"u8, RubyDate, "Thu Feb 04 21:00:57 +0000 2010"u8, false, true, 1, 0),
    new("RubyDate"u8, RubyDate, "Thu Feb 04 21:00:57 +1130 2010"u8, false, true, 1, 0)
}.slice();

// Problematic time zone format needs special tests.
public static void TestRubyParse(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in rubyTests) {
        ref var test = ref heap(new ParseTest(), out var Ꮡtest);
        test = vᴛ1;

        var (time, err) = Parse(test.format, test.value);
        if (err != default!){
            Ꮡt.Errorf("%s error: %v"u8, test.name, err);
        } else {
            checkTime(time, Ꮡtest, Ꮡt);
        }
    }
}

internal static void checkTime(Δtime.Time time, ж<ParseTest> Ꮡtest, ж<Δtesting.T> Ꮡt) {
    ref var test = ref Ꮡtest.DerefOrNull();

    // The time should be Thu Feb  4 21:00:57 PST 2010
    if (test.yearSign >= 0 && test.yearSign * time.Year() != 2010) {
        Ꮡt.Errorf("%s: bad year: %d not %d"u8, test.name, time.Year(), (nint)(2010));
    }
    if (time.Month() != February) {
        Ꮡt.Errorf("%s: bad month: %s not %s"u8, test.name, time.Month(), February);
    }
    if (time.Day() != 4) {
        Ꮡt.Errorf("%s: bad day: %d not %d"u8, test.name, time.Day(), (nint)(4));
    }
    if (time.Hour() != 21) {
        Ꮡt.Errorf("%s: bad hour: %d not %d"u8, test.name, time.Hour(), (nint)(21));
    }
    if (time.Minute() != 0) {
        Ꮡt.Errorf("%s: bad minute: %d not %d"u8, test.name, time.Minute(), (nint)(0));
    }
    if (time.Second() != 57) {
        Ꮡt.Errorf("%s: bad second: %d not %d"u8, test.name, time.Second(), (nint)(57));
    }
    // Nanoseconds must be checked against the precision of the input.
    var (nanosec, err) = strconv.ParseUint(((@string)"012345678"u8.slice(0, test.fracDigits)) + "000000000"u8.slice(0, 9 - test.fracDigits), 10, 0);
    if (err != default!) {
        throw panic(err);
    }
    if (time.Nanosecond() != (nint)nanosec) {
        Ꮡt.Errorf("%s: bad nanosecond: %d not %d"u8, test.name, time.Nanosecond(), nanosec);
    }
    var (name, offset) = time.Zone();
    if (test.hasTZ && offset != -28800) {
        Ꮡt.Errorf("%s: bad tz offset: %s %d not %d"u8, test.name, name, offset, (nint)(-28800));
    }
    if (test.hasWD && time.Weekday() != Thursday) {
        Ꮡt.Errorf("%s: bad weekday: %s not %s"u8, test.name, time.Weekday(), Thursday);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fmtᶜ = "Mon MST 2006-01-02T15:04:05Z07:00";

public static void TestFormatAndParse(ж<Δtesting.T> Ꮡt) {
    @string fmt = fmtᶜ; // all fields
    var f = (int64 sec) => {
        var t1 = Unix(sec / 2, 0);
        if (t1.Year() < 1000 || t1.Year() > 9999 || t1.Unix() != sec) {
            // not required to work
            return true;
        }
        var (t2, err) = Parse(fmt, t1.Format(fmt));
        if (err != default!) {
            Ꮡt.Errorf("error: %s"u8, err);
            return false;
        }
        if (t1.Unix() != t2.Unix() || t1.Nanosecond() != t2.Nanosecond()) {
            Ꮡt.Errorf("FormatAndParse %d: %q(%d) %q(%d)"u8, sec, t1, t1.Unix(), t2, t2.Unix());
            return false;
        }
        return true;
    };
    var fʗ1 = f;
    var f32 = (int32 sec) => fʗ1((int64)sec);
    var cfg = Ꮡ(new quick.Config(MaxCount: 10000));
    // Try a reasonable date first, then the huge ones.
    {
        var err = quick.Check((f32).OrTypedNilFunc(), cfg); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = quick.Check((f).OrTypedNilFunc(), cfg); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

partial struct ParseTimeZoneTest {
    internal @string value;
    internal nint length;
    internal bool ok;
}

// four letters must end in T.
// run of upper-case letters too long.
// five letters must end in T.
// Issue #18251
// Issue #24071
// Issue #26032
internal static slice<ParseTimeZoneTest> parseTimeZoneTests = new ParseTimeZoneTest[]{
    new("gmt hi there"u8, 0, false),
    new("GMT hi there"u8, 3, true),
    new("GMT+12 hi there"u8, 6, true),
    new("GMT+00 hi there"u8, 6, true),
    new("GMT+"u8, 3, true),
    new("GMT+3"u8, 5, true),
    new("GMT+a"u8, 3, true),
    new("GMT+3a"u8, 5, true),
    new("GMT-5 hi there"u8, 5, true),
    new("GMT-51 hi there"u8, 3, true),
    new("ChST hi there"u8, 4, true),
    new("MeST hi there"u8, 4, true),
    new("MSDx"u8, 3, true),
    new("MSDY"u8, 0, false),
    new("ESAST hi"u8, 5, true),
    new("ESASTT hi"u8, 0, false),
    new("ESATY hi"u8, 0, false),
    new("WITA hi"u8, 4, true),
    new("+03 hi"u8, 3, true),
    new("-04 hi"u8, 3, true),
    new("+00"u8, 3, true),
    new("-11"u8, 3, true),
    new("-12"u8, 3, true),
    new("-23"u8, 3, true),
    new("-24"u8, 0, false),
    new("+13"u8, 3, true),
    new("+14"u8, 3, true),
    new("+23"u8, 3, true),
    new("+24"u8, 0, false)
}.slice();

public static void TestParseTimeZone(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in parseTimeZoneTests) {
        var (length, ok) = time_internal_test_package.ParseTimeZone(test.value);
        if (ok != test.ok){
            Ꮡt.Errorf("expected %t for %q got %t"u8, test.ok, test.value, ok);
        } else 
        if (length != test.length) {
            Ꮡt.Errorf("expected %d for %q got %d"u8, test.length, test.value, length);
        }
    }
}

partial struct ParseErrorTest {
    internal @string format;
    internal @string value;
    internal @string expect; // must appear within the error
}

// issue 4502. StampNano requires exactly 9 digits of precision.
// issue 4493. Helpful errors.
// invalid second followed by optional fractional seconds
// issue 54569
// issue 21113
// invalid or mismatched day-of-year
// issue 45391.
// issue 54570
// issue 56730
// issue 67470
internal static slice<ParseErrorTest> parseErrorTests = new ParseErrorTest[]{
    new(ANSIC, "Feb  4 21:00:60 2010"u8, @"cannot parse ""Feb  4 21:00:60 2010"" as ""Mon"""u8),
    new(ANSIC, "Thu Feb  4 21:00:57 @2010"u8, @"cannot parse ""@2010"" as ""2006"""u8),
    new(ANSIC, "Thu Feb  4 21:00:60 2010"u8, "second out of range"u8),
    new(ANSIC, "Thu Feb  4 21:61:57 2010"u8, "minute out of range"u8),
    new(ANSIC, "Thu Feb  4 24:00:60 2010"u8, "hour out of range"u8),
    new("Mon Jan _2 15:04:05.000 2006"u8, "Thu Feb  4 23:00:59x01 2010"u8, @"cannot parse ""x01 2010"" as "".000"""u8),
    new("Mon Jan _2 15:04:05.000 2006"u8, "Thu Feb  4 23:00:59.xxx 2010"u8, @"cannot parse "".xxx 2010"" as "".000"""u8),
    new("Mon Jan _2 15:04:05.000 2006"u8, "Thu Feb  4 23:00:59.-123 2010"u8, "fractional second out of range"u8),
    new(StampNano, "Dec  7 11:22:01.000000"u8, @"cannot parse "".000000"" as "".000000000"""u8),
    new(StampNano, "Dec  7 11:22:01.0000000000"u8, @"extra text: ""0"""u8),
    new(RFC3339, "2006-01-02T15:04:05Z07:00"u8, @"parsing time ""2006-01-02T15:04:05Z07:00"": extra text: ""07:00"""u8),
    new(RFC3339, "2006-01-02T15:04_abc"u8, @"parsing time ""2006-01-02T15:04_abc"" as ""2006-01-02T15:04:05Z07:00"": cannot parse ""_abc"" as "":"""u8),
    new(RFC3339, "2006-01-02T15:04:05_abc"u8, @"parsing time ""2006-01-02T15:04:05_abc"" as ""2006-01-02T15:04:05Z07:00"": cannot parse ""_abc"" as ""Z07:00"""u8),
    new(RFC3339, "2006-01-02T15:04:05Z_abc"u8, @"parsing time ""2006-01-02T15:04:05Z_abc"": extra text: ""_abc"""u8),
    new(RFC3339, "2010-02-04T21:00:67.012345678-08:00"u8, "second out of range"u8),
    new(RFC3339, "0000-01-01T00:00:.0+00:00"u8, @"parsing time ""0000-01-01T00:00:.0+00:00"" as ""2006-01-02T15:04:05Z07:00"": cannot parse "".0+00:00"" as ""05"""u8),
    new("_2 Jan 06 15:04 MST"u8, "4 --- 00 00:00 GMT"u8, @"cannot parse ""--- 00 00:00 GMT"" as ""Jan"""u8),
    new("_2 January 06 15:04 MST"u8, "4 --- 00 00:00 GMT"u8, @"cannot parse ""--- 00 00:00 GMT"" as ""January"""u8),
    new("Jan _2 002 2006"u8, "Feb  4 034 2006"u8, "day-of-year does not match day"u8),
    new("Jan _2 002 2006"u8, "Feb  4 004 2006"u8, "day-of-year does not match month"u8),
    new(@"""2006-01-02T15:04:05Z07:00"""u8, "0"u8, @"parsing time ""0"" as ""\""2006-01-02T15:04:05Z07:00\"""": cannot parse ""0"" as ""\"""""u8),
    new(RFC3339, "\""u8, @"parsing time ""\"""" as ""2006-01-02T15:04:05Z07:00"": cannot parse ""\"""" as ""2006"""u8),
    new(RFC3339, "0000-01-01T00:00:00+00:+0"u8, @"parsing time ""0000-01-01T00:00:00+00:+0"" as ""2006-01-02T15:04:05Z07:00"": cannot parse ""+00:+0"" as ""Z07:00"""u8),
    new(RFC3339, "0000-01-01T00:00:00+-0:00"u8, @"parsing time ""0000-01-01T00:00:00+-0:00"" as ""2006-01-02T15:04:05Z07:00"": cannot parse ""+-0:00"" as ""Z07:00"""u8),
    new("2006-01-02"u8, "22-10-25"u8, @"parsing time ""22-10-25"" as ""2006-01-02"": cannot parse ""22-10-25"" as ""2006"""u8),
    new("06-01-02"u8, "a2-10-25"u8, @"parsing time ""a2-10-25"" as ""06-01-02"": cannot parse ""a2-10-25"" as ""06"""u8),
    new("03:04PM"u8, "12:03pM"u8, @"parsing time ""12:03pM"" as ""03:04PM"": cannot parse ""pM"" as ""PM"""u8),
    new("03:04pm"u8, "12:03pM"u8, @"parsing time ""12:03pM"" as ""03:04pm"": cannot parse ""pM"" as ""pm"""u8),
    new("-07"u8, "-25"u8, "time zone offset hour out of range"u8),
    new("-07:00"u8, "+25:00"u8, "time zone offset hour out of range"u8),
    new("-07:00"u8, "-23:61"u8, "time zone offset minute out of range"u8),
    new("-07:00:00"u8, "+23:59:61"u8, "time zone offset second out of range"u8),
    new("Z07"u8, "-25"u8, "time zone offset hour out of range"u8),
    new("Z07:00"u8, "+25:00"u8, "time zone offset hour out of range"u8),
    new("Z07:00"u8, "-23:61"u8, "time zone offset minute out of range"u8),
    new("Z07:00:00"u8, "+23:59:61"u8, "time zone offset second out of range"u8)
}.slice();

public static void TestParseErrors(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in parseErrorTests) {
        var (_, err) = Parse(test.format, test.value);
        if (err == default!){
            Ꮡt.Errorf("expected error for %q %q"u8, test.format, test.value);
        } else 
        if (!strings.Contains(err.Error(), test.expect)) {
            Ꮡt.Errorf("expected error with %q for %q %q; got %s"u8, test.expect, test.format, test.value, err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectᶜ = "12:00PM"u8;

public static void TestNoonIs12PM(ж<Δtesting.T> Ꮡt) {
    var noon = Date(0, January, 1, 12, 0, 0, 0, ΔUTC);
    @string expect = expectᶜ;
    @string got = noon.Format("3:04PM"u8);
    if (got != expect) {
        Ꮡt.Errorf("got %q; expect %q"u8, got, expect);
    }
    got = noon.Format("03:04PM"u8);
    if (got != expect) {
        Ꮡt.Errorf("got %q; expect %q"u8, got, expect);
    }
}

public static void TestMidnightIs12AM(ж<Δtesting.T> Ꮡt) {
    var midnight = Date(0, January, 1, 0, 0, 0, 0, ΔUTC);
    @string expect = "12:00AM"u8;
    @string got = midnight.Format("3:04PM"u8);
    if (got != expect) {
        Ꮡt.Errorf("got %q; expect %q"u8, got, expect);
    }
    got = midnight.Format("03:04PM"u8);
    if (got != expect) {
        Ꮡt.Errorf("got %q; expect %q"u8, got, expect);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorParsingDateˢ = (@string)"error parsing date:"u8;

public static void Test12PMIsNoon(ж<Δtesting.T> Ꮡt) {
    var (noon, err) = Parse("3:04PM"u8, "12:00PM"u8);
    if (err != default!) {
        Ꮡt.Fatal(errorParsingDateˢ, err);
    }
    if (noon.Hour() != 12) {
        Ꮡt.Errorf("got %d; expect 12"u8, noon.Hour());
    }
    (noon, err) = Parse("03:04PM"u8, "12:00PM"u8);
    if (err != default!) {
        Ꮡt.Fatal(errorParsingDateˢ, err);
    }
    if (noon.Hour() != 12) {
        Ꮡt.Errorf("got %d; expect 12"u8, noon.Hour());
    }
}

public static void Test12AMIsMidnight(ж<Δtesting.T> Ꮡt) {
    var (midnight, err) = Parse("3:04PM"u8, "12:00AM"u8);
    if (err != default!) {
        Ꮡt.Fatal(errorParsingDateˢ, err);
    }
    if (midnight.Hour() != 0) {
        Ꮡt.Errorf("got %d; expect 0"u8, midnight.Hour());
    }
    (midnight, err) = Parse("03:04PM"u8, "12:00AM"u8);
    if (err != default!) {
        Ꮡt.Fatal(errorParsingDateˢ, err);
    }
    if (midnight.Hour() != 0) {
        Ꮡt.Errorf("got %d; expect 0"u8, midnight.Hour());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string thuFeb0216100305002006ˢ = "Thu Feb 02 16:10:03 -0500 2006"u8;
internal static readonly @string thuFeb216100305002006ˢ = "Thu Feb  2 16:10:03 -0500 2006"u8;

// Check that a time without a Zone still produces a (numeric) time zone
// when formatted with MST as a requested zone.
public static void TestMissingZone(ж<Δtesting.T> Ꮡt) {
    var (time, err) = Parse(RubyDate, thuFeb0216100305002006ˢ);
    if (err != default!) {
        Ꮡt.Fatal(errorParsingDateˢ, err);
    }
    @string expect = thuFeb216100305002006ˢ; // -0500 not EST
    @string str = time.Format(UnixDate); // uses MST as its time zone
    if (str != expect) {
        Ꮡt.Errorf("got %s; expect %s"u8, str, expect);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string monJan0215040501232006ˢ = "Mon Jan 02 15:04:05 +0123 2006"u8;

public static void TestMinutesInTimeZone(ж<Δtesting.T> Ꮡt) {
    var (time, err) = Parse(RubyDate, monJan0215040501232006ˢ);
    if (err != default!) {
        Ꮡt.Fatal(errorParsingDateˢ, err);
    }
    nint expected = (1 * 60 + 23) * 60;
    var (_, offset) = time.Zone();
    if (offset != expected) {
        Ꮡt.Errorf("ZoneOffset = %d, want %d"u8, offset, expected);
    }
}

partial struct SecondsTimeZoneOffsetTest {
    internal @string format;
    internal @string value;
    internal nint expectedoffset;
}

internal static slice<SecondsTimeZoneOffsetTest> secondsTimeZoneOffsetTests = new SecondsTimeZoneOffsetTest[]{
    new("2006-01-02T15:04:05-070000"u8, "1871-01-01T05:33:02-003408"u8, -(34 * 60 + 8)),
    new("2006-01-02T15:04:05-07:00:00"u8, "1871-01-01T05:33:02-00:34:08"u8, -(34 * 60 + 8)),
    new("2006-01-02T15:04:05-070000"u8, "1871-01-01T05:33:02+003408"u8, 34 * 60 + 8),
    new("2006-01-02T15:04:05-07:00:00"u8, "1871-01-01T05:33:02+00:34:08"u8, 34 * 60 + 8),
    new("2006-01-02T15:04:05Z070000"u8, "1871-01-01T05:33:02-003408"u8, -(34 * 60 + 8)),
    new("2006-01-02T15:04:05Z07:00:00"u8, "1871-01-01T05:33:02+00:34:08"u8, 34 * 60 + 8),
    new("2006-01-02T15:04:05-07"u8, "1871-01-01T05:33:02+01"u8, 1 * 60 * 60),
    new("2006-01-02T15:04:05-07"u8, "1871-01-01T05:33:02-02"u8, -2 * 60 * 60),
    new("2006-01-02T15:04:05Z07"u8, "1871-01-01T05:33:02-02"u8, -2 * 60 * 60)
}.slice();

public static void TestParseSecondsInTimeZone(ж<Δtesting.T> Ꮡt) {
    // should accept timezone offsets with seconds like: Zone America/New_York   -4:56:02 -      LMT     1883 Nov 18 12:03:58
    foreach (var (_, test) in secondsTimeZoneOffsetTests) {
        var (time, err) = Parse(test.format, test.value);
        if (err != default!) {
            Ꮡt.Fatal(errorParsingDateˢ, err);
        }
        var (_, offset) = time.Zone();
        if (offset != test.expectedoffset) {
            Ꮡt.Errorf("ZoneOffset = %d, want %d"u8, offset, test.expectedoffset);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lmtˢ = "LMT"u8;

public static void TestFormatSecondsInTimeZone(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in secondsTimeZoneOffsetTests) {
        var d = Date(1871, 1, 1, 5, 33, 2, 0, FixedZone(lmtˢ, test.expectedoffset));
        @string timestr = d.Format(test.format);
        if (timestr != test.value) {
            Ꮡt.Errorf("Format = %s, want %s"u8, timestr, test.value);
        }
    }
}

// Issue 11334.
public static void TestUnderscoreTwoThousand(ж<Δtesting.T> Ꮡt) {
    @string format = "15:04_20060102"u8;
    @string input = "14:38_20150618"u8;
    var (time, err) = Parse(format, input);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    {
        var (y, m, d) = time.Date(); if (y != 2015 || m != 6 || d != 18) {
            Ꮡt.Errorf("Incorrect y/m/d, got %d/%d/%d"u8, y, m, d);
        }
    }
    {
        nint h = time.Hour(); if (h != 14) {
            Ꮡt.Errorf("Incorrect hour, got %d"u8, h);
        }
    }
    {
        nint m = time.Minute(); if (m != 38) {
            Ꮡt.Errorf("Incorrect minute, got %d"u8, m);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotParseˢ = "cannot parse"u8;

internal partial struct TestStd0xParseError_tests /*dyn*/ {
    internal @string format, value, valueElemPrefix;
}

// Issue 29918, 29916
public static void TestStd0xParseError(ж<Δtesting.T> Ꮡt) {
    var tests = new TestStd0xParseError_tests[]{
        new("01 MST"u8, "0 MST"u8, "0"u8),
        new("01 MST"u8, "1 MST"u8, "1"u8),
        new(RFC850, "Thursday, 04-Feb-1 21:00:57 PST"u8, "1"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        var (_, err) = Parse(tt.format, tt.value);
        if (err == default!){
            Ꮡt.Errorf("Parse(%q, %q) did not fail as expected"u8, tt.format, tt.value);
        } else 
        {
            var (perr, ok) = err._<ж<Δtime.ParseError>>(ᐧ); if (!ok){
                Ꮡt.Errorf("Parse(%q, %q) returned error type %T, expected ParseError"u8, tt.format, tt.value, perr.OrTypedNil());
            } else 
            if (!strings.Contains(perr.Error(), cannotParseˢ) || !strings.HasPrefix((~perr).ValueElem, tt.valueElemPrefix)) {
                Ꮡt.Errorf("Parse(%q, %q) returned wrong parsing error message: %v"u8, tt.format, tt.value, perr.OrTypedNil());
            }
        }
    }
}


partial struct monthOutOfRangeTestsᴛ1 /*dyn*/ {
    internal @string value;
    internal bool ok;
}
internal static slice<monthOutOfRangeTestsᴛ1> monthOutOfRangeTests = new monthOutOfRangeTestsᴛ1[]{
    new("00-01"u8, false),
    new("13-01"u8, false),
    new("01-01"u8, true)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string monthOutOfRangeˢ = "month out of range"u8;

public static void TestParseMonthOutOfRange(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in monthOutOfRangeTests) {
        var (_, err) = Parse("01-02"u8, test.value);
        switch (ᐧ) {
        case {} when !test.ok && err != default!: {
            if (!strings.Contains(err.Error(), monthOutOfRangeˢ)) {
                Ꮡt.Errorf("%q: expected 'month' error, got %v"u8, test.value, err);
            }
            break;
        }
        case {} when test.ok && err != default!: {
            Ꮡt.Errorf("%q: unexpected error: %v"u8, test.value, err);
            break;
        }
        case {} when !test.ok && err == default!: {
            Ꮡt.Errorf("%q: expected 'month' error, got none"u8, test.value);
            break;
        }}

    }
}

// Issue 37387.
public static void TestParseYday(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    for (nint i = 1; i <= 365; i++) {
        @string d = fmt.Sprintf("2020-%03d"u8, i);
        var (tm, err) = Parse("2006-002"u8, d);
        if (err != default!){
            Ꮡt.Errorf("unexpected error for %s: %v"u8, d, err);
        } else 
        if (tm.Year() != 2020 || tm.YearDay() != i) {
            Ꮡt.Errorf("got year %d yearday %d, want %d %d"u8, tm.Year(), tm.YearDay(), (nint)(2020), i);
        }
    }
}

internal partial struct TestQuote_tests /*dyn*/ {
    internal @string s, want;
}

// Issue 45391.
public static void TestQuote(ж<Δtesting.T> Ꮡt) {
    var tests = new TestQuote_tests[]{
        new(@""""u8, @"""\"""""u8),
        new(@"abc""xyz"""u8, @"""abc\""xyz\"""""u8),
        new(""u8, @""""""u8),
        new("abc"u8, @"""abc"""u8),
        new(@"☺"u8, @"""\xe2\x98\xba"""u8),
        new(@"☺ hello ☺ hello"u8, @"""\xe2\x98\xba hello \xe2\x98\xba hello"""u8),
        new("\x04"u8, @"""\x04"""u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        {
            @string q = time_internal_test_package.Quote(tt.s); if (q != tt.want) {
                Ꮡt.Errorf("Quote(%q) = got %q, want %q"u8, tt.s, q, tt.want);
            }
        }
    }
}

internal partial struct TestFormatFractionalSecondSeparators_tests /*dyn*/ {
    internal @string s, want;
}

// Issue 48037
public static void TestFormatFractionalSecondSeparators(ж<Δtesting.T> Ꮡt) {
    var tests = new TestFormatFractionalSecondSeparators_tests[]{
        new(@"15:04:05.000"u8, @"21:00:57.012"u8),
        new(@"15:04:05.999"u8, @"21:00:57.012"u8),
        new(@"15:04:05,000"u8, @"21:00:57,012"u8),
        new(@"15:04:05,999"u8, @"21:00:57,012"u8)
    }.slice();
    // The numeric time represents Thu Feb  4 21:00:57.012345600 PST 2009
    var time = Unix(0, 1233810057012345600L);
    foreach (var (_, tt) in tests) {
        {
            @string q = time.Format(tt.s); if (q != tt.want) {
                Ꮡt.Errorf("Format(%q) = got %q, want %q"u8, tt.s, q, tt.want);
            }
        }
    }
}

// 9 digits
// 10 digits, truncates
// 11 digits, truncates
// 12 digits, truncates
// 15 digits, truncates

partial struct longFractionalDigitsTestsᴛ1 /*dyn*/ {
    internal @string value;
    internal nint want;
}
internal static slice<longFractionalDigitsTestsᴛ1> longFractionalDigitsTests = new longFractionalDigitsTestsᴛ1[]{
    new("2021-09-29T16:04:33.000000000Z"u8, 0),
    new("2021-09-29T16:04:33.000000001Z"u8, 1),
    new("2021-09-29T16:04:33.100000000Z"u8, 100_000_000),
    new("2021-09-29T16:04:33.100000001Z"u8, 100_000_001),
    new("2021-09-29T16:04:33.999999999Z"u8, 999_999_999),
    new("2021-09-29T16:04:33.012345678Z"u8, 12_345_678),
    new("2021-09-29T16:04:33.0000000000Z"u8, 0),
    new("2021-09-29T16:04:33.0000000001Z"u8, 0),
    new("2021-09-29T16:04:33.1000000000Z"u8, 100_000_000),
    new("2021-09-29T16:04:33.1000000009Z"u8, 100_000_000),
    new("2021-09-29T16:04:33.9999999999Z"u8, 999_999_999),
    new("2021-09-29T16:04:33.0123456789Z"u8, 12_345_678),
    new("2021-09-29T16:04:33.10000000000Z"u8, 100_000_000),
    new("2021-09-29T16:04:33.00123456789Z"u8, 1_234_567),
    new("2021-09-29T16:04:33.000123456789Z"u8, 123_456),
    new("2021-09-29T16:04:33.9999999999999999Z"u8, 999_999_999)
}.slice();

// Issue 48685 and 54567.
public static void TestParseFractionalSecondsLongerThanNineDigits(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in longFractionalDigitsTests) {
        foreach (var (_, format) in new @string[]{RFC3339, RFC3339Nano}.slice()) {
            var (tm, err) = Parse(format, tt.value);
            if (err != default!) {
                Ꮡt.Errorf("Parse(%q, %q) error: %v"u8, format, tt.value, err);
                continue;
            }
            {
                nint got = tm.Nanosecond(); if (got != tt.want) {
                    Ꮡt.Errorf("Parse(%q, %q) = got %d, want %d"u8, format, tt.value, got, tt.want);
                }
            }
        }
    }
}

public static void FuzzFormatRFC3339(ж<Δtesting.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, vᴛ1) in GoReflect.WithElemDims(new array<int64>[]{
        new int64[]{Δmath.MinInt64, Δmath.MinInt64}.array(), // 292277026304-08-26T15:42:51Z

        new int64[]{-62167219200L, 0}.array(), // 0000-01-01T00:00:00Z

        new int64[]{1661201140, 676836973}.array(), // 2022-08-22T20:45:40.676836973Z

        new int64[]{253402300799L, 999999999}.array(), // 9999-12-31T23:59:59.999999999Z

        new int64[]{Δmath.MaxInt64, Δmath.MaxInt64}.array()
    }.slice(), 2)) {
        var ts = vᴛ1.Clone();

        // -292277022365-05-08T08:17:07Z
        f.Add(ts[0], ts[1], true, false, (nint)(0));
        f.Add(ts[0], ts[1], false, true, (nint)(0));
        foreach (var (_, offset) in new nint[]{0, 60, 60 * 60, 99 * 60 * 60 + 99 * 60, 123456789}.slice()) {
            f.Add(ts[0], ts[1], false, false, -offset);
            f.Add(ts[0], ts[1], false, false, +offset);
        }
    }
    Ꮡf.Fuzz((ж<Δtesting.T> t, int64 sec, int64 nsec, bool useUTC, bool useLocal, nint tzOffset) => {
        ж<timeꓸLocation> loc = default!;
        switch (ᐧ) {
        case {} when useUTC: {
            loc = ΔUTC;
            break;
        }
        case {} when useLocal: {
            loc = ΔLocal;
            break;
        }
        default: {
            loc = FixedZone(""u8, tzOffset);
            break;
        }}

        var ts = Unix(sec, nsec).In(loc);
        var got = time_internal_test_package.AppendFormatRFC3339(ts, default!, false);
        var want = time_internal_test_package.AppendFormatAny(ts, default!, RFC3339);
        if (!bytes.Equal(got, want)) {
            t.Errorf("Format(%s, RFC3339) mismatch:\n\tgot:  %s\n\twant: %s"u8, ts, got, want);
        }
        var gotNanos = time_internal_test_package.AppendFormatRFC3339(ts, default!, true);
        var wantNanos = time_internal_test_package.AppendFormatAny(ts, default!, RFC3339Nano);
        if (!bytes.Equal(gotNanos, wantNanos)) {
            t.Errorf("Format(%s, RFC3339Nano) mismatch:\n\tgot:  %s\n\twant: %s"u8, ts, gotNanos, wantNanos);
        }
    });
}

public static void FuzzParseRFC3339(ж<Δtesting.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, tt) in formatTests) {
        f.Add(tt.result);
    }
    foreach (var (_, tt) in parseTests) {
        f.Add(tt.value);
    }
    foreach (var (_, tt) in parseErrorTests) {
        f.Add(tt.value);
    }
    foreach (var (_, tt) in longFractionalDigitsTests) {
        f.Add(tt.value);
    }
    Ꮡf.Fuzz((ж<Δtesting.T> t, @string s) => {
        // equalTime is like time.Time.Equal, but also compares the time zone.
        bool equalTime(Δtime.Time t1, Δtime.Time t2) {
            var (name1, offset1) = t1.Zone();
            var (name2, offset2) = t2.Zone();
            return t1.Equal(t2) && name1 == name2 && offset1 == offset2;
        }
        foreach (var (_, tz) in new ж<timeꓸLocation>[]{ΔUTC, ΔLocal}.slice()) {
            // Parsing as RFC3339 or RFC3339Nano should be identical.
            var (t1, err1) = time_internal_test_package.ParseAny(RFC3339, s, ΔUTC, tz);
            var (t2, err2) = time_internal_test_package.ParseAny(RFC3339Nano, s, ΔUTC, tz);
            switch (ᐧ) {
            case {} when (err1 == default!) != (err2 == default!): {
                t.Fatalf("ParseAny(%q) error mismatch:\n\tgot:  %v\n\twant: %v"u8, s, err1, err2);
                break;
            }
            case {} when !equalTime(t1, t2): {
                t.Fatalf("ParseAny(%q) value mismatch:\n\tgot:  %v\n\twant: %v"u8, s, t1, t2);
                break;
            }}

            // TODO(https://go.dev/issue/54580):
            // Remove these checks after ParseAny rejects all invalid RFC 3339.
            if (err1 == default!) {
                byte num2(@string sΔ1) => (byte)(10 * (sΔ1[0] - (rune)'0') + (sΔ1[1] - (rune)'0'));
                switch (ᐧ) {
                case {} when len(s) > 12 && s[12] == (rune)':': {
                    t.Skipf("ParseAny(%q) incorrectly allows single-digit hour fields"u8, s);
                    break;
                }
                case {} when len(s) > 19 && s[19] == (rune)',': {
                    t.Skipf("ParseAny(%q) incorrectly allows comma as sub-second separator"u8, s);
                    break;
                }
                case {} when !strings.HasSuffix(s, "Z"u8) && len(s) > 4 && (num2(s.slice(len(s) - 5)) >= 24 || num2(s.slice(len(s) - 2)) >= 60): {
                    t.Skipf("ParseAny(%q) incorrectly allows out-of-range zone offset"u8, s);
                    break;
                }}

            }
            // Customized parser should be identical to general parser.
            {
                var (got, ok) = time_internal_test_package.ParseRFC3339(s, tz);
                switch (ᐧ) {
                case {} when ok != (err1 == default!): {
                    t.Fatalf("ParseRFC3339(%q) error mismatch:\n\tgot:  %v\n\twant: %v"u8, s, ok, err1 == default!);
                    break;
                }
                case {} when !equalTime(got, t1): {
                    t.Fatalf("ParseRFC3339(%q) value mismatch:\n\tgot:  %v\n\twant: %v"u8, s, got, t2);
                    break;
                }}
            }

        }
    });
}

} // end time_test_package
