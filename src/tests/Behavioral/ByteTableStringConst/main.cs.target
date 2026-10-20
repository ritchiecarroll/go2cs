namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly @string revTab = ((@string)(new byte[]{0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0}));

internal static readonly @string greedyHex = ((@string)(new byte[]{0x0f, 0x01, 0x46, 0x43, 0x47, 0x49, 0x5f, 0x4d, 0x50, 0x58, 0x53, 0x5f, 0x43, 0x4f, 0x4e, 0x4e, 0x53, 0x31, 0x0a, 0x0d, 0x42, 0x45, 0x45, 0x46}));

internal static void Main() {
    for (nint i = 0; i < len(revTab); i++) {
        fmt.Printf("%d "u8, revTab[i]);
    }
    fmt.Println();
    fmt.Println("len:", len(revTab));

    for (nint i = 0; i < len(greedyHex); i++) {
        fmt.Printf("%d "u8, greedyHex[i]);
    }
    fmt.Println();
    fmt.Println("len:", len(greedyHex));
    fmt.Printf("%q\n"u8, greedyHex);
}

} // end main_package
