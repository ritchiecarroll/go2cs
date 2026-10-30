namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using @unsafe = unsafe_package;
using System.Runtime.InteropServices;

partial class main_package {

public partial struct nocopy {
}

[StructLayout(LayoutKind.Explicit, Size = 4)] partial struct Counter {
    [FieldOffset(0)] internal readonly nocopy _;
    [FieldOffset(0)] internal int32 v;
}

[StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Wide {
    [FieldOffset(0)] internal readonly nocopy _;
    [FieldOffset(0)] internal readonly nocopy __;
    [FieldOffset(0)] internal int64 v;
}

partial struct Plain {
    internal int32 a;
    internal int64 b;
}

partial struct Managed {
    internal nocopy _;
    internal @string s;
}

internal static void Main() {
    fmt.Println("Counter size:", /* unsafe.Sizeof(Counter{}) */ (uintptr)4, "v offset:", /* unsafe.Offsetof(Counter{}.v) */ (uintptr)0);
    fmt.Println("Wide size:", /* unsafe.Sizeof(Wide{}) */ (uintptr)8, "v offset:", /* unsafe.Offsetof(Wide{}.v) */ (uintptr)0);
    fmt.Println("Plain size:", /* unsafe.Sizeof(Plain{}) */ (uintptr)16, "b offset:", /* unsafe.Offsetof(Plain{}.b) */ (uintptr)8);

    ref var raw = ref heap(new int32(), out var Ꮡraw);
    raw = 7;
    var view = Ꮡraw.Reinterpret<int32, Counter>();
    fmt.Println("view reads:", view.v);

    view.v = 42;
    fmt.Println("write through view reaches the original:", raw);

    var c = new Counter(v: 3);
    var w = new Wide(v: 4);
    var p = new Plain(a: 1, b: 2);
    var m = new Managed(s: "managed");
    fmt.Println(c.v, w.v, p.a, p.b, m.s);

    c = new Counter();
    fmt.Println("cleared:", c.v);

    namedZeroSizeWrites();
}

[StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Carrier {
    [FieldOffset(0)] public readonly nocopy Z;
    [FieldOffset(0)] public uint64 V;
}

partial struct Outer {
    public Carrier C;
}

internal static UntypedInt pattern => 0x0102030405060708;

internal static nint sideCalls;

internal static nocopy side() {
    sideCalls++;
    return new nocopy();
}

internal static void namedZeroSizeWrites() {
    ref var x = ref heap(new Carrier(), out var Ꮡx);
    x.V = pattern;
    var rx = reflect.ValueOf(Ꮡx).Elem();

    rx.Field(0).Set(reflect.ValueOf(new nocopy()));
    fmt.Printf("after reflect Set: %#x\n"u8, x.V);

    x.V = pattern;
    var rp = rx.Field(0).Addr().Interface()._<ж<nocopy>>();
    rp.Value = new nocopy();
    fmt.Printf("after write through reflect's pointer: %#x\n"u8, x.V);

    fmt.Println("reflect Addr == &x.Z:", (uintptr)rx.Field(0).Addr().UnsafePointer() == @unsafe.Pointer.FromPinnedBox(Ꮡx.ᏑZ));

    x.V = pattern;
    var p = Ꮡx;
    var arr = new Carrier[]{new(V: pattern), new(V: pattern)}.array();
    var o = new Outer(C: new Carrier(V: pattern));
    var s = slice<Carrier>([new(V: pattern), new(V: pattern)]);

    Carrier.ᏑZ(ref x) = side();
    Carrier.ᏑZ(ref p.Value) = side();
    Carrier.ᏑZ(ref arr[1]) = side();
    Carrier.ᏑZ(ref o.C) = side();
    Carrier.ᏑZ(ref s[1]) = side();

    nint n = default;
    (Carrier.ᏑZ(ref x), n) = (side(), 7);

    fmt.Printf("assignments ran %d right sides; n = %d; V: %#x %#x %#x %#x\n"u8, sideCalls, n, x.V, arr[1].V, o.C.V, s[1].V);
}

} // end main_package
