namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var q = 1.0D;
    for (nint i = 0; i < 10; i++) {
        q = q * 10.0D;
    }
    fmt.Println(q);

    fmt.Println(0.0001D, 0.00001D);
    fmt.Println(100000.0D, 1000000.0D);
    fmt.Println(999999.0D, 1000001.0D);
    fmt.Println(123456.0D, 1234567.0D);
    fmt.Println(1.610612736e9D);
    fmt.Println(1e20D, 1e21D, 1e22D);
    fmt.Println(1e100D, 1e-100D);

    fmt.Println(1.7976931348623157e+308D, 5e-324D);

    fmt.Println(4.611686018427388e+18D, 9.223372036854776e+18D);
    fmt.Println(123456789.0D, 0.000123456789D);

    var zero = 0.0D;
    var negZero = -zero;
    var posInf = 1.0D / zero;
    var negInf = -1.0D / zero;
    var nan = zero / zero;
    fmt.Println(0.0D, negZero, -1e10D, -0.0001D);
    fmt.Println(posInf, negInf, nan);

    fmt.Println(3.14D, 2.5D, 0.1D, 100.0D, -7.5D);

    float32 f32 = 0.1F;
    fmt.Println(f32, (float64)f32);
    fmt.Println((float32)1e10F, (float32)16777216F, (float32)1e-7F);
    fmt.Println((float32)3.4028235e+38F, (float32)3.14F);

    var v = 1234.5678D;
    fmt.Printf("[%e] [%E] [%f] [%F] [%g] [%G]\n"u8, v, v, v, v, v, v);
    fmt.Printf("[%e] [%f] [%g]\n"u8, 0.0D, 0.0D, 0.0D);
    fmt.Printf("[%e] [%g] [%G]\n"u8, 1e10D, 1e10D, 1e10D);

    fmt.Printf("[%.3e] [%.3E] [%.3f] [%.3g] [%.3G]\n"u8, v, v, v, v, v);
    fmt.Printf("[%.0e] [%.0f] [%.0g] [%.1g]\n"u8, v, v, v, v);
    fmt.Printf("[%.4g] [%.8g] [%.10g]\n"u8, v, v, v);
    fmt.Printf("[%e] [%.2e] [%.2e]\n"u8, 1e-7D, 1e-7D, 1e100D);

    fmt.Printf("[%.5g] [%.5g] [%.5g]\n"u8, 1.5D, 100000.0D, 1e10D);

    fmt.Printf("[%.17g] [%.20g] [%.20e]\n"u8, 0.1D, 0.1D, 0.1D);
    fmt.Printf("[%.20f] [%.30f]\n"u8, 0.1D, 0.1D);

    fmt.Printf("[%.0f] [%.0f] [%.0f] [%.0f]\n"u8, 0.5D, 1.5D, 2.5D, 3.5D);
    fmt.Printf("[%.2f] [%.1f] [%.0e]\n"u8, 2.675D, 0.25D, 2.5D);

    fmt.Printf("[%12.3e] [%-12.3e] [%012.3e]\n"u8, v, v, v);
    fmt.Printf("[%+.2f] [% .2f] [%+g] [% g]\n"u8, v, v, v, v);
    fmt.Printf("[%8.2f] [%-8.2f] [%08.2f] [%08.2f]\n"u8, v, v, v, -v);

    fmt.Println(fmt.Sprint(1e10D), fmt.Sprintf("%v"u8, 1e10D), fmt.Sprintf("%g"u8, 1e10D));
}

} // end main_package
