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

partial struct counter {
    internal nint n;
}

internal static void add(this ж<counter> Ꮡc, nint d) {
    ref var c = ref Ꮡc.DerefOrNull();

    addInt(ref nonnil(ref c).n, d);
}

internal static void addInt(ref nint x, nint d) {
    x += d;
}

internal static void bumpTwice(this ж<counter> Ꮡp) {
    Ꮡp.add(1);
    Ꮡp.add(1);
}

internal static nint addInClosure(ж<counter> Ꮡp, nint d) {
    ref var Δp = ref Ꮡp.DerefOrNull();

    void apply() {
        Ꮡp.Value.n += d;
        addInt(ref (Ꮡp.of(counter.Ꮡn)).DerefOrNull(), d);
    }
    apply();
    return Δp.n;
}

internal static void Main() {
    var c = Ꮡ(new counter(n: 0));
    c.bumpTwice();
    fmt.Println((~c).n);
    fmt.Println(addInClosure(c, 5));
    fmt.Println((~c).n);
    Δp pv = default!;
    pv.id = 7;
    tagger t = default!;
    t.p();
    fmt.Println(pv.id);
}

} // end main_package
