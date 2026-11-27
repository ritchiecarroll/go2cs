namespace go.ForeignEmbedPromotedPointer.fam;

using right = go.ForeignEmbedPromotedPointer.fam.right_package;
using lib = go.ForeignEmbedPromotedPointer.lib_package;
using go.ForeignEmbedPromotedPointer;
using go.ForeignEmbedPromotedPointer.fam;

partial class left_package {

partial struct LR {
    /*embed*/ public go.ForeignEmbedPromotedPointer.fam.right_package.T T;
}

partial struct LL {
    /*embed*/ public go.ForeignEmbedPromotedPointer.lib_package.T T;
}

public static @string Right() {
    right.Whoer w = new LRжWhoer(Ꮡ(new LR()));
    return w.Who();
}

public static @string Lib() {
    var ll = Ꮡ(new LL());
    lib.Counter c = new LLжCounter(ll);
    c.Inc();
    return c.Name();
}

} // end left_package
