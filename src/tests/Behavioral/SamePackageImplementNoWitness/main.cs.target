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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object assertˢ = (@string)"assert:"u8;
private static readonly object assertMissˢ = (@string)"assert-miss:"u8;
private static readonly object switchˢ = (@string)"switch:"u8;
private static readonly object counterˢ = (@string)"Counter"u8;
private static readonly object gaugeˢ = (@string)"Gauge"u8;
private static readonly object defaultˢ = (@string)"default"u8;
private static readonly @string fromIfaceˢ = "from-iface"u8;
private static readonly object mapˢ = (@string)"map:"u8;
private static readonly object callˢ = (@string)"call:"u8;
private static readonly object explicitˢ = (@string)"explicit:"u8;
private static readonly object funcˢ = (@string)"func:"u8;
private static readonly object ptrˢ = (@string)"ptr:"u8;
private static readonly object ptrAssertˢ = (@string)"ptr-assert:"u8;
private static readonly object ptrAssertMissˢ = (@string)"ptr-assert-miss:"u8;
private static readonly object hiddenˢ = (@string)"hidden:"u8;
private static readonly object unexportedTargetˢ = (@string)"unexported-target:"u8;
private static readonly object genericˢ = (@string)"generic:"u8;
private static readonly object localˢ = (@string)"local:"u8;

internal static void Main() {
    ledger.Metric m = new ledger.Counter(N: 7);
    var want = new ledger.Counter(N: 7);
    fmt.Printf("counter: %v %T\n"u8, m, m);
    fmt.Println((@string)"eq:"u8, AreEqual(m, want), AreEqual(want, m));
    fmt.Println((@string)"ne:"u8, AreEqual(m, new ledger.Counter(N: 8)));
    ledger.Metric g = ((ledger.Gauge)4);
    fmt.Printf("gauge: %v %T %d\n"u8, g, g, g.Value());
    var (c, ok) = m._<ledger.Counter>(ᐧ);
    fmt.Println(assertˢ, ok, c.N);
    var (_, badOK) = m._<ledger.Gauge>(ᐧ);
    fmt.Println(assertMissˢ, badOK);
    switch (g.type()) {
    case ledger.Counter t: {
        fmt.Println(switchˢ, counterˢ, t.N);
        break;
    }
    case ledger.Gauge t: {
        fmt.Println(switchˢ, gaugeˢ, (nint)t);
        break;
    }
    default: {
        var t = g;
        fmt.Println(switchˢ, defaultˢ);
        break;
    }}
    var seen = new map<ledger.Metric, @string>{};
    seen[m] = fromIfaceˢ;
    fmt.Println(mapˢ, seen[want], len(seen));
    fmt.Println(callˢ, m.Value());
    var sc = ((ledger.Metric)((ledger.Gauge)9));
    fmt.Println(explicitˢ, sc.Value());
    ledger.Metric f = new ledger_MeterᴠMetric(new ledger.Meter(() => 11));
    fmt.Println(funcˢ, f.Value());
    ledger.Metric p = new ledger.TallyжMetric(Ꮡ(new ledger.Tally(N: 5)));
    fmt.Println(ptrˢ, p.Value());
    var (tp, tok) = p._<ж<ledger.Tally>>(ᐧ);
    fmt.Println(ptrAssertˢ, tok, (~tp).N);
    var (_, tbad) = p._<ledger.Counter>(ᐧ);
    fmt.Println(ptrAssertMissˢ, tbad);
    fmt.Println(hiddenˢ, ledger.Depth(3));
    fmt.Println(unexportedTargetˢ, ledger.Count(6));
    var b = new ledger.Box<nint>(V: 9);
    fmt.Println(genericˢ, b.Value(), b.V);
    fmt.Println(localˢ, new Stamp(S: "s"u8).Mark());
}

} // end main_package
