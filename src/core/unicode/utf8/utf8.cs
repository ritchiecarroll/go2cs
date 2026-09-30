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
public static UntypedInt RuneError => /* '\uFFFD' */ 65533; // the "error" Rune or "Unicode replacement character"

public static UntypedInt RuneSelf => 0x80; // characters below RuneSelf are represented as themselves in a single byte.

public static UntypedInt MaxRune => /* '\U0010FFFF' */ 1114111; // Maximum valid Unicode code point.

public static UntypedInt UTFMax => 4; // maximum number of bytes of a UTF-8 encoded Unicode character.

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
internal static UntypedInt locb => 0b10000000;
internal static UntypedInt hicb => 0b10111111;
internal static UntypedInt xx => 0xF1; // invalid: size 1
internal static UntypedInt @as => 0xF0; // ASCII: size 1
internal static UntypedInt s1 => 0x02; // accept 0, size 2
internal static UntypedInt s2 => 0x13; // accept 1, size 3
internal static UntypedInt s3 => 0x03; // accept 0, size 3
internal static UntypedInt s4 => 0x23; // accept 2, size 3
internal static UntypedInt s5 => 0x34; // accept 3, size 4
internal static UntypedInt s6 => 0x04; // accept 0, size 4
internal static UntypedInt s7 => 0x44; // accept 4, size 4

internal static UntypedInt runeErrorByte0 => /* t3 | (RuneError >> 12) */ 239;
internal static UntypedInt runeErrorByte1 => /* tx | (RuneError>>6)&maskx */ 191;
internal static UntypedInt runeErrorByte2 => /* tx | RuneError&maskx */ 189;

//   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
// 0x00-0x0F
// 0x10-0x1F
// 0x20-0x2F
// 0x30-0x3F
// 0x40-0x4F
// 0x50-0x5F
// 0x60-0x6F
// 0x70-0x7F
//   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
// 0x80-0x8F
// 0x90-0x9F
// 0xA0-0xAF
// 0xB0-0xBF
// 0xC0-0xCF
// 0xD0-0xDF
// 0xE0-0xEF
// 0xF0-0xFF
// first is information about the first byte in a UTF-8 sequence.
internal static array<uint8> first = new uint8[]{
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as, @as,
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx,
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx,
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx,
    xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx,
    xx, xx, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1,
    s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1, s1,
    s2, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s3, s4, s3, s3,
    s5, s6, s6, s6, s7, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx, xx
}.array();

// acceptRange gives the range of valid values for the second byte in a UTF-8
// sequence.
[GoType] partial struct acceptRange {
    internal uint8 lo; // lowest value for second byte.
    internal uint8 hi; // highest value for second byte.
}

// acceptRanges has size 16 to avoid bounds checks in the code that uses it.
internal static array<acceptRange> acceptRanges = new array<acceptRange>(16){
    [0] = new(locb, hicb),
    [1] = new(0xA0, hicb),
    [2] = new(locb, 0x9F),
    [3] = new(0x90, hicb),
    [4] = new(locb, 0x8F)
};

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
    var accept = acceptRanges[(x >> (int)(4))];
    if (n > 1 && (p[1] < accept.lo || accept.hi < p[1])){
        return true;
    } else 
    if (n > 2 && (p[2] < locb || hicb < p[2])) {
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
    var accept = acceptRanges[(x >> (int)(4))];
    if (n > 1 && (s[1] < accept.lo || accept.hi < s[1])){
        return true;
    } else 
    if (n > 2 && (s[2] < locb || hicb < s[2])) {
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
        var mask = (((rune)x << (int)(31)) >> (int)(31)); // Create 0x0000 or 0xFFFF.
        return ((rune)((rune)((rune)p[0] & ~mask) | (rune)((rune)RuneError & mask)), 1);
    }
    nint sz = (nint)((uint8)(x & 7));
    var accept = acceptRanges[(x >> (int)(4))];
    if (n < sz) {
        return (RuneError, 1);
    }
    var b1 = p[1];
    if (b1 < accept.lo || accept.hi < b1) {
        return (RuneError, 1);
    }
    if (sz <= 2) {
        // <= instead of == to help the compiler eliminate some bounds checks
        return ((rune)(((rune)((byte)(p0 & (byte)mask2)) << (int)(6)) | (rune)((byte)(b1 & (byte)maskx))), 2);
    }
    var b2 = p[2];
    if (b2 < locb || hicb < b2) {
        return (RuneError, 1);
    }
    if (sz <= 3) {
        return ((rune)((rune)(((rune)((byte)(p0 & (byte)mask3)) << (int)(12)) | ((rune)((byte)(b1 & (byte)maskx)) << (int)(6))) | (rune)((byte)(b2 & (byte)maskx))), 3);
    }
    var b3 = p[3];
    if (b3 < locb || hicb < b3) {
        return (RuneError, 1);
    }
    return ((rune)((rune)((rune)(((rune)((byte)(p0 & (byte)mask4)) << (int)(18)) | ((rune)((byte)(b1 & (byte)maskx)) << (int)(12))) | ((rune)((byte)(b2 & (byte)maskx)) << (int)(6))) | (rune)((byte)(b3 & (byte)maskx))), 4);
}

