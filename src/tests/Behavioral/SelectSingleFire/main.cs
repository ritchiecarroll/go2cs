namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deliveredˢ = (@string)"delivered:"u8;
private static readonly object gotˢ = (@string)"got:"u8;
private static readonly object remainingˢ = (@string)"remaining:"u8;
private static readonly object after100Selectsˢ = (@string)"after 100 selects:"u8;
private static readonly object sumˢ = (@string)"sum:"u8;

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
    fmt.Println(deliveredˢ, len(a) + len(b));

    nint got = default!;
    var selᴛ3 = a;
    var selᴛ4 = b;
    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out got): {
        break;
    }
    case 1 when selᴛ4.ꟷᐳ(out got): {
        break;
    }}
    fmt.Println(gotˢ, got, remainingˢ, len(a) + len(b));

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
    fmt.Println(after100Selectsˢ, len(c) + len(d));
    nint sum = 0;
    while (len(c) > 0) {
        sum += ᐸꟷ(c);
    }
    while (len(d) > 0) {
        sum += ᐸꟷ(d);
    }
    fmt.Println(sumˢ, sum);
}

} // end main_package
