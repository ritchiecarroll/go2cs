namespace go;

using fmt = fmt_package;
using strings = strings_package;
using time = time_package;

partial class main_package {

partial struct point {
    internal nint x, y;
}

partial struct grid {
    internal array<nint> cells = new(3);
    internal @string name;
}

partial struct nested {
    internal point at;
    internal grid board;
    internal slice<@string> tags;
    internal map<@string, bool> seen;
}

partial struct pair<T> {
    internal T a, b;
}

partial struct boxed<T> {
    internal array<T> items = new(2);
    internal T last;
}

partial struct wrapped /*point*/;

partial struct embeds {
    /*embed*/ internal point point;
    /*embed*/ internal grid grid;
    internal @string label;
}

partial struct empty {
}

partial struct IntSlice /*[]nint*/;

partial struct StrMap /*map[@string, nint]*/;

internal static void Main() {
    var p = new point();
    p.x = 3;
    fmt.Println("point:", p, p.x + p.y);

    var g = new grid();
    g.cells[1] = 7;
    fmt.Println("grid:", g.cells, len(g.cells), g.name == ""u8);

    var n = new nested();
    n.board.cells[2] = 9;
    fmt.Println("nested:", n.at, n.board.cells, n.tags == default, n.seen == default);

    var q = new pair<@string>();
    q.a = "a";
    fmt.Printf("pair: %q %q\n"u8, q.a, q.b);

    var bx = new boxed<nint>();
    bx.items[1] = 5;
    fmt.Println("boxed:", bx.items, bx.last);

    var w = new wrapped(new point());
    w.y = 4;
    fmt.Println("wrapped:", w.x, w.y);

    var e = new embeds();
    e.cells[0] = 1;
    e.x = 2;
    fmt.Println("embeds:", e.point, e.grid.cells, e.label == ""u8);

    var z = new empty();
    fmt.Println("empty:", z == new empty());

    var pp = Ꮡ(new point());
    pp.Value.y = 8;
    fmt.Println("pointer:", pp.Value);

    var pg = Ꮡ(new grid());
    pg.Value.cells[0] = 6;
    fmt.Println("pointer grid:", (~pg).cells);

    ref var b = ref heap(new strings.Builder(), out var Ꮡb);

    b = new strings.Builder();
    Ꮡb.WriteString("built");
    fmt.Println("builder:", b.String(), b.Len());

    var t = new time.Time();
    fmt.Println("time zero:", t.IsZero(), t.Year());

    fmt.Println("equal zero values:", new point() == new point(), new grid() == new grid(), p == new point());

    var s = new IntSlice(new nint[]{}.slice());
    fmt.Println("IntSlice{} == nil:", s == default, len(s));

    var m = new StrMap(new map<@string, nint>{});
    m["k"] = 1;
    fmt.Println("StrMap{} == nil:", m == default, len(m));
}

} // end main_package
