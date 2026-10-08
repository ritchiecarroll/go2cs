// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using strconv = strconv_package;
using utf8 = unicode.utf8_package;
using unicode;

partial class fmt_package {

internal static readonly @string ldigits = "0123456789abcdefx"u8;
internal static readonly @string udigits = "0123456789ABCDEFX"u8;

internal const bool signed = true;
internal const bool unsigned = false;

// flags placed in a separate struct for easy clearing.
partial struct fmtFlags {
    internal bool widPresent;
    internal bool precPresent;
    internal bool minus;
    internal bool plus;
    internal bool sharp;
    internal bool space;
    internal bool zero;
    // For the formats %+v %#v, we set the plusV/sharpV flags
    // and clear the plus/sharp flags since %+v and %#v are in effect
    // different, flagless formats set at the top level.
    internal bool plusV;
    internal bool sharpV;
}

// A fmt is the raw formatter used by Printf etc.
// It prints into a buffer that must be set up separately.
partial struct fmt {
    internal ж<buffer> buf;
    internal partial ref fmtFlags fmtFlags { get; }
    internal nint wid; // width
    internal nint prec; // precision
    // intbuf is large enough to store %b of an int64 with a sign and
    // avoids padding at the end of the struct on 32 bit architectures.
    internal array<byte> intbuf = new(68);
}

internal static void clearflags(this ref fmt f) {
    f.fmtFlags = new fmtFlags(nil);
    f.wid = 0;
    f.prec = 0;
}

internal static void init(this ref fmt f, ж<buffer> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    f.buf = Ꮡbuf;
    f.clearflags();
}

// writePadding generates n bytes of padding.
internal static void writePadding(this ref fmt f, nint n) {
    if (n <= 0) {
        // No padding bytes needed.
        return;
    }
    var buf = f.buf.ValueSlot;
    nint oldLen = len(buf);
    nint newLen = oldLen + n;
    // Make enough room for padding.
    if (newLen > cap(buf)) {
        buf = new buffer(cap(buf) * 2 + n);
        copy(buf, f.buf.ValueSlot);
    }
    // Decide which byte the padding should be filled with.
    var padByte = (byte)(rune)' ';
    // Zero padding is allowed only to the left.
    if (f.zero && !f.minus) {
        padByte = (byte)(rune)'0';
    }
    // Fill padding with padByte.
    var padding = buf.slice(oldLen, newLen);
    foreach (var (i, _) in padding) {
        padding[i] = padByte;
    }
    f.buf.ValueSlot = buf.slice(0, newLen);
}

// pad appends b to f.buf, padded on left (!f.minus) or right (f.minus).
internal static void pad(this ref fmt f, slice<byte> b) {
    if (!f.widPresent || f.wid == 0) {
        f.buf.write(b);
        return;
    }
    nint width = f.wid - utf8.RuneCount(b);
    if (!f.minus){
        // left padding
        f.writePadding(width);
        f.buf.write(b);
    } else {
        // right padding
        f.buf.write(b);
        f.writePadding(width);
    }
}

// padString appends s to f.buf, padded on left (!f.minus) or right (f.minus).
internal static void padString(this ref fmt f, sstring s) {
    if (!f.widPresent || f.wid == 0) {
        f.buf.writeString(s);
        return;
    }
    nint width = f.wid - utf8.RuneCountInString(s);
    if (!f.minus){
        // left padding
        f.writePadding(width);
        f.buf.writeString(s);
    } else {
        // right padding
        f.buf.writeString(s);
        f.writePadding(width);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string trueˢ = "true"u8;
internal static readonly @string falseˢ = "false"u8;

// fmtBoolean formats a boolean.
internal static void fmtBoolean(this ref fmt f, bool v) {
    if (v){
        f.padString(trueˢ);
    } else {
        f.padString(falseˢ);
    }
}

// fmtUnicode formats a uint64 as "U+0078" or with f.sharp set as "U+0078 'x'".
internal static void fmtUnicode(this ref fmt f, uint64 u) {
    var buf = f.intbuf[0..];
    // With default precision set the maximum needed buf length is 18
    // for formatting -1 with %#U ("U+FFFFFFFFFFFFFFFF") which fits
    // into the already allocated intbuf with a capacity of 68 bytes.
    nint prec = 4;
    if (f.precPresent && f.prec > 4) {
        prec = f.prec;
        // Compute space needed for "U+" , number, " '", character, "'".
        nint width = 2 + prec + 2 + (nint)utf8.UTFMax + 1;
        if (width > len(buf)) {
            buf = new slice<byte>(width);
        }
    }
    // Format into buf, ending at buf[i]. Formatting numbers is easier right-to-left.
    nint i = len(buf);
    // For %#U we want to add a space and a quoted character at the end of the buffer.
    if (f.sharp && u <= utf8.MaxRune && strconv.IsPrint((rune)u)) {
        i--;
        buf[i] = (rune)'\'';
        i -= utf8.RuneLen((rune)u);
        utf8.EncodeRune(buf.slice(i), (rune)u);
        i--;
        buf[i] = (rune)'\'';
        i--;
        buf[i] = (rune)' ';
    }
    // Format the Unicode code point u as a hexadecimal number.
    while (u >= 16) {
        i--;
        buf[i] = udigits[(uint64)(u & 0xF)];
        prec--;
        u >>= (int)(4);
    }
    i--;
    buf[i] = udigits[u];
    prec--;
    // Add zeros in front of the number until requested precision is reached.
    while (prec > 0) {
        i--;
        buf[i] = (rune)'0';
        prec--;
    }
    // Add a leading "U+".
    i--;
    buf[i] = (rune)'+';
    i--;
    buf[i] = (rune)'U';
    var oldZero = f.zero;
    f.zero = false;
    f.pad(buf.slice(i));
    f.zero = oldZero;
}

// fmtInteger formats signed and unsigned integers.
internal static void fmtInteger(this ref fmt f, uint64 u, nint @base, bool isSigned, rune verb, sstring digits) {
    var negative = isSigned && (int64)u < 0;
    if (negative) {
        u = ((uint64)0 - u);
    }
    var buf = f.intbuf[0..];
    // The already allocated f.intbuf with a capacity of 68 bytes
    // is large enough for integer formatting when no precision or width is set.
    if (f.widPresent || f.precPresent) {
        // Account 3 extra bytes for possible addition of a sign and "0x".
        nint width = 3 + f.wid + f.prec; // wid and prec are always positive.
        if (width > len(buf)) {
            // We're going to need a bigger boat.
            buf = new slice<byte>(width);
        }
    }
    // Two ways to ask for extra leading zero digits: %.3d or %03d.
    // If both are specified the f.zero flag is ignored and
    // padding with spaces is used instead.
    nint prec = 0;
    if (f.precPresent){
        prec = f.prec;
        // Precision of 0 and value of 0 means "print nothing" but padding.
        if (prec == 0 && u == 0) {
            var oldZeroΔ1 = f.zero;
            f.zero = false;
            f.writePadding(f.wid);
            f.zero = oldZeroΔ1;
            return;
        }
    } else 
    if (f.zero && !f.minus && f.widPresent) {
        // Zero padding is allowed only to the left.
        prec = f.wid;
        if (negative || f.plus || f.space) {
            prec--; // leave room for sign
        }
    }
    // Because printing is easier right-to-left: format u into buf, ending at buf[i].
    // We could make things marginally faster by splitting the 32-bit case out
    // into a separate block but it's not worth the duplication, so u has 64 bits.
    nint i = len(buf);
    // Use constants for the division and modulo for more efficient code.
    // Switch cases ordered by popularity.
    switch (@base) {
    case 10: {
        while (u >= 10) {
            i--;
            var next = u / 10;
            buf[i] = (byte)((rune)'0' + u - next * 10);
            u = next;
        }
        break;
    }
    case 16: {
        while (u >= 16) {
            i--;
            buf[i] = digits[(uint64)(u & 0xF)];
            u >>= (int)(4);
        }
        break;
    }
    case 8: {
        while (u >= 8) {
            i--;
            buf[i] = (byte)((rune)'0' + (uint64)(u & 7));
            u >>= (int)(3);
        }
        break;
    }
    case 2: {
        while (u >= 2) {
            i--;
            buf[i] = (byte)((rune)'0' + (uint64)(u & 1));
            u >>= (int)(1);
        }
        break;
    }
    default: {
        throw panic("fmt: unknown base; can't happen");
        break;
    }}

    i--;
    buf[i] = digits[u];
    while (i > 0 && prec > len(buf) - i) {
        i--;
        buf[i] = (rune)'0';
    }
    // Various prefixes: 0x, -, etc.
    if (f.sharp) {
        switch (@base) {
        case 2: {
            i--;
            buf[i] = (rune)'b';
            i--;
            buf[i] = (rune)'0';
            break;
        }
        case 8: {
            if (buf[i] != (rune)'0') {
                // Add a leading 0b.
                i--;
                buf[i] = (rune)'0';
            }
            break;
        }
        case 16: {
            i--;
            buf[i] = digits[16];
            i--;
            buf[i] = (rune)'0';
            break;
        }}

    }
    // Add a leading 0x or 0X.
    if (verb == (rune)'O') {
        i--;
        buf[i] = (rune)'o';
        i--;
        buf[i] = (rune)'0';
    }
    if (negative){
        i--;
        buf[i] = (rune)'-';
    } else 
    if (f.plus){
        i--;
        buf[i] = (rune)'+';
    } else 
    if (f.space) {
        i--;
        buf[i] = (rune)' ';
    }
    // Left padding with zeros has already been handled like precision earlier
    // or the f.zero flag is ignored due to an explicitly set precision.
    var oldZero = f.zero;
    f.zero = false;
    f.pad(buf.slice(i));
    f.zero = oldZero;
}

// truncateString truncates the string s to the specified precision, if present.
internal static @string truncateString(this ref fmt f, @string s) {
    if (f.precPresent) {
        nint n = f.prec;
        foreach (var (i, _) in s) {
            n--;
            if (n < 0) {
                return s.slice(0, i);
            }
        }
    }
    return s;
}

// truncate truncates the byte slice b as a string of the specified precision, if present.
internal static slice<byte> truncate(this ref fmt f, slice<byte> b) {
    if (f.precPresent) {
        nint n = f.prec;
        for (nint i = 0; i < len(b); ) {
            n--;
            if (n < 0) {
                return b.slice(0, i);
            }
            nint wid = 1;
            if (b[i] >= utf8.RuneSelf) {
                (_, wid) = utf8.DecodeRune(b.slice(i));
            }
            i += wid;
        }
    }
    return b;
}

// fmtS formats a string.
internal static void fmtS(this ref fmt f, @string s) {
    s = f.truncateString(s);
    f.padString(s);
}

// fmtBs formats the byte slice b as if it was formatted as string with fmtS.
internal static void fmtBs(this ref fmt f, slice<byte> b) {
    b = f.truncate(b);
    f.pad(b);
}

// fmtSbx formats a string or byte slice as a hexadecimal encoding of its bytes.
internal static void fmtSbx(this ref fmt f, sstring s, slice<byte> b, sstring digits) {
    nint length = len(b);
    if (b == default!) {
        // No byte slice present. Assume string s should be encoded.
        length = len(s);
    }
    // Set length to not process more bytes than the precision demands.
    if (f.precPresent && f.prec < length) {
        length = f.prec;
    }
    // Compute width of the encoding taking into account the f.sharp and f.space flag.
    nint width = 2 * length;
    if (width > 0){
        if (f.space){
            // Each element encoded by two hexadecimals will get a leading 0x or 0X.
            if (f.sharp) {
                width *= 2;
            }
            // Elements will be separated by a space.
            width += length - 1;
        } else 
        if (f.sharp) {
            // Only a leading 0x or 0X will be added for the whole string.
            width += 2;
        }
    } else {
        // The byte slice or string that should be encoded is empty.
        if (f.widPresent) {
            f.writePadding(f.wid);
        }
        return;
    }
    // Handle padding to the left.
    if (f.widPresent && f.wid > width && !f.minus) {
        f.writePadding(f.wid - width);
    }
    // Write the encoding directly into the output buffer.
    var buf = f.buf.ValueSlot;
    if (f.sharp) {
        // Add leading 0x or 0X.
        buf = append(buf, (byte)((rune)'0'), digits[16]);
    }
    byte c = default!;
    for (nint i = 0; i < length; i++) {
        if (f.space && i > 0) {
            // Separate elements with a space.
            buf = append(buf, (byte)((rune)' '));
            if (f.sharp) {
                // Add leading 0x or 0X for each element.
                buf = append(buf, (byte)((rune)'0'), digits[16]);
            }
        }
        if (b != default!){
            c = b[i]; // Take a byte from the input byte slice.
        } else {
            c = s[i]; // Take a byte from the input string.
        }
        // Encode each byte as two hexadecimal digits.
        buf = append(buf, digits[(c >> (int)(4))], digits[(byte)(c & 0xF)]);
    }
    f.buf.ValueSlot = buf;
    // Handle padding to the right.
    if (f.widPresent && f.wid > width && f.minus) {
        f.writePadding(f.wid - width);
    }
}

// fmtSx formats a string as a hexadecimal encoding of its bytes.
internal static void fmtSx(this ref fmt f, sstring s, sstring digits) {
    f.fmtSbx(s, default!, digits);
}

// fmtBx formats a byte slice as a hexadecimal encoding of its bytes.
internal static void fmtBx(this ref fmt f, slice<byte> b, sstring digits) {
    f.fmtSbx(""u8, b, digits);
}

// fmtQ formats a string as a double-quoted, escaped Go string constant.
// If f.sharp is set a raw (backquoted) string may be returned instead
// if the string does not contain any control characters other than tab.
internal static void fmtQ(this ref fmt f, @string s) {
    s = f.truncateString(s);
    if (f.sharp && strconv.CanBackquote(s)) {
        f.padString("`"u8 + s + "`"u8);
        return;
    }
    var buf = f.intbuf[..0];
    if (f.plus){
        f.pad(strconv.AppendQuoteToASCII(buf, s));
    } else {
        f.pad(strconv.AppendQuote(buf, s));
    }
}

// fmtC formats an integer as a Unicode character.
// If the character is not valid Unicode, it will print '\ufffd'.
internal static void fmtC(this ref fmt f, uint64 c) {
    // Explicitly check whether c exceeds utf8.MaxRune since the conversion
    // of a uint64 to a rune may lose precision that indicates an overflow.
    var r = (rune)c;
    if (c > utf8.MaxRune) {
        r = utf8.RuneError;
    }
    var buf = f.intbuf[..0];
    f.pad(utf8.AppendRune(buf, r));
}

// fmtQc formats an integer as a single-quoted, escaped Go character constant.
// If the character is not valid Unicode, it will print '\ufffd'.
internal static void fmtQc(this ref fmt f, uint64 c) {
    var r = (rune)c;
    if (c > utf8.MaxRune) {
        r = utf8.RuneError;
    }
    var buf = f.intbuf[..0];
    if (f.plus){
        f.pad(strconv.AppendQuoteRuneToASCII(buf, r));
    } else {
        f.pad(strconv.AppendQuoteRune(buf, r));
    }
}

// fmtFloat formats a float64. It assumes that verb is a valid format specifier
// for strconv.AppendFloat and therefore fits into a byte.
internal static void fmtFloat(this ref fmt f, float64 v, nint size, rune verb, nint prec) {
    // Explicit precision in format specifier overrules default precision.
    if (f.precPresent) {
        prec = f.prec;
    }
    // Format number, reserving space for leading + sign if needed.
    var num = strconv.AppendFloat(f.intbuf[..1], v, (byte)verb, prec, size);
    if (num[1] == (rune)'-' || num[1] == (rune)'+'){
        num = num[1..];
    } else {
        num[0] = (rune)'+';
    }
    // f.space means to add a leading space instead of a "+" sign unless
    // the sign is explicitly asked for by f.plus.
    if (f.space && num[0] == (rune)'+' && !f.plus) {
        num[0] = (rune)' ';
    }
    // Special handling for infinities and NaN,
    // which don't look like a number so shouldn't be padded with zeros.
    if (num[1] == (rune)'I' || num[1] == (rune)'N') {
        var oldZero = f.zero;
        f.zero = false;
        // Remove sign before NaN if not asked for.
        if (num[1] == (rune)'N' && !f.space && !f.plus) {
            num = num[1..];
        }
        f.pad(num);
        f.zero = oldZero;
        return;
    }
    // The sharp flag forces printing a decimal point for non-binary formats
    // and retains trailing zeros, which we may need to restore.
    if (f.sharp && verb != (rune)'b') {
        nint digits = 0;
        switch (verb) {
        case (rune)'v' or (rune)'g' or (rune)'G' or (rune)'x': {
            digits = prec;
            if (digits == -1) {
                // If no precision is set explicitly use a precision of 6.
                digits = 6;
            }
            break;
        }}

        // Buffer pre-allocated with enough room for
        // exponent notations of the form "e+123" or "p-1023".
        array<byte> tailBuf = new(6);
        var tail = tailBuf[..0];
        var hasDecimalPoint = false;
        var sawNonzeroDigit = false;
        // Starting from i = 1 to skip sign at num[0].
        for (nint i = 1; i < len(num); i++) {
            var exprᴛ1 = num[i];
            var matchᴛ1 = false;
            if (exprᴛ1 is (rune)'.') { matchᴛ1 = true;
                hasDecimalPoint = true;
            }
            else if (exprᴛ1 is (rune)'p' or (rune)'P') { matchᴛ1 = true;
                tail = appendꓸꓸꓸ(tail, num.slice(i));
                num = num.slice(0, i);
            }
            else if (exprᴛ1 is (rune)'e' or (rune)'E') { matchᴛ1 = true;
                do {
                    if (verb != (rune)'x' && verb != (rune)'X') {
                        tail = appendꓸꓸꓸ(tail, num.slice(i));
                        num = num.slice(0, i);
                        break;
                    }
                    fallthrough = true;
                } while (false);
            }
            if (fallthrough || !matchᴛ1) { /* default: */
                if (num[i] != (rune)'0') {
                    sawNonzeroDigit = true;
                }
                if (sawNonzeroDigit) {
                    // Count significant digits after the first non-zero digit.
                    digits--;
                }
            }

        }
        if (!hasDecimalPoint) {
            // Leading digit 0 should contribute once to digits.
            if (len(num) == 2 && num[1] == (rune)'0') {
                digits--;
            }
            num = append(num, (byte)((rune)'.'));
        }
        while (digits > 0) {
            num = append(num, (byte)((rune)'0'));
            digits--;
        }
        num = appendꓸꓸꓸ(num, tail);
    }
    // We want a sign if asked for and if the sign is not positive.
    if (f.plus || num[0] != (rune)'+') {
        // If we're zero padding to the left we want the sign before the leading zeros.
        // Achieve this by writing the sign out and then padding the unsigned number.
        // Zero padding is allowed only to the left.
        if (f.zero && !f.minus && f.widPresent && f.wid > len(num)) {
            f.buf.writeByte(num[0]);
            f.writePadding(f.wid - len(num));
            f.buf.write(num[1..]);
            return;
        }
        f.pad(num);
        return;
    }
    // No sign to show and the number is positive; just print the unsigned number.
    f.pad(num[1..]);
}

} // end fmt_package
