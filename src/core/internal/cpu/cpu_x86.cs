// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build 386 || amd64
namespace go.@internal;

using ꓸꓸꓸuint32 = Span<uint32>;

partial class cpu_package {

public static UntypedInt CacheLinePadSize => 64;

// cpuid is implemented in cpu_x86.s.
internal static partial (uint32 eax, uint32 ebx, uint32 ecx, uint32 edx) cpuid(uint32 eaxArg, uint32 ecxArg);

// xgetbv with ecx = 0 is implemented in cpu_x86.s.
internal static partial (uint32 eax, uint32 edx) xgetbv();

// go2cs generated this placeholder — func getGOAMD64level is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// eax bits
internal static UntypedInt cpuid_AVXVNNI => /* 1 << 4 */ 16;

// ecx bits
internal static UntypedInt cpuid_SSE3 => /* 1 << 0 */ 1;
internal static UntypedInt cpuid_PCLMULQDQ => /* 1 << 1 */ 2;
internal static UntypedInt cpuid_AVX512VBMI => /* 1 << 1 */ 2;
internal static UntypedInt cpuid_AVX512VBMI2 => /* 1 << 6 */ 64;
internal static UntypedInt cpuid_SSSE3 => /* 1 << 9 */ 512;
internal static UntypedInt cpuid_AVX512GFNI => /* 1 << 8 */ 256;
internal static UntypedInt cpuid_VAES => /* 1 << 9 */ 512;
internal static UntypedInt cpuid_AVX512VNNI => /* 1 << 11 */ 2048;
internal static UntypedInt cpuid_AVX512BITALG => /* 1 << 12 */ 4096;
internal static UntypedInt cpuid_FMA => /* 1 << 12 */ 4096;
internal static UntypedInt cpuid_AVX512VPOPCNTDQ => /* 1 << 14 */ 16384;
internal static UntypedInt cpuid_SSE41 => /* 1 << 19 */ 524288;
internal static UntypedInt cpuid_SSE42 => /* 1 << 20 */ 1048576;
internal static UntypedInt cpuid_POPCNT => /* 1 << 23 */ 8388608;
internal static UntypedInt cpuid_AES => /* 1 << 25 */ 33554432;
internal static UntypedInt cpuid_OSXSAVE => /* 1 << 27 */ 134217728;
internal static UntypedInt cpuid_AVX => /* 1 << 28 */ 268435456;

// "Extended Feature Flag" bits returned in EBX for CPUID EAX=0x7 ECX=0x0
internal static UntypedInt cpuid_BMI1 => /* 1 << 3 */ 8;
internal static UntypedInt cpuid_AVX2 => /* 1 << 5 */ 32;
internal static UntypedInt cpuid_BMI2 => /* 1 << 8 */ 256;
internal static UntypedInt cpuid_ERMS => /* 1 << 9 */ 512;
internal static UntypedInt cpuid_AVX512F => /* 1 << 16 */ 65536;
internal static UntypedInt cpuid_AVX512DQ => /* 1 << 17 */ 131072;
internal static UntypedInt cpuid_ADX => /* 1 << 19 */ 524288;
internal static UntypedInt cpuid_AVX512CD => /* 1 << 28 */ 268435456;
internal static UntypedInt cpuid_SHA => /* 1 << 29 */ 536870912;
internal static UntypedInt cpuid_AVX512BW => /* 1 << 30 */ 1073741824;
internal static UntypedInt cpuid_AVX512VL => /* 1 << 31 */ 2147483648;

// "Extended Feature Flag" bits returned in ECX for CPUID EAX=0x7 ECX=0x0
internal static UntypedInt cpuid_AVX512_VBMI => /* 1 << 1 */ 2;
internal static UntypedInt cpuid_AVX512_VBMI2 => /* 1 << 6 */ 64;
internal static UntypedInt cpuid_GFNI => /* 1 << 8 */ 256;
internal static UntypedInt cpuid_AVX512VPCLMULQDQ => /* 1 << 10 */ 1024;
internal static UntypedInt cpuid_AVX512_BITALG => /* 1 << 12 */ 4096;

// edx bits
internal static UntypedInt cpuid_FSRM => /* 1 << 4 */ 16;
// edx bits for CPUID 0x80000001
internal static UntypedInt cpuid_RDTSCP => /* 1 << 27 */ 134217728;

internal static uint32 maxExtendedFunctionInformation;

internal static void doinit() {
    options = new option[]{
        new(Name: "adx"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasADX)),
        new(Name: "aes"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAES)),
        new(Name: "erms"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasERMS)),
        new(Name: "fsrm"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasFSRM)),
        new(Name: "pclmulqdq"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasPCLMULQDQ)),
        new(Name: "rdtscp"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasRDTSCP)),
        new(Name: "sha"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSHA)),
        new(Name: "vpclmulqdq"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VPCLMULQDQ))
    }.slice();
    var level = getGOAMD64level();
    if (level < 2) {
        // These options are required at level 2. At lower levels
        // they can be turned off.
        options = append(options,
            new option(Name: "popcnt"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasPOPCNT)),
            new option(Name: "sse3"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE3)),
            new option(Name: "sse41"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE41)),
            new option(Name: "sse42"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE42)),
            new option(Name: "ssse3"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSSE3)));
    }
    if (level < 3) {
        // These options are required at level 3. At lower levels
        // they can be turned off.
        options = append(options,
            new option(Name: "avx"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX)),
            new option(Name: "avx2"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX2)),
            new option(Name: "bmi1"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasBMI1)),
            new option(Name: "bmi2"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasBMI2)),
            new option(Name: "fma"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasFMA)));
    }
    if (level < 4) {
        // These options are required at level 4. At lower levels
        // they can be turned off.
        options = append(options,
            new option(Name: "avx512f"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512F)),
            new option(Name: "avx512cd"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512CD)),
            new option(Name: "avx512bw"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512BW)),
            new option(Name: "avx512dq"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512DQ)),
            new option(Name: "avx512vl"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VL)));
    }

    var (maxID, _, _, _) = cpuid(0, 0);

    if (maxID < 1) {
        osInit();
        return;
    }

    (maxExtendedFunctionInformation, _, _, _) = cpuid(0x80000000U, 0);

    var (_, _, ecx1, _) = cpuid(1, 0);

    X86.HasSSE3 = isSet(ecx1, cpuid_SSE3);
    X86.HasPCLMULQDQ = isSet(ecx1, cpuid_PCLMULQDQ);
    X86.HasSSSE3 = isSet(ecx1, cpuid_SSSE3);
    X86.HasSSE41 = isSet(ecx1, cpuid_SSE41);
    X86.HasSSE42 = isSet(ecx1, cpuid_SSE42);
    X86.HasPOPCNT = isSet(ecx1, cpuid_POPCNT);
    X86.HasAES = isSet(ecx1, cpuid_AES);

    // OSXSAVE can be false when using older Operating Systems
    // or when explicitly disabled on newer Operating Systems by
    // e.g. setting the xsavedisable boot option on Windows 10.
    X86.HasOSXSAVE = isSet(ecx1, cpuid_OSXSAVE);

    var osSupportsAVX = false;
    var osSupportsAVX512 = false;
    // For XGETBV, OSXSAVE bit is required and sufficient.
    if (X86.HasOSXSAVE) {
        var (eax, _) = xgetbv();
        // Check if XMM and YMM registers have OS support.
        osSupportsAVX = isSet(eax, ((uint32)1 << (int)(1))) && isSet(eax, ((uint32)1 << (int)(2)));

        // AVX512 detection does not work on Darwin,
        // see https://github.com/golang/go/issues/49233
        //
        // Check if opmask, ZMMhi256 and Hi16_ZMM have OS support.
        osSupportsAVX512 = osSupportsAVX && isSet(eax, ((uint32)1 << (int)(5))) && isSet(eax, ((uint32)1 << (int)(6))) && isSet(eax, ((uint32)1 << (int)(7)));
    }

    X86.HasAVX = isSet(ecx1, cpuid_AVX) && osSupportsAVX;

    // The FMA instruction set extension requires both the FMA and AVX flags.
    //
    // Furthermore, the FMA instructions are all VEX prefixed instructions.
    // VEX prefixed instructions require OSXSAVE to be enabled.
    // See Intel 64 and IA-32 Architecture Software Developer’s Manual Volume 2
    // Section 2.4 "AVX and SSE Instruction Exception Specification"
    X86.HasFMA = isSet(ecx1, cpuid_FMA) && X86.HasAVX && X86.HasOSXSAVE;

    if (maxID < 7) {
        osInit();
        return;
    }

    var (eax7, ebx7, ecx7, edx7) = cpuid(7, 0);
    X86.HasBMI1 = isSet(ebx7, cpuid_BMI1);
    X86.HasAVX2 = isSet(ebx7, cpuid_AVX2) && osSupportsAVX;
    X86.HasBMI2 = isSet(ebx7, cpuid_BMI2);
    X86.HasERMS = isSet(ebx7, cpuid_ERMS);
    X86.HasADX = isSet(ebx7, cpuid_ADX);
    X86.HasSHA = isSet(ebx7, cpuid_SHA);
    X86.HasVAES = isSet(ecx7, cpuid_VAES) && X86.HasAVX;

    X86.HasAVX512F = isSet(ebx7, cpuid_AVX512F) && osSupportsAVX512;
    if (X86.HasAVX512F) {
        X86.HasAVX512CD = isSet(ebx7, cpuid_AVX512CD);
        X86.HasAVX512BW = isSet(ebx7, cpuid_AVX512BW);
        X86.HasAVX512DQ = isSet(ebx7, cpuid_AVX512DQ);
        X86.HasAVX512VL = isSet(ebx7, cpuid_AVX512VL);
        X86.HasAVX512GFNI = isSet(ecx7, cpuid_AVX512GFNI);
        X86.HasAVX512BITALG = isSet(ecx7, cpuid_AVX512BITALG);
        X86.HasAVX512VPOPCNTDQ = isSet(ecx7, cpuid_AVX512VPOPCNTDQ);
        X86.HasAVX512VBMI = isSet(ecx7, cpuid_AVX512VBMI);
        X86.HasAVX512VBMI2 = isSet(ecx7, cpuid_AVX512VBMI2);
        X86.HasAVX512VAES = isSet(ecx7, cpuid_VAES) && X86.HasAES && isSet(ebx7, cpuid_AVX512VL);
        X86.HasAVX512VNNI = isSet(ecx7, cpuid_AVX512VNNI);
        X86.HasAVX512VPCLMULQDQ = isSet(ecx7, cpuid_AVX512VPCLMULQDQ);
        X86.HasAVX512VBMI = isSet(ecx7, cpuid_AVX512_VBMI);
        X86.HasAVX512VBMI2 = isSet(ecx7, cpuid_AVX512_VBMI2);
        X86.HasGFNI = isSet(ecx7, cpuid_GFNI);
        X86.HasAVX512BITALG = isSet(ecx7, cpuid_AVX512_BITALG);
    }

    X86.HasFSRM = isSet(edx7, cpuid_FSRM);

    uint32 maxExtendedInformation = default!;
    (maxExtendedInformation, _, _, _) = cpuid(0x80000000U, 0);

    if (maxExtendedInformation < 0x80000001U) {
        osInit();
        return;
    }

    var (_, _, _, edxExt1) = cpuid(0x80000001U, 0);
    X86.HasRDTSCP = isSet(edxExt1, cpuid_RDTSCP);

    doDerived = () => {
        // Rather than carefully gating on fundamental AVX-512 features, we have
        // a virtual "AVX512" feature that captures F+CD+BW+DQ+VL. BW, DQ, and
        // VL have a huge effect on which AVX-512 instructions are available,
        // and these have all been supported on everything except the earliest
        // Phi chips with AVX-512. No CPU has had CD without F, so we include
        // it. GOAMD64=v4 also implies exactly this set, and these are all
        // included in AVX10.1.
        X86.HasAVX512 = X86.HasAVX512F && X86.HasAVX512CD && X86.HasAVX512BW && X86.HasAVX512DQ && X86.HasAVX512VL;
    };

    if (eax7 >= 1) {
        var (eax71, _, _, _) = cpuid(7, 1);
        if (X86.HasAVX) {
            X86.HasAVXVNNI = isSet(eax71, cpuid_AVXVNNI);
        }
    }

    osInit();
}

