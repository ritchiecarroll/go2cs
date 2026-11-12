namespace go;

using fmt = fmt_package;
using shapes = FuncMethodSameNameCollision.shapes_package;
using FuncMethodSameNameCollision;
using static main_package;

partial class main_package {

partial struct Node {
    public nint Depth;
}

} partial class mainᴛNode {

public static bool Deep(this ref Node n) { return n.Depth > 2; }

} partial class main_package {

public static bool Deep(ж<Node> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    return n.Depth > 5;
}

internal static void main() {
    var n = Ꮡ(new Node(Depth: 3));
    fmt.Println("local", n.Deep(), Deep(n));

    var check = Deep;
    fmt.Println("value", check(Ꮡ(new Node(Depth: 9))));

    var t = Ꮡ(new shapes.Type(Flags: 3));
    fmt.Println("foreign", t.IsTupleType(), shapes.IsTupleType(t), shapes.IsTupleType(Ꮡ(new shapes.Type())));
    fmt.Println("extra", t.Combine(4), shapes.Combine(t, 4));
    fmt.Println("control", t.Size(), shapes.Size(t, 10));

    var isTuple = shapes.IsTupleType;
    fmt.Println("foreign value", isTuple(t));
}

} // end main_package
