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

internal static int32 add(this ж<Counter> Ꮡc, int32 delta) {
    ref var c = ref Ꮡc.DerefOrNull();

    return bump(ref nonnil(ref c).n, delta);
}

public static void Set(this ж<Counter> Ꮡc, int32 v) {
    (Ꮡc.of(Counter.Ꮡn)).Value = v;
}

public static int32 Get(this ref Counter c) {
    return c.n;
}

partial struct Flag {
    internal Counter c;
    internal @string label;
}

public static int32 Incr(this ж<Flag> Ꮡf) {
    return Ꮡf.of(Flag.Ꮡc).Add(1);
}

public static int32 AddN(this ж<Flag> Ꮡf, int32 d) {
    return Ꮡf.of(Flag.Ꮡc).Add(d);
}

public static void Reset(this ж<Flag> Ꮡf, int32 v) {
    Ꮡf.of(Flag.Ꮡc).Set(v);
}

public static int32 Value(this ref Flag f) {
    return f.c.Get();
}

public static @string Label(this ref Flag f) {
    return f.label;
}

internal static int32 applyTwice(Func<int32, int32> f, int32 d) {
    f(d);
    return f(d);
}

internal static int32 readVia(Func<int32> get) {
    return get();
}

public static int32 AddTwice(this ж<Counter> Ꮡc, int32 d) {
    return applyTwice(Ꮡc.Add, d);
}

public static int32 AddViaValue(this ж<Flag> Ꮡf, int32 d) {
    return applyTwice(Ꮡf.of(Flag.Ꮡc).Add, d);
}

public static int32 ReadViaValue(this ж<Flag> Ꮡf) {
    return readVia(Ꮡf.of(Flag.Ꮡc).Get);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string hitsˢ = "hits"u8;
private static readonly object startˢ = (@string)"start:"u8;
private static readonly object incrˢ = (@string)"Incr:"u8;
private static readonly object addN5ˢ = (@string)"AddN 5:"u8;
private static readonly object finalˢ = (@string)"final:"u8;
private static readonly object addTwice3ˢ = (@string)"AddTwice 3:"u8;
private static readonly object addViaValue2ˢ = (@string)"AddViaValue 2:"u8;
private static readonly object readViaValueˢ = (@string)"ReadViaValue:"u8;
private static readonly object localValueˢ = (@string)"local value:"u8;
private static readonly object caseTwinˢ = (@string)"case twin:"u8;

internal static void Main() {
    ref var fl = ref heap(new Flag(), out var Ꮡfl);
    fl.label = hitsˢ;
    Ꮡfl.Reset(10);
    fmt.Println(fl.Label(), startˢ, fl.Value());
    fmt.Println(incrˢ, Ꮡfl.Incr());
    fmt.Println(incrˢ, Ꮡfl.Incr());
    fmt.Println(addN5ˢ, Ꮡfl.AddN(5));
    fmt.Println(finalˢ, fl.Value());
    fmt.Println(addTwice3ˢ, Ꮡfl.of(Flag.Ꮡc).AddTwice(3));
    fmt.Println(addViaValue2ˢ, Ꮡfl.AddViaValue(2));
    fmt.Println(readViaValueˢ, Ꮡfl.ReadViaValue());
    fmt.Println(localValueˢ, applyTwice(Ꮡfl.of(Flag.Ꮡc).Add, 1));
    fmt.Println(caseTwinˢ, Ꮡfl.of(Flag.Ꮡc).add(1));
}

} // end main_package
