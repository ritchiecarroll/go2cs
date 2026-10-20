namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var a = new channel<nint>(1);
    var b = new channel<nint>(1);
    nint aCount = 0;
    nint bCount = 0;

    for (nint i = 0; i < 200; i++) {
        a.ᐸꟷ(1);
        b.ᐸꟷ(1);
        var selᴛ1 = a;
        var selᴛ2 = b;
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out _): {
            aCount++;
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            bCount++;
            break;
        }}
        var selᴛ3 = a;
        var selᴛ4 = b;
        switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
        case 0 when selᴛ3.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ4.ꟷᐳ(out _): {
            break;
        }}
    }

    fmt.Println("total:", aCount + bCount);
    fmt.Println("both branches taken:", aCount > 0 && bCount > 0);
}

} // end main_package
