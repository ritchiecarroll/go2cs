namespace go;

using fmt = fmt_package;

partial class main_package {

public static (T, T) Swap<T>(T a, T b) {
    return (b, a);
}

internal static void Main() {
    nint a = 10;
    nint b = 20;
    (a, b) = Swap<nint>(a, b);
    fmt.Printf("After swap: a=%d, b=%d\n"u8, a, b);

    @string x = "hello";
    @string y = "world";
    (x, y) = Swap(x, y);
    fmt.Printf("After swap: x=%s, y=%s\n"u8, x, y);
}

} // end main_package
