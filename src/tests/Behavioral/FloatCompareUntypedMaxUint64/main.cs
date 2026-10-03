namespace go;

using fmt = fmt_package;
using Δmath = math_package;

partial class main_package {

internal static void Main() {
    foreach (var (_, f) in new float64[]{9223372036854775808D, 18446744073709551616D, 1.8446744073709552e37D}.slice()) {
        fmt.Println(f, f >= Δmath.MaxUint64, f <= Δmath.MaxUint64, f > Δmath.MaxInt64);
    }
}

} // end main_package
