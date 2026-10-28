namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var a = new channel<nint>(1);
    var b = new channel<nint>(1);

    var selᴛ1 = a.ᐸꟷ(9, ꓸꓸꓸ);
    var selᴛ2 = b.ᐸꟷ(9, ꓸꓸꓸ);
    switch (select(selᴛ1, selᴛ2)) {
    case 0: {
        break;
    }
    case 1: {
        break;
    }}
    fmt.Println("delivered:", len(a) + len(b));

    nint got = default;
    var selᴛ3 = a;
    var selᴛ4 = b;
    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out got): {
        break;
    }
    case 1 when selᴛ4.ꟷᐳ(out got): {
        break;
    }}
    fmt.Println("got:", got, "remaining:", len(a) + len(b));

    var c = new channel<nint>(100);
    var d = new channel<nint>(100);
    for (nint i = 0; i < 100; i++) {
        var selᴛ5 = c.ᐸꟷ(i, ꓸꓸꓸ);
        var selᴛ6 = d.ᐸꟷ(i, ꓸꓸꓸ);
        switch (select(selᴛ5, selᴛ6)) {
        case 0: {
            break;
        }
        case 1: {
            break;
        }}
    }
    fmt.Println("after 100 selects:", len(c) + len(d));
    nint sum = 0;
    while (len(c) > 0) {
        sum += ᐸꟷ(c);
    }
    while (len(d) > 0) {
        sum += ᐸꟷ(d);
    }
    fmt.Println("sum:", sum);
}

} // end main_package
