namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt pkgConst => 1000;

internal static UntypedInt hugeConst => /* 1 << 63 */ 9223372036854775808;

internal static void Main() {
    UntypedInt m = 3;
    nint i = m;
    float64 fl = m;
    float32 f32 = m;
    fmt.Println(i, fl, f32);

    float64 half = m;
    fmt.Println(half / 2D);

    float64 pf = pkgConst;
    nint pi = pkgConst;
    fmt.Println(pf / 8D, pi);

    float64 hf = hugeConst;
    uint64 hu = hugeConst;
    fmt.Println(hf / (1152921504606846976D), hu);

    complex128 c = m;
    complex64 c64 = m;
    fmt.Println(real(c), imag(c), real(c64));

    UntypedInt neg = -7;
    float64 nf = neg;
    nint ni = neg;
    fmt.Println(nf / 2D, ni);
}

} // end main_package
