namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct holder {
    internal ж<nint> p;
    internal slice<nint> s;
}

internal static void Main() {
    ref var p1 = ref heap<ж<nint>>(out var Ꮡp1);
    p1 = ((ж<nint>)nil);
    ref var p2 = ref heap<ж<nint>>(out var Ꮡp2);
    p2 = ((ж<nint>)nil);

    var pp1 = Ꮡp1;
    var pp2 = Ꮡp2;

    fmt.Println("pp1==pp2", pp1 == pp2);

    fmt.Println("pp1==nil", pp1 == nil);

    fmt.Println("*pp1==nil", pp1.ValueSlot == nil);
    fmt.Println("*pp1==*pp2", pp1.ValueSlot == pp2.ValueSlot);

    var alias = pp1;
    fmt.Println("alias==pp1", alias == pp1);

    ref var n = ref heap<nint>(out var Ꮡn);
    n = 42;
    alias.ValueSlot = Ꮡn;
    fmt.Println("p1 set", pp1.ValueSlot != nil, (pp1.ValueSlot).Value);

    ref var v = ref heap<nint>(out var Ꮡv);
    v = 5;
    ref var c = ref heap<ж<nint>>(out var Ꮡc);
    c = Ꮡv;
    ref var d = ref heap<ж<nint>>(out var Ꮡd);
    d = Ꮡv;
    fmt.Println("c==d", c == d, "&c==&d", Ꮡc == Ꮡd);

    var keys = new map<ж<ж<nint>>, @string>{[Ꮡc] = "c", [Ꮡd] = "d"};
    fmt.Println("keys", len(keys), keys[Ꮡc], keys[Ꮡd]);

    ref var late = ref heap<ж<nint>>(out var Ꮡlate);
    var lp = Ꮡlate;
    var stable = new map<ж<ж<nint>>, @string>{[lp] = "stable"};
    fmt.Println("before", stable[lp]);
    late = Ꮡv;
    fmt.Println("after", stable[lp], len(stable), (lp.ValueSlot).Value);

    ref var h = ref heap(new holder(), out var Ꮡh);
    var fp = Ꮡh.of(holder.Ꮡp);
    var fs = Ꮡh.of(holder.Ꮡs);
    fmt.Println("field nil", fp.ValueSlot == nil, fs.ValueSlot == default!, len(fs.ValueSlot));
    var fields = new map<ж<ж<nint>>, @string>{[fp] = "h.p"};
    fmt.Println("field key", fields[fp]);
    h.p = Ꮡv;
    fmt.Println("field set", fields[fp], (fp.ValueSlot).Value, fp.ValueSlot == c);
}

} // end main_package
