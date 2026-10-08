namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void reverse<E>(slice<E> s) {
    for ((nint i, nint j) = (0, len(s) - 1); i < j; (i, j) = (i + 1, j - 1)) {
        (s[i], s[j]) = (s[j], s[i]);
    }
}

internal static void reverseSeq<S, E>(S x)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    reverse(new slice<E>(x));
}

internal static void explicitReverseSeq<S, E>(S x)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    reverse(new slice<E>(x));
}

internal static void insertionSort<E>(slice<E> data, Func<E, E, bool> less) {
    for (nint i = 1; i < len(data); i++) {
        for (nint j = i; j > 0 && less(data[j], data[j - 1]); j--) {
            (data[j], data[j - 1]) = (data[j - 1], data[j]);
        }
    }
}

internal static void sortSeq<S, E>(S x, Func<E, E, bool> less)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    insertionSort(new slice<E>(x), less);
}

partial struct numbers /*[]nint*/;

partial struct sortedMap /*[]nint*/;

internal static void Main() {
    var a = new nint[]{1, 2, 3, 4, 5}.slice();
    reverseSeq<slice<nint>, nint>(a);
    fmt.Println(a);
    var b = new numbers(new nint[]{10, 20, 30}.slice());
    reverseSeq<numbers, nint>(b);
    fmt.Println(b);
    var s = new @string[]{"go"u8, "2"u8, "cs"u8}.slice();
    reverseSeq<slice<@string>, @string>(s);
    fmt.Println(s);
    var c = new nint[]{6, 7, 8, 9}.slice();
    explicitReverseSeq<slice<nint>, nint>(c);
    fmt.Println(c);
    var d = new numbers(new nint[]{100, 200}.slice());
    explicitReverseSeq<numbers, nint>(d);
    fmt.Println(d);
    var m = new sortedMap(0, 4);
    m = append(m, (nint)(3), (nint)(1), (nint)(4), (nint)(1));
    sortSeq(m, (nint x, nint y) => x < y);
    fmt.Println(m);
}

} // end main_package
