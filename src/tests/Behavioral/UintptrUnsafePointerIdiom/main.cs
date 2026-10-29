namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct point {
    internal int32 x, y;
}

internal static readonly ж<point> Ꮡorigin = new StaticBox<point>(new point(x: 3, y: 4));
internal static ref point origin => ref ((StaticBox<point>)Ꮡorigin).ValueSlot;

internal static uintptr addrOf(ж<point> p) {
    return (uintptr)p;
}

internal static void Main() {
    var g1 = (uintptr)Ꮡorigin;
    var g2 = (uintptr)Ꮡorigin;
    fmt.Println("global:", g1 != 0, g1 == g2);

    ref var link = ref heap<ж<point>>(out var Ꮡlink);
    link = Ꮡorigin;
    fmt.Println("param:", addrOf(link) == g1);

    ж<point> missing = default;
    fmt.Println("nil param:", addrOf(missing) == 0);

    ref var buf = ref heap(new array<int32>(4), out var Ꮡbuf);
    var (e0, e2) = (Ꮡbuf.at<int32>(0), Ꮡbuf.at<int32>(2));
    var a0 = (uintptr)e0;
    var a2 = (uintptr)e2;
    fmt.Println("stride:", a0 != 0, a2 - a0 == 2 * /* unsafe.Sizeof(buf[0]) */ (uintptr)4);

    @unsafe.Pointer up = @unsafe.Pointer.FromPinnedBox(e0);
    fmt.Println("round trip:", (uintptr)up == a0);

    var shifted = (uintptr)(@unsafe.Pointer)((uintptr)e0 + 2 * /* unsafe.Sizeof(buf[0]) */ (uintptr)4);
    fmt.Println("arithmetic:", shifted == a2);

    var pp = Ꮡlink;
    fmt.Println("deref operand:", (uintptr)(pp.ValueSlot) == g1);

    link.y = 7;
    fmt.Println("written:", link.x, link.y, addrOf(link) == g1);
}

} // end main_package
