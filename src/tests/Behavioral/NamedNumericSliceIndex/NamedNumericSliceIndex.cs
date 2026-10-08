namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct PID /*num:int64*/;

partial struct rank /*num:nint*/;

internal static (@string, @string) lookup<K>(slice<@string> dense, K id)
    where K : /* ~uint64 */ IAdditionOperators<K, K, K>, ISubtractionOperators<K, K, K>, IMultiplyOperators<K, K, K>, IDivisionOperators<K, K, K>, IIncrementOperators<K>, IDecrementOperators<K>, IUnaryNegationOperators<K, K>, IModulusOperators<K, K, K>, IBitwiseOperators<K, K, K>, IShiftOperators<K, int, K>, IEqualityOperators<K, K, bool>, IComparisonOperators<K, K, bool>, new()
{
    return (dense[(nint)(ConvertToUInt64<K>(id))], dense[(nint)(ConvertToUInt64<K>(id / ConvertToType<K>(2)))]);
}

internal static uint8 bitset<K>(K id)
    where K : /* ~uint64 */ IAdditionOperators<K, K, K>, ISubtractionOperators<K, K, K>, IMultiplyOperators<K, K, K>, IDivisionOperators<K, K, K>, IIncrementOperators<K>, IDecrementOperators<K>, IUnaryNegationOperators<K, K>, IModulusOperators<K, K, K>, IBitwiseOperators<K, K, K>, IShiftOperators<K, int, K>, IEqualityOperators<K, K, bool>, IComparisonOperators<K, K, bool>, new()
{
    return (uint8)((uint8)1 << (int)(ConvertToUInt64<K>((id % ConvertToType<K>(8)))));
}

internal static (@string, @string) pick(slice<@string> spans, PID p) {
    return (spans[(nint)(p)], spans[(nint)(p - 1)]);
}

internal static void Main() {
    var dense = new @string[]{"a"u8, "b"u8, "c"u8, "d"u8, "e"u8, "f"u8}.slice();
    var (x, y) = lookup(dense, (uint64)5);
    fmt.Println(x, y);
    fmt.Println(bitset((uint64)3), bitset((uint64)10));
    var spans = new @string[]{"zero"u8, "one"u8, "two"u8, "three"u8}.slice();
    var (a, b) = pick(spans, ((PID)3));
    fmt.Println(a, b);
    var names = new @string[]{"lo"u8, "mid"u8, "hi"u8}.slice();
    rank r = 2;
    fmt.Println(names[r]);
}

} // end main_package
