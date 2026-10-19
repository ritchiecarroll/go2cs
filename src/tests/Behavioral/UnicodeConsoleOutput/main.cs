namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object helloˢ = (@string)"Hello, 世界"u8;
private static readonly object earthˢ = (@string)"🌍 earth"u8;

internal static void Main() {
    fmt.Println(helloˢ);

    fmt.Printf("%s|%s|%s\n"u8, (@string)"日本語"u8, (@string)"Ελληνικά"u8, (@string)"Кириллица"u8);
    fmt.Println((@string)"π ≈ 3.14159"u8);

    fmt.Println(earthˢ);

    @string world = "世界";
    fmt.Println(len(world), ((@string)(slice<rune>(world))[0]));
}

} // end main_package
