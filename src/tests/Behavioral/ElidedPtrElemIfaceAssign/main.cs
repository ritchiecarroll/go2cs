namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Item {
    public @string Name;
    public nint N;
}

partial interface Describer {
    @string Describe();
}

partial struct Reg /*map[@string, ж<Item>]*/;

public static @string Describe(this Reg r) {
    @string @out = ""u8;
    foreach (var (_, k) in new @string[]{"a"u8, "b"u8}.slice()) {
        {
            var (it, ok) = r[k, ꟷ]; if (ok) {
                @out += fmt.Sprintf("%s=%s/%d "u8, k, (~it).Name, (~it).N);
            }
        }
    }
    return @out;
}

partial struct Bag /*map[@string, Describer]*/;

public static @string Describe(this Bag b) {
    @string @out = ""u8;
    foreach (var (_, k) in new @string[]{"r"u8, "s"u8}.slice()) {
        {
            var (d, ok) = b[k, ꟷ]; if (ok) {
                @out += k + "{"u8 + d.Describe() + "} "u8;
            }
        }
    }
    return @out;
}

partial struct List /*[]ж<Item>*/;

public static @string Describe(this List l) {
    @string @out = ""u8;
    foreach (var (_, it) in l) {
        @out += fmt.Sprintf("%s/%d "u8, (~it).Name, (~it).N);
    }
    return @out;
}

internal static @string take(Describer d) {
    return d.Describe();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object declˢ = (@string)"decl:"u8;
private static readonly object assignˢ = (@string)"assign:"u8;
private static readonly object concreteˢ = (@string)"concrete:"u8;
private static readonly object mutatedˢ = (@string)"mutated:"u8;
private static readonly object argˢ = (@string)"arg:"u8;
private static readonly object sliceˢ = (@string)"slice:"u8;
private static readonly object bagdeclˢ = (@string)"bagdecl:"u8;
private static readonly object bagassignˢ = (@string)"bagassign:"u8;

internal static void Main() {
    Describer d = new Reg(new map<@string, ж<Item>>{["a"u8] = Ꮡ(new Item(Name: "alpha"u8, N: 1))});
    fmt.Println(declˢ, d.Describe());
    d = new Reg(new map<@string, ж<Item>>{
        ["a"u8] = Ꮡ(new Item(Name: "beta"u8, N: 2)),
        ["b"u8] = Ꮡ(new Item(Name: "gamma"u8, N: 3))
    });
    fmt.Println(assignˢ, d.Describe());
    Reg c = default!;
    c = new Reg(new map<@string, ж<Item>>{["a"u8] = Ꮡ(new Item(Name: "delta"u8, N: 4))});
    fmt.Println(concreteˢ, c.Describe());
    c["a"u8].Value.N = 40;
    fmt.Println(mutatedˢ, c.Describe());
    fmt.Println(argˢ, take(new Reg(new map<@string, ж<Item>>{["b"u8] = Ꮡ(new Item(Name: "epsilon"u8, N: 5))})));
    d = new List(new ж<Item>[]{Ꮡ(new Item(Name: "zeta"u8, N: 6)), Ꮡ(new Item(Name: "eta"u8, N: 7))}.slice());
    fmt.Println(sliceˢ, d.Describe());
    Describer b = new Bag(new map<@string, Describer>{["r"u8] = new Reg(new map<@string, ж<Item>>{["a"u8] = Ꮡ(new Item(Name: "theta"u8, N: 8))})});
    fmt.Println(bagdeclˢ, b.Describe());
    b = new Bag(new map<@string, Describer>{["r"u8] = new Reg(new map<@string, ж<Item>>{["a"u8] = Ꮡ(new Item(Name: "iota"u8, N: 9))}), ["s"u8] = new List(new ж<Item>[]{Ꮡ(new Item(Name: "kappa"u8, N: 10))}.slice())
    });
    fmt.Println(bagassignˢ, b.Describe());
}

} // end main_package
