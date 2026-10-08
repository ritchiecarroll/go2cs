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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string toneˢ = "tone"u8;

public static @string Name(this ref ΔTone t) {
    return toneˢ;
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string plainˢ = "plain"u8;

public static @string Name(this ref Plain p) {
    return plainˢ;
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string loneˢ = "lone"u8;

public static @string Name(this ref Lone l) {
    return loneˢ;
}

public static void Set(this ref Lone l, nint d) {
    l.D = d;
}

public static Level Default = new ΔToneжLevel(Ꮡ(new ΔTone(D: 3)));
public static Level DefaultPlain = new PlainжLevel(Ꮡ(new Plain(D: 4)));

public static Level Describe(ж<ΔTone> Ꮡt) {
    return new ΔToneжLevel(Ꮡt);
}

} // end tone_package
