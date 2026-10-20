namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var z = 1.0D;
    float64 x = 3D;
    while (x >= 1D) {
        z = z * x;
        x = x - 1D;
    }
    fmt.Println(z);

    float32 f = 2.5F;
    f = f * 2F;
    fmt.Println(f);
}

} // end main_package
