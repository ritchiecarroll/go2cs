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

internal static void Main() {
    Func<nint, nint> zero = default!;

    var argSlot = take((zero).OrTypedNilFunc());

    var elemSlot = new any[]{(zero).OrTypedNilFunc()}.slice();

    var fieldSlot = new row[]{new((zero).OrTypedNilFunc(), true)}.slice();

    var keyedSlot = new map<@string, any>{["k"] = (zero).OrTypedNilFunc()};

    var appendSlot = append(new any[]{}.slice(), (any)((zero).OrTypedNilFunc()));

    fmt.Println("arg    ", argSlot == default!);
    fmt.Println("elem   ", elemSlot[0] == default!);
    fmt.Println("field  ", fieldSlot[0].v == default!);
    fmt.Println("keyed  ", keyedSlot["k"] == default!);
    fmt.Println("append ", appendSlot[0] == default!);

    fmt.Println("conv   ", take(((Action)(default!)).OrTypedNilFunc()) == default!);

    holder h = default!;

    fmt.Println("declared", take(declared) != default!);
    fmt.Println("qualified", take(fmt.Sprint) != default!);
    var hʗ1 = h;
    fmt.Println("methodval", take(() => hʗ1.method()) != default!);
    fmt.Println("literal ", take(() => {
    }) != default!);

    fmt.Println("fieldfn ", take((h.fn).OrTypedNilFunc()) == default!);

    h.fn = declared;
    fmt.Println("fieldset", take((h.fn).OrTypedNilFunc()) == default!);
}

} // end main_package
