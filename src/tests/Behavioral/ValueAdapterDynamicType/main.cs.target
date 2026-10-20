namespace go;

using fmt = fmt_package;
using colorlike = ValueAdapterDynamicType.colorlike_package;
using ValueAdapterDynamicType;

partial class main_package {

partial struct Img {
    internal colorlike.NRGBA px;
}

public static colorlike.Color At(this ref Img p) {
    return p.px;
}

public static colorlike.Color Alt(this ref Img p) {
    return new colorlike.Gray(Y: 7);
}

internal static void Main() {
    var p = Ꮡ(new Img(new colorlike.NRGBA(R: 0x99, G: 0x99, B: 0x99, A: 0xff)));
    var got = p.At();
    var want = new colorlike.NRGBA(R: 0x99, G: 0x99, B: 0x99, A: 0xff);

    fmt.Println("eq:", AreEqual(got, want), AreEqual(want, got));
    fmt.Println("ne:", AreEqual(got, new colorlike.NRGBA(R: 1, G: 2, B: 3, A: 4)));

    colorlike.Color w2 = want;
    fmt.Println("iface-eq:", AreEqual(got, w2));

    var (v, ok) = got._<colorlike.NRGBA>(ᐧ);
    fmt.Println("assert:", ok, v.R, v.A);
    var (_, badOK) = got._<colorlike.Gray>(ᐧ);
    fmt.Println("assert-miss:", badOK);
    fmt.Println("assert-1:", got._<colorlike.Color, colorlike.NRGBA>().G);

    switch (got.type()) {
    case colorlike.Gray t: {
        fmt.Println("switch:", "Gray", t.Y);
        break;
    }
    case colorlike.NRGBA t: {
        fmt.Println("switch:", "NRGBA", t.B);
        break;
    }
    default: {
        var t = got;
        fmt.Println("switch:", "default");
        break;
    }}

    var alt = p.Alt();
    fmt.Println("alt-eq:", AreEqual(alt, new colorlike.Gray(Y: 7)), AreEqual(alt, got));
    var (g, gok) = alt._<colorlike.Gray>(ᐧ);
    fmt.Println("alt-assert:", gok, g.Y);
    switch (alt.type()) {
    case colorlike.NRGBA t: {
        fmt.Println("alt-switch:", "NRGBA", t.R);
        break;
    }
    case colorlike.Gray t: {
        fmt.Println("alt-switch:", "Gray", t.Y);
        break;
    }}

    fmt.Printf("type: %T %T\n"u8, got, alt);

    var (r, gg, b, a) = got.RGBA();
    fmt.Println("rgba:", r, gg, b, a);

    var seen = new map<colorlike.Color, @string>{};
    seen[got] = "from-iface";
    fmt.Println("map:", seen[want], len(seen));
}

} // end main_package
