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
internal static ref readonly @string asiaJerusalemˢ => ref asiaJerusalemˢᵇ;
private static readonly @string asiaJerusalemˢᵇ = "Asia/Jerusalem"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string testZoneinfoᶜ => ref testZoneinfoᶜᵇ;
private static readonly @string testZoneinfoᶜᵇ = "foo.zip"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string envᶜ => ref envᶜᵇ;
private static readonly @string envᶜᵇ = "ZONEINFO"u8;

public static void TestEnvVarUsage(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        time_internal_test_package.ResetZoneinfoForTesting();

        @string testZoneinfo = testZoneinfoᶜ;
        @string env = envᶜ;

        t.Setenv(env, testZoneinfo);

        // Result isn't important, we're testing the side effect of this command
        Δtime.LoadLocation(asiaJerusalemˢ);
        defer(time_internal_test_package.ResetZoneinfoForTesting, ref ᒐ);

        {
            var zoneinfo = time_internal_test_package.ZoneinfoForTesting(); if (testZoneinfo != zoneinfo.Value) {
                t.Errorf("zoneinfo does not match env variable: got %q want %q", zoneinfo.Value, testZoneinfo);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string asiaSomethingNotExistˢ => ref asiaSomethingNotExistˢᵇ;
private static readonly @string asiaSomethingNotExistˢᵇ = "Asia/SomethingNotExist"u8;

public static void TestBadLocationErrMsg(ж<Δtesting.T> t) {
    time_internal_test_package.ResetZoneinfoForTesting();
    @string loc = asiaSomethingNotExistˢ;
    var want = errors.New("unknown time zone "u8 + loc);
    var (_, err) = Δtime.LoadLocation(loc);
    if (err.Error() != want.Error()) {
        t.Errorf("LoadLocation(%q) error = %v; want %v", loc, err, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string envᶜ1 => ref envᶜ1ᵇ;
private static readonly @string envᶜ1ᵇ = "ZONEINFO"u8;

public static void TestLoadLocationValidatesNames(ж<Δtesting.T> t) {
    time_internal_test_package.ResetZoneinfoForTesting();
    @string env = envᶜ1;
    t.Setenv(env, ""u8);

    var bad = slice<@string>([
        "/usr/foo/Foo",
        "\\UNC\foo",
        "..",
        "a.."
    ]);
    foreach (var (_, v) in bad) {
        var (_, err) = Δtime.LoadLocation(v);
        if (!AreEqual(err, time_internal_test_package.ErrLocation)) {
            t.Errorf("LoadLocation(%q) error = %v; want ErrLocation", v, err);
        }
    }
}

public static void TestVersion3(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);
        var (_, err) = Δtime.LoadLocation(asiaJerusalemˢ);
        if (err != nil) {
            t.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string formatᶜ => ref formatᶜᵇ;
private static readonly @string formatᶜᵇ = "Mon, 02 Jan 2006 15:04:05 -0700 (MST)"u8;

internal partial struct TestFirstZone_type /*dyn*/ {
    internal @string zone;
    internal int64 unix;
    internal @string want1;
    internal @string want2;
}

// Test that we get the correct results for times before the first
// transition time. To do this we explicitly check early dates in a
// couple of specific timezones.
public static void TestFirstZone(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);

        @string format = formatᶜ;
        slice<TestFirstZone_type> tests = slice<TestFirstZone_type>([
            new(
                "PST8PDT",
                -1633269601,
                "Sun, 31 Mar 1918 01:59:59 -0800 (PST)",
                "Sun, 31 Mar 1918 03:00:00 -0700 (PDT)"
            ),
            new(
                "Pacific/Fakaofo",
                1325242799,
                "Thu, 29 Dec 2011 23:59:59 -1100 (-11)",
                "Sat, 31 Dec 2011 00:00:00 +1300 (+13)"
            )
        ]);

        foreach (var (_, test) in tests) {
            var (z, err) = Δtime.LoadLocation(test.zone);
            if (err != nil) {
                t.Fatal(err);
            }
            @string s = Δtime.Unix(test.unix, 0).In(z).Format(format);
            if (s != test.want1) {
                t.Errorf("for %s %d got %q want %q", test.zone, test.unix, s, test.want1);
            }
            s = Δtime.Unix(test.unix + 1, 0).In(z).Format(format);
            if (s != test.want2) {
                t.Errorf("for %s %d got %q want %q", test.zone, test.unix, s, test.want2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestLocationNames(ж<Δtesting.T> t) {
    if (Δtime.ΔLocal.String() != "Local"u8) {
        t.Errorf(@"invalid Local location name: got %q want ""Local""", Δtime.ΔLocal.OrTypedNil());
    }
    if (Δtime.ΔUTC.String() != "UTC"u8) {
        t.Errorf(@"invalid UTC location name: got %q want ""UTC""", Δtime.ΔUTC.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToLocateTzinfoˢ = (@string)"Failed to locate tzinfo source in GOROOT."u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string locationNameᶜ => ref locationNameᶜᵇ;
private static readonly @string locationNameᶜᵇ = "Asia/Jerusalem"u8;

public static void TestLoadLocationFromTZData(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);

        @string locationName = locationNameᶜ;
        var (reference, err) = Δtime.LoadLocation(locationName);
        if (err != nil) {
            t.Fatal(err);
        }

        var (gorootSource, ok) = time_internal_test_package.GorootZoneSource(testenv.GOROOT(new time_test_package.testing_TжTB(t)));
        if (!ok) {
            t.Fatal(failedToLocateTzinfoˢ);
        }
        (var tzinfo, err) = time_internal_test_package.LoadTzinfo(locationName, gorootSource);
        if (err != nil) {
            t.Fatal(err);
        }
        (var sample, err) = Δtime.LoadLocationFromTZData(locationName, tzinfo);
        if (err != nil) {
            t.Fatal(err);
        }

        if (!reflect.DeepEqual(reference.OrTypedNil(), sample.OrTypedNil())) {
            t.Errorf("return values of LoadLocationFromTZData and LoadLocation don't match");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string estˢ => ref estˢᵇ;
private static readonly @string estˢᵇ = "EST"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string locNameᶜ => ref locNameᶜᵇ;
private static readonly @string locNameᶜᵇ = "America/New_York"u8;

// Issue 30099.
public static void TestEarlyLocation(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);

        @string locName = locNameᶜ;
        var (loc, err) = Δtime.LoadLocation(locName);
        if (err != nil) {
            t.Fatal(err);
        }

        var d = Δtime.Date(1900, Δtime.January, 1, 0, 0, 0, 0, loc);
        var (tzName, tzOffset) = d.Zone();
        {
            @string want = estˢ; if (tzName != want) {
                t.Errorf("Zone name == %s, want %s", tzName, want);
            }
        }
        {
            nint want = -18000; if (tzOffset != want) {
                t.Errorf("Zone offset == %d, want %d", tzOffset, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = "abc"u8;
internal static readonly object expectedErrorGotNoneˢ = (@string)"expected error, got none"u8;

public static void TestMalformedTZData(ж<Δtesting.T> t) {
    // The goal here is just that malformed tzdata results in an error, not a panic.
    @string issue29437 = ((@string)(new byte[]{0x54, 0x5a, 0x69, 0x66, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30}));
    var (_, err) = Δtime.LoadLocationFromTZData(abcˢ, slice<byte>(issue29437));
    if (err == nil) {
        t.Error(expectedErrorGotNoneˢ);
    }
}

partial struct slimTestsᴛ1 /*dyn*/ {
    internal @string zoneName;
    internal @string fileName;
    internal Func<ж<timeꓸLocation>, Δtime.Time> date;
    internal @string wantName;
    internal nint wantOffset;
}
internal static slice<slimTestsᴛ1> slimTests = slice<slimTestsᴛ1>([
    new(
        // 2020b slim tzdata for Europe/Berlin.
        zoneName: "Europe/Berlin",
        fileName: "2020b_Europe_Berlin",
        date: (ж<timeꓸLocation> loc) => Δtime.Date(2020, Δtime.October, 29, 15, 30, 0, 0, loc),
        wantName: "CET",
        wantOffset: 3600
    ),
    new(
        // 2021a slim tzdata for America/Nuuk.
        zoneName: "America/Nuuk",
        fileName: "2021a_America_Nuuk",
        date: (ж<timeꓸLocation> loc) => Δtime.Date(2020, Δtime.October, 29, 15, 30, 0, 0, loc),
        wantName: "-03",
        wantOffset: -10800
    ),
    new(
        // 2021a slim tzdata for Asia/Gaza.
        zoneName: "Asia/Gaza",
        fileName: "2021a_Asia_Gaza",
        date: (ж<timeꓸLocation> loc) => Δtime.Date(2020, Δtime.October, 29, 15, 30, 0, 0, loc),
        wantName: "EET",
        wantOffset: 7200
    ),
    new(
        // 2021a slim tzdata for Europe/Dublin.
        zoneName: "Europe/Dublin",
        fileName: "2021a_Europe_Dublin",
        date: (ж<timeꓸLocation> loc) => Δtime.Date(2021, Δtime.April, 2, 11, 12, 13, 0, loc),
        wantName: "IST",
        wantOffset: 3600
    )
]);

public static void TestLoadLocationFromTZDataSlim(ж<Δtesting.T> t) {
    foreach (var (_, test) in slimTests) {
        var (tzData, err) = Δos.ReadFile("testdata/"u8 + test.fileName);
        if (err != nil) {
            t.Error(err);
            continue;
        }
        (var reference, err) = Δtime.LoadLocationFromTZData(test.zoneName, tzData);
        if (err != nil) {
            t.Error(err);
            continue;
        }

        var d = test.date(reference);
        var (tzName, tzOffset) = d.Zone();
        if (tzName != test.wantName) {
            t.Errorf("Zone name == %s, want %s", tzName, test.wantName);
        }
        if (tzOffset != test.wantOffset) {
            t.Errorf("Zone offset == %d, want %d", tzOffset, test.wantOffset);
        }
    }
}

internal partial struct TestTzset_type /*dyn*/ {
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

public static void TestTzset(ж<Δtesting.T> t) {
    foreach (var (_, test) in slice<TestTzset_type>([
        new("", 0, 0, "", 0, 0, 0, false, false),
        new("PST8PDT,M3.2.0,M11.1.0", 0, 2159200800L, "PDT", -7 * 60 * 60, 2152173600L, 2172733200L, true, true),
        new("PST8PDT,M3.2.0,M11.1.0", 0, 2152173599L, "PST", -8 * 60 * 60, 2145916800, 2152173600L, false, true),
        new("PST8PDT,M3.2.0,M11.1.0", 0, 2152173600L, "PDT", -7 * 60 * 60, 2152173600L, 2172733200L, true, true),
        new("PST8PDT,M3.2.0,M11.1.0", 0, 2152173601L, "PDT", -7 * 60 * 60, 2152173600L, 2172733200L, true, true),
        new("PST8PDT,M3.2.0,M11.1.0", 0, 2172733199L, "PDT", -7 * 60 * 60, 2152173600L, 2172733200L, true, true),
        new("PST8PDT,M3.2.0,M11.1.0", 0, 2172733200L, "PST", -8 * 60 * 60, 2172733200L, 2177452800L, false, true),
        new("PST8PDT,M3.2.0,M11.1.0", 0, 2172733201L, "PST", -8 * 60 * 60, 2172733200L, 2177452800L, false, true),
        new("KST-9", 592333200, 1677246697, "KST", 9 * 60 * 60, 592333200, 9223372036854775807L, false, true)
    ])) {
        var (name, off, start, end, isDST, ok) = time_internal_test_package.Tzset(test.inStr, test.inEnd, test.inSec);
        if (name != test.name || off != test.off || start != test.start || end != test.end || isDST != test.isDST || ok != test.ok) {
            t.Errorf("tzset(%q, %d, %d) = %q, %d, %d, %d, %t, %t, want %q, %d, %d, %d, %t, %t", test.inStr, test.inEnd, test.inSec, name, off, start, end, isDST, ok, test.name, test.off, test.start, test.end, test.isDST, test.ok);
        }
    }
}

internal partial struct TestTzsetName_type /*dyn*/ {
    internal @string @in;
    internal @string name;
    internal @string @out;
    internal bool ok;
}

public static void TestTzsetName(ж<Δtesting.T> t) {
    foreach (var (_, test) in slice<TestTzsetName_type>([
        new("", "", "", false),
        new("X", "", "", false),
        new("PST", "PST", "", true),
        new("PST8PDT", "PST", "8PDT", true),
        new("PST-08", "PST", "-08", true),
        new("<A+B>+08", "A+B", "+08", true)
    ])) {
        var (name, @out, ok) = time_internal_test_package.TzsetName(test.@in);
        if (name != test.name || @out != test.@out || ok != test.ok) {
            t.Errorf("tzsetName(%q) = %q, %q, %t, want %q, %q, %t", test.@in, name, @out, ok, test.name, test.@out, test.ok);
        }
    }
}

internal partial struct TestTzsetOffset_type /*dyn*/ {
    internal @string @in;
    internal nint off;
    internal @string @out;
    internal bool ok;
}

public static void TestTzsetOffset(ж<Δtesting.T> t) {
    foreach (var (_, test) in slice<TestTzsetOffset_type>([
        new("", 0, "", false),
        new("X", 0, "", false),
        new("+", 0, "", false),
        new("+08", 8 * 60 * 60, "", true),
        new("-01:02:03", -1 * 60 * 60 - 2 * 60 - 3, "", true),
        new("01", 1 * 60 * 60, "", true),
        new("100", 100 * 60 * 60, "", true),
        new("1000", 0, "", false),
        new("8PDT", 8 * 60 * 60, "PDT", true)
    ])) {
        var (off, @out, ok) = time_internal_test_package.TzsetOffset(test.@in);
        if (off != test.off || @out != test.@out || ok != test.ok) {
            t.Errorf("tzsetName(%q) = %d, %q, %t, want %d, %q, %t", test.@in, off, @out, ok, test.off, test.@out, test.ok);
        }
    }
}

internal partial struct TestTzsetRule_type /*dyn*/ {
    internal @string @in;
    internal global::go.time_internal_test_package.Rule r;
    internal @string @out;
    internal bool ok;
}

public static void TestTzsetRule(ж<Δtesting.T> t) {
    foreach (var (_, test) in slice<TestTzsetRule_type>([
        new("", new time_internal_test_package.Rule(), "", false),
        new("X", new time_internal_test_package.Rule(), "", false),
        new("J10", new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleJulian, Day: 10, Time: 2 * 60 * 60), "", true),
        new("20", new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleDOY, Day: 20, Time: 2 * 60 * 60), "", true),
        new("M1.2.3", new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleMonthWeekDay, Mon: 1, Week: 2, Day: 3, Time: 2 * 60 * 60), "", true),
        new("30/03:00:00", new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleDOY, Day: 30, Time: 3 * 60 * 60), "", true),
        new("M4.5.6/03:00:00", new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleMonthWeekDay, Mon: 4, Week: 5, Day: 6, Time: 3 * 60 * 60), "", true),
        new("M4.5.7/03:00:00", new time_internal_test_package.Rule(), "", false),
        new("M4.5.6/-04", new time_internal_test_package.Rule(Kind: time_internal_test_package.RuleMonthWeekDay, Mon: 4, Week: 5, Day: 6, Time: -4 * 60 * 60), "", true)
    ])) {
        var (r, @out, ok) = time_internal_test_package.TzsetRule(test.@in);
        if (r != test.r || @out != test.@out || ok != test.ok) {
            t.Errorf("tzsetName(%q) = %#v, %q, %t, want %#v, %q, %t", test.@in, r, @out, ok, test.r, test.@out, test.ok);
        }
    }
}

} // end time_test_package
