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

partial struct p384 {
    internal nint v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string p384ˢ = "p384"u8;

internal static @string label(this ref p384 p) {
    return p384ˢ;
}

internal static ж<p384> combine(this ref p384 p, ж<p384> Ꮡo) {
    ref var o = ref Ꮡo.DerefOrNull();

    return Ꮡ(new p384(v: p.v * o.v));
}

internal static (ж<p384>, error) restore(this ref p384 p, slice<byte> b) {
    return (Ꮡ(new p384(v: (nint)b[0])), default!);
}

internal static ж<p384> newP384() {
    return Ꮡ(new p384(v: 2));
}

partial struct curve<Point>
    where Point : point<Point>
{
    internal @string name;
    internal Func<Point> newPoint;
    internal Point @base;
}

partial interface Curve {
    @string Name();
    @string BaseLabel();
    @string Combined();
    @string Fresh();
}

internal static @string Name<Point>(this ref curve<Point> c)
    where Point : point<Point>
{
    return c.name;
}

internal static @string BaseLabel<Point>(this ref curve<Point> c)
    where Point : point<Point>
{
    return c.@base.label();
}

internal static @string Combined<Point>(this ref curve<Point> c)
    where Point : point<Point>
{
    return c.@base.combine(c.@base).label();
}

internal static @string Fresh<Point>(this ref curve<Point> c)
    where Point : point<Point>
{
    var p = c.newPoint();
    var (r, _) = p.restore(new byte[]{7}.slice());
    return r.label();
}

internal static @string describe<P>(P p)
    where P : point<P>
{
    return p.label() + "/"u8 + p.combine(p).label();
}

internal static @string build<P>(Func<P> newPoint, @string tag)
    where P : point<P>
{
    var p = newPoint();
    var (r, _) = p.restore(new byte[]{9}.slice());
    return tag + ":"u8 + r.label();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string b224ˢ = "b224"u8;
private static readonly @string b384ˢ = "b384"u8;

internal static void Main() {
    Curve c1 = new curveжCurve<p224жpoint>(Ꮡ(new curve<p224жpoint>(name: "c224"u8, newPoint: () => newP224(), @base: Ꮡ(new p224(v: 3)))));
    Curve c2 = new curveжCurve<p384жpoint>(Ꮡ(new curve<p384жpoint>(name: "c384"u8, newPoint: () => newP384(), @base: Ꮡ(new p384(v: 5)))));
    fmt.Println(describe<p224жpoint>(Ꮡ(new p224(v: 4))), describe<p384жpoint>(Ꮡ(new p384(v: 6))));
    fmt.Println(build<p224жpoint>(() => newP224(), b224ˢ), build<p384жpoint>(() => newP384(), b384ˢ));
    fmt.Println(c1.Name(), c1.BaseLabel(), c1.Combined(), c1.Fresh());
    fmt.Println(c2.Name(), c2.BaseLabel(), c2.Combined(), c2.Fresh());
    foreach (var (_, c) in new Curve[]{c1, c2}.slice()) {
        fmt.Println(c.Name(), (@string)"->"u8, c.BaseLabel(), (@string)"->"u8, c.Combined(), (@string)"->"u8, c.Fresh());
    }
}

} // end main_package
