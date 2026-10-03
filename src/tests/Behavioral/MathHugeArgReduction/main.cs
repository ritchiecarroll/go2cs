namespace go;

using fmt = fmt_package;
using Δmath = math_package;

partial class main_package {

internal static void Main() {
    foreach (var (_, x) in new float64[]{9223372036854775808D, 18446744073709551616D, 1e300D, -(9223372036854775808D)}.slice()) {
        var (s, c) = Δmath.Sincos(x);
        fmt.Printf("%g: sin=%.17g cos=%.17g tan=%.17g sincos=(%.17g, %.17g)\n"u8, x, Δmath.Sin(x), Δmath.Cos(x), Δmath.Tan(x), s, c);
    }
}

} // end main_package
