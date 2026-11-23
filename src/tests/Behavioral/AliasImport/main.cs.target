namespace go;

using fmt = fmt_package;
using time = time_package;
using AliasImportLib = AliasImportLib_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static void main() {
    AliasImportLibꓸIntFn f = [GoFunc("main.func1")] (nint x) => x + 1;
    AliasImportLibꓸBoxFn g = [GoFunc("main.func2")] (AliasImportLib.Box b) => new AliasImportLib.Box(V: b.V * 2);
    AliasImportLibꓸDurFn d = [GoFunc("main.func3")] (time.Duration t) => (nint)(t / time.Second);
    AliasImportLibꓸAct a = [GoFunc("main.func4")] () => {
        fmt.Println("act");
    };
    AliasImportLibꓸMulti m = [GoFunc("main.func5")] (nint x) => (x * 10, default);
    AliasImportLibꓸVar v = [GoFunc("main.func6")] (params ꓸꓸꓸnint xsʗp) => {
        var xs = xsʗp.sslice();
        return len(xs);
    };
    AliasImportLibꓸNamed nr = [GoFunc("main.func7")] (nint x) => {
        return (x + 100, x > 0);
    };

    a();
    var (r, err) = m(5);
    fmt.Println("func:", f(1), g(new AliasImportLibꓸB2(V: 3)).V, d(2 * time.Second), r, err, v(1, 2, 3), AliasImportLib.ApplyVar(v, 4, 5));
    var (n, ok) = nr(7);
    fmt.Println("named:", n, ok);
    fmt.Println("struct:", new AliasImportLibꓸB2(V: 4));
    aliased();
}

} // end main_package
