namespace go;

using fmt = fmt_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static slice<nint> keep(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    return xs;
}

partial struct box {
    internal slice<nint> items;
}

internal static void set(this ref box b, params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    b.items = xs;
}

internal static void copyInto(ref slice<nint> dst, params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    dst = xs;
}

internal static Func<nint> capture(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    var xsʗ1 = xs;
    return () => xsʗ1[0];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object keepˢ = (@string)"keep"u8;
private static readonly object storeˢ = (@string)"store"u8;
private static readonly object copyIntoˢ = (@string)"copyInto"u8;
private static readonly object captureˢ = (@string)"capture"u8;
private static readonly object appendˢ = (@string)"append"u8;
private static readonly object literalˢ = (@string)"literal"u8;

internal static void Main() {
    var xs = new nint[]{7, 2}.slice();
    var ys = keep(xs.ꓸꓸꓸ);
    ys[1] = 8;
    fmt.Println(keepˢ, xs);
    var b = Ꮡ(new box(nil));
    b.set(xs.ꓸꓸꓸ);
    xs[0] = 1;
    fmt.Println(storeˢ, (~b).items);
    ref var d = ref heap<slice<nint>>(out var Ꮡd);
    copyInto(ref d, xs.ꓸꓸꓸ);
    d[1] = 3;
    fmt.Println(copyIntoˢ, xs);
    var f = capture(xs.ꓸꓸꓸ);
    xs[0] = 42;
    fmt.Println(captureˢ, f());
    var zs = new slice<nint>(1, 4);
    var w = keep(zs.ꓸꓸꓸ);
    w = append(w, (nint)(5));
    fmt.Println(appendˢ, zs[..2], len(w), cap(w));
    var lit = keep(1, 2);
    lit[0] = 9;
    fmt.Println(literalˢ, lit);
}

} // end main_package
