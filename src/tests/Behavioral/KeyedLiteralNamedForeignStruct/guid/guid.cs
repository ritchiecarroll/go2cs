namespace go.KeyedLiteralNamedForeignStruct;

using win = go.KeyedLiteralNamedForeignStruct.win_package;
using go.KeyedLiteralNamedForeignStruct;
using static guid_package;

partial class guid_package {

partial struct GUID /*global::go.KeyedLiteralNamedForeignStruct.win_package.GUID*/;

} partial class guidᴛGUID {

public static uint32 First(this GUID g) {
    g = g.ΔClone();

    return g.Data1;
}

} partial class guid_package {

public partial struct inner {
    public uint32 A;
    public array<byte> B = new(2);
}

partial struct Outer /*inner*/;

} partial class guidᴛOuter {

public static uint32 Sum(this Outer o) {
    o = o.ΔClone();

    return o.A + (uint32)o.B[0] + (uint32)o.B[1];
}

} partial class guid_package {

public static (slice<Outer>, slice<ж<Outer>>) Pair() {
    return (slice<Outer>([new(new inner(A: 1)), new(new inner(B: new byte[]{2}.array(2)))]), slice<ж<Outer>>([Ꮡ(new Outer(new inner(A: 3)))]));
}

public static (Outer, slice<Outer>, slice<ж<Outer>>) Zeros() {
    return (new Outer(new inner()), slice<Outer>([new()]), slice<ж<Outer>>([Ꮡ(new Outer())]));
}

} // end guid_package
