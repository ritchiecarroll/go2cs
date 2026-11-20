// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using os = os_package;
using strings = strings_package;

partial class cpu_package {

public static readonly ж<bool> ᏑInitialized = new StaticBox<bool>(default(bool));
public static ref bool Initialized => ref ((StaticBox<bool>)ᏑInitialized).ValueSlot;

partial struct CacheLinePad {
    internal array<byte> _ = new(cacheLineSize);
}


partial struct X86ᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasAES;
    public bool HasADX;
    public bool HasAVX;
    public bool HasAVX2;
    public bool HasAVX512;
    public bool HasAVX512F;
    public bool HasAVX512CD;
    public bool HasAVX512ER;
    public bool HasAVX512PF;
    public bool HasAVX512VL;
    public bool HasAVX512BW;
    public bool HasAVX512DQ;
    public bool HasAVX512IFMA;
    public bool HasAVX512VBMI;
    public bool HasAVX5124VNNIW;
    public bool HasAVX5124FMAPS;
    public bool HasAVX512VPOPCNTDQ;
    public bool HasAVX512VPCLMULQDQ;
    public bool HasAVX512VNNI;
    public bool HasAVX512GFNI;
    public bool HasAVX512VAES;
    public bool HasAVX512VBMI2;
    public bool HasAVX512BITALG;
    public bool HasAVX512BF16;
    public bool HasAMXTile;
    public bool HasAMXInt8;
    public bool HasAMXBF16;
    public bool HasBMI1;
    public bool HasBMI2;
    public bool HasCX16;
    public bool HasERMS;
    public bool HasF16C;
    public bool HasFMA;
    public bool HasLAHF;
    public bool HasLZCNT;
    public bool HasMOVBE;
    public bool HasOSXSAVE;
    public bool HasPCLMULQDQ;
    public bool HasPOPCNT;
    public bool HasRDRAND;
    public bool HasRDSEED;
    public bool HasSSE2;
    public bool HasSSE3;
    public bool HasSSSE3;
    public bool HasSSE41;
    public bool HasSSE42;
    public bool HasAVXIFMA;
    public bool HasAVXVNNI;
    public bool HasAVXVNNIInt8;
    public bool HasXSAVE;
    internal CacheLinePad __;
}
public static readonly ж<X86ᴛ1> ᏑX86 = new StaticBox<X86ᴛ1>(default(X86ᴛ1));
public static ref X86ᴛ1 X86 => ref ((StaticBox<X86ᴛ1>)ᏑX86).ValueSlot;


partial struct ARM64ᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasFP;
    public bool HasASIMD;
    public bool HasEVTSTRM;
    public bool HasAES;
    public bool HasPMULL;
    public bool HasSHA1;
    public bool HasSHA2;
    public bool HasCRC32;
    public bool HasATOMICS;
    public bool HasFPHP;
    public bool HasASIMDHP;
    public bool HasCPUID;
    public bool HasASIMDRDM;
    public bool HasJSCVT;
    public bool HasFCMA;
    public bool HasLRCPC;
    public bool HasDCPOP;
    public bool HasSHA3;
    public bool HasSM3;
    public bool HasSM4;
    public bool HasASIMDDP;
    public bool HasSHA512;
    public bool HasSVE;
    public bool HasSVE2;
    public bool HasASIMDFHM;
    public bool HasDIT;
    public bool HasI8MM;
    internal CacheLinePad __;
}
public static readonly ж<ARM64ᴛ1> ᏑARM64 = new StaticBox<ARM64ᴛ1>(default(ARM64ᴛ1));
public static ref ARM64ᴛ1 ARM64 => ref ((StaticBox<ARM64ᴛ1>)ᏑARM64).ValueSlot;


partial struct ARMᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasSWP;
    public bool HasHALF;
    public bool HasTHUMB;
    public bool Has26BIT;
    public bool HasFASTMUL;
    public bool HasFPA;
    public bool HasVFP;
    public bool HasEDSP;
    public bool HasJAVA;
    public bool HasIWMMXT;
    public bool HasCRUNCH;
    public bool HasTHUMBEE;
    public bool HasNEON;
    public bool HasVFPv3;
    public bool HasVFPv3D16;
    public bool HasTLS;
    public bool HasVFPv4;
    public bool HasIDIVA;
    public bool HasIDIVT;
    public bool HasVFPD32;
    public bool HasLPAE;
    public bool HasEVTSTRM;
    public bool HasAES;
    public bool HasPMULL;
    public bool HasSHA1;
    public bool HasSHA2;
    public bool HasCRC32;
    internal CacheLinePad __;
}
public static readonly ж<ARMᴛ1> ᏑARM = new StaticBox<ARMᴛ1>(default(ARMᴛ1));
public static ref ARMᴛ1 ARM => ref ((StaticBox<ARMᴛ1>)ᏑARM).ValueSlot;


partial struct Loong64ᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasLSX;
    public bool HasLASX;
    public bool HasCRC32;
    public bool HasLAMCAS;
    public bool HasLAM_BH;
    public bool HasLLACQ_SCREL;
    public bool HasSCQ;
    public bool HasDBAR_HINTS;
    internal CacheLinePad __;
}
public static readonly ж<Loong64ᴛ1> ᏑLoong64 = new StaticBox<Loong64ᴛ1>(default(Loong64ᴛ1));
public static ref Loong64ᴛ1 Loong64 => ref ((StaticBox<Loong64ᴛ1>)ᏑLoong64).ValueSlot;


