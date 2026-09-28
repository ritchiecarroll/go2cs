// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using reflect = reflect_package;
using Δtesting = testing_package;
using Δtime = time_package;
using @internal;
using static go.time_internal_test_package;

partial class time_test_package {

[GoInit] internal static void init() {
    if (time_internal_test_package.ZoneinfoForTesting() != nil) {
        throw panic(fmt.Errorf("zoneinfo initialized before first LoadLocation"u8));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string asiaJerusalemˢ = "Asia/Jerusalem"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testZoneinfoᶜ = "foo.zip"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string envᶜ = "ZONEINFO"u8;

public static void TestEnvVarUsage(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        time_internal_test_package.ResetZoneinfoForTesting();
        @string testZoneinfo = testZoneinfoᶜ;
        @string env = envᶜ;
        Ꮡt.Setenv(env, testZoneinfo);
        // Result isn't important, we're testing the side effect of this command
        Δtime.LoadLocation(asiaJerusalemˢ);
        defer(time_internal_test_package.ResetZoneinfoForTesting, ref ᒐ);
        {
            var zoneinfo = time_internal_test_package.ZoneinfoForTesting(); if (testZoneinfo != zoneinfo.Value) {
                Ꮡt.Errorf("zoneinfo does not match env variable: got %q want %q"u8, zoneinfo.Value, testZoneinfo);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string asiaSomethingNotExistˢ = "Asia/SomethingNotExist"u8;

public static void TestBadLocationErrMsg(ж<Δtesting.T> Ꮡt) {
    time_internal_test_package.ResetZoneinfoForTesting();
    @string loc = asiaSomethingNotExistˢ;
    var want = errors.New("unknown time zone "u8 + loc);
    var (_, err) = Δtime.LoadLocation(loc);
    if (err.Error() != want.Error()) {
        Ꮡt.Errorf("LoadLocation(%q) error = %v; want %v"u8, loc, err, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string envᶜ1 = "ZONEINFO"u8;

public static void TestLoadLocationValidatesNames(ж<Δtesting.T> Ꮡt) {
    time_internal_test_package.ResetZoneinfoForTesting();
    @string env = envᶜ1;
    Ꮡt.Setenv(env, ""u8);
    var bad = new @string[]{
        "/usr/foo/Foo"u8,
        "\\UNC\foo"u8,
        ".."u8,
        "a.."u8
    }.slice();
    foreach (var (_, v) in bad) {
        var (_, err) = Δtime.LoadLocation(v);
        if (!AreEqual(err, time_internal_test_package.ErrLocation)) {
            Ꮡt.Errorf("LoadLocation(%q) error = %v; want ErrLocation"u8, v, err);
        }
    }
}

public static void TestVersion3(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);
        var (_, err) = Δtime.LoadLocation(asiaJerusalemˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string formatᶜ = "Mon, 02 Jan 2006 15:04:05 -0700 (MST)"u8;

[GoType("dyn")] internal partial struct TestFirstZone_type {
    internal @string zone;
    internal int64 unix;
    internal @string want1;
    internal @string want2;
}

// Test that we get the correct results for times before the first
// transition time. To do this we explicitly check early dates in a
// couple of specific timezones.
public static void TestFirstZone(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);
        @string format = formatᶜ;
        slice<TestFirstZone_type> tests = new TestFirstZone_type[]{
            new(
                "PST8PDT"u8,
                -1633269601,
                "Sun, 31 Mar 1918 01:59:59 -0800 (PST)"u8,
                "Sun, 31 Mar 1918 03:00:00 -0700 (PDT)"u8
            ),
            new(
                "Pacific/Fakaofo"u8,
                1325242799,
                "Thu, 29 Dec 2011 23:59:59 -1100 (-11)"u8,
                "Sat, 31 Dec 2011 00:00:00 +1300 (+13)"u8
            )
        }.slice();
        foreach (var (_, test) in tests) {
            var (z, err) = Δtime.LoadLocation(test.zone);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            @string s = Δtime.Unix(test.unix, 0).In(z).Format(format);
            if (s != test.want1) {
                Ꮡt.Errorf("for %s %d got %q want %q"u8, test.zone, test.unix, s, test.want1);
            }
            s = Δtime.Unix(test.unix + 1, 0).In(z).Format(format);
            if (s != test.want2) {
                Ꮡt.Errorf("for %s %d got %q want %q"u8, test.zone, test.unix, s, test.want2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestLocationNames(ж<Δtesting.T> Ꮡt) {
    if (Δtime.ΔLocal.String() != "Local"u8) {
        Ꮡt.Errorf(@"invalid Local location name: got %q want ""Local"""u8, Δtime.ΔLocal.OrTypedNil());
    }
    if (Δtime.ΔUTC.String() != "UTC"u8) {
        Ꮡt.Errorf(@"invalid UTC location name: got %q want ""UTC"""u8, Δtime.ΔUTC.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToLocateTzinfoˢ = (@string)"Failed to locate tzinfo source in GOROOT."u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string locationNameᶜ = "Asia/Jerusalem"u8;

public static void TestLoadLocationFromTZData(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);
        @string locationName = locationNameᶜ;
        var (reference, err) = Δtime.LoadLocation(locationName);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var (gorootSource, ok) = time_internal_test_package.GorootZoneSource(testenv.GOROOT(new time_test_package.testing_TжTB(Ꮡt)));
        if (!ok) {
            Ꮡt.Fatal(failedToLocateTzinfoˢ);
        }
        (var tzinfo, err) = time_internal_test_package.LoadTzinfo(locationName, gorootSource);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var sample, err) = Δtime.LoadLocationFromTZData(locationName, tzinfo);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!reflect.DeepEqual(reference.OrTypedNil(), sample.OrTypedNil())) {
            Ꮡt.Errorf("return values of LoadLocationFromTZData and LoadLocation don't match"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string estˢ = "EST"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string locNameᶜ = "America/New_York"u8;

// Issue 30099.
public static void TestEarlyLocation(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);
        @string locName = locNameᶜ;
        var (loc, err) = Δtime.LoadLocation(locName);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var d = Δtime.Date(1900, Δtime.January, 1, 0, 0, 0, 0, loc);
        var (tzName, tzOffset) = d.Zone();
        {
            @string want = estˢ; if (tzName != want) {
                Ꮡt.Errorf("Zone name == %s, want %s"u8, tzName, want);
            }
        }
        {
            nint want = -18000; if (tzOffset != want) {
                Ꮡt.Errorf("Zone offset == %d, want %d"u8, tzOffset, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcˢ = "abc"u8;
internal static readonly object expectedErrorGotNoneˢ = (@string)"expected error, got none"u8;

public static void TestMalformedTZData(ж<Δtesting.T> Ꮡt) {
    // The goal here is just that malformed tzdata results in an error, not a panic.
    @string issue29437 = ((@string)(new byte[]{0x54, 0x5a, 0x69, 0x66, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30}));
    var (_, err) = Δtime.LoadLocationFromTZData(abcˢ, slice<byte>(issue29437));
    if (err == default!) {
        Ꮡt.Error(expectedErrorGotNoneˢ);
    }
}

// 2020b slim tzdata for Europe/Berlin.
// 2021a slim tzdata for America/Nuuk.
// 2021a slim tzdata for Asia/Gaza.
// 2021a slim tzdata for Europe/Dublin.

[GoType("dyn")] partial struct slimTestsᴛ1 {
    internal @string zoneName;
    internal @string fileName;
    internal Func<ж<timeꓸLocation>, Δtime.Time> date;
    internal @string wantName;
    internal nint wantOffset;
}
internal static slice<slimTestsᴛ1> slimTests = new slimTestsᴛ1[]{
    new(
        zoneName: "Europe/Berlin"u8,
        fileName: "2020b_Europe_Berlin"u8,
        date: (ж<timeꓸLocation> loc) => Δtime.Date(2020, Δtime.October, 29, 15, 30, 0, 0, loc),
        wantName: "CET"u8,
        wantOffset: 3600
    ),
    new(
        zoneName: "America/Nuuk"u8,
        fileName: "2021a_America_Nuuk"u8,
        date: (ж<timeꓸLocation> loc) => Δtime.Date(2020, Δtime.October, 29, 15, 30, 0, 0, loc),
        wantName: "-03"u8,
        wantOffset: -10800
    ),
    new(
        zoneName: "Asia/Gaza"u8,
        fileName: "2021a_Asia_Gaza"u8,
        date: (ж<timeꓸLocation> loc) => Δtime.Date(2020, Δtime.October, 29, 15, 30, 0, 0, loc),
        wantName: "EET"u8,
        wantOffset: 7200
    ),
    new(
        zoneName: "Europe/Dublin"u8,
        fileName: "2021a_Europe_Dublin"u8,
        date: (ж<timeꓸLocation> loc) => Δtime.Date(2021, Δtime.April, 2, 11, 12, 13, 0, loc),
        wantName: "IST"u8,
        wantOffset: 3600
    )
}.slice();

public static void TestLoadLocationFromTZDataSlim(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in slimTests) {
        var (tzData, err) = Δos.ReadFile("testdata/"u8 + test.fileName);
        if (err != default!) {
            Ꮡt.Error(err);
            continue;
        }
        (var reference, err) = Δtime.LoadLocationFromTZData(test.zoneName, tzData);
        if (err != default!) {
            Ꮡt.Error(err);
            continue;
        }
        var d = test.date(reference);
        var (tzName, tzOffset) = d.Zone();
        if (tzName != test.wantName) {
            Ꮡt.Errorf("Zone name == %s, want %s"u8, tzName, test.wantName);
        }
        if (tzOffset != test.wantOffset) {
            Ꮡt.Errorf("Zone offset == %d, want %d"u8, tzOffset, test.wantOffset);
        }
    }
}

[GoType("dyn")] internal partial struct TestTzset_type {
    internal @string inStr;
    internal int64 inEnd;
    internal int64 inSec;
    internal @string name;
    internal nint off;
    internal int64 start;
    internal int64 end;
    internal bool isDST;
    internal bool ok;
}

public static void TestTzset(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestTzset_type[]{
        new(""u8, 0, 0, ""u8, 0, 0, 0, false, false),
        new("PST8PDT,M3.2.0,M11.1.0"u8, 0, 2159200800L, "PDT"u8, -7 * 60 * 60, 2152173600L, 2172733200L, true, true),
        new("PST8PDT,M3.2.0,M11.1.0"u8, 0, 2152173599L, "PST"u8, -8 * 60 * 60, 2145916800, 2152173600L, false, true),
        new("PST8PDT,M3.2.0,M11.1.0"u8, 0, 2152173600L, "PDT"u8, -7 * 60 * 60, 2152173600L, 2172733200L, true, true),
        new("PST8PDT,M3.2.0,M11.1.0"u8, 0, 2152173601L, "PDT"u8, -7 * 60 * 60, 2152173600L, 2172733200L, true, true),
        new("PST8PDT,M3.2.0,M11.1.0"u8, 0, 2172733199L, "PDT"u8, -7 * 60 * 60, 2152173600L, 2172733200L, true, true),
        new("PST8PDT,M3.2.0,M11.1.0"u8, 0, 2172733200L, "PST"u8, -8 * 60 * 60, 2172733200L, 2177452800L, false, true),
        new("PST8PDT,M3.2.0,M11.1.0"u8, 0, 2172733201L, "PST"u8, -8 * 60 * 60, 2172733200L, 2177452800L, false, true),
        new("KST-9"u8, 592333200, 1677246697, "KST"u8, 9 * 60 * 60, 592333200, 9223372036854775807L, false, true)
    }.slice()) {
        var (name, off, start, end, isDST, ok) = time_internal_test_package.Tzset(test.inStr, test.inEnd, test.inSec);
        if (name != test.name || off != test.off || start != test.start || end != test.end || isDST != test.isDST || ok != test.ok) {
            Ꮡt.Errorf("tzset(%q, %d, %d) = %q, %d, %d, %d, %t, %t, want %q, %d, %d, %d, %t, %t"u8, test.inStr, test.inEnd, test.inSec, name, off, start, end, isDST, ok, test.name, test.off, test.start, test.end, test.isDST, test.ok);
        }
    }
}

[GoType("dyn")] internal partial struct TestTzsetName_type {
    internal @string @in;
    internal @string name;
    internal @string @out;
    internal bool ok;
}

public static void TestTzsetName(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestTzsetName_type[]{
        new(""u8, ""u8, ""u8, false),
        new("X"u8, ""u8, ""u8, false),
        new("PST"u8, "PST"u8, ""u8, true),
        new("PST8PDT"u8, "PST"u8, "8PDT"u8, true),
        new("PST-08"u8, "PST"u8, "-08"u8, true),
        new("<A+B>+08"u8, "A+B"u8, "+08"u8, true)
    }.slice()) {
        var (name, @out, ok) = time_internal_test_package.TzsetName(test.@in);
        if (name != test.name || @out != test.@out || ok != test.ok) {
            Ꮡt.Errorf("tzsetName(%q) = %q, %q, %t, want %q, %q, %t"u8, test.@in, name, @out, ok, test.name, test.@out, test.ok);
        }
    }
}

[GoType("dyn")] internal partial struct TestTzsetOffset_type {
    internal @string @in;
    internal nint off;
    internal @string @out;
    internal bool ok;
}

public static void TestTzsetOffset(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestTzsetOffset_type[]{
        new(""u8, 0, ""u8, false),
        new("X"u8, 0, ""u8, false),
        new("+"u8, 0, ""u8, false),
        new("+08"u8, 8 * 60 * 60, ""u8, true),
        new("-01:02:03"u8, -1 * 60 * 60 - 2 * 60 - 3, ""u8, true),
        new("01"u8, 1 * 60 * 60, ""u8, true),
        new("100"u8, 100 * 60 * 60, ""u8, true),
        new("1000"u8, 0, ""u8, false),
        new("8PDT"u8, 8 * 60 * 60, "PDT"u8, true)
    }.slice()) {
        var (off, @out, ok) = time_internal_test_package.TzsetOffset(test.@in);
        if (off != test.off || @out != test.@out || ok != test.ok) {
            Ꮡt.Errorf("tzsetName(%q) = %d, %q, %t, want %d, %q, %t"u8, test.@in, off, @out, ok, test.off, test.@out, test.ok);
        }
    }
}

[GoType("dyn")] internal partial struct TestTzsetRule_type {
    internal @string @in;
    internal global::go.time_internal_test_package.Rule r;
    internal @string @out;
    internal bool ok;
}

public static void TestTzsetRule(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestTzsetRule_type[]{
        new(""u8, new time_internal_test_package.Rule(nil), ""u8, false),
        new("X"u8, new time_internal_test_package.Rule(nil), ""u8, false),
        new("J10"u8, new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleJulian, Day: 10, Time: 2 * 60 * 60), ""u8, true),
        new("20"u8, new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleDOY, Day: 20, Time: 2 * 60 * 60), ""u8, true),
        new("M1.2.3"u8, new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleMonthWeekDay, Mon: 1, Week: 2, Day: 3, Time: 2 * 60 * 60), ""u8, true),
        new("30/03:00:00"u8, new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleDOY, Day: 30, Time: 3 * 60 * 60), ""u8, true),
        new("M4.5.6/03:00:00"u8, new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleMonthWeekDay, Mon: 4, Week: 5, Day: 6, Time: 3 * 60 * 60), ""u8, true),
        new("M4.5.7/03:00:00"u8, new time_internal_test_package.Rule(nil), ""u8, false),
        new("M4.5.6/-04"u8, new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleMonthWeekDay, Mon: 4, Week: 5, Day: 6, Time: -4 * 60 * 60), ""u8, true)
    }.slice()) {
        var (r, @out, ok) = time_internal_test_package.TzsetRule(test.@in);
        if (r != test.r || @out != test.@out || ok != test.ok) {
            Ꮡt.Errorf("tzsetName(%q) = %#v, %q, %t, want %#v, %q, %t"u8, test.@in, r, @out, ok, test.r, test.@out, test.ok);
        }
    }
}

} // end time_test_package
