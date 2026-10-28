namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Node {
    nint Pos();
}

partial struct Item {
    internal nint pos;
}

public static nint Pos(this ref Item i) {
    return i.pos;
}

internal static (@string, bool) lookup(map<Node, @string> seen, ж<Item> it) {
    var (value, ok) = seen[new ItemжNode(it), ꟷ];
    return (value, ok);
}

internal static void record(map<Node, @string> seen, ж<Item> it, @string label) {
    seen[new ItemжNode(it)] = label;
}

internal static @string plainRead(map<Node, @string> seen, ж<Item> it) {
    return seen[new ItemжNode(it)];
}

internal static void Main() {
    var item = Ꮡ(new Item(pos: 7));
    var seen = new map<Node, @string>{};

    seen[new ItemжNode(item)] = "kept";
    var (value, ok) = seen[new ItemжNode(item), ꟷ];

    fmt.Println(value, ok, item.Pos());

    (value, ok) = lookup(seen, item);
    fmt.Println(value, ok);

    var other = Ꮡ(new Item(pos: 9));
    record(seen, other, "added");
    fmt.Println(plainRead(seen, other), len(seen));
}

} // end main_package
