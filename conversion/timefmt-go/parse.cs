namespace go.github.com.itchyny.timefmt_go;

using errors = errors_package;
using fmt = fmt_package;
using time = time_package;

partial class timefmt_package {

public static (time.Time t, error err) Parse(@string source, @string format) {
    return parse(source, format, time.ΔUTC, time.ΔLocal);
}

public static (time.Time t, error err) ParseInLocation(@string source, @string format, ж<timeꓸLocation> loc) {
    return parse(source, format, loc, loc);
}

internal static (time.Time t, error err) parse(@string source, @string format, ж<timeꓸLocation> Ꮡloc, ж<timeꓸLocation> @base) {
    time.Time t = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var loc = ref Ꮡloc.DerefOrNull();

        nint year = 1900;
        nint month = 1;
        nint day = 0;
        nint hour = 0;
        nint minute = 0;
        nint second = 0;
        nint nanosecond = 0;
        defer(() => {
            if (err != nil) {
                err = fmt.Errorf("failed to parse %q with %q: %w"u8, source, format, err);
            }
        }, ref ᒐ);
        nint j = default;
        nint week = default;
        nint weekday = default;
        nint yday = default;
        nint colons = default;
        nint sign = default;
        nint century = -1;
        timeꓸWeekday weekstart = (timeꓸWeekday)(-1);
        bool pm = default;
        bool hasISOYear = default;
        bool hasZoneName = default;
        bool hasZoneOffset = default;
        @string pending = default;
        for ((nint i, nint l) = (0, len(source)); i < len(format); i++) {
            var b = format[i]; if (b == '%') {
                i++; if (i == len(format)) {
                    err = errors.New(@"stray ""%""");
                    goto ᒐdone;
                }
                b = format[i];
L:
                var exprᴛ1 = b;
                var matchᴛ1 = false;
                if (exprᴛ1 is (byte)'G') { matchᴛ1 = true;
                    hasISOYear = true;
                    fallthrough = true;
                }
                if (fallthrough || !matchᴛ1 && exprᴛ1 is (byte)'Y') { matchᴛ1 = true;
                    (sign, j) = parseSign(source, j, l);
                    (year, j, err) = parseInt(source, j, 4, 0, 9999, b); if (err != nil) {
                        goto ᒐdone;
                    }
                    year *= sign;
                }
                else if (exprᴛ1 is (byte)'g') { matchᴛ1 = true;
                    hasISOYear = true;
                    fallthrough = true;
                }
                if (fallthrough || !matchᴛ1 && exprᴛ1 is (byte)'y') { matchᴛ1 = true;
                    (year, j, err) = parseInt(source, j, 2, 0, 99, b); if (err != nil) {
                        goto ᒐdone;
                    }
                    if (year < 69) {
                        year += 2000;
                    } else {
                        year += 1900;
                    }
                }
                else if (exprᴛ1 is (byte)'C') { matchᴛ1 = true;
                    (sign, j) = parseSign(source, j, l);
                    if (sign < 0) {
                        err = errors.New(@"negative century is not supported for ""%C""");
                        goto ᒐdone;
                    }
                    (century, j, err) = parseInt(source, j, 2, 0, 99, (byte)'C'); if (err != nil) {
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'m') { matchᴛ1 = true;
                    (month, j, err) = parseInt(source, j, 2, 1, 12, (byte)'m'); if (err != nil) {
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'B') { matchᴛ1 = true;
                    (month, j, err) = parseAny(source, j, longMonthNames, (byte)'B'); if (err != nil) {
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'b' or (byte)'h') { matchᴛ1 = true;
                    (month, j, err) = parseAny(source, j, shortMonthNames, b); if (err != nil) {
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'A') { matchᴛ1 = true;
                    (weekday, j, err) = parseAny(source, j, longWeekNames, (byte)'A'); if (err != nil) {
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'a') { matchᴛ1 = true;
                    (weekday, j, err) = parseAny(source, j, shortWeekNames, (byte)'a'); if (err != nil) {
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'w') { matchᴛ1 = true;
                    (weekday, j, err) = parseInt(source, j, 1, 0, 6, (byte)'w'); if (err != nil) {
                        goto ᒐdone;
                    }
                    weekday++;
                }
                else if (exprᴛ1 is (byte)'u') { matchᴛ1 = true;
                    (weekday, j, err) = parseInt(source, j, 1, 1, 7, (byte)'u'); if (err != nil) {
                        goto ᒐdone;
                    }
                    weekday = weekday % 7 + 1;
                }
                else if (exprᴛ1 is (byte)'V') { matchᴛ1 = true;
                    (week, j, err) = parseInt(source, j, 2, 1, 53, b); if (err != nil) {
                        goto ᒐdone;
                    }
                    weekstart = time.Thursday;
                    weekday = or(weekday, 2);
                }
                else if (exprᴛ1 is (byte)'U') { matchᴛ1 = true;
                    (week, j, err) = parseInt(source, j, 2, 0, 53, b); if (err != nil) {
                        goto ᒐdone;
                    }
                    weekstart = time.Sunday;
                    weekday = or(weekday, 1);
                }
                else if (exprᴛ1 is (byte)'W') { matchᴛ1 = true;
                    (week, j, err) = parseInt(source, j, 2, 0, 53, b); if (err != nil) {
                        goto ᒐdone;
                    }
                    weekstart = time.Monday;
                    weekday = or(weekday, 2);
                }
                else if (exprᴛ1 is (byte)'e') { matchᴛ1 = true;
                    if (j < l && source[j] == ' ') {
                        j++;
                    }
                    fallthrough = true;
                }
                if (fallthrough || !matchᴛ1 && exprᴛ1 is (byte)'d') { matchᴛ1 = true;
                    (day, j, err) = parseInt(source, j, 2, 1, 31, b); if (err != nil) {
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'j') { matchᴛ1 = true;
                    (yday, j, err) = parseInt(source, j, 3, 1, 366, (byte)'j'); if (err != nil) {
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'k') { matchᴛ1 = true;
                    if (j < l && source[j] == ' ') {
                        j++;
                    }
                    fallthrough = true;
                }
                if (fallthrough || !matchᴛ1 && exprᴛ1 is (byte)'H') { matchᴛ1 = true;
                    (hour, j, err) = parseInt(source, j, 2, 0, 23, b); if (err != nil) {
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'l') { matchᴛ1 = true;
                    if (j < l && source[j] == ' ') {
                        j++;
                    }
                    fallthrough = true;
                }
                if (fallthrough || !matchᴛ1 && exprᴛ1 is (byte)'I') { matchᴛ1 = true;
                    (hour, j, err) = parseInt(source, j, 2, 1, 12, b); if (err != nil) {
                        goto ᒐdone;
                    }
                    if (hour == 12) {
                        hour = 0;
                    }
                }
                else if (exprᴛ1 is (byte)'P' or (byte)'p') { matchᴛ1 = true;
                    nint ampmΔ1 = default;
                    (ampmΔ1, j, err) = parseAny(source, j, ["AM", "PM"], b); if (err != nil) {
                        goto ᒐdone;
                    }
                    pm = ampmΔ1 == 2;
                }
                else if (exprᴛ1 is (byte)'M') { matchᴛ1 = true;
                    (minute, j, err) = parseInt(source, j, 2, 0, 59, (byte)'M'); if (err != nil) {
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'S') { matchᴛ1 = true;
                    (second, j, err) = parseInt(source, j, 2, 0, 60, (byte)'S'); if (err != nil) {
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'s') { matchᴛ1 = true;
                    (sign, j) = parseSign(source, j, l);
                    int64 unixΔ1 = default;
                    (unixΔ1, j, err) = parseInt64(source, j, 19, (byte)'s'); if (err != nil) {
                        goto ᒐdone;
                    }
                    t = time.Unix((int64)sign * unixΔ1, 0).In(time.ΔUTC);
                    timeꓸMonth mon = default;
                    (year, mon, day) = t.Date();
                    (hour, minute, second) = t.Clock();
                    month = (nint)mon;
                }
                else if (exprᴛ1 is (byte)'f') { matchᴛ1 = true;
                    nint microsecondΔ1 = 0;
                    nint iΔ4 = j;
                    (microsecondΔ1, j, err) = parseInt(source, j, 6, 0, 999999, (byte)'f'); if (err != nil) {
                        goto ᒐdone;
                    }
                    for (iΔ4 = j - iΔ4; iΔ4 < 6; iΔ4++) {
                        microsecondΔ1 *= 10;
                    }
                    nanosecond = microsecondΔ1 * 1000;
                }
                else if (exprᴛ1 is (byte)'Z') { matchᴛ1 = true;
                    nint iΔ5 = j;
                    for (; j < l; j++) {
                        var c = source[j]; if (c < 'A' || 'Z' < c) {
                            break;
                        }
                    }
                    (t, err) = time.ParseInLocation("MST", source.slice(iΔ5, j), @base);
                    if (err != nil) {
                        err = fmt.Errorf(@"cannot parse %q with ""%%Z"""u8, source.slice(iΔ5, j));
                        goto ᒐdone;
                    }
                    if (hasZoneOffset) {
                        var (name, _) = t.Zone();
                        var (_, offset) = locationZone(Ꮡloc);
                        Ꮡloc = time.FixedZone(name, offset); loc = ref Ꮡloc.DerefOrNull();
                    } else {
                        Ꮡloc = t.Location(); loc = ref Ꮡloc.DerefOrNull();
                    }
                    hasZoneName = true;
                }
                else if (exprᴛ1 is (byte)'z') { matchᴛ1 = true;
                    if (j >= l) {
                        err = ((parseZFormatError)colons);
                        goto ᒐdone;
                    }
                    sign = 1;
                    var exprᴛ2 = source[j];
                    var matchᴛ2 = false;
                    if (exprᴛ2 is (byte)'-') { matchᴛ2 = true;
                        sign = -1;
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ2 && exprᴛ2 is (byte)'+') { matchᴛ2 = true;
                        nint hourΔ4 = 0;
                        nint minuteΔ4 = 0;
                        nint secondΔ4 = 0;
                        nint iΔ7 = j + 1;
                        (hourΔ4, j, _) = parseInt(source, iΔ7, 2, 0, 23, (byte)'z'); if (j != iΔ7 + 2) {
                            err = ((parseZFormatError)colons);
                            goto ᒐdone;
                        }
                        if (j >= l || source[j] != ':') {
                            if (colons > 0 && colons < 3) {
                                err = ((expectedColonForZFormatError)colons);
                                goto ᒐdone;
                            }
                        } else {
                            j++; if (colons == 0) {
                                colons = 4;
                            }
                        }
                        iΔ7 = j;
                        (minuteΔ4, j, _) = parseInt(source, iΔ7, 2, 0, 59, (byte)'z'); if (j != iΔ7 + 2) {
                            if (colons > 0 && colons != 3) {
                                err = ((parseZFormatError)(colons & 3));
                                goto ᒐdone;
                            }
                            j = iΔ7;
                        } else if (colons > 1) {
                            if (j >= l || source[j] != ':') {
                                if (colons < 3) {
                                    err = ((expectedColonForZFormatError)colons);
                                    goto ᒐdone;
                                }
                            } else {
                                iΔ7 = j + 1;
                                (secondΔ4, j, _) = parseInt(source, iΔ7, 2, 0, 59, (byte)'z'); if (j != iΔ7 + 2) {
                                    if (colons < 3) {
                                        err = ((parseZFormatError)colons);
                                        goto ᒐdone;
                                    }
                                    j = iΔ7 - 1;
                                }
                            }
                        }
                        @string name = default;
                        if (hasZoneName) {
                            (name, _) = locationZone(Ꮡloc);
                        }
                        (Ꮡloc, colons) = (time.FixedZone(name, sign * ((hourΔ4 * 60 + minuteΔ4) * 60 + secondΔ4)), 0); loc = ref Ꮡloc.DerefOrNull();
                        hasZoneOffset = true;
                    }
                    else if (exprᴛ2 is (byte)'Z') {
                        (Ꮡloc, colons, j) = (time.ΔUTC, 0, j + 1); loc = ref Ꮡloc.DerefOrNull();
                    }
                    else if (!matchᴛ2) { /* default: */
                        err = ((parseZFormatError)colons);
                        goto ᒐdone;
                    }

                }
                else if (exprᴛ1 is (byte)':') { matchᴛ1 = true;
                    if (pending != ""u8) {
                        if (j >= l || source[j] != b) {
                            err = ((expectedFormatError)b);
                            goto ᒐdone;
                        }
                        j++;
                    } else {
                        for (colons = 1; colons <= 3; colons++) {
                            i++; if (i == len(format)) {
                                break;
                            } else {
                                b = format[i]; if (b == 'z') {
                                    goto L;
                                } else if (b != ':' || colons == 3) {
                                    break;
                                }
                            }
                        }
                        err = ((expectedZAfterColonError)colons);
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'t' or (byte)'n') { matchᴛ1 = true;
                    nint iΔ8 = j;
K:
                    for (; j < l; j++) {
                        switch (source[j]) {
                        case (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\v' or (byte)'\f' or (byte)'\r': {
                            break;
                        }
                        default: {
                            goto break_K;
                            break;
                        }}

continue_K:;
                    }
break_K:;
                    if (iΔ8 == j) {
                        err = fmt.Errorf(@"expected a space for ""%%%c"""u8, b);
                        goto ᒐdone;
                    }
                }
                else if (exprᴛ1 is (byte)'%') { matchᴛ1 = true;
                    if (j >= l || source[j] != b) {
                        err = ((expectedFormatError)b);
                        goto ᒐdone;
                    }
                    j++;
                }
                else if (exprᴛ1 is (byte)'c') { matchᴛ1 = true;
                    pending = "a b e H:M:S Y";
                }
                else if (exprᴛ1 is (byte)'+') { matchᴛ1 = true;
                    pending = "a b e H:M:S Z Y";
                }
                else if (exprᴛ1 is (byte)'v') { matchᴛ1 = true;
                    pending = "e-b-Y";
                }
                else if (exprᴛ1 is (byte)'r') { matchᴛ1 = true;
                    pending = "I:M:S p";
                }
                else if (exprᴛ1 is (byte)'F') { matchᴛ1 = true;
                    pending = "Y-m-d";
                }
                else if (exprᴛ1 is (byte)'D' or (byte)'x') { matchᴛ1 = true;
                    pending = "m/d/y";
                }
                else if (exprᴛ1 is (byte)'T' or (byte)'X') { matchᴛ1 = true;
                    pending = "H:M:S";
                }
                else if (exprᴛ1 is (byte)'R') {
                    pending = "H:M";
                }
                else if (!matchᴛ1) { /* default: */
                    if (pending == ""u8) {
                        err = fmt.Errorf(@"unexpected format ""%%%c"""u8, b);
                        goto ᒐdone;
                    }
                    if (j >= l || source[j] != b) {
                        err = ((expectedFormatError)b);
                        goto ᒐdone;
                    }
                    j++;
                }

                break_L:;
                if (pending != ""u8) {
                    (b, pending) = (pending[0], pending[1..]);
                    goto L;
                }
            } else if (j >= l || source[j] != b) {
                err = ((expectedFormatError)b);
                goto ᒐdone;
            } else {
                j++;
            }
        }
        if (j < len(source)) {
            err = fmt.Errorf("unparsed string %q"u8, source.slice(j));
            goto ᒐdone;
        }
        if (pm) {
            hour += 12;
        }
        if (century >= 0) {
            year = century * 100 + year % 100;
        }
        if (day == 0) {
            if (yday > 0) {
                if (hasISOYear) {
                    err = errors.New(@"use ""%Y"" to parse non-ISO year for ""%j""");
                    goto ᒐdone;
                }
                (t, err) = (time.Date(year, time.January, yday, hour, minute, second, nanosecond, Ꮡloc), default); goto ᒐdone;
            }
            if (weekstart >= time.Sunday) {
                if (weekstart == time.Thursday) {
                    if (!hasISOYear) {
                        err = errors.New(@"use ""%G"" to parse ISO year for ""%V""");
                        goto ᒐdone;
                    }
                } else if (hasISOYear) {
                    err = errors.New(@"use ""%Y"" to parse non-ISO year for ""%U"" or ""%W""");
                    goto ᒐdone;
                }
                if (weekstart > time.Sunday && weekday == 1) {
                    week++;
                }
                var tΔ1 = time.Date(year, time.January, -(nint)weekstart, hour, minute, second, nanosecond, Ꮡloc);
                (t, err) = (tΔ1.AddDate(0, 0, week * 7 - (nint)tΔ1.Weekday() + weekday - 1), default); goto ᒐdone;
            }
            day = 1;
        }
        (t, err) = (time.Date(year, (timeꓸMonth)month, day, hour, minute, second, nanosecond, Ꮡloc), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (t, err);
}

internal static (@string name, nint offset) locationZone(ж<timeꓸLocation> loc) {
    return time.Date(2000, time.January, 1, 0, 0, 0, 0, loc).Zone();
}

partial struct parseFormatError /*num:byte*/;

internal static @string Error(this parseFormatError err) {
    return fmt.Sprintf(@"cannot parse ""%%%c"""u8, (byte)err);
}

partial struct expectedFormatError /*num:byte*/;

internal static @string Error(this expectedFormatError err) {
    return fmt.Sprintf("expected %q"u8, (byte)err);
}

partial struct parseZFormatError /*num:nint*/;

internal static @string Error(this parseZFormatError err) {
    return @"cannot parse ""%" + ((@string)@":::z"""u8.slice(3 - err));
}

partial struct expectedColonForZFormatError /*num:nint*/;

internal static @string Error(this expectedColonForZFormatError err) {
    return @"expected ':' for ""%" + ((@string)@":::z"""u8.slice(3 - err));
}

partial struct expectedZAfterColonError /*num:nint*/;

internal static @string Error(this expectedZAfterColonError err) {
    return @"expected 'z' after ""%" + ((@string)@":::"""u8.slice(3 - err));
}

internal static (nint, nint) parseSign(@string source, nint index, nint l) {
    if (index < l && source[index] == '-') {
        return (-1, index + 1);
    }
    return (1, index);
}

internal static (nint, nint, error) parseInt(@string source, nint index, nint size, nint minimum, nint maximum, byte format) {
    nint value = default;
    nint i = index;
    for (size = min(i + size, len(source)); i < size; i++) {
        var b = (byte)(source[i] - '0'); if (b < 10) {
            value = value * 10 + (nint)b;
        } else {
            break;
        }
    }
    if (i == index || value < minimum || maximum < value) {
        return (0, 0, ((parseFormatError)format));
    }
    return (value, i, default);
}

internal static (int64, nint, error) parseInt64(@string source, nint index, nint size, byte format) {
    int64 value = default;
    nint i = index;
    for (size = min(i + size, len(source)); i < size; i++) {
        var b = (byte)(source[i] - '0'); if (b < 10) {
            value = value * 10 + (int64)b;
        } else {
            break;
        }
    }
    if (i == index) {
        return (0, 0, ((parseFormatError)format));
    }
    return (value, i, default);
}

internal static (nint, nint, error) parseAny(@string source, nint index, slice<@string> candidates, byte format) {
L:
    foreach (var (i, xs) in range(candidates)) {
        nint j = index;
        for (nint k = 0; k < len(xs); (k, j) = (k + 1, j + 1)) {
            if (j >= len(source)) {
                goto continue_L;
            }
            var (x, y) = (xs[k], source[j]); if (x != y && (byte)(x | 0x20) != (byte)(y | 0x20)) {
                goto continue_L;
            }
        }
        return (i + 1, j, default);
continue_L:;
    }
break_L:;
    return (0, 0, ((parseFormatError)format));
}

} // end timefmt_package
