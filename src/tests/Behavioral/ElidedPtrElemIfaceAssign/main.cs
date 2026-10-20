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
    @string @out = "";
    foreach (var (_, k) in new @string[]{"a", "b"}.slice()) {
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
    @string @out = "";
    foreach (var (_, k) in new @string[]{"r", "s"}.slice()) {
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
    @string @out = "";
    foreach (var (_, it) in l) {
        @out += fmt.Sprintf("%s/%d "u8, (~it).Name, (~it).N);
    }
    return @out;
}

internal static @string take(Describer d) {
    return d.Describe();
}

internal static void Main() {
    Describer d = new Reg(new map<@string, ж<Item>>{["a"] = Ꮡ(new Item(Name: "alpha"u8, N: 1))});
    fmt.Println("decl:", d.Describe());

    d = new Reg(new map<@string, ж<Item>>{
        ["a"] = Ꮡ(new Item(Name: "beta"u8, N: 2)),
        ["b"] = Ꮡ(new Item(Name: "gamma"u8, N: 3))
    });
    fmt.Println("assign:", d.Describe());

    Reg c = default!;
    c = new Reg(new map<@string, ж<Item>>{["a"] = Ꮡ(new Item(Name: "delta"u8, N: 4))});
    fmt.Println("concrete:", c.Describe());

    c["a"].Value.N = 40;
    fmt.Println("mutated:", c.Describe());

    fmt.Println("arg:", take(new Reg(new map<@string, ж<Item>>{["b"] = Ꮡ(new Item(Name: "epsilon"u8, N: 5))})));

    d = new List(new ж<Item>[]{Ꮡ(new Item(Name: "zeta"u8, N: 6)), Ꮡ(new Item(Name: "eta"u8, N: 7))}.slice());
    fmt.Println("slice:", d.Describe());

    Describer b = new Bag(new map<@string, Describer>{["r"] = new Reg(new map<@string, ж<Item>>{["a"] = Ꮡ(new Item(Name: "theta"u8, N: 8))})});
    fmt.Println("bagdecl:", b.Describe());
    b = new Bag(new map<@string, Describer>{["r"] = new Reg(new map<@string, ж<Item>>{["a"] = Ꮡ(new Item(Name: "iota"u8, N: 9))}), ["s"] = new List(new ж<Item>[]{Ꮡ(new Item(Name: "kappa"u8, N: 10))}.slice())
    });
    fmt.Println("bagassign:", b.Describe());
}

} // end main_package
