namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct G<T> {
    internal T v;
}

partial interface I {
    @string M();
}

public static @string M<T>(this G<T> g) {
    return fmt.Sprint(g.v);
}

partial struct T /*num:nint*/;

public static @string M(this T t) {
    return fmt.Sprint((nint)t);
}

internal static void Main() {
    I a = new G<nint>(7);
    I b = new G<@string>("x"u8);
    I c = new G<G<nint>>(new G<nint>(9));
    I d = ((T)3);
    fmt.Println(a.M(), b.M(), c.M(), d.M());
    fmt.Println(new G<nint>(7) == new G<nint>(7), new G<nint>(7) == new G<nint>(8));
    fmt.Println(new G<@string>("x"u8) == new G<@string>("x"u8), new G<@string>("x"u8) == new G<@string>("y"u8));
    fmt.Println(AreEqual(a, new G<nint>(7)), AreEqual(a, new G<nint>(8)), AreEqual(b, new G<@string>("x"u8)), AreEqual(d, ((T)3)));
    foreach (var (_, item) in new I[]{a, b, c, d}.slice()) {
        fmt.Print(item.M(), (@string)";"u8);
    }
    fmt.Println();
}

} // end main_package
