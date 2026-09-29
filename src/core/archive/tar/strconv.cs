// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using bytes = bytes_package;
using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;

partial class tar_package {

// hasNUL reports whether the NUL character exists within s.
internal static bool hasNUL(@string s) {
    return strings.Contains(s, "\x00"u8);
}

// isASCII reports whether the input is an ASCII C-style string.
internal static bool isASCII(@string s) {
    foreach (var (_, c) in s) {
        if (c >= 0x80 || c == 0x00) {
            return false;
        }
    }
    return true;
}

// toASCII converts the input to an ASCII C-style string.
// This is a best effort conversion, so invalid characters are dropped.
internal static @string toASCII(@string s) {
    if (isASCII(s)) {
        return s;
    }
    var b = new slice<byte>(0, len(s));
    foreach (var (_, c) in s) {
        if (c < 0x80 && c != 0x00) {
            b = append(b, (byte)c);
        }
    }
    return ((@string)b);
}

[GoType] partial struct parser {
    internal error err; // Last error seen
}

[GoType] partial struct formatter {
    internal error err; // Last error seen
}

// parseString parses bytes as a NUL-terminated C-style string.
// If a NUL byte is not found then the whole slice is returned as a string.
[GoRecv] internal static @string parseString(this ref parser _, slice<byte> b) {
    {
        nint i = bytes.IndexByte(b, 0); if (i >= 0) {
            return ((@string)(b.slice(0, i)));
        }
    }
    return ((@string)b);
}

// formatString copies s into b, NUL-terminating if possible.
[GoRecv] internal static void formatString(this ref formatter f, slice<byte> b, @string s) {
    if (len(s) > len(b)) {
        f.err = ErrFieldTooLong;
    }
    copy(b, s);
    if (len(s) < len(b)) {
        b[len(s)] = 0;
    }
    // Some buggy readers treat regular files with a trailing slash
    // in the V7 path field as a directory even though the full path
    // recorded elsewhere (e.g., via PAX record) contains no trailing slash.
    if (len(s) > len(b) && b[len(b) - 1] == (rune)'/') {
        nint n = len(strings.TrimRight(s.slice(0, len(b) - 1), "/"u8));
        b[n] = 0; // Replace trailing slash with NUL terminator
    }
}

// fitsInBase256 reports whether x can be encoded into n bytes using base-256
// encoding. Unlike octal encoding, base-256 encoding does not require that the
// string ends with a NUL character. Thus, all n bytes are available for output.
//
// If operating in binary mode, this assumes strict GNU binary mode; which means
// that the first byte can only be either 0x80 or 0xff. Thus, the first byte is
// equivalent to the sign bit in two's complement form.
internal static bool fitsInBase256(nint n, int64 x) {
    nuint binBits = (nuint)(n - 1) * 8;
    return n >= 9 || (x >= ((int64)(-1)).Lsh(binBits) && x < ((int64)1).Lsh(binBits));
}

// parseNumeric parses the input as being encoded in either base-256 or octal.
// This function may return negative numbers.
// If parsing fails or an integer overflow occurs, err will be set.
[GoRecv] internal static int64 parseNumeric(this ref parser p, slice<byte> b) {
    // Check for base-256 (binary) format first.
    // If the first bit is set, then all following bits constitute a two's
    // complement encoded number in big-endian byte order.
    if (len(b) > 0 && (byte)(b[0] & 0x80) != 0) {
        // Handling negative numbers relies on the following identity:
        //	-a-1 == ^a
        //
        // If the number is negative, we use an inversion mask to invert the
        // data bytes and treat the value as an unsigned number.
        byte inv = default!;       // 0x00 if positive or zero, 0xff if negative
        if ((byte)(b[0] & 0x40) != 0) {
            inv = 0xff;
        }
        uint64 x = default!;
        foreach (var (i, vᴛ1) in b) {
            var c = vᴛ1;

            c ^= (byte)(inv); // Inverts c only if inv is 0xff, otherwise does nothing
            if (i == 0) {
                c &= (byte)(0x7f); // Ignore signal bit in first byte
            }
            if (((x >> (int)(56))) > 0) {
                p.err = ErrHeader; // Integer overflow
                return 0;
            }
            x = (uint64)((x << (int)(8)) | (uint64)c);
        }
        if (((x >> (int)(63))) > 0) {
            p.err = ErrHeader; // Integer overflow
            return 0;
        }
        if (inv == 0xff) {
            return ~(int64)x;
        }
        return (int64)x;
    }
    // Normal case is base-8 (octal) format.
    return p.parseOctal(b);
}

// formatNumeric encodes x into b using base-8 (octal) encoding if possible.
// Otherwise it will attempt to use base-256 (binary) encoding.
[GoRecv] internal static void formatNumeric(this ref formatter f, slice<byte> b, int64 x) {
    if (fitsInOctal(len(b), x)) {
        f.formatOctal(b, x);
        return;
    }
    if (fitsInBase256(len(b), x)) {
        for (nint i = len(b) - 1; i >= 0; i--) {
            b[i] = (byte)x;
            x >>= (int)(8);
        }
        b[0] |= (byte)(0x80); // Highest bit indicates binary format
        return;
    }
    f.formatOctal(b, 0); // Last resort, just write zero
    f.err = ErrFieldTooLong;
}

[GoRecv] internal static int64 parseOctal(this ref parser p, slice<byte> b) {
    // Because unused fields are filled with NULs, we need
    // to skip leading NULs. Fields may also be padded with
    // spaces or NULs.
    // So we remove leading and trailing NULs and spaces to
    // be sure.
    b = bytes.Trim(b, " \x00"u8);
    if (len(b) == 0) {
        return 0;
    }
    var (x, perr) = strconv.ParseUint(p.parseString(b), 8, 64);
    if (perr != default!) {
        p.err = ErrHeader;
    }
    return (int64)x;
}

[GoRecv] internal static void formatOctal(this ref formatter f, slice<byte> b, int64 x) {
    if (!fitsInOctal(len(b), x)) {
        x = 0; // Last resort, just write zero
        f.err = ErrFieldTooLong;
    }
    @string s = strconv.FormatInt(x, 8);
    // Add leading zeros, but leave room for a NUL.
    {
        nint n = len(b) - len(s) - 1; if (n > 0) {
            s = strings.Repeat("0"u8, n) + s;
        }
    }
    f.formatString(b, s);
}

// fitsInOctal reports whether the integer x fits in a field n-bytes long
// using octal encoding with the appropriate NUL terminator.
internal static bool fitsInOctal(nint n, int64 x) {
    nuint octBits = (nuint)(n - 1) * 3;
    return x >= 0 && (n >= 22 || x < ((int64)1).Lsh(octBits));
}

// parsePAXTime takes a string of the form %d.%d as described in the PAX
// specification. Note that this implementation allows for negative timestamps,
// which is allowed for by the PAX specification, but not always portable.
internal static (time.Time, error) parsePAXTime(@string s) {
    const nint maxNanoSecondDigits = 9;
    // Split string into seconds and sub-seconds parts.
    var (ss, sn, _) = strings.Cut(s, "."u8);
    // Parse the seconds.
    var (secs, err) = strconv.ParseInt(ss, 10, 64);
    if (err != default!) {
        return (new time.Time(nil), ErrHeader);
    }
    if (len(sn) == 0) {
        return (time.Unix(secs, 0), default!); // No sub-second values
    }
    // Parse the nanoseconds.
    if (strings.Trim(sn, "0123456789"u8) != ""u8) {
        return (new time.Time(nil), ErrHeader);
    }
    if (len(sn) < maxNanoSecondDigits){
        sn += strings.Repeat("0"u8, maxNanoSecondDigits - len(sn)); // Right pad
    } else {
        sn = sn[..(int)(maxNanoSecondDigits)]; // Right truncate
    }
    var (nsecs, _) = strconv.ParseInt(sn, 10, 64); // Must succeed
    if (len(ss) > 0 && ss[0] == (rune)'-') {
        return (time.Unix(secs, -1 * nsecs), default!); // Negative correction
    }
    return (time.Unix(secs, nsecs), default!);
}

// formatPAXTime converts ts into a time of the form %d.%d as described in the
// PAX specification. This function is capable of negative timestamps.
internal static @string /*s*/ formatPAXTime(time.Time ts) {
    var secs = ts.Unix();
    nint nsecs = ts.Nanosecond();
    if (nsecs == 0) {
        return strconv.FormatInt(secs, 10);
    }
    // If seconds is negative, then perform correction.
    @string sign = ""u8;
    if (secs < 0) {
        sign = "-"u8; // Remember sign
        secs = -(secs + 1); // Add a second to secs
        nsecs = -(nsecs - 1000000000); // Take that second away from nsecs
    }
    return strings.TrimRight(fmt.Sprintf("%s%d.%09d"u8, sign, secs, nsecs), "0"u8);
}

// parsePAXRecord parses the input PAX record string into a key-value pair.
// If parsing is successful, it will slice off the currently read record and
// return the remainder as r.
internal static (@string k, @string v, @string r, error err) parsePAXRecord(@string s) {
    @string k = default!;
    @string v = default!;

    // The size field ends at the first space.
    var (nStr, rest, ok) = strings.Cut(s, " "u8);
    if (!ok) {
        return ("", "", s, ErrHeader);
    }
    // Parse the first token as a decimal integer.
    var (n, perr) = strconv.ParseInt(nStr, 10, 0); // Intentionally parse as native int
    if (perr != default! || n < 5 || n > (int64)len(s)) {
        return ("", "", s, ErrHeader);
    }
    n -= (int64)(len(nStr) + 1); // convert from index in s to index in rest
    if (n <= 0) {
        return ("", "", s, ErrHeader);
    }
    // Extract everything between the space and the final newline.
    @string rec = rest.slice(0, (nint)(n - 1));
    @string nl = rest.slice((nint)(n - 1), (nint)(n));
    @string rem = rest.slice((nint)(n));
    if (nl != "\n"u8) {
        return ("", "", s, ErrHeader);
    }
    // The first equals separates the key from the value.
    (k, v, ok) = strings.Cut(rec, "="u8);
    if (!ok) {
        return ("", "", s, ErrHeader);
    }
    if (!validPAXRecord(k, v)) {
        return ("", "", s, ErrHeader);
    }
    return (k, v, rem, default!);
}

// formatPAXRecord formats a single PAX record, prefixing it with the
// appropriate length.
internal static (@string, error) formatPAXRecord(@string k, @string v) {
    if (!validPAXRecord(k, v)) {
        return ("", ErrHeader);
    }
    const nint padding = 3; // Extra padding for ' ', '=', and '\n'
    nint size = len(k) + len(v) + padding;
    size += len(strconv.Itoa(size));
    @string record = strconv.Itoa(size) + " "u8 + k + "="u8 + v + "\n"u8;
    // Final adjustment if adding size field increased the record size.
    if (len(record) != size) {
        size = len(record);
        record = strconv.Itoa(size) + " "u8 + k + "="u8 + v + "\n"u8;
    }
    return (record, default!);
}

// validPAXRecord reports whether the key-value pair is valid where each
// record is formatted as:
//
//	"%d %s=%s\n" % (size, key, value)
//
// Keys and values should be UTF-8, but the number of bad writers out there
// forces us to be a more liberal.
// Thus, we only reject all keys with NUL, and only reject NULs in values
// for the PAX version of the USTAR string fields.
// The key must not contain an '=' character.
internal static bool validPAXRecord(@string k, @string v) {
    if (k == ""u8 || strings.Contains(k, "="u8)) {
        return false;
    }
    var exprᴛ1 = k;
    if (exprᴛ1 == paxPath || exprᴛ1 == paxLinkpath || exprᴛ1 == paxUname || exprᴛ1 == paxGname) {
        return !hasNUL(v);
    }
    { /* default: */
        return !hasNUL(k);
    }

}

} // end tar_package
