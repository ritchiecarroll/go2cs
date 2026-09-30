// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

// Numbers fundamental to the encoding.
internal static UntypedInt runeError => /* '\uFFFD' */ 65533; // the "error" Rune or "Unicode replacement character"

internal static UntypedInt runeSelf => 0x80; // characters below runeSelf are represented as themselves in a single byte.

internal static UntypedInt maxRune => /* '\U0010FFFF' */ 1114111; // Maximum valid Unicode code point.

// Code points in the surrogate range are not valid for UTF-8.
internal static UntypedInt surrogateMin => 0xD800;

internal static UntypedInt surrogateMax => 0xDFFF;

internal static UntypedInt t1 => 0x00; // 0000 0000
internal static UntypedInt tx => 0x80; // 1000 0000
internal static UntypedInt t2 => 0xC0; // 1100 0000
internal static UntypedInt t3 => 0xE0; // 1110 0000
internal static UntypedInt t4 => 0xF0; // 1111 0000
internal static UntypedInt t5 => 0xF8; // 1111 1000
internal static UntypedInt maskx => 0x3F; // 0011 1111
internal static UntypedInt mask2 => 0x1F; // 0001 1111
internal static UntypedInt mask3 => 0x0F; // 0000 1111
internal static UntypedInt mask4 => 0x07; // 0000 0111
internal static UntypedInt rune1Max => /* 1<<7 - 1 */ 127;
internal static UntypedInt rune2Max => /* 1<<11 - 1 */ 2047;
internal static UntypedInt rune3Max => /* 1<<16 - 1 */ 65535;
internal static UntypedInt locb => 0x80; // 1000 0000
internal static UntypedInt hicb => 0xBF; // 1011 1111

// countrunes returns the number of runes in s.
internal static nint countrunes(@string s) {
    nint n = 0;
    foreach ((_, _) in s) {
        n++;
    }
    return n;
}

// decoderune returns the non-ASCII rune at the start of
// s[k:] and the index after the rune in s.
//
// decoderune assumes that caller has checked that
// the to be decoded rune is a non-ASCII rune.
//
// If the string appears to be incomplete or decoding problems
// are encountered (runeerror, k + 1) is returned to ensure
// progress when decoderune is used to iterate over a string.
internal static (rune r, nint pos) decoderune(@string s, nint k) {
    rune r = default!;
    nint pos = default!;

    pos = k;
    if (k >= len(s)) {
        return (runeError, k + 1);
    }
    s = s.slice(k);
    switch (ᐧ) {
    case {} when t2 <= s[0] && s[0] < t3: {
        if (len(s) > 1 && (locb <= s[1] && s[1] <= hicb)) {
            // 0080-07FF two byte sequence
            r = (rune)(((rune)((byte)(s[0] & (byte)mask2)) << (int)(6)) | (rune)((byte)(s[1] & (byte)maskx)));
            pos += 2;
            if (rune1Max < r) {
                return (r, pos);
            }
        }
        break;
    }
    case {} when t3 <= s[0] && s[0] < t4: {
        if (len(s) > 2 && (locb <= s[1] && s[1] <= hicb) && (locb <= s[2] && s[2] <= hicb)) {
            // 0800-FFFF three byte sequence
            r = (rune)((rune)(((rune)((byte)(s[0] & (byte)mask3)) << (int)(12)) | ((rune)((byte)(s[1] & (byte)maskx)) << (int)(6))) | (rune)((byte)(s[2] & (byte)maskx)));
            pos += 3;
            if (rune2Max < r && !(surrogateMin <= r && r <= surrogateMax)) {
                return (r, pos);
            }
        }
        break;
    }
    case {} when t4 <= s[0] && s[0] < t5: {
        if (len(s) > 3 && (locb <= s[1] && s[1] <= hicb) && (locb <= s[2] && s[2] <= hicb) && (locb <= s[3] && s[3] <= hicb)) {
            // 10000-1FFFFF four byte sequence
            r = (rune)((rune)((rune)(((rune)((byte)(s[0] & (byte)mask4)) << (int)(18)) | ((rune)((byte)(s[1] & (byte)maskx)) << (int)(12))) | ((rune)((byte)(s[2] & (byte)maskx)) << (int)(6))) | (rune)((byte)(s[3] & (byte)maskx)));
            pos += 4;
            if (rune3Max < r && r <= maxRune) {
                return (r, pos);
            }
        }
        break;
    }}

    return (runeError, k + 1);
}

// encoderune writes into p (which must be large enough) the UTF-8 encoding of the rune.
// It returns the number of bytes written.
internal static nint encoderune(slice<byte> Δp, rune r) {
    // Negative values are erroneous. Making it unsigned addresses the problem.
    {
        var i = (uint32)r;
        var matchᴛ1 = false;
        if (i <= rune1Max) { matchᴛ1 = true;
            Δp[0] = (byte)r;
            return 1;
        }
        if (i <= rune2Max) { matchᴛ1 = true;
            _ = Δp[1]; // eliminate bounds checks
            Δp[0] = (byte)((byte)t2 | (byte)((r >> (int)(6))));
            Δp[1] = (byte)((byte)tx | (byte)((byte)r & (byte)maskx));
            return 2;
        }
        if ((i > maxRune) || (surrogateMin <= i && i <= surrogateMax)) { matchᴛ1 = true;
            r = runeError;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (i <= rune3Max)) {
            _ = Δp[2]; // eliminate bounds checks
            Δp[0] = (byte)((byte)t3 | (byte)((r >> (int)(12))));
            Δp[1] = (byte)((byte)tx | (byte)((byte)((r >> (int)(6))) & (byte)maskx));
            Δp[2] = (byte)((byte)tx | (byte)((byte)r & (byte)maskx));
            return 3;
        }
        if (!matchᴛ1) { /* default: */
            _ = Δp[3]; // eliminate bounds checks
            Δp[0] = (byte)((byte)t4 | (byte)((r >> (int)(18))));
            Δp[1] = (byte)((byte)tx | (byte)((byte)((r >> (int)(12))) & (byte)maskx));
            Δp[2] = (byte)((byte)tx | (byte)((byte)((r >> (int)(6))) & (byte)maskx));
            Δp[3] = (byte)((byte)tx | (byte)((byte)r & (byte)maskx));
            return 4;
        }
        return default!;
    }

}

} // end runtime_package
