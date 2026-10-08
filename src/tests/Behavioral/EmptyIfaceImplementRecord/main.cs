global using EmptyInterface = object;

namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

partial struct name {
    internal @string s;
}

internal static @string String(this name n) {
    return n.s;
}

internal static nint args(params ꓸꓸꓸany xsʗp) {
    var xs = xsʗp.sslice();

    return len(xs);
}

internal partial struct main_xs /*dyn*/ {
    /*embed*/ public EmptyInterface EmptyInterface;
}

internal static void Main() {
    fmt.Stringer s = new name("ok"u8);
    fmt.Println(args(new main_xs()), s.String());
}

} // end main_package
