namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    var data = new channel<nint>(1);
    var @out = new channel<nint>(1);
    data.ᐸꟷ(5);
    nint recvGot = 0;
    nint sentTo = 0;
    nint rounds = 0;
    while (len(data) > 0 || len(@out) == 0) {
        rounds++;
        var dataᴛ1 = data;
        var outᴛ1 = @out.ᐸꟷ(7, ꓸꓸꓸ);
        switch (select(ᐸꟷ(dataᴛ1, ꓸꓸꓸ), outᴛ1)) {
        case 0 when dataᴛ1.ꟷᐳ(out var v): {
            recvGot = v;
            break;
        }
        case 1: {
            sentTo++;
            break;
        }}
    }
    fmt.Println("rounds:", rounds, "recvGot:", recvGot, "sentTo:", sentTo, "out:", ᐸꟷ(@out));

    var ch = new channel<nint>(1);
    ch.ᐸꟷ(3);
    nint took = 0;
    var chᴛ1 = ch.ᐸꟷ(8, ꓸꓸꓸ);
    var chᴛ2 = ch;
    switch (select(chᴛ1, ᐸꟷ(chᴛ2, ꓸꓸꓸ))) {
    case 0: {
        fmt.Println("send fired on full channel (wrong)");
        break;
    }
    case 1 when chᴛ2.ꟷᐳ(out took): {
        break;
    }}
    fmt.Println("took:", took, "len:", len(ch));

    var chᴛ3 = ch.ᐸꟷ(8, ꓸꓸꓸ);
    var chᴛ4 = ch;
    switch (select(chᴛ3, ᐸꟷ(chᴛ4, ꓸꓸꓸ))) {
    case 0: {
        break;
    }
    case 1 when chᴛ4.ꟷᐳ(out took): {
        fmt.Println("recv fired on empty channel (wrong)");
        break;
    }}
    fmt.Println("len:", len(ch), "drained:", ᐸꟷ(ch));
}

} // end main_package
