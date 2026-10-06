namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using System.Runtime.CompilerServices;
using sync;

partial class main_package {

[GoType] partial struct controller {
    internal atomic.Int64 total;
}

internal static ж<controller> Ꮡctrl = new StandardBox<controller>(default(controller));
internal static ref controller ctrl => ref Ꮡctrl.Value;

internal static partial void keep(ж<controller> Ꮡc) {
    _ = Ꮡc;
}

internal static void bump() {
    Ꮡctrl.of(controller.Ꮡtotal).Add(5);
}

internal static void Main() {
    keep(Ꮡctrl);
    bump();
    bump();
    fmt.Println(Ꮡctrl.of(controller.Ꮡtotal).Load());
}

} // end main_package
