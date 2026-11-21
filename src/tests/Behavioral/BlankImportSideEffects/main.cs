namespace go;

using fmt = fmt_package;
// import _ "BlankImportSideEffects/jpeglike" (init forced in ImportInitializers)
// import _ "BlankImportSideEffects/pnglike" (init forced in ImportInitializers)
using registry = BlankImportSideEffects.registry_package;
using BlankImportSideEffects;

partial class main_package {

internal static nint countAtInit;

[GoInit] internal static void init() {
    countAtInit = registry.Count();
}

internal static void main() {
    fmt.Println("count at init:", countAtInit);

    foreach (var (_, name) in range(slice<@string>(["jpeglike", "pnglike", "giflike"]))) {
        var (describe, ok) = registry.Lookup(name); if (ok) {
            fmt.Println("found:", name, "=>", describe);
        } else {
            fmt.Println("missing:", name);
        }
    }

    fmt.Println("count:", registry.Count());
}

} // end main_package
