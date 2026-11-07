// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

[GoLazy(Initial = true)] partial class lazyᴛArchS390X { public static ж<Arch> ArchS390X = Ꮡ(new Arch(
    Name: "s390x",
    WordBits: 64,
    WordBytes: 8,
    CarrySafeLoop: true,

    regs: [
        // R0 is 0 by convention in this code (see setup).
        // R10 is the assembler/linker temporary.
        // R11 is a second assembler/linker temporary, for wide multiply.
        // We allow allocating R10 and R11 so that we can use them as
        // direct multiplication targets while tracking whether they're in use.
        // R13 is g.
        // R14 is LR.
        // R15 is SP.
        "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9",
        "R10", "R11", "R12"
    ],
    reg0: "R0",
    regTmp: "R10",
    setup: s390xSetup,
    maxColumns: 2,
    op3: s390xOp3,
    hint: s390xHint,

    // Instruction reference: chapter 7 of
    // https://www.ibm.com/docs/en/SSQ2R2_15.0.0/com.ibm.tpf.toolkit.hlasm.doc/dz9zr006.pdf

    mov: "MOVD",
    adds: "ADDC", // ADD is an alias for ADDC, sets carry
    adcs: "ADDE",
    subs: "SUBC", // SUB is an alias for SUBC, sets carry
    sbcs: "SUBE",
    mulWideF: s390MulWide,
    lsh: "SLD",
    rsh: "SRD",
    and: "AND",
    or: "OR",
    xor: "XOR",
    neg: "NEG",
    lea: "LAY", // LAY because LA only accepts positive offsets

    jmpZero: "CMPBEQ %s, $0, %s",
    jmpNonZero: "CMPBNE %s, $0, %s"
)); }

internal static void s390xSetup(ж<ΔFunc> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    var a = f.Asm;
    if (f.Name == "addVV"u8 || f.Name == "subVV"u8) {
        // S390x, unlike every other system, has vector instructions
        // that can propagate carry bits during parallel adds (VACC).
        // Instead of trying to generate that for this one system,
        // jump to the hand-written code in arithvec_s390x.s.
        a.Printf("\tMOVB ·hasVX(SB), R1\n");
        a.Printf("\tCMPBEQ R1, $0, novec\n");
        a.Printf("\tJMP ·%svec(SB)\n", f.Name);
        a.Printf("novec:\n");
    }
    a.Printf("\tMOVD $0, R0\n");
}

internal static bool s390xOp3(@string name) {
    if (name == "AND"u8) { // AND with immediate only takes imm, reg; not imm, reg, reg.
        return false;
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string r11ˢ => ref r11ˢᵇ;
private static readonly @string r11ˢᵇ = "R11"u8;
internal static ref readonly @string r10ˢ => ref r10ˢᵇ;
private static readonly @string r10ˢᵇ = "R10"u8;

internal static @string s390xHint(ж<Asm> _, Hint h) {
    switch (h) {
    case var ᴛ when ᴛ == HintMulSrc: {
        return r11ˢ;
    }
    case var ᴛ when ᴛ == HintMulHi: {
        return r10ˢ;
    }}
    return ""u8;
}

internal static void s390MulWide(ж<Asm> a, ΔReg src1, ΔReg src2, ΔReg dstlo, ΔReg dsthi) {
    if (src1.name != "R11"u8 && src2.name != "R11"u8) {
        a.Fatalf("mulWide src1 or src2 must be R11");
    }
    if (dstlo.name != "R11"u8) {
        a.Fatalf("mulWide dstlo must be R11");
    }
    if (dsthi.name != "R10"u8) {
        a.Fatalf("mulWide dsthi must be R10");
    }
    var src = src1;
    if (src.name == "R11"u8) {
        src = src2;
    }
    a.Printf("\tMLGR %s, R10\n", src);
}

} // end asmgen_package
