namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object capˢ = (@string)"cap:"u8;
private static readonly object lenˢ = (@string)"len:"u8;
private static readonly object sendReadyWrongForˢ = (@string)"send: ready (wrong for unbuffered)"u8;
private static readonly object sendNotReadyNoReceiverˢ = (@string)"send: not ready (no receiver)"u8;
private static readonly object recvReadyWrongˢ = (@string)"recv: ready (wrong):"u8;
private static readonly object recvNotReadyNoSenderˢ = (@string)"recv: not ready (no sender)"u8;
private static readonly object afterProbesLenˢ = (@string)"after probes: len:"u8;
private static readonly object replyˢ = (@string)"reply:"u8;
private static readonly object afterRendezvousLenˢ = (@string)"after rendezvous: len:"u8;
private static readonly object pongˢ = (@string)"pong:"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void Main() {
    var ch = new channel<nint>(0);
    fmt.Println(capˢ, cap(ch), lenˢ, len(ch));
    var selᴛ1 = ch.ᐸꟷ(1, ꓸꓸꓸ);
    switch (trySelect(selᴛ1)) {
    case 0: {
        fmt.Println(sendReadyWrongForˢ);
        break;
    }
    default: {
        fmt.Println(sendNotReadyNoReceiverˢ);
        break;
    }}
    var selᴛ2 = ch;
    switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out var v): {
        fmt.Println(recvReadyWrongˢ, v);
        break;
    }
    default: {
        fmt.Println(recvNotReadyNoSenderˢ);
        break;
    }}
    fmt.Println(afterProbesLenˢ, len(ch), capˢ, cap(ch));
    var reply = new channel<nint>(0);
    var chʗ1 = ch;
    var replyʗ1 = reply;
    goǃ(() => {
        nint v = ᐸꟷ(chʗ1);
        replyʗ1.ᐸꟷ(v * 2);
    });
    ch.ᐸꟷ(21);
    fmt.Println(replyˢ, ᐸꟷ(reply));
    fmt.Println(afterRendezvousLenˢ, len(ch), capˢ, cap(ch));
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
        fmt.Println(pongˢ, ᐸꟷ(pong));
    }
}

} // end main_package
