namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct Row /*[3]nint*/;

internal static Row typedCastDeref(ж<Row> p) {
    return (~p).Clone();
}

internal static array<uintptr> typedCastDerefDirect(/*[2]*/ ж<array<uintptr>> p) {
    return (~p).Clone();
}

partial struct holder {
    internal Row r;
}

internal static holder typedCastDerefIntoStruct(ж<Row> p) {
    return new holder(r: (~p).Clone());
}

internal static void typedCastDerefAssign(ж<Row> p) {
    (p).Value = new Row(new nint[]{40, 50, 60}.array());
}

internal static Row castDerefReturn(@unsafe.Pointer p) {
    return (~(ж<Row>)(uintptr)(p)).Clone();
}

internal static array<uintptr> castDerefReturnDirect(@unsafe.Pointer p) {
    return (~(ж<array<uintptr>>)(uintptr)(p)).Clone();
}

internal static void Main() {
    ref var src = ref heap<Row>(new Row(new nint[]{1, 2, 3}.array()), out var Ꮡsrc);
    var got = typedCastDeref(Ꮡsrc);
    got[0] = 99;
    fmt.Println("copy:", got, "original:", src);

    ref var pair = ref heap(new array<uintptr>(2), out var Ꮡpair);

    pair = new uintptr[]{7, 8}.array();
    var gotPair = typedCastDerefDirect(Ꮡpair);
    gotPair[1] = 77;
    fmt.Println("direct copy:", gotPair, "original:", pair);

    var h = typedCastDerefIntoStruct(Ꮡsrc);
    h.r[2] = 33;
    fmt.Println("struct field copy:", h.r, "original:", src);

    typedCastDerefAssign(Ꮡsrc);
    fmt.Println("assigned through:", src);

    ref var r = ref heap(new Row(), out var Ꮡr);
    ref var u = ref heap(new array<uintptr>(2), out var Ꮡu);
    _ = castDerefReturn(@unsafe.Pointer.FromPinnedBox(Ꮡr));
    _ = castDerefReturnDirect(@unsafe.Pointer.FromPinnedBox(Ꮡu));
}

} // end main_package
