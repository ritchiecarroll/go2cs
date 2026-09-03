global using alias = go.main_package.Inner;
global using words = go.array<ulong>;

namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Inner {
    public nint V;
    public nint W;
}

internal static alias keyed() {
    return new alias(V: 7, W: 3);
}

internal static alias empty() {
    return new alias();
}

internal static alias positional() {
    return new alias(1, 2);
}

internal static uint64 sum([GoArrayDims(4)] words w) {
    w = w.Clone();

    uint64 s = default!;
    for (nint i = 0; i < len(w); i++) {
        s += w[i];
    }
    return s;
}

internal static uint64 rangeSum([GoArrayDims(4)] words w) {
    w = w.Clone();

    uint64 s = default!;
    foreach (var (_, e) in w.ΔRangeSnapshot()) {
        s += e;
    }
    return s;
}

internal static words gw = new(4);

internal static void Main() {
    var a = keyed();
    var b = empty();
    var c = positional();
    fmt.Println(a.V, a.W, b.V, c.V, c.W);
    var w = new uint64[]{10, 20, 30, 40}.array();
    fmt.Println(sum(w), len(w));
    var d = new uint64[]{10, 20, 30, 40}.array();
    fmt.Println(sum(d), rangeSum(d));
    words z = new(4);
    for (nint i = 0; i < len(z); i++) {
        z[i] = (uint64)(i + 1);
    }
    fmt.Println(rangeSum(z), z[3]);
    gw[0] = 99;
    fmt.Println(gw[0], len(gw));
}

} // end main_package
