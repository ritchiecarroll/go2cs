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
    ref var arr = ref heap<array<nint>>(new nint[]{10, 20, 30, 40, 50}.array(), out var Ꮡarr);
    var p = Ꮡarr;

    var full = p.slice();
    var low = p.slice(2);
    var high = p.slice(0, 3);
    var mid = p.slice(1, 4);
    var three = p.slice(1, 3, 4);

    fmt.Println(full);
    fmt.Println(low);
    fmt.Println(high);
    fmt.Println(mid);
    fmt.Println(three, len(three), cap(three));

    full[0] = 99;
    fmt.Println(arr[0]);

    ref var g = ref heap<grid>(new grid(new nint[]{2, 4, 6, 8}.array()), out var Ꮡg);
    fmt.Println(g.firstTwo(), sumParam(Ꮡg));

    rangeIndexOnly();
}

internal static void rangeIndexOnly() {
    ж<array<nint>> nilp = default;
    foreach (var i in range(nilp, 3)) {
        fmt.Print(i, " ");
    }
    fmt.Println();

    ж<grid> nilg = default;
    nint count = 0;
    foreach (var _ᴛ1 in range(nilg, 4)) {
        count++;
    }
    fmt.Println("nil *grid ranges", count);

    var p = Ꮡ(new @string[]{"a", "b"}.array());
    foreach (var i in range(p, 2)) {
        p.Value[i] += "!"u8;
    }
    fmt.Println(p.Value[0], p.Value[1]);

    grid g = default;
    g.fill();
    fmt.Println("receiver ranges", g);
}

internal static void fill(this ref grid b) {
    foreach (var i in range(4)) {
        b.Value[i] = i * 10;
    }
}

} // end main_package
