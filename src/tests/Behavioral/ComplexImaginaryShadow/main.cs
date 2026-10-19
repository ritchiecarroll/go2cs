namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string encComplex(nint i, complex128 c) {
    _ = i;
    if (c != 0D.i()) {
        return "nonzero";
    }
    return "zero";
}

internal static void Main() {
    fmt.Println(encComplex(1, complex(0D, 0D)));
    fmt.Println(encComplex(2, complex(3D, -4D)));
    fmt.Println(encComplex(3, 5D.i()));
}

} // end main_package
