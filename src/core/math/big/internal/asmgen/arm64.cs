// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

[GoLazy(Initial = true)] partial class lazyᴛArchARM64 { public static ж<Arch> ArchARM64 = Ꮡ(new Arch(
    Name: "arm64"u8,
    WordBits: 64,
    WordBytes: 8,
    CarrySafeLoop: true,

    regs: slice<@string>([
        // R18 is the platform register.
        // R27 is the assembler/linker temporary (which we could potentially use but don't).
        // R28 is g.
        // R29 is FP.
        // R30 is LR.
        "R0"u8, "R1"u8, "R2"u8, "R3"u8, "R4"u8, "R5"u8, "R6"u8, "R7"u8, "R8"u8, "R9"u8,
        "R10"u8, "R11"u8, "R12"u8, "R13"u8, "R14"u8, "R15"u8, "R16"u8, "R17"u8, "R19"u8,
        "R20"u8, "R21"u8, "R22"u8, "R23"u8, "R24"u8, "R25"u8, "R26"u8
    ]),
    reg0: "ZR"u8,

    mov: "MOVD"u8,
    add: "ADD"u8,
    adds: "ADDS"u8,
    adc: "ADC"u8,
    adcs: "ADCS"u8,
    sub: "SUB"u8,
    subs: "SUBS"u8,
    sbc: "SBC"u8,
    sbcs: "SBCS"u8,
    mul: "MUL"u8,
    mulhi: "UMULH"u8,
    lsh: "LSL"u8,
    rsh: "LSR"u8,
    and: "AND"u8,
    or: "ORR"u8,
    xor: "EOR"u8,

    addWords: "ADD %[1]s<<3, %[2]s, %[3]s"u8,

    jmpZero: "CBZ %s, %s"u8,
    jmpNonZero: "CBNZ %s, %s"u8,

    loadIncN: arm64LoadIncN,
    loadDecN: arm64LoadDecN,
    storeIncN: arm64StoreIncN,
    storeDecN: arm64StoreDecN
)); }

internal static void arm64LoadIncN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (len(regs) == 1) {
        Ꮡa.Printf("\tMOVD.P %d(%s), %s\n"u8, a.Arch.WordBytes, p, regs[0]);
        return;
    }
    Ꮡa.Printf("\tLDP.P %d(%s), (%s, %s)\n"u8, len(regs) * a.Arch.WordBytes, p, regs[0], regs[1]);
    nint i = default;
    for (i = 2; i + 2 <= len(regs); i += 2) {
        Ꮡa.Printf("\tLDP %d(%s), (%s, %s)\n"u8, (i - len(regs)) * a.Arch.WordBytes, p, regs[i], regs[i + 1]);
    }
    if (i < len(regs)) {
        Ꮡa.Printf("\tMOVD %d(%s), %s\n"u8, -1 * a.Arch.WordBytes, p, regs[i]);
    }
}

internal static void arm64LoadDecN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (len(regs) == 1) {
        Ꮡa.Printf("\tMOVD.W -%d(%s), %s\n"u8, a.Arch.WordBytes, p, regs[0]);
        return;
    }
    Ꮡa.Printf("\tLDP.W %d(%s), (%s, %s)\n"u8, -len(regs) * a.Arch.WordBytes, p, regs[len(regs) - 1], regs[len(regs) - 2]);
    nint i = default;
    for (i = 2; i + 2 <= len(regs); i += 2) {
        Ꮡa.Printf("\tLDP %d(%s), (%s, %s)\n"u8, i * a.Arch.WordBytes, p, regs[len(regs) - 1 - i], regs[len(regs) - 2 - i]);
    }
    if (i < len(regs)) {
        Ꮡa.Printf("\tMOVD %d(%s), %s\n"u8, i * a.Arch.WordBytes, p, regs[0]);
    }
}

internal static void arm64StoreIncN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (len(regs) == 1) {
        Ꮡa.Printf("\tMOVD.P %s, %d(%s)\n"u8, regs[0], a.Arch.WordBytes, p);
        return;
    }
    Ꮡa.Printf("\tSTP.P (%s, %s), %d(%s)\n"u8, regs[0], regs[1], len(regs) * a.Arch.WordBytes, p);
    nint i = default;
    for (i = 2; i + 2 <= len(regs); i += 2) {
        Ꮡa.Printf("\tSTP (%s, %s), %d(%s)\n"u8, regs[i], regs[i + 1], (i - len(regs)) * a.Arch.WordBytes, p);
    }
    if (i < len(regs)) {
        Ꮡa.Printf("\tMOVD %s, %d(%s)\n"u8, regs[i], -1 * a.Arch.WordBytes, p);
    }
}

internal static void arm64StoreDecN(ж<Asm> Ꮡa, RegPtr p, slice<ΔReg> regs) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (len(regs) == 1) {
        Ꮡa.Printf("\tMOVD.W %s, -%d(%s)\n"u8, regs[0], a.Arch.WordBytes, p);
        return;
    }
    Ꮡa.Printf("\tSTP.W (%s, %s), %d(%s)\n"u8, regs[len(regs) - 1], regs[len(regs) - 2], -len(regs) * a.Arch.WordBytes, p);
    nint i = default;
    for (i = 2; i + 2 <= len(regs); i += 2) {
        Ꮡa.Printf("\tSTP (%s, %s), %d(%s)\n"u8, regs[len(regs) - 1 - i], regs[len(regs) - 2 - i], i * a.Arch.WordBytes, p);
    }
    if (i < len(regs)) {
        Ꮡa.Printf("\tMOVD %s, %d(%s)\n"u8, regs[0], i * a.Arch.WordBytes, p);
    }
}

} // end asmgen_package
