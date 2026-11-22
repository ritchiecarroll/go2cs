namespace go.ForeignPairStructConversion;

using lib = go.ForeignPairStructConversion.lib_package;
using go.ForeignPairStructConversion;

partial class other_package {

partial struct Pitch {
    public nint Hz;
}

internal static nint Octave⁔(/*this*/ Pitch p) { return p.Hz * 2; }

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

internal static nint Triple⁔(/*this*/ Tag t) { return ((lib.Pitch)t).Hz * 3; }

public static Pitch Low() { return new Pitch(Hz: 110); }

} // end other_package
