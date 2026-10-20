namespace go;

using fmt = fmt_package;
using lib = global::go.go.xpkgmu_package;
using global::go.go;

partial class main_package {

internal static void Main() {
    lib.ᏑBoxed.RLock();
    lib.ᏑBoxed.RUnlock();
    fmt.Println("boxed: rlock/runlock ok");

    lib.ᏑPlain.RLock();
    lib.ᏑPlain.RUnlock();
    fmt.Println("plain: rlock/runlock ok");

    lib.ᏑBoxed.Lock();
    lib.ᏑBoxed.Unlock();
    lib.ᏑPlain.Lock();
    lib.ᏑPlain.Unlock();
    fmt.Println("lock/unlock ok");

    lib.Cnt.Inc();
    lib.Cnt.Inc();
    fmt.Println("counter:", lib.Cnt.Value());

    lib.Touch();
    lib.ᏑBoxed.RLock();
    lib.ᏑBoxed.RUnlock();
    fmt.Println("after touch ok");
}

} // end main_package
