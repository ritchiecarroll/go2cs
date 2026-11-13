namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    channel<nint> nilRecv = default;

    var nilRecvᴛ1 = nilRecv;
    switch (trySelect(ᐸꟷ(nilRecvᴛ1, ꓸꓸꓸ))) {
    case 0 when nilRecvᴛ1.ꟷᐳ(out var v): {
        fmt.Println("nil recv took case", v);
        break;
    }
    default: {
        fmt.Println("nil recv default");
        break;
    }}

    fmt.Println("nil len:", len(nilRecv), "nil cap:", cap(nilRecv));

    var nilRecvᴛ2 = nilRecv;
    switch (trySelect(ᐸꟷ(nilRecvᴛ2, ꓸꓸꓸ))) {
    case 0 when nilRecvᴛ2.ꟷᐳ(out var v, out var ok): {
        fmt.Println("nil comma-ok took case", v, ok);
        break;
    }
    default: {
        fmt.Println("nil comma-ok default");
        break;
    }}

    var ready = new channel<nint>(1);

    var readyᴛ1 = ready;
    switch (trySelect(ᐸꟷ(readyᴛ1, ꓸꓸꓸ))) {
    case 0 when readyᴛ1.ꟷᐳ(out var v): {
        fmt.Println("real recv took case", v);
        break;
    }
    default: {
        fmt.Println("real empty default");
        break;
    }}

    ready.ᐸꟷ(7);

    var readyᴛ2 = ready;
    switch (trySelect(ᐸꟷ(readyᴛ2, ꓸꓸꓸ))) {
    case 0 when readyᴛ2.ꟷᐳ(out var v): {
        fmt.Println("real recv", v);
        break;
    }
    default: {
        fmt.Println("real default");
        break;
    }}

    ready.ᐸꟷ(9);

    var nilRecvᴛ3 = nilRecv;
    var readyᴛ3 = ready;
    switch (select(ᐸꟷ(nilRecvᴛ3, ꓸꓸꓸ), ᐸꟷ(readyᴛ3, ꓸꓸꓸ))) {
    case 0 when nilRecvᴛ3.ꟷᐳ(out var v): {
        fmt.Println("mixed took nil", v);
        break;
    }
    case 1 when readyᴛ3.ꟷᐳ(out var v): {
        fmt.Println("mixed took real", v);
        break;
    }}
}

} // end main_package
