namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct boxedResult {
    internal nint value;
    internal @string tag;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object escapeˢ = (@string)"escape:"u8;
private static readonly @string mutatedˢ = "mutated"u8;
private static readonly object afterˢ = (@string)"after:"u8;

internal static void selectEscape() {
    var ch = new channel<boxedResult>(1);
    ch.ᐸꟷ(new boxedResult(value: 7, tag: "orig"u8));
    ж<boxedResult> saved = default!;
    var selᴛ1 = ch;
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var resᴛ1): {
        ref var res = ref heap(resᴛ1, out var Ꮡres);
        saved = Ꮡres;
        saved.Value.value = 42;
        fmt.Println(escapeˢ, res.value, res.tag);
        res.tag = mutatedˢ;
        break;
    }}
    fmt.Println(afterˢ, (~saved).value, (~saved).tag);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object commaOkˢ = (@string)"comma-ok:"u8;
private static readonly object fieldˢ = (@string)"field:"u8;

internal static void selectEscapeCommaOk() {
    var ch = new channel<boxedResult>(1);
    ch.ᐸꟷ(new boxedResult(value: 3, tag: "ok-form"u8));
    ж<boxedResult> whole = default!;
    ж<nint> field = default!;
    var selᴛ2 = ch;
    switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out var resᴛ2, out var ok): {
        ref var res = ref heap(resᴛ2, out var Ꮡres);
        whole = Ꮡres;
        field = Ꮡres.of(boxedResult.Ꮡvalue);
        field.Value += 10;
        fmt.Println(commaOkˢ, res.value, res.tag, ok);
        break;
    }}
    fmt.Println(fieldˢ, field.Value, (~whole).value);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string escapedˢ = "escaped"u8;
private static readonly object mixedˢ = (@string)"mixed:"u8;
private static readonly object plainˢ = (@string)"plain:"u8;

internal static void selectEscapeMixed() {
    var a = new channel<boxedResult>(1);
    var b = new channel<nint>(1);
    a.ᐸꟷ(new boxedResult(value: 1, tag: "x"u8));
    ж<boxedResult> keep = default!;
    var selᴛ3 = a;
    var selᴛ4 = b;
    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out var rᴛ1): {
        ref var r = ref heap(rᴛ1, out var Ꮡr);
        keep = Ꮡr;
        keep.Value.tag = escapedˢ;
        fmt.Println(mixedˢ, r.tag);
        break;
    }
    case 1 when selᴛ4.ꟷᐳ(out var n): {
        fmt.Println(plainˢ, n);
        break;
    }}
}

internal static void Main() {
    selectEscape();
    selectEscapeCommaOk();
    selectEscapeMixed();
}

} // end main_package
