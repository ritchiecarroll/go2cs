// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using Δsync = sync_package;
using syscall = syscall_package;

partial class time_package {

//go:generate env ZONEINFO=$GOROOT/lib/time/zoneinfo.zip go run genzabbrs.go -output zoneinfo_abbrs_windows.go

// A Location maps time instants to the zone in use at that time.
// Typically, the Location represents the collection of time offsets
// in use in a geographical area. For many Locations the time offset varies
// depending on whether daylight savings time is in use at the time instant.
//
// Location is used to provide a time zone in a printed Time value and for
// calculations involving intervals that may cross daylight savings time
// boundaries.
partial struct ΔLocation {
    internal @string name;
    internal slice<zone> zone;
    internal slice<zoneTrans> tx;
    // The tzdata information can be followed by a string that describes
    // how to handle DST transitions not recorded in zoneTrans.
    // The format is the TZ environment variable without a colon; see
    // https://pubs.opengroup.org/onlinepubs/9699919799/basedefs/V1_chap08.html.
    // Example string, for America/Los_Angeles: PST8PDT,M3.2.0,M11.1.0
    internal @string extend;
    // Most lookups will be for the current time.
    // To avoid the binary search through tx, keep a
    // static one-element cache that gives the correct
    // zone for the time when the Location was created.
    // if cacheStart <= t < cacheEnd,
    // lookup can return cacheZone.
    // The units for cacheStart and cacheEnd are seconds
    // since January 1, 1970 UTC, to match the argument
    // to lookup.
    internal int64 cacheStart;
    internal int64 cacheEnd;
    internal ж<zone> cacheZone;
}

// A zone represents a single time zone such as CET.
partial struct zone {
    internal @string name; // abbreviated name, "CET"
    internal nint offset;   // seconds east of UTC
    internal bool isDST;   // is this zone Daylight Savings Time?
}

// A zoneTrans represents a single time zone transition.
partial struct zoneTrans {
    internal int64 when; // transition time, in seconds since 1970 GMT
    internal uint8 index; // the index of the zone that goes into effect at that time
    internal bool isstd, isutc;  // ignored - no idea what these mean
}

// alpha and omega are the beginning and end of time for zone
// transitions.
internal static UntypedInt alpha => /* -1 << 63 */ -9223372036854775808; // math.MinInt64

internal static UntypedInt omega => /* 1<<63 - 1 */ 9223372036854775807; // math.MaxInt64

// UTC represents Universal Coordinated Time (UTC).
public static ж<ΔLocation> ΔUTC;
internal static void initᴛΔUTC() { ΔUTC = ᏑutcLoc; }

// utcLoc is separate so that get can refer to &utcLoc
// and ensure that it never returns a nil *Location,
// even if a badly behaved client has changed UTC.
internal static ж<ΔLocation> ᏑutcLoc = new StandardBox<ΔLocation>(new ΔLocation(name: "UTC"u8));
internal static ref ΔLocation utcLoc => ref ᏑutcLoc.Value;

// Local represents the system's local time zone.
// On Unix systems, Local consults the TZ environment
// variable to find the time zone to use. No TZ means
// use the system default /etc/localtime.
// TZ="" means use UTC.
// TZ="foo" means use file foo in the system timezone directory.
public static ж<ΔLocation> ΔLocal;
internal static void initᴛΔLocal() { ΔLocal = ᏑlocalLoc; }

// localLoc is separate so that initLocal can initialize
// it even if a client has changed Local.
internal static ж<ΔLocation> ᏑlocalLoc = new StandardBox<ΔLocation>(default(ΔLocation));
internal static ref ΔLocation localLoc => ref ᏑlocalLoc.Value;

internal static ж<Δsync.Once> ᏑlocalOnce = new StandardBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once localOnce => ref ᏑlocalOnce.Value;

internal static ж<ΔLocation> get(this ж<ΔLocation> Ꮡl) {
    if (Ꮡl == nil) {
        return ᏑutcLoc;
    }
    if (Ꮡl == ᏑlocalLoc) {
        ᏑlocalOnce.Do(initLocal);
    }
    return Ꮡl;
}

// String returns a descriptive name for the time zone information,
// corresponding to the name argument to [LoadLocation] or [FixedZone].
public static @string String(this ж<ΔLocation> Ꮡl) {
    return (~Ꮡl.get()).name;
}

internal static slice<ж<ΔLocation>> unnamedFixedZones;

internal static ж<Δsync.Once> ᏑunnamedFixedZonesOnce = new StandardBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once unnamedFixedZonesOnce => ref ᏑunnamedFixedZonesOnce.Value;

// FixedZone returns a [Location] that always uses
// the given zone name and offset (seconds east of UTC).
public static ж<ΔLocation> FixedZone(@string name, nint offset) {
    // Most calls to FixedZone have an unnamed zone with an offset by the hour.
    // Optimize for that case by returning the same *Location for a given hour.
    UntypedInt hoursBeforeUTC = 12;
    UntypedInt hoursAfterUTC = 14;
    nint hour = offset / 60 / 60;
    if (name == ""u8 && (nint)(-hoursBeforeUTC) <= hour && hour <= (nint)(+hoursAfterUTC) && hour * 60 * 60 == offset) {
        ᏑunnamedFixedZonesOnce.Do(() => {
            unnamedFixedZones = new slice<ж<ΔLocation>>(hoursBeforeUTC + 1 + hoursAfterUTC);
            for (nint hr = -hoursBeforeUTC; hr <= (nint)(+hoursAfterUTC); hr++) {
                unnamedFixedZones[hr + (nint)hoursBeforeUTC] = fixedZone(""u8, hr * 60 * 60);
            }
        });
        return unnamedFixedZones[hour + (nint)hoursBeforeUTC];
    }
    return fixedZone(name, offset);
}

internal static ж<ΔLocation> fixedZone(@string name, nint offset) {
    var l = Ꮡ(new ΔLocation(
        name: name,
        zone: new zone[]{new(name, offset, false)}.slice(),
        tx: new zoneTrans[]{new(alpha, 0, false, false)}.slice(),
        cacheStart: alpha,
        cacheEnd: omega
    ));
    l.Value.cacheZone = Ꮡ((~l).zone, 0);
    return l;
}

// lookup returns information about the time zone in use at an
// instant in time expressed as seconds since January 1, 1970 00:00:00 UTC.
//
// The returned information gives the name of the zone (such as "CET"),
// the start and end times bracketing sec when that zone is in effect,
// the offset in seconds east of UTC (such as -5*60*60), and whether
// the daylight savings is being observed at that time.
internal static (@string name, nint offset, int64 start, int64 end, bool isDST) lookup(this ж<ΔLocation> Ꮡl, int64 sec) {
    @string name = default!;
    nint offset = default!;
    int64 start = default!;
    int64 end = default!;
    bool isDST = default!;

    ref var l = ref Ꮡl.DerefOrNull();
    Ꮡl = Ꮡl.get(); l = ref Ꮡl.DerefOrNull();
    if (len(l.zone) == 0) {
        name = utcˢ;
        offset = 0;
        start = alpha;
        end = omega;
        isDST = false;
        return (name, offset, start, end, isDST);
    }
    {
        var zoneΔ1 = l.cacheZone; if (zoneΔ1 != nil && l.cacheStart <= sec && sec < l.cacheEnd) {
            name = zoneΔ1.Value.name;
            offset = zoneΔ1.Value.offset;
            start = l.cacheStart;
            end = l.cacheEnd;
            isDST = zoneΔ1.Value.isDST;
            return (name, offset, start, end, isDST);
        }
    }
    if (len(l.tx) == 0 || sec < l.tx[0].when) {
        var zoneΔ2 = Ꮡ(l.zone, l.lookupFirstZone());
        name = zoneΔ2.Value.name;
        offset = zoneΔ2.Value.offset;
        start = alpha;
        if (len(l.tx) > 0){
            end = l.tx[0].when;
        } else {
            end = omega;
        }
        isDST = zoneΔ2.Value.isDST;
        return (name, offset, start, end, isDST);
    }
    // Binary search for entry with largest time <= sec.
    // Not using sort.Search to avoid dependencies.
    var tx = l.tx;
    end = omega;
    nint lo = 0;
    nint hi = len(tx);
    while (hi - lo > 1) {
        nint m = (nint)(((nuint)(lo + hi) >> (int)(1)));
        var lim = tx[m].when;
        if (sec < lim){
            end = lim;
            hi = m;
        } else {
            lo = m;
        }
    }
    var zone = Ꮡ(l.zone, tx[lo].index);
    name = zone.Value.name;
    offset = zone.Value.offset;
    start = tx[lo].when;
    // end = maintained during the search
    isDST = zone.Value.isDST;
    // If we're at the end of the known zone transitions,
    // try the extend string.
    if (lo == len(tx) - 1 && l.extend != ""u8) {
        {
            var (ename, eoffset, estart, eend, eisDST, ok) = tzset(l.extend, start, sec); if (ok) {
                return (ename, eoffset, estart, eend, eisDST);
            }
        }
    }
    return (name, offset, start, end, isDST);
}

// lookupFirstZone returns the index of the time zone to use for times
// before the first transition time, or when there are no transition
// times.
//
// The reference implementation in localtime.c from
// https://www.iana.org/time-zones/repository/releases/tzcode2013g.tar.gz
// implements the following algorithm for these cases:
//  1. If the first zone is unused by the transitions, use it.
//  2. Otherwise, if there are transition times, and the first
//     transition is to a zone in daylight time, find the first
//     non-daylight-time zone before and closest to the first transition
//     zone.
//  3. Otherwise, use the first zone that is not daylight time, if
//     there is one.
//  4. Otherwise, use the first zone.
internal static nint lookupFirstZone(this ref ΔLocation l) {
    // Case 1.
    if (!l.firstZoneUsed()) {
        return 0;
    }
    // Case 2.
    if (len(l.tx) > 0 && l.zone[l.tx[0].index].isDST) {
        for (nint zi = (nint)l.tx[0].index - 1; zi >= 0; zi--) {
            if (!l.zone[zi].isDST) {
                return zi;
            }
        }
    }
    // Case 3.
    foreach (var (zi, _) in l.zone) {
        if (!l.zone[zi].isDST) {
            return zi;
        }
    }
    // Case 4.
    return 0;
}

// firstZoneUsed reports whether the first zone is used by some
// transition.
internal static bool firstZoneUsed(this ref ΔLocation l) {
    foreach (var (_, tx) in l.tx) {
        if (tx.index == 0) {
            return true;
        }
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string m320M1110ˢ = ",M3.2.0,M11.1.0"u8;

// tzset takes a timezone string like the one found in the TZ environment
// variable, the time of the last time zone transition expressed as seconds
// since January 1, 1970 00:00:00 UTC, and a time expressed the same way.
// We call this a tzset string since in C the function tzset reads TZ.
// The return values are as for lookup, plus ok which reports whether the
// parse succeeded.
internal static (@string name, nint offset, int64 start, int64 end, bool isDST, bool ok) tzset(@string s, int64 lastTxSec, int64 sec) {
    bool ok = default!;

    @string stdName = default!;
    @string dstName = default!;
    nint stdOffset = default!;
    nint dstOffset = default!;
    (stdName, s, ok) = tzsetName(s);
    if (ok) {
        (stdOffset, s, ok) = tzsetOffset(s);
    }
    if (!ok) {
        return ("", 0, 0, 0, false, false);
    }
    // The numbers in the tzset string are added to local time to get UTC,
    // but our offsets are added to UTC to get local time,
    // so we negate the number we see here.
    stdOffset = -stdOffset;
    if (len(s) == 0 || s[0] == (rune)',') {
        // No daylight savings time.
        return (stdName, stdOffset, lastTxSec, omega, false, true);
    }
    (dstName, s, ok) = tzsetName(s);
    if (ok) {
        if (len(s) == 0 || s[0] == (rune)','){
            dstOffset = stdOffset + (nint)secondsPerHour;
        } else {
            (dstOffset, s, ok) = tzsetOffset(s);
            dstOffset = -dstOffset; // as with stdOffset, above
        }
    }
    if (!ok) {
        return ("", 0, 0, 0, false, false);
    }
    if (len(s) == 0) {
        // Default DST rules per tzcode.
        s = m320M1110ˢ;
    }
    // The TZ definition does not mention ';' here but tzcode accepts it.
    if (s[0] != (rune)',' && s[0] != (rune)';') {
        return ("", 0, 0, 0, false, false);
    }
    s = s[1..];
    rule startRule = default!;
    rule endRule = default!;
    (startRule, s, ok) = tzsetRule(s);
    if (!ok || len(s) == 0 || s[0] != (rune)',') {
        return ("", 0, 0, 0, false, false);
    }
    s = s[1..];
    (endRule, s, ok) = tzsetRule(s);
    if (!ok || len(s) > 0) {
        return ("", 0, 0, 0, false, false);
    }
    // Compute start of year in seconds since Unix epoch,
    // and seconds since then to get to sec.
    var (year, yday) = ((absSeconds)(uint64)(sec + unixToInternal + internalToAbsolute)).days().yearYday();
    var ysec = (int64)((yday - 1) * (nint)secondsPerDay) + sec % (int64)secondsPerDay;
    var ystart = sec - ysec;
    var startSec = (int64)tzruleTime(year, startRule, stdOffset);
    var endSec = (int64)tzruleTime(year, endRule, dstOffset);
    var (dstIsDST, stdIsDST) = (true, false);
    // Note: this is a flipping of "DST" and "STD" while retaining the labels
    // This happens in southern hemispheres. The labelling here thus is a little
    // inconsistent with the goal.
    if (endSec < startSec) {
        (startSec, endSec) = (endSec, startSec);
        (stdName, dstName) = (dstName, stdName);
        (stdOffset, dstOffset) = (dstOffset, stdOffset);
        (stdIsDST, dstIsDST) = (dstIsDST, stdIsDST);
    }
    // The start and end values that we return are accurate
    // close to a daylight savings transition, but are otherwise
    // just the start and end of the year. That suffices for
    // the only caller that cares, which is Date.
    if (ysec < startSec){
        return (stdName, stdOffset, ystart, startSec + ystart, stdIsDST, true);
    } else 
    if (ysec >= endSec){
        return (stdName, stdOffset, endSec + ystart, ystart + (int64)(365 * secondsPerDay), stdIsDST, true);
    } else {
        return (dstName, dstOffset, startSec + ystart, endSec + ystart, dstIsDST, true);
    }
}

// tzsetName returns the timezone name at the start of the tzset string s,
// and the remainder of s, and reports whether the parsing is OK.
internal static (@string, @string, bool) tzsetName(@string s) {
    if (len(s) == 0) {
        return ("", "", false);
    }
    if (s[0] != (rune)'<'){
        foreach (var (i, r) in s) {
            switch (r) {
            case (rune)'0' or (rune)'1' or (rune)'2' or (rune)'3' or (rune)'4' or (rune)'5' or (rune)'6' or (rune)'7' or (rune)'8' or (rune)'9' or (rune)',' or (rune)'-' or (rune)'+': {
                if (i < 3) {
                    return ("", "", false);
                }
                return (s.slice(0, i), s.slice(i), true);
            }}

        }
        if (len(s) < 3) {
            return ("", "", false);
        }
        return (s, "", true);
    } else {
        foreach (var (i, r) in s) {
            if (r == (rune)'>') {
                return (s.slice(1, i), s.slice(i + 1), true);
            }
        }
        return ("", "", false);
    }
}

// tzsetOffset returns the timezone offset at the start of the tzset string s,
// and the remainder of s, and reports whether the parsing is OK.
// The timezone offset is returned as a number of seconds.
internal static (nint offset, @string rest, bool ok) tzsetOffset(@string s) {
    bool ok = default!;

    if (len(s) == 0) {
        return (0, "", false);
    }
    var neg = false;
    if (s[0] == (rune)'+'){
        s = s[1..];
    } else 
    if (s[0] == (rune)'-') {
        s = s[1..];
        neg = true;
    }
    // The tzdata code permits values up to 24 * 7 here,
    // although POSIX does not.
    nint hours = default!;
    (hours, s, ok) = tzsetNum(s, 0, 24 * 7);
    if (!ok) {
        return (0, "", false);
    }
    nint off = hours * (nint)secondsPerHour;
    if (len(s) == 0 || s[0] != (rune)':') {
        if (neg) {
            off = -off;
        }
        return (off, s, true);
    }
    nint mins = default!;
    (mins, s, ok) = tzsetNum(s[1..], 0, 59);
    if (!ok) {
        return (0, "", false);
    }
    off += mins * (nint)secondsPerMinute;
    if (len(s) == 0 || s[0] != (rune)':') {
        if (neg) {
            off = -off;
        }
        return (off, s, true);
    }
    nint secs = default!;
    (secs, s, ok) = tzsetNum(s[1..], 0, 59);
    if (!ok) {
        return (0, "", false);
    }
    off += secs;
    if (neg) {
        off = -off;
    }
    return (off, s, true);
}

partial struct ruleKind /*num:nint*/;

internal static ruleKind ruleJulian => /* iota */ 0;
internal static ruleKind ruleDOY => 1;
internal static ruleKind ruleMonthWeekDay => 2;

// rule is a rule read from a tzset string.
partial struct rule {
    internal ruleKind kind;
    internal nint day;
    internal nint week;
    internal nint mon;
    internal nint time; // transition time
}

// tzsetRule parses a rule from a tzset string.
// It returns the rule, and the remainder of the string, and reports success.
internal static (rule, @string, bool) tzsetRule(@string s) {
    rule r = default!;
    if (len(s) == 0) {
        return (new rule(nil), "", false);
    }
    var ok = false;
    if (s[0] == (rune)'J'){
        nint jday = default!;
        (jday, s, ok) = tzsetNum(s[1..], 1, 365);
        if (!ok) {
            return (new rule(nil), "", false);
        }
        r.kind = ruleJulian;
        r.day = jday;
    } else 
    if (s[0] == (rune)'M'){
        nint mon = default!;
        (mon, s, ok) = tzsetNum(s[1..], 1, 12);
        if (!ok || len(s) == 0 || s[0] != (rune)'.') {
            return (new rule(nil), "", false);
        }
        nint week = default!;
        (week, s, ok) = tzsetNum(s[1..], 1, 5);
        if (!ok || len(s) == 0 || s[0] != (rune)'.') {
            return (new rule(nil), "", false);
        }
        nint day = default!;
        (day, s, ok) = tzsetNum(s[1..], 0, 6);
        if (!ok) {
            return (new rule(nil), "", false);
        }
        r.kind = ruleMonthWeekDay;
        r.day = day;
        r.week = week;
        r.mon = mon;
    } else {
        nint day = default!;
        (day, s, ok) = tzsetNum(s, 0, 365);
        if (!ok) {
            return (new rule(nil), "", false);
        }
        r.kind = ruleDOY;
        r.day = day;
    }
    if (len(s) == 0 || s[0] != (rune)'/') {
        r.time = 2 * secondsPerHour; // 2am is the default
        return (r, s, true);
    }
    (var offset, s, ok) = tzsetOffset(s[1..]);
    if (!ok) {
        return (new rule(nil), "", false);
    }
    r.time = offset;
    return (r, s, true);
}

// tzsetNum parses a number from a tzset string.
// It returns the number, and the remainder of the string, and reports success.
// The number must be between min and max.
internal static (nint num, @string rest, bool ok) tzsetNum(@string s, nint min, nint max) {
    nint num = default!;

    if (len(s) == 0) {
        return (0, "", false);
    }
    num = 0;
    foreach (var (i, r) in s) {
        if (r < (rune)'0' || r > (rune)'9') {
            if (i == 0 || num < min) {
                return (0, "", false);
            }
            return (num, s.slice(i), true);
        }
        num *= 10;
        num += (nint)r - (rune)'0';
        if (num > max) {
            return (0, "", false);
        }
    }
    if (num < min) {
        return (0, "", false);
    }
    return (num, "", true);
}

// tzruleTime takes a year, a rule, and a timezone offset,
// and returns the number of seconds since the start of the year
// that the rule takes effect.
internal static nint tzruleTime(nint year, rule r, nint off) {
    nint s = default!;
    var exprᴛ1 = r.kind;
    if (exprᴛ1 == ruleJulian) {
        s = (r.day - 1) * (nint)secondsPerDay;
        if (isLeap(year) && r.day >= 60) {
            s += secondsPerDay;
        }
    }
    else if (exprᴛ1 == ruleDOY) {
        s = r.day * (nint)secondsPerDay;
    }
    else if (exprᴛ1 == ruleMonthWeekDay) {
        nint m1 = (r.mon + 9) % 12 + 1;
        nint yy0 = year;
        if (r.mon <= 2) {
            // Zeller's Congruence.
            yy0--;
        }
        nint yy1 = yy0 / 100;
        nint yy2 = yy0 % 100;
        nint dow = ((26 * m1 - 2) / 10 + 1 + yy2 + yy2 / 4 + yy1 / 4 - 2 * yy1) % 7;
        if (dow < 0) {
            dow += 7;
        }
        nint d = r.day - dow;
        if (d < 0) {
            // Now dow is the day-of-week of the first day of r.mon.
            // Get the day-of-month of the first "dow" day.
            d += 7;
        }
        for (nint i = 1; i < r.week; i++) {
            if (d + 7 >= daysIn(((ΔMonth)r.mon), year)) {
                break;
            }
            d += 7;
        }
        d += (nint)daysBefore(((ΔMonth)r.mon));
        if (isLeap(year) && r.mon > 2) {
            d++;
        }
        s = d * (nint)secondsPerDay;
    }

    return s + r.time - off;
}

// lookupName returns information about the time zone with
// the given name (such as "EST") at the given pseudo-Unix time
// (what the given time of day would be in UTC).
internal static (nint offset, bool ok) lookupName(this ж<ΔLocation> Ꮡl, @string name, int64 unix) {
    nint offset = default!;
    bool ok = default!;

    ref var l = ref Ꮡl.DerefOrNull();
    Ꮡl = Ꮡl.get(); l = ref Ꮡl.DerefOrNull();
    // First try for a zone with the right name that was actually
    // in effect at the given time. (In Sydney, Australia, both standard
    // and daylight-savings time are abbreviated "EST". Using the
    // offset helps us pick the right one for the given time.
    // It's not perfect: during the backward transition we might pick
    // either one.)
    foreach (var (i, _) in l.zone) {
        var zone = Ꮡ(l.zone, i);
        if ((~zone).name == name) {
            var (nam, offsetΔ1, _, _, _) = Ꮡl.lookup(unix - (int64)(~zone).offset);
            if (nam == (~zone).name) {
                return (offsetΔ1, true);
            }
        }
    }
    // Otherwise fall back to an ordinary name match.
    foreach (var (i, _) in l.zone) {
        var zone = Ꮡ(l.zone, i);
        if ((~zone).name == name) {
            return ((~zone).offset, true);
        }
    }
    // Otherwise, give up.
    return (offset, ok);
}

// NOTE(rsc): Eventually we will need to accept the POSIX TZ environment
// syntax too, but I don't feel like implementing it today.
internal static error errLocation = errors.New("time: invalid location name"u8);

internal static ж<@string> zoneinfo;

internal static ж<Δsync.Once> ᏑzoneinfoOnce = new StandardBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once zoneinfoOnce => ref ᏑzoneinfoOnce.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zoneinfoˢ = "ZONEINFO"u8;

// LoadLocation returns the Location with the given name.
//
// If the name is "" or "UTC", LoadLocation returns UTC.
// If the name is "Local", LoadLocation returns Local.
//
// Otherwise, the name is taken to be a location name corresponding to a file
// in the IANA Time Zone database, such as "America/New_York".
//
// LoadLocation looks for the IANA Time Zone database in the following
// locations in order:
//
//   - the directory or uncompressed zip file named by the ZONEINFO environment variable
//   - on a Unix system, the system standard installation location
//   - $GOROOT/lib/time/zoneinfo.zip
//   - the time/tzdata package, if it was imported
public static (ж<ΔLocation>, error) LoadLocation(@string name) {
    if (name == ""u8 || name == "UTC"u8) {
        return (ΔUTC, default!);
    }
    if (name == "Local"u8) {
        return (ΔLocal, default!);
    }
    if (containsDotDot(name) || name[0] == (rune)'/' || name[0] == (rune)'\\') {
        // No valid IANA Time Zone name contains a single dot,
        // much less dot dot. Likewise, none begin with a slash.
        return (default!, errLocation);
    }
    ᏑzoneinfoOnce.Do(() => {
        ref var env = ref heap<@string>(out var Ꮡenv);
        (env, _) = syscall.Getenv(zoneinfoˢ);
        zoneinfo = Ꮡenv;
    });
    error firstErr = default!;
    if (zoneinfo.Value != ""u8) {
        {
            var (zoneData, err) = loadTzinfoFromDirOrZip(zoneinfo.Value, name); if (err == default!){
                {
                    var (z, errΔ1) = LoadLocationFromTZData(name, zoneData); if (errΔ1 == default!) {
                        return (z, default!);
                    }
                }
                firstErr = err;
            } else 
            if (!AreEqual(err, syscall.ENOENT)) {
                firstErr = err;
            }
        }
    }
    {
        var (z, err) = loadLocation(name, platformZoneSources); if (err == default!){
            return (z, default!);
        } else 
        if (firstErr == default!) {
            firstErr = err;
        }
    }
    return (default!, firstErr);
}

// containsDotDot reports whether s contains "..".
internal static bool containsDotDot(@string s) {
    if (len(s) < 2) {
        return false;
    }
    for (nint i = 0; i < len(s) - 1; i++) {
        if (s[i] == (rune)'.' && s[i + 1] == (rune)'.') {
            return true;
        }
    }
    return false;
}

} // end time_package
