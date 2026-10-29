namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸnint = Span<nint>;
using ꓸꓸꓸжPoint = Span<ж<main_package.Point>>;

partial class main_package {

partial struct Point {
    public nint X, Y;
}

internal static void Main() {
    var a = (Actionꓸꓸꓸ<nint>)(default);
    fmt.Println("A1 variadic-1-param Action nil:", a == nil);

    var b = (Funcꓸꓸꓸ<Point, nint>)(default);
    fmt.Println("A2 variadic-1-param Func nil:", b == nil);

    var c = (Funcꓸꓸꓸ<@string, (nint, bool)>)(default);
    fmt.Println("A3 variadic-1-param multi-result nil:", c == nil);

    var d = (Funcꓸꓸꓸ<Point, Point, nint>)(default);
    fmt.Println("B1 variadic-2-param nil:", d == nil);

    var e = (Func<Point, nint>)(default);
    fmt.Println("B2 non-variadic nil:", e == nil);

    var f = (Func<nint, bool>)(default);
    fmt.Println("B3 non-variadic bool nil:", f == nil);

    var g = (Action)(default);
    fmt.Println("B4 no-param nil:", g == nil);

    var h = (Funcꓸꓸꓸ<nint, nint>)(sum);
    fmt.Println("B5 conversion of a real func:", h(1, 2, 3));

    fmt.Println("C1 one nil arg:", countArgs("x", (any)(default)));
    fmt.Println("C2 nil among others:", countArgs("x", (nint)1, (any)(default), "y"));
    fmt.Println("C3 no variadic args:", countArgs("x"));
    fmt.Println("C4 spread:", countArgs("x"u8, slice<any>([(nint)1, default, (nint)3]).ꓸꓸꓸ));
    fmt.Println("C5 typed nil slice arg:", countPtrs((ж<Point>)(nil), (ж<Point>)(nil)));
    fmt.Println("C6 sum:", sum(4, 5, 6));
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
