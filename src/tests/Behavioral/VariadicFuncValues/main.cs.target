namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static @string gather(@string prefix, params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.sslice();

    nint total = 0;
    foreach (var (_, v) in vals) {
        total += v;
    }
    return fmt.Sprintf("%s:%d(%d)"u8, prefix, total, len(vals));
}

internal static void apply(Funcꓸꓸꓸ<@string, nint, @string> f) {
    fmt.Println(f("loose"u8, 1, 2, 3));
    fmt.Println(f("empty"u8));
    var nums = new nint[]{4, 5}.slice();
    fmt.Println(f("spread"u8, nums.ꓸꓸꓸ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string bareˢ = "bare"u8;

internal static void report(Actionꓸꓸꓸ<@string, any> emit) {
    emit("%s=%d"u8, (@string)"x"u8, (nint)(7));
    emit(bareˢ);
}

partial struct logger {
    internal @string tag;
}

internal static void errorf(this ref logger l, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    fmt.Printf(l.tag + "!"u8 + format + "\n"u8, args.ꓸꓸꓸ);
}

internal static void logf(this ref logger l, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    fmt.Printf(l.tag + "~"u8 + format + "\n"u8, args.ꓸꓸꓸ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string oneDˢ = "one %d"u8;
private static readonly @string twoDDˢ = "two %d %d"u8;
private static readonly @string noneˢ = "none"u8;

internal static void swapEmitter(ж<logger> Ꮡl, bool swap) {
    Actionꓸꓸꓸ<@string, any> emit = (@string p1, params ꓸꓸꓸany p2) => Ꮡl.errorf(p1, p2);
    if (swap) {
        emit = (@string p1, params ꓸꓸꓸany p2) => Ꮡl.logf(p1, p2);
    }
    emit(oneDˢ, (nint)(1));
    emit(twoDDˢ, (nint)(2), (nint)(3));
    emit(noneˢ);
    var rest = new any[]{(nint)(4), (nint)(5)}.slice();
    emit("spread %d %d"u8, rest.ꓸꓸꓸ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilFuncValueˢ = (@string)"nil func value"u8;

internal static void Main() {
    apply(gather);
    apply((@string prefix, params ꓸꓸꓸnint valsʗp) => {
        var vals = valsʗp.sslice();
        return fmt.Sprintf("%s|%d"u8, prefix, len(vals));
    });
    Funcꓸꓸꓸ<@string, nint, @string> f = default!;
    if (f == default!) {
        fmt.Println(nilFuncValueˢ);
    }
    f = gather;
    fmt.Println(f("var"u8, 10));
    report((@string format, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.sslice();
        fmt.Printf(format + "\n"u8, args.ꓸꓸꓸ);
    });
    var lg = Ꮡ(new logger(tag: "L"u8));
    swapEmitter(lg, false);
    swapEmitter(lg, true);
}

} // end main_package
