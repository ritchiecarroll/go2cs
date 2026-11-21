namespace go.ForeignPointerImplementSuppression;

partial class tone_package {

partial interface Level {
    nint Tone();
    @string Name();
    void Set(nint d);
}

partial struct Tone {
    public nint D;
}

internal static nint Tone⁔(/*this*/ ref Tone t) { return t.D; }
internal static @string Name⁔(/*this*/ ref Tone t) { return "tone"; }
internal static void Set⁔(/*this*/ ref Tone t, nint d) { t.D = d; }

partial struct Plain {
    public nint D;
}

internal static nint Tone⁔(/*this*/ ref Plain p) { return p.D * 10; }
internal static @string Name⁔(/*this*/ ref Plain p) { return "plain"; }
internal static void Set⁔(/*this*/ ref Plain p, nint d) { p.D = d; }

partial struct Lone {
    public nint D;
}

internal static nint Tone⁔(/*this*/ ref Lone l) { return l.D * 100; }
internal static @string Name⁔(/*this*/ ref Lone l) { return "lone"; }
internal static void Set⁔(/*this*/ ref Lone l, nint d) { l.D = d; }

public static Level Default = new ToneжLevel(Ꮡ(new Tone(D: 3)));
public static Level DefaultPlain = new PlainжLevel(Ꮡ(new Plain(D: 4)));

public static Level Describe(ж<Tone> t) { return new ToneжLevel(t); }

} // end tone_package
