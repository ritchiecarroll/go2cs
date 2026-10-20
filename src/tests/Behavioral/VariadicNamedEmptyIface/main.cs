global using Hook = object;

namespace go;

using fmt = fmt_package;
using ꓸꓸꓸHook = Span<Hook>;

partial class main_package {

internal static nint compose(params ꓸꓸꓸHook hsʗp) {
    var hs = hsʗp.sslice();

    return len(hs);
}

internal static void Main() {
    fmt.Println(compose((nint)1, "two", 3.0D));
}

} // end main_package
