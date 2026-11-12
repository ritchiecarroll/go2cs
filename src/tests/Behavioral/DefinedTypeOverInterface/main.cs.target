global using Token = object;
global using Named = go.main_package.Stringer;

namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial interface Stringer {
    @string String();
}

partial struct point {
    internal nint x, y;
}

} partial class mainᴛpoint {

internal static @string String(this point p) { return fmt.Sprintf("(%d,%d)"u8, p.x, p.y); }

} partial class main_package {

internal static @string describe(Token t) {
    switch (t.type()) {
    case nint v: {
        return fmt.Sprintf("int:%d"u8, v);
    }
    case @string v: {
        return "str:"u8 + v;
    }
    case point v: {
        return "pt:"u8 + v.String();
    }}
    return "?";
}

internal static void main() {
    any t = (nint)42;
    fmt.Println(describe(t));

    @string msg = "hi";
    Token ts = msg;
    fmt.Println(describe(ts));

    fmt.Println(describe(new point(1, 2)));

    Named n = new point(3, 4);
    fmt.Println(n.String());
}

} // end main_package
