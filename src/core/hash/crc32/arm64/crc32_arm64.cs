// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// ARM64-specific hardware-assisted CRC32 algorithms. See crc32.go for a
// description of the interface that each architecture-specific file
// implements.
namespace go.hash;

using cpu = @internal.cpu_package;
using @internal;

partial class crc32_package {

internal static partial uint32 castagnoliUpdate(uint32 crc, slice<byte> p);
internal static partial uint32 ieeeUpdate(uint32 crc, slice<byte> p);

internal static bool archAvailableCastagnoli() {
    return cpu.ARM64.HasCRC32;
}

internal static void archInitCastagnoli() {
    if (!cpu.ARM64.HasCRC32) {
        throw panic("arch-specific crc32 instruction for Castagnoli not available");
    }
}

internal static uint32 archUpdateCastagnoli(uint32 crc, slice<byte> p) {
    if (!cpu.ARM64.HasCRC32) {
        throw panic("arch-specific crc32 instruction for Castagnoli not available");
    }

    return ~castagnoliUpdate(~crc, p);
}

internal static bool archAvailableIEEE() {
    return cpu.ARM64.HasCRC32;
}

internal static void archInitIEEE() {
    if (!cpu.ARM64.HasCRC32) {
        throw panic("arch-specific crc32 instruction for IEEE not available");
    }
}

internal static uint32 archUpdateIEEE(uint32 crc, slice<byte> p) {
    if (!cpu.ARM64.HasCRC32) {
        throw panic("arch-specific crc32 instruction for IEEE not available");
    }

    return ~ieeeUpdate(~crc, p);
}

} // end crc32_package
