namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct P {
    internal nint x;
}

internal static @string show<E>(E e) {
    return fmt.Sprintf("%T %v"u8, (e).OrTypedNilOf(), (e).OrTypedNilOf());
}

internal static any box<E>(E e) {
    return (e).OrTypedNilOf();
}

internal static bool isNil<E>(E e) {
    return ((any)(e).OrTypedNilOf()) == default!;
}

internal static any assign<E>(E e) {
    any x = default!;
    x = (e).OrTypedNilOf();
    return x;
}

internal static slice<any> slot<E>(E e) {
    return new any[]{(e).OrTypedNilOf()}.slice();
}

internal static any send<E>(E e) {
    var c = new channel<any>(1);
    c.ᐸꟷ((e).OrTypedNilOf());
    return ᐸꟷ(c);
}

partial interface compError :
    error
{
}

internal static @string nameOf<E>(E want) {
    return fmt.Sprintf("%T"u8, (want).OrTypedNilOf());
}

partial struct myErr {
}

internal static @string Error(this ref myErr _) {
    return "myErr";
}

internal static any plain<T>(T t)
    where T : /* ~int | ~string */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return t;
}

internal static void Main() {
    ж<P> p = default!;
    Action f = default!;
    error e = default!;

    fmt.Println(show(p));
    fmt.Println(box(p) == default!, isNil(p), assign(p) == default!, slot(p)[0] == default!, send(p) == default!);
    fmt.Printf("%T %T\n"u8, box(p), assign(p));
    fmt.Println(fmt.Sprintf("%T"u8, box((f).OrTypedNilFunc())), box((f).OrTypedNilFunc()) == default!, isNil((f).OrTypedNilFunc()));
    fmt.Println(nameOf<ж<myErr>>(nil));
    fmt.Println(box(e) == default!, isNil(e));
    fmt.Println(show(Ꮡ(new P(1))), plain<nint>(3), plain((@string)"s"));
}

} // end main_package
