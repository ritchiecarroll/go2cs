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
    (Ꮡc.of(Counter.Ꮡn)).Value = v;
}

public static int32 Get(this ref Counter c) {
    return c.n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object afterSetˢ = (@string)"after Set:"u8;
private static readonly object add10ˢ = (@string)"Add 10:"u8;
private static readonly object add5ˢ = (@string)"Add 5:"u8;
private static readonly object finalˢ = (@string)"final:"u8;

internal static void Main() {
    ref var c = ref heap(new Counter(), out var Ꮡc);
    Ꮡc.Set(100);
    fmt.Println(afterSetˢ, c.Get());
    fmt.Println(add10ˢ, Ꮡc.Add(10));
    fmt.Println(add5ˢ, Ꮡc.Add(5));
    fmt.Println(finalˢ, c.Get());
}

} // end main_package
