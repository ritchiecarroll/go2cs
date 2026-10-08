namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct CommonType {
    public int64 ByteSize;
    public @string Label;
}

public static ж<CommonType> Common(this ж<CommonType> Ꮡc) {
    return Ꮡc;
}

public static @string String(this ref CommonType c) {
    return c.Label;
}

partial interface Node :
    fmt.Stringer
{
    ж<CommonType> Common();
}

partial struct Base {
    public partial ref CommonType CommonType { get; }
    public int64 BitSize;
}

public static ж<Base> Basic(this ж<Base> Ꮡb) {
    return Ꮡb;
}

partial struct Uint {
    public partial ref Base Base { get; }
}

partial struct Int {
    public partial ref Base Base { get; }
}

partial struct Ptr {
    public partial ref CommonType CommonType { get; }
    public Node Node;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string uintˢ = "uint"u8;
private static readonly object uintAssertsToAnonBasicˢ = (@string)"uint asserts to anon Basic:"u8;
private static readonly @string uint64ˢ = "uint64"u8;
private static readonly object uintAfterWriteThroughˢ = (@string)"uint after write through promoted pointer:"u8;
private static readonly @string intˢ = "int"u8;
private static readonly object intAfterWriteThroughˢ = (@string)"int after write through promoted pointer:"u8;
private static readonly object typeSwitchMatchedAnonˢ = (@string)"type switch matched anon Basic, bit size:"u8;
private static readonly object typeSwitchMatchedDefaultˢ = (@string)"type switch matched default"u8;
private static readonly object uintValueAssertsToAnonˢ = (@string)"uint VALUE asserts to anon Basic:"u8;
private static readonly @string innerˢ = "inner"u8;
private static readonly @string ptrˢ = "ptr"u8;
private static readonly object ptrCommonThroughEmbedˢ = (@string)"ptr Common through embed:"u8;
private static readonly object ptrStringThroughEmbedˢ = (@string)"ptr String through embed:"u8;
private static readonly @string emptyˢ = "empty"u8;
private static readonly object ptrCommonWithNilFieldˢ = (@string)"ptr Common with nil field:"u8;
private static readonly object ptrAfterWriteThroughˢ = (@string)"ptr after write through Common:"u8;
private static readonly object ptrFieldNodeˢ = (@string)"ptr field node:"u8;

internal partial interface main_type /*dyn*/ {
    ж<Base> Basic();
}

internal static void Main() {
    var u = Ꮡ(new Uint(nil));
    u.Value.Label = uintˢ;
    Node n = new UintжNode(u);
    var (basic, ok) = n._<main_type>(ᐧ);
    fmt.Println(uintAssertsToAnonBasicˢ, ok);
    if (ok) {
        var b = basic.Basic();
        b.Value.BitSize = 64;
        b.Value.Label = uint64ˢ;
    }
    fmt.Println(uintAfterWriteThroughˢ, (~u).BitSize, (~u).Label, n.String());
    var i = Ꮡ(new Int(nil));
    i.Value.Label = intˢ;
    Node ni = new IntжNode(i);
    {
        var (bi, okI) = ni._<main_type>(ᐧ); if (okI) {
            bi.Basic().Value.BitSize = 32;
        }
    }
    fmt.Println(intAfterWriteThroughˢ, (~i).BitSize, (~i).Label);
    switch (n.type()) {
    case {} Δt when Δt._<main_type>(out var t): {
        fmt.Println(typeSwitchMatchedAnonˢ, (~t.Basic()).BitSize);
        break;
    }
    default: {
        var t = n;
        fmt.Println(typeSwitchMatchedDefaultˢ);
        break;
    }}
    any anyValue = new Uint(nil);
    var (_, valueOK) = anyValue._<main_type>(ᐧ);
    fmt.Println(uintValueAssertsToAnonˢ, valueOK);
    var inner = Ꮡ(new Uint(nil));
    inner.Value.Label = innerˢ;
    inner.Value.ByteSize = 8;
    var p = Ꮡ(new Ptr(Node: new UintжNode(inner)));
    p.Value.Label = ptrˢ;
    p.Value.ByteSize = 4;
    Node np = new PtrжNode(p);
    fmt.Println(ptrCommonThroughEmbedˢ, (~np.Common()).Label, (~np.Common()).ByteSize);
    fmt.Println(ptrStringThroughEmbedˢ, np.String());
    var empty = Ꮡ(new Ptr(nil));
    empty.Value.Label = emptyˢ;
    empty.Value.ByteSize = 2;
    Node ne = new PtrжNode(empty);
    fmt.Println(ptrCommonWithNilFieldˢ, (~ne.Common()).Label, (~ne.Common()).ByteSize);
    ne.Common().Value.ByteSize = 16;
    fmt.Println(ptrAfterWriteThroughˢ, (~empty).ByteSize);
    fmt.Println(ptrFieldNodeˢ, (~(~p).Node.Common()).Label, (~p).Node.String());
}

} // end main_package
