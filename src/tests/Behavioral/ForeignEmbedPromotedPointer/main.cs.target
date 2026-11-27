namespace go;

using fmt = fmt_package;
using left = ForeignEmbedPromotedPointer.fam.left_package;
using lib = ForeignEmbedPromotedPointer.lib_package;
using ForeignEmbedPromotedPointer;
using ForeignEmbedPromotedPointer.fam;

partial class main_package {

partial struct U {
    /*embed*/ public ForeignEmbedPromotedPointer.lib_package.T T;
    internal @string label;
}

partial struct W {
    /*embed*/ public ForeignEmbedPromotedPointer.lib_package.V V;
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

    lib.Adder adder = new UжAdder(u);
    var (sum, err) = adder.Add(3, 4);
    fmt.Println(sum, err);
    (sum, err) = adder.Add(50, 50);
    fmt.Println(sum, err, u.of(U.ᏑT).Inc());

    var w = Ꮡ(new W());
    w.of(W.ᏑInner).SetTag("w");
    lib.Tagger tagger = new WжTagger(w);
    fmt.Println(tagger.Tag());

    fmt.Println(left.Right(), left.Lib());
}

} // end main_package
