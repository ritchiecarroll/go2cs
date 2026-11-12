namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct pair {
    internal int32 a, b;
}

internal static int32 mutate(ref pair p) {
    p.a = 100;
    return 2;
}

internal static int32 scaled(pair p, int32 k) { return (p.a * 1000 + p.b) * k; }
internal static int32 readOnly(ref pair p) { return p.a + p.b; }

internal static int32 writeB(ref pair p) {
    p.b = 9;
    return 1;
}

partial struct counter {
    internal nint n;
}

internal static nint bump(this ref counter c) {
    c.n += 10;
    return 1;
}

internal static nint peek(this ref counter c) { return c.n; }

partial struct holder {
    internal ж<pair> p;
}

internal static int32 repoint(ref ж<pair> pp) {
    pp = Ꮡ(new pair(50, 0));
    return 1;
}

internal static int32 touch(this ref holder h) {
    h.p.a = 5;
    return 1;
}

partial struct small {
    internal int8 f;
    internal nint g;
}

internal static nint poke(this ref small s) {
    (s.f, s.g) = (9, 9);
    return 1;
}

partial struct key {
    internal nint a;
    internal @string s;
}

internal static nint sum3(nint a, nint b, nint c) { return a * 100 + b * 10 + c; }

internal static nint fill(slice<nint> s) {
    s[0] = 9;
    return 1;
}

internal static nint other() { return 1; }

internal static void main() {
    var p = new pair(1, 2);
    var ᴛ1 = mutate(ref p);
    fmt.Println("args-addr", scaled(p, ᴛ1));

    var n = (int32)1;
    int32 bump() {
        n = 50;
        return 3;
    }
    var ᴛ2 = bump();
    fmt.Println("binary-closure", n + ᴛ2);

    var t = new counter(n: 1);
    var ᴛ3 = t.bump();
    fmt.Println("args-method", sum3(t.n, ᴛ3, t.n));

    nint k = 1;
    nint setK() {
        k = 7;
        return 1;
    }
    var ᴛ4 = setK();
    var s = slice<nint>([k, ᴛ4, k]);
    fmt.Println("complit", s[0], s[1], s[2]);

    ref var ys = ref heap<slice<nint>>(slice<nint>([0, 0]), out var Ꮡys);
    nint swap() {
        Ꮡys.ValueSlot = [5, 5];
        return 4;
    }
    var ᴛ5 = swap();
    ys[1] = ᴛ5;
    fmt.Println("lhs-index", ys[0], ys[1]);

    nint c = 1;
    nint setC() {
        c = 7;
        return 1;
    }
    var ᴛ6 = setC();
    c += ᴛ6;
    fmt.Println("compound", c);

    nint m = 1;
    nint setM() {
        m = 7;
        return 1;
    }
    var ᴛ7 = setM();
    nint a = m;
    nint b = ᴛ7;
    fmt.Println("multi-assign", a, b);

    var ch = new channel<nint>(1);
    nint q = 1;
    nint setQ() {
        q = 7;
        return 1;
    }
    var ᴛ8 = setQ();
    ch.ᐸꟷ(q + ᴛ8);
    fmt.Println("send", ᐸꟷ(ch));

    nint r = 1;
    nint setR() {
        r = 7;
        return 1;
    }
    var ᴛ9 = setR();
    fmt.Println("conversion", (int64)r + (int64)ᴛ9);

    fmt.Println("return1", ret1());

    @string str = "a";
    @string cat() {
        str = "Z";
        return "b";
    }
    var ᴛ10 = cat();
    fmt.Println("concat", str + ᴛ10);

    var pt = Ꮡ(new counter(n: 1));
    var ᴛ11 = pt.bump();
    fmt.Println("ptr-recv", pt.n + ᴛ11);

    var sl = slice<nint>([1, 2]);
    var ᴛ12 = fill(sl);
    fmt.Println("slice-arg", sl[0] + ᴛ12);

    ref var rp = ref heap<ж<pair>>(Ꮡ(new pair(1, 2)), out var Ꮡrp);
    var ᴛ13 = repoint(ref rp);
    fmt.Println("repoint", rp.a + ᴛ13);

    var p2 = new pair(1, 2);
    fmt.Println("neg-readonly", scaled(p2, readOnly(ref p2)));
    var p3 = new pair(1, 2);
    fmt.Println("neg-otherfield", p3.a + writeB(ref p3));
    var t2 = new counter(n: 1);
    fmt.Println("neg-peek", t2.n + t2.peek());
    nint x = 1;
    fmt.Println("neg-after", other() + x);
    var h = Ꮡ(new holder(p: Ꮡ(new pair(1, 2))));
    fmt.Println("neg-pointee", h.p == nil, h.touch());

    var se = new small(1, 1);
    fmt.Println("inplace-iface-expr", (int8)(se.f + 1), se.poke());
    var sk = new small(1, 1);
    var mk = new map<key, nint>{[new(1, "a")] = 10, [new(9, "a")] = 90};
    fmt.Println("inplace-mapkey-lit", mk[new key(sk.g, "a")] + sk.poke());
    ref var sc = ref heap<small>(new small(1, 1), out var Ꮡsc);
    nint setSc() {
        Ꮡsc.Value.f = 9;
        return 1;
    }
    fmt.Println("inplace-iface-closure", sc, setSc());
    ref var kc = ref heap<key>(new key(1, "a"), out var Ꮡkc);
    nint setKc() {
        Ꮡkc.Value.a = 9;
        return 1;
    }
    fmt.Println("inplace-mapkey-closure", mk[kc] + setKc());

    var sa = new small(1, 1);
    var ᴛ14 = sa.poke();
    fmt.Println("inplace-neg-addr", sa, ᴛ14);
    var sw = new small(1, 1);
    var ᴛ15 = sw.poke();
    fmt.Println("inplace-neg-word", sw.g + 1, ᴛ15);
}

internal static nint ret1() {
    nint v = 1;
    nint setV() {
        v = 7;
        return 1;
    }
    var ᴛ16 = setV();
    return v + ᴛ16;
}

} // end main_package
