namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    nint z = 1;
    [GoFunc("main.func1")] nint setZ() {
        z = 10;
        return 2;
    }
    if (z < setZ()) {
        fmt.Println("condition: z read first (1 < 2)");
    } else {
        fmt.Println("condition: z read after the call (10 < 2 is false)");
    }

    nint w = 1;
    [GoFunc("main.func2")] nint setW() {
        w = 10;
        return 2;
    }
    nint n = 3; if (w + setW() == 12) {
        fmt.Println("init and condition: w read after the call, n =", n);
    } else {
        fmt.Println("init and condition: w read first, n =", n);
    }

    nint u = 1;
    [GoFunc("main.func3")] nint setU() {
        u = 10;
        return 2;
    }
    if (false) {
    } else if (u == setU() * 5) {
        fmt.Println("else if: u read after the call");
    } else {
        fmt.Println("else if: u read first");
    }
}

} // end main_package
