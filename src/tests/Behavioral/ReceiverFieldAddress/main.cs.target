namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Counter {
    internal int32 n;
}

internal static int32 bump(ref int32 p, int32 delta) {
    p += delta;
    return p;
}

public static int32 Add(this ж<Counter> Ꮡc, int32 delta) {
    ref var c = ref Ꮡc.DerefOrNull();

    return bump(ref nonnil(ref c).n, delta);
}

public static void Set(this ж<Counter> Ꮡc, int32 v) {
    (Ꮡc.Ꮡn).Value = v;
}

public static int32 Get(this ref Counter c) {
    return c.n;
}

internal static void Main() {
    ref var c = ref heap(new Counter(), out var Ꮡc);
    Ꮡc.Set(100);
    fmt.Println("after Set:", c.Get());
    fmt.Println("Add 10:", Ꮡc.Add(10));
    fmt.Println("Add 5:", Ꮡc.Add(5));
    fmt.Println("final:", c.Get());
}

} // end main_package
