// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package utf16 implements encoding and decoding of UTF-16 sequences.
namespace go.unicode;

partial class utf16_package {

// The conditions replacementChar==unicode.ReplacementChar and
// maxRune==unicode.MaxRune are verified in the tests.
// Defining them locally avoids this package depending on package unicode.
internal static UntypedInt replacementChar => /* '\uFFFD' */ 65533; // Unicode replacement character
internal static UntypedInt maxRune => /* '\U0010FFFF' */ 1114111; // Maximum valid Unicode code point.

internal static UntypedInt surr1 => 0xd800;
internal static UntypedInt surr2 => 0xdc00;
internal static UntypedInt surr3 => 0xe000;
internal static UntypedInt surrSelf => 0x10000;

// IsSurrogate reports whether the specified Unicode code point
// can appear in a surrogate pair.
public static bool IsSurrogate(rune r) {
    return surr1 <= r && r < surr3;
}

// DecodeRune returns the UTF-16 decoding of a surrogate pair.
// If the pair is not a valid UTF-16 surrogate pair, DecodeRune returns
// the Unicode replacement code point U+FFFD.
public static rune DecodeRune(rune r1, rune r2) {
    if (surr1 <= r1 && r1 < surr2 && surr2 <= r2 && r2 < surr3) {
        return (rune)(((r1 - (rune)surr1) << (int)(10)) | (r2 - (rune)surr2)) + (rune)surrSelf;
    }
    return replacementChar;
}

// EncodeRune returns the UTF-16 surrogate pair r1, r2 for the given rune.
// If the rune is not a valid Unicode code point or does not need encoding,
// EncodeRune returns U+FFFD, U+FFFD.
public static (rune r1, rune r2) EncodeRune(rune r) {
    if (r < surrSelf || r > maxRune) {
        return (replacementChar, replacementChar);
    }
    r -= surrSelf;
    return ((rune)surr1 + (rune)(((r >> (int)(10))) & 0x3ff), (rune)surr2 + (rune)(r & 0x3ff));
}

// RuneLen returns the number of 16-bit words in the UTF-16 encoding of the rune.
// It returns -1 if the rune is not a valid value to encode in UTF-16.
public static nint RuneLen(rune r) {
    switch (ᐧ) {
    case {} when (0 <= r && r < surr1) || (surr3 <= r && r < surrSelf): {
        return 1;
    }
    case {} when surrSelf <= r && r <= maxRune: {
        return 2;
    }
    default: {
        return -1;
    }}

}

// Encode returns the UTF-16 encoding of the Unicode code point sequence s.
public static slice<uint16> Encode(slice<rune> s) {
    nint n = len(s);
    foreach (var (_, v) in s) {
        if (v >= surrSelf) {
            n++;
        }
    }
    var a = new slice<uint16>(n);
    n = 0;
    foreach (var (_, v) in s) {
        switch (RuneLen(v)) {
        case 1: {
            a[n] = (uint16)v;
            n++;
            break;
        }
        case 2: {
            var (r1, r2) = EncodeRune(v);
            a[n] = (uint16)r1;
            a[n + 1] = (uint16)r2;
            n += 2;
            break;
        }
        default: {
            a[n] = (uint16)replacementChar;
            n++;
            break;
        }}

    }
    // normal rune
    // needs surrogate sequence
    return a.slice(0, n);
}

// AppendRune appends the UTF-16 encoding of the Unicode code point r
// to the end of p and returns the extended buffer. If the rune is not
// a valid Unicode code point, it appends the encoding of U+FFFD.
public static slice<uint16> AppendRune(slice<uint16> a, rune r) {
    // This function is inlineable for fast handling of ASCII.
    switch (ᐧ) {
    case {} when (0 <= r && r < surr1) || (surr3 <= r && r < surrSelf): {
        return append(a, // normal rune
 (uint16)r);
    }
    case {} when surrSelf <= r && r <= maxRune: {
        var (r1, r2) = EncodeRune(r);
        return append(a, // needs surrogate sequence
 (uint16)r1, (uint16)r2);
    }}

    return append(a, (uint16)(replacementChar));
}

// Decode returns the Unicode code point sequence represented
// by the UTF-16 encoding s.
public static slice<rune> Decode(slice<uint16> s) {
    // Preallocate capacity to hold up to 64 runes.
    // Decode inlines, so the allocation can live on the stack.
    var buf = new slice<rune>(0, 64);
    return decode(s, buf);
}

// decode appends to buf the Unicode code point sequence represented
// by the UTF-16 encoding s and return the extended buffer.
internal static slice<rune> decode(slice<uint16> s, slice<rune> buf) {
    for (nint i = 0; i < len(s); i++) {
        rune ar = default!;
        {
            var r = s[i];
            switch (ᐧ) {
            case {} when (r < surr1) || (surr3 <= r): {
                ar = (rune)r;
                break;
            }
            case {} when surr1 <= r && r < surr2 && i + 1 < len(s) && surr2 <= s[i + 1] && s[i + 1] < surr3: {
                ar = DecodeRune((rune)r, // normal rune
 // valid surrogate sequence
 (rune)s[i + 1]);
                i++;
                break;
            }
            default: {
                ar = replacementChar;
                break;
            }}
        }

        // invalid surrogate sequence
        buf = append(buf, ar);
    }
    return buf;
}

} // end utf16_package
