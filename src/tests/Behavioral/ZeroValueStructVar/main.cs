namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct holder {
    internal @string name;
    internal array<nint> tbl = new(8);
    internal slice<nint> tail;
}

[GoType] partial struct wrapper {
    internal nint id;
    internal holder h;
}

[GoType] partial struct point {
    internal nint x, y;
}

internal static void Main() {
    holder z = new();
    fmt.Println(len(z.name), len(z.tbl), len(z.tail));
    foreach (var (i, _) in z.tbl) {
        z.tbl[i] = i * i;
    }
    nint sum = 0;
    foreach (var (_, v) in z.tbl.ΔRangeSnapshot()) {
        sum += v;
    }
    fmt.Println(sum, z.tbl[7]);
    wrapper w = new();
    w.h.tbl[3] = 42;
    fmt.Println(w.id, len(w.h.tbl), w.h.tbl[3]);
    point p = default!;
    p.x = 3;
    fmt.Println(p.x + p.y);
}

} // end main_package
