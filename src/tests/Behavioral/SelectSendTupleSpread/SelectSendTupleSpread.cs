namespace go;

using fmt = fmt_package;

partial class main_package {

internal static slice<@string> log;

internal static (nint, error) pair() {
    log = append(log, (@string)"pair");
    return (7, default);
}

internal static nint must(nint v, error err) {
    if (err != nil) {
        throw panic(err);
    }
    return v;
}

internal static channel<nint> chanFor(channel<nint> ch) {
    log = append(log, (@string)"chan");
    return ch;
}

internal static void main() {
    var ch = new channel<nint>(4);

    var selᴛ1 = ch;
    var (ᴛ1, ᴛ2) = pair();
    var chᴛ1 = selᴛ1.ᐸꟷ(must(ᴛ1, ᴛ2), ꓸꓸꓸ);
    switch (select(chᴛ1)) {
    case 0: {
        fmt.Println("sent alone");
        break;
    }}

    var never = new channel<nint>(0);
    var neverᴛ1 = never;
    var selᴛ4 = ch;
    var (ᴛ3, ᴛ4) = pair();
    var chᴛ2 = selᴛ4.ᐸꟷ(must(ᴛ3, ᴛ4), ꓸꓸꓸ);
    switch (select(ᐸꟷ(neverᴛ1, ꓸꓸꓸ), chᴛ2)) {
    case 0 when neverᴛ1.ꟷᐳ(out var v): {
        fmt.Println("impossible", v);
        break;
    }
    case 1: {
        fmt.Println("sent beside a receive");
        break;
    }}

    log = default;
    var selᴛ6 = chanFor(ch);
    var (ᴛ5, ᴛ6) = pair();
    var selᴛ7 = selᴛ6.ᐸꟷ(must(ᴛ5, ᴛ6), ꓸꓸꓸ);
    switch (select(selᴛ7)) {
    case 0: {
        fmt.Println("order:", log);
        break;
    }}

    var (ᴛ7, ᴛ8) = pair();
    ch.ᐸꟷ(must(ᴛ7, ᴛ8));

    fmt.Println(ᐸꟷ(ch), ᐸꟷ(ch), ᐸꟷ(ch), ᐸꟷ(ch));
}

} // end main_package
