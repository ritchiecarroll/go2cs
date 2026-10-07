namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct buf {
    internal nint n;
}

internal static partial void add(ref nint p, nint d) {
    p += d;
}

internal static void bump(this ж<buf> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    add(ref nonnil(ref b).n, 1);
}

[GoRecv] internal static nint get(this ref buf b) {
    return b.n;
}

[GoType] partial struct tracer {
    internal slice<buf> tabs;
}

internal static ж<tracer> Ꮡtr = new StandardBox<tracer>(default(tracer));
internal static ref tracer tr => ref Ꮡtr.Value;

internal static partial void keep(ж<tracer> Ꮡt) {
    _ = Ꮡt;
}

internal static void Main() {
    keep(Ꮡtr);
    tr.tabs = new slice<buf>(2);
    Ꮡ(tr.tabs, 1).bump();
    Ꮡ(tr.tabs, 1).bump();
    Ꮡ(tr.tabs, 0).bump();
    fmt.Println(tr.tabs[0].get(), tr.tabs[1].get());
}

} // end main_package
