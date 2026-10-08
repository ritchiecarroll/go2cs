namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct inner {
    internal nint a, b;
}

partial struct outer {
    internal inner mid;
    internal nint tag;
}

internal static ж<outer> Ꮡg = new StandardBox<outer>(default(outer));
internal static ref outer g => ref Ꮡg.Value;

internal static partial void keep(ж<outer> Ꮡp) {
    _ = Ꮡp;
}

internal static void mutate() {
    var p = Ꮡg.of(outer.Ꮡmid).of(inner.Ꮡa);
    p.Value = 42;
    var q = Ꮡg.of(outer.Ꮡmid);
    q.Value.b = 7;
}

internal static void Main() {
    keep(Ꮡg);
    mutate();
    fmt.Println(g.mid.a, g.mid.b, g.tag);
}

} // end main_package
