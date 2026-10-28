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
    ж<uint16> count = default;
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

internal static void Main() {
    var o = Ꮡ(new outer(name: "x", ptr: Ꮡ(new inner(nil))));

    o.bumpA();
    o.bumpA();
    fmt.Println("one hop:", (~o).@in.a);

    o.bumpSelected(0);
    o.bumpSelected(1);
    fmt.Println("selected:", (~o).@in.a, (~o).@in.b);

    o.bumpViaPtr();
    fmt.Println("through pointer hop:", (~(~o).ptr).b, "value field untouched:", (~o).@in.b);

    var d = Ꮡ(new deep(nil));
    d.bumpLeaf();
    d.bumpLeaf();
    fmt.Println("two hops:", (~d).md.lf.n);

    fmt.Println("read back:", o.readA());
    fmt.Println("name intact:", (~o).name);
}

} // end main_package
