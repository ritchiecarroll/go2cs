namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct name {
    internal @string space;
    internal @string local;
}

partial struct start {
    internal name n;
    internal slice<name> attr;
}

partial struct end {
    internal name n;
}

partial struct resolver {
    internal map<@string, @string> ns;
}

internal static void fix(this ref resolver r, ж<name> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    {
        var (v, ok) = r.ns[n.space, ꟷ]; if (ok) {
            n.space = v;
        }
    }
}

internal static void reset(this ref resolver r, ж<end> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    e.n.space = "reset";
}

internal static any process(any tok, ж<resolver> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    switch (tok.type()) {
    case start t1ᴛ1: {
        ref var t1 = ref heap(t1ᴛ1, out var Ꮡt1);
        r.fix(Ꮡt1.Ꮡn);
        foreach (var i in range(t1.attr)) {
            r.fix(Ꮡ(t1.attr, i));
        }
        return t1;
    }
    case end t1ᴛ2: {
        ref var t1 = ref heap(t1ᴛ2, out var Ꮡt1);
        var p = Ꮡt1.Ꮡn;
        p.space = "held";
        r.reset(Ꮡt1);
        return t1;
    }}
    return tok;
}

internal static void Main() {
    var r = Ꮡ(new resolver(ns: new map<@string, @string>{["a"] = "urn:a"}));

    var s = process(new start(n: new name(space: "a", local: "x"), attr: slice<name>([new(space: "a", local: "y")])), r)._<start>();
    fmt.Println(s.n.space, s.n.local);
    fmt.Println(s.attr[0].space, s.attr[0].local);

    var e = process(new end(n: new name(space: "z", local: "w")), r)._<end>();
    fmt.Println(e.n.space, e.n.local);
}

} // end main_package
