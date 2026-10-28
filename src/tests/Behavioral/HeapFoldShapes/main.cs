namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct grid {
    internal array<nint> cells = new(3);
    internal @string name;
}

internal static void bump(ref nint p) {
    p++;
}

internal static void mark(ref grid g, nint i) {
    g.cells[i] = 9;
}

internal static (nint, error) pair() {
    return (7, default);
}

internal static slice<ж<nint>> keptInts;
internal static slice<ж<grid>> keptGrids;

internal partial struct main_anon /*dyn*/ {
    internal nint v;
}

internal static void Main() {
    var keepᴷ = new stackAppend<ж<nint>>();

    ref var a = ref heap<nint>(out var Ꮡa);
    a = 10;
    keptInts = append(keptInts, Ꮡa);
    bump(ref a);
    fmt.Println("a:", a);

    ref var s = ref heap<@string>(out var Ꮡs);
    s = "hi";
    var ps = Ꮡs;
    ps.Value += "!"u8;
    fmt.Println("s:", s);

    ref var g = ref heap<grid>(out var Ꮡg);
    g = new grid(name: "g");
    keptGrids = append(keptGrids, Ꮡg);
    mark(ref g, 0);
    ref var h = ref heap<grid>(out var Ꮡh);
    h = g.ΔClone();
    keptGrids = append(keptGrids, Ꮡh);
    mark(ref h, 1);
    h.name = "h";
    fmt.Println("g:", g.cells, g.name, "h:", h.cells, h.name, "kept:", (~keptGrids[0]).cells, (~keptGrids[1]).cells, keptInts[0].Value);

    ref var c = ref heap<nint>(out var Ꮡc);
    c = 1;
    void inc() {
        var p = Ꮡc;
        p.Value += 10;
    }
    inc();
    inc();
    fmt.Println("c:", c);

    ref var anon = ref heap<main_anon>(out var Ꮡanon);
    anon = new main_anon(v: 3);
    var pa = Ꮡanon;
    pa.Value.v *= 2;
    fmt.Println("anon:", anon.v);

    ref var x = ref heap<nint>(out var Ꮡx);
    x = 1;
    ref var y = ref heap<nint>(out var Ꮡy);
    y = 2;
    var (px, py) = (Ꮡx, Ꮡy);
    (px.Value, py.Value) = (py.Value, px.Value);
    fmt.Println("x y:", x, y);

    ref var n = ref heap<nint>(out var Ꮡn);
    ref var err = ref heap<error>(out var Ꮡerr);
    (n, err) = pair();
    var (pn, pe) = (Ꮡn, Ꮡerr);
    pn.Value += 1;
    fmt.Println("n err:", n, pe.ValueSlot);

    slice<ж<nint>> keep = default;
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        ref var i = ref heap<nint>(out var Ꮡi);
        i = iᴛ1;
        keep = append(keepᴷ, keep, Ꮡi);
        iᴛ1 = i;
    }
    fmt.Println("loop:", keep[0].Value, keep[1].Value, keep[2].Value);

    ref var z = ref heap(new nint(), out var Ꮡz);
    var pz = Ꮡz;
    z = 5;
    fmt.Println("z:", pz.Value);
}

} // end main_package
