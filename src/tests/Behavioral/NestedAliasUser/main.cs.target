namespace go;

using fmt = fmt_package;
using inner = NestedAliasUser.inner_package;
using NestedAliasUser;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string alphaˢ = "alpha"u8;

internal static void Main() {
    var e = inner.NewEntry(alphaˢ, 3);
    fmt.Println(e.Name, e.Count);

    var e2 = new innerꓸEntry(Name: "beta"u8, Data: slice<byte>("xy"u8), Count: 5);
    fmt.Println(e2.Name, len(e2.Data), e2.Count);

    fmt.Println(inner.Total(new innerꓸEntry[]{e, e2}.slice()));
}

} // end main_package
