namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly @string constTable = ((@string)(new byte[]{0xff, 0x00, 0x80, 0x01, 0xfe, 0x7f, 0x0a, 0xff}));

internal static @string varTable = ""u8 + ((@string)(new byte[]{0xff, 0x00, 0x80, 0x01})) + ((@string)(new byte[]{0xfe, 0x7f, 0x0a, 0xff}));

internal static @string varSingle = ((@string)(new byte[]{0xff, 0x80, 0x01}));

internal static @string varTyped = ""u8 + ((@string)(new byte[]{0xff, 0x80})) + "\x7f"u8;

internal static readonly @string constText = "hello world";

internal static @string varText = ""u8 + "café "u8 + "白鵬翔"u8;

internal static void dump(@string name, @string s) {
    fmt.Print(name, " len=", len(s), " bytes=");

    for (nint i = 0; i < len(s); i++) {
        fmt.Print(s[i], " ");
    }

    fmt.Println();
}

internal static void Main() {
    dump("constTable", constTable);
    dump("varTable", varTable);
    dump("varSingle", varSingle);
    dump("varTyped", varTyped);

    fmt.Println("index", constTable[2], varTable[2], constTable[4], varTable[4]);

    @string local = ""u8 + ((@string)(new byte[]{0xff, 0x80})) + "\x02"u8;
    dump("local", local);

    var b = slice<byte>("" + ((@string)(new byte[]{0xff, 0x80})));
    fmt.Println("bytes", len(b), b[0], b[1]);

    dump("constText", constText);
    fmt.Println("varText", varText, len(varText));
}

} // end main_package
