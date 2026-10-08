namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct holder {
    internal Func<nint, nint> fn;
}

internal static void method(this holder h) {
}

partial struct row {
    internal any v;
    internal bool ok;
}

internal static any take(any v) {
    return v;
}

internal static nint declared(nint x) {
    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object argˢ = (@string)"arg    "u8;
private static readonly object elemˢ = (@string)"elem   "u8;
private static readonly object fieldˢ = (@string)"field  "u8;
private static readonly object keyedˢ = (@string)"keyed  "u8;
private static readonly object appendˢ = (@string)"append "u8;
private static readonly object convˢ = (@string)"conv   "u8;
private static readonly object declaredˢ = (@string)"declared"u8;
private static readonly object qualifiedˢ = (@string)"qualified"u8;
private static readonly object methodvalˢ = (@string)"methodval"u8;
private static readonly object literalˢ = (@string)"literal "u8;
private static readonly object fieldfnˢ = (@string)"fieldfn "u8;
private static readonly object fieldsetˢ = (@string)"fieldset"u8;

internal static void Main() {
    Func<nint, nint> zero = default!;
    var argSlot = take((zero).OrTypedNilFunc());
    var elemSlot = new any[]{(zero).OrTypedNilFunc()}.slice();
    var fieldSlot = new row[]{new((zero).OrTypedNilFunc(), true)}.slice();
    var keyedSlot = new map<@string, any>{["k"u8] = (zero).OrTypedNilFunc()};
    var appendSlot = append(new any[]{}.slice(), (any)((zero).OrTypedNilFunc()));
    fmt.Println(argˢ, argSlot == default!);
    fmt.Println(elemˢ, elemSlot[0] == default!);
    fmt.Println(fieldˢ, fieldSlot[0].v == default!);
    fmt.Println(keyedˢ, keyedSlot["k"u8] == default!);
    fmt.Println(appendˢ, appendSlot[0] == default!);
    fmt.Println(convˢ, take(((Action)(default!)).OrTypedNilFunc()) == default!);
    holder h = default!;
    fmt.Println(declaredˢ, take(declared) != default!);
    fmt.Println(qualifiedˢ, take(fmt.Sprint) != default!);
    var hʗ1 = h;
    fmt.Println(methodvalˢ, take(() => hʗ1.method()) != default!);
    fmt.Println(literalˢ, take(() => {
    }) != default!);
    fmt.Println(fieldfnˢ, take((h.fn).OrTypedNilFunc()) == default!);
    h.fn = declared;
    fmt.Println(fieldsetˢ, take((h.fn).OrTypedNilFunc()) == default!);
}

} // end main_package
