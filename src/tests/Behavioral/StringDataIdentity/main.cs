namespace go;

using fmt = fmt_package;
using strings = strings_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void main() {
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

    var p = @unsafe.StringData(s);
    fmt.Println(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(p), 1) == @unsafe.Pointer.FromPinnedBox(@unsafe.StringData(s[1..])));
    fmt.Println(Ꮡ(@unsafe.Slice(p, len(s)), 2) == @unsafe.StringData(s[2..]));
    fmt.Println(@unsafe.String(p, 8));

    @string h = strings.Clone("hello, heap");
    @string g = h[7..];
    @unsafe.StringData(h[7..]).Value = (byte)'H';
    fmt.Println(h, g, "hello, heap");

    @string b2 = (@string)slice<byte>([(byte)'o', (byte)'k']);
    @unsafe.StringData(b2).Value = (byte)'O';
    fmt.Println(b2);
}

} // end main_package
