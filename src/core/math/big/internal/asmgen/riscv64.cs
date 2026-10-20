// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

public static ж<Arch> ArchRISCV64 = Ꮡ(new Arch(
    Name: "riscv64"u8,
    WordBits: 64,
    WordBytes: 8,
    CarrySafeLoop: true,
    regs: new @string[]{
        // X0 is zero.
        // X1 is LR.
        // X2 is SP.
        // X3 is SB.
        // X4 is TP.
        // X27 is g.
        // X28 and X29 are our virtual carry flags.
        // X31 is the assembler/linker temporary (which we use too).
        "X5"u8, "X6"u8, "X7"u8, "X8"u8, "X9"u8,
        "X10"u8, "X11"u8, "X12"u8, "X13"u8, "X14"u8, "X15"u8, "X16"u8, "X17"u8, "X18"u8, "X19"u8,
        "X20"u8, "X21"u8, "X22"u8, "X23"u8, "X24"u8, "X25"u8, "X26"u8,
        "X30"u8
    }.slice(),
    reg0: "X0"u8,
    regCarry: "X28"u8,
    regAltCarry: "X29"u8,
    regTmp: "X31"u8,
    mov: "MOV"u8,
    add: "ADD"u8,
    sub: "SUB"u8,
    mul: "MUL"u8,
    mulhi: "MULHU"u8,
    lsh: "SLL"u8,
    rsh: "SRL"u8,
    and: "AND"u8,
    or: "OR"u8,
    xor: "XOR"u8,
    sltu: "SLTU"u8,
    jmpZero: "BEQZ %s, %s"u8,
    jmpNonZero: "BNEZ %s, %s"u8
));

} // end asmgen_package
