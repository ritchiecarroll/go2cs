namespace go;

using fmt = fmt_package;
using lib = EmbeddedArrayCopy.lib_package;
using other = EmbeddedArrayCopy.other_package;
using EmbeddedArrayCopy;

partial class main_package {

partial struct emb {
    /*embed*/ public EmbeddedArrayCopy.lib_package.Inner Inner;
    public nint N;
}

partial struct emb2 {
    /*embed*/ public EmbeddedArrayCopy.lib_package.Inner Inner;
    public nint N;
}

partial struct pair /*[2]nint*/;

partial struct tagged {
    /*embed*/ internal pair pair;
    public @string Tag;
}

partial struct outer {
    /*embed*/ public EmbeddedArrayCopy.lib_package.Emb Emb;
}

internal static lib.Emb fresh() {
    return new lib.Emb(Inner: new lib.Inner(V: new nint[]{1, 2}.array()), N: 1);
}

internal static nint bump(emb e) {
    e = e.ΔClone();

    e.V[0] = 100;
    return e.V[0];
}

internal static emb keep(emb e) {
    e = e.ΔClone();

    return e.ΔClone();
}

internal static void main() {
    var le = fresh();

    var c = le.ΔClone();
    (c.V[0], c.N) = (9, 9);
    fmt.Println("assign:", le.V, le.N, c.V, c.N);

    var e = (emb)le.ΔClone();
    (e.V[0], e.N) = (8, 8);
    fmt.Println("local from foreign:", le.V, le.N, e.V, e.N);

    var eb = (lib.Emb)e.ΔClone();
    (eb.V[1], eb.N) = (7, 7);
    fmt.Println("foreign from local:", e.V, e.N, eb.V, eb.N);

    var e2 = (emb2)e.ΔClone();
    (e2.Inner.V[0], e2.N) = (6, 6);
    fmt.Println("local from local:", e.V, e.N, e2.V, e2.N);

    var oe = otherꓸEmb(le.ΔClone());
    (oe.V[1], oe.N) = (5, 5);
    fmt.Println("foreign from foreign:", le.V, le.N, oe.V, oe.N);

    nint got = bump(e);
    fmt.Println("parameter:", e.V, got);

    var k = keep(e);
    k.V[1] = 44;
    fmt.Println("return:", e.V, k.V);

    var list = slice<lib.Emb>([le.ΔClone(), le.ΔClone()]);
    foreach (var (_, vᴛ1) in range(list)) {
        var x = vᴛ1.ΔClone();

        x.V[0] = 33;
    }
    var first = list[0].ΔClone();
    first.V[1] = 22;
    fmt.Println("range and element:", list[0].V, list[1].V, first.V);

    var t = new tagged(pair: new pair(new nint[]{1, 2}.array()), Tag: "t");
    var t2 = t.ΔClone();
    t2.pair[0] = 11;
    fmt.Println("embedded array:", t.pair, t2.pair);

    var o = new outer(Emb: le.ΔClone());
    var o2 = o.ΔClone();
    o2.V[0] = 77;
    fmt.Println("two levels:", o.V, o2.V, le.V);
}

} // end main_package
