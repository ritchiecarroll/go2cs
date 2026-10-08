namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface point<T> {
    @string label();
    T combine(T _);
    (T, error) restore(slice<byte> _);
}

partial struct p224 {
    internal nint v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string p224ˢ = "p224"u8;

internal static @string label(this ref p224 p) {
    return p224ˢ;
}

internal static ж<p224> combine(this ref p224 p, ж<p224> Ꮡo) {
    ref var o = ref Ꮡo.DerefOrNull();

    return Ꮡ(new p224(v: p.v + o.v));
}

internal static (ж<p224>, error) restore(this ref p224 p, slice<byte> b) {
    return (Ꮡ(new p224(v: (nint)b[0])), default!);
}

internal static ж<p224> newP224() {
    return Ꮡ(new p224(v: 1));
}

partial struct curve<Point>
    where Point : point<Point>
{
    internal @string name;
    internal Func<Point> newPoint;
    internal Point seed;
}

internal static @string Tag<Point>(this ref curve<Point> c)
    where Point : point<Point>
{
    return c.name;
}

internal static @string SeedLabel<Point>(this ref curve<Point> c)
    where Point : point<Point>
{
    return c.seed.label();
}

internal static @string LabelOf<Point>(this ref curve<Point> c, Point p)
    where Point : point<Point>
{
    return p.label();
}

internal static @string Fresh<Point>(this ref curve<Point> c)
    where Point : point<Point>
{
    var p = c.newPoint();
    var (r, _) = p.restore(new byte[]{7}.slice());
    return r.label();
}

partial interface Named {
    @string Tag();
    @string SeedLabel();
    @string Fresh();
}

partial struct p224Curve {
    internal partial ref curve<p224жpoint> curve { get; }
}

internal static void Main() {
    var pc = Ꮡ(new p224Curve(new curve<p224жpoint>(name: "p224c"u8, newPoint: () => newP224(), seed: Ꮡ(new p224(v: 3)))));
    fmt.Println((~pc).name);
    fmt.Println(pc.of(p224Curve.Ꮡcurve).LabelOf((~pc).seed));
    Named n = new p224CurveжNamed(pc);
    fmt.Println(n.Tag());
    fmt.Println(n.SeedLabel());
    fmt.Println(n.Fresh());
}

} // end main_package
