namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void Main() {
    channel<nint> nilCh = default;

    fmt.Println("nil len:", len(nilCh), "cap:", cap(nilCh));

    var live = new channel<nint>(1);
    live.ᐸꟷ(42);
    var selᴛ1 = nilCh;
    var selᴛ2 = nilCh.ᐸꟷ(1, ꓸꓸꓸ);
    var selᴛ3 = live;
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), selᴛ2, ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var v): {
        fmt.Println("nil recv (wrong):", v);
        break;
    }
    case 1: {
        fmt.Println("nil send (wrong)");
        break;
    }
    case 2 when selᴛ3.ꟷᐳ(out var v): {
        fmt.Println("live recv:", v);
        break;
    }}

    var u = new channel<nint>(0);
    var uʗ1 = u;
    goǃ(() => {
        uʗ1.ᐸꟷ(7);
    }, GoStack.Bounded);
    var selᴛ4 = nilCh;
    var selᴛ5 = u;
    switch (select(ᐸꟷ(selᴛ4, ꓸꓸꓸ), ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out var v): {
        fmt.Println("nil recv (wrong):", v);
        break;
    }
    case 1 when selᴛ5.ꟷᐳ(out var v): {
        fmt.Println("rendezvous recv:", v);
        break;
    }}

    var @out = new channel<nint>(1);
    var selᴛ6 = nilCh.ᐸꟷ(9, ꓸꓸꓸ);
    var selᴛ7 = @out.ᐸꟷ(5, ꓸꓸꓸ);
    switch (select(selᴛ6, selᴛ7)) {
    case 0: {
        fmt.Println("nil send (wrong)");
        break;
    }
    case 1: {
        fmt.Println("live send fired");
        break;
    }}
    fmt.Println("out:", ᐸꟷ(@out));
}

} // end main_package
