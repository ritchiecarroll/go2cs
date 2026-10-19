namespace go;

using fmt = fmt_package;
using shade = ForeignPointerImplementSuppression.shade_package;
using tone = ForeignPointerImplementSuppression.tone_package;
using ForeignPointerImplementSuppression;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object toneˢ = (@string)"tone:"u8;
private static readonly object aliasˢ = (@string)"alias:"u8;
private static readonly object assertˢ = (@string)"assert:"u8;
private static readonly object assertMissˢ = (@string)"assert-miss:"u8;
private static readonly object switchˢ = (@string)"switch:"u8;
private static readonly object plainˢ = (@string)"plain"u8;
private static readonly object toneˢ2 = (@string)"tone"u8;
private static readonly object defaultˢ = (@string)"default"u8;
private static readonly object mapˢ = (@string)"map:"u8;
private static readonly object plainˢ2 = (@string)"plain:"u8;
private static readonly object libˢ = (@string)"lib:"u8;
private static readonly object describeˢ = (@string)"describe:"u8;
private static readonly object loneˢ = (@string)"lone:"u8;
private static readonly object shadeˢ = (@string)"shade:"u8;
private static readonly object crossˢ = (@string)"cross:"u8;

internal static void Main() {
    var t = Ꮡ(new toneꓸTone(D: 3));
    tone.Level l = new tone.ΔToneжLevel(t);

    fmt.Println(toneˢ, l.Tone(), l.Name());

    l.Set(7);
    fmt.Println(aliasˢ, (~t).D, l.Tone());

    tone.Level l2 = new tone.ΔToneжLevel(t);
    fmt.Println((@string)"eq:"u8, AreEqual(l, l2), AreEqual(l, ((tone.Level)new tone.ΔToneжLevel(Ꮡ(new toneꓸTone(D: 7))))));

    var (back, ok) = l._<ж<toneꓸTone>>(ᐧ);
    fmt.Println(assertˢ, ok, back == t);
    var (_, badOK) = l._<ж<tone.Plain>>(ᐧ);
    fmt.Println(assertMissˢ, badOK);

    switch (l.type()) {
    case ж<tone.Plain> v: {
        fmt.Println(switchˢ, plainˢ, (~v).D);
        break;
    }
    case ж<toneꓸTone> v: {
        fmt.Println(switchˢ, toneˢ2, (~v).D);
        break;
    }
    default: {
        var v = l;
        fmt.Println(switchˢ, defaultˢ);
        break;
    }}

    var seen = new map<tone.Level, @string>{};
    seen[l] = "first";
    seen[l2] = "second";
    fmt.Println(mapˢ, seen[l], len(seen));

    var p = Ꮡ(new tone.Plain(D: 4));
    tone.Level pl = new tone.PlainжLevel(p);
    pl.Set(6);
    fmt.Println(plainˢ2, pl.Tone(), pl.Name(), (~p).D);

    fmt.Println(libˢ, tone.Default.Tone(), tone.DefaultPlain.Tone());

    fmt.Println(describeˢ, AreEqual(tone.Describe(t), l));

    tone.Level ln = new tone.LoneжLevel(Ꮡ(new tone.Lone(D: 5)));
    ln.Set(2);
    fmt.Println(loneˢ, ln.Tone(), ln.Name());

    shade.Level sl = new tone_ΔToneжLevel(t);
    sl.Set(9);
    fmt.Println(shadeˢ, sl.Tone(), sl.Name(), (~t).D);

    fmt.Println(crossˢ, l.Tone(), sl.Tone(), (~back).D);
}

} // end main_package
