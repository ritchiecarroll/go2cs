namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct box {
    internal nint n;
}

internal static nint get(this box b) {
    return b.n;
}

[GoRecv] internal static void bump(this ref box b, nint d) {
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object funcLiteralsˢ = (@string)"func literals:"u8;
private static readonly object methodGroupsˢ = (@string)"method groups:"u8;
private static readonly object methodValuesˢ = (@string)"method values:"u8;
private static readonly object funcLiteralBesideAˢ = (@string)"func literal beside a pointer:"u8;
private static readonly object genericInstantiationsˢ = (@string)"generic instantiations:"u8;

internal static void Main() {
    nint n = 3;
    var small = (nint v) => v < n;
    var even = (nint v) => v % 2 == 0;
    fmt.Println(funcLiteralsˢ, small(2), even(2), small(5), even(5));
    var d = @double;
    var t = triple;
    fmt.Println(methodGroupsˢ, d(2), t(2));
    var b = Ꮡ(new box(n: 2));
    var recvʗ1 = ~b;
    
    var bʗ1 = b;
    var get = () => recvʗ1.get();
    
    var bʗ2 = bʗ1;
    var bump = (nint p1) => bʗ2.bump(p1);
    bump(3);
    fmt.Println(methodValuesˢ, get(), (~b).n);
    var show = nint () => 1;
    var p = b;
    fmt.Println(funcLiteralBesideAˢ, show(), (~p).n);
    var id = identity<@string>;
    var sq = square<float64>;
    fmt.Println(genericInstantiationsˢ, id("go"u8), sq(1.5D));
}

} // end main_package
