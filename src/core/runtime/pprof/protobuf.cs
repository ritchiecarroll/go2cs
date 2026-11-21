// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using static pprof_package;

partial class pprof_package {

// A protobuf is a simple protocol buffer encoder.
partial struct protobuf {
    internal slice<byte> data;
    internal array<byte> tmp = new(16);
    internal nint nest;
}

} partial class pprof_package {

[GoMethodBody("varint")] internal static void varintᴍ(ref protobuf b, uint64 x) {
    while (x >= 128) {
        b.data = append(b.data, (byte)((byte)x | 0x80));
        x >>= 7;
    }
    b.data = append(b.data, (byte)x);
}

[GoMethodBody("length")] internal static void lengthᴍ(ref protobuf b, nint tag, nint len) {
    b.varint((uint64)tag << 3 | 2);
    b.varint((uint64)len);
}

[GoMethodBody("uint64")] internal static void uint64ᴍ(ref protobuf b, nint tag, uint64 x) {
    // append varint to b.data
    b.varint((uint64)tag << 3 | 0);
    b.varint(x);
}

[GoMethodBody("uint64s")] internal static void uint64sᴍ(ref protobuf b, nint tag, slice<uint64> x) {
    if (len(x) > 2) {
        // Use packed encoding
        nint n1 = len(b.data);
        foreach (var (_, u) in range(x)) {
            b.varint(u);
        }
        nint n2 = len(b.data);
        b.length(tag, n2 - n1);
        nint n3 = len(b.data);
        copy(b.tmp[..], b.data.slice(n2, n3));
        copy(b.data.slice(n1 + (n3 - n2)), b.data.slice(n1, n2));
        copy(b.data.slice(n1), b.tmp.slice(0, n3 - n2));
        return;
    }
    foreach (var (_, u) in range(x)) {
        b.uint64(tag, u);
    }
}

[GoMethodBody("uint64Opt")] internal static void uint64Optᴍ(ref protobuf b, nint tag, uint64 x) {
    if (x == 0) {
        return;
    }
    b.uint64(tag, x);
}

[GoMethodBody("int64")] internal static void int64ᴍ(ref protobuf b, nint tag, int64 x) {
    var u = (uint64)x;
    b.uint64(tag, u);
}

[GoMethodBody("int64Opt")] internal static void int64Optᴍ(ref protobuf b, nint tag, int64 x) {
    if (x == 0) {
        return;
    }
    b.int64(tag, x);
}

[GoMethodBody("int64s")] internal static void int64sᴍ(ref protobuf b, nint tag, slice<int64> x) {
    if (len(x) > 2) {
        // Use packed encoding
        nint n1 = len(b.data);
        foreach (var (_, u) in range(x)) {
            b.varint((uint64)u);
        }
        nint n2 = len(b.data);
        b.length(tag, n2 - n1);
        nint n3 = len(b.data);
        copy(b.tmp[..], b.data.slice(n2, n3));
        copy(b.data.slice(n1 + (n3 - n2)), b.data.slice(n1, n2));
        copy(b.data.slice(n1), b.tmp.slice(0, n3 - n2));
        return;
    }
    foreach (var (_, u) in range(x)) {
        b.int64(tag, u);
    }
}

[GoMethodBody("string")] internal static void @stringᴍ(ref protobuf b, nint tag, @string x) {
    b.length(tag, len(x));
    b.data = append(b.data, x.ꓸꓸꓸ);
}

[GoMethodBody("strings")] internal static void stringsᴍ(ref protobuf b, nint tag, slice<@string> x) {
    foreach (var (_, s) in range(x)) {
        b.@string(tag, s);
    }
}

[GoMethodBody("stringOpt")] internal static void stringOptᴍ(ref protobuf b, nint tag, @string x) {
    if (x == ""u8) {
        return;
    }
    b.@string(tag, x);
}

[GoMethodBody("bool")] internal static void @boolᴍ(ref protobuf b, nint tag, bool x) {
    if (x) {
        b.uint64(tag, 1);
    } else {
        b.uint64(tag, 0);
    }
}

[GoMethodBody("boolOpt")] internal static void boolOptᴍ(ref protobuf b, nint tag, bool x) {
    if (!x) {
        return;
    }
    b.@bool(tag, x);
}

} partial class pprof_package {

partial struct msgOffset /*num:nint*/;

} partial class pprof_package {

[GoMethodBody("startMessage")] internal static msgOffset startMessageᴍ(ref protobuf b) {
    b.nest++;
    return (msgOffset)len(b.data);
}

[GoMethodBody("endMessage")] internal static void endMessageᴍ(ref protobuf b, nint tag, msgOffset start) {
    nint n1 = (nint)start;
    nint n2 = len(b.data);
    b.length(tag, n2 - n1);
    nint n3 = len(b.data);
    copy(b.tmp[..], b.data.slice(n2, n3));
    copy(b.data.slice(n1 + (n3 - n2)), b.data.slice(n1, n2));
    copy(b.data.slice(n1), b.tmp.slice(0, n3 - n2));
    b.nest--;
}

}
