namespace go;

using fmt = fmt_package;
using lib = global::go.go.ppembedlib_package;
using global::go.go;
using ppembedlib = global::go.go.ppembedlib_package;

partial class main_package {

partial struct wrap {
    /*embed*/ public global::go.go.ppembedlib_package.Link Link;
}

internal static void add(this ref wrap w, @string s) { w.Add(s); }

partial struct holder {
    internal lib.Link link;
}

internal static void main() {
    var l = lib.NewLink("dest");
    l.Add("a");
    l.Add("b");
    var evalᴛ1 = l.Count();
    var evalᴛ2 = l.Last();
    fmt.Println("link:", l.Dest, evalᴛ1, evalᴛ2);

    lib.Link v = new(nil);
    v.Add("v1");
    fmt.Println("link value:", v.Count(), v.Last());

    var img = Ꮡ(new lib.Image());
    img.Add("i1");
    img.Add("i2");
    fmt.Println("image:", img.Count(), img.Last());

    lib.Image iv = new(nil);
    iv.Add("iv");
    fmt.Println("image value:", iv.Count(), iv.Last());

    var e = Ꮡ(new lib.Emph());
    e.Add("e1");
    e.Add("e2");
    fmt.Println("emph:", e.Count(), e.Last());

    var p = Ꮡ(new lib.Para());
    p.of(lib.Para.ᏑBaseNode).Add("p1");
    fmt.Println("para:", p.of(lib.Para.ᏑBaseNode).Count(), p.of(lib.Para.ᏑBaseNode).Last());

    var t = Ꮡ(new lib.Tag());
    t.Bump();
    fmt.Println("tag:", t.Bump());

    var w = Ꮡ(new wrap());
    w.add("w1");
    w.add("w2");
    fmt.Println("wrap:", w.Count(), w.Last());

    var s = new slice<lib.Link>(2, () => new(nil));
    s[0].Add("s0");
    s[1].Add("s1");
    s[1].Add("s1b");
    fmt.Println("slice:", s[0].Count(), s[1].Count(), s[1].Last());

    array<lib.Link> a = new(2, () => new(nil));
    a[0].Add("a0");
    a[0].Add("a0b");
    fmt.Println("array:", a[0].Count(), a[0].Last(), a[1].Count());

    var h = Ꮡ(new holder());
    h.link.Add("h1");
    fmt.Println("holder:", h.link.Count(), h.link.Last());

    ref var ev = ref heap(new lib.Link(), out var Ꮡev);
    var pe = Ꮡev;
    ev.Add("x");
    fmt.Println("escaped:", pe.Count(), pe.Last());
}

} // end main_package
