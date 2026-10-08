namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct sample {
    internal @string name;
    internal float64 value;
    internal float32 small;
}

internal static float64 bigReturn() {
    return 123456789123456789123456789D;
}

internal static float64 smallReturn() {
    return 33909D;
}

internal static float32 f32Return() {
    return 5F;
}

internal static float64 directBig = 123456789123456789123456789D;

internal static float64 directSmall = 42D;

internal static float64 decimalControl = 339.7784D;

internal static void Main() {
    var samples = new sample[]{
        new("big"u8, 123456789123456789123456789D, 5F),
        new("decimal"u8, 339.7784D, 3.5F),
        new("small"u8, 33909D, 8F)
    }.slice();
    foreach (var (_, s) in samples) {
        fmt.Println(s.name, s.value, s.small);
    }
    fmt.Println(bigReturn(), smallReturn(), f32Return());
    fmt.Println(directBig, directSmall, decimalControl);
}

} // end main_package
