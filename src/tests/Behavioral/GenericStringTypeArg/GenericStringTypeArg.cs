namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Box<T> {
    internal T v;
}

partial struct Pair<A, B> {
    internal A a;
    internal B b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string boxedˢ = "boxed"u8;
private static readonly @string deepˢ = "deep"u8;

internal static void Main() {
    Pair<nint, @string> p = default!;
    p.a = 5;
    p.b = "hi"u8;
    fmt.Println(p.a, p.b);
    Box<@string> b = default!;
    b.v = boxedˢ;
    fmt.Println(b.v);
    Pair<@string, @string> sp = default!;
    sp.a = "x"u8;
    sp.b = "y"u8;
    fmt.Println(sp.a, sp.b);
    Pair<nint, Box<@string>> nested = default!;
    nested.a = 9;
    nested.b.v = deepˢ;
    fmt.Println(nested.a, nested.b.v);
}

} // end main_package
