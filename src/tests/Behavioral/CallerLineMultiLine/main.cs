namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

internal static bool all = os.Getenv("GO2CS_CALLER_LINE_ALL_SHAPES") == "1"u8;

internal static void show(bool exactInRelease, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    if (all || exactInRelease) {
        fmt.Println(args.ꓸꓸꓸ);
    }
}

internal static partial nint line() {
    var (_, _, l, _) = runtime.Caller(1);
    return l;
}

partial struct chain {
    internal slice<nint> lines;
}

internal static partial ж<chain> Add(this ж<chain> Ꮡc, nint n) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (_, _, l, _) = runtime.Caller(1);
    c.lines = append(c.lines, l);
    return Ꮡc;
}

internal static partial void Done(this ref chain c, @string label) {
    var (_, _, l, _) = runtime.Caller(1);
    show(false, label, c.lines, l);
    show(true, label + "-terminal", l);
}

partial struct impl {
}

internal static partial nint Line(this impl _Δp0) {
    var (_, _, l, _) = runtime.Caller(1);
    return l;
}

internal static slice<nint> three(nint a, nint b, nint c) {
    return slice<nint>([a, b, c]);
}

internal static slice<nint> recorded;

internal static bool rec(nint l) {
    recorded = append(recorded, l);
    return true;
}

internal static slice<nint> pkgSlice = slice<nint>([line(),
    line(), line(),
    line()
]);

internal static nint pkgSum = line() + line() + line();


partial struct pkgStructsᴛ1 /*dyn*/ {
    internal nint a, b;
}
internal static slice<pkgStructsᴛ1> pkgStructs = slice<pkgStructsᴛ1>([
    new(line(), line()),
    new(
        line(),
        line()
    )
]);

internal static partial slice<nint> ret() {
    return three(line(),
        line(),
        line());
}

internal static void Main() {
    show(false, "pkgSlice", pkgSlice);
    show(false, "pkgSum", pkgSum);
    show(false, "pkgStructs", pkgStructs);

    (Ꮡ(new chain())).Add(1).Add(2).Add(3).Done("S1");

    show(false, "S2", three(line(),
        line(),
        line()));

    nint sum = line() + line() + line();
    show(false, "S3", sum);

    var f = line;
    show(false, "S4", f(),
        f());

    impl im = default;
    show(false, "S5", im.Line(),
        im.Line());

    if (rec(line()) && rec(line())) {
        show(true, "S6", recorded);
    }

    var local = slice<nint>([line(),
        line()
    ]);
    show(false, "S7", local);

    show(false, "S8", ret());
}

} // end main_package
