// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go.@internal;

using byteorder = go.@internal.byteorder_package;
using go.@internal;

partial class routebsd_package {

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
    uint64 Uint64(slice<byte> _);
}

partial struct binaryLittleEndian {
}

internal static uint16 Uint16(this binaryLittleEndian _, slice<byte> b) {
    return byteorder.LEUint16(b);
}

internal static uint32 Uint32(this binaryLittleEndian _, slice<byte> b) {
    return byteorder.LEUint32(b);
}

internal static uint64 Uint64(this binaryLittleEndian _, slice<byte> b) {
    return byteorder.LEUint64(b);
}

partial struct binaryBigEndian {
}

internal static uint16 Uint16(this binaryBigEndian _, slice<byte> b) {
    return byteorder.BEUint16(b);
}

internal static uint32 Uint32(this binaryBigEndian _, slice<byte> b) {
    return byteorder.BEUint32(b);
}

internal static uint64 Uint64(this binaryBigEndian _, slice<byte> b) {
    return byteorder.BEUint64(b);
}

} // end routebsd_package
