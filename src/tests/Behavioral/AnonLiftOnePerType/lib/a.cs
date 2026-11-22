namespace go.AnonLiftOnePerType;

partial class lib_package {

partial interface Option {
    nint A();
}

partial interface Other {
    nint B();
}

partial struct both {
    internal nint n;
}

internal static nint A⁔(/*this*/ ref both b) { return b.n; }
internal static nint B⁔(/*this*/ ref both b) { return b.n * 2; }

public partial interface With_R0 /*dyn*/ :
    Option,
    Other
{
}

public static With_R0 With(nint n) {
    return new bothжWith_R0(Ꮡ(new both(n)));
}


partial struct Holder {
    public With_R0 V;
}

public static Holder Mk(nint n) { return new Holder(V: With(n)); }

public partial struct Pair_R0 /*dyn*/ {
    public nint X, Y;
}

public static Pair_R0 Pair(nint n) { return new Pair_R0(n, n + 1); }


partial struct Box {
    public Pair_R0 P;
}

public static Box MkBox(nint n) { return new Box(P: Pair(n)); }

} // end lib_package
