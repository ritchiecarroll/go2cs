namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct box {
    internal nint n;
}

internal static nint get(this box b) {
    return b.n;
}

internal static void bump(this ref box b, nint d) {
    b.n += d;
}

internal static nint @double(nint v) {
    return v * 2;
}

internal static nint triple(nint v) {
    return v * 3;
}

internal static T identity<T>(T v) {
    return v;
}

internal static T square<T>(T v)
    where T : /* ~int | ~float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return v * v;
}

internal static void Main() {
    nint n = 3;

    var small = (nint v) => v < n;
    var even = (nint v) => v % 2 == 0;
    fmt.Println("func literals:", small(2), even(2), small(5), even(5));

    var d = @double;
    var t = triple;
    fmt.Println("method groups:", d(2), t(2));

    var b = Ꮡ(new box(n: 2));
    var recvʗ1 = ~b;
    
    var bʗ1 = b;
    var get = () => recvʗ1.get();
    
    var bʗ2 = bʗ1;
    var bump = (nint p1) => bʗ2.bump(p1);
    bump(3);
    fmt.Println("method values:", get(), (~b).n);

    var show = nint () => 1;
    var p = b;
    fmt.Println("func literal beside a pointer:", show(), (~p).n);

    var id = identity<@string>;
    var sq = square<float64>;
    fmt.Println("generic instantiations:", id("go"), sq(1.5D));
}

} // end main_package
