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

partial struct Point /*[]int32*/;

public static @string String(this Point p) {
    return fmt.Sprintf("%d"u8, p);
}

public static S Scale<S, E>(S s, E c)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IIncrementOperators<E>, IDecrementOperators<E>, IUnaryNegationOperators<E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, int, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    var r = make<S>(len(s));
    foreach (var (i, v) in s) {
        r[i] = v * c;
    }
    return r;
}

public static void ScaleAndPrint(Point p) {
    var r = Scale(p, (int32)(2));
    fmt.Println(r.String());
}

public static S Twice<S, E>(S s, E c)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IIncrementOperators<E>, IDecrementOperators<E>, IUnaryNegationOperators<E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, int, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    return Scale(s, c);
}

public static (E, E) CopyClearMinMax<S, E>(S dst, S src)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IIncrementOperators<E>, IDecrementOperators<E>, IUnaryNegationOperators<E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, int, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    copy(dst, src);
    var lo = min(dst[0], src[1]);
    var hi = max(dst[0], src[1]);
    clear(src);
    return (lo, hi);
}

public static E SumHalves<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IIncrementOperators<E>, IDecrementOperators<E>, IUnaryNegationOperators<E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, int, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    if (len(s) == 1) {
        s[0] += s[0];
        return s[0];
    }
    nint mid = len(s) / 2;
    return SumHalves<S, E>(subslice<S, E>(s, 0, mid)) + SumHalves<S, E>(subslice<S, E>(s, mid));
}

public static S AppendKeep<S, E>(S s, E v)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IIncrementOperators<E>, IDecrementOperators<E>, IUnaryNegationOperators<E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, int, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    var @out = append(s, v);
    return @out;
}

internal static void setFirst<E>(slice<E> s, E v)
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IIncrementOperators<E>, IDecrementOperators<E>, IUnaryNegationOperators<E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, int, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    if (len(s) > 0) {
        s[0] = v;
    }
}

public static void PassSlice<S, E>(S s, E v)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IIncrementOperators<E>, IDecrementOperators<E>, IUnaryNegationOperators<E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, int, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    setFirst(new slice<E>(s), v);
}

partial struct Grades /*map[@string, nint]*/;

public static bool EqualMaps<M, K, V>(M m1, M m2)
    where M : /* ~map[K]V */ IMap<K, V>, ISupportMake<M>, new()
{
    if (len(m1) != len(m2)) {
        return false;
    }
    foreach (var (k, v1) in m1) {
        {
            var (v2, ok) = m2[k, ꟷ]; if (!ok || !AreEqual(v1, v2)) {
                return false;
            }
        }
    }
    return true;
}

public delegate void Seq<V>(Func<V, bool> yield);

public delegate void KVSeq<K, V>(Func<K, V, bool> yield);

internal static Seq<nint> countdown(nint n) {
    return (Func<nint, bool> yield) => {
        for (nint i = n; i > 0; i--) {
            if (!yield(i)) {
                return;
            }
        }
    };
}

internal static KVSeq<@string, nint> letters() {
    return (Func<@string, nint, bool> yield) => {
        _ = yield("a"u8, 1) && yield("b"u8, 2);
    };
}

public static M CloneOrNil<M, K, V>(M m)
    where M : /* ~map[K]V */ IMap<K, V>, ISupportMake<M>, new()
{
    if (m.IsNil) {
        return default!;
    }
    var @out = make<M>(len(m));
    foreach (var (k, v) in m) {
        @out[k] = v;
    }
    return @out;
}

public static void DropKey<M, K, V>(M m, K key)
    where M : /* ~map[K]V */ IMap<K, V>, ISupportMake<M>, new()
{
    delete(m, key);
}

