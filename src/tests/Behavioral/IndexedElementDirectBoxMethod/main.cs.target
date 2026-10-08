namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void inc(this ж<counter> Ꮡc) {
    var p = Ꮡc.of(counter.Ꮡn);
    p.Value++;
}

internal static nint get(this ref counter c) {
    return c.n;
}

partial struct holder {
    internal array<counter> arr = new(3);
}

internal static void bump(ж<holder> Ꮡh, nint i) {
    Ꮡh.at(holder.Ꮡarr, i).inc();
}

internal static void Main() {
    var h = Ꮡ(new holder(arr: new counter[]{new(), new(), new()}.array()));
    bump(h, 0);
    bump(h, 0);
    bump(h, 1);
    bump(h, 1);
    bump(h, 1);
    fmt.Println((~h).arr[0].get(), (~h).arr[1].get(), (~h).arr[2].get());
}

} // end main_package
