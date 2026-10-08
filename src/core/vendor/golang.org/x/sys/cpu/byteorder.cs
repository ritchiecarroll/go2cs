// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.sys;

using runtime = runtime_package;

partial class cpu_package {

// byteOrder is a subset of encoding/binary.ByteOrder.
partial interface byteOrder {
    uint32 Uint32(slice<byte> _);
    uint64 Uint64(slice<byte> _);
}

partial struct littleEndian {
}

partial struct bigEndian {
}

internal static uint32 Uint32(this littleEndian _Δp0, slice<byte> b) {
    _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint32)((uint32)((uint32)((uint32)b[0] | ((uint32)b[1] << (int)(8))) | ((uint32)b[2] << (int)(16))) | ((uint32)b[3] << (int)(24)));
}

internal static uint64 Uint64(this littleEndian _Δp0, slice<byte> b) {
    _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)b[0] | ((uint64)b[1] << (int)(8))) | ((uint64)b[2] << (int)(16))) | ((uint64)b[3] << (int)(24))) | ((uint64)b[4] << (int)(32))) | ((uint64)b[5] << (int)(40))) | ((uint64)b[6] << (int)(48))) | ((uint64)b[7] << (int)(56)));
}

internal static uint32 Uint32(this bigEndian _Δp0, slice<byte> b) {
    _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint32)((uint32)((uint32)((uint32)b[3] | ((uint32)b[2] << (int)(8))) | ((uint32)b[1] << (int)(16))) | ((uint32)b[0] << (int)(24)));
}

internal static uint64 Uint64(this bigEndian _Δp0, slice<byte> b) {
    _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)b[7] | ((uint64)b[6] << (int)(8))) | ((uint64)b[5] << (int)(16))) | ((uint64)b[4] << (int)(24))) | ((uint64)b[3] << (int)(32))) | ((uint64)b[2] << (int)(40))) | ((uint64)b[1] << (int)(48))) | ((uint64)b[0] << (int)(56)));
}

// hostByteOrder returns littleEndian on little-endian machines and
// bigEndian on big-endian machines.
internal static byteOrder hostByteOrder() {
    var exprᴛ1 = runtime.GOARCH;
    if (exprᴛ1 == "386"u8 || exprᴛ1 == "amd64"u8 || exprᴛ1 == "amd64p32"u8 || exprᴛ1 == "alpha"u8 || exprᴛ1 == "arm"u8 || exprᴛ1 == "arm64"u8 || exprᴛ1 == "loong64"u8 || exprᴛ1 == "mipsle"u8 || exprᴛ1 == "mips64le"u8 || exprᴛ1 == "mips64p32le"u8 || exprᴛ1 == "nios2"u8 || exprᴛ1 == "ppc64le"u8 || exprᴛ1 == "riscv"u8 || exprᴛ1 == "riscv64"u8 || exprᴛ1 == "sh"u8) {
        return new littleEndian(nil);
    }
    if (exprᴛ1 == "armbe"u8 || exprᴛ1 == "arm64be"u8 || exprᴛ1 == "m68k"u8 || exprᴛ1 == "mips"u8 || exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64p32"u8 || exprᴛ1 == "ppc"u8 || exprᴛ1 == "ppc64"u8 || exprᴛ1 == "s390"u8 || exprᴛ1 == "s390x"u8 || exprᴛ1 == "shbe"u8 || exprᴛ1 == "sparc"u8 || exprᴛ1 == "sparc64"u8) {
        return new bigEndian(nil);
    }

    throw panic("unknown architecture");
}

} // end cpu_package
