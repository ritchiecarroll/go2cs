namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var rs = new rune[]{(rune)'a', 0xD800, 0xDFFF, 0x110000, -1, 0x4E16}.slice();

    @string s = (@string)rs;
    fmt.Println(len(s));

    for (nint i = 0; i < len(s); i++) {
        fmt.Println(s[i]);
    }

    foreach (var (_, r) in rs) {
        fmt.Println(len((@string)r));
    }
}

} // end main_package
