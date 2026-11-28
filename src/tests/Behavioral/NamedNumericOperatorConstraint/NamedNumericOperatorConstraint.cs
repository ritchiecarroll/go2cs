namespace go;

using fmt = fmt_package;
using Δmath = math_package;

partial class main_package {

partial struct stringID /*num:uint64*/;
partial struct offset /*num:int32*/;
partial struct small /*num:uint8*/;
partial struct handle /*num:uintptr*/;
partial struct celsius /*num:float64*/;

internal static K mix<K>(K id, K step)
    where K : /* ~uint64 | ~int32 */ IAdditionOperators<K, K, K>, ISubtractionOperators<K, K, K>, IMultiplyOperators<K, K, K>, IDivisionOperators<K, K, K>, IIncrementOperators<K>, IDecrementOperators<K>, IUnaryNegationOperators<K, K>, IModulusOperators<K, K, K>, IBitwiseOperators<K, K, K>, IShiftOperators<K, int, K>, IEqualityOperators<K, K, bool>, IComparisonOperators<K, K, bool>, new()
{
    var h = id;
    h = (K)(h ^ (h >> 2));
    h = (K)(h | (step << 1));
    h = (K)(h & step);
    h = rem(h, (step + builtin.ConvertToType<K>(7)));
    return h;
}

internal static T conv<T>(uint64 x)
    where T : /* ~uint8 | ~uintptr | ~int32 | ~uint64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{ return builtin.ConvertToType<T>(x); }

internal static T convSigned<T>(int64 x)
    where T : /* ~uint8 | ~int32 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{ return builtin.ConvertToType<T>(x); }

internal static uint64 widen<T>(T v)
    where T : /* ~uint8 | ~uintptr | ~int32 | ~uint64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{ return builtin.ConvertToUInt64<T>(v); }

internal static float64 asFloat64<T>(T v)
    where T : /* ~int32 | ~float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{ return builtin.ConvertToFloat64<T>(v); }

internal static float32 asFloat32<T>(T v)
    where T : /* ~int32 | ~float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{ return builtin.ConvertToFloat32<T>(v); }

internal static T fromFloat<T>(float64 f)
    where T : /* ~int32 | ~uint8 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{ return builtin.ConvertToType<T>(f); }

internal static void main() {
    stringID a = 12345;
    stringID b = 11;
    fmt.Println(mix(a, b));

    offset x = -8;
    offset y = 5;
    fmt.Println(mix(x, y));

    fmt.Println(mix((stringID)255, (stringID)3), mix((offset)100, (offset)9));

    fmt.Println(conv<small>(300), conv<offset>((uint64)(4294967296L + 5)), conv<stringID>(Δmath.MaxUint64), conv<handle>((uint64)1 << 40));
    fmt.Println(convSigned<small>(-1), convSigned<offset>(-7), convSigned<small>(-256));

    fmt.Println(widen((small)200), widen((offset)(-1)), widen((stringID)Δmath.MaxUint64), widen((handle)7));

    fmt.Println(asFloat64((celsius)21.5D), asFloat64((offset)(-3)), asFloat32((celsius)0.1D), asFloat32((offset)16777217));
    fmt.Println(fromFloat<offset>(-7.9D), fromFloat<small>(200.7D), fromFloat<offset>(2147483647.4D));
}

} // end main_package
