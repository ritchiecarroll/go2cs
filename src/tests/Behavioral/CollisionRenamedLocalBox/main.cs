namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Δp {
    internal nint id;
}

partial struct tagger {
}

internal static void p(this tagger _) {
}

partial struct box {
    internal nint n;
}

internal static void setN(ref box b, nint v) {
    b.n = v;
}

internal static void bump(ref nint np) {
    np += 100;
}

internal static ж<box> ᏑΔcounter = new StandardBox<box>(default(box));
internal static ref box Δcounter => ref ᏑΔcounter.Value;

internal static nint counter(this tagger _) {
    return 1;
}

internal static nint usesTypeP() {
    Δp pv = default!;
    pv.id = 1;
    tagger t = default!;
    t.p();
    return pv.id;
}

internal static void Main() {
    var Δp = new box(n: 0);
    setN(ref Δp, 7);
    setN(ref Δp, Δp.n + 3);
    bump(ref Δp.n);
    bump(ref Δcounter.n);
    fmt.Println(Δcounter.n);
    fmt.Println(Δp.n, usesTypeP());
}

} // end main_package
