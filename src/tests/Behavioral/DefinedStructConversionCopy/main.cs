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
    internal array<counts> tags = new(2, () => new());
}

partial struct Grid /*grid*/;

internal static void Main() {
    Counts c = new();
    c.vals[1] = 5;
    var b = ((counts)c);
    b.vals[1] = 99;
    fmt.Println("to base:", c.vals[1], b.vals[1]);

    counts x = new();
    x.vals[2] = 7;
    var w = ((Counts)x);
    w.vals[2] = 88;
    fmt.Println("to wrapper:", x.vals[2], w.vals[2]);

    Grid g = new();
    g.cells[1][2] = 3;
    g.tags[1].vals[0] = 4;
    var h = ((grid)g);
    h.cells[1][2] = 30;
    h.tags[1].vals[0] = 40;
    fmt.Println("nested:", g.cells[1][2], g.tags[1].vals[0], h.cells[1][2], h.tags[1].vals[0]);

    var d = c.ΔClone();
    d.vals[1] = 11;
    fmt.Println("assignment:", c.vals[1], d.vals[1]);
}

} // end main_package
