namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct pub {
    public nint I;
}

partial class pubPtr /*ж<pub>*/;

partial struct holder {
    internal pubPtr p;
}

internal static (ж<nint>, ж<nint>, ж<nint>) viaParam(pubPtr pp, ж<pub> q) {
    return (pp.of(pub.ᏑI), pp.of(pub.ᏑI), q.of(pub.ᏑI));
}

internal static void Main() {
    var pp = new pubPtr(Ꮡ(new pub(3)));
    var a = pp.of(pub.ᏑI);
    a.Value = 7;
    fmt.Println("explicit named:", (pp.Value).I, a == pp.of(pub.ᏑI));

    var b = pp.of(pub.ᏑI);
    b.Value = 8;
    fmt.Println("implicit named:", (pp.Value).I);

    var f = reflect.ValueOf(pp).Elem().Field(0);
    fmt.Println("reflect agrees:", f.Addr().Interface()._<ж<nint>>() == pp.of(pub.ᏑI));

    var h = new holder(p: new pubPtr(Ꮡ(new pub(1))));
    var (c, d) = (h.p.of(pub.ᏑI), h.p.of(pub.ᏑI));
    c.Value = 4;
    d.Value += 1;
    fmt.Println("named in a field:", (h.p.Value).I);

    var q = Ꮡ(new pub(10));
    var (x, y, z) = viaParam(pp, q);
    (x.Value, y.Value, z.Value) = (20, 21, 30);
    fmt.Println("through parameters:", (pp.Value).I, q.I);

    var p = Ꮡ(new pub(1));
    (p.of(pub.ᏑI)).Value = 2;
    var e = p.of(pub.ᏑI);
    e.Value = 5;
    fmt.Println("plain:", p.I, e == p.of(pub.ᏑI));
}

} // end main_package
