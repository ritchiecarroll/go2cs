namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

internal static ж<atomic.Int32> Ꮡlocked = new StaticBox<atomic.Int32>(default(atomic.Int32));
internal static ref atomic.Int32 locked => ref Ꮡlocked.Value;
internal static ж<atomic.Uint64> ᏑrunGoid = new StaticBox<atomic.Uint64>(default(atomic.Uint64));
internal static ref atomic.Uint64 runGoid => ref ᏑrunGoid.Value;

internal static int32 lockUnlock(uint64 id) {
    GoFrame ᒐ = default;
    try {
        while (!Ꮡlocked.CompareAndSwap(0, 1)) {
        }
        defer(Ꮡlocked.Store, (int32)0, ref ᒐ);
        ᏑrunGoid.Store(id);
        defer(ᏑrunGoid.Store, (uint64)0, ref ᒐ);
        return Ꮡlocked.Load();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static int32 localAtomicDefer() {
    GoFrame ᒐ = default;
    try {
        ref var n = ref heap(new atomic.Int32(), out var Ꮡn);
        Ꮡn.Store(41);
        defer(Ꮡn.Store, (int32)0, ref ᒐ);
        return Ꮡn.Load();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    fmt.Println("while locked:", lockUnlock(7));
    fmt.Println("after unlock:", Ꮡlocked.Load());
    fmt.Println("runGoid:", ᏑrunGoid.Load());
    fmt.Println("local during:", localAtomicDefer());
}

} // end main_package
