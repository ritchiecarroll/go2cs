namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct header {
    public uint32 Out;
    public uint32 Arg;
}

[GoType] public partial struct inst {
    internal partial ref header header { get; }
    public @string Name;
}

[GoType] partial struct prog {
    public slice<inst> Inst;
}

[GoType] partial struct cell {
    internal nint n, m;
}

internal static partial void setU32(ref uint32 p, uint32 v) {
    p = v;
}

internal static partial void setInt(ref nint p, nint v) {
    p = v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object localSliceˢ = (@string)"local slice:"u8;
private static readonly object localArrayˢ = (@string)"local array:"u8;
private static readonly object promotedˢ = (@string)"promoted:"u8;
private static readonly object swappedˢ = (@string)"swapped:"u8;
private static readonly object patchedˢ = (@string)"patched:"u8;
private static readonly @string patchedˢ2 = "patched"u8;
private static readonly object nameˢ = (@string)"name:"u8;

internal static void Main() {
    var cells = new slice<cell>(3);
    var p = Ꮡ(cells, 1).of(cell.Ꮡn);
    p.Value = 7;
    setInt(ref cells[2].m, 9);
    fmt.Println(localSliceˢ, cells[1].n, cells[2].m);
    ref var arr = ref heap(new array<cell>(3), out var Ꮡarr);
    var q = Ꮡarr.at<cell>(2).of(cell.Ꮡn);
    q.Value = 11;
    setInt(ref arr[0].m, 13);
    fmt.Println(localArrayˢ, arr[2].n, arr[0].m);
    var pr = Ꮡ(new prog(Inst: new slice<inst>(4, () => new(nil))));
    foreach (var (i, _) in (~pr).Inst) {
        (~pr).Inst[i].Name = fmt.Sprintf("i%d"u8, i);
    }
    var a = Ꮡ((~pr).Inst, 0).of(inst.ᏑOut);
    var b = Ꮡ((~pr).Inst, 0).of(inst.ᏑArg);
    a.Value = 100;
    b.Value = 200;
    setU32(ref (Ꮡ((~pr).Inst, 1).of(inst.ᏑOut)).DerefOrNull(), 300);
    fmt.Println(promotedˢ, (~pr).Inst[0].Out, (~pr).Inst[0].Arg, (~pr).Inst[1].Out);
    var x = Ꮡ((~pr).Inst, 2).of(inst.ᏑOut);
    var y = Ꮡ((~pr).Inst, 2).of(inst.ᏑArg);
    (x, y) = (y, x);
    x.Value = 41;
    y.Value = 42;
    fmt.Println(swappedˢ, (~pr).Inst[2].Out, (~pr).Inst[2].Arg);
    (~pr).Inst[3].Out = 55;
    var src = Ꮡ((~pr).Inst, 3).of(inst.ᏑOut);
    var dst = Ꮡ((~pr).Inst, 1).of(inst.ᏑArg);
    dst.Value = src.Value;
    fmt.Println(patchedˢ, (~pr).Inst[1].Arg, (~pr).Inst[3].Out);
    var n = Ꮡ((~pr).Inst, 3).of(inst.ᏑName);
    n.Value = patchedˢ2;
    fmt.Println(nameˢ, (~pr).Inst[3].Name);
}

} // end main_package
