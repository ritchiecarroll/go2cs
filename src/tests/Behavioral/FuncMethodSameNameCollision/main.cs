namespace go;

using fmt = fmt_package;
using shapes = FuncMethodSameNameCollision.shapes_package;
using FuncMethodSameNameCollision;

partial class main_package {

partial struct Node {
    public nint Depth;
}

public static bool Deep(this ref Node n) { return n.Depth > 2; }

public static bool ΔDeep(ж<Node> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    return n.Depth > 5;
}

internal static void main() {
    var n = Ꮡ(new Node(Depth: 3));
    fmt.Println("local", n.Deep(), ΔDeep(n));

    Func<ж<Node>, bool> check = ΔDeep;
    fmt.Println("value", check(Ꮡ(new Node(Depth: 9))));

    var t = Ꮡ(new shapes.Type(Flags: 3));
    fmt.Println("foreign", t.IsTupleType(), shapes.ΔIsTupleType(t), shapes.ΔIsTupleType(Ꮡ(new shapes.Type())));
    fmt.Println("extra", t.Combine(4), shapes.ΔCombine(t, 4));
    fmt.Println("control", t.Size(), shapes.Size(t, 10));

    Func<ж<shapes.Type>, bool> isTuple = shapes.ΔIsTupleType;
    fmt.Println("foreign value", isTuple(t));
}

} // end main_package
