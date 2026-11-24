namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    nint b = 1;
    [GoFunc("main.func1")] nint setB() {
        b = 10;
        return 2;
    }
    nint m1 = b;
    nint m2 = setB();
    fmt.Println("define:", m1, m2);

    b = 1;
    nint n1 = default;
    nint n2 = default;
    (n1, n2) = (b, setB());
    fmt.Println("assign:", n1, n2);

    nint x = 1;
    nint y = 5;
    [GoFunc("main.func2")] nint setY() {
        x = 10;
        return y;
    }
    (x, y) = (setY(), x);
    fmt.Println("swap through a call:", x, y);
}

} // end main_package
