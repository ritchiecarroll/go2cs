namespace go;

using fmt = fmt_package;
using ShadowedImportConstLib = ShadowedImportConstLib_package;
using static main_package;

partial class main_package {

partial struct gauge {
    internal nint level;
}

} partial class mainᴛgauge {

internal static nint ShadowedImportConstLib(this gauge g) { return g.level; }

} partial class main_package {

internal static void main() {
    var g = new gauge(level: 3);

    var span = ((ShadowedImportConstLib.Span)2) * ShadowedImportConstLib.Peak;
    fmt.Println("span:", (int64)span);

    var m = new ShadowedImportConstLib.Meter(Level: g.ShadowedImportConstLib());
    fmt.Println("peak method:", m.Peak());
}

} // end main_package