internal static void Main() {
    Point p = default!;
    p = new int32[]{1, 2, 3}.slice();
    ScaleAndPrint(p);
    fmt.Println(Twice(new Point(new int32[]{3, 4}.slice()), (int32)(2)).String());
    var (dst2, src2) = (new Point(new int32[]{0, 0, 0}.slice()), new Point(new int32[]{7, 8, 9}.slice()));
    var (lo, hi) = CopyClearMinMax<Point, int32>(dst2, src2);
    fmt.Println(dst2.String(), src2.String(), lo, hi);
    var q = new Point(new int32[]{1, 2, 3, 4}.slice());
    var total = SumHalves<Point, int32>(q);
    fmt.Println(q.String(), total);
    var grown = AppendKeep(new Point(new int32[]{5, 6}.slice()), (int32)(7));
    fmt.Println(grown.String());
    var pt = new Point(new int32[]{1, 2}.slice());
    PassSlice(pt, (int32)(42));
    fmt.Println(pt.String());
    var g1 = new Grades(new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2});
    var g2 = new Grades(new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2});
    var g3 = new Grades(new map<@string, nint>{["a"u8] = 9});
    fmt.Println(EqualMaps<Grades, @string, nint>(g1, g2), EqualMaps<Grades, @string, nint>(g1, g3));
    nint sum = 0;
    foreach (var v in range<nint>(countdown(5).Invoke)) {
        if (v == 2) {
            break;
        }
        sum += v;
    }
    fmt.Println(sum);
    foreach (var (k, v) in range<@string, nint>(letters().Invoke)) {
        fmt.Println(k, v);
    }
    Grades nilG = default!;
    fmt.Println(CloneOrNil<Grades, @string, nint>(nilG) == default!);
    var cl = CloneOrNil<Grades, @string, nint>(g1);
    fmt.Println(len(cl), cl["a"u8]);
    DropKey<Grades, @string, nint>(g1, (@string)"a");
    fmt.Println(len(g1));
    var (i16, ok16) = bsearchLike(new uint16[]{2, 4, 8}.slice(), (uint16)4);
    var (i32, ok32) = bsearchLike(new uint32[]{10, 20, 30}.slice(), (uint32)25);
    fmt.Println(i16, ok16, i32, ok32, halve((uint16)6), halve((uint32)100));
    fmt.Println(halveN((int32)10), halveN((int64)(-3)));
    var ssum = (int64)0;
    foreach (var v in range<int64>(seqOf<int64, int32>((int32)4).Invoke)) {
        ssum += v;
    }
    fmt.Println(ssum);
    fmt.Println(growShrink((uintptr)1, (uintptr)20), growShrink((uint32)2, (uint32)9));
}

internal static U growShrink<U>(U v, U lim)
    where U : /* ~uint32 | ~uintptr */ IAdditionOperators<U, U, U>, ISubtractionOperators<U, U, U>, IMultiplyOperators<U, U, U>, IDivisionOperators<U, U, U>, IIncrementOperators<U>, IDecrementOperators<U>, IUnaryNegationOperators<U, U>, IModulusOperators<U, U, U>, IBitwiseOperators<U, U, U>, IShiftOperators<U, int, U>, IEqualityOperators<U, U, bool>, IComparisonOperators<U, U, bool>, new()
{
    while (v < lim) {
        v = v * ConvertToType<U>(2) + ConvertToType<U>(1);
    }
    return ((v % lim) >> (int)(1));
}

internal static Seq<T> seqOf<T, N>(N n)
    where T : /* ~int64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
    where N : /* ~int32 | ~int64 */ IAdditionOperators<N, N, N>, ISubtractionOperators<N, N, N>, IMultiplyOperators<N, N, N>, IDivisionOperators<N, N, N>, IIncrementOperators<N>, IDecrementOperators<N>, IUnaryNegationOperators<N, N>, IModulusOperators<N, N, N>, IBitwiseOperators<N, N, N>, IShiftOperators<N, int, N>, IEqualityOperators<N, N, bool>, IComparisonOperators<N, N, bool>, new()
{
    return (Func<T, bool> yield) => {
        for (var i = ConvertToType<T>(0); i < ConvertToType<T>(ConvertToUInt64<N>(n)); i++) {
            if (!yield(i)) {
                return;
            }
        }
    };
}

internal static (nint, bool) bsearchLike<S, E>(S s, E v)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* ~uint16 | ~uint32 */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IIncrementOperators<E>, IDecrementOperators<E>, IUnaryNegationOperators<E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, int, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    nint n = len(s);
    nint i = 0;
    nint j = n;
    while (i < j) {
        nint h = i + ((j - i) >> (int)(1));
        if (s[h] < v){
            i = h + 1;
        } else {
            j = h;
        }
    }
    return (i, i < n && AreEqual(s[i], v));
}

internal static Int halveN<Int>(Int n)
    where Int : /* ~int32 | ~int64 */ IAdditionOperators<Int, Int, Int>, ISubtractionOperators<Int, Int, Int>, IMultiplyOperators<Int, Int, Int>, IDivisionOperators<Int, Int, Int>, IIncrementOperators<Int>, IDecrementOperators<Int>, IUnaryNegationOperators<Int, Int>, IModulusOperators<Int, Int, Int>, IBitwiseOperators<Int, Int, Int>, IShiftOperators<Int, int, Int>, IEqualityOperators<Int, Int, bool>, IComparisonOperators<Int, Int, bool>, new()
{
    if (n <= ConvertToType<Int>(0)) {
        return n;
    }
    return ConvertToType<Int>(ConvertToUInt64<Int>(n) / 2);
}

internal static E halve<E>(E v)
    where E : /* ~uint16 | ~uint32 */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IIncrementOperators<E>, IDecrementOperators<E>, IUnaryNegationOperators<E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, int, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    return (v >> (int)(1));
}

} // end main_package
