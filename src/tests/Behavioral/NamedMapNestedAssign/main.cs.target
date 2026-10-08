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
    var d = new Outer(new map<@string, Inner>{["e"u8] = new Inner(new map<@string, nint>{})});
    d["e"u8].Set("f"u8, 1);
    d["e"u8].Set("g"u8, 2);
    fmt.Println(namedOfNamedˢ, d["e"u8]["f"u8], d["e"u8]["g"u8], len(d["e"u8]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object unnamedOfNamedˢ = (@string)"unnamed of named:"u8;

internal static void unnamedOfNamed() {
    var n = new map<@string, Inner>{["e"u8] = new map<@string, nint>{}};
    n["e"u8].Set("f"u8, 3);
    fmt.Println(unnamedOfNamedˢ, n["e"u8]["f"u8], len(n["e"u8]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedOfUnnamedˢ = (@string)"named of unnamed:"u8;

internal static void namedOfUnnamed() {
    var u = new OuterU(new map<@string, map<@string, nint>>{["e"u8] = new map<@string, nint>{}});
    u["e"u8].Set("f"u8, 4);
    fmt.Println(namedOfUnnamedˢ, u["e"u8]["f"u8], len(u["e"u8]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object holderˢ = (@string)"holder:"u8;

internal static void holder() {
    var t = new Tree(new map<@string, Tree>{["a"u8] = new Tree(new map<@string, Tree>{})});
    t["a"u8].Set("b"u8, new Tree(new map<@string, Tree>{}));
    t["a"u8]["b"u8].Set("c"u8, default!);
    var (_, hasC) = t["a"u8]["b"u8]["c"u8, ꟷ];
    fmt.Println(holderˢ, len(t), len(t["a"u8]), len(t["a"u8]["b"u8]), hasC, t["a"u8]["b"u8]["c"u8] == default!);
}

internal static void Main() {
    namedOfNamed();
    unnamedOfNamed();
    namedOfUnnamed();
    holder();
}

} // end main_package
