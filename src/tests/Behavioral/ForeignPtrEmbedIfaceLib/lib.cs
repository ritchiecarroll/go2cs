namespace go;

partial class ForeignPtrEmbedIfaceLib_package {

partial struct Meter {
    internal nint total;
}

public static nint Add(this ж<Meter> Ꮡm, nint delta) {
    var p = Ꮡm.of(Meter.Ꮡtotal);
    p.Value += delta;
    return p.Value;
}

public static nint Total(this ж<Meter> Ꮡm) {
    var p = Ꮡm.of(Meter.Ꮡtotal);
    return p.Value;
}

partial struct Gauge {
    internal nint value;
}

public static void Set(this ref Gauge g, nint v) {
    g.value = v;
}

public static nint Get(this ref Gauge g) {
    return g.value;
}

partial struct Pair {
    /*embed*/ public ж<Meter> Meter;
    /*embed*/ public ж<Gauge> Gauge;
}

public static ж<Meter> NewMeter() {
    return Ꮡ(new Meter());
}

public static ж<Pair> NewPair() {
    return Ꮡ(new Pair(Meter: Ꮡ(new Meter()), Gauge: Ꮡ(new Gauge())));
}

} // end ForeignPtrEmbedIfaceLib_package
