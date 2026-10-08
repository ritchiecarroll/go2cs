namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static nint start(this ref counter c) {
    return c.n;
}

internal static nint shadowedForInit() {
    var b = Ꮡ(new counter(n: 100));
    nint total = b.Value.n;
    {
        var bΔ1 = Ꮡ(new counter(n: 5));
        for ((nint i, nint x) = (0, bΔ1.start()); i < 3; i++) {
            total += x + (~bΔ1).n + i;
        }
    }
    return total;
}

internal static void Main() {
    fmt.Println(shadowedForInit());
}

} // end main_package
