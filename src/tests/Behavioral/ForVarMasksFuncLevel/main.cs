namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint search(slice<nint> xs, nint t) {
    for (nint bΔ1 = 0; bΔ1 < len(xs); bΔ1++) {
        if (xs[bΔ1] == t) {
            return bΔ1;
        }
    }
    for (nint bΔ2 = len(xs) - 1; bΔ2 >= 0; bΔ2--) {
        if (xs[bΔ2] == t * 2) {
            return bΔ2;
        }
    }
    nint b = -1;
    return b;
}

internal static void Main() {
    var xs = slice<nint>([5, 6, 7, 8]);
    fmt.Println(search(xs, 7));
    fmt.Println(search(xs, 3));
    fmt.Println(search(xs, 99));
}

} // end main_package
