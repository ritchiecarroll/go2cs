namespace go;

using fmt = fmt_package;
using ꓸꓸꓸbox = Span<main_package.box>;
using ꓸꓸꓸgreatestEnd_nodes = Span<main_package.greatestEnd_nodes>;
using ꓸꓸꓸsumPoints_points = Span<main_package.sumPoints_points>;

partial class main_package {

partial struct span {
    internal nint end;
}

internal static nint End(this span s) { return s.end; }

partial struct box {
    internal nint n;
}

internal static nint boxes(params ꓸꓸꓸbox bsʗp) {
    var bs = bsʗp.sslice();

    nint total = 0;

    foreach (var (_, b) in range(bs)) {
        total += b.n;
    }

    return total;
}

internal partial interface greatestEnd_nodes /*dyn*/ {
    nint End();
}

internal static nint greatestEnd(nint end, params ꓸꓸꓸgreatestEnd_nodes nodesʗp) {
    var nodes = nodesʗp.sslice();

    foreach (var (_, n) in range(nodes)) {
        if (n.End() > end) {
            end = n.End();
        }
    }

    return end;
}

internal partial struct sumPoints_points /*dyn*/ {
    internal nint x, y;
}

internal static nint sumPoints(params ꓸꓸꓸsumPoints_points pointsʗp) {
    var points = pointsʗp.sslice();

    nint total = 0;

    foreach (var (_, p) in range(points)) {
        total += p.x * 10 + p.y;
    }

    return total;
}

internal static void Main() {
    fmt.Println(greatestEnd(1, new span(4), new span(2)), greatestEnd(7));

    var nodes = slice<greatestEnd_nodes>([new span(5), new span(9), new span(3)]);
    fmt.Println(greatestEnd(0, nodes.ꓸꓸꓸ), leastEnd(100, nodes.ꓸꓸꓸ));

    fmt.Println(sumPoints(new sumPoints_points(1, 2), new sumPoints_points(3, 4)), sumPoints());
    fmt.Println(boxes(new box(1), new box(2)), boxes());
}

} // end main_package
