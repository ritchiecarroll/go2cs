namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint line() {
    var (_, _, l, _) = runtime.Caller(1);
    return l;
}

[GoType] partial struct chain {
    internal slice<nint> lines;
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static ж<chain> Add(this ж<chain> Ꮡc, nint n) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (_, _, l, _) = runtime.Caller(1);
    c.lines = append(c.lines, l);
    return Ꮡc;
}

[MethodImpl(MethodImplOptions.NoInlining)] [GoRecv] internal static void Done(this ref chain c, @string label) {
    var (_, _, l, _) = runtime.Caller(1);
    fmt.Println(label, c.lines, l);
}

[GoType] partial interface liner {
    nint Line();
}

[GoType] partial struct impl {
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint Line(this impl _Δp0) {
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


[GoType("dyn")] partial struct pkgStructsᴛ1 {
    internal nint a, b;
}
internal static slice<pkgStructsᴛ1> pkgStructs = new pkgStructsᴛ1[]{
    new(line(), line()),
    new(
        line(),
        line()
    )
}.slice();

internal static slice<nint> ret() {
    return three(line(),
        line(),
        line());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pkgSliceˢ = (@string)"pkgSlice"u8;
private static readonly object pkgSumˢ = (@string)"pkgSum"u8;
private static readonly object pkgStructsˢ = (@string)"pkgStructs"u8;

internal static void Main() {
    fmt.Println(pkgSliceˢ, pkgSlice);
    fmt.Println(pkgSumˢ, pkgSum);
    fmt.Println(pkgStructsˢ, pkgStructs);
    (Ꮡ(new chain(nil))).Add(1).Add(2).Add(3).Done("S1"u8);
    fmt.Println((@string)"S2"u8, three(line(),
        line(),
        line()));
    nint sum = line() + line() + line();
    fmt.Println((@string)"S3"u8, sum);
    var f = line;
    fmt.Println((@string)"S4"u8, f(),
        f());
    impl im = default!;
    fmt.Println((@string)"S5"u8, im.Line(),
        im.Line());
    if (rec(line()) && rec(line())) {
        fmt.Println((@string)"S6"u8, recorded);
    }
    var local = new nint[]{line(),
        line()
    }.slice();
    fmt.Println((@string)"S7"u8, local);
    fmt.Println((@string)"S8"u8, ret());
}

} // end main_package
