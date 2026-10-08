namespace go.go;

using sync = sync_package;

partial class xpkgmu_package {

public static ж<sync.RWMutex> ᏑBoxed = new StandardBox<sync.RWMutex>(default(sync.RWMutex));
public static ref sync.RWMutex Boxed => ref ᏑBoxed.Value;

public static ж<sync.RWMutex> ᏑPlain = new StandardBox<sync.RWMutex>(default(sync.RWMutex));
public static ref sync.RWMutex Plain => ref ᏑPlain.Value;

partial struct Counter {
    internal nint n;
}

public static void Inc(this ref Counter c) {
    c.n++;
}

public static nint Value(this Counter c) {
    return c.n;
}

public static Counter Cnt;

public static void Touch() {
    ᏑBoxed.Lock();
    ᏑBoxed.Unlock();
}

} // end xpkgmu_package
