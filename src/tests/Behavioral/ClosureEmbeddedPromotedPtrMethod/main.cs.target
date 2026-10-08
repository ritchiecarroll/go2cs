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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string startˢ = "start"u8;
private static readonly @string builtˢ = "built"u8;
private static readonly object innerˢ = (@string)"inner:"u8;
private static readonly @string appliedˢ = "applied"u8;
private static readonly object appliedˢ2 = (@string)"applied:"u8;

internal partial struct run_rep /*dyn*/ {
    internal partial ref counter counter { get; }
    internal @string label;
}

internal static (@string, nint) run() {
    run_rep rep = new(nil);
    rep.label = startˢ;
    nint build() {
        rep.counter.bump();
        rep.label = builtˢ;
        return rep.n;
    }
    fmt.Println(innerˢ, build(), rep.label);
    nint got = apply(() => {
        void touch() {
            rep.counter.bump();
            rep.label = appliedˢ;
        }
        touch();
        return rep.n;
    });
    fmt.Println(appliedˢ2, got);
    return (rep.label, rep.n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object outerˢ = (@string)"outer:"u8;

internal static void Main() {
    var (label, n) = run();
    fmt.Println(outerˢ, label, n);
}

} // end main_package
