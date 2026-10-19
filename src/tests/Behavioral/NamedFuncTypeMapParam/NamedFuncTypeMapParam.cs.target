namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Node {
    internal @string name;
}

// type Importer is a methodless func type — rendered inline as its base delegate

internal static ж<Node> newPackage(Func<map<@string, ж<Node>>, @string, (ж<Node>, error)> importer, map<@string, ж<Node>> files) {
    var (n, _) = importer(files, "root");
    return n;
}

internal static (ж<Node> pkg, error err) lookup(map<@string, ж<Node>> imports, @string path) {
    return (imports["a"], default!);
}

internal static void Main() {
    var files = new map<@string, ж<Node>>{["a"] = Ꮡ(new Node(name: "alpha"u8))};
    var pkg = newPackage(new Func<map<@string, ж<Node>>, @string, (ж<Node>, error)>(lookup), files);
    fmt.Println((~pkg).name);
}

} // end main_package
