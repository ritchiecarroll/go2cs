namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    channel<nint> nilRecv = default!;

    var selᴛ1 = nilRecv;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var v): {
        fmt.Println("nil recv took case", v);
        break;
    }
    default: {
        fmt.Println("nil recv default");
        break;
    }}

    fmt.Println("nil len:", len(nilRecv), "nil cap:", cap(nilRecv));

    var selᴛ2 = nilRecv;
    switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out var v, out var ok): {
        fmt.Println("nil comma-ok took case", v, ok);
        break;
    }
    default: {
        fmt.Println("nil comma-ok default");
        break;
    }}

    var ready = new channel<nint>(1);

    var selᴛ3 = ready;
    switch (trySelect(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out var v): {
        fmt.Println("real recv took case", v);
        break;
    }
    default: {
        fmt.Println("real empty default");
        break;
    }}

    ready.ᐸꟷ(7);

    var selᴛ4 = ready;
    switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out var v): {
        fmt.Println("real recv", v);
        break;
    }
    default: {
        fmt.Println("real default");
        break;
    }}

    ready.ᐸꟷ(9);

    var selᴛ5 = nilRecv;
    var selᴛ6 = ready;
    switch (select(ᐸꟷ(selᴛ5, ꓸꓸꓸ), ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
    case 0 when selᴛ5.ꟷᐳ(out var v): {
        fmt.Println("mixed took nil", v);
        break;
    }
    case 1 when selᴛ6.ꟷᐳ(out var v): {
        fmt.Println("mixed took real", v);
        break;
    }}
}

} // end main_package
