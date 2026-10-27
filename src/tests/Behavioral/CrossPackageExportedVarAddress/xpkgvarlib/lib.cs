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

public static nint N;

public static Counter V;

public static Holder S;

public static array<nint> Arr = new(3);

public static bool Flag;

public static slice<nint> Table = new nint[]{1, 2, 3}.slice();

public static nint ReadN() {
    return N;
}

public static nint ReadS() {
    return S.N;
}

} // end xpkgvar_package
