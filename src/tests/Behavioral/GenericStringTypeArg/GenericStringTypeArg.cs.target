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

internal static void Main() {
    Pair<nint, @string> p = default;
    p.a = 5;
    p.b = "hi";
    fmt.Println(p.a, p.b);

    Box<@string> b = default;
    b.v = "boxed";
    fmt.Println(b.v);

    Pair<@string, @string> sp = default;
    sp.a = "x";
    sp.b = "y";
    fmt.Println(sp.a, sp.b);

    Pair<nint, Box<@string>> nested = default;
    nested.a = 9;
    nested.b.v = "deep";
    fmt.Println(nested.a, nested.b.v);
}

} // end main_package
