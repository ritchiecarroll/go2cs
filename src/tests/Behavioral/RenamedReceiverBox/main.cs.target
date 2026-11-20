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

partial struct counter {
    internal nint n;
}

} partial class mainᴛcounter {

internal static void add(this ж<counter> Ꮡc, nint d) {
    ref var c = ref Ꮡc.DerefOrNull();

    addInt(ref nonnil(ref c).n, d);
}

} partial class main_package {

internal static void addInt(ref nint x, nint d) { x += d; }

} partial class mainᴛcounter {

internal static void bumpTwice(this ж<counter> Ꮡp) {
    Ꮡp.add(1);
    Ꮡp.add(1);
}

} partial class main_package {

internal static nint addInClosure(ж<counter> Ꮡp, nint d) {
    ref var p = ref Ꮡp.DerefOrNull();

    void apply() {
        Ꮡp.Value.n += d;
        addInt(ref (Ꮡp.Ꮡn).DerefOrNull(), d);
    }
    apply();
    return p.n;
}

internal static void main() {
    var c = Ꮡ(new counter(n: 0));
    c.bumpTwice();
    fmt.Println(c.n);

    fmt.Println(addInClosure(c, 5));
    fmt.Println(c.n);

    p pv = default;
    pv.id = 7;
    tagger t = default;
    t.p();
    fmt.Println(pv.id);
}

} // end main_package
