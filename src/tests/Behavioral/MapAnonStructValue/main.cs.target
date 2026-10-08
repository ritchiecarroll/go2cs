namespace go;

using fmt = fmt_package;

partial class main_package {


partial struct kindsᴛ1 /*dyn*/ {
    internal @string name;
    internal nint size;
    internal Func<nint, nint> scale;
}
internal static map<uint16, kindsᴛ1> kinds = new map<uint16, kindsᴛ1>{
    [0x0001] = new(name: "alpha"u8, size: 16, scale: (nint n) => n * 2),
    [0x0002] = new(name: "beta"u8, size: 32, scale: triple)
};

internal static nint triple(nint n) {
    return n * 3;
}

internal static void Main() {
    var a = kinds[0x0001];
    fmt.Println(a.name, a.size, a.scale(a.size));
    var b = kinds[0x0002];
    fmt.Println(b.name, b.size, b.scale(b.size));
    fmt.Println(len(kinds));
}

} // end main_package
