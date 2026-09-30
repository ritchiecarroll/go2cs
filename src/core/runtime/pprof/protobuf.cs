// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

partial class pprof_package {

// A protobuf is a simple protocol buffer encoder.
[GoType] partial struct protobuf {
    internal slice<byte> data;
    internal array<byte> tmp = new(16);
    internal nint nest;
}

[GoRecv] internal static void varint(this ref protobuf b, uint64 x) {
    while (x >= 128) {
        b.data = append(b.data, (byte)((byte)x | 0x80));
        x >>= (int)(7);
    }
    b.data = append(b.data, (byte)x);
}

[GoRecv] internal static void length(this ref protobuf b, nint tag, nint len) {
    b.varint((uint64)(((uint64)tag << (int)(3)) | 2));
    b.varint((uint64)len);
}

[GoRecv] internal static void uint64(this ref protobuf b, nint tag, uint64 x) {
    // append varint to b.data
    b.varint((uint64)(((uint64)tag << (int)(3)) | 0));
    b.varint(x);
}

[GoRecv] internal static void uint64s(this ref protobuf b, nint tag, slice<uint64> x) {
    if (len(x) > 2) {
        // Use packed encoding
        nint n1 = len(b.data);
        foreach (var (_, u) in x) {
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
    foreach (var (_, u) in x) {
        b.uint64(tag, u);
    }
}

[GoRecv] internal static void uint64Opt(this ref protobuf b, nint tag, uint64 x) {
    if (x == 0) {
        return;
    }
    b.uint64(tag, x);
}

[GoRecv] internal static void int64(this ref protobuf b, nint tag, int64 x) {
    var u = (uint64)x;
    b.uint64(tag, u);
}

[GoRecv] internal static void int64Opt(this ref protobuf b, nint tag, int64 x) {
    if (x == 0) {
        return;
    }
    b.int64(tag, x);
}

[GoRecv] internal static void int64s(this ref protobuf b, nint tag, slice<int64> x) {
    if (len(x) > 2) {
        // Use packed encoding
        nint n1 = len(b.data);
        foreach (var (_, u) in x) {
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
    foreach (var (_, u) in x) {
        b.int64(tag, u);
    }
}

[GoRecv] internal static void @string(this ref protobuf b, nint tag, @string x) {
    b.length(tag, len(x));
    b.data = append(b.data, x.ꓸꓸꓸ);
}

[GoRecv] internal static void strings(this ref protobuf b, nint tag, slice<@string> x) {
    foreach (var (_, s) in x) {
        b.@string(tag, s);
    }
}

[GoRecv] internal static void stringOpt(this ref protobuf b, nint tag, @string x) {
    if (x == ""u8) {
        return;
    }
    b.@string(tag, x);
}

[GoRecv] internal static void @bool(this ref protobuf b, nint tag, bool x) {
    if (x){
        b.uint64(tag, 1);
    } else {
        b.uint64(tag, 0);
    }
}

[GoRecv] internal static void boolOpt(this ref protobuf b, nint tag, bool x) {
    if (!x) {
        return;
    }
    b.@bool(tag, x);
}

[GoType("num:nint")] partial struct msgOffset;

[GoRecv] internal static msgOffset startMessage(this ref protobuf b) {
    b.nest++;
    return ((msgOffset)len(b.data));
}

[GoRecv] internal static void endMessage(this ref protobuf b, nint tag, msgOffset start) {
    nint n1 = (nint)start;
    nint n2 = len(b.data);
    b.length(tag, n2 - n1);
    nint n3 = len(b.data);
    copy(b.tmp[..], b.data.slice(n2, n3));
    copy(b.data.slice(n1 + (n3 - n2)), b.data.slice(n1, n2));
    copy(b.data.slice(n1), b.tmp.slice(0, n3 - n2));
    b.nest--;
}

} // end pprof_package
