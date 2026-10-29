namespace go;

using fmt = fmt_package;
using Δmath = math_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static float64 localInf() {
    uint64 bits = 0x7FF0000000000000UL;
    return bitcast<uint64, float64>(bits);
}

internal static ж<uint32> kept;

internal static float32 alsoAddressed(uint32 bʗp) {
    ref var b = ref heap(bʗp, out var Ꮡb);

    kept = Ꮡb;
    return bitcast<uint32, float32>(b);
}

internal static float32 orBits(float32 xʗp, float32 y) {
    ref var x = ref heap(xʗp, out var Ꮡx);

    (Ꮡx.Reinterpret<float32, uint32>()).Value |= bitcast<float32, uint32>(y);
    return x;
}

internal static void Main() {
    foreach (var (_, f) in slice<float64>([0D, 1.5D, -2.25D, Δmath.Inf(1), Δmath.Inf(-1), Δmath.MaxFloat64, Δmath.SmallestNonzeroFloat64])) {
        fmt.Printf("%v %#016x %v\n"u8, f, Δmath.Float64bits(f), Δmath.Float64frombits(Δmath.Float64bits(f)) == f);
    }

    var negZero = Δmath.Copysign(0D, -1D);
    fmt.Printf("-0 %#016x %v\n"u8, Δmath.Float64bits(negZero), Δmath.Signbit(Δmath.Float64frombits(Δmath.Float64bits(negZero))));

    var nan = Δmath.Float64frombits(0x7FF8000000000123UL);
    fmt.Printf("nan payload %#016x %v\n"u8, Δmath.Float64bits(nan), Δmath.IsNaN(nan));

    foreach (var (_, f) in slice<float32>([0F, 1.5F, -2.25F, (float32)Δmath.Inf(1), Δmath.MaxFloat32])) {
        fmt.Printf("%v %#08x %v\n"u8, f, Δmath.Float32bits(f), Δmath.Float32frombits(Δmath.Float32bits(f)) == f);
    }

    fmt.Println(localInf(), alsoAddressed(0x3FC00000), kept.Value == 0x3FC00000);
    var negZero32 = Δmath.Float32frombits(0x80000000U);
    fmt.Println(orBits(1.5F, negZero32), Δmath.Float32bits(orBits(1.5F, negZero32)) == 0xBFC00000U);
}

} // end main_package
