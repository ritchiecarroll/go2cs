namespace go;

using fmt = fmt_package;

partial class main_package {

internal static (nint, nint) two(nint x) {
    return (x, x * 10);
}

internal static nint redeclareParam(nint a) {
    if (a < 0) {
        return -a;
    }
    (a, var b) = two(a);
    return a + b;
}

internal static nint redeclareLocal() {
    nint c = 5;
    (c, var d) = two(c);
    return c + d;
}

internal static void main() {
    fmt.Println(redeclareParam(3));
    fmt.Println(redeclareLocal());
}

} // end main_package
