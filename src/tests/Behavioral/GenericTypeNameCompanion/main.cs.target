global using eface = object;
global using namedIface = go.fmt_package.Stringer;
global using aliasIface = go.fmt_package.Stringer;

namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct ordinary /*@string*/;

partial interface inlineIface {
    void Do();
}

internal static void nameOf<T, Tᴺ>(@string label) {
    var t = reflect.TypeFor<Tᴺ>();
    fmt.Printf("%-11s Name=%q String=%q PkgPath=%q Kind=%v\n"u8, label, t.Name(), t.String(), t.PkgPath(), t.Kind());
}

internal static void sizeOf<T>(@string label, T v) {
    fmt.Printf("%-11s dynamic=%T\n"u8, label, (v).OrTypedNilOf());
}

internal static void Main() {
    nameOf<eface, efaceᴅ>("eface");
    nameOf<namedIface, namedIfaceᴅ>("namedIface");
    nameOf<any, any>("any");
    nameOf<aliasIface, aliasIface>("aliasIface");
    nameOf<inlineIface, inlineIface>("inlineIface");
    nameOf<ordinary, ordinary>("ordinary");

    sizeOf("neg-ordinary", (ordinary)(@string)"x"u8);
    sizeOf<eface>("neg-eface", default!);
}

} // end main_package
