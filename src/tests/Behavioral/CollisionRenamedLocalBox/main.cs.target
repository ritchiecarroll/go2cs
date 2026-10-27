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

internal static ж<box> lastBox;

internal static ж<nint> lastInt;

internal static void setN(ж<box> Ꮡb, nint v) {
    ref var b = ref Ꮡb.DerefOrNull();

    lastBox = Ꮡb;
    b.n = v;
}

internal static void bump(ж<nint> Ꮡnp) {
    ref var np = ref Ꮡnp.DerefOrNull();

    lastInt = Ꮡnp;
    np += 100;
}

internal static readonly ж<box> ᏑΔcounter = new StaticBox<box>(default(box));
internal static ref box Δcounter => ref ((StaticBox<box>)ᏑΔcounter).ValueSlot;

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
    ref var Δp = ref heap<box>(out var Ꮡp);
    Δp = new box(n: 0);
    setN(Ꮡp, 7);
    setN(Ꮡp, Δp.n + 3);
    bump(Ꮡp.of(box.Ꮡn));
    bump(ᏑΔcounter.of(box.Ꮡn));
    fmt.Println(Δcounter.n);
    fmt.Println(Δp.n, usesTypeP());
    fmt.Println((~lastBox).n, lastInt.Value);
}

} // end main_package
