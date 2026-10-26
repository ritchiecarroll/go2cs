namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    @string s = "a¢b☺\U0001d11e!";
    foreach (var (i, r) in s) {
        fmt.Println(i, (int32)r, (int32)s[i]);
    }

    @string bad = "x"u8 + ((sstring)new byte[]{0xED, 0xA0, 0x80}.slice()) + ((@string)new byte[]{0xC0, 0xAF}.slice()) + "y"u8 + ((@string)new byte[]{0xE2, 0x98}.slice());
    foreach (var (i, r) in bad) {
        fmt.Println(i, (int32)r);
    }

    var runes = slice<rune>(bad);
    fmt.Println(len(runes));
    foreach (var i in range(runes)) {
        fmt.Println(i, (int32)runes[i]);
    }

    @string t = "abc";
    var b = slice<byte>(t);
    b[0] = (rune)'X';
    fmt.Println(t, (@string)b);
}

} // end main_package
