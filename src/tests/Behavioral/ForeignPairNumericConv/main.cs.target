namespace go;

using fmt = fmt_package;
using convlib = ForeignPairNumericConv.convlib_package;
using ForeignPairNumericConv;

partial class main_package {

partial struct LocalID /*num:uintptr*/;

internal static void Main() {
    var tok = convlib.NewToken(0x2A);

    var h = ((convlib.Handle)(uintptr)tok);
    fmt.Println("foreign pair:", (uintptr)h, h.String());

    var id = ((LocalID)(uintptr)tok);
    fmt.Println("foreign to local:", (uintptr)id);

    var back = ((convlib.Token)(uintptr)id);
    fmt.Println("local to foreign:", back.Raw());
}

} // end main_package
