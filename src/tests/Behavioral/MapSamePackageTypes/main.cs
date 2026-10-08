namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct OSArch {
    internal @string os, arch;
}

partial struct info {
    internal bool supported;
}

internal static map<OSArch, info> table = new map<OSArch, info>{
    [new("linux"u8, "amd64"u8)] = new(supported: true),
    [new("windows"u8, "386"u8)] = new(supported: false)
};

partial struct Description {
    public @string Name;
    public @string ΔDescription;
}

internal static slice<Description> allDesc = new Description[]{
    new(Name: "a"u8, ΔDescription: "first"u8),
    new(Name: "b"u8, ΔDescription: "second"u8)
}.slice();

internal static Description describe() {
    return new Description(Name: "c"u8, ΔDescription: "third"u8);
}

internal static void Main() {
    fmt.Println(table[new OSArch("linux"u8, "amd64"u8)].supported);
    fmt.Println(table[new OSArch("windows"u8, "386"u8)].supported);
    fmt.Println(len(table));
    foreach (var (_, d) in allDesc) {
        fmt.Println(d.Name, d.ΔDescription);
    }
    var c = describe();
    fmt.Println(c.Name, c.ΔDescription);
}

} // end main_package
