// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

public static ж<Arch> ArchLoong64 = Ꮡ(new Arch(
    Name: "loong64"u8,
    WordBits: 64,
    WordBytes: 8,
    CarrySafeLoop: true,

    regs: new @string[]{
        // R0 is set to 0.
        // R1 is LR.
        // R2 is ???
        // R3 is SP.
        // R22 is g.
        // R28 and R29 are our virtual carry flags.
        // R30 is the linker/assembler temp, which we use too.
        "R4"u8, "R5"u8, "R6"u8, "R7"u8, "R8"u8, "R9"u8,
        "R10"u8, "R11"u8, "R12"u8, "R13"u8, "R14"u8, "R15"u8, "R16"u8, "R17"u8, "R18"u8, "R19"u8,
        "R20"u8, "R21"u8, "R23"u8, "R24"u8, "R25"u8, "R26"u8, "R27"u8,
        "R31"u8
    }.slice(),
    reg0: "R0"u8,
    regCarry: "R28"u8,
    regAltCarry: "R29"u8,
    regTmp: "R30"u8,

    mov: "MOVV"u8,
    add: "ADDVU"u8,
    sub: "SUBVU"u8,
    sltu: "SGTU"u8,
    mul: "MULV"u8,
    mulhi: "MULHVU"u8,
    lsh: "SLLV"u8,
    rsh: "SRLV"u8,
    and: "AND"u8,
    or: "OR"u8,
    xor: "XOR"u8,

    jmpZero: "BEQ %s, %s"u8,
    jmpNonZero: "BNE %s, %s"u8
));

} // end asmgen_package
