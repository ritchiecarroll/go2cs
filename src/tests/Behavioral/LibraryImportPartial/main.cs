namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object libraryImportPartialˢ = (@string)"library import partial:"u8;

internal static void Main() {
    fmt.Println(libraryImportPartialˢ, describe());
}

internal static @string describe() {
    return "compiled, initialized and ran";
}

} // end main_package
