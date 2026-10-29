namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var wᴷ = new stackAppend<byte>();

    slice<byte> b = default;
    for (var ch = (rune)'A'; ch <= 'E'; ch++) {
        b = append(b, ((@string)ch).ꓸꓸꓸ);
    }
    fmt.Println((@string)b);

    var bs = slice<byte>([(byte)'H', (byte)'i']);
    fmt.Println((((@string)bs))[0]);

    fmt.Println(len((@string)bs));

    slice<byte> m = default;
    m = append(m, ((@string)(rune)'世').ꓸꓸꓸ);
    fmt.Println(len(m));

    slice<byte> eb = default;
    eb = append(eb, ((@string)"runtime error: "u8).ꓸꓸꓸ);
    eb = append(eb, ((@string)"oops"u8).ꓸꓸꓸ);
    fmt.Println((@string)eb, len(eb));

    nint tag = 7;
    fmt.Println((@string)("g=" + "\tm="), tag);
    fmt.Println((@string)("a" + "b" + "c"));

    sink w = default;
    w = append(wᴷ, w, (byte)'h', (byte)'i');
    fmt.Println((@string)(slice<byte>)w, len(w));

    var glyphs = new map<@string, rune>{
        ["frak"] = (rune)0x1D504,
        ["ae"] = '\U000000C6'
    };
    fmt.Println(glyphs["frak"], glyphs["ae"], (@string)glyphs["frak"]);
}

partial struct sink /*[]byte*/;

} // end main_package
