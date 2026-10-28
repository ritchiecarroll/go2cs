namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    @string s = "identity probe string";
    @string t = s;

    fmt.Println(@unsafe.StringData(s) == @unsafe.StringData(t));
    fmt.Println(@unsafe.StringData(s) == @unsafe.StringData(s));

    @string u = (@string)append(slice<byte>(default), s.ꓸꓸꓸ);
    fmt.Println(@unsafe.StringData(s) == @unsafe.StringData(u));
    fmt.Println(s == u);

    @string v = s[1..];
    @string w = s[1..];

    fmt.Println(@unsafe.StringData(v) == @unsafe.StringData(v));
    fmt.Println(@unsafe.StringData(v) == @unsafe.StringData(w));
    fmt.Println(@unsafe.StringData(v) == @unsafe.StringData(s));
}

} // end main_package
