namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string build(nint x) {
    @string s = default;
    switch (x) {
    case 1: {
        s += "one"u8;
        break;
    }
    case 2: {
        s += "two"u8;
        break;
    }}

    s += "!"u8;
    return s;
}

internal static void Main() {
    @string s = default;
    s += "a"u8;
    s += "b"u8;
    fmt.Println(s);

    @string z = default;
    fmt.Println(len(z), z == ""u8, "[" + z + "]");

    fmt.Println(build(1));
    fmt.Println(build(2));
    fmt.Println(build(3));
}

} // end main_package
