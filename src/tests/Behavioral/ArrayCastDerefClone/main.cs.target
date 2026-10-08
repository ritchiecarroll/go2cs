namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct Row /*[3]nint*/;

internal static Row typedCastDeref(ж<Row> Ꮡp) {
    return (~Ꮡp).Clone();
}

internal static array<uintptr> typedCastDerefDirect(/*[2]*/ ж<array<uintptr>> Ꮡp) {
    return (~Ꮡp).Clone();
}

partial struct holder {
    internal Row r;
}

internal static holder typedCastDerefIntoStruct(ж<Row> Ꮡp) {
    return new holder(r: (~Ꮡp).Clone());
}

internal static void typedCastDerefAssign(ж<Row> Ꮡp) {
    (Ꮡp).Value = new Row(new nint[]{40, 50, 60}.array());
}

internal static Row castDerefReturn(@unsafe.Pointer p) {
    return (~(ж<Row>)(uintptr)(p)).Clone();
}

internal static array<uintptr> castDerefReturnDirect(@unsafe.Pointer p) {
    return (~(ж<array<uintptr>>)(uintptr)(p)).Clone();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object copyˢ = (@string)"copy:"u8;
private static readonly object originalˢ = (@string)"original:"u8;
private static readonly object directCopyˢ = (@string)"direct copy:"u8;
private static readonly object structFieldCopyˢ = (@string)"struct field copy:"u8;
private static readonly object assignedThroughˢ = (@string)"assigned through:"u8;

internal static void Main() {
    ref var src = ref heap<Row>(out var Ꮡsrc);
    src = new Row(new nint[]{1, 2, 3}.array());
    var got = typedCastDeref(Ꮡsrc);
    got[0] = 99;
    fmt.Println(copyˢ, got, originalˢ, src);
    ref var pair = ref heap(new array<uintptr>(2), out var Ꮡpair);

    pair = new uintptr[]{7, 8}.array();
    var gotPair = typedCastDerefDirect(Ꮡpair);
    gotPair[1] = 77;
    fmt.Println(directCopyˢ, gotPair, originalˢ, pair);
    var h = typedCastDerefIntoStruct(Ꮡsrc);
    h.r[2] = 33;
    fmt.Println(structFieldCopyˢ, h.r, originalˢ, src);
    typedCastDerefAssign(Ꮡsrc);
    fmt.Println(assignedThroughˢ, src);
    ref var r = ref heap(new Row(), out var Ꮡr);
    ref var u = ref heap(new array<uintptr>(2), out var Ꮡu);
    _ = castDerefReturn(@unsafe.Pointer.FromPinnedBox(Ꮡr));
    _ = castDerefReturnDirect(@unsafe.Pointer.FromPinnedBox(Ꮡu));
}

} // end main_package
