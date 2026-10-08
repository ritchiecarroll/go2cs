namespace go;

using fmt = fmt_package;
using convlib = ForeignPairNumericConv.convlib_package;
using ForeignPairNumericConv;

partial class main_package {

partial struct LocalID /*num:uintptr*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object foreignPairˢ = (@string)"foreign pair:"u8;
private static readonly object foreignToLocalˢ = (@string)"foreign to local:"u8;
private static readonly object localToForeignˢ = (@string)"local to foreign:"u8;

internal static void Main() {
    var tok = convlib.NewToken(0x2A);
    var h = ((convlib.Handle)(uintptr)tok);
    fmt.Println(foreignPairˢ, (uintptr)h, h.String());
    var id = ((LocalID)(uintptr)tok);
    fmt.Println(foreignToLocalˢ, (uintptr)id);
    var back = ((convlib.Token)(uintptr)id);
    fmt.Println(localToForeignˢ, back.Raw());
}

} // end main_package
