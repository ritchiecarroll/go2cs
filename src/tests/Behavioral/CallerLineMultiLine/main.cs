namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

internal static bool all = os.Getenv("GO2CS_CALLER_LINE_ALL_SHAPES"u8) == "1"u8;

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
    return new nint[]{a, b, c}.slice();
}

internal static slice<nint> recorded;

internal static bool rec(nint l) {
    recorded = append(recorded, l);
    return true;
}

internal static slice<nint> pkgSlice = new nint[]{line(),
    line(), line(),
    line()
}.slice();

internal static nint pkgSum = line() + line() + line();


partial struct pkgStructsᴛ1 /*dyn*/ {
    internal nint a, b;
}
internal static slice<pkgStructsᴛ1> pkgStructs = new pkgStructsᴛ1[]{
    new(line(), line()),
    new(
        line(),
        line()
    )
}.slice();

internal static partial slice<nint> ret() {
    return three(line(),
        line(),
        line());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pkgSliceˢ = (@string)"pkgSlice"u8;
private static readonly object pkgSumˢ = (@string)"pkgSum"u8;
private static readonly object pkgStructsˢ = (@string)"pkgStructs"u8;

internal static void Main() {
    show(false, pkgSliceˢ, pkgSlice);
    show(false, pkgSumˢ, pkgSum);
    show(false, pkgStructsˢ, pkgStructs);
    (Ꮡ(new chain(nil))).Add(1).Add(2).Add(3).Done("S1"u8);
    show(false, (@string)"S2"u8, three(line(),
        line(),
        line()));
    nint sum = line() + line() + line();
    show(false, (@string)"S3"u8, sum);
    var f = line;
    show(false, (@string)"S4"u8, f(),
        f());
    impl im = default!;
    show(false, (@string)"S5"u8, im.Line(),
        im.Line());
    if (rec(line()) && rec(line())) {
        show(true, (@string)"S6"u8, recorded);
    }
    var local = new nint[]{line(),
        line()
    }.slice();
    show(false, (@string)"S7"u8, local);
    show(false, (@string)"S8"u8, ret());
}

} // end main_package
