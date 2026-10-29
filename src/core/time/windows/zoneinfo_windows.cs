// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using registry = @internal.syscall.windows.registry_package;
using syscall = syscall_package;
using @internal.syscall.windows;

partial class time_package {

internal static slice<@string> platformZoneSources; // none: Windows uses system calls instead

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string muiStdˢ = "MUI_Std"u8;
internal static readonly @string muiDltˢ = "MUI_Dlt"u8;
internal static readonly @string stdˢ = "Std"u8;
internal static readonly @string dltˢ = "Dlt"u8;

// TODO(rsc): Fall back to copy of zoneinfo files.
// BUG(brainman,rsc): On Windows, the operating system does not provide complete
// time zone information.
// The implementation assumes that this year's rules for daylight savings
// time apply to all previous and future years as well.

// matchZoneKey checks if stdname and dstname match the corresponding key
// values "MUI_Std" and "MUI_Dlt" or "Std" and "Dlt" in the kname key stored
// under the open registry key zones.
internal static (bool matched, error err2) matchZoneKey(registry.Key zones, @string kname, @string stdname, @string dstname) {
    bool matched = default;
    error err2 = default;
    GoFrame ᒐ = default;
    try {
        var (k, err) = registry.OpenKey(zones, kname, registry.READ);
        if (err != nil) {
            (matched, err2) = (false, err); goto ᒐdone;
        }
        defer(() => k.Close(), ref ᒐ);

        @string std = default;
        @string dlt = default;
        // Try MUI_Std and MUI_Dlt first, fallback to Std and Dlt if *any* error occurs
        (std, err) = k.GetMUIStringValue(muiStdˢ);
        if (err == nil) {
            (dlt, err) = k.GetMUIStringValue(muiDltˢ);
        }
        if (err != nil) { // Fallback to Std and Dlt
            {
                (std, _, err) = k.GetStringValue(stdˢ); if (err != nil) {
                    (matched, err2) = (false, err); goto ᒐdone;
                }
            }
            {
                (dlt, _, err) = k.GetStringValue(dltˢ); if (err != nil) {
                    (matched, err2) = (false, err); goto ᒐdone;
                }
            }
        }

        if (std != stdname) {
            (matched, err2) = (false, default); goto ᒐdone;
        }
        if (dlt != dstname && dstname != stdname) {
            (matched, err2) = (false, default); goto ᒐdone;
        }
        (matched, err2) = (true, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (matched, err2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string softwareMicrosoftWindowsˢ = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones"u8;

// toEnglishName searches the registry for an English name of a time zone
// whose zone names are stdname and dstname and returns the English name.
internal static (@string, error) toEnglishName(@string stdname, @string dstname) {
    GoFrame ᒐ = default;
    try {
        var (k, err) = registry.OpenKey(registry.LOCAL_MACHINE, softwareMicrosoftWindowsˢ, (uint32)((uint32)registry.ENUMERATE_SUB_KEYS | (uint32)registry.QUERY_VALUE));
        if (err != nil) {
            return ("", err);
        }
        defer(() => k.Close(), ref ᒐ);

        (var names, err) = k.ReadSubKeyNames();
        if (err != nil) {
            return ("", err);
        }
        foreach (var (_, name) in names) {
            var (matched, errΔ1) = matchZoneKey(k, name, stdname, dstname);
            if (errΔ1 == nil && matched) {
                return (name, default);
            }
        }
        return ("", errors.New(@"English name for time zone """u8 + stdname + @""" not found in registry"u8));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// extractCAPS extracts capital letters from description desc.
internal static @string extractCAPS(@string desc) {
    var shortᴷ = new stackAppend<rune>();

    slice<rune> @short = default;
    foreach (var (_, c) in desc) {
        if ('A' <= c && c <= 'Z') {
            @short = append(shortᴷ, @short, c);
        }
    }
    return (@string)@short;
}

// abbrev returns the abbreviations to use for the given zone z.
internal static (@string std, @string dst) abbrev(ref syscall.Timezoneinformation z) {
    @string stdName = syscall.UTF16ToString(z.StandardName[..]);
    var (a, ok) = abbrs[stdName, ꟷ];
    if (!ok) {
        @string dstName = syscall.UTF16ToString(z.DaylightName[..]);
        // Perhaps stdName is not English. Try to convert it.
        var (englishName, err) = toEnglishName(stdName, dstName);
        if (err == nil) {
            (a, ok) = abbrs[englishName, ꟷ];
            if (ok) {
                return (a.std, a.dst);
            }
        }
        // fallback to using capital letters
        return (extractCAPS(stdName), extractCAPS(dstName));
    }
    return (a.std, a.dst);
}

// pseudoUnix returns the pseudo-Unix time (seconds since Jan 1 1970 *LOCAL TIME*)
// denoted by the system date+time d in the given year.
// It is up to the caller to convert this local time into a UTC-based time.
internal static int64 pseudoUnix(nint year, ref syscall.Systemtime d) {
    // Windows specifies daylight savings information in "day in month" format:
    // d.Month is month number (1-12)
    // d.DayOfWeek is appropriate weekday (Sunday=0 to Saturday=6)
    // d.Day is week within the month (1 to 5, where 5 is last week of the month)
    // d.Hour, d.Minute and d.Second are absolute time
    nint day = 1;
    var t = Date(year, (ΔMonth)(nint)d.Month, day, (nint)d.Hour, (nint)d.Minute, (nint)d.Second, 0, ΔUTC);
    nint i = (nint)d.DayOfWeek - (nint)t.Weekday();
    if (i < 0) {
        i += 7;
    }
    day += i;
    {
        nint week = (nint)d.Day - 1; if (week < 4) {
            day += week * 7;
        } else {
            // "Last" instance of the day.
            day += 4 * 7;
            if (day > daysIn((ΔMonth)(nint)d.Month, year)) {
                day -= 7;
            }
        }
    }
    return t.sec() + (int64)(day - 1) * (int64)secondsPerDay + internalToUnix;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string localˢ = "Local"u8;

internal static void initLocalFromTZI(ж<syscall.Timezoneinformation> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    var l = ᏑlocalLoc;

    l.name = localˢ;

    nint nzone = 1;
    if (i.StandardDate.Month > 0) {
        nzone++;
    }
    l.zone = new slice<zone>(nzone);

    var (stdname, dstname) = abbrev(ref (Ꮡi).DerefOrNull());

    var std = Ꮡ(l.zone, 0);
    std.name = stdname;
    if (nzone == 1) {
        // No daylight savings.
        std.offset = -(nint)i.Bias * 60;
        l.cacheStart = alpha;
        l.cacheEnd = omega;
        l.cacheZone = std;
        l.tx = new slice<zoneTrans>(1);
        l.tx[0].when = l.cacheStart;
        l.tx[0].index = 0;
        return;
    }

    // StandardBias must be ignored if StandardDate is not set,
    // so this computation is delayed until after the nzone==1
    // return above.
    std.offset = -(nint)(i.Bias + i.StandardBias) * 60;

    var dst = Ꮡ(l.zone, 1);
    dst.name = dstname;
    dst.offset = -(nint)(i.Bias + i.DaylightBias) * 60;
    dst.isDST = true;

    // Arrange so that d0 is first transition date, d1 second,
    // i0 is index of zone after first transition, i1 second.
    var d0 = Ꮡi.of(syscall.Timezoneinformation.ᏑStandardDate);
    var d1 = Ꮡi.of(syscall.Timezoneinformation.ᏑDaylightDate);
    nint i0 = 0;
    nint i1 = 1;
    if (d0.Month > d1.Month) {
        (d0, d1) = (d1, d0);
        (i0, i1) = (i1, i0);
    }

    // 2 tx per year, 100 years on each side of this year
    l.tx = new slice<zoneTrans>(400);

    var t = Now().UTC();
    nint year = t.Year();
    nint txi = 0;
    for (nint y = year - 100; y < year + 100; y++) {
        var tx = Ꮡ(l.tx, txi);
        tx.when = pseudoUnix(y, ref (d0).DerefOrNull()) - (int64)l.zone[i1].offset;
        tx.index = (uint8)i0;
        txi++;

        tx = Ꮡ(l.tx, txi);
        tx.when = pseudoUnix(y, ref (d1).DerefOrNull()) - (int64)l.zone[i0].offset;
        tx.index = (uint8)i1;
        txi++;
    }
}

[GoLazy] partial class lazyᴛusPacific { internal static readonly ж<syscall.Timezoneinformation> ᏑusPacific = new StaticBox<syscall.Timezoneinformation>(new syscall.Timezoneinformation(
    Bias: 8 * 60,
    StandardName: new uint16[]{
        'P', 'a', 'c', 'i', 'f', 'i', 'c', ' ', 'S', 't', 'a', 'n', 'd', 'a', 'r', 'd', ' ', 'T', 'i', 'm', 'e'
    }.array(32),
    StandardDate: new syscall.Systemtime(Month: 11, Day: 1, Hour: 2),
    DaylightName: new uint16[]{
        'P', 'a', 'c', 'i', 'f', 'i', 'c', ' ', 'D', 'a', 'y', 'l', 'i', 'g', 'h', 't', ' ', 'T', 'i', 'm', 'e'
    }.array(32),
    DaylightDate: new syscall.Systemtime(Month: 3, Day: 2, Hour: 2),
    DaylightBias: -60
)); }
internal static ref syscall.Timezoneinformation usPacific => ref ((StaticBox<syscall.Timezoneinformation>)ᏑusPacific).ValueSlot;

[GoLazy] partial class lazyᴛaus { internal static readonly ж<syscall.Timezoneinformation> Ꮡaus = new StaticBox<syscall.Timezoneinformation>(new syscall.Timezoneinformation(
    Bias: -10 * 60,
    StandardName: new uint16[]{
        'A', 'U', 'S', ' ', 'E', 'a', 's', 't', 'e', 'r', 'n', ' ', 'S', 't', 'a', 'n', 'd', 'a', 'r', 'd', ' ', 'T', 'i', 'm', 'e'
    }.array(32),
    StandardDate: new syscall.Systemtime(Month: 4, Day: 1, Hour: 3),
    DaylightName: new uint16[]{
        'A', 'U', 'S', ' ', 'E', 'a', 's', 't', 'e', 'r', 'n', ' ', 'D', 'a', 'y', 'l', 'i', 'g', 'h', 't', ' ', 'T', 'i', 'm', 'e'
    }.array(32),
    DaylightDate: new syscall.Systemtime(Month: 10, Day: 1, Hour: 2),
    DaylightBias: -60
)); }
internal static ref syscall.Timezoneinformation aus => ref ((StaticBox<syscall.Timezoneinformation>)Ꮡaus).ValueSlot;

internal static void initLocal() {
    ref var i = ref heap(new syscall.Timezoneinformation(), out var Ꮡi);
    {
        var (_, err) = syscall.GetTimeZoneInformation(Ꮡi); if (err != nil) {
            localLoc.name = utcˢ;
            return;
        }
    }
    initLocalFromTZI(Ꮡi);
}

} // end time_package
