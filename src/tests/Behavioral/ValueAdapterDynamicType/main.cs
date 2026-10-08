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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object ifaceEqˢ = (@string)"iface-eq:"u8;
private static readonly object assertˢ = (@string)"assert:"u8;
private static readonly object assertMissˢ = (@string)"assert-miss:"u8;
private static readonly object assert1ˢ = (@string)"assert-1:"u8;
private static readonly object switchˢ = (@string)"switch:"u8;
private static readonly object grayˢ = (@string)"Gray"u8;
private static readonly object nrgbaˢ = (@string)"NRGBA"u8;
private static readonly object defaultˢ = (@string)"default"u8;
private static readonly object altEqˢ = (@string)"alt-eq:"u8;
private static readonly object altAssertˢ = (@string)"alt-assert:"u8;
private static readonly object altSwitchˢ = (@string)"alt-switch:"u8;
private static readonly object rgbaˢ = (@string)"rgba:"u8;
private static readonly @string fromIfaceˢ = "from-iface"u8;
private static readonly object mapˢ = (@string)"map:"u8;

internal static void Main() {
    var p = Ꮡ(new Img(new colorlike.NRGBA(R: 0x99, G: 0x99, B: 0x99, A: 0xff)));
    var got = p.At();
    var want = new colorlike.NRGBA(R: 0x99, G: 0x99, B: 0x99, A: 0xff);
    fmt.Println((@string)"eq:"u8, AreEqual(got, want), AreEqual(want, got));
    fmt.Println((@string)"ne:"u8, AreEqual(got, new colorlike.NRGBA(R: 1, G: 2, B: 3, A: 4)));
    colorlike.Color w2 = want;
    fmt.Println(ifaceEqˢ, AreEqual(got, w2));
    var (v, ok) = got._<colorlike.NRGBA>(ᐧ);
    fmt.Println(assertˢ, ok, v.R, v.A);
    var (_, badOK) = got._<colorlike.Gray>(ᐧ);
    fmt.Println(assertMissˢ, badOK);
    fmt.Println(assert1ˢ, got._<colorlike.NRGBA>().G);
    switch (got.type()) {
    case colorlike.Gray t: {
        fmt.Println(switchˢ, grayˢ, t.Y);
        break;
    }
    case colorlike.NRGBA t: {
        fmt.Println(switchˢ, nrgbaˢ, t.B);
        break;
    }
    default: {
        var t = got;
        fmt.Println(switchˢ, defaultˢ);
        break;
    }}
    var alt = p.Alt();
    fmt.Println(altEqˢ, AreEqual(alt, new colorlike.Gray(Y: 7)), AreEqual(alt, got));
    var (g, gok) = alt._<colorlike.Gray>(ᐧ);
    fmt.Println(altAssertˢ, gok, g.Y);
    switch (alt.type()) {
    case colorlike.NRGBA t: {
        fmt.Println(altSwitchˢ, nrgbaˢ, t.R);
        break;
    }
    case colorlike.Gray t: {
        fmt.Println(altSwitchˢ, grayˢ, t.Y);
        break;
    }}
    fmt.Printf("type: %T %T\n"u8, got, alt);
    var (r, gg, b, a) = got.RGBA();
    fmt.Println(rgbaˢ, r, gg, b, a);
    var seen = new map<colorlike.Color, @string>{};
    seen[got] = fromIfaceˢ;
    fmt.Println(mapˢ, seen[want], len(seen));
}

} // end main_package
