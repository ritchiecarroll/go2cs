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

internal static slice<@string> nextStdChunkTests = [
    "(2006)-(01)-(02)T(15):(04):(05)(Z07:00)",
    "(2006)-(01)-(02) (002) (15):(04):(05)",
    "(2006)-(01) (002) (15):(04):(05)",
    "(2006)-(002) (15):(04):(05)",
    "(2006)(002)(01) (15):(04):(05)",
    "(2006)(002)(04) (15):(04):(05)"
];

public static void TestNextStdChunk(ж<Δtesting.T> t) {
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
        if (r == '(' || r == ')') {
            return -1;
        }
        return r;
    };

    foreach (var (_, marked) in range(nextStdChunkTests)) {
        // marked is an expected output from markChunks.
        // If we delete the parens and pass it through markChunks,
        // we should get the original back.
        @string format = strings.Map(noParens, marked);
        @string @out = markChunks(format);
        if (@out != marked) {
            t.Errorf("nextStdChunk parses %q as %q, want %q", format, @out, marked);
        }
    }
}

partial struct TimeFormatTest {
    internal Δtime.Time time;
    internal @string formattedValue;
}

internal static slice<TimeFormatTest> rfc3339Formats = slice<TimeFormatTest>([
    new(Date(2008, 9, 17, 20, 4, 26, 0, ΔUTC), "2008-09-17T20:04:26Z"),
    new(Date(1994, 9, 17, 20, 4, 26, 0, FixedZone("EST", -18000)), "1994-09-17T20:04:26-05:00"),
    new(Date(2000, 12, 26, 1, 15, 6, 0, FixedZone("OTO", 15600)), "2000-12-26T01:15:06+04:20")
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rfc3339ˢ = (@string)"RFC3339:"u8;

public static void TestRFC3339Conversion(ж<Δtesting.T> t) {
    foreach (var (_, f) in range(rfc3339Formats)) {
        if (f.time.Format(RFC3339) != f.formattedValue) {
            t.Error(rfc3339ˢ);
            t.Errorf("  want=%+v", f.formattedValue);
            t.Errorf("  have=%+v", f.time.Format(RFC3339));
        }
    }
}

internal partial struct TestAppendInt_tests /*dyn*/ {
    internal nint @in;
    internal nint width;
    internal @string want;
}

public static void TestAppendInt(ж<Δtesting.T> t) {
    var tests = slice<TestAppendInt_tests>([
        new(0, 0, "0"),
        new(0, 1, "0"),
        new(0, 2, "00"),
        new(0, 3, "000"),
        new(1, 0, "1"),
        new(1, 1, "1"),
        new(1, 2, "01"),
        new(1, 3, "001"),
        new(-1, 0, "-1"),
        new(-1, 1, "-1"),
        new(-1, 2, "-01"),
        new(-1, 3, "-001"),
        new(99, 2, "99"),
        new(100, 2, "100"),
        new(1, 4, "0001"),
        new(12, 4, "0012"),
        new(123, 4, "0123"),
        new(1234, 4, "1234"),
        new(12345, 4, "12345"),
        new(1, 5, "00001"),
        new(12, 5, "00012"),
        new(123, 5, "00123"),
        new(1234, 5, "01234"),
        new(12345, 5, "12345"),
        new(123456, 5, "123456"),
        new(0, 9, "000000000"),
        new(123, 9, "000000123"),
        new(123456, 9, "000123456"),
        new(123456789, 9, "123456789")
    ]);
    slice<byte> got = default;
    foreach (var (_, tt) in range(tests)) {
        got = time_internal_test_package.AppendInt(got[..0], tt.@in, tt.width);
        if (((sstring)got) != tt.want) {
            t.Errorf("appendInt(%d, %d) = %s, want %s", tt.@in, tt.width, got, tt.want);
        }
    }
}

partial struct FormatTest {
    internal @string name;
    internal @string format;
    internal @string result;
}

internal static slice<FormatTest> formatTests = [
    new("ANSIC", ANSIC, "Wed Feb  4 21:00:57 2009"),
    new("UnixDate", UnixDate, "Wed Feb  4 21:00:57 PST 2009"),
    new("RubyDate", RubyDate, "Wed Feb 04 21:00:57 -0800 2009"),
    new("RFC822", RFC822, "04 Feb 09 21:00 PST"),
    new("RFC850", RFC850, "Wednesday, 04-Feb-09 21:00:57 PST"),
    new("RFC1123", RFC1123, "Wed, 04 Feb 2009 21:00:57 PST"),
    new("RFC1123Z", RFC1123Z, "Wed, 04 Feb 2009 21:00:57 -0800"),
    new("RFC3339", RFC3339, "2009-02-04T21:00:57-08:00"),
    new("RFC3339Nano", RFC3339Nano, "2009-02-04T21:00:57.0123456-08:00"),
    new("Kitchen", Kitchen, "9:00PM"),
    new("am/pm", "3pm", "9pm"),
    new("AM/PM", "3PM", "9PM"),
    new("two-digit year", "06 01 02", "09 02 04"),
    // Three-letter months and days must not be followed by lower-case letter.
    new("Janet", "Hi Janet, the Month is January", "Hi Janet, the Month is February"),
    // Time stamps, Fractional seconds.
    new("Stamp", Stamp, "Feb  4 21:00:57"),
    new("StampMilli", StampMilli, "Feb  4 21:00:57.012"),
    new("StampMicro", StampMicro, "Feb  4 21:00:57.012345"),
    new("StampNano", StampNano, "Feb  4 21:00:57.012345600"),
    new("DateTime", DateTime, "2009-02-04 21:00:57"),
    new("DateOnly", DateOnly, "2009-02-04"),
    new("TimeOnly", TimeOnly, "21:00:57"),
    new("YearDay", "Jan  2 002 __2 2", "Feb  4 035  35 4"),
    new("Year", "2006 6 06 _6 __6 ___6", "2009 6 09 _6 __6 ___6"),
    new("Month", "Jan January 1 01 _1", "Feb February 2 02 _2"),
    new("DayOfMonth", "2 02 _2 __2", "4 04  4  35"),
    new("DayOfWeek", "Mon Monday", "Wed Wednesday"),
    new("Hour", "15 3 03 _3", "21 9 09 _9"),
    new("Minute", "4 04 _4", "0 00 _0"),
    new("Second", "5 05 _5", "57 57 _57")
];

public static void TestFormat(ж<Δtesting.T> t) {
    // The numeric time represents Thu Feb  4 21:00:57.012345600 PST 2009
    var time = Unix(0, 1233810057012345600L);
    foreach (var (_, test) in range(formatTests)) {
        @string result = time.Format(test.format);
        if (result != test.result) {
            t.Errorf("%s expected %q got %q", test.name, test.result, result);
        }
    }
}

partial struct goStringTestsᴛ1 /*dyn*/ {
    internal Δtime.Time @in;
    internal @string want;
}
internal static slice<goStringTestsᴛ1> goStringTests = slice<goStringTestsᴛ1>([
    new(Date(2009, February, 5, 5, 0, 57, 12345600, ΔUTC),
        "time.Date(2009, time.February, 5, 5, 0, 57, 12345600, time.UTC)"),
    new(Date(2009, February, 5, 5, 0, 57, 12345600, ΔLocal),
        "time.Date(2009, time.February, 5, 5, 0, 57, 12345600, time.Local)"),
    new(Date(2009, February, 5, 5, 0, 57, 12345600, FixedZone("Europe/Berlin", 3 * 60 * 60)),
        @"time.Date(2009, time.February, 5, 5, 0, 57, 12345600, time.Location(""Europe/Berlin""))"
    ),
    new(Date(2009, February, 5, 5, 0, 57, 12345600, FixedZone("Non-ASCII character ⏰", 3 * 60 * 60)),
        @"time.Date(2009, time.February, 5, 5, 0, 57, 12345600, time.Location(""Non-ASCII character \xe2\x8f\xb0""))"
    )
]);

public static void TestGoString(ж<Δtesting.T> t) {
    // The numeric time represents Thu Feb  4 21:00:57.012345600 PST 2009
    foreach (var (_, tt) in range(goStringTests)) {
        if (tt.@in.GoString() != tt.want) {
            t.Errorf("GoString (%q): got %q want %q", tt.@in, tt.@in.GoString(), tt.want);
        }
    }
}

// issue 12440.
public static void TestFormatSingleDigits(ж<Δtesting.T> t) {
    var time = Date(2001, 2, 3, 4, 5, 6, 700000000, ΔUTC);
    var test = new FormatTest("single digit format", "3:4:5", "4:5:6");
    @string result = time.Format(test.format);
    if (result != test.result) {
        t.Errorf("%s expected %q got %q", test.name, test.result, result);
    }
}

public static void TestFormatShortYear(ж<Δtesting.T> t) {
    var years = slice<nint>([
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
    ]);

    foreach (var (_, y) in range(years)) {
        var time = Date(y, January, 1, 0, 0, 0, 0, ΔUTC);
        @string result = time.Format("2006.01.02");
        @string want = default;
        if (y < 0) {
            // The 4 in %04d counts the - sign, so print -y instead
            // and introduce our own - sign.
            want = fmt.Sprintf("-%04d.%02d.%02d"u8, -y, (nint)1, (nint)1);
        } else {
            want = fmt.Sprintf("%04d.%02d.%02d"u8, y, (nint)1, (nint)1);
        }
        if (result != want) {
            t.Errorf("(jan 1 %d).Format(\"2006.01.02\") = %q, want %q", y, result, want);
        }
    }
}

partial struct ParseTest {
    internal @string name;
    internal @string format;
    internal @string value;
    internal bool hasTZ;      // contains a time zone
    internal bool hasWD;      // contains a weekday
    internal nint yearSign;   // sign of year, -1 indicates the year is not present in the format
    internal nint fracDigits; // number of digits of fractional second
}

internal static slice<ParseTest> parseTests = [
    new("ANSIC", ANSIC, "Thu Feb  4 21:00:57 2010", false, true, 1, 0),
    new("UnixDate", UnixDate, "Thu Feb  4 21:00:57 PST 2010", true, true, 1, 0),
    new("RubyDate", RubyDate, "Thu Feb 04 21:00:57 -0800 2010", true, true, 1, 0),
    new("RFC850", RFC850, "Thursday, 04-Feb-10 21:00:57 PST", true, true, 1, 0),
    new("RFC1123", RFC1123, "Thu, 04 Feb 2010 21:00:57 PST", true, true, 1, 0),
    new("RFC1123", RFC1123, "Thu, 04 Feb 2010 22:00:57 PDT", true, true, 1, 0),
    new("RFC1123Z", RFC1123Z, "Thu, 04 Feb 2010 21:00:57 -0800", true, true, 1, 0),
    new("RFC3339", RFC3339, "2010-02-04T21:00:57-08:00", true, false, 1, 0),
    new("custom: \"2006-01-02 15:04:05-07\"", "2006-01-02 15:04:05-07", "2010-02-04 21:00:57-08", true, false, 1, 0),
    // Optional fractional seconds.
    new("ANSIC", ANSIC, "Thu Feb  4 21:00:57.0 2010", false, true, 1, 1),
    new("UnixDate", UnixDate, "Thu Feb  4 21:00:57.01 PST 2010", true, true, 1, 2),
    new("RubyDate", RubyDate, "Thu Feb 04 21:00:57.012 -0800 2010", true, true, 1, 3),
    new("RFC850", RFC850, "Thursday, 04-Feb-10 21:00:57.0123 PST", true, true, 1, 4),
    new("RFC1123", RFC1123, "Thu, 04 Feb 2010 21:00:57.01234 PST", true, true, 1, 5),
    new("RFC1123Z", RFC1123Z, "Thu, 04 Feb 2010 21:00:57.01234 -0800", true, true, 1, 5),
    new("RFC3339", RFC3339, "2010-02-04T21:00:57.012345678-08:00", true, false, 1, 9),
    new("custom: \"2006-01-02 15:04:05\"", "2006-01-02 15:04:05", "2010-02-04 21:00:57.0", false, false, 1, 0),
    // Amount of white space should not matter.
    new("ANSIC", ANSIC, "Thu Feb 4 21:00:57 2010", false, true, 1, 0),
    new("ANSIC", ANSIC, "Thu      Feb     4     21:00:57     2010", false, true, 1, 0),
    // Case should not matter
    new("ANSIC", ANSIC, "THU FEB 4 21:00:57 2010", false, true, 1, 0),
    new("ANSIC", ANSIC, "thu feb 4 21:00:57 2010", false, true, 1, 0),
    // Fractional seconds.
    new("millisecond:: dot separator", "Mon Jan _2 15:04:05.000 2006", "Thu Feb  4 21:00:57.012 2010", false, true, 1, 3),
    new("microsecond:: dot separator", "Mon Jan _2 15:04:05.000000 2006", "Thu Feb  4 21:00:57.012345 2010", false, true, 1, 6),
    new("nanosecond:: dot separator", "Mon Jan _2 15:04:05.000000000 2006", "Thu Feb  4 21:00:57.012345678 2010", false, true, 1, 9),
    new("millisecond:: comma separator", "Mon Jan _2 15:04:05,000 2006", "Thu Feb  4 21:00:57.012 2010", false, true, 1, 3),
    new("microsecond:: comma separator", "Mon Jan _2 15:04:05,000000 2006", "Thu Feb  4 21:00:57.012345 2010", false, true, 1, 6),
    new("nanosecond:: comma separator", "Mon Jan _2 15:04:05,000000000 2006", "Thu Feb  4 21:00:57.012345678 2010", false, true, 1, 9),

    // Leading zeros in other places should not be taken as fractional seconds.
    new("zero1", "2006.01.02.15.04.05.0", "2010.02.04.21.00.57.0", false, false, 1, 1),
    new("zero2", "2006.01.02.15.04.05.00", "2010.02.04.21.00.57.01", false, false, 1, 2),
    // Month and day names only match when not followed by a lower-case letter.
    new("Janet", "Hi Janet, the Month is January: Jan _2 15:04:05 2006", "Hi Janet, the Month is February: Feb  4 21:00:57 2010", false, true, 1, 0),

    // GMT with offset.
    new("GMT-8", UnixDate, "Fri Feb  5 05:00:57 GMT-8 2010", true, true, 1, 0),

    // Accept any number of fractional second digits (including none) for .999...
    // In Go 1, .999... was completely ignored in the format, meaning the first two
    // cases would succeed, but the next four would not. Go 1.1 accepts all six.
    // decimal "." separator.
    new("", "2006-01-02 15:04:05.9999 -0700 MST", "2010-02-04 21:00:57 -0800 PST", true, false, 1, 0),
    new("", "2006-01-02 15:04:05.999999999 -0700 MST", "2010-02-04 21:00:57 -0800 PST", true, false, 1, 0),
    new("", "2006-01-02 15:04:05.9999 -0700 MST", "2010-02-04 21:00:57.0123 -0800 PST", true, false, 1, 4),
    new("", "2006-01-02 15:04:05.999999999 -0700 MST", "2010-02-04 21:00:57.0123 -0800 PST", true, false, 1, 4),
    new("", "2006-01-02 15:04:05.9999 -0700 MST", "2010-02-04 21:00:57.012345678 -0800 PST", true, false, 1, 9),
    new("", "2006-01-02 15:04:05.999999999 -0700 MST", "2010-02-04 21:00:57.012345678 -0800 PST", true, false, 1, 9),
    // comma "," separator.
    new("", "2006-01-02 15:04:05,9999 -0700 MST", "2010-02-04 21:00:57 -0800 PST", true, false, 1, 0),
    new("", "2006-01-02 15:04:05,999999999 -0700 MST", "2010-02-04 21:00:57 -0800 PST", true, false, 1, 0),
    new("", "2006-01-02 15:04:05,9999 -0700 MST", "2010-02-04 21:00:57.0123 -0800 PST", true, false, 1, 4),
    new("", "2006-01-02 15:04:05,999999999 -0700 MST", "2010-02-04 21:00:57.0123 -0800 PST", true, false, 1, 4),
    new("", "2006-01-02 15:04:05,9999 -0700 MST", "2010-02-04 21:00:57.012345678 -0800 PST", true, false, 1, 9),
    new("", "2006-01-02 15:04:05,999999999 -0700 MST", "2010-02-04 21:00:57.012345678 -0800 PST", true, false, 1, 9),

    // issue 4502.
    new("", StampNano, "Feb  4 21:00:57.012345678", false, false, -1, 9),
    new("", "Jan _2 15:04:05.999", "Feb  4 21:00:57.012300000", false, false, -1, 4),
    new("", "Jan _2 15:04:05.999", "Feb  4 21:00:57.012345678", false, false, -1, 9),
    new("", "Jan _2 15:04:05.999999999", "Feb  4 21:00:57.0123", false, false, -1, 4),
    new("", "Jan _2 15:04:05.999999999", "Feb  4 21:00:57.012345678", false, false, -1, 9),

    // Day of year.
    new("", "2006-01-02 002 15:04:05", "2010-02-04 035 21:00:57", false, false, 1, 0),
    new("", "2006-01 002 15:04:05", "2010-02 035 21:00:57", false, false, 1, 0),
    new("", "2006-002 15:04:05", "2010-035 21:00:57", false, false, 1, 0),
    new("", "200600201 15:04:05", "201003502 21:00:57", false, false, 1, 0),
    new("", "200600204 15:04:05", "201003504 21:00:57", false, false, 1, 0),

    // Time zone offsets
    new("", "2006-01-02T15:04:05Z07", "2010-02-04T21:00:57Z", false, false, 1, 0),
    new("", "2006-01-02T15:04:05Z07", "2010-02-04T21:00:57+08", false, false, 1, 0),
    new("", "2006-01-02T15:04:05Z07", "2010-02-04T21:00:57-08", true, false, 1, 0),
    new("", "2006-01-02T15:04:05Z0700", "2010-02-04T21:00:57Z", false, false, 1, 0),
    new("", "2006-01-02T15:04:05Z0700", "2010-02-04T21:00:57+0800", false, false, 1, 0),
    new("", "2006-01-02T15:04:05Z0700", "2010-02-04T21:00:57-0800", true, false, 1, 0),
    new("", "2006-01-02T15:04:05Z07:00", "2010-02-04T21:00:57Z", false, false, 1, 0),
    new("", "2006-01-02T15:04:05Z07:00", "2010-02-04T21:00:57+08:00", false, false, 1, 0),
    new("", "2006-01-02T15:04:05Z07:00", "2010-02-04T21:00:57-08:00", true, false, 1, 0),
    new("", "2006-01-02T15:04:05Z070000", "2010-02-04T21:00:57Z", false, false, 1, 0),
    new("", "2006-01-02T15:04:05Z070000", "2010-02-04T21:00:57+080000", false, false, 1, 0),
    new("", "2006-01-02T15:04:05Z070000", "2010-02-04T21:00:57-080000", true, false, 1, 0),
    new("", "2006-01-02T15:04:05Z07:00:00", "2010-02-04T21:00:57Z", false, false, 1, 0),
    new("", "2006-01-02T15:04:05Z07:00:00", "2010-02-04T21:00:57+08:00:00", false, false, 1, 0),
    new("", "2006-01-02T15:04:05Z07:00:00", "2010-02-04T21:00:57-08:00:00", true, false, 1, 0)
];

public static void TestParse(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(parseTests)) {
        ref var test = ref heap(new ParseTest(), out var Ꮡtest);
        test = vᴛ1;

        var (time, err) = Parse(test.format, test.value);
        if (err != nil) {
            t.Errorf("%s error: %v", test.name, err);
        } else {
            checkTime(time, Ꮡtest, t);
        }
    }
}

// All parsed with ANSIC.
partial struct dayOutOfRangeTestsᴛ1 /*dyn*/ {
    internal @string date;
    internal bool ok;
}
internal static slice<dayOutOfRangeTestsᴛ1> dayOutOfRangeTests = [
    new("Thu Jan 99 21:00:57 2010", false),
    new("Thu Jan 31 21:00:57 2010", true),
    new("Thu Jan 32 21:00:57 2010", false),
    new("Thu Feb 28 21:00:57 2012", true),
    new("Thu Feb 29 21:00:57 2012", true),
    new("Thu Feb 29 21:00:57 2010", false),
    new("Thu Mar 31 21:00:57 2010", true),
    new("Thu Mar 32 21:00:57 2010", false),
    new("Thu Apr 30 21:00:57 2010", true),
    new("Thu Apr 31 21:00:57 2010", false),
    new("Thu May 31 21:00:57 2010", true),
    new("Thu May 32 21:00:57 2010", false),
    new("Thu Jun 30 21:00:57 2010", true),
    new("Thu Jun 31 21:00:57 2010", false),
    new("Thu Jul 31 21:00:57 2010", true),
    new("Thu Jul 32 21:00:57 2010", false),
    new("Thu Aug 31 21:00:57 2010", true),
    new("Thu Aug 32 21:00:57 2010", false),
    new("Thu Sep 30 21:00:57 2010", true),
    new("Thu Sep 31 21:00:57 2010", false),
    new("Thu Oct 31 21:00:57 2010", true),
    new("Thu Oct 32 21:00:57 2010", false),
    new("Thu Nov 30 21:00:57 2010", true),
    new("Thu Nov 31 21:00:57 2010", false),
    new("Thu Dec 31 21:00:57 2010", true),
    new("Thu Dec 32 21:00:57 2010", false),
    new("Thu Dec 00 21:00:57 2010", false)
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dayOutOfRangeˢ => ref dayOutOfRangeˢᵇ;
private static readonly @string dayOutOfRangeˢᵇ = "day out of range"u8;

public static void TestParseDayOutOfRange(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(dayOutOfRangeTests)) {
        var (_, err) = Parse(ANSIC, test.date);
        switch (ᐧ) {
        case {} when test.ok && err == nil: {
            // OK
            break;
        }
        case {} when !test.ok && err != nil: {
            if (!strings.Contains(err.Error(), dayOutOfRangeˢ)) {
                t.Errorf("%q: expected 'day' error, got %v", test.date, err);
            }
            break;
        }
        case {} when test.ok && err != nil: {
            t.Errorf("%q: unexpected error: %v", test.date, err);
            break;
        }
        case {} when !test.ok && err == nil: {
            t.Errorf("%q: expected 'day' error, got none", test.date);
            break;
        }}
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string asiaBaghdadˢ => ref asiaBaghdadˢᵇ;
private static readonly @string asiaBaghdadˢᵇ = "Asia/Baghdad"u8;
internal static ref readonly @string jan022006Mstˢ => ref jan022006Mstˢᵇ;
private static readonly @string jan022006Mstˢᵇ = "Jan 02 2006 MST"u8;
internal static ref readonly @string feb012013Astˢ => ref feb012013Astˢᵇ;
private static readonly @string feb012013Astˢᵇ = "Feb 01 2013 AST"u8;
internal static ref readonly @string americaBlancSablonˢ => ref americaBlancSablonˢᵇ;
private static readonly @string americaBlancSablonˢᵇ = "America/Blanc-Sablon"u8;

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
public static void TestParseInLocation(ж<Δtesting.T> t) {
    var (baghdad, err) = LoadLocation(asiaBaghdadˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    Δtime.Time t1 = default;
    Δtime.Time t2 = default;

    (t1, err) = ParseInLocation(jan022006Mstˢ, feb012013Astˢ, baghdad);
    if (err != nil) {
        t.Fatal(err);
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
            t.Fatalf("ParseInLocation(Feb 01 2013 AST, Baghdad) = %v, want %v", t1, t2);
        }
        if (offset != 3 * 60 * 60) {
            t.Fatalf("ParseInLocation(Feb 01 2013 AST, Baghdad).Zone = _, %d, want _, %d", offset, (nint)(3 * 60 * 60));
        }
    }

    (var blancSablon, err) = LoadLocation(americaBlancSablonˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    // In this case 'AST' means 'Atlantic Standard Time', and we
    // expect the abbreviation to correctly match the american
    // location.
    (t1, err) = ParseInLocation(jan022006Mstˢ, feb012013Astˢ, blancSablon);
    if (err != nil) {
        t.Fatal(err);
    }
    t2 = Date(2013, February, 1, 0, 0, 0, 0, blancSablon);
    if (t1 != t2) {
        t.Fatalf("ParseInLocation(Feb 01 2013 AST, Blanc-Sablon) = %v, want %v", t1, t2);
    }
    (_, offset) = t1.Zone();
    if (offset != -4 * 60 * 60) {
        t.Fatalf("ParseInLocation(Feb 01 2013 AST, Blanc-Sablon).Zone = _, %d, want _, %d", offset, (nint)(-4 * 60 * 60));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string australiaSydneyˢ => ref australiaSydneyˢᵇ;
private static readonly @string australiaSydneyˢᵇ = "Australia/Sydney"u8;

public static void TestLoadLocationZipFile(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);

        var (_, err) = LoadLocation(australiaSydneyˢ);
        if (err != nil) {
            t.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<ParseTest> rubyTests = [
    new("RubyDate", RubyDate, "Thu Feb 04 21:00:57 -0800 2010", true, true, 1, 0),
    // Ignore the time zone in the test. If it parses, it'll be OK.
    new("RubyDate", RubyDate, "Thu Feb 04 21:00:57 -0000 2010", false, true, 1, 0),
    new("RubyDate", RubyDate, "Thu Feb 04 21:00:57 +0000 2010", false, true, 1, 0),
    new("RubyDate", RubyDate, "Thu Feb 04 21:00:57 +1130 2010", false, true, 1, 0)
];

// Problematic time zone format needs special tests.
public static void TestRubyParse(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(rubyTests)) {
        ref var test = ref heap(new ParseTest(), out var Ꮡtest);
        test = vᴛ1;

        var (time, err) = Parse(test.format, test.value);
        if (err != nil) {
            t.Errorf("%s error: %v", test.name, err);
        } else {
            checkTime(time, Ꮡtest, t);
        }
    }
}

internal static void checkTime(Δtime.Time time, ж<ParseTest> Ꮡtest, ж<Δtesting.T> t) {
    ref var test = ref Ꮡtest.DerefOrNull();

    // The time should be Thu Feb  4 21:00:57 PST 2010
    if (test.yearSign >= 0 && test.yearSign * time.Year() != 2010) {
        t.Errorf("%s: bad year: %d not %d", test.name, time.Year(), (nint)2010);
    }
    if (time.Month() != February) {
        t.Errorf("%s: bad month: %s not %s", test.name, time.Month(), February);
    }
    if (time.Day() != 4) {
        t.Errorf("%s: bad day: %d not %d", test.name, time.Day(), (nint)4);
    }
    if (time.Hour() != 21) {
        t.Errorf("%s: bad hour: %d not %d", test.name, time.Hour(), (nint)21);
    }
    if (time.Minute() != 0) {
        t.Errorf("%s: bad minute: %d not %d", test.name, time.Minute(), (nint)0);
    }
    if (time.Second() != 57) {
        t.Errorf("%s: bad second: %d not %d", test.name, time.Second(), (nint)57);
    }
    // Nanoseconds must be checked against the precision of the input.
    var (nanosec, err) = strconv.ParseUint(((@string)"012345678"u8.slice(0, test.fracDigits)) + "000000000"u8.slice(0, 9 - test.fracDigits), 10, 0);
    if (err != nil) {
        throw panic(err);
    }
    if (time.Nanosecond() != (nint)nanosec) {
        t.Errorf("%s: bad nanosecond: %d not %d", test.name, time.Nanosecond(), nanosec);
    }
    var (name, offset) = time.Zone();
    if (test.hasTZ && offset != -28800) {
        t.Errorf("%s: bad tz offset: %s %d not %d", test.name, name, offset, (nint)(-28800));
    }
    if (test.hasWD && time.Weekday() != Thursday) {
        t.Errorf("%s: bad weekday: %s not %s", test.name, time.Weekday(), Thursday);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string fmtᶜ => ref fmtᶜᵇ;
private static readonly @string fmtᶜᵇ = "Mon MST 2006-01-02T15:04:05Z07:00";

public static void TestFormatAndParse(ж<Δtesting.T> t) {
    @string fmt = fmtᶜ; // all fields
    var f = (int64 sec) => {
        var t1 = Unix(sec / 2, 0);
        if (t1.Year() < 1000 || t1.Year() > 9999 || t1.Unix() != sec) {
            // not required to work
            return true;
        }
        var (t2, err) = Parse(fmt, t1.Format(fmt));
        if (err != nil) {
            t.Errorf("error: %s", err);
            return false;
        }
        if (t1.Unix() != t2.Unix() || t1.Nanosecond() != t2.Nanosecond()) {
            t.Errorf("FormatAndParse %d: %q(%d) %q(%d)", sec, t1, t1.Unix(), t2, t2.Unix());
            return false;
        }
        return true;
    };
    var fʗ1 = f;
    var f32 = (int32 sec) => fʗ1((int64)sec);
    var cfg = Ꮡ(new quick.Config(MaxCount: 10000));

    // Try a reasonable date first, then the huge ones.
    {
        var err = quick.Check((f32).OrTypedNilFunc(), cfg); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = quick.Check((f).OrTypedNilFunc(), cfg); if (err != nil) {
            t.Fatal(err);
        }
    }
}

partial struct ParseTimeZoneTest {
    internal @string value;
    internal nint length;
    internal bool ok;
}

internal static slice<ParseTimeZoneTest> parseTimeZoneTests = [
    new("gmt hi there", 0, false),
    new("GMT hi there", 3, true),
    new("GMT+12 hi there", 6, true),
    new("GMT+00 hi there", 6, true),
    new("GMT+", 3, true),
    new("GMT+3", 5, true),
    new("GMT+a", 3, true),
    new("GMT+3a", 5, true),
    new("GMT-5 hi there", 5, true),
    new("GMT-51 hi there", 3, true),
    new("ChST hi there", 4, true),
    new("MeST hi there", 4, true),
    new("MSDx", 3, true),
    new("MSDY", 0, false), // four letters must end in T.
    new("ESAST hi", 5, true),
    new("ESASTT hi", 0, false), // run of upper-case letters too long.
    new("ESATY hi", 0, false),  // five letters must end in T.
    new("WITA hi", 4, true),    // Issue #18251
    // Issue #24071
    new("+03 hi", 3, true),
    new("-04 hi", 3, true),
    // Issue #26032
    new("+00", 3, true),
    new("-11", 3, true),
    new("-12", 3, true),
    new("-23", 3, true),
    new("-24", 0, false),
    new("+13", 3, true),
    new("+14", 3, true),
    new("+23", 3, true),
    new("+24", 0, false)
];

public static void TestParseTimeZone(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(parseTimeZoneTests)) {
        var (length, ok) = time_internal_test_package.ParseTimeZone(test.value);
        if (ok != test.ok) {
            t.Errorf("expected %t for %q got %t", test.ok, test.value, ok);
        } else if (length != test.length) {
            t.Errorf("expected %d for %q got %d", test.length, test.value, length);
        }
    }
}

partial struct ParseErrorTest {
    internal @string format;
    internal @string value;
    internal @string expect; // must appear within the error
}

internal static slice<ParseErrorTest> parseErrorTests = [
    new(ANSIC, "Feb  4 21:00:60 2010", @"cannot parse ""Feb  4 21:00:60 2010"" as ""Mon"""),
    new(ANSIC, "Thu Feb  4 21:00:57 @2010", @"cannot parse ""@2010"" as ""2006"""),
    new(ANSIC, "Thu Feb  4 21:00:60 2010", "second out of range"),
    new(ANSIC, "Thu Feb  4 21:61:57 2010", "minute out of range"),
    new(ANSIC, "Thu Feb  4 24:00:60 2010", "hour out of range"),
    new("Mon Jan _2 15:04:05.000 2006", "Thu Feb  4 23:00:59x01 2010", @"cannot parse ""x01 2010"" as "".000"""),
    new("Mon Jan _2 15:04:05.000 2006", "Thu Feb  4 23:00:59.xxx 2010", @"cannot parse "".xxx 2010"" as "".000"""),
    new("Mon Jan _2 15:04:05.000 2006", "Thu Feb  4 23:00:59.-123 2010", "fractional second out of range"),
    // issue 4502. StampNano requires exactly 9 digits of precision.
    new(StampNano, "Dec  7 11:22:01.000000", @"cannot parse "".000000"" as "".000000000"""),
    new(StampNano, "Dec  7 11:22:01.0000000000", @"extra text: ""0"""),
    // issue 4493. Helpful errors.
    new(RFC3339, "2006-01-02T15:04:05Z07:00", @"parsing time ""2006-01-02T15:04:05Z07:00"": extra text: ""07:00"""),
    new(RFC3339, "2006-01-02T15:04_abc", @"parsing time ""2006-01-02T15:04_abc"" as ""2006-01-02T15:04:05Z07:00"": cannot parse ""_abc"" as "":"""),
    new(RFC3339, "2006-01-02T15:04:05_abc", @"parsing time ""2006-01-02T15:04:05_abc"" as ""2006-01-02T15:04:05Z07:00"": cannot parse ""_abc"" as ""Z07:00"""),
    new(RFC3339, "2006-01-02T15:04:05Z_abc", @"parsing time ""2006-01-02T15:04:05Z_abc"": extra text: ""_abc"""),
    // invalid second followed by optional fractional seconds
    new(RFC3339, "2010-02-04T21:00:67.012345678-08:00", "second out of range"),
    // issue 54569
    new(RFC3339, "0000-01-01T00:00:.0+00:00", @"parsing time ""0000-01-01T00:00:.0+00:00"" as ""2006-01-02T15:04:05Z07:00"": cannot parse "".0+00:00"" as ""05"""),
    // issue 21113
    new("_2 Jan 06 15:04 MST", "4 --- 00 00:00 GMT", @"cannot parse ""--- 00 00:00 GMT"" as ""Jan"""),
    new("_2 January 06 15:04 MST", "4 --- 00 00:00 GMT", @"cannot parse ""--- 00 00:00 GMT"" as ""January"""),

    // invalid or mismatched day-of-year
    new("Jan _2 002 2006", "Feb  4 034 2006", "day-of-year does not match day"),
    new("Jan _2 002 2006", "Feb  4 004 2006", "day-of-year does not match month"),

    // issue 45391.
    new(@"""2006-01-02T15:04:05Z07:00""", "0", @"parsing time ""0"" as ""\""2006-01-02T15:04:05Z07:00\"""": cannot parse ""0"" as ""\"""""),
    new(RFC3339, "\"", @"parsing time ""\"""" as ""2006-01-02T15:04:05Z07:00"": cannot parse ""\"""" as ""2006"""),

    // issue 54570
    new(RFC3339, "0000-01-01T00:00:00+00:+0", @"parsing time ""0000-01-01T00:00:00+00:+0"" as ""2006-01-02T15:04:05Z07:00"": cannot parse ""+00:+0"" as ""Z07:00"""),
    new(RFC3339, "0000-01-01T00:00:00+-0:00", @"parsing time ""0000-01-01T00:00:00+-0:00"" as ""2006-01-02T15:04:05Z07:00"": cannot parse ""+-0:00"" as ""Z07:00"""),

    // issue 56730
    new("2006-01-02", "22-10-25", @"parsing time ""22-10-25"" as ""2006-01-02"": cannot parse ""22-10-25"" as ""2006"""),
    new("06-01-02", "a2-10-25", @"parsing time ""a2-10-25"" as ""06-01-02"": cannot parse ""a2-10-25"" as ""06"""),
    new("03:04PM", "12:03pM", @"parsing time ""12:03pM"" as ""03:04PM"": cannot parse ""pM"" as ""PM"""),
    new("03:04pm", "12:03pM", @"parsing time ""12:03pM"" as ""03:04pm"": cannot parse ""pM"" as ""pm"""),

    // issue 67470
    new("-07", "-25", "time zone offset hour out of range"),
    new("-07:00", "+25:00", "time zone offset hour out of range"),
    new("-07:00", "-23:61", "time zone offset minute out of range"),
    new("-07:00:00", "+23:59:61", "time zone offset second out of range"),
    new("Z07", "-25", "time zone offset hour out of range"),
    new("Z07:00", "+25:00", "time zone offset hour out of range"),
    new("Z07:00", "-23:61", "time zone offset minute out of range"),
    new("Z07:00:00", "+23:59:61", "time zone offset second out of range")
];

public static void TestParseErrors(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(parseErrorTests)) {
        var (_, err) = Parse(test.format, test.value);
        if (err == nil) {
            t.Errorf("expected error for %q %q", test.format, test.value);
        } else if (!strings.Contains(err.Error(), test.expect)) {
            t.Errorf("expected error with %q for %q %q; got %s", test.expect, test.format, test.value, err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectᶜ => ref expectᶜᵇ;
private static readonly @string expectᶜᵇ = "12:00PM"u8;

public static void TestNoonIs12PM(ж<Δtesting.T> t) {
    var noon = Date(0, January, 1, 12, 0, 0, 0, ΔUTC);
    @string expect = expectᶜ;
    @string got = noon.Format("3:04PM");
    if (got != expect) {
        t.Errorf("got %q; expect %q", got, expect);
    }
    got = noon.Format("03:04PM");
    if (got != expect) {
        t.Errorf("got %q; expect %q", got, expect);
    }
}

public static void TestMidnightIs12AM(ж<Δtesting.T> t) {
    var midnight = Date(0, January, 1, 0, 0, 0, 0, ΔUTC);
    @string expect = "12:00AM";
    @string got = midnight.Format("3:04PM");
    if (got != expect) {
        t.Errorf("got %q; expect %q", got, expect);
    }
    got = midnight.Format("03:04PM");
    if (got != expect) {
        t.Errorf("got %q; expect %q", got, expect);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorParsingDateˢ = (@string)"error parsing date:"u8;

public static void Test12PMIsNoon(ж<Δtesting.T> t) {
    var (noon, err) = Parse("3:04PM", "12:00PM");
    if (err != nil) {
        t.Fatal(errorParsingDateˢ, err);
    }
    if (noon.Hour() != 12) {
        t.Errorf("got %d; expect 12", noon.Hour());
    }
    (noon, err) = Parse("03:04PM", "12:00PM");
    if (err != nil) {
        t.Fatal(errorParsingDateˢ, err);
    }
    if (noon.Hour() != 12) {
        t.Errorf("got %d; expect 12", noon.Hour());
    }
}

public static void Test12AMIsMidnight(ж<Δtesting.T> t) {
    var (midnight, err) = Parse("3:04PM", "12:00AM");
    if (err != nil) {
        t.Fatal(errorParsingDateˢ, err);
    }
    if (midnight.Hour() != 0) {
        t.Errorf("got %d; expect 0", midnight.Hour());
    }
    (midnight, err) = Parse("03:04PM", "12:00AM");
    if (err != nil) {
        t.Fatal(errorParsingDateˢ, err);
    }
    if (midnight.Hour() != 0) {
        t.Errorf("got %d; expect 0", midnight.Hour());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string thuFeb0216100305002006ˢ => ref thuFeb0216100305002006ˢᵇ;
private static readonly @string thuFeb0216100305002006ˢᵇ = "Thu Feb 02 16:10:03 -0500 2006"u8;
internal static ref readonly @string thuFeb216100305002006ˢ => ref thuFeb216100305002006ˢᵇ;
private static readonly @string thuFeb216100305002006ˢᵇ = "Thu Feb  2 16:10:03 -0500 2006"u8;

// Check that a time without a Zone still produces a (numeric) time zone
// when formatted with MST as a requested zone.
public static void TestMissingZone(ж<Δtesting.T> t) {
    var (time, err) = Parse(RubyDate, thuFeb0216100305002006ˢ);
    if (err != nil) {
        t.Fatal(errorParsingDateˢ, err);
    }
    @string expect = thuFeb216100305002006ˢ; // -0500 not EST
    @string str = time.Format(UnixDate);     // uses MST as its time zone
    if (str != expect) {
        t.Errorf("got %s; expect %s", str, expect);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string monJan0215040501232006ˢ => ref monJan0215040501232006ˢᵇ;
private static readonly @string monJan0215040501232006ˢᵇ = "Mon Jan 02 15:04:05 +0123 2006"u8;

public static void TestMinutesInTimeZone(ж<Δtesting.T> t) {
    var (time, err) = Parse(RubyDate, monJan0215040501232006ˢ);
    if (err != nil) {
        t.Fatal(errorParsingDateˢ, err);
    }
    nint expected = (1 * 60 + 23) * 60;
    var (_, offset) = time.Zone();
    if (offset != expected) {
        t.Errorf("ZoneOffset = %d, want %d", offset, expected);
    }
}

partial struct SecondsTimeZoneOffsetTest {
    internal @string format;
    internal @string value;
    internal nint expectedoffset;
}

internal static slice<SecondsTimeZoneOffsetTest> secondsTimeZoneOffsetTests = [
    new("2006-01-02T15:04:05-070000", "1871-01-01T05:33:02-003408", -(34 * 60 + 8)),
    new("2006-01-02T15:04:05-07:00:00", "1871-01-01T05:33:02-00:34:08", -(34 * 60 + 8)),
    new("2006-01-02T15:04:05-070000", "1871-01-01T05:33:02+003408", 34 * 60 + 8),
    new("2006-01-02T15:04:05-07:00:00", "1871-01-01T05:33:02+00:34:08", 34 * 60 + 8),
    new("2006-01-02T15:04:05Z070000", "1871-01-01T05:33:02-003408", -(34 * 60 + 8)),
    new("2006-01-02T15:04:05Z07:00:00", "1871-01-01T05:33:02+00:34:08", 34 * 60 + 8),
    new("2006-01-02T15:04:05-07", "1871-01-01T05:33:02+01", 1 * 60 * 60),
    new("2006-01-02T15:04:05-07", "1871-01-01T05:33:02-02", -2 * 60 * 60),
    new("2006-01-02T15:04:05Z07", "1871-01-01T05:33:02-02", -2 * 60 * 60)
];

public static void TestParseSecondsInTimeZone(ж<Δtesting.T> t) {
    // should accept timezone offsets with seconds like: Zone America/New_York   -4:56:02 -      LMT     1883 Nov 18 12:03:58
    foreach (var (_, test) in range(secondsTimeZoneOffsetTests)) {
        var (time, err) = Parse(test.format, test.value);
        if (err != nil) {
            t.Fatal(errorParsingDateˢ, err);
        }
        var (_, offset) = time.Zone();
        if (offset != test.expectedoffset) {
            t.Errorf("ZoneOffset = %d, want %d", offset, test.expectedoffset);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lmtˢ => ref lmtˢᵇ;
private static readonly @string lmtˢᵇ = "LMT"u8;

public static void TestFormatSecondsInTimeZone(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(secondsTimeZoneOffsetTests)) {
        var d = Date(1871, 1, 1, 5, 33, 2, 0, FixedZone(lmtˢ, test.expectedoffset));
        @string timestr = d.Format(test.format);
        if (timestr != test.value) {
            t.Errorf("Format = %s, want %s", timestr, test.value);
        }
    }
}

// Issue 11334.
public static void TestUnderscoreTwoThousand(ж<Δtesting.T> t) {
    @string format = "15:04_20060102";
    @string input = "14:38_20150618";
    var (time, err) = Parse(format, input);
    if (err != nil) {
        t.Error(err);
    }
    {
        var (y, m, d) = time.Date(); if (y != 2015 || m != 6 || d != 18) {
            t.Errorf("Incorrect y/m/d, got %d/%d/%d", y, m, d);
        }
    }
    nint h = time.Hour(); if (h != 14) {
        t.Errorf("Incorrect hour, got %d", h);
    }
    {
        nint m = time.Minute(); if (m != 38) {
            t.Errorf("Incorrect minute, got %d", m);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cannotParseˢ => ref cannotParseˢᵇ;
private static readonly @string cannotParseˢᵇ = "cannot parse"u8;

internal partial struct TestStd0xParseError_tests /*dyn*/ {
    internal @string format, value, valueElemPrefix;
}

// Issue 29918, 29916
public static void TestStd0xParseError(ж<Δtesting.T> t) {
    var tests = slice<TestStd0xParseError_tests>([
        new("01 MST", "0 MST", "0"),
        new("01 MST", "1 MST", "1"),
        new(RFC850, "Thursday, 04-Feb-1 21:00:57 PST", "1")
    ]);
    foreach (var (_, tt) in range(tests)) {
        var (_, err) = Parse(tt.format, tt.value);
        if (err == nil) {
            t.Errorf("Parse(%q, %q) did not fail as expected", tt.format, tt.value);
        } else if (!err._<ж<Δtime.ParseError>>(out var perr)) {
            t.Errorf("Parse(%q, %q) returned error type %T, expected ParseError", tt.format, tt.value, perr.OrTypedNil());
        } else if (!strings.Contains(perr.Error(), cannotParseˢ) || !strings.HasPrefix(perr.ValueElem, tt.valueElemPrefix)) {
            t.Errorf("Parse(%q, %q) returned wrong parsing error message: %v", tt.format, tt.value, perr.OrTypedNil());
        }
    }
}

partial struct monthOutOfRangeTestsᴛ1 /*dyn*/ {
    internal @string value;
    internal bool ok;
}
internal static slice<monthOutOfRangeTestsᴛ1> monthOutOfRangeTests = [
    new("00-01", false),
    new("13-01", false),
    new("01-01", true)
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string monthOutOfRangeˢ => ref monthOutOfRangeˢᵇ;
private static readonly @string monthOutOfRangeˢᵇ = "month out of range"u8;

public static void TestParseMonthOutOfRange(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(monthOutOfRangeTests)) {
        var (_, err) = Parse("01-02", test.value);
        switch (ᐧ) {
        case {} when !test.ok && err != nil: {
            if (!strings.Contains(err.Error(), monthOutOfRangeˢ)) {
                t.Errorf("%q: expected 'month' error, got %v", test.value, err);
            }
            break;
        }
        case {} when test.ok && err != nil: {
            t.Errorf("%q: unexpected error: %v", test.value, err);
            break;
        }
        case {} when !test.ok && err == nil: {
            t.Errorf("%q: expected 'month' error, got none", test.value);
            break;
        }}
    }
}

// Issue 37387.
public static void TestParseYday(ж<Δtesting.T> t) {
    t.Parallel();
    for (nint i = 1; i <= 365; i++) {
        @string d = fmt.Sprintf("2020-%03d"u8, i);
        var (tm, err) = Parse("2006-002", d);
        if (err != nil) {
            t.Errorf("unexpected error for %s: %v", d, err);
        } else if (tm.Year() != 2020 || tm.YearDay() != i) {
            t.Errorf("got year %d yearday %d, want %d %d", tm.Year(), tm.YearDay(), (nint)2020, i);
        }
    }
}

internal partial struct TestQuote_tests /*dyn*/ {
    internal @string s, want;
}

// Issue 45391.
public static void TestQuote(ж<Δtesting.T> t) {
    var tests = slice<TestQuote_tests>([
        new(@"""", @"""\"""""),
        new(@"abc""xyz""", @"""abc\""xyz\"""""),
        new("", @""""""),
        new("abc", @"""abc"""),
        new(@"☺", @"""\xe2\x98\xba"""),
        new(@"☺ hello ☺ hello", @"""\xe2\x98\xba hello \xe2\x98\xba hello"""),
        new("\x04", @"""\x04""")
    ]);
    foreach (var (_, tt) in range(tests)) {
        @string q = time_internal_test_package.Quote(tt.s); if (q != tt.want) {
            t.Errorf("Quote(%q) = got %q, want %q", tt.s, q, tt.want);
        }
    }
}

internal partial struct TestFormatFractionalSecondSeparators_tests /*dyn*/ {
    internal @string s, want;
}

// Issue 48037
public static void TestFormatFractionalSecondSeparators(ж<Δtesting.T> t) {
    var tests = slice<TestFormatFractionalSecondSeparators_tests>([
        new(@"15:04:05.000", @"21:00:57.012"),
        new(@"15:04:05.999", @"21:00:57.012"),
        new(@"15:04:05,000", @"21:00:57,012"),
        new(@"15:04:05,999", @"21:00:57,012")
    ]);

    // The numeric time represents Thu Feb  4 21:00:57.012345600 PST 2009
    var time = Unix(0, 1233810057012345600L);
    foreach (var (_, tt) in range(tests)) {
        @string q = time.Format(tt.s); if (q != tt.want) {
            t.Errorf("Format(%q) = got %q, want %q", tt.s, q, tt.want);
        }
    }
}

partial struct longFractionalDigitsTestsᴛ1 /*dyn*/ {
    internal @string value;
    internal nint want;
}
internal static slice<longFractionalDigitsTestsᴛ1> longFractionalDigitsTests = [
    // 9 digits
    new("2021-09-29T16:04:33.000000000Z", 0),
    new("2021-09-29T16:04:33.000000001Z", 1),
    new("2021-09-29T16:04:33.100000000Z", 100_000_000),
    new("2021-09-29T16:04:33.100000001Z", 100_000_001),
    new("2021-09-29T16:04:33.999999999Z", 999_999_999),
    new("2021-09-29T16:04:33.012345678Z", 12_345_678),
    // 10 digits, truncates
    new("2021-09-29T16:04:33.0000000000Z", 0),
    new("2021-09-29T16:04:33.0000000001Z", 0),
    new("2021-09-29T16:04:33.1000000000Z", 100_000_000),
    new("2021-09-29T16:04:33.1000000009Z", 100_000_000),
    new("2021-09-29T16:04:33.9999999999Z", 999_999_999),
    new("2021-09-29T16:04:33.0123456789Z", 12_345_678),
    // 11 digits, truncates
    new("2021-09-29T16:04:33.10000000000Z", 100_000_000),
    new("2021-09-29T16:04:33.00123456789Z", 1_234_567),
    // 12 digits, truncates
    new("2021-09-29T16:04:33.000123456789Z", 123_456),
    // 15 digits, truncates
    new("2021-09-29T16:04:33.9999999999999999Z", 999_999_999)
];

// Issue 48685 and 54567.
public static void TestParseFractionalSecondsLongerThanNineDigits(ж<Δtesting.T> t) {
    foreach (var (_, tt) in range(longFractionalDigitsTests)) {
        foreach (var (_, format) in range(slice<@string>([RFC3339, RFC3339Nano]))) {
            var (tm, err) = Parse(format, tt.value);
            if (err != nil) {
                t.Errorf("Parse(%q, %q) error: %v", format, tt.value, err);
                continue;
            }
            nint got = tm.Nanosecond(); if (got != tt.want) {
                t.Errorf("Parse(%q, %q) = got %d, want %d", format, tt.value, got, tt.want);
            }
        }
    }
}

public static void FuzzFormatRFC3339(ж<Δtesting.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, vᴛ1) in range(GoReflect.WithElemDims(new array<int64>[]{
        new int64[]{Δmath.MinInt64, Δmath.MinInt64}.array(), // 292277026304-08-26T15:42:51Z
        new int64[]{-62167219200L, 0}.array(),               // 0000-01-01T00:00:00Z
        new int64[]{1661201140, 676836973}.array(),          // 2022-08-22T20:45:40.676836973Z
        new int64[]{253402300799L, 999999999}.array(),       // 9999-12-31T23:59:59.999999999Z
        new int64[]{Δmath.MaxInt64, Δmath.MaxInt64}.array()  // -292277022365-05-08T08:17:07Z
    }.slice(), 2))) {
        var ts = vᴛ1.Clone();

        f.Add(ts[0], ts[1], true, false, (nint)0);
        f.Add(ts[0], ts[1], false, true, (nint)0);
        foreach (var (_, offset) in range(slice<nint>([0, 60, 60 * 60, 99 * 60 * 60 + 99 * 60, 123456789]))) {
            f.Add(ts[0], ts[1], false, false, -offset);
            f.Add(ts[0], ts[1], false, false, +offset);
        }
    }

    Ꮡf.Fuzz((ж<Δtesting.T> t, int64 sec, int64 nsec, bool useUTC, bool useLocal, nint tzOffset) => {
        ж<timeꓸLocation> loc = default;
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

        var got = time_internal_test_package.AppendFormatRFC3339(ts, default, false);
        var want = time_internal_test_package.AppendFormatAny(ts, default, RFC3339);
        if (!bytes.Equal(got, want)) {
            t.Errorf("Format(%s, RFC3339) mismatch:\n\tgot:  %s\n\twant: %s", ts, got, want);
        }

        var gotNanos = time_internal_test_package.AppendFormatRFC3339(ts, default, true);
        var wantNanos = time_internal_test_package.AppendFormatAny(ts, default, RFC3339Nano);
        if (!bytes.Equal(gotNanos, wantNanos)) {
            t.Errorf("Format(%s, RFC3339Nano) mismatch:\n\tgot:  %s\n\twant: %s", ts, gotNanos, wantNanos);
        }
    });
}

public static void FuzzParseRFC3339(ж<Δtesting.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, tt) in range(formatTests)) {
        f.Add(tt.result);
    }
    foreach (var (_, tt) in range(parseTests)) {
        f.Add(tt.value);
    }
    foreach (var (_, tt) in range(parseErrorTests)) {
        f.Add(tt.value);
    }
    foreach (var (_, tt) in range(longFractionalDigitsTests)) {
        f.Add(tt.value);
    }

    Ꮡf.Fuzz((ж<Δtesting.T> t, @string s) => {
        // equalTime is like time.Time.Equal, but also compares the time zone.
        bool equalTime(Δtime.Time t1, Δtime.Time t2) {
            var (name1, offset1) = t1.Zone();
            var (name2, offset2) = t2.Zone();
            return t1.Equal(t2) && name1 == name2 && offset1 == offset2;
        }

        foreach (var (_, tz) in range(slice<ж<timeꓸLocation>>([ΔUTC, ΔLocal]))) {
            // Parsing as RFC3339 or RFC3339Nano should be identical.
            var (t1, err1) = time_internal_test_package.ParseAny(RFC3339, s, ΔUTC, tz);
            var (t2, err2) = time_internal_test_package.ParseAny(RFC3339Nano, s, ΔUTC, tz);
            switch (ᐧ) {
            case {} when (err1 == nil) != (err2 == nil): {
                t.Fatalf("ParseAny(%q) error mismatch:\n\tgot:  %v\n\twant: %v", s, err1, err2);
                break;
            }
            case {} when !equalTime(t1, t2): {
                t.Fatalf("ParseAny(%q) value mismatch:\n\tgot:  %v\n\twant: %v", s, t1, t2);
                break;
            }}

            // TODO(https://go.dev/issue/54580):
            // Remove these checks after ParseAny rejects all invalid RFC 3339.
            if (err1 == nil) {
                byte num2(@string sΔ1) => (byte)(10 * (sΔ1[0] - '0') + (sΔ1[1] - '0'));
                switch (ᐧ) {
                case {} when len(s) > 12 && s[12] == (byte)':': {
                    t.Skipf("ParseAny(%q) incorrectly allows single-digit hour fields", s);
                    break;
                }
                case {} when len(s) > 19 && s[19] == (byte)',': {
                    t.Skipf("ParseAny(%q) incorrectly allows comma as sub-second separator", s);
                    break;
                }
                case {} when !strings.HasSuffix(s, "Z") && len(s) > 4 && (num2(s.slice(len(s) - 5)) >= 24 || num2(s.slice(len(s) - 2)) >= 60): {
                    t.Skipf("ParseAny(%q) incorrectly allows out-of-range zone offset", s);
                    break;
                }}
            }

            // Customized parser should be identical to general parser.
            var (got, ok) = time_internal_test_package.ParseRFC3339(s, tz);
            switch (ᐧ) {
            case {} when ok != (err1 == nil): {
                t.Fatalf("ParseRFC3339(%q) error mismatch:\n\tgot:  %v\n\twant: %v", s, ok, err1 == nil);
                break;
            }
            case {} when !equalTime(got, t1): {
                t.Fatalf("ParseRFC3339(%q) value mismatch:\n\tgot:  %v\n\twant: %v", s, got, t2);
                break;
            }}
        }
    });
}

} // end time_test_package
