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
    Func<nint, nint> zero = default;

    var argSlot = take((zero).OrTypedNilFunc());

    var elemSlot = slice<any>([(zero).OrTypedNilFunc()]);

    var fieldSlot = slice<row>([new((zero).OrTypedNilFunc(), true)]);

    var keyedSlot = new map<@string, any>{["k"] = (zero).OrTypedNilFunc()};

    var appendSlot = append(slice<any>([]), (any)((zero).OrTypedNilFunc()));

    fmt.Println("arg    ", argSlot == nil);
    fmt.Println("elem   ", elemSlot[0] == nil);
    fmt.Println("field  ", fieldSlot[0].v == nil);
    fmt.Println("keyed  ", keyedSlot["k"] == nil);
    fmt.Println("append ", appendSlot[0] == nil);

    fmt.Println("conv   ", take(((Action)(default)).OrTypedNilFunc()) == nil);

    holder h = default;

    fmt.Println("declared", take(declared) != nil);
    fmt.Println("qualified", take(fmt.Sprint) != nil);
    var hʗ1 = h;
    fmt.Println("methodval", take(() => hʗ1.method()) != nil);
    fmt.Println("literal ", take(() => {
    }) != nil);

    fmt.Println("fieldfn ", take((h.fn).OrTypedNilFunc()) == nil);

    h.fn = declared;
    fmt.Println("fieldset", take((h.fn).OrTypedNilFunc()) == nil);
}

} // end main_package
