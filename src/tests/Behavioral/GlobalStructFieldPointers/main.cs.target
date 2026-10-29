namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var options = slice<option>([
        new("fast", ᏑFeatures.of(Featuresᴛ1.ᏑHasFast)),
        new("wide", ᏑFeatures.of(Featuresᴛ1.ᏑHasWide))
    ]);

    foreach (var (_, o) in options) {
        o.flag.Value = true;
    }

    foreach (var (_, o) in options) {
        fmt.Printf("%s=%t\n"u8, o.name, o.flag.Value);
    }

    var level = ᏑFeatures.of(Featuresᴛ1.ᏑLevel);
    level.Value = 3;
    fmt.Printf("level=%d\n"u8, level.Value);

    fmt.Printf("global: HasFast=%t HasWide=%t Level=%d\n"u8,
        Features.HasFast, Features.HasWide, Features.Level);
}

} // end main_package
