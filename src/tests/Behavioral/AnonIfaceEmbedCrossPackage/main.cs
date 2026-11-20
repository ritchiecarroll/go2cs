namespace go;

using fmt = fmt_package;
using lib = global::go.go.anonifaceembed_package;
using anonifaceembed = global::go.go.anonifaceembed_package;
using global::go.go;
using static main_package;

partial class main_package {

partial struct mine {
}

} partial class mainᴛmine {

internal static nint Apply(this mine _) { return 5; }
internal static @string Name(this mine _) { return "mine"; }

} partial class main_package {

internal static nint useOption(lib.Option o) { return o.Apply(); }

internal static @string useOther(lib.Other o) { return o.Name(); }

partial struct local {
    internal nint n;
}

} partial class mainᴛlocal {

internal static nint run(this local l) { return l.n + 1; }

} partial class main_package {

internal static void main() {
    fmt.Println(useOption(lib.With(3)));
    fmt.Println(useOther(lib.With(3)));
    fmt.Println(useOption(lib.WithMixed(4)));
    fmt.Println(lib.WithMixed(4).Extra());
    fmt.Println(lib.Take(new mine()));
    fmt.Println(lib.Take(lib.With(6)));

    var w = lib.With(7);
    lib.Option o = w;
    fmt.Println(o.Apply(), w.Name());

    fmt.Println(useRun(new local(8)));
    runs rr = new useRun_rᴠruns(mkRun(9));
    fmt.Println(rr.run());
    _ = lib.Runner(9);
}

internal partial interface useRun_r /*dyn*/ {
    nint run();
}

internal static nint useRun(useRun_r r) { return r.run(); }

partial interface runs {
    nint run();
}

internal static useRun_r mkRun(nint n) { return new local(n); }

} // end main_package
