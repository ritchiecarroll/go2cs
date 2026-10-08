namespace go;

using fmt = fmt_package;
using Δmath = math_package;

partial class main_package {

public static UntypedFloat MaxFloat32 => /* 0x1p127 * (1 + (1 - 0x1p-23)) */ 3.4028234663852886e+38;
public static UntypedFloat SmallestNonzeroFloat32 => /* 0x1p-126 * 0x1p-23 */ 1.401298464324817e-45;
public static UntypedFloat MaxFloat64 => /* 0x1p1023 * (1 + (1 - 0x1p-52)) */ 1.7976931348623157e+308;
public static UntypedFloat SmallestNonzeroFloat64 => /* 0x1p-1022 * 0x1p-52 */ 5e-324;

public static UntypedFloat Ln10 => 2.30258509299404568401799145468436420760110148862877297603332790;
public static UntypedFloat Log10E => /* 1 / Ln10 */ 0.4342944819032518;
public static UntypedFloat Pi => 3.14159265358979323846264338327950288419716939937510582097494459;
internal static UntypedFloat twoPi => /* 2 * Pi */ 6.283185307179586;
internal static UntypedFloat halfPi => /* Pi / 2 */ 1.5707963267948966;
internal static UntypedFloat third => /* 1.0 / 3.0 */ 0.3333333333333333;

partial struct MyFloat /*num:float64*/;

public static float64 Abs(this MyFloat f) {
    if (f < 0D) {
        return (float64)(-f);
    }
    return (float64)f;
}

internal static bool isInf(float64 f, nint sign) {
    return sign >= 0 && f > MaxFloat64 || sign <= 0 && f < /* -MaxFloat64 */ -1.7976931348623157e+308D;
}

internal static void bits64(@string label, uint64 got, uint64 want) {
    fmt.Println(label, got, got == want);
}

internal static void bits32(@string label, uint32 got, uint32 want) {
    fmt.Println(label, got, got == want);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedFloatConversionˢ = (@string)"-- named float conversion --"u8;
private static readonly object float64Constantˢ = (@string)"-- float64 constant expressions (IEEE 754 bits, want-match) --"u8;
private static readonly @string maxFloat64ˢ = "MaxFloat64            "u8;
private static readonly @string smallestNonzeroFloat64ˢ = "SmallestNonzeroFloat64"u8;
private static readonly @string ln10ˢ = "Ln10                  "u8;
private static readonly @string log10Eˢ = "Log10E                "u8;
private static readonly @string twoPiˢ = "twoPi                 "u8;
private static readonly @string halfPiˢ = "halfPi                "u8;
private static readonly @string thirdˢ = "third                 "u8;
private static readonly object float32Constantˢ = (@string)"-- float32 constant expressions (IEEE 754 bits, want-match) --"u8;
private static readonly @string maxFloat32ˢ = "MaxFloat32            "u8;
private static readonly @string smallestNonzeroFloat32ˢ = "SmallestNonzeroFloat32"u8;
private static readonly object exactValueIdentitiesˢ = (@string)"-- exact-value identities --"u8;
private static readonly object maxFloat64Literalˢ = (@string)"MaxFloat64 == literal:"u8;
private static readonly object smallestNonzeroFloat64ˢ2 = (@string)"SmallestNonzeroFloat64 == literal:"u8;
private static readonly object maxFloat32Literalˢ = (@string)"MaxFloat32 == literal:"u8;
private static readonly object log10ELiteralˢ = (@string)"Log10E == literal:"u8;
private static readonly object isInfBoundaryˢ = (@string)"-- IsInf boundary --"u8;
private static readonly object isInfMaxFloat64ˢ = (@string)"isInf(MaxFloat64):"u8;
private static readonly object isInfMaxFloat64ˢ2 = (@string)"isInf(-MaxFloat64):"u8;
private static readonly object isInfTruncated179769e308ˢ = (@string)"isInf(truncated 1.79769e+308):"u8;
private static readonly object truncatedMaxFloat64ˢ = (@string)"truncated < MaxFloat64:"u8;

internal static void Main() {
    fmt.Println(namedFloatConversionˢ);
    var f = ((MyFloat)(/* -math.Sqrt2 */ -1.4142135623730951D));
    fmt.Println(f.Abs());
    fmt.Println(float64Constantˢ);
    bits64(maxFloat64ˢ, Δmath.Float64bits(MaxFloat64), 0x7fefffffffffffffUL);
    bits64(smallestNonzeroFloat64ˢ, Δmath.Float64bits(SmallestNonzeroFloat64), 0x1);
    bits64(ln10ˢ, Δmath.Float64bits(Ln10), 0x40026bb1bbb55516UL);
    bits64(log10Eˢ, Δmath.Float64bits(Log10E), 0x3fdbcb7b1526e50eUL);
    bits64("Pi                    "u8, Δmath.Float64bits(Pi), 0x400921fb54442d18UL);
    bits64(twoPiˢ, Δmath.Float64bits(twoPi), 0x401921fb54442d18UL);
    bits64(halfPiˢ, Δmath.Float64bits(halfPi), 0x3ff921fb54442d18UL);
    bits64(thirdˢ, Δmath.Float64bits(third), 0x3fd5555555555555UL);
    fmt.Println(float32Constantˢ);
    bits32(maxFloat32ˢ, Δmath.Float32bits(MaxFloat32), 0x7f7fffff);
    bits32(smallestNonzeroFloat32ˢ, Δmath.Float32bits(SmallestNonzeroFloat32), 0x1);
    fmt.Println(exactValueIdentitiesˢ);
    fmt.Println(maxFloat64Literalˢ, (float64)MaxFloat64 == 1.7976931348623157e+308D);
    fmt.Println(smallestNonzeroFloat64ˢ2, (float64)SmallestNonzeroFloat64 == 5e-324D);
    fmt.Println(maxFloat32Literalˢ, (float32)MaxFloat32 == 3.4028235e+38F);
    fmt.Println(log10ELiteralˢ, (float64)Log10E == 0.4342944819032518D);
    fmt.Println(isInfBoundaryˢ);
    fmt.Println(isInfMaxFloat64ˢ, isInf(1.7976931348623157e+308D, 1));
    fmt.Println(isInfMaxFloat64ˢ2, isInf(-1.7976931348623157e+308D, -1));
    fmt.Println(isInfTruncated179769e308ˢ, isInf(1.79769e+308D, 1));
    fmt.Println(truncatedMaxFloat64ˢ, 1.79769e+308D < (float64)MaxFloat64);
}

} // end main_package
