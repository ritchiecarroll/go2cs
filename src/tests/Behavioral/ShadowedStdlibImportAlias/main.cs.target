namespace go;

using stderrors = errors_package;
using fmt = fmt_package;
using errors = ShadowedStdlibImportAlias.errors_package;
using ShadowedStdlibImportAlias;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string baseˢ = "base"u8;
private static readonly object unwrapˢ = (@string)"unwrap:"u8;

internal static void Main() {
    var @base = stderrors.New(baseˢ);
    var wrapped = fmt.Errorf("wrapped: %w"u8, @base);
    fmt.Println((@string)"is:"u8, errors.Is(wrapped, @base));
    fmt.Println(unwrapˢ, AreEqual(errors.Unwrap(wrapped), @base));
}

} // end main_package
