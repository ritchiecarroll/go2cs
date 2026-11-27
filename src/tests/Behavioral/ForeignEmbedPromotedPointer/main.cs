namespace go;

using fmt = fmt_package;
using lib = ForeignEmbedPromotedPointer.lib_package;
using ForeignEmbedPromotedPointer;

partial class main_package {

partial struct U {
    /*embed*/ public ForeignEmbedPromotedPointer.lib_package.T T;
    internal @string label;
}

internal static void main() {
    var u = Ꮡ(new U(label: "u"));

    lib.Named named = new UжNamed(u);
    fmt.Println(named.Name());

    lib.Counter counter = new UжCounter(u);
    fmt.Println(counter.Inc(), counter.Inc(), u.of(U.ᏑT).Inc());
    fmt.Println(counter.Name(), u.label);

    lib.Named byValue = new U(label: "v");
    fmt.Println(byValue.Name());
}

} // end main_package
