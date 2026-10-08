namespace go;

using fmt = fmt_package;

partial class main_package {

public static nint Index<S, E>(S s, E v)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    foreach (var (i, _) in s) {
        if (AreEqual(s[i], v)) {
            return i;
        }
    }
    return -1;
}

internal static S appendAll<S, E>(S s, params Span<E> vʗp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    var v = vʗp.sslice();

    return appendꓸꓸꓸ<S, E>(s, v);
}

partial struct numbers /*[]nint*/;

internal static void Main() {
    var ints = new nint[]{10, 20, 30, 40}.slice();
    fmt.Println(Index(ints, (nint)(20)));
    fmt.Println(Index(ints, (nint)(99)));
    var ns = new numbers(new nint[]{5, 6, 7}.slice());
    fmt.Println(Index(ns, (nint)(7)));
    var bs = new byte[]{1, 2, 3}.slice();
    fmt.Println(Index(bs, (byte)(2)));
    var rs = new int32[]{100, 200, 300}.slice();
    fmt.Println(Index(rs, (int32)(200)));
    fmt.Println(appendAll(ints, (nint)(50), (nint)(60)));
    fmt.Println(appendAll(bs, (byte)(4), (byte)(5)));
    fmt.Println(appendAll(new numbers(new nint[]{1}.slice()), (nint)(2)));
}

} // end main_package
