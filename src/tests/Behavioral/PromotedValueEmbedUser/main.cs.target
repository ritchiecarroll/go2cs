namespace go;

using fmt = fmt_package;
using PromotedValueEmbedLib = PromotedValueEmbedLib_package;

partial class main_package {

internal static @string describe(ж<PromotedValueEmbedLib.Widget> Ꮡw) {
    if (Ꮡw.Name() == "widget"u8) {
        return "match";
    }
    return "nomatch";
}

internal static void Main() {
    var w = PromotedValueEmbedLib.New("widget");
    fmt.Println(w.Name(), w.Name() == "widget"u8, describe(w));
    var g = PromotedValueEmbedLib.NewGadget("gadget");
    fmt.Println(g.Name());
}

} // end main_package
