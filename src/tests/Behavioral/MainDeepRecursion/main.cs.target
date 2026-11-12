namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt levels => 2_000_000;

internal static nint depth(nint n) {
    if (n == 0) {
        return 0;
    }
    return depth(n - 1) + 1;
}

internal static void main() {
    fmt.Println("depth:", depth(levels));
}

} // end main_package
