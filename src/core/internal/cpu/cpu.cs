// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cpu implements processor feature detection
// used by the Go standard library.
namespace go.@internal;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname

partial class cpu_package {

// DebugOptions is set to true by the runtime if the OS supports reading
// GODEBUG early in runtime startup.
// This should not be changed after it is initialized.
public static bool DebugOptions;

// CacheLinePad is used to pad structs to avoid false sharing.
partial struct CacheLinePad {
    internal array<byte> _ = new(CacheLinePadSize);
}

// CacheLineSize is the CPU's assumed cache line size.
// There is currently no runtime detection of the real cache line size
// so we use the constant per GOARCH CacheLinePadSize as an approximation.
public static uintptr CacheLineSize = CacheLinePadSize;

// The booleans in X86 contain the correspondingly named cpuid feature bit.
// HasAVX and HasAVX2 are only set if the OS does support XMM and YMM registers
// in addition to the cpuid feature bit being set.
// The struct is padded to avoid false sharing.

partial struct X86ᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasAES;
    public bool HasADX;
    public bool HasAVX;
    public bool HasAVX2;
    public bool HasAVX512F;
    public bool HasAVX512BW;
    public bool HasAVX512VL;
    public bool HasBMI1;
    public bool HasBMI2;
    public bool HasERMS;
    public bool HasFSRM;
    public bool HasFMA;
    public bool HasOSXSAVE;
    public bool HasPCLMULQDQ;
    public bool HasPOPCNT;
    public bool HasRDTSCP;
    public bool HasSHA;
    public bool HasSSE3;
    public bool HasSSSE3;
    public bool HasSSE41;
    public bool HasSSE42;
    internal CacheLinePad __;
}
public static ж<X86ᴛ1> ᏑX86 = new StandardBox<X86ᴛ1>(default(X86ᴛ1));
public static ref X86ᴛ1 X86 => ref ᏑX86.Value;

// The booleans in ARM contain the correspondingly named cpu feature bit.
// The struct is padded to avoid false sharing.

partial struct ARMᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasVFPv4;
    public bool HasIDIVA;
    public bool HasV7Atomics;
    internal CacheLinePad __;
}
public static ARMᴛ1 ARM;

// The booleans in ARM64 contain the correspondingly named cpu feature bit.
// The struct is padded to avoid false sharing.

partial struct ARM64ᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasAES;
    public bool HasPMULL;
    public bool HasSHA1;
    public bool HasSHA2;
    public bool HasSHA512;
    public bool HasCRC32;
    public bool HasATOMICS;
    public bool HasCPUID;
    public bool HasDIT;
    public bool IsNeoverse;
    internal CacheLinePad __;
}
public static ARM64ᴛ1 ARM64;

// The booleans in Loong64 contain the correspondingly named cpu feature bit.
// The struct is padded to avoid false sharing.

partial struct Loong64ᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasLSX; // support 128-bit vector extension
    public bool HasCRC32; // support CRC instruction
    public bool HasLAMCAS; // support AMCAS[_DB].{B/H/W/D}
    public bool HasLAM_BH; // support AM{SWAP/ADD}[_DB].{B/H} instruction
    internal CacheLinePad __;
}
public static Loong64ᴛ1 Loong64;


partial struct MIPS64Xᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasMSA; // MIPS SIMD architecture
    internal CacheLinePad __;
}
public static MIPS64Xᴛ1 MIPS64X;

// For ppc64(le), it is safe to check only for ISA level starting on ISA v3.00,
// since there are no optional categories. There are some exceptions that also
// require kernel support to work (darn, scv), so there are feature bits for
// those as well. The minimum processor requirement is POWER8 (ISA 2.07).
// The struct is padded to avoid false sharing.

partial struct PPC64ᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasDARN; // Hardware random number generator (requires kernel enablement)
    public bool HasSCV; // Syscall vectored (requires kernel enablement)
    public bool IsPOWER8; // ISA v2.07 (POWER8)
    public bool IsPOWER9; // ISA v3.00 (POWER9)
    public bool IsPOWER10; // ISA v3.1  (POWER10)
    internal CacheLinePad __;
}
public static PPC64ᴛ1 PPC64;


