// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

[GoLazy(Initial = true)] partial class lazyᴛArchARM { public static ж<Arch> ArchARM = Ꮡ(new Arch(
    Name: "arm",
    WordBits: 32,
    WordBytes: 4,
    CarrySafeLoop: true,

    regs: [
        // R10 is g.
        // R11 is the assembler/linker temporary (but we use it as a regular register).
        // R13 is SP.
        // R14 is LR.
        // R15 is PC.
        "R0", "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R11", "R12"
    ],
    regShift: true,

    mov: "MOVW",
    add: "ADD",
    adds: "ADD.S",
    adc: "ADC",
    adcs: "ADC.S",
    sub: "SUB",
    subs: "SUB.S",
    sbc: "SBC",
    sbcs: "SBC.S",
    rsb: "RSB",
    and: "AND",
    or: "ORR",
    xor: "EOR",

    mulWideF: armMulWide,

    addWords: "ADD %s<<2, %s, %s",

    jmpZero: "TEQ $0, %s; BEQ %s",
    jmpNonZero: "TEQ $0, %s; BNE %s",

    loadIncN: armLoadIncN,
    loadDecN: armLoadDecN,
    storeIncN: armStoreIncN,
    storeDecN: armStoreDecN
)); }

internal static void armMulWide(ж<Asm> a, Reg src1, Reg src2, Reg dstlo, Reg dsthi) {
    a.Printf("\tMULLU %s, %s, (%s, %s)\n", src1, src2, dsthi, dstlo);
}

internal static void armLoadIncN(ж<Asm> Ꮡa, RegPtr p, slice<Reg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    foreach (var (_, r) in range(regs)) {
        Ꮡa.Printf("\tMOVW.P %d(%s), %s\n", a.Arch.WordBytes, p, r);
    }
}

internal static void armLoadDecN(ж<Asm> Ꮡa, RegPtr p, slice<Reg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    foreach (var (_, r) in range(regs)) {
        Ꮡa.Printf("\tMOVW.W %d(%s), %s\n", -a.Arch.WordBytes, p, r);
    }
}

internal static void armStoreIncN(ж<Asm> Ꮡa, RegPtr p, slice<Reg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    foreach (var (_, r) in range(regs)) {
        Ꮡa.Printf("\tMOVW.P %s, %d(%s)\n", r, a.Arch.WordBytes, p);
    }
}

internal static void armStoreDecN(ж<Asm> Ꮡa, RegPtr p, slice<Reg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    foreach (var (_, r) in range(regs)) {
        Ꮡa.Printf("\tMOVW.W %s, %d(%s)\n", r, -a.Arch.WordBytes, p);
    }
}

} // end asmgen_package
