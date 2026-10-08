namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct cycle {
    internal nint n;
}

partial struct rec {
    internal array<cycle> future = new(3);
}

partial struct holder {
    internal ж<rec> r;
}

internal static void bump(ref cycle c) {
    c.n++;
}

internal static void viaParam(ж<rec> Ꮡp, nint i) {
    var c = Ꮡp.at(rec.Ꮡfuture, i);
    bump(ref (c).DerefOrNull());
}

internal static void viaLocal(ref holder h, nint i) {
    var p = h.r;
    var c = p.at(rec.Ꮡfuture, i);
    bump(ref (c).DerefOrNull());
}

internal static void Main() {
    ref var r = ref heap<rec>(out var Ꮡr);
    r = new rec(future: new cycle[]{new(0), new(0), new(0)}.array());
    viaParam(Ꮡr, 0);
    viaParam(Ꮡr, 0);
    fmt.Println(r.future[0].n);
    var h = Ꮡ(new holder(r: Ꮡr));
    viaLocal(ref (h).DerefOrNull(), 1);
    fmt.Println(r.future[1].n);
}

} // end main_package
