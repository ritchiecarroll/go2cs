// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using runtime = runtime_package;

partial class cpu_package {

internal static UntypedInt cacheLineSize => 64;

internal static void initOptions() {
    options = [
        new(Name: "adx", Feature: ᏑX86.of(X86ᴛ1.ᏑHasADX)),
        new(Name: "aes", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAES)),
        new(Name: "avx", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX)),
        new(Name: "avx2", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX2)),
        new(Name: "avx512", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512)),
        new(Name: "avx512f", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512F)),
        new(Name: "avx512cd", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512CD)),
        new(Name: "avx512er", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512ER)),
        new(Name: "avx512pf", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512PF)),
        new(Name: "avx512vl", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VL)),
        new(Name: "avx512bw", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512BW)),
        new(Name: "avx512dq", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512DQ)),
        new(Name: "avx512ifma", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512IFMA)),
        new(Name: "avx512vbmi", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VBMI)),
        new(Name: "avx512vnniw", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX5124VNNIW)),
        new(Name: "avx5124fmaps", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX5124FMAPS)),
        new(Name: "avx512vpopcntdq", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VPOPCNTDQ)),
        new(Name: "avx512vpclmulqdq", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VPCLMULQDQ)),
        new(Name: "avx512vnni", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VNNI)),
        new(Name: "avx512gfni", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512GFNI)),
        new(Name: "avx512vaes", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VAES)),
        new(Name: "avx512vbmi2", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VBMI2)),
        new(Name: "avx512bitalg", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512BITALG)),
        new(Name: "avx512bf16", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512BF16)),
        new(Name: "amxtile", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAMXTile)),
        new(Name: "amxint8", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAMXInt8)),
        new(Name: "amxbf16", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAMXBF16)),
        new(Name: "bmi1", Feature: ᏑX86.of(X86ᴛ1.ᏑHasBMI1)),
        new(Name: "bmi2", Feature: ᏑX86.of(X86ᴛ1.ᏑHasBMI2)),
        new(Name: "cx16", Feature: ᏑX86.of(X86ᴛ1.ᏑHasCX16)),
        new(Name: "erms", Feature: ᏑX86.of(X86ᴛ1.ᏑHasERMS)),
        new(Name: "f16c", Feature: ᏑX86.of(X86ᴛ1.ᏑHasF16C)),
        new(Name: "fma", Feature: ᏑX86.of(X86ᴛ1.ᏑHasFMA)),
        new(Name: "lahf", Feature: ᏑX86.of(X86ᴛ1.ᏑHasLAHF)),
        new(Name: "lzcnt", Feature: ᏑX86.of(X86ᴛ1.ᏑHasLZCNT)),
        new(Name: "movbe", Feature: ᏑX86.of(X86ᴛ1.ᏑHasMOVBE)),
        new(Name: "osxsave", Feature: ᏑX86.of(X86ᴛ1.ᏑHasOSXSAVE)),
        new(Name: "pclmulqdq", Feature: ᏑX86.of(X86ᴛ1.ᏑHasPCLMULQDQ)),
        new(Name: "popcnt", Feature: ᏑX86.of(X86ᴛ1.ᏑHasPOPCNT)),
        new(Name: "rdrand", Feature: ᏑX86.of(X86ᴛ1.ᏑHasRDRAND)),
        new(Name: "rdseed", Feature: ᏑX86.of(X86ᴛ1.ᏑHasRDSEED)),
        new(Name: "sse3", Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE3)),
        new(Name: "sse41", Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE41)),
        new(Name: "sse42", Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE42)),
        new(Name: "ssse3", Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSSE3)),
        new(Name: "avxifma", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVXIFMA)),
        new(Name: "avxvnni", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVXVNNI)),
        new(Name: "avxvnniint8", Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVXVNNIInt8)),
        new(Name: "xsave", Feature: ᏑX86.of(X86ᴛ1.ᏑHasXSAVE)),

        new(Name: "sse2", Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE2), Required: runtime.GOARCH == "amd64"u8)
    ];
}

