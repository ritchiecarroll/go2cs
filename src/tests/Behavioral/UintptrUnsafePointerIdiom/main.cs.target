namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct point {
    internal int32 x, y;
}

internal static ж<point> Ꮡorigin = new StandardBox<point>(new point(x: 3, y: 4));
internal static ref point origin => ref Ꮡorigin.Value;

internal static uintptr addrOf(ж<point> Ꮡp) {
    return (uintptr)Ꮡp;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object globalˢ = (@string)"global:"u8;
private static readonly object paramˢ = (@string)"param:"u8;
private static readonly object nilParamˢ = (@string)"nil param:"u8;
private static readonly object strideˢ = (@string)"stride:"u8;
private static readonly object roundTripˢ = (@string)"round trip:"u8;
private static readonly object arithmeticˢ = (@string)"arithmetic:"u8;
private static readonly object derefOperandˢ = (@string)"deref operand:"u8;
private static readonly object writtenˢ = (@string)"written:"u8;

internal static void Main() {
    var g1 = (uintptr)Ꮡorigin;
    var g2 = (uintptr)Ꮡorigin;
    fmt.Println(globalˢ, g1 != 0, g1 == g2);
    ref var link = ref heap<ж<point>>(out var Ꮡlink);
    link = Ꮡorigin;
    fmt.Println(paramˢ, addrOf(link) == g1);
    ж<point> missing = default!;
    fmt.Println(nilParamˢ, addrOf(missing) == 0);
    ref var buf = ref heap(new array<int32>(4), out var Ꮡbuf);
    var (e0, e2) = (Ꮡbuf.at<int32>(0), Ꮡbuf.at<int32>(2));
    var a0 = (uintptr)e0;
    var a2 = (uintptr)e2;
    fmt.Println(strideˢ, a0 != 0, a2 - a0 == 2 * /* unsafe.Sizeof(buf[0]) */ (uintptr)4);
    @unsafe.Pointer up = @unsafe.Pointer.FromPinnedBox(e0);
    fmt.Println(roundTripˢ, (uintptr)up == a0);
    var shifted = (uintptr)(@unsafe.Pointer)((uintptr)e0 + 2 * /* unsafe.Sizeof(buf[0]) */ (uintptr)4);
    fmt.Println(arithmeticˢ, shifted == a2);
    var pp = Ꮡlink;
    fmt.Println(derefOperandˢ, (uintptr)(pp.ValueSlot) == g1);
    link.Value.y = 7;
    fmt.Println(writtenˢ, (~link).x, (~link).y, addrOf(link) == g1);
}

} // end main_package
