namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilRecvTookCaseˢ = (@string)"nil recv took case"u8;
private static readonly object nilRecvDefaultˢ = (@string)"nil recv default"u8;
private static readonly object nilLenˢ = (@string)"nil len:"u8;
private static readonly object nilCapˢ = (@string)"nil cap:"u8;
private static readonly object nilCommaOkTookCaseˢ = (@string)"nil comma-ok took case"u8;
private static readonly object nilCommaOkDefaultˢ = (@string)"nil comma-ok default"u8;
private static readonly object realRecvTookCaseˢ = (@string)"real recv took case"u8;
private static readonly object realEmptyDefaultˢ = (@string)"real empty default"u8;
private static readonly object realRecvˢ = (@string)"real recv"u8;
private static readonly object realDefaultˢ = (@string)"real default"u8;
private static readonly object mixedTookNilˢ = (@string)"mixed took nil"u8;
private static readonly object mixedTookRealˢ = (@string)"mixed took real"u8;

internal static void Main() {
    channel<nint> nilRecv = default!;

    var selᴛ1 = nilRecv;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var v): {
        fmt.Println(nilRecvTookCaseˢ, v);
        break;
    }
    default: {
        fmt.Println(nilRecvDefaultˢ);
        break;
    }}

    fmt.Println(nilLenˢ, len(nilRecv), nilCapˢ, cap(nilRecv));

    var selᴛ2 = nilRecv;
    switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out var v, out var ok): {
        fmt.Println(nilCommaOkTookCaseˢ, v, ok);
        break;
    }
    default: {
        fmt.Println(nilCommaOkDefaultˢ);
        break;
    }}

    var ready = new channel<nint>(1);

    var selᴛ3 = ready;
    switch (trySelect(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out var v): {
        fmt.Println(realRecvTookCaseˢ, v);
        break;
    }
    default: {
        fmt.Println(realEmptyDefaultˢ);
        break;
    }}

    ready.ᐸꟷ(7);

    var selᴛ4 = ready;
    switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out var v): {
        fmt.Println(realRecvˢ, v);
        break;
    }
    default: {
        fmt.Println(realDefaultˢ);
        break;
    }}

    ready.ᐸꟷ(9);

    var selᴛ5 = nilRecv;
    var selᴛ6 = ready;
    switch (select(ᐸꟷ(selᴛ5, ꓸꓸꓸ), ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
    case 0 when selᴛ5.ꟷᐳ(out var v): {
        fmt.Println(mixedTookNilˢ, v);
        break;
    }
    case 1 when selᴛ6.ꟷᐳ(out var v): {
        fmt.Println(mixedTookRealˢ, v);
        break;
    }}
}

} // end main_package
