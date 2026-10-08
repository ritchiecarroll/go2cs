namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct point {
    internal nint x, y;
    internal @string a, b;
    internal float64 wide, tall;
}

partial struct mixed {
    public nint X;
    internal nint y;
    internal nint p, q;
    internal @string label;
}

internal static void Main() {
    var p = new point(x: 1, y: 2, a: "h"u8, b: "i"u8, wide: 3.5D, tall: 4.5D);
    fmt.Println(p.x, p.y, p.a, p.b, p.wide, p.tall);
    var m = new mixed(X: 10, y: 20, p: 30, q: 40, label: "tag"u8);
    fmt.Println(m.X, m.y, m.p, m.q, m.label);
}

} // end main_package
