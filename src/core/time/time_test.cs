// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using gob = encoding.gob_package;
using json = encoding.json_package;
using fmt = fmt_package;
using Δmath = math_package;
using big = go.math.big_package;
using rand = go.math.rand_package;
using Δos = os_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using Δsync = sync_package;
using Δtesting = testing_package;
using quick = go.testing.quick_package;
using static time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using encoding;
using fs = io.fs_package;
using go.math;
using go.testing;
using static go.time_internal_test_package;
using Δio = io_package;
using Δtime = time_package;

partial class time_test_package {

public static void TestInternal(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in time_internal_test_package.InternalTests) {
        ref var tt = ref heap(new InternalTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<Δtesting.T> tΔ1) => {
            ttʗ1.Test(new time_test_package.testing_TжtestingT(tΔ1));
        });
    }
}

public static void TestZeroTime(ж<Δtesting.T> Ꮡt) {
    Δtime.Time zero = default!;
    var (year, month, day) = zero.Date();
    var (hour, min, sec) = zero.Clock();
    nint nsec = zero.Nanosecond();
    nint yday = zero.YearDay();
    timeꓸWeekday wday = zero.Weekday();
    if (year != 1 || month != January || day != 1 || hour != 0 || min != 0 || sec != 0 || nsec != 0 || yday != 1 || wday != Monday) {
        Ꮡt.Errorf("zero time = %v %v %v year %v %02d:%02d:%02d.%09d yday %d want Monday Jan 1 year 1 00:00:00.000000000 yday 1"u8,
            wday, month, day, year, hour, min, sec, nsec, yday);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object likelyProblemTheTimeZoneˢ = (@string)"Likely problem: the time zone files have not been installed."u8;

// We should be in PST/PDT, but if the time zone files are missing we
// won't be. The purpose of this test is to at least explain why some of
// the subsequent tests fail.
public static void TestZoneData(ж<Δtesting.T> Ꮡt) {
    var lt = Now();
    // PST is 8 hours west, PDT is 7 hours west. We could use the name but it's not unique.
    {
        var (name, off) = lt.Zone(); if (off != -8 * 60 * 60 && off != -7 * 60 * 60) {
            Ꮡt.Errorf("Unable to find US Pacific time zone data for testing; time zone is %q offset %d"u8, name, off);
            Ꮡt.Error(likelyProblemTheTimeZoneˢ);
        }
    }
}

// parsedTime is the struct representing a parsed time value.
partial struct parsedTime {
    public nint Year;
    public timeꓸMonth Month;
    public nint Day;
    public nint Hour, Minute, Second; // 15:04:05 is 15, 4, 5.
    public nint Nanosecond; // Fractional second.
    public timeꓸWeekday Weekday;
    public nint ZoneOffset;   // seconds east of UTC, e.g. -7*60*60 for -0700
    public @string Zone; // e.g., "MST"
}

partial struct TimeTest {
    internal int64 seconds;
    internal parsedTime golden;
}

internal static slice<TimeTest> utctests = new TimeTest[]{
    new(0, new parsedTime(1970, January, 1, 0, 0, 0, 0, Thursday, 0, "UTC"u8)),
    new(1221681866, new parsedTime(2008, September, 17, 20, 4, 26, 0, Wednesday, 0, "UTC"u8)),
    new(-1221681866, new parsedTime(1931, April, 16, 3, 55, 34, 0, Thursday, 0, "UTC"u8)),
    new(-11644473600L, new parsedTime(1601, January, 1, 0, 0, 0, 0, Monday, 0, "UTC"u8)),
    new(599529660, new parsedTime(1988, December, 31, 0, 1, 0, 0, Saturday, 0, "UTC"u8)),
    new(978220860, new parsedTime(2000, December, 31, 0, 1, 0, 0, Sunday, 0, "UTC"u8))
}.slice();

internal static slice<TimeTest> nanoutctests = new TimeTest[]{
    new(0, new parsedTime(1970, January, 1, 0, 0, 0, 100000000, Thursday, 0, "UTC"u8)),
    new(1221681866, new parsedTime(2008, September, 17, 20, 4, 26, 200000000, Wednesday, 0, "UTC"u8))
}.slice();

internal static slice<TimeTest> localtests = new TimeTest[]{
    new(0, new parsedTime(1969, December, 31, 16, 0, 0, 0, Wednesday, -8 * 60 * 60, "PST"u8)),
    new(1221681866, new parsedTime(2008, September, 17, 13, 4, 26, 0, Wednesday, -7 * 60 * 60, "PDT"u8)),
    new(2159200800L, new parsedTime(2038, June, 3, 11, 0, 0, 0, Thursday, -7 * 60 * 60, "PDT"u8)),
    new(2152173599L, new parsedTime(2038, March, 14, 1, 59, 59, 0, Sunday, -8 * 60 * 60, "PST"u8)),
    new(2152173600L, new parsedTime(2038, March, 14, 3, 0, 0, 0, Sunday, -7 * 60 * 60, "PDT"u8)),
    new(2152173601L, new parsedTime(2038, March, 14, 3, 0, 1, 0, Sunday, -7 * 60 * 60, "PDT"u8)),
    new(2172733199L, new parsedTime(2038, November, 7, 1, 59, 59, 0, Sunday, -7 * 60 * 60, "PDT"u8)),
    new(2172733200L, new parsedTime(2038, November, 7, 1, 0, 0, 0, Sunday, -8 * 60 * 60, "PST"u8)),
    new(2172733201L, new parsedTime(2038, November, 7, 1, 0, 1, 0, Sunday, -8 * 60 * 60, "PST"u8))
}.slice();

internal static slice<TimeTest> nanolocaltests = new TimeTest[]{
    new(0, new parsedTime(1969, December, 31, 16, 0, 0, 100000000, Wednesday, -8 * 60 * 60, "PST"u8)),
    new(1221681866, new parsedTime(2008, September, 17, 13, 4, 26, 300000000, Wednesday, -7 * 60 * 60, "PDT"u8))
}.slice();

internal static bool same(Δtime.Time t, ж<parsedTime> Ꮡu) {
    ref var u = ref Ꮡu.DerefOrNull();

    // Check aggregates.
    var (year, month, day) = t.Date();
    var (hour, min, sec) = t.Clock();
    var (name, offset) = t.Zone();
    if (year != u.Year || month != u.Month || day != u.Day || hour != u.Hour || min != u.Minute || sec != u.Second || name != u.Zone || offset != u.ZoneOffset) {
        return false;
    }
    // Check individual entries.
    return t.Year() == u.Year && t.Month() == u.Month && t.Day() == u.Day && t.Hour() == u.Hour && t.Minute() == u.Minute && t.Second() == u.Second && t.Nanosecond() == u.Nanosecond && t.Weekday() == u.Weekday;
}

public static void TestUnixUTC(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in utctests) {
        ref var test = ref heap(new TimeTest(), out var Ꮡtest);
        test = vᴛ1;

        var sec = test.seconds;
        var golden = Ꮡtest.of(TimeTest.Ꮡgolden);
        var tm = Unix(sec, 0).UTC();
        var newsec = tm.Unix();
        if (newsec != sec) {
            Ꮡt.Errorf("Unix(%d, 0).Unix() = %d"u8, sec, newsec);
        }
        if (!same(tm, golden)) {
            Ꮡt.Errorf("Unix(%d, 0):  // %#v"u8, sec, tm);
            Ꮡt.Errorf("  want=%+v"u8, golden.Value);
            Ꮡt.Errorf("  have=%v"u8, tm.Format(RFC3339 + " MST"));
        }
    }
}

public static void TestUnixNanoUTC(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in nanoutctests) {
        ref var test = ref heap(new TimeTest(), out var Ꮡtest);
        test = vᴛ1;

        var golden = Ꮡtest.of(TimeTest.Ꮡgolden);
        var nsec = test.seconds * 1000000000 + (int64)(~golden).Nanosecond;
        var tm = Unix(0, nsec).UTC();
        var newnsec = tm.Unix() * 1000000000 + (int64)tm.Nanosecond();
        if (newnsec != nsec) {
            Ꮡt.Errorf("Unix(0, %d).Nanoseconds() = %d"u8, nsec, newnsec);
        }
        if (!same(tm, golden)) {
            Ꮡt.Errorf("Unix(0, %d):"u8, nsec);
            Ꮡt.Errorf("  want=%+v"u8, golden.Value);
            Ꮡt.Errorf("  have=%+v"u8, tm.Format(RFC3339 + " MST"));
        }
    }
}

public static void TestUnix(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in localtests) {
        ref var test = ref heap(new TimeTest(), out var Ꮡtest);
        test = vᴛ1;

        var sec = test.seconds;
        var golden = Ꮡtest.of(TimeTest.Ꮡgolden);
        var tm = Unix(sec, 0);
        var newsec = tm.Unix();
        if (newsec != sec) {
            Ꮡt.Errorf("Unix(%d, 0).Seconds() = %d"u8, sec, newsec);
        }
        if (!same(tm, golden)) {
            Ꮡt.Errorf("Unix(%d, 0):"u8, sec);
            Ꮡt.Errorf("  want=%+v"u8, golden.Value);
            Ꮡt.Errorf("  have=%+v"u8, tm.Format(RFC3339 + " MST"));
        }
    }
}

public static void TestUnixNano(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in nanolocaltests) {
        ref var test = ref heap(new TimeTest(), out var Ꮡtest);
        test = vᴛ1;

        var golden = Ꮡtest.of(TimeTest.Ꮡgolden);
        var nsec = test.seconds * 1000000000 + (int64)(~golden).Nanosecond;
        var tm = Unix(0, nsec);
        var newnsec = tm.Unix() * 1000000000 + (int64)tm.Nanosecond();
        if (newnsec != nsec) {
            Ꮡt.Errorf("Unix(0, %d).Seconds() = %d"u8, nsec, newnsec);
        }
        if (!same(tm, golden)) {
            Ꮡt.Errorf("Unix(0, %d):"u8, nsec);
            Ꮡt.Errorf("  want=%+v"u8, golden.Value);
            Ꮡt.Errorf("  have=%+v"u8, tm.Format(RFC3339 + " MST"));
        }
    }
}

