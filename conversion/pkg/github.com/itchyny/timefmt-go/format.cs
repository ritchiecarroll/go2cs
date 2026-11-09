namespace go.github.com.itchyny.timefmt_go;

using strconv = strconv_package;
using time = time_package;

partial class timefmt_package {

public static @string Format(time.Time t, @string format) {
    return (@string)AppendFormat(new slice<byte>(0, 64), t, format);
}

public static slice<byte> AppendFormat(slice<byte> buf, time.Time t, @string format) {
    var (year, month, day) = t.Date();
    var (hour, minute, second) = t.Clock();
    nint width = default;
    nint colons = default;
    byte padding = default;
    @string pending = default;
    bool upper = default;
    bool swap = default;
    for (nint i = 0; i < len(format); i++) {
        var b = format[i]; if (b == '%') {
            i++; if (i == len(format)) {
                buf = append(buf, (byte)'%');
                break;
            }
            (b, width, padding, upper, swap) = (format[i], 0, (byte)'0', false, false);
L:
            var exprᴛ1 = b;
            var matchᴛ1 = false;
            if (exprᴛ1 is (byte)'-') { matchᴛ1 = true;
                do {
                    if (pending != ""u8) {
                        buf = append(buf, (byte)'-');
                        break;
                    }
                    i++; if (i == len(format)) {
                        goto K;
                    }
                    padding = unchecked((byte)(~paddingMask));
                    b = format[i];
                    goto L;
                } while (false);
            }
            else if (exprᴛ1 is (byte)'_') { matchᴛ1 = true;
                i++; if (i == len(format)) {
                    goto K;
                }
                padding = (byte)((rune)' ' | unchecked((byte)(~paddingMask)));
                b = format[i];
                goto L;
            }
            else if (exprᴛ1 is (byte)'^') { matchᴛ1 = true;
                i++; if (i == len(format)) {
                    goto K;
                }
                upper = true;
                b = format[i];
                goto L;
            }
            else if (exprᴛ1 is (byte)'#') { matchᴛ1 = true;
                i++; if (i == len(format)) {
                    goto K;
                }
                swap = true;
                b = format[i];
                goto L;
            }
            else if (exprᴛ1 is (byte)'0') { matchᴛ1 = true;
                i++; if (i == len(format)) {
                    goto K;
                }
                padding = (byte)((rune)'0' | unchecked((byte)(~paddingMask)));
                b = format[i];
                goto L;
            }
            else if (exprᴛ1 is (byte)'1' or (byte)'2' or (byte)'3' or (byte)'4' or (byte)'5' or (byte)'6' or (byte)'7' or (byte)'8' or (byte)'9') { matchᴛ1 = true;
                width = (nint)((byte)(b & 0x0F));
                for (i++; i < len(format); i++) {
                    b = format[i]; if (b <= '9' && '0' <= b) {
                        width = min(width * 10 + (nint)((byte)(b & 0x0F)), 1024);
                    } else {
                        break;
                    }
                }
                if (padding == unchecked((byte)(~paddingMask))) {
                    padding = (byte)((rune)' ' | unchecked((byte)(~paddingMask)));
                }
                if (i == len(format)) {
                    goto K;
                }
                goto L;
            }
            else if (exprᴛ1 is (byte)'Y') { matchᴛ1 = true;
                buf = appendInt(buf, year, or(width, 4), padding);
            }
            else if (exprᴛ1 is (byte)'y') { matchᴛ1 = true;
                buf = appendInt(buf, abs(year % 100), max(width, 2), padding);
            }
            else if (exprᴛ1 is (byte)'C') { matchᴛ1 = true;
                nint c = year / 100;
                var z = year < 0 && c == 0;
                if (z) {
                    c = -1;
                }
                buf = appendInt(buf, c, max(width, 2), padding);
                if (z) {
                    buf[len(buf) - 1] = (byte)'0';
                }
            }
            else if (exprᴛ1 is (byte)'g') { matchᴛ1 = true;
                var (yearΔ2, _) = t.ISOWeek();
                buf = appendInt(buf, abs(yearΔ2 % 100), max(width, 2), padding);
            }
            else if (exprᴛ1 is (byte)'G') { matchᴛ1 = true;
                var (yearΔ3, _) = t.ISOWeek();
                buf = appendInt(buf, yearΔ3, or(width, 4), padding);
            }
            else if (exprᴛ1 is (byte)'m') { matchᴛ1 = true;
                buf = appendInt(buf, (nint)month, max(width, 2), padding);
            }
            else if (exprᴛ1 is (byte)'B') { matchᴛ1 = true;
                buf = appendString(buf, longMonthNames[month - 1], width, padding, upper, swap);
            }
            else if (exprᴛ1 is (byte)'b' or (byte)'h') { matchᴛ1 = true;
                buf = appendString(buf, shortMonthNames[month - 1], width, padding, upper, swap);
            }
            else if (exprᴛ1 is (byte)'A') { matchᴛ1 = true;
                buf = appendString(buf, longWeekNames[t.Weekday()], width, padding, upper, swap);
            }
            else if (exprᴛ1 is (byte)'a') { matchᴛ1 = true;
                buf = appendString(buf, shortWeekNames[t.Weekday()], width, padding, upper, swap);
            }
            else if (exprᴛ1 is (byte)'w') { matchᴛ1 = true;
                buf = appendInt(buf, (nint)t.Weekday(), width, padding);
            }
            else if (exprᴛ1 is (byte)'u') { matchᴛ1 = true;
                buf = appendInt(buf, or((nint)t.Weekday(), 7), width, padding);
            }
            else if (exprᴛ1 is (byte)'V') { matchᴛ1 = true;
                var (_, week) = t.ISOWeek();
                buf = appendInt(buf, week, max(width, 2), padding);
            }
            else if (exprᴛ1 is (byte)'U') { matchᴛ1 = true;
                nint week = (t.YearDay() + 6 - (nint)t.Weekday()) / 7;
                buf = appendInt(buf, week, max(width, 2), padding);
            }
            else if (exprᴛ1 is (byte)'W') { matchᴛ1 = true;
                nint week = t.YearDay();
                if ((nint)t.Weekday() > 0) {
                    week -= (nint)t.Weekday() - 7;
                }
                week /= 7;
                buf = appendInt(buf, week, max(width, 2), padding);
            }
            else if (exprᴛ1 is (byte)'e') { matchᴛ1 = true;
                if (padding < unchecked((byte)(~paddingMask))) {
                    padding = (byte)' ';
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 is (byte)'d') { matchᴛ1 = true;
                buf = appendInt(buf, day, max(width, 2), padding);
            }
            else if (exprᴛ1 is (byte)'j') { matchᴛ1 = true;
                buf = appendInt(buf, t.YearDay(), max(width, 3), padding);
            }
            else if (exprᴛ1 is (byte)'k') { matchᴛ1 = true;
                if (padding < unchecked((byte)(~paddingMask))) {
                    padding = (byte)' ';
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 is (byte)'H') { matchᴛ1 = true;
                buf = appendInt(buf, hour, max(width, 2), padding);
            }
            else if (exprᴛ1 is (byte)'l') { matchᴛ1 = true;
                if (padding < unchecked((byte)(~paddingMask))) {
                    padding = (byte)' ';
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 is (byte)'I') { matchᴛ1 = true;
                buf = appendInt(buf, or(hour % 12, 12), max(width, 2), padding);
            }
            else if (exprᴛ1 is (byte)'P') { matchᴛ1 = true;
                swap = !(upper || swap);
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 is (byte)'p') { matchᴛ1 = true;
                if (hour < 12) {
                    buf = appendString(buf, "AM", width, padding, upper, swap);
                } else {
                    buf = appendString(buf, "PM", width, padding, upper, swap);
                }
            }
            else if (exprᴛ1 is (byte)'M') { matchᴛ1 = true;
                buf = appendInt(buf, minute, max(width, 2), padding);
            }
            else if (exprᴛ1 is (byte)'S') { matchᴛ1 = true;
                buf = appendInt(buf, second, max(width, 2), padding);
            }
            else if (exprᴛ1 is (byte)'s') { matchᴛ1 = true;
                if (padding < unchecked((byte)(~paddingMask))) {
                    padding = (byte)' ';
                }
                buf = appendInt64(buf, t.Unix(), width, padding);
            }
            else if (exprᴛ1 is (byte)'f') { matchᴛ1 = true;
                buf = appendInt(buf, t.Nanosecond() / 1000, or(width, 6), padding);
            }
            else if (exprᴛ1 is (byte)'Z' or (byte)'z') { matchᴛ1 = true;
                do {
                    var (name, offsetΔ1) = t.Zone();
                    if (b == 'Z' && name != ""u8) {
                        buf = appendString(buf, name, width, padding, upper, swap);
                        break;
                    }
                    nint iΔ2 = len(buf);
                    if (padding != unchecked((byte)(~paddingMask))) {
                        for (; width > 1; width--) {
                            buf = append(buf, (byte)(padding & paddingMask));
                        }
                    }
                    nint j = len(buf);
                    if (offsetΔ1 < 0) {
                        buf = append(buf, (byte)'-');
                        offsetΔ1 = -offsetΔ1;
                    } else {
                        buf = append(buf, (byte)'+');
                    }
                    nint kΔ1 = len(buf);
                    buf = appendInt(buf, offsetΔ1 / 3600, 2, padding);
                    if (buf[kΔ1] == ' ') {
                        (buf[kΔ1 - 1], buf[kΔ1]) = (buf[kΔ1], buf[kΔ1 - 1]);
                    }
                    offsetΔ1 %= 3600; if (colons <= 2 || offsetΔ1 != 0) {
                        if (colons != 0) {
                            buf = append(buf, (byte)':');
                        }
                        buf = appendInt(buf, offsetΔ1 / 60, 2, (byte)'0');
                        offsetΔ1 %= 60; if (colons == 2 || colons == 3 && offsetΔ1 != 0) {
                            buf = append(buf, (byte)':');
                            buf = appendInt(buf, offsetΔ1, 2, (byte)'0');
                        }
                    }
                    colons = 0;
                    kΔ1 = min(len(buf) - j - 1, j - iΔ2); if (kΔ1 > 0) {
                        copy(buf.slice(j - kΔ1), buf.slice(j));
                        buf = buf.slice(0, len(buf) - kΔ1);
                        if ((byte)(padding & paddingMask) == '0') {
                            (buf[iΔ2], buf[j - kΔ1]) = (buf[j - kΔ1], buf[iΔ2]);
                        }
                    }
                } while (false);
            }
            else if (exprᴛ1 is (byte)':') { matchᴛ1 = true;
                if (pending != ""u8) {
                    buf = append(buf, (byte)':');
                } else {
                    colons = 1;
M:
                    for (i++; i < len(format); i++) {
                        switch (format[i]) {
                        case (byte)':': {
                            colons++;
                            break;
                        }
                        case (byte)'z': {
                            if (colons > 3) {
                                i++;
                                goto break_M;
                            }
                            b = (byte)'z';
                            goto L;
                            break;
                        }
                        default: {
                            goto break_M;
                            break;
                        }}

continue_M:;
                    }
break_M:;
                    buf = appendLast(buf, format.slice(0, i), width, padding);
                    i--;
                    colons = 0;
                }
            }
            else if (exprᴛ1 is (byte)'t') { matchᴛ1 = true;
                buf = appendString(buf, "\t", width, padding, false, false);
            }
            else if (exprᴛ1 is (byte)'n') { matchᴛ1 = true;
                buf = appendString(buf, "\n", width, padding, false, false);
            }
            else if (exprᴛ1 is (byte)'%') { matchᴛ1 = true;
                buf = appendString(buf, "%", width, padding, false, false);
            }
            else if (exprᴛ1 is (byte)'c') { matchᴛ1 = true;
                (pending, swap) = ("a b e H:M:S Y", false);
            }
            else if (exprᴛ1 is (byte)'+') { matchᴛ1 = true;
                (pending, swap) = ("a b e H:M:S Z Y", false);
            }
            else if (exprᴛ1 is (byte)'v') { matchᴛ1 = true;
                (pending, swap) = ("e-b-Y", false);
            }
            else if (exprᴛ1 is (byte)'r') { matchᴛ1 = true;
                (pending, swap) = ("I:M:S p", false);
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
                    buf = appendLast(buf, format.slice(0, i), width - 1, padding);
                }
                buf = append(buf, b);
            }

            break_L:;
            if (pending != ""u8) {
                (b, pending, width, padding) = (pending[0], pending[1..], 0, (byte)'0');
                goto L;
            }
        } else {
            buf = append(buf, b);
        }
    }
    return buf;
K:
    return appendLast(buf, format, width, padding);
}

internal static readonly @string smalls = "00010203040506070809101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899";

internal static slice<byte> appendInt(slice<byte> buf, nint num, nint width, byte padding) {
    nint digits = default;
    var matchᴛ1 = false;
    var matchᴛ2 = num is < 0 || num < 100 && width == 2 && padding == (byte)'0' || num is < 10 || num is < 100 || num is < 1000 || num is < 10000;
    if (!matchᴛ2) { /* default: */
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && num is < 0) { matchᴛ1 = true;
        return appendInt64(buf, (int64)num, width, padding);
    }
    if (num < 100 && width == 2 && padding == (byte)'0') { matchᴛ1 = true;
        return append(buf, smalls.slice(num * 2, num * 2 + 2).ꓸꓸꓸ);
    }
    if (num is < 10) { matchᴛ1 = true;
        digits = 1;
    }
    else if (num is < 100) { matchᴛ1 = true;
        digits = 2;
    }
    else if (num is < 1000) {
        digits = 3;
    }
    else if (num is < 10000) { matchᴛ1 = true;
        digits = 4;
    }

    if (padding != unchecked((byte)(~paddingMask))) {
        padding &= (byte)(paddingMask);
        for (; width > digits; width--) {
            buf = append(buf, padding);
        }
    }
    var exprᴛ2 = digits;
    var matchᴛ3 = false;
    if (exprᴛ2 is 4) { matchᴛ3 = true;
        buf = append(buf, (byte)((byte)(num / 1000) | '0'));
        num %= 1000;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ3 && exprᴛ2 is 3) { matchᴛ3 = true;
        buf = append(buf, (byte)((byte)(num / 100) | '0'));
        num %= 100;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ3 && exprᴛ2 is 2) { matchᴛ3 = true;
        buf = append(buf, (byte)((byte)(num / 10) | '0'));
        num %= 10;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ3) { /* default: */
        return append(buf, (byte)((byte)num | '0'));
    }
    return default;

}

internal static slice<byte> appendInt64(slice<byte> buf, int64 num, nint width, byte padding) {
    if (padding == unchecked((byte)(~paddingMask))) {
        return strconv.AppendInt(buf, num, 10);
    }
    padding &= (byte)(paddingMask);
    nint i = len(buf);
    for (; width > 1; width--) {
        buf = append(buf, padding);
    }
    nint j = len(buf);
    buf = strconv.AppendInt(buf, num, 10);
    nint k = min(len(buf) - j - 1, j - i); if (k > 0) {
        copy(buf.slice(j - k), buf.slice(j));
        buf = buf.slice(0, len(buf) - k);
        j -= k; if (buf[i] == '0' && buf[j] == '-') {
            (buf[i], buf[j]) = ((byte)'-', (byte)'0');
        }
    }
    return buf;
}

internal static slice<byte> appendString(slice<byte> buf, @string str, nint width, byte padding, bool upper, bool swap) {
    if (width > len(str) && padding != unchecked((byte)(~paddingMask))) {
        if (padding < unchecked((byte)(~paddingMask))) {
            padding = (byte)' ';
        } else {
            padding &= (byte)(paddingMask);
        }
        for (width -= len(str); width > 0; width--) {
            buf = append(buf, padding);
        }
    }
    var matchᴛ1 = false;
    if (swap) { matchᴛ1 = true;
        do {
            if (str[1] < 'a') {
                foreach (var (_, b) in range(slice<byte>(str))) {
                    buf = append(buf, (byte)(b | 0x20));
                }
                break;
            }
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && (upper)) {
        foreach (var (_, b) in range(slice<byte>(str))) {
            buf = append(buf, (byte)(b & 0x5F));
        }
    }
    else if (!matchᴛ1) { /* default: */
        buf = append(buf, str.ꓸꓸꓸ);
    }

    return buf;
}

internal static slice<byte> appendLast(slice<byte> buf, @string format, nint width, byte padding) {
    for (nint i = len(format) - 1; i >= 0; i--) {
        if (format[i] == '%') {
            buf = appendString(buf, format.slice(i), width, padding, false, false);
            break;
        }
    }
    return buf;
}

internal static nint or(nint x, nint y) {
    if (x != 0) {
        return x;
    }
    return y;
}

internal static nint abs(nint x) {
    if (x >= 0) {
        return x;
    }
    return -x;
}

internal const byte paddingMask = 0x7F;

internal static slice<@string> longMonthNames = [
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December"
];

internal static slice<@string> shortMonthNames = [
    "Jan",
    "Feb",
    "Mar",
    "Apr",
    "May",
    "Jun",
    "Jul",
    "Aug",
    "Sep",
    "Oct",
    "Nov",
    "Dec"
];

internal static slice<@string> longWeekNames = [
    "Sunday",
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday"
];

internal static slice<@string> shortWeekNames = [
    "Sun",
    "Mon",
    "Tue",
    "Wed",
    "Thu",
    "Fri",
    "Sat"
];

} // end timefmt_package
