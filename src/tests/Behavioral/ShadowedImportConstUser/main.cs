namespace go;

using fmt = fmt_package;
using ShadowedImportConstLib = ShadowedImportConstLib_package;

partial class main_package {

partial struct gauge {
    internal nint level;
}

internal static nint ShadowedImportConstLib(this gauge g) {
    return g.level;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object spanˢ = (@string)"span:"u8;
private static readonly object peakMethodˢ = (@string)"peak method:"u8;

internal static void Main() {
    var g = new gauge(level: 3);
    var span = ((ShadowedImportConstLib.Span)2) * ShadowedImportConstLib_package.ΔPeak;
    fmt.Println(spanˢ, (int64)span);
    var m = new ShadowedImportConstLib_package.Meter(Level: g.ShadowedImportConstLib());
    fmt.Println(peakMethodˢ, m.Peak());
}

} // end main_package
