namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct inner {
    internal uint16 a;
    internal uint16 b;
}

partial struct outer {
    internal @string name;
    internal inner @in;
    internal ж<inner> ptr;
}

internal static void bumpA(this ж<outer> Ꮡo) {
    var p = Ꮡo.of(outer.Ꮡin).of(inner.Ꮡa);
    p.Value++;
}

internal static void bumpSelected(this ж<outer> Ꮡo, nint which) {
    ж<uint16> count = default!;
    switch (which) {
    case 0: {
        count = Ꮡo.of(outer.Ꮡin).of(inner.Ꮡa);
        break;
    }
    case 1: {
        count = Ꮡo.of(outer.Ꮡin).of(inner.Ꮡb);
        break;
    }}

    count.Value += 10;
}

internal static void bumpViaPtr(this ref outer o) {
    var p = o.ptr.of(inner.Ꮡb);
    p.Value += 3;
}

partial struct leaf {
    internal nint n;
}

partial struct mid {
    internal leaf lf;
}

partial struct deep {
    internal mid md;
}

internal static void bumpLeaf(this ж<deep> Ꮡd) {
    var p = Ꮡd.of(deep.Ꮡmd).of(mid.Ꮡlf).of(leaf.Ꮡn);
    p.Value += 5;
}

internal static uint16 readA(this ж<outer> Ꮡo) {
    var p = Ꮡo.of(outer.Ꮡin).of(inner.Ꮡa);
    return p.Value;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object oneHopˢ = (@string)"one hop:"u8;
private static readonly object selectedˢ = (@string)"selected:"u8;
private static readonly object throughPointerHopˢ = (@string)"through pointer hop:"u8;
private static readonly object valueFieldUntouchedˢ = (@string)"value field untouched:"u8;
private static readonly object twoHopsˢ = (@string)"two hops:"u8;
private static readonly object readBackˢ = (@string)"read back:"u8;
private static readonly object nameIntactˢ = (@string)"name intact:"u8;

internal static void Main() {
    var o = Ꮡ(new outer(name: "x"u8, ptr: Ꮡ(new inner(nil))));
    o.bumpA();
    o.bumpA();
    fmt.Println(oneHopˢ, (~o).@in.a);
    o.bumpSelected(0);
    o.bumpSelected(1);
    fmt.Println(selectedˢ, (~o).@in.a, (~o).@in.b);
    o.bumpViaPtr();
    fmt.Println(throughPointerHopˢ, (~(~o).ptr).b, valueFieldUntouchedˢ, (~o).@in.b);
    var d = Ꮡ(new deep(nil));
    d.bumpLeaf();
    d.bumpLeaf();
    fmt.Println(twoHopsˢ, (~d).md.lf.n);
    fmt.Println(readBackˢ, o.readA());
    fmt.Println(nameIntactˢ, (~o).name);
}

} // end main_package
