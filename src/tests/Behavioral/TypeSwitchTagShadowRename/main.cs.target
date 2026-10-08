namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal any inner;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nodeIntˢ = "node:int"u8;
private static readonly @string nodeStringˢ = "node:string"u8;
private static readonly @string nodeOtherˢ = "node:other"u8;
private static readonly @string plainˢ = "plain"u8;

internal static @string inspect(any n) {
    switch (n.type()) {
    case ж<node> nΔ1: {
        switch ((~nΔ1).inner.type()) {
        case nint: {
            return nodeIntˢ;
        }
        case int32: {
            return nodeIntˢ;
        }
        case @string: {
            return nodeStringˢ;
        }
        default: {
            return nodeOtherˢ;
        }}

        break;
    }
    default: {
        var nΔ1 = n;
        _ = nΔ1;
        return plainˢ;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string shadowˢ = "shadow"u8;

internal static (@string, @string) classifyVia(any v) {
    any unwrap(any vΔ1) {
        {
            var (p, ok) = vΔ1._<ж<node>>(ᐧ); if (ok) {
                return (~p).inner;
            }
        }
        return vΔ1;
    }
    var unwrapʗ1 = unwrap;
    @string pick(any vΔ2) {
        var switchᴛ1 = unwrapʗ1(vΔ2);
        switch (switchᴛ1.type()) {
        case nint vΔ3: {
            return fmt.Sprintf("int:%d"u8, vΔ3);
        }
        case int32 vΔ3: {
            return fmt.Sprintf("int:%d"u8, vΔ3);
        }
        case @string vΔ3: {
            return fmt.Sprintf("string:%s"u8, vΔ3);
        }
        default: {
            var vΔ3 = switchᴛ1;
            return fmt.Sprintf("other:%v"u8, vΔ3);
        }}
    }
    @string label = shadowˢ;
    var other = Ꮡ(new node(nil));
    other.Value.inner = label;
    return (pick(v), pick(other.OrTypedNil()));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object directˢ = (@string)"direct"u8;

internal static void Main() {
    @string hi = "hi"u8;
    var sn = Ꮡ(new node(nil));
    sn.Value.inner = hi;
    fmt.Println(inspect(Ꮡ(new node(inner: (nint)(5)))));
    fmt.Println(inspect(sn.OrTypedNil()));
    fmt.Println(inspect(Ꮡ(new node(inner: 1.5D))));
    fmt.Println(inspect((nint)(42)));
    var (a, b) = classifyVia(Ꮡ(new node(inner: (nint)(7))));
    fmt.Println(a, b);
    var (c, d) = classifyVia(directˢ);
    fmt.Println(c, d);
}

} // end main_package
