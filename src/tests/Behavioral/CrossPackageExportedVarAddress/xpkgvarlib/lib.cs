namespace go.go;

using sync = sync_package;

partial class xpkgvar_package {

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

public static readonly ж<nint> ᏑN = new StaticBox<nint>(default(nint));
public static ref nint N => ref ((StaticBox<nint>)ᏑN).ValueSlot;

public static readonly ж<Counter> ᏑV = new StaticBox<Counter>(default(Counter));
public static ref Counter V => ref ((StaticBox<Counter>)ᏑV).ValueSlot;

public static readonly ж<Holder> ᏑS = new StaticBox<Holder>(default(Holder));
public static ref Holder S => ref ((StaticBox<Holder>)ᏑS).ValueSlot;

public static readonly ж<array<nint>> ᏑArr = new StaticBox<array<nint>>(new array<nint>(3));
public static ref array<nint> Arr => ref ((StaticBox<array<nint>>)ᏑArr).ValueSlot;

public static readonly ж<bool> ᏑFlag = new StaticBox<bool>(default(bool));
public static ref bool Flag => ref ((StaticBox<bool>)ᏑFlag).ValueSlot;

public static readonly ж<slice<nint>> ᏑTable = new StaticBox<slice<nint>>(new nint[]{1, 2, 3}.slice());
public static ref slice<nint> Table => ref ((StaticBox<slice<nint>>)ᏑTable).ValueSlot;

public static nint ReadN() {
    return N;
}

public static nint ReadS() {
    return S.N;
}

} // end xpkgvar_package
