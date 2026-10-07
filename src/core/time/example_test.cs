// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δmath = math_package;
using Δtime = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.time_internal_test_package;

partial class time_test_package {

internal static void expensiveCall() {
}

public static void ExampleDuration() {
    var t0 = Δtime.Now();
    expensiveCall();
    var t1 = Δtime.Now();
    fmt.Printf("The call took %v to run.\n"u8, t1.Sub(t0));
}

public static void ExampleDuration_Round() {
    var (d, err) = Δtime.ParseDuration("1h15m30.918273645s"u8);
    if (err != default!) {
        throw panic(err);
    }
    var round = new Δtime.Duration[]{
        Δtime.ΔNanosecond,
        Δtime.Microsecond,
        Δtime.Millisecond,
        Δtime.ΔSecond,
        2 * Δtime.ΔSecond,
        Δtime.ΔMinute,
        (Δtime.Duration)(600000000000L),
        Δtime.ΔHour
    }.slice();
    foreach (var (_, r) in round) {
        fmt.Printf("d.Round(%6s) = %s\n"u8, r, d.Round(r).String());
    }
}

// Output:
// d.Round(   1ns) = 1h15m30.918273645s
// d.Round(   1µs) = 1h15m30.918274s
// d.Round(   1ms) = 1h15m30.918s
// d.Round(    1s) = 1h15m31s
// d.Round(    2s) = 1h15m30s
// d.Round(  1m0s) = 1h16m0s
// d.Round( 10m0s) = 1h20m0s
// d.Round(1h0m0s) = 1h0m0s
public static void ExampleDuration_String() {
    fmt.Println((Δtime.Duration)(3720300000000L));
    fmt.Println(300 * Δtime.Millisecond);
}

// Output:
// 1h2m0.3s
// 300ms
public static void ExampleDuration_Truncate() {
    var (d, err) = Δtime.ParseDuration("1h15m30.918273645s"u8);
    if (err != default!) {
        throw panic(err);
    }
    var trunc = new Δtime.Duration[]{
        Δtime.ΔNanosecond,
        Δtime.Microsecond,
        Δtime.Millisecond,
        Δtime.ΔSecond,
        2 * Δtime.ΔSecond,
        Δtime.ΔMinute,
        (Δtime.Duration)(600000000000L),
        Δtime.ΔHour
    }.slice();
    foreach (var (_, t) in trunc) {
        fmt.Printf("d.Truncate(%6s) = %s\n"u8, t, d.Truncate(t).String());
    }
}

// Output:
// d.Truncate(   1ns) = 1h15m30.918273645s
// d.Truncate(   1µs) = 1h15m30.918273s
// d.Truncate(   1ms) = 1h15m30.918s
// d.Truncate(    1s) = 1h15m30s
// d.Truncate(    2s) = 1h15m30s
// d.Truncate(  1m0s) = 1h15m0s
// d.Truncate( 10m0s) = 1h10m0s
// d.Truncate(1h0m0s) = 1h0m0s
public static void ExampleParseDuration() {
    var (hours, _) = Δtime.ParseDuration("10h"u8);
    var (complex, _) = Δtime.ParseDuration("1h10m10s"u8);
    var (micro, _) = Δtime.ParseDuration("1µs"u8);
    // The package also accepts the incorrect but common prefix u for micro.
    var (micro2, _) = Δtime.ParseDuration("1us"u8);
    fmt.Println(hours);
    fmt.Println(complex);
    fmt.Printf("There are %.0f seconds in %v.\n"u8, complex.Seconds(), complex);
    fmt.Printf("There are %d nanoseconds in %v.\n"u8, micro.Nanoseconds(), micro);
    fmt.Printf("There are %6.2e seconds in %v.\n"u8, micro2.Seconds(), micro2);
}

// Output:
// 10h0m0s
// 1h10m10s
// There are 4210 seconds in 1h10m10s.
// There are 1000 nanoseconds in 1µs.
// There are 1.00e-06 seconds in 1µs.
public static void ExampleSince() {
    var start = Δtime.Now();
    expensiveCall();
    var elapsed = Δtime.Since(start);
    fmt.Printf("The call took %v to run.\n"u8, elapsed);
}

public static void ExampleUntil() {
    var futureTime = Δtime.Now().Add((Δtime.Duration)(5000000000L));
    var durationUntil = Δtime.Until(futureTime);
    fmt.Printf("Duration until future time: %.0f seconds"u8, Δmath.Ceil(durationUntil.Seconds()));
}

// Output: Duration until future time: 5 seconds
public static void ExampleDuration_Abs() {
    var positiveDuration = (Δtime.Duration)(5000000000L);
    var negativeDuration = (Δtime.Duration)(-3000000000L);
    var minInt64CaseDuration = ((Δtime.Duration)Δmath.MinInt64);
    var absPositive = positiveDuration.Abs();
    var absNegative = negativeDuration.Abs();
    var absSpecial = minInt64CaseDuration.Abs() == ((Δtime.Duration)Δmath.MaxInt64);
    fmt.Printf("Absolute value of positive duration: %v\n"u8, absPositive);
    fmt.Printf("Absolute value of negative duration: %v\n"u8, absNegative);
    fmt.Printf("Absolute value of MinInt64 equal to MaxInt64: %t\n"u8, absSpecial);
}

// Output:
// Absolute value of positive duration: 5s
// Absolute value of negative duration: 3s
// Absolute value of MinInt64 equal to MaxInt64: true
public static void ExampleDuration_Hours() {
    var (h, _) = Δtime.ParseDuration("4h30m"u8);
    fmt.Printf("I've got %.1f hours of work left."u8, h.Hours());
}

// Output: I've got 4.5 hours of work left.
public static void ExampleDuration_Microseconds() {
    var (u, _) = Δtime.ParseDuration("1s"u8);
    fmt.Printf("One second is %d microseconds.\n"u8, u.Microseconds());
}

// Output:
// One second is 1000000 microseconds.
public static void ExampleDuration_Milliseconds() {
    var (u, _) = Δtime.ParseDuration("1s"u8);
    fmt.Printf("One second is %d milliseconds.\n"u8, u.Milliseconds());
}

// Output:
// One second is 1000 milliseconds.
public static void ExampleDuration_Minutes() {
    var (m, _) = Δtime.ParseDuration("1h30m"u8);
    fmt.Printf("The movie is %.0f minutes long."u8, m.Minutes());
}

// Output: The movie is 90 minutes long.
public static void ExampleDuration_Nanoseconds() {
    var (u, _) = Δtime.ParseDuration("1µs"u8);
    fmt.Printf("One microsecond is %d nanoseconds.\n"u8, u.Nanoseconds());
}

// Output:
// One microsecond is 1000 nanoseconds.
public static void ExampleDuration_Seconds() {
    var (m, _) = Δtime.ParseDuration("1m30s"u8);
    fmt.Printf("Take off in t-%.0f seconds."u8, m.Seconds());
}

// Output: Take off in t-90 seconds.
internal static channel<nint> c;

internal static void handle(nint _) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object timedOutˢ = (@string)"timed out"u8;

public static void ExampleAfter() {
    var selᴛ1 = c;
    var selᴛ2 = Δtime.After((Δtime.Duration)(10000000000L));
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var m): {
        handle(m);
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        fmt.Println(timedOutˢ);
        break;
    }}
}

