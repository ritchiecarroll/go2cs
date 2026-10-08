namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸnint = Span<nint>;
using ꓸꓸꓸжPoint = Span<ж<main_package.Point>>;

partial class main_package {

partial struct Point {
    public nint X, Y;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object a1Variadic1ParamActionˢ = (@string)"A1 variadic-1-param Action nil:"u8;
private static readonly object a2Variadic1ParamFuncNilˢ = (@string)"A2 variadic-1-param Func nil:"u8;
private static readonly object a3Variadic1ParamMultiˢ = (@string)"A3 variadic-1-param multi-result nil:"u8;
private static readonly object b1Variadic2ParamNilˢ = (@string)"B1 variadic-2-param nil:"u8;
private static readonly object b2NonVariadicNilˢ = (@string)"B2 non-variadic nil:"u8;
private static readonly object b3NonVariadicBoolNilˢ = (@string)"B3 non-variadic bool nil:"u8;
private static readonly object b4NoParamNilˢ = (@string)"B4 no-param nil:"u8;
private static readonly object b5ConversionOfARealFuncˢ = (@string)"B5 conversion of a real func:"u8;
private static readonly object c1OneNilArgˢ = (@string)"C1 one nil arg:"u8;
private static readonly object c2NilAmongOthersˢ = (@string)"C2 nil among others:"u8;
private static readonly object c3NoVariadicArgsˢ = (@string)"C3 no variadic args:"u8;
private static readonly object c4Spreadˢ = (@string)"C4 spread:"u8;
private static readonly object c5TypedNilSliceArgˢ = (@string)"C5 typed nil slice arg:"u8;
private static readonly object c6Sumˢ = (@string)"C6 sum:"u8;

internal static void Main() {
    var a = (Actionꓸꓸꓸ<nint>)(default!);
    fmt.Println(a1Variadic1ParamActionˢ, a == default!);
    var b = (Funcꓸꓸꓸ<Point, nint>)(default!);
    fmt.Println(a2Variadic1ParamFuncNilˢ, b == default!);
    var c = (Funcꓸꓸꓸ<@string, (nint, bool)>)(default!);
    fmt.Println(a3Variadic1ParamMultiˢ, c == default!);
    var d = (Funcꓸꓸꓸ<Point, Point, nint>)(default!);
    fmt.Println(b1Variadic2ParamNilˢ, d == default!);
    var e = (Func<Point, nint>)(default!);
    fmt.Println(b2NonVariadicNilˢ, e == default!);
    var f = (Func<nint, bool>)(default!);
    fmt.Println(b3NonVariadicBoolNilˢ, f == default!);
    var g = (Action)(default!);
    fmt.Println(b4NoParamNilˢ, g == default!);
    var h = (Funcꓸꓸꓸ<nint, nint>)(sum);
    fmt.Println(b5ConversionOfARealFuncˢ, h(1, 2, 3));
    fmt.Println(c1OneNilArgˢ, countArgs("x"u8, (any)(default!)));
    fmt.Println(c2NilAmongOthersˢ, countArgs("x"u8, (nint)(1), (any)(default!), (@string)"y"u8));
    fmt.Println(c3NoVariadicArgsˢ, countArgs("x"u8));
    fmt.Println(c4Spreadˢ, countArgs("x"u8, new any[]{(nint)(1), default!, (nint)(3)}.slice().ꓸꓸꓸ));
    fmt.Println(c5TypedNilSliceArgˢ, countPtrs((ж<Point>)(nil), (ж<Point>)(nil)));
    fmt.Println(c6Sumˢ, sum(4, 5, 6));
}

internal static nint sum(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.sslice();

    nint t = 0;
    foreach (var (_, x) in xs) {
        t += x;
    }
    return t;
}

internal static nint countArgs(@string s, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return len(args);
}

internal static nint countPtrs(params ꓸꓸꓸжPoint argsʗp) {
    var args = argsʗp.sslice();

    return len(args);
}

} // end main_package
