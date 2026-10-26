// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build 386 || amd64 || amd64p32
namespace go.vendor.golang.org.x.sys;

using runtime = runtime_package;

partial class cpu_package {

internal static UntypedInt cacheLineSize => 64;

internal static void initOptions() {
    options = new option[]{
        new(Name: "adx"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasADX)),
        new(Name: "aes"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAES)),
        new(Name: "avx"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX)),
        new(Name: "avx2"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX2)),
        new(Name: "avx512"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512)),
        new(Name: "avx512f"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512F)),
        new(Name: "avx512cd"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512CD)),
        new(Name: "avx512er"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512ER)),
        new(Name: "avx512pf"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512PF)),
        new(Name: "avx512vl"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VL)),
        new(Name: "avx512bw"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512BW)),
        new(Name: "avx512dq"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512DQ)),
        new(Name: "avx512ifma"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512IFMA)),
        new(Name: "avx512vbmi"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VBMI)),
        new(Name: "avx512vnniw"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX5124VNNIW)),
        new(Name: "avx5124fmaps"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX5124FMAPS)),
        new(Name: "avx512vpopcntdq"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VPOPCNTDQ)),
        new(Name: "avx512vpclmulqdq"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VPCLMULQDQ)),
        new(Name: "avx512vnni"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VNNI)),
        new(Name: "avx512gfni"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512GFNI)),
        new(Name: "avx512vaes"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VAES)),
        new(Name: "avx512vbmi2"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VBMI2)),
        new(Name: "avx512bitalg"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512BITALG)),
        new(Name: "avx512bf16"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512BF16)),
        new(Name: "amxtile"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAMXTile)),
        new(Name: "amxint8"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAMXInt8)),
        new(Name: "amxbf16"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAMXBF16)),
        new(Name: "bmi1"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasBMI1)),
        new(Name: "bmi2"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasBMI2)),
        new(Name: "cx16"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasCX16)),
        new(Name: "erms"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasERMS)),
        new(Name: "fma"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasFMA)),
        new(Name: "osxsave"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasOSXSAVE)),
        new(Name: "pclmulqdq"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasPCLMULQDQ)),
        new(Name: "popcnt"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasPOPCNT)),
        new(Name: "rdrand"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasRDRAND)),
        new(Name: "rdseed"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasRDSEED)),
        new(Name: "sse3"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE3)),
        new(Name: "sse41"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE41)),
        new(Name: "sse42"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE42)),
        new(Name: "ssse3"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSSE3)),
        new(Name: "avxifma"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVXIFMA)),
        new(Name: "avxvnni"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVXVNNI)),
        new(Name: "avxvnniint8"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVXVNNIInt8)),

        // These capabilities should always be enabled on amd64:
        new(Name: "sse2"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE2), Required: runtime.GOARCH == "amd64"u8)
    }.slice();
}

internal static void archInit() {
    Initialized = true;

    var (maxID, _, _, _) = cpuid(0, 0);

    if (maxID < 1) {
        return;
    }

    var (_, _, ecx1, edx1) = cpuid(1, 0);
    X86.HasSSE2 = isSet(26, edx1);

    X86.HasSSE3 = isSet(0, ecx1);
    X86.HasPCLMULQDQ = isSet(1, ecx1);
    X86.HasSSSE3 = isSet(9, ecx1);
    X86.HasFMA = isSet(12, ecx1);
    X86.HasCX16 = isSet(13, ecx1);
    X86.HasSSE41 = isSet(19, ecx1);
    X86.HasSSE42 = isSet(20, ecx1);
    X86.HasPOPCNT = isSet(23, ecx1);
    X86.HasAES = isSet(25, ecx1);
    X86.HasOSXSAVE = isSet(27, ecx1);
    X86.HasRDRAND = isSet(30, ecx1);

    bool osSupportsAVX = default!;
    bool osSupportsAVX512 = default!;
    // For XGETBV, OSXSAVE bit is required and sufficient.
    if (X86.HasOSXSAVE) {
        var (eax, _) = xgetbv();
        // Check if XMM and YMM registers have OS support.
        osSupportsAVX = isSet(1, eax) && isSet(2, eax);

        if (runtime.GOOS == "darwin"u8){
            // Darwin requires special AVX512 checks, see cpu_darwin_x86.go
            osSupportsAVX512 = osSupportsAVX && darwinSupportsAVX512();
        } else {
            // Check if OPMASK and ZMM registers have OS support.
            osSupportsAVX512 = osSupportsAVX && isSet(5, eax) && isSet(6, eax) && isSet(7, eax);
        }
    }

    X86.HasAVX = isSet(28, ecx1) && osSupportsAVX;

    if (maxID < 7) {
        return;
    }

    var (eax7, ebx7, ecx7, edx7) = cpuid(7, 0);
    X86.HasBMI1 = isSet(3, ebx7);
    X86.HasAVX2 = isSet(5, ebx7) && osSupportsAVX;
    X86.HasBMI2 = isSet(8, ebx7);
    X86.HasERMS = isSet(9, ebx7);
    X86.HasRDSEED = isSet(18, ebx7);
    X86.HasADX = isSet(19, ebx7);

    X86.HasAVX512 = isSet(16, ebx7) && osSupportsAVX512; // Because avx-512 foundation is the core required extension
    if (X86.HasAVX512) {
        X86.HasAVX512F = true;
        X86.HasAVX512CD = isSet(28, ebx7);
        X86.HasAVX512ER = isSet(27, ebx7);
        X86.HasAVX512PF = isSet(26, ebx7);
        X86.HasAVX512VL = isSet(31, ebx7);
        X86.HasAVX512BW = isSet(30, ebx7);
        X86.HasAVX512DQ = isSet(17, ebx7);
        X86.HasAVX512IFMA = isSet(21, ebx7);
        X86.HasAVX512VBMI = isSet(1, ecx7);
        X86.HasAVX5124VNNIW = isSet(2, edx7);
        X86.HasAVX5124FMAPS = isSet(3, edx7);
        X86.HasAVX512VPOPCNTDQ = isSet(14, ecx7);
        X86.HasAVX512VPCLMULQDQ = isSet(10, ecx7);
        X86.HasAVX512VNNI = isSet(11, ecx7);
        X86.HasAVX512GFNI = isSet(8, ecx7);
        X86.HasAVX512VAES = isSet(9, ecx7);
        X86.HasAVX512VBMI2 = isSet(6, ecx7);
        X86.HasAVX512BITALG = isSet(12, ecx7);
    }

    X86.HasAMXTile = isSet(24, edx7);
    X86.HasAMXInt8 = isSet(25, edx7);
    X86.HasAMXBF16 = isSet(22, edx7);

    // These features depend on the second level of extended features.
    if (eax7 >= 1) {
        var (eax71, _, _, edx71) = cpuid(7, 1);
        if (X86.HasAVX512) {
            X86.HasAVX512BF16 = isSet(5, eax71);
        }
        if (X86.HasAVX) {
            X86.HasAVXIFMA = isSet(23, eax71);
            X86.HasAVXVNNI = isSet(4, eax71);
            X86.HasAVXVNNIInt8 = isSet(4, edx71);
        }
    }
}

internal static bool isSet(nuint bitpos, uint32 value) {
    return (value & (((uint32)1).Lsh(bitpos))) != 0;
}

} // end cpu_package
