// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go.vendor.golang.org.x.net;

partial class route_package {

// This file contains duplicates of encoding/binary package.
//
// This package is supposed to be used by the net package of standard
// library. Therefore the package set used in the package must be the
// same as net package.
internal static binaryLittleEndian littleEndian;
internal static binaryBigEndian bigEndian;

partial interface binaryByteOrder {
    uint16 Uint16(slice<byte> _);
    uint32 Uint32(slice<byte> _);
    void PutUint16(slice<byte> _Δp0, uint16 _Δp1);
    void PutUint32(slice<byte> _Δp0, uint32 _Δp1);
    uint64 Uint64(slice<byte> _);
}

partial struct binaryLittleEndian {
}

internal static uint16 Uint16(this binaryLittleEndian _Δp0, slice<byte> b) {
    _ = b[1]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint16)((uint16)b[0] | (uint16)((uint16)b[1] << (int)(8)));
}

internal static void PutUint16(this binaryLittleEndian _Δp0, slice<byte> b, uint16 v) {
    _ = b[1]; // early bounds check to guarantee safety of writes below
    b[0] = (byte)v;
    b[1] = (byte)((v >> (int)(8)));
}

internal static uint32 Uint32(this binaryLittleEndian _Δp0, slice<byte> b) {
    _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint32)((uint32)((uint32)((uint32)b[0] | ((uint32)b[1] << (int)(8))) | ((uint32)b[2] << (int)(16))) | ((uint32)b[3] << (int)(24)));
}

internal static void PutUint32(this binaryLittleEndian _Δp0, slice<byte> b, uint32 v) {
    _ = b[3]; // early bounds check to guarantee safety of writes below
    b[0] = (byte)v;
    b[1] = (byte)((v >> (int)(8)));
    b[2] = (byte)((v >> (int)(16)));
    b[3] = (byte)((v >> (int)(24)));
}

internal static uint64 Uint64(this binaryLittleEndian _Δp0, slice<byte> b) {
    _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)b[0] | ((uint64)b[1] << (int)(8))) | ((uint64)b[2] << (int)(16))) | ((uint64)b[3] << (int)(24))) | ((uint64)b[4] << (int)(32))) | ((uint64)b[5] << (int)(40))) | ((uint64)b[6] << (int)(48))) | ((uint64)b[7] << (int)(56)));
}

partial struct binaryBigEndian {
}

internal static uint16 Uint16(this binaryBigEndian _Δp0, slice<byte> b) {
    _ = b[1]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint16)((uint16)b[1] | (uint16)((uint16)b[0] << (int)(8)));
}

internal static void PutUint16(this binaryBigEndian _Δp0, slice<byte> b, uint16 v) {
    _ = b[1]; // early bounds check to guarantee safety of writes below
    b[0] = (byte)((v >> (int)(8)));
    b[1] = (byte)v;
}

internal static uint32 Uint32(this binaryBigEndian _Δp0, slice<byte> b) {
    _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint32)((uint32)((uint32)((uint32)b[3] | ((uint32)b[2] << (int)(8))) | ((uint32)b[1] << (int)(16))) | ((uint32)b[0] << (int)(24)));
}

internal static void PutUint32(this binaryBigEndian _Δp0, slice<byte> b, uint32 v) {
    _ = b[3]; // early bounds check to guarantee safety of writes below
    b[0] = (byte)((v >> (int)(24)));
    b[1] = (byte)((v >> (int)(16)));
    b[2] = (byte)((v >> (int)(8)));
    b[3] = (byte)v;
}

internal static uint64 Uint64(this binaryBigEndian _Δp0, slice<byte> b) {
    _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)b[7] | ((uint64)b[6] << (int)(8))) | ((uint64)b[5] << (int)(16))) | ((uint64)b[4] << (int)(24))) | ((uint64)b[3] << (int)(32))) | ((uint64)b[2] << (int)(40))) | ((uint64)b[1] << (int)(48))) | ((uint64)b[0] << (int)(56)));
}

} // end route_package
