namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static void bump(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    foreach (var (i, _) in xs) {
        xs[i] += 10;
    }
}

internal static void forward(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    bump(xs.ꓸꓸꓸ);
}

internal static nint sum(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    nint t = 0;
    foreach (var (_, x) in xs) {
        t += x;
    }
    return t;
}

internal static slice<nint> gather(slice<nint> dst, params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    return appendꓸꓸꓸ(dst, xs);
}

internal static nint fill(slice<nint> dst, params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    return copy(dst, xs);
}

internal static @string describe(@string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return fmt.Sprintf(format, args.ꓸꓸꓸ);
}

internal static slice<nint> into(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    return append(xs, (nint)(4));
}

internal static nint copyInto(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    return copy(xs, new nint[]{7, 7}.slice());
}

internal static slice<nint> keep(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    return xs;
}

internal static Func<nint> capture(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    var xsʗ1 = xs;
    return () => sum(xsʗ1.ꓸꓸꓸ);
}

internal static void deferred(ж<nint> Ꮡout, params ꓸꓸꓸnint xsʗp) {
    GoFrame ᒐ = default;
    try {
        var xs = xsʗp.slice();

        var xsʗ1 = xs;
        defer(() => {
            Ꮡout.Value = sum(xsʗ1.ꓸꓸꓸ);
        }, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    var a = new nint[]{1, 2, 3}.slice();
    forward(a.ꓸꓸꓸ);
    fmt.Println("forward wrote through:", a);

    fmt.Println("sum:", sum(a.ꓸꓸꓸ), sum(), sum(5));
    fmt.Println("gather:", gather(new nint[]{0}.slice(), 1, 2), gather(default!, a.ꓸꓸꓸ));

    var d = new slice<nint>(2);
    nint n = fill(d, 8, 9, 10);
    fmt.Println("fill:", n, d);

    fmt.Println("describe:", describe("%d-%s-%v", (nint)(1), "two", 3.5D));

    fmt.Println("into:", into(1, 2));
    fmt.Println("copyInto:", copyInto(1, 2, 3));
    var k = keep(5, 6);
    k[0] = 100;
    fmt.Println("keep:", k);
    var f = capture(1, 2);
    fmt.Println("capture:", f());
    ref var total = ref heap(new nint(), out var Ꮡtotal);
    deferred(Ꮡtotal, 1, 2);
    fmt.Println("deferred:", total);
}

} // end main_package
