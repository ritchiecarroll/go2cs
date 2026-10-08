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
    public partial ref ж<ForeignPtrEmbedIfaceLib_package.Meter> Meter { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object valueˢ = (@string)"value:"u8;
private static readonly object pointerˢ = (@string)"pointer:"u8;
private static readonly object pairˢ = (@string)"pair:"u8;

internal static void Main() {
    ref var box = ref heap<meterBox>(out var Ꮡbox);
    box = new meterBox(Meter: ForeignPtrEmbedIfaceLib.NewMeter());
    accumulator a = box;
    a.Add(5);
    a.Add(7);
    fmt.Println(valueˢ, a.Total(), box.Meter.Total());
    accumulator p = new meterBoxжaccumulator(Ꮡbox);
    p.Add(10);
    fmt.Println(pointerˢ, p.Total(), box.Meter.Total());
    var pair = ForeignPtrEmbedIfaceLib.NewPair();
    combo c = new ForeignPtrEmbedIfaceLib_Pairжcombo(pair);
    c.Add(3);
    c.Set(42);
    fmt.Println(pairˢ, c.Get(), c.Add(1), pair.Value.Meter.Total());
}

} // end main_package
