namespace go.AnonLiftOnePerType;

partial class lib_package {


partial struct Holder2 {
    public With_R0 W;
}

public static Holder2 Mk2(nint n) { return new Holder2(W: With(n)); }


partial struct Box2 {
    public Pair_R0 Q;
}

public static Box2 MkBox2(nint n) { return new Box2(Q: Pair(n)); }

} // end lib_package
