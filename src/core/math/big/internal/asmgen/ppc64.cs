// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

public static ж<Arch> ArchPPC64x = Ꮡ(new Arch(
    Name: "ppc64x"u8,
    Build: "ppc64 || ppc64le"u8,
    WordBits: 64,
    WordBytes: 8,
    CarrySafeLoop: true,

    // Note: The old, hand-written ppc64x assembly used MOVDU
    // to avoid explicit pointer updates in a few routines, but the new
    // generated code runs just as fast, so we haven't bothered to try
    // to add that back. (It's not trivial; you'd have to keep the pointers
    // shifted one word in order to make the semantics work.)
    //
    // The old assembly also used some complex vector instructions
    // to implement lshVU and rshVU, but the generated code that uses
    // ordinary integer instructions is much faster than the vector code was,
    // at least on the power10 gomote.
    regs: new @string[]{
        // R0 is 0 by convention.
        // R1 is SP.
        // R2 is TOC.
        // R30 is g.
        // R31 is the assembler/linker temporary (which we use too).
        "R3"u8, "R4"u8, "R5"u8, "R6"u8, "R7"u8, "R8"u8, "R9"u8,
        "R10"u8, "R11"u8, "R12"u8, /*R13 is TLS*/ "R14"u8, "R15"u8, "R16"u8, "R17"u8, "R18"u8, "R19"u8,
        "R20"u8, "R21"u8, "R22"u8, "R23"u8, "R24"u8, "R25"u8, "R26"u8, "R27"u8, "R28"u8, "R29"u8
    }.slice(),
    reg0: "R0"u8,
    regTmp: "R31"u8,

    // Note: Could write an addF and subF to use ADDZE and SUBZE,
    // but we have R0 so it doesn't seem to matter much.
    mov: "MOVD"u8,
    add: "ADD"u8,
    adds: "ADDC"u8,
    adcs: "ADDE"u8,
    sub: "SUB"u8,
    subs: "SUBC"u8,
    sbcs: "SUBE"u8,
    mul: "MULLD"u8,
    mulhi: "MULHDU"u8,
    lsh: "SLD"u8,
    rsh: "SRD"u8,
    and: "ANDCC"u8, // regular AND does not accept immediates
    or: "OR"u8,
    xor: "XOR"u8,
    jmpZero: "CMP %[1]s, $0; BEQ %[2]s"u8,
    jmpNonZero: "CMP %s, $0; BNE %s"u8,

    // Note: Using CTR means that we could free the count register
    // during the loop body, but the portable logic doesn't know that,
    // and we're not hurting for registers.
    loopTop: "CMP %[1]s, $0; BEQ %[2]s; MOVD %[1]s, CTR"u8,
    loopBottom: "BDNZ %[2]s"u8
));

} // end asmgen_package
