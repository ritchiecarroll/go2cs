// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

[GoLazy(Initial = true)] partial class lazyᴛArchS390X { public static ж<Arch> ArchS390X = Ꮡ(new Arch(
    Name: "s390x"u8,
    WordBits: 64,
    WordBytes: 8,
    CarrySafeLoop: true,

    regs: new @string[]{
        // R0 is 0 by convention in this code (see setup).
        // R10 is the assembler/linker temporary.
        // R11 is a second assembler/linker temporary, for wide multiply.
        // We allow allocating R10 and R11 so that we can use them as
        // direct multiplication targets while tracking whether they're in use.
        // R13 is g.
        // R14 is LR.
        // R15 is SP.
        "R1"u8, "R2"u8, "R3"u8, "R4"u8, "R5"u8, "R6"u8, "R7"u8, "R8"u8, "R9"u8,
        "R10"u8, "R11"u8, "R12"u8
    }.slice(),
    reg0: "R0"u8,
    regTmp: "R10"u8,
    setup: s390xSetup,
    maxColumns: 2,
    op3: s390xOp3,
    hint: s390xHint,

    // Instruction reference: chapter 7 of
    // https://www.ibm.com/docs/en/SSQ2R2_15.0.0/com.ibm.tpf.toolkit.hlasm.doc/dz9zr006.pdf

    mov: "MOVD"u8,
    adds: "ADDC"u8, // ADD is an alias for ADDC, sets carry
    adcs: "ADDE"u8,
    subs: "SUBC"u8, // SUB is an alias for SUBC, sets carry
    sbcs: "SUBE"u8,
    mulWideF: s390MulWide,
    lsh: "SLD"u8,
    rsh: "SRD"u8,
    and: "AND"u8,
    or: "OR"u8,
    xor: "XOR"u8,
    neg: "NEG"u8,
    lea: "LAY"u8, // LAY because LA only accepts positive offsets

    jmpZero: "CMPBEQ %s, $0, %s"u8,
    jmpNonZero: "CMPBNE %s, $0, %s"u8
)); }

internal static void s390xSetup(ж<ΔFunc> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    var a = f.Asm;
    if (f.Name == "addVV"u8 || f.Name == "subVV"u8) {
        // S390x, unlike every other system, has vector instructions
        // that can propagate carry bits during parallel adds (VACC).
        // Instead of trying to generate that for this one system,
        // jump to the hand-written code in arithvec_s390x.s.
        a.Printf("\tMOVB ·hasVX(SB), R1\n"u8);
        a.Printf("\tCMPBEQ R1, $0, novec\n"u8);
        a.Printf("\tJMP ·%svec(SB)\n"u8, f.Name);
        a.Printf("novec:\n"u8);
    }
    a.Printf("\tMOVD $0, R0\n"u8);
}

internal static bool s390xOp3(@string name) {
    if (name == "AND"u8) { // AND with immediate only takes imm, reg; not imm, reg, reg.
        return false;
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string r11ˢ = "R11"u8;
internal static readonly @string r10ˢ = "R10"u8;

internal static @string s390xHint(ж<Asm> _, Hint h) {
    var exprᴛ1 = h;
    if (exprᴛ1 == HintMulSrc) {
        return r11ˢ;
    }
    if (exprᴛ1 == HintMulHi) {
        return r10ˢ;
    }
    return ""u8;
}

internal static void s390MulWide(ж<Asm> Ꮡa, ΔReg src1, ΔReg src2, ΔReg dstlo, ΔReg dsthi) {
    if (src1.name != "R11"u8 && src2.name != "R11"u8) {
        Ꮡa.Fatalf("mulWide src1 or src2 must be R11"u8);
    }
    if (dstlo.name != "R11"u8) {
        Ꮡa.Fatalf("mulWide dstlo must be R11"u8);
    }
    if (dsthi.name != "R10"u8) {
        Ꮡa.Fatalf("mulWide dsthi must be R10"u8);
    }
    var src = src1;
    if (src.name == "R11"u8) {
        src = src2;
    }
    Ꮡa.Printf("\tMLGR %s, R10\n"u8, src);
}

} // end asmgen_package
