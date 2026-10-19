namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Inner /*map[@string, nint]*/;

partial struct Outer /*map[@string, Inner]*/;

partial struct OuterU /*map[@string, map<@string, nint>]*/;

partial struct Tree /*map[@string, Tree]*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedOfNamedˢ = (@string)"named of named:"u8;

internal static void namedOfNamed() {
    var d = new Outer(new map<@string, Inner>{["e"] = new Inner(new map<@string, nint>{})});
    d["e"].Set("f", 1);
    d["e"].Set("g", 2);
    fmt.Println(namedOfNamedˢ, d["e"]["f"], d["e"]["g"], len(d["e"]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object unnamedOfNamedˢ = (@string)"unnamed of named:"u8;

internal static void unnamedOfNamed() {
    var n = new map<@string, Inner>{["e"] = new map<@string, nint>{}};
    n["e"].Set("f", 3);
    fmt.Println(unnamedOfNamedˢ, n["e"]["f"], len(n["e"]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedOfUnnamedˢ = (@string)"named of unnamed:"u8;

internal static void namedOfUnnamed() {
    var u = new OuterU(new map<@string, map<@string, nint>>{["e"] = new map<@string, nint>{}});
    u["e"].Set("f", 4);
    fmt.Println(namedOfUnnamedˢ, u["e"]["f"], len(u["e"]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object holderˢ = (@string)"holder:"u8;

internal static void holder() {
    var t = new Tree(new map<@string, Tree>{["a"] = new Tree(new map<@string, Tree>{})});
    t["a"].Set("b", new Tree(new map<@string, Tree>{}));
    t["a"]["b"].Set("c", default!);
    var (_, hasC) = t["a"]["b"]["c", ꟷ];
    fmt.Println(holderˢ, len(t), len(t["a"]), len(t["a"]["b"]), hasC, t["a"]["b"]["c"] == default!);
}

internal static void Main() {
    namedOfNamed();
    unnamedOfNamed();
    namedOfUnnamed();
    holder();
}

} // end main_package
