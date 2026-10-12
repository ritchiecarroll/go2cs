namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string offˢ = "off"u8;

internal static @string setFlag(bool @true) {
    if (@true) {
        return "on"u8;
    }
    return offˢ;
}

internal static void Main() {
    fmt.Println(setFlag(true), setFlag(false));

    nint @false = 7;
    fmt.Println(@false + 1);
}

} // end main_package
