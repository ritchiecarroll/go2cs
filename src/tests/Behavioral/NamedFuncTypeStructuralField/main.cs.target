namespace go;

using fmt = fmt_package;

partial class main_package {

public partial struct item {
    internal @string name;
    internal nint rank;
}

public delegate bool By(ж<item> a, ж<item> b);

public static void Sort(this By by, slice<item> items) {
    var s = Ꮡ(new sorter(items: items, by: new Func<ж<item>, ж<item>, bool>(by)));
    s.run();
}

partial struct sorter {
    internal slice<item> items;
    internal Func<ж<item>, ж<item>, bool> by;
}

internal static void run(this ref sorter s) {
    nint n = len(s.items);
    for (nint i = 0; i < n; i++) {
        for (nint j = i + 1; j < n; j++) {
            if (s.by(Ꮡ(s.items, j), Ꮡ(s.items, i))) {
                (s.items[i], s.items[j]) = (s.items[j], s.items[i]);
            }
        }
    }
}

internal static bool byName(ж<item> Ꮡa, ж<item> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    return a.name < b.name;
}

partial struct comparer {
    internal Func<ж<item>, ж<item>, bool> cmp;
}

internal static void Main() {
    var items = new item[]{new("delta"u8, 4), new("alpha"u8, 1), new("charlie"u8, 3), new("bravo"u8, 2)}.slice();
    new By((ж<item> a, ж<item> b) => (~a).rank < (~b).rank).Sort(items);
    foreach (var (_, it) in items) {
        fmt.Println(it.name, it.rank);
    }
    var c = new comparer(cmp: byName);
    fmt.Println(c.cmp(Ꮡ(items, 0), Ꮡ(items, 1)));
}

} // end main_package
