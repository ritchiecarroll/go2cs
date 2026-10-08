namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct vector {
    internal @string name;
    internal slice<byte> key;
}

internal static slice<vector> vectors = new vector[]{
    new("split"u8, slice<byte>((@string)("this is a longer than block-size key "u8 + "written as several pieces so it fits the "u8 + "source width, exactly as crypto/hmac writes it"u8))),
    new("plain"u8, slice<byte>("single"u8))
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloˢ = "hello"u8;

internal static void Main() {
    var bs = slice<byte>("hello"u8);
    var rs = slice<rune>((@string)"héllo");
    fmt.Println(len(bs), ((@string)bs));
    fmt.Println(len(rs), ((@string)rs));
    fmt.Println(slice<byte>("hi"u8));
    fmt.Println(slice<rune>((@string)"aΩ"));
    fmt.Println(slice<byte>(@"ab"u8));
    fmt.Println(slice<byte>(((@string)(new byte[]{0xff, 0xfe}))));
    var split = slice<byte>((@string)("first "u8 + "second "u8 + "third"u8));
    fmt.Println(len(split), ((@string)split));
    var rsplit = slice<rune>((@string)("α"u8 + "β"u8));
    fmt.Println(len(rsplit), ((@string)rsplit));
    fmt.Println(slice<byte>((@string)(@"raw "u8 + "cooked"u8)));
    foreach (var (_, v) in vectors) {
        fmt.Println(v.name, len(v.key), v.key[0], v.key[len(v.key) - 1]);
    }
    @string s = helloˢ;
    fmt.Println(((sstring)slice<byte>(s)) == ((sstring)bs));
}

} // end main_package
