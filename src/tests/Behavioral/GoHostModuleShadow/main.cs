namespace go;

using fmt = fmt_package;
using consumer = GoHostModuleShadow.consumer_package;
using GoHostModuleShadow;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goLangˢ = "GoLang"u8;

internal static void Main() {
    fmt.Println(consumer.Describe(goLangˢ));
}

} // end main_package
