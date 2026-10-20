namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt named => 7;

internal static @string show<E>(E e) {
    return fmt.Sprintf("%T %v"u8, (e).OrTypedNilOf(), (e).OrTypedNilOf());
}

internal static @string pair<E>(E a, E b) {
    return fmt.Sprintf("%T"u8, (b).OrTypedNilOf());
}

internal static @string many<E>(params Span<E> esʗp) {
    var es = esʗp.slice();

    return fmt.Sprintf("%T %d"u8, es, len(es));
}

internal static @string keyed<K, V>(K k, V v) {
    return fmt.Sprintf("%T %T"u8, (k).OrTypedNilOf(), (v).OrTypedNilOf());
}

internal static void Main() {
    fmt.Println(show<nint>(3));
    fmt.Println(show(2.5D));
    fmt.Println(show((rune)'a'));
    fmt.Println(show(1D + 2D.i()));
    fmt.Println(show<nint>(named));
    fmt.Println(show<nint>(1 << 10));
    fmt.Println(show((@string)"s"));
    fmt.Println(show(true));
    fmt.Println(many<nint>(1, 2, 3));
    fmt.Println(keyed<@string, nint>((@string)"k", 4));

    int8 x = 1;
    fmt.Println(pair(x, (int8)3));
    fmt.Println(show(x));
    fmt.Println(show((int16)5));
}

} // end main_package
