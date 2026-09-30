// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.text.unicode;

using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class norm_package {

[GoType] partial struct input {
    internal @string str;
    internal slice<byte> bytes;
}

internal static input inputBytes(slice<byte> str) {
    return new input(bytes: str);
}

internal static input inputString(@string str) {
    return new input(str: str);
}

[GoRecv] internal static void setBytes(this ref input @in, slice<byte> str) {
    @in.str = ""u8;
    @in.bytes = str;
}

[GoRecv] internal static void setString(this ref input @in, @string str) {
    @in.str = str;
    @in.bytes = default!;
}

[GoRecv] internal static byte _byte(this ref input @in, nint p) {
    if (@in.bytes == default!) {
        return @in.str[p];
    }
    return @in.bytes[p];
}

[GoRecv] internal static nint skipASCII(this ref input @in, nint p, nint max) {
    if (@in.bytes == default!){
        for (; p < max && @in.str[p] < utf8.RuneSelf; p++) {
        }
    } else {
        for (; p < max && @in.bytes[p] < utf8.RuneSelf; p++) {
        }
    }
    return p;
}

[GoRecv] internal static nint skipContinuationBytes(this ref input @in, nint p) {
    if (@in.bytes == default!){
        for (; p < len(@in.str) && !utf8.RuneStart(@in.str[p]); p++) {
        }
    } else {
        for (; p < len(@in.bytes) && !utf8.RuneStart(@in.bytes[p]); p++) {
        }
    }
    return p;
}

[GoRecv] internal static slice<byte> appendSlice(this ref input @in, slice<byte> buf, nint b, nint e) {
    if (@in.bytes != default!) {
        return appendꓸꓸꓸ(buf, @in.bytes.slice(b, e));
    }
    for (nint i = b; i < e; i++) {
        buf = append(buf, @in.str[i]);
    }
    return buf;
}

[GoRecv] internal static nint copySlice(this ref input @in, slice<byte> buf, nint b, nint e) {
    if (@in.bytes == default!) {
        return copy(buf, @in.str.slice(b, e));
    }
    return copy(buf, @in.bytes.slice(b, e));
}

[GoRecv] internal static (uint16, nint) charinfoNFC(this ref input @in, nint p) {
    if (@in.bytes == default!) {
        return nfcData.lookupString(@in.str.slice(p));
    }
    return nfcData.lookup(@in.bytes.slice(p));
}

[GoRecv] internal static (uint16, nint) charinfoNFKC(this ref input @in, nint p) {
    if (@in.bytes == default!) {
        return nfkcData.lookupString(@in.str.slice(p));
    }
    return nfkcData.lookup(@in.bytes.slice(p));
}

[GoRecv] internal static rune /*r*/ hangul(this ref input @in, nint p) {
    rune r = default!;

    nint size = default!;
    if (@in.bytes == default!){
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

} // end norm_package
