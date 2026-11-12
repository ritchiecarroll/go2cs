namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct G<T> {
    internal T v;
}

partial interface I {
    @string M();
}

} partial class mainᴛG {

public static @string M<T>(this G<T> g) { return fmt.Sprint((g.v).OrTypedNilOf()); }

} partial class main_package {

partial struct T /*num:nint*/;

} partial class mainᴛT {

public static @string M(this T t) { return fmt.Sprint((nint)t); }

} partial class main_package {

internal static void main() {
    I a = new G<nint>(7);
    I b = new G<@string>("x");
    I c = new G<G<nint>>(new G<nint>(9));
    I d = ((T)3);
    fmt.Println(a.M(), b.M(), c.M(), d.M());

    fmt.Println(new G<nint>(7) == new G<nint>(7), new G<nint>(7) == new G<nint>(8));
    fmt.Println(new G<@string>("x") == new G<@string>("x"), new G<@string>("x") == new G<@string>("y"));

    fmt.Println(AreEqual(a, new G<nint>(7)), AreEqual(a, new G<nint>(8)), AreEqual(b, new G<@string>("x")), AreEqual(d, ((T)3)));

    foreach (var (_, item) in range(slice<I>([a, b, c, d]))) {
        fmt.Print(item.M(), ";");
    }
    fmt.Println();
}

} // end main_package
