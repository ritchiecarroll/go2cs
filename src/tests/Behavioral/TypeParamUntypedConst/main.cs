namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;

partial class main_package {

partial interface integer<ΔT> /*operators = Sum, Arithmetic, Integer, Comparable, Ordered*/ {
    //  Type constraints: ~uint8 | ~uint32 | ~int8 | ~int64
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

internal static UntypedInt five => 5;

internal static nuint three = 3;
internal static nuint nine = 9;
internal static nuint forty = 40;
internal static nint neg = -1;

internal static void arm(@string @class, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        if (len(os.Args) > 1 && os.Args[1] != @class) {
            return;
        }

        defer(() => {
            var r = recover(); if (r != nil) {
                var (_, isRuntimeError) = r._<runtimeꓸError>(ᐧ);
                fmt.Println(@class + ": panic:", r, isRuntimeError);
            }
        }, ref ᒐ);

        fmt.Println(@class + ":", f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static T one<T>()
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return ConvertToType<T>(1);
}

internal static T named<T>()
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return ConvertToType<T>(five);
}

internal static T constShift<T>()
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return ConvertToType<T>(1 << 3);
}

internal static T bit<T>(nuint n)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return ConvertToType<T>(1).Lsh(n);
}

internal static T bitSigned<T>(nint n)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return ConvertToType<T>(1).Lsh((int64)(n));
}

internal static T mask<T>(nuint n)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return ConvertToType<T>(1).Lsh(n) - ConvertToType<T>(1);
}

internal static T varBit<T>(nuint n)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T x = ConvertToType<T>(1).Lsh(n);
    return x;
}

internal static T orBit<T>(T x, nuint n)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    x |= ConvertToType<T>(1).Lsh(n);
    return x;
}

internal static T assign<T>()
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T x = GoZero<T>();
    x = ConvertToType<T>(7);
    return x;
}

internal static T pair<T>(T a, T b)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return a + b;
}

internal static T passConst<T>(T x)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return pair(x, ConvertToType<T>(2));
}

internal static T sum<T>()
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T total = GoZero<T>();

    foreach (var (_, v) in slice<T>([ConvertToType<T>(1), ConvertToType<T>(2), ConvertToType<T>(3)])) {
        total += v;
    }

    return total;
}

internal static bool isZero<T>(T x)
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return AreEqual(x, ConvertToType<T>(0));
}

internal static T convert<T>()
    where T : /* integer */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return ConvertToType<T>(4);
}

internal static void Main() {
    arm("return constant", () => fmt.Sprint(one<uint8>(), " ", one<int64>()));
    arm("named constant", () => fmt.Sprint(named<uint32>(), " ", named<int8>()));
    arm("constant shift", () => fmt.Sprint(constShift<uint8>(), " ", constShift<int64>()));
    arm("shift in range", () => fmt.Sprint(bit<uint8>(three), " ", bit<uint32>(three), " ", bit<int8>(three)));
    arm("shift past width", () => fmt.Sprint(bit<uint8>(nine), " ", bit<uint32>(forty), " ", bit<int8>(nine)));
    arm("shift wide type", () => fmt.Sprint(bit<int64>(forty), " ", bit<uint32>(nine)));
    arm("negative count", () => fmt.Sprint(bitSigned<uint8>(neg)));
    arm("mask", () => fmt.Sprint(mask<uint8>(three), " ", mask<uint8>(nine), " ", mask<uint32>(forty)));
    arm("var init", () => fmt.Sprint(varBit<uint8>(three), " ", varBit<uint8>(nine)));
    arm("compound or", () => fmt.Sprint(orBit<uint8>(1, three), " ", orBit<uint8>(1, nine)));
    arm("assign", () => fmt.Sprint(assign<uint8>(), " ", assign<int64>()));
    arm("call argument", () => fmt.Sprint(passConst<uint8>(40), " ", passConst<int8>((int8)(-3))));
    arm("composite elements", () => fmt.Sprint(sum<uint8>(), " ", sum<int64>()));
    arm("comparison", () => fmt.Sprint(isZero<uint8>(0), " ", isZero<int64>(5)));
    arm("conversion", () => fmt.Sprint(convert<uint32>(), " ", convert<int8>()));
}

} // end main_package
