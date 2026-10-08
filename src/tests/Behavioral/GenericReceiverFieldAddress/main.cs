namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Box<T> {
    internal T v;
}

internal static void setT<T>(ref T p, T val) {
    p = val;
}

internal static T getT<T>(ref T p) {
    return p;
}

public static void Set<T>(this ж<Box<T>> Ꮡb, T val) {
    ref var b = ref Ꮡb.DerefOrNull();

    setT(ref nonnil(ref b).v, val);
}

public static T Get<T>(this ж<Box<T>> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    return getT(ref nonnil(ref b).v);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object intˢ = (@string)"int:"u8;
private static readonly @string helloˢ = "hello"u8;
private static readonly object stringˢ = (@string)"string:"u8;

internal static void Main() {
    ref var bi = ref heap(new Box<nint>(), out var Ꮡbi);
    Ꮡbi.Set(42);
    Ꮡbi.Set(Ꮡbi.Get() + 1);
    fmt.Println(intˢ, Ꮡbi.Get());
    ref var bs = ref heap(new Box<@string>(), out var Ꮡbs);
    Ꮡbs.Set(helloˢ);
    fmt.Println(stringˢ, Ꮡbs.Get());
}

} // end main_package
