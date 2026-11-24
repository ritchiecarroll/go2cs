namespace go;

using fmt = fmt_package;
using ꓸꓸꓸOption = Span<main_package.Option>;

partial class main_package {

public partial struct options {
    internal nint level;
}

public delegate void Option(ж<options> _);

internal static Option withLevel(nint n) {
    return [GoFunc("withLevel.func1")] (ж<options> o) => {
        o.level = n;
    };
}

internal static nint apply(params ꓸꓸꓸOption optsʗp) {
    var opts = optsʗp.sslice();

    var o = Ꮡ(new options());
    foreach (var (_, opt) in range(opts)) {
        opt(o);
    }
    return o.level;
}

internal static void main() {
    fmt.Println(apply(withLevel(5), withLevel(9)));
}

} // end main_package
