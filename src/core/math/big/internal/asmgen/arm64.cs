// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

[GoLazy(Initial = true)] partial class lazyᴛArchARM64 { public static ж<Arch> ArchARM64 = Ꮡ(new Arch(
    Name: "arm64",
    WordBits: 64,
    WordBytes: 8,
    CarrySafeLoop: true,

    regs: slice<@string>([
        // R18 is the platform register.
        // R27 is the assembler/linker temporary (which we could potentially use but don't).
        // R28 is g.
        // R29 is FP.
        // R30 is LR.
        "R0", "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9",
        "R10", "R11", "R12", "R13", "R14", "R15", "R16", "R17", "R19",
        "R20", "R21", "R22", "R23", "R24", "R25", "R26"
    ]),
    reg0: "ZR",

    mov: "MOVD",
    add: "ADD",
    adds: "ADDS",
    adc: "ADC",
    adcs: "ADCS",
    sub: "SUB",
    subs: "SUBS",
    sbc: "SBC",
    sbcs: "SBCS",
    mul: "MUL",
    mulhi: "UMULH",
    lsh: "LSL",
    rsh: "LSR",
    and: "AND",
    or: "ORR",
    xor: "EOR",

    addWords: "ADD %[1]s<<3, %[2]s, %[3]s",

    jmpZero: "CBZ %s, %s",
    jmpNonZero: "CBNZ %s, %s",

    loadIncN: arm64LoadIncN,
    loadDecN: arm64LoadDecN,
    storeIncN: arm64StoreIncN,
    storeDecN: arm64StoreDecN
)); }

internal static void arm64LoadIncN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (len(regs) == 1) {
        Ꮡa.Printf("\tMOVD.P %d(%s), %s\n", a.Arch.WordBytes, p, regs[0]);
        return;
    }
    Ꮡa.Printf("\tLDP.P %d(%s), (%s, %s)\n", len(regs) * a.Arch.WordBytes, p, regs[0], regs[1]);
    nint i = default;
    for (i = 2; i + 2 <= len(regs); i += 2) {
        Ꮡa.Printf("\tLDP %d(%s), (%s, %s)\n", (i - len(regs)) * a.Arch.WordBytes, p, regs[i], regs[i + 1]);
    }
    if (i < len(regs)) {
        Ꮡa.Printf("\tMOVD %d(%s), %s\n", -1 * a.Arch.WordBytes, p, regs[i]);
    }
}

internal static void arm64LoadDecN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (len(regs) == 1) {
        Ꮡa.Printf("\tMOVD.W -%d(%s), %s\n", a.Arch.WordBytes, p, regs[0]);
        return;
    }
    Ꮡa.Printf("\tLDP.W %d(%s), (%s, %s)\n", -len(regs) * a.Arch.WordBytes, p, regs[len(regs) - 1], regs[len(regs) - 2]);
    nint i = default;
    for (i = 2; i + 2 <= len(regs); i += 2) {
        Ꮡa.Printf("\tLDP %d(%s), (%s, %s)\n", i * a.Arch.WordBytes, p, regs[len(regs) - 1 - i], regs[len(regs) - 2 - i]);
    }
    if (i < len(regs)) {
        Ꮡa.Printf("\tMOVD %d(%s), %s\n", i * a.Arch.WordBytes, p, regs[0]);
    }
}

internal static void arm64StoreIncN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (len(regs) == 1) {
        Ꮡa.Printf("\tMOVD.P %s, %d(%s)\n", regs[0], a.Arch.WordBytes, p);
        return;
    }
    Ꮡa.Printf("\tSTP.P (%s, %s), %d(%s)\n", regs[0], regs[1], len(regs) * a.Arch.WordBytes, p);
    nint i = default;
    for (i = 2; i + 2 <= len(regs); i += 2) {
        Ꮡa.Printf("\tSTP (%s, %s), %d(%s)\n", regs[i], regs[i + 1], (i - len(regs)) * a.Arch.WordBytes, p);
    }
    if (i < len(regs)) {
        Ꮡa.Printf("\tMOVD %s, %d(%s)\n", regs[i], -1 * a.Arch.WordBytes, p);
    }
}

internal static void arm64StoreDecN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (len(regs) == 1) {
        Ꮡa.Printf("\tMOVD.W %s, -%d(%s)\n", regs[0], a.Arch.WordBytes, p);
        return;
    }
    Ꮡa.Printf("\tSTP.W (%s, %s), %d(%s)\n", regs[len(regs) - 1], regs[len(regs) - 2], -len(regs) * a.Arch.WordBytes, p);
    nint i = default;
    for (i = 2; i + 2 <= len(regs); i += 2) {
        Ꮡa.Printf("\tSTP (%s, %s), %d(%s)\n", regs[len(regs) - 1 - i], regs[len(regs) - 2 - i], i * a.Arch.WordBytes, p);
    }
    if (i < len(regs)) {
        Ꮡa.Printf("\tMOVD %s, %d(%s)\n", regs[0], i * a.Arch.WordBytes, p);
    }
}

} // end asmgen_package
