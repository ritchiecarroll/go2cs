namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilLenˢ = (@string)"nil len:"u8;
private static readonly object capˢ = (@string)"cap:"u8;
private static readonly object nilRecvWrongˢ = (@string)"nil recv (wrong):"u8;
private static readonly object nilSendWrongˢ = (@string)"nil send (wrong)"u8;
private static readonly object liveRecvˢ = (@string)"live recv:"u8;
private static readonly object rendezvousRecvˢ = (@string)"rendezvous recv:"u8;
private static readonly object liveSendFiredˢ = (@string)"live send fired"u8;
private static readonly object outˢ = (@string)"out:"u8;

internal static partial void Main() {
    channel<nint> nilCh = default!;
    fmt.Println(nilLenˢ, len(nilCh), capˢ, cap(nilCh));
    var live = new channel<nint>(1);
    live.ᐸꟷ(42);
    var selᴛ1 = nilCh;
    var selᴛ2 = nilCh.ᐸꟷ(1, ꓸꓸꓸ);
    var selᴛ3 = live;
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), selᴛ2, ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var v): {
        fmt.Println(nilRecvWrongˢ, v);
        break;
    }
    case 1: {
        fmt.Println(nilSendWrongˢ);
        break;
    }
    case 2 when selᴛ3.ꟷᐳ(out var v): {
        fmt.Println(liveRecvˢ, v);
        break;
    }}
    var u = new channel<nint>(0);
    var uʗ1 = u;
    goǃ(() => {
        uʗ1.ᐸꟷ(7);
    });
    var selᴛ4 = nilCh;
    var selᴛ5 = u;
    switch (select(ᐸꟷ(selᴛ4, ꓸꓸꓸ), ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out var v): {
        fmt.Println(nilRecvWrongˢ, v);
        break;
    }
    case 1 when selᴛ5.ꟷᐳ(out var v): {
        fmt.Println(rendezvousRecvˢ, v);
        break;
    }}
    var @out = new channel<nint>(1);
    var selᴛ6 = nilCh.ᐸꟷ(9, ꓸꓸꓸ);
    var selᴛ7 = @out.ᐸꟷ(5, ꓸꓸꓸ);
    switch (select(selᴛ6, selᴛ7)) {
    case 0: {
        fmt.Println(nilSendWrongˢ);
        break;
    }
    case 1: {
        fmt.Println(liveSendFiredˢ);
        break;
    }}
    fmt.Println(outˢ, ᐸꟷ(@out));
}

} // end main_package
