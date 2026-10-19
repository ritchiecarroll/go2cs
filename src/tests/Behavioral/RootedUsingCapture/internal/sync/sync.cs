namespace go.RootedUsingCapture.@internal;

partial class sync_package {

partial struct Mutex {
    internal bool held;
}

public static void Lock(this ref Mutex m) {
    m.held = true;
}

public static void Unlock(this ref Mutex m) {
    m.held = false;
}

partial struct Gauge {
    internal nint v;
}

public static Gauge NewGauge(nint v) {
    return new Gauge(v);
}

public static nint Value(this Gauge g) {
    return g.v;
}

} // end sync_package
