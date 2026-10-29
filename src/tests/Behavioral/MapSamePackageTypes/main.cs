namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct OSArch {
    internal @string os, arch;
}

partial struct info {
    internal bool supported;
}

[GoLazy] partial class lazyᴛtable { internal static map<OSArch, info> table = new map<OSArch, info>{
    [new("linux", "amd64")] = new(supported: true),
    [new("windows", "386")] = new(supported: false)
}; }

partial struct Description {
    public @string Name;
    public @string ΔDescription;
}

internal static slice<Description> allDesc = slice<Description>([
    new(Name: "a", ΔDescription: "first"),
    new(Name: "b", ΔDescription: "second")
]);

internal static Description describe() {
    return new Description(Name: "c", ΔDescription: "third");
}

internal static void Main() {
    fmt.Println(table[new OSArch("linux", "amd64")].supported);
    fmt.Println(table[new OSArch("windows", "386")].supported);
    fmt.Println(len(table));

    foreach (var (_, d) in allDesc) {
        fmt.Println(d.Name, d.ΔDescription);
    }
    var c = describe();
    fmt.Println(c.Name, c.ΔDescription);
}

} // end main_package
