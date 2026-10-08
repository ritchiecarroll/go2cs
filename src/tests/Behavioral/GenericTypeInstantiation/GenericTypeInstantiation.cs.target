namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Stack<T>
    where T : /* ~int | ~string */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    internal slice<T> elements;
}

public static void Push<T>(this ref Stack<T> s, T element)
    where T : /* ~int | ~string */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    s.elements = append(s.elements, element);
}

public static (T, bool) Pop<T>(this ref Stack<T> s)
    where T : /* ~int | ~string */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T zero = GoZero<T>();
    if (len(s.elements) == 0) {
        return (zero, false);
    }
    nint index = len(s.elements) - 1;
    var element = s.elements[index];
    s.elements = s.elements.slice(0, index);
    return (element, true);
}

public static slice<U> MapElements<T, U>(ж<Stack<T>> Ꮡs, Func<T, U> mapper)
    where T : /* ~int */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    ref var s = ref Ꮡs.DerefOrNull();

    var result = new slice<U>(0, len(s.elements));
    foreach (var (_, elem) in s.elements) {
        result = append(result, mapper(elem));
    }
    return result;
}

public delegate bool Seq2Like<K, V>(K _Δp0, V _Δp1);

internal static bool consume<K, V>(Seq2Like<K, V> s, K k, V v) {
    return s(k, v);
}

internal static @string describe<T>(@string label) {
    T zero = GoZero<T>();
    _ = zero;
    return label;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloˢ = "hello"u8;
private static readonly @string worldˢ = "world"u8;
private static readonly @string fourˢ = "four"u8;
private static readonly @string nopeˢ = "nope"u8;
private static readonly @string stringerˢ = "stringer"u8;
private static readonly @string ptrIntˢ = "ptr-int"u8;

internal static void Main() {
    var intStack = new Stack<nint>(nil);
    intStack.Push(10);
    intStack.Push(20);
    var (val, _) = intStack.Pop();
    fmt.Printf("Popped from int stack: %d\n"u8, val);
    var stringStack = new Stack<@string>(nil);
    stringStack.Push(helloˢ);
    stringStack.Push(worldˢ);
    var (text, _) = stringStack.Pop();
    fmt.Printf("Popped from string stack: %s\n"u8, text);
    var pair = new Seq2Like<@string, nint>((@string k, nint v) => len(k) == v);
    fmt.Println(consume(pair, fourˢ, 4), pair(nopeˢ, 3));
    fmt.Println(describe<fmt.Stringer>(stringerˢ));
    fmt.Println(describe<ж<nint>>(ptrIntˢ));
}

} // end main_package
