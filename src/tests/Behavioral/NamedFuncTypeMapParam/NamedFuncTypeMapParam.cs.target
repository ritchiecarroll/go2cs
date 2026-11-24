namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Node {
    internal @string name;
}

public delegate (ж<Node> pkg, error err) Importer(map<@string, ж<Node>> imports, @string path);

internal static ж<Node> newPackage(Importer importer, map<@string, ж<Node>> files) {
    var (n, _) = importer(files, "root");
    return n;
}

internal static (ж<Node> pkg, error err) lookup(map<@string, ж<Node>> imports, @string path) {
    return (imports["a"], default);
}

internal static void main() {
    var files = new map<@string, ж<Node>>{["a"] = Ꮡ(new Node(name: "alpha"))};
    var pkg = newPackage(new Importer(lookup), files);
    fmt.Println(pkg.name);
}

} // end main_package
