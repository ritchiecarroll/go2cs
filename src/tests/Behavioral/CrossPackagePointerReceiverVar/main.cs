namespace go;

using fmt = fmt_package;
using lib = global::go.go.xpkgmu_package;
using global::go.go;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object boxedRlockRunlockOkˢ = (@string)"boxed: rlock/runlock ok"u8;
private static readonly object plainRlockRunlockOkˢ = (@string)"plain: rlock/runlock ok"u8;
private static readonly object lockUnlockOkˢ = (@string)"lock/unlock ok"u8;
private static readonly object counterˢ = (@string)"counter:"u8;
private static readonly object afterTouchOkˢ = (@string)"after touch ok"u8;

internal static void Main() {
    lib.ᏑBoxed.RLock();
    lib.ᏑBoxed.RUnlock();
    fmt.Println(boxedRlockRunlockOkˢ);

    lib.ᏑPlain.RLock();
    lib.ᏑPlain.RUnlock();
    fmt.Println(plainRlockRunlockOkˢ);

    lib.ᏑBoxed.Lock();
    lib.ᏑBoxed.Unlock();
    lib.ᏑPlain.Lock();
    lib.ᏑPlain.Unlock();
    fmt.Println(lockUnlockOkˢ);

    lib.Cnt.Inc();
    lib.Cnt.Inc();
    fmt.Println(counterˢ, lib.Cnt.Value());

    lib.Touch();
    lib.ᏑBoxed.RLock();
    lib.ᏑBoxed.RUnlock();
    fmt.Println(afterTouchOkˢ);
}

} // end main_package
