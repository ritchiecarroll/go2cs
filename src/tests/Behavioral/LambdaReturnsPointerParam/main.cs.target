namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct item {
    internal @string name;
}

partial struct entry {
    internal Func<(ж<item>, error)> get;
}

internal static slice<entry> register(slice<entry> table, Func<(ж<item>, error)> get) {
    return append(table, new entry(get: get));
}

internal static slice<entry> addItem(slice<entry> table, ж<item> it) {
    return register(table, () => (it, default));
}

internal static Func<ж<item>> pick(ж<item> it) {
    return () => it;
}

internal static ж<item> passthrough(ж<item> it) {
    return it;
}

internal static void Main() {
    slice<entry> table = default;
    var a = Ꮡ(new item(name: "alpha"));
    var b = Ꮡ(new item(name: "beta"));
    table = addItem(table, a);
    table = addItem(table, b);

    foreach (var (_, e) in table) {
        var (it, err) = e.get();
        fmt.Println((~it).name, err);
    }

    fmt.Println((~pick(a)()).name);
    fmt.Println((~passthrough(b)).name);
}

} // end main_package
