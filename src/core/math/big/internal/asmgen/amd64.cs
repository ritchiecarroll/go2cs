// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

[GoLazy(Initial = true)] partial class lazyᴛArchAMD64 { public static ж<Arch> ArchAMD64 = Ꮡ(new Arch(
    Name: "amd64"u8,
    WordBits: 64,
    WordBytes: 8,

    regs: new @string[]{
        "BX"u8, "SI"u8, "DI"u8,
        "R8"u8, "R9"u8, "R10"u8, "R11"u8, "R12"u8, "R13"u8, "R14"u8, "R15"u8,
        "AX"u8, "DX"u8, "CX"u8 // last to leave available for hinted allocation
    }.slice(),
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

    mov: "MOVQ"u8,
    adds: "ADDQ"u8,
    adcs: "ADCQ"u8,
    subs: "SUBQ"u8,
    sbcs: "SBBQ"u8,
    lsh: "SHLQ"u8,
    lshd: "SHLQ"u8,
    rsh: "SHRQ"u8,
    rshd: "SHRQ"u8,
    and: "ANDQ"u8,
    or: "ORQ"u8,
    xor: "XORQ"u8,
    neg: "NEGQ"u8,
    lea: "LEAQ"u8,
    addF: amd64Add,
    mulWideF: x86MulWide,

    addWords: "LEAQ (%[2]s)(%[1]s*8), %[3]s"u8,

    jmpZero: "TESTQ %[1]s, %[1]s; JZ %[2]s"u8,
    jmpNonZero: "TESTQ %[1]s, %[1]s; JNZ %[2]s"u8,
    loopBottom: "SUBQ $1, %[1]s; JNZ %[2]s"u8,
    loopBottomNeg: "ADDQ $1, %[1]s; JNZ %[2]s"u8
)); }

internal static void amd64JmpADX(ж<Asm> Ꮡa, @string label) {
    Ꮡa.Printf("\tCMPB ·hasADX(SB), $0; JNZ %s\n"u8, label);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string adoxqˢ = "ADOXQ"u8;
internal static readonly @string adcxqˢ = "ADCXQ"u8;

internal static bool amd64Add(ж<Asm> Ꮡa, ΔReg src1, ΔReg src2, ΔReg dst, ΔCarry carry) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (a.Enabled(OptionAltCarry)) {
        // If OptionAltCarry is enabled, the generator is emitting ADD instructions
        // both with and without the AltCarry flag set; the AltCarry flag means to
        // use ADOX. Otherwise we have to use ADCX.
        // Using regular ADD/ADC would smash both carry flags,
        // so we reject anything we can't handled with ADCX/ADOX.
        if ((ΔCarry)(carry & UseCarry) != 0 && (ΔCarry)(carry & ((ΔCarry)(SetCarry | SmashCarry))) != 0) {
            if ((ΔCarry)(carry & ΔAltCarry) != 0){
                Ꮡa.op3(adoxqˢ, src1, src2, dst);
            } else {
                Ꮡa.op3(adcxqˢ, src1, src2, dst);
            }
            return true;
        }
        if ((ΔCarry)(carry & ((ΔCarry)(SetCarry | UseCarry))) == SetCarry && a.IsZero(src1) && src2 == dst) {
            // Clearing carry flag. Caller will add EOL comment.
            Ꮡa.Printf("\tTESTQ AX, AX\n"u8);
            return true;
        }
        if (carry != KeepCarry) {
            Ꮡa.Fatalf("unsupported carry"u8);
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

    var exprᴛ1 = h;
    if (exprᴛ1 == HintShiftCount) {
        return "CX"u8;
    }
    if (exprᴛ1 == HintMulSrc) {
        if (a.Enabled(OptionAltCarry)) { // using MULX
            return "DX"u8;
        }
        return "AX"u8;
    }
    if (exprᴛ1 == HintMulHi) {
        if (a.Enabled(OptionAltCarry)) { // using MULX
            return ""u8;
        }
        return "DX"u8;
    }
    return ""u8;
}

internal static @string x86Suffix(ref Asm a) {
    // Note: Not using a.Arch == Arch386 to avoid init cycle.
    if ((~a.Arch).Name == "386"u8) {
        return "L"u8;
    }
    return "Q"u8;
}

internal static void x86MulWide(ж<Asm> Ꮡa, ΔReg src1, ΔReg src2, ΔReg dstlo, ΔReg dsthi) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (a.Enabled(OptionAltCarry)) {
        // Using ADCX/ADOX; use MULX to avoid clearing carry flag.
        if (src1.name != "DX"u8) {
            if (src2.name != "DX"u8) {
                Ꮡa.Fatalf("mul src1 or src2 must be DX"u8);
            }
            src2 = src1;
        }
        Ꮡa.Printf("\tMULXQ %s, %s, %s\n"u8, src2, dstlo, dsthi);
        return;
    }

    if (src1.name != "AX"u8) {
        if (src2.name != "AX"u8) {
            Ꮡa.Fatalf("mulwide src1 or src2 must be AX"u8);
        }
        src2 = src1;
    }
    if (dstlo.name != "AX"u8) {
        Ꮡa.Fatalf("mulwide dstlo must be AX"u8);
    }
    if (dsthi.name != "DX"u8) {
        Ꮡa.Fatalf("mulwide dsthi must be DX"u8);
    }
    Ꮡa.Printf("\tMUL%s %s\n"u8, x86Suffix(ref (Ꮡa).DerefOrNull()), src2);
}

} // end asmgen_package
