namespace go;

using fmt = fmt_package;

partial class main_package {

internal static (nint, nint) match(nint x, nint y) {
    if (x > y) {
        return (y, x);
    }
    return (x, y);
}

internal static (nint lo, nint) keyed(nint x, nint y) {
    if (x < y) {
        return (x, y);
    }
    return (y, x);
}

internal static void Main() {
    var (a, b) = match(5, 2);
    fmt.Println(a, b);
    var (c, d) = match(1, 9);
    fmt.Println(c, d);

    var (lo, hi) = keyed(7, 3);
    fmt.Println(lo, hi);
}

} // end main_package
