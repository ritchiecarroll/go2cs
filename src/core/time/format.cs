// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using stringslite = @internal.stringslite_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal;

partial class time_package {

// These are predefined layouts for use in [Time.Format] and [time.Parse].
// The reference time used in these layouts is the specific time stamp:
//
//	01/02 03:04:05PM '06 -0700
//
// (January 2, 15:04:05, 2006, in time zone seven hours west of GMT).
// That value is recorded as the constant named [Layout], listed below. As a Unix
// time, this is 1136239445. Since MST is GMT-0700, the reference would be
// printed by the Unix date command as:
//
//	Mon Jan 2 15:04:05 MST 2006
//
// It is a regrettable historic error that the date uses the American convention
// of putting the numerical month before the day.
//
// The example for Time.Format demonstrates the working of the layout string
// in detail and is a good reference.
//
// Note that the [RFC822], [RFC850], and [RFC1123] formats should be applied
// only to local times. Applying them to UTC times will use "UTC" as the
// time zone abbreviation, while strictly speaking those RFCs require the
// use of "GMT" in that case.
// When using the [RFC1123] or [RFC1123Z] formats for parsing, note that these
// formats define a leading zero for the day-in-month portion, which is not
// strictly allowed by RFC 1123. This will result in an error when parsing
// date strings that occur in the first 9 days of a given month.
// In general [RFC1123Z] should be used instead of [RFC1123] for servers
// that insist on that format, and [RFC3339] should be preferred for new protocols.
// [RFC3339], [RFC822], [RFC822Z], [RFC1123], and [RFC1123Z] are useful for formatting;
// when used with time.Parse they do not accept all the time formats
// permitted by the RFCs and they do accept time formats not formally defined.
// The [RFC3339Nano] format removes trailing zeros from the seconds field
// and thus may not sort correctly once formatted.
//
// Most programs can use one of the defined constants as the layout passed to
// Format or Parse. The rest of this comment can be ignored unless you are
// creating a custom layout string.
//
// To define your own format, write down what the reference time would look like
// formatted your way; see the values of constants like [ANSIC], [StampMicro] or
// [Kitchen] for examples. The model is to demonstrate what the reference time
// looks like so that the Format and Parse methods can apply the same
// transformation to a general time value.
//
// Here is a summary of the components of a layout string. Each element shows by
// example the formatting of an element of the reference time. Only these values
// are recognized. Text in the layout string that is not recognized as part of
// the reference time is echoed verbatim during Format and expected to appear
// verbatim in the input to Parse.
//
//	Year: "2006" "06"
//	Month: "Jan" "January" "01" "1"
//	Day of the week: "Mon" "Monday"
//	Day of the month: "2" "_2" "02"
//	Day of the year: "__2" "002"
//	Hour: "15" "3" "03" (PM or AM)
//	Minute: "4" "04"
//	Second: "5" "05"
//	AM/PM mark: "PM"
//
// Numeric time zone offsets format as follows:
//
//	"-0700"     ±hhmm
//	"-07:00"    ±hh:mm
//	"-07"       ±hh
//	"-070000"   ±hhmmss
//	"-07:00:00" ±hh:mm:ss
//
// Replacing the sign in the format with a Z triggers
// the ISO 8601 behavior of printing Z instead of an
// offset for the UTC zone. Thus:
//
//	"Z0700"      Z or ±hhmm
//	"Z07:00"     Z or ±hh:mm
//	"Z07"        Z or ±hh
//	"Z070000"    Z or ±hhmmss
//	"Z07:00:00"  Z or ±hh:mm:ss
//
// Within the format string, the underscores in "_2" and "__2" represent spaces
// that may be replaced by digits if the following number has multiple digits,
// for compatibility with fixed-width Unix time formats. A leading zero represents
// a zero-padded value.
//
// The formats __2 and 002 are space-padded and zero-padded
// three-character day of year; there is no unpadded day of year format.
//
// A comma or decimal point followed by one or more zeros represents
// a fractional second, printed to the given number of decimal places.
// A comma or decimal point followed by one or more nines represents
// a fractional second, printed to the given number of decimal places, with
// trailing zeros removed.
// For example "15:04:05,000" or "15:04:05.000" formats or parses with
// millisecond precision.
//
// Some valid layouts are invalid time values for time.Parse, due to formats
// such as _ for space padding and Z for zone information.
public static readonly @string Layout = "01/02 03:04:05PM '06 -0700"u8; // The reference time, in numerical order.

public static readonly @string ANSIC = "Mon Jan _2 15:04:05 2006"u8;

public static readonly @string UnixDate = "Mon Jan _2 15:04:05 MST 2006"u8;

public static readonly @string RubyDate = "Mon Jan 02 15:04:05 -0700 2006"u8;

public static readonly @string RFC822 = "02 Jan 06 15:04 MST"u8;

public static readonly @string RFC822Z = "02 Jan 06 15:04 -0700"u8; // RFC822 with numeric zone

public static readonly @string RFC850 = "Monday, 02-Jan-06 15:04:05 MST"u8;

public static readonly @string RFC1123 = "Mon, 02 Jan 2006 15:04:05 MST"u8;

public static readonly @string RFC1123Z = "Mon, 02 Jan 2006 15:04:05 -0700"u8; // RFC1123 with numeric zone

public static readonly @string RFC3339 = "2006-01-02T15:04:05Z07:00"u8;

public static readonly @string RFC3339Nano = "2006-01-02T15:04:05.999999999Z07:00"u8;

public static readonly @string Kitchen = "3:04PM"u8;

public static readonly @string Stamp = "Jan _2 15:04:05"u8;

public static readonly @string StampMilli = "Jan _2 15:04:05.000"u8;

public static readonly @string StampMicro = "Jan _2 15:04:05.000000"u8;

public static readonly @string StampNano = "Jan _2 15:04:05.000000000"u8;

public static readonly @string DateTime = "2006-01-02 15:04:05"u8;

public static readonly @string DateOnly = "2006-01-02"u8;

public static readonly @string TimeOnly = "15:04:05"u8;

internal static UntypedInt _ᴛ1ʗ => iota;
internal static UntypedInt stdLongMonth => /* iota + stdNeedDate */ 257; // "January"
internal static UntypedInt stdMonth => 258;                    // "Jan"
internal static UntypedInt stdNumMonth => 259;                 // "1"
internal static UntypedInt stdZeroMonth => 260;                // "01"
internal static UntypedInt stdLongWeekDay => 261;              // "Monday"
internal static UntypedInt stdWeekDay => 262;                  // "Mon"
internal static UntypedInt stdDay => 263;                      // "2"
internal static UntypedInt stdUnderDay => 264;                 // "_2"
internal static UntypedInt stdZeroDay => 265;                  // "02"
internal static UntypedInt stdUnderYearDay => /* iota + stdNeedYday */ 522; // "__2"
internal static UntypedInt stdZeroYearDay => 523;              // "002"
internal static UntypedInt stdHour => /* iota + stdNeedClock */ 1036; // "15"
internal static UntypedInt stdHour12 => 1037;                   // "3"
internal static UntypedInt stdZeroHour12 => 1038;               // "03"
internal static UntypedInt stdMinute => 1039;                   // "4"
internal static UntypedInt stdZeroMinute => 1040;               // "04"
internal static UntypedInt stdSecond => 1041;                   // "5"
internal static UntypedInt stdZeroSecond => 1042;               // "05"
internal static UntypedInt stdLongYear => /* iota + stdNeedDate */ 275; // "2006"
internal static UntypedInt stdYear => 276;                     // "06"
internal static UntypedInt stdPM => /* iota + stdNeedClock */ 1045; // "PM"
internal static UntypedInt stdpm => 1046;                       // "pm"
internal static UntypedInt stdTZ => /* iota */ 23;   // "MST"
internal static UntypedInt stdISO8601TZ => 24;                // "Z0700"  // prints Z for UTC
internal static UntypedInt stdISO8601SecondsTZ => 25;         // "Z070000"
internal static UntypedInt stdISO8601ShortTZ => 26;           // "Z07"
internal static UntypedInt stdISO8601ColonTZ => 27;           // "Z07:00" // prints Z for UTC
internal static UntypedInt stdISO8601ColonSecondsTZ => 28;    // "Z07:00:00"
internal static UntypedInt stdNumTZ => 29;                    // "-0700"  // always numeric
internal static UntypedInt stdNumSecondsTz => 30;             // "-070000"
internal static UntypedInt stdNumShortTZ => 31;               // "-07"    // always numeric
internal static UntypedInt stdNumColonTZ => 32;               // "-07:00" // always numeric
internal static UntypedInt stdNumColonSecondsTZ => 33;        // "-07:00:00"
internal static UntypedInt stdFracSecond0 => 34;              // ".0", ".00", ... , trailing zeros included
internal static UntypedInt stdFracSecond9 => 35;              // ".9", ".99", ..., trailing zeros omitted
internal static UntypedInt stdNeedDate => /* 1 << 8 */ 256; // need month, day, year
internal static UntypedInt stdNeedYday => /* 1 << 9 */ 512; // need yday
internal static UntypedInt stdNeedClock => /* 1 << 10 */ 1024; // need hour, minute, second
internal static UntypedInt stdArgShift => 16;  // extra argument in high bits, above low stdArgShift
internal static UntypedInt stdSeparatorShift => 28;  // extra argument in high 4 bits for fractional second separators
internal static UntypedInt stdMask => /* 1<<stdArgShift - 1 */ 65535; // mask out argument

// std0x records the std values for "01", "02", ..., "06".
internal static array<nint> std0x = new nint[]{stdZeroMonth, stdZeroDay, stdZeroHour12, stdZeroMinute, stdZeroSecond, stdYear}.array();

// startsWithLowerCase reports whether the string has a lower-case letter at the beginning.
// Its purpose is to prevent matching strings like "Month" when looking for "Mon".
internal static bool startsWithLowerCase(@string str) {
    if (len(str) == 0) {
        return false;
    }
    var c = str[0];
    return (rune)'a' <= c && c <= (rune)'z';
}

// nextStdChunk finds the first occurrence of a std string in
// layout and returns the text before, the std string, and the text after.
//
// nextStdChunk should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang/go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname nextStdChunk
internal static (@string prefix, nint std, @string suffix) nextStdChunk(@string layout) {
    for (nint i = 0; i < len(layout); i++) {
        {
            nint c = (nint)layout[i];
            switch (c) {
            case (rune)'J': {
                if (len(layout) >= i + 3 && layout.slice(i, i + 3) == "Jan") {
                    // January, Jan
                    if (len(layout) >= i + 7 && layout.slice(i, i + 7) == "January") {
                        return (layout.slice(0, i), stdLongMonth, layout.slice(i + 7));
                    }
                    if (!startsWithLowerCase(layout.slice(i + 3))) {
                        return (layout.slice(0, i), stdMonth, layout.slice(i + 3));
                    }
                }
                break;
            }
            case (rune)'M': {
                if (len(layout) >= i + 3) {
                    // Monday, Mon, MST
                    if (layout.slice(i, i + 3) == "Mon") {
                        if (len(layout) >= i + 6 && layout.slice(i, i + 6) == "Monday") {
                            return (layout.slice(0, i), stdLongWeekDay, layout.slice(i + 6));
                        }
                        if (!startsWithLowerCase(layout.slice(i + 3))) {
                            return (layout.slice(0, i), stdWeekDay, layout.slice(i + 3));
                        }
                    }
                    if (layout.slice(i, i + 3) == "MST") {
                        return (layout.slice(0, i), stdTZ, layout.slice(i + 3));
                    }
                }
                break;
            }
            case (rune)'0': {
                if (len(layout) >= i + 2 && (rune)'1' <= layout[i + 1] && layout[i + 1] <= (rune)'6') {
                    // 01, 02, 03, 04, 05, 06, 002
                    return (layout.slice(0, i), std0x[(byte)(layout[i + 1] - (rune)'1')], layout.slice(i + 2));
                }
                if (len(layout) >= i + 3 && layout[i + 1] == (rune)'0' && layout[i + 2] == (rune)'2') {
                    return (layout.slice(0, i), stdZeroYearDay, layout.slice(i + 3));
                }
                break;
            }
            case (rune)'1': {
                if (len(layout) >= i + 2 && layout[i + 1] == (rune)'5') {
                    // 15, 1
                    return (layout.slice(0, i), stdHour, layout.slice(i + 2));
                }
                return (layout.slice(0, i), stdNumMonth, layout.slice(i + 1));
            }
            case (rune)'2': {
                if (len(layout) >= i + 4 && layout.slice(i, i + 4) == "2006") {
                    // 2006, 2
                    return (layout.slice(0, i), stdLongYear, layout.slice(i + 4));
                }
                return (layout.slice(0, i), stdDay, layout.slice(i + 1));
            }
            case (rune)'_': {
                if (len(layout) >= i + 2 && layout[i + 1] == (rune)'2') {
                    // _2, _2006, __2
                    // _2006 is really a literal _, followed by stdLongYear
                    if (len(layout) >= i + 5 && layout.slice(i + 1, i + 5) == "2006") {
                        return (layout.slice(0, i + 1), stdLongYear, layout.slice(i + 5));
                    }
                    return (layout.slice(0, i), stdUnderDay, layout.slice(i + 2));
                }
                if (len(layout) >= i + 3 && layout[i + 1] == (rune)'_' && layout[i + 2] == (rune)'2') {
                    return (layout.slice(0, i), stdUnderYearDay, layout.slice(i + 3));
                }
                break;
            }
            case (rune)'3': {
                return (layout.slice(0, i), stdHour12, layout.slice(i + 1));
            }
            case (rune)'4': {
                return (layout.slice(0, i), stdMinute, layout.slice(i + 1));
            }
            case (rune)'5': {
                return (layout.slice(0, i), stdSecond, layout.slice(i + 1));
            }
            case (rune)'P': {
                if (len(layout) >= i + 2 && layout[i + 1] == (rune)'M') {
                    // PM
                    return (layout.slice(0, i), stdPM, layout.slice(i + 2));
                }
                break;
            }
            case (rune)'p': {
                if (len(layout) >= i + 2 && layout[i + 1] == (rune)'m') {
                    // pm
                    return (layout.slice(0, i), stdpm, layout.slice(i + 2));
                }
                break;
            }
            case (rune)'-': {
                if (len(layout) >= i + 7 && layout.slice(i, i + 7) == "-070000") {
                    // -070000, -07:00:00, -0700, -07:00, -07
                    return (layout.slice(0, i), stdNumSecondsTz, layout.slice(i + 7));
                }
                if (len(layout) >= i + 9 && layout.slice(i, i + 9) == "-07:00:00") {
                    return (layout.slice(0, i), stdNumColonSecondsTZ, layout.slice(i + 9));
                }
                if (len(layout) >= i + 5 && layout.slice(i, i + 5) == "-0700") {
                    return (layout.slice(0, i), stdNumTZ, layout.slice(i + 5));
                }
                if (len(layout) >= i + 6 && layout.slice(i, i + 6) == "-07:00") {
                    return (layout.slice(0, i), stdNumColonTZ, layout.slice(i + 6));
                }
                if (len(layout) >= i + 3 && layout.slice(i, i + 3) == "-07") {
                    return (layout.slice(0, i), stdNumShortTZ, layout.slice(i + 3));
                }
                break;
            }
            case (rune)'Z': {
                if (len(layout) >= i + 7 && layout.slice(i, i + 7) == "Z070000") {
                    // Z070000, Z07:00:00, Z0700, Z07:00,
                    return (layout.slice(0, i), stdISO8601SecondsTZ, layout.slice(i + 7));
                }
                if (len(layout) >= i + 9 && layout.slice(i, i + 9) == "Z07:00:00") {
                    return (layout.slice(0, i), stdISO8601ColonSecondsTZ, layout.slice(i + 9));
                }
                if (len(layout) >= i + 5 && layout.slice(i, i + 5) == "Z0700") {
                    return (layout.slice(0, i), stdISO8601TZ, layout.slice(i + 5));
                }
                if (len(layout) >= i + 6 && layout.slice(i, i + 6) == "Z07:00") {
                    return (layout.slice(0, i), stdISO8601ColonTZ, layout.slice(i + 6));
                }
                if (len(layout) >= i + 3 && layout.slice(i, i + 3) == "Z07") {
                    return (layout.slice(0, i), stdISO8601ShortTZ, layout.slice(i + 3));
                }
                break;
            }
            case (rune)'.' or (rune)',': {
                if (i + 1 < len(layout) && (layout[i + 1] == (rune)'0' || layout[i + 1] == (rune)'9')) {
                    // ,000, or .000, or ,999, or .999 - repeated digits for fractional seconds.
                    var ch = layout[i + 1];
                    nint j = i + 1;
                    while (j < len(layout) && layout[j] == ch) {
                        j++;
                    }
                    // String of digits must end here - only fractional second is all digits.
                    if (!isDigit(layout, j)) {
                        nint code = stdFracSecond0;
                        if (layout[i + 1] == (rune)'9') {
                            code = stdFracSecond9;
                        }
                        nint stdΔ2 = stdFracSecond(code, j - (i + 1), c);
                        return (layout.slice(0, i), stdΔ2, layout.slice(j));
                    }
                }
                break;
            }}
        }

    }
    return (layout, 0, "");
}

internal static slice<@string> longDayNames = new @string[]{
    "Sunday"u8,
    "Monday"u8,
    "Tuesday"u8,
    "Wednesday"u8,
    "Thursday"u8,
    "Friday"u8,
    "Saturday"u8
}.slice();

internal static slice<@string> shortDayNames = new @string[]{
    "Sun"u8,
    "Mon"u8,
    "Tue"u8,
    "Wed"u8,
    "Thu"u8,
    "Fri"u8,
    "Sat"u8
}.slice();

internal static slice<@string> shortMonthNames = new @string[]{
    "Jan"u8,
    "Feb"u8,
    "Mar"u8,
    "Apr"u8,
    "May"u8,
    "Jun"u8,
    "Jul"u8,
    "Aug"u8,
    "Sep"u8,
    "Oct"u8,
    "Nov"u8,
    "Dec"u8
}.slice();

internal static slice<@string> longMonthNames = new @string[]{
    "January"u8,
    "February"u8,
    "March"u8,
    "April"u8,
    "May"u8,
    "June"u8,
    "July"u8,
    "August"u8,
    "September"u8,
    "October"u8,
    "November"u8,
    "December"u8
}.slice();

// match reports whether s1 and s2 match ignoring case.
// It is assumed s1 and s2 are the same length.
internal static bool match(@string s1, @string s2) {
    for (nint i = 0; i < len(s1); i++) {
        var c1 = s1[i];
        var c2 = s2[i];
        if (c1 != c2) {
            // Switch to lower-case; 'a'-'A' is known to be a single bit.
            c1 |= (byte)((rune)'a' - (rune)'A');
            c2 |= (byte)((rune)'a' - (rune)'A');
            if (c1 != c2 || c1 < (rune)'a' || c1 > (rune)'z') {
                return false;
            }
        }
    }
    return true;
}

internal static (nint, @string, error) lookup(slice<@string> tab, @string val) {
    foreach (var (i, v) in tab) {
        if (len(val) >= len(v) && match(val.slice(0, len(v)), v)) {
            return (i, val.slice(len(v)), default!);
        }
    }
    return (-1, val, errBad);
}

// appendInt appends the decimal form of x to b and returns the result.
// If the decimal form (excluding sign) is shorter than width, the result is padded with leading 0's.
// Duplicates functionality in strconv, but avoids dependency.
internal static slice<byte> appendInt(slice<byte> b, nint x, nint width) {
    nuint u = (nuint)x;
    if (x < 0) {
        b = append(b, (byte)((rune)'-'));
        u = (nuint)(-x);
    }
    // 2-digit and 4-digit fields are the most common in time formats.
    byte utod(nuint uΔ1) => (byte)((rune)'0' + (byte)uΔ1);
    switch (ᐧ) {
    case {} when width == 2 && u < 100: {
        return append(b, utod(u / 10), utod(u % 10));
    }
    case {} when width == 4 && u < 10000: {
        return append(b, utod(u / 1000), utod(u / 100 % 10), utod(u / 10 % 10), utod(u % 10));
    }}

    // Compute the number of decimal digits.
    nint n = default!;
    if (u == 0) {
        n = 1;
    }
    for (nuint u2 = u; u2 > 0; u2 /= 10) {
        n++;
    }
    // Add 0-padding.
    for (nint pad = width - n; pad > 0; pad--) {
        b = append(b, (byte)((rune)'0'));
    }
    // Ensure capacity.
    if (len(b) + n <= cap(b)){
        b = b.slice(0, len(b) + n);
    } else {
        b = appendꓸꓸꓸ(b, makeꓸꓸꓸ<byte>(n));
    }
    // Assemble decimal in reverse order.
    nint i = len(b) - 1;
    while (u >= 10 && i > 0) {
        nuint q = u / 10;
        b[i] = utod(u - q * 10);
        u = q;
        i--;
    }
    b[i] = utod(u);
    return b;
}

// Never printed, just needs to be non-nil for return by atoi.
internal static error errAtoi = errors.New("time: invalid number"u8);

// Duplicates functionality in strconv, but avoids dependency.
internal static (nint x, error err) atoi<bytes>(bytes s)
    where bytes : /* []byte | string */ IByteSeq<bytes, byte>, new()
{
    nint x = default!;
    error err = default!;

    var neg = false;
    if (len(s) > 0 && (s[0] == (rune)'-' || s[0] == (rune)'+')) {
        neg = s[0] == (rune)'-';
        s = s[1..];
    }
    (var q, var rem, err) = leadingInt(s);
    x = (nint)q;
    if (err != default! || len(rem) > 0) {
        return (0, errAtoi);
    }
    if (neg) {
        x = -x;
    }
    return (x, default!);
}

// The "std" value passed to appendNano contains two packed fields: the number of
// digits after the decimal and the separator character (period or comma).
// These functions pack and unpack that variable.
internal static nint stdFracSecond(nint code, nint n, nint c) {
    // Use 0xfff to make the failure case even more absurd.
    if (c == (rune)'.') {
        return (nint)(code | ((((nint)(n & 0xfff)) << (int)(stdArgShift))));
    }
    return (nint)((nint)(code | ((((nint)(n & 0xfff)) << (int)(stdArgShift)))) | (nint)(1 << (int)(stdSeparatorShift)));
}

internal static nint digitsLen(nint std) {
    return (nint)(((std >> (int)(stdArgShift))) & 0xfff);
}

internal static byte separator(nint std) {
    if (((std >> (int)(stdSeparatorShift))) == 0) {
        return (rune)'.';
    }
    return (rune)',';
}

// appendNano appends a fractional second, as nanoseconds, to b
// and returns the result. The nanosec must be within [0, 999999999].
internal static slice<byte> appendNano(slice<byte> b, nint nanosec, nint std) {
    var trim = (nint)(std & (nint)stdMask) == stdFracSecond9;
    nint n = digitsLen(std);
    if (trim && (n == 0 || nanosec == 0)) {
        return b;
    }
    var dot = separator(std);
    b = append(b, dot);
    b = appendInt(b, nanosec, 9);
    if (n < 9) {
        b = b.slice(0, len(b) - 9 + n);
    }
    if (trim) {
        while (len(b) > 0 && b[len(b) - 1] == (rune)'0') {
            b = b.slice(0, len(b) - 1);
        }
        if (len(b) > 0 && b[len(b) - 1] == dot) {
            b = b.slice(0, len(b) - 1);
        }
    }
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mstˢ = "2006-01-02 15:04:05.999999999 -0700 MST"u8;

// String returns the time formatted using the format string
//
//	"2006-01-02 15:04:05.999999999 -0700 MST"
//
// If the time has a monotonic clock reading, the returned string
// includes a final field "m=±<value>", where value is the monotonic
// clock reading formatted as a decimal number of seconds.
//
// The returned string is meant for debugging; for a stable serialized
// representation, use t.MarshalText, t.MarshalBinary, or t.Format
// with an explicit format string.
public static @string String(this Time t) {
    @string s = t.Format(mstˢ);
    // Format monotonic clock reading as m=±ddd.nnnnnnnnn.
    if ((uint64)(t.wall & (uint64)hasMonotonic) != 0) {
        var m2 = (uint64)t.ext;
        var sign = (byte)(rune)'+';
        if (t.ext < 0) {
            sign = (rune)'-';
            m2 = ((uint64)0 - m2);
        }
        var m1 = m2 / 1000000000;
        m2 = m2 % 1000000000;
        var m0 = m1 / 1000000000;
        m1 = m1 % 1000000000;
        var buf = new slice<byte>(0, 24);
        buf = append(buf, ((@string)" m="u8).ꓸꓸꓸ);
        buf = append(buf, sign);
        nint wid = 0;
        if (m0 != 0) {
            buf = appendInt(buf, (nint)m0, 0);
            wid = 9;
        }
        buf = appendInt(buf, (nint)m1, wid);
        buf = append(buf, (byte)((rune)'.'));
        buf = appendInt(buf, (nint)m2, 9);
        s += ((@string)buf);
    }
    return s;
}

// GoString implements [fmt.GoStringer] and formats t to be printed in Go source
// code.
public static @string GoString(this Time t) {
    var abs = t.absSec();
    var (year, month, day) = abs.days().date();
    var (hour, minute, second) = abs.clock();
    var buf = new slice<byte>(0, len("time.Date(9999, time.September, 31, 23, 59, 59, 999999999, time.Local)"));
    buf = append(buf, ((@string)"time.Date("u8).ꓸꓸꓸ);
    buf = appendInt(buf, year, 0);
    if (January <= month && month <= December){
        buf = append(buf, ((@string)", time."u8).ꓸꓸꓸ);
        buf = append(buf, longMonthNames[month - 1].ꓸꓸꓸ);
    } else {
        // It's difficult to construct a time.Time with a date outside the
        // standard range but we might as well try to handle the case.
        buf = appendInt(buf, (nint)month, 0);
    }
    buf = append(buf, ((@string)", "u8).ꓸꓸꓸ);
    buf = appendInt(buf, day, 0);
    buf = append(buf, ((@string)", "u8).ꓸꓸꓸ);
    buf = appendInt(buf, hour, 0);
    buf = append(buf, ((@string)", "u8).ꓸꓸꓸ);
    buf = appendInt(buf, minute, 0);
    buf = append(buf, ((@string)", "u8).ꓸꓸꓸ);
    buf = appendInt(buf, second, 0);
    buf = append(buf, ((@string)", "u8).ꓸꓸꓸ);
    buf = appendInt(buf, t.Nanosecond(), 0);
    buf = append(buf, ((@string)", "u8).ꓸꓸꓸ);
    {
        var loc = t.Location();
        var exprᴛ1 = loc;
        if (exprᴛ1 == ΔUTC || exprᴛ1 == default!) {
            buf = append(buf, ((@string)"time.UTC"u8).ꓸꓸꓸ);
        }
        else if (exprᴛ1 == ΔLocal) {
            buf = append(buf, ((@string)"time.Local"u8).ꓸꓸꓸ);
        }
        else { /* default: */
            buf = append(buf, // there are several options for how we could display this, none of
 // which are great:
 //
 // - use Location(loc.name), which is not technically valid syntax
 // - use LoadLocation(loc.name), which will cause a syntax error when
 // embedded and also would require us to escape the string without
 // importing fmt or strconv
 // - try to use FixedZone, which would also require escaping the name
 // and would represent e.g. "America/Los_Angeles" daylight saving time
 // shifts inaccurately
 // - use the pointer format, which is no worse than you'd get with the
 // old fmt.Sprintf("%#v", t) format.
 //
 // Of these, Location(loc.name) is the least disruptive. This is an edge
 // case we hope not to hit too often.
 ((@string)@"time.Location("u8).ꓸꓸꓸ);
            buf = append(buf, quote((~loc).name).ꓸꓸꓸ);
            buf = append(buf, (byte)((rune)')'));
        }
    }

    buf = append(buf, (byte)((rune)')'));
    return ((@string)buf);
}

// Format returns a textual representation of the time value formatted according
// to the layout defined by the argument. See the documentation for the
// constant called [Layout] to see how to represent the layout format.
//
// The executable example for [Time.Format] demonstrates the working
// of the layout string in detail and is a good reference.
public static @string Format(this Time t, @string layout) {
    UntypedInt bufSize = 64;
    slice<byte> b = default!;
    nint max = len(layout) + 10;
    if (max < bufSize){
        array<byte> buf = new(64); /* bufSize */
        b = buf[..0];
    } else {
        b = new slice<byte>(0, max);
    }
    b = t.AppendFormat(b, layout);
    return ((@string)b);
}

// AppendFormat is like [Time.Format] but appends the textual
// representation to b and returns the extended buffer.
public static slice<byte> AppendFormat(this Time t, slice<byte> b, @string layout) {
    // Optimize for RFC3339 as it accounts for over half of all representations.
    var exprᴛ1 = layout;
    if (exprᴛ1 == RFC3339) {
        return t.appendFormatRFC3339(b, false);
    }
    if (exprᴛ1 == RFC3339Nano) {
        return t.appendFormatRFC3339(b, true);
    }
    { /* default: */
        return t.appendFormat(b, layout);
    }

}

internal static slice<byte> appendFormat(this Time t, slice<byte> b, @string layout) {
    var (name, offset, abs) = t.locabs();
    var days = abs.days();
    nint year = -1;
    ΔMonth month = default!;
    nint day = default!;
    nint yday = -1;
    nint hour = -1;
    nint min = default!;
    nint sec = default!;
    // Each iteration generates one std value.
    while (layout != ""u8) {
        var (prefix, std, suffix) = nextStdChunk(layout);
        if (prefix != ""u8) {
            b = append(b, prefix.ꓸꓸꓸ);
        }
        if (std == 0) {
            break;
        }
        layout = suffix;
        // Compute year, month, day if needed.
        if (year < 0 && (nint)(std & (nint)stdNeedDate) != 0) {
            (year, month, day) = days.date();
        }
        if (yday < 0 && (nint)(std & (nint)stdNeedYday) != 0) {
            (_, yday) = days.yearYday();
        }
        // Compute hour, minute, second if needed.
        if (hour < 0 && (nint)(std & (nint)stdNeedClock) != 0) {
            (hour, min, sec) = abs.clock();
        }
        var exprᴛ1 = (nint)(std & (nint)stdMask);
        if (exprᴛ1 == stdYear) {
            nint y = year;
            if (y < 0) {
                y = -y;
            }
            b = appendInt(b, y % 100, 2);
        }
        else if (exprᴛ1 == stdLongYear) {
            b = appendInt(b, year, 4);
        }
        else if (exprᴛ1 == stdMonth) {
            b = append(b, month.String()[..3].ꓸꓸꓸ);
        }
        else if (exprᴛ1 == stdLongMonth) {
            @string m = month.String();
            b = append(b, m.ꓸꓸꓸ);
        }
        else if (exprᴛ1 == stdNumMonth) {
            b = appendInt(b, (nint)month, 0);
        }
        else if (exprᴛ1 == stdZeroMonth) {
            b = appendInt(b, (nint)month, 2);
        }
        else if (exprᴛ1 == stdWeekDay) {
            b = append(b, days.weekday().String()[..3].ꓸꓸꓸ);
        }
        else if (exprᴛ1 == stdLongWeekDay) {
            @string s = days.weekday().String();
            b = append(b, s.ꓸꓸꓸ);
        }
        else if (exprᴛ1 == stdDay) {
            b = appendInt(b, day, 0);
        }
        else if (exprᴛ1 == stdUnderDay) {
            if (day < 10) {
                b = append(b, (byte)((rune)' '));
            }
            b = appendInt(b, day, 0);
        }
        else if (exprᴛ1 == stdZeroDay) {
            b = appendInt(b, day, 2);
        }
        else if (exprᴛ1 == stdUnderYearDay) {
            if (yday < 100) {
                b = append(b, (byte)((rune)' '));
                if (yday < 10) {
                    b = append(b, (byte)((rune)' '));
                }
            }
            b = appendInt(b, yday, 0);
        }
        else if (exprᴛ1 == stdZeroYearDay) {
            b = appendInt(b, yday, 3);
        }
        else if (exprᴛ1 == stdHour) {
            b = appendInt(b, hour, 2);
        }
        else if (exprᴛ1 == stdHour12) {
            nint hr = hour % 12;
            if (hr == 0) {
                // Noon is 12PM, midnight is 12AM.
                hr = 12;
            }
            b = appendInt(b, hr, 0);
        }
        else if (exprᴛ1 == stdZeroHour12) {
            nint hr = hour % 12;
            if (hr == 0) {
                // Noon is 12PM, midnight is 12AM.
                hr = 12;
            }
            b = appendInt(b, hr, 2);
        }
        else if (exprᴛ1 == stdMinute) {
            b = appendInt(b, min, 0);
        }
        else if (exprᴛ1 == stdZeroMinute) {
            b = appendInt(b, min, 2);
        }
        else if (exprᴛ1 == stdSecond) {
            b = appendInt(b, sec, 0);
        }
        else if (exprᴛ1 == stdZeroSecond) {
            b = appendInt(b, sec, 2);
        }
        else if (exprᴛ1 == stdPM) {
            if (hour >= 12){
                b = append(b, ((@string)"PM"u8).ꓸꓸꓸ);
            } else {
                b = append(b, ((@string)"AM"u8).ꓸꓸꓸ);
            }
        }
        else if (exprᴛ1 == stdpm) {
            if (hour >= 12){
                b = append(b, ((@string)"pm"u8).ꓸꓸꓸ);
            } else {
                b = append(b, ((@string)"am"u8).ꓸꓸꓸ);
            }
        }
        else if (exprᴛ1 == stdISO8601TZ || exprᴛ1 == stdISO8601ColonTZ || exprᴛ1 == stdISO8601SecondsTZ || exprᴛ1 == stdISO8601ShortTZ || exprᴛ1 == stdISO8601ColonSecondsTZ || exprᴛ1 == stdNumTZ || exprᴛ1 == stdNumColonTZ || exprᴛ1 == stdNumSecondsTz || exprᴛ1 == stdNumShortTZ || exprᴛ1 == stdNumColonSecondsTZ) {
            do {
                if (offset == 0 && (std == stdISO8601TZ || std == stdISO8601ColonTZ || std == stdISO8601SecondsTZ || std == stdISO8601ShortTZ || std == stdISO8601ColonSecondsTZ)) {
                    // Ugly special case. We cheat and take the "Z" variants
                    // to mean "the time zone as formatted for ISO 8601".
                    b = append(b, (byte)((rune)'Z'));
                    break;
                }
                nint zone = offset / 60; // convert to minutes
                nint absoffset = offset;
                if (zone < 0){
                    b = append(b, (byte)((rune)'-'));
                    zone = -zone;
                    absoffset = -absoffset;
                } else {
                    b = append(b, (byte)((rune)'+'));
                }
                b = appendInt(b, zone / 60, 2);
                if (std == stdISO8601ColonTZ || std == stdNumColonTZ || std == stdISO8601ColonSecondsTZ || std == stdNumColonSecondsTZ) {
                    b = append(b, (byte)((rune)':'));
                }
                if (std != stdNumShortTZ && std != stdISO8601ShortTZ) {
                    b = appendInt(b, zone % 60, 2);
                }
                if (std == stdISO8601SecondsTZ || std == stdNumSecondsTz || std == stdNumColonSecondsTZ || std == stdISO8601ColonSecondsTZ) {
                    // append seconds if appropriate
                    if (std == stdNumColonSecondsTZ || std == stdISO8601ColonSecondsTZ) {
                        b = append(b, (byte)((rune)':'));
                    }
                    b = appendInt(b, absoffset % 60, 2);
                }
            } while (false);
        }
        else if (exprᴛ1 == stdTZ) {
            do {
                if (name != ""u8) {
                    b = append(b, name.ꓸꓸꓸ);
                    break;
                }
                nint zone = offset / 60; // convert to minutes
                if (zone < 0){
                    // No time zone known for this time, but we must print one.
                    // Use the -0700 format.
                    b = append(b, (byte)((rune)'-'));
                    zone = -zone;
                } else {
                    b = append(b, (byte)((rune)'+'));
                }
                b = appendInt(b, zone / 60, 2);
                b = appendInt(b, zone % 60, 2);
            } while (false);
        }
        else if (exprᴛ1 == stdFracSecond0 || exprᴛ1 == stdFracSecond9) {
            b = appendNano(b, t.Nanosecond(), std);
        }

    }
    return b;
}

internal static error errBad = errors.New("bad value for field"u8); // placeholder not passed to user

// ParseError describes a problem parsing a time string.
[GoType] partial struct ParseError {
    public @string Layout;
    public @string Value;
    public @string LayoutElem;
    public @string ValueElem;
    public @string Message;
}

// newParseError creates a new ParseError.
// The provided value and valueElem are cloned to avoid escaping their values.
internal static ж<ParseError> newParseError(@string layout, @string value, @string layoutElem, @string valueElem, @string message) {
    ref var valueCopy = ref heap<@string>(out var ᏑvalueCopy);
    valueCopy = stringslite.Clone(value);
    ref var valueElemCopy = ref heap<@string>(out var ᏑvalueElemCopy);
    valueElemCopy = stringslite.Clone(valueElem);
    return Ꮡ(new ParseError(layout, valueCopy, layoutElem, valueElemCopy, message));
}

// These are borrowed from unicode/utf8 and strconv and replicate behavior in
// that package, since we can't take a dependency on either.
internal static readonly @string lowerhex = "0123456789abcdef"u8;

internal static UntypedInt runeSelf => 0x80;

internal static UntypedInt runeError => /* '\uFFFD' */ 65533;

internal static @string quote(@string s) {
    var buf = new slice<byte>(1, len(s) + 2); // slice will be at least len(s) + quotes
    buf[0] = (rune)'"';
    foreach (var (i, c) in s) {
        if (c >= runeSelf || c < (rune)' '){
            // This means you are asking us to parse a time.Duration or
            // time.Location with unprintable or non-ASCII characters in it.
            // We don't expect to hit this case very often. We could try to
            // reproduce strconv.Quote's behavior with full fidelity but
            // given how rarely we expect to hit these edge cases, speed and
            // conciseness are better.
            nint width = default!;
            if (c == runeError){
                width = 1;
                if (i + 2 < len(s) && s.slice(i, i + 3) == ((@string)(rune)runeError)) {
                    width = 3;
                }
            } else {
                width = len(((@string)c));
            }
            for (nint j = 0; j < width; j++) {
                buf = append(buf, ((@string)@"\x"u8).ꓸꓸꓸ);
                buf = append(buf, lowerhex[(s[i + j] >> (int)(4))]);
                buf = append(buf, lowerhex[(byte)(s[i + j] & 0xF)]);
            }
        } else {
            if (c == (rune)'"' || c == (rune)'\\') {
                buf = append(buf, (byte)((rune)'\\'));
            }
            buf = append(buf, ((@string)c).ꓸꓸꓸ);
        }
    }
    buf = append(buf, (byte)((rune)'"'));
    return ((@string)buf);
}

// Error returns the string representation of a ParseError.
[GoRecv] public static @string Error(this ref ParseError e) {
    if (e.Message == ""u8) {
        return "parsing time "u8 + quote(e.Value) + " as "u8 + quote(e.Layout) + ": cannot parse "u8 + quote(e.ValueElem) + " as "u8 + quote(e.LayoutElem);
    }
    return "parsing time "u8 + quote(e.Value) + e.Message;
}

// isDigit reports whether s[i] is in range and is a decimal digit.
internal static bool isDigit<bytes>(bytes s, nint i)
    where bytes : /* []byte | string */ IByteSeq<bytes, byte>, new()
{
    if (len(s) <= i) {
        return false;
    }
    var c = s[i];
    return (rune)'0' <= c && c <= (rune)'9';
}

// getnum parses s[0:1] or s[0:2] (fixed forces s[0:2])
// as a decimal integer and returns the integer and the
// remainder of the string.
internal static (nint, @string, error) getnum(@string s, bool @fixed) {
    if (!isDigit(s, 0)) {
        return (0, s, errBad);
    }
    if (!isDigit(s, 1)) {
        if (@fixed) {
            return (0, s, errBad);
        }
        return ((nint)((byte)(s[0] - (rune)'0')), s[1..], default!);
    }
    return ((nint)((byte)(s[0] - (rune)'0')) * 10 + (nint)((byte)(s[1] - (rune)'0')), s[2..], default!);
}

// getnum3 parses s[0:1], s[0:2], or s[0:3] (fixed forces s[0:3])
// as a decimal integer and returns the integer and the remainder
// of the string.
internal static (nint, @string, error) getnum3(@string s, bool @fixed) {
    nint n = default!;
    nint i = default!;
    for (i = 0; i < 3 && isDigit(s, i); i++) {
        n = n * 10 + (nint)((byte)(s[i] - (rune)'0'));
    }
    if (i == 0 || @fixed && i != 3) {
        return (0, s, errBad);
    }
    return (n, s.slice(i), default!);
}

internal static @string cutspace(@string s) {
    while (len(s) > 0 && s[0] == (rune)' ') {
        s = s[1..];
    }
    return s;
}

// skip removes the given prefix from value,
// treating runs of space characters as equivalent.
internal static (@string, error) skip(@string value, @string prefix) {
    while (len(prefix) > 0) {
        if (prefix[0] == (rune)' ') {
            if (len(value) > 0 && value[0] != (rune)' ') {
                return (value, errBad);
            }
            prefix = cutspace(prefix);
            value = cutspace(value);
            continue;
        }
        if (len(value) == 0 || value[0] != prefix[0]) {
            return (value, errBad);
        }
        prefix = prefix[1..];
        value = value[1..];
    }
    return (value, default!);
}

// Parse parses a formatted string and returns the time value it represents.
// See the documentation for the constant called [Layout] to see how to
// represent the format. The second argument must be parseable using
// the format string (layout) provided as the first argument.
//
// The example for [Time.Format] demonstrates the working of the layout string
// in detail and is a good reference.
//
// When parsing (only), the input may contain a fractional second
// field immediately after the seconds field, even if the layout does not
// signify its presence. In that case either a comma or a decimal point
// followed by a maximal series of digits is parsed as a fractional second.
// Fractional seconds are truncated to nanosecond precision.
//
// Elements omitted from the layout are assumed to be zero or, when
// zero is impossible, one, so parsing "3:04pm" returns the time
// corresponding to Jan 1, year 0, 15:04:00 UTC (note that because the year is
// 0, this time is before the zero Time).
// Years must be in the range 0000..9999. The day of the week is checked
// for syntax but it is otherwise ignored.
//
// For layouts specifying the two-digit year 06, a value NN >= 69 will be treated
// as 19NN and a value NN < 69 will be treated as 20NN.
//
// The remainder of this comment describes the handling of time zones.
//
// In the absence of a time zone indicator, Parse returns a time in UTC.
//
// When parsing a time with a zone offset like -0700, if the offset corresponds
// to a time zone used by the current location ([Local]), then Parse uses that
// location and zone in the returned time. Otherwise it records the time as
// being in a fabricated location with time fixed at the given zone offset.
//
// When parsing a time with a zone abbreviation like MST, if the zone abbreviation
// has a defined offset in the current location, then that offset is used.
// The zone abbreviation "UTC" is recognized as UTC regardless of location.
// If the zone abbreviation is unknown, Parse records the time as being
// in a fabricated location with the given zone abbreviation and a zero offset.
// This choice means that such a time can be parsed and reformatted with the
// same layout losslessly, but the exact instant used in the representation will
// differ by the actual zone offset. To avoid such problems, prefer time layouts
// that use a numeric zone offset, or use [ParseInLocation].
public static (Time, error) Parse(@string layout, @string value) {
    // Optimize for RFC3339 as it accounts for over half of all representations.
    if (layout == RFC3339 || layout == RFC3339Nano) {
        {
            var (t, ok) = parseRFC3339(value, ΔLocal); if (ok) {
                return (t, default!);
            }
        }
    }
    return parse(layout, value, ΔUTC, ΔLocal);
}

// ParseInLocation is like Parse but differs in two important ways.
// First, in the absence of time zone information, Parse interprets a time as UTC;
// ParseInLocation interprets the time as in the given location.
// Second, when given a zone offset or abbreviation, Parse tries to match it
// against the Local location; ParseInLocation uses the given location.
public static (Time, error) ParseInLocation(@string layout, @string value, ж<ΔLocation> Ꮡloc) {
    // Optimize for RFC3339 as it accounts for over half of all representations.
    if (layout == RFC3339 || layout == RFC3339Nano) {
        {
            var (t, ok) = parseRFC3339(value, Ꮡloc); if (ok) {
                return (t, default!);
            }
        }
    }
    return parse(layout, value, Ꮡloc, Ꮡloc);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string monthˢ = "month"u8;
internal static readonly @string hourˢ = "hour"u8;
internal static readonly @string minuteˢ = "minute"u8;
internal static readonly @string secondˢ = "second"u8;
internal static readonly @string timeZoneOffsetHourˢ = "time zone offset hour"u8;
internal static readonly @string timeZoneOffsetMinuteˢ = "time zone offset minute"u8;
internal static readonly @string timeZoneOffsetSecondˢ = "time zone offset second"u8;
internal static readonly @string dayOfYearOutOfRangeˢ = ": day-of-year out of range"u8;
internal static readonly @string dayOfYearDoesNotMatchˢ = ": day-of-year does not match month"u8;
internal static readonly @string dayOfYearDoesNotMatchDayˢ = ": day-of-year does not match day"u8;
internal static readonly @string dayOutOfRangeˢ = ": day out of range"u8;

internal static (Time, error) parse(@string layout, @string value, ж<ΔLocation> ᏑdefaultLocation, ж<ΔLocation> Ꮡlocal) {
    ref var local = ref Ꮡlocal.DerefOrNull();

    @string alayout = layout;
    @string avalue = value;
    @string rangeErrString = ""u8; // set if a value is out of range
    var amSet = false; // do we need to subtract 12 from the hour for midnight?
    var pmSet = false; // do we need to add 12 to the hour?
    // Time being constructed.
    nint year = default!;
    
    nint month = -1;
    
    nint day = -1;
    
    nint yday = -1;
    
    nint hour = default!;
    
    nint min = default!;
    
    nint sec = default!;
    
    nint nsec = default!;
    
    ж<ΔLocation> z = default!;
    
    nint zoneOffset = -1;
    
    @string zoneName = default!;
    // Each iteration processes one std value.
    while (ᐧ) {
        error err = default!;
        var (prefix, std, suffix) = nextStdChunk(layout);
        @string stdstr = layout.slice(len(prefix), len(layout) - len(suffix));
        (value, err) = skip(value, prefix);
        if (err != default!) {
            return (new Time(nil), new ParseErrorжerror(newParseError(alayout, avalue, prefix, value, ""u8)));
        }
        if (std == 0) {
            if (len(value) != 0) {
                return (new Time(nil), new ParseErrorжerror(newParseError(alayout, avalue, ""u8, value, ": extra text: "u8 + quote(value))));
            }
            break;
        }
        layout = suffix;
        @string p = default!;
        @string hold = value;
        var exprᴛ1 = (nint)(std & (nint)stdMask);
        var matchᴛ1 = false;
        if (exprᴛ1 == stdYear) { matchᴛ1 = true;
            do {
                if (len(value) < 2) {
                    err = errBad;
                    break;
                }
                (p, value) = (value[0..2], value[2..]);
                (year, err) = atoi(p);
                if (err != default!) {
                    break;
                }
                if (year >= 69){
                    // Unix time starts Dec 31 1969 in some time zones
                    year += 1900;
                } else {
                    year += 2000;
                }
            } while (false);
        }
        else if (exprᴛ1 == stdLongYear) { matchᴛ1 = true;
            do {
                if (len(value) < 4 || !isDigit(value, 0)) {
                    err = errBad;
                    break;
                }
                (p, value) = (value[0..4], value[4..]);
                (year, err) = atoi(p);
            } while (false);
        }
        else if (exprᴛ1 == stdMonth) { matchᴛ1 = true;
            (month, value, err) = lookup(shortMonthNames, value);
            month++;
        }
        else if (exprᴛ1 == stdLongMonth) { matchᴛ1 = true;
            (month, value, err) = lookup(longMonthNames, value);
            month++;
        }
        else if (exprᴛ1 == stdNumMonth || exprᴛ1 == stdZeroMonth) { matchᴛ1 = true;
            (month, value, err) = getnum(value, std == stdZeroMonth);
            if (err == default! && (month <= 0 || 12 < month)) {
                rangeErrString = monthˢ;
            }
        }
        else if (exprᴛ1 == stdWeekDay) { matchᴛ1 = true;
            (_, value, err) = lookup(shortDayNames, // Ignore weekday except for error checking.
 value);
        }
        else if (exprᴛ1 == stdLongWeekDay) { matchᴛ1 = true;
            (_, value, err) = lookup(longDayNames, value);
        }
        else if (exprᴛ1 == stdDay || exprᴛ1 == stdUnderDay || exprᴛ1 == stdZeroDay) { matchᴛ1 = true;
            if (std == stdUnderDay && len(value) > 0 && value[0] == (rune)' ') {
                value = value[1..];
            }
            (day, value, err) = getnum(value, std == stdZeroDay);
        }
        else if (exprᴛ1 == stdUnderYearDay || exprᴛ1 == stdZeroYearDay) { matchᴛ1 = true;
            for (nint i = 0; i < 2; i++) {
                // Note that we allow any one- or two-digit day here.
                // The month, day, year combination is validated after we've completed parsing.
                if (std == stdUnderYearDay && len(value) > 0 && value[0] == (rune)' ') {
                    value = value[1..];
                }
            }
            (yday, value, err) = getnum3(value, std == stdZeroYearDay);
        }
        else if (exprᴛ1 == stdHour) { matchᴛ1 = true;
            (hour, value, err) = getnum(value, // Note that we allow any one-, two-, or three-digit year-day here.
 // The year-day, year combination is validated after we've completed parsing.
 false);
            if (hour < 0 || 24 <= hour) {
                rangeErrString = hourˢ;
            }
        }
        else if (exprᴛ1 == stdHour12 || exprᴛ1 == stdZeroHour12) { matchᴛ1 = true;
            (hour, value, err) = getnum(value, std == stdZeroHour12);
            if (hour < 0 || 12 < hour) {
                rangeErrString = hourˢ;
            }
        }
        else if (exprᴛ1 == stdMinute || exprᴛ1 == stdZeroMinute) { matchᴛ1 = true;
            (min, value, err) = getnum(value, std == stdZeroMinute);
            if (min < 0 || 60 <= min) {
                rangeErrString = minuteˢ;
            }
        }
        else if (exprᴛ1 == stdSecond || exprᴛ1 == stdZeroSecond) { matchᴛ1 = true;
            do {
                (sec, value, err) = getnum(value, std == stdZeroSecond);
                if (err != default!) {
                    break;
                }
                if (sec < 0 || 60 <= sec) {
                    rangeErrString = secondˢ;
                    break;
                }
                if (len(value) >= 2 && commaOrPeriod(value[0]) && isDigit(value, // Special case: do we have a fractional second but no
 // fractional second in the format?
 1)) {
                    (_, std, _) = nextStdChunk(layout);
                    std &= (nint)(stdMask);
                    if (std == stdFracSecond0 || std == stdFracSecond9) {
                        // Fractional second in the layout; proceed normally
                        break;
                    }
                    // No fractional second in the layout but we have one in the input.
                    nint n = 2;
                    for (; n < len(value) && isDigit(value, n); n++) {
                    }
                    (nsec, rangeErrString, err) = parseNanoseconds(value, n);
                    value = value.slice(n);
                }
            } while (false);
        }
        else if (exprᴛ1 == stdPM) { matchᴛ1 = true;
            do {
                if (len(value) < 2) {
                    err = errBad;
                    break;
                }
                (p, value) = (value[0..2], value[2..]);
                var exprᴛ2 = p;
                if (exprᴛ2 == "PM"u8) {
                    pmSet = true;
                }
                else if (exprᴛ2 == "AM"u8) {
                    amSet = true;
                }
                else { /* default: */
                    err = errBad;
                }

            } while (false);
        }
        else if (exprᴛ1 == stdpm) { matchᴛ1 = true;
            do {
                if (len(value) < 2) {
                    err = errBad;
                    break;
                }
                (p, value) = (value[0..2], value[2..]);
                var exprᴛ3 = p;
                if (exprᴛ3 == "pm"u8) {
                    pmSet = true;
                }
                else if (exprᴛ3 == "am"u8) {
                    amSet = true;
                }
                else { /* default: */
                    err = errBad;
                }

            } while (false);
        }
        else if (exprᴛ1 == stdISO8601TZ || exprᴛ1 == stdISO8601ShortTZ || exprᴛ1 == stdISO8601ColonTZ || exprᴛ1 == stdISO8601SecondsTZ || exprᴛ1 == stdISO8601ColonSecondsTZ) { matchᴛ1 = true;
            do {
                if (len(value) >= 1 && value[0] == (rune)'Z') {
                    value = value[1..];
                    z = ΔUTC;
                    break;
                }
                fallthrough = true;
            } while (false);
        }
        if (fallthrough || !matchᴛ1 && (exprᴛ1 == stdNumTZ || exprᴛ1 == stdNumShortTZ || exprᴛ1 == stdNumColonTZ || exprᴛ1 == stdNumSecondsTz || exprᴛ1 == stdNumColonSecondsTZ)) { matchᴛ1 = true;
            do {
                @string sign = default!;
                @string hourΔ2 = default!;
                @string minΔ2 = default!;
                @string seconds = default!;
                if (std == stdISO8601ColonTZ || std == stdNumColonTZ){
                    if (len(value) < 6) {
                        err = errBad;
                        break;
                    }
                    if (value[3] != (rune)':') {
                        err = errBad;
                        break;
                    }
                    (sign, hourΔ2, minΔ2, seconds, value) = (value[0..1], value[1..3], value[4..6], "00", value[6..]);
                } else 
                if (std == stdNumShortTZ || std == stdISO8601ShortTZ){
                    if (len(value) < 3) {
                        err = errBad;
                        break;
                    }
                    (sign, hourΔ2, minΔ2, seconds, value) = (value[0..1], value[1..3], "00", "00", value[3..]);
                } else 
                if (std == stdISO8601ColonSecondsTZ || std == stdNumColonSecondsTZ){
                    if (len(value) < 9) {
                        err = errBad;
                        break;
                    }
                    if (value[3] != (rune)':' || value[6] != (rune)':') {
                        err = errBad;
                        break;
                    }
                    (sign, hourΔ2, minΔ2, seconds, value) = (value[0..1], value[1..3], value[4..6], value[7..9], value[9..]);
                } else 
                if (std == stdISO8601SecondsTZ || std == stdNumSecondsTz){
                    if (len(value) < 7) {
                        err = errBad;
                        break;
                    }
                    (sign, hourΔ2, minΔ2, seconds, value) = (value[0..1], value[1..3], value[3..5], value[5..7], value[7..]);
                } else {
                    if (len(value) < 5) {
                        err = errBad;
                        break;
                    }
                    (sign, hourΔ2, minΔ2, seconds, value) = (value[0..1], value[1..3], value[3..5], "00", value[5..]);
                }
                nint hr = default!;
                nint mm = default!;
                nint ss = default!;
                (hr, _, err) = getnum(hourΔ2, true);
                if (err == default!) {
                    (mm, _, err) = getnum(minΔ2, true);
                    if (err == default!) {
                        (ss, _, err) = getnum(seconds, true);
                    }
                }
                if (hr > 24) {
                    // The range test use > rather than >=,
                    // as some people do write offsets of 24 hours
                    // or 60 minutes or 60 seconds.
                    rangeErrString = timeZoneOffsetHourˢ;
                }
                if (mm > 60) {
                    rangeErrString = timeZoneOffsetMinuteˢ;
                }
                if (ss > 60) {
                    rangeErrString = timeZoneOffsetSecondˢ;
                }
                zoneOffset = (hr * 60 + mm) * 60 + ss; // offset is in seconds
                switch (sign[0]) {
                case (rune)'+': {
                    break;
                }
                case (rune)'-': {
                    zoneOffset = -zoneOffset;
                    break;
                }
                default: {
                    err = errBad;
                    break;
                }}

            } while (false);
        }
        else if (exprᴛ1 == stdTZ) { matchᴛ1 = true;
            do {
                if (len(value) >= 3 && value[0..3] == "UTC") {
                    // Does it look like a time zone?
                    z = ΔUTC;
                    value = value[3..];
                    break;
                }
                var (n, ok) = parseTimeZone(value);
                if (!ok) {
                    err = errBad;
                    break;
                }
                (zoneName, value) = (value.slice(0, n), value.slice(n));
            } while (false);
        }
        else if (exprᴛ1 == stdFracSecond0) {
            do {
                nint ndigit = 1 + digitsLen(std);
                if (len(value) < ndigit) {
                    // stdFracSecond0 requires the exact number of digits as specified in
                    // the layout.
                    err = errBad;
                    break;
                }
                (nsec, rangeErrString, err) = parseNanoseconds(value, ndigit);
                value = value.slice(ndigit);
            } while (false);
        }
        else if (exprᴛ1 == stdFracSecond9) { matchᴛ1 = true;
            do {
                if (len(value) < 2 || !commaOrPeriod(value[0]) || value[1] < (rune)'0' || (rune)'9' < value[1]) {
                    // Fractional second omitted.
                    break;
                }
                nint i = 0;
                while (i + 1 < len(value) && (rune)'0' <= value[i + 1] && value[i + 1] <= (rune)'9') {
                    // Take any number of digits, even more than asked for,
                    // because it is what the stdSecond case would do.
                    i++;
                }
                (nsec, rangeErrString, err) = parseNanoseconds(value, 1 + i);
                value = value.slice(1 + i);
            } while (false);
        }

        if (rangeErrString != ""u8) {
            return (new Time(nil), new ParseErrorжerror(newParseError(alayout, avalue, stdstr, value, ": "u8 + rangeErrString + " out of range"u8)));
        }
        if (err != default!) {
            return (new Time(nil), new ParseErrorжerror(newParseError(alayout, avalue, stdstr, hold, ""u8)));
        }
    }
    if (pmSet && hour < 12){
        hour += 12;
    } else 
    if (amSet && hour == 12) {
        hour = 0;
    }
    // Convert yday to day, month.
    if (yday >= 0){
        nint d = default!;
        nint m = default!;
        if (isLeap(year)) {
            if (yday == 31 + 29){
                m = (nint)February;
                d = 29;
            } else 
            if (yday > 31 + 29) {
                yday--;
            }
        }
        if (yday < 1 || yday > 365) {
            return (new Time(nil), new ParseErrorжerror(newParseError(alayout, avalue, ""u8, value, dayOfYearOutOfRangeˢ)));
        }
        if (m == 0) {
            m = (yday - 1) / 31 + 1;
            if (daysBefore(((ΔMonth)(m + 1))) < yday) {
                m++;
            }
            d = yday - daysBefore(((ΔMonth)m));
        }
        // If month, day already seen, yday's m, d must match.
        // Otherwise, set them from m, d.
        if (month >= 0 && month != m) {
            return (new Time(nil), new ParseErrorжerror(newParseError(alayout, avalue, ""u8, value, dayOfYearDoesNotMatchˢ)));
        }
        month = m;
        if (day >= 0 && day != d) {
            return (new Time(nil), new ParseErrorжerror(newParseError(alayout, avalue, ""u8, value, dayOfYearDoesNotMatchDayˢ)));
        }
        day = d;
    } else {
        if (month < 0) {
            month = (nint)January;
        }
        if (day < 0) {
            day = 1;
        }
    }
    // Validate the day of the month.
    if (day < 1 || day > daysIn(((ΔMonth)month), year)) {
        return (new Time(nil), new ParseErrorжerror(newParseError(alayout, avalue, ""u8, value, dayOutOfRangeˢ)));
    }
    if (z != nil) {
        return (Date(year, ((ΔMonth)month), day, hour, min, sec, nsec, z), default!);
    }
    if (zoneOffset != -1) {
        var t = Date(year, ((ΔMonth)month), day, hour, min, sec, nsec, ΔUTC);
        t.addSec(-(int64)zoneOffset);
        // Look for local zone with the given offset.
        // If that zone was in effect at the given time, use it.
        var (name, offset, _, _, _) = Ꮡlocal.lookup(t.unixSec());
        if (offset == zoneOffset && (zoneName == ""u8 || name == zoneName)) {
            t.setLoc(Ꮡlocal);
            return (t, default!);
        }
        // Otherwise create fake zone to record offset.
        @string zoneNameCopy = stringslite.Clone(zoneName); // avoid leaking the input value
        t.setLoc(FixedZone(zoneNameCopy, zoneOffset));
        return (t, default!);
    }
    if (zoneName != ""u8) {
        var t = Date(year, ((ΔMonth)month), day, hour, min, sec, nsec, ΔUTC);
        // Look for local zone with the given offset.
        // If that zone was in effect at the given time, use it.
        var (offset, ok) = Ꮡlocal.lookupName(zoneName, t.unixSec());
        if (ok) {
            t.addSec(-(int64)offset);
            t.setLoc(Ꮡlocal);
            return (t, default!);
        }
        // Otherwise, create fake zone with unknown offset.
        if (len(zoneName) > 3 && zoneName[..3] == "GMT") {
            (offset, _) = atoi(zoneName[3..]); // Guaranteed OK by parseGMT.
            offset *= 3600;
        }
        @string zoneNameCopy = stringslite.Clone(zoneName); // avoid leaking the input value
        t.setLoc(FixedZone(zoneNameCopy, offset));
        return (t, default!);
    }
    // Otherwise, fall back to default.
    return (Date(year, ((ΔMonth)month), day, hour, min, sec, nsec, ᏑdefaultLocation), default!);
}

// parseTimeZone parses a time zone string and returns its length. Time zones
// are human-generated and unpredictable. We can't do precise error checking.
// On the other hand, for a correct parse there must be a time zone at the
// beginning of the string, so it's almost always true that there's one
// there. We look at the beginning of the string for a run of upper-case letters.
// If there are more than 5, it's an error.
// If there are 4 or 5 and the last is a T, it's a time zone.
// If there are 3, it's a time zone.
// Otherwise, other than special cases, it's not a time zone.
// GMT is special because it can have an hour offset.
internal static (nint length, bool ok) parseTimeZone(@string value) {
    nint length = default!;

    if (len(value) < 3) {
        return (0, false);
    }
    // Special case 1: ChST and MeST are the only zones with a lower-case letter.
    if (len(value) >= 4 && (value[..4] == "ChST" || value[..4] == "MeST")) {
        return (4, true);
    }
    // Special case 2: GMT may have an hour offset; treat it specially.
    if (value[..3] == "GMT") {
        length = parseGMT(value);
        return (length, true);
    }
    // Special Case 3: Some time zones are not named, but have +/-00 format
    if (value[0] == (rune)'+' || value[0] == (rune)'-') {
        length = parseSignedOffset(value);
        var okΔ1 = length > 0; // parseSignedOffset returns 0 in case of bad input
        return (length, okΔ1);
    }
    // How many upper-case letters are there? Need at least three, at most five.
    nint nUpper = default!;
    for (nUpper = 0; nUpper < 6; nUpper++) {
        if (nUpper >= len(value)) {
            break;
        }
        {
            var c = value[nUpper]; if (c < (rune)'A' || (rune)'Z' < c) {
                break;
            }
        }
    }
    switch (nUpper) {
    case 0 or 1 or 2 or 6: {
        return (0, false);
    }
    case 5: {
        if (value[4] == (rune)'T') {
            // Must end in T to match.
            return (5, true);
        }
        break;
    }
    case 4: {
        if (value[3] == (rune)'T' || value[..4] == "WITA") {
            // Must end in T, except one special case.
            return (4, true);
        }
        break;
    }
    case 3: {
        return (3, true);
    }}

    return (0, false);
}

// parseGMT parses a GMT time zone. The input string is known to start "GMT".
// The function checks whether that is followed by a sign and a number in the
// range -23 through +23 excluding zero.
internal static nint parseGMT(@string value) {
    value = value[3..];
    if (len(value) == 0) {
        return 3;
    }
    return 3 + parseSignedOffset(value);
}

// parseSignedOffset parses a signed timezone offset (e.g. "+03" or "-04").
// The function checks for a signed number in the range -23 through +23 excluding zero.
// Returns length of the found offset string or 0 otherwise.
internal static nint parseSignedOffset(@string value) {
    var sign = value[0];
    if (sign != (rune)'-' && sign != (rune)'+') {
        return 0;
    }
    var (x, rem, err) = leadingInt(value[1..]);
    // fail if nothing consumed by leadingInt
    if (err != default! || value[1..] == rem) {
        return 0;
    }
    if (x > 23) {
        return 0;
    }
    return len(value) - len(rem);
}

internal static bool commaOrPeriod(byte b) {
    return b == (rune)'.' || b == (rune)',';
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fractionalSecondˢ = "fractional second"u8;

internal static (nint ns, @string rangeErrString, error err) parseNanoseconds<bytes>(bytes value, nint nbytes)
    where bytes : /* []byte | string */ IByteSeq<bytes, byte>, new()
{
    nint ns = default!;
    @string rangeErrString = default!;
    error err = default!;

    if (!commaOrPeriod(value[0])) {
        err = errBad;
        return (ns, rangeErrString, err);
    }
    if (nbytes > 10) {
        value = value[..10];
        nbytes = 10;
    }
    {
        (ns, err) = atoi(value.slice(1, nbytes)); if (err != default!) {
            return (ns, rangeErrString, err);
        }
    }
    if (ns < 0) {
        rangeErrString = fractionalSecondˢ;
        return (ns, rangeErrString, err);
    }
    // We need nanoseconds, which means scaling by the number
    // of missing digits in the format, maximum length 10.
    nint scaleDigits = 10 - nbytes;
    for (nint i = 0; i < scaleDigits; i++) {
        ns *= 10;
    }
    return (ns, rangeErrString, err);
}

internal static error errLeadingInt = errors.New("time: bad [0-9]*"u8); // never printed

// leadingInt consumes the leading [0-9]* from s.
internal static (uint64 x, bytes rem, error err) leadingInt<bytes>(bytes s)
    where bytes : /* []byte | string */ IByteSeq<bytes, byte>, new()
{
    uint64 x = default!;
    bytes rem = GoZero<bytes>();

    nint i = 0;
    for (; i < len(s); i++) {
        var c = s[i];
        if (c < (rune)'0' || c > (rune)'9') {
            break;
        }
        if (x > 922337203685477580UL) {
            // overflow
            return (0, rem, errLeadingInt);
        }
        x = x * 10 + (uint64)c - (rune)'0';
        if (x > ((uint64)1 << (int)(63))) {
            // overflow
            return (0, rem, errLeadingInt);
        }
    }
    return (x, s.slice(i), default!);
}

// leadingFraction consumes the leading [0-9]* from s.
// It is used only for fractions, so does not return an error on overflow,
// it just stops accumulating precision.
internal static (uint64 x, float64 scale, @string rem) leadingFraction(@string s) {
    uint64 x = default!;
    float64 scale = default!;

    nint i = 0;
    scale = 1D;
    var overflow = false;
    for (; i < len(s); i++) {
        var c = s[i];
        if (c < (rune)'0' || c > (rune)'9') {
            break;
        }
        if (overflow) {
            continue;
        }
        if (x > 922337203685477580UL) {
            // It's possible for overflow to give a positive number, so take care.
            overflow = true;
            continue;
        }
        var y = x * 10 + (uint64)c - (rune)'0';
        if (y > ((uint64)1 << (int)(63))) {
            overflow = true;
            continue;
        }
        x = y;
        scale *= 10D;
    }
    return (x, scale, s.slice(i));
}

// U+00B5 = micro symbol
// U+03BC = Greek letter mu
internal static map<@string, uint64> unitMap = new map<@string, uint64>{
    ["ns"u8] = (uint64)(int64)ΔNanosecond,
    ["us"u8] = (uint64)(int64)Microsecond,
    ["µs"u8] = (uint64)(int64)Microsecond,
    ["μs"u8] = (uint64)(int64)Microsecond,
    ["ms"u8] = (uint64)(int64)Millisecond,
    ["s"u8] = (uint64)(int64)ΔSecond,
    ["m"u8] = (uint64)(int64)ΔMinute,
    ["h"u8] = (uint64)(int64)ΔHour
};

// ParseDuration parses a duration string.
// A duration string is a possibly signed sequence of
// decimal numbers, each with optional fraction and a unit suffix,
// such as "300ms", "-1.5h" or "2h45m".
// Valid time units are "ns", "us" (or "µs"), "ms", "s", "m", "h".
public static (Duration, error) ParseDuration(@string s) {
    // [-+]?([0-9]*(\.[0-9]*)?[a-z]+)+
    @string orig = s;
    uint64 d = default!;
    var neg = false;
    // Consume [-+]?
    if (s != ""u8) {
        var c = s[0];
        if (c == (rune)'-' || c == (rune)'+') {
            neg = c == (rune)'-';
            s = s[1..];
        }
    }
    // Special case: if all that is left is "0", this is zero.
    if (s == "0"u8) {
        return (0, default!);
    }
    if (s == ""u8) {
        return (0, errors.New("time: invalid duration "u8 + quote(orig)));
    }
    while (s != ""u8) {
        uint64 v = default!;                      // integers before, after decimal point
        uint64 f = default!;
        float64 scale = 1D;   // value = v + f/scale
        error err = default!;
        // The next character must be [0-9.]
        if (!(s[0] == (rune)'.' || (rune)'0' <= s[0] && s[0] <= (rune)'9')) {
            return (0, errors.New("time: invalid duration "u8 + quote(orig)));
        }
        // Consume [0-9]*
        nint pl = len(s);
        (v, s, err) = leadingInt(s);
        if (err != default!) {
            return (0, errors.New("time: invalid duration "u8 + quote(orig)));
        }
        var pre = pl != len(s); // whether we consumed anything before a period
        // Consume (\.[0-9]*)?
        var post = false;
        if (s != ""u8 && s[0] == (rune)'.') {
            s = s[1..];
            nint plΔ1 = len(s);
            (f, scale, s) = leadingFraction(s);
            post = plΔ1 != len(s);
        }
        if (!pre && !post) {
            // no digits (e.g. ".s" or "-.s")
            return (0, errors.New("time: invalid duration "u8 + quote(orig)));
        }
        // Consume unit.
        nint i = 0;
        for (; i < len(s); i++) {
            var c = s[i];
            if (c == (rune)'.' || (rune)'0' <= c && c <= (rune)'9') {
                break;
            }
        }
        if (i == 0) {
            return (0, errors.New("time: missing unit in duration "u8 + quote(orig)));
        }
        @string u = s.slice(0, i);
        s = s.slice(i);
        var (unit, ok) = unitMap[u, ꟷ];
        if (!ok) {
            return (0, errors.New("time: unknown unit "u8 + quote(u) + " in duration "u8 + quote(orig)));
        }
        if (v > ((uint64)1 << (int)(63)) / unit) {
            // overflow
            return (0, errors.New("time: invalid duration "u8 + quote(orig)));
        }
        v *= unit;
        if (f > 0) {
            // float64 is needed to be nanosecond accurate for fractions of hours.
            // v >= 0 && (f*unit/scale) <= 3.6e+12 (ns/h, h is the largest unit)
            v += (uint64)((float64)f * ((float64)unit / scale));
            if (v > ((uint64)1 << (int)(63))) {
                // overflow
                return (0, errors.New("time: invalid duration "u8 + quote(orig)));
            }
        }
        d += v;
        if (d > ((uint64)1 << (int)(63))) {
            return (0, errors.New("time: invalid duration "u8 + quote(orig)));
        }
    }
    if (neg) {
        return (-((Duration)(int64)d), default!);
    }
    if (d > 9223372036854775807UL) {
        return (0, errors.New("time: invalid duration "u8 + quote(orig)));
    }
    return (((Duration)(int64)d), default!);
}

} // end time_package
