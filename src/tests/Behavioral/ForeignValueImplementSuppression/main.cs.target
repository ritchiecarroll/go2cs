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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object ifaceEqˢ = (@string)"iface-eq:"u8;
private static readonly object assertˢ = (@string)"assert:"u8;
private static readonly object assertMissˢ = (@string)"assert-miss:"u8;
private static readonly object switchˢ = (@string)"switch:"u8;
private static readonly object grayˢ = (@string)"Gray"u8;
private static readonly object shadeˢ = (@string)"Shade"u8;
private static readonly object defaultˢ = (@string)"default"u8;
private static readonly @string fromIfaceˢ = "from-iface"u8;
private static readonly object mapˢ = (@string)"map:"u8;
private static readonly object callˢ = (@string)"call:"u8;
private static readonly object describeˢ = (@string)"describe:"u8;
private static readonly object funcˢ = (@string)"func:"u8;
private static readonly object funcLibˢ = (@string)"func-lib:"u8;

internal static void Main() {
    var p = Ꮡ(new Img(new hueꓸShade(L: 0x99, A: 0xff)));
    var got = p.At();
    var want = new hueꓸShade(L: 0x99, A: 0xff);
    fmt.Printf("shade: %v %T\n"u8, got, got);
    fmt.Println((@string)"eq:"u8, AreEqual(got, want), AreEqual(want, got));
    hue.Tint w = want;
    fmt.Println(ifaceEqˢ, AreEqual(got, w));
    fmt.Println((@string)"ne:"u8, AreEqual(got, new hueꓸShade(L: 1, A: 2)));
    var (v, ok) = got._<hueꓸShade>(ᐧ);
    fmt.Println(assertˢ, ok, v.L, v.A);
    var (_, badOK) = got._<hue.Gray>(ᐧ);
    fmt.Println(assertMissˢ, badOK);
    switch (got.type()) {
    case hue.Gray t: {
        fmt.Println(switchˢ, grayˢ, t.Y);
        break;
    }
    case hueꓸShade t: {
        fmt.Println(switchˢ, shadeˢ, t.L);
        break;
    }
    default: {
        var t = got;
        fmt.Println(switchˢ, defaultˢ);
        break;
    }}
    var seen = new map<hue.Tint, @string>{};
    seen[got] = fromIfaceˢ;
    fmt.Println(mapˢ, seen[want], len(seen));
    var (l, a) = got.Shade();
    fmt.Println(callˢ, l, a);
    fmt.Printf("lib: %v %T %v\n"u8, hue.Default, hue.Default, AreEqual(hue.Default, new hueꓸShade(L: 1, A: 2)));
    fmt.Printf("lib-gray: %v %T\n"u8, hue.DefaultGray, hue.DefaultGray);
    hue.Tint g = new hue.Gray(Y: 5);
    fmt.Printf("gray: %v %T %v\n"u8, g, g, AreEqual(g, new hue.Gray(Y: 5)));
    fmt.Println(describeˢ, AreEqual(hue.Describe(want), got));
    hue.Tint f = new hue_TinterᴠTint(new hue.Tinter(() => ((uint32)(11), (uint32)(22))));
    var (fl, fa) = f.Shade();
    fmt.Println(funcˢ, fl, fa);
    var (dl, da) = hue.DefaultFunc.Shade();
    fmt.Println(funcLibˢ, dl, da);
}

} // end main_package
