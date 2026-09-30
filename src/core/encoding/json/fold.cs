// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class json_package {

// foldName returns a folded string such that foldName(x) == foldName(y)
// is identical to bytes.EqualFold(x, y).
internal static slice<byte> foldName(slice<byte> @in) {
    // This is inlinable to take advantage of "function outlining".
    array<byte> arr = new(32);               // large enough for most JSON names
    return appendFoldedName(arr[..0], @in);
}

internal static slice<byte> appendFoldedName(slice<byte> @out, slice<byte> @in) {
    for (nint i = 0; i < len(@in); ) {
        // Handle single-byte ASCII.
        {
            var c = @in[i]; if (c < utf8.RuneSelf) {
                if ((rune)'a' <= c && c <= (rune)'z') {
                    c -= (byte)((rune)'a' - (rune)'A');
                }
                @out = append(@out, c);
                i++;
                continue;
            }
        }
        // Handle multi-byte Unicode.
        var (r, n) = utf8.DecodeRune(@in.slice(i));
        @out = utf8.AppendRune(@out, foldRune(r));
        i += n;
    }
    return @out;
}

// foldRune is returns the smallest rune for all runes in the same fold set.
internal static rune foldRune(rune r) {
    while (ᐧ) {
        var r2 = unicode.SimpleFold(r);
        if (r2 <= r) {
            return r2;
        }
        r = r2;
    }
}

} // end json_package
