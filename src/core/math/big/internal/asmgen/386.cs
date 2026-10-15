// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

using fmt = fmt_package;

partial class asmgen_package {

public static ж<Arch> Arch386;
internal static void initᴛArch386() { Arch386 = Ꮡ(new Arch(
    Name: "386"u8,
    WordBits: 32,
    WordBytes: 4,
    regs: new @string[]{
        "BX"u8, "SI"u8, "DI"u8, "BP"u8,
        "CX"u8, "DX"u8, "AX"u8 // last, to leave available for hinted allocation
    }.slice(),
    op3: x86Op3,
    hint: x86Hint,
    memOK: true,
    subCarryIsBorrow: true,
    maxColumns: 1, // not enough registers for more

    // Note: It would be nice to not set memIndex and then
    // delete all the code in pipe.go that supports it.
    // But a few routines, notably lshVU and mulAddVWW,
    // benefit dramatically from the use of index registers.
    // Perhaps some day we will decide 386 performance
    // does not matter enough to keep this code.
    memIndex: _386MemIndex,
    mov: "MOVL"u8,
    adds: "ADDL"u8,
    adcs: "ADCL"u8,
    subs: "SUBL"u8,
    sbcs: "SBBL"u8,
    lsh: "SHLL"u8,
    lshd: "SHLL"u8,
    rsh: "SHRL"u8,
    rshd: "SHRL"u8,
    and: "ANDL"u8,
    or: "ORL"u8,
    xor: "XORL"u8,
    neg: "NEGL"u8,
    lea: "LEAL"u8,
    mulWideF: x86MulWide,
    addWords: "LEAL (%[2]s)(%[1]s*4), %[3]s"u8,
    jmpZero: "TESTL %[1]s, %[1]s; JZ %[2]s"u8,
    jmpNonZero: "TESTL %[1]s, %[1]s; JNZ %[2]s"u8,
    loopBottom: "SUBL $1, %[1]s; JNZ %[2]s"u8,
    loopBottomNeg: "ADDL $1, %[1]s; JNZ %[2]s"u8
)); }

internal static ΔReg _386MemIndex(ж<Asm> Ꮡa, nint off, ΔReg ix, RegPtr p) {
    ref var a = ref Ꮡa.DerefOrNull();

    return new ΔReg(fmt.Sprintf("%d(%s)(%s*%d)"u8, off, p, ix, (~a.Arch).WordBytes));
}

} // end asmgen_package
