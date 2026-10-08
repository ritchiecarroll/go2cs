namespace go;

using fmt = fmt_package;
using Δmath = math_package;

partial class main_package {

partial interface Interface {
    nint Len();
    bool Less(nint i, nint j);
    void Swap(nint i, nint j);
}

partial struct Float64Slice /*[]float64*/;

public static nint Len(this Float64Slice x) {
    return len(x);
}

public static bool Less(this Float64Slice x, nint i, nint j) {
    return x[i] < x[j] || (fisNaN(x[i]) && !fisNaN(x[j]));
}

public static void Swap(this Float64Slice x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}

internal static bool fisNaN(float64 f) {
    return f != f;
}

partial struct reverse {
    /*embed*/ public Interface Interface;
}

internal static bool Less(this reverse r, nint i, nint j) {
    return r.Interface.Less(j, i);
}

public static Interface Reverse(Interface data) {
    return new reverseжInterface(Ꮡ(new reverse(data)));
}

internal static void insertionSort(Interface data) {
    nint n = data.Len();
    for (nint i = 1; i < n; i++) {
        for (nint j = i; j > 0 && data.Less(j, j - 1); j--) {
            data.Swap(j, j - 1);
        }
    }
}

partial interface ordered<ΔT> /*operators = Sum, Comparable, Ordered*/ {
    //  Type constraints: ~int | ~float32 | ~float64 | ~string
    // Derived operators: +, ==, !=, <, <=, >, >=
}

internal static bool isNaN<T>(T x)
    where T : /* ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return !AreEqual(x, x);
}

internal static bool less<T>(T x, T y)
    where T : /* ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return (isNaN(x) && !isNaN(y)) || x < y;
}

internal static bool eq<T>(T a, T b) {
    return AreEqual(a, b);
}

internal static void Main() {
    var nan = Δmath.NaN();
    var ints = new Float64Slice(new float64[]{74D, 59D, 238D, -784D, 9845D, 959D, 905D, 0D, 42D}.slice());
    insertionSort(Reverse(ints));
    fmt.Println(ints);
    var floats = new Float64Slice(new float64[]{74.3D, 59.0D, Δmath.Inf(1), 238.2D, -784.0D, 2.3D, nan, nan, Δmath.Inf(-1), 905D}.slice());
    insertionSort(floats);
    fmt.Println(floats);
    fmt.Println(isNaN(nan), isNaN(1.5D), isNaN((float32)Δmath.NaN()), isNaN(7), isNaN((@string)"x"));
    fmt.Println(less(nan, -784.0D), less(-784.0D, nan), less(nan, nan), less(1.0D, 2.0D));
    fmt.Println(eq(nan, nan), eq(1.5D, 1.5D));
    fmt.Println(eq((float32)Δmath.NaN(), (float32)Δmath.NaN()), eq((float32)1.5F, (float32)1.5F));
    fmt.Println(eq(complex(nan, 0D), complex(nan, 0D)), eq(complex(1D, 2D), complex(1D, 2D)));
    fmt.Println(eq((complex64)complex(nan, 0D), (complex64)complex(nan, 0D)), eq((complex64)complex(1F, 2F), (complex64)complex(1F, 2F)));
    any bx = nan;
    any by = nan;
    fmt.Println(AreEqual(bx, by), AreEqual(((any)1.5D), ((any)1.5D)));
}

} // end main_package
