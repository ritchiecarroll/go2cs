namespace go;

using fmt = fmt_package;
using hue = ForeignValueImplementSuppression.hue_package;
using ForeignValueImplementSuppression;

partial class main_package {

partial struct Img {
    internal hueꓸShade px;
}

public static hue.Tint At(this ref Img p) {
    return p.px;
}

internal static void Main() {
    var p = Ꮡ(new Img(new hueꓸShade(L: 0x99, A: 0xff)));
    var got = p.At();
    var want = new hueꓸShade(L: 0x99, A: 0xff);

    fmt.Printf("shade: %v %T\n"u8, got, got);

    fmt.Println("eq:", AreEqual(got, want), AreEqual(want, got));

    hue.Tint w = want;
    fmt.Println("iface-eq:", AreEqual(got, w));
    fmt.Println("ne:", AreEqual(got, new hueꓸShade(L: 1, A: 2)));

    var (v, ok) = got._<hueꓸShade>(ᐧ);
    fmt.Println("assert:", ok, v.L, v.A);
    var (_, badOK) = got._<hue.Gray>(ᐧ);
    fmt.Println("assert-miss:", badOK);

    switch (got.type()) {
    case hue.Gray t: {
        fmt.Println("switch:", "Gray", t.Y);
        break;
    }
    case hueꓸShade t: {
        fmt.Println("switch:", "Shade", t.L);
        break;
    }
    default: {
        var t = got;
        fmt.Println("switch:", "default");
        break;
    }}

    var seen = new map<hue.Tint, @string>{};
    seen[got] = "from-iface";
    fmt.Println("map:", seen[want], len(seen));

    var (l, a) = got.Shade();
    fmt.Println("call:", l, a);

    fmt.Printf("lib: %v %T %v\n"u8, hue.Default, hue.Default, AreEqual(hue.Default, new hueꓸShade(L: 1, A: 2)));
    fmt.Printf("lib-gray: %v %T\n"u8, hue.DefaultGray, hue.DefaultGray);

    hue.Tint g = new hue.Gray(Y: 5);
    fmt.Printf("gray: %v %T %v\n"u8, g, g, AreEqual(g, new hue.Gray(Y: 5)));

    fmt.Println("describe:", AreEqual(hue.Describe(want), got));

    hue.Tint f = new hue_TinterᴠTint(new hue.Tinter(() => ((uint32)(11), (uint32)(22))));
    var (fl, fa) = f.Shade();
    fmt.Println("func:", fl, fa);

    var (dl, da) = hue.DefaultFunc.Shade();
    fmt.Println("func-lib:", dl, da);
}

} // end main_package
