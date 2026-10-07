namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

[GoType] partial struct pub {
    public nint I;
}

[GoType("ж<pub>")] partial class pubPtr;

[GoType] partial struct holder {
    internal pubPtr p;
}

internal static (ж<nint>, ж<nint>, ж<nint>) viaParam(pubPtr pp, ж<pub> Ꮡq) {
    return (pp.of(pub.ᏑI), pp.of(pub.ᏑI), Ꮡq.of(pub.ᏑI));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object explicitNamedˢ = (@string)"explicit named:"u8;
private static readonly object implicitNamedˢ = (@string)"implicit named:"u8;
private static readonly object reflectAgreesˢ = (@string)"reflect agrees:"u8;
private static readonly object namedInAFieldˢ = (@string)"named in a field:"u8;
private static readonly object throughParametersˢ = (@string)"through parameters:"u8;
private static readonly object plainˢ = (@string)"plain:"u8;

internal static void Main() {
    var pp = new pubPtr(Ꮡ(new pub(3)));
    var a = pp.of(pub.ᏑI);
    a.Value = 7;
    fmt.Println(explicitNamedˢ, (pp.Value).I, a == pp.of(pub.ᏑI));
    var b = pp.of(pub.ᏑI);
    b.Value = 8;
    fmt.Println(implicitNamedˢ, (pp.Value).I);
    var f = reflect.ValueOf(pp).Elem().Field(0);
    fmt.Println(reflectAgreesˢ, f.Addr().Interface()._<ж<nint>>() == pp.of(pub.ᏑI));
    var h = new holder(p: new pubPtr(Ꮡ(new pub(1))));
    var (c, d) = (h.p.of(pub.ᏑI), h.p.of(pub.ᏑI));
    c.Value = 4;
    d.Value += 1;
    fmt.Println(namedInAFieldˢ, (h.p.Value).I);
    var q = Ꮡ(new pub(10));
    var (x, y, z) = viaParam(pp, q);
    (x.Value, y.Value, z.Value) = (20, 21, 30);
    fmt.Println(throughParametersˢ, (pp.Value).I, (~q).I);
    var p = Ꮡ(new pub(1));
    (p.of(pub.ᏑI)).Value = 2;
    var e = p.of(pub.ᏑI);
    e.Value = 5;
    fmt.Println(plainˢ, (~p).I, e == p.of(pub.ᏑI));
}

} // end main_package
