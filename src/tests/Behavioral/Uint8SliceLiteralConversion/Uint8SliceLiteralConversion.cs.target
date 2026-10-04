global using B = go.slice<byte>;

namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[]uint8")] partial struct buf;

internal static nint take(slice<uint8> b) {
    return len(b);
}

internal static slice<uint8> g = slice<uint8>("glob"u8);

internal static void Main() {
    var a = slice<uint8>("foo"u8);
    var b = slice<byte>("bar"u8);
    buf c = ((buf)slice<uint8>((@string)"baz"u8));
    var r = slice<int32>((@string)"héllo");
    var s = slice<uint8>((@string)("a"u8 + "b"u8));
    fmt.Println(len(a), len(b), len(c), take(slice<uint8>("qux"u8)), len(g), len(r), len(s), len(((B)"ali"u8)));
    fmt.Println(((@string)a), ((@string)g), ((@string)s), ((@string)r), r[1]);
}

} // end main_package