public static void TestUnixUTCAndBack(ж<Δtesting.T> Ꮡt) {
    var f = (int64 sec) => Unix(sec, 0).UTC().Unix() == sec;
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

public static void TestUnixNanoUTCAndBack(ж<Δtesting.T> Ꮡt) {
    var f = (int64 nsec) => {
        var tΔ1 = Unix(0, nsec).UTC();
        var ns = tΔ1.Unix() * 1000000000 + (int64)tΔ1.Nanosecond();
        return ns == nsec;
    };
    var fʗ1 = f;
    var f32 = (int32 nsec) => fʗ1((int64)nsec);
    var cfg = Ꮡ(new quick.Config(MaxCount: 10000));
    // Try a small date first, then the large ones. (The span is only a few hundred years
    // for nanoseconds in an int64.)
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

public static void TestUnixMilli(ж<Δtesting.T> Ꮡt) {
    var f = (int64 msec) => {
        var tΔ1 = UnixMilli(msec);
        return tΔ1.UnixMilli() == msec;
    };
    var cfg = Ꮡ(new quick.Config(MaxCount: 10000));
    {
        var err = quick.Check((f).OrTypedNilFunc(), cfg); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

public static void TestUnixMicro(ж<Δtesting.T> Ꮡt) {
    var f = (int64 usec) => {
        var tΔ1 = UnixMicro(usec);
        return tΔ1.UnixMicro() == usec;
    };
    var cfg = Ꮡ(new quick.Config(MaxCount: 10000));
    {
        var err = quick.Check((f).OrTypedNilFunc(), cfg); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

// The time routines provide no way to get absolute time
// (seconds since zero), but we need it to compute the right
// answer for bizarre roundings like "to the nearest 3 ns".
// Compute as t - year1 = (t - 1970) + (1970 - 2001) + (2001 - 1).
// t - 1970 is returned by Unix and Nanosecond.
// 1970 - 2001 is -(31*365+8)*86400 = -978307200 seconds.
// 2001 - 1 is 2000*365.2425*86400 = 63113904000 seconds.
internal static UntypedInt unixToZero => /* -978307200 + 63113904000 */ 62135596800;

// abs returns the absolute time stored in t, as seconds and nanoseconds.
internal static (int64 sec, int64 nsec) abs(Δtime.Time t) {
    var unix = t.Unix();
    nint nano = t.Nanosecond();
    return (unix + (int64)unixToZero, (int64)nano);
}

// absString returns abs as a decimal string.
internal static @string absString(Δtime.Time t) {
    var (sec, nsec) = abs(t);
    if (sec < 0) {
        sec = -sec;
        nsec = -nsec;
        if (nsec < 0) {
            nsec += 1000000000;
            sec--;
        }
        return fmt.Sprintf("-%d%09d"u8, sec, nsec);
    }
    return fmt.Sprintf("%d%09d"u8, sec, nsec);
}

// 5.8*d rounds to 6*d, but .8*d+.8*d < 0 < d

partial struct truncateRoundTestsᴛ1 /*dyn*/ {
    internal Δtime.Time t;
    internal Δtime.Duration d;
}
internal static slice<truncateRoundTestsᴛ1> truncateRoundTests = new truncateRoundTestsᴛ1[]{
    new(Date(-1, January, 1, 12, 15, 30, 500000000, ΔUTC), 3),
    new(Date(-1, January, 1, 12, 15, 31, 500000000, ΔUTC), 3),
    new(Date(2012, January, 1, 12, 15, 30, 500000000, ΔUTC), ΔSecond),
    new(Date(2012, January, 1, 12, 15, 31, 500000000, ΔUTC), ΔSecond),
    new(Unix(-19012425939L, 649146258), 7435029458905025217L)
}.slice();

public static void TestTruncateRound(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ж<bigꓸInt> bsec = @new<bigꓸInt>();
    ж<bigꓸInt> bnsec = @new<bigꓸInt>();
    ж<bigꓸInt> bd = @new<bigꓸInt>();
    ж<bigꓸInt> bt = @new<bigꓸInt>();
    ж<bigꓸInt> br = @new<bigꓸInt>();
    ж<bigꓸInt> bq = @new<bigꓸInt>();
    ж<bigꓸInt> b1e9 = @new<bigꓸInt>();
    b1e9.SetInt64(1000000000);
    var b1e9ʗ1 = b1e9;
    var bdʗ1 = bd;
    var bnsecʗ1 = bnsec;
    var bqʗ1 = bq;
    var brʗ1 = br;
    var bsecʗ1 = bsec;
    var btʗ1 = bt;
    bool testOne(int64 ti, int64 tns, int64 di) {
        Ꮡt.Helper();
        var t0 = Unix(ti, tns).UTC();
        var d = ((Δtime.Duration)di);
        if (d < 0) {
            d = -d;
        }
        if (d <= 0) {
            d = 1;
        }
        // Compute bt = absolute nanoseconds.
        var (sec, nsec) = abs(t0);
        bsecʗ1.SetInt64(sec);
        bnsecʗ1.SetInt64(nsec);
        btʗ1.Mul(bsecʗ1, b1e9ʗ1);
        btʗ1.Add(btʗ1, bnsecʗ1);
        // Compute quotient and remainder mod d.
        bdʗ1.SetInt64((int64)d);
        bqʗ1.DivMod(btʗ1, bdʗ1, brʗ1);
        // To truncate, subtract remainder.
        // br is < d, so it fits in an int64.
        var r = brʗ1.Int64();
        var t1 = t0.Add(-((Δtime.Duration)r));
        // Check that time.Truncate works.
        {
            var trunc = t0.Truncate(d); if (trunc != t1) {
                Ꮡt.Errorf("Time.Truncate(%s, %s) = %s, want %s\n"u8 + "%v trunc %v =\n%v want\n%v"u8,
                    t0.Format(RFC3339Nano), d, trunc, t1.Format(RFC3339Nano),
                    absString(t0), (int64)d, absString(trunc), absString(t1));
                return false;
            }
        }
        // To round, add d back if remainder r > d/2 or r == exactly d/2.
        // The commented out code would round half to even instead of up,
        // but that makes it time-zone dependent, which is a bit strange.
        if (r > (int64)d / 2 || r + r == (int64)d) {
            /*&& bq.Bit(0) == 1*/
            t1 = t1.Add(d);
        }
        // Check that time.Round works.
        {
            var rnd = t0.Round(d); if (rnd != t1) {
                Ꮡt.Errorf("Time.Round(%s, %s) = %s, want %s\n"u8 + "%v round %v =\n%v want\n%v"u8,
                    t0.Format(RFC3339Nano), d, rnd, t1.Format(RFC3339Nano),
                    absString(t0), (int64)d, absString(rnd), absString(t1));
                return false;
            }
        }
        return true;
    }
    // manual test cases
    foreach (var (_, tt) in truncateRoundTests) {
        testOne(tt.t.Unix(), (int64)tt.t.Nanosecond(), (int64)tt.d);
    }
    // exhaustive near 0
    for (nint i = 0; i < 100; i++) {
        for (nint j = 1; j < 100; j++) {
            testOne(unixToZero, (int64)i, (int64)j);
            testOne(unixToZero, -(int64)i, (int64)j);
            if (Ꮡt.Failed()) {
                return;
            }
        }
    }
    if (Ꮡt.Failed()) {
        return;
    }
    // randomly generated test cases
    var cfg = Ꮡ(new quick.Config(MaxCount: 100000));
    if (Δtesting.Short()) {
        cfg.Value.MaxCount = 1000;
    }
    // divisors of Second
    var testOneʗ1 = testOne;
    var f1 = (int64 ti, int32 tns, int32 logdi) => {
        var d = ((Δtime.Duration)1);
        nuint a = (nuint)(logdi % 9);
        var b = ((logdi >> (int)(16))) % 9;
        d <<= (int)(a);
        for (nint i = 0; i < (nint)b; i++) {
            d *= 5;
        }
        // Make room for unix ↔ internal conversion.
        // We don't care about behavior too close to ± 2^63 Unix seconds.
        // It is full of wraparounds but will never happen in a reasonable program.
        // (Or maybe not? See go.dev/issue/20678. In any event, they're not handled today.)
        ti >>= (int)(1);
        return testOneʗ1(ti, (int64)tns, (int64)d);
    };
    quick.Check((f1).OrTypedNilFunc(), cfg);
    // multiples of Second
    var testOneʗ2 = testOne;
    var f2 = (int64 ti, int32 tns, int32 di) => {
        var d = ((Δtime.Duration)(int64)di) * ΔSecond;
        if (d < 0) {
            d = -d;
        }
        ti >>= (int)(1); // see comment in f1
        return testOneʗ2(ti, (int64)tns, (int64)d);
    };
    quick.Check((f2).OrTypedNilFunc(), cfg);
    // halfway cases
    var testOneʗ3 = testOne;
    var f3 = (int64 tns, int64 di) => {
        di &= (int64)(0xfffffffeL);
        if (di == 0) {
            di = 2;
        }
        tns -= rem(tns, di);
        if (tns < 0){
            tns += di / 2;
        } else {
            tns -= di / 2;
        }
        return testOneʗ3(0, tns, di);
    };
    quick.Check((f3).OrTypedNilFunc(), cfg);
    // full generality
    var testOneʗ4 = testOne;
    var f4 = (int64 ti, int32 tns, int64 di) => {
        ti >>= (int)(1); // see comment in f1
        return testOneʗ4(ti, (int64)tns, di);
    };
    quick.Check((f4).OrTypedNilFunc(), cfg);
}

partial struct ISOWeekTest {
    internal nint year; // year
    internal nint month, day; // month and day
    internal nint yex; // expected year
    internal nint wex; // expected week
}

internal static slice<ISOWeekTest> isoWeekTests = new ISOWeekTest[]{
    new(1981, 1, 1, 1981, 1), new(1982, 1, 1, 1981, 53), new(1983, 1, 1, 1982, 52),
    new(1984, 1, 1, 1983, 52), new(1985, 1, 1, 1985, 1), new(1986, 1, 1, 1986, 1),
    new(1987, 1, 1, 1987, 1), new(1988, 1, 1, 1987, 53), new(1989, 1, 1, 1988, 52),
    new(1990, 1, 1, 1990, 1), new(1991, 1, 1, 1991, 1), new(1992, 1, 1, 1992, 1),
    new(1993, 1, 1, 1992, 53), new(1994, 1, 1, 1993, 52), new(1995, 1, 2, 1995, 1),
    new(1996, 1, 1, 1996, 1), new(1996, 1, 7, 1996, 1), new(1996, 1, 8, 1996, 2),
    new(1997, 1, 1, 1997, 1), new(1998, 1, 1, 1998, 1), new(1999, 1, 1, 1998, 53),
    new(2000, 1, 1, 1999, 52), new(2001, 1, 1, 2001, 1), new(2002, 1, 1, 2002, 1),
    new(2003, 1, 1, 2003, 1), new(2004, 1, 1, 2004, 1), new(2005, 1, 1, 2004, 53),
    new(2006, 1, 1, 2005, 52), new(2007, 1, 1, 2007, 1), new(2008, 1, 1, 2008, 1),
    new(2009, 1, 1, 2009, 1), new(2010, 1, 1, 2009, 53), new(2010, 1, 1, 2009, 53),
    new(2011, 1, 1, 2010, 52), new(2011, 1, 2, 2010, 52), new(2011, 1, 3, 2011, 1),
    new(2011, 1, 4, 2011, 1), new(2011, 1, 5, 2011, 1), new(2011, 1, 6, 2011, 1),
    new(2011, 1, 7, 2011, 1), new(2011, 1, 8, 2011, 1), new(2011, 1, 9, 2011, 1),
    new(2011, 1, 10, 2011, 2), new(2011, 1, 11, 2011, 2), new(2011, 6, 12, 2011, 23),
    new(2011, 6, 13, 2011, 24), new(2011, 12, 25, 2011, 51), new(2011, 12, 26, 2011, 52),
    new(2011, 12, 27, 2011, 52), new(2011, 12, 28, 2011, 52), new(2011, 12, 29, 2011, 52),
    new(2011, 12, 30, 2011, 52), new(2011, 12, 31, 2011, 52), new(1995, 1, 1, 1994, 52),
    new(2012, 1, 1, 2011, 52), new(2012, 1, 2, 2012, 1), new(2012, 1, 8, 2012, 1),
    new(2012, 1, 9, 2012, 2), new(2012, 12, 23, 2012, 51), new(2012, 12, 24, 2012, 52),
    new(2012, 12, 30, 2012, 52), new(2012, 12, 31, 2013, 1), new(2013, 1, 1, 2013, 1),
    new(2013, 1, 6, 2013, 1), new(2013, 1, 7, 2013, 2), new(2013, 12, 22, 2013, 51),
    new(2013, 12, 23, 2013, 52), new(2013, 12, 29, 2013, 52), new(2013, 12, 30, 2014, 1),
    new(2014, 1, 1, 2014, 1), new(2014, 1, 5, 2014, 1), new(2014, 1, 6, 2014, 2),
    new(2015, 1, 1, 2015, 1), new(2016, 1, 1, 2015, 53), new(2017, 1, 1, 2016, 52),
    new(2018, 1, 1, 2018, 1), new(2019, 1, 1, 2019, 1), new(2020, 1, 1, 2020, 1),
    new(2021, 1, 1, 2020, 53), new(2022, 1, 1, 2021, 52), new(2023, 1, 1, 2022, 52),
    new(2024, 1, 1, 2024, 1), new(2025, 1, 1, 2025, 1), new(2026, 1, 1, 2026, 1),
    new(2027, 1, 1, 2026, 53), new(2028, 1, 1, 2027, 52), new(2029, 1, 1, 2029, 1),
    new(2030, 1, 1, 2030, 1), new(2031, 1, 1, 2031, 1), new(2032, 1, 1, 2032, 1),
    new(2033, 1, 1, 2032, 53), new(2034, 1, 1, 2033, 52), new(2035, 1, 1, 2035, 1),
    new(2036, 1, 1, 2036, 1), new(2037, 1, 1, 2037, 1), new(2038, 1, 1, 2037, 53),
    new(2039, 1, 1, 2038, 52), new(2040, 1, 1, 2039, 52)
}.slice();

public static void TestISOWeek(ж<Δtesting.T> Ꮡt) {
    // Selected dates and corner cases
    foreach (var (_, wt) in isoWeekTests) {
        var dt = Date(wt.year, ((timeꓸMonth)wt.month), wt.day, 0, 0, 0, 0, ΔUTC);
        var (y, w) = dt.ISOWeek();
        if (w != wt.wex || y != wt.yex) {
            Ꮡt.Errorf("got %d/%d; expected %d/%d for %d-%02d-%02d"u8,
                y, w, wt.yex, wt.wex, wt.year, wt.month, wt.day);
        }
    }
    // The only real invariant: Jan 04 is in week 1
    for (nint year = 1950; year < 2100; year++) {
        {
            var (y, w) = Date(year, January, 4, 0, 0, 0, 0, ΔUTC).ISOWeek(); if (y != year || w != 1) {
                Ꮡt.Errorf("got %d/%d; expected %d/1 for Jan 04"u8, y, w, year);
            }
        }
    }
}

partial struct YearDayTest {
    internal nint year, month, day;
    internal nint yday;
}

// Non-leap-year tests
// Leap-year tests
// Looks like leap-year (but isn't) tests
// Year one tests (non-leap)
// Year minus one tests (non-leap)
// 400 BC tests (leap-year)
// Special Cases
// Gregorian calendar change (no effect)
// Test YearDay in several different scenarios
// and corner cases
internal static slice<YearDayTest> yearDayTests = new YearDayTest[]{
    new(2007, 1, 1, 1),
    new(2007, 1, 15, 15),
    new(2007, 2, 1, 32),
    new(2007, 2, 15, 46),
    new(2007, 3, 1, 60),
    new(2007, 3, 15, 74),
    new(2007, 4, 1, 91),
    new(2007, 12, 31, 365),
    new(2008, 1, 1, 1),
    new(2008, 1, 15, 15),
    new(2008, 2, 1, 32),
    new(2008, 2, 15, 46),
    new(2008, 3, 1, 61),
    new(2008, 3, 15, 75),
    new(2008, 4, 1, 92),
    new(2008, 12, 31, 366),
    new(1900, 1, 1, 1),
    new(1900, 1, 15, 15),
    new(1900, 2, 1, 32),
    new(1900, 2, 15, 46),
    new(1900, 3, 1, 60),
    new(1900, 3, 15, 74),
    new(1900, 4, 1, 91),
    new(1900, 12, 31, 365),
    new(1, 1, 1, 1),
    new(1, 1, 15, 15),
    new(1, 2, 1, 32),
    new(1, 2, 15, 46),
    new(1, 3, 1, 60),
    new(1, 3, 15, 74),
    new(1, 4, 1, 91),
    new(1, 12, 31, 365),
    new(-1, 1, 1, 1),
    new(-1, 1, 15, 15),
    new(-1, 2, 1, 32),
    new(-1, 2, 15, 46),
    new(-1, 3, 1, 60),
    new(-1, 3, 15, 74),
    new(-1, 4, 1, 91),
    new(-1, 12, 31, 365),
    new(-400, 1, 1, 1),
    new(-400, 1, 15, 15),
    new(-400, 2, 1, 32),
    new(-400, 2, 15, 46),
    new(-400, 3, 1, 61),
    new(-400, 3, 15, 75),
    new(-400, 4, 1, 92),
    new(-400, 12, 31, 366),
    new(1582, 10, 4, 277),
    new(1582, 10, 15, 288)
}.slice();

// Check to see if YearDay is location sensitive
internal static slice<ж<timeꓸLocation>> yearDayLocations = new ж<timeꓸLocation>[]{
    FixedZone("UTC-8"u8, -8 * 60 * 60),
    FixedZone("UTC-4"u8, -4 * 60 * 60),
    ΔUTC,
    FixedZone("UTC+4"u8, 4 * 60 * 60),
    FixedZone("UTC+8"u8, 8 * 60 * 60)
}.slice();

public static void TestYearDay(ж<Δtesting.T> Ꮡt) {
    foreach (var (i, loc) in yearDayLocations) {
        foreach (var (_, ydt) in yearDayTests) {
            var dt = Date(ydt.year, ((timeꓸMonth)ydt.month), ydt.day, 0, 0, 0, 0, loc);
            nint yday = dt.YearDay();
            if (yday != ydt.yday) {
                Ꮡt.Errorf("Date(%d-%02d-%02d in %v).YearDay() = %d, want %d"u8,
                    ydt.year, ydt.month, ydt.day, loc.OrTypedNil(), yday, ydt.yday);
                continue;
            }
            if (ydt.year < 0 || ydt.year > 9999) {
                continue;
            }
            @string f = fmt.Sprintf("%04d-%02d-%02d %03d %+.2d00"u8,
                ydt.year, ydt.month, ydt.day, ydt.yday, (i - 2) * 4);
            var (dt1, err) = Parse("2006-01-02 002 -0700"u8, f);
            if (err != default!) {
                Ꮡt.Errorf(@"Parse(""2006-01-02 002 -0700"", %q): %v"u8, f, err);
                continue;
            }
            if (!dt1.Equal(dt)) {
                Ꮡt.Errorf(@"Parse(""2006-01-02 002 -0700"", %q) = %v, want %v"u8, f, dt1, dt);
            }
        }
    }
}


partial struct durationTestsᴛ1 /*dyn*/ {
    internal @string str;
    internal Δtime.Duration d;
}
internal static slice<durationTestsᴛ1> durationTests = new durationTestsᴛ1[]{
    new("0s"u8, 0),
    new("1ns"u8, 1 * ΔNanosecond),
    new("1.1µs"u8, 1100 * ΔNanosecond),
    new("2.2ms"u8, 2200 * Microsecond),
    new("3.3s"u8, (Δtime.Duration)(3300000000L)),
    new("4m5s"u8, (Δtime.Duration)(245000000000L)),
    new("4m5.001s"u8, (Δtime.Duration)(245001000000L)),
    new("5h6m7.001s"u8, (Δtime.Duration)(18367001000000L)),
    new("8m0.000000001s"u8, (Δtime.Duration)(480000000001L)),
    new("2562047h47m16.854775807s"u8, (Δtime.Duration)(9223372036854775807L)),
    new("-2562047h47m16.854775808s"u8, (Δtime.Duration)(-9223372036854775808L))
}.slice();

public static void TestDurationString(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in durationTests) {
        {
            @string str = tt.d.String(); if (str != tt.str) {
                Ꮡt.Errorf("Duration(%d).String() = %s, want %s"u8, (int64)tt.d, str, tt.str);
            }
        }
        if (tt.d > 0) {
            {
                @string str = (-tt.d).String(); if (str != "-"u8 + tt.str) {
                    Ꮡt.Errorf("Duration(%d).String() = %s, want %s"u8, (int64)(-tt.d), str, "-" + tt.str);
                }
            }
        }
    }
}

// 1:00:00 PDT
// 1:59:59 PDT
// 2:00:00 PST
// 1:00:00 PST
// 1:59:59 PST
// 3:00:00 PDT
// 2:30:00 PDT ≡ 1:30 PST
// Leap year
// Many names for Fri Nov 18 7:56:35 PST 2011
// Nov 18 7:56:35
// Nov 19 -17:56:35
// Nov 17 31:56:35
// Nov 18 6:116:35
// Oct 49 7:56:35
// Nov 18 7:55:95
// Nov 18 7:56:34 + 10⁹ns
// Dec -21 7:56:35
// Jan -52 7:56:35 2012
// (Jan-2) 18 7:56:35 2012
// (Dec+11) 18 7:56:35 2010
// large number of days
// negative Unix time

partial struct dateTestsᴛ1 /*dyn*/ {
    internal nint year, month, day, hour, min, sec, nsec;
    internal ж<timeꓸLocation> z;
    internal int64 unix;
}
internal static slice<dateTestsᴛ1> dateTests = new dateTestsᴛ1[]{
    new(2011, 11, 6, 1, 0, 0, 0, ΔLocal, 1320566400),
    new(2011, 11, 6, 1, 59, 59, 0, ΔLocal, 1320569999),
    new(2011, 11, 6, 2, 0, 0, 0, ΔLocal, 1320573600),
    new(2011, 3, 13, 1, 0, 0, 0, ΔLocal, 1300006800),
    new(2011, 3, 13, 1, 59, 59, 0, ΔLocal, 1300010399),
    new(2011, 3, 13, 3, 0, 0, 0, ΔLocal, 1300010400),
    new(2011, 3, 13, 2, 30, 0, 0, ΔLocal, 1300008600),
    new(2012, 12, 24, 0, 0, 0, 0, ΔLocal, 1356336000),
    new(2011, 11, 18, 7, 56, 35, 0, ΔLocal, 1321631795),
    new(2011, 11, 19, -17, 56, 35, 0, ΔLocal, 1321631795),
    new(2011, 11, 17, 31, 56, 35, 0, ΔLocal, 1321631795),
    new(2011, 11, 18, 6, 116, 35, 0, ΔLocal, 1321631795),
    new(2011, 10, 49, 7, 56, 35, 0, ΔLocal, 1321631795),
    new(2011, 11, 18, 7, 55, 95, 0, ΔLocal, 1321631795),
    new(2011, 11, 18, 7, 56, 34, 1000000000, ΔLocal, 1321631795),
    new(2011, 12, -12, 7, 56, 35, 0, ΔLocal, 1321631795),
    new(2012, 1, -43, 7, 56, 35, 0, ΔLocal, 1321631795),
    new(2012, (nint)(January - 2), 18, 7, 56, 35, 0, ΔLocal, 1321631795),
    new(2010, (nint)(December + 11), 18, 7, 56, 35, 0, ΔLocal, 1321631795),
    new(1970, 1, 15297, 7, 56, 35, 0, ΔLocal, 1321631795),
    new(1970, 1, -25508, 0, 0, 0, 0, ΔLocal, -2203948800L)
}.slice();

public static void TestDate(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in dateTests) {
        var time = Date(tt.year, ((timeꓸMonth)tt.month), tt.day, tt.hour, tt.min, tt.sec, tt.nsec, tt.z);
        var want = Unix(tt.unix, 0);
        if (!time.Equal(want)) {
            Ꮡt.Errorf("Date(%d, %d, %d, %d, %d, %d, %d, %s) = %v, want %v"u8,
                tt.year, tt.month, tt.day, tt.hour, tt.min, tt.sec, tt.nsec, tt.z.OrTypedNil(),
                time, want);
        }
    }
}

// Several ways of getting from
// Fri Nov 18 7:56:35 PST 2011
// to
// Thu Mar 19 7:56:35 PST 2016

partial struct addDateTestsᴛ1 /*dyn*/ {
    internal nint years, months, days;
}
internal static slice<addDateTestsᴛ1> addDateTests = new addDateTestsᴛ1[]{
    new(4, 4, 1),
    new(3, 16, 1),
    new(3, 15, 30),
    new(5, -6, -18 - 30 - 12)
}.slice();

public static void TestAddDate(ж<Δtesting.T> Ꮡt) {
    var t0 = Date(2011, 11, 18, 7, 56, 35, 0, ΔUTC);
    var t1 = Date(2016, 3, 19, 7, 56, 35, 0, ΔUTC);
    foreach (var (_, at) in addDateTests) {
        var time = t0.AddDate(at.years, at.months, at.days);
        if (!time.Equal(t1)) {
            Ꮡt.Errorf("AddDate(%d, %d, %d) = %v, want %v"u8,
                at.years, at.months, at.days,
                time, t1);
        }
    }
    var t2 = Date(1899, 12, 31, 0, 0, 0, 0, ΔUTC);
    var days = t2.Unix() / (24 * 60 * 60);
    var t3 = Unix(0, 0).AddDate(0, 0, (nint)days);
    if (!t2.Equal(t3)) {
        Ꮡt.Errorf("Adddate(0, 0, %d) = %v, want %v"u8, days, t3, t2);
    }
}

// January, first month, 31 days
// February, non-leap year, 28 days
// February, leap year, 29 days
// June, 30 days
// December, last month, 31 days

partial struct daysInTestsᴛ1 /*dyn*/ {
    internal nint year, month, di;
}
internal static slice<daysInTestsᴛ1> daysInTests = new daysInTestsᴛ1[]{
    new(2011, 1, 31),
    new(2011, 2, 28),
    new(2012, 2, 29),
    new(2011, 6, 30),
    new(2011, 12, 31)
}.slice();

public static void TestDaysIn(ж<Δtesting.T> Ꮡt) {
    // The daysIn function is not exported.
    // Test the daysIn function via the `var DaysIn = daysIn`
    // statement in the internal_test.go file.
    foreach (var (_, tt) in daysInTests) {
        nint di = time_internal_test_package.DaysIn(((timeꓸMonth)tt.month), tt.year);
        if (di != tt.di) {
            Ꮡt.Errorf("got %d; expected %d for %d-%02d"u8,
                di, tt.di, tt.year, tt.month);
        }
    }
}

public static void TestAddToExactSecond(ж<Δtesting.T> Ꮡt) {
    // Add an amount to the current time to round it up to the next exact second.
    // This test checks that the nsec field still lies within the range [0, 999999999].
    var t1 = Now();
    var t2 = t1.Add(ΔSecond - ((Δtime.Duration)(int64)t1.Nanosecond()));
    nint sec = (t1.Second() + 1) % 60;
    if (t2.Second() != sec || t2.Nanosecond() != 0) {
        Ꮡt.Errorf("sec = %d, nsec = %d, want sec = %d, nsec = 0"u8, t2.Second(), t2.Nanosecond(), sec);
    }
}

internal static bool equalTimeAndZone(Δtime.Time a, Δtime.Time b) {
    var (aname, aoffset) = a.Zone();
    var (bname, boffset) = b.Zone();
    return a.Equal(b) && aoffset == boffset && aname == bname;
}

// Time.sec: 0x0123456789ABCDEF
// nil location
internal static slice<Δtime.Time> gobTests = new Δtime.Time[]{
    Date(0, 1, 2, 3, 4, 5, 6, ΔUTC),
    Date(7, 8, 9, 10, 11, 12, 13, FixedZone(""u8, 0)),
    Unix(81985467080890095L, 0x76543210),
    new(),
    Date(1, 2, 3, 4, 5, 6, 7, FixedZone(""u8, 32767 * 60)),
    Date(1, 2, 3, 4, 5, 6, 7, FixedZone(""u8, -32768 * 60))
}.slice();

public static void TestTimeGob(ж<Δtesting.T> Ꮡt) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var enc = gob.NewEncoder(new time_test_package.bytes_BufferжWriter(Ꮡb));
    var dec = gob.NewDecoder(new time_test_package.bytes_BufferжReader(Ꮡb));
    foreach (var (_, vᴛ1) in gobTests) {
        ref var tt = ref heap(new Δtime.Time(), out var Ꮡtt);
        tt = vᴛ1;

        ref var gobtt = ref heap(new Δtime.Time(), out var Ꮡgobtt);
        {
            var err = enc.Encode(Ꮡtt); if (err != default!){
                Ꮡt.Errorf("%v gob Encode error = %q, want nil"u8, tt, err);
            } else 
            {
                var errΔ1 = dec.Decode(Ꮡgobtt); if (errΔ1 != default!){
                    Ꮡt.Errorf("%v gob Decode error = %q, want nil"u8, tt, errΔ1);
                } else 
                if (!equalTimeAndZone(gobtt, tt)) {
                    Ꮡt.Errorf("Decoded time = %v, want %v"u8, gobtt, tt);
                }
            }
        }
        b.Reset();
    }
}


partial struct invalidEncodingTestsᴛ1 /*dyn*/ {
    internal slice<byte> bytes;
    internal @string want;
}
internal static slice<invalidEncodingTestsᴛ1> invalidEncodingTests = new invalidEncodingTestsᴛ1[]{
    new(new byte[]{}.slice(), "Time.UnmarshalBinary: no data"u8),
    new(new byte[]{0, 2, 3}.slice(), "Time.UnmarshalBinary: unsupported version"u8),
    new(new byte[]{1, 2, 3}.slice(), "Time.UnmarshalBinary: invalid length"u8)
}.slice();

public static void TestInvalidTimeGob(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in invalidEncodingTests) {
        Δtime.Time ignored = default!;
        var err = ignored.GobDecode(tt.bytes);
        if (err == default! || err.Error() != tt.want) {
            Ꮡt.Errorf("time.GobDecode(%#v) error = %v, want %v"u8, tt.bytes, err, tt.want);
        }
        err = ignored.UnmarshalBinary(tt.bytes);
        if (err == default! || err.Error() != tt.want) {
            Ꮡt.Errorf("time.UnmarshalBinary(%#v) error = %v, want %v"u8, tt.bytes, err, tt.want);
        }
    }
}


partial struct notEncodableTimesᴛ1 /*dyn*/ {
    internal Δtime.Time time;
    internal @string want;
}
internal static slice<notEncodableTimesᴛ1> notEncodableTimes = new notEncodableTimesᴛ1[]{
    new(Date(0, 1, 2, 3, 4, 5, 6, FixedZone(""u8, -1 * 60)), "Time.MarshalBinary: unexpected zone offset"u8),
    new(Date(0, 1, 2, 3, 4, 5, 6, FixedZone(""u8, -32769 * 60)), "Time.MarshalBinary: unexpected zone offset"u8),
    new(Date(0, 1, 2, 3, 4, 5, 6, FixedZone(""u8, 32768 * 60)), "Time.MarshalBinary: unexpected zone offset"u8)
}.slice();

public static void TestNotGobEncodableTime(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in notEncodableTimes) {
        var (_, err) = tt.time.GobEncode();
        if (err == default! || err.Error() != tt.want) {
            Ꮡt.Errorf("%v GobEncode error = %v, want %v"u8, tt.time, err, tt.want);
        }
        (_, err) = tt.time.MarshalBinary();
        if (err == default! || err.Error() != tt.want) {
            Ꮡt.Errorf("%v MarshalBinary error = %v, want %v"u8, tt.time, err, tt.want);
        }
    }
}


partial struct jsonTestsᴛ1 /*dyn*/ {
    internal Δtime.Time time;
    internal @string json;
}
internal static slice<jsonTestsᴛ1> jsonTests = new jsonTestsᴛ1[]{
    new(Date(9999, 4, 12, 23, 20, 50, 520 * 1000000, ΔUTC), @"""9999-04-12T23:20:50.52Z"""u8),
    new(Date(1996, 12, 19, 16, 39, 57, 0, ΔLocal), @"""1996-12-19T16:39:57-08:00"""u8),
    new(Date(0, 1, 1, 0, 0, 0, 1, FixedZone(""u8, 1 * 60)), @"""0000-01-01T00:00:00.000000001+00:01"""u8),
    new(Date(2020, 1, 1, 0, 0, 0, 0, FixedZone(""u8, 23 * 60 * 60 + 59 * 60)), @"""2020-01-01T00:00:00+23:59"""u8)
}.slice();

public static void TestTimeJSON(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in jsonTests) {
        ref var jsonTime = ref heap(new Δtime.Time(), out var ᏑjsonTime);
        {
            var (jsonBytes, err) = json.Marshal(tt.time); if (err != default!){
                Ꮡt.Errorf("%v json.Marshal error = %v, want nil"u8, tt.time, err);
            } else 
            if (((sstring)jsonBytes) != tt.json){
                Ꮡt.Errorf("%v JSON = %#q, want %#q"u8, tt.time, ((@string)jsonBytes), tt.json);
            } else 
            {
                err = json.Unmarshal(jsonBytes, ᏑjsonTime); if (err != default!){
                    Ꮡt.Errorf("%v json.Unmarshal error = %v, want nil"u8, tt.time, err);
                } else 
                if (!equalTimeAndZone(jsonTime, tt.time)) {
                    Ꮡt.Errorf("Unmarshaled time = %v, want %v"u8, jsonTime, tt.time);
                }
            }
        }
    }
}

