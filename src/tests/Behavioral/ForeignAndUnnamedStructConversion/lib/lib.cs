namespace go.ForeignAndUnnamedStructConversion;

partial class lib_package {

partial struct Pitch {
    public nint Hz;
}

internal static nint Double⁔(/*this*/ Pitch p) { return p.Hz * 2; }

partial struct Note {
    public @string Name;
    public Pitch Pitch;
    public slice<nint> Marks;
    public ж<Note> Next;
    public float64 Weight;
}

public static Pitch Middle() { return new Pitch(Hz: 440); }

} // end lib_package
