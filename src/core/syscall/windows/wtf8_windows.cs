// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Windows UTF-16 strings can contain unpaired surrogates, which can't be
// decoded into a valid UTF-8 string. This file defines a set of functions
// that can be used to encode and decode potentially ill-formed UTF-16 strings
// by using the [the WTF-8 encoding](https://simonsapin.github.io/wtf-8/).
//
// WTF-8 is a strict superset of UTF-8, i.e. any string that is
// well-formed in UTF-8 is also well-formed in WTF-8 and the content
// is unchanged. Also, the conversion never fails and is lossless.
//
// The benefit of using WTF-8 instead of UTF-8 when decoding a UTF-16 string
// is that the conversion is lossless even for ill-formed UTF-16 strings.
// This property allows to read an ill-formed UTF-16 string, convert it
// to a Go string, and convert it back to the same original UTF-16 string.
//
// See go.dev/issues/59971 for more info.
namespace go;

using utf16 = unicode.utf16_package;
using utf8 = unicode.utf8_package;
using unicode;

partial class syscall_package {

internal static UntypedInt surr1 => 0xd800;
internal static UntypedInt surr2 => 0xdc00;
internal static UntypedInt surr3 => 0xe000;
internal static UntypedInt tx => 0b10000000;
internal static UntypedInt t3 => 0b11100000;
internal static UntypedInt maskx => 0b00111111;
internal static UntypedInt mask3 => 0b00001111;
internal static UntypedInt rune1Max => /* 1<<7 - 1 */ 127;
internal static UntypedInt rune2Max => /* 1<<11 - 1 */ 2047;

// encodeWTF16 returns the potentially ill-formed
// UTF-16 encoding of s.
internal static slice<uint16> encodeWTF16(@string s, slice<uint16> buf) {
    for (nint i = 0; i < len(s); ) {
        // Cannot use 'for range s' because it expects valid
        // UTF-8 runes.
        var (r, size) = utf8.DecodeRuneInString(s.slice(i));
        if (r == utf8.RuneError) {
            // Check if s[i:] contains a valid WTF-8 encoded surrogate.
            {
                @string sc = s.slice(i); if (len(sc) >= 3 && sc[0] == 0xED && 0xA0 <= sc[1] && sc[1] <= 0xBF && 0x80 <= sc[2] && sc[2] <= 0xBF) {
                    r = ((rune)((byte)(sc[0] & (byte)mask3)) << (int)(12)) + ((rune)((byte)(sc[1] & (byte)maskx)) << (int)(6)) + (rune)((byte)(sc[2] & (byte)maskx));
                    buf = append(buf, (uint16)r);
                    i += 3;
                    continue;
                }
            }
        }
        i += size;
        buf = utf16.AppendRune(buf, r);
    }
    return buf;
}

// decodeWTF16 returns the WTF-8 encoding of
// the potentially ill-formed UTF-16 s.
internal static slice<byte> decodeWTF16(slice<uint16> s, slice<byte> buf) {
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
                ar = utf16.DecodeRune((rune)r, // normal rune
 // valid surrogate sequence
 (rune)s[i + 1]);
                i++;
                break;
            }
            default: {
                ar = (rune)r;
                if (ar > utf8.MaxRune) {
                    // WTF-8 fallback.
                    // This only handles the 3-byte case of utf8.AppendRune,
                    // as surrogates always fall in that case.
                    ar = utf8.RuneError;
                }
                buf = append(buf, (byte)((byte)t3 | (byte)((ar >> (int)(12)))), (byte)((byte)tx | (byte)((byte)((ar >> (int)(6))) & (byte)maskx)), (byte)((byte)tx | (byte)((byte)ar & (byte)maskx)));
                continue;
                break;
            }}
        }

        buf = utf8.AppendRune(buf, ar);
    }
    return buf;
}

} // end syscall_package
