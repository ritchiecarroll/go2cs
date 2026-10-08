namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint addInt(nint a, nint b) {
    return a + b;
}

internal static E foldSlice<S, E>(S s, Func<E, E, E> combine)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    E acc = GoZero<E>();
    foreach (var (_, v) in s) {
        acc = combine(acc, v);
    }
    return acc;
}

internal static bool equalPair<T>(T a, T b) {
    return AreEqual(a, b);
}

internal static bool pairEqual<S1, S2, E1, E2>(S1 s1, S2 s2, Func<E1, E2, bool> eq)
    where S1 : /* ~[]E1 */ ISlice<E1>, ISupportMake<S1>, ISliceWrap<S1, E1>, new()
    where S2 : /* ~[]E2 */ ISlice<E2>, ISupportMake<S2>, ISliceWrap<S2, E2>, new()
{
    if (len(s1) != len(s2)) {
        return false;
    }
    foreach (var (i, _) in s1) {
        if (!eq(s1[i], s2[i])) {
            return false;
        }
    }
    return true;
}

internal static S insertAt<S, E>(S s, nint i, params Span<E> vʗp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    var v = vʗp.sslice();

    var @out = make<S>(0, len(s) + len(v));
    @out = appendꓸꓸꓸ<S, E>(@out, subslice<S, E>(s, 0, i));
    @out = appendꓸꓸꓸ<S, E>(@out, v);
    @out = appendꓸꓸꓸ<S, E>(@out, subslice<S, E>(s, i));
    return @out;
}

internal static void reverse<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    for ((nint i, nint j) = (0, len(s) - 1); i < j; (i, j) = (i + 1, j - 1)) {
        (s[i], s[j]) = (s[j], s[i]);
    }
}

internal static void applyTo<S>(S v, Action<S> f) {
    f(v);
}

partial struct namedInts /*[]nint*/;

internal static bool sliceEq<S, E>(S a, S b)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    if (len(a) != len(b)) {
        return false;
    }
    foreach (var (i, _) in a) {
        if (!AreEqual(a[i], b[i])) {
            return false;
        }
    }
    return true;
}

partial struct row /*[]nint*/;

internal static bool rowsEqual(slice<row> a, slice<row> b) {
    return pairEqual<slice<row>, slice<row>, row, row>(a, b, sliceEq<row, nint>);
}

internal static void Main() {
    var nums = new nint[]{1, 2, 3, 4}.slice();
    nint sum = foldSlice<slice<nint>, nint>(nums, addInt);
    fmt.Println(sum);
    fmt.Println(pairEqual<slice<nint>, slice<nint>, nint, nint>(nums, new nint[]{1, 2, 3, 4}.slice(), equalPair<nint>));
    fmt.Println(pairEqual<slice<nint>, slice<nint>, nint, nint>(nums, new nint[]{1, 2, 9, 4}.slice(), equalPair<nint>));
    fmt.Println(insertAt<slice<nint>, nint>(nums, 1));
    fmt.Println(insertAt(nums, 1, (nint)(7), (nint)(8)));
    var plain = new nint[]{1, 2, 3}.slice();
    applyTo<slice<nint>>(plain, reverse<slice<nint>, nint>);
    fmt.Println(plain);
    var named = new namedInts(new nint[]{4, 5, 6}.slice());
    applyTo<namedInts>(named, reverse<namedInts, nint>);
    fmt.Println(named);
    fmt.Println(rowsEqual(new row[]{new nint[]{1, 2}.slice(), new nint[]{3}.slice()}.slice(), new row[]{new nint[]{1, 2}.slice(), new nint[]{3}.slice()}.slice()));
    fmt.Println(rowsEqual(new row[]{new nint[]{1, 2}.slice(), new nint[]{3}.slice()}.slice(), new row[]{new nint[]{1, 2}.slice(), new nint[]{4}.slice()}.slice()));
}

} // end main_package
