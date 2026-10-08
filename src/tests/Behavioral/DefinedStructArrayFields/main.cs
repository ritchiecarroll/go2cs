namespace go;

using fmt = fmt_package;

partial class main_package {

public partial struct counts {
    internal array<nint> vals = new(4);
    internal nint n;
}

partial struct Counts /*counts*/;

public partial struct grid {
    internal array<array<nint>> cells = new(2, () => new(3));
}

partial struct Grid /*grid*/;

partial struct pt {
    public nint X, Y;
}

public partial struct path {
    internal array<pt> pts = new(3);
}

partial struct Path /*path*/;

internal static Counts global = new();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object zeroˢ = (@string)"zero:"u8;
private static readonly object newˢ = (@string)"new:"u8;
private static readonly object literalˢ = (@string)"literal:"u8;
private static readonly object copyˢ = (@string)"copy:"u8;
private static readonly object globalˢ = (@string)"global:"u8;
private static readonly object nestedˢ = (@string)"nested:"u8;
private static readonly object structsˢ = (@string)"structs:"u8;
private static readonly object arrayˢ = (@string)"array:"u8;
private static readonly object fieldˢ = (@string)"field:"u8;
private static readonly object fieldLiteralˢ = (@string)"field literal:"u8;
private static readonly object sliceˢ = (@string)"slice:"u8;

internal static void Main() {
    Counts c = new();
    c.vals[1] = 5;
    fmt.Println(zeroˢ, len(c.vals), c.vals, c.n);
    var p = @new<Counts>();
    p.Value.vals[3] = 7;
    fmt.Println(newˢ, len((~p).vals), (~p).vals);
    var l = new Counts(new counts(n: 1));
    l.vals[2] = 9;
    fmt.Println(literalˢ, len(l.vals), l.vals, l.n);
    var d = c.ΔClone();
    d.vals[0] = 11;
    fmt.Println(copyˢ, c.vals, d.vals);
    global.vals[0] = 3;
    fmt.Println(globalˢ, len(global.vals), global.vals);
    Grid g = new();
    g.cells[1][2] = 6;
    fmt.Println(nestedˢ, len(g.cells), len(g.cells[1]), g.cells);
    Path pa = new();
    pa.pts[2].Y = 8;
    fmt.Println(structsˢ, len(pa.pts), pa.pts);
    array<Counts> arr = new(3, () => new());
    arr[1].vals[1] = 5;
    fmt.Println(arrayˢ, len(arr[1].vals), arr[1].vals, arr[2].vals);
    holder h = new();
    h.c.vals[2] = 4;
    fmt.Println(fieldˢ, len(h.c.vals), h.c.vals, h.id);
    var hl = new holder(id: 1);
    hl.c.vals[3] = 6;
    fmt.Println(fieldLiteralˢ, len(hl.c.vals), hl.c.vals, hl.id);
    var s = new slice<Counts>(2, () => new());
    s[1].vals[3] = 2;
    fmt.Println(sliceˢ, len(s[1].vals), s[1].vals, s[0].vals);
}

partial struct holder {
    internal nint id;
    internal Counts c;
}

} // end main_package
