namespace go.NestedNamespaceReach;

using stdsync = sync_package;
using sync = go.NestedNamespaceReach.x.sync_package;
using go.NestedNamespaceReach.x;
using static lib_package;

partial class lib_package {

partial struct Counter {
    /*embed*/ public go.sync_package.Mutex Mutex;
    internal nint n;
}

} partial class libᴛCounter {

public static nint Inc(this ж<Counter> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        Ꮡc.of(Counter.ᏑMutex).Lock();
        ᒐd1 = true;
        c.n++;
        return c.n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡc.of(Counter.ᏑMutex).Unlock(); ᒐ.Run(); }
}

} partial class lib_package {

public static @string Tag() { return sync.Name(); }

} // end lib_package
