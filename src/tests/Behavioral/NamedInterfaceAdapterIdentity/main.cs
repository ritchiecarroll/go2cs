namespace go;

using fmt = fmt_package;
using identlib = NamedInterfaceAdapterIdentity.identlib_package;
using NamedInterfaceAdapterIdentity;
using static main_package;

partial class main_package {

partial struct mark {
    internal nint n;
}

} partial class mainᴛmark {

internal static @string Greet(this mark m) { return fmt.Sprintf("mark%d"u8, m.n); }

} partial class main_package {

partial struct loud {
    internal nint n;
}

} partial class mainᴛloud {

internal static @string Greet(this ref loud l) { return fmt.Sprintf("loud%d"u8, l.n); }

} partial class main_package {

internal static void main() {
    var v = new mark(n: 1);
    var (g, ok) = identlib.TryGreet(v);
    fmt.Println("value", ok);
    fmt.Println(identlib.Describe(g, v));

    var p = Ꮡ(new loud(n: 2));
    var (g2, ok2) = identlib.TryGreet(p.OrTypedNil());
    fmt.Println("pointer", ok2);
    fmt.Println(identlib.Describe(g2, p.OrTypedNil()));
}

} // end main_package
