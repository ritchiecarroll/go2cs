namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct stringID /*num:uint64*/;

partial struct offset /*num:int32*/;

internal static K mix<K>(K id, K step)
    where K : /* ~uint64 | ~int32 */ IAdditionOperators<K, K, K>, ISubtractionOperators<K, K, K>, IMultiplyOperators<K, K, K>, IDivisionOperators<K, K, K>, IIncrementOperators<K>, IDecrementOperators<K>, IUnaryNegationOperators<K, K>, IModulusOperators<K, K, K>, IBitwiseOperators<K, K, K>, IShiftOperators<K, int, K>, IEqualityOperators<K, K, bool>, IComparisonOperators<K, K, bool>, new()
{
    var h = id;
    h = (K)(h ^ ((h >> (int)(2))));
    h = (K)(h | ((step << (int)(1))));
    h = (K)(h & step);
    h = rem(h, (step + ConvertToType<K>(7)));
    return h;
}

internal static void Main() {
    stringID a = 12345;
    stringID b = 11;
    fmt.Println(mix(a, b));
    offset x = -8;
    offset y = 5;
    fmt.Println(mix(x, y));
    fmt.Println(mix(((stringID)255), ((stringID)3)), mix(((offset)100), ((offset)9)));
}

} // end main_package
