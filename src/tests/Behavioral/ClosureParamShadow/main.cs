namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint run(Func<nint, nint> f) {
    return f(10);
}

internal static void Main() {
    nint n = 100;

    nint r = run((nint nΔ1) => {
        nΔ1 = nΔ1 * 2;
        return nΔ1 + 1;
    });

    fmt.Println(r, n);

    nint add = 7;
    nint g(nint nΔ2) => nΔ2 + add;
    fmt.Println(g(n), n);
}

} // end main_package
