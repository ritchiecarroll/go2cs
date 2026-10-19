namespace go;

using fmt = fmt_package;
using PromotedValueEmbedLib = PromotedValueEmbedLib_package;

partial class main_package {

internal static map<@string, any> registry() {
    return new map<@string, any>{["w"] = PromotedValueEmbedLib.New("boxed").OrTypedNil()};
}

internal static void Main() {
    var m = registry();
    fmt.Println(m["w"]._<ж<PromotedValueEmbedLib.Widget>>().Name());
    fmt.Println(PromotedValueEmbedLib.New("direct").Name());
}

} // end main_package