// DecodeRuneInString is like [DecodeRune] but its input is a string. If s is
// empty it returns ([RuneError], 0). Otherwise, if the encoding is invalid, it
// returns (RuneError, 1). Both are impossible results for correct, non-empty
// UTF-8.
//
// An encoding is invalid if it is incorrect UTF-8, encodes a rune that is
// out of range, or is not the shortest possible UTF-8 encoding for the
// value. No other validation is performed.
[GoStr] public static (rune r, nint size) DecodeRuneInString(sstring s) {
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
        var mask = (((rune)x << (int)(31)) >> (int)(31)); // Create 0x0000 or 0xFFFF.
        return ((rune)((rune)((rune)s[0] & ~mask) | (rune)((rune)RuneError & mask)), 1);
    }
    nint sz = (nint)((uint8)(x & 7));
    var accept = acceptRanges[(x >> (int)(4))];
    if (n < sz) {
        return (RuneError, 1);
    }
    var s1 = s[1];
    if (s1 < accept.lo || accept.hi < s1) {
        return (RuneError, 1);
    }
    if (sz <= 2) {
        // <= instead of == to help the compiler eliminate some bounds checks
        return ((rune)(((rune)((byte)(s0 & (byte)mask2)) << (int)(6)) | (rune)((byte)(s1 & (byte)maskx))), 2);
    }
    var s2 = s[2];
    if (s2 < locb || hicb < s2) {
        return (RuneError, 1);
    }
    if (sz <= 3) {
        return ((rune)((rune)(((rune)((byte)(s0 & (byte)mask3)) << (int)(12)) | ((rune)((byte)(s1 & (byte)maskx)) << (int)(6))) | (rune)((byte)(s2 & (byte)maskx))), 3);
    }
    var s3 = s[3];
    if (s3 < locb || hicb < s3) {
        return (RuneError, 1);
    }
    return ((rune)((rune)((rune)(((rune)((byte)(s0 & (byte)mask4)) << (int)(18)) | ((rune)((byte)(s1 & (byte)maskx)) << (int)(12))) | ((rune)((byte)(s2 & (byte)maskx)) << (int)(6))) | (rune)((byte)(s3 & (byte)maskx))), 4);
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
    rune r = default!;
    nint size = default!;

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
    nint lim = end - (nint)UTFMax;
    if (lim < 0) {
        lim = 0;
    }
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
    rune r = default!;
    nint size = default!;

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
    nint lim = end - (nint)UTFMax;
    if (lim < 0) {
        lim = 0;
    }
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
            p[0] = (byte)((byte)t2 | (byte)((r >> (int)(6))));
            p[1] = (byte)((byte)tx | (byte)((byte)r & (byte)maskx));
            return 2;
        }
        case {} when (i < surrogateMin) || (surrogateMax < i && i <= rune3Max): {
            _ = p[2]; // eliminate bounds checks
            p[0] = (byte)((byte)t3 | (byte)((r >> (int)(12))));
            p[1] = (byte)((byte)tx | (byte)((byte)((r >> (int)(6))) & (byte)maskx));
            p[2] = (byte)((byte)tx | (byte)((byte)r & (byte)maskx));
            return 3;
        }
        case {} when i > rune3Max && i <= MaxRune: {
            _ = p[3]; // eliminate bounds checks
            p[0] = (byte)((byte)t4 | (byte)((r >> (int)(18))));
            p[1] = (byte)((byte)tx | (byte)((byte)((r >> (int)(12))) & (byte)maskx));
            p[2] = (byte)((byte)tx | (byte)((byte)((r >> (int)(6))) & (byte)maskx));
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
            return append(p, (byte)((byte)t2 | (byte)((r >> (int)(6)))), (byte)((byte)tx | (byte)((byte)r & (byte)maskx)));
        }
        case {} when (i < surrogateMin) || (surrogateMax < i && i <= rune3Max): {
            return append(p, (byte)((byte)t3 | (byte)((r >> (int)(12)))), (byte)((byte)tx | (byte)((byte)((r >> (int)(6))) & (byte)maskx)), (byte)((byte)tx | (byte)((byte)r & (byte)maskx)));
        }
        case {} when i > rune3Max && i <= MaxRune: {
            return append(p, (byte)((byte)t4 | (byte)((r >> (int)(18)))), (byte)((byte)tx | (byte)((byte)((r >> (int)(12))) & (byte)maskx)), (byte)((byte)tx | (byte)((byte)((r >> (int)(6))) & (byte)maskx)), (byte)((byte)tx | (byte)((byte)r & (byte)maskx)));
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
    nint n = default!;
    for (; n < np; n++) {
        {
            var c = p[n]; if (c >= RuneSelf) {
                // non-ASCII slow path
                return n + RuneCountInString(((@string)(p.slice(n))));
            }
        }
    }
    return n;
}

