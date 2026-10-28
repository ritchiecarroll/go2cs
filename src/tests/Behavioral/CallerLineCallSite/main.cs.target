namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

partial struct E {
    internal nint n;
}

partial struct P {
    internal nint a, b;
}

internal static nint got;

internal static partial nint viaCallers() {
    var pcs = new slice<uintptr>(8);
    nint n = runtime.Callers(3, pcs);
    var (frame, _) = runtime.CallersFrames(pcs.slice(0, n)).Next();
    return frame.Line;
}

public static partial void Info(this ref E e, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    got = viaCallers();
}

public static partial void Take(this ref E e, P p) {
    got = viaCallers();
}

public static partial void One(this ref E e, any a) {
    got = viaCallers();
}

public static partial void Bare(this ref E e) {
    got = viaCallers();
}

public static partial nint Value(this ref E e, any a) {
    return viaCallers();
}

internal static partial void takeP(P p) {
    got = viaCallers();
}

internal static partial void plain(any a) {
    got = viaCallers();
}

internal static partial void plainV(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    got = viaCallers();
}

internal static partial nint line() {
    var (_, _, l, _) = runtime.Caller(1);
    return l;
}

internal static void report(@string name, nint have, nint want) {
    fmt.Println(name, have == want);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object looksDeliciousˢ = (@string)"looks delicious"u8;
private static readonly object litˢ = (@string)"lit"u8;

internal static void Main() {
    var e = Ꮡ(new E(nil));
    nint l = default;

    e.Info(looksDeliciousˢ);
    l = line();
    report("S1 method, variadic, one literal:", got, l - 1);

    plain("x");
    l = line();
    report("S2 func, one converted literal:", got, l - 1);

    e.Bare();
    e.Value.n++;
    report("S3 method, no args, then a non-call:", got, line() - 2);

    nint r = e.Value((@string)"y"u8);
    report("S4 method, result used:", r, line() - 1);

    e.Info((@string)"a"u8, (nint)1, 2.5D);
    l = line();
    report("S5 method, variadic, three args:", got, l - 1);

    e.Take(new P(1, 2));
    l = line();
    report("S6 method, struct literal arg:", got, l - 1);

    takeP(new P(3, 4));
    l = line();
    report("S7 func, struct literal arg:", got, l - 1);

    plainV("lit");
    l = line();
    report("S8 func, variadic, one literal:", got, l - 1);

    e.One(litˢ);
    l = line();
    report("S9 method, one literal, not variadic:", got, l - 1);
}

} // end main_package
