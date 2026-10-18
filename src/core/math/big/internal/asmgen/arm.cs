// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

public static ж<Arch> ArchARM;
internal static void initᴛArchARM() { ArchARM = Ꮡ(new Arch(
    Name: "arm"u8,
    WordBits: 32,
    WordBytes: 4,
    CarrySafeLoop: true,
    regs: new @string[]{
        // R10 is g.
        // R11 is the assembler/linker temporary (but we use it as a regular register).
        // R13 is SP.
        // R14 is LR.
        // R15 is PC.
        "R0"u8, "R1"u8, "R2"u8, "R3"u8, "R4"u8, "R5"u8, "R6"u8, "R7"u8, "R8"u8, "R9"u8, "R11"u8, "R12"u8
    }.slice(),
    regShift: true,
    mov: "MOVW"u8,
    add: "ADD"u8,
    adds: "ADD.S"u8,
    adc: "ADC"u8,
    adcs: "ADC.S"u8,
    sub: "SUB"u8,
    subs: "SUB.S"u8,
    sbc: "SBC"u8,
    sbcs: "SBC.S"u8,
    rsb: "RSB"u8,
    and: "AND"u8,
    or: "ORR"u8,
    xor: "EOR"u8,
    mulWideF: armMulWide,
    addWords: "ADD %s<<2, %s, %s"u8,
    jmpZero: "TEQ $0, %s; BEQ %s"u8,
    jmpNonZero: "TEQ $0, %s; BNE %s"u8,
    loadIncN: armLoadIncN,
    loadDecN: armLoadDecN,
    storeIncN: armStoreIncN,
    storeDecN: armStoreDecN
)); }

internal static void armMulWide(ж<Asm> Ꮡa, ΔReg src1, ΔReg src2, ΔReg dstlo, ΔReg dsthi) {
    Ꮡa.Printf("\tMULLU %s, %s, (%s, %s)\n"u8, src1, src2, dsthi, dstlo);
}

internal static void armLoadIncN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    foreach (var (_, r) in regs) {
        Ꮡa.Printf("\tMOVW.P %d(%s), %s\n"u8, (~a.Arch).WordBytes, p, r);
    }
}

internal static void armLoadDecN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    foreach (var (_, r) in regs) {
        Ꮡa.Printf("\tMOVW.W %d(%s), %s\n"u8, -(~a.Arch).WordBytes, p, r);
    }
}

internal static void armStoreIncN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    foreach (var (_, r) in regs) {
        Ꮡa.Printf("\tMOVW.P %s, %d(%s)\n"u8, r, (~a.Arch).WordBytes, p);
    }
}

internal static void armStoreDecN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    foreach (var (_, r) in regs) {
        Ꮡa.Printf("\tMOVW.W %s, %d(%s)\n"u8, r, -(~a.Arch).WordBytes, p);
    }
}

} // end asmgen_package
