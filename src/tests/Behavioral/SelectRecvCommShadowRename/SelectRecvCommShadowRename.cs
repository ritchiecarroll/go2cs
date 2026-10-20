namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    for (nint loop = 0; loop < 2; loop++) {
        channel<nint> cΔ1 = default!;
        cΔ1 = new channel<nint>(2);
        cΔ1.ᐸꟷ(40 + loop);

        var selᴛ1 = cΔ1;
        switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out var x): {
            fmt.Println("recv", x);
            break;
        }
        default: {
            fmt.Println("empty");
            break;
        }}

        cΔ1.ᐸꟷ(50 + loop);
        var selᴛ2 = cΔ1;
        switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ2.ꟷᐳ(out var x, out var ok): {
            fmt.Println("recv2", x, ok);
            break;
        }
        default: {
            fmt.Println("empty2");
            break;
        }}

        var selᴛ3 = cΔ1;
        switch (trySelect(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
        case 0 when selᴛ3.ꟷᐳ(out var x): {
            fmt.Println("unexpected", x);
            break;
        }
        default: {
            fmt.Println("drained");
            break;
        }}

        var d = new channel<nint>(1);
        var selᴛ4 = d.ᐸꟷ(7, ꓸꓸꓸ);
        var selᴛ5 = cΔ1;
        switch (select(selᴛ4, ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
        case 0: {
            fmt.Println("sent");
            break;
        }
        case 1 when selᴛ5.ꟷᐳ(out _): {
            fmt.Println("unexpected recv");
            break;
        }}
        fmt.Println("d", ᐸꟷ(d));
    }

    channel<@string> c = default!;
    c = new channel<@string>(1);
    c.ᐸꟷ("tail");
    fmt.Println(ᐸꟷ(c));
}

} // end main_package
