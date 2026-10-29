namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct Pt {
    public nint X, Y;
}

partial struct Count /*num:uint32*/;

internal static Pt derefStruct(ж<Pt> p) {
    return ~p;
}

internal static Count derefNamedNum(ж<Count> p) {
    return ~p;
}

internal static ж<Pt> viaUnsafe(ж<Pt> p) {
    return p;
}

internal static ж<Pt> convIdentity(ж<Pt> p) {
    return p;
}

internal static void assignThrough(ж<Pt> p) {
    (p).Value = new Pt(7, 8);
}

internal static Pt derefLocal() {
    ref var pt = ref heap<Pt>(new Pt(3, 4), out var Ꮡpt);
    var q = Ꮡpt;
    return ~q;
}

internal static ж<Pt> advance(ж<Pt> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.X += 100;
    return Ꮡp;
}

internal static void Main() {
    ref var pt = ref heap<Pt>(new Pt(1, 2), out var Ꮡpt);
    ref var c = ref heap(new Count(), out var Ꮡc);
    c = 42;

    fmt.Println("struct:", derefStruct(Ꮡpt));
    fmt.Println("named num:", derefNamedNum(Ꮡc));

    viaUnsafe(Ꮡpt).Value.Y = 20;
    fmt.Println("via unsafe writes through:", pt);

    convIdentity(Ꮡpt).Value.X = 10;
    fmt.Println("conv identity writes through:", pt);

    var got = derefStruct(Ꮡpt);
    got.X = 555;
    fmt.Println("copy:", got, "original:", pt);

    assignThrough(Ꮡpt);
    fmt.Println("assigned through:", pt);

    fmt.Println("local:", derefLocal());

    fmt.Println("call survives:", advance(Ꮡpt).Value);
}

} // end main_package
