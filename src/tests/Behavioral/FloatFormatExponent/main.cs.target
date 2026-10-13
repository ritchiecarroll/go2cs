namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    float64 e5 = 1e5D;
    float64 e6 = 1e6D;
    float64 million = 1000000.0D;
    float64 justUnder = 999999.0D;
    float64 manyDigits = 1234567.0D;
    float64 mixed = 123456.789D;
    fmt.Println(e5, e6, million, justUnder);
    fmt.Println(manyDigits, mixed);

    float64 em4 = 1e-4D;
    float64 em5 = 1e-5D;
    fmt.Println(em4, em5);
    fmt.Println(0.0001D, 0.00001D);

    float64 e20 = 1e20D;
    float64 e21 = 1e21D;
    float64 e22 = 1e22D;
    fmt.Println(e20, e21, e22);

    float64 maxFloat64 = 1.7976931348623157e+308D;
    float64 smallestNonzeroFloat64 = 5e-324D;
    float64 tiniestNormal = 2.2250738585072014e-308D;
    fmt.Println(maxFloat64, smallestNonzeroFloat64, tiniestNormal);

    float64 zero = 0.0D;
    var negZero = -zero;
    fmt.Println(zero, negZero);
    fmt.Println(-e6, -em5, -maxFloat64, -1.5D);

    float32 f32 = 0.1F;
    float32 f32e6 = 1e6F;
    float32 f32em5 = 1e-5F;
    float32 maxFloat32 = 3.4028235e+38F;
    float32 smallestNonzeroFloat32 = 1e-45F;
    fmt.Println(f32, f32e6, f32em5);
    fmt.Println(maxFloat32, smallestNonzeroFloat32);
    fmt.Println((float32)(1.0F / 3.0F), 1.0D / 3.0D);

    fmt.Printf("[%v] [%g] [%e] [%f]\n"u8, e6, e6, e6, e6);
    fmt.Printf("[%v] [%g] [%e] [%f]\n"u8, em5, em5, em5, em5);
    fmt.Printf("[%v] [%g] [%e] [%f]\n"u8, mixed, mixed, mixed, mixed);

    fmt.Printf("[%.3g] [%.3e] [%.3f]\n"u8, mixed, mixed, mixed);
    fmt.Printf("[%.0f] [%.1e] [%.10g]\n"u8, mixed, mixed, mixed);

    fmt.Printf("[%.0g] [%.0g] [%.0G] [%.0e]\n"u8, 0.7D, -0.7D, 0.7D, 0.7D);

    fmt.Printf("[%G] [%E] [%G] [%E]\n"u8, e6, e6, smallestNonzeroFloat64, maxFloat64);

    fmt.Printf("[%v] [%g] [%e] [%f] [%.2f]\n"u8, negZero, negZero, negZero, negZero, negZero);
    fmt.Printf("[%v] [%g] [%e] [%f] [%.2f]\n"u8, zero, zero, zero, zero, zero);

    fmt.Printf("[%v] [%g] [%e] [%f]\n"u8, f32, f32, f32, f32);
    fmt.Printf("[%v] [%g]\n"u8, maxFloat32, smallestNonzeroFloat32);

    fmt.Println(fmt.Sprint(e6), fmt.Sprint(em5), fmt.Sprint(negZero));
    fmt.Println(fmt.Sprintf("%v|%g"u8, maxFloat64, smallestNonzeroFloat64));

    float64 big = 4611686018427387904D;
    fmt.Printf("%g\n"u8, big);
}

} // end main_package
