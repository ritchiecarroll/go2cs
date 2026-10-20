// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package byteorder provides functions for decoding and encoding
// little and big endian integer types from/to byte slices.
namespace go.@internal;

partial class byteorder_package {

public static uint16 LEUint16(slice<byte> b) {
    _ = b[1]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint16)((uint16)b[0] | (uint16)((uint16)b[1] << 8));
}

public static void LEPutUint16(slice<byte> b, uint16 v) {
    _ = b[1]; // early bounds check to guarantee safety of writes below
    b[0] = (byte)v;
    b[1] = (byte)(v >> 8);
}

public static slice<byte> LEAppendUint16(slice<byte> b, uint16 v) {
    return append(b,
        (byte)v,
        (byte)(v >> 8));
}

public static uint32 LEUint32(slice<byte> b) {
    _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint32)b[0] | (uint32)b[1] << 8 | (uint32)b[2] << 16 | (uint32)b[3] << 24;
}

public static void LEPutUint32(slice<byte> b, uint32 v) {
    _ = b[3]; // early bounds check to guarantee safety of writes below
    b[0] = (byte)v;
    b[1] = (byte)(v >> 8);
    b[2] = (byte)(v >> 16);
    b[3] = (byte)(v >> 24);
}

public static slice<byte> LEAppendUint32(slice<byte> b, uint32 v) {
    return append(b,
        (byte)v,
        (byte)(v >> 8),
        (byte)(v >> 16),
        (byte)(v >> 24));
}

public static uint64 LEUint64(slice<byte> b) {
    _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint64)b[0] | (uint64)b[1] << 8 | (uint64)b[2] << 16 | (uint64)b[3] << 24 | (uint64)b[4] << 32 | (uint64)b[5] << 40 | (uint64)b[6] << 48 | (uint64)b[7] << 56;
}

public static void LEPutUint64(slice<byte> b, uint64 v) {
    _ = b[7]; // early bounds check to guarantee safety of writes below
    b[0] = (byte)v;
    b[1] = (byte)(v >> 8);
    b[2] = (byte)(v >> 16);
    b[3] = (byte)(v >> 24);
    b[4] = (byte)(v >> 32);
    b[5] = (byte)(v >> 40);
    b[6] = (byte)(v >> 48);
    b[7] = (byte)(v >> 56);
}

public static slice<byte> LEAppendUint64(slice<byte> b, uint64 v) {
    return append(b,
        (byte)v,
        (byte)(v >> 8),
        (byte)(v >> 16),
        (byte)(v >> 24),
        (byte)(v >> 32),
        (byte)(v >> 40),
        (byte)(v >> 48),
        (byte)(v >> 56));
}

public static uint16 BEUint16(slice<byte> b) {
    _ = b[1]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint16)((uint16)b[1] | (uint16)((uint16)b[0] << 8));
}

public static void BEPutUint16(slice<byte> b, uint16 v) {
    _ = b[1]; // early bounds check to guarantee safety of writes below
    b[0] = (byte)(v >> 8);
    b[1] = (byte)v;
}

public static slice<byte> BEAppendUint16(slice<byte> b, uint16 v) {
    return append(b,
        (byte)(v >> 8),
        (byte)v);
}

public static uint32 BEUint32(slice<byte> b) {
    _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint32)b[3] | (uint32)b[2] << 8 | (uint32)b[1] << 16 | (uint32)b[0] << 24;
}

public static void BEPutUint32(slice<byte> b, uint32 v) {
    _ = b[3]; // early bounds check to guarantee safety of writes below
    b[0] = (byte)(v >> 24);
    b[1] = (byte)(v >> 16);
    b[2] = (byte)(v >> 8);
    b[3] = (byte)v;
}

public static slice<byte> BEAppendUint32(slice<byte> b, uint32 v) {
    return append(b,
        (byte)(v >> 24),
        (byte)(v >> 16),
        (byte)(v >> 8),
        (byte)v);
}

public static uint64 BEUint64(slice<byte> b) {
    _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint64)b[7] | (uint64)b[6] << 8 | (uint64)b[5] << 16 | (uint64)b[4] << 24 | (uint64)b[3] << 32 | (uint64)b[2] << 40 | (uint64)b[1] << 48 | (uint64)b[0] << 56;
}

public static void BEPutUint64(slice<byte> b, uint64 v) {
    _ = b[7]; // early bounds check to guarantee safety of writes below
    b[0] = (byte)(v >> 56);
    b[1] = (byte)(v >> 48);
    b[2] = (byte)(v >> 40);
    b[3] = (byte)(v >> 32);
    b[4] = (byte)(v >> 24);
    b[5] = (byte)(v >> 16);
    b[6] = (byte)(v >> 8);
    b[7] = (byte)v;
}

public static slice<byte> BEAppendUint64(slice<byte> b, uint64 v) {
    return append(b,
        (byte)(v >> 56),
        (byte)(v >> 48),
        (byte)(v >> 40),
        (byte)(v >> 32),
        (byte)(v >> 24),
        (byte)(v >> 16),
        (byte)(v >> 8),
        (byte)v);
}

} // end byteorder_package
