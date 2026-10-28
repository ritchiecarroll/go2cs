namespace go;

using fmt = fmt_package;
using addrlib = CrossPkgLiteralNestedField.addrlib_package;
using CrossPkgLiteralNestedField;

partial class main_package {

internal static void Main() {
    var a = Ꮡ(new addrlib.Addr(Name: "hello"));
    fmt.Println(a.Capacity());

    var (ᴛ1, ᴛ2) = a.Encode();
    fmt.Println(ᴛ1, ᴛ2);
    fmt.Println(a.PathByte(0), a.PathByte(4));

    var @long = Ꮡ(new addrlib.Addr(Name: "0123456789abcdef"));
    fmt.Println(@long.Capacity());
    var (ᴛ3, ᴛ4) = @long.Encode();
    fmt.Println(ᴛ3, ᴛ4);

    var e = Ꮡ(new addrlib.Embedder(Name: "world"));
    fmt.Println(e.Slots());
    fmt.Println(e.Put(2, 7));

    addrlib.Addr z = new();
    fmt.Println(z.Capacity());
    var empty = Ꮡ(new addrlib.Addr());
    fmt.Println(empty.Capacity());
}

} // end main_package