public static void ExampleSleep() {
    Δtime.Sleep(100 * Δtime.Millisecond);
}

internal static @string statusUpdate() {
    return ""u8;
}

public static void ExampleTick() {
    var c = Δtime.Tick((Δtime.Duration)(5000000000L));
    foreach (var next in c) {
        fmt.Printf("%v %s\n"u8, next, statusUpdate());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object happyGoDayˢ = (@string)"Happy Go day!"u8;

public static void ExampleMonth() {
    var (_, month, day) = Δtime.Now().Date();
    if (month == Δtime.November && day == 10) {
        fmt.Println(happyGoDayˢ);
    }
}

public static void ExampleDate() {
    var t = Δtime.Date(2009, Δtime.November, 10, 23, 0, 0, 0, Δtime.ΔUTC);
    fmt.Printf("Go launched at %s\n"u8, t.Local());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object doneˢ = (@string)"Done!"u8;
internal static readonly object currentTimeˢ = (@string)"Current time: "u8;

// Output: Go launched at 2009-11-10 15:00:00 -0800 PST
public static partial void ExampleNewTicker() {
    GoFrame ᒐ = default;
    try {
        var ticker = Δtime.NewTicker(Δtime.ΔSecond);
        var tickerʗ1 = ticker;
        defer(tickerʗ1.Stop, ref ᒐ);
        var done = new channel<bool>(0);
        var doneʗ1 = done;
        goǃ(() => {
            Δtime.Sleep((Δtime.Duration)(10000000000L));
            doneʗ1.ᐸꟷ(true);
        });
        while (ᐧ) {
            var selᴛ3 = done;
            var selᴛ4 = (~ticker).C;
            switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
            case 0 when selᴛ3.ꟷᐳ(out _): {
                fmt.Println(doneˢ);
                return;
            }
            case 1 when selᴛ4.ꟷᐳ(out var t): {
                fmt.Println(currentTimeˢ, t);
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wedFeb25110639Pst2015ˢ = "Wed Feb 25 11:06:39 PST 2015"u8;
internal static readonly @string asiaShanghaiˢ = "Asia/Shanghai"u8;
internal static readonly object defaultFormatˢ = (@string)"default format:"u8;
internal static readonly object unixFormatˢ = (@string)"Unix format:"u8;
internal static readonly object sameInUtcˢ = (@string)"Same, in UTC:"u8;
internal static readonly object inShanghaiWithSecondsˢ = (@string)"in Shanghai with seconds:"u8;
internal static readonly object inShanghaiWithColonˢ = (@string)"in Shanghai with colon seconds:"u8;
internal static readonly @string basicFullDateˢ = "Basic full date"u8;
internal static readonly @string monJan2150405Mst2006ˢ = "Mon Jan 2 15:04:05 MST 2006"u8;
internal static readonly @string basicShortDateˢ = "Basic short date"u8;
internal static readonly @string amPmˢ = "AM/PM"u8;
internal static readonly @string wedFeb251106391234Pstˢ = "Wed Feb 25 11:06:39.1234 PST 2015"u8;
internal static readonly @string noFractionˢ = "No fraction"u8;
internal static readonly @string forFractionˢ = "0s for fraction"u8;
internal static readonly @string forFractionˢ2 = "9s for fraction"u8;

public static void ExampleTime_Format() {
    // Parse a time value from a string in the standard Unix format.
    ref var t = ref heap<Δtime.Time>(out var Ꮡt);
    (t, var err) = Δtime.Parse(Δtime.UnixDate, wedFeb25110639Pst2015ˢ);
    if (err != default!) {
        // Always check errors even if they should not happen.
        throw panic(err);
    }
    (var tz, err) = Δtime.LoadLocation(asiaShanghaiˢ);
    if (err != default!) {
        // Always check errors even if they should not happen.
        throw panic(err);
    }
    // time.Time's Stringer method is useful without any format.
    fmt.Println(defaultFormatˢ, t);
    // Predefined constants in the package implement common layouts.
    fmt.Println(unixFormatˢ, t.Format(Δtime.UnixDate));
    // The time zone attached to the time value affects its output.
    fmt.Println(sameInUtcˢ, t.UTC().Format(Δtime.UnixDate));
    fmt.Println(inShanghaiWithSecondsˢ, t.In(tz).Format("2006-01-02T15:04:05 -070000"u8));
    fmt.Println(inShanghaiWithColonˢ, t.In(tz).Format("2006-01-02T15:04:05 -07:00:00"u8));
    // The rest of this function demonstrates the properties of the
    // layout string used in the format.
    // The layout string used by the Parse function and Format method
    // shows by example how the reference time should be represented.
    // We stress that one must show how the reference time is formatted,
    // not a time of the user's choosing. Thus each layout string is a
    // representation of the time stamp,
    //	Jan 2 15:04:05 2006 MST
    // An easy way to remember this value is that it holds, when presented
    // in this order, the values (lined up with the elements above):
    //	  1 2  3  4  5    6  -7
    // There are some wrinkles illustrated below.
    // Most uses of Format and Parse use constant layout strings such as
    // the ones defined in this package, but the interface is flexible,
    // as these examples show.
    // Define a helper function to make the examples' output look nice.
    void @do(@string name, @string layout, @string want) {
        @string got = Ꮡt.Value.Format(layout);
        if (want != got) {
            fmt.Printf("error: for %q got %q; expected %q\n"u8, layout, got, want);
            return;
        }
        fmt.Printf("%-16s %q gives %q\n"u8, name, layout, got);
    }
    // Print a header in our output.
    fmt.Printf("\nFormats:\n\n"u8);
    // Simple starter examples.
    @do(basicFullDateˢ, monJan2150405Mst2006ˢ, wedFeb25110639Pst2015ˢ);
    @do(basicShortDateˢ, "2006/01/02"u8, "2015/02/25"u8);
    // The hour of the reference time is 15, or 3PM. The layout can express
    // it either way, and since our value is the morning we should see it as
    // an AM time. We show both in one format string. Lower case too.
    @do(amPmˢ, "3PM==3pm==15h"u8, "11AM==11am==11h"u8);
    // When parsing, if the seconds value is followed by a decimal point
    // and some digits, that is taken as a fraction of a second even if
    // the layout string does not represent the fractional second.
    // Here we add a fractional second to our time value used above.
    (t, err) = Δtime.Parse(Δtime.UnixDate, wedFeb251106391234Pstˢ);
    if (err != default!) {
        throw panic(err);
    }
    // It does not appear in the output if the layout string does not contain
    // a representation of the fractional second.
    @do(noFractionˢ, Δtime.UnixDate, wedFeb25110639Pst2015ˢ);
    // Fractional seconds can be printed by adding a run of 0s or 9s after
    // a decimal point in the seconds value in the layout string.
    // If the layout digits are 0s, the fractional second is of the specified
    // width. Note that the output has a trailing zero.
    @do(forFractionˢ, "15:04:05.00000"u8, "11:06:39.12340"u8);
    // If the fraction in the layout is 9s, trailing zeros are dropped.
    @do(forFractionˢ2, "15:04:05.99999999"u8, "11:06:39.1234"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string satMar7110639Pst2015ˢ = "Sat Mar 7 11:06:39 PST 2015"u8;
internal static readonly @string unixˢ = "Unix"u8;
internal static readonly @string satMar7110639Pst2015ˢ2 = "Sat Mar  7 11:06:39 PST 2015"u8;
internal static readonly @string noPadˢ = "No pad"u8;
internal static readonly @string spacesˢ = "Spaces"u8;
internal static readonly @string zerosˢ = "Zeros"u8;
internal static readonly @string suppressedPadˢ = "Suppressed pad"u8;

// Output:
// default format: 2015-02-25 11:06:39 -0800 PST
// Unix format: Wed Feb 25 11:06:39 PST 2015
// Same, in UTC: Wed Feb 25 19:06:39 UTC 2015
// in Shanghai with seconds: 2015-02-26T03:06:39 +080000
// in Shanghai with colon seconds: 2015-02-26T03:06:39 +08:00:00
//
// Formats:
//
// Basic full date  "Mon Jan 2 15:04:05 MST 2006" gives "Wed Feb 25 11:06:39 PST 2015"
// Basic short date "2006/01/02" gives "2015/02/25"
// AM/PM            "3PM==3pm==15h" gives "11AM==11am==11h"
// No fraction      "Mon Jan _2 15:04:05 MST 2006" gives "Wed Feb 25 11:06:39 PST 2015"
// 0s for fraction  "15:04:05.00000" gives "11:06:39.12340"
// 9s for fraction  "15:04:05.99999999" gives "11:06:39.1234"
public static void ExampleTime_Format_pad() {
    // Parse a time value from a string in the standard Unix format.
    ref var t = ref heap<Δtime.Time>(out var Ꮡt);
    (t, var err) = Δtime.Parse(Δtime.UnixDate, satMar7110639Pst2015ˢ);
    if (err != default!) {
        // Always check errors even if they should not happen.
        throw panic(err);
    }
    // Define a helper function to make the examples' output look nice.
    var tʗ1 = t;
    void @do(@string name, @string layout, @string want) {
        @string got = tʗ1.Format(layout);
        if (want != got) {
            fmt.Printf("error: for %q got %q; expected %q\n"u8, layout, got, want);
            return;
        }
        fmt.Printf("%-16s %q gives %q\n"u8, name, layout, got);
    }
    // The predefined constant Unix uses an underscore to pad the day.
    @do(unixˢ, Δtime.UnixDate, satMar7110639Pst2015ˢ2);
    // For fixed-width printing of values, such as the date, that may be one or
    // two characters (7 vs. 07), use an _ instead of a space in the layout string.
    // Here we print just the day, which is 2 in our layout string and 7 in our
    // value.
    @do(noPadˢ, "<2>"u8, "<7>"u8);
    // An underscore represents a space pad, if the date only has one digit.
    @do(spacesˢ, "<_2>"u8, "< 7>"u8);
    // A "0" indicates zero padding for single-digit values.
    @do(zerosˢ, "<02>"u8, "<07>"u8);
    // If the value is already the right width, padding is not used.
    // For instance, the second (05 in the reference time) in our value is 39,
    // so it doesn't need padding, but the minutes (04, 06) does.
    @do(suppressedPadˢ, "04:05"u8, "06:39"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string jan22006At304pmMstˢ = "Jan 2, 2006 at 3:04pm (MST)"u8;
internal static readonly @string feb32013At754pmUtcˢ = "Feb 3, 2013 at 7:54pm (UTC)"u8;

// Output:
// Unix             "Mon Jan _2 15:04:05 MST 2006" gives "Sat Mar  7 11:06:39 PST 2015"
// No pad           "<2>" gives "<7>"
// Spaces           "<_2>" gives "< 7>"
// Zeros            "<02>" gives "<07>"
// Suppressed pad   "04:05" gives "06:39"
public static void ExampleTime_GoString() {
    var t = Δtime.Date(2009, Δtime.November, 10, 23, 0, 0, 0, Δtime.ΔUTC);
    fmt.Println(t.GoString());
    t = t.Add((Δtime.Duration)(60000000000L));
    fmt.Println(t.GoString());
    t = t.AddDate(0, 1, 0);
    fmt.Println(t.GoString());
    (t, _) = Δtime.Parse(jan22006At304pmMstˢ, feb32013At754pmUtcˢ);
    fmt.Println(t.GoString());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string feb32013At754pmPstˢ = "Feb 3, 2013 at 7:54pm (PST)"u8;
internal static readonly @string feb03ˢ = "2013-Feb-03"u8;
internal static readonly object errorˢ = (@string)"error"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string longFormᶜ = "Jan 2, 2006 at 3:04pm (MST)"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string shortFormᶜ = "2006-Jan-02"u8;

// Output:
// time.Date(2009, time.November, 10, 23, 0, 0, 0, time.UTC)
// time.Date(2009, time.November, 10, 23, 1, 0, 0, time.UTC)
// time.Date(2009, time.December, 10, 23, 1, 0, 0, time.UTC)
// time.Date(2013, time.February, 3, 19, 54, 0, 0, time.UTC)
public static void ExampleParse() {
    // See the example for Time.Format for a thorough description of how
    // to define the layout string to parse a time.Time value; Parse and
    // Format use the same model to describe their input and output.
    // longForm shows by example how the reference time would be represented in
    // the desired layout.
    @string longForm = longFormᶜ;
    var (t, _) = Δtime.Parse(longForm, feb32013At754pmPstˢ);
    fmt.Println(t);
    // shortForm is another way the reference time would be represented
    // in the desired layout; it has no time zone present.
    // Note: without explicit zone, returns time in UTC.
    @string shortForm = shortFormᶜ;
    (t, _) = Δtime.Parse(shortForm, feb03ˢ);
    fmt.Println(t);
    // Some valid layouts are invalid time values, due to format specifiers
    // such as _ for space padding and Z for zone information.
    // For example the RFC3339 layout 2006-01-02T15:04:05Z07:00
    // contains both Z and a time zone offset in order to handle both valid options:
    // 2006-01-02T15:04:05Z
    // 2006-01-02T15:04:05+07:00
    (t, _) = Δtime.Parse(Δtime.RFC3339, "2006-01-02T15:04:05Z"u8);
    fmt.Println(t);
    (t, _) = Δtime.Parse(Δtime.RFC3339, "2006-01-02T15:04:05+07:00"u8);
    fmt.Println(t);
    var (_, err) = Δtime.Parse(Δtime.RFC3339, Δtime.RFC3339);
    fmt.Println(errorˢ, err); // Returns an error as the layout is not a valid time value
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string europeBerlinˢ = "Europe/Berlin"u8;
internal static readonly @string jul92012At502amCestˢ = "Jul 9, 2012 at 5:02am (CEST)"u8;
internal static readonly @string jul09ˢ = "2012-Jul-09"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string longFormᶜ1 = "Jan 2, 2006 at 3:04pm (MST)"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string shortFormᶜ1 = "2006-Jan-02"u8;

// Output:
// 2013-02-03 19:54:00 -0800 PST
// 2013-02-03 00:00:00 +0000 UTC
// 2006-01-02 15:04:05 +0000 UTC
// 2006-01-02 15:04:05 +0700 +0700
// error parsing time "2006-01-02T15:04:05Z07:00": extra text: "07:00"
public static void ExampleParseInLocation() {
    var (loc, _) = Δtime.LoadLocation(europeBerlinˢ);
    // This will look for the name CEST in the Europe/Berlin time zone.
    @string longForm = longFormᶜ1;
    var (t, _) = Δtime.ParseInLocation(longForm, jul92012At502amCestˢ, loc);
    fmt.Println(t);
    // Note: without explicit zone, returns time in given location.
    @string shortForm = shortFormᶜ1;
    (t, _) = Δtime.ParseInLocation(shortForm, jul09ˢ, loc);
    fmt.Println(t);
}

// Output:
// 2012-07-09 05:02:00 +0200 CEST
// 2012-07-09 00:00:00 +0200 CEST
public static void ExampleUnix() {
    var unixTime = Δtime.Date(2009, Δtime.November, 10, 23, 0, 0, 0, Δtime.ΔUTC);
    fmt.Println(unixTime.Unix());
    var t = Δtime.Unix(unixTime.Unix(), 0).UTC();
    fmt.Println(t);
}

// Output:
// 1257894000
// 2009-11-10 23:00:00 +0000 UTC
public static void ExampleUnixMicro() {
    var umt = Δtime.Date(2009, Δtime.November, 10, 23, 0, 0, 0, Δtime.ΔUTC);
    fmt.Println(umt.UnixMicro());
    var t = Δtime.UnixMicro(umt.UnixMicro()).UTC();
    fmt.Println(t);
}

// Output:
// 1257894000000000
// 2009-11-10 23:00:00 +0000 UTC
public static void ExampleUnixMilli() {
    var umt = Δtime.Date(2009, Δtime.November, 10, 23, 0, 0, 0, Δtime.ΔUTC);
    fmt.Println(umt.UnixMilli());
    var t = Δtime.UnixMilli(umt.UnixMilli()).UTC();
    fmt.Println(t);
}

// Output:
// 1257894000000
// 2009-11-10 23:00:00 +0000 UTC
public static void ExampleTime_Unix() {
    // 1 billion seconds of Unix, three ways.
    fmt.Println(Δtime.Unix(1000000000, 0).UTC()); // 1e9 seconds
    fmt.Println(Δtime.Unix(0, 1000000000000000000).UTC()); // 1e18 nanoseconds
    fmt.Println(Δtime.Unix(2000000000, -1000000000000000000).UTC()); // 2e9 seconds - 1e18 nanoseconds
    var t = Δtime.Date(2001, Δtime.September, 9, 1, 46, 40, 0, Δtime.ΔUTC);
    fmt.Println(t.Unix()); // seconds since 1970
    fmt.Println(t.UnixNano()); // nanoseconds since 1970
}

// Output:
// 2001-09-09 01:46:40 +0000 UTC
// 2001-09-09 01:46:40 +0000 UTC
// 2001-09-09 01:46:40 +0000 UTC
// 1000000000
// 1000000000000000000
public static void ExampleTime_Round() {
    var t = Δtime.Date(0, 0, 0, 12, 15, 30, 918273645, Δtime.ΔUTC);
    var round = new Δtime.Duration[]{
        Δtime.ΔNanosecond,
        Δtime.Microsecond,
        Δtime.Millisecond,
        Δtime.ΔSecond,
        2 * Δtime.ΔSecond,
        Δtime.ΔMinute,
        (Δtime.Duration)(600000000000L),
        Δtime.ΔHour
    }.slice();
    foreach (var (_, d) in round) {
        fmt.Printf("t.Round(%6s) = %s\n"u8, d, t.Round(d).Format("15:04:05.999999999"u8));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string jan02150405ˢ = "2006 Jan 02 15:04:05"u8;
internal static readonly @string dec07121530918273645ˢ = "2012 Dec 07 12:15:30.918273645"u8;

// Output:
// t.Round(   1ns) = 12:15:30.918273645
// t.Round(   1µs) = 12:15:30.918274
// t.Round(   1ms) = 12:15:30.918
// t.Round(    1s) = 12:15:31
// t.Round(    2s) = 12:15:30
// t.Round(  1m0s) = 12:16:00
// t.Round( 10m0s) = 12:20:00
// t.Round(1h0m0s) = 12:00:00
public static void ExampleTime_Truncate() {
    var (t, _) = Δtime.Parse(jan02150405ˢ, dec07121530918273645ˢ);
    var trunc = new Δtime.Duration[]{
        Δtime.ΔNanosecond,
        Δtime.Microsecond,
        Δtime.Millisecond,
        Δtime.ΔSecond,
        2 * Δtime.ΔSecond,
        Δtime.ΔMinute,
        (Δtime.Duration)(600000000000L)
    }.slice();
    foreach (var (_, d) in trunc) {
        fmt.Printf("t.Truncate(%5s) = %s\n"u8, d, t.Truncate(d).Format("15:04:05.999999999"u8));
    }
    // To round to the last midnight in the local timezone, create a new Date.
    var midnight = Δtime.Date(t.Year(), t.Month(), t.Day(), 0, 0, 0, 0, Δtime.ΔLocal);
    _ = midnight;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string americaLosAngelesˢ = "America/Los_Angeles"u8;

// Output:
// t.Truncate(  1ns) = 12:15:30.918273645
// t.Truncate(  1µs) = 12:15:30.918273
// t.Truncate(  1ms) = 12:15:30.918
// t.Truncate(   1s) = 12:15:30
// t.Truncate(   2s) = 12:15:30
// t.Truncate( 1m0s) = 12:15:00
// t.Truncate(10m0s) = 12:10:00
public static void ExampleLoadLocation() {
    var (location, err) = Δtime.LoadLocation(americaLosAngelesˢ);
    if (err != default!) {
        throw panic(err);
    }
    var timeInUTC = Δtime.Date(2018, 8, 30, 12, 0, 0, 0, Δtime.ΔUTC);
    fmt.Println(timeInUTC.In(location));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string beijingTimeˢ = "Beijing Time"u8;

// Output: 2018-08-30 05:00:00 -0700 PDT
public static void ExampleLocation() {
    // China doesn't have daylight saving. It uses a fixed 8 hour offset from UTC.
    nint secondsEastOfUTC = (nint)((Δtime.Duration)(28800000000000L)).Seconds();
    var beijing = Δtime.FixedZone(beijingTimeˢ, secondsEastOfUTC);
    // If the system has a timezone database present, it's possible to load a location
    // from that, e.g.:
    //    newYork, err := time.LoadLocation("America/New_York")
    // Creating a time requires a location. Common locations are time.Local and time.UTC.
    var timeInUTC = Δtime.Date(2009, 1, 1, 12, 0, 0, 0, Δtime.ΔUTC);
    var sameTimeInBeijing = Δtime.Date(2009, 1, 1, 20, 0, 0, 0, beijing);
    // Although the UTC clock time is 1200 and the Beijing clock time is 2000, Beijing is
    // 8 hours ahead so the two dates actually represent the same instant.
    var timesAreEqual = timeInUTC.Equal(sameTimeInBeijing);
    fmt.Println(timesAreEqual);
}

// Output:
// true
public static void ExampleTime_Add() {
    var start = Δtime.Date(2009, 1, 1, 12, 0, 0, 0, Δtime.ΔUTC);
    var afterTenSeconds = start.Add((Δtime.Duration)(10000000000L));
    var afterTenMinutes = start.Add((Δtime.Duration)(600000000000L));
    var afterTenHours = start.Add((Δtime.Duration)(36000000000000L));
    var afterTenDays = start.Add((Δtime.Duration)(864000000000000L));
    fmt.Printf("start = %v\n"u8, start);
    fmt.Printf("start.Add(time.Second * 10) = %v\n"u8, afterTenSeconds);
    fmt.Printf("start.Add(time.Minute * 10) = %v\n"u8, afterTenMinutes);
    fmt.Printf("start.Add(time.Hour * 10) = %v\n"u8, afterTenHours);
    fmt.Printf("start.Add(time.Hour * 24 * 10) = %v\n"u8, afterTenDays);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string europeZurichˢ = "Europe/Zurich"u8;

// Output:
// start = 2009-01-01 12:00:00 +0000 UTC
// start.Add(time.Second * 10) = 2009-01-01 12:00:10 +0000 UTC
// start.Add(time.Minute * 10) = 2009-01-01 12:10:00 +0000 UTC
// start.Add(time.Hour * 10) = 2009-01-01 22:00:00 +0000 UTC
// start.Add(time.Hour * 24 * 10) = 2009-01-11 12:00:00 +0000 UTC
public static void ExampleTime_AddDate() {
    var start = Δtime.Date(2023, 3, 25, 12, 0, 0, 0, Δtime.ΔUTC);
    var oneDayLater = start.AddDate(0, 0, 1);
    var dayDuration = oneDayLater.Sub(start);
    var oneMonthLater = start.AddDate(0, 1, 0);
    var oneYearLater = start.AddDate(1, 0, 0);
    var (zurich, err) = Δtime.LoadLocation(europeZurichˢ);
    if (err != default!) {
        throw panic(err);
    }
    // This was the day before a daylight saving time transition in Zürich.
    var startZurich = Δtime.Date(2023, 3, 25, 12, 0, 0, 0, zurich);
    var oneDayLaterZurich = startZurich.AddDate(0, 0, 1);
    var dayDurationZurich = oneDayLaterZurich.Sub(startZurich);
    fmt.Printf("oneDayLater: start.AddDate(0, 0, 1) = %v\n"u8, oneDayLater);
    fmt.Printf("oneMonthLater: start.AddDate(0, 1, 0) = %v\n"u8, oneMonthLater);
    fmt.Printf("oneYearLater: start.AddDate(1, 0, 0) = %v\n"u8, oneYearLater);
    fmt.Printf("oneDayLaterZurich: startZurich.AddDate(0, 0, 1) = %v\n"u8, oneDayLaterZurich);
    fmt.Printf("Day duration in UTC: %v | Day duration in Zürich: %v\n"u8, dayDuration, dayDurationZurich);
}

// Output:
// oneDayLater: start.AddDate(0, 0, 1) = 2023-03-26 12:00:00 +0000 UTC
// oneMonthLater: start.AddDate(0, 1, 0) = 2023-04-25 12:00:00 +0000 UTC
// oneYearLater: start.AddDate(1, 0, 0) = 2024-03-25 12:00:00 +0000 UTC
// oneDayLaterZurich: startZurich.AddDate(0, 0, 1) = 2023-03-26 12:00:00 +0200 CEST
// Day duration in UTC: 24h0m0s | Day duration in Zürich: 23h0m0s
public static void ExampleTime_After() {
    var year2000 = Δtime.Date(2000, 1, 1, 0, 0, 0, 0, Δtime.ΔUTC);
    var year3000 = Δtime.Date(3000, 1, 1, 0, 0, 0, 0, Δtime.ΔUTC);
    var isYear3000AfterYear2000 = year3000.After(year2000); // True
    var isYear2000AfterYear3000 = year2000.After(year3000); // False
    fmt.Printf("year3000.After(year2000) = %v\n"u8, isYear3000AfterYear2000);
    fmt.Printf("year2000.After(year3000) = %v\n"u8, isYear2000AfterYear3000);
}

// Output:
// year3000.After(year2000) = true
// year2000.After(year3000) = false
public static void ExampleTime_Before() {
    var year2000 = Δtime.Date(2000, 1, 1, 0, 0, 0, 0, Δtime.ΔUTC);
    var year3000 = Δtime.Date(3000, 1, 1, 0, 0, 0, 0, Δtime.ΔUTC);
    var isYear2000BeforeYear3000 = year2000.Before(year3000); // True
    var isYear3000BeforeYear2000 = year3000.Before(year2000); // False
    fmt.Printf("year2000.Before(year3000) = %v\n"u8, isYear2000BeforeYear3000);
    fmt.Printf("year3000.Before(year2000) = %v\n"u8, isYear3000BeforeYear2000);
}

// Output:
// year2000.Before(year3000) = true
// year3000.Before(year2000) = false
public static void ExampleTime_Date() {
    var d = Δtime.Date(2000, 2, 1, 12, 30, 0, 0, Δtime.ΔUTC);
    var (year, month, day) = d.Date();
    fmt.Printf("year = %v\n"u8, year);
    fmt.Printf("month = %v\n"u8, month);
    fmt.Printf("day = %v\n"u8, day);
}

// Output:
// year = 2000
// month = February
// day = 1
public static void ExampleTime_Day() {
    var d = Δtime.Date(2000, 2, 1, 12, 30, 0, 0, Δtime.ΔUTC);
    nint day = d.Day();
    fmt.Printf("day = %v\n"u8, day);
}

// Output:
// day = 1
public static void ExampleTime_Equal() {
    nint secondsEastOfUTC = (nint)((Δtime.Duration)(28800000000000L)).Seconds();
    var beijing = Δtime.FixedZone(beijingTimeˢ, secondsEastOfUTC);
    // Unlike the equal operator, Equal is aware that d1 and d2 are the
    // same instant but in different time zones.
    var d1 = Δtime.Date(2000, 2, 1, 12, 30, 0, 0, Δtime.ΔUTC);
    var d2 = Δtime.Date(2000, 2, 1, 20, 30, 0, 0, beijing);
    var datesEqualUsingEqualOperator = d1 == d2;
    var datesEqualUsingFunction = d1.Equal(d2);
    fmt.Printf("datesEqualUsingEqualOperator = %v\n"u8, datesEqualUsingEqualOperator);
    fmt.Printf("datesEqualUsingFunction = %v\n"u8, datesEqualUsingFunction);
}

// Output:
// datesEqualUsingEqualOperator = false
// datesEqualUsingFunction = true
public static void ExampleTime_String() {
    var timeWithNanoseconds = Δtime.Date(2000, 2, 1, 12, 13, 14, 15, Δtime.ΔUTC);
    @string withNanoseconds = timeWithNanoseconds.String();
    var timeWithoutNanoseconds = Δtime.Date(2000, 2, 1, 12, 13, 14, 0, Δtime.ΔUTC);
    @string withoutNanoseconds = timeWithoutNanoseconds.String();
    fmt.Printf("withNanoseconds = %v\n"u8, ((@string)withNanoseconds));
    fmt.Printf("withoutNanoseconds = %v\n"u8, ((@string)withoutNanoseconds));
}

// Output:
// withNanoseconds = 2000-02-01 12:13:14.000000015 +0000 UTC
// withoutNanoseconds = 2000-02-01 12:13:14 +0000 UTC
public static void ExampleTime_Sub() {
    var start = Δtime.Date(2000, 1, 1, 0, 0, 0, 0, Δtime.ΔUTC);
    var end = Δtime.Date(2000, 1, 1, 12, 0, 0, 0, Δtime.ΔUTC);
    var difference = end.Sub(start);
    fmt.Printf("difference = %v\n"u8, difference);
}

// Output:
// difference = 12h0m0s
public static void ExampleTime_AppendFormat() {
    var t = Δtime.Date(2017, Δtime.November, 4, 11, 0, 0, 0, Δtime.ΔUTC);
    var text = slice<byte>("Time: "u8);
    text = t.AppendFormat(text, Δtime.Kitchen);
    fmt.Println(((@string)text));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string utc8ˢ = "UTC-8"u8;
internal static readonly object theTimeIsˢ = (@string)"The time is:"u8;

// Output:
// Time: 11:00AM
public static void ExampleFixedZone() {
    var loc = Δtime.FixedZone(utc8ˢ, -8 * 60 * 60);
    var t = Δtime.Date(2009, Δtime.November, 10, 23, 0, 0, 0, loc);
    fmt.Println(theTimeIsˢ, t.Format(Δtime.RFC822));
}

// Output: The time is: 10 Nov 09 23:00 UTC-8

} // end time_test_package
