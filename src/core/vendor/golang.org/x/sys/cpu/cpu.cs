// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cpu implements processor feature detection for
// various CPU architectures.
namespace go.vendor.golang.org.x.sys;

using os = os_package;
using strings = strings_package;

partial class cpu_package {

// Initialized reports whether the CPU features were initialized.
//
// For some GOOS/GOARCH combinations initialization of the CPU features depends
// on reading an operating specific file, e.g. /proc/self/auxv on linux/arm
// Initialized will report false if reading the file fails.
public static bool Initialized;

// CacheLinePad is used to pad structs to avoid false sharing.
[GoType] partial struct CacheLinePad {
    internal array<byte> _ = new(cacheLineSize);
}

// X86 contains the supported CPU features of the
// current X86/AMD64 platform. If the current platform
// is not X86/AMD64 then all feature flags are false.
//
// X86 is padded to avoid false sharing. Further the HasAVX
// and HasAVX2 are only set if the OS supports XMM and YMM
// registers in addition to the CPUID feature bit being set.

[GoType("dyn")] partial struct X86ᴛ1 {
    internal CacheLinePad _;
    public bool HasAES; // AES hardware implementation (AES NI)
    public bool HasADX; // Multi-precision add-carry instruction extensions
    public bool HasAVX; // Advanced vector extension
    public bool HasAVX2; // Advanced vector extension 2
    public bool HasAVX512; // Advanced vector extension 512
    public bool HasAVX512F; // Advanced vector extension 512 Foundation Instructions
    public bool HasAVX512CD; // Advanced vector extension 512 Conflict Detection Instructions
    public bool HasAVX512ER; // Advanced vector extension 512 Exponential and Reciprocal Instructions
    public bool HasAVX512PF; // Advanced vector extension 512 Prefetch Instructions
    public bool HasAVX512VL; // Advanced vector extension 512 Vector Length Extensions
    public bool HasAVX512BW; // Advanced vector extension 512 Byte and Word Instructions
    public bool HasAVX512DQ; // Advanced vector extension 512 Doubleword and Quadword Instructions
    public bool HasAVX512IFMA; // Advanced vector extension 512 Integer Fused Multiply Add
    public bool HasAVX512VBMI; // Advanced vector extension 512 Vector Byte Manipulation Instructions
    public bool HasAVX5124VNNIW; // Advanced vector extension 512 Vector Neural Network Instructions Word variable precision
    public bool HasAVX5124FMAPS; // Advanced vector extension 512 Fused Multiply Accumulation Packed Single precision
    public bool HasAVX512VPOPCNTDQ; // Advanced vector extension 512 Double and quad word population count instructions
    public bool HasAVX512VPCLMULQDQ; // Advanced vector extension 512 Vector carry-less multiply operations
    public bool HasAVX512VNNI; // Advanced vector extension 512 Vector Neural Network Instructions
    public bool HasAVX512GFNI; // Advanced vector extension 512 Galois field New Instructions
    public bool HasAVX512VAES; // Advanced vector extension 512 Vector AES instructions
    public bool HasAVX512VBMI2; // Advanced vector extension 512 Vector Byte Manipulation Instructions 2
    public bool HasAVX512BITALG; // Advanced vector extension 512 Bit Algorithms
    public bool HasAVX512BF16; // Advanced vector extension 512 BFloat16 Instructions
    public bool HasAMXTile; // Advanced Matrix Extension Tile instructions
    public bool HasAMXInt8; // Advanced Matrix Extension Int8 instructions
    public bool HasAMXBF16; // Advanced Matrix Extension BFloat16 instructions
    public bool HasBMI1; // Bit manipulation instruction set 1
    public bool HasBMI2; // Bit manipulation instruction set 2
    public bool HasCX16; // Compare and exchange 16 Bytes
    public bool HasERMS; // Enhanced REP for MOVSB and STOSB
    public bool HasFMA; // Fused-multiply-add instructions
    public bool HasOSXSAVE; // OS supports XSAVE/XRESTOR for saving/restoring XMM registers.
    public bool HasPCLMULQDQ; // PCLMULQDQ instruction - most often used for AES-GCM
    public bool HasPOPCNT; // Hamming weight instruction POPCNT.
    public bool HasRDRAND; // RDRAND instruction (on-chip random number generator)
    public bool HasRDSEED; // RDSEED instruction (on-chip random number generator)
    public bool HasSSE2; // Streaming SIMD extension 2 (always available on amd64)
    public bool HasSSE3; // Streaming SIMD extension 3
    public bool HasSSSE3; // Supplemental streaming SIMD extension 3
    public bool HasSSE41; // Streaming SIMD extension 4 and 4.1
    public bool HasSSE42; // Streaming SIMD extension 4 and 4.2
    internal CacheLinePad __;
}
public static ж<X86ᴛ1> ᏑX86 = new StandardBox<X86ᴛ1>(default(X86ᴛ1));
public static ref X86ᴛ1 X86 => ref ᏑX86.Value;

// ARM64 contains the supported CPU features of the
// current ARMv8(aarch64) platform. If the current platform
// is not arm64 then all feature flags are false.

[GoType("dyn")] partial struct ARM64ᴛ1 {
    internal CacheLinePad _;
    public bool HasFP; // Floating-point instruction set (always available)
    public bool HasASIMD; // Advanced SIMD (always available)
    public bool HasEVTSTRM; // Event stream support
    public bool HasAES; // AES hardware implementation
    public bool HasPMULL; // Polynomial multiplication instruction set
    public bool HasSHA1; // SHA1 hardware implementation
    public bool HasSHA2; // SHA2 hardware implementation
    public bool HasCRC32; // CRC32 hardware implementation
    public bool HasATOMICS; // Atomic memory operation instruction set
    public bool HasFPHP; // Half precision floating-point instruction set
    public bool HasASIMDHP; // Advanced SIMD half precision instruction set
    public bool HasCPUID; // CPUID identification scheme registers
    public bool HasASIMDRDM; // Rounding double multiply add/subtract instruction set
    public bool HasJSCVT; // Javascript conversion from floating-point to integer
    public bool HasFCMA; // Floating-point multiplication and addition of complex numbers
    public bool HasLRCPC; // Release Consistent processor consistent support
    public bool HasDCPOP; // Persistent memory support
    public bool HasSHA3; // SHA3 hardware implementation
    public bool HasSM3; // SM3 hardware implementation
    public bool HasSM4; // SM4 hardware implementation
    public bool HasASIMDDP; // Advanced SIMD double precision instruction set
    public bool HasSHA512; // SHA512 hardware implementation
    public bool HasSVE; // Scalable Vector Extensions
    public bool HasSVE2; // Scalable Vector Extensions 2
    public bool HasASIMDFHM; // Advanced SIMD multiplication FP16 to FP32
    public bool HasDIT; // Data Independent Timing support
    public bool HasI8MM; // Advanced SIMD Int8 matrix multiplication instructions
    internal CacheLinePad __;
}
public static ARM64ᴛ1 ARM64;

// ARM contains the supported CPU features of the current ARM (32-bit) platform.
// All feature flags are false if:
//  1. the current platform is not arm, or
//  2. the current operating system is not Linux.

[GoType("dyn")] partial struct ARMᴛ1 {
    internal CacheLinePad _;
    public bool HasSWP; // SWP instruction support
    public bool HasHALF; // Half-word load and store support
    public bool HasTHUMB; // ARM Thumb instruction set
    public bool Has26BIT; // Address space limited to 26-bits
    public bool HasFASTMUL; // 32-bit operand, 64-bit result multiplication support
    public bool HasFPA; // Floating point arithmetic support
    public bool HasVFP; // Vector floating point support
    public bool HasEDSP; // DSP Extensions support
    public bool HasJAVA; // Java instruction set
    public bool HasIWMMXT; // Intel Wireless MMX technology support
    public bool HasCRUNCH; // MaverickCrunch context switching and handling
    public bool HasTHUMBEE; // Thumb EE instruction set
    public bool HasNEON; // NEON instruction set
    public bool HasVFPv3; // Vector floating point version 3 support
    public bool HasVFPv3D16; // Vector floating point version 3 D8-D15
    public bool HasTLS; // Thread local storage support
    public bool HasVFPv4; // Vector floating point version 4 support
    public bool HasIDIVA; // Integer divide instruction support in ARM mode
    public bool HasIDIVT; // Integer divide instruction support in Thumb mode
    public bool HasVFPD32; // Vector floating point version 3 D15-D31
    public bool HasLPAE; // Large Physical Address Extensions
    public bool HasEVTSTRM; // Event stream support
    public bool HasAES; // AES hardware implementation
    public bool HasPMULL; // Polynomial multiplication instruction set
    public bool HasSHA1; // SHA1 hardware implementation
    public bool HasSHA2; // SHA2 hardware implementation
    public bool HasCRC32; // CRC32 hardware implementation
    internal CacheLinePad __;
}
public static ARMᴛ1 ARM;

// MIPS64X contains the supported CPU features of the current mips64/mips64le
// platforms. If the current platform is not mips64/mips64le or the current
// operating system is not Linux then all feature flags are false.

[GoType("dyn")] partial struct MIPS64Xᴛ1 {
    internal CacheLinePad _;
    public bool HasMSA; // MIPS SIMD architecture
    internal CacheLinePad __;
}
public static MIPS64Xᴛ1 MIPS64X;

// PPC64 contains the supported CPU features of the current ppc64/ppc64le platforms.
// If the current platform is not ppc64/ppc64le then all feature flags are false.
//
// For ppc64/ppc64le, it is safe to check only for ISA level starting on ISA v3.00,
// since there are no optional categories. There are some exceptions that also
// require kernel support to work (DARN, SCV), so there are feature bits for
// those as well. The struct is padded to avoid false sharing.

[GoType("dyn")] partial struct PPC64ᴛ1 {
    internal CacheLinePad _;
    public bool HasDARN; // Hardware random number generator (requires kernel enablement)
    public bool HasSCV; // Syscall vectored (requires kernel enablement)
    public bool IsPOWER8; // ISA v2.07 (POWER8)
    public bool IsPOWER9; // ISA v3.00 (POWER9), implies IsPOWER8
    internal CacheLinePad __;
}
public static PPC64ᴛ1 PPC64;

// S390X contains the supported CPU features of the current IBM Z
// (s390x) platform. If the current platform is not IBM Z then all
// feature flags are false.
//
// S390X is padded to avoid false sharing. Further HasVX is only set
// if the OS supports vector registers in addition to the STFLE
// feature bit being set.

[GoType("dyn")] partial struct S390Xᴛ1 {
    internal CacheLinePad _;
    public bool HasZARCH; // z/Architecture mode is active [mandatory]
    public bool HasSTFLE; // store facility list extended
    public bool HasLDISP; // long (20-bit) displacements
    public bool HasEIMM; // 32-bit immediates
    public bool HasDFP; // decimal floating point
    public bool HasETF3EH; // ETF-3 enhanced
    public bool HasMSA; // message security assist (CPACF)
    public bool HasAES; // KM-AES{128,192,256} functions
    public bool HasAESCBC; // KMC-AES{128,192,256} functions
    public bool HasAESCTR; // KMCTR-AES{128,192,256} functions
    public bool HasAESGCM; // KMA-GCM-AES{128,192,256} functions
    public bool HasGHASH; // KIMD-GHASH function
    public bool HasSHA1; // K{I,L}MD-SHA-1 functions
    public bool HasSHA256; // K{I,L}MD-SHA-256 functions
    public bool HasSHA512; // K{I,L}MD-SHA-512 functions
    public bool HasSHA3; // K{I,L}MD-SHA3-{224,256,384,512} and K{I,L}MD-SHAKE-{128,256} functions
    public bool HasVX; // vector facility
    public bool HasVXE; // vector-enhancements facility 1
    internal CacheLinePad __;
}
public static S390Xᴛ1 S390X;

// RISCV64 contains the supported CPU features and performance characteristics for riscv64
// platforms. The booleans in RISCV64, with the exception of HasFastMisaligned, indicate
// the presence of RISC-V extensions.
//
// It is safe to assume that all the RV64G extensions are supported and so they are omitted from
// this structure. As riscv64 Go programs require at least RV64G, the code that populates
// this structure cannot run successfully if some of the RV64G extensions are missing.
// The struct is padded to avoid false sharing.

[GoType("dyn")] partial struct RISCV64ᴛ1 {
    internal CacheLinePad _;
    public bool HasFastMisaligned; // Fast misaligned accesses
    public bool HasC; // Compressed instruction-set extension
    public bool HasV; // Vector extension compatible with RVV 1.0
    public bool HasZba; // Address generation instructions extension
    public bool HasZbb; // Basic bit-manipulation extension
    public bool HasZbs; // Single-bit instructions extension
    internal CacheLinePad __;
}
public static RISCV64ᴛ1 RISCV64;

[GoInit] internal static void init() {
    archInit();
    initOptions();
    processOptions();
}

// options contains the cpu debug options that can be used in GODEBUG.
// Options are arch dependent and are added by the arch specific initOptions functions.
// Features that are mandatory for the specific GOARCH should have the Required field set
// (e.g. SSE2 on amd64).
internal static slice<option> options;

// Option names should be lower case. e.g. avx instead of AVX.
[GoType] partial struct option {
    public @string Name;
    public ж<bool> Feature;
    public bool Specified; // whether feature value was specified in GODEBUG
    public bool Enable; // whether feature should be enabled
    public bool Required; // whether feature is mandatory and can not be disabled
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string godebugˢ = "GODEBUG"u8;

internal static void processOptions() {
    @string env = os.Getenv(godebugˢ);
field:
    while (env != ""u8) {
        @string field = ""u8;
        nint i = strings.IndexByte(env, (rune)',');
        if (i < 0){
            (field, env) = (env, "");
        } else {
            (field, env) = (env.slice(0, i), env.slice(i + 1));
        }
        if (len(field) < 4 || field[..4] != "cpu.") {
            continue;
        }
        i = strings.IndexByte(field, (rune)'=');
        if (i < 0) {
            print((@string)"GODEBUG sys/cpu: no value specified for \""u8, field, (@string)"\"\n"u8);
            continue;
        }
        @string key = field.slice(4, i);
        @string value = field.slice(i + 1); // e.g. "SSE2", "on"
        bool enable = default!;
        var exprᴛ1 = value;
        if (exprᴛ1 == "on"u8) {
            enable = true;
        }
        else if (exprᴛ1 == "off"u8) {
            enable = false;
        }
        else { /* default: */
            print((@string)"GODEBUG sys/cpu: value \""u8, value, (@string)"\" not supported for cpu option \""u8, key, (@string)"\"\n"u8);
            goto continue_field;
        }

        if (key == "all"u8) {
            foreach (var (iΔ1, _) in options) {
                options[iΔ1].Specified = true;
                options[iΔ1].Enable = enable || options[iΔ1].Required;
            }
            goto continue_field;
        }
        foreach (var (iΔ2, _) in options) {
            if (options[iΔ2].Name == key) {
                options[iΔ2].Specified = true;
                options[iΔ2].Enable = enable;
                goto continue_field;
            }
        }
        print((@string)"GODEBUG sys/cpu: unknown cpu feature \""u8, key, (@string)"\"\n"u8);
continue_field:;
    }
break_field:;
    foreach (var (_, o) in options) {
        if (!o.Specified) {
            continue;
        }
        if (o.Enable && !o.Feature.Value) {
            print((@string)"GODEBUG sys/cpu: can not enable \""u8, o.Name, (@string)"\", missing CPU support\n"u8);
            continue;
        }
        if (!o.Enable && o.Required) {
            print((@string)"GODEBUG sys/cpu: can not disable \""u8, o.Name, (@string)"\", required CPU feature\n"u8);
            continue;
        }
        o.Feature.Value = o.Enable;
    }
}

} // end cpu_package
