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
    /*embed*/ public CommonType CommonType;
    public int64 BitSize;
}

public static ж<Base> Basic(this ж<Base> Ꮡb) {
    return Ꮡb;
}

partial struct Uint {
    /*embed*/ public Base Base;
}

partial struct Int {
    /*embed*/ public Base Base;
}

partial struct Ptr {
    /*embed*/ public CommonType CommonType;
    public Node Node;
}

internal partial interface main_type /*dyn*/ {
    ж<Base> Basic();
}

internal static void Main() {
    var u = Ꮡ(new Uint());
    u.Label = "uint";
    Node n = new UintжNode(u);

    var (basic, ok) = n._<main_type>(ᐧ);
    fmt.Println("uint asserts to anon Basic:", ok);

    if (ok) {
        var b = basic.Basic();
        b.BitSize = 64;
        b.Label = "uint64";
    }

    fmt.Println("uint after write through promoted pointer:", u.BitSize, u.Label, n.String());

    var i = Ꮡ(new Int());
    i.Label = "int";
    Node ni = new IntжNode(i);
    {
        var (bi, okI) = ni._<main_type>(ᐧ); if (okI) {
            bi.Basic().BitSize = 32;
        }
    }
    fmt.Println("int after write through promoted pointer:", i.BitSize, i.Label);

    switch (n.type()) {
    case {} Δt when Δt._<main_type>(out var t): {
        fmt.Println("type switch matched anon Basic, bit size:", t.Basic().BitSize);
        break;
    }
    default: {
        var t = n;
        fmt.Println("type switch matched default");
        break;
    }}

    any anyValue = new Uint();
    var (_, valueOK) = anyValue._<main_type>(ᐧ);
    fmt.Println("uint VALUE asserts to anon Basic:", valueOK);

    var inner = Ꮡ(new Uint());
    inner.Label = "inner";
    inner.ByteSize = 8;

    var p = Ꮡ(new Ptr(Node: new UintжNode(inner)));
    p.Label = "ptr";
    p.ByteSize = 4;

    Node np = new PtrжNode(p);
    fmt.Println("ptr Common through embed:", np.Common().Label, np.Common().ByteSize);
    fmt.Println("ptr String through embed:", np.String());

    var empty = Ꮡ(new Ptr());
    empty.Label = "empty";
    empty.ByteSize = 2;
    Node ne = new PtrжNode(empty);
    fmt.Println("ptr Common with nil field:", ne.Common().Label, ne.Common().ByteSize);

    ne.Common().ByteSize = 16;
    fmt.Println("ptr after write through Common:", empty.ByteSize);

    fmt.Println("ptr field node:", p.Node.Common().Label, p.Node.String());
}

} // end main_package
