namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    var a = new channel<nint>(1);
    var b = new channel<nint>(1);
    nint aCount = 0;
    nint bCount = 0;

    for (nint i = 0; i < 200; i++) {
        a.ᐸꟷ(1);
        b.ᐸꟷ(1);
        var aᴛ1 = a;
        var bᴛ1 = b;
        switch (select(ᐸꟷ(aᴛ1, ꓸꓸꓸ), ᐸꟷ(bᴛ1, ꓸꓸꓸ))) {
        case 0 when aᴛ1.ꟷᐳ(out _): {
            aCount++;
            break;
        }
        case 1 when bᴛ1.ꟷᐳ(out _): {
            bCount++;
            break;
        }}
        var aᴛ2 = a;
        var bᴛ2 = b;
        switch (select(ᐸꟷ(aᴛ2, ꓸꓸꓸ), ᐸꟷ(bᴛ2, ꓸꓸꓸ))) {
        case 0 when aᴛ2.ꟷᐳ(out _): {
            break;
        }
        case 1 when bᴛ2.ꟷᐳ(out _): {
            break;
        }}
    }

    fmt.Println("total:", aCount + bCount);
    fmt.Println("both branches taken:", aCount > 0 && bCount > 0);
}

} // end main_package
