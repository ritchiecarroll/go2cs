namespace go;

using fmt = fmt_package;
using al = AliasImportLib_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object aliasedˢ = (@string)"aliased:"u8;

internal static void aliased() {
    var b = new AliasImportLibꓸB2(V: 7);
    var c = ((AliasImportLibꓸB2)new al.Box(V: 8));
    AliasImportLibꓸIntFn f = (nint x) => x * 3;
    fmt.Println(aliasedˢ, b, b.V, c.V, f(2));
}

} // end main_package
