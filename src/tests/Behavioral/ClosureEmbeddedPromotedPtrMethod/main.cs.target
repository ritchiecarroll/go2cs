namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void bump(this ref counter c) {
    c.n++;
}

internal static nint apply(Func<nint> f) {
    return f();
}

internal partial struct run_rep /*dyn*/ {
    /*embed*/ internal counter counter;
    internal @string label;
}

internal static (@string, nint) run() {
    run_rep rep = new(nil);
    rep.label = "start";

    nint build() {
        rep.counter.bump();
        rep.label = "built";
        return rep.n;
    }

    fmt.Println("inner:", build(), rep.label);

    nint got = apply(() => {
        void touch() {
            rep.counter.bump();
            rep.label = "applied";
        }
        touch();
        return rep.n;
    });
    fmt.Println("applied:", got);

    return (rep.label, rep.n);
}

internal static void Main() {
    var (label, n) = run();
    fmt.Println("outer:", label, n);
}

} // end main_package
