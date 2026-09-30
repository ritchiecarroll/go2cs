// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;

partial class time_package {

// RFC 3339 is the most commonly used format.
//
// It is implicitly used by the Time.(Marshal|Unmarshal)(Text|JSON) methods.
// Also, according to analysis on https://go.dev/issue/52746,
// RFC 3339 accounts for 57% of all explicitly specified time formats,
// with the second most popular format only being used 8% of the time.
// The overwhelming use of RFC 3339 compared to all other formats justifies
// the addition of logic to optimize formatting and parsing.
internal static slice<byte> appendFormatRFC3339(this Time t, slice<byte> b, bool nanos) {
    var (_, offset, abs) = t.locabs();
    // Format date.
    var (year, month, day) = abs.days().date();
    b = appendInt(b, year, 4);
    b = append(b, (byte)((rune)'-'));
    b = appendInt(b, (nint)month, 2);
    b = append(b, (byte)((rune)'-'));
    b = appendInt(b, day, 2);
    b = append(b, (byte)((rune)'T'));
    // Format time.
    var (hour, min, sec) = abs.clock();
    b = appendInt(b, hour, 2);
    b = append(b, (byte)((rune)':'));
    b = appendInt(b, min, 2);
    b = append(b, (byte)((rune)':'));
    b = appendInt(b, sec, 2);
    if (nanos) {
        nint std = stdFracSecond(stdFracSecond9, 9, (rune)'.');
        b = appendNano(b, t.Nanosecond(), std);
    }
    if (offset == 0) {
        return append(b, (byte)((rune)'Z'));
    }
    // Format zone.
    nint zone = offset / 60; // convert to minutes
    if (zone < 0){
        b = append(b, (byte)((rune)'-'));
        zone = -zone;
    } else {
        b = append(b, (byte)((rune)'+'));
    }
    b = appendInt(b, zone / 60, 2);
    b = append(b, (byte)((rune)':'));
    b = appendInt(b, zone % 60, 2);
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string yearOutsideOfRange09999ˢ = "year outside of range [0,9999]"u8;
internal static readonly @string timezoneHourOutsideOfˢ = "timezone hour outside of range [0,23]"u8;

internal static (slice<byte>, error) appendStrictRFC3339(this Time t, slice<byte> b) {
    nint n0 = len(b);
    b = t.appendFormatRFC3339(b, true);
    // Not all valid Go timestamps can be serialized as valid RFC 3339.
    // Explicitly check for these edge cases.
    // See https://go.dev/issue/4556 and https://go.dev/issue/54580.
    byte num2(slice<byte> bΔ1) => (byte)(10 * (bΔ1[0] - (rune)'0') + (bΔ1[1] - (rune)'0'));
    switch (ᐧ) {
    case {} when b[n0 + len("9999")] != (rune)'-': {
        return (b, errors.New(yearOutsideOfRange09999ˢ));
    }
    case {} when b[len(b) - 1] is not (rune)'Z': {
        var c = b[len(b) - len("Z07:00")];
        if (((rune)'0' <= c && c <= (rune)'9') || num2(b.slice(len(b) - len("07:00"))) >= 24) {
            // year must be exactly 4 digits wide
            return (b, errors.New(timezoneHourOutsideOfˢ));
        }
        break;
    }}

    return (b, default!);
}

internal static (Time, bool) parseRFC3339<bytes>(bytes s, ж<ΔLocation> Ꮡlocal)
    where bytes : /* []byte | string */ IByteSeq<bytes, byte>, new()
{
    ref var local = ref Ꮡlocal.DerefOrNull();

    // parseUint parses s as an unsigned decimal integer and
    // verifies that it is within some range.
    // If it is invalid or out-of-range,
    // it sets ok to false and returns the min value.
    var ok = true;
    nint /*x*/ parseUint(bytes sΔ1, nint minΔ1, nint max) {
        nint x = default!;
        foreach (var (_, c) in sΔ1.ToSlice()) {
            if (c < (rune)'0' || (rune)'9' < c) {
                ok = false;
                return minΔ1;
            }
            x = x * 10 + (nint)c - (rune)'0';
        }
        if (x < minΔ1 || max < x) {
            ok = false;
            return minΔ1;
        }
        return x;
    }
    // Parse the date and time.
    if (len(s) < len("2006-01-02T15:04:05")) {
        return (new Time(nil), false);
    }
    nint year = parseUint(s[0..4], 0, 9999); // e.g., 2006
    nint month = parseUint(s[5..7], 1, 12); // e.g., 01
    nint day = parseUint(s[8..10], 1, daysIn(((ΔMonth)month), year)); // e.g., 02
    nint hour = parseUint(s[11..13], 0, 23); // e.g., 15
    nint min = parseUint(s[14..16], 0, 59); // e.g., 04
    nint sec = parseUint(s[17..19], 0, 59); // e.g., 05
    if (!ok || !(s[4] == (rune)'-' && s[7] == (rune)'-' && s[10] == (rune)'T' && s[13] == (rune)':' && s[16] == (rune)':')) {
        return (new Time(nil), false);
    }
    s = s[19..];
    // Parse the fractional second.
    nint nsec = default!;
    if (len(s) >= 2 && s[0] == (rune)'.' && isDigit(s, 1)) {
        nint n = 2;
        for (; n < len(s) && isDigit(s, n); n++) {
        }
        (nsec, _, _) = parseNanoseconds(s, n);
        s = s.slice(n);
    }
    // Parse the time zone.
    var t = Date(year, ((ΔMonth)month), day, hour, min, sec, nsec, ΔUTC);
    if (len(s) != 1 || s[0] != (rune)'Z') {
        if (len(s) != len("-07:00")) {
            return (new Time(nil), false);
        }
        nint hr = parseUint(s[1..3], 0, 23); // e.g., 07
        nint mm = parseUint(s[4..6], 0, 59); // e.g., 00
        if (!ok || !((s[0] == (rune)'-' || s[0] == (rune)'+') && s[3] == (rune)':')) {
            return (new Time(nil), false);
        }
        nint zoneOffset = (hr * 60 + mm) * 60;
        if (s[0] == (rune)'-') {
            zoneOffset *= -1;
        }
        t.addSec(-(int64)zoneOffset);
        // Use local zone with the given offset if possible.
        {
            var (_, offset, _, _, _) = Ꮡlocal.lookup(t.unixSec()); if (offset == zoneOffset){
                t.setLoc(Ꮡlocal);
            } else {
                t.setLoc(FixedZone(""u8, zoneOffset));
            }
        }
    }
    return (t, true);
}

internal static (Time, error) parseStrictRFC3339(slice<byte> b) {
    var (t, ok) = parseRFC3339(b, ΔLocal);
    if (!ok) {
        var (tΔ1, err) = Parse(RFC3339, ((@string)b));
        if (err != default!) {
            return (new Time(nil), err);
        }
        // The parse template syntax cannot correctly validate RFC 3339.
        // Explicitly check for cases that Parse is unable to validate for.
        // See https://go.dev/issue/54580.
        byte num2(slice<byte> bΔ1) => (byte)(10 * (bΔ1[0] - (rune)'0') + (bΔ1[1] - (rune)'0'));
        switch (ᐧ) {
        case {} when ᐧᐧ: {
            return (tΔ1, default!);
        }
        case {} when b[len("2006-01-02T") + 1] == (rune)':': {
            return (new Time(nil), new ParseErrorжerror(Ꮡ(new ParseError( // TODO(https://go.dev/issue/54580): Strict parsing is disabled for now.
 // Enable this again with a GODEBUG opt-out.
 // hour must be two digits
RFC3339, ((@string)b), "15"u8, ((@string)(b[(int)(len("2006-01-02T"))..][..1])), ""u8))));
        }
        case {} when b[len("2006-01-02T15:04:05")] == (rune)',': {
            return (new Time(nil), new ParseErrorжerror(Ꮡ(new ParseError( // sub-second separator must be a period
RFC3339, ((@string)b), "."u8, ","u8, ""u8))));
        }
        case {} when b[len(b) - 1] is not (rune)'Z': {
            switch (ᐧ) {
            case {} when num2(b.slice(len(b) - len("07:00"))) >= 24: {
                return (new Time(nil), new ParseErrorжerror(Ꮡ(new ParseError( // timezone hour must be in range
RFC3339, ((@string)b), "Z07:00"u8, ((@string)(b.slice(len(b) - len("Z07:00")))), ": timezone hour out of range"u8))));
            }
            case {} when num2(b.slice(len(b) - len("00"))) >= 60: {
                return (new Time(nil), new ParseErrorжerror(Ꮡ(new ParseError( // timezone minute must be in range
RFC3339, ((@string)b), "Z07:00"u8, ((@string)(b.slice(len(b) - len("Z07:00")))), ": timezone minute out of range"u8))));
            }}

            break;
        }
        default: {
            return (new Time(nil), new ParseErrorжerror(Ꮡ(new ParseError( // unknown error; should not occur
RFC3339, ((@string)b), RFC3339, ((@string)b), ""u8))));
        }}

    }
    return (t, default!);
}

} // end time_package
