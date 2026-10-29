namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedFloat gHalfPi => 1.5707963267948966;

internal static UntypedComplex cRational => /* 5.5 + 1.5i */ 5.5D + 1.5D.i();
internal static UntypedComplex cNegImag => /* 2.25 - 0.75i */ 2.25D + -0.75D.i();
internal static UntypedComplex cPureImag => /* 3i */ 3D.i();
internal static UntypedComplex cWideEnough => /* 1.5e308 + 1.0e307i */ 1.5e+308D + 1e+307D.i();
internal static UntypedComplex cFolded => /* (1 + 2i) * (3 + 4i) */ -5D + 10D.i();

internal static complex64 c64 => /* 1.5 + 2.5i */ 1.5F + 2.5F.i();

internal static UntypedFloat maxFloat32 => 3.40282346638528859811704183484516925440e+38;

internal static void showComplex(@string name, complex128 c) {
    fmt.Println(name, real(c), imag(c));
}

internal static void Main() {
    var huge = slice<complex128>([3.4359738368e+10D + 0D.i(), 1.7668470647783843e+72D + 0D.i(), -1.329227995784916e+36D + 0D.i(), 1.3574217516913025e+27D + 0D.i()]);
    foreach (var (_, h) in huge) {
        fmt.Println(real(h));
    }

    var c = complex((float64)0D, (float64)(gHalfPi));
    fmt.Println(real(c), imag(c));

    float64 q = 7 / 2;
    var e = complex(7 / 2, 0D);
    fmt.Println(q, real(e), imag(e));

    fmt.Println((float64)(gHalfPi / 2D));

    float64 rf = 1.5D;
    var r = complex(rf, 0D);
    fmt.Println(real(r), imag(r));

    showComplex("cRational", cRational);
    showComplex("cNegImag", cNegImag);
    showComplex("cPureImag", cPureImag);
    showComplex("cWideEnough", cWideEnough);
    showComplex("cFolded", cFolded);
    fmt.Println("c64", real(c64), imag(c64));

    complex128 local = /* 0.5 - 0.25i */ 0.5D + -0.25D.i();
    showComplex("local", local);

    var over = complex((float64)(maxFloat32 * 2D), (float64)(maxFloat32 * 2D));
    fmt.Printf("over %T %v %v\n"u8, over, real(over), imag(over));
    fmt.Println("over-fits-float32", (float64)(float32)real(over) == real(over));

    var pair = complex((float64)(gHalfPi), (float64)(gHalfPi));
    fmt.Printf("pair %T %v %v\n"u8, pair, real(pair), imag(pair));

    complex64 narrow = complex((float32)(gHalfPi), (float32)(gHalfPi));
    fmt.Printf("narrow %T %v %v\n"u8, narrow, real(narrow), imag(narrow));

    float64 rf64 = 2D;
    var mixed = complex(rf64, gHalfPi);
    fmt.Printf("mixed %T %v %v\n"u8, mixed, real(mixed), imag(mixed));
}

} // end main_package
