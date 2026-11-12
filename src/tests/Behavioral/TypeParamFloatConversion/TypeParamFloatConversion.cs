namespace go;

using fmt = fmt_package;
using Δmath = math_package;

partial class main_package {

internal static float64 toFloat64<T>(T v)
    where T : /* int | float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{ return ConvertToFloat64<T>(v); }

internal static float32 toFloat32<T>(T v)
    where T : /* int64 | float32 | float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{ return ConvertToFloat32<T>(v); }

partial interface integer<ΔT> /*operators = Sum, Arithmetic, Integer, Comparable, Ordered*/ {
    //  Type constraints: ~int | ~int8 | ~int16 | ~int32 | ~int64 | ~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

partial struct count /*num:int32*/;

internal static T fromAny<T>(any value)
    where T : /* ~int32 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{ return ConvertToType<T>(value._<float64>()); }

internal static T toInteger<T>(float64 v)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{ return ConvertToType<T>(v); }

internal static T toIntegerFrom32<T>(float32 v)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{ return ConvertToType<T>(v); }

internal static @string csv<T>(@string name, slice<T> values)
    where T : /* int | float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    @string @out = name;
    foreach (var (_, v) in range(values)) {
        if (Δmath.IsInf(ConvertToFloat64<T>(v), +1)) {
            @out += ",inf"u8;
        } else {
            @out += fmt.Sprintf(",%v"u8, ConvertToFloat64<T>(v));
        }
    }
    return @out;
}

internal static void main() {
    fmt.Println(toFloat64<nint>(7), toFloat64(2.5D), toFloat64(unchecked((nint)(9007199254740993L))) == 9007199254740992D, toFloat64<nint>(-3));
    fmt.Println(toFloat32<int64>((1 << 24) + 1) == 1 << 24, toFloat32((float32)1.5F), toFloat32(16777217.0D) == 1 << 24);
    fmt.Println(csv("ints", slice<nint>([1, 2, 3])));
    fmt.Println(csv("floats", slice<float64>([0.5D, Δmath.Inf(1), -2D])));
    fmt.Println(fromAny<int32>((float64)41.99D), fromAny<count>(-7.5D), fromAny<int32>(-2147483648.9D));
    fmt.Println(toInteger<nint>(-1.9D), toInteger<int8>(127.9D), toInteger<int8>(-128.9D), toInteger<int16>(-32768.5D), toInteger<int64>(9.2e18D));
    fmt.Println(toInteger<nuint>(3.99D), toInteger<uint8>(255.9D), toInteger<uint16>(65535.5D), toInteger<uint32>(4294967295.5D));
    fmt.Println(toInteger<uint64>(1.8e19D), toInteger<uintptr>(2.5D), toInteger<count>(-0.5D), toIntegerFrom32<int32>(-3.75F));
}

} // end main_package
