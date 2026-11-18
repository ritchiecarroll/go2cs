namespace go.ForeignPairStructConversion;

using static lib_package;

partial class lib_package {

partial struct Pitch {
    public nint Hz;
}

} partial class libᴛPitch {

public static nint Double(this Pitch p) { return p.Hz * 2; }

} partial class lib_package {

partial struct Twin {
    public nint Hz;
}

} partial class libᴛTwin {

public static nint Half(this Twin t) { return t.Hz / 2; }

} partial class lib_package {

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
