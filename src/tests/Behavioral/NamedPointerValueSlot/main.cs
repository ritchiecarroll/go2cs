namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct M /*map[nint, nint]*/;

partial class P /*ж<M>*/;

partial class PM /*ж<map<@string, nint>>*/;

partial class PS /*ж<slice<nint>>*/;

partial class PP /*ж<ж<nint>>*/;

internal static void namedMap() {
    ref var m = ref heap<M>(new M(new map<nint, nint>{[1] = 1, [2] = 2}), out var Ꮡm);
    var p = new P(Ꮡm);
    (p.ValueSlot)[3] = 3;
    fmt.Println("named map:", len(p.ValueSlot), (p.ValueSlot)[3], len(m));
}

internal static void unnamedMap() {
    ref var m = ref heap<map<@string, nint>>(new map<@string, nint>{["a"] = 1}, out var Ꮡm);
    var p = new PM(Ꮡm);
    (p.ValueSlot)["b"] = 2;
    fmt.Println("unnamed map:", len(p.ValueSlot), (p.ValueSlot)["b"], len(m));
}

internal static void Δslice() {
    ref var s = ref heap<slice<nint>>(new nint[]{1, 2, 3}.slice(), out var Ꮡs);
    var p = new PS(Ꮡs);
    p.ValueSlot = append(p.ValueSlot, (nint)4);
    fmt.Println("slice:", len(p.ValueSlot), (p.ValueSlot)[3], len(s));
}

internal static void pointer() {
    ref var ip = ref heap<ж<nint>>(out var Ꮡip);
    var pp = new PP(Ꮡip);
    fmt.Println("pointer nil:", pp.ValueSlot == nil);
    ref var v = ref heap<nint>(7, out var Ꮡv);
    pp.ValueSlot = Ꮡv;
    fmt.Println("pointer set:", (pp.ValueSlot).Value, ip.Value);
}

internal static void Main() {
    namedMap();
    unnamedMap();
    Δslice();
    pointer();
}

} // end main_package
