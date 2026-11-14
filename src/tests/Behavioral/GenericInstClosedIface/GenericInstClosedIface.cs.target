namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct point {
    internal nint x, y;
}

partial struct box<T> {
    internal T v;
}

internal static T val<T>(this box<T> b) { return b.v; }

partial interface valuer {
    point val();
}

partial struct cell<T> {
    internal T v;
}

internal static T get<T>(this ref cell<T> c) { return c.v; }

partial interface getter {
    point get();
}

partial struct sink<T> {
    internal nint n;
}

internal static nint take<T>(this sink<T> s, T v) { return s.n + 1; }

partial interface taker {
    nint take(point v);
}

partial struct bag<T> {
    internal slice<T> items;
}

internal static nint size<T>(this bag<T> b) { return len(b.items); }

partial interface sizer {
    nint size();
}

partial struct outer {
    /*embed*/ internal box<point> box;
    internal nint n;
}

partial struct holder {
    internal valuer v;
    internal nint n;
}

internal static void show(valuer v) {
    var p = v.val();
    fmt.Println("argument", p.x, p.y);
}

internal static valuer build(nint x, nint y) {
    return new boxᴠvaluer(new box<point>(new point(x, y)));
}

internal static void main() {
    valuer v = default;
    v = new boxᴠvaluer(new box<point>(new point(1, 2)));
    fmt.Println("assignment", v.val().x, v.val().y);

    valuer w = new boxᴠvaluer(new box<point>(new point(3, 4)));
    fmt.Println("var", w.val().x, w.val().y);

    show(new boxᴠvaluer(new box<point>(new point(5, 6))));

    var r = build(7, 8);
    fmt.Println("return", r.val().x, r.val().y);

    var s = slice<valuer>([new boxᴠvaluer(new box<point>(new point(9, 10))), new boxᴠvaluer(new box<point>(new point(11, 12)))]);
    fmt.Println("element", s[0].val().x, s[1].val().y);

    var h = new holder(v: new boxᴠvaluer(new box<point>(new point(13, 14))), n: 1);
    fmt.Println("field", h.v.val().x, h.n);

    var m = new map<@string, valuer>{["a"] = new boxᴠvaluer(new box<point>(new point(15, 16)))};
    fmt.Println("map value", m["a"].val().y);

    var ch = new channel<valuer>(1);
    ch.ᐸꟷ(new boxᴠvaluer(new box<point>(new point(17, 18))));
    fmt.Println("send", (ᐸꟷ(ch)).val().x);

    var b = new box<point>(new point(1, 2));
    fmt.Println("equal to value", AreEqual(v, b), AreEqual(w, b));
    fmt.Println("equal interfaces", AreEqual(v, ((valuer)new boxᴠvaluer(b))), AreEqual(v, w));

    var (back, ok) = v._<box<point>>(ᐧ);
    fmt.Println("assert back", back.v.x, back.v.y, ok);

    switch (w.type()) {
    case box<point> tΔ1: {
        fmt.Println("type switch", tΔ1.v.x, tΔ1.v.y);
        break;
    }
    default: {
        var tΔ1 = w;
        fmt.Println("type switch missed");
        break;
    }}

    fmt.Printf("verbs %v %T\n"u8, v, v);

    any a = new box<point>(new point(1, 2));
    (var av, ok) = a._<valuer>(ᐧ);
    fmt.Println("any to valuer", ok, av.val().x, av.val().y);
    fmt.Println("asserted equals converted", AreEqual(av, v));
    var keys = new map<valuer, @string>{[v] = "converted"};
    keys[av] = "asserted";
    fmt.Println("map key", len(keys), keys[v]);
    fmt.Printf("asserted verbs %v %v\n"u8, av, v);

    any n = new box<nint>(7);
    (_, ok) = n._<valuer>(ᐧ);
    fmt.Println("box[int] is valuer", ok);

    var c = Ꮡ(new cell<point>(new point(21, 22)));
    getter g = new cellжgetter(c);
    fmt.Println("pointer", g.get().x, g.get().y);
    c.v.x = 23;
    fmt.Println("pointer shares", g.get().x);
    (var gc, ok) = g._<ж<cell<point>>>(ᐧ);
    fmt.Println("pointer back", gc == c, ok);

    taker t = new sinkᴠtaker(new sink<point>(n: 4));
    fmt.Println("parameter", t.take(new point(1, 1)));

    sizer z = new bag<nint>([1, 2, 3]);
    sizer y = new bag<@string>(["a"]);
    fmt.Println("no parameter named", z.size(), y.size());

    valuer o = new outer(new box<point>(new point(24, 25)), 1);
    fmt.Println("embedded", o.val().x, o.val().y);
}

} // end main_package
