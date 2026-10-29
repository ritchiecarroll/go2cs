namespace go;

using fmt = fmt_package;
using ledger = SamePackageImplementNoWitness.ledger_package;
using SamePackageImplementNoWitness;

partial class main_package {

partial interface Marker {
    @string Mark();
}

partial struct Stamp {
    public @string S;
}

public static @string Mark(this Stamp s) {
    return s.S;
}

internal static void Main() {
    ledger.Metric m = new ledger.Counter(N: 7);
    var want = new ledger.Counter(N: 7);

    fmt.Printf("counter: %v %T\n"u8, m, m);

    fmt.Println("eq:", AreEqual(m, want), AreEqual(want, m));
    fmt.Println("ne:", AreEqual(m, new ledger.Counter(N: 8)));

    ledger.Metric g = ((ledger.Gauge)4);
    fmt.Printf("gauge: %v %T %d\n"u8, g, g, g.Value());

    var (c, ok) = m._<ledger.Counter>(ᐧ);
    fmt.Println("assert:", ok, c.N);
    var (_, badOK) = m._<ledger.Gauge>(ᐧ);
    fmt.Println("assert-miss:", badOK);

    switch (g.type()) {
    case ledger.Counter t: {
        fmt.Println("switch:", "Counter", t.N);
        break;
    }
    case ledger.Gauge t: {
        fmt.Println("switch:", "Gauge", (nint)t);
        break;
    }
    default: {
        var t = g;
        fmt.Println("switch:", "default");
        break;
    }}

    var seen = new map<ledger.Metric, @string>{};
    seen[m] = "from-iface";
    fmt.Println("map:", seen[want], len(seen));

    fmt.Println("call:", m.Value());

    var sc = ((ledger.Metric)((ledger.Gauge)9));
    fmt.Println("explicit:", sc.Value());

    ledger.Metric f = new ledger_MeterᴠMetric(new ledger.Meter(() => 11));
    fmt.Println("func:", f.Value());

    ledger.Metric p = new ledger.TallyжMetric(Ꮡ(new ledger.Tally(N: 5)));
    fmt.Println("ptr:", p.Value());

    var (tp, tok) = p._<ж<ledger.Tally>>(ᐧ);
    fmt.Println("ptr-assert:", tok, tp.N);
    var (_, tbad) = p._<ledger.Counter>(ᐧ);
    fmt.Println("ptr-assert-miss:", tbad);

    fmt.Println("hidden:", ledger.Depth(3));

    fmt.Println("unexported-target:", ledger.Count(6));

    var b = new ledger.Box<nint>(V: 9);
    fmt.Println("generic:", b.Value(), b.V);

    fmt.Println("local:", new Stamp(S: "s").Mark());
}

} // end main_package
