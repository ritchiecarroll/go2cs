namespace go;

using fmt = fmt_package;
// blank import: BlankImportSideEffects.jpeglike_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: BlankImportSideEffects.pnglike_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using registry = BlankImportSideEffects.registry_package;
using BlankImportSideEffects;

partial class main_package {

internal static nint countAtInit;

[GoInit] internal static void init() {
    countAtInit = registry.Count();
}

internal static void Main() {
    fmt.Println("count at init:", countAtInit);

    foreach (var (_, name) in new @string[]{"jpeglike", "pnglike", "giflike"}.slice()) {
        {
            var (describe, ok) = registry.Lookup(name); if (ok){
                fmt.Println("found:", name, "=>", describe);
            } else {
                fmt.Println("missing:", name);
            }
        }
    }

    fmt.Println("count:", registry.Count());
}

} // end main_package
