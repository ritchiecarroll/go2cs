global using eface = object;
global using namedIface = go.fmt_package.Stringer;
global using aliasIface = go.fmt_package.Stringer;

namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {
// Descriptor carrier for `eface` — uninhabited; see GoDescriptorTypeAttribute.
[GoLocalName("eface")] internal interface efaceᴅ { }

// Descriptor carrier for `namedIface` — uninhabited; see GoDescriptorTypeAttribute.
[GoLocalName("namedIface")] internal interface namedIfaceᴅ { }


[GoType("@string")] partial struct ordinary;

[GoType] partial interface inlineIface {
    void Do();
}

internal static void nameOf<T, Tᴺ>(@string label) {
    var t = reflect.TypeFor<Tᴺ>();
    fmt.Printf("%-11s Name=%q String=%q PkgPath=%q Kind=%v\n"u8, label, t.Name(), t.String(), t.PkgPath(), t.Kind());
}

internal static void sizeOf<T>(@string label, T v) {
    fmt.Printf("%-11s dynamic=%T\n"u8, label, v);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string efaceˢ = "eface"u8;
private static readonly @string namedIfaceˢ = "namedIface"u8;
private static readonly @string anyˢ = "any"u8;
private static readonly @string aliasIfaceˢ = "aliasIface"u8;
private static readonly @string inlineIfaceˢ = "inlineIface"u8;
private static readonly @string ordinaryˢ = "ordinary"u8;
private static readonly @string negOrdinaryˢ = "neg-ordinary"u8;
private static readonly @string negEfaceˢ = "neg-eface"u8;

internal static void Main() {
    nameOf<eface, efaceᴅ>(efaceˢ);
    nameOf<namedIface, namedIfaceᴅ>(namedIfaceˢ);
    nameOf<any, any>(anyˢ);
    nameOf<aliasIface, aliasIface>(aliasIfaceˢ);
    nameOf<inlineIface, inlineIface>(inlineIfaceˢ);
    nameOf<ordinary, ordinary>(ordinaryˢ);
    sizeOf(negOrdinaryˢ, ((ordinary)(@string)"x"u8));
    sizeOf<eface>(negEfaceˢ, default!);
}

} // end main_package
