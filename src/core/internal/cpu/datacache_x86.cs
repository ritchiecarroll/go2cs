// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build 386 || amd64
namespace go.@internal;

partial class cpu_package {

// DataCacheSizes returns the size of each data cache from lowest
// level in the hierarchy to highest.
//
// Unlike other parts of this package's public API, it is not safe
// to reference early in runtime initialization because it allocates.
// It's intended for testing only.
public static slice<uintptr> DataCacheSizes() {
    var (maxFunctionInformation, ebx0, ecx0, edx0) = cpuid(0, 0);
    if (maxFunctionInformation < 1) {
        return default;
    }

    switch (ᐧ) {
    // Check for "GenuineIntel"
    case {} when ebx0 == 0x756E6547 && ecx0 == 0x6C65746E && edx0 == 0x49656E69: {
        return getDataCacheSizesIntel(maxFunctionInformation);
    }
    // Check for "AuthenticAMD"
    case {} when ebx0 == 0x68747541 && ecx0 == 0x444D4163 && edx0 == 0x69746E65: {
        return getDataCacheSizesAMD();
    }}
    return default;
}

internal static uint32 extractBits(uint32 arg, nint l, nint r) {
    if (l > r) {
        throw panic("bad bit range");
    }
    return (arg.Rsh((int64)(l))) & ((((uint32)1).Lsh((int64)(r - l + 1))) - 1);
}

internal static slice<uintptr> getDataCacheSizesIntel(uint32 maxID) {
    var cachesᴷ = new stackAppend<uintptr>(moves: true);

    // Constants for cache types
    UntypedInt noCache = 0;
    const uint32 dataCache = 1;
    UntypedInt instructionCache = 2;
    const uint32 unifiedCache = 3;
    if (maxID < 4) {
        return default;
    }

    // Iterate through CPUID leaf 4 (deterministic cache parameters)
    slice<uintptr> caches = default;
    for (var i = (uint32)0; i < 0xFFFF; i++) {
        var (eax, ebx, ecx, _) = cpuid(4, i);

        var cacheType = eax & 0xF; // EAX bits 4-0: Cache Type
        if (cacheType == 0) {
            break;
        }

        // Report only data caches.
        if (!(cacheType == dataCache || cacheType == unifiedCache)) {
            continue;
        }

        // Guaranteed to always start counting from 1.
        var level = (eax >> 5) & 0x7;

        var lineSize = extractBits(ebx, 0, 11) + 1;               // Bits 11-0: Line size in bytes - 1
        var partitions = extractBits(ebx, 12, 21) + 1;            // Bits 21-12: Physical line partitions - 1
        var ways = extractBits(ebx, 22, 31) + 1;                  // Bits 31-22: Ways of associativity - 1
        var sets = (uint64)ecx + 1;                               // Number of sets - 1
        var size = (uint64)(ways * partitions * lineSize) * sets; // Calculate cache size in bytes

        caches = append(cachesᴷ, caches, (uintptr)size);

        // If we see more than one cache described per level, or they appear
        // out of order, crash.
        //
        // Going by the SDM, it's not clear whether this is actually possible,
        // so this code is purely defensive.
        if (level != (uint32)len(caches)) {
            throw panic("expected levels to be in order and for there to be one data/unified cache per level");
        }
    }
    caches = move2heap(cachesᴷ, caches);
    return caches;
}

internal static slice<uintptr> getDataCacheSizesAMD() {
    var (maxExtendedFunctionInformation, _, _, _) = cpuid(0x80000000U, 0);
    if (maxExtendedFunctionInformation < 0x80000006U) {
        return default;
    }

    slice<uintptr> caches = default;

    var (_, _, ecx5, _) = cpuid(0x80000005U, 0);
    var (_, _, ecx6, edx6) = cpuid(0x80000006U, 0);

    // The size is return in kb, turning into bytes.
    var l1dSize = (uintptr)(extractBits(ecx5, 24, 31) << 10);
    caches = append(caches, l1dSize);

    // Check that L2 cache is present.
    {
        var l2Assoc = extractBits(ecx6, 12, 15); if (l2Assoc == 0) {
            return caches;
        }
    }
    var l2Size = (uintptr)(extractBits(ecx6, 16, 31) << 10);
    caches = append(caches, l2Size);

    // Check that L3 cache is present.
    {
        var l3Assoc = extractBits(edx6, 12, 15); if (l3Assoc == 0) {
            return caches;
        }
    }
    // Specifies the L3 cache size is within the following range:
    // (L3Size[31:18] * 512KB) <= L3 cache size < ((L3Size[31:18]+1) * 512KB).
    var l3Size = (uintptr)(extractBits(edx6, 18, 31) * ((uint32)512 << 10));
    caches = append(caches, l3Size);

    return caches;
}

} // end cpu_package
