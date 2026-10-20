// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140deps;

using byteorder = go.@internal.byteorder_package;
using go.@internal;

partial class byteorder_package {

public static uint16 LEUint16(slice<byte> b) {
    return byteorder.LEUint16(b);
}

public static uint32 BEUint32(slice<byte> b) {
    return byteorder.BEUint32(b);
}

public static uint64 BEUint64(slice<byte> b) {
    return byteorder.BEUint64(b);
}

public static uint64 LEUint64(slice<byte> b) {
    return byteorder.LEUint64(b);
}

public static void BEPutUint16(slice<byte> b, uint16 v) {
    byteorder.BEPutUint16(b, v);
}

public static void BEPutUint32(slice<byte> b, uint32 v) {
    byteorder.BEPutUint32(b, v);
}

public static void BEPutUint64(slice<byte> b, uint64 v) {
    byteorder.BEPutUint64(b, v);
}

public static void LEPutUint16(slice<byte> b, uint16 v) {
    byteorder.LEPutUint16(b, v);
}

public static void LEPutUint64(slice<byte> b, uint64 v) {
    byteorder.LEPutUint64(b, v);
}

public static slice<byte> BEAppendUint16(slice<byte> b, uint16 v) {
    return byteorder.BEAppendUint16(b, v);
}

public static slice<byte> BEAppendUint32(slice<byte> b, uint32 v) {
    return byteorder.BEAppendUint32(b, v);
}

public static slice<byte> BEAppendUint64(slice<byte> b, uint64 v) {
    return byteorder.BEAppendUint64(b, v);
}

} // end byteorder_package
