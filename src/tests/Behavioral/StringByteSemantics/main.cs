namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abcˢ = "abc"u8;

internal static void Main() {
    @string s = "a¢b☺\U0001d11e!"u8;
    foreach (var (i, r) in s) {
        fmt.Println(i, (int32)r, (int32)s[i]);
    }

    @string bad = "x"u8 + ((sstring)new byte[]{0xED, 0xA0, 0x80}.slice()) + ((@string)new byte[]{0xC0, 0xAF}.slice()) + "y"u8 + ((@string)new byte[]{0xE2, 0x98}.slice());
    foreach (var (i, r) in bad) {
        fmt.Println(i, (int32)r);
    }

    var runes = slice<rune>(bad);
    fmt.Println(len(runes));
    foreach (var (i, _) in runes) {
        fmt.Println(i, (int32)runes[i]);
    }

    @string t = abcˢ;
    var b = slice<byte>(t);
    b[0] = (rune)'X';
    fmt.Println(t, ((@string)b));
}

} // end main_package
