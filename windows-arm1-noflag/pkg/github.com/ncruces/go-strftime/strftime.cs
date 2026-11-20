namespace go.github.com.ncruces.go_strftime;

using bytes = bytes_package;
using strconv = strconv_package;
using time = time_package;

partial class strftime_package {

public static @string Format(@string fmt, time.Time t) {
    var buf = buffer(fmt);
    return (@string)AppendFormat(buf, fmt, t);
}

public static slice<byte> AppendFormat(slice<byte> dst, @string fmt, time.Time t) {
    parser parser = default;

    parser.literal = error (byte b) => {
        dst = append(dst, b);
        return default;
    };

    var tʗ1 = t;
    parser.format = error (byte spec, byte flag) => {
        switch (spec) {
        case (byte)'A': {
            dst = append(dst, tʗ1.Weekday().String().ꓸꓸꓸ);
            return default;
        }
        case (byte)'a': {
            dst = append(dst, tʗ1.Weekday().String()[..3].ꓸꓸꓸ);
            return default;
        }
        case (byte)'B': {
            dst = append(dst, tʗ1.Month().String().ꓸꓸꓸ);
            return default;
        }
        case (byte)'b' or (byte)'h': {
            dst = append(dst, tʗ1.Month().String()[..3].ꓸꓸꓸ);
            return default;
        }
        case (byte)'m': {
            dst = appendInt2(dst, (nint)tʗ1.Month(), flag);
            return default;
        }
        case (byte)'d': {
            dst = appendInt2(dst, (nint)tʗ1.Day(), flag);
            return default;
        }
        case (byte)'e': {
            dst = appendInt2(dst, (nint)tʗ1.Day(), (byte)' ');
            return default;
        }
        case (byte)'I': {
            dst = append12Hour(dst, tʗ1, flag);
            return default;
        }
        case (byte)'l': {
            dst = append12Hour(dst, tʗ1, (byte)' ');
            return default;
        }
        case (byte)'H': {
            dst = appendInt2(dst, tʗ1.Hour(), flag);
            return default;
        }
        case (byte)'k': {
            dst = appendInt2(dst, tʗ1.Hour(), (byte)' ');
            return default;
        }
        case (byte)'M': {
            dst = appendInt2(dst, tʗ1.Minute(), flag);
            return default;
        }
        case (byte)'S': {
            dst = appendInt2(dst, tʗ1.Second(), flag);
            return default;
        }
        case (byte)'L': {
            dst = append(dst, tʗ1.Format(".000")[1..].ꓸꓸꓸ);
            return default;
        }
        case (byte)'f': {
            dst = append(dst, tʗ1.Format(".000000")[1..].ꓸꓸꓸ);
            return default;
        }
        case (byte)'N': {
            dst = append(dst, tʗ1.Format(".000000000")[1..].ꓸꓸꓸ);
            return default;
        }
        case (byte)'y': {
            dst = tʗ1.AppendFormat(dst, "06");
            return default;
        }
        case (byte)'Y': {
            dst = tʗ1.AppendFormat(dst, "2006");
            return default;
        }
        case (byte)'C': {
            nint centuryΔ2 = century(tʗ1);
            if (centuryΔ2 >= 0 && centuryΔ2 < 10) {
                dst = append(dst, (byte)'0');
            }
            dst = strconv.AppendInt(dst, (int64)centuryΔ2, 10);
            return default;
        }
        case (byte)'U': {
            dst = appendWeekNumber(dst, tʗ1, flag, true);
            return default;
        }
        case (byte)'W': {
            dst = appendWeekNumber(dst, tʗ1, flag, false);
            return default;
        }
        case (byte)'V': {
            var (_, w) = tʗ1.ISOWeek();
            dst = appendInt2(dst, w, flag);
            return default;
        }
        case (byte)'g': {
            var (y, _) = tʗ1.ISOWeek();
            dst = year(y).AppendFormat(dst, "06");
            return default;
        }
        case (byte)'G': {
            var (y, _) = tʗ1.ISOWeek();
            dst = year(y).AppendFormat(dst, "2006");
            return default;
        }
        case (byte)'s': {
            dst = strconv.AppendInt(dst, tʗ1.Unix(), 10);
            return default;
        }
        case (byte)'Q': {
            dst = strconv.AppendInt(dst, tʗ1.UnixMilli(), 10);
            return default;
        }
        case (byte)'w': {
            time.Weekday w = tʗ1.Weekday();
            dst = appendInt1(dst, (nint)w);
            return default;
        }
        case (byte)'u': {
            {
                time.Weekday w = tʗ1.Weekday(); if (w == 0) {
                    dst = append(dst, (byte)'7');
                } else {
                    dst = appendInt1(dst, (nint)w);
                }
            }
            return default;
        }
        case (byte)'j': {
            if (flag == '-') {
                dst = strconv.AppendInt(dst, (int64)tʗ1.YearDay(), 10);
            } else {
                dst = tʗ1.AppendFormat(dst, "002");
            }
            return default;
        }}

        @string layout = goLayout(spec, flag, false); if (layout != ""u8) {
            dst = tʗ1.AppendFormat(dst, layout);
            return default;
        }

        dst = append(dst, (byte)'%');
        if (flag != 0) {
            dst = append(dst, flag);
        }
        dst = append(dst, spec);
        return default;
    };

    parser.parse(fmt);
    return dst;
}

public static (time.Time, error) Parse(@string fmt, @string value) {
    var (pattern, err) = layout(fmt, true);
    if (err != nil) {
        return (new time.Time(), err);
    }
    return time.Parse(pattern, value);
}

public static (@string, error) Layout(@string fmt) {
    return layout(fmt, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string janˢ = "Jan"u8;
internal static readonly @string monˢ = "Mon"u8;
internal static readonly @string mstˢ = "MST"u8;

internal static (@string, error) layout(@string fmt, bool parsing) {
    ref var dst = ref heap<slice<byte>>(buffer(fmt), out var Ꮡdst);
    parser parser = default;

    parser.literal = error (byte b) => {
        if ('0' <= b && b <= '9') {
            return ((literalErr)(@string)b);
        }
        Ꮡdst.ValueSlot = append(Ꮡdst.ValueSlot, b);
        if (b == 'M' || b == 'T' || b == 'm' || b == 'n') {
            switch (ᐧ) {
            case {} when bytes.HasSuffix(Ꮡdst.ValueSlot, slice<byte>("Jan"u8)): {
                return ((literalErr)(@string)janˢ);
            }
            case {} when bytes.HasSuffix(Ꮡdst.ValueSlot, slice<byte>("Mon"u8)): {
                return ((literalErr)(@string)monˢ);
            }
            case {} when bytes.HasSuffix(Ꮡdst.ValueSlot, slice<byte>("MST"u8)): {
                return ((literalErr)(@string)mstˢ);
            }
            case {} when bytes.HasSuffix(Ꮡdst.ValueSlot, slice<byte>("PM"u8)): {
                return ((literalErr)(@string)"PM");
            }
            case {} when bytes.HasSuffix(Ꮡdst.ValueSlot, slice<byte>("pm"u8)): {
                return ((literalErr)(@string)"pm");
            }}

        }
        return default;
    };

    parser.format = error (byte spec, byte flag) => {
        @string layout = goLayout(spec, flag, parsing); if (layout != ""u8) {
            Ꮡdst.ValueSlot = append(Ꮡdst.ValueSlot, layout.ꓸꓸꓸ);
            return default;
        }

        switch (spec) {
        default: {
            return new formatError();
        }
        case (byte)'L' or (byte)'f' or (byte)'N': {
            if (bytes.HasSuffix(Ꮡdst.ValueSlot, slice<byte>("."u8)) || bytes.HasSuffix(Ꮡdst.ValueSlot, slice<byte>(","u8))) {
                switch (spec) {
                default: {
                    Ꮡdst.ValueSlot = append(Ꮡdst.ValueSlot, ((@string)"000").ꓸꓸꓸ);
                    break;
                }
                case (byte)'f': {
                    Ꮡdst.ValueSlot = append(Ꮡdst.ValueSlot, ((@string)"000000").ꓸꓸꓸ);
                    break;
                }
                case (byte)'N': {
                    Ꮡdst.ValueSlot = append(Ꮡdst.ValueSlot, ((@string)"000000000").ꓸꓸꓸ);
                    break;
                }}

                return default;
            }
            return new formatError(message: "must follow '.' or ','");
        }}

    };

    var err = parser.parse(fmt); if (err != nil) {
        return ("", err);
    }
    return ((@string)dst, default);
}

public static (@string, error) UTS35(@string fmt) {
    const byte quote = /* '\'' */ 39;
    bool quoted = default;
    ref var dst = ref heap<slice<byte>>(buffer(fmt), out var Ꮡdst);

    parser parser = default;

    parser.literal = error (byte b) => {
        if (b == quote) {
            Ꮡdst.ValueSlot = append(Ꮡdst.ValueSlot, quote, quote);
            return default;
        }
        if (!quoted && ('a' <= b && b <= 'z' || 'A' <= b && b <= 'Z')) {
            Ꮡdst.ValueSlot = append(Ꮡdst.ValueSlot, quote);
            quoted = true;
        }
        Ꮡdst.ValueSlot = append(Ꮡdst.ValueSlot, b);
        return default;
    };

    parser.format = error (byte spec, byte flag) => {
        if (quoted) {
            Ꮡdst.ValueSlot = append(Ꮡdst.ValueSlot, quote);
            quoted = false;
        }
        @string pattern = uts35Pattern(spec, flag); if (pattern != ""u8) {
            Ꮡdst.ValueSlot = append(Ꮡdst.ValueSlot, pattern.ꓸꓸꓸ);
            return default;
        }
        return new formatError();
    };

    var err = parser.parse(fmt); if (err != nil) {
        return ("", err);
    }
    if (quoted) {
        dst = append(dst, quote);
    }
    return ((@string)dst, default);
}

internal static slice<byte> /*buf*/ buffer(@string format) {
    slice<byte> buf = default;

    UntypedInt bufSize = 64;
    nint max = len(format) + 10;
    if (max < bufSize) {
        array<byte> b = new(64); /* bufSize */
        buf = b[..0];
    } else {
        buf = new slice<byte>(0, max);
    }
    return buf;
}

internal static time.Time year(nint y) {
    return time.Date(y, time.January, 1, 0, 0, 0, 0, time.UTC);
}

internal static nint century(time.Time t) {
    nint y = t.Year();
    nint c = y / 100;
    if (y % 100 < 0) {
        c--;
    }
    return c;
}

internal static slice<byte> appendWeekNumber(slice<byte> dst, time.Time t, byte flag, bool sunday) {
    nint offset = (nint)t.Weekday();
    if (sunday) {
        offset = 6 - offset;
    } else if (offset != 0) {
        offset = 7 - offset;
    }
    return appendInt2(dst, (t.YearDay() + offset) / 7, flag);
}

internal static slice<byte> append12Hour(slice<byte> dst, time.Time t, byte flag) {
    nint h = t.Hour();
    if (h == 0) {
        h = 12;
    } else if (h > 12) {
        h -= 12;
    }
    return appendInt2(dst, h, flag);
}

internal static slice<byte> appendInt1(slice<byte> dst, nint i) {
    return append(dst, (byte)('0' + i));
}

internal static slice<byte> appendInt2(slice<byte> dst, nint i, byte flag) {
    if (flag == 0 || i >= 10) {
        return append(dst, smallsString.slice(i * 2, i * 2 + 2).ꓸꓸꓸ);
    }
    if (flag == ' ') {
        dst = append(dst, flag);
    }
    return appendInt1(dst, i);
}

internal static readonly @string smallsString = "00010203040506070809101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899";

} // end strftime_package
