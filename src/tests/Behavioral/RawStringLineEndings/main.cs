namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly @string pemBlock = """
-----BEGIN TEST-----
QUJDREVGR0g=
-----END TEST-----

"""u8;

internal static slice<byte> rawBytes = slice<byte>("""
first
second
"""u8);

internal static @string describe(@string name) {
    return fmt.Sprintf("""
name: %s
done
"""u8, name);
}

internal static readonly @string quote = "`"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string lineOneLineTwoˢ = """
line one
line two
"""u8;
private static readonly @string endsWithANewlineˢ = """
ends with a newline

"""u8;
private static readonly @string cNoNewlineHereˢ = @"C:\no\newline\here"u8;
private static readonly @string keepsItsCrˢ = "keeps\r\nits CR"u8;

internal static void Main() {
    @string plain = lineOneLineTwoˢ;
    @string trailing = endsWithANewlineˢ;
    @string chain = """
head

""" + quote + @"x" + quote + """

tail
""";
    @string oneLine = cNoNewlineHereˢ;
    @string interpreted = keepsItsCrˢ;
    foreach (var (_, s) in new @string[]{plain, trailing, pemBlock, describe("p"u8), chain, oneLine, interpreted}.slice()) {
        fmt.Println(len(s));
        fmt.Printf("%q\n"u8, s);
    }
    fmt.Println(len(rawBytes));
    fmt.Printf("%q\n"u8, rawBytes);
    var h = (uint32)2166136261U;
    for (nint i = 0; i < len(pemBlock); i++) {
        h = ((uint32)(h ^ (uint32)pemBlock[i])) * 16777619;
    }
    fmt.Printf("%08x\n"u8, h);
}

} // end main_package
