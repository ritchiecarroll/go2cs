namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint takesInt(nint n) {
    return n * 10;
}

internal static void main() {
    fmt.Println(takesInt(1));
    fmt.Println(takesInt(-2));
    var x = 1.5D;
    fmt.Println(x);
}

} // end main_package
