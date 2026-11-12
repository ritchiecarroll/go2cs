namespace go.RootedUsingCapture.@internal;

using static sync_package;

partial class sync_package {

partial struct Mutex {
    internal bool held;
}

} partial class syncᴛMutex {

public static void Lock(this ref Mutex m) { m.held = true; }
public static void Unlock(this ref Mutex m) { m.held = false; }

} partial class sync_package {

partial struct Gauge {
    internal nint v;
}

public static Gauge NewGauge(nint v) { return new Gauge(v); }

} partial class syncᴛGauge {

public static nint Value(this Gauge g) { return g.v; }

} // end syncᴛGauge
