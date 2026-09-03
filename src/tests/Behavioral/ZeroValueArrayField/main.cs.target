namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct holder {
    internal @string name;
    internal array<nint> tbl = new(8);
    internal slice<nint> tail;
}

internal static ж<holder> makeHolder(@string name) {
    var h = Ꮡ(new holder(
        name: name,
        tail: new slice<nint>(2)
    ));
    foreach (var (i, _) in (~h).tbl) {
        h.Value.tbl[i] = len(name);
    }
    h.Value.tbl[3] = 42;
    return h;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abcˢ = "abc"u8;

internal static void Main() {
    var h = makeHolder(abcˢ);
    nint sum = 0;
    foreach (var (i, v) in (~h).tbl.ΔRangeSnapshot()) {
        sum += i * v;
    }
    fmt.Println((~h).name, len((~h).tbl), sum, (~h).tbl[3], len((~h).tail));
    var src = new nint[]{1, 2, 3, 4, 5, 6, 7, 8}.array();
    var g = new holder(name: "xyz"u8, tbl: src.Clone());
    fmt.Println(g.tbl[0], g.tbl[7], len(g.tbl));
}

} // end main_package
