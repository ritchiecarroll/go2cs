namespace go.SamePackageImplementNoWitness;

using static ledger_package;

partial class ledger_package {

partial interface Metric {
    nint Value();
}

partial struct Counter {
    public nint N;
}

} partial class ledgerᴛCounter {

public static nint Value(this Counter c) { return c.N; }

} partial class ledger_package {

partial struct Gauge /*num:nint*/;

} partial class ledgerᴛGauge {

public static nint Value(this Gauge g) { return (nint)g * 2; }

} partial class ledger_package {

public delegate nint Meter();

} partial class ledgerᴛMeter {

public static nint Value(this Meter m) { return m(); }

} partial class ledger_package {

partial struct Tally {
    public nint N;
}

} partial class ledgerᴛTally {

public static nint Value(this ref Tally t) { return t.N; }

} partial class ledger_package {

partial struct tick {
    public nint N;
}

} partial class ledgerᴛtick {

internal static nint Value(this ref tick t) { return t.N; }

} partial class ledger_package {

public static nint Count(nint n) { return (Ꮡ(new tick(N: n))).Value(); }

partial struct Box<T> {
    public T V;
}

} partial class ledgerᴛBox {

public static nint Value<T>(this Box<T> b) { return 3; }

} partial class ledger_package {

partial interface probe {
    nint depth();
}

partial struct well {
    public nint D;
}

} partial class ledgerᴛwell {

internal static nint depth(this well w) { return w.D; }

} partial class ledger_package {

public static nint Depth(nint d) { return new well(D: d).depth(); }

} // end ledger_package
