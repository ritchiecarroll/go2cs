namespace go.EmbeddedArrayCopy;

partial class lib_package {

partial struct Inner {
    public array<nint> V = new(2);
}

partial struct Emb {
    /*embed*/ public Inner Inner;
    public nint N;
}

} // end lib_package
