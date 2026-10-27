namespace go.go;

using sync = sync_package;

partial class xpkgmu_package {

public static readonly ж<sync.RWMutex> ᏑBoxed = new StaticBox<sync.RWMutex>(default(sync.RWMutex));
public static ref sync.RWMutex Boxed => ref ((StaticBox<sync.RWMutex>)ᏑBoxed).ValueSlot;

public static readonly ж<sync.RWMutex> ᏑPlain = new StaticBox<sync.RWMutex>(default(sync.RWMutex));
public static ref sync.RWMutex Plain => ref ((StaticBox<sync.RWMutex>)ᏑPlain).ValueSlot;

partial struct Counter {
    internal nint n;
}

public static void Inc(this ref Counter c) {
    c.n++;
}

public static nint Value(this Counter c) {
    return c.n;
}

public static readonly ж<Counter> ᏑCnt = new StaticBox<Counter>(default(Counter));
public static ref Counter Cnt => ref ((StaticBox<Counter>)ᏑCnt).ValueSlot;

public static void Touch() {
    ᏑBoxed.Lock();
    ᏑBoxed.Unlock();
}

} // end xpkgmu_package
