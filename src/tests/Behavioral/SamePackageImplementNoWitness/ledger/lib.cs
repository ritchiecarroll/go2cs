namespace go.SamePackageImplementNoWitness;

partial class ledger_package {

partial interface Metric {
    nint Value();
}

partial struct Counter {
    public nint N;
}

public static nint Value(this Counter c) {
    return c.N;
}

partial struct Gauge /*num:nint*/;

public static nint Value(this Gauge g) {
    return (nint)g * 2;
}

public delegate nint Meter();

public static nint Value(this Meter m) {
    return m();
}

partial struct Tally {
    public nint N;
}

public static nint Value(this ref Tally t) {
    return t.N;
}

partial struct tick {
    public nint N;
}

internal static nint Value(this ref tick t) {
    return t.N;
}

public static nint Count(nint n) {
    return (Ꮡ(new tick(N: n))).Value();
}

partial struct Box<T> {
    public T V;
}

public static nint Value<T>(this Box<T> b) {
    return 3;
}

partial interface probe {
    nint depth();
}

partial struct well {
    public nint D;
}

internal static nint depth(this well w) {
    return w.D;
}

public static nint Depth(nint d) {
    return new well(D: d).depth();
}

} // end ledger_package
