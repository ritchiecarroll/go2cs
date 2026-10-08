namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct grid /*[4]nint*/;

internal static nint firstTwo(this ref grid b) {
    nint total = 0;
    foreach (var (_, v) in b.Value[..2]) {
        total += v;
    }
    return total;
}

internal static nint sumParam(ж<grid> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint total = 0;
    foreach (var (_, v) in p.Value[..]) {
        total += v;
    }
    return total;
}

internal static void Main() {
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{10, 20, 30, 40, 50}.array();
    var p = Ꮡarr;
    var full = (~p)[..];
    var low = (~p)[2..];
    var high = (~p)[..3];
    var mid = (~p)[1..4];
    var three = (~p).slice(1, 3, 4);
    fmt.Println(full);
    fmt.Println(low);
    fmt.Println(high);
    fmt.Println(mid);
    fmt.Println(three, len(three), cap(three));
    full[0] = 99;
    fmt.Println(arr[0]);
    ref var g = ref heap<grid>(out var Ꮡg);
    g = new grid(new nint[]{2, 4, 6, 8}.array());
    fmt.Println(g.firstTwo(), sumParam(Ꮡg));
}

} // end main_package
