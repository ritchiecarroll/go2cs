namespace go.ForeignPairStructConversion;

partial class lib_package {

partial struct Pitch {
    public nint Hz;
}

internal static nint Double⁔(/*this*/ Pitch p) { return p.Hz * 2; }

partial struct Twin {
    public nint Hz;
}

internal static nint Half⁔(/*this*/ Twin t) { return t.Hz / 2; }

partial struct Note {
    public @string Name;
    public Pitch Pitch;
    public slice<nint> Marks;
    public ж<Pitch> Ref;
    public float64 Weight;
}

partial struct Rest {
}

public static Pitch Middle() { return new Pitch(Hz: 440); }

} // end lib_package
