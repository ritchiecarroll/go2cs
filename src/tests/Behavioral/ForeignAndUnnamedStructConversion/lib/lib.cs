namespace go.ForeignAndUnnamedStructConversion;

using static lib_package;

partial class lib_package {

partial struct Pitch {
    public nint Hz;
}

} partial class libᴛPitch {

public static nint Double(this Pitch p) { return p.Hz * 2; }

} partial class lib_package {

partial struct Note {
    public @string Name;
    public Pitch Pitch;
    public slice<nint> Marks;
    public ж<Note> Next;
    public float64 Weight;
}

public static Pitch Middle() { return new Pitch(Hz: 440); }

} // end lib_package
