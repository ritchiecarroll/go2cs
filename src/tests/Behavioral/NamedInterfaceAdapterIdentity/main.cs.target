namespace go;

using fmt = fmt_package;
using identlib = NamedInterfaceAdapterIdentity.identlib_package;
using NamedInterfaceAdapterIdentity;

partial class main_package {

partial struct mark {
    internal nint n;
}

internal static @string Greet(this mark m) {
    return fmt.Sprintf("mark%d"u8, m.n);
}

partial struct loud {
    internal nint n;
}

internal static @string Greet(this ref loud l) {
    return fmt.Sprintf("loud%d"u8, l.n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object valueˢ = (@string)"value"u8;
private static readonly object pointerˢ = (@string)"pointer"u8;

internal static void Main() {
    var v = new mark(n: 1);
    var (g, ok) = identlib.TryGreet(v);
    fmt.Println(valueˢ, ok);
    fmt.Println(identlib.Describe(g, v));
    var p = Ꮡ(new loud(n: 2));
    var (g2, ok2) = identlib.TryGreet(p.OrTypedNil());
    fmt.Println(pointerˢ, ok2);
    fmt.Println(identlib.Describe(g2, p.OrTypedNil()));
}

} // end main_package
