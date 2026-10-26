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

public static ж<nint> ᏑN = new StandardBox<nint>(default(nint));
public static ref nint N => ref ᏑN.Value;

public static ж<Counter> ᏑV = new StandardBox<Counter>(default(Counter));
public static ref Counter V => ref ᏑV.Value;

public static ж<Holder> ᏑS = new StandardBox<Holder>(default(Holder));
public static ref Holder S => ref ᏑS.Value;

public static ж<array<nint>> ᏑArr = new StandardBox<array<nint>>(new array<nint>(3));
public static ref array<nint> Arr => ref ᏑArr.Value;

public static ж<sync.RWMutex> ᏑRW = new StandardBox<sync.RWMutex>(default(sync.RWMutex));
public static ref sync.RWMutex RW => ref ᏑRW.Value;

public static ж<slice<nint>> ᏑTable = new StandardBox<slice<nint>>(new nint[]{1, 2, 3}.slice());
public static ref slice<nint> Table => ref ᏑTable.ValueSlot;

public static nint ReadN() {
    return N;
}

} // end lib_package
