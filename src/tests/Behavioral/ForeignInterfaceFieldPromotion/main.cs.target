namespace go;

using fmt = fmt_package;
using lib = ForeignInterfaceFieldPromotion.lib_package;
using ForeignInterfaceFieldPromotion;

partial class main_package {

partial struct S {
    /*embed*/ public ForeignInterfaceFieldPromotion.lib_package.I I;
    internal @string label;
}

partial struct R {
    /*embed*/ public ForeignInterfaceFieldPromotion.lib_package.Sentinel Sentinel;
}

internal static void main() {
    var s = new S(I: lib.New("s"), label: "own");
    fmt.Println(lib.CallM(s), s.I.Name(), s.label);

    lib.I p = new SжI(Ꮡ(new S(I: lib.New("p"))));
    fmt.Println(lib.CallM(p), p.Name());

    lib.Sentinel sentinel = new R(lib.Mark);
    fmt.Println(sentinel != nil, lib.IsMarked(sentinel));
    fmt.Println(lib.IsMarked(lib.Mark));
}

} // end main_package
