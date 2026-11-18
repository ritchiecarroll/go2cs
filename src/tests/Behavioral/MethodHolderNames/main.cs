global using Crate = go.main_package.Box;

namespace go;

using fmt = fmt_package;
using main = main_package;
using static main_package;

partial class main_package {

public static UntypedInt Limit => 7;

public static nint Count = 3;

partial struct Kind /*num:nint*/;

public static nint Scale(nint n) { return n * 10; }

} partial class mainᴛKind {

public static @string String(this Kind k) { return fmt.Sprintf("Kind(%d)"u8, (nint)k); }

} partial class main_package {

partial struct Box {
    internal nint n;
}

} partial class mainᴛBox {

public static nint Limit(this Box b) { return b.n + (nint)main.Limit; }

public static nint Count(this ref Box b) {
    main.Count++;
    return b.n + main.Count;
}

public static Kind Kind(this Box b) { return (Kind)b.n; }

public static nint Scale(this Box b) {
    var f = main.Scale;
    return main.Scale(b.n) + f(1);
}

public static @string Describe(this Box b, any v) {
    switch (v.type()) {
    case Kind k: {
        return "a Kind: "u8 + k.String();
    }
    case Box k: {
        return fmt.Sprint("a Box: ", k.n);
    }}

    var (_, ok) = v._<Kind>(ᐧ); if (ok) {
        return "unreachable";
    }

    return fmt.Sprint("other: ", v, " zero ", (Kind)0);
}

public static @string Class(this Box b, any v) {
    switch (v.type()) {
    case Kind _: {
        return "kind";
    }
    case Box _:
    case ж<Box> _: {
        return "box";
    }}

    return "neither";
}

} partial class main_package {

partial struct Pair<T> {
    internal T a, b;
}

} partial class mainᴛPair {

public static nint Scale<T>(this Pair<T> p) { return main.Scale(2) + (nint)main.Limit; }

public static nint Limit<T>(this ref Pair<T> p) { return main.Limit * 2; }

} partial class mainᴛBox {

public static nint Total(this Crate c) { return c.Limit() + (nint)main.Limit + main.Scale(c.n); }

} partial class main_package {

partial struct Shelf {
    /*embed*/ public Box Box;
    internal nint extra;
}

} partial class mainᴛShelf {

public static nint Limit(this Shelf s) { return s.Box.Limit() + s.extra + (nint)main.Limit; }

} partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object textˢ = (@string)"text"u8;

internal static void main() {
    ref var b = ref heap<Box>(new Box(n: 1), out var Ꮡb);
    fmt.Println("const", b.Limit(), (nint)(Limit));
    fmt.Println("var", b.Count(), Count);
    fmt.Println("type", b.Kind(), (Kind)5);
    fmt.Println("func", b.Scale(), Scale(4));
    fmt.Println(b.Describe((Kind)2));
    fmt.Println(b.Describe(new Box(n: 9)));
    fmt.Println(b.Describe(textˢ));
    fmt.Println("cases", b.Class((Kind)1), b.Class(b), b.Class(Ꮡb), b.Class((nint)1));

    var p = new Pair<@string>("x", "y");
    fmt.Println("generic", p.Scale(), p.Limit(), p.a + p.b);

    fmt.Println("alias", new Crate(n: 2).Total());

    var s = new Shelf(Box: new Box(n: 3), extra: 100);
    fmt.Println("embedded", s.Limit(), s.Scale(), s.Kind(), s.Box.Count());

    var limit = ((Func<Box, nint>)(mainᴛBox.Limit));
    var count = ((Func<ж<Box>, nint>)(mainᴛBox.Count));
    
    var bʗ1 = b;
    var scale = () => bʗ1.Scale();
    fmt.Println("expressions", limit(b), count(Ꮡb), scale(), Scale(1), (nint)(Limit));

    shadows();
    others();
    forms();
}

} // end main_package
