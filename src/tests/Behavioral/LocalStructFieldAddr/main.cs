namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Thing {
    internal nint val;
}

partial struct Wrap {
    internal Thing inner;
    internal ж<Thing> ptr;
}

internal static nint plainFunc() {
    ref var x = ref heap<Thing>(new Thing(val: 7), out var Ꮡx);
    var p = Ꮡx.Ꮡval;
    p.Value = 99;
    return x.val;
}

internal static nint nestedChain() {
    ref var w = ref heap<Wrap>(new Wrap(inner: new Thing(val: 5)), out var Ꮡw);
    var p = Ꮡw.Ꮡinner.Ꮡval;
    p.Value = 42;
    return w.inner.val;
}

internal static nint pointerHop() {
    ref var t = ref heap<Thing>(new Thing(val: 3), out var Ꮡt);
    var w = new Wrap(ptr: Ꮡt);
    var p = w.ptr.Ꮡval;
    p.Value = 11;
    return t.val;
}

internal static nint methodBody(this ref Thing t) {
    ref var y = ref heap<Thing>(new Thing(val: 1), out var Ꮡy);
    var p = Ꮡy.Ꮡval;
    p.Value = 88;
    return y.val;
}

partial struct Embed {
    internal nint ev;
}

partial struct Outer {
    /*embed*/ public Embed Embed;
    internal nint other;
}

internal static nint promotedValueEmbed() {
    ref var o = ref heap<Outer>(new Outer(), out var Ꮡo);
    var p = Ꮡo.of(Outer.Ꮡev);
    p.Value = 33;
    return o.ev;
}

partial struct PtrHolder {
    internal ж<nint> p;
}

internal static nint compositePos() {
    ref var x = ref heap<Thing>(new Thing(val: 2), out var Ꮡx);
    var h = new PtrHolder(p: Ꮡx.Ꮡval);
    h.p.Value = 55;
    return x.val;
}

internal static ж<nint> returnPos() {
    ref var x = ref heap<Thing>(new Thing(val: 4), out var Ꮡx);
    return Ꮡx.Ꮡval;
}

internal static void Main() {
    fmt.Println(plainFunc());
    fmt.Println(nestedChain());
    fmt.Println(pointerHop());
    var t = new Thing();
    fmt.Println(t.methodBody());
    fmt.Println(promotedValueEmbed());
    fmt.Println(compositePos());
    var rp = returnPos();
    rp.Value = 66;
    fmt.Println(rp.Value);
}

} // end main_package
