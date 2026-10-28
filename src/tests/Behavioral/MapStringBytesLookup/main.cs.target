namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var interned = new map<@string, @string>{
        ["Content-Length"] = "len",
        ["Host"] = "host"
    };

    var b = slice<byte>("Content-Length"u8);

    fmt.Println(interned[tmpstring(b)]);

    b[0] = (byte)'X';
    fmt.Println(interned[tmpstring(b)] == "");

    b[0] = (byte)'C';
    var (v, ok) = interned[tmpstring(b), ꟷ];
    fmt.Println(v, ok);
    b[0] = (byte)'X';
    (v, ok) = interned[tmpstring(b), ꟷ];
    fmt.Println(v == ""u8, ok);
    b[0] = (byte)'C';

    fmt.Println(interned[tmpstring(b[..4])] == "");
    fmt.Println(interned[tmpstring(b[0..])]);

    var w = new map<@string, nint>{};
    var k = slice<byte>("alpha"u8);
    w[((@string)k)] = 42;
    k[0] = (byte)'Z';
    fmt.Println(w["alpha"], len(w));
    var (_, hit) = w[tmpstring(k), ꟷ];
    fmt.Println(hit);

    delete(w, (@string)slice<byte>("alpha"u8));
    fmt.Println(len(w));
}

} // end main_package
