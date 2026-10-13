namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object roundsˢ = (@string)"rounds:"u8;
private static readonly object recvGotˢ = (@string)"recvGot:"u8;
private static readonly object sentToˢ = (@string)"sentTo:"u8;
private static readonly object outˢ = (@string)"out:"u8;
private static readonly object sendFiredOnFullChannelˢ = (@string)"send fired on full channel (wrong)"u8;
private static readonly object tookˢ = (@string)"took:"u8;
private static readonly object lenˢ = (@string)"len:"u8;
private static readonly object recvFiredOnEmptyChannelˢ = (@string)"recv fired on empty channel (wrong)"u8;
private static readonly object drainedˢ = (@string)"drained:"u8;

internal static void Main() {
    var data = new channel<nint>(1);
    var @out = new channel<nint>(1);
    data.ᐸꟷ(5);
    nint recvGot = 0;
    nint sentTo = 0;
    nint rounds = 0;
    while (len(data) > 0 || len(@out) == 0) {
        rounds++;
        var selᴛ1 = data;
        var selᴛ2 = @out.ᐸꟷ(7, ꓸꓸꓸ);
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), selᴛ2)) {
        case 0 when selᴛ1.ꟷᐳ(out var v): {
            recvGot = v;
            break;
        }
        case 1: {
            sentTo++;
            break;
        }}
    }
    fmt.Println(roundsˢ, rounds, recvGotˢ, recvGot, sentToˢ, sentTo, outˢ, ᐸꟷ(@out));

    var ch = new channel<nint>(1);
    ch.ᐸꟷ(3);
    nint took = 0;
    var selᴛ3 = ch.ᐸꟷ(8, ꓸꓸꓸ);
    var selᴛ4 = ch;
    switch (select(selᴛ3, ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0: {
        fmt.Println(sendFiredOnFullChannelˢ);
        break;
    }
    case 1 when selᴛ4.ꟷᐳ(out took): {
        break;
    }}
    fmt.Println(tookˢ, took, lenˢ, len(ch));

    var selᴛ5 = ch.ᐸꟷ(8, ꓸꓸꓸ);
    var selᴛ6 = ch;
    switch (select(selᴛ5, ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
    case 0: {
        break;
    }
    case 1 when selᴛ6.ꟷᐳ(out took): {
        fmt.Println(recvFiredOnEmptyChannelˢ);
        break;
    }}
    fmt.Println(lenˢ, len(ch), drainedˢ, ᐸꟷ(ch));
}

} // end main_package