partial struct S390Xᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasZARCH; // z architecture mode is active [mandatory]
    public bool HasSTFLE; // store facility list extended [mandatory]
    public bool HasLDISP; // long (20-bit) displacements [mandatory]
    public bool HasEIMM; // 32-bit immediates [mandatory]
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
    public bool HasVX; // vector facility. Note: the runtime sets this when it processes auxv records.
    public bool HasVXE; // vector-enhancements facility 1
    public bool HasKDSA; // elliptic curve functions
    public bool HasECDSA; // NIST curves
    public bool HasEDDSA; // Edwards curves
    internal CacheLinePad __;
}
public static S390Xᴛ1 S390X;

// CPU feature variables are accessed by assembly code in various packages.
//go:linkname X86
//go:linkname ARM
//go:linkname ARM64
//go:linkname Loong64
//go:linkname MIPS64X
//go:linkname PPC64
//go:linkname S390X

// Initialize examines the processor and sets the relevant variables above.
// This is called by the runtime package early in program initialization,
// before normal init functions are run. env is set by runtime if the OS supports
// cpu feature options in GODEBUG.
public static void Initialize(@string env) {
    doinit();
    processOptions(env);
}

// options contains the cpu debug options that can be used in GODEBUG.
// Options are arch dependent and are added by the arch specific doinit functions.
// Features that are mandatory for the specific GOARCH should not be added to options
// (e.g. SSE2 on amd64).
internal static slice<option> options;

// Option names should be lower case. e.g. avx instead of AVX.
partial struct option {
    public @string Name;
    public ж<bool> Feature;
    public bool Specified; // whether feature value was specified in GODEBUG
    public bool Enable; // whether feature should be enabled
}

// processOptions enables or disables CPU feature values based on the parsed env string.
// The env string is expected to be of the form cpu.feature1=value1,cpu.feature2=value2...
// where feature names is one of the architecture specific list stored in the
// cpu packages options variable and values are either 'on' or 'off'.
// If env contains cpu.all=off then all cpu features referenced through the options
// variable are disabled. Other feature names and values result in warning messages.
internal static void processOptions(@string env) {
field:
    while (env != ""u8) {
        @string field = ""u8;
        nint i = indexByte(env, (rune)',');
        if (i < 0){
            (field, env) = (env, "");
        } else {
            (field, env) = (env.slice(0, i), env.slice(i + 1));
        }
        if (len(field) < 4 || field[..4] != "cpu.") {
            continue;
        }
        i = indexByte(field, (rune)'=');
        if (i < 0) {
            print((@string)"GODEBUG: no value specified for \""u8, field, (@string)"\"\n"u8);
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
            print((@string)"GODEBUG: value \""u8, value, (@string)"\" not supported for cpu option \""u8, key, (@string)"\"\n"u8);
            goto continue_field;
        }

        if (key == "all"u8) {
            foreach (var (iΔ1, _) in options) {
                options[iΔ1].Specified = true;
                options[iΔ1].Enable = enable;
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
        print((@string)"GODEBUG: unknown cpu feature \""u8, key, (@string)"\"\n"u8);
continue_field:;
    }
break_field:;
    foreach (var (_, o) in options) {
        if (!o.Specified) {
            continue;
        }
        if (o.Enable && !o.Feature.Value) {
            print((@string)"GODEBUG: can not enable \""u8, o.Name, (@string)"\", missing CPU support\n"u8);
            continue;
        }
        o.Feature.Value = o.Enable;
    }
}

// indexByte returns the index of the first instance of c in s,
// or -1 if c is not present in s.
// indexByte is semantically the same as [strings.IndexByte].
// We copy this function because "internal/cpu" should not have external dependencies.
internal static nint indexByte(@string s, byte c) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == c) {
            return i;
        }
    }
    return -1;
}

} // end cpu_package
