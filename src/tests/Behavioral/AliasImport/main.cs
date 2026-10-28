namespace go;

using fmt = fmt_package;
using time = time_package;
using AliasImportLib = AliasImportLib_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static void Main() {
    AliasImportLibꓸIntFn f = (nint x) => x + 1;
    AliasImportLibꓸBoxFn g = (AliasImportLib.Box b) => new AliasImportLib.Box(V: b.V * 2);
    AliasImportLibꓸDurFn d = (time.Duration t) => (nint)(t / time.ΔSecond);
    AliasImportLibꓸAct a = () => {
        fmt.Println("act");
    };
    AliasImportLibꓸMulti m = (nint x) => (x * 10, default);
    AliasImportLibꓸVar v = (params ꓸꓸꓸnint xsʗp) => {
        var xs = xsʗp.sslice();
        return len(xs);
    };
    AliasImportLibꓸNamed nr = (nint x) => {
        return (x + 100, x > 0);
    };

    a();
    var (r, err) = m(5);
    fmt.Println("func:", f(1), g(new AliasImportLibꓸB2(V: 3)).V, d(2 * time.ΔSecond), r, err, v(1, 2, 3), AliasImportLib.ApplyVar(v, 4, 5));
    var (n, ok) = nr(7);
    fmt.Println("named:", n, ok);
    fmt.Println("struct:", new AliasImportLibꓸB2(V: 4));
    aliased();
}

} // end main_package
