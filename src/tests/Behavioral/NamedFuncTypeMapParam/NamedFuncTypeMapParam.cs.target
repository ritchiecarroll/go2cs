namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Node {
    internal @string name;
}

// type Importer is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string rootˢ = "root"u8;

internal static ж<Node> newPackage(Func<map<@string, ж<Node>>, @string, (ж<Node>, error)> importer, map<@string, ж<Node>> files) {
    var (n, _) = importer(files, rootˢ);
    return n;
}

internal static (ж<Node> pkg, error err) lookup(map<@string, ж<Node>> imports, @string path) {
    return (imports["a"u8], default!);
}

internal static void Main() {
    var files = new map<@string, ж<Node>>{["a"u8] = Ꮡ(new Node(name: "alpha"u8))};
    var pkg = newPackage(new Func<map<@string, ж<Node>>, @string, (ж<Node>, error)>(lookup), files);
    fmt.Println((~pkg).name);
}

} // end main_package
