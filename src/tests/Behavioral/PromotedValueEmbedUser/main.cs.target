namespace go;

using fmt = fmt_package;
using PromotedValueEmbedLib = PromotedValueEmbedLib_package;

partial class main_package {

internal static @string describe(ж<PromotedValueEmbedLib.Widget> w) {
    if (w.Name() == "widget"u8) {
        return "match";
    }
    return "nomatch";
}

internal static void main() {
    var w = PromotedValueEmbedLib.New("widget");
    fmt.Println(w.Name(), w.Name() == "widget"u8, describe(w));
    var g = PromotedValueEmbedLib.NewGadget("gadget");
    fmt.Println(g.Name());

    ref var b = ref heap<PromotedValueEmbedLib.Badge>(PromotedValueEmbedLib.NewBadge("badge"), out var Ꮡb);
    var (text, ok) = b.Both();
    var (self, name) = Ꮡb.of(PromotedValueEmbedLib.Badge.ᏑMeta).Self();
    fmt.Println(b.Text(), b.Ok(), text, ok, self.Text(), name);
}

} // end main_package
