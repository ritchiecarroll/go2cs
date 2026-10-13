namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object totalˢ = (@string)"total:"u8;
private static readonly object bothBranchesTakenˢ = (@string)"both branches taken:"u8;

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

    fmt.Println(totalˢ, aCount + bCount);
    fmt.Println(bothBranchesTakenˢ, aCount > 0 && bCount > 0);
}

} // end main_package
