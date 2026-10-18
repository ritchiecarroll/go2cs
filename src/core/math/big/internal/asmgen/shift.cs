// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ret0ˢ = "ret0"u8;
internal static readonly @string shiftFirstWordIntoCarryˢ = "shift first word into carry"u8;
internal static readonly @string shiftRemainingWordsˢ = "shift remaining words"u8;
internal static readonly @string storeFinalShiftedBitsˢ = "store final shifted bits"u8;

// shiftVU generates lshVU and rshVU, which do
// z, c = x << s and z, c = x >> s, for 0 < s < _W.
internal static void shiftVU(ж<Asm> Ꮡa, @string name) {
    ref var a = ref Ꮡa.DerefOrNull();

    // Because these routines can be called for z.Lsh(z, N) and z.Rsh(z, N),
    // the input and output slices may be aliased at different offsets.
    // For example (on 64-bit systems), during z.Lsh(z, 65), &z[0] == &x[1],
    // and during z.Rsh(z, 65), &z[1] == &x[0].
    // For left shift, we must process the slices from len(z)-1 down to 0,
    // so that we don't overwrite a word before we need to read it.
    // For right shift, we must process the slices from 0 up to len(z)-1.
    // The different traversals at least make the two cases more consistent,
    // since we're always delaying the output by one word compared
    // to the input.
    var f = Ꮡa.Func("func "u8 + name + "(z, x []Word, s uint) (c Word)"u8);

    // Check for no input early, since we need to start by reading 1 word.
    var n = f.Arg(zLenˢ);
    Ꮡa.JmpZero(n, ret0ˢ);

    // Start loop by reading first input word.
    ref var s = ref heap<ΔReg>(out var Ꮡs);
    s = f.ArgHint("s"u8, HintShiftCount);
    var p = f.Pipe();
    if (name == "lshVU"u8) {
        p.SetBackward();
    }
    var unroll = new nint[]{1, 4}.slice();
    if (a.Arch == Arch386) {
        unroll = new nint[]{1}.slice(); // too few registers for more
        p.SetUseIndexCounter();
    }
    p.LoadPtrs(n);
    Ꮡa.Comment(shiftFirstWordIntoCarryˢ);
    ref var prev = ref heap<ΔReg>(out var Ꮡprev);
    prev = p.LoadN(1)[0][0];

    // Decide how to shift. On systems with a wide shift (x86), use that.
    // Otherwise, we need shift by s and negative (reverse) shift by 64-s or 32-s.
    var shift = (ΔReg p1, ΔReg p2, ΔReg p3) => Ꮡa.Lsh(p1, p2, p3);
    var shiftWide = (ΔReg p1, ΔReg p2, ΔReg p3, ΔReg p4) => Ꮡa.LshWide(p1, p2, p3, p4);
    var negShift = (ΔReg p1, ΔReg p2, ΔReg p3) => Ꮡa.Rsh(p1, p2, p3);
    var negShiftReg = (ΔReg p1, ΔReg p2) => Ꮡa.RshReg(p1, p2);
    if (name == "rshVU"u8) {
        shift = (ΔReg p1, ΔReg p2, ΔReg p3) => Ꮡa.Rsh(p1, p2, p3);
        shiftWide = (ΔReg p1, ΔReg p2, ΔReg p3, ΔReg p4) => Ꮡa.RshWide(p1, p2, p3, p4);
        negShift = (ΔReg p1, ΔReg p2, ΔReg p3) => Ꮡa.Lsh(p1, p2, p3);
        negShiftReg = (ΔReg p1, ΔReg p2) => Ꮡa.LshReg(p1, p2);
    }
    if (a.Arch.HasShiftWide()){
        // Use wide shift to avoid needing negative shifts.
        // The invariant is that prev holds the previous word (not shifted at all),
        // to be used as input into the wide shift.
        // After the loop finishes, prev holds the final output word to be written.
        var c = Ꮡa.Reg();
        shiftWide(s, prev, a.Imm(0), c);
        f.StoreArg(c, "c"u8);
        Ꮡa.Free(c);
        Ꮡa.Comment(shiftRemainingWordsˢ);
        p.Start(n, unroll.ꓸꓸꓸ);
        var pʗ1 = p;
        var prevʗ1 = prev;
        var sʗ1 = s;
        var shiftWideʗ1 = shiftWide;
        p.Loop((slice<slice<ΔReg>> @in, slice<slice<ΔReg>> @out) => {
            // We reuse the input registers as output, delayed one cycle; prev is the first output.
            // After writing the outputs to memory, we can copy the final x value into prev
            // for the next iteration.
            var old = prevʗ1;
            foreach (var (i, x) in @in[0]) {
                shiftWideʗ1(sʗ1, x, old, old);
                @out[0][i] = old;
                old = x;
            }
            pʗ1.StoreN(@out);
            Ꮡa.Mov(old, prevʗ1);
        });
        Ꮡa.Comment(storeFinalShiftedBitsˢ);
        shift(s, prev, prev);
    } else {
        // Construct values from x << s and x >> (64-s).
        // After the first word has been processed, the invariant is that
        // prev holds x << s, to be used as the high bits of the next output word,
        // once we find the low bits after reading the next input word.
        // After the loop finishes, prev holds the final output word to be written.
        ref var sNeg = ref heap<ΔReg>(out var ᏑsNeg);
        sNeg = Ꮡa.Reg();
        Ꮡa.Mov(a.Imm((~a.Arch).WordBits), sNeg);
        Ꮡa.Sub(s, sNeg, sNeg, SmashCarry);
        var c = Ꮡa.Reg();
        negShift(sNeg, prev, c);
        shift(s, prev, prev);
        f.StoreArg(c, "c"u8);
        Ꮡa.Free(c);
        Ꮡa.Comment(shiftRemainingWordsˢ);
        p.Start(n, unroll.ꓸꓸꓸ);
        var negShiftʗ1 = negShift;
        var negShiftRegʗ1 = negShiftReg;
        var pʗ2 = p;
        var prevʗ2 = prev;
        var sʗ2 = s;
        var sNegʗ1 = sNeg;
        var shiftʗ1 = shift;
        p.Loop((slice<slice<ΔReg>> @in, slice<slice<ΔReg>> @out) => {
            if (Ꮡa.Value.HasRegShift()) {
                // ARM (32-bit) allows shifts in most arithmetic expressions,
                // including OR, letting us combine the negShift and a.Or.
                // The simplest way to manage the registers is to do StoreN for
                // one output at a time, and since we don't use multi-register
                // stores on ARM, that doesn't hurt us.
                @out[0] = @out[0][..1];
                foreach (var (_, x) in @in[0]) {
                    Ꮡa.Or(negShiftRegʗ1(sNegʗ1, x), prevʗ2, prevʗ2);
                    @out[0][0] = prevʗ2;
                    pʗ2.StoreN(@out);
                    shiftʗ1(sʗ2, x, prevʗ2);
                }
                return;
            }
            // We reuse the input registers as output, delayed one cycle; z0 is the first output.
            var z0 = Ꮡa.Reg();
            var z = z0;
            foreach (var (i, x) in @in[0]) {
                negShiftʗ1(sNegʗ1, x, z);
                Ꮡa.Or(prevʗ2, z, z);
                shiftʗ1(sʗ2, x, prevʗ2);
                @out[0][i] = z;
                z = x;
            }
            pʗ2.StoreN(@out);
        });
        Ꮡa.Comment(storeFinalShiftedBitsˢ);
    }
    p.StoreN(new slice<ΔReg>[]{new ΔReg[]{prev}.slice()}.slice());
    p.Done();
    Ꮡa.Free(s);
    Ꮡa.Ret();

    // Return 0, used from above.
    Ꮡa.Label(ret0ˢ);
    f.StoreArg(a.Imm(0), "c"u8);
    Ꮡa.Ret();
}

} // end asmgen_package
