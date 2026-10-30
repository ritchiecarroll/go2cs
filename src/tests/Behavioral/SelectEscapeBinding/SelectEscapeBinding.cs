namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct boxedResult {
    internal nint value;
    internal @string tag;
}

internal static void selectEscape() {
    var ch = new channel<boxedResult>(1);
    ch.ᐸꟷ(new boxedResult(value: 7, tag: "orig"));

    ж<boxedResult> saved = default;

    var selᴛ1 = ch;
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var resᴛ1): {
        ref var res = ref heap(resᴛ1, out var Ꮡres);
        saved = Ꮡres;
        saved.value = 42;
        fmt.Println("escape:", res.value, res.tag);
        res.tag = "mutated";
        break;
    }}

    fmt.Println("after:", saved.value, saved.tag);
}

internal static void selectEscapeCommaOk() {
    var ch = new channel<boxedResult>(1);
    ch.ᐸꟷ(new boxedResult(value: 3, tag: "ok-form"));

    ж<boxedResult> whole = default;
    ж<nint> field = default;

    var selᴛ2 = ch;
    switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out var resᴛ2, out var ok): {
        ref var res = ref heap(resᴛ2, out var Ꮡres);
        whole = Ꮡres;
        field = Ꮡres.Ꮡvalue;
        field.Value += 10;
        fmt.Println("comma-ok:", res.value, res.tag, ok);
        break;
    }}

    fmt.Println("field:", field.Value, whole.value);
}

internal static void selectEscapeMixed() {
    var a = new channel<boxedResult>(1);
    var b = new channel<nint>(1);
    a.ᐸꟷ(new boxedResult(value: 1, tag: "x"));

    ж<boxedResult> keep = default;

    var selᴛ3 = a;
    var selᴛ4 = b;
    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out var rᴛ1): {
        ref var r = ref heap(rᴛ1, out var Ꮡr);
        keep = Ꮡr;
        keep.tag = "escaped";
        fmt.Println("mixed:", r.tag);
        break;
    }
    case 1 when selᴛ4.ꟷᐳ(out var n): {
        fmt.Println("plain:", n);
        break;
    }}
}

internal static void Main() {
    selectEscape();
    selectEscapeCommaOk();
    selectEscapeMixed();
}

} // end main_package
