namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct p {
    internal nint id;
}

partial struct tagger {
}

} partial class mainᴛtagger {

internal static void p(this tagger _) { }

} partial class main_package {

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

internal static readonly ж<box> Ꮡcounter = new StaticBox<box>(default(box));
internal static ref box counter => ref ((StaticBox<box>)Ꮡcounter).ValueSlot;

} partial class mainᴛtagger {

internal static nint counter(this tagger _) { return 1; }

} partial class main_package {

internal static nint usesTypeP() {
    p pv = default;
    pv.id = 1;
    tagger t = default;
    t.p();
    return pv.id;
}

internal static void main() {
    ref var p = ref heap<box>(new box(n: 0), out var Ꮡp);
    setN(Ꮡp, 7);
    setN(Ꮡp, p.n + 3);
    bump(Ꮡp.Ꮡn);
    bump(Ꮡcounter.Ꮡn);
    fmt.Println(counter.n);
    fmt.Println(p.n, usesTypeP());
    fmt.Println(lastBox.n, lastInt.Value);
}

} // end main_package
