// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δsync = sync_package;
using static go.time_package;

partial class time_internal_test_package {

public static void ResetLocalOnceForTest() {
    localOnce = new Δsync.Once(nil);
    localLoc = new ΔLocation(nil);
}

public static void ForceUSPacificForTesting() {
    ResetLocalOnceForTest();
    ᏑlocalOnce.Do(initTestingZone);
}

public static ж<@string> ZoneinfoForTesting() {
    return zoneinfo;
}

public static void ResetZoneinfoForTesting() {
    zoneinfo = default!;
    zoneinfoOnce = new Δsync.Once(nil);
}

public static Func<Action> DisablePlatformSources;
internal static void initᴛDisablePlatformSources() { DisablePlatformSources = disablePlatformSources; }
public static Func<@string, (@string, bool)> GorootZoneSource = gorootZoneSource;
public static Func<@string, (nint, bool)> ParseTimeZone;
internal static void initᴛParseTimeZone() { ParseTimeZone = parseTimeZone; }
public static Action<ж<global::go.time_package.Time>, int64> SetMono = ((Action<ж<global::go.time_package.Time>, int64>)(global::go.time_package.setMono));
public static Func<ж<global::go.time_package.Time>, int64> GetMono = ((Func<ж<global::go.time_package.Time>, int64>)(global::go.time_package.mono));
public static error ErrLocation;
internal static void initᴛErrLocation() { ErrLocation = errLocation; }
public static Func<@string, (slice<byte>, error)> ReadFile = readFile;
public static Func<@string, @string, (slice<byte>, error)> LoadTzinfo;
internal static void initᴛLoadTzinfo() { LoadTzinfo = loadTzinfo; }
public static Func<@string, (@string, nint, @string)> NextStdChunk;
internal static void initᴛNextStdChunk() { NextStdChunk = nextStdChunk; }
public static Func<@string, int64, int64, (@string, nint, int64, int64, bool, bool)> Tzset = tzset;
public static Func<@string, (@string, @string, bool)> TzsetName = tzsetName;
public static Func<@string, (nint, @string, bool)> TzsetOffset = tzsetOffset;

public static (@string, error) LoadFromEmbeddedTZData(@string zone) {
    return loadFromEmbeddedTZData(zone);
}

public partial struct RuleKind /*num:nint*/;

public static RuleKind RuleJulian => /* RuleKind(ruleJulian) */ 0;
public static RuleKind RuleDOY => /* RuleKind(ruleDOY) */ 1;
public static RuleKind RuleMonthWeekDay => /* RuleKind(ruleMonthWeekDay) */ 2;
public const int64 UnixToInternal = /* unixToInternal */ 62135596800;

public partial struct Rule {
    public RuleKind Kind;
    public nint Day;
    public nint Week;
    public nint Mon;
    public nint Time;
}

public static (Rule, @string, bool) TzsetRule(@string s) {
    var (r, rs, ok) = tzsetRule(s);
    var rr = new Rule(
        Kind: ((RuleKind)(nint)r.kind),
        Day: r.day,
        Week: r.week,
        Mon: r.mon,
        Time: r.time
    );
    return (rr, rs, ok);
}

// StdChunkNames maps from nextStdChunk results to the matched strings.
public static map<nint, @string> StdChunkNames = new map<nint, @string>{
    [0] = ""u8,
    [stdLongMonth] = "January"u8,
    [stdMonth] = "Jan"u8,
    [stdNumMonth] = "1"u8,
    [stdZeroMonth] = "01"u8,
    [stdLongWeekDay] = "Monday"u8,
    [stdWeekDay] = "Mon"u8,
    [stdDay] = "2"u8,
    [stdUnderDay] = "_2"u8,
    [stdZeroDay] = "02"u8,
    [stdUnderYearDay] = "__2"u8,
    [stdZeroYearDay] = "002"u8,
    [stdHour] = "15"u8,
    [stdHour12] = "3"u8,
    [stdZeroHour12] = "03"u8,
    [stdMinute] = "4"u8,
    [stdZeroMinute] = "04"u8,
    [stdSecond] = "5"u8,
    [stdZeroSecond] = "05"u8,
    [stdLongYear] = "2006"u8,
    [stdYear] = "06"u8,
    [stdPM] = "PM"u8,
    [stdpm] = "pm"u8,
    [stdTZ] = "MST"u8,
    [stdISO8601TZ] = "Z0700"u8,
    [stdISO8601SecondsTZ] = "Z070000"u8,
    [stdISO8601ShortTZ] = "Z07"u8,
    [stdISO8601ColonTZ] = "Z07:00"u8,
    [stdISO8601ColonSecondsTZ] = "Z07:00:00"u8,
    [stdNumTZ] = "-0700"u8,
    [stdNumSecondsTz] = "-070000"u8,
    [stdNumShortTZ] = "-07"u8,
    [stdNumColonTZ] = "-07:00"u8,
    [stdNumColonSecondsTZ] = "-07:00:00"u8,
    [(nint)((nint)stdFracSecond0 | (nint)(1 << (int)(stdArgShift)))] = ".0"u8,
    [(nint)((nint)stdFracSecond0 | (nint)(2 << (int)(stdArgShift)))] = ".00"u8,
    [(nint)((nint)stdFracSecond0 | (nint)(3 << (int)(stdArgShift)))] = ".000"u8,
    [(nint)((nint)stdFracSecond0 | (nint)(4 << (int)(stdArgShift)))] = ".0000"u8,
    [(nint)((nint)stdFracSecond0 | (nint)(5 << (int)(stdArgShift)))] = ".00000"u8,
    [(nint)((nint)stdFracSecond0 | (nint)(6 << (int)(stdArgShift)))] = ".000000"u8,
    [(nint)((nint)stdFracSecond0 | (nint)(7 << (int)(stdArgShift)))] = ".0000000"u8,
    [(nint)((nint)stdFracSecond0 | (nint)(8 << (int)(stdArgShift)))] = ".00000000"u8,
    [(nint)((nint)stdFracSecond0 | (nint)(9 << (int)(stdArgShift)))] = ".000000000"u8,
    [(nint)((nint)stdFracSecond9 | (nint)(1 << (int)(stdArgShift)))] = ".9"u8,
    [(nint)((nint)stdFracSecond9 | (nint)(2 << (int)(stdArgShift)))] = ".99"u8,
    [(nint)((nint)stdFracSecond9 | (nint)(3 << (int)(stdArgShift)))] = ".999"u8,
    [(nint)((nint)stdFracSecond9 | (nint)(4 << (int)(stdArgShift)))] = ".9999"u8,
    [(nint)((nint)stdFracSecond9 | (nint)(5 << (int)(stdArgShift)))] = ".99999"u8,
    [(nint)((nint)stdFracSecond9 | (nint)(6 << (int)(stdArgShift)))] = ".999999"u8,
    [(nint)((nint)stdFracSecond9 | (nint)(7 << (int)(stdArgShift)))] = ".9999999"u8,
    [(nint)((nint)stdFracSecond9 | (nint)(8 << (int)(stdArgShift)))] = ".99999999"u8,
    [(nint)((nint)stdFracSecond9 | (nint)(9 << (int)(stdArgShift)))] = ".999999999"u8
};

public static Func<@string, @string> Quote;
internal static void initᴛQuote() { Quote = quote; }

public static Func<slice<byte>, nint, nint, slice<byte>> AppendInt = appendInt;

public static Func<global::go.time_package.Time, slice<byte>, @string, slice<byte>> AppendFormatAny;
internal static void initᴛAppendFormatAny() { AppendFormatAny = ((Func<global::go.time_package.Time, slice<byte>, @string, slice<byte>>)(global::go.time_package.appendFormat)); }

public static Func<global::go.time_package.Time, slice<byte>, bool, slice<byte>> AppendFormatRFC3339;
internal static void initᴛAppendFormatRFC3339() { AppendFormatRFC3339 = ((Func<global::go.time_package.Time, slice<byte>, bool, slice<byte>>)(global::go.time_package.appendFormatRFC3339)); }

public static Func<@string, @string, ж<global::go.time_package.ΔLocation>, ж<global::go.time_package.ΔLocation>, (global::go.time_package.Time, error)> ParseAny;
internal static void initᴛParseAny() { ParseAny = parse; }

public static Func<@string, ж<global::go.time_package.ΔLocation>, (global::go.time_package.Time, bool)> ParseRFC3339;
internal static void initᴛParseRFC3339() { ParseRFC3339 = parseRFC3339<@string>; }

} // end time_internal_test_package
