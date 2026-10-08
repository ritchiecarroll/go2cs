namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial @unsafe.Pointer add(@unsafe.Pointer p, uintptr x) {
    return (@unsafe.Pointer)((uintptr)p + x);
}

partial struct nih {
    internal uint32 a, b;
}

internal static ж<nih> add(this ж<nih> Ꮡp, uintptr bytes) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (bytes == 0) {
        return Ꮡp;
    }
    return (ж<nih>)(uintptr)((@unsafe.Pointer)((uintptr)(uintptr)@unsafe.Pointer.FromRef(ref p) + bytes));
}

internal static partial uint32 step(ж<uint32> Ꮡv) {
    var q = (ж<uint32>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(Ꮡv), /* unsafe.Sizeof(uint32(0)) */ (uintptr)4));
    return q.Value;
}

partial struct holder {
    internal uint32 x, y;
}

internal static uint32 second(this ж<holder> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    return ~(ж<uint32>)(uintptr)(add((uintptr)@unsafe.Pointer.FromRef(ref h), /* unsafe.Sizeof(uint32(0)) */ (uintptr)4));
}

internal static void Main() {
    ref var vals = ref heap<array<uint32>>(out var Ꮡvals);
    vals = new uint32[]{10, 20, 30, 40}.array();
    fmt.Println(step(Ꮡvals.at<uint32>(0)));
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡvals.at<uint32>(1));
    fmt.Println(~(ж<uint32>)(uintptr)(add(p, 8)));
    ref var n = ref heap<nih>(out var Ꮡn);
    n = new nih(a: 7, b: 9);
    var m = Ꮡn.add(0);
    fmt.Println((~m).a, (~m).b);
    var pb = Ꮡn.add(4).Reinterpret<nih, uint32>();
    fmt.Println(pb.Value);
    ref var hh = ref heap<holder>(out var Ꮡhh);
    hh = new holder(x: 3, y: 5);
    fmt.Println(Ꮡhh.second());
}

} // end main_package
