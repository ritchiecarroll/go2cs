namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Signed<ΔT> /*operators = Sum, Arithmetic, Integer, Comparable, Ordered*/ {
    //  Type constraints: ~int | ~int8 | ~int16 | ~int32 | ~int64
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

partial interface Unsigned<ΔT> /*operators = Sum, Arithmetic, Integer, Comparable, Ordered*/ {
    //  Type constraints: ~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

partial interface Integer<ΔT> /*operators = Sum, Arithmetic, Integer, Comparable, Ordered*/ {
    //  Type constraints: Signed | Unsigned
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

partial interface Float<ΔT> /*operators = Sum, Arithmetic, Comparable, Ordered*/ {
    //  Type constraints: ~float32 | ~float64
    // Derived operators: +, -, *, /, ==, !=, <, <=, >, >=
}

partial interface Ordered<ΔT> /*operators = Sum, Comparable, Ordered*/ {
    //  Type constraints: Integer | Float | ~string
    // Derived operators: +, ==, !=, <, <=, >, >=
}

public static T Min<T>(T a, T b)
    where T : /* Ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    if (a < b) {
        return a;
    }
    return b;
}

public static U Convert<T, U>(T value, Func<T, U> converter) {
    return converter(value);
}

internal static T escape<T>(T x) {
    return x;
}

internal static ж<nint> renew(ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    Ꮡp = escape(Ꮡp); p = ref Ꮡp.DerefOrNull();
    p += 10;
    return Ꮡp;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloˢ = "hello"u8;

internal static void Main() {
    nint minValue = Min<nint>(42, 17);
    fmt.Printf("Min value: %d\n"u8, minValue);
    nint strLength = Convert<@string, nint>(helloˢ, (@string s) => len(s));
    fmt.Printf("String length: %d\n"u8, strLength);
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 5;
    fmt.Println(renew(Ꮡn).Value);
}

} // end main_package
