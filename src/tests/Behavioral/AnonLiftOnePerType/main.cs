namespace go;

using fmt = fmt_package;
using lib = AnonLiftOnePerType.lib_package;
using AnonLiftOnePerType;

partial class main_package {

internal static void main() {
    var h = lib.Mk(1);
    fmt.Println(h.V.A(), h.V.B());

    var h2 = lib.Mk2(3);
    fmt.Println(h2.W.A(), h2.W.B());

    var b = lib.MkBox(5);
    fmt.Println(b.P.X, b.P.Y);

    var b2 = lib.MkBox2(7);
    fmt.Println(b2.Q.X, b2.Q.Y);
}

} // end main_package
