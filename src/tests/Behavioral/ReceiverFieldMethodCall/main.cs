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
    (Ꮡc.Ꮡn).Value = v;
}

public static int32 Get(this ref Counter c) {
    return c.n;
}

partial struct Flag {
    internal Counter c;
    internal @string label;
}

public static int32 Incr(this ж<Flag> Ꮡf) {
    return Ꮡf.Ꮡc.Add(1);
}

public static int32 AddN(this ж<Flag> Ꮡf, int32 d) {
    return Ꮡf.Ꮡc.Add(d);
}

public static void Reset(this ж<Flag> Ꮡf, int32 v) {
    Ꮡf.Ꮡc.Set(v);
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
    return applyTwice(Ꮡf.Ꮡc.Add, d);
}

public static int32 ReadViaValue(this ж<Flag> Ꮡf) {
    return readVia(Ꮡf.Ꮡc.Get);
}

internal static void Main() {
    ref var fl = ref heap(new Flag(), out var Ꮡfl);
    fl.label = "hits";

    Ꮡfl.Reset(10);
    fmt.Println(fl.Label(), "start:", fl.Value());
    fmt.Println("Incr:", Ꮡfl.Incr());
    fmt.Println("Incr:", Ꮡfl.Incr());
    fmt.Println("AddN 5:", Ꮡfl.AddN(5));
    fmt.Println("final:", fl.Value());
    fmt.Println("AddTwice 3:", Ꮡfl.Ꮡc.AddTwice(3));
    fmt.Println("AddViaValue 2:", Ꮡfl.AddViaValue(2));
    fmt.Println("ReadViaValue:", Ꮡfl.ReadViaValue());
    fmt.Println("local value:", applyTwice(Ꮡfl.Ꮡc.Add, 1));
    fmt.Println("case twin:", Ꮡfl.Ꮡc.add(1));
}

} // end main_package
