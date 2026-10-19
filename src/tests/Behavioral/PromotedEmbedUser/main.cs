namespace go;

using fmt = fmt_package;
using PromotedEmbedLib = PromotedEmbedLib_package;

partial class main_package {

internal static void bump(ж<PromotedEmbedLib.Counter> Ꮡc, nint n) {
    Ꮡc.Add(n);
}

internal static void Main() {
    var c = PromotedEmbedLib.New("widget");
    c.Add(3);
    bump(c, 4);
    fmt.Println((~c).Label, c.Report().Sum);
}

} // end main_package
