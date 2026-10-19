namespace go;

using fmt = fmt_package;
using Δmath = math_package;

partial class main_package {

internal static float64 toFloat64<T>(T v)
    where T : /* int | float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return ConvertToFloat64<T>(v);
}

internal static float32 toFloat32<T>(T v)
    where T : /* int64 | float32 | float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return ConvertToFloat32<T>(v);
}

internal static @string csv<T>(@string name, slice<T> values)
    where T : /* int | float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    @string @out = name;
    foreach (var (_, v) in values) {
        if (Δmath.IsInf(ConvertToFloat64<T>(v), +1)){
            @out += ",inf"u8;
        } else {
            @out += fmt.Sprintf(",%v"u8, ConvertToFloat64<T>(v));
        }
    }
    return @out;
}

internal static void Main() {
    fmt.Println(toFloat64<nint>(7), toFloat64(2.5D), toFloat64(unchecked((nint)(9007199254740993L))) == 9007199254740992D, toFloat64<nint>(-3));
    fmt.Println(toFloat32<int64>((1 << (int)(24)) + 1) == (1 << (int)(24)), toFloat32((float32)1.5F), toFloat32(16777217.0D) == (1 << (int)(24)));
    fmt.Println(csv("ints", new nint[]{1, 2, 3}.slice()));
    fmt.Println(csv("floats", new float64[]{0.5D, Δmath.Inf(1), -2D}.slice()));
}

} // end main_package