internal partial struct TestUnmarshalInvalidTimes_tests /*dyn*/ {
    internal @string @in;
    internal @string want;
}

public static void TestUnmarshalInvalidTimes(ж<Δtesting.T> Ꮡt) {
    var tests = new TestUnmarshalInvalidTimes_tests[]{
        new(@"{}"u8, "Time.UnmarshalJSON: input is not a JSON string"u8),
        new(@"[]"u8, "Time.UnmarshalJSON: input is not a JSON string"u8),
        new(@"""2000-01-01T1:12:34Z"""u8, @"<nil>"u8),
        new(@"""2000-01-01T00:00:00,000Z"""u8, @"<nil>"u8),
        new(@"""2000-01-01T00:00:00+24:00"""u8, @"<nil>"u8),
        new(@"""2000-01-01T00:00:00+00:60"""u8, @"<nil>"u8),
        new(@"""2000-01-01T00:00:00+123:45"""u8, @"parsing time ""2000-01-01T00:00:00+123:45"" as ""2006-01-02T15:04:05Z07:00"": cannot parse ""+123:45"" as ""Z07:00"""u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        ref var ts = ref heap(new Δtime.Time(), out var Ꮡts);
        @string want = tt.want;
        var err = json.Unmarshal(slice<byte>(tt.@in), Ꮡts);
        if (fmt.Sprint(err) != want) {
            Ꮡt.Errorf("Time.UnmarshalJSON(%s) = %v, want %v"u8, tt.@in, err, want);
        }
        if (strings.HasPrefix(tt.@in, @""""u8) && strings.HasSuffix(tt.@in, @""""u8)) {
            err = ts.UnmarshalText(slice<byte>(strings.Trim(tt.@in, @""""u8)));
            if (fmt.Sprint(err) != want) {
                Ꮡt.Errorf("Time.UnmarshalText(%s) = %v, want %v"u8, tt.@in, err, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string jsonˢ = "JSON"u8;
internal static readonly @string textˢ = "Text"u8;
internal static readonly @string marshalJSONˢ = "MarshalJSON"u8;
internal static readonly @string appendTextˢ = "AppendText"u8;

public static void TestMarshalInvalidTimes(ж<Δtesting.T> Ꮡt) {
    var tests = new notEncodableTimesᴛ1[]{
        new(Date(10000, 1, 1, 0, 0, 0, 0, ΔUTC), "Time.MarshalJSON: year outside of range [0,9999]"u8),
        new(Date(-998, 1, 1, 0, 0, 0, 0, ΔUTC).Add(-ΔSecond), "Time.MarshalJSON: year outside of range [0,9999]"u8),
        new(Date(0, 1, 1, 0, 0, 0, 0, ΔUTC).Add(-ΔNanosecond), "Time.MarshalJSON: year outside of range [0,9999]"u8),
        new(Date(2020, 1, 1, 0, 0, 0, 0, FixedZone(""u8, 24 * 60 * 60)), "Time.MarshalJSON: timezone hour outside of range [0,23]"u8),
        new(Date(2020, 1, 1, 0, 0, 0, 0, FixedZone(""u8, 123 * 60 * 60)), "Time.MarshalJSON: timezone hour outside of range [0,23]"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        @string want = tt.want;
        var (b, err) = tt.time.MarshalJSON();
        switch (ᐧ) {
        case {} when b != default!: {
            Ꮡt.Errorf("(%v).MarshalText() = %q, want nil"u8, tt.time, b);
            break;
        }
        case {} when err == default! || err.Error() != want: {
            Ꮡt.Errorf("(%v).MarshalJSON() error = %v, want %v"u8, tt.time, err, want);
            break;
        }}

        want = strings.ReplaceAll(tt.want, jsonˢ, textˢ);
        (b, err) = tt.time.MarshalText();
        switch (ᐧ) {
        case {} when b != default!: {
            Ꮡt.Errorf("(%v).MarshalText() = %q, want nil"u8, tt.time, b);
            break;
        }
        case {} when err == default! || err.Error() != want: {
            Ꮡt.Errorf("(%v).MarshalText() error = %v, want %v"u8, tt.time, err, want);
            break;
        }}

        var buf = new slice<byte>(0, 64);
        want = strings.ReplaceAll(tt.want, marshalJSONˢ, appendTextˢ);
        (b, err) = tt.time.AppendText(buf);
        switch (ᐧ) {
        case {} when b != default!: {
            Ꮡt.Errorf("(%v).AppendText() = %q, want nil"u8, tt.time, b);
            break;
        }
        case {} when err == default! || err.Error() != want: {
            Ꮡt.Errorf("(%v).AppendText() error = %v, want %v"u8, tt.time, err, want);
            break;
        }}

    }
}

// simple
// sign
// decimal
// different units
// U+00B5
// U+03BC
// composite durations
// large value
// more than 9 digits after decimal point, see https://golang.org/issue/6617
// 9007199254740993 = 1<<53+1 cannot be stored precisely in a float64
// largest duration that can be represented by int64 in nanoseconds
// largest negative value
// largest negative round trip value, see https://golang.org/issue/48629
// huge string; issue 15011.
// This value tests the first overflow check in leadingFraction.

partial struct parseDurationTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal Δtime.Duration want;
}
internal static slice<parseDurationTestsᴛ1> parseDurationTests = new parseDurationTestsᴛ1[]{
    new("0"u8, 0),
    new("5s"u8, (Δtime.Duration)(5000000000L)),
    new("30s"u8, (Δtime.Duration)(30000000000L)),
    new("1478s"u8, (Δtime.Duration)(1478000000000L)),
    new("-5s"u8, (Δtime.Duration)(-5000000000L)),
    new("+5s"u8, (Δtime.Duration)(5000000000L)),
    new("-0"u8, 0),
    new("+0"u8, 0),
    new("5.0s"u8, (Δtime.Duration)(5000000000L)),
    new("5.6s"u8, (Δtime.Duration)(5600000000L)),
    new("5.s"u8, (Δtime.Duration)(5000000000L)),
    new(".5s"u8, 500 * Millisecond),
    new("1.0s"u8, 1 * ΔSecond),
    new("1.00s"u8, 1 * ΔSecond),
    new("1.004s"u8, 1 * ΔSecond + 4 * Millisecond),
    new("1.0040s"u8, 1 * ΔSecond + 4 * Millisecond),
    new("100.00100s"u8, (Δtime.Duration)(100001000000L)),
    new("10ns"u8, 10 * ΔNanosecond),
    new("11us"u8, 11 * Microsecond),
    new("12µs"u8, 12 * Microsecond),
    new("12μs"u8, 12 * Microsecond),
    new("13ms"u8, 13 * Millisecond),
    new("14s"u8, (Δtime.Duration)(14000000000L)),
    new("15m"u8, (Δtime.Duration)(900000000000L)),
    new("16h"u8, (Δtime.Duration)(57600000000000L)),
    new("3h30m"u8, (Δtime.Duration)(12600000000000L)),
    new("10.5s4m"u8, (Δtime.Duration)(250500000000L)),
    new("-2m3.4s"u8, -((Δtime.Duration)(123400000000L))),
    new("1h2m3s4ms5us6ns"u8, (Δtime.Duration)(3723004005006L)),
    new("39h9m14.425s"u8, (Δtime.Duration)(140954425000000L)),
    new("52763797000ns"u8, (Δtime.Duration)(52763797000L)),
    new("0.3333333333333333333h"u8, (Δtime.Duration)(1200000000000L)),
    new("9007199254740993ns"u8, (Δtime.Duration)(9007199254740993L)),
    new("9223372036854775807ns"u8, (Δtime.Duration)(9223372036854775807L)),
    new("9223372036854775.807us"u8, (Δtime.Duration)(9223372036854775807L)),
    new("9223372036s854ms775us807ns"u8, (Δtime.Duration)(9223372036854775807L)),
    new("-9223372036854775808ns"u8, (Δtime.Duration)(-9223372036854775808L)),
    new("-9223372036854775.808us"u8, (Δtime.Duration)(-9223372036854775808L)),
    new("-9223372036s854ms775us808ns"u8, (Δtime.Duration)(-9223372036854775808L)),
    new("-9223372036854775808ns"u8, (Δtime.Duration)(-9223372036854775808L)),
    new("-2562047h47m16.854775808s"u8, (Δtime.Duration)(-9223372036854775808L)),
    new("0.100000000000000000000h"u8, (Δtime.Duration)(360000000000L)),
    new("0.830103483285477580700h"u8, (Δtime.Duration)(2988372539827L))
}.slice();

public static void TestParseDuration(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tc) in parseDurationTests) {
        var (d, err) = ParseDuration(tc.@in);
        if (err != default! || d != tc.want) {
            Ꮡt.Errorf("ParseDuration(%q) = %v, %v, want %v, nil"u8, tc.@in, d, err, tc.want);
        }
    }
}

// invalid
// utf8.RuneError
// utf8.RuneError
// overflow

partial struct parseDurationErrorTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string expect;
}
internal static slice<parseDurationErrorTestsᴛ1> parseDurationErrorTests = new parseDurationErrorTestsᴛ1[]{
    new(""u8, @""""""u8),
    new("3"u8, @"""3"""u8),
    new("-"u8, @"""-"""u8),
    new("s"u8, @"""s"""u8),
    new("."u8, @"""."""u8),
    new("-."u8, @"""-."""u8),
    new(".s"u8, @""".s"""u8),
    new("+.s"u8, @"""+.s"""u8),
    new("1d"u8, @"""1d"""u8),
    new(((@string)(new byte[]{0x85, 0x85})), @"""\x85\x85"""u8),
    new(((@string)(new byte[]{0xff, 0x66, 0x66})), @"""\xffff"""u8),
    new(((@string)(new byte[]{0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0xff, 0x66, 0x66, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64})), @"""hello \xffff world"""u8),
    new("\uFFFD"u8, @"""\xef\xbf\xbd"""u8),
    new("\uFFFD hello \uFFFD world"u8, @"""\xef\xbf\xbd hello \xef\xbf\xbd world"""u8),
    new("9223372036854775810ns"u8, @"""9223372036854775810ns"""u8),
    new("9223372036854775808ns"u8, @"""9223372036854775808ns"""u8),
    new("-9223372036854775809ns"u8, @"""-9223372036854775809ns"""u8),
    new("9223372036854776us"u8, @"""9223372036854776us"""u8),
    new("3000000h"u8, @"""3000000h"""u8),
    new("9223372036854775.808us"u8, @"""9223372036854775.808us"""u8),
    new("9223372036854ms775us808ns"u8, @"""9223372036854ms775us808ns"""u8)
}.slice();

public static void TestParseDurationErrors(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tc) in parseDurationErrorTests) {
        var (_, err) = ParseDuration(tc.@in);
        if (err == default!){
            Ꮡt.Errorf("ParseDuration(%q) = _, nil, want _, non-nil"u8, tc.@in);
        } else 
        if (!strings.Contains(err.Error(), tc.expect)) {
            Ꮡt.Errorf("ParseDuration(%q) = _, %q, error does not contain %q"u8, tc.@in, err, tc.expect);
        }
    }
}

public static void TestParseDurationRoundTrip(ж<Δtesting.T> Ꮡt) {
    // https://golang.org/issue/48629
    var max0 = ((Δtime.Duration)Δmath.MaxInt64);
    var (max1, err) = ParseDuration(max0.String());
    if (err != default! || max0 != max1) {
        Ꮡt.Errorf("round-trip failed: %d => %q => %d, %v"u8, max0, max0.String(), max1, err);
    }
    var min0 = ((Δtime.Duration)Δmath.MinInt64);
    (var min1, err) = ParseDuration(min0.String());
    if (err != default! || min0 != min1) {
        Ꮡt.Errorf("round-trip failed: %d => %q => %d, %v"u8, min0, min0.String(), min1, err);
    }
    for (nint i = 0; i < 100; i++) {
        // Resolutions finer than milliseconds will result in
        // imprecise round-trips.
        var d0 = ((Δtime.Duration)(int64)rand.Int31()) * Millisecond;
        @string s = d0.String();
        var (d1, errΔ1) = ParseDuration(s);
        if (errΔ1 != default! || d0 != d1) {
            Ꮡt.Errorf("round-trip failed: %d => %q => %d, %v"u8, d0, s, d1, errΔ1);
        }
    }
}

// golang.org/issue/4622
public static partial void TestLocationRace(ж<Δtesting.T> Ꮡt) {
    time_internal_test_package.ResetLocalOnceForTest(); // reset the Once to trigger the race
    var c = new channel<@string>(1);
    var cʗ1 = c;
    goǃ(() => {
        cʗ1.ᐸꟷ(Now().String());
    });
    _ = Now().String();
    ᐸꟷ(c);
    Sleep(100 * Millisecond);
    // Back to Los Angeles for subsequent tests:
    time_internal_test_package.ForceUSPacificForTesting();
}

internal static Δtime.Time t;
internal static int64 u;


partial struct mallocTestᴛ1 /*dyn*/ {
    internal nint count;
    internal @string desc;
    internal Action fn;
}
internal static slice<mallocTestᴛ1> mallocTest = new mallocTestᴛ1[]{
    new(0, @"time.Now()"u8, () => {
        t = Now();
    }),
    new(0, @"time.Now().UnixNano()"u8, () => {
        u = Now().UnixNano();
    }),
    new(0, @"time.Now().UnixMilli()"u8, () => {
        u = Now().UnixMilli();
    }),
    new(0, @"time.Now().UnixMicro()"u8, () => {
        u = Now().UnixMicro();
    })
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

public static void TestCountMallocs(ж<Δtesting.T> Ꮡt) {
    if (Δtesting.Short()) {
        Ꮡt.Skip(skippingMallocCountInˢ);
    }
    if (Δruntime.GOMAXPROCS(0) > 1) {
        Ꮡt.Skip(skippingGomaxprocs1ˢ);
    }
    foreach (var (_, mt) in mallocTest) {
        nint allocs = (nint)Δtesting.AllocsPerRun(100, mt.fn);
        if (allocs > mt.count) {
            Ꮡt.Errorf("%s: %d allocs, want %d"u8, mt.desc, allocs, mt.count);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string etcGmt1ˢ = "Etc/GMT+1"u8;

public static void TestLoadFixed(ж<Δtesting.T> Ꮡt) {
    // Issue 4064: handle locations without any zone transitions.
    var (loc, err) = LoadLocation(etcGmt1ˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // The tzdata name Etc/GMT+1 uses "east is negative",
    // but Go and most other systems use "east is positive".
    // So GMT+1 corresponds to -3600 in the Go zone, not +3600.
    var (name, offset) = Now().In(loc).Zone();
    // The zone abbreviation is "-01" since tzdata-2016g, and "GMT+1"
    // on earlier versions; we accept both. (Issue 17276.)
    var wantName = new @string[]{"GMT+1"u8, "-01"u8}.slice();
    // The zone abbreviation may be "+01" on OpenBSD. (Issue 69840.)
    if (Δruntime.GOOS == "openbsd"u8) {
        wantName = append(wantName, "+01"u8);
    }
    if (!slices.Contains(wantName, name) || offset != -1 * 60 * 60) {
        Ꮡt.Errorf("Now().In(loc).Zone() = %q, %d, want %q (one of), %d"u8,
            name, offset, wantName, (nint)(-1 * 60 * 60));
    }
}

internal static Δtime.Duration minDuration => /* -1 << 63 */ -9223372036854775808;
internal static Δtime.Duration maxDuration => /* 1<<63 - 1 */ 9223372036854775807;


partial struct subTestsᴛ1 /*dyn*/ {
    internal Δtime.Time t;
    internal Δtime.Time u;
    internal Δtime.Duration d;
}
internal static slice<subTestsᴛ1> subTests = new subTestsᴛ1[]{
    new(new Time(nil), new Time(nil), ((Δtime.Duration)0)),
    new(Date(2009, 11, 23, 0, 0, 0, 1, ΔUTC), Date(2009, 11, 23, 0, 0, 0, 0, ΔUTC), ((Δtime.Duration)1)),
    new(Date(2009, 11, 23, 0, 0, 0, 0, ΔUTC), Date(2009, 11, 24, 0, 0, 0, 0, ΔUTC), (Δtime.Duration)(-86400000000000L)),
    new(Date(2009, 11, 24, 0, 0, 0, 0, ΔUTC), Date(2009, 11, 23, 0, 0, 0, 0, ΔUTC), (Δtime.Duration)(86400000000000L)),
    new(Date(-2009, 11, 24, 0, 0, 0, 0, ΔUTC), Date(-2009, 11, 23, 0, 0, 0, 0, ΔUTC), (Δtime.Duration)(86400000000000L)),
    new(new Time(nil), Date(2109, 11, 23, 0, 0, 0, 0, ΔUTC), minDuration),
    new(Date(2109, 11, 23, 0, 0, 0, 0, ΔUTC), new Time(nil), maxDuration),
    new(new Time(nil), Date(-2109, 11, 23, 0, 0, 0, 0, ΔUTC), maxDuration),
    new(Date(-2109, 11, 23, 0, 0, 0, 0, ΔUTC), new Time(nil), minDuration),
    new(Date(2290, 1, 1, 0, 0, 0, 0, ΔUTC), Date(2000, 1, 1, 0, 0, 0, 0, ΔUTC), (Δtime.Duration)(9151574400000000000L)),
    new(Date(2300, 1, 1, 0, 0, 0, 0, ΔUTC), Date(2000, 1, 1, 0, 0, 0, 0, ΔUTC), maxDuration),
    new(Date(2000, 1, 1, 0, 0, 0, 0, ΔUTC), Date(2290, 1, 1, 0, 0, 0, 0, ΔUTC), (Δtime.Duration)(-9151574400000000000L)),
    new(Date(2000, 1, 1, 0, 0, 0, 0, ΔUTC), Date(2300, 1, 1, 0, 0, 0, 0, ΔUTC), minDuration),
    new(Date(2311, 11, 26, 2, 16, 47, 63535996, ΔUTC), Date(2019, 8, 16, 2, 29, 30, 268436582, ΔUTC), 9223372036795099414L),
    new(time_internal_test_package.MinMonoTime, time_internal_test_package.MaxMonoTime, minDuration),
    new(time_internal_test_package.MaxMonoTime, time_internal_test_package.MinMonoTime, maxDuration)
}.slice();

public static void TestSub(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (i, st) in subTests) {
        var got = st.t.Sub(st.u);
        if (got != st.d) {
            Ꮡt.Errorf("#%d: Sub(%v, %v): got %v; want %v"u8, i, st.t, st.u, got, st.d);
        }
    }
}


partial struct nsDurationTestsᴛ1 /*dyn*/ {
    internal Δtime.Duration d;
    internal int64 want;
}
internal static slice<nsDurationTestsᴛ1> nsDurationTests = new nsDurationTestsᴛ1[]{
    new(((Δtime.Duration)(-1000)), -1000),
    new(((Δtime.Duration)(-1)), -1),
    new(((Δtime.Duration)1), 1),
    new(((Δtime.Duration)1000), 1000)
}.slice();

public static void TestDurationNanoseconds(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in nsDurationTests) {
        {
            var got = tt.d.Nanoseconds(); if (got != tt.want) {
                Ꮡt.Errorf("Duration(%s).Nanoseconds() = %d; want: %d"u8, tt.d, got, tt.want);
            }
        }
    }
}

internal static slice<nsDurationTestsᴛ1> usDurationTests = new nsDurationTestsᴛ1[]{
    new(((Δtime.Duration)(-1000)), -1),
    new(((Δtime.Duration)1000), 1)
}.slice();

public static void TestDurationMicroseconds(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in usDurationTests) {
        {
            var got = tt.d.Microseconds(); if (got != tt.want) {
                Ꮡt.Errorf("Duration(%s).Microseconds() = %d; want: %d"u8, tt.d, got, tt.want);
            }
        }
    }
}

internal static slice<nsDurationTestsᴛ1> msDurationTests = new nsDurationTestsᴛ1[]{
    new(((Δtime.Duration)(-1000000)), -1),
    new(((Δtime.Duration)1000000), 1)
}.slice();

public static void TestDurationMilliseconds(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in msDurationTests) {
        {
            var got = tt.d.Milliseconds(); if (got != tt.want) {
                Ꮡt.Errorf("Duration(%s).Milliseconds() = %d; want: %d"u8, tt.d, got, tt.want);
            }
        }
    }
}


partial struct secDurationTestsᴛ1 /*dyn*/ {
    internal Δtime.Duration d;
    internal float64 want;
}
internal static slice<secDurationTestsᴛ1> secDurationTests = new secDurationTestsᴛ1[]{
    new(((Δtime.Duration)300000000), 0.3D)
}.slice();

public static void TestDurationSeconds(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in secDurationTests) {
        {
            var got = tt.d.Seconds(); if (got != tt.want) {
                Ꮡt.Errorf("Duration(%s).Seconds() = %g; want: %g"u8, tt.d, got, tt.want);
            }
        }
    }
}

internal static slice<secDurationTestsᴛ1> minDurationTests = new secDurationTestsᴛ1[]{
    new(((Δtime.Duration)(-60000000000L)), -1D),
    new(((Δtime.Duration)(-1)), -1D / 60e9D),
    new(((Δtime.Duration)1), 1D / 60e9D),
    new(((Δtime.Duration)60000000000L), 1D),
    new(((Δtime.Duration)3000), 5e-8D)
}.slice();

public static void TestDurationMinutes(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in minDurationTests) {
        {
            var got = tt.d.Minutes(); if (got != tt.want) {
                Ꮡt.Errorf("Duration(%s).Minutes() = %g; want: %g"u8, tt.d, got, tt.want);
            }
        }
    }
}

internal static slice<secDurationTestsᴛ1> hourDurationTests = new secDurationTestsᴛ1[]{
    new(((Δtime.Duration)(-3600000000000L)), -1D),
    new(((Δtime.Duration)(-1)), -1D / 3600e9D),
    new(((Δtime.Duration)1), 1D / 3600e9D),
    new(((Δtime.Duration)3600000000000L), 1D),
    new(((Δtime.Duration)36), 1e-11D)
}.slice();

public static void TestDurationHours(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in hourDurationTests) {
        {
            var got = tt.d.Hours(); if (got != tt.want) {
                Ꮡt.Errorf("Duration(%s).Hours() = %g; want: %g"u8, tt.d, got, tt.want);
            }
        }
    }
}


partial struct durationTruncateTestsᴛ1 /*dyn*/ {
    internal Δtime.Duration d;
    internal Δtime.Duration m;
    internal Δtime.Duration want;
}
internal static slice<durationTruncateTestsᴛ1> durationTruncateTests = new durationTruncateTestsᴛ1[]{
    new(0, ΔSecond, 0),
    new(ΔMinute, (Δtime.Duration)(-7000000000L), ΔMinute),
    new(ΔMinute, 0, ΔMinute),
    new(ΔMinute, 1, ΔMinute),
    new((Δtime.Duration)(70000000000L), (Δtime.Duration)(10000000000L), (Δtime.Duration)(70000000000L)),
    new((Δtime.Duration)(130000000000L), ΔMinute, (Δtime.Duration)(120000000000L)),
    new((Δtime.Duration)(610000000000L), (Δtime.Duration)(180000000000L), (Δtime.Duration)(540000000000L)),
    new((Δtime.Duration)(70000000000L), (Δtime.Duration)(70000000001L), 0),
    new((Δtime.Duration)(70000000000L), ΔHour, 0),
    new(-ΔMinute, ΔSecond, -ΔMinute),
    new((Δtime.Duration)(-600000000000L), (Δtime.Duration)(180000000000L), (Δtime.Duration)(-540000000000L)),
    new((Δtime.Duration)(-600000000000L), ΔHour, 0)
}.slice();

public static void TestDurationTruncate(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in durationTruncateTests) {
        {
            var got = tt.d.Truncate(tt.m); if (got != tt.want) {
                Ꮡt.Errorf("Duration(%s).Truncate(%s) = %s; want: %s"u8, tt.d, tt.m, got, tt.want);
            }
        }
    }
}

internal static slice<durationTruncateTestsᴛ1> durationRoundTests = new durationTruncateTestsᴛ1[]{
    new(0, ΔSecond, 0),
    new(ΔMinute, (Δtime.Duration)(-11000000000L), ΔMinute),
    new(ΔMinute, 0, ΔMinute),
    new(ΔMinute, 1, ΔMinute),
    new((Δtime.Duration)(120000000000L), ΔMinute, (Δtime.Duration)(120000000000L)),
    new((Δtime.Duration)(130000000000L), ΔMinute, (Δtime.Duration)(120000000000L)),
    new((Δtime.Duration)(150000000000L), ΔMinute, (Δtime.Duration)(180000000000L)),
    new((Δtime.Duration)(170000000000L), ΔMinute, (Δtime.Duration)(180000000000L)),
    new(-ΔMinute, 1, -ΔMinute),
    new((Δtime.Duration)(-120000000000L), ΔMinute, (Δtime.Duration)(-120000000000L)),
    new((Δtime.Duration)(-130000000000L), ΔMinute, (Δtime.Duration)(-120000000000L)),
    new((Δtime.Duration)(-150000000000L), ΔMinute, (Δtime.Duration)(-180000000000L)),
    new((Δtime.Duration)(-170000000000L), ΔMinute, (Δtime.Duration)(-180000000000L)),
    new(8000000000000000000, 3000000000000000000, 9000000000000000000),
    new(9000000000000000000, 5000000000000000000, (Δtime.Duration)(9223372036854775807L)),
    new(-8000000000000000000, 3000000000000000000, -9000000000000000000),
    new(-9000000000000000000, 5000000000000000000, (Δtime.Duration)(-9223372036854775808L)),
    new((Δtime.Duration)(6917529027641081855L), (Δtime.Duration)(6917529027641081856L), (Δtime.Duration)(6917529027641081856L))
}.slice();

public static void TestDurationRound(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in durationRoundTests) {
        {
            var got = tt.d.Round(tt.m); if (got != tt.want) {
                Ꮡt.Errorf("Duration(%s).Round(%s) = %s; want: %s"u8, tt.d, tt.m, got, tt.want);
            }
        }
    }
}


partial struct durationAbsTestsᴛ1 /*dyn*/ {
    internal Δtime.Duration d;
    internal Δtime.Duration want;
}
internal static slice<durationAbsTestsᴛ1> durationAbsTests = new durationAbsTestsᴛ1[]{
    new(0, 0),
    new(1, 1),
    new(-1, 1),
    new((Δtime.Duration)(60000000000L), (Δtime.Duration)(60000000000L)),
    new((Δtime.Duration)(-60000000000L), (Δtime.Duration)(60000000000L)),
    new(minDuration, maxDuration),
    new((Δtime.Duration)(-9223372036854775807L), maxDuration),
    new((Δtime.Duration)(-9223372036854775806L), (Δtime.Duration)(9223372036854775806L)),
    new(maxDuration, maxDuration),
    new((Δtime.Duration)(9223372036854775806L), (Δtime.Duration)(9223372036854775806L))
}.slice();

public static void TestDurationAbs(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in durationAbsTests) {
        {
            var got = tt.d.Abs(); if (got != tt.want) {
                Ꮡt.Errorf("Duration(%s).Abs() = %s; want: %s"u8, tt.d, got, tt.want);
            }
        }
    }
}

// Using Equal since Add don't modify loc using "==" will cause a fail
// Original cause for this test case bug 15852

partial struct defaultLocTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal Func<Δtime.Time, Δtime.Time, bool> f;
}
internal static slice<defaultLocTestsᴛ1> defaultLocTests = new defaultLocTestsᴛ1[]{
    new("After"u8, (Δtime.Time t1, Δtime.Time t2) => t1.After(t2) == t2.After(t1)),
    new("Before"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Before(t2) == t2.Before(t1)),
    new("Equal"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Equal(t2) == t2.Equal(t1)),
    new("Compare"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Compare(t2) == t2.Compare(t1)),
    new("IsZero"u8, (Δtime.Time t1, Δtime.Time t2) => t1.IsZero() == t2.IsZero()),
    new("Date"u8, (Δtime.Time t1, Δtime.Time t2) => {
        var (a1, b1, c1) = t1.Date();
        var (a2, b2, c2) = t2.Date();
        return a1 == a2 && b1 == b2 && c1 == c2;
    }),
    new("Year"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Year() == t2.Year()),
    new("Month"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Month() == t2.Month()),
    new("Day"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Day() == t2.Day()),
    new("Weekday"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Weekday() == t2.Weekday()),
    new("ISOWeek"u8, (Δtime.Time t1, Δtime.Time t2) => {
        var (a1, b1) = t1.ISOWeek();
        var (a2, b2) = t2.ISOWeek();
        return a1 == a2 && b1 == b2;
    }),
    new("Clock"u8, (Δtime.Time t1, Δtime.Time t2) => {
        var (a1, b1, c1) = t1.Clock();
        var (a2, b2, c2) = t2.Clock();
        return a1 == a2 && b1 == b2 && c1 == c2;
    }),
    new("Hour"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Hour() == t2.Hour()),
    new("Minute"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Minute() == t2.Minute()),
    new("Second"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Second() == t2.Second()),
    new("Nanosecond"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Hour() == t2.Hour()),
    new("YearDay"u8, (Δtime.Time t1, Δtime.Time t2) => t1.YearDay() == t2.YearDay()),
    new("Add"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Add(ΔHour).Equal(t2.Add(ΔHour))),
    new("Sub"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Sub(t2) == t2.Sub(t1)),
    new("AddDate"u8, (Δtime.Time t1, Δtime.Time t2) => t1.AddDate(1991, 9, 3) == t2.AddDate(1991, 9, 3)),
    new("UTC"u8, (Δtime.Time t1, Δtime.Time t2) => t1.UTC() == t2.UTC()),
    new("Local"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Local() == t2.Local()),
    new("In"u8, (Δtime.Time t1, Δtime.Time t2) => t1.In(ΔUTC) == t2.In(ΔUTC)),
    new("Local"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Local() == t2.Local()),
    new("Zone"u8, (Δtime.Time t1, Δtime.Time t2) => {
        var (a1, b1) = t1.Zone();
        var (a2, b2) = t2.Zone();
        return a1 == a2 && b1 == b2;
    }),
    new("Unix"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Unix() == t2.Unix()),
    new("UnixNano"u8, (Δtime.Time t1, Δtime.Time t2) => t1.UnixNano() == t2.UnixNano()),
    new("UnixMilli"u8, (Δtime.Time t1, Δtime.Time t2) => t1.UnixMilli() == t2.UnixMilli()),
    new("UnixMicro"u8, (Δtime.Time t1, Δtime.Time t2) => t1.UnixMicro() == t2.UnixMicro()),
    new("AppendBinary"u8, (Δtime.Time t1, Δtime.Time t2) => {
        var buf1 = new slice<byte>(4, 32);
        var buf2 = new slice<byte>(4, 32);
        var (a1, b1) = t1.AppendBinary(buf1);
        var (a2, b2) = t2.AppendBinary(buf2);
        return bytes.Equal(a1[4..], a2[4..]) && AreEqual(b1, b2);
    }),
    new("MarshalBinary"u8, (Δtime.Time t1, Δtime.Time t2) => {
        var (a1, b1) = t1.MarshalBinary();
        var (a2, b2) = t2.MarshalBinary();
        return bytes.Equal(a1, a2) && AreEqual(b1, b2);
    }),
    new("GobEncode"u8, (Δtime.Time t1, Δtime.Time t2) => {
        var (a1, b1) = t1.GobEncode();
        var (a2, b2) = t2.GobEncode();
        return bytes.Equal(a1, a2) && AreEqual(b1, b2);
    }),
    new("MarshalJSON"u8, (Δtime.Time t1, Δtime.Time t2) => {
        var (a1, b1) = t1.MarshalJSON();
        var (a2, b2) = t2.MarshalJSON();
        return bytes.Equal(a1, a2) && AreEqual(b1, b2);
    }),
    new("AppendText"u8, (Δtime.Time t1, Δtime.Time t2) => {
        nint maxCap = len(RFC3339Nano) + 4;
        var buf1 = new slice<byte>(4, maxCap);
        var buf2 = new slice<byte>(4, maxCap);
        var (a1, b1) = t1.AppendText(buf1);
        var (a2, b2) = t2.AppendText(buf2);
        return bytes.Equal(a1[4..], a2[4..]) && AreEqual(b1, b2);
    }),
    new("MarshalText"u8, (Δtime.Time t1, Δtime.Time t2) => {
        var (a1, b1) = t1.MarshalText();
        var (a2, b2) = t2.MarshalText();
        return bytes.Equal(a1, a2) && AreEqual(b1, b2);
    }),
    new("Truncate"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Truncate(ΔHour).Equal(t2.Truncate(ΔHour))),
    new("Round"u8, (Δtime.Time t1, Δtime.Time t2) => t1.Round(ΔHour).Equal(t2.Round(ΔHour))),
    new("== Time{}"u8, (Δtime.Time t1, Δtime.Time t2) => (t1 == new Time(nil)) == (t2 == new Time(nil)))
}.slice();

public static void TestDefaultLoc(ж<Δtesting.T> Ꮡt) {
    // Verify that all of Time's methods behave identically if loc is set to
    // nil or UTC.
    foreach (var (_, tt) in defaultLocTests) {
        var t1 = new Time(nil);
        var t2 = new Time(nil).UTC();
        if (!tt.f(t1, t2)) {
            Ꮡt.Errorf("Time{} and Time{}.UTC() behave differently for %s"u8, tt.name);
        }
    }
}

public static void BenchmarkNow(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        t = Now();
    }
}

public static void BenchmarkNowUnixNano(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        u = Now().UnixNano();
    }
}

public static void BenchmarkNowUnixMilli(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        u = Now().UnixMilli();
    }
}

public static void BenchmarkNowUnixMicro(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        u = Now().UnixMicro();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string monJan21504052006ˢ = "Mon Jan  2 15:04:05 2006"u8;

public static void BenchmarkFormat(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Unix(1265346057, 0);
    for (nint i = 0; i < b.N; i++) {
        t.Format(monJan21504052006ˢ);
    }
}

public static void BenchmarkFormatRFC3339(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Unix(1265346057, 0);
    for (nint i = 0; i < b.N; i++) {
        t.Format("2006-01-02T15:04:05Z07:00"u8);
    }
}

public static void BenchmarkFormatRFC3339Nano(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Unix(1265346057, 0);
    for (nint i = 0; i < b.N; i++) {
        t.Format("2006-01-02T15:04:05.999999999Z07:00"u8);
    }
}

public static void BenchmarkFormatNow(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Like BenchmarkFormat, but easier, because the time zone
    // lookup cache is optimized for the present.
    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        t.Format(monJan21504052006ˢ);
    }
}

public static void BenchmarkMarshalJSON(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        t.MarshalJSON();
    }
}

public static void BenchmarkMarshalText(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        t.MarshalText();
    }
}

public static void BenchmarkMarshalBinary(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        t.MarshalBinary();
    }
}

public static void BenchmarkParse(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Parse(ANSIC, monJan21504052006ˢ);
    }
}

internal static readonly @string testdataRFC3339UTC = "2020-08-22T11:27:43.123456789Z"u8;

public static void BenchmarkParseRFC3339UTC(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Parse(RFC3339, testdataRFC3339UTC);
    }
}

internal static slice<byte> testdataRFC3339UTCBytes = slice<byte>(testdataRFC3339UTC);

public static void BenchmarkParseRFC3339UTCBytes(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Parse(RFC3339, ((@string)testdataRFC3339UTCBytes));
    }
}

internal static readonly @string testdataRFC3339TZ = "2020-08-22T11:27:43.123456789-02:00"u8;

public static void BenchmarkParseRFC3339TZ(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Parse(RFC3339, testdataRFC3339TZ);
    }
}

internal static slice<byte> testdataRFC3339TZBytes = slice<byte>(testdataRFC3339TZ);

public static void BenchmarkParseRFC3339TZBytes(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Parse(RFC3339, ((@string)testdataRFC3339TZBytes));
    }
}

public static void BenchmarkParseDuration(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseDuration("9007199254.740993ms"u8);
        ParseDuration("9007199254740993ns"u8);
    }
}

public static void BenchmarkHour(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        _ = t.Hour();
    }
}

public static void BenchmarkSecond(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        _ = t.Second();
    }
}

public static void BenchmarkDate(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        (_, _, _) = t.Date();
    }
}

public static void BenchmarkYear(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        _ = t.Year();
    }
}

public static void BenchmarkYearDay(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        _ = t.YearDay();
    }
}

public static void BenchmarkMonth(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        _ = t.Month();
    }
}

public static void BenchmarkDay(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        _ = t.Day();
    }
}

public static void BenchmarkISOWeek(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        (_, _) = t.ISOWeek();
    }
}

public static void BenchmarkGoString(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = Now();
    for (nint i = 0; i < b.N; i++) {
        _ = t.GoString();
    }
}

public static void BenchmarkDateFunc(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    Δtime.Time t = default!;
    foreach (var _ᴛ1 in range(b.N)) {
        t = Date(2020, 8, 22, 11, 27, 43, 123456789, ΔUTC);
    }
    _ = t;
}

public static void BenchmarkUnmarshalText(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    Δtime.Time t = default!;
    var @in = slice<byte>("2020-08-22T11:27:43.123456789-02:00"u8);
    for (nint i = 0; i < b.N; i++) {
        t.UnmarshalText(@in);
    }
}

public static void TestMarshalBinaryZeroTime(ж<Δtesting.T> Ꮡt) {
    var t0 = new Time(nil);
    var (enc, err) = t0.MarshalBinary();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var t1 = Now(); // not zero
    {
        var errΔ1 = t1.UnmarshalBinary(enc); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    if (t1 != t0) {
        Ꮡt.Errorf("t0=%#v\nt1=%#v\nwant identical structures"u8, t0, t1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string usEasternˢ = "US/Eastern"u8;

public static void TestMarshalBinaryVersion2(ж<Δtesting.T> Ꮡt) {
    var (t0, err) = Parse(RFC3339, "1880-01-01T00:00:00Z"u8);
    if (err != default!) {
        Ꮡt.Errorf("Failed to parse time, error = %v"u8, err);
    }
    (var loc, err) = LoadLocation(usEasternˢ);
    if (err != default!) {
        Ꮡt.Errorf("Failed to load location, error = %v"u8, err);
    }
    var t1 = t0.In(loc);
    (var b, err) = t1.MarshalBinary();
    if (err != default!) {
        Ꮡt.Errorf("Failed to Marshal, error = %v"u8, err);
    }
    var t2 = new Time(nil);
    err = t2.UnmarshalBinary(b);
    if (err != default!) {
        Ꮡt.Errorf("Failed to Unmarshal, error = %v"u8, err);
    }
    if (!(t0.Equal(t1) && t1.Equal(t2))) {
        if (!t0.Equal(t1)) {
            Ꮡt.Errorf("The result t1: %+v after Marshal is not matched original t0: %+v"u8, t1, t0);
        }
        if (!t1.Equal(t2)) {
            Ꮡt.Errorf("The result t2: %+v after Unmarshal is not matched original t1: %+v"u8, t2, t1);
        }
    }
}

public static void TestUnmarshalTextAllocations(ж<Δtesting.T> Ꮡt) {
    var @in = slice<byte>(testdataRFC3339UTC); // short enough to be stack allocated
    {
        var inʗ1 = @in;
        var allocs = Δtesting.AllocsPerRun(100, () => {
            Δtime.Time tΔ1 = default!;
            tΔ1.UnmarshalText(inʗ1);
        }); if (allocs != 0D) {
            Ꮡt.Errorf("got %v allocs, want 0 allocs"u8, allocs);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string month0ˢ = "%!Month(0)"u8;

// Issue 17720: Zero value of time.Month fails to print
public static void TestZeroMonthString(ж<Δtesting.T> Ꮡt) {
    {
        @string got = ((timeꓸMonth)0).String();
        @string want = month0ˢ; if (got != want) {
            Ꮡt.Errorf("zero month = %q; want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tuesdayˢ = "Tuesday"u8;
internal static readonly @string weekday14ˢ = "%!Weekday(14)"u8;

// Issue 24692: Out of range weekday panics
public static void TestWeekdayString(ж<Δtesting.T> Ꮡt) {
    {
        @string got = Tuesday.String();
        @string want = tuesdayˢ; if (got != want) {
            Ꮡt.Errorf("Tuesday weekday = %q; want %q"u8, got, want);
        }
    }
    {
        @string got = ((timeꓸWeekday)14).String();
        @string want = weekday14ˢ; if (got != want) {
            Ꮡt.Errorf("14th weekday = %q; want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestWithoutADevˢ = (@string)"skipping test without a /dev/zero"u8;
internal static readonly @string isTooLargeˢ = "is too large"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string zeroᶜ = "/dev/zero"u8;

public static void TestReadFileLimit(ж<Δtesting.T> Ꮡt) {
    @string zero = zeroᶜ;
    {
        var (_, errΔ1) = Δos.Stat(zero); if (errΔ1 != default!) {
            Ꮡt.Skip(skippingTestWithoutADevˢ);
        }
    }
    var (_, err) = time_internal_test_package.ReadFile(zero);
    if (err == default! || !strings.Contains(err.Error(), isTooLargeˢ)) {
        Ꮡt.Errorf("readFile(%q) error = %v; want error containing 'is too large'"u8, zero, err);
    }
}

// Issue 25686: hard crash on concurrent timer access.
// Issue 37400: panic with "racy use of timers"
// This test deliberately invokes a race condition.
// We are testing that we don't crash with "fatal error: panic holding locks",
// and that we also don't panic.
public static partial void TestConcurrentTimerReset(ж<Δtesting.T> Ꮡt) {
    const nint goroutines = 8;
    const nint tries = 1000;
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(goroutines);
    var timer = NewTimer(ΔHour);
    for (nint i = 0; i < goroutines; i++) {
        var timerʗ1 = timer;
        goǃ((nint iΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                for (nint j = 0; j < tries; j++) {
                    timerʗ1.Reset(ΔHour + ((Δtime.Duration)(int64)(iΔ1 * j)));
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
    }
    Ꮡwg.Wait();
}

// Issue 37400: panic with "racy use of timers".
public static partial void TestConcurrentTimerResetStop(ж<Δtesting.T> Ꮡt) {
    UntypedInt goroutines = 8;
    const nint tries = 1000;
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(goroutines * 2);
    var timer = NewTimer(ΔHour);
    for (nint i = 0; i < goroutines; i++) {
        var timerʗ1 = timer;
        goǃ((nint iΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                for (nint j = 0; j < tries; j++) {
                    timerʗ1.Reset(ΔHour + ((Δtime.Duration)(int64)(iΔ1 * j)));
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
        var timerʗ2 = timer;
        goǃ((nint iΔ2) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                timerʗ2.Stop();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string australiaBrisbaneˢ = "Australia/Brisbane"u8;
internal static readonly @string fixedTimeˢ = "FIXED_TIME"u8;

internal partial struct TestTimeIsDST_tests /*dyn*/ {
    internal Δtime.Time time;
    internal bool want;
}

public static void TestTimeIsDST(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);
        var (tzWithDST, err) = LoadLocation(australiaSydneyˢ);
        if (err != default!) {
            Ꮡt.Fatalf("could not load tz 'Australia/Sydney': %v"u8, err);
        }
        (var tzWithoutDST, err) = LoadLocation(australiaBrisbaneˢ);
        if (err != default!) {
            Ꮡt.Fatalf("could not load tz 'Australia/Brisbane': %v"u8, err);
        }
        var tzFixed = FixedZone(fixedTimeˢ, 12345);
        var tests = new array<TestTimeIsDST_tests>(8){
            [0] = new(Date(2009, 1, 1, 12, 0, 0, 0, ΔUTC), false),
            [1] = new(Date(2009, 6, 1, 12, 0, 0, 0, ΔUTC), false),
            [2] = new(Date(2009, 1, 1, 12, 0, 0, 0, tzWithDST), true),
            [3] = new(Date(2009, 6, 1, 12, 0, 0, 0, tzWithDST), false),
            [4] = new(Date(2009, 1, 1, 12, 0, 0, 0, tzWithoutDST), false),
            [5] = new(Date(2009, 6, 1, 12, 0, 0, 0, tzWithoutDST), false),
            [6] = new(Date(2009, 1, 1, 12, 0, 0, 0, tzFixed), false),
            [7] = new(Date(2009, 6, 1, 12, 0, 0, 0, tzFixed), false)
        };
        foreach (var (i, tt) in tests.ΔRangeSnapshot()) {
            var got = tt.time.IsDST();
            if (got != tt.want) {
                Ꮡt.Errorf("#%d:: (%#v).IsDST()=%t, want %t"u8, i, tt.time.Format(RFC3339), got, tt.want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTimeAddSecOverflow(ж<Δtesting.T> Ꮡt) {
    // Test it with positive delta.
    int64 maxInt64 = 9223372036854775807L;
    var timeExt = maxInt64 - time_internal_test_package.UnixToInternal - 50;
    var notMonoTime = Unix(timeExt, 0);
    for (var i = (int64)0; i < 100; i++) {
        var sec = notMonoTime.Unix();
        notMonoTime = notMonoTime.Add(((Δtime.Duration)(i * 1000000000)));
        {
            var newSec = notMonoTime.Unix(); if (newSec != sec + i && newSec + time_internal_test_package.UnixToInternal != maxInt64) {
                Ꮡt.Fatalf("time ext: %d overflows with positive delta, overflow threshold: %d"u8, newSec, maxInt64);
            }
        }
    }
    // Test it with negative delta.
    maxInt64 = -maxInt64;
    notMonoTime = time_internal_test_package.NotMonoNegativeTime;
    for (var i = (int64)0; i > -100; i--) {
        var sec = notMonoTime.Unix();
        notMonoTime = notMonoTime.Add(((Δtime.Duration)(i * 1000000000)));
        {
            var newSec = notMonoTime.Unix(); if (newSec != sec + i && newSec + time_internal_test_package.UnixToInternal != maxInt64) {
                Ꮡt.Fatalf("time ext: %d overflows with positive delta, overflow threshold: %d"u8, newSec, maxInt64);
            }
        }
    }
}

internal partial struct TestTimeWithZoneTransition_tests /*dyn*/ {
    internal Δtime.Time give;
    internal Δtime.Time want;
}

// Issue 49284: time: ParseInLocation incorrectly because of Daylight Saving Time
public static void TestTimeWithZoneTransition(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);
        var (loc, err) = LoadLocation(asiaShanghaiˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var tests = new array<TestTimeWithZoneTransition_tests>(8){ // 14 Apr 1991 - Daylight Saving Time Started
 // When time of "Asia/Shanghai" was about to reach
 // Sunday, 14 April 1991, 02:00:00 clocks were turned forward 1 hour to
 // Sunday, 14 April 1991, 03:00:00 local daylight time instead.
 // The UTC time was 13 April 1991, 18:00:00

            [0] = new(Date(1991, April, 13, 17, 50, 0, 0, loc), Date(1991, April, 13, 9, 50, 0, 0, ΔUTC)),
            [1] = new(Date(1991, April, 13, 18, 0, 0, 0, loc), Date(1991, April, 13, 10, 0, 0, 0, ΔUTC)),
            [2] = new(Date(1991, April, 14, 1, 50, 0, 0, loc), Date(1991, April, 13, 17, 50, 0, 0, ΔUTC)),
            [3] = new(Date(1991, April, 14, 3, 0, 0, 0, loc), Date(1991, April, 13, 18, 0, 0, 0, ΔUTC)), // 15 Sep 1991 - Daylight Saving Time Ended
 // When local daylight time of "Asia/Shanghai" was about to reach
 // Sunday, 15 September 1991, 02:00:00 clocks were turned backward 1 hour to
 // Sunday, 15 September 1991, 01:00:00 local standard time instead.
 // The UTC time was 14 September 1991, 17:00:00

            [4] = new(Date(1991, September, 14, 16, 50, 0, 0, loc), Date(1991, September, 14, 7, 50, 0, 0, ΔUTC)),
            [5] = new(Date(1991, September, 14, 17, 0, 0, 0, loc), Date(1991, September, 14, 8, 0, 0, 0, ΔUTC)),
            [6] = new(Date(1991, September, 15, 0, 50, 0, 0, loc), Date(1991, September, 14, 15, 50, 0, 0, ΔUTC)),
            [7] = new(Date(1991, September, 15, 2, 0, 0, 0, loc), Date(1991, September, 14, 18, 0, 0, 0, ΔUTC))
        };
        foreach (var (i, tt) in tests.ΔRangeSnapshot()) {
            if (!tt.give.Equal(tt.want)) {
                Ꮡt.Errorf("#%d:: %#v is not equal to %#v"u8, i, tt.give.Format(RFC3339), tt.want.Format(RFC3339));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestZoneBounds_realTests /*dyn*/ {
    internal Δtime.Time giveTime;
    internal Δtime.Time wantStart;
    internal Δtime.Time wantEnd;
}

public static void TestZoneBounds(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);
        var (loc, err) = LoadLocation(asiaShanghaiˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // The ZoneBounds of a UTC location would just return two zero Time.
        foreach (var (_, vᴛ1) in utctests) {
            ref var test = ref heap(new TimeTest(), out var Ꮡtest);
            test = vᴛ1;

            var sec = test.seconds;
            var golden = Ꮡtest.of(TimeTest.Ꮡgolden);
            var tm = Unix(sec, 0).UTC();
            var (startΔ1, endΔ1) = tm.ZoneBounds();
            if (!(startΔ1.IsZero() && endΔ1.IsZero())) {
                Ꮡt.Errorf("ZoneBounds of %+v expects two zero Time, got:\n  start=%v\n  end=%v"u8, golden.Value, startΔ1, endΔ1);
            }
        }
        // If the zone begins at the beginning of time, start will be returned as a zero Time.
        // Use math.MinInt32 to avoid overflow of int arguments on 32-bit systems.
        var beginTime = Date(Δmath.MinInt32, January, 1, 0, 0, 0, 0, loc);
        var (start, end) = beginTime.ZoneBounds();
        if (!start.IsZero() || end.IsZero()) {
            Ꮡt.Errorf("ZoneBounds of %v expects start is zero Time, got:\n  start=%v\n  end=%v"u8, beginTime, start, end);
        }
        // If the zone goes on forever, end will be returned as a zero Time.
        // Use math.MaxInt32 to avoid overflow of int arguments on 32-bit systems.
        var foreverTime = Date(Δmath.MaxInt32, January, 1, 0, 0, 0, 0, loc);
        (start, end) = foreverTime.ZoneBounds();
        if (start.IsZero() || !end.IsZero()) {
            Ꮡt.Errorf("ZoneBounds of %v expects end is zero Time, got:\n  start=%v\n  end=%v"u8, foreverTime, start, end);
        }
        // Check some real-world cases to make sure we're getting the right bounds.
        var boundOne = Date(1990, September, 16, 1, 0, 0, 0, loc);
        var boundTwo = Date(1991, April, 14, 3, 0, 0, 0, loc);
        var boundThree = Date(1991, September, 15, 1, 0, 0, 0, loc);
        Δtime.Time makeLocalTime(int64 sec) => Unix(sec, 0);
        var realTests = new array<TestZoneBounds_realTests>(23){ // The ZoneBounds of "Asia/Shanghai" Daylight Saving Time

            [0] = new(Date(1991, April, 13, 17, 50, 0, 0, loc), boundOne, boundTwo),
            [1] = new(Date(1991, April, 13, 18, 0, 0, 0, loc), boundOne, boundTwo),
            [2] = new(Date(1991, April, 14, 1, 50, 0, 0, loc), boundOne, boundTwo),
            [3] = new(boundTwo, boundTwo, boundThree),
            [4] = new(Date(1991, September, 14, 16, 50, 0, 0, loc), boundTwo, boundThree),
            [5] = new(Date(1991, September, 14, 17, 0, 0, 0, loc), boundTwo, boundThree),
            [6] = new(Date(1991, September, 15, 0, 50, 0, 0, loc), boundTwo, boundThree), // The ZoneBounds of a "Asia/Shanghai" after the last transition (Standard Time)

            [7] = new(boundThree, boundThree, new Time(nil)),
            [8] = new(Date(1991, December, 15, 1, 50, 0, 0, loc), boundThree, new Time(nil)),
            [9] = new(Date(1992, April, 13, 17, 50, 0, 0, loc), boundThree, new Time(nil)),
            [10] = new(Date(1992, April, 13, 18, 0, 0, 0, loc), boundThree, new Time(nil)),
            [11] = new(Date(1992, April, 14, 1, 50, 0, 0, loc), boundThree, new Time(nil)),
            [12] = new(Date(1992, September, 14, 16, 50, 0, 0, loc), boundThree, new Time(nil)),
            [13] = new(Date(1992, September, 14, 17, 0, 0, 0, loc), boundThree, new Time(nil)),
            [14] = new(Date(1992, September, 15, 0, 50, 0, 0, loc), boundThree, new Time(nil)), // The ZoneBounds of a local time would return two local Time.
 // Note: We preloaded "America/Los_Angeles" as time.Local for testing

            [15] = new(makeLocalTime(0), makeLocalTime(-5756400), makeLocalTime(9972000)),
            [16] = new(makeLocalTime(1221681866), makeLocalTime(1205056800), makeLocalTime(1225616400)),
            [17] = new(makeLocalTime(2152173599L), makeLocalTime(2145916800), makeLocalTime(2152173600L)),
            [18] = new(makeLocalTime(2152173600L), makeLocalTime(2152173600L), makeLocalTime(2172733200L)),
            [19] = new(makeLocalTime(2152173601L), makeLocalTime(2152173600L), makeLocalTime(2172733200L)),
            [20] = new(makeLocalTime(2159200800L), makeLocalTime(2152173600L), makeLocalTime(2172733200L)),
            [21] = new(makeLocalTime(2172733199L), makeLocalTime(2152173600L), makeLocalTime(2172733200L)),
            [22] = new(makeLocalTime(2172733200L), makeLocalTime(2172733200L), makeLocalTime(2177452800L))
        };
        foreach (var (i, tt) in realTests.ΔRangeSnapshot()) {
            var (startΔ2, endΔ2) = tt.giveTime.ZoneBounds();
            if (!startΔ2.Equal(tt.wantStart) || !endΔ2.Equal(tt.wantEnd)) {
                Ꮡt.Errorf("#%d:: ZoneBounds of %v expects right bounds:\n  got start=%v\n  want start=%v\n  got end=%v\n  want end=%v"u8,
                    i, tt.giveTime, startΔ2, tt.wantStart, endΔ2, tt.wantEnd);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end time_test_package
