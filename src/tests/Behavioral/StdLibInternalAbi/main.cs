namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    ΔKind kind = default!;
    kind = Slice;
    fmt.Printf("kind = %s\n"u8, kind.String());

    IntArgRegBitmap bitmap = default!;

    bitmap.Set(3);
    fmt.Printf("bitmap[0] = %t\n"u8, bitmap.Get(0));
    fmt.Printf("bitmap[3] = %t\n"u8, bitmap.Get(3));
}

} // end main_package
