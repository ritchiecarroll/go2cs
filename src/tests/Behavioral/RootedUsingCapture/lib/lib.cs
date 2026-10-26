namespace go.RootedUsingCapture;

using sync = sync_package;
using isync = go.RootedUsingCapture.@internal.sync_package;
using go.RootedUsingCapture.@internal;

partial class lib_package {

partial struct Guarded {
    /*embed*/ public go.sync_package.Mutex Mutex;
    internal nint n;
}

public static ж<Guarded> New(nint n) {
    return Ꮡ(new Guarded(n: n));
}

public static nint Add(this ж<Guarded> Ꮡg, nint d) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var g = ref Ꮡg.DerefOrNull();

        Ꮡg.of(Guarded.ᏑMutex).Lock();
        ᒐd1 = true;
        g.n += d;
        return g.n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡg.of(Guarded.ᏑMutex).Unlock(); ᒐ.Run(); }
}

public static nint Base() {
    return isync.NewGauge(3).Value();
}

} // end lib_package
