namespace go;

using fmt = fmt_package;

partial class main_package {

internal static T negate<T>(T x)
    where T : /* int | uint | int32 | uint32 | int64 | uint64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return -x;
}

internal static T negateSum<T>(T a, T b)
    where T : /* int | uint | int32 | uint32 | int64 | uint64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return -(a + b);
}

partial struct counter /*num:uint64*/;

partial struct offset /*num:int32*/;

internal static T negateNamed<T>(T x)
    where T : /* ~uint64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return -x;
}

internal static T negateNamedSigned<T>(T x)
    where T : /* ~int32 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return -x;
}

internal static void Main() {
    fmt.Println(negate(5));
    fmt.Println(negate((int32)(-7)));
    fmt.Println(negate((int64)(1099511627776L)));
    fmt.Println(negate((uint32)1));
    fmt.Println(negate((uint64)0));
    fmt.Println(negate((uint64)1));
    fmt.Println(negate((nuint)3));
    fmt.Println(negateSum(10, 32));
    fmt.Println(negateSum((uint32)2, (uint32)3));
    fmt.Println(negateNamed(((counter)0)));
    fmt.Println(negateNamed(((counter)1)));
    fmt.Println(negateNamed((counter)((uint64)1 << (int)(63))));
    fmt.Println(negateNamedSigned(((offset)9)));
    fmt.Println(negateNamedSigned(((offset)(-9))));
    fmt.Println(negate(negate((uint64)12345)));
    fmt.Println(negateNamed(negateNamed(((counter)999))));
}

} // end main_package
