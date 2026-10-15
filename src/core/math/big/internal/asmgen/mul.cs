// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string funcMulAddVWWZXWordMAˢ = "func mulAddVWW(z, x []Word, m, a Word) (c Word)"u8;

// mulAddVWW generates mulAddVWW, which does z, c = x*m + a.
internal static void mulAddVWW(ж<Asm> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    var f = Ꮡa.Func(funcMulAddVWWZXWordMAˢ);

    if (a.AltCarry().Valid()) {
        addMulVirtualCarry(f, 0);
        return;
    }
    addMul(f, ""u8, "x"u8, 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string funcAddMulVVWWZXYWordMAˢ = "func addMulVVWW(z, x, y []Word, m, a Word) (c Word)"u8;
internal static readonly @string altcarryˢ = "altcarry"u8;

// addMulVVWW generates addMulVVWW which does z, c = x + y*m + a.
// (A more pedantic name would be addMulAddVVWW.)
internal static void addMulVVWW(ж<Asm> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    var f = Ꮡa.Func(funcAddMulVVWWZXYWordMAˢ);

    // If the architecture has virtual carries, emit that version unconditionally.
    if (a.AltCarry().Valid()) {
        addMulVirtualCarry(f, 1);
        return;
    }

    // If the architecture optionally has two carries, test and emit both versions.
    if (Ꮡa.JmpEnable(OptionAltCarry, altcarryˢ)) {
        var regs = a.RegsUsed();
        addMul(f, "x"u8, "y"u8, 1);
        Ꮡa.Label(altcarryˢ);
        a.SetOption(OptionAltCarry, true);
        a.SetRegsUsed(regs);
        addMulAlt(f);
        a.SetOption(OptionAltCarry, false);
        return;
    }

    // Otherwise emit the one-carry form.
    addMul(f, "x"u8, "y"u8, 1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string multiplyˢ = "multiply"u8;
internal static readonly @string addˢ = "add"u8;

// Computing z = addsrc + m*mulsrc + a, we need:
//
//	for i := range z {
//		lo, hi := m * mulsrc[i]
//		lo, carry = bits.Add(lo, a, 0)
//		lo, carryAlt = bits.Add(lo, addsrc[i], 0)
//		z[i] = lo
//		a = hi + carry + carryAlt  // cannot overflow
//	}
//
// The final addition cannot overflow because after processing N words,
// the maximum possible value is (for a 64-bit system):
//
//	  (2**64N - 1) + (2**64 - 1)*(2**64N - 1) + (2**64 - 1)
//	= (2**64)*(2**64N - 1) + (2**64 - 1)
//	= 2**64(N+1) - 1,
//
// which fits in N+1 words (the high order one being the new value of a).
//
// (For example, with 3 decimal words, 999 + 9*999 + 9 = 999*10 + 9 = 9999.)
//
// If we unroll the loop a bit, then we can chain the carries in two passes.
// Consider:
//
//	lo0, hi0 := m * mulsrc[i]
//	lo0, carry = bits.Add(lo0, a, 0)
//	lo0, carryAlt = bits.Add(lo0, addsrc[i], 0)
//	z[i] = lo0
//	a = hi + carry + carryAlt // cannot overflow
//
//	lo1, hi1 := m * mulsrc[i]
//	lo1, carry = bits.Add(lo1, a, 0)
//	lo1, carryAlt = bits.Add(lo1, addsrc[i], 0)
//	z[i] = lo1
//	a = hi + carry + carryAlt // cannot overflow
//
//	lo2, hi2 := m * mulsrc[i]
//	lo2, carry = bits.Add(lo2, a, 0)
//	lo2, carryAlt = bits.Add(lo2, addsrc[i], 0)
//	z[i] = lo2
//	a = hi + carry + carryAlt // cannot overflow
//
//	lo3, hi3 := m * mulsrc[i]
//	lo3, carry = bits.Add(lo3, a, 0)
//	lo3, carryAlt = bits.Add(lo3, addsrc[i], 0)
//	z[i] = lo3
//	a = hi + carry + carryAlt // cannot overflow
//
// There are three ways we can optimize this sequence.
//
// (1) Reordering, we can chain carries so that we can use one hardware carry flag
// but amortize the cost of saving and restoring it across multiple instructions:
//
//	// multiply
//	lo0, hi0 := m * mulsrc[i]
//	lo1, hi1 := m * mulsrc[i+1]
//	lo2, hi2 := m * mulsrc[i+2]
//	lo3, hi3 := m * mulsrc[i+3]
//
//	lo0, carry = bits.Add(lo0, a, 0)
//	lo1, carry = bits.Add(lo1, hi0, carry)
//	lo2, carry = bits.Add(lo2, hi1, carry)
//	lo3, carry = bits.Add(lo3, hi2, carry)
//	a = hi3 + carry // cannot overflow
//
//	// add
//	lo0, carryAlt = bits.Add(lo0, addsrc[i], 0)
//	lo1, carryAlt = bits.Add(lo1, addsrc[i+1], carryAlt)
//	lo2, carryAlt = bits.Add(lo2, addsrc[i+2], carryAlt)
//	lo3, carryAlt = bits.Add(lo3, addrsc[i+3], carryAlt)
//	a = a + carryAlt // cannot overflow
//
//	z[i] = lo0
//	z[i+1] = lo1
//	z[i+2] = lo2
//	z[i+3] = lo3
//
// addMul takes this approach, using the hardware carry flag
// first for carry and then for carryAlt.
//
// (2) addMulAlt assumes there are two hardware carry flags available.
// It dedicates one each to carry and carryAlt, so that a multi-block
// unrolling can keep the flags in hardware across all the blocks.
// So even if the block size is 1, the code can do:
//
//	// multiply and add
//	lo0, hi0 := m * mulsrc[i]
//	lo0, carry = bits.Add(lo0, a, 0)
//	lo0, carryAlt = bits.Add(lo0, addsrc[i], 0)
//	z[i] = lo0
//
//	lo1, hi1 := m * mulsrc[i+1]
//	lo1, carry = bits.Add(lo1, hi0, carry)
//	lo1, carryAlt = bits.Add(lo1, addsrc[i+1], carryAlt)
//	z[i+1] = lo1
//
//	lo2, hi2 := m * mulsrc[i+2]
//	lo2, carry = bits.Add(lo2, hi1, carry)
//	lo2, carryAlt = bits.Add(lo2, addsrc[i+2], carryAlt)
//	z[i+2] = lo2
//
//	lo3, hi3 := m * mulsrc[i+3]
//	lo3, carry = bits.Add(lo3, hi2, carry)
//	lo3, carryAlt = bits.Add(lo3, addrsc[i+3], carryAlt)
//	z[i+3] = lo2
//
//	a = hi3 + carry + carryAlt // cannot overflow
//
// (3) addMulVirtualCarry optimizes for systems with explicitly computed carry bits
// (loong64, mips, riscv64), cutting the number of actual instructions almost by half.
// Look again at the original word-at-a-time version:
//
//	lo1, hi1 := m * mulsrc[i]
//	lo1, carry = bits.Add(lo1, a, 0)
//	lo1, carryAlt = bits.Add(lo1, addsrc[i], 0)
//	z[i] = lo1
//	a = hi + carry + carryAlt // cannot overflow
//
// Although it uses four adds per word, those are cheap adds: the two bits.Add adds
// use two instructions each (ADD+SLTU) and the final + adds only use one ADD each,
// for a total of 6 instructions per word. In contrast, the middle stanzas in (2) use
// only two “adds” per word, but these are SetCarry|UseCarry adds, which compile to
// five instruction each, for a total of 10 instructions per word. So the word-at-a-time
// loop is actually better. And we can reorder things slightly to use only a single carry bit:
//
//	lo1, hi1 := m * mulsrc[i]
//	lo1, carry = bits.Add(lo1, a, 0)
//	a = hi + carry
//	lo1, carry = bits.Add(lo1, addsrc[i], 0)
//	a = a + carry
//	z[i] = lo1
internal static void addMul(ж<ΔFunc> Ꮡf, @string addsrc, @string mulsrc, nint mulIndex) {
    ref var f = ref Ꮡf.DerefOrNull();

    var a = f.Asm;
    Hint mh = HintNone;
    if ((~a).Arch == Arch386 && addsrc != ""u8) {
        mh = HintMemOK; // too few registers otherwise
    }
    ref var m = ref heap<ΔReg>(out var Ꮡm);
    m = f.ArgHint("m"u8, mh);
    ref var c = ref heap<ΔReg>(out var Ꮡc);
    c = f.Arg("a"u8);
    var n = f.Arg(zLenˢ);

    var p = Ꮡf.Pipe();
    if (addsrc != ""u8) {
        p.SetHint(addsrc, HintMemOK);
    }
    p.SetHint(mulsrc, HintMulSrc);
    var unroll = new nint[]{1, 4}.slice();
    var exprᴛ1 = (~a).Arch;
    if (exprᴛ1 == Arch386) {
        unroll = new nint[]{1}.slice(); // too few registers
    }
    else if (exprᴛ1 == ArchARM) {
        p.SetMaxColumns(2); // too few registers (but more than 386)
    }
    else if (exprᴛ1 == ArchARM64) {
        unroll = new nint[]{1, 8}.slice(); // 5% speedup on c4as16
    }

    // See the large comment above for an explanation of the code being generated.
    // This is optimization strategy 1.
    p.Start(n, unroll.ꓸꓸꓸ);
    var aʗ1 = a;
    var cʗ1 = c;
    var mʗ1 = m;
    var pʗ1 = p;
    p.Loop((slice<slice<ΔReg>> @in, slice<slice<ΔReg>> @out) => {
        aʗ1.Comment(multiplyˢ);
        var prev = cʗ1;
        ΔCarry flag = SetCarry;
        foreach (var (i, x) in @in[mulIndex]) {
            var hi = aʗ1.RegHint(HintMulHi);
            aʗ1.MulWide(mʗ1, x, x, hi);
            aʗ1.Add(prev, x, x, flag);
            flag = (ΔCarry)(UseCarry | SetCarry);
            if (prev != cʗ1) {
                aʗ1.Free(prev);
            }
            @out[0][i] = x;
            prev = hi;
        }
        aʗ1.Add(aʗ1.Imm(0), prev, cʗ1, (ΔCarry)(UseCarry | SmashCarry));
        if (addsrc != ""u8) {
            aʗ1.Comment(addˢ);
            ΔCarry flagΔ1 = SetCarry;
            foreach (var (i, x) in @in[0]) {
                aʗ1.Add(x, @out[0][i], @out[0][i], flagΔ1);
                flagΔ1 = (ΔCarry)(UseCarry | SetCarry);
            }
            aʗ1.Add(aʗ1.Imm(0), cʗ1, cʗ1, (ΔCarry)(UseCarry | SmashCarry));
        }
        pʗ1.StoreN(@out);
    });

    f.StoreArg(c, "c"u8);
    a.Ret();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string altˢ = "alt"u8;
internal static readonly @string multiplyAndAddˢ = "multiply and add"u8;

internal static void addMulAlt(ж<ΔFunc> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    var a = f.Asm;
    ref var m = ref heap<ΔReg>(out var Ꮡm);
    m = f.ArgHint("m"u8, HintMulSrc);
    ref var c = ref heap<ΔReg>(out var Ꮡc);
    c = f.Arg("a"u8);
    var n = f.Arg(zLenˢ);

    // On amd64, we need a non-immediate for the AtUnrollEnd adds.
    ref var r0 = ref heap<ΔReg>(out var Ꮡr0);
    r0 = a.ZR();
    if (!r0.Valid()) {
        r0 = a.Reg();
        a.Mov(a.Imm(0), r0);
    }

    var p = Ꮡf.Pipe();
    p.SetLabel(altˢ);
    p.SetHint("x"u8, HintMemOK);
    p.SetHint("y"u8, HintMemOK);
    if ((~a).Arch == ArchAMD64) {
        p.SetMaxColumns(2);
    }

    // See the large comment above for an explanation of the code being generated.
    // This is optimization strategy (2).
    ref var hi = ref heap(new ΔReg(), out var Ꮡhi);
    ref var prev = ref heap<ΔReg>(out var Ꮡprev);
    prev = c;
    p.Start(n, 1, 8);
    var aʗ1 = a;
    p.AtUnrollStart(() => {
        aʗ1.Comment(multiplyAndAddˢ);
        aʗ1.ClearCarry((ΔCarry)(AddCarry | ΔAltCarry));
        aʗ1.ClearCarry(AddCarry);
        Ꮡhi.Value = aʗ1.Reg();
    });
    var aʗ2 = a;
    var cʗ1 = c;
    var r0ʗ1 = r0;
    p.AtUnrollEnd(() => {
        aʗ2.Add(r0ʗ1, Ꮡprev.Value, cʗ1, (ΔCarry)(UseCarry | SmashCarry));
        aʗ2.Add(r0ʗ1, cʗ1, cʗ1, (ΔCarry)((ΔCarry)(UseCarry | SmashCarry) | ΔAltCarry));
        Ꮡprev.Value = cʗ1;
    });
    var aʗ3 = a;
    var mʗ1 = m;
    var pʗ1 = p;
    p.Loop((slice<slice<ΔReg>> @in, slice<slice<ΔReg>> @out) => {
        foreach (var (i, y) in @in[1]) {
            var x = @in[0][i];
            var lo = y;
            if (lo.IsMem()) {
                lo = aʗ3.Reg();
            }
            aʗ3.MulWide(mʗ1, y, lo, Ꮡhi.Value);
            aʗ3.Add(Ꮡprev.Value, lo, lo, (ΔCarry)(UseCarry | SetCarry));
            aʗ3.Add(x, lo, lo, (ΔCarry)((ΔCarry)(UseCarry | SetCarry) | ΔAltCarry));
            @out[0][i] = lo;
            (Ꮡprev.Value, Ꮡhi.Value) = (Ꮡhi.Value, Ꮡprev.Value);
        }
        pʗ1.StoreN(@out);
    });

    f.StoreArg(c, "c"u8);
    a.Ret();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string syntheticCarryOneColumnˢ = "synthetic carry, one column at a time"u8;

internal static void addMulVirtualCarry(ж<ΔFunc> Ꮡf, nint mulIndex) {
    ref var f = ref Ꮡf.DerefOrNull();

    var a = f.Asm;
    ref var m = ref heap<ΔReg>(out var Ꮡm);
    m = f.Arg("m"u8);
    ref var c = ref heap<ΔReg>(out var Ꮡc);
    c = f.Arg("a"u8);
    var n = f.Arg(zLenˢ);

    // See the large comment above for an explanation of the code being generated.
    // This is optimization strategy (3).
    var p = Ꮡf.Pipe();
    p.Start(n, 1, 4);
    var aʗ1 = a;
    var cʗ1 = c;
    var mʗ1 = m;
    var pʗ1 = p;
    p.Loop((slice<slice<ΔReg>> @in, slice<slice<ΔReg>> @out) => {
        aʗ1.Comment(syntheticCarryOneColumnˢ);
        var (lo, hi) = (aʗ1.Reg(), aʗ1.Reg());
        foreach (var (i, x) in @in[mulIndex]) {
            aʗ1.MulWide(mʗ1, x, lo, hi);
            if (mulIndex == 1) {
                aʗ1.Add(@in[0][i], lo, lo, SetCarry);
                aʗ1.Add(aʗ1.Imm(0), hi, hi, (ΔCarry)(UseCarry | SmashCarry));
            }
            aʗ1.Add(cʗ1, lo, x, SetCarry);
            aʗ1.Add(aʗ1.Imm(0), hi, cʗ1, (ΔCarry)(UseCarry | SmashCarry));
            @out[0][i] = x;
        }
        pʗ1.StoreN(@out);
    });
    f.StoreArg(c, "c"u8);
    a.Ret();
}

} // end asmgen_package
