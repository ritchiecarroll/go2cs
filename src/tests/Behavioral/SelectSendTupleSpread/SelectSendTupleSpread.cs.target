namespace go;

using fmt = fmt_package;

partial class main_package {

internal static slice<@string> log;

internal static (nint, error) pair() {
    log = append(log, "pair"u8);
    return (7, default!);
}

internal static nint must(nint v, error err) {
    if (err != default!) {
        throw panic(err);
    }
    return v;
}

internal static channel<nint> chanFor(channel<nint> ch) {
    log = append(log, "chan"u8);
    return ch;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sentAloneˢ = (@string)"sent alone"u8;
private static readonly object impossibleˢ = (@string)"impossible"u8;
private static readonly object sentBesideAReceiveˢ = (@string)"sent beside a receive"u8;
private static readonly object orderˢ = (@string)"order:"u8;

internal static void Main() {
    var ch = new channel<nint>(4);
    var selᴛ1 = ch;
    var (ᴛ1, ᴛ2) = pair();
    var selᴛ2 = selᴛ1.ᐸꟷ(must(ᴛ1, ᴛ2), ꓸꓸꓸ);
    switch (select(selᴛ2)) {
    case 0: {
        fmt.Println(sentAloneˢ);
        break;
    }}
    var never = new channel<nint>(0);
    var selᴛ3 = never;
    var selᴛ4 = ch;
    var (ᴛ3, ᴛ4) = pair();
    var selᴛ5 = selᴛ4.ᐸꟷ(must(ᴛ3, ᴛ4), ꓸꓸꓸ);
    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), selᴛ5)) {
    case 0 when selᴛ3.ꟷᐳ(out var v): {
        fmt.Println(impossibleˢ, v);
        break;
    }
    case 1: {
        fmt.Println(sentBesideAReceiveˢ);
        break;
    }}
    log = default!;
    var selᴛ6 = chanFor(ch);
    var (ᴛ5, ᴛ6) = pair();
    var selᴛ7 = selᴛ6.ᐸꟷ(must(ᴛ5, ᴛ6), ꓸꓸꓸ);
    switch (select(selᴛ7)) {
    case 0: {
        fmt.Println(orderˢ, log);
        break;
    }}
    var (ᴛ7, ᴛ8) = pair();
    ch.ᐸꟷ(must(ᴛ7, ᴛ8));
    fmt.Println(ᐸꟷ(ch), ᐸꟷ(ch), ᐸꟷ(ch), ᐸꟷ(ch));
}

} // end main_package
