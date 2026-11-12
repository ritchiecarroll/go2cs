namespace go.ForeignPointerImplementSuppression;

using static tone_package;

partial class tone_package {

partial interface Level {
    nint Tone();
    @string Name();
    void Set(nint d);
}

partial struct Tone {
    public nint D;
}

} partial class toneᴛTone {

public static nint Tone(this ref Tone t) { return t.D; }
public static @string Name(this ref Tone t) { return "tone"; }
public static void Set(this ref Tone t, nint d) { t.D = d; }

} partial class tone_package {

partial struct Plain {
    public nint D;
}

} partial class toneᴛPlain {

public static nint Tone(this ref Plain p) { return p.D * 10; }
public static @string Name(this ref Plain p) { return "plain"; }
public static void Set(this ref Plain p, nint d) { p.D = d; }

} partial class tone_package {

partial struct Lone {
    public nint D;
}

} partial class toneᴛLone {

public static nint Tone(this ref Lone l) { return l.D * 100; }
public static @string Name(this ref Lone l) { return "lone"; }
public static void Set(this ref Lone l, nint d) { l.D = d; }

} partial class tone_package {

public static Level Default = new ToneжLevel(Ꮡ(new Tone(D: 3)));
public static Level DefaultPlain = new PlainжLevel(Ꮡ(new Plain(D: 4)));

public static Level Describe(ж<Tone> t) { return new ToneжLevel(t); }

} // end tone_package
