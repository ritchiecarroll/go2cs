namespace go.EmbeddedArrayCopy;

using lib = go.EmbeddedArrayCopy.lib_package;
using go.EmbeddedArrayCopy;

partial class other_package {

partial struct Emb {
    /*embed*/ public go.EmbeddedArrayCopy.lib_package.Inner Inner;
    public nint N;
}

} // end other_package
