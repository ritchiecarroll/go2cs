// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

[GoLazy(Initial = true)] partial class lazyᴛArchMIPS64x { public static ж<Arch> ArchMIPS64x = Ꮡ(new Arch(
    Name: "mips64x"u8,
    Build: "mips64 || mips64le"u8,
    WordBits: 64,
    WordBytes: 8,
    CarrySafeLoop: true,

    regs: slice<@string>([
        // R0 is 0
        // R23 is the assembler/linker temporary (which we use too).
        // R24 and R25 are our virtual carry flags.
        // R28 is SB.
        // R29 is SP.
        // R30 is g.
        // R31 is LR.
        "R1"u8, "R2"u8, "R3"u8, "R4"u8, "R5"u8, "R6"u8, "R7"u8, "R8"u8, "R9"u8,
        "R10"u8, "R11"u8, "R12"u8, "R13"u8, "R14"u8, "R15"u8, "R16"u8, "R17"u8, "R18"u8, "R19"u8,
        "R20"u8, "R21"u8, "R22"u8, "R24"u8, "R25"u8
    ]),
    reg0: "R0"u8,
    regTmp: "R23"u8,
    regCarry: "R24"u8,
    regAltCarry: "R25"u8,

    mov: "MOVV"u8,
    add: "ADDVU"u8,
    sltu: "SGTU"u8, // SGTU args are swapped, so it's really SLTU
    sub: "SUBVU"u8,
    mulWideF: mips64MulWide,
    lsh: "SLLV"u8,
    rsh: "SRLV"u8,
    and: "AND"u8,
    or: "OR"u8,
    xor: "XOR"u8,

    jmpZero: "BEQ %s, %s"u8,
    jmpNonZero: "BNE %s, %s"u8
)); }

internal static void mips64MulWide(ж<Asm> a, ΔReg src1, ΔReg src2, ΔReg dstlo, ΔReg dsthi) {
    a.Printf("\tMULVU %s, %s\n\tMOVV LO, %s\n\tMOVV HI, %s\n"u8, src1, src2, dstlo, dsthi);
}

} // end asmgen_package
