namespace go.LazyPackageVars;

partial class tables_package {

partial struct Entry {
    public @string Name;
    public nint Weight;
}

partial struct Rule {
    public @string Name;
    public Func<nint, nint> Apply;
}

public static slice<@string> Order;

internal static nint record(@string name) {
    Order = append(Order, name);
    return len(Order);
}

internal static nint first = record("first");

[GoLazy] partial class lazyᴛbig { internal static array<Entry> big = new Entry[]{new("a", 1), new("b", 2), new("c", 3), new("d", 4), new("e", 5), new("f", 6), new("g", 7)}.array(); }

[GoLazy(Initial = true)] partial class lazyᴛHeavy { public static slice<Entry> Heavy = big[4..7]; }

[GoLazy] partial class lazyᴛByName { public static map<@string, slice<Entry>> ByName = new map<@string, slice<Entry>>{["heavy"] = lazyᴛHeavy.initial, ["all"] = big[..]}; }

[GoLazy] partial class lazyᴛRoots { public static array<ж<Entry>> Roots = new ж<Entry>[]{Ꮡ(new Entry("r1", 1)), Ꮡ(new Entry("r2", 2)), Ꮡ(new Entry("r3", 3)), Ꮡ(new Entry("r4", 4))}.array(); }

internal static nint second = record("second");

[GoLazy] partial class lazyᴛRules { public static array<Rule> Rules = new Rule[]{
    new("double", (nint n) => n * 2),
    new("square", (nint n) => n * n),
    new("negate", (nint n) => -n)
}.array(); }

[GoInit] internal static void init() {
    record("init");
}

public static (nint, nint) Seen() {
    return (first, second);
}

} // end tables_package
