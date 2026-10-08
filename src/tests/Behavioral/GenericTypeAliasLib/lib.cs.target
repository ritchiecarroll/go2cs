global using Word = go.@string;
global using IntBox = go.GenericTypeAliasLib_package.Box<nint>;

namespace go;

partial class GenericTypeAliasLib_package {

partial struct Box<T> {
    public T V;
}

public static T Get<T>(this Box<T> b) {
    return b.V;
}

public static void Set<T>(this ref Box<T> b, T v) {
    b.V = v;
}

public static Box<T> NewBox<T>(T v) {
    return new Box<T>(V: v);
}
// type Alias[T any] = Box[T]
// type Set[T comparable] = map[T]struct{}
// type List[T any] = []T
// type Mapper[T any] = func(T) T
// type Ptr[T any] = *Box[T]

partial interface Number<ΔT> /*operators = Sum, Arithmetic, Comparable, Ordered*/ {
    //  Type constraints: ~int | ~float64
    // Derived operators: +, -, *, /, ==, !=, <, <=, >, >=
}
// type NumBox[T Number] = Box[T]

public static T Sum<T>(slice<T> xs)
    where T : /* Number */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T s = GoZero<T>();
    foreach (var (_, x) in xs) {
        s += x;
    }
    return s;
}

public static T Twice<T>(Func<T, T> m, T v) {
    return m(m(v));
}

public static Word Label(Word w) {
    return "<"u8 + w + ">"u8;
}

public static nint Unbox(IntBox b) {
    return b.Get();
}

} // end GenericTypeAliasLib_package
