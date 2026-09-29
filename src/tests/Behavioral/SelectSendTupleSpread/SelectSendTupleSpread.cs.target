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
    var selᴛ1 = ch.ᐸꟷ(must(pair()), ꓸꓸꓸ);
    switch (select(selᴛ1)) {
    case 0: {
        fmt.Println(sentAloneˢ);
        break;
    }}
    var never = new channel<nint>(0);
    var selᴛ2 = never;
    var selᴛ3 = ch.ᐸꟷ(must(pair()), ꓸꓸꓸ);
    switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), selᴛ3)) {
    case 0 when selᴛ2.ꟷᐳ(out var v): {
        fmt.Println(impossibleˢ, v);
        break;
    }
    case 1: {
        fmt.Println(sentBesideAReceiveˢ);
        break;
    }}
    log = default!;
    var selᴛ4 = chanFor(ch).ᐸꟷ(must(pair()), ꓸꓸꓸ);
    switch (select(selᴛ4)) {
    case 0: {
        fmt.Println(orderˢ, log);
        break;
    }}
    var (ᴛ1, ᴛ2) = pair();
    ch.ᐸꟷ(must(ᴛ1, ᴛ2));
    fmt.Println(ᐸꟷ(ch), ᐸꟷ(ch), ᐸꟷ(ch), ᐸꟷ(ch));
}

} // end main_package
