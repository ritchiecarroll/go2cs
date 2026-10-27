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

public static readonly ж<slice<@string>> ᏑOrder = new StaticBox<slice<@string>>(default(slice<@string>));
public static ref slice<@string> Order => ref ((StaticBox<slice<@string>>)ᏑOrder).ValueSlot;

internal static nint record(@string name) {
    Order = append(Order, name);
    return len(Order);
}

internal static nint first = record("first");

[GoLazy] partial class lazyᴛbig { internal static array<Entry> big = new Entry[]{new("a", 1), new("b", 2), new("c", 3), new("d", 4), new("e", 5), new("f", 6), new("g", 7)}.array(); }

[GoLazy] partial class lazyᴛHeavy { public static readonly ж<slice<Entry>> ᏑHeavy = new StaticBox<slice<Entry>>(big[4..7]); }
public static ref slice<Entry> Heavy => ref ((StaticBox<slice<Entry>>)ᏑHeavy).ValueSlot;

public static readonly ж<map<@string, slice<Entry>>> ᏑByName = new StaticBox<map<@string, slice<Entry>>>(new map<@string, slice<Entry>>{["heavy"] = Heavy, ["all"] = big[..]});
public static ref map<@string, slice<Entry>> ByName => ref ((StaticBox<map<@string, slice<Entry>>>)ᏑByName).ValueSlot;

[GoLazy] partial class lazyᴛRoots { public static readonly ж<array<ж<Entry>>> ᏑRoots = new StaticBox<array<ж<Entry>>>(new ж<Entry>[]{Ꮡ(new Entry("r1", 1)), Ꮡ(new Entry("r2", 2)), Ꮡ(new Entry("r3", 3)), Ꮡ(new Entry("r4", 4))}.array()); }
public static ref array<ж<Entry>> Roots => ref ((StaticBox<array<ж<Entry>>>)ᏑRoots).ValueSlot;

internal static nint second = record("second");

[GoLazy] partial class lazyᴛRules { public static readonly ж<array<Rule>> ᏑRules = new StaticBox<array<Rule>>(new Rule[]{
    new("double", (nint n) => n * 2),
    new("square", (nint n) => n * n),
    new("negate", (nint n) => -n)
}.array()); }
public static ref array<Rule> Rules => ref ((StaticBox<array<Rule>>)ᏑRules).ValueSlot;

[GoLazy] partial class lazyᴛGrid { public static readonly ж<array<nint>> ᏑGrid = new StaticBox<array<nint>>(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20}.array()); }
public static ref array<nint> Grid => ref ((StaticBox<array<nint>>)ᏑGrid).ValueSlot;

public static ж<array<nint>> GridAddr() {
    return ᏑGrid;
}

[GoInit] internal static void init() {
    record("init");
}

public static (nint, nint) Seen() {
    return (first, second);
}

} // end tables_package
