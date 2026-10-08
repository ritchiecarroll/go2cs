namespace go;

using fmt = fmt_package;
using Δvlib = go.vlib.vlib_package;
using go.vlib;

partial class main_package {

partial struct wrapper {
    internal ж<Δvlib.Rand> r;
}

internal static void Main() {
    var w = new wrapper(r: Δvlib.New(new Δvlib.PCGжSource(Δvlib.NewPCG(42))));
    for (nint i = 0; i < 3; i++) {
        fmt.Println(w.r.IntN(100));
    }
}

} // end main_package
