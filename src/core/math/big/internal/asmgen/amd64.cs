// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

[GoLazy(Initial = true)] partial class lazyᴛArchAMD64 { public static ж<Arch> ArchAMD64 = Ꮡ(new Arch(
    Name: "amd64",
    WordBits: 64,
    WordBytes: 8,

    regs: [
        "BX", "SI", "DI",
        "R8", "R9", "R10", "R11", "R12", "R13", "R14", "R15",
        "AX", "DX", "CX" // last to leave available for hinted allocation
    ],
    op3: x86Op3,
    hint: x86Hint,
    memOK: true,
    subCarryIsBorrow: true,

    // Note: Not setting memIndex, because code generally runs faster
    // if we avoid the use of scaled-index memory references,
    // particularly in ADX instructions.

    options: new map<Option, Action<ж<Asm>, @string>>{
        [OptionAltCarry] = amd64JmpADX
    },

    mov: "MOVQ",
    adds: "ADDQ",
    adcs: "ADCQ",
    subs: "SUBQ",
    sbcs: "SBBQ",
    lsh: "SHLQ",
    lshd: "SHLQ",
    rsh: "SHRQ",
    rshd: "SHRQ",
    and: "ANDQ",
    or: "ORQ",
    xor: "XORQ",
    neg: "NEGQ",
    lea: "LEAQ",
    addF: amd64Add,
    mulWideF: x86MulWide,

    addWords: "LEAQ (%[2]s)(%[1]s*8), %[3]s",

    jmpZero: "TESTQ %[1]s, %[1]s; JZ %[2]s",
    jmpNonZero: "TESTQ %[1]s, %[1]s; JNZ %[2]s",
    loopBottom: "SUBQ $1, %[1]s; JNZ %[2]s",
    loopBottomNeg: "ADDQ $1, %[1]s; JNZ %[2]s"
)); }

internal static void amd64JmpADX(ж<Asm> a, @string label) {
    a.Printf("\tCMPB ·hasADX(SB), $0; JNZ %s\n", label);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string adoxqˢ => ref adoxqˢᵇ;
private static readonly @string adoxqˢᵇ = "ADOXQ"u8;
internal static ref readonly @string adcxqˢ => ref adcxqˢᵇ;
private static readonly @string adcxqˢᵇ = "ADCXQ"u8;

internal static bool amd64Add(ж<Asm> Ꮡa, ΔReg src1, ΔReg src2, ΔReg dst, ΔCarry carry) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (a.Enabled(OptionAltCarry)) {
        // If OptionAltCarry is enabled, the generator is emitting ADD instructions
        // both with and without the AltCarry flag set; the AltCarry flag means to
        // use ADOX. Otherwise we have to use ADCX.
        // Using regular ADD/ADC would smash both carry flags,
        // so we reject anything we can't handled with ADCX/ADOX.
        if ((ΔCarry)(carry & UseCarry) != 0 && (ΔCarry)(carry & ((ΔCarry)(SetCarry | SmashCarry))) != 0) {
            if ((ΔCarry)(carry & ΔAltCarry) != 0) {
                Ꮡa.op3(adoxqˢ, src1, src2, dst);
            } else {
                Ꮡa.op3(adcxqˢ, src1, src2, dst);
            }
            return true;
        }
        if ((ΔCarry)(carry & ((ΔCarry)(SetCarry | UseCarry))) == SetCarry && a.IsZero(src1) && src2 == dst) {
            // Clearing carry flag. Caller will add EOL comment.
            Ꮡa.Printf("\tTESTQ AX, AX\n");
            return true;
        }
        if (carry != KeepCarry) {
            Ꮡa.Fatalf("unsupported carry");
        }
    }
    return false;
}

// The x86-prefixed functions are shared with Arch386 in 386.go.
internal static bool x86Op3(@string name) {
    // As far as a.op3 is concerned, there are no 3-op instructions.
    // (We print instructions like MULX ourselves.)
    return false;
}

internal static @string x86Hint(ж<Asm> Ꮡa, Hint h) {
    ref var a = ref Ꮡa.DerefOrNull();

    switch (h) {
    case var ᴛ when ᴛ == HintShiftCount: {
        return "CX";
    }
    case var ᴛ when ᴛ == HintMulSrc: {
        if (a.Enabled(OptionAltCarry)) { // using MULX
            return "DX";
        }
        return "AX";
    }
    case var ᴛ when ᴛ == HintMulHi: {
        if (a.Enabled(OptionAltCarry)) { // using MULX
            return ""u8;
        }
        return "DX";
    }}
    return ""u8;
}

internal static @string x86Suffix(ref Asm a) {
    // Note: Not using a.Arch == Arch386 to avoid init cycle.
    if (a.Arch.Name == "386"u8) {
        return "L";
    }
    return "Q";
}

internal static void x86MulWide(ж<Asm> Ꮡa, ΔReg src1, ΔReg src2, ΔReg dstlo, ΔReg dsthi) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (a.Enabled(OptionAltCarry)) {
        // Using ADCX/ADOX; use MULX to avoid clearing carry flag.
        if (src1.name != "DX"u8) {
            if (src2.name != "DX"u8) {
                Ꮡa.Fatalf("mul src1 or src2 must be DX");
            }
            src2 = src1;
        }
        Ꮡa.Printf("\tMULXQ %s, %s, %s\n", src2, dstlo, dsthi);
        return;
    }

    if (src1.name != "AX"u8) {
        if (src2.name != "AX"u8) {
            Ꮡa.Fatalf("mulwide src1 or src2 must be AX");
        }
        src2 = src1;
    }
    if (dstlo.name != "AX"u8) {
        Ꮡa.Fatalf("mulwide dstlo must be AX");
    }
    if (dsthi.name != "DX"u8) {
        Ꮡa.Fatalf("mulwide dsthi must be DX");
    }
    Ꮡa.Printf("\tMUL%s %s\n", x86Suffix(ref (Ꮡa).DerefOrNull()), src2);
}

} // end asmgen_package
