namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    for (nint loop = 0; loop < 2; loop++) {
        channel<nint> cΔ1 = default;
        cΔ1 = new channel<nint>(2);
        cΔ1.ᐸꟷ(40 + loop);

        var cᴛ1 = cΔ1;
        switch (trySelect(ᐸꟷ(cᴛ1, ꓸꓸꓸ))) {
        case 0 when cᴛ1.ꟷᐳ(out var x): {
            fmt.Println("recv", x);
            break;
        }
        default: {
            fmt.Println("empty");
            break;
        }}

        cΔ1.ᐸꟷ(50 + loop);
        var cᴛ2 = cΔ1;
        switch (trySelect(ᐸꟷ(cᴛ2, ꓸꓸꓸ))) {
        case 0 when cᴛ2.ꟷᐳ(out var x, out var ok): {
            fmt.Println("recv2", x, ok);
            break;
        }
        default: {
            fmt.Println("empty2");
            break;
        }}

        var cᴛ3 = cΔ1;
        switch (trySelect(ᐸꟷ(cᴛ3, ꓸꓸꓸ))) {
        case 0 when cᴛ3.ꟷᐳ(out var x): {
            fmt.Println("unexpected", x);
            break;
        }
        default: {
            fmt.Println("drained");
            break;
        }}

        var d = new channel<nint>(1);
        var dᴛ1 = d.ᐸꟷ(7, ꓸꓸꓸ);
        var cᴛ4 = cΔ1;
        switch (select(dᴛ1, ᐸꟷ(cᴛ4, ꓸꓸꓸ))) {
        case 0: {
            fmt.Println("sent");
            break;
        }
        case 1 when cᴛ4.ꟷᐳ(out _): {
            fmt.Println("unexpected recv");
            break;
        }}
        fmt.Println("d", ᐸꟷ(d));
    }

    channel<@string> c = default;
    c = new channel<@string>(1);
    c.ᐸꟷ("tail");
    fmt.Println(ᐸꟷ(c));
}

} // end main_package
