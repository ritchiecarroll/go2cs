namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void inc(this ref counter c) {
    c.n++;
}

internal static ж<array<counter>> Ꮡpool = new StandardBox<array<counter>>(new array<counter>(3));
internal static ref array<counter> pool => ref Ꮡpool.Value;

internal static void Main() {
    pool[0].inc();
    pool[0].inc();
    pool[1].inc();
    fmt.Println(pool[0].n, pool[1].n, pool[2].n);
}

} // end main_package
