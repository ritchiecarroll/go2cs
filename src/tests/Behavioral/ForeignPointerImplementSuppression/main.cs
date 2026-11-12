namespace go;

using fmt = fmt_package;
using shade = ForeignPointerImplementSuppression.shade_package;
using tone = ForeignPointerImplementSuppression.tone_package;
using ForeignPointerImplementSuppression;

partial class main_package {

internal static void main() {
    var t = Ꮡ(new tone.Tone(D: 3));
    tone.Level l = new tone.ToneжLevel(t);

    fmt.Println("tone:", l.Tone(), l.Name());

    l.Set(7);
    fmt.Println("alias:", t.D, l.Tone());

    tone.Level l2 = new tone.ToneжLevel(t);
    fmt.Println("eq:", AreEqual(l, l2), AreEqual(l, ((tone.Level)new tone.ToneжLevel(Ꮡ(new tone.Tone(D: 7))))));

    var (back, ok) = l._<ж<tone.Tone>>(ᐧ);
    fmt.Println("assert:", ok, back == t);
    var (_, badOK) = l._<ж<tone.Plain>>(ᐧ);
    fmt.Println("assert-miss:", badOK);

    switch (l.type()) {
    case ж<tone.Plain> v: {
        fmt.Println("switch:", "plain", v.D);
        break;
    }
    case ж<tone.Tone> v: {
        fmt.Println("switch:", "tone", v.D);
        break;
    }
    default: {
        var v = l;
        fmt.Println("switch:", "default");
        break;
    }}

    var seen = new map<tone.Level, @string>{};
    seen[l] = "first";
    seen[l2] = "second";
    fmt.Println("map:", seen[l], len(seen));

    var p = Ꮡ(new tone.Plain(D: 4));
    tone.Level pl = new tone.PlainжLevel(p);
    pl.Set(6);
    fmt.Println("plain:", pl.Tone(), pl.Name(), p.D);

    fmt.Println("lib:", tone.Default.Tone(), tone.DefaultPlain.Tone());

    fmt.Println("describe:", AreEqual(tone.Describe(t), l));

    tone.Level ln = new tone.LoneжLevel(Ꮡ(new tone.Lone(D: 5)));
    ln.Set(2);
    fmt.Println("lone:", ln.Tone(), ln.Name());

    shade.Level sl = new tone_ToneжLevel(t);
    sl.Set(9);
    fmt.Println("shade:", sl.Tone(), sl.Name(), t.D);

    fmt.Println("cross:", l.Tone(), sl.Tone(), back.D);
}

} // end main_package