internal static void archInit() {
    const uint32 cpuid_AVXVNNI = /* 1 << 4 */ 16;
    const uint32 cpuid_SSE3 = /* 1 << 0 */ 1;
    const uint32 cpuid_PCLMULQDQ = /* 1 << 1 */ 2;
    UntypedInt cpuid_AVX512VBMI = /* 1 << 1 */ 2;
    const uint32 cpuid_AVX512VBMI2 = /* 1 << 6 */ 64;
    const uint32 cpuid_SSSE3 = /* 1 << 9 */ 512;
    const uint32 cpuid_AVX512GFNI = /* 1 << 8 */ 256;
    const uint32 cpuid_AVX512VAES = /* 1 << 9 */ 512;
    const uint32 cpuid_AVX512VNNI = /* 1 << 11 */ 2048;
    const uint32 cpuid_AVX512BITALG = /* 1 << 12 */ 4096;
    const uint32 cpuid_FMA = /* 1 << 12 */ 4096;
    const uint32 cpuid_AVX512VPOPCNTDQ = /* 1 << 14 */ 16384;
    const uint32 cpuid_SSE41 = /* 1 << 19 */ 524288;
    const uint32 cpuid_SSE42 = /* 1 << 20 */ 1048576;
    const uint32 cpuid_MOVBE = /* 1 << 22 */ 4194304;
    const uint32 cpuid_POPCNT = /* 1 << 23 */ 8388608;
    const uint32 cpuid_AES = /* 1 << 25 */ 33554432;
    const uint32 cpuid_XSAVE = /* 1 << 26 */ 67108864;
    const uint32 cpuid_OSXSAVE = /* 1 << 27 */ 134217728;
    const uint32 cpuid_AVX = /* 1 << 28 */ 268435456;
    const uint32 cpuid_F16C = /* 1 << 29 */ 536870912;
    const uint32 cpuid_BMI1 = /* 1 << 3 */ 8;
    const uint32 cpuid_AVX2 = /* 1 << 5 */ 32;
    const uint32 cpuid_BMI2 = /* 1 << 8 */ 256;
    const uint32 cpuid_ERMS = /* 1 << 9 */ 512;
    const uint32 cpuid_AVX512F = /* 1 << 16 */ 65536;
    const uint32 cpuid_AVX512DQ = /* 1 << 17 */ 131072;
    const uint32 cpuid_ADX = /* 1 << 19 */ 524288;
    const uint32 cpuid_AVX512CD = /* 1 << 28 */ 268435456;
    UntypedInt cpuid_SHA = /* 1 << 29 */ 536870912;
    const uint32 cpuid_AVX512BW = /* 1 << 30 */ 1073741824;
    const uint32 cpuid_AVX512VL = /* 1 << 31 */ 2147483648;
    const uint32 cpuid_AVX512_VBMI = /* 1 << 1 */ 2;
    UntypedInt cpuid_AVX512_VBMI2 = /* 1 << 6 */ 64;
    UntypedInt cpuid_GFNI = /* 1 << 8 */ 256;
    const uint32 cpuid_AVX512VPCLMULQDQ = /* 1 << 10 */ 1024;
    UntypedInt cpuid_AVX512_BITALG = /* 1 << 12 */ 4096;
    UntypedInt cpuid_FSRM = /* 1 << 4 */ 16;
    const uint32 cpuid_LAHF = /* 1 << 0 */ 1;
    const uint32 cpuid_LZCNT = /* 1 << 5 */ 32;
    UntypedInt cpuid_RDTSCP = /* 1 << 27 */ 134217728;
    const uint32 cpuid_SSE2 = /* 1 << 26 */ 67108864;
    const uint32 cpuid_CX16 = /* 1 << 13 */ 8192;
    const uint32 cpuid_RDRAND = /* 1 << 30 */ 1073741824;
    const uint32 cpuid_RDSEED = /* 1 << 18 */ 262144;
    const uint32 cpuid_AVX512IFMA = /* 1 << 21 */ 2097152;
    const uint32 cpuid_AVX512PF = /* 1 << 26 */ 67108864;
    const uint32 cpuid_AVX512ER = /* 1 << 27 */ 134217728;
    const uint32 cpuid_AVX5124VNNIW = /* 1 << 2 */ 4;
    const uint32 cpuid_AVX5124FMAPS = /* 1 << 3 */ 8;
    const uint32 cpuid_AMXBF16 = /* 1 << 22 */ 4194304;
    const uint32 cpuid_AMXTile = /* 1 << 24 */ 16777216;
    const uint32 cpuid_AMXInt8 = /* 1 << 25 */ 33554432;
    const uint32 cpuid_AVX512BF16 = /* 1 << 5 */ 32;
    const uint32 cpuid_AVXIFMA = /* 1 << 23 */ 8388608;
    const uint32 cpuid_AVXVNNIInt8 = /* 1 << 4 */ 16;

    Initialized = true;

    var (maxID, _, _, _) = cpuid(0, 0);

    if (maxID < 1) {
        return;
    }

    var (_, _, ecx1, edx1) = cpuid(1, 0);
    X86.HasSSE2 = isSet(edx1, cpuid_SSE2);

    X86.HasSSE3 = isSet(ecx1, cpuid_SSE3);
    X86.HasPCLMULQDQ = isSet(ecx1, cpuid_PCLMULQDQ);
    X86.HasSSSE3 = isSet(ecx1, cpuid_SSSE3);
    X86.HasFMA = isSet(ecx1, cpuid_FMA);
    X86.HasCX16 = isSet(ecx1, cpuid_CX16);
    X86.HasSSE41 = isSet(ecx1, cpuid_SSE41);
    X86.HasSSE42 = isSet(ecx1, cpuid_SSE42);
    X86.HasPOPCNT = isSet(ecx1, cpuid_POPCNT);
    X86.HasAES = isSet(ecx1, cpuid_AES);
    X86.HasXSAVE = isSet(ecx1, cpuid_XSAVE);
    X86.HasOSXSAVE = isSet(ecx1, cpuid_OSXSAVE);
    X86.HasF16C = isSet(ecx1, cpuid_F16C);
    X86.HasMOVBE = isSet(ecx1, cpuid_MOVBE);
    X86.HasRDRAND = isSet(ecx1, cpuid_RDRAND);

    var (maxExtID, _, _, _) = cpuid(0x80000000U, 0); if (maxExtID >= 0x80000001U) {
        var (_, _, ecxExt1, _) = cpuid(0x80000001U, 0);
        X86.HasLAHF = isSet(ecxExt1, cpuid_LAHF);
        X86.HasLZCNT = isSet(ecxExt1, cpuid_LZCNT);
    }

    bool osSupportsAVX = default;
    bool osSupportsAVX512 = default;
    if (X86.HasOSXSAVE) {
        var (eax, _) = xgetbv();
        osSupportsAVX = isSet(eax, (uint32)1 << 1) && isSet(eax, (uint32)1 << 2);

        if (runtime.GOOS == "darwin"u8) {
            osSupportsAVX512 = osSupportsAVX && darwinSupportsAVX512();
        } else {
            osSupportsAVX512 = osSupportsAVX && isSet(eax, (uint32)1 << 5) && isSet(eax, (uint32)1 << 6) && isSet(eax, (uint32)1 << 7);
        }
    }

    X86.HasAVX = isSet(ecx1, cpuid_AVX) && osSupportsAVX;

    if (maxID < 7) {
        return;
    }

    var (eax7, ebx7, ecx7, edx7) = cpuid(7, 0);
    X86.HasBMI1 = isSet(ebx7, cpuid_BMI1);
    X86.HasAVX2 = isSet(ebx7, cpuid_AVX2) && osSupportsAVX;
    X86.HasBMI2 = isSet(ebx7, cpuid_BMI2);
    X86.HasERMS = isSet(ebx7, cpuid_ERMS);
    X86.HasRDSEED = isSet(ebx7, cpuid_RDSEED);
    X86.HasADX = isSet(ebx7, cpuid_ADX);

    X86.HasAVX512 = isSet(ebx7, cpuid_AVX512F) && osSupportsAVX512;
    if (X86.HasAVX512) {
        X86.HasAVX512F = true;
        X86.HasAVX512CD = isSet(ebx7, cpuid_AVX512CD);
        X86.HasAVX512ER = isSet(ebx7, cpuid_AVX512ER);
        X86.HasAVX512PF = isSet(ebx7, cpuid_AVX512PF);
        X86.HasAVX512VL = isSet(ebx7, cpuid_AVX512VL);
        X86.HasAVX512BW = isSet(ebx7, cpuid_AVX512BW);
        X86.HasAVX512DQ = isSet(ebx7, cpuid_AVX512DQ);
        X86.HasAVX512IFMA = isSet(ebx7, cpuid_AVX512IFMA);
        X86.HasAVX512VBMI = isSet(ecx7, cpuid_AVX512_VBMI);
        X86.HasAVX5124VNNIW = isSet(edx7, cpuid_AVX5124VNNIW);
        X86.HasAVX5124FMAPS = isSet(edx7, cpuid_AVX5124FMAPS);
        X86.HasAVX512VPOPCNTDQ = isSet(ecx7, cpuid_AVX512VPOPCNTDQ);
        X86.HasAVX512VPCLMULQDQ = isSet(ecx7, cpuid_AVX512VPCLMULQDQ);
        X86.HasAVX512VNNI = isSet(ecx7, cpuid_AVX512VNNI);
        X86.HasAVX512GFNI = isSet(ecx7, cpuid_AVX512GFNI);
        X86.HasAVX512VAES = isSet(ecx7, cpuid_AVX512VAES);
        X86.HasAVX512VBMI2 = isSet(ecx7, cpuid_AVX512VBMI2);
        X86.HasAVX512BITALG = isSet(ecx7, cpuid_AVX512BITALG);
    }

    X86.HasAMXTile = isSet(edx7, cpuid_AMXTile);
    X86.HasAMXInt8 = isSet(edx7, cpuid_AMXInt8);
    X86.HasAMXBF16 = isSet(edx7, cpuid_AMXBF16);

    if (eax7 >= 1) {
        var (eax71, _, _, edx71) = cpuid(7, 1);
        if (X86.HasAVX512) {
            X86.HasAVX512BF16 = isSet(eax71, cpuid_AVX512BF16);
        }
        if (X86.HasAVX) {
            X86.HasAVXIFMA = isSet(eax71, cpuid_AVXIFMA);
            X86.HasAVXVNNI = isSet(eax71, cpuid_AVXVNNI);
            X86.HasAVXVNNIInt8 = isSet(edx71, cpuid_AVXVNNIInt8);
        }
    }
}

internal static bool isSet(uint32 hwc, uint32 value) {
    return (hwc & value) != 0;
}

} // end cpu_package
