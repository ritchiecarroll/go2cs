namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    fmt.Println("Hello, 世界");

    fmt.Printf("%s|%s|%s\n"u8, "日本語", "Ελληνικά", "Кириллица");
    fmt.Println("π ≈ 3.14159");

    fmt.Println("🌍 earth");

    @string world = "世界";
    fmt.Println(len(world), ((@string)(slice<rune>(world))[0]));
}

} // end main_package
