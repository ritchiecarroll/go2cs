namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    var a = new channel<nint>(1);
    var b = new channel<nint>(1);

    var aᴛ1 = a.ᐸꟷ(9, ꓸꓸꓸ);
    var bᴛ1 = b.ᐸꟷ(9, ꓸꓸꓸ);
    switch (select(aᴛ1, bᴛ1)) {
    case 0: {
        break;
    }
    case 1: {
        break;
    }}
    fmt.Println("delivered:", len(a) + len(b));

    nint got = default;
    var aᴛ2 = a;
    var bᴛ2 = b;
    switch (select(ᐸꟷ(aᴛ2, ꓸꓸꓸ), ᐸꟷ(bᴛ2, ꓸꓸꓸ))) {
    case 0 when aᴛ2.ꟷᐳ(out got): {
        break;
    }
    case 1 when bᴛ2.ꟷᐳ(out got): {
        break;
    }}
    fmt.Println("got:", got, "remaining:", len(a) + len(b));

    var c = new channel<nint>(100);
    var d = new channel<nint>(100);
    for (nint i = 0; i < 100; i++) {
        var cᴛ1 = c.ᐸꟷ(i, ꓸꓸꓸ);
        var dᴛ1 = d.ᐸꟷ(i, ꓸꓸꓸ);
        switch (select(cᴛ1, dᴛ1)) {
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
