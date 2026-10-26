namespace go.xpprobe;

using sync = sync_package;

partial class lib_package {

partial struct Counter {
    internal nint n;
}

public static void Inc(this ref Counter c) {
    c.n++;
}

public static nint Value(this Counter c) {
    return c.n;
}

partial struct Holder {
    public sync.WaitGroup WG;
    public nint N;
}

public static nint N;

public static Counter V;

public static Holder S;

public static array<nint> Arr = new(3);

public static ж<sync.RWMutex> ᏑRW = new StandardBox<sync.RWMutex>(default(sync.RWMutex));
public static ref sync.RWMutex RW => ref ᏑRW.Value;

public static slice<nint> Table = new nint[]{1, 2, 3}.slice();

public static nint ReadN() {
    return N;
}

} // end lib_package
