namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void addEdge(map<@string, map<@string, bool>> g, @string from, @string to) {
    if (g[from] == nil) {
        g[from] = new map<@string, bool>{};
    }
    g[from].Set(to, true);
}

internal static void Main() {
    var g = new map<@string, map<@string, bool>>{};
    addEdge(g, "a", "b");
    addEdge(g, "a", "c");
    addEdge(g, "b", "c");
    fmt.Println(g["a"]["b"], g["a"]["c"], g["b"]["c"], g["a"]["z"]);
    fmt.Println(len(g["a"]));
}

} // end main_package
