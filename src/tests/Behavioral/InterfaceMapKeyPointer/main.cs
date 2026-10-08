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

internal static (@string, bool) lookup(map<Node, @string> seen, ж<Item> Ꮡit) {
    var (value, ok) = seen[new ItemжNode(Ꮡit), ꟷ];
    return (value, ok);
}

internal static void record(map<Node, @string> seen, ж<Item> Ꮡit, @string label) {
    seen[new ItemжNode(Ꮡit)] = label;
}

internal static @string plainRead(map<Node, @string> seen, ж<Item> Ꮡit) {
    return seen[new ItemжNode(Ꮡit)];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string keptˢ = "kept"u8;
private static readonly @string addedˢ = "added"u8;

internal static void Main() {
    var item = Ꮡ(new Item(pos: 7));
    var seen = new map<Node, @string>{};
    seen[new ItemжNode(item)] = keptˢ;
    var (value, ok) = seen[new ItemжNode(item), ꟷ];
    fmt.Println(value, ok, item.Pos());
    (value, ok) = lookup(seen, item);
    fmt.Println(value, ok);
    var other = Ꮡ(new Item(pos: 9));
    record(seen, other, addedˢ);
    fmt.Println(plainRead(seen, other), len(seen));
}

} // end main_package