// RuneCountInString is like [RuneCount] but its input is a string.
[GoStr] public static nint /*n*/ RuneCountInString(sstring s) {
    nint n = default!;

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

// Valid reports whether p consists entirely of valid UTF-8-encoded runes.
public static bool Valid(slice<byte> p) {
    // This optimization avoids the need to recompute the capacity
    // when generating code for p[8:], bringing it to parity with
    // ValidString, which was 20% faster on long ASCII strings.
    p = p.slice(0, len(p), len(p));
    // Fast path. Check for and skip 8 bytes of ASCII characters per iteration.
    while (len(p) >= 8) {
        // Combining two 32 bit loads allows the same code to be used
        // for 32 and 64 bit platforms.
        // The compiler can generate a 32bit load for first32 and second32
        // on many platforms. See test/codegen/memcombine.go.
        var first32 = (uint32)((uint32)((uint32)((uint32)p[0] | ((uint32)p[1] << (int)(8))) | ((uint32)p[2] << (int)(16))) | ((uint32)p[3] << (int)(24)));
        var second32 = (uint32)((uint32)((uint32)((uint32)p[4] | ((uint32)p[5] << (int)(8))) | ((uint32)p[6] << (int)(16))) | ((uint32)p[7] << (int)(24)));
        if ((uint32)(((uint32)(first32 | second32)) & 0x80808080U) != 0) {
            // Found a non ASCII byte (>= RuneSelf).
            break;
        }
        p = p[8..];
    }
    nint n = len(p);
    for (nint i = 0; i < n; ) {
        var pi = p[i];
        if (pi < RuneSelf) {
            i++;
            continue;
        }
        var x = first[pi];
        if (x == xx) {
            return false; // Illegal starter byte.
        }
        nint size = (nint)((uint8)(x & 7));
        if (i + size > n) {
            return false; // Short or invalid.
        }
        var accept = acceptRanges[(x >> (int)(4))];
        {
            var c = p[i + 1]; if (c < accept.lo || accept.hi < c){
                return false;
            } else 
            if (size == 2){
            } else 
            {
                var cΔ1 = p[i + 2]; if (cΔ1 < locb || hicb < cΔ1){
                    return false;
                } else 
                if (size == 3){
                } else 
                {
                    var cΔ2 = p[i + 3]; if (cΔ2 < locb || hicb < cΔ2) {
                        return false;
                    }
                }
            }
        }
        i += size;
    }
    return true;
}

// ValidString reports whether s consists entirely of valid UTF-8-encoded runes.
public static bool ValidString(@string s) {
    // Fast path. Check for and skip 8 bytes of ASCII characters per iteration.
    while (len(s) >= 8) {
        // Combining two 32 bit loads allows the same code to be used
        // for 32 and 64 bit platforms.
        // The compiler can generate a 32bit load for first32 and second32
        // on many platforms. See test/codegen/memcombine.go.
        var first32 = (uint32)((uint32)((uint32)((uint32)s[0] | ((uint32)s[1] << (int)(8))) | ((uint32)s[2] << (int)(16))) | ((uint32)s[3] << (int)(24)));
        var second32 = (uint32)((uint32)((uint32)((uint32)s[4] | ((uint32)s[5] << (int)(8))) | ((uint32)s[6] << (int)(16))) | ((uint32)s[7] << (int)(24)));
        if ((uint32)(((uint32)(first32 | second32)) & 0x80808080U) != 0) {
            // Found a non ASCII byte (>= RuneSelf).
            break;
        }
        s = s[8..];
    }
    nint n = len(s);
    for (nint i = 0; i < n; ) {
        var si = s[i];
        if (si < RuneSelf) {
            i++;
            continue;
        }
        var x = first[si];
        if (x == xx) {
            return false; // Illegal starter byte.
        }
        nint size = (nint)((uint8)(x & 7));
        if (i + size > n) {
            return false; // Short or invalid.
        }
        var accept = acceptRanges[(x >> (int)(4))];
        {
            var c = s[i + 1]; if (c < accept.lo || accept.hi < c){
                return false;
            } else 
            if (size == 2){
            } else 
            {
                var cΔ1 = s[i + 2]; if (cΔ1 < locb || hicb < cΔ1){
                    return false;
                } else 
                if (size == 3){
                } else 
                {
                    var cΔ2 = s[i + 3]; if (cΔ2 < locb || hicb < cΔ2) {
                        return false;
                    }
                }
            }
        }
        i += size;
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
