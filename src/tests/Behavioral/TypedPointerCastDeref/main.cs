namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct Pt {
    public nint X, Y;
}

partial struct Count /*num:uint32*/;

internal static Pt derefStruct(ж<Pt> Ꮡp) {
    return ~Ꮡp;
}

internal static Count derefNamedNum(ж<Count> Ꮡp) {
    return ~Ꮡp;
}

internal static ж<Pt> viaUnsafe(ж<Pt> Ꮡp) {
    return Ꮡp;
}

internal static ж<Pt> convIdentity(ж<Pt> Ꮡp) {
    return Ꮡp;
}

internal static void assignThrough(ж<Pt> Ꮡp) {
    (Ꮡp).Value = new Pt(7, 8);
}

internal static Pt derefLocal() {
    ref var pt = ref heap<Pt>(out var Ꮡpt);
    pt = new Pt(3, 4);
    var q = Ꮡpt;
    return ~q;
}

internal static ж<Pt> advance(ж<Pt> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.X += 100;
    return Ꮡp;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object structˢ = (@string)"struct:"u8;
private static readonly object namedNumˢ = (@string)"named num:"u8;
private static readonly object viaUnsafeWritesThroughˢ = (@string)"via unsafe writes through:"u8;
private static readonly object convIdentityWritesˢ = (@string)"conv identity writes through:"u8;
private static readonly object copyˢ = (@string)"copy:"u8;
private static readonly object originalˢ = (@string)"original:"u8;
private static readonly object assignedThroughˢ = (@string)"assigned through:"u8;
private static readonly object localˢ = (@string)"local:"u8;
private static readonly object callSurvivesˢ = (@string)"call survives:"u8;

internal static void Main() {
    ref var pt = ref heap<Pt>(out var Ꮡpt);
    pt = new Pt(1, 2);
    ref var c = ref heap(new Count(), out var Ꮡc);
    c = 42;
    fmt.Println(structˢ, derefStruct(Ꮡpt));
    fmt.Println(namedNumˢ, derefNamedNum(Ꮡc));
    viaUnsafe(Ꮡpt).Value.Y = 20;
    fmt.Println(viaUnsafeWritesThroughˢ, pt);
    convIdentity(Ꮡpt).Value.X = 10;
    fmt.Println(convIdentityWritesˢ, pt);
    var got = derefStruct(Ꮡpt);
    got.X = 555;
    fmt.Println(copyˢ, got, originalˢ, pt);
    assignThrough(Ꮡpt);
    fmt.Println(assignedThroughˢ, pt);
    fmt.Println(localˢ, derefLocal());
    fmt.Println(callSurvivesˢ, advance(Ꮡpt).Value);
}

} // end main_package
