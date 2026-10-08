namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string frakˢ = "frak"u8;

internal static void Main() {
    slice<byte> b = default!;
    for (var ch = (rune)(rune)'A'; ch <= (rune)'E'; ch++) {
        b = append(b, ((@string)ch).ꓸꓸꓸ);
    }
    fmt.Println(((@string)b));
    var bs = new byte[]{(rune)'H', (rune)'i'}.slice();
    fmt.Println((((@string)bs))[0]);
    fmt.Println(len(((@string)bs)));
    slice<byte> m = default!;
    m = append(m, ((@string)(rune)(rune)'世').ꓸꓸꓸ);
    fmt.Println(len(m));
    slice<byte> eb = default!;
    eb = append(eb, ((@string)"runtime error: "u8).ꓸꓸꓸ);
    eb = append(eb, ((@string)"oops"u8).ꓸꓸꓸ);
    fmt.Println(((@string)eb), len(eb));
    nint tag = 7;
    fmt.Println((@string)("g=" + "\tm="), tag);
    fmt.Println((@string)("a" + "b" + "c"));
    sink w = default!;
    w = append(w, (byte)((rune)'h'), (byte)((rune)'i'));
    fmt.Println(((@string)(slice<byte>)w), len(w));
    var glyphs = new map<@string, rune>{
        ["frak"u8] = (rune)0x1D504,
        ["ae"u8] = (rune)'\U000000C6'
    };
    fmt.Println(glyphs[frakˢ], glyphs["ae"u8], ((@string)glyphs[frakˢ]));
}

partial struct sink /*[]byte*/;

} // end main_package
