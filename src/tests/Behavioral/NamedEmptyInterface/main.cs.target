global using I = object;

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct point {
    public nint X, Y;
}

internal static void show(I v) {
    fmt.Printf("I: %T %v\n"u8, v, v);
}

internal static void anon(any v) {
    fmt.Printf("interface{}: %T %v\n"u8, v, v);
}

internal static I back(int8 n) {
    return n;
}

internal static I pick(nint which) {
    switch (which) {
    case 0: {
        return (nint)(42);
    }
    case 1: {
        return (@string)"go"u8;
    }
    case 2: {
        return new point(1, 2);
    }}

    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object intˢ = (@string)"int"u8;
private static readonly object pointˢ = (@string)"point"u8;
private static readonly object otherˢ = (@string)"other"u8;

internal static void Main() {
    any a = (int8)(-5);
    any b = (nint)(42);
    any c = (@string)"go"u8;
    I d = new point(3, 4);
    I e = default!;
    fmt.Printf("%T %v | %T %v | %T %v | %T %v | %T %v\n"u8, a, a, b, b, c, c, d, d, e, e);
    show((int8)7);
    show((nint)(9));
    show((@string)"s"u8);
    show(new point(5, 6));
    show(default!);
    anon((int8)7);
    anon((nint)(9));
    anon((@string)"s"u8);
    anon(new point(5, 6));
    anon(default!);
    fmt.Printf("%T %v\n"u8, back((int8)(-1)), back((int8)(-1)));
    for (nint i = 0; i < 4; i++) {
        show(pick(i));
    }
    e = a;
    fmt.Println(AreEqual(e, a), AreEqual(e, b), e == default!);
    {
        var (n, ok) = b._<nint>(ᐧ); if (ok) {
            fmt.Println(intˢ, n + 1);
        }
    }
    switch (d.type()) {
    case point v: {
        fmt.Println(pointˢ, v.X + v.Y);
        break;
    }
    default: {
        var v = d;
        fmt.Println(otherˢ);
        break;
    }}
    var list = new I[]{(int8)1, (nint)(2), (@string)"three"u8, new point(4, 4), default!}.slice();
    fmt.Println(len(list), list);
    var m = new map<@string, I>{["n"u8] = (nint)(1), ["s"u8] = (@string)"one"u8};
    fmt.Println(m["n"u8], m["s"u8]);
}

} // end main_package
