namespace go.StructConversionCopy;

partial class lib_package {

partial struct Inner {
    public array<nint> V = new(2);
}

partial struct Chord {
    public @string Name;
    public array<nint> Notes = new(2);
    public Inner In;
}

} // end lib_package
