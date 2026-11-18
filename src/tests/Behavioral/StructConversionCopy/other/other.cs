namespace go.StructConversionCopy;

using lib = go.StructConversionCopy.lib_package;
using go.StructConversionCopy;

partial class other_package {

partial struct Chord {
    public @string Name;
    public array<nint> Notes = new(2);
    public lib.Inner In;
}

} // end other_package
