namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly @string constTable = ((@string)(new byte[]{0xff, 0x00, 0x80, 0x01, 0xfe, 0x7f, 0x0a, 0xff}));

internal static @string varTable = ""u8 + ((@string)(new byte[]{0xff, 0x00, 0x80, 0x01})) + ((@string)(new byte[]{0xfe, 0x7f, 0x0a, 0xff}));

internal static @string varSingle = ((@string)(new byte[]{0xff, 0x80, 0x01}));

internal static @string varTyped = ""u8 + ((@string)(new byte[]{0xff, 0x80})) + "\x7f"u8;

internal static readonly @string constText = "hello world";

internal static @string varText = ""u8 + "café "u8 + "白鵬翔"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object lenˢ = (@string)" len="u8;
private static readonly object bytesˢ = (@string)" bytes="u8;

internal static void dump(@string name, @string s) {
    fmt.Print(name, lenˢ, len(s), bytesˢ);

    for (nint i = 0; i < len(s); i++) {
        fmt.Print(s[i], (@string)" "u8);
    }

    fmt.Println();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object indexˢ = (@string)"index"u8;
private static readonly object bytesˢ2 = (@string)"bytes"u8;
private static readonly object varTextˢ = (@string)"varText"u8;

internal static void Main() {
    dump("constTable", constTable);
    dump("varTable", varTable);
    dump("varSingle", varSingle);
    dump("varTyped", varTyped);

    fmt.Println(indexˢ, constTable[2], varTable[2], constTable[4], varTable[4]);

    @string local = ""u8 + ((@string)(new byte[]{0xff, 0x80})) + "\x02"u8;
    dump("local", local);

    var b = slice<byte>("" + ((@string)(new byte[]{0xff, 0x80})));
    fmt.Println(bytesˢ2, len(b), b[0], b[1]);

    dump("constText", constText);
    fmt.Println(varTextˢ, varText, len(varText));
}

} // end main_package
