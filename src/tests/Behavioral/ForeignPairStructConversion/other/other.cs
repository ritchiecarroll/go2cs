namespace go.ForeignPairStructConversion;

using lib = go.ForeignPairStructConversion.lib_package;
using go.ForeignPairStructConversion;
using static other_package;

partial class other_package {

partial struct Pitch {
    public nint Hz;
}

} partial class otherᴛPitch {

public static nint Octave(this Pitch p) { return p.Hz * 2; }

} partial class other_package {

partial struct Tagged {
    public nint Hz; /*`json:"hz"`*/
}

partial struct Note {
    public @string Name;
    public lib.Pitch Pitch;
    public slice<nint> Marks;
    public ж<lib.Pitch> Ref;
    public float64 Weight;
}

partial struct Rest {
}

partial struct Tag /*global::go.ForeignPairStructConversion.lib_package.Pitch*/;

} partial class otherᴛTag {

public static nint Triple(this Tag t) { return ((lib.Pitch)t).Hz * 3; }

} partial class other_package {

public static Pitch Low() { return new Pitch(Hz: 110); }

} // end other_package