partial struct MIPS64Xᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasMSA;
    internal CacheLinePad __;
}
public static readonly ж<MIPS64Xᴛ1> ᏑMIPS64X = new StaticBox<MIPS64Xᴛ1>(default(MIPS64Xᴛ1));
public static ref MIPS64Xᴛ1 MIPS64X => ref ((StaticBox<MIPS64Xᴛ1>)ᏑMIPS64X).ValueSlot;


partial struct PPC64ᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasDARN;
    public bool HasSCV;
    public bool IsPOWER8;
    public bool IsPOWER9;
    public bool IsPOWER10;
    internal CacheLinePad __;
}
public static readonly ж<PPC64ᴛ1> ᏑPPC64 = new StaticBox<PPC64ᴛ1>(default(PPC64ᴛ1));
public static ref PPC64ᴛ1 PPC64 => ref ((StaticBox<PPC64ᴛ1>)ᏑPPC64).ValueSlot;


partial struct S390Xᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasZARCH;
    public bool HasSTFLE;
    public bool HasLDISP;
    public bool HasEIMM;
    public bool HasDFP;
    public bool HasETF3EH;
    public bool HasMSA;
    public bool HasAES;
    public bool HasAESCBC;
    public bool HasAESCTR;
    public bool HasAESGCM;
    public bool HasGHASH;
    public bool HasSHA1;
    public bool HasSHA256;
    public bool HasSHA512;
    public bool HasSHA3;
    public bool HasVX;
    public bool HasVXE;
    internal CacheLinePad __;
}
public static readonly ж<S390Xᴛ1> ᏑS390X = new StaticBox<S390Xᴛ1>(default(S390Xᴛ1));
public static ref S390Xᴛ1 S390X => ref ((StaticBox<S390Xᴛ1>)ᏑS390X).ValueSlot;


partial struct RISCV64ᴛ1 /*dyn*/ {
    internal CacheLinePad _;
    public bool HasFastMisaligned;
    public bool HasC;
    public bool HasV;
    public bool HasZba;
    public bool HasZbb;
    public bool HasZbs;
    public bool HasZbc;
    public bool HasZvbb;
    public bool HasZvbc;
    public bool HasZvkb;
    public bool HasZvkt;
    public bool HasZvkg;
    public bool HasZvkn;
    public bool HasZvknc;
    public bool HasZvkng;
    public bool HasZvks;
    public bool HasZvksc;
    public bool HasZvksg;
    public nuint VLENB;
    internal CacheLinePad __;
}
public static readonly ж<RISCV64ᴛ1> ᏑRISCV64 = new StaticBox<RISCV64ᴛ1>(default(RISCV64ᴛ1));
public static ref RISCV64ᴛ1 RISCV64 => ref ((StaticBox<RISCV64ᴛ1>)ᏑRISCV64).ValueSlot;

internal static Action doDerived;

[GoInit] internal static void init() {
    archInit();
    initOptions();
    processOptions();
    if (doDerived != nil) {
        doDerived();
    }
}

internal static slice<option> options;

partial struct option {
    public @string Name;
    public ж<bool> Feature;
    public bool Specified;
    public bool Enable;
    public bool Required;
}

internal static void processOptions() {
    @string env = os.Getenv("GODEBUG");
field:
    while (env != ""u8) {
        @string field = "";
        nint i = strings.IndexByte(env, (byte)',');
        if (i < 0) {
            (field, env) = (env, "");
        } else {
            (field, env) = (env.slice(0, i), env.slice(i + 1));
        }
        if (len(field) < 4 || field[..4] != "cpu.") {
            continue;
        }
        i = strings.IndexByte(field, (byte)'=');
        if (i < 0) {
            print((@string)"GODEBUG sys/cpu: no value specified for \"", field, (@string)"\"\n");
            continue;
        }
        @string key = field.slice(4, i);
        @string value = field.slice(i + 1);

        bool enable = default;
        switch (ᐧ) {
        case {} when value == "on"u8: {
            enable = true;
            break;
        }
        case {} when value == "off"u8: {
            enable = false;
            break;
        }
        default: {
            print((@string)"GODEBUG sys/cpu: value \"", value, (@string)"\" not supported for cpu option \"", key, (@string)"\"\n");
            goto continue_field;
            break;
        }}

        if (key == "all"u8) {
            foreach (var iΔ1 in range(len(options))) {
                options[iΔ1].Specified = true;
                options[iΔ1].Enable = enable || options[iΔ1].Required;
            }
            goto continue_field;
        }

        foreach (var iΔ2 in range(len(options))) {
            if (options[iΔ2].Name == key) {
                options[iΔ2].Specified = true;
                options[iΔ2].Enable = enable;
                goto continue_field;
            }
        }

        print((@string)"GODEBUG sys/cpu: unknown cpu feature \"", key, (@string)"\"\n");
continue_field:;
    }
break_field:;

    foreach (var (_, o) in range(options)) {
        if (!o.Specified) {
            continue;
        }

        if (o.Enable && !o.Feature.Value) {
            print((@string)"GODEBUG sys/cpu: can not enable \"", o.Name, (@string)"\", missing CPU support\n");
            continue;
        }

        if (!o.Enable && o.Required) {
            print((@string)"GODEBUG sys/cpu: can not disable \"", o.Name, (@string)"\", required CPU feature\n");
            continue;
        }

        o.Feature.Value = o.Enable;
    }
}

} // end cpu_package
