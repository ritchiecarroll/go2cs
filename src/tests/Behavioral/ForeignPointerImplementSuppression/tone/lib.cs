namespace go.ForeignPointerImplementSuppression;

partial class tone_package {

partial interface Level {
    nint Tone();
    @string Name();
    void Set(nint d);
}

partial struct ΔTone {
    public nint D;
}

public static nint Tone(this ref ΔTone t) {
    return t.D;
}

public static @string Name(this ref ΔTone t) {
    return "tone";
}

public static void Set(this ref ΔTone t, nint d) {
    t.D = d;
}

partial struct Plain {
    public nint D;
}

public static nint Tone(this ref Plain p) {
    return p.D * 10;
}

public static @string Name(this ref Plain p) {
    return "plain";
}

public static void Set(this ref Plain p, nint d) {
    p.D = d;
}

partial struct Lone {
    public nint D;
}

public static nint Tone(this ref Lone l) {
    return l.D * 100;
}

public static @string Name(this ref Lone l) {
    return "lone";
}

public static void Set(this ref Lone l, nint d) {
    l.D = d;
}

public static Level Default = new ΔToneжLevel(Ꮡ(new ΔTone(D: 3)));
public static Level DefaultPlain = new PlainжLevel(Ꮡ(new Plain(D: 4)));

public static Level Describe(ж<ΔTone> t) {
    return new ΔToneжLevel(t);
}

} // end tone_package
