// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package utf8 implements functions and constants to support text encoded in
// UTF-8. It includes functions to translate between runes and UTF-8 byte sequences.
// See https://en.wikipedia.org/wiki/UTF-8
namespace go.unicode;

partial class utf8_package {

// The conditions RuneError==unicode.ReplacementChar and
// MaxRune==unicode.MaxRune are verified in the tests.
// Defining them locally avoids this package depending on package unicode.

// Numbers fundamental to the encoding.
public static UntypedInt RuneError => /* '\uFFFD' */ 65533;     // the "error" Rune or "Unicode replacement character"
public static UntypedInt RuneSelf => 0x80;                      // characters below RuneSelf are represented as themselves in a single byte.
public static UntypedInt MaxRune => /* '\U0010FFFF' */ 1114111; // Maximum valid Unicode code point.
public static UntypedInt UTFMax => 4;                           // maximum number of bytes of a UTF-8 encoded Unicode character.

// Code points in the surrogate range are not valid for UTF-8.
internal static UntypedInt surrogateMin => 0xD800;
internal static UntypedInt surrogateMax => 0xDFFF;

internal static UntypedInt t1 => 0b00000000;
internal static UntypedInt tx => 0b10000000;
internal static UntypedInt t2 => 0b11000000;
internal static UntypedInt t3 => 0b11100000;
internal static UntypedInt t4 => 0b11110000;
internal static UntypedInt t5 => 0b11111000;

internal static UntypedInt maskx => 0b00111111;
internal static UntypedInt mask2 => 0b00011111;
internal static UntypedInt mask3 => 0b00001111;
internal static UntypedInt mask4 => 0b00000111;

internal static UntypedInt rune1Max => /* 1<<7 - 1 */ 127;
internal static UntypedInt rune2Max => /* 1<<11 - 1 */ 2047;
internal static UntypedInt rune3Max => /* 1<<16 - 1 */ 65535;

// The default lowest and highest continuation byte.
internal static UntypedInt locb => 0b10000000;
internal static UntypedInt hicb => 0b10111111;

// These names of these constants are chosen to give nice alignment in the
// table below. The first nibble is an index into acceptRanges or F for
// special one-byte cases. The second nibble is the Rune length or the
// Status for the special one-byte case.
internal static UntypedInt xx => 0xF1;  // invalid: size 1
internal static UntypedInt @as => 0xF0; // ASCII: size 1
internal static UntypedInt s1 => 0x02;  // accept 0, size 2
internal static UntypedInt s2 => 0x13;  // accept 1, size 3
internal static UntypedInt s3 => 0x03;  // accept 0, size 3
internal static UntypedInt s4 => 0x23;  // accept 2, size 3
internal static UntypedInt s5 => 0x34;  // accept 3, size 4
internal static UntypedInt s6 => 0x04;  // accept 0, size 4
internal static UntypedInt s7 => 0x44;  // accept 4, size 4

internal static UntypedInt runeErrorByte0 => /* t3 | (RuneError >> 12) */ 239;
internal static UntypedInt runeErrorByte1 => /* tx | (RuneError>>6)&maskx */ 191;
internal static UntypedInt runeErrorByte2 => /* tx | RuneError&maskx */ 189;

// first is information about the first byte in a UTF-8 sequence.
[GoLazy] partial class lazyᴛfirst { internal static array<uint8> first = new uint8[]{
    //   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x00-0x0F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x10-0x1F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x20-0x2F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x30-0x3F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x40-0x4F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x50-0x5F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x60-0x6F
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, // 0x70-0x7F
    //   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0x80-0x8F
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0x90-0x9F
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0xA0-0xAF
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, // 0xB0-0xBF
    xx, xx, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, // 0xC0-0xCF
    s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, // 0xD0-0xDF
    s2, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s4, s3, s3, // 0xE0-0xEF
    s5, s6, s6, s6, s7, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx  // 0xF0-0xFF
}.array(); }

// acceptRange gives the range of valid values for the second byte in a UTF-8
// sequence.
partial struct acceptRange {
    internal uint8 lo; // lowest value for second byte.
    internal uint8 hi; // highest value for second byte.
}

// acceptRanges has size 16 to avoid bounds checks in the code that uses it.
[GoLazy] partial class lazyᴛacceptRanges { internal static array<acceptRange> acceptRanges = new array<acceptRange>(16){
    [0] = new(locb, hicb),
    [1] = new(0xA0, hicb),
    [2] = new(locb, 0x9F),
    [3] = new(0x90, hicb),
    [4] = new(locb, 0x8F)
}; }

// FullRune reports whether the bytes in p begin with a full UTF-8 encoding of a rune.
// An invalid encoding is considered a full Rune since it will convert as a width-1 error rune.
public static bool FullRune(slice<byte> p) {
    nint n = len(p);
    if (n == 0) {
        return false;
    }
    var x = first[p[0]];
    if (n >= (nint)((uint8)(x & 7))) {
        return true; // ASCII, invalid or valid.
    }
    // Must be short or invalid.
    var accept = acceptRanges[(x >> 4)];
    if (n > 1 && (p[1] < accept.lo || accept.hi < p[1])) {
        return true;
    } else if (n > 2 && (p[2] < locb || hicb < p[2])) {
        return true;
    }
    return false;
}

// FullRuneInString is like FullRune but its input is a string.
public static bool FullRuneInString(@string s) {
    nint n = len(s);
    if (n == 0) {
        return false;
    }
    var x = first[s[0]];
    if (n >= (nint)((uint8)(x & 7))) {
        return true; // ASCII, invalid, or valid.
    }
    // Must be short or invalid.
    var accept = acceptRanges[(x >> 4)];
    if (n > 1 && (s[1] < accept.lo || accept.hi < s[1])) {
        return true;
    } else if (n > 2 && (s[2] < locb || hicb < s[2])) {
        return true;
    }
    return false;
}

// DecodeRune unpacks the first UTF-8 encoding in p and returns the rune and
// its width in bytes. If p is empty it returns ([RuneError], 0). Otherwise, if
// the encoding is invalid, it returns (RuneError, 1). Both are impossible
// results for correct, non-empty UTF-8.
//
// An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
// out of range, or is not the shortest possible UTF-8 encoding for the
// value. No other validation is performed.
public static (rune r, nint size) DecodeRune(slice<byte> p) {
    rune r = default;
    nint size = default;

    // Inlineable fast path for ASCII characters; see #48195.
    // This implementation is weird but effective at rendering the
    // function inlineable.
    foreach (var (_, b) in p) {
        if (b < RuneSelf) {
            return ((rune)b, 1);
        }
        break;
    }
    (r, size) = decodeRuneSlow(p);
    return (r, size);
}

internal static (rune r, nint size) decodeRuneSlow(slice<byte> p) {
    nint n = len(p);
    if (n < 1) {
        return (RuneError, 0);
    }
    var p0 = p[0];
    var x = first[p0];
    if (x >= @as) {
        // The following code simulates an additional check for x == xx and
        // handling the ASCII and invalid cases accordingly. This mask-and-or
        // approach prevents an additional branch.
        var mask = ((rune)x << 31) >> 31; // Create 0x0000 or 0xFFFF.
        return ((rune)p[0] & ~mask | (rune)RuneError & mask, 1);
    }
    nint sz = (nint)((uint8)(x & 7));
    var accept = acceptRanges[(x >> 4)];
    if (n < sz) {
        return (RuneError, 1);
    }
    var b1 = p[1];
    if (b1 < accept.lo || accept.hi < b1) {
        return (RuneError, 1);
    }
    if (sz <= 2) { // <= instead of == to help the compiler eliminate some bounds checks
        return ((rune)((byte)(p0 & (byte)mask2)) << 6 | (rune)((byte)(b1 & (byte)maskx)), 2);
    }
    var b2 = p[2];
    if (b2 < locb || hicb < b2) {
        return (RuneError, 1);
    }
    if (sz <= 3) {
        return ((rune)((byte)(p0 & (byte)mask3)) << 12 | (rune)((byte)(b1 & (byte)maskx)) << 6 | (rune)((byte)(b2 & (byte)maskx)), 3);
    }
    var b3 = p[3];
    if (b3 < locb || hicb < b3) {
        return (RuneError, 1);
    }
    return ((rune)((byte)(p0 & (byte)mask4)) << 18 | (rune)((byte)(b1 & (byte)maskx)) << 12 | (rune)((byte)(b2 & (byte)maskx)) << 6 | (rune)((byte)(b3 & (byte)maskx)), 4);
}

// DecodeRuneInString is like [DecodeRune] but its input is a string. If s is
// empty it returns ([RuneError], 0). Otherwise, if the encoding is invalid, it
// returns (RuneError, 1). Both are impossible results for correct, non-empty
// UTF-8.
//
// An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
// out of range, or is not the shortest possible UTF-8 encoding for the
// value. No other validation is performed.
public static (rune r, nint size) DecodeRuneInString(sstring s) {
    rune r = default;
    nint size = default;

    // Inlineable fast path for ASCII characters; see #48195.
    // This implementation is a bit weird but effective at rendering the
    // function inlineable.
    if (s != ""u8 && s[0] < RuneSelf) {
        return ((rune)s[0], 1);
    } else {
        (r, size) = decodeRuneInStringSlow(s);
    }
    return (r, size);
}

internal static (rune, nint) decodeRuneInStringSlow(@string s) {
    nint n = len(s);
    if (n < 1) {
        return (RuneError, 0);
    }
    var s0 = s[0];
    var x = first[s0];
    if (x >= @as) {
        // The following code simulates an additional check for x == xx and
        // handling the ASCII and invalid cases accordingly. This mask-and-or
        // approach prevents an additional branch.
        var mask = ((rune)x << 31) >> 31; // Create 0x0000 or 0xFFFF.
        return ((rune)s[0] & ~mask | (rune)RuneError & mask, 1);
    }
    nint sz = (nint)((uint8)(x & 7));
    var accept = acceptRanges[(x >> 4)];
    if (n < sz) {
        return (RuneError, 1);
    }
    var s1 = s[1];
    if (s1 < accept.lo || accept.hi < s1) {
        return (RuneError, 1);
    }
    if (sz <= 2) { // <= instead of == to help the compiler eliminate some bounds checks
        return ((rune)((byte)(s0 & (byte)mask2)) << 6 | (rune)((byte)(s1 & (byte)maskx)), 2);
    }
    var s2 = s[2];
    if (s2 < locb || hicb < s2) {
        return (RuneError, 1);
    }
    if (sz <= 3) {
        return ((rune)((byte)(s0 & (byte)mask3)) << 12 | (rune)((byte)(s1 & (byte)maskx)) << 6 | (rune)((byte)(s2 & (byte)maskx)), 3);
    }
    var s3 = s[3];
    if (s3 < locb || hicb < s3) {
        return (RuneError, 1);
    }
    return ((rune)((byte)(s0 & (byte)mask4)) << 18 | (rune)((byte)(s1 & (byte)maskx)) << 12 | (rune)((byte)(s2 & (byte)maskx)) << 6 | (rune)((byte)(s3 & (byte)maskx)), 4);
}

// DecodeLastRune unpacks the last UTF-8 encoding in p and returns the rune and
// its width in bytes. If p is empty it returns ([RuneError], 0). Otherwise, if
// the encoding is invalid, it returns (RuneError, 1). Both are impossible
// results for correct, non-empty UTF-8.
//
// An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
// out of range, or is not the shortest possible UTF-8 encoding for the
// value. No other validation is performed.
public static (rune r, nint size) DecodeLastRune(slice<byte> p) {
    rune r = default;
    nint size = default;

    nint end = len(p);
    if (end == 0) {
        return (RuneError, 0);
    }
    nint start = end - 1;
    r = (rune)p[start];
    if (r < RuneSelf) {
        return (r, 1);
    }
    // guard against O(n^2) behavior when traversing
    // backwards through strings with long sequences of
    // invalid UTF-8.
    nint lim = max(end - (nint)UTFMax, 0);
    for (start--; start >= lim; start--) {
        if (RuneStart(p[start])) {
            break;
        }
    }
    if (start < 0) {
        start = 0;
    }
    (r, size) = DecodeRune(p.slice(start, end));
    if (start + size != end) {
        return (RuneError, 1);
    }
    return (r, size);
}

// DecodeLastRuneInString is like [DecodeLastRune] but its input is a string. If
// s is empty it returns ([RuneError], 0). Otherwise, if the encoding is invalid,
// it returns (RuneError, 1). Both are impossible results for correct,
// non-empty UTF-8.
//
// An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
// out of range, or is not the shortest possible UTF-8 encoding for the
// value. No other validation is performed.
public static (rune r, nint size) DecodeLastRuneInString(@string s) {
    rune r = default;
    nint size = default;

    nint end = len(s);
    if (end == 0) {
        return (RuneError, 0);
    }
    nint start = end - 1;
    r = (rune)s[start];
    if (r < RuneSelf) {
        return (r, 1);
    }
    // guard against O(n^2) behavior when traversing
    // backwards through strings with long sequences of
    // invalid UTF-8.
    nint lim = max(end - (nint)UTFMax, 0);
    for (start--; start >= lim; start--) {
        if (RuneStart(s[start])) {
            break;
        }
    }
    if (start < 0) {
        start = 0;
    }
    (r, size) = DecodeRuneInString(s.slice(start, end));
    if (start + size != end) {
        return (RuneError, 1);
    }
    return (r, size);
}

// RuneLen returns the number of bytes in the UTF-8 encoding of the rune.
// It returns -1 if the rune is not a valid value to encode in UTF-8.
public static nint RuneLen(rune r) {
    switch (ᐧ) {
    case {} when r is < 0: {
        return -1;
    }
    case {} when r <= rune1Max: {
        return 1;
    }
    case {} when r <= rune2Max: {
        return 2;
    }
    case {} when surrogateMin <= r && r <= surrogateMax: {
        return -1;
    }
    case {} when r <= rune3Max: {
        return 3;
    }
    case {} when r <= MaxRune: {
        return 4;
    }}
    return -1;
}

// EncodeRune writes into p (which must be large enough) the UTF-8 encoding of the rune.
// If the rune is out of range, it writes the encoding of [RuneError].
// It returns the number of bytes written.
public static nint EncodeRune(slice<byte> p, rune r) {
    // This function is inlineable for fast handling of ASCII.
    if ((uint32)r <= rune1Max) {
        p[0] = (byte)r;
        return 1;
    }
    return encodeRuneNonASCII(p, r);
}

internal static nint encodeRuneNonASCII(slice<byte> p, rune r) {
    // Negative values are erroneous. Making it unsigned addresses the problem.
    {
        var i = (uint32)r;
        switch (ᐧ) {
        case {} when i <= rune2Max: {
            _ = p[1]; // eliminate bounds checks
            p[0] = (byte)((byte)t2 | (byte)(r >> 6));
            p[1] = (byte)((byte)tx | (byte)((byte)r & (byte)maskx));
            return 2;
        }
        case {} when (i < surrogateMin) || (surrogateMax < i && i <= rune3Max): {
            _ = p[2]; // eliminate bounds checks
            p[0] = (byte)((byte)t3 | (byte)(r >> 12));
            p[1] = (byte)((byte)tx | (byte)((byte)(r >> 6) & (byte)maskx));
            p[2] = (byte)((byte)tx | (byte)((byte)r & (byte)maskx));
            return 3;
        }
        case {} when i > rune3Max && i <= MaxRune: {
            _ = p[3]; // eliminate bounds checks
            p[0] = (byte)((byte)t4 | (byte)(r >> 18));
            p[1] = (byte)((byte)tx | (byte)((byte)(r >> 12) & (byte)maskx));
            p[2] = (byte)((byte)tx | (byte)((byte)(r >> 6) & (byte)maskx));
            p[3] = (byte)((byte)tx | (byte)((byte)r & (byte)maskx));
            return 4;
        }
        default: {
            _ = p[2]; // eliminate bounds checks
            p[0] = runeErrorByte0;
            p[1] = runeErrorByte1;
            p[2] = runeErrorByte2;
            return 3;
        }}
    }
}

// AppendRune appends the UTF-8 encoding of r to the end of p and
// returns the extended buffer. If the rune is out of range,
// it appends the encoding of [RuneError].
public static slice<byte> AppendRune(slice<byte> p, rune r) {
    // This function is inlineable for fast handling of ASCII.
    if ((uint32)r <= rune1Max) {
        return append(p, (byte)r);
    }
    return appendRuneNonASCII(p, r);
}

internal static slice<byte> appendRuneNonASCII(slice<byte> p, rune r) {
    // Negative values are erroneous. Making it unsigned addresses the problem.
    {
        var i = (uint32)r;
        switch (ᐧ) {
        case {} when i <= rune2Max: {
            return append(p, (byte)((byte)t2 | (byte)(r >> 6)), (byte)((byte)tx | (byte)((byte)r & (byte)maskx)));
        }
        case {} when (i < surrogateMin) || (surrogateMax < i && i <= rune3Max): {
            return append(p, (byte)((byte)t3 | (byte)(r >> 12)), (byte)((byte)tx | (byte)((byte)(r >> 6) & (byte)maskx)), (byte)((byte)tx | (byte)((byte)r & (byte)maskx)));
        }
        case {} when i > rune3Max && i <= MaxRune: {
            return append(p, (byte)((byte)t4 | (byte)(r >> 18)), (byte)((byte)tx | (byte)((byte)(r >> 12) & (byte)maskx)), (byte)((byte)tx | (byte)((byte)(r >> 6) & (byte)maskx)), (byte)((byte)tx | (byte)((byte)r & (byte)maskx)));
        }
        default: {
            return append(p, (byte)(runeErrorByte0), (byte)(runeErrorByte1), (byte)(runeErrorByte2));
        }}
    }
}

// RuneCount returns the number of runes in p. Erroneous and short
// encodings are treated as single runes of width 1 byte.
public static nint RuneCount(slice<byte> p) {
    nint np = len(p);
    nint n = default;
    for (; n < np; n++) {
        {
            var c = p[n]; if (c >= RuneSelf) {
                // non-ASCII slow path
                return n + RuneCountInString((sstring)(p.slice(n)));
            }
        }
    }
    return n;
}

// RuneCountInString is like [RuneCount] but its input is a string.
public static nint /*n*/ RuneCountInString(sstring s) {
    nint n = default;

    foreach ((_, _) in s) {
        n++;
    }
    return n;
}

// RuneStart reports whether the byte could be the first byte of an encoded,
// possibly invalid rune. Second and subsequent bytes always have the top two
// bits set to 10.
public static bool RuneStart(byte b) {
    return (byte)(b & 0xC0) != 0x80;
}

internal static UntypedInt ptrSize => /* 4 << (^uintptr(0) >> 63) */ 8;

internal static UntypedInt hiBits => /* 0x8080808080808080 >> (64 - 8*ptrSize) */ 9259542123273814144;

internal static uintptr word<T>(T s)
    where T : /* string | []byte */ IByteSeq<T, byte>, new()
{
    if (ptrSize == 4) {
        return (uintptr)((uintptr)((uintptr)((uintptr)s[0] | (uintptr)s[1] << 8) | (uintptr)s[2] << 16) | (uintptr)s[3] << 24);
    }
    return (uintptr)((uint64)s[0] | (uint64)s[1] << 8 | (uint64)s[2] << 16 | (uint64)s[3] << 24 | (uint64)s[4] << 32 | (uint64)s[5] << 40 | (uint64)s[6] << 48 | (uint64)s[7] << 56);
}

// Valid reports whether p consists entirely of valid UTF-8-encoded runes.
public static bool Valid(slice<byte> p) {
    // This optimization avoids the need to recompute the capacity
    // when generating code for slicing p, bringing it to parity with
    // ValidString, which was 20% faster on long ASCII strings.
    p = p.slice(0, len(p), len(p));

    while (len(p) > 0) {
        var p0 = p[0];
        if (p0 < RuneSelf) {
            p = p[1..];
            // If there's one ASCII byte, there are probably more.
            // Advance quickly through ASCII-only data.
            // Note: using > instead of >= here is intentional. That avoids
            // needing pointing-past-the-end fixup on the slice operations.
            if (len(p) > ptrSize && (uintptr)(word(p) & (uintptr)hiBits) == 0) {
                p = p[(int)(ptrSize)..];
                if (len(p) > (nint)(2 * ptrSize) && (uintptr)(((uintptr)(word(p) | word(p[(int)(ptrSize)..]))) & (uintptr)hiBits) == 0) {
                    p = p[(int)(2 * ptrSize)..];
                    while (len(p) > (nint)(4 * ptrSize) && (uintptr)(((uintptr)(((uintptr)(word(p) | word(p[(int)(ptrSize)..]))) | ((uintptr)(word(p[(int)(2 * ptrSize)..]) | word(p[(int)(3 * ptrSize)..]))))) & (uintptr)hiBits) == 0) {
                        p = p[(int)(4 * ptrSize)..];
                    }
                }
            }
            continue;
        }
        var x = first[p0];
        nint size = (nint)((uint8)(x & 7));
        var accept = acceptRanges[(x >> 4)];
        switch (size) {
        case 2: {
            if (len(p) < 2 || p[1] < accept.lo || accept.hi < p[1]) {
                return false;
            }
            p = p[2..];
            break;
        }
        case 3: {
            if (len(p) < 3 || p[1] < accept.lo || accept.hi < p[1] || p[2] < locb || hicb < p[2]) {
                return false;
            }
            p = p[3..];
            break;
        }
        case 4: {
            if (len(p) < 4 || p[1] < accept.lo || accept.hi < p[1] || p[2] < locb || hicb < p[2] || p[3] < locb || hicb < p[3]) {
                return false;
            }
            p = p[4..];
            break;
        }
        default: {
            return false; // illegal starter byte
        }}
    }
    return true;
}

// ValidString reports whether s consists entirely of valid UTF-8-encoded runes.
public static bool ValidString(@string s) {
    while (len(s) > 0) {
        var s0 = s[0];
        if (s0 < RuneSelf) {
            s = s[1..];
            // If there's one ASCII byte, there are probably more.
            // Advance quickly through ASCII-only data.
            // Note: using > instead of >= here is intentional. That avoids
            // needing pointing-past-the-end fixup on the slice operations.
            if (len(s) > ptrSize && (uintptr)(word(s) & (uintptr)hiBits) == 0) {
                s = s[(int)(ptrSize)..];
                if (len(s) > (nint)(2 * ptrSize) && (uintptr)(((uintptr)(word(s) | word(s[(int)(ptrSize)..]))) & (uintptr)hiBits) == 0) {
                    s = s[(int)(2 * ptrSize)..];
                    while (len(s) > (nint)(4 * ptrSize) && (uintptr)(((uintptr)(((uintptr)(word(s) | word(s[(int)(ptrSize)..]))) | ((uintptr)(word(s[(int)(2 * ptrSize)..]) | word(s[(int)(3 * ptrSize)..]))))) & (uintptr)hiBits) == 0) {
                        s = s[(int)(4 * ptrSize)..];
                    }
                }
            }
            continue;
        }
        var x = first[s0];
        nint size = (nint)((uint8)(x & 7));
        var accept = acceptRanges[(x >> 4)];
        switch (size) {
        case 2: {
            if (len(s) < 2 || s[1] < accept.lo || accept.hi < s[1]) {
                return false;
            }
            s = s[2..];
            break;
        }
        case 3: {
            if (len(s) < 3 || s[1] < accept.lo || accept.hi < s[1] || s[2] < locb || hicb < s[2]) {
                return false;
            }
            s = s[3..];
            break;
        }
        case 4: {
            if (len(s) < 4 || s[1] < accept.lo || accept.hi < s[1] || s[2] < locb || hicb < s[2] || s[3] < locb || hicb < s[3]) {
                return false;
            }
            s = s[4..];
            break;
        }
        default: {
            return false; // illegal starter byte
        }}
    }
    return true;
}

// ValidRune reports whether r can be legally encoded as UTF-8.
// Code points that are out of range or a surrogate half are illegal.
public static bool ValidRune(rune r) {
    switch (ᐧ) {
    case {} when 0 <= r && r < surrogateMin: {
        return true;
    }
    case {} when surrogateMax < r && r <= MaxRune: {
        return true;
    }}
    return false;
}

} // end utf8_package
