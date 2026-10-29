// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.big.@internal;

partial class asmgen_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zLenˢ = "z_len"u8;
internal static readonly @string clearSavedCarryˢ = "clear saved carry"u8;

// addOrSubVV generates addVV or subVV,
// which do z, c = x ± y.
// The caller guarantees that len(z) == len(x) == len(y).
internal static void addOrSubVV(ж<Asm> Ꮡa, @string name) {
    ref var a = ref Ꮡa.DerefOrNull();

    var f = Ꮡa.Func("func "u8 + name + "(z, x, y []Word) (c Word)"u8);

    var add = (ΔReg p1, ΔReg p2, ΔReg p3, ΔCarry p4) => Ꮡa.Add(p1, p2, p3, p4);
    ΔCarry which = AddCarry;
    if (name == "subVV"u8) {
        add = (ΔReg p1, ΔReg p2, ΔReg p3, ΔCarry p4) => Ꮡa.Sub(p1, p2, p3, p4);
        which = SubCarry;
    }

    var n = f.Arg(zLenˢ);
    var p = f.Pipe();
    p.SetHint("y"u8, HintMemOK); // allow y to be used from memory on x86
    p.Start(n, 1, 4);
    ref var c = ref heap(new ΔReg(), out var Ꮡc);
    if (!a.Arch.CarrySafeLoop) {
        // Carry smashed by loop tests; allocate and save in register
        // around unrolled blocks.
        c = Ꮡa.Reg();
        Ꮡa.Mov(a.Imm(0), c);
        Ꮡa.EOL(clearSavedCarryˢ);
        p.AtUnrollStart(() => {
            Ꮡa.RestoreCarry(Ꮡc.Value);
            Ꮡa.Free(Ꮡc.Value);
        });
        p.AtUnrollEnd(() => {
            Ꮡa.Unfree(Ꮡc.Value);
            Ꮡa.SaveCarry(Ꮡc.Value);
        });
    } else {
        // Carry preserved by loop; clear now, ahead of loop
        // (but after Start, which may have modified it).
        Ꮡa.ClearCarry(which);
    }
    var addʗ1 = add;
    var pʗ1 = p;
    p.Loop((slice<slice<ΔReg>> @in, slice<slice<ΔReg>> @out) => {
        foreach (var (i, x) in @in[0]) {
            var y = @in[1][i];
            addʗ1(y, x, x, (ΔCarry)(SetCarry | UseCarry));
        }
        pʗ1.StoreN(@in[..1]);
    });
    p.Done();

    // Copy carry to output.
    if (c.Valid()) {
        Ꮡa.ConvertCarry(which, c);
    } else {
        c = Ꮡa.RegHint(HintCarry);
        Ꮡa.SaveConvertCarry(which, c);
    }
    f.StoreArg(c, "c"u8);
    Ꮡa.Free(c);
    Ꮡa.Ret();
}

} // end asmgen_package
