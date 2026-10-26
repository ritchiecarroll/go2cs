namespace go;

using fmt = fmt_package;
using ForeignPtrEmbedIfaceLib = ForeignPtrEmbedIfaceLib_package;

partial class main_package {

partial interface accumulator {
    nint Add(nint delta);
    nint Total();
}

partial interface combo {
    nint Add(nint delta);
    void Set(nint v);
    nint Get();
}

partial struct meterBox {
    /*embed*/ public ж<ForeignPtrEmbedIfaceLib_package.Meter> Meter;
}

internal static void Main() {
    ref var box = ref heap<meterBox>(out var Ꮡbox);
    box = new meterBox(Meter: ForeignPtrEmbedIfaceLib.NewMeter());
    accumulator a = box;
    a.Add(5);
    a.Add(7);
    fmt.Println("value:", a.Total(), box.Meter.Total());

    accumulator p = new meterBoxжaccumulator(Ꮡbox);
    p.Add(10);
    fmt.Println("pointer:", p.Total(), box.Meter.Total());

    var pair = ForeignPtrEmbedIfaceLib.NewPair();
    combo c = new ForeignPtrEmbedIfaceLib_Pairжcombo(pair);
    c.Add(3);
    c.Set(42);
    fmt.Println("pair:", c.Get(), c.Add(1), pair.Value.Meter.Total());
}

} // end main_package
