namespace go;

using fmt = fmt_package;

partial class main_package {

public partial struct options {
    internal nint level;
}

// type Option is a methodless func type — rendered inline as its base delegate

internal static Action<ж<options>> withLevel(nint n) {
    return (ж<options> o) => {
        o.Value.level = n;
    };
}

internal static nint apply(params Span<Action<ж<options>>> optsʗp) {
    var opts = optsʗp.sslice();

    var o = Ꮡ(new options(nil));
    foreach (var (_, opt) in opts) {
        opt(o);
    }
    return (~o).level;
}

internal static void Main() {
    fmt.Println(apply(withLevel(5), withLevel(9)));
}

} // end main_package
