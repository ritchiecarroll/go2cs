namespace go;

using fmt = fmt_package;

partial class main_package {

internal static partial void main() {
    channel<nint> nilCh = default;

    fmt.Println("nil len:", len(nilCh), "cap:", cap(nilCh));

    var live = new channel<nint>(1);
    live.ᐸꟷ(42);
    var nilChᴛ1 = nilCh;
    var nilChᴛ2 = nilCh.ᐸꟷ(1, ꓸꓸꓸ);
    var liveᴛ1 = live;
    switch (select(ᐸꟷ(nilChᴛ1, ꓸꓸꓸ), nilChᴛ2, ᐸꟷ(liveᴛ1, ꓸꓸꓸ))) {
    case 0 when nilChᴛ1.ꟷᐳ(out var v): {
        fmt.Println("nil recv (wrong):", v);
        break;
    }
    case 1: {
        fmt.Println("nil send (wrong)");
        break;
    }
    case 2 when liveᴛ1.ꟷᐳ(out var v): {
        fmt.Println("live recv:", v);
        break;
    }}

    var u = new channel<nint>(0);
    var uʗ1 = u;
    goǃ(() => {
        uʗ1.ᐸꟷ(7);
    });
    var nilChᴛ3 = nilCh;
    var uᴛ1 = u;
    switch (select(ᐸꟷ(nilChᴛ3, ꓸꓸꓸ), ᐸꟷ(uᴛ1, ꓸꓸꓸ))) {
    case 0 when nilChᴛ3.ꟷᐳ(out var v): {
        fmt.Println("nil recv (wrong):", v);
        break;
    }
    case 1 when uᴛ1.ꟷᐳ(out var v): {
        fmt.Println("rendezvous recv:", v);
        break;
    }}

    var @out = new channel<nint>(1);
    var nilChᴛ4 = nilCh.ᐸꟷ(9, ꓸꓸꓸ);
    var outᴛ1 = @out.ᐸꟷ(5, ꓸꓸꓸ);
    switch (select(nilChᴛ4, outᴛ1)) {
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
