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
    ref var x = ref heap<Thing>(out var Ꮡx);
    x = new Thing(val: 7);
    var p = Ꮡx.of(Thing.Ꮡval);
    p.Value = 99;
    return x.val;
}

internal static nint nestedChain() {
    ref var w = ref heap<Wrap>(out var Ꮡw);
    w = new Wrap(inner: new Thing(val: 5));
    var p = Ꮡw.of(Wrap.Ꮡinner).of(Thing.Ꮡval);
    p.Value = 42;
    return w.inner.val;
}

internal static nint pointerHop() {
    ref var t = ref heap<Thing>(out var Ꮡt);
    t = new Thing(val: 3);
    var w = new Wrap(ptr: Ꮡt);
    var p = w.ptr.of(Thing.Ꮡval);
    p.Value = 11;
    return t.val;
}

internal static nint methodBody(this ref Thing t) {
    ref var y = ref heap<Thing>(out var Ꮡy);
    y = new Thing(val: 1);
    var p = Ꮡy.of(Thing.Ꮡval);
    p.Value = 88;
    return y.val;
}

partial struct Embed {
    internal nint ev;
}

partial struct Outer {
    public partial ref Embed Embed { get; }
    internal nint other;
}

internal static nint promotedValueEmbed() {
    ref var o = ref heap<Outer>(out var Ꮡo);
    o = new Outer(nil);
    var p = Ꮡo.of(Outer.Ꮡev);
    p.Value = 33;
    return o.ev;
}

partial struct PtrHolder {
    internal ж<nint> p;
}

internal static nint compositePos() {
    ref var x = ref heap<Thing>(out var Ꮡx);
    x = new Thing(val: 2);
    var h = new PtrHolder(p: Ꮡx.of(Thing.Ꮡval));
    h.p.Value = 55;
    return x.val;
}

internal static ж<nint> returnPos() {
    ref var x = ref heap<Thing>(out var Ꮡx);
    x = new Thing(val: 4);
    return Ꮡx.of(Thing.Ꮡval);
}

internal static void Main() {
    fmt.Println(plainFunc());
    fmt.Println(nestedChain());
    fmt.Println(pointerHop());
    var t = new Thing(nil);
    fmt.Println(t.methodBody());
    fmt.Println(promotedValueEmbed());
    fmt.Println(compositePos());
    var rp = returnPos();
    rp.Value = 66;
    fmt.Println(rp.Value);
}

} // end main_package
