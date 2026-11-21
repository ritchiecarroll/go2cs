// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.text.unicode;

using utf8 = go.unicode.utf8_package;
using go.unicode;
using static norm_package;

partial class norm_package {

partial struct input {
    internal @string str;
    internal slice<byte> bytes;
}

internal static input inputBytes(slice<byte> str) {
    return new input(bytes: str);
}

internal static input inputString(@string str) {
    return new input(str: str);
}

} partial class norm_package {

[GoMethodBody("setBytes")] internal static void setBytesᴍ(ref input @in, slice<byte> str) {
    @in.str = ""u8;
    @in.bytes = str;
}

[GoMethodBody("setString")] internal static void setStringᴍ(ref input @in, @string str) {
    @in.str = str;
    @in.bytes = default;
}

[GoMethodBody("_byte")] internal static byte _byteᴍ(ref input @in, nint p) {
    if (@in.bytes == nil) {
        return @in.str[p];
    }
    return @in.bytes[p];
}

[GoMethodBody("skipASCII")] internal static nint skipASCIIᴍ(ref input @in, nint p, nint max) {
    if (@in.bytes == nil) {
        for (; p < max && @in.str[p] < utf8.RuneSelf; p++) {
        }
    } else {
        for (; p < max && @in.bytes[p] < utf8.RuneSelf; p++) {
        }
    }
    return p;
}

[GoMethodBody("skipContinuationBytes")] internal static nint skipContinuationBytesᴍ(ref input @in, nint p) {
    if (@in.bytes == nil) {
        for (; p < len(@in.str) && !utf8.RuneStart(@in.str[p]); p++) {
        }
    } else {
        for (; p < len(@in.bytes) && !utf8.RuneStart(@in.bytes[p]); p++) {
        }
    }
    return p;
}

[GoMethodBody("appendSlice")] internal static slice<byte> appendSliceᴍ(ref input @in, slice<byte> buf, nint b, nint e) {
    if (@in.bytes != nil) {
        return appendꓸꓸꓸ(buf, @in.bytes.slice(b, e));
    }
    for (nint i = b; i < e; i++) {
        buf = append(buf, @in.str[i]);
    }
    return buf;
}

[GoMethodBody("copySlice")] internal static nint copySliceᴍ(ref input @in, slice<byte> buf, nint b, nint e) {
    if (@in.bytes == nil) {
        return copy(buf, @in.str.slice(b, e));
    }
    return copy(buf, @in.bytes.slice(b, e));
}

[GoMethodBody("charinfoNFC")] internal static (uint16, nint) charinfoNFCᴍ(ref input @in, nint p) {
    if (@in.bytes == nil) {
        return nfcData.lookupString(@in.str.slice(p));
    }
    return nfcData.lookup(@in.bytes.slice(p));
}

[GoMethodBody("charinfoNFKC")] internal static (uint16, nint) charinfoNFKCᴍ(ref input @in, nint p) {
    if (@in.bytes == nil) {
        return nfkcData.lookupString(@in.str.slice(p));
    }
    return nfkcData.lookup(@in.bytes.slice(p));
}

[GoMethodBody("hangul")] internal static rune /*r*/ hangulᴍ(ref input @in, nint p) {
    rune r = default;

    nint size = default;
    if (@in.bytes == nil) {
        if (!isHangulString(@in.str.slice(p))) {
            return 0;
        }
        (r, size) = utf8.DecodeRuneInString(@in.str.slice(p));
    } else {
        if (!isHangul(@in.bytes.slice(p))) {
            return 0;
        }
        (r, size) = utf8.DecodeRune(@in.bytes.slice(p));
    }
    if (size != hangulUTF8Size) {
        return 0;
    }
    return r;
}

}
