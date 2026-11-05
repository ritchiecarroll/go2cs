namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void Main() {
    var ch = new channel<nint>(0);
    fmt.Println("cap:", cap(ch), "len:", len(ch));

    var selᴛ1 = ch.ᐸꟷ(1, ꓸꓸꓸ);
    switch (trySelect(selᴛ1)) {
    case 0: {
        fmt.Println("send: ready (wrong for unbuffered)");
        break;
    }
    default: {
        fmt.Println("send: not ready (no receiver)");
        break;
    }}

    var selᴛ2 = ch;
    switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out var v): {
        fmt.Println("recv: ready (wrong):", v);
        break;
    }
    default: {
        fmt.Println("recv: not ready (no sender)");
        break;
    }}

    fmt.Println("after probes: len:", len(ch), "cap:", cap(ch));

    var reply = new channel<nint>(0);
    var chʗ1 = ch;
    var replyʗ1 = reply;
    goǃ(() => {
        nint v = ᐸꟷ(chʗ1);
        replyʗ1.ᐸꟷ(v * 2);
    });
    ch.ᐸꟷ(21);
    fmt.Println("reply:", ᐸꟷ(reply));
    fmt.Println("after rendezvous: len:", len(ch), "cap:", cap(ch));

    var ping = new channel<nint>(0);
    var pong = new channel<nint>(0);
    var pingʗ1 = ping;
    var pongʗ1 = pong;
    goǃ(() => {
        for (nint i = 0; i < 3; i++) {
            nint v = ᐸꟷ(pingʗ1);
            pongʗ1.ᐸꟷ(v + 1);
        }
    });
    for (nint i = 0; i < 3; i++) {
        ping.ᐸꟷ(i * 10);
        fmt.Println("pong:", ᐸꟷ(pong));
    }
}

} // end main_package