internal static bool isSet(uint32 hwc, uint32 value) {
    return (uint32)(hwc & value) != 0;
}

// Name returns the CPU name given by the vendor.
// If the CPU name can not be determined an
// empty string is returned.
public static @string Name() {
    if (maxExtendedFunctionInformation < 0x80000004U) {
        return ""u8;
    }

    var data = new slice<byte>(0, 3 * 4 * 4);

    uint32 eax = default!;
    uint32 ebx = default!;
    uint32 ecx = default!;
    uint32 edx = default!;
    (eax, ebx, ecx, edx) = cpuid(0x80000002U, 0);
    data = appendBytes(data, eax, ebx, ecx, edx);
    (eax, ebx, ecx, edx) = cpuid(0x80000003U, 0);
    data = appendBytes(data, eax, ebx, ecx, edx);
    (eax, ebx, ecx, edx) = cpuid(0x80000004U, 0);
    data = appendBytes(data, eax, ebx, ecx, edx);

    // Trim leading spaces.
    while (len(data) > 0 && data[0] == (rune)' ') {
        data = data[1..];
    }

    // Trim tail after and including the first null byte.
    foreach (var (i, c) in data) {
        if (c == (rune)'\x00') {
            data = data.slice(0, i);
            break;
        }
    }

    return ((@string)data);
}

internal static slice<byte> appendBytes(slice<byte> b, params ꓸꓸꓸuint32 argsʗp) {
    var args = argsʗp.sslice();

    foreach (var (_, arg) in args) {
        b = append(b,
            (byte)((arg >> (int)(0))),
            (byte)((arg >> (int)(8))),
            (byte)((arg >> (int)(16))),
            (byte)((arg >> (int)(24))));
    }
    return b;
}

} // end cpu_package
