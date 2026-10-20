namespace go;

using stderrors = errors_package;
using fmt = fmt_package;
using errors = ShadowedStdlibImportAlias.errors_package;
using ShadowedStdlibImportAlias;

partial class main_package {

internal static void Main() {
    var @base = stderrors.New("base");
    var wrapped = fmt.Errorf("wrapped: %w"u8, @base);
    fmt.Println("is:", errors.Is(wrapped, @base));
    fmt.Println("unwrap:", AreEqual(errors.Unwrap(wrapped), @base));
}

} // end main_package
