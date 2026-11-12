namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial interface Tree<N> {
    @string Name();
    slice<N> Children();
}

partial struct Node {
    internal @string name;
    internal slice<ж<Node>> kids;
}

} partial class mainᴛNode {

public static @string Name(this ref Node n) { return n.name; }
public static slice<ж<Node>> Children(this ref Node n) { return n.kids; }

} partial class main_package {

public static void Walk<N>(N n)
    where N : Tree<N>
{
    fmt.Println("visit", n.Name());
    foreach (var (_, kid) in range(n.Children())) {
        Walk(kid);
    }
}

internal static void main() {
    var root = Ꮡ(new Node(name: "root", kids: [Ꮡ(new Node(name: "a")), Ꮡ(new Node(name: "b"))]));
    fmt.Println("before the walk");
    Walk<NodeжTree>(root);
    fmt.Println("after the walk");
}

} // end main_package
