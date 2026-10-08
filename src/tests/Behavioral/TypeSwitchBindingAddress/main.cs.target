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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string resetˢ = "reset"u8;

internal static void reset(this ref resolver r, ж<end> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    e.n.space = resetˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string heldˢ = "held"u8;

internal static any process(any tok, ж<resolver> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    switch (tok.type()) {
    case start t1ᴛ1: {
        ref var t1 = ref heap(t1ᴛ1, out var Ꮡt1);
        r.fix(Ꮡt1.of(start.Ꮡn));
        foreach (var (i, _) in t1.attr) {
            r.fix(Ꮡ(t1.attr, i));
        }
        return t1;
    }
    case end t1ᴛ2: {
        ref var t1 = ref heap(t1ᴛ2, out var Ꮡt1);
        var p = Ꮡt1.of(end.Ꮡn);
        p.Value.space = heldˢ;
        r.reset(Ꮡt1);
        return t1;
    }}
    return tok;
}

internal static void Main() {
    var r = Ꮡ(new resolver(ns: new map<@string, @string>{["a"u8] = "urn:a"u8}));
    var s = process(new start(n: new name(space: "a"u8, local: "x"u8), attr: new name[]{new(space: "a"u8, local: "y"u8)}.slice()), r)._<start>();
    fmt.Println(s.n.space, s.n.local);
    fmt.Println(s.attr[0].space, s.attr[0].local);
    var e = process(new end(n: new name(space: "z"u8, local: "w"u8)), r)._<end>();
    fmt.Println(e.n.space, e.n.local);
}

} // end main_package
