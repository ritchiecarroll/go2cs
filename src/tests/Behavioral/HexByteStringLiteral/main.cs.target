namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly @string data = ((@string)(new byte[]{0x50, 0x4b, 0x03, 0x04, 0xdb, 0x35, 0x30, 0xff, 0x92, 0x00, 0x4c, 0x4d, 0x54}));

internal static readonly @string octalData = ((@string)(new byte[]{0xff, 0x80, 0xc3, 0xbf, 0x41, 0x00, 0x7f, 0x5a}));

internal static readonly @string asciiOctal = "\u0041\u0042\u0009\u0043"u8;

internal static readonly @string escapedOctal = "\\101|\u0041|\\\u0041|\\377"u8;

internal static UntypedInt backslashRune => /* '\\' */ 92;

internal static UntypedInt octalRune => /* '\101' */ 65;

internal static nint get4(@string s, nint i) {
    return (nint)s[i] | (nint)s[i + 1] << 8 | (nint)s[i + 2] << 16 | (nint)s[i + 3] << 24;
}

internal static void dump(@string label, @string s) {
    fmt.Println(label);
    fmt.Println(len(s));
    for (nint i = 0; i < len(s); i++) {
        fmt.Printf("%d "u8, s[i]);
    }
    fmt.Println();
}

internal static void Main() {
    fmt.Println(len(data));
    for (nint i = 0; i < len(data); i++) {
        fmt.Printf("%d "u8, data[i]);
    }
    fmt.Println();
    fmt.Println(get4(data, 0));

    dump("octalData", octalData);
    dump("asciiOctal", asciiOctal);

    @string local = ((@string)(new byte[]{0xfe, 0xfd}));
    dump("local", local);

    dump("escapedOctal", escapedOctal);

    @string localEscaped = ((@string)(new byte[]{0x5c, 0x33, 0x37, 0x37, 0x7c, 0xff}));
    dump("localEscaped", localEscaped);

    fmt.Println((@string)(rune)backslashRune, (@string)(rune)octalRune, (nint)backslashRune, (nint)octalRune);
}

} // end main_package
